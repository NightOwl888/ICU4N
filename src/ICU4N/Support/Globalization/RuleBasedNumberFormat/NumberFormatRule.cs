using ICU4N.Impl;
using ICU4N.Text;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
#nullable enable

namespace ICU4N.Globalization
{
#if FEATURE_SPAN
    /// <summary>
    /// A class representing a single rule in a <see cref="INumberFormatRules"/>. A rule
    /// inserts its text into the result string and then passes control to its
    /// substitutions, which do the same thing.
    /// </summary>
    internal sealed partial class NumberFormatRule
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

        private static readonly string[] RulePrefixes = new string[] {
            "<<", "<%", "<#", "<0",
            ">>", ">%", ">#", ">0",
            "=%", "=#", "=0"
        };

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
        private string decimalPoint = string.Empty;

        /// <summary>
        /// The rule's rule text. When formatting a number, the rule's text
        /// is inserted into the result string, and then the text from any
        /// substitutions is inserted into the result string.
        /// </summary>
        private string? ruleText = null;

        ///// <summary>
        ///// The rule's plural Format when defined. This is not a substitution
        ///// because it only works on the current baseValue. It's normally not used
        ///// due to the overhead.
        ///// </summary>
        //private PluralFormat rulePatternFormat = null;

        // ICU4N specific - we hold a reference to PluralRules instead of PluralFormat for now.
        // Once we figure out a way to format PluralFormat, this may need to be changed back.

        ///// <summary>
        ///// The rule's pluaral rules when defined. This is not a substitution
        ///// because it only works on the current <see cref="baseValue"/>. It's normally not
        ///// used due to the overhead.
        ///// </summary>
        //private PluralRules? pluralRules = null; // ICU4N TODO: This must be instantiated in CultureData so it can be shared with instances of UNumberFormatInfo. It is instantiated using PluralRules.ForLocale(), but this needs to be fixed to pass the culture name as a string.

        /// <summary>
        /// The plural rules message pattern when defined.
        /// </summary>
        private MessagePattern? pluralMessagePattern = null;

        /// <summary>
        /// The rule's plural rule text when defined. This value is not defined when <see cref="pluralMessagePattern"/> is <c>null</c>.
        /// </summary>
        private string? pluralRulesText = null;

        /// <summary>
        /// The rule's plural type when defined. This value is not defined when <see cref="pluralMessagePattern"/> is <c>null</c>.
        /// </summary>
        private PluralType pluralType;


        /// <summary>
        /// The rule's first substitution (the one with the lower offset
        /// into the rule text)
        /// </summary>
        private NumberFormatSubstitution? sub1 = null;

        /// <summary>
        /// The rule's second substitution (the one with the higher offset
        /// into the rule text)
        /// </summary>
        private NumberFormatSubstitution? sub2 = null;

