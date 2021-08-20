using J2N.Collections;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using StringBuffer = System.Text.StringBuilder();

namespace ICU4N.Support.Text
{
    internal class MessageFormat : Formatter
    {
        private CultureInfo locale = CultureInfo.CurrentCulture;

#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private string[] strings;

        private int[] argumentNumbers;

        private Formatter[] formats;

        private int maxOffset;

#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private int maxArgumentIndex;

        /**
         * Constructs a new {@code MessageFormat} using the specified pattern and
         * the specified locale for formats.
         * 
         * @param template
         *            the pattern.
         * @param locale
         *            the locale.
         * @throws IllegalArgumentException
         *            if the pattern cannot be parsed.
         */
        public MessageFormat(string template, CultureInfo locale)
        {
            this.locale = locale;
            applyPattern(template);
        }

        /**
         * Constructs a new {@code MessageFormat} using the specified pattern and
         * the default locale for formats.
         * 
         * @param template
         *            the pattern.
         * @throws IllegalArgumentException
         *            if the pattern cannot be parsed.
         */
        public MessageFormat(string template)
        {
            applyPattern(template);
        }

        /**
         * Changes this {@code MessageFormat} to use the specified pattern.
         * 
         * @param template
         *            the new pattern.
         * @throws IllegalArgumentException
         *            if the pattern cannot be parsed.
         */
        public void applyPattern(string template)
        {
            int length = template.Length;
            StringBuffer buffer = new StringBuffer();
            ParsePosition position = new ParsePosition(0);
            List<string> localStrings = new List<string>();
            int argCount = 0;
            int[] args = new int[10];
            int maxArg = -1;
            List<Formatter> localFormats = new List<Formatter>();
            while (position.Index < length)
            {
                if (Formatter.UpTo(template, position, buffer, '{'))
                {
                    int arg = 0;
                    int offset = position.Index;
                    if (offset >= length)
                    {
                        // text.19=Invalid argument number
                        throw new ArgumentException("Invalid argument number"); //$NON-NLS-1$
                    }
                    // Get argument number
                    char ch;
                    while ((ch = template[offset++]) != '}' && ch != ',')
                    {
                        if (ch < '0' && ch > '9')
                        {
                            // text.19=Invalid argument number
                            throw new ArgumentException("Invalid argument number"); //$NON-NLS-1$
                        }

                        arg = arg * 10 + (ch - '0');

                        if (arg < 0 || offset >= length)
                        {
                            // text.19=Invalid argument number
                            throw new ArgumentException("Invalid argument number"); //$NON-NLS-1$
                        }
                    }
                    offset--;
                    position.Index=offset;
                    localFormats.Add(parseVariable(template, position));
                    if (argCount >= args.Length)
                    {
                        int[] newArgs = new int[args.Length * 2];
                        System.arraycopy(args, 0, newArgs, 0, args.Length);
                        args = newArgs;
                    }
                    args[argCount++] = arg;
                    if (arg > maxArg)
                    {
                        maxArg = arg;
                    }
                }
                localStrings.addElement(buffer.toString());
                buffer.setLength(0);
            }
            this.strings = new String[localStrings.size()];
            for (int i = 0; i < localStrings.size(); i++)
            {
                this.strings[i] = localStrings.elementAt(i);
            }
            argumentNumbers = args;
            this.formats = new Format[argCount];
            for (int i = 0; i < argCount; i++)
            {
                this.formats[i] = localFormats.elementAt(i);
            }
            maxOffset = argCount - 1;
            maxArgumentIndex = maxArg;
        }

        /**
         * Returns a new instance of {@code MessageFormat} with the same pattern and
         * formats as this {@code MessageFormat}.
         * 
         * @return a shallow copy of this {@code MessageFormat}.
         * @see java.lang.Cloneable
         */
        @Override
    public Object clone()
        {
            MessageFormat clone = (MessageFormat)super.clone();
            Format[] array = new Format[formats.Length];
            for (int i = formats.Length; --i >= 0;)
            {
                if (formats[i] != null)
                {
                    array[i] = (Format)formats[i].clone();
                }
            }
            clone.formats = array;
            return clone;
        }

