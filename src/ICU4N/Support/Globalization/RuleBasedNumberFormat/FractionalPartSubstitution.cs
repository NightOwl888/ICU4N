using ICU4N.Numerics;
using ICU4N.Support.Text;
using System;
using System.Diagnostics;
#nullable enable

namespace ICU4N.Globalization
{
#if FEATURE_SPAN
    //===================================================================
    // FractionalPartSubstitution
    //===================================================================

    /// <summary>
    /// A substitution that formats the fractional part of a number. This is
    /// represented by &gt;&gt; in a fraction rule.
    /// </summary>
    internal sealed class FractionalPartSubstitution : NumberFormatSubstitution
    {
        //-----------------------------------------------------------------------
        // data members
        //-----------------------------------------------------------------------

        /// <summary>
        /// <c>true</c> if this substitution should have the default "by digits"
        /// behavior; <c>false</c> otherwise.
        /// </summary>
        internal readonly bool byDigits; // Internal for testing

        /// <summary>
        /// <c>true</c> if we automatically insert spaces to separate names of digits
        /// set to <c>false</c> by '>>>' in fraction rules, used by Thai.
        /// </summary>
        internal readonly bool useSpaces; // Internal for testing

        //-----------------------------------------------------------------------
        // construction
        //-----------------------------------------------------------------------

        /// <summary>
        /// Constructs a <see cref="FractionalPartSubstitution"/>. This object keeps a flag
        /// telling whether it should format by digits or not. In addition,
        /// it marks the rule set it calls (if any) as a fraction rule set.
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
        internal FractionalPartSubstitution(int pos,
                NumberFormatRuleSet ruleSet,
                ReadOnlySpan<char> description)
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
                this.ruleSet!.MakeIntoFractionRuleSet();
            }
        }

        //-----------------------------------------------------------------------
        // formatting
        //-----------------------------------------------------------------------

        /// <summary>
        /// If in "by digits" mode, fills in the substitution one decimal digit
        /// at a time using the rule set containing this substitution.
        /// Otherwise, uses the superclass function.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <param name="toInsertInto">The string to insert the result of formatting
        /// the substitution into.</param>
        /// <param name="position">The position of the owning rule's rule text in
        /// <paramref name="toInsertInto"/>.</param>
        /// <param name="info">The <see cref="UNumberFormatInfo"/> that contains the culture specific number formatting settings.</param>
        /// <param name="recursionCount">The number of recursive calls to this method.</param>
        public override void DoSubstitution(double number, ref ValueStringBuilder toInsertInto, int position, UNumberFormatInfo info, int recursionCount)
        {
            Debug.Assert(info != null);

            if (!byDigits)
            {
                // if we're not in "byDigits" mode, just use the inherited
                // doSubstitution() routine
                base.DoSubstitution(number, ref toInsertInto, position, info!, recursionCount);
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
                    ruleSet!.Format(fq.GetDigit(mag++), ref toInsertInto, position + pos, info, recursionCount);
                }
            }
        }

        /// <summary>
        /// Returns the fractional part of the <paramref name="number"/>, which will always be
        /// zero if it's a <see cref="long"/>.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <returns>Always 0.</returns>
        public override long TransformNumber(long number)
        {
            return 0;
        }

        /// <summary>
        /// Returns the fractional part of the <paramref name="number"/>.
        /// </summary>
        /// <param name="number">The number being formatted.</param>
        /// <returns><paramref name="number"/> - Floor(<paramref name="number"/>).</returns>
        public override double TransformNumber(double number)
        {
            return number - Math.Floor(number);
        }

        //-----------------------------------------------------------------------
        // parsing
        //-----------------------------------------------------------------------

        // ICU4N TODO: Implementation
