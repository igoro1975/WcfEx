//===========================================================================
// MODULE:  Server.cs
// PURPOSE: test WCF server for the deflate message encoder
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
using System.IO;
using System.Linq;
using System.ServiceModel;
// Project References

namespace WcfEx.Test.Deflate
{
   /// <summary>
   /// Deflate encoder server contract
   /// </summary>
   /// <remarks>
   /// This interface is implemented by the test server and
   /// used as the contract for testing the deflate WCF message
   /// encoder.
   /// </remarks>
   [ServiceContract(Namespace = "http://brentspell.us/Projects/WcfEx/Test/Deflate")]
   public interface IServer
   {
      /// <summary>
      /// Reverses a string
      /// </summary>
      /// <param name="param">
      /// The string to reverse
      /// </param>
      /// <returns>
      /// The reversed string
      /// </returns>
      [OperationContract]
      String Reverse (String param);
      /// <summary>
      /// Echoes the contents of a stream
      /// </summary>
      /// <param name="stream">
      /// The stream to echo
      /// </param>
      /// <returns>
      /// A copy of the specified stream
      /// </returns>
      [OperationContract]
      Stream Echo (Stream stream);
   }

   /// <summary>
   /// Deflate encoder test server
   /// </summary>
   /// <remarks>
   /// This class implements the test contract interface
   /// over the deflate message encoder.
   /// </remarks>
   [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
   public class Server : IServer
   {
      #region IServer Implementation
      /// <summary>
      /// Reverses a string
      /// </summary>
      /// <param name="param">
      /// The string to reverse
      /// </param>
      /// <returns>
      /// The reversed string
      /// </returns>
      public String Reverse (String param)
      {
         return new String(param.Reverse().ToArray());
      }
      /// <summary>
      /// Echoes the contents of a stream
      /// </summary>
      /// <param name="stream">
      /// The stream to echo
      /// </param>
      /// <returns>
      /// A copy of the specified stream
      /// </returns>
      public Stream Echo (Stream stream)
      {
         Stream echo = new MemoryStream();
         Byte[] buffer = new Byte[8192];
         Int32 actual = 0;
         do
            echo.Write(buffer, 0, actual = stream.Read(buffer, 0, buffer.Length));
         while (actual != 0);
         echo.Position = 0;
         return echo;
      }
      #endregion
   }
}
