using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using static ICU4N.Numerics.Padder;
using static ICU4N.Numerics.Parser;
using static ICU4N.Text.CompactDecimalFormat;

namespace ICU4N.Numerics //ICU4N.Impl.Number
{
#if FEATURE_SERIALIZABLE
    [Serializable]
#endif
    internal class DecimalFormatProperties
#if FEATURE_CLONEABLE
        , ICloneable
#endif
    {
        private static readonly DecimalFormatProperties DEFAULT = new DecimalFormatProperties();

        /** Auto-generated. */
        //private static final long serialVersionUID = 4095518955889349243L;

        // The setters in this class should NOT have any side-effects or perform any validation. It is
        // up to the consumer of the property bag to deal with property validation.

        // The fields are all marked "transient" because custom serialization is being used.

        /*--------------------------------------------------------------------------------------------+/
        /| IMPORTANT!                                                                                 |/
        /| WHEN ADDING A NEW PROPERTY, add it here, in #_clear(), in #_copyFrom(), in #equals(),      |/
        /| and in #_hashCode().                                                                       |/
        /|                                                                                            |/
        /| The unit test PropertiesTest will catch if you forget to add it to #clear(), #copyFrom(),  |/
        /| or #equals(), but it will NOT catch if you forget to add it to #hashCode().                |/
        /+--------------------------------------------------------------------------------------------*/

#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private IDictionary<string, IDictionary<string, string>> compactCustomData;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private CompactStyle? compactStyle;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private Currency currency;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private CurrencyPluralInfo currencyPluralInfo;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private CurrencyUsage? currencyUsage;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private bool decimalPatternMatchRequired;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private bool decimalSeparatorAlwaysShown;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private bool exponentSignAlwaysShown;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private int formatWidth;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private int groupingSize;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private int magnitudeMultiplier;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private BigMath.MathContext mathContext;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private int maximumFractionDigits;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private int maximumIntegerDigits;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private int maximumSignificantDigits;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private int minimumExponentDigits;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private int minimumFractionDigits;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private int minimumGroupingDigits;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private int minimumIntegerDigits;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private int minimumSignificantDigits;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private BigMath.BigDecimal multiplier;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private string negativePrefix;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private string negativePrefixPattern;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private string negativeSuffix;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private string negativeSuffixPattern;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private Padder.PadPosition? padPosition;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private string padString;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private bool parseCaseSensitive;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private GroupingMode? parseGroupingMode;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private bool parseIntegerOnly;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private ParseMode? parseMode;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private bool parseNoExponent;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private bool parseToBigDecimal;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private PluralRules pluralRules;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private string positivePrefix;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private string positivePrefixPattern;

#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private string positiveSuffix;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private string positiveSuffixPattern;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private BigMath.BigDecimal roundingIncrement;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private BigMath.RoundingMode? roundingMode;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private int secondaryGroupingSize;
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private bool signAlwaysShown;

        /*--------------------------------------------------------------------------------------------+/
        /| IMPORTANT!                                                                                 |/
        /| WHEN ADDING A NEW PROPERTY, add it here, in #_clear(), in #_copyFrom(), in #equals(),      |/
        /| and in #_hashCode().                                                                       |/
        /|                                                                                            |/
        /| The unit test PropertiesTest will catch if you forget to add it to #clear(), #copyFrom(),  |/
        /| or #equals(), but it will NOT catch if you forget to add it to #hashCode().                |/
        /+--------------------------------------------------------------------------------------------*/

        public DecimalFormatProperties()
        {
            Clear();
        }

        /**
         * Sets all properties to their defaults (unset).
         *
         * <p>
         * All integers default to -1 EXCEPT FOR MAGNITUDE MULTIPLIER which has a default of 0 (since negative numbers are
         * important).
         *
         * <p>
         * All booleans default to false.
         *
         * <p>
         * All non-primitive types default to null.
         *
         * @return The property bag, for chaining.
         */
        private DecimalFormatProperties ClearImpl()
        {
            compactCustomData = null;
            compactStyle = null;
            currency = null;
            currencyPluralInfo = null;
            currencyUsage = null;
            decimalPatternMatchRequired = false;
            decimalSeparatorAlwaysShown = false;
            exponentSignAlwaysShown = false;
            formatWidth = -1;
            groupingSize = -1;
            magnitudeMultiplier = 0;
            mathContext = null;
            maximumFractionDigits = -1;
            maximumIntegerDigits = -1;
            maximumSignificantDigits = -1;
            minimumExponentDigits = -1;
            minimumFractionDigits = -1;
            minimumGroupingDigits = -1;
            minimumIntegerDigits = -1;
            minimumSignificantDigits = -1;
            multiplier = null;
            negativePrefix = null;
            negativePrefixPattern = null;
            negativeSuffix = null;
            negativeSuffixPattern = null;
            padPosition = null;
            padString = null;
            parseCaseSensitive = false;
            parseGroupingMode = null;
            parseIntegerOnly = false;
            parseMode = null;
            parseNoExponent = false;
            parseToBigDecimal = false;
            pluralRules = null;
            positivePrefix = null;
            positivePrefixPattern = null;
            positiveSuffix = null;
            positiveSuffixPattern = null;
            roundingIncrement = null;
            roundingMode = null;
            secondaryGroupingSize = -1;
            signAlwaysShown = false;
            return this;
        }

        private DecimalFormatProperties CopyFromImpl(DecimalFormatProperties other)
        {
            compactCustomData = other.compactCustomData;
            compactStyle = other.compactStyle;
            currency = other.currency;
            currencyPluralInfo = other.currencyPluralInfo;
            currencyUsage = other.currencyUsage;
            decimalPatternMatchRequired = other.decimalPatternMatchRequired;
            decimalSeparatorAlwaysShown = other.decimalSeparatorAlwaysShown;
            exponentSignAlwaysShown = other.exponentSignAlwaysShown;
            formatWidth = other.formatWidth;
            groupingSize = other.groupingSize;
            magnitudeMultiplier = other.magnitudeMultiplier;
            mathContext = other.mathContext;
            maximumFractionDigits = other.maximumFractionDigits;
            maximumIntegerDigits = other.maximumIntegerDigits;
            maximumSignificantDigits = other.maximumSignificantDigits;
            minimumExponentDigits = other.minimumExponentDigits;
            minimumFractionDigits = other.minimumFractionDigits;
            minimumGroupingDigits = other.minimumGroupingDigits;
            minimumIntegerDigits = other.minimumIntegerDigits;
            minimumSignificantDigits = other.minimumSignificantDigits;
            multiplier = other.multiplier;
            negativePrefix = other.negativePrefix;
            negativePrefixPattern = other.negativePrefixPattern;
            negativeSuffix = other.negativeSuffix;
            negativeSuffixPattern = other.negativeSuffixPattern;
            padPosition = other.padPosition;
            padString = other.padString;
            parseCaseSensitive = other.parseCaseSensitive;
            parseGroupingMode = other.parseGroupingMode;
            parseIntegerOnly = other.parseIntegerOnly;
            parseMode = other.parseMode;
            parseNoExponent = other.parseNoExponent;
            parseToBigDecimal = other.parseToBigDecimal;
            pluralRules = other.pluralRules;
            positivePrefix = other.positivePrefix;
            positivePrefixPattern = other.positivePrefixPattern;
            positiveSuffix = other.positiveSuffix;
            positiveSuffixPattern = other.positiveSuffixPattern;
            roundingIncrement = other.roundingIncrement;
            roundingMode = other.roundingMode;
            secondaryGroupingSize = other.secondaryGroupingSize;
            signAlwaysShown = other.signAlwaysShown;
            return this;
        }

        private bool EqualsImpl(DecimalFormatProperties other)
        {
            bool eq = true;
            eq = eq && _equalsHelper(compactCustomData, other.compactCustomData);
            eq = eq && _equalsHelper(compactStyle, other.compactStyle);
            eq = eq && _equalsHelper(currency, other.currency);
            eq = eq && _equalsHelper(currencyPluralInfo, other.currencyPluralInfo);
            eq = eq && _equalsHelper(currencyUsage, other.currencyUsage);
            eq = eq && _equalsHelper(decimalPatternMatchRequired, other.decimalPatternMatchRequired);
            eq = eq && _equalsHelper(decimalSeparatorAlwaysShown, other.decimalSeparatorAlwaysShown);
            eq = eq && _equalsHelper(exponentSignAlwaysShown, other.exponentSignAlwaysShown);
            eq = eq && _equalsHelper(formatWidth, other.formatWidth);
            eq = eq && _equalsHelper(groupingSize, other.groupingSize);
            eq = eq && _equalsHelper(magnitudeMultiplier, other.magnitudeMultiplier);
            eq = eq && _equalsHelper(mathContext, other.mathContext);
            eq = eq && _equalsHelper(maximumFractionDigits, other.maximumFractionDigits);
            eq = eq && _equalsHelper(maximumIntegerDigits, other.maximumIntegerDigits);
            eq = eq && _equalsHelper(maximumSignificantDigits, other.maximumSignificantDigits);
            eq = eq && _equalsHelper(minimumExponentDigits, other.minimumExponentDigits);
            eq = eq && _equalsHelper(minimumFractionDigits, other.minimumFractionDigits);
            eq = eq && _equalsHelper(minimumGroupingDigits, other.minimumGroupingDigits);
            eq = eq && _equalsHelper(minimumIntegerDigits, other.minimumIntegerDigits);
            eq = eq && _equalsHelper(minimumSignificantDigits, other.minimumSignificantDigits);
            eq = eq && _equalsHelper(multiplier, other.multiplier);
            eq = eq && _equalsHelper(negativePrefix, other.negativePrefix);
            eq = eq && _equalsHelper(negativePrefixPattern, other.negativePrefixPattern);
            eq = eq && _equalsHelper(negativeSuffix, other.negativeSuffix);
            eq = eq && _equalsHelper(negativeSuffixPattern, other.negativeSuffixPattern);
            eq = eq && _equalsHelper(padPosition, other.padPosition);
            eq = eq && _equalsHelper(padString, other.padString);
            eq = eq && _equalsHelper(parseCaseSensitive, other.parseCaseSensitive);
            eq = eq && _equalsHelper(parseGroupingMode, other.parseGroupingMode);
            eq = eq && _equalsHelper(parseIntegerOnly, other.parseIntegerOnly);
            eq = eq && _equalsHelper(parseMode, other.parseMode);
            eq = eq && _equalsHelper(parseNoExponent, other.parseNoExponent);
            eq = eq && _equalsHelper(parseToBigDecimal, other.parseToBigDecimal);
            eq = eq && _equalsHelper(pluralRules, other.pluralRules);
            eq = eq && _equalsHelper(positivePrefix, other.positivePrefix);
            eq = eq && _equalsHelper(positivePrefixPattern, other.positivePrefixPattern);
            eq = eq && _equalsHelper(positiveSuffix, other.positiveSuffix);
            eq = eq && _equalsHelper(positiveSuffixPattern, other.positiveSuffixPattern);
            eq = eq && _equalsHelper(roundingIncrement, other.roundingIncrement);
            eq = eq && _equalsHelper(roundingMode, other.roundingMode);
            eq = eq && _equalsHelper(secondaryGroupingSize, other.secondaryGroupingSize);
            eq = eq && _equalsHelper(signAlwaysShown, other.signAlwaysShown);
            return eq;
        }

        private bool _equalsHelper(bool mine, bool theirs)
        {
            return mine == theirs;
        }

        private bool _equalsHelper(int mine, int theirs)
        {
            return mine == theirs;
        }

        private bool _equalsHelper(object mine, object theirs)
        {
            if (mine == theirs)
                return true;
            if (mine == null)
                return false;
            return mine.Equals(theirs);
        }

        private int GetHashCodeImpl()
        {
            int hashCode = 0;
            hashCode ^= _hashCodeHelper(compactCustomData);
            hashCode ^= _hashCodeHelper(compactStyle);
            hashCode ^= _hashCodeHelper(currency);
            hashCode ^= _hashCodeHelper(currencyPluralInfo);
            hashCode ^= _hashCodeHelper(currencyUsage);
            hashCode ^= _hashCodeHelper(decimalPatternMatchRequired);
            hashCode ^= _hashCodeHelper(decimalSeparatorAlwaysShown);
            hashCode ^= _hashCodeHelper(exponentSignAlwaysShown);
            hashCode ^= _hashCodeHelper(formatWidth);
            hashCode ^= _hashCodeHelper(groupingSize);
            hashCode ^= _hashCodeHelper(magnitudeMultiplier);
            hashCode ^= _hashCodeHelper(mathContext);
            hashCode ^= _hashCodeHelper(maximumFractionDigits);
            hashCode ^= _hashCodeHelper(maximumIntegerDigits);
            hashCode ^= _hashCodeHelper(maximumSignificantDigits);
            hashCode ^= _hashCodeHelper(minimumExponentDigits);
            hashCode ^= _hashCodeHelper(minimumFractionDigits);
            hashCode ^= _hashCodeHelper(minimumGroupingDigits);
            hashCode ^= _hashCodeHelper(minimumIntegerDigits);
            hashCode ^= _hashCodeHelper(minimumSignificantDigits);
            hashCode ^= _hashCodeHelper(multiplier);
            hashCode ^= _hashCodeHelper(negativePrefix);
            hashCode ^= _hashCodeHelper(negativePrefixPattern);
            hashCode ^= _hashCodeHelper(negativeSuffix);
            hashCode ^= _hashCodeHelper(negativeSuffixPattern);
            hashCode ^= _hashCodeHelper(padPosition);
            hashCode ^= _hashCodeHelper(padString);
            hashCode ^= _hashCodeHelper(parseCaseSensitive);
            hashCode ^= _hashCodeHelper(parseGroupingMode);
            hashCode ^= _hashCodeHelper(parseIntegerOnly);
            hashCode ^= _hashCodeHelper(parseMode);
            hashCode ^= _hashCodeHelper(parseNoExponent);
            hashCode ^= _hashCodeHelper(parseToBigDecimal);
            hashCode ^= _hashCodeHelper(pluralRules);
            hashCode ^= _hashCodeHelper(positivePrefix);
            hashCode ^= _hashCodeHelper(positivePrefixPattern);
            hashCode ^= _hashCodeHelper(positiveSuffix);
            hashCode ^= _hashCodeHelper(positiveSuffixPattern);
            hashCode ^= _hashCodeHelper(roundingIncrement);
            hashCode ^= _hashCodeHelper(roundingMode);
            hashCode ^= _hashCodeHelper(secondaryGroupingSize);
            hashCode ^= _hashCodeHelper(signAlwaysShown);
            return hashCode;
        }

        private int _hashCodeHelper(bool value)
        {
            return value ? 1 : 0;
        }

        private int _hashCodeHelper(int value)
        {
            return value * 13;
        }

        private int _hashCodeHelper(object value)
        {
            if (value == null)
                return 0;
            return value.GetHashCode();
        }

        public DecimalFormatProperties Clear()
        {
            return ClearImpl();
        }

        /** Creates and returns a shallow copy of the property bag. */
        public object Clone()
        {
            // super.clone() returns a shallow copy.
            return (DecimalFormatProperties)base.MemberwiseClone();
        }

        /**
         * Shallow-copies the properties from the given property bag into this property bag.
         *
         * @param other
         *            The property bag from which to copy and which will not be modified.
         * @return The current property bag (the one modified by this operation), for chaining.
         */
        public DecimalFormatProperties CopyFrom(DecimalFormatProperties other)
        {
            return CopyFromImpl(other);
        }

        public override bool Equals(object other)
        {
            if (other == null)
                return false;
            if (this == other)
                return true;
            if (!(other is DecimalFormatProperties))
                return false;
            return EqualsImpl((DecimalFormatProperties)other);
        }

        /// BEGIN GETTERS/SETTERS ///

        public IDictionary<string, IDictionary<string, string>> CompactCustomData
        {
            get => compactCustomData;
            set => compactCustomData = value;
        }


        public CompactStyle? CompactStyle
        {
            get => compactStyle;
            set => compactStyle = value;
        }

        public Currency Currency
        {
            get => currency;
            set => currency = value;
        }

        public CurrencyPluralInfo CurrencyPluralInfo
        {
            get => currencyPluralInfo;
            set => currencyPluralInfo = value;
        }

        public CurrencyUsage? CurrencyUsage
        {
            get => currencyUsage;
            set => currencyUsage = value;
        }

        public bool DecimalPatternMatchRequired
        {
            get => decimalPatternMatchRequired;
            set => decimalPatternMatchRequired = value;
        }

        public bool DecimalSeparatorAlwaysShown
        {
            get => decimalSeparatorAlwaysShown;
            set => decimalSeparatorAlwaysShown = value;
        }

        public bool ExponentSignAlwaysShown
        {
            get => exponentSignAlwaysShown;
            set => exponentSignAlwaysShown = value;
        }

        public int FormatWidth
        {
            get => formatWidth;
            set => formatWidth = value;
        }

        public int GroupingSize
        {
            get => groupingSize;
            set => groupingSize = value;
        }

        public int MagnitudeMultiplier
        {
            get => magnitudeMultiplier;
            set => magnitudeMultiplier = value;
        }

        public BigMath.MathContext MathContext
        {
            get => mathContext;
            set => mathContext = value;
        }

        public int MaximumFractionDigits
        {
            get => maximumFractionDigits;
            set => maximumFractionDigits = value;
        }

        public int MaximumIntegerDigits
        {
            get => maximumIntegerDigits;
            set => maximumIntegerDigits = value;
        }

        public int MaximumSignificantDigits
        {
            get => maximumSignificantDigits;
            set => minimumSignificantDigits = value;
        }

        public int MinimumExponentDigits
        {
            get => minimumExponentDigits;
            set => minimumExponentDigits = value;
        }

        public int MinimumFractionDigits
        {
            get => minimumFractionDigits;
            set => minimumFractionDigits = value;
        }

        public int MinimumGroupingDigits
        {
            get => minimumGroupingDigits;
            set => minimumGroupingDigits = value;
        }

        public int MinimumIntegerDigits
        {
            get => minimumIntegerDigits;
            set => minimumIntegerDigits = value;
        }

        public int MinimumSignificantDigits
        {
            get => minimumSignificantDigits;
            set => minimumSignificantDigits = value;
        }

        public BigMath.BigDecimal Multiplier
        {
            get => multiplier;
            set => multiplier = value;
        }

        public string NegativePrefix
        {
            get => negativePrefix;
            set => negativePrefix = value;
        }

        public string NegativePrefixPattern
        {
            get => negativePrefixPattern;
            set => negativePrefixPattern = value;
        }

        public string NegativeSuffix
        {
            get => negativeSuffix;
            set => negativeSuffix = value;
        }

        public string NegativeSuffixPattern
        {
            get => negativeSuffixPattern;
            set => negativeSuffixPattern = value;
        }

        public Padder.PadPosition? PadPosition
        {
            get => padPosition;
            set => padPosition = value;
        }

        public string PadString
        {
            get => padString;
            set => padString = value;
        }

        public bool ParseCaseSensitive // ICU4N TODO: Should this be an enum in .NET?
        {
            get => parseCaseSensitive;
            set => parseCaseSensitive = value;
        }

        public GroupingMode? ParseGroupingMode
        {
            get => parseGroupingMode;
            set => parseGroupingMode = value;
        }

        public bool ParseIntegerOnly // ICU4N: Roughly corresponds to the NumberStyles.Integer enum value in .NET
        {
            get => parseIntegerOnly;
            set => parseIntegerOnly = value;
        }

        public ParseMode? ParseMode
        {
            get => parseMode;
            set => parseMode = value;
        }

        public bool ParseNoExponent
        {
            get => parseNoExponent;
            set => parseNoExponent = value;
        }

        public bool ParseToBigDecimal
        {
            get => parseToBigDecimal;
            set => parseToBigDecimal = value;
        }

        public PluralRules PluralRules
        {
            get => pluralRules;
            set => pluralRules = value;
        }

        public string PositivePrefix
        {
            get => positivePrefix;
            set => positivePrefix = value;
        }

        public string PositivePrefixPattern
        {
            get => positivePrefixPattern;
            set => positivePrefixPattern = value;
        }

        public string PositiveSuffix
        {
            get => positiveSuffix;
            set => positiveSuffix = value;
        }

        public string PositiveSuffixPattern
        {
            get => positiveSuffixPattern;
            set => positiveSuffixPattern = value;
        }

        public BigMath.BigDecimal RoundingIncrement
        {
            get => roundingIncrement;
            set => roundingIncrement = value;
        }

        public BigMath.RoundingMode? RoundingMode
        {
            get => roundingMode;
            set => roundingMode = value;
        }

        public int SecondaryGroupingSize
        {
            get => secondaryGroupingSize;
            set => secondaryGroupingSize = value;
        }

        public bool SignAlwaysShown
        {
            get => signAlwaysShown;
            set => signAlwaysShown = value;
        }

        public override int GetHashCode()
        {
            return GetHashCodeImpl();
        }

        // ICU4N TODO: Serialization

        //        /** Custom serialization: re-create object from serialized properties. */
        //        private void readObject(ObjectInputStream ois) throws IOException, ClassNotFoundException {
        //        readObjectImpl(ois);
        //    }

        //    /* package-private */
        //    void readObjectImpl(ObjectInputStream ois) throws IOException, ClassNotFoundException {
        //        ois.defaultReadObject();

        //        // Initialize to empty
        //        clear();

        //    // Extra int for possible future use
        //    ois.readInt();

        //        // 1) How many fields were serialized?
        //        int count = ois.readInt();

        //        // 2) Read each field by its name and value
        //        for (int i = 0; i<count; i++) {
        //            String name = (String)ois.readObject();
        //    Object value = ois.readObject();

        //    // Get the field reference
        //    Field field = null;
        //            try {
        //                field = DecimalFormatProperties.class.getDeclaredField(name);
        //} catch (NoSuchFieldException e)
        //{
        //    // The field name does not exist! Possibly corrupted serialization. Ignore this entry.
        //    continue;
        //}
        //catch (SecurityException e)
        //{
        //    // Should not happen
        //    throw new AssertionError(e);
        //}

        //// NOTE: If the type of a field were changed in the future, this would be the place to check:
        //// If the variable `value` is the old type, perform any conversions necessary.

        //// Save value into the field
        //try
        //{
        //    field.set(this, value);
        //}
        //catch (IllegalArgumentException e)
        //{
        //    // Should not happen
        //    throw new AssertionError(e);
        //}
        //catch (IllegalAccessException e)
        //{
        //    // Should not happen
        //    throw new AssertionError(e);
        //}
        //        }
        //    }

        //    /**
        //     * Specifies custom data to be used instead of CLDR data when constructing a CompactDecimalFormat. The argument
        //     * should be a map with the following structure:
        //     *
        //     * <pre>
        //     * {
        //     *   "1000": {
        //     *     "one": "0 thousand",
        //     *     "other": "0 thousand"
        //     *   },
        //     *   "10000": {
        //     *     "one": "00 thousand",
        //     *     "other": "00 thousand"
        //     *   },
        //     *   // ...
        //     * }
        //     * </pre>
        //     *
        //     * This API endpoint is used by the CLDR Survey Tool.
        //     *
        //     * @param compactCustomData
        //     *            A map with the above structure.
        //     * @return The property bag, for chaining.
        //     */
        //    public DecimalFormatProperties SetCompactCustomData(IDictionary<string, IDictionary<string, string>> compactCustomData)
        //{
        //    // TODO: compactCustomData is not immutable.
        //    this.compactCustomData = compactCustomData;
        //    return this;
        //}

        ///**
        // * Use compact decimal formatting with the specified {@link CompactStyle}. CompactStyle.SHORT produces output like
        // * "10K" in locale <em>en-US</em>, whereas CompactStyle.LONG produces output like "10 thousand" in that locale.
        // *
        // * @param compactStyle
        // *            The style of prefixes/suffixes to append.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setCompactStyle(CompactStyle compactStyle)
        //{
        //    this.compactStyle = compactStyle;
        //    return this;
        //}

        ///**
        // * Use the specified currency to substitute currency placeholders ('¤') in the pattern string.
        // *
        // * @param currency
        // *            The currency.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setCurrency(Currency currency)
        //{
        //    this.currency = currency;
        //    return this;
        //}

        ///**
        // * Use the specified {@link CurrencyPluralInfo} instance when formatting currency long names.
        // *
        // * @param currencyPluralInfo
        // *            The currency plural info object.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setCurrencyPluralInfo(CurrencyPluralInfo currencyPluralInfo)
        //{
        //    // TODO: In order to maintain immutability, we have to perform a clone here.
        //    // It would be better to just retire CurrencyPluralInfo entirely.
        //    if (currencyPluralInfo != null)
        //    {
        //        currencyPluralInfo = (CurrencyPluralInfo)currencyPluralInfo.clone();
        //    }
        //    this.currencyPluralInfo = currencyPluralInfo;
        //    return this;
        //}

        ///**
        // * Use the specified {@link CurrencyUsage} instance, which provides default rounding rules for the currency in two
        // * styles, CurrencyUsage.CASH and CurrencyUsage.STANDARD.
        // *
        // * <p>
        // * The CurrencyUsage specified here will not be used unless there is a currency placeholder in the pattern.
        // *
        // * @param currencyUsage
        // *            The currency usage. Defaults to CurrencyUsage.STANDARD.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setCurrencyUsage(CurrencyUsage currencyUsage)
        //{
        //    this.currencyUsage = currencyUsage;
        //    return this;
        //}

        ///**
        // * PARSING: Whether to require that the presence of decimal point matches the pattern. If a decimal point is not
        // * present, but the pattern contained a decimal point, parse will not succeed: null will be returned from
        // * <code>parse()</code>, and an error index will be set in the {@link ParsePosition}.
        // *
        // * @param decimalPatternMatchRequired
        // *            true to set an error if decimal is not present
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setDecimalPatternMatchRequired(boolean decimalPatternMatchRequired)
        //{
        //    this.decimalPatternMatchRequired = decimalPatternMatchRequired;
        //    return this;
        //}

        ///**
        // * Sets whether to always show the decimal point, even if the number doesn't require one. For example, if always
        // * show decimal is true, the number 123 would be formatted as "123." in locale <em>en-US</em>.
        // *
        // * @param alwaysShowDecimal
        // *            Whether to show the decimal point when it is optional.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setDecimalSeparatorAlwaysShown(boolean alwaysShowDecimal)
        //{
        //    this.decimalSeparatorAlwaysShown = alwaysShowDecimal;
        //    return this;
        //}

        ///**
        // * Sets whether to show the plus sign in the exponent part of numbers with a zero or positive exponent. For example,
        // * the number "1200" with the pattern "0.0E0" would be formatted as "1.2E+3" instead of "1.2E3" in <em>en-US</em>.
        // *
        // * @param exponentSignAlwaysShown
        // *            Whether to show the plus sign in positive exponents.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setExponentSignAlwaysShown(boolean exponentSignAlwaysShown)
        //{
        //    this.exponentSignAlwaysShown = exponentSignAlwaysShown;
        //    return this;
        //}

        ///**
        // * Sets the minimum width of the string output by the formatting pipeline. For example, if padding is enabled and
        // * paddingWidth is set to 6, formatting the number "3.14159" with the pattern "0.00" will result in "··3.14" if '·'
        // * is your padding string.
        // *
        // * <p>
        // * If the number is longer than your padding width, the number will display as if no padding width had been
        // * specified, which may result in strings longer than the padding width.
        // *
        // * <p>
        // * Width is counted in UTF-16 code units.
        // *
        // * @param paddingWidth
        // *            The output width.
        // * @return The property bag, for chaining.
        // * @see #setPadPosition
        // * @see #setPadString
        // */
        //public DecimalFormatProperties setFormatWidth(int paddingWidth)
        //{
        //    this.formatWidth = paddingWidth;
        //    return this;
        //}

        ///**
        // * Sets the number of digits between grouping separators. For example, the <em>en-US</em> locale uses a grouping
        // * size of 3, so the number 1234567 would be formatted as "1,234,567". For locales whose grouping sizes vary with
        // * magnitude, see {@link #setSecondaryGroupingSize(int)}.
        // *
        // * @param groupingSize
        // *            The primary grouping size.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setGroupingSize(int groupingSize)
        //{
        //    this.groupingSize = groupingSize;
        //    return this;
        //}

        ///**
        // * Multiply all numbers by this power of ten before formatting. Negative multipliers reduce the magnitude and make
        // * numbers smaller (closer to zero).
        // *
        // * @param magnitudeMultiplier
        // *            The number of powers of ten to scale.
        // * @return The property bag, for chaining.
        // * @see #setMultiplier
        // */
        //public DecimalFormatProperties setMagnitudeMultiplier(int magnitudeMultiplier)
        //{
        //    this.magnitudeMultiplier = magnitudeMultiplier;
        //    return this;
        //}

        ///**
        // * Sets the {@link MathContext} to be used during math and rounding operations. A MathContext encapsulates a
        // * RoundingMode and the number of significant digits in the output.
        // *
        // * @param mathContext
        // *            The math context to use when rounding is required.
        // * @return The property bag, for chaining.
        // * @see MathContext
        // * @see #setRoundingMode
        // */
        //public DecimalFormatProperties setMathContext(MathContext mathContext)
        //{
        //    this.mathContext = mathContext;
        //    return this;
        //}

        ///**
        // * Sets the maximum number of digits to display after the decimal point. If the number has fewer than this number of
        // * digits, the number will be rounded off using the rounding mode specified by
        // * {@link #setRoundingMode(RoundingMode)}. The pattern "#00.0#", for example, corresponds to 2 maximum fraction
        // * digits, and the number 456.789 would be formatted as "456.79" in locale <em>en-US</em> with the default rounding
        // * mode. Note that the number 456.999 would be formatted as "457.0" given the same configurations.
        // *
        // * @param maximumFractionDigits
        // *            The maximum number of fraction digits to output.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setMaximumFractionDigits(int maximumFractionDigits)
        //{
        //    this.maximumFractionDigits = maximumFractionDigits;
        //    return this;
        //}

        ///**
        // * Sets the maximum number of digits to display before the decimal point. If the number has more than this number of
        // * digits, the extra digits will be truncated. For example, if maximum integer digits is 2, and you attempt to
        // * format the number 1970, you will get "70" in locale <em>en-US</em>. It is not possible to specify the maximum
        // * integer digits using a pattern string, except in the special case of a scientific format pattern.
        // *
        // * @param maximumIntegerDigits
        // *            The maximum number of integer digits to output.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setMaximumIntegerDigits(int maximumIntegerDigits)
        //{
        //    this.maximumIntegerDigits = maximumIntegerDigits;
        //    return this;
        //}

        ///**
        // * Sets the maximum number of significant digits to display. The number of significant digits is equal to the number
        // * of digits counted from the leftmost nonzero digit through the rightmost nonzero digit; for example, the number
        // * "2010" has 3 significant digits. If the number has more significant digits than specified here, the extra
        // * significant digits will be rounded off using the rounding mode specified by
        // * {@link #setRoundingMode(RoundingMode)}. For example, if maximum significant digits is 3, the number 1234.56 will
        // * be formatted as "1230" in locale <em>en-US</em> with the default rounding mode.
        // *
        // * <p>
        // * If both maximum significant digits and maximum integer/fraction digits are set at the same time, the behavior is
        // * undefined.
        // *
        // * <p>
        // * The number of significant digits can be specified in a pattern string using the '@' character. For example, the
        // * pattern "@@#" corresponds to a minimum of 2 and a maximum of 3 significant digits.
        // *
        // * @param maximumSignificantDigits
        // *            The maximum number of significant digits to display.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setMaximumSignificantDigits(int maximumSignificantDigits)
        //{
        //    this.maximumSignificantDigits = maximumSignificantDigits;
        //    return this;
        //}

        ///**
        // * Sets the minimum number of digits to display in the exponent. For example, the number "1200" with the pattern
        // * "0.0E00", which has 2 exponent digits, would be formatted as "1.2E03" in <em>en-US</em>.
        // *
        // * @param minimumExponentDigits
        // *            The minimum number of digits to display in the exponent field.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setMinimumExponentDigits(int minimumExponentDigits)
        //{
        //    this.minimumExponentDigits = minimumExponentDigits;
        //    return this;
        //}

        ///**
        // * Sets the minimum number of digits to display after the decimal point. If the number has fewer than this number of
        // * digits, the number will be padded with zeros. The pattern "#00.0#", for example, corresponds to 1 minimum
        // * fraction digit, and the number 456 would be formatted as "456.0" in locale <em>en-US</em>.
        // *
        // * @param minimumFractionDigits
        // *            The minimum number of fraction digits to output.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setMinimumFractionDigits(int minimumFractionDigits)
        //{
        //    this.minimumFractionDigits = minimumFractionDigits;
        //    return this;
        //}

        ///**
        // * Sets the minimum number of digits required to be beyond the first grouping separator in order to enable grouping.
        // * For example, if the minimum grouping digits is 2, then 1234 would be formatted as "1234" but 12345 would be
        // * formatted as "12,345" in <em>en-US</em>. Note that 1234567 would still be formatted as "1,234,567", not
        // * "1234,567".
        // *
        // * @param minimumGroupingDigits
        // *            How many digits must appear before a grouping separator before enabling grouping.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setMinimumGroupingDigits(int minimumGroupingDigits)
        //{
        //    this.minimumGroupingDigits = minimumGroupingDigits;
        //    return this;
        //}

        ///**
        // * Sets the minimum number of digits to display before the decimal point. If the number has fewer than this number
        // * of digits, the number will be padded with zeros. The pattern "#00.0#", for example, corresponds to 2 minimum
        // * integer digits, and the number 5.3 would be formatted as "05.3" in locale <em>en-US</em>.
        // *
        // * @param minimumIntegerDigits
        // *            The minimum number of integer digits to output.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setMinimumIntegerDigits(int minimumIntegerDigits)
        //{
        //    this.minimumIntegerDigits = minimumIntegerDigits;
        //    return this;
        //}

        ///**
        // * Sets the minimum number of significant digits to display. If, after rounding to the number of significant digits
        // * specified by {@link #setMaximumSignificantDigits}, the number of remaining significant digits is less than the
        // * minimum, the number will be padded with zeros. For example, if minimum significant digits is 3, the number 5.8
        // * will be formatted as "5.80" in locale <em>en-US</em>. Note that minimum significant digits is relevant only when
        // * numbers have digits after the decimal point.
        // *
        // * <p>
        // * If both minimum significant digits and minimum integer/fraction digits are set at the same time, both values will
        // * be respected, and the one that results in the greater number of padding zeros will be used. For example,
        // * formatting the number 73 with 3 minimum significant digits and 2 minimum fraction digits will produce "73.00".
        // *
        // * <p>
        // * The number of significant digits can be specified in a pattern string using the '@' character. For example, the
        // * pattern "@@#" corresponds to a minimum of 2 and a maximum of 3 significant digits.
        // *
        // * @param minimumSignificantDigits
        // *            The minimum number of significant digits to display.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setMinimumSignificantDigits(int minimumSignificantDigits)
        //{
        //    this.minimumSignificantDigits = minimumSignificantDigits;
        //    return this;
        //}

        ///**
        // * Multiply all numbers by this amount before formatting.
        // *
        // * @param multiplier
        // *            The amount to multiply by.
        // * @return The property bag, for chaining.
        // * @see #setMagnitudeMultiplier
        // */
        //public DecimalFormatProperties setMultiplier(BigDecimal multiplier)
        //{
        //    this.multiplier = multiplier;
        //    return this;
        //}

        ///**
        // * Sets the prefix to prepend to negative numbers. The prefix will be interpreted literally. For example, if you set
        // * a negative prefix of <code>n</code>, then the number -123 will be formatted as "n123" in the locale
        // * <em>en-US</em>. Note that if the negative prefix is left unset, the locale's minus sign is used.
        // *
        // * <p>
        // * For more information on prefixes and suffixes, see {@link MutablePatternModifier}.
        // *
        // * @param negativePrefix
        // *            The CharSequence to prepend to negative numbers.
        // * @return The property bag, for chaining.
        // * @see #setNegativePrefixPattern
        // */
        //public DecimalFormatProperties setNegativePrefix(String negativePrefix)
        //{
        //    this.negativePrefix = negativePrefix;
        //    return this;
        //}

        ///**
        // * Sets the prefix to prepend to negative numbers. Locale-specific symbols will be substituted into the string
        // * according to Unicode Technical Standard #35 (LDML).
        // *
        // * <p>
        // * For more information on prefixes and suffixes, see {@link MutablePatternModifier}.
        // *
        // * @param negativePrefixPattern
        // *            The CharSequence to prepend to negative numbers after locale symbol substitutions take place.
        // * @return The property bag, for chaining.
        // * @see #setNegativePrefix
        // */
        //public DecimalFormatProperties setNegativePrefixPattern(String negativePrefixPattern)
        //{
        //    this.negativePrefixPattern = negativePrefixPattern;
        //    return this;
        //}

        ///**
        // * Sets the suffix to append to negative numbers. The suffix will be interpreted literally. For example, if you set
        // * a suffix prefix of <code>n</code>, then the number -123 will be formatted as "-123n" in the locale
        // * <em>en-US</em>. Note that the minus sign is prepended by default unless otherwise specified in either the pattern
        // * string or in one of the {@link #setNegativePrefix} methods.
        // *
        // * <p>
        // * For more information on prefixes and suffixes, see {@link MutablePatternModifier}.
        // *
        // * @param negativeSuffix
        // *            The CharSequence to append to negative numbers.
        // * @return The property bag, for chaining.
        // * @see #setNegativeSuffixPattern
        // */
        //public DecimalFormatProperties setNegativeSuffix(String negativeSuffix)
        //{
        //    this.negativeSuffix = negativeSuffix;
        //    return this;
        //}

        ///**
        // * Sets the suffix to append to negative numbers. Locale-specific symbols will be substituted into the string
        // * according to Unicode Technical Standard #35 (LDML).
        // *
        // * <p>
        // * For more information on prefixes and suffixes, see {@link MutablePatternModifier}.
        // *
        // * @param negativeSuffixPattern
        // *            The CharSequence to append to negative numbers after locale symbol substitutions take place.
        // * @return The property bag, for chaining.
        // * @see #setNegativeSuffix
        // */
        //public DecimalFormatProperties setNegativeSuffixPattern(String negativeSuffixPattern)
        //{
        //    this.negativeSuffixPattern = negativeSuffixPattern;
        //    return this;
        //}

        ///**
        // * Sets the location where the padding string is to be inserted to maintain the padding width: one of BEFORE_PREFIX,
        // * AFTER_PREFIX, BEFORE_SUFFIX, or AFTER_SUFFIX.
        // *
        // * <p>
        // * Must be used in conjunction with {@link #setFormatWidth}.
        // *
        // * @param paddingLocation
        // *            The output width.
        // * @return The property bag, for chaining.
        // * @see #setFormatWidth
        // */
        //public DecimalFormatProperties setPadPosition(PadPosition paddingLocation)
        //{
        //    this.padPosition = paddingLocation;
        //    return this;
        //}

        ///**
        // * Sets the string used for padding. The string should contain a single character or grapheme cluster.
        // *
        // * <p>
        // * Must be used in conjunction with {@link #setFormatWidth}.
        // *
        // * @param paddingString
        // *            The padding string. Defaults to an ASCII space (U+0020).
        // * @return The property bag, for chaining.
        // * @see #setFormatWidth
        // */
        //public DecimalFormatProperties setPadString(String paddingString)
        //{
        //    this.padString = paddingString;
        //    return this;
        //}

        ///**
        // * Whether to require cases to match when parsing strings; default is true. Case sensitivity applies to prefixes,
        // * suffixes, the exponent separator, the symbol "NaN", and the infinity symbol. Grouping separators, decimal
        // * separators, and padding are always case-sensitive. Currencies are always case-insensitive.
        // *
        // * <p>
        // * This setting is ignored in fast mode. In fast mode, strings are always compared in a case-sensitive way.
        // *
        // * @param parseCaseSensitive
        // *            true to be case-sensitive when parsing; false to allow any case.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setParseCaseSensitive(boolean parseCaseSensitive)
        //{
        //    this.parseCaseSensitive = parseCaseSensitive;
        //    return this;
        //}

        ///**
        // * Sets the strategy used during parsing when a code point needs to be interpreted as either a decimal separator or
        // * a grouping separator.
        // *
        // * <p>
        // * The comma, period, space, and apostrophe have different meanings in different locales. For example, in
        // * <em>en-US</em> and most American locales, the period is used as a decimal separator, but in <em>es-PY</em> and
        // * most European locales, it is used as a grouping separator.
        // *
        // * <p>
        // * Suppose you are in <em>fr-FR</em> the parser encounters the string "1.234". In <em>fr-FR</em>, the grouping is a
        // * space and the decimal is a comma. The <em>grouping mode</em> is a mechanism to let you specify whether to accept
        // * the string as 1234 (GroupingMode.DEFAULT) or whether to reject it since the separators don't match
        // * (GroupingMode.RESTRICTED).
        // *
        // * <p>
        // * When resolving grouping separators, it is the <em>equivalence class</em> of separators that is considered. For
        // * example, a period is seen as equal to a fixed set of other period-like characters.
        // *
        // * @param parseGroupingMode
        // *            The {@link GroupingMode} to use; either DEFAULT or RESTRICTED.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setParseGroupingMode(GroupingMode parseGroupingMode)
        //{
        //    this.parseGroupingMode = parseGroupingMode;
        //    return this;
        //}

        ///**
        // * Whether to ignore the fractional part of numbers. For example, parses "123.4" to "123" instead of "123.4".
        // *
        // * @param parseIntegerOnly
        // *            true to parse integers only; false to parse integers with their fraction parts
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setParseIntegerOnly(boolean parseIntegerOnly)
        //{
        //    this.parseIntegerOnly = parseIntegerOnly;
        //    return this;
        //}

        ///**
        // * Controls certain rules for how strict this parser is when reading strings. See {@link ParseMode#LENIENT} and
        // * {@link ParseMode#STRICT}.
        // *
        // * @param parseMode
        // *            Either {@link ParseMode#LENIENT} or {@link ParseMode#STRICT}.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setParseMode(ParseMode parseMode)
        //{
        //    this.parseMode = parseMode;
        //    return this;
        //}

        ///**
        // * Whether to ignore the exponential part of numbers. For example, parses "123E4" to "123" instead of "1230000".
        // *
        // * @param parseNoExponent
        // *            true to ignore exponents; false to parse them.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setParseNoExponent(boolean parseNoExponent)
        //{
        //    this.parseNoExponent = parseNoExponent;
        //    return this;
        //}

        ///**
        // * Whether to always return a BigDecimal from {@link Parse#parse} and all other parse methods. By default, a Long or
        // * a BigInteger are returned when possible.
        // *
        // * @param parseToBigDecimal
        // *            true to always return a BigDecimal; false to return a Long or a BigInteger when possible.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setParseToBigDecimal(boolean parseToBigDecimal)
        //{
        //    this.parseToBigDecimal = parseToBigDecimal;
        //    return this;
        //}

        ///**
        // * Sets the PluralRules object to use instead of the default for the locale.
        // *
        // * @param pluralRules
        // *            The object to reference.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setPluralRules(PluralRules pluralRules)
        //{
        //    this.pluralRules = pluralRules;
        //    return this;
        //}

        ///**
        // * Sets the prefix to prepend to positive numbers. The prefix will be interpreted literally. For example, if you set
        // * a positive prefix of <code>p</code>, then the number 123 will be formatted as "p123" in the locale
        // * <em>en-US</em>.
        // *
        // * <p>
        // * For more information on prefixes and suffixes, see {@link MutablePatternModifier}.
        // *
        // * @param positivePrefix
        // *            The CharSequence to prepend to positive numbers.
        // * @return The property bag, for chaining.
        // * @see #setPositivePrefixPattern
        // */
        //public DecimalFormatProperties setPositivePrefix(String positivePrefix)
        //{
        //    this.positivePrefix = positivePrefix;
        //    return this;
        //}

        ///**
        // * Sets the prefix to prepend to positive numbers. Locale-specific symbols will be substituted into the string
        // * according to Unicode Technical Standard #35 (LDML).
        // *
        // * <p>
        // * For more information on prefixes and suffixes, see {@link MutablePatternModifier}.
        // *
        // * @param positivePrefixPattern
        // *            The CharSequence to prepend to positive numbers after locale symbol substitutions take place.
        // * @return The property bag, for chaining.
        // * @see #setPositivePrefix
        // */
        //public DecimalFormatProperties setPositivePrefixPattern(String positivePrefixPattern)
        //{
        //    this.positivePrefixPattern = positivePrefixPattern;
        //    return this;
        //}

        ///**
        // * Sets the suffix to append to positive numbers. The suffix will be interpreted literally. For example, if you set
        // * a positive suffix of <code>p</code>, then the number 123 will be formatted as "123p" in the locale
        // * <em>en-US</em>.
        // *
        // * <p>
        // * For more information on prefixes and suffixes, see {@link MutablePatternModifier}.
        // *
        // * @param positiveSuffix
        // *            The CharSequence to append to positive numbers.
        // * @return The property bag, for chaining.
        // * @see #setPositiveSuffixPattern
        // */
        //public DecimalFormatProperties setPositiveSuffix(String positiveSuffix)
        //{
        //    this.positiveSuffix = positiveSuffix;
        //    return this;
        //}

        ///**
        // * Sets the suffix to append to positive numbers. Locale-specific symbols will be substituted into the string
        // * according to Unicode Technical Standard #35 (LDML).
        // *
        // * <p>
        // * For more information on prefixes and suffixes, see {@link MutablePatternModifier}.
        // *
        // * @param positiveSuffixPattern
        // *            The CharSequence to append to positive numbers after locale symbol substitutions take place.
        // * @return The property bag, for chaining.
        // * @see #setPositiveSuffix
        // */
        //public DecimalFormatProperties setPositiveSuffixPattern(String positiveSuffixPattern)
        //{
        //    this.positiveSuffixPattern = positiveSuffixPattern;
        //    return this;
        //}

        ///**
        // * Sets the increment to which to round numbers. For example, with a rounding interval of 0.05, the number 11.17
        // * would be formatted as "11.15" in locale <em>en-US</em> with the default rounding mode.
        // *
        // * <p>
        // * You can use either a rounding increment or significant digits, but not both at the same time.
        // *
        // * <p>
        // * The rounding increment can be specified in a pattern string. For example, the pattern "#,##0.05" corresponds to a
        // * rounding interval of 0.05 with 1 minimum integer digit and a grouping size of 3.
        // *
        // * @param roundingIncrement
        // *            The interval to which to round.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setRoundingIncrement(BigDecimal roundingIncrement)
        //{
        //    this.roundingIncrement = roundingIncrement;
        //    return this;
        //}

        ///**
        // * Sets the rounding mode, which determines under which conditions extra decimal places are rounded either up or
        // * down. See {@link RoundingMode} for details on the choices of rounding mode. The default if not set explicitly is
        // * {@link RoundingMode#HALF_EVEN}.
        // *
        // * <p>
        // * This setting is ignored if {@link #setMathContext} is used.
        // *
        // * @param roundingMode
        // *            The rounding mode to use when rounding is required.
        // * @return The property bag, for chaining.
        // * @see RoundingMode
        // * @see #setMathContext
        // */
        //public DecimalFormatProperties setRoundingMode(RoundingMode roundingMode)
        //{
        //    this.roundingMode = roundingMode;
        //    return this;
        //}

        ///**
        // * Sets the number of digits between grouping separators higher than the least-significant grouping separator. For
        // * example, the locale <em>hi</em> uses a primary grouping size of 3 and a secondary grouping size of 2, so the
        // * number 1234567 would be formatted as "12,34,567".
        // *
        // * <p>
        // * The two levels of grouping separators can be specified in the pattern string. For example, the <em>hi</em>
        // * locale's default decimal format pattern is "#,##,##0.###".
        // *
        // * @param secondaryGroupingSize
        // *            The secondary grouping size.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setSecondaryGroupingSize(int secondaryGroupingSize)
        //{
        //    this.secondaryGroupingSize = secondaryGroupingSize;
        //    return this;
        //}

        ///**
        // * Sets whether to always display of a plus sign on positive numbers.
        // *
        // * <p>
        // * If the location of the negative sign is specified by the decimal format pattern (or by the negative prefix/suffix
        // * pattern methods), a plus sign is substituted into that location, in accordance with Unicode Technical Standard
        // * #35 (LDML) section 3.2.1. Otherwise, the plus sign is prepended to the number. For example, if the decimal format
        // * pattern <code>#;#-</code> is used, then formatting 123 would result in "123+" in the locale <em>en-US</em>.
        // *
        // * <p>
        // * This method should be used <em>instead of</em> setting the positive prefix/suffix. The behavior is undefined if
        // * alwaysShowPlusSign is set but the positive prefix/suffix already contains a plus sign.
        // *
        // * @param signAlwaysShown
        // *            Whether positive numbers should display a plus sign.
        // * @return The property bag, for chaining.
        // */
        //public DecimalFormatProperties setSignAlwaysShown(boolean signAlwaysShown)
        //{
        //    this.signAlwaysShown = signAlwaysShown;
        //    return this;
        //}

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("<Properties");
            ToStringBare(result);
            result.Append(">");
            return result.ToString();
        }

