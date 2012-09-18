//===========================================================================
// MODULE:  Test.cs
// PURPOSE: UDP WCF transport unit test driver
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// Project References
using WcfEx;

namespace WcfEx.Test.Udp
{
   [TestClass]
   public class Test
   {
      private const Int32 TestIterations = 100;
      private const Int32 TestParallelIterations = 100;
      private const Int32 TestPerformanceIterations = 1000;
      private static readonly String OneWayServiceUri = String.Format("udp://{0}:42000/", System.Net.Dns.GetHostName());
      private static readonly String TwoWayServiceUri = "udp://0.0.0.0:42001/";
      private static readonly String DuplexServiceUri = "udp://0.0.0.0:42002/";
      private static readonly String OneWayClientUri = OneWayServiceUri;
      private static readonly String TwoWayClientUri = "udp://127.0.0.1:42001/";
      private static readonly String BroadcastOneWayClientUri = "udp://255.255.255.255:42000/";
      private static readonly String BroadcastTwoWayClientUri = "udp://255.255.255.255:42001/";
      private static readonly String DuplexClientUri = "udp://127.0.0.1:42002/";
      private ServiceHost serviceHost;

      [TestInitialize]
      public void Initialize ()
      {
         this.serviceHost = new ServiceHost(typeof(Server));
         // start up the UDP test server
         this.serviceHost.AddServiceEndpoint(
            typeof(IOneWayServer),
            new WcfEx.Udp.Binding()
            {
               Element = new WcfEx.Udp.BindingElement()
               {
                  ReceiveBufferSize = 1048576
               }
            },
            OneWayServiceUri
         );
         this.serviceHost.AddServiceEndpoint(
            typeof(ITwoWayServer),
            new WcfEx.Udp.Binding()
            {
               Element = new WcfEx.Udp.BindingElement()
               {
                  ReceiveBufferSize = 1048576,
                  MaxReceivedMessageSize = 65536
               }
            },
            TwoWayServiceUri
         );
         this.serviceHost.AddServiceEndpoint(
            typeof(IDuplexServer),
            new CustomBinding(
               new ReliableSessionBindingElement(false)
               {
                  MaxPendingChannels = 16384
               },
               new CompositeDuplexBindingElement(),
               new WcfEx.Udp.BindingElement()
            ),
            DuplexServiceUri
         );
         this.serviceHost.Description.Behaviors.Add(
            new ServiceThrottlingBehavior()
            {
               MaxConcurrentCalls = Int32.MaxValue,
               MaxConcurrentInstances = Int32.MaxValue,
               MaxConcurrentSessions = Int32.MaxValue
            }
         );
         this.serviceHost.CloseTimeout = TimeSpan.Zero;
         this.serviceHost.Open();
      }

      [TestCleanup]
      public void Cleanup ()
      {
         serviceHost.Close();
      }

      [TestMethod]
      public void TestOneWay ()
      {
         // consecutive requests through dedicated clients
         using (var waiter = Server.ResetOneWayCounter(TestIterations))
         {
            for (Int32 i = 0; i < TestIterations; i++)
               using (var client = ConnectOneWay())
                  client.Server.FireAndForget(String.Format("test{0}", i));
            Assert.IsTrue(waiter.WaitOne(TestIterations * 100));
         }
         using (var waiter = Server.ResetOneWayCounter(TestIterations))
         {
            for (Int32 i = 0; i < TestIterations; i++)
               using (var client = ConnectTwoWay())
                  client.Server.FireAndForget(String.Format("test{0}", i));
            Assert.IsTrue(waiter.WaitOne(TestIterations * 100));
         }
         // consecutive requests through shared client
         using (var waiter = Server.ResetOneWayCounter(TestIterations))
         {
            using (var client = ConnectOneWay())
               for (Int32 i = 0; i < TestIterations; i++)
                  client.Server.FireAndForget(String.Format("test{0}", i));
            Assert.IsTrue(waiter.WaitOne(TestIterations * 100));
         }
         using (var waiter = Server.ResetOneWayCounter(TestIterations))
         {
            using (var client = ConnectTwoWay())
               for (Int32 i = 0; i < TestIterations; i++)
                  client.Server.FireAndForget(String.Format("test{0}", i));
            Assert.IsTrue(waiter.WaitOne(TestIterations * 100));
         }
         // parallel requests through dedicated clients
         using (var waiter = Server.ResetOneWayCounter(TestParallelIterations))
         {
            Parallel.For(0, TestParallelIterations,
               i =>
               {
                  using (var client = ConnectOneWay())
                     client.Server.FireAndForget(String.Format("test{0}", i));
               }
            );
            Assert.IsTrue(waiter.WaitOne(TestParallelIterations * 100));
         }
         using (var waiter = Server.ResetOneWayCounter(TestParallelIterations))
         {
            Parallel.For(0, TestParallelIterations,
               i =>
               {
                  using (var client = ConnectTwoWay())
                     client.Server.FireAndForget(String.Format("test{0}", i));
               }
            );
            Assert.IsTrue(waiter.WaitOne(TestParallelIterations * 100));
         }
         // parallel requests through shared client
         using (var waiter = Server.ResetOneWayCounter(TestParallelIterations))
         {
            using (var client = ConnectOneWay())
               Parallel.For(0, TestParallelIterations,
                  i => client.Server.FireAndForget(String.Format("test{0}", i))
               );
            Assert.IsTrue(waiter.WaitOne(TestParallelIterations * 100));
         }
         using (var waiter = Server.ResetOneWayCounter(TestParallelIterations))
         {
            using (var client = ConnectTwoWay())
               Parallel.For(0, TestParallelIterations,
                  i => client.Server.FireAndForget(String.Format("test{0}", i))
               );
            Assert.IsTrue(waiter.WaitOne(TestParallelIterations * 100));
         }
      }

