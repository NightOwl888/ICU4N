using ICU4N.Lang;
using ICU4N.Support.Collections;
using ICU4N.Util;
using NUnit.Framework;
using System;
using System.Collections;
using System.Globalization;
using System.Text;

namespace ICU4N.Dev.Test.Lang
{
    public class DataDrivenUScriptTest : TestFmwk
    {
        private static String ScriptsToString(int[] scripts)
        {
            if (scripts == null)
            {
                return "null";
            }
            StringBuilder sb = new StringBuilder();
            foreach (int script in scripts)
            {
                if (sb.Length > 0)
                {
                    sb.Append(' ');
                }
                sb.Append(UScript.GetShortName(script));
            }
            return sb.ToString();
        }

        private static void AssertEqualScripts(String msg, int[] expectedScripts, int[] actualScripts)
        {
            assertEquals(msg, ScriptsToString(expectedScripts), ScriptsToString(actualScripts));
        }

        public class LocaleGetCodeTest
        {
            public static IEnumerable TestData
            {
                get
                {
                    yield return new TestCaseData(new ULocale("en"), UScript.LATIN);
                    yield return new TestCaseData(new ULocale("en_US"), UScript.LATIN);
                    yield return new TestCaseData(new ULocale("sr"), UScript.CYRILLIC);
                    yield return new TestCaseData(new ULocale("ta"), UScript.TAMIL);
                    yield return new TestCaseData(new ULocale("te_IN"), UScript.TELUGU);
                    yield return new TestCaseData(new ULocale("hi"), UScript.DEVANAGARI);
                    yield return new TestCaseData(new ULocale("he"), UScript.HEBREW);
                    yield return new TestCaseData(new ULocale("ar"), UScript.ARABIC);
                    yield return new TestCaseData(new ULocale("abcde"), UScript.INVALID_CODE);
                    yield return new TestCaseData(new ULocale("abcde_cdef"), UScript.INVALID_CODE);
                    yield return new TestCaseData(new ULocale("iw"), UScript.HEBREW);
                }
            }

            [Test, TestCaseSource(typeof(LocaleGetCodeTest), "TestData")]
            public void TestLocaleGetCode(ULocale testLocaleName, int expected)
            {
                int[] code = UScript.GetCode(testLocaleName);
                if (code == null)
                {
                    if (expected != UScript.INVALID_CODE)
                    {
                        Errln("Error testing UScript.getCode(). Got: null" + " Expected: " + expected + " for locale "
                                + testLocaleName);
                    }
                }
                else if ((code[0] != expected))
                {
                    Errln("Error testing UScript.getCode(). Got: " + code[0] + " Expected: " + expected + " for locale "
                            + testLocaleName);
                }

                ULocale defaultLoc = ULocale.GetDefault();
                ULocale esperanto = new ULocale("eo_DE");
                ULocale.SetDefault(esperanto);
                code = UScript.GetCode(esperanto);
                if (code != null)
                {
                    if (code[0] != UScript.LATIN)
                    {
                        Errln("Did not get the expected script code for Esperanto");
                    }
                }
                else
                {
                    Warnln("Could not load the locale data.");
                }
                ULocale.SetDefault(defaultLoc);

                // Should work regardless of whether we have locale data for the language.
                AssertEqualScripts("tg script: Cyrl", // Tajik
                        new int[] { UScript.CYRILLIC }, UScript.GetCode(new ULocale("tg")));
                AssertEqualScripts("xsr script: Deva", // Sherpa
                        new int[] { UScript.DEVANAGARI }, UScript.GetCode(new ULocale("xsr")));

                // Multi-script languages.
                AssertEqualScripts("ja scripts: Kana Hira Hani",
                        new int[] { UScript.KATAKANA, UScript.HIRAGANA, UScript.HAN }, UScript.GetCode(ULocale.JAPANESE));
                AssertEqualScripts("ko scripts: Hang Hani", new int[] { UScript.HANGUL, UScript.HAN },
                        UScript.GetCode(ULocale.KOREAN));
                AssertEqualScripts("zh script: Hani", new int[] { UScript.HAN }, UScript.GetCode(ULocale.CHINESE));
                AssertEqualScripts("zh-Hant scripts: Hani Bopo", new int[] { UScript.HAN, UScript.BOPOMOFO },
                        UScript.GetCode(ULocale.TRADITIONAL_CHINESE));
                AssertEqualScripts("zh-TW scripts: Hani Bopo", new int[] { UScript.HAN, UScript.BOPOMOFO },
                        UScript.GetCode(ULocale.TAIWAN));

                // Ambiguous API, but this probably wants to return Latin rather than Rongorongo (Roro).
                AssertEqualScripts("ro-RO script: Latn", new int[] { UScript.LATIN }, UScript.GetCode("ro-RO")); // String
                                                                                                                 // not
                                                                                                                 // ULocale
            }
        }

