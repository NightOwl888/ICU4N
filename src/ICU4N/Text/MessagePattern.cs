using ICU4N.Impl;
using ICU4N.Support.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ICU4N.Text
{
    //Note: Minimize ICU dependencies, only use a very small part of the ICU core.
    //In particular, do not depend on *Format classes.

    /// <summary>
    /// Mode for when an apostrophe starts quoted literal text for <see cref="MessageFormat"/> output.
    /// The default is DOUBLE_OPTIONAL unless overridden via <see cref="ICUConfig"/>.
    /// </summary>
    /// <remarks>
    /// A pair of adjacent apostrophes always results in a single apostrophe in the output,
    /// even when the pair is between two single, text-quoting apostrophes.
    /// <para/>
    /// The following table shows examples of desired <see cref="MessageFormat.Format(string, object[])"/> output
    /// with the pattern strings that yield that output.
    /// <list type="table">
    ///     <listheader>
    ///         <term>Desired output</term>
    ///         <term><see cref="DoubleOptional"/></term>
    ///         <term><see cref="DoubleRequired"/></term>
    ///     </listheader>
    ///     <item>
    ///         <term>I see {many}</term>
    ///         <term>I see '{many}'</term>
    ///         <term>(same)</term>
    ///     </item>
    ///     <item>
    ///         <term>I said {'Wow!'}</term>
    ///         <term>I said '{''Wow!''}'</term>
    ///         <term>(same)</term>
    ///     </item>
    ///     <item>
    ///         <term>I don't know</term>
    ///         <term>I don't know OR<br/> I don''t know</term>
    ///         <term>I don''t know</term>
    ///     </item>
    /// </list>
    /// </remarks>
    /// <stable>ICU 4.8</stable>
    public enum ApostropheMode
    {
        /// <summary>
        /// A literal apostrophe is represented by
        /// either a single or a double apostrophe pattern character.
        /// Within a <see cref="MessageFormat"/> pattern, a single apostrophe only starts quoted literal text
        /// if it immediately precedes a curly brace {},
        /// or a pipe symbol | if inside a choice format,
        /// or a pound symbol # if inside a plural format.
        /// <para/>
        /// This is the default behavior starting with ICU 4.8.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        DoubleOptional,

        /// <summary>
        /// A literal apostrophe must be represented by
        /// a double apostrophe pattern character.
        /// A single apostrophe always starts quoted literal text.
        /// <para/>
        /// This is the behavior of ICU 4.6 and earlier, and of the JDK's java.text.MessageFormat.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        DoubleRequired
    }

    /// <summary>
    /// Argument type constants.
    /// Returned by <see cref="MessagePatternPart.ArgType"/> for 
    /// <see cref="MessagePatternPartType.ArgStart"/> and <see cref="MessagePatternPartType.ArgLimit"/> parts.
    /// <para/>
    /// Messages nested inside an argument are each delimited by <see cref="MessagePatternPartType.MsgStart"/> and <see cref="MessagePatternPartType.MsgLimit"/>,
    /// with a nesting level one greater than the surrounding message.
    /// </summary>
    /// <stable>ICU 4.8</stable>
    public enum MessagePatternArgType
    {
        /// <summary>
        /// The argument has no specified type.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        None,

        /// <summary>
        /// The argument has a "simple" type which is provided by the <see cref="MessagePatternPartType.ArgType"/> part.
        /// An <see cref="MessagePatternPartType.ArgStyle"/> part might follow that.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        Simple,

        /// <summary>
        /// The argument is a <see cref="ChoiceFormat"/> with one or more
        /// ((<see cref="MessagePatternPartType.ArgInt"/> | <see cref="MessagePatternPartType.ArgDouble"/>), 
        /// <see cref="MessagePatternPartType.ArgSelector"/>, message) tuples.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        Choice,

        /// <summary>
        /// The argument is a cardinal-number <see cref="PluralFormat"/> with an optional <see cref="MessagePatternPartType.ArgInt"/> 
        /// or <see cref="MessagePatternPartType.ArgDouble"/> offset (e.g., offset:1)
        /// and one or more (<see cref="MessagePatternPartType.ArgSelector"/> [explicit-value] message) tuples.
        /// If the selector has an explicit value (e.g., =2), then that value is provided by the 
        /// <see cref="MessagePatternPartType.ArgInt"/>  or <see cref="MessagePatternPartType.ArgDouble"/> part preceding the message.
        /// Otherwise the message immediately follows the <see cref="MessagePatternPartType.ArgSelector"/>.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        Plural,

        /// <summary>
        /// The argument is a <see cref="SelectFormat"/> with one or more (<see cref="MessagePatternPartType.ArgSelector"/>, message) pairs.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        Select,

        /// <summary>
        /// The argument is an ordinal-number <see cref="PluralFormat"/>
        /// with the same style parts sequence and semantics as <see cref="MessagePatternArgType.Plural"/>
        /// </summary>
        /// <stable>ICU 50</stable>
        SelectOrdinal
    }

    /// <summary>
    /// Part type constants.
    /// </summary>
    /// <stable>ICU 4.8</stable>
    public enum MessagePatternPartType // ICU4N specific - renamed from MessagePattern.Part.Type
    {
        /// <summary>
        /// Start of a message pattern (main or nested).
        /// The length is 0 for the top-level message
        /// and for a choice argument sub-message, otherwise 1 for the '{'.
        /// The value indicates the nesting level, starting with 0 for the main message.
        /// <para/>
        /// There is always a later <see cref="MsgLimit"/> part.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        MsgStart,

        /// <summary>
        /// End of a message pattern (main or nested).
        /// The length is 0 for the top-level message and
        /// the last sub-message of a choice argument,
        /// otherwise 1 for the '}' or (in a choice argument style) the '|'.
        /// The value indicates the nesting level, starting with 0 for the main message.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        MsgLimit,

        /// <summary>
        /// Indicates a substring of the pattern string which is to be skipped when formatting.
        /// For example, an apostrophe that begins or ends quoted text
        /// would be indicated with such a part.
        /// The value is undefined and currently always 0.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        SkipSyntax,

        /// <summary>
        /// Indicates that a syntax character needs to be inserted for auto-quoting.
        /// The length is 0.
        /// The value is the character code of the insertion character. (U+0027=APOSTROPHE)
        /// </summary>
        /// <stable>ICU 4.8</stable>
        InsertChar,

        /// <summary>
        /// Indicates a syntactic (non-escaped) # symbol in a plural variant.
        /// When formatting, replace this part's substring with the
        /// (value-offset) for the plural argument value.
        /// The value is undefined and currently always 0.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        ReplaceNumber,

        /// <summary>
        /// Start of an argument.
        /// The length is 1 for the '{'.
        /// The value is the ordinal value of the <see cref="MessagePatternArgType"/>.
        /// Use <see cref="MessagePatternPart.ArgType"/>.
        /// <para/>
        /// This part is followed by either an <see cref="ArgNumber"/> or <see cref="ArgName"/>,
        /// followed by optional argument sub-parts (see <see cref="MessagePatternArgType"/>)
        /// and finally an <see cref="ArgLimit"/> part.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        ArgStart,

        /// <summary>
        /// End of an argument.
        /// The length is 1 for the '}'.
        /// The value is the ordinal value of the <see cref="MessagePatternArgType"/>. 
        /// Use <see cref="MessagePatternPart.ArgType"/>.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        ArgLimit,

        /// <summary>
        /// The argument number, provided by the value.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        ArgNumber,

        /// <summary>
        /// The argument name.
        /// The value is undefined and currently always 0.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        ArgName,

        /// <summary>
        /// The argument type.
        /// The value is undefined and currently always 0.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        ArgType,

        /// <summary>
        /// The argument style text.
        /// The value is undefined and currently always 0.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        ArgStyle,

        /// <summary>
        /// A selector substring in a "complex" argument style.
        /// The value is undefined and currently always 0.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        ArgSelector,

        /// <summary>
        /// An integer value, for example the offset or an explicit selector value
        /// in a <see cref="PluralFormat"/> style.
        /// The part value is the integer value.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        ArgInt,

        /// <summary>
        /// A numeric value, for example the offset or an explicit selector value
        /// in a <see cref="PluralFormat"/> style.
        /// The part value is an index into an internal array of numeric values;
        /// use <see cref="MessagePattern.GetNumericValue(MessagePatternPart)"/>.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        ArgDouble
    }

    /// <summary>
    /// Extension methods for <see cref="MessagePatternArgType"/> and <see cref="MessagePatternPartType"/>.
    /// </summary>
    public static class MessagePatternEnumExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="argType"></param>
        /// <returns>true if the argument type has a plural style part sequence and semantics,
        /// for example <see cref="MessagePatternArgType.Plural"/> and <see cref="MessagePatternArgType.SelectOrdinal"/>.
        /// </returns>
        public static bool HasPluralStyle(this MessagePatternArgType argType)
        {
            return argType == MessagePatternArgType.Plural || argType == MessagePatternArgType.SelectOrdinal;
        }

        /// <summary>
        /// Indicates whether this part has a numeric value.
        /// If so, then that numeric value can be retrieved via <see cref="MessagePattern.GetNumericValue(MessagePatternPart)"/>
        /// </summary>
        /// <param name="partType"></param>
        /// <returns>true if this part has a numeric value.</returns>
        /// <stable>ICU 4.8</stable>
        public static bool HasNumericValue(this MessagePatternPartType partType)
        {
            return partType == MessagePatternPartType.ArgInt || partType == MessagePatternPartType.ArgDouble;
        }
    }

    /// <summary>
    /// A message pattern "part", representing a pattern parsing event.
    /// There is a part for the start and end of a message or argument,
    /// for quoting and escaping of and with ASCII apostrophes,
    /// and for syntax elements of "complex" arguments.
    /// </summary>
    /// <stable>ICU 4.8</stable>
    public sealed class MessagePatternPart // ICU4N renamed from MessagePattern.Part
