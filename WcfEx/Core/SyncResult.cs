//===========================================================================
// MODULE:  SyncResult.cs
// PURPOSE: generic synchronous IAsyncResult implementation
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
using System.Threading;
// Project References

namespace WcfEx
{
   /// <summary>
   /// Synchronous operation result
   /// </summary>
   /// <remarks>
   /// This class provides support for implementing synchronous processing 
   /// under the Asynchronous Programming Model (APM). Clients may call the 
   /// asynchronous (Begin/End) versions of an operation, and the implementor 
   /// may return a SyncResult if it was possible to complete the operation 
   /// without making a context switch. This is useful for operations that 
   /// require no delay to process, but still need to support the APM 
   /// interface.
   /// </remarks>
   public sealed class SyncResult : IAsyncResult
   {
      #region Internal Data Members
      private Object state;
      private Object result;
      private Lazy<ManualResetEvent> completeEvent =
         new Lazy<ManualResetEvent>(() => new ManualResetEvent(true));
      #endregion

      #region Construction/Disposal
      /// <summary>
      /// Initializes a new result instance
      /// </summary>
      /// <param name="callback">
      /// The delegate to call for APM completion
      /// </param>
      /// <param name="state">
      /// The state parameter for the APM operation
      /// </param>
      /// <param name="result">
      /// The optional result of the synchronous operation
      /// </param>
      public SyncResult (AsyncCallback callback, Object state, Object result = null)
      {
         this.state = state;
         this.result = result;
         try
         {
            if (callback != null)
               callback(this);
         }
         catch { }
      }
      #endregion

      #region Result Operations
      /// <summary>
      /// Retrieves the result of the operation
      /// </summary>
      /// <returns>
      /// The operation result
      /// </returns>
      public Object GetResult ()
      { 
         return this.result;
      }
      /// <summary>
      /// Retrieves the result of the operation
      /// </summary>
      /// <returns>
      /// The operation result
      /// </returns>
      public TResult GetResult<TResult> ()
      {
         return (TResult)GetResult();
      }
      #endregion

      #region IAsyncResult Implementation
      /// <summary>
      /// The state object associated with this result
      /// </summary>
      public Object AsyncState
      {
         get { return this.state; }
      }
      /// <summary>
      /// The wait handle that can be used to
      /// await completion of the result
      /// </summary>
      public WaitHandle AsyncWaitHandle
      {
         get { return completeEvent.Value; }
      }
      /// <summary>
      /// Specifies whether the result completed
      /// in the Begin method
      /// </summary>
      public Boolean CompletedSynchronously
      {
         get { return true; }
      }
      /// <summary>
      /// Specifies whether the operation has 
      /// finished processing
      /// </summary>
      public Boolean IsCompleted
      {
         get { return true; }
      }
      #endregion
   }
}
