//===========================================================================
// MODULE:  Test.cs
// PURPOSE: in-process WCF transport unit test driver
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

namespace WcfEx.Test.InProc
{
   [TestClass]
   public class Test
   {
      private const Int32 TestIterations = 100;
      private const Int32 TestParallelIterations = 100;
      private const Int32 TestPerformanceIterations = 1000;
      private const Int32 TestStreamLength = 100000;
      private const String TestServiceUri = "inProc://this/WcfExTest.InProc.Server";
      private ServiceHost serviceHost = new ServiceHost(typeof(Server));

      [TestInitialize]
      public void Initialize ()
      {
         // max out the thread pools, to avoid WCF thread spin up time
         Int32 minThreads, maxThreads;
         System.Threading.ThreadPool.GetMinThreads(out minThreads, out minThreads);
         System.Threading.ThreadPool.GetMaxThreads(out maxThreads, out maxThreads);
         System.Threading.ThreadPool.SetMaxThreads(
            Math.Max(TestParallelIterations, maxThreads),
            Math.Max(TestParallelIterations, maxThreads)
         );
         System.Threading.ThreadPool.SetMinThreads(
            Math.Max(256, minThreads),
            Math.Max(256, minThreads)
         );
         // start up the in-process test server
         serviceHost.AddServiceEndpoint(
            typeof(IServer),
            new WcfEx.InProc.Binding(),
            TestServiceUri
         );
         serviceHost.Description.Behaviors.Add(
            new ServiceThrottlingBehavior()
            {
               MaxConcurrentCalls = Int32.MaxValue,
               MaxConcurrentInstances = Int32.MaxValue,
               MaxConcurrentSessions = Int32.MaxValue
            }
         );
         serviceHost.Open();
      }

      [TestCleanup]
      public void Cleanup ()
      {
         serviceHost.Close();
      }

      [TestMethod]
      public void TestRequestResponse ()
      {
         // consecutive requests through dedicated clients
         for (Int32 i = 0; i < TestIterations; i++)
            using (var client = Connect())
               Assert.AreEqual(
                  client.Server.Reverse(String.Format("test{0}", i)),
                  new String(String.Format("test{0}", i).Reverse().ToArray())
               );
         // consecutive requests through shared client
         using (var client = Connect())
            for (Int32 i = 0; i < TestIterations; i++)
               Assert.AreEqual(
                  client.Server.Reverse(String.Format("test{0}", i)), 
                  new String(String.Format("test{0}", i).Reverse().ToArray())
               );
         // parallel requests through dedicated clients
         Parallel.For(0, TestParallelIterations,
            i =>
            {
               using (var client = Connect())
                  Assert.AreEqual(
                     client.Server.Reverse(String.Format("test{0}", i)),
                     new String(String.Format("test{0}", i).Reverse().ToArray())
                  );
            }
         );
         // parallel requests through shared client
         using (var client = Connect())
            Parallel.For(0, TestParallelIterations,
               i => Assert.AreEqual(
                  client.Server.Reverse(String.Format("test{0}", i)),
                  new String(String.Format("test{0}", i).Reverse().ToArray())
               )
            );
      }

      [TestMethod]
      public void TestOneWay ()
      {
         // one-way requests through dedicated clients
         using (var waiter = Server.ResetOneWayCounter(TestIterations))
         {
            for (Int32 i = 0; i < TestIterations; i++)
               using (var client = Connect())
                  client.Server.FireAndForget(String.Format("test{0}", i));
            Assert.IsTrue(waiter.WaitOne(TestIterations * 100));
         }
         // one-way requests through shared client
         using (var waiter = Server.ResetOneWayCounter(TestIterations))
         {
            using (var client = Connect())
               for (Int32 i = 0; i < TestIterations; i++)
                  client.Server.FireAndForget(String.Format("test{0}", i));
            Assert.IsTrue(waiter.WaitOne(TestIterations * 100));
         }
      }

      [TestMethod]
      public void TestSession ()
      {
         // session round tripping test
         using (var client = Connect())
         {
            String sessionID = client.Context.SessionId;
            for (Int32 i = 0; i < TestIterations; i++)
            {
               Assert.AreEqual(client.Server.QuerySessionID(), sessionID);
               Assert.AreEqual(client.Context.SessionId, sessionID);
            }
         }
         // parallel session with dedicated clients
         Parallel.For(0, TestParallelIterations,
            i =>
            {
               using (var client = Connect())
               {
                  String sessionID = client.Context.SessionId;
                  Assert.AreEqual(client.Server.QuerySessionID(), sessionID);
                  Assert.AreEqual(client.Context.SessionId, sessionID);
               }
            }
         );
         // parallel session with shared client
         using (var client = Connect())
         {
            String sessionID = client.Context.SessionId;
            Parallel.For(0, TestParallelIterations,
               i =>
               {
                  Assert.AreEqual(client.Server.QuerySessionID(), sessionID);
                  Assert.AreEqual(client.Context.SessionId, sessionID);
               }
            );
         }
      }

