//===========================================================================
// MODULE:  BindingElement.cs
// PURPOSE: UDP WCF binding element class
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
using System.Configuration;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
// Project References

namespace WcfEx.Udp
{
   /// <summary>
   /// User Datagram Protocol (UDP) WCF binding element
   /// </summary>
   /// <remarks>
   /// This class represents the <netUdpTransport/> WCF binding extension
   /// element within a custom binding configuration element. It serves as
   /// the entry point between WCF and the UDP transport layer.
   /// </remarks>
   /// <example>
   ///   <system.serviceModel>
   ///      <extensions>
   ///         <bindingElementExtensions>
   ///            <add name="netUdpTransport" type="WcfEx.Udp.BindingElement+Config,WcfEx"/>
   ///         </bindingElementExtensions>
   ///      </extensions>
   ///      <bindings>
   ///         <customBinding>
   ///            <binding name="udp">
   ///               <netUdpTransport/>
   ///            </binding>
   ///         </customBinding>
   ///      </bindings>
   ///      ...
   ///   </system.serviceModel>
   /// </example>
   public sealed class BindingElement : TransportBindingElement
   {
      public const Int32 DefaultMaxBufferPoolSize = 524288;
      public const Int32 DefaultMaxReceivedMessageSize = 1472;
      public const Int32 DefaultSendBufferSize = 8192;
      public const Int32 DefaultReceiveBufferSize = 65536;

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new configuration instance
      /// </summary>
      public BindingElement ()
      {
         base.MaxBufferPoolSize = DefaultMaxBufferPoolSize;
         base.MaxReceivedMessageSize = DefaultMaxReceivedMessageSize;
         this.SendBufferSize = DefaultSendBufferSize;
         this.ReceiveBufferSize = DefaultReceiveBufferSize;
         this.ReuseAddress = false;
      }
      #endregion

      #region Properties
      /// <summary>
      /// The UDP socket send buffer size
      /// </summary>
      /// <seealso cref="System.Net.Sockets.Socket.SendBufferSize"/>
      public Int32 SendBufferSize { get; set; }
      /// <summary>
      /// The UDP socket receive buffer size
      /// </summary>
      /// <seealso cref="System.Net.Sockets.Socket.ReceiveBufferSize"/>
      public Int32 ReceiveBufferSize { get; set; }
      /// <summary>
      /// Specifies whether reusing the address/port
      /// combination is supported
      /// </summary>
      public Boolean ReuseAddress { get; set; }
      #endregion

      #region BindingElement Overrides
      /// <summary>
      /// Creates a deep copy of the binding configuration
      /// </summary>
      /// <returns>
      /// The copied binding element object
      /// </returns>
      public override System.ServiceModel.Channels.BindingElement Clone ()
      {
         return new BindingElement()
         {
            ManualAddressing = this.ManualAddressing,
            MaxBufferPoolSize = this.MaxBufferPoolSize,
            MaxReceivedMessageSize = this.MaxReceivedMessageSize,
            SendBufferSize = this.SendBufferSize,
            ReceiveBufferSize = this.ReceiveBufferSize,
            ReuseAddress = this.ReuseAddress
         };
      }
      #endregion

      #region TransportBindingElement Overrides
      /// <summary>
      /// The URI scheme for UDP addresses
      /// </summary>
      public override String Scheme
      {
         get { return "udp"; }
      }
      /// <summary>
      /// Specifies which channel factory (client) types 
      /// are supported by the UDP transport layer
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
         if (typeof(TChannel) == typeof(IOutputChannel))
            return true;
         if (typeof(TChannel) == typeof(IRequestChannel))
            return true;
         return false;
      }
      /// <summary>
      /// Constructs a UDP channel factory (client)
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
         if (typeof(TChannel) == typeof(IOutputChannel))
            return new Factory<IOutputChannel>(this, context) as IChannelFactory<TChannel>;
         if (typeof(TChannel) == typeof(IRequestChannel))
            return new Factory<IRequestChannel>(this, context) as IChannelFactory<TChannel>;
         throw new InvalidOperationException(String.Format("Channel type {0} not supported", typeof(TChannel)));
      }
      /// <summary>
      /// Specifies which channel listener (server) types 
      /// are supported by the UDP transport layer
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
         if (typeof(TChannel) == typeof(IInputChannel))
            return true;
         if (typeof(TChannel) == typeof(IReplyChannel))
            return true;
         return false;
      }
      /// <summary>
      /// Constructs a UDP channel listener (server)
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
         if (typeof(TChannel) == typeof(IInputChannel))
            return new Listener<IInputChannel>(this, context) as IChannelListener<TChannel>;
         if (typeof(TChannel) == typeof(IReplyChannel))
            return new Listener<IReplyChannel>(this, context) as IChannelListener<TChannel>;
         throw new InvalidOperationException(String.Format("Channel type {0} not supported", typeof(TChannel)));
      }
      #endregion

      /// <summary>
      /// Configuration extension element for the 
      /// UDP binding element
      /// </summary>
      public sealed class Config : BindingElementExtension<BindingElement>
      {
         #region Configuration Properties
         /// <summary>
         /// Maximum size of the transport buffer pool
         /// </summary>
         [ConfigurationProperty("maxBufferPoolSize", DefaultValue = DefaultMaxBufferPoolSize)]
         public Int32 MaxBufferPoolSize
         {
            get { return (Int32)base["maxBufferPoolSize"]; }
         }
         /// <summary>
         /// Maximum size of the transport buffer pool
         /// </summary>
         [ConfigurationProperty("maxReceivedMessageSize", DefaultValue = DefaultMaxReceivedMessageSize)]
         public Int32 MaxReceivedMessageSize
         {
            get { return (Int32)base["maxReceivedMessageSize"]; }
         }
         /// <summary>
         /// Maximum number of bytes to buffer
         /// on the sending side of a socket
         /// </summary>
         [ConfigurationProperty("sendBufferSize", DefaultValue = DefaultSendBufferSize)]
         public Int32 SendBufferSize
         {
            get { return (Int32)base["sendBufferSize"]; }
         }
         /// <summary>
         /// Maximum number of bytes to buffer
         /// on the receiving side of a socket
         /// </summary>
         [ConfigurationProperty("receiveBufferSize", DefaultValue = DefaultReceiveBufferSize)]
         public Int32 ReceiveBufferSize
         {
            get { return (Int32)base["receiveBufferSize"]; }
         }
         /// <summary>
         /// Specifies whether reusing the address/port
         /// combination is supported
         /// </summary>
         [ConfigurationProperty("reuseAddress", DefaultValue = false)]
         public Boolean ReuseAddress
         {
            get { return (Boolean)base["reuseAddress"]; }
         }
         #endregion

         #region BindingElementExtension Overrides
         /// <summary>
         /// Assigns binding configuration properties
         /// </summary>
         /// <param name="bindingElement">
         /// The binding to configure
         /// </param>
         public override void ApplyConfiguration (System.ServiceModel.Channels.BindingElement bindingElement)
         {
            base.ApplyConfiguration(bindingElement);
            BindingElement elem = (BindingElement)bindingElement;
            elem.MaxBufferPoolSize = this.MaxBufferPoolSize;
            elem.MaxReceivedMessageSize = this.MaxReceivedMessageSize;
            elem.SendBufferSize = this.SendBufferSize;
            elem.ReceiveBufferSize = this.ReceiveBufferSize;
            elem.ReuseAddress = this.ReuseAddress;
         }
         #endregion
      }
   }
}
