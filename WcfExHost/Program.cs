//===========================================================================
// MODULE:  Program.cs
// PURPOSE: service host application entry point
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
using System.ServiceProcess;
// Project References

namespace WcfEx.Host
{
   /// <summary>
   /// Service host entry point
   /// </summary>
   /// <remarks>
   /// This entry point starts the WCF service host,
   /// either as a Windows service or as a console
   /// application.
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
         if (!Environment.UserInteractive)
            ServiceBase.Run(new WindowsService());
         else
         {
            using (ServiceHost host = new ServiceHost())
            {
               if (host.Startup())
               {
                  Console.Write("Press enter to shut down.");
                  Console.ReadLine();
               }
            }
         }
      }
   }
}
