using ICU4N.Impl;
using ICU4N.Lang;
using ICU4N.Support;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Translit
{
    /// <summary>
    /// Round trip test of Transliterator
    /// </summary>
    public class RoundTripTest : TestFmwk
    {
        //TODO - revisit test cases referencing FIX_ME
        internal static readonly bool FIX_ME = false;

        internal static readonly bool EXTRA_TESTS = true;
        internal static readonly bool PRINT_RULES = true;

        /*
        public void TestSingle() throws IOException, ParseException {
            Transliterator t = Transliterator.GetInstance("Latin-Greek");
            String s = t.Transliterate("\u0101\u0069");
        }
         */

        /*
        Note: Unicode 3.2 added new Hiragana/Katakana characters:

        3095..3096    ; 3.2 #   [2] HIRAGANA LETTER SMALL KA..HIRAGANA LETTER SMALL KE
        309F..30A0    ; 3.2 #   [2] HIRAGANA DIGRAPH YORI..KATAKANA-HIRAGANA DOUBLE HYPHEN
        30FF          ; 3.2 #       KATAKANA DIGRAPH KOTO
        31F0..31FF    ; 3.2 #  [16] KATAKANA LETTER SMALL KU..KATAKANA LETTER SMALL RO

            Unicode 5.2 added another Hiragana character:
        1F200         ; 5.2 #       SQUARE HIRAGANA HOKA

        We will not add them to the rules until they are more supported (e.g. in fonts on Windows)
        A bug has been filed to remind us to do this: #1979.
         */

        internal static String KATAKANA = "[[[:katakana:][\u30A1-\u30FA\u30FC]]-[\u30FF\u31F0-\u31FF]-[:^age=5.2:]]";
        internal static String HIRAGANA = "[[[:hiragana:][\u3040-\u3094]]-[\u3095-\u3096\u309F-\u30A0\\U0001F200-\\U0001F2FF]-[:^age=5.2:]]";
        internal static String LENGTH = "[\u30FC]";
        internal static String HALFWIDTH_KATAKANA = "[\uFF65-\uFF9D]";
        internal static String KATAKANA_ITERATION = "[\u30FD\u30FE]";
        internal static String HIRAGANA_ITERATION = "[\u309D\u309E]";

        //------------------------------------------------------------------
        // AbbreviatedUnicodeSetIterator
        //------------------------------------------------------------------

        internal class AbbreviatedUnicodeSetIterator : UnicodeSetIterator
        {

            private bool abbreviated;
            private int perRange;

            public AbbreviatedUnicodeSetIterator()
                    : base()
            {
                abbreviated = false;
            }

            public override void Reset(UnicodeSet newSet)
            {
                Reset(newSet, false);
            }

            public void Reset(UnicodeSet newSet, bool abb)
            {
                Reset(newSet, abb, 100);
            }

            public void Reset(UnicodeSet newSet, bool abb, int density)
            {
                base.Reset(newSet);
                abbreviated = abb;
                perRange = newSet.RangeCount;
                if (perRange != 0)
                {
                    perRange = density / perRange;
                }
            }

            protected override void LoadRange(int myRange)
            {
                base.LoadRange(myRange);
                if (abbreviated && (endElement > nextElement + perRange))
                {
                    endElement = nextElement + perRange;
                }
            }
        }

        //--------------------------------------------------------------------

        public void ShowElapsed(long start, String name)
        {
            double dur = (Time.CurrentTimeMilliseconds() - start) / 1000.0;
            Logln(name + " took " + dur + " seconds");
        }

        [Test]
        public void TestKana()
        {
            long start = Time.CurrentTimeMilliseconds();
            new TransliterationTest("Katakana-Hiragana")
            .Test(KATAKANA, "[" + HIRAGANA + LENGTH + "]", "[" + HALFWIDTH_KATAKANA + LENGTH + "]", this, new Legal());
            ShowElapsed(start, "TestKana");
        }

        [Test]
        public void TestHiragana()
        {
            long start = Time.CurrentTimeMilliseconds();
            new TransliterationTest("Latin-Hiragana")
            .Test("[a-zA-Z]", HIRAGANA, HIRAGANA_ITERATION, this, new Legal());
            ShowElapsed(start, "TestHiragana");
        }

        [Test]
        public void TestKatakana()
        {
            long start = Time.CurrentTimeMilliseconds();
            new TransliterationTest("Latin-Katakana")
            .Test("[a-zA-Z]", KATAKANA, "[" + KATAKANA_ITERATION + HALFWIDTH_KATAKANA + "]", this, new Legal());
            ShowElapsed(start, "TestKatakana");
        }

        [Test]
        public void TestJamo()
        {
            long start = Time.CurrentTimeMilliseconds();
            new TransliterationTest("Latin-Jamo")
            .Test("[a-zA-Z]", "[\u1100-\u1112 \u1161-\u1175 \u11A8-\u11C2]", "", this, new LegalJamo());
            ShowElapsed(start, "TestJamo");
        }

        /*
            SBase = 0xAC00, LBase = 0x1100, VBase = 0x1161, TBase = 0x11A7,
            LCount = 19, VCount = 21, TCount = 28,
            NCount = VCount * TCount,   // 588
            SCount = LCount * NCount,   // 11172
            LLimit = LBase + LCount,    // 1113
            VLimit = VBase + VCount,    // 1176
            TLimit = TBase + TCount,    // 11C3
            SLimit = SBase + SCount;    // D7A4
         */

        [Test]
        public void TestHangul()
        {
            long start = Time.CurrentTimeMilliseconds();
            TransliterationTest t = new TransliterationTest("Latin-Hangul", 5);
            bool TEST_ALL = GetBooleanProperty("HangulRoundTripAll", false);
            if (TEST_ALL && TestFmwk.GetExhaustiveness() == 10)
            {
                t.SetPairLimit(int.MaxValue); // only go to the limit if we have TEST_ALL and getInclusion
            }
            t.Test("[a-zA-Z]", "[\uAC00-\uD7A4]", "", this, new Legal());
            ShowElapsed(start, "TestHangul");
        }

        /**
         * This is a shorter version of the test for doubles, that allows us to skip lots of cases, but
         * does check the ones that should cause problems (if any do).
         */
        [Test]
        public void TestHangul2()
        {
            Transliterator lh = Transliterator.GetInstance("Latin-Hangul");
            Transliterator hl = lh.GetInverse();
            UnicodeSet representativeHangul = GetRepresentativeHangul();
            for (UnicodeSetIterator it = new UnicodeSetIterator(representativeHangul); it.Next();)
            {
                AssertRoundTripTransform("Transform", it.GetString(), lh, hl);
            }
        }

        private void AssertRoundTripTransform(String message, String source, Transliterator lh, Transliterator hl)
        {
            String to = hl.Transform(source);
            String back = lh.Transform(to);
            if (!source.Equals(back))
            {
                String to2 = hl.Transform(Regex.Replace(source, "(.)", "$1 ").Trim());
                String to3 = hl.Transform(Regex.Replace(back, "(.)", "$1 ").Trim());
                assertEquals(message + " " + source + " [" + to + "/" + to2 + "/" + to3 + "]", source, back);
            }
        }

        public static UnicodeSet GetRepresentativeHangul()
        {
            UnicodeSet extraSamples = new UnicodeSet("[\uCE20{\uAD6C\uB514}{\uAD73\uC774}{\uBB34\uB837}{\uBB3C\uC5FF}{\uC544\uAE4C}{\uC544\uB530}{\uC544\uBE60}{\uC544\uC2F8}{\uC544\uC9DC}{\uC544\uCC28}{\uC545\uC0AC}{\uC545\uC2F8}{\uC546\uCE74}{\uC548\uAC00}{\uC548\uC790}{\uC548\uC9DC}{\uC548\uD558}{\uC54C\uAC00}{\uC54C\uB530}{\uC54C\uB9C8}{\uC54C\uBC14}{\uC54C\uBE60}{\uC54C\uC0AC}{\uC54C\uC2F8}{\uC54C\uD0C0}{\uC54C\uD30C}{\uC54C\uD558}{\uC555\uC0AC}{\uC555\uC2F8}{\uC558\uC0AC}{\uC5C5\uC12F\uC501}{\uC5C6\uC5C8\uC2B5}]");
            UnicodeSet sourceSet = new UnicodeSet();
            AddRepresentativeHangul(sourceSet, 2, false);
            AddRepresentativeHangul(sourceSet, 3, false);
            AddRepresentativeHangul(sourceSet, 2, true);
            AddRepresentativeHangul(sourceSet, 3, true);
            // add the boundary cases; we want an example of each case of V + L and one example of each case of T+L

            UnicodeSet more = GetRepresentativeBoundaryHangul();
            sourceSet.AddAll(more);
            sourceSet.AddAll(extraSamples);
            return sourceSet;
        }

        private static UnicodeSet GetRepresentativeBoundaryHangul()
        {
            UnicodeSet resultToAddTo = new UnicodeSet();
            // U+1100 HANGUL CHOSEONG KIYEOK
            // U+1161 HANGUL JUNGSEONG A
            UnicodeSet L = new UnicodeSet("[:hst=L:]");
            UnicodeSet V = new UnicodeSet("[:hst=V:]");
            UnicodeSet T = new UnicodeSet("[:hst=T:]");

            String prefixLV = "\u1100\u1161";
            String prefixL = "\u1100";
            String suffixV = "\u1161";
            String nullL = "\u110B"; // HANGUL CHOSEONG IEUNG

            UnicodeSet L0 = new UnicodeSet("[\u1100\u110B]");

            // do all combinations of L0 + V + nullL + V

            for (UnicodeSetIterator iL0 = new UnicodeSetIterator(L0); iL0.Next();)
            {
                for (UnicodeSetIterator iV = new UnicodeSetIterator(V); iV.Next();)
                {
                    for (UnicodeSetIterator iV2 = new UnicodeSetIterator(V); iV2.Next();)
                    {
                        String sample = iL0.GetString() + iV.GetString() + nullL + iV2.GetString();
                        String trial = Normalizer.Compose(sample, false);
                        if (trial.Length == 2)
                        {
                            resultToAddTo.Add(trial);
                        }
                    }
                }
            }

            for (UnicodeSetIterator iL = new UnicodeSetIterator(L); iL.Next();)
            {
                // do all combinations of "g" + V + L + "a"
                String suffix = iL.GetString() + suffixV;
                for (UnicodeSetIterator iV = new UnicodeSetIterator(V); iV.Next();)
                {
                    String sample = prefixL + iV.GetString() + suffix;
                    String trial = Normalizer.Compose(sample, false);
                    if (trial.Length == 2)
                    {
                        resultToAddTo.Add(trial);
                    }
                }
                // do all combinations of "ga" + T + L + "a"
                for (UnicodeSetIterator iT = new UnicodeSetIterator(T); iT.Next();)
                {
                    String sample = prefixLV + iT.GetString() + suffix;
                    String trial = Normalizer.Compose(sample, false);
                    if (trial.Length == 2)
                    {
                        resultToAddTo.Add(trial);
                    }
                }
            }
            return resultToAddTo;
        }

        private static void AddRepresentativeHangul(UnicodeSet resultToAddTo, int leng, bool noFirstConsonant)
        {
            UnicodeSet notYetSeen = new UnicodeSet();
            for (char c = '\uAC00'; c < '\uD7AF'; ++c)
            {
                String charStr = c + "";
                String decomp = Normalizer.Decompose(charStr, false);
                if (decomp.Length != leng)
                {
                    continue; // only take one length at a time
                }
                if (decomp.StartsWith("\u110B ", StringComparison.Ordinal) != noFirstConsonant)
                {
                    continue;
                }
                if (!notYetSeen.ContainsAll(decomp))
                {
                    resultToAddTo.Add(c);
                    notYetSeen.AddAll(decomp);
                }
            }
        }


        // ICU4N TODO: This test runs in 3 seconds in Java, but takes about 1 minute in .NET
        [Test]
        public void TestHan()
        {
            try
            {
                UnicodeSet exemplars = LocaleData.GetExemplarSet(new ULocale("zh"), 0);
                // create string with all chars
                StringBuffer b = new StringBuffer();
                for (UnicodeSetIterator it = new UnicodeSetIterator(exemplars); it.Next();)
                {
                    UTF16.Append(b, it.Codepoint);
                }
                String source = b.ToString();
                // transform with Han translit
                Transliterator han = Transliterator.GetInstance("Han-Latin");
                String target = han.Transliterate(source);
                // now verify that there are no Han characters left
                UnicodeSet allHan = new UnicodeSet("[:han:]");
                assertFalse("No Han must be left after Han-Latin transliteration", allHan.ContainsSome(target));
                // check the pinyin translit
                Transliterator pn = Transliterator.GetInstance("Latin-NumericPinyin");
                String target2 = pn.Transliterate(target);
                // verify that there are no marks
                Transliterator nfc = Transliterator.GetInstance("nfc");
                String nfced = nfc.Transliterate(target2);
                UnicodeSet allMarks = new UnicodeSet("[:mark:]");
                assertFalse("NumericPinyin must contain no marks", allMarks.ContainsSome(nfced));
                // verify roundtrip
                Transliterator np = pn.GetInverse();
                String target3 = np.Transliterate(target);
                bool roundtripOK = target3.Equals(target);
                assertTrue("NumericPinyin must roundtrip", roundtripOK);
                if (!roundtripOK)
                {
                    String filename = "numeric-pinyin.log.txt";
                    using (TextWriter @out = new StreamWriter(
                                                            new FileStream(filename, FileMode.Create, FileAccess.Write), Encoding.UTF8))
                    {
                        Errln("Creating log file " + new FileInfo(filename).FullName);
                        @out.WriteLine("Pinyin:                " + target);
                        @out.WriteLine("Pinyin-Numeric-Pinyin: " + target2);
                    }
                }
            }
            catch (MissingManifestResourceException ex)
            {
                Warnln("Could not load the locale data for fetching the exemplar characters." + ex.ToString());
            }
        }

        [Test]
        public void TestSingle()
        {
            Transliterator t = Transliterator.GetInstance("Latin-Greek");
            t.Transliterate("\u0061\u0101\u0069");
        }

        internal string GetGreekSet()
        {
            if (FIX_ME)
            {
                Errln("TestGreek needs to be updated to remove delete the [:Age=4.0:] filter ");
            }
            else
            {
                // We temporarily filter against Unicode 4.1, but we only do this
                // before version 3.5.
                Logln("TestGreek needs to be updated to remove delete the section marked [:Age=4.0:] filter");
            }

            return
                    "[\u003B\u00B7[[:Greek:]&[:Letter:]]-[" +
                    "\u1D26-\u1D2A" + // L&   [5] GREEK LETTER SMALL CAPITAL GAMMA..GREEK LETTER SMALL CAPITAL PSI
                    "\u1D5D-\u1D61" + // Lm   [5] MODIFIER LETTER SMALL BETA..MODIFIER LETTER SMALL CHI
                    "\u1D66-\u1D6A" + // L&   [5] GREEK SUBSCRIPT SMALL LETTER BETA..GREEK SUBSCRIPT SMALL LETTER CHI
                    "\u03D7-\u03EF" + // \N{GREEK KAI SYMBOL}..\N{COPTIC SMALL LETTER DEI}
                    "] & [:Age=4.0:]]";
        }

        [Test]
        public void TestGreek()
        {
            long start = Time.CurrentTimeMilliseconds();
            new TransliterationTest("Latin-Greek", 50)
            .Test("[a-zA-Z]", GetGreekSet(),
                    "[\u00B5\u037A\u03D0-\u03F5\u03F9]", /* roundtrip exclusions */
                    this, new LegalGreek(true));
            ShowElapsed(start, "TestGreek");
        }

        [Test]
        public void TestGreekUNGEGN()
        {
            long start = Time.CurrentTimeMilliseconds();
            new TransliterationTest("Latin-Greek/UNGEGN")
            .Test("[a-zA-Z]", GetGreekSet(),
                    "[\u00B5\u037A\u03D0-\uFFFF{\u039C\u03C0}]", /* roundtrip exclusions */
                    this, new LegalGreek(false));
            ShowElapsed(start, "TestGreekUNGEGN");
        }

        [Test]
        public void Testel()
        {
            long start = Time.CurrentTimeMilliseconds();
            new TransliterationTest("Latin-el")
            .Test("[a-zA-Z]", GetGreekSet(),
                    "[\u00B5\u037A\u03D0-\uFFFF{\u039C\u03C0}]", /* roundtrip exclusions */
                    this, new LegalGreek(false));
            ShowElapsed(start, "Testel");
        }

        [Test]
        public void TestCyrillic()
        {
            long start = Time.CurrentTimeMilliseconds();
            new TransliterationTest("Latin-Cyrillic")
            .Test("[a-zA-Z\u0110\u0111\u02BA\u02B9]", "[\u0400-\u045F]", null, this, new Legal());
            ShowElapsed(start, "TestCyrillic");
        }

        static readonly String ARABIC = "[\u06A9\u060C\u061B\u061F\u0621\u0627-\u063A\u0641-\u0655\u0660-\u066C\u067E\u0686\u0698\u06A4\u06AD\u06AF\u06CB-\u06CC\u06F0-\u06F9]";

        [Test]
        public void TestArabic()
        {
            long start = Time.CurrentTimeMilliseconds();
            new TransliterationTest("Latin-Arabic")
            .Test("[a-zA-Z\u02BE\u02BF]", ARABIC, "[a-zA-Z\u02BE\u02BF\u207F]", null, this, new Legal()); //
            ShowElapsed(start, "TestArabic");
        }

        [Test]
        public void TestHebrew()
        {
            if (FIX_ME)
            {
                Errln("TestHebrew needs to be updated to remove delete the [:Age=4.0:] filter ");
            }
            else
            {
                // We temporarily filter against Unicode 4.1, but we only do this
                // before version 3.5.
                Logln("TestHebrew needs to be updated to remove delete the section marked [:Age=4.0:] filter");
            }
            long start = Time.CurrentTimeMilliseconds();
            new TransliterationTest("Latin-Hebrew")
            .Test("[a-zA-Z\u02BC\u02BB]", "[[[:hebrew:]-[\u05BD\uFB00-\uFBFF]]& [:Age=4.0:]]", "[\u05F0\u05F1\u05F2]", this, new LegalHebrew());
            ShowElapsed(start, "TestHebrew");
        }

        [Test]
        public void TestThai()
        {
            long start = Time.CurrentTimeMilliseconds();
            if (FIX_ME)
            {
                new TransliterationTest("Latin-Thai")
                .Test("[a-zA-Z\u0142\u1ECD\u00E6\u0131\u0268\u02CC]",
                        "[\u0E01-\u0E3A\u0E40-\u0E5B]",
                        "[a-zA-Z\u0142\u1ECD\u00E6\u0131\u0268\u02B9\u02CC]",
                        null, this, new LegalThai());
            }
            else
            {
                new TransliterationTest("Latin-Thai")
                .Test("[a-zA-Z\u0142\u1ECD\u00E6\u0131\u0268\u02CC]",
                        "[\u0E01-\u0E3A\u0E40-\u0E5B]",
                        "[a-zA-Z\u0142\u1ECD\u00E6\u0131\u0268\u02B9\u02CC]",
                        "[\u0E4F]", this, new LegalThai());
            }

            ShowElapsed(start, "TestThai");
        }

        //----------------------------------
        // Inter-Indic Tests
        //----------------------------------
        public class LegalIndic : Legal
        {
            UnicodeSet vowelSignSet = new UnicodeSet();

            public LegalIndic()
            {
                vowelSignSet.AddAll(new UnicodeSet("[\u0901\u0902\u0903\u0904\u093e-\u094c\u0962\u0963]"));               /* Devanagari */
                vowelSignSet.AddAll(new UnicodeSet("[\u0981\u0982\u0983\u09be-\u09cc\u09e2\u09e3\u09D7]"));         /* Bengali */
                vowelSignSet.AddAll(new UnicodeSet("[\u0a01\u0a02\u0a03\u0a3e-\u0a4c\u0a62\u0a63\u0a70\u0a71]"));   /* Gurmukhi */
                vowelSignSet.AddAll(new UnicodeSet("[\u0a81\u0a82\u0a83\u0abe-\u0acc\u0ae2\u0ae3]"));               /* Gujarati */
                vowelSignSet.AddAll(new UnicodeSet("[\u0b01\u0b02\u0b03\u0b3e-\u0b4c\u0b62\u0b63\u0b56\u0b57]"));   /* Oriya */
                vowelSignSet.AddAll(new UnicodeSet("[\u0b81\u0b82\u0b83\u0bbe-\u0bcc\u0be2\u0be3\u0bd7]"));         /* Tamil */
                vowelSignSet.AddAll(new UnicodeSet("[\u0c01\u0c02\u0c03\u0c3e-\u0c4c\u0c62\u0c63\u0c55\u0c56]"));   /* Telugu */
                vowelSignSet.AddAll(new UnicodeSet("[\u0c81\u0c82\u0c83\u0cbe-\u0ccc\u0ce2\u0ce3\u0cd5\u0cd6]"));   /* Kannada */
                vowelSignSet.AddAll(new UnicodeSet("[\u0d01\u0d02\u0d03\u0d3e-\u0d4c\u0d62\u0d63\u0d57]"));         /* Malayalam */
            }

            String avagraha = "\u093d\u09bd\u0abd\u0b3d\u0cbd";
            String nukta = "\u093c\u09bc\u0a3c\u0abc\u0b3c\u0cbc";
            String virama = "\u094d\u09cd\u0a4d\u0acd\u0b4d\u0bcd\u0c4d\u0ccd\u0d4d";
            String sanskritStressSigns = "\u0951\u0952\u0953\u0954\u097d";
            String chandrabindu = "\u0901\u0981\u0A81\u0b01\u0c01";

            public override bool Is(String sourceString)
            {
                int cp = sourceString[0];

                // A vowel sign cannot be the first char
                if (vowelSignSet.Contains(cp))
                {
                    return false;
                }
                else if (avagraha.IndexOf(cp) != -1)
                {
                    return false;
                }
                else if (virama.IndexOf(cp) != -1)
                {
                    return false;
                }
                else if (nukta.IndexOf(cp) != -1)
                {
                    return false;
                }
                else if (sanskritStressSigns.IndexOf(cp) != -1)
                {
                    return false;
                }
                else if ((chandrabindu.IndexOf(cp) != -1) &&
                       (sourceString.Length > 1 &&
                               vowelSignSet.Contains(sourceString[1])))
                {
                    return false;
                }
                return true;
            }
        }
        internal static String latinForIndic = "[['.0-9A-Za-z~\u00C0-\u00C5\u00C7-\u00CF\u00D1-\u00D6\u00D9-\u00DD" +
                "\u00E0-\u00E5\u00E7-\u00EF\u00F1-\u00F6\u00F9-\u00FD\u00FF-\u010F" +
                "\u0112-\u0125\u0128-\u0130\u0134-\u0137\u0139-\u013E\u0143-\u0148" +
                "\u014C-\u0151\u0154-\u0165\u0168-\u017E\u01A0-\u01A1\u01AF-\u01B0" +
                "\u01CD-\u01DC\u01DE-\u01E3\u01E6-\u01ED\u01F0\u01F4-\u01F5\u01F8-\u01FB" +
                "\u0200-\u021B\u021E-\u021F\u0226-\u0233\u0294\u0303-\u0304\u0306\u0314-\u0315" +
                "\u0325\u040E\u0419\u0439\u045E\u04C1-\u04C2\u04D0-\u04D1\u04D6-\u04D7" +
                "\u04E2-\u04E3\u04EE-\u04EF\u1E00-\u1E99\u1EA0-\u1EF9\u1F01\u1F03\u1F05" +
                "\u1F07\u1F09\u1F0B\u1F0D\u1F0F\u1F11\u1F13\u1F15\u1F19\u1F1B\u1F1D\u1F21" +
                "\u1F23\u1F25\u1F27\u1F29\u1F2B\u1F2D\u1F2F\u1F31\u1F33\u1F35\u1F37\u1F39" +
                "\u1F3B\u1F3D\u1F3F\u1F41\u1F43\u1F45\u1F49\u1F4B\u1F4D\u1F51\u1F53\u1F55" +
                "\u1F57\u1F59\u1F5B\u1F5D\u1F5F\u1F61\u1F63\u1F65\u1F67\u1F69\u1F6B\u1F6D" +
                "\u1F6F\u1F81\u1F83\u1F85\u1F87\u1F89\u1F8B\u1F8D\u1F8F\u1F91\u1F93\u1F95" +
                "\u1F97\u1F99\u1F9B\u1F9D\u1F9F\u1FA1\u1FA3\u1FA5\u1FA7\u1FA9\u1FAB\u1FAD" +
                "\u1FAF-\u1FB1\u1FB8-\u1FB9\u1FD0-\u1FD1\u1FD8-\u1FD9\u1FE0-\u1FE1\u1FE5" +
                "\u1FE8-\u1FE9\u1FEC\u212A-\u212B\uE04D\uE064]" +
                "-[\uE000-\uE080 \u01E2\u01E3]& [[:latin:][:mark:]]]";

        [Test]
        public void TestDevanagariLatin()
        {
            long start = Time.CurrentTimeMilliseconds();
            if (FIX_ME)
            {
                // We temporarily filter against Unicode 4.1, but we only do this
                // before version 3.4.
                Errln("FAIL: TestDevanagariLatin needs to be updated to remove delete the [:Age=4.1:] filter ");
                return;
            }
            Logln("Warning: TestDevanagariLatin needs to be updated to remove delete the section marked [:Age=4.1:] filter");

            String minusDevAbb = logKnownIssue("cldrbug:4375", null) ? "-[\u0970]" : "";
            new TransliterationTest("Latin-DEVANAGARI", 50)
            .Test(latinForIndic, "[[[:Devanagari:][\u094d][\u0964\u0965]" + minusDevAbb + "]&[:Age=4.1:]]", "[\u0965\u0904]", this, new LegalIndic());
            ShowElapsed(start, "TestDevanagariLatin");
        }

        private static readonly String[][] interIndicArray = new String[][]{
        new String[] {  "BENGALI-DEVANAGARI",
                "[:BENGALI:]", "[:Devanagari:]",
                "[\u0904\u0951-\u0954\u0943-\u0949\u094a\u0962\u0963\u090D\u090e\u0911\u0912\u0929\u0933\u0934\u0935\u0950\u0958\u0959\u095a\u095b\u095e\u097d]", /*roundtrip exclusions*/
        },
        new String[] {  "DEVANAGARI-BENGALI",
                "[:Devanagari:]", "[:BENGALI:]",
                "[\u09D7\u090D\u090e\u0911\u0912\u0929\u0933\u0934\u0935\u0950\u0958\u0959\u095a\u095b\u095e\u09f0\u09f1\u09f2-\u09fa\u09ce]", /*roundtrip exclusions*/
        },

        new String[] {  "GURMUKHI-DEVANAGARI",
                "[:GURMUKHI:]", "[:Devanagari:]",
                "[\u0904\u0902\u0936\u0933\u0951-\u0954\u0902\u0903\u0943-\u0949\u094a\u0962\u0963\u090B\u090C\u090D\u090e\u0911\u0912\u0934\u0937\u093D\u0950\u0960\u0961\u097d]", /*roundtrip exclusions*/
        },
        new String[] {  "DEVANAGARI-GURMUKHI",
                "[:Devanagari:]", "[:GURMUKHI:]",
                "[\u0A02\u0946\u0A5C\u0951-\u0954\u0A70\u0A71\u090B\u090C\u090D\u090e\u0911\u0912\u0934\u0937\u093D\u0950\u0960\u0961\u0a72\u0a73\u0a74]", /*roundtrip exclusions*/
        },

        new String[] {  "GUJARATI-DEVANAGARI",
                "[:GUJARATI:]", "[:Devanagari:]",
                "[\u0904\u0946\u094A\u0962\u0963\u0951-\u0954\u0961\u090c\u090e\u0912\u097d]", /*roundtrip exclusions*/
        },
        new String[] {  "DEVANAGARI-GUJARATI",
                "[:Devanagari:]", "[:GUJARATI:]",
                "[\u0951-\u0954\u0961\u090c\u090e\u0912]", /*roundtrip exclusions*/
        },

        new String[] {  "ORIYA-DEVANAGARI",
                "[:ORIYA:]", "[:Devanagari:]",
                "[\u0904\u0912\u0911\u090D\u090e\u0931\u0943-\u094a\u0962\u0963\u0951-\u0954\u0950\u097d]", /*roundtrip exclusions*/
        },
        new String[] {  "DEVANAGARI-ORIYA",
                "[:Devanagari:]", "[:ORIYA:]",
                "[\u0b5f\u0b56\u0b57\u0b70\u0b71\u0950\u090D\u090e\u0912\u0911\u0931]", /*roundtrip exclusions*/
        },

        new String[] {  "Tamil-DEVANAGARI",
                "[:tamil:]", "[:Devanagari:]",
                "[\u0901\u0904\u093c\u0943-\u094a\u0951-\u0954\u0962\u0963\u090B\u090C\u090D\u0911\u0916\u0917\u0918\u091B\u091D\u0920\u0921\u0922\u0925\u0926\u0927\u092B\u092C\u092D\u0936\u093d\u0950[\u0958-\u0961]\u097d]", /*roundtrip exclusions*/
        },
        new String[] {  "DEVANAGARI-Tamil",
                "[:Devanagari:]", "[:tamil:]",
                "[\u0bd7\u0BF0\u0BF1\u0BF2]", /*roundtrip exclusions*/
        },

        new String[] {  "Telugu-DEVANAGARI",
                "[:telugu:]", "[:Devanagari:]",
                "[\u0904\u093c\u0950\u0945\u0949\u0951-\u0954\u0962\u0963\u090D\u0911\u093d\u0929\u0934[\u0958-\u095f]\u097d]", /*roundtrip exclusions*/
        },
        new String[] {  "DEVANAGARI-TELUGU",
                "[:Devanagari:]", "[:TELUGU:]",
                "[\u0c55\u0c56\u0950\u090D\u0911\u093d\u0929\u0934[\u0958-\u095f]]", /*roundtrip exclusions*/
        },

        new String[] {  "KANNADA-DEVANAGARI",
                "[:KANNADA:]", "[:Devanagari:]",
                "[\u0901\u0904\u0946\u0950\u0945\u0949\u0951-\u0954\u0962\u0963\u0950\u090D\u0911\u093d\u0929\u0934[\u0958-\u095f]\u097d]", /*roundtrip exclusions*/
        },
        new String[] {  "DEVANAGARI-KANNADA",
                "[:Devanagari:]", "[:KANNADA:]",
                "[{\u0cb0\u0cbc}{\u0cb3\u0cbc}\u0cde\u0cd5\u0cd6\u0950\u090D\u0911\u093d\u0929\u0934[\u0958-\u095f]]", /*roundtrip exclusions*/
        },

        new String[] {  "MALAYALAM-DEVANAGARI",
                "[:MALAYALAM:]", "[:Devanagari:]",
                "[\u0901\u0904\u094a\u094b\u094c\u093c\u0950\u0944\u0945\u0949\u0951-\u0954\u0962\u0963\u090D\u0911\u093d\u0929\u0934[\u0958-\u095f]\u097d]", /*roundtrip exclusions*/
        },
        new String[] {  "DEVANAGARI-MALAYALAM",
                "[:Devanagari:]", "[:MALAYALAM:]",
                "[\u0d4c\u0d57\u0950\u090D\u0911\u093d\u0929\u0934[\u0958-\u095f]]", /*roundtrip exclusions*/
        },

        new String[] {  "GURMUKHI-BENGALI",
                "[:GURMUKHI:]", "[:BENGALI:]",
                "[\u0982\u09b6\u09e2\u09e3\u09c3\u09c4\u09d7\u098B\u098C\u09B7\u09E0\u09E1\u09F0\u09F1\u09f2-\u09fa\u09ce]", /*roundtrip exclusions*/
        },
        new String[] {  "BENGALI-GURMUKHI",
                "[:BENGALI:]", "[:GURMUKHI:]",
                "[\u0A02\u0a5c\u0a47\u0a70\u0a71\u0A33\u0A35\u0A59\u0A5A\u0A5B\u0A5E\u0A72\u0A73\u0A74]", /*roundtrip exclusions*/
        },

        new String[] {  "GUJARATI-BENGALI",
                "[:GUJARATI:]", "[:BENGALI:]",
                "[\u09d7\u09e2\u09e3\u098c\u09e1\u09f0\u09f1\u09f2-\u09fa\u09ce]", /*roundtrip exclusions*/
        },
        new String[] {  "BENGALI-GUJARATI",
                "[:BENGALI:]", "[:GUJARATI:]",
                "[\u0A82\u0a83\u0Ac9\u0Ac5\u0ac7\u0A8D\u0A91\u0AB3\u0AB5\u0ABD\u0AD0]", /*roundtrip exclusions*/
        },

        new String[] {  "ORIYA-BENGALI",
                "[:ORIYA:]", "[:BENGALI:]",
                "[\u09c4\u09e2\u09e3\u09f0\u09f1\u09f2-\u09fa\u09ce]", /*roundtrip exclusions*/
        },
        new String[] {  "BENGALI-ORIYA",
                "[:BENGALI:]", "[:ORIYA:]",
                "[\u0b35\u0b71\u0b5f\u0b56\u0b33\u0b3d]", /*roundtrip exclusions*/
        },

        new String[] {  "Tamil-BENGALI",
                "[:tamil:]", "[:BENGALI:]",
                "[\u0981\u09bc\u09c3\u09c4\u09e2\u09e3\u09f0\u09f1\u098B\u098C\u0996\u0997\u0998\u099B\u099D\u09A0\u09A1\u09A2\u09A5\u09A6\u09A7\u09AB\u09AC\u09AD\u09B6\u09DC\u09DD\u09DF\u09E0\u09E1\u09f2-\u09fa\u09ce]", /*roundtrip exclusions*/
        },
        new String[] {  "BENGALI-Tamil",
                "[:BENGALI:]", "[:tamil:]",
                "[\u0bc6\u0bc7\u0bca\u0B8E\u0B92\u0BA9\u0BB1\u0BB3\u0BB4\u0BB5\u0BF0\u0BF1\u0BF2]", /*roundtrip exclusions*/
        },

        new String[] {  "Telugu-BENGALI",
                "[:telugu:]", "[:BENGALI:]",
                "[\u09e2\u09e3\u09bc\u09d7\u09f0\u09f1\u09dc\u09dd\u09df\u09f2-\u09fa\u09ce]", /*roundtrip exclusions*/
        },
        new String[] {  "BENGALI-TELUGU",
                "[:BENGALI:]", "[:TELUGU:]",
                "[\u0c55\u0c56\u0c47\u0c46\u0c4a\u0C0E\u0C12\u0C31\u0C33\u0C35]", /*roundtrip exclusions*/
        },

        new String[] {  "KANNADA-BENGALI",
                "[:KANNADA:]", "[:BENGALI:]",
                "[\u0981\u09e2\u09e3\u09bc\u09d7\u09f0\u09f1\u09dc\u09dd\u09df\u09f2-\u09fa\u09ce]", /*roundtrip exclusions*/
        },
        new String[] {  "BENGALI-KANNADA",
                "[:BENGALI:]", "[:KANNADA:]",
                "[{\u0cb0\u0cbc}{\u0cb3\u0cbc}\u0cc6\u0cca\u0cd5\u0cd6\u0cc7\u0C8E\u0C92\u0CB1\u0cb3\u0cb5\u0cde]", /*roundtrip exclusions*/
        },

        new String[] {  "MALAYALAM-BENGALI",
                "[:MALAYALAM:]", "[:BENGALI:]",
                "[\u0981\u09e2\u09e3\u09bc\u09c4\u09f0\u09f1\u09dc\u09dd\u09df\u09f2-\u09fa\u09ce]", /*roundtrip exclusions*/
        },
        new String[] {  "BENGALI-MALAYALAM",
                "[:BENGALI:]", "[:MALAYALAM:]",
                "[\u0d46\u0d4a\u0d47\u0d31-\u0d35\u0d0e\u0d12]", /*roundtrip exclusions*/
        },

        new String[] {  "GUJARATI-GURMUKHI",
                "[:GUJARATI:]", "[:GURMUKHI:]",
                "[\u0A02\u0ab3\u0ab6\u0A70\u0a71\u0a82\u0a83\u0ac3\u0ac4\u0ac5\u0ac9\u0a5c\u0a72\u0a73\u0a74\u0a8b\u0a8d\u0a91\u0abd]", /*roundtrip exclusions*/
        },
        new String[] {  "GURMUKHI-GUJARATI",
                "[:GURMUKHI:]", "[:GUJARATI:]",
                "[\u0a5c\u0A70\u0a71\u0a72\u0a73\u0a74\u0a82\u0a83\u0a8b\u0a8c\u0a8d\u0a91\u0ab3\u0ab6\u0ab7\u0abd\u0ac3\u0ac4\u0ac5\u0ac9\u0ad0\u0ae0\u0ae1]", /*roundtrip exclusions*/
        },

        new String[] {  "ORIYA-GURMUKHI",
                "[:ORIYA:]", "[:GURMUKHI:]",
                "[\u0A02\u0a5c\u0a21\u0a47\u0a71\u0b02\u0b03\u0b33\u0b36\u0b43\u0b56\u0b57\u0B0B\u0B0C\u0B37\u0B3D\u0B5F\u0B60\u0B61\u0a35\u0a72\u0a73\u0a74]", /*roundtrip exclusions*/
        },
        new String[] {  "GURMUKHI-ORIYA",
                "[:GURMUKHI:]", "[:ORIYA:]",
                "[\u0a71\u0b02\u0b03\u0b33\u0b36\u0b43\u0b56\u0b57\u0B0B\u0B0C\u0B37\u0B3D\u0B5F\u0B60\u0B61\u0b70\u0b71]", /*roundtrip exclusions*/
        },

        new String[] {  "TAMIL-GURMUKHI",
                "[:TAMIL:]", "[:GURMUKHI:]",
                "[\u0A01\u0A02\u0a33\u0a36\u0a3c\u0a70\u0a71\u0a47\u0A16\u0A17\u0A18\u0A1B\u0A1D\u0A20\u0A21\u0A22\u0A25\u0A26\u0A27\u0A2B\u0A2C\u0A2D\u0A59\u0A5A\u0A5B\u0A5C\u0A5E\u0A72\u0A73\u0A74]", /*roundtrip exclusions*/
        },
        new String[] {  "GURMUKHI-TAMIL",
                "[:GURMUKHI:]", "[:TAMIL:]",
                "[\u0b82\u0bc6\u0bca\u0bd7\u0bb7\u0bb3\u0b83\u0B8E\u0B92\u0BA9\u0BB1\u0BB4\u0bb6\u0BF0\u0BF1\u0BF2]", /*roundtrip exclusions*/
        },

        new String[] {  "TELUGU-GURMUKHI",
                "[:TELUGU:]", "[:GURMUKHI:]",
                "[\u0A02\u0a33\u0a36\u0a3c\u0a70\u0a71\u0A59\u0A5A\u0A5B\u0A5C\u0A5E\u0A72\u0A73\u0A74]", /*roundtrip exclusions*/
        },
        new String[] {  "GURMUKHI-TELUGU",
                "[:GURMUKHI:]", "[:TELUGU:]",
                "[\u0c02\u0c03\u0c33\u0c36\u0c44\u0c43\u0c46\u0c4a\u0c56\u0c55\u0C0B\u0C0C\u0C0E\u0C12\u0C31\u0C37\u0C60\u0C61]", /*roundtrip exclusions*/
        },
        new String[] {  "KANNADA-GURMUKHI",
                "[:KANNADA:]", "[:GURMUKHI:]",
                "[\u0A01\u0A02\u0a33\u0a36\u0a3c\u0a70\u0a71\u0A59\u0A5A\u0A5B\u0A5C\u0A5E\u0A72\u0A73\u0A74]", /*roundtrip exclusions*/
        },
        new String[] {  "GURMUKHI-KANNADA",
                "[:GURMUKHI:]", "[:KANNADA:]",
                "[{\u0cb0\u0cbc}{\u0cb3\u0cbc}\u0c82\u0c83\u0cb3\u0cb6\u0cc4\u0cc3\u0cc6\u0cca\u0cd5\u0cd6\u0C8B\u0C8C\u0C8E\u0C92\u0CB1\u0CB7\u0cbd\u0CE0\u0CE1\u0cde]", /*roundtrip exclusions*/
        },

        new String[] {  "MALAYALAM-GURMUKHI",
                "[:MALAYALAM:]", "[:GURMUKHI:]",
                "[\u0A01\u0A02\u0a4b\u0a4c\u0a33\u0a36\u0a3c\u0a70\u0a71\u0A59\u0A5A\u0A5B\u0A5C\u0A5E\u0A72\u0A73\u0A74]", /*roundtrip exclusions*/
        },
        new String[] {  "GURMUKHI-MALAYALAM",
                "[:GURMUKHI:]", "[:MALAYALAM:]",
                "[\u0d02\u0d03\u0d33\u0d36\u0d43\u0d46\u0d4a\u0d4c\u0d57\u0D0B\u0D0C\u0D0E\u0D12\u0D31\u0D34\u0D37\u0D60\u0D61]", /*roundtrip exclusions*/
        },

        new String[] {  "GUJARATI-ORIYA",
                "[:GUJARATI:]", "[:ORIYA:]",
                "[\u0b56\u0b57\u0B0C\u0B5F\u0B61\u0b70\u0b71]", /*roundtrip exclusions*/
        },
        new String[] {  "ORIYA-GUJARATI",
                "[:ORIYA:]", "[:GUJARATI:]",
                "[\u0Ac4\u0Ac5\u0Ac9\u0Ac7\u0A8D\u0A91\u0AB5\u0Ad0]", /*roundtrip exclusions*/
        },

        new String[] {  "TAMIL-GUJARATI",
                "[:TAMIL:]", "[:GUJARATI:]",
                "[\u0A81\u0a8c\u0abc\u0ac3\u0Ac4\u0Ac5\u0Ac9\u0Ac7\u0A8B\u0A8D\u0A91\u0A96\u0A97\u0A98\u0A9B\u0A9D\u0AA0\u0AA1\u0AA2\u0AA5\u0AA6\u0AA7\u0AAB\u0AAC\u0AAD\u0AB6\u0ABD\u0AD0\u0AE0\u0AE1]", /*roundtrip exclusions*/
        },
        new String[] {  "GUJARATI-TAMIL",
                "[:GUJARATI:]", "[:TAMIL:]",
                "[\u0Bc6\u0Bca\u0Bd7\u0B8E\u0B92\u0BA9\u0BB1\u0BB4\u0BF0\u0BF1\u0BF2]", /*roundtrip exclusions*/
        },

        new String[] {  "TELUGU-GUJARATI",
                "[:TELUGU:]", "[:GUJARATI:]",
                "[\u0abc\u0Ac5\u0Ac9\u0A8D\u0A91\u0ABD\u0Ad0]", /*roundtrip exclusions*/
        },
        new String[] {  "GUJARATI-TELUGU",
                "[:GUJARATI:]", "[:TELUGU:]",
                "[\u0c46\u0c4a\u0c55\u0c56\u0C0C\u0C0E\u0C12\u0C31\u0C61]", /*roundtrip exclusions*/
        },

        new String[] {  "KANNADA-GUJARATI",
                "[:KANNADA:]", "[:GUJARATI:]",
                "[\u0A81\u0abc\u0Ac5\u0Ac9\u0A8D\u0A91\u0ABD\u0Ad0]", /*roundtrip exclusions*/
        },
        new String[] {  "GUJARATI-KANNADA",
                "[:GUJARATI:]", "[:KANNADA:]",
                "[{\u0cb0\u0cbc}{\u0cb3\u0cbc}\u0cc6\u0cca\u0cd5\u0cd6\u0C8C\u0C8E\u0C92\u0CB1\u0CDE\u0CE1]", /*roundtrip exclusions*/
        },

        new String[] {  "MALAYALAM-GUJARATI",
                "[:MALAYALAM:]", "[:GUJARATI:]",
                "[\u0A81\u0ac4\u0acb\u0acc\u0abc\u0Ac5\u0Ac9\u0A8D\u0A91\u0ABD\u0Ad0]", /*roundtrip exclusions*/
        },
        new String[] {  "GUJARATI-MALAYALAM",
                "[:GUJARATI:]", "[:MALAYALAM:]",
                "[\u0d46\u0d4a\u0d4c\u0d55\u0d57\u0D0C\u0D0E\u0D12\u0D31\u0D34\u0D61]", /*roundtrip exclusions*/
        },

        new String[] {  "TAMIL-ORIYA",
                "[:TAMIL:]", "[:ORIYA:]",
                "[\u0B01\u0b3c\u0b43\u0b56\u0B0B\u0B0C\u0B16\u0B17\u0B18\u0B1B\u0B1D\u0B20\u0B21\u0B22\u0B25\u0B26\u0B27\u0B2B\u0B2C\u0B2D\u0B36\u0B3D\u0B5C\u0B5D\u0B5F\u0B60\u0B61\u0b70\u0b71]", /*roundtrip exclusions*/
        },
        new String[] {  "ORIYA-TAMIL",
                "[:ORIYA:]", "[:TAMIL:]",
                "[\u0bc6\u0bca\u0bc7\u0B8E\u0B92\u0BA9\u0BB1\u0BB4\u0BB5\u0BF0\u0BF1\u0BF2]", /*roundtrip exclusions*/
        },

        new String[] {  "TELUGU-ORIYA",
                "[:TELUGU:]", "[:ORIYA:]",
                "[\u0b3c\u0b57\u0b56\u0B3D\u0B5C\u0B5D\u0B5F\u0b70\u0b71]", /*roundtrip exclusions*/
        },
        new String[] {  "ORIYA-TELUGU",
                "[:ORIYA:]", "[:TELUGU:]",
                "[\u0c44\u0c46\u0c4a\u0c55\u0c47\u0C0E\u0C12\u0C31\u0C35]", /*roundtrip exclusions*/
        },

        new String[] {  "KANNADA-ORIYA",
                "[:KANNADA:]", "[:ORIYA:]",
                "[\u0B01\u0b3c\u0b57\u0B3D\u0B5C\u0B5D\u0B5F\u0b70\u0b71]", /*roundtrip exclusions*/
        },
        new String[] {  "ORIYA-KANNADA",
                "[:ORIYA:]", "[:KANNADA:]",
                "[{\u0cb0\u0cbc}{\u0cb3\u0cbc}\u0cc4\u0cc6\u0cca\u0cd5\u0cc7\u0C8E\u0C92\u0CB1\u0CB5\u0CDE]", /*roundtrip exclusions*/
        },

        new String[] {  "MALAYALAM-ORIYA",
                "[:MALAYALAM:]", "[:ORIYA:]",
                "[\u0B01\u0b3c\u0b56\u0B3D\u0B5C\u0B5D\u0B5F\u0b70\u0b71]", /*roundtrip exclusions*/
        },
        new String[] {  "ORIYA-MALAYALAM",
                "[:ORIYA:]", "[:MALAYALAM:]",
                "[\u0D47\u0D46\u0D4a\u0D0E\u0D12\u0D31\u0D34\u0D35]", /*roundtrip exclusions*/
        },

        new String[] {  "TELUGU-TAMIL",
                "[:TELUGU:]", "[:TAMIL:]",
                "[\u0bd7\u0ba9\u0bb4\u0BF0\u0BF1\u0BF2\u0BF0\u0BF1\u0BF2]", /*roundtrip exclusions*/
        },
        new String[] {  "TAMIL-TELUGU",
                "[:TAMIL:]", "[:TELUGU:]",
                "[\u0C01\u0c43\u0c44\u0c46\u0c47\u0c55\u0c56\u0c66\u0C0B\u0C0C\u0C16\u0C17\u0C18\u0C1B\u0C1D\u0C20\u0C21\u0C22\u0C25\u0C26\u0C27\u0C2B\u0C2C\u0C2D\u0C36\u0C60\u0C61]", /*roundtrip exclusions*/
        },

        new String[] {  "KANNADA-TAMIL",
                "[:KANNADA:]", "[:TAMIL:]",
                "[\u0bd7\u0bc6\u0ba9\u0bb4\u0BF0\u0BF1\u0BF2]", /*roundtrip exclusions*/
        },
        new String[] {  "TAMIL-KANNADA",
                "[:TAMIL:]", "[:KANNADA:]",
                "[\u0cc3\u0cc4\u0cc6\u0cc7\u0cd5\u0cd6\u0C8B\u0C8C\u0C96\u0C97\u0C98\u0C9B\u0C9D\u0CA0\u0CA1\u0CA2\u0CA5\u0CA6\u0CA7\u0CAB\u0CAC\u0CAD\u0CB6\u0cbc\u0cbd\u0CDE\u0CE0\u0CE1]", /*roundtrip exclusions*/
        },

        new String[] {  "MALAYALAM-TAMIL",
                "[:MALAYALAM:]", "[:TAMIL:]",
                "[\u0ba9\u0BF0\u0BF1\u0BF2]", /*roundtrip exclusions*/
        },
        new String[] {  "TAMIL-MALAYALAM",
                "[:TAMIL:]", "[:MALAYALAM:]",
                "[\u0d43\u0d12\u0D0B\u0D0C\u0D16\u0D17\u0D18\u0D1B\u0D1D\u0D20\u0D21\u0D22\u0D25\u0D26\u0D27\u0D2B\u0D2C\u0D2D\u0D36\u0D60\u0D61]", /*roundtrip exclusions*/
        },

        new String[] {  "KANNADA-TELUGU",
                "[:KANNADA:]", "[:TELUGU:]",
                "[\u0C01\u0c3f\u0c46\u0c48\u0c4a]", /*roundtrip exclusions*/
        },
        new String[] {  "TELUGU-KANNADA",
                "[:TELUGU:]", "[:KANNADA:]",
                "[\u0cc8\u0cd5\u0cd6\u0CDE\u0cbc\u0cbd]", /*roundtrip exclusions*/
        },

        new String[] {  "MALAYALAM-TELUGU",
                "[:MALAYALAM:]", "[:TELUGU:]",
                "[\u0C01\u0c44\u0c4a\u0c4c\u0c4b\u0c55\u0c56]", /*roundtrip exclusions*/
        },
        new String[] {  "TELUGU-MALAYALAM",
                "[:TELUGU:]", "[:MALAYALAM:]",
                "[\u0d4c\u0d57\u0D34]", /*roundtrip exclusions*/
        },

        new String[] {  "MALAYALAM-KANNADA",
                "[:MALAYALAM:]", "[:KANNADA:]",
                "[\u0cbc\u0cbd\u0cc4\u0cc6\u0cca\u0ccc\u0ccb\u0cd5\u0cd6\u0cDe]", /*roundtrip exclusions*/
        },
        new String[] {  "Latin-Bengali",
                latinForIndic, "[[:Bengali:][\u0964\u0965]]",
                "[\u0965\u09f0-\u09fa\u09ce]", /*roundtrip exclusions*/
        },
        new String[] {  "Latin-Gurmukhi",
                latinForIndic, "[[:Gurmukhi:][\u0964\u0965]]",
                "[\u0a01\u0a02\u0965\u0a72\u0a73\u0a74]", /*roundtrip exclusions*/
        },
        new String[] {  "Latin-Gujarati",
                latinForIndic, "[[:Gujarati:][\u0964\u0965]]",
                "[\u0965]", /*roundtrip exclusions*/
        },
        new String[] {  "Latin-Oriya",
                latinForIndic, "[[:Oriya:][\u0964\u0965]]",
                "[\u0965\u0b70]", /*roundtrip exclusions*/
        },
        new String[] {  "Latin-Tamil",
                latinForIndic, "[:Tamil:]",
                "[\u0BF0\u0BF1\u0BF2]", /*roundtrip exclusions*/
        },
        new String[] {  "Latin-Telugu",
                latinForIndic, "[:Telugu:]",
                null, /*roundtrip exclusions*/
        },
        new String[] {  "Latin-Kannada",
                latinForIndic, "[:Kannada:]",
                null, /*roundtrip exclusions*/
        },
        new String[] {  "Latin-Malayalam",
                latinForIndic, "[:Malayalam:]",
                null, /*roundtrip exclusions*/
        },
    };

        [Test]
        public void TestInterIndic()
        {
            long start = Time.CurrentTimeMilliseconds();
            int num = interIndicArray.Length;
            if (IsQuick())
            {
                Logln("Testing only 5 of " + interIndicArray.Length + " Skipping rest (use -e for exhaustive)");
                num = 5;
            }
            if (FIX_ME)
            {
                // We temporarily filter against Unicode 4.1, but we only do this
                // before version 3.4.
                Errln("FAIL: TestInterIndic needs to be updated to remove delete the [:Age=4.1:] filter ");
                return;
            }
            Logln("Warning: TestInterIndic needs to be updated to remove delete the section marked [:Age=4.1:] filter");

            for (int i = 0; i < num; i++)
            {
                Logln("Testing " + interIndicArray[i][0] + " at index " + i);
                /*TODO: uncomment the line below when the transliterator is fixed
                new Test(interIndicArray[i][0], 50)
                    .test(interIndicArray[i][1],
                          interIndicArray[i][2],
                          interIndicArray[i][3],
                          this, new LegalIndic());
                 */
                /* comment lines below  when transliterator is fixed */
                // start
                // TODO(Mark): Fix ticket #8989, transliterate U+0970.
                String minusDevAbb = logKnownIssue("cldrbug:4375", null) ? "-[\u0970]" : "";

                new TransliterationTest(interIndicArray[i][0], 50)
                .Test("[[" + interIndicArray[i][1] + minusDevAbb + "] &[:Age=4.1:]]",
                        "[[" + interIndicArray[i][2] + minusDevAbb + "] &[:Age=4.1:]]",
                        interIndicArray[i][3],
                        this, new LegalIndic());
                //end
            }
            ShowElapsed(start, "TestInterIndic");
        }

        //---------------
        // End Indic
        //---------------

        public class Legal
        {
            public virtual bool Is(String sourceString) { return true; }
        }

        public class LegalJamo : Legal
        {
            // any initial must be followed by a medial (or initial)
            // any medial must follow an initial (or medial)
            // any final must follow a medial (or final)

            public override bool Is(String sourceString)
            {
                try
                {
                    int t;
                    String decomp = Normalizer.Normalize(sourceString, Normalizer.NFD);
                    for (int i = 0; i < decomp.Length; ++i)
                    { // don't worry about surrogates
                        switch (GetType(decomp[i]))
                        {
                            case 0:
                                t = GetType(decomp[i + 1]);
                                if (t != 0 && t != 1) return false;
                                break;
                            case 1:
                                t = GetType(decomp[i - 1]);
                                if (t != 0 && t != 1) return false;
                                break;
                            case 2:
                                t = GetType(decomp[i - 1]);
                                if (t != 1 && t != 2) return false;
                                break;
                        }
                    }
                    return true;
                }
                catch (IndexOutOfRangeException e)
                {
                    return false;
                }
            }

            public int GetType(char c)
            {
                if ('\u1100' <= c && c <= '\u1112') return 0;
                else if ('\u1161' <= c && c <= '\u1175') return 1;
                else if ('\u11A8' <= c && c <= '\u11C2') return 2;
                return -1; // other
            }
        }

        //static BreakIterator thaiBreak = BreakIterator.getWordInstance(new Locale("th", "TH"));
        // anything is legal except word ending with Logical-order-exception
        public class LegalThai : Legal
        {
            public override bool Is(String sourceString)
            {
                if (sourceString.Length == 0) return true;
                char ch = sourceString[sourceString.Length - 1]; // don't worry about surrogates.
                if (UCharacter.HasBinaryProperty(ch, UProperty.LOGICAL_ORDER_EXCEPTION)) return false;


                // disallow anything with a wordbreak between
                /*
                if (UTF16.countCodePoint(sourceString) <= 1) return true;
                thaiBreak.setText(sourceString);
                for (int pos = thaiBreak.first(); pos != BreakIterator.DONE; pos = thaiBreak.next()) {
                    if (pos > 0 && pos < sourceString.Length) {
                        Console.Out.WriteLine("Skipping " + Utility.Escape(sourceString));
                        return false;
                    }
                }
                 */
                return true;
            }
        }

        // anything is legal except that Final letters can't be followed by letter; NonFinal must be
        public class LegalHebrew : Legal
        {
            static UnicodeSet FINAL = new UnicodeSet("[\u05DA\u05DD\u05DF\u05E3\u05E5]");
            static UnicodeSet NON_FINAL = new UnicodeSet("[\u05DB\u05DE\u05E0\u05E4\u05E6]");
            static UnicodeSet LETTER = new UnicodeSet("[:letter:]");

            public override bool Is(String sourceString)
            {
                if (sourceString.Length == 0) return true;
                // don't worry about surrogates.
                for (int i = 0; i < sourceString.Length; ++i)
                {
                    char ch = sourceString[i];
                    char next = i + 1 == sourceString.Length ? '\u0000' : sourceString[i];
                    if (FINAL.Contains(ch))
                    {
                        if (LETTER.Contains(next)) return false;
                    }
                    else if (NON_FINAL.Contains(ch))
                    {
                        if (!LETTER.Contains(next)) return false;
                    }
                }
                return true;
            }
        }


        public class LegalGreek : Legal
        {

            bool full;

            public LegalGreek(bool full)
            {
                this.full = full;
            }

            static readonly char IOTA_SUBSCRIPT = '\u0345';
            static readonly UnicodeSet breathing = new UnicodeSet("[\\u0313\\u0314']");
            static readonly UnicodeSet validSecondVowel = new UnicodeSet("[\\u03C5\\u03B9\\u03A5\\u0399]");

            public static bool IsVowel(char c)
            {
                return "\u03B1\u03B5\u03B7\u03B9\u03BF\u03C5\u03C9\u0391\u0395\u0397\u0399\u039F\u03A5\u03A9".IndexOf(c) >= 0;
            }

            public static bool IsRho(char c)
            {
                return "\u03C1\u03A1".IndexOf(c) >= 0;
            }

            public override bool Is(String sourceString)
            {
                try
                {
                    String decomp = Normalizer.Normalize(sourceString, Normalizer.NFD);

                    // modern is simpler: don't care about anything but a grave
                    if (!full)
                    {
                        //if (sourceString.Equals("\u039C\u03C0")) return false;
                        for (int i = 0; i < decomp.Length; ++i)
                        {
                            char c = decomp[i];
                            // exclude all the accents
                            if (c == '\u0313' || c == '\u0314' || c == '\u0300' || c == '\u0302'
                                    || c == '\u0342' || c == '\u0345'
                                    ) return false;
                        }
                        return true;
                    }

                    // Legal full Greek has breathing marks IFF there is a vowel or RHO at the start
                    // IF it has them, it has exactly one.
                    // IF it starts with a RHO, then the breathing mark must come before the second letter.
                    // IF it starts with a vowel, then it must before the third letter.
                    //  it will only come after the second if of the format [vowel] [no iota subscript!] [upsilon or iota]
                    // Since there are no surrogates in greek, don't worry about them

                    bool firstIsVowel = false;
                    bool firstIsRho = false;
                    bool noLetterYet = true;
                    int breathingCount = 0;
                    int letterCount = 0;
                    //int breathingPosition = -1;

                    for (int i = 0; i < decomp.Length; ++i)
                    {
                        char c = decomp[i];
                        if (UCharacter.IsLetter(c))
                        {
                            ++letterCount;
                            if (firstIsVowel && !validSecondVowel.Contains(c) && breathingCount == 0) return false;
                            if (noLetterYet)
                            {
                                noLetterYet = false;
                                firstIsVowel = IsVowel(c);
                                firstIsRho = IsRho(c);
                            }
                            if (firstIsRho && letterCount == 2 && breathingCount == 0) return false;
                        }
                        if (c == IOTA_SUBSCRIPT && firstIsVowel && breathingCount == 0) return false;
                        if (breathing.Contains(c))
                        {
                            // breathingPosition = i;
                            ++breathingCount;
                        }
                    }

                    if (firstIsVowel || firstIsRho) return breathingCount == 1;
                    return breathingCount == 0;
                }
                catch (Exception t)
                {
                    Console.Out.WriteLine(t.GetType().Name + " " + t.ToString());
                    return true;
                }
            }
        }

        internal class TransliterationTest
        {

            internal TextWriter @out;

            private String transliteratorID;
            private int errorLimit = 500;
            private int errorCount = 0;
            private long pairLimit = 1000000; // make default be 1M.
            private int density = 100;
            UnicodeSet sourceRange;
            UnicodeSet targetRange;
            UnicodeSet toSource;
            UnicodeSet toTarget;
            UnicodeSet roundtripExclusions;

            internal RoundTripTest log;
            Legal legalSource;
            UnicodeSet badCharacters;

            /*
             * create a test for the given script transliterator.
             */
            internal TransliterationTest(String transliteratorID)
                : this(transliteratorID, 100)
            {
            }

            internal TransliterationTest(String transliteratorID, int dens)
            {
                this.transliteratorID = transliteratorID;
                this.density = dens;
            }

            public void SetErrorLimit(int limit)
            {
                errorLimit = limit;
            }

            public void SetPairLimit(int limit)
            {
                pairLimit = limit;
            }

            // Added to do better equality check.

            public static bool IsSame(String a, String b)
            {
                if (a.Equals(b)) return true;
                if (a.Equals(b, StringComparison.OrdinalIgnoreCase) && IsCamel(a)) return true;
                a = Normalizer.Normalize(a, Normalizer.NFD);
                b = Normalizer.Normalize(b, Normalizer.NFD);
                if (a.Equals(b)) return true;
                if (a.Equals(b, StringComparison.OrdinalIgnoreCase) && IsCamel(a)) return true;
                return false;
            }

            /*
            public bool includesSome(UnicodeSet set, String a) {
                int cp;
                for (int i = 0; i < a.Length; i += UTF16.getCharCount(cp)) {
                    cp = UTF16.charAt(a, i);
                    if (set.Contains(cp)) return true;
                }
                return false;
            }
             */

            public static bool IsCamel(String a)
            {
                //Console.Out.WriteLine("CamelTest");
                // see if string is of the form aB; e.g. lower, then upper or title
                int cp;
                bool haveLower = false;
                for (int i = 0; i < a.Length; i += UTF16.GetCharCount(cp))
                {
                    cp = UTF16.CharAt(a, i);
                    UnicodeCategory t = UCharacter.GetType(cp);
                    //Console.Out.WriteLine("\t" + t + " " + Integer.toString(cp,16) + " " + UCharacter.getName(cp));
                    switch (t)
                    {
                        case UnicodeCategory.UppercaseLetter:
                            if (haveLower) return true;
                            break;
                        case UnicodeCategory.TitlecaseLetter:
                            if (haveLower) return true;
                            haveLower = true;
                            break;
                        // drop through, since second letter is lower.
                        case UnicodeCategory.LowercaseLetter:
                            haveLower = true;
                            break;
                    }
                }
                //Console.Out.WriteLine("FALSE");
                return false;
            }

            static readonly UnicodeSet okAnyway = new UnicodeSet("[^[:Letter:]]");
            static readonly UnicodeSet neverOk = new UnicodeSet("[:Other:]");

            //[Test] // ICU4N: Not runnable test (called from elsewhere)
            public void Test(String srcRange, String trgtRange,
                        String rdtripExclusions, RoundTripTest logger, Legal legalSrc)
            {
                Test(srcRange, trgtRange, srcRange, rdtripExclusions, logger, legalSrc);
            }

            /**
             * Will test
             * that everything in sourceRange maps to targetRange,
             * that everything in targetRange maps to backtoSourceRange
             * that everything roundtrips from target -> source -> target, except roundtripExceptions
             */
            //[Test] // ICU4N: Not runnable test (called from elsewhere)
            public void Test(String srcRange, String trgtRange, String backtoSourceRange,
                            String rdtripExclusions, RoundTripTest logger, Legal legalSrc)
            {

                legalSource = legalSrc;
                sourceRange = new UnicodeSet(srcRange);
                sourceRange.RemoveAll(neverOk);

                targetRange = new UnicodeSet(trgtRange);
                targetRange.RemoveAll(neverOk);

                toSource = new UnicodeSet(backtoSourceRange);
                toSource.AddAll(okAnyway);

                toTarget = new UnicodeSet(trgtRange);
                toTarget.AddAll(okAnyway);

                if (rdtripExclusions != null && rdtripExclusions.Length > 0)
                {
                    roundtripExclusions = new UnicodeSet(rdtripExclusions);
                }
                else
                {
                    roundtripExclusions = new UnicodeSet(); // empty
                }

                log = logger;

                TestFmwk.Logln(Utility.Escape("Source:  " + sourceRange));
                TestFmwk.Logln(Utility.Escape("Target:  " + targetRange));
                TestFmwk.Logln(Utility.Escape("Exclude: " + roundtripExclusions));
                if (TestFmwk.IsQuick()) TestFmwk.Logln("Abbreviated Test");

                badCharacters = new UnicodeSet("[:other:]");

                // make a UTF-8 output file we can read with a browser

                // note: check that every transliterator transliterates the null string correctly!

                // {dlf} reorganize so can run test in protected security environment
                //              String logFileName = "test_" + transliteratorID.replace('/', '_') + ".html";

                //              File lf = new File(logFileName);
                //              log.Logln("Creating log file " + lf.getAbsoluteFile());

                //              out = new PrintWriter(new BufferedWriter(new OutputStreamWriter(
                //                        new FileOutputStream(logFileName), "UTF8"), 4*1024));

                MemoryStream bast = new MemoryStream();
                @out = new StreamWriter(
                        bast, Encoding.UTF8);
                {
                    //@out.write('\uFFEF');    // BOM
                    @out.WriteLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">");
                    @out.WriteLine("<HTML><HEAD>");
                    @out.WriteLine("<META content=\"text/html; charset=utf-8\" http-equiv=Content-Type></HEAD>");
                    @out.WriteLine("<BODY bgcolor='#FFFFFF' style='font-family: Arial Unicode MS'>");

                    try
                    {
                        Test2();
                    }
                    catch (TestTruncated e)
                    {
                        @out.WriteLine(e.ToString());
                    }
                    @out.WriteLine("</BODY></HTML>");
                    @out.Dispose();

                    if (errorCount > 0)
                    {
                        try
                        {
                            DirectoryInfo translitErrorDirectory = new DirectoryInfo("translitErrorLogs");
                            if (!translitErrorDirectory.Exists)
                            {
                                translitErrorDirectory.Create();
                            }
                            String logFileName = "translitErrorLogs/test_" + transliteratorID.Replace('/', '_') + ".html";
                            FileInfo lf = new FileInfo(logFileName);
                            TestFmwk.Logln("Creating log file " + lf.FullName);
                            using (FileStream fos = new FileStream(lf.FullName, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                var bytes = bast.ToArray();
                                fos.Write(bytes, 0, bytes.Length);
                            }
                            TestFmwk.Errln(transliteratorID + " errors: "
                                    + errorCount + (errorCount > errorLimit ? " (at least!)" : "")
                                    + ", see " + lf.FullName);
                        }
                        catch (SecurityException e)
                        {
                            TestFmwk.Errln(transliteratorID + " errors: "
                                    + errorCount + (errorCount > errorLimit ? " (at least!)" : "")
                                    + ", no log provided due to protected test domain");
                        }
                    }
                    else
                    {
                        TestFmwk.Logln(transliteratorID + " ok");
                        //                  new File(logFileName).delete();
                    }
                }
            }

            // ok if at least one is not equal
            public bool CheckIrrelevants(Transliterator t, String irrelevants)
            {
                for (int i = 0; i < irrelevants.Length; ++i)
                {
                    char c = irrelevants[i];
                    String cs = UTF16.ValueOf(c);
                    String targ = t.Transliterate(cs);
                    if (cs.Equals(targ)) return true;
                }
                return false;
            }

            internal AbbreviatedUnicodeSetIterator usi = new AbbreviatedUnicodeSetIterator();
            internal AbbreviatedUnicodeSetIterator usi2 = new AbbreviatedUnicodeSetIterator();

            internal Transliterator sourceToTarget;
            internal Transliterator targetToSource;

            //[Test] // ICU4N: Not runnable test (called from elsewhere)
            public void Test2()
            {

                sourceToTarget = Transliterator.GetInstance(transliteratorID);
                targetToSource = sourceToTarget.GetInverse();

                TestFmwk.Logln("Checking that at least one irrevant characters is not NFC'ed");
                @out.WriteLine("<h3>Checking that at least one irrevant characters is not NFC'ed</h3>");

                String irrelevants = "\u2000\u2001\u2126\u212A\u212B\u2329"; // string is from NFC_NO in the UCD

                if (!CheckIrrelevants(sourceToTarget, irrelevants))
                {
                    LogFails("" + GetSourceTarget(transliteratorID) + ", Must not NFC everything");
                }
                if (!CheckIrrelevants(targetToSource, irrelevants))
                {
                    LogFails("" + GetTargetSource(transliteratorID) + ", irrelevants");
                }

                if (EXTRA_TESTS)
                {
                    TestFmwk.Logln("Checking that toRules works");
                    String rules = "";
                    Transliterator sourceToTarget2;
                    Transliterator targetToSource2;
                    try
                    {
                        rules = sourceToTarget.ToRules(false);
                        sourceToTarget2 = Transliterator.CreateFromRules("s2t2", rules, Transliterator.FORWARD);
                        if (PRINT_RULES)
                        {
                            @out.WriteLine("<h3>Forward Rules:</h3><p>");
                            @out.WriteLine(TestUtility.Replace(rules, "\n", "\u200E<br>\n\u200E"));
                            @out.WriteLine("</p>");
                        }
                        rules = targetToSource.ToRules(false);
                        targetToSource2 = Transliterator.CreateFromRules("t2s2", rules, Transliterator.FORWARD);
                        if (PRINT_RULES)
                        {
                            @out.WriteLine("<h3>Backward Rules:</h3><p>");
                            @out.WriteLine(TestUtility.Replace(rules, "\n", "\u200E<br>\n\u200E"));
                            @out.WriteLine("</p>");
                        }
                    }
                    catch (Exception e)
                    {
                        @out.WriteLine("<h3>Broken Rules:</h3><p>");
                        @out.WriteLine(TestUtility.Replace(rules, "\n", "<br>\n"));
                        @out.WriteLine("</p>");
                        @out.Flush();
                        throw;
                    }

                    @out.WriteLine("<h3>Roundtrip Exclusions: " + new UnicodeSet(roundtripExclusions) + "</h3>");
                    @out.Flush();

                    CheckSourceTargetSource(sourceToTarget2);

                    CheckTargetSourceTarget(targetToSource2);
                }

                UnicodeSet failSourceTarg = new UnicodeSet();


                CheckSourceTargetSingles(failSourceTarg);

                bool quickRt = CheckSourceTargetDoubles(failSourceTarg);

                UnicodeSet failTargSource = new UnicodeSet();
                UnicodeSet failRound = new UnicodeSet();

                CheckTargetSourceSingles(failTargSource, failRound);
                CheckTargetSourceDoubles(quickRt, failTargSource, failRound);
            }

            private void CheckSourceTargetSource(Transliterator sourceToTarget2)
            {
                TestFmwk.Logln("Checking that source -> target -> source");
                @out.WriteLine("<h3>Checking that source -> target -> source</h3>");

                usi.Reset(sourceRange);
                while (usi.Next())
                {
                    int c = usi.Codepoint;

                    String cs = UTF16.ValueOf(c);
                    String targ = sourceToTarget.Transliterate(cs);
                    String targ2 = sourceToTarget2.Transliterate(cs);
                    if (!targ.Equals(targ2))
                    {
                        LogToRulesFails("" + GetSourceTarget(transliteratorID) + ", toRules", cs, targ, targ2);
                    }
                }
            }

            private void CheckTargetSourceTarget(Transliterator targetToSource2)
            {
                TestFmwk.Logln("Checking that target -> source -> target");
                @out.WriteLine("<h3>Checking that target -> source -> target</h3>");
                usi.Reset(targetRange);
                while (usi.Next())
                {
                    int c = usi.Codepoint;

                    String cs = UTF16.ValueOf(c);
                    String targ = targetToSource.Transliterate(cs);
                    String targ2 = targetToSource2.Transliterate(cs);
                    if (!targ.Equals(targ2))
                    {
                        LogToRulesFails("" + GetTargetSource(transliteratorID) + ", toRules", cs, targ, targ2);
                    }
                }
            }

            private void CheckSourceTargetSingles(UnicodeSet failSourceTarg)
            {
                TestFmwk.Logln("Checking that source characters convert to target - Singles");
                @out.WriteLine("<h3>Checking that source characters convert to target - Singles</h3>");


                /*
                for (char c = 0; c < 0xFFFF; ++c) {
                    if (!sourceRange.Contains(c)) continue;
                 */
                usi.Reset(sourceRange);
                while (usi.Next())
                {
                    int c = usi.Codepoint;

                    String cs = UTF16.ValueOf(c);
                    String targ = sourceToTarget.Transliterate(cs);
                    if (!toTarget.ContainsAll(targ)
                            || badCharacters.ContainsSome(targ))
                    {
                        String targD = Normalizer.Normalize(targ, Normalizer.NFD);
                        if (!toTarget.ContainsAll(targD)
                                || badCharacters.ContainsSome(targD))
                        {
                            LogWrongScript("" + GetSourceTarget(transliteratorID) + "", cs, targ, toTarget, badCharacters);
                            failSourceTarg.Add(c);
                            continue;
                        }
                    }

                    String cs2 = Normalizer.Normalize(cs, Normalizer.NFD);
                    String targ2 = sourceToTarget.Transliterate(cs2);
                    if (!targ.Equals(targ2))
                    {
                        LogNotCanonical("" + GetSourceTarget(transliteratorID) + "", cs, targ, cs2, targ2);
                    }
                }
            }

            private bool CheckSourceTargetDoubles(UnicodeSet failSourceTarg)
            {
                TestFmwk.Logln("Checking that source characters convert to target - Doubles");
                @out.WriteLine("<h3>Checking that source characters convert to target - Doubles</h3>");
                long count = 0;

                /*
                for (char c = 0; c < 0xFFFF; ++c) {
                    if (TestUtility.isUnassigned(c) ||
                        !sourceRange.Contains(c)) continue;
                    if (failSourceTarg.get(c)) continue;

                 */

                UnicodeSet sourceRangeMinusFailures = new UnicodeSet(sourceRange);
                sourceRangeMinusFailures.RemoveAll(failSourceTarg);

                bool quickRt = TestFmwk.GetExhaustiveness() < 10;

                usi.Reset(sourceRangeMinusFailures, quickRt, density);

                while (usi.Next())
                {
                    int c = usi.Codepoint;

                    /*
                    for (char d = 0; d < 0xFFFF; ++d) {
                        if (TestUtility.isUnassigned(d) ||
                            !sourceRange.Contains(d)) continue;
                        if (failSourceTarg.get(d)) continue;
                     */
                    TestFmwk.Logln(count + "/" + pairLimit + " Checking starting with " + UTF16.ValueOf(c));
                    usi2.Reset(sourceRangeMinusFailures, quickRt, density);

                    while (usi2.Next())
                    {
                        int d = usi2.Codepoint;
                        ++count;

                        String cs = UTF16.ValueOf(c) + UTF16.ValueOf(d);
                        String targ = sourceToTarget.Transliterate(cs);
                        if (!toTarget.ContainsAll(targ)
                                || badCharacters.ContainsSome(targ))
                        {
                            String targD = Normalizer.Normalize(targ, Normalizer.NFD);
                            if (!toTarget.ContainsAll(targD)
                                    || badCharacters.ContainsSome(targD))
                            {
                                LogWrongScript("" + GetSourceTarget(transliteratorID) + "", cs, targ, toTarget, badCharacters);
                                continue;
                            }
                        }
                        String cs2 = Normalizer.Normalize(cs, Normalizer.NFD);
                        String targ2 = sourceToTarget.Transliterate(cs2);
                        if (!targ.Equals(targ2))
                        {
                            LogNotCanonical("" + GetSourceTarget(transliteratorID) + "", cs, targ, cs2, targ2);
                        }
                    }
                }
                return quickRt;
            }

            internal void CheckTargetSourceSingles(UnicodeSet failTargSource, UnicodeSet failRound)
            {
                TestFmwk.Logln("Checking that target characters convert to source and back - Singles");
                @out.WriteLine("<h3>Checking that target characters convert to source and back - Singles</h3>");


                /*for (char c = 0; c < 0xFFFF; ++c) {
                    if (TestUtility.isUnassigned(c) ||
                        !targetRange.Contains(c)) continue;
                 */

                usi.Reset(targetRange);
                while (usi.Next())
                {
                    String cs;
                    int c;
                    if (usi.Codepoint == UnicodeSetIterator.IS_STRING)
                    {
                        cs = usi.String;
                        c = UTF16.CharAt(cs, 0);
                    }
                    else
                    {
                        c = usi.Codepoint;
                        cs = UTF16.ValueOf(c);
                    }

                    String targ = targetToSource.Transliterate(cs);
                    String reverse = sourceToTarget.Transliterate(targ);

                    if (!toSource.ContainsAll(targ)
                            || badCharacters.ContainsSome(targ))
                    {
                        String targD = Normalizer.Normalize(targ, Normalizer.NFD);
                        if (!toSource.ContainsAll(targD)
                                || badCharacters.ContainsSome(targD))
                        {
                            /*UnicodeSet temp = */
                            new UnicodeSet().AddAll(targD);
                            LogWrongScript("" + GetTargetSource(transliteratorID) + "", cs, targ, toSource, badCharacters);
                            failTargSource.Add(cs);
                            continue;
                        }
                    }
                    if (!IsSame(cs, reverse) && !roundtripExclusions.Contains(c)
                            && !roundtripExclusions.Contains(cs))
                    {
                        LogRoundTripFailure(cs, targetToSource.ID, targ, sourceToTarget.ID, reverse);
                        failRound.Add(c);
                        continue;
                    }
                    String targ2 = Normalizer.Normalize(targ, Normalizer.NFD);
                    String reverse2 = sourceToTarget.Transliterate(targ2);
                    if (!reverse.Equals(reverse2))
                    {
                        LogNotCanonical("" + GetTargetSource(transliteratorID) + "", targ, reverse, targ2, reverse2);
                    }
                }

            }

            private void CheckTargetSourceDoubles(bool quickRt, UnicodeSet failTargSource,
                    UnicodeSet failRound)
            {
                TestFmwk.Logln("Checking that target characters convert to source and back - Doubles");
                @out.WriteLine("<h3>Checking that target characters convert to source and back - Doubles</h3>");
                long count = 0;

                UnicodeSet targetRangeMinusFailures = new UnicodeSet(targetRange);
                targetRangeMinusFailures.RemoveAll(failTargSource);
                targetRangeMinusFailures.RemoveAll(failRound);

                //char[] buf = new char[4]; // maximum we can have with 2 code points
                /*
                for (char c = 0; c < 0xFFFF; ++c) {
                    if (TestUtility.isUnassigned(c) ||
                        !targetRange.Contains(c)) continue;
                 */

                usi.Reset(targetRangeMinusFailures, quickRt, density);

                while (usi.Next())
                {
                    int c = usi.Codepoint;

                    //log.log(TestUtility.hex(c));

                    /*
                    for (char d = 0; d < 0xFFFF; ++d) {
                        if (TestUtility.isUnassigned(d) ||
                            !targetRange.Contains(d)) continue;
                     */
                    TestFmwk.Logln(count + "/" + pairLimit + " Checking starting with " + UTF16.ValueOf(c));
                    usi2.Reset(targetRangeMinusFailures, quickRt, density);

                    while (usi2.Next())
                    {

                        int d = usi2.Codepoint;
                        if (d < 0) break;

                        if (++count > pairLimit)
                        {
                            throw new TestTruncated("Test truncated at " + pairLimit);
                        }

                        String cs = UTF16.ValueOf(c) + UTF16.ValueOf(d);
                        String targ = targetToSource.Transliterate(cs);
                        String reverse = sourceToTarget.Transliterate(targ);

                        if (!toSource.ContainsAll(targ) /*&& !failTargSource.Contains(c) && !failTargSource.Contains(d)*/
                                || badCharacters.ContainsSome(targ))
                        {
                            String targD = Normalizer.Normalize(targ, Normalizer.NFD);
                            if (!toSource.ContainsAll(targD) /*&& !failTargSource.Contains(c) && !failTargSource.Contains(d)*/
                                    || badCharacters.ContainsSome(targD))
                            {
                                LogWrongScript("" + GetTargetSource(transliteratorID) + "", cs, targ, toSource, badCharacters);
                                continue;
                            }
                        }
                        if (!IsSame(cs, reverse) /*&& !failRound.Contains(c) && !failRound.Contains(d)*/
                                && !roundtripExclusions.Contains(c)
                                && !roundtripExclusions.Contains(d)
                                && !roundtripExclusions.Contains(cs))
                        {
                            LogRoundTripFailure(cs, targetToSource.ID, targ, sourceToTarget.ID, reverse);
                            continue;
                        }
                        String targ2 = Normalizer.Normalize(targ, Normalizer.NFD);
                        String reverse2 = sourceToTarget.Transliterate(targ2);
                        if (!reverse.Equals(reverse2))
                        {
                            LogNotCanonical("" + GetTargetSource(transliteratorID) + "", targ, reverse, targ2, reverse2);
                        }
                    }
                }
                TestFmwk.Logln("");
            }

            /**
             * @param transliteratorID2
             * @return
             */
            private String GetTargetSource(String transliteratorID2)
            {
                return "Target-Source [" + transliteratorID2 + "]";
            }

            /**
             * @param transliteratorID2
             * @return
             */
            private String GetSourceTarget(String transliteratorID2)
            {
                return "Source-Target [" + transliteratorID2 + "]";
            }

            internal string Info(string s)
            {
                StringBuffer result = new StringBuffer();
                result.Append("\u200E").Append(s).Append("\u200E (").Append(TestUtility.Hex(s)).Append("/");
                if (false)
                { // append age, as a check
                    int cp = 0;
                    for (int i = 0; i < s.Length; i += UTF16.GetCharCount(cp))
                    {
                        cp = UTF16.CharAt(s, i);
                        if (i > 0) result.Append(", ");
                        result.Append(UCharacter.GetAge(cp));
                    }
                }
                result.Append(")");
                return result.ToString();
            }

            internal void LogWrongScript(String label, String from, String to,
                    UnicodeSet shouldContainAll, UnicodeSet shouldNotContainAny)
            {
                if (++errorCount > errorLimit)
                {
                    throw new TestTruncated("Test truncated; too many failures");
                }
                String toD = Normalizer.Normalize(to, Normalizer.NFD);
                UnicodeSet temp = new UnicodeSet().AddAll(toD);
                UnicodeSet bad = new UnicodeSet(shouldNotContainAny).RetainAll(temp)
                        .AddAll(new UnicodeSet(temp).RemoveAll(shouldContainAll));

                @out.WriteLine("<br>Fail " + label + ": " +
                        Info(from) + " => " + Info(to) + " " + bad
                        );
            }

            internal void LogNotCanonical(String label, String from, String to, String fromCan, String toCan)
            {
                if (++errorCount > errorLimit)
                {
                    throw new TestTruncated("Test truncated; too many failures");
                }
                @out.WriteLine("<br>Fail (can.equiv) " + label + ": " +
                            Info(from) + " => " + Info(to) +
                            " -- " +
                            Info(fromCan) + " => " + Info(toCan) + ")"
                            );
            }

            internal void LogFails(String label)
            {
                if (++errorCount > errorLimit)
                {
                    throw new TestTruncated("Test truncated; too many failures");
                }
                @out.WriteLine("<br>Fail (can.equiv)" + label);
            }

            internal void LogToRulesFails(String label, String from, String to, String toCan)
            {
                if (++errorCount > errorLimit)
                {
                    throw new TestTruncated("Test truncated; too many failures");
                }
                @out.WriteLine("<br>Fail " + label + ": " +
                            Info(from) + " => " + Info(to) + ", " + Info(toCan)
                            );
            }

            internal void LogRoundTripFailure(String from, String toID, String to, String backID, String back)
            {
                if (!legalSource.Is(from)) return; // skip illegals

                if (++errorCount > errorLimit)
                {
                    throw new TestTruncated("Test truncated; too many failures");
                }
                @out.WriteLine("<br>Fail Roundtrip: " +
                            Info(from) + " " + toID + " => " + Info(to) + " " + backID + " => " + Info(back)
                            );
            }

            /*
             * Characters to filter for source-target mapping completeness
             * Typically is base alphabet, minus extended characters
             * Default is ASCII letters for Latin
             */
            /*
            public bool isSource(char c) {
                if (!sourceRange.Contains(c)) return false;
                return true;
            }
             */

            /*
             * Characters to check for target back to source mapping.
             * Typically the same as the target script, plus punctuation
             */
            /*
            public bool isReceivingSource(char c) {
                if (!targetRange.Contains(c)) return false;
                return true;
            }
             */
            /*
             * Characters to filter for target-source mapping
             * Typically is base alphabet, minus extended characters
             */
            /*
            public bool isTarget(char c) {
                byte script = TestUtility.getScript(c);
                if (script != targetScript) return false;
                if (!TestUtility.isLetter(c)) return false;
                if (targetRange != null && !targetRange.Contains(c)) return false;
                return true;
            }
             */

            /*
             * Characters to check for target-source mapping
             * Typically the same as the source script, plus punctuation
             */
            /*
            public bool isReceivingTarget(char c) {
                byte script = TestUtility.getScript(c);
                return (script == targetScript || script == TestUtility.COMMON_SCRIPT);
            }

            final bool isSource(String s) {
                for (int i = 0; i < s.Length; ++i) {
                    if (!isSource(s.charAt(i))) return false;
                }
                return true;
            }

            final bool isTarget(String s) {
                for (int i = 0; i < s.Length; ++i) {
                    if (!isTarget(s.charAt(i))) return false;
                }
                return true;
            }

            final bool isReceivingSource(String s) {
                for (int i = 0; i < s.Length; ++i) {
                    if (!isReceivingSource(s.charAt(i))) return false;
                }
                return true;
            }

            final bool isReceivingTarget(String s) {
                for (int i = 0; i < s.Length; ++i) {
                    if (!isReceivingTarget(s.charAt(i))) return false;
                }
                return true;
            }
             */

            internal class TestTruncated : Exception
            {
                /**
                 * For serialization
                 */
                private static readonly long serialVersionUID = 3361828190488168323L;

                internal TestTruncated(String msg)
                : base(msg)
                {
                }
            }
        }

        //  static class TestHangul extends Test {
        //      TestHangul () {
        //          super("Jamo-Hangul", TestUtility.JAMO_SCRIPT, TestUtility.HANGUL_SCRIPT);
        //      }
        //
        //      public bool isSource(char c) {
        //          if (0x1113 <= c && c <= 0x1160) return false;
        //          if (0x1176 <= c && c <= 0x11F9) return false;
        //          if (0x3131 <= c && c <= 0x318E) return false;
        //          return super.isSource(c);
        //      }
        //  }

    }
}
