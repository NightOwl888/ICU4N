using System;
using System.Diagnostics.CodeAnalysis;

namespace ICU4N.Globalization
{
#if FEATURE_SPAN
    //===================================================================
    // NFSubstitution (abstract base class)
    //===================================================================

    /// <summary>
    /// An abstract class defining protocol for substitutions.  A substitution
    /// is a section of a rule that inserts text into the rule's rule text
    /// based on some part of the number being formatted.
    /// </summary>
    /// <author>Richard Gillam</author>
    internal abstract partial class NumberFormatSubstitution
    {
        //-----------------------------------------------------------------------
        // data members
        //-----------------------------------------------------------------------

        /// <summary>
        /// The substitution's position in the rule text of the rule that owns it
        /// </summary>
        internal readonly int pos;

        /// <summary>
        /// The rule set this substitution uses to format its result, or null.
        /// (Either this or <see cref="numberFormatPattern"/> has to be non-null.)
        /// </summary>
        internal readonly NumberFormatRuleSet ruleSet;

        /// <summary>
        /// The pattern string this substitution uses to format its result, or <c>null</c>.
        /// (Either this or <see cref="ruleSet"/> has to be non-null.)
        /// </summary>
        internal readonly string numberFormatPattern;

        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        // ICU4N TODO: Implementation

