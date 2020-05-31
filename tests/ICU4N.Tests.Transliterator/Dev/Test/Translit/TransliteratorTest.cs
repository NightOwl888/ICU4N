using ICU4N.Dev.Util;
using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Support.Collections;
using ICU4N.Text;
using ICU4N.Util;
using J2N;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using StringBuffer = System.Text.StringBuilder;

/***********************************************************************

                     HOW TO USE THIS TEST FILE
                               -or-
                  How I developed on two platforms
                without losing (too much of) my mind


1. Add new tests by copying/pasting/changing existing tests.  On Java,
   any public void method named Test...() taking no parameters becomes
   a test.  On C++, you need to modify the header and add a line to
   the runIndexedTest() dispatch method.

2. Make liberal use of the expect() method; it is your friend.

3. The tests in this file exactly match those in a sister file on the
   other side.  The two files are:

   icu4j:  src/com.ibm.icu.dev.test/translit/TransliteratorTest.java
   icu4c:  source/test/intltest/transtst.cpp

                  ==> THIS IS THE IMPORTANT PART <==

   When you add a test in this file, add it in transtst.cpp too.
   Give it the same name and put it in the same relative place.  This
   makes maintenance a lot simpler for any poor soul who ends up
   trying to synchronize the tests between icu4j and icu4c.

4. If you MUST enter a test that is NOT paralleled in the sister file,
   then add it in the special non-mirrored section.  These are
   labeled

     "icu4j ONLY"

   or

     "icu4c ONLY"

   Make sure you document the reason the test is here and not there.


Thank you.
The Management
 ***********************************************************************/

namespace ICU4N.Dev.Test.Translit
{
    /// <summary>
    /// General test of Transliterator
    /// </summary>
    public class TransliteratorTest : TestFmwk
    {
        [Test]
        public void TestHangul()
        {

            Transliterator lh = Transliterator.GetInstance("Latin-Hangul");
            Transliterator hl = lh.GetInverse();

            AssertTransform("Transform", "\uCE20", lh, "ch");

            AssertTransform("Transform", "\uC544\uB530", lh, hl, "atta", "a-tta");
            AssertTransform("Transform", "\uC544\uBE60", lh, hl, "appa", "a-ppa");
            AssertTransform("Transform", "\uC544\uC9DC", lh, hl, "ajja", "a-jja");
            AssertTransform("Transform", "\uC544\uAE4C", lh, hl, "akka", "a-kka");
            AssertTransform("Transform", "\uC544\uC2F8", lh, hl, "assa", "a-ssa");
            AssertTransform("Transform", "\uC544\uCC28", lh, hl, "acha", "a-cha");
            AssertTransform("Transform", "\uC545\uC0AC", lh, hl, "agsa", "ag-sa");
            AssertTransform("Transform", "\uC548\uC790", lh, hl, "anja", "an-ja");
            AssertTransform("Transform", "\uC548\uD558", lh, hl, "anha", "an-ha");
            AssertTransform("Transform", "\uC54C\uAC00", lh, hl, "alga", "al-ga");
            AssertTransform("Transform", "\uC54C\uB9C8", lh, hl, "alma", "al-ma");
            AssertTransform("Transform", "\uC54C\uBC14", lh, hl, "alba", "al-ba");
            AssertTransform("Transform", "\uC54C\uC0AC", lh, hl, "alsa", "al-sa");
            AssertTransform("Transform", "\uC54C\uD0C0", lh, hl, "alta", "al-ta");
            AssertTransform("Transform", "\uC54C\uD30C", lh, hl, "alpa", "al-pa");
            AssertTransform("Transform", "\uC54C\uD558", lh, hl, "alha", "al-ha");
            AssertTransform("Transform", "\uC555\uC0AC", lh, hl, "absa", "ab-sa");
            AssertTransform("Transform", "\uC548\uAC00", lh, hl, "anga", "an-ga");
            AssertTransform("Transform", "\uC545\uC2F8", lh, hl, "agssa", "ag-ssa");
            AssertTransform("Transform", "\uC548\uC9DC", lh, hl, "anjja", "an-jja");
            AssertTransform("Transform", "\uC54C\uC2F8", lh, hl, "alssa", "al-ssa");
            AssertTransform("Transform", "\uC54C\uB530", lh, hl, "altta", "al-tta");
            AssertTransform("Transform", "\uC54C\uBE60", lh, hl, "alppa", "al-ppa");
            AssertTransform("Transform", "\uC555\uC2F8", lh, hl, "abssa", "ab-ssa");
            AssertTransform("Transform", "\uC546\uCE74", lh, hl, "akkka", "akk-ka");
            AssertTransform("Transform", "\uC558\uC0AC", lh, hl, "asssa", "ass-sa");

        }

        [Test]
        public void TestChinese()
        {
            Transliterator hanLatin = Transliterator.GetInstance("Han-Latin");
            AssertTransform("Transform", "z\u00E0o Unicode", hanLatin, "\u9020Unicode");
            AssertTransform("Transform", "z\u00E0i chu\u00E0ng z\u00E0o Unicode zh\u012B qi\u00E1n", hanLatin, "\u5728\u5275\u9020Unicode\u4E4B\u524D");
        }

        [Test]
        public void TestRegistry()
        {
            checkRegistry("foo3", "::[a-z]; ::NFC; [:letter:] a > b;"); // check compound
            checkRegistry("foo2", "::NFC; [:letter:] a > b;"); // check compound
            checkRegistry("foo1", "[:letter:] a > b;");
            foreach (var id in Transliterator.GetAvailableIDs())
            {
                checkRegistry(id);
            }
            // Need to remove these test-specific transliterators in order not to interfere with other tests.
            Transliterator.Unregister("foo3");
            Transliterator.Unregister("foo2");
            Transliterator.Unregister("foo1");
        }

        private void checkRegistry(String id, String rules)
        {
            Transliterator foo = Transliterator.CreateFromRules(id, rules, Transliterator.Forward);
            Transliterator.RegisterInstance(foo);
            checkRegistry(id);
        }

        private void checkRegistry(String id)
        {
            Transliterator fie = Transliterator.GetInstance(id);
            UnicodeSet fae = new UnicodeSet("[a-z5]");
            fie.Filter = (fae);
            Transliterator foe = Transliterator.GetInstance(id);
            UnicodeFilter fee = foe.Filter;
            if (fae.Equals(fee))
            {
                Errln("Changed what is in registry for " + id);
            }
        }

        [Test]
        public void TestInstantiationError()
        {
            try
            {
                String ID = "<Not a valid Transliterator ID>";
                Transliterator t = Transliterator.GetInstance(ID);
                Errln("FAIL: " + ID + " returned " + t);
            }
            catch (ArgumentException ex)
            {
                Logln("OK: Bogus ID handled properly");
            }
        }

        [Test]
        public void TestSimpleRules()
        {
            /* Example: rules 1. ab>x|y
             *                2. yc>z
             *
             * []|eabcd  start - no match, copy e to tranlated buffer
             * [e]|abcd  match rule 1 - copy output & adjust cursor
             * [ex|y]cd  match rule 2 - copy output & adjust cursor
             * [exz]|d   no match, copy d to transliterated buffer
             * [exzd]|   done
             */
            Expect("ab>x|y;" +
                    "yc>z",
                    "eabcd", "exzd");

            /* Another set of rules:
             *    1. ab>x|yzacw
             *    2. za>q
             *    3. qc>r
             *    4. cw>n
             *
             * []|ab       Rule 1
             * [x|yzacw]   No match
             * [xy|zacw]   Rule 2
             * [xyq|cw]    Rule 4
             * [xyqn]|     Done
             */
            Expect("ab>x|yzacw;" +
                    "za>q;" +
                    "qc>r;" +
                    "cw>n",
                    "ab", "xyqn");

            /* Test categories
             */
            Transliterator t = Transliterator.CreateFromRules("<ID>",
                    "$dummy=\uE100;" +
                    "$vowel=[aeiouAEIOU];" +
                    "$lu=[:Lu:];" +
                    "$vowel } $lu > '!';" +
                    "$vowel > '&';" +
                    "'!' { $lu > '^';" +
                    "$lu > '*';" +
                    "a>ERROR",
                    Transliterator.Forward);
            Expect(t, "abcdefgABCDEFGU", "&bcd&fg!^**!^*&");
        }

        /**
         * Test inline set syntax and set variable syntax.
         */
        [Test]
        public void TestInlineSet()
        {
            Expect("{ [:Ll:] } x > y; [:Ll:] > z;", "aAbxq", "zAyzz");
            Expect("a[0-9]b > qrs", "1a7b9", "1qrs9");

            Expect("$digit = [0-9];" +
                    "$alpha = [a-zA-Z];" +
                    "$alphanumeric = [$digit $alpha];" + // ***
                    "$special = [^$alphanumeric];" +     // ***
                    "$alphanumeric > '-';" +
                    "$special > '*';",

                    "thx-1138", "---*----");
        }

        /**
         * Create some inverses and confirm that they work.  We have to be
         * careful how we do this, since the inverses will not be true
         * inverses -- we can't throw any random string at the composition
         * of the transliterators and expect the identity function.  F x
         * F' != I.  However, if we are careful about the input, we will
         * get the expected results.
         */
        [Test]
        public void TestRuleBasedInverse()
        {
            String RULES =
                "abc>zyx;" +
                "ab>yz;" +
                "bc>zx;" +
                "ca>xy;" +
                "a>x;" +
                "b>y;" +
                "c>z;" +

                "abc<zyx;" +
                "ab<yz;" +
                "bc<zx;" +
                "ca<xy;" +
                "a<x;" +
                "b<y;" +
                "c<z;" +

                "";

            String[] DATA = {
                // Careful here -- random strings will not work.  If we keep
                // the left side to the domain and the right side to the range
                // we will be okay though (left, abc; right xyz).
                "a", "x",
                "abcacab", "zyxxxyy",
                "caccb", "xyzzy",
        };

            Transliterator fwd = Transliterator.CreateFromRules("<ID>", RULES, Transliterator.Forward);
            Transliterator rev = Transliterator.CreateFromRules("<ID>", RULES, Transliterator.Reverse);
            for (int i = 0; i < DATA.Length; i += 2)
            {
                Expect(fwd, DATA[i], DATA[i + 1]);
                Expect(rev, DATA[i + 1], DATA[i]);
            }
        }

        /**
         * Basic test of keyboard.
         */
        [Test]
        public void TestKeyboard()
        {
            Transliterator t = Transliterator.CreateFromRules("<ID>",
                    "psch>Y;"
                    + "ps>y;"
                    + "ch>x;"
                    + "a>A;", Transliterator.Forward);
            String[] DATA = {
                // insertion, buffer
                "a", "A",
                "p", "Ap",
                "s", "Aps",
                "c", "Apsc",
                "a", "AycA",
                "psch", "AycAY",
                null, "AycAY", // null means finishKeyboardTransliteration
        };

            keyboardAux(t, DATA);
        }

        /**
         * Basic test of keyboard with cursor.
         */
        [Test]
        public void TestKeyboard2()
        {
            Transliterator t = Transliterator.CreateFromRules("<ID>",
                    "ych>Y;"
                    + "ps>|y;"
                    + "ch>x;"
                    + "a>A;", Transliterator.Forward);
            String[] DATA = {
                // insertion, buffer
                "a", "A",
                "p", "Ap",
                "s", "Aps", // modified for rollback - "Ay",
                "c", "Apsc", // modified for rollback - "Ayc",
                "a", "AycA",
                "p", "AycAp",
                "s", "AycAps", // modified for rollback - "AycAy",
                "c", "AycApsc", // modified for rollback - "AycAyc",
                "h", "AycAY",
                null, "AycAY", // null means finishKeyboardTransliteration
        };

            keyboardAux(t, DATA);
        }

        /**
         * Test keyboard transliteration with back-replacement.
         */
        [Test]
        public void TestKeyboard3()
        {
            // We want th>z but t>y.  Furthermore, during keyboard
            // transliteration we want t>y then yh>z if t, then h are
            // typed.
            String RULES =
                "t>|y;" +
                "yh>z;" +
                "";

            String[] DATA = {
                // Column 1: characters to add to buffer (as if typed)
                // Column 2: expected appearance of buffer after
                //           keyboard xliteration.
                "a", "a",
                "b", "ab",
                "t", "abt", // modified for rollback - "aby",
                "c", "abyc",
                "t", "abyct", // modified for rollback - "abycy",
                "h", "abycz",
                null, "abycz", // null means finishKeyboardTransliteration
        };

            Transliterator t = Transliterator.CreateFromRules("<ID>", RULES, Transliterator.Forward);
            keyboardAux(t, DATA);
        }

        private void keyboardAux(Transliterator t, String[] DATA)
        {
            TransliterationPosition index = new TransliterationPosition();
            ReplaceableString s = new ReplaceableString();
            for (int i = 0; i < DATA.Length; i += 2)
            {
                StringBuffer log;
                if (DATA[i] != null)
                {
                    log = new StringBuffer(s.ToString() + " + "
                            + DATA[i]
                                   + " -> ");
                    t.Transliterate(s, index, DATA[i]);
                }
                else
                {
                    log = new StringBuffer(s.ToString() + " => ");
                    t.FinishTransliteration(s, index);
                }
                UtilityExtensions.FormatInput(log, s, index);
                if (s.ToString().Equals(DATA[i + 1]))
                {
                    Logln(log.ToString());
                }
                else
                {
                    Errln("FAIL: " + log.ToString() + ", expected " + DATA[i + 1]);
                }
            }
        }

        // Latin-Arabic has been temporarily removed until it can be
        // done correctly.

        //  public void TestArabic() {
        //      String DATA[] = {
        //          "Arabic",
        //              "\u062a\u062a\u0645\u062a\u0639 "+
        //              "\u0627\u0644\u0644\u063a\u0629 "+
        //              "\u0627\u0644\u0639\u0631\u0628\u0628\u064a\u0629 "+
        //              "\u0628\u0628\u0646\u0638\u0645 "+
        //              "\u0643\u062a\u0627\u0628\u0628\u064a\u0629 "+
        //              "\u062c\u0645\u064a\u0644\u0629"
        //      };

        //      Transliterator t = Transliterator.GetInstance("Latin-Arabic");
        //      for (int i=0; i<DATA.Length; i+=2) {
        //          expect(t, DATA[i], DATA[i+1]);
        //      }
        //  }

        /**
         * Compose the Kana transliterator forward and reverse and try
         * some strings that should come out unchanged.
         */
        [Test]
        public void TestCompoundKana()
        {
            Transliterator t = Transliterator.GetInstance("Latin-Katakana;Katakana-Latin");
            Expect(t, "aaaaa", "aaaaa");
        }

        /**
         * Compose the hex transliterators forward and reverse.
         */
        [Test]
        public void TestCompoundHex()
        {
            Transliterator a = Transliterator.GetInstance("Any-Hex");
            Transliterator b = Transliterator.GetInstance("Hex-Any");
            // Transliterator[] trans = { a, b };
            // Transliterator ab = Transliterator.GetInstance(trans);
            Transliterator ab = Transliterator.GetInstance("Any-Hex;Hex-Any");

            // Do some basic tests of b
            Expect(b, "\\u0030\\u0031", "01");

            String s = "abcde";
            Expect(ab, s, s);

            // trans = new Transliterator[] { b, a };
            // Transliterator ba = Transliterator.GetInstance(trans);
            Transliterator ba = Transliterator.GetInstance("Hex-Any;Any-Hex");
            ReplaceableString str = new ReplaceableString(s);
            a.Transliterate(str);
            Expect(ba, str.ToString(), str.ToString());
        }

        private class TestFilteringUnicodeFilter : UnicodeFilter
        {
            public override bool Contains(int c)
            {
                return c != 'c';
            }

            public override string ToPattern(bool escapeUnprintable)
            {
                return "";
            }

            public override bool MatchesIndexValue(int v)
            {
                return false;
            }

            public override void AddMatchSetTo(UnicodeSet toUnionTo)
            {
            }


        }

        /**
         * Do some basic tests of filtering.
         */
        [Test]
        public void TestFiltering()
        {

            Transliterator tempTrans = Transliterator.CreateFromRules("temp", "x > y; x{a} > b; ", Transliterator.Forward);
            tempTrans.Filter = (new UnicodeSet("[a]"));
            String tempResult = tempTrans.Transform("xa");
            assertEquals("context should not be filtered ", "xb", tempResult);

            tempTrans = Transliterator.CreateFromRules("temp", "::[a]; x > y; x{a} > b; ", Transliterator.Forward);
            tempResult = tempTrans.Transform("xa");
            assertEquals("context should not be filtered ", "xb", tempResult);

            Transliterator hex = Transliterator.GetInstance("Any-Hex");
            hex.Filter = (new TestFilteringUnicodeFilter());
            String s = "abcde";
            String @out = hex.Transliterate(s);
            String exp = "\\u0061\\u0062c\\u0064\\u0065";
            if (@out.Equals(exp))
            {
                Logln("Ok:   \"" + exp + "\"");
            }
            else
            {
                Logln("FAIL: \"" + @out + "\", wanted \"" + exp + "\"");
            }
        }

        /**
         * Test anchors
         */
        [Test]
        public void TestAnchors()
        {
            Expect("^ab  > 01 ;" +
                    " ab  > |8 ;" +
                    "  b  > k ;" +
                    " 8x$ > 45 ;" +
                    " 8x  > 77 ;",

                    "ababbabxabx",
            "018k7745");
            Expect("$s = [z$] ;" +
                    "$s{ab    > 01 ;" +
                    "   ab    > |8 ;" +
                    "    b    > k ;" +
                    "   8x}$s > 45 ;" +
                    "   8x    > 77 ;",

                    "abzababbabxzabxabx",
            "01z018k45z01x45");
        }

        /**
         * Test pattern quoting and escape mechanisms.
         */
        [Test]
        public void TestPatternQuoting()
        {
            // Array of 3n items
            // Each item is <rules>, <input>, <expected output>
            String[] DATA = {
                "\u4E01>'[male adult]'", "\u4E01", "[male adult]",
        };

            for (int i = 0; i < DATA.Length; i += 3)
            {
                Logln("Pattern: " + Utility.Escape(DATA[i]));
                Transliterator t = Transliterator.CreateFromRules("<ID>", DATA[i], Transliterator.Forward);
                Expect(t, DATA[i + 1], DATA[i + 2]);
            }
        }

        [Test]
        public void TestVariableNames()
        {
            Transliterator gl = Transliterator.CreateFromRules("foo5", "$\u2DC0 = qy; a>b;", Transliterator.Forward);
            if (gl == null)
            {
                Errln("FAIL: null Transliterator returned.");
            }
        }

        /**
         * Regression test for bugs found in Greek transliteration.
         */
        [Test]
        public void TestJ277()
        {
            Transliterator gl = Transliterator.GetInstance("Greek-Latin; NFD; [:M:]Remove; NFC");

            char sigma = (char)0x3C3;
            char upsilon = (char)0x3C5;
            char nu = (char)0x3BD;
            // not used char PHI = (char)0x3A6;
            char alpha = (char)0x3B1;
            // not used char omega = (char)0x3C9;
            // not used char omicron = (char)0x3BF;
            // not used char epsilon = (char)0x3B5;

            // sigma upsilon nu -> syn
            StringBuffer buf = new StringBuffer();
            buf.Append(sigma).Append(upsilon).Append(nu);
            String syn = buf.ToString();
            Expect(gl, syn, "syn");

            // sigma alpha upsilon nu -> saun
            buf.Length = (0);
            buf.Append(sigma).Append(alpha).Append(upsilon).Append(nu);
            String sayn = buf.ToString();
            Expect(gl, sayn, "saun");

            // Again, using a smaller rule set
            String rules =
                "$alpha   = \u03B1;" +
                "$nu      = \u03BD;" +
                "$sigma   = \u03C3;" +
                "$ypsilon = \u03C5;" +
                "$vowel   = [aeiouAEIOU$alpha$ypsilon];" +
                "s <>           $sigma;" +
                "a <>           $alpha;" +
                "u <>  $vowel { $ypsilon;" +
                "y <>           $ypsilon;" +
                "n <>           $nu;";
            Transliterator mini = Transliterator.CreateFromRules
            ("mini", rules, Transliterator.Reverse);
            Expect(mini, syn, "syn");
            Expect(mini, sayn, "saun");

            //|    // Transliterate the Greek locale data
            //|    Locale el("el");
            //|    DateFormatSymbols syms(el, status);
            //|    if (U_FAILURE(status)) { Errln("FAIL: Transliterator constructor failed"); return; }
            //|    int32_t i, count;
            //|    const UnicodeString* data = syms.getMonths(count);
            //|    for (i=0; i<count; ++i) {
            //|        if (data[i].Length == 0) {
            //|            continue;
            //|        }
            //|        UnicodeString out(data[i]);
            //|        gl->transliterate(out);
            //|        bool_t ok = TRUE;
            //|        if (data[i].Length >= 2 && out.Length >= 2 &&
            //|            u_isupper(data[i].charAt(0)) && u_islower(data[i].charAt(1))) {
            //|            if (!(u_isupper(out.charAt(0)) && u_islower(out.charAt(1)))) {
            //|                ok = FALSE;
            //|            }
            //|        }
            //|        if (ok) {
            //|            Logln(prettify(data[i] + " -> " + out));
            //|        } else {
            //|            Errln(UnicodeString("FAIL: ") + prettify(data[i] + " -> " + out));
            //|        }
            //|    }
        }

