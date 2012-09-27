//===========================================================================
// MODULE:  WindowsService.cs
// PURPOSE: Windows service for the WCF service host
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
using System.ServiceProcess;
// Project References

namespace WcfEx.Host
{
   /// <summary>
   /// WCF host Windows service
   /// </summary>
   /// <remarks>
   /// This class implements the Windows service for the
   /// WCF service host, for unattended and automatic
   /// hosting of installed services.
   /// </remarks>
   [System.ComponentModel.DesignerCategory("Code")]
   internal sealed class WindowsService : ServiceBase
   {
      ServiceHost host = null;

      #region ServiceBase Overrides
      /// <summary>
      /// Service startup override
      /// </summary>
      /// <param name="args">
      /// Service command line parameters
      /// </param>
      protected override void OnStart (String[] args)
      {
         this.host = new ServiceHost();
         if (!this.host.Startup())
            this.Stop();
      }
      /// <summary>
      /// Service stop override
      /// </summary>
      protected override void OnStop ()
      {
         if (this.host != null)
            this.host.Dispose();
         this.host = null;
      }
      /// <summary>
      /// Service shutdown override
      /// </summary>
      protected override void OnShutdown ()
      {
         OnStop();
      }
      #endregion
   }
}
