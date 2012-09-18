//===========================================================================
// MODULE:  Peer.cs
// PURPOSE: UDP gossip node peer reference
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
// Project References

namespace WcfEx.Samples.Gossip
{
   /// <summary>
   /// Gossip peer
   /// </summary>
   /// <remarks>
   /// This class represents a remote peer node referenced by the current
   /// node in the gossiping system.
   /// </remarks>
   public class Peer : IDisposable
   {
      /// <summary>
      /// Peer communication failure event
      /// </summary>
      public event Action<Peer, Exception> OnException;

      /// <summary>
      /// Initializes a new peer instance
      /// </summary>
      /// <param name="id"></param>
      public Peer (Uri id)
      {
         this.ID = id;
         this.Timestamp = DateTime.MinValue;
         this.Proxy = new Client<INode>("GossipClient", this.ID);
      }
      /// <summary>
      /// Disconnects from the peer
      /// </summary>
      public void Dispose ()
      {
         this.Proxy.Dispose();
      }

      /// <summary>
      /// The remote peer ID
      /// </summary>
      public Uri ID { get; private set; }
      /// <summary>
      /// The current peers timestamp for the remote peer
      /// </summary>
      public DateTime Timestamp { get; private set; }
      /// <summary>
      /// The WCF proxy used to communicate with the peer
      /// </summary>
      private Client<INode> Proxy { get; set; }

      /// <summary>
      /// Invokes the peer's SelectPeer method
      /// </summary>
      /// <returns>
      /// The ID of a random peer referenced by the current peer
      /// </returns>
      public Uri CallSelectPeer ()
      {
         Uri otherID = null;
         try
         {
            otherID = this.Proxy.Server.SelectPeer();
         }
         catch (Exception e)
         {
            Dispatch(e);
            throw;
         }
         return otherID;
      }
      /// <summary>
      /// Invokes the peer's Combine method
      /// </summary>
      /// <param name="input">
      /// The item to combine
      /// </param>
      /// <param name="output">
      /// The result of the peer's combine operation
      /// </param>
      /// <returns>
      /// True if the combination was successful
      /// False otherwise
      /// </returns>
      public Boolean CallCombine (Item input, out Item output)
      {
         Boolean combined = false;
         output = null;
         try
         {
            combined = this.Proxy.Server.Combine(input, out output);
         }
         catch (Exception e)
         {
            Dispatch(e);
            throw;
         }
         return combined;
      }
      /// <summary>
      /// Submits a pull notification to the peer
      /// </summary>
      /// <param name="fromID">
      /// The current node's identifier
      /// </param>
      public void SendPull (Uri fromID)
      {
         try
         {
            this.Proxy.Server.Pull(fromID, this.Timestamp);
         }
         catch { }
      }
      /// <summary>
      /// Submits a commit notification to the peer
      /// </summary>
      /// <param name="fromID">
      /// The current node's identifier
      /// </param>
      /// <param name="timestamp">
      /// The commit timestamp
      /// </param>
      public void SendCommit (Uri fromID, DateTime timestamp)
      {
         try
         {
            this.Proxy.Server.Commit(fromID, timestamp);
         }
         catch { }
      }
      /// <summary>
      /// Commits the peer's timestamp
      /// </summary>
      /// <param name="timestamp">
      /// The updated timestamp
      /// </param>
      public void Commit (DateTime timestamp)
      {
         lock (this)
            if (timestamp > this.Timestamp)
               this.Timestamp = timestamp;
      }
      /// <summary>
      /// Dispatches an exception to attached event handlers
      /// </summary>
      /// <param name="e">
      /// The exception to dispatch
      /// </param>
      private void Dispatch (Exception e)
      {
         if (this.OnException != null)
            this.OnException(this, e);
      }
   }
}