      [TestMethod]
      public void TestTwoWay()
      {
         // consecutive requests through dedicated clients
         for (Int32 i = 0; i < TestIterations; i++)
            using (var client = ConnectTwoWay())
               Assert.AreEqual(
                  client.Server.Reverse(String.Format("test{0}", i)),
                  new String(String.Format("test{0}", i).Reverse().ToArray())
               );
         // consecutive requests through shared client
         using (var client = ConnectTwoWay())
            for (Int32 i = 0; i < TestIterations; i++)
               Assert.AreEqual(
                  client.Server.Reverse(String.Format("test{0}", i)),
                  new String(String.Format("test{0}", i).Reverse().ToArray())
               );
         // parallel requests through dedicated clients
         Parallel.For(0, TestParallelIterations,
            i =>
            {
               using (var client = ConnectTwoWay())
                  Assert.AreEqual(
                     client.Server.Reverse(String.Format("test{0}", i)),
                     new String(String.Format("test{0}", i).Reverse().ToArray())
                  );
            }
         );
         // parallel requests through shared client
         using (var client = ConnectTwoWay())
            Parallel.For(0, TestParallelIterations,
               i => Assert.AreEqual(
                  client.Server.Reverse(String.Format("test{0}", i)),
                  new String(String.Format("test{0}", i).Reverse().ToArray())
               )
            );
         // timeout
         using (var client = ConnectTwoWay())
         {
            client.Context.OperationTimeout = TimeSpan.FromMilliseconds(10);
            // shut down the existing server instance
            Cleanup();
            try
            {
               // submit timeout request
               try
               {
                  client.Server.Reverse("test");
                  Assert.Fail("Expected: TimeoutException");
               }
               catch (TimeoutException) { }
            }
            finally
            {
               // restart the server instance
               Initialize();
            }
            // verify subsequent request succeeds
            for (Int32 i = 0; i < TestIterations; i++)
               Assert.AreEqual(
                  client.Server.EndReverse(client.Server.BeginReverse("test2", null, null)),
                  "2tset"
               );
         }
      }

