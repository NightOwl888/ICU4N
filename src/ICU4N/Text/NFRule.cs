﻿using ICU4N.Impl;
using ICU4N.Support.Text;
using J2N.Numerics;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Double = J2N.Numerics.Double;
using Long = J2N.Numerics.Int64;

namespace ICU4N.Text
{
    /// <summary>
    /// A class representing a single rule in a RuleBasedNumberFormat. A rule
    /// inserts its text into the result string and then passes control to its
    /// substitutions, which do the same thing.
    /// </summary>
    internal sealed partial class NFRule
    {
        //-----------------------------------------------------------------------
        // constants
        //-----------------------------------------------------------------------

        /// <summary>
        /// Special base value used to identify a negative-number rule
        /// </summary>
        internal const int NegativeNumberRule = -1;

        /// <summary>
        /// Special base value used to identify an improper fraction (x.x) rule
        /// </summary>
        internal const int ImproperFractionRule = -2;

        /// <summary>
        /// Special base value used to identify a proper fraction (0.x) rule
        /// </summary>
        internal const int ProperFractionRule = -3;

        /// <summary>
        /// Special base value used to identify a master rule
        /// </summary>
        internal const int MasterRule = -4;

        /// <summary>
        /// Special base value used to identify an infinity rule
        /// </summary>
        internal const int InfinityRule = -5;

        /// <summary>
        /// Special base value used to identify a not a number rule
        /// </summary>
        internal const int NaNRule = -6;

        internal static readonly Long Zero = Long.GetInstance(0);

        //-----------------------------------------------------------------------
        // data members
        //-----------------------------------------------------------------------

        /// <summary>
        /// The rule's base value
        /// </summary>
        private long baseValue;

        /// <summary>
        /// The rule's radix (the radix to the power of the exponent equals
        /// the rule's divisor)
        /// </summary>
        private int radix = 10;

        /// <summary>
        /// The rule's exponent (the radix raised to the power of the exponent
        /// equals the rule's divisor)
        /// </summary>
        private short exponent = 0;

        /// <summary>
        /// If this is a fraction rule, this is the decimal point from <see cref="IDecimalFormatSymbols"/> to match.
        /// </summary>
        private char decimalPoint = (char)0;

        /// <summary>
        /// The rule's rule text. When formatting a number, the rule's text
        /// is inserted into the result string, and then the text from any
        /// substitutions is inserted into the result string.
        /// </summary>
        private string ruleText = null;

        /// <summary>
        /// The rule's plural Format when defined. This is not a substitution
        /// because it only works on the current baseValue. It's normally not used
        /// due to the overhead.
        /// </summary>
        private PluralFormat rulePatternFormat = null;

        /// <summary>
        /// The rule's first substitution (the one with the lower offset
        /// into the rule text)
        /// </summary>
        private NFSubstitution sub1 = null;

        /// <summary>
        /// The rule's second substitution (the one with the higher offset
        /// into the rule text)
        /// </summary>
        private NFSubstitution sub2 = null;

        /// <summary>
        /// The <see cref="RuleBasedNumberFormat"/> that owns this rule.
        /// </summary>
        private readonly RuleBasedNumberFormat formatter;

        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        /// <summary>
        /// Creates one or more rules based on the <paramref name="description"/> passed in.
        /// </summary>
        /// <param name="description">The description of the rule(s).</param>
        /// <param name="owner">The rule set containing the new rule(s).</param>
        /// <param name="predecessor">The rule that precedes the new one(s) in "owner"'s
        /// rule list.</param>
        /// <param name="ownersOwner">The <see cref="RuleBasedNumberFormat"/> that owns the
        /// rule set that owns the new rule(s).</param>
        /// <param name="returnList">One or more instances of <see cref="NFRule"/> are added
        /// and returned here.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="predecessor"/>.<see cref="NFRule.ruleText"/> does not have a defined type ("ordinal" or "cardinal").
        /// <para/>
        /// -or-
        /// <para/>
        /// <paramref name="predecessor"/>.<see cref="NFRule.ruleText"/> has an invalid type (one other than "ordinal" or "cardinal").
        /// <para/>
        /// -or-
        /// <para/>
        /// A substitution within <paramref name="predecessor"/>.<see cref="NFRule.ruleText"/> starts with '&lt;'
        /// and <paramref name="predecessor"/>.<see cref="NFRule.BaseValue"/> is <see cref="NFRule.NegativeNumberRule"/>.
        /// <para/>
        /// -or-
        /// <para/>
        /// A substitution within <paramref name="predecessor"/>.<see cref="NFRule.ruleText"/> starts with '&gt;'
        /// and <paramref name="owner"/>.<see cref="NFRuleSet.IsFractionSet"/> is <c>true</c>.
        /// <para/>
        /// -or-
        /// <para/>
        /// A substitution within <paramref name="predecessor"/>.<see cref="NFRule.ruleText"/> starts with a <see cref="char"/> other than '&lt;', '&gt;', or '='.
        /// </exception>
        public static void MakeRules(string description,
                                       NFRuleSet owner,
                                       NFRule predecessor,
                                       RuleBasedNumberFormat ownersOwner,
                                       IList<NFRule> returnList)
        {
            // we know we're making at least one rule, so go ahead and
            // new it up and initialize its basevalue and divisor
            // (this also strips the rule descriptor, if any, off the
            // description string)
            NFRule rule1 = new NFRule(ownersOwner, description);
            description = rule1.ruleText;

            // check the description to see whether there's text enclosed
            // in brackets
            int brack1 = description.IndexOf('[');
            int brack2 = brack1 < 0 ? -1 : description.IndexOf(']');

            // if the description doesn't contain a matched pair of brackets,
            // or if it's of a type that doesn't recognize bracketed text,
            // then leave the description alone, initialize the rule's
            // rule text and substitutions, and return that rule
            if (brack2 < 0 || brack1 > brack2
                || rule1.baseValue == ProperFractionRule
                || rule1.baseValue == NegativeNumberRule
                || rule1.baseValue == InfinityRule
                || rule1.baseValue == NaNRule)
            {
                rule1.ExtractSubstitutions(owner, description, predecessor);
            }
            else
            {
                // if the description does contain a matched pair of brackets,
                // then it's really shorthand for two rules (with one exception)
                NFRule rule2 = null;
                StringBuilder sbuf = new StringBuilder();

                // we'll actually only split the rule into two rules if its
                // base value is an even multiple of its divisor (or it's one
                // of the special rules)
                if ((rule1.baseValue > 0
                     && rule1.baseValue % (Power(rule1.radix, rule1.exponent)) == 0)
                    || rule1.baseValue == ImproperFractionRule
                    || rule1.baseValue == MasterRule)
                {

                    // if it passes that test, new up the second rule.  If the
                    // rule set both rules will belong to is a fraction rule
                    // set, they both have the same base value; otherwise,
                    // increment the original rule's base value ("rule1" actually
                    // goes SECOND in the rule set's rule list)
                    rule2 = new NFRule(ownersOwner, null);
                    if (rule1.baseValue >= 0)
                    {
                        rule2.baseValue = rule1.baseValue;
                        if (!owner.IsFractionSet)
                        {
                            ++rule1.baseValue;
                        }
                    }
                    else if (rule1.baseValue == ImproperFractionRule)
                    {
                        // if the description began with "x.x" and contains bracketed
                        // text, it describes both the improper fraction rule and
                        // the proper fraction rule
                        rule2.baseValue = ProperFractionRule;
                    }
                    else if (rule1.baseValue == MasterRule)
                    {
                        // if the description began with "x.0" and contains bracketed
                        // text, it describes both the master rule and the
                        // improper fraction rule
                        rule2.baseValue = rule1.baseValue;
                        rule1.baseValue = ImproperFractionRule;
                    }

                    // both rules have the same radix and exponent (i.e., the
                    // same divisor)
                    rule2.radix = rule1.radix;
                    rule2.exponent = rule1.exponent;

                    // rule2's rule text omits the stuff in brackets: initialize
                    // its rule text and substitutions accordingly
                    sbuf.Append(description.Substring(0, brack1)); // ICU4N: Checked 2nd parameter
                    if (brack2 + 1 < description.Length)
                    {
                        sbuf.Append(description.Substring(brack2 + 1));
                    }
                    rule2.ExtractSubstitutions(owner, sbuf.ToString(), predecessor);
                }

                // rule1's text includes the text in the brackets but omits
                // the brackets themselves: initialize _its_ rule text and
                // substitutions accordingly
                sbuf.Length = 0;
                sbuf.Append(description.Substring(0, brack1)); // ICU4N: Checked 2nd parameter
                sbuf.Append(description.Substring(brack1 + 1, brack2 - (brack1 + 1))); // ICU4N: Corrected 2nd parameter
                if (brack2 + 1 < description.Length)
                {
                    sbuf.Append(description.Substring(brack2 + 1));
                }
                rule1.ExtractSubstitutions(owner, sbuf.ToString(), predecessor);

                // if we only have one rule, return it; if we have two, return
                // a two-element array containing them (notice that rule2 goes
                // BEFORE rule1 in the list: in all cases, rule2 OMITS the
                // material in the brackets and rule1 INCLUDES the material
                // in the brackets)
                if (rule2 != null)
                {
                    if (rule2.baseValue >= 0)
                    {
                        returnList.Add(rule2);
                    }
                    else
                    {
                        owner.SetNonNumericalRule(rule2);
                    }
                }
            }
            if (rule1.baseValue >= 0)
            {
                returnList.Add(rule1);
            }
            else
            {
                owner.SetNonNumericalRule(rule1);
            }
        }

