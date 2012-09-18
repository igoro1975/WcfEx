//===========================================================================
// MODULE:  Broker.cs
// PURPOSE: In-process WCF session broker class
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
using System.Collections.Concurrent;
using System.ServiceModel;
// Project References

namespace WcfEx.InProc
{
   /// <summary>
   /// InProc session broker
   /// </summary>
   /// <remarks>
   /// This class maintains a mapping of address URIs to 
   /// channel listener instances, allowing the channel factory
   /// to locate an in-process listener to connect to.
   /// </remarks>
   internal static class Broker
   {
      private static ConcurrentDictionary<Uri, Listener> listeners =
         new ConcurrentDictionary<Uri, Listener>();

      /// <summary>
      /// Registers a listener instance with the broker
      /// </summary>
      /// <param name="address">
      /// The address of the listener
      /// </param>
      /// <param name="listener">
      /// The listener to register
      /// </param>
      public static void Listen (Uri address, Listener listener)
      {
         if (!listeners.TryAdd(address, listener))
            throw new InvalidOperationException(
               String.Format("A listener already exists for address {0}", address)
            );
      }
      /// <summary>
      /// Removes a listener from the broker
      /// </summary>
      /// <param name="address">
      /// The address to remove
      /// </param>
      public static void Unlisten (Uri address)
      {
         Listener listener;
         listeners.TryRemove(address, out listener);
      }
      /// <summary>
      /// Establishes a new in-process session
      /// </summary>
      /// <param name="address">
      /// The server address to connect
      /// </param>
      /// <returns>
      /// The client end of the session
      /// </returns>
      public static Session Connect (EndpointAddress address)
      {
         // retrieve a listener for the specified address
         Listener listener;
         if (!listeners.TryGetValue(address.Uri, out listener))
            throw new InvalidOperationException(
               String.Format("No listener found for address {0}", address.Uri)
            );
         // establish the new server and client session
         Session session = new Session();
         listener.Accept(session);
         return session.Connect();
      }
   }
}
