using ICU4N.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable

namespace ICU4N.Impl.Locale
{
    public struct AsciiCaseInsensitiveKey : IEquatable<AsciiCaseInsensitiveKey>
    {
        private string value;
        private int hashCode;
        public AsciiCaseInsensitiveKey(string value)
        {
            this.value = value;
            hashCode = AsciiUtil.GetHashCodeOrdinalIgnoreCase(value.AsSpan());
        }

        public bool Equals(ReadOnlySpan<char> other)
        {
            return AsciiUtil.CaseIgnoreMatch(value.AsSpan(), other);
        }

        public bool Equals(string other)
        {
            return AsciiUtil.CaseIgnoreMatch(value, other);
        }

        public bool Equals(AsciiCaseInsensitiveKey other)
        {
            return Equals(other.value);
        }

        public override bool Equals(object? obj)
        {
            if (obj is AsciiCaseInsensitiveKey other)
                return Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            return hashCode;
        }
    }
}
