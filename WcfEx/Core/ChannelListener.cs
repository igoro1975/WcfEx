//===========================================================================
// MODULE:  ChannelListener.cs
// PURPOSE: channel listener base class
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
   /// Channel listener base
   /// </summary>
   /// <remarks>
   /// This class provides a resonable default implementation of the
   /// tedious listener methods for channels.
   /// </remarks>
   /// <typeparam name="TChannel">
   /// The WCF contract interface type
   /// </typeparam>
   public abstract class ChannelListener<TChannel> : ChannelListenerBase<TChannel>
      where TChannel : class, IChannel
   {
      private BindingContext context;
      private EndpointAddress address;

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new listener instance
      /// </summary>
      /// <param name="context">
      /// The listener binding context
      /// </param>
      public ChannelListener (BindingContext context) : base(context.Binding)
      {
         this.context = context;
         this.address = new EndpointAddress(
            new Uri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress),
            new AddressHeader[0]
         );
         MessageEncodingBindingElement mebe = context.Binding.Elements
            .Find<MessageEncodingBindingElement>();
         TransportBindingElement txbe = context.Binding.Elements
            .Find<TransportBindingElement>();
         if (mebe == null)
            context.Binding.Elements.Add(mebe = new BinaryMessageEncodingBindingElement());
         this.Codec = new MessageCodec(
            BufferManager.CreateBufferManager(
               (Int32)txbe.MaxBufferPoolSize,
               (Int32)txbe.MaxReceivedMessageSize
            ),
            mebe.CreateMessageEncoderFactory().Encoder,
            (Int32)txbe.MaxReceivedMessageSize
         );
      }
      #endregion

      #region Properties
      /// <summary>
      /// The listening address
      /// </summary>
      public EndpointAddress Address 
      { 
         get { return this.address; } 
      }
      /// <summary>
      /// The message encoder/decoder
      /// </summary>
      protected MessageCodec Codec
      { 
         get; private set; 
      }
      /// <summary>
      /// Transport binding configuration
      /// </summary>
      protected TransportBindingElement TransportConfig
      {
         get
         {
            return context.Binding.Elements
               .Find<TransportBindingElement>();
         }
      }
      /// <summary>
      /// Encoding binding configuration
      /// </summary>
      protected MessageEncodingBindingElement CodecConfig
      {
         get
         {
            return context.Binding.Elements
               .Find<MessageEncodingBindingElement>();
         }
      }
      #endregion

      #region ChannelListenerBase Overrides
      /// <summary>
      /// The listeining address URI
      /// </summary>
      public override Uri Uri
      {
         get { return this.address.Uri; }
      }
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
         return OnEndWaitForChannel(OnBeginWaitForChannel(timeout, null, null));
      }
      /// <summary>
      /// Listener connection wait callback
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the wait operation
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
      protected override IAsyncResult OnBeginWaitForChannel (TimeSpan timeout, AsyncCallback callback, Object state)
      {
         Boolean result = OnWaitForChannel(timeout);
         return new SyncResult(callback, state, result);
      }
      /// <summary>
      /// Listener connection wait callback
      /// </summary>
      /// <param name="result">
      /// The asynchronous completion token
      /// </param>
      /// <returns>
      /// True if a connection is waiting on the listener
      /// False otherwise
      /// </returns>
      protected override Boolean OnEndWaitForChannel (IAsyncResult result)
      {
         return ((SyncResult)result).GetResult<Boolean>();
      }
      /// <summary>
      /// Client connection accept callback
      /// </summary>
      /// <param name="timeout">
      /// The maximum amount of time to wait
      /// </param>
      /// <returns>
      /// The new connected server channel if a connection occurred within the timeout
      /// Null otherwise
      /// </returns>
      protected override TChannel OnAcceptChannel (TimeSpan timeout)
      {
         return OnEndAcceptChannel(OnBeginAcceptChannel(timeout, null, null));
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
         TChannel result = OnAcceptChannel(timeout);
         return new SyncResult(callback, state, result);
      }
      /// <summary>
      /// Listener connection accept callback
      /// </summary>
      /// <param name="result">
      /// The asynchronous completion token
      /// </param>
      protected override TChannel OnEndAcceptChannel (IAsyncResult result)
      {
         return ((SyncResult)result).GetResult<TChannel>();
      }
      /// <summary>
      /// Listener initialization callback
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the open operation
      /// </param>
      protected override void OnOpen (TimeSpan timeout)
      {
         OnEndOpen(OnBeginOpen(timeout, null, null));
      }
      /// <summary>
      /// Listener initialization callback
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the open operation
      /// </param>
      /// <param name="callback">
      /// Asynchronous completion delegate
      /// </param>
      /// <param name="state">
      /// Asynchronous completion delegate parameter
      /// </param>
      protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, Object state)
      {
         OnOpen(timeout);
         return new SyncResult(callback, state);
      }
      /// <summary>
      /// Listener initialization callback
      /// </summary>
      /// <param name="result">
      /// The asynchronous result to complete
      /// </param>
      protected override void OnEndOpen (IAsyncResult result)
      {
         result.WaitFor();
      }
      /// <summary>
      /// Listener graceful shutdown callback
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the close operation
      /// </param>
      protected override void OnClose (TimeSpan timeout)
      {
         OnEndClose(OnBeginClose(timeout, null, null));
      }
      /// <summary>
      /// Listener graceful termination callback
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the close operation
      /// </param>
      /// <param name="callback">
      /// Asynchronous completion delegate
      /// </param>
      /// <param name="state">
      /// Asynchronous completion delegate parameter
      /// </param>
      protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, Object state)
      {
         OnClose(timeout);
         return new SyncResult(callback, state);
      }
      /// <summary>
      /// Listener graceful termination callback
      /// </summary>
      /// <param name="result">
      /// The asynchronous result to complete
      /// </param>
      protected override void OnEndClose (IAsyncResult result)
      {
         result.WaitFor();
      }
      /// <summary>
      /// Listener unexpected shutdown callback
      /// </summary>
      protected override void OnAbort ()
      {
         OnClose(TimeSpan.FromMilliseconds(0));
      }
      #endregion
   }
}
