using J2N.Numerics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Text;
using Double = J2N.Numerics.Double;
using Float = J2N.Numerics.Single;

namespace ICU4N.Numerics
{

    // ICU4N TODO: API - Try to refactor this into a struct
    // ICU4N TODO: API - Refactor Add, Subtract, etc into static methods that accept both parameters to match .NET BigInteger.
    // ICU4N TODO: API - Add operator overrides
    // ICU4N TODO: API - Implement IEquatable<BigDecimal>?
    // ICU4N TODO: API - Implement IComparable
#if FEATURE_SERIALIZABLE
    [Serializable]
#endif
    internal class BigDecimal : Number, IComparable<BigDecimal> // ICU4N TODO: API - this was public in ICU4J
    {
        // private static final java.lang.String $0="BigDecimal.nrx";

        /* ----- Constants ----- */
        /* properties constant public */// useful to others
        /**
         * The <code>BigDecimal</code> constant "0".
         *
         * @see #ONE
         * @see #TEN
         * @stable ICU 2.0
         */
        public static readonly BigDecimal Zero = new BigDecimal((long)0); // use long as we
                                                                          // want the int
                                                                          // constructor
                                                                          // .. to be able to use this, for speed

        /**
         * The <code>BigDecimal</code> constant "1".
         *
         * @see #TEN
         * @see #ZERO
         * @stable ICU 2.0
         */
        public static readonly BigDecimal One = new BigDecimal((long)1); // use long as we
                                                                         // want the int
                                                                         // constructor
                                                                         // .. to be able to use this, for speed

        /**
         * The <code>BigDecimal</code> constant "10".
         *
         * @see #ONE
         * @see #ZERO
         * @stable ICU 2.0
         */
        public static readonly BigDecimal Ten = new BigDecimal(10);

        // the rounding modes (copied here for upwards compatibility)
        /**
         * Rounding mode to round to a more positive number.
         *
         * @see MathContext#ROUND_CEILING
         * @stable ICU 2.0
         */
        public const RoundingMode RoundToPositiveInfinity = RoundingMode.ToPositiveInfinity;

        /**
         * Rounding mode to round towards zero.
         *
         * @see MathContext#ROUND_DOWN
         * @stable ICU 2.0
         */
        public const RoundingMode RoundToZero = RoundingMode.ToZero;

        /**
         * Rounding mode to round to a more negative number.
         *
         * @see MathContext#ROUND_FLOOR
         * @stable ICU 2.0
         */
        public const RoundingMode RoundToNegativeInfinity = RoundingMode.ToNegativeInfinity;

        /**
         * Rounding mode to round to nearest neighbor, where an equidistant value is rounded down.
         *
         * @see MathContext#ROUND_HALF_DOWN
         * @stable ICU 2.0
         */
        public const RoundingMode RoundHalfDown = RoundingMode.HalfDown;

        /**
         * Rounding mode to round to nearest neighbor, where an equidistant value is rounded to the nearest even neighbor.
         *
         * @see MathContext#ROUND_HALF_EVEN
         * @stable ICU 2.0
         */
        public const RoundingMode RoundHalfEven = RoundingMode.HalfEven;

        /**
         * Rounding mode to round to nearest neighbor, where an equidistant value is rounded up.
         *
         * @see MathContext#ROUND_HALF_UP
         * @stable ICU 2.0
         */
        public const RoundingMode RoundHalfUp = RoundingMode.HalfUp;

        /**
         * Rounding mode to assert that no rounding is necessary.
         *
         * @see MathContext#ROUND_UNNECESSARY
         * @stable ICU 2.0
         */
        public const RoundingMode RoundUnnecessary = RoundingMode.Unnecessary;

        /**
         * Rounding mode to round away from zero.
         *
         * @see MathContext#ROUND_UP
         * @stable ICU 2.0
         */
        public const RoundingMode RoundAwayFromZero = RoundingMode.AwayFromZero;

        /* properties constant private */// locals
        private const sbyte ispos = 1; // ind: indicates positive (must be 1)
        private const sbyte iszero = 0; // ind: indicates zero (must be 0)
        private const sbyte isneg = -1; // ind: indicates negative (must be -1)
                                        // [later could add NaN, +/- infinity, here]

        private const int MinExp = -999999999; // minimum exponent allowed
        private const int MaxExp = 999999999; // maximum exponent allowed
        private const int MinArg = -999999999; // minimum argument integer
        private const int MaxArg = 999999999; // maximum argument integer

        private static readonly MathContext plainMC = new MathContext(0,
                ExponentForm.Plain); // context for plain unlimited math

        /* properties constant private unused */// present but not referenced
                                                // Serialization version
                                                //private static readonly long serialVersionUID = 8245355804974198832L;

        // private static final java.lang.String
        // copyright=" Copyright (c) IBM Corporation 1996, 2000.  All rights reserved. ";

        /* properties static private */
        // Precalculated constant arrays (used by byteaddsub)
        private static sbyte[] bytecar = new sbyte[(90 + 99) + 1]; // carry/borrow array
        private static byte[] bytedig = DigInit(); // next digit array

        /* ----- Instance properties [all private and immutable] ----- */
        /* properties private */

        /**
         * The indicator. This may take the values:
         * <ul>
         * <li>ispos -- the number is positive <li>iszero -- the number is zero <li>isneg -- the number is negative
         * </ul>
         *
         * @serial
         */
        private sbyte ind; // assumed undefined
                           // Note: some code below assumes IND = Sign [-1, 0, 1], at present.
                           // We only need two bits for this, but use a byte [also permits
                           // smooth future extension].

        /**
         * The formatting style. This may take the values:
         * <ul>
         * <li>MathContext.PLAIN -- no exponent needed <li>MathContext.SCIENTIFIC -- scientific notation required <li>
         * MathContext.ENGINEERING -- engineering notation required
         * </ul>
         * <p>
         * This property is an optimization; it allows us to defer number layout until it is actually needed as a string,
         * hence avoiding unnecessary formatting.
         *
         * @serial
         */
        private byte form = (byte)ExponentForm.Plain; // assumed PLAIN
                                                      // We only need two bits for this, at present, but use a byte
                                                      // [again, to allow for smooth future extension]

        /**
         * The value of the mantissa.
         * <p>
         * Once constructed, this may become shared between several BigDecimal objects, so must not be altered.
         * <p>
         * For efficiency (speed), this is a byte array, with each byte taking a value of 0 -&gt; 9.
         * <p>
         * If the first byte is 0 then the value of the number is zero (and mant.length=1, except when constructed from a
         * plain number, for example, 0.000).
         *
         * @serial
         */
        private byte[] mant; // assumed null

        /**
         * The exponent.
         * <p>
         * For fixed point arithmetic, scale is <code>-exp</code>, and can apply to zero.
         *
         * Note that this property can have a value less than MinExp when the mantissa has more than one digit.
         *
         * @serial
         */
        private int exp;

        // assumed 0

        /* ---------------------------------------------------------------- */
        /* Constructors */
        /* ---------------------------------------------------------------- */

        /**
         * Constructs a <code>BigDecimal</code> object from a <code>java.math.BigDecimal</code>.
         * <p>
         * Constructs a <code>BigDecimal</code> as though the parameter had been represented as a <code>String</code> (using
         * its <code>toString</code> method) and the {@link #BigDecimal(java.lang.String)} constructor had then been used.
         * The parameter must not be <code>null</code>.
         * <p>
         * <i>(Note: this constructor is provided only in the <code>com.ibm.icu.math</code> version of the BigDecimal class.
         * It would not be present in a <code>java.math</code> version.)</i>
         *
         * @param bd The <code>BigDecimal</code> to be translated.
         * @stable ICU 2.0
         */

        public BigDecimal(Deveel.Math.BigDecimal bd)
            : this(bd?.ToString(CultureInfo.InvariantCulture))
        {
            //this(bd.toString());
            //return;
        }

        /**
         * Constructs a <code>BigDecimal</code> object from a <code>BigInteger</code>, with scale 0.
         * <p>
         * Constructs a <code>BigDecimal</code> which is the exact decimal representation of the <code>BigInteger</code>,
         * with a scale of zero. The value of the <code>BigDecimal</code> is identical to the value of the <code>BigInteger
         * </code>. The parameter must not be <code>null</code>.
         * <p>
         * The <code>BigDecimal</code> will contain only decimal digits, prefixed with a leading minus sign (hyphen) if the
         * <code>BigInteger</code> is negative. A leading zero will be present only if the <code>BigInteger</code> is zero.
         *
         * @param bi The <code>BigInteger</code> to be converted.
         * @stable ICU 2.0
         */

        public BigDecimal(BigInteger bi)
            : this(bi.ToString(CultureInfo.InvariantCulture)) // ICU4N TODO: Revisit approach to loading char array
        {
        }

        // exp remains 0

        /**
         * Constructs a <code>BigDecimal</code> object from a <code>BigInteger</code> and a scale.
         * <p>
         * Constructs a <code>BigDecimal</code> which is the exact decimal representation of the <code>BigInteger</code>,
         * scaled by the second parameter, which may not be negative. The value of the <code>BigDecimal</code> is the <code>
         * BigInteger</code> divided by ten to the power of the scale. The <code>BigInteger</code> parameter must not be
         * <code>null</code>.
         * <p>
         * The <code>BigDecimal</code> will contain only decimal digits, (with an embedded decimal point followed by <code>
         * scale</code> decimal digits if the scale is positive), prefixed with a leading minus sign (hyphen) if the <code>
         * BigInteger</code> is negative. A leading zero will be present only if the <code>BigInteger</code> is zero.
         *
         * @param bi The <code>BigInteger</code> to be converted.
         * @param scale The <code>int</code> specifying the scale.
         * @throws NumberFormatException If the scale is negative.
         * @stable ICU 2.0
         */

        public BigDecimal(BigInteger bi, int scale)
            : this(bi.ToString(CultureInfo.InvariantCulture)) // ICU4N TODO: Revisit approach to loading char array
        {
            if (scale < 0)
                throw new ArgumentOutOfRangeException(nameof(scale), "Negative scale:" + " " + scale);
            exp = -scale; // exponent is -scale
        }

        /**
         * Constructs a <code>BigDecimal</code> object from an array of characters.
         * <p>
         * Constructs a <code>BigDecimal</code> as though a <code>String</code> had been constructed from the character
         * array and the {@link #BigDecimal(java.lang.String)} constructor had then been used. The parameter must not be
         * <code>null</code>.
         * <p>
         * Using this constructor is faster than using the <code>BigDecimal(String)</code> constructor if the string is
         * already available in character array form.
         *
         * @param inchars The <code>char[]</code> array containing the number to be converted.
         * @throws NumberFormatException If the parameter is not a valid number.
         * @stable ICU 2.0
         */

        public BigDecimal(char[] inchars)
            : this(inchars, 0, inchars?.Length ?? 0)
        {
        }

        /**
         * Constructs a <code>BigDecimal</code> object from an array of characters.
         * <p>
         * Constructs a <code>BigDecimal</code> as though a <code>String</code> had been constructed from the character
         * array (or a subarray of that array) and the {@link #BigDecimal(java.lang.String)} constructor had then been used.
         * The first parameter must not be <code>null</code>, and the subarray must be wholly contained within it.
         * <p>
         * Using this constructor is faster than using the <code>BigDecimal(String)</code> constructor if the string is
         * already available within a character array.
         *
         * @param inchars The <code>char[]</code> array containing the number to be converted.
         * @param offset The <code>int</code> offset into the array of the start of the number to be converted.
         * @param length The <code>int</code> length of the number.
         * @throws NumberFormatException If the parameter is not a valid number for any reason.
         * @stable ICU 2.0
         */

