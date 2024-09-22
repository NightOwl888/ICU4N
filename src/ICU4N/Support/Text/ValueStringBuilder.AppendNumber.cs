using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable

namespace ICU4N.Text
{
    // ICU4N NOTE: These default to invariant culture, not current culture, but can be overridden unlike StringBuilder.

    internal ref partial struct ValueStringBuilder
    {
        //[CLSCompliant(false)]
        public void Append(sbyte value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => AppendSpanFormattable(value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Append(value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

        public void Append(byte value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => AppendSpanFormattable(value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Append(value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

        public void Append(short value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => AppendSpanFormattable(value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Append(value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

        public void Append(int value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => AppendSpanFormattable(value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Append(value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

        public void Append(long value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => AppendSpanFormattable(value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Append(value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

        public void Append(float value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => AppendSpanFormattable(value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Append(value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

        public void Append(double value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => AppendSpanFormattable(value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Append(value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

        public void Append(decimal value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => AppendSpanFormattable(value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Append(value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

        //[CLSCompliant(false)]
        public void Append(ushort value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => AppendSpanFormattable(value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Append(value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

        //[CLSCompliant(false)]
        public void Append(uint value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => AppendSpanFormattable(value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Append(value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

        //[CLSCompliant(false)]
        public void Append(ulong value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => AppendSpanFormattable(value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Append(value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif




        //[CLSCompliant(false)]
        public void Insert(int index, sbyte value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => InsertSpanFormattable(index, value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Insert(index, value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

        public void Insert(int index, byte value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => InsertSpanFormattable(index, value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Insert(index, value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

        public void Insert(int index, short value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => InsertSpanFormattable(index, value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Insert(index, value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

        public void Insert(int index, int value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => InsertSpanFormattable(index, value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Insert(index, value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

        public void Insert(int index, long value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => InsertSpanFormattable(index, value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Insert(index, value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

        public void Insert(int index, float value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => InsertSpanFormattable(index, value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Insert(index, value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

        public void Insert(int index, double value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => InsertSpanFormattable(index, value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Insert(index, value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

        public void Insert(int index, decimal value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => InsertSpanFormattable(index, value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Insert(index, value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

        //[CLSCompliant(false)]
        public void Insert(int index, ushort value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => InsertSpanFormattable(index, value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Insert(index, value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

        //[CLSCompliant(false)]
        public void Insert(int index, uint value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => InsertSpanFormattable(index, value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Insert(index, value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

        //[CLSCompliant(false)]
        public void Insert(int index, ulong value, string? format = null, IFormatProvider? provider = null)

#if FEATURE_SPANFORMATTABLE
            => InsertSpanFormattable(index, value, format, provider ?? NumberFormatInfo.InvariantInfo);
#else
            => Insert(index, value.ToString(format, provider ?? NumberFormatInfo.InvariantInfo));
#endif

    }
}
