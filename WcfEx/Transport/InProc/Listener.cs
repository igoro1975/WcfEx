//===========================================================================
// MODULE:  Listener.cs
// PURPOSE: In-process WCF channel listener class
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
// Project References

namespace WcfEx.InProc
{
   /// <summary>
   /// InProc channel listener
   /// </summary>
   /// <remarks>
   /// This class provides support for creating server channels upon 
   /// connection requests from in-process clients.
   /// </remarks>
   internal sealed class Listener : ChannelListener<IDuplexSessionChannel>
   {
      private ConcurrentQueue<Session> connections = new ConcurrentQueue<Session>();
      private AsyncResult onAccepted;

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new listener instance
      /// </summary>
      /// <param name="binding">
      /// The WCF custom configuration for this listener type
      /// </param>
      /// <param name="context">
      /// The address binding configuration for this listener instance
      /// </param>
      public Listener (BindingElement binding, BindingContext context) : base(context)
      {
      }
      #endregion

      #region Operations
      /// <summary>
      /// Accepts a new client connection
      /// </summary>
      /// <param name="session">
      /// The server end of the session
      /// </param>
      public void Accept (Session session)
      {
         AsyncResult onAccepted = null;
         this.connections.Enqueue(session);
         lock (base.ThisLock)
         {
            if (this.onAccepted != null && this.connections.TryDequeue(out session))
            {
               onAccepted = this.onAccepted;
               this.onAccepted = null;
            }
         }
         if (onAccepted != null)
            ThreadPool.UnsafeQueueUserWorkItem(o => onAccepted.Complete(session), null);
      }
      #endregion

      #region ChannelListener Overrides
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
         throw new NotSupportedException();
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
         if (this.State != CommunicationState.Opened)
            throw new CommunicationObjectFaultedException();
         // determine whether there are any existing
         // connection requests that can be accepted
         Session session = null;
         if (this.connections.TryDequeue(out session))
            return new SyncResult(callback, state, session);
         lock (base.ThisLock)
         {
            // double-check after locking to ensure no new
            // session requests were enqueued
            if (this.connections.TryDequeue(out session))
               return new SyncResult(callback, state, session);
            // if not, complete this call asynchronously
            // once a client connection arrives
            if (this.onAccepted != null)
               throw new InvalidOperationException("Existing accept call pending");
            return this.onAccepted = new AsyncResult(callback, state, timeout);
         }
      }
      /// <summary>
      /// Listener connection accept callback
      /// </summary>
      /// <param name="result">
      /// The asynchronous completion token
      /// </param>
      protected override IDuplexSessionChannel OnEndAcceptChannel (IAsyncResult result)
      {
         SyncResult sync = result as SyncResult;
         Session connect = (sync != null) ?
            sync.GetResult<Session>() :
            ((AsyncResult)result).GetResult<Session>();
         return (connect != null) ?
            new Channel(this, connect, null) as IDuplexSessionChannel :
            null;
      }
      /// <summary>
      /// Listener initialization callback
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the open operation
      /// </param>
      protected override void OnOpen (TimeSpan timeout)
      {
         Broker.Listen(this.Address.Uri, this);
      }
      /// <summary>
      /// Listener graceful shutdown callback
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the close operation
      /// </param>
      protected override void OnClose (TimeSpan timeout)
      {
         Broker.Unlisten(this.Address.Uri);
         // complete any outstanding async waits
         // with null sessions
         AsyncResult onAccepted = null;
         lock (base.ThisLock)
         {
            onAccepted = this.onAccepted;
            this.onAccepted = null;
         }
         if (onAccepted != null)
            onAccepted.Complete(null);
      }
      #endregion
   }
}