//        /// <summary>
//        /// If in "by digits" mode, parses the string as if it were a string
//        /// of individual digits; otherwise, uses the superclass function.
//        /// </summary>
//        /// <param name="text">The string to parse.</param>
//        /// <param name="parsePosition">Ignored on entry, but updated on exit to point
//        /// to the first unmatched character.</param>
//        /// <param name="baseValue">The partial parse result prior to entering this
//        /// function.</param>
//        /// <param name="upperBound">Only consider rules with base values lower than
//        /// this when filling in the substitution.</param>
//        /// <param name="lenientParse">If <c>true</c>, try matching the text as numerals if
//        /// matching as words doesn't work.</param>
//        /// <returns>If the match was successful, the current partial parse
//        /// result; otherwise <c>Long.GetInstance(0)</c>. The result is either a <see cref="Long"/> or
//        /// a <see cref="Double"/>.</returns>
//        public override Number DoParse(string text, ParsePosition parsePosition, double baseValue,
//                              double upperBound, bool lenientParse)
//        {
//            // if we're not in byDigits mode, we can just use the inherited
//            // doParse()
//            if (!byDigits)
//            {
//                return base.DoParse(text, parsePosition, baseValue, 0, lenientParse);
//            }
//            else
//            {
//                // if we ARE in byDigits mode, parse the text one digit at a time
//                // using this substitution's owning rule set (we do this by setting
//                // upperBound to 10 when calling doParse() ) until we reach
//                // nonmatching text
//                string workText = text;
//                ParsePosition workPos = new ParsePosition(1);
//                double result;
//                int digit;

//                DecimalQuantity_DualStorageBCD fq = new DecimalQuantity_DualStorageBCD();
//                int leadingZeros = 0;
//                while (workText.Length > 0 && workPos.Index != 0)
//                {
//                    workPos.Index = 0;
//                    digit = ruleSet.Parse(workText, workPos, 10).ToInt32();
//                    if (lenientParse && workPos.Index == 0)
//                    {
//                        Number n = ruleSet.owner.DecimalFormat.Parse(workText, workPos);
//                        if (n != null)
//                        {
//                            digit = n.ToInt32();
//                        }
//                    }

//                    if (workPos.Index != 0)
//                    {
//                        if (digit == 0)
//                        {
//                            leadingZeros++;
//                        }
//                        else
//                        {
//#pragma warning disable CS0618 // Type or member is obsolete
//                            fq.AppendDigit((byte)digit, leadingZeros, false);
//#pragma warning restore CS0618 // Type or member is obsolete
//                            leadingZeros = 0;
//                        }

//                        parsePosition.Index = parsePosition.Index + workPos.Index;
//                        workText = workText.Substring(workPos.Index);
//                        while (workText.Length > 0 && workText[0] == ' ')
//                        {
//                            workText = workText.Substring(1);
//                            parsePosition.Index = parsePosition.Index + 1;
//                        }
//                    }
//                }
//                result = fq.ToDouble();

//                result = ComposeRuleValue(result, baseValue);
//                return Double.GetInstance(result);
//            }
//        }

        /// <summary>
        /// Returns the sum of the two partial parse results.
        /// </summary>
        /// <param name="newRuleValue">The result of parsing the substitution.</param>
        /// <param name="oldRuleValue">The partial parse result prior to calling
        /// this function.</param>
        /// <returns><paramref name="newRuleValue"/> + <paramref name="oldRuleValue"/>.</returns>
        public override double ComposeRuleValue(double newRuleValue, double oldRuleValue)
        {
            return newRuleValue + oldRuleValue;
        }

        /// <summary>
        /// Not used.
        /// </summary>
        public override double CalcUpperBound(double oldUpperBound)
        {
            return 0;   // this value is ignored
        }

        //-----------------------------------------------------------------------
        // simple accessor
        //-----------------------------------------------------------------------

        /// <summary>
        /// The token character for a <see cref="FractionalPartSubstitution"/> is '&gt;'. Returns '&gt;'.
        /// </summary>
        private protected override char TokenChar => '>';
    }
#endif
}