        /**
         * Compares the specified object to this {@code MessageFormat} and indicates
         * if they are equal. In order to be equal, {@code object} must be an
         * instance of {@code MessageFormat} and have the same pattern.
         * 
         * @param object
         *            the object to compare with this object.
         * @return {@code true} if the specified object is equal to this
         *         {@code MessageFormat}; {@code false} otherwise.
         * @see #hashCode
         */
    public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (!(obj is MessageFormat)) {
                return false;
            }
            MessageFormat format = (MessageFormat)obj;
            if (maxOffset != format.maxOffset)
            {
                return false;
            }
            // Must use a loop since the lengths may be different due
            // to serialization cross-loading
            for (int i = 0; i <= maxOffset; i++)
            {
                if (argumentNumbers[i] != format.argumentNumbers[i])
                {
                    return false;
                }
            }
            return locale.Equals(format.locale)
                    && ArrayEqualityComparer<string>.OneDimensional.Equals(strings, format.strings)
                    && ArrayEqualityComparer<Formatter>.OneDimensional.Equals(formats, format.formats);
        }

        /**
         * Formats the specified object using the rules of this message format and
         * returns an {@code AttributedCharacterIterator} with the formatted message and
         * attributes. The {@code AttributedCharacterIterator} returned also includes the
         * attributes from the formats of this message format.
         * 
         * @param object
         *            the object to format.
         * @return an {@code AttributedCharacterIterator} with the formatted message and
         *         attributes.
         * @throws IllegalArgumentException
         *            if the arguments in the object array cannot be formatted
         *            by this message format.
         */
    public override AttributedCharacterIterator FormatToCharacterIterator(object obj)
        {
            if (obj is null)
                throw new ArgumentNullException(nameof(obj));

            StringBuffer buffer = new StringBuffer();
            List<FieldContainer> fields = new List<FieldContainer>();

            // format the message, and find fields
            formatImpl((object[])obj, buffer, new FieldPosition(0), fields);

            // create an AttributedString with the formatted buffer
            AttributedString @as = new AttributedString(buffer.ToString());

            // add MessageFormat field attributes and values to the AttributedString
            for (int i = 0; i < fields.Count; i++)
            {
                FieldContainer fc = fields[i];
            @as.AddAttribute(fc.attribute, fc.value, fc.start, fc.end);
            }

            // return the CharacterIterator from AttributedString
            return @as.GetIterator();
        }

        /**
         * Converts the specified objects into a string which it appends to the
         * specified string buffer using the pattern of this message format.
         * <p>
         * If the {@code field} member of the specified {@code FieldPosition} is
         * {@code MessageFormat.Field.ARGUMENT}, then the begin and end index of
         * this field position is set to the location of the first occurrence of a
         * message format argument. Otherwise, the {@code FieldPosition} is ignored.
         *
         * @param objects
         *            the array of objects to format.
         * @param buffer
         *            the target string buffer to append the formatted message to.
         * @param field
         *            on input: an optional alignment field; on output: the offsets
         *            of the alignment field in the formatted text.
         * @return the string buffer.
         */
        public StringBuffer format(object[] objects, StringBuffer buffer,
                FieldPosition field)
        {
            return formatImpl(objects, buffer, field, null);
        }

        private StringBuffer formatImpl(object[] objects, StringBuffer buffer,
                FieldPosition position, IList<FieldContainer> fields)
        {
            FieldPosition passedField = new FieldPosition(0);
            for (int i = 0; i <= maxOffset; i++)
            {
                buffer.Append(strings[i]);
                int begin = buffer.Length;
                object arg;
                if (objects != null && argumentNumbers[i] < objects.Length)
                {
                    arg = objects[argumentNumbers[i]];
                }
                else
                {
                    buffer.Append('{');
                    buffer.Append(argumentNumbers[i]);
                    buffer.Append('}');
                    handleArgumentField(begin, buffer.Length, argumentNumbers[i],
                            position, fields);
                    continue;
                }
                Formatter format = formats[i];
                if (format == null || arg == null)
                {
                    if (arg is Number) {
                        //format = NumberFormat.getInstance();
                        throw new NotSupportedException("Number formatting is not supported");
            } else if (arg is DateTime) {
                        //format = DateFormat.getInstance();
                        throw new NotSupportedException("Date formatting is not supported");
            } else
            {
                buffer.Append(arg);
                handleArgumentField(begin, buffer.Length,
                        argumentNumbers[i], position, fields);
                continue;
            }
        }
            if (format is ChoiceFormat) {
                string result = format.Format(arg);
        MessageFormat mf = new MessageFormat(result);
        mf.Culture = locale;
                mf.Format(objects, buffer, passedField);
                handleArgumentField(begin, buffer.Length, argumentNumbers[i],
                        position, fields);
        handleformat(format, arg, begin, fields);
    } else {
                format.Format(arg, buffer, passedField);
                handleArgumentField(begin, buffer.Length, argumentNumbers[i],
                        position, fields);
    handleformat(format, arg, begin, fields);
}
        }
        if (maxOffset + 1 < strings.Length)
{
    buffer.Append(strings[maxOffset + 1]);
}
return buffer;
    }

