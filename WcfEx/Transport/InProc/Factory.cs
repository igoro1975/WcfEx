//===========================================================================
// MODULE:  Factory.cs
// PURPOSE: In-process WCF channel factory class
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
using System.ServiceModel;
using System.ServiceModel.Channels;
// Project References

namespace WcfEx.InProc
{
   /// <summary>
   /// InProc channel factory
   /// </summary>
   /// <remarks>
   /// This class provides support for creating client
   /// channels for submitting requests to services
   /// hosted in-process.
   /// </remarks>
   internal sealed class Factory : ChannelFactory<IDuplexSessionChannel>
   {
      #region Construction/Disposal
      /// <summary>
      /// Initializes a new factory instance
      /// </summary>
      /// <param name="binding">
      /// The in-process binding element
      /// </param>
      /// <param name="context">
      /// The requested WCF binding context
      /// </param>
      public Factory (BindingElement element, BindingContext context) : base(context)
      {
      }
      #endregion

      #region ChannelFactory Overrides
      /// <summary>
      /// Factory initialization callback
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the open operation
      /// </param>
      protected override void OnOpen (TimeSpan timeout)
      {
      }
      /// <summary>
      /// Factory graceful shutdown callback
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the close operation
      /// </param>
      protected override void OnClose (TimeSpan timeout)
      {
      }
      /// <summary>
      /// Channel creation callback
      /// </summary>
      /// <param name="address">
      /// The endpoint address to connect
      /// </param>
      /// <param name="via">
      /// The transport address to connect
      /// </param>
      /// <returns>
      /// The connected client channel
      /// </returns>
      protected override IDuplexSessionChannel OnCreateChannel (EndpointAddress address, Uri via)
      {
         if (via != null && via != address.Uri)
            throw new ArgumentException("via");
         return new Channel(this, Broker.Connect(address), address);
      }
      #endregion
   }
}
