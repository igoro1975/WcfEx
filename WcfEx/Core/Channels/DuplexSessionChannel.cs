//===========================================================================
// MODULE:  DuplexSessionChannel.cs
// PURPOSE: duplex session channel base class
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
   /// Duplex session channel base class
   /// </summary>
   /// <remarks>
   /// This class provides a resonable default implementation of the
   /// tedious channel methods for duplex session channels. it encapsulates
   /// the communication channel as well as the channel state (ID).
   /// For flexibility, this class implements both the sync and async
   /// versions of all channel/interface methods automatically in terms
   /// of the other (sync calls async, async calls sync), allowing the 
   /// derived class to either implement the sync or async version and get
   /// the other for free. However, this means that the derived class MUST 
   /// implement either the sync or async versions to avoid a stack overflow.
   /// </remarks>
   public abstract class DuplexSessionChannel : 
      DuplexChannel, 
      IDuplexSessionChannel, 
      IDuplexSession
   {
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
      public DuplexSessionChannel (
         ChannelManagerBase manager,
         MessageCodec codec,
         EndpointAddress localAddress,
         EndpointAddress remoteAddress) 
         : base(manager, codec, localAddress, remoteAddress)
      {
      }
      #endregion

      #region ISessionChannel Implementation
      /// <summary>
      /// Retrieves the session for this channel
      /// </summary>
      public IDuplexSession Session
      {
         get { return this; }
      }
      #endregion

      #region IDuplexSession Implementation
      /// <summary>
      /// The session identifier string
      /// </summary>
      public abstract String Id { get; }
      /// <summary>
      /// Terminates the current session
      /// </summary>
      public void CloseOutputSession ()
      {
         CloseOutputSession(base.DefaultCloseTimeout);
      }
      /// <summary>
      /// Terminates the current session
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the close operation
      /// </param>
      public virtual void CloseOutputSession (TimeSpan timeout)
      {
         EndCloseOutputSession(BeginCloseOutputSession(timeout, null, null));
      }
      /// <summary>
      /// Closes the session
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
      public IAsyncResult BeginCloseOutputSession (AsyncCallback callback, Object state)
      {
         return BeginCloseOutputSession(base.DefaultCloseTimeout, callback, state);
      }
      /// <summary>
      /// Closes the session
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
      public virtual IAsyncResult BeginCloseOutputSession (TimeSpan timeout, AsyncCallback callback, Object state)
      {
         CloseOutputSession(timeout);
         return new SyncResult(callback, state);
      }
      /// <summary>
      /// Completes a session close operation
      /// </summary>
      /// <param name="result">
      /// The asynchronous completion token
      /// </param>
      public virtual void EndCloseOutputSession (IAsyncResult result)
      {
         result.WaitFor();
      }
      #endregion
   }
}
