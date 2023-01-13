using J2N.Numerics;
using System;
using System.Text;
using Long = J2N.Numerics.Int64;
using Double = J2N.Numerics.Double;
using J2N.Text;
using ICU4N.Numerics;

namespace ICU4N.Text
{
    //===================================================================
    // NFSubstitution (abstract base class)
    //===================================================================

    /// <summary>
    /// An abstract class defining protocol for substitutions.  A substitution
    /// is a section of a rule that inserts text into the rule's rule text
    /// based on some part of the number being formatted.
    /// </summary>
    /// <author>Richard Gillam</author>
    internal abstract class NFSubstitution
    {
        //-----------------------------------------------------------------------
        // data members
        //-----------------------------------------------------------------------

        /**
         * The substitution's position in the rule text of the rule that owns it
         */
        internal readonly int pos;

        /**
         * The rule set this substitution uses to format its result, or null.
         * (Either this or numberFormat has to be non-null.)
         */
        internal readonly NFRuleSet ruleSet;

        /**
         * The DecimalFormat this substitution uses to format its result,
         * or null.  (Either this or ruleSet has to be non-null.)
         */
        internal readonly DecimalFormat numberFormat;

        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        /**
         * Parses the description, creates the right kind of substitution,
         * and initializes it based on the description.
         * @param pos The substitution's position in the rule text of the
         * rule that owns it.
         * @param rule The rule containing this substitution
         * @param rulePredecessor The rule preceding the one that contains
         * this substitution in the rule set's rule list (this is used
         * only for >>> substitutions).
         * @param ruleSet The rule set containing the rule containing this
         * substitution
         * @param formatter The RuleBasedNumberFormat that ultimately owns
         * this substitution
         * @param description The description to parse to build the substitution
         * (this is just the substring of the rule's description containing
         * the substitution token itself)
         * @return A new substitution constructed according to the description
         */
        public static NFSubstitution MakeSubstitution(int pos,
                                                      NFRule rule,
                                                      NFRule rulePredecessor,
                                                      NFRuleSet ruleSet,
                                                      RuleBasedNumberFormat formatter,
                                                      String description)
        {
            // if the description is empty, return a NullSubstitution
            if (description.Length == 0)
            {
                return null;
            }

            switch (description[0])
            {
                case '<':
                    if (rule.BaseValue == NFRule.NEGATIVE_NUMBER_RULE)
                    {
                        // throw an exception if the rule is a negative number rule
                        ///CLOVER:OFF
                        // If you look at the call hierarchy of this method, the rule would
                        // never be directly modified by the user and therefore makes the
                        // following pointless unless the user changes the ruleset.
                        throw new ArgumentException("<< not allowed in negative-number rule");
                        ///CLOVER:ON
                    }
                    else if (rule.BaseValue == NFRule.IMPROPER_FRACTION_RULE
                             || rule.BaseValue == NFRule.PROPER_FRACTION_RULE
                             || rule.BaseValue == NFRule.MASTER_RULE)
                    {
                        // if the rule is a fraction rule, return an IntegralPartSubstitution
                        return new IntegralPartSubstitution(pos, ruleSet, description);
                    }
                    else if (ruleSet.IsFractionSet)
                    {
                        // if the rule set containing the rule is a fraction
                        // rule set, return a NumeratorSubstitution
                        return new NumeratorSubstitution(pos, rule.BaseValue,
                                                         formatter.DefaultRuleSet, description);
                    }
                    else
                    {
                        // otherwise, return a MultiplierSubstitution
                        return new MultiplierSubstitution(pos, rule, ruleSet,
                                                          description);
                    }

                case '>':
                    if (rule.BaseValue == NFRule.NEGATIVE_NUMBER_RULE)
                    {
                        // if the rule is a negative-number rule, return
                        // an AbsoluteValueSubstitution
                        return new AbsoluteValueSubstitution(pos, ruleSet, description);
                    }
                    else if (rule.BaseValue == NFRule.IMPROPER_FRACTION_RULE
                             || rule.BaseValue == NFRule.PROPER_FRACTION_RULE
                             || rule.BaseValue == NFRule.MASTER_RULE)
                    {
                        // if the rule is a fraction rule, return a
                        // FractionalPartSubstitution
                        return new FractionalPartSubstitution(pos, ruleSet, description);
                    }
                    else if (ruleSet.IsFractionSet)
                    {
                        // if the rule set owning the rule is a fraction rule set,
                        // throw an exception
                        ///CLOVER:OFF
                        // If you look at the call hierarchy of this method, the rule would
                        // never be directly modified by the user and therefore makes the
                        // following pointless unless the user changes the ruleset.
                        throw new ArgumentException(">> not allowed in fraction rule set");
                        ///CLOVER:ON
                    }
                    else
                    {
                        // otherwise, return a ModulusSubstitution
                        return new ModulusSubstitution(pos, rule, rulePredecessor,
                                                       ruleSet, description);
                    }
                case '=':
                    return new SameValueSubstitution(pos, ruleSet, description);
                default:
                    // and if it's anything else, throw an exception
                    ///CLOVER:OFF
                    // If you look at the call hierarchy of this method, the rule would
                    // never be directly modified by the user and therefore makes the
                    // following pointless unless the user changes the ruleset.
                    throw new ArgumentException("Illegal substitution character");
                    ///CLOVER:ON
            }
        }

        /**
         * Base constructor for substitutions.  This constructor sets up the
         * fields which are common to all substitutions.
         * @param pos The substitution's position in the owning rule's rule
         * text
         * @param ruleSet The rule set that owns this substitution
         * @param description The substitution descriptor (i.e., the text
         * inside the token characters)
         */
        private protected NFSubstitution(int pos, // ICU4N: Changed from internal to private protected
                       NFRuleSet ruleSet,
                       string description)
        {
            // initialize the substitution's position in its parent rule
            this.pos = pos;
            int descriptionLen = description.Length;

            // the description should begin and end with the same character.
            // If it doesn't that's a syntax error.  Otherwise,
            // makeSubstitution() was the only thing that needed to know
            // about these characters, so strip them off
            if (descriptionLen >= 2 && description[0] == description[descriptionLen - 1])
            {
                description = description.Substring(1, descriptionLen - 2); // ICU4N: Corrected 2nd parameter
            }
            else if (descriptionLen != 0)
            {
                throw new ArgumentException("Illegal substitution syntax");
            }

            // if the description was just two paired token characters
            // (i.e., "<<" or ">>"), it uses the rule set it belongs to to
            // format its result
            if (description.Length == 0)
            {
                this.ruleSet = ruleSet;
                this.numberFormat = null;
            }
            else if (description[0] == '%')
            {
                // if the description contains a rule set name, that's the rule
                // set we use to format the result: get a reference to the
                // names rule set
                this.ruleSet = ruleSet.owner.FindRuleSet(description);
                this.numberFormat = null;
            }
            else if (description[0] == '#' || description[0] == '0')
            {
                // if the description begins with 0 or #, treat it as a
                // DecimalFormat pattern, and initialize a DecimalFormat with
                // that pattern (then set it to use the DecimalFormatSymbols
                // belonging to our formatter)
                this.ruleSet = null;
                this.numberFormat = (DecimalFormat)ruleSet.owner.DecimalFormat.Clone();
                this.numberFormat.ApplyPattern(description);
            }
            else if (description[0] == '>')
            {
                // if the description is ">>>", this substitution bypasses the
                // usual rule-search process and always uses the rule that precedes
                // it in its own rule set's rule list (this is used for place-value
                // notations: formats where you want to see a particular part of
                // a number even when it's 0)
                this.ruleSet = ruleSet; // was null, thai rules added to control space
                this.numberFormat = null;
            }
            else
            {
                // and of the description is none of these things, it's a syntax error
                throw new ArgumentException("Illegal substitution syntax");
            }
        }