        /// <summary>
        /// Nominal constructor for <see cref="NFRule"/>.  Most of the work of constructing
        /// an <see cref="NFRule"/> is actually performed by
        /// <see cref="MakeRules(string, NFRuleSet, NFRule, RuleBasedNumberFormat, IList{NFRule})"/>.
        /// </summary>
        /// <param name="formatter">The <see cref="RuleBasedNumberFormat"/> that owns this rule.</param>
        /// <param name="ruleText">The rule's rule text. When formatting a number, the rule's text
        /// is inserted into the result string, and then the text from any
        /// substitutions is inserted into the result string.</param>
        public NFRule(RuleBasedNumberFormat formatter, string ruleText)
        {
            this.formatter = formatter;
            this.ruleText = ruleText == null ? null : ParseRuleDescriptor(ruleText);
        }

        /// <summary>
        /// This function parses the rule's rule descriptor (i.e., the base
        /// value and/or other tokens that precede the rule's rule text
        /// in the description) and sets the rule's base value, radix, and
        /// exponent according to the descriptor.  (If the description doesn't
        /// include a rule descriptor, then this function sets everything to
        /// default values and the rule set sets the rule's real base value).
        /// </summary>
        /// <param name="description">The rule's description.</param>
        /// <returns>If <paramref name="description"/> included a rule descriptor, this is
        /// <paramref name="description"/> with the descriptor and any trailing whitespace
        /// stripped off.  Otherwise; it's <paramref name="description"/> unchanged.</returns>
        /// <exception cref="ArgumentException">
        /// The <paramref name="description"/> contains an illegal character.
        /// <para/>
        /// -or-
        /// <para/>
        /// The rule's radix is effectively zero.
        /// </exception>
        private string ParseRuleDescriptor(string description)
        {
            string descriptor;

            // the description consists of a rule descriptor and a rule body,
            // separated by a colon.  The rule descriptor is optional.  If
            // it's omitted, just set the base value to 0.
            int p = description.IndexOf(':');
            if (p != -1)
            {
                // copy the descriptor out into its own string and strip it,
                // along with any trailing whitespace, out of the original
                // description
                descriptor = description.Substring(0, p); // ICU4N: Checked 2nd parameter
                ++p;
                while (p < description.Length && PatternProps.IsWhiteSpace(description[p]))
                {
                    ++p;
                }
                description = description.Substring(p);

                // check first to see if the rule descriptor matches the token
                // for one of the special rules.  If it does, set the base
                // value to the correct identifier value
                int descriptorLength = descriptor.Length;
                char firstChar = descriptor[0];
                char lastChar = descriptor[descriptorLength - 1];
                if (firstChar >= '0' && firstChar <= '9' && lastChar != 'x')
                {
                    // if the rule descriptor begins with a digit, it's a descriptor
                    // for a normal rule
                    long tempValue = 0;
                    char c = (char)0;
                    p = 0;

                    // begin parsing the descriptor: copy digits
                    // into "tempValue", skip periods, commas, and spaces,
                    // stop on a slash or > sign (or at the end of the string),
                    // and throw an exception on any other character
                    while (p < descriptorLength)
                    {
                        c = descriptor[p];
                        if (c >= '0' && c <= '9')
                        {
                            tempValue = tempValue * 10 + (c - '0');
                        }
                        else if (c == '/' || c == '>')
                        {
                            break;
                        }
                        else if (!PatternProps.IsWhiteSpace(c) && c != ',' && c != '.')
                        {
                            throw new ArgumentException("Illegal character " + c + " in rule descriptor");
                        }
                        ++p;
                    }

                    // Set the rule's base value according to what we parsed
                    SetBaseValue(tempValue);

                    // if we stopped the previous loop on a slash, we're
                    // now parsing the rule's radix.  Again, accumulate digits
                    // in tempValue, skip punctuation, stop on a > mark, and
                    // throw an exception on anything else
                    if (c == '/')
                    {
                        tempValue = 0;
                        ++p;
                        while (p < descriptorLength)
                        {
                            c = descriptor[p];
                            if (c >= '0' && c <= '9')
                            {
                                tempValue = tempValue * 10 + (c - '0');
                            }
                            else if (c == '>')
                            {
                                break;
                            }
                            else if (!PatternProps.IsWhiteSpace(c) && c != ',' && c != '.')
                            {
                                throw new ArgumentException("Illegal character " + c + " in rule descriptor");
                            }
                            ++p;
                        }

                        // tempValue now contains the rule's radix.  Set it
                        // accordingly, and recalculate the rule's exponent
                        radix = (int)tempValue;
                        if (radix == 0)
                        {
                            throw new ArgumentException("Rule can't have radix of 0");
                        }
                        exponent = GetExpectedExponent();
                    }

                    // if we stopped the previous loop on a > sign, then continue
                    // for as long as we still see > signs.  For each one,
                    // decrement the exponent (unless the exponent is already 0).
                    // If we see another character before reaching the end of
                    // the descriptor, that's also a syntax error.
                    if (c == '>')
                    {
                        while (p < descriptorLength)
                        {
                            c = descriptor[p];
                            if (c == '>' && exponent > 0)
                            {
                                --exponent;
                            }
                            else
                            {
                                throw new ArgumentException("Illegal character in rule descriptor");
                            }
                            ++p;
                        }
                    }
                }
                else if (descriptor.Equals("-x", StringComparison.Ordinal))
                {
                    SetBaseValue(NegativeNumberRule);
                }
                else if (descriptorLength == 3)
                {
                    if (firstChar == '0' && lastChar == 'x')
                    {
                        SetBaseValue(ProperFractionRule);
                        decimalPoint = descriptor[1];
                    }
                    else if (firstChar == 'x' && lastChar == 'x')
                    {
                        SetBaseValue(ImproperFractionRule);
                        decimalPoint = descriptor[1];
                    }
                    else if (firstChar == 'x' && lastChar == '0')
                    {
                        SetBaseValue(MasterRule);
                        decimalPoint = descriptor[1];
                    }
                    else if (descriptor.Equals("NaN", StringComparison.Ordinal))
                    {
                        SetBaseValue(NaNRule);
                    }
                    else if (descriptor.Equals("Inf", StringComparison.Ordinal))
                    {
                        SetBaseValue(InfinityRule);
                    }
                }
            }
            // else use the default base value for now.

            // finally, if the rule body begins with an apostrophe, strip it off
            // (this is generally used to put whitespace at the beginning of
            // a rule's rule text)
            if (description.Length > 0 && description[0] == '\'')
            {
                description = description.Substring(1);
            }

            // return the description with all the stuff we've just waded through
            // stripped off the front.  It now contains just the rule body.
            return description;
        }

