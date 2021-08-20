using ICU4N.Text;
using J2N;
using J2N.Text;
using J2N.Numerics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Support.Text
{
    // from Apache Harmony

    internal class ChoiceFormat : Formatter
    {
        //private static readonly long serialVersionUID = 1795184449645032964L;

        private double[] choiceLimits;

        private string[] choiceFormats;

        /// <summary>
        /// Constructs a new <see cref="ChoiceFormat"/> with the specified <see cref="double"/> values
        /// and associated strings. When calling
        /// <see cref="Format(double, StringBuffer, FieldPosition)"/> with a double
        /// value <c>d</c>, then the element <c>i</c> in <paramref name="formats"/> is
        /// selected where <c>i</c> fulfills <c>limits[i] &lt;= d &lt; limits[i + 1]</c>.
        /// <para/>
        /// The length of the <paramref name="limits"/> and <paramref name="formats"/> arrays must be the
        /// same.
        /// </summary>
        /// <param name="limits">An array of <see cref="double"/>s in ascending order. The lowest and highest
        /// possible values are negative and positive infinity.</param>
        /// <param name="formats">the strings associated with the ranges defined through <paramref name="limits"/>.
        /// The lower bound of the associated range is at the same index as the string.</param>
        public ChoiceFormat(double[] limits, string[] formats)
        {
            SetChoices(limits, formats);
        }

        /// <summary>
        /// Constructs a new <see cref="ChoiceFormat"/> with the strings and limits parsed
        /// from the specified pattern.
        /// </summary>
        /// <param name="template">The pattern of strings and ranges.</param>
        /// <exception cref="ArgumentException">If an error occurs while parsing the pattern.</exception>
        public ChoiceFormat(string template)
        {
            ApplyPattern(template);
        }

        /// <summary>
        /// Parses the pattern to determine new strings and ranges for this
        /// <see cref="ChoiceFormat"/>.
        /// </summary>
        /// <param name="template">The pattern of strings and ranges.</param>
        /// <exception cref="ArgumentException">If an error occurs while parsing the pattern.</exception>
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
                        throw new ArgumentException(); // ICU4N TODO: Shouldn't this be FormatException in .NET?
                }
                if (limitCount > 0 && next <= limits[limitCount - 1])
                {
                    throw new ArgumentException(); // ICU4N TODO: Shouldn't this be FormatException in .NET?
                }
                buffer.Length = (0);
                position.Index = (index);
                UpTo(template, position, buffer, '|');
                index = position.Index;
                limits[limitCount++] = next;
                formats.Add(buffer.ToString());
            }
        }

        /// <summary>
        /// Returns a new instance of <see cref="ChoiceFormat"/> with the same ranges and
        /// strings as this <see cref="ChoiceFormat"/>.
        /// </summary>
        /// <returns>A shallow copy of this <see cref="ChoiceFormat"/>.</returns>
        public override object Clone()
        {
            ChoiceFormat clone = (ChoiceFormat)base.MemberwiseClone();
            clone.choiceLimits = (double[])choiceLimits.Clone();
            clone.choiceFormats = (string[])choiceFormats.Clone();
            return clone;
        }

        /// <summary>
        /// Compares the specified object with this <see cref="ChoiceFormat"/>. The object
        /// must be an instance of <see cref="ChoiceFormat"/> and have the same limits and
        /// formats to be equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><c>true</c> if the specified object is equal to this instance;
        /// <c>false</c> otherwise.</returns>
        /// <seealso cref="GetHashCode()"/>
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

        /// <summary>
        /// Appends the string associated with the range in which the specified
        /// <see cref="double"/> value fits to the specified string <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The <see cref="double"/> to format.</param>
        /// <param name="buffer">The target string buffer to append the formatted value to.</param>
        /// <param name="field">A <see cref="FieldPosition"/> which is ignored.</param>
        /// <returns>The <see cref="StringBuffer"/>.</returns>
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

        /// <summary>
        /// Appends the string associated with the range in which the specified <see cref="long"/>
        /// value fits to the specified string <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The <see cref="long"/> to format.</param>
        /// <param name="buffer">The target string buffer to append the formatted value to.</param>
        /// <param name="field">A <see cref="FieldPosition"/> which is ignored.</param>
        /// <returns>The <see cref="StringBuffer"/>.</returns>
        //@Override
        public StringBuffer Format(long value, StringBuffer buffer, FieldPosition field)
        {
            return Format((double)value, buffer, field);
        }

        /// <summary>
        /// Returns the strings associated with the ranges of this <see cref="ChoiceFormat"/>.
        /// </summary>
        /// <returns>An array of format strings.</returns>
        public object[] GetFormats() // ICU4N TODO: API - should this be string[] instead of object?
        {
            return choiceFormats;
        }

        /// <summary>
        /// Returns the limits of this <see cref="ChoiceFormat"/>.
        /// </summary>
        /// <returns>The array of <see cref="double"/>s which make up the limits of this <see cref="ChoiceFormat"/>.</returns>
        public double[] GetLimits()
        {
            return choiceLimits;
        }

        /// <summary>
        /// Returns an <see cref="int"/> hash code for the receiver. Objects which are equal
        /// return the same value for this method.
        /// </summary>
        /// <returns>The receiver's hash.</returns>
        /// <seealso cref="Equals(object)"/>
        public override int GetHashCode()
        {
            int hashCode = 0;
            for (int i = 0; i < choiceLimits.Length; i++)
            {
                long v = BitConversion.DoubleToInt64Bits(choiceLimits[i]);
                hashCode += (int)(v ^ (v.TripleShift(32))) + choiceFormats[i].GetHashCode();
            }
            return hashCode;
        }

        /// <summary>
        /// Returns the <see cref="double"/> value which is closest to the specified <see cref="double"/> but
        /// larger.
        /// </summary>
        /// <param name="value">A <see cref="double"/> value.</param>
        /// <returns>The next larger <see cref="double"/> value.</returns>
        public static double NextDouble(double value)
        {
            if (value == double.PositiveInfinity)
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

        /// <summary>
        /// Returns the <see cref="double"/> value which is closest to the specified double but
        /// either larger or smaller as specified.
        /// </summary>
        /// <param name="value">A <see cref="double"/> value.</param>
        /// <param name="increment"><c>true</c> to get the next larger value, <c>false</c> to
        /// get the previous smaller value.</param>
        /// <returns>The next larger or smaller <see cref="double"/> value.</returns>
        public static double NextDouble(double value, bool increment)
        {
            return increment ? NextDouble(value) : PreviousDouble(value);
        }

        /// <summary>
        /// Parses a <see cref="double"/> from the specified string starting at the index specified
        /// by <paramref name="position"/>. The string is compared to the strings of this
        /// <see cref="ChoiceFormat"/> and if a match occurs then the lower bound of the
        /// corresponding range in the limits array is returned. If the string is
        /// successfully parsed then the index of the <see cref="ParsePosition"/> passed to
        /// this method is updated to the index following the parsed text.
        /// <para/>
        /// If one of the format strings of this <see cref="ChoiceFormat"/> instance is
        /// found in <paramref name="str"/> starting at <c>position.Index</c> then
        /// <list type="bullet">
        ///     <item>
        ///         <description>
        ///             the index in <paramref name="position"/> is set to the index following the
        ///             parsed text;
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             the <see cref="double"/> corresponding to the format
        ///             string is returned.
        ///         </description>
        ///     </item>
        /// </list>
        /// If none of the format strings is found in <paramref name="str"/> then
        /// <list type="bullet">
        ///     <item>
        ///         <description>
        ///             the error index in <paramref name="position"/> is set to the current index in
        ///             <paramref name="position"/>;
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             <see cref="double.NaN"/> is returned.
        ///         </description>
        ///     </item>
        /// </list>
        /// </summary>
        /// <param name="str">The source string to parse.</param>
        /// <param name="position">Input/output parameter, specifies the start index in <paramref name="str"/>
        /// from where to start parsing. See the <em>Returns</em>
        /// section for a description of the output values.</param>
        /// <returns>A <see cref="double"/> resulting from the parse, or <see cref="double.NaN"/> if there is an error.</returns>
        //@Override
        public double Parse(string str, ParsePosition position)
        {
            int offset = position.Index;
            for (int i = 0; i < choiceFormats.Length; i++)
            {
                if (str.StartsWith(choiceFormats[i], offset, StringComparison.Ordinal))
                {
                    position.Index = offset + choiceFormats[i].Length;
                    return choiceLimits[i];
                }
            }
            position.ErrorIndex = offset;
            return double.NaN;
        }

        /// <summary>
        /// Returns the double value which is closest to the specified double but
        /// smaller.
        /// </summary>
        /// <param name="value">A <see cref="double"/> value.</param>
        /// <returns>The next smaller <see cref="double"/> value.</returns>
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

        /// <summary>
        /// Sets the <see cref="double"/> values and associated strings of this <see cref="ChoiceFormat"/>. When
        /// calling <see cref="Format(double, StringBuffer, FieldPosition)"/> with
        /// a <see cref="double"/> value, <c>d</c>, then the element <c>i</c> in <paramref name="formats"/>
        /// is selected where <c>i</c> fulfills
        /// <c>limits[i] &lt;= d &lt; limits[i + 1]</c>.
        /// <para/>
        /// The length of the <paramref name="limits"/> and <paramref name="formats"/> arrays must be the same.
        /// possible values are negative and positive infinity.</summary>
        /// <param name="limits">An array of <see cref="double"/>s in ascending order. The lowest and highest
        /// </param>
        /// <param name="formats">The strings associated with the ranges defined through <paramref name="limits"/>.
        /// The lower bound of the associated range is at the same index as the string.</param>
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

        /// <summary>
        /// Returns the pattern of this <see cref="ChoiceFormat"/> which specifies the
        /// ranges and their associated strings.
        /// </summary>
        /// <returns>The pattern.</returns>
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
