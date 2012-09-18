//===========================================================================
// MODULE:  UdpSocket.cs
// PURPOSE: UDP socket wrapper class
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
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
// Project References

namespace WcfEx.Udp
{
   /// <summary>
   /// UDP socket
   /// </summary>
   /// <remarks>
   /// This class encapsulates a framework socket and simplifies
   /// its interface for the WCF UDP transport.
   /// </remarks>
   internal sealed class UdpSocket : IDisposable
   {
      static Byte[] flushBuffer = new Byte[2048];
      Socket socket;
      EndPoint remoteEP;

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new socket instance
      /// </summary>
      public UdpSocket()
      {
      }
      /// <summary>
      /// Releases socket resources
      /// </summary>
      public void Dispose()
      {
         if (this.socket != null)
            this.socket.Close();
      }
      #endregion

      #region Properties
      /// <summary>
      /// Socket send timeout, in milliseconds
      /// </summary>
      public Int32 SendTimeout
      {
         get; set;
      }
      /// <summary>
      /// Size of the socket send buffer,
      /// for buffered sockets
      /// </summary>
      public Int32 SendBufferSize
      {
         get; set;
      }
      /// <summary>
      /// Size of the socket receive buffer,
      /// for buffered sockets
      /// </summary>
      public Int32 ReceiveBufferSize
      {
         get; set;
      }
      /// <summary>
      /// Specifies whether the configured
      /// address/port can be reused during
      /// listener binding
      /// </summary>
      public Boolean ReuseAddress
      {
         get; set;
      }
      #endregion

