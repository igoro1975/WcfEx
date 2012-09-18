//===========================================================================
// MODULE:  TestManagedBuffer.cs
// PURPOSE: WCF managed message buffer shim unit test driver
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
using System.ServiceModel.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// Project References

namespace WcfEx.Test.Core
{
   [TestClass]
   public class TestManagedBuffer
   {
      BufferManager manager = BufferManager.CreateBufferManager(1048576, 8192);

      [TestMethod]
      public void TestBuffer ()
      {
         var buffer = default(ManagedBuffer);
         // invalid constructor
         try
         {
            new ManagedBuffer(null, 0);
            Assert.Fail("Expected: Exception");
         }
         catch (AssertFailedException) { throw; }
         catch { }
         try
         {
            new ManagedBuffer(null, 8192);
            Assert.Fail("Expected: Exception");
         }
         catch (AssertFailedException) { throw; }
         catch { }
         try
         {
            new ManagedBuffer(manager, 0);
            Assert.Fail("Expected: Exception");
         }
         catch (AssertFailedException) { throw; }
         catch { }
         try
         {
            new ManagedBuffer(manager, -1);
            Assert.Fail("Expected: Exception");
         }
         catch (AssertFailedException) { throw; }
         catch { }
         try
         {
            new ManagedBuffer(null, default(ArraySegment<Byte>));
            Assert.Fail("Expected: Exception");
         }
         catch (AssertFailedException) { throw; }
         catch { }
         try
         {
            new ManagedBuffer(null, new ArraySegment<Byte>(new Byte[8192]));
            Assert.Fail("Expected: Exception");
         }
         catch (AssertFailedException) { throw; }
         catch { }
         try
         {
            new ManagedBuffer(manager, default(ArraySegment<Byte>));
            Assert.Fail("Expected: Exception");
         }
         catch (AssertFailedException) { throw; }
         catch { }
         // size constructor
         buffer = new ManagedBuffer(manager, 8192);
         Assert.IsNotNull(buffer.Raw);
         Assert.IsNotNull(buffer.Data.Array);
         Assert.AreEqual(buffer.Offset, 0);
         Assert.AreEqual(buffer.Data.Offset, 0);
         Assert.AreEqual(buffer.Length, 8192);
         Assert.AreEqual(buffer.Data.Count, 8192);
         buffer.Dispose();
         Assert.IsNull(buffer.Raw);
         Assert.IsNull(buffer.Data.Array);
         Assert.AreEqual(buffer.Offset, 0);
         Assert.AreEqual(buffer.Data.Offset, 0);
         Assert.AreEqual(buffer.Length, 0);
         Assert.AreEqual(buffer.Data.Count, 0);
         // buffer constructor
         buffer = new ManagedBuffer(manager, new ArraySegment<Byte>(manager.TakeBuffer(8192)));
         Assert.IsNotNull(buffer.Raw);
         Assert.IsNotNull(buffer.Data.Array);
         Assert.AreEqual(buffer.Offset, 0);
         Assert.AreEqual(buffer.Data.Offset, 0);
         Assert.AreEqual(buffer.Length, 8192);
         Assert.AreEqual(buffer.Data.Count, 8192);
         buffer.Dispose();
         Assert.IsNull(buffer.Raw);
         Assert.IsNull(buffer.Data.Array);
         Assert.AreEqual(buffer.Offset, 0);
         Assert.AreEqual(buffer.Data.Offset, 0);
         Assert.AreEqual(buffer.Length, 0);
         Assert.AreEqual(buffer.Data.Count, 0);
         // multiple disposal
         buffer.Dispose();
         buffer.Dispose();
      }
   }
}
