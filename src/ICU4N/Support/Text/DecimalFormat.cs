using J2N.Globalization;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Support.Text
{
    internal class DecimalFormat : NumberFormat
    {

        //private static final long serialVersionUID = 864413376551465018L;

        public NumberStyle NumberStyle { get; set; }

        [NonSerialized]
    private bool parseBigDecimal = false;

        //    [NonSerialized]
        //private DecimalFormatSymbols symbols;

        //    [NonSerialized]
        //private com.ibm.icu.text.DecimalFormat dform;

        //[NonSerialized]
        //private com.ibm.icu.text.DecimalFormatSymbols icuSymbols;

    private const int CURRENT_SERIAL_VERTION = 3;

        [NonSerialized]
        private int serialVersionOnStream = 3;

        private string pattern = null;

    /**
     * Constructs a new {@code DecimalFormat} for formatting and parsing numbers
     * for the default locale.
     */
    public DecimalFormat()
            : base(CultureInfo.CurrentCulture)
    {
        //Locale locale = Locale.getDefault();
        //icuSymbols = new com.ibm.icu.text.DecimalFormatSymbols(locale);
        //symbols = new DecimalFormatSymbols(locale);
        //dform = new com.ibm.icu.text.DecimalFormat();

        //super.setMaximumFractionDigits(dform.getMaximumFractionDigits());
        //super.setMaximumIntegerDigits(dform.getMaximumIntegerDigits());
        //super.setMinimumFractionDigits(dform.getMinimumFractionDigits());
        //super.setMinimumIntegerDigits(dform.getMinimumIntegerDigits());
    }

    /**
     * Constructs a new {@code DecimalFormat} using the specified non-localized
     * pattern and the {@code DecimalFormatSymbols} for the default Locale.
     * 
     * @param pattern
     *            the non-localized pattern.
     * @throws IllegalArgumentException
     *            if the pattern cannot be parsed.
     */
    public DecimalFormat(string pattern)
             : base(CultureInfo.CurrentCulture)
        {
            this.pattern = pattern;
            //Locale locale = Locale.getDefault();
            //icuSymbols = new com.ibm.icu.text.DecimalFormatSymbols(locale);
            //symbols = new DecimalFormatSymbols(locale);
            //dform = new com.ibm.icu.text.DecimalFormat(pattern, icuSymbols);

            //super.setMaximumFractionDigits(dform.getMaximumFractionDigits());
            //super.setMaximumIntegerDigits(dform.getMaximumIntegerDigits());
            //super.setMinimumFractionDigits(dform.getMinimumFractionDigits());
            //super.setMinimumIntegerDigits(dform.getMinimumIntegerDigits());
        }

        /**
         * Constructs a new {@code DecimalFormat} using the specified non-localized
         * pattern and {@code DecimalFormatSymbols}.
         * 
         * @param pattern
         *            the non-localized pattern.
         * @param value
         *            the DecimalFormatSymbols.
         * @throws IllegalArgumentException
         *            if the pattern cannot be parsed.
         */
        public DecimalFormat(string pattern, IFormatProvider provider)
            : base(provider ?? CultureInfo.CurrentCulture)
        {
            this.pattern = pattern;
        }
        //public DecimalFormat(String pattern, DecimalFormatSymbols value)
        //{
        //    symbols = (DecimalFormatSymbols)value.clone();
        //    Locale locale = symbols.getLocale();
        //    icuSymbols = new com.ibm.icu.text.DecimalFormatSymbols(locale);
        //    copySymbols(icuSymbols, symbols);

        //    dform = new com.ibm.icu.text.DecimalFormat(pattern, icuSymbols);

        //    super.setMaximumFractionDigits(dform.getMaximumFractionDigits());
        //    super.setMaximumIntegerDigits(dform.getMaximumIntegerDigits());
        //    super.setMinimumFractionDigits(dform.getMinimumFractionDigits());
        //    super.setMinimumIntegerDigits(dform.getMinimumIntegerDigits());
        //}

    //    DecimalFormat(String pattern, DecimalFormatSymbols value, com.ibm.icu.text.DecimalFormat icuFormat)
    //{
    //    symbols = value;
    //    icuSymbols = value.getIcuSymbols();
    //    dform = icuFormat;

    //    super.setMaximumFractionDigits(dform.getMaximumFractionDigits());
    //    super.setMaximumIntegerDigits(dform.getMaximumIntegerDigits());
    //    super.setMinimumFractionDigits(dform.getMinimumFractionDigits());
    //    super.setMinimumIntegerDigits(dform.getMinimumIntegerDigits());
    //}

    /////**
    //// * Changes the pattern of this decimal format to the specified pattern which
    //// * uses localized pattern characters.
    //// * 
    //// * @param pattern
    //// *            the localized pattern.
    //// * @throws IllegalArgumentException
    //// *            if the pattern cannot be parsed.
    //// */
    ////public void applyLocalizedPattern(String pattern)
    ////{
    ////    dform.applyLocalizedPattern(pattern);
    ////    super.setMaximumFractionDigits(dform.getMaximumFractionDigits());
    ////    super.setMaximumIntegerDigits(dform.getMaximumIntegerDigits());
    ////    super.setMinimumFractionDigits(dform.getMinimumFractionDigits());
    ////    super.setMinimumIntegerDigits(dform.getMinimumIntegerDigits());
    ////}

    /**
     * Changes the pattern of this decimal format to the specified pattern which
     * uses non-localized pattern characters.
     * 
     * @param pattern
     *            the non-localized pattern.
     * @throws IllegalArgumentException
     *            if the pattern cannot be parsed.
     */
    public void ApplyPattern(string pattern)
        {
            this.pattern = pattern;
        //dform.applyPattern(pattern);
        //super.setMaximumFractionDigits(dform.getMaximumFractionDigits());
        //super.setMaximumIntegerDigits(dform.getMaximumIntegerDigits());
        //super.setMinimumFractionDigits(dform.getMinimumFractionDigits());
        //super.setMinimumIntegerDigits(dform.getMinimumIntegerDigits());
        }

    /**
     * Returns a new instance of {@code DecimalFormat} with the same pattern and
     * properties as this decimal format.
     * 
     * @return a shallow copy of this decimal format.
     * @see java.lang.Cloneable
     */
    public override object Clone()
    {
        DecimalFormat clone = (DecimalFormat)base.Clone();
        //clone.dform = (com.ibm.icu.text.DecimalFormat)dform.clone();
        //clone.symbols = (DecimalFormatSymbols)symbols.clone();
        return clone;
    }

    /**
     * Compares the specified object to this decimal format and indicates if
     * they are equal. In order to be equal, {@code object} must be an instance
     * of {@code DecimalFormat} with the same pattern and properties.
     * 
     * @param object
     *            the object to compare with this object.
     * @return {@code true} if the specified object is equal to this decimal
     *         format; {@code false} otherwise.
     * @see #hashCode
     */
    public override bool Equals(object obj)
    {
        if (this == obj)
        {
            return true;
        }
        if (obj is null || !(obj is DecimalFormat format)) {
            return false;
        }
            return FormatProvider is null ? format.FormatProvider is null : FormatProvider.Equals(format.FormatProvider)
                    && pattern.Equals(pattern);

        //return (this.dform == null ? format.dform == null : this.dform
        //        .equals(format.dform));
    }

    /**
     * Formats the specified object using the rules of this decimal format and
     * returns an {@code AttributedCharacterIterator} with the formatted number
     * and attributes.
     * 
     * @param object
     *            the object to format.
     * @return an AttributedCharacterIterator with the formatted number and
     *         attributes.
     * @throws IllegalArgumentException
     *             if {@code object} cannot be formatted by this format.
     * @throws NullPointerException
     *             if {@code object} is {@code null}.
     */
    public override AttributedCharacterIterator FormatToCharacterIterator(object obj)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }
            throw new NotImplementedException();
        //return dform.formatToCharacterIterator(obj);
    }

    /**
     * Formats the specified double value as a string using the pattern of this
     * decimal format and appends the string to the specified string buffer.
     * <p>
     * If the {@code field} member of {@code position} contains a value
     * specifying a format field, then its {@code beginIndex} and
     * {@code endIndex} members will be updated with the position of the first
     * occurrence of this field in the formatted text.
     *
     * @param value
     *            the double to format.
     * @param buffer
     *            the target string buffer to append the formatted double value
     *            to.
     * @param position
     *            on input: an optional alignment field; on output: the offsets
     *            of the alignment field in the formatted text.
     * @return the string buffer.
     */
    public override StringBuffer Format(double value, StringBuffer buffer,
            FieldPosition position)
    {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            //return dform.format(value, buffer, position);
            return buffer.Append(J2N.Numerics.Double.ToString(value, pattern, FormatProvider));
    }

        /**
 * Formats the specified double value as a string using the pattern of this
 * decimal format and appends the string to the specified string buffer.
 * <p>
 * If the {@code field} member of {@code position} contains a value
 * specifying a format field, then its {@code beginIndex} and
 * {@code endIndex} members will be updated with the position of the first
 * occurrence of this field in the formatted text.
 *
 * @param value
 *            the double to format.
 * @param buffer
 *            the target string buffer to append the formatted double value
 *            to.
 * @param position
 *            on input: an optional alignment field; on output: the offsets
 *            of the alignment field in the formatted text.
 * @return the string buffer.
 */
        public override StringBuffer Format(float value, StringBuffer buffer,
                FieldPosition position)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            //return dform.format(value, buffer, position);
            return buffer.Append(J2N.Numerics.Single.ToString(value, pattern, FormatProvider));
        }

        /**
 * Formats the specified double value as a string using the pattern of this
 * decimal format and appends the string to the specified string buffer.
 * <p>
 * If the {@code field} member of {@code position} contains a value
 * specifying a format field, then its {@code beginIndex} and
 * {@code endIndex} members will be updated with the position of the first
 * occurrence of this field in the formatted text.
 *
 * @param value
 *            the double to format.
 * @param buffer
 *            the target string buffer to append the formatted double value
 *            to.
 * @param position
 *            on input: an optional alignment field; on output: the offsets
 *            of the alignment field in the formatted text.
 * @return the string buffer.
 */
        public override StringBuffer Format(decimal value, StringBuffer buffer,
                FieldPosition position)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            //return dform.format(value, buffer, position);
            return buffer.Append(value.ToString(pattern, FormatProvider));
        }

        /**
         * Formats the specified long value as a string using the pattern of this
         * decimal format and appends the string to the specified string buffer.
         * <p>
         * If the {@code field} member of {@code position} contains a value
         * specifying a format field, then its {@code beginIndex} and
         * {@code endIndex} members will be updated with the position of the first
         * occurrence of this field in the formatted text.
         *
         * @param value
         *            the long to format.
         * @param buffer
         *            the target string buffer to append the formatted long value
         *            to.
         * @param position
         *            on input: an optional alignment field; on output: the offsets
         *            of the alignment field in the formatted text.
         * @return the string buffer.
         */
        public override StringBuffer Format(long value, StringBuffer buffer,
            FieldPosition position)
    {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            //return dform.format(value, buffer, position);
            return buffer.Append(J2N.Numerics.Int64.ToString(value, pattern, FormatProvider));
        }

    /**
     * Formats the specified object as a string using the pattern of this
     * decimal format and appends the string to the specified string buffer.
     * <p>
     * If the {@code field} member of {@code position} contains a value
     * specifying a format field, then its {@code beginIndex} and
     * {@code endIndex} members will be updated with the position of the first
     * occurrence of this field in the formatted text.
     *
     * @param number
     *            the object to format.
     * @param toAppendTo
     *            the target string buffer to append the formatted number to.
     * @param pos
     *            on input: an optional alignment field; on output: the offsets
     *            of the alignment field in the formatted text.
     * @return the string buffer.
     * @throws IllegalArgumentException
     *             if {@code number} is not an instance of {@code Number}.
     * @throws NullPointerException
     *             if {@code toAppendTo} or {@code pos} is {@code null}.
     */
    public override sealed StringBuffer Format(object number, StringBuffer buffer,
            FieldPosition pos)
    {
            return base.Format(number, buffer, pos);

        //    if (toAppendTo is null)
        //        throw new ArgumentNullException(nameof(toAppendTo));

        //    if (!(number is J2N.Numerics.Number)) {
        //    throw new ArgumentException();
        //}
        //if (toAppendTo == null || pos == null)
        //{
        //    throw new NullPointerException();
        //}
        //if (number instanceof BigInteger || number instanceof BigDecimal) {
        //    return dform.format(number, toAppendTo, pos);
        //}
        //return super.format(number, toAppendTo, pos);
    }

    /////**
    //// * Returns the {@code DecimalFormatSymbols} used by this decimal format.
    //// * 
    //// * @return a copy of the {@code DecimalFormatSymbols} used by this decimal
    //// *         format.
    //// */
    ////public DecimalFormatSymbols getDecimalFormatSymbols()
    ////{
    ////    return (DecimalFormatSymbols)symbols.clone();
    ////}

    /////**
    //// * Returns the currency used by this decimal format.
    //// * 
    //// * @return the currency used by this decimal format.
    //// * @see DecimalFormatSymbols#getCurrency()
    //// */
    ////@Override
    ////public Currency getCurrency()
    ////{
    ////    final com.ibm.icu.util.Currency cur = dform.getCurrency();
    ////    final String code = (cur == null) ? "XXX" : cur.getCurrencyCode(); //$NON-NLS-1$

    ////    return Currency.getInstance(code);
    ////}

    /////**
    //// * Returns the number of digits grouped together by the grouping separator.
    //// * This only allows to get the primary grouping size. There is no API to get
    //// * the secondary grouping size.
    //// *
    //// * @return the number of digits grouped together.
    //// */
    ////public int getGroupingSize()
    ////{
    ////    return dform.getGroupingSize();
    ////}

    /////**
    //// * Returns the multiplier which is applied to the number before formatting
    //// * or after parsing.
    //// * 
    //// * @return the multiplier.
    //// */
    ////public int getMultiplier()
    ////{
    ////    return dform.getMultiplier();
    ////}

    /////**
    //// * Returns the prefix which is formatted or parsed before a negative number.
    //// * 
    //// * @return the negative prefix.
    //// */
    ////public String getNegativePrefix()
    ////{
    ////    return dform.getNegativePrefix();
    ////}

    /////**
    //// * Returns the suffix which is formatted or parsed after a negative number.
    //// * 
    //// * @return the negative suffix.
    //// */
    ////public String getNegativeSuffix()
    ////{
    ////    return dform.getNegativeSuffix();
    ////}

    /////**
    //// * Returns the prefix which is formatted or parsed before a positive number.
    //// * 
    //// * @return the positive prefix.
    //// */
    ////public String getPositivePrefix()
    ////{
    ////    return dform.getPositivePrefix();
    ////}

    /////**
    //// * Returns the suffix which is formatted or parsed after a positive number.
    //// * 
    //// * @return the positive suffix.
    //// */
    ////public String getPositiveSuffix()
    ////{
    ////    return dform.getPositiveSuffix();
    ////}

    public override int GetHashCode()
    {
            return FormatProvider?.GetHashCode() ?? 0 ^ pattern?.GetHashCode() ?? 0;
        //return dform.hashCode();
    }

    /////**
    //// * Indicates whether the decimal separator is shown when there are no
    //// * fractional digits.
    //// * 
    //// * @return {@code true} if the decimal separator should always be formatted;
    //// *         {@code false} otherwise.
    //// */
    ////public boolean isDecimalSeparatorAlwaysShown()
    ////{
    ////    return dform.isDecimalSeparatorAlwaysShown();
    ////}

    /////**
    //// * This value indicates whether the return object of the parse operation is
    //// * of type {@code BigDecimal}. This value defaults to {@code false}.
    //// * 
    //// * @return {@code true} if parse always returns {@code BigDecimals},
    //// *         {@code false} if the type of the result is {@code Long} or
    //// *         {@code Double}.
    //// */
    ////public boolean isParseBigDecimal()
    ////{
    ////    return this.parseBigDecimal;
    ////}

    ///**
    // * Sets the flag that indicates whether numbers will be parsed as integers.
    // * When this decimal format is used for parsing and this value is set to
    // * {@code true}, then the resulting numbers will be of type
    // * {@code java.lang.Integer}. Special cases are NaN, positive and negative
    // * infinity, which are still returned as {@code java.lang.Double}.
    // * 
    // *
    // * @param value
    // *            {@code true} that the resulting numbers of parse operations
    // *            will be of type {@code java.lang.Integer} except for the
    // *            special cases described above.
    // */
    //@Override
    //public void setParseIntegerOnly(boolean value)
    //{
    //    // In this implementation, com.ibm.icu.text.DecimalFormat is wrapped to
    //    // fulfill most of the format and parse feature. And this method is
    //    // delegated to the wrapped instance of com.ibm.icu.text.DecimalFormat.

    //    dform.setParseIntegerOnly(value);
    //}

    ///**
    // * Indicates whether parsing with this decimal format will only
    // * return numbers of type {@code java.lang.Integer}.
    // * 
    // * @return {@code true} if this {@code DecimalFormat}'s parse method only
    // *         returns {@code java.lang.Integer}; {@code false} otherwise.
    // */
    //@Override
    //public boolean isParseIntegerOnly()
    //{
    //    return dform.isParseIntegerOnly();
    //}

    //private static final Double NEGATIVE_ZERO_DOUBLE = new Double(-0.0);

    /**
     * Parses a {@code Long} or {@code Double} from the specified string
     * starting at the index specified by {@code position}. If the string is
     * successfully parsed then the index of the {@code ParsePosition} is
     * updated to the index following the parsed text. On error, the index is
     * unchanged and the error index of {@code ParsePosition} is set to the
     * index where the error occurred.
     * 
     * @param string
     *            the string to parse.
     * @param position
     *            input/output parameter, specifies the start index in
     *            {@code string} from where to start parsing. If parsing is
     *            successful, it is updated with the index following the parsed
     *            text; on error, the index is unchanged and the error index is
     *            set to the index where the error occurred.
     * @return a {@code Long} or {@code Double} resulting from the parse or
     *         {@code null} if there is an error. The result will be a
     *         {@code Long} if the parsed number is an integer in the range of a
     *         long, otherwise the result is a {@code Double}. If
     *         {@code isParseBigDecimal} is {@code true} then it returns the
     *         result as a {@code BigDecimal}.
     */
    public override J2N.Numerics.Number Parse(string str, ParsePosition position)
    {
            return (J2N.Numerics.Double)J2N.Numerics.Double.Parse(str, NumberStyle, FormatProvider);

        //Number number = dform.parse(str, position);
        //if (null == number)
        //{
        //    return null;
        //}
        //if (this.isParseBigDecimal())
        //{
        //    if (number instanceof Long) {
        //        return new BigDecimal(number.longValue());
        //    }
        //    if ((number instanceof Double) && !((Double)number).isInfinite()
        //            && !((Double)number).isNaN()) {

        //        return new BigDecimal(number.doubleValue());
        //    }
        //    if (number instanceof BigInteger) {
        //        return new BigDecimal(number.doubleValue());
        //    }
        //    if (number instanceof com.ibm.icu.math.BigDecimal) {
        //        return new BigDecimal(number.toString());
        //    }
        //    return number;
        //}
        //if ((number instanceof com.ibm.icu.math.BigDecimal)
        //        || (number instanceof BigInteger)) {
        //    return new Double(number.doubleValue());
        //}

        //if (this.isParseIntegerOnly() && number.equals(NEGATIVE_ZERO_DOUBLE))
        //{
        //    return new Long(0);
        //}
        //return number;

    }

    /////**
    //// * Sets the {@code DecimalFormatSymbols} used by this decimal format.
    //// * 
    //// * @param value
    //// *            the {@code DecimalFormatSymbols} to set.
    //// */
    ////public void setDecimalFormatSymbols(DecimalFormatSymbols value)
    ////{
    ////    if (value != null)
    ////    {
    ////        symbols = (DecimalFormatSymbols)value.clone();
    ////        icuSymbols = dform.getDecimalFormatSymbols();
    ////        copySymbols(icuSymbols, symbols);
    ////        dform.setDecimalFormatSymbols(icuSymbols);
    ////    }
    ////}

    /////**
    //// * Sets the currency used by this decimal format. The min and max fraction
    //// * digits remain the same.
    //// * 
    //// * @param currency
    //// *            the currency this {@code DecimalFormat} should use.
    //// * @see DecimalFormatSymbols#setCurrency(Currency)
    //// */
    ////@Override
    ////public void setCurrency(Currency currency)
    ////{
    ////    dform.setCurrency(com.ibm.icu.util.Currency.getInstance(currency
    ////            .getCurrencyCode()));
    ////    symbols.setCurrency(currency);
    ////}

    /////**
    //// * Sets whether the decimal separator is shown when there are no fractional
    //// * digits.
    //// * 
    //// * @param value
    //// *            {@code true} if the decimal separator should always be
    //// *            formatted; {@code false} otherwise.
    //// */
    ////public void setDecimalSeparatorAlwaysShown(boolean value)
    ////{
    ////    dform.setDecimalSeparatorAlwaysShown(value);
    ////}

    /////**
    //// * Sets the number of digits grouped together by the grouping separator.
    //// * This only allows to set the primary grouping size; the secondary grouping
    //// * size can only be set with a pattern.
    //// *
    //// * @param value
    //// *            the number of digits grouped together.
    //// */
    ////public void setGroupingSize(int value)
    ////{
    ////    dform.setGroupingSize(value);
    ////}

    /////**
    //// * Sets whether or not grouping will be used in this format. Grouping
    //// * affects both parsing and formatting.
    //// * 
    //// * @param value
    //// *            {@code true} if grouping is used; {@code false} otherwise.
    //// */
    ////@Override
    ////public void setGroupingUsed(boolean value)
    ////{
    ////    dform.setGroupingUsed(value);
    ////}

    /////**
    //// * Indicates whether grouping will be used in this format.
    //// * 
    //// * @return {@code true} if grouping is used; {@code false} otherwise.
    //// */
    ////@Override
    ////public boolean isGroupingUsed()
    ////{
    ////    return dform.isGroupingUsed();
    ////}

    /////**
    //// * Sets the maximum number of fraction digits that are printed when
    //// * formatting numbers other than {@code BigDecimal} and {@code BigInteger}.
    //// * If the maximum is less than the number of fraction digits, the least
    //// * significant digits are truncated. If the value passed is bigger than 340
    //// * then it is replaced by 340. If the value passed is negative then it is
    //// * replaced by 0.
    //// * 
    //// * @param value
    //// *            the maximum number of fraction digits.
    //// */
    ////@Override
    ////public void setMaximumFractionDigits(int value)
    ////{
    ////    super.setMaximumFractionDigits(value);
    ////    dform.setMaximumFractionDigits(value);
    ////}

    /////**
    //// * Sets the maximum number of integer digits that are printed when
    //// * formatting numbers other than {@code BigDecimal} and {@code BigInteger}.
    //// * If the maximum is less than the number of integer digits, the most
    //// * significant digits are truncated. If the value passed is bigger than 309
    //// * then it is replaced by 309. If the value passed is negative then it is
    //// * replaced by 0.
    //// * 
    //// * @param value
    //// *            the maximum number of integer digits.
    //// */
    ////@Override
    ////public void setMaximumIntegerDigits(int value)
    ////{
    ////    super.setMaximumIntegerDigits(value);
    ////    dform.setMaximumIntegerDigits(value);
    ////}

    /////**
    //// * Sets the minimum number of fraction digits that are printed when
    //// * formatting numbers other than {@code BigDecimal} and {@code BigInteger}.
    //// * If the value passed is bigger than 340 then it is replaced by 340. If the
    //// * value passed is negative then it is replaced by 0.
    //// * 
    //// * @param value
    //// *            the minimum number of fraction digits.
    //// */
    ////@Override
    ////public void setMinimumFractionDigits(int value)
    ////{
    ////    super.setMinimumFractionDigits(value);
    ////    dform.setMinimumFractionDigits(value);
    ////}

    /////**
    //// * Sets the minimum number of integer digits that are printed when
    //// * formatting numbers other than {@code BigDecimal} and {@code BigInteger}.
    //// * If the value passed is bigger than 309 then it is replaced by 309. If the
    //// * value passed is negative then it is replaced by 0.
    //// * 
    //// * @param value
    //// *            the minimum number of integer digits.
    //// */
    ////@Override
    ////public void setMinimumIntegerDigits(int value)
    ////{
    ////    super.setMinimumIntegerDigits(value);
    ////    dform.setMinimumIntegerDigits(value);
    ////}

    /////**
    //// * Sets the multiplier which is applied to the number before formatting or
    //// * after parsing.
    //// * 
    //// * @param value
    //// *            the multiplier.
    //// */
    ////public void setMultiplier(int value)
    ////{
    ////    dform.setMultiplier(value);
    ////}

    /////**
    //// * Sets the prefix which is formatted or parsed before a negative number.
    //// * 
    //// * @param value
    //// *            the negative prefix.
    //// */
    ////public void setNegativePrefix(String value)
    ////{
    ////    dform.setNegativePrefix(value);
    ////}

    /////**
    //// * Sets the suffix which is formatted or parsed after a negative number.
    //// * 
    //// * @param value
    //// *            the negative suffix.
    //// */
    ////public void setNegativeSuffix(String value)
    ////{
    ////    dform.setNegativeSuffix(value);
    ////}

    /////**
    //// * Sets the prefix which is formatted or parsed before a positive number.
    //// * 
    //// * @param value
    //// *            the positive prefix.
    //// */
    ////public void setPositivePrefix(String value)
    ////{
    ////    dform.setPositivePrefix(value);
    ////}

    /////**
    //// * Sets the suffix which is formatted or parsed after a positive number.
    //// * 
    //// * @param value
    //// *            the positive suffix.
    //// */
    ////public void setPositiveSuffix(String value)
    ////{
    ////    dform.setPositiveSuffix(value);
    ////}

    /////**
    //// * Sets the behaviour of the parse method. If set to {@code true} then all
    //// * the returned objects will be of type {@code BigDecimal}.
    //// * 
    //// * @param newValue
    //// *            {@code true} if all the returned objects should be of type
    //// *            {@code BigDecimal}; {@code false} otherwise.
    //// */
    ////public void setParseBigDecimal(boolean newValue)
    ////{
    ////    this.parseBigDecimal = newValue;
    ////}

    /////**
    //// * Returns the pattern of this decimal format using localized pattern
    //// * characters.
    //// * 
    //// * @return the localized pattern.
    //// */
    ////public String toLocalizedPattern()
    ////{
    ////    return dform.toLocalizedPattern();
    ////}

