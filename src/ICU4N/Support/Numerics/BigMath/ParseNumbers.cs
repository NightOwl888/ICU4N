using J2N;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Integer = J2N.Numerics.Int32;
using SR = ICU4N.Support.Numerics.BigMath.Messages;

namespace ICU4N.Numerics.BigMath
{
    internal static class ParseNumbers // This class is based on ParseNumbers in .NET
    {
        internal const int LeftAlign = 0x0001;
        internal const int RightAlign = 0x0004;
        internal const int PrefixSpace = 0x0008;
        internal const int PrintSign = 0x0010;
        internal const int PrintBase = 0x0020;
        internal const int PrintAsI1 = 0x0040;
        internal const int PrintAsI2 = 0x0080;
        internal const int PrintAsI4 = 0x0100;
        internal const int TreatAsUnsigned = 0x0200;
        internal const int TreatAsI1 = 0x0400;
        internal const int TreatAsI2 = 0x0800;
        internal const int IsTight = 0x1000;
        internal const int NoSpace = 0x2000;
        internal const int PrintRadixBase = 0x4000;


        internal enum ParsingStatus
        {
            OK,
            Format_EmptyInputString,
            Format_ExtraJunkAtEnd,
            Format_NoParsibleDigits,
            Format_UnparsibleDigit, // For BigInteger, BigDecimal where we are considering digits separately
            Overflow_NegativeUnsigned,
            Overflow
        }

        internal static ParsingStatus TryStringToBigInteger(string s, int radix, int flags, int sign, ref int currPos, int length, out BigInteger result)
        {
            result = default;

            // They're requied to tell me where to start parsing.
            int i = currPos;

            //if (s is null)
            //    throw new ArgumentNullException(nameof(s));

            if ((radix < Character.MinRadix) || (radix > Character.MaxRadix))
            {
                // math.11=Radix out of range
                throw new ArgumentOutOfRangeException(nameof(radix), SR.ArgumentOutOfRange_Radix);
                //exception = new FormatException(Messages.math11);
                //value = null;
                //return false;
            }
            if (s.Length == 0)
            {
                //exception = new FormatException(Messages.math12); // ICU4N TODO: Return an enum so we know which error message to use?
                //result = null;
                return ParsingStatus.Format_EmptyInputString;
            }

            int end = i + length; // Calculate the exclusive end index now, so we don't lose track when we increment i later
            //int stringLength = length;

            // Get rid of the whitespace and then check that we've still got some digits to parse.
            if (((flags & IsTight) == 0) && ((flags & NoSpace) == 0))
            {
                int iBefore = i;
                EatWhiteSpace(s, ref i);
                if (i == end)
                    return ParsingStatus.Format_EmptyInputString;
                    //throw new FormatException(SR.Format_EmptyInputString);
                //stringLength = stringLength - iBefore;
            }

            int[] digits;
            int numberLength;
            //int startChar;
            //int endChar = stringLength;

            // Check for a sign
            if (s[i] == '-')
            {
                sign = -1;
                i++;
                //startChar = 1;
                //stringLength--;
            }
            else if (s[i] == '+')
            {
                i++;
                //stringLength--;
            }
            //else
            //{
            //    sign = 1;
            //    startChar = 0;
            //}

            // Consume the 0x if we're in base-16. This is for compatibility with .NET.
            if ((radix == 16) && (i + 1 < length) && s[i] == '0')
            {
                if (s[i + 1] == 'x' || s[i + 1] == 'X')
                {
                    i += 2;
                }
            }

            /*
            * We use the following algorithm: split a string into portions of n
            * char and convert each portion to an integer according to the
            * radix. Then convert an exp(radix, n) based number to binary using the
            * multiplication method. See D. Knuth, The Art of Computer Programming,
            * vol. 2.
            */
            int stringLength = length - i;
            int charsPerInt = Conversion.digitFitInInt[radix];
            int bigRadixDigitsLength = stringLength / charsPerInt;
            int topChars = stringLength % charsPerInt;

            if (topChars != 0)
            {
                bigRadixDigitsLength++;
            }
            digits = new int[bigRadixDigitsLength];
            // Get the maximal power of radix that fits in int
            int bigRadix = Conversion.bigRadices[radix - 2];
            // Parse an input string and accumulate the BigInteger's magnitude
            int digitIndex = 0; // index of digits array
            int substrEnd = /*startChar*/ i + ((topChars == 0) ? charsPerInt : topChars);
            int newDigit;
            int charsParsed = i;

            for (int substrStart = i; //startChar;
                substrStart < end; //endChar;
                substrStart = substrEnd, substrEnd = substrStart
                                                        + charsPerInt)
            {
                //if (!Integer.TryParse(s, substrStart, substrEnd - substrStart, radix, out int bigRadixDigit)) // ICU4N TODO: When this is put into J2N - call the internal method
                int grabNumbersStart = substrStart;
                if (!TryGrabInts(radix, s, ref grabNumbersStart, substrEnd, isUnsigned: true, out int bigRadixDigit))
                {
                    //exception = null;
                    //result = null;
                    return ParsingStatus.Overflow;
                }

                charsParsed += grabNumbersStart - substrStart;

                // Check if they passed us a string with no parsable digits.
                if (substrStart == grabNumbersStart)
                    return ParsingStatus.Format_UnparsibleDigit;

                newDigit = Multiplication.MultiplyByInt(digits, digitIndex, bigRadix);
                newDigit += Elementary.InplaceAdd(digits, digitIndex, bigRadixDigit);
                digits[digitIndex++] = newDigit;
            }

            if ((flags & IsTight) != 0)
            {
                // If we've got effluvia left at the end of the string, complain.
                if (charsParsed < end)
                    return ParsingStatus.Format_ExtraJunkAtEnd;
            }

            numberLength = digitIndex;


            result = new BigInteger(); // ICU4N TODO: Create constructor to do this work so we can make BigInteger immutable
            result.sign = sign;
            result.numberLength = numberLength;
            result.digits = digits;
            result.CutOffLeadingZeroes();
            //exception = null;
            return ParsingStatus.OK;
        }

