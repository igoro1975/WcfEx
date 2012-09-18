//===========================================================================
// MODULE:  Item.cs
// PURPOSE: UDP gossip sample database item
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
using System.Runtime.Serialization;
// Project References

namespace WcfEx.Samples.Gossip
{
   /// <summary>
   /// Gossip database item
   /// </summary>
   /// <remarks>
   /// This class represents an item stored in the gossip overlay
   /// network on a given node. It contains value/formatted properties
   /// as well as additional metadata used to control combination 
   /// operations and timestamps for incremental pull-based item 
   /// replication.
   /// </remarks>
   [DataContract]
   public class Item : ICloneable
   {
      #region Construction/Disposal
      /// <summary>
      /// Initializes a new item instance
      /// </summary>
      /// <param name="key">
      /// The global key for the item
      /// </param>
      public Item (String key)
      {
         this.Key = key;
         this.Created = DateTime.UtcNow;
         this.Updated = DateTime.MinValue;
         this.Contagion = 1;
      }
      /// <summary>
      /// Copy constructor
      /// </summary>
      /// <param name="other">
      /// Item to copy
      /// </param>
      public Item (Item other)
      {
         this.Key = other.Key;
         this.Created = other.Created;
         this.Updated = other.Updated;
         this.Contagion = other.Contagion;
         this.Data = other.Data;
      }
      /// <summary>
      /// Creates a deep copy of the item
      /// </summary>
      /// <returns>
      /// The copied item
      /// </returns>
      public virtual Object Clone ()
      {
         return new Item(this);
      }
      #endregion

      #region Properties
      /// <summary>
      /// The global key for the item
      /// </summary>
      [DataMember]
      public String Key { get; private set; }
      /// <summary>
      /// The item creation timestamp
      /// </summary>
      [DataMember]
      public DateTime Created { get; set; }
      /// <summary>
      /// The item last update timestamp
      /// </summary>
      [DataMember]
      public DateTime Updated { get; set; }
      /// <summary>
      /// The current contagion factor for the item,
      /// used with a [0-1) random number generator
      /// to determine whether to spread the item
      /// </summary>
      [DataMember]
      public Double Contagion { get; set; }
      /// <summary>
      /// The current data value of the item
      /// </summary>
      [DataMember]
      public Data Data { get; set; }
      #endregion
   }
}
