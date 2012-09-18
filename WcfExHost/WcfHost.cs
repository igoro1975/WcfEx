//===========================================================================
// MODULE:  WcfHost.cs
// PURPOSE: configuration-based WCF service host
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
using System.ServiceModel;
using System.ServiceModel.Configuration;
// Project References

namespace WcfEx.Host
{
   /// <summary>
   /// WCF service host
   /// </summary>
   /// <remarks>
   /// This class extends the default WCF service host to support 
   /// starting a service based on a custom <service/> configuration
   /// element (from an external .config file).
   /// </remarks>
   internal sealed class WcfHost : System.ServiceModel.ServiceHost
   {
      ServiceElement config;

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new host instance
      /// </summary>
      /// <param name="serviceType">
      /// The service class type
      /// </param>
      /// <param name="config">
      /// The configuration element for the service
      /// </param>
      public WcfHost (Type serviceType, ServiceElement config)
      {
         this.config = config;
         base.InitializeDescription(serviceType, new UriSchemeKeyedCollection());
      }
      #endregion

      #region ServiceHost Overrides
      /// <summary>
      /// Initializes the service from the attached
      /// configuration element
      /// </summary>
      protected override void ApplyConfiguration ()
      {
         base.LoadConfigurationSection(this.config);
      }
      #endregion
   }
}
