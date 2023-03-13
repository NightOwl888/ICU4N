using ICU4N.Text;
using System;
using System.Globalization;
using System.Text;

namespace ICU4N.Globalization
{
#if FEATURE_SPAN
    /// <summary>
    /// A class representing a single rule in a <see cref="INumberFormatRules"/>. A rule
    /// inserts its text into the result string and then passes control to its
    /// substitutions, which do the same thing.
    /// </summary>
    internal partial class NumberFormatRule
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
        private string ruleText = null;

        ///// <summary>
        ///// The rule's plural Format when defined. This is not a substitution
        ///// because it only works on the current baseValue. It's normally not used
        ///// due to the overhead.
        ///// </summary>
        //private PluralFormat rulePatternFormat = null;

        // ICU4N specific - we hold a reference to PluralRules instead of PluralFormat for now.
        // Once we figure out a way to format PluralFormat, this may need to be changed back.

        /// <summary>
        /// The rule's pluaral rules when defined. This is not a substitution
        /// because it only works on the current <see cref="baseValue"/>. It's normally not
        /// used due to the overhead.
        /// </summary>
        private PluralRules pluralRules = null;

        private string pluralRulesText = null;

        // ICU4N TODO: Use a MessagePattern instance instead of pluralRulesText to do the rule parsing during construction?

        /// <summary>
        /// The rule's first substitution (the one with the lower offset
        /// into the rule text)
        /// </summary>
        private NumberFormatSubstitution sub1 = null;

        /// <summary>
        /// The rule's second substitution (the one with the higher offset
        /// into the rule text)
        /// </summary>
        private NumberFormatSubstitution sub2 = null;

        /// <summary>
        /// The <see cref="INumberFormatRules"/> that owns this formatter.
        /// </summary>
        private readonly INumberFormatRules numberFormatRules; // ICU4N: This was a reference to RuleBasedNumberFormat in ICU4J

        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        // ICU4N TODO: Implementation


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
            if (that is NumberFormatRule that2)
            {
                return baseValue == that2.baseValue
                    && radix == that2.radix
                    && exponent == that2.exponent
                    && ruleText.Equals(that2.ruleText, StringComparison.Ordinal)
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
