// 
//  Copyright 2009-2017  Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using ICU4N.Support.Numerics.BigMath;
using System;

namespace ICU4N.Numerics.BigMath
{
    /// <summary>
    /// Enumerates the possible strategies used to round results.
    /// </summary>
    internal enum RoundingMode // ICU4N TODO: Clean up and make public
    {
        /// <summary>
        /// Positive values are rounded towards positive infinity and negative 
        /// values towards negative infinity. (<c>x.Round().Abs() >= x.Abs()</c>
        /// </summary>
        Up = 0,

        /// <summary>
        /// The values are rounded towards zero. (<c>x.Round().Abs() &lt;= x.abs()</c>)
        /// </summary>
        Down = 1,

        /// <summary>
        /// Rounds towards positive infinity: for positive values this rounding mode behaves 
        /// as <see cref="Up"/>, for negative values as <see cref="Down"/> (<c>x.round() &gt;= x</c>)
        /// </summary>
        Ceiling = 2,

        /// <summary>
        /// Rounds towards negative infinity: for positive values this rounding mode behaves 
        /// as <see cref="Down"/>, for negative values as <see cref="Up"/> (<c>x.Round() &lt;= x</c>).
        /// </summary>
        Floor = 3,

        /// <summary>
        /// Values are rounded towards the nearest neighbor and ties are broken by rounding up.
        /// </summary>
        HalfUp = 4,

        /// <summary>
        /// Values are rounded towards the nearest neighbor and ties are broken by rounding down.
        /// </summary>
        HalfDown = 5,

        /// <summary>
        /// Values are rounded towards the nearest neighbor and ties are broken by rounding to the 
        /// even neighbor.
        /// </summary>
        HalfEven = 6,

        /// <summary>
        /// The rounding operations throws an <see cref="ArithmeticException"/> for the case that rounding 
        /// is necessary (i.e. for the case that the value cannot be represented exactly)
        /// </summary>
        Unnecessary = 7
    }

    /// <summary>
    /// Extensions to <see cref="RoundingMode"/>.
    /// </summary>
    internal static class RoundingModeExtensions
    {
        /// <summary>
        /// Returns a boolean telling whether a given integral value, or its name as a string, exists in
        /// a specified enumeration.
        /// </summary>
        /// <param name="roundingMode">The value or name of a constant.</param>
        /// <returns><c>true</c> if a given integral value, or its name as a string, exists in a specified
        /// enumeration; <c>false</c> otherwise.</returns>
        public static bool IsDefined(this RoundingMode roundingMode)
        {
            return roundingMode >= RoundingMode.Up && roundingMode <= RoundingMode.Unnecessary;
        }
    }
}