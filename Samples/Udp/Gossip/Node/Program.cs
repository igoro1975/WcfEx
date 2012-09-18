//===========================================================================
// MODULE:  Program.cs
// PURPOSE: UDP gossip sample node program
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
using System.Collections.Concurrent;
using System.Configuration;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;
// Project References

namespace WcfEx.Samples.Gossip
{
   /// <summary>
   /// The gossip node program
   /// </summary>
   /// <remarks>
   /// This program provides the ability to start one or more gossip node
   /// instances on the local machine. Starting multiple instances is useful
   /// for testing and experimentation without requiring virtual/physical
   /// machines for each instance. Each node is allocated a UDP port 
   /// (offset from the basePort configuration value) for listening on the
   /// gossip contract.
   /// For remote joining, the node program accepts a peer host parameter,
   /// which indicates a remote machine whose root node (at base port) should
   /// be joined as a peer of the new root node.
   /// When launching multiple nodes, the application automatically joins child
   /// nodes to a parent node using a heap-like allocation, where each child node
   /// has one initial peer, where 
   ///   parentIdx = (nodeIdx - 1) / MaxPeers
   /// </remarks>
   class Program
   {
      static Int32 NodeCount = 1;
      static String PeerHost = null;
      static ConcurrentQueue<String> messages = new ConcurrentQueue<String>();

      /// <summary>
      /// Program entry point
      /// </summary>
      /// <param name="args">
      /// Program arguments
      /// </param>
      static void Main (String[] args)
      {
         Console.WriteLine("WcfEx Gossip Sample Node");
         if (!ParseArguments(args))
            ReportUsage();
         else
            StartNodes();
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
         for (var i = 0; i < args.Length; i++)
         {
            var arg = args[i++].ToLower();
            if (arg[0] != '/' && arg[0] != '-')
               return false;
            if (i >= args.Length)
               return false;
            var val = args[i];
            switch (Char.ToLower(arg[1]))
            {
               case 'n':
                  if (!Int32.TryParse(val, out NodeCount))
                     return false;
                  break;
               case 'p':
                  if (String.IsNullOrWhiteSpace(val))
                     return false;
                  PeerHost = val;
                  break;
               default:
                  return false;
            }
         }
         // validate parameters
         if (NodeCount <= 0)
            return false;
         return true;
      }
      /// <summary>
      /// Starts up the configured number of node instances
      /// </summary>
      static void StartNodes ()
      {
         Console.Write("Starting up {0} node servers...", NodeCount);
         var hosts = new List<ServiceHost>();
         try
         {
            for (var i = 0; i < NodeCount; i++)
               hosts.Add(StartNode(i));
            Console.WriteLine("done.");
            Console.WriteLine("Press escape to exit.");
            // trace any recorded database events,
            // and wait until the user terminates
            for ( ; ; )
            {
               String message;
               while (messages.TryDequeue(out message))
                  Trace.TraceInformation(message);
               if (Console.KeyAvailable)
                  if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                     break;
               System.Threading.Thread.Sleep(10);
            }
         }
         finally
         {
            foreach (var host in hosts)
               try { host.Close(); }
               catch { }
         }
      }
      /// <summary>
      /// Starts up a single gossip node instance
      /// </summary>
      /// <param name="nodeIdx">
      /// The zero-based node index
      /// </param>
      /// <returns>
      /// The WCF service host for the instance
      /// </returns>
      static ServiceHost StartNode (Int32 nodeIdx)
      {
         // construct the node/parent address URIs
         var parentIdx = (nodeIdx - 1) / Config.Instance.MaxPeersPerNode;
         var nodeAddress = new UriBuilder(
            "udp", 
            System.Net.Dns.GetHostName(), 
            Config.Instance.BasePort + nodeIdx
         ).Uri;
         var parentAddress = (nodeIdx == 0) ?
            (PeerHost != null) ? 
               new UriBuilder("udp", PeerHost, Config.Instance.BasePort).Uri : 
               null :
            new UriBuilder(
               "udp", 
               System.Net.Dns.GetHostName(), 
               Config.Instance.BasePort + parentIdx
            ).Uri;
         // create and configure the current node's database, and
         // create and register the configured item combinators
         var db = new Database();
         db.OnCombined += item => HandleCombined(nodeIdx, item);
         foreach (var config in Config.Instance.Combinators)
            db.Register(config.Namespace, CreateCombinator(config));
         // create the node and the WCF service host, 
         // and listen on the node's address
         var node = new Node(nodeAddress, parentAddress, db);
         var wcfHost = new ServiceHost(node);
         wcfHost.AddServiceEndpoint(
            typeof(INode),
            new Udp.Binding(),
            nodeAddress
         );
         wcfHost.Open();
         return wcfHost;
      }
      /// <summary>
      /// Creates and configures a gossip item combinator
      /// </summary>
      /// <param name="config">
      /// Combinator configuration
      /// </param>
      /// <returns>
      /// The initializes combinator instance
      /// </returns>
      static ICombinator CreateCombinator (Config.Combinator config)
      {
         // create the new combinator instance
         var type = Type.GetType(config.ConfigType, true);
         var combinator = (ICombinator)Activator.CreateInstance(type);
         // configure the combinator's properties
         foreach (var prop in config.Properties)
         {
            try
            {
               // reflect the configured property
               var propFlags =
                  BindingFlags.Public |
                  BindingFlags.Instance |
                  BindingFlags.FlattenHierarchy |
                  BindingFlags.IgnoreCase |
                  BindingFlags.ExactBinding;
               var rtProp = type.GetProperty(prop.Key, propFlags);
               if (rtProp != null)
               {
                  // if the configured property exists, then type-convert
                  // it from the configured string value
                  var converter = TypeDescriptor.GetConverter(rtProp.PropertyType);
                  rtProp.SetValue(combinator, converter.ConvertFrom(prop.Value), null);
               }
            }
            catch (Exception e)
            {
               throw new ConfigurationErrorsException(
                  String.Format("Error configuring combinator property {0}", prop.Key),
                  e
               );
            }
         }
         return combinator;
      }
      /// <summary>
      /// Gossip database combined event handler
      /// </summary>
      /// <param name="nodeIdx">
      /// The current node index
      /// </param>
      /// <param name="item">
      /// The combined item
      /// </param>
      static void HandleCombined (Int32 nodeIdx, Item item)
      {
         messages.Enqueue(
            String.Format(
               "{0:hh:mm:ss.fff} - node: {1,3}, contagion: {2:+0.000;-0.000}, formatted: {3,10}, raw: {4}",
               item.Updated,
               nodeIdx,
               item.Contagion,
               item.Data.Formatted,
               item.Data.Value
            )
         );
      }
      /// <summary>
      /// Displays a program usage message
      /// </summary>
      static void ReportUsage ()
      {
         Console.WriteLine("   Usage: GossipNode [/s {node-count}] [/o {node-order}] [/p {peer-host}]");
         Console.WriteLine("      node-count: number of node instances to start (default/min: 1)");
         Console.WriteLine("      peer-host:  a remote host name on an existing peer network");
      }
   }
}
