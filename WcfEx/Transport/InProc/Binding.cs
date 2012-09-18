//===========================================================================
// MODULE:  Binding.cs
// PURPOSE: In-process WCF binding class
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
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
// Project References

namespace WcfEx.InProc
{
   /// <summary>
   /// In-process WCF binding
   /// </summary>
   /// <remarks>
   /// This class represents the <inProcBinding/> WCF binding extension. It 
   /// serves as the entry point between WCF and the inProc transport layer
   /// for configured bindings. This class creates a default binding stack
   /// equivalent to <inProcTransport/>.
   /// </remarks>
   /// <example>
   ///   <system.serviceModel>
   ///      <extensions>
   ///         <bindingExtensions>
   ///            <add name="inProcBinding" type="WcfEx.InProc.Binding+Config,WcfEx"/>
   ///         </bindingExtensions>
   ///      </extensions>
   ///      <services>
   ///         <service>
   ///            <endpoint binding="inProcBinding"/>
   ///         </service>
   ///      </services>
   ///   </system.serviceModel>
   /// </example>
   public sealed class Binding : System.ServiceModel.Channels.Binding
   {
      #region Construction/Disposal
      /// <summary>
      /// Initializes a new binding instance
      /// </summary>
      public Binding ()
      {
         this.Element = new BindingElement();
      }
      #endregion

      #region Properties
      /// <summary>
      /// The UDP binding element configured
      /// for this binding
      /// </summary>
      public BindingElement Element
      {
         get; set;
      }
      #endregion

      #region Binding Overrides
      /// <summary>
      /// Creates a collection that contains the binding elements 
      /// that are part of the current binding
      /// </summary>
      /// <returns>
      /// The binding element collection
      /// </returns>
      public override BindingElementCollection CreateBindingElements ()
      {
         return new BindingElementCollection(new[] { this.Element });
      }
      /// <summary>
      /// The in-process address URI scheme
      /// </summary>
      public override String Scheme
      {
         get { return "inProc"; }
      }
      #endregion

      /// <summary>
      /// Configuration extension element for the 
      /// in-process binding
      /// </summary>
      public sealed class Config : SimpleBindingExtension<Binding>
      {
         #region Configuration Properties
         /// <summary>
         /// Transport configuration child element
         /// </summary>
         [ConfigurationProperty("inProcTransport")]
         public BindingElement.Config Transport
         {
            get { return (BindingElement.Config)base["inProcTransport"]; }
         }
         #endregion

         #region SimpleBindingExtension Overrides
         /// <summary>
         /// Override for custom binding configuration
         /// </summary>
         /// <param name="binding">
         /// The binding instance to configure
         /// </returns>
         protected override void ApplyDefaultConfiguration (Binding binding)
         {
            binding.Name = "inProcBinding";
            if (this.Transport != null)
               this.Transport.ApplyConfiguration(binding.Element);
         }
         #endregion
      }
   }
}