        /**
         * Appends a string containing properties that differ from the default, but without being surrounded by
         * &lt;Properties&gt;.
         */
        public virtual void ToStringBare(StringBuilder result)
        {
            FieldInfo[] fields = typeof(DecimalFormatProperties).GetFields();
            foreach (FieldInfo field in fields)
            {
                Object myValue, defaultValue;
                try
                {
                    myValue = field.GetValue(this);
                    defaultValue = field.GetValue(DEFAULT);
                }
                catch (ArgumentException e)
                {
                    Trace.TraceError(e.ToString());
                    continue;
                }
                catch (FieldAccessException e)
                {
                    Trace.TraceError(e.ToString());
                    continue;
                }
                if (myValue == null && defaultValue == null)
                {
                    //continue;
                    // Intentionally empty
                }
                else if (myValue == null || defaultValue == null)
                {
                    result.Append(" " + field.Name + ":" + myValue);
                }
                else if (!myValue.Equals(defaultValue))
                {
                    result.Append(" " + field.Name + ":" + myValue);
                }
            }
        }

        // ICU4N TODO: Serialization
        //    /**
        //     * Custom serialization: save fields along with their name, so that fields can be easily added in the future in any
        //     * order. Only save fields that differ from their default value.
        //     */
        //    private void writeObject(ObjectOutputStream oos) throws IOException
        //{
        //    writeObjectImpl(oos);
        //}