    /**
     * Adds a new FieldContainer with MessageFormat.Field.ARGUMENT field,
     * argnumber, begin and end index to the fields vector, or sets the
     * position's begin and end index if it has MessageFormat.Field.ARGUMENT as
     * its field attribute.
     * 
     * @param begin
     * @param end
     * @param argnumber
     * @param position
     * @param fields
     */
    private void handleArgumentField(int begin, int end, int argnumber,
            FieldPosition position, IList<FieldContainer> fields)
{
    if (fields != null)
    {
        fields.Add(new FieldContainer(begin, end, Field.ARGUMENT,
                new Integer(argnumber)));
    }
    else
    {
        if (position != null
                && position.getFieldAttribute() == Field.ARGUMENT
                && position.getEndIndex() == 0)
        {
            position.setBeginIndex(begin);
            position.setEndIndex(end);
        }
    }
}

/**
 * An inner class to store attributes, values, start and end indices.
 * Instances of this inner class are used as elements for the fields vector
 */
internal class FieldContainer
{
    internal int start, end;

    internal AttributedCharacterIteratorAttribute attribute;

    internal object value;

    public FieldContainer(int start, int end,
            AttributedCharacterIteratorAttribute attribute, object value)
    {
        this.start = start;
        this.end = end;
        this.attribute = attribute;
        this.value = value;
    }
}

/**
 * If fields vector is not null, find and add the fields of this format to
 * the fields vector by iterating through its AttributedCharacterIterator
 * 
 * @param format
 *            the format to find fields for
 * @param arg
 *            object to format
 * @param begin
 *            the index where the string this format has formatted begins
 * @param fields
 *            fields vector, each entry in this vector are of type
 *            FieldContainer.
 */
private void handleformat(Formatter format, object arg, int begin,
        IList<FieldContainer> fields)
{
    if (fields != null)
    {
        AttributedCharacterIterator iterator = format
                .FormatToCharacterIterator(arg);
        while (iterator.Index != iterator.EndIndex)
        {
            int start = iterator.GetRunStart();
            int end = iterator.GetRunLimit();

                    //Iterator <?> it = iterator.getAttributes().keySet().iterator();
                    using var it = iterator.GetAttributes().GetEnumerator();
            while (it.MoveNext())
            {
                AttributedCharacterIteratorAttribute attribute = (AttributedCharacterIteratorAttribute)it.Current.Value;
                object value = iterator.GetAttribute(attribute);
                fields.Add(new FieldContainer(begin + start, begin + end,
                        attribute, value));
            }
            iterator.SetIndex(end);
        }
    }
}

/**
 * Converts the specified objects into a string which it appends to the
 * specified string buffer using the pattern of this message format.
 * <p>
 * If the {@code field} member of the specified {@code FieldPosition} is
 * {@code MessageFormat.Field.ARGUMENT}, then the begin and end index of
 * this field position is set to the location of the first occurrence of a
 * message format argument. Otherwise, the {@code FieldPosition} is ignored.
 * <p>
 * Calling this method is equivalent to calling
 * <blockquote>
 * 
 * <pre>
 * format((Object[])object, buffer, field)
 * </pre>
 *
 * </blockquote>
 *
 * @param object
 *            the object to format, must be an array of {@code Object}.
 * @param buffer
 *            the target string buffer to append the formatted message to.
 * @param field
 *            on input: an optional alignment field; on output: the offsets
 *            of the alignment field in the formatted text.
 * @return the string buffer.
 * @throws ClassCastException
 *             if {@code object} is not an array of {@code Object}.
 */
    public override sealed StringBuffer Format(object obj, StringBuffer buffer,
            FieldPosition field) {
    return format((object[])obj, buffer, field);
}

/**
 * Formats the supplied objects using the specified message format pattern.
 * 
 * @param template
 *            the pattern to use for formatting.
 * @param objects
 *            the array of objects to format.
 * @return the formatted result.
 * @throws IllegalArgumentException
 *            if the pattern cannot be parsed.
 */
public static string format(string template, params object[] objects)
{
    if (objects != null)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == null)
            {
                objects[i] = "null";
            }
        }
    }
    return com.ibm.icu.text.MessageFormat.format(template, objects);
}

