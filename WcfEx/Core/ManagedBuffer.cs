//===========================================================================
// MODULE:  Buffer.cs
// PURPOSE: WCF managed message buffer shim
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
using System.ServiceModel;
using System.ServiceModel.Channels;
// Project References

namespace WcfEx
{
   /// <summary>
   /// Message buffer shim class
   /// </summary>
   /// <remarks>
   /// This shim automatically releases a managed 
   /// buffer upon disposal.
   /// </remarks>
   public struct ManagedBuffer : IDisposable
   {
      BufferManager manager;
      ArraySegment<Byte> data;

      /// <summary>
      /// Initializes a new buffer instance
      /// </summary>
      /// <param name="manager">
      /// The manager for this buffer
      /// </param>
      /// <param name="size">
      /// The size of the buffer to allocate, in bytes
      /// </param>
      public ManagedBuffer (BufferManager manager, Int32 size)
      {
         if (manager == null)
            throw new ArgumentNullException("manager");
         if (size <= 0)
            throw new ArgumentException("data");
         this.manager = manager;
         this.data = new ArraySegment<Byte>(manager.TakeBuffer(size));
      }
      /// <summary>
      /// Initializes a new buffer instance
      /// </summary>
      /// <param name="manager">
      /// The manager for this buffer
      /// </param>
      /// <param name="data">
      /// The buffer to attach
      /// </param>
      public ManagedBuffer (BufferManager manager, ArraySegment<Byte> data)
      {
         if (manager == null)
            throw new ArgumentNullException("manager");
         if (data.Array == null || data.Count <= 0)
            throw new ArgumentException("data");
         this.manager = manager;
         this.data = data;
      }
      /// <summary>
      /// Releases the attached buffer
      /// </summary>
      public void Dispose ()
      {
         if (this.manager != null)
         {
            this.manager.ReturnBuffer(this.data.Array);
            this.manager = null;
            this.data = default(ArraySegment<Byte>);
         }
      }
      /// <summary>
      /// The data segment of the buffer
      /// </summary>
      public ArraySegment<Byte> Data
      {
         get { return this.data; }
      }
      /// <summary>
      /// The raw byte array of the segment
      /// </summary>
      public Byte[] Raw
      {
         get { return this.data.Array; }
      }
      /// <summary>
      /// The data offset of the buffer
      /// </summary>
      public Int32 Offset
      {
         get { return this.data.Offset; }
      }
      /// <summary>
      /// The data length of the buffer
      /// </summary>
      public Int32 Length
      {
         get { return this.data.Count; }
      }
      /// <summary>
      /// Converts to an array segment
      /// </summary>
      /// <param name="b">
      /// The buffer to convert
      /// </param>
      /// <returns>
      /// The converted array segment
      /// </returns>
      public static implicit operator ArraySegment<Byte> (ManagedBuffer b)
      {
         return b.Data;
      }
   }
}
