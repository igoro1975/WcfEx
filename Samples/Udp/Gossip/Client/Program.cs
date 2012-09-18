//===========================================================================
// MODULE:  Program.cs
// PURPOSE: UDP gossip sample client program
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
using System.Threading;
// Project References

namespace WcfEx.Samples.Gossip
{
   /// <summary>
   /// Client program mode
   /// </summary>
   public enum Operation
   {
      Get,     // retrieve a value from the gossip network
      Set,     // combine a value into the gossip network
      Mon      // monitor the nodes for a value in the gossip network
   }

   /// <summary>
   /// The gossip client program
   /// </summary>
   /// <remarks>
   /// This program interacts with a running gossip mesh network, providing
   /// the ability to retrieve, combine, or monitor the values of an item
   /// in the network's database.
   /// When retrieving an item from the network, the application connects to
   /// the local gossip node on port 40000 and queries for the item's key.
   /// Whe combining an item in the network, the application connects to the
   /// local gossip node and tells the node to combine the specified value
   /// with its local copy, according to the rules of that item's combinator.
   /// When monitoring an item in the network, the application attempts to
   /// discover as many nodes as possible within the network and periodically 
   /// samples the value of the specified item from all reachable nodes, 
   /// reporting statistics for the value during each sampling period.
   /// </remarks>
   class Program
   {
      const Int32 BasePort = 40000;
      static Operation gossipOperation = Operation.Get;
      static String gossipKey = "Global.Count";
      static String gossipValue = null;
      static HashSet<Uri> nodeSet = new HashSet<Uri>();
      static Boolean shutdown = false;

