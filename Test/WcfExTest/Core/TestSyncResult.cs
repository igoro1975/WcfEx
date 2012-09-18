//===========================================================================
// MODULE:  TestSyncResult.cs
// PURPOSE: SyncResult unit test driver
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
   public class TestSyncResult
   {
      [TestMethod]
      public void TestState ()
      {
         SyncResult result;
         result = new SyncResult(null, null);
         Assert.IsNull(result.AsyncState);
         Assert.IsTrue(result.IsCompleted);
         result = new SyncResult(null, 1);
         Assert.AreEqual(result.AsyncState, 1);
         Assert.IsTrue(result.IsCompleted);
         result = new SyncResult(o => { }, 2);
         Assert.AreEqual(result.AsyncState, 2);
         Assert.IsTrue(result.IsCompleted);
      }

      [TestMethod]
      public void TestCallback ()
      {
         SyncResult result;
         Boolean called;
         called = false;
         result = new SyncResult(
            ar =>
            {
               Assert.IsNull(ar.AsyncState);
               Assert.IsTrue(ar.CompletedSynchronously);
               Assert.IsTrue(ar.IsCompleted);
               Assert.IsTrue(ar.AsyncWaitHandle.WaitOne(0));
               called = true;
            },
            null
         );
         Assert.IsTrue(result.IsCompleted);
         Assert.IsTrue(called);
         called = false;
         result = new SyncResult(
            ar => 
            {
               Assert.AreEqual(ar.AsyncState, 1);
               Assert.IsTrue(ar.CompletedSynchronously);
               Assert.IsTrue(ar.IsCompleted);
               Assert.IsTrue(ar.AsyncWaitHandle.WaitOne(0));
               called = true;
            },
            1
         );
         Assert.IsTrue(result.IsCompleted);
         Assert.IsTrue(called);
      }

      [TestMethod]
      public void TestCompletion ()
      {
         SyncResult result;
         result = new SyncResult(null, null);
         Assert.IsTrue(result.CompletedSynchronously);
         Assert.IsTrue(result.IsCompleted);
         Assert.IsTrue(result.AsyncWaitHandle.WaitOne(0));
         result = new SyncResult(o => { }, null);
         Assert.IsTrue(result.CompletedSynchronously);
         Assert.IsTrue(result.IsCompleted);
         Assert.IsTrue(result.AsyncWaitHandle.WaitOne(0));
         result = new SyncResult(o => { }, 1);
         Assert.IsTrue(result.CompletedSynchronously);
         Assert.IsTrue(result.IsCompleted);
         Assert.IsTrue(result.AsyncWaitHandle.WaitOne(0));
      }

      [TestMethod]
      public void TestResult ()
      {
         SyncResult result;
         result = new SyncResult(null, null, null);
         Assert.AreEqual(result.GetResult(), null);
         Assert.AreEqual(result.GetResult<String>(), null);
         Assert.IsTrue(result.IsCompleted);
         result = new SyncResult(null, null, "complete");
         Assert.AreEqual(result.GetResult(), "complete");
         Assert.AreEqual(result.GetResult<String>(), "complete");
         try
         {
            result.GetResult<Int32>();
            Assert.Fail("InvalidCastException expected");
         }
         catch (InvalidCastException) { }
         Assert.IsTrue(result.IsCompleted);
      }

      [TestMethod]
      public void TestWait ()
      {
         SyncResult result = new SyncResult(null, null);
         Assert.IsTrue(result.TryWaitFor(TimeSpan.Zero));
         Assert.IsTrue(result.TryWaitFor(TimeSpan.MaxValue));
         result.WaitFor();
         result.WaitFor(TimeSpan.Zero);
         result.WaitFor(TimeSpan.MaxValue);
      }
   }
}
