//===========================================================================
// MODULE:  Program.cs
// PURPOSE: UDP sample client driver program
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

namespace WcfEx.Samples.Udp
{
   /// <summary>
   /// The UDP console echo client program
   /// </summary>
   /// <remarks>
   /// This program reads characters from either the keyboard or a 
   /// redirected file and echoes them to the console server.
   /// Note that if a redirected file is used, the output on the server
   /// may be received out of order if the IConsoleOutput contract is set
   /// to IsOneWay. This is because one-way messages do not block the client
   /// application until the server processes the message.
   /// </remarks>
   class Program
   {
      public const Int32 MaxBlockSize = 80;

      /// <summary>
      /// Program entry point
      /// </summary>
      /// <param name="args">
      /// Program arguments
      /// </param>
      static void Main (String[] args)
      {
         Console.WriteLine("WcfEx UDP Sample Client");
         using (var proxy = new Client<IConsoleOutput>())
         {
            // echo an empty message to validate the connection
            Console.Write("   Connecting to {0}...", proxy.Endpoint.Address);
            proxy.Server.Write("");
            Console.WriteLine("done.");
            // attach the console character reader
            var reader = new ConsoleReader();
            if (!reader.IsRedirected)
            {
               Console.WriteLine("   Press escape to exit.");
               Console.WriteLine();
            }
            // read from the console input, one character at a time
            // echo the results to the server
            do
               proxy.Server.Write(reader.ReadBlock(MaxBlockSize));
            while (!reader.IsEof);
         }
      }
   }
}
