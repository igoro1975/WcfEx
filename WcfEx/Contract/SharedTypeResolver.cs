//===========================================================================
// MODULE:  SharedTypeResolver.cs
// PURPOSE: WCF shared managed type data contract type resolver
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
// Project References

namespace WcfEx
{
   /// <summary>
   /// The shared type resolver
   /// </summary>
   /// <remarks>
   /// This resolver supports resolving types based on their fully-qualified
   /// assembly name. It is useful only in cases in which the client and
   /// server are both coupled to the contract type via assembly reference.
   /// </remarks>
   public sealed class SharedTypeResolver : TypeResolver
   {
      #region DataContractResolver Overrides
      /// <summary>
      /// Type resolution override
      /// </summary>
      /// <param name="type">
      /// The managed type to resolve
      /// </param>
      /// <param name="declared">
      /// The declared parameter type
      /// </param>
      /// <param name="ns">
      /// Return the type namespace via here
      /// </param>
      /// <param name="name">
      /// Return the type name via here
      /// </param>
      /// <returns>
      /// True if the type was resolved
      /// False otherwise
      /// </returns>
      protected override Boolean TryResolve (
         Type type,
         Type declared,
         out String ns,
         out String name)
      {
         name = type.FullName;
         ns = type.Assembly.FullName;
         return true;
      }
      /// <summary>
      /// Resolves a type name to a managed type
      /// </summary>
      /// <param name="ns">
      /// The named type's namespace
      /// </param>
      /// <param name="name">
      /// The named type's name
      /// </param>
      /// <param name="declared">
      /// The declared parameter type
      /// </param>
      /// <returns>
      /// The resolved managed type
      /// </returns>
      protected override Type Resolve (
         String ns,
         String name,
         Type declared)
      {
         return Type.GetType(String.Format("{0}, {1}", name, ns));
      }
      #endregion
   }
}
