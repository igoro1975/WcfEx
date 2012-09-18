//===========================================================================
// MODULE:  MessageCodec.cs
// PURPOSE: WCF message codec wrapper
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
using System.ServiceModel;
using System.ServiceModel.Channels;
// Project References

namespace WcfEx
{
   /// <summary>
   /// WCF message encoding/decoding/buffer wrapper
   /// </summary>
   /// <remarks>
   /// This class encapsulates the tedious and error-prone
   /// process of managing message buffers and the WCF
   /// buffer manager when encoding/decoding messages via
   /// a MessageEncoder.
   /// </remarks>
   public sealed class MessageCodec
   {
      BufferManager manager;
      MessageEncoder codec;
      Int32 maxMessageSize;

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new codec instance
      /// </summary>
      /// <param name="manager">
      /// The WCF message buffer manager
      /// </param>
      /// <param name="codec">
      /// The channel message encoder
      /// </param>
      /// <param name="maxMessageSize">
      /// The maximum length of any message on the channel
      /// </param>
      public MessageCodec (
         BufferManager manager, 
         MessageEncoder codec,
         Int32 maxMessageSize)
      {
         if (manager == null)
            throw new ArgumentNullException("manager");
         if (codec == null)
            throw new ArgumentNullException("codec");
         if (maxMessageSize <= 0)
            throw new ArgumentException("maxMessageSize");
         this.manager = manager;
         this.codec = codec;
         this.maxMessageSize = maxMessageSize;
      }
      #endregion

      #region Operations
      /// <summary>
      /// Allocates a new buffer large enough to hold
      /// a maximum-length message from the attached
      /// buffer manager
      /// </summary>
      /// <returns>
      /// The allocated buffer
      /// </returns>
      public ManagedBuffer Allocate ()
      {
         return new ManagedBuffer(this.manager, this.maxMessageSize);
      }
      /// <summary>
      /// Encodes a WCF message to a buffer
      /// </summary>
      /// <param name="message">
      /// The message to encode
      /// </param>
      /// <returns>
      /// The encoded buffer
      /// The caller is responsible for disposing of this buffer
      /// </returns>
      public ManagedBuffer Encode (Message message)
      {
         return new ManagedBuffer(
            this.manager,
            this.codec.WriteMessage(message, this.maxMessageSize, this.manager)
         );
      }
      /// <summary>
      /// Encodes a WCF message to a stream
      /// </summary>
      /// <param name="message">
      /// The message to encode
      /// </param>
      /// <param name="stream">
      /// The stream to write
      /// </param>
      public void Encode (Message message, Stream stream)
      {
         this.codec.WriteMessage(message, stream);
      }
      /// <summary>
      /// Executes a callback with an encoded message,
      /// ensuring that the message buffer is freed
      /// </summary>
      /// <param name="message">
      /// The message to encode
      /// </param>
      /// <param name="callback">
      /// The callback to execute on the encoded message
      /// </param>
      public void UsingEncoded (Message message, Action<ManagedBuffer> callback)
      {
         using (ManagedBuffer b = Encode(message))
            callback(b);
      }
      /// <summary>
      /// Decodes a WCF message from a buffer
      /// </summary>
      /// <param name="buffer">
      /// The buffer to read
      /// </param>
      /// <returns>
      /// The decoded message
      /// </returns>
      public Message Decode (Byte[] buffer)
      {
         if (buffer == null || buffer.Length == 0)
            return null;
         return Decode(new ArraySegment<Byte>(buffer));
      }
      /// <summary>
      /// Decodes a WCF message from a buffer
      /// </summary>
      /// <param name="buffer">
      /// The buffer to read
      /// </param>
      /// <returns>
      /// The decoded message
      /// </returns>
      public Message Decode (ArraySegment<Byte> buffer)
      {
         // decode the buffer via the encoder
         if (buffer.Count > 0)
            return this.codec.ReadMessage(buffer, this.manager);
         // the caller assumes that the decoder will return
         // the input buffer to the buffer manager, so we
         // must do so here if the buffer segment is empty
         if (buffer.Array != null)
            this.manager.ReturnBuffer(buffer.Array);
         return null;
      }
      /// <summary>
      /// Decodes a WCF message from a stream
      /// </summary>
      /// <param name="stream">
      /// The stream to read
      /// </param>
      /// <returns>
      /// The decoded message
      /// </returns>
      public Message Decode (Stream stream)
      {
         if (stream == null)
            return null;
         if (stream.CanSeek && stream.Length - stream.Position == 0)
            return null;
         return this.codec.ReadMessage(stream, this.maxMessageSize);
      }
      #endregion
   }
}
