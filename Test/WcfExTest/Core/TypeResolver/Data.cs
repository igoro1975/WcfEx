//===========================================================================
// MODULE:  StringResolver.cs
// PURPOSE: test type resolver
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
using System.Linq;
using System.Reflection;
// Project References

namespace WcfEx.Test.Core.TypeResolver
{
   /// <summary>
   /// Type resolver test data class
   /// </summary>
   /// <remarks>
   /// DataContractAttribute is not required here, because we are
   /// using a custom type resolver.
   /// </remarks>
   public abstract class Data
   {
   }

   /// <summary>
   /// Derived type resolver test data class
   /// </summary>
   public sealed class Data1 : Data
   {
      public Int32 Value1;
   }

   /// <summary>
   /// Derived type resolver test data class
   /// </summary>
   public sealed class Data2 : Data
   {
      public Int32 Value2;
   }
}
