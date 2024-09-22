using ICU4N.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable

namespace ICU4N.Impl.Locale
{
    public abstract class AsciiStringComparer : StringComparer
    {
        new public static AsciiStringComparer Ordinal { get; } = new OrdinalComparer(ignoreCase: false);

        new public static AsciiStringComparer OrdinalIgnoreCase { get; } = new OrdinalComparer(ignoreCase: true);

        public abstract int Compare(ReadOnlySpan<char> x, ReadOnlySpan<char> y);

        public abstract bool Equals(ReadOnlySpan<char> x, ReadOnlySpan<char> y);

        public abstract int GetHashCode(ReadOnlySpan<char> obj);

        private class OrdinalComparer : AsciiStringComparer
        {
            private readonly bool ignoreCase;
            public OrdinalComparer(bool ignoreCase)
            {
                this.ignoreCase = ignoreCase;
            }

            public override int Compare(ReadOnlySpan<char> x, ReadOnlySpan<char> y)
            {
                if (x[0] - y[0] != 0)
                {
                    return x[0] - y[0];
                }

                if (ignoreCase)
                {
                    return AsciiUtil.CaseIgnoreCompare(x, y);
                }

                return System.MemoryExtensions.CompareTo(x, y, StringComparison.Ordinal);
            }

            public override int Compare(string? x, string? y)
            {
                if (ignoreCase)
                {
                    return AsciiUtil.CaseIgnoreCompare(x, y);
                }
                return StringComparer.Ordinal.Compare(x, y);
            }

            public override bool Equals(ReadOnlySpan<char> x, ReadOnlySpan<char> y)
            {
                if (ignoreCase)
                {
                    return AsciiUtil.CaseIgnoreMatch(x, y);
                }
                return x.Equals(y, StringComparison.Ordinal);
            }

            public override bool Equals(string? x, string? y)
            {
                if (ReferenceEquals(x, y))
                    return true;

                if (x is null)
                    return y is null;
                else if (y is null)
                    return false;

                return Equals(x, y);
            }

            public override int GetHashCode(ReadOnlySpan<char> obj)
            {
                if (ignoreCase)
                {
                    return AsciiUtil.GetHashCodeOrdinalIgnoreCase(obj);
                }
                return StringHelper.GetHashCode(obj);
            }

            public override int GetHashCode(string? obj)
            {
                if (obj is null)
                    return 0;

                return GetHashCode(obj.AsSpan());
            }
        }
    }
}
