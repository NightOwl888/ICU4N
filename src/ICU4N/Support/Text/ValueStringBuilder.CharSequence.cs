using J2N;
using J2N.Text;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Text;
#nullable enable

namespace ICU4N.Text
{
    internal ref partial struct ValueStringBuilder
    {
        // ICU4N: Returns the number of characters in the codepoint (eliminates a method in CaseMapImpl)
        public int AppendCodePoint(int codePoint)
        {
            int count = Character.ToChars(codePoint, out char high, out char low);

            int pos = _pos;
            if (pos > _chars.Length - count)
            {
                Grow(count);
            }

            _chars[pos++] = high;
            if (count == 2)
                _chars[pos++] = low;
            _pos += count;
            return count;
        }

        // ICU4N: Returns the number of characters in the codepoint
        public int InsertCodePoint(int index, int codePoint)
        {
            int count = Character.ToChars(codePoint, out char high, out char low);

            if (_pos > _chars.Length - count)
            {
                Grow(count);
            }

            int remaining = _pos - index;
            _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
            _chars[index] = high;
            if (count == 2)
                _chars[index + 1] = low;
            _pos += count;
            return count;
        }

        public int CodePointAt(int index) => AsSpan().CodePointAt(index);
        public int CodePointBefore(int index) => AsSpan().CodePointBefore(index);

        public int CodePointCount(int startIndex, int length) => AsSpan(startIndex, length).CodePointCount();

        public int OffsetByCodePoints(int index, int codePointOffset) => AsSpan().OffsetByCodePoints(index, codePointOffset);

        public void Append(string? value, int startIndex, int count)
        {
            if (value is null || count == 0) return;

            Append(value.AsSpan(startIndex, count));
        }

        public void Append(ReadOnlySpan<char> value, int startIndex, int count)
        {
            if (value == default || count == 0) return;

            Append(value.Slice(startIndex, count));
        }

        public void Append(StringBuilder? value) => Append(value, 0, value?.Length ?? 0);

        public void Append(StringBuilder? value, int startIndex, int count)
        {
            Debug.Assert(startIndex >= 0);
            Debug.Assert(count >= 0);
            Debug.Assert(value is null || value?.Length - startIndex >= count);

            if (value is null || count == 0) return;

            int pos = _pos;
            if (pos > _chars.Length - count)
            {
                Grow(count);
            }

            if (_arrayToReturnToPool is null)
            {
                // Only call our extension method (which may allocate a buffer) if we have to.
                value.CopyTo(startIndex, _chars.Slice(_pos), count);
            }
            else
            {
                // If we are already on the heap, we can use CopyTo(int, char[], int, int) for better efficiency
                value.CopyTo(startIndex, _arrayToReturnToPool, _pos, count);
            }
            _pos += count;
        }

        public void Append(ICharSequence? value) => Append(value, 0, value?.Length ?? 0);

        public void Append(ICharSequence? value, int startIndex, int count)
        {
            Debug.Assert(startIndex >= 0);
            Debug.Assert(count >= 0);

            if (value == null || !value.HasValue)
            {
                return;
            }

            Debug.Assert(value.Length - startIndex >= count);

            if (value != null && value.HasValue)
            {
                if (value is StringCharSequence str)
                {
                    Append(str.Value, startIndex, count);
                    return;
                }
                if (value is CharArrayCharSequence chars)
                {
                    Append(chars.Value, startIndex, count);
                    return;
                }
                if (value is StringBuilderCharSequence sb)
                {
                    Append(sb.Value, startIndex, count);
                    return;
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
        }

        public void Insert(int index, StringBuilder? value)
        {
            if (value == null)
            {
                return;
            }

            int count = value.Length;

            if (count == 0)
            {
                return;
            }

            Debug.Assert(index >= 0 && index <= value.Length);

            if (_pos > (_chars.Length - count))
            {
                Grow(count);
            }

            int remaining = _pos - index;
            _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
            if (_arrayToReturnToPool is null)
            {
                // Only call our extension method (which may allocate a buffer) if we have to.
                value.CopyTo(0, _chars.Slice(index), count);
            }
            else
            {
                // If we are already on the heap, we can use CopyTo(int, char[], int, int) for better efficiency
                value.CopyTo(0, _arrayToReturnToPool, index, count);
            }
            _pos += count;
        }

        public void Insert(int index, ICharSequence? value)
        {
            if (value == null || !value.HasValue)
            {
                return;
            }

            if (value is StringCharSequence str)
            {
                Insert(index, str.Value);
                return;
            }
            if (value is CharArrayCharSequence chars)
            {
                Insert(index, chars.Value);
                return;
            }
            if (value is StringBuilderCharSequence sb)
            {
                Insert(index, sb.Value);
                return;
            }

            int count = value.Length;

            if (count == 0)
            {
                return;
            }

            Debug.Assert(index >= 0 && index <= value.Length);

            if (_pos > (_chars.Length - count))
            {
                Grow(count);
            }

            int remaining = _pos - index;
            _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
            for (int i = 0; i < count; i++)
            {
                _chars[index++] = value[i];
            }
            _pos += count;
        }

        public void Reverse()
        {
            _chars.Slice(0, _pos).ReverseText();
        }

        public void Delete(int startIndex, int count)
        {
            Debug.Assert(startIndex >= 0 || startIndex <= _pos);
            Debug.Assert(count >= 0);

            int pos = _pos;
            if (startIndex + count > pos)
                count = pos - startIndex;
            if (count > 0)
                Remove(startIndex, count);
        }

        public void Replace(int startIndex, int count, string newValue)
        {
            Debug.Assert(newValue != null);

            Replace(startIndex, count, newValue.AsSpan());
        }

        public void Replace(int startIndex, int count, ReadOnlySpan<char> newValue)
        {
            Debug.Assert(startIndex >= 0 || startIndex <= _pos);
            Debug.Assert(count >= 0);

            int end = startIndex + count;
            if (end > _pos)
            {
                end = _pos;
            }
            if (end > startIndex)
            {
                int stringLength = newValue.Length;
                int diff = end - startIndex - stringLength;
                if (diff > 0)
                { // replacing with fewer characters
                    Remove(startIndex, diff);
                }
                else if (diff < 0)
                {
                    // replacing with more characters...need some room
                    InsertBlank(startIndex, -diff);
                }
                // copy the chars based on the new length
                newValue.CopyTo(_chars.Slice(startIndex, stringLength));
            }
            if (startIndex == end)
            {
                Insert(startIndex, newValue);
            }
        }

        public int IndexOf(char value) => _chars.Slice(0, _pos).IndexOf(value);

        public int IndexOf(ReadOnlySpan<char> value, StringComparison comparisonType)
            => AsSpan().IndexOf(value, comparisonType);

        public int IndexOf(ReadOnlySpan<char> value, int startIndex, StringComparison comparisonType)
            => AsSpan(startIndex).IndexOf(value, comparisonType) + startIndex;

#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
        public int IndexOf(string value, StringComparison comparisonType)
            => AsSpan().IndexOf(value.AsSpan(), comparisonType);

        public int IndexOf(string value, int startIndex, StringComparison comparisonType)
            => AsSpan(startIndex).IndexOf(value.AsSpan(), comparisonType) + startIndex;
#endif

        public int LastIndexOf(char value) => _chars.Slice(0, _pos).LastIndexOf(value);

        public int LastIndexOf(ReadOnlySpan<char> value, StringComparison comparisonType)
            => AsSpan().LastIndexOf(value, comparisonType);

        public int LastIndexOf(ReadOnlySpan<char> value, int startIndex, StringComparison comparisonType)
            => AsSpan(0, startIndex + 1).LastIndexOf(value, comparisonType);

#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
        public int LastIndexOf(string value, StringComparison comparisonType)
            => AsSpan().LastIndexOf(value.AsSpan(), comparisonType);

        public int LastIndexOf(string value, int startIndex, StringComparison comparisonType)
            => AsSpan(0, startIndex + 1).LastIndexOf(value.AsSpan(), comparisonType);
#endif
    }
}
