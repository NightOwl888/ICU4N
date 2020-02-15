using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// <see cref="RuleBasedTransliterator"/> is a transliterator
    /// that reads a set of rules in order to determine how to perform
    /// translations. Rule sets are stored in resource bundles indexed by
    /// name. Rules within a rule set are separated by semicolons (';').
    /// To include a literal semicolon, prefix it with a backslash ('\').
    /// Unicode <see cref="Globalization.UProperty.Pattern_White_Space"/> is ignored.
    /// If the first non-blank character on a line is '#',
    /// the entire line is ignored as a comment.
    /// 
    /// <para/>Each set of rules consists of two groups, one forward, and one
    /// reverse. This is a convention that is not enforced; rules for one
    /// direction may be omitted, with the result that translations in
    /// that direction will not modify the source text. In addition,
    /// bidirectional forward-reverse rules may be specified for
    /// symmetrical transformations.
    /// 
    /// <para/><b>Rule syntax</b>
    /// 
    /// <para/>Rule statements take one of the following forms:
    /// 
    /// <dl>
    ///     <dt><c>$alefmadda=\u0622;</c></dt>
    ///     <dd><strong>Variable definition.</strong> The name on the
    ///         left is assigned the text on the right. In this example,
    ///         after this statement, instances of the left hand name,
    ///         &quot;<c>$alefmadda</c>&quot;, will be replaced by
    ///         the Unicode character U+0622. Variable names must begin
    ///         with a letter and consist only of letters, digits, and
    ///         underscores. Case is significant. Duplicate names cause
    ///         an exception to be thrown, that is, variables cannot be
    ///         redefined. The right hand side may contain well-formed
    ///         text of any length, including no text at all (&quot;<c>$empty=;</c>&quot;).
    ///         The right hand side may contain embedded <see cref="UnicodeSet"/>
    ///         patterns, for example, &quot;<c>$softvowel=[eiyEIY]</c>&quot;.</dd>
    ///     <dd>&#160;</dd>
    ///     <dt><c>ai&gt;$alefmadda;</c></dt>
    ///     <dd><strong>Forward translation rule.</strong> This rule
    ///         states that the string on the left will be changed to the
    ///         string on the right when performing forward
    ///         transliteration.</dd>
    ///     <dt>&#160;</dt>
    ///     <dt><c>ai&lt;$alefmadda;</c></dt>
    ///     <dd><strong>Reverse translation rule.</strong> This rule
    ///         states that the string on the right will be changed to
    ///         the string on the left when performing reverse
    ///         transliteration.</dd>
    /// </dl>
    /// 
    /// <dl>
    ///     <dt><c>ai&lt;&gt;$alefmadda;</c></dt>
    ///     <dd><strong>Bidirectional translation rule.</strong> This
    ///         rule states that the string on the right will be changed
    ///         to the string on the left when performing forward
    ///         transliteration, and vice versa when performing reverse
    ///         transliteration.</dd>
    /// </dl>
    /// 
    /// <para/>Translation rules consist of a <em>match pattern</em> and an <em>output
    /// string</em>. The match pattern consists of literal characters,
    /// optionally preceded by context, and optionally followed by
    /// context. Context characters, like literal pattern characters,
    /// must be matched in the text being transliterated. However, unlike
    /// literal pattern characters, they are not replaced by the output
    /// text. For example, the pattern &quot;<c>abc{def}</c>&quot;
    /// indicates the characters &quot;<c>def</c>&quot; must be
    /// preceded by &quot;<c>abc</c>&quot; for a successful match.
    /// If there is a successful match, &quot;<c>def</c>&quot; will
    /// be replaced, but not &quot;<c>abc</c>&quot;. The final '<c>}</c>'
    /// is optional, so &quot;<c>abc{def</c>&quot; is equivalent to
    /// &quot;<c>abc{def}</c>&quot;. Another example is &quot;<c>{123}456</c>&quot;
    /// (or &quot;<c>123}456</c>&quot;) in which the literal
    /// pattern &quot;<c>123</c>&quot; must be followed by &quot;<c>456</c>&quot;.
    /// 
    /// <para/>The output string of a forward or reverse rule consists of
    /// characters to replace the literal pattern characters. If the
    /// output string contains the character '<c>|</c>', this is
    /// taken to indicate the location of the <em>cursor</em> after
    /// replacement. The cursor is the point in the text at which the
    /// next replacement, if any, will be applied. The cursor is usually
    /// placed within the replacement text; however, it can actually be
    /// placed into the precending or following context by using the
    /// special character '<c>@</c>'. Examples:
    /// 
    /// <para/><code>a {foo} z &gt; | @ bar; # foo -&gt; bar, move cursor
    /// before a<br/>
    /// {foo} xyz &gt; bar @@|; #&#160;foo -&gt; bar, cursor between
    /// y and z
    /// </code>
    /// 
    /// <para/><b>UnicodeSet</b>
    /// 
    /// <para/><see cref="UnicodeSet"/> patterns may appear anywhere that
    /// makes sense. They may appear in variable definitions.
    /// Contrariwise, <see cref="UnicodeSet"/> patterns may themselves
    /// contain variable references, such as &quot;<c>$a=[a-z];$not_a=[^$a]</c>&quot;,
    /// or &quot;<c>$range=a-z;$ll=[$range]</c>&quot;.
    /// 
    /// <para/><see cref="UnicodeSet"/> patterns may also be embedded directly
    /// into rule strings. Thus, the following two rules are equivalent:
    /// 
    /// <para/><code>$vowel=[aeiou]; $vowel&gt;'*'; # One way to do this<br/>
    /// [aeiou]&gt;'*';
    /// &#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;#
    /// Another way
    /// </code>
    /// 
    /// <para/>See <see cref="UnicodeSet"/> for more documentation and examples.
    /// 
    /// <para/><b>Segments</b>
    /// 
    /// <para/>Segments of the input string can be matched and copied to the
    /// output string. This makes certain sets of rules simpler and more
    /// general, and makes reordering possible. For example:
    /// 
    /// <para/><code>([a-z]) &gt; $1 $1;
    /// &#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;#
    /// double lowercase letters<br/>
    /// ([:Lu:]) ([:Ll:]) &gt; $2 $1; # reverse order of Lu-Ll pairs
    /// </code>
    /// 
    /// <para/>The segment of the input string to be copied is delimited by
    /// &quot;<c>(</c>&quot; and &quot;<c>)</c>&quot;. Up to
    /// nine segments may be defined. Segments may not overlap. In the
    /// output string, &quot;<c>$1</c>&quot; through &quot;<c>$9</c>&quot;
    /// represent the input string segments, in left-to-right order of
    /// definition.
    /// 
    /// <para/><b>Anchors</b>
    /// 
    /// <para/>Patterns can be anchored to the beginning or the end of the text. This is done with the
    /// special characters '<c>^</c>' and '<c>$</c>'. For example:
    /// 
    /// <para/><code>^ a&#160;&#160; &gt; 'BEG_A'; &#160;&#160;# match 'a' at start of text<br/>
    /// &#160; a&#160;&#160; &gt; 'A';&#160;&#160;&#160;&#160;&#160;&#160; # match other instances
    /// of 'a'<br/>
    /// &#160; z $ &gt; 'END_Z'; &#160;&#160;# match 'z' at end of text<br/>
    /// &#160; z&#160;&#160; &gt; 'Z';&#160;&#160;&#160;&#160;&#160;&#160; # match other instances
    /// of 'z'
    /// </code>
    /// 
    /// <para/>It is also possible to match the beginning or the end of the text using a <see cref="UnicodeSet"/>.
    /// This is done by including a virtual anchor character '<c>$</c>' at the end of the
    /// set pattern. Although this is usually the match chafacter for the end anchor, the set will
    /// match either the beginning or the end of the text, depending on its placement. For
    /// example:
    /// 
    /// <para/><code>$x = [a-z$]; &#160;&#160;# match 'a' through 'z' OR anchor<br/>
    ///   $x 1&#160;&#160;&#160; &gt; 2;&#160;&#160; # match '1' after a-z or at the start<br/>
    ///   &#160;&#160; 3 $x &gt; 4; &#160;&#160;# match '3' before a-z or at the end
    /// </code>
    /// 
    /// <para/><b>Example</b>
    /// 
    /// <para/>The following example rules illustrate many of the features of
    /// the rule language.
    /// 
    /// <list type="table">
    ///     <item>
    ///         <term>Rule 1.</term>
    ///         <term><c>abc{def}&gt;x|y</c></term>
    ///     </item>
    ///     <item>
    ///         <term>Rule 2.</term>
    ///         <term><c>xyz&gt;r</c></term>
    ///     </item>
    ///     <item>
    ///         <term>Rule 3.</term>
    ///         <term><c>yz&gt;q</c></term>
    ///     </item>
    /// </list>
    /// 
    /// <para/>Applying these rules to the string &quot;<c>adefabcdefz</c>&quot;
    /// yields the following results:
    /// 
    /// <list type="table">
    ///     <item>
    ///         <term><c>|adefabcdefz</c></term>
    ///         <term>Initial state, no rules match. Advance
    ///         cursor.</term>
    ///     </item>
    ///     <item>
    ///         <term><c>a|defabcdefz</c></term>
    ///         <term>Still no match. Rule 1 does not match
    ///         because the preceding context is not present.</term>
    ///     </item>
    ///     <item>
    ///         <term><c>ad|efabcdefz</c></term>
    ///         <term>Still no match. Keep advancing until
    ///         there is a match...</term>
    ///     </item>
    ///     <item>
    ///         <term><c>ade|fabcdefz</c></term>
    ///         <term>...</term>
    ///     </item>
    ///     <item>
    ///         <term><c>adef|abcdefz</c></term>
    ///         <term>...</term>
    ///     </item>
    ///     <item>
    ///         <term><c>adefa|bcdefz</c></term>
    ///         <term>...</term>
    ///     </item>
    ///     <item>
    ///         <term><c>adefab|cdefz</c></term>
    ///         <term>...</term>
    ///     </item>
    ///     <item>
    ///         <term><c>adefabc|defz</c></term>
    ///         <term>Rule 1 matches; replace &quot;<c>def</c>&quot;
    ///         with &quot;<c>xy</c>&quot; and back up the cursor
    ///         to before the '<c>y</c>'.</term>
    ///     </item>
    ///     <item>
    ///         <term><c>adefabcx|yz</c></term>
    ///         <term>Although &quot;<c>xyz</c>&quot; is
    ///         present, rule 2 does not match because the cursor is
    ///         before the '<c>y</c>', not before the '<c>x</c>'.
    ///         Rule 3 does match. Replace &quot;<c>yz</c>&quot;
    ///         with &quot;<c>q</c>&quot;.</term>
    ///     </item>
    ///     <item>
    ///         <term><c>adefabcxq|</c></term>
    ///         <term>The cursor is at the end;
    ///         transliteration is complete.</term>
    ///     </item>
    /// </list>
    /// 
    /// <para/>The order of rules is significant. If multiple rules may match
    /// at some point, the first matching rule is applied.
    /// 
    /// <para/>Forward and reverse rules may have an empty output string.
    /// Otherwise, an empty left or right hand side of any statement is a
    /// syntax error.
    /// 
    /// <para/>Single quotes are used to quote any character other than a
    /// digit or letter. To specify a single quote itself, inside or
    /// outside of quotes, use two single quotes in a row. For example,
    /// the rule &quot;<c>'&gt;'&gt;o''clock</c>&quot; changes the
    /// string &quot;<c>&gt;</c>&quot; to the string &quot;<c>o'clock</c>&quot;.
    /// 
    /// <para/><b>Notes</b>
    /// 
    /// <para/>While a <see cref="RuleBasedTransliterator"/> is being built, it checks that
    /// the rules are added in proper order. For example, if the rule
    /// &quot;a&gt;x&quot; is followed by the rule &quot;ab&gt;y&quot;,
    /// then the second rule will throw an exception. The reason is that
    /// the second rule can never be triggered, since the first rule
    /// always matches anything it matches. In other words, the first
    /// rule <em>masks</em> the second rule.
    /// </summary>
    /// <author>Alan Liu</author>
    /// <internal/>
    [Obsolete("This API is ICU internal only.")]
    public class RuleBasedTransliterator : Transliterator // ICU4N NOTE: This needs to be public to support Lucene.NET's ICUTransformFilter
    {
        private readonly Data data;

        //    /**
        //     * Constructs a new transliterator from the given rules.
        //     * @param rules rules, separated by ';'
        //     * @param direction either FORWARD or REVERSE.
        //     * @exception IllegalArgumentException if rules are malformed
        //     * or direction is invalid.
        //     */
        //     public RuleBasedTransliterator(String ID, String rules, int direction,
        //                                   UnicodeFilter filter) {
        //        super(ID, filter);
        //        if (direction != FORWARD && direction != REVERSE) {
        //            throw new IllegalArgumentException("Invalid direction");
        //        }
        //
        //        TransliteratorParser parser = new TransliteratorParser();
        //        parser.parse(rules, direction);
        //        if (parser.idBlockVector.size() != 0 ||
        //            parser.compoundFilter != null) {
        //            throw new IllegalArgumentException("::ID blocks illegal in RuleBasedTransliterator constructor");
        //        }
        //
        //        data = (Data)parser.dataVector.get(0);
        //        setMaximumContextLength(data.ruleSet.getMaximumContextLength());
        //     }

        //    /**
        //     * Constructs a new transliterator from the given rules in the
        //     * <code>FORWARD</code> direction.
        //     * @param rules rules, separated by ';'
        //     * @exception IllegalArgumentException if rules are malformed
        //     * or direction is invalid.
        //     */
        //    public RuleBasedTransliterator(String ID, String rules) {
        //        this(ID, rules, FORWARD, null);
        //    }

        internal RuleBasedTransliterator(string ID, Data data, UnicodeFilter filter)
                 : base(ID, filter)
        {
            this.data = data;
            MaximumContextLength = data.RuleSet.MaximumContextLength;
        }

        /// <summary>
        /// Implements <see cref="Transliterator.HandleTransliterate(IReplaceable, TransliterationPosition, bool)"/>
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#pragma warning disable 809
        protected override void HandleTransliterate(IReplaceable text,
                                       TransliterationPosition index, bool incremental)
#pragma warning disable 809
        {
            /* We keep start and limit fixed the entire time,
             * relative to the text -- limit may move numerically if text is
             * inserted or removed.  The cursor moves from start to limit, with
             * replacements happening under it.
             *
             * Example: rules 1. ab>x|y
             *                2. yc>z
             *
             * |eabcd   start - no match, advance cursor
             * e|abcd   match rule 1 - change text & adjust cursor
             * ex|ycd   match rule 2 - change text & adjust cursor
             * exz|d    no match, advance cursor
             * exzd|    done
             */

            /* A rule like
             *   a>b|a
             * creates an infinite loop. To prevent that, we put an arbitrary
             * limit on the number of iterations that we take, one that is
             * high enough that any reasonable rules are ok, but low enough to
             * prevent a server from hanging.  The limit is 16 times the
             * number of characters n, unless n is so large that 16n exceeds a
             * uint32_t.
             */
            lock (data)
            {
                int loopCount = 0;
                int loopLimit = (index.Limit - index.Start) << 4;
                if (loopLimit < 0)
                {
                    loopLimit = 0x7FFFFFFF;
                }

                while (index.Start < index.Limit &&
                        loopCount <= loopLimit &&
                        data.RuleSet.Transliterate(text, index, incremental))
                {
                    ++loopCount;
                }
            }
        }


        internal class Data
        {
            public Data()
            {
                variableNames = new Dictionary<string, char[]>();
                RuleSet = new TransliterationRuleSet();
            }

            /// <summary>
            /// Rule table.  May be empty.
            /// </summary>
            public TransliterationRuleSet RuleSet { get; set; }

            /// <summary>
            /// Map variable name (String) to variable (char[]).  A variable name
            /// corresponds to zero or more characters, stored in a char[] array in
            /// this hash.  One or more of these chars may also correspond to a
            /// <see cref="UnicodeSet"/>, in which case the character in the char[] in this hash is
            /// a stand-in: it is an index for a secondary lookup in
            /// data.variables.  The stand-in also represents the <see cref="UnicodeSet"/> in
            /// the stored rules.
            /// </summary>
            internal IDictionary<string, char[]> variableNames;

            /// <summary>
            /// Map category variable (Character) to <see cref="IUnicodeMatcher"/> or <see cref="IUnicodeReplacer"/>.
            /// Variables that correspond to a set of characters are mapped
            /// from variable name to a stand-in character in <see cref="variableNames"/>.
            /// The stand-in then serves as a key in this hash to lookup the
            /// actual <see cref="UnicodeSet"/> object.  In addition, the stand-in is
            /// stored in the rule text to represent the set of characters.
            /// variables[i] represents character (variablesBase + i).
            /// </summary>
            internal object[] variables;

            /// <summary>
            /// The character that represents variables[0].  Characters
            /// variablesBase through variablesBase +
            /// variables.length - 1 represent <see cref="UnicodeSet"/> objects.
            /// </summary>
            internal char variablesBase;

            /// <summary>
            /// Return the <see cref="IUnicodeMatcher"/> represented by the given character, or
            /// null if none.
            /// </summary>
            public IUnicodeMatcher LookupMatcher(int standIn)
            {
                int i = standIn - variablesBase;
                return (i >= 0 && i < variables.Length)
                    ? (IUnicodeMatcher)variables[i] : null;
            }

            /// <summary>
            /// Return the <see cref="IUnicodeReplacer"/> represented by the given character, or
            /// null if none.
            /// </summary>
            public IUnicodeReplacer LookupReplacer(int standIn)
            {
                int i = standIn - variablesBase;
                return (i >= 0 && i < variables.Length)
                    ? (IUnicodeReplacer)variables[i] : null;
            }
        }

        /// <summary>
        /// Return a representation of this transliterator as source rules.
        /// These rules will produce an equivalent transliterator if used
        /// to construct a new transliterator.
        /// </summary>
        /// <param name="escapeUnprintable">if TRUE then convert unprintable
        /// character to their hex escape representations, \\uxxxx or
        /// \\Uxxxxxxxx.  Unprintable characters are those other than
        /// U+000A, U+0020..U+007E.</param>
        /// <returns>Rules string.</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public override string ToRules(bool escapeUnprintable)
        {
            return data.RuleSet.ToRules(escapeUnprintable);
        }

        //    /**
        //     * Return the set of all characters that may be modified by this
        //     * Transliterator, ignoring the effect of our filter.
        //     */
        //    protected UnicodeSet handleGetSourceSet() {
        //        return data.ruleSet.getSourceTargetSet(false, unicodeFilter);
        //    }
        //
        //    /**
        //     * Returns the set of all characters that may be generated as
        //     * replacement text by this transliterator.
        //     */
        //    public UnicodeSet getTargetSet() {
        //        return data.ruleSet.getSourceTargetSet(true, unicodeFilter);
        //    }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public override void AddSourceTargetSet(UnicodeSet filter, UnicodeSet sourceSet, UnicodeSet targetSet)
        {
            data.RuleSet.AddSourceTargetSet(filter, sourceSet, targetSet);
        }

        /// <summary>
        /// Temporary hack for registry problem. Needs to be replaced by better architecture.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public Transliterator SafeClone()
        {
            UnicodeFilter filter = Filter;
            if (filter != null && filter is UnicodeSet)
            {
                filter = new UnicodeSet((UnicodeSet)filter);
            }
            return new RuleBasedTransliterator(ID, data, filter);
        }
    }
}
