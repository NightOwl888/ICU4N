using J2N.Globalization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
#nullable enable

namespace ICU4N.Numerics
{
    internal partial class BigDecimal
    {

        // ICU4N TODO: ReadOnlySpan<char> overloads

        public static bool TryParse(char[] value, int startIndex, int length, NumberStyle styles, IFormatProvider? provider, out BigDecimal result)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_NeedNonNegNum);
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_NeedNonNegNum);
            if (startIndex > value.Length - length) // Checks for int overflow
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_IndexLength);

            return BigNumber.TryParseBigDecimalFloatStyle(value, startIndex, length, styles, NumberFormatInfo.GetInstance(provider), out result);
        }

        public static bool TryParse(string value, int startIndex, int length, NumberStyle styles, IFormatProvider? provider, out BigDecimal result)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_NeedNonNegNum);
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_NeedNonNegNum);
            if (startIndex > value.Length - length) // Checks for int overflow
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_IndexLength);

            // ICU4N TODO: Revisit ToCharArray()
            return BigNumber.TryParseBigDecimalFloatStyle(value.ToCharArray(), startIndex, length, styles, NumberFormatInfo.GetInstance(provider), out result);
        }

        public static bool TryParse(char[] value, NumberStyle styles, IFormatProvider? provider, out BigDecimal result)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            // ICU4N TODO: Revisit ToCharArray()
            return BigNumber.TryParseBigDecimalFloatStyle(value, startIndex: 0, length: value.Length, styles, NumberFormatInfo.GetInstance(provider), out result);
        }

        public static bool TryParse(string value, NumberStyle styles, IFormatProvider? provider, out BigDecimal result)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            // ICU4N TODO: Revisit ToCharArray()
            return BigNumber.TryParseBigDecimalFloatStyle(value.ToCharArray(), startIndex: 0, length: value.Length, styles, NumberFormatInfo.GetInstance(provider), out result);
        }

        public static bool TryParse(char[] value, IFormatProvider? provider, out BigDecimal result)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            return BigNumber.TryParseBigDecimalFloatStyle(value, startIndex: 0, length: value.Length, styles: NumberStyle.Float | NumberStyle.AllowThousands, NumberFormatInfo.GetInstance(provider), out result);
        }

        public static bool TryParse(string value, IFormatProvider? provider, out BigDecimal result)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            // ICU4N TODO: Revisit ToCharArray()
            return BigNumber.TryParseBigDecimalFloatStyle(value.ToCharArray(), startIndex: 0, length: value.Length, styles: NumberStyle.Float | NumberStyle.AllowThousands, NumberFormatInfo.GetInstance(provider), out result);
        }

        public static BigDecimal Parse(string value, int startIndex, int length, NumberStyle styles, IFormatProvider? provider)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_NeedNonNegNum);
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_NeedNonNegNum);
            if (startIndex > value.Length - length) // Checks for int overflow
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_IndexLength);

            // ICU4N TODO: Revisit ToCharArray()
            if (!BigNumber.TryParseBigDecimalFloatStyle(value.ToCharArray(), startIndex, length, styles, NumberFormatInfo.GetInstance(provider), out BigDecimal result))
                throw new FormatException(string.Format(SR.Format_InvalidString, value));

            return result;
        }

        public static BigDecimal Parse(char[] value, int startIndex, int length, NumberStyle styles, IFormatProvider? provider)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_NeedNonNegNum);
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_NeedNonNegNum);
            if (startIndex > value.Length - length) // Checks for int overflow
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_IndexLength);

            if (!BigNumber.TryParseBigDecimalFloatStyle(value, startIndex, length, styles, NumberFormatInfo.GetInstance(provider), out BigDecimal result))
                throw new FormatException(string.Format(SR.Format_InvalidString, value));

            return result;
        }

        public static BigDecimal Parse(string value, IFormatProvider? provider)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            // ICU4N TODO: Revisit ToCharArray()
            if (!BigNumber.TryParseBigDecimalFloatStyle(value.ToCharArray(), startIndex: 0, length: value.Length, styles: NumberStyle.Float | NumberStyle.AllowThousands, NumberFormatInfo.GetInstance(provider), out BigDecimal result))
                throw new FormatException(string.Format(SR.Format_InvalidString, value));

            return result;
        }

        public static BigDecimal Parse(char[] value, IFormatProvider? provider)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (!BigNumber.TryParseBigDecimalFloatStyle(value, startIndex: 0, length: value.Length, styles: NumberStyle.Float | NumberStyle.AllowThousands, NumberFormatInfo.GetInstance(provider), out BigDecimal result))
                throw new FormatException(string.Format(SR.Format_InvalidString, value));

            return result;
        }

        public static BigDecimal Parse(string value, NumberStyle styles, IFormatProvider? provider)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            // ICU4N TODO: Revisit ToCharArray()
            if (!BigNumber.TryParseBigDecimalFloatStyle(value.ToCharArray(), startIndex: 0, length: value.Length, styles, NumberFormatInfo.GetInstance(provider), out BigDecimal result))
                throw new FormatException(string.Format(SR.Format_InvalidString, value));

            return result;
        }

        public static BigDecimal Parse(char[] value, NumberStyle styles, IFormatProvider? provider)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (!BigNumber.TryParseBigDecimalFloatStyle(value, startIndex: 0, length: value.Length, styles, NumberFormatInfo.GetInstance(provider), out BigDecimal result))
                throw new FormatException(string.Format(SR.Format_InvalidString, value));

            return result;
        }

        private static partial class BigNumber
        {
            public static bool TryParseBigDecimalFloatStyle(char[] value, int startIndex, int length, NumberStyle styles, NumberFormatInfo info, out BigDecimal result)
            {
                
                result = default;

                // This is from the ICU4J constructor that parses a char array

                // ICU4N TODO: NumberStyle
                // ICU4N TODO: NumberFormatInfo (or UNumberFormatInfo?)

                bool exotic;
                bool hadexp;
                int d;
                int dotoff;
                int last;
                int i = 0;
                char si;
                bool eneg = false;
                int k = 0;
                int elen = 0;
                int j = 0;
                char sj;
                int dvalue = 0;
                int mag = 0;

                // Outputs (to be passed to the constructor)
                sbyte indicator = ispos; // assume positive
                int exponent = 0;
                byte form = 0;


                if (length <= 0)
                    return false; // bad conversion (empty string)

                // [bad offset will raise array bounds exception]

                /* Handle and step past sign */
                if (value[startIndex] == '-')
                {
                    length--;
                    if (length == 0)
                        return false; // nothing after sign
                    indicator = isneg;
                    startIndex++;
                }
                else if (value[startIndex] == '+')
                {
                    length--;
                    if (length == 0)
                        return false; // nothing after sign
                    startIndex++;
                }

                /* We're at the start of the number */
                exotic = false; // have extra digits
                hadexp = false; // had explicit exponent
                d = 0; // count of digits found
                dotoff = -1; // offset where dot was found
                last = -1; // last character of mantissa

                int len = length;
                i = startIndex;

                for (; len > 0; len--, i++)
                {
                    si = value[i];
                    if (IsDigit(si)) // test for Arabic digit
                    {
                        last = i;
                        d++; // still in mantissa
                        continue;
                    }
                    if (si == '.')
                    { // record and ignore
                        if (dotoff >= 0)
                            return false; // two dots
                        dotoff = i - startIndex; // offset into mantissa
                        continue;
                    }
                    if (si != 'E' && si != 'e')
                    {
                        // expect an extra digit
                        if (!UChar.IsDigit(si)) // ICU4N TODO: Why are we using any digit here, rather than just ASCII?
                            return false; // not a number
                        exotic = true; // will need conversion later
                        last = i;
                        d++; // still in mantissa
                        continue;
                    }

                    /* Found 'e' or 'E' -- now process explicit exponent */
                    // 1998.07.11: sign no longer required
                    if ((i - startIndex) > (length - 2))
                        return false; // no room for even one digit

                    eneg = false;
                    if (value[i + 1] == '-')
                    {
                        eneg = true;
                        k = i + 2;
                    }
                    else if (value[i + 1] == '+')
                        k = i + 2;
                    else
                        k = i + 1;

                    // k is offset of first expected digit
                    elen = length - (k - startIndex); // possible number of digits
                    if ((elen == 0) || (elen > 9))
                        return false; // 0 or more than 9 digits

                    int len2 = elen;
                    j = k;
                    for (; len2 > 0; len2--, j++)
                    {
                        sj = value[j];
                        if (sj < '0')
                            return false; // always bad
                        if (sj > '9')
                        { // maybe an exotic digit
                            if (!UChar.IsDigit(sj))
                                return false; // not a number
                            dvalue = UChar.Digit(sj, 10); // check base
                            if (dvalue < 0)
                                return false; // not base 10
                        }
                        else
                            dvalue = sj - '0';
                        exponent = (exponent * 10) + dvalue;
                    }

                    if (eneg)
                        exponent = -exponent; // was negative
                    hadexp = true; // remember we had one
                    break; // we are done
                }

                /* Here when all inspected */
                if (d == 0)
                    return false; // no mantissa digits
                if (dotoff >= 0)
                    exponent = (exponent + dotoff) - d; // adjust exponent if had dot

                // ICU4N TODO: Refactor so there is only 1 pass..?

                /* strip leading zeros/dot (leave final if all 0's) */
                int len3 = last - 1;
                i = startIndex;

                for (; i <= len3; i++)
                {
                    si = value[i];
                    if (si == '0')
                    {
                        startIndex++;
                        dotoff--;
                        d--;
                    }
                    else if (si == '.')
                    {
                        startIndex++; // step past dot
                        dotoff--;
                    }
                    else if (si <= '9')
                        break; /* non-0 */
                    else
                    {/* exotic */
                        if (UChar.Digit(si, 10) != 0)
                            break; // non-0 or bad
                                   // is 0 .. strip like '0'
                        startIndex++;
                        dotoff--;
                        d--;
                    }
                }

                /* Create the mantissa array */
                byte[] mantissa = new byte[d]; // we know the length
                j = startIndex; // input offset
                if (exotic)
                {
                    // slow: check for exotica
                    int len4 = d;
                    i = 0;
                    for (; len4 > 0; len4--, i++)
                    {
                        if (i == dotoff)
                            j++; // at dot
                        sj = value[j];
                        if (sj <= '9')
                            mantissa[i] = (byte)(sj - '0');/* easy */
                        else
                        {
                            dvalue = UChar.Digit(sj, 10);
                            if (dvalue < 0)
                                return false; // not a number after all
                            mantissa[i] = (byte)dvalue;
                        }
                        j++;
                    }
                }/* exotica */
                else
                {
                    int len5 = d;
                    i = 0;
                    for (; len5 > 0; len5--, i++)
                    {
                        if (i == dotoff)
                            j++;
                        mantissa[i] = (byte)(value[j] - '0');
                        j++;
                    }
                }/* simple */

                /* Looks good. Set the sign indicator and form, as needed. */
                // Trailing zeros are preserved
                // The rule here for form is:
                // If no E-notation, then request plain notation
                // Otherwise act as though add(0,DEFAULT) and request scientific notation
                // [form is already PLAIN]
                if (mantissa[0] == 0)
                {
                    indicator = iszero; // force to show zero

                    // negative exponent is significant (e.g., -3 for 0.000) if plain
                    if (exponent > 0)
                        exponent = 0; // positive exponent can be ignored
                    if (hadexp)
                    { // zero becomes single digit from add
                        mantissa = Zero.mant; // ICU4N TODO: Clone?
                        exponent = 0;
                    }
                }
                else
                { // non-zero
                    // [ind was set earlier]
                    // now determine form
                    if (hadexp)
                    {
                        form = (byte)ExponentForm.Scientific;
                        // 1999.06.29 check for overflow
                        mag = (exponent + mantissa.Length) - 1; // true exponent in scientific notation
                        if ((mag < MinExp) || (mag > MaxExp))
                            return false;
                    }
                }
                // say 'BD(c[]): mant[0] mantlen exp ind form:' mant[0] mant.length exp ind form

                result = new BigDecimal(indicator, form, mantissa, exponent);
                return true;
            }

            private static bool IsDigit(int ch) => ((uint)ch - '0') <= 9;
        }
    }
}