        /**
         * Set's the substitution's divisor.  Used by NFRule.setBaseValue().
         * A no-op for all substitutions except multiplier and modulus
         * substitutions.
         * @param radix The radix of the divisor
         * @param exponent The exponent of the divisor
         */
        public virtual void SetDivisor(int radix, short exponent)
        {
            // a no-op for all substitutions except multiplier and modulus substitutions
        }

        //-----------------------------------------------------------------------
        // boilerplate
        //-----------------------------------------------------------------------

        /**
         * Compares two substitutions for equality
         * @param that The substitution to compare this one to
         * @return true if the two substitutions are functionally equivalent
         */
        public override bool Equals(object that)
        {
            // compare class and all of the fields all substitutions have
            // in common
            if (that == null)
            {
                return false;
            }
            if (this == that)
            {
                return true;
            }
            if (this.GetType() == that.GetType())
            {
                NFSubstitution that2 = (NFSubstitution)that;

                return pos == that2.pos
                    && (ruleSet != null || that2.ruleSet == null) // can't compare tree structure, no .equals or recurse
                    && (numberFormat == null ? (that2.numberFormat == null) : numberFormat.Equals(that2.numberFormat));
            }
            return false;
        }

        public override int GetHashCode()
        {
            //assert false : "hashCode not designed";
            return 42;
        }

        /**
         * Returns a textual description of the substitution
         * @return A textual description of the substitution.  This might
         * not be identical to the description it was created from, but
         * it'll produce the same result.
         */

        public override string ToString()
        {
            // use TokenChar to get the character at the beginning and
            // end of the substitution token.  In between them will go
            // either the name of the rule set it uses, or the pattern of
            // the DecimalFormat it uses
            if (ruleSet != null)
            {
                return TokenChar + ruleSet.Name + TokenChar;
            }
            else
            {
                return TokenChar + numberFormat.ToPattern() + TokenChar;
            }
        }

        //-----------------------------------------------------------------------
        // formatting
        //-----------------------------------------------------------------------

        private const long MAX_INT64_IN_DOUBLE = 0x1FFFFFFFFFFFFFL;

        /**
         * Performs a mathematical operation on the number, formats it using
         * either ruleSet or decimalFormat, and inserts the result into
         * toInsertInto.
         * @param number The number being formatted.
         * @param toInsertInto The string we insert the result into
         * @param position The position in toInsertInto where the owning rule's
         * rule text begins (this value is added to this substitution's
         * position to determine exactly where to insert the new text)
         */
        public virtual void DoSubstitution(long number, StringBuilder toInsertInto, int position, int recursionCount)
        {
            if (ruleSet != null)
            {
                // Perform a transformation on the number that is dependent
                // on the type of substitution this is, then just call its
                // rule set's format() method to format the result
                long numberToFormat = TransformNumber(number);

                ruleSet.Format(numberToFormat, toInsertInto, position + pos, recursionCount);
            }
            else
            {
                if (number <= MAX_INT64_IN_DOUBLE)
                {
                    // or perform the transformation on the number (preserving
                    // the result's fractional part if the formatter it set
                    // to show it), then use that formatter's format() method
                    // to format the result
                    double numberToFormat = TransformNumber((double)number);
                    if (numberFormat.MaximumFractionDigits == 0)
                    {
                        numberToFormat = Math.Floor(numberToFormat);
                    }

                    toInsertInto.Insert(position + pos, numberFormat.Format(numberToFormat));
                }
                else
                {
                    // We have gone beyond double precision. Something has to give.
                    // We're favoring accuracy of the large number over potential rules
                    // that round like a CompactDecimalFormat, which is not a common use case.
                    //
                    // Perform a transformation on the number that is dependent
                    // on the type of substitution this is, then just call its
                    // rule set's format() method to format the result
                    long numberToFormat = TransformNumber(number);
                    toInsertInto.Insert(position + pos, numberFormat.Format(numberToFormat));
                }
            }
        }

        /**
         * Performs a mathematical operation on the number, formats it using
         * either ruleSet or decimalFormat, and inserts the result into
         * toInsertInto.
         * @param number The number being formatted.
         * @param toInsertInto The string we insert the result into
         * @param position The position in toInsertInto where the owning rule's
         * rule text begins (this value is added to this substitution's
         * position to determine exactly where to insert the new text)
         */
        public virtual void DoSubstitution(double number, StringBuilder toInsertInto, int position, int recursionCount)
        {
            // perform a transformation on the number being formatted that
            // is dependent on the type of substitution this is
            double numberToFormat = TransformNumber(number);

            if (double.IsInfinity(numberToFormat))
            {
                // This is probably a minus rule. Combine it with an infinite rule.
                NFRule infiniteRule = ruleSet.FindRule(double.PositiveInfinity);
                infiniteRule.DoFormat(numberToFormat, toInsertInto, position + pos, recursionCount);
                return;
            }

            // if the result is an integer, from here on out we work in integer
            // space (saving time and memory and preserving accuracy)
            if (numberToFormat == Math.Floor(numberToFormat) && ruleSet != null) // ICU4N TODO: exact equality for double (bad)
            {
                ruleSet.Format((long)numberToFormat, toInsertInto, position + pos, recursionCount);

                // if the result isn't an integer, then call either our rule set's
                // format() method or our DecimalFormat's format() method to
                // format the result
            }
            else
            {
                if (ruleSet != null)
                {
                    ruleSet.Format(numberToFormat, toInsertInto, position + pos, recursionCount);
                }
                else
                {
                    toInsertInto.Insert(position + this.pos, numberFormat.Format(numberToFormat));
                }
            }
        }

