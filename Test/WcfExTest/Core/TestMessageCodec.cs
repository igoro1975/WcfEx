//===========================================================================
// MODULE:  TestMessageCodec.cs
// PURPOSE: WCF message codec wrapper unit test driver
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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// Project References

namespace WcfEx.Test.Core
{
   [TestClass]
   public class TestMessageCodec
   {
      BufferManager manager = BufferManager.CreateBufferManager(1048576, 65536);
      MessageEncoder wcfCodec = new BinaryMessageEncodingBindingElement()
         .CreateMessageEncoderFactory()
         .Encoder;

      [TestMethod]
      public void TestConstruction ()
      {
         // invalid constructor
         try
         {
            new MessageCodec(null, null, 0);
            Assert.Fail("Expected: Exception");
         }
         catch (AssertFailedException) { throw; }
         catch { }
         try
         {
            new MessageCodec(manager, null, 0);
            Assert.Fail("Expected: Exception");
         }
         catch (AssertFailedException) { throw; }
         catch { }
         try
         {
            new MessageCodec(manager, wcfCodec, 0);
            Assert.Fail("Expected: Exception");
         }
         catch (AssertFailedException) { throw; }
         catch { }
         try
         {
            new MessageCodec(manager, wcfCodec, -1);
            Assert.Fail("Expected: Exception");
         }
         catch (AssertFailedException) { throw; }
         catch { }
         try
         {
            new MessageCodec(manager, null, -1);
            Assert.Fail("Expected: Exception");
         }
         catch (AssertFailedException) { throw; }
         catch { }
         try
         {
            new MessageCodec(null, wcfCodec, -1);
            Assert.Fail("Expected: Exception");
         }
         catch (AssertFailedException) { throw; }
         catch { }
      }

      [TestMethod]
      public void TestAllocate ()
      {
         var codec = new MessageCodec(manager, wcfCodec, 65536);
         using (var buffer1 = codec.Allocate())
         {
            Assert.IsNotNull(buffer1.Raw);
            Assert.AreEqual(buffer1.Offset, 0);
            Assert.AreEqual(buffer1.Length, 65536);
            using (var buffer2 = codec.Allocate())
            {
               Assert.IsNotNull(buffer2.Raw);
               Assert.AreEqual(buffer2.Offset, 0);
               Assert.AreEqual(buffer2.Length, 65536);
               Assert.AreNotEqual(buffer1.Raw, buffer2.Raw);
            }
         }
      }

      [TestMethod]
      public void TestEncodeDecode ()
      {
         var codec = new MessageCodec(manager, wcfCodec, 65536);
         // invalid encode
         try
         {
            codec.Encode(null);
            Assert.Fail("Expected: Exception");
         }
         catch (AssertFailedException) { throw; }
         catch { }
         try
         {
            codec.Encode(null, null);
            Assert.Fail("Expected: Exception");
         }
         catch (AssertFailedException) { throw; }
         catch { }
         try
         {
            codec.Encode(CreateMessage(1), null);
            Assert.Fail("Expected: Exception");
         }
         catch (AssertFailedException) { throw; }
         catch { }
         try
         {
            codec.Encode(null, new MemoryStream());
            Assert.Fail("Expected: Exception");
         }
         catch (AssertFailedException) { throw; }
         catch { }
         // invalid decode
         Assert.AreEqual(codec.Decode((Byte[])null), null);
         Assert.AreEqual(codec.Decode(new Byte[0]), null);
         try
         {
            codec.Decode(new Byte[1024]);
            Assert.Fail("Expected: Exception");
         }
         catch (AssertFailedException) { throw; }
         catch { }
         Assert.AreEqual(codec.Decode(default(ArraySegment<Byte>)), null);
         try
         {
            codec.Decode(new ArraySegment<Byte>(new Byte[1024]));
            Assert.Fail("Expected: Exception");
         }
         catch (AssertFailedException) { throw; }
         catch { }
         Assert.AreEqual(codec.Decode((Stream)null), null);
         Assert.AreEqual(codec.Decode(new MemoryStream()), null);
         try
         {
            var stream = new MemoryStream();
            stream.Write(new Byte[1024], 0, 1024);
            stream.Position = 0;
            codec.Decode(stream);
            Assert.Fail("Expected: Exception");
         }
         catch (AssertFailedException) { throw; }
         catch { }
         // valid buffered message
         AssertIsEqualMessage(
            CreateMessage(1), 
            codec.Decode(codec.Encode(CreateMessage(1)))
         );
         var encoded = codec.Encode(CreateMessage(1));
         AssertIsEqualMessage(
            CreateMessage(1),
            codec.Decode(encoded.Raw.Skip(encoded.Offset).Take(encoded.Length).ToArray())
         );
         // valid streamed message
         using (var stream = new MemoryStream())
         {
            codec.Encode(CreateMessage(2), stream);
            stream.Position = 0;
            AssertIsEqualMessage(CreateMessage(2), codec.Decode(stream));
         }
         // using encoded
         codec.UsingEncoded(
            CreateMessage(3),
            buffer1 =>
            {
               var buffer2 = codec.Allocate();
               Array.Copy(buffer1.Raw, buffer1.Offset, buffer2.Raw, buffer2.Offset, buffer1.Length);
               AssertIsEqualMessage(
                  CreateMessage(3),
                  codec.Decode(new ArraySegment<Byte>(buffer2.Raw, buffer2.Offset, buffer1.Length))
               );
            }
         );
      }

      private Message CreateMessage (Int32 value)
      {
         return Message.CreateMessage(
            wcfCodec.MessageVersion,
            "test",
            new Data { Value = value }
         );
      }

      private void AssertIsEqualMessage (Message message1, Message message2)
      {
         Data data1 = message1.GetBody<Data>();
         Data data2 = message2.GetBody<Data>();
         Assert.AreEqual(data1.Value, data2.Value);
      }

      [DataContract]
      public struct Data
      {
         [DataMember]
         public Int32 Value;
      }
   }
}
