//===========================================================================
// MODULE:  CustomResolver.cs
// PURPOSE: test WCF custom type resolver
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
using System.Text;
// Project References

namespace WcfEx.Test.Core.TypeResolver
{
   /// <summary>
   /// Test custom type resolver
   /// </summary>
   /// <remarks>
   /// This resolver resolves the type names/types for derivatives of
   /// the Data class within the same assembly.
   /// </remarks>
   class CustomResolver : WcfEx.TypeResolver
   {
      public static readonly Uri Namespace = new Uri("http://brentspell.us/Projects/WcfEx/Test/Core/TypeResolver/");

      #region TypeResolver Overrides
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
      protected override Boolean TryResolve (Type type, Type declared, out String ns, out String name)
      {
         if (declared == typeof(Data))
         {
            ns = Namespace.ToString();
            name = type.Name;
            return true;
         }
         ns = name = null;
         return false;
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
      protected override Type Resolve (String ns, String name, Type declared)
      {
         Uri uri;
         if (Uri.TryCreate(ns, UriKind.Absolute, out uri))
            if (uri == Namespace)
               return declared.Assembly
                  .GetType(String.Format("{0}.{1}", declared.Namespace, name), true);
         return null;
      }
      #endregion
   }
}
