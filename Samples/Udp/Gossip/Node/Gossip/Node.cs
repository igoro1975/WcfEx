//===========================================================================
// MODULE:  Node.cs
// PURPOSE: UDP gossip node contract implementation
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
using System.Linq;
using System.ServiceModel;
// Project References

namespace WcfEx.Samples.Gossip
{
   /// <summary>
   /// The gossip node
   /// </summary>
   /// <remarks>
   /// This class represents a single instance of a gossiping entity, 
   /// equipped with a gossip database and zero or more peers with which
   /// to exchange registered items. This class implements both anti-entropy 
   /// (pull) and gossiping (push) epidemic protocols. These protocols are 
   /// driven by a configured timer. Each time the timer fires (see Wakeup), 
   /// the node performs the following operations:
   ///   1. attempt to locate new peers from existing peers
   ///   2. generate any automatic items from all combinators
   ///   3. pull updates from a single random peer (anti-entropy)
   ///   4. push updates to random peers, based on item contagion (gossiping)
   /// The node eagerly trims the list of peers (except for the static 
   /// parent) as peer communication failures occur, to ensure that peer
   /// timeouts only cause transient latency in the node.
   /// </remarks>
   /// <see>
   ///   http://books.google.com/books?vid=ISBN0132392275
   ///   Section 4.5 in 1st edition
   /// </see>
   [ServiceBehavior(
      InstanceContextMode = InstanceContextMode.Single, 
      ConcurrencyMode = ConcurrencyMode.Multiple)]
   public sealed class Node : INode
   {
      private PeerList peers;
      private System.Timers.Timer wakeupTimer;
      private Uri nodeID;
      private Uri parentID;
      private Database db;
      
      #region Construction/Disposal
      /// <summary>
      /// Initializes a new node instance
      /// </summary>
      /// <param name="nodeID">
      /// This node's ID
      /// </param>
      /// <param name="parentID">
      /// An optional initial peer ID
      /// </param>
      /// <param name="db">
      /// This node's database
      /// </param>
      public Node (
         Uri nodeID, 
         Uri parentID,
         Database db)
      {
         this.peers = new PeerList(Config.Instance.MaxPeersPerNode);
         this.peers.OnPeerException += this.HandlePeerException;
         this.wakeupTimer = new System.Timers.Timer();
         this.nodeID = nodeID;
         this.parentID = parentID;
         if (parentID != null)
            this.peers.GetOrAdd(parentID);
         this.db = db;
         this.wakeupTimer.Elapsed += Wakeup;
         this.wakeupTimer.AutoReset = false;
         this.wakeupTimer.Interval = StaticRandom.NextCentered(Config.Instance.WakeupInterval, 100);
         this.wakeupTimer.Start();
      }
      #endregion

