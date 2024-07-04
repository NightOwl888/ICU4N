using J2N.Text;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
#nullable enable

namespace ICU4N.Text
{
    /// <summary>
    /// StringBuilder that uses a continguous array, so it can be indexed through like a string.
    /// This more closely resembles the StringBuilder in Java than the .NET implementation of StringBuilder.
    /// <para/>
    /// <see cref="OpenStringBuilder"/> must be used when the builder instance is used as a local buffer, however,
    /// if the StringBuilder lifetime can be put on the stack, <see cref="Text.ValueStringBuilder"/> should
    /// be used instead.
    /// </summary>
    internal sealed partial class OpenStringBuilder : ICharSequence, ISpanAppendable
    {
        /// <summary>
        /// The default capacity of a <see cref="StringBuilder"/>.
        /// </summary>
        internal const int DefaultCapacity = 16;

        private char[] _chars;
        private int _pos;

        public OpenStringBuilder() : this(DefaultCapacity) { }

        public OpenStringBuilder(int capacity)
        {
            _chars = new char[capacity];
            _pos = 0;
        }

        public OpenStringBuilder(string value) : this(value.AsSpan()) { }
        public OpenStringBuilder(ReadOnlySpan<char> value)
        {
            _chars = new char[RoundUpToPowerOf2(value.Length)];
            value.CopyTo(_chars.AsSpan(_pos));
            _pos = value.Length;
        }

        public OpenStringBuilder(StringBuilder value)
        {
            Debug.Assert(value != null);

            int length = value!.Length;
            _chars = new char[RoundUpToPowerOf2(length)];
            value.CopyTo(0, _chars.AsSpan(_pos), length);
            _pos = length;
        }

        public int Length
        {
            get => _pos;
            set => _pos = value;
        }

        public int Capacity => _chars.Length;

        public char this[int index]
        {
            get => _chars[index];
            set => _chars[index] = value;
        }

        /// <summary>
        /// Returns a span around the contents of the builder.
        /// </summary>
        /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
        public ReadOnlySpan<char> AsSpan(bool terminate)
        {
            if (terminate)
            {
                EnsureCapacity(Length + 1);
                _chars[Length] = '\0';
            }
            return _chars.AsSpan(0, _pos);
        }

        public ReadOnlySpan<char> AsSpan() => _chars.AsSpan(0, _pos);
        public ReadOnlySpan<char> AsSpan(int start) => _chars.AsSpan(start, _pos - start);
        public ReadOnlySpan<char> AsSpan(int start, int length) => _chars.AsSpan(start, length);


        /// <summary>
        /// Returns a span around the contents of the builder.
        /// </summary>
        /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
        public ReadOnlyMemory<char> AsMemory(bool terminate)
        {
            if (terminate)
            {
                EnsureCapacity(Length + 1);
                _chars[Length] = '\0';
            }
            return _chars.AsMemory(0, _pos);
        }

        public ReadOnlyMemory<char> AsMemory() => _chars.AsMemory(0, _pos);
        public ReadOnlyMemory<char> AsMemory(int start) => _chars.AsMemory(start, _pos - start);
        public ReadOnlyMemory<char> AsMemory(int start, int length) => _chars.AsMemory(start, length);

        public bool TryCopyTo(Span<char> destination, out int charsWritten)
        {
            if (_chars.AsSpan(0, _pos).TryCopyTo(destination))
            {
                charsWritten = _pos;
                return true;
            }
            else
            {
                charsWritten = 0;
                return false;
            }
        }

        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            Debug.Assert(_pos - sourceIndex >= count);
            _chars.AsSpan(sourceIndex, count).CopyTo(destination.AsSpan(destinationIndex));
        }

        public void CopyTo(int sourceIndex, Span<char> destination, int count)
        {
            Debug.Assert(_pos - sourceIndex >= count);
            _chars.AsSpan(sourceIndex, count).CopyTo(destination);
        }

        public OpenStringBuilder Insert(int index, char value)
            => Insert(index, value, count: 1);

        public OpenStringBuilder Insert(int index, char value, int count)
        {
            if (_pos > _chars.Length - count)
            {
                Grow(count);
            }

            int remaining = _pos - index;
            _chars.AsSpan(index, remaining).CopyTo(_chars.AsSpan(index + count));
            _chars.AsSpan(index, count).Fill(value);
            _pos += count;
            return this;
        }

        public OpenStringBuilder Insert(int index, string? s)
        {
            if (s == null)
            {
                return this;
            }

            int count = s.Length;

            if (count == 0)
            {
                return this;
            }

            if (_pos > (_chars.Length - count))
            {
                Grow(count);
            }

            int remaining = _pos - index;
            _chars.AsSpan(index, remaining).CopyTo(_chars.AsSpan(index + count));
            s
#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
                .AsSpan()
#endif
                .CopyTo(_chars.AsSpan(index));
            _pos += count;
            return this;
        }