        //    /**
        //     * Prefix, suffix support in hex transliterators
        //     */
        //    public void TestJ243() {
        //        // Test default Hex-Any, which should handle
        //        // \\u, \\U, u+, and U+
        //        HexToUnicodeTransliterator hex = new HexToUnicodeTransliterator();
        //        expect(hex, "\\u0041+\\U0042,u+0043uu+0044z", "A+B,CuDz");
        //
        //        // Try a custom Hex-Any
        //        // \\uXXXX and &#xXXXX;
        //        HexToUnicodeTransliterator hex2 = new HexToUnicodeTransliterator("\\\\u###0;&\\#x###0\\;");
        //        expect(hex2, "\\u61\\u062\\u0063\\u00645\\u66x&#x30;&#x031;&#x0032;&#x00033;",
        //               "abcd5fx012&#x00033;");
        //
        //        // Try custom Any-Hex (default is tested elsewhere)
        //        UnicodeToHexTransliterator hex3 = new UnicodeToHexTransliterator("&\\#x###0;");
        //        expect(hex3, "012", "&#x30;&#x31;&#x32;");
        //    }

        [Test]
        public void TestJ329()
        {

            Object[] DATA = {
                false, "a > b; c > d",
                true,  "a > b; no operator; c > d",
        };

            for (int i = 0; i < DATA.Length; i += 2)
            {
                String err = null;
                try
                {
                    Transliterator.CreateFromRules("<ID>",
                            (String)DATA[i + 1],
                            Transliterator.Forward);
                }
                catch (ArgumentException e)
                {
                    err = e.ToString();
                }
                bool gotError = (err != null);
                String desc = (String)DATA[i + 1] +
                (gotError ? (" -> error: " + err) : " -> no error");
                if ((err != null) == ((bool)DATA[i]))
                {
                    Logln("Ok:   " + desc);
                }
                else
                {
                    Errln("FAIL: " + desc);
                }
            }
        }

        /**
         * Test segments and segment references.
         */
        [Test]
        public void TestSegments()
        {
            // Array of 3n items
            // Each item is <rules>, <input>, <expected output>
            String[] DATA = {
                "([a-z]) '.' ([0-9]) > $2 '-' $1",
                "abc.123.xyz.456",
                "ab1-c23.xy4-z56",
        };

            for (int i = 0; i < DATA.Length; i += 3)
            {
                Logln("Pattern: " + Utility.Escape(DATA[i]));
                Transliterator t = Transliterator.CreateFromRules("<ID>", DATA[i], Transliterator.Forward);
                Expect(t, DATA[i + 1], DATA[i + 2]);
            }
        }

        /**
         * Test cursor positioning outside of the key
         */
        [Test]
        public void TestCursorOffset()
        {
            // Array of 3n items
            // Each item is <rules>, <input>, <expected output>
            String[] DATA = {
                "pre {alpha} post > | @ ALPHA ;" +
                "eALPHA > beta ;" +
                "pre {beta} post > BETA @@ | ;" +
                "post > xyz",

                "prealphapost prebetapost",
                "prbetaxyz preBETApost",
        };

            for (int i = 0; i < DATA.Length; i += 3)
            {
                Logln("Pattern: " + Utility.Escape(DATA[i]));
                Transliterator t = Transliterator.CreateFromRules("<ID>", DATA[i], Transliterator.Forward);
                Expect(t, DATA[i + 1], DATA[i + 2]);
            }
        }

        /**
         * Test zero length and > 1 char length variable values.  Test
         * use of variable refs in UnicodeSets.
         */
        [Test]
        public void TestArbitraryVariableValues()
        {
            // Array of 3n items
            // Each item is <rules>, <input>, <expected output>
            String[] DATA = {
                "$abe = ab;" +
                "$pat = x[yY]z;" +
                "$ll  = 'a-z';" +
                "$llZ = [$ll];" +
                "$llY = [$ll$pat];" +
                "$emp = ;" +

                "$abe > ABE;" +
                "$pat > END;" +
                "$llZ > 1;" +
                "$llY > 2;" +
                "7$emp 8 > 9;" +
                "",

                "ab xYzxyz stY78",
                "ABE ENDEND 1129",
        };

            for (int i = 0; i < DATA.Length; i += 3)
            {
                Logln("Pattern: " + Utility.Escape(DATA[i]));
                Transliterator t = Transliterator.CreateFromRules("<ID>", DATA[i], Transliterator.Forward);
                Expect(t, DATA[i + 1], DATA[i + 2]);
            }
        }

        /**
         * Confirm that the contextStart, contextLimit, start, and limit
         * behave correctly.
         */
        [Test]
        public void TestPositionHandling()
        {
            // Array of 3n items
            // Each item is <rules>, <input>, <expected output>
            String[] DATA = {
                "a{t} > SS ; {t}b > UU ; {t} > TT ;",
                "xtat txtb", // pos 0,9,0,9
                "xTTaSS TTxUUb",

                "a{t} > SS ; {t}b > UU ; {t} > TT ;",
                "xtat txtb", // pos 2,9,3,8
                "xtaSS TTxUUb",

                "a{t} > SS ; {t}b > UU ; {t} > TT ;",
                "xtat txtb", // pos 3,8,3,8
                "xtaTT TTxTTb",
        };

            // Array of 4n positions -- these go with the DATA array
            // They are: contextStart, contextLimit, start, limit
            int[] POS = {
                0, 9, 0, 9,
                2, 9, 3, 8,
                3, 8, 3, 8,
        };

            int n = DATA.Length / 3;
            for (int i = 0; i < n; i++)
            {
                Transliterator t = Transliterator.CreateFromRules("<ID>", DATA[3 * i], Transliterator.Forward);
                TransliterationPosition pos = new TransliterationPosition(
                        POS[4 * i], POS[4 * i + 1], POS[4 * i + 2], POS[4 * i + 3]);
                ReplaceableString rsource = new ReplaceableString(DATA[3 * i + 1]);
                t.Transliterate(rsource, pos);
                t.FinishTransliteration(rsource, pos);
                String result = rsource.ToString();
                String exp = DATA[3 * i + 2];
                ExpectAux(Utility.Escape(DATA[3 * i]),
                        DATA[3 * i + 1],
                        result,
                        result.Equals(exp),
                        exp);
            }
        }

        /**
         * Test the Hiragana-Katakana transliterator.
         */
        [Test]
        public void TestHiraganaKatakana()
        {
            Transliterator hk = Transliterator.GetInstance("Hiragana-Katakana");
            Transliterator kh = Transliterator.GetInstance("Katakana-Hiragana");

            // Array of 3n items
            // Each item is "hk"|"kh"|"both", <Hiragana>, <Katakana>
            String[] DATA = {
                "both",
                "\u3042\u3090\u3099\u3092\u3050",
                "\u30A2\u30F8\u30F2\u30B0",

                "kh",
                "\u307C\u3051\u3060\u3042\u3093\u30FC",
                "\u30DC\u30F6\u30C0\u30FC\u30F3\u30FC",
        };

            for (int i = 0; i < DATA.Length; i += 3)
            {
                switch (DATA[i][0])
                {
                    case 'h': // Hiragana-Katakana
                        Expect(hk, DATA[i + 1], DATA[i + 2]);
                        break;
                    case 'k': // Katakana-Hiragana
                        Expect(kh, DATA[i + 2], DATA[i + 1]);
                        break;
                    case 'b': // both
                        Expect(hk, DATA[i + 1], DATA[i + 2]);
                        Expect(kh, DATA[i + 2], DATA[i + 1]);
                        break;
                }
            }

        }

        [Test]
        public void TestCopyJ476()
        {
            // This is a C++-only copy constructor test
        }

        /**
         * Test inter-Indic transliterators.  These are composed.
         */
        [Test]
        public void TestInterIndic()
        {
            String ID = "Devanagari-Gujarati";
            Transliterator dg = Transliterator.GetInstance(ID);
            if (dg == null)
            {
                Errln("FAIL: getInstance(" + ID + ") returned null");
                return;
            }
            String id = dg.ID;
            if (!id.Equals(ID))
            {
                Errln("FAIL: getInstance(" + ID + ").ID => " + id);
            }
            String dev = "\u0901\u090B\u0925";
            String guj = "\u0A81\u0A8B\u0AA5";
            Expect(dg, dev, guj);
        }

        /**
         * Test filter syntax in IDs. (J23)
         */
        [Test]
        public void TestFilterIDs()
        {
            String[] DATA = {
                "[aeiou]Any-Hex", // ID
                "[aeiou]Hex-Any", // expected inverse ID
                "quizzical",      // src
                "q\\u0075\\u0069zz\\u0069c\\u0061l", // expected ID.translit(src)

                "[aeiou]Any-Hex;[^5]Hex-Any",
                "[^5]Any-Hex;[aeiou]Hex-Any",
                "quizzical",
                "q\\u0075izzical",

                "[abc]Null",
                "[abc]Null",
                "xyz",
                "xyz",
        };

            for (int i = 0; i < DATA.Length; i += 4)
            {
                String ID = DATA[i];
                Transliterator t = Transliterator.GetInstance(ID);
                Expect(t, DATA[i + 2], DATA[i + 3]);

                // Check the ID
                if (!ID.Equals(t.ID))
                {
                    Errln("FAIL: getInstance(" + ID + ").ID => " +
                            t.ID);
                }

                // Check the inverse
                String uID = DATA[i + 1];
                Transliterator u = t.GetInverse();
                if (u == null)
                {
                    Errln("FAIL: " + ID + ".GetInverse() returned NULL");
                }
                else if (!u.ID.Equals(uID))
                {
                    Errln("FAIL: " + ID + ".GetInverse().ID => " +
                            u.ID + ", expected " + uID);
                }
            }
        }

        /**
         * Test the case mapping transliterators.
         */
        [Test]
        public void TestCaseMap()
        {
            Transliterator toUpper =
                Transliterator.GetInstance("Any-Upper[^xyzXYZ]");
            Transliterator toLower =
                Transliterator.GetInstance("Any-Lower[^xyzXYZ]");
            Transliterator toTitle =
                Transliterator.GetInstance("Any-Title[^xyzXYZ]");

            Expect(toUpper, "The quick brown fox jumped over the lazy dogs.",
            "THE QUICK BROWN FOx JUMPED OVER THE LAzy DOGS.");
            Expect(toLower, "The quIck brown fOX jUMPED OVER THE LAzY dogs.",
            "the quick brown foX jumped over the lazY dogs.");
            Expect(toTitle, "the quick brown foX caN'T jump over the laZy dogs.",
            "The Quick Brown FoX Can't Jump Over The LaZy Dogs.");
        }

        /**
         * Test the name mapping transliterators.
         */
        [Test]
        public void TestNameMap()
        {
            Transliterator uni2name =
                Transliterator.GetInstance("Any-Name[^abc]");
            Transliterator name2uni =
                Transliterator.GetInstance("Name-Any");

            Expect(uni2name, "\u00A0abc\u4E01\u00B5\u0A81\uFFFD\u0004\u0009\u0081\uFFFF",
            "\\N{NO-BREAK SPACE}abc\\N{CJK UNIFIED IDEOGRAPH-4E01}\\N{MICRO SIGN}\\N{GUJARATI SIGN CANDRABINDU}\\N{REPLACEMENT CHARACTER}\\N{<control-0004>}\\N{<control-0009>}\\N{<control-0081>}\\N{<noncharacter-FFFF>}");
            Expect(name2uni, "{\\N { NO-BREAK SPACE}abc\\N{  CJK UNIFIED  IDEOGRAPH-4E01  }\\N{x\\N{MICRO SIGN}\\N{GUJARATI SIGN CANDRABINDU}\\N{REPLACEMENT CHARACTER}\\N{<control-0004>}\\N{<control-0009>}\\N{<control-0081>}\\N{<noncharacter-FFFF>}\\N{<control-0004>}\\N{",
            "{\u00A0abc\u4E01\\N{x\u00B5\u0A81\uFFFD\u0004\u0009\u0081\uFFFF\u0004\\N{");

            // round trip
            Transliterator t = Transliterator.GetInstance("Any-Name;Name-Any");

            String s = "{\u00A0abc\u4E01\\N{x\u00B5\u0A81\uFFFD\u0004\u0009\u0081\uFFFF\u0004\\N{";
            Expect(t, s, s);
        }

        /**
         * Test liberalized ID syntax.  1006c
         */
        [Test]
        public void TestLiberalizedID()
        {
            // Some test cases have an expected getID() value of NULL.  This
            // means I have disabled the test case for now.  This stuff is
            // still under development, and I haven't decided whether to make
            // getID() return canonical case yet.  It will all get rewritten
            // with the move to Source-Target/Variant IDs anyway. [aliu]
            String[] DATA = {
                "latin-greek", null /*"Latin-Greek"*/, "case insensitivity",
                "  Null  ", "Null", "whitespace",
                " Latin[a-z]-Greek  ", "[a-z]Latin-Greek", "inline filter",
                "  null  ; latin-greek  ", null /*"Null;Latin-Greek"*/, "compound whitespace",
        };

            for (int i = 0; i < DATA.Length; i += 3)
            {
                try
                {
                    Transliterator t = Transliterator.GetInstance(DATA[i]);
                    if (DATA[i + 1] == null || DATA[i + 1].Equals(t.ID))
                    {
                        Logln("Ok: " + DATA[i + 2] +
                                " create ID \"" + DATA[i] + "\" => \"" +
                                t.ID + "\"");
                    }
                    else
                    {
                        Errln("FAIL: " + DATA[i + 2] +
                                " create ID \"" + DATA[i] + "\" => \"" +
                                t.ID + "\", exp \"" + DATA[i + 1] + "\"");
                    }
                }
                catch (ArgumentException e)
                {
                    Errln("FAIL: " + DATA[i + 2] +
                            " create ID \"" + DATA[i] + "\"");
                }
            }
        }

        [Test]
        public void TestCreateInstance()
        {
            String FORWARD = "F";
            String REVERSE = "R";
            String[] DATA = {
                // Column 1: id
                // Column 2: direction
                // Column 3: expected ID, or "" if expect failure
                "Latin-Hangul", REVERSE, "Hangul-Latin", // JB#912

                // JB#2689: bad compound causes crash
                "InvalidSource-InvalidTarget", FORWARD, "",
                "InvalidSource-InvalidTarget", REVERSE, "",
                "Hex-Any;InvalidSource-InvalidTarget", FORWARD, "",
                "Hex-Any;InvalidSource-InvalidTarget", REVERSE, "",
                "InvalidSource-InvalidTarget;Hex-Any", FORWARD, "",
                "InvalidSource-InvalidTarget;Hex-Any", REVERSE, "",

                null
        };

            for (int i = 0; DATA[i] != null; i += 3)
            {
                String id = DATA[i];
                TransliterationDirection dir = (DATA[i + 1] == FORWARD) ?
                        Transliterator.Forward : Transliterator.Reverse;
                String expID = DATA[i + 2];
                Exception e = null;
                Transliterator t;
                try
                {
                    t = Transliterator.GetInstance(id, dir);
                }
                catch (Exception e1)
                {
                    e = e1;
                    t = null;
                }
                String newID = (t != null) ? t.ID : "";
                bool ok = (newID.Equals(expID));
                if (t == null)
                {
                    newID = e.Message;
                }
                if (ok)
                {
                    Logln("Ok: createInstance(" +
                            id + "," + DATA[i + 1] + ") => " + newID);
                }
                else
                {
                    Errln("FAIL: createInstance(" +
                            id + "," + DATA[i + 1] + ") => " + newID +
                            ", expected " + expID);
                }
            }
        }

        /**
         * Test the normalization transliterator.
         */
        [Test]
        public void TestNormalizationTransliterator()
        {
            // THE FOLLOWING TWO TABLES ARE COPIED FROM com.ibm.icu.dev.test.normalizer.BasicTest
            // PLEASE KEEP THEM IN SYNC WITH BasicTest.
            String[][] CANON = {
                // Input               Decomposed            Composed
                new string[] {"cat",                "cat",                "cat"               },
                new string[] {"\u00e0ardvark",      "a\u0300ardvark",     "\u00e0ardvark"     },

                new string[] {"\u1e0a",             "D\u0307",            "\u1e0a"            }, // D-dot_above
                new string[] {"D\u0307",            "D\u0307",            "\u1e0a"            }, // D dot_above

                new string[] {"\u1e0c\u0307",       "D\u0323\u0307",      "\u1e0c\u0307"      }, // D-dot_below dot_above
                new string[] {"\u1e0a\u0323",       "D\u0323\u0307",      "\u1e0c\u0307"      }, // D-dot_above dot_below
                new string[] {"D\u0307\u0323",      "D\u0323\u0307",      "\u1e0c\u0307"      }, // D dot_below dot_above

                new string[] {"\u1e10\u0307\u0323", "D\u0327\u0323\u0307","\u1e10\u0323\u0307"}, // D dot_below cedilla dot_above
                new string[] {"D\u0307\u0328\u0323","D\u0328\u0323\u0307","\u1e0c\u0328\u0307"}, // D dot_above ogonek dot_below

                new string[] {"\u1E14",             "E\u0304\u0300",      "\u1E14"            }, // E-macron-grave
                new string[] {"\u0112\u0300",       "E\u0304\u0300",      "\u1E14"            }, // E-macron + grave
                new string[] {"\u00c8\u0304",       "E\u0300\u0304",      "\u00c8\u0304"      }, // E-grave + macron

                new string[] {"\u212b",             "A\u030a",            "\u00c5"            }, // angstrom_sign
                new string[] {"\u00c5",             "A\u030a",            "\u00c5"            }, // A-ring

                new string[] {"\u00fdffin",         "y\u0301ffin",        "\u00fdffin"        }, //updated with 3.0
                new string[] {"\u00fd\uFB03n",      "y\u0301\uFB03n",     "\u00fd\uFB03n"     }, //updated with 3.0

                new string[] {"Henry IV",           "Henry IV",           "Henry IV"          },
                new string[] {"Henry \u2163",       "Henry \u2163",       "Henry \u2163"      },

                new string[] {"\u30AC",             "\u30AB\u3099",       "\u30AC"            }, // ga (Katakana)
                new string[] {"\u30AB\u3099",       "\u30AB\u3099",       "\u30AC"            }, // ka + ten
                new string[] {"\uFF76\uFF9E",       "\uFF76\uFF9E",       "\uFF76\uFF9E"      }, // hw_ka + hw_ten
                new string[] {"\u30AB\uFF9E",       "\u30AB\uFF9E",       "\u30AB\uFF9E"      }, // ka + hw_ten
                new string[] {"\uFF76\u3099",       "\uFF76\u3099",       "\uFF76\u3099"      }, // hw_ka + ten

                new string[] {"A\u0300\u0316",      "A\u0316\u0300",      "\u00C0\u0316"      },
        };

            String[][] COMPAT = {
                // Input               Decomposed            Composed
                new string[] {"\uFB4f",             "\u05D0\u05DC",       "\u05D0\u05DC"      }, // Alef-Lamed vs. Alef, Lamed

                new string[] {"\u00fdffin",         "y\u0301ffin",        "\u00fdffin"        }, //updated for 3.0
                new string[] {"\u00fd\uFB03n",      "y\u0301ffin",        "\u00fdffin"        }, // ffi ligature -> f + f + i

                new string[] {"Henry IV",           "Henry IV",           "Henry IV"          },
                new string[] {"Henry \u2163",       "Henry IV",           "Henry IV"          },

                new string[] {"\u30AC",             "\u30AB\u3099",       "\u30AC"            }, // ga (Katakana)
                new string[] {"\u30AB\u3099",       "\u30AB\u3099",       "\u30AC"            }, // ka + ten

                new string[] {"\uFF76\u3099",       "\u30AB\u3099",       "\u30AC"            }, // hw_ka + ten
        };

            Transliterator NFD = Transliterator.GetInstance("NFD");
            Transliterator NFC = Transliterator.GetInstance("NFC");
            for (int i = 0; i < CANON.Length; ++i)
            {
                String @in = CANON[i][0];
                String expd = CANON[i][1];
                String expc = CANON[i][2];
                Expect(NFD, @in, expd);
                Expect(NFC, @in, expc);
            }

            Transliterator NFKD = Transliterator.GetInstance("NFKD");
            Transliterator NFKC = Transliterator.GetInstance("NFKC");
            for (int i = 0; i < COMPAT.Length; ++i)
            {
                String @in = COMPAT[i][0];
                String expkd = COMPAT[i][1];
                String expkc = COMPAT[i][2];
                Expect(NFKD, @in, expkd);
                Expect(NFKC, @in, expkc);
            }

            Transliterator t = Transliterator.GetInstance("NFD; [x]Remove");
            Expect(t, "\u010dx", "c\u030C");
        }