/**
 * Returns the {@code Format} instances used by this message format.
 * 
 * @return an array of {@code Format} instances.
 */
public Format[] getFormats()
{
    return formats.clone();
}

/**
 * Returns the formats used for each argument index. If an argument is
 * placed more than once in the pattern string, then this returns the format
 * of the last one.
 * 
 * @return an array of formats, ordered by argument index.
 */
public Format[] getFormatsByArgumentIndex()
{
    Format[] answer = new Format[maxArgumentIndex + 1];
    for (int i = 0; i < maxOffset + 1; i++)
    {
        answer[argumentNumbers[i]] = formats[i];
    }
    return answer;
}

/**
 * Sets the format used for the argument at index {@code argIndex} to
 * {@code format}.
 * 
 * @param argIndex
 *            the index of the format to set.
 * @param format
 *            the format that will be set at index {@code argIndex}.
 */
public void setFormatByArgumentIndex(int argIndex, Format format)
{
    for (int i = 0; i < maxOffset + 1; i++)
    {
        if (argumentNumbers[i] == argIndex)
        {
            formats[i] = format;
        }
    }
}

/**
 * Sets the formats used for each argument. The {@code formats} array
 * elements should be in the order of the argument indices.
 * 
 * @param formats
 *            the formats in an array.
 */
public void setFormatsByArgumentIndex(Format[] formats)
{
    for (int j = 0; j < formats.length; j++)
    {
        for (int i = 0; i < maxOffset + 1; i++)
        {
            if (argumentNumbers[i] == j)
            {
                this.formats[i] = formats[j];
            }
        }
    }
}

/**
 * Returns the locale used when creating formats.
 * 
 * @return the locale used to create formats.
 */
public Locale getLocale()
{
    return locale;
}

@Override
    public int hashCode()
{
    int hashCode = 0;
    for (int i = 0; i <= maxOffset; i++)
    {
        hashCode += argumentNumbers[i] + strings[i].hashCode();
        if (formats[i] != null)
        {
            hashCode += formats[i].hashCode();
        }
    }
    if (maxOffset + 1 < strings.length)
    {
        hashCode += strings[maxOffset + 1].hashCode();
    }
    if (locale != null)
    {
        return hashCode + locale.hashCode();
    }
    return hashCode;
}

/**
 * Parses the message arguments from the specified string using the rules of
 * this message format.
 * 
 * @param string
 *            the string to parse.
 * @return the array of {@code Object} arguments resulting from the parse.
 * @throws ParseException
 *            if an error occurs during parsing.
 */
