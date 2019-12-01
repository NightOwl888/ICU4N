using J2N;
using System;
using System.Globalization;

namespace ICU4N.Support
{
    /// <summary>
    /// A simple class for number conversions.
    /// </summary>
    public static class Number
    {
        /// <summary>
        /// Converts a number to System.String.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static System.String ToString(double d)
        {
            if ((double)(int)d == d)
            {
                // Special case: When we have an integer value,
                // the standard .NET formatting removes the decimal point
                // and everything to the right. But we need to always
                // have at least decimal place to match Java.
                return d.ToString("0.0", CultureInfo.InvariantCulture);
            }
            else
            {
                // Although the MSDN documentation says that 
                // round-trip on float will be limited to 7 decimals, it appears
                // not to be the case. Also, when specifying "0.0######", we only
                // get a result to 6 decimal places maximum. So, we must round before
                // doing a round-trip format to guarantee 7 decimal places.
                return Math.Round(d, 7).ToString("R", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Returns the signum function of the specified <see cref="int"/> value.  (The
        /// return value is -1 if the specified value is negative; 0 if the
        /// specified value is zero; and 1 if the specified value is positive.)
        /// <para/>
        /// This can be useful for testing the results of two <see cref="IComparable{T}.CompareTo(T)"/>
        /// methods against each other, since only the sign is guaranteed to be the same between impementations.
        /// </summary>
        /// <returns>the signum function of the specified <see cref="int"/> value.</returns>
        public static int Signum(int i)
        {
            // HD, Section 2-7
            return (i >> 31) | (-i.TripleShift(31));
        }

        public static bool IsNumber(this object value)
        {
            return value is sbyte
                   || value is byte
                   || value is short
                   || value is ushort
                   || value is int
                   || value is uint
                   || value is long
                   || value is ulong
                   || value is float
                   || value is double
                   || value is decimal;
        }
    }
}
