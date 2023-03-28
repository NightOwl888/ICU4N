using ICU4N.Support.Text;
using J2N.Numerics;
using System;
using System.Diagnostics;

namespace ICU4N.Globalization
{
#if FEATURE_SPAN
    internal sealed partial class NumberFormatRuleSet
    {
        /// <summary>
        /// Formats the <paramref name="number"/>. Selects an appropriate rule and dispatches
        /// control to it.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <param name="toInsertInto">The string where the result is to be placed.</param>
        /// <param name="pos">The position in toInsertInto where the result of
        /// this operation is to be inserted.</param>
        /// <param name="info">The <see cref="UNumberFormatInfo"/> that contains the culture specific number formatting settings.</param>
        /// <param name="recursionCount">The number of recursive calls to this method.</param>
        /// <exception cref="InvalidOperationException">The <paramref name="recursionCount"/> went over the <see cref="RecursionLimit"/>.</exception>
        public void Format(long number, ref ValueStringBuilder toInsertInto, int pos, UNumberFormatInfo info, int recursionCount)
        {
            Debug.Assert(info != null);

            if (recursionCount >= RecursionLimit)
            {
                throw new InvalidOperationException("Recursion limit exceeded when applying ruleSet " + name);
            }
            NumberFormatRule applicableRule = FindNormalRule(number, info);
            applicableRule.DoFormat(number, ref toInsertInto, pos, info, ++recursionCount);
        }

        /// <summary>
        /// Formats the <paramref name="number"/>. Selects an appropriate rule and dispatches
        /// control to it.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <param name="toInsertInto">The string where the result is to be placed.</param>
        /// <param name="pos">The position in toInsertInto where the result of
        /// this operation is to be inserted.</param>
        /// <param name="info">The <see cref="UNumberFormatInfo"/> that contains the culture specific number formatting settings.</param>
        /// <param name="recursionCount">The number of recursive calls to this method.</param>
        /// <exception cref="InvalidOperationException">The <paramref name="recursionCount"/> went over the <see cref="RecursionLimit"/>.</exception>
        public void Format(double number, ref ValueStringBuilder toInsertInto, int pos, UNumberFormatInfo info, int recursionCount)
        {
            Debug.Assert(info != null);

            if (recursionCount >= RecursionLimit)
            {
                throw new InvalidOperationException("Recursion limit exceeded when applying ruleSet " + name);
            }
            NumberFormatRule applicableRule = FindRule(number, info);
            applicableRule.DoFormat(number, ref toInsertInto, pos, info, ++recursionCount);
        }

