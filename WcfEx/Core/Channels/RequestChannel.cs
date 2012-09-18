//===========================================================================
// MODULE:  RequestChannel.cs
// PURPOSE: request channel base class
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
   /// Request channel base class
   /// </summary>
   /// <remarks>
   /// This class provides a resonable default implementation of the
   /// tedious channel methods for request channels. it encapsulates
   /// the request side of a request/response communication channel.
   /// For flexibility, this class implements both the sync and async
   /// versions of all channel/interface methods automatically in terms
   /// of the other (sync calls async, async calls sync), allowing the 
   /// derived class to either implement the sync or async version and get
   /// the other for free. However, this means that the derived class MUST 
   /// implement either the sync or async versions to avoid a stack overflow.
   /// Note that the BeginRequest/EndRequest methods are not virtual,
   /// because WCF never calls them - it always calls the synchronous
   /// Request method.
   /// </remarks>
   public abstract class RequestChannel : Channel, IRequestChannel
   {
      private EndpointAddress remoteAddress;

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
      /// The address of the remote endpoint
      /// </param>
      public RequestChannel (
         ChannelManagerBase manager,
         MessageCodec codec,
         EndpointAddress remoteAddress) 
         : base(manager, codec)
      {
         this.remoteAddress = remoteAddress;
      }
      #endregion

      #region IRequestChannel Implementation
      /// <summary>
      /// The address of the remote endpoint
      /// </summary>
      public EndpointAddress RemoteAddress
      {
         get { return this.remoteAddress; }
      }
      /// <summary>
      /// The address of the remote endpoint
      /// </summary>
      public Uri Via
      {
         get { return this.remoteAddress.Uri; }
      }
      /// <summary>
      /// Submits a request on the channel
      /// </summary>
      /// <param name="request">
      /// The request message to send
      /// </param>
      /// <returns>
      /// The request response
      /// </returns>
      public Message Request (Message request)
      {
         return Request(request, base.DefaultReceiveTimeout);
      }
      /// <summary>
      /// Submits a request on the channel
      /// </summary>
      /// <param name="request">
      /// The request message to send
      /// </param>
      /// <param name="timeout">
      /// The timeout for the submit operation
      /// </param>
      /// <returns>
      /// The request response
      /// </returns>
      public virtual Message Request (Message request, TimeSpan timeout)
      {
         return EndRequest(BeginRequest(request, timeout, null, null));
      }
      /// <summary>
      /// Submits a request on the channel
      /// </summary>
      /// <param name="request">
      /// The request message to send
      /// </param>
      /// <param name="callback">
      /// Asynchronous completion delegate
      /// </param>
      /// <param name="state">
      /// Asynchronous completion delegate parameter
      /// </param>
      public IAsyncResult BeginRequest (
         Message request, 
         AsyncCallback callback, 
         Object state)
      {
         return BeginRequest(request, base.DefaultReceiveTimeout, callback, state);
      }
      /// <summary>
      /// Submits a request on the channel
      /// </summary>
      /// <param name="request">
      /// The request message to submit
      /// </param>
      /// <param name="timeout">
      /// The timeout for the submit operation
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
      public virtual IAsyncResult BeginRequest (
         Message request, 
         TimeSpan timeout, 
         AsyncCallback callback, 
         Object state)
      {
         Message response = Request(request, timeout);
         return new SyncResult(callback, state, response);
      }
      /// <summary>
      /// Completes a request submission operation
      /// </summary>
      /// <param name="result">
      /// The asynchronous completion token
      /// </param>
      /// <returns>
      /// The request response
      /// </returns>
      public virtual Message EndRequest (IAsyncResult result)
      {
         return ((SyncResult)result).GetResult<Message>();
      }
      #endregion
   }
}