////    /**
////     * Returns the pattern of this decimal format using non-localized pattern
////     * characters.
////     * 
////     * @return the non-localized pattern.
////     */
////    public String toPattern()
////    {
////        return dform.toPattern();
////    }

////    // the fields list to be serialized
////    private static final ObjectStreamField[] serialPersistentFields = {
////            new ObjectStreamField("positivePrefix", String.class), //$NON-NLS-1$
////            new ObjectStreamField("positiveSuffix", String.class), //$NON-NLS-1$
////            new ObjectStreamField("negativePrefix", String.class), //$NON-NLS-1$
////            new ObjectStreamField("negativeSuffix", String.class), //$NON-NLS-1$
////            new ObjectStreamField("posPrefixPattern", String.class), //$NON-NLS-1$
////            new ObjectStreamField("posSuffixPattern", String.class), //$NON-NLS-1$
////            new ObjectStreamField("negPrefixPattern", String.class), //$NON-NLS-1$
////            new ObjectStreamField("negSuffixPattern", String.class), //$NON-NLS-1$
////            new ObjectStreamField("multiplier", int.class), //$NON-NLS-1$
////            new ObjectStreamField("groupingSize", byte.class), //$NON-NLS-1$
////            new ObjectStreamField("decimalSeparatorAlwaysShown", boolean.class), //$NON-NLS-1$
////            new ObjectStreamField("parseBigDecimal", boolean.class), //$NON-NLS-1$
////            new ObjectStreamField("symbols", DecimalFormatSymbols.class), //$NON-NLS-1$
////            new ObjectStreamField("useExponentialNotation", boolean.class), //$NON-NLS-1$
////            new ObjectStreamField("minExponentDigits", byte.class), //$NON-NLS-1$
////            new ObjectStreamField("maximumIntegerDigits", int.class), //$NON-NLS-1$
////            new ObjectStreamField("minimumIntegerDigits", int.class), //$NON-NLS-1$
////            new ObjectStreamField("maximumFractionDigits", int.class), //$NON-NLS-1$
////            new ObjectStreamField("minimumFractionDigits", int.class), //$NON-NLS-1$
////            new ObjectStreamField("serialVersionOnStream", int.class), }; //$NON-NLS-1$

