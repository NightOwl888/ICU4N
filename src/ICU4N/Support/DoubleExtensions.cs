// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace ICU4N.Support
{
    /// <summary>
    /// Extensions to <see cref="double"/>.
    /// </summary>
    internal static class DoubleExtensions
    {
        /// <summary>
        /// Determines if a value represents an integral value.
        /// </summary>
        /// <param name="value">The value to be checked.</param>
        /// <returns><c>true</c> if <paramref name="value"/> is an integer; otherwise, <c>false</c>.</returns>
        public static bool IsInteger(this double value)
        {
            return IsFinite(value) && (value == Math.Truncate(value));
        }

        /// <summary>
        /// Determines whether the specified value is finite (zero, subnormal, or normal).
        /// </summary>
        /// <param name="value">A double-precision floating-point number.</param>
        /// <returns><c>true</c> if the <paramref name="value"/> is finite (zero, subnormal or normal); <c>false</c> otherwise.</returns>
        public static bool IsFinite(this double value)
        {
            long bits = BitConverter.DoubleToInt64Bits(value);
            return (bits & 0x7FFFFFFFFFFFFFFF) < 0x7FF0000000000000;
        }
    }
}
