//===========================================================================
// MODULE:  ServiceHost.cs
// PURPOSE: generic configuration aggregating WCF service host
// 
// Copyright © 2012
// Brent M. Spell. All rights reserved.
//
// This library is free software; you can redistribute it and/or modify it 
// under the terms of the GNU Lesser General Public License as published 
// by the Free Software Foundation; either version 3 of the License, or 
// (at your option) any later version. This library is distributed in the 
// hope that it will be useful, but WITHOUT ANY WARRANTY; without even the 
// implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU Lesser General Public License for more details. You should 
// have received a copy of the GNU Lesser General Public License along with 
// this library; if not, write to 
//    Free Software Foundation, Inc. 
//    51 Franklin Street, Fifth Floor 
//    Boston, MA 02110-1301 USA
//===========================================================================
// System References
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Configuration;
// Project References

namespace WcfEx.Host
{
   /// <summary>
   /// Aggregate service host
   /// </summary>
   /// <remarks>
   /// This class hosts any WCF services present within the current
   /// application directory and configured via .dll.config files. On startup,
   /// it searches for any .dll.config files within the appdomain base directory
   /// and starts up a WCF host instance for each configured service.
   /// Since WCF does not provide a means for identifying service classes via
   /// assembly-qualified names in system.serviceModel/services/service, the 
   /// host assumes that the service classes  exist in one of the assemblies 
   /// within the application directory or its private assembly search path.
   /// </remarks>
   internal sealed class ServiceHost : IDisposable
   {
      TraceSource log = new TraceSource("WcfEx.Host", SourceLevels.All);
      IList<WcfHost> hosts = null;

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new aggregate host instance
      /// </summary>
      public ServiceHost ()
      {
      }
      /// <summary>
      /// Stops all running WCF hosts
      /// </summary>
      public void Dispose ()
      {
         Shutdown();
      }
      #endregion

      #region Service Host Operations
      /// <summary>
      /// Starts up the service host and the WCF hosts
      /// for any configured services
      /// </summary>
      /// <returns>
      /// True if successful
      /// False otherwise
      /// </returns>
      public Boolean Startup ()
      {
         LogInfo("Starting up the service host");
         // retrieve the list of search paths
         // load all assemblies in the search paths
         // load all configured services in all configuration files
         IList<String> searchPaths = LoadSearchPaths();
         IList<Configuration> configFiles = LoadConfigFiles(searchPaths);
         IList<Assembly> assemblies = LoadAssemblies(searchPaths);
         this.hosts = LoadServices(configFiles, assemblies);
         LogInfo("Discovered {0} services to host", this.hosts.Count);
         // start up the list of configured services
         foreach (WcfHost host in this.hosts)
         {
            try
            {
               host.Open();
               LogInfo("Hosting service {0}", host.Description.ServiceType);
            }
            catch (Exception e)
            {
               LogError(e, "Failed to host service {0}", host.Description.ServiceType);
            }
         }
         return this.hosts.Any();
      }
      /// <summary>
      /// Stops all running WCF hosts
      /// </summary>
      public void Shutdown ()
      {
         if (this.hosts != null)
         {
            LogInfo("Shutting down the service host");
            foreach (WcfHost host in hosts)
               try { host.Close(); }
               catch { }
            this.hosts = null;
         }
      }
      #endregion

