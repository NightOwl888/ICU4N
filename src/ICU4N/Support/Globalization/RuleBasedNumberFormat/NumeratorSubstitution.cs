using ICU4N.Support.Text;
using System;
using System.Diagnostics;
#nullable enable

namespace ICU4N.Globalization
{
    //===================================================================
    // NumeratorSubstitution
    //===================================================================

    /// <summary>
    /// A substitution that multiplies the number being formatted (which is
    /// between 0 and 1) by the base value of the rule that owns it and
    /// formats the result. It is represented by &lt;&lt; in the rules
    /// in a fraction rule set.
    /// </summary>
    internal sealed class NumeratorSubstitution : NumberFormatSubstitution
    {
        //-----------------------------------------------------------------------
        // data members
        //-----------------------------------------------------------------------

        /// <summary>
        /// The denominator of the fraction we're finding the numerator for.
        /// (The base value of the rule that owns this substitution.)
        /// </summary>
        internal readonly double denominator; // Internal for testing

        /// <summary>
        /// True if we format leading zeros (this is a hack for Hebrew spellout).
        /// </summary>
        internal readonly bool withZeros; // Internal for testing

        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        /// <summary>
        /// Constructs a <see cref="NumeratorSubstitution"/>. In addition to the inherited
        /// fields, a <see cref="NumeratorSubstitution"/> keeps track of a denominator, which
        /// is merely the base value of the rule that owns it.
        /// </summary>
        /// <param name="pos">The substitution's position in the owning rule's rule text.</param>
        /// <param name="denominator">The denominator of the fraction we're finding the numerator for.</param>
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
        internal NumeratorSubstitution(int pos,
                              double denominator,
                              NumberFormatRuleSet? ruleSet,
                              ReadOnlySpan<char> description)
            : base(pos, ruleSet, FixDescription(description))
        {


            // this substitution's behavior depends on the rule's base value
            // Rather than keeping a backpointer to the rule, we copy its
            // base value here
            this.denominator = denominator;

            this.withZeros = description.EndsWith("<<", StringComparison.Ordinal);
        }

        internal static ReadOnlySpan<char> FixDescription(ReadOnlySpan<char> description)
        {
            return description.EndsWith("<<", StringComparison.Ordinal)
                ? description.Slice(0, description.Length - 1) // ICU4N: Checked 2nd parameter
                : description;
        }

        //-----------------------------------------------------------------------
        // boilerplate
        //-----------------------------------------------------------------------

        /// <summary>
        /// Tests two <see cref="NumeratorSubstitution"/>s for equality.
        /// </summary>
        /// <param name="that">The other <see cref="NumeratorSubstitution"/>.</param>
        /// <returns><c>true</c> if the two objects are functionally equivalent.</returns>
        public override bool Equals(object? that)
        {
            if (base.Equals(that))
            {
                NumeratorSubstitution that2 = (NumeratorSubstitution)that;
                return denominator == that2.denominator && withZeros == that2.withZeros;
            }
            return false;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ denominator.GetHashCode() ^ withZeros.GetHashCode();
        }

        //-----------------------------------------------------------------------
        // formatting
        //-----------------------------------------------------------------------

        /// <summary>
        /// Performs a mathematical operation on the number, formats it using
        /// either <c>ruleSet</c> or <c>numberFormat</c>, and inserts the result into
        /// <paramref name="toInsertInto"/>.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <param name="toInsertInto">The string we insert the result into.</param>
        /// <param name="position">The position in toInsertInto where the owning rule's
        /// rule text begins (this value is added to this substitution's
        /// position to determine exactly where to insert the new text).</param>
        /// <param name="info">The <see cref="UNumberFormatInfo"/> that contains the culture specific number formatting settings.</param>
        /// <param name="recursionCount">The number of recursive calls to this method.</param>
        public override void DoSubstitution(double number, ref ValueStringBuilder toInsertInto, int position, UNumberFormatInfo info, int recursionCount)
        {
            Debug.Assert(info != null);

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
                    ruleSet.Format(0, ref toInsertInto, position + pos, info, recursionCount);
                }
                position += toInsertInto.Length - len;
            }

