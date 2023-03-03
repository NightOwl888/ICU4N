using System;

namespace ICU4N.Text
{
    // ICU4N TODO: Remove once MeasureFormat is ported. This stub is just for XML docs.
    internal class MeasureFormat
    {
        public enum FormatWidth { }
    }
}

namespace ICU4N.Util
{
    /// <summary>
    /// An amount of a specified unit, consisting of a Number and a Unit.
    /// For example, a length measure consists of a Number and a length
    /// unit, such as feet or meters.
    /// <para/>
    /// Measure objects are parsed and formatted by subclasses of
    /// <see cref="ICU4N.Text.MeasureFormat"/>.
    /// <para/>
    /// Measure objects are immutable. All subclasses must guarantee that.
    /// (However, subclassing is discouraged.)
    /// </summary>
    /// <seealso cref="J2N.Numerics.Number"/>
    /// <seealso cref="MeasureUnit"/>
    /// <seealso cref="ICU4N.Text.MeasureFormat"/>
    /// <author>Alan Liu</author>
    /// <stable>ICU 3.0</stable>
    // ICU4N TODO: API - Make generic? Generally subclasses will only be 1 numeric type
    internal class Measure // ICU4N TODO: API - this was public in ICU4J
    {
        private readonly J2N.Numerics.Number number;
        private readonly MeasureUnit unit;

        /// <summary>
        /// Constructs a new object given a number and a unit.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="unit">The unit.</param>
        /// <exception cref="ArgumentNullException"><paramref name="number"/> or <paramref name="unit"/> is <c>null</c>.</exception>
        /// <stable>ICU 3.0</stable>
        public Measure(J2N.Numerics.Number number, MeasureUnit unit)
        {
            this.number = number ?? throw new ArgumentNullException(nameof(number), "Number and MeasureUnit must not be null");
            this.unit = unit ?? throw new ArgumentNullException(nameof(unit), "Number and MeasureUnit must not be null");
        }

        /// <summary>
        /// Returns <c>true</c> if the given object is equal to this object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><c>true</c> if this object is equal to the given object; otherwise <c>false</c>.</returns>
        /// <stable>ICU 3.0</stable>
        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (!(obj is Measure m))
            {
                return false;
            }
            return unit.Equals(m.unit) && NumbersEqual(number, m.number);
        }

        /// <summary>
        /// See if two numbers are identical or have the same double value.
        /// </summary>
        /// <param name="a">A number.</param>
        /// <param name="b">Another number to be compared with.</param>
        /// <returns>Returns true if two numbers are identical or have the same <see cref="double"/> value.</returns>
        // TODO improve this to catch more cases (two different longs that have same double values, BigDecimals, etc)
        private static bool NumbersEqual(J2N.Numerics.Number a, J2N.Numerics.Number b)
        {
            if (a.Equals(b))
            {
                return true;
            }
            if (a.ToDouble() == b.ToDouble())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a hashcode for this object.
        /// </summary>
        /// <returns>A 32-bit hash.</returns>
        /// <stable>ICU 3.0</stable>
        public override int GetHashCode()
        {
            return 31 * number.ToDouble().GetHashCode() + unit.GetHashCode();
        }

        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        /// <returns>A string representation consisting of the ISO currency
        /// code together with the numeric amount.</returns>
        /// <stable>ICU 3.0</stable>
        public override string ToString()
        {
            return number.ToString() + ' ' + unit.ToString();
        }

        /// <summary>
        /// Gets the numeric value of this object.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public J2N.Numerics.Number Number => number;

        /// <summary>
        /// Gets the unit of this object.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public MeasureUnit Unit => unit;
    }
}
