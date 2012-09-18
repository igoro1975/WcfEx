//===========================================================================
// MODULE:  TypeResolver.cs
// PURPOSE: WCF base data contract type resolver
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
using System.Xml;
// Project References

namespace WcfEx
{
   /// <summary>
   /// Base type resolver
   /// </summary>
   /// <remarks>
   /// This class implements common functionality for custom type resolvers,
   /// including delegating default resolution to the known type resolver.
   /// </remarks>
   public abstract class TypeResolver : DataContractResolver
   {
      XmlDictionary dictionary = new XmlDictionary();

      #region DataContractResolver Overrides
      /// <summary>
      /// Attempts to resolve a type to an XML schema type name
      /// </summary>
      /// <param name="type">
      /// The managed type to resolve
      /// </param>
      /// <param name="declared">
      /// The declared parameter type
      /// </param>
      /// <param name="knownTypes">
      /// The base known type resolver
      /// </param>
      /// <param name="xmlName">
      /// Return the type name via here
      /// </param>
      /// <param name="xmlNamespace">
      /// Return the type namespace via here
      /// </param>
      /// <returns>
      /// True if the type was resolved
      /// False otherwise
      /// </returns>
      public override Boolean TryResolveType (
         Type type,
         Type declared,
         DataContractResolver knownTypes,
         out XmlDictionaryString xmlName,
         out XmlDictionaryString xmlNamespace)
      {
         if (!knownTypes.TryResolveType(type, declared, null, out xmlName, out xmlNamespace))
         {
            String ns, name;
            if (!TryResolve(type, declared, out ns, out name))
               return false;
            xmlNamespace = this.dictionary.Add(ns);
            xmlName = this.dictionary.Add(name);
         }
         return true;
      }
      /// <summary>
      /// Resolves a type name to a managed type
      /// </summary>
      /// <param name="name">
      /// The name of the type to resolve
      /// </param>
      /// <param name="ns">
      /// The named type's namespace
      /// </param>
      /// <param name="declared">
      /// Teh declared parameter type
      /// </param>
      /// <param name="knownTypes">
      /// The base known type resolver
      /// </param>
      /// <returns>
      /// The resolved managed type
      /// </returns>
      public override Type ResolveName (
         String name,
         String ns,
         Type declared,
         DataContractResolver knownTypes)
      {
         return
            knownTypes.ResolveName(name, ns, declared, null) ??
            Resolve(ns, name, declared);
      }
      #endregion

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
      protected abstract Boolean TryResolve (
         Type type,
         Type declared,
         out String ns,
         out String name);
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
      protected abstract Type Resolve (
         String ns,
         String name,
         Type declared);
      #endregion

   }
}
