using System;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Support.Text
{
    // from Apache Harmony

    /// <summary>
    /// The base class for all formats. This class resembles the Format
    /// class in Java, but was renamed Formatter because we want the main
    /// methods to be named Format and there was a collision.
    /// </summary>
    // ICU4N TODO: API - Add IFormatProvider, ICustomFormatter. See: https://stackoverflow.com/a/35577288
    public abstract class Formatter
#if FEATURE_CLONEABLE
        : ICloneable
#endif
    {
        private static readonly long serialVersionUID = -299282585814624189L;

        /// <summary>
        /// Constructs a new <see cref="Formatter"/> instance.
        /// </summary>
        public Formatter()
        {
        }

        /// <summary>
        /// Returns a copy of this <see cref="Formatter"/> instance.
        /// </summary>
        /// <returns>A shallow copy of this format.</returns>
        public virtual object Clone()
        {
            return base.MemberwiseClone();
        }

        internal string ConvertPattern(string template, string fromChars, string toChars,
                bool check)
        {
            if (!check && fromChars.Equals(toChars))
            {
                return template;
            }
            bool quote = false;
            StringBuilder output = new StringBuilder();
            int length = template.Length;
            for (int i = 0; i < length; i++)
            {
                int index;
                char next = template[i];
                if (next == '\'')
                {
                    quote = !quote;
                }
                if (!quote && (index = fromChars[next]) != -1)
                {
                    output.Append(toChars[index]);
                }
                else if (check
                      && !quote
                      && ((next >= 'a' && next <= 'z') || (next >= 'A' && next <= 'Z')))
                {
                    // text.05=Invalid pattern char {0} in {1}
                    throw new ArgumentException(/*Messages.getString(
                            "text.05", String.valueOf(next), template)*/); //$NON-NLS-1$
                }
                else
                {
                    output.Append(next);
                }
            }
            if (quote)
            {
                // text.04=Unterminated quote
                throw new ArgumentException(/*Messages.getString("text.04")*/); //$NON-NLS-1$
            }
            return output.ToString();
        }

        /// <summary>
        /// Formats the specified object using the rules of this format.
        /// </summary>
        /// <param name="obj">The object to format.</param>
        /// <returns>the formatted string.</returns>
        /// <exception cref="ArgumentException">If the object cannot be formatted by this format.</exception>
        public string Format(object obj)
        {
            return Format(obj, new StringBuffer(), new FieldPosition(0))
                    .ToString();
        }

        /// <summary>
        /// Appends the specified object to the specified string buffer using the
        /// rules of this format.
        /// <para/>
        /// <paramref name="field"/> is an input/output parameter. If its <paramref name="field"/>
        /// member contains an enum value specifying a field on input, then its
        /// <c>BeginIndex</c> and <c>EndIndex</c> members will be updated with the
        /// text offset of the first occurrence of this field in the formatted text.
        /// </summary>
        /// <param name="obj">the object to format.</param>
        /// <param name="buffer">the string buffer where the formatted string is appended to.</param>
        /// <param name="field">on input: an optional alignment field; on output: the offsets
        /// of the alignment field in the formatted text.</param>
        /// <returns>The <see cref="StringBuffer"/>.</returns>
        /// <exception cref="ArgumentException">If the object cannot be formatted by this format.</exception>
        public abstract StringBuffer Format(object obj, StringBuffer buffer,
                FieldPosition field);

        /// <summary>
        /// Formats the specified object using the rules of this format and returns
        /// an <see cref="AttributedCharacterIterator"/> with the formatted string and no
        /// attributes.
        /// <para/>
        /// Subclasses should return an <see cref="AttributedCharacterIterator"/> with the
        /// appropriate attributes.
        /// </summary>
        /// <param name="obj">The object to format.</param>
        /// <returns>An <see cref="AttributedCharacterIterator"/> with the formatted object
        /// and attributes.</returns>
        /// <exception cref="ArgumentException">if the object cannot be formatted by this format.</exception>
        public AttributedCharacterIterator FormatToCharacterIterator(object obj)
        {
            return new AttributedString(Format(obj)).GetIterator();
        }

        /// <summary>
        /// Parses the specified string using the rules of this format.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <returns>The object resulting from the parse.</returns>
        /// <exception cref="FormatException">If an error occurs during parsing.</exception>
        public object ParseObject(string str)
        {
            ParsePosition position = new ParsePosition(0);
            object result = ParseObject(str, position);
            if (position.Index == 0)
            {
                // text.1C=Format.parseObject(String) parse failure
                throw new FormatException(
                        /*Messages.getString("text.1C"), position.getErrorIndex()*/); //$NON-NLS-1$
            }
            return result;
        }

        /// <summary>
        /// Parses the specified string starting at the index specified by
        /// <paramref name="position"/>. If the <paramref name="str"/> is successfully parsed then the index of
        /// the <see cref="ParsePosition"/> is updated to the index following the parsed
        /// text. On error, the index is unchanged and the error index of
        /// <see cref="ParsePosition"/> is set to the index where the error occurred.
        /// </summary>
        /// <param name="str">the string to parse.</param>
        /// <param name="position">input/output parameter, specifies the start index in
        /// <paramref name="str"/> from where to start parsing. If parsing is
        /// successful, it is updated with the index following the parsed
        /// text; on error, the index is unchanged and the error index is
        /// set to the index where the error occurred.</param>
        /// <returns>The object resulting from the parse or <c>null</c> if there is
        /// an error.</returns>
        public abstract object ParseObject(string str, ParsePosition position);

        ///*
        // * Gets private field value by reflection.
        // * 
        // * @param fieldName the field name to be set @param target the object which
        // * field to be gotten
        // */
        //internal static object GetInternalField(string fieldName, object target)
        //{
        //    Object value = AccessController
        //            .doPrivileged(new PrivilegedAction<Object>()
        //            {
        //                    public Object run()
        //    {
        //        Object result = null;
        //        java.lang.reflect.Field field = null;
        //        try
        //        {
        //            field = target.getClass().getDeclaredField(
        //                    fieldName);
        //            field.setAccessible(true);
        //            result = field.get(target);
        //        }
        //        catch (Exception e1)
        //        {
        //            return null;
        //        }
        //        return result;
        //    }
        //});
        //        return value;
        //    }

        internal static bool UpTo(string str, ParsePosition position,
                StringBuffer buffer, char stop)
        {
            int index = position.Index, length = str.Length;
            bool lastQuote = false, quote = false;
            while (index < length)
            {
                char ch = str[index++];
                if (ch == '\'')
                {
                    if (lastQuote)
                    {
                        buffer.Append('\'');
                    }
                    quote = !quote;
                    lastQuote = true;
                }
                else if (ch == stop && !quote)
                {
                    position.Index = index;
                    return true;
                }
                else
                {
                    lastQuote = false;
                    buffer.Append(ch);
                }
            }
            position.Index = index;
            return false;
        }

        internal static bool UpToWithQuotes(String str, ParsePosition position,
                StringBuffer buffer, char stop, char start)
        {
            int index = position.Index, length = str.Length, count = 1;
            bool quote = false;
            while (index < length)
            {
                char ch = str[index++];
                if (ch == '\'')
                {
                    quote = !quote;
                }
                if (!quote)
                {
                    if (ch == stop)
                    {
                        count--;
                    }
                    if (count == 0)
                    {
                        position.Index = index;
                        return true;
                    }
                    if (ch == start)
                    {
                        count++;
                    }
                }
                buffer.Append(ch);
            }
            // text.07=Unmatched braces in the pattern
            throw new ArgumentException(/*Messages.getString("text.07")*/); //$NON-NLS-1$
        }
    }

    /// <summary>
    /// Class used to represet <see cref="Formatter"/> attributes in the
    /// <see cref="AttributedCharacterIterator"/> that the
    /// <see cref="Formatter.FormatToCharacterIterator(object)"/> method returns
    /// in subclasses.
    /// </summary>
    public class FormatField : AttributedCharacterIteratorAttribute
    {

        private static readonly long serialVersionUID = 276966692217360283L;

        /// <summary>
        /// Constructs a new instance of <see cref="FormatField"/> with the given field name.
        /// </summary>
        /// <param name="fieldName">the field name</param>
        protected FormatField(string fieldName)
            : base(fieldName)
        {
        }
    }
}