        public class TestMultipleUScript : TestFmwk
        {
            public static IEnumerable TestData
            {
                get
                {
                    yield return new TestCaseData("ja", new int[] { UScript.KATAKANA, UScript.HIRAGANA, UScript.HAN }, new CultureInfo("ja"));
                    yield return new TestCaseData("ko_KR", new int[] { UScript.HANGUL, UScript.HAN }, new CultureInfo("ko-KR"));
                    yield return new TestCaseData("zh", new int[] { UScript.HAN }, new CultureInfo("zh"));
                    yield return new TestCaseData("zh_TW", new int[] { UScript.HAN, UScript.BOPOMOFO }, new CultureInfo("zh-TW"));
                }
            }

            [Test, TestCaseSource(typeof(TestMultipleUScript), "TestData")]
            public void TestMultipleCodes(string testLocaleName, int[] expected, CultureInfo testLocale)
            {
                int[] code = UScript.GetCode(testLocaleName);
                if (code != null)
                {
                    for (int j = 0; j < code.Length; j++)
                    {
                        if (code[j] != expected[j])
                        {
                            Errln("Error testing UScript.getCode(). Got: " + code[j] + " Expected: " + expected[j]
                                    + " for locale " + testLocaleName);
                        }
                    }
                }
                else
                {
                    Errln("Error testing UScript.getCode() for locale " + testLocaleName);
                }

                Logln("  Testing UScript.getCode(Locale) with locale: " + testLocale.DisplayName);
                code = UScript.GetCode(testLocale);
                if (code != null)
                {
                    for (int j = 0; j < code.Length; j++)
                    {
                        if (code[j] != expected[j])
                        {
                            Errln("Error testing UScript.getCode(). Got: " + code[j] + " Expected: " + expected[j]
                                    + " for locale " + testLocaleName);
                        }
                    }
                }
                else
                {
                    Errln("Error testing UScript.getCode() for locale " + testLocaleName);
                }
            }
        }