public Object[] parse(String string) throws ParseException
{
    ParsePosition position = new ParsePosition(0);
Object[] result = parse(string, position);
if (position.getIndex() == 0)
{
    // text.1B=MessageFormat.parseObject(String) parse failure
    throw new ParseException(
            Messages.getString("text.1B"), position.getErrorIndex()); //$NON-NLS-1$
}
return result;
    }

    /**
     * Parses the message argument from the specified string starting at the
     * index specified by {@code position}. If the string is successfully
     * parsed then the index of the {@code ParsePosition} is updated to the
     * index following the parsed text. On error, the index is unchanged and the
     * error index of {@code ParsePosition} is set to the index where the error
     * occurred.
     * 
     * @param string
     *            the string to parse.
     * @param position
     *            input/output parameter, specifies the start index in
     *            {@code string} from where to start parsing. If parsing is
     *            successful, it is updated with the index following the parsed
     *            text; on error, the index is unchanged and the error index is
     *            set to the index where the error occurred.
     * @return the array of objects resulting from the parse, or {@code null} if
     *         there is an error.
     */
    public Object[] parse(String string, ParsePosition position)
{
    if (string == null)
    {
        return new Object[0];
    }
    ParsePosition internalPos = new ParsePosition(0);
    int offset = position.getIndex();
    Object[] result = new Object[maxArgumentIndex + 1];
    for (int i = 0; i <= maxOffset; i++)
    {
        String sub = strings[i];
        if (!string.startsWith(sub, offset))
        {
            position.setErrorIndex(offset);
            return null;
        }
        offset += sub.length();
        Object parse;
        Format format = formats[i];
        if (format == null)
        {
            if (i + 1 < strings.length)
            {
                int next = string.indexOf(strings[i + 1], offset);
                if (next == -1)
                {
                    position.setErrorIndex(offset);
                    return null;
                }
                parse = string.substring(offset, next);
                offset = next;
            }
            else
            {
                parse = string.substring(offset);
                offset = string.length();
            }
        }
        else
        {
            internalPos.setIndex(offset);
            parse = format.parseObject(string, internalPos);
            if (internalPos.getErrorIndex() != -1)
            {
                position.setErrorIndex(offset);
                return null;
            }
            offset = internalPos.getIndex();
        }
        result[argumentNumbers[i]] = parse;
    }
    if (maxOffset + 1 < strings.length)
    {
        String sub = strings[maxOffset + 1];
        if (!string.startsWith(sub, offset))
        {
            position.setErrorIndex(offset);
            return null;
        }
        offset += sub.length();
    }
    position.setIndex(offset);
    return result;
}

/**
 * Parses the message argument from the specified string starting at the
 * index specified by {@code position}. If the string is successfully
 * parsed then the index of the {@code ParsePosition} is updated to the
 * index following the parsed text. On error, the index is unchanged and the
 * error index of {@code ParsePosition} is set to the index where the error
 * occurred.
 * 
 * @param string
 *            the string to parse.
 * @param position
 *            input/output parameter, specifies the start index in
 *            {@code string} from where to start parsing. If parsing is
 *            successful, it is updated with the index following the parsed
 *            text; on error, the index is unchanged and the error index is
 *            set to the index where the error occurred.
 * @return the array of objects resulting from the parse, or {@code null} if
 *         there is an error.
 */
@Override
    public Object parseObject(String string, ParsePosition position)
{
    return parse(string, position);
}

private int match(String string, ParsePosition position, boolean last,
        String[] tokens)
{
    int length = string.length(), offset = position.getIndex(), token = -1;
    while (offset < length && Character.isWhitespace(string.charAt(offset)))
    {
        offset++;
    }
    for (int i = tokens.length; --i >= 0;)
    {
        if (string.regionMatches(true, offset, tokens[i], 0, tokens[i]
                .length()))
        {
            token = i;
            break;
        }
    }
    if (token == -1)
    {
        return -1;
    }
    offset += tokens[token].length();
    while (offset < length && Character.isWhitespace(string.charAt(offset)))
    {
        offset++;
    }
    char ch;
    if (offset < length
            && ((ch = string.charAt(offset)) == '}' || (!last && ch == ',')))
    {
        position.setIndex(offset + 1);
        return token;
    }
    return -1;
}