        /**
         * Subclasses override this function to perform some kind of
         * mathematical operation on the number.  The result of this operation
         * is formatted using the rule set or DecimalFormat that this
         * substitution refers to, and the result is inserted into the result
         * string.
         * @param number The number being formatted
         * @return The result of performing the opreration on the number
         */
        public abstract long TransformNumber(long number);

        /**
         * Subclasses override this function to perform some kind of
         * mathematical operation on the number.  The result of this operation
         * is formatted using the rule set or DecimalFormat that this
         * substitution refers to, and the result is inserted into the result
         * string.
         * @param number The number being formatted
         * @return The result of performing the opreration on the number
         */
        public abstract double TransformNumber(double number);

        //-----------------------------------------------------------------------
        // parsing
        //-----------------------------------------------------------------------

        /**
         * Parses a string using the rule set or DecimalFormat belonging
         * to this substitution.  If there's a match, a mathematical
         * operation (the inverse of the one used in formatting) is
         * performed on the result of the parse and the value passed in
         * and returned as the result.  The parse position is updated to
         * point to the first unmatched character in the string.
         * @param text The string to parse
         * @param parsePosition On entry, ignored, but assumed to be 0.
         * On exit, this is updated to point to the first unmatched
         * character (or 0 if the substitution didn't match)
         * @param baseValue A partial parse result that should be
         * combined with the result of this parse
         * @param upperBound When searching the rule set for a rule
         * matching the string passed in, only rules with base values
         * lower than this are considered
         * @param lenientParse If true and matching against rules fails,
         * the substitution will also try matching the text against
         * numerals using a default-constructed NumberFormat.  If false,
         * no extra work is done.  (This value is false whenever the
         * formatter isn't in lenient-parse mode, but is also false
         * under some conditions even when the formatter _is_ in
         * lenient-parse mode.)
         * @return If there's a match, this is the result of composing
         * baseValue with whatever was returned from matching the
         * characters.  This will be either a Long or a Double.  If there's
         * no match this is new Long(0) (not null), and parsePosition
         * is left unchanged.
         */
        public virtual Number DoParse(string text, ParsePosition parsePosition, double baseValue,
                              double upperBound, bool lenientParse)
        {
            Number tempResult;

            // figure out the highest base value a rule can have and match
            // the text being parsed (this varies according to the type of
            // substitutions: multiplier, modulus, and numerator substitutions
            // restrict the search to rules with base values lower than their
            // own; same-value substitutions leave the upper bound wherever
            // it was, and the others allow any rule to match
            upperBound = CalcUpperBound(upperBound);

            // use our rule set to parse the text.  If that fails and
            // lenient parsing is enabled (this is always false if the
            // formatter's lenient-parsing mode is off, but it may also
            // be false even when the formatter's lenient-parse mode is
            // on), then also try parsing the text using a default-
            // constructed NumberFormat
            if (ruleSet != null)
            {
                tempResult = ruleSet.Parse(text, parsePosition, upperBound);
                if (lenientParse && !ruleSet.IsFractionSet && parsePosition.Index == 0)
                {
                    tempResult = ruleSet.owner.DecimalFormat.Parse(text, parsePosition);
                }

                // ...or use our DecimalFormat to parse the text
            }
            else
            {
                tempResult = numberFormat.Parse(text, parsePosition);
            }

            // if the parse was successful, we've already advanced the caller's
            // parse position (this is the one function that doesn't have one
            // of its own).  Derive a parse result and return it as a Long,
            // if possible, or a Double
            if (parsePosition.Index != 0)
            {
                double result = tempResult.ToDouble();

                // composeRuleValue() produces a full parse result from
                // the partial parse result passed to this function from
                // the caller (this is either the owning rule's base value
                // or the partial result obtained from composing the
                // owning rule's base value with its other substitution's
                // parse result) and the partial parse result obtained by
                // matching the substitution (which will be the same value
                // the caller would get by parsing just this part of the
                // text with RuleBasedNumberFormat.parse() ).  How the two
                // values are used to derive the full parse result depends
                // on the types of substitutions: For a regular rule, the
                // ultimate result is its multiplier substitution's result
                // times the rule's divisor (or the rule's base value) plus
                // the modulus substitution's result (which will actually
                // supersede part of the rule's base value).  For a negative-
                // number rule, the result is the negative of its substitution's
                // result.  For a fraction rule, it's the sum of its two
                // substitution results.  For a rule in a fraction rule set,
                // it's the numerator substitution's result divided by
                // the rule's base value.  Results from same-value substitutions
                // propagate back upward, and null substitutions don't affect
                // the result.
                result = ComposeRuleValue(result, baseValue);
                if (result == (long)result)
                {
                    return Long.GetInstance((long)result);
                }
                else
                {
                    return Double.GetInstance(result);
                }

                // if the parse was UNsuccessful, return 0
            }
            else
            {
                return tempResult;
            }
        }

        /**
         * Derives a new value from the two values passed in.  The two values
         * are typically either the base values of two rules (the one containing
         * the substitution and the one matching the substitution) or partial
         * parse results derived in some other way.  The operation is generally
         * the inverse of the operation performed by transformNumber().
         * @param newRuleValue The value produced by matching this substitution
         * @param oldRuleValue The value that was passed to the substitution
         * by the rule that owns it
         * @return A third value derived from the other two, representing a
         * partial parse result
         */
        public abstract double ComposeRuleValue(double newRuleValue, double oldRuleValue);

        /**
         * Calculates an upper bound when searching for a rule that matches
         * this substitution.  Rules with base values greater than or equal
         * to upperBound are not considered.
         * @param oldUpperBound The current upper-bound setting.  The new
         * upper bound can't be any higher.
         */
        public abstract double CalcUpperBound(double oldUpperBound);

        //-----------------------------------------------------------------------
        // simple accessors
        //-----------------------------------------------------------------------

        /**
         * Returns the substitution's position in the rule that owns it.
         * @return The substitution's position in the rule that owns it.
         */
        public int Pos => pos;

        /**
         * Returns the character used in the textual representation of
         * substitutions of this type.  Used by toString().
         * @return This substitution's token character.
         */
        private protected abstract char TokenChar { get; }

        /**
         * Returns true if this is a modulus substitution.  (We didn't do this
         * with instanceof partially because it causes source files to
         * proliferate and partially because we have to port this to C++.)
         * @return true if this object is an instance of ModulusSubstitution
         */
        public virtual bool IsModulusSubstitution => false;


        public virtual void SetDecimalFormatSymbols(DecimalFormatSymbols newSymbols)
        {
            numberFormat?.SetDecimalFormatSymbols(newSymbols);
        }
    }

    //===================================================================
    // SameValueSubstitution
    //===================================================================

