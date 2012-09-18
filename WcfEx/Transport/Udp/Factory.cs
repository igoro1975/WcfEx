//===========================================================================
// MODULE:  Factory.cs
// PURPOSE: UDP WCF channel factory class
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
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
// Project References

namespace WcfEx.Udp
{
   /// <summary>
   /// UDP channel factory
   /// </summary>
   /// <remarks>
   /// This class provides support for creating client
   /// channels for submitting requests to services
   /// over UDP.
   /// </remarks>
   internal sealed class Factory<TChannel> : ChannelFactory<TChannel>
      where TChannel : class, IChannel
   {
      #region Construction/Disposal
      /// <summary>
      /// Initializes a new factory instance
      /// </summary>
      /// <param name="binding">
      /// The UDP binding element
      /// </param>
      /// <param name="context">
      /// The requested WCF binding context
      /// </param>
      public Factory (BindingElement element, BindingContext context) : base(context)
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

      #region ChannelFactory Overrides
      /// <summary>
      /// Factory initialization callback
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the open operation
      /// </param>
      protected override void OnOpen (TimeSpan timeout)
      {
      }
      /// <summary>
      /// Factory graceful shutdown callback
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the close operation
      /// </param>
      protected override void OnClose (TimeSpan timeout)
      {
      }
      /// <summary>
      /// Channel creation callback
      /// </summary>
      /// <param name="address">
      /// The endpoint address to connect
      /// </param>
      /// <param name="via">
      /// The transport address to connect
      /// </param>
      /// <returns>
      /// The connected client channel
      /// </returns>
      protected override TChannel OnCreateChannel (EndpointAddress address, Uri via)
      {
         if (via != null && via != address.Uri)
            throw new ArgumentException("via");
         UdpSocket socket = new UdpSocket()
         {
            SendTimeout = Convert.ToInt32(this.DefaultSendTimeout.TotalMilliseconds),
            SendBufferSize = this.TransportConfig.SendBufferSize,
            ReceiveBufferSize = this.TransportConfig.ReceiveBufferSize
         };
         socket.Connect(UdpSocket.MapEndpoints(address.Uri).First());
         try
         {
            if (typeof(TChannel) == typeof(IOutputChannel))
               return new OutputChannel(this, this.Codec, address, socket) as TChannel;
            else if (typeof(TChannel) == typeof(IRequestChannel))
               return new RequestChannel(this, this.Codec, address, socket) as TChannel;
            else
               throw new NotSupportedException();
         }
         catch
         {
            socket.Dispose();
            throw;
         }
      }
      #endregion
   }
}
