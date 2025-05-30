﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#nullable enable

namespace ICU4N.Text
{
    [StructLayout(LayoutKind.Sequential)]
    internal ref partial struct ValueStringBuilder
    {
        private char[]? _arrayToReturnToPool;
        private Span<char> _chars;
        private int _pos;
        private int _maxLength;
        private bool _capacityExceeded;

        public ValueStringBuilder(Span<char> initialBuffer)
        {
            _arrayToReturnToPool = null;
            _chars = initialBuffer;
            _pos = 0;
            _maxLength = 0;
            _capacityExceeded = false;
        }

        public ValueStringBuilder(int initialCapacity)
        {
            _arrayToReturnToPool = ArrayPool<char>.Shared.Rent(initialCapacity);
            _chars = _arrayToReturnToPool;
            _pos = 0;
            _maxLength = 0;
            _capacityExceeded = false;
        }

        public int Length
        {
            get => _pos;
            set
            {
                Debug.Assert(value >= 0);
                Debug.Assert(value <= _chars.Length);
                _pos = value;
                UpdateMaxLength();
            }
        }

        public int Capacity => _chars.Length;

        public bool CapacityExceeded => _capacityExceeded;

        /// <summary>
        /// The maximum length that was reached during the lifetime of this instance.
        /// This is the minimum buffer size required for the operation to succeed when
        /// <see cref="CapacityExceeded"/> is <c>true</c>.
        /// </summary>
        public int MaxLength => _maxLength;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateMaxLength()
        {
            if (_pos > _maxLength)
                _maxLength = _pos;
        }

        public void EnsureCapacity(int capacity)
            => EnsureCapacity(capacity, out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EnsureCapacity(int capacity, out int newCapacity) // Internal for testing
        {
            // This is not expected to be called this with negative capacity
            Debug.Assert(capacity >= 0);

            newCapacity = default;

            // If the caller has a bug and calls this with negative capacity, make sure to call Grow to throw an exception.
            if ((uint)capacity > (uint)_chars.Length)
                Grow(capacity - _pos, out newCapacity);
        }

        /// <summary>
        /// Get a pinnable reference to the builder.
        /// Does not ensure there is a null char after <see cref="Length"/>
        /// </summary>
        public ref char GetPinnableReference()
        {
            return ref MemoryMarshal.GetReference(_chars);
        }

        /// <summary>
        /// Get a pinnable reference to the builder.
        /// </summary>
        /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
        public ref char GetPinnableReference(bool terminate)
        {
            if (terminate)
            {
                EnsureCapacity(Length + 1);
                _chars[Length] = '\0';
            }
            return ref MemoryMarshal.GetReference(_chars);
        }

        public ref char this[int index]
        {
            get
            {
                Debug.Assert(index < _pos);
                return ref _chars[index];
            }
        }

        public override string ToString()
        {
            string s = _chars.Slice(0, _pos).ToString();
            Dispose();
            return s;
        }

        /// <summary>Returns the underlying storage of the builder.</summary>
        public Span<char> RawChars => _chars;

        /// <summary>Returns the pooled array or <c>null</c> if the stack is still in use.</summary>
        public char[]? RawArray => _arrayToReturnToPool;

        /// <summary>
        /// Returns a memory around the contents of the builder.
        /// <para/>
        /// NOTE: This can only be used if this instance is constructed using the <see cref="ValueStringBuilder(int)"/>
        /// or <see cref="ValueStringBuilder()"/> constructors and you ensure that the returned value goes out of scope
        /// prior to calling <see cref="Dispose()"/>.
        /// </summary>
        /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
        public ReadOnlyMemory<char> AsMemory(bool terminate)
        {
            Debug.Assert(_arrayToReturnToPool != null, "ValueStringBuilder must be constructed using ValueStringBuilder(int) or ValueStringBuilder() to use as memory.");
            if (terminate)
            {
                EnsureCapacity(Length + 1);
                _chars[Length] = '\0';
            }
            return _arrayToReturnToPool.AsMemory(0, _pos);
        }

        /// <summary>
        /// Returns a memory around the contents of the builder.
        /// <para/>
        /// NOTE: This can only be used if this instance is constructed using the <see cref="ValueStringBuilder(int)"/>
        /// or <see cref="ValueStringBuilder()"/> constructors and you ensure that the returned value goes out of scope
        /// prior to calling <see cref="Dispose()"/>.
        /// </summary>
        public ReadOnlyMemory<char> AsMemory()
        {
            Debug.Assert(_arrayToReturnToPool != null, "ValueStringBuilder must be constructed using ValueStringBuilder(int) or ValueStringBuilder() to use as memory.");
            return _arrayToReturnToPool.AsMemory(0, _pos);
        }

        /// <summary>
        /// Returns a memory around the contents of the builder.
        /// <para/>
        /// NOTE: This can only be used if this instance is constructed using the <see cref="ValueStringBuilder(int)"/>
        /// or <see cref="ValueStringBuilder()"/> constructors and you ensure that the returned value goes out of scope
        /// prior to calling <see cref="Dispose()"/>.
        /// </summary>
        public ReadOnlyMemory<char> AsMemory(int start)
        {
            Debug.Assert(_arrayToReturnToPool != null, "ValueStringBuilder must be constructed using ValueStringBuilder(int) or ValueStringBuilder() to use as memory.");
            return _arrayToReturnToPool.AsMemory(start, _pos - start);
        }

        /// <summary>
        /// Returns a memory around the contents of the builder.
        /// <para/>
        /// NOTE: This can only be used if this instance is constructed using the <see cref="ValueStringBuilder(int)"/>
        /// or <see cref="ValueStringBuilder()"/> constructors and you ensure that the returned value goes out of scope
        /// prior to calling <see cref="Dispose()"/>.
        /// </summary>
        public ReadOnlyMemory<char> AsMemory(int start, int length)
        {
            Debug.Assert(_arrayToReturnToPool != null, "ValueStringBuilder must be constructed using ValueStringBuilder(int) or ValueStringBuilder() to use as memory.");
            return _arrayToReturnToPool.AsMemory(start, length);
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
            return _chars.Slice(0, _pos);
        }

        public ReadOnlySpan<char> AsSpan() => _chars.Slice(0, _pos);
        public ReadOnlySpan<char> AsSpan(int start) => _chars.Slice(start, _pos - start);
        public ReadOnlySpan<char> AsSpan(int start, int length) => _chars.Slice(start, length);

        public bool TryCopyTo(Span<char> destination, out int charsWritten)
        {
            if (_chars.Slice(0, _pos).TryCopyTo(destination))
            {
                charsWritten = _pos;
                Dispose();
                return true;
            }
            else
            {
                charsWritten = 0;
                Dispose();
                return false;
            }
        }

        public bool FitsInitialBuffer(out int charsLength)
        {
            if (_capacityExceeded)
            {
                charsLength = _maxLength;
                return false;
            }
            else
            {
                charsLength = _pos;
                return true;
            }
        }

        public void Insert(int index, char value)
            => Insert(index, value, count: 1);

        public void Insert(int index, char value, int count)
        {
            if (_pos > _chars.Length - count)
            {
                Grow(count);
            }

            int remaining = _pos - index;
            _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
            _chars.Slice(index, count).Fill(value);
            _pos += count;
            UpdateMaxLength();
        }

        public void Insert(int index, string? s)
        {
            if (s == null)
            {
                return;
            }

            int count = s.Length;

            if (count == 0)
            {
                return;
            }

            if (_pos > (_chars.Length - count))
            {
                Grow(count);
            }

            int remaining = _pos - index;
            _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
            s
#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
                .AsSpan()
#endif
                .CopyTo(_chars.Slice(index));
            _pos += count;
            UpdateMaxLength();
        }

        public void Insert(int index, scoped ReadOnlySpan<char> s)
        {
            int count = s.Length;

            if (count == 0)
            {
                return;
            }

            if (_pos > (_chars.Length - count))
            {
                Grow(count);
            }

            int remaining = _pos - index;
            _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
            s.CopyTo(_chars.Slice(index));
            _pos += count;
            UpdateMaxLength();
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
            _chars.Slice(index, remaining).CopyTo(_chars.Slice(index + count));
            _pos += count;
            UpdateMaxLength();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(char c)
        {
            int pos = _pos;
            if ((uint)pos < (uint)_chars.Length)
            {
                _chars[pos] = c;
                _pos = pos + 1;
                UpdateMaxLength();
            }
            else
            {
                GrowAndAppend(c);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(string? s)
        {
            if (s == null)
            {
                return;
            }

            int pos = _pos;
            if (s.Length == 1 && (uint)pos < (uint)_chars.Length) // very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
            {
                _chars[pos] = s[0];
                _pos = pos + 1;
                UpdateMaxLength();
            }
            else
            {
                AppendSlow(s);
            }
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
                .CopyTo(_chars.Slice(pos));
            _pos += s.Length;
            UpdateMaxLength();
        }

        public void Append(char c, int count)
        {
            if (_pos > _chars.Length - count)
            {
                Grow(count);
            }

            Span<char> dst = _chars.Slice(_pos, count);
            for (int i = 0; i < dst.Length; i++)
            {
                dst[i] = c;
            }
            _pos += count;
            UpdateMaxLength();
        }

        public unsafe void Append(char* value, int length)
        {
            int pos = _pos;
            if (pos > _chars.Length - length)
            {
                Grow(length);
            }

            Span<char> dst = _chars.Slice(_pos, length);
            for (int i = 0; i < dst.Length; i++)
            {
                dst[i] = *value++;
            }
            _pos += length;
            UpdateMaxLength();
        }

        public void Append(scoped ReadOnlySpan<char> value)
        {
            int pos = _pos;
            if (pos > _chars.Length - value.Length)
            {
                Grow(value.Length);
            }

            value.CopyTo(_chars.Slice(_pos));
            _pos += value.Length;
            UpdateMaxLength();
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
            UpdateMaxLength();
            return _chars.Slice(origPos, length);
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
                _chars.Slice(endIndex).CopyTo(_chars.Slice(startIndex));
                _pos -= length;
            }
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int additionalCapacityBeyondPos)
            => Grow(additionalCapacityBeyondPos, out _);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Grow(int additionalCapacityBeyondPos, out int newCapacity)
        {
            Debug.Assert(additionalCapacityBeyondPos > 0);
            Debug.Assert(_pos > _chars.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

            _capacityExceeded = true;

            const uint ArrayMaxLength = 0x7FFFFFC7; // same as Array.MaxLength

            // Increase to at least the required size (_pos + additionalCapacityBeyondPos), but try
            // to double the size if possible, bounding the doubling to not go beyond the max array length.
            newCapacity = (int)Math.Max(
                (uint)(_pos + additionalCapacityBeyondPos),
                Math.Min((uint)_chars.Length * 2, ArrayMaxLength));

            // Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative
            char[] poolArray = ArrayPool<char>.Shared.Rent(newCapacity);

            _chars.Slice(0, _pos).CopyTo(poolArray);

            char[]? toReturn = _arrayToReturnToPool;
            _chars = _arrayToReturnToPool = poolArray;
            if (toReturn != null)
            {
                ArrayPool<char>.Shared.Return(toReturn);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            char[]? toReturn = _arrayToReturnToPool;
            this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
            if (toReturn != null)
            {
                ArrayPool<char>.Shared.Return(toReturn);
            }
        }
    }
}