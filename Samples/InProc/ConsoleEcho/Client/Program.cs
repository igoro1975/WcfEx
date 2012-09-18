//===========================================================================
// MODULE:  Program.cs
// PURPOSE: in-process sample client driver program
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
// Project References

namespace WcfEx.Samples.InProc
{
   /// <summary>
   /// The in-process console program
   /// </summary>
   /// <remarks>
   /// This program reads input lines from the application console and
   /// sends them to the output service as configured in App.Config.
   /// If the in-process service is configured along with the ClientHost
   /// behavior, then all lines will be echoed within the same console
   /// window (in the client process).
   /// Otherwise, all lines will be sent to the remote server instance
   /// and echoed in its console window.
   /// </remarks>
   class Program
   {
      /// <summary>
      /// Program entry point
      /// </summary>
      /// <param name="args">
      /// Program arguments
      /// </param>
      static void Main (String[] args)
      {
         Console.WriteLine("WcfEx In-Process Sample Client");
         using (var proxy = new Client<IConsoleOutput>())
         {
            // Send an empty line to validate the server connection
            Console.Write("   Connecting to {0}...", proxy.Endpoint.Address);
            proxy.Server.WriteLine(null);
            Console.WriteLine("done.");
            Console.WriteLine("   Press {Ctrl}-Z to end.");
            Console.WriteLine();
            // echo all lines on the server
            for ( ; ; )
            {
               String message = Console.ReadLine();
               if (message == null)
                  break;
               proxy.Server.WriteLine(message);
            }
         }
      }
   }
}
