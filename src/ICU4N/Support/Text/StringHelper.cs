using System;
#if FEATURE_SPAN
using System.Buffers;
using System.Linq;
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

        public unsafe static string Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1)
        {
#if FEATURE_STRING_CONCAT_READONLYSPAN
            return string.Concat(str0, str1);
#else
            int length = str0.Length + str1.Length;
            if (length == 0)
            {
                return string.Empty;
            }

            bool usePool = length > CharStackBufferSize;
            char[]? arrayToReturnToPool = usePool ? ArrayPool<char>.Shared.Rent(length) : null;
            try
            {
                Span<char> buffer = usePool ? arrayToReturnToPool : stackalloc char[length];

                str0.CopyTo(buffer.Slice(0, str0.Length));
                str1.CopyTo(buffer.Slice(str0.Length));

                fixed (char* pBuffer = buffer)
                    return new string(pBuffer, startIndex: 0, length);
            }
            finally
            {
                if (arrayToReturnToPool is not null)
                    ArrayPool<char>.Shared.Return(arrayToReturnToPool);
            }
#endif
        }

        public unsafe static string Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2)
        {
#if FEATURE_STRING_CONCAT_READONLYSPAN
            return string.Concat(str0, str1, str2);
#else
            int length = str0.Length + str1.Length + str2.Length;
            if (length == 0)
            {
                return string.Empty;
            }

            bool usePool = length > CharStackBufferSize;
            char[]? arrayToReturnToPool = usePool ? ArrayPool<char>.Shared.Rent(length) : null;
            try
            {
                Span<char> buffer = usePool ? arrayToReturnToPool : stackalloc char[length];

                str0.CopyTo(buffer.Slice(0, str0.Length));
                str1.CopyTo(buffer.Slice(str0.Length, str1.Length));
                str2.CopyTo(buffer.Slice(str0.Length + str1.Length));

                fixed (char* pBuffer = buffer)
                    return new string(pBuffer, startIndex: 0, length);

            }
            finally
            {
                if (arrayToReturnToPool is not null)
                    ArrayPool<char>.Shared.Return(arrayToReturnToPool);
            }
#endif
        }

        public unsafe static string Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2, ReadOnlySpan<char> str3)
        {
#if FEATURE_STRING_CONCAT_READONLYSPAN
            return string.Concat(str0, str1, str2, str3);
#else
            int length = str0.Length + str1.Length + str2.Length + str3.Length;
            if (length == 0)
            {
                return string.Empty;
            }

            bool usePool = length > CharStackBufferSize;
            char[]? arrayToReturnToPool = usePool ? ArrayPool<char>.Shared.Rent(length) : null;
            try
            {
                Span<char> buffer = usePool ? arrayToReturnToPool : stackalloc char[length];

                str0.CopyTo(buffer.Slice(0, str0.Length));
                str1.CopyTo(buffer.Slice(str0.Length, str1.Length));
                str2.CopyTo(buffer.Slice(str0.Length + str1.Length, str2.Length));
                str3.CopyTo(buffer.Slice(str0.Length + str1.Length + str2.Length));

                fixed (char* pBuffer = buffer)
                    return new string(pBuffer, startIndex: 0, length);

            }
            finally
            {
                if (arrayToReturnToPool is not null)
                    ArrayPool<char>.Shared.Return(arrayToReturnToPool);
            }

#endif
        }

        public unsafe static string Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2, ReadOnlySpan<char> str3, ReadOnlySpan<char> str4)
        {
            int length = str0.Length + str1.Length + str2.Length + str3.Length + str4.Length;
            if (length == 0)
            {
                return string.Empty;
            }

            bool usePool = length > CharStackBufferSize;
            char[]? arrayToReturnToPool = usePool ? ArrayPool<char>.Shared.Rent(length) : null;
            try
            {
                Span<char> buffer = usePool ? arrayToReturnToPool : stackalloc char[length];

                str0.CopyTo(buffer.Slice(0, str0.Length));
                str1.CopyTo(buffer.Slice(str0.Length, str1.Length));
                str2.CopyTo(buffer.Slice(str0.Length + str1.Length, str2.Length));
                str3.CopyTo(buffer.Slice(str0.Length + str1.Length + str2.Length, str3.Length));
                str4.CopyTo(buffer.Slice(str0.Length + str1.Length + str2.Length + str3.Length));

                fixed (char* pBuffer = buffer)
                    return new string(pBuffer, startIndex: 0, length);
            }
            finally
            {
                if (arrayToReturnToPool is not null)
                    ArrayPool<char>.Shared.Return(arrayToReturnToPool);
            }


        }

        public unsafe static string Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2, ReadOnlySpan<char> str3, ReadOnlySpan<char> str4, ReadOnlySpan<char> str5)
        {
            int length = str0.Length + str1.Length + str2.Length + str3.Length + str4.Length + str5.Length;
            if (length == 0)
            {
                return string.Empty;
            }

            bool usePool = length > CharStackBufferSize;
            char[]? arrayToReturnToPool = usePool ? ArrayPool<char>.Shared.Rent(length) : null;
            try
            {
                Span<char> buffer = usePool ? arrayToReturnToPool : stackalloc char[length];

                str0.CopyTo(buffer.Slice(0, str0.Length));
                str1.CopyTo(buffer.Slice(str0.Length, str1.Length));
                str2.CopyTo(buffer.Slice(str0.Length + str1.Length, str2.Length));
                str3.CopyTo(buffer.Slice(str0.Length + str1.Length + str2.Length, str3.Length));
                str4.CopyTo(buffer.Slice(str0.Length + str1.Length + str2.Length + str3.Length, str4.Length));
                str5.CopyTo(buffer.Slice(str0.Length + str1.Length + str2.Length + str3.Length + str4.Length));

                fixed (char* pBuffer = buffer)
                    return new string(pBuffer, startIndex: 0, length);
            }
            finally
            {
                if (arrayToReturnToPool is not null)
                    ArrayPool<char>.Shared.Return(arrayToReturnToPool);
            }
        }
    }
#endif
}
