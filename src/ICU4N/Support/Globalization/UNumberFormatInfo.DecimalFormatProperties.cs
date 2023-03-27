using ICU4N.Numerics;
using System;

namespace ICU4N.Globalization
{
    public sealed partial class UNumberFormatInfo
    {
        internal NumberPatternStringProperties decimalPatternProperties; // The settings parsed from decimalFormat pattern string

        internal int[] numberGroupSizes = new int[] { 3 };
        internal int numberMaximumDecimalDigits = 2; // ICU4N TODO: Check default value for invariant
        internal int numberMinimumDecimalDigits = 2; // ICU4N TODO: Check default value for invariant
        //internal int currencyDecimalDigits = 2;
        //internal int percentDecimalDigits = 2;


        /// <summary>
        /// Check the values of the groupSize array.
        /// Every element in the groupSize array should be between 1 and 9
        /// except the last element could be zero.
        /// </summary>
        internal static void CheckGroupSize(string propName, int[] groupSize)
        {
            for (int i = 0; i < groupSize.Length; i++)
            {
                if (groupSize[i] < 1)
                {
                    if (i == groupSize.Length - 1 && groupSize[i] == 0)
                    {
                        return;
                    }

                    throw new ArgumentException(SR.Argument_InvalidGroupSize, propName);
                }
                else if (groupSize[i] > 9)
                {
                    throw new ArgumentException(SR.Argument_InvalidGroupSize, propName);
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of digits in each group to the left of the decimal in numeric values.
        /// </summary>
        /// <value>The number of digits in each group to the left of the decimal in numeric values. The default
        /// for <see cref="InvariantInfo"/> is a one-dimensional array with only one element, which is set to 3.</value>
        /// <exception cref="ArgumentNullException">The property is being set to <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// The property is being set and the array contains an entry that is less than 0 or greater than 9.
        /// <para/>
        /// -or-
        /// <para/>
        /// The property is being set and the array contains an entry, other than the last entry, that is set to 0.
        /// </exception>
        /// <exception cref="InvalidOperationException">The property is being set and the <see cref="UNumberFormatInfo"/>
        /// object is read-only.</exception>
        public int[] NumberGroupSizes
        {
            get => (int[])numberGroupSizes.Clone();
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value));

                VerifyWritable();

                int[] inputSizes = (int[])value.Clone();
                CheckGroupSize(nameof(value), inputSizes);
                numberGroupSizes = inputSizes;
            }
        }

        internal int[] NumberGroupSizesLocal => numberGroupSizes;
        internal string NumberPattern => CultureData.decimalFormat;

        public int NumberMinimumDecimalDigits
        {
            get => numberMinimumDecimalDigits;
            set
            {
                // ICU4N specific - added guard clauses instead of putting in "corrective" side effects
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), SR.ArgumentOutOfRange_NeedNonNegNum);
                if (value > NumberMaximumDecimalDigits)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        string.Format(SR.ArgumentOutOfRange_MinDigits, nameof(NumberMinimumDecimalDigits), nameof(NumberMaximumDecimalDigits)));

                VerifyWritable();
                numberMinimumDecimalDigits = value;
            }
        }

        public int NumberMaximumDecimalDigits
        {
            get => numberMaximumDecimalDigits;
            set
            {
                // ICU4N specific - added guard clause instead of putting in "corrective" side effects
                if (value < numberMinimumDecimalDigits)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        string.Format(SR.ArgumentOutOfRange_MaxDigits, nameof(NumberMaximumDecimalDigits), nameof(NumberMinimumDecimalDigits)));
                // ICU4N TODO: Adding the same limit as in .NET of 99. Once we stop using the .NET
                // formatter, we can eliminate this check, as ICU has no such limitation.
                if (value > 99)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        string.Format(SR.ArgumentOutOfRange_MinDigits, nameof(NumberMinimumDecimalDigits), 99));

                VerifyWritable();
                numberMaximumDecimalDigits = value;
            }
        }

    }
}
