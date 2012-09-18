//===========================================================================
// MODULE:  RequestReply.cs
// PURPOSE: request/reply context base class
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
   /// Request/reply context base class
   /// </summary>
   /// <remarks>
   /// This class provides a resonable default implementation of the
   /// tedious methods for request context objects. it encapsulates
   /// the request/response state on the server side of a request/response 
   /// communication channel.
   /// For flexibility, this class implements both the sync and async
   /// versions of all request context methods automatically in terms
   /// of the other (sync calls async, async calls sync), allowing the 
   /// derived class to either implement the sync or async version and get
   /// the other for free. However, this means that the derived class MUST 
   /// implement either the sync or async versions to avoid a stack overflow.
   /// </remarks>
   public abstract class RequestReply : RequestContext
   {
      private Message requestMessage;
      private MessageCodec codec;

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
      public RequestReply (Message request, MessageCodec codec) 
      {
         this.requestMessage = request;
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

      #region RequestContext Overrides
      /// <summary>
      /// Request context graceful shutdown callback
      /// </summary>
      public override void Close ()
      {
         Close(TimeSpan.MaxValue);
      }
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
      /// Request context unexpected shutdown callback
      /// </summary>
      public override void Abort ()
      {
         Close(TimeSpan.FromMilliseconds(0));
      }
      /// <summary>
      /// The original request message
      /// </summary>
      public override Message RequestMessage
      {
         get { return this.requestMessage; }
      }
      /// <summary>
      /// Submits a reply back to the client
      /// </summary>
      /// <param name="message">
      /// The reply message to send
      /// </param>
      public override void Reply (Message message)
      {
         Reply(message, TimeSpan.MaxValue);
      }
      /// <summary>
      /// Submits a reply message back to the client
      /// </summary>
      /// <param name="message">
      /// The reply message to send
      /// </param>
      /// <param name="timeout">
      /// The timeout for the submit operation
      /// </param>
      public override void Reply (Message message, TimeSpan timeout)
      {
         EndReply(BeginReply(message, timeout, null, null));
      }
      /// <summary>
      /// Submits a reply message back to the client
      /// </summary>
      /// <param name="message">
      /// The reply message to send
      /// </param>
      /// <param name="callback">
      /// Asynchronous completion delegate
      /// </param>
      /// <param name="state">
      /// Asynchronous completion delegate parameter
      /// </param>
      public override IAsyncResult BeginReply (
         Message message, 
         AsyncCallback callback, 
         Object state)
      {
         return BeginReply(message, TimeSpan.MaxValue, callback, state);
      }
      /// <summary>
      /// Submits a reply message back to the client
      /// </summary>
      /// <param name="message">
      /// The reply message to submit
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
      public override IAsyncResult BeginReply (
         Message message, 
         TimeSpan timeout, 
         AsyncCallback callback, 
         Object state)
      {
         Reply(message, timeout);
         return new SyncResult(callback, state);
      }
      /// <summary>
      /// Completes a message submission operation
      /// </summary>
      /// <param name="result">
      /// The asynchronous completion token
      /// </param>
      /// <returns>
      /// The request response
      /// </returns>
      public override void EndReply (IAsyncResult result)
      {
         result.WaitFor();
      }
      #endregion
   }
}