      #region Service Host Configuration
      /// <summary>
      /// Retrieves the list of config/assembly search
      /// paths for the current appdomain
      /// </summary>
      /// <returns>
      /// The search path list
      /// </returns>
      private IList<String> LoadSearchPaths ()
      {
         List<String> searchPaths = new List<String>();
         searchPaths.Add(AppDomain.CurrentDomain.BaseDirectory);
         if (AppDomain.CurrentDomain.RelativeSearchPath != null)
            searchPaths.AddRange(
               AppDomain.CurrentDomain.RelativeSearchPath
                  .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
            );
         return searchPaths;
      }
      /// <summary>
      /// Retrieves the list of managed assemblies within
      /// the assembly search path for the current appdomain
      /// </summary>
      /// <param name="searchPaths"></param>
      /// <returns></returns>
      private IList<Assembly> LoadAssemblies (IList<String> searchPaths)
      {
         return searchPaths
            .SelectMany(p => Directory.GetFiles(p, "*.dll"))
            .Select(f => LoadAssembly(f))
            .Where(a => a != null)
            .ToList();
      }
      /// <summary>
      /// Loads a managed assembly from a file
      /// </summary>
      /// <param name="path">
      /// The assembly file path
      /// </param>
      /// <returns>
      /// The loaded assembly, if successful
      /// Null otherwise
      /// </returns>
      private Assembly LoadAssembly (String path)
      {
         try
         {
            return Assembly.LoadFile(path);
         }
         catch (BadImageFormatException)
         {
            // ignore native/invalid DLLs
         }
         catch (Exception e)
         {
            LogError(e, "Failed to load application assembly {0}", path);
         }
         return null;
      }
      /// <summary>
      /// Retrieves the list of configurations from the
      /// .dll.config files in the search paths for the current
      /// appdomain
      /// </summary>
      /// <param name="searchPaths">
      /// The list of paths to search
      /// </param>
      /// <returns>
      /// The list of assembly configurations
      /// </returns>
      private IList<Configuration> LoadConfigFiles (IList<String> searchPaths)
      {
         Configuration thisConfig = ConfigurationManager.OpenExeConfiguration(
            Assembly.GetEntryAssembly().Location
         );
         return searchPaths
            .SelectMany(p => Directory.GetFiles(p, "*.dll.config"))
            .Select(c => LoadConfigFile(c))
            .Concat(new[] { thisConfig })
            .Where(c => c != null)
            .ToList();
      }
      /// <summary>
      /// Loads a managed configuration from a file
      /// </summary>
      /// <param name="configFile">
      /// The path to the configuration file
      /// </param>
      /// <returns>
      /// The managed configuration instance
      /// </returns>
      private Configuration LoadConfigFile (String configFile)
      {
         try
         {
            return ConfigurationManager.OpenMappedExeConfiguration(
               new ExeConfigurationFileMap() { ExeConfigFilename = configFile },
               ConfigurationUserLevel.None
            );
         }
         catch (Exception e)
         {
            LogError(e, "Failed to read configuration file {0}", configFile);
         }
         return null;
      }
      /// <summary>
      /// Creates a list of WCF service host instances
      /// for the services within a set of configuration files
      /// </summary>
      /// <param name="configs">
      /// The configuration instances to search
      /// for configured WCF services
      /// </param>
      /// <param name="assemblies">
      /// The list of application assemblies to search 
      /// for WCF service classes
      /// </param>
      /// <returns>
      /// The list of configured service hosts
      /// </returns>
      private IList<WcfHost> LoadServices (
         IList<Configuration> configs, 
         IList<Assembly> assemblies)
      {
         List<WcfHost> hosts = new List<WcfHost>();
         foreach (Configuration config in configs)
         {
            ServiceElementCollection services = ServiceModelSectionGroup
               .GetSectionGroup(config)
               .Services.Services;
            if (services != null)
               hosts.AddRange(
                  services.Cast<ServiceElement>()
                     .Select(s => LoadService(s, assemblies))
                     .Where(s => s != null)
               );
         }
         return hosts;
      }
      /// <summary>
      /// Loads and configures a WCF service instance
      /// </summary>
      /// <param name="service">
      /// The service to load
      /// </param>
      /// <param name="assemblies">
      /// The list of assemblies to search for the
      /// WCF service class
      /// </param>
      /// <returns>
      /// The configured service host, if successful
      /// Null otherwise
      /// </returns>
      private WcfHost LoadService (ServiceElement service, IList<Assembly> assemblies)
      {
         // locate the type of the search assembly list
         Type serviceType = null;
         foreach (Assembly assembly in assemblies)
         {
            serviceType = assembly.GetType(service.Name, false);
            if (serviceType != null)
               break;
         }
         // create the service host for this service
         if (serviceType != null)
            return new WcfHost(serviceType, service);
         LogError(
            new TypeLoadException(
               String.Format(
                  "The service type {0} was not found in the assembly search list\r\n{1}",
                  service.Name,
                  String.Join("\r\n", assemblies)
               )
            ),
            "Failed to initialize a the service instance {0}",
            service.Name
         );
         return null;
      }
      #endregion

      #region Logging
      /// <summary>
      /// Logs an informational message
      /// </summary>
      /// <param name="format">
      /// Message format string
      /// </param>
      /// <param name="args">
      /// Message format parameters
      /// </param>
      private void LogInfo (String format, params Object[] args)
      {
         log.TraceEvent(
            TraceEventType.Information, 
            1, 
            format, 
            args
         );
      }
      /// <summary>
      /// Logs an error message
      /// </summary>
      /// <param name="e">
      /// The error exception
      /// </param>
      /// <param name="format">
      /// Message format string
      /// </param>
      /// <param name="args">
      /// Message format parameters
      /// </param>
      private void LogError(Exception e, String format, params Object[] args)
      {
         log.TraceEvent(
            TraceEventType.Error, 
            2, 
            String.Format("{0}\r\n{1}", String.Format(format, args), e)
         );
      }
      #endregion
   }
}
