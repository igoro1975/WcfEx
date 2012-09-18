//===========================================================================
// MODULE:  ServiceBehavior.cs
// PURPOSE: WCF service behavior base class
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
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
// Project References

namespace WcfEx
{
   /// <summary>
   /// Service behavior base class
   /// </summary>
   /// <remarks>
   /// This class provides a simple configuration-based service behavior 
   /// base implementation. The base class serves both as the behavior 
   /// configuration element and the IServiceBehavior implementation. 
   /// Declare custom configuration attributes and override the desired 
   /// IServiceBehavior method for custom functionality.
   /// </remarks>
   /// <example>
   ///   <system.serviceModel>
   ///      <extensions>
   ///         <behaviorExtensions>
   ///            <add name="myBehavior" type="MyApp.MyBehavior,MyAssembly"/>
   ///         </behaviorExtensions>
   ///      </extensions>
   ///      ...
   ///      <behaviors>
   ///         <serviceBehaviors>
   ///            <behavior>
   ///               <myBehavior/>
   ///            </behavior>
   ///         </serviceBehaviors>
   ///      </behaviors>
   ///   </system.serviceModel>
   /// </example>
   public abstract class ServiceBehavior : 
      BehaviorExtensionElement, 
      IServiceBehavior
   {
      #region BehaviorExtensionElement Overrides
      /// <summary>
      /// The behavior runtime type, always
      /// this type for element-based behaviors
      /// </summary>
      public override Type BehaviorType
      {
         get { return GetType(); }
      }
      /// <summary>
      /// Creates a new runtime behavior instance,
      /// always this for element-based behaviors
      /// </summary>
      /// <returns>
      /// The current behavior
      /// </returns>
      protected override Object CreateBehavior ()
      {
         ApplyConfiguration();
         return this;
      }
      #endregion

      #region IServiceBehavior Implementation
      /// <summary>
      /// Binding configuration behavior override
      /// </summary>
      /// <param name="serviceDescription">
      /// The contract description for the service
      /// </param>
      /// <param name="serviceHostBase">
      /// The WCF service host for the service
      /// </param>
      /// <param name="endpoints">
      /// The service endpoints being configured
      /// </param>
      /// <param name="bindingParameters">
      /// The binding parameters to configure
      /// </param>
      public virtual void AddBindingParameters (
         ServiceDescription serviceDescription,
         ServiceHostBase serviceHostBase,
         Collection<ServiceEndpoint> endpoints,
         BindingParameterCollection bindingParameters)
      {
      }
      /// <summary>
      /// Dispatch runtime behavior override
      /// </summary>
      /// <param name="serviceDescription">
      /// The contract description for the service
      /// </param>
      /// <param name="serviceHostBase">
      /// The WCF service host for the service
      /// </param>
      public virtual void ApplyDispatchBehavior (
         ServiceDescription serviceDescription,
         ServiceHostBase serviceHostBase)
      {
      }
      /// <summary>
      /// Service behavior validation override
      /// </summary>
      /// <param name="serviceDescription">
      /// The contract description for the service
      /// </param>
      /// <param name="serviceHostBase">
      /// The WCF service host for the service
      /// </param>
      public virtual void Validate (
         ServiceDescription serviceDescription,
         ServiceHostBase serviceHostBase)
      {
      }
      #endregion

      #region ServiceBehavior Overrides
      /// <summary>
      /// Behavior initialization override
      /// </summary>
      protected virtual void ApplyConfiguration ()
      {
      }
      #endregion
   }
}
