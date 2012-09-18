//===========================================================================
// MODULE:  AsyncResult.cs
// PURPOSE: generic asynchronous IAsyncResult implementation
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
   /// Asynchronous operation result
   /// </summary>
   /// <remarks>
   /// This class provides support for operation completion under the 
   /// Asynchronous Programming Model (APM). To use, construct an
   /// instance in the APM Begin method, and return that to the client.
   /// When the asynchronous operation completes, call the Complete
   /// method. Finally, within the APM End method, call GetResult()
   /// to ensure that the method completed and return the result.
   /// </remarks>
   public sealed class AsyncResult : IAsyncResult
   {
      #region Internal Data Members
      private static Object dummy = new Object();
      private AsyncCallback callback;
      private Object state;
      private TimeSpan timeout;
      private Object result;
      private Exception exception;
      private ManualResetEvent completeEvent;
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
      public AsyncResult (AsyncCallback callback, Object state)
         : this(callback, state, TimeSpan.MaxValue)
      {
      }
      /// <summary>
      /// Initializes a new result instance
      /// </summary>
      /// <param name="callback">
      /// The delegate to call for APM completion
      /// </param>
      /// <param name="state">
      /// The state parameter for the APM operation
      /// </param>
      /// <param name="timeout">
      /// The default operation timeout
      /// </param>
      public AsyncResult (AsyncCallback callback, Object state, TimeSpan timeout)
      {
         this.callback = callback;
         this.state = state;
         this.timeout = timeout;
      }
      #endregion

      #region Properties
      /// <summary>
      /// The optional result timeout
      /// </summary>
      public TimeSpan Timeout { get { return this.timeout; } }
      /// <summary>
      /// Specifies whether the async result has failed
      /// </summary>
      public Boolean IsFaulted { get { return this.exception != null; } }
      #endregion

      #region Result Operations
      /// <summary>
      /// Waits for completion, and retrieves the 
      /// result of the async operation
      /// </summary>
      /// <returns>
      /// The operation result
      /// </returns>
      public Object GetResult ()
      {
         ((IAsyncResult)this).WaitFor();
         if (this.exception != null)
            throw this.exception;
         return (this.result != dummy) ? this.result : null;
      }
      /// <summary>
      /// Waits for completion, and retrieves the 
      /// result of the async operation
      /// </summary>
      /// <returns>
      /// The operation result
      /// </returns>
      public TResult GetResult<TResult> ()
      {
         return (TResult)GetResult();
      }
      /// <summary>
      /// Signals completion of the async operation
      /// </summary>
      /// <param name="result">
      /// The operation result
      /// </param>
      /// <param name="exception">
      /// The operation exception, if any
      /// </param>
      public void Complete (Object result, Exception exception = null)
      {
         // use the dummy object to stand in for
         // the null result value, since we use
         // the null state to determine completion
         if (result == null)
            result = dummy;
         // ensure atomic completion
         this.exception = exception;
         if (Interlocked.CompareExchange(ref this.result, result, null) != null)
            throw new InvalidOperationException("Attempt to complete a result multiple times");
         // signal the completion event (if allocated)
         // and dispatch the callback (if specified)
         if (this.completeEvent != null)
            this.completeEvent.Set();
         try
         {
            if (this.callback != null)
               this.callback(this);
         }
         catch { }
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
         get
         {
            // lazily allocate and atomically assign the
            // completion event, and set it if completed
            if (this.completeEvent == null)
               if (Interlocked.CompareExchange(
                     ref this.completeEvent, 
                     new ManualResetEvent(false), 
                     null) == null)
                  if (this.IsCompleted)
                     this.completeEvent.Set();
            return this.completeEvent;
         }
      }
      /// <summary>
      /// Specifies whether the result completed
      /// in the Begin method
      /// </summary>
      public Boolean CompletedSynchronously
      {
         get { return false; }
      }
      /// <summary>
      /// Specifies whether the operation has 
      /// finished processing
      /// </summary>
      public Boolean IsCompleted
      {
         get { return (this.result != null); }
      }
      #endregion
   }

   /// <summary>
   /// IAsyncResult extension methods
   /// </summary>
   public static class IAsyncResultExtensions
   {
      /// <summary>
      /// Suspends the current thread until the
      /// asynchronous operation has completed
      /// </summary>
      /// <param name="result">
      /// The result to wait on
      /// </param>
      /// <param name="timeout">
      /// An override for the default result timeout
      /// </param>
      /// <returns>
      /// True if the result completed within the timeout
      /// False otherwise
      /// </returns>
      public static Boolean TryWaitFor (this IAsyncResult result, TimeSpan timeout)
      {
         return (
            result.IsCompleted ||
            result.AsyncWaitHandle.WaitOne(
               (Int32)Math.Min(timeout.TotalMilliseconds, Int32.MaxValue)
            )
         );
      }
      /// <summary>
      /// Waits indefinitely for a result to complete
      /// </summary>
      /// <param name="result">
      /// The result to wait on
      /// </param>
      public static void WaitFor (this IAsyncResult result)
      {
         if (!result.IsCompleted)
         {
            AsyncResult ar = result as AsyncResult;
            TimeSpan timeout = (ar != null) ? ar.Timeout : TimeSpan.MaxValue;
            if (!result.TryWaitFor(timeout))
               throw new TimeoutException();
         }
      }
      /// <summary>
      /// Waits for a result to complete
      /// </summary>
      /// <param name="result">
      /// The result to wait on
      /// </param>
      /// <param name="timeout">
      /// The maximum amount of time to wait
      /// </param>
      public static void WaitFor (this IAsyncResult result, TimeSpan timeout)
      {
         if (!result.TryWaitFor(timeout))
            throw new TimeoutException();
      }
   }
}
