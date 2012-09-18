//===========================================================================
// MODULE:  Client.cs
// PURPOSE: generic WCF simplex client wrapper
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
   /// Generic WCF client wrapper, for simplex interfaces
   /// </summary>
   /// <typeparam name="TContract">
   /// The service contract interface type
   /// </typeparam>
   /// <remarks>
   /// This class serves as a wrapper around the ClientBase class for a WCF
   /// interface, allowing client applications to interact with a WCF
   /// service without having to use the WCF contract code generator. It
   /// also provides dispose semantics for deterministic channel cleanup.
   /// </remarks>
   public class Client<TContract> : ClientBase<TContract>, IDisposable 
      where TContract : class
   {
      #region Construction/Disposal
      /// <summary>
      /// Initializes a new client instance
      /// </summary>
      public Client ()
      {
      }
      /// <summary>
      /// Initializes a new client instance
      /// </summary>
      /// <param name="endpointName">
      /// The name of the configured endpoint to use to
      /// connect to the WCF service
      /// </param>
      public Client (String endpointName)
         : base(endpointName)
      {
      }
      /// <summary>
      /// Initializes a new client instance
      /// </summary>
      /// <param name="endpointName">
      /// The name of the configured endpoint to use to
      /// connect to the WCF service
      /// </param>
      /// <param name="address">
      /// The remote address of the WCF service
      /// </param>
      public Client (String endpointName, Uri address)
         : base(endpointName, new EndpointAddress(address))
      {
      }
      /// <summary>
      /// Initializes a new client instance
      /// </summary>
      /// <param name="binding">
      /// The channel binding for the WCF service
      /// </param>
      /// <param name="address">
      /// The remote address of the WCF service
      /// </param>
      public Client (Binding binding, Uri address)
         : base(binding, new EndpointAddress(address))
      {
      }
      /// <summary>
      /// Initializes a new client instance
      /// </summary>
      /// <param name="binding">
      /// The channel binding for the WCF service
      /// </param>
      /// <param name="address">
      /// The remote address of the WCF service
      /// </param>
      public Client (Binding binding, String address)
         : base(binding, new EndpointAddress(address))
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
         get
         {
            // we must explicitly open the proxy to avoid serializing 
            // all calls made through a shared client instance
            // http://blogs.msdn.com/b/wenlong/archive/2007/10/26/best-practice-always-open-wcf-client-proxy-explicitly-when-it-is-shared.aspx
            if (base.State != CommunicationState.Opened)
               lock (this)
                  if (base.State != CommunicationState.Opened)
                     base.Open();
            return base.Channel;
         }
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
