//===========================================================================
// MODULE:  ConsoleOutput.cs
// PURPOSE: UDP sample server implementation
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
   /// Service console output implementation
   /// </summary>
   /// <remarks>
   /// This class implements the IConsoleOutput contract in the remote
   /// server process, echoing the client's console to the server's.
   /// </remarks>
   public sealed class ConsoleOutput : IConsoleOutput
   {
      #region IConsoleOutput Implementation
      /// <summary>
      /// Console message output
      /// </summary>
      /// <param name="message">
      /// The message to output
      /// </param>
      public void Write (String line)
      {
         Console.Write(line);
      }
      #endregion
   }
}
