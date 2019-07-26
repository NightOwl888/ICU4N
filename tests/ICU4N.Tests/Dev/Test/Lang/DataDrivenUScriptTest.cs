using ICU4N.Globalization;
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
                    yield return new TestCaseData(new ULocale("en"), UScript.Latin);
                    yield return new TestCaseData(new ULocale("en_US"), UScript.Latin);
                    yield return new TestCaseData(new ULocale("sr"), UScript.Cyrillic);
                    yield return new TestCaseData(new ULocale("ta"), UScript.Tamil);
                    yield return new TestCaseData(new ULocale("te_IN"), UScript.Telugu);
                    yield return new TestCaseData(new ULocale("hi"), UScript.Devanagari);
                    yield return new TestCaseData(new ULocale("he"), UScript.Hebrew);
                    yield return new TestCaseData(new ULocale("ar"), UScript.Arabic);
                    yield return new TestCaseData(new ULocale("abcde"), UScript.InvalidCode);
                    yield return new TestCaseData(new ULocale("abcde_cdef"), UScript.InvalidCode);
                    yield return new TestCaseData(new ULocale("iw"), UScript.Hebrew);
                }
            }

            [Test, TestCaseSource(typeof(LocaleGetCodeTest), "TestData")]
            public void TestLocaleGetCode(ULocale testLocaleName, int expected)
            {
                int[] code = UScript.GetCode(testLocaleName);
                if (code == null)
                {
                    if (expected != UScript.InvalidCode)
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
                    if (code[0] != UScript.Latin)
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
                        new int[] { UScript.Cyrillic }, UScript.GetCode(new ULocale("tg")));
                AssertEqualScripts("xsr script: Deva", // Sherpa
                        new int[] { UScript.Devanagari }, UScript.GetCode(new ULocale("xsr")));

                // Multi-script languages.
                AssertEqualScripts("ja scripts: Kana Hira Hani",
                        new int[] { UScript.Katakana, UScript.Hiragana, UScript.Han }, UScript.GetCode(ULocale.JAPANESE));
                AssertEqualScripts("ko scripts: Hang Hani", new int[] { UScript.Hangul, UScript.Han },
                        UScript.GetCode(ULocale.KOREAN));
                AssertEqualScripts("zh script: Hani", new int[] { UScript.Han }, UScript.GetCode(ULocale.CHINESE));
                AssertEqualScripts("zh-Hant scripts: Hani Bopo", new int[] { UScript.Han, UScript.Bopomofo },
                        UScript.GetCode(ULocale.TRADITIONAL_CHINESE));
                AssertEqualScripts("zh-TW scripts: Hani Bopo", new int[] { UScript.Han, UScript.Bopomofo },
                        UScript.GetCode(ULocale.TAIWAN));

                // Ambiguous API, but this probably wants to return Latin rather than Rongorongo (Roro).
                AssertEqualScripts("ro-RO script: Latn", new int[] { UScript.Latin }, UScript.GetCode("ro-RO")); // String
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
                    yield return new TestCaseData("ja", new int[] { UScript.Katakana, UScript.Hiragana, UScript.Han }, new CultureInfo("ja"));
                    yield return new TestCaseData("ko_KR", new int[] { UScript.Hangul, UScript.Han }, new CultureInfo("ko-KR"));
                    yield return new TestCaseData("zh", new int[] { UScript.Han }, new CultureInfo("zh"));
                    yield return new TestCaseData("zh_TW", new int[] { UScript.Han, UScript.Bopomofo }, new CultureInfo("zh-TW"));
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
                    yield return new TestCaseData("en", UScript.Latin);
                    yield return new TestCaseData("en_US", UScript.Latin);
                    yield return new TestCaseData("sr", UScript.Cyrillic);
                    yield return new TestCaseData("ta", UScript.Tamil);
                    yield return new TestCaseData("gu", UScript.Gujarati);
                    yield return new TestCaseData("te_IN", UScript.Telugu);
                    yield return new TestCaseData("hi", UScript.Devanagari);
                    yield return new TestCaseData("he", UScript.Hebrew);
                    yield return new TestCaseData("ar", UScript.Arabic);
                    yield return new TestCaseData("abcde", UScript.InvalidCode);
                    yield return new TestCaseData("abscde_cdef", UScript.InvalidCode);
                    yield return new TestCaseData("iw", UScript.Hebrew);
                    /* test abbr */
                    yield return new TestCaseData("Hani", UScript.Han);
                    yield return new TestCaseData("Hang", UScript.Hangul);
                    yield return new TestCaseData("Hebr", UScript.Hebrew);
                    yield return new TestCaseData("Hira", UScript.Hiragana);
                    yield return new TestCaseData("Knda", UScript.Kannada);
                    yield return new TestCaseData("Kana", UScript.Katakana);
                    yield return new TestCaseData("Khmr", UScript.Khmer);
                    yield return new TestCaseData("Lao", UScript.Lao);
                    yield return new TestCaseData("Latn", UScript.Latin); /* "Latf","Latg", */
                    yield return new TestCaseData("Mlym", UScript.Malayalam);
                    yield return new TestCaseData("Mong", UScript.Mongolian);
                    /* test names */
                    yield return new TestCaseData("CYRILLIC", UScript.Cyrillic);
                    yield return new TestCaseData("DESERET", UScript.Deseret);
                    yield return new TestCaseData("DEVANAGARI", UScript.Devanagari);
                    yield return new TestCaseData("ETHIOPIC", UScript.Ethiopic);
                    yield return new TestCaseData("GEORGIAN", UScript.Georgian);
                    yield return new TestCaseData("GOTHIC", UScript.Gothic);
                    yield return new TestCaseData("GREEK", UScript.Greek);
                    yield return new TestCaseData("GUJARATI", UScript.Gujarati);
                    yield return new TestCaseData("COMMON", UScript.Common);
                    yield return new TestCaseData("INHERITED", UScript.Inherited);
                    /* test lower case names */
                    yield return new TestCaseData("malayalam", UScript.Malayalam);
                    yield return new TestCaseData("mongolian", UScript.Mongolian);
                    yield return new TestCaseData("myanmar", UScript.Myanmar);
                    yield return new TestCaseData("ogham", UScript.Ogham);
                    yield return new TestCaseData("old-italic", UScript.OldItalic);
                    yield return new TestCaseData("oriya", UScript.Oriya);
                    yield return new TestCaseData("runic", UScript.Runic);
                    yield return new TestCaseData("sinhala", UScript.Sinhala);
                    yield return new TestCaseData("syriac", UScript.Syriac);
                    yield return new TestCaseData("tamil", UScript.Tamil);
                    yield return new TestCaseData("telugu", UScript.Telugu);
                    yield return new TestCaseData("thaana", UScript.Thaana);
                    yield return new TestCaseData("thai", UScript.Thai);
                    yield return new TestCaseData("tibetan", UScript.Tibetan);
                    /* test the bounds */
                    yield return new TestCaseData("Cans", UScript.CanadianAboriginal);
                    yield return new TestCaseData("arabic", UScript.Arabic);
                    yield return new TestCaseData("Yi", UScript.Yi);
                    yield return new TestCaseData("Zyyy", UScript.Common);
                }
            }

            [Test, TestCaseSource(typeof(GetCodeTest), "TestData")]
            public void TestGetCode(string testName, int expected)
            {
                int[] code = UScript.GetCode(testName);
                if (code == null)
                {
                    if (expected != UScript.InvalidCode)
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
                    yield return new TestCaseData(UScript.Cyrillic, "Cyrillic");
                    yield return new TestCaseData(UScript.Deseret, "Deseret");
                    yield return new TestCaseData(UScript.Devanagari, "Devanagari");
                    yield return new TestCaseData(UScript.Ethiopic, "Ethiopic");
                    yield return new TestCaseData(UScript.Georgian, "Georgian");
                    yield return new TestCaseData(UScript.Gothic, "Gothic");
                    yield return new TestCaseData(UScript.Greek, "Greek");
                    yield return new TestCaseData(UScript.Gujarati, "Gujarati");
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
                    yield return new TestCaseData(UScript.Han, "Hani");
                    yield return new TestCaseData(UScript.Hangul, "Hang");
                    yield return new TestCaseData(UScript.Hebrew, "Hebr");
                    yield return new TestCaseData(UScript.Hiragana, "Hira");
                    yield return new TestCaseData(UScript.Kannada, "Knda");
                    yield return new TestCaseData(UScript.Katakana, "Kana");
                    yield return new TestCaseData(UScript.Khmer, "Khmr");
                    yield return new TestCaseData(UScript.Lao, "Laoo");
                    yield return new TestCaseData(UScript.Latin, "Latn");
                    yield return new TestCaseData(UScript.Malayalam, "Mlym");
                    yield return new TestCaseData(UScript.Mongolian, "Mong");
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
                    yield return new TestCaseData(0x0000FF9D, UScript.Katakana);
                    yield return new TestCaseData(0x0000FFBE, UScript.Hangul);
                    yield return new TestCaseData(0x0000FFC7, UScript.Hangul);
                    yield return new TestCaseData(0x0000FFCF, UScript.Hangul);
                    yield return new TestCaseData(0x0000FFD7, UScript.Hangul);
                    yield return new TestCaseData(0x0000FFDC, UScript.Hangul);
                    yield return new TestCaseData(0x00010300, UScript.OldItalic);
                    yield return new TestCaseData(0x00010330, UScript.Gothic);
                    yield return new TestCaseData(0x0001034A, UScript.Gothic);
                    yield return new TestCaseData(0x00010400, UScript.Deseret);
                    yield return new TestCaseData(0x00010428, UScript.Deseret);
                    yield return new TestCaseData(0x0001D167, UScript.Inherited);
                    yield return new TestCaseData(0x0001D17B, UScript.Inherited);
                    yield return new TestCaseData(0x0001D185, UScript.Inherited);
                    yield return new TestCaseData(0x0001D1AA, UScript.Inherited);
                    yield return new TestCaseData(0x00020000, UScript.Han);
                    yield return new TestCaseData(0x00000D02, UScript.Malayalam);
                    yield return new TestCaseData(0x00050005, UScript.Unknown); // new Zzzz value in Unicode 5.0
                    yield return new TestCaseData(0x00000000, UScript.Common);
                    yield return new TestCaseData(0x0001D169, UScript.Inherited);
                    yield return new TestCaseData(0x0001D182, UScript.Inherited);
                    yield return new TestCaseData(0x0001D18B, UScript.Inherited);
                    yield return new TestCaseData(0x0001D1AD, UScript.Inherited);
                }
            }

            [Test, TestCaseSource(typeof(GetScriptTest), "TestData")]
            public void TestGetScript(int codepoint, int expected)
            {

                int code = UScript.InvalidCode;

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