/////**
//// * Writes serialized fields following serialized forms specified by Java
//// * specification.
//// * 
//// * @param stream
//// *            the output stream to write serialized bytes
//// * @throws IOException
//// *             if some I/O error occurs
//// * @throws ClassNotFoundException
//// */
////@SuppressWarnings("nls")
////    private void writeObject(ObjectOutputStream stream) throws IOException,
////            ClassNotFoundException {
////        ObjectOutputStream.PutField fields = stream.putFields();
////fields.put("positivePrefix", dform.getPositivePrefix());
////fields.put("positiveSuffix", dform.getPositiveSuffix());
////fields.put("negativePrefix", dform.getNegativePrefix());
////fields.put("negativeSuffix", dform.getNegativeSuffix());
////String posPrefixPattern = (String)Format.getInternalField(
////        "posPrefixPattern", dform);
////fields.put("posPrefixPattern", posPrefixPattern);
////String posSuffixPattern = (String)Format.getInternalField(
////        "posSuffixPattern", dform);
////fields.put("posSuffixPattern", posSuffixPattern);
////String negPrefixPattern = (String)Format.getInternalField(
////        "negPrefixPattern", dform);
////fields.put("negPrefixPattern", negPrefixPattern);
////String negSuffixPattern = (String)Format.getInternalField(
////        "negSuffixPattern", dform);
////fields.put("negSuffixPattern", negSuffixPattern);
////fields.put("multiplier", dform.getMultiplier());
////fields.put("groupingSize", (byte)dform.getGroupingSize());
////fields.put("decimalSeparatorAlwaysShown", dform
////        .isDecimalSeparatorAlwaysShown());
////fields.put("parseBigDecimal", parseBigDecimal);
////fields.put("symbols", symbols);
////boolean useExponentialNotation = ((Boolean)Format.getInternalField(
////        "useExponentialNotation", dform)).booleanValue();
////fields.put("useExponentialNotation", useExponentialNotation);
////byte minExponentDigits = ((Byte)Format.getInternalField(
////        "minExponentDigits", dform)).byteValue();
////fields.put("minExponentDigits", minExponentDigits);
////fields.put("maximumIntegerDigits", dform.getMaximumIntegerDigits());
////fields.put("minimumIntegerDigits", dform.getMinimumIntegerDigits());
////fields.put("maximumFractionDigits", dform.getMaximumFractionDigits());
////fields.put("minimumFractionDigits", dform.getMinimumFractionDigits());
////fields.put("serialVersionOnStream", CURRENT_SERIAL_VERTION);
////stream.writeFields();
////    }

