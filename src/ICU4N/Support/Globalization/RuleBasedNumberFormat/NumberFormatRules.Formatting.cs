using ICU4N.Support.Text;
using System.Diagnostics;

namespace ICU4N.Globalization
{
#if FEATURE_SPAN
    public sealed partial class NumberFormatRules
    {
        private const int CharStackBufferSize = 128;

        //-----------------------------------------------------------------------
        // formatting implementation
        //-----------------------------------------------------------------------

        /**
         * Bottleneck through which all the public format() methods
         * that take a long pass. By the time we get here, we know
         * which rule set we're using to do the formatting.
         * @param number The number to format
         * @param ruleSet The rule set to use to format the number
         * @return The text that resulted from formatting the number
         */
        private string Format(long number, NumberFormatRuleSet ruleSet, UNumberFormatInfo info)
        {
            Debug.Assert(ruleSet != null);
            Debug.Assert(info != null);

            // all API format() routines that take a double vector through
            // here.  We have these two identical functions-- one taking a
            // double and one taking a long-- the couple digits of precision
            // that long has but double doesn't (both types are 8 bytes long,
            // but double has to borrow some of the mantissa bits to hold
            // the exponent).
            // Create an empty string buffer where the result will
            // be built, and pass it to the rule set (along with an insertion
            // position of 0 and the number being formatted) to the rule set
            // for formatting
            var result = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            if (number == long.MinValue)
            {
                // We can't handle this value right now. Provide an accurate default value.
                //result.Append(DecimalFormat.Format(long.MinValue));
                result.Append(IcuNumber.FormatInt64(long.MinValue, info.CultureData.decimalFormat, info));
            }
            else
            {
                ruleSet.Format(number, ref result, pos: 0, info, recursionCount: 0);
            }
            PostProcess(ref result, ruleSet, info);
            return result.ToString();
        }

        /**
         * Bottleneck through which all the public format() methods
         * that take a double pass. By the time we get here, we know
         * which rule set we're using to do the formatting.
         * @param number The number to format
         * @param ruleSet The rule set to use to format the number
         * @return The text that resulted from formatting the number
         */
        private string Format(double number, NumberFormatRuleSet ruleSet, UNumberFormatInfo info)
        {
            Debug.Assert(ruleSet != null);
            Debug.Assert(info != null);

            // all API format() routines that take a double vector through
            // here.  Create an empty string buffer where the result will
            // be built, and pass it to the rule set (along with an insertion
            // position of 0 and the number being formatted) to the rule set
            // for formatting
            var result = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);

            //if (RoundingMode != Numerics.BigMath.RoundingMode.Unnecessary && !double.IsNaN(number) && !double.IsInfinity(number))
            //{
            //    // We convert to a string because BigDecimal insists on excessive precision.
            //    number = Numerics.BigDecimal.Parse(Double.ToString(number, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture).SetScale(MaximumFractionDigits, roundingMode.ToICURoundingMode()).ToDouble();
            //    //RoundingModeExtensions
            //}
            // ICU4N TODO: See about using the .NET NumberBuffer here so we can share business logic between data types.

            ruleSet.Format(number, ref result, pos: 0, info, recursionCount: 0);
            PostProcess(ref result, ruleSet, info);
            return result.ToString();
        }

        private void PostProcess(ref ValueStringBuilder result, NumberFormatRuleSet ruleSet, UNumberFormatInfo info)
        {
            Debug.Assert(ruleSet != null);
            Debug.Assert(info != null);

            if (postProcessRules is not null)
            {
                // ICU4N TODO: Implementation
            }
        }
    }
#endif
}
