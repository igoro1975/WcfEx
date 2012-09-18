//===========================================================================
// MODULE:  DuplexChannel.cs
// PURPOSE: duplex channel base class
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
   /// Duplex channel base class
   /// </summary>
   /// <remarks>
   /// This class provides a resonable default implementation of the
   /// tedious channel methods for duplex channels. it encapsulates
   /// the input/output communication channel.
   /// For flexibility, this class implements both the sync and async
   /// versions of all channel/interface methods automatically in terms
   /// of the other (sync calls async, async calls sync), allowing the 
   /// derived class to either implement the sync or async version and get
   /// the other for free. However, this means that the derived class MUST 
   /// implement either the sync or async versions to avoid a stack overflow.
   /// </remarks>
   public abstract class DuplexChannel : Channel, IDuplexChannel
   {
      private EndpointAddress localAddress;
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
      /// <param name="localAddress">
      /// The address of the local endpoint
      /// </param>
      /// <param name="remoteAddress">
      /// The address of the remote endpoint
      /// </param>
      public DuplexChannel (
         ChannelManagerBase manager,
         MessageCodec codec,
         EndpointAddress localAddress,
         EndpointAddress remoteAddress) 
         : base(manager, codec)
      {
         this.localAddress = localAddress;
         this.remoteAddress = remoteAddress;
      }
      #endregion

      #region IOutputChannel Implementation
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
      /// Submits a message on the channel
      /// </summary>
      /// <param name="message">
      /// The message to send
      /// </param>
      public void Send (Message message)
      {
         Send(message, base.DefaultSendTimeout);
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
      public virtual void Send (Message message, TimeSpan timeout)
      {
         EndSend(BeginSend(message, timeout, null, null));
      }
      /// <summary>
      /// Submits a message on the channel
      /// </summary>
      /// <param name="message">
      /// The message to send
      /// </param>
      /// <param name="callback">
      /// Asynchronous completion delegate
      /// </param>
      /// <param name="state">
      /// Asynchronous completion delegate parameter
      /// </param>
      public IAsyncResult BeginSend (Message message, AsyncCallback callback, Object state)
      {
         return BeginSend(message, base.DefaultSendTimeout, callback, state);
      }
      /// <summary>
      /// Submits a message to the other end of the channel
      /// </summary>
      /// <param name="message">
      /// The WCF message to submit
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
      public virtual IAsyncResult BeginSend (Message message, TimeSpan timeout, AsyncCallback callback, Object state)
      {
         Send(message, timeout);
         return new SyncResult(callback, state);
      }
      /// <summary>
      /// Completes a message submission operation
      /// </summary>
      /// <param name="result">
      /// The asynchronous completion token
      /// </param>
      public virtual void EndSend (IAsyncResult result)
      {
         result.WaitFor();
      }
      #endregion

      #region IInputChannel Implementation
      /// <summary>
      /// The address of the local endpoint
      /// </summary>
      public EndpointAddress LocalAddress
      {
         get { return this.localAddress; }
      }
      /// <summary>
      /// Waits for a message on the channel
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the wait operation
      /// </param>
      /// <param name="message">
      /// Return the received message via here
      /// </param>
      /// <returns>
      /// True if a message was received within the timeout
      /// False otherwise
      /// </returns>
      public virtual Boolean TryReceive (TimeSpan timeout, out Message message)
      {
         return EndTryReceive(BeginTryReceive(timeout, null, null), out message);
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
      public virtual IAsyncResult BeginTryReceive (TimeSpan timeout, AsyncCallback callback, Object state)
      {
         Message message;
         TryReceive(timeout, out message);
         return new SyncResult(callback, state, message);
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
      public virtual Boolean EndTryReceive (IAsyncResult result, out Message message)
      {
         message = ((SyncResult)result).GetResult<Message>();
         return (message != null);
      }
      /// <summary>
      /// Receives a message on the channel
      /// </summary>
      /// <returns>
      /// The message received
      /// </returns>
      public Message Receive ()
      {
         return Receive(base.DefaultReceiveTimeout);
      }
      /// <summary>
      /// Receives a message on the channel
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the receive operation
      /// </param>
      /// <returns>
      /// The message received
      /// </returns>
      public Message Receive (TimeSpan timeout)
      {
         Message message = null;
         if (!TryReceive(base.DefaultReceiveTimeout, out message))
            throw new TimeoutException();
         return message;
      }
      /// <summary>
      /// Receives a message on the channel
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
      public IAsyncResult BeginReceive (AsyncCallback callback, Object state)
      {
         return BeginReceive(base.DefaultReceiveTimeout, callback, state);
      }
      /// <summary>
      /// Receives a message on the channel
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
      public IAsyncResult BeginReceive (TimeSpan timeout, AsyncCallback callback, Object state)
      {
         return BeginTryReceive(timeout, callback, state);
      }
      /// <summary>
      /// Completes a message receive operation
      /// </summary>
      /// <param name="result">
      /// The asynchronous completion token
      /// </param>
      /// <returns>
      /// The received message
      /// </returns>
      public Message EndReceive (IAsyncResult result)
      {
         Message message;
         if (!EndTryReceive(result, out message))
            throw new TimeoutException();
         return message;
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
      public virtual Boolean WaitForMessage (TimeSpan timeout)
      {
         return EndWaitForMessage(BeginWaitForMessage(timeout, null, null));
      }
      /// <summary>
      /// Waits for a message on the channel
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
      public virtual IAsyncResult BeginWaitForMessage (TimeSpan timeout, AsyncCallback callback, Object state)
      {
         Boolean result = WaitForMessage(timeout);
         return new SyncResult(callback, state, result);
      }
      /// <summary>
      /// Completes a message wait operation
      /// </summary>
      /// <param name="result">
      /// The asynchronous completion token
      /// </param>
      /// <returns>
      /// True if a message is available
      /// False otherwise
      /// </returns>
      public virtual Boolean EndWaitForMessage (IAsyncResult result)
      {
         return ((SyncResult)result).GetResult<Boolean>();
      }
      #endregion
   }
}
