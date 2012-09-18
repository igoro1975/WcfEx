//===========================================================================
// MODULE:  ChannelFactory.cs
// PURPOSE: channel factory base class
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

namespace WcfEx
{
   /// <summary>
   /// Channel factory base
   /// </summary>
   /// <remarks>
   /// This class provides a resonable default implementation of the
   /// tedious factory methods for channels.
   /// </remarks>
   /// <typeparam name="TChannel">
   /// The WCF contract interface type
   /// </typeparam>
   public abstract class ChannelFactory<TChannel> : ChannelFactoryBase<TChannel>
      where TChannel : class, IChannel
   {
      private BindingContext context;

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new listener instance
      /// </summary>
      /// <param name="context">
      /// The listener binding context
      /// </param>
      public ChannelFactory (BindingContext context) : base(context.Binding)
      {
         this.context = context;
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
      /// The message encoder/decoder
      /// </summary>
      protected MessageCodec Codec
      {
         get;
         private set;
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

      #region ChannelFactoryBase Overrides
      /// <summary>
      /// Factory initialization callback
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the open operation
      /// </param>
      protected override void OnOpen (TimeSpan timeout)
      {
         OnEndOpen(OnBeginOpen(timeout, null, null));
      }
      /// <summary>
      /// Factory initialization callback
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
      /// <returns>
      /// Asynchronous completion token
      /// </returns>
      protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, Object state)
      {
         OnOpen(timeout);
         return new SyncResult(callback, state);
      }
      /// <summary>
      /// Factory initialization callback
      /// </summary>
      /// <param name="result">
      /// The asynchronous result to complete
      /// </param>
      protected override void OnEndOpen (IAsyncResult result)
      {
         result.WaitFor();
      }
      /// <summary>
      /// Factory graceful shutdown callback
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the close operation
      /// </param>
      protected override void OnClose (TimeSpan timeout)
      {
         OnEndClose(OnBeginClose(timeout, null, null));
      }
      /// <summary>
      /// Factory graceful shutdown callback
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
      /// <returns>
      /// Asynchronous completion token
      /// </returns>
      protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, Object state)
      {
         OnClose(timeout);
         return new SyncResult(callback, state);
      }
      /// <summary>
      /// Factory graceful shutdown callback
      /// </summary>
      /// <param name="result">
      /// The asynchronous result to complete
      /// </param>
      protected override void OnEndClose (IAsyncResult result)
      {
         result.WaitFor();
      }
      /// <summary>
      /// Factory unexpected shutdown callback
      /// </summary>
      protected override void OnAbort ()
      {
         OnClose(TimeSpan.FromMilliseconds(0));
      }
      #endregion
   }
}