      [TestMethod]
      public void TestOneWayBroadcast()
      {
         // consecutive requests through dedicated clients
         using (var waiter = Server.ResetOneWayCounter(TestIterations))
         {
            for (Int32 i = 0; i < TestIterations; i++)
               using (var client = ConnectBroadcastOneWay())
                  client.Server.FireAndForget(String.Format("test{0}", i));
            Assert.IsTrue(waiter.WaitOne(TestIterations * 100));
         }
         using (var waiter = Server.ResetOneWayCounter(TestIterations))
         {
            for (Int32 i = 0; i < TestIterations; i++)
               using (var client = ConnectBroadcastTwoWay())
                  client.Server.FireAndForget(String.Format("test{0}", i));
            Assert.IsTrue(waiter.WaitOne(TestIterations * 100));
         }
         // consecutive requests through shared client
         using (var waiter = Server.ResetOneWayCounter(TestIterations))
         {
            using (var client = ConnectBroadcastOneWay())
               for (Int32 i = 0; i < TestIterations; i++)
                  client.Server.FireAndForget(String.Format("test{0}", i));
            Assert.IsTrue(waiter.WaitOne(TestIterations * 100));
         }
         using (var waiter = Server.ResetOneWayCounter(TestIterations))
         {
            using (var client = ConnectBroadcastTwoWay())
               for (Int32 i = 0; i < TestIterations; i++)
                  client.Server.FireAndForget(String.Format("test{0}", i));
            Assert.IsTrue(waiter.WaitOne(TestIterations * 100));
         }
         // parallel requests through dedicated clients
         using (var waiter = Server.ResetOneWayCounter(TestParallelIterations))
         {
            Parallel.For(0, TestParallelIterations,
               i =>
               {
                  using (var client = ConnectBroadcastOneWay())
                     client.Server.FireAndForget(String.Format("test{0}", i));
               }
            );
            Assert.IsTrue(waiter.WaitOne(TestParallelIterations * 100));
         }
         using (var waiter = Server.ResetOneWayCounter(TestParallelIterations))
         {
            Parallel.For(0, TestParallelIterations,
               i =>
               {
                  using (var client = ConnectBroadcastTwoWay())
                     client.Server.FireAndForget(String.Format("test{0}", i));
               }
            );
            Assert.IsTrue(waiter.WaitOne(TestParallelIterations * 1000));
         }
         // parallel requests through shared client
         using (var waiter = Server.ResetOneWayCounter(TestParallelIterations))
         {
            using (var client = ConnectBroadcastOneWay())
               Parallel.For(0, TestParallelIterations,
                  i => client.Server.FireAndForget(String.Format("test{0}", i))
               );
            Assert.IsTrue(waiter.WaitOne(TestParallelIterations * 1000));
         }
         using (var waiter = Server.ResetOneWayCounter(TestParallelIterations))
         {
            using (var client = ConnectBroadcastTwoWay())
               Parallel.For(0, TestParallelIterations,
                  i => client.Server.FireAndForget(String.Format("test{0}", i))
               );
            Assert.IsTrue(waiter.WaitOne(TestParallelIterations * 1000));
         }
      }

      [TestMethod]
      public void TestTwoWayBroadcast()
      {
         // consecutive requests through dedicated clients
         for (Int32 i = 0; i < TestIterations; i++)
            using (var client = ConnectBroadcastTwoWay())
               Assert.AreEqual(
                  client.Server.Reverse(String.Format("test{0}", i)),
                  new String(String.Format("test{0}", i).Reverse().ToArray())
               );
         // consecutive requests through shared client
         using (var client = ConnectBroadcastTwoWay())
            for (Int32 i = 0; i < TestIterations; i++)
               Assert.AreEqual(
                  client.Server.Reverse(String.Format("test{0}", i)),
                  new String(String.Format("test{0}", i).Reverse().ToArray())
               );
         // parallel requests through dedicated clients
         Parallel.For(0, TestParallelIterations,
            i =>
            {
               using (var client = ConnectBroadcastTwoWay())
                  Assert.AreEqual(
                     client.Server.Reverse(String.Format("test{0}", i)),
                     new String(String.Format("test{0}", i).Reverse().ToArray())
                  );
            }
         );
         // parallel requests through shared client
         using (var client = ConnectBroadcastTwoWay())
            Parallel.For(0, TestParallelIterations,
               i => Assert.AreEqual(
                  client.Server.Reverse(String.Format("test{0}", i)),
                  new String(String.Format("test{0}", i).Reverse().ToArray())
               )
            );
      }

      [TestMethod]
      public void TestDuplex()
      {
         // consecutive requests through dedicated clients
         for (Int32 i = 0; i < TestIterations; i++)
            using (var client = ConnectDuplex(i))
               Assert.AreEqual(
                  client.Server.Callback(String.Format("test{0}", i)),
                  new String(String.Format("test{0}", i).Reverse().ToArray())
               );
         // consecutive requests through shared client
         using (var client = ConnectDuplex(0))
            for (Int32 i = 0; i < TestIterations; i++)
               Assert.AreEqual(
                  client.Server.Callback(String.Format("test{0}", i)),
                  new String(String.Format("test{0}", i).Reverse().ToArray())
               );
         // parallel requests through dedicated clients
         Parallel.For(0, TestParallelIterations,
            i =>
            {
               using (var client = ConnectDuplex(i))
                  Assert.AreEqual(
                     client.Server.Callback(String.Format("test{0}", i)),
                     new String(String.Format("test{0}", i).Reverse().ToArray())
                  );
            }
         );
         // parallel requests through shared client
         using (var client = ConnectDuplex(0))
            Parallel.For(0, TestParallelIterations,
               i => Assert.AreEqual(
                  client.Server.Callback(String.Format("test{0}", i)),
                  new String(String.Format("test{0}", i).Reverse().ToArray())
               )
            );
      }