        ///* package-private */
        //void writeObjectImpl(ObjectOutputStream oos) throws IOException
        //{
        //    oos.defaultWriteObject();

        //    // Extra int for possible future use
        //    oos.writeInt(0);

        //    ArrayList<Field> fieldsToSerialize = new ArrayList<Field>();
        //ArrayList<Object> valuesToSerialize = new ArrayList<Object>();
        //Field[] fields = DecimalFormatProperties.class.getDeclaredFields();
        //for (Field field : fields)
        //{
        //    if (Modifier.isStatic(field.getModifiers()))
        //    {
        //        continue;
        //    }
        //    try
        //    {
        //        Object myValue = field.get(this);
        //        if (myValue == null)
        //        {
        //            // All *Object* values default to null; no need to serialize.
        //            continue;
        //        }
        //        Object defaultValue = field.get(DEFAULT);
        //        if (!myValue.equals(defaultValue))
        //        {
        //            fieldsToSerialize.add(field);
        //            valuesToSerialize.add(myValue);
        //        }
        //    }
        //    catch (IllegalArgumentException e)
        //    {
        //        // Should not happen
        //        throw new AssertionError(e);
        //    }
        //    catch (IllegalAccessException e)
        //    {
        //        // Should not happen
        //        throw new AssertionError(e);
        //    }
        //}

        //// 1) How many fields are to be serialized?
        //int count = fieldsToSerialize.size();
        //oos.writeInt(count);

        //// 2) Write each field with its name and value
        //for (int i = 0; i < count; i++)
        //{
        //    Field field = fieldsToSerialize.get(i);
        //    Object value = valuesToSerialize.get(i);
        //    oos.writeObject(field.getName());
        //    oos.writeObject(value);
        //}
        //    }
    }
}
