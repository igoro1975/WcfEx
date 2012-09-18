//===========================================================================
// MODULE:  ConsoleReader.cs
// PURPOSE: keyboard or redirected console character reader
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
using System.Runtime.InteropServices;
// Project References

namespace WcfEx.Samples.Udp
{
   /// <summary>
   /// Console character reader
   /// </summary>
   /// <remarks>
   /// This class reads individual characters from STDIN, either from the
   /// keyboard or redirected from a file. It indicates EOF when the user
   /// presses the escape key or when the end of the redirected input
   /// is reached
   /// </remarks>
   public sealed class ConsoleReader
   {
      Boolean isRedirected = (GetFileType(GetStdHandle(StdHandle.StdIn)) != FileType.Char);

      #region Operations
      /// <summary>
      /// Specifies whether Console.In is redirected from a file
      /// </summary>
      public Boolean IsRedirected 
      {
         get { return isRedirected; }
      }
      /// <summary>
      /// Indicates whether the end of input has been reached
      /// </summary>
      public Boolean IsEof
      {
         get; private set;
      }
      /// <summary>
      /// Indicates whether a next character is available from
      /// the input stream
      /// </summary>
      public Boolean HasChar
      {
         get
         {
            if (this.IsEof)
               return false;
            if (this.IsRedirected)
               return true;
            return Console.KeyAvailable;
         }
      }
      /// <summary>
      /// Reads the next character from the input stream,
      /// blocking if no character is available
      /// </summary>
      /// <returns>
      /// The character read if successful
      /// default(Char) if the end of input was reached
      /// </returns>
      public Char Read ()
      {
         var ch = 0;
         if (this.IsRedirected)
         {
            ch = Console.Read();
            if (ch < 0)
               this.IsEof = true;
         }
         else
         {
            ch = Console.ReadKey().KeyChar;
            switch (ch)
            {
               case (Int32)ConsoleKey.Escape:
                  this.IsEof = true;
                  break;
               case '\r':
                  if (!this.IsRedirected)
                     Console.WriteLine();
                  break;
               default:
                  break;
            }
         }
         return (!IsEof) ? Convert.ToChar(ch) : default(Char);
      }
      /// <summary>
      /// Reads a block of text from the console stream
      /// blocking only to read at least one character
      /// </summary>
      /// <param name="maxLength">
      /// The maximum number of characters to read
      /// </param>
      /// <returns>
      /// The block of text that could be read without
      /// blocking after the first character
      /// </returns>
      public String ReadBlock (Int32 maxLength)
      {
         var buffer = new Char[maxLength];
         var n = 0;
         do
         {
            var ch = Read();
            if (ch != default(Char))
               buffer[n++] = ch;
         } while (n < buffer.Length && this.HasChar);
         var message = new String(buffer, 0, n);
         if (!this.IsRedirected)
            message = message.Replace("\r", "\r\n");
         return message;
      }
      #endregion

      #region Interop Definitions
      private enum FileType { Unknown, Disk, Char, Pipe };
      private enum StdHandle { StdIn = -10, StdOut = -11, StdErr = -12 };
      [DllImport("kernel32.dll")]
      private static extern FileType GetFileType (IntPtr hdl);
      [DllImport("kernel32.dll")]
      private static extern IntPtr GetStdHandle (StdHandle std);
      #endregion
   }
}
