//===========================================================================
// MODULE:  PeerList.cs
// PURPOSE: UDP gossip node peer list
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
// Project References

namespace WcfEx.Samples.Gossip
{
   /// <summary>
   /// Gossip peer list
   /// </summary>
   /// <remarks>
   /// This class maintains a thread-safe set of peers associated with
   /// a local node. The class constrains the peer list to a configured 
   /// maximum count.
   /// </remarks>
   public sealed class PeerList
   {
      private Dictionary<Uri, Peer> peerMap;
      private List<Peer> peerList;

      /// <summary>
      /// Peer communication failure event
      /// </summary>
      public Action<Peer, Exception> OnPeerException;

      /// <summary>
      /// Initializes a new list instance
      /// </summary>
      /// <param name="capacity">
      /// The maximum number of peers allowed in the list
      /// </param>
      public PeerList (Int32 capacity)
      {
         this.peerMap = new Dictionary<Uri, Peer>(capacity);
         this.peerList = new List<Peer>(capacity);
      }
      
      /// <summary>
      /// Samples the current peer count
      /// </summary>
      public Int32 Count
      {
         get { return this.peerList.Count; }
      }
      /// <summary>
      /// Returns whether there are any peers in the list
      /// </summary>
      public Boolean IsEmpty 
      { 
         get { return !this.peerList.Any(); } 
      }
      /// <summary>
      /// Returns whether the list can accept new peers
      /// </summary>
      public Boolean IsFull
      {
         get { return (this.peerList.Count == this.peerList.Capacity); }
      }

      /// <summary>
      /// Retrieves a peer by its node identifier
      /// </summary>
      /// <param name="id">
      /// The peer node identifier
      /// </param>
      /// <returns>
      /// The requested peer if found
      /// Null otherwise
      /// </returns>
      public Peer Get (Uri id)
      {
         Peer peer = null;
         lock (this)
            this.peerMap.TryGetValue(id, out peer);
         return peer;
      }
      /// <summary>
      /// Retrieves a peer by its node identifier,
      /// or adds it to the list if not full
      /// </summary>
      /// <param name="id">
      /// The peer node identifier
      /// </param>
      /// <returns>
      /// The requested peer if found or added
      /// Null otherwise
      /// </returns>
      public Peer GetOrAdd (Uri id)
      {
         Peer peer = null;
         lock (this)
            if (!this.peerMap.TryGetValue(id, out peer) && !IsFull)
               peerList.Add(peerMap[id] = peer = Create(id));
         return peer;
      }
      /// <summary>
      /// Removes a peer from the list
      /// </summary>
      /// <param name="id">
      /// The peer node identifier
      /// </param>
      /// <returns>
      /// True if the peer was found and removed
      /// False otherwise
      /// </returns>
      public Boolean Remove (Uri id)
      {
         Peer peer = null;
         lock (this)
         {
            if (this.peerMap.TryGetValue(id, out peer))
            {
               this.peerMap.Remove(id);
               this.peerList.Remove(peer);
            }
         }
         if (peer != null)
         {
            peer.Dispose();
            return true;
         }
         return false;
      }
      /// <summary>
      /// Selects a random peer from the list
      /// </summary>
      /// <returns>
      /// The random peer if the list is not empty
      /// Null otherwise
      /// </returns>
      public Peer Random ()
      {
         Peer peer = null;
         if (!IsEmpty)
            lock (this)
               if (this.peerList.Any())
                  peer = this.peerList[StaticRandom.Next(0, this.peerList.Count)];
         return peer;
      }
      /// <summary>
      /// Copies the peer list to a new instance
      /// </summary>
      /// <returns>
      /// The new peer list
      /// </returns>
      public IList<Peer> ToList ()
      {
         List<Peer> list = new List<Peer>(this.peerList.Count);
         lock (this)
            list.AddRange(this.peerList);
         return list;
      }
      /// <summary>
      /// Creates a new peer instance
      /// </summary>
      /// <param name="id">
      /// The peer's node identifier
      /// </param>
      /// <returns>
      /// The new peer instance
      /// </returns>
      public Peer Create (Uri id)
      {
         Peer peer = new Peer(id);
         if (this.OnPeerException != null)
            peer.OnException += HandleException;
         return peer;
      }
      /// <summary>
      /// Handles a peer communication exception,
      /// bubbling the exception up to any configured
      /// event handlers
      /// </summary>
      /// <param name="peer">
      /// The peer raising the exception
      /// </param>
      /// <param name="e">
      /// The communication exception
      /// </param>
      private void HandleException (Peer peer, Exception e)
      {
         if (this.OnPeerException != null)
            this.OnPeerException(peer, e);
      }
   }
}