        // These are the same implementation as in J2N
        #region TryGrabInts

#if FEATURE_READONLYSPAN

        private static bool TryGrabInts(int radix, ReadOnlySpan<char> s, ref int i, int end, bool isUnsigned, out int result) // KEEP OVERLOADS FOR ICharSequence, char[], ReadOnlySpan<char>, and string IN SYNC
        {
            uint unsignedResult = 0;
            uint maxVal;

            // Allow all non-decimal numbers to set the sign bit.
            if (radix == 10 && !isUnsigned)
            {
                maxVal = (0x7FFFFFFF / 10);

                // Read all of the digits and convert to a number
                while (i < end && IsDigit(s, i, end, radix, out int value, out int charCount))
                {
                    // Check for overflows - this is sufficient & correct.
                    if (unsignedResult > maxVal || (int)unsignedResult < 0)
                    {
                        result = default;
                        return false;
                    }
                    unsignedResult = unsignedResult * (uint)radix + (uint)value;
                    i += charCount;
                }
                if ((int)unsignedResult < 0 && unsignedResult != 0x80000000)
                {
                    result = default;
                    return false;
                }
            }
            else
            {
                Debug.Assert(radix >= Character.MinRadix && radix <= Character.MaxRadix);
                maxVal = 0xffffffff / (uint)radix;

                // Read all of the digits and convert to a number
                while (i < end && IsDigit(s, i, end, radix, out int value, out int charCount))
                {
                    // Check for overflows - this is sufficient & correct.
                    if (unsignedResult > maxVal)
                    {
                        result = default;
                        return false;
                    }

                    uint temp = unsignedResult * (uint)radix + (uint)value;

                    if (temp < unsignedResult) // this means overflow as well
                    {
                        result = default;
                        return false;
                    }

                    unsignedResult = temp;
                    i += charCount;
                }
            }

            result = (int)unsignedResult;
            return true;
        }

#endif

        private static bool TryGrabInts(int radix, string s, ref int i, int end, bool isUnsigned, out int result) // KEEP OVERLOADS FOR ICharSequence, char[], ReadOnlySpan<char>, and string IN SYNC
        {
            uint unsignedResult = 0;
            uint maxVal;

            // Allow all non-decimal numbers to set the sign bit.
            if (radix == 10 && !isUnsigned)
            {
                maxVal = (0x7FFFFFFF / 10);

                // Read all of the digits and convert to a number
                while (i < end && IsDigit(s, i, end, radix, out int value, out int charCount))
                {
                    // Check for overflows - this is sufficient & correct.
                    if (unsignedResult > maxVal || (int)unsignedResult < 0)
                    {
                        result = default;
                        return false;
                    }
                    unsignedResult = unsignedResult * (uint)radix + (uint)value;
                    i += charCount;
                }
                if ((int)unsignedResult < 0 && unsignedResult != 0x80000000)
                {
                    result = default;
                    return false;
                }
            }
            else
            {
                Debug.Assert(radix >= Character.MinRadix && radix <= Character.MaxRadix);
                maxVal = 0xffffffff / (uint)radix;

                // Read all of the digits and convert to a number
                while (i < end && IsDigit(s, i, end, radix, out int value, out int charCount))
                {
                    // Check for overflows - this is sufficient & correct.
                    if (unsignedResult > maxVal)
                    {
                        result = default;
                        return false;
                    }

                    uint temp = unsignedResult * (uint)radix + (uint)value;

                    if (temp < unsignedResult) // this means overflow as well
                    {
                        result = default;
                        return false;
                    }

                    unsignedResult = temp;
                    i += charCount;
                }
            }

            result = (int)unsignedResult;
            return true;
        }