#if FEATURE_CLONEABLE
        : ICloneable
#endif
    {
        internal MessagePatternPart(MessagePatternPartType t, int i, int l, int v)
        {
            type = t;
            index = i;
            length = (char)l;
            value = (short)v;
        }

        /// <summary>
        /// Returns the type of this <see cref="MessagePatternPart"/>.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public MessagePatternPartType Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets the pattern string index associated with this <see cref="MessagePatternPart"/>.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public int Index
        {
            get { return index; }
        }

        /// <summary>
        /// Gets the length of the pattern substring associated with this <see cref="MessagePatternPart"/>.
        /// This is 0 for some parts.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public int Length
        {
            get { return length; }
        }

        /// <summary>
        /// Gets the pattern string limit (exclusive-end) index associated with this <see cref="MessagePatternPart"/>.
        /// Convenience method for <see cref="Index"/> + <see cref="Length"/>.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public int Limit
        {
            get { return index + length; }
        }

        /// <summary>
        /// Gets a value associated with this part.
        /// See the documentation of each part type for details.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public short Value
        {
            get { return value; }
            internal set { this.value = value; }
        }

        /// <summary>
        /// Gets the argument type if this part is of type <see cref="MessagePatternPartType.ArgStart"/> or 
        /// <see cref="MessagePatternPartType.ArgLimit"/>, otherwise <see cref="MessagePatternArgType.None"/>.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public MessagePatternArgType ArgType
        {
            get
            {
                MessagePatternPartType type = Type;
                if (type == MessagePatternPartType.ArgStart || type == MessagePatternPartType.ArgLimit)
                {
                    return argTypes[value];
                }
                else
                {
                    return MessagePatternArgType.None;
                }
            }
        }

        // ICU4N specific - de-nested Type and renamed MessagePatternPartType

        /// <returns>a string representation of this part.</returns>
        /// <stable>ICU 4.8</stable>
        public override string ToString()
        {
            string valueString = (type == MessagePatternPartType.ArgStart || type == MessagePatternPartType.ArgLimit) ?
                ArgType.ToString() : ((int)value).ToString();
            return type.ToString() + "(" + valueString + ")@" + index;
        }

        /// <param name="other">another object to compare with.</param>
        /// <returns>true if this object is equivalent to the other one.</returns>
        /// <stable>ICU 4.8</stable>
        public override bool Equals(object other)
        {
            if (this == other)
            {
                return true;
            }
            if (other == null || GetType() != other.GetType())
            {
                return false;
            }
            MessagePatternPart o = (MessagePatternPart)other;
            return
                type.Equals(o.type) &&
                index == o.index &&
                length == o.length &&
                value == o.value &&
                limitPartIndex == o.limitPartIndex;
        }

        /// <stable>ICU 4.8</stable>
        public override int GetHashCode()
        {
            return ((type.GetHashCode() * 37 + index) * 37 + length) * 37 + value;
        }

        /// <summary>
        /// Creates a shallow copy of this object.
        /// </summary>
        /// <draft>ICU4N 60.1</draft>
        public object Clone()
        {
            return base.MemberwiseClone();
        }

        internal const int MAX_LENGTH = 0xffff;
        internal const int MAX_VALUE = short.MaxValue;

        // Some fields are not final because they are modified during pattern parsing.
        // After pattern parsing, the parts are effectively immutable.
        private readonly MessagePatternPartType type;
        private readonly int index;
        private readonly char length;
        private short value;
        internal int limitPartIndex;

        private static readonly MessagePatternArgType[] argTypes = (MessagePatternArgType[])Enum.GetValues(typeof(MessagePatternArgType));
    }

    /// <summary>
    /// Parses and represents ICU <see cref="MessageFormat"/> patterns.
    /// Also handles patterns for <see cref="ChoiceFormat"/>, <see cref="PluralFormat"/> and <see cref="SelectFormat"/>.
    /// Used in the implementations of those classes as well as in tools
    /// for message validation, translation and format conversion.
    /// </summary>
    /// <remarks>
    /// The parser handles all syntax relevant for identifying message arguments.
    /// This includes "complex" arguments whose style strings contain
    /// nested MessageFormat pattern substrings.
    /// For "simple" arguments (with no nested <see cref="MessageFormat"/> pattern substrings),
    /// the argument style is not parsed any further.
    /// <para/>
    /// The parser handles named and numbered message arguments and allows both in one message.
    /// <para/>
    /// Once a pattern has been parsed successfully, iterate through the parsed data
    /// with <see cref="CountParts()"/>, <see cref="GetPart(int)"/> and related methods.
    /// <para/>
    /// The data logically represents a parse tree, but is stored and accessed
    /// as a list of "parts" for fast and simple parsing and to minimize object allocations.
    /// Arguments and nested messages are best handled via recursion.
    /// For every _START "part", <see cref="GetLimitPartIndex(int)"/> efficiently returns
    /// the index of the corresponding _LIMIT "part".
    /// <para/>
    /// List of "parts":
    /// <code>
    /// message = MSG_START (SKIP_SYNTAX | INSERT_CHAR | REPLACE_NUMBER | argument)* MSG_LIMIT
    /// argument = noneArg | simpleArg | complexArg
    /// complexArg = choiceArg | pluralArg | selectArg
    /// 
    /// noneArg = ARG_START.NONE (ARG_NAME | ARG_NUMBER) ARG_LIMIT.NONE
    /// simpleArg = ARG_START.SIMPLE (ARG_NAME | ARG_NUMBER) ARG_TYPE [ARG_STYLE] ARG_LIMIT.SIMPLE
    /// choiceArg = ARG_START.CHOICE (ARG_NAME | ARG_NUMBER) choiceStyle ARG_LIMIT.CHOICE
    /// pluralArg = ARG_START.PLURAL (ARG_NAME | ARG_NUMBER) pluralStyle ARG_LIMIT.PLURAL
    /// selectArg = ARG_START.SELECT (ARG_NAME | ARG_NUMBER) selectStyle ARG_LIMIT.SELECT
    /// 
    /// choiceStyle = ((ARG_INT | ARG_DOUBLE) ARG_SELECTOR message)+
    /// pluralStyle = [ARG_INT | ARG_DOUBLE] (ARG_SELECTOR [ARG_INT | ARG_DOUBLE] message)+
    /// selectStyle = (ARG_SELECTOR message)+
    /// </code>
    /// <list type="bullet">
    ///     <item>
    ///         <description>
    ///             Literal output text is not represented directly by "parts" but accessed
    ///             between parts of a message, from one part's <see cref="MessagePatternPart.Limit"/> 
    ///             to the next part's <see cref="MessagePatternPart.Index"/>.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             <c>ARG_START.CHOICE</c> stands for an <see cref="MessagePatternPartType.ArgStart"/> <see cref="MessagePatternPart"/> 
    ///             with <see cref="MessagePatternArgType.Choice"/>.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             In the choiceStyle, the <see cref="MessagePatternPartType.ArgSelector"/> has the '&lt;', the '#' or
    ///             the less-than-or-equal-to sign (U+2264).
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             In the pluralStyle, the first, optional numeric Part has the "offset:" value.
    ///             The optional numeric <see cref="MessagePatternPart"/> between each 
    ///             (<see cref="MessagePatternPartType.ArgSelector"/>, message) pair
    ///             is the value of an explicit-number selector like "=2",
    ///             otherwise the selector is a non-numeric identifier.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             The <see cref="MessagePatternPartType.ReplaceNumber"/> <see cref="MessagePatternPart"/> can occur only 
    ///             in an immediate sub-message of the pluralStyle.
    ///         </description>
    ///     </item>
    /// </list>
    /// <para/>
    /// This class is not intended for public subclassing.
    /// </remarks>
    /// <stable>ICU 4.8</stable>
    /// <author>Markus Scherer</author>
    public sealed class MessagePattern : IFreezable<MessagePattern>