////    /**
////     * Reads serialized fields following serialized forms specified by Java
////     * specification.
////     * 
////     * @param stream
////     *            the input stream to read serialized bytes
////     * @throws IOException
////     *             if some I/O error occurs
////     * @throws ClassNotFoundException
////     *             if some class of serialized objects or fields cannot be found
////     */
////    @SuppressWarnings("nls")
////    private void readObject(ObjectInputStream stream) throws IOException,
////            ClassNotFoundException {

////        ObjectInputStream.GetField fields = stream.readFields();
////String positivePrefix = (String)fields.get("positivePrefix", "");
////String positiveSuffix = (String)fields.get("positiveSuffix", "");
////String negativePrefix = (String)fields.get("negativePrefix", "-");
////String negativeSuffix = (String)fields.get("negativeSuffix", "");

////String posPrefixPattern = (String)fields.get("posPrefixPattern", "");
////String posSuffixPattern = (String)fields.get("posSuffixPattern", "");
////String negPrefixPattern = (String)fields.get("negPrefixPattern", "-");
////String negSuffixPattern = (String)fields.get("negSuffixPattern", "");

////int multiplier = fields.get("multiplier", 1);
////byte groupingSize = fields.get("groupingSize", (byte)3);
////boolean decimalSeparatorAlwaysShown = fields.get(
////        "decimalSeparatorAlwaysShown", false);
////boolean parseBigDecimal = fields.get("parseBigDecimal", false);
////symbols = (DecimalFormatSymbols)fields.get("symbols", null);