        /// <summary>
        /// Searches the rule's rule text for the substitution tokens,
        /// creates the substitutions, and removes the substitution tokens
        /// from the rule's rule text.
        /// </summary>
        /// <param name="owner">The rule set containing this rule.</param>
        /// <param name="ruleText">The rule text.</param>
        /// <param name="predecessor">The rule preceding this one in "owners" rule list.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="ruleText"/> does not have a defined type ("ordinal" or "cardinal").
        /// <para/>
        /// -or-
        /// <para/>
        /// <paramref name="ruleText"/> has an invalid type (one other than "ordinal" or "cardinal").
        /// <para/>
        /// -or-
        /// <para/>
        /// A substitution within <paramref name="ruleText"/> starts with '&lt;'
        /// and <paramref name="predecessor"/>.<see cref="NFRule.BaseValue"/> is <see cref="NFRule.NegativeNumberRule"/>.
        /// <para/>
        /// -or-
        /// <para/>
        /// A substitution within <paramref name="ruleText"/> starts with '&gt;'
        /// and <paramref name="owner"/>.<see cref="NFRuleSet.IsFractionSet"/> is <c>true</c>.
        /// <para/>
        /// -or-
        /// <para/>
        /// A substitution within <paramref name="ruleText"/> starts with a <see cref="char"/> other than '&lt;', '&gt;', or '='.
        /// </exception>
        private void ExtractSubstitutions(NFRuleSet owner,
                                          string ruleText,
                                          NFRule predecessor)
        {
            this.ruleText = ruleText;
            sub1 = ExtractSubstitution(owner, predecessor);
            if (sub1 == null)
            {
                // Small optimization. There is no need to create a redundant NullSubstitution.
                sub2 = null;
            }
            else
            {
                sub2 = ExtractSubstitution(owner, predecessor);
            }
            ruleText = this.ruleText;
            int pluralRuleStart = ruleText.IndexOf("$(", StringComparison.Ordinal);
            int pluralRuleEnd = (pluralRuleStart >= 0 ? ruleText.IndexOf(")$", pluralRuleStart, StringComparison.Ordinal) : -1);
            if (pluralRuleEnd >= 0)
            {
                int endType = ruleText.IndexOf(',', pluralRuleStart);
                if (endType < 0)
                {
                    throw new ArgumentException("Rule \"" + ruleText + "\" does not have a defined type");
                }
                string type = this.ruleText.Substring(pluralRuleStart + 2, endType - (pluralRuleStart + 2)); // ICU4N: Corrected 2nd parameter
                PluralType pluralType;
                if ("cardinal".Equals(type, StringComparison.Ordinal))
                {
                    pluralType = PluralType.Cardinal;
                }
                else if ("ordinal".Equals(type, StringComparison.Ordinal))
                {
                    pluralType = PluralType.Ordinal;
                }
                else
                {
                    throw new ArgumentException(type + " is an unknown type");
                }
                rulePatternFormat = formatter.CreatePluralFormat(pluralType,
                        ruleText.Substring(endType + 1, pluralRuleEnd - (endType + 1))); // ICU4N: Corrected 2nd parameter
            }
        }

        /// <summary>
        /// Searches the rule's rule text for the first substitution token,
        /// creates a substitution based on it, and removes the token from
        /// the rule's rule text.
        /// </summary>
        /// <param name="owner">The rule set containing this rule.</param>
        /// <param name="predecessor">The rule preceding this one in the rule set's
        /// rule list.</param>
        /// <returns>The newly-created substitution or <c>null</c>.</returns>
        /// <exception cref="ArgumentException">
        /// A substitution within <see name="ruleText"/> starts with '&lt;'
        /// and <paramref name="predecessor"/>.<see cref="NFRule.BaseValue"/> is <see cref="NFRule.NegativeNumberRule"/>.
        /// <para/>
        /// -or-
        /// <para/>
        /// A substitution within <see name="ruleText"/> starts with '&gt;'
        /// and <paramref name="owner"/>.<see cref="NFRuleSet.IsFractionSet"/> is <c>true</c>.
        /// <para/>
        /// -or-
        /// <para/>
        /// A substitution within <see name="ruleText"/> starts with a <see cref="char"/> other than '&lt;', '&gt;', or '='.
        /// </exception>
        private NFSubstitution ExtractSubstitution(NFRuleSet owner,
                                                   NFRule predecessor)
        {
            NFSubstitution result;
            int subStart;
            int subEnd;

            // search the rule's rule text for the first two characters of
            // a substitution token
            subStart = IndexOfAnyRulePrefix(ruleText);

            // if we didn't find one, create a null substitution positioned
            // at the end of the rule text
            if (subStart == -1)
            {
                return null;
            }

            // special-case the ">>>" token, since searching for the > at the
            // end will actually find the > in the middle
            if (ruleText.StartsWith(">>>", subStart, StringComparison.Ordinal))
            {
                subEnd = subStart + 2;
            }
            else
            {
                // otherwise the substitution token ends with the same character
                // it began with
                char c = ruleText[subStart];
                subEnd = ruleText.IndexOf(c, subStart + 1);
                // special case for '<%foo<<'
                if (c == '<' && subEnd != -1 && subEnd < ruleText.Length - 1 && ruleText[subEnd + 1] == c)
                {
                    // ordinals use "=#,##0==%abbrev=" as their rule.  Notice that the '==' in the middle
                    // occurs because of the juxtaposition of two different rules.  The check for '<' is a hack
                    // to get around this.  Having the duplicate at the front would cause problems with
                    // rules like "<<%" to Format, say, percents...
                    ++subEnd;
                }
            }

            // if we don't find the end of the token (i.e., if we're on a single,
            // unmatched token character), create a null substitution positioned
            // at the end of the rule
            if (subEnd == -1)
            {
                return null;
            }

            // if we get here, we have a real substitution token (or at least
            // some text bounded by substitution token characters).  Use
            // makeSubstitution() to create the right kind of substitution
            result = NFSubstitution.MakeSubstitution(subStart, this, predecessor, owner,
                    this.formatter, ruleText.Substring(subStart, (subEnd + 1) - subStart)); // ICU4N: Corrected 2nd parameter

            // remove the substitution from the rule text
            ruleText = ruleText.Substring(0, subStart) + ruleText.Substring(subEnd + 1); // ICU4N: Checked 2nd parameter
            return result;
        }

