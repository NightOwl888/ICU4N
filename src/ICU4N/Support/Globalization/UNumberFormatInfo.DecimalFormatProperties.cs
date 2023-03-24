using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICU4N.Globalization
{
    public sealed partial class UNumberFormatInfo
    {
        internal int[] numberGroupSizes = new int[] { 3 };

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
    }
}