        public OpenStringBuilder Insert(int index, ReadOnlySpan<char> s)
        {
            int count = s.Length;

            if (count == 0)
            {
                return this;
            }

            if (_pos > (_chars.Length - count))
            {
                Grow(count);
            }

            int remaining = _pos - index;
            _chars.AsSpan(index, remaining).CopyTo(_chars.AsSpan(index + count));
            s.CopyTo(_chars.AsSpan(index));
            _pos += count;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InsertBlank(int index, int numberOfChars)
        {
            int count = numberOfChars;

            if (count == 0)
            {
                return;
            }

            if (_pos > (_chars.Length - count))
            {
                Grow(count);
            }

            int remaining = _pos - index;
            _chars.AsSpan(index, remaining).CopyTo(_chars.AsSpan(index + count));
            _pos += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public OpenStringBuilder Append(char c)
        {
            int pos = _pos;
            if ((uint)pos < (uint)_chars.Length)
            {
                _chars[pos] = c;
                _pos = pos + 1;
            }
            else
            {
                GrowAndAppend(c);
            }
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public OpenStringBuilder Append(string? s)
        {
            if (s == null)
            {
                return this;
            }

            int pos = _pos;
            if (s.Length == 1 && (uint)pos < (uint)_chars.Length) // very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
            {
                _chars[pos] = s[0];
                _pos = pos + 1;
            }
            else
            {
                AppendSlow(s);
            }
            return this;
        }

        private void AppendSlow(string s)
        {
            int pos = _pos;
            if (pos > _chars.Length - s.Length)
            {
                Grow(s.Length);
            }

            s
#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
                .AsSpan()
#endif
                .CopyTo(_chars.AsSpan(pos));
            _pos += s.Length;
        }

        public OpenStringBuilder Append(char c, int count)
        {
            if (_pos > _chars.Length - count)
            {
                Grow(count);
            }

            Span<char> dst = _chars.AsSpan(_pos, count);
            for (int i = 0; i < dst.Length; i++)
            {
                dst[i] = c;
            }
            _pos += count;
            return this;
        }

        public unsafe OpenStringBuilder Append(char* value, int length)
        {
            int pos = _pos;
            if (pos > _chars.Length - length)
            {
                Grow(length);
            }

            Span<char> dst = _chars.AsSpan(_pos, length);
            for (int i = 0; i < dst.Length; i++)
            {
                dst[i] = *value++;
            }
            _pos += length;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<char> AppendSpan(int length)
        {
            int origPos = _pos;
            if (origPos > _chars.Length - length)
            {
                Grow(length);
            }

            _pos = origPos + length;
            return _chars.AsSpan(origPos, length);
        }

        public void Remove(int startIndex, int length)
        {
            if (_pos == length && startIndex == 0)
            {
                _pos = 0;
                return;
            }

            if (length > 0)
            {
                int endIndex = startIndex + length;
                _chars.AsSpan(endIndex).CopyTo(_chars.AsSpan(startIndex));
                _pos -= length;
            }
        }

        /// <summary>Round the specified value up to the next power of 2, if it isn't one already.</summary>
        internal static int RoundUpToPowerOf2(int i)
        {
            // Based on https://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
            --i;
            i |= i >> 1;
            i |= i >> 2;
            i |= i >> 4;
            i |= i >> 8;
            i |= i >> 16;
            return i + 1;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void GrowAndAppend(char c)
        {
            Grow(1);
            Append(c);
        }

        /// <summary>
        /// Resize the internal buffer either by doubling current buffer size or
        /// by adding <paramref name="additionalCapacityBeyondPos"/> to
        /// <see cref="_pos"/> whichever is greater.
        /// </summary>
        /// <param name="additionalCapacityBeyondPos">
        /// Number of chars requested beyond current position.
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Grow(int additionalCapacityBeyondPos)
        {
            Debug.Assert(additionalCapacityBeyondPos > 0);
            Debug.Assert(_pos > _chars.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

            const uint ArrayMaxLength = 0x7FFFFFC7; // same as Array.MaxLength

            // Increase to at least the required size (_pos + additionalCapacityBeyondPos), but try
            // to double the size if possible, bounding the doubling to not go beyond the max array length.
            int newCapacity = (int)Math.Max(
                (uint)(_pos + additionalCapacityBeyondPos),
                Math.Min((uint)_chars.Length * 2, ArrayMaxLength));

            // Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative.
            // This could also go negative if the actual required length wraps around.
            char[] temp = new char[RoundUpToPowerOf2(newCapacity)];

            _chars.AsSpan(0, _pos).CopyTo(temp);
            _chars = temp;
        }

        public void EnsureCapacity(int capacity)
        {
            // This is not expected to be called this with negative capacity
            Debug.Assert(capacity >= 0);

            // If the caller has a bug and calls this with negative capacity, make sure to call Grow to throw an exception.
            if ((uint)capacity > (uint)_chars.Length)
                Grow(capacity - _pos);
        }

        public override string ToString()
        {
            return _chars.AsSpan(0, _pos).ToString();
        }

    }
}
