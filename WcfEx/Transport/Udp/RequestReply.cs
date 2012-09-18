//===========================================================================
// MODULE:  RequestReply.cs
// PURPOSE: UDP WCF communication request/reply context
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
using System.ServiceModel.Channels;
// Project References

namespace WcfEx.Udp
{
   /// <summary>
   /// UDP request/reply context
   /// </summary>
   /// <remarks>
   /// This class encapsulates the correlation of WCF requests to replies
   /// on the server side. It relays all messaging operations to the 
   /// underlying socket.
   /// </remarks>
   internal sealed class RequestReply : WcfEx.RequestReply
   {
      UdpSocket socket;
      EndPoint clientEndpoint;

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new request context instance
      /// </summary>
      /// <param name="request">
      /// The original request message
      /// </param>
      /// <param name="codec">
      /// The channel message coder/decoder
      /// </param>
      /// <param name="socket">
      /// The datagram socket for this channel
      /// </param>
      /// <param name="replyEP">
      /// The address/port of the client endpoint to send to
      /// </param>
      public RequestReply (
         Message request,
         MessageCodec codec,
         UdpSocket socket,
         EndPoint replyEP)
         : base(request, codec)
      {
         this.socket = socket;
         this.clientEndpoint = replyEP;
      }
      #endregion

      #region RequestReply Overrides
      /// <summary>
      /// Request context graceful shutdown callback
      /// </summary>
      /// <param name="timespan">
      /// The timeout for the close operation
      /// </param>
      public override void Close (TimeSpan timespan)
      {
      }
      /// <summary>
      /// Submits a reply message back to the client
      /// </summary>
      /// <param name="reply">
      /// The reply message to send
      /// </param>
      /// <param name="timeout">
      /// The timeout for the submit operation
      /// </param>
      public override void Reply (Message reply, TimeSpan timeout)
      {
         if (reply != null)
         {
            reply.Headers.MessageId = this.RequestMessage.Headers.MessageId;
            using (ManagedBuffer buffer = this.Codec.Encode(reply))
               this.socket.Send(this.clientEndpoint, buffer);
         }
      }
      #endregion
   }
}
