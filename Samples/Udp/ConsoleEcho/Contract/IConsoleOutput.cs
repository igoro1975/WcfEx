//===========================================================================
// MODULE:  IConsoleOutput.cs
// PURPOSE: UDP sample service contract interface
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
// Project References

namespace WcfEx.Samples.Udp
{
   /// <summary>
   /// Console output service contract
   /// </summary>
   [ServiceContract(Namespace = "http://brentspell.us/Projects/WcfEx/Samples/Udp/")]
   public interface IConsoleOutput
   {
      /// <summary>
      /// Console message output
      /// </summary>
      /// <remarks>
      /// Note that IsOneWay is specified here, which means that
      /// the service may receive messages outside the order in which the
      /// client sends them. To prevent this, set IsOneWay = false, which
      /// will generate a WCF response for each request.
      /// </remarks>
      /// <param name="message">
      /// The message to output
      /// </param>
      [OperationContract(IsOneWay = true)]
      void Write (String message);
   }
}
