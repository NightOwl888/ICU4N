using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICU4N.Text
{
    internal ref partial struct ValueStringBuilder
    {
        public void AppendUpperInvariant(string value) => AppendUpperInvariant(value.AsSpan());

        public void AppendUpperInvariant(ReadOnlySpan<char> value)
        {
            int valueLength = value.Length;
            int pos = _pos;
            if (pos > _chars.Length - valueLength)
            {
                Grow(valueLength);
            }

            int length = value.ToUpperInvariant(_chars.Slice(_pos));
            while (length < 0) // rare
            {
                Grow(valueLength);
                length = value.ToUpperInvariant(_chars.Slice(_pos));
            }
            _pos += length;
            UpdateMaxLength();
        }

        public void AppendLowerInvariant(string value) => AppendLowerInvariant(value.AsSpan());

        public void AppendLowerInvariant(ReadOnlySpan<char> value)
        {
            int valueLength = value.Length;
            int pos = _pos;
            if (pos > _chars.Length - valueLength)
            {
                Grow(valueLength);
            }

            int length = value.ToLowerInvariant(_chars.Slice(_pos));
            while (length < 0) // rare
            {
                Grow(valueLength);
                length = value.ToLowerInvariant(_chars.Slice(_pos));
            }
            _pos += length;
            UpdateMaxLength();
        }
    }
}