        /**
         * Test compound RBT rules.
         */
        [Test]
        public void TestCompoundRBT()
        {
            // Careful with spacing and ';' here:  Phrase this exactly
            // as toRules() is going to return it.  If toRules() changes
            // with regard to spacing or ';', then adjust this string.
            String rule = "::Hex-Any;\n" +
            "::Any-Lower;\n" +
            "a > '.A.';\n" +
            "b > '.B.';\n" +
            "::[^t]Any-Upper;";
            Transliterator t = Transliterator.CreateFromRules("Test", rule, Transliterator.Forward);
            if (t == null)
            {
                Errln("FAIL: createFromRules failed");
                return;
            }
            Expect(t, "\u0043at in the hat, bat on the mat",
            "C.A.t IN tHE H.A.t, .B..A.t ON tHE M.A.t");
            String r = t.ToRules(true);
            if (r.Equals(rule))
            {
                Logln("OK: toRules() => " + r);
            }
            else
            {
                Errln("FAIL: toRules() => " + r +
                        ", expected " + rule);
            }

            // Now test toRules
            t = Transliterator.GetInstance("Greek-Latin; Latin-Cyrillic", Transliterator.Forward);
            if (t == null)
            {
                Errln("FAIL: createInstance failed");
                return;
            }
            String exp = "::Greek-Latin;\n::Latin-Cyrillic;";
            r = t.ToRules(true);
            if (!r.Equals(exp))
            {
                Errln("FAIL: toRules() => " + r +
                        ", expected " + exp);
            }
            else
            {
                Logln("OK: toRules() => " + r);
            }

            // Round trip the result of toRules
            t = Transliterator.CreateFromRules("Test", r, Transliterator.Forward);
            if (t == null)
            {
                Errln("FAIL: createFromRules #2 failed");
                return;
            }
            else
            {
                Logln("OK: createFromRules(" + r + ") succeeded");
            }

            // Test toRules again
            r = t.ToRules(true);
            if (!r.Equals(exp))
            {
                Errln("FAIL: toRules() => " + r +
                        ", expected " + exp);
            }
            else
            {
                Logln("OK: toRules() => " + r);
            }

            // Test Foo(Bar) IDs.  Careful with spacing in id; make it conform
            // to what the regenerated ID will look like.
            String id = "Upper(Lower);(NFKC)";
            t = Transliterator.GetInstance(id, Transliterator.Forward);
            if (t == null)
            {
                Errln("FAIL: createInstance #2 failed");
                return;
            }
            if (t.ID.Equals(id))
            {
                Logln("OK: created " + id);
            }
            else
            {
                Errln("FAIL: createInstance(" + id +
                        ").ID => " + t.ID);
            }

            Transliterator u = t.GetInverse();
            if (u == null)
            {
                Errln("FAIL: createInverse failed");
                return;
            }
            exp = "NFKC();Lower(Upper)";
            if (u.ID.Equals(exp))
            {
                Logln("OK: createInverse(" + id + ") => " +
                        u.ID);
            }
            else
            {
                Errln("FAIL: createInverse(" + id + ") => " +
                        u.ID);
            }
        }

        /**
         * Compound filter semantics were orginially not implemented
         * correctly.  Originally, each component filter f(i) is replaced by
         * f'(i) = f(i) &amp;&amp; g, where g is the filter for the compound
         * transliterator.
         *
         * From Mark:
         *
         * Suppose and I have a transliterator X. Internally X is
         * "Greek-Latin; Latin-Cyrillic; Any-Lower". I use a filter [^A].
         *
         * The compound should convert all greek characters (through latin) to
         * cyrillic, then lowercase the result. The filter should say "don't
         * touch 'A' in the original". But because an intermediate result
         * happens to go through "A", the Greek Alpha gets hung up.
         */
        [Test]
        public void TestCompoundFilter()
        {
            Transliterator t = Transliterator.GetInstance
            ("Greek-Latin; Latin-Greek; Lower", Transliterator.Forward);
            t.Filter = (new UnicodeSet("[^A]"));

            // Only the 'A' at index 1 should remain unchanged
            Expect(t,
                    CharsToUnicodeString("BA\\u039A\\u0391"),
                    CharsToUnicodeString("\\u03b2A\\u03ba\\u03b1"));
        }

        /**
         * Test the "Remove" transliterator.
         */
        [Test]
        public void TestRemove()
        {
            Transliterator t = Transliterator.GetInstance("Remove[aeiou]");
            Expect(t, "The quick brown fox.",
            "Th qck brwn fx.");
        }

        [Test]
        public void TestToRules()
        {
            String RBT = "rbt";
            String SET = "set";
            String[] DATA = {
                RBT,
                "$a=\\u4E61; [$a] > A;",
                "[\\u4E61] > A;",

                RBT,
                "$white=[[:Zs:][:Zl:]]; $white{a} > A;",
                "[[:Zs:][:Zl:]]{a} > A;",

                SET,
                "[[:Zs:][:Zl:]]",
                "[[:Zs:][:Zl:]]",

                SET,
                "[:Ps:]",
                "[:Ps:]",

                SET,
                "[:L:]",
                "[:L:]",

                SET,
                "[[:L:]-[A]]",
                "[[:L:]-[A]]",

                SET,
                "[~[:Lu:][:Ll:]]",
                "[~[:Lu:][:Ll:]]",

                SET,
                "[~[a-z]]",
                "[~[a-z]]",

                RBT,
                "$white=[:Zs:]; $black=[^$white]; $black{a} > A;",
                "[^[:Zs:]]{a} > A;",

                RBT,
                "$a=[:Zs:]; $b=[[a-z]-$a]; $b{a} > A;",
                "[[a-z]-[:Zs:]]{a} > A;",

                RBT,
                "$a=[:Zs:]; $b=[$a&[a-z]]; $b{a} > A;",
                "[[:Zs:]&[a-z]]{a} > A;",

                RBT,
                "$a=[:Zs:]; $b=[x$a]; $b{a} > A;",
                "[x[:Zs:]]{a} > A;",

                RBT,
                "$accentMinus = [ [\\u0300-\\u0345] & [:M:] - [\\u0338]] ;"+
                "$macron = \\u0304 ;"+
                "$evowel = [aeiouyAEIOUY] ;"+
                "$iotasub = \\u0345 ;"+
                "($evowel $macron $accentMinus *) i > | $1 $iotasub ;",
                "([AEIOUYaeiouy]\\u0304[[\\u0300-\\u0345]&[:M:]-[\\u0338]]*)i > | $1 \\u0345;",

                RBT,
                "([AEIOUYaeiouy]\\u0304[[:M:]-[\\u0304\\u0345]]*)i > | $1 \\u0345;",
                "([AEIOUYaeiouy]\\u0304[[:M:]-[\\u0304\\u0345]]*)i > | $1 \\u0345;",
        };

            for (int d = 0; d < DATA.Length; d += 3)
            {
                if (DATA[d] == RBT)
                {
                    // Transliterator test
                    Transliterator t = Transliterator.CreateFromRules("ID",
                            DATA[d + 1], Transliterator.Forward);
                    if (t == null)
                    {
                        Errln("FAIL: createFromRules failed");
                        return;
                    }
                    String rules, escapedRules;
                    rules = t.ToRules(false);
                    escapedRules = t.ToRules(true);
                    String expRules = Utility.Unescape(DATA[d + 2]);
                    String expEscapedRules = DATA[d + 2];
                    if (rules.Equals(expRules))
                    {
                        Logln("Ok: " + DATA[d + 1] +
                                " => " + Utility.Escape(rules));
                    }
                    else
                    {
                        Errln("FAIL: " + DATA[d + 1] +
                                " => " + Utility.Escape(rules + ", exp " + expRules));
                    }
                    if (escapedRules.Equals(expEscapedRules))
                    {
                        Logln("Ok: " + DATA[d + 1] +
                                " => " + escapedRules);
                    }
                    else
                    {
                        Errln("FAIL: " + DATA[d + 1] +
                                " => " + escapedRules + ", exp " + expEscapedRules);
                    }

                }
                else
                {
                    // UnicodeSet test
                    String pat = DATA[d + 1];
                    String expToPat = DATA[d + 2];
                    UnicodeSet set = new UnicodeSet(pat);

                    // Adjust spacing etc. as necessary.
                    String toPat;
                    toPat = set.ToPattern(true);
                    if (expToPat.Equals(toPat))
                    {
                        Logln("Ok: " + pat +
                                " => " + toPat);
                    }
                    else
                    {
                        Errln("FAIL: " + pat +
                                " => " + Utility.Escape(toPat) +
                                ", exp " + Utility.Escape(pat));
                    }
                }
            }
        }

        [Test]
        public void TestContext()
        {
            TransliterationPosition pos = new TransliterationPosition(0, 2, 0, 1); // cs cl s l

            Expect("de > x; {d}e > y;",
                    "de",
                    "ye",
                    pos);

            Expect("ab{c} > z;",
                    "xadabdabcy",
            "xadabdabzy");
        }

        internal static String CharsToUnicodeString(String s)
        {
            return Utility.Unescape(s);
        }

        [Test]
        public void TestSupplemental()
        {

            Expect(CharsToUnicodeString("$a=\\U00010300; $s=[\\U00010300-\\U00010323];" +
            "a > $a; $s > i;"),
            CharsToUnicodeString("ab\\U0001030Fx"),
            CharsToUnicodeString("\\U00010300bix"));

            Expect(CharsToUnicodeString("$a=[a-z\\U00010300-\\U00010323];" +
                    "$b=[A-Z\\U00010400-\\U0001044D];" +
            "($a)($b) > $2 $1;"),
            CharsToUnicodeString("aB\\U00010300\\U00010400c\\U00010401\\U00010301D"),
            CharsToUnicodeString("Ba\\U00010400\\U00010300\\U00010401cD\\U00010301"));

            // k|ax\\U00010300xm

            // k|a\\U00010400\\U00010300xm
            // ky|\\U00010400\\U00010300xm
            // ky\\U00010400|\\U00010300xm

            // ky\\U00010400|\\U00010300\\U00010400m
            // ky\\U00010400y|\\U00010400m
            Expect(CharsToUnicodeString("$a=[a\\U00010300-\\U00010323];" +
                    "$a {x} > | @ \\U00010400;" +
            "{$a} [^\\u0000-\\uFFFF] > y;"),
            CharsToUnicodeString("kax\\U00010300xm"),
            CharsToUnicodeString("ky\\U00010400y\\U00010400m"));

            Expect(Transliterator.GetInstance("Any-Name"),
                    CharsToUnicodeString("\\U00010330\\U000E0061\\u00A0"),
            "\\N{GOTHIC LETTER AHSA}\\N{TAG LATIN SMALL LETTER A}\\N{NO-BREAK SPACE}");

            Expect(Transliterator.GetInstance("Name-Any"),
                    "\\N{GOTHIC LETTER AHSA}\\N{TAG LATIN SMALL LETTER A}\\N{NO-BREAK SPACE}",
                    CharsToUnicodeString("\\U00010330\\U000E0061\\u00A0"));

            Expect(Transliterator.GetInstance("Any-Hex/Unicode"),
                    CharsToUnicodeString("\\U00010330\\U0010FF00\\U000E0061\\u00A0"),
            "U+10330U+10FF00U+E0061U+00A0");

            Expect(Transliterator.GetInstance("Any-Hex/C"),
                    CharsToUnicodeString("\\U00010330\\U0010FF00\\U000E0061\\u00A0"),
            "\\U00010330\\U0010FF00\\U000E0061\\u00A0");

            Expect(Transliterator.GetInstance("Any-Hex/Perl"),
                    CharsToUnicodeString("\\U00010330\\U0010FF00\\U000E0061\\u00A0"),
            "\\x{10330}\\x{10FF00}\\x{E0061}\\x{A0}");

            Expect(Transliterator.GetInstance("Any-Hex/Java"),
                    CharsToUnicodeString("\\U00010330\\U0010FF00\\U000E0061\\u00A0"),
            "\\uD800\\uDF30\\uDBFF\\uDF00\\uDB40\\uDC61\\u00A0");

            Expect(Transliterator.GetInstance("Any-Hex/XML"),
                    CharsToUnicodeString("\\U00010330\\U0010FF00\\U000E0061\\u00A0"),
            "&#x10330;&#x10FF00;&#xE0061;&#xA0;");

            Expect(Transliterator.GetInstance("Any-Hex/XML10"),
                    CharsToUnicodeString("\\U00010330\\U0010FF00\\U000E0061\\u00A0"),
            "&#66352;&#1113856;&#917601;&#160;");

            Expect(Transliterator.GetInstance("[\\U000E0000-\\U000E0FFF] Remove"),
                    CharsToUnicodeString("\\U00010330\\U0010FF00\\U000E0061\\u00A0"),
                    CharsToUnicodeString("\\U00010330\\U0010FF00\\u00A0"));
        }

        [Test]
        public void TestQuantifier()
        {

            // Make sure @ in a quantified anteContext works
            Expect("a+ {b} > | @@ c; A > a; (a+ c) > '(' $1 ')';",
                    "AAAAAb",
            "aaa(aac)");

            // Make sure @ in a quantified postContext works
            Expect("{b} a+ > c @@ |; (a+) > '(' $1 ')';",
                    "baaaaa",
            "caa(aaa)");

            // Make sure @ in a quantified postContext with seg ref works
            Expect("{(b)} a+ > $1 @@ |; (a+) > '(' $1 ')';",
                    "baaaaa",
            "baa(aaa)");

            // Make sure @ past ante context doesn't enter ante context
            TransliterationPosition pos = new TransliterationPosition(0, 5, 3, 5);
            Expect("a+ {b} > | @@ c; x > y; (a+ c) > '(' $1 ')';",
                    "xxxab",
                    "xxx(ac)",
                    pos);

            // Make sure @ past post context doesn't pass limit
            TransliterationPosition pos2 = new TransliterationPosition(0, 4, 0, 2);
            Expect("{b} a+ > c @@ |; x > y; a > A;",
                    "baxx",
                    "caxx",
                    pos2);

            // Make sure @ past post context doesn't enter post context
            Expect("{b} a+ > c @@ |; x > y; a > A;",
                    "baxx",
            "cayy");

            Expect("(ab)? c > d;",
                    "c abc ababc",
            "d d abd");

            // NOTE: The (ab)+ when referenced just yields a single "ab",
            // not the full sequence of them.  This accords with perl behavior.
            Expect("(ab)+ {x} > '(' $1 ')';",
                    "x abx ababxy",
            "x ab(ab) abab(ab)y");

            Expect("b+ > x;",
                    "ac abc abbc abbbc",
            "ac axc axc axc");

            Expect("[abc]+ > x;",
                    "qac abrc abbcs abtbbc",
            "qx xrx xs xtx");

            Expect("q{(ab)+} > x;",
                    "qa qab qaba qababc qaba",
            "qa qx qxa qxc qxa");

            Expect("q(ab)* > x;",
                    "qa qab qaba qababc",
            "xa x xa xc");

            // NOTE: The (ab)+ when referenced just yields a single "ab",
            // not the full sequence of them.  This accords with perl behavior.
            Expect("q(ab)* > '(' $1 ')';",
                    "qa qab qaba qababc",
            "()a (ab) (ab)a (ab)c");

            // 'foo'+ and 'foo'* -- the quantifier should apply to the entire
            // quoted string
            Expect("'ab'+ > x;",
                    "bb ab ababb",
            "bb x xb");

            // $foo+ and $foo* -- the quantifier should apply to the entire
            // variable reference
            Expect("$var = ab; $var+ > x;",
                    "bb ab ababb",
            "bb x xb");
        }

        internal class TestFact : ITransliteratorFactory
        {
            internal class NameableNullTrans : Transliterator
            {
                public NameableNullTrans(String id)
                        : base(id, null)
                {
                }

                protected override void HandleTransliterate(IReplaceable text,
                        TransliterationPosition offsets, bool incremental)
                {
                    offsets.Start = offsets.Limit;
                }
            }
            String id;
            public TestFact(String theID)
            {
                id = theID;
            }
            public virtual Transliterator GetInstance(String ignoredID)
            {
                return new NameableNullTrans(id);
            }
        }

        [Test]
        public void TestSTV()
        {
            using (var es = Transliterator.GetAvailableSources().GetEnumerator())
                for (int i = 0; es.MoveNext(); ++i)
                {
                    String source = (String)es.Current;
                    Logln("" + i + ": " + source);
                    if (source.Length == 0)
                    {
                        Errln("FAIL: empty source");
                        continue;
                    }
                    using (var et = Transliterator.GetAvailableTargets(source).GetEnumerator())
                        for (int j = 0; et.MoveNext(); ++j)
                        {
                            String target = (String)et.Current;
                            Logln(" " + j + ": " + target);
                            if (target.Length == 0)
                            {
                                Errln("FAIL: empty target");
                                continue;
                            }
                            using (var ev = Transliterator.GetAvailableVariants(source, target).GetEnumerator())
                                for (int k = 0; ev.MoveNext(); ++k)
                                {
                                    String variant = (String)ev.Current;
                                    if (variant.Length == 0)
                                    {
                                        Logln("  " + k + ": <empty>");
                                    }
                                    else
                                    {
                                        Logln("  " + k + ": " + variant);
                                    }
                                }
                        }
                }

            // Test registration
            String[] IDS = { "Fieruwer", "Seoridf-Sweorie", "Oewoir-Oweri/Vsie" };
            String[] FULL_IDS = { "Any-Fieruwer", "Seoridf-Sweorie", "Oewoir-Oweri/Vsie" };
            String[] SOURCES = { null, "Seoridf", "Oewoir" };
            for (int i = 0; i < 3; ++i)
            {
                Transliterator.RegisterFactory(IDS[i], new TestFact(IDS[i]));
                try
                {
                    Transliterator t = Transliterator.GetInstance(IDS[i]);
                    if (t.ID.Equals(IDS[i]))
                    {
                        Logln("Ok: Registration/creation succeeded for ID " +
                                IDS[i]);
                    }
                    else
                    {
                        Errln("FAIL: Registration of ID " +
                                IDS[i] + " creates ID " + t.ID);
                    }
                    Transliterator.Unregister(IDS[i]);
                    try
                    {
                        t = Transliterator.GetInstance(IDS[i]);
                        Errln("FAIL: Unregistration failed for ID " +
                                IDS[i] + "; still receiving ID " + t.ID);
                    }
                    catch (ArgumentException e2)
                    {
                        // Good; this is what we expect
                        Logln("Ok; Unregistered " + IDS[i]);
                    }
                }
                catch (ArgumentException e)
                {
                    Errln("FAIL: Registration/creation failed for ID " +
                            IDS[i]);
                }
                finally
                {
                    Transliterator.Unregister(IDS[i]);
                }
            }

            // Make sure getAvailable API reflects removal
            foreach (var id in Transliterator.GetAvailableIDs())
            {
                for (int i = 0; i < 3; ++i)
                {
                    if (id.Equals(FULL_IDS[i]))
                    {
                        Errln("FAIL: unregister(" + id + ") failed");
                    }
                }
            }
            foreach (var t in Transliterator.GetAvailableTargets("Any"))
            {
                if (t.Equals(IDS[0]))
                {
                    Errln("FAIL: unregister(Any-" + t + ") failed");
                }
            }
            foreach (var s in Transliterator.GetAvailableSources())
            {
                for (int i = 0; i < 3; ++i)
                {
                    if (SOURCES[i] == null) continue;
                    if (s.Equals(SOURCES[i]))
                    {
                        Errln("FAIL: unregister(" + s + "-*) failed");
                    }
                }
            }
        }

        /**
         * Test inverse of Greek-Latin; Title()
         */
        [Test]
        public void TestCompoundInverse()
        {
            Transliterator t = Transliterator.GetInstance
            ("Greek-Latin; Title()", Transliterator.Reverse);
            if (t == null)
            {
                Errln("FAIL: createInstance");
                return;
            }
            String exp = "(Title);Latin-Greek";
            if (t.ID.Equals(exp))
            {
                Logln("Ok: inverse of \"Greek-Latin; Title()\" is \"" +
                        t.ID);
            }
            else
            {
                Errln("FAIL: inverse of \"Greek-Latin; Title()\" is \"" +
                        t.ID + "\", expected \"" + exp + "\"");
            }
        }

        /**
         * Test NFD chaining with RBT
         */
        [Test]
        public void TestNFDChainRBT()
        {
            Transliterator t = Transliterator.CreateFromRules(
                    "TEST", "::NFD; aa > Q; a > q;",
                    Transliterator.Forward);
            Logln(t.ToRules(true));
            Expect(t, "aa", "Q");
        }

        /**
         * Inverse of "Null" should be "Null". (J21)
         */
        [Test]
        public void TestNullInverse()
        {
            Transliterator t = Transliterator.GetInstance("Null");
            Transliterator u = t.GetInverse();
            if (!u.ID.Equals("Null"))
            {
                Errln("FAIL: Inverse of Null should be Null");
            }
        }

        /**
         * Check ID of inverse of alias. (J22)
         */
        [Test]
        public void TestAliasInverseID()
        {
            String ID = "Latin-Hangul"; // This should be any alias ID with an inverse
            Transliterator t = Transliterator.GetInstance(ID);
            Transliterator u = t.GetInverse();
            String exp = "Hangul-Latin";
            String got = u.ID;
            if (!got.Equals(exp))
            {
                Errln("FAIL: Inverse of " + ID + " is " + got +
                        ", expected " + exp);
            }
        }

        /**
         * Test IDs of inverses of compound transliterators. (J20)
         */
        [Test]
        public void TestCompoundInverseID()
        {
            String ID = "Latin-Jamo;NFC(NFD)";
            Transliterator t = Transliterator.GetInstance(ID);
            Transliterator u = t.GetInverse();
            String exp = "NFD(NFC);Jamo-Latin";
            String got = u.ID;
            if (!got.Equals(exp))
            {
                Errln("FAIL: Inverse of " + ID + " is " + got +
                        ", expected " + exp);
            }
        }

        /**
         * Test undefined variable.
         */
        [Test]
        public void TestUndefinedVariable()
        {
            String rule = "$initial } a <> \u1161;";
            try
            {
                Transliterator.CreateFromRules("<ID>", rule, Transliterator.Forward);
            }
            catch (ArgumentException e)
            {
                Logln("OK: Got exception for " + rule + ", as expected: " +
                        e.ToString());
                return;
            }
            Errln("Fail: bogus rule " + rule + " compiled without error");
        }

        /**
         * Test empty context.
         */
        [Test]
        public void TestEmptyContext()
        {
            Expect(" { a } > b;", "xay a ", "xby b ");
        }