        /// <summary>
        /// Sets the rule's base value, and causes the radix and exponent
        /// to be recalculated.  This is used during construction when we
        /// don't know the rule's base value until after it's been
        /// constructed.  It should not be used at any other time.
        /// </summary>
        /// <param name="newBaseValue">The new base value for the rule.</param>
        internal void SetBaseValue(long newBaseValue)
        {
            // set the base value
            baseValue = newBaseValue;
            radix = 10;

            // if this isn't a special rule, recalculate the radix and exponent
            // (the radix always defaults to 10; if it's supposed to be something
            // else, it's cleaned up by the caller and the exponent is
            // recalculated again-- the only function that does this is
            // NFRule.ParseRuleDescriptor() )
            if (baseValue >= 1)
            {
                exponent = GetExpectedExponent();

                // this function gets called on a fully-constructed rule whose
                // description didn't specify a base value.  This means it
                // has substitutions, and some substitutions hold on to copies
                // of the rule's divisor.  Fix their copies of the divisor.
                sub1?.SetDivisor(radix, exponent);
                sub2?.SetDivisor(radix, exponent);
            }
            else
            {
                // if this is a special rule, its radix and exponent are basically
                // ignored.  Set them to "safe" default values
                exponent = 0;
            }
        }

        /// <summary>
        /// This calculates the rule's exponent based on its radix and base
        /// value. This will be the highest power the radix can be raised to
        /// and still produce a result less than or equal to the base value.
        /// </summary>
        private short GetExpectedExponent()
        {
            // since the log of 0, or the log base 0 of something, causes an
            // error, declare the exponent in these cases to be 0 (we also
            // deal with the special-rule identifiers here)
            if (radix == 0 || baseValue < 1)
            {
                return 0;
            }

            // we get rounding error in some cases-- for example, log 1000 / log 10
            // gives us 1.9999999996 instead of 2.  The extra logic here is to take
            // that into account
            short tempResult = (short)(Math.Log(baseValue) / Math.Log(radix));
            if (Power(radix, (short)(tempResult + 1)) <= baseValue)
            {
                return (short)(tempResult + 1);
            }
            else
            {
                return tempResult;
            }
        }

        private static readonly string[] RulePrefixes = new string[] {
            "<<", "<%", "<#", "<0",
            ">>", ">%", ">#", ">0",
            "=%", "=#", "=0"
        };

        /// <summary>
        /// Searches the rule's rule text for any of the specified strings.
        /// </summary>
        /// <param name="ruleText">The rule text.</param>
        /// <returns>The index of the first match in the rule's rule text
        /// (i.e., the first substring in the rule's rule text that matches
        /// _any_ of the strings in "strings").  If none of the strings in
        /// "strings" is found in the rule's rule text, returns -1.</returns>
        private static int IndexOfAnyRulePrefix(string ruleText)
        {
            int result = -1;
            if (ruleText.Length > 0)
            {
                int pos;
                foreach (string str in RulePrefixes)
                {
                    pos = ruleText.IndexOf(str, StringComparison.Ordinal);
                    if (pos != -1 && (result == -1 || pos < result))
                    {
                        result = pos;
                    }
                }
            }
            return result;
        }

        //-----------------------------------------------------------------------
        // boilerplate
        //-----------------------------------------------------------------------

        /// <summary>
        /// Tests two rules for equality.
        /// </summary>
        /// <param name="that">The rule to compare this one against.</param>
        /// <returns><c>true</c> if the two rules are functionally equivalent.</returns>
        public override bool Equals(object that)
        {
            if (that is NFRule that2)
            {
                return baseValue == that2.baseValue
                    && radix == that2.radix
                    && exponent == that2.exponent
                    && ruleText.Equals(that2.ruleText, StringComparison.Ordinal)
                    && Utility.ObjectEquals(sub1, that2.sub1)
                    && Utility.ObjectEquals(sub2, that2.sub2);
            }
            return false;
        }

        /// <inheritdoc/>
        public override int GetHashCode() // ICU4N TODO: Create real hash code - we can definitely rule out cases here.
        {
            //assert false : "hashCode not designed";
            return 42;
        }

        /// <summary>
        /// Returns a textual representation of the rule. This won't
        /// necessarily be the same as the description that this rule
        /// was created with, but it will produce the same result.
        /// </summary>
        /// <returns>A textual description of the rule.</returns>
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            // start with the rule descriptor.  Special-case the special rules
            if (baseValue == NegativeNumberRule)
            {
                result.Append("-x: ");
            }
            else if (baseValue == ImproperFractionRule)
            {
                result.Append('x').Append(decimalPoint == 0 ? '.' : decimalPoint).Append("x: ");
            }
            else if (baseValue == ProperFractionRule)
            {
                result.Append('0').Append(decimalPoint == 0 ? '.' : decimalPoint).Append("x: ");
            }
            else if (baseValue == MasterRule)
            {
                result.Append('x').Append(decimalPoint == 0 ? '.' : decimalPoint).Append("0: ");
            }
            else if (baseValue == InfinityRule)
            {
                result.Append("Inf: ");
            }
            else if (baseValue == NaNRule)
            {
                result.Append("NaN: ");
            }
            else
            {
                // for a normal rule, write out its base value, and if the radix is
                // something other than 10, write out the radix (with the preceding
                // slash, of course).  Then calculate the expected exponent and if
                // if isn't the same as the actual exponent, write an appropriate
                // number of > signs.  Finally, terminate the whole thing with
                // a colon.
                result.Append(baseValue.ToString(CultureInfo.InvariantCulture));
                if (radix != 10)
                {
                    result.Append('/').Append(radix);
                }
                int numCarets = GetExpectedExponent() - exponent;
                for (int i = 0; i < numCarets; i++)
                    result.Append('>');
                result.Append(": ");
            }

