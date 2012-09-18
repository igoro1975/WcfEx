//===========================================================================
// MODULE:  InputChannel.cs
// PURPOSE: UDP WCF communication input channel class
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
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
// Project References

namespace WcfEx.Udp
{
   /// <summary>
   /// UDP input channel
   /// </summary>
   /// <remarks>
   /// This class represents the receive side of the UDP connectionless
   /// transport for WCF. It relays all messaging operations to 
   /// the underlying socket.
   /// </remarks>
   internal sealed class InputChannel : WcfEx.InputChannel
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
      /// The message encoder/decoder for this channel
      /// </param>
      /// <param name="localAddress">
      /// The input address
      /// </param>
      /// <param name="socket">
      /// The datagram socket for this channel
      /// </param>
      public InputChannel (
         ChannelManagerBase manager,
         MessageCodec codec,
         EndpointAddress localAddress,
         UdpSocket socket) 
         : base(manager, codec, localAddress)
      {
         this.socket = socket;
      }
      #endregion

      #region InputChannel Overrides
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
      /// Receives a message from the other end of the channel
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the receive operation
      /// </param>
      /// <param name="callback">
      /// Asynchronous completion delegate
      /// </param>
      /// <param name="state">
      /// Asynchronous completion delegate parameter
      /// </param>
      /// <returns>
      /// The asynchronous completion token
      /// </returns>
      public override IAsyncResult BeginTryReceive (TimeSpan timeout, AsyncCallback callback, Object state)
      {
         IAsyncResult result = null;
         ManagedBuffer buffer = this.Codec.Allocate();
         try
         {
            result = this.socket.BeginReceive(buffer, callback, state);
         }
         catch (ObjectDisposedException)
         {
            // if the socket was disposed, then the other side of the 
            // channel closed, so close this side
            // we must throw here to force the WCF dispatcher to shut down
            // the channel; otherwise, it will continue to call us
            if (base.State == CommunicationState.Opened)
               base.Close();
            throw new CommunicationObjectFaultedException();
         }
         finally
         {
            if (result == null)
               buffer.Dispose();
         }
         return result;
      }
      /// <summary>
      /// Completes a message receive operation
      /// </summary>
      /// <param name="result">
      /// The asynchronous completion token
      /// </param>
      /// <param name="message">
      /// Return the received message via here
      /// </param>
      /// <returns>
      /// True if a message was received within the timeout
      /// False otherwise
      /// </returns>
      public override Boolean EndTryReceive (IAsyncResult result, out Message message)
      {
         EndPoint ep;
         message = this.Codec.Decode(this.socket.EndReceive(result, out ep));
         return (message != null);
      }
      /// <summary>
      /// Waits for a message on the channel
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the wait operation
      /// </param>
      /// <returns>
      /// True if a message is available on the channel
      /// False otherwise
      /// </returns>
      public override Boolean WaitForMessage (TimeSpan timeout)
      {
         return this.socket.Poll(timeout);
      }
      #endregion
   }
}
