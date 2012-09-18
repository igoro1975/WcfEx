//===========================================================================
// MODULE:  StaticRandom.cs
// PURPOSE: UDP gossip sample random number generator
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

namespace WcfEx.Samples.Gossip
{
   /// <summary>
   /// Static random number generator
   /// </summary>
   /// <remarks>
   /// This class supports generating random numbers without allocating a 
   /// new Random() instance for each number. It is also thread-safe, 
   /// avoiding the need to serialize access to the Random instance when
   /// sharing RNGs.
   /// </remarks>
   public static class StaticRandom
   {
      [ThreadStatic]
      private static Random rng;
      private static Random Rng
      {
         get { return rng != null ? rng : rng = new Random(Thread.CurrentThread.ManagedThreadId); }
      }

      /// <summary>
      /// Retrieves the next integer random number
      /// within a specified range
      /// </summary>
      /// <param name="min">
      /// The lower (inclusive) bound for the generated number
      /// </param>
      /// <param name="max">
      /// The upper (exclusive) bound for the generated number
      /// </param>
      /// <returns>
      /// The generated random number
      /// </returns>
      public static Int32 Next (Int32 min, Int32 max)
      {
         return Rng.Next(min, max);
      }
      /// <summary>
      /// Retrieves the next integer random number centered
      /// around a given value
      /// </summary>
      /// <param name="center">
      /// The center value
      /// </param>
      /// <param name="radius">
      /// The maximum distance (inclusive) from the center
      /// </param>
      /// <returns>
      /// The generated random number
      /// </returns>
      public static Int32 NextCentered (Int32 center, Int32 radius)
      {
         return Rng.Next(center - radius, center + radius + 1);
      }
      /// <summary>
      /// Retrieves a random floating-point number
      /// in the range [0,1)
      /// </summary>
      /// <returns>
      /// The generated random number
      /// </returns>
      public static Double NextDouble ()
      {
         return Rng.NextDouble();
      }
   }
}
