//===========================================================================
// MODULE:  EndpointBehavior.cs
// PURPOSE: WCF endpoint behavior base class
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
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
// Project References

namespace WcfEx
{
   /// <summary>
   /// Endpoint behavior base class
   /// </summary>
   /// <remarks>
   /// This class provides a simple configuration-based endpoint behavior 
   /// base implementation. The base class serves both as the behavior 
   /// configuration element and the IEndpointBehavior implementation. 
   /// Declare custom configuration attributes and override the desired 
   /// IEndpointBehavior method for custom functionality.
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
   ///         <endpointBehaviors>
   ///            <behavior>
   ///               <myBehavior/>
   ///            </behavior>
   ///         </endpointBehaviors>
   ///      </behaviors>
   ///   </system.serviceModel>
   /// </example>
   public abstract class EndpointBehavior : 
      BehaviorExtensionElement, 
      IEndpointBehavior
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

      #region IEndpointBehavior Implementation
      /// <summary>
      /// Binding configuration behavior override
      /// </summary>
      /// <param name="endpoint">
      /// The client/service endpoint being configured
      /// </param>
      /// <param name="bindingParameters">
      /// The binding parameters to configure
      /// </param>
      public virtual void AddBindingParameters (
         ServiceEndpoint endpoint, 
         BindingParameterCollection bindingParameters)
      {
      }
      /// <summary>
      /// Client runtime behavior override
      /// </summary>
      /// <param name="endpoint">
      /// The client endpoint being configured
      /// </param>
      /// <param name="runtime">
      /// The current client runtime instance
      /// </param>
      public virtual void ApplyClientBehavior (
         ServiceEndpoint endpoint, 
         ClientRuntime runtime)
      {
      }
      /// <summary>
      /// Dispatch runtime behavior override
      /// </summary>
      /// <param name="endpoint">
      /// The service endpoint being configured
      /// </param>
      /// <param name="runtime">
      /// The current dispatcher runtime instance
      /// </param>
      public virtual void ApplyDispatchBehavior (
         ServiceEndpoint endpoint, 
         EndpointDispatcher runtime)
      {
      }
      /// <summary>
      /// Endpoint behavior validation override
      /// </summary>
      /// <param name="endpoint">
      /// The client/service endpoint being configured
      /// </param>
      public virtual void Validate (ServiceEndpoint endpoint)
      {
      }
      #endregion

      #region EndpointBehavior Overrides
      /// <summary>
      /// Behavior initialization override
      /// </summary>
      protected virtual void ApplyConfiguration ()
      {
      }
      #endregion
   }
}
