//===========================================================================
// MODULE:  ReplyChannel.cs
// PURPOSE: reply channel base class
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
   /// Reply channel base class
   /// </summary>
   /// <remarks>
   /// This class provides a resonable default implementation of the
   /// tedious channel methods for reply channels. it encapsulates
   /// the request/response state on the server side of a request/response 
   /// communication channel.
   /// For flexibility, this class implements both the sync and async
   /// versions of all channel/interface methods automatically in terms
   /// of the other (sync calls async, async calls sync), allowing the 
   /// derived class to either implement the sync or async version and get
   /// the other for free. However, this means that the derived class MUST 
   /// implement either the sync or async versions to avoid a stack overflow.
   /// </remarks>
   public abstract class ReplyChannel : Channel, IReplyChannel
   {
      private EndpointAddress localAddress;

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
      /// <param name="localAddress">
      /// The address of the local endpoint
      /// </param>
      public ReplyChannel (
         ChannelManagerBase manager,
         MessageCodec codec,
         EndpointAddress localAddress) 
         : base(manager, codec)
      {
         this.localAddress = localAddress;
      }
      #endregion

      #region IReplyChannel Implementation
      /// <summary>
      /// The address of the local endpoint
      /// </summary>
      public EndpointAddress LocalAddress
      {
         get { return this.localAddress; }
      }
      /// <summary>
      /// Waits for a request on the channel
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the wait operation
      /// </param>
      /// <param name="request">
      /// Return the received request via here
      /// </param>
      /// <returns>
      /// True if a request was received within the timeout
      /// False otherwise
      /// </returns>
      public virtual Boolean TryReceiveRequest (TimeSpan timeout, out RequestContext request)
      {
         return EndTryReceiveRequest(BeginTryReceiveRequest(timeout, null, null), out request);
      }
      /// <summary>
      /// Receives a request from the other end of the channel
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
      public virtual IAsyncResult BeginTryReceiveRequest (TimeSpan timeout, AsyncCallback callback, Object state)
      {
         RequestContext request;
         TryReceiveRequest(timeout, out request);
         return new SyncResult(callback, state, request);
      }
      /// <summary>
      /// Completes a request receive operation
      /// </summary>
      /// <param name="result">
      /// The asynchronous completion token
      /// </param>
      /// <param name="request">
      /// Return the received request via here
      /// </param>
      /// <returns>
      /// True if a request was received within the timeout
      /// False otherwise
      /// </returns>
      public virtual Boolean EndTryReceiveRequest (IAsyncResult result, out RequestContext request)
      {
         request = ((SyncResult)result).GetResult<RequestContext>();
         return (request != null);
      }
      /// <summary>
      /// Receives a request on the channel
      /// </summary>
      /// <returns>
      /// The request received
      /// </returns>
      public RequestContext ReceiveRequest ()
      {
         return ReceiveRequest(base.DefaultReceiveTimeout);
      }
      /// <summary>
      /// Receives a request on the channel
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the receive operation
      /// </param>
      /// <returns>
      /// The request received
      /// </returns>
      public RequestContext ReceiveRequest (TimeSpan timeout)
      {
         RequestContext request = null;
         if (!TryReceiveRequest(base.DefaultReceiveTimeout, out request))
            throw new TimeoutException();
         return request;
      }
      /// <summary>
      /// Receives a request on the channel
      /// </summary>
      /// <param name="callback">
      /// Asynchronous completion delegate
      /// </param>
      /// <param name="state">
      /// Asynchronous completion delegate parameter
      /// </param>
      /// <returns>
      /// The asynchronous completion token
      /// </returns>
      public IAsyncResult BeginReceiveRequest (AsyncCallback callback, Object state)
      {
         return BeginReceiveRequest(base.DefaultReceiveTimeout, callback, state);
      }
      /// <summary>
      /// Receives a request on the channel
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
      public IAsyncResult BeginReceiveRequest (TimeSpan timeout, AsyncCallback callback, Object state)
      {
         return BeginTryReceiveRequest(timeout, callback, state);
      }
      /// <summary>
      /// Completes a request receive operation
      /// </summary>
      /// <param name="result">
      /// The asynchronous completion token
      /// </param>
      /// <returns>
      /// The received request
      /// </returns>
      public RequestContext EndReceiveRequest (IAsyncResult result)
      {
         RequestContext request;
         if (!EndTryReceiveRequest(result, out request))
            throw new TimeoutException();
         return request;
      }
      /// <summary>
      /// Waits for a request on the channel
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the wait operation
      /// </param>
      /// <returns>
      /// True if a request is available on the channel
      /// False otherwise
      /// </returns>
      public virtual Boolean WaitForRequest (TimeSpan timeout)
      {
         return EndWaitForRequest(BeginWaitForRequest(timeout, null, null));
      }
      /// <summary>
      /// Waits for a request on the channel
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
      public virtual IAsyncResult BeginWaitForRequest (TimeSpan timeout, AsyncCallback callback, Object state)
      {
         Boolean result = WaitForRequest(timeout);
         return new SyncResult(callback, state, result);
      }
      /// <summary>
      /// Completes a request wait operation
      /// </summary>
      /// <param name="result">
      /// The asynchronous completion token
      /// </param>
      /// <returns>
      /// True if a request is available
      /// False otherwise
      /// </returns>
      public virtual Boolean EndWaitForRequest (IAsyncResult result)
      {
         return ((SyncResult)result).GetResult<Boolean>();
      }
      #endregion
   }
}