    /**
     * A substitution that passes the value passed to it through unchanged.
     * Represented by == in rule descriptions.
     */
    internal class SameValueSubstitution : NFSubstitution
    {
        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        /**
         * Constructs a SameValueSubstution.  This function just uses the
         * superclass constructor, but it performs a check that this
         * substitution doesn't call the rule set that owns it, since that
         * would lead to infinite recursion.
         */
        internal SameValueSubstitution(int pos,
                          NFRuleSet ruleSet,
                          string description)
            : base(pos, ruleSet, description)
        {
            if (description.Equals("=="))
            {
                throw new ArgumentException("== is not a legal token");
            }
        }

        //-----------------------------------------------------------------------
        // formatting
        //-----------------------------------------------------------------------

        /**
         * Returns "number" unchanged.
         * @return "number"
         */
        public override long TransformNumber(long number)
        {
            return number;
        }

        /**
         * Returns "number" unchanged.
         * @return "number"
         */
        public override double TransformNumber(double number)
        {
            return number;
        }

        //-----------------------------------------------------------------------
        // parsing
        //-----------------------------------------------------------------------

        /**
         * Returns newRuleValue and ignores oldRuleValue. (The value we got
         * matching the substitution supersedes the value of the rule
         * that owns the substitution.)
         * @param newRuleValue The value resulting from matching the substitution
         * @param oldRuleValue The value of the rule containing the
         * substitution.
         * @return newRuleValue
         */
        public override double ComposeRuleValue(double newRuleValue, double oldRuleValue)
        {
            return newRuleValue;
        }

        /**
         * SameValueSubstitution doesn't change the upper bound.
         * @param oldUpperBound The current upper bound.
         * @return oldUpperBound
         */
        public override double CalcUpperBound(double oldUpperBound)
        {
            return oldUpperBound;
        }

        //-----------------------------------------------------------------------
        // simple accessor
        //-----------------------------------------------------------------------

        /**
         * The token character for a SameValueSubstitution is =.
         * @return '='
         */
        private protected override char TokenChar => '=';
    }

    //===================================================================
    // MultiplierSubstitution
    //===================================================================

    /**
     * A substitution that divides the number being formatted by the rule's
     * divisor and formats the quotient.  Represented by &lt;&lt; in normal
     * rules.
     */
    internal class MultiplierSubstitution : NFSubstitution
    {
        //-----------------------------------------------------------------------
        // data members
        //-----------------------------------------------------------------------

        /**
         * The divisor of the rule that owns this substitution.
         */
        private long divisor;

        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        /**
         * Constructs a MultiplierSubstitution.  This uses the superclass
         * constructor to initialize most members, but this substitution
         * also maintains its own copy of its rule's divisor.
         * @param pos The substitution's position in its rule's rule text
         * @param rule The rule that owns this substitution
         * @param ruleSet The ruleSet this substitution uses to format its result
         * @param description The description describing this substitution
         */
        internal MultiplierSubstitution(int pos,
                               NFRule rule,
                               NFRuleSet ruleSet,
                               string description)
            : base(pos, ruleSet, description)
        {

            // the owning rule's divisor affects the behavior of this
            // substitution.  Rather than keeping a back-pointer to the
            // rule, we keep a copy of the divisor
            this.divisor = rule.Divisor;

            if (divisor == 0)
            { // this will cause recursion
                throw new InvalidOperationException("Substitution with divisor 0 " + description.Substring(0, pos) + // ICU4N: Checked 2nd parameter
                             " | " + description.Substring(pos));
            }
        }

        /**
         * Sets the substitution's divisor based on the values passed in.
         * @param radix The radix of the divisor.
         * @param exponent The exponent of the divisor.
         */
        public override void SetDivisor(int radix, short exponent)
        {
            divisor = NFRule.Power(radix, exponent);

            if (divisor == 0)
            {
                throw new InvalidOperationException("Substitution with divisor 0");
            }
        }

        //-----------------------------------------------------------------------
        // boilerplate
        //-----------------------------------------------------------------------

        /**
         * Augments the superclass's equals() function by comparing divisors.
         * @param that The other substitution
         * @return true if the two substitutions are functionally equal
         */
        public override bool Equals(object that)
        {
            return base.Equals(that) && divisor == ((MultiplierSubstitution)that).divisor;
        }

        public override int GetHashCode() // ICU4N specific - need to override hash code
        {
            return base.GetHashCode() ^ divisor.GetHashCode();
        }

        //-----------------------------------------------------------------------
        // formatting
        //-----------------------------------------------------------------------

        /**
         * Divides the number by the rule's divisor and returns the quotient.
         * @param number The number being formatted.
         * @return "number" divided by the rule's divisor
         */
        public override long TransformNumber(long number)
        {
            //return (long)Math.Floor((double)(number / divisor));
            // ICU4N TODO: Tests for this logic (need to find some edge cases that cause the number to flip in Java)
            // source: https://stackoverflow.com/a/28060018
            return (number / divisor - Convert.ToInt32(((number < 0) ^ (divisor < 0)) && (number % divisor != 0)));
        }

        /**
         * Divides the number by the rule's divisor and returns the quotient.
         * This is an integral quotient if we're filling in the substitution
         * using another rule set, but it's the full quotient (integral and
         * fractional parts) if we're filling in the substitution using
         * a DecimalFormat.  (This allows things such as "1.2 million".)
         * @param number The number being formatted
         * @return "number" divided by the rule's divisor
         */
        public override double TransformNumber(double number)
        {
            if (ruleSet == null)
            {
                return number / divisor;
            }
            else
            {
                return Math.Floor(number / divisor);
            }
        }

        //-----------------------------------------------------------------------
        // parsing
        //-----------------------------------------------------------------------

        /**
         * Returns newRuleValue times the divisor.  Ignores oldRuleValue.
         * (The result of matching a << substitution supersedes the base
         * value of the rule that contains it.)
         * @param newRuleValue The result of matching the substitution
         * @param oldRuleValue The base value of the rule containing the
         * substitution
         * @return newRuleValue * divisor
         */
        public override double ComposeRuleValue(double newRuleValue, double oldRuleValue)
        {
            return newRuleValue * divisor;
        }

        /**
         * Sets the upper bound down to the rule's divisor.
         * @param oldUpperBound Ignored.
         * @return The rule's divisor.
         */
        public override double CalcUpperBound(double oldUpperBound)
        {
            return divisor;
        }

        //-----------------------------------------------------------------------
        // simple accessor
        //-----------------------------------------------------------------------

        /**
         * The token character for a multiplier substitution is &lt;.
         * @return '&lt;'
         */
        private protected override char TokenChar => '<';
    }

    //===================================================================
    // ModulusSubstitution
    //===================================================================