        /// <summary>
        /// The <see cref="INumberFormatRules"/> that owns this formatter.
        /// </summary>
        private readonly INumberFormatRules numberFormatRules; // ICU4N: This was a reference to RuleBasedNumberFormat in ICU4J

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
        /// <param name="returnList">One or more instances of <see cref="NumberFormatRule"/> are added
        /// and returned here.</param>
        /// <exception cref="ArgumentNullException"><paramref name="owner"/>, <paramref name="ownersOwner"/>,
        /// or <paramref name="returnList"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="predecessor"/>.<see cref="NumberFormatRule.ruleText"/> does not have a defined type ("ordinal" or "cardinal").
        /// <para/>
        /// -or-
        /// <para/>
        /// <paramref name="predecessor"/>.<see cref="NumberFormatRule.ruleText"/> has an invalid type (one other than "ordinal" or "cardinal").
        /// <para/>
        /// -or-
        /// <para/>
        /// A substitution within <paramref name="predecessor"/>.<see cref="NumberFormatRule.ruleText"/> starts with '&lt;'
        /// and <paramref name="predecessor"/>.<see cref="NumberFormatRule.BaseValue"/> is <see cref="NumberFormatRule.NegativeNumberRule"/>.
        /// <para/>
        /// -or-
        /// <para/>
        /// A substitution within <paramref name="predecessor"/>.<see cref="NumberFormatRule.ruleText"/> starts with '&gt;'
        /// and <paramref name="owner"/>.<see cref="NumberFormatRuleSet.IsFractionSet"/> is <c>true</c>.
        /// <para/>
        /// -or-
        /// <para/>
        /// A substitution within <paramref name="predecessor"/>.<see cref="NumberFormatRule.ruleText"/> starts with a <see cref="char"/> other than '&lt;', '&gt;', or '='.
        /// </exception>
        public static void MakeRules(ReadOnlySpan<char> description,
            NumberFormatRuleSet owner,
            NumberFormatRule? predecessor,
            INumberFormatRules ownersOwner,
            LinkedList<NumberFormatRule> returnList)
        {
            if (owner is null)
                throw new ArgumentNullException(nameof(owner));
            if (ownersOwner is null)
                throw new ArgumentNullException(nameof(ownersOwner));
            if (returnList is null)
                throw new ArgumentNullException(nameof(returnList));

            // we know we're making at least one rule, so go ahead and
            // new it up and initialize its basevalue and divisor
            // (this also strips the rule descriptor, if any, off the
            // description string)
            NumberFormatRule rule1 = new NumberFormatRule(ownersOwner); // ICU4N: Removed ruleText in constructor, since we have more processing to do before setting it, and don't want to allocate the string yet.
            description = rule1.ParseRuleDescription(description);

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
                NumberFormatRule? rule2 = null;
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
                    rule2 = new NumberFormatRule(ownersOwner);
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
                    sbuf.Append(description.Slice(0, brack1)); // ICU4N: Checked 2nd parameter
                    if (brack2 + 1 < description.Length)
                    {
                        sbuf.Append(description.Slice(brack2 + 1));
                    }
                    rule2.ExtractSubstitutions(owner, sbuf.ToString(), predecessor);
                }

