//===========================================================================
// MODULE:  TestServiceHost.cs
// PURPOSE: WCF service host unit test driver
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
using System.Diagnostics;
using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// Project References

namespace WcfEx.Host.Test
{
   [TestClass]
   public class TestServiceHost
   {
      Process host = null;

      [TestInitialize]
      public void Initialize ()
      {
         host = Process.Start(@"..\..\..\Test\Bin\WcfExHost.exe");
         System.Threading.Thread.Sleep(1000);
      }

      [TestCleanup]
      public void Cleanup ()
      {
         if (host != null)
            host.Kill();
      }

      [TestMethod]
      public void TestServers ()
      {
         using (var proxy = Connect1())
         {
            Assert.AreEqual(proxy.Server.Ping("test1"), "test1");
         }
         using (var proxy = Connect2())
         {
            Assert.AreEqual(proxy.Server.Ping("test1"), "test1");
         }
         using (var proxy = Connect3())
         {
            Assert.AreEqual(proxy.Server.Ping("test1"), "test1");
         }
      }

      private Client<IServer1> Connect1()
      {
         return new Client<IServer1>(
            new NetTcpBinding()
            {
               OpenTimeout = TimeSpan.FromSeconds(10)
            },
            "net.tcp://localhost:42000/Server1/"
         );
      }

      private Client<IServer2> Connect2()
      {
         return new Client<IServer2>(
            new Udp.Binding()
            {
               OpenTimeout = TimeSpan.FromSeconds(10)
            },
            "udp://localhost:42000/Server2/"
         );
      }

      private Client<IServer3> Connect3()
      {
         return new Client<IServer3>(
            new NetNamedPipeBinding()
            {
               OpenTimeout = TimeSpan.FromSeconds(10)
            },
            "net.pipe://localhost/WcfExHost/Test/Server3/"
         );
      }
   }
}