      [TestMethod]
      public void TestCallback ()
      {
         // consecutive requests with callback through dedicated clients
         for (Int32 i = 0; i < TestIterations; i++)
            using (var client = Connect())
               Assert.AreEqual(
                  client.Server.Callback(String.Format("test{0}", i)),
                  new String(String.Format("test{0}", i).Reverse().ToArray())
               );
         // consecutive requests with callback through shared client
         using (var client = Connect())
            for (Int32 i = 0; i < TestIterations; i++)
               Assert.AreEqual(
                  client.Server.Callback(String.Format("test{0}", i)),
                  new String(String.Format("test{0}", i).Reverse().ToArray())
               );
         // parallel requests with callback through dedicated clients
         Parallel.For(0, TestParallelIterations,
            i =>
            {
               using (var client = Connect())
                  Assert.AreEqual(
                     client.Server.Callback(String.Format("test{0}", i)),
                     new String(String.Format("test{0}", i).Reverse().ToArray())
                  );
            }
         );
         // parallel requests with callback through shared client
         using (var client = Connect())
            Parallel.For(0, TestParallelIterations,
               i => Assert.AreEqual(
                  client.Server.Callback(String.Format("test{0}", i)),
                  new String(String.Format("test{0}", i).Reverse().ToArray())
               )
            );
      }

      [TestMethod]
      public void TestStreaming ()
      {
         var rng = new Random();
         var buffer = new Byte[TestStreamLength];
         var result = new Byte[buffer.Length];
         // consecutive requests through dedicated clients
         for (Int32 i = 0; i < TestIterations; i++)
            using (var client = Connect())
            {
               rng.NextBytes(buffer);
               var echo = client.Server.Echo(new MemoryStream(buffer));
               for (Int32 len = 0, read = 1; read != 0; len += read)
                  read = echo.Read(result, len, result.Length - len);
               Assert.IsTrue(Enumerable.SequenceEqual(buffer, result));
            }
         // consecutive requests through shared client
         using (var client = Connect())
            for (Int32 i = 0; i < TestIterations; i++)
            {
               rng.NextBytes(buffer);
               var echo = client.Server.Echo(new MemoryStream(buffer));
               for (Int32 len = 0, read = 1; read != 0; len += read)
                  read = echo.Read(result, len, result.Length - len);
               Assert.IsTrue(Enumerable.SequenceEqual(buffer, result));
            }
      }

      [TestMethod]
      public void TestException ()
      {
         for (Int32 i = 0; i < TestIterations; i++)
            using (var client = Connect())
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
         // connect/disconnect
         clock.Restart();
         for (Int32 i = 0; i < TestPerformanceIterations; i++)
            Connect().Close();
         clock.Stop();
         TraceClock("C", clock);
         // one-way message
         using (var waiter = Server.ResetOneWayCounter(TestPerformanceIterations))
         {
            using (var client = Connect())
            {
               clock.Restart();
               for (Int32 i = 0; i < TestPerformanceIterations; i++)
                  client.Server.FireAndForget("t");
               clock.Stop();
               TraceClock("O", clock);
            }
            Assert.IsTrue(waiter.WaitOne(TestPerformanceIterations * 100));
         }
         // request/response
         using (var client = Connect())
         {
            clock.Restart();
            for (Int32 i = 0; i < TestPerformanceIterations; i++)
               client.Server.Reverse("t");
            clock.Stop();
            TraceClock("R", clock);
         }
         // callback
         using (var client = Connect())
         {
            clock.Restart();
            for (Int32 i = 0; i < TestPerformanceIterations; i++)
               client.Server.Callback("t");
            clock.Stop();
            TraceClock("B", clock);
         }
      }

      private DuplexClient<IServer, ICallback> Connect ()
      {
         return new DuplexClient<IServer, ICallback>(
            new ClientCallback(),
            new WcfEx.InProc.Binding(),
            TestServiceUri
         );
      }

      private void TraceClock (String trace, Stopwatch clock)
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
      private class ClientCallback : ICallback
      {
         public String Callback (String param)
         {
            return new String(param.Reverse().ToArray());
         }
      }
   }
}
