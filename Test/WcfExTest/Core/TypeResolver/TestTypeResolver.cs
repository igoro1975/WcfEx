//===========================================================================
// MODULE:  TestTypeResolver.cs
// PURPOSE: WCF type resolver framework unit test driver
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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// Project References

namespace WcfEx.Test.Core.TypeResolver
{
   [TestClass]
   public class TestTypeResolver
   {
      public const String Address = "net.pipe://localhost/WcfExTest.Core.TypeResolver/";

      [TestMethod]
      public void TestSharedTypeResolver ()
      {
         using (var host = new ServiceHost(typeof(Server)))
         {
            host.AddServiceEndpoint(typeof(ISharedTypeResolverServer), new NetNamedPipeBinding(), Address);
            host.Open();
            using (var client = new Client<ISharedTypeResolverServer>(new NetNamedPipeBinding(), Address))
            {
               Assert.AreEqual(((Data1)client.Server.Echo(new Data1() { Value1 = 1 })).Value1, 1);
               Assert.AreEqual(((Data2)client.Server.Echo(new Data2() { Value2 = 2 })).Value2, 2);
            }
         }
      }

      [TestMethod]
      public void TestCustomResolver ()
      {
         using (var host = new ServiceHost(typeof(Server)))
         {
            host.AddServiceEndpoint(typeof(ICustomResolverServer), new NetNamedPipeBinding(), Address);
            host.Open();
            using (var client = new Client<ICustomResolverServer>(new NetNamedPipeBinding(), Address))
            {
               Assert.AreEqual(((Data1)client.Server.Echo(new Data1() { Value1 = 1 })).Value1, 1);
               Assert.AreEqual(((Data2)client.Server.Echo(new Data2() { Value2 = 2 })).Value2, 2);
            }
         }
      }
   }
}
