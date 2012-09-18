//===========================================================================
// MODULE:  MockOutput.cs
// PURPOSE: in-process sample mock contract implementation
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
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
// Project References

namespace WcfEx.Samples.InProc
{
   /// <summary>
   /// Console output mock implementation
   /// </summary>
   /// <remarks>
   /// This class provides an in-process implementation of the 
   /// IConsoleOutput contract, replacing the remote console server and
   /// writing dispatched to the client's console.
   /// </remarks>
   public sealed class MockOutput : IConsoleOutput
   {
      #region IConsoleOutput Implementation
      /// <summary>
      /// Console line output
      /// </summary>
      /// <param name="line">
      /// The message to output
      /// </param>
      public void WriteLine (String line)
      {
         if (line != null)
            Console.WriteLine("Mock: {0}", line);
      }
      #endregion
   }
}
