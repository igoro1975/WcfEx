//===========================================================================
// MODULE:  Listener.cs
// PURPOSE: UDP WCF channel listener class
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
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
// Project References

namespace WcfEx.Udp
{
   /// <summary>
   /// UDP channel listener
   /// </summary>
   /// <remarks>
   /// This class provides support for creating server channels upon 
   /// connection requests from UDP clients.
   /// </remarks>
   internal sealed class Listener<TChannel> : ChannelListener<TChannel>
      where TChannel : class, IChannel
   {
      List<IChannel> channels = new List<IChannel>();
      Int32 currentChannel = -1;
      AsyncResult onClose;

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new listener instance
      /// </summary>
      /// <param name="binding">
      /// The WCF custom configuration for this listener type
      /// </param>
      /// <param name="context">
      /// The address binding configuration for this listener instance
      /// </param>
      public Listener (BindingElement binding, BindingContext context) : 
         base(context)
      {
      }
      #endregion

      #region Properties
      /// <summary>
      /// Transport binding configuration
      /// </summary>
      private new BindingElement TransportConfig
      {
         get { return (BindingElement)base.TransportConfig; }
      }
      #endregion

      #region ChannelListener Overrides
      /// <summary>
      /// Client connection wait callback
      /// </summary>
      /// <param name="timeout">
      /// The maximum amount of time to wait
      /// </param>
      /// <returns>
      /// True if a connection request was received within the timeout
      /// False otherwise
      /// </returns>
      protected override Boolean OnWaitForChannel (TimeSpan timeout)
      {
         return true;
      }
      /// <summary>
      /// Listener connection accept callback
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the accept operation
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
      protected override IAsyncResult OnBeginAcceptChannel (TimeSpan timeout, AsyncCallback callback, Object state)
      {
         if (this.State != CommunicationState.Opened)
            throw new CommunicationObjectFaultedException();
         if (this.currentChannel < this.channels.Count)
            return new SyncResult(callback, state);
         else
         {
            if (this.onClose == null)
               this.onClose = new AsyncResult(callback, state, timeout);
            return this.onClose;
         }
      }
      /// <summary>
      /// Listener connection accept callback
      /// </summary>
      /// <param name="result">
      /// The asynchronous completion token
      /// </param>
      protected override TChannel OnEndAcceptChannel (IAsyncResult result)
      {
         result.WaitFor();
         if (this.State == CommunicationState.Opened)
         {
            Int32 channelIdx = Interlocked.Increment(ref currentChannel);
            if (channelIdx < this.channels.Count)
               return this.channels[channelIdx] as TChannel;
         }
         return null;
      }
      /// <summary>
      /// Listener initialization callback
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the open operation
      /// </param>
      protected override void OnOpen (TimeSpan timeout)
      {
         foreach (EndPoint ep in UdpSocket.MapEndpoints(this.Address.Uri))
         {
            UdpSocket socket = new UdpSocket()
            {
               SendTimeout = Convert.ToInt32(this.DefaultSendTimeout.TotalMilliseconds),
               SendBufferSize = this.TransportConfig.SendBufferSize,
               ReceiveBufferSize = this.TransportConfig.ReceiveBufferSize,
               ReuseAddress = this.TransportConfig.ReuseAddress
            };
            socket.Bind(ep);
            try
            {
               if (typeof(TChannel) == typeof(IInputChannel))
                  this.channels.Add(new InputChannel(this, this.Codec, this.Address, socket));
               else if (typeof(TChannel) == typeof(IReplyChannel))
                  this.channels.Add(new ReplyChannel(this, this.Codec, this.Address, socket));
               else
                  throw new NotSupportedException();
            }
            catch
            {
               socket.Dispose();
               throw;
            }
         }
      }
      /// <summary>
      /// Listener graceful shutdown callback
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the close operation
      /// </param>
      protected override void OnClose (TimeSpan timeout)
      {
         foreach (IChannel channel in this.channels)
            channel.Close();
         this.channels.Clear();
         this.currentChannel = -1;
         if (this.onClose != null)
            this.onClose.Complete(null);
         this.onClose = null;
      }
      #endregion
   }
}
