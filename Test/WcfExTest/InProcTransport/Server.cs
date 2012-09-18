//===========================================================================
// MODULE:  Server.cs
// PURPOSE: test in-process WCF server
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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
// Project References

namespace WcfEx.Test.InProc
{
   /// <summary>
   /// InProc client callback
   /// </summary>
   /// <remarks>
   /// This interface is implemented by the unit test driver
   /// to support testing WCF client callbacks across the 
   /// in-process transport layer.
   /// </remarks>
   public interface ICallback
   {
      /// <summary>
      /// Callback dispatch
      /// </summary>
      /// <param name="param">
      /// Callback parameter
      /// </param>
      /// <returns>
      /// Callback result
      /// </returns>
      [OperationContract]
      String Callback (String param);
   }

   /// <summary>
   /// InProc server contract
   /// </summary>
   /// <remarks>
   /// This interface is implemented by the test server and
   /// used as the contract for testing the in-process
   /// WCF transport layer.
   /// </remarks>
   [ServiceContract(Namespace = "http://brentspell.us/Projects/WcfEx/Test/InProc", CallbackContract = typeof(ICallback))]
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
      ///  Executes a one-way (no response) request
      /// </summary>
      /// <param name="param">
      /// The request parameter
      /// </param>
      [OperationContract(IsOneWay = true)]
      void FireAndForget (String param);
      /// <summary>
      /// Retrieves the in-process transport session identifier
      /// </summary>
      /// <returns>
      /// The current WCF session identifier
      /// </returns>
      [OperationContract]
      String QuerySessionID ();
      /// <summary>
      /// Executes a client callback
      /// </summary>
      /// <param name="param">
      /// The callback parameter
      /// </param>
      /// <returns>
      /// The result of the callback operation
      /// </returns>
      [OperationContract]
      String Callback (String param);
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
      /// <summary>
      /// Raises an error over the WCF channel
      /// </summary>
      /// <param name="message">
      /// The error message to raise
      /// </param>
      [OperationContract]
      void Throw (String message);
   }

   /// <summary>
   /// InProc test server
   /// </summary>
   /// <remarks>
   /// This class implements the test contract interface
   /// over the in-process transport layer.
   /// </remarks>
   [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
   public class Server : IServer
   {
      private static CountdownEvent oneWayCounter = new CountdownEvent(0);

      /// <summary>
      /// Resets the global request counter
      /// </summary>
      /// <param name="count">
      /// The number of requests to expect
      /// </param>
      /// <returns>
      /// A wait handle for the counter
      /// </returns>
      public static WaitHandle ResetOneWayCounter (Int32 count)
      {
         return (oneWayCounter = new CountdownEvent(count)).WaitHandle;
      }

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
      ///  Executes a one-way (no response) request
      /// </summary>
      /// <param name="param">
      /// The request parameter
      /// </param>
      public void FireAndForget (String param)
      {
         oneWayCounter.Signal();
      }
      /// <summary>
      /// Retrieves the in-process transport session identifier
      /// </summary>
      /// <returns>
      /// The current WCF session identifier
      /// </returns>
      public String QuerySessionID ()
      {
         return OperationContext.Current.SessionId;
      }
      /// <summary>
      /// Executes a client callback
      /// </summary>
      /// <param name="param">
      /// The callback parameter
      /// </param>
      /// <returns>
      /// The result of the callback operation
      /// </returns>
      public String Callback (String value)
      {
         return OperationContext.Current
            .GetCallbackChannel<ICallback>()
            .Callback(value);
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
      /// <summary>
      /// Raises an error over the WCF channel
      /// </summary>
      /// <param name="message">
      /// The error message to raise
      /// </param>
      public void Throw (String message)
      {
         throw new Exception(message);
      }
      #endregion
   }
}
