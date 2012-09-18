//===========================================================================
// MODULE:  ConsoleOutput.cs
// PURPOSE: in-process sample remote server implementation
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
using System.Runtime.InteropServices;
// Project References

namespace WcfEx.Samples.InProc
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
      #region Windows Console API
      /// <summary>
      /// Win32 AllocConsole API, required for starting up a new
      /// console application within the test WCF service host
      /// </summary>
      /// <returns>
      /// Nonzero if successful
      /// Zero otherwise
      /// </returns>
      [DllImport(
         "kernel32.dll",
         EntryPoint = "AllocConsole",
         SetLastError = true,
         CharSet = CharSet.Auto,
         CallingConvention = CallingConvention.StdCall)
      ]
      private static extern Int32 AllocConsole ();
      #endregion

      /// <summary>
      /// Initializes the service class
      /// </summary>
      static ConsoleOutput ()
      {
         // ensure that a console window is running,
         // in case we are hosted in a Windows app,
         // such as the WCF debug host
         AllocConsole();
         Console.WriteLine("WcfEx In-Process Sample Remote TCP Server");
         Console.WriteLine();
      }

      #region IConsoleOutput Implementation
      /// <summary>
      /// Console line output
      /// </summary>
      /// <param name="message">
      /// The message to output
      /// </param>
      public void WriteLine (String message)
      {
         if (message != null)
            Console.WriteLine(message);
      }
      #endregion
   }
}