    /**
     * A substitution that divides the number being formatted by the its rule's
     * divisor and formats the remainder.  Represented by "&gt;&gt;" in a
     * regular rule.
     */
    internal class ModulusSubstitution : NFSubstitution
    {
        //-----------------------------------------------------------------------
        // data members
        //-----------------------------------------------------------------------

        /**
         * The divisor of the rule owning this substitution
         */
        private long divisor;

        /**
         * If this is a &gt;&gt;&gt; substitution, the rule to use to format
         * the substitution value.  Otherwise, null.
         */
        private readonly NFRule ruleToUse;

        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        /**
         * Constructs a ModulusSubstitution.  In addition to the inherited
         * members, a ModulusSubstitution keeps track of the divisor of the
         * rule that owns it, and may also keep a reference to the rule
         * that precedes the rule containing this substitution in the rule
         * set's rule list.
         * @param pos The substitution's position in its rule's rule text
         * @param rule The rule that owns this substitution
         * @param rulePredecessor The rule that precedes this substitution's
         * rule in its rule set's rule list
         * @param description The description for this substitution
         */
        internal ModulusSubstitution(int pos,
                            NFRule rule,
                            NFRule rulePredecessor,
                            NFRuleSet ruleSet,
                            string description)
                        : base(pos, ruleSet, description)
        {

            // the owning rule's divisor controls the behavior of this
            // substitution: rather than keeping a backpointer to the rule,
            // we keep a copy of the divisor
            this.divisor = rule.Divisor;

            if (divisor == 0)
            { // this will cause recursion
                throw new InvalidOperationException("Substitution with bad divisor (" + divisor + ") " + description.Substring(0, pos) + // ICU4N: Checked 2nd parameter
                        " | " + description.Substring(pos));
            }

            // the >>> token doesn't alter how this substitution calculates the
            // values it uses for formatting and parsing, but it changes
            // what's done with that value after it's obtained: >>> short-
            // circuits the rule-search process and goes straight to the
            // specified rule to format the substitution value
            if (description.Equals(">>>"))
            {
                ruleToUse = rulePredecessor;
            }
            else
            {
                ruleToUse = null;
            }
        }

        /**
         * Makes the substitution's divisor conform to that of the rule
         * that owns it.  Used when the divisor is determined after creation.
         * @param radix The radix of the divisor.
         * @param exponent The exponent of the divisor.
         */
        public override void SetDivisor(int radix, short exponent)
        {
            divisor = NFRule.Power(radix, exponent);

            if (divisor == 0)
            { // this will cause recursion
                throw new InvalidOperationException("Substitution with bad divisor");
            }
        }

        //-----------------------------------------------------------------------
        // boilerplate
        //-----------------------------------------------------------------------

