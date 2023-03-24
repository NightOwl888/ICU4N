using ICU4N.Numerics;
using J2N.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICU4N
{
    internal static partial class IcuNumber
    {
        ///// Parses the given pattern string and overwrites the settings specified in the pattern string.
        ///// The properties corresponding to the following setters are overwritten, either with their
        ///// default values or with the value specified in the pattern string:
        ///// 
        ///// <list type="bullet">
        /////     <item><description><see cref="DecimalSeparatorAlwaysShown"/></description></item>
        /////     <item><description><see cref="ExponentSignAlwaysShown"/></description></item>
        /////     <item><description><see cref="FormatWidth"/></description></item>
        /////     <item><description><see cref="GroupingSize"/></description></item>
        /////     <item><description><see cref="Multiplier"/>  (percent/permille)</description></item>
        /////     <item><description><see cref="MaximumFractionDigits"/></description></item>
        /////     <item><description><see cref="MaximumIntegerDigits"/></description></item>
        /////     <item><description><see cref="MaximumSignificantDigits"/></description></item>
        /////     <item><description><see cref="MinimumExponentDigits"/></description></item>
        /////     <item><description><see cref="MinimumFractionDigits"/></description></item>
        /////     <item><description><see cref="MinimumIntegerDigits"/></description></item>
        /////     <item><description><see cref="MinimumSignificantDigits"/></description></item>
        /////     <item><description><see cref="PadPosition"/></description></item>
        /////     <item><description><see cref="PadCharacter"/></description></item>
        /////     <item><description><see cref="RoundingIncrement"/></description></item>
        /////     <item><description><see cref="SecondaryGroupingSize"/></description></item>
        ///// </list>
        ///// All other settings remain untouched.

        // ICU4N TODO: This is a temporary workaround. This corresponds with GroupingSize/SecondaryGroupingSize.
        // We ought to be able to set all of the above properties on UNumberFormatInfo.
        // But, in .NET we need to convert these to a pattern string for the formatter to understand them (when possible).
        internal static int[] GetGroupingSizes(string pattern)
        {
            // This is how the group sizes are determined in ICU - need to deconstruct.
            PatternStringParser.ParsedPatternInfo patternInfo = PatternStringParser.ParseToPatternInfo(pattern);
            PatternStringParser.ParsedSubpatternInfo positive = patternInfo.positive;
            // Grouping settings
            short grouping1 = (short)(positive.groupingSizes & 0xffff);
            short grouping2 = (short)((positive.groupingSizes.TripleShift(16)) & 0xffff);
            short grouping3 = (short)((positive.groupingSizes.TripleShift(32)) & 0xffff);

            int groupingSize = grouping1 < 0 ? 0 : grouping1;
            int secondaryGroupingSize = grouping3 != -1 ? (grouping2 < 0 ? 0 : grouping2) : 0;
            if (groupingSize == 0 || secondaryGroupingSize == 0)
                return new int[] { groupingSize };

            return new int[] { groupingSize, secondaryGroupingSize };
        }
    }
}