      /// <summary>
      /// Program entry point
      /// </summary>
      /// <param name="args">
      /// Program arguments
      /// </param>
      static void Main (String[] args)
      {
         Console.WriteLine("WcfEx Gossip Sample Client");
         if (!ParseArguments(args))
            ReportUsage();
         else if (gossipOperation == Operation.Get)
            Get();
         else if (gossipOperation == Operation.Set)
            Set();
         else if (gossipOperation == Operation.Mon)
            Mon();
      }
      /// <summary>
      /// Parses the command line parameters for the program
      /// </summary>
      /// <param name="args">
      /// Program arguments
      /// </param>
      /// <returns>
      /// True if the parameters are valid
      /// False otherwise
      /// </returns>
      static Boolean ParseArguments (String[] args)
      {
         // parse parameters
         if (args.Length > 0)
         {
            if (String.Compare(args[0], "get", true) == 0)
               gossipOperation = Operation.Get;
            else if (String.Compare(args[0], "set", true) == 0)
               gossipOperation = Operation.Set;
            else if (String.Compare(args[0], "mon", true) == 0)
               gossipOperation = Operation.Mon;
            else
               return false;
            if (args.Length > 1)
            {
               gossipKey = args[1];
               if (args.Length > 2)
               {
                  gossipValue = args[2];
                  if (args.Length > 3)
                     return false;
               }
            }
         }
         // validate parameters
         if (String.IsNullOrEmpty(gossipKey))
            return false;
         if (gossipOperation == Operation.Set && String.IsNullOrEmpty(gossipValue))
            return false;
         if (gossipOperation != Operation.Set && !String.IsNullOrEmpty(gossipValue))
            return false;
         return true;
      }
      /// <summary>
      /// Executes the application's query function
      /// </summary>
      static void Get ()
      {
         using (var proxy = Connect(new UriBuilder("udp", System.Net.Dns.GetHostName(), BasePort).Uri))
            Console.WriteLine("{0} = {1}", gossipKey, proxy.Server.Query(gossipKey).Formatted);
      }
      /// <summary>
      /// Executes the application's combine function
      /// </summary>
      static void Set ()
      {
         Item result = null;
         using (var proxy = Connect(new UriBuilder("udp", System.Net.Dns.GetHostName(), BasePort).Uri))
            proxy.Server.Combine(
               new Item(gossipKey) 
               { 
                  Data = new Data(gossipValue, gossipValue), 
                  Updated = DateTime.Now 
               }, 
               out result
            );
         Console.WriteLine("{0} = {1}", gossipKey, (result != null) ? result.Data.Formatted : "(null)");
      }
      /// <summary>
      /// Executes the application's monitor function
      /// </summary>
      static void Mon ()
      {
         Console.WriteLine("Starting the monitor. Press escape to exit.");
         Console.WriteLine();
         // add the default local node to the peer list
         nodeSet.Add(new UriBuilder("udp", System.Net.Dns.GetHostName(), BasePort).Uri);
         // start up the discovery thread
         var discoverer = new Thread(Discover);
         discoverer.Start();
         while (!shutdown)
         {
            // wait until the next sampling period, and
            // check to see if the user has cancelled
            System.Threading.Thread.Sleep(5000);
            if (Console.KeyAvailable)
               if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                  shutdown = true;
            if (shutdown)
               break;
            // sample the current node set discovered by the discovery thread
            var nodes = new List<Uri>(nodeSet.Count);
            lock (nodeSet)
               nodes.AddRange(nodeSet);
            // sample the values of the specified item in all of the nodes
            List<String> nominals = new List<String>(nodes.Count);
            List<Double> ordinals = new List<Double>(nodes.Count);
            foreach (var nodeID in nodes)
            {
               if (Console.KeyAvailable)
                  if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                     shutdown = true;
               if (shutdown)
                  break;
               using (var proxy = Connect(nodeID))
               {
                  // query the node for the item value
                  // remove a node as soon as we are
                  // unable to communicate with it, to
                  // avoid blocking the next sample
                  try
                  { 
                     var data = proxy.Server.Query(gossipKey);
                     var nominal = Convert.ToString(data.Value);
                     var ordinal = 0.0d;
                     // add the ordinal or nominal value to the
                     // list of sampled item values, depending on its type
                     if (Double.TryParse(nominal, out ordinal))
                        ordinals.Add(ordinal);
                     else
                        nominals.Add(nominal = data.Formatted);
                  }
                  catch
                  { 
                     lock (nodeSet) 
                        if (nodeSet.Count > 1)
                           nodeSet.Remove(nodeID);
                  }
               }
            }
            // report the item statistics
            // . range/mean/deviation for ordinals
            // . frequency distribution for nominals
            Console.WriteLine("Item:  {0}", gossipKey);
            Console.WriteLine("Count: {0}", ordinals.Any() ? ordinals.Count : nominals.Count);
            if (ordinals.Any())
            {
               Double min = ordinals.Min();
               Double max = ordinals.Max();
               Double mean = ordinals.Average();
               Double stdev = Math.Sqrt(ordinals.Average(o => (o - mean) * (o - mean)));
               Console.WriteLine("Min:   {0:0.000000}", min);
               Console.WriteLine("Max:   {0:0.000000}", max);
               Console.WriteLine("Mean:  {0:0.000000}", mean);
               Console.WriteLine("Stdev: {0:0.000000}", stdev);
            }
            else
            {
               Dictionary<String, Int32> freq = new Dictionary<String, Int32>();
               foreach (var nominal in nominals)
                  freq[nominal] = (freq.ContainsKey(nominal)) ? freq[nominal] + 1 : 1;
               foreach (var nomfreq in freq.OrderByDescending(n => n.Value))
                  Console.WriteLine("{0}: {1}", nomfreq.Key, nomfreq.Value);
            }
            Console.WriteLine();
         }
         discoverer.Join();
      }
      /// <summary>
      /// Node discovery thread
      /// </summary>
      /// <remarks>.
      /// This thread runs continuously when monitoring, connecting 
      /// to known peers and attempting to discover their peers until
      /// all peers are known or shutdown is signaled.
      /// </remarks>
      static void Discover ()
      {
         var queue = new Queue<Uri>();
         while (!shutdown)
         {
            var outerCount = nodeSet.Count;
            // add the existing list of nodes to the query queue
            foreach (var nodeID in nodeSet)
               queue.Enqueue(nodeID);
            while (!shutdown && queue.Any())
            {
               var nodeID = queue.Dequeue();
               var innerCount = nodeSet.Count;
               using (var proxy = Connect(nodeID))
               {
                  while (!shutdown)
                  {
                     try
                     { 
                        // attempt to retrieve a peer from the
                        // specified node, and add it to the list
                        // break on the first duplicate, so that
                        // other nodes can be queried
                        var peerID = proxy.Server.SelectPeer();
                        if (peerID == null)
                           break;
                        lock (nodeSet)
                           if (nodeSet.Add(peerID))
                              queue.Enqueue(peerID);
                           else
                              break;
                     }
                     catch
                     {
                        // remove a node as soon as we are
                        // unable to communicate with it, to
                        // avoid blocking the monitor
                        lock (nodeSet)
                           if (nodeSet.Count > 1)
                              nodeSet.Remove(nodeID);
                        break;
                     }
                  }
               }
               if (innerCount == nodeSet.Count)
                  Thread.Sleep(10);
            }
            if (outerCount == nodeSet.Count)
               Thread.Sleep(1000);
         }
      }
      /// <summary>
      /// Connects to a gossip node instance
      /// </summary>
      /// <param name="nodeID">
      /// The ID of the node to connect
      /// </param>
      /// <returns>
      /// A WCF client instance for the node
      /// </returns>
      static Client<INode> Connect (Uri nodeID)
      {
         return new Client<INode>(
            new Udp.Binding()
            {
               SendTimeout = TimeSpan.FromSeconds(1)
            },
            nodeID
         );
      }
      /// <summary>
      /// Displays a program usage message
      /// </summary>
      static void ReportUsage ()
      {
         Console.WriteLine("   Usage: GossipClient [mode] [key] [value]");
         Console.WriteLine("      mode:    program mode (get/set/mon, default: get)");
         Console.WriteLine("      key:     the gossip key to monitor (monitor default: node count)");
         Console.WriteLine("      value:   the key value to assign (for set operations)");
      }
   }
}