        private static bool TryGrabInts(int radix, char[] s, ref int i, int end, bool isUnsigned, out int result) // KEEP OVERLOADS FOR ICharSequence, char[], ReadOnlySpan<char>, and string IN SYNC
        {
            uint unsignedResult = 0;
            uint maxVal;

            // Allow all non-decimal numbers to set the sign bit.
            if (radix == 10 && !isUnsigned)
            {
                maxVal = (0x7FFFFFFF / 10);

                // Read all of the digits and convert to a number
                while (i < end && IsDigit(s, i, end, radix, out int value, out int charCount))
                {
                    // Check for overflows - this is sufficient & correct.
                    if (unsignedResult > maxVal || (int)unsignedResult < 0)
                    {
                        result = default;
                        return false;
                    }
                    unsignedResult = unsignedResult * (uint)radix + (uint)value;
                    i += charCount;
                }
                if ((int)unsignedResult < 0 && unsignedResult != 0x80000000)
                {
                    result = default;
                    return false;
                }
            }
            else
            {
                Debug.Assert(radix >= Character.MinRadix && radix <= Character.MaxRadix);
                maxVal = 0xffffffff / (uint)radix;

                // Read all of the digits and convert to a number
                while (i < end && IsDigit(s, i, end, radix, out int value, out int charCount))
                {
                    // Check for overflows - this is sufficient & correct.
                    if (unsignedResult > maxVal)
                    {
                        result = default;
                        return false;
                    }

                    uint temp = unsignedResult * (uint)radix + (uint)value;

                    if (temp < unsignedResult) // this means overflow as well
                    {
                        result = default;
                        return false;
                    }

                    unsignedResult = temp;
                    i += charCount;
                }
            }

            result = (int)unsignedResult;
            return true;
        }

        private static bool TryGrabInts(int radix, ICharSequence s, ref int i, int end, bool isUnsigned, out int result) // KEEP OVERLOADS FOR ICharSequence, char[], ReadOnlySpan<char>, and string IN SYNC
        {
            uint unsignedResult = 0;
            uint maxVal;

            // Allow all non-decimal numbers to set the sign bit.
            if (radix == 10 && !isUnsigned)
            {
                maxVal = (0x7FFFFFFF / 10);

                // Read all of the digits and convert to a number
                while (i < end && IsDigit(s, i, end, radix, out int value, out int charCount))
                {
                    // Check for overflows - this is sufficient & correct.
                    if (unsignedResult > maxVal || (int)unsignedResult < 0)
                    {
                        result = default;
                        return false;
                    }
                    unsignedResult = unsignedResult * (uint)radix + (uint)value;
                    i += charCount;
                }
                if ((int)unsignedResult < 0 && unsignedResult != 0x80000000)
                {
                    result = default;
                    return false;
                }
            }
            else
            {
                Debug.Assert(radix >= Character.MinRadix && radix <= Character.MaxRadix);
                maxVal = 0xffffffff / (uint)radix;

                // Read all of the digits and convert to a number
                while (i < end && IsDigit(s, i, end, radix, out int value, out int charCount))
                {
                    // Check for overflows - this is sufficient & correct.
                    if (unsignedResult > maxVal)
                    {
                        result = default;
                        return false;
                    }

                    uint temp = unsignedResult * (uint)radix + (uint)value;

                    if (temp < unsignedResult) // this means overflow as well
                    {
                        result = default;
                        return false;
                    }

                    unsignedResult = temp;
                    i += charCount;
                }
            }

            result = (int)unsignedResult;
            return true;
        }

        #endregion TryGrabInts

        // These are the same implementation as in J2N
        #region EatWhiteSpace

#if FEATURE_READONLYSPAN
        private static void EatWhiteSpace(ReadOnlySpan<char> s, ref int i) // KEEP OVERLOADS FOR ICharSequence, char[], ReadOnlySpan<char>, and string IN SYNC
        {
            int localIndex = i;
            for (; localIndex < s.Length && char.IsWhiteSpace(s[localIndex]); localIndex++) ;
            i = localIndex;
        }
#endif

