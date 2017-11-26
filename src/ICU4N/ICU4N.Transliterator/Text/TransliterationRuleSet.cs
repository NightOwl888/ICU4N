using ICU4N.Impl;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// A set of rules for a <see cref="RuleBasedTransliterator"/>.  This set encodes
    /// the transliteration in one direction from one set of characters or short
    /// strings to another.  A <see cref="RuleBasedTransliterator"/> consists of up to
    /// two such sets, one for the forward direction, and one for the reverse.
    /// <para/>
    /// A <see cref="TransliterationRuleSet"/> has one important operation, that of
    /// finding a matching rule at a given point in the text.  This is accomplished
    /// by the <see cref="FindMatch()"/> method.
    /// <para/>
    /// Copyright &copy; IBM Corporation 1999.  All rights reserved.
    /// </summary>
    /// <author>Alan Liu</author>
    internal class TransliterationRuleSet
    {
        /// <summary>
        /// Vector of rules, in the order added.
        /// </summary>
        private List<TransliterationRule> ruleVector;

        /// <summary>
        /// Length of the longest preceding context
        /// </summary>
        private int maxContextLength;

        /// <summary>
        /// Sorted and indexed table of rules.  This is created by <see cref="Freeze()"/> from
        /// the rules in ruleVector.  rules.Length >= ruleVector.Count, and the
        /// references in rules[] are aliases of the references in ruleVector.
        /// A single rule in ruleVector is listed one or more times in rules[].
        /// </summary>
        private TransliterationRule[] rules;

        /// <summary>
        /// Index table.  For text having a first character c, compute x = c&0xFF.
        /// Now use rules[index[x]..index[x+1]-1].  This index table is created by
        /// <see cref="Freeze()"/>.
        /// </summary>
        private int[] index;

        /// <summary>
        /// Construct a new empty rule set.
        /// </summary>
        public TransliterationRuleSet()
        {
            ruleVector = new List<TransliterationRule>();
            maxContextLength = 0;
        }

        /// <summary>
        /// Return the maximum context length.
        /// </summary>
        /// <returns>the length of the longest preceding context.</returns>
        public virtual int MaximumContextLength
        {
            get { return maxContextLength; }
        }

        /// <summary>
        /// Add a rule to this set.  Rules are added in order, and order is
        /// significant.
        /// </summary>
        /// <param name="rule">The rule to add.</param>
        public virtual void AddRule(TransliterationRule rule)
        {
            ruleVector.Add(rule);
            int len;
            if ((len = rule.AnteContextLength) > maxContextLength)
            {
                maxContextLength = len;
            }

            rules = null;
        }

        /// <summary>
        /// Close this rule set to further additions, check it for masked rules,
        /// and index it to optimize performance.
        /// </summary>
        /// <exception cref="ArgumentException">If some rules are masked.</exception>
        public virtual void Freeze()
        {
            /* Construct the rule array and index table.  We reorder the
             * rules by sorting them into 256 bins.  Each bin contains all
             * rules matching the index value for that bin.  A rule
             * matches an index value if string whose first key character
             * has a low byte equal to the index value can match the rule.
             *
             * Each bin contains zero or more rules, in the same order
             * they were found originally.  However, the total rules in
             * the bins may exceed the number in the original vector,
             * since rules that have a variable as their first key
             * character will generally fall into more than one bin.
             *
             * That is, each bin contains all rules that either have that
             * first index value as their first key character, or have
             * a set containing the index value as their first character.
             */
            int n = ruleVector.Count;
            index = new int[257]; // [sic]
            List<TransliterationRule> v = new List<TransliterationRule>(2 * n); // heuristic; adjust as needed

            /* Precompute the index values.  This saves a LOT of time.
             */
            int[] indexValue = new int[n];
            for (int j = 0; j < n; ++j)
            {
                TransliterationRule r = ruleVector[j];
                indexValue[j] = r.GetIndexValue();
            }
            for (int x = 0; x < 256; ++x)
            {
                index[x] = v.Count;
                for (int j = 0; j < n; ++j)
                {
                    if (indexValue[j] >= 0)
                    {
                        if (indexValue[j] == x)
                        {
                            v.Add(ruleVector[j]);
                        }
                    }
                    else
                    {
                        // If the indexValue is < 0, then the first key character is
                        // a set, and we must use the more time-consuming
                        // matchesIndexValue check.  In practice this happens
                        // rarely, so we seldom tread this code path.
                        TransliterationRule r = ruleVector[j];
                        if (r.MatchesIndexValue(x))
                        {
                            v.Add(r);
                        }
                    }
                }
            }
            index[256] = v.Count;

            /* Freeze things into an array.
             */
            rules = new TransliterationRule[v.Count];
            v.CopyTo(rules);

            StringBuilder errors = null;

            /* Check for masking.  This is MUCH faster than our old check,
             * which was each rule against each following rule, since we
             * only have to check for masking within each bin now.  It's
             * 256*O(n2^2) instead of O(n1^2), where n1 is the total rule
             * count, and n2 is the per-bin rule count.  But n2<<n1, so
             * it's a big win.
             */
            for (int x = 0; x < 256; ++x)
            {
                for (int j = index[x]; j < index[x + 1] - 1; ++j)
                {
                    TransliterationRule r1 = rules[j];
                    for (int k = j + 1; k < index[x + 1]; ++k)
                    {
                        TransliterationRule r2 = rules[k];
                        if (r1.Masks(r2))
                        {
                            if (errors == null)
                            {
                                errors = new StringBuilder();
                            }
                            else
                            {
                                errors.Append("\n");
                            }
                            errors.Append("Rule " + r1 + " masks " + r2);
                        }
                    }
                }
            }

            if (errors != null)
            {
                throw new ArgumentException(errors.ToString());
            }
        }

        /// <summary>
        /// Transliterate the given text with the given UTransPosition
        /// indices.  Return TRUE if the transliteration should continue
        /// or FALSE if it should halt (because of a U_PARTIAL_MATCH match).
        /// Note that FALSE is only ever returned if isIncremental is TRUE.
        /// </summary>
        /// <param name="text">The text to be transliterated.</param>
        /// <param name="pos">The position indices, which will be updated.</param>
        /// <param name="incremental">If TRUE, assume new text may be inserted
        /// at index.Limit, and return FALSE if thre is a partial match.</param>
        /// <returns>TRUE unless a U_PARTIAL_MATCH has been obtained,
        /// indicating that transliteration should stop until more text
        /// arrives.</returns>
        public virtual bool Transliterate(IReplaceable text,
                                     Transliterator.Position pos,
                                     bool incremental)
        {
            int indexByte = text.Char32At(pos.Start) & 0xFF;
            for (int i = index[indexByte]; i < index[indexByte + 1]; ++i)
            {
                int m = rules[i].MatchAndReplace(text, pos, incremental);
                switch (m)
                {
                    case UnicodeMatcher.U_MATCH:
                        if (Transliterator.DEBUG)
                        {
                            Console.Out.WriteLine((incremental ? "Rule.i: match " : "Rule: match ") +
                                               rules[i].ToRule(true) + " => " +
                                               UtilityExtensions.FormatInput(text, pos));
                        }
                        return true;
                    case UnicodeMatcher.U_PARTIAL_MATCH:
                        if (Transliterator.DEBUG)
                        {
                            Console.Out.WriteLine((incremental ? "Rule.i: partial match " : "Rule: partial match ") +
                                               rules[i].ToRule(true) + " => " +
                                               UtilityExtensions.FormatInput(text, pos));
                        }
                        return false;
                    default:
                        if (Transliterator.DEBUG)
                        {
                            Console.Out.WriteLine("Rule: no match " + rules[i]);
                        }
                        break;
                }
            }
            // No match or partial match from any rule
            pos.Start += UTF16.GetCharCount(text.Char32At(pos.Start));
            if (Transliterator.DEBUG)
            {
                Console.Out.WriteLine((incremental ? "Rule.i: no match => " : "Rule: no match => ") +
                                   UtilityExtensions.FormatInput(text, pos));
            }
            return true;
        }

        /// <summary>
        /// Create rule strings that represents this rule set.
        /// </summary>
        internal virtual string ToRules(bool escapeUnprintable)
        {
            int i;
            int count = ruleVector.Count;
            StringBuilder ruleSource = new StringBuilder();
            for (i = 0; i < count; ++i)
            {
                if (i != 0)
                {
                    ruleSource.Append('\n');
                }
                TransliterationRule r = ruleVector[i];
                ruleSource.Append(r.ToRule(escapeUnprintable));
            }
            return ruleSource.ToString();
        }

        // TODO Handle the case where we have :: [a] ; a > |b ; b > c ;
        // TODO Merge into r.addSourceTargetSet, to avoid duplicate testing
        internal virtual void AddSourceTargetSet(UnicodeSet filter, UnicodeSet sourceSet, UnicodeSet targetSet)
        {
            UnicodeSet currentFilter = new UnicodeSet(filter);
            UnicodeSet revisiting = new UnicodeSet();
            int count = ruleVector.Count;
            for (int i = 0; i < count; ++i)
            {
                TransliterationRule r = ruleVector[i];
                r.AddSourceTargetSet(currentFilter, sourceSet, targetSet, revisiting.Clear());
                currentFilter.AddAll(revisiting);
            }
        }
    }
}
