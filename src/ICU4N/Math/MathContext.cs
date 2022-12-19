using System;
using System.Collections.Generic;
using System.Text;

// © 2016 and later: Unicode, Inc. and others.
// License & terms of use: http://www.unicode.org/copyright.html#License
/* Generated from 'MathContext.nrx' 8 Sep 2000 11:07:48 [v2.00] */
/* Options: Binary Comments Crossref Format Java Logo Strictargs Strictcase Trace2 Verbose3 */

/* ------------------------------------------------------------------ */
/* MathContext -- Math context settings                               */
/* ------------------------------------------------------------------ */
/* Copyright IBM Corporation, 1997, 2000, 2005, 2007.  All Rights Reserved. */
/*                                                                    */
/*   The MathContext object encapsulates the settings used by the     */
/*   BigDecimal class; it could also be used by other arithmetics.    */
/* ------------------------------------------------------------------ */
/* Notes:                                                             */
/*                                                                    */
/* 1. The properties are checked for validity on construction, so     */
/*    the BigDecimal class may assume that they are correct.          */
/* ------------------------------------------------------------------ */
/* Author:    Mike Cowlishaw                                          */
/* 1997.09.03 Initial version (edited from netrexx.lang.RexxSet)      */
/* 1997.09.12 Add lostDigits property                                 */
/* 1998.05.02 Make the class immutable and final; drop set methods    */
/* 1998.06.05 Add Round (rounding modes) property                     */
/* 1998.06.25 Rename from DecimalContext; allow digits=0              */
/* 1998.10.12 change to com.ibm.icu.math package                          */
/* 1999.02.06 add javadoc comments                                    */
/* 1999.03.05 simplify; changes from discussion with J. Bloch         */
/* 1999.03.13 1.00 release to IBM Centre for Java Technology          */
/* 1999.07.10 1.04 flag serialization unused                          */
/* 2000.01.01 1.06 copyright update                                   */
/* ------------------------------------------------------------------ */

namespace ICU4N.Numerics
{

    // The rounding modes match the original BigDecimal class values
    // but were changed to match the rounding names in .NET's MidPointRounding enum.

    /// <summary>
    /// Rounding mode to be used during a <see cref="BigDecimal"/> operation.
    /// </summary>
    /// <stable>ICU 2.0</stable>
    internal enum RoundingMode // ICU4N TODO: API Align names with .NET
    {
        /// <summary>
        /// Rounding mode to round to a more positive number.
        /// Used as a setting to control the rounding mode used during a
        /// <see cref="BigDecimal"/> operation.
        /// <para/>
        /// If any of the discarded digits are non-zero then the result
        /// should be rounded towards the next more positive digit.
        /// <para/>
        /// This is named ROUND_CEILING in ICU4J.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        Ceiling = 2,

        /// <summary>
        /// Rounding mode to round towards zero.
        /// Used as a setting to control the rounding mode used during a
        /// <see cref="BigDecimal"/> operation.
        /// <para/>
        /// All discarded digits are ignored (truncated). The result is
        /// neither incremented nor decremented.
        /// <para/>
        /// This is named ROUND_DOWN in ICU4J.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        Down = 1,

        /// <summary>
        /// Rounding mode to round to a more negative number.
        /// Used as a setting to control the rounding mode used during a
        /// <see cref="BigDecimal"/> operation.
        /// <para/>
        /// If any of the discarded digits are non-zero then the result
        /// should be rounded towards the next more negative digit.
        /// <para/>
        /// This is named ROUND_FLOOR in ICU4J.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        Floor = 3,

        /// <summary>
        /// Rounding mode to round to nearest neighbor, where an equidistant
        /// value is rounded down.
        /// Used as a setting to control the rounding mode used during a
        /// <see cref="BigDecimal"/> operation.
        /// <para/>
        /// If the discarded digits represent greater than half (0.5 times)
        /// the value of a one in the next position then the result should be
        /// rounded up (away from zero).  Otherwise the discarded digits are
        /// ignored.
        /// <para/>
        /// This is named ROUND_HALF_DOWN in ICU4J.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        HalfDown = 5,