#if FEATURE_CLONEABLE
        , ICloneable
#endif
    {
        /// <summary>
        /// Constructs an empty <see cref="MessagePattern"/> with default <see cref="Text.ApostropheMode"/>.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public MessagePattern()
        {
            aposMode = defaultAposMode;
        }

        /// <summary>
        /// Constructs an empty <see cref="MessagePattern"/>.
        /// </summary>
        /// <param name="mode">Explicit <see cref="Text.ApostropheMode"/>.</param>
        /// <stable>ICU 4.8</stable>
        public MessagePattern(ApostropheMode mode)
        {
            aposMode = mode;
        }

        /// <summary>
        /// Constructs a <see cref="MessagePattern"/> with default <see cref="Text.ApostropheMode"/> and
        /// parses the <see cref="MessageFormat"/> pattern string.
        /// </summary>
        /// <param name="pattern">a <see cref="MessageFormat"/> pattern string</param>
        /// <exception cref="ArgumentException">for syntax errors in the pattern string</exception>
        /// <exception cref="IndexOutOfRangeException">if certain limits are exceeded
        /// (e.g., argument number too high, argument name too long, etc.)</exception>
        /// <exception cref="FormatException">if a number could not be parsed</exception>
        /// <stable>ICU 4.8</stable>
        public MessagePattern(string pattern)
        {
            aposMode = defaultAposMode;
            Parse(pattern);
        }

        /// <summary>
        /// Parses a <see cref="MessageFormat"/> pattern string.
        /// </summary>
        /// <param name="pattern">a <see cref="MessageFormat"/> pattern string</param>
        /// <returns>this</returns>
        /// <exception cref="ArgumentException">for syntax errors in the pattern string</exception>
        /// <exception cref="IndexOutOfRangeException">if certain limits are exceeded
        /// (e.g., argument number too high, argument name too long, etc.)</exception>
        /// <exception cref="FormatException">if a number could not be parsed</exception>
        /// <stable>ICU 4.8</stable>
        public MessagePattern Parse(string pattern)
        {
            PreParse(pattern);
            ParseMessage(0, 0, 0, MessagePatternArgType.None);
            PostParse();
            return this;
        }

        /// <summary>
        /// Parses a <see cref="ChoiceFormat"/> pattern string.
        /// </summary>
        /// <returns>this</returns>
        /// <exception cref="ArgumentException">for syntax errors in the pattern string</exception>
        /// <exception cref="IndexOutOfRangeException">if certain limits are exceeded
        /// (e.g., argument number too high, argument name too long, etc.)</exception>
        /// <exception cref="FormatException">if a number could not be parsed</exception>
        /// <stable>ICU 4.8</stable>
        public MessagePattern ParseChoiceStyle(string pattern)
        {
            PreParse(pattern);
            ParseChoiceStyle(0, 0);
            PostParse();
            return this;
        }

        /// <summary>
        /// Parses a <see cref="PluralFormat"/> pattern string.
        /// </summary>
        /// <param name="pattern">A <see cref="PluralFormat"/> pattern string</param>
        /// <returns>this</returns>
        /// <exception cref="ArgumentException">for syntax errors in the pattern string</exception>
        /// <exception cref="IndexOutOfRangeException">if certain limits are exceeded
        /// (e.g., argument number too high, argument name too long, etc.)</exception>
        /// <exception cref="FormatException">if a number could not be parsed</exception>
        /// <stable>ICU 4.8</stable>
        public MessagePattern ParsePluralStyle(string pattern)
        {
            PreParse(pattern);
            ParsePluralOrSelectStyle(MessagePatternArgType.Plural, 0, 0);
            PostParse();
            return this;
        }

        /// <summary>
        /// Parses a <see cref="SelectFormat"/> pattern string.
        /// </summary>
        /// <param name="pattern">A <see cref="SelectFormat"/> pattern string</param>
        /// <returns>this</returns>
        /// <exception cref="ArgumentException">for syntax errors in the pattern string</exception>
        /// <exception cref="IndexOutOfRangeException">if certain limits are exceeded
        /// (e.g., argument number too high, argument name too long, etc.)</exception>
        /// <exception cref="FormatException">if a number could not be parsed</exception>
        /// <stable>ICU 4.8</stable>
        public MessagePattern ParseSelectStyle(string pattern)
        {
            PreParse(pattern);
            ParsePluralOrSelectStyle(MessagePatternArgType.Select, 0, 0);
            PostParse();
            return this;
        }

        /// <summary>
        /// Clears this <see cref="MessagePattern"/>.
        /// <see cref="CountParts()"/> will return 0.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public void Clear()
        {
            // Mostly the same as preParse().
            if (IsFrozen)
            {
                throw new InvalidOperationException(
                    "Attempt to clear() a frozen MessagePattern instance.");
            }
            msg = null;
            hasArgNames = hasArgNumbers = false;
            needsAutoQuoting = false;
            parts.Clear();
            if (numericValues != null)
            {
                numericValues.Clear();
            }
        }

        /// <summary>
        /// Clears this <see cref="MessagePattern"/> and sets the <see cref="Text.ApostropheMode"/>.
        /// <see cref="CountParts()"/> will return 0.
        /// </summary>
        /// <param name="mode">The new <see cref="Text.ApostropheMode"/></param>
        /// <stable>ICU 4.8</stable>
        public void ClearPatternAndSetApostropheMode(ApostropheMode mode)
        {
            Clear();
            aposMode = mode;
        }

        /// <param name="other">another object to compare with.</param>
        /// <returns>true if this object is equivalent to the other one.</returns>
        /// <stable>ICU 4.8</stable>
        public override bool Equals(object other)
        {
            if (this == other)
            {
                return true;
            }
            if (other == null || GetType() != other.GetType())
            {
                return false;
            }
            MessagePattern o = (MessagePattern)other;
            return
                aposMode.Equals(o.aposMode) &&
                (msg == null ? o.msg == null : msg.Equals(o.msg)) &&
                parts.Equals(o.parts);
            // No need to compare numericValues if msg and parts are the same.
        }

        /// <stable>ICU 4.8</stable>
        public override int GetHashCode()
        {
            return (aposMode.GetHashCode() * 37 + (msg != null ? msg.GetHashCode() : 0)) * 37 + parts.GetHashCode();
        }

        /// <summary>
        /// Gets this instance's <see cref="Text.ApostropheMode"/>.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public ApostropheMode ApostropheMode
        {
            get { return aposMode; }
        }

        /// <summary>
        /// Gets true if <see cref="ApostropheMode"/> == <see cref="ApostropheMode.DoubleRequired"/>
        /// </summary>
        /// <internal/>
        internal bool JdkAposMode // ICU4N TODO: API - is this required in .NET?
        {
            get { return aposMode == ApostropheMode.DoubleRequired; }
        }

        /// <summary>
        /// Gets the parsed pattern string (null if none was parsed).
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public string PatternString
        {
            get { return msg; }
        }

        /// <summary>
        /// Does the parsed pattern have named arguments like {first_name}?
        /// Returns true if the parsed pattern has at least one named argument.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public bool HasNamedArguments
        {
            get { return hasArgNames; }
        }

        /// <summary>
        /// Does the parsed pattern have numbered arguments like {2}?
        /// Returns true if the parsed pattern has at least one numbered argument.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public bool HasNumberedArguments
        {
            get { return hasArgNumbers; }
        }

        /// <stable>ICU 4.8</stable>
        public override string ToString()
        {
            return msg;
        }

        /// <summary>
        /// Validates and parses an argument name or argument number string.
        /// An argument name must be a "pattern identifier", that is, it must contain
        /// no Unicode <see cref="ICU4N.Globalization.UProperty.Pattern_Syntax"/> or 
        /// <see cref="ICU4N.Globalization.UProperty.Pattern_White_Space"/> characters.
        /// If it only contains ASCII digits, then it must be a small integer with no leading zero.
        /// </summary>
        /// <param name="name">Input string.</param>
        /// <returns>&gt;=0 if the name is a valid number,
        /// <see cref="ArgNameNotNumber"/> (-1) if it is a "pattern identifier" but not all ASCII digits,
        /// <see cref="ArgNameNotValid"/> (-2) if it is neither.</returns>
        /// <stable>ICU 4.8</stable>
        public static int ValidateArgumentName(string name)
        {
            if (!PatternProps.IsIdentifier(name))
            {
                return ArgNameNotValid;
            }
            return ParseArgNumber(name, 0, name.Length);
        }

        /// <summary>
        /// Return value from <see cref="ValidateArgumentName(string)"/> for when
        /// the string is a valid "pattern identifier" but not a number.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public static readonly int ArgNameNotNumber = -1;

        /// <summary>
        /// Return value from <see cref="ValidateArgumentName(string)"/> for when
        /// the string is invalid.
        /// It might not be a valid "pattern identifier",
        /// or it have only ASCII digits but there is a leading zero or the number is too large.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public static readonly int ArgNameNotValid = -2;

        /// <summary>
        /// Returns a version of the parsed pattern string where each ASCII apostrophe
        /// is doubled (escaped) if it is not already, and if it is not interpreted as quoting syntax.
        /// <para/>
        /// For example, this turns "I don't '{know}' {gender,select,female{h''er}other{h'im}}."
        /// into "I don''t '{know}' {gender,select,female{h''er}other{h''im}}."
        /// </summary>
        /// <returns>The deep-auto-quoted version of the parsed pattern string.</returns>
        /// <seealso cref="MessageFormat.AutoQuoteApostrophe(string)"/>
        /// <stable>ICU 4.8</stable>
        public string AutoQuoteApostropheDeep()
        {
            if (!needsAutoQuoting)
            {
                return msg;
            }
            StringBuilder modified = null;
            // Iterate backward so that the insertion indexes do not change.
            int count = CountParts();
            for (int i = count; i > 0;)
            {
                MessagePatternPart part;
                if ((part = GetPart(--i)).Type == MessagePatternPartType.InsertChar)
                {
                    if (modified == null)
                    {
                        modified = new StringBuilder(msg.Length + 10).Append(msg);
                    }
                    modified.Insert(part.Index, (char)part.Value);
                }
            }
            if (modified == null)
            {
                return msg;
            }
            else
            {
                return modified.ToString();
            }
        }

        /// <summary>
        /// Returns the number of "parts" created by parsing the pattern string.
        /// Returns 0 if no pattern has been parsed or <see cref="Clear()"/> was called.
        /// </summary>
        /// <returns>the number of pattern parts.</returns>
        /// <stable>ICU 4.8</stable>
        public int CountParts()
        {
            return parts.Count;
        }

        /// <summary>
        /// Gets the i-th pattern "part".
        /// </summary>
        /// <param name="i">The index of the <see cref="MessagePatternPart"/> data. (0..<see cref="CountParts()"/>-1)</param>
        /// <returns>the i-th pattern "part".</returns>
        /// <exception cref="IndexOutOfRangeException">if partIndex is outside the (0..<see cref="CountParts()"/>-1) range</exception>
        /// <stable>ICU 4.8</stable>
        public MessagePatternPart GetPart(int i)
        {
            return parts[i];
        }

        /// <summary>
        /// Returns the <see cref="MessagePatternPartType"/> of the i-th pattern "part".
        /// Convenience method for <c>GetPart(i).Type</c>.
        /// </summary>
        /// <param name="i">The index of the Part data. (0..<see cref="CountParts()"/>-1)</param>
        /// <returns>The <see cref="MessagePatternPartType"/> of the i-th <see cref="MessagePatternPart"/>.</returns>
        /// <exception cref="IndexOutOfRangeException">if partIndex is outside the (0..<see cref="CountParts()"/>-1) range</exception>
        /// <stable>ICU 4.8</stable>
        public MessagePatternPartType GetPartType(int i)
        {
            return parts[i].Type;
        }

        /// <summary>
        /// Returns the pattern index of the specified pattern "part".
        /// Convenience method for <c>GetPart(partIndex).Index</c>.
        /// </summary>
        /// <param name="partIndex">The index of the Part data. (0..<see cref="CountParts()"/>-1)</param>
        /// <returns>The pattern index of this <see cref="MessagePatternPart"/>.</returns>
        /// <exception cref="IndexOutOfRangeException">if partIndex is outside the (0..<see cref="CountParts()"/>-1) range</exception>
        /// <stable>ICU 4.8</stable>
        public int GetPatternIndex(int partIndex)
        {
            return parts[partIndex].Index;
        }

        /// <summary>
        /// Returns the substring of the pattern string indicated by the <paramref name="part"/>.
        /// Convenience method for <c>PatternString.Substring(part.Index, part.Limit - part.Index)</c>.
        /// </summary>
        /// <param name="part">A part of this <see cref="MessagePattern"/>.</param>
        /// <returns>The substring associated with <paramref name="part"/>.</returns>
        /// <stable>ICU 4.8</stable>
        public string GetSubstring(MessagePatternPart part)
        {
            int index = part.Index;
            return msg.Substring(index, part.Length); // ICU4N: (index + part.Length) - index = part.Length
        }

        /// <summary>
        /// Compares the <paramref name="part"/>'s substring with the input string <paramref name="s"/>.
        /// </summary>
        /// <param name="part">A part of this <see cref="MessagePattern"/>.</param>
        /// <param name="s">A string.</param>
        /// <returns>true if <c>GetSubstring(part).Equals(s)</c>.</returns>
        /// <stable>ICU 4.8</stable>
        public bool PartSubstringMatches(MessagePatternPart part, string s)
        {
            return part.Length == s.Length && msg.RegionMatches(part.Index, s, 0, part.Length);
        }

        /// <summary>
        /// Returns the numeric value associated with an <see cref="MessagePatternPartType.ArgInt"/> 
        /// or <see cref="MessagePatternPartType.ArgDouble"/>.
        /// </summary>
        /// <param name="part">A part of this <see cref="MessagePattern"/>.</param>
        /// <returns>The part's numeric value, or <see cref="NoNumericValue"/> if this is not a numeric part.</returns>
        /// <stable>ICU 4.8</stable>
        public double GetNumericValue(MessagePatternPart part)
        {
            MessagePatternPartType type = part.Type;
            if (type == MessagePatternPartType.ArgInt)
            {
                return part.Value;
            }
            else if (type == MessagePatternPartType.ArgDouble)
            {
                return numericValues[part.Value];
            }
            else
            {
                return NoNumericValue;
            }
        }

        /// <summary>
        /// Special value that is returned by <see cref="GetNumericValue(MessagePatternPart)"/> when no
        /// numeric value is defined for a part.
        /// </summary>
        /// <seealso cref="GetNumericValue(MessagePatternPart)"/>
        /// <stable>ICU 4.8</stable>
        public static readonly double NoNumericValue = -123456789;

        /// <summary>
        /// Returns the "offset:" value of a <see cref="PluralFormat"/> argument, or 0 if none is specified.
        /// </summary>
        /// <param name="pluralStart">the index of the first <see cref="PluralFormat"/> argument style part. (0..<see cref="CountParts()"/>-1)</param>
        /// <returns>the "offset:" value.</returns>
        /// <exception cref="IndexOutOfRangeException">if start is outside the (0..<see cref="CountParts()"/>-1) range.</exception>
        /// <stable>ICU 4.8</stable>
        public double GetPluralOffset(int pluralStart)
        {
            MessagePatternPart part = parts[pluralStart];
            if (part.Type.HasNumericValue())
            {
                return GetNumericValue(part);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns the index of the <see cref="MessagePatternPartType.ArgLimit"/>|<see cref="MessagePatternPartType.MsgLimit"/> 
        /// part corresponding to the <see cref="MessagePatternPartType.ArgStart"/>|<see cref="MessagePatternPartType.MsgStart"/> 
        /// at <paramref name="start"/>.
        /// </summary>
        /// <param name="start">The index of some <see cref="MessagePatternPart"/> data (0..countParts()-1);
        /// this <see cref="MessagePatternPart"/> should be of <see cref="MessagePatternPart.Type"/> 
        /// <see cref="MessagePatternPartType.ArgStart"/> or <see cref="MessagePatternPartType.MsgStart"/>.</param>
        /// <returns>The first i&gt;start where GetPart(i).Type==<see cref="MessagePatternPartType.ArgLimit"/>|<see cref="MessagePatternPartType.MsgLimit"/> at the same nesting level,
        /// or start itself if getPartType(msgStart)!=ARG|MSG_START.</returns>
        /// <exception cref="IndexOutOfRangeException">if start is outside the (0..<see cref="CountParts()"/>-1) range.</exception>
        /// <stable>ICU 4.8</stable>
        public int GetLimitPartIndex(int start)
        {
            int limit = parts[start].limitPartIndex;
            if (limit < start)
            {
                return start;
            }
            return limit;
        }

        // ICU4N specific - de-nested Part class

        /// <summary>
        /// Creates and returns a copy of this object.
        /// </summary>
        /// <returns>A copy of this object (or itself if frozen).</returns>
        /// <stable>ICU 4.8</stable>
        public object Clone()
        {
            if (IsFrozen)
            {
                return this;
            }
            else
            {
                return CloneAsThawed();
            }
        }

        /// <summary>
        /// Creates and returns an unfrozen copy of this object.
        /// </summary>
        /// <returns>A copy of this object.</returns>
        /// <stable>ICU 4.8</stable>
        public MessagePattern CloneAsThawed()
        {
            MessagePattern newMsg = (MessagePattern)base.MemberwiseClone();

            // Clone the list
            var newParts = new List<MessagePatternPart>();
            foreach (var part in parts)
                newParts.Add((MessagePatternPart)part.Clone());

            newMsg.parts = newParts; 
            if (numericValues != null)
            {
                newMsg.numericValues = numericValues.ToList(); // Clone the list
            }
            newMsg.frozen = false;
            return newMsg;
        }

        /// <summary>
        /// Freezes this object, making it immutable and thread-safe.
        /// </summary>
        /// <returns>this</returns>
        /// <stable>ICU 4.8</stable>
        public MessagePattern Freeze()
        {
            frozen = true;
            return this;
        }

        /// <summary>
        /// Gets whether this object is frozen (immutable) or not.
        /// Returns true if this object is frozen.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public bool IsFrozen => frozen;

        private void PreParse(string pattern)
        {
            if (IsFrozen)
            {
                throw new InvalidOperationException(
                    "Attempt to parse(" + Prefix(pattern) + ") on frozen MessagePattern instance.");
            }
            msg = pattern;
            hasArgNames = hasArgNumbers = false;
            needsAutoQuoting = false;
            parts.Clear();
            if (numericValues != null)
            {
                numericValues.Clear();
            }
        }

        private void PostParse()
        {
            // Nothing to be done currently.
        }

        private int ParseMessage(int index, int msgStartLength, int nestingLevel, MessagePatternArgType parentType)
        {
            if (nestingLevel > MessagePatternPart.MAX_VALUE)
            {
                throw new IndexOutOfRangeException();
            }
            int msgStart = parts.Count;
            AddPart(MessagePatternPartType.MsgStart, index, msgStartLength, nestingLevel);
            index += msgStartLength;
            while (index < msg.Length)
            {
                char c = msg[index++];
                if (c == '\'')
                {
                    if (index == msg.Length)
                    {
                        // The apostrophe is the last character in the pattern.
                        // Add a Part for auto-quoting.
                        AddPart(MessagePatternPartType.InsertChar, index, 0, '\'');  // value=char to be inserted
                        needsAutoQuoting = true;
                    }
                    else
                    {
                        c = msg[index];
                        if (c == '\'')
                        {
                            // double apostrophe, skip the second one
                            AddPart(MessagePatternPartType.SkipSyntax, index++, 1, 0);
                        }
                        else if (
                          aposMode == ApostropheMode.DoubleRequired ||
                          c == '{' || c == '}' ||
                          (parentType == MessagePatternArgType.Choice && c == '|') ||
                          (parentType.HasPluralStyle() && c == '#')
                      )
                        {
                            // skip the quote-starting apostrophe
                            AddPart(MessagePatternPartType.SkipSyntax, index - 1, 1, 0);
                            // find the end of the quoted literal text
                            for (; ; )
                            {
                                index = msg.IndexOf('\'', index + 1);
                                if (index >= 0)
                                {
                                    if ((index + 1) < msg.Length && msg[index + 1] == '\'')
                                    {
                                        // double apostrophe inside quoted literal text
                                        // still encodes a single apostrophe, skip the second one
                                        AddPart(MessagePatternPartType.SkipSyntax, ++index, 1, 0);
                                    }
                                    else
                                    {
                                        // skip the quote-ending apostrophe
                                        AddPart(MessagePatternPartType.SkipSyntax, index++, 1, 0);
                                        break;
                                    }
                                }
                                else
                                {
                                    // The quoted text reaches to the end of the of the message.
                                    index = msg.Length;
                                    // Add a Part for auto-quoting.
                                    AddPart(MessagePatternPartType.InsertChar, index, 0, '\'');  // value=char to be inserted
                                    needsAutoQuoting = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // Interpret the apostrophe as literal text.
                            // Add a Part for auto-quoting.
                            AddPart(MessagePatternPartType.InsertChar, index, 0, '\'');  // value=char to be inserted
                            needsAutoQuoting = true;
                        }
                    }
                }
                else if (parentType.HasPluralStyle() && c == '#')
                {
                    // The unquoted # in a plural message fragment will be replaced
                    // with the (number-offset).
                    AddPart(MessagePatternPartType.ReplaceNumber, index - 1, 1, 0);
                }
                else if (c == '{')
                {
                    index = ParseArg(index - 1, 1, nestingLevel);
                }
                else if ((nestingLevel > 0 && c == '}') || (parentType == MessagePatternArgType.Choice && c == '|'))
                {
                    // Finish the message before the terminator.
                    // In a choice style, report the "}" substring only for the following ARG_LIMIT,
                    // not for this MSG_LIMIT.
                    int limitLength = (parentType == MessagePatternArgType.Choice && c == '}') ? 0 : 1;
                    AddLimitPart(msgStart, MessagePatternPartType.MsgLimit, index - 1, limitLength, nestingLevel);
                    if (parentType == MessagePatternArgType.Choice)
                    {
                        // Let the choice style parser see the '}' or '|'.
                        return index - 1;
                    }
                    else
                    {
                        // continue parsing after the '}'
                        return index;
                    }
                }  // else: c is part of literal text
            }
            if (nestingLevel > 0 && !InTopLevelChoiceMessage(nestingLevel, parentType))
            {
                throw new ArgumentException(
                    "Unmatched '{' braces in message " + Prefix());
            }
            AddLimitPart(msgStart, MessagePatternPartType.MsgLimit, index, 0, nestingLevel);
            return index;
        }

        private int ParseArg(int index, int argStartLength, int nestingLevel)
        {
            int argStart = parts.Count;
            MessagePatternArgType argType = MessagePatternArgType.None;
            AddPart(MessagePatternPartType.ArgStart, index, argStartLength, (int)argType);
            int nameIndex = index = SkipWhiteSpace(index + argStartLength);
            if (index == msg.Length)
            {
                throw new ArgumentException(
                    "Unmatched '{' braces in message " + Prefix());
            }
            // parse argument name or number
            index = SkipIdentifier(index);
            int number = ParseArgNumber(nameIndex, index);
            if (number >= 0)
            {
                int length = index - nameIndex;
                if (length > MessagePatternPart.MAX_LENGTH || number > MessagePatternPart.MAX_VALUE)
                {
                    throw new IndexOutOfRangeException(
                        "Argument number too large: " + Prefix(nameIndex));
                }
                hasArgNumbers = true;
                AddPart(MessagePatternPartType.ArgNumber, nameIndex, length, number);
            }
            else if (number == ArgNameNotNumber)
            {
                int length = index - nameIndex;
                if (length > MessagePatternPart.MAX_LENGTH)
                {
                    throw new IndexOutOfRangeException(
                        "Argument name too long: " + Prefix(nameIndex));
                }
                hasArgNames = true;
                AddPart(MessagePatternPartType.ArgName, nameIndex, length, 0);
            }
            else
            {  // number<-1 (ARG_NAME_NOT_VALID)
                throw new ArgumentException("Bad argument syntax: " + Prefix(nameIndex));
            }
            index = SkipWhiteSpace(index);
            if (index == msg.Length)
            {
                throw new ArgumentException(
                    "Unmatched '{' braces in message " + Prefix());
            }
            char c = msg[index];
            if (c == '}')
            {
                // all done
            }
            else if (c != ',')
            {
                throw new ArgumentException("Bad argument syntax: " + Prefix(nameIndex));
            }
            else /* ',' */
            {
                // parse argument type: case-sensitive a-zA-Z
                int typeIndex = index = SkipWhiteSpace(index + 1);
                while (index < msg.Length && IsArgTypeChar(msg[index]))
                {
                    ++index;
                }
                int length = index - typeIndex;
                index = SkipWhiteSpace(index);
                if (index == msg.Length)
                {
                    throw new ArgumentException(
                        "Unmatched '{' braces in message " + Prefix());
                }
                if (length == 0 || ((c = msg[index]) != ',' && c != '}'))
                {
                    throw new ArgumentException("Bad argument syntax: " + Prefix(nameIndex));
                }
                if (length > MessagePatternPart.MAX_LENGTH)
                {
                    throw new IndexOutOfRangeException(
                        "Argument type name too long: " + Prefix(nameIndex));
                }
                argType = MessagePatternArgType.Simple;
                if (length == 6)
                {
                    // case-insensitive comparisons for complex-type names
                    if (IsChoice(typeIndex))
                    {
                        argType = MessagePatternArgType.Choice;
                    }
                    else if (IsPlural(typeIndex))
                    {
                        argType = MessagePatternArgType.Plural;
                    }
                    else if (IsSelect(typeIndex))
                    {
                        argType = MessagePatternArgType.Select;
                    }
                }
                else if (length == 13)
                {
                    if (IsSelect(typeIndex) && IsOrdinal(typeIndex + 6))
                    {
                        argType = MessagePatternArgType.SelectOrdinal;
                    }
                }
                // change the ARG_START type from NONE to argType
                parts[argStart].Value = (short)argType;
                if (argType == MessagePatternArgType.Simple)
                {
                    AddPart(MessagePatternPartType.ArgType, typeIndex, length, 0);
                }
                // look for an argument style (pattern)
                if (c == '}')
                {
                    if (argType != MessagePatternArgType.Simple)
                    {
                        throw new ArgumentException(
                            "No style field for complex argument: " + Prefix(nameIndex));
                    }
                }
                else /* ',' */
                {
                    ++index;
                    if (argType == MessagePatternArgType.Simple)
                    {
                        index = ParseSimpleStyle(index);
                    }
                    else if (argType == MessagePatternArgType.Choice)
                    {
                        index = ParseChoiceStyle(index, nestingLevel);
                    }
                    else
                    {
                        index = ParsePluralOrSelectStyle(argType, index, nestingLevel);
                    }
                }
            }
            // Argument parsing stopped on the '}'.
            AddLimitPart(argStart, MessagePatternPartType.ArgLimit, index, 1, (int)argType);
            return index + 1;
        }

        private int ParseSimpleStyle(int index)
        {
            int start = index;
            int nestedBraces = 0;
            while (index < msg.Length)
            {
                char c = msg[index++];
                if (c == '\'')
                {
                    // Treat apostrophe as quoting but include it in the style part.
                    // Find the end of the quoted literal text.
                    index = msg.IndexOf('\'', index);
                    if (index < 0)
                    {
                        throw new ArgumentException(
                            "Quoted literal argument style text reaches to the end of the message: " +
                            Prefix(start));
                    }
                    // skip the quote-ending apostrophe
                    ++index;
                }
                else if (c == '{')
                {
                    ++nestedBraces;
                }
                else if (c == '}')
                {
                    if (nestedBraces > 0)
                    {
                        --nestedBraces;
                    }
                    else
                    {
                        int length = --index - start;
                        if (length > MessagePatternPart.MAX_LENGTH)
                        {
                            throw new IndexOutOfRangeException(
                                "Argument style text too long: " + Prefix(start));
                        }
                        AddPart(MessagePatternPartType.ArgStyle, start, length, 0);
                        return index;
                    }
                }  // c is part of literal text
            }
            throw new ArgumentException(
                "Unmatched '{' braces in message " + Prefix());
        }

        private int ParseChoiceStyle(int index, int nestingLevel)
        {
            int start = index;
            index = SkipWhiteSpace(index);
            if (index == msg.Length || msg[index] == '}')
            {
                throw new ArgumentException(
                    "Missing choice argument pattern in " + Prefix());
            }
            for (; ; )
            {
                // The choice argument style contains |-separated (number, separator, message) triples.
                // Parse the number.
                int numberIndex = index;
                index = SkipDouble(index);
                int length = index - numberIndex;
                if (length == 0)
                {
                    throw new ArgumentException("Bad choice pattern syntax: " + Prefix(start));
                }
                if (length > MessagePatternPart.MAX_LENGTH)
                {
                    throw new IndexOutOfRangeException(
                        "Choice number too long: " + Prefix(numberIndex));
                }
                ParseDouble(numberIndex, index, true);  // adds ARG_INT or ARG_DOUBLE
                                                        // Parse the separator.
                index = SkipWhiteSpace(index);
                if (index == msg.Length)
                {
                    throw new ArgumentException("Bad choice pattern syntax: " + Prefix(start));
                }
                char c = msg[index];
                if (!(c == '#' || c == '<' || c == '\u2264'))
                {  // U+2264 is <=
                    throw new ArgumentException(
                        "Expected choice separator (#<\u2264) instead of '" + c +
                        "' in choice pattern " + Prefix(start));
                }
                AddPart(MessagePatternPartType.ArgSelector, index, 1, 0);
                // Parse the message fragment.
                index = ParseMessage(++index, 0, nestingLevel + 1, MessagePatternArgType.Choice);
                // parseMessage(..., CHOICE) returns the index of the terminator, or msg.length().
                if (index == msg.Length)
                {
                    return index;
                }
                if (msg[index] == '}')
                {
                    if (!inMessageFormatPattern(nestingLevel))
                    {
                        throw new ArgumentException(
                            "Bad choice pattern syntax: " + Prefix(start));
                    }
                    return index;
                }  // else the terminator is '|'
                index = SkipWhiteSpace(index + 1);
            }
        }

        private int ParsePluralOrSelectStyle(MessagePatternArgType argType, int index, int nestingLevel)
        {
            int start = index;
            bool isEmpty = true;
            bool hasOther = false;
            for (; ; )
            {
                // First, collect the selector looking for a small set of terminators.
                // It would be a little faster to consider the syntax of each possible
                // token right here, but that makes the code too complicated.
                index = SkipWhiteSpace(index);
                bool eos = index == msg.Length;
                if (eos || msg[index] == '}')
                {
                    if (eos == inMessageFormatPattern(nestingLevel))
                    {
                        throw new ArgumentException(
                            "Bad " +
                            argType.ToString().ToLowerInvariant() +
                            " pattern syntax: " + Prefix(start));
                    }
                    if (!hasOther)
                    {
                        throw new ArgumentException(
                            "Missing 'other' keyword in " +
                            argType.ToString().ToLowerInvariant() +
                            " pattern in " + Prefix());
                    }
                    return index;
                }
                int selectorIndex = index;
                if (argType.HasPluralStyle() && msg[selectorIndex] == '=')
                {
                    // explicit-value plural selector: =double
                    index = SkipDouble(index + 1);
                    int length = index - selectorIndex;
                    if (length == 1)
                    {
                        throw new ArgumentException(
                            "Bad " +
                            argType.ToString().ToLowerInvariant() +
                            " pattern syntax: " + Prefix(start));
                    }
                    if (length > MessagePatternPart.MAX_LENGTH)
                    {
                        throw new IndexOutOfRangeException(
                            "Argument selector too long: " + Prefix(selectorIndex));
                    }
                    AddPart(MessagePatternPartType.ArgSelector, selectorIndex, length, 0);
                    ParseDouble(selectorIndex + 1, index, false);  // adds ARG_INT or ARG_DOUBLE
                }
                else
                {
                    index = SkipIdentifier(index);
                    int length = index - selectorIndex;
                    if (length == 0)
                    {
                        throw new ArgumentException(
                            "Bad " +
                            argType.ToString().ToLowerInvariant() +
                            " pattern syntax: " + Prefix(start));
                    }
                    // Note: The ':' in "offset:" is just beyond the skipIdentifier() range.
                    if (argType.HasPluralStyle() && length == 6 && index < msg.Length &&
                        msg.RegionMatches(selectorIndex, "offset:", 0, 7)
                    )
                    {
                        // plural offset, not a selector
                        if (!isEmpty)
                        {
                            throw new ArgumentException(
                                "Plural argument 'offset:' (if present) must precede key-message pairs: " +
                                Prefix(start));
                        }
                        // allow whitespace between offset: and its value
                        int valueIndex = SkipWhiteSpace(index + 1);  // The ':' is at index.
                        index = SkipDouble(valueIndex);
                        if (index == valueIndex)
                        {
                            throw new ArgumentException(
                                "Missing value for plural 'offset:' " + Prefix(start));
                        }
                        if ((index - valueIndex) > MessagePatternPart.MAX_LENGTH)
                        {
                            throw new IndexOutOfRangeException(
                                "Plural offset value too long: " + Prefix(valueIndex));
                        }
                        ParseDouble(valueIndex, index, false);  // adds ARG_INT or ARG_DOUBLE
                        isEmpty = false;
                        continue;  // no message fragment after the offset
                    }
                    else
                    {
                        // normal selector word
                        if (length > MessagePatternPart.MAX_LENGTH)
                        {
                            throw new IndexOutOfRangeException(
                                "Argument selector too long: " + Prefix(selectorIndex));
                        }
                        AddPart(MessagePatternPartType.ArgSelector, selectorIndex, length, 0);
                        if (msg.RegionMatches(selectorIndex, "other", 0, length))
                        {
                            hasOther = true;
                        }
                    }
                }

                // parse the message fragment following the selector
                index = SkipWhiteSpace(index);
                if (index == msg.Length || msg[index] != '{')
                {
                    throw new ArgumentException(
                        "No message fragment after " +
                        argType.ToString().ToLowerInvariant() +
                        " selector: " + Prefix(selectorIndex));
                }
                index = ParseMessage(index, 1, nestingLevel + 1, argType);
                isEmpty = false;
            }
        }

        /// <summary>
        /// Validates and parses an argument name or argument number string.
        /// This internal method assumes that the input substring is a "pattern identifier".
        /// </summary>
        /// <param name="s"></param>
        /// <param name="start"></param>
        /// <param name="limit"></param>
        /// <returns>&gt;=0 if the name is a valid number,
        /// <see cref="ArgNameNotNumber"/> (-1) if it is a "pattern identifier" but not all ASCII digits,
        /// <see cref="ArgNameNotValid"/> (-2) if it is neither.</returns>
        /// <seealso cref="ValidateArgumentName(string)"/>
        private static int ParseArgNumber(string s, int start, int limit)
        {
            // If the identifier contains only ASCII digits, then it is an argument _number_
            // and must not have leading zeros (except "0" itself).
            // Otherwise it is an argument _name_.
            if (start >= limit)
            {
                return ArgNameNotValid;
            }
            int number;
            // Defer numeric errors until we know there are only digits.
            bool badNumber;
            char c = s[start++];
            if (c == '0')
            {
                if (start == limit)
                {
                    return 0;
                }
                else
                {
                    number = 0;
                    badNumber = true;  // leading zero
                }
            }
            else if ('1' <= c && c <= '9')
            {
                number = c - '0';
                badNumber = false;
            }
            else
            {
                return ArgNameNotNumber;
            }
            while (start < limit)
            {
                c = s[start++];
                if ('0' <= c && c <= '9')
                {
                    if (number >= int.MaxValue / 10)
                    {
                        badNumber = true;  // overflow
                    }
                    number = number * 10 + (c - '0');
                }
                else
                {
                    return ArgNameNotNumber;
                }
            }
            // There are only ASCII digits.
            if (badNumber)
            {
                return ArgNameNotValid;
            }
            else
            {
                return number;
            }
        }

        private int ParseArgNumber(int start, int limit)
        {
            return ParseArgNumber(msg, start, limit);
        }

        /// <summary>
        /// Parses a number from the specified message substring.
        /// </summary>
        /// <param name="start">start index into the message string</param>
        /// <param name="limit">limit index into the message string, must be start&lt;limit</param>
        /// <param name="allowInfinity">allowInfinity true if U+221E is allowed (for <see cref="ChoiceFormat"/>)</param>
        private void ParseDouble(int start, int limit, bool allowInfinity)
        {
            Debug.Assert(start < limit);
            // fake loop for easy exit and single throw statement
            for (; ; )
            {
                // fast path for small integers and infinity
                int value = 0;
                int isNegative = 0;  // not boolean so that we can easily add it to value
                int index = start;
                char c = msg[index++];
                if (c == '-')
                {
                    isNegative = 1;
                    if (index == limit)
                    {
                        break;  // no number
                    }
                    c = msg[index++];
                }
                else if (c == '+')
                {
                    if (index == limit)
                    {
                        break;  // no number
                    }
                    c = msg[index++];
                }
                if (c == 0x221e)
                {  // infinity
                    if (allowInfinity && index == limit)
                    {
                        AddArgDoublePart(
                            isNegative != 0 ? double.NegativeInfinity : double.PositiveInfinity,
                            start, limit - start);
                        return;
                    }
                    else
                    {
                        break;
                    }
                }
                // try to parse the number as a small integer but fall back to a double
                while ('0' <= c && c <= '9')
                {
                    value = value * 10 + (c - '0');
                    if (value > (MessagePatternPart.MAX_VALUE + isNegative))
                    {
                        break;  // not a small-enough integer
                    }
                    if (index == limit)
                    {
                        AddPart(MessagePatternPartType.ArgInt, start, limit - start, isNegative != 0 ? -value : value);
                        return;
                    }
                    c = msg[index++];
                }
                // Let Double.parseDouble() throw a NumberFormatException.
                //double numericValue = Double.parseDouble(msg.Substring(start, limit - start)); // ICU4N: Fixed 2nd substring arg
                double numericValue = 0;
                double.TryParse(msg.Substring(start, limit - start), NumberStyles.Float, CultureInfo.InvariantCulture, out numericValue);
                AddArgDoublePart(numericValue, start, limit - start);
                return;
            }
            throw new FormatException(
                "Bad syntax for numeric value: " + msg.Substring(start, limit));
        }

        /// <summary>
        /// Appends the s[start, limit[ substring to sb, but with only half of the apostrophes
        /// according to JDK pattern behavior.
        /// </summary>
        /// <internal/>
        internal static void AppendReducedApostrophes(string s, int start, int limit,
                                             StringBuilder sb)
        {
            int doubleApos = -1;
            for (; ; )
            {
                int i = s.IndexOf('\'', start);
                if (i < 0 || i >= limit)
                {
                    sb.Append(s, start, limit);
                    break;
                }
                if (i == doubleApos)
                {
                    // Double apostrophe at start-1 and start==i, append one.
                    sb.Append('\'');
                    ++start;
                    doubleApos = -1;
                }
                else
                {
                    // Append text between apostrophes and skip this one.
                    sb.Append(s, start, i);
                    doubleApos = start = i + 1;
                }
            }
        }

        private int SkipWhiteSpace(int index)
        {
            return PatternProps.SkipWhiteSpace(msg, index);
        }

        private int SkipIdentifier(int index)
        {
            return PatternProps.SkipIdentifier(msg, index);
        }

        /// <summary>
        /// Skips a sequence of characters that could occur in a double value.
        /// Does not fully parse or validate the value.
        /// </summary>
        private int SkipDouble(int index)
        {
            while (index < msg.Length)
            {
                char c = msg[index];
                // U+221E: Allow the infinity symbol, for ChoiceFormat patterns.
                if ((c < '0' && "+-.".IndexOf(c) < 0) || (c > '9' && c != 'e' && c != 'E' && c != 0x221e))
                {
                    break;
                }
                ++index;
            }
            return index;
        }

        private static bool IsArgTypeChar(int c)
        {
            return ('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z');
        }

        private bool IsChoice(int index)
        {
            char c;
            return
                ((c = msg[index++]) == 'c' || c == 'C') &&
                ((c = msg[index++]) == 'h' || c == 'H') &&
                ((c = msg[index++]) == 'o' || c == 'O') &&
                ((c = msg[index++]) == 'i' || c == 'I') &&
                ((c = msg[index++]) == 'c' || c == 'C') &&
                ((c = msg[index]) == 'e' || c == 'E');
        }

        private bool IsPlural(int index)
        {
            char c;
            return
                ((c = msg[index++]) == 'p' || c == 'P') &&
                ((c = msg[index++]) == 'l' || c == 'L') &&
                ((c = msg[index++]) == 'u' || c == 'U') &&
                ((c = msg[index++]) == 'r' || c == 'R') &&
                ((c = msg[index++]) == 'a' || c == 'A') &&
                ((c = msg[index]) == 'l' || c == 'L');
        }

        private bool IsSelect(int index)
        {
            char c;
            return
                ((c = msg[index++]) == 's' || c == 'S') &&
                ((c = msg[index++]) == 'e' || c == 'E') &&
                ((c = msg[index++]) == 'l' || c == 'L') &&
                ((c = msg[index++]) == 'e' || c == 'E') &&
                ((c = msg[index++]) == 'c' || c == 'C') &&
                ((c = msg[index]) == 't' || c == 'T');
        }

        private bool IsOrdinal(int index)
        {
            char c;
            return
                ((c = msg[index++]) == 'o' || c == 'O') &&
                ((c = msg[index++]) == 'r' || c == 'R') &&
                ((c = msg[index++]) == 'd' || c == 'D') &&
                ((c = msg[index++]) == 'i' || c == 'I') &&
                ((c = msg[index++]) == 'n' || c == 'N') &&
                ((c = msg[index++]) == 'a' || c == 'A') &&
                ((c = msg[index]) == 'l' || c == 'L');
        }

        /// <returns>true if we are inside a <see cref="MessageFormat"/> (sub-)pattern,
        /// as opposed to inside a top-level choice/plural/select pattern.</returns>
        private bool inMessageFormatPattern(int nestingLevel)
        {
            return nestingLevel > 0 || parts[0].Type == MessagePatternPartType.MsgStart;
        }

        /// <returns>true if we are in a MessageFormat sub-pattern
        /// of a top-level <see cref="ChoiceFormat"/> pattern.</returns>
        private bool InTopLevelChoiceMessage(int nestingLevel, MessagePatternArgType parentType)
        {
            return
                nestingLevel == 1 &&
                parentType == MessagePatternArgType.Choice &&
                parts[0].Type != MessagePatternPartType.MsgStart;
        }

        private void AddPart(MessagePatternPartType type, int index, int length, int value)
        {
            parts.Add(new MessagePatternPart(type, index, length, value));
        }

        private void AddLimitPart(int start, MessagePatternPartType type, int index, int length, int value)
        {
            parts[start].limitPartIndex = parts.Count;
            AddPart(type, index, length, value);
        }

        private void AddArgDoublePart(double numericValue, int start, int length)
        {
            int numericIndex;
            if (numericValues == null)
            {
                numericValues = new List<double>();
                numericIndex = 0;
            }
            else
            {
                numericIndex = numericValues.Count;
                if (numericIndex > MessagePatternPart.MAX_VALUE)
                {
                    throw new IndexOutOfRangeException("Too many numeric values");
                }
            }
            numericValues.Add(numericValue);
            AddPart(MessagePatternPartType.ArgDouble, start, length, numericIndex);
        }

        private static readonly int MaxPrefixLength = 24;

        /// <summary>
        /// Returns a prefix of s.Substring(start). Used for Exception messages.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="start">start index in <paramref name="s"/></param>
        /// <returns> s.Substring(start) or a prefix of that</returns>
        private static string Prefix(string s, int start)
        {
            StringBuilder prefix = new StringBuilder(MaxPrefixLength + 20);
            if (start == 0)
            {
                prefix.Append("\"");
            }
            else
            {
                prefix.Append("[at pattern index ").Append(start).Append("] \"");
            }
            int substringLength = s.Length - start;
            if (substringLength <= MaxPrefixLength)
            {
                prefix.Append(start == 0 ? s : s.Substring(start));
            }
            else
            {
                int limit = start + MaxPrefixLength - 4;
                if (char.IsHighSurrogate(s[limit - 1]))
                {
                    // remove lead surrogate from the end of the prefix
                    --limit;
                }
                prefix.Append(s, start, limit).Append(" ...");
            }
            return prefix.Append("\"").ToString();
        }

        private static string Prefix(string s)
        {
            return Prefix(s, 0);
        }

        private string Prefix(int start)
        {
            return Prefix(msg, start);
        }

        private string Prefix()
        {
            return Prefix(msg, 0);
        }

        private ApostropheMode aposMode;
        private string msg;
        private IList<MessagePatternPart> parts = new List<MessagePatternPart>();
        private IList<double> numericValues;
        private bool hasArgNames;
        private bool hasArgNumbers;
        private bool needsAutoQuoting;
        private volatile bool frozen;
        private static readonly ApostropheMode defaultAposMode;

        // ICU4N specific - moved argTypes to Part class

        static MessagePattern()
        {
            // ICU4N specific - changed the casing of the text to match that of the enum
            // (both here and in ICUConfig.resx)
            defaultAposMode = (ApostropheMode)Enum.Parse(typeof(ApostropheMode),
                ICU4N.Impl.ICUConfig.Get("MessagePattern_ApostropheMode", "DoubleOptional"));
        }
    }
}
