//===========================================================================
// MODULE:  TypeResolverAttribute.cs
// PURPOSE: WCF custom type resolver attribute
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
using System.Runtime.Serialization;
using System.ServiceModel.Description;
// Project References

namespace WcfEx
{
   /// <summary>
   /// Type resolver attribute
   /// </summary>
   /// <remarks>
   /// This attribute registers a DataContractResolver derivative as the
   /// resolver for an entire contract interface, or a single contract
   /// operation method.
   /// </remarks>
   [AttributeUsage(
      AttributeTargets.Interface | AttributeTargets.Method, 
      AllowMultiple = false, 
      Inherited = true)]
   public class TypeResolverAttribute : ContractBehaviorAttribute
   {
      private Type type;
      private DataContractResolver resolver = null;

      /// <summary>
      /// Initializes a new attribute instance
      /// </summary>
      /// <param name="type">
      /// The data contract resolver type
      /// </param>
      public TypeResolverAttribute (Type type)
      {
         this.type = type;
      }

      #region ContractBehaviorAttribute Overrides
      /// <summary>
      /// Validates the type resolver
      /// </summary>
      protected override void Validate ()
      {
         if (this.type == null)
            throw new ArgumentException("Type");
         if (this.resolver == null)
            this.resolver = (DataContractResolver)Activator.CreateInstance(
               this.type
            );
      }
      /// <summary>
      /// Applies the type resolver behavior to an
      /// operation
      /// </summary>
      /// <param name="desc">
      /// The WCF operation description
      /// </param>
      protected override void ApplyOperationBehavior (OperationDescription desc)
      {
         DataContractSerializerOperationBehavior dcsob =
            desc.Behaviors.Find<DataContractSerializerOperationBehavior>();
         if (dcsob == null)
            desc.Behaviors.Add(
               dcsob = new DataContractSerializerOperationBehavior(desc)
            );
         dcsob.DataContractResolver = this.resolver;
      }
      #endregion
   }
}