        /**
         * Test compound filter ID syntax
         */
        [Test]
        public void TestCompoundFilterID()
        {
            String[] DATA = {
                // Col. 1 = ID or rule set (latter must start with #)

                // = columns > 1 are null if expect col. 1 to be illegal =

                // Col. 2 = direction, "F..." or "R..."
                // Col. 3 = source string
                // Col. 4 = exp result

                "[abc]; [abc]", null, null, null, // multiple filters
                "Latin-Greek; [abc];", null, null, null, // misplaced filter
                "[b]; Latin-Greek; Upper; ([xyz])", "F", "abc", "a\u0392c",
                "[b]; (Lower); Latin-Greek; Upper(); ([\u0392])", "R", "\u0391\u0392\u0393", "\u0391b\u0393",
                "#\n::[b]; ::Latin-Greek; ::Upper; ::([xyz]);", "F", "abc", "a\u0392c",
                "#\n::[b]; ::(Lower); ::Latin-Greek; ::Upper(); ::([\u0392]);", "R", "\u0391\u0392\u0393", "\u0391b\u0393",
        };

            for (int i = 0; i < DATA.Length; i += 4)
            {
                String id = DATA[i];
                TransliterationDirection direction = (DATA[i + 1] != null && DATA[i + 1][0] == 'R') ?
                        Transliterator.Reverse : Transliterator.Forward;
                String source = DATA[i + 2];
                String exp = DATA[i + 3];
                bool expOk = (DATA[i + 1] != null);
                Transliterator t = null;
                ArgumentException e = null;
                try
                {
                    if (id[0] == '#')
                    {
                        t = Transliterator.CreateFromRules("ID", id, direction);
                    }
                    else
                    {
                        t = Transliterator.GetInstance(id, direction);
                    }
                }
                catch (ArgumentException ee)
                {
                    e = ee;
                }
                bool ok = (t != null && e == null);
                if (ok == expOk)
                {
                    Logln("Ok: " + id + " => " + t +
                            (e != null ? (", " + e.ToString()) : ""));
                    if (source != null)
                    {
                        Expect(t, source, exp);
                    }
                }
                else
                {
                    Errln("FAIL: " + id + " => " + t +
                            (e != null ? (", " + e.ToString()) : ""));
                }
            }
        }

        /**
         * Test new property set syntax
         */
        [Test]
        public void TestPropertySet()
        {
            Expect("a>A; \\p{Lu}>x; \\p{Any}>y;", "abcDEF", "Ayyxxx");
            Expect("(.+)>'[' $1 ']';", " a stitch \n in time \r saves 9",
            "[ a stitch ]\n[ in time ]\r[ saves 9]");
        }

        /**
         * Test various failure points of the new 2.0 engine.
         */
        [Test]
        public void TestNewEngine()
        {
            Transliterator t = Transliterator.GetInstance("Latin-Hiragana");
            // Katakana should be untouched
            Expect(t, "a\u3042\u30A2", "\u3042\u3042\u30A2");

            if (true)
            {
                // This test will only work if Transliterator.ROLLBACK is
                // true.  Otherwise, this test will fail, revealing a
                // limitation of global filters in incremental mode.

                Transliterator a =
                    Transliterator.CreateFromRules("a_to_A", "a > A;", Transliterator.Forward);
                Transliterator A =
                    Transliterator.CreateFromRules("A_to_b", "A > b;", Transliterator.Forward);

                //Transliterator array[] = new Transliterator[] {
                //    a,
                //    Transliterator.GetInstance("NFD"),
                //    A };
                //t = Transliterator.GetInstance(array, new UnicodeSet("[:Ll:]"));

                try
                {
                    Transliterator.RegisterInstance(a);
                    Transliterator.RegisterInstance(A);

                    t = Transliterator.GetInstance("[:Ll:];a_to_A;NFD;A_to_b");
                    Expect(t, "aAaA", "bAbA");

                    Transliterator[] u = t.GetElements();
                    assertTrue("getElements().Length", u.Length == 3);
                    assertEquals("getElements()[0]", u[0].ID, "a_to_A");
                    assertEquals("getElements()[1]", u[1].ID, "NFD");
                    assertEquals("getElements()[2]", u[2].ID, "A_to_b");

                    t = Transliterator.GetInstance("a_to_A;NFD;A_to_b");
                    t.Filter = (new UnicodeSet("[:Ll:]"));
                    Expect(t, "aAaA", "bAbA");
                }
                finally
                {
                    Transliterator.Unregister("a_to_A");
                    Transliterator.Unregister("A_to_b");
                }
            }

            Expect("$smooth = x; $macron = q; [:^L:] { ([aeiouyAEIOUY] $macron?) } [^aeiouyAEIOUY$smooth$macron] > | $1 $smooth ;",
                    "a",
            "ax");

            String gr =
                "$ddot = \u0308 ;" +
                "$lcgvowel = [\u03b1\u03b5\u03b7\u03b9\u03bf\u03c5\u03c9] ;" +
                "$rough = \u0314 ;" +
                "($lcgvowel+ $ddot?) $rough > h | $1 ;" +
                "\u03b1 <> a ;" +
                "$rough <> h ;";

            Expect(gr, "\u03B1\u0314", "ha");
        }

        /**
         * Test quantified segment behavior.  We want:
         * ([abc])+ > x $1 x; applied to "cba" produces "xax"
         */
        [Test]
        public void TestQuantifiedSegment()
        {
            // The normal case
            Expect("([abc]+) > x $1 x;", "cba", "xcbax");

            // The tricky case; the quantifier is around the segment
            Expect("([abc])+ > x $1 x;", "cba", "xax");

            // Tricky case in reverse direction
            Expect("([abc])+ { q > x $1 x;", "cbaq", "cbaxax");

            // Check post-context segment
            Expect("{q} ([a-d])+ > '(' $1 ')';", "ddqcba", "dd(a)cba");

            // Test toRule/toPattern for non-quantified segment.
            // Careful with spacing here.
            String r = "([a-c]){q} > x $1 x;";
            Transliterator t = Transliterator.CreateFromRules("ID", r, Transliterator.Forward);
            String rr = t.ToRules(true);
            if (!r.Equals(rr))
            {
                Errln("FAIL: \"" + r + "\" x toRules() => \"" + rr + "\"");
            }
            else
            {
                Logln("Ok: \"" + r + "\" x toRules() => \"" + rr + "\"");
            }

            // Test toRule/toPattern for quantified segment.
            // Careful with spacing here.
            r = "([a-c])+{q} > x $1 x;";
            t = Transliterator.CreateFromRules("ID", r, Transliterator.Forward);
            rr = t.ToRules(true);
            if (!r.Equals(rr))
            {
                Errln("FAIL: \"" + r + "\" x toRules() => \"" + rr + "\"");
            }
            else
            {
                Logln("Ok: \"" + r + "\" x toRules() => \"" + rr + "\"");
            }
        }

        //======================================================================
        // Ram's tests
        //======================================================================
        /* this test performs  test of rules in ISO 15915 */
        [Test]
        public void TestDevanagariLatinRT()
        {
            String[] source = {
                "bh\u0101rata",
                "kra",
                "k\u1E63a",
                "khra",
                "gra",
                "\u1E45ra",
                "cra",
                "chra",
                "j\u00F1a",
                "jhra",
                "\u00F1ra",
                "\u1E6Dya",
                "\u1E6Dhra",
                "\u1E0Dya",
                //"r\u0323ya", // \u095c is not valid in Devanagari
                "\u1E0Dhya",
                "\u1E5Bhra",
                "\u1E47ra",
                "tta",
                "thra",
                "dda",
                "dhra",
                "nna",
                "pra",
                "phra",
                "bra",
                "bhra",
                "mra",
                "\u1E49ra",
                //"l\u0331ra",
                "yra",
                "\u1E8Fra",
                //"l-",
                "vra",
                "\u015Bra",
                "\u1E63ra",
                "sra",
                "hma",
                "\u1E6D\u1E6Da",
                "\u1E6D\u1E6Dha",
                "\u1E6Dh\u1E6Dha",
                "\u1E0D\u1E0Da",
                "\u1E0D\u1E0Dha",
                "\u1E6Dya",
                "\u1E6Dhya",
                "\u1E0Dya",
                "\u1E0Dhya",
                // Not roundtrippable --
                // \u0939\u094d\u094d\u092E  - hma
                // \u0939\u094d\u092E         - hma
                // CharsToUnicodeString("hma"),
                "hya",
                "\u015Br\u0325",
                "\u015Bca",
                "\u0115",
                "san\u0304j\u012Bb s\u0113nagupta",
                "\u0101nand vaddir\u0101ju",
        };
            String[] expected = {
                "\u092D\u093E\u0930\u0924",    /* bha\u0304rata */
                "\u0915\u094D\u0930",          /* kra         */
                "\u0915\u094D\u0937",          /* ks\u0323a  */
                "\u0916\u094D\u0930",          /* khra        */
                "\u0917\u094D\u0930",          /* gra         */
                "\u0919\u094D\u0930",          /* n\u0307ra  */
                "\u091A\u094D\u0930",          /* cra         */
                "\u091B\u094D\u0930",          /* chra        */
                "\u091C\u094D\u091E",          /* jn\u0303a  */
                "\u091D\u094D\u0930",          /* jhra        */
                "\u091E\u094D\u0930",          /* n\u0303ra  */
                "\u091F\u094D\u092F",          /* t\u0323ya  */
                "\u0920\u094D\u0930",          /* t\u0323hra */
                "\u0921\u094D\u092F",          /* d\u0323ya  */
                //"\u095C\u094D\u092F",          /* r\u0323ya  */ // \u095c is not valid in Devanagari
                "\u0922\u094D\u092F",          /* d\u0323hya */
                "\u0922\u093C\u094D\u0930",    /* r\u0323hra */
                "\u0923\u094D\u0930",          /* n\u0323ra  */
                "\u0924\u094D\u0924",          /* tta         */
                "\u0925\u094D\u0930",          /* thra        */
                "\u0926\u094D\u0926",          /* dda         */
                "\u0927\u094D\u0930",          /* dhra        */
                "\u0928\u094D\u0928",          /* nna         */
                "\u092A\u094D\u0930",          /* pra         */
                "\u092B\u094D\u0930",          /* phra        */
                "\u092C\u094D\u0930",          /* bra         */
                "\u092D\u094D\u0930",          /* bhra        */
                "\u092E\u094D\u0930",          /* mra         */
                "\u0929\u094D\u0930",          /* n\u0331ra  */
                //"\u0934\u094D\u0930",          /* l\u0331ra  */
                "\u092F\u094D\u0930",          /* yra         */
                "\u092F\u093C\u094D\u0930",    /* y\u0307ra  */
                //"l-",
                "\u0935\u094D\u0930",          /* vra         */
                "\u0936\u094D\u0930",          /* s\u0301ra  */
                "\u0937\u094D\u0930",          /* s\u0323ra  */
                "\u0938\u094D\u0930",          /* sra         */
                "\u0939\u094d\u092E",          /* hma         */
                "\u091F\u094D\u091F",          /* t\u0323t\u0323a  */
                "\u091F\u094D\u0920",          /* t\u0323t\u0323ha */
                "\u0920\u094D\u0920",          /* t\u0323ht\u0323ha*/
                "\u0921\u094D\u0921",          /* d\u0323d\u0323a  */
                "\u0921\u094D\u0922",          /* d\u0323d\u0323ha */
                "\u091F\u094D\u092F",          /* t\u0323ya  */
                "\u0920\u094D\u092F",          /* t\u0323hya */
                "\u0921\u094D\u092F",          /* d\u0323ya  */
                "\u0922\u094D\u092F",          /* d\u0323hya */
                // "hma",                         /* hma         */
                "\u0939\u094D\u092F",          /* hya         */
                "\u0936\u0943",                /* s\u0301r\u0325a  */
                "\u0936\u094D\u091A",          /* s\u0301ca  */
                "\u090d",                      /* e\u0306    */
                "\u0938\u0902\u091C\u0940\u092C\u094D \u0938\u0947\u0928\u0917\u0941\u092A\u094D\u0924",
                "\u0906\u0928\u0902\u0926\u094D \u0935\u0926\u094D\u0926\u093F\u0930\u093E\u091C\u0941",
        };

            Transliterator latinToDev = Transliterator.GetInstance("Latin-Devanagari", Transliterator.Forward);
            Transliterator devToLatin = Transliterator.GetInstance("Devanagari-Latin", Transliterator.Forward);

            for (int i = 0; i < source.Length; i++)
            {
                Expect(latinToDev, (source[i]), (expected[i]));
                Expect(devToLatin, (expected[i]), (source[i]));
            }

        }
        [Test]
        public void TestTeluguLatinRT()
        {
            String[] source = {
                "raghur\u0101m vi\u015Bvan\u0101dha",                           /* Raghuram Viswanadha    */
                "\u0101nand vaddir\u0101ju",                                    /* Anand Vaddiraju        */
                "r\u0101j\u012Bv ka\u015Barab\u0101da",                         /* Rajeev Kasarabada      */
                "san\u0304j\u012Bv ka\u015Barab\u0101da",                       /* sanjeev kasarabada     */
                "san\u0304j\u012Bb sen'gupta",                                  /* sanjib sengupata       */
                "amar\u0113ndra hanum\u0101nula",                               /* Amarendra hanumanula   */
                "ravi kum\u0101r vi\u015Bvan\u0101dha",                         /* Ravi Kumar Viswanadha  */
                "\u0101ditya kandr\u0113gula",                                  /* Aditya Kandregula      */
                "\u015Br\u012Bdhar ka\u1E47\u1E6Dama\u015Be\u1E6D\u1E6Di",      /* Shridhar Kantamsetty   */
                "m\u0101dhav de\u015Be\u1E6D\u1E6Di"                            /* Madhav Desetty         */
        };

            String[] expected = {
                "\u0c30\u0c18\u0c41\u0c30\u0c3e\u0c2e\u0c4d \u0c35\u0c3f\u0c36\u0c4d\u0c35\u0c28\u0c3e\u0c27",
                "\u0c06\u0c28\u0c02\u0c26\u0c4d \u0C35\u0C26\u0C4D\u0C26\u0C3F\u0C30\u0C3E\u0C1C\u0C41",
                "\u0c30\u0c3e\u0c1c\u0c40\u0c35\u0c4d \u0c15\u0c36\u0c30\u0c2c\u0c3e\u0c26",
                "\u0c38\u0c02\u0c1c\u0c40\u0c35\u0c4d \u0c15\u0c36\u0c30\u0c2c\u0c3e\u0c26",
                "\u0c38\u0c02\u0c1c\u0c40\u0c2c\u0c4d \u0c38\u0c46\u0c28\u0c4d\u0c17\u0c41\u0c2a\u0c4d\u0c24",
                "\u0c05\u0c2e\u0c30\u0c47\u0c02\u0c26\u0c4d\u0c30 \u0c39\u0c28\u0c41\u0c2e\u0c3e\u0c28\u0c41\u0c32",
                "\u0c30\u0c35\u0c3f \u0c15\u0c41\u0c2e\u0c3e\u0c30\u0c4d \u0c35\u0c3f\u0c36\u0c4d\u0c35\u0c28\u0c3e\u0c27",
                "\u0c06\u0c26\u0c3f\u0c24\u0c4d\u0c2f \u0C15\u0C02\u0C26\u0C4D\u0C30\u0C47\u0C17\u0C41\u0c32",
                "\u0c36\u0c4d\u0c30\u0c40\u0C27\u0C30\u0C4D \u0c15\u0c02\u0c1f\u0c2e\u0c36\u0c46\u0c1f\u0c4d\u0c1f\u0c3f",
                "\u0c2e\u0c3e\u0c27\u0c35\u0c4d \u0c26\u0c46\u0c36\u0c46\u0c1f\u0c4d\u0c1f\u0c3f",
        };


            Transliterator latinToDev = Transliterator.GetInstance("Latin-Telugu", Transliterator.Forward);
            Transliterator devToLatin = Transliterator.GetInstance("Telugu-Latin", Transliterator.Forward);

            for (int i = 0; i < source.Length; i++)
            {
                Expect(latinToDev, (source[i]), (expected[i]));
                Expect(devToLatin, (expected[i]), (source[i]));
            }
        }

        [Test]
        public void TestSanskritLatinRT()
        {
            int MAX_LEN = 15;
            String[] source = {
                "rmk\u1E63\u0113t",
                "\u015Br\u012Bmad",
                "bhagavadg\u012Bt\u0101",
                "adhy\u0101ya",
                "arjuna",
                "vi\u1E63\u0101da",
                "y\u014Dga",
                "dhr\u0325tar\u0101\u1E63\u1E6Dra",
                "uv\u0101cr\u0325",
                "dharmak\u1E63\u0113tr\u0113",
                "kuruk\u1E63\u0113tr\u0113",
                "samav\u0113t\u0101",
                "yuyutsava\u1E25",
                "m\u0101mak\u0101\u1E25",
                // "p\u0101\u1E47\u1E0Dav\u0101\u015Bcaiva",
                "kimakurvata",
                "san\u0304java",
        };
            String[] expected = {
                "\u0930\u094D\u092E\u094D\u0915\u094D\u0937\u0947\u0924\u094D",
                "\u0936\u094d\u0930\u0940\u092e\u0926\u094d",
                "\u092d\u0917\u0935\u0926\u094d\u0917\u0940\u0924\u093e",
                "\u0905\u0927\u094d\u092f\u093e\u092f",
                "\u0905\u0930\u094d\u091c\u0941\u0928",
                "\u0935\u093f\u0937\u093e\u0926",
                "\u092f\u094b\u0917",
                "\u0927\u0943\u0924\u0930\u093e\u0937\u094d\u091f\u094d\u0930",
                "\u0909\u0935\u093E\u091A\u0943",
                "\u0927\u0930\u094d\u092e\u0915\u094d\u0937\u0947\u0924\u094d\u0930\u0947",
                "\u0915\u0941\u0930\u0941\u0915\u094d\u0937\u0947\u0924\u094d\u0930\u0947",
                "\u0938\u092e\u0935\u0947\u0924\u093e",
                "\u092f\u0941\u092f\u0941\u0924\u094d\u0938\u0935\u0903",
                "\u092e\u093e\u092e\u0915\u093e\u0903",
                //"\u092a\u093e\u0923\u094d\u0921\u0935\u093e\u0936\u094d\u091a\u0948\u0935",
                "\u0915\u093f\u092e\u0915\u0941\u0930\u094d\u0935\u0924",
                "\u0938\u0902\u091c\u0935",
        };

            Transliterator latinToDev = Transliterator.GetInstance("Latin-Devanagari", Transliterator.Forward);
            Transliterator devToLatin = Transliterator.GetInstance("Devanagari-Latin", Transliterator.Forward);
            for (int i = 0; i < MAX_LEN; i++)
            {
                Expect(latinToDev, (source[i]), (expected[i]));
                Expect(devToLatin, (expected[i]), (source[i]));
            }
        }

        [Test]
        public void TestCompoundLatinRT()
        {
            int MAX_LEN = 15;
            String[] source = {
                "rmk\u1E63\u0113t",
                "\u015Br\u012Bmad",
                "bhagavadg\u012Bt\u0101",
                "adhy\u0101ya",
                "arjuna",
                "vi\u1E63\u0101da",
                "y\u014Dga",
                "dhr\u0325tar\u0101\u1E63\u1E6Dra",
                "uv\u0101cr\u0325",
                "dharmak\u1E63\u0113tr\u0113",
                "kuruk\u1E63\u0113tr\u0113",
                "samav\u0113t\u0101",
                "yuyutsava\u1E25",
                "m\u0101mak\u0101\u1E25",
                // "p\u0101\u1E47\u1E0Dav\u0101\u015Bcaiva",
                "kimakurvata",
                "san\u0304java"
        };
            String[] expected = {
                "\u0930\u094D\u092E\u094D\u0915\u094D\u0937\u0947\u0924\u094D",
                "\u0936\u094d\u0930\u0940\u092e\u0926\u094d",
                "\u092d\u0917\u0935\u0926\u094d\u0917\u0940\u0924\u093e",
                "\u0905\u0927\u094d\u092f\u093e\u092f",
                "\u0905\u0930\u094d\u091c\u0941\u0928",
                "\u0935\u093f\u0937\u093e\u0926",
                "\u092f\u094b\u0917",
                "\u0927\u0943\u0924\u0930\u093e\u0937\u094d\u091f\u094d\u0930",
                "\u0909\u0935\u093E\u091A\u0943",
                "\u0927\u0930\u094d\u092e\u0915\u094d\u0937\u0947\u0924\u094d\u0930\u0947",
                "\u0915\u0941\u0930\u0941\u0915\u094d\u0937\u0947\u0924\u094d\u0930\u0947",
                "\u0938\u092e\u0935\u0947\u0924\u093e",
                "\u092f\u0941\u092f\u0941\u0924\u094d\u0938\u0935\u0903",
                "\u092e\u093e\u092e\u0915\u093e\u0903",
                //  "\u092a\u093e\u0923\u094d\u0921\u0935\u093e\u0936\u094d\u091a\u0948\u0935",
                "\u0915\u093f\u092e\u0915\u0941\u0930\u094d\u0935\u0924",
                "\u0938\u0902\u091c\u0935"
        };

            Transliterator latinToDevToLatin = Transliterator.GetInstance("Latin-Devanagari;Devanagari-Latin", Transliterator.Forward);
            Transliterator devToLatinToDev = Transliterator.GetInstance("Devanagari-Latin;Latin-Devanagari", Transliterator.Forward);
            for (int i = 0; i < MAX_LEN; i++)
            {
                Expect(latinToDevToLatin, (source[i]), (source[i]));
                Expect(devToLatinToDev, (expected[i]), (expected[i]));
            }
        }
        /**
         * Test Gurmukhi-Devanagari Tippi and Bindi
         */
        [Test]
        public void TestGurmukhiDevanagari()
        {
            // the rule says:
            // (\u0902) (when preceded by vowel)      --->  (\u0A02)
            // (\u0902) (when preceded by consonant)  --->  (\u0A70)

            UnicodeSet vowel = new UnicodeSet("[\u0905-\u090A \u090F\u0910\u0913\u0914 \u093e-\u0942\u0947\u0948\u094B\u094C\u094D]");
            UnicodeSet non_vowel = new UnicodeSet("[\u0915-\u0928\u092A-\u0930]");

            UnicodeSetIterator vIter = new UnicodeSetIterator(vowel);
            UnicodeSetIterator nvIter = new UnicodeSetIterator(non_vowel);
            Transliterator trans = Transliterator.GetInstance("Devanagari-Gurmukhi");
            StringBuffer src = new StringBuffer(" \u0902");
            StringBuffer expect = new StringBuffer(" \u0A02");
            while (vIter.Next())
            {
                src[0] = (char)vIter.Codepoint;
                expect[0] = (char)(vIter.Codepoint + 0x0100);
                Expect(trans, src.ToString(), expect.ToString());
            }

            expect[1] = '\u0A70';
            while (nvIter.Next())
            {
                //src.setCharAt(0,(char) nvIter.codepoint);
                src[0] = (char)nvIter.Codepoint;
                expect[0] = (char)(nvIter.Codepoint + 0x0100);
                Expect(trans, src.ToString(), expect.ToString());
            }
        }
        /**
         * Test instantiation from a locale.
         */
        [Test]
        public void TestLocaleInstantiation()
        {
            Transliterator t;
            try
            {
                t = Transliterator.GetInstance("te_IN-Latin");
                //expect(t, "\u0430", "a");
            }
            catch (ArgumentException ex)
            {
                Warnln("Could not load locale data for obtaining the script used in the locale te_IN. " + ex.ToString());
            }
            try
            {
                t = Transliterator.GetInstance("ru_RU-Latin");
                Expect(t, "\u0430", "a");
            }
            catch (ArgumentException ex)
            {
                Warnln("Could not load locale data for obtaining the script used in the locale ru_RU. " + ex.ToString());
            }
            try
            {
                t = Transliterator.GetInstance("en-el");
                Expect(t, "a", "\u03B1");
            }
            catch (ArgumentException ex)
            {
                Warnln("Could not load locale data for obtaining the script used in the locale el. " + ex.ToString());
            }
        }

