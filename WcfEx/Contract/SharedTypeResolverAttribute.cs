//===========================================================================
// MODULE:  SharedTypeResolverAttribute.cs
// PURPOSE: WCF shared type resolver attribute
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
   /// Shared type resolver attribute
   /// </summary>
   /// <remarks>
   /// This attribute registers a SharedTypeResolver as the resolver for an 
   /// entire contract interface, or a single contract operation method.
   /// </remarks>
   [AttributeUsage(
      AttributeTargets.Interface | AttributeTargets.Method, 
      AllowMultiple = false, 
      Inherited = true)]
   public sealed class SharedTypeResolverAttribute : TypeResolverAttribute
   {
      /// <summary>
      /// Initializes a new attribute instance
      /// </summary>
      public SharedTypeResolverAttribute () : base(typeof(SharedTypeResolver))
      {
      }
   }
}
