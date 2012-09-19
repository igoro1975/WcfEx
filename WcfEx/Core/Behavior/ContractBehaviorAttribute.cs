//===========================================================================
// MODULE:  ContractBehaviorAttribute.cs
// PURPOSE: WCF contract behavior attribute
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
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
// Project References

namespace WcfEx
{
   /// <summary>
   /// Contract behavior base attribute
   /// </summary>
   /// <remarks>
   /// This base attribute implements the tedious methods of the 
   /// IContractBehavior and IOperationBehavior interfaces, allowing a
   /// custom behavior attribute to simply override the desired methods.
   /// </remarks>
   [AttributeUsage(
      AttributeTargets.Interface | AttributeTargets.Method,
      AllowMultiple = false,
      Inherited = true)]
   public abstract class ContractBehaviorAttribute :
      Attribute,
      IContractBehavior,
      IOperationBehavior
   {
      #region IContractBehavior Implementation
      /// <summary>
      /// Validates the contract behavior
      /// </summary>
      /// <param name="desc">
      /// The WCF contract description
      /// </param>
      /// <param name="endpoint">
      /// The WCF contract endpoint
      /// </param>
      public virtual void Validate (ContractDescription desc, ServiceEndpoint endpoint)
      {
         Validate();
         foreach (OperationDescription op in desc.Operations)
            Validate(op);
      }
      /// <summary>
      /// Registers custom contract binding parameters
      /// </summary>
      /// <param name="desc">
      /// The WCF contract description
      /// </param>
      /// <param name="endpoint">
      /// The WCF contract endpoint
      /// </param>
      /// <param name="binding">
      /// The WCF endpoint binding parameters
      /// </param>
      public virtual void AddBindingParameters (
         ContractDescription desc,
         ServiceEndpoint endpoint,
         BindingParameterCollection binding)
      {
         foreach (OperationDescription op in desc.Operations)
            AddBindingParameters(op, binding);
      }
      /// <summary>
      /// Applies the current behavior to the client side of a contract
      /// </summary>
      /// <param name="desc">
      /// The WCF contract description
      /// </param>
      /// <param name="endpoint">
      /// The WCF contract endpoint
      /// </param>
      /// <param name="client">
      /// The WCF client runtime
      /// </param>
      public virtual void ApplyClientBehavior (
         ContractDescription desc,
         ServiceEndpoint endpoint,
         ClientRuntime client)
      {
         for (Int32 i = 0; i < desc.Operations.Count; i++)
            ApplyClientBehavior(desc.Operations[i], client.Operations[i]);
      }
      /// <summary>
      /// Applies the current behavior to the service side of a contract
      /// </summary>
      /// <param name="desc">
      /// The WCF contract description
      /// </param>
      /// <param name="endpoint">
      /// The WCF contract endpoint
      /// </param>
      /// <param name="dispatch">
      /// The WCF service runtime
      /// </param>
      public virtual void ApplyDispatchBehavior (
         ContractDescription desc,
         ServiceEndpoint endpoint,
         DispatchRuntime dispatch)
      {
         for (Int32 i = 0; i < desc.Operations.Count; i++)
            ApplyDispatchBehavior(desc.Operations[i], dispatch.Operations[i]);
      }
      #endregion

      #region IOperationBehavior Implementation
      /// <summary>
      /// Validates the operation description
      /// </summary>
      /// <param name="desc">
      /// The WCF operation description
      /// </param>
      public virtual void Validate (OperationDescription desc)
      {
         Validate();
      }
      /// <summary>
      /// Registers custom operation binding parameters
      /// </summary>
      /// <param name="desc">
      /// The WCF operation description
      /// </param>
      /// <param name="binding">
      /// The WCF endpoint binding parameters
      /// </param>
      public virtual void AddBindingParameters (
         OperationDescription desc,
         BindingParameterCollection binding)
      {
      }
      /// <summary>
      /// Applies the current behavior to the client side of a 
      /// contract operation
      /// </summary>
      /// <param name="desc">
      /// The WCF operation description
      /// </param>
      /// <param name="op">
      /// The WCF client operation
      /// </param>
      public virtual void ApplyClientBehavior (
         OperationDescription desc,
         ClientOperation op)
      {
         ApplyOperationBehavior(desc);
      }
      /// <summary>
      /// Applies the current behavior to the service side of a 
      /// contract operation
      /// </summary>
      /// <param name="desc">
      /// The WCF operation description
      /// </param>
      /// <param name="op">
      /// The WCF service operation
      /// </param>
      public virtual void ApplyDispatchBehavior (
         OperationDescription desc,
         DispatchOperation op)
      {
         ApplyOperationBehavior(desc);
      }
      #endregion

      #region Behavior Helper Overrides
      /// <summary>
      /// Validates the type resolver
      /// </summary>
      protected virtual void Validate ()
      {
      }
      /// <summary>
      /// Applies the current behavior to an
      /// operation
      /// </summary>
      /// <param name="desc">
      /// The WCF operation description
      /// </param>
      protected virtual void ApplyOperationBehavior (OperationDescription desc)
      {
      }
      #endregion
   }
}