        /**
         * Test title case handling of accent (should ignore accents)
         */
        [Test]
        public void TestTitleAccents()
        {
            Transliterator t = Transliterator.GetInstance("Title");
            Expect(t, "a\u0300b can't abe", "A\u0300b Can't Abe");
        }

        /**
         * Basic test of a locale resource based rule.
         */
        [Test]
        public void TestLocaleResource()
        {
            String[] DATA = {
                // id                    from             to
                "Latin-Greek/UNGEGN",    "b",             "\u03bc\u03c0",
                "Latin-el",              "b",             "\u03bc\u03c0",
                "Latin-Greek",           "b",             "\u03B2",
                "Greek-Latin/UNGEGN",    "\u03B2",        "v",
                "el-Latin",              "\u03B2",        "v",
                "Greek-Latin",           "\u03B2",        "b",
        };
            for (int i = 0; i < DATA.Length; i += 3)
            {
                Transliterator t = Transliterator.GetInstance(DATA[i]);
                Expect(t, DATA[i + 1], DATA[i + 2]);
            }
        }

        /**
         * Make sure parse errors reference the right line.
         */
        [Test]
        public void TestParseError()
        {
            String rule =
                "a > b;\n" +
                "# more stuff\n" +
                "d << b;";
            try
            {
                Transliterator t = Transliterator.CreateFromRules("ID", rule, Transliterator.Forward);
                if (t != null)
                {
                    Errln("FAIL: Did not get expected exception");
                }
            }
            catch (ArgumentException e)
            {
                String err = e.Message;
                if (err.IndexOf("d << b", StringComparison.Ordinal) >= 0)
                {
                    Logln("Ok: " + err);
                }
                else
                {
                    Errln("FAIL: " + err);
                }
                return;
            }
            Errln("FAIL: no syntax error");
        }

        /**
         * Make sure sets on output are disallowed.
         */
        [Test]
        public void TestOutputSet()
        {
            String rule = "$set = [a-cm-n]; b > $set;";
            Transliterator t = null;
            try
            {
                t = Transliterator.CreateFromRules("ID", rule, Transliterator.Forward);
                if (t != null)
                {
                    Errln("FAIL: Did not get the expected exception");
                }
            }
            catch (ArgumentException e)
            {
                Logln("Ok: " + e.ToString());
                return;
            }
            Errln("FAIL: No syntax error");
        }

        /**
         * Test the use variable range pragma, making sure that use of
         * variable range characters is detected and flagged as an error.
         */
        [Test]
        public void TestVariableRange()
        {
            String rule = "use variable range 0x70 0x72; a > A; b > B; q > Q;";
            try
            {
                Transliterator t =
                    Transliterator.CreateFromRules("ID", rule, Transliterator.Forward);
                if (t != null)
                {
                    Errln("FAIL: Did not get the expected exception");
                }
            }
            catch (ArgumentException e)
            {
                Logln("Ok: " + e.ToString());
                return;
            }
            Errln("FAIL: No syntax error");
        }

        /**
         * Test invalid post context error handling
         */
        [Test]
        public void TestInvalidPostContext()
        {
            try
            {
                Transliterator t =
                    Transliterator.CreateFromRules("ID", "a}b{c>d;", Transliterator.Forward);
                if (t != null)
                {
                    Errln("FAIL: Did not get the expected exception");
                }
            }
            catch (ArgumentException e)
            {
                String msg = e.ToString();
                if (msg.IndexOf("a}b{c", StringComparison.Ordinal) >= 0)
                {
                    Logln("Ok: " + msg);
                }
                else
                {
                    Errln("FAIL: " + msg);
                }
                return;
            }
            Errln("FAIL: No syntax error");
        }

        /**
         * Test ID form variants
         */
        [Test]
        public void TestIDForms()
        {
            String[] DATA = {
                "NFC", null, "NFD",
                "nfd", null, "NFC", // make sure case is ignored
                "Any-NFKD", null, "Any-NFKC",
                "Null", null, "Null",
                "-nfkc", "nfkc", "NFKD",
                "-nfkc/", "nfkc", "NFKD",
                "Latin-Greek/UNGEGN", null, "Greek-Latin/UNGEGN",
                "Greek/UNGEGN-Latin", "Greek-Latin/UNGEGN", "Latin-Greek/UNGEGN",
                "Bengali-Devanagari/", "Bengali-Devanagari", "Devanagari-Bengali",
                "Source-", null, null,
                "Source/Variant-", null, null,
                "Source-/Variant", null, null,
                "/Variant", null, null,
                "/Variant-", null, null,
                "-/Variant", null, null,
                "-/", null, null,
                "-", null, null,
                "/", null, null,
        };

            for (int i = 0; i < DATA.Length; i += 3)
            {
                String ID = DATA[i];
                String expID = DATA[i + 1];
                String expInvID = DATA[i + 2];
                bool expValid = (expInvID != null);
                if (expID == null)
                {
                    expID = ID;
                }
                try
                {
                    Transliterator t =
                        Transliterator.GetInstance(ID);
                    Transliterator u = t.GetInverse();
                    if (t.ID.Equals(expID) &&
                            u.ID.Equals(expInvID))
                    {
                        Logln("Ok: " + ID + ".GetInverse() => " + expInvID);
                    }
                    else
                    {
                        Errln("FAIL: getInstance(" + ID + ") => " +
                                t.ID + " x getInverse() => " + u.ID +
                                ", expected " + expInvID);
                    }
                }
                catch (ArgumentException e)
                {
                    if (!expValid)
                    {
                        Logln("Ok: getInstance(" + ID + ") => " + e.ToString());
                    }
                    else
                    {
                        Errln("FAIL: getInstance(" + ID + ") => " + e.ToString());
                    }
                }
            }
        }

        void checkRules(String label, Transliterator t2, String testRulesForward)
        {
            String rules2 = t2.ToRules(true);
            //rules2 = TestUtility.replaceAll(rules2, new UnicodeSet("[' '\n\r]"), "");
            rules2 = TestUtility.Replace(rules2, " ", "");
            rules2 = TestUtility.Replace(rules2, "\n", "");
            rules2 = TestUtility.Replace(rules2, "\r", "");
            testRulesForward = TestUtility.Replace(testRulesForward, " ", "");

            if (!rules2.Equals(testRulesForward))
            {
                Errln(label);
                Logln("GENERATED RULES: " + rules2);
                Logln("SHOULD BE:       " + testRulesForward);
            }
        }

        /**
         * Mark's toRules test.
         */
        [Test]
        public void TestToRulesMark()
        {

            String testRules =
                "::[[:Latin:][:Mark:]];"
                + "::NFKD (NFC);"
                + "::Lower (Lower);"
                + "a <> \\u03B1;" // alpha
                + "::NFKC (NFD);"
                + "::Upper (Lower);"
                + "::Lower ();"
                + "::([[:Greek:][:Mark:]]);"
                ;
            String testRulesForward =
                "::[[:Latin:][:Mark:]];"
                + "::NFKD(NFC);"
                + "::Lower(Lower);"
                + "a > \\u03B1;"
                + "::NFKC(NFD);"
                + "::Upper (Lower);"
                + "::Lower ();"
                ;
            String testRulesBackward =
                "::[[:Greek:][:Mark:]];"
                + "::Lower (Upper);"
                + "::NFD(NFKC);"
                + "\\u03B1 > a;"
                + "::Lower(Lower);"
                + "::NFC(NFKD);"
                ;
            String source = "\u00E1"; // a-acute
            String target = "\u03AC"; // alpha-acute

            Transliterator t2 = Transliterator.CreateFromRules("source-target", testRules, Transliterator.Forward);
            Transliterator t3 = Transliterator.CreateFromRules("target-source", testRules, Transliterator.Reverse);

            Expect(t2, source, target);
            Expect(t3, target, source);

            checkRules("Failed toRules FORWARD", t2, testRulesForward);
            checkRules("Failed toRules BACKWARD", t3, testRulesBackward);
        }

        /**
         * Test Escape and Unescape transliterators.
         */
        [Test]
        public void TestEscape()
        {
            Expect(Transliterator.GetInstance("Hex-Any"),
                    "\\x{40}\\U00000031&#x32;&#81;",
            "@12Q");
            Expect(Transliterator.GetInstance("Any-Hex/C"),
                    CharsToUnicodeString("A\\U0010BEEF\\uFEED"),
            "\\u0041\\U0010BEEF\\uFEED");
            Expect(Transliterator.GetInstance("Any-Hex/DotNet"),
                    CharsToUnicodeString("A\\U0010BEEF\\uFEED"),
            "\\u0041\\uDBEF\\uDEEF\\uFEED");
            Expect(Transliterator.GetInstance("Any-Hex/Perl"),
                    CharsToUnicodeString("A\\U0010BEEF\\uFEED"),
            "\\x{41}\\x{10BEEF}\\x{FEED}");
        }

        /**
         * Make sure display names of variants look reasonable.
         */
        [Test]
        public void TestDisplayName()
        {
            String[] DATA = {
                // ID, forward name, reverse name
                // Update the text as necessary -- the important thing is
                // not the text itself, but how various cases are handled.

                // Basic test
                "Any-Hex", "Any to Hex Escape", "Hex Escape to Any",

                // Variants
                "Any-Hex/Perl", "Any to Hex Escape/Perl", "Hex Escape to Any/Perl",

                // Target-only IDs
                "NFC", "Any to NFC", "Any to NFD",
            };

            CultureInfo US = new CultureInfo("en-US");

            for (int i = 0; i < DATA.Length; i += 3)
            {
                String name = Transliterator.GetDisplayName(DATA[i], US);
                if (!name.Equals(DATA[i + 1]))
                {
                    Errln("FAIL: " + DATA[i] + ".getDisplayName() => " +
                            name + ", expected " + DATA[i + 1]);
                }
                else
                {
                    Logln("Ok: " + DATA[i] + ".getDisplayName() => " + name);
                }
                Transliterator t = Transliterator.GetInstance(DATA[i], Transliterator.Reverse);
                name = Transliterator.GetDisplayName(t.ID, US);
                if (!name.Equals(DATA[i + 2]))
                {
                    Errln("FAIL: " + t.ID + ".getDisplayName() => " +
                            name + ", expected " + DATA[i + 2]);
                }
                else
                {
                    Logln("Ok: " + t.ID + ".getDisplayName() => " + name);
                }

                // Cover getDisplayName(String)
                ULocale save = ULocale.GetDefault();
                ULocale.SetDefault(ULocale.US);
                String name2 = Transliterator.GetDisplayName(t.ID);
                if (!name.Equals(name2))
                    Errln("FAIL: getDisplayName with default locale failed");
                ULocale.SetDefault(save);
            }
        }

        /**
         * Test anchor masking
         */
        [Test]
        public void TestAnchorMasking()
        {
            String rule = "^a > Q; a > q;";
            try
            {
                Transliterator t = Transliterator.CreateFromRules("ID", rule, Transliterator.Forward);
                if (t == null)
                {
                    Errln("FAIL: Did not get the expected exception");
                }
            }
            catch (ArgumentException e)
            {
                Errln("FAIL: " + rule + " => " + e);
            }
        }

        /**
         * This test is not in trnstst.cpp. This test has been moved from com/ibm/icu/dev/test/lang/TestUScript.java
         * during ICU4J modularization to remove dependency of tests on Transliterator.
         */
        [Test]
        public void TestScriptAllCodepoints()
        {
            int code;
            HashSet<string> scriptIdsChecked = new HashSet<string>();
            HashSet<string> scriptAbbrsChecked = new HashSet<string>();
            for (int i = 0; i <= 0x10ffff; i++)
            {
                code = UScript.GetScript(i);
                if (code == UScript.InvalidCode)
                {
                    Errln("UScript.getScript for codepoint 0x" + Hex(i) + " failed");
                }
                String id = UScript.GetName(code);
                String abbr = UScript.GetShortName(code);
                if (!scriptIdsChecked.Contains(id))
                {
                    scriptIdsChecked.Add(id);
                    String newId = "[:" + id + ":];NFD";
                    try
                    {
                        Transliterator t = Transliterator.GetInstance(newId);
                        if (t == null)
                        {
                            Errln("Failed to create transliterator for " + Hex(i) +
                                    " script code: " + id);
                        }
                    }
                    catch (Exception e)
                    {
                        Errln("Failed to create transliterator for " + Hex(i)
                                + " script code: " + id
                                + " Exception: " + e.ToString());
                    }
                }
                if (!scriptAbbrsChecked.Contains(abbr))
                {
                    scriptAbbrsChecked.Add(abbr);
                    String newAbbrId = "[:" + abbr + ":];NFD";
                    try
                    {
                        Transliterator t = Transliterator.GetInstance(newAbbrId);
                        if (t == null)
                        {
                            Errln("Failed to create transliterator for " + Hex(i) +
                                    " script code: " + abbr);
                        }
                    }
                    catch (Exception e)
                    {
                        Errln("Failed to create transliterator for " + Hex(i)
                                + " script code: " + abbr
                                + " Exception: " + e.ToString());
                    }
                }
            }
        }

        /**
         * This test is not in trnstst.cpp. This test has been moved from com/ibm/icu/dev/test/lang/TestUScript.java
         * during ICU4J modularization to remove dependency of tests on Transliterator.
         */
        [Test]
        public void TestScriptAllCodepointsUsingTry()
        {
            int code;
            HashSet<string> scriptIdsChecked = new HashSet<string>();
            HashSet<string> scriptAbbrsChecked = new HashSet<string>();
            for (int i = 0; i <= 0x10ffff; i++)
            {
                code = UScript.GetScript(i);
                if (code == UScript.InvalidCode)
                {
                    Errln("UScript.getScript for codepoint 0x" + Hex(i) + " failed");
                }

                if (!UScript.TryGetName(code, out string id))
                    Errln("UScript.TryGetName for codepoint 0x" + Hex(i) + " failed");

                if (!UScript.TryGetShortName(code, out string abbr))
                    Errln("UScript.TryGetShortName for codepoint 0x" + Hex(i) + " failed");

                if (!scriptIdsChecked.Contains(id))
                {
                    scriptIdsChecked.Add(id);
                    string newId = "[:" + id + ":];NFD";
                    try
                    {
                        Transliterator t = Transliterator.GetInstance(newId);
                        if (t == null)
                        {
                            Errln("Failed to create transliterator for " + Hex(i) +
                                    " script code: " + id);
                        }
                    }
                    catch (Exception e)
                    {
                        Errln("Failed to create transliterator for " + Hex(i)
                                + " script code: " + id
                                + " Exception: " + e.ToString());
                    }
                }
                if (!scriptAbbrsChecked.Contains(abbr))
                {
                    scriptAbbrsChecked.Add(abbr);
                    String newAbbrId = "[:" + abbr + ":];NFD";
                    try
                    {
                        Transliterator t = Transliterator.GetInstance(newAbbrId);
                        if (t == null)
                        {
                            Errln("Failed to create transliterator for " + Hex(i) +
                                    " script code: " + abbr);
                        }
                    }
                    catch (Exception e)
                    {
                        Errln("Failed to create transliterator for " + Hex(i)
                                + " script code: " + abbr
                                + " Exception: " + e.ToString());
                    }
                }
            }
        }


        static readonly string[][] registerRules = {
            new string[] {"Any-Dev1", "x > X; y > Y;"},
            new string[] {"Any-Dev2", "XY > Z"},
            new string[] {"Greek-Latin/FAKE",
                "[^[:L:][:M:]] { \u03bc\u03c0 > b ; "+
                "\u03bc\u03c0 } [^[:L:][:M:]] > b ; "+
                "[^[:L:][:M:]] { [\u039c\u03bc][\u03a0\u03c0] > B ; "+
                "[\u039c\u03bc][\u03a0\u03c0] } [^[:L:][:M:]] > B ;"
            },
        };

        internal static readonly String DESERET_DEE = UTF16.ValueOf(0x10414);
        internal static readonly String DESERET_dee = UTF16.ValueOf(0x1043C);

        internal static readonly String[][] testCases = {

        // NORMALIZATION
        // should add more test cases
        new string[] {"NFD" , "a\u0300 \u00E0 \u1100\u1161 \uFF76\uFF9E\u03D3"},
        new string[] {"NFC" , "a\u0300 \u00E0 \u1100\u1161 \uFF76\uFF9E\u03D3"},
        new string[] {"NFKD", "a\u0300 \u00E0 \u1100\u1161 \uFF76\uFF9E\u03D3"},
        new string[] {"NFKC", "a\u0300 \u00E0 \u1100\u1161 \uFF76\uFF9E\u03D3"},

        // mp -> b BUG
        new string[] {"Greek-Latin/UNGEGN", "(\u03BC\u03C0)", "(b)"},
        new string[] {"Greek-Latin/FAKE", "(\u03BC\u03C0)", "(b)"},

        // check for devanagari bug
        new string[] {"nfd;Dev1;Dev2;nfc", "xy", "Z"},

        // ff, i, dotless-i, I, dotted-I, LJLjlj deseret deeDEE
        new string[] {"Title", "ab'cD ffi\u0131I\u0130 \u01C7\u01C8\u01C9 " + DESERET_dee + DESERET_DEE,
            "Ab'cd Ffi\u0131ii\u0307 \u01C8\u01C9\u01C9 " + DESERET_DEE + DESERET_dee},
            //TODO: enable this test once Titlecase works right
            //{"Title", "\uFB00i\u0131I\u0130 \u01C7\u01C8\u01C9 " + DESERET_dee + DESERET_DEE,
            //          "Ffi\u0131ii \u01C8\u01C9\u01C9 " + DESERET_DEE + DESERET_dee},

            new string[] {"Upper", "ab'cD \uFB00i\u0131I\u0130 \u01C7\u01C8\u01C9 " + DESERET_dee + DESERET_DEE,
                "AB'CD FFIII\u0130 \u01C7\u01C7\u01C7 " + DESERET_DEE + DESERET_DEE},
                new string[] {"Lower", "ab'cD \uFB00i\u0131I\u0130 \u01C7\u01C8\u01C9 " + DESERET_dee + DESERET_DEE,
                    "ab'cd \uFB00i\u0131ii\u0307 \u01C9\u01C9\u01C9 " + DESERET_dee + DESERET_dee},

                    new string[] {"Upper", "ab'cD \uFB00i\u0131I\u0130 \u01C7\u01C8\u01C9 " + DESERET_dee + DESERET_DEE},
                    new string[] {"Lower", "ab'cD \uFB00i\u0131I\u0130 \u01C7\u01C8\u01C9 " + DESERET_dee + DESERET_DEE},

                    // FORMS OF S
                    new string[] {"Greek-Latin/UNGEGN", "\u03C3 \u03C3\u03C2 \u03C2\u03C3", "s ss s\u0331s\u0331"},
                    new string[] {"Latin-Greek/UNGEGN", "s ss s\u0331s\u0331", "\u03C3 \u03C3\u03C2 \u03C2\u03C3"},
                    new string[] {"Greek-Latin", "\u03C3 \u03C3\u03C2 \u03C2\u03C3", "s ss s\u0331s\u0331"},
                    new string[] {"Latin-Greek", "s ss s\u0331s\u0331", "\u03C3 \u03C3\u03C2 \u03C2\u03C3"},

                    // Tatiana bug
                    // Upper: TAT\u02B9\u00C2NA
                    // Lower: tat\u02B9\u00E2na
                    // Title: Tat\u02B9\u00E2na
                    new string[] {"Upper", "tat\u02B9\u00E2na", "TAT\u02B9\u00C2NA"},
                    new string[] {"Lower", "TAT\u02B9\u00C2NA", "tat\u02B9\u00E2na"},
                    new string[] {"Title", "tat\u02B9\u00E2na", "Tat\u02B9\u00E2na"},
        };

