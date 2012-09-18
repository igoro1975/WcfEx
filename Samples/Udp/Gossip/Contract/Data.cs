//===========================================================================
// MODULE:  Data.cs
// PURPOSE: UDP gossip sample database item data value
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
using System.Runtime.Serialization;
// Project References

namespace WcfEx.Samples.Gossip
{
   /// <summary>
   /// Item data value structure
   /// </summary>
   /// <remarks>
   /// This structure encapsulates an item value in the gossip database,
   /// including its binary and formatted values.
   /// </remarks>
   [DataContract]
   public struct Data
   {
      public static readonly Data Empty = new Data();

      /// <summary>
      /// Initializes a new value instance
      /// </summary>
      /// <param name="value">
      /// Binary value
      /// </param>
      /// <param name="formatted">
      /// Formatted value
      /// </param>
      public Data (Object value, String formatted)
         : this()
      {
         this.Value = value;
         this.Formatted = formatted;
      }

      /// <summary>
      /// Binary item value
      /// </summary>
      [DataMember]
      public Object Value { get; private set; }
      /// <summary>
      /// Formatted item value
      /// </summary>
      [DataMember]
      public String Formatted { get; private set; }
   }
}