      #region Operations
      /// <summary>
      /// Creates a new system socket
      /// </summary>
      /// <param name="af">
      /// Address family of the socket
      /// </param>
      private void Create (AddressFamily af)
      {
         if (this.socket != null)
            throw new InvalidOperationException();
         this.socket = new Socket(
            af,
            SocketType.Dgram,
            ProtocolType.Udp
         );
         if (af == AddressFamily.InterNetwork)
            this.socket.DontFragment = true;
         this.socket.EnableBroadcast = true;
         this.socket.SendTimeout = this.SendTimeout;
         this.socket.ReceiveTimeout = 1;
         this.socket.SendBufferSize = this.SendBufferSize;
         this.socket.ReceiveBufferSize = this.ReceiveBufferSize;
         if (this.ReuseAddress)
            this.socket.SetSocketOption(
               SocketOptionLevel.Socket, 
               SocketOptionName.ReuseAddress, 
               true
            );
         this.socket.SetSocketOption(
            (af == AddressFamily.InterNetwork) ? SocketOptionLevel.IP : SocketOptionLevel.IPv6,
            SocketOptionName.PacketInformation,
            true
         );
         // disable SIO_UDP_CONNRESET for UDP sockets with no remote listener
         // http://stackoverflow.com/questions/5199026/c-sharp-async-udp-listener-socketexception
         this.socket.IOControl(
            -1744830452,   // SIO_UDP_CONNRESET
            new[] { Convert.ToByte(false) },
            null
         );
      }
      /// <summary>
      /// Binds the socket to a local address
      /// </summary>
      /// <param name="ep">
      /// The UDP endpoint of the socket
      /// </param>
      public void Bind(EndPoint ep)
      {
         Create(ep.AddressFamily);
         this.socket.Bind(ep);
      }
      /// <summary>
      /// Binds the socket to a remote address
      /// </summary>
      /// <param name="ep">
      /// The UDP endpoint to bind
      /// </param>
      public void Connect(EndPoint ep)
      {
         // initialize the socket and attach the connected endpoint
         // do not call Connect on the socket, because doing so
         // prevents receives on the broadcast address - all
         // UDP Receive calls will specify the remote endpoint,
         // simulating the socket Connect mechanism for UDP
         Create(ep.AddressFamily);
         this.socket.Bind(
            new IPEndPoint(MapAnyAddress(ep.AddressFamily), 0)
         );
         this.remoteEP = ep;
      }
      /// <summary>
      /// Sends a buffer to an unbound socket
      /// </summary>
      /// <param name="buffer">
      /// The buffer to transfer
      /// </param>
      public void Send (ArraySegment<Byte> buffer)
      {
         Send(this.remoteEP, buffer);
      }
      /// <summary>
      /// Sends a buffer to an unbound socket
      /// </summary>
      /// <param name="ep">
      /// The address of the host to send to
      /// </param>
      /// <param name="buffer">
      /// The buffer to transfer
      /// </param>
      public void Send(EndPoint ep, ArraySegment<Byte> buffer)
      {
         try
         {
            this.socket.SendTo(
               buffer.Array,
               buffer.Offset,
               buffer.Count,
               SocketFlags.None,
               ep
            );
         }
         catch (SocketException e)
         {
            throw MapException(e);
         }
      }
      /// <summary>
      /// Starts an asynchronous socket read operation
      /// </summary>
      /// <param name="buffer">
      /// The socket receive buffer
      /// </param>
      /// <param name="callback">
      /// Asynchronous completion delegate
      /// </param>
      /// <param name="state">
      /// Asynchronous completion delegate parameter
      /// </param>
      /// <returns>
      /// The asynchronous completion token
      /// </returns>
      public IAsyncResult BeginReceive (
         ArraySegment<Byte> buffer, 
         AsyncCallback callback, 
         Object state)
      {
         try
         {
            AsyncResult result = new AsyncResult(callback, state);
            ReceiveResult recvResult = new ReceiveResult()
            {
               Buffer = buffer,
               Endpoint = GetReceiveEndpoint(),
               AsyncResult = result
            };
            this.socket.BeginReceiveFrom(
               buffer.Array,
               buffer.Offset,
               buffer.Count,
               SocketFlags.None,
               ref recvResult.Endpoint,
               this.OnReceiveFrom,
               recvResult
            );
            return result;
         }
         catch (SocketException e)
         {
            throw MapException(e);
         }
      }
      /// <summary>
      /// Completes an asynchronous socket read operation
      /// </summary>
      /// <param name="result">
      /// The asynchronous completion token
      /// </param>
      /// <returns>
      /// The buffer subarray containing the data received
      /// </returns>
      public ArraySegment<Byte> EndReceive (IAsyncResult result)
      {
         EndPoint ep;
         return EndReceive(result, out ep);
      }
      /// <summary>
      /// Completes an asynchronous socket read operation
      /// </summary>
      /// <param name="result">
      /// The asynchronous completion token
      /// </param>
      /// <param name="ep">
      /// Return the address of the sending host via here
      /// </param>
      /// <returns>
      /// The buffer subarray containing the data received
      /// </returns>
      public ArraySegment<Byte> EndReceive (IAsyncResult result, out EndPoint ep)
      {
         try
         {
            ReceiveResult recvResult = ((AsyncResult)result).GetResult<ReceiveResult>();
            ep = recvResult.Endpoint;
            return recvResult.Buffer;
         }
         catch (SocketException e)
         {
            throw MapException(e);
         }
      }
      /// <summary>
      /// Socket async receive callback
      /// </summary>
      /// <param name="result">
      /// The completed socket result
      /// </param>
      private void OnReceiveFrom (IAsyncResult result)
      {
         ReceiveResult recvResult = (ReceiveResult)result.AsyncState;
         Int32 received = 0;
         Exception exception = null;
         try
         {
            received = this.socket.EndReceiveFrom(result, ref recvResult.Endpoint);
         }
         catch (Exception e)
         {
            exception = e;
         }
         recvResult.Buffer = new ArraySegment<Byte>(
            recvResult.Buffer.Array, 
            recvResult.Buffer.Offset, 
            received
         );
         recvResult.AsyncResult.Complete(recvResult, exception);
         recvResult.AsyncResult = null;
      }
      /// <summary>
      /// Flushes the socket receive buffer, reading and
      /// discarding any outstanding data
      /// </summary>
      public void FlushReceive ()
      {
         try
         {
            for ( ; ; )
               if (this.socket.Receive(flushBuffer) == 0)
                  break;
         }
         catch (SocketException e)
         {
            if (e.SocketErrorCode != SocketError.TimedOut)
               throw MapException(e);
         }
      }
      /// <summary>
      /// Creates an endpoint structure for use in
      /// ReceiveFrom, based on the current connection state
      /// </summary>
      /// <returns></returns>
      private EndPoint GetReceiveEndpoint()
      {
         return this.remoteEP ?? new IPEndPoint(MapAnyAddress(this.socket.AddressFamily), 0);
      }
      /// <summary>
      /// Waits until data is available to
      /// be received on the socket
      /// </summary>
      /// <param name="timeout">
      /// The timeout for the poll operation
      /// </param>
      /// <returns>
      /// True if data is available on the socket
      /// False otherwise
      /// </returns>
      public Boolean Poll(TimeSpan timeout)
      {
         try
         {
            return this.socket.Poll(
               (Int32)Math.Min(timeout.TotalMilliseconds, Int32.MaxValue),
               SelectMode.SelectRead
            );
         }
         catch (SocketException e)
         {
            throw MapException(e);
         }
      }
      /// <summary>
      /// Utility for determining the socket address 
      /// family for a URI
      /// </summary>
      /// <param name="address">
      /// The UDP address URI to map
      /// </param>
      /// <returns>
      /// The mapped address family
      /// </returns>
      private static AddressFamily MapAddressFamily(Uri address)
      {
         switch (address.HostNameType)
         {
            case UriHostNameType.IPv4:
               return AddressFamily.InterNetwork;
            case UriHostNameType.IPv6:
               return AddressFamily.InterNetworkV6;
            default:
               return Dns.GetHostEntry(address.Host).AddressList[0].AddressFamily;
         }
      }
      /// <summary>
      /// Utility for constructing the "any" address
      /// for an address family
      /// </summary>
      /// <param name="family">
      /// The address family to map
      /// </param>
      /// <returns>
      /// The "any" address for the specified address family
      /// </returns>
      private static IPAddress MapAnyAddress (AddressFamily family)
      {
         switch (family)
         {
            case AddressFamily.InterNetwork:
               return IPAddress.Any;
            case AddressFamily.InterNetworkV6:
               return IPAddress.IPv6Any;
            default:
               throw new NotSupportedException();
         }
      }
      /// <summary>
      /// Utility for mapping a UDP address URI
      /// to a list of socket endpoint
      /// </summary>
      /// <param name="address">
      /// The UDP address URI to map
      /// </param>
      /// <returns>
      /// The mapped socket endpoint
      /// </returns>
      public static IEnumerable<EndPoint> MapEndpoints (Uri address)
      {
         HashSet<EndPoint> endpoints = new HashSet<EndPoint>();
         switch (address.HostNameType)
         {
            case UriHostNameType.IPv4:
               endpoints.Add(new IPEndPoint(IPAddress.Parse(address.Host), address.Port));
               break;
            case UriHostNameType.IPv6:
               endpoints.Add(new IPEndPoint(IPAddress.Parse(address.Host), address.Port));
               break;
            default:
               foreach (IPAddress addr in Dns.GetHostEntry(address.Host).AddressList)
                  endpoints.Add(new IPEndPoint(addr, address.Port));
               if (String.Compare(address.Host, Dns.GetHostName(), true) == 0)
               {
                  if (Socket.OSSupportsIPv4)
                     endpoints.Add(new IPEndPoint(IPAddress.Loopback, address.Port));
                  if (Socket.OSSupportsIPv6)
                     endpoints.Add(new IPEndPoint(IPAddress.IPv6Loopback, address.Port));
               }
               break;
         }
         return endpoints;
      }
      /// <summary>
      /// Maps a socket exception to a typed CLR/WCF exception,
      /// to prevent naked socket exceptions from being propagated
      /// up to the client
      /// </summary>
      /// <param name="e">
      /// The exception to map
      /// </param>
      /// <returns>
      /// The mapped exception
      /// </returns>
      public static Exception MapException (SocketException e)
      {
         switch (e.SocketErrorCode)
         {
            case SocketError.TimedOut:
               throw new TimeoutException("The socket operation timed out", e);
            case SocketError.OperationAborted:
               throw new OperationCanceledException("The socket operation was aborted", e);
            default:
               throw new CommunicationException(String.Format("Socket error {0}", e.ErrorCode), e);
         }
      }
      #endregion

      /// <summary>
      /// Async socket receive shim
      /// </summary>
      private class ReceiveResult
      {
         public ArraySegment<Byte> Buffer;
         public EndPoint Endpoint;
         public AsyncResult AsyncResult;
      }
   }
}