        [Test]
        public void TestSpecialCases()
        {

            for (int i = 0; i < registerRules.Length; ++i)
            {
                Transliterator t = Transliterator.CreateFromRules(registerRules[i][0],
                        registerRules[i][1], Transliterator.Forward);
                DummyFactory.Add(registerRules[i][0], t);
            }
            for (int i = 0; i < testCases.Length; ++i)
            {
                String name = testCases[i][0];
                Transliterator t = Transliterator.GetInstance(name);
                String id = t.ID;
                String source = testCases[i][1];
                String target = null;

                // Automatic generation of targets, to make it simpler to add test cases (and more fail-safe)

                if (testCases[i].Length > 2) target = testCases[i][2];
                else if (id.Equals("NFD", StringComparison.OrdinalIgnoreCase)) target = Normalizer.Normalize(source, NormalizerMode.NFD);
                else if (id.Equals("NFC", StringComparison.OrdinalIgnoreCase)) target = Normalizer.Normalize(source, NormalizerMode.NFC);
                else if (id.Equals("NFKD", StringComparison.OrdinalIgnoreCase)) target = Normalizer.Normalize(source, NormalizerMode.NFKD);
                else if (id.Equals("NFKC", StringComparison.OrdinalIgnoreCase)) target = Normalizer.Normalize(source, NormalizerMode.NFKC);
                else if (id.Equals("Lower", StringComparison.OrdinalIgnoreCase)) target = UChar.ToLower(new CultureInfo("en-US"), source);
                else if (id.Equals("Upper", StringComparison.OrdinalIgnoreCase)) target = UChar.ToUpper(new CultureInfo("en-US"), source);

                Expect(t, source, target);
            }
            for (int i = 0; i < registerRules.Length; ++i)
            {
                Transliterator.Unregister(registerRules[i][0]);
            }
        }

        // seems like there should be an easier way to just register an instance of a transliterator

        internal class DummyFactory : ITransliteratorFactory
        {
            static DummyFactory singleton = new DummyFactory();
            static IDictionary<string, Transliterator> m = new Dictionary<string, Transliterator>();

            // Since Transliterators are immutable, we don't have to clone on set & get
            internal static void Add(String ID, Transliterator t)
            {
                m[ID] = t;
                //System.out.println("Registering: " + ID + ", " + t.ToRules(true));
                Transliterator.RegisterFactory(ID, singleton);
            }

            public virtual Transliterator GetInstance(String ID)
            {
                return (Transliterator)m.Get(ID);
            }
        }

        [Test]
        public void TestCasing()
        {
            Transliterator toLower = Transliterator.GetInstance("lower");
            Transliterator toCasefold = Transliterator.GetInstance("casefold");
            Transliterator toUpper = Transliterator.GetInstance("upper");
            Transliterator toTitle = Transliterator.GetInstance("title");
            for (int i = 0; i < 0x600; ++i)
            {
                String s = UTF16.ValueOf(i);

                String lower = UChar.ToLower(ULocale.ROOT, s);
                assertEquals("Lowercase", lower, toLower.Transform(s));

                String casefold = UChar.FoldCase(s, true);
                assertEquals("Casefold", casefold, toCasefold.Transform(s));

                if (i != 0x0345)
                {
                    // ICU 60 changes the default titlecasing index adjustment.
                    // For word breaks it is mostly the same as before,
                    // but it is different for the iota subscript (the only cased combining mark).
                    // This should be ok because the iota subscript is not supposed to appear
                    // at the start of a word.
                    // The title Transliterator is far below feature parity with the
                    // UCharacter and CaseMap titlecasing functions.
                    String title = UChar.ToTitleCase(ULocale.ROOT, s, null);
                    assertEquals("Title", title, toTitle.Transform(s));
                }

                String upper = UChar.ToUpper(ULocale.ROOT, s);
                assertEquals("Upper", upper, toUpper.Transform(s));
            }
        }

        [Test]
        public void TestSurrogateCasing()
        {
            // check that casing handles surrogates
            // titlecase is currently defective
            int dee = UTF16.CharAt(DESERET_dee, 0);
            int DEE = UChar.ToTitleCase(dee);
            if (!UTF16.ValueOf(DEE).Equals(DESERET_DEE))
            {
                Errln("Fails titlecase of surrogates" + Convert.ToString(dee, 16) + ", " + Convert.ToString(DEE, 16));
            }

            if (!UChar.ToUpper(DESERET_dee + DESERET_DEE).Equals(DESERET_DEE + DESERET_DEE))
            {
                Errln("Fails uppercase of surrogates");
            }

            if (!UChar.ToLower(DESERET_dee + DESERET_DEE).Equals(DESERET_dee + DESERET_dee))
            {
                Errln("Fails lowercase of surrogates");
            }
        }


        [Test]
        public void TestFunction()
        {
            // Careful with spacing and ';' here:  Phrase this exactly
            // as toRules() is going to return it.  If toRules() changes
            // with regard to spacing or ';', then adjust this string.
            String rule =
                "([:Lu:]) > $1 '(' &Lower( $1 ) '=' &Hex( &Any-Lower( $1 ) ) ')';";

            Transliterator t = Transliterator.CreateFromRules("Test", rule, Transliterator.Forward);
            if (t == null)
            {
                Errln("FAIL: createFromRules failed");
                return;
            }

            String r = t.ToRules(true);
            if (r.Equals(rule))
            {
                Logln("OK: toRules() => " + r);
            }
            else
            {
                Errln("FAIL: toRules() => " + r +
                        ", expected " + rule);
            }

            Expect(t, "The Quick Brown Fox",
            "T(t=\\u0074)he Q(q=\\u0071)uick B(b=\\u0062)rown F(f=\\u0066)ox");
            rule =
                "([^\\ -\\u007F]) > &Hex/Unicode( $1 ) ' ' &Name( $1 ) ;";

            t = Transliterator.CreateFromRules("Test", rule, Transliterator.Forward);
            if (t == null)
            {
                Errln("FAIL: createFromRules failed");
                return;
            }

            r = t.ToRules(true);
            if (r.Equals(rule))
            {
                Logln("OK: toRules() => " + r);
            }
            else
            {
                Errln("FAIL: toRules() => " + r +
                        ", expected " + rule);
            }

            Expect(t, "\u0301",
            "U+0301 \\N{COMBINING ACUTE ACCENT}");
        }

        [Test]
        public void TestInvalidBackRef()
        {
            String rule = ". > $1;";
            String rule2 = "(.) <> &hex/unicode($1) &name($1); . > $1; [{}] >\u0020;";
            try
            {
                Transliterator t = Transliterator.CreateFromRules("Test", rule, Transliterator.Forward);
                if (t != null)
                {
                    Errln("FAIL: createFromRules should have returned NULL");
                }
                Errln("FAIL: Ok: . > $1; => no error");
                Transliterator t2 = Transliterator.CreateFromRules("Test2", rule2, Transliterator.Forward);
                if (t2 != null)
                {
                    Errln("FAIL: createFromRules should have returned NULL");
                }
                Errln("FAIL: Ok: . > $1; => no error");
            }
            catch (ArgumentException e)
            {
                Logln("Ok: . > $1; => " + e.ToString());
            }
        }

        [Test]
        public void TestMulticharStringSet()
        {
            // Basic testing
            String rule =
                "       [{aa}]       > x;" +
                "         a          > y;" +
                "       [b{bc}]      > z;" +
                "[{gd}] { e          > q;" +
                "         e } [{fg}] > r;";

            Transliterator t = Transliterator.CreateFromRules("Test", rule, Transliterator.Forward);
            if (t == null)
            {
                Errln("FAIL: createFromRules failed");
                return;
            }

            Expect(t, "a aa ab bc d gd de gde gdefg ddefg",
            "y x yz z d gd de gdq gdqfg ddrfg");

            // Overlapped string test.  Make sure that when multiple
            // strings can match that the longest one is matched.
            rule =
                "    [a {ab} {abc}]    > x;" +
                "           b          > y;" +
                "           c          > z;" +
                " q [t {st} {rst}] { e > p;";

            t = Transliterator.CreateFromRules("Test", rule, Transliterator.Forward);
            if (t == null)
            {
                Errln("FAIL: createFromRules failed");
                return;
            }

            Expect(t, "a ab abc qte qste qrste",
            "x x x qtp qstp qrstp");
        }

        /**
         * Test that user-registered transliterators can be used under function
         * syntax.
         */
        [Test]
        public void TestUserFunction()
        {
            Transliterator t;

            // There's no need to register inverses if we don't use them
            TestUserFunctionFactory.Add("Any-gif",
                    Transliterator.CreateFromRules("gif",
                            "'\\'u(..)(..) > '<img src=\"http://www.unicode.org/gifs/24/' $1 '/U' $1$2 '.gif\">';",
                            Transliterator.Forward));
            //TestUserFunctionFactory.add("gif-Any", Transliterator.GetInstance("Any-Null"));

            TestUserFunctionFactory.Add("Any-RemoveCurly",
                    Transliterator.CreateFromRules("RemoveCurly", "[\\{\\}] > ; \\\\N > ;", Transliterator.Forward));
            //TestUserFunctionFactory.add("RemoveCurly-Any", Transliterator.GetInstance("Any-Null"));

            Logln("Trying &hex");
            t = Transliterator.CreateFromRules("hex2", "(.) > &hex($1);", Transliterator.Forward);
            Logln("Registering");
            TestUserFunctionFactory.Add("Any-hex2", t);
            t = Transliterator.GetInstance("Any-hex2");
            Expect(t, "abc", "\\u0061\\u0062\\u0063");

            Logln("Trying &gif");
            t = Transliterator.CreateFromRules("gif2", "(.) > &Gif(&Hex2($1));", Transliterator.Forward);
            Logln("Registering");
            TestUserFunctionFactory.Add("Any-gif2", t);
            t = Transliterator.GetInstance("Any-gif2");
            Expect(t, "ab", "<img src=\"http://www.unicode.org/gifs/24/00/U0061.gif\">" +
            "<img src=\"http://www.unicode.org/gifs/24/00/U0062.gif\">");

            // Test that filters are allowed after &
            t = Transliterator.CreateFromRules("test",
                    "(.) > &Hex($1) ' ' &Any-RemoveCurly(&Name($1)) ' ';", Transliterator.Forward);
            Expect(t, "abc", "\\u0061 LATIN SMALL LETTER A \\u0062 LATIN SMALL LETTER B \\u0063 LATIN SMALL LETTER C ");

            // Unregister our test stuff
            TestUserFunctionFactory.Unregister();
        }

        internal class TestUserFunctionFactory : ITransliteratorFactory
        {
            static TestUserFunctionFactory singleton = new TestUserFunctionFactory();
            static IDictionary<CaseInsensitiveString, Transliterator> m = new Dictionary<CaseInsensitiveString, Transliterator>();

            internal static void Add(String ID, Transliterator t)
            {
                m[new CaseInsensitiveString(ID)] = t;
                Transliterator.RegisterFactory(ID, singleton);
            }

            public virtual Transliterator GetInstance(String ID)
            {
                return (Transliterator)m.Get(new CaseInsensitiveString(ID));
            }

            internal static void Unregister()
            {
                var toRemove = new List<CaseInsensitiveString>();
                using (var ids = m.Keys.GetEnumerator())
                    while (ids.MoveNext())
                    {
                        CaseInsensitiveString id = (CaseInsensitiveString)ids.Current;
                        Transliterator.Unregister(id.String);
                        //ids.Remove(); // removes pair from m
                        toRemove.Add(id);
                    }

                foreach (var item in toRemove)
                    m.Remove(item);
            }
        }

        /**
         * Test the Any-X transliterators.
         */
        [Test]
        public void TestAnyX()
        {
            Transliterator anyLatin =
                Transliterator.GetInstance("Any-Latin", Transliterator.Forward);

            Expect(anyLatin,
                    "greek:\u03B1\u03B2\u03BA\u0391\u0392\u039A hiragana:\u3042\u3076\u304F cyrillic:\u0430\u0431\u0446",
            "greek:abkABK hiragana:abuku cyrillic:abc");
        }

        /**
         * Test Any-X transliterators with sample letters from all scripts.
         */
        [Test]
        public void TestAny()
        {
            UnicodeSet alphabetic = new UnicodeSet("[:alphabetic:]").Freeze();
            StringBuffer testString = new StringBuffer();
            for (int i = 0; i < UScript.CodeLimit; ++i)
            {
                UnicodeSet sample = new UnicodeSet().ApplyPropertyAlias("script", UScript.GetShortName(i)).RetainAll(alphabetic);
                int count = 5;
                for (UnicodeSetIterator it = new UnicodeSetIterator(sample); it.Next();)
                {
                    testString.Append(it.GetString());
                    if (--count < 0) break;
                }
            }
            Logln("Sample set for Any-Latin: " + testString);
            Transliterator anyLatin = Transliterator.GetInstance("any-Latn");
            String result = anyLatin.Transliterate(testString.ToString());
            Logln("Sample result for Any-Latin: " + result);
        }


        /**
         * Test the source and target set API.  These are only implemented
         * for RBT and CompoundTransliterator at this time.
         */
        [Test]
        public void TestSourceTargetSet()
        {
            // Rules
            String r =
                "a > b; " +
                "r [x{lu}] > q;";

            // Expected source
            UnicodeSet expSrc = new UnicodeSet("[arx{lu}]");

            // Expected target
            UnicodeSet expTrg = new UnicodeSet("[bq]");

            Transliterator t = Transliterator.CreateFromRules("test", r, Transliterator.Forward);
            UnicodeSet src = t.GetSourceSet();
            UnicodeSet trg = t.GetTargetSet();

            if (src.Equals(expSrc) && trg.Equals(expTrg))
            {
                Logln("Ok: " + r + " => source = " + src.ToPattern(true) +
                        ", target = " + trg.ToPattern(true));
            }
            else
            {
                Errln("FAIL: " + r + " => source = " + src.ToPattern(true) +
                        ", expected " + expSrc.ToPattern(true) +
                        "; target = " + trg.ToPattern(true) +
                        ", expected " + expTrg.ToPattern(true));
            }
        }

        [Test]
        public void TestSourceTargetSet2()
        {


            Normalizer2 nfc = Normalizer2.GetNFCInstance();
            Normalizer2 nfd = Normalizer2.GetNFDInstance();

            //        Normalizer2 nfkd = Normalizer2.GetInstance(null, "nfkd", Mode.DECOMPOSE);
            //        UnicodeSet nfkdSource = new UnicodeSet();
            //        UnicodeSet nfkdTarget = new UnicodeSet();
            //        for (int i = 0; i <= 0x10FFFF; ++i) {
            //            if (nfkd.isInert(i)) {
            //                continue;
            //            }
            //            nfkdSource.add(i);
            //            String t = nfkd.getDecomposition(i);
            //            if (t != null) {
            //                nfkdTarget.addAll(t);
            //            } else {
            //                nfkdTarget.add(i);
            //            }
            //        }
            //        nfkdSource.freeze();
            //        nfkdTarget.freeze();
            //        Logln("NFKD Source: " + nfkdSource.ToPattern(false));
            //        Logln("NFKD Target: " + nfkdTarget.ToPattern(false));

            UnicodeMap<UnicodeSet> leadToTrail = new UnicodeMap<UnicodeSet>();
            UnicodeMap<UnicodeSet> leadToSources = new UnicodeMap<UnicodeSet>();
            UnicodeSet nonStarters = new UnicodeSet("[:^ccc=0:]").Freeze();
            CanonicalEnumerator can = new CanonicalEnumerator("");

            UnicodeSet disorderedMarks = new UnicodeSet();

            for (int i = 0; i <= 0x10FFFF; ++i)
            {
                String s = nfd.GetDecomposition(i);
                if (s == null)
                {
                    continue;
                }

                can.SetSource(s);
                while (can.MoveNext())
                    disorderedMarks.Add(can.Current);
                //for (String t = can.Next(); t != null; t = can.Next())
                //{
                //    disorderedMarks.Add(t);
                //}

                // if s has two code points, (or more), add the lead/trail information
                int first = s.CodePointAt(0);
                int firstCount = Character.CharCount(first);
                if (s.Length == firstCount) continue;
                String trailString = s.Substring(firstCount);

                // add all the trail characters
                if (!nonStarters.ContainsSome(trailString))
                {
                    continue;
                }
                UnicodeSet trailSet = leadToTrail.Get(first);
                if (trailSet == null)
                {
                    leadToTrail.Put(first, trailSet = new UnicodeSet());
                }
                trailSet.AddAll(trailString); // add remaining trails

                // add the sources
                UnicodeSet sourcesSet = leadToSources.Get(first);
                if (sourcesSet == null)
                {
                    leadToSources.Put(first, sourcesSet = new UnicodeSet());
                }
                sourcesSet.Add(i);
            }


            foreach (var x in leadToSources.EntrySet())
            {
                String lead = x.Key;
                UnicodeSet sources = x.Value;
                UnicodeSet trailSet = leadToTrail.Get(lead);
                foreach (String source in sources)
                {
                    foreach (String trail in trailSet)
                    {
                        can.SetSource(source + trail);
                        //for (String t = can.Next(); t != null; t = can.Next())
                        while (can.MoveNext())
                        {
                            string t = can.Current;
                            if (t.EndsWith(trail, StringComparison.Ordinal)) continue;
                            disorderedMarks.Add(t);
                        }
                    }
                }
            }


            foreach (String s in nonStarters)
            {
                disorderedMarks.Add("\u0345" + s);
                disorderedMarks.Add(s + "\u0323");
                String xx = nfc.Normalize("\u01EC" + s);
                if (!xx.StartsWith("\u01EC", StringComparison.Ordinal))
                {
                    Logln("??");
                }
            }

            //        for (int i = 0; i <= 0x10FFFF; ++i) {
            //            String s = nfkd.getDecomposition(i);
            //            if (s != null) {
            //                disorderedMarks.add(s);
            //                disorderedMarks.add(nfc.normalize(s));
            //                addDerivedStrings(nfc, disorderedMarks, s);
            //            }
            //            s = nfd.getDecomposition(i);
            //            if (s != null) {
            //                disorderedMarks.add(s);
            //            }
            //            if (!nfc.isInert(i)) {
            //                if (i == 0x00C0) {
            //                    Logln("\u00C0");
            //                }
            //                can.setSource(s+"\u0334");
            //                for (String t = can.next(); t != null; t = can.next()) {
            //                    addDerivedStrings(nfc, disorderedMarks, t);
            //                }
            //                can.setSource(s+"\u0345");
            //                for (String t = can.next(); t != null; t = can.next()) {
            //                    addDerivedStrings(nfc, disorderedMarks, t);
            //                }
            //                can.setSource(s+"\u0323");
            //                for (String t = can.next(); t != null; t = can.next()) {
            //                    addDerivedStrings(nfc, disorderedMarks, t);
            //                }
            //            }
            //        }
            Logln("Test cases: " + disorderedMarks.Count);
            disorderedMarks.AddAll(0, 0x10FFFF).Freeze();
            Logln("isInert \u0104 " + nfc.IsInert('\u0104'));

            Object[][] rules = {
                new object[] {":: [:sc=COMMON:] any-name;", null},

                new object[] {":: [:Greek:] hex-any/C;", null},
                new object[] {":: [:Greek:] any-hex/C;", null},

                new object[] {":: [[:Mn:][:Me:]] remove;", null},
                new object[] {":: [[:Mn:][:Me:]] null;", null},


                new object[] {":: lower;", null},
                new object[] {":: upper;", null},
                new object[] {":: title;", null},
                new object[] {":: CaseFold;", null},

                new object[] {":: NFD;", null},
                new object[] {":: NFC;", null},
                new object[] {":: NFKD;", null},
                new object[] {":: NFKC;", null},

                new object[] {":: [[:Mn:][:Me:]] NFKD;", null},
                new object[] {":: Latin-Greek;", null},
                new object[] {":: [:Latin:] NFKD;", null},
                new object[] {":: NFKD;", null},
                new object[] {":: NFKD;\n" +
                    ":: [[:Mn:][:Me:]] remove;\n" +
                    ":: NFC;", null},
        };
            foreach (Object[] rulex in rules)
            {
                String rule = (String)rulex[0];
                Transliterator trans = Transliterator.CreateFromRules("temp", rule, Transliterator.Forward);
                UnicodeSet actualSource = trans.GetSourceSet();
                UnicodeSet actualTarget = trans.GetTargetSet();
                UnicodeSet empiricalSource = new UnicodeSet();
                UnicodeSet empiricalTarget = new UnicodeSet();
                String ruleDisplay = rule.Replace("\n", "\t\t");
                UnicodeSet toTest = disorderedMarks;
                //            if (rulex[1] != null) {
                //                toTest = new UnicodeSet(disorderedMarks);
                //                toTest.addAll((UnicodeSet) rulex[1]);
                //            }

                String test = nfd.Normalize("\u0104");
                bool DEBUG = true;
                int count = 0; // for debugging
                foreach (String s in toTest)
                {
                    if (s.Equals(test))
                    {
                        Logln(test);
                    }
                    String t = trans.Transform(s);
                    if (!s.Equals(t))
                    {
                        if (!isAtomic(s, t, trans))
                        {
                            isAtomic(s, t, trans);
                            continue;
                        }

                        // only keep the part that changed; so skip the front and end.
                        //                    int start = findSharedStartLength(s,t);
                        //                    int end = findSharedEndLength(s,t);
                        //                    if (start != 0 || end != 0) {
                        //                        s = s.substring(start, s.Length - end);
                        //                        t = t.substring(start, t.Length - end);
                        //                    }
                        if (DEBUG)
                        {
                            if (!actualSource.ContainsAll(s))
                            {
                                count++;
                            }
                            if (!actualTarget.ContainsAll(t))
                            {
                                count++;
                            }
                        }
                        AddSourceTarget(s, empiricalSource, t, empiricalTarget);
                    }
                }
                if (rule.Contains("title"))
                {
                    // See the comment in TestCasing() about the iota subscript.
                    empiricalSource.Remove(0x345);
                }
                assertEquals("getSource(" + ruleDisplay + ")", empiricalSource, actualSource, SetAssert.MISSING_OK);
                assertEquals("getTarget(" + ruleDisplay + ")", empiricalTarget, actualTarget, SetAssert.MISSING_OK);
            }
        }