            // if the result is an integer, from here on out we work in integer
            // space (saving time and memory and preserving accuracy)
            if (numberToFormat == Math.Floor(numberToFormat) && ruleSet != null) // ICU4N: This is quite a bit faster than using numberToFormat.IsInteger()
            {
                ruleSet.Format((long)numberToFormat, ref toInsertInto, position + pos, info, recursionCount);

                // if the result isn't an integer, then call either our rule set's
                // format() method or our DecimalFormat's format() method to
                // format the result
            }
            else
            {
                if (ruleSet != null)
                {
                    ruleSet.Format(numberToFormat, ref toInsertInto, position + pos, info, recursionCount);
                }
                else
                {
                    //toInsertInto.Insert(position + pos, numberFormat.Format(numberToFormat));
                    toInsertInto.InsertFormat(position + pos, numberToFormat, numberFormatPattern, info!, numberPatternProperties.GroupingSizes);
                }
            }
        }

        /// <summary>
        /// Returns the number being formatted times the denominator.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <returns><paramref name="number"/> * <see cref="denominator"/>.</returns>
        public override long TransformNumber(long number)
        {
            return (long)Math.Round(number * denominator); // ICU4N NOTE: This is different than the Java default of ToPositiveInfinity (Math.Ceiling()), but only this makes the tests pass
        }

        /// <summary>
        /// Returns the number being formatted times the denominator.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <returns><paramref name="number"/> * <see cref="denominator"/>.</returns>
        public override double TransformNumber(double number)
        {
            return Math.Round(number * denominator); // ICU4N NOTE: This is different than the Java default of ToPositiveInfinity (Math.Ceiling()), but only this makes the tests pass
        }

        //-----------------------------------------------------------------------
        // parsing
        //-----------------------------------------------------------------------

        // ICU4N TODO: Implementation

        ///// <summary>
        ///// Dispatches to the inherited version of this function, but makes
        ///// sure that <paramref name="lenientParse"/> is off.
        ///// </summary>
        ///// <param name="text">The string to parse.</param>
        ///// <param name="parsePosition">On entry, ignored, but assumed to be 0.
        ///// On exit, this is updated to point to the first unmatched
        ///// character (or 0 if the substitution didn't match).</param>
        ///// <param name="baseValue">A partial parse result that should be
        ///// combined with the result of this parse.</param>
        ///// <param name="upperBound">When searching the rule set for a rule
        ///// matching the string passed in, only rules with base values
        ///// lower than this are considered.</param>
        ///// <param name="lenientParse">Ignored.</param>
        ///// <returns>If there's a match, this is the result of composing
        ///// <paramref name="baseValue"/> with whatever was returned from matching the
        ///// characters. This will be either a <see cref="Long"/> or a <see cref="Double"/>. If there's
        ///// no match this is <c>Long.GetInstance(0)</c> (not <c>null</c>), and <paramref name="parsePosition"/>
        ///// is left unchanged.</returns>
        //public override Number DoParse(string text, ParsePosition parsePosition, double baseValue,
        //                      double upperBound, bool lenientParse)
        //{
        //    // we don't have to do anything special to do the parsing here,
        //    // but we have to turn lenient parsing off-- if we leave it on,
        //    // it SERIOUSLY messes up the algorithm

        //    // if withZeros is true, we need to count the zeros
        //    // and use that to adjust the parse result
        //    int zeroCount = 0;
        //    if (withZeros)
        //    {
        //        string workText = text;
        //        ParsePosition workPos = new ParsePosition(1);
        //        //int digit;

        //        while (workText.Length > 0 && workPos.Index != 0)
        //        {
        //            workPos.Index = 0;
        //            /*digit = */
        //            ruleSet.Parse(workText, workPos, 1).ToInt32(); // parse zero or nothing at all
        //            if (workPos.Index == 0)
        //            {
        //                // we failed, either there were no more zeros, or the number was formatted with digits
        //                // either way, we're done
        //                break;
        //            }

        //            ++zeroCount;
        //            parsePosition.Index = parsePosition.Index + workPos.Index;
        //            workText = workText.Substring(workPos.Index);
        //            while (workText.Length > 0 && workText[0] == ' ')
        //            {
        //                workText = workText.Substring(1);
        //                parsePosition.Index = parsePosition.Index + 1;
        //            }
        //        }

        //        text = text.Substring(parsePosition.Index); // arrgh!
        //        parsePosition.Index = 0;
        //    }

        //    // we've parsed off the zeros, now let's parse the rest from our current position
        //    Number result = base.DoParse(text, parsePosition, withZeros ? 1 : baseValue, upperBound, false);

        //    if (withZeros)
        //    {
        //        // any base value will do in this case.  is there a way to
        //        // force this to not bother trying all the base values?

        //        // compute the 'effective' base and prescale the value down
        //        long n = result.ToInt64();
        //        long d = 1;
        //        while (d <= n)
        //        {
        //            d *= 10;
        //        }
        //        // now add the zeros
        //        while (zeroCount > 0)
        //        {
        //            d *= 10;
        //            --zeroCount;
        //        }
        //        // d is now our true denominator
        //        result = Double.GetInstance(n / (double)d);
        //    }

        //    return result;
        //}

        /// <summary>
        /// Divides the result of parsing the substitution by the partial
        /// parse result.
        /// </summary>
        /// <param name="newRuleValue">The result of parsing the substitution.</param>
        /// <param name="oldRuleValue">The owning rule's base value.</param>
        /// <returns><paramref name="newRuleValue"/> / <paramref name="oldRuleValue"/>.</returns>
        public override double ComposeRuleValue(double newRuleValue, double oldRuleValue)
        {
            return newRuleValue / oldRuleValue;
        }

        /// <summary>
        /// Sets the upper bound down to this rule's base value.
        /// </summary>
        /// <param name="oldUpperBound">Ignored.</param>
        /// <returns>The base value of the rule owning this substitution.</returns>
        public override double CalcUpperBound(double oldUpperBound)
        {
            return denominator;
        }

        //-----------------------------------------------------------------------
        // simple accessor
        //-----------------------------------------------------------------------

        /// <summary>
        /// The token character for a NumeratorSubstitution is '&lt;'. Returns '&lt;'.
        /// </summary>
        private protected override char TokenChar => '<';
    }
}
