using System;
using System.Diagnostics;
using static ICU4N.Numerics.PatternStringParser;

namespace ICU4N.Numerics
{
    /// <internal/>
    [Obsolete("This API is a technical preview. It is likely to change in an upcoming release.")]
    internal class Grouper // ICU4N TODO: API - this was public in ICU4J
    {
        // Conveniences for Java handling of bytes
        private const sbyte N2 = -2;
        private const sbyte N1 = -1;
        private const sbyte B2 = 2;
        private const sbyte B3 = 3;

        private static readonly Grouper DEFAULTS = new Grouper(N2, N2, false);
        private static readonly Grouper MIN2 = new Grouper(N2, N2, true);
        private static readonly Grouper NONE = new Grouper(N1, N1, false);

        private readonly sbyte grouping1; // -2 means "needs locale data"; -1 means "no grouping"
        private readonly sbyte grouping2;
        private readonly bool min2;

        private Grouper(sbyte grouping1, sbyte grouping2, bool min2)
        {
            this.grouping1 = grouping1;
            this.grouping2 = grouping2;
            this.min2 = min2;
        }

        /**
         * @internal
         * @deprecated This API is a technical preview. It is likely to change in an upcoming release.
         */
        [Obsolete("This API is a technical preview. It is likely to change in an upcoming release.")]
        public static Grouper Defaults => DEFAULTS;

        /**
         * @internal
         * @deprecated This API is a technical preview. It is likely to change in an upcoming release.
         */
        [Obsolete("This API is a technical preview. It is likely to change in an upcoming release.")]
        public static Grouper MinTwoDigits => MIN2;

        /**
         * @internal
         * @deprecated This API is a technical preview. It is likely to change in an upcoming release.
         */
        [Obsolete("This API is a technical preview. It is likely to change in an upcoming release.")]
        public static Grouper None => NONE;

        //////////////////////////
        // PACKAGE-PRIVATE APIS //
        //////////////////////////

        private static readonly Grouper GROUPING_3 = new Grouper(B3, B3, false);
        private static readonly Grouper GROUPING_3_2 = new Grouper(B3, B2, false);
        private static readonly Grouper GROUPING_3_MIN2 = new Grouper(B3, B3, true);
        private static readonly Grouper GROUPING_3_2_MIN2 = new Grouper(B3, B2, true);

        internal static Grouper GetInstance(sbyte grouping1, sbyte grouping2, bool min2)
        {
            if (grouping1 == -1)
            {
                return NONE;
            }
            else if (!min2 && grouping1 == 3 && grouping2 == 3)
            {
                return GROUPING_3;
            }
            else if (!min2 && grouping1 == 3 && grouping2 == 2)
            {
                return GROUPING_3_2;
            }
            else if (min2 && grouping1 == 3 && grouping2 == 3)
            {
                return GROUPING_3_MIN2;
            }
            else if (min2 && grouping1 == 3 && grouping2 == 2)
            {
                return GROUPING_3_2_MIN2;
            }
            else
            {
                return new Grouper(grouping1, grouping2, min2);
            }
        }

        internal Grouper WithLocaleData(ParsedPatternInfo patternInfo)
        {
            if (this.grouping1 != -2)
            {
                return this;
            }
            // TODO: short or byte?
            sbyte grouping1 = (sbyte)(patternInfo.positive.groupingSizes & 0xffff);
            sbyte grouping2 = (sbyte)((patternInfo.positive.groupingSizes >>> 16) & 0xffff);
            sbyte grouping3 = (sbyte)((patternInfo.positive.groupingSizes >>> 32) & 0xffff);
            if (grouping2 == -1)
            {
                grouping1 = -1;
            }
            if (grouping3 == -1)
            {
                grouping2 = grouping1;
            }
            return GetInstance(grouping1, grouping2, min2);
        }

        internal bool GroupAtPosition(int position, IDecimalQuantity value)
        {
            Debug.Assert(grouping1 != -2);
            if (grouping1 == -1 || grouping1 == 0)
            {
                // Either -1 or 0 means "no grouping"
                return false;
            }
            position -= grouping1;
            return position >= 0 && (position % grouping2) == 0
                    && value.UpperDisplayMagnitude - grouping1 + 1 >= (min2 ? 2 : 1);
        }
    }
}
