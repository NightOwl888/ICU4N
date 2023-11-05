using System;
#if FEATURE_SPAN
using System.Buffers;
#endif
#nullable enable

namespace ICU4N.Text
{
#if FEATURE_SPAN

    /// <summary>
    /// Shim for string concat operations on <see cref="ReadOnlySpan{T}"/> where they are unsupported.
    /// </summary>
    internal static class StringHelper
    {

        private const int CharStackBufferSize = 64;

#if !FEATURE_STRING_IMPLCIT_TO_READONLYSPAN
        public static ReadOnlySpan<char> Concat(string str0, string str1)
            => string.Concat(str0, str1).AsSpan();

        public static ReadOnlySpan<char> Concat(string str0, string str1, string str2)
            => string.Concat(str0, str1, str2).AsSpan();

        public static ReadOnlySpan<char> Concat(string str0, string str1, string str2, string str3)
            => string.Concat(str0, str1, str2, str3).AsSpan();
#endif


        public unsafe static string Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1)
        {
#if FEATURE_STRING_CONCAT_READONLYSPAN
            return string.Concat(str0, str1);
#else
            int length = str0.Length + str1.Length;
            char[]? arrayToReturnToPool = null;
            Span<char> buffer;
            if (length <= CharStackBufferSize)
            {
#pragma warning disable CS9081 // The result of this expression may be exposed outside of the containing method
                buffer = stackalloc char[length];
#pragma warning restore CS9081 // The result of this expression may be exposed outside of the containing method
            }
            else
            {
                arrayToReturnToPool = ArrayPool<char>.Shared.Rent(length);
                buffer = arrayToReturnToPool;
            }

            str0.CopyTo(buffer.Slice(0, str0.Length));
            str1.CopyTo(buffer.Slice(str0.Length));

            string result;
            fixed (char* pBuffer = buffer)
                result = new string(pBuffer, startIndex: 0, length);

            if (arrayToReturnToPool is not null)
                ArrayPool<char>.Shared.Return(arrayToReturnToPool);

            return result;
#endif
        }

        public unsafe static string Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2)
        {
#if FEATURE_STRING_CONCAT_READONLYSPAN
            return string.Concat(str0, str1, str2);
#else
            int length = str0.Length + str1.Length + str2.Length;
            char[]? arrayToReturnToPool = null;
            Span<char> buffer;
            if (length <= CharStackBufferSize)
            {
#pragma warning disable CS9081 // The result of this expression may be exposed outside of the containing method
                buffer = stackalloc char[length];
#pragma warning restore CS9081 // The result of this expression may be exposed outside of the containing method
            }
            else
            {
                arrayToReturnToPool = ArrayPool<char>.Shared.Rent(length);
                buffer = arrayToReturnToPool;
            }

            str0.CopyTo(buffer.Slice(0, str0.Length));
            str1.CopyTo(buffer.Slice(str0.Length, str1.Length));
            str2.CopyTo(buffer.Slice(str0.Length + str1.Length));

            string result;
            fixed (char* pBuffer = buffer)
                result = new string(pBuffer, startIndex: 0, length);

            if (arrayToReturnToPool is not null)
                ArrayPool<char>.Shared.Return(arrayToReturnToPool);

            return result;
#endif
        }

        public unsafe static string Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2, ReadOnlySpan<char> str3)
        {
#if FEATURE_STRING_CONCAT_READONLYSPAN
            return string.Concat(str0, str1, str2);
#else
            int length = str0.Length + str1.Length + str2.Length + str3.Length;
            char[]? arrayToReturnToPool = null;
            Span<char> buffer;
            if (length <= CharStackBufferSize)
            {
#pragma warning disable CS9081 // The result of this expression may be exposed outside of the containing method
                buffer = stackalloc char[length];
#pragma warning restore CS9081 // The result of this expression may be exposed outside of the containing method
            }
            else
            {
                arrayToReturnToPool = ArrayPool<char>.Shared.Rent(length);
                buffer = arrayToReturnToPool;
            }

            str0.CopyTo(buffer.Slice(0, str0.Length));
            str1.CopyTo(buffer.Slice(str0.Length, str1.Length));
            str2.CopyTo(buffer.Slice(str0.Length + str1.Length, str2.Length));
            str3.CopyTo(buffer.Slice(str0.Length + str1.Length + str2.Length));

            string result;
            fixed (char* pBuffer = buffer)
                result = new string(pBuffer, startIndex: 0, length);

            if (arrayToReturnToPool is not null)
                ArrayPool<char>.Shared.Return(arrayToReturnToPool);

            return result;
#endif
        }
    }
#endif
}