            // if the rule text begins with a space, write an apostrophe
            // (whitespace after the rule descriptor is ignored; the
            // apostrophe is used to make the whitespace significant)
            if (ruleText.StartsWith(" ", StringComparison.Ordinal) && (sub1 == null || sub1.Pos != 0))
            {
                result.Append('\'');
            }

            // now, write the rule's rule text, inserting appropriate
            // substitution tokens in the appropriate places
            StringBuilder ruleTextCopy = new StringBuilder(ruleText);
            if (sub2 != null)
            {
                ruleTextCopy.Insert(sub2.Pos, sub2.ToString());
            }
            if (sub1 != null)
            {
                ruleTextCopy.Insert(sub1.Pos, sub1.ToString());
            }
            result.Append(ruleTextCopy.ToString());

            // and finally, top the whole thing off with a semicolon and
            // return the result
            result.Append(';');
            return result.ToString();
        }

        //-----------------------------------------------------------------------
        // simple accessors
        //-----------------------------------------------------------------------

        /// <summary>
        /// Gets the rule's decimal point character.
        /// </summary>
        public char DecimalPoint => decimalPoint; // ICU4N TODO: Make this into a string

        /// <summary>
        /// Gets the rule's base value.
        /// </summary>
        public long BaseValue => baseValue;

        /// <summary>
        /// Gets the rule's divisor (the value that controls the behavior
        /// of its substitutions).
        /// </summary>
        public long Divisor => Power(radix, exponent);

        //-----------------------------------------------------------------------
        // formatting
        //-----------------------------------------------------------------------

        /// <summary>
        /// Formats the <paramref name="number"/>, and inserts the resulting text into <paramref name="toInsertInto"/>.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <param name="toInsertInto">The string where the resultant text should be inserted.</param>
        /// <param name="pos">The position in toInsertInto where the resultant text should be inserted.</param>
        /// <param name="recursionCount">The number of recursive calls to this method.</param>
        public void DoFormat(long number, StringBuilder toInsertInto, int pos, int recursionCount)
        {
            // first, insert the rule's rule text into toInsertInto at the
            // specified position, then insert the results of the substitutions
            // into the right places in toInsertInto (notice we do the
            // substitutions in reverse order so that the offsets don't get
            // messed up)
            int pluralRuleStart = ruleText.Length;
            int lengthOffset = 0;
            if (rulePatternFormat == null)
            {
                toInsertInto.Insert(pos, ruleText);
            }
            else
            {
                pluralRuleStart = ruleText.IndexOf("$(", StringComparison.Ordinal);
                int pluralRuleEnd = ruleText.IndexOf(")$", pluralRuleStart, StringComparison.Ordinal);
                int initialLength = toInsertInto.Length;
                if (pluralRuleEnd < ruleText.Length - 1)
                {
                    toInsertInto.Insert(pos, ruleText.AsSpan(pluralRuleEnd + 2));
                }
                toInsertInto.Insert(pos, rulePatternFormat.Format(number / Power(radix, exponent)));
                if (pluralRuleStart > 0)
                {
                    toInsertInto.Insert(pos, ruleText.AsSpan(0, pluralRuleStart)); // ICU4N: Checked 2nd parameter
                }
                lengthOffset = ruleText.Length - (toInsertInto.Length - initialLength);
            }
            sub2?.DoSubstitution(number, toInsertInto, pos - (sub2.Pos > pluralRuleStart ? lengthOffset : 0), recursionCount);
            sub1?.DoSubstitution(number, toInsertInto, pos - (sub1.Pos > pluralRuleStart ? lengthOffset : 0), recursionCount);
        }

        /// <summary>
        /// Formats the <paramref name="number"/>, and inserts the resulting text into <paramref name="toInsertInto"/>.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <param name="toInsertInto">The string where the resultant text should be inserted.</param>
        /// <param name="pos">The position in toInsertInto where the resultant text should be inserted.</param>
        /// <param name="recursionCount">The number of recursive calls to this method.</param>
        public void DoFormat(double number, StringBuilder toInsertInto, int pos, int recursionCount)
        {
            // first, insert the rule's rule text into toInsertInto at the
            // specified position, then insert the results of the substitutions
            // into the right places in toInsertInto
            // [again, we have two copies of this routine that do the same thing
            // so that we don't sacrifice precision in a long by casting it
            // to a double]
            int pluralRuleStart = ruleText.Length;
            int lengthOffset = 0;
            if (rulePatternFormat == null)
            {
                toInsertInto.Insert(pos, ruleText);
            }
            else
            {
                pluralRuleStart = ruleText.IndexOf("$(", StringComparison.Ordinal);
                int pluralRuleEnd = ruleText.IndexOf(")$", pluralRuleStart, StringComparison.Ordinal);
                int initialLength = toInsertInto.Length;
                if (pluralRuleEnd < ruleText.Length - 1)
                {
                    toInsertInto.Insert(pos, ruleText.AsSpan(pluralRuleEnd + 2));
                }
                double pluralVal = number;
                if (0 <= pluralVal && pluralVal < 1)
                {
                    // We're in a fractional rule, and we have to match the NumeratorSubstitution behavior.
                    // 2.3 can become 0.2999999999999998 for the fraction due to rounding errors.
                    pluralVal = Math.Round(pluralVal * Power(radix, exponent)); // ICU4N NOTE: This is different than the Java default of ToPositiveInfinity (Math.Ceiling()), but only this makes the tests pass
                }
                else
                {
                    pluralVal = pluralVal / Power(radix, exponent);
                }
                toInsertInto.Insert(pos, rulePatternFormat.Format((long)(pluralVal)));
                if (pluralRuleStart > 0)
                {
                    toInsertInto.Insert(pos, ruleText.AsSpan(0, pluralRuleStart)); // ICU4N: Checked 2nd parameter
                }
                lengthOffset = ruleText.Length - (toInsertInto.Length - initialLength);
            }
            sub2?.DoSubstitution(number, toInsertInto, pos - (sub2.Pos > pluralRuleStart ? lengthOffset : 0), recursionCount);
            sub1?.DoSubstitution(number, toInsertInto, pos - (sub1.Pos > pluralRuleStart ? lengthOffset : 0), recursionCount);
        }

        /// <summary>
        /// This is an equivalent to <see cref="Math.Pow(double, double)"/> that accurately works on 64-bit numbers.
        /// </summary>
        /// <param name="base">The base.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>radix ** exponent.</returns>
        /// <seealso cref="Math.Pow(double, double)"/>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="exponent"/> is less than zero.
        /// <para/>
        /// -or-
        /// <para/>
        /// <paramref name="base"/> is less than zero.
        /// </exception>
        internal static long Power(long @base, short exponent)
        {
            if (exponent < 0)
                throw new ArgumentOutOfRangeException(nameof(exponent), "Exponent can not be negative");
            if (@base < 0)
                throw new ArgumentOutOfRangeException(nameof(@base), "Base can not be negative");

            long result = 1;
            while (exponent > 0)
            {
                if ((exponent & 1) == 1)
                {
                    result *= @base;
                }
                @base *= @base;
                exponent >>= 1;
            }
            return result;
        }