        /// <summary>
        /// Rounding mode to round to nearest neighbor, where an equidistant
        /// value is rounded to the nearest even neighbor.
        /// Used as a setting to control the rounding mode used during a
        /// <see cref="BigDecimal"/> operation.
        /// <para/>
        /// If the discarded digits represent greater than half (0.5 times)
        /// the value of a one in the next position then the result should be
        /// rounded up (away from zero).  If they represent less than half,
        /// then the result should be rounded down.
        /// <para/>
        /// Otherwise (they represent exactly half) the result is rounded
        /// down if its rightmost digit is even, or rounded up if its
        /// rightmost digit is odd (to make an even digit).
        /// <para/>
        /// This is named ROUND_HALF_EVEN in ICU4J.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        HalfEven = 6, // ICU4N TODO: Rename ToEven() ? This is the same function, but it follows the convention of the other 2 which we have no equivalent for.

        /// <summary>
        /// Rounding mode to round to nearest neighbor, where an equidistant
        /// value is rounded up.
        /// Used as a setting to control the rounding mode used during a
        /// <see cref="BigDecimal"/> operation.
        /// <para/>
        /// If the discarded digits represent greater than or equal to half
        /// (0.5 times) the value of a one in the next position then the result
        /// should be rounded up (away from zero).  Otherwise the discarded
        /// digits are ignored.
        /// <para/>
        /// This is named ROUND_HALF_UP in ICU4J.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        HalfUp = 4,

        /// <summary>
        /// Rounding mode to assert that no rounding is necessary.
        /// Used as a setting to control the rounding mode used during a
        /// <see cref="BigDecimal"/> operation.
        /// <para/>
        /// Rounding (potential loss of information) is not permitted.
        /// If any of the discarded digits are non-zero then an
        /// <see cref="ArithmeticException"/> should be thrown.
        /// <para/>
        /// This is named ROUND_UNNECESSARY in ICU4J.
        /// </summary>
        Unnecessary = 7,

        /// <summary>
        /// Rounding mode to round away from zero.
        /// Used as a setting to control the rounding mode used during a
        /// <see cref="BigDecimal"/> operation.
        /// <para/>
        /// If any of the discarded digits are non-zero then the result will
        /// be rounded up (away from zero).
        /// <para/>
        /// This is named ROUND_UP in ICU4J.
        /// </summary>
        Up = 0, // ICU4N TODO: Rearrange this to make HalfUp the default (with a value of 0) so we don't need to use -1.
    }

    /// <summary>
    /// Exponent form to be used during a <see cref="BigDecimal"/> operation.
    /// </summary>
    /// <stable>ICU 2.0</stable>
    internal enum ExponentForm : sbyte
    {
        /// <summary>
        /// Plain (fixed point) notation, without any exponent.
        /// Used as a setting to control the form of the result of a
        /// <code>BigDecimal</code> operation.
        /// <para/>
        /// A zero result in plain form may have a decimal part of one or
        /// more zeros.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        Plain = 0, // [no exponent] // ICU4N TODO: Rearrange this to make Scientific the default (with a value of 0) so we don't need to use -1.

        /// <summary>
        /// Standard floating point notation (with scientific exponential
        /// format, where there is one digit before any decimal point).
        /// Used as a setting to control the form of the result of a
        /// <see cref="BigDecimal"/> operation.
        /// <para/>
        /// A zero result in plain form may have a decimal part of one or
        /// more zeros.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        Scientific = 1, // 1 digit before .

        /// <summary>
        /// Standard floating point notation (with engineering exponential
        /// format, where the power of ten is a multiple of 3).
        /// Used as a setting to control the form of the result of a
        /// <see cref="BigDecimal"/> operation.
        /// <para/>
        /// A zero result in plain form may have a decimal part of one or
        /// more zeros.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        Engineering = 2, // 1-3 digits before .
    }

    /// <summary>
    /// The <see cref="MathContext"/> immutable class encapsulates the
    /// settings understood by the operator methods of the <see cref="BigDecimal"/>
    /// class (and potentially other classes).  Operator methods are those
    /// that effect an operation on a number or a pair of numbers.
    /// <para/>
    /// The settings, which are not base-dependent, comprise:
    /// <list type="number">
    ///     <item>
    ///         <term>digits</term>
    ///         <description>The number of digits (precision) to be used for an operation.</description>
    ///     </item>
    ///     <item>
    ///         <term>form</term>
    ///         <description>The form of any exponent that results from the operation.</description>
    ///     </item>
    ///     <item>
    ///         <term>lostDigits</term>
    ///         <description>Whether checking for lost digits is enabled.</description>
    ///     </item>
    ///     <item>
    ///         <term>roundingMode</term>
    ///         <description>The algorithm to be used for rounding.</description>
    ///     </item>
    /// </list>
    /// <para/>
    /// When provided, a <see cref="MathContext"/> object supplies the
    /// settings for an operation directly.
    /// <para/>
    /// When <see cref="MathContext.Default"/> is provided for a
    /// <see cref="MathContext"/> parameter then the default settings are used
    /// (<c>9, Scientific, false, RoundHalfUp</c>).
    /// <para/>
    /// In the <see cref="BigDecimal"/> class, all methods which accept a
    /// <see cref="MathContext"/> object defaults) also have a version of the
    /// method which does not accept a <see cref="MathContext"/> parameter.  These versions
    /// carry out unlimited precision fixed point arithmetic (as though the
    /// settings were (<c>0, Plain, false, RoundHalfUp</c>).
    /// <para/>
    /// The instance variables are shared with default access (so they are
    /// directly accessible to the <see cref="BigDecimal"/> class), but must
    /// never be changed.
    /// <para/>
    /// The rounding mode constants have the same names and values as the
    /// constants of the same name in <c>java.math.BigDecimal</c>, to
    /// maintain compatibility with earlier versions of
    /// <see cref="BigDecimal"/>.
    /// </summary>
    /// <seealso cref="BigDecimal"/>
    /// <author>Mike Cowlishaw</author>
    /// <stable>ICU 2.0</stable>
#if FEATURE_SERIALIZABLE
    [Serializable]
#endif
    internal sealed class MathContext // ICU4N TODO: API - this was public in ICU4J
    {
        //private static final java.lang.String $0="MathContext.nrx";

        /* ----- Properties ----- */
        /* properties public constant */

        /// <summary>
        /// Plain (fixed point) notation, without any exponent.
        /// Used as a setting to control the form of the result of a
        /// <code>BigDecimal</code> operation.
        /// <para/>
        /// A zero result in plain form may have a decimal part of one or
        /// more zeros.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public const ExponentForm Plain = 0; // [no exponent]

        /// <summary>
        /// Standard floating point notation (with scientific exponential
        /// format, where there is one digit before any decimal point).
        /// Used as a setting to control the form of the result of a
        /// <see cref="BigDecimal"/> operation.
        /// <para/>
        /// A zero result in plain form may have a decimal part of one or
        /// more zeros.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public const ExponentForm Scientific = ExponentForm.Scientific; // 1 digit before .

        /// <summary>
        /// Standard floating point notation (with engineering exponential
        /// format, where the power of ten is a multiple of 3).
        /// Used as a setting to control the form of the result of a
        /// <see cref="BigDecimal"/> operation.
        /// <para/>
        /// A zero result in plain form may have a decimal part of one or
        /// more zeros.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public const ExponentForm Engineering = ExponentForm.Engineering; // 1-3 digits before .

        // The rounding modes match the original BigDecimal class values
        // but were changed to match the rounding names in .NET's MidPointRounding enum.

        /// <summary>
        /// Rounding mode to round to a more positive number.
        /// Used as a setting to control the rounding mode used during a
        /// <see cref="BigDecimal"/> operation.
        /// <para/>
        /// If any of the discarded digits are non-zero then the result
        /// should be rounded towards the next more positive digit.
        /// <para/>
        /// This is named ROUND_CEILING in ICU4J.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public const RoundingMode RoundCeiling = RoundingMode.Ceiling;

        /// <summary>
        /// Rounding mode to round towards zero.
        /// Used as a setting to control the rounding mode used during a
        /// <see cref="BigDecimal"/> operation.
        /// <para/>
        /// All discarded digits are ignored (truncated). The result is
        /// neither incremented nor decremented.
        /// <para/>
        /// This is named ROUND_DOWN in ICU4J.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public const RoundingMode RoundDown = RoundingMode.Down;

        /// <summary>
        /// Rounding mode to round to a more negative number.
        /// Used as a setting to control the rounding mode used during a
        /// <see cref="BigDecimal"/> operation.
        /// <para/>
        /// If any of the discarded digits are non-zero then the result
        /// should be rounded towards the next more negative digit.
        /// <para/>
        /// This is named ROUND_FLOOR in ICU4J.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public const RoundingMode RoundFloor = RoundingMode.Floor;

        /// <summary>
        /// Rounding mode to round to nearest neighbor, where an equidistant
        /// value is rounded down.
        /// Used as a setting to control the rounding mode used during a
        /// <see cref="BigDecimal"/> operation.
        /// <para/>
        /// If the discarded digits represent greater than half (0.5 times)
        /// the value of a one in the next position then the result should be
        /// rounded up (away from zero).  Otherwise the discarded digits are
        /// ignored.
        /// <para/>
        /// This is named ROUND_HALF_DOWN in ICU4J.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public const RoundingMode RoundHalfDown = RoundingMode.HalfDown;

        /// <summary>
        /// Rounding mode to round to nearest neighbor, where an equidistant
        /// value is rounded to the nearest even neighbor.
        /// Used as a setting to control the rounding mode used during a
        /// <see cref="BigDecimal"/> operation.
        /// <para/>
        /// If the discarded digits represent greater than half (0.5 times)
        /// the value of a one in the next position then the result should be
        /// rounded up (away from zero).  If they represent less than half,
        /// then the result should be rounded down.
        /// <para/>
        /// Otherwise (they represent exactly half) the result is rounded
        /// down if its rightmost digit is even, or rounded up if its
        /// rightmost digit is odd (to make an even digit).
        /// <para/>
        /// This is named ROUND_HALF_EVEN in ICU4J.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public const RoundingMode RoundHalfEven = RoundingMode.HalfEven;

        /// <summary>
        /// Rounding mode to round to nearest neighbor, where an equidistant
        /// value is rounded up.
        /// Used as a setting to control the rounding mode used during a
        /// <see cref="BigDecimal"/> operation.
        /// <para/>
        /// If the discarded digits represent greater than or equal to half
        /// (0.5 times) the value of a one in the next position then the result
        /// should be rounded up (away from zero).  Otherwise the discarded
        /// digits are ignored.
        /// <para/>
        /// This is named ROUND_HALF_UP in ICU4J.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public const RoundingMode RoundHalfUp = RoundingMode.HalfUp;

        /// <summary>
        /// Rounding mode to assert that no rounding is necessary.
        /// Used as a setting to control the rounding mode used during a
        /// <see cref="BigDecimal"/> operation.
        /// <para/>
        /// Rounding (potential loss of information) is not permitted.
        /// If any of the discarded digits are non-zero then an
        /// <see cref="ArithmeticException"/> should be thrown.
        /// <para/>
        /// This is named ROUND_UNNECESSARY in ICU4J.
        /// </summary>
        public const RoundingMode RoundUnnecessary = RoundingMode.Unnecessary;

        /// <summary>
        /// Rounding mode to round away from zero.
        /// Used as a setting to control the rounding mode used during a
        /// <see cref="BigDecimal"/> operation.
        /// <para/>
        /// If any of the discarded digits are non-zero then the result will
        /// be rounded up (away from zero).
        /// <para/>
        /// This is named ROUND_UP in ICU4J.
        /// </summary>
        public const RoundingMode RoundUp = RoundingMode.Up;


        /* properties shared */

        /// <summary>
        /// The number of digits (precision) to be used for an operation.
        /// A value of 0 indicates that unlimited precision (as many digits
        /// as are required) will be used.
        /// <para/>
        /// The <see cref="BigDecimal"/> operator methods use this value to
        /// determine the precision of results.
        /// Note that leading zeros (in the integer part of a number) are
        /// never significant.
        /// <para/>
        /// <see cref="digits"/> will always be non-negative.
        /// </summary>
        private readonly int digits;

        /// <summary>
        /// The form of results from an operation.
        /// <para/>
        /// The <see cref="BigDecimal"/> operator methods use this value to
        /// determine the form of results, in particular whether and how
        /// exponential notation should be used.
        /// </summary>
        private readonly ExponentForm form; // values for this must fit in a byte

        /// <summary>
        /// Controls whether lost digits checking is enabled for an
        /// operation.
        /// Set to <c>true</c> to enable checking, or
        /// to <c>false</c> to disable checking.
        /// <para/>
        /// When enabled, the <see cref="BigDecimal"/> operator methods check
        /// the precision of their operand or operands, and throw an
        /// <see cref="ArithmeticException"/> if an operand is more precise
        /// than the digits setting (that is, digits would be lost).
        /// When disabled, operands are rounded to the specified digits.
        /// </summary>
        private readonly bool lostDigits;

        /// <summary>
        /// The rounding algorithm to be used for an operation.
        /// <para/>
        /// The <see cref="BigDecimal"/> operator methods use this value to
        /// determine the algorithm to be used when non-zero digits have to
        /// be discarded in order to reduce the precision of a result.
        /// The value must be one of the values in <see cref="Numerics.RoundingMode"/>.
        /// </summary>
        private readonly RoundingMode roundingMode;

        /* properties private constant */
        // default settings
        private const ExponentForm DEFAULT_FORM = ExponentForm.Scientific;
        private const int DEFAULT_DIGITS = 9;
        private const bool DEFAULT_LOSTDIGITS=false;
        private const RoundingMode DEFAULT_ROUNDINGMODE = RoundingMode.HalfUp;

        /* properties private constant */

        private const int MIN_DIGITS = 0; // smallest value for DIGITS.
        private const int MAX_DIGITS = 999999999;   // largest value for DIGITS.  If increased,
                                                    // the BigDecimal class may need update.
                                                    // list of valid rounding mode values, most common two first
        private static readonly RoundingMode[] ROUNDS = new RoundingMode[] { RoundingMode.HalfUp, RoundingMode.Unnecessary, RoundingMode.Ceiling, RoundingMode.Down, RoundingMode.Floor, RoundingMode.HalfDown, RoundingMode.HalfEven, RoundingMode.Up };

        // ICU4N TODO: Revisit these names - we should make them match the RoundingMode enum, ideally.
        private static readonly string[] ROUNDWORDS= new string[] { "ROUND_HALF_UP", "ROUND_UNNECESSARY", "ROUND_CEILING", "ROUND_DOWN", "ROUND_FLOOR", "ROUND_HALF_DOWN", "ROUND_HALF_EVEN", "ROUND_UP" }; // matching names of the ROUNDS values




        /* properties private constant unused */

        // Serialization version
        //private static readonly long serialVersionUID = 7163376998892515376L;

        /* properties public constant */

        /// <summary>
        /// A <see cref="MathContext"/> object initialized to the default
        /// settings for general-purpose arithmetic.  That is,
        /// <c>digits=9 form=Scientific lostDigits=false roundingMode=HalfUp</c>.
        /// </summary>
        /// <seealso cref="Numerics.RoundingMode"/>
        /// <seealso cref="ExponentForm"/>
        /// <stable>ICU 2.0</stable>
        public static readonly MathContext Default = new MathContext(DEFAULT_DIGITS, DEFAULT_FORM, DEFAULT_LOSTDIGITS, DEFAULT_ROUNDINGMODE);


        /* ----- Constructors ----- */

        /// <summary>
        /// Constructs a new <see cref="MathContext"/> with a specified
        /// precision.
        /// The other settings are set to the default values
        /// (<see cref="Default"/>).
        /// <para/>
        /// An <see cref="ArgumentOutOfRangeException"/> is thrown if the
        /// <paramref name="digits"/> parameter is out of range
        /// (&lt;0 or &gt;999999999).
        /// </summary>
        /// <param name="digits">The <see cref="int"/> digits setting
        ///                         for this <see cref="MathContext"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="digits"/> is out of range.</exception>
        /// <stable>ICU 2.0</stable>
        public MathContext(int digits)
            : this(digits, DEFAULT_FORM, DEFAULT_LOSTDIGITS, DEFAULT_ROUNDINGMODE)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="MathContext"/> with a specified
        /// precision and form.
        /// The other settings are set to the default values
        /// (<see cref="Default"/>).
        /// <para/>
        /// An <see cref="ArgumentOutOfRangeException"/> is thrown if the
        /// <paramref name="digits"/> parameter is out of range
        /// (&lt;0 or &gt;999999999), or if the value given for the
        /// <paramref name="form"/> parameter is
        /// not one of the appropriate enum values.
        /// </summary>
        /// <param name="digits">The <see cref="int"/> digits setting
        ///                         for this <see cref="MathContext"/>.</param>
        /// <param name="form">The <see cref="ExponentForm"/> form setting
        ///                         for this <see cref="MathContext"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="digits"/> or <paramref name="form"/> is out of range.</exception>
        /// <stable>ICU 2.0</stable>
        public MathContext(int digits, ExponentForm form)
            : this(digits, form, DEFAULT_LOSTDIGITS, DEFAULT_ROUNDINGMODE)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="MathContext"/> with a specified
        /// precision, form, and lostDigits setting.
        /// The <see cref="RoundingMode"/> setting is set to its default value
        /// (<see cref="Default"/>).
        /// <para/>
        /// An <see cref="ArgumentOutOfRangeException"/> is thrown if the
        /// <paramref name="digits"/> parameter is out of range
        /// (&lt;0 or &gt;999999999), or if the value given for the
        /// <paramref name="form"/> parameter is
        /// not one of the appropriate enum values.
        /// </summary>
        /// <param name="digits">The <see cref="int"/> digits setting
        ///                         for this <see cref="MathContext"/>.</param>
        /// <param name="form">The <see cref="ExponentForm"/> form setting
        ///                         for this <see cref="MathContext"/>.</param>
        /// <param name="lostDigits">The <see cref="bool"/> lostDigits
        ///                         setting for this <see cref="MathContext"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="digits"/>, <paramref name="form"/>, or <paramref name="lostDigits"/> is out of range.</exception>
        /// <stable>ICU 2.0</stable>
        public MathContext(int digits, ExponentForm form, bool lostDigits)
            : this(digits, form, lostDigits, DEFAULT_ROUNDINGMODE)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="MathContext"/> with a specified
        /// precision, form, lostDigits, and roundingMode setting.
        /// <para/>
        /// An <see cref="ArgumentOutOfRangeException"/> is thrown if the
        /// <paramref name="digits"/> parameter is out of range
        /// (&lt;0 or &gt;999999999), or if the value given for the
        /// <paramref name="form"/> or <paramref name="roundingMode"/> parameters is
        /// not one of the appropriate enum values.
        /// </summary>
        /// <param name="digits">The <see cref="int"/> digits setting
        ///                         for this <see cref="MathContext"/>.</param>
        /// <param name="form">The <see cref="ExponentForm"/> form setting
        ///                         for this <see cref="MathContext"/>.</param>
        /// <param name="lostDigits">The <see cref="bool"/> lostDigits
        ///                         setting for this <see cref="MathContext"/>.</param>
        /// <param name="roundingMode">The <see cref="Numerics.RoundingMode"/> setting
        ///                         for this <see cref="MathContext"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="digits"/>, <paramref name="form"/>, <paramref name="lostDigits"/>, or <paramref name="roundingMode"/> is out of range.</exception>
        /// <stable>ICU 2.0</stable>
        public MathContext(int digits, ExponentForm form, bool lostDigits, RoundingMode roundingMode)
        {
            // set values, after checking
            if (digits != DEFAULT_DIGITS)
            {
                if (digits < MIN_DIGITS)
                    throw new ArgumentOutOfRangeException(nameof(digits), "Digits too small:" + " " + digits);
                if (digits > MAX_DIGITS)
                    throw new ArgumentOutOfRangeException(nameof(digits), "Digits too large:" + " " + digits);
            }
            if (form == ExponentForm.Scientific)
            {
                // [most common]
            }
            else if (form == ExponentForm.Engineering)
            {
                // Intentionally blank
            }
            else if (form == ExponentForm.Plain)
            {
                // Intentionally blank
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(form), "Bad form value:" + " " + form);
            }
            if (!IsValidRound(roundingMode))
                throw new ArgumentOutOfRangeException(nameof(roundingMode), "Bad roundingMode value:" + " " + roundingMode);
            this.digits = digits;
            this.form = form;
            this.lostDigits = lostDigits; // [no bad value possible]
            this.roundingMode = roundingMode;
        }

        /// <summary>
        /// Gets the digits setting.
        /// This value is always non-negative.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public int Digits => digits;

        /// <summary>
        /// Gets the form setting.
        /// This will be one of the <see cref="ExponentForm"/> values.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public ExponentForm Form => form;

        /// <summary>
        /// Gets the lostDigits setting.
        /// This will be either <c>true</c> (enabled) or
        /// <c>false</c> (disabled).
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public bool LostDigits => lostDigits;

        /// <summary>
        /// Gets the roundingMode setting.
        /// This will be one of the <see cref="Numerics.RoundingMode"/> values.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public RoundingMode RoundingMode => roundingMode;

        /// <summary>
        /// Returns the <see cref="MathContext"/> as a readable string.
        /// The <see cref="string"/> returned represents the settings of the
        /// <see cref="MathContext"/> object as four blank-delimited words
        /// separated by a single blank and with no leading or trailing blanks,
        /// as follows:
        /// <list type="number">
        ///     <item>
        ///         <description><c>digits=</c>, immediately followed by the value of the digits setting as a numeric word.</description>
        ///     </item>
        ///     <item>
        ///         <description><c>form=</c>, immediately followed by the value of the form setting as an uppercase word
        ///             (one of <code>SCIENTIFIC</code>, <code>PLAIN</code>, or <code>ENGINEERING</code>).</description>
        ///     </item>
        ///     <item>
        ///         <description><c>lostDigits=</c>, immediately followed by the value of the lostDigits setting
        ///         (<c>1</c> if enabled, <c>0</c> if disabled).</description>
        ///     </item>
        ///     <item>
        ///         <term></term>
        ///         <description><c>roundingMode=</c>, immediately followed by the value of the roundingMode setting as a word.
        ///         This word will be the same as the name of the corresponding public constant.</description>
        ///     </item>
        /// </list>
        /// <para/>
        /// For example:
        /// <code>
        /// digits=9 form=SCIENTIFIC lostDigits=0 roundingMode=ROUND_HALF_UP
        /// </code>
        /// <para/>
        /// Additional words may be appended to the result of
        /// <see cref="ToString()"/> in the future if more properties are added
        /// to the class.
        /// </summary>
        /// <returns>A <see cref="string"/> representing the context settings.</returns>
        /// <stable>ICU 2.0</stable>
        public override string ToString()
        {
            string formstr = null;
            int r = 0;
            string roundword = null;
            if (form == ExponentForm.Scientific)
                formstr = "SCIENTIFIC";
            else if (form == ExponentForm.Engineering)
                formstr = "ENGINEERING"; // ICU4N TODO: Revisit casing of strings
            else
            {
                formstr = "PLAIN";/* form=PLAIN */
            }
            int l = ROUNDS.Length; r = 0; 
            for (;l > 0;l--,r++){
                if (roundingMode == ROUNDS[r])
                {
                    roundword = ROUNDWORDS[r];
                    break;
                }
            }

            return "digits=" + digits + " " + "form=" + formstr + " " + "lostDigits=" + (lostDigits ? "1" : "0") + " " + "roundingMode=" + roundword;
        }


        /* <sgml> Test whether round is valid. </sgml> */
        // This could be made shared for use by BigDecimal for setScale.

        internal static bool IsValidRound(RoundingMode testround)
        {
            int r = 0;
            int test = ROUNDS.Length; for (r = 0;test > 0;test--,r++){
                if (testround == ROUNDS[r])
                    return true;
            }
            return false;
        }
    }
}