private Format parseVariable(String string, ParsePosition position)
{
    int length = string.length(), offset = position.getIndex();
    char ch;
    if (offset >= length
            || ((ch = string.charAt(offset++)) != '}' && ch != ','))
    {
        // text.15=Missing element format
        throw new IllegalArgumentException(Messages.getString("text.15")); //$NON-NLS-1$
    }
    position.setIndex(offset);
    if (ch == '}')
    {
        return null;
    }
    int type = match(string, position, false, new String[] { "time", //$NON-NLS-1$
                "date", "number", "choice" }); //$NON-NLS-1$ //$NON-NLS-2$ //$NON-NLS-3$
    if (type == -1)
    {
        // text.16=Unknown element format
        throw new IllegalArgumentException(Messages.getString("text.16")); //$NON-NLS-1$
    }
    StringBuffer buffer = new StringBuffer();
    ch = string.charAt(position.getIndex() - 1);
    switch (type)
    {
        case 0: // time
        case 1: // date
            if (ch == '}')
            {
                return type == 1 ? DateFormat.getDateInstance(
                        DateFormat.DEFAULT, locale) : DateFormat
                        .getTimeInstance(DateFormat.DEFAULT, locale);
            }
            int dateStyle = match(string, position, true, new String[] {
                        "full", "long", "medium", "short" }); //$NON-NLS-1$ //$NON-NLS-2$ //$NON-NLS-3$ //$NON-NLS-4$
            if (dateStyle == -1)
            {
                Format.upToWithQuotes(string, position, buffer, '}', '{');
                return new SimpleDateFormat(buffer.toString(), locale);
            }
            switch (dateStyle)
            {
                case 0:
                    dateStyle = DateFormat.FULL;
                    break;
                case 1:
                    dateStyle = DateFormat.LONG;
                    break;
                case 2:
                    dateStyle = DateFormat.MEDIUM;
                    break;
                case 3:
                    dateStyle = DateFormat.SHORT;
                    break;
            }
            return type == 1 ? DateFormat
                    .getDateInstance(dateStyle, locale) : DateFormat
                    .getTimeInstance(dateStyle, locale);
        case 2: // number
            if (ch == '}')
            {
                return NumberFormat.getInstance();
            }
            int numberStyle = match(string, position, true, new String[] {
                        "currency", "percent", "integer" }); //$NON-NLS-1$ //$NON-NLS-2$ //$NON-NLS-3$
            if (numberStyle == -1)
            {
                Format.upToWithQuotes(string, position, buffer, '}', '{');
                return new DecimalFormat(buffer.toString(),
                        new DecimalFormatSymbols(locale));
            }
            switch (numberStyle)
            {
                case 0: // currency
                    return NumberFormat.getCurrencyInstance(locale);
                case 1: // percent
                    return NumberFormat.getPercentInstance(locale);
            }
            return NumberFormat.getIntegerInstance(locale);
    }
    // choice
    try
    {
        Format.upToWithQuotes(string, position, buffer, '}', '{');
    }
    catch (IllegalArgumentException e)
    {
        // ignored
    }
    return new ChoiceFormat(buffer.toString());
}

/**
 * Sets the specified format used by this message format.
 * 
 * @param offset
 *            the index of the format to change.
 * @param format
 *            the {@code Format} that replaces the old format.
 */
public void setFormat(int offset, Format format)
{
    formats[offset] = format;
}

/**
 * Sets the formats used by this message format.
 * 
 * @param formats
 *            an array of {@code Format}.
 */
public void setFormats(Format[] formats)
{
    int min = this.formats.length;
    if (formats.length < min)
    {
        min = formats.length;
    }
    for (int i = 0; i < min; i++)
    {
        this.formats[i] = formats[i];
    }
}

/**
 * Sets the locale to use when creating {@code Format} instances. Changing
 * the locale may change the behavior of {@code applyPattern},
 * {@code toPattern}, {@code format} and {@code formatToCharacterIterator}.
 * 
 * @param locale
 *            the new locale.
 */
public void setLocale(Locale locale)
{
    this.locale = locale;
    for (int i = 0; i <= maxOffset; i++)
    {
        Format format = formats[i];
        if (format instanceof DecimalFormat) {
    formats[i] = new DecimalFormat(((DecimalFormat)format)
            .toPattern(), new DecimalFormatSymbols(locale));
} else if (format instanceof SimpleDateFormat) {
    formats[i] = new SimpleDateFormat(((SimpleDateFormat)format)
            .toPattern(), locale);
}

        }
    }

    private String decodeDecimalFormat(StringBuffer buffer, Format format)
{
    buffer.append(",number"); //$NON-NLS-1$
    if (format.equals(NumberFormat.getNumberInstance(locale)))
    {
        // Empty block
    }
    else if (format.equals(NumberFormat.getIntegerInstance(locale)))
    {
        buffer.append(",integer"); //$NON-NLS-1$
    }
    else if (format.equals(NumberFormat.getCurrencyInstance(locale)))
    {
        buffer.append(",currency"); //$NON-NLS-1$
    }
    else if (format.equals(NumberFormat.getPercentInstance(locale)))
    {
        buffer.append(",percent"); //$NON-NLS-1$
    }
    else
    {
        buffer.append(',');
        return ((DecimalFormat)format).toPattern();
    }
    return null;
}

