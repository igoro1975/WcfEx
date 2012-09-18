//===========================================================================
// MODULE:  ClientHostBehavior.cs
// PURPOSE: WCF automatic client-hosted service endpoint behavior
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
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
// Project References

namespace WcfEx
{
   /// <summary>
   /// Automatic client host behavior
   /// </summary>
   /// <remarks>
   /// This class configures a client endpoint to automatically
   /// launch the service host for the service, for services hosted
   /// within the client process. This allows a client application to
   /// be completely agnostic of how a service is hosted, supporting
   /// in-process hosting without requiring the client application
   /// to launch a ServiceHost instance.
   /// Services hosted within this behavior remain running until the
   /// current appdomain is unloaded. If control is needed over the service 
   /// lifetime, then the client application should run its own ServiceHost 
   /// for the service.
   /// The behavior locates the configured <service/> instance within
   /// the application configuration file with an endpoint whose
   /// contract name matches the client contract name. It then attempts
   /// to load that service from one of the following sources (in order):
   ///   1) the assembly specified in the serviceAssembly attribute on the 
   ///      service behavior itself (if specified)
   ///   2) the entry assembly for the current process (if any)
   ///   3) the assembly containing the contract type
   ///   4) Type.GetType()
   /// </remarks>
   /// <example>
   ///   <system.serviceModel>
   ///      <extensions>
   ///         <behaviorExtensions>
   ///            <add name="clientHost" type="WcfEx.ClientHostBehavior,WcfEx"/>
   ///         </behaviorExtensions>
   ///      </extensions>
   ///      <services>
   ///         <service name="MyNamespace.MyService">
   ///            <endpoint contract="MyNamespace.IMyService" .../>
   ///         </service>
   ///      </services>
   ///      <client>
   ///         <endpoint 
   ///            contract="MyNamespace.IMyService"
   ///            ...
   ///            behaviorConfiguration="myClientBehavior"/>
   ///      </client>
   ///      <behaviors>
   ///         <endpointBehaviors>
   ///            <behavior name="myClientBehavior">
   ///               <clientHost serviceAssembly="MyServiceAssembly"/>
   ///            </behavior>
   ///         </endpointBehaviors>
   ///      </behaviors>
   ///   </system.serviceModel>
   /// </example>
   public sealed class ClientHostBehavior : EndpointBehavior
   {
      static Dictionary<Type, ServiceHost> hostMap = new Dictionary<Type, ServiceHost>();
      private IList<Assembly> assemblies;

      #region Configuration Properties
      /// <summary>
      /// The assembly containing the service to host
      /// </summary>
      [ConfigurationProperty("serviceAssembly")]
      public String ServiceAssembly
      {
         get { return (String)base["serviceAssembly"]; }
      }
      #endregion

      #region EndpointBehavior Overrides
      /// <summary>
      /// Behavior initialization override
      /// </summary>
      protected override void ApplyConfiguration ()
      {
         if (this.assemblies == null)
         {
            List<Assembly> assemblies = new List<Assembly>();
            if (!String.IsNullOrWhiteSpace(this.ServiceAssembly))
               assemblies.Add(Assembly.Load(this.ServiceAssembly));
            if (Assembly.GetEntryAssembly() != null)
               assemblies.Add(Assembly.GetEntryAssembly());
            this.assemblies = assemblies.AsReadOnly();
         }
      }
      /// <summary>
      /// Client runtime behavior override
      /// </summary>
      /// <param name="clientEndpoint">
      /// The client endpoint being configured
      /// </param>
      /// <param name="clientRuntime">
      /// The current client runtime instance
      /// </param>
      public override void ApplyClientBehavior (
         ServiceEndpoint clientEndpoint, 
         ClientRuntime clientRuntime)
      {
         // search the application configuration file 
         // for a service endpoint contract matching 
         // this client endpoint
         ServicesSection services = ServiceModelSectionGroup
            .GetSectionGroup(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None))
            .Services;
         foreach (ServiceElement service in services.Services)
         {
            foreach (ServiceEndpointElement serviceEndpoint in service.Endpoints)
            {
               if (serviceEndpoint.Contract == clientEndpoint.Contract.ContractType.FullName)
               {
                  // initialize the assembly search list
                  List<Assembly> search = new List<Assembly>(this.assemblies);
                  search.Add(clientRuntime.ContractClientType.Assembly);
                  // attempt to locate the assembly in the search list
                  Type serviceType = null;
                  foreach (Assembly assembly in search)
                     if ((serviceType = assembly.GetType(service.Name, false)) != null)
                        break;
                  // if not found, fall back on Type.GetType()
                  if (serviceType == null)
                     serviceType = Type.GetType(service.Name, false);
                  // if still not found, raise an error
                  if (serviceType == null)
                     throw new TypeLoadException(
                        String.Format(
                           "The service type {0} was not found in the assembly search list\r\n{1}", 
                           service.Name,
                           String.Join("\r\n", search)
                        )
                     );
                  // if no service host is currently running for this
                  // service type, start up a new one and exit
                  lock (hostMap)
                  {
                     if (!hostMap.ContainsKey(serviceType))
                     {
                        ServiceHost host = new ServiceHost(serviceType);
                        host.Open();
                        hostMap[serviceType] = host;
                     }
                  }
                  return;
               }
            }
         }
      }
      #endregion
   }
}
