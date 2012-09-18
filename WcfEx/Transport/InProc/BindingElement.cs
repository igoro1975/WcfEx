//===========================================================================
// MODULE:  BindingElement.cs
// PURPOSE: In-process WCF binding element class
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
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
// Project References

namespace WcfEx.InProc
{
   /// <summary>
   /// InProc WCF binding configuration
   /// </summary>
   /// <remarks>
   /// This class represents the configuration properties for
   /// the in-process transport layer, providing the ability
   /// to create channel factories and listeners for the
   /// in-process communication model.
   /// </remarks>
   /// <example>
   ///   <system.serviceModel>
   ///      <extensions>
   ///         <bindingElementExtensions>
   ///            <add name="inProcTransport" type="WcfEx.InProc.BindingElement+Config,WcfEx"/>
   ///         </bindingElementExtensions>
   ///      </extensions>
   ///      <bindings>
   ///         <customBinding>
   ///            <binding name="inProc">
   ///               <inProcTransport/>
   ///            </binding>
   ///         </customBinding>
   ///      </bindings>
   ///      ...
   ///   </system.serviceModel>
   /// </example>
   public sealed class BindingElement : TransportBindingElement
   {
      #region BindingElement Overrides
      /// <summary>
      /// Creates a deep copy of the binding configuration
      /// </summary>
      /// <returns>
      /// The copied binding element object
      /// </returns>
      public override System.ServiceModel.Channels.BindingElement Clone()
      {
         return new BindingElement();
      }
      #endregion

      #region TransportBindingElement Overrides
      /// <summary>
      /// The URI scheme for inProc addresses
      /// </summary>
      public override String Scheme
      {
         get { return "inProc"; }
      }
      /// <summary>
      /// Specifies which channel factory (client) types 
      /// are supported by the inProc transport layer
      /// </summary>
      /// <typeparam name="TChannel">
      /// The requested channel interface type
      /// </typeparam>
      /// <param name="context">
      /// WCF binding context
      /// </param>
      /// <returns>
      /// True if the specified factory type is supported
      /// False otherwise
      /// </returns>
      public override Boolean CanBuildChannelFactory<TChannel> (BindingContext context)
      {
         if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            return true;
         return false;
      }
      /// <summary>
      /// Constructs an inProc channel factory (client)
      /// </summary>
      /// <typeparam name="TChannel">
      /// The type of factory requested by WCF
      /// </typeparam>
      /// <param name="context">
      /// WCF binding context
      /// </param>
      /// <returns>
      /// The new channel factory
      /// </returns>
      public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (BindingContext context)
      {
         if (!CanBuildChannelFactory<TChannel>(context))
            throw new InvalidOperationException(String.Format("Channel type {0} not supported", typeof(TChannel)));
         return new Factory(this, context) as IChannelFactory<TChannel>;
      }
      /// <summary>
      /// Specifies which channel listener (server) types 
      /// are supported by the inProc transport layer
      /// </summary>
      /// <typeparam name="TChannel">
      /// The requested channel interface type
      /// </typeparam>
      /// <param name="context">
      /// WCF binding context
      /// </param>
      /// <returns>
      /// True if the specified listener type is supported
      /// False otherwise
      /// </returns>
      public override Boolean CanBuildChannelListener<TChannel> (BindingContext context)
      {
         if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            return true;
         return false;
      }
      /// <summary>
      /// Constructs an inProc channel listener (server)
      /// </summary>
      /// <typeparam name="TChannel">
      /// The type of listener requested by WCF
      /// </typeparam>
      /// <param name="context">
      /// WCF binding context
      /// </param>
      /// <returns>
      /// The new channel factory
      /// </returns>
      public override IChannelListener<TChannel> BuildChannelListener<TChannel> (BindingContext context)
      {
         if (!CanBuildChannelListener<TChannel>(context))
            throw new InvalidOperationException(String.Format("Channel type {0} not supported", typeof(TChannel)));
         return new Listener(this, context) as IChannelListener<TChannel>;
      }
      #endregion

      /// <summary>
      /// Configuration extension element for the 
      /// in-process binding element
      /// </summary>
      public sealed class Config : BindingElementExtension<BindingElement>
      {
      }
   }
}
