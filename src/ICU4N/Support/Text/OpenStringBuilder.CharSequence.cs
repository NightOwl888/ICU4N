﻿using J2N;
using J2N.Text;
using System;
using System.Diagnostics;

namespace ICU4N.Text
{
    internal sealed partial class OpenStringBuilder
    {
        public void AppendCodePoint(int codePoint)
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
        }

        public void InsertCodePoint(int index, int codePoint)
        {
            int count = Character.ToChars(codePoint, out char high, out char low);

            if (_pos > _chars.Length - count)
            {
                Grow(count);
            }

            int remaining = _pos - index;
            _chars.AsSpan(index, remaining).CopyTo(_chars.AsSpan(index + count));
            _chars[index] = high;
            if (count == 2)
                _chars[index + 1] = low;
            _pos += count;
        }

        public int CodePointAt(int index) => AsSpan().CodePointAt(index);
        public int CodePointBefore(int index) => AsSpan().CodePointBefore(index);

        public int CodePointCount(int startIndex, int length) => AsSpan(startIndex, length).CodePointCount();

        public int OffsetByCodePoints(int index, int codePointOffset) => AsSpan().OffsetByCodePoints(index, codePointOffset);

        public void Reverse()
        {
            _chars.AsSpan(0, _pos).ReverseText();
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

        public int IndexOf(char value) => _chars.AsSpan(0, _pos).IndexOf(value);

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

        public int LastIndexOf(char value) => _chars.AsSpan(0, _pos).LastIndexOf(value);

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


        #region ICharSequence

        char ICharSequence.this[int index] => _chars[index];

        bool ICharSequence.HasValue => true;

        int ICharSequence.Length => _pos;

        ICharSequence ICharSequence.Subsequence(int startIndex, int length)
        {
            //throw new NotSupportedException("This is a horrible way to slice text - use AsSpan() or AsMemory() instead");
            // From Apache Harmony String class
            if (_chars is null || (startIndex == 0 && length == _chars.Length))
            {
                return new CharArrayCharSequence(_chars);
            }
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), $"{nameof(length)} may not be negative.");
            if (startIndex > _chars.Length - length) // LUCENENET: Checks for int overflow
                throw new ArgumentOutOfRangeException(nameof(length), $"Index and length must refer to a location within the string. For example {nameof(startIndex)} + {nameof(length)} <= {nameof(Length)}.");

            char[] result = new char[length];
            _chars.AsSpan(startIndex, length).CopyTo(result);
            return new CharArrayCharSequence(result);
        }
        string ICharSequence.ToString() => ToString();

        #endregion ICharSequence
    }
}