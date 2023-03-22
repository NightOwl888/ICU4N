using ICU4N.Text;
using J2N;
using System;
using System.Linq;
using System.Threading;
#nullable enable

namespace ICU4N.Globalization
{
    public sealed partial class UNumberFormatInfo
    {
        internal UCultureData? cultureData;

        internal string currencySymbol = "\x00a4";  // U+00a4 is the symbol for International Monetary Fund.
        internal string exponentMultiplicationSign = "\u00D7";
        internal string exponentSeparator = "E";

        internal string positiveSign = "+";
        internal string negativeSign = "-";

        internal string numberGroupSeparator = ",";
        internal string numberDecimalSeparator = ".";
        internal string currencyGroupSeparator = ",";
        internal string currencyDecimalSeparator = ".";

        internal string nanSymbol = "NaN";
        internal string positiveInfinitySymbol = "∞"; // ICU4N: This differs from .NET //"Infinity";
        internal string negativeInfinitySymbol = "-∞"; // ICU4N: This differs from .NET //"-Infinity"; // NOTE THIS CURRENTLY IS NOT IN USE BY ICU
        internal string currencyInternationalSymbol = "XXX"; // ICU4N TODO: This is in RegionInfo in .NET. Perhaps our Currency formatter should accept a URegionInfo to supply this.
        //internal ICU4N.Util.Currency currency = ICU4N.Util.Currency.GetInstance("XXX");
        internal string percentDecimalSeparator = ".";
        internal string percentGroupSeparator = ",";
        internal string percentSymbol = "%";
        internal string perMilleSymbol = "\u2030";

        // Pattern stuff
        internal char padEscape = '*';
        internal char digit = '#';  // Localized pattern character no longer in CLDR
        internal char patternSeparator = ';';
        internal char significantDigit = '@';

        internal char[]? nativeDigitChars; // Lazily loaded on demand.
        internal string[] nativeDigits = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        //internal int numberDecimalDigits = 2;
        //internal int currencyDecimalDigits = 2;
        ////internal int currencyPositivePattern;
        ////internal int currencyNegativePattern;
        ////internal int numberNegativePattern = 1;
        ////internal int percentPositivePattern;
        ////internal int percentNegativePattern;
        //internal int percentDecimalDigits = 2;

        // ICU4N: These correspond with GetPatternForCurrencySpacing() in ICU4J, as separate fields.
        internal string? currencySpacingMatchPrefix = "[:^S:]";
        internal string? currencySpacingSurroundingMatchPrefix = "[:digit:]";
        internal string? currencySpacingInsertBetweenPrefix = " ";
        internal string? currencySpacingMatchSuffix = "[:^S:]";
        internal string? currencySpacingSurroundingMatchSuffix = "[:digit:]";
        internal string? currencySpacingInsertBetweenSuffix = " ";


        internal string? numberingSystemName = "latn";
        internal bool isAlgorithmic = false;

        // Capitalization Settings
        internal bool capitalizationForListOrMenu = false;
        internal bool capitalizationForStandAlone = false;

        internal UCultureData CultureData
        {
            get
            {
                if (cultureData is null)
                {
                    // Since we have no other choice, we re
                    UCultureData temp = UCultureData.Invariant;
                    Interlocked.CompareExchange(ref cultureData, temp, null);
                }
                return cultureData;
            }
        }

        internal object SentenceBreakIteratorLock => CultureData.SentenceBreakIteratorLock;
        internal BreakIterator SentenceBreakIterator => CultureData.SentenceBreakIterator;
        internal PluralRules OrdinalPluralRules => CultureData.OrdinalPluralRules;
        internal PluralRules CardinalPluralRules => CultureData.CardinalPluralRules;

        /// <summary>
        /// Gets or sets the string to use as the currency symbol.
        /// <para/>
        /// The default for <see cref="InvariantInfo"/> is "¤".
        /// </summary>
        /// <exception cref="ArgumentNullException">The property is being set to <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        public string CurrencySymbol
        {
            get => currencySymbol;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value));

                VerifyWritable();
                currencySymbol = value;
            }
        }

        /// <summary>
        /// Gets or sets the string to use as the decimal separator in numeric values.
        /// <para/>
        /// The default for <see cref="InvariantInfo"/> is ".".
        /// </summary>
        /// <exception cref="ArgumentNullException">The property is being set to <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The property is being set to the empty string.</exception>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        public string NumberDecimalSeparator
        {
            get => numberDecimalSeparator;
            set
            {
                VerifyWritable();
                VerifyDecimalSeparator(value, nameof(value));
                numberDecimalSeparator = value;
            }
        }

        /// <summary>
        /// Gets or sets the string that separates groups of digits to the left of
        /// the decimal in numeric values.
        /// <para/>
        /// The default for <see cref="InvariantInfo"/> is ",".
        /// </summary>
        /// <exception cref="ArgumentNullException">The property is being set to <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        public string NumberGroupSeparator // ICU4N: Corresponds to GroupingSeparatorString in ICU4J
        {
            get => numberGroupSeparator;
            set
            {
                VerifyWritable();
                VerifyGroupSeparator(value, nameof(value));
                numberGroupSeparator = value;
            }
        }

        ///// <summary>
        ///// Gets or sets the string to use as the decimal separator in percent values.
        ///// <para/>
        ///// The default for <see cref="InvariantInfo"/> is ".".
        ///// </summary>
        ///// <exception cref="ArgumentNullException">The property is being set to <c>null</c>.</exception>
        ///// <exception cref="ArgumentException">The property is being set to the empty string.</exception>
        ///// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        //public string PercentDecimalSeparator // ICU4N: Corresponds to GroupingSeparatorString in ICU4J, but currently isn't read by DecimalFormat. TODO: Implement in DecimalFormat/NumberFormat?
        //{
        //    get => percentDecimalSeparator;
        //    set
        //    {
        //        VerifyWritable();
        //        VerifyDecimalSeparator(value, nameof(value));
        //        percentDecimalSeparator = value;
        //    }
        //}

        ///// <summary>
        ///// Gets or sets the string that separates groups of digits to the left of
        ///// the decimal in percent values.
        ///// <para/>
        ///// The default for <see cref="InvariantInfo"/> is ",".
        ///// </summary>
        ///// <exception cref="ArgumentNullException">The property is being set to <c>null</c>.</exception>
        ///// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        //public string PercentGroupSeparator // ICU4N: Corresponds to GroupingSeparatorString in ICU4J, but currently isn't read by DecimalFormat.  TODO: Implement in DecimalFormat/NumberFormat?
        //{
        //    get => percentGroupSeparator;
        //    set
        //    {
        //        VerifyWritable();
        //        VerifyGroupSeparator(value, nameof(value));
        //        percentGroupSeparator = value;
        //    }
        //}

        /// <summary>
        /// Gets or sets the character used for a digit in a pattern.
        /// </summary>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        /// <stable>ICU 2.0</stable>
        internal char Digit // ICU4N TODO: Make public ?
        {
            get => digit;
            set
            {
                VerifyWritable();
                digit = value;
            }
        }

        /// <summary>
        /// Gets or sets a string array of native digits equivalent to the Western digits 0 through 9.
        /// <para/>
        /// The default for <see cref="InvariantInfo"/> is an array having the elements "0",
        /// "1", "2", "3", "4", "5", "6", "7", "8", and "9".
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// The property is being set to <c>null</c>.
        /// <para/>
        /// -or-
        /// <para/>
        /// In a set operation, an element of the value array is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// In a set operation, the value array does not contain 10 elements.
        /// <para/>
        /// -or-
        /// <para/>
        /// In a set operation, an element of the value array does not contain either
        /// a single <see cref="char"/> object or a pair of <see cref="char"/> objects that
        /// comprise a surrogate pair.
        /// <para/>
        /// -or-
        /// <para/>
        /// In a set operation, an element of the value array is not a number digit as defined by the
        /// <a href="https://home.unicode.org/">Unicode Standard</a>. That is, the digit in the array
        /// element does not have the Unicode <c>Number, Decimal Digit</c> (Nd) General Category value.
        /// <para/>
        /// -or-
        /// <para/>
        /// In a set operation, the numeric value of an element in the value array does not correspond
        /// to the element's position in the array. That is, the element at index 0, which is the first
        /// element of the array, does not have a numeric value of 0, or the element at index 1 does not
        /// have a numeric value of 1.
        /// </exception>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        public string[] NativeDigits
        {
            get => (string[])nativeDigits.Clone();
            set
            {
                VerifyWritable();
                VerifyNativeDigits(value, nameof(value));
                nativeDigits = value;
                nativeDigitChars = null; // Invalidate the cache. This is lazily loaded on demand.
            }
        }

        /// <summary>
        /// Gets or sets the multiplication sign.
        /// </summary>
        /// <exception cref="ArgumentNullException">The property is being set to <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        /// <stable>ICU 54</stable>
        public string ExponentMultiplicationSign
        {
            get => exponentMultiplicationSign;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value));

                VerifyWritable();
                exponentMultiplicationSign = value;
            }
        }

        /// <summary>
        /// <icu/> Gets or sets the string used to separate the mantissa from the exponent.
        /// Examples: "x10^" for 1.23x10^4, "E" for 1.23E4. The localized exponent symbol is
        /// used in localized patterns and formatted strings.
        /// </summary>
        /// <exception cref="ArgumentNullException">The property is being set to <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        /// <stable>ICU 2.0</stable>
        public string ExponentSeparator
        {
            get => exponentSeparator;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value));

                VerifyWritable();
                exponentSeparator = value;
            }
        }

        /// <summary>
        /// Gets or sets the string that represents positive infinity.
        /// <para/>
        /// The default for <see cref="InvariantInfo"/> is "Infinity".
        /// </summary>
        /// <exception cref="ArgumentNullException">The property is being set to <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        public string PositiveInfinitySymbol // ICU4N: Corresponds to InfinitySymbol in ICU4J // ICU4N TODO: Check default value and update docs.
        {
            get => positiveInfinitySymbol;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value));

                VerifyWritable();
                positiveInfinitySymbol = value;
            }
        }

        ///// <summary>
        ///// Gets or sets the string that represents negative infinity.
        ///// <para/>
        ///// The default for <see cref="InvariantInfo"/> is "-Infinity".
        ///// </summary>
        ///// <exception cref="ArgumentNullException">The property is being set to <c>null</c>.</exception>
        ///// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        //public string NegativeInfinitySymbol // ICU4N TODO: Doesn't correspond to a property in ICU4J DecimalFormat - need to implement..?
        //{
        //    get => negativeInfinitySymbol;
        //    set
        //    {
        //        if (value is null)
        //            throw new ArgumentNullException(nameof(value));

        //        VerifyWritable();
        //        negativeInfinitySymbol = value;
        //    }
        //}

        ///// <summary>
        ///// Gets or sets the international string denoting the local currency.
        ///// <para/>
        ///// The default for <see cref="InvariantInfo"/> is "XXX".
        ///// </summary>
        ///// <exception cref="ArgumentNullException">The property is being set to <c>null</c>.</exception>
        ///// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        //public string CurrencyInternationalSymbol
        //{
        //    get => currencyInternationalSymbol;
        //    set
        //    {
        //        if (value is null)
        //            throw new ArgumentNullException(nameof(value));

        //        VerifyWritable();
        //        currencyInternationalSymbol = value;
        //    }
        //}

        /// <summary>
        /// Gets or sets the string that denotes that the associated number is negative.
        /// <para/>
        /// The default for <see cref="InvariantInfo"/> is "-".
        /// </summary>
        /// <exception cref="ArgumentNullException">The property is being set to <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        public string NegativeSign
        {
            get => negativeSign;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value));

                VerifyWritable();
                negativeSign = value;
                //UpdateHasInvariantNumberSigns();
            }
        }

        /// <summary>
        /// Gets or sets the string to use as the decimal separator in currency values.
        /// <para/>
        /// The default for <see cref="InvariantInfo"/> is ".".
        /// </summary>
        /// <exception cref="ArgumentNullException">The property is being set to <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The property is being set to the empty string.</exception>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        public string CurrencyDecimalSeparator // ICU4N: Corresponds with MonetaryDecimalSeparatorString in ICU4J
        {
            get => currencyDecimalSeparator;
            set
            {
                VerifyWritable();
                VerifyDecimalSeparator(value, nameof(value));
                currencyDecimalSeparator = value;
            }
        }

        /// <summary>
        /// Gets or sets the string that separates groups of digits to the left of the decimal in currency values.
        /// <para/>
        /// The default for <see cref="InvariantInfo"/> is ",".
        /// </summary>
        /// <exception cref="ArgumentNullException">The property is being set to <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        public string CurrencyGroupSeparator // ICU4N: Corresponds with MonetaryDecimalSeparatorString in ICU4J
        {
            get => currencyGroupSeparator;
            set
            {
                VerifyWritable();
                VerifyGroupSeparator(value, nameof(value));
                currencyGroupSeparator = value;
            }
        }

        /// <summary>
        /// Gets or sets the string that represents the IEEE NaN (not a number) value.
        /// <para/>
        /// The default for <see cref="InvariantInfo"/> is "NaN".
        /// </summary>
        /// <exception cref="ArgumentNullException">The property is being set to <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        public string NaNSymbol
        {
            get => nanSymbol;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value));

                VerifyWritable();
                nanSymbol = value;
            }
        }

        /// <summary>
        /// <icu/> Gets or sets the character used to pad numbers out to a specified width. This is
        /// not the pad character itself; rather, it is the special pattern character
        /// <em>preceding</em> the pad character.  In the pattern "*_#,##0", '*' is the pad
        /// escape, and '_' is the pad character.
        /// </summary>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        /// <stable>ICU 2.0</stable>

        ///// <seealso cref="FormatWidth"/>
        ///// <seealso cref="PadPosition"/>
        ///// <seealso cref="PadCharacter"/>
        internal char PadEscape // ICU4N: The above doc seealso properties are from DecimalFormat orginally.
        {
            get => padEscape;
            set
            {
                VerifyWritable();
                padEscape = value;
            }
        }

        /// <summary>
        /// Gets or sets the character used to separate positive and negative subpatterns
        /// in a pattern.
        /// <para/>
        /// The default for <see cref="InvariantInfo"/> is ' '.
        /// </summary>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        /// <stable>ICU 2.0</stable>
        internal char PatternSeparator
        {
            get => patternSeparator;
            set
            {
                VerifyWritable();
                patternSeparator = value;
            }
        }

        /// <summary>
        /// Gets or sets the string to use as the percent symbol.
        /// <para/>
        /// The default for <see cref="InvariantInfo"/> is "%".
        /// </summary>
        /// <exception cref="ArgumentNullException">The property is being set to <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        /// <stable>ICU 58</stable>
        public string PercentSymbol // ICU4N: Corresponds to PercentString in ICU4J
        {
            get => percentSymbol;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value));

                VerifyWritable();
                percentSymbol = value;
            }
        }

        /// <summary>
        /// Gets or sets the string to use as the per mille symbol.
        /// <para/>
        /// The default for <see cref="InvariantInfo"/> is "‰", which is the Unicode character U+2030.
        /// </summary>
        /// <exception cref="ArgumentNullException">The property is being set to <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        /// <stable>ICU 58</stable>
        public string PerMilleSymbol // ICU4N: Corresponds to PerMillString in ICU4J
        {
            get => perMilleSymbol;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value));

                VerifyWritable();
                perMilleSymbol = value;
            }
        }

        /// <summary>
        /// Gets or sets the string that denotes that the associated number is positive.
        /// <para/>
        /// The default for <see cref="InvariantInfo"/> is "+".
        /// </summary>
        /// <exception cref="ArgumentNullException">The property is being set to <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        /// <stable>ICU 58</stable>
        public string PositiveSign // ICU4N: Corresponds to PlusSignString in ICU4J
        {
            get => positiveSign;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value));

                VerifyWritable();
                positiveSign = value;
                //UpdateHasInvariantNumberSigns();
            }
        }

        internal char SignificantDigit
        {
            get => significantDigit;
            set
            {
                VerifyWritable();
                significantDigit = value;
            }
        }

        /// <summary>
        /// Gets or sets the currency spacing pattern prefix. Original values come from ICU's
        /// CLDR data based on the locale provided during construction, and can be <c>null</c>. These
        /// values govern what and when text is inserted between a currency code/name/symbol
        /// and the currency amount when formatting money.
        /// <para/>
        /// The default for <see cref="InvariantInfo"/> is "[:^S:]".
        /// </summary>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        internal string? CurrencySpacingMatchPrefix // ICU4N TODO: Make public?
        {
            get => currencySpacingMatchPrefix;
            set
            {
                VerifyWritable();
                currencySpacingMatchPrefix = value;
            }
        }

        /// <summary>
        /// Gets or sets the currency spacing pattern suffix. Original values come from ICU's
        /// CLDR data based on the locale provided during construction, and can be <c>null</c>. These
        /// values govern what and when text is inserted between a currency code/name/symbol
        /// and the currency amount when formatting money.
        /// <para/>
        /// The default for <see cref="InvariantInfo"/> is "[:^S:]".
        /// </summary>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        internal string? CurrencySpacingMatchSuffix // ICU4N TODO: Make public?
        {
            get => currencySpacingMatchSuffix;
            set
            {
                VerifyWritable();
                currencySpacingMatchSuffix = value;
            }
        }

        /// <summary>
        /// Gets or sets the currency surrounding pattern prefix. Original values come from ICU's
        /// CLDR data based on the locale provided during construction, and can be <c>null</c>. These
        /// values govern what and when text is inserted between a currency code/name/symbol
        /// and the currency amount when formatting money.
        /// <para/>
        /// The default for <see cref="InvariantInfo"/> is "[:digit:]".
        /// </summary>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        internal string? CurrencySpacingSurroundingMatchPrefix // ICU4N TODO: Make public?
        {
            get => currencySpacingSurroundingMatchPrefix;
            set
            {
                VerifyWritable();
                currencySpacingSurroundingMatchPrefix = value;
            }
        }

        /// <summary>
        /// Gets or sets the currency surrounding pattern suffix. Original values come from ICU's
        /// CLDR data based on the locale provided during construction, and can be <c>null</c>. These
        /// values govern what and when text is inserted between a currency code/name/symbol
        /// and the currency amount when formatting money.
        /// <para/>
        /// The default for <see cref="InvariantInfo"/> is "[:digit:]".
        /// </summary>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        internal string? CurrencySpacingSurroundingMatchSuffix // ICU4N TODO: Make public?
        {
            get => currencySpacingSurroundingMatchSuffix;
            set
            {
                VerifyWritable();
                currencySpacingSurroundingMatchSuffix = value;
            }
        }

        /// <summary>
        /// Gets or sets the currency spacing insert between prefix. Original values come from ICU's
        /// CLDR data based on the locale provided during construction, and can be <c>null</c>. These
        /// values govern what and when text is inserted between a currency code/name/symbol
        /// and the currency amount when formatting money.
        /// <para/>
        /// The default for <see cref="InvariantInfo"/> is " ".
        /// </summary>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        internal string? CurrencySpacingInsertBetweenPrefix // ICU4N TODO: Make public?
        {
            get => currencySpacingInsertBetweenPrefix;
            set
            {
                VerifyWritable();
                currencySpacingInsertBetweenPrefix = value;
            }
        }

        /// <summary>
        /// Gets or sets the currency spacing insert between suffix. Original values come from ICU's
        /// CLDR data based on the locale provided during construction, and can be <c>null</c>. These
        /// values govern what and when text is inserted between a currency code/name/symbol
        /// and the currency amount when formatting money.
        /// <para/>
        /// The default for <see cref="InvariantInfo"/> is " ".
        /// </summary>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/> object is read-only.</exception>
        internal string? CurrencySpacingInsertBetweenSuffix // ICU4N TODO: Make public?
        {
            get => currencySpacingInsertBetweenSuffix;
            set
            {
                VerifyWritable();
                currencySpacingInsertBetweenSuffix = value;
            }
        }


        #region IDecimalFormatSymbols members

        int IDecimalFormatSymbols.CodePointZero
        {
            get => Character.CodePointAt(nativeDigits[0], 0);
        }

        //ICU4N.Util.Currency IDecimalFormatSymbols.Currency
        //{
        //    get => ICU4N.Util.Currency.GetInstance(currencySymbol); // ICU4N: This is cached
        //    set
        //    {
        //        if (value is null) 
        //            throw new ArgumentNullException(nameof(value));

        //        currency = value;
        //        currencyInternationalSymbol = value.CurrencyCode;
        //        currencySymbol = value.GetSymbol(requestedLocale);
        //    }
        //}

        string IDecimalFormatSymbols.CurrencySymbol
        {
            get => CurrencySymbol;
            set => CurrencySymbol = value;
        }

        char IDecimalFormatSymbols.DecimalSeparator
        {
            get => NumberDecimalSeparator[0];
            set => NumberDecimalSeparator = char.ToString(value);
        }

        string IDecimalFormatSymbols.DecimalSeparatorString
        {
            get => NumberDecimalSeparator;
            set => NumberDecimalSeparator = value;
        }

        char IDecimalFormatSymbols.Digit
        {
            get => Digit;
            set => Digit = value;
        }

        char[] IDecimalFormatSymbols.Digits => nativeDigitChars = nativeDigits.Select(d => d[0]).ToArray();

        string[] IDecimalFormatSymbols.DigitStrings
        {
            get => NativeDigits;
            set => NativeDigits = value; // ICU4N TODO: Add validation/setter logic here ?
        }

        string[] IDecimalFormatSymbols.DigitStringsLocal => nativeDigits;

        string IDecimalFormatSymbols.ExponentMultiplicationSign
        {
            get => ExponentMultiplicationSign;
            set => ExponentMultiplicationSign = value;
        }

        string IDecimalFormatSymbols.ExponentSeparator
        {
            get => ExponentSeparator;
            set => ExponentSeparator = value;
        }

        char IDecimalFormatSymbols.GroupingSeparator
        {
            get => NumberGroupSeparator[0];
            set => NumberGroupSeparator = char.ToString(value);
        }

        string IDecimalFormatSymbols.GroupingSeparatorString
        {
            get => NumberGroupSeparator;
            set => NumberGroupSeparator = value;
        }

        string IDecimalFormatSymbols.Infinity
        {
            get => PositiveInfinitySymbol;
            set => PositiveInfinitySymbol = value;
        }

        string IDecimalFormatSymbols.InternationalCurrencySymbol // ICU4N TODO: This is in RegionInfo in .NET, so probably shouldn't be part of this interface.
        {
            get => currencyInternationalSymbol;
            set => currencyInternationalSymbol = value;
        }

        char IDecimalFormatSymbols.MinusSign
        {
            get => NegativeSign[0];
            set => NegativeSign = char.ToString(value);
        }

        string IDecimalFormatSymbols.MinusSignString
        {
            get => NegativeSign;
            set => NegativeSign = value;
        }

        char IDecimalFormatSymbols.MonetaryDecimalSeparator
        {
            get => CurrencyDecimalSeparator[0];
            set => CurrencyDecimalSeparator = char.ToString(value);
        }

        string IDecimalFormatSymbols.MonetaryDecimalSeparatorString
        {
            get => CurrencyDecimalSeparator;
            set => CurrencyDecimalSeparator = value;
        }

        char IDecimalFormatSymbols.MonetaryGroupingSeparator
        {
            get => CurrencyGroupSeparator[0];
            set => CurrencyGroupSeparator = char.ToString(value);
        }

        string IDecimalFormatSymbols.MonetaryGroupingSeparatorString
        {
            get => CurrencyGroupSeparator;
            set => CurrencyGroupSeparator = value;
        }

        string IDecimalFormatSymbols.NaN
        {
            get => NaNSymbol;
            set => NaNSymbol = value;
        }
        char IDecimalFormatSymbols.PadEscape
        {
            get => PadEscape;
            set => PadEscape = value;
        }

        char IDecimalFormatSymbols.PatternSeparator
        {
            get => PatternSeparator;
            set => PatternSeparator = value;
        }

        char IDecimalFormatSymbols.Percent
        {
            get => PercentSymbol[0];
            set => PercentSymbol = char.ToString(value);
        }

        string IDecimalFormatSymbols.PercentString
        {
            get => PercentSymbol;
            set => PercentSymbol = value;
        }

        char IDecimalFormatSymbols.PerMill
        {
            get => PerMilleSymbol[0];
            set => PerMilleSymbol = char.ToString(value);
        }

        string IDecimalFormatSymbols.PerMillString
        {
            get => PerMilleSymbol;
            set => PerMilleSymbol = value;
        }

        char IDecimalFormatSymbols.PlusSign
        {
            get => PositiveSign[0];
            set => PositiveSign = char.ToString(value);
        }

        string IDecimalFormatSymbols.PlusSignString
        {
            get => PositiveSign;
            set => PositiveSign = value;
        }

        char IDecimalFormatSymbols.SignificantDigit
        {
            get => SignificantDigit;
            set => SignificantDigit = value;
        }

        char IDecimalFormatSymbols.ZeroDigit
        {
            get => nativeDigits[0][0];
            set => throw new NotSupportedException(SR.NotSupported_UseNativeDigitsInstead);
        }

        /// <summary>
        /// <icu/> Returns the desired currency spacing value. Original values come from ICU's
        /// CLDR data based on the locale provided during construction, and can be null. These
        /// values govern what and when text is inserted between a currency code/name/symbol
        /// and the currency amount when formatting money.
        /// <para/>
        /// For more information, see <a href="http://www.unicode.org/reports/tr35/#Currencies">
        /// UTS#35 section 5.10.2</a>.
        /// </summary>
        /// <param name="itemType">The spacing property to retrieve.</param>
        /// <param name="beforeCurrency"><c>true</c> to get the <c>beforeCurrency</c> values,
        /// <c>false</c> to get the <c>afterCurrency</c> values.</param>
        /// <returns>The value or <c>null</c>.</returns>
        /// <exception cref="ArgumentException"><paramref name="itemType"/> is out of range for <see cref="CurrencySpacingPattern"/>.</exception>
        /// <seealso cref="DecimalFormatSymbols.SetPatternForCurrencySpacing(CurrencySpacingPattern, bool, string)"/>
        /// <stable>ICU 4.2</stable>
        string? IDecimalFormatSymbols.GetPatternForCurrencySpacing(CurrencySpacingPattern itemType, bool beforeCurrency) // ICU4N TODO: Refactor this into 6 separate properties, this is ugly.
        {
            switch (itemType)
            {
                case CurrencySpacingPattern.CurrencyMatch:
                    return beforeCurrency ? currencySpacingMatchPrefix : currencySpacingMatchSuffix;
                case CurrencySpacingPattern.SurroundingMatch:
                    return beforeCurrency ? currencySpacingSurroundingMatchPrefix : currencySpacingSurroundingMatchSuffix;
                case CurrencySpacingPattern.InsertBetween:
                    return beforeCurrency ? currencySpacingInsertBetweenPrefix : currencySpacingInsertBetweenSuffix;
                default:
                    throw new ArgumentException(string.Format(SR.Argument_UnknownCurrencySpacing, itemType));
            }
        }

        #endregion
    }
}