      [TestMethod]
      public void TestAsync ()
      {
         // async consecutive requests
         using (var client = ConnectTwoWay())
            for (Int32 i = 0; i < TestIterations; i++)
            {
               var ar = client.Server.BeginReverse(String.Format("test{0}", i), null, null);
               if (ar.CompletedSynchronously || ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(60)))
                  Assert.AreEqual(
                     client.Server.EndReverse(ar),
                     new String(String.Format("test{0}", i).Reverse().ToArray())
                  );
            }
         // async parallel requests
         using (var client = ConnectTwoWay())
            Parallel.For(0, TestParallelIterations,
               i => Assert.AreEqual(
                  client.Server.EndReverse(client.Server.BeginReverse(String.Format("test{0}", i), null, null)),
                  new String(String.Format("test{0}", i).Reverse().ToArray())
               )
            );
         // async request with timeout
         using (var client = ConnectTwoWay())
         {
            IAsyncResult ar1 = null;
            IAsyncResult ar2 = null;
            client.Context.OperationTimeout = TimeSpan.FromMilliseconds(10);
            // shut down the existing server instance
            Cleanup();
            try
            {
               // submit timeout requests
               ar1 = client.Server.BeginReverse("test1", null, null);
               Assert.IsFalse(ar1.CompletedSynchronously);
               Assert.IsFalse(ar1.AsyncWaitHandle.WaitOne(20));
               ar2 = client.Server.BeginReverse("test1", null, null);
               Assert.IsFalse(ar2.CompletedSynchronously);
               Assert.IsFalse(ar2.AsyncWaitHandle.WaitOne(20));
               try
               {
                  client.Server.EndReverse(ar1);
                  Assert.Fail("Expected: TimeoutException");
               }
               catch (TimeoutException) { }
            }
            finally
            {
               // restart the server instance
               Initialize();
            }
            // verify subsequent request succeeds
            for (Int32 i = 0; i < TestIterations; i++)
               Assert.AreEqual(
                  client.Server.EndReverse(client.Server.BeginReverse("test2", null, null)),
                  "2tset"
               );
            try
            {
               client.Server.EndReverse(ar2);
               Assert.Fail("Expected: TimeoutException");
            }
            catch (TimeoutException) { }
         }
      }

      [TestMethod]
      public void TestException ()
      {
         // unicast exception through dedicated client
         for (Int32 i = 0; i < TestIterations; i++)
            using (var client = ConnectTwoWay())
               try
               {
                  client.Server.Throw(String.Format("test{0}", i));
                  Assert.Fail();
               }
               catch (Exception e)
               {
                  Assert.AreEqual(e.Message, String.Format("test{0}", i));
               }
         // unicast exception through shared client
         using (var client = ConnectTwoWay())
            for (Int32 i = 0; i < TestIterations; i++)
               try
               {
                  client.Server.Throw(String.Format("test{0}", i));
                  Assert.Fail();
               }
               catch (Exception e)
               {
                  Assert.AreEqual(e.Message, String.Format("test{0}", i));
               }
         // broadcast exception through dedicated client
         for (Int32 i = 0; i < TestIterations; i++)
            using (var client = ConnectBroadcastTwoWay())
               try
               {
                  client.Server.Throw(String.Format("test{0}", i));
                  Assert.Fail();
               }
               catch (Exception e)
               {
                  Assert.AreEqual(e.Message, String.Format("test{0}", i));
               }
         // broadcast exception through shared client
         using (var client = ConnectBroadcastTwoWay())
            for (Int32 i = 0; i < TestIterations; i++)
               try
               {
                  client.Server.Throw(String.Format("test{0}", i));
                  Assert.Fail();
               }
               catch (Exception e)
               {
                  Assert.AreEqual(e.Message, String.Format("test{0}", i));
               }
      }

