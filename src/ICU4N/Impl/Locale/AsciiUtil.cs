using ICU4N.Text;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
#nullable enable

namespace ICU4N.Impl.Locale
{
    public static partial class AsciiUtil
    {
        private const int CharStackBufferSize = 32;
        private const char CaseDifference = (char)0x20;

        public static bool CaseIgnoreMatch(string s1, string s2)
        {
            //if (Utility.SameObjects(s1, s2))
            if (ReferenceEquals(s1, s2))
            {
                return true;
            }
            if (s1 is null)
                return s2 is null;
            else if (s2 is null)
                return false;

            int len = s1.Length;
            if (len != s2.Length)
            {
                return false;
            }
            int i = 0;
            while (i < len)
            {
                char c1 = s1[i];
                char c2 = s2[i];
                if (c1 != c2 && ToLower(c1) != ToLower(c2))
                {
                    break;
                }
                i++;
            }
            return (i == len);
        }

        public static bool CaseIgnoreMatch(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2)
        {
            int len = s1.Length;
            if (len != s2.Length)
            {
                return false;
            }
            int i = 0;
            while (i < len)
            {
                char c1 = s1[i];
                char c2 = s2[i];
                if (c1 != c2 && ToLower(c1) != ToLower(c2))
                {
                    break;
                }
                i++;
            }
            return (i == len);
        }

        public static int CaseIgnoreCompare(string? s1, string? s2)
        {
            //if (Utility.SameObjects(s1, s2))
            if (ReferenceEquals(s1, s2))
            {
                return 0;
            }

            // They can't both be null at this point.
            if (s1 == null)
            {
                return -1;
            }
            if (s2 == null)
            {
                return 1;
            }

            return CaseIgnoreCompare(s1.AsSpan(), s2.AsSpan());
        }

        public static int CaseIgnoreCompare(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2)
        {
            int s1Length = s1.Length, s2Length = s2.Length;
            if (s1Length > 0 && s2Length > 0)
            {
                if (s1[0] - s2[0] != 0)
                {
                    return s1[0] - s2[0];
                }
            }

            bool s1UsePool = s1Length > CharStackBufferSize, s2UsePool = s2Length > CharStackBufferSize;
            char[]? s1PoolArray = s1UsePool ? ArrayPool<char>.Shared.Rent(s1Length) : null;
            char[]? s2PoolArray = s2UsePool ? ArrayPool<char>.Shared.Rent(s2Length) : null;
            try
            {
                Span<char> s1Buffer = s1UsePool ? s1PoolArray : stackalloc char[s1Length];
                Span<char> s2Buffer = s2UsePool ? s2PoolArray : stackalloc char[s2Length];

                ReadOnlySpan<char> s1Lowered = AsciiUtil.ToLower(s1, s1Buffer);
                ReadOnlySpan<char> s2Lowered = AsciiUtil.ToLower(s2, s2Buffer);

                return s1Lowered.CompareTo(s2Lowered, StringComparison.Ordinal);
            }
            finally
            {
                if (s1PoolArray is not null)
                    ArrayPool<char>.Shared.Return(s1PoolArray);
                if (s2PoolArray is not null)
                    ArrayPool<char>.Shared.Return(s2PoolArray);
            }
        }

        public static char ToUpper(char c)
        {
            if (c >= 'a' && c <= 'z')
            {
                c -= CaseDifference;
            }
            return c;
        }

        public static char ToLower(char c)
        {
            if (c >= 'A' && c <= 'Z')
            {
                c += CaseDifference;
            }
            return c;
        }

