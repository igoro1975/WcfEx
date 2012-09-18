//===========================================================================
// MODULE:  Channel.cs
// PURPOSE: In-process WCF communication channel class
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

namespace WcfEx.InProc
{
   /// <summary>
   /// InProc channel
   /// </summary>
   /// <remarks>
   /// This class represents each side of the symmetric
   /// session-oriented in-process WCF connection. It relays
   /// all messaging operations to the underlying Session instance.
   /// </remarks>
   internal sealed class Channel : DuplexSessionChannel
   {
      private Session session;

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new channel instance
      /// </summary>
      /// <param name="manager">
      /// Containing channel listener/factory
      /// </param>
      /// <param name="session">
      /// The current in-process session
      /// </param>
      /// <param name="address">
      /// The server communication address
      /// </param>
      public Channel (
         ChannelManagerBase manager,
         Session session,
         EndpointAddress address) 
         : base(manager, null, address, address)
      {
         this.session = session;
      }
      #endregion

      #region DuplexSessionChannel Overrides
      /// <summary>
      /// Retrieves the inProc session ID for this channel
      /// </summary>
      public override String Id
      {
         get { return this.session.ID; }
      }
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
         this.session.Complete();
      }
      /// <summary>
      /// Submits a message on the channel
      /// </summary>
      /// <param name="message">
      /// The message to send
      /// </param>
      /// <param name="timeout">
      /// The timeout for the submit operation
      /// </param>
      public override void Send (Message message, TimeSpan timeout)
      {
         message = message.CreateBufferedCopy(Int32.MaxValue).CreateMessage();
         if (this.RemoteAddress != null && message.Headers.To == null)
            this.RemoteAddress.ApplyTo(message);
         this.session.Enqeue(message);
      }
      /// <summary>
      /// Receives a message from the other end of the channel
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
      public override IAsyncResult BeginTryReceive (TimeSpan timeout, AsyncCallback callback, Object state)
      {
         // attempt to remove a message from the queue
         // if no message was dequeued, then the other side of the 
         // channel closed, so close this side
         // we must throw here to force the WCF dispatcher to shut down
         // the channel; otherwise, it will continue to call us
         IAsyncResult result = this.session.Dequeue(callback, state);
         if (result == null)
         {
            base.Close();
            throw new CommunicationObjectFaultedException();
         }
         return result;
      }
      /// <summary>
      /// Completes a message receive operation
      /// </summary>
      /// <param name="result">
      /// The asynchronous completion token
      /// </param>
      /// <param name="message">
      /// Return the received message via here
      /// </param>
      /// <returns>
      /// True if a message was received within the timeout
      /// False otherwise
      /// </returns>
      public override Boolean EndTryReceive (IAsyncResult result, out Message message)
      {
         SyncResult sync = result as SyncResult;
         message = (sync != null) ?
            sync.GetResult<Message>() :
            ((AsyncResult)result).GetResult<Message>();
         return (message != null);
      }
      /// <summary>
      /// Waits for a message on the channel
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the wait operation
      /// </param>
      /// <returns>
      /// True if a message is available on the channel
      /// False otherwise
      /// </returns>
      public override Boolean WaitForMessage (TimeSpan timeout)
      {
         throw new NotSupportedException();
      }
      /// <summary>
      /// Terminates the current session
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the close operation
      /// </param>
      public override void CloseOutputSession (TimeSpan timeout)
      {
         base.Close();
      }
      #endregion
   }
}