private String decodeSimpleDateFormat(StringBuffer buffer, Format format)
{
    if (format.equals(DateFormat
            .getTimeInstance(DateFormat.DEFAULT, locale)))
    {
        buffer.append(",time"); //$NON-NLS-1$
    }
    else if (format.equals(DateFormat.getDateInstance(DateFormat.DEFAULT,
          locale)))
    {
        buffer.append(",date"); //$NON-NLS-1$
    }
    else if (format.equals(DateFormat.getTimeInstance(DateFormat.SHORT,
          locale)))
    {
        buffer.append(",time,short"); //$NON-NLS-1$
    }
    else if (format.equals(DateFormat.getDateInstance(DateFormat.SHORT,
          locale)))
    {
        buffer.append(",date,short"); //$NON-NLS-1$
    }
    else if (format.equals(DateFormat.getTimeInstance(DateFormat.LONG,
          locale)))
    {
        buffer.append(",time,long"); //$NON-NLS-1$
    }
    else if (format.equals(DateFormat.getDateInstance(DateFormat.LONG,
          locale)))
    {
        buffer.append(",date,long"); //$NON-NLS-1$
    }
    else if (format.equals(DateFormat.getTimeInstance(DateFormat.FULL,
          locale)))
    {
        buffer.append(",time,full"); //$NON-NLS-1$
    }
    else if (format.equals(DateFormat.getDateInstance(DateFormat.FULL,
          locale)))
    {
        buffer.append(",date,full"); //$NON-NLS-1$
    }
    else
    {
        buffer.append(",date,"); //$NON-NLS-1$
        return ((SimpleDateFormat)format).toPattern();
    }
    return null;
}

/**
 * Returns the pattern of this message format.
 * 
 * @return the pattern.
 */
public String toPattern()
{
    StringBuffer buffer = new StringBuffer();
    for (int i = 0; i <= maxOffset; i++)
    {
        appendQuoted(buffer, strings[i]);
        buffer.append('{');
        buffer.append(argumentNumbers[i]);
        Format format = formats[i];
        String pattern = null;
        if (format instanceof ChoiceFormat) {
    buffer.append(",choice,"); //$NON-NLS-1$
    pattern = ((ChoiceFormat)format).toPattern();
} else if (format instanceof DecimalFormat) {
    pattern = decodeDecimalFormat(buffer, format);
} else if (format instanceof SimpleDateFormat) {
    pattern = decodeSimpleDateFormat(buffer, format);
} else if (format != null)
{
    // text.17=Unknown format
    throw new IllegalArgumentException(Messages
            .getString("text.17")); //$NON-NLS-1$
}
if (pattern != null)
{
    boolean quote = false;
    int index = 0, length = pattern.length(), count = 0;
    while (index < length)
    {
        char ch = pattern.charAt(index++);
        if (ch == '\'')
        {
            quote = !quote;
        }
        if (!quote)
        {
            if (ch == '{')
            {
                count++;
            }
            if (ch == '}')
            {
                if (count > 0)
                {
                    count--;
                }
                else
                {
                    buffer.append("'}"); //$NON-NLS-1$
                    ch = '\'';
                }
            }
        }
        buffer.append(ch);
    }
}
buffer.append('}');
        }
        if (maxOffset + 1 < strings.length)
{
    appendQuoted(buffer, strings[maxOffset + 1]);
}
return buffer.toString();
    }

    private void appendQuoted(StringBuffer buffer, String string)
{
    int length = string.length();
    for (int i = 0; i < length; i++)
    {
        char ch = string.charAt(i);
        if (ch == '{' || ch == '}')
        {
            buffer.append('\'');
            buffer.append(ch);
            buffer.append('\'');
        }
        else
        {
            buffer.append(ch);
        }
    }
}

