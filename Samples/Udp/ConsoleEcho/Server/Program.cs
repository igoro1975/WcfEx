//===========================================================================
// MODULE:  Program.cs
// PURPOSE: UDP sample server host program
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

namespace WcfEx.Samples.Udp
{
   /// <summary>
   /// The UDP console echo server program
   /// </summary>
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
         Console.WriteLine("WcfEx UDP Sample Server");
         // hook up the service host
         using (var host = new ServiceHost(typeof(ConsoleOutput)))
         {
            Console.Write("   Starting up server on {0}...", host.Description.Endpoints[0].Address.Uri);
            host.Open();
            Console.WriteLine("done.");
            Console.WriteLine("   Press escape to exit.");
            Console.WriteLine();
            // wait until the user terminates
            for ( ; ; )
               if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                  break;
         }
      }
   }
}