                // rule1's text includes the text in the brackets but omits
                // the brackets themselves: initialize _its_ rule text and
                // substitutions accordingly
                sbuf.Length = 0;
                sbuf.Append(description.Slice(0, brack1)); // ICU4N: Checked 2nd parameter
                sbuf.Append(description.Slice(brack1 + 1, brack2 - (brack1 + 1))); // ICU4N: Corrected 2nd parameter
                if (brack2 + 1 < description.Length)
                {
                    sbuf.Append(description.Slice(brack2 + 1));
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
                        returnList.AddLast(rule2);
                    }
                    else
                    {
                        owner.SetNonNumericalRule(rule2);
                    }
                }
            }
            if (rule1.baseValue >= 0)
            {
                returnList.AddLast(rule1);
            }
            else
            {
                owner.SetNonNumericalRule(rule1);
            }
        }

        /// <summary>
        /// Nominal constructor for <see cref="NumberFormatRule"/>.  Most of the work of constructing
        /// an <see cref="NumberFormatRule"/> is actually performed by
        /// <see cref="MakeRules(ReadOnlySpan{char}, NumberFormatRuleSet, NumberFormatRule, INumberFormatRules, LinkedList{NumberFormatRule})"/>.
        /// </summary>
        /// <param name="owner">The <see cref="RuleBasedNumberFormat"/> that owns this rule.</param>
        /// <exception cref="ArgumentNullException"><paramref name="owner"/> is <c>null</c>.</exception>
        public NumberFormatRule(INumberFormatRules owner)
        {
            this.numberFormatRules = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <summary>
        /// Intializes a new instance of <see cref="NumberFormatRule"/> for simple cases where we don't have any substitutions,
        /// such as <c>"Inf", decimalFormatSymbols.Infinity</c> or <c>"NaN", decimalFormatSymbols.NaN</c>.
        /// </summary>
        /// <param name="owner">The <see cref="RuleBasedNumberFormat"/> that owns this rule.</param>
        /// <param name="descriptor">The rule's descriptor (such as "Inf" or "NaN").</param>
        /// <param name="description">The rule's description, minus the descriptor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="owner"/> is <c>null</c>.</exception>
        public NumberFormatRule(INumberFormatRules owner, ReadOnlySpan<char> descriptor, ReadOnlySpan<char> description)
        {
            this.numberFormatRules = owner ?? throw new ArgumentNullException(nameof(owner));
            this.ruleText = new string(ParseRuleDescriptor(descriptor, description));
        }

        private static ReadOnlySpan<char> RemoveLeadingApostrophe(ReadOnlySpan<char> description)
            => description.Length > 0 && description[0] == '\'' ? description.Slice(1) : description;

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
        private ReadOnlySpan<char> ParseRuleDescription(ReadOnlySpan<char> description)
        {
            ReadOnlySpan<char> descriptor;

            // the description consists of a rule descriptor and a rule body,
            // separated by a colon.  The rule descriptor is optional.  If
            // it's omitted, just set the base value to 0.
            int p = description.IndexOf(':');
            if (p != -1)
            {
                // copy the descriptor out into its own string and strip it,
                // along with any trailing whitespace, out of the original
                // description
                descriptor = description.Slice(0, p); // ICU4N: Checked 2nd parameter
                //++p;
                //while (p < description.Length && PatternProps.IsWhiteSpace(description[p]))
                //{
                //    ++p;
                //}
                //description = description.Slice(p).TrimStart(PatternProps.WhiteSpace);

                return ParseRuleDescriptor(descriptor, description.Slice(p + 1).TrimStart(PatternProps.WhiteSpace));
            }
            // else use the default base value for now.

            // finally, if the rule body begins with an apostrophe, strip it off
            // (this is generally used to put whitespace at the beginning of
            // a rule's rule text)
            return RemoveLeadingApostrophe(description);
        }

        /// <summary>
        /// This function parses the rule's rule descriptor (i.e., the base
        /// value and/or other tokens that precede the rule's rule text
        /// in the description) and sets the rule's base value, radix, and
        /// exponent according to the descriptor.
        /// </summary>
        /// <param name="descriptor">The rule's descriptor (i.e., the base value and/or other tokens).</param>
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
        private ReadOnlySpan<char> ParseRuleDescriptor(ReadOnlySpan<char> descriptor, ReadOnlySpan<char> description)
        {
            // check first to see if the rule descriptor matches the token
            // for one of the special rules.  If it does, set the base
            // value to the correct identifier value
            int p;
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
                    decimalPoint = char.ToString(descriptor[1]);
                }
                else if (firstChar == 'x' && lastChar == 'x')
                {
                    SetBaseValue(ImproperFractionRule);
                    decimalPoint = char.ToString(descriptor[1]);
                }
                else if (firstChar == 'x' && lastChar == '0')
                {
                    SetBaseValue(MasterRule);
                    decimalPoint = char.ToString(descriptor[1]);
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

            // finally, if the rule body begins with an apostrophe, strip it off
            // (this is generally used to put whitespace at the beginning of
            // a rule's rule text)

            // return the description with all the stuff we've just waded through
            // stripped off the front.  It now contains just the rule body.
            return RemoveLeadingApostrophe(description);
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
        /// and <paramref name="predecessor"/>.<see cref="NumberFormatRule.BaseValue"/> is <see cref="NumberFormatRule.NegativeNumberRule"/>.
        /// <para/>
        /// -or-
        /// <para/>
        /// A substitution within <paramref name="ruleText"/> starts with '&gt;'
        /// and <paramref name="owner"/>.<see cref="NumberFormatRuleSet.IsFractionSet"/> is <c>true</c>.
        /// <para/>
        /// -or-
        /// <para/>
        /// A substitution within <paramref name="ruleText"/> starts with a <see cref="char"/> other than '&lt;', '&gt;', or '='.
        /// </exception>
        private void ExtractSubstitutions(NumberFormatRuleSet owner,
                                        ReadOnlySpan<char> ruleText,
                                        NumberFormatRule? predecessor)
        {
            sub1 = ExtractSubstitution(owner, ruleText, predecessor, out ReadOnlySpan<char> prefix1, out ReadOnlySpan<char> suffix1);
            if (sub1 is not null)
            {
                sub2 = ExtractSubstitution(owner, suffix1, predecessor, out ReadOnlySpan<char> prefix2, out ReadOnlySpan<char> suffix2);
                if (sub2 is not null)
                {
                    // We have a 3 parts to concatenate to a string
                    this.ruleText = string.Concat(prefix1, prefix2, suffix2);
                }
                else
                {
                    // We have 2 parts to concatenate to a string
                    this.ruleText = string.Concat(prefix1, suffix1);
                }
            }
            else
            {
                // Use the string as is - there were no substitutions to remove
                this.ruleText = new string(ruleText);
            }
            ExtractPluralRules(this.ruleText);
        }

        /// <summary>
        /// Extracts the plural rules from the <paramref name="ruleText"/> and sets the
        /// <see cref="pluralMessagePattern"/>, <see cref="pluralRulesText"/>, and <see cref="pluralType"/>
        /// fields, if a plural rule was found.
        /// </summary>
        /// <param name="ruleText">The rule text.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="ruleText"/> has a plural rule that does not have a type.
        /// <para/>
        /// -or-
        /// <para/>
        /// <paramref name="ruleText"/> has a plural rule that has an unknown type (i.e. not "cardinal" or "ordinal").
        /// </exception>
        private void ExtractPluralRules(ReadOnlySpan<char> ruleText)
        {
            int pluralRuleStart = ruleText.IndexOf("$(", StringComparison.Ordinal);
            int pluralRuleEnd = pluralRuleStart >= 0 ? ruleText.Slice(pluralRuleStart).IndexOf(")$", StringComparison.Ordinal) : -1;
            if (pluralRuleEnd >= 0)
            {
                int endType = ruleText.Slice(pluralRuleStart).IndexOf(',');
                if (endType < 0)
                {
                    throw new ArgumentException(string.Concat("Rule \"", ruleText, "\" does not have a defined type"));
                }
                ReadOnlySpan<char> type = ruleText.Slice(pluralRuleStart + 2, endType - (pluralRuleStart + 2)); // ICU4N: Corrected 2nd parameter
                if (type.Equals("cardinal", StringComparison.Ordinal))
                {
                    this.pluralType = PluralType.Cardinal;
                }
                else if (type.Equals("ordinal", StringComparison.Ordinal))
                {
                    this.pluralType = PluralType.Ordinal;
                }
                else
                {
                    throw new ArgumentException(string.Concat(type, " is an unknown type"));
                }
                ReadOnlySpan<char> pluralRuleText = ruleText.Slice(endType + 1, pluralRuleEnd - (endType + 1)); // ICU4N: Corrected 2nd parameter
                //this.pluralRules = PluralRules.ParseDescription(pluralRuleText);
                this.pluralRulesText = new string(pluralRuleText);
                this.pluralMessagePattern = new MessagePattern().ParsePluralStyle(pluralRulesText);

                //rulePatternFormat = formatter.CreatePluralFormat(pluralType,
                //        ruleText.Substring(endType + 1, pluralRuleEnd - (endType + 1))); // ICU4N: Corrected 2nd parameter
            }
        }

        /// <summary>
        /// Searches the rule's rule text for the first substitution token,
        /// creates a substitution based on it, and removes the token from
        /// the rule's rule text.
        /// </summary>
        /// <param name="owner">The rule set containing this rule.</param>
        /// <param name="ruleText">The rule text.</param>
        /// <param name="predecessor">The rule preceding this one in the rule set's
        /// rule list.</param>
        /// <param name="prefix">Upon return, contains a <see cref="ReadOnlySpan{T}"/> with the text prior
        /// to the substitution rule definition that was parsed.</param>
        /// <param name="suffix">Upon return, contains a <see cref="ReadOnlySpan{T}"/> with the text after
        /// the substitution rule definition that was parsed.</param>
        /// <returns>The newly-created substitution or <c>null</c>.</returns>
        /// <exception cref="ArgumentException">
        /// A substitution within <see name="ruleText"/> starts with '&lt;'
        /// and <paramref name="predecessor"/>.<see cref="NumberFormatRule.BaseValue"/> is <see cref="NumberFormatRule.NegativeNumberRule"/>.
        /// <para/>
        /// -or-
        /// <para/>
        /// A substitution within <see name="ruleText"/> starts with '&gt;'
        /// and <paramref name="owner"/>.<see cref="NumberFormatRuleSet.IsFractionSet"/> is <c>true</c>.
        /// <para/>
        /// -or-
        /// <para/>
        /// A substitution within <see name="ruleText"/> starts with a <see cref="char"/> other than '&lt;', '&gt;', or '='.
        /// </exception>
        private NumberFormatSubstitution? ExtractSubstitution(NumberFormatRuleSet owner,
                                        ReadOnlySpan<char> ruleText,
                                        NumberFormatRule? predecessor,
                                        out ReadOnlySpan<char> prefix,
                                        out ReadOnlySpan<char> suffix)
        {
            int subStart;
            int subEnd;

            // search the rule's rule text for the first two characters of
            // a substitution token
            subStart = IndexOfAnyRulePrefix(ruleText);

            // if we didn't find one, create a null substitution positioned
            // at the end of the rule text
            if (subStart == -1)
            {
                prefix = ReadOnlySpan<char>.Empty;
                suffix = ruleText;
                return null;
            }

            // special-case the ">>>" token, since searching for the > at the
            // end will actually find the > in the middle
            if (ruleText.Slice(subStart).StartsWith(">>>", StringComparison.Ordinal))
            {
                subEnd = subStart + 2;
            }
            else
            {
                // otherwise the substitution token ends with the same character
                // it began with
                char c = ruleText[subStart];
                subEnd = ruleText.Slice(subStart + 1).IndexOf(c);
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
                prefix = ReadOnlySpan<char>.Empty;
                suffix = ruleText;
                return null;
            }

            // Return the text prior and the text after the substitution so we can concat it later.
            prefix = subStart == 0 ? ReadOnlySpan<char>.Empty : ruleText.Slice(0, subStart);
            suffix = ruleText.Slice(subEnd + 1);

            // if we get here, we have a real substitution token (or at least
            // some text bounded by substitution token characters).  Use
            // MakeSubstitution() to create the right kind of substitution
            return NumberFormatSubstitution.MakeSubstitution(subStart, this, predecessor, owner,
                    this.numberFormatRules, ruleText.Slice(subStart, (subEnd + 1) - subStart)); // ICU4N: Corrected 2nd parameter
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
            // NumberFormatRule.ParseRuleDescriptor() )
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
        private short GetExpectedExponent() // ICU4N: Does this really belong under "construction"?
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

        /// <summary>
        /// Searches the rule's rule text for any of the specified strings.
        /// </summary>
        /// <param name="ruleText">The rule text.</param>
        /// <returns>The index of the first match in the rule's rule text
        /// (i.e., the first substring in the rule's rule text that matches
        /// _any_ of the strings in "strings").  If none of the strings in
        /// "strings" is found in the rule's rule text, returns -1.</returns>
        private static int IndexOfAnyRulePrefix(ReadOnlySpan<char> ruleText)
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
        public override bool Equals(object? that)
        {
            if (that is NumberFormatRule that2)
            {
                return baseValue == that2.baseValue
                    && radix == that2.radix
                    && exponent == that2.exponent
                    && ruleText!.Equals(that2.ruleText!, StringComparison.Ordinal)
                    && Equals(sub1, that2.sub1)
                    && Equals(sub2, that2.sub2);
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
                result.Append('x').Append(decimalPoint.Length == 0 ? "." : decimalPoint).Append("x: ");
            }
            else if (baseValue == ProperFractionRule)
            {
                result.Append('0').Append(decimalPoint.Length == 0 ? "." : decimalPoint).Append("x: ");
            }
            else if (baseValue == MasterRule)
            {
                result.Append('x').Append(decimalPoint.Length == 0 ? "." : decimalPoint).Append("0: ");
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
            if (ruleText!.StartsWith(" ", StringComparison.Ordinal) && (sub1 is null || sub1.Pos != 0))
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
            result.Append(ruleTextCopy);

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
        public string DecimalPoint => decimalPoint; // ICU4N: Made this into a string instead of a char

        /// <summary>
        /// Gets the rule's base value.
        /// </summary>
        public long BaseValue => baseValue;

        /// <summary>
        /// Gets the rule's divisor (the value that controls the behavior
        /// of its substitutions).
        /// </summary>
        public long Divisor => Power(radix, exponent);

    }
#endif
}
