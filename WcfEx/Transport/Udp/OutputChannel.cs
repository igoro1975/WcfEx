//===========================================================================
// MODULE:  OutputChannel.cs
// PURPOSE: UDP WCF communication output channel class
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

namespace WcfEx.Udp
{
   /// <summary>
   /// UDP output channel
   /// </summary>
   /// <remarks>
   /// This class represents the send side of the UDP connectionless
   /// transport for WCF. It relays all messaging operations to 
   /// the underlying socket.
   /// </remarks>
   internal sealed class OutputChannel : WcfEx.OutputChannel
   {
      UdpSocket socket;

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new channel instance
      /// </summary>
      /// <param name="manager">
      /// Containing channel listener/factory
      /// </param>
      /// <param name="codec">
      /// The channel message coder/decoder
      /// </param>
      /// <param name="remoteAddress">
      /// The output address
      /// </param>
      /// <param name="socket">
      /// The UDP channel socket
      /// </param>
      public OutputChannel (
         ChannelManagerBase manager,
         MessageCodec codec,
         EndpointAddress remoteAddress,
         UdpSocket socket) 
         : base(manager, codec, remoteAddress)
      {
         this.socket = socket;
      }
      #endregion

      #region OutputChannel Overrides
      /// <summary>
      /// Channel initialization callback
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the open operation
      /// </param>
      protected override void OnOpen (TimeSpan timeout)
      {
      }
      /// <summary>
      /// Channel graceful shutdown callback
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the close operation
      /// </param>
      protected override void OnClose (TimeSpan timeout)
      {
         this.socket.Dispose();
      }
      /// <summary>
      /// Submits a message on the channel
      /// </summary>
      /// <param name="message">
      /// The message to send
      /// </param>
      /// <param name="timeout">
      /// The timeout for the submit operation
      /// </param>
      public override void Send (Message message, TimeSpan timeout)
      {
         if (message.Headers.To == null)
            this.RemoteAddress.ApplyTo(message);
         using (ManagedBuffer buffer = this.Codec.Encode(message))
            this.socket.Send(buffer);
      }
      #endregion
   }
}
