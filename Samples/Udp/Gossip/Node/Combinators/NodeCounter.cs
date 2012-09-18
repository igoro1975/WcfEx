//===========================================================================
// MODULE:  NodeCounter.cs
// PURPOSE: UDP gossip sample node counter combinator
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
using System.Runtime.Serialization;
using System.Threading;
// Project References

namespace WcfEx.Samples.Gossip.Combinators
{
   /// <summary>
   /// The node counter combinator
   /// </summary>
   /// <remarks>
   /// This class implements the epidemic protocol for counting the number 
   /// of nodes in the mesh network. This is done by having an initiating 
   /// node set an item to the value 1, all other nodes setting the value 
   /// to 0, and then during each round, each node computes the average of
   /// its value with a peer. Once all values converge (to within an error), 
   /// the reciprocal of the averages should be an approximation to the
   /// size of the mesh network.
   /// During item generation, the combinator only generates a new item
   /// if the current default item is older than EpochInterval. Since 
   /// multiple nodes may decide to generate the item at the same time, the
   /// epoch counter is used to determine priority, as only one node should
   /// generate the maximum (random) value within the mesh.
   /// If a node successfully combines with another node, it excites the
   /// item's contagion value (affecting how it is pushed through the system)
   /// by the difference between the two nodes' values, the intent being 
   /// to spread large changes more frequently than small changes, which 
   /// has been shown to decrease the time to convergence. Similarly, if a
   /// combination fails (by converging), the combinator decreases the item's
   /// contagion factor, to minimize network traffic.
   /// </remarks>
   /// <example>
   /// Consider a network with 4 nodes. In an optimal series of exchanges,
   /// they will converge to the following values:
   ///   1.00 0.00 0.00 0.00     start
   ///   0.50 0.50 0.00 0.00     exchange(0, 1)
   ///   0.50 0.25 0.25 0.00     exchange(1, 2)
   ///   0.25 0.25 0.25 0.25     exchange(0, 3)
   /// This results in all nodes computing the average 0.25, whose reciprocal
   /// is 4, the size of the network.
   /// Other orderings will converge in more steps, but they can be proven
   /// to converge eventually.
   /// </example>
   /// <see>
   ///   http://books.google.com/books?vid=ISBN0132392275
   ///   Section 4.5 in 1st edition
   /// </see>
   public sealed class NodeCounter : ICombinator
   {
      private const String DefaultName = ".Count";
      private Count defaultItem = null;

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new counter instance
      /// </summary>
      public NodeCounter ()
      {
         // apply configuration defaults
         this.MaximumError = 0.5d;
         this.EpochInterval = TimeSpan.FromSeconds(60);
      }
      #endregion

      #region Configuration Properties
      /// <summary>
      /// The amount of time that must elapse between successful
      /// combinations before a new epoch will be started, which
      /// will restart the node count
      /// </summary>
      public TimeSpan EpochInterval { get; set; }
      /// <summary>
      /// The value to use to test count convergence against
      /// incoming combinations
      /// </summary>
      public Double MaximumError { get; set; }
      #endregion

