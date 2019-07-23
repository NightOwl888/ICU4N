using ICU4N.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Support.Text
{
    // from Apache Harmony

    public class ChoiceFormat : Formatter
    {
        //private static readonly long serialVersionUID = 1795184449645032964L;

        private double[] choiceLimits;

        private string[] choiceFormats;

        /**
         * Constructs a new {@code ChoiceFormat} with the specified double values
         * and associated strings. When calling
         * {@link #format(double, StringBuffer, FieldPosition) format} with a double
         * value {@code d}, then the element {@code i} in {@code formats} is
         * selected where {@code i} fulfills {@code limits[i] &lt;= d &lt; limits[i+1]}.
         * <p>
         * The length of the {@code limits} and {@code formats} arrays must be the
         * same.
         *
         * @param limits
         *            an array of doubles in ascending order. The lowest and highest
         *            possible values are negative and positive infinity.
         * @param formats
         *            the strings associated with the ranges defined through {@code
         *            limits}. The lower bound of the associated range is at the
         *            same index as the string.
         */
        public ChoiceFormat(double[] limits, string[] formats)
        {
            SetChoices(limits, formats);
        }

        /**
         * Constructs a new {@code ChoiceFormat} with the strings and limits parsed
         * from the specified pattern.
         * 
         * @param template
         *            the pattern of strings and ranges.
         * @throws IllegalArgumentException
         *            if an error occurs while parsing the pattern.
         */
        public ChoiceFormat(string template)
        {
            ApplyPattern(template);
        }

        /**
         * Parses the pattern to determine new strings and ranges for this
         * {@code ChoiceFormat}.
         * 
         * @param template
         *            the pattern of strings and ranges.
         * @throws IllegalArgumentException
         *            if an error occurs while parsing the pattern.
         */
        public void ApplyPattern(string template)
        {
            double[] limits = new double[5];
            List<string> formats = new List<string>();
            int length = template.Length, limitCount = 0, index = 0;
            StringBuffer buffer = new StringBuffer();
            NumberFormat format = NumberFormat.GetInstance(CultureInfo.InvariantCulture);
            ParsePosition position = new ParsePosition(0);
            while (true)
            {
                index = SkipWhitespace(template, index);
                if (index >= length)
                {
                    if (limitCount == limits.Length)
                    {
                        choiceLimits = limits;
                    }
                    else
                    {
                        choiceLimits = new double[limitCount];
                        System.Array.Copy(limits, 0, choiceLimits, 0, limitCount);
                    }
                    choiceFormats = new String[formats.Count];
                    for (int i = 0; i < formats.Count; i++)
                    {
                        choiceFormats[i] = formats[i];
                    }
                    return;
                }
                position.Index = index;
                object value = format.Parse(template, position);

                index = SkipWhitespace(template, position.Index);
                if (position.ErrorIndex != -1 || index >= length)
                {
                    // Fix Harmony 540
                    choiceLimits = new double[0];
                    choiceFormats = new string[0];
                    return;
                }
                char ch = template[index++];
                if (limitCount == limits.Length)
                {
                    double[] newLimits = new double[limitCount * 2];
                    System.Array.Copy(limits, 0, newLimits, 0, limitCount);
                    limits = newLimits;
                }
                double next;
                switch (ch)
                {
                    case '#':
                    case '\u2264':
                        next = Convert.ToDouble(value);
                        break;
                    case '<':
                        next = NextDouble(Convert.ToDouble(value));
                        break;
                    default:
                        throw new ArgumentException();
                }
                if (limitCount > 0 && next <= limits[limitCount - 1])
                {
                    throw new ArgumentException();
                }
                buffer.Length = (0);
                position.Index = (index);
                UpTo(template, position, buffer, '|');
                index = position.Index;
                limits[limitCount++] = next;
                formats.Add(buffer.ToString());
            }
        }

        /**
         * Returns a new instance of {@code ChoiceFormat} with the same ranges and
         * strings as this {@code ChoiceFormat}.
         * 
         * @return a shallow copy of this {@code ChoiceFormat}.
         * 
         * @see java.lang.Cloneable
         */
        public override object Clone()
        {
            ChoiceFormat clone = (ChoiceFormat)base.MemberwiseClone();
            clone.choiceLimits = (double[])choiceLimits.Clone();
            clone.choiceFormats = (string[])choiceFormats.Clone();
            return clone;
        }

        /**
         * Compares the specified object with this {@code ChoiceFormat}. The object
         * must be an instance of {@code ChoiceFormat} and have the same limits and
         * formats to be equal to this instance.
         * 
         * @param object
         *            the object to compare with this instance.
         * @return {@code true} if the specified object is equal to this instance;
         *         {@code false} otherwise.
         * @see #hashCode
         */
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (!(obj is ChoiceFormat))
            {
                return false;
            }
            ChoiceFormat choice = (ChoiceFormat)obj;
            return choiceLimits.SequenceEqual(choice.choiceLimits)
                && choiceFormats.SequenceEqual(choice.choiceFormats);
        }

        /**
         * Appends the string associated with the range in which the specified
         * double value fits to the specified string buffer.
         * 
         * @param value
         *            the double to format.
         * @param buffer
         *            the target string buffer to append the formatted value to.
         * @param field
         *            a {@code FieldPosition} which is ignored.
         * @return the string buffer.
         */
        public StringBuffer Format(double value, StringBuffer buffer, FieldPosition field)
        {
            for (int i = choiceLimits.Length - 1; i >= 0; i--)
            {
                if (choiceLimits[i] <= value)
                {
                    return buffer.Append(choiceFormats[i]);
                }
            }
            return choiceFormats.Length == 0 ? buffer : buffer
                    .Append(choiceFormats[0]);
        }

        /**
         * Appends the string associated with the range in which the specified long
         * value fits to the specified string buffer.
         * 
         * @param value
         *            the long to format.
         * @param buffer
         *            the target string buffer to append the formatted value to.
         * @param field
         *            a {@code FieldPosition} which is ignored.
         * @return the string buffer.
         */
        //@Override
        public StringBuffer Format(long value, StringBuffer buffer, FieldPosition field)
        {
            return Format((double)value, buffer, field);
        }

        /**
         * Returns the strings associated with the ranges of this {@code
         * ChoiceFormat}.
         * 
         * @return an array of format strings.
         */
        public object[] GetFormats()
        {
            return choiceFormats;
        }

        /**
         * Returns the limits of this {@code ChoiceFormat}.
         * 
         * @return the array of doubles which make up the limits of this {@code
         *         ChoiceFormat}.
         */
        public double[] GetLimits()
        {
            return choiceLimits;
        }

        /**
         * Returns an integer hash code for the receiver. Objects which are equal
         * return the same value for this method.
         * 
         * @return the receiver's hash.
         * 
         * @see #equals
         */
        public override int GetHashCode()
        {
            int hashCode = 0;
            for (int i = 0; i < choiceLimits.Length; i++)
            {
                long v = BitConverter.DoubleToInt64Bits(choiceLimits[i]);
                hashCode += (int)(v ^ (v.TripleShift(32))) + choiceFormats[i].GetHashCode();
            }
            return hashCode;
        }

        /**
         * Returns the double value which is closest to the specified double but
         * larger.
         * 
         * @param value
         *            a double value.
         * @return the next larger double value.
         */
        public static double NextDouble(double value)
        {
            if (value == Double.PositiveInfinity)
            {
                return value;
            }
            long bits;
            // Handle -0.0
            if (value == 0)
            {
                bits = 0;
            }
            else
            {
                bits = BitConverter.DoubleToInt64Bits(value);
            }
            return BitConverter.Int64BitsToDouble(value < 0 ? bits - 1 : bits + 1);
        }

        /**
         * Returns the double value which is closest to the specified double but
         * either larger or smaller as specified.
         * 
         * @param value
         *            a double value.
         * @param increment
         *            {@code true} to get the next larger value, {@code false} to
         *            get the previous smaller value.
         * @return the next larger or smaller double value.
         */
        public static double NextDouble(double value, bool increment)
        {
            return increment ? NextDouble(value) : PreviousDouble(value);
        }

        /**
         * Parses a double from the specified string starting at the index specified
         * by {@code position}. The string is compared to the strings of this
         * {@code ChoiceFormat} and if a match occurs then the lower bound of the
         * corresponding range in the limits array is returned. If the string is
         * successfully parsed then the index of the {@code ParsePosition} passed to
         * this method is updated to the index following the parsed text.
         * <p>
         * If one of the format strings of this {@code ChoiceFormat} instance is
         * found in {@code string} starting at {@code position.getIndex()} then
         * <ul>
         * <li>the index in {@code position} is set to the index following the
         * parsed text;
         * <li>the {@link java.lang.Double Double} corresponding to the format
         * string is returned.</li>
         * </ul>
         * <p>
         * If none of the format strings is found in {@code string} then
         * <ul>
         * <li>the error index in {@code position} is set to the current index in
         * {@code position};</li>
         * <li> {@link java.lang.Double#NaN Double.NaN} is returned.
         * </ul>
         * @param string
         *            the source string to parse.
         * @param position
         *            input/output parameter, specifies the start index in {@code
         *            string} from where to start parsing. See the <em>Returns</em>
         *            section for a description of the output values.
         * @return a Double resulting from the parse, or Double.NaN if there is an
         *         error
         */
        //@Override
        public double Parse(string str, ParsePosition position)
        {
            int offset = position.Index;
            for (int i = 0; i < choiceFormats.Length; i++)
            {
                if (str.StartsWith(choiceFormats[i], offset))
                {
                    position.Index = offset + choiceFormats[i].Length;
                    return choiceLimits[i];
                }
            }
            position.ErrorIndex = offset;
            return double.NaN;
        }

        /**
         * Returns the double value which is closest to the specified double but
         * smaller.
         * 
         * @param value
         *            a double value.
         * @return the next smaller double value.
         */
        public static double PreviousDouble(double value)
        {
            if (value == double.NegativeInfinity)
            {
                return value;
            }
            long bits;
            // Handle 0.0
            if (value == 0)
            {
                bits = unchecked((long)0x8000000000000000L);
            }
            else
            {
                bits = BitConverter.DoubleToInt64Bits(value);
            }
            return BitConverter.Int64BitsToDouble(value <= 0 ? bits + 1 : bits - 1);
        }

        /**
         * Sets the double values and associated strings of this ChoiceFormat. When
         * calling {@link #format(double, StringBuffer, FieldPosition) format} with
         * a double value {@code d}, then the element {@code i} in {@code formats}
         * is selected where {@code i} fulfills
         * {@code limits[i] <= d < limits[i+1]}.
         * <p>
         * The length of the {@code limits} and {@code formats} arrays must be the
         * same.
         *
         * @param limits
         *            an array of doubles in ascending order. The lowest and highest
         *            possible values are negative and positive infinity.
         * @param formats
         *            the strings associated with the ranges defined through {@code
         *            limits}. The lower bound of the associated range is at the
         *            same index as the string.
         */
        public virtual void SetChoices(double[] limits, string[] formats)
        {
            if (limits.Length != formats.Length)
            {
                throw new ArgumentException();
            }
            choiceLimits = limits;
            choiceFormats = formats;
        }

        private int SkipWhitespace(string str, int index)
        {
            int length = str.Length;
            while (index < length && char.IsWhiteSpace(str[index]))
            {
                index++;
            }
            return index;
        }

        /**
         * Returns the pattern of this {@code ChoiceFormat} which specifies the
         * ranges and their associated strings.
         * 
         * @return the pattern.
         */
        public virtual string ToPattern()
        {
            StringBuilder buffer = new StringBuilder();
            for (int i = 0; i < choiceLimits.Length; i++)
            {
                if (i != 0)
                {
                    buffer.Append('|');
                }
                string previous = Number.ToString(PreviousDouble(choiceLimits[i]));
                string limit = Number.ToString(choiceLimits[i]);
                if (previous.Length < limit.Length)
                {
                    buffer.Append(previous);
                    buffer.Append('<');
                }
                else
                {
                    buffer.Append(limit);
                    buffer.Append('#');
                }
                bool quote = (choiceFormats[i].IndexOf('|') != -1);
                if (quote)
                {
                    buffer.Append('\'');
                }
                buffer.Append(choiceFormats[i]);
                if (quote)
                {
                    buffer.Append('\'');
                }
            }
            return buffer.ToString();
        }

        //// From Format.java
        //internal static bool UpTo(string str, ParsePosition position,
        //    StringBuffer buffer, char stop)
        //{
        //    int index = position.Index, length = str.Length;
        //    bool lastQuote = false, quote = false;
        //    while (index < length)
        //    {
        //        char ch = str[index++];
        //        if (ch == '\'')
        //        {
        //            if (lastQuote)
        //            {
        //                buffer.Append('\'');
        //            }
        //            quote = !quote;
        //            lastQuote = true;
        //        }
        //        else if (ch == stop && !quote)
        //        {
        //            position.Index = index;
        //            return true;
        //        }
        //        else
        //        {
        //            lastQuote = false;
        //            buffer.Append(ch);
        //        }
        //    }
        //    position.Index = index;
        //    return false;
        //}

        //public string Format(double obj)
        //{
        //    return Format(obj, new StringBuffer()).ToString();
        //}

        // From NumberFormat.java

        public override StringBuffer Format(object obj, StringBuffer buffer, FieldPosition field)
        {
            if (obj.IsNumber())
            {
                double dv = Convert.ToDouble(obj);
                long lv = BitConverter.DoubleToInt64Bits(dv);
                if (dv == lv)
                {
                    return Format(lv, buffer, field);
                }
                return Format(dv, buffer, field);
            }
            throw new ArgumentException();
        }

        public override object ParseObject(string str, ParsePosition position)
        {
            if (position == null)
            {
                // text.1A=position is null
                //throw new NullPointerException(Messages.getString("text.1A")); //$NON-NLS-1$
                throw new ArgumentNullException(nameof(position));
            }

            try
            {
                return Parse(str, position);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
