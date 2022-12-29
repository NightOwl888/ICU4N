using J2N;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Integer = J2N.Numerics.Int32;
using SR = ICU4N.Support.Numerics.BigMath.Messages;

namespace ICU4N.Numerics.BigMath
{
    internal static class ParseNumbers // This class is based on ParseNumbers in .NET
    {
        internal enum ParsingStatus
        {
            OK,
            Format_EmptyInputString,
            Format_ExtraJunkAtEnd,
            Format_NoParseableDigits,
            Overflow_NegativeUnsigned,
            Overflow
        }

        internal static ParsingStatus TryStringToBigInteger(string s, int radix, int sign, ref int currPos, int length, out BigInteger result)
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

            // ICU4N TODO: Fix EatWhiteSpace to match J2N parsing?


            int[] digits;
            int numberLength;
            int stringLength = length;
            //int startChar;
            int endChar = stringLength;

            // Check for a sign
            if (s[i] == '-')
            {
                sign = -1;
                i++;
                //startChar = 1;
                stringLength--;
            }
            else if (s[i] == '+')
            {
                i++;
                stringLength--;
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

            for (int substrStart = i; //startChar;
                substrStart < endChar;
                substrStart = substrEnd, substrEnd = substrStart
                                                        + charsPerInt)
            {
                if (!Integer.TryParse(s, substrStart, substrEnd - substrStart, radix, out int bigRadixDigit))
                {
                    //exception = null;
                    //result = null;
                    return ParsingStatus.Overflow;
                }
                newDigit = Multiplication.MultiplyByInt(digits, digitIndex, bigRadix);
                newDigit += Elementary.InplaceAdd(digits, digitIndex, bigRadixDigit);
                digits[digitIndex++] = newDigit;
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
                case ParsingStatus.Format_NoParseableDigits:
                    throw new FormatException(SR.Format_NoParsibleDigits);
            }
        }
    }
}
