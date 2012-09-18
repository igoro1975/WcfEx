//===========================================================================
// MODULE:  INode.cs
// PURPOSE: UDP gossip sample service contract interface
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
using System.ServiceModel;
// Project References

namespace WcfEx.Samples.Gossip
{
   /// <summary>
   /// Gossip node contract
   /// </summary>
   /// <remarks>
   /// This is the interface used to communicate among the nodes (machines) 
   /// in the gossip overlay network and to query/update the local node.
   /// </remarks>
   [ServiceContract(Namespace = "http://brentspell.us/Projects/WcfEx/Samples/Gossip/")]
   [TypeResolver(typeof(SharedTypeResolver))]
   public interface INode
   {
      /// <summary>
      /// Retrieves a randome peer from this node's list
      /// </summary>
      /// <returns>
      /// A random node ID from this node's peer list
      /// </returns>
      [OperationContract]
      Uri SelectPeer ();
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
      [OperationContract]
      Data Query (String key);
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
      [OperationContract]
      Boolean Combine (Item input, out Item output);
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
      [OperationContract(IsOneWay = true)]
      void Pull (Uri fromID, DateTime timestamp);
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
      [OperationContract(IsOneWay = true)]
      void Commit (Uri fromID, DateTime timestamp);
   }
}
