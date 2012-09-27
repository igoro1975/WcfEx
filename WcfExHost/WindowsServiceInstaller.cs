//===========================================================================
// MODULE:  WindowsServiceInstaller.cs
// PURPOSE: installer for the WCF host Windows service
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
using System.Collections;
using System.ComponentModel;
using System.ServiceProcess;
// Project References

namespace WcfEx.Host
{
   /// <summary>
   /// WCF host service installer
   /// </summary>
   /// <remarks>
   /// This class configures the installation of the
   /// WCF host service, which can be done via installutil.
   /// </remarks>
   [RunInstaller(true)]
   [System.ComponentModel.DesignerCategory("Code")]
   public sealed class WindowsServiceInstaller : System.Configuration.Install.Installer
   {
      ServiceProcessInstaller processInstaller;
      ServiceInstaller serviceInstaller;

      #region Construction/Disposal
      /// <summary>
      /// Initializes the installer
      /// </summary>
      public WindowsServiceInstaller ()
      {
         Installers.Add(
            this.processInstaller = new ServiceProcessInstaller()
            {
               Account = ServiceAccount.NetworkService
            }
         );
         Installers.Add(
            this.serviceInstaller = new ServiceInstaller()
            {
               ServiceName = "WcfEx.Host",
               Description = "WcfEx Host Service",
               StartType = ServiceStartMode.Automatic,
               DelayedAutoStart = true
            }
         );
      }
      #endregion

      #region Installer Overrides
      /// <summary>
      /// Pre-installation callback
      /// </summary>
      /// <param name="installState">
      /// Installation state values
      /// </param>
      protected override void OnBeforeInstall(IDictionary installState)
      {
         // override the default service name/description with
         // context parameters, to support multiple installations
         // of the service host
         // register the service name with install state, so that
         // the custom service name can be correctly uninstalled
         installState["ServiceName"] = this.serviceInstaller.ServiceName =
            this.Context.Parameters["ServiceName"] ??
            this.serviceInstaller.ServiceName;
         this.serviceInstaller.Description =
            this.Context.Parameters["ServiceDescription"] ??
            this.serviceInstaller.Description;
         base.OnBeforeInstall(installState);
      }
      /// <summary>
      /// Pre-uninstallation callback
      /// </summary>
      /// <param name="installState">
      /// Installation state values
      /// </param>
      protected override void OnBeforeUninstall (IDictionary installState)
      {
         // restore the custom service name from configuration
         this.serviceInstaller.ServiceName = (String)installState["ServiceName"];
         base.OnBeforeUninstall(installState);
      }
      #endregion
   }
}
