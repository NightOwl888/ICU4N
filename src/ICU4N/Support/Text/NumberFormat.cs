using J2N.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Support.Text
{
    internal abstract class NumberFormat : Formatter
    {
        private readonly IFormatProvider formatProvider;

        //private int maximumIntegerDigits;
        //private int minimumIntegerDigits;
        //private int maximumFractionDigits;
        //private int minimumFractionDigits;

        public NumberFormat(IFormatProvider formatProvider)
        {
            this.formatProvider = formatProvider;
        }

        public IFormatProvider FormatProvider => formatProvider;


        /**
        * Formats the specified double using the rules of this number format.
        * 
        * @param value
        *            the double to format.
        * @return the formatted string.
        */
        public string Format(double value)
        {
            return Format(value, new StringBuffer(), new FieldPosition(0))
                    .ToString();
        }

        /**
         * Formats the specified double value as a string using the pattern of this
         * number format and appends the string to the specified string buffer.
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
         * @param field
         *            on input: an optional alignment field; on output: the offsets
         *            of the alignment field in the formatted text.
         * @return the string buffer.
         */
        public abstract StringBuffer Format(double value, StringBuffer buffer,
                FieldPosition field);

        /**
        * Formats the specified double using the rules of this number format.
        * 
        * @param value
        *            the double to format.
        * @return the formatted string.
        */
        public string Format(float value)
        {
            return Format(value, new StringBuffer(), new FieldPosition(0))
                    .ToString();
        }

        /**
         * Formats the specified double value as a string using the pattern of this
         * number format and appends the string to the specified string buffer.
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
         * @param field
         *            on input: an optional alignment field; on output: the offsets
         *            of the alignment field in the formatted text.
         * @return the string buffer.
         */
        public abstract StringBuffer Format(float value, StringBuffer buffer,
                FieldPosition field);

        /**
        * Formats the specified double using the rules of this number format.
        * 
        * @param value
        *            the double to format.
        * @return the formatted string.
        */
        public string Format(decimal value)
        {
            return Format(value, new StringBuffer(), new FieldPosition(0))
                    .ToString();
        }

        /**
         * Formats the specified double value as a string using the pattern of this
         * number format and appends the string to the specified string buffer.
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
         * @param field
         *            on input: an optional alignment field; on output: the offsets
         *            of the alignment field in the formatted text.
         * @return the string buffer.
         */
        public abstract StringBuffer Format(decimal value, StringBuffer buffer,
                FieldPosition field);

        /**
         * Formats the specified long using the rules of this number format.
         * 
         * @param value
         *            the long to format.
         * @return the formatted string.
         */
        public string Format(long value)
        {
            return Format(value, new StringBuffer(), new FieldPosition(0))
                    .ToString();
        }

        /**
         * Formats the specified long value as a string using the pattern of this
         * number format and appends the string to the specified string buffer.
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
         * @param field
         *            on input: an optional alignment field; on output: the offsets
         *            of the alignment field in the formatted text.
         * @return the string buffer.
         */
        public abstract StringBuffer Format(long value, StringBuffer buffer,
                FieldPosition field);


        public override StringBuffer Format(object number, StringBuffer buffer,
                FieldPosition field)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            //string format = GetNumberFormat();

            if (number is J2N.Numerics.Number num)
            {
                double dv = num.ToDouble();
                long lv = num.ToInt64();
                if (dv == lv)
                    return Format(lv, buffer, field);
                return Format(dv, buffer, field);
            }
            else if (number is int i)
            {
                return Format(i, buffer, field);
            }
            else if (number is long l)
            {
                return Format(l, buffer, field);
            }
            else if (number is short s)
            {
                return Format(s, buffer, field);
            }
            else if (number is float f)
            {
                return Format(f, buffer, field);
            }
            else if (number is double d)
            {
                return Format(d, buffer, field);
            }
            else if (number is decimal dec)
            {
                return Format(dec, buffer, field);
            }

            throw new ArgumentException("Cannot format given object as a Number");
        }

        // TODO: Move to decimal format
        //public virtual string Format(double number)
        //{
        //    string format = GetNumberFormat();
        //    return J2N.Numerics.Double.ToString(number, format, formatProvider);
        //}

        // TODO: Move to decimal format
        //public virtual string Format(long number)
        //{
        //    string format = GetNumberFormat();
        //    return number.ToString(format, formatProvider);
        //}

        /// <summary>
        /// When overridden in a subclass, provides the numeric format as a <see cref="string"/>.
        /// Generally, this is the same format that is passed into the <see cref="M:string.Format(IFormatProvider, string, object)"/>
        /// method.
        /// </summary>
        /// <returns>A numeric format string.</returns>
        protected virtual string GetNumberFormat()
        {
            return null;
        }

        /**
         * Parses a {@code Number} from the specified string using the rules of this
         * number format.
         * 
         * @param string
         *            the string to parse.
         * @return the {@code Number} resulting from the parsing.
         * @throws ParseException
         *            if an error occurs during parsing.
         */
        public virtual J2N.Numerics.Number Parse(string source)
        {
            ParsePosition pos = new ParsePosition(0);
            J2N.Numerics.Number number = Parse(source, pos);
            if (pos.Index == 0)
            {
                // text.1D=Unparseable number: {0}
                throw new ParseException(string.Format("Unparseable number: {0}", source), pos.ErrorIndex); //$NON-NLS-1$
            }
            return number;
        }

        /**
         * Parses a {@code Number} from the specified string starting at the index
         * specified by {@code position}. If the string is successfully parsed then
         * the index of the {@code ParsePosition} is updated to the index following
         * the parsed text. On error, the index is unchanged and the error index of
         * {@code ParsePosition} is set to the index where the error occurred.
         * 
         * @param string
         *            the string to parse.
         * @param position
         *            input/output parameter, specifies the start index in
         *            {@code string} from where to start parsing. If parsing is
         *            successful, it is updated with the index following the parsed
         *            text; on error, the index is unchanged and the error index is
         *            set to the index where the error occurred.
         * @return the {@code Number} resulting from the parse or {@code null} if
         *         there is an error.
         */
        public abstract J2N.Numerics.Number Parse(string str, ParsePosition position);


        public override object ParseObject(string str, ParsePosition position)
        {
            if (position is null)
                throw new ArgumentNullException(nameof(position));

            try
            {
                return Parse(str, position);
            }
            catch
            {
                return null;
            }
        }

        public override string ToString()
        {
            return base.ToString() + " - " + GetNumberFormat() + " - " + formatProvider.ToString();
        }

        // ICU4N TODO: Add additional functionality to edit the NumberFormatInfo
        // properties, which provides somewhat similar functionality to the below Java
        // getters and setters.

        //public virtual int MaximumIntegerDigits
        //{
        //    get { return this.maximumIntegerDigits; }
        //}

        //public virtual void SetMaximumIntegerDigits(int newValue)
        //{
        //    this.maximumIntegerDigits = Math.Max(0, newValue);
        //    if (maximumIntegerDigits < minimumIntegerDigits)
        //    {
        //        minimumIntegerDigits = maximumIntegerDigits;
        //    }
        //}

        //public virtual int MinimumIntegerDigits
        //{
        //    get { return this.minimumIntegerDigits; }
        //}

        //public virtual void SetMinimumIntegerDigits(int newValue)
        //{
        //    this.minimumIntegerDigits = Math.Max(0, newValue);
        //    if (minimumIntegerDigits > maximumIntegerDigits)
        //    {
        //        maximumIntegerDigits = minimumIntegerDigits;
        //    }
        //}

        //public virtual int MaximumFractionDigits
        //{
        //    get { return this.maximumFractionDigits; }
        //}

        //public virtual void SetMaximumFractionDigits(int newValue)
        //{
        //    maximumFractionDigits = Math.Max(0, newValue);
        //    if (maximumFractionDigits < minimumFractionDigits)
        //    {
        //        minimumFractionDigits = maximumFractionDigits;
        //    }
        //}

        //public virtual int MinimumFractionDigits
        //{
        //    get { return this.minimumFractionDigits; }
        //}

        //public void SetMinimumFractionDigits(int newValue)
        //{
        //    minimumFractionDigits = Math.Max(0, newValue);
        //    if (maximumFractionDigits < minimumFractionDigits)
        //    {
        //        maximumFractionDigits = minimumFractionDigits;
        //    }
        //}

        public bool ParseIntegerOnly { get; set; }

        /**
     * Returns a {@code NumberFormat} for formatting and parsing currency values
     * for the default locale.
     * 
     * @return a {@code NumberFormat} for handling currency values.
     */
        public static NumberFormat GetCurrencyInstance()
        {
            return GetCurrencyInstance(CultureInfo.CurrentCulture);
        }

        /**
         * Returns a {@code NumberFormat} for formatting and parsing currency values
         * for the specified locale.
         * 
         * @param locale
         *            the locale to use.
         * @return a {@code NumberFormat} for handling currency values.
         */
        public static NumberFormat GetCurrencyInstance(IFormatProvider provider)
        {
            //com.ibm.icu.text.DecimalFormat icuFormat = (com.ibm.icu.text.DecimalFormat)com.ibm.icu.text.NumberFormat
            //        .getCurrencyInstance(locale);
            //String pattern = icuFormat.toPattern();
            //return new DecimalFormat(pattern, new DecimalFormatSymbols(locale));
            throw new NotImplementedException();
        }

        /**
         * Returns a {@code NumberFormat} for formatting and parsing integers for the
         * default locale.
         * 
         * @return a {@code NumberFormat} for handling integers.
         */
        public static NumberFormat GetIntegerInstance()
        {
            return GetIntegerInstance(CultureInfo.CurrentCulture);
        }

        /**
         * Returns a {@code NumberFormat} for formatting and parsing integers for
         * the specified locale.
         * 
         * @param locale
         *            the locale to use.
         * @return a {@code NumberFormat} for handling integers.
         */
        public static NumberFormat GetIntegerInstance(IFormatProvider provider)
        {
            //com.ibm.icu.text.DecimalFormat icuFormat = (com.ibm.icu.text.DecimalFormat)com.ibm.icu.text.NumberFormat
            //        .getIntegerInstance(locale);
            //String pattern = icuFormat.toPattern();
            //DecimalFormat format = new DecimalFormat(pattern, new DecimalFormatSymbols(locale));
            //format.setParseIntegerOnly(true);
            //return format;
            throw new NotImplementedException();
        }

        /**
         * Returns a {@code NumberFormat} for formatting and parsing numbers for the
         * default locale.
         * 
         * @return a {@code NumberFormat} for handling {@code Number} objects.
         */
        public static NumberFormat GetInstance()
        {
            return GetNumberInstance();
        }

        /**
         * Returns a {@code NumberFormat} for formatting and parsing numbers for the
         * specified locale.
         * 
         * @param locale
         *            the locale to use.
         * @return a {@code NumberFormat} for handling {@code Number} objects.
         */
        public static NumberFormat GetInstance(IFormatProvider provider)
        {
            return GetNumberInstance(provider);
        }

        /**
    * Returns a {@code NumberFormat} for formatting and parsing numbers for the
    * default locale.
    * 
    * @return a {@code NumberFormat} for handling {@code Number} objects.
    */
        public static NumberFormat GetNumberInstance()
        {
            return GetNumberInstance(CultureInfo.CurrentCulture);
        }

        /**
         * Returns a {@code NumberFormat} for formatting and parsing numbers for the
         * specified locale.
         * 
         * @param locale
         *            the locale to use.
         * @return a {@code NumberFormat} for handling {@code Number} objects.
         */
        public static NumberFormat GetNumberInstance(IFormatProvider provider)
        {
            //com.ibm.icu.text.DecimalFormat icuFormat = (com.ibm.icu.text.DecimalFormat)com.ibm.icu.text.NumberFormat
            //        .getNumberInstance(locale);
            //String pattern = icuFormat.toPattern();
            //return new DecimalFormat(pattern, new DecimalFormatSymbols(locale, icuFormat.getDecimalFormatSymbols()), icuFormat);
            //throw new NotImplementedException();

            return new DecimalFormat(null, provider) { NumberStyle = J2N.Globalization.NumberStyle.Number };
        }

        /**
         * Returns a {@code NumberFormat} for formatting and parsing percentage
         * values for the default locale.
         * 
         * @return a {@code NumberFormat} for handling percentage values.
         */
        public static NumberFormat GetPercentInstance()
        {
            return GetPercentInstance(CultureInfo.CurrentCulture);
        }

        /**
         * Returns a {@code NumberFormat} for formatting and parsing percentage
         * values for the specified locale.
         * 
         * @param locale
         *            the locale to use.
         * @return a {@code NumberFormat} for handling percentage values.
         */
        public static NumberFormat GetPercentInstance(IFormatProvider provider)
        {
            //com.ibm.icu.text.DecimalFormat icuFormat = (com.ibm.icu.text.DecimalFormat)com.ibm.icu.text.NumberFormat
            //        .getPercentInstance(locale);
            //String pattern = icuFormat.toPattern();
            //return new DecimalFormat(pattern, new DecimalFormatSymbols(locale));
            throw new NotImplementedException();
        }
    }
}
