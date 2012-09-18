//===========================================================================
// MODULE:  TestAsyncResult.cs
// PURPOSE: AsyncResult unit test driver
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
// Project References

namespace WcfEx.Test.Core
{
   [TestClass]
   public class TestAsyncResult
   {
      [TestMethod]
      public void TestState ()
      {
         AsyncResult result;
         // null state
         result = new AsyncResult(null, null);
         Assert.IsNull(result.AsyncState);
         Assert.IsFalse(result.IsCompleted);
         result.Complete(null);
         Assert.IsNull(result.AsyncState);
         Assert.IsTrue(result.IsCompleted);
         // valid state
         result = new AsyncResult(null, 1);
         Assert.AreEqual(result.AsyncState, 1);
         Assert.IsFalse(result.IsCompleted);
         result.Complete(null);
         Assert.AreEqual(result.AsyncState, 1);
         Assert.IsTrue(result.IsCompleted);
         // callback + state
         result = new AsyncResult(o => { }, 2);
         Assert.AreEqual(result.AsyncState, 2);
         Assert.IsFalse(result.IsCompleted);
         result.Complete(null);
         Assert.AreEqual(result.AsyncState, 2);
         Assert.IsTrue(result.IsCompleted);
      }

      [TestMethod]
      public void TestCallback ()
      {
         AsyncResult result;
         Boolean called;
         // callback with null state
         called = false;
         result = new AsyncResult(
            ar =>
            {
               Assert.IsNull(ar.AsyncState);
               Assert.IsFalse(ar.CompletedSynchronously);
               Assert.IsTrue(ar.IsCompleted);
               Assert.IsTrue(ar.AsyncWaitHandle.WaitOne(0));
               called = true;
            },
            null
         );
         Assert.IsNull(result.AsyncState);
         Assert.IsFalse(result.IsCompleted);
         Assert.IsFalse(called);
         result.Complete(null);
         Assert.IsTrue(called);
         // callback with valid state
         called = false;
         result = new AsyncResult(
            ar =>
            {
               Assert.AreEqual(ar.AsyncState, 1);
               Assert.IsFalse(ar.CompletedSynchronously);
               Assert.IsTrue(ar.IsCompleted);
               Assert.IsTrue(ar.AsyncWaitHandle.WaitOne(0));
               called = true;
            },
            1
         );
         Assert.AreEqual(result.AsyncState, 1);
         Assert.IsFalse(result.IsCompleted);
         Assert.IsFalse(called);
         result.Complete(null);
         Assert.IsTrue(called);
      }

      [TestMethod]
      public void TestCompletion ()
      {
         AsyncResult result;
         // default completion
         result = new AsyncResult(null, null);
         Assert.IsFalse(result.CompletedSynchronously);
         Assert.IsFalse(result.IsCompleted);
         Assert.IsFalse(result.AsyncWaitHandle.WaitOne(0));
         Assert.IsFalse(result.AsyncWaitHandle.WaitOne(1));
         result.Complete(null);
         Assert.IsFalse(result.CompletedSynchronously);
         Assert.IsTrue(result.IsCompleted);
         Assert.IsTrue(result.AsyncWaitHandle.WaitOne(0));
         Assert.IsTrue(result.AsyncWaitHandle.WaitOne(1));
         // completion with callback
         result = new AsyncResult(o => { }, null);
         Assert.IsFalse(result.CompletedSynchronously);
         Assert.IsFalse(result.IsCompleted);
         Assert.IsFalse(result.AsyncWaitHandle.WaitOne(0));
         Assert.IsFalse(result.AsyncWaitHandle.WaitOne(1));
         result.Complete(null);
         Assert.IsFalse(result.CompletedSynchronously);
         Assert.IsTrue(result.IsCompleted);
         Assert.IsTrue(result.AsyncWaitHandle.WaitOne(0));
         Assert.IsTrue(result.AsyncWaitHandle.WaitOne(1));
         // completion with callback/state
         result = new AsyncResult(o => { }, 1);
         Assert.IsFalse(result.CompletedSynchronously);
         Assert.IsFalse(result.IsCompleted);
         Assert.IsFalse(result.AsyncWaitHandle.WaitOne(0));
         Assert.IsFalse(result.AsyncWaitHandle.WaitOne(1));
         result.Complete(null);
         Assert.IsFalse(result.CompletedSynchronously);
         Assert.IsTrue(result.IsCompleted);
         Assert.IsTrue(result.AsyncWaitHandle.WaitOne(0));
         Assert.IsTrue(result.AsyncWaitHandle.WaitOne(1));
         // completion with exception
         result = new AsyncResult(o => { }, 1);
         Assert.IsFalse(result.CompletedSynchronously);
         Assert.IsFalse(result.IsCompleted);
         Assert.IsFalse(result.AsyncWaitHandle.WaitOne(0));
         Assert.IsFalse(result.AsyncWaitHandle.WaitOne(1));
         result.Complete(null, new Exception("exception"));
         Assert.IsFalse(result.CompletedSynchronously);
         Assert.IsTrue(result.IsCompleted);
         Assert.IsTrue(result.AsyncWaitHandle.WaitOne(0));
         Assert.IsTrue(result.AsyncWaitHandle.WaitOne(1));
         // multiple completion
         result = new AsyncResult(o => { }, 1);
         result.Complete(null, new Exception("exception"));
         Assert.IsFalse(result.CompletedSynchronously);
         Assert.IsTrue(result.IsCompleted);
         Assert.IsTrue(result.AsyncWaitHandle.WaitOne(0));
         Assert.IsTrue(result.AsyncWaitHandle.WaitOne(1));
         try
         {
            result.Complete(null);
            Assert.Fail("Exception expected");
         }
         catch (AssertFailedException) { throw; }
         catch { }
      }