        private static void EatWhiteSpace(string s, ref int i) // KEEP OVERLOADS FOR ICharSequence, char[], ReadOnlySpan<char>, and string IN SYNC
        {
            int localIndex = i;
            for (; localIndex < s.Length && char.IsWhiteSpace(s[localIndex]); localIndex++) ;
            i = localIndex;
        }

        private static void EatWhiteSpace(char[] s, ref int i) // KEEP OVERLOADS FOR ICharSequence, char[], ReadOnlySpan<char>, and string IN SYNC
        {
            int localIndex = i;
            for (; localIndex < s.Length && char.IsWhiteSpace(s[localIndex]); localIndex++) ;
            i = localIndex;
        }

        private static void EatWhiteSpace(ICharSequence s, ref int i) // KEEP OVERLOADS FOR ICharSequence, char[], ReadOnlySpan<char>, and string IN SYNC
        {
            int localIndex = i;
            for (; localIndex < s.Length && char.IsWhiteSpace(s[localIndex]); localIndex++) ;
            i = localIndex;
        }

        #endregion EatWhiteSpace

        // These are the same implementation as in J2N
        #region IsDigit

#if FEATURE_READONLYSPAN

#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static bool IsDigit(ReadOnlySpan<char> s, int i, int end, int radix, out int result, out int charCount) // KEEP OVERLOADS FOR ICharSequence, char[], ReadOnlySpan<char>, and string IN SYNC
        {
            if (char.IsHighSurrogate(s[i]) && i + 1 < end && char.IsLowSurrogate(s[i + 1]))
            {
                result = Character.Digit(Character.ToCodePoint(s[i++], s[i++]), radix);
                charCount = result == -1 ? 0 : 2;
                return result != -1;
            }
            result = Character.Digit(s[i++], radix);
            charCount = result == -1 ? 0 : 1;
            return result != -1;
        }

#endif

#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif 
        private static bool IsDigit(string s, int i, int end, int radix, out int result, out int charCount) // KEEP OVERLOADS FOR ICharSequence, char[], ReadOnlySpan<char>, and string IN SYNC
        {
            if (char.IsHighSurrogate(s[i]) && i + 1 < end && char.IsLowSurrogate(s[i + 1]))
            {
                result = Character.Digit(Character.ToCodePoint(s[i++], s[i++]), radix);
                charCount = result == -1 ? 0 : 2;
                return result != -1;
            }
            result = Character.Digit(s[i++], radix);
            charCount = result == -1 ? 0 : 1;
            return result != -1;
        }

#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif 
        private static bool IsDigit(char[] s, int i, int end, int radix, out int result, out int charCount) // KEEP OVERLOADS FOR ICharSequence, char[], ReadOnlySpan<char>, and string IN SYNC
        {
            if (char.IsHighSurrogate(s[i]) && i + 1 < end && char.IsLowSurrogate(s[i + 1]))
            {
                result = Character.Digit(Character.ToCodePoint(s[i++], s[i++]), radix);
                charCount = result == -1 ? 0 : 2;
                return result != -1;
            }
            result = Character.Digit(s[i++], radix);
            charCount = result == -1 ? 0 : 1;
            return result != -1;
        }

#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif 
        private static bool IsDigit(ICharSequence s, int i, int end, int radix, out int result, out int charCount) // KEEP OVERLOADS FOR ICharSequence, char[], ReadOnlySpan<char>, and string IN SYNC
        {
            if (char.IsHighSurrogate(s[i]) && i + 1 < end && char.IsLowSurrogate(s[i + 1]))
            {
                result = Character.Digit(Character.ToCodePoint(s[i++], s[i++]), radix);
                charCount = result == -1 ? 0 : 2;
                return result != -1;
            }
            result = Character.Digit(s[i++], radix);
            charCount = result == -1 ? 0 : 1;
            return result != -1;
        }

        #endregion IsDigit


        [DoesNotReturn]
        internal static void ThrowParsingException(ParsingStatus status)
        {
            switch (status)
            {
                case ParsingStatus.Overflow:
                    throw new OverflowException(SR.Overflow_BigInteger);
                case ParsingStatus.Overflow_NegativeUnsigned:
                    throw new OverflowException(SR.Overflow_NegativeUnsigned);
                case ParsingStatus.Format_EmptyInputString:
                    throw new FormatException(SR.Format_EmptyInputString);
                case ParsingStatus.Format_ExtraJunkAtEnd:
                    throw new FormatException(SR.Format_ExtraJunkAtEnd);
                case ParsingStatus.Format_NoParsibleDigits:
                    throw new FormatException(SR.Format_NoParsibleDigits);
                case ParsingStatus.Format_UnparsibleDigit:
                    throw new FormatException(SR.Format_UnparsibleDigit);
            }
        }
    }
}