        public class GetCodeTest : TestFmwk
        {
            public static IEnumerable TestData
            {
                get
                {
                    /* test locale */
                    yield return new TestCaseData("en", UScript.LATIN);
                    yield return new TestCaseData("en_US", UScript.LATIN);
                    yield return new TestCaseData("sr", UScript.CYRILLIC);
                    yield return new TestCaseData("ta", UScript.TAMIL);
                    yield return new TestCaseData("gu", UScript.GUJARATI);
                    yield return new TestCaseData("te_IN", UScript.TELUGU);
                    yield return new TestCaseData("hi", UScript.DEVANAGARI);
                    yield return new TestCaseData("he", UScript.HEBREW);
                    yield return new TestCaseData("ar", UScript.ARABIC);
                    yield return new TestCaseData("abcde", UScript.INVALID_CODE);
                    yield return new TestCaseData("abscde_cdef", UScript.INVALID_CODE);
                    yield return new TestCaseData("iw", UScript.HEBREW);
                    /* test abbr */
                    yield return new TestCaseData("Hani", UScript.HAN);
                    yield return new TestCaseData("Hang", UScript.HANGUL);
                    yield return new TestCaseData("Hebr", UScript.HEBREW);
                    yield return new TestCaseData("Hira", UScript.HIRAGANA);
                    yield return new TestCaseData("Knda", UScript.KANNADA);
                    yield return new TestCaseData("Kana", UScript.KATAKANA);
                    yield return new TestCaseData("Khmr", UScript.KHMER);
                    yield return new TestCaseData("Lao", UScript.LAO);
                    yield return new TestCaseData("Latn", UScript.LATIN); /* "Latf","Latg", */
                    yield return new TestCaseData("Mlym", UScript.MALAYALAM);
                    yield return new TestCaseData("Mong", UScript.MONGOLIAN);
                    /* test names */
                    yield return new TestCaseData("CYRILLIC", UScript.CYRILLIC);
                    yield return new TestCaseData("DESERET", UScript.DESERET);
                    yield return new TestCaseData("DEVANAGARI", UScript.DEVANAGARI);
                    yield return new TestCaseData("ETHIOPIC", UScript.ETHIOPIC);
                    yield return new TestCaseData("GEORGIAN", UScript.GEORGIAN);
                    yield return new TestCaseData("GOTHIC", UScript.GOTHIC);
                    yield return new TestCaseData("GREEK", UScript.GREEK);
                    yield return new TestCaseData("GUJARATI", UScript.GUJARATI);
                    yield return new TestCaseData("COMMON", UScript.COMMON);
                    yield return new TestCaseData("INHERITED", UScript.INHERITED);
                    /* test lower case names */
                    yield return new TestCaseData("malayalam", UScript.MALAYALAM);
                    yield return new TestCaseData("mongolian", UScript.MONGOLIAN);
                    yield return new TestCaseData("myanmar", UScript.MYANMAR);
                    yield return new TestCaseData("ogham", UScript.OGHAM);
                    yield return new TestCaseData("old-italic", UScript.OLD_ITALIC);
                    yield return new TestCaseData("oriya", UScript.ORIYA);
                    yield return new TestCaseData("runic", UScript.RUNIC);
                    yield return new TestCaseData("sinhala", UScript.SINHALA);
                    yield return new TestCaseData("syriac", UScript.SYRIAC);
                    yield return new TestCaseData("tamil", UScript.TAMIL);
                    yield return new TestCaseData("telugu", UScript.TELUGU);
                    yield return new TestCaseData("thaana", UScript.THAANA);
                    yield return new TestCaseData("thai", UScript.THAI);
                    yield return new TestCaseData("tibetan", UScript.TIBETAN);
                    /* test the bounds */
                    yield return new TestCaseData("Cans", UScript.CANADIAN_ABORIGINAL);
                    yield return new TestCaseData("arabic", UScript.ARABIC);
                    yield return new TestCaseData("Yi", UScript.YI);
                    yield return new TestCaseData("Zyyy", UScript.COMMON);
                }
            }

            [Test, TestCaseSource(typeof(GetCodeTest), "TestData")]
            public void TestGetCode(string testName, int expected)
            {
                int[] code = UScript.GetCode(testName);
                if (code == null)
                {
                    if (expected != UScript.INVALID_CODE)
                    {
                        // getCode returns null if the code could not be found
                        Errln("Error testing UScript.getCode(). Got: null" + " Expected: " + expected + " for locale "
                                + testName);
                    }
                }
                else if ((code[0] != expected))
                {
                    Errln("Error testing UScript.getCode(). Got: " + code[0] + " Expected: " + expected + " for locale "
                            + testName);
                }
            }
        }

        public class GetNameTest
        {
            public static IEnumerable TestData
            {
                get
                {
                    yield return new TestCaseData(UScript.CYRILLIC, "Cyrillic");
                    yield return new TestCaseData(UScript.DESERET, "Deseret");
                    yield return new TestCaseData(UScript.DEVANAGARI, "Devanagari");
                    yield return new TestCaseData(UScript.ETHIOPIC, "Ethiopic");
                    yield return new TestCaseData(UScript.GEORGIAN, "Georgian");
                    yield return new TestCaseData(UScript.GOTHIC, "Gothic");
                    yield return new TestCaseData(UScript.GREEK, "Greek");
                    yield return new TestCaseData(UScript.GUJARATI, "Gujarati");
                }
            }