      #region IGossipNode Implementation
      /// <summary>
      /// Retrieves a randome peer from this node's list
      /// </summary>
      /// <returns>
      /// A random node ID from this node's peer list
      /// </returns>
      public Uri SelectPeer ()
      {
         var peer = this.peers.Random();
         return (peer != null) ? peer.ID : null;
      }
      /// <summary>
      /// Queries the current node for a database item
      /// </summary>
      /// <param name="key">
      /// The database item key
      /// </param>
      /// <returns>
      /// The requested item's data if found
      /// Data.Empty otherwise
      /// </returns>
      public Data Query (String key)
      {
         return this.db.Query(key);
      }
      /// <summary>
      /// Combines a database item from a remote node
      /// </summary>
      /// <param name="input">
      /// The incoming item to combine
      /// </param>
      /// <param name="output">
      /// The result of the combination
      /// </param>
      /// <returns>
      /// True if the combination was successful
      /// False otherwise
      /// </returns>
      public Boolean Combine (Item input, out Item output)
      {
         return this.db.Combine(input, out output);
      }
      /// <summary>
      /// Requests that a node send its updates to
      /// the requesting node
      /// </summary>
      /// <param name="fromID">
      /// The pulling node identifier
      /// </param>
      /// <param name="timestamp">
      /// The minimum timestamp of the items to retrieve;
      /// used to avoid retrieving an item multiple times
      /// </param>
      public void Pull (Uri fromID, DateTime timestamp)
      {
         // attempt to retrieve the peer from the list
         // randomly choose to add the pulling peer to the liset
         var peer = (StaticRandom.NextDouble() > Config.Instance.PullJoinProbability) ?
            this.peers.GetOrAdd(fromID) :
            this.peers.Get(fromID);
         // if this is not a peer in the list, allocate a
         // temporary peer object for communication
         var isTemp = (peer == null);
         if (isTemp)
            peer = this.peers.Create(fromID);
         try
         {
            // generate the new commit stamp, and transfer any 
            // items changed since the previous commit stamp
            var newStamp = DateTime.UtcNow;
            var items = this.db.List().Where(i => i.Updated >= timestamp);
            if (items.Any())
            {
               foreach (var item in items)
                  Push(peer, item);
               // if all were pushed without an exception, send the commit message
               peer.SendCommit(this.nodeID, newStamp);
            }
         }
         finally
         {
            if (isTemp)
               peer.Dispose();
         }
      }
      /// <summary>
      /// Signals the end of a successful pull operation,
      /// called by the pushing node
      /// </summary>
      /// <param name="fromID">
      /// The pushing node identifier
      /// </param>
      /// <param name="timestamp">
      /// The timestamp of the start of the pull operation,
      /// to be used in subsequent Pull calls
      /// </param>
      public void Commit (Uri fromID, DateTime timestamp)
      {
         Peer peer = this.peers.Get(fromID);
         if (peer != null)
            peer.Commit(timestamp);
      }
      #endregion

      #region Operations
      /// <summary>
      /// Attempts to discover new peers from the existing
      /// peer list, using SelectPeer()
      /// </summary>
      private void Find ()
      {
         if (!this.peers.IsFull)
         {
            var peer = this.peers.Random();
            if (peer != null)
            {
               var otherID = (Uri)null;
               try { otherID = peer.CallSelectPeer(); }
               catch { }
               if (otherID != null && otherID != this.nodeID)
                  this.peers.GetOrAdd(otherID);
            }
         }
      }
      /// <summary>
      /// Generates any new items from the registered combinators
      /// </summary>
      private void Gen ()
      {
         this.db.Generate();
      }
      /// <summary>
      /// Starts an item pull operation from a random peer
      /// </summary>
      private void Pull ()
      {
         var peer = this.peers.Random();
         if (peer != null)
            peer.SendPull(this.nodeID);
      }
      /// <summary>
      /// Pushes all items in the database to a random selection
      /// of the peers, based on the item contagion
      /// </summary>
      private void Push ()
      {
         foreach (var item in this.db.List())
         {
            var to = this.peers.ToList();
            while (to.Any() && StaticRandom.NextDouble() < item.Contagion)
            {
               var peerIdx = StaticRandom.Next(0, to.Count);
               try { Push(to[peerIdx], item); }
               catch { }
               to.RemoveAt(peerIdx);
            }
         }
      }
      /// <summary>
      /// Pushes a single item to a peer, and combines
      /// the result locally
      /// </summary>
      /// <param name="peer">
      /// The peer to push to
      /// </param>
      /// <param name="item">
      /// The item to push
      /// </param>
      private void Push (Peer peer, Item item)
      {
         lock(item)
         {
            var output = item;
            if (peer.CallCombine(item, out output))
               this.db.Combine(output, out output);
            else
               item.Contagion /= 2;
         }
      }
      /// <summary>
      /// Handles peer communication exceptions,
      /// eagerly removing failed peers from the list
      /// </summary>
      /// <param name="peer">
      /// The peer causing the failure
      /// </param>
      /// <param name="e">
      /// The exception that occurred
      /// </param>
      private void HandlePeerException (Peer peer, Exception e)
      {
         if (peer.ID != this.parentID)
            this.peers.Remove(peer.ID);
      }
      #endregion

      #region Event Handlers
      /// <summary>
      /// Node timer event handller
      /// </summary>
      /// <param name="s">
      /// The event sender
      /// </param>
      /// <param name="a">
      /// Event parameters
      /// </param>
      private void Wakeup (Object s, EventArgs a)
      {
         Find();
         Gen();
         Pull();
         Push();
         wakeupTimer.Start();
      }
      #endregion
   }
}
