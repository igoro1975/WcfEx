//===========================================================================
// MODULE:  BindingElement.cs
// PURPOSE: Deflate codec WCF binding configuration element
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
using System.ServiceModel.Description;
using System.Xml;
// Project References

namespace WcfEx.Deflate
{
   /// <summary>
   /// Deflate codec binding configuration
   /// </summary>
   /// <remarks>
   /// This class represents the WCF binding element used
   /// to create instances of the deflate encoder factory, and
   /// in turn, the deflate encoder. Use this class directly with
   /// a WCF CustomBinding or via the configuration extension.
   /// </remarks>
   /// <example>
   ///   <system.serviceModel>
   ///      <extensions>
   ///         <bindingElementExtensions>
   ///            <add name="deflateEncoding" type="WcfEx.Deflate.BindingElement+Config,WcfEx"/>
   ///         </bindingElementExtensions>
   ///      </extensions>
   ///      <bindings>
   ///         <customBinding>
   ///            <binding name="compressedTcp">
   ///               <deflateEncoding/>
   ///               <tcpTransport/>
   ///            </binding>
   ///         </customBinding>
   ///      </bindings>
   ///      ...
   ///   </system.serviceModel>
   /// </example>
   public sealed class BindingElement : MessageEncodingBindingElement, IPolicyExportExtension
   {
      private MessageEncodingBindingElement baseEncoding;

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new binding instance
      /// </summary>
      public BindingElement()
      {
         this.baseEncoding = new BinaryMessageEncodingBindingElement();
      }
      /// <summary>
      /// Initializes a new binding instance
      /// </summary>
      /// <param name="baseEncoding">
      /// The base transport encoding to use with this encoding
      /// </param>
      private BindingElement(MessageEncodingBindingElement baseEncoding)
      {
         this.baseEncoding = baseEncoding;
      }
      #endregion

      #region MessageEncodingBindingElement Overrides
      /// <summary>
      /// Creates an instance of the factory used to
      /// create new message encoders
      /// </summary>
      /// <returns>
      /// The configured message encoder factory
      /// </returns>
      public override MessageEncoderFactory CreateMessageEncoderFactory()
      {
         return new Factory(this.baseEncoding.CreateMessageEncoderFactory());
      }
      /// <summary>
      /// The current WCF message version
      /// </summary>
      public override MessageVersion MessageVersion
      {
         get { return this.baseEncoding.MessageVersion; }
         set { this.baseEncoding.MessageVersion = value; }
      }
      /// <summary>
      /// Creates a copy of this binding configuration element
      /// </summary>
      /// <returns>
      /// The new binding config element
      /// </returns>
      public override System.ServiceModel.Channels.BindingElement Clone()
      {
         return new BindingElement(
            (MessageEncodingBindingElement)this.baseEncoding.Clone()
         );
      }
      /// <summary>
      /// Retrieves a WCF typed configuration property
      /// </summary>
      /// <typeparam name="T">
      /// The type to retrieve
      /// </typeparam>
      /// <param name="context">
      /// The current binding context
      /// </param>
      /// <returns>
      /// The requested property value
      /// </returns>
      public override T GetProperty<T>(BindingContext context)
      {
         if (typeof(T) == typeof(XmlDictionaryReaderQuotas))
            return baseEncoding.GetProperty<T>(context);
         else
            return base.GetProperty<T>(context);
      }
      /// <summary>
      /// Specifies whether this configuration element
      /// can create an instance of a channel listener
      /// </summary>
      /// <typeparam name="TChannel">
      /// The channel listener interface type
      /// </typeparam>
      /// <param name="context">
      /// The current binding context
      /// </param>
      /// <returns>
      /// True if the specified channel listener is supported
      /// False otherwise
      /// </returns>
      public override Boolean CanBuildChannelListener<TChannel> (BindingContext context)
      {
         PrepareBinding(context);
         return context.CanBuildInnerChannelListener<TChannel>();
      }
      /// <summary>
      /// Specifies whether this configuration element
      /// can create an instance of a channel factory
      /// </summary>
      /// <typeparam name="TChannel">
      /// The channel factory interface type
      /// </typeparam>
      /// <param name="context">
      /// The current binding context
      /// </param>
      /// <returns>
      /// True if the specified channel factory is supported
      /// False otherwise
      /// </returns>
      public override Boolean CanBuildChannelFactory<TChannel> (BindingContext context)
      {
         PrepareBinding(context);
         return context.CanBuildInnerChannelFactory<TChannel>();
      }
      /// <summary>
      /// Creates and configures a new channel listener instance
      /// </summary>
      /// <typeparam name="TChannel">
      /// The channel listener interface type
      /// </typeparam>
      /// <param name="context">
      /// The current binding context
      /// </param>
      /// <returns>
      /// The requested channel listener
      /// </returns>
      public override IChannelListener<TChannel> BuildChannelListener<TChannel> (BindingContext context)
      {
         PrepareBinding(context);
         return context.BuildInnerChannelListener<TChannel>();
      }
      /// <summary>
      /// Creates and configures a new channel factory instance
      /// </summary>
      /// <typeparam name="TChannel">
      /// The channel factory interface type
      /// </typeparam>
      /// <param name="context">
      /// The current binding context
      /// </param>
      /// <returns>
      /// The requested channel factory
      /// </returns>
      public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (BindingContext context)
      {
         PrepareBinding(context);
         return context.BuildInnerChannelFactory<TChannel>();
      }
      #endregion

      #region IPolicyExportExtension Implementation
      /// <summary>
      /// Exports a custom policy assertion for the
      /// encoder binding
      /// </summary>
      /// <param name="exporter">
      /// The metadata exporter used to modify the 
      /// exporting process
      /// </param>
      /// <param name="context">
      /// The policy conversion context used to to 
      /// insert the custom policy assertion
      /// </param>
      public void ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
      {
         context.GetBindingAssertions().Add(
            new XmlDocument().CreateElement("deflate", "DeflateEncoding", "http://www.ietf.org/rfc/rfc1951.txt")
         );
      }
      #endregion

      #region Operations
      /// <summary>
      /// Preprocesses a binding context, adding this
      /// element into the binding parameters and replacing
      /// any existing MEBEs
      /// </summary>
      /// <param name="context"></param>
      private void PrepareBinding (BindingContext context)
      {
         if (!context.BindingParameters.Contains(this))
         {
            MessageEncodingBindingElement baseEncoding =
               context.BindingParameters.Remove<MessageEncodingBindingElement>();
            if (baseEncoding != null)
               this.baseEncoding = baseEncoding;
            context.BindingParameters.Add(this);
         }
      }
      #endregion

      /// <summary>
      /// Configuration extension element for the 
      /// deflate binding element
      /// </summary>
      public sealed class Config : BindingElementExtension<BindingElement>
      {
      }
   }
}
