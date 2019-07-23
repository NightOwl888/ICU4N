using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    [Obsolete("This API is ICU internal only.")]
    public class RuleBasedTransliterator : Transliterator
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

        /**
         * Implements {@link Transliterator#handleTransliterate}.
         * @internal
         * @deprecated This API is ICU internal only.
         */
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

            /**
             * Rule table.  May be empty.
             */
            public TransliterationRuleSet RuleSet { get; set; }

            /**
             * Map variable name (String) to variable (char[]).  A variable name
             * corresponds to zero or more characters, stored in a char[] array in
             * this hash.  One or more of these chars may also correspond to a
             * UnicodeSet, in which case the character in the char[] in this hash is
             * a stand-in: it is an index for a secondary lookup in
             * data.variables.  The stand-in also represents the UnicodeSet in
             * the stored rules.
             */
            internal IDictionary<string, char[]> variableNames;

            /**
             * Map category variable (Character) to UnicodeMatcher or UnicodeReplacer.
             * Variables that correspond to a set of characters are mapped
             * from variable name to a stand-in character in data.variableNames.
             * The stand-in then serves as a key in this hash to lookup the
             * actual UnicodeSet object.  In addition, the stand-in is
             * stored in the rule text to represent the set of characters.
             * variables[i] represents character (variablesBase + i).
             */
            internal object[] variables;

            /**
             * The character that represents variables[0].  Characters
             * variablesBase through variablesBase +
             * variables.length - 1 represent UnicodeSet objects.
             */
            internal char variablesBase;

            /**
             * Return the UnicodeMatcher represented by the given character, or
             * null if none.
             */
            public IUnicodeMatcher LookupMatcher(int standIn)
            {
                int i = standIn - variablesBase;
                return (i >= 0 && i < variables.Length)
                    ? (IUnicodeMatcher)variables[i] : null;
            }

            /**
             * Return the UnicodeReplacer represented by the given character, or
             * null if none.
             */
            public IUnicodeReplacer LookupReplacer(int standIn)
            {
                int i = standIn - variablesBase;
                return (i >= 0 && i < variables.Length)
                    ? (IUnicodeReplacer)variables[i] : null;
            }
        }


        /**
         * Return a representation of this transliterator as source rules.
         * These rules will produce an equivalent transliterator if used
         * to construct a new transliterator.
         * @param escapeUnprintable if TRUE then convert unprintable
         * character to their hex escape representations, \\uxxxx or
         * \\Uxxxxxxxx.  Unprintable characters are those other than
         * U+000A, U+0020..U+007E.
         * @return rules string
         * @internal
         * @deprecated This API is ICU internal only.
         */
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

        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public override void AddSourceTargetSet(UnicodeSet filter, UnicodeSet sourceSet, UnicodeSet targetSet)
        {
            data.RuleSet.AddSourceTargetSet(filter, sourceSet, targetSet);
        }

        /**
         * Temporary hack for registry problem. Needs to be replaced by better architecture.
         * @internal
         * @deprecated This API is ICU internal only.
         */
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