            [Test, TestCaseSource(typeof(GetNameTest), "TestData")]
            public void TestGetName(int testCode, string expected)
            {
                String scriptName = UScript.GetName(testCode);
                if (!expected.Equals(scriptName))
                {
                    Errln("Error testing UScript.getName(). Got: " + scriptName + " Expected: " + expected);
                }
            }
        }

        public class GetShortNameTest
        {
            public static IEnumerable TestData
            {
                get
                {
                    yield return new TestCaseData(UScript.HAN, "Hani");
                    yield return new TestCaseData(UScript.HANGUL, "Hang");
                    yield return new TestCaseData(UScript.HEBREW, "Hebr");
                    yield return new TestCaseData(UScript.HIRAGANA, "Hira");
                    yield return new TestCaseData(UScript.KANNADA, "Knda");
                    yield return new TestCaseData(UScript.KATAKANA, "Kana");
                    yield return new TestCaseData(UScript.KHMER, "Khmr");
                    yield return new TestCaseData(UScript.LAO, "Laoo");
                    yield return new TestCaseData(UScript.LATIN, "Latn");
                    yield return new TestCaseData(UScript.MALAYALAM, "Mlym");
                    yield return new TestCaseData(UScript.MONGOLIAN, "Mong");
                }
            }

            [Test, TestCaseSource(typeof(GetShortNameTest), "TestData")]
            public void TestGetShortName(int testCode, string expected)
            {
                string shortName = UScript.GetShortName(testCode);
                if (!expected.Equals(shortName))
                {
                    Errln("Error testing UScript.getShortName(). Got: " + shortName + " Expected: " + expected);
                }
            }
        }

        public class GetScriptTest
        {
            public static IEnumerable TestData
            {
                get
                {
                    yield return new TestCaseData(0x0000FF9D, UScript.KATAKANA);
                    yield return new TestCaseData(0x0000FFBE, UScript.HANGUL);
                    yield return new TestCaseData(0x0000FFC7, UScript.HANGUL);
                    yield return new TestCaseData(0x0000FFCF, UScript.HANGUL);
                    yield return new TestCaseData(0x0000FFD7, UScript.HANGUL);
                    yield return new TestCaseData(0x0000FFDC, UScript.HANGUL);
                    yield return new TestCaseData(0x00010300, UScript.OLD_ITALIC);
                    yield return new TestCaseData(0x00010330, UScript.GOTHIC);
                    yield return new TestCaseData(0x0001034A, UScript.GOTHIC);
                    yield return new TestCaseData(0x00010400, UScript.DESERET);
                    yield return new TestCaseData(0x00010428, UScript.DESERET);
                    yield return new TestCaseData(0x0001D167, UScript.INHERITED);
                    yield return new TestCaseData(0x0001D17B, UScript.INHERITED);
                    yield return new TestCaseData(0x0001D185, UScript.INHERITED);
                    yield return new TestCaseData(0x0001D1AA, UScript.INHERITED);
                    yield return new TestCaseData(0x00020000, UScript.HAN);
                    yield return new TestCaseData(0x00000D02, UScript.MALAYALAM);
                    yield return new TestCaseData(0x00050005, UScript.UNKNOWN); // new Zzzz value in Unicode 5.0
                    yield return new TestCaseData(0x00000000, UScript.COMMON);
                    yield return new TestCaseData(0x0001D169, UScript.INHERITED);
                    yield return new TestCaseData(0x0001D182, UScript.INHERITED);
                    yield return new TestCaseData(0x0001D18B, UScript.INHERITED);
                    yield return new TestCaseData(0x0001D1AD, UScript.INHERITED);
                }
            }

            [Test, TestCaseSource(typeof(GetScriptTest), "TestData")]
            public void TestGetScript(int codepoint, int expected)
            {

                int code = UScript.INVALID_CODE;

                code = UScript.GetScript(codepoint);

                if (code != expected)
                {
                    Errln("Error testing UScript.getScript(). Got: " + code + " Expected: " + expected
                            + " for codepoint 0x + Hex(codepoint).");
                }
            }
        }
    }
}
