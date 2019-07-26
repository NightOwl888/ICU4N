using ICU4N.Globalization;
using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Globalization;

namespace ICU4N.Dev.Test.Normalizers
{
    public class NormalizationMonkeyTest : TestFmwk
    {
        int loopCount = 100;
        int maxCharCount = 20;
        int maxCodePoint = 0x10ffff;
        Random random = null; // initialized in getTestSource
        UnicodeNormalizer unicode_NFD;
        UnicodeNormalizer unicode_NFC;
        UnicodeNormalizer unicode_NFKD;
        UnicodeNormalizer unicode_NFKC;

        public NormalizationMonkeyTest()
        {
        }

        [Test]
        public void TestNormalize()
        {
            if (unicode_NFD == null)
            {
                try
                {
                    unicode_NFD = new UnicodeNormalizer(UnicodeNormalizer.D, true);
                    unicode_NFC = new UnicodeNormalizer(UnicodeNormalizer.C, true);
                    unicode_NFKD = new UnicodeNormalizer(UnicodeNormalizer.KD, true);
                    unicode_NFKC = new UnicodeNormalizer(UnicodeNormalizer.KC, true);
                }
                catch (Exception e)
                {
                    Errln("Normalization tests could not be run: " + e.ToString());
                }
            }
            int i = 0;
            while (i < loopCount)
            {
                String source = GetTestSource();
                Logln("Test source:" + source);
                //NFD
                String uncodeNorm = unicode_NFD.normalize(source);
                String icuNorm = Normalizer.Normalize(source, NormalizerMode.NFD);
                Logln("\tNFD(Unicode): " + uncodeNorm);
                Logln("\tNFD(icu4j)  : " + icuNorm);
                if (!uncodeNorm.Equals(icuNorm))
                {
                    Errln("NFD: Unicode sample output => " + uncodeNorm + "; icu4j output=> " + icuNorm);
                }
                //NFC
                uncodeNorm = unicode_NFC.normalize(source);
                icuNorm = Normalizer.Normalize(source, NormalizerMode.NFC);
                Logln("\tNFC(Unicode): " + uncodeNorm);
                Logln("\tNFC(icu4j)  : " + icuNorm);
                if (!uncodeNorm.Equals(icuNorm))
                {
                    Errln("NFC: Unicode sample output => " + uncodeNorm + "; icu4j output=> " + icuNorm);
                }
                //NFKD
                uncodeNorm = unicode_NFKD.normalize(source);
                icuNorm = Normalizer.Normalize(source, NormalizerMode.NFKD);
                Logln("\tNFKD(Unicode): " + uncodeNorm);
                Logln("\tNFKD(icu4j)  : " + icuNorm);
                if (!uncodeNorm.Equals(icuNorm))
                {
                    Errln("NFKD: Unicode sample output => " + uncodeNorm + "; icu4j output=> " + icuNorm);
                }
                //NFKC
                uncodeNorm = unicode_NFKC.normalize(source);
                icuNorm = Normalizer.Normalize(source, NormalizerMode.NFKC);
                Logln("\tNFKC(Unicode): " + uncodeNorm);
                Logln("\tNFKC(icu4j)  : " + icuNorm);
                if (!uncodeNorm.Equals(icuNorm))
                {
                    Errln("NFKC: Unicode sample output => " + uncodeNorm + "; icu4j output=> " + icuNorm);
                }

                i++;
            }
        }

        internal String GetTestSource()
        {
            if (random == null)
            {
                random = CreateRandom(); // use test framework's random seed
            }
            String source = "";
            int i = 0;
            while (i < (random.Next(maxCharCount) + 1))
            {
                int codepoint = random.Next(maxCodePoint);
                //Elimate unassigned characters
                while (UChar.GetUnicodeCategory(codepoint) == UUnicodeCategory.OtherNotAssigned)
                {
                    codepoint = random.Next(maxCodePoint);
                }
                source = source + UTF16.ValueOf(codepoint);
                i++;
            }
            return source;
        }
    }
}
