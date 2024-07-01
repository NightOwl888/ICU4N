using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#nullable enable

namespace ICU4N.Impl.Locale
{
    public abstract class AsciiCharComparer : IEqualityComparer<char>, IComparer<char>
    {
        public static AsciiCharComparer Ordinal { get; } = new OrdinalComparer(ignoreCase: false);

        public static AsciiCharComparer OrdinalIgnoreCase { get; } = new OrdinalComparer(ignoreCase: true);

        public abstract int Compare(char x, char y);
        public abstract bool Equals(char x, char y);
        public abstract int GetHashCode([DisallowNull] char obj);

        private class OrdinalComparer : AsciiCharComparer
        {
            private readonly bool ignoreCase;
            public OrdinalComparer(bool ignoreCase)
            {
                this.ignoreCase = ignoreCase;
            }

            public override int Compare(char x, char y)
            {
                if (ignoreCase)
                {
                    return AsciiUtil.ToLower(x).CompareTo(AsciiUtil.ToLower(y));
                }

                return x.CompareTo(y);
            }

            public override bool Equals(char x, char y)
            {
                if (ignoreCase)
                {
                    return AsciiUtil.ToLower(x) == AsciiUtil.ToLower(y);
                }
                return x == y;
            }

            public override int GetHashCode([DisallowNull] char obj)
            {
                if (ignoreCase)
                {
                    return AsciiUtil.ToLower(obj).GetHashCode();
                }

                return obj.GetHashCode();
            }
        }
    }
}
