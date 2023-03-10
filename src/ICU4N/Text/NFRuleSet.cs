using ICU4N.Impl;
using J2N.Numerics;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// A collection of rules used by a <see cref="RuleBasedNumberFormat"/> to Format and
    /// parse numbers. It is the responsibility of a <see cref="NFRuleSet"/> to select an
    /// appropriate rule for formatting a particular number and dispatch
    /// control to it, and to arbitrate between different rules when parsing
    /// a number.
    /// </summary>
    internal sealed class NFRuleSet
    {
        //-----------------------------------------------------------------------
        // data members
        //-----------------------------------------------------------------------

        /**
         * The rule set's name
         */
        private readonly string name;

        /**
         * The rule set's regular rules
         */
        private NFRule[] rules;

        /**
         * The rule set's non-numerical rules like negative, fractions, infinity and NaN
         */
        internal readonly NFRule[] nonNumericalRules = new NFRule[6];

        /**
         * These are a pile of fraction rules in declared order. They may have alternate
         * ways to represent fractions.
         */
        private LinkedList<NFRule> fractionRules;

        /** -x */
        private const int NegativeRuleIndex = 0;
        /** x.x */
        private const int ImproperFractionRuleIndex = 1;
        /** 0.x */
        private const int ProperFractionRuleIndex = 2;
        /** x.0 */
        private const int MasterRuleIndex = 3;
        /** Inf */
        private const int InfinityRuleIndex = 4;
        /** NaN */
        private const int NaNRuleIndex = 5;

        /**
         * The RuleBasedNumberFormat that owns this rule
         */
        internal readonly RuleBasedNumberFormat owner;

        /**
         * True if the rule set is a fraction rule set.  A fraction rule set
         * is a rule set that is used to Format the fractional part of a
         * number.  It is called from a >> substitution in another rule set's
         * fraction rule, and is only called upon to Format values between
         * 0 and 1.  A fraction rule set has different rule-selection
         * behavior than a regular rule set.
         */
        private bool isFractionRuleSet = false;

        /**
         * True if the rule set is parseable.
         */
        private readonly bool isParseable;

        /**
         * Limit of recursion. It's about a 64 bit number formatted in base 2.
         */
        private static readonly int RecursionLimit = 64;

        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        /**
         * Constructs a rule set.
         * @param owner The formatter that owns this rule set
         * @param descriptions An array of Strings representing rule set
         * descriptions.  On exit, this rule set's entry in the array will
         * have been stripped of its rule set name and any trailing whitespace.
         * @param index The index into "descriptions" of the description
         * for the rule to be constructed
         */
        public NFRuleSet(RuleBasedNumberFormat owner, string[] descriptions, int index)
        {
            this.owner = owner;
            string description = descriptions[index];

            if (description.Length == 0)
            {
                throw new ArgumentException("Empty rule set description");
            }

            // if the description begins with a rule set name (the rule set
            // name can be omitted in formatter descriptions that consist
            // of only one rule set), copy it out into our "name" member
            // and delete it from the description
            if (description[0] == '%')
            {
                int pos = description.IndexOf(':');
                if (pos == -1)
                {
                    throw new ArgumentException("Rule set name doesn't end in colon");
                }
                else
                {
                    string name = description.Substring(0, pos); // ICU4N: Checked 2nd parameter
                    this.isParseable = !name.EndsWith("@noparse", StringComparison.Ordinal);
                    if (!this.isParseable)
                    {
                        name = name.Substring(0, name.Length - 8); // Remove the @noparse from the name
                    }
                    this.name = name;

                    //noinspection StatementWithEmptyBody
                    while (pos < description.Length && PatternProps.IsWhiteSpace(description[++pos]))
                    {
                        // ICU4N: Intentionally empty
                    }
                    description = description.Substring(pos);
                    descriptions[index] = description;
                }
            }
            else
            {
                // if the description doesn't begin with a rule set name, its
                // name is "%default"
                name = "%default";
                isParseable = true;
            }

            if (description.Length == 0)
            {
                throw new ArgumentException("Empty rule set description");
            }

            // all of the other members of NFRuleSet are initialized
            // by parseRules()
        }

        /**
         * Construct the subordinate data structures used by this object.
         * This function is called by the RuleBasedNumberFormat constructor
         * after all the rule sets have been created to actually parse
         * the description and build rules from it.  Since any rule set
         * can refer to any other rule set, we have to have created all of
         * them before we can create anything else.
         * @param description The textual description of this rule set
         */
        public void ParseRules(string description)
        {
            // (the number of elements in the description list isn't necessarily
            // the number of rules-- some descriptions may expend into two rules)
            List<NFRule> tempRules = new List<NFRule>();

            // we keep track of the rule before the one we're currently working
            // on solely to support >>> substitutions
            NFRule predecessor = null;

            // Iterate through the rules.  The rules
            // are separated by semicolons (there's no escape facility: ALL
            // semicolons are rule delimiters)
            int oldP = 0;
            int descriptionLen = description.Length;
            int p;
            do
            {
                p = description.IndexOf(';', oldP);
                if (p < 0)
                {
                    p = descriptionLen;
                }

                // makeRules (a factory method on NFRule) will return either
                // a single rule or an array of rules.  Either way, add them
                // to our rule vector
                NFRule.MakeRules(description.Substring(oldP, p - oldP), // ICU4N: Corrected 2nd parameter
                        this, predecessor, owner, tempRules);
                if (tempRules.Count != 0)
                {
                    predecessor = tempRules[tempRules.Count - 1];
                }

                oldP = p + 1;
            }
            while (oldP < descriptionLen);

            // for rules that didn't specify a base value, their base values
            // were initialized to 0.  Make another pass through the list and
            // set all those rules' base values.  We also remove any special
            // rules from the list and put them into their own member variables
            long defaultBaseValue = 0;

            foreach (NFRule rule in tempRules)
            {
                long baseValue = rule.BaseValue;
                if (baseValue == 0)
                {
                    // if the rule's base value is 0, fill in a default
                    // base value (this will be 1 plus the preceding
                    // rule's base value for regular rule sets, and the
                    // same as the preceding rule's base value in fraction
                    // rule sets)
                    rule.SetBaseValue(defaultBaseValue);
                }
                else
                {
                    // if it's a regular rule that already knows its base value,
                    // check to make sure the rules are in order, and update
                    // the default base value for the next rule
                    if (baseValue < defaultBaseValue)
                    {
                        throw new ArgumentException("Rules are not in order, base: " +
                                baseValue + " < " + defaultBaseValue);
                    }
                    defaultBaseValue = baseValue;
                }
                if (!isFractionRuleSet)
                {
                    ++defaultBaseValue;
                }
            }

            // finally, we can copy the rules from the vector into a
            // fixed-length array
            rules = new NFRule[tempRules.Count];
            tempRules.CopyTo(rules);
        }

        /**
         * Set one of the non-numerical rules.
         * @param rule The rule to set.
         */
        internal void SetNonNumericalRule(NFRule rule)
        {
            long baseValue = rule.BaseValue;
            if (baseValue == NFRule.NegativeNumberRule)
            {
                nonNumericalRules[NFRuleSet.NegativeRuleIndex] = rule;
            }
            else if (baseValue == NFRule.ImproperFractionRule)
            {
                SetBestFractionRule(NFRuleSet.ImproperFractionRuleIndex, rule, true);
            }
            else if (baseValue == NFRule.ProperFractionRule)
            {
                SetBestFractionRule(NFRuleSet.ProperFractionRuleIndex, rule, true);
            }
            else if (baseValue == NFRule.MasterRule)
            {
                SetBestFractionRule(NFRuleSet.MasterRuleIndex, rule, true);
            }
            else if (baseValue == NFRule.InfinityRule)
            {
                nonNumericalRules[NFRuleSet.InfinityRuleIndex] = rule;
            }
            else if (baseValue == NFRule.NaNRule)
            {
                nonNumericalRules[NFRuleSet.NaNRuleIndex] = rule;
            }
        }

        /**
         * Determine the best fraction rule to use. Rules matching the decimal point from
         * DecimalFormatSymbols become the main set of rules to use.
         * @param originalIndex The index into nonNumericalRules
         * @param newRule The new rule to consider
         * @param rememberRule Should the new rule be added to fractionRules.
         */
        private void SetBestFractionRule(int originalIndex, NFRule newRule, bool rememberRule)
        {
            if (rememberRule)
            {
                if (fractionRules == null)
                {
                    fractionRules = new LinkedList<NFRule>();
                }
                fractionRules.AddLast(newRule);
            }
            NFRule bestResult = nonNumericalRules[originalIndex];
            if (bestResult == null)
            {
                nonNumericalRules[originalIndex] = newRule;
            }
            else
            {
                // We have more than one. Which one is better?
                DecimalFormatSymbols decimalFormatSymbols = owner.DecimalFormatSymbols;
                if (decimalFormatSymbols.DecimalSeparator == newRule.DecimalPoint)
                {
                    nonNumericalRules[originalIndex] = newRule;
                }
                // else leave it alone
            }
        }

        /**
         * Flags this rule set as a fraction rule set.  This function is
         * called during the construction process once we know this rule
         * set is a fraction rule set.  We don't know a rule set is a
         * fraction rule set until we see it used somewhere.  This function
         * is not ad must not be called at any time other than during
         * construction of a RuleBasedNumberFormat.
         */
        public void MakeIntoFractionRuleSet()
        {
            isFractionRuleSet = true;
        }

        //-----------------------------------------------------------------------
        // boilerplate
        //-----------------------------------------------------------------------

        /**
         * Compares two rule sets for equality.
         * @param that The other rule set
         * @return true if the two rule sets are functionally equivalent.
         */
        public override bool Equals(object that)
        {
            // if different classes, they're not equal
            if (!(that is NFRuleSet that2))
            {
                return false;
            }
            else
            {
                // otherwise, compare the members one by one...
                if (!name.Equals(that2.name, StringComparison.Ordinal)
                        || rules.Length != that2.rules.Length
                        || isFractionRuleSet != that2.isFractionRuleSet)
                {
                    return false;
                }

                // ...then compare the non-numerical rule lists...
                for (int i = 0; i < nonNumericalRules.Length; i++)
                {
                    if (!Utility.ObjectEquals(nonNumericalRules[i], that2.nonNumericalRules[i]))
                    {
                        return false;
                    }
                }

                // ...then compare the rule lists...
                for (int i = 0; i < rules.Length; i++)
                {
                    if (!rules[i].Equals(that2.rules[i]))
                    {
                        return false;
                    }
                }

                // ...and if we make it here, they're equal
                return true;
            }
        }

        public override int GetHashCode()
        {
            //assert false : "hashCode not designed";
            return 42;
        }


        /**
         * Builds a textual representation of a rule set.
         * @return A textual representation of a rule set.  This won't
         * necessarily be the same description that the rule set was
         * constructed with, but it will produce the same results.
         */
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            // the rule set name goes first...
            result.Append(name).Append(":\n");

            // followed by the regular rules...
            foreach (NFRule rule in rules)
            {
                result.Append(rule.ToString()).Append("\n");
            }

            // followed by the special rules (if they exist)
            foreach (NFRule rule in nonNumericalRules)
            {
                if (rule != null)
                {
                    if (rule.BaseValue == NFRule.ImproperFractionRule
                        || rule.BaseValue == NFRule.ProperFractionRule
                        || rule.BaseValue == NFRule.MasterRule)
                    {
                        foreach (NFRule fractionRule in fractionRules)
                        {
                            if (fractionRule.BaseValue == rule.BaseValue)
                            {
                                result.Append(fractionRule.ToString()).Append("\n");
                            }
                        }
                    }
                    else
                    {
                        result.Append(rule.ToString()).Append("\n");
                    }
                }
            }

            return result.ToString();
        }

        //-----------------------------------------------------------------------
        // simple accessors
        //-----------------------------------------------------------------------

        /**
         * Says whether this rule set is a fraction rule set.
         * @return true if this rule is a fraction rule set; false if it isn't
         */
        public bool IsFractionSet => isFractionRuleSet;

        /**
         * Returns the rule set's name
         * @return The rule set's name
         */
        public string Name => name;

        /**
         * Return true if the rule set is public.
         * @return true if the rule set is public
         */
        public bool IsPublic => !name.StartsWith("%%", StringComparison.Ordinal);


        /**
         * Return true if the rule set can be used for parsing.
         * @return true if the rule set can be used for parsing.
         */
        public bool IsParseable => isParseable;

        //-----------------------------------------------------------------------
        // formatting
        //-----------------------------------------------------------------------

        /**
         * Formats a long.  Selects an appropriate rule and dispatches
         * control to it.
         * @param number The number being formatted
         * @param toInsertInto The string where the result is to be placed
         * @param pos The position in toInsertInto where the result of
         * this operation is to be inserted
         */
        public void Format(long number, StringBuilder toInsertInto, int pos, int recursionCount)
        {
            if (recursionCount >= RecursionLimit)
            {
                throw new InvalidOperationException("Recursion limit exceeded when applying ruleSet " + name);
            }
            NFRule applicableRule = FindNormalRule(number);
            applicableRule.DoFormat(number, toInsertInto, pos, ++recursionCount);
        }

        /**
         * Formats a double.  Selects an appropriate rule and dispatches
         * control to it.
         * @param number The number being formatted
         * @param toInsertInto The string where the result is to be placed
         * @param pos The position in toInsertInto where the result of
         * this operation is to be inserted
         */
        public void Format(double number, StringBuilder toInsertInto, int pos, int recursionCount)
        {
            if (recursionCount >= RecursionLimit)
            {
                throw new InvalidOperationException("Recursion limit exceeded when applying ruleSet " + name);
            }
            NFRule applicableRule = FindRule(number);
            applicableRule.DoFormat(number, toInsertInto, pos, ++recursionCount);
        }

        /**
         * Selects an appropriate rule for formatting the number.
         * @param number The number being formatted.
         * @return The rule that should be used to Format it
         */
        internal NFRule FindRule(double number)
        {
            // if this is a fraction rule set, use FindFractionRuleSetRule()
            if (isFractionRuleSet)
            {
                return FindFractionRuleSetRule(number);
            }

            if (double.IsNaN(number))
            {
                NFRule rule = nonNumericalRules[NaNRuleIndex];
                if (rule == null)
                {
                    rule = owner.DefaultNaNRule;
                }
                return rule;
            }

            // if the number is negative, return the negative number rule
            // (if there isn't a negative-number rule, we pretend it's a
            // positive number)
            if (number < 0)
            {
                if (nonNumericalRules[NegativeRuleIndex] != null)
                {
                    return nonNumericalRules[NegativeRuleIndex];
                }
                else
                {
                    number = -number;
                }
            }

            if (double.IsInfinity(number))
            {
                NFRule rule = nonNumericalRules[InfinityRuleIndex];
                if (rule == null)
                {
                    rule = owner.DefaultInfinityRule;
                }
                return rule;
            }

            // if the number isn't an integer, we use one f the fraction rules...
            if (number != Math.Floor(number))
            {
                if (number < 1 && nonNumericalRules[ProperFractionRuleIndex] != null)
                {
                    // if the number is between 0 and 1, return the proper
                    // fraction rule
                    return nonNumericalRules[ProperFractionRuleIndex];
                }
                else if (nonNumericalRules[ImproperFractionRuleIndex] != null)
                {
                    // otherwise, return the improper fraction rule
                    return nonNumericalRules[ImproperFractionRuleIndex];
                }
            }

            // if there's a master rule, use it to Format the number
            if (nonNumericalRules[MasterRuleIndex] != null)
            {
                return nonNumericalRules[MasterRuleIndex];
            }
            else
            {
                // and if we haven't yet returned a rule, use FindNormalRule()
                // to find the applicable rule
                return FindNormalRule((long)Math.Round(number)); // ICU4N: Added cast to long
            }
        }

        /**
         * If the value passed to FindRule() is a positive integer, FindRule()
         * uses this function to select the appropriate rule.  The result will
         * generally be the rule with the highest base value less than or equal
         * to the number.  There is one exception to this: If that rule has
         * two substitutions and a base value that is not an even multiple of
         * its divisor, and the number itself IS an even multiple of the rule's
         * divisor, then the result will be the rule that preceded the original
         * result in the rule list.  (This behavior is known as the "rollback
         * rule", and is used to handle optional text: a rule with optional
         * text is represented internally as two rules, and the rollback rule
         * selects appropriate between them.  This avoids things like "two
         * hundred zero".)
         * @param number The number being formatted
         * @return The rule to use to Format this number
         */
        private NFRule FindNormalRule(long number)
        {
            // if this is a fraction rule set, use FindFractionRuleSetRule()
            // to find the rule (we should only go into this clause if the
            // value is 0)
            if (isFractionRuleSet)
            {
                return FindFractionRuleSetRule(number);
            }

            // if the number is negative, return the negative-number rule
            // (if there isn't one, pretend the number is positive)
            if (number < 0)
            {
                if (nonNumericalRules[NegativeRuleIndex] != null)
                {
                    return nonNumericalRules[NegativeRuleIndex];
                }
                else
                {
                    number = -number;
                }
            }

            // we have to repeat the preceding two checks, even though we
            // do them in FindRule(), because the version of Format() that
            // takes a long bypasses FindRule() and goes straight to this
            // function.  This function does skip the fraction rules since
            // we know the value is an integer (it also skips the master
            // rule, since it's considered a fraction rule.  Skipping the
            // master rule in this function is also how we avoid infinite
            // recursion)

            // binary-search the rule list for the applicable rule
            // (a rule is used for all values from its base value to
            // the next rule's base value)
            int lo = 0;
            int hi = rules.Length;
            if (hi > 0)
            {
                while (lo < hi)
                {
                    int mid = (lo + hi).TripleShift(1);
                    long ruleBaseValue = rules[mid].BaseValue;
                    if (ruleBaseValue == number)
                    {
                        return rules[mid];
                    }
                    else if (ruleBaseValue > number)
                    {
                        hi = mid;
                    }
                    else
                    {
                        lo = mid + 1;
                    }
                }
                if (hi == 0)
                { // bad rule set
                    throw new InvalidOperationException("The rule set " + name + " cannot Format the value " + number);
                }
                NFRule result = rules[hi - 1];

                // use shouldRollBack() to see whether we need to invoke the
                // rollback rule (see shouldRollBack()'s documentation for
                // an explanation of the rollback rule).  If we do, roll back
                // one rule and return that one instead of the one we'd normally
                // return
                if (result.ShouldRollBack(number))
                {
                    if (hi == 1)
                    { // bad rule set
                        throw new InvalidOperationException("The rule set " + name + " cannot roll back from the rule '" +
                                result + "'");
                    }
                    result = rules[hi - 2];
                }
                return result;
            }
            // else use the master rule
            return nonNumericalRules[MasterRuleIndex];
        }

        /**
         * If this rule is a fraction rule set, this function is used by
         * FindRule() to select the most appropriate rule for formatting
         * the number.  Basically, the base value of each rule in the rule
         * set is treated as the denominator of a fraction.  Whichever
         * denominator can produce the fraction closest in value to the
         * number passed in is the result.  If there's a tie, the earlier
         * one in the list wins.  (If there are two rules in a row with the
         * same base value, the first one is used when the numerator of the
         * fraction would be 1, and the second rule is used the rest of the
         * time.
         * @param number The number being formatted (which will always be
         * a number between 0 and 1)
         * @return The rule to use to Format this number
         */
        private NFRule FindFractionRuleSetRule(double number)
        {
            // the obvious way to do this (multiply the value being formatted
            // by each rule's base value until you get an integral result)
            // doesn't work because of rounding error.  This method is more
            // accurate

            // find the least common multiple of the rules' base values
            // and multiply this by the number being formatted.  This is
            // all the precision we need, and we can do all of the rest
            // of the math using integer arithmetic
            long leastCommonMultiple = rules[0].BaseValue;
            for (int i = 1; i < rules.Length; i++)
            {
                leastCommonMultiple = Lcm(leastCommonMultiple, rules[i].BaseValue);
            }
            long numerator = (long)Math.Round(number * leastCommonMultiple); // ICU4N: Added cast to long

            // for each rule, do the following...
            long tempDifference;
            long difference = long.MaxValue;
            int winner = 0;
            for (int i = 0; i < rules.Length; i++)
            {
                // "numerator" is the numerator of the fraction is the
                // denominator is the LCD.  The numerator if the the rule's
                // base value is the denominator is "numerator" times the
                // base value divided by the LCD.  Here we check to see if
                // that's an integer, and if not, how close it is to being
                // an integer.
                tempDifference = numerator * rules[i].BaseValue % leastCommonMultiple;

                // normalize the result of the above calculation: we want
                // the numerator's distance from the CLOSEST multiple
                // of the LCD
                if (leastCommonMultiple - tempDifference < tempDifference)
                {
                    tempDifference = leastCommonMultiple - tempDifference;
                }

                // if this is as close as we've come, keep track of how close
                // that is, and the line number of the rule that did it.  If
                // we've scored a direct hit, we don't have to look at any more
                // rules
                if (tempDifference < difference)
                {
                    difference = tempDifference;
                    winner = i;
                    if (difference == 0)
                    {
                        break;
                    }
                }
            }

            // if we have two successive rules that both have the winning base
            // value, then the first one (the one we found above) is used if
            // the numerator of the fraction is 1 and the second one is used if
            // the numerator of the fraction is anything else (this lets us
            // do things like "one third"/"two thirds" without having to define
            // a whole bunch of extra rule sets)
            if (winner + 1 < rules.Length
                    && rules[winner + 1].BaseValue == rules[winner].BaseValue)
            {
                if (Math.Round(number * rules[winner].BaseValue) < 1
                        || Math.Round(number * rules[winner].BaseValue) >= 2)
                {
                    ++winner;
                }
            }

            // finally, return the winning rule
            return rules[winner];
        }

        /**
         * Calculates the least common multiple of x and y.
         */
        private static long Lcm(long x, long y)
        {
            // binary gcd algorithm from Knuth, "The Art of Computer Programming,"
            // vol. 2, 1st ed., pp. 298-299
            long x1 = x;
            long y1 = y;

            int p2 = 0;
            while ((x1 & 1) == 0 && (y1 & 1) == 0)
            {
                ++p2;
                x1 >>= 1;
                y1 >>= 1;
            }

            long t;
            if ((x1 & 1) == 1)
            {
                t = -y1;
            }
            else
            {
                t = x1;
            }

            while (t != 0)
            {
                while ((t & 1) == 0)
                {
                    t >>= 1;
                }
                if (t > 0)
                {
                    x1 = t;
                }
                else
                {
                    y1 = -t;
                }
                t = x1 - y1;
            }
            long gcd = x1 << p2;

            // x * y == gcd(x, y) * Lcm(x, y)
            return x / gcd * y;
        }

        //-----------------------------------------------------------------------
        // parsing
        //-----------------------------------------------------------------------

        /**
         * Parses a string.  Matches the string to be parsed against each
         * of its rules (with a base value less than upperBound) and returns
         * the value produced by the rule that matched the most characters
         * in the source string.
         * @param text The string to parse
         * @param parsePosition The initial position is ignored and assumed
         * to be 0.  On exit, this object has been updated to point to the
         * first character position this rule set didn't consume.
         * @param upperBound Limits the rules that can be allowed to match.
         * Only rules whose base values are strictly less than upperBound
         * are considered.
         * @return The numerical result of parsing this string.  This will
         * be the matching rule's base value, composed appropriately with
         * the results of matching any of its substitutions.  The object
         * will be an instance of Long if it's an integral value; otherwise,
         * it will be an instance of Double.  This function always returns
         * a valid object: If nothing matched the input string at all,
         * this function returns new Long(0), and the parse position is
         * left unchanged.
         */
        public Number Parse(string text, ParsePosition parsePosition, double upperBound)
        {
            // try matching each rule in the rule set against the text being
            // parsed.  Whichever one matches the most characters is the one
            // that determines the value we return.

            ParsePosition highWaterMark = new ParsePosition(0);
            Number result = NFRule.Zero;
            Number tempResult;

            // dump out if there's no text to parse
            if (text.Length == 0)
            {
                return result;
            }

            // Try each of the negative rules, fraction rules, infinity rules and NaN rules
            foreach (NFRule fractionRule in nonNumericalRules)
            {
                if (fractionRule != null)
                {
                    tempResult = fractionRule.DoParse(text, parsePosition, false, upperBound);
                    if (parsePosition.Index > highWaterMark.Index)
                    {
                        result = tempResult;
                        highWaterMark.Index = parsePosition.Index;
                    }
                    // commented out because the error-index API on ParsePosition isn't there in 1.1.x
                    //        if (parsePosition.ErrorIndex > highWaterMark.ErrorIndex)
                    //        {
                    //            highWaterMark.ErrorIndex = parsePosition.ErrorIndex;
                    //        }
                    parsePosition.Index = 0;
                }
            }

            // finally, go through the regular rules one at a time.  We start
            // at the end of the list because we want to try matching the most
            // significant rule first (this helps ensure that we parse
            // "five thousand three hundred six" as
            // "(five thousand) (three hundred) (six)" rather than
            // "((five thousand three) hundred) (six)").  Skip rules whose
            // base values are higher than the upper bound (again, this helps
            // limit ambiguity by making sure the rules that match a rule's
            // are less significant than the rule containing the substitutions)/
            for (int i = rules.Length - 1; i >= 0 && highWaterMark.Index < text.Length; i--)
            {
                if (!isFractionRuleSet && rules[i].BaseValue >= upperBound)
                {
                    continue;
                }

                tempResult = rules[i].DoParse(text, parsePosition, isFractionRuleSet, upperBound);
                if (parsePosition.Index > highWaterMark.Index)
                {
                    result = tempResult;
                    highWaterMark.Index = parsePosition.Index;
                }
                // commented out because the error-index API on ParsePosition isn't there in 1.1.x
                //            if (parsePosition.ErrorIndex > highWaterMark.ErrorIndex {
                //                highWaterMark.ErrorIndex = parsePosition.ErrorIndex;
                //            }
                parsePosition.Index = 0;
            }

            // finally, update the parse position we were passed to point to the
            // first character we didn't use, and return the result that
            // corresponds to that string of characters
            parsePosition.Index = highWaterMark.Index;
            // commented out because the error-index API on ParsePosition isn't there in 1.1.x
            //if (parsePosition.Index == 0)
            //{
            //    parsePosition.ErrorIndex = highWaterMark.ErrorIndex;
            //}

            return result;
        }

        public void SetDecimalFormatSymbols(DecimalFormatSymbols newSymbols)
        {
            foreach (NFRule rule in rules)
            {
                rule.SetDecimalFormatSymbols(newSymbols);
            }
            // Switch the fraction rules to mirror the DecimalFormatSymbols.
            if (fractionRules != null)
            {
                for (int nonNumericalIdx = ImproperFractionRuleIndex; nonNumericalIdx <= MasterRuleIndex; nonNumericalIdx++)
                {
                    if (nonNumericalRules[nonNumericalIdx] != null)
                    {
                        foreach (NFRule rule in fractionRules)
                        {
                            if (nonNumericalRules[nonNumericalIdx].BaseValue == rule.BaseValue)
                            {
                                SetBestFractionRule(nonNumericalIdx, rule, false);
                            }
                        }
                    }
                }
            }

            foreach (NFRule rule in nonNumericalRules)
            {
                if (rule != null)
                {
                    rule.SetDecimalFormatSymbols(newSymbols);
                }
            }
        }
    }
}