      [TestMethod]
      public void TestResult ()
      {
         AsyncResult result;
         // null result
         result = new AsyncResult(null, null);
         Assert.IsFalse(result.IsFaulted);
         result.Complete(null);
         Assert.AreEqual(result.GetResult(), null);
         Assert.AreEqual(result.GetResult<String>(), null);
         Assert.IsTrue(result.IsCompleted);
         Assert.IsFalse(result.IsFaulted);
         // non-null result
         result = new AsyncResult(null, null);
         Assert.IsFalse(result.IsFaulted);
         result.Complete("complete");
         Assert.AreEqual(result.GetResult(), "complete");
         Assert.AreEqual(result.GetResult<String>(), "complete");
         try
         {
            result.GetResult<Int32>();
            Assert.Fail("InvalidCastException expected");
         }
         catch (InvalidCastException) { }
         Assert.IsTrue(result.IsCompleted);
         Assert.IsFalse(result.IsFaulted);
         // exception result
         result = new AsyncResult(null, null);
         Assert.IsFalse(result.IsFaulted);
         result.Complete(null, new Exception("exception"));
         try
         {
            result.GetResult();
            Assert.Fail("Exception expected");
         }
         catch (Exception e)
         {
            Assert.AreEqual(e.Message, "exception");
         }
         Assert.IsTrue(result.IsCompleted);
         Assert.IsTrue(result.IsFaulted);
         // exception with typed result
         result = new AsyncResult(null, null);
         Assert.IsFalse(result.IsFaulted);
         result.Complete("complete", new Exception("exception"));
         try
         {
            result.GetResult<String>();
            Assert.Fail("Exception expected");
         }
         catch (Exception e)
         {
            Assert.AreEqual(e.Message, "exception");
         }
         Assert.IsTrue(result.IsCompleted);
         Assert.IsTrue(result.IsFaulted);
         // exception with invalid typed result
         result = new AsyncResult(null, null);
         Assert.IsFalse(result.IsFaulted);
         result.Complete("complete", new Exception("exception"));
         try
         {
            result.GetResult<Int32>();
            Assert.Fail("Exception expected");
         }
         catch (Exception e)
         {
            Assert.AreEqual(e.Message, "exception");
         }
         Assert.IsTrue(result.IsCompleted);
         Assert.IsTrue(result.IsFaulted);
      }

      [TestMethod]
      public void TestWait ()
      {
         AsyncResult result;
         // default timeout
         result = new AsyncResult(null, null);
         Assert.AreEqual(result.Timeout, TimeSpan.MaxValue);
         Assert.IsFalse(result.TryWaitFor(TimeSpan.Zero));
         Assert.IsFalse(result.TryWaitFor(TimeSpan.FromMilliseconds(1)));
         try
         {
            result.WaitFor(TimeSpan.Zero);
            Assert.Fail("TimeoutException expected");
         }
         catch (TimeoutException) { }
         try
         {
            result.WaitFor(TimeSpan.FromMilliseconds(1));
            Assert.Fail("TimeoutException expected");
         }
         catch (TimeoutException) { }
         result.Complete("complete", new Exception("exception"));
         Assert.IsTrue(result.TryWaitFor(TimeSpan.Zero));
         Assert.IsTrue(result.TryWaitFor(TimeSpan.MaxValue));
         result.WaitFor();
         result.WaitFor(TimeSpan.Zero);
         result.WaitFor(TimeSpan.MaxValue);
         // valid timeout
         result = new AsyncResult(null, null, TimeSpan.FromMilliseconds(10));
         Assert.AreEqual(result.Timeout, TimeSpan.FromMilliseconds(10));
         Assert.IsFalse(result.TryWaitFor(TimeSpan.Zero));
         Assert.IsFalse(result.TryWaitFor(TimeSpan.FromMilliseconds(1)));
         try
         {
            result.WaitFor(TimeSpan.Zero);
            Assert.Fail("TimeoutException expected");
         }
         catch (TimeoutException) { }
         try
         {
            result.WaitFor(TimeSpan.FromMilliseconds(1));
            Assert.Fail("TimeoutException expected");
         }
         catch (TimeoutException) { }
         try
         {
            result.WaitFor();
            Assert.Fail("TimeoutException expected");
         }
         catch (TimeoutException) { }
         result.Complete("complete", new Exception("exception"));
         Assert.IsTrue(result.TryWaitFor(TimeSpan.Zero));
         Assert.IsTrue(result.TryWaitFor(TimeSpan.MaxValue));
         result.WaitFor();
         result.WaitFor(TimeSpan.Zero);
         result.WaitFor(TimeSpan.MaxValue);
      }
   }
}