      #region ICombinator Implementation
      /// <summary>
      /// Combinator item factory
      /// </summary>
      /// <param name="key">
      /// The new item's key
      /// </param>
      /// <returns>
      /// The generated item
      /// </returns>
      public Item Create (String key)
      {
         if (key.EndsWith(DefaultName, StringComparison.OrdinalIgnoreCase))
            return
               this.defaultItem ??
               Interlocked.CompareExchange(ref this.defaultItem, new Count(key), null) ??
               this.defaultItem;
         return new Count(key);
      }
      /// <summary>
      /// Autonomously generates a new counter if there hasn't
      /// been a count update since EpochInterval
      /// </summary>
      /// <param name="ns">
      /// The item's namespace
      /// </param>
      /// <returns>
      /// The generated items
      /// </returns>
      public IEnumerable<Item> Generate (String ns)
      {
         if (this.defaultItem == null)
            Create(ns + DefaultName);
         if (DateTime.UtcNow - this.defaultItem.Updated > EpochInterval)
         {
            lock (this.defaultItem)
            {
               this.defaultItem.Created = this.defaultItem.Updated = DateTime.UtcNow;
               this.defaultItem.Contagion = 1;
               this.defaultItem.Epoch = Math.Ceiling(this.defaultItem.Epoch) + StaticRandom.NextDouble();
               this.defaultItem.Average = 1.0d;
            }
            yield return this.defaultItem;
         }
      }
      /// <summary>
      /// Combines an existing count item with an incoming value
      /// </summary>
      /// <param name="storeItem">
      /// The current stored counter value
      /// </param>
      /// <param name="inputItem">
      /// The incoming counter value
      /// </param>
      /// <param name="output">
      /// The resulting counter value
      /// </param>
      /// <returns></returns>
      public Boolean Combine (Item storeItem, Item inputItem, out Item output)
      {
         output = null;
         // verify type compatibility
         Count store = storeItem as Count;
         Count input = inputItem as Count;
         if (store != null && input != null)
         {
            // if the incoming item has a later epoch (winner),
            // then join its epoch and start over at 0
            if (input.Epoch > store.Epoch)
            {
               store.Average = 0.0d;
               store.Created = store.Updated = input.Created;
               store.Epoch = input.Epoch;
            }
            // save off the existing version of the counter value
            // as the combination output, so that both peers will 
            // compute the same average
            output = (Item)store.Clone();
            // only combine with items that have the same epoch
            if (input.Epoch == store.Epoch)
            {
               // compute the current node counts for the
               // stored and input items, and compare them
               // to the convergence error value
               var valueError = Math.Abs(input.Average - store.Average);
               var countError = Math.Abs(store.Value - input.Value);
               if (countError > this.MaximumError)
               {
                  // update the stored average, and excite the item
                  // so that it will spread to other peers quickly
                  store.Average = (store.Average + input.Average) / 2;
                  store.Updated = DateTime.UtcNow;
                  store.Contagion += valueError;
                  return true;
               }
               // since we have converged, suppress the spreading
               // of the item by 1 - the error value
               store.Contagion -= (1 - valueError);
            }
         }
         return false;
      }
      #endregion

      /// <summary>
      /// Node counter item derivative
      /// </summary>
      [DataContract]
      public class Count : Item
      {
         /// <summary>
         /// Initializes a new item instance
         /// </summary>
         /// <param name="key">
         /// The item key
         /// </param>
         public Count (String key) : base(key)
         {
            this.Epoch = 0;
         }
         /// <summary>
         /// Copy constructor
         /// </summary>
         /// <param name="other">
         /// The item to copy
         /// </param>
         public Count (Count other) : base(other)
         {
            this.Epoch = other.Epoch;
         }
         /// <summary>
         /// Creates a deep copy of the item
         /// </summary>
         /// <returns>
         /// The copied item
         /// </returns>
         public override Object Clone ()
         {
            return new Count(this);
         }
         /// <summary>
         /// The current node count reciprocal
         /// being calculated
         /// </summary>
         public Double Average
         {
            get
            {
               return (Double)(base.Data.Value ?? 0.0d);
            }
            set
            {
               base.Data = new Data(
                  value,
                  Convert.ToInt32(
                     Math.Min(1 / value, Int32.MaxValue)
                  ).ToString()
               );
            }
         }
         /// <summary>
         /// The current count estimate
         /// </summary>
         public Double Value
         {
            get { return 1 / this.Average; }
         }
         /// <summary>
         /// The epoch for the item, used with a [0-1]
         /// random number generator to determine priority
         /// of a node's copy during conflicts
         /// </summary>
         [DataMember]
         public Double Epoch
         {
            get; set;
         }
      }
   }
}