        public BigDecimal(char[] inchars, int startIndex, int length)
        {
            // ICU4N: Added guard clauses
            if (inchars is null)
                throw new ArgumentNullException(nameof(inchars));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_NeedNonNegNum);
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_NeedNonNegNum);
            if (startIndex > inchars.Length - length) // Checks for int overflow
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_IndexLength);

            bool exotic;
            bool hadexp;
            int d;
            int dotoff;
            int last;
            int i = 0;
            char si = (char)0;
            bool eneg = false;
            int k = 0;
            int elen = 0;
            int j = 0;
            char sj = (char)0;
            int dvalue = 0;
            int mag = 0;
            // This is the primary constructor; all incoming strings end up
            // here; it uses explicit (inline) parsing for speed and to avoid
            // generating intermediate (temporary) objects of any kind.
            // 1998.06.25: exponent form built only if E/e in string
            // 1998.06.25: trailing zeros not removed for zero
            // 1999.03.06: no embedded blanks; allow offset and length
            if (length <= 0) /// ICU4N: Already checked this above
                Bad(inchars); // bad conversion (empty string)
                              // [bad offset will raise array bounds exception]

            /* Handle and step past sign */
            ind = ispos; // assume positive
            if (inchars[startIndex] == '-')
            {
                length--;
                if (length == 0)
                    Bad(inchars); // nothing after sign
                ind = isneg;
                startIndex++;
            }
            else if (inchars[startIndex] == '+')
            {
                length--;
                if (length == 0)
                    Bad(inchars); // nothing after sign
                startIndex++;
            }

            /* We're at the start of the number */
            exotic = false; // have extra digits
            hadexp = false; // had explicit exponent
            d = 0; // count of digits found
            dotoff = -1; // offset where dot was found
            last = -1; // last character of mantissa
            {
                int len = length;
                i = startIndex;
                /*i:*/
                for (; len > 0; len--, i++)
                {
                    si = inchars[i];
                    if (si >= '0') // test for Arabic digit
                        if (si <= '9')
                        {
                            last = i;
                            d++; // still in mantissa
                            continue;// i;
                        }
                    if (si == '.')
                    { // record and ignore
                        if (dotoff >= 0)
                            Bad(inchars); // two dots
                        dotoff = i - startIndex; // offset into mantissa
                        continue; // i;
                    }
                    if (si != 'e')
                        if (si != 'E')
                        { // expect an extra digit
                            if (!UChar.IsDigit(si))
                                Bad(inchars); // not a number
                                              // defer the base 10 check until later to avoid extra method call
                            exotic = true; // will need conversion later
                            last = i;
                            d++; // still in mantissa
                            continue; // i;
                        }
                    /* Found 'e' or 'E' -- now process explicit exponent */
                    // 1998.07.11: sign no longer required
                    if ((i - startIndex) > (length - 2))
                        Bad(inchars); // no room for even one digit
                    eneg = false;
                    if (inchars[i + 1] == '-')
                    {
                        eneg = true;
                        k = i + 2;
                    }
                    else if (inchars[i + 1] == '+')
                        k = i + 2;
                    else
                        k = i + 1;
                    // k is offset of first expected digit
                    elen = length - (k - startIndex); // possible number of digits
                    if ((elen == 0) || (elen > 9))
                        Bad(inchars); // 0 or more than 9 digits
                    {
                        int len2 = elen;
                        j = k;
                        for (; len2 > 0; len2--, j++)
                        {
                            sj = inchars[j];
                            if (sj < '0')
                                Bad(inchars); // always bad
                            if (sj > '9')
                            { // maybe an exotic digit
                                if (!UChar.IsDigit(sj))
                                    Bad(inchars); // not a number
                                dvalue = UChar.Digit(sj, 10); // check base
                                if (dvalue < 0)
                                    Bad(inchars); // not base 10
                            }
                            else
                                dvalue = sj - '0';
                            exp = (exp * 10) + dvalue;
                        }
                    }/* j */
                    if (eneg)
                        exp = -exp; // was negative
                    hadexp = true; // remember we had one
                    break; // i; // we are done
                }
            }/* i */

            /* Here when all inspected */
            if (d == 0)
                Bad(inchars); // no mantissa digits
            if (dotoff >= 0)
                exp = (exp + dotoff) - d; // adjust exponent if had dot

            /* strip leading zeros/dot (leave final if all 0's) */
            {
                int len3 = last - 1;
                i = startIndex;
                /*i:*/
                for (; i <= len3; i++)
                {
                    si = inchars[i];
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
                        break; // i;/* non-0 */
                    else
                    {/* exotic */
                        if (UChar.Digit(si, 10) != 0)
                            break; // i; // non-0 or bad
                                   // is 0 .. strip like '0'
                        startIndex++;
                        dotoff--;
                        d--;
                    }
                }
            }/* i */

            /* Create the mantissa array */
            mant = new byte[d]; // we know the length
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
                    sj = inchars[j];
                    if (sj <= '9')
                        mant[i] = (byte)(sj - '0');/* easy */
                    else
                    {
                        dvalue = UChar.Digit(sj, 10);
                        if (dvalue < 0)
                            Bad(inchars); // not a number after all
                        mant[i] = (byte)dvalue;
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
                    mant[i] = (byte)(inchars[j] - '0');
                    j++;
                }

            }/* simple */

            /* Looks good. Set the sign indicator and form, as needed. */
            // Trailing zeros are preserved
            // The rule here for form is:
            // If no E-notation, then request plain notation
            // Otherwise act as though add(0,DEFAULT) and request scientific notation
            // [form is already PLAIN]
            if (mant[0] == 0)
            {
                ind = iszero; // force to show zero
                              // negative exponent is significant (e.g., -3 for 0.000) if plain
                if (exp > 0)
                    exp = 0; // positive exponent can be ignored
                if (hadexp)
                { // zero becomes single digit from add
                    mant = Zero.mant;
                    exp = 0;
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
                    mag = (exp + mant.Length) - 1; // true exponent in scientific notation
                    if ((mag < MinExp) | (mag > MaxExp))
                        Bad(inchars);
                }
            }
            // say 'BD(c[]): mant[0] mantlen exp ind form:' mant[0] mant.length exp ind form
        }

        /**
         * Constructs a <code>BigDecimal</code> object directly from a <code>double</code>.
         * <p>
         * Constructs a <code>BigDecimal</code> which is the exact decimal representation of the 64-bit signed binary
         * floating point parameter.
         * <p>
         * Note that this constructor it an exact conversion; it does not give the same result as converting <code>num
         * </code> to a <code>String</code> using the <code>Double.toString()</code> method and then using the
         * {@link #BigDecimal(java.lang.String)} constructor. To get that result, use the static {@link #valueOf(double)}
         * method to construct a <code>BigDecimal</code> from a <code>double</code>.
         *
         * @param num The <code>double</code> to be converted.
         * @throws NumberFormatException If the parameter is infinite or not a number.
         * @stable ICU 2.0
         */

        public BigDecimal(double num)
            //: this(Double.ToString(num, CultureInfo.InvariantCulture)) // ICU4N TODO: Determine if we have a method for returning a precise result
            : this(ConvertDoubleToPreciseString(num))
        {
            //// 1999.03.06: use exactly the old algorithm
            //// 2000.01.01: note that this constructor does give an exact result,
            //// so perhaps it should not be deprecated
            //// 2000.06.18: no longer deprecated
            //this((new java.math.BigDecimal(num)).toString());
            //return;
        }

        // Workaround for wrong errors being thrown by Deveel Math
        private static string ConvertDoubleToPreciseString(double value)
        {
            try
            {
                return new Deveel.Math.BigDecimal(value).ToString(CultureInfo.InvariantCulture);
            }
            catch (FormatException fe)
            {
                throw new OverflowException(fe.Message, fe);
            }
        }

        /**
         * Constructs a <code>BigDecimal</code> object directly from a <code>int</code>.
         * <p>
         * Constructs a <code>BigDecimal</code> which is the exact decimal representation of the 32-bit signed binary
         * integer parameter. The <code>BigDecimal</code> will contain only decimal digits, prefixed with a leading minus
         * sign (hyphen) if the parameter is negative. A leading zero will be present only if the parameter is zero.
         *
         * @param num The <code>int</code> to be converted.
         * @stable ICU 2.0
         */

        public BigDecimal(int num)
        {
            int mun;
            int i = 0;
            // We fastpath commoners
            if (num <= 9)
                if (num >= (-9))
                {


                    // very common single digit case
                    if (num == 0)
                    {
                        mant = Zero.mant;
                        ind = iszero;
                    }
                    else if (num == 1)
                    {
                        mant = One.mant;
                        ind = ispos;
                    }
                    else if (num == (-1))
                    {
                        mant = One.mant;
                        ind = isneg;
                    }
                    else
                    {
                        mant = new byte[1];
                        if (num > 0)
                        {
                            mant[0] = (byte)num;
                            ind = ispos;
                        }
                        else
                        { // num<-1
                            mant[0] = (byte)-num;
                            ind = isneg;
                        }
                    }
                    return;
                }/* singledigit */

            /* We work on negative numbers so we handle the most negative number */
            if (num > 0)
            {
                ind = ispos;
                num = -num;
            }
            else
                ind = isneg;/* negative */// [0 case already handled]
                                          // [it is quicker, here, to pre-calculate the length with
                                          // one loop, then allocate exactly the right length of byte array,
                                          // then re-fill it with another loop]
            mun = num; // working copy
            {
                i = 9;
                /*i:*/
                for (; ; i--)
                {
                    mun = mun / 10;
                    if (mun == 0)
                        break; // i;
                }
            }/* i */
            // i is the position of the leftmost digit placed
            mant = new byte[10 - i];
            {
                i = (10 - i) - 1;
                /*i:*/
                for (; ; i--)
                {
                    mant[i] = (byte)-(byte)(num % 10);
                    num = num / 10;
                    if (num == 0)
                        break; // i;
                }
            }/* i */
        }

        /**
         * Constructs a <code>BigDecimal</code> object directly from a <code>long</code>.
         * <p>
         * Constructs a <code>BigDecimal</code> which is the exact decimal representation of the 64-bit signed binary
         * integer parameter. The <code>BigDecimal</code> will contain only decimal digits, prefixed with a leading minus
         * sign (hyphen) if the parameter is negative. A leading zero will be present only if the parameter is zero.
         *
         * @param num The <code>long</code> to be converted.
         * @stable ICU 2.0
         */

        public BigDecimal(long num)
        {
            long mun;
            int i = 0;
            // Not really worth fastpathing commoners in this constructor [also,
            // we use this to construct the static constants].
            // This is much faster than: this(String.valueOf(num).toCharArray())
            /* We work on negative num so we handle the most negative number */
            if (num > 0)
            {
                ind = ispos;
                num = -num;
            }
            else if (num == 0)
                ind = iszero;
            else
                ind = isneg;/* negative */
            mun = num;
            {
                i = 18;
                /*i:*/
                for (; ; i--)
                {
                    mun = mun / 10;
                    if (mun == 0)
                        break;// i;
                }
            }/* i */
            // i is the position of the leftmost digit placed
            mant = new byte[19 - i];
            {
                i = (19 - i) - 1;
                /*i:*/
                for (; ; i--)
                {
                    mant[i] = (byte)-(byte)(num % 10);
                    num = num / 10;
                    if (num == 0)
                        break;// i;
                }
            }/* i */
        }

        /**
         * Constructs a <code>BigDecimal</code> object from a <code>String</code>.
         * <p>
         * Constructs a <code>BigDecimal</code> from the parameter, which must not be <code>null</code> and must represent a
         * valid <i>number</i>, as described formally in the documentation referred to {@link BigDecimal above}.
         * <p>
         * In summary, numbers in <code>String</code> form must have at least one digit, may have a leading sign, may have a
         * decimal point, and exponential notation may be used. They follow conventional syntax, and may not contain blanks.
         * <p>
         * Some valid strings from which a <code>BigDecimal</code> might be constructed are:
         *
         * <pre>
         *
         * "0" -- Zero "12" -- A whole number "-76" -- A signed whole number "12.70" -- Some decimal places "+0.003" -- Plus
         * sign is allowed "17." -- The same as 17 ".5" -- The same as 0.5 "4E+9" -- Exponential notation "0.73e-7" --
         * Exponential notation
         *
         * </pre>
         * <p>
         * (Exponential notation means that the number includes an optional sign and a power of ten following an
         * '<code>E</code>' that indicates how the decimal point will be shifted. Thus the <code>"4E+9"</code> above is
         * just a short way of writing <code>4000000000</code>, and the <code>"0.73e-7"</code> is short for <code>
         * 0.000000073</code>.)
         * <p>
         * The <code>BigDecimal</code> constructed from the String is in a standard form, with no blanks, as though the
         * {@link #add(BigDecimal)} method had been used to add zero to the number with unlimited precision. If the string
         * uses exponential notation (that is, includes an <code>e</code> or an <code>E</code>), then the <code>BigDecimal
         * </code> number will be expressed in scientific notation (where the power of ten is adjusted so there is a single
         * non-zero digit to the left of the decimal point); in this case if the number is zero then it will be expressed as
         * the single digit 0, and if non-zero it will have an exponent unless that exponent would be 0. The exponent must
         * fit in nine digits both before and after it is expressed in scientific notation.
         * <p>
         * Any digits in the parameter must be decimal; that is, <code>Character.digit(c, 10)</code> (where <code>c</code>
         * is the character in question) would not return -1.
         *
         * @param string The <code>String</code> to be converted.
         * @throws NumberFormatException If the parameter is not a valid number.
         * @stable ICU 2.0
         */

        public BigDecimal(string str)
            : this(str?.ToCharArray(), 0, str?.Length ?? 0) // ICU4N TODO: Remove this allocation and copy logic from char[] constructors
        {
            //this(string.toCharArray(), 0, string.length());
            //return;
        }

        /* <sgml> Make a default BigDecimal object for local use. </sgml> */

        private BigDecimal()
        {
            //super();
            //return;
        }

        /* ---------------------------------------------------------------- */
        /* Operator methods [methods which take a context parameter] */
        /* ---------------------------------------------------------------- */

        /**
         * Returns a plain <code>BigDecimal</code> whose value is the absolute value of this <code>BigDecimal</code>.
         * <p>
         * The same as {@link #abs(MathContext)}, where the context is <code>new MathContext(0, MathContext.PLAIN)</code>.
         * <p>
         * The length of the decimal part (the scale) of the result will be <code>this.scale()</code>
         *
         * @return A <code>BigDecimal</code> whose value is the absolute value of this <code>BigDecimal</code>.
         * @stable ICU 2.0
         */

        public BigDecimal Abs()
        {
            return this.Abs(plainMC);
        }

        /**
         * Returns a <code>BigDecimal</code> whose value is the absolute value of this <code>BigDecimal</code>.
         * <p>
         * If the current object is zero or positive, then the same result as invoking the {@link #plus(MathContext)} method
         * with the same parameter is returned. Otherwise, the same result as invoking the {@link #negate(MathContext)}
         * method with the same parameter is returned.
         *
         * @param set The <code>MathContext</code> arithmetic settings.
         * @return A <code>BigDecimal</code> whose value is the absolute value of this <code>BigDecimal</code>.
         * @stable ICU 2.0
         */

        public BigDecimal Abs(MathContext set)
        {
            if (this.ind == isneg)
                return this.Negate(set);
            return this.Plus(set);
        }

        /**
         * Returns a plain <code>BigDecimal</code> whose value is <code>this+rhs</code>, using fixed point arithmetic.
         * <p>
         * The same as {@link #add(BigDecimal, MathContext)}, where the <code>BigDecimal</code> is <code>rhs</code>, and the
         * context is <code>new MathContext(0, MathContext.PLAIN)</code>.
         * <p>
         * The length of the decimal part (the scale) of the result will be the maximum of the scales of the two operands.
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the addition.
         * @return A <code>BigDecimal</code> whose value is <code>this+rhs</code>, using fixed point arithmetic.
         * @stable ICU 2.0
         */

        public BigDecimal Add(BigDecimal rhs)
        {
            return this.Add(rhs, plainMC);
        }

        /**
         * Returns a <code>BigDecimal</code> whose value is <code>this+rhs</code>.
         * <p>
         * Implements the addition (<b><code>+</code></b>) operator (as defined in the decimal documentation, see
         * {@link BigDecimal class header}), and returns the result as a <code>BigDecimal</code> object.
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the addition.
         * @param set The <code>MathContext</code> arithmetic settings.
         * @return A <code>BigDecimal</code> whose value is <code>this+rhs</code>.
         * @stable ICU 2.0
         */

        public BigDecimal Add(BigDecimal rhs, MathContext set)
        {
            // ICU4N: added guard clauses
            if (rhs is null)
                throw new ArgumentNullException(nameof(rhs));
            if (set is null)
                throw new ArgumentNullException(nameof(set));

            BigDecimal lhs;
            int reqdig;
            BigDecimal res;
            byte[] usel;
            int usellen;
            byte[] user;
            int userlen;
            int newlen = 0;
            int tlen = 0;
            int mult = 0;
            byte[] t = null;
            int ia = 0;
            int ib = 0;
            int ea = 0;
            int eb = 0;
            byte ca = 0;
            byte cb = 0;
            /* determine requested digits and form */
            if (set.LostDigits)
                CheckDigits(rhs, set.Digits);
            lhs = this; // name for clarity and proxy

            /* Quick exit for add floating 0 */
            // plus() will optimize to return same object if possible
            if (lhs.ind == 0)
                if (set.Form != ExponentForm.Plain)
                    return rhs.Plus(set);
            if (rhs.ind == 0)
                if (set.Form != ExponentForm.Plain)
                    return lhs.Plus(set);

            /* Prepare numbers (round, unless unlimited precision) */
            reqdig = set.Digits; // local copy (heavily used)
            if (reqdig > 0)
            {
                if (lhs.mant.Length > reqdig)
                    lhs = Clone(lhs).Round(set);
                if (rhs.mant.Length > reqdig)
                    rhs = Clone(rhs).Round(set);
                // [we could reuse the new LHS for result in this case]
            }

            res = new BigDecimal(); // build result here

            /*
             * Now see how much we have to pad or truncate lhs or rhs in order to align the numbers. If one number is much
             * larger than the other, then the smaller cannot affect the answer [but we may still need to pad with up to
             * DIGITS trailing zeros].
             */
            // Note sign may be 0 if digits (reqdig) is 0
            // usel and user will be the byte arrays passed to the adder; we'll
            // use them on all paths except quick exits
            usel = lhs.mant;
            usellen = lhs.mant.Length;
            user = rhs.mant;
            userlen = rhs.mant.Length;
            {

                if (lhs.exp == rhs.exp)
                {/* no padding needed */
                    // This is the most common, and fastest, path
                    res.exp = lhs.exp;
                }
                else if (lhs.exp > rhs.exp)
                { // need to pad lhs and/or truncate rhs
                    newlen = (usellen + lhs.exp) - rhs.exp;
                    /*
                     * If, after pad, lhs would be longer than rhs by digits+1 or more (and digits>0) then rhs cannot
                     * affect answer, so we only need to pad up to a length of DIGITS+1.
                     */
                    if (newlen >= ((userlen + reqdig) + 1))
                        if (reqdig > 0)
                        {
                            // LHS is sufficient
                            res.mant = usel;
                            res.exp = lhs.exp;
                            res.ind = lhs.ind;
                            if (usellen < reqdig)
                            { // need 0 padding
                                res.mant = Extend(lhs.mant, reqdig);
                                res.exp = res.exp - ((reqdig - usellen));
                            }
                            return res.Finish(set, false);
                        }
                    // RHS may affect result
                    res.exp = rhs.exp; // expected final exponent
                    if (newlen > (reqdig + 1))
                        if (reqdig > 0)
                        {
                            // LHS will be max; RHS truncated
                            tlen = (newlen - reqdig) - 1; // truncation length
                            userlen = userlen - tlen;
                            res.exp = res.exp + tlen;
                            newlen = reqdig + 1;
                        }
                    if (newlen > usellen)
                        usellen = newlen; // need to pad LHS
                }
                else
                { // need to pad rhs and/or truncate lhs
                    newlen = (userlen + rhs.exp) - lhs.exp;
                    if (newlen >= ((usellen + reqdig) + 1))
                        if (reqdig > 0)
                        {
                            // RHS is sufficient
                            res.mant = user;
                            res.exp = rhs.exp;
                            res.ind = rhs.ind;
                            if (userlen < reqdig)
                            { // need 0 padding
                                res.mant = Extend(rhs.mant, reqdig);
                                res.exp = res.exp - ((reqdig - userlen));
                            }
                            return res.Finish(set, false);
                        }
                    // LHS may affect result
                    res.exp = lhs.exp; // expected final exponent
                    if (newlen > (reqdig + 1))
                        if (reqdig > 0)
                        {
                            // RHS will be max; LHS truncated
                            tlen = (newlen - reqdig) - 1; // truncation length
                            usellen = usellen - tlen;
                            res.exp = res.exp + tlen;
                            newlen = reqdig + 1;
                        }
                    if (newlen > userlen)
                        userlen = newlen; // need to pad RHS
                }
            }/* padder */

            /* OK, we have aligned mantissas. Now add or subtract. */
            // 1998.06.27 Sign may now be 0 [e.g., 0.000] .. treat as positive
            // 1999.05.27 Allow for 00 on lhs [is not larger than 2 on rhs]
            // 1999.07.10 Allow for 00 on rhs [is not larger than 2 on rhs]
            if (lhs.ind == iszero)
                res.ind = ispos;
            else
                res.ind = lhs.ind; // likely sign, all paths
            if (((lhs.ind == isneg) ? 1 : 0) == ((rhs.ind == isneg) ? 1 : 0)) // same sign, 0 non-negative
                mult = 1;
            else
            {
                do
                {
                    // different signs, so subtraction is needed
                    mult = -1; // will cause subtract
                    /*
                     * Before we can subtract we must determine which is the larger, as our add/subtract routine only
                     * handles non-negative results so we may need to swap the operands.
                     */
                    {
                        do
                        {/* select */
                            if (rhs.ind == iszero)
                            {
                                // original A bigger
                            }
                            else if ((usellen < userlen) | (lhs.ind == iszero))
                            { // original B bigger
                                t = usel;
                                usel = user;
                                user = t; // swap
                                tlen = usellen;
                                usellen = userlen;
                                userlen = tlen; // ..
                                res.ind = (sbyte)-res.ind; // and set sign
                            }
                            else if (usellen > userlen)
                            {
                                // original A bigger
                            }
                            else
                            {
                                {/* logical lengths the same */// need compare
                                    /* may still need to swap: compare the strings */
                                    ia = 0;
                                    ib = 0;
                                    ea = usel.Length - 1;
                                    eb = user.Length - 1;
                                    {
                                        /*compare:*/
                                        for (; ; )
                                        {
                                            if (ia <= ea)
                                                ca = usel[ia];
                                            else
                                            {
                                                if (ib > eb)
                                                {/* identical */
                                                    if (set.Form != ExponentForm.Plain)
                                                        return Zero;
                                                    // [if PLAIN we must do the subtract, in case of 0.000 results]
                                                    break; // compare;
                                                }
                                                ca = (byte)0;
                                            }
                                            if (ib <= eb)
                                                cb = user[ib];
                                            else
                                                cb = (byte)0;
                                            if (ca != cb)
                                            {
                                                if (ca < cb)
                                                {/* swap needed */
                                                    t = usel;
                                                    usel = user;
                                                    user = t; // swap
                                                    tlen = usellen;
                                                    usellen = userlen;
                                                    userlen = tlen; // ..
                                                    res.ind = (sbyte)-res.ind;
                                                }
                                                break; // compare;
                                            }
                                            /* mantissas the same, so far */
                                            ia++;
                                            ib++;
                                        }
                                    }/* compare */
                                } // lengths the same
                            }
                        } while (false);
                    }/* swaptest */
                } while (false);
            }/* signdiff */

            /* here, A is > B if subtracting */
            // add [A+B*1] or subtract [A+(B*-1)]
            res.mant = ByteAddSub(usel, usellen, user, userlen, mult, false);
            // [reuse possible only after chop; accounting makes not worthwhile]

            // Finish() rounds before stripping leading 0's, then sets form, etc.
            return res.Finish(set, false);
        }

        /**
         * Compares this <code>BigDecimal</code> to another, using unlimited precision.
         * <p>
         * The same as {@link #compareTo(BigDecimal, MathContext)}, where the <code>BigDecimal</code> is <code>rhs</code>,
         * and the context is <code>new MathContext(0, MathContext.PLAIN)</code>.
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the comparison.
         * @return An <code>int</code> whose value is -1, 0, or 1 as <code>this</code> is numerically less than, equal to,
         *         or greater than <code>rhs</code>.
         * @stable ICU 2.0
         */

        public int CompareTo(BigDecimal rhs)
        {
            return this.CompareTo(rhs, plainMC);
        }

        /**
         * Compares this <code>BigDecimal</code> to another.
         * <p>
         * Implements numeric comparison, (as defined in the decimal documentation, see {@link BigDecimal class header}),
         * and returns a result of type <code>int</code>.
         * <p>
         * The result will be:
         * <table cellpadding=2>
         * <tr>
         * <td align=right><b>-1</b></td> <td>if the current object is less than the first parameter</td>
         * </tr>
         * <tr>
         * <td align=right><b>0</b></td> <td>if the current object is equal to the first parameter</td>
         * </tr>
         * <tr>
         * <td align=right><b>1</b></td> <td>if the current object is greater than the first parameter.</td>
         * </tr>
         * </table>
         * <p>
         * A {@link #compareTo(BigDecimal)} method is also provided.
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the comparison.
         * @param set The <code>MathContext</code> arithmetic settings.
         * @return An <code>int</code> whose value is -1, 0, or 1 as <code>this</code> is numerically less than, equal to,
         *         or greater than <code>rhs</code>.
         * @stable ICU 2.0
         */

        public int CompareTo(BigDecimal rhs, MathContext set)
        {
            // ICU4N: added guard clauses
            if (set is null)
                throw new ArgumentNullException(nameof(set));

            if (rhs is null) return 1; // ICU4N: Using 1 if other is null as specified here: https://stackoverflow.com/a/4852537

            int thislength = 0;
            int i = 0;
            BigDecimal newrhs;
            // rhs=null will raise NullPointerException, as per Comparable interface
            if (set.LostDigits)
                CheckDigits(rhs, set.Digits);
            // [add will recheck in slowpath cases .. but would report -rhs]
            if ((this.ind == rhs.ind) && (this.exp == rhs.exp))
            {
                /* sign & exponent the same [very common] */
                thislength = this.mant.Length;
                if (thislength < rhs.mant.Length)
                    return (sbyte)-this.ind;
                if (thislength > rhs.mant.Length)
                    return this.ind;
                /*
                 * lengths are the same; we can do a straight mantissa compare unless maybe rounding [rounding is very
                 * unusual]
                 */
                if ((thislength <= set.Digits) || (set.Digits == 0))
                {
                    {
                        int len6 = thislength;
                        i = 0;
                        for (; len6 > 0; len6--, i++)
                        {
                            if ((sbyte)this.mant[i] < (sbyte)rhs.mant[i])
                                return (sbyte)-this.ind;
                            if ((sbyte)this.mant[i] > (sbyte)rhs.mant[i])
                                return this.ind;
                        }
                    }/* i */
                    return 0; // identical
                }
                /* drop through for full comparison */
            }
            else
            {
                /* More fastpaths possible */
                if (this.ind < rhs.ind)
                    return -1;
                if (this.ind > rhs.ind)
                    return 1;
            }
            /* carry out a subtract to make the comparison */
            newrhs = Clone(rhs); // safe copy
            newrhs.ind = (sbyte)-newrhs.ind; // prepare to subtract
            return this.Add(newrhs, set).ind; // add, and return sign of result
        }

        /**
         * Returns a plain <code>BigDecimal</code> whose value is <code>this/rhs</code>, using fixed point arithmetic.
         * <p>
         * The same as {@link #divide(BigDecimal, int)}, where the <code>BigDecimal</code> is <code>rhs</code>, and the
         * rounding mode is {@link MathContext#ROUND_HALF_UP}.
         *
         * The length of the decimal part (the scale) of the result will be the same as the scale of the current object, if
         * the latter were formatted without exponential notation.
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the division.
         * @return A plain <code>BigDecimal</code> whose value is <code>this/rhs</code>, using fixed point arithmetic.
         * @throws ArithmeticException If <code>rhs</code> is zero.
         * @stable ICU 2.0
         */

        public BigDecimal Divide(BigDecimal rhs)
        {
            return this.DoDivide('D', rhs, plainMC, -1);
        }

        /**
         * Returns a plain <code>BigDecimal</code> whose value is <code>this/rhs</code>, using fixed point arithmetic and a
         * rounding mode.
         * <p>
         * The same as {@link #divide(BigDecimal, int, int)}, where the <code>BigDecimal</code> is <code>rhs</code>, and the
         * second parameter is <code>this.scale()</code>, and the third is <code>round</code>.
         * <p>
         * The length of the decimal part (the scale) of the result will therefore be the same as the scale of the current
         * object, if the latter were formatted without exponential notation.
         * <p>
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the division.
         * @param round The <code>int</code> rounding mode to be used for the division (see the {@link MathContext} class).
         * @return A plain <code>BigDecimal</code> whose value is <code>this/rhs</code>, using fixed point arithmetic and
         *         the specified rounding mode.
         * @throws IllegalArgumentException if <code>round</code> is not a valid rounding mode.
         * @throws ArithmeticException if <code>rhs</code> is zero.
         * @throws ArithmeticException if <code>round</code> is {@link MathContext#ROUND_UNNECESSARY} and <code>this.scale()</code> is insufficient to represent the result exactly.
         * @stable ICU 2.0
         */

        public BigDecimal Divide(BigDecimal rhs, RoundingMode round)
        {
            MathContext set = new MathContext(0, ExponentForm.Plain, false, round); // [checks round,
                                                                                    // too]
            return this.DoDivide('D', rhs, set, -1); // take scale from LHS
        }

        /**
         * Returns a plain <code>BigDecimal</code> whose value is <code>this/rhs</code>, using fixed point arithmetic and a
         * given scale and rounding mode.
         * <p>
         * The same as {@link #divide(BigDecimal, MathContext)}, where the <code>BigDecimal</code> is <code>rhs</code>,
         * <code>new MathContext(0, MathContext.PLAIN, false, round)</code>, except that the length of the decimal part (the
         * scale) to be used for the result is explicit rather than being taken from <code>this</code>.
         * <p>
         * The length of the decimal part (the scale) of the result will be the same as the scale of the current object, if
         * the latter were formatted without exponential notation.
         * <p>
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the division.
         * @param scale The <code>int</code> scale to be used for the result.
         * @param round The <code>int</code> rounding mode to be used for the division (see the {@link MathContext} class).
         * @return A plain <code>BigDecimal</code> whose value is <code>this/rhs</code>, using fixed point arithmetic and
         *         the specified rounding mode.
         * @throws IllegalArgumentException if <code>round</code> is not a valid rounding mode.
         * @throws ArithmeticException if <code>rhs</code> is zero.
         * @throws ArithmeticException if <code>scale</code> is negative.
         * @throws ArithmeticException if <code>round</code> is {@link MathContext#ROUND_UNNECESSARY} and <code>scale</code> is insufficient
         *             to represent the result exactly.
         * @stable ICU 2.0
         */

        public BigDecimal Divide(BigDecimal rhs, int scale, RoundingMode round)
        {
            MathContext set;
            if (scale < 0)
                throw new ArgumentOutOfRangeException(nameof(scale), "Negative scale:" + " " + scale);
            set = new MathContext(0, ExponentForm.Plain, false, round); // [checks round]
            return this.DoDivide('D', rhs, set, scale);
        }

        /**
         * Returns a <code>BigDecimal</code> whose value is <code>this/rhs</code>.
         * <p>
         * Implements the division (<b><code>/</code></b>) operator (as defined in the decimal documentation, see
         * {@link BigDecimal class header}), and returns the result as a <code>BigDecimal</code> object.
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the division.
         * @param set The <code>MathContext</code> arithmetic settings.
         * @return A <code>BigDecimal</code> whose value is <code>this/rhs</code>.
         * @throws ArithmeticException if <code>rhs</code> is zero.
         * @stable ICU 2.0
         */

        public BigDecimal Divide(BigDecimal rhs, MathContext set)
        {
            return this.DoDivide('D', rhs, set, -1);
        }

        /**
         * Returns a plain <code>BigDecimal</code> whose value is the integer part of <code>this/rhs</code>.
         * <p>
         * The same as {@link #divideInteger(BigDecimal, MathContext)}, where the <code>BigDecimal</code> is <code>rhs
         * </code>, and the context is <code>new MathContext(0, MathContext.PLAIN)</code>.
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the integer division.
         * @return A <code>BigDecimal</code> whose value is the integer part of <code>this/rhs</code>.
         * @throws ArithmeticException if <code>rhs</code> is zero.
         * @stable ICU 2.0
         */

        public BigDecimal DivideInteger(BigDecimal rhs)
        {
            // scale 0 to drop .000 when plain
            return this.DoDivide('I', rhs, plainMC, 0);
        }

        /**
         * Returns a <code>BigDecimal</code> whose value is the integer part of <code>this/rhs</code>.
         * <p>
         * Implements the integer division operator (as defined in the decimal documentation, see {@link BigDecimal class
         * header}), and returns the result as a <code>BigDecimal</code> object.
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the integer division.
         * @param set The <code>MathContext</code> arithmetic settings.
         * @return A <code>BigDecimal</code> whose value is the integer part of <code>this/rhs</code>.
         * @throws ArithmeticException if <code>rhs</code> is zero.
         * @throws ArithmeticException if the result will not fit in the number of digits specified for the context.
         * @stable ICU 2.0
         */

        public BigDecimal DivideInteger(BigDecimal rhs, MathContext set)
        {
            // scale 0 to drop .000 when plain
            return this.DoDivide('I', rhs, set, 0);
        }

        /**
         * Returns a plain <code>BigDecimal</code> whose value is the maximum of <code>this</code> and <code>rhs</code>.
         * <p>
         * The same as {@link #max(BigDecimal, MathContext)}, where the <code>BigDecimal</code> is <code>rhs</code>, and the
         * context is <code>new MathContext(0, MathContext.PLAIN)</code>.
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the comparison.
         * @return A <code>BigDecimal</code> whose value is the maximum of <code>this</code> and <code>rhs</code>.
         * @stable ICU 2.0
         */

        public BigDecimal Max(BigDecimal rhs)
        {
            return this.Max(rhs, plainMC);
        }

        /**
         * Returns a <code>BigDecimal</code> whose value is the maximum of <code>this</code> and <code>rhs</code>.
         * <p>
         * Returns the larger of the current object and the first parameter.
         * <p>
         * If calling the {@link #compareTo(BigDecimal, MathContext)} method with the same parameters would return <code>1
         * </code> or <code>0</code>, then the result of calling the {@link #plus(MathContext)} method on the current object
         * (using the same <code>MathContext</code> parameter) is returned. Otherwise, the result of calling the
         * {@link #plus(MathContext)} method on the first parameter object (using the same <code>MathContext</code>
         * parameter) is returned.
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the comparison.
         * @param set The <code>MathContext</code> arithmetic settings.
         * @return A <code>BigDecimal</code> whose value is the maximum of <code>this</code> and <code>rhs</code>.
         * @stable ICU 2.0
         */

        public BigDecimal Max(BigDecimal rhs, MathContext set)
        {
            if (rhs is null)
                throw new ArgumentNullException(nameof(rhs));

            if ((this.CompareTo(rhs, set)) >= 0)
                return this.Plus(set);
            else
                return rhs.Plus(set);
        }

        /**
         * Returns a plain <code>BigDecimal</code> whose value is the minimum of <code>this</code> and <code>rhs</code>.
         * <p>
         * The same as {@link #min(BigDecimal, MathContext)}, where the <code>BigDecimal</code> is <code>rhs</code>, and the
         * context is <code>new MathContext(0, MathContext.PLAIN)</code>.
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the comparison.
         * @return A <code>BigDecimal</code> whose value is the minimum of <code>this</code> and <code>rhs</code>.
         * @stable ICU 2.0
         */

        public BigDecimal Min(BigDecimal rhs) // ICU4N TODO: API - Revisit making these static
        {
            return this.Min(rhs, plainMC);
        }

        /**
         * Returns a <code>BigDecimal</code> whose value is the minimum of <code>this</code> and <code>rhs</code>.
         * <p>
         * Returns the smaller of the current object and the first parameter.
         * <p>
         * If calling the {@link #compareTo(BigDecimal, MathContext)} method with the same parameters would return <code>-1
         * </code> or <code>0</code>, then the result of calling the {@link #plus(MathContext)} method on the current object
         * (using the same <code>MathContext</code> parameter) is returned. Otherwise, the result of calling the
         * {@link #plus(MathContext)} method on the first parameter object (using the same <code>MathContext</code>
         * parameter) is returned.
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the comparison.
         * @param set The <code>MathContext</code> arithmetic settings.
         * @return A <code>BigDecimal</code> whose value is the minimum of <code>this</code> and <code>rhs</code>.
         * @stable ICU 2.0
         */

        public BigDecimal Min(BigDecimal rhs, MathContext set)
        {
            if (rhs is null)
                throw new ArgumentNullException(nameof(rhs));

            if ((this.CompareTo(rhs, set)) <= 0)
                return this.Plus(set);
            else
                return rhs.Plus(set);
        }

        /**
         * Returns a plain <code>BigDecimal</code> whose value is <code>this*rhs</code>, using fixed point arithmetic.
         * <p>
         * The same as {@link #add(BigDecimal, MathContext)}, where the <code>BigDecimal</code> is <code>rhs</code>, and the
         * context is <code>new MathContext(0, MathContext.PLAIN)</code>.
         * <p>
         * The length of the decimal part (the scale) of the result will be the sum of the scales of the operands, if they
         * were formatted without exponential notation.
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the multiplication.
         * @return A <code>BigDecimal</code> whose value is <code>this*rhs</code>, using fixed point arithmetic.
         * @stable ICU 2.0
         */

        public BigDecimal Multiply(BigDecimal rhs)
        {
            return this.Multiply(rhs, plainMC);
        }

        /**
         * Returns a <code>BigDecimal</code> whose value is <code>this*rhs</code>.
         * <p>
         * Implements the multiplication (<b><code>&#42;</code></b>) operator (as defined in the decimal documentation, see
         * {@link BigDecimal class header}), and returns the result as a <code>BigDecimal</code> object.
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the multiplication.
         * @param set The <code>MathContext</code> arithmetic settings.
         * @return A <code>BigDecimal</code> whose value is <code>this*rhs</code>.
         * @stable ICU 2.0
         */

        public BigDecimal Multiply(BigDecimal rhs, MathContext set)
        {
            // ICU4N: added guard clauses
            if (rhs is null)
                throw new ArgumentNullException(nameof(rhs));
            if (set is null)
                throw new ArgumentNullException(nameof(set));

            BigDecimal lhs;
            int padding;
            int reqdig;
            byte[] multer = null;
            byte[] multand = null;
            int multandlen;
            int acclen = 0;
            BigDecimal res;
            byte[] acc;
            int n = 0;
            byte mult = 0;
            if (set.LostDigits)
                CheckDigits(rhs, set.Digits);
            lhs = this; // name for clarity and proxy

            /* Prepare numbers (truncate, unless unlimited precision) */
            padding = 0; // trailing 0's to add
            reqdig = set.Digits; // local copy
            if (reqdig > 0)
            {
                if (lhs.mant.Length > reqdig)
                    lhs = Clone(lhs).Round(set);
                if (rhs.mant.Length > reqdig)
                    rhs = Clone(rhs).Round(set);
                // [we could reuse the new LHS for result in this case]
            }
            else
            {/* unlimited */
                // fixed point arithmetic will want every trailing 0; we add these
                // after the calculation rather than before, for speed.
                if (lhs.exp > 0)
                    padding = padding + lhs.exp;
                if (rhs.exp > 0)
                    padding = padding + rhs.exp;
            }

            // For best speed, as in DMSRCN, we use the shorter number as the
            // multiplier and the longer as the multiplicand.
            // 1999.12.22: We used to special case when the result would fit in
            // a long, but with Java 1.3 this gave no advantage.
            if (lhs.mant.Length < rhs.mant.Length)
            {
                multer = lhs.mant;
                multand = rhs.mant;
            }
            else
            {
                multer = rhs.mant;
                multand = lhs.mant;
            }

            /* Calculate how long result byte array will be */
            multandlen = (multer.Length + multand.Length) - 1; // effective length
                                                               // optimize for 75% of the cases where a carry is expected...
            if ((multer[0] * multand[0]) > 9)
                acclen = multandlen + 1;
            else
                acclen = multandlen;

            /* Now the main long multiplication loop */
            res = new BigDecimal(); // where we'll build result
            acc = new byte[acclen]; // accumulator, all zeros
                                    // 1998.07.01: calculate from left to right so that accumulator goes
                                    // to likely final length on first addition; this avoids a one-digit
                                    // extension (and object allocation) each time around the loop.
                                    // Initial number therefore has virtual zeros added to right.
            {
                int len7 = multer.Length;
                n = 0;
                for (; len7 > 0; len7--, n++)
                {
                    mult = multer[n];
                    if (mult != 0)
                    { // [optimization]
                      // accumulate [accumulator is reusable array]
                        acc = ByteAddSub(acc, acc.Length, multand, multandlen, mult, true);
                    }
                    // divide multiplicand by 10 for next digit to right
                    multandlen--; // 'virtual length'
                }
            }/* n */

            res.ind = (sbyte)(lhs.ind * rhs.ind); // final sign
            res.exp = (lhs.exp + rhs.exp) - padding; // final exponent
                                                     // [overflow is checked by finish]

            /* add trailing zeros to the result, if necessary */
            if (padding == 0)
                res.mant = acc;
            else
                res.mant = Extend(acc, acc.Length + padding); // add trailing 0s
            return res.Finish(set, false);
        }

        /**
         * Returns a plain <code>BigDecimal</code> whose value is <code>-this</code>.
         * <p>
         * The same as {@link #negate(MathContext)}, where the context is <code>new MathContext(0, MathContext.PLAIN)</code>
         * .
         * <p>
         * The length of the decimal part (the scale) of the result will be be <code>this.scale()</code>
         *
         *
         * @return A <code>BigDecimal</code> whose value is <code>-this</code>.
         * @stable ICU 2.0
         */

        public BigDecimal Negate()
        {
            return this.Negate(plainMC);
        }

        /**
         * Returns a <code>BigDecimal</code> whose value is <code>-this</code>.
         * <p>
         * Implements the negation (Prefix <b><code>-</code></b>) operator (as defined in the decimal documentation, see
         * {@link BigDecimal class header}), and returns the result as a <code>BigDecimal</code> object.
         *
         * @param set The <code>MathContext</code> arithmetic settings.
         * @return A <code>BigDecimal</code> whose value is <code>-this</code>.
         * @stable ICU 2.0
         */

        public BigDecimal Negate(MathContext set)
        {
            // ICU4N: added guard clause
            if (set is null)
                throw new ArgumentNullException(nameof(set));

            BigDecimal res;
            // Originally called minus(), changed to matched Java precedents
            // This simply clones, flips the sign, and possibly rounds
            if (set.LostDigits)
                CheckDigits((BigDecimal)null, set.Digits);
            res = Clone(this); // safe copy
            res.ind = (sbyte)-res.ind;
            return res.Finish(set, false);
        }

        /**
         * Returns a plain <code>BigDecimal</code> whose value is <code>+this</code>. Note that <code>this</code> is not
         * necessarily a plain <code>BigDecimal</code>, but the result will always be.
         * <p>
         * The same as {@link #plus(MathContext)}, where the context is <code>new MathContext(0, MathContext.PLAIN)</code>.
         * <p>
         * The length of the decimal part (the scale) of the result will be be <code>this.scale()</code>
         *
         * @return A <code>BigDecimal</code> whose value is <code>+this</code>.
         * @stable ICU 2.0
         */

        public BigDecimal Plus()
        {
            return this.Plus(plainMC);
        }

        /**
         * Returns a <code>BigDecimal</code> whose value is <code>+this</code>.
         * <p>
         * Implements the plus (Prefix <b><code>+</code></b>) operator (as defined in the decimal documentation, see
         * {@link BigDecimal class header}), and returns the result as a <code>BigDecimal</code> object.
         * <p>
         * This method is useful for rounding or otherwise applying a context to a decimal value.
         *
         * @param set The <code>MathContext</code> arithmetic settings.
         * @return A <code>BigDecimal</code> whose value is <code>+this</code>.
         * @stable ICU 2.0
         */

        public BigDecimal Plus(MathContext set)
        {
            // ICU4N: Added guard clause
            if (set is null)
                throw new ArgumentNullException(nameof(set));

            // This clones and forces the result to the new settings
            // May return same object
            if (set.LostDigits)
                CheckDigits((BigDecimal)null, set.Digits);
            // Optimization: returns same object for some common cases
            if (set.Form == ExponentForm.Plain)
                if (this.form == (byte)ExponentForm.Plain)
                {
                    if (this.mant.Length <= set.Digits)
                        return this;
                    if (set.Digits == 0)
                        return this;
                }
            return Clone(this).Finish(set, false);
        }

        /**
         * Returns a plain <code>BigDecimal</code> whose value is <code>this**rhs</code>, using fixed point arithmetic.
         * <p>
         * The same as {@link #pow(BigDecimal, MathContext)}, where the <code>BigDecimal</code> is <code>rhs</code>, and the
         * context is <code>new MathContext(0, MathContext.PLAIN)</code>.
         * <p>
         * The parameter is the power to which the <code>this</code> will be raised; it must be in the range 0 through
         * 999999999, and must have a decimal part of zero. Note that these restrictions may be removed in the future, so
         * they should not be used as a test for a whole number.
         * <p>
         * In addition, the power must not be negative, as no <code>MathContext</code> is used and so the result would then
         * always be 0.
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the operation (the power).
         * @return A <code>BigDecimal</code> whose value is <code>this**rhs</code>, using fixed point arithmetic.
         * @throws ArithmeticException if <code>rhs</code> is out of range or is not a whole number.
         * @stable ICU 2.0
         */

        public BigDecimal Pow(BigDecimal rhs)
        {
            return this.Pow(rhs, plainMC);
        }

        // The name for this method is inherited from the precedent set by the
        // BigInteger and Math classes.

        /**
         * Returns a <code>BigDecimal</code> whose value is <code>this**rhs</code>.
         * <p>
         * Implements the power (<b><code>^</code></b>) operator (as defined in the decimal documentation, see
         * {@link BigDecimal class header}), and returns the result as a <code>BigDecimal</code> object.
         * <p>
         * The first parameter is the power to which the <code>this</code> will be raised; it must be in the range
         * -999999999 through 999999999, and must have a decimal part of zero. Note that these restrictions may be removed
         * in the future, so they should not be used as a test for a whole number.
         * <p>
         * If the <code>digits</code> setting of the <code>MathContext</code> parameter is 0, the power must be zero or
         * positive.
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the operation (the power).
         * @param set The <code>MathContext</code> arithmetic settings.
         * @return A <code>BigDecimal</code> whose value is <code>this**rhs</code>.
         * @throws ArithmeticException if <code>rhs</code> is out of range or is not a whole number.
         * @stable ICU 2.0
         */

        public BigDecimal Pow(BigDecimal rhs, MathContext set)
        {
            // ICU4N: added guard clauses
            if (rhs is null)
                throw new ArgumentNullException(nameof(rhs));
            if (set is null)
                throw new ArgumentNullException(nameof(set));

            int n;
            BigDecimal lhs;
            int reqdig;
            int workdigits = 0;
            int L = 0;
            MathContext workset;
            BigDecimal res;
            bool seenbit;
            int i = 0;
            if (set.LostDigits)
                CheckDigits(rhs, set.Digits);
            n = rhs.IntCheck(MinArg, MaxArg); // check RHS by the rules
            lhs = this; // clarified name

            reqdig = set.Digits; // local copy (heavily used)
            if (reqdig == 0)
            {
                if (rhs.ind == isneg)
                    throw new OverflowException("Negative power:" + " " + rhs.ToString());
                workdigits = 0;
            }
            else
            {/* non-0 digits */
                if ((rhs.mant.Length + rhs.exp) > reqdig)
                    throw new OverflowException("Too many digits:" + " " + rhs.ToString());

                /* Round the lhs to DIGITS if need be */
                if (lhs.mant.Length > reqdig)
                    lhs = Clone(lhs).Round(set);

                /* L for precision calculation [see ANSI X3.274-1996] */
                L = rhs.mant.Length + rhs.exp; // length without decimal zeros/exp
                workdigits = (reqdig + L) + 1; // calculate the working DIGITS
            }

            /* Create a copy of set for working settings */
            // Note: no need to check for lostDigits again.
            // 1999.07.17 Note: this construction must follow RHS check
            workset = new MathContext(workdigits, set.Form, false, set.RoundingMode);

            res = One; // accumulator
            if (n == 0)
                return res; // x**0 == 1
            if (n < 0)
                n = -n; // [rhs.ind records the sign]
            seenbit = false; // set once we've seen a 1-bit
            {
                i = 1;
                /*i:*/
                for (; ; i++)
                { // for each bit [top bit ignored]
                    n = n + n; // shift left 1 bit
                    if (n < 0)
                    { // top bit is set
                        seenbit = true; // OK, we're off
                        res = res.Multiply(lhs, workset); // acc=acc*x
                    }
                    if (i == 31)
                        break;// i; // that was the last bit
                    if ((!seenbit))
                        continue; // i; // we don't have to square 1
                    res = res.Multiply(res, workset); // acc=acc*acc [square]
                }
            }/* i */// 32 bits
            if (rhs.ind < 0) // was a **-n [hence digits>0]
                res = One.Divide(res, workset); // .. so acc=1/acc
            return res.Finish(set, true); // round and strip [original digits]
        }

        /**
         * Returns a plain <code>BigDecimal</code> whose value is the remainder of <code>this/rhs</code>, using fixed point
         * arithmetic.
         * <p>
         * The same as {@link #remainder(BigDecimal, MathContext)}, where the <code>BigDecimal</code> is <code>rhs</code>,
         * and the context is <code>new MathContext(0, MathContext.PLAIN)</code>.
         * <p>
         * This is not the modulo operator -- the result may be negative.
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the remainder operation.
         * @return A <code>BigDecimal</code> whose value is the remainder of <code>this/rhs</code>, using fixed point
         *         arithmetic.
         * @throws ArithmeticException if <code>rhs</code> is zero.
         * @stable ICU 2.0
         */

        public BigDecimal Remainder(BigDecimal rhs)
        {
            return this.DoDivide('R', rhs, plainMC, -1);
        }

        /**
         * Returns a <code>BigDecimal</code> whose value is the remainder of <code>this/rhs</code>.
         * <p>
         * Implements the remainder operator (as defined in the decimal documentation, see {@link BigDecimal class header}),
         * and returns the result as a <code>BigDecimal</code> object.
         * <p>
         * This is not the modulo operator -- the result may be negative.
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the remainder operation.
         * @param set The <code>MathContext</code> arithmetic settings.
         * @return A <code>BigDecimal</code> whose value is the remainder of <code>this+rhs</code>.
         * @throws ArithmeticException if <code>rhs</code> is zero.
         * @throws ArithmeticException  if the integer part of the result will not fit in the number of digits specified for the context.
         * @stable ICU 2.0
         */

        public BigDecimal Remainder(BigDecimal rhs, MathContext set)
        {
            return this.DoDivide('R', rhs, set, -1);
        }

        /**
         * Returns a plain <code>BigDecimal</code> whose value is <code>this-rhs</code>, using fixed point arithmetic.
         * <p>
         * The same as {@link #subtract(BigDecimal, MathContext)}, where the <code>BigDecimal</code> is <code>rhs</code>,
         * and the context is <code>new MathContext(0, MathContext.PLAIN)</code>.
         * <p>
         * The length of the decimal part (the scale) of the result will be the maximum of the scales of the two operands.
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the subtraction.
         * @return A <code>BigDecimal</code> whose value is <code>this-rhs</code>, using fixed point arithmetic.
         * @stable ICU 2.0
         */

        public BigDecimal Subtract(BigDecimal rhs)
        {
            return this.Subtract(rhs, plainMC);
        }

        /**
         * Returns a <code>BigDecimal</code> whose value is <code>this-rhs</code>.
         * <p>
         * Implements the subtraction (<b><code>-</code></b>) operator (as defined in the decimal documentation, see
         * {@link BigDecimal class header}), and returns the result as a <code>BigDecimal</code> object.
         *
         * @param rhs The <code>BigDecimal</code> for the right hand side of the subtraction.
         * @param set The <code>MathContext</code> arithmetic settings.
         * @return A <code>BigDecimal</code> whose value is <code>this-rhs</code>.
         * @stable ICU 2.0
         */

        public BigDecimal Subtract(BigDecimal rhs, MathContext set)
        {
            // ICU4N: added guard clauses
            if (rhs is null)
                throw new ArgumentNullException(nameof(rhs));
            if (set is null)
                throw new ArgumentNullException(nameof(set));

            BigDecimal newrhs;
            if (set.LostDigits)
                CheckDigits(rhs, set.Digits);
            // [add will recheck .. but would report -rhs]
            /* carry out the subtraction */
            // we could fastpath -0, but it is too rare.
            newrhs = Clone(rhs); // safe copy
            newrhs.ind = (sbyte)-newrhs.ind; // prepare to subtract
            return this.Add(newrhs, set); // arithmetic
        }

        /* ---------------------------------------------------------------- */
        /* Other methods */
        /* ---------------------------------------------------------------- */

        /**
         * Converts this <code>BigDecimal</code> to a <code>byte</code>. If the <code>BigDecimal</code> has a non-zero
         * decimal part or is out of the possible range for a <code>byte</code> (8-bit signed integer) result then an <code>
         * ArithmeticException</code> is thrown.
         *
         * @return A <code>byte</code> equal in value to <code>this</code>.
         * @throws ArithmeticException if <code>this</code> has a non-zero decimal part, or will not fit in a <code>byte</code>.
         * @stable ICU 2.0
         */

        public byte ToByteExact()
        {
            int num;
            num = this.ToInt32Exact(); // will check decimal part too
            if ((num > 127) | (num < (-128)))
                throw new OverflowException("Conversion overflow:" + " " + this.ToString());
            return (byte)num;
        }

        // ICU4N specific
        public sbyte ToSByteExact()
        {
            int num;
            num = this.ToInt32Exact(); // will check decimal part too
            if ((num > 127) | (num < (-128)))
                throw new OverflowException("Conversion overflow:" + " " + this.ToString());
            return (sbyte)num;
        }

        /**
         * Converts this <code>BigDecimal</code> to a <code>double</code>. If the <code>BigDecimal</code> is out of the
         * possible range for a <code>double</code> (64-bit signed floating point) result then an <code>ArithmeticException
         * </code> is thrown.
         * <p>
         * The double produced is identical to result of expressing the <code>BigDecimal</code> as a <code>String</code> and
         * then converting it using the <code>Double(String)</code> constructor; this can result in values of <code>
         * Double.NEGATIVE_INFINITY</code> or <code>Double.POSITIVE_INFINITY</code>.
         *
         * @return A <code>double</code> corresponding to <code>this</code>.
         * @stable ICU 2.0
         */

        public override double ToDouble()
        {
            // We go via a String [as does BigDecimal in JDK 1.2]
            // Next line could possibly raise NumberFormatException
            return Double.GetInstance(ToString(), CultureInfo.InvariantCulture).ToDouble();
        }

        /**
         * Compares this <code>BigDecimal</code> with <code>rhs</code> for equality.
         * <p>
         * If the parameter is <code>null</code>, or is not an instance of the BigDecimal type, or is not exactly equal to
         * the current <code>BigDecimal</code> object, then <i>false</i> is returned. Otherwise, <i>true</i> is returned.
         * <p>
         * "Exactly equal", here, means that the <code>String</code> representations of the <code>BigDecimal</code> numbers
         * are identical (they have the same characters in the same sequence).
         * <p>
         * The {@link #compareTo(BigDecimal, MathContext)} method should be used for more general comparisons.
         *
         * @param obj The <code>Object</code> for the right hand side of the comparison.
         * @return A <code>boolean</code> whose value <i>true</i> if and only if the operands have identical string
         *         representations.
         * @throws ClassCastException if <code>rhs</code> cannot be cast to a <code>BigDecimal</code> object.
         * @stable ICU 2.0
         * @see #compareTo(BigDecimal)
         * @see #compareTo(BigDecimal, MathContext)
         */

        public override bool Equals(object obj)
        {
            int i = 0;
            char[] lca = null;
            char[] rca = null;
            // We are equal iff toString of both are exactly the same
            if (obj == null)
                return false; // not equal
            if (!(obj is BigDecimal rhs))
                return false; // not a decimal
            if (this.ind != rhs.ind)
                return false; // different signs never match
            if (((this.mant.Length == rhs.mant.Length) && (this.exp == rhs.exp)) && (this.form == rhs.form))

            { // mantissas say all
              // here with equal-length byte arrays to compare
                {
                    int len8 = this.mant.Length;
                    i = 0;
                    for (; len8 > 0; len8--, i++)
                    {
                        if (this.mant[i] != rhs.mant[i])
                            return false;
                    }
                }/* i */
            }
            else
            { // need proper layout
                lca = this.GetLayout(); // layout to character array
                rca = rhs.GetLayout();
                if (lca.Length != rca.Length)
                    return false; // mismatch
                                  // here with equal-length character arrays to compare
                {
                    int len9 = lca.Length;
                    i = 0;
                    for (; len9 > 0; len9--, i++)
                    {
                        if (lca[i] != rca[i])
                            return false;
                    }
                }/* i */
            }
            return true; // arrays have identical content
        }

        /**
         * Converts this <code>BigDecimal</code> to a <code>float</code>. If the <code>BigDecimal</code> is out of the
         * possible range for a <code>float</code> (32-bit signed floating point) result then an <code>ArithmeticException
         * </code> is thrown.
         * <p>
         * The float produced is identical to result of expressing the <code>BigDecimal</code> as a <code>String</code> and
         * then converting it using the <code>Float(String)</code> constructor; this can result in values of <code>
         * Float.NEGATIVE_INFINITY</code> or <code>Float.POSITIVE_INFINITY</code>.
         *
         * @return A <code>float</code> corresponding to <code>this</code>.
         * @stable ICU 2.0
         */

        public override float ToSingle()
        {
            return Float.GetInstance(ToString(), CultureInfo.InvariantCulture).ToSingle();
        }

        /**
         * Returns the <code>String</code> representation of this <code>BigDecimal</code>, modified by layout parameters.
         * <p>
         * <i>This method is provided as a primitive for use by more sophisticated classes, such as <code>DecimalFormat
         * </code>, that can apply locale-sensitive editing of the result. The level of formatting that it provides is a
         * necessary part of the BigDecimal class as it is sensitive to and must follow the calculation and rounding rules
         * for BigDecimal arithmetic. However, if the function is provided elsewhere, it may be removed from this class.
         * </i>
         * <p>
         * The parameters, for both forms of the <code>format</code> method are all of type <code>int</code>. A value of -1
         * for any parameter indicates that the default action or value for that parameter should be used.
         * <p>
         * The parameters, <code>before</code> and <code>after</code>, specify the number of characters to be used for the
         * integer part and decimal part of the result respectively. Exponential notation is not used. If either parameter
         * is -1 (which indicates the default action), the number of characters used will be exactly as many as are needed
         * for that part.
         * <p>
         * <code>before</code> must be a positive number; if it is larger than is needed to contain the integer part, that
         * part is padded on the left with blanks to the requested length. If <code>before</code> is not large enough to
         * contain the integer part of the number (including the sign, for negative numbers) an exception is thrown.
         * <p>
         * <code>after</code> must be a non-negative number; if it is not the same size as the decimal part of the number,
         * the number will be rounded (or extended with zeros) to fit. Specifying 0 for <code>after</code> will cause the
         * number to be rounded to an integer (that is, it will have no decimal part or decimal point). The rounding method
         * will be the default, <code>MathContext.ROUND_HALF_UP</code>.
         * <p>
         * Other rounding methods, and the use of exponential notation, can be selected by using
         * {@link #format(int,int,int,int,int,int)}. Using the two-parameter form of the method has exactly the same effect
         * as using the six-parameter form with the final four parameters all being -1.
         *
         * @param before The <code>int</code> specifying the number of places before the decimal point. Use -1 for 'as many as are needed'.
         * @param after The <code>int</code> specifying the number of places after the decimal point. Use -1 for 'as many as are needed'.
         * @return A <code>String</code> representing this <code>BigDecimal</code>, laid out according to the specified parameters
         * @throws ArithmeticException if the number cannot be laid out as requested.
         * @throws IllegalArgumentException if a parameter is out of range.
         * @stable ICU 2.0
         * @see #toString
         * @see #toCharArray
         */

        public string Format(int before, int after)
        {
            return Format(before, after, -1, -1, ExponentForm.Scientific, RoundHalfUp);
        }

        /**
         * Returns the <code>String</code> representation of this <code>BigDecimal</code>, modified by layout parameters and
         * allowing exponential notation.
         * <p>
         * <i>This method is provided as a primitive for use by more sophisticated classes, such as <code>DecimalFormat
         * </code>, that can apply locale-sensitive editing of the result. The level of formatting that it provides is a
         * necessary part of the BigDecimal class as it is sensitive to and must follow the calculation and rounding rules
         * for BigDecimal arithmetic. However, if the function is provided elsewhere, it may be removed from this class.
         * </i>
         * <p>
         * The parameters are all of type <code>int</code>. A value of -1 for any parameter indicates that the default
         * action or value for that parameter should be used.
         * <p>
         * The first two parameters (<code>before</code> and <code>after</code>) specify the number of characters to be used
         * for the integer part and decimal part of the result respectively, as defined for {@link #format(int,int)}. If
         * either of these is -1 (which indicates the default action), the number of characters used will be exactly as many
         * as are needed for that part.
         * <p>
         * The remaining parameters control the use of exponential notation and rounding. Three (<code>explaces</code>,
         * <code>exdigits</code>, and <code>exform</code>) control the exponent part of the result. As before, the default
         * action for any of these parameters may be selected by using the value -1.
         * <p>
         * <code>explaces</code> must be a positive number; it sets the number of places (digits after the sign of the
         * exponent) to be used for any exponent part, the default (when <code>explaces</code> is -1) being to use as many
         * as are needed. If <code>explaces</code> is not -1, space is always reserved for an exponent; if one is not needed
         * (for example, if the exponent will be 0) then <code>explaces</code>+2 blanks are appended to the result.
         * (This preserves vertical alignment of similarly formatted numbers in a monospace font.) If <code>explaces
         * </code> is not -1 and is not large enough to contain the exponent, an exception is thrown.
         * <p>
         * <code>exdigits</code> sets the trigger point for use of exponential notation. If, before any rounding, the number
         * of places needed before the decimal point exceeds <code>exdigits</code>, or if the absolute value of the result
         * is less than <code>0.000001</code>, then exponential form will be used, provided that <code>exdigits</code> was
         * specified. When <code>exdigits</code> is -1, exponential notation will never be used. If 0 is specified for
         * <code>exdigits</code>, exponential notation is always used unless the exponent would be 0.
         * <p>
         * <code>exform</code> sets the form for exponential notation (if needed). It may be either
         * {@link MathContext#SCIENTIFIC} or {@link MathContext#ENGINEERING}. If the latter, engineering, form is requested,
         * up to three digits (plus sign, if negative) may be needed for the integer part of the result (<code>before</code>
         * ). Otherwise, only one digit (plus sign, if negative) is needed.
         * <p>
         * Finally, the sixth argument, <code>exround</code>, selects the rounding algorithm to be used, and must be one of
         * the values indicated by a public constant in the {@link MathContext} class whose name starts with <code>ROUND_
         * </code>. The default (<code>ROUND_HALF_UP</code>) may also be selected by using the value -1, as before.
         * <p>
         * The special value <code>MathContext.ROUND_UNNECESSARY</code> may be used to detect whether non-zero digits are
         * discarded -- if <code>exround</code> has this value than if non-zero digits would be discarded (rounded) during
         * formatting then an <code>ArithmeticException</code> is thrown.
         *
         * @param before The <code>int</code> specifying the number of places before the decimal point. Use -1 for 'as many as
         *            are needed'.
         * @param after The <code>int</code> specifying the number of places after the decimal point. Use -1 for 'as many as
         *            are needed'.
         * @param explaces The <code>int</code> specifying the number of places to be used for any exponent. Use -1 for 'as many
         *            as are needed'.
         * @param exdigits The <code>int</code> specifying the trigger (digits before the decimal point) which if exceeded causes
         *            exponential notation to be used. Use 0 to force exponential notation. Use -1 to force plain notation
         *            (no exponential notation).
         * @param exformint The <code>int</code> specifying the form of exponential notation to be used (
         *            {@link MathContext#SCIENTIFIC} or {@link MathContext#ENGINEERING}).
         * @param exround The <code>int</code> specifying the rounding mode to use. Use -1 for the default,
         *            {@link MathContext#ROUND_HALF_UP}.
         * @return A <code>String</code> representing this <code>BigDecimal</code>, laid out according to the specified
         *         parameters
         * @throws ArithmeticException if the number cannot be laid out as requested.
         * @throws IllegalArgumentException if a parameter is out of range.
         * @see #toString
         * @see #toCharArray
         * @stable ICU 2.0
         */

        public string Format(int before, int after, int explaces, int exdigits, ExponentForm exformint, RoundingMode exround) // ICU4N TODO: Revisit default values for ExponentForm and RoundingMode (ideally 0 rather than -1, but 0 is currently the value of another mode)
        {
            BigDecimal num;
            int mag = 0;
            int thisafter = 0;
            int lead = 0;
            byte[] newmant = null;
            int chop = 0;
            int need = 0;
            int oldexp = 0;
            char[] a;
            int p = 0;
            char[] newa = null;
            int i = 0;
            int places = 0;

            /* Check arguments */
            if ((before < (-1)) || (before == 0))
                BadArg("format", 1, before.ToString(CultureInfo.InvariantCulture));
            if (after < (-1))
                BadArg("format", 2, after.ToString(CultureInfo.InvariantCulture));
            if ((explaces < (-1)) || (explaces == 0))
                BadArg("format", 3, explaces.ToString(CultureInfo.InvariantCulture));
            if (exdigits < (-1))
                BadArg("format", 4, explaces.ToString(CultureInfo.InvariantCulture));
            {/* select */
                if (exformint == ExponentForm.Scientific)
                {
                    // Intentionally blank
                }
                else if (exformint == ExponentForm.Engineering)
                {
                    // Intentionally blank
                }
                else if ((sbyte)exformint == (-1))
                    exformint = ExponentForm.Scientific;
                // note PLAIN isn't allowed
                else
                {
                    BadArg("format", 5, ((sbyte)exformint).ToString(CultureInfo.InvariantCulture));
                }
            }
            // checking the rounding mode is done by trying to construct a
            // MathContext object with that mode; it will fail if bad
            if (exround != RoundHalfUp)
            {
                // if non-default...
                if ((int)exround == (-1))
                    exround = RoundHalfUp;
                else if (!MathContext.IsValidRound(exround)) // ICU4N: Do an actual check rather than try/catch
                    BadArg("format", 6, ((int)exround).ToString(CultureInfo.InvariantCulture));


                //try
                //{ // if non-default...
                //    if ((int)exround == (-1))
                //        exround = RoundHalfUp;
                //    else
                //        new MathContext(9, ExponentForm.Scientific, false, exround);
                //}
                //catch (ArgumentException e) // ICU4N TODO: TryFormat
                //{
                //    BadArg("format", 6, ((int)exround).ToString(CultureInfo.InvariantCulture));
                //}
            }

            num = Clone(this); // make private copy

            /*
             * Here: num is BigDecimal to format before is places before point [>0] after is places after point [>=0]
             * explaces is exponent places [>0] exdigits is exponent digits [>=0] exformint is exponent form [one of two]
             * exround is rounding mode [one of eight] 'before' through 'exdigits' are -1 if not specified
             */

            /* determine form */
            //{
            //    do
            //    {/* select */
            if (exdigits == (-1))
                num.form = (byte)ExponentForm.Plain;
            else if (num.ind == iszero)
                num.form = (byte)ExponentForm.Plain;
            else
            {
                // determine whether triggers
                mag = num.exp + num.mant.Length;
                if (mag > exdigits)
                    num.form = (byte)exformint;
                else if (mag < (-5))
                    num.form = (byte)exformint;
                else
                    num.form = (byte)ExponentForm.Plain;
            }
            //    } while (false);
            //}/* setform */

            /*
             * If 'after' was specified then we may need to adjust the mantissa. This is a little tricky, as we must conform
             * to the rules of exponential layout if necessary (e.g., we cannot end up with 10.0 if scientific).
             */
            if (after >= 0)
            {
                /*setafter:*/
                for (; ; )
                {
                    // calculate the current after-length
                    {/* select */
                        if (num.form == (byte)ExponentForm.Plain)
                            thisafter = -num.exp; // has decimal part
                        else if (num.form == (byte)ExponentForm.Scientific)
                            thisafter = num.mant.Length - 1;
                        else
                        { // engineering
                            lead = (((num.exp + num.mant.Length) - 1)) % 3; // exponent to use
                            if (lead < 0)
                                lead = 3 + lead; // negative exponent case
                            lead++; // number of leading digits
                            if (lead >= num.mant.Length)
                                thisafter = 0;
                            else
                                thisafter = num.mant.Length - lead;
                        }
                    }
                    if (thisafter == after)
                        break; // setafter; // we're in luck
                    if (thisafter < after)
                    { // need added trailing zeros
                      // [thisafter can be negative]
                        newmant = Extend(num.mant, (num.mant.Length + after) - thisafter);
                        num.mant = newmant;
                        num.exp = num.exp - ((after - thisafter)); // adjust exponent
                        if (num.exp < MinExp)
                            throw new OverflowException("Exponent Overflow:" + " " + num.exp);
                        break; // setafter;
                    }
                    // We have too many digits after the decimal point; this could
                    // cause a carry, which could change the mantissa...
                    // Watch out for implied leading zeros in PLAIN case
                    chop = thisafter - after; // digits to lop [is >0]
                    if (chop > num.mant.Length)
                    { // all digits go, no chance of carry
                      // carry on with zero
                        num.mant = Zero.mant;
                        num.ind = iszero;
                        num.exp = 0;
                        continue; // setafter; // recheck: we may need trailing zeros
                    }
                    // we have a digit to inspect from existing mantissa
                    // round the number as required
                    need = num.mant.Length - chop; // digits to end up with [may be 0]
                    oldexp = num.exp; // save old exponent
                    num.Round(need, exround);
                    // if the exponent grew by more than the digits we chopped, then
                    // we must have had a carry, so will need to recheck the layout
                    if ((num.exp - oldexp) == chop)
                        break; // setafter; // number did not have carry
                               // mantissa got extended .. so go around and check again
                }
            }/* setafter */

            a = num.GetLayout(); // lay out, with exponent if required, etc.

            /* Here we have laid-out number in 'a' */
            // now apply 'before' and 'explaces' as needed
            if (before > 0)
            {
                // look for '.' or 'E'
                {
                    int len11 = a.Length;
                    p = 0;
                    /*p:*/
                    for (; len11 > 0; len11--, p++)
                    {
                        if (a[p] == '.')
                            break; // p;
                        if (a[p] == 'E')
                            break; // p;
                    }
                }/* p */
                // p is now offset of '.', 'E', or character after end of array
                // that is, the current length of before part
                if (p > before)
                    BadArg("format", 1, before.ToString(CultureInfo.InvariantCulture)); // won't fit
                if (p < before)
                { // need leading blanks
                    newa = new char[(a.Length + before) - p];
                    {
                        int len12 = before - p;
                        i = 0;
                        for (; len12 > 0; len12--, i++)
                        {
                            newa[i] = ' ';
                        }
                    }/* i */
                    Array.Copy(a, 0, newa, i, a.Length);
                    a = newa;
                }
                // [if p=before then it's just the right length]
            }

            if (explaces > 0)
            {
                // look for 'E' [cannot be at offset 0]
                {
                    int len13 = a.Length - 1;
                    p = a.Length - 1;
                    /*p:*/
                    for (; len13 > 0; len13--, p--)
                    {
                        if (a[p] == 'E')
                            break;// p;
                    }
                }/* p */
                // p is now offset of 'E', or 0
                if (p == 0)
                { // no E part; add trailing blanks
                    newa = new char[(a.Length + explaces) + 2];
                    Array.Copy(a, 0, newa, 0, a.Length);
                    {
                        int len14 = explaces + 2;
                        i = a.Length;
                        for (; len14 > 0; len14--, i++)
                        {
                            newa[i] = ' ';
                        }
                    }/* i */
                    a = newa;
                }
                else
                {/* found E */// may need to insert zeros
                    places = (a.Length - p) - 2; // number so far
                    if (places > explaces)
                        BadArg("format", 3, explaces.ToString(CultureInfo.InvariantCulture));
                    if (places < explaces)
                    { // need to insert zeros
                        newa = new char[(a.Length + explaces) - places];
                        Array.Copy(a, 0, newa, 0, p + 2); // through E
                                                          // and sign
                        {
                            int len15 = explaces - places;
                            i = p + 2;
                            for (; len15 > 0; len15--, i++)
                            {
                                newa[i] = '0';
                            }
                        }/* i */
                        Array.Copy(a, p + 2, newa, i, places); // remainder
                                                               // of
                                                               // exponent
                        a = newa;
                    }
                    // [if places=explaces then it's just the right length]
                }
            }
            return new string(a);
        }

        /**
         * Returns the hashcode for this <code>BigDecimal</code>. This hashcode is suitable for use by the <code>
         * java.util.Hashtable</code> class.
         * <p>
         * Note that two <code>BigDecimal</code> objects are only guaranteed to produce the same hashcode if they are
         * exactly equal (that is, the <code>String</code> representations of the <code>BigDecimal</code> numbers are
         * identical -- they have the same characters in the same sequence).
         *
         * @return An <code>int</code> that is the hashcode for <code>this</code>.
         * @stable ICU 2.0
         */

        public override int GetHashCode()
        {
            // Maybe calculate ourselves, later. If so, note that there can be
            // more than one internal representation for a given toString() result.
            return this.ToString().GetHashCode();
        }

        /**
         * Converts this <code>BigDecimal</code> to an <code>int</code>. If the <code>BigDecimal</code> has a non-zero
         * decimal part it is discarded. If the <code>BigDecimal</code> is out of the possible range for an <code>int</code>
         * (32-bit signed integer) result then only the low-order 32 bits are used. (That is, the number may be
         * <i>decapitated</i>.) To avoid unexpected errors when these conditions occur, use the {@link #intValueExact}
         * method.
         *
         * @return An <code>int</code> converted from <code>this</code>, truncated and decapitated if necessary.
         * @stable ICU 2.0
         */

        public override int ToInt32()
        {
            // ICU4N TODO: In .NET Standard 2.1 and higher there is a TryToInt32() method that returns false if
            // the value won't fit. It should be used as the primary way to convert, using the below as a fallback.

            // Create a BigInteger value
            BigInteger bigInt = ToBigInteger();

            if (bigInt >= int.MinValue && bigInt <= int.MaxValue)
            {
                return (int)bigInt;
            }

            bool negate = bigInt.Sign < 1;

            if (negate)
            {
                bigInt = BigInteger.Abs(bigInt);
            }

            // Convert the BigInteger to a byte array
            byte[] bytes = bigInt.ToByteArray();

            // Truncate all but the low-order 4 bytes
            const int size = sizeof(int);
            if (bytes.Length > size)
            {
                Array.Resize(ref bytes, size); // Decapitate
            }

            // Convert the byte array to an int
            int result = BitConverter.ToInt32(bytes, 0);

            // If the BigInteger value was negative, negate the result
            if (negate)
            {
                result = -result;
            }

            return result;
        }

        /**
         * Converts this <code>BigDecimal</code> to an <code>int</code>. If the <code>BigDecimal</code> has a non-zero
         * decimal part or is out of the possible range for an <code>int</code> (32-bit signed integer) result then an
         * <code>ArithmeticException</code> is thrown.
         *
         * @return An <code>int</code> equal in value to <code>this</code>.
         * @throws ArithmeticException if <code>this</code> has a non-zero decimal part, or will not fit in an <code>int</code>.
         * @stable ICU 2.0
         */

        public int ToInt32Exact()
        {
            int lodigit;
            int useexp = 0;
            int result;
            int i = 0;
            int topdig = 0;
            // This does not use longValueExact() as the latter can be much
            // slower.
            // intcheck (from pow) relies on this to check decimal part
            if (ind == iszero)
                return 0; // easy, and quite common
            /* test and drop any trailing decimal part */
            lodigit = mant.Length - 1;
            if (exp < 0)
            {
                lodigit = lodigit + exp; // reduces by -(-exp)
                /* all decimal places must be 0 */
                if ((!(AllZero(mant, lodigit + 1))))
                    throw new OverflowException("Decimal part non-zero:" + " " + this.ToString());
                if (lodigit < 0)
                    return 0; // -1<this<1
                useexp = 0;
            }
            else
            {/* >=0 */
                if ((exp + lodigit) > 9) // early exit
                    throw new OverflowException("Conversion overflow:" + " " + this.ToString());
                useexp = exp;
            }
            /* convert the mantissa to binary, inline for speed */
            result = 0;
            {
                int len16 = lodigit + useexp;
                i = 0;
                for (; i <= len16; i++)
                {
                    result = result * 10;
                    if (i <= lodigit)
                        result = result + mant[i];
                }
            }/* i */

            /* Now, if the risky length, check for overflow */
            if ((lodigit + useexp) == 9)
            {
                // note we cannot just test for -ve result, as overflow can move a
                // zero into the top bit [consider 5555555555]
                topdig = result / 1000000000; // get top digit, preserving sign
                if (topdig != mant[0])
                { // digit must match and be positive
                  // except in the special case ...
                    if (result == int.MinValue) // looks like the special
                        if (ind == isneg) // really was negative
                            if (mant[0] == 2)
                                return result; // really had top digit 2
                    throw new OverflowException("Conversion overflow:" + " " + this.ToString());
                }
            }

            /* Looks good */
            if (ind == ispos)
                return result;
            return -result;
        }

        /**
         * Converts this <code>BigDecimal</code> to a <code>long</code>. If the <code>BigDecimal</code> has a non-zero
         * decimal part it is discarded. If the <code>BigDecimal</code> is out of the possible range for a <code>long</code>
         * (64-bit signed integer) result then only the low-order 64 bits are used. (That is, the number may be
         * <i>decapitated</i>.) To avoid unexpected errors when these conditions occur, use the {@link #longValueExact}
         * method.
         *
         * @return A <code>long</code> converted from <code>this</code>, truncated and decapitated if necessary.
         * @stable ICU 2.0
         */

        public override long ToInt64()
        {
            // ICU4N TODO: In .NET Standard 2.1 and higher there is a TryToInt32() method that returns false if
            // the value won't fit. It should be used as the primary way to convert, using the below as a fallback.

            // Create a BigInteger value
            BigInteger bigInt = ToBigInteger();

            if (bigInt >= long.MinValue && bigInt <= long.MaxValue)
            {
                return (long)bigInt;
            }

            bool negate = bigInt.Sign < 1;

            if (negate)
            {
                bigInt = BigInteger.Abs(bigInt);
            }

            // Convert the BigInteger to a byte array
            byte[] bytes = bigInt.ToByteArray();

            // Truncate all but the low-order 4 bytes
            const int size = sizeof(long);
            if (bytes.Length > size)
            {
                Array.Resize(ref bytes, size); // Decapitate
            }

            // Convert the byte array to an int
            long result = BitConverter.ToInt64(bytes, 0);

            // If the BigInteger value was negative, negate the result
            if (negate)
            {
                result = -result;
            }

            return result;
        }

        /**
         * Converts this <code>BigDecimal</code> to a <code>long</code>. If the <code>BigDecimal</code> has a non-zero
         * decimal part or is out of the possible range for a <code>long</code> (64-bit signed integer) result then an
         * <code>ArithmeticException</code> is thrown.
         *
         * @return A <code>long</code> equal in value to <code>this</code>.
         * @throws ArithmeticException if <code>this</code> has a non-zero decimal part, or will not fit in a <code>long</code>.
         * @stable ICU 2.0
         */

        public long ToInt64Exact()
        {
            int lodigit;
            int cstart = 0;
            int useexp = 0;
            long result;
            int i = 0;
            long topdig = 0;
            // Identical to intValueExact except for result=long, and exp>=20 test
            if (ind == 0)
                return 0; // easy, and quite common
            lodigit = mant.Length - 1; // last included digit
            if (exp < 0)
            {
                lodigit = lodigit + exp; // -(-exp)
                /* all decimal places must be 0 */
                if (lodigit < 0)
                    cstart = 0;
                else
                    cstart = lodigit + 1;
                if ((!(AllZero(mant, cstart))))
                    throw new OverflowException("Decimal part non-zero:" + " " + this.ToString());
                if (lodigit < 0)
                    return 0; // -1<this<1
                useexp = 0;
            }
            else
            {/* >=0 */
                if ((exp + mant.Length) > 18) // early exit
                    throw new OverflowException("Conversion overflow:" + " " + this.ToString());
                useexp = exp;
            }

            /* convert the mantissa to binary, inline for speed */
            // note that we could safely use the 'test for wrap to negative'
            // algorithm here, but instead we parallel the intValueExact
            // algorithm for ease of checking and maintenance.
            result = 0;
            {
                int len17 = lodigit + useexp;
                i = 0;
                for (; i <= len17; i++)
                {
                    result = result * 10;
                    if (i <= lodigit)
                        result = result + mant[i];
                }
            }/* i */

            /* Now, if the risky length, check for overflow */
            if ((lodigit + useexp) == 18)
            {
                topdig = result / 1000000000000000000L; // get top digit, preserving sign
                if (topdig != mant[0])
                { // digit must match and be positive
                  // except in the special case ...
                    if (result == long.MinValue) // looks like the special
                        if (ind == isneg) // really was negative
                            if (mant[0] == 9)
                                return result; // really had top digit 9
                    throw new OverflowException("Conversion overflow:" + " " + this.ToString());
                }
            }

            /* Looks good */
            if (ind == ispos)
                return result;
            return -result;
        }

        /**
         * Returns a plain <code>BigDecimal</code> whose decimal point has been moved to the left by a specified number of
         * positions. The parameter, <code>n</code>, specifies the number of positions to move the decimal point. That is,
         * if <code>n</code> is 0 or positive, the number returned is given by:
         * <p>
         * <code> this.multiply(TEN.pow(new BigDecimal(-n))) </code>
         * <p>
         * <code>n</code> may be negative, in which case the method returns the same result as <code>movePointRight(-n)
         * </code>.
         *
         * @param n The <code>int</code> specifying the number of places to move the decimal point leftwards.
         * @return A <code>BigDecimal</code> derived from <code>this</code>, with the decimal point moved <code>n</code>
         *         places to the left.
         * @stable ICU 2.0
         */

        public BigDecimal MovePointLeft(int n)
        {
            BigDecimal res;
            // very little point in optimizing for shift of 0
            res = Clone(this);
            res.exp = res.exp - n;
            return res.Finish(plainMC, false); // finish sets form and checks exponent
        }

        /**
         * Returns a plain <code>BigDecimal</code> whose decimal point has been moved to the right by a specified number of
         * positions. The parameter, <code>n</code>, specifies the number of positions to move the decimal point. That is,
         * if <code>n</code> is 0 or positive, the number returned is given by:
         * <p>
         * <code> this.multiply(TEN.pow(new BigDecimal(n))) </code>
         * <p>
         * <code>n</code> may be negative, in which case the method returns the same result as <code>movePointLeft(-n)
         * </code>.
         *
         * @param n The <code>int</code> specifying the number of places to move the decimal point rightwards.
         * @return A <code>BigDecimal</code> derived from <code>this</code>, with the decimal point moved <code>n</code>
         *         places to the right.
         * @stable ICU 2.0
         */

        public BigDecimal MovePointRight(int n)
        {
            BigDecimal res;
            res = Clone(this);
            res.exp = res.exp + n;
            return res.Finish(plainMC, false);
        }

        /**
         * Returns the scale of this <code>BigDecimal</code>. Returns a non-negative <code>int</code> which is the scale of
         * the number. The scale is the number of digits in the decimal part of the number if the number were formatted
         * without exponential notation.
         *
         * @return An <code>int</code> whose value is the scale of this <code>BigDecimal</code>.
         * @stable ICU 2.0
         */

        public int Scale
        {
            get
            {
                if (exp >= 0)
                    return 0; // scale can never be negative
                return -exp;
            }
        }

        /**
         * Returns a plain <code>BigDecimal</code> with a given scale.
         * <p>
         * If the given scale (which must be zero or positive) is the same as or greater than the length of the decimal part
         * (the scale) of this <code>BigDecimal</code> then trailing zeros will be added to the decimal part as necessary.
         * <p>
         * If the given scale is less than the length of the decimal part (the scale) of this <code>BigDecimal</code> then
         * trailing digits will be removed, and in this case an <code>ArithmeticException</code> is thrown if any discarded
         * digits are non-zero.
         * <p>
         * The same as {@link #setScale(int, int)}, where the first parameter is the scale, and the second is <code>
         * MathContext.ROUND_UNNECESSARY</code>.
         *
         * @param scale The <code>int</code> specifying the scale of the resulting <code>BigDecimal</code>.
         * @return A plain <code>BigDecimal</code> with the given scale.
         * @throws ArithmeticException if <code>scale</code> is negative.
         * @throws ArithmeticException if reducing scale would discard non-zero digits.
         * @stable ICU 2.0
         */

        public BigDecimal SetScale(int scale)
        {
            return SetScale(scale, RoundUnnecessary);
        }

        /**
         * Returns a plain <code>BigDecimal</code> with a given scale.
         * <p>
         * If the given scale (which must be zero or positive) is the same as or greater than the length of the decimal part
         * (the scale) of this <code>BigDecimal</code> then trailing zeros will be added to the decimal part as necessary.
         * <p>
         * If the given scale is less than the length of the decimal part (the scale) of this <code>BigDecimal</code> then
         * trailing digits will be removed, and the rounding mode given by the second parameter is used to determine if the
         * remaining digits are affected by a carry. In this case, an <code>IllegalArgumentException</code> is thrown if
         * <code>round</code> is not a valid rounding mode.
         * <p>
         * If <code>round</code> is <code>MathContext.ROUND_UNNECESSARY</code>, an <code>ArithmeticException</code> is
         * thrown if any discarded digits are non-zero.
         *
         * @param scale The <code>int</code> specifying the scale of the resulting <code>BigDecimal</code>.
         * @param round The <code>int</code> rounding mode to be used for the division (see the {@link MathContext} class).
         * @return A plain <code>BigDecimal</code> with the given scale.
         * @throws IllegalArgumentException if <code>round</code> is not a valid rounding mode.
         * @throws ArithmeticException if <code>scale</code> is negative.
         * @throws ArithmeticException if <code>round</code> is <code>MathContext.ROUND_UNNECESSARY</code>, and reducing scale would discard
         *             non-zero digits.
         * @stable ICU 2.0
         */

        public BigDecimal SetScale(int scale, RoundingMode round)
        {
            int ourscale;
            BigDecimal res;
            int padding = 0;
            int newlen = 0;
            // at present this naughtily only checks the round value if it is
            // needed (used), for speed
            ourscale = this.Scale;
            if (ourscale == scale) // already correct scale
                if (this.form == (byte)ExponentForm.Plain) // .. and form
                    return this;
            res = Clone(this); // need copy
            if (ourscale <= scale)
            { // simply zero-padding/changing form
              // if ourscale is 0 we may have lots of 0s to add
                if (ourscale == 0)
                    padding = res.exp + scale;
                else
                    padding = scale - ourscale;
                res.mant = Extend(res.mant, res.mant.Length + padding);
                res.exp = -scale; // as requested
            }
            else
            {/* ourscale>scale: shortening, probably */
                if (scale < 0)
                    throw new ArithmeticException("Negative scale:" + " " + scale); // ICU4N TODO: Throw ArgumentOutOfRangeException? Check BigInteger logic.
                // [round() will raise exception if invalid round]
                newlen = res.mant.Length - ((ourscale - scale)); // [<=0 is OK]
                res = res.Round(newlen, round); // round to required length
                                                // This could have shifted left if round (say) 0.9->1[.0]
                                                // Repair if so by adding a zero and reducing exponent
                if (res.exp != -scale)
                {
                    res.mant = Extend(res.mant, res.mant.Length + 1);
                    res.exp = res.exp - 1;
                }
            }
            res.form = (byte)ExponentForm.Plain; // by definition
            return res;
        }

        /**
         * Converts this <code>BigDecimal</code> to a <code>short</code>. If the <code>BigDecimal</code> has a non-zero
         * decimal part or is out of the possible range for a <code>short</code> (16-bit signed integer) result then an
         * <code>ArithmeticException</code> is thrown.
         *
         * @return A <code>short</code> equal in value to <code>this</code>.
         * @throws ArithmeticException if <code>this</code> has a non-zero decimal part, or will not fit in a <code>short</code>.
         * @stable ICU 2.0
         */

        public short ToInt16Exact()
        {
            int num;
            num = this.ToInt32Exact(); // will check decimal part too
            if ((num > 32767) || (num < (-32768)))
                throw new OverflowException("Conversion overflow:" + " " + this.ToString());
            return (short)num;
        }

        /**
         * Returns the sign of this <code>BigDecimal</code>, as an <code>int</code>. This returns the <i>signum</i> function
         * value that represents the sign of this <code>BigDecimal</code>. That is, -1 if the <code>BigDecimal</code> is
         * negative, 0 if it is numerically equal to zero, or 1 if it is positive.
         *
         * @return An <code>int</code> which is -1 if the <code>BigDecimal</code> is negative, 0 if it is numerically equal
         *         to zero, or 1 if it is positive.
         * @stable ICU 2.0
         */

        public int Sign // ICU4N NOTE: This was signum() in ICU4J
        {
            get => this.ind; // [note this assumes values for ind.]
        }

        /**
         * Converts this <code>BigDecimal</code> to a <code>java.math.BigDecimal</code>.
         * <p>
         * This is an exact conversion; the result is the same as if the <code>BigDecimal</code> were formatted as a plain
         * number without any rounding or exponent and then the <code>java.math.BigDecimal(java.lang.String)</code>
         * constructor were used to construct the result.
         * <p>
         * <i>(Note: this method is provided only in the <code>com.ibm.icu.math</code> version of the BigDecimal class. It
         * would not be present in a <code>java.math</code> version.)</i>
         *
         * @return The <code>java.math.BigDecimal</code> equal in value to this <code>BigDecimal</code>.
         * @stable ICU 2.0
         */

        // ICU4N TODO: Complete implementation
        public Deveel.Math.BigDecimal ToBigDecimal()
        {
            return new Deveel.Math.BigDecimal(new Deveel.Math.BigInteger(this.ToUnscaledValue().ToByteArray()), this.Scale);
        }

        /**
         * Converts this <code>BigDecimal</code> to a <code>java.math.BigInteger</code>.
         * <p>
         * Any decimal part is truncated (discarded). If an exception is desired should the decimal part be non-zero, use
         * {@link #toBigIntegerExact()}.
         *
         * @return The <code>java.math.BigInteger</code> equal in value to the integer part of this <code>BigDecimal</code>.
         * @stable ICU 2.0
         */

        public BigInteger ToBigInteger()
        {
            BigDecimal res = null;
            int newlen = 0;
            byte[] newmant = null;
            {/* select */
                if ((exp >= 0) && (form == (byte)ExponentForm.Plain))
                    res = this; // can layout simply
                else if (exp >= 0)
                {
                    res = Clone(this); // safe copy
                    res.form = (byte)ExponentForm.Plain; // .. and request PLAIN
                }
                else
                {
                    { // exp<0; scale to be truncated
                      // we could use divideInteger, but we may as well be quicker
                        if (-this.exp >= this.mant.Length)
                            res = Zero; // all blows away
                        else
                        {
                            res = Clone(this); // safe copy
                            newlen = res.mant.Length + res.exp;
                            newmant = new byte[newlen]; // [shorter]
                            Array.Copy(res.mant, 0, newmant, 0,
                                    newlen);
                            res.mant = newmant;
                            res.form = (byte)ExponentForm.Plain;
                            res.exp = 0;
                        }
                    }
                }
            }
            return BigInteger.Parse(new string(res.GetLayout()), CultureInfo.InvariantCulture);
            //return new BigInteger(new string(res.GetLayout()));
        }

        /**
         * Converts this <code>BigDecimal</code> to a <code>java.math.BigInteger</code>.
         * <p>
         * An exception is thrown if the decimal part (if any) is non-zero.
         *
         * @return The <code>java.math.BigInteger</code> equal in value to the integer part of this <code>BigDecimal</code>.
         * @throws ArithmeticException if <code>this</code> has a non-zero decimal part.
         * @stable ICU 2.0
         */

        public BigInteger ToBigIntegerExact()
        {
            /* test any trailing decimal part */
            if (exp < 0)
            { // possible decimal part
                /* all decimal places must be 0; note exp<0 */
                if ((!(AllZero(mant, mant.Length + exp))))
                    throw new OverflowException("Decimal part non-zero:" + " " + this.ToString());
            }
            return ToBigInteger();
        }

        /**
         * Returns the <code>BigDecimal</code> as a character array. The result of this method is the same as using the
         * sequence <code>toString().toCharArray()</code>, but avoids creating the intermediate <code>String</code> and
         * <code>char[]</code> objects.
         *
         * @return The <code>char[]</code> array corresponding to this <code>BigDecimal</code>.
         * @stable ICU 2.0
         */

        public char[] ToCharArray()
        {
            return GetLayout();
        }

        /**
         * Returns the <code>BigDecimal</code> as a <code>String</code>. This returns a <code>String</code> that exactly
         * represents this <code>BigDecimal</code>, as defined in the decimal documentation (see {@link BigDecimal class
         * header}).
         * <p>
         * By definition, using the {@link #BigDecimal(String)} constructor on the result <code>String</code> will create a
         * <code>BigDecimal</code> that is exactly equal to the original <code>BigDecimal</code>.
         *
         * @return The <code>String</code> exactly corresponding to this <code>BigDecimal</code>.
         * @see #format(int, int)
         * @see #format(int, int, int, int, int, int)
         * @see #toCharArray()
         * @stable ICU 2.0
         */

        public override string ToString()
        {
            return new string(GetLayout());
        }

        // ICU4N specific overload
        public override string ToString(string format, IFormatProvider provider)
        {
            // ICU4N TODO: make culture aware?
            return new string(GetLayout());
        }

        /**
         * Returns the number as a <code>BigInteger</code> after removing the scale. That is, the number is expressed as a
         * plain number, any decimal point is then removed (retaining the digits of any decimal part), and the result is
         * then converted to a <code>BigInteger</code>.
         *
         * @return The <code>java.math.BigInteger</code> equal in value to this <code>BigDecimal</code> multiplied by ten to
         *         the power of <code>this.scale()</code>.
         * @stable ICU 2.0
         */

        public BigInteger ToUnscaledValue()
        {
            BigDecimal res = null;
            if (exp >= 0)
                res = this;
            else
            {
                res = Clone(this); // safe copy
                res.exp = 0; // drop scale
            }
            return res.ToBigInteger();
        }

        /**
         * Translates a <code>double</code> to a <code>BigDecimal</code>.
         * <p>
         * Returns a <code>BigDecimal</code> which is the decimal representation of the 64-bit signed binary floating point
         * parameter. If the parameter is infinite, or is not a number (NaN), a <code>NumberFormatException</code> is
         * thrown.
         * <p>
         * The number is constructed as though <code>num</code> had been converted to a <code>String</code> using the <code>
         * Double.toString()</code> method and the {@link #BigDecimal(java.lang.String)} constructor had then been used.
         * This is typically not an exact conversion.
         *
         * @param dub The <code>double</code> to be translated.
         * @return The <code>BigDecimal</code> equal in value to <code>dub</code>.
         * @throws NumberFormatException if the parameter is infinite or not a number.
         * @stable ICU 2.0
         */

        public static BigDecimal GetInstance(double dub)
        {
            // Reminder: a zero double returns '0.0', so we cannot fastpath to
            // use the constant ZERO. This might be important enough to justify
            // a factory approach, a cache, or a few private constants, later.
            return new BigDecimal(Double.ToString(dub, CultureInfo.InvariantCulture));
        }

        /**
         * Translates a <code>long</code> to a <code>BigDecimal</code>. That is, returns a plain <code>BigDecimal</code>
         * whose value is equal to the given <code>long</code>.
         *
         * @param lint The <code>long</code> to be translated.
         * @return The <code>BigDecimal</code> equal in value to <code>lint</code>.
         * @stable ICU 2.0
         */

        public static BigDecimal GetInstance(long lint)
        {
            return GetInstance(lint, 0);
        }

        /**
         * Translates a <code>long</code> to a <code>BigDecimal</code> with a given scale. That is, returns a plain <code>
         * BigDecimal</code> whose unscaled value is equal to the given <code>long</code>, adjusted by the second parameter,
         * <code>scale</code>.
         * <p>
         * The result is given by:
         * <p>
         * <code> (new BigDecimal(lint)).divide(TEN.pow(new BigDecimal(scale))) </code>
         * <p>
         * A <code>NumberFormatException</code> is thrown if <code>scale</code> is negative.
         *
         * @param lint The <code>long</code> to be translated.
         * @param scale The <code>int</code> scale to be applied.
         * @return The <code>BigDecimal</code> equal in value to <code>lint</code>.
         * @throws NumberFormatException if the scale is negative.
         * @stable ICU 2.0
         */

        public static BigDecimal GetInstance(long lint, int scale)
        {
            BigDecimal res = null;
            {/* select */
                if (lint == 0)
                    res = Zero;
                else if (lint == 1)
                    res = One;
                else if (lint == 10)
                    res = Ten;
                else
                {
                    res = new BigDecimal(lint);
                }
            }
            if (scale == 0)
                return res;
            if (scale < 0)
                throw new ArgumentOutOfRangeException(nameof(scale), "Negative scale:" + " " + scale);
            res = Clone(res); // safe copy [do not mutate]
            res.exp = -scale; // exponent is -scale
            return res;
        }

        /* ---------------------------------------------------------------- */
        /* Private methods */
        /* ---------------------------------------------------------------- */

        /*
         * <sgml> Return char array value of a BigDecimal (conversion from BigDecimal to laid-out canonical char array).
         * <p>The mantissa will either already have been rounded (following an operation) or will be of length appropriate
         * (in the case of construction from an int, for example). <p>We must not alter the mantissa, here. <p>'form'
         * describes whether we are to use exponential notation (and if so, which), or if we are to lay out as a plain/pure
         * numeric. </sgml>
         */

        private char[] GetLayout()
        {
            char[] cmant;
            int i = 0;
            StringBuilder sb = null;
            int euse = 0;
            int sig = 0;
            char csign = (char)0;
            char[] rec = null;
            int needsign;
            int mag;
            int len = 0;
            cmant = new char[mant.Length]; // copy byte[] to a char[]
            {
                int len18 = mant.Length;
                i = 0;
                for (; len18 > 0; len18--, i++)
                {
                    cmant[i] = (char)(mant[i] + '0');
                }
            }/* i */

            if (form != (byte)ExponentForm.Plain)
            {/* exponential notation needed */
                sb = new StringBuilder(cmant.Length + 15); // -x.xxxE+999999999
                if (ind == isneg)
                    sb.Append('-');
                euse = (exp + cmant.Length) - 1; // exponent to use
                /* setup sig=significant digits and copy to result */
                if (form == (byte)ExponentForm.Scientific)
                { // [default]
                    sb.Append(cmant[0]); // significant character
                    if (cmant.Length > 1) // have decimal part
                        sb.Append('.').Append(cmant, 1, cmant.Length - 1);
                }
                else
                {
                    do
                    {
                        sig = euse % 3; // common
                        if (sig < 0)
                            sig = 3 + sig; // negative exponent
                        euse = euse - sig;
                        sig++;
                        if (sig >= cmant.Length)
                        { // zero padding may be needed
                            sb.Append(cmant, 0, cmant.Length);
                            {
                                int len19 = sig - cmant.Length;
                                for (; len19 > 0; len19--)
                                {
                                    sb.Append('0');
                                }
                            }
                        }
                        else
                        { // decimal point needed
                            sb.Append(cmant, 0, sig).Append('.').Append(cmant, sig, cmant.Length - sig);
                        }
                    } while (false);
                }/* engineering */
                if (euse != 0)
                {
                    if (euse < 0)
                    {
                        csign = '-';
                        euse = -euse;
                    }
                    else
                        csign = '+';
                    sb.Append('E').Append(csign).Append(euse);
                }
                rec = new char[sb.Length];
                int srcEnd = sb.Length;
                if (0 != srcEnd)
                {
                    sb.CopyTo(0, rec, 0, srcEnd);
                    //sb.getChars(0, srcEnd, rec, 0);
                }
                return rec;
            }

            /* Here for non-exponential (plain) notation */
            if (exp == 0)
            {/* easy */
                if (ind >= 0)
                    return cmant; // non-negative integer
                rec = new char[cmant.Length + 1];
                rec[0] = '-';
                Array.Copy(cmant, 0, rec, 1, cmant.Length);
                return rec;
            }

            /* Need a '.' and/or some zeros */
            needsign = (ind == isneg) ? 1 : 0; // space for sign? 0 or 1

            /*
             * MAG is the position of the point in the mantissa (index of the character it follows)
             */
            mag = exp + cmant.Length;

            if (mag < 1)
            {/* 0.00xxxx form */
                len = (needsign + 2) - exp; // needsign+2+(-mag)+cmant.length
                rec = new char[len];
                if (needsign != 0)
                    rec[0] = '-';
                rec[needsign] = '0';
                rec[needsign + 1] = '.';
                {
                    int len20 = -mag;
                    i = needsign + 2;
                    for (; len20 > 0; len20--, i++)
                    { // maybe none
                        rec[i] = '0';
                    }
                }/* i */
                Array.Copy(cmant, 0, rec, (needsign + 2) - mag,
                        cmant.Length);
                return rec;
            }

            if (mag > cmant.Length)
            {/* xxxx0000 form */
                len = needsign + mag;
                rec = new char[len];
                if (needsign != 0)
                    rec[0] = '-';
                Array.Copy(cmant, 0, rec, needsign, cmant.Length);
                {
                    int len21 = mag - cmant.Length;
                    i = needsign + cmant.Length;
                    for (; len21 > 0; len21--, i++)
                    { // never 0
                        rec[i] = '0';
                    }
                }/* i */
                return rec;
            }

            /* decimal point is in the middle of the mantissa */
            len = (needsign + 1) + cmant.Length;
            rec = new char[len];
            if (needsign != 0)
                rec[0] = '-';
            Array.Copy(cmant, 0, rec, needsign, mag);
            rec[needsign + mag] = '.';
            Array.Copy(cmant, mag, rec, (needsign + mag) + 1,
                    cmant.Length - mag);
            return rec;
        }

        /*
         * <sgml> Checks a BigDecimal argument to ensure it's a true integer in a given range. <p>If OK, returns it as an
         * int. </sgml>
         */
        // [currently only used by pow]
        private int IntCheck(int min, int max)
        {
            int i;
            i = this.ToInt32Exact(); // [checks for non-0 decimal part]
                                     // Use same message as though intValueExact failed due to size
            if ((i < min) || (i > max))
                throw new OverflowException("Conversion overflow:" + " " + i);
            return i;
        }

        /* <sgml> Carry out division operations. </sgml> */
        /*
         * Arg1 is operation code: D=divide, I=integer divide, R=remainder Arg2 is the rhs. Arg3 is the context. Arg4 is
         * explicit scale iff code='D' or 'I' (-1 if none).
         *
         * Underlying algorithm (complications for Remainder function and scaled division are omitted for clarity):
         *
         * Test for x/0 and then 0/x Exp =Exp1 - Exp2 Exp =Exp +len(var1) -len(var2) Sign=Sign1 Sign2 Pad accumulator (Var1)
         * to double-length with 0's (pad1) Pad Var2 to same length as Var1 B2B=1st two digits of var2, +1 to allow for
         * roundup have=0 Do until (have=digits+1 OR residue=0) if exp<0 then if integer divide/residue then leave
         * this_digit=0 Do forever compare numbers if <0 then leave inner_loop if =0 then (- quick exit without subtract -)
         * do this_digit=this_digit+1; output this_digit leave outer_loop; end Compare lengths of numbers (mantissae): If
         * same then CA=first_digit_of_Var1 else CA=first_two_digits_of_Var1 mult=ca10/b2b -- Good and safe guess at divisor
         * if mult=0 then mult=1 this_digit=this_digit+mult subtract end inner_loop if have\=0 | this_digit\=0 then do
         * output this_digit have=have+1; end var2=var2/10 exp=exp-1 end outer_loop exp=exp+1 -- set the proper exponent if
         * have=0 then generate answer=0 Return to FINISHED Result defined by MATHV1
         *
         * For extended commentary, see DMSRCN.
         */

        private BigDecimal DoDivide(char code, BigDecimal rhs,
                MathContext set, int scale)
        {
            // ICU4N: added guard clauses
            if (rhs is null)
                throw new ArgumentNullException(nameof(rhs));
            if (set is null)
                throw new ArgumentNullException(nameof(set));

            BigDecimal lhs;
            int reqdig;
            int newexp;
            BigDecimal res;
            int newlen;
            byte[] var1;
            int var1len;
            byte[] var2;
            int var2len;
            int b2b;
            int have;
            int thisdigit = 0;
            int i = 0;
            byte v2 = 0;
            int ba = 0;
            int mult = 0;
            int start = 0;
            int padding = 0;
            int d = 0;
            byte[] newvar1 = null;
            byte lasthave = 0;
            int actdig = 0;
            byte[] newmant = null;

            if (set.LostDigits)
                CheckDigits(rhs, set.Digits);
            lhs = this; // name for clarity

            // [note we must have checked lostDigits before the following checks]
            if (rhs.ind == 0)
                //throw new ArithmeticException("Divide by 0"); // includes 0/0
                throw new DivideByZeroException("Divide by 0"); // includes 0/0
            if (lhs.ind == 0)
            { // 0/x => 0 [possibly with .0s]
                if (set.Form != ExponentForm.Plain)
                    return Zero;
                if (scale == (-1))
                    return lhs;
                return lhs.SetScale(scale);
            }

            /* Prepare numbers according to BigDecimal rules */
            reqdig = set.Digits; // local copy (heavily used)
            if (reqdig > 0)
            {
                if (lhs.mant.Length > reqdig)
                    lhs = Clone(lhs).Round(set);
                if (rhs.mant.Length > reqdig)
                    rhs = Clone(rhs).Round(set);
            }
            else
            {/* scaled divide */
                if (scale == (-1))
                    scale = lhs.Scale;
                // set reqdig to be at least large enough for the computation
                reqdig = lhs.mant.Length; // base length
                                          // next line handles both positive lhs.exp and also scale mismatch
                if (scale != -lhs.exp)
                    reqdig = (reqdig + scale) + lhs.exp;
                reqdig = (reqdig - ((rhs.mant.Length - 1))) - rhs.exp; // reduce by RHS effect
                if (reqdig < lhs.mant.Length)
                    reqdig = lhs.mant.Length; // clamp
                if (reqdig < rhs.mant.Length)
                    reqdig = rhs.mant.Length; // ..
            }

            /* precalculate exponent */
            newexp = ((lhs.exp - rhs.exp) + lhs.mant.Length) - rhs.mant.Length;
            /* If new exponent -ve, then some quick exits are possible */
            if (newexp < 0)
                if (code != 'D')
                {
                    if (code == 'I')
                        return Zero; // easy - no integer part
                    /* Must be 'R'; remainder is [finished clone of] input value */
                    return Clone(lhs).Finish(set, false);
                }

            /* We need slow division */
            res = new BigDecimal(); // where we'll build result
            res.ind = (sbyte)(lhs.ind * rhs.ind); // final sign (for D/I)
            res.exp = newexp; // initial exponent (for D/I)
            res.mant = new byte[reqdig + 1]; // where build the result

            /* Now [virtually pad the mantissae with trailing zeros */
            // Also copy the LHS, which will be our working array
            newlen = (reqdig + reqdig) + 1;
            var1 = Extend(lhs.mant, newlen); // always makes longer, so new safe array
            var1len = newlen; // [remaining digits are 0]

            var2 = rhs.mant;
            var2len = newlen;

            /* Calculate first two digits of rhs (var2), +1 for later estimations */
            b2b = (var2[0] * 10) + 1;
            if (var2.Length > 1)
                b2b = b2b + var2[1];

            /* start the long-division loops */
            have = 0;
            {
                /*outer:*/
                for (; ; )
                {
                    thisdigit = 0;
                    /* find the next digit */
                    {
                        /*inner:*/
                        for (; ; )
                        {
                            if (var1len < var2len)
                                break; // inner; // V1 too low
                            if (var1len == var2len)
                            { // compare needed
                                {
                                    /*compare:*/
                                    do
                                    { // comparison
                                        {
                                            int len22 = var1len;
                                            i = 0;
                                            for (; len22 > 0; len22--, i++)
                                            {
                                                // var1len is always <= var1.length
                                                if (i < var2.Length)
                                                    v2 = var2[i];
                                                else
                                                    v2 = (byte)0;
                                                if (var1[i] < v2)
                                                    //break; // inner; // V1 too low
                                                    goto inner_break; // V1 too low
                                                if (var1[i] > v2)
                                                    //break; // compare; // OK to subtract
                                                    goto compare_break; // OK to subtract
                                            }
                                        }/* i */
                                        /*
                                         * reach here if lhs and rhs are identical; subtraction will increase digit by one,
                                         * and the residue will be 0 so we are done; leave the loop with residue set to 0
                                         * (in case code is 'R' or ROUND_UNNECESSARY or a ROUND_HALF_xxxx is being checked)
                                         */
                                        thisdigit++;
                                        res.mant[have] = (byte)thisdigit;
                                        have++;
                                        var1[0] = (byte)0; // residue to 0 [this is all we'll test]
                                                           // var1len=1 -- [optimized out]
                                        goto outer_break;
                                    compare_continue: { /* Intentionally blank */ }
                                    } while (false);
                                compare_break: { /* Intentionally blank */ }
                                }/* compare */
                            
                                /* prepare for subtraction. Estimate BA (lengths the same) */
                                ba = var1[0]; // use only first digit
                            } // lengths the same
                            else
                            {/* lhs longer than rhs */
                                /* use first two digits for estimate */
                                ba = var1[0] * 10;
                                if (var1len > 1)
                                    ba = ba + var1[1];
                            }
                            /* subtraction needed; V1>=V2 */
                            mult = (ba * 10) / b2b;
                            if (mult == 0)
                                mult = 1;
                            thisdigit = thisdigit + mult;
                            // subtract; var1 reusable
                            var1 = ByteAddSub(var1, var1len, var2, var2len, -mult, true);
                            if (var1[0] != 0)
                                goto inner_continue; // maybe another subtract needed
                            /*
                             * V1 now probably has leading zeros, remove leading 0's and try again. (It could be longer than
                             * V2)
                             */
                            {
                                int len23 = var1len - 2;
                                start = 0;
                                /*start:*/
                                for (; start <= len23; start++)
                                {
                                    if (var1[start] != 0)
                                        break; // start;
                                    var1len--;
                                }
                            }/* start */
                            if (start == 0)
                                continue; // inner;
                                          // shift left
                            Array.Copy(var1, start, var1, 0, var1len);
                        inner_continue: { /* Intentionally blank */ }
                        }
                    inner_break: { /* Intentionally blank */ }
                    }/* inner */
                

                    /* We have the next digit */
                    if ((have != 0) || (thisdigit != 0))
                    { // put the digit we got
                        res.mant[have] = (byte)thisdigit;
                        have++;
                        if (have == (reqdig + 1))
                            //break; // outer; // we have all we need
                            goto outer_break; // we have all we need 
                        if (var1[0] == 0)
                            //break; // outer; // residue now 0
                            goto outer_break; // residue now 0
                    }
                    /* can leave now if a scaled divide and exponent is small enough */
                    if (scale >= 0)
                        if (-res.exp > scale)
                            //break; // outer;
                            goto outer_break;
                    /* can leave now if not Divide and no integer part left */
                    if (code != 'D')
                        if (res.exp <= 0)
                            //break; // outer;
                            goto outer_break;
                    res.exp = res.exp - 1; // reduce the exponent
                    /*
                     * to get here, V1 is less than V2, so divide V2 by 10 and go for the next digit
                     */
                    var2len--;
                outer_continue: { /* Intentionally blank */ }
                }
            outer_break: { /* Intentionally blank */ }
            }/* outer */
        

            /* here when we have finished dividing, for some reason */
            // have is the number of digits we collected in res.mant
            if (have == 0)
                have = 1; // res.mant[0] is 0; we always want a digit

            if ((code == 'I') || (code == 'R'))
            {/* check for integer overflow needed */
                if ((have + res.exp) > reqdig)
                    throw new OverflowException("Integer overflow");

                if (code == 'R')
                {
                    do
                    {
                        /* We were doing Remainder -- return the residue */
                        if (res.mant[0] == 0) // no integer part was found
                            return Clone(lhs).Finish(set, false); // .. so return lhs, canonical
                        if (var1[0] == 0)
                            return Zero; // simple 0 residue
                        res.ind = lhs.ind; // sign is always as LHS
                        /*
                         * Calculate the exponent by subtracting the number of padding zeros we added and adding the
                         * original exponent
                         */
                        padding = ((reqdig + reqdig) + 1) - lhs.mant.Length;
                        res.exp = (res.exp - padding) + lhs.exp;

                        /*
                         * strip insignificant padding zeros from residue, and create/copy the resulting mantissa if need be
                         */
                        d = var1len;
                        {
                            i = d - 1;
                            /*i:*/
                            for (; i >= 1; i--)
                            {
                                if (!((res.exp < lhs.exp) & (res.exp < rhs.exp)))
                                    break;
                                if (var1[i] != 0)
                                    break; // i;
                                d--;
                                res.exp = res.exp + 1;
                            }
                        }/* i */
                        if (d < var1.Length)
                        {/* need to reduce */
                            newvar1 = new byte[d];
                            Array.Copy(var1, 0, newvar1, 0, d); // shorten
                            var1 = newvar1;
                        }
                        res.mant = var1;
                        return res.Finish(set, false);
                    } while (false);
                }/* remainder */
            }

            else
            {/* 'D' -- no overflow check needed */
                // If there was a residue then bump the final digit (iff 0 or 5)
                // so that the residue is visible for ROUND_UP, ROUND_HALF_xxx and
                // ROUND_UNNECESSARY checks (etc.) later.
                // [if we finished early, the residue will be 0]
                if (var1[0] != 0)
                { // residue not 0
                    lasthave = res.mant[have - 1];
                    if (lasthave % 5 == 0)
                        res.mant[have - 1] = (byte)(lasthave + 1);
                }
            }

            /* Here for Divide or Integer Divide */
            // handle scaled results first ['I' always scale 0, optional for 'D']
            if (scale >= 0)
            {
                do
                {
                    // say 'scale have res.exp len' scale have res.exp res.mant.length
                    if (have != res.mant.Length)
                        // already padded with 0's, so just adjust exponent
                        res.exp = res.exp - (res.mant.Length - have);
                    // calculate number of digits we really want [may be 0]
                    actdig = res.mant.Length - (-res.exp - scale);
                    res.Round(actdig, set.RoundingMode); // round to desired length
                                                         // This could have shifted left if round (say) 0.9->1[.0]
                                                         // Repair if so by adding a zero and reducing exponent
                    if (res.exp != -scale)
                    {
                        res.mant = Extend(res.mant, res.mant.Length + 1);
                        res.exp = res.exp - 1;
                    }
                    return res.Finish(set, true); // [strip if not PLAIN]
                } while (false);
            }/* scaled */

            // reach here only if a non-scaled
            if (have == res.mant.Length)
            { // got digits+1 digits
                res.Round(set);
                have = reqdig;
            }
            else
            {/* have<=reqdig */
                if (res.mant[0] == 0)
                    return Zero; // fastpath
                                 // make the mantissa truly just 'have' long
                                 // [we could let finish do this, during strip, if we adjusted
                                 // the exponent; however, truncation avoids the strip loop]
                newmant = new byte[have]; // shorten
                Array.Copy(res.mant, 0, newmant, 0, have);
                res.mant = newmant;
            }
            return res.Finish(set, true);
        }

        /* <sgml> Report a conversion exception. </sgml> */

        private void Bad(char[] s)
        {
            throw new FormatException("Not a number:" + " " + new string(s)); // ICU4N TODO: This was originally NumberFormatException - verify this works
        }

        /*
         * <sgml> Report a bad argument to a method. </sgml> Arg1 is method name Arg2 is argument position Arg3 is what was
         * found
         */

        private void BadArg(string name, int pos, string value)
        {
            throw new ArgumentOutOfRangeException("Bad argument" + " " + pos + " " + "to" + " " + name + ":" + " "
                    + value);
        }

        /*
         * <sgml> Extend byte array to given length, padding with 0s. If no extension is required then return the same
         * array. </sgml>
         *
         * Arg1 is the source byte array Arg2 is the new length (longer)
         */

        private static byte[] Extend(byte[] inarr, int newlen)
        {
            byte[] newarr;
            if (inarr.Length == newlen)
                return inarr;
            newarr = new byte[newlen];
            Array.Copy(inarr, 0, newarr, 0, inarr.Length);
            // 0 padding is carried out by the JVM on allocation initialization
            return newarr;
        }

        /*
         * <sgml> Add or subtract two >=0 integers in byte arrays <p>This routine performs the calculation: <pre> C=A+(BM)
         * </pre> Where M is in the range -9 through +9 <p> If M<0 then A>=B must be true, so the result is always
         * non-negative.
         *
         * Leading zeros are not removed after a subtraction. The result is either the same length as the longer of A and B,
         * or 1 longer than that (if a carry occurred).
         *
         * A is not altered unless Arg6 is 1. B is never altered.
         *
         * Arg1 is A Arg2 is A length to use (if longer than A, pad with 0's) Arg3 is B Arg4 is B length to use (if longer
         * than B, pad with 0's) Arg5 is M, the multiplier Arg6 is 1 if A can be used to build the result (if it fits)
         *
         * This routine is severely performance-critical;any change here must be measured (timed) to assure no performance
         * degradation.
         */
        // 1996.02.20 -- enhanced version of DMSRCN algorithm (1981)
        // 1997.10.05 -- changed to byte arrays (from char arrays)
        // 1998.07.01 -- changed to allow destructive reuse of LHS
        // 1998.07.01 -- changed to allow virtual lengths for the arrays
        // 1998.12.29 -- use lookaside for digit/carry calculation
        // 1999.08.07 -- avoid multiply when mult=1, and make db an int
        // 1999.12.22 -- special case m=-1, also drop 0 special case
        private static byte[] ByteAddSub(byte[] a, int avlen, byte[] b, int bvlen, int m, bool reuse)
        {
            int alength;
            int blength;
            int ap;
            int bp;
            int maxarr;
            byte[] reb;
            bool quickm;
            int digit;
            int op = 0;
            int dp90 = 0;
            byte[] newarr;
            int i = 0;

            // We'll usually be right if we assume no carry
            alength = a.Length; // physical lengths
            blength = b.Length; // ..
            ap = avlen - 1; // -> final (rightmost) digit
            bp = bvlen - 1; // ..
            maxarr = bp;
            if (maxarr < ap)
                maxarr = ap;
            reb = null; // result byte array
            if (reuse)
                if ((maxarr + 1) == alength)
                    reb = a; // OK to reuse A
            if (reb == null)
                reb = new byte[maxarr + 1]; // need new array

            quickm = false; // 1 if no multiply needed
            if (m == 1)
                quickm = true; // most common
            else if (m == (-1))
                quickm = true; // also common

            digit = 0; // digit, with carry or borrow
            {
                op = maxarr;
                /*op:*/
                for (; op >= 0; op--)
                {
                    if (ap >= 0)
                    {
                        if (ap < alength)
                            digit = digit + (sbyte)a[ap]; // within A
                        ap--;
                    }
                    if (bp >= 0)
                    {
                        if (bp < blength)
                        { // within B
                            if (quickm)
                            {
                                if (m > 0)
                                    digit = digit + (sbyte)b[bp]; // most common
                                else
                                    digit = digit - (sbyte)b[bp]; // also common
                            }
                            else
                                digit = digit + ((sbyte)b[bp] * m);
                        }
                        bp--;
                    }
                    /* result so far (digit) could be -90 through 99 */
                    if (digit < 10)
                        if (digit >= 0)
                        {
                            do
                            { // 0-9
                                reb[op] = (byte)digit;
                                digit = 0; // no carry
                                //continue; // op;
                                goto op_continue;
                            } while (false);
                        }/* quick */
                    dp90 = digit + 90;
                    reb[op] = bytedig[dp90]; // this digit
                    digit = bytecar[dp90]; // carry or borrow
                    op_continue: { /* Intentionally blank */ }
                }
            op_break: { /* Intentionally blank */ }
            }/* op */

            if (digit == 0)
                return reb; // no carry
                            // following line will become an Assert, later
                            // if digit<0 then signal ArithmeticException("internal.error ["digit"]")

            /* We have carry -- need to make space for the extra digit */
            newarr = null;
            if (reuse)
                if ((maxarr + 2) == a.Length)
                    newarr = a; // OK to reuse A
            if (newarr == null)
                newarr = new byte[maxarr + 2];
            newarr[0] = (byte)digit; // the carried digit ..
                                     // .. and all the rest [use local loop for short numbers]
            if (maxarr < 10)
            {
                int len24 = maxarr + 1;
                i = 0;
                for (; len24 > 0; len24--, i++)
                {
                    newarr[i + 1] = reb[i];
                }
            }/* i */
            else
                Array.Copy(reb, 0, newarr, 1, maxarr + 1);
            return newarr;
        }

        /*
         * <sgml> Initializer for digit array properties (lookaside). </sgml> Returns the digit array, and initializes the
         * carry array.
         */

        private static byte[] DigInit()
        {
            byte[] work;
            int op = 0;
            int digit = 0;
            work = new byte[(90 + 99) + 1];
            {
                op = 0;
                /*op:*/
                for (; op <= (90 + 99); op++)
                {
                    digit = op - 90;
                    if (digit >= 0)
                    {
                        work[op] = (byte)(digit % 10);
                        bytecar[op] = (sbyte)(digit / 10); // calculate carry
                        continue; // op;
                    }
                    // borrowing...
                    digit = digit + 100; // yes, this is right [consider -50]
                    work[op] = (byte)(digit % 10);
                    bytecar[op] = (sbyte)((digit / 10) - 10); // calculate borrow [NB: - after %]
                }
            }/* op */
            return work;
        }

        /*
         * <sgml> Create a copy of BigDecimal object for local use. <p>This does NOT make a copy of the mantissa array.
         * </sgml> Arg1 is the BigDecimal to clone (non-null)
         */

        private static BigDecimal Clone(BigDecimal dec)
        {
            return new BigDecimal
            {
                ind = dec.ind, // ICU4N TODO: Clone this?
                exp = dec.exp,
                form = dec.form,
                mant = dec.mant // ICU4N TODO: Clone this?
            };
        }

        /*
         * <sgml> Check one or two numbers for lost digits. </sgml> Arg1 is RHS (or null, if none) Arg2 is current DIGITS
         * setting returns quietly or throws an exception
         */

        private void CheckDigits(BigDecimal rhs, int dig)
        {
            if (dig == 0)
                return; // don't check if digits=0
                        // first check lhs...
            if (this.mant.Length > dig)
                if ((!(AllZero(this.mant, dig))))
                    throw new OverflowException("Too many digits:" + " " + this.ToString());
            if (rhs == null)
                return; // monadic
            if (rhs.mant.Length > dig)
                if ((!(AllZero(rhs.mant, dig))))
                    throw new OverflowException("Too many digits:" + " " + rhs.ToString());
        }

        /*
         * <sgml> Round to specified digits, if necessary. </sgml> Arg1 is requested MathContext [with length and rounding
         * mode] returns this, for convenience
         */

        private BigDecimal Round(MathContext set)
        {
            // ICU4N: added guard clause
            if (set is null)
                throw new ArgumentNullException(nameof(set));

            return Round(set.Digits, set.RoundingMode);
        }

        /*
         * <sgml> Round to specified digits, if necessary. Arg1 is requested length (digits to round to) [may be <=0 when
         * called from format, dodivide, etc.] Arg2 is rounding mode returns this, for convenience
         *
         * ind and exp are adjusted, but not cleared for a mantissa of zero
         *
         * The length of the mantissa returned will be Arg1, except when Arg1 is 0, in which case the returned mantissa
         * length will be 1. </sgml>
         */

        private BigDecimal Round(int len, RoundingMode mode)
        {
            int adjust;
            int sign;
            byte[] oldmant;
            bool reuse = false;
            byte first = 0;
            int increment;
            byte[] newmant = null;
            adjust = mant.Length - len;
            if (adjust <= 0)
                return this; // nowt to do

            exp = exp + adjust; // exponent of result
            sign = ind; // save [assumes -1, 0, 1]
            oldmant = mant; // save
            if (len > 0)
            {
                // remove the unwanted digits
                mant = new byte[len];
                Array.Copy(oldmant, 0, mant, 0, len);
                reuse = true; // can reuse mantissa
                first = oldmant[len]; // first of discarded digits
            }
            else
            {/* len<=0 */
                mant = Zero.mant;
                ind = iszero;
                reuse = false; // cannot reuse mantissa
                if (len == 0)
                    first = oldmant[0];
                else
                    first = (byte)0; // [virtual digit]
            }

            // decide rounding adjustment depending on mode, sign, and discarded digits
            increment = 0; // bumper
            {
                do
                {/* select */
                    if (mode == RoundHalfUp)
                    { // default first [most common]
                        if (first >= 5)
                            increment = sign;
                    }
                    else if (mode == RoundUnnecessary)
                    { // default for setScale()
                      // discarding any non-zero digits is an error
                        if ((!(AllZero(oldmant, len))))
                            throw new OverflowException("Rounding necessary");
                    }
                    else if (mode == RoundingMode.HalfDown)
                    { // 0.5000 goes down
                        if (first > 5)
                            increment = sign;
                        else if (first == 5)
                            if ((!(AllZero(oldmant, len + 1))))
                                increment = sign;
                    }
                    else if (mode == RoundHalfEven)
                    { // 0.5000 goes down if left digit even
                        if (first > 5)
                            increment = sign;
                        else if (first == 5)
                        {
                            if ((!(AllZero(oldmant, len + 1))))
                                increment = sign;
                            else /* 0.5000 */
                            if ((((mant[mant.Length - 1]) % 2)) != 0)
                                increment = sign;
                        }
                    }
                    else if (mode == RoundToZero)
                    {
                        // never increment
                    }
                    else if (mode == RoundAwayFromZero)
                    { // increment if discarded non-zero
                        if ((!(AllZero(oldmant, len))))
                            increment = sign;
                    }
                    else if (mode == RoundToPositiveInfinity)
                    { // more positive
                        if (sign > 0)
                            if ((!(AllZero(oldmant, len))))
                                increment = sign;
                    }
                    else if (mode == RoundToNegativeInfinity)
                    { // more negative
                        if (sign < 0)
                            if ((!(AllZero(oldmant, len))))
                                increment = sign;
                    }
                    else
                    {
                        throw new ArgumentException("Bad round value:" + " " + mode);
                    }
                } while (false);
            }/* modes */

            if (increment != 0)
            {
                do
                {
                    if (ind == iszero)
                    {
                        // we must not subtract from 0, but result is trivial anyway
                        mant = One.mant;
                        ind = (sbyte)increment;
                    }
                    else
                    {
                        // mantissa is non-0; we can safely add or subtract 1
                        if (ind == isneg)
                            increment = -increment;
                        newmant = ByteAddSub(mant, mant.Length, One.mant, 1, increment, reuse);
                        if (newmant.Length > mant.Length)
                        { // had a carry
                          // drop rightmost digit and raise exponent
                            exp++;
                            // mant is already the correct length
                            Array.Copy(newmant, 0, mant, 0,
                                    mant.Length);
                        }
                        else
                            mant = newmant;
                    }
                } while (false);
            }/* bump */
            // rounding can increase exponent significantly
            if (exp > MaxExp)
                throw new OverflowException("Exponent Overflow:" + " " + exp);
            return this;
        }

        /*
         * <sgml> Test if rightmost digits are all 0. Arg1 is a mantissa array to test Arg2 is the offset of first digit to
         * check [may be negative; if so, digits to left are 0's] returns 1 if all the digits starting at Arg2 are 0
         *
         * Arg2 may be beyond array bounds, in which case 1 is returned </sgml>
         */

        private static bool AllZero(byte[] array, int start)
        {
            int i = 0;
            if (start < 0)
                start = 0;
            {
                int len25 = array.Length - 1;
                i = start;
                for (; i <= len25; i++)
                {
                    if (array[i] != 0)
                        return false;
                }
            }/* i */
            return true;
        }

        /*
         * <sgml> Carry out final checks and canonicalization <p> This finishes off the current number by: 1. Rounding if
         * necessary (NB: length includes leading zeros) 2. Stripping trailing zeros (if requested and \PLAIN) 3. Stripping
         * leading zeros (always) 4. Selecting exponential notation (if required) 5. Converting a zero result to just '0'
         * (if \PLAIN) In practice, these operations overlap and share code. It always sets form. </sgml> Arg1 is requested
         * MathContext (length to round to, trigger, and FORM) Arg2 is 1 if trailing insignificant zeros should be removed
         * after round (for division, etc.), provided that set.form isn't PLAIN. returns this, for convenience
         */

        private BigDecimal Finish(MathContext set, bool strip)
        {
            int d = 0;
            int i = 0;
            byte[] newmant = null;
            int mag = 0;
            int sig = 0;
            /* Round if mantissa too long and digits requested */
            if (set.Digits != 0)
                if (this.mant.Length > set.Digits)
                    this.Round(set);

            /*
             * If strip requested (and standard formatting), remove insignificant trailing zeros.
             */
            if (strip)
                if (set.Form != ExponentForm.Plain)
                {
                    d = this.mant.Length;
                    /* see if we need to drop any trailing zeros */
                    {
                        i = d - 1;
                        /*i:*/
                        for (; i >= 1; i--)
                        {
                            if (this.mant[i] != 0)
                                break;// i;
                            d--;
                            exp++;
                        }
                    }/* i */
                    if (d < this.mant.Length)
                    {/* need to reduce */
                        newmant = new byte[d];
                        Array.Copy(this.mant, 0, newmant, 0, d);
                        this.mant = newmant;
                    }
                }

            form = (byte)ExponentForm.Plain; // preset

            /* Now check for leading- and all- zeros in mantissa */
            {
                int len26 = this.mant.Length;
                i = 0;
                for (; len26 > 0; len26--, i++)
                {
                    if (this.mant[i] != 0)
                    {
                        // non-0 result; ind will be correct
                        // remove leading zeros [e.g., after subtract]
                        if (i > 0)
                        {
                            do
                            {
                                newmant = new byte[this.mant.Length - i];
                                Array.Copy(this.mant, i, newmant, 0,
                                        this.mant.Length - i);
                                this.mant = newmant;
                            } while (false);
                        }/* delead */
                        // now determine form if not PLAIN
                        mag = exp + mant.Length;
                        if (mag > 0)
                        { // most common path
                            if (mag > set.Digits)
                                if (set.Digits != 0)
                                    form = (byte)set.Form;
                            if ((mag - 1) <= MaxExp)
                                return this; // no overflow; quick return
                        }
                        else if (mag < (-5))
                            form = (byte)set.Form;
                        /* check for overflow */
                        mag--;
                        if ((mag < MinExp) | (mag > MaxExp))
                        {
                            /*overflow:*/
                            do
                            {
                                // possible reprieve if form is engineering
                                if (form == (byte)ExponentForm.Engineering)
                                {
                                    sig = mag % 3; // leftover
                                    if (sig < 0)
                                        sig = 3 + sig; // negative exponent
                                    mag = mag - sig; // exponent to use
                                                     // 1999.06.29: second test here must be MaxExp
                                    if (mag >= MinExp)
                                        if (mag <= MaxExp)
                                            break; // overflow;
                                }
                                throw new OverflowException("Exponent Overflow:" + " " + mag);
                            } while (false);
                        }/* overflow */
                        return this;
                    }
                }
            }/* i */

            // Drop through to here only if mantissa is all zeros
            ind = iszero;
            {/* select */
                if (set.Form != ExponentForm.Plain)
                    exp = 0; // standard result; go to '0'
                else if (exp > 0)
                    exp = 0; // +ve exponent also goes to '0'
                else
                {
                    // a plain number with -ve exponent; preserve and check exponent
                    if (exp < MinExp)
                        throw new OverflowException("Exponent Overflow:" + " " + exp);
                }
            }
            mant = Zero.mant; // canonical mantissa
            return this;
        }


        private static class SR
        {
            public const string ArgumentOutOfRange_NeedNonNegNum = "Non-negative number required.";
            public const string ArgumentOutOfRange_IndexLength = "Index and length must refer to a location within the string.";
            public const string ArgumentOutOfRange_NeedPositiveNum = "Positive non-zero number required.";
        }

    }

    
}