        [Test]
        public void TestSourceTargetSetFilter()
        {
            String[][] tests = {
                // rules, expectedTarget-FORWARD, expectedTarget-REVERSE
                new string[] {"[] Latin-Greek", null, "[\']"},
                new string[] {"::[] ; ::NFD ; ::NFKC ; :: ([]) ;"},
                new string[] {"[] Any-Latin"},
                new string[] {"[] casefold"},
                new string[] {"[] NFKD;"},
                new string[] {"[] NFKC;"},
                new string[] {"[] hex"},
                new string[] {"[] lower"},
                new string[] {"[] null"},
                new string[] {"[] remove"},
                new string[] {"[] title"},
                new string[] {"[] upper"},
        };
            UnicodeSet expectedSource = UnicodeSet.Empty;
            foreach (String[] testPair in tests)
            {
                String test = testPair[0];
                Transliterator t0;
                try
                {
                    t0 = Transliterator.GetInstance(test);
                }
                catch (Exception e)
                {
                    t0 = Transliterator.CreateFromRules("temp", test, Transliterator.Forward);
                }
                Transliterator t1;
                try
                {
                    t1 = t0.GetInverse();
                }
                catch (Exception e)
                {
                    t1 = Transliterator.CreateFromRules("temp", test, Transliterator.Reverse);
                }
                int targetIndex = 0;
                foreach (Transliterator t in new Transliterator[] { t0, t1 })
                {
                    bool ok;
                    UnicodeSet source = t.GetSourceSet();
                    String direction = t == t0 ? "FORWARD\t" : "REVERSE\t";
                    targetIndex++;
                    UnicodeSet expectedTarget = testPair.Length <= targetIndex ? expectedSource
                            : testPair[targetIndex] == null ? expectedSource
                                    : testPair[targetIndex].Length == 0 ? expectedSource
                                            : new UnicodeSet(testPair[targetIndex]);
                    ok = assertEquals(direction + "getSource\t\"" + test + '"', expectedSource, source);
                    if (!ok)
                    { // for debugging
                        source = t.GetSourceSet();
                    }
                    UnicodeSet target = t.GetTargetSet();
                    ok = assertEquals(direction + "getTarget\t\"" + test + '"', expectedTarget, target);
                    if (!ok)
                    { // for debugging
                        target = t.GetTargetSet();
                    }
                }
            }
        }

        private bool isAtomic(String s, String t, Transliterator trans)
        {
            for (int i = 1; i < s.Length; ++i)
            {
                if (!CharSequences.OnCharacterBoundary(s, i))
                {
                    continue;
                }
                String q = trans.Transform(s.Substring(0, i)); // ICU4N: Checked 2nd parameter
                if (t.StartsWith(q, StringComparison.Ordinal))
                {
                    String r = trans.Transform(s.Substring(i));
                    if (t.Length == q.Length + r.Length && t.EndsWith(r, StringComparison.Ordinal))
                    {
                        return false;
                    }
                }
            }
            return true;
            //        // make sure that every part is different
            //        if (s.codePointCount(0, s.Length) > 1) {
            //            int[] codePoints = It.codePoints(s);
            //            for (int k = 0; k < codePoints.Length; ++k) {
            //                int pos = indexOf(t,codePoints[k]);
            //                if (pos >= 0) {
            //                    int x;
            //                }
            //            }
            //            if (s.contains("\u00C0")) {
            //                Logln("\u00C0");
            //            }
            //        }
        }

        private void AddSourceTarget(String s, UnicodeSet expectedSource, String t, UnicodeSet expectedTarget)
        {
            expectedSource.AddAll(s);
            if (t.Length > 0)
            {
                expectedTarget.AddAll(t);
            }
        }

        //    private void addDerivedStrings(Normalizer2 nfc, UnicodeSet disorderedMarks, String s) {
        //        disorderedMarks.add(s);
        //        for (int j = 1; j < s.Length; ++j) {
        //            if (CharSequences.onCharacterBoundary(s, j)) {
        //                String shorter = s.substring(0,j);
        //                disorderedMarks.add(shorter);
        //                disorderedMarks.add(nfc.normalize(shorter) + s.substring(j));
        //            }
        //        }
        //    }

        [Test]
        public void TestCharUtils()
        {
            String[][] startTests = {
                new string[] {"1", "a", "ab"},
                new string[] {"0", "a", "xb"},
                new string[] {"0", "\uD800", "\uD800\uDC01"},
                new string[] {"1", "\uD800a", "\uD800b"},
                new string[] {"0", "\uD800\uDC00", "\uD800\uDC01"},
        };
            foreach (String[] row in startTests)
            {
                int actual = findSharedStartLength(row[1], row[2]);
                assertEquals("findSharedStartLength(" + row[1] + "," + row[2] + ")",
                        int.Parse(row[0], CultureInfo.InvariantCulture),
                        actual);
            }
            String[][] endTests = {
                new string[] {"0", "\uDC00", "\uD801\uDC00"},
                new string[] {"1", "a", "ba"},
                new string[] {"0", "a", "bx"},
                new string[] {"1", "a\uDC00", "b\uDC00"},
                new string[] {"0", "\uD800\uDC00", "\uD801\uDC00"},
        };
            foreach (String[] row in endTests)
            {
                int actual = findSharedEndLength(row[1], row[2]);
                assertEquals("findSharedEndLength(" + row[1] + "," + row[2] + ")",
                        int.Parse(row[0], CultureInfo.InvariantCulture),
                        actual);
            }
        }

        /**
         * @param s
         * @param t
         * @return
         */
        // TODO make generally available
        private static int findSharedStartLength(string s, string t) // ICU4N specfific - changed s and t from ICharSequence to string
        {
            int min = Math.Min(s.Length, t.Length);
            int i;
            char sch, tch;
            for (i = 0; i < min; ++i)
            {
                sch = s[i];
                tch = t[i];
                if (sch != tch)
                {
                    break;
                }
            }
            return CharSequences.OnCharacterBoundary(s, i) && CharSequences.OnCharacterBoundary(t, i) ? i : i - 1;
        }

        /**
         * @param s
         * @param t
         * @return
         */
        // TODO make generally available
        private static int findSharedEndLength(string s, string t) // ICU4N specific - changed s and t from ICharSequence to string
        {
            int slength = s.Length;
            int tlength = t.Length;
            int min = Math.Min(slength, tlength);
            int i;
            char sch, tch;
            // TODO can make the calculations slightly faster... Not sure if it is worth the complication, tho'
            for (i = 0; i < min; ++i)
            {
                sch = s[slength - i - 1];
                tch = t[tlength - i - 1];
                if (sch != tch)
                {
                    break;
                }
            }
            return CharSequences.OnCharacterBoundary(s, slength - i) && CharSequences.OnCharacterBoundary(t, tlength - i) ? i : i - 1;
        }

        enum SetAssert { EQUALS, MISSING_OK, EXTRA_OK }

        void assertEquals(String message, UnicodeSet empirical, UnicodeSet actual, SetAssert setAssert)
        {
            bool haveError = false;
            if (!actual.ContainsAll(empirical))
            {
                UnicodeSet missing = new UnicodeSet(empirical).RemoveAll(actual);
                Errln(message + " \tgetXSet < empirical (" + missing.Count + "): " + toPattern(missing));
                haveError = true;
            }
            if (!empirical.ContainsAll(actual))
            {
                UnicodeSet extra = new UnicodeSet(actual).RemoveAll(empirical);
                Logln("WARNING: " + message + " \tgetXSet > empirical (" + extra.Count + "): " + toPattern(extra));
                haveError = true;
            }
            if (!haveError)
            {
                Logln("OK " + message + ' ' + toPattern(empirical));
            }
        }

        private String toPattern(UnicodeSet missing)
        {
            String result = missing.ToPattern(false);
            if (result.Length < 200)
            {
                return result;
            }
            return result.Substring(0, CharSequences.OnCharacterBoundary(result, 200) ? 200 : 199) + "\u2026"; // ICU4N: Checked 2nd substring parameter
        }


        /**
         * Test handling of Pattern_White_Space, for both RBT and UnicodeSet.
         */
        [Test]
        public void TestPatternWhitespace()
        {
            // Rules
            String r = "a > \u200E b;";

            Transliterator t = Transliterator.CreateFromRules("test", r, Transliterator.Forward);

            Expect(t, "a", "b");

            // UnicodeSet
            UnicodeSet set = new UnicodeSet("[a \u200E]");

            if (set.Contains(0x200E))
            {
                Errln("FAIL: U+200E not being ignored by UnicodeSet");
            }
        }

        [Test]
        public void TestAlternateSyntax()
        {
            // U+2206 == &
            // U+2190 == <
            // U+2192 == >
            // U+2194 == <>
            Expect("a \u2192 x; b \u2190 y; c \u2194 z",
                    "abc",
            "xbz");
            Expect("([:^ASCII:]) \u2192 \u2206Name($1);",
                    "<=\u2190; >=\u2192; <>=\u2194; &=\u2206",
            "<=\\N{LEFTWARDS ARROW}; >=\\N{RIGHTWARDS ARROW}; <>=\\N{LEFT RIGHT ARROW}; &=\\N{INCREMENT}");
        }

        [Test]
        public void TestPositionAPI()
        {
            TransliterationPosition a = new TransliterationPosition(3, 5, 7, 11);
            TransliterationPosition b = new TransliterationPosition(a);
            TransliterationPosition c = new TransliterationPosition();
            c.Set(a);
            // Call the toString() API:
            if (a.Equals(b) && a.Equals(c))
            {
                Logln("Ok: " + a + " == " + b + " == " + c);
            }
            else
            {
                Errln("FAIL: " + a + " != " + b + " != " + c);
            }
        }

        //======================================================================
        // New tests for the ::BEGIN/::END syntax
        //======================================================================

        private static readonly String[] BEGIN_END_RULES = new String[] {
        // [0]
        "abc > xy;"
        + "aba > z;",

        // [1]
        /*
        "::BEGIN;"
        + "abc > xy;"
        + "::END;"
        + "::BEGIN;"
        + "aba > z;"
        + "::END;",
         */
        "", // test case commented out below, this is here to keep from messing up the indexes

        // [2]
        /*
        "abc > xy;"
        + "::BEGIN;"
        + "aba > z;"
        + "::END;",
         */
        "", // test case commented out below, this is here to keep from messing up the indexes

        // [3]
        /*
        "::BEGIN;"
        + "abc > xy;"
        + "::END;"
        + "aba > z;",
         */
        "", // test case commented out below, this is here to keep from messing up the indexes

        // [4]
        "abc > xy;"
        + "::Null;"
        + "aba > z;",

        // [5]
        "::Upper;"
        + "ABC > xy;"
        + "AB > x;"
        + "C > z;"
        + "::Upper;"
        + "XYZ > p;"
        + "XY > q;"
        + "Z > r;"
        + "::Upper;",

        // [6]
        "$ws = [[:Separator:][\\u0009-\\u000C]$];"
        + "$delim = [\\-$ws];"
        + "$ws $delim* > ' ';"
        + "'-' $delim* > '-';",

        // [7]
        "::Null;"
        + "$ws = [[:Separator:][\\u0009-\\u000C]$];"
        + "$delim = [\\-$ws];"
        + "$ws $delim* > ' ';"
        + "'-' $delim* > '-';",

        // [8]
        "$ws = [[:Separator:][\\u0009-\\u000C]$];"
        + "$delim = [\\-$ws];"
        + "$ws $delim* > ' ';"
        + "'-' $delim* > '-';"
        + "::Null;",

        // [9]
        "$ws = [[:Separator:][\\u0009-\\u000C]$];"
        + "$delim = [\\-$ws];"
        + "::Null;"
        + "$ws $delim* > ' ';"
        + "'-' $delim* > '-';",

        // [10]
        /*
        "::BEGIN;"
        + "$ws = [[:Separator:][\\u0009-\\u000C]$];"
        + "$delim = [\\-$ws];"
        + "::END;"
        + "$ws $delim* > ' ';"
        + "'-' $delim* > '-';",
         */
        "", // test case commented out below, this is here to keep from messing up the indexes

        // [11]
        /*
        "$ws = [[:Separator:][\\u0009-\\u000C]$];"
        + "$delim = [\\-$ws];"
        + "::BEGIN;"
        + "$ws $delim* > ' ';"
        + "'-' $delim* > '-';"
        + "::END;",
         */
        "", // test case commented out below, this is here to keep from messing up the indexes

        // [12]
        /*
        "$ws = [[:Separator:][\\u0009-\\u000C]$];"
        + "$delim = [\\-$ws];"
        + "$ab = [ab];"
        + "::BEGIN;"
        + "$ws $delim* > ' ';"
        + "'-' $delim* > '-';"
        + "::END;"
        + "::BEGIN;"
        + "$ab { ' ' } $ab > '-';"
        + "c { ' ' > ;"
        + "::END;"
        + "::BEGIN;"
        + "'a-a' > a\\%|a;"
        + "::END;",
         */
        "", // test case commented out below, this is here to keep from messing up the indexes

        // [13]
        "$ws = [[:Separator:][\\u0009-\\u000C]$];"
        + "$delim = [\\-$ws];"
        + "$ab = [ab];"
        + "::Null;"
        + "$ws $delim* > ' ';"
        + "'-' $delim* > '-';"
        + "::Null;"
        + "$ab { ' ' } $ab > '-';"
        + "c { ' ' > ;"
        + "::Null;"
        + "'a-a' > a\\%|a;",

        // [14]
        /*
        "::[abc];"
        + "::BEGIN;"
        + "abc > xy;"
        + "::END;"
        + "::BEGIN;"
        + "aba > yz;"
        + "::END;"
        + "::Upper;",
         */
        "", // test case commented out below, this is here to keep from messing up the indexes

        // [15]
        "::[abc];"
        + "abc > xy;"
        + "::Null;"
        + "aba > yz;"
        + "::Upper;",

        // [16]
        /*
        "::[abc];"
        + "::BEGIN;"
        + "abc <> xy;"
        + "::END;"
        + "::BEGIN;"
        + "aba <> yz;"
        + "::END;"
        + "::Upper(Lower);"
        + "::([XYZ]);",
         */
        "", // test case commented out below, this is here to keep from messing up the indexes

        // [17]
        "::[abc];"
        + "abc <> xy;"
        + "::Null;"
        + "aba <> yz;"
        + "::Upper(Lower);"
        + "::([XYZ]);"
    };

        /*
    (This entire test is commented out below and will need some heavy revision when we re-add
    the ::BEGIN/::END stuff)
        private static final String[] BOGUS_BEGIN_END_RULES = new String[] {
            // [7]
            "::BEGIN;"
            + "abc > xy;"
            + "::BEGIN;"
            + "aba > z;"
            + "::END;"
            + "::END;",

            // [8]
            "abc > xy;"
            + " aba > z;"
            + "::END;",

            // [9]
            "::BEGIN;"
            + "::Upper;"
            + "::END;"
        };
         */

        private static readonly String[] BEGIN_END_TEST_CASES = new String[] {
        BEGIN_END_RULES[0], "abc ababc aba", "xy zbc z",
        //        BEGIN_END_RULES[1], "abc ababc aba", "xy abxy z",
        //        BEGIN_END_RULES[2], "abc ababc aba", "xy abxy z",
        //        BEGIN_END_RULES[3], "abc ababc aba", "xy abxy z",
        BEGIN_END_RULES[4], "abc ababc aba", "xy abxy z",
        BEGIN_END_RULES[5], "abccabaacababcbc", "PXAARXQBR",

        BEGIN_END_RULES[6], "e   e - e---e-  e", "e e e-e-e",
        BEGIN_END_RULES[7], "e   e - e---e-  e", "e e e-e-e",
        BEGIN_END_RULES[8], "e   e - e---e-  e", "e e e-e-e",
        BEGIN_END_RULES[9], "e   e - e---e-  e", "e e e-e-e",
        //        BEGIN_END_RULES[10], "e   e - e---e-  e", "e e e-e-e",
        //        BEGIN_END_RULES[11], "e   e - e---e-  e", "e e e-e-e",
        //        BEGIN_END_RULES[12], "e   e - e---e-  e", "e e e-e-e",
        //        BEGIN_END_RULES[12], "a    a    a    a", "a%a%a%a",
        //        BEGIN_END_RULES[12], "a a-b c b a", "a%a-b cb-a",
        BEGIN_END_RULES[13], "e   e - e---e-  e", "e e e-e-e",
        BEGIN_END_RULES[13], "a    a    a    a", "a%a%a%a",
        BEGIN_END_RULES[13], "a a-b c b a", "a%a-b cb-a",

        //        BEGIN_END_RULES[14], "abc xy ababc xyz aba", "XY xy ABXY xyz YZ",
        BEGIN_END_RULES[15], "abc xy ababc xyz aba", "XY xy ABXY xyz YZ",
        //        BEGIN_END_RULES[16], "abc xy ababc xyz aba", "XY xy ABXY xyz YZ",
        BEGIN_END_RULES[17], "abc xy ababc xyz aba", "XY xy ABXY xyz YZ"
    };

        [Test]
        public void TestBeginEnd()
        {
            // run through the list of test cases above
            for (int i = 0; i < BEGIN_END_TEST_CASES.Length; i += 3)
            {
                Expect(BEGIN_END_TEST_CASES[i], BEGIN_END_TEST_CASES[i + 1], BEGIN_END_TEST_CASES[i + 2]);
            }

            // instantiate the one reversible rule set in the reverse direction and make sure it does the right thing
            Transliterator reversed = Transliterator.CreateFromRules("Reversed", BEGIN_END_RULES[17],
                    Transliterator.Reverse);
            Expect(reversed, "xy XY XYZ yz YZ", "xy abc xaba yz aba");

            // finally, run through the list of syntactically-ill-formed rule sets above and make sure
            // that all of them cause errors
            /*
        (commented out until we have the real ::BEGIN/::END stuff in place
            for (int i = 0; i < BOGUS_BEGIN_END_RULES.Length; i++) {
                try {
                    Transliterator t = Transliterator.CreateFromRules("foo", BOGUS_BEGIN_END_RULES[i],
                            Transliterator.FORWARD);
                    Errln("Should have gotten syntax error from " + BOGUS_BEGIN_END_RULES[i]);
                }
                catch (ArgumentException e) {
                    // this is supposed to happen; do nothing here
                }
            }
             */
        }

        [Test]
        public void TestBeginEndToRules()
        {
            // run through the same list of test cases we used above, but this time, instead of just
            // instantiating a Transliterator from the rules and running the test against it, we instantiate
            // a Transliterator from the rules, do toRules() on it, instantiate a Transliterator from
            // the resulting set of rules, and make sure that the generated rule set is semantically equivalent
            // to (i.e., does the same thing as) the original rule set
            for (int i = 0; i < BEGIN_END_TEST_CASES.Length; i += 3)
            {
                Transliterator t = Transliterator.CreateFromRules("--", BEGIN_END_TEST_CASES[i],
                        Transliterator.Forward);
                String rules = t.ToRules(false);
                Transliterator t2 = Transliterator.CreateFromRules("Test case #" + (i / 3), rules, Transliterator.Forward);
                Expect(t2, BEGIN_END_TEST_CASES[i + 1], BEGIN_END_TEST_CASES[i + 2]);
            }

            {
                // do the same thing for the reversible test case
                Transliterator reversed = Transliterator.CreateFromRules("Reversed", BEGIN_END_RULES[17],
                        Transliterator.Reverse);
                String rules = reversed.ToRules(false);
                Transliterator reversed2 = Transliterator.CreateFromRules("Reversed", rules, Transliterator.Forward);
                Expect(reversed2, "xy XY XYZ yz YZ", "xy abc xaba yz aba");
            }
        }

        [Test]
        public void TestRegisterAlias()
        {
            String longID = "Lower;[aeiou]Upper";
            String shortID = "Any-CapVowels";
            String reallyShortID = "CapVowels";

            Transliterator.RegisterAlias(shortID, longID);

            Transliterator t1 = Transliterator.GetInstance(longID);
            Transliterator t2 = Transliterator.GetInstance(reallyShortID);

            if (!t1.ID.Equals(longID))
                Errln("Transliterator instantiated with long ID doesn't have long ID");
            if (!t2.ID.Equals(reallyShortID))
                Errln("Transliterator instantiated with short ID doesn't have short ID");

            if (!t1.ToRules(true).Equals(t2.ToRules(true)))
                Errln("Alias transliterators aren't the same");

            Transliterator.Unregister(shortID);

            try
            {
                t1 = Transliterator.GetInstance(shortID);
                Errln("Instantiation with short ID succeeded after short ID was unregistered");
            }
            catch (ArgumentException e)
            {
            }

            // try the same thing again, but this time with something other than
            // an instance of CompoundTransliterator
            String realID = "Latin-Greek";
            String fakeID = "Latin-dlgkjdflkjdl";
            Transliterator.RegisterAlias(fakeID, realID);

            t1 = Transliterator.GetInstance(realID);
            t2 = Transliterator.GetInstance(fakeID);

            if (!t1.ToRules(true).Equals(t2.ToRules(true)))
                Errln("Alias transliterators aren't the same");

            Transliterator.Unregister(fakeID);
        }