        /// <summary>
        /// Used by the owning rule set to determine whether to invoke the
        /// rollback rule (i.e., whether this rule or the one that precedes
        /// it in the rule set's list should be used to Format the number).
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <returns><c>true</c> if the rule set should use the rule that precedes
        /// this one in its list; <c>false</c> if it should use this rule.</returns>
        public bool ShouldRollBack(long number)
        {
            // we roll back if the rule contains a modulus substitution,
            // the number being formatted is an even multiple of the rule's
            // divisor, and the rule's base value is NOT an even multiple
            // of its divisor
            // In other words, if the original description had
            //    100: << hundred[ >>];
            // that expands into
            //    100: << hundred;
            //    101: << hundred >>;
            // internally.  But when we're formatting 200, if we use the rule
            // at 101, which would normally apply, we get "two hundred zero".
            // To prevent this, we roll back and use the rule at 100 instead.
            // This is the logic that makes this happen: the rule at 101 has
            // a modulus substitution, its base value isn't an even multiple
            // of 100, and the value we're trying to Format _is_ an even
            // multiple of 100.  This is called the "rollback rule."
            if (!((sub1 != null && sub1.IsModulusSubstitution) || (sub2 != null && sub2.IsModulusSubstitution)))
            {
                return false;
            }
            long divisor = Power(radix, exponent);
            return (number % divisor) == 0 && (baseValue % divisor) != 0;
        }

        //-----------------------------------------------------------------------
        // parsing
        //-----------------------------------------------------------------------

        /// <summary>
        /// Attempts to parse the string with this rule.
        /// </summary>
        /// <param name="text">The string being parsed.</param>
        /// <param name="parsePosition">On entry, the value is ignored and assumed to
        /// be 0. On exit, this has been updated with the position of the first
        /// character not consumed by matching the text against this rule
        /// (if this rule doesn't match the text at all, the parse position
        /// if left unchanged (presumably at 0) and the function returns
        /// <c>Long.GetInstance(0)</c>.</param>
        /// <param name="isFractionRule"><c>true</c> if this rule is contained within a
        /// fraction rule set. This is only used if the rule has no
        /// substitutions.</param>
        /// <param name="upperBound">When matching the substitution, it will only
        /// consider rules with base values lower than this value.</param>
        /// <returns>If this rule matched the text, this is the rule's base value
        /// combined appropriately with the results of parsing the substitutions.
        /// If nothing matched, this is <c>Long.GetInstance(0)</c> and the parse position is
        /// left unchanged.  The result will be an instance of <see cref="Long"/> if the
        /// result is an integer and <see cref="Double"/> otherwise. The result is never <c>null</c>.</returns>
        public Number DoParse(string text, ParsePosition parsePosition, bool isFractionRule,
                              double upperBound)
        {

            // internally we operate on a copy of the string being parsed
            // (because we're going to change it) and use our own ParsePosition
            ParsePosition pp = new ParsePosition(0);

            // check to see whether the text before the first substitution
            // matches the text at the beginning of the string being
            // parsed.  If it does, strip that off the front of workText;
            // otherwise, dump out with a mismatch
            int sub1Pos = sub1 != null ? sub1.Pos : ruleText.Length;
            int sub2Pos = sub2 != null ? sub2.Pos : ruleText.Length;
            string workText = StripPrefix(text, ruleText.Substring(0, sub1Pos), pp); // ICU4N: Checked 2nd parameter
            int prefixLength = text.Length - workText.Length;

            if (pp.Index == 0 && sub1Pos != 0)
            {
                // commented out because ParsePosition doesn't have error index in 1.1.x
                //                parsePosition.ErrorIndex = pp.ErrorIndex;
                return Zero;
            }
            if (baseValue == InfinityRule)
            {
                // If you match this, don't try to perform any calculations on it.
                parsePosition.Index = pp.Index;
                return Double.GetInstance(double.PositiveInfinity);
            }
            if (baseValue == NaNRule)
            {
                // If you match this, don't try to perform any calculations on it.
                parsePosition.Index = pp.Index;
                return Double.GetInstance(double.NaN);
            }

            // this is the fun part.  The basic guts of the rule-matching
            // logic is MatchToDelimiter(), which is called twice.  The first
            // time it searches the input string for the rule text BETWEEN
            // the substitutions and tries to match the intervening text
            // in the input string with the first substitution.  If that
            // succeeds, it then calls it again, this time to look for the
            // rule text after the second substitution and to match the
            // intervening input text against the second substitution.
            //
            // For example, say we have a rule that looks like this:
            //    first << middle >> last;
            // and input text that looks like this:
            //    first one middle two last
            // First we use StripPrefix() to match "first " in both places and
            // strip it off the front, leaving
            //    one middle two last
            // Then we use MatchToDelimiter() to match " middle " and try to
            // match "one" against a substitution.  If it's successful, we now
            // have
            //    two last
            // We use MatchToDelimiter() a second time to match " last" and
            // try to match "two" against a substitution.  If "two" matches
            // the substitution, we have a successful parse.
            //
            // Since it's possible in many cases to find multiple instances
            // of each of these pieces of rule text in the input string,
            // we need to try all the possible combinations of these
            // locations.  This prevents us from prematurely declaring a mismatch,
            // and makes sure we match as much input text as we can.
            int highWaterMark = 0;
            double result = 0;
            int start = 0;
            double tempBaseValue = Math.Max(0, baseValue);

            do
            {
                // our partial parse result starts out as this rule's base
                // value.  If it finds a successful match, MatchToDelimiter()
                // will compose this in some way with what it gets back from
                // the substitution, giving us a new partial parse result
                pp.Index = 0;
                double partialResult = MatchToDelimiter(workText, start, tempBaseValue,
                                                        ruleText.Substring(sub1Pos, sub2Pos - sub1Pos), rulePatternFormat, // ICU4N: Corrected 2nd parameter
                                                        pp, sub1, upperBound).ToDouble();

                // if we got a successful match (or were trying to match a
                // null substitution), pp is now pointing at the first unmatched
                // character.  Take note of that, and try MatchToDelimiter()
                // on the input text again
                if (pp.Index != 0 || sub1 == null)
                {
                    start = pp.Index;

                    string workText2 = workText.Substring(pp.Index);
                    ParsePosition pp2 = new ParsePosition(0);

                    // the second MatchToDelimiter() will compose our previous
                    // partial result with whatever it gets back from its
                    // substitution if there's a successful match, giving us
                    // a real result
                    partialResult = MatchToDelimiter(workText2, 0, partialResult,
                                                     ruleText.Substring(sub2Pos), rulePatternFormat, pp2, sub2,
                                                     upperBound).ToDouble();

                    // if we got a successful match on this second
                    // MatchToDelimiter() call, update the high-water mark
                    // and result (if necessary)
                    if (pp2.Index != 0 || sub2 == null)
                    {
                        if (prefixLength + pp.Index + pp2.Index > highWaterMark)
                        {
                            highWaterMark = prefixLength + pp.Index + pp2.Index;
                            result = partialResult;
                        }
                    }
                    // commented out because ParsePosition doesn't have error index in 1.1.x
                    //                    else {
                    //                        int temp = pp2.ErrorIndex + sub1.Pos + pp.Index;
                    //                        if (temp> parsePosition.ErrorIndex) {
                    //                            parsePosition.ErrorIndex = temp;
                    //                        }
                    //                    }
                }
                // commented out because ParsePosition doesn't have error index in 1.1.x
                //                else {
                //                    int temp = sub1.Pos + pp.ErrorIndex;
                //                    if (temp > parsePosition.ErrorIndex) {
                //                        parsePosition.ErrorIndex = temp;
                //                    }
                //                }
                // keep trying to match things until the outer MatchToDelimiter()
                // call fails to make a match (each time, it picks up where it
                // left off the previous time)
            }
            while (sub1Pos != sub2Pos && pp.Index > 0 && pp.Index
                     < workText.Length && pp.Index != start);

            // update the caller's ParsePosition with our high-water mark
            // (i.e., it now points at the first character this function
            // didn't match-- the ParsePosition is therefore unchanged if
            // we didn't match anything)
            parsePosition.Index = highWaterMark;
            // commented out because ParsePosition doesn't have error index in 1.1.x
            //        if (highWaterMark > 0) {
            //            parsePosition.ErrorIndex = 0;
            //        }

            // this is a hack for one unusual condition: Normally, whether this
            // rule belong to a fraction rule set or not is handled by its
            // substitutions.  But if that rule HAS NO substitutions, then
            // we have to account for it here.  By definition, if the matching
            // rule in a fraction rule set has no substitutions, its numerator
            // is 1, and so the result is the reciprocal of its base value.
            if (isFractionRule && highWaterMark > 0 && sub1 == null)
            {
                result = 1 / result;
            }

            // return the result as a Long if possible, or as a Double
            if (result == (long)result)
            {
                return Long.GetInstance((long)result);
            }
            else
            {
                return Double.GetInstance(result);
            }
        }

