//===========================================================================
// MODULE:  RequestChannel.cs
// PURPOSE: UDP WCF communication request channel class
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
// Project References

namespace WcfEx.Udp
{
   /// <summary>
   /// UDP request channel
   /// </summary>
   /// <remarks>
   /// This class represents the send side of the UDP connectionless
   /// transport for WCF. It relays all messaging operations to 
   /// the underlying socket.
   /// </remarks>
   internal sealed class RequestChannel : WcfEx.RequestChannel
   {
      UdpSocket socket;
      Dictionary<System.Xml.UniqueId, PendingRequest> requestMap;
      Int32 pending;

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
      public RequestChannel (
         ChannelManagerBase manager,
         MessageCodec codec,
         EndpointAddress remoteAddress,
         UdpSocket socket)
         : base(manager, codec, remoteAddress)
      {
         this.socket = socket;
         this.requestMap = new Dictionary<System.Xml.UniqueId, PendingRequest>();
         this.pending = 0;
      }
      #endregion

      #region RequestChannel Overrides
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
         // abort any pending requests
         if (this.requestMap.Count > 0)
         {
            List<PendingRequest> requests = new List<PendingRequest>(this.requestMap.Count);
            lock (base.ThisLock)
            {
               requests.AddRange(this.requestMap.Values);
               this.requestMap.Clear();
            }
            foreach (PendingRequest request in requests)
               request.Result.Complete(null, new CommunicationObjectFaultedException());
         }
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
      public override IAsyncResult BeginRequest (
         Message request, 
         TimeSpan timeout, 
         AsyncCallback callback, 
         Object state)
      {
         Boolean isOneWay = (request.Headers.ReplyTo == null);
         // prepare the request message for submission
         this.RemoteAddress.ApplyTo(request);
         if (request.Headers.MessageId == null)
            request.Headers.MessageId = new System.Xml.UniqueId();
         // abort any pending timed-out requests
         FlushTimeouts();
         // register the two-way request in the pending request map
         AsyncResult async = null;
         if (!isOneWay)
         {
            DateTime now = DateTime.UtcNow;
            PendingRequest context = new PendingRequest()
            {
               Result = async = new AsyncResult(callback, state, timeout),
               Expiration = (timeout < DateTime.MaxValue - now) ?
                  DateTime.UtcNow + timeout :
                  DateTime.MaxValue
            };
            lock (base.ThisLock)
               this.requestMap.Add(request.Headers.MessageId, context);
         }
         // submit the request start an async receive
         try
         {
            try
            {
               // send the request message on the socket
               using (ManagedBuffer requestBuffer = this.Codec.Encode(request))
                  this.socket.Send(requestBuffer);
               // start a receiver if none is running
               if (!isOneWay)
                  if (Interlocked.Increment(ref this.pending) == 1)
                     BeginReceive();
               // if no response is expected, complete the request sync
               return (isOneWay) ? new SyncResult(callback, state) : (IAsyncResult)async;
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
         }
         catch
         {
            // if an error occurred, remove the pending request context
            if (!isOneWay)
               lock (base.ThisLock)
                  this.requestMap.Remove(request.Headers.MessageId);
            throw;
         }
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
      public override Message EndRequest (IAsyncResult result)
      {
         result.WaitFor();
         return (result is SyncResult) ? 
            null : 
            ((AsyncResult)result).GetResult<Message>();
      }
      #endregion

      #region Request Helpers
      /// <summary>
      /// Starts an async response read operation on the 
      /// attached socket
      /// </summary>
      private void BeginReceive ()
      {
         ManagedBuffer buffer = this.Codec.Allocate();
         try
         {
            this.socket.BeginReceive(buffer, OnReceived, buffer);
         }
         catch
         {
            buffer.Dispose();
            throw;
         }
      }
      /// <summary>
      /// Async socket receive callback
      /// </summary>
      /// <param name="result">
      /// The asynchronous completion token
      /// </param>
      private void OnReceived (IAsyncResult result)
      {
         // receive the socket read results
         ArraySegment<Byte> received = new ArraySegment<Byte>();
         try { received = this.socket.EndReceive(result); }
         catch { ((ManagedBuffer)result.AsyncState).Dispose(); }
         // decode and dispatch the received message
         try
         {
            Message message = this.Codec.Decode(received);
            if (message != null)
            {
               PendingRequest request;
               lock (base.ThisLock)
                  if (this.requestMap.TryGetValue(message.Headers.MessageId, out request))
                     this.requestMap.Remove(message.Headers.MessageId);
               if (request.Result != null)
                  request.Result.Complete(message);
            }
         }
         catch { }
         // if there are additional requests pending,
         // restart the async socket receive
         if (Interlocked.Decrement(ref this.pending) > 0)
            try { BeginReceive(); }
            catch { }
      }
      /// <summary>
      /// Aborts any pending requests that have timed out
      /// </summary>
      private void FlushTimeouts ()
      {
         if (this.requestMap.Count > 0)
         {
            // serialize the list of pending requests
            List<KeyValuePair<System.Xml.UniqueId, PendingRequest>> entries = 
               new List<KeyValuePair<System.Xml.UniqueId, PendingRequest>>(this.requestMap.Count);
            lock (base.ThisLock)
               entries.AddRange(this.requestMap);
            // expire any requests whose expiration has passed
            DateTime now = DateTime.UtcNow;
            foreach (KeyValuePair<System.Xml.UniqueId, PendingRequest> entry in entries)
            {
               if (entry.Value.Expiration < now)
               {
                  Boolean removed = false;
                  lock (base.ThisLock)
                     removed = this.requestMap.Remove(entry.Key);
                  if (removed)
                     entry.Value.Result.Complete(null, new TimeoutException());
               }
            }
         }
      }
      #endregion

      /// <summary>
      /// Lightweight structure representing a pending 
      /// response read request on the current channel
      /// </summary>
      private struct PendingRequest
      {
         public AsyncResult Result;
         public DateTime Expiration;
      }
   }
}
