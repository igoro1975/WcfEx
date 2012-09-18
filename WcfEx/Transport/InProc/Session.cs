//===========================================================================
// MODULE:  Session.cs
// PURPOSE: In-process WCF communication session class
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
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading;
// Project References

namespace WcfEx.InProc
{
   /// <summary>
   /// InProc session state class
   /// </summary>
   /// <remarks>
   /// This class represents a persistent session between an in-process
   /// WCF client and server. It encapsulates a pair of queues used
   /// to transfer messages between the client and server endpoints.
   /// This class uses coarse-grained locks on the session instance for
   /// synchronization, since it must synchronize a pair of queues for
   /// each message/callback processed.
   /// </remarks>
   internal sealed class Session
   {
      private static Int32 lastSessionID = 0;
      private String id;
      private Queue<Message> inputMessageQueue;
      private Queue<Message> outputMessageQueue;
      private Queue<AsyncResult> inputCallbackQueue;
      private Queue<AsyncResult> outputCallbackQueue;
      private Action completer;
      private Boolean isComplete;
      private Boolean isDraining;

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new server session instance
      /// </summary>
      public Session ()
      {
         this.id = Interlocked.Increment(ref lastSessionID).ToString();
         this.inputMessageQueue = new Queue<Message>();
         this.outputMessageQueue = new Queue<Message>();
         this.inputCallbackQueue = new Queue<AsyncResult>();
         this.outputCallbackQueue = new Queue<AsyncResult>();
      }
      /// <summary>
      /// Initializes a new client session instance
      /// </summary>
      /// <param name="id">
      /// The server session ID
      /// </param>
      /// <param name="inputMessageQueue">
      /// The client message queue
      /// </param>
      /// <param name="outputMessageQueue">
      /// The server message queue
      /// </param>
      /// <param name="inputCallbackQueue">
      /// The client callback queue
      /// </param>
      /// <param name="outputCallbackQueue">
      /// The server callback queue
      /// </param>
      private Session (
         String id,
         Queue<Message> inputMessageQueue,
         Queue<Message> outputMessageQueue,
         Queue<AsyncResult> inputCallbackQueue,
         Queue<AsyncResult> outputCallbackQueue
      )
      {
         this.id = id;
         this.inputMessageQueue = inputMessageQueue;
         this.outputMessageQueue = outputMessageQueue;
         this.inputCallbackQueue = inputCallbackQueue;
         this.outputCallbackQueue = outputCallbackQueue;
      }
      #endregion

      #region Properties
      /// <summary>
      /// The ID of the current in-process session
      /// </summary>
      public String ID
      {
         get { return this.id; }
      }
      /// <summary>
      /// The completion status of this session
      /// </summary>
      public Boolean IsCompleted
      {
         get { return this.isComplete && !this.inputMessageQueue.Any(); }
      }
      #endregion

      #region Operations
      /// <summary>
      /// Connects a client endpoint to the current
      /// server session
      /// </summary>
      /// <returns>
      /// The client side of the current session
      /// </returns>
      public Session Connect ()
      {
         if (this.completer != null)
            throw new InvalidOperationException("The current session is already connected.");
         Session other = new Session(
            this.id,
            this.outputMessageQueue,
            this.inputMessageQueue,
            this.outputCallbackQueue,
            this.inputCallbackQueue
         );
         this.completer = () => other.isComplete = true;
         other.completer = () => this.isComplete = true;
         return other;
      }
      /// <summary>
      /// Submits a message on the session
      /// </summary>
      /// <param name="message">
      /// The message to submit
      /// </param>
      public void Enqeue (Message message)
      {
         Boolean wasDraining;
         lock (this.outputMessageQueue)
         {
            this.outputMessageQueue.Enqueue(message);
            wasDraining = this.isDraining;
            this.isDraining = true;
         }
         if (!wasDraining)
            Dispatch(DrainQueue);
      }
      /// <summary>
      /// Retrieves a message from the session
      /// If no message is available, enqueues a callback
      /// for when a message is submitted on the other end
      /// </summary>
      /// <param name="callback">
      /// Asynchronous callback delegate
      /// </param>
      /// <param name="state">
      /// Asynchronous callback parameter
      /// </param>
      /// <returns>
      /// The asynchronous completion token
      /// </returns>
      public IAsyncResult Dequeue (AsyncCallback callback, Object state)
      {
         IAsyncResult result = null;
         Message message = null;
         lock (this.inputMessageQueue)
         {
            if (this.inputMessageQueue.Any())
               message = this.inputMessageQueue.Dequeue();
            else if (!this.isComplete)
            {
               AsyncResult async = new AsyncResult(callback, state);
               this.inputCallbackQueue.Enqueue(async);
               result = async;
            }
         }
         if (message != null)
            result = new SyncResult(callback, state, message);
         return result;
      }
      /// <summary>
      /// Gracefully terminates communication on this side of 
      /// the in-process session, and drains any outstanding
      /// callbacks
      /// </summary>
      public void Complete ()
      {
         Action completer = this.completer;
         this.completer = null;
         if (completer != null)
            completer();
         Dispatch(PurgeQueue);
      }
      /// <summary>
      /// Dispaches a delegate asynchronously
      /// </summary>
      /// <param name="action">
      /// The asynchronous delegate to execute
      /// </param>
      private void Dispatch (WaitCallback action)
      {
         ThreadPool.UnsafeQueueUserWorkItem(action, null);
      }
      /// <summary>
      /// Dispatches any outstanding callbacks on this
      /// side of the channel for messages that have arrived
      /// </summary>
      /// <param name="param">
      /// Unused
      /// </param>
      private void DrainQueue (Object param)
      {
         Queue<AsyncResult> callers = new Queue<AsyncResult>();
         Queue<Message> messages = new Queue<Message>();
         lock (this.outputMessageQueue)
         {
            while (this.outputCallbackQueue.Any() && this.outputMessageQueue.Any())
            {
               callers.Enqueue(this.outputCallbackQueue.Dequeue());
               messages.Enqueue(this.outputMessageQueue.Dequeue());
            }
            this.isDraining = false;
         }
         while (callers.Any())
            callers.Dequeue().Complete(messages.Dequeue());
      }
      /// <summary>
      /// Dispatches any outstanding callbacks on this
      /// side of the channel, regardless of whether
      /// messages are available
      /// </summary>
      /// <param name="param">
      /// Unused
      /// </param>
      private void PurgeQueue (Object param)
      {
         Queue<AsyncResult> callers = new Queue<AsyncResult>();
         Queue<Message> messages = new Queue<Message>();
         lock (this.outputMessageQueue)
         {
            while (this.outputCallbackQueue.Any())
            {
               callers.Enqueue(this.outputCallbackQueue.Dequeue());
               if (this.outputMessageQueue.Any())
                  messages.Enqueue(this.outputMessageQueue.Dequeue());
            }
         }
         while (callers.Any())
            callers.Dequeue().Complete((messages.Any()) ? messages.Dequeue() : null);
      }
      #endregion
   }
}
