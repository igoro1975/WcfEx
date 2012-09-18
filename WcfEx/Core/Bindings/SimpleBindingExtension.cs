//===========================================================================
// MODULE:  SimpleBindingExtension.cs
// PURPOSE: simple binding extension configuration base class
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
using System.Collections.ObjectModel;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
// Project References

namespace WcfEx
{
   /// <summary>
   /// Simple binding extension base class
   /// </summary>
   /// <remarks>
   /// This class provides a resonable default implementation of the
   /// tedious binding configuration methods. This class supports
   /// binding extensions that exist as singletons without a binding
   /// configuration collection.
   /// </remarks>
   /// <typeparam name="TBinding">
   /// The binding type
   /// </typeparam>
   public abstract class SimpleBindingExtension<TBinding> : 
      BindingCollectionElement
      where TBinding : Binding, new()
   {
      #region Configuration Properties
      /// <summary>
      /// UDP socket open timeout
      /// </summary>
      [ConfigurationProperty("openTimeout", DefaultValue = "00:01:00")]
      public TimeSpan OpenTimeout
      {
         get { return (TimeSpan)base["openTimeout"]; }
      }
      /// <summary>
      /// UDP socket close timeout
      /// </summary>
      [ConfigurationProperty("closeTimeout", DefaultValue = "00:01:00")]
      public TimeSpan CloseTimeout
      {
         get { return (TimeSpan)base["closeTimeout"]; }
      }
      /// <summary>
      /// UDP socket send timeout
      /// </summary>
      [ConfigurationProperty("sendTimeout", DefaultValue = "00:01:00")]
      public TimeSpan SendTimeout
      {
         get { return (TimeSpan)base["sendTimeout"]; }
      }
      /// <summary>
      /// UDP socket receive timeout
      /// </summary>
      [ConfigurationProperty("receiveTimeout", DefaultValue = "00:01:00")]
      public TimeSpan ReceiveTimeout
      {
         get { return (TimeSpan)base["receiveTimeout"]; }
      }
      #endregion

      #region BindingCollectionElement Overrides
      /// <summary>
      /// Determines whether the configuration element
      /// contains a specified binding
      /// </summary>
      /// <param name="name">
      /// The binding to find
      /// </param>
      /// <returns>
      /// True if the binding exists
      /// False otherwise
      /// </returns>
      public override Boolean ContainsKey(String name)
      {
         // multiple bindings are not supported by this extension
         return true;
      }
      /// <summary>
      /// Adds a binding to the list
      /// </summary>
      /// <param name="name">
      /// Binding name
      /// </param>
      /// <param name="binding">
      /// Binding configuration
      /// </param>
      /// <param name="config">
      /// Configuration manager
      /// </param>
      /// <returns>
      /// True if the binding element was added
      /// False otherwise
      /// </returns>
      protected override Boolean TryAdd(
         String name,
         Binding binding,
         Configuration config)
      {
         // multiple bindings are not supported by this extension
         throw new NotSupportedException();
      }
      /// <summary>
      /// The binding runtime type
      /// </summary>
      public override Type BindingType
      {
         get { return typeof(TBinding); }
      }
      /// <summary>
      /// Constructs the default binding
      /// </summary>
      /// <returns>
      /// The default binding configuration
      /// </returns>
      protected override Binding GetDefault()
      {
         TBinding binding = new TBinding();
         // apply common binding configuration
         binding.OpenTimeout = this.OpenTimeout;
         binding.CloseTimeout = this.CloseTimeout;
         binding.SendTimeout = this.SendTimeout;
         binding.ReceiveTimeout = this.ReceiveTimeout;
         // apply custom binding configuration
         ApplyDefaultConfiguration(binding);
         return binding;
      }
      /// <summary>
      /// Retrieves the list of configured bindings,
      /// consisting of only the default binding
      /// </summary>
      public override ReadOnlyCollection<IBindingConfigurationElement> ConfiguredBindings
      {
         get
         {
            return new ReadOnlyCollection<IBindingConfigurationElement>(
               new[] { GetDefault() as IBindingConfigurationElement }
            );
         }
      }
      #endregion

      #region SimpleBindingExtension Overrides
      /// <summary>
      /// Override for custom binding configuration
      /// </summary>
      /// <param name="binding">
      /// The binding instance to configure
      /// </returns>
      protected virtual void ApplyDefaultConfiguration (TBinding binding)
      {
      }
      #endregion
   }
}
