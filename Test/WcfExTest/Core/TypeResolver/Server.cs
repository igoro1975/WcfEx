//===========================================================================
// MODULE:  Server.cs
// PURPOSE: test WCF server for the type resolver
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
using System.IO;
using System.Linq;
using System.ServiceModel;
// Project References

namespace WcfEx.Test.Core.TypeResolver
{
   /// <summary>
   /// Shared type resolver server contract
   /// </summary>
   /// <remarks>
   /// This interface is implemented by the test server and
   /// used as the contract for testing the type resolver framework.
   /// </remarks>
   [ServiceContract(Namespace = "http://brentspell.us/Projects/WcfEx/Test/TestTypeResolver/")]
   public interface ISharedTypeResolverServer
   {
      /// <summary>
      /// Echoes a data value
      /// </summary>
      /// <param name="data">
      /// The value to echo
      /// </param>
      /// <returns>
      /// The echoed value
      /// </returns>
      [OperationContract]
      [SharedTypeResolver]
      Data Echo (Data data);
   }

   /// <summary>
   /// Custom type resolver server contract
   /// </summary>
   /// <remarks>
   /// This interface is implemented by the test server and
   /// used as the contract for testing the type resolver framework.
   /// </remarks>
   [ServiceContract(Namespace = "http://brentspell.us/Projects/WcfEx/Test/TestTypeResolver/")]
   [TypeResolver(typeof(CustomResolver))]
   public interface ICustomResolverServer
   {
      /// <summary>
      /// Echoes a data value
      /// </summary>
      /// <param name="data">
      /// The value to echo
      /// </param>
      /// <returns>
      /// The echoed value
      /// </returns>
      [OperationContract]
      Data Echo (Data data);
   }

   /// <summary>
   /// Type resolver test server
   /// </summary>
   /// <remarks>
   /// This class implements the test contract interface.
   /// </remarks>
   [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
   public class Server : 
      ISharedTypeResolverServer,
      ICustomResolverServer
   {
      #region IServer Implementation
      /// <summary>
      /// Echoes a data value
      /// </summary>
      /// <param name="data">
      /// The value to echo
      /// </param>
      /// <returns>
      /// The echoed value
      /// </returns>
      public Data Echo (Data data)
      {
         return data;
      }
      #endregion
   }
}
