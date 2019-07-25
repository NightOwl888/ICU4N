using System;
using System.Collections.Generic;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Normalizers
{
    /// <summary>
    /// 
    /// </summary>
    public class UnicodeNormalizer
    {
        //    static final String copyright = "Copyright (C) 1998-2003 International Business Machines Corporation and Unicode, Inc.";

        /**
         * Create a normalizer for a given form.
         */
        public UnicodeNormalizer(byte form, bool fullData)
        {
            this.form = form;
            if (data == null) data = NormalizerBuilder.Build(fullData); // load 1st time
        }

        /**
        * Masks for the form selector
        */
        static readonly byte
            COMPATIBILITY_MASK = 1,
            COMPOSITION_MASK = 2;

        /**
        * Normalization Form Selector
        */
        public static readonly byte
            D = 0,
            C = COMPOSITION_MASK,
            KD = COMPATIBILITY_MASK,
            KC = (byte)(COMPATIBILITY_MASK + COMPOSITION_MASK);

        /**
        * Normalizes text according to the chosen form,
        * replacing contents of the target buffer.
        * @param   source      the original text, unnormalized
        * @param   target      the resulting normalized text
        */
        public StringBuffer normalize(String source, StringBuffer target)
        {

            // First decompose the source into target,
            // then compose if the form requires.

            if (source.Length != 0)
            {
                internalDecompose(source, target);
                if ((form & COMPOSITION_MASK) != 0)
                {
                    internalCompose(target);
                }
            }
            return target;
        }

        /**
        * Normalizes text according to the chosen form
        * @param   source      the original text, unnormalized
        * @return  target      the resulting normalized text
        */
        public String normalize(String source)
        {
            return normalize(source, new StringBuffer()).ToString();
        }

        // ======================================
        //                  PRIVATES
        // ======================================

        /**
         * The current form.
         */
        private byte form;

        /**
        * Decomposes text, either canonical or compatibility,
        * replacing contents of the target buffer.
        * @param   form        the normalization form. If COMPATIBILITY_MASK
        *                      bit is on in this byte, then selects the recursive
        *                      compatibility decomposition, otherwise selects
        *                      the recursive canonical decomposition.
        * @param   source      the original text, unnormalized
        * @param   target      the resulting normalized text
        */
        private void internalDecompose(String source, StringBuffer target)
        {
            StringBuffer buffer = new StringBuffer();
            bool canonical = (form & COMPATIBILITY_MASK) == 0;
            int ch;
            for (int i = 0; i < source.Length;)
            {
                buffer.Length=(0);
                ch = UTF16Util.NextCodePoint(source, i);
                i += UTF16Util.CodePointLength(ch);
                data.GetRecursiveDecomposition(canonical, ch, buffer);

                // add all of the characters in the decomposition.
                // (may be just the original character, if there was
                // no decomposition mapping)

                for (int j = 0; j < buffer.Length;)
                {
                    ch = UTF16Util.NextCodePoint(buffer, j);
                    j += UTF16Util.CodePointLength(ch);
                    int chClass = data.GetCanonicalClass(ch);
                    int k = target.Length; // insertion point
                    if (chClass != 0)
                    {

                        // bubble-sort combining marks as necessary

                        int ch2;
                        for (; k > 0; k -= UTF16Util.CodePointLength(ch2))
                        {
                            ch2 = UTF16Util.PrevCodePoint(target, k);
                            if (data.GetCanonicalClass(ch2) <= chClass) break;
                        }
                    }
                    UTF16Util.InsertCodePoint(target, k, ch);
                }
            }
        }

        /**
        * Composes text in place. Target must already
        * have been decomposed.
        * @param   target      input: decomposed text.
        *                      output: the resulting normalized text.
        */
        private void internalCompose(StringBuffer target)
        {

            int starterPos = 0;
            int starterCh = UTF16Util.NextCodePoint(target, 0);
            int compPos = UTF16Util.CodePointLength(starterCh);
            int lastClass = data.GetCanonicalClass(starterCh);
            if (lastClass != 0) lastClass = 256; // fix for irregular combining sequence

            // Loop on the decomposed characters, combining where possible

            for (int decompPos = UTF16Util.CodePointLength(starterCh); decompPos < target.Length;)
            {
                int ch = UTF16Util.NextCodePoint(target, decompPos);
                decompPos += UTF16Util.CodePointLength(ch);
                int chClass = data.GetCanonicalClass(ch);
                int composite = data.GetPairwiseComposition(starterCh, ch);
                if (composite != NormalizerData.NOT_COMPOSITE
                && (lastClass < chClass || lastClass == 0))
                {
                    UTF16Util.SetCodePointAt(target, starterPos, composite);
                    starterCh = composite;
                }
                else
                {
                    if (chClass == 0)
                    {
                        starterPos = compPos;
                        starterCh = ch;
                    }
                    lastClass = chClass;
                    decompPos += UTF16Util.SetCodePointAt(target, compPos, ch);
                    compPos += UTF16Util.CodePointLength(ch);
                }
            }
            target.Length=(compPos);
        }

        /**
        * Contains normalization data from the Unicode Character Database.
        * use false for the minimal set, true for the real set.
        */
        private static NormalizerData data = null;

        /**
        * Just accessible for testing.
        */
        internal bool GetExcluded(char ch)
        {
            return data.GetExcluded(ch);
        }

        /**
        * Just accessible for testing.
        */
        internal String GetRawDecompositionMapping(char ch)
        {
            return data.GetRawDecompositionMapping(ch);
        }
    }
}