        /// <summary>
        /// Base constructor for substitutions. This constructor sets up the
        /// fields which are common to all substitutions.
        /// </summary>
        /// <param name="pos">The substitution's position in the owning rule's rule text.</param>
        /// <param name="ruleSet">The rule set that owns this substitution.</param>
        /// <param name="description">The substitution descriptor (i.e., the text
        /// inside the token characters)</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="description"/> length is 1.
        /// <para/>
        /// -or-
        /// <para/>
        /// <paramref name="description"/> doesn't begin with and end with the same character.
        /// <para/>
        /// -or-
        /// <para/>
        /// <paramref name="description"/> starts with a <see cref="char"/> other than '%', '#', '0', or '&gt;'.
        /// </exception>
        [SuppressMessage("Major Code Smell", "S1871:Two branches in a conditional structure should not have exactly the same implementation", Justification = "These are tested in a specific order")]
        private protected NumberFormatSubstitution(int pos, // ICU4N: Changed from internal to private protected
                       NumberFormatRuleSet ruleSet,
                       ReadOnlySpan<char> description)
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
                description = description.Slice(1, descriptionLen - 2); // ICU4N: Corrected 2nd parameter
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
                this.numberFormatPattern = null;
            }
            else if (description[0] == '%')
            {
                // if the description contains a rule set name, that's the rule
                // set we use to format the result: get a reference to the
                // names rule set
                this.ruleSet = ruleSet.owner.FindRuleSet(new string(description));
                this.numberFormatPattern = null;
            }
            else if (description[0] == '#' || description[0] == '0')
            {
                // if the description begins with 0 or #, treat it as a
                // DecimalFormat pattern, and initialize a DecimalFormat with
                // that pattern (then set it to use the DecimalFormatSymbols
                // belonging to our formatter)
                this.ruleSet = null;
                //this.numberFormat = (DecimalFormat)ruleSet.owner.DecimalFormat.Clone();
                //this.numberFormat.ApplyPattern(description);
                this.numberFormatPattern = new string(description);
            }
            else if (description[0] == '>')
            {
                // if the description is ">>>", this substitution bypasses the
                // usual rule-search process and always uses the rule that precedes
                // it in its own rule set's rule list (this is used for place-value
                // notations: formats where you want to see a particular part of
                // a number even when it's 0)
                this.ruleSet = ruleSet; // was null, thai rules added to control space
                this.numberFormatPattern = null;
            }
            else
            {
                // and of the description is none of these things, it's a syntax error
                throw new ArgumentException("Illegal substitution syntax");
            }
        }

        /// <summary>
        /// Set's the substitution's divisor. Used by <see cref="NumberFormatRule.SetBaseValue(long)"/>.
        /// A no-op for all substitutions except multiplier and modulus
        /// substitutions.
        /// </summary>
        /// <param name="radix">The radix of the divisor.</param>
        /// <param name="exponent">The exponent of the divisor.</param>
        public virtual void SetDivisor(int radix, short exponent)
        {
            // a no-op for all substitutions except multiplier and modulus substitutions
        }

        //-----------------------------------------------------------------------
        // boilerplate
        //-----------------------------------------------------------------------

        /// <summary>
        /// Compares two substitutions for equality.
        /// </summary>
        /// <param name="that">The substitution to compare this one to.</param>
        /// <returns><c>true</c> if the two substitutions are functionally equivalent.</returns>
        public override bool Equals(object that)
        {
            // compare class and all of the fields all substitutions have
            // in common
            if (that is null)
            {
                return false;
            }
            if (this == that)
            {
                return true;
            }
            if (this.GetType() == that.GetType()) // ICU4N TODO: This probably won't work for inherited members. Need to check.
            {
                NumberFormatSubstitution that2 = (NumberFormatSubstitution)that;

                return pos == that2.pos // ICU4N TODO: Compare RuleSets (once we fix the Equals() to check all duplicate non-numeric sets for various decimal characters)
                    && (ruleSet != null || that2.ruleSet == null) // can't compare tree structure, no .equals or recurse
                    && StringComparer.Ordinal.Equals(numberFormatPattern, that2.numberFormatPattern);  //(numberFormat == null ? (that2.numberFormat == null) : numberFormat.Equals(that2.numberFormat));
            }
            return false;
        }

        public override int GetHashCode() // ICU4N TODO: Implmentation
        {
            //assert false : "hashCode not designed";
            return 42;
        }

        /// <summary>
        /// Returns a textual description of the substitution.
        /// </summary>
        /// <returns>A textual description of the substitution. This might
        /// not be identical to the description it was created from, but
        /// it'll produce the same result.</returns>
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
                return TokenChar + numberFormatPattern + TokenChar;
            }
        }

        //-----------------------------------------------------------------------
        // formatting
        //-----------------------------------------------------------------------

        private const long MAX_INT64_IN_DOUBLE = 0x1FFFFFFFFFFFFFL;

        // ICU4N TODO: Implementation

        // Create a virtual DoSubstitution method.
        // 1. Pass in ValueStringBuilder
        // 2. Pass in UNumberFormatInfo (or perhaps IDecimalFormatSymbols?)
        // 3. Use a NumberBuffer so we don't have to have separate overloads for different data types for all of the business logic? Need to investigate.

        ///// <summary>
        ///// Performs a mathematical operation on the number, formats it using
        ///// either <see cref="ruleSet"/> or <see cref="numberFormatPattern"/>, and inserts the result into
        ///// <paramref name="toInsertInto"/>.
        ///// </summary>
        ///// <param name="number">The number being formatted.</param>
        ///// <param name="toInsertInto">The string we insert the result into.</param>
        ///// <param name="position">The position in toInsertInto where the owning rule's
        ///// rule text begins (this value is added to this substitution's
        ///// position to determine exactly where to insert the new text).</param>
        ///// <param name="recursionCount">The number of recursive calls to this method.</param>
        //public virtual void DoSubstitution(long number, StringBuilder toInsertInto, int position, int recursionCount) // ICU4N TODO: Need to pass UNumberFormatInfo here
        //{
        //    throw new NotImplementedException(); // ICU4N TODO: Implementation

        //    //if (ruleSet != null)
        //    //{
        //    //    // Perform a transformation on the number that is dependent
        //    //    // on the type of substitution this is, then just call its
        //    //    // rule set's format() method to format the result
        //    //    long numberToFormat = TransformNumber(number);

        //    //    ruleSet.Format(numberToFormat, toInsertInto, position + pos, recursionCount);
        //    //}
        //    //else
        //    //{
        //    //    if (number <= MAX_INT64_IN_DOUBLE)
        //    //    {
        //    //        // or perform the transformation on the number (preserving
        //    //        // the result's fractional part if the formatter it set
        //    //        // to show it), then use that formatter's format() method
        //    //        // to format the result
        //    //        double numberToFormat = TransformNumber((double)number);
        //    //        if (numberFormat.MaximumFractionDigits == 0)
        //    //        {
        //    //            numberToFormat = Math.Floor(numberToFormat);
        //    //        }

        //    //        toInsertInto.Insert(position + pos, numberFormat.Format(numberToFormat));
        //    //    }
        //    //    else
        //    //    {
        //    //        // We have gone beyond double precision. Something has to give.
        //    //        // We're favoring accuracy of the large number over potential rules
        //    //        // that round like a CompactDecimalFormat, which is not a common use case.
        //    //        //
        //    //        // Perform a transformation on the number that is dependent
        //    //        // on the type of substitution this is, then just call its
        //    //        // rule set's format() method to format the result
        //    //        long numberToFormat = TransformNumber(number);
        //    //        toInsertInto.Insert(position + pos, numberFormat.Format(numberToFormat));
        //    //    }
        //    //}
        //}

        ///// <summary>
        ///// Performs a mathematical operation on the number, formats it using
        ///// either <see cref="ruleSet"/> or <see cref="numberFormatPattern"/>, and inserts the result into
        ///// <paramref name="toInsertInto"/>.
        ///// </summary>
        ///// <param name="number">The number being formatted.</param>
        ///// <param name="toInsertInto">The string we insert the result into.</param>
        ///// <param name="position">The position in <paramref name="toInsertInto"/> where the owning rule's
        ///// rule text begins (this value is added to this substitution's
        ///// position to determine exactly where to insert the new text).</param>
        ///// <param name="recursionCount">The number of recursive calls to this method.</param>
        //public virtual void DoSubstitution(double number, StringBuilder toInsertInto, int position, int recursionCount) // ICU4N TODO: Need to pass UNumberFormatInfo here
        //{
        //    throw new NotImplementedException(); // ICU4N TODO: Implementation

        //    //// perform a transformation on the number being formatted that
        //    //// is dependent on the type of substitution this is
        //    //double numberToFormat = TransformNumber(number);

        //    //if (double.IsInfinity(numberToFormat))
        //    //{
        //    //    // This is probably a minus rule. Combine it with an infinite rule.
        //    //    NFRule infiniteRule = ruleSet.FindRule(double.PositiveInfinity);
        //    //    infiniteRule.DoFormat(numberToFormat, toInsertInto, position + pos, recursionCount);
        //    //    return;
        //    //}

        //    //// if the result is an integer, from here on out we work in integer
        //    //// space (saving time and memory and preserving accuracy)
        //    //if (numberToFormat == Math.Floor(numberToFormat) && ruleSet != null) // ICU4N: This is quite a bit faster than using numberToFormat.IsInteger()
        //    //{
        //    //    ruleSet.Format((long)numberToFormat, toInsertInto, position + pos, recursionCount);

        //    //    // if the result isn't an integer, then call either our rule set's
        //    //    // format() method or our DecimalFormat's format() method to
        //    //    // format the result
        //    //}
        //    //else
        //    //{
        //    //    if (ruleSet != null)
        //    //    {
        //    //        ruleSet.Format(numberToFormat, toInsertInto, position + pos, recursionCount);
        //    //    }
        //    //    else
        //    //    {
        //    //        toInsertInto.Insert(position + this.pos, numberFormat.Format(numberToFormat));
        //    //    }
        //    //}
        //}

        /// <summary>
        /// Subclasses override this function to perform some kind of
        /// mathematical operation on the number. The result of this operation
        /// is formatted using the rule set or <see cref="numberFormatPattern"/> that this
        /// substitution refers to, and the result is inserted into the result
        /// string.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <returns>The result of performing the opreration on the number.</returns>
        public abstract long TransformNumber(long number);

        /// <summary>
        /// Subclasses override this function to perform some kind of
        /// mathematical operation on the number. The result of this operation
        /// is formatted using the rule set or <see cref="numberFormatPattern"/> that this
        /// substitution refers to, and the result is inserted into the result
        /// string.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <returns>The result of performing the opreration on the number.</returns>
        public abstract double TransformNumber(double number);

        //-----------------------------------------------------------------------
        // parsing
        //-----------------------------------------------------------------------

        // ICU4N TODO: Implementation

        /////// <summary>
        /////// Parses a string using the rule set or <see cref="DecimalFormat"/> belonging
        /////// to this substitution.  If there's a match, a mathematical
        /////// operation (the inverse of the one used in formatting) is
        /////// performed on the result of the parse and the value passed in
        /////// and returned as the result.  The parse position is updated to
        /////// point to the first unmatched character in the string.
        /////// </summary>
        /////// <param name="text">The string to parse.</param>
        /////// <param name="parsePosition">On entry, ignored, but assumed to be 0.
        /////// On exit, this is updated to point to the first unmatched
        /////// character (or 0 if the substitution didn't match).</param>
        /////// <param name="baseValue">A partial parse result that should be
        /////// combined with the result of this parse.</param>
        /////// <param name="upperBound">When searching the rule set for a rule
        /////// matching the string passed in, only rules with base values
        /////// lower than this are considered.</param>
        /////// <param name="lenientParse">If <c>true</c> and matching against rules fails,
        /////// the substitution will also try matching the text against
        /////// numerals using a default-constructed <see cref="NumberFormat"/>. If <c>false</c>,
        /////// no extra work is done.  (This value is false whenever the
        /////// formatter isn't in lenient-parse mode, but is also false
        /////// under some conditions even when the formatter _is_ in
        /////// lenient-parse mode.)</param>
        /////// <returns>If there's a match, this is the result of composing
        /////// <paramref name="baseValue"/> with whatever was returned from matching the
        /////// characters. This will be either a <see cref="Long"/> or a <see cref="Double"/>. If there's
        /////// no match this is <c>Long.GetInstance(0)</c> (not <c>null</c>), and <paramref name="parsePosition"/>
        /////// is left unchanged.</returns>
        ////public virtual Number DoParse(string text, ParsePosition parsePosition, double baseValue,
        ////                      double upperBound, bool lenientParse)
        ////{
        ////    Number tempResult;

        ////    // figure out the highest base value a rule can have and match
        ////    // the text being parsed (this varies according to the type of
        ////    // substitutions: multiplier, modulus, and numerator substitutions
        ////    // restrict the search to rules with base values lower than their
        ////    // own; same-value substitutions leave the upper bound wherever
        ////    // it was, and the others allow any rule to match
        ////    upperBound = CalcUpperBound(upperBound);

        ////    // use our rule set to parse the text.  If that fails and
        ////    // lenient parsing is enabled (this is always false if the
        ////    // formatter's lenient-parsing mode is off, but it may also
        ////    // be false even when the formatter's lenient-parse mode is
        ////    // on), then also try parsing the text using a default-
        ////    // constructed NumberFormat
        ////    if (ruleSet != null)
        ////    {
        ////        tempResult = ruleSet.Parse(text, parsePosition, upperBound);
        ////        if (lenientParse && !ruleSet.IsFractionSet && parsePosition.Index == 0)
        ////        {
        ////            tempResult = ruleSet.owner.DecimalFormat.Parse(text, parsePosition);
        ////        }

        ////        // ...or use our DecimalFormat to parse the text
        ////    }
        ////    else
        ////    {
        ////        tempResult = numberFormat.Parse(text, parsePosition);
        ////    }

        ////    // if the parse was successful, we've already advanced the caller's
        ////    // parse position (this is the one function that doesn't have one
        ////    // of its own).  Derive a parse result and return it as a Long,
        ////    // if possible, or a Double
        ////    if (parsePosition.Index != 0)
        ////    {
        ////        double result = tempResult.ToDouble();

        ////        // composeRuleValue() produces a full parse result from
        ////        // the partial parse result passed to this function from
        ////        // the caller (this is either the owning rule's base value
        ////        // or the partial result obtained from composing the
        ////        // owning rule's base value with its other substitution's
        ////        // parse result) and the partial parse result obtained by
        ////        // matching the substitution (which will be the same value
        ////        // the caller would get by parsing just this part of the
        ////        // text with RuleBasedNumberFormat.parse() ).  How the two
        ////        // values are used to derive the full parse result depends
        ////        // on the types of substitutions: For a regular rule, the
        ////        // ultimate result is its multiplier substitution's result
        ////        // times the rule's divisor (or the rule's base value) plus
        ////        // the modulus substitution's result (which will actually
        ////        // supersede part of the rule's base value).  For a negative-
        ////        // number rule, the result is the negative of its substitution's
        ////        // result.  For a fraction rule, it's the sum of its two
        ////        // substitution results.  For a rule in a fraction rule set,
        ////        // it's the numerator substitution's result divided by
        ////        // the rule's base value.  Results from same-value substitutions
        ////        // propagate back upward, and null substitutions don't affect
        ////        // the result.
        ////        result = ComposeRuleValue(result, baseValue);
        ////        if (result == (long)result) // ICU4N: This is quite a bit faster than using result.IsInteger()
        ////        {
        ////            return Long.GetInstance((long)result);
        ////        }
        ////        else
        ////        {
        ////            return Double.GetInstance(result);
        ////        }

        ////        // if the parse was UNsuccessful, return 0
        ////    }
        ////    else
        ////    {
        ////        return tempResult;
        ////    }
        ////}

        /// <summary>
        /// Derives a new value from the two values passed in.  The two values
        /// are typically either the base values of two rules (the one containing
        /// the substitution and the one matching the substitution) or partial
        /// parse results derived in some other way.  The operation is generally
        /// the inverse of the operation performed by <see cref="TransformNumber(double)"/>.
        /// </summary>
        /// <param name="newRuleValue">The value produced by matching this substitution.</param>
        /// <param name="oldRuleValue">The value that was passed to the substitution by the rule that owns it.</param>
        /// <returns>A third value derived from the other two, representing a partial parse result.</returns>
        public abstract double ComposeRuleValue(double newRuleValue, double oldRuleValue);

        /// <summary>
        /// Calculates an upper bound when searching for a rule that matches
        /// this substitution.  Rules with base values greater than or equal
        /// to <paramref name="oldUpperBound"/> are not considered.
        /// </summary>
        /// <param name="oldUpperBound">The current upper-bound setting. The new
        /// upper bound can't be any higher.</param>
        /// <returns>The new upper bound.</returns>
        public abstract double CalcUpperBound(double oldUpperBound);

        //-----------------------------------------------------------------------
        // simple accessors
        //-----------------------------------------------------------------------

        /// <summary>
        /// Gets the substitution's position in the rule that owns it.
        /// </summary>
        public int Pos => pos;

        /// <summary>
        /// Gets the character used in the textual representation of
        /// substitutions of this type (This substitution's token character).
        /// Used by <see cref="ToString()"/>.
        /// </summary>
        private protected abstract char TokenChar { get; }

        /// <summary>
        /// Returns <c>true</c> if this is a modulus substitution.  (We didn't do this
        /// with 'is' partially because it causes source files to proliferate
        /// and partially because we have to port this to C++.)
        /// </summary>
        public virtual bool IsModulusSubstitution => false;
    }
#endif
}
