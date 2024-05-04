using ICU4N.Support.Text;
using ICU4N.Text;
using J2N;
using J2N.Text;
using System;
using System.Text;

namespace ICU4N.Impl
{
    /// <summary>
    /// Ported code from ICU punycode.c 
    /// </summary>
    /// <author>ram</author>
    public static partial class Punycode // ICU4N specific - made class static since it only has static members
    {
        private const int CharStackBufferSize = 32;

        /* Punycode parameters for Bootstring */
        private const int BASE = 36;
        private const int TMIN = 1;
        private const int TMAX = 26;
        private const int SKEW = 38;
        private const int DAMP = 700;
        private const int INITIAL_BIAS = 72;
        private const int INITIAL_N = 0x80;

        /* "Basic" Unicode/ASCII code points */
        private const char HYPHEN = (char)0x2d;
        private const char DELIMITER = HYPHEN;

        private const int ZERO = 0x30;
        //private const int NINE           = 0x39;

        private const int SMALL_A = 0x61;
        private const int SMALL_Z = 0x7a;

        private const int CAPITAL_A = 0x41;
        private const int CAPITAL_Z = 0x5a;

        private static int AdaptBias(int delta, int length, bool firstTime)
        {
            if (firstTime)
            {
                delta /= DAMP;
            }
            else
            {
                delta /= 2;
            }
            delta += delta / length;

            int count = 0;
            for (; delta > ((BASE - TMIN) * TMAX) / 2; count += BASE)
            {
                delta /= (BASE - TMIN);
            }

            return count + (((BASE - TMIN + 1) * delta) / (delta + SKEW));
        }

        /// <summary>
        /// basicToDigit[] contains the numeric value of a basic code
        /// point (for use in representing integers) in the range 0 to
        /// BASE-1, or -1 if b is does not represent a value.
        /// </summary>
        internal static readonly int[] basicToDigit = new int[]{
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,

                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                26, 27, 28, 29, 30, 31, 32, 33, 34, 35, -1, -1, -1, -1, -1, -1,

                -1,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14,
                15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, -1, -1, -1, -1, -1,

                -1,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14,
                15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, -1, -1, -1, -1, -1,

                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,

                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,

                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,

                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1
            };

        //CLOVER:OFF
        private static char AsciiCaseMap(char b, bool uppercase)
        {
            if (uppercase)
            {
                if (SMALL_A <= b && b <= SMALL_Z)
                {
                    b = (char)(b - (SMALL_A - CAPITAL_A));
                }
            }
            else
            {
                if (CAPITAL_A <= b && b <= CAPITAL_Z)
                {
                    b = (char)(b + (SMALL_A - CAPITAL_A));
                }
            }
            return b;
        }
        //CLOVER:ON
        /// <summary>
        /// <see cref="DigitToBasic(int, bool)"/> returns the basic code point whose value
        /// (when used for representing integers) is d, which must be in the
        /// range 0 to BASE-1. The lowercase form is used unless the uppercase flag is
        /// nonzero, in which case the uppercase form is used.
        /// </summary>
        private static char DigitToBasic(int digit, bool uppercase)
        {
            /*  0..25 map to ASCII a..z or A..Z */
            /* 26..35 map to ASCII 0..9         */
            if (digit < 26)
            {
                if (uppercase)
                {
                    return (char)(CAPITAL_A + digit);
                }
                else
                {
                    return (char)(SMALL_A + digit);
                }
            }
            else
            {
                return (char)((ZERO - 26) + digit);
            }
        }

        // ICU4N specific - Encode(ICharSequence src, bool[] caseFlags) moved to Punycode.generated.tt

        // ICU4N TODO: Since we are managing memory reuse internally, the below public methods should be changed
        // to return string instead of StringBuilder. StringBuilder is not a form that is very useful because
        // 1. It cannot be converted to ReadOnlySpan<char> without allocating again
        // 2. Indexing the StringBuilder to read the chars is very slow in .NET
        //
        // We should also add a Try... version of each method so there is an end user path that allows use of the stack.
        // Although, guessing the amount of memory to allocate up front isn't nearly as simple as using ValueStringBuilder.

