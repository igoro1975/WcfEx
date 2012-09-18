//===========================================================================
// MODULE:  ICombinator.cs
// PURPOSE: UDP gossip sample item combinator interface
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
// Project References

namespace WcfEx.Samples.Gossip
{
   /// <summary>
   /// Item combinator plugin interface
   /// </summary>
   /// <remarks>
   /// This is the interface between the generic gossip node and the 
   /// specific combinator implementations. Combinators may spawn items
   /// via the Generate method and merge items from other nodes via
   /// the Combine method.
   /// </remarks>
   public interface ICombinator
   {
      /// <summary>
      /// Combinator item factory
      /// </summary>
      /// <param name="key">
      /// The new item's key
      /// </param>
      /// <returns>
      /// The generated item
      /// </returns>
      Item Create (String key);
      /// <summary>
      /// Generates a set of default items in the gossip database
      /// </summary>
      /// <param name="ns">
      /// The combinator's namespace
      /// </param>
      /// <returns>
      /// A list of items to add to the gossip database
      /// </returns>
      IEnumerable<Item> Generate (String ns);
      /// <summary>
      /// Combines an item from another node
      /// </summary>
      /// <param name="store">
      /// The local node's copy of the item
      /// </param>
      /// <param name="input">
      /// The remote node's copy of the item
      /// </param>
      /// <param name="output">
      /// The result of the combination, to return to the remote node
      /// </param>
      /// <returns>
      /// True if the item was combined
      /// False otherwise
      /// </returns>
      Boolean Combine (Item store, Item input, out Item output);
   }
}
