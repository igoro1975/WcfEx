//===========================================================================
// MODULE:  Database.cs
// PURPOSE: UDP gossip sample node database
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
using System.Linq;
using System.Threading;
// Project References

namespace WcfEx.Samples.Gossip
{
   /// <summary>
   /// Node item database
   /// </summary>
   /// <remarks>
   /// This class maintains a registration of configured combinators,
   /// as well as the list of items for the current node. It provides
   /// fullly concurrent access to the item database.
   /// </remarks>
   public sealed class Database
   {
      // registered namespace->combinator mapping
      private ConcurrentDictionary<String, ICombinator> combinators =
         new ConcurrentDictionary<String, ICombinator>(StringComparer.OrdinalIgnoreCase);
      // key->item mapping
      private ConcurrentDictionary<String, Entry> entries =
         new ConcurrentDictionary<String, Entry>(StringComparer.OrdinalIgnoreCase);

      #region Events
      /// <summary>
      /// The item combined successfully event
      /// </summary>
      public event Action<Item> OnCombined;
      #endregion

      #region Operations
      /// <summary>
      /// Registers a combinator with the database
      /// </summary>
      /// <param name="ns">
      /// The combinator namespace
      /// </param>
      /// <param name="combinator">
      /// The combinator to register
      /// </param>
      public void Register (String ns, ICombinator combinator)
      {
         if (ns.Contains('.'))
            throw new ArgumentException("ns");
         if (combinator == null)
            throw new ArgumentNullException("combinator");
         this.combinators[GetCombinatorKey(ns, combinator)] = combinator;
      }
      /// <summary>
      /// Invokes all combinators to generate new items
      /// </summary>
      public void Generate ()
      {
         var generated = this.combinators.SelectMany(
            e => e.Value
               .Generate(GetCombinatorNamespace(e.Key))
               .Select(i => new Entry() { Combinator = e.Value, Item = i })
         );
         foreach (var entry in generated)
            this.entries.TryAdd(entry.Item.Key, entry);
      }
      /// <summary>
      /// Retrieves the full list of items from the
      /// database
      /// </summary>
      /// <returns>
      /// The list of items currently stored
      /// </returns>
      public IList<Item> List ()
      {
         return this.entries.Values.Select(e => e.Item).ToList();
      }
      /// <summary>
      /// Queries the database for an item value
      /// </summary>
      /// <param name="key">
      /// The item key to query
      /// </param>
      /// <returns>
      /// The key's value if found
      /// Data.Empty otherwise
      /// </returns>
      public Data Query (String key)
      {
         Entry entry;
         if (this.entries.TryGetValue(key, out entry))
            lock (entry.Item)
               return entry.Item.Data;
         return Data.Empty;
      }
      /// <summary>
      /// Combines a database item with an input item
      /// </summary>
      /// <param name="input">
      /// The input item to combine
      /// </param>
      /// <param name="output">
      /// Return the result of the combination via here
      /// </param>
      /// <returns>
      /// True if the combination was successful
      /// False otherwise
      /// </returns>
      public Boolean Combine (Item input, out Item output)
      {
         output = null;
         // locate the entry/combinator for the item
         // if not found, attempt to add it
         var entry = default(Entry);
         if (!this.entries.TryGetValue(input.Key, out entry))
         {
            ICombinator combinator = null;
            if (!this.combinators.TryGetValue(GetItemNamespace(input.Key), out combinator))
               return false;
            entry = new Entry()
            {
               Combinator = combinator,
               Item = combinator.Create(input.Key)
            };
            if (!this.entries.TryAdd(input.Key, entry))
               entry = this.entries[input.Key];
         }
         // combine the input with the item
         var combined = false;
         if (Monitor.TryEnter(entry.Item, Config.Instance.DeadlockTimeout))
            try { combined = entry.Combinator.Combine(entry.Item, input, out output); }
            finally { Monitor.Exit(entry.Item); }
         // notify the combine completion
         if (combined && this.OnCombined != null)
            this.OnCombined(entry.Item);
         return combined;
      }
      /// <summary>
      /// Constructs a combinator mapping key
      /// </summary>
      /// <param name="ns">
      /// The combinator namespace
      /// </param>
      /// <param name="combinator">
      /// The combinator instance
      /// </param>
      /// <returns>
      /// The constructed mapping key
      /// </returns>
      private String GetCombinatorKey (String ns, ICombinator combinator)
      {
         return String.Format("{0}.{1}", ns, combinator.GetType().FullName);
      }
      /// <summary>
      /// Decodes the namespace from a combinator key
      /// </summary>
      /// <param name="key">
      /// The combinator key
      /// </param>
      /// <returns>
      /// The registered namespace for the combinator
      /// </returns>
      private String GetCombinatorNamespace (String key)
      {
         Int32 nameIdx = key.IndexOf('.');
         if (nameIdx == -1)
            throw new ArgumentException("key");
         return key.Substring(0, nameIdx);
      }
      /// <summary>
      /// Decodes the namespace from an item key
      /// </summary>
      /// <param name="key">
      /// The item key
      /// </param>
      /// <returns>
      /// The namespace for the item
      /// </returns>
      private String GetItemNamespace (String key)
      {
         Int32 nameIdx = key.LastIndexOf('.');
         if (nameIdx == -1)
            throw new ArgumentException("key");
         return key.Substring(0, nameIdx);
      }
      /// <summary>
      /// Decodes the name from an item key
      /// </summary>
      /// <param name="key">
      /// The item key
      /// </param>
      /// <returns>
      /// The local name for the item
      /// </returns>
      private String GetItemName (String key)
      {
         Int32 nameIdx = key.LastIndexOf('.');
         if (nameIdx == -1)
            throw new ArgumentException("key");
         return key.Substring(nameIdx + 1);
      }
      #endregion

      /// <summary>
      /// The item dictionary mapping entry
      /// </summary>
      private struct Entry
      {
         public ICombinator Combinator;
         public Item Item;
      }
   }
}