        /// <summary>
        /// Converts Unicode to Punycode.
        /// The input string must not contain single, unpaired surrogates.
        /// The output will be represented as an array of ASCII code points.
        /// </summary>
        /// <param name="src">The source of the string Buffer passed.</param>
        /// <param name="caseFlags">The boolean array of case flags.</param>
        /// <returns>An array of ASCII code points.</returns>
        public static StringBuilder Encode(ReadOnlySpan<char> src, bool[] caseFlags)
        {
            ValueStringBuilder sb = src.Length <= CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                : new ValueStringBuilder(src.Length);
            try
            {
                Encode(src, ref sb, caseFlags);
                return new StringBuilder(sb.Length).Append(sb.AsSpan());
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Converts Unicode to Punycode.
        /// The input string must not contain single, unpaired surrogates.
        /// The output will be represented as an array of ASCII code points.
        /// </summary>
        /// <param name="src">The source of the string Buffer passed.</param>
        /// <param name="dest">The destination. Upon return, will contain a set of ASCII code points.</param>
        /// <param name="caseFlags">The boolean array of case flags.</param>
        /// <returns>An array of ASCII code points.</returns>
        internal static void Encode(ReadOnlySpan<char> src, ref ValueStringBuilder dest, bool[] caseFlags)
        {
            int n, delta, handledCPCount, basicLength, bias, j, m, q, k, t, srcCPCount;
            char c, c2;
            int srcLength = src.Length;
            int[] cpBuffer = new int[srcLength];
            //StringBuilder dest = new StringBuilder(srcLength);
            /*
             * Handle the basic code points and
             * convert extended ones to UTF-32 in cpBuffer (caseFlag in sign bit):
             */
            srcCPCount = 0;

            for (j = 0; j < srcLength; ++j)
            {
                c = src[j];
                if (IsBasic(c))
                {
                    cpBuffer[srcCPCount++] = 0;
                    dest.Append(caseFlags != null ? AsciiCaseMap(c, caseFlags[j]) : c);
                }
                else
                {
                    n = ((caseFlags != null && caseFlags[j]) ? 1 : 0) << 31; // ICU4N TODO: Check this conversion (changed from 31L to 31 (int))
                    if (!UTF16.IsSurrogate(c))
                    {
                        n |= c;
                    }
                    else if (UTF16.IsLeadSurrogate(c) && (j + 1) < srcLength && UTF16.IsTrailSurrogate(c2 = src[j + 1]))
                    {
                        ++j;

                        n |= UChar.ConvertToUtf32(c, c2);
                    }
                    else
                    {
                        /* error: unmatched surrogate */
                        throw new StringPrepParseException("Illegal char found", StringPrepErrorType.IllegalCharFound);
                    }
                    cpBuffer[srcCPCount++] = n;
                }
            }

            /* Finish the basic string - if it is not empty - with a delimiter. */
            basicLength = dest.Length;
            if (basicLength > 0)
            {
                dest.Append(DELIMITER);
            }

            /*
             * handledCPCount is the number of code points that have been handled
             * basicLength is the number of basic code points
             * destLength is the number of chars that have been output
             */

            /* Initialize the state: */
            n = INITIAL_N;
            delta = 0;
            bias = INITIAL_BIAS;

            /* Main encoding loop: */
            for (handledCPCount = basicLength; handledCPCount < srcCPCount; /* no op */)
            {
                /*
                 * All non-basic code points < n have been handled already.
                 * Find the next larger one:
                 */
                for (m = 0x7fffffff, j = 0; j < srcCPCount; ++j)
                {
                    q = cpBuffer[j] & 0x7fffffff; /* remove case flag from the sign bit */
                    if (n <= q && q < m)
                    {
                        m = q;
                    }
                }

                /*
                 * Increase delta enough to advance the decoder's
                 * <n,i> state to <m,0>, but guard against overflow:
                 */
                if (m - n > (0x7fffffff - delta) / (handledCPCount + 1))
                {
                    throw new InvalidOperationException("Internal program error");
                }
                delta += (m - n) * (handledCPCount + 1);
                n = m;

                /* Encode a sequence of same code points n */
                for (j = 0; j < srcCPCount; ++j)
                {
                    q = cpBuffer[j] & 0x7fffffff; /* remove case flag from the sign bit */
                    if (q < n)
                    {
                        ++delta;
                    }
                    else if (q == n)
                    {
                        /* Represent delta as a generalized variable-length integer: */
                        for (q = delta, k = BASE; /* no condition */; k += BASE)
                        {

                            /* RAM: comment out the old code for conformance with draft-ietf-idn-punycode-03.txt   

                            t=k-bias;
                            if(t<TMIN) {
                                t=TMIN;
                            } else if(t>TMAX) {
                                t=TMAX;
                            }
                            */

                            t = k - bias;
                            if (t < TMIN)
                            {
                                t = TMIN;
                            }
                            else if (k >= (bias + TMAX))
                            {
                                t = TMAX;
                            }

                            if (q < t)
                            {
                                break;
                            }

                            dest.Append(DigitToBasic(t + (q - t) % (BASE - t), false));
                            q = (q - t) / (BASE - t);
                        }

                        dest.Append(DigitToBasic(q, (cpBuffer[j] < 0)));
                        bias = AdaptBias(delta, handledCPCount + 1, (handledCPCount == basicLength));
                        delta = 0;
                        ++handledCPCount;
                    }
                }

                ++delta;
                ++n;
            }
        }

        private static bool IsBasic(int ch)
        {
            return (ch < INITIAL_N);
        }
        //CLOVER:OFF
        private static bool IsBasicUpperCase(int ch)
        {
            return (CAPITAL_A <= ch && ch >= CAPITAL_Z);
        }
        //CLOVER:ON
        private static bool IsSurrogate(int ch)
        {
            return (((ch) & 0xfffff800) == 0xd800);
        }

        // ICU4N specific - Decode(ICharSequence src, bool[] caseFlags) moved to Punycode.generated.tt

        // ICU4N TODO: Since we are managing memory reuse internally, the below public methods should be changed
        // to return string instead of StringBuilder. StringBuilder is not a form that is very useful because
        // 1. It cannot be converted to ReadOnlySpan<char> without allocating again
        // 2. Indexing the StringBuilder to read the chars is very slow in .NET
        //
        // We should also add a Try... version of each method so there is an end user path that allows use of the stack.
        // Although, guessing the amount of memory to allocate up front isn't nearly as simple as using ValueStringBuilder.

        /// <summary>
        /// Converts Punycode to Unicode.
        /// The Unicode string will be at most as long as the Punycode string.
        /// </summary>
        /// <param name="src">The source of the string buffer being passed.</param>
        /// <param name="caseFlags">The array of bool case flags.</param>
        /// <returns><see cref="StringBuilder"/> string.</returns>
        public static StringBuilder Decode(ReadOnlySpan<char> src, bool[] caseFlags)
        {
            ValueStringBuilder sb = src.Length <= CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                : new ValueStringBuilder(src.Length);
            try
            {
                Decode(src, ref sb, caseFlags);
                return new StringBuilder(sb.Length).Append(sb.AsSpan());
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Converts Punycode to Unicode.
        /// The Unicode string will be at most as long as the Punycode string.
        /// </summary>
        /// <param name="src">The source of the string buffer being passed.</param>
        /// <param name="dest">The destination where the output will be written</param>
        /// <param name="caseFlags">The array of bool case flags.</param>
        internal static void Decode(ReadOnlySpan<char> src, ref ValueStringBuilder dest, bool[] caseFlags)
        {
            int srcLength = src.Length;
            //..StringBuilder dest = new StringBuilder(src.Length);
            int n, i, bias, basicLength, j, input, oldi, w, k, digit, t,
                            destCPCount, firstSupplementaryIndex, cpLength;
            char b;

            /*
             * Handle the basic code points:
             * Let basicLength be the number of input code points
             * before the last delimiter, or 0 if there is none,
             * then copy the first basicLength code points to the output.
             *
             * The following loop iterates backward.
             */
            for (j = srcLength; j > 0;)
            {
                if (src[--j] == DELIMITER)
                {
                    break;
                }
            }
            basicLength = destCPCount = j;

            for (j = 0; j < basicLength; ++j)
            {
                b = src[j];
                if (!IsBasic(b))
                {
                    throw new StringPrepParseException("Illegal char found", StringPrepErrorType.InvalidCharFound);
                }
                dest.Append(b);

                if (caseFlags != null && j < caseFlags.Length)
                {
                    caseFlags[j] = IsBasicUpperCase(b);
                }
            }

            /* Initialize the state: */
            n = INITIAL_N;
            i = 0;
            bias = INITIAL_BIAS;
            firstSupplementaryIndex = 1000000000;

            /*
             * Main decoding loop:
             * Start just after the last delimiter if any
             * basic code points were copied; start at the beginning otherwise.
             */
            for (input = basicLength > 0 ? basicLength + 1 : 0; input < srcLength; /* no op */)
            {
                /*
                 * in is the index of the next character to be consumed, and
                 * destCPCount is the number of code points in the output array.
                 *
                 * Decode a generalized variable-length integer into delta,
                 * which gets added to i.  The overflow checking is easier
                 * if we increase i as we go, then subtract off its starting
                 * value at the end to obtain delta.
                 */
                for (oldi = i, w = 1, k = BASE; /* no condition */; k += BASE)
                {
                    if (input >= srcLength)
                    {
                        throw new StringPrepParseException("Illegal char found", StringPrepErrorType.IllegalCharFound);
                    }

                    digit = basicToDigit[src[input++] & 0xFF];
                    if (digit < 0)
                    {
                        throw new StringPrepParseException("Invalid char found", StringPrepErrorType.InvalidCharFound);
                    }
                    if (digit > (0x7fffffff - i) / w)
                    {
                        /* integer overflow */
                        throw new StringPrepParseException("Illegal char found", StringPrepErrorType.IllegalCharFound);
                    }

                    i += digit * w;
                    t = k - bias;
                    if (t < TMIN)
                    {
                        t = TMIN;
                    }
                    else if (k >= (bias + TMAX))
                    {
                        t = TMAX;
                    }
                    if (digit < t)
                    {
                        break;
                    }

                    if (w > 0x7fffffff / (BASE - t))
                    {
                        /* integer overflow */
                        throw new StringPrepParseException("Illegal char found", StringPrepErrorType.IllegalCharFound);
                    }
                    w *= BASE - t;
                }

                /*
                 * Modification from sample code:
                 * Increments destCPCount here,
                 * where needed instead of in for() loop tail.
                 */
                ++destCPCount;
                bias = AdaptBias(i - oldi, destCPCount, (oldi == 0));

                /*
                 * i was supposed to wrap around from (incremented) destCPCount to 0,
                 * incrementing n each time, so we'll fix that now:
                 */
                if (i / destCPCount > (0x7fffffff - n))
                {
                    /* integer overflow */
                    throw new StringPrepParseException("Illegal char found", StringPrepErrorType.IllegalCharFound);
                }

                n += i / destCPCount;
                i %= destCPCount;
                /* not needed for Punycode: */
                /* if (decode_digit(n) <= BASE) return punycode_invalid_input; */

                if (n > 0x10ffff || IsSurrogate(n))
                {
                    /* Unicode code point overflow */
                    throw new StringPrepParseException("Illegal char found", StringPrepErrorType.IllegalCharFound);
                }

                /* Insert n at position i of the output: */
                cpLength = Character.CharCount(n);
                int codeUnitIndex;

                /*
                 * Handle indexes when supplementary code points are present.
                 *
                 * In almost all cases, there will be only BMP code points before i
                 * and even in the entire string.
                 * This is handled with the same efficiency as with UTF-32.
                 *
                 * Only the rare cases with supplementary code points are handled
                 * more slowly - but not too bad since this is an insertion anyway.
                 */
                if (i <= firstSupplementaryIndex)
                {
                    codeUnitIndex = i;
                    if (cpLength > 1)
                    {
                        firstSupplementaryIndex = codeUnitIndex;
                    }
                    else
                    {
                        ++firstSupplementaryIndex;
                    }
                }
                else
                {
                    codeUnitIndex = dest.OffsetByCodePoints(firstSupplementaryIndex, i - firstSupplementaryIndex);
                }

                /* use the UChar index codeUnitIndex instead of the code point index i */
                if (caseFlags != null && (dest.Length + cpLength) <= caseFlags.Length)
                {
                    if (codeUnitIndex < dest.Length)
                    {
                        Array.Copy(caseFlags, codeUnitIndex,
                                         caseFlags, codeUnitIndex + cpLength,
                                         dest.Length - codeUnitIndex);
                    }
                    /* Case of last character determines uppercase flag: */
                    caseFlags[codeUnitIndex] = IsBasicUpperCase(src[input - 1]);
                    if (cpLength == 2)
                    {
                        caseFlags[codeUnitIndex + 1] = false;
                    }
                }
                if (cpLength == 1)
                {
                    /* BMP, insert one code unit */
                    dest.Insert(codeUnitIndex, (char)n);
                }
                else
                {
                    /* supplementary character, insert two code units */
                    dest.Insert(codeUnitIndex, UTF16.GetLeadSurrogate(n));
                    dest.Insert(codeUnitIndex + 1, UTF16.GetTrailSurrogate(n));
                }
                ++i;
            }
        }
    }
}
