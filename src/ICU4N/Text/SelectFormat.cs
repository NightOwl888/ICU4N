using ICU4N.Impl;
using ICU4N.Support.Text;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    /// <summary>
    /// <see cref="SelectFormat"/> supports the creation of internationalized
    /// messages by selecting phrases based on keywords. The pattern specifies
    /// how to map keywords to phrases and provides a default phrase. The
    /// object provided to the format method is a string that's matched
    /// against the keywords. If there is a match, the corresponding phrase
    /// is selected; otherwise, the default phrase is used.
    /// </summary>
    /// <remarks>
    /// <h3>Using <see cref="SelectFormat"/> for Gender Agreement</h3>
    /// <para/>
    /// Note: Typically, select formatting is done via <see cref="MessageFormat"/>
    /// with a <c>select</c> argument type,
    /// rather than using a stand-alone <see cref="SelectFormat"/>.
    /// <para/>
    /// The main use case for the select format is gender based inflection.
    /// When names or nouns are inserted into sentences, their gender can affect pronouns,
    /// verb forms, articles, and adjectives. Special care needs to be
    /// taken for the case where the gender cannot be determined.
    /// The impact varies between languages:
    /// <list type="bullet">
    ///     <item>
    ///         <description>
    ///             English has three genders, and unknown gender is handled as a special
    ///             case. Names use the gender of the named person (if known), nouns referring
    ///             to people use natural gender, and inanimate objects are usually neutral.
    ///             The gender only affects pronouns: "he", "she", "it", "they".
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             German differs from English in that the gender of nouns is rather
    ///             arbitrary, even for nouns referring to people ("M&#xE4;dchen", girl, is  neutral).
    ///             The gender affects pronouns ("er", "sie", "es"), articles ("der", "die",
    ///             "das"), and adjective forms ("guter Mann", "gute Frau", "gutes  M&#xE4;dchen").
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             French has only two genders; as in German the gender of nouns
    ///             is rather arbitrary - for sun and moon, the genders
    ///             are the opposite of those in German. The gender affects
    ///             pronouns ("il", "elle"), articles ("le", "la"),
    ///             adjective forms ("bon", "bonne"), and sometimes
    ///             verb forms ("all&#xE9;", "all&#xE9;e").
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             Polish distinguishes five genders (or noun classes),
    ///             human masculine, animate non-human masculine, inanimate masculine,
    ///             feminine, and neuter.
    ///         </description>
    ///     </item>
    /// </list>
    /// <para/>
    /// Some other languages have noun classes that are not related to gender,
    /// but similar in grammatical use.
    /// Some African languages have around 20 noun classes.
    /// <para/>
    /// <b>Note:</b>For the gender of a <i>person</i> in a given sentence,
    /// we usually need to distinguish only between female, male and other/unknown.
    /// <para/>
    /// To enable localizers to create sentence patterns that take their
    /// language's gender dependencies into consideration, software has to provide
    /// information about the gender associated with a noun or name to
    /// <see cref="MessageFormat"/>.
    /// Two main cases can be distinguished:
    /// <list type="bullet">
    ///     <item>
    ///         <description>
    ///             For people, natural gender information should be maintained for each person.
    ///             Keywords like "male", "female", "mixed" (for groups of people)
    ///             and "unknown" could be used.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             For nouns, grammatical gender information should be maintained for
    ///             each noun and per language, e.g., in resource bundles.
    ///             The keywords "masculine", "feminine", and "neuter" are commonly used,
    ///             but some languages may require other keywords.
    ///         </description>
    ///     </item>
    /// </list>
    /// <para/>
    /// The resulting keyword is provided to <see cref="MessageFormat"/> as a
    /// parameter separate from the name or noun it's associated with. For example,
    /// to generate a message such as "Jean went to Paris", three separate arguments
    /// would be provided: The name of the person as argument 0, the gender of
    /// the person as argument 1, and the name of the city as argument 2.
    /// The sentence pattern for English, where the gender of the person has
    /// no impact on this simple sentence, would not refer to argument 1 at all:
    /// <code>{0} went to {2}.</code>
    /// <para/>
    /// <b>Note:</b> The entire sentence should be included (and partially repeated)
    /// inside each phrase. Otherwise translators would have to be trained on how to
    /// move bits of the sentence in and out of the select argument of a message
    /// (The examples below do not follow this recommendation!)
    /// <para/>
    /// The sentence pattern for French, where the gender of the person affects
    /// the form of the participle, uses a select format based on argument 1:
    /// <code>{0} est {1, select, female {all&#xE9;e} other {all&#xE9;}} &#xE0; {2}.</code>
    /// <para/>
    /// Patterns can be nested, so that it's possible to handle interactions of
    /// number and gender where necessary. For example, if the above sentence should
    /// allow for the names of several people to be inserted, the following sentence
    /// pattern can be used (with argument 0 the list of people's names,
    /// argument 1 the number of people, argument 2 their combined gender, and
    /// argument 3 the city name):
    /// <code>
    /// {0} {1, plural, 
    /// one {est {2, select, female {all&#xE9;e} other {all&#xE9;}}}
    /// other {sont {2, select, female {all&#xE9;es} other {all&#xE9;s}}}
    /// }&#xE0; {3}.
    /// </code>
    /// <h4>Patterns and Their Interpretation</h4>
    /// <para/>
    /// The <see cref="SelectFormat"/> pattern string defines the phrase output
    /// for each user-defined keyword.
    /// The pattern is a sequence of (keyword, message) pairs.
    /// A keyword is a "pattern identifier": <c>[^[[:Pattern_Syntax:][:Pattern_White_Space:]]]+</c>
    /// <para/>
    /// Each message is a <see cref="MessageFormat"/> pattern string enclosed in {curly braces}.
    /// <para/>
    /// You always have to define a phrase for the default keyword
    /// <c>other</c>; this phrase is returned when the keyword
    /// provided to
    /// the <c>format</c> method matches no other keyword.
    /// If a pattern does not provide a phrase for <c>other</c>, the method
    /// it's provided to returns the error <c>U_DEFAULT_KEYWORD_MISSING</c>.
    /// <para/>
    /// Pattern_White_Space between keywords and messages is ignored.
    /// Pattern_White_Space within a message is preserved and output.
    /// <code>
    /// // Example:
    /// MessageFormat msgFmt = new MessageFormat("{0} est " +
    ///     "{1, select, female {all&#xE9;e} other {all&#xE9;}} &#xE0; Paris.",
    ///     new ULocale("fr"));
    /// object args[] = {"Kirti","female"};
    /// Console.WriteLine(msgFmt.Format(args));
    /// </code>
    /// <para/>
    /// Produces the output:
    /// <code>Kirti est all&#xE9;e &#xE0; Paris.</code>
    /// </remarks>
    /// <stable>ICU 4.4</stable>
    internal class SelectFormat : Formatter // ICU4N: Marked internal until implementation of formatters are completed
    {
        // Generated by serialver from JDK 1.5
        //private static readonly long serialVersionUID = 2993154333257524984L;

        /// <summary>
        /// The applied pattern string.
        /// </summary>
        private string pattern = null;

        /// <summary>
        /// The <see cref="MessagePattern"/> which contains the parsed structure of the pattern string.
        /// </summary>
#if FEATURE_SERIALIZABLE
        [NonSerialized]
#endif
        private MessagePattern msgPattern;

        /// <summary>
        /// Creates a new <see cref="SelectFormat"/> for a given pattern string.
        /// </summary>
        /// <param name="pattern">The pattern for this <see cref="SelectFormat"/>.</param>
        /// <stable>ICU 4.4</stable>
        public SelectFormat(string pattern)
        {
            ApplyPattern(pattern);
        }

        /// <summary>
        /// Resets the <see cref="SelectFormat"/> object.
        /// </summary>
        private void Reset()
        {
            pattern = null;
            if (msgPattern != null)
            {
                msgPattern.Clear();
            }
        }

        /// <summary>
        /// Sets the pattern used by this select format.
        /// Patterns and their interpretation are specified in the class description.
        /// </summary>
        /// <param name="pattern">The pattern for this select format.</param>
        /// <exception cref="ArgumentException">When the <paramref name="pattern"/> is not a valid select format pattern.</exception>
        /// <stable>ICU 4.4</stable>
        public virtual void ApplyPattern(string pattern)
        {
            this.pattern = pattern;
            if (msgPattern == null)
            {
                msgPattern = new MessagePattern();
            }
            try
            {
                msgPattern.ParseSelectStyle(pattern);
            }
            catch (Exception)
            {
                Reset();
                throw;
            }
        }

        /// <summary>
        /// Returns the pattern for this <see cref="SelectFormat"/>.
        /// </summary>
        /// <returns>The pattern string</returns>
        /// <stable>ICU 4.4</stable>
        public virtual string ToPattern()
        {
            return pattern;
        }

        /// <summary>
        /// Finds the <see cref="SelectFormat"/> sub-message for the given <paramref name="keyword"/>, or the "other" sub-message.
        /// </summary>
        /// <param name="pattern">A <see cref="MessagePattern"/>.</param>
        /// <param name="partIndex">The index of the first <see cref="SelectFormat"/> argument style part.</param>
        /// <param name="keyword">A keyword to be matched to one of the <see cref="SelectFormat"/> argument's keywords.</param>
        /// <returns>The sub-message start part index.</returns>
        internal static int FindSubMessage(MessagePattern pattern, int partIndex, string keyword)
        {
            int count = pattern.CountParts();
            int msgStart = 0;
            // Iterate over (ARG_SELECTOR, message) pairs until ARG_LIMIT or end of select-only pattern.
            do
            {
                MessagePatternPart part = pattern.GetPart(partIndex++);
                MessagePatternPartType type = part.Type;
                if (type == MessagePatternPartType.ArgLimit)
                {
                    break;
                }
                Debug.Assert(type == MessagePatternPartType.ArgSelector);
                // part is an ARG_SELECTOR followed by a message
                if (pattern.PartSubstringMatches(part, keyword))
                {
                    // keyword matches
                    return partIndex;
                }
                else if (msgStart == 0 && pattern.PartSubstringMatches(part, "other"))
                {
                    msgStart = partIndex;
                }
                partIndex = pattern.GetLimitPartIndex(partIndex);
            } while (++partIndex < count);
            return msgStart;
        }

        /// <summary>
        /// Selects the phrase for the given <paramref name="keyword"/>.
        /// </summary>
        /// <param name="keyword">A phrase selection keyword.</param>
        /// <returns>The string containing the formatted select message.</returns>
        /// <exception cref="ArgumentException">When the given keyword is not a "pattern identifier".</exception>
        /// <stable>ICU 4.4</stable>
        public string Format(string keyword)
        {
            //Check for the validity of the keyword
            if (!PatternProps.IsIdentifier(keyword))
            {
                throw new ArgumentException("Invalid formatting argument.");
            }
            // If no pattern was applied, throw an exception
            if (msgPattern == null || msgPattern.CountParts() == 0)
            {
                throw new InvalidOperationException("Invalid format error.");
            }

            // Get the appropriate sub-message.
            int msgStart = FindSubMessage(msgPattern, 0, keyword);
            if (!msgPattern.JdkAposMode)
            {
                int msgLimit = msgPattern.GetLimitPartIndex(msgStart);
                return msgPattern.PatternString.Substring(msgPattern.GetPart(msgStart).Limit,
                                                               msgPattern.GetPatternIndex(msgLimit));
            }
            // JDK compatibility mode: Remove SKIP_SYNTAX.
            StringBuilder result = null;
            int prevIndex = msgPattern.GetPart(msgStart).Limit;
            for (int i = msgStart; ;)
            {
                MessagePatternPart part = msgPattern.GetPart(++i);
                MessagePatternPartType type = part.Type;
                int index = part.Index;
                if (type == MessagePatternPartType.MsgLimit)
                {
                    if (result == null)
                    {
                        return pattern.Substring(prevIndex, index - prevIndex); // ICU4N: Corrected 2nd arg
                    }
                    else
                    {
                        return result.Append(pattern, prevIndex, index).ToString();
                    }
                }
                else if (type == MessagePatternPartType.SkipSyntax)
                {
                    if (result == null)
                    {
                        result = new StringBuilder();
                    }
                    result.Append(pattern, prevIndex, index);
                    prevIndex = part.Limit;
                }
                else if (type == MessagePatternPartType.ArgStart)
                {
                    if (result == null)
                    {
                        result = new StringBuilder();
                    }
                    result.Append(pattern, prevIndex, index);
                    prevIndex = index;
                    i = msgPattern.GetLimitPartIndex(i);
                    index = msgPattern.GetPart(i).Limit;
                    MessagePattern.AppendReducedApostrophes(pattern, prevIndex, index, result);
                    prevIndex = index;
                }
            }
        }

        /// <summary>
        /// Selects the phrase for the given <paramref name="keyword"/>.
        /// and appends the formatted message to the given <see cref="StringBuffer"/>.
        /// </summary>
        /// <param name="keyword">a phrase selection keyword.</param>
        /// <param name="toAppendTo">the selected phrase will be appended to this
        /// <see cref="StringBuffer"/>.</param>
        /// <param name="pos">will be ignored by this method.</param>
        /// <returns>the string buffer passed in as <paramref name="toAppendTo"/>, with formatted text appended.</returns>
        /// <exception cref="ArgumentException">when the given keyword is not a <see cref="string"/> or not a "pattern identifier".</exception>
        /// <stable>ICU 4.4</stable>
        public override StringBuffer Format(object keyword, StringBuffer toAppendTo,
                FieldPosition pos)
        {
            if (keyword is string)
            {
                toAppendTo.Append(Format((string)keyword));
            }
            else
            {
                throw new ArgumentException("'" + keyword + "' is not a String");
            }
            return toAppendTo;
        }

        /// 
        /// <summary>
        /// This method is not supported by <see cref="SelectFormat"/>.
        /// </summary>
        /// <param name="source">the string to be parsed.</param>
        /// <param name="pos">defines the position where parsing is to begin,
        /// and upon return, the position where parsing left off.  If the position
        /// has not changed upon return, then parsing failed.</param>
        /// <returns>nothing because this method is not supported.</returns>
        /// <exception cref="NotSupportedException">thrown always.</exception>
        /// <stable>ICU 4.4</stable>
        public override object ParseObject(string source, ParsePosition pos)
        {
            throw new NotSupportedException();
        }

        /// <stable>ICU 4.4</stable>
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            SelectFormat sf = (SelectFormat)obj;
            return msgPattern == null ? sf.msgPattern == null : msgPattern.Equals(sf.msgPattern);
        }

        /// <stable>ICU 4.4</stable>
        public override int GetHashCode()
        {
            if (pattern != null)
            {
                return pattern.GetHashCode();
            }
            return 0;
        }

        /// <stable>ICU 4.4</stable>
        public override string ToString()
        {
            return "pattern='" + pattern + "'";
        }

        private void ReadObject(Stream @in)
        {
            // ICU4N TODO: serialization
            //@in.defaultReadObject();
            if (pattern != null)
            {
                ApplyPattern(pattern);
            }
        }
    }
}
