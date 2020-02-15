using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Text;
using J2N.Text;
using NUnit.Framework;
using System;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Normalizers
{
    public class TestDeprecatedNormalizerAPI : TestFmwk
    {
        public TestDeprecatedNormalizerAPI()
        {
        }

        [Test]
        public void TestNormalizerAPI()
        {
            // instantiate a Normalizer from a CharacterIterator
            string s = Utility.Unescape("a\u0308\uac00\\U0002f800");
            // make s a bit longer and more interesting
            CharacterIterator iter = new StringCharacterIterator(s + s);
            //test deprecated constructors
            Normalizer norm = new Normalizer(iter, NormalizerMode.NFC, 0);
            if (norm.Next() != 0xe4)
            {
                Errln("error in Normalizer(CharacterIterator).next()");
            }
            Normalizer norm2 = new Normalizer(s, NormalizerMode.NFC, 0);
            if (norm2.Next() != 0xe4)
            {
                Errln("error in Normalizer(CharacterIterator).next()");
            }
            // test clone(), ==, and hashCode()
            Normalizer clone = (Normalizer)norm.Clone();
            if (clone.GetBeginIndex() != norm.GetBeginIndex())
            {
                Errln("error in Normalizer.getBeginIndex()");
            }

            if (clone.GetEndIndex() != norm.GetEndIndex())
            {
                Errln("error in Normalizer.getEndIndex()");
            }

            // test setOption() and getOption()
            clone.SetOption(0xaa0000, true);
            clone.SetOption(0x20000, false);
            if (clone.GetOption(0x880000) == 0 || clone.GetOption(0x20000) == 1)
            {
                Errln("error in Normalizer::setOption() or Normalizer::getOption()");
            }

            // ICU4N specific - test setting normalizer options via enum
            clone.UnicodeVersion = NormalizerUnicodeVersion.Unicode3_2;
            assertEquals("error in Normalizer.UnicodeVersion property", NormalizerUnicodeVersion.Unicode3_2, clone.UnicodeVersion);
            clone.UnicodeVersion = NormalizerUnicodeVersion.Default;
            assertEquals("error in Normalizer.UnicodeVersion property", NormalizerUnicodeVersion.Default, clone.UnicodeVersion);

            //test deprecated normalize method
            Normalizer.Normalize(s, NormalizerMode.NFC, 0);
            //test deprecated compose method
            Normalizer.Compose(s, false, 0);
            //test deprecated decompose method
            Normalizer.Decompose(s, false, 0);

        }

        /**
         * Run through all of the characters returned by a composed-char iterator
         * and make sure that:
         * <ul>
         * <li>a) They do indeed have decompositions.</li>
         * <li>b) The decomposition according to the iterator is the same as
         *          returned by Normalizer.decompose().</li>
         * <li>c) All characters <em>not</em> returned by the iterator do not
         *          have decompositions.</li>
         * </ul>
         */
        [Test]
        public void TestComposedCharIter()
        {
            doTestComposedChars(false);
        }

        private void doTestComposedChars(bool compat)
        {
            int options = Normalizer.IGNORE_HANGUL;
            ComposedCharIter iter = new ComposedCharIter(compat, options);

            char lastChar = (char)0;

            while (iter.HasNext)
            {
                char ch = iter.Next();

                // Test all characters between the last one and this one to make
                // sure that they don't have decompositions
                assertNoDecomp(lastChar, ch, compat, options);
                lastChar = ch;

                // Now make sure that the decompositions for this character
                // make sense
                String chString = new StringBuffer().Append(ch).ToString();
                String iterDecomp = iter.Decomposition();
                String normDecomp = Normalizer.Decompose(chString, compat);

                if (iterDecomp.Equals(chString))
                {
                    Errln("ERROR: " + Hex(ch) + " has identical decomp");
                }
                else if (!iterDecomp.Equals(normDecomp))
                {
                    Errln("ERROR: Normalizer decomp for " + Hex(ch) + " (" + Hex(normDecomp) + ")"
                        + " != iter decomp (" + Hex(iterDecomp) + ")");
                }
            }
            assertNoDecomp(lastChar, '\uFFFF', compat, options);
        }

        void assertNoDecomp(char start, char limit, bool compat, int options)
        {
            for (char x = ++start; x < limit; x++)
            {
                String xString = new StringBuffer().Append(x).ToString();
                String decomp = Normalizer.Decompose(xString, compat);
                if (!decomp.Equals(xString))
                {
                    Errln("ERROR: " + Hex(x) + " has decomposition (" + Hex(decomp) + ")"
                        + " but was not returned by iterator");
                }
            }
        }


        [Test]
        public void TestRoundTrip()
        {
            int options = Normalizer.IGNORE_HANGUL;
            bool compat = false;

            ComposedCharIter iter = new ComposedCharIter(false, options);
            while (iter.HasNext)
            {
                char ch = iter.Next();

                string chStr = "" + ch;
                string decomp = iter.Decomposition();
                string comp = Normalizer.Compose(decomp, compat);

                if (UChar.HasBinaryProperty(ch, UProperty.Full_Composition_Exclusion))
                {
                    Logln("Skipped excluded char " + Hex(ch) + " (" + UChar.GetName(ch) + ")");
                    continue;
                }

                // Avoid disparaged characters
                if (decomp.Length == 4) continue;

                if (!comp.Equals(chStr))
                {
                    Errln("ERROR: Round trip invalid: " + Hex(chStr) + " --> " + Hex(decomp)
                        + " --> " + Hex(comp));

                    Errln("  char decomp is '" + decomp + "'");
                }
            }
        }
    }
}