////boolean useExponentialNotation = fields.get("useExponentialNotation",
////        false);
////byte minExponentDigits = fields.get("minExponentDigits", (byte)0);

////int maximumIntegerDigits = fields.get("maximumIntegerDigits", 309);
////int minimumIntegerDigits = fields.get("minimumIntegerDigits", 309);
////int maximumFractionDigits = fields.get("maximumFractionDigits", 340);
////int minimumFractionDigits = fields.get("minimumFractionDigits", 340);
////this.serialVersionOnStream = fields.get("serialVersionOnStream", 0);

////Locale locale = (Locale)Format.getInternalField("locale", symbols);
////dform = new com.ibm.icu.text.DecimalFormat("",
////        new com.ibm.icu.text.DecimalFormatSymbols(locale));
////setInternalField("useExponentialNotation", dform, Boolean
////        .valueOf(useExponentialNotation));
////setInternalField("minExponentDigits", dform,
////        new Byte(minExponentDigits));
////dform.setPositivePrefix(positivePrefix);
////dform.setPositiveSuffix(positiveSuffix);
////dform.setNegativePrefix(negativePrefix);
////dform.setNegativeSuffix(negativeSuffix);
////setInternalField("posPrefixPattern", dform, posPrefixPattern);
////setInternalField("posSuffixPattern", dform, posSuffixPattern);
////setInternalField("negPrefixPattern", dform, negPrefixPattern);
////setInternalField("negSuffixPattern", dform, negSuffixPattern);
////dform.setMultiplier(multiplier);
////dform.setGroupingSize(groupingSize);
////dform.setDecimalSeparatorAlwaysShown(decimalSeparatorAlwaysShown);
////dform.setMinimumIntegerDigits(minimumIntegerDigits);
////dform.setMaximumIntegerDigits(maximumIntegerDigits);
////dform.setMinimumFractionDigits(minimumFractionDigits);
////dform.setMaximumFractionDigits(maximumFractionDigits);
////this.setParseBigDecimal(parseBigDecimal);