        public static string ToLower(string s) // ICU4N specific - renamed from ToLowerString()
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            int idx = 0;
            for (; idx < s.Length; idx++)
            {
                char c = s[idx];
                if (c >= 'A' && c <= 'Z')
                {
                    break;
                }
            }
            if (idx == s.Length)
            {
                return s;
            }
            ValueStringBuilder buf = s.Length <= CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[s.Length])
                : new ValueStringBuilder(s.Length);
            try
            {
                buf.Append(s.AsSpan(0, idx)); // ICU4N: Checked 2nd parameter
                for (; idx < s.Length; idx++)
                {
                    buf.Append(ToLower(s[idx]));
                }
                return buf.ToString();
            }
            finally
            {
                buf.Dispose();
            }
        }

        public static ReadOnlySpan<char> ToLower(ReadOnlySpan<char> s, Span<char> buffer) // ICU4N specific - renamed from ToLowerString()
        {
            if (buffer.Length < s.Length)
                throw new ArgumentException("buffer must be at least the length of 's'.");

            int idx = 0;
            for (; idx < s.Length; idx++)
            {
                char c = s[idx];
                if (c >= 'A' && c <= 'Z')
                {
                    break;
                }
            }
            if (idx == s.Length)
            {
                return s;
            }
            s.Slice(0, idx).CopyTo(buffer); // ICU4N: Checked 2nd parameter
            for (; idx < s.Length; idx++)
            {
                buffer[idx] = ToLower(s[idx]); // This is okay because we only support ASCII
            }
            return buffer.Slice(0, s.Length);
        }

        public static string ToUpper(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));
            int idx = 0;
            for (; idx < s.Length; idx++)
            {
                char c = s[idx];
                if (c >= 'a' && c <= 'z')
                {
                    break;
                }
            }
            if (idx == s.Length)
            {
                return s;
            }
            ValueStringBuilder buf = s.Length <= CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[s.Length])
                : new ValueStringBuilder(s.Length);
            try
            {
                buf.Append(s.AsSpan(0, idx)); // ICU4N: Checked 2nd parameter
                for (; idx < s.Length; idx++)
                {
                    buf.Append(ToUpper(s[idx]));
                }
                return buf.ToString();
            }
            finally
            {
                buf.Dispose();
            }
        }

        public static ReadOnlySpan<char> ToUpper(ReadOnlySpan<char> s, Span<char> buffer) // ICU4N specific - renamed from ToUpperString()
        {
            if (buffer.Length < s.Length)
                throw new ArgumentException("buffer must be at least the length of 's'.");

            int idx = 0;
            for (; idx < s.Length; idx++)
            {
                char c = s[idx];
                if (c >= 'a' && c <= 'z')
                {
                    break;
                }
            }
            if (idx == s.Length)
            {
                return s;
            }
            s.Slice(0, idx).CopyTo(buffer); // ICU4N: Checked 2nd parameter
            for (; idx < s.Length; idx++)
            {
                buffer[idx] = ToUpper(s[idx]); // This is okay because we only support ASCII
            }
            return buffer.Slice(0, s.Length);
        }

        public static string ToTitle(string s) // ICU4N specific - renamed from ToTitleString()
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));
            if (s.Length == 0)
            {
                return s;
            }
            int idx = 0;
            char c = s[idx];
            if (!(c >= 'a' && c <= 'z'))
            {
                for (idx = 1; idx < s.Length; idx++)
                {
                    if (c >= 'A' && c <= 'Z')
                    {
                        break;
                    }
                }
            }
            if (idx == s.Length)
            {
                return s;
            }
            ValueStringBuilder buf = s.Length <= CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[s.Length])
                : new ValueStringBuilder(s.Length);
            try
            {
                buf.Append(s.AsSpan(0, idx)); // ICU4N: Checked 2nd parameter
                if (idx == 0)
                {
                    buf.Append(ToUpper(s[idx]));
                    idx++;
                }
                for (; idx < s.Length; idx++)
                {
                    buf.Append(ToLower(s[idx]));
                }
                return buf.ToString();
            }
            finally
            {
                buf.Dispose();
            }
        }

        public static ReadOnlySpan<char> ToTitle(ReadOnlySpan<char> s, Span<char> buffer) // ICU4N specific - renamed from ToTitleString()
        {
            if (buffer.Length < s.Length)
                throw new ArgumentException("buffer must be at least the length of 's'.");

            if (s.Length == 0)
            {
                return s;
            }
            int idx = 0;
            char c = s[idx];
            if (!(c >= 'a' && c <= 'z'))
            {
                for (idx = 1; idx < s.Length; idx++)
                {
                    if (c >= 'A' && c <= 'Z')
                    {
                        break;
                    }
                }
            }
            if (idx == s.Length)
            {
                return s;
            }
            s.Slice(0, idx).CopyTo(buffer); // ICU4N: Checked 2nd parameter
            if (idx == 0)
            {
                buffer[idx] = ToUpper(s[idx]); // This is okay because we only support ASCII
                idx++;
            }
            for (; idx < s.Length; idx++)
            {
                buffer[idx] = ToLower(s[idx]); // This is okay because we only support ASCII
            }
            return buffer.Slice(0, s.Length);
        }

        public static bool IsAlpha(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlpha(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));
            return IsAlpha(s.AsSpan());
        }

        public static bool IsAlpha(ReadOnlySpan<char> s) // ICU4N specific - renamed from ToAlphaString()
        {
            bool b = true;
            for (int i = 0; i < s.Length; i++)
            {
                if (!IsAlpha(s[i]))
                {
                    b = false;
                    break;
                }
            }
            return b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNumeric(char c)
        {
            return (c >= '0' && c <= '9');
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNumeric(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));
            return IsNumeric(s.AsSpan());
        }
        public static bool IsNumeric(ReadOnlySpan<char> s) // ICU4N specific - renamed from IsNumericString()
        {
            bool b = true;
            for (int i = 0; i < s.Length; i++)
            {
                if (!IsNumeric(s[i]))
                {
                    b = false;
                    break;
                }
            }
            return b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsNumeric(c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlphaNumeric(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));
            return IsAlphaNumeric(s.AsSpan());
        }

        public static bool IsAlphaNumeric(ReadOnlySpan<char> s) // ICU4N specific - renamed from IsAlphaNumericString()
        {
            bool b = true;
            for (int i = 0; i < s.Length; i++)
            {
                if (!IsAlphaNumeric(s[i]))
                {
                    b = false;
                    break;
                }
            }
            return b;
        }
    }
}
