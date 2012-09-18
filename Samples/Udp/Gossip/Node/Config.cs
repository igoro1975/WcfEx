//===========================================================================
// MODULE:  Config.cs
// PURPOSE: UDP gossip node configuration
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
using System.Configuration;
using System.Linq;
// Project References

namespace WcfEx.Samples.Gossip
{
   /// <summary>
   /// Gossiping configuration
   /// </summary>
   /// <remarks>
   /// This class encapsulates the configuration parameters in the node's
   /// app.config file. Node configuration includes various parameters
   /// described below, including the list of combinators to register
   /// when starting up new nodes.
   /// </remarks>
   public sealed class Config : ConfigurationSection
   {
      #region Configuration Properties
      /// <summary>
      /// The base UDP port number to use for launching node instances
      /// </summary>
      [ConfigurationProperty("basePort", DefaultValue = 40000)]
      public Int32 BasePort { get { return (Int32)base["basePort"]; } }
      /// <summary>
      /// The list of registered combinator configurations,
      /// specifying which combinators to create when starting the node
      /// </summary>
      [ConfigurationProperty("combinators")]
      public Combinator.Collection ConfigCombinators { get { return (Combinator.Collection)base["combinators"]; } }
      /// <summary>
      /// The maximum number of peer connections allowed per node
      /// </summary>
      [ConfigurationProperty("maxPeersPerNode", DefaultValue = 10)]
      public Int32 MaxPeersPerNode { get { return (Int32)base["maxPeersPerNode"]; } }
      /// <summary>
      /// The timeout, in milliseconds, used to resolve deadlocks
      /// when combinding items among peers
      /// </summary>
      [ConfigurationProperty("deadlockTimeout", DefaultValue = 100)]
      public Int32 DeadlockTimeout { get { return (Int32)base["deadlockTimeout"]; } }
      /// <summary>
      /// The node timer wakup interval, in milliseconds
      /// </summary>
      [ConfigurationProperty("wakeupInterval", DefaultValue = 500)]
      public Int32 WakeupInterval { get { return (Int32)base["wakeupInterval"]; } }
      /// <summary>
      /// The probability [0.0-1.0) that a node will join with a peer
      /// requesting a pull operation
      /// </summary>
      [ConfigurationProperty("pullJoinProbability", DefaultValue = 0.5d)]
      public Double PullJoinProbability { get { return (Double)base["pullJoinProbability"]; } }
      #endregion

      #region Runtime Properties
      /// <summary>
      /// The configuration singleton instance
      /// </summary>
      public static Config Instance
      { 
         get { return (Config)ConfigurationManager.GetSection("gossip"); } 
      }
      /// <summary>
      /// The list of registered combinator configurations
      /// </summary>
      public IEnumerable<Combinator> Combinators
      {
         get { return this.ConfigCombinators.Cast<Combinator>(); }
      }
      #endregion

      /// <summary>
      /// Item combinator configuration
      /// </summary>
      /// <remarks>
      /// This class specifies the CLR type and configuration properties
      /// for a combinator to add to the node's gossip database. Combinators
      /// must implement the ICombinator interface.
      /// </remarks>
      public sealed class Combinator : ConfigurationElement
      {
         #region Configuration Properties
         /// <summary>
         /// The combinator's namespace, for item name isolation
         /// </summary>
         [ConfigurationProperty("namespace", DefaultValue = "Global")]
         public String Namespace
         {
            get { return (String)base["namespace"]; }
         }
         /// <summary>
         /// The combinator's managed type name
         /// </summary>
         [ConfigurationProperty("type", IsRequired = true)]
         public String ConfigType 
         { 
            get { return (String)base["type"]; } 
         }
         /// <summary>
         /// The list of properties to configure on the combinator
         /// </summary>
         [ConfigurationProperty("properties")]
         public NameValueConfigurationCollection ConfigProperties
         { 
            get { return (NameValueConfigurationCollection)base["properties"]; } 
         }
         #endregion

         #region Runtime Properties
         /// <summary>
         /// The list of runtime properties to assign to the combinator
         /// </summary>
         public new IEnumerable<KeyValuePair<String, String>> Properties
         {
            get
            { 
               return this.ConfigProperties
                  .Cast<NameValueConfigurationElement>()
                  .Select(p => new KeyValuePair<String, String>(p.Name, p.Value));
            }
         }
         #endregion

         /// <summary>
         /// The combinator configuration collection
         /// </summary>
         public sealed class Collection : ConfigurationElementCollection
         {
            /// <summary>
            /// Retrieves the combinator's configuration key
            /// </summary>
            /// <param name="element">
            /// The combinator configuration element
            /// </param>
            /// <returns>
            /// The combinator key
            /// </returns>
            protected override Object GetElementKey (ConfigurationElement element)
            {
               return ((Combinator)element).ConfigType;
            }
            /// <summary>
            /// Creates a new combinator configuration element
            /// </summary>
            /// <returns>
            /// The new combinator configuration element
            /// </returns>
            protected override ConfigurationElement CreateNewElement ()
            {
               return new Combinator();
            }
         }
      }
   }
}