        /// <summary>
        /// This function is used by <see cref="DoParse(string, ParsePosition, bool, double)"/>
        /// to match the text being parsed against a possible prefix string. This function
        /// matches characters from the beginning of the string being parsed
        /// to characters from the prospective prefix.  If they match, <paramref name="pp"/> is
        /// updated to the first character not matched, and the result is
        /// the unparsed part of the string. If they don't match, the whole
        /// string is returned, and <paramref name="pp"/> is left unchanged.
        /// </summary>
        /// <param name="text">The string being parsed.</param>
        /// <param name="prefix">The text to match against.</param>
        /// <param name="pp">On entry, ignored and assumed to be 0.  On exit, points
        /// to the first unmatched character (assuming the whole prefix matched),
        /// or is unchanged (if the whole prefix didn't match).</param>
        /// <returns>If things match, this is the unparsed part of <paramref name="text"/>;
        /// if they didn't match, this is <paramref name="text"/>.</returns>
        private string StripPrefix(string text, string prefix, ParsePosition pp)
        {
            // if the prefix text is empty, dump out without doing anything
            if (prefix.Length == 0)
            {
                return text;
            }
            else
            {
                // otherwise, use PrefixLength() to match the beginning of
                // "text" against "prefix".  This function returns the
                // number of characters from "text" that matched (or 0 if
                // we didn't match the whole prefix)
                int pfl = PrefixLength(text, prefix);
                if (pfl != 0)
                {
                    // if we got a successful match, update the parse position
                    // and strip the prefix off of "text"
                    pp.Index = pp.Index + pfl;
                    return text.Substring(pfl);

                    // if we didn't get a successful match, leave everything alone
                }
                else
                {
                    return text;
                }
            }
        }

        /// <summary>
        /// Used by <see cref="DoParse(string, ParsePosition, bool, double)"/>
        /// to match a substitution and any following text.
        /// <paramref name="text"/> is searched for instances of <paramref name="delimiter"/>
        /// For each instance of <paramref name="delimiter"/>, the intervening text is tested
        /// to see whether it matches the substitution <paramref name="sub"/>. The longest match wins.
        /// </summary>
        /// <param name="text">The string being parsed.</param>
        /// <param name="startPos">The position in <paramref name="text"/> where we should start
        /// looking for <paramref name="delimiter"/>.</param>
        /// <param name="baseVal">A partial parse result (often the rule's base value),
        /// which is combined with the result from matching the substitution <paramref name="sub"/>.</param>
        /// <param name="delimiter">The string to search <paramref name="text"/> for.</param>
        /// <param name="pluralFormatDelimiter"></param>
        /// <param name="pp">Ignored and presumed to be 0 on entry. If there's a match,
        /// on exit this will point to the first unmatched character.</param>
        /// <param name="sub">If we find <paramref name="delimiter"/> in <paramref name="text"/>, this substitution is used
        /// to match the text between the beginning of the string and the
        /// position of <paramref name="delimiter"/>. (If "delimiter" is the empty string, then
        /// this function just matches against this substitution and updates
        /// everything accordingly.)</param>
        /// <param name="upperBound">When matching the substitution, it will only
        /// consider rules with base values lower than this value.</param>
        /// <returns>If there's a match, this is the result of composing
        /// <paramref name="baseVal"/> with the result of matching the substitution. Otherwise,
        /// this is <c>Long.GetInstance(0)</c>. It's never <c>null</c>. If the result is an integer,
        /// this will be an instance of <see cref="Long"/>; otherwise, it's an instance of
        /// <see cref="Double"/>.</returns>
        private Number MatchToDelimiter(string text, int startPos, double baseVal,
                                        string delimiter, PluralFormat pluralFormatDelimiter, ParsePosition pp, NFSubstitution sub, double upperBound)
        {
            // if "delimiter" contains real (i.e., non-ignorable) text, search
            // it for "delimiter" beginning at "start".  If that succeeds, then
            // use "sub"'s doParse() method to match the text before the
            // instance of "delimiter" we just found.
            if (!AllIgnorable(delimiter))
            {
                ParsePosition tempPP = new ParsePosition(0);
                Number tempResult;

                // use FindText() to search for "delimiter".  It returns a two-
                // element array: element 0 is the position of the match, and
                // element 1 is the number of characters that matched
                // "delimiter".
                int[] temp = FindText(text, delimiter, pluralFormatDelimiter, startPos);
                int dPos = temp[0];
                int dLen = temp[1];

                // if FindText() succeeded, isolate the text preceding the
                // match, and use "sub" to match that text
                while (dPos >= 0)
                {
                    string subText = text.Substring(0, dPos); // ICU4N: Checked 2nd parameter
                    if (subText.Length > 0)
                    {
                        tempResult = sub.DoParse(subText, tempPP, baseVal, upperBound,
                                                 formatter.LenientParseEnabled);

                        // if the substitution could match all the text up to
                        // where we found "delimiter", then this function has
                        // a successful match.  Bump the caller's parse position
                        // to point to the first character after the text
                        // that matches "delimiter", and return the result
                        // we got from parsing the substitution.
                        if (tempPP.Index == dPos)
                        {
                            pp.Index = dPos + dLen;
                            return tempResult;
                        }
                        // commented out because ParsePosition doesn't have error index in 1.1.x
                        //                    else {
                        //                        if (tempPP.ErrorIndex > 0) {
                        //                            pp.ErrorIndex = tempPP.ErrorIndex;
                        //                        } else {
                        //                            pp.ErrorIndex = tempPP.Index;
                        //                        }
                        //                    }
                    }

                    // if we didn't match the substitution, search for another
                    // copy of "delimiter" in "text" and repeat the loop if
                    // we find it
                    tempPP.Index = 0;
                    temp = FindText(text, delimiter, pluralFormatDelimiter, dPos + dLen);
                    dPos = temp[0];
                    dLen = temp[1];
                }
                // if we make it here, this was an unsuccessful match, and we
                // leave pp unchanged and return 0
                pp.Index = 0;
                return Zero;

                // if "delimiter" is empty, or consists only of ignorable characters
                // (i.e., is semantically empty), thwe we obviously can't search
                // for "delimiter".  Instead, just use "sub" to parse as much of
                // "text" as possible.
            }
            else if (sub == null)
            {
                return Double.GetInstance(baseVal);
            }
            else
            {
                ParsePosition tempPP = new ParsePosition(0);
                Number result = Zero;
                // try to match the whole string against the substitution
                Number tempResult = sub.DoParse(text, tempPP, baseVal, upperBound,
                        formatter.LenientParseEnabled);
                if (tempPP.Index != 0)
                {
                    // if there's a successful match (or it's a null
                    // substitution), update pp to point to the first
                    // character we didn't match, and pass the result from
                    // sub.doParse() on through to the caller
                    pp.Index = tempPP.Index;
                    if (tempResult != null)
                    {
                        result = tempResult;
                    }
                }
                // commented out because ParsePosition doesn't have error index in 1.1.x
                //            else {
                //                pp.ErrorIndex = tempPP.ErrorIndex;
                //            }

                // and if we get to here, then nothing matched, so we return
                // 0 and leave pp alone
                return result;
            }
        }

