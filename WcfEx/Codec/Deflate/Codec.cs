//===========================================================================
// MODULE:  Codec.cs
// PURPOSE: Deflate WCF message encoder/decoder
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
using System.IO.Compression;
using System.ServiceModel.Channels;
// Project References

namespace WcfEx.Deflate
{
   /// <summary>
   /// Deflate message codec
   /// </summary>
   /// <remarks>
   /// This class implements a custom WCF message encoder
   /// via the System.IO.Compression.DeflateStream class.
   /// </remarks>
   internal sealed class Codec : MessageEncoder
   {
      private MessageEncoder baseEncoder;

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new codec instance
      /// </summary>
      /// <param name="baseEncoder">
      /// The base transport encoder
      /// </param>
      public Codec(MessageEncoder baseEncoder)
      {
         this.baseEncoder = baseEncoder;
      }
      #endregion

      #region MessageEncoder Overrides
      /// <summary>
      /// The deflate MIME type
      /// </summary>
      public override String ContentType
      {
         get { return "application/x-deflate"; }
      }
      /// <summary>
      /// The deflate MIME type
      /// </summary>
      public override String MediaType
      {
         get { return this.ContentType; }
      }
      /// <summary>
      /// The current encoded message version
      /// </summary>
      public override MessageVersion MessageVersion
      {
         get { return baseEncoder.MessageVersion; }
      }
      /// <summary>
      /// Decodes a WCF buffered message
      /// </summary>
      /// <param name="buffer">
      /// The message buffer/offset
      /// </param>
      /// <param name="bufferManager">
      /// The WCF buffer manager for this channel
      /// </param>
      /// <param name="contentType">
      /// The underlying message content type
      /// </param>
      /// <returns>
      /// The decoded WCF message
      /// </returns>
      public override Message ReadMessage(
         ArraySegment<Byte> buffer, 
         BufferManager bufferManager, 
         String contentType)
      {
         try
         {
            return ReadMessage(
               new MemoryStream(buffer.Array, buffer.Offset, buffer.Count, false),
               65536,
               contentType
            );
         }
         finally
         {
            // the streamed read does not access the buffer,
            // so we must always return it to the manager
            bufferManager.ReturnBuffer(buffer.Array);
         }
      }
      /// <summary>
      /// Decodes a WCF streamed message
      /// </summary>
      /// <param name="stream">
      /// The channel stream
      /// </param>
      /// <param name="maxSizeOfHeaders">
      /// The maximum size of the headers that can be read from the message
      /// </param>
      /// <param name="contentType">
      /// The underlying message content type
      /// </param>
      /// <returns>
      /// The decoded WCF message
      /// </returns>
      public override Message ReadMessage (
         Stream stream, 
         Int32 maxSizeOfHeaders, 
         String contentType)
      {
         return baseEncoder.ReadMessage(
            new DeflateStream(stream, CompressionMode.Decompress), 
            maxSizeOfHeaders
         );
      }
      /// <summary>
      /// Encodes a WCF message to a buffer
      /// </summary>
      /// <param name="message">
      /// The message to encode
      /// </param>
      /// <param name="maxLength">
      /// The maximum message size that can be written
      /// </param>
      /// <param name="bufferManager">
      /// The WCF buffer manager for the current channel
      /// </param>
      /// <param name="offset">
      /// The offset of the segment that begins from the 
      /// start of the byte array that provides the buffer
      /// </param>
      /// <returns>
      /// The encoded WCF message buffer
      /// </returns>
      public override ArraySegment<Byte> WriteMessage(
         Message message, 
         Int32 maxLength, 
         BufferManager bufferManager, 
         Int32 offset)
      {
         // compress the message to an in-memory stream
         using (MemoryStream stream = new MemoryStream())
         {
            WriteMessage(message, stream);
            // transfer the stream buffer to the message buffer
            Int32  length = (Int32)stream.Length;
            Byte[] buffer = bufferManager.TakeBuffer(offset + length);
            Array.Copy(stream.GetBuffer(), 0, buffer, offset, length);
            return new ArraySegment<Byte>(buffer, offset, length);
         }
      }
      /// <summary>
      /// Encodes a WCF message to a stream
      /// </summary>
      /// <param name="message">
      /// The message to encode
      /// </param>
      /// <param name="stream">
      /// The WCF stream to write
      /// </param>
      public override void WriteMessage(Message message, Stream stream)
      {
         using (DeflateStream gz = new DeflateStream(stream, CompressionMode.Compress, true))
            baseEncoder.WriteMessage(message, gz);
         stream.Flush();
      }
      #endregion
   }
}
