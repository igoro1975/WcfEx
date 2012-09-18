//===========================================================================
// MODULE:  Test.cs
// PURPOSE: Deflate WCF encoder unit test driver
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

namespace WcfEx.Test.Deflate
{
   [TestClass]
   public class Test
   {
      private const Int32 TestIterations = 100;
      private const Int32 TestParallelIterations = 100;
      private const Int32 TestPerformanceIterations = 1000;
      private const Int32 TestStreamLength = 100000;
      private const String TestBufferedUri = "net.pipe://localhost/WcfExTest.Deflate.Server/Buffered/";
      private const String TestStreamedUri = "net.pipe://localhost/WcfExTest.Deflate.Server/Streamed/";
      private ServiceHost serviceHost = new ServiceHost(typeof(Server));

      [TestInitialize]
      public void Initialize ()
      {
         // start up the in-process test server
         serviceHost.AddServiceEndpoint(
            typeof(IServer),
            new CustomBinding(
               new WcfEx.Deflate.BindingElement(),
               new NamedPipeTransportBindingElement()
               {
                  TransferMode = System.ServiceModel.TransferMode.Buffered,
                  MaxReceivedMessageSize = 2 * TestStreamLength
               }
            ),
            TestBufferedUri
         );
         serviceHost.AddServiceEndpoint(
            typeof(IServer),
            new CustomBinding(
               new WcfEx.Deflate.BindingElement(),
               new NamedPipeTransportBindingElement()
               {
                  TransferMode = System.ServiceModel.TransferMode.Streamed,
                  MaxReceivedMessageSize = 2 * TestStreamLength
               }
            ),
            TestStreamedUri
         );
         serviceHost.Description.Behaviors.Add(
            new ServiceThrottlingBehavior()
            {
               MaxConcurrentCalls = Int32.MaxValue,
               MaxConcurrentInstances = Int32.MaxValue,
               MaxConcurrentSessions = Int32.MaxValue
            }
         );
         serviceHost.CloseTimeout = TimeSpan.Zero;
         serviceHost.Open();
      }

      [TestCleanup]
      public void Cleanup ()
      {
         serviceHost.Close();
      }

      [TestMethod]
      public void TestConsecutive ()
      {
         // consecutive requests through dedicated clients
         for (Int32 i = 0; i < TestIterations; i++)
            using (var client = ConnectBuffered())
               Assert.AreEqual(
                  client.Server.Reverse(String.Format("test{0}", i)),
                  new String(String.Format("test{0}", i).Reverse().ToArray())
               );
         // consecutive requests through shared client
         using (var client = ConnectBuffered())
            for (Int32 i = 0; i < TestIterations; i++)
               Assert.AreEqual(
                  client.Server.Reverse(String.Format("test{0}", i)), 
                  new String(String.Format("test{0}", i).Reverse().ToArray())
               );
         // parallel requests through dedicated clients
         Parallel.For(0, TestParallelIterations,
            i =>
            {
               using (var client = ConnectBuffered())
                  Assert.AreEqual(
                     client.Server.Reverse(String.Format("test{0}", i)),
                     new String(String.Format("test{0}", i).Reverse().ToArray())
                  );
            }
         );
         // parallel requests through shared client
         using (var client = ConnectBuffered())
            Parallel.For(0, TestParallelIterations,
               i => Assert.AreEqual(
                  client.Server.Reverse(String.Format("test{0}", i)),
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
            using (var client = ConnectStreamed())
            {
               rng.NextBytes(buffer);
               var echo = client.Server.Echo(new MemoryStream(buffer));
               for (Int32 len = 0, read = 1; read != 0; len += read)
                  read = echo.Read(result, len, result.Length - len);
               Assert.IsTrue(Enumerable.SequenceEqual(buffer, result));
            }
         // consecutive requests through shared client
         using (var client = ConnectStreamed())
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
      public void TestPerformance ()
      {
         Stopwatch clock = new Stopwatch();
         // request/response
         using (var client = ConnectBuffered())
         {
            clock.Restart();
            for (Int32 i = 0; i < TestPerformanceIterations; i++)
               client.Server.Reverse("t");
            clock.Stop();
            TraceClock("R", clock);
         }
      }

      private Client<IServer> ConnectBuffered ()
      {
         return new Client<IServer>(
            new CustomBinding(
               new WcfEx.Deflate.BindingElement(),
               new NamedPipeTransportBindingElement()
               {
                  TransferMode = System.ServiceModel.TransferMode.Buffered
               }
            ),
            TestBufferedUri
         );
      }

      private Client<IServer> ConnectStreamed ()
      {
         return new Client<IServer>(
            new CustomBinding(
               new WcfEx.Deflate.BindingElement(),
               new NamedPipeTransportBindingElement()
               {
                  TransferMode = System.ServiceModel.TransferMode.Streamed,
                  MaxReceivedMessageSize = 2 * TestStreamLength
               }
            ),
            TestStreamedUri
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
   }
}