////if (serialVersionOnStream < 3)
////{
////    setMaximumIntegerDigits(super.getMaximumIntegerDigits());
////    setMinimumIntegerDigits(super.getMinimumIntegerDigits());
////    setMaximumFractionDigits(super.getMaximumFractionDigits());
////    setMinimumFractionDigits(super.getMinimumFractionDigits());
////}
////if (serialVersionOnStream < 1)
////{
////    this.setInternalField("useExponentialNotation", dform,
////            Boolean.FALSE);
////}
////serialVersionOnStream = 3;
////    }

////    /*
////     * Copies decimal format symbols from text object to ICU one.
////     * 
////     * @param icu the object which receives the new values. @param dfs the
////     * object which contains the new values.
////     */
////    private void copySymbols(final com.ibm.icu.text.DecimalFormatSymbols icu,
////            final DecimalFormatSymbols dfs)
////{
////    Currency currency = dfs.getCurrency();
////    if (currency == null)
////    {
////        icu.setCurrency(com.ibm.icu.util.Currency.getInstance("XXX")); //$NON-NLS-1$
////    }
////    else
////    {
////        icu.setCurrency(com.ibm.icu.util.Currency.getInstance(dfs
////                .getCurrency().getCurrencyCode()));
////    }

////    icu.setCurrencySymbol(dfs.getCurrencySymbol());
////    icu.setDecimalSeparator(dfs.getDecimalSeparator());
////    icu.setDigit(dfs.getDigit());
////    icu.setGroupingSeparator(dfs.getGroupingSeparator());
////    icu.setInfinity(dfs.getInfinity());
////    icu
////            .setInternationalCurrencySymbol(dfs
////                    .getInternationalCurrencySymbol());
////    icu.setMinusSign(dfs.getMinusSign());
////    icu.setMonetaryDecimalSeparator(dfs.getMonetaryDecimalSeparator());
////    icu.setNaN(dfs.getNaN());
////    icu.setPatternSeparator(dfs.getPatternSeparator());
////    icu.setPercent(dfs.getPercent());
////    icu.setPerMill(dfs.getPerMill());
////    icu.setZeroDigit(dfs.getZeroDigit());
////}

/////*
//// * Sets private field value by reflection.
//// * 
//// * @param fieldName the field name to be set @param target the object which
//// * field to be set @param value the value to be set
//// */
////private void setInternalField(final String fieldName, final Object target,
////        final Object value)
////{
////    AccessController
////            .doPrivileged(new PrivilegedAction<java.lang.reflect.Field>()
////            {
////                    public java.lang.reflect.Field run()
////    {
////        java.lang.reflect.Field field = null;
////        try
////        {
////            field = target.getClass().getDeclaredField(
////                    fieldName);
////            field.setAccessible(true);
////            field.set(target, value);
////        }
////        catch (Exception e)
////        {
////            return null;
////        }
////        return field;
////    }
////});
    }
}
