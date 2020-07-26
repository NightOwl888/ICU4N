using J2N.Collections;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Normalizers
{
    /// <summary>
    /// Accesses the Normalization Data used for Forms C and D.
    /// </summary>
    /// <author>
    /// Mark Davis
    /// Updates for supplementary code points:
    /// Vladimir Weinstein &amp; Markus Scher
    /// </author>
    public class NormalizerData
    {
        //    static final String copyright = "Copyright (C) 1998-2003 International Business Machines Corporation and Unicode, Inc.";

        /**
        * Constant for use in getPairwiseComposition
        */
        public static readonly int NOT_COMPOSITE = '\uFFFF';

        /**
        * Gets the combining class of a character from the
        * Unicode Character Database.
        * @param   ch      the source character
        * @return          value from 0 to 255
        */
        public int GetCanonicalClass(int ch)
        {
            return canonicalClass.Get(ch);
        }

        /**
        * Returns the composite of the two characters. If the two
        * characters don't combine, returns NOT_COMPOSITE.
        * @param   first   first character (e.g. 'c')
        * @param   second  second character (e.g. \u0327 cedilla)
        * @return          composite (e.g. \u00C7 c cedilla)
        */
        public int GetPairwiseComposition(int first, int second)
        {
            return compose.Get(((long)first << 32) | (uint)second);
        }


        /**
        * Gets recursive decomposition of a character from the
        * Unicode Character Database.
        * @param   canonical    If true
        *                  bit is on in this byte, then selects the recursive
        *                  canonical decomposition, otherwise selects
        *                  the recursive compatibility and canonical decomposition.
        * @param   ch      the source character
        * @param   buffer  buffer to be filled with the decomposition
        */
        public void GetRecursiveDecomposition(bool canonical, int ch, StringBuffer buffer)
        {
            string decomp = decompose.Get(ch);
            if (decomp != null && !(canonical && isCompatibility.Get(ch)))
            {
                for (int i = 0; i < decomp.Length; i += UTF16Util.CodePointLength(ch))
                {
                    ch = UTF16Util.NextCodePoint(decomp, i);
                    GetRecursiveDecomposition(canonical, ch, buffer);
                }
            }
            else
            {                    // if no decomp, append
                UTF16Util.AppendCodePoint(buffer, ch);
            }
        }

        // =================================================
        //                   PRIVATES
        // =================================================

        /**
         * Only accessed by NormalizerBuilder.
         */
        internal NormalizerData(IntHashtable canonicalClass, IntStringHashtable decompose,
            LongHashtable compose, BitSet isCompatibility, BitSet isExcluded)
        {
            this.canonicalClass = canonicalClass;
            this.decompose = decompose;
            this.compose = compose;
            this.isCompatibility = isCompatibility;
            this.isExcluded = isExcluded;
        }

        /**
        * Just accessible for testing.
        */
        internal bool GetExcluded(char ch)
        {
            return isExcluded.Get(ch);
        }

        /**
        * Just accessible for testing.
        */
        internal string GetRawDecompositionMapping(char ch)
        {
            return decompose.Get(ch);
        }

        /**
        * For now, just use IntHashtable
        * Two-stage tables would be used in an optimized implementation.
        */
        private IntHashtable canonicalClass;

        /**
        * The main data table maps chars to a 32-bit int.
        * It holds either a pair: top = first, bottom = second
        * or singleton: top = 0, bottom = single.
        * If there is no decomposition, the value is 0.
        * Two-stage tables would be used in an optimized implementation.
        * An optimization could also map chars to a small index, then use that
        * index in a small array of ints.
        */
        private IntStringHashtable decompose;

        /**
        * Maps from pairs of characters to single.
        * If there is no decomposition, the value is NOT_COMPOSITE.
        */
        private LongHashtable compose;

        /**
        * Tells whether decomposition is canonical or not.
        */
        private BitSet isCompatibility = new BitSet(1);

        /**
        * Tells whether character is script-excluded or not.
        * Used only while building, and for testing.
        */

        private BitSet isExcluded = new BitSet(1);
    }
}