        /// <summary>
        /// Selects an appropriate rule for formatting the number.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <param name="info">The <see cref="UNumberFormatInfo"/> that contains the culture specific number formatting settings.</param>
        /// <returns>The rule that should be used to format it</returns>
        internal NumberFormatRule FindRule(double number, UNumberFormatInfo info) // ICU4N TODO: Pass in IDecimalFormatSymbols to get the current NaN and Infinity strings
        {
            Debug.Assert(info != null);

            // if this is a fraction rule set, use FindFractionRuleSetRule()
            if (isFractionRuleSet)
            {
                return FindFractionRuleSetRule(number);
            }

            if (double.IsNaN(number))
            {
                NumberFormatRule rule = nonNumericalRules[NaNRuleIndex];
                if (rule == null)
                {
                    //rule = owner.DefaultNaNRule;
                    rule = new NumberFormatRule(owner, "NaN: ", info.NaNSymbol); // ICU4N TODO: Cache a reference to this using async local/thread local?
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
                NumberFormatRule rule = nonNumericalRules[InfinityRuleIndex];
                if (rule == null)
                {
                    //rule = owner.DefaultInfinityRule;
                    rule = new NumberFormatRule(owner, "Inf: ", info.PositiveInfinitySymbol); // ICU4N TODO: Cache a reference to this using async local/thread local?
                }
                return rule;
            }

            // if the number isn't an integer, we use one f the fraction rules...
            if (number != Math.Floor(number))
            {
                //if (number < 1 && nonNumericalRules[ProperFractionRuleIndex] != null)
                //{
                //    // if the number is between 0 and 1, return the proper
                //    // fraction rule
                //    return nonNumericalRules[ProperFractionRuleIndex];
                //}
                //else if (nonNumericalRules[ImproperFractionRuleIndex] != null)
                //{
                //    // otherwise, return the improper fraction rule
                //    return nonNumericalRules[ImproperFractionRuleIndex];
                //}
                NumberFormatRule temp;
                if (number < 1 && (temp = GetBestFractionRule(NumberFormatRule.ProperFractionRule, info)) is not null)
                {
                    // if the number is between 0 and 1, return the proper
                    // fraction rule
                    return temp;
                }
                else if ((temp = GetBestFractionRule(NumberFormatRule.ImproperFractionRule, info)) is not null)
                {
                    // otherwise, return the improper fraction rule
                    return temp;
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
                return FindNormalRule((long)Math.Round(number), info); // ICU4N NOTE: This is different than the Java default of ToPositiveInfinity (Math.Ceiling()), but only this makes the tests pass
            }
        }

        /// <summary>
        /// If the value passed to <see cref="FindRule(double, UNumberFormatInfo)"/>
        /// is a positive integer, <see cref="FindRule(double, UNumberFormatInfo)"/>
        /// uses this function to select the appropriate rule.  The result will
        /// generally be the rule with the highest base value less than or equal
        /// to the number.  There is one exception to this: If that rule has
        /// two substitutions and a base value that is not an even multiple of
        /// its divisor, and the number itself IS an even multiple of the rule's
        /// divisor, then the result will be the rule that preceded the original
        /// result in the rule list.  (This behavior is known as the "rollback
        /// rule", and is used to handle optional text: a rule with optional
        /// text is represented internally as two rules, and the rollback rule
        /// selects appropriate between them. This avoids things like "two
        /// hundred zero".)
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <param name="info">The culture specific formatting information.</param>
        /// <returns>The rule to use to format this number.</returns>
        /// <exception cref="InvalidOperationException">
        /// This rule set cannot format the <paramref name="number"/> because it doesn't contain a corresponding rule.
        /// <para/>
        /// -or-
        /// <para/>
        /// The rule requires a rollback rule, but there is no valid rule to roll back to.
        /// </exception>
        private NumberFormatRule FindNormalRule(long number, UNumberFormatInfo info)
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
                NumberFormatRule result = rules[hi - 1];

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
            //return nonNumericalRules[MasterRuleIndex];
            return GetBestFractionRule(NumberFormatRule.MasterRule, info);
        }

        /// <summary>
        /// If this rule is a fraction rule set, this function is used by
        /// <see cref="FindRule(double, UNumberFormatInfo)"/> to select the most appropriate rule for formatting
        /// the number. Basically, the base value of each rule in the rule
        /// set is treated as the denominator of a fraction. Whichever
        /// denominator can produce the fraction closest in value to the
        /// number passed in is the result. If there's a tie, the earlier
        /// one in the list wins.  (If there are two rules in a row with the
        /// same base value, the first one is used when the numerator of the
        /// fraction would be 1, and the second rule is used the rest of the
        /// time.
        /// </summary>
        /// <param name="number">The number being formatted (which will always be
        /// a number between 0 and 1).</param>
        /// <returns>The rule to use to format this number.</returns>
        private NumberFormatRule FindFractionRuleSetRule(double number)
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
            long numerator = (long)Math.Round(number * leastCommonMultiple); // ICU4N NOTE: This is different than the Java default of ToPositiveInfinity (Math.Ceiling()), but only this makes the tests pass

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
                        || Math.Round(number * rules[winner].BaseValue) >= 2) // ICU4N NOTE: This is different than the Java default of ToPositiveInfinity (Math.Ceiling()), but only this makes the tests pass
                {
                    ++winner;
                }
            }

            // finally, return the winning rule
            return rules[winner];
        }

        /// <summary>
        /// Calculates the least common multiple of <paramref name="x"/> and <paramref name="y"/>.
        /// </summary>
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
    }
#endif
}
