//===========================================================================
// MODULE:  DuplexClient.cs
// PURPOSE: generic WCF full-duplex client wrapper
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

namespace WcfEx
{
   /// <summary>
   /// Generic WCF client wrapper, for duplex interfaces
   /// </summary>
   /// <typeparam name="TContract">
   /// The service contract interface type
   /// </typeparam>
   /// <remarks>
   /// This class serves as a wrapper around the DuplexClientBase class for a 
   /// WCF interface, allowing client applications to interact with a WCF
   /// service without having to use the WCF contract code generator. It
   /// also provides dispose semantics for deterministic channel cleanup.
   /// </remarks>
   public class DuplexClient<TContract, TCallback> : DuplexClientBase<TContract>, IDisposable
      where TContract : class 
      where TCallback : class
   {
      #region Construction/Disposal
      /// <summary>
      /// Initializes a new client instance
      /// </summary>
      /// <param name="callback">
      /// The client callback interface
      /// </param>
      public DuplexClient (TCallback callback)
         : base(callback)
      {
      }
      /// <summary>
      /// Initializes a new client instance
      /// </summary>
      /// <param name="callback">
      /// The client callback interface
      /// </param>
      /// <param name="endpointName">
      /// The name of the configured endpoint to use to
      /// connect to the WCF service
      /// </param>
      public DuplexClient (TCallback callback, String endpointName)
         : base(callback, endpointName)
      {
      }
      /// <summary>
      /// Initializes a new client instance
      /// </summary>
      /// <param name="callback">
      /// The client callback interface
      /// </param>
      /// <param name="endpointName">
      /// The name of the configured endpoint to use to
      /// connect to the WCF service
      /// </param>
      /// <param name="address">
      /// The remote address of the WCF service
      /// </param>
      public DuplexClient (
         TCallback callback, 
         String endpointName,
         Uri address)
         : base(callback, endpointName, new EndpointAddress(address))
      {
      }
      /// <summary>
      /// Initializes a new client instance
      /// </summary>
      /// <param name="callback">
      /// The client callback interface
      /// </param>
      /// <param name="binding">
      /// The channel binding for the WCF service
      /// </param>
      /// <param name="address">
      /// The remote address of the WCF service
      /// </param>
      public DuplexClient (TCallback callback, Binding binding, Uri address)
         : base(callback, binding, new EndpointAddress(address))
      {
      }
      /// <summary>
      /// Initializes a new client instance
      /// </summary>
      /// <param name="callback">
      /// The client callback interface
      /// </param>
      /// <param name="binding">
      /// The channel binding for the WCF service
      /// </param>
      /// <param name="address">
      /// The remote address of the WCF service
      /// </param>
      public DuplexClient (TCallback callback, Binding binding, String address)
         : base(callback, binding, new EndpointAddress(address))
      {
      }
      /// <summary>
      /// Initializes a new client instance
      /// </summary>
      /// <param name="context">
      /// The duplex service instance context that
      /// maintains the client callback
      /// </param>
      public DuplexClient (InstanceContext context) 
         : base(context)
      {
      }
      /// <summary>
      /// Initializes a new client instance
      /// </summary>
      /// <param name="context">
      /// The duplex service instance context that
      /// maintains the client callback
      /// </param>
      /// <param name="endpointName">
      /// The name of the configured endpoint to use to
      /// connect to the WCF service
      /// </param>
      public DuplexClient (InstanceContext context, String endpointName)
         : base(context, endpointName)
      {
      }
      /// <summary>
      /// Initializes a new client instance
      /// </summary>
      /// <param name="context">
      /// The duplex service instance context that
      /// maintains the client callback
      /// </param>
      /// <param name="binding">
      /// The channel binding for the WCF service
      /// </param>
      /// <param name="address">
      /// The remote address of the WCF service
      /// </param>
      public DuplexClient (InstanceContext context, Binding binding, Uri address)
         : base(context, binding, new EndpointAddress(address))
      {
      }
      /// <summary>
      /// Initializes a new client instance
      /// </summary>
      /// <param name="context">
      /// The duplex service instance context that
      /// maintains the client callback
      /// </param>
      /// <param name="binding">
      /// The channel binding for the WCF service
      /// </param>
      /// <param name="address">
      /// The remote address of the WCF service
      /// </param>
      public DuplexClient (InstanceContext context, Binding binding, String address)
         : base(context, binding, new EndpointAddress(address))
      {
      }
      /// <summary>
      /// Deterministically closes the service channel
      /// </summary>
      public void Dispose ()
      {
         try
         {
            Close();
         }
         catch (CommunicationException)
         {
            Abort();
         }
         catch (TimeoutException)
         {
            Abort();
         }
         catch (Exception)
         {
            Abort();
            throw;
         }
      }
      #endregion

      #region Properties
      /// <summary>
      /// The interface to the WCF service contract
      /// </summary>
      public TContract Server
      {
         get { return base.Channel; }
      }
      /// <summary>
      /// The IContextChannel interface, for setting up
      /// method-spanning operation scopes
      /// </summary>
      public IContextChannel Context
      {
         get { return (IContextChannel)this.Server; }
      }
      #endregion
   }
}