        /**
         * Test the Halfwidth-Fullwidth transliterator (ticket 6281).
         */
        [Test]
        public void TestHalfwidthFullwidth()
        {
            Transliterator hf = Transliterator.GetInstance("Halfwidth-Fullwidth");
            Transliterator fh = Transliterator.GetInstance("Fullwidth-Halfwidth");

            // Array of 3n items
            // Each item is
            //   "hf"|"fh"|"both",
            //   <Halfwidth>,
            //   <Fullwidth>
            String[] DATA = {
                "both",
                "\uFFE9\uFFEA\uFFEB\uFFEC\u0061\uFF71\u00AF\u0020",
                "\u2190\u2191\u2192\u2193\uFF41\u30A2\uFFE3\u3000",
        };

            for (int i = 0; i < DATA.Length; i += 3)
            {
                switch (DATA[i][0])
                {
                    case 'h': // Halfwidth-Fullwidth only
                        Expect(hf, DATA[i + 1], DATA[i + 2]);
                        break;
                    case 'f': // Fullwidth-Halfwidth only
                        Expect(fh, DATA[i + 2], DATA[i + 1]);
                        break;
                    case 'b': // both directions
                        Expect(hf, DATA[i + 1], DATA[i + 2]);
                        Expect(fh, DATA[i + 2], DATA[i + 1]);
                        break;
                }
            }

        }

        /**
         *  Test Thai.  The text is the first paragraph of "What is Unicode" from the Unicode.org web site.
         *              TODO: confirm that the expected results are correct.
         *              For now, test just confirms that C++ and Java give identical results.
         */
        [Test]
        public void TestThai()
        {
            Transliterator tr = Transliterator.GetInstance("Any-Latin", Transliterator.Forward);
            String thaiText =
                "\u0e42\u0e14\u0e22\u0e1e\u0e37\u0e49\u0e19\u0e10\u0e32\u0e19\u0e41\u0e25\u0e49\u0e27, \u0e04\u0e2d" +
                "\u0e21\u0e1e\u0e34\u0e27\u0e40\u0e15\u0e2d\u0e23\u0e4c\u0e08\u0e30\u0e40\u0e01\u0e35\u0e48\u0e22" +
                "\u0e27\u0e02\u0e49\u0e2d\u0e07\u0e01\u0e31\u0e1a\u0e40\u0e23\u0e37\u0e48\u0e2d\u0e07\u0e02\u0e2d" +
                "\u0e07\u0e15\u0e31\u0e27\u0e40\u0e25\u0e02. \u0e04\u0e2d\u0e21\u0e1e\u0e34\u0e27\u0e40\u0e15\u0e2d" +
                "\u0e23\u0e4c\u0e08\u0e31\u0e14\u0e40\u0e01\u0e47\u0e1a\u0e15\u0e31\u0e27\u0e2d\u0e31\u0e01\u0e29" +
                "\u0e23\u0e41\u0e25\u0e30\u0e2d\u0e31\u0e01\u0e02\u0e23\u0e30\u0e2d\u0e37\u0e48\u0e19\u0e46 \u0e42" +
                "\u0e14\u0e22\u0e01\u0e32\u0e23\u0e01\u0e33\u0e2b\u0e19\u0e14\u0e2b\u0e21\u0e32\u0e22\u0e40\u0e25" +
                "\u0e02\u0e43\u0e2b\u0e49\u0e2a\u0e33\u0e2b\u0e23\u0e31\u0e1a\u0e41\u0e15\u0e48\u0e25\u0e30\u0e15" +
                "\u0e31\u0e27. \u0e01\u0e48\u0e2d\u0e19\u0e2b\u0e19\u0e49\u0e32\u0e17\u0e35\u0e48\u0e4a Unicode \u0e08" +
                "\u0e30\u0e16\u0e39\u0e01\u0e2a\u0e23\u0e49\u0e32\u0e07\u0e02\u0e36\u0e49\u0e19, \u0e44\u0e14\u0e49" +
                "\u0e21\u0e35\u0e23\u0e30\u0e1a\u0e1a encoding \u0e2d\u0e22\u0e39\u0e48\u0e2b\u0e25\u0e32\u0e22\u0e23" +
                "\u0e49\u0e2d\u0e22\u0e23\u0e30\u0e1a\u0e1a\u0e2a\u0e33\u0e2b\u0e23\u0e31\u0e1a\u0e01\u0e32\u0e23" +
                "\u0e01\u0e33\u0e2b\u0e19\u0e14\u0e2b\u0e21\u0e32\u0e22\u0e40\u0e25\u0e02\u0e40\u0e2b\u0e25\u0e48" +
                "\u0e32\u0e19\u0e35\u0e49. \u0e44\u0e21\u0e48\u0e21\u0e35 encoding \u0e43\u0e14\u0e17\u0e35\u0e48" +
                "\u0e21\u0e35\u0e08\u0e33\u0e19\u0e27\u0e19\u0e15\u0e31\u0e27\u0e2d\u0e31\u0e01\u0e02\u0e23\u0e30" +
                "\u0e21\u0e32\u0e01\u0e40\u0e1e\u0e35\u0e22\u0e07\u0e1e\u0e2d: \u0e22\u0e01\u0e15\u0e31\u0e27\u0e2d" +
                "\u0e22\u0e48\u0e32\u0e07\u0e40\u0e0a\u0e48\u0e19, \u0e40\u0e09\u0e1e\u0e32\u0e30\u0e43\u0e19\u0e01" +
                "\u0e25\u0e38\u0e48\u0e21\u0e2a\u0e2b\u0e20\u0e32\u0e1e\u0e22\u0e38\u0e42\u0e23\u0e1b\u0e40\u0e1e" +
                "\u0e35\u0e22\u0e07\u0e41\u0e2b\u0e48\u0e07\u0e40\u0e14\u0e35\u0e22\u0e27 \u0e01\u0e47\u0e15\u0e49" +
                "\u0e2d\u0e07\u0e01\u0e32\u0e23\u0e2b\u0e25\u0e32\u0e22 encoding \u0e43\u0e19\u0e01\u0e32\u0e23\u0e04" +
                "\u0e23\u0e2d\u0e1a\u0e04\u0e25\u0e38\u0e21\u0e17\u0e38\u0e01\u0e20\u0e32\u0e29\u0e32\u0e43\u0e19" +
                "\u0e01\u0e25\u0e38\u0e48\u0e21. \u0e2b\u0e23\u0e37\u0e2d\u0e41\u0e21\u0e49\u0e41\u0e15\u0e48\u0e43" +
                "\u0e19\u0e20\u0e32\u0e29\u0e32\u0e40\u0e14\u0e35\u0e48\u0e22\u0e27 \u0e40\u0e0a\u0e48\u0e19 \u0e20" +
                "\u0e32\u0e29\u0e32\u0e2d\u0e31\u0e07\u0e01\u0e24\u0e29 \u0e01\u0e47\u0e44\u0e21\u0e48\u0e21\u0e35" +
                " encoding \u0e43\u0e14\u0e17\u0e35\u0e48\u0e40\u0e1e\u0e35\u0e22\u0e07\u0e1e\u0e2d\u0e2a\u0e33\u0e2b" +
                "\u0e23\u0e31\u0e1a\u0e17\u0e38\u0e01\u0e15\u0e31\u0e27\u0e2d\u0e31\u0e01\u0e29\u0e23, \u0e40\u0e04" +
                "\u0e23\u0e37\u0e48\u0e2d\u0e07\u0e2b\u0e21\u0e32\u0e22\u0e27\u0e23\u0e23\u0e04\u0e15\u0e2d\u0e19" +
                " \u0e41\u0e25\u0e30\u0e2a\u0e31\u0e0d\u0e25\u0e31\u0e01\u0e29\u0e13\u0e4c\u0e17\u0e32\u0e07\u0e40" +
                "\u0e17\u0e04\u0e19\u0e34\u0e04\u0e17\u0e35\u0e48\u0e43\u0e0a\u0e49\u0e01\u0e31\u0e19\u0e2d\u0e22" +
                "\u0e39\u0e48\u0e17\u0e31\u0e48\u0e27\u0e44\u0e1b.";

            String latinText =
                "doy ph\u1ee5\u0304\u0302n \u1e6d\u0304h\u0101n l\u00e6\u0302w, khxmphiwtexr\u0312 ca ke\u012b\u0300" +
                "ywk\u0304\u0125xng k\u1ea1b re\u1ee5\u0304\u0300xng k\u0304hxng t\u1ea1wlek\u0304h. khxmphiwtexr" +
                "\u0312 c\u1ea1d k\u0115b t\u1ea1w x\u1ea1ks\u0304\u02b9r l\u00e6a x\u1ea1kk\u0304h ra x\u1ee5\u0304" +
                "\u0300n\u00ab doy k\u0101r k\u1ea3h\u0304nd h\u0304m\u0101ylek\u0304h h\u0304\u0131\u0302 s\u0304" +
                "\u1ea3h\u0304r\u1ea1b t\u00e6\u0300la t\u1ea1w. k\u0300xn h\u0304n\u0302\u0101 th\u012b\u0300\u0301" +
                " Unicode ca t\u0304h\u016bk s\u0304r\u0302\u0101ng k\u0304h\u1ee5\u0302n, d\u1ecb\u0302 m\u012b " +
                "rabb encoding xy\u016b\u0300 h\u0304l\u0101y r\u0302xy rabb s\u0304\u1ea3h\u0304r\u1ea1b k\u0101" +
                "r k\u1ea3h\u0304nd h\u0304m\u0101ylek\u0304h h\u0304el\u0300\u0101 n\u012b\u0302. m\u1ecb\u0300m" +
                "\u012b encoding d\u0131 th\u012b\u0300 m\u012b c\u1ea3nwn t\u1ea1w x\u1ea1kk\u0304hra m\u0101k p" +
                "he\u012byng phx: yk t\u1ea1wx\u1ef3\u0101ng ch\u00e8n, c\u0304heph\u0101a n\u0131 kl\u00f9m s\u0304" +
                "h\u0304p\u0323h\u0101ph yurop phe\u012byng h\u0304\u00e6\u0300ng de\u012byw k\u0306 t\u0302xngk\u0101" +
                "r h\u0304l\u0101y encoding n\u0131 k\u0101r khrxbkhlum thuk p\u0323h\u0101s\u0304\u02b9\u0101 n\u0131" +
                " kl\u00f9m. h\u0304r\u1ee5\u0304x m\u00e6\u0302t\u00e6\u0300 n\u0131 p\u0323h\u0101s\u0304\u02b9" +
                "\u0101 de\u012b\u0300yw ch\u00e8n p\u0323h\u0101s\u0304\u02b9\u0101 x\u1ea1ngkvs\u0304\u02b9 k\u0306" +
                " m\u1ecb\u0300m\u012b encoding d\u0131 th\u012b\u0300 phe\u012byng phx s\u0304\u1ea3h\u0304r\u1ea1" +
                "b thuk t\u1ea1w x\u1ea1ks\u0304\u02b9r, kher\u1ee5\u0304\u0300xngh\u0304m\u0101y wrrkh txn l\u00e6" +
                "a s\u0304\u1ea1\u1ef5l\u1ea1ks\u0304\u02b9\u1e47\u0312 th\u0101ng thekhnikh th\u012b\u0300 ch\u0131" +
                "\u0302 k\u1ea1n xy\u016b\u0300 th\u1ea1\u0300wp\u1ecb.";

            Expect(tr, thaiText, latinText);
        }


        //======================================================================
        // These tests are not mirrored (yet) in icu4c at
        // source/test/intltest/transtst.cpp
        //======================================================================

        /**
         * Improve code coverage.
         */
        [Test]
        public void TestCoverage()
        {
            // NullTransliterator
            Transliterator t = Transliterator.GetInstance("Null", Transliterator.Forward);
            Expect(t, "a", "a");

            // Source, target set
            t = Transliterator.GetInstance("Latin-Greek", Transliterator.Forward);
            t.Filter = (new UnicodeSet("[A-Z]"));
            Logln("source = " + t.GetSourceSet());
            Logln("target = " + t.GetTargetSet());

            t = Transliterator.CreateFromRules("x", "(.) > &Any-Hex($1);", Transliterator.Forward);
            Logln("source = " + t.GetSourceSet());
            Logln("target = " + t.GetTargetSet());
        }
        /*
         * Test case for threading problem in NormalizationTransliterator
         * reported by ticket#5160
         */
        [Test]
        public void TestT5160()
        {
            String[] testData = {
                "a",
                "b",
                "\u09BE",
                "A\u0301",
            };
            String[] expected = {
                "a",
                "b",
                "\u09BE",
                "\u00C1",
            };
            Transliterator translit = Transliterator.GetInstance("NFC");
            NormTranslitTask[] tasks = new NormTranslitTask[testData.Length];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = new NormTranslitTask(translit, testData[i], expected[i]);
            }
            TestUtil.RunUntilDone(tasks.Select(x => (Action)(() => x.Run())).ToArray());

            for (int i = 0; i < tasks.Length; i++)
            {
                if (tasks[i].ErrorMessage != null)
                {
                    Console.Out.WriteLine("Fail: thread#" + i + " " + tasks[i].ErrorMessage);
                    break;
                }
            }
        }

        internal class NormTranslitTask //implements Runnable
        {
            Transliterator translit;
            String testData;
            String expectedData;
            String errorMsg;

            internal NormTranslitTask(Transliterator translit, String testData, String expectedData)
            {
                this.translit = translit;
                this.testData = testData;
                this.expectedData = expectedData;
            }

            public void Run()
            {
                errorMsg = null;
                StringBuffer inBuf = new StringBuffer(testData);
                StringBuffer expectedBuf = new StringBuffer(expectedData);

                for (int i = 0; i < 1000; i++)
                {
                    String @in = inBuf.ToString();
                    String @out = translit.Transliterate(@in);
                    String expected = expectedBuf.ToString();
                    if (!@out.Equals(expected))
                    {
                        errorMsg = "in {" + @in + "} / out {" + @out + "} / expected {" + expected + "}";
                        break;
                    }
                    inBuf.Append(testData);
                    expectedBuf.Append(expectedData);
                }
            }

            public String ErrorMessage
            {
                get { return errorMsg; }
            }
        }

        //======================================================================
        // Support methods
        //======================================================================
        internal static void Expect(String rules,
                String source,
                String expectedResult,
                TransliterationPosition pos)
        {
            Transliterator t = Transliterator.CreateFromRules("<ID>", rules, Transliterator.Forward);
            Expect(t, source, expectedResult, pos);
        }

        internal static void Expect(String rules, String source, String expectedResult)
        {
            Expect(rules, source, expectedResult, null);
        }

        internal static void Expect(Transliterator t, String source, String expectedResult,
                Transliterator reverseTransliterator)
        {
            Expect(t, source, expectedResult);
            if (reverseTransliterator != null)
            {
                Expect(reverseTransliterator, expectedResult, source);
            }
        }

        internal static void Expect(Transliterator t, String source, String expectedResult)
        {
            Expect(t, source, expectedResult, (TransliterationPosition)null);
        }

        internal static void Expect(Transliterator t, String source, String expectedResult,
                TransliterationPosition pos)
        {
            if (pos == null)
            {
                String result = t.Transliterate(source);
                if (!ExpectAux(t.ID + ":String", source, result, expectedResult)) return;
            }

            TransliterationPosition index = null;
            if (pos == null)
            {
                index = new TransliterationPosition(0, source.Length, 0, source.Length);
            }
            else
            {
                index = new TransliterationPosition(pos.ContextStart, pos.ContextLimit,
                        pos.Start, pos.Limit);
            }

            ReplaceableString rsource = new ReplaceableString(source);

            t.FinishTransliteration(rsource, index);
            // Do it all at once -- below we do it incrementally

            if (index.Start != index.Limit)
            {
                ExpectAux(t.ID + ":UNFINISHED", source,
                        "start: " + index.Start + ", limit: " + index.Limit, false, expectedResult);
                return;
            }
            {
                String result = rsource.ToString();
                if (!ExpectAux(t.ID + ":Replaceable", source, result, expectedResult)) return;



                if (pos == null)
                {
                    index = new TransliterationPosition();
                }
                else
                {
                    index = new TransliterationPosition(pos.ContextStart, pos.ContextLimit,
                            pos.Start, pos.Limit);
                }

                // Test incremental transliteration -- this result
                // must be the same after we finalize (see below).
                List<String> v = new List<String>();
                v.Add(source);
                rsource.Replace(0, rsource.Length - 0, ""); // ICU4N: Corrected 2nd parameter
                if (pos != null)
                {
                    rsource.Replace(0, 0 - 0, source); // ICU4N: Corrected 2nd parameter
                    v.Add(UtilityExtensions.FormatInput(rsource, index));
                    t.Transliterate(rsource, index);
                    v.Add(UtilityExtensions.FormatInput(rsource, index));
                }
                else
                {
                    for (int i = 0; i < source.Length; ++i)
                    {
                        //v.add(i == 0 ? "" : " + " + source.charAt(i) + "");
                        //log.append(source.charAt(i)).append(" -> "));
                        t.Transliterate(rsource, index, source[i]);
                        //v.add(UtilityExtensions.FormatInput(rsource, index) + source.substring(i+1));
                        v.Add(UtilityExtensions.FormatInput(rsource, index) +
                                ((i < source.Length - 1) ? (" + '" + source[i + 1] + "' ->") : " =>"));
                    }
                }

                // As a final step in keyboard transliteration, we must call
                // transliterate to finish off any pending partial matches that
                // were waiting for more input.
                t.FinishTransliteration(rsource, index);
                result = rsource.ToString();
                //log.append(" => ").append(rsource.toString());
                v.Add(result);

                String[] results = new String[v.Count];
                v.CopyTo(results);
                ExpectAux(t.ID + ":Incremental", results,
                        result.Equals(expectedResult),
                        expectedResult);
            }
        }

        internal static bool ExpectAux(String tag, String source,
                String result, String expectedResult)
        {
            return ExpectAux(tag, new String[] { source, result },
                    result.Equals(expectedResult),
                    expectedResult);
        }

        internal static bool ExpectAux(String tag, String source,
        String result, bool pass,
        String expectedResult)
        {
            return ExpectAux(tag, new String[] { source, result },
                    pass,
                    expectedResult);
        }

        internal static bool ExpectAux(String tag, String source,
        bool pass,
        String expectedResult)
        {
            return ExpectAux(tag, new String[] { source },
                    pass,
                    expectedResult);
        }

        internal static bool ExpectAux(String tag, String[] results, bool pass,
        String expectedResult)
        {
            msg((pass ? "(" : "FAIL: (") + tag + ")", pass ? LOG : ERR, true, true);

            for (int i = 0; i < results.Length; ++i)
            {
                String label;
                if (i == 0)
                {
                    label = "source:   ";
                }
                else if (i == results.Length - 1)
                {
                    label = "result:   ";
                }
                else
                {
                    if (!IsVerbose() && pass) continue;
                    label = "interm" + i + ":  ";
                }
                msg("    " + label + results[i], pass ? LOG : ERR, false, true);
            }

            if (!pass)
            {
                msg("    expected: " + expectedResult, ERR, false, true);
            }

            return pass;
        }

        static private void AssertTransform(String message, String expected, IStringTransform t, String source)
        {
            assertEquals(message + " " + source, expected, t.Transform(source));
        }


        static private void AssertTransform(String message, String expected, IStringTransform t, IStringTransform back, String source, String source2)
        {
            assertEquals(message + " " + source, expected, t.Transform(source));
            assertEquals(message + " " + source2, expected, t.Transform(source2));
            assertEquals(message + " " + expected, source, back.Transform(expected));
        }

        /*
         * Tests the method public Enumeration<String> getAvailableTargets(String source)
         */
        [Test]
        public void TestGetAvailableTargets()
        {
            try
            {
                // Tests when if (targets == null) is true
                Transliterator.GetAvailableTargets("");
            }
            catch (Exception e)
            {
                Errln("TransliteratorRegistry.getAvailableTargets(String) was not " + "supposed to return an exception.");
            }
        }

        /*
         * Tests the method public Enumeration<String> getAvailableVariants(String source, String target)
         */
        [Test]
        public void TestGetAvailableVariants()
        {
            try
            {
                // Tests when if (targets == null) is true
                Transliterator.GetAvailableVariants("", "");
            }
            catch (Exception e)
            {
                Errln("TransliteratorRegistry.getAvailableVariants(String) was not " + "supposed to return an exception.");
            }
        }

        /*
         * Tests the mehtod String nextLine() in RuleBody
         */
        [Test]
        public void TestNextLine()
        {
            // Tests when "if (s != null && s.Length > 0 && s.charAt(s.Length - 1) == '\\') is true
            try
            {
                Transliterator.CreateFromRules("gif", "\\", Transliterator.Forward);
            }
            catch (Exception e)
            {
                Errln("TransliteratorParser.nextLine() was not suppose to return an " +
                "exception for a rule of '\\'");
            }
        }
    }
}
