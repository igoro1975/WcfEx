//===========================================================================
// MODULE:  Factory.cs
// PURPOSE: GZip WCF message encoder/decoder factory
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
using System.ServiceModel.Channels;
// Project References

namespace WcfEx.GZip
{
   /// <summary>
   /// GZip codec factory
   /// </summary>
   /// <remarks>
   /// This class creates and initializes GZip
   /// codec instances based on an underlying
   /// message codec.
   /// </remarks>
   internal sealed class Factory : MessageEncoderFactory
   {
      MessageEncoderFactory baseFactory;

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new factory instance
      /// </summary>
      /// <param name="baseFactory">
      /// The base encoder factory for the channel
      /// </param>
      public Factory(MessageEncoderFactory baseFactory)
      {
         this.baseFactory = baseFactory;
      }
      #endregion

      #region MessageEncoderFactory Overrides
      /// <summary>
      /// Creates and initializes a new codec instance
      /// </summary>
      public override MessageEncoder Encoder
      {
         get { return new Codec(this.baseFactory.Encoder); }
      }
      /// <summary>
      /// The current encoded message version
      /// </summary>
      public override MessageVersion MessageVersion
      {
         get { return this.baseFactory.MessageVersion; }
      }
      #endregion
   }
}
