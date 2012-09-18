//===========================================================================
// MODULE:  BindingElementExtension.cs
// PURPOSE: binding element extension configuration base class
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
using System.ServiceModel.Configuration;
// Project References

namespace WcfEx
{
   /// <summary>
   /// Binding element extension base class
   /// </summary>
   /// <remarks>
   /// This class provides a resonable default implementation of the
   /// tedious binding element configuration methods.
   /// </remarks>
   /// <typeparam name="TElement">
   /// The binding element type
   /// </typeparam>
   public abstract class BindingElementExtension<TElement> : BindingElementExtensionElement
      where TElement : BindingElement, new()
   {
      #region BindingElementExtensionElement Overrides
      /// <summary>
      /// Specifies the CLR type of the
      /// binding configuration class
      /// </summary>
      public override Type BindingElementType
      {
         get { return typeof(TElement); }
      }
      /// <summary>
      /// Creates a new instance of the
      /// binding configuration class
      /// </summary>
      /// <returns>
      /// The new binding object
      /// </returns>
      protected override System.ServiceModel.Channels.BindingElement CreateBindingElement()
      {
         BindingElement elem = new TElement();
         ApplyConfiguration(elem);
         return elem;
      }
      #endregion
   }
}
