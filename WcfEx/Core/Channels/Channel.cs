//===========================================================================
// MODULE:  Channel.cs
// PURPOSE: channel base class
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
   /// Channel base class
   /// </summary>
   /// <remarks>
   /// This class provides a resonable default implementation of the
   /// tedious base channel methods for all channels.
   /// For flexibility, this class implements both the sync and async
   /// versions of all channel/interface methods automatically in terms
   /// of the other (sync calls async, async calls sync), allowing the 
   /// derived class to either implement the sync or async version and get
   /// the other for free. However, this means that the derived class MUST 
   /// implement either the sync or async versions to avoid a stack overflow.
   /// </remarks>
   public abstract class Channel : ChannelBase, IChannel
   {
      private MessageCodec codec;

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
      public Channel (
         ChannelManagerBase manager,
         MessageCodec codec) 
         : base(manager)
      {
         this.codec = codec;
      }
      #endregion

      #region Properties
      /// <summary>
      /// The message encoder/decoder
      /// </summary>
      protected MessageCodec Codec
      {
         get { return this.codec; }
      }
      #endregion

      #region ChannelBase Overrides
      /// <summary>
      /// Channel initialization callback
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the open operation
      /// </param>
      protected override void OnOpen (TimeSpan timeout)
      {
         OnEndOpen(OnBeginOpen(timeout, null, null));
      }
      /// <summary>
      /// Channel initialization callback
      /// </summary>
      /// <param name="timeout">
      /// The initialization timeout
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
      protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, Object state)
      {
         OnOpen(timeout);
         return new SyncResult(callback, state);
      }
      /// <summary>
      /// Channel initialization callback
      /// </summary>
      /// <param name="result">
      /// The asynchronous completion token
      /// </param>
      protected override void OnEndOpen (IAsyncResult result)
      {
         result.WaitFor();
      }
      /// <summary>
      /// Channel graceful shutdown callback
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the close operation
      /// </param>
      protected override void OnClose (TimeSpan timeout)
      {
         OnEndClose(OnBeginClose(timeout, null, null));
      }
      /// <summary>
      /// Channel graceful shutdown callback
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
      /// The asynchronous completion token
      /// </returns>
      protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, Object state)
      {
         OnClose(timeout);
         return new SyncResult(callback, state);
      }
      /// <summary>
      /// Channel graceful shutdown callback
      /// </summary>
      /// <param name="result">
      /// The asynchronous completion token
      /// </param>
      protected override void OnEndClose (IAsyncResult result)
      {
         result.WaitFor();
      }
      /// <summary>
      /// Channel unexpected shutdown callback
      /// </summary>
      protected override void OnAbort ()
      {
         OnClose(TimeSpan.FromMilliseconds(0));
      }
      #endregion
   }
}