        /**
         * Augments the inherited equals() function by comparing divisors and
         * ruleToUse.
         * @param that The other substitution
         * @return true if the two substitutions are functionally equivalent
         */
        public override bool Equals(object that)
        {
            if (base.Equals(that))
            {
                ModulusSubstitution that2 = (ModulusSubstitution)that;

                return divisor == that2.divisor;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode() // ICU4N specific - need to override hash code
        {
            return base.GetHashCode() ^ divisor.GetHashCode();
        }

        //-----------------------------------------------------------------------
        // formatting
        //-----------------------------------------------------------------------

        /**
         * If this is a &gt;&gt;&gt; substitution, use ruleToUse to fill in
         * the substitution.  Otherwise, just use the superclass function.
         * @param number The number being formatted
         * @param toInsertInto The string to insert the result of this substitution
         * into
         * @param position The position of the rule text in toInsertInto
         */
        public override void DoSubstitution(long number, StringBuilder toInsertInto, int position, int recursionCount)
        {
            // if this isn't a >>> substitution, just use the inherited version
            // of this function (which uses either a rule set or a DecimalFormat
            // to format its substitution value)
            if (ruleToUse == null)
            {
                base.DoSubstitution(number, toInsertInto, position, recursionCount);

            }
            else
            {
                // a >>> substitution goes straight to a particular rule to
                // format the substitution value
                long numberToFormat = TransformNumber(number);
                ruleToUse.DoFormat(numberToFormat, toInsertInto, position + pos, recursionCount);
            }
        }

        /**
         * If this is a &gt;&gt;&gt; substitution, use ruleToUse to fill in
         * the substitution.  Otherwise, just use the superclass function.
         * @param number The number being formatted
         * @param toInsertInto The string to insert the result of this substitution
         * into
         * @param position The position of the rule text in toInsertInto
         */
        public override void DoSubstitution(double number, StringBuilder toInsertInto, int position, int recursionCount)
        {
            // if this isn't a >>> substitution, just use the inherited version
            // of this function (which uses either a rule set or a DecimalFormat
            // to format its substitution value)
            if (ruleToUse == null)
            {
                base.DoSubstitution(number, toInsertInto, position, recursionCount);

            }
            else
            {
                // a >>> substitution goes straight to a particular rule to
                // format the substitution value
                double numberToFormat = TransformNumber(number);
                ruleToUse.DoFormat(numberToFormat, toInsertInto, position + pos, recursionCount);
            }
        }

        /**
         * Divides the number being formatted by the rule's divisor and
         * returns the remainder.
         * @param number The number being formatted
         * @return "number" mod divisor
         */
        public override long TransformNumber(long number)
        {
            return number % divisor;
        }

        /**
         * Divides the number being formatted by the rule's divisor and
         * returns the remainder.
         * @param number The number being formatted
         * @return "number" mod divisor
         */
        public override double TransformNumber(double number)
        {
            return Math.Floor(number % divisor);
        }

        //-----------------------------------------------------------------------
        // parsing
        //-----------------------------------------------------------------------

        /**
         * If this is a &gt;&gt;&gt; substitution, match only against ruleToUse.
         * Otherwise, use the superclass function.
         * @param text The string to parse
         * @param parsePosition Ignored on entry, updated on exit to point to
         * the first unmatched character.
         * @param baseValue The partial parse result prior to calling this
         * routine.
         */
        public override Number DoParse(string text, ParsePosition parsePosition, double baseValue,
                              double upperBound, bool lenientParse)
        {
            // if this isn't a >>> substitution, we can just use the
            // inherited parse() routine to do the parsing
            if (ruleToUse == null)
            {
                return base.DoParse(text, parsePosition, baseValue, upperBound, lenientParse);

            }
            else
            {
                // but if it IS a >>> substitution, we have to do it here: we
                // use the specific rule's doParse() method, and then we have to
                // do some of the other work of NFRuleSet.parse()
                Number tempResult = ruleToUse.DoParse(text, parsePosition, false, upperBound);

                if (parsePosition.Index != 0)
                {
                    double result = tempResult.ToDouble();

                    result = ComposeRuleValue(result, baseValue);
                    if (result == (long)result)
                    {
                        return Long.GetInstance((long)result);
                    }
                    else
                    {
                        return Double.GetInstance(result);
                    }
                }
                else
                {
                    return tempResult;
                }
            }
        }

        /**
         * Returns the highest multiple of the rule's divisor that its less
         * than or equal to oldRuleValue, plus newRuleValue.  (The result
         * is the sum of the result of parsing the substitution plus the
         * base value of the rule containing the substitution, but if the
         * owning rule's base value isn't an even multiple of its divisor,
         * we have to round it down to a multiple of the divisor, or we
         * get unwanted digits in the result.)
         * @param newRuleValue The result of parsing the substitution
         * @param oldRuleValue The base value of the rule containing the
         * substitution
         */
        public override double ComposeRuleValue(double newRuleValue, double oldRuleValue)
        {
            return (oldRuleValue - (oldRuleValue % divisor)) + newRuleValue;
        }

        /**
         * Sets the upper bound down to the owning rule's divisor
         * @param oldUpperBound Ignored
         * @return The owning rule's divisor
         */
        public override double CalcUpperBound(double oldUpperBound)
        {
            return divisor;
        }

        //-----------------------------------------------------------------------
        // simple accessors
        //-----------------------------------------------------------------------

        /**
         * Returns true.  This _is_ a ModulusSubstitution.
         * @return true
         */
        public override bool IsModulusSubstitution => true;


        /**
         * The token character of a ModulusSubstitution is &gt;.
         * @return '&gt;'
         */
        private protected override char TokenChar => '>';
    }

    //===================================================================
    // IntegralPartSubstitution
    //===================================================================

    /**
     * A substitution that formats the number's integral part.  This is
     * represented by &lt;&lt; in a fraction rule.
     */
    internal class IntegralPartSubstitution : NFSubstitution
    {
        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        /**
         * Constructs an IntegralPartSubstitution.  This just calls
         * the superclass constructor.
         */
        internal IntegralPartSubstitution(int pos,
                                 NFRuleSet ruleSet,
                                 string description)
                    : base(pos, ruleSet, description)
        {
        }

        //-----------------------------------------------------------------------
        // formatting
        //-----------------------------------------------------------------------

        /**
         * Returns the number's integral part. (For a long, that's just the
         * number unchanged.)
         * @param number The number being formatted
         * @return "number" unchanged
         */
        public override long TransformNumber(long number)
        {
            return number;
        }

        /**
         * Returns the number's integral part.
         * @param number The integral part of the number being formatted
         * @return floor(number)
         */
        public override double TransformNumber(double number)
        {
            return Math.Floor(number);
        }

        //-----------------------------------------------------------------------
        // parsing
        //-----------------------------------------------------------------------

        /**
         * Returns the sum of the result of parsing the substitution and the
         * owning rule's base value.  (The owning rule, at best, has an
         * integral-part substitution and a fractional-part substitution,
         * so we can safely just add them.)
         * @param newRuleValue The result of matching the substitution
         * @param oldRuleValue The partial result of the parse prior to
         * calling this function
         * @return oldRuleValue + newRuleValue
         */
        public override double ComposeRuleValue(double newRuleValue, double oldRuleValue)
        {
            return newRuleValue + oldRuleValue;
        }

        /**
         * An IntegralPartSubstitution sets the upper bound back up so all
         * potentially matching rules are considered.
         * @param oldUpperBound Ignored
         * @return Double.MAX_VALUE
         */
        public override double CalcUpperBound(double oldUpperBound)
        {
            return double.MaxValue;
        }

        //-----------------------------------------------------------------------
        // simple accessor
        //-----------------------------------------------------------------------

        /**
         * An IntegralPartSubstitution's token character is &lt;
         * @return '&lt;'
         */
        private protected override char TokenChar => '<';
    }

    //===================================================================
    // FractionalPartSubstitution
    //===================================================================

    /**
     * A substitution that formats the fractional part of a number.  This is
     * represented by &gt;&gt; in a fraction rule.
     */
    internal class FractionalPartSubstitution : NFSubstitution
    {
        //-----------------------------------------------------------------------
        // data members
        //-----------------------------------------------------------------------

        /**
         * true if this substitution should have the default "by digits"
         * behavior, false otherwise
         */
        private readonly bool byDigits;

        /**
         * true if we automatically insert spaces to separate names of digits
         * set to false by '>>>' in fraction rules, used by Thai.
         */
        private readonly bool useSpaces;

        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        /**
         * Constructs a FractionalPartSubstitution.  This object keeps a flag
         * telling whether it should format by digits or not.  In addition,
         * it marks the rule set it calls (if any) as a fraction rule set.
         */
        internal FractionalPartSubstitution(int pos,
                                   NFRuleSet ruleSet,
                                   string description)
                           : base(pos, ruleSet, description)
        {
            if (description.Equals(">>", StringComparison.Ordinal) || description.Equals(">>>", StringComparison.Ordinal) || ruleSet == this.ruleSet)
            {
                byDigits = true;
                useSpaces = !description.Equals(">>>", StringComparison.Ordinal);
            }
            else
            {
                byDigits = false;
                useSpaces = true;
                this.ruleSet.MakeIntoFractionRuleSet();
            }
        }

        //-----------------------------------------------------------------------
        // formatting
        //-----------------------------------------------------------------------

        /**
         * If in "by digits" mode, fills in the substitution one decimal digit
         * at a time using the rule set containing this substitution.
         * Otherwise, uses the superclass function.
         * @param number The number being formatted
         * @param toInsertInto The string to insert the result of formatting
         * the substitution into
         * @param position The position of the owning rule's rule text in
         * toInsertInto
         */
        public override void DoSubstitution(double number, StringBuilder toInsertInto, int position, int recursionCount)
        {
            if (!byDigits)
            {
                // if we're not in "byDigits" mode, just use the inherited
                // doSubstitution() routine
                base.DoSubstitution(number, toInsertInto, position, recursionCount);
            }
            else
            {
                // if we're in "byDigits" mode, transform the value into an integer
                // by moving the decimal point eight places to the right and
                // pulling digits off the right one at a time, formatting each digit
                // as an integer using this substitution's owning rule set
                // (this is slower, but more accurate, than doing it from the
                // other end)

                DecimalQuantity_DualStorageBCD fq = new DecimalQuantity_DualStorageBCD(number);
                fq.RoundToInfinity(); // ensure doubles are resolved using slow path

                bool pad = false;
                int mag = fq.LowerDisplayMagnitude;
                while (mag < 0)
                {
                    if (pad && useSpaces)
                    {
                        toInsertInto.Insert(position + pos, ' ');
                    }
                    else
                    {
                        pad = true;
                    }
                    ruleSet.Format(fq.GetDigit(mag++), toInsertInto, position + pos, recursionCount);
                }
            }
        }

        /**
         * Returns the fractional part of the number, which will always be
         * zero if it's a long.
         * @param number The number being formatted
         * @return 0
         */
        public override long TransformNumber(long number)
        {
            return 0;
        }

        /**
         * Returns the fractional part of the number.
         * @param number The number being formatted.
         * @return number - floor(number)
         */
        public override double TransformNumber(double number)
        {
            return number - Math.Floor(number);
        }

        //-----------------------------------------------------------------------
        // parsing
        //-----------------------------------------------------------------------

        /**
         * If in "by digits" mode, parses the string as if it were a string
         * of individual digits; otherwise, uses the superclass function.
         * @param text The string to parse
         * @param parsePosition Ignored on entry, but updated on exit to point
         * to the first unmatched character
         * @param baseValue The partial parse result prior to entering this
         * function
         * @param upperBound Only consider rules with base values lower than
         * this when filling in the substitution
         * @param lenientParse If true, try matching the text as numerals if
         * matching as words doesn't work
         * @return If the match was successful, the current partial parse
         * result; otherwise new Long(0).  The result is either a Long or
         * a Double.
         */
        public override Number DoParse(string text, ParsePosition parsePosition, double baseValue,
                              double upperBound, bool lenientParse)
        {
            // if we're not in byDigits mode, we can just use the inherited
            // doParse()
            if (!byDigits)
            {
                return base.DoParse(text, parsePosition, baseValue, 0, lenientParse);
            }
            else
            {
                // if we ARE in byDigits mode, parse the text one digit at a time
                // using this substitution's owning rule set (we do this by setting
                // upperBound to 10 when calling doParse() ) until we reach
                // nonmatching text
                string workText = text;
                ParsePosition workPos = new ParsePosition(1);
                double result;
                int digit;

                DecimalQuantity_DualStorageBCD fq = new DecimalQuantity_DualStorageBCD();
                int leadingZeros = 0;
                while (workText.Length > 0 && workPos.Index != 0)
                {
                    workPos.Index = 0;
                    digit = ruleSet.Parse(workText, workPos, 10).ToInt32();
                    if (lenientParse && workPos.Index == 0)
                    {
                        Number n = ruleSet.owner.DecimalFormat.Parse(workText, workPos);
                        if (n != null)
                        {
                            digit = n.ToInt32();
                        }
                    }

                    if (workPos.Index != 0)
                    {
                        if (digit == 0)
                        {
                            leadingZeros++;
                        }
                        else
                        {
                            fq.AppendDigit((byte)digit, leadingZeros, false);
                            leadingZeros = 0;
                        }

                        parsePosition.Index = parsePosition.Index + workPos.Index;
                        workText = workText.Substring(workPos.Index);
                        while (workText.Length > 0 && workText[0] == ' ')
                        {
                            workText = workText.Substring(1);
                            parsePosition.Index = parsePosition.Index + 1;
                        }
                    }
                }
                result = fq.ToDouble();

                result = ComposeRuleValue(result, baseValue);
                return Double.GetInstance(result);
            }
        }

        /**
         * Returns the sum of the two partial parse results.
         * @param newRuleValue The result of parsing the substitution
         * @param oldRuleValue The partial parse result prior to calling
         * this function
         * @return newRuleValue + oldRuleValue
         */
        public override double ComposeRuleValue(double newRuleValue, double oldRuleValue)
        {
            return newRuleValue + oldRuleValue;
        }

        /**
         * Not used.
         */
        public override double CalcUpperBound(double oldUpperBound)
        {
            return 0;   // this value is ignored
        }

        //-----------------------------------------------------------------------
        // simple accessor
        //-----------------------------------------------------------------------

        /**
         * The token character for a FractionalPartSubstitution is &gt;.
         * @return '&gt;'
         */
        private protected override char TokenChar => '>';
    }

    //===================================================================
    // AbsoluteValueSubstitution
    //===================================================================

    /**
     * A substitution that formats the absolute value of the number.
     * This substitution is represented by &gt;&gt; in a negative-number rule.
     */
    internal class AbsoluteValueSubstitution : NFSubstitution
    {
        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        /**
         * Constructs an AbsoluteValueSubstitution.  This just uses the
         * superclass constructor.
         */
        internal AbsoluteValueSubstitution(int pos,
                                  NFRuleSet ruleSet,
                                  string description)
                            : base(pos, ruleSet, description)
        {
        }

        //-----------------------------------------------------------------------
        // formatting
        //-----------------------------------------------------------------------

        /**
         * Returns the absolute value of the number.
         * @param number The number being formatted.
         * @return abs(number)
         */
        public override long TransformNumber(long number)
        {
            return Math.Abs(number);
        }

        /**
         * Returns the absolute value of the number.
         * @param number The number being formatted.
         * @return abs(number)
         */
        public override double TransformNumber(double number)
        {
            return Math.Abs(number);
        }

        //-----------------------------------------------------------------------
        // parsing
        //-----------------------------------------------------------------------

        /**
         * Returns the additive inverse of the result of parsing the
         * substitution (this supersedes the earlier partial result)
         * @param newRuleValue The result of parsing the substitution
         * @param oldRuleValue The partial parse result prior to calling
         * this function
         * @return -newRuleValue
         */
        public override double ComposeRuleValue(double newRuleValue, double oldRuleValue)
        {
            return -newRuleValue;
        }

        /**
         * Sets the upper bound beck up to consider all rules
         * @param oldUpperBound Ignored.
         * @return Double.MAX_VALUE
         */
        public override double CalcUpperBound(double oldUpperBound)
        {
            return double.MaxValue;
        }

        //-----------------------------------------------------------------------
        // simple accessor
        //-----------------------------------------------------------------------

        /**
         * The token character for an AbsoluteValueSubstitution is &gt;
         * @return '&gt;'
         */
        private protected override char TokenChar => '>';
    }

    //===================================================================
    // NumeratorSubstitution
    //===================================================================

    /**
     * A substitution that multiplies the number being formatted (which is
     * between 0 and 1) by the base value of the rule that owns it and
     * formats the result.  It is represented by &lt;&lt; in the rules
     * in a fraction rule set.
     */
    internal class NumeratorSubstitution : NFSubstitution
    {
        //-----------------------------------------------------------------------
        // data members
        //-----------------------------------------------------------------------

        /**
         * The denominator of the fraction we're finding the numerator for.
         * (The base value of the rule that owns this substitution.)
         */
        private readonly double denominator;

        /**
         * True if we format leading zeros (this is a hack for Hebrew spellout)
         */
        private readonly bool withZeros;

        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        /**
         * Constructs a NumeratorSubstitution.  In addition to the inherited
         * fields, a NumeratorSubstitution keeps track of a denominator, which
         * is merely the base value of the rule that owns it.
         */
        internal NumeratorSubstitution(int pos,
                              double denominator,
                              NFRuleSet ruleSet,
                              string description)
            : base(pos, ruleSet, Fixdesc(description))
        {


            // this substitution's behavior depends on the rule's base value
            // Rather than keeping a backpointer to the rule, we copy its
            // base value here
            this.denominator = denominator;

            this.withZeros = description.EndsWith("<<", StringComparison.Ordinal);
        }

        internal static string Fixdesc(string description)
        {
            return description.EndsWith("<<", StringComparison.Ordinal)
                ? description.Substring(0, description.Length - 1) // ICU4N: Checked 2nd parameter
                : description;
        }

        //-----------------------------------------------------------------------
        // boilerplate
        //-----------------------------------------------------------------------

        /**
         * Tests two NumeratorSubstitutions for equality
         * @param that The other NumeratorSubstitution
         * @return true if the two objects are functionally equivalent
         */
        public override bool Equals(object that)
        {
            if (base.Equals(that))
            {
                NumeratorSubstitution that2 = (NumeratorSubstitution)that;
                return denominator == that2.denominator && withZeros == that2.withZeros;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ denominator.GetHashCode() ^ withZeros.GetHashCode();
        }

        //-----------------------------------------------------------------------
        // formatting
        //-----------------------------------------------------------------------

        /**
         * Performs a mathematical operation on the number, formats it using
         * either ruleSet or decimalFormat, and inserts the result into
         * toInsertInto.
         * @param number The number being formatted.
         * @param toInsertInto The string we insert the result into
         * @param position The position in toInsertInto where the owning rule's
         * rule text begins (this value is added to this substitution's
         * position to determine exactly where to insert the new text)
         */
        public override void DoSubstitution(double number, StringBuilder toInsertInto, int position, int recursionCount)
        {
            // perform a transformation on the number being formatted that
            // is dependent on the type of substitution this is
            //String s = toInsertInto.toString();
            double numberToFormat = TransformNumber(number);

            if (withZeros && ruleSet != null)
            {
                // if there are leading zeros in the decimal expansion then emit them
                long nf = (long)numberToFormat;
                int len = toInsertInto.Length;
                while ((nf *= 10) < denominator)
                {
                    toInsertInto.Insert(position + pos, ' ');
                    ruleSet.Format(0, toInsertInto, position + pos, recursionCount);
                }
                position += toInsertInto.Length - len;
            }

            // if the result is an integer, from here on out we work in integer
            // space (saving time and memory and preserving accuracy)
            if (numberToFormat == Math.Floor(numberToFormat) && ruleSet != null)
            {
                ruleSet.Format((long)numberToFormat, toInsertInto, position + pos, recursionCount);

                // if the result isn't an integer, then call either our rule set's
                // format() method or our DecimalFormat's format() method to
                // format the result
            }
            else
            {
                if (ruleSet != null)
                {
                    ruleSet.Format(numberToFormat, toInsertInto, position + pos, recursionCount);
                }
                else
                {
                    toInsertInto.Insert(position + pos, numberFormat.Format(numberToFormat));
                }
            }
        }

        /**
         * Returns the number being formatted times the denominator.
         * @param number The number being formatted
         * @return number * denominator
         */
        public override long TransformNumber(long number)
        {
            return (long)Math.Round(number * denominator);
        }

        /**
         * Returns the number being formatted times the denominator.
         * @param number The number being formatted
         * @return number * denominator
         */
        public override double TransformNumber(double number)
        {
            return Math.Round(number * denominator);
        }

        //-----------------------------------------------------------------------
        // parsing
        //-----------------------------------------------------------------------

        /**
         * Dispatches to the inherited version of this function, but makes
         * sure that lenientParse is off.
         */
        public override Number DoParse(string text, ParsePosition parsePosition, double baseValue,
                              double upperBound, bool lenientParse)
        {
            // we don't have to do anything special to do the parsing here,
            // but we have to turn lenient parsing off-- if we leave it on,
            // it SERIOUSLY messes up the algorithm

            // if withZeros is true, we need to count the zeros
            // and use that to adjust the parse result
            int zeroCount = 0;
            if (withZeros)
            {
                string workText = text;
                ParsePosition workPos = new ParsePosition(1);
                //int digit;

                while (workText.Length > 0 && workPos.Index != 0)
                {
                    workPos.Index = 0;
                    /*digit = */
                    ruleSet.Parse(workText, workPos, 1).ToInt32(); // parse zero or nothing at all
                    if (workPos.Index == 0)
                    {
                        // we failed, either there were no more zeros, or the number was formatted with digits
                        // either way, we're done
                        break;
                    }

                    ++zeroCount;
                    parsePosition.Index = parsePosition.Index + workPos.Index;
                    workText = workText.Substring(workPos.Index);
                    while (workText.Length > 0 && workText[0] == ' ')
                    {
                        workText = workText.Substring(1);
                        parsePosition.Index = parsePosition.Index + 1;
                    }
                }

                text = text.Substring(parsePosition.Index); // arrgh!
                parsePosition.Index = 0;
            }

            // we've parsed off the zeros, now let's parse the rest from our current position
            Number result = base.DoParse(text, parsePosition, withZeros ? 1 : baseValue, upperBound, false);

            if (withZeros)
            {
                // any base value will do in this case.  is there a way to
                // force this to not bother trying all the base values?

                // compute the 'effective' base and prescale the value down
                long n = result.ToInt64();
                long d = 1;
                while (d <= n)
                {
                    d *= 10;
                }
                // now add the zeros
                while (zeroCount > 0)
                {
                    d *= 10;
                    --zeroCount;
                }
                // d is now our true denominator
                result = Double.GetInstance(n / (double)d);
            }

            return result;
        }

        /**
         * Divides the result of parsing the substitution by the partial
         * parse result.
         * @param newRuleValue The result of parsing the substitution
         * @param oldRuleValue The owning rule's base value
         * @return newRuleValue / oldRuleValue
         */
        public override double ComposeRuleValue(double newRuleValue, double oldRuleValue)
        {
            return newRuleValue / oldRuleValue;
        }

        /**
         * Sets the upper bound down to this rule's base value
         * @param oldUpperBound Ignored
         * @return The base value of the rule owning this substitution
         */
        public override double CalcUpperBound(double oldUpperBound)
        {
            return denominator;
        }

        //-----------------------------------------------------------------------
        // simple accessor
        //-----------------------------------------------------------------------

        /**
         * The token character for a NumeratorSubstitution is &lt;
         * @return '&lt;'
         */
        private protected override char TokenChar => '<';
    }
}