        /// <summary>
        /// Used by <see cref="StripPrefix(string, string, ParsePosition)"/> to match characters. If lenient parse mode
        /// is off, this just calls <see cref="string.StartsWith(string, StringComparison)"/> using <see cref="StringComparison.Ordinal"/>.
        /// If lenient parse mode is on, this function uses <see cref="CollationElementIterator"/>s to match characters in
        /// the strings (only primary-order differences are significant in
        /// determining whether there's a match).
        /// </summary>
        /// <param name="str">The string being tested.</param>
        /// <param name="prefix">The text we're hoping to see at the beginning of <paramref name="str"/>.</param>
        /// <returns>If <paramref name="prefix"/> is found at the beginning of <paramref name="str"/>, this
        /// is the number of characters in <paramref name="str"/> that were matched (this
        /// isn't necessarily the same as the length of <paramref name="prefix"/> when matching
        /// text with a collator). If there's no match, this is 0.</returns>
        private int PrefixLength(string str, string prefix)
        {
            // if we're looking for an empty prefix, it obviously matches
            // zero characters.  Just go ahead and return 0.
            if (prefix.Length == 0)
            {
                return 0;
            }

#pragma warning disable CS0618 // Type or member is obsolete
            IRbnfLenientScanner scanner = formatter.LenientScanner;
            if (scanner != null)
            {
                return scanner.PrefixLength(str, prefix);
            }
#pragma warning restore CS0618 // Type or member is obsolete

            // If lenient parsing is turned off, forget all that crap above.
            // Just use String.startsWith() and be done with it.
            if (str.StartsWith(prefix, StringComparison.Ordinal))
            {
                return prefix.Length;
            }
            return 0;
        }

        /// <summary>
        /// Searches a string for another string. If lenient parsing is off,
        /// this just calls <see cref="string.IndexOf(string, int, StringComparison)"/>.
        /// If lenient parsing is on, this function
        /// uses <see cref="CollationElementIterator"/> to match characters, and only
        /// primary-order differences are significant in determining whether
        /// there's a match.
        /// </summary>
        /// <param name="str">The string to search.</param>
        /// <param name="key">The string to search <paramref name="str"/> for.</param>
        /// <param name="pluralFormatKey"></param>
        /// <param name="startingAt">The index into <paramref name="str"/> where the search is to begin.</param>
        /// <returns>A two-element array of ints.  Element 0 is the position
        /// of the match, or -1 if there was no match.  Element 1 is the
        /// number of characters in <paramref name="str"/> that matched (which isn't necessarily
        /// the same as the length of <paramref name="key"/>).</returns>
        private int[] FindText(string str, string key, PluralFormat pluralFormatKey, int startingAt)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            IRbnfLenientScanner scanner = formatter.LenientScanner;
#pragma warning restore CS0618 // Type or member is obsolete
            if (pluralFormatKey != null)
            {
                FieldPosition position = new FieldPosition(NumberFormat.IntegerField);
                position.BeginIndex = startingAt;
                pluralFormatKey.ParseType(str, scanner, position);
                int start = position.BeginIndex;
                if (start >= 0)
                {
                    int pluralRuleStart = ruleText.IndexOf("$(", StringComparison.Ordinal);
                    int pluralRuleSuffix = ruleText.IndexOf(")$", pluralRuleStart, StringComparison.Ordinal) + 2;
                    int matchLen = position.EndIndex - start;
                    string prefix = ruleText.Substring(0, pluralRuleStart); // ICU4N: Checked 2nd parameter
                    string suffix = ruleText.Substring(pluralRuleSuffix);
                    if (str.RegionMatches(start - prefix.Length, prefix, 0, prefix.Length, StringComparison.Ordinal)
                            && str.RegionMatches(start + matchLen, suffix, 0, suffix.Length, StringComparison.Ordinal))
                    {
                        return new int[] { start - prefix.Length, matchLen + prefix.Length + suffix.Length };
                    }
                }
                return new int[] { -1, 0 };
            }

            if (scanner != null)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                // if lenient parsing is turned ON, we've got some work
                // ahead of us
                return scanner.FindText(str, key, startingAt);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            // if lenient parsing is turned off, this is easy. Just call
            // String.indexOf() and we're done
            return new int[] { str.IndexOf(key, startingAt, StringComparison.Ordinal), key.Length };
        }

        /// <summary>
        /// Checks to see whether a string consists entirely of ignorable
        /// characters.
        /// </summary>
        /// <param name="str">The string to test.</param>
        /// <returns><c>true</c> if the string is empty of consists entirely of
        /// characters that the number formatter's collator says are
        /// ignorable at the primary-order level; <c>false</c> otherwise.</returns>
        private bool AllIgnorable(string str)
        {
            // if the string is empty, we can just return true
            if (str == null || str.Length == 0)
            {
                return true;
            }
#pragma warning disable CS0618 // Type or member is obsolete
            IRbnfLenientScanner scanner = formatter.LenientScanner;
            return scanner != null && scanner.AllIgnorable(str);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public void SetDecimalFormatSymbols(DecimalFormatSymbols newSymbols)
        {
            if (sub1 != null)
            {
                sub1.SetDecimalFormatSymbols(newSymbols);
            }
            if (sub2 != null)
            {
                sub2.SetDecimalFormatSymbols(newSymbols);
            }
        }
    }
}
