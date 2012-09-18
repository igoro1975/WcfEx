//===========================================================================
// MODULE:  Server.cs
// PURPOSE: test UDP WCF server
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
using System.Linq;
using System.ServiceModel;
using System.Threading;
// Project References

namespace WcfEx.Test.Udp
{
   /// <summary>
   /// One-way UDP server contract
   /// </summary>
   /// <remarks>
   /// This interface is implemented by the test server and used as 
   /// the contract for testing the UDP WCF transport layer. This 
   /// interface is strictly one-way, for testing the input/output 
   /// UDP channels.
   /// </remarks>
   [ServiceContract(Namespace = "http://brentspell.us/Projects/WcfEx/Test/Udp")]
   public interface IOneWayServer
   {
      /// <summary>
      /// Executes a one-way (no response) request
      /// </summary>
      /// <param name="param">
      /// The request parameter
      /// </param>
      [OperationContract(IsOneWay = true)]
      void FireAndForget (String param);
   }

   /// <summary>
   /// Two-way UDP server contract
   /// </summary>
   /// <remarks>
   /// This interface is implemented by the test server and used as 
   /// the contract for testing the UDP WCF transport layer. This 
   /// interface is two-way, for testing the request/response UDP 
   /// channels.
   /// </remarks>
   [ServiceContract(Namespace = "http://brentspell.us/Projects/WcfEx/Test/Udp")]
   public interface ITwoWayServer
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
      /// Reverse method APM pattern
      /// </summary>
      [OperationContract(AsyncPattern = true)]
      IAsyncResult BeginReverse (String param, AsyncCallback callback, Object state);
      /// <summary>
      /// Reverse method APM pattern
      /// </summary>
      String EndReverse (IAsyncResult result);
      /// <summary>
      /// Executes a one-way (no response) request
      /// </summary>
      /// <param name="param">
      /// The request parameter
      /// </param>
      [OperationContract(IsOneWay = true)]
      void FireAndForget (String param);
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
   /// Duplex UDP server contract
   /// </summary>
   /// <remarks>
   /// This interface is implemented by the test server and used as 
   /// the contract for testing the UDP WCF transport layer. This 
   /// interface is full duplex, for testing the request/response UDP 
   /// channels when used with the composite duplex binding.
   /// </remarks>
   [ServiceContract(
      Namespace = "http://brentspell.us/Projects/WcfEx/Test/Udp",
      CallbackContract = typeof(IDuplexCallback))]
   public interface IDuplexServer
   {
      /// <summary>
      /// Dispatches a request to the callback interface
      /// </summary>
      /// <param name="param">
      /// The string callback parameter
      /// </param>
      /// <returns>
      /// The callback result
      /// </returns>
      [OperationContract]
      String Callback (String param);
   }

   /// <summary>
   /// Duplex UDP server contract callback
   /// </summary>
   public interface IDuplexCallback
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
   }

   /// <summary>
   /// Two-way UDP test server
   /// </summary>
   /// <remarks>
   /// This class implements the test contract interface over the 
   /// UDP transport layer. This server is two-way, for testing the 
   /// request/response UDP channels.
   /// </remarks>
   [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
   public class Server : IOneWayServer, ITwoWayServer, IDuplexServer
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
      /// Reverse async stub method
      /// </summary>
      public IAsyncResult BeginReverse (
         String param, 
         AsyncCallback callback, 
         Object state)
      {
         throw new NotSupportedException();
      }
      /// <summary>
      /// Reverse async stub method
      /// </summary>
      public String EndReverse (IAsyncResult result)
      {
         throw new NotSupportedException();
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
      /// Raises an error over the WCF channel
      /// </summary>
      /// <param name="message">
      /// The error message to raise
      /// </param>
      public void Throw (String message)
      {
         throw new Exception(message);
      }
      /// <summary>
      /// Dispatches a request to the callback interface
      /// </summary>
      /// <param name="param">
      /// The string callback parameter
      /// </param>
      /// <returns>
      /// The callback result
      /// </returns>
      public String Callback(String message)
      {
         return OperationContext.Current
            .GetCallbackChannel<IDuplexCallback>()
            .Reverse(message);
      }
      #endregion
   }
}