private static final ObjectStreamField[] serialPersistentFields = {
    new ObjectStreamField("argumentNumbers", int[].class), //$NON-NLS-1$
            new ObjectStreamField("formats", Format[].class), //$NON-NLS-1$
            new ObjectStreamField("locale", Locale.class), //$NON-NLS-1$
            new ObjectStreamField("maxOffset", Integer.TYPE), //$NON-NLS-1$
            new ObjectStreamField("offsets", int[].class), //$NON-NLS-1$
            new ObjectStreamField("pattern", String.class), }; //$NON-NLS-1$

private void writeObject(ObjectOutputStream stream) throws IOException
{
    ObjectOutputStream.PutField fields = stream.putFields();
    fields.put("argumentNumbers", argumentNumbers); //$NON-NLS-1$
    Format []
    compatibleFormats = formats;
    fields.put("formats", compatibleFormats); //$NON-NLS-1$
    fields.put("locale", locale); //$NON-NLS-1$
    fields.put("maxOffset", maxOffset); //$NON-NLS-1$
        int offset = 0;
        int offsetsLength = maxOffset + 1;
        int[]
    offsets = new int[offsetsLength];
StringBuilder pattern = new StringBuilder();
for (int i = 0; i <= maxOffset; i++)
{
    offset += strings[i].length();
    offsets[i] = offset;
    pattern.append(strings[i]);
}
if (maxOffset + 1 < strings.length)
{
    pattern.append(strings[maxOffset + 1]);
}
fields.put("offsets", offsets); //$NON-NLS-1$
fields.put("pattern", pattern.toString()); //$NON-NLS-1$
stream.writeFields();
    }

    private void readObject(ObjectInputStream stream) throws IOException,
            ClassNotFoundException {
        ObjectInputStream.GetField fields = stream.readFields();
argumentNumbers = (int[])fields.get("argumentNumbers", null); //$NON-NLS-1$
formats = (Format[])fields.get("formats", null); //$NON-NLS-1$
locale = (Locale)fields.get("locale", null); //$NON-NLS-1$
maxOffset = fields.get("maxOffset", 0); //$NON-NLS-1$
int[] offsets = (int[])fields.get("offsets", null); //$NON-NLS-1$
String pattern = (String)fields.get("pattern", null); //$NON-NLS-1$
int length;
if (maxOffset < 0)
{
    length = pattern.length() > 0 ? 1 : 0;
}
else
{
    length = maxOffset
            + (offsets[maxOffset] == pattern.length() ? 1 : 2);
}
strings = new String[length];
int last = 0;
for (int i = 0; i <= maxOffset; i++)
{
    strings[i] = pattern.substring(last, offsets[i]);
    last = offsets[i];
}
if (maxOffset + 1 < strings.length)
{
    strings[strings.length - 1] = pattern.substring(last, pattern
            .length());
}
    }

    /**
     * The instances of this inner class are used as attribute keys in
     * {@code AttributedCharacterIterator} that the
     * {@link MessageFormat#formatToCharacterIterator(Object)} method returns.
     * <p>
     * There is no public constructor in this class, the only instances are the
     * constants defined here.
     */
    public static class Field extends Format.Field
{

        private static final long serialVersionUID = 7899943957617360810L;

/**
 * This constant stands for the message argument.
 */
public static final Field ARGUMENT = new Field("message argument field"); //$NON-NLS-1$

/**
 * Constructs a new instance of {@code MessageFormat.Field} with the
 * given field name.
 *
 * @param fieldName
 *            the field name.
 */
protected Field(String fieldName)
{
    super(fieldName);
}

/**
 * Resolves instances that are deserialized to the constant
 * {@code MessageFormat.Field} values.
 *
 * @return the resolved field object.
 * @throws InvalidObjectException
 *             if an error occurs while resolving the field object.
 */
@Override
        protected Object readResolve() throws InvalidObjectException
{
    String name = this.getName();
            if (name == null) {
        // text.18=Not a valid {0}, subclass should override
        // readResolve()
        throw new InvalidObjectException(Messages.getString(
                "text.18", "MessageFormat.Field")); //$NON-NLS-1$ //$NON-NLS-2$
    }

            if (name.equals(ARGUMENT.getName())) {
        return ARGUMENT;
    }
            // text.18=Not a valid {0}, subclass should override readResolve()
            throw new InvalidObjectException(Messages.getString(
                    "text.18", "MessageFormat.Field")); //$NON-NLS-1$ //$NON-NLS-2$
        }
    }

}
