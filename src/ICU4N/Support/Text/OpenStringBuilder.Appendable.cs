using J2N.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable

namespace ICU4N.Text
{
    internal sealed partial class OpenStringBuilder
    {
        public OpenStringBuilder Append(ReadOnlySpan<char> value)
        {
            int pos = _pos;
            if (pos > _chars.Length - value.Length)
            {
                Grow(value.Length);
            }

            value.CopyTo(_chars.AsSpan(_pos));
            _pos += value.Length;
            return this;
        }

        public OpenStringBuilder Append(string? value, int startIndex, int count)
        {
            if (value is null || count == 0) return this;

            return Append(value.AsSpan(startIndex, count));
        }

        public OpenStringBuilder Append(ReadOnlySpan<char> value, int startIndex, int count)
        {
            if (value == default || count == 0) return this;

            return Append(value.Slice(startIndex, count));
        }

        public OpenStringBuilder Append(StringBuilder? value) => Append(value, 0, value?.Length ?? 0);

        public OpenStringBuilder Append(StringBuilder? value, int startIndex, int count)
        {
            Debug.Assert(startIndex >= 0);
            Debug.Assert(count >= 0);
            Debug.Assert(value is null || value?.Length - startIndex >= count);

            if (value is null || count == 0) return this;

            int pos = _pos;
            if (pos > _chars.Length - count)
            {
                Grow(count);
            }

            value.CopyTo(startIndex, _chars, _pos, count);
            _pos += count;
            return this;
        }

        public OpenStringBuilder Append(ICharSequence? value)
        {
            return Append(value, 0, value?.Length ?? 0);
        }

        public OpenStringBuilder Append(ICharSequence? value, int startIndex, int count) // ICU4N: changed to startIndex/length to match .NET
        {
            Debug.Assert(startIndex >= 0);
            Debug.Assert(count >= 0);

            if (value == null || !value.HasValue)
            {
                return this;
            }

            Debug.Assert(value.Length - startIndex >= count);

            if (value != null && value.HasValue)
            {
                if (value is StringCharSequence str)
                {
                    return Append(str.Value, startIndex, count);
                }
                if (value is CharArrayCharSequence chars)
                {
                    return Append(chars.Value, startIndex, count);
                }
                if (value is StringBuilderCharSequence sb)
                {
                    return Append(sb.Value, startIndex, count);
                }

                int pos = _pos;
                if (pos > _chars.Length - count)
                {
                    Grow(count);
                }

                for (int i = 0; i < count; i++)
                {
                    _chars[pos++] = value[i + startIndex];
                }
                _pos += count;
            }
            return this;
        }

        #region ISpanAppendable

        ISpanAppendable ISpanAppendable.Append(ReadOnlySpan<char> value) => Append(value);

        #endregion ISpanAppendable

        #region IAppendable

        IAppendable IAppendable.Append(char value) => Append(value);

        IAppendable IAppendable.Append(string? value)
        {
            throw new NotImplementedException();
        }

        IAppendable IAppendable.Append(string? value, int startIndex, int count) => Append(value, startIndex, count);

        IAppendable IAppendable.Append(StringBuilder? value) => Append(value);

        IAppendable IAppendable.Append(StringBuilder? value, int startIndex, int count)
        {
            throw new NotImplementedException();
        }

        IAppendable IAppendable.Append(char[]? value) => Append(value);

        IAppendable IAppendable.Append(char[]? value, int startIndex, int count) => Append(value, startIndex, count);

        IAppendable IAppendable.Append(ICharSequence? value) => Append(value);

        IAppendable IAppendable.Append(ICharSequence? value, int startIndex, int count) => Append(value, startIndex, count);

        #endregion IAppendable
    }
}