      [TestMethod]
      public void TestPerformance ()
      {
         Stopwatch clock = new Stopwatch();
         // one-way connect/disconnect
         clock.Restart();
         for (Int32 i = 0; i < TestPerformanceIterations; i++)
            ConnectOneWay().Close();
         clock.Stop();
         TraceClock("1C", clock);
         // two-way connect/disconnect
         clock.Restart();
         for (Int32 i = 0; i < TestPerformanceIterations; i++)
            ConnectTwoWay().Close();
         clock.Stop();
         TraceClock("2C", clock);
         // one-way unicast
         using (var waiter = Server.ResetOneWayCounter(TestPerformanceIterations))
         {
            using (var client = ConnectOneWay())
            {
               clock.Restart();
               for (Int32 i = 0; i < TestPerformanceIterations; i++)
                  client.Server.FireAndForget("t");
               clock.Stop();
               TraceClock("1U", clock);
            }
            Assert.IsTrue(waiter.WaitOne(TestPerformanceIterations * 100));
         }
         // two-way unicast
         using (var waiter = Server.ResetOneWayCounter(TestPerformanceIterations))
         {
            using (var client = ConnectTwoWay())
            {
               clock.Restart();
               for (Int32 i = 0; i < TestPerformanceIterations; i++)
                  client.Server.FireAndForget("t");
               clock.Stop();
               TraceClock("2U", clock);
            }
            Assert.IsTrue(waiter.WaitOne(TestPerformanceIterations * 100));
         }
         // one-way broadcast
         using (var waiter = Server.ResetOneWayCounter(TestPerformanceIterations))
         {
            using (var client = ConnectBroadcastOneWay())
            {
               clock.Restart();
               for (Int32 i = 0; i < TestPerformanceIterations; i++)
                  client.Server.FireAndForget("t");
               clock.Stop();
               TraceClock("1B", clock);
            }
            Assert.IsTrue(waiter.WaitOne(TestPerformanceIterations * 100));
         }
         // two-way broadcast
         using (var waiter = Server.ResetOneWayCounter(TestPerformanceIterations))
         {
            using (var client = ConnectBroadcastTwoWay())
            {
               clock.Restart();
               for (Int32 i = 0; i < TestPerformanceIterations; i++)
                  client.Server.FireAndForget("t");
               clock.Stop();
               TraceClock("2B", clock);
            }
            Assert.IsTrue(waiter.WaitOne(TestPerformanceIterations * 100));
         }
      }

      private Client<IOneWayServer> ConnectOneWay ()
      {
         return new Client<IOneWayServer>(
            new WcfEx.Udp.Binding(),
            OneWayClientUri
         );
      }

      private Client<ITwoWayServer> ConnectTwoWay ()
      {
         return new Client<ITwoWayServer>(
            new WcfEx.Udp.Binding()
            {
               Element = new WcfEx.Udp.BindingElement()
               {
                  MaxReceivedMessageSize = 65536
               }
            },
            TwoWayClientUri
         );
      }

      private Client<IOneWayServer> ConnectBroadcastOneWay()
      {
         return new Client<IOneWayServer>(
            new WcfEx.Udp.Binding(),
            BroadcastOneWayClientUri
         );
      }

      private Client<ITwoWayServer> ConnectBroadcastTwoWay()
      {
         return new Client<ITwoWayServer>(
            new CustomBinding(
               new WcfEx.Udp.BindingElement()
               {
                  MaxReceivedMessageSize = 65536
               }
            ),
            BroadcastTwoWayClientUri
         );
      }

      private DuplexClient<IDuplexServer, IDuplexCallback> ConnectDuplex(Int32 portOffset)
      {
         var client = new DuplexClient<IDuplexServer, IDuplexCallback>(
            new ClientCallback(),
            new CustomBinding(
               new ReliableSessionBindingElement(false)
               {
                  MaxPendingChannels = 16384
               },
               new CompositeDuplexBindingElement()
               {
                  ClientBaseAddress = new Uri(
                     String.Format(
                        "udp://{0}:{1}/",
                        System.Net.Dns.GetHostName(),
                        43000 + portOffset
                     )
                  )
               },
               new WcfEx.Udp.BindingElement()
            ),
            DuplexClientUri
         );
         client.Open();
         return client;
      }

      private void TraceClock(String trace, Stopwatch clock)
      {
         Debug.WriteLine(
            String.Format(
               "{0}: {1,5} ms, {2:0.000} ms/i, {3} i",
               trace,
               clock.ElapsedMilliseconds,
               (float)clock.ElapsedMilliseconds / (float)TestPerformanceIterations,
               TestPerformanceIterations
            )
         );
      }

      [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
      private class ClientCallback : IDuplexCallback
      {
         public String Reverse(String param)
         {
            return new String(param.Reverse().ToArray());
         }
      }
   }
}
