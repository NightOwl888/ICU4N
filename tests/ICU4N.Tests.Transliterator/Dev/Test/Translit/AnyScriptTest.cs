using ICU4N.Globalization;
using ICU4N.Text;
using NUnit.Framework;
using System;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Translit
{
    /// <author>markdavis</author>
    public class AnyScriptTest : TestFmwk
    {
        [Test]
        public void TestContext()
        {
            Transliterator t = Transliterator.CreateFromRules("foo", "::[bc]; a{b}d > B;", Transliterator.Forward);
            String sample = "abd abc b";
            assertEquals("context works", "aBd abc b", t.Transform(sample));
        }

        [Test]
        public void TestScripts()
        {
            // get a couple of characters of each script for testing

            StringBuffer testBuffer = new StringBuffer();
            for (int script = 0; script < UScript.CodeLimit; ++script)
            {
                UnicodeSet test = new UnicodeSet().ApplyPropertyAlias("script", UScript.GetName(script));
                int count = Math.Min(20, test.Count);
                for (int i = 0; i < count; ++i)
                {
                    testBuffer.Append(UTF16.ValueOf(test[i]));
                }
            }
            {
                String test = testBuffer.ToString();
                Logln("Test line: " + test);

                int inclusion = TestFmwk.GetExhaustiveness();
                bool testedUnavailableScript = false;

                for (int script = 0; script < UScript.CodeLimit; ++script)
                {
                    if (script == UScript.Common || script == UScript.Inherited)
                    {
                        continue;
                    }
                    // if the inclusion rate is not 10, skip all but a small number of items.
                    // Make sure, however, that we test at least one unavailable script
                    if (inclusion < 10 && script != UScript.Latin
                            && script != UScript.Han
                            && script != UScript.Hiragana
                            && testedUnavailableScript
                            )
                    {
                        continue;
                    }

                    String scriptName = UScript.GetName(script);  // long name
                    UCultureInfo locale = new UCultureInfo(scriptName);
                    if (locale.Language.Equals("new") || locale.Language.Equals("pau"))
                    {
                        if (logKnownIssue("11171",
                                "long script name loosely looks like a locale ID with a known likely script"))
                        {
                            continue;
                        }
                    }
                    Transliterator t;
                    try
                    {
                        t = Transliterator.GetInstance("any-" + scriptName);
                    }
                    catch (Exception e)
                    {
                        testedUnavailableScript = true;
                        Logln("Skipping unavailable: " + scriptName);
                        continue; // we don't handle all scripts
                    }
                    Logln("Checking: " + scriptName);
                    if (t != null)
                    {
                        t.Transform(test); // just verify we don't crash
                    }
                    String shortScriptName = UScript.GetShortName(script);  // 4-letter script code
                    try
                    {
                        t = Transliterator.GetInstance("any-" + shortScriptName);
                    }
                    catch (Exception e)
                    {
                        Errln("Transliterator.GetInstance() worked for \"any-" + scriptName +
                                "\" but not for \"any-" + shortScriptName + '\"');
                    }
                    t.Transform(test); // just verify we don't crash
                }
            }
        }

        [Test]
        public void TestScriptsUsingTry()
        {
            // get a couple of characters of each script for testing

            StringBuffer testBuffer = new StringBuffer();
            for (int script = 0; script < UScript.CodeLimit; ++script)
            {
                if (UScript.TryGetName(script, out string name))
                {
                    UnicodeSet test = new UnicodeSet().ApplyPropertyAlias("script", name);
                    int count = Math.Min(20, test.Count);
                    for (int i = 0; i < count; ++i)
                    {
                        testBuffer.Append(UTF16.ValueOf(test[i]));
                    }
                }
            }
            {
                string test = testBuffer.ToString();
                Logln("Test line: " + test);

                int inclusion = TestFmwk.GetExhaustiveness();
                bool testedUnavailableScript = false;

                for (int script = 0; script < UScript.CodeLimit; ++script)
                {
                    if (script == UScript.Common || script == UScript.Inherited)
                    {
                        continue;
                    }
                    // if the inclusion rate is not 10, skip all but a small number of items.
                    // Make sure, however, that we test at least one unavailable script
                    if (inclusion < 10 && script != UScript.Latin
                            && script != UScript.Han
                            && script != UScript.Hiragana
                            && testedUnavailableScript
                            )
                    {
                        continue;
                    }

                    if (UScript.TryGetName(script, out string scriptName))
                    {
                        UCultureInfo locale = new UCultureInfo(scriptName);
                        if (locale.Language.Equals("new") || locale.Language.Equals("pau"))
                        {
                            if (logKnownIssue("11171",
                                    "long script name loosely looks like a locale ID with a known likely script"))
                            {
                                continue;
                            }
                        }
                    }
                    Transliterator t;
                    try
                    {
                        t = Transliterator.GetInstance("any-" + scriptName);
                    }
                    catch (Exception e)
                    {
                        testedUnavailableScript = true;
                        Logln("Skipping unavailable: " + scriptName);
                        continue; // we don't handle all scripts
                    }
                    Logln("Checking: " + scriptName);
                    if (t != null)
                    {
                        t.Transform(test); // just verify we don't crash
                    }

                    string shortScriptName = "";
                    if (UScript.TryGetName(script, out scriptName))  // 4-letter script code
                    {
                        try
                        {
                            t = Transliterator.GetInstance("any-" + shortScriptName);
                        }
                        catch (Exception e)
                        {
                            Errln("Transliterator.GetInstance() worked for \"any-" + scriptName +
                                    "\" but not for \"any-" + shortScriptName + '\"');
                        }
                    }
                    t.Transform(test); // just verify we don't crash
                }
            }
        }

        /**
         * Check to make sure that wide characters are converted when going to narrow scripts.
         */
        [Test]
        public void TestForWidth()
        {
            Transliterator widen = Transliterator.GetInstance("halfwidth-fullwidth");
            Transliterator narrow = Transliterator.GetInstance("fullwidth-halfwidth");
            UnicodeSet ASCII = new UnicodeSet("[:ascii:]");
            string lettersAndSpace = "abc def";
            string punctOnly = "( )";

            String wideLettersAndSpace = widen.Transform(lettersAndSpace);
            String widePunctOnly = widen.Transform(punctOnly);
            assertContainsNone("Should be wide", ASCII, wideLettersAndSpace);
            assertContainsNone("Should be wide", ASCII, widePunctOnly);

            String back;
            back = narrow.Transform(wideLettersAndSpace);
            assertEquals("Should be narrow", lettersAndSpace, back);
            back = narrow.Transform(widePunctOnly);
            assertEquals("Should be narrow", punctOnly, back);

            Transliterator latin = Transliterator.GetInstance("any-Latn");
            back = latin.Transform(wideLettersAndSpace);
            assertEquals("Should be ascii", lettersAndSpace, back);

            back = latin.Transform(widePunctOnly);
            assertEquals("Should be ascii", punctOnly, back);

            // Han-Latin is now forward-only per CLDR ticket #5630
            //Transliterator t2 = Transliterator.GetInstance("any-Han");
            //back = t2.transform(widePunctOnly);
            //assertEquals("Should be same", widePunctOnly, back);


        }

        [Test]
        public void TestCommonDigits()
        {
            UnicodeSet westernDigitSet = new UnicodeSet("[0-9]");
            UnicodeSet westernDigitSetAndMarks = new UnicodeSet("[[0-9][:Mn:]]");
            UnicodeSet arabicDigitSet = new UnicodeSet("[[:Nd:]&[:block=Arabic:]]");
            Transliterator latin = Transliterator.GetInstance("Any-Latn");
            Transliterator arabic = Transliterator.GetInstance("Any-Arabic");
            String westernDigits = getList(westernDigitSet);
            String arabicDigits = getList(arabicDigitSet);

            String fromArabic = latin.Transform(arabicDigits);
            assertContainsAll("Any-Latin transforms Arabic digits", westernDigitSetAndMarks, fromArabic);
            if (false)
            { // we don't require conversion to Arabic digits
#pragma warning disable CS0162 // Unreachable code detected
                String fromLatin = arabic.Transform(westernDigits);
#pragma warning restore CS0162 // Unreachable code detected
                assertContainsAll("Any-Arabic transforms Western digits", arabicDigitSet, fromLatin);
            }
        }

        // might want to add to TestFmwk
        private void assertContainsAll(String message, UnicodeSet set, String str)
        {
            handleAssert(set.ContainsAll(str), message, set, str, "contains all of", false);
        }

        private void assertContainsNone(String message, UnicodeSet set, String str)
        {
            handleAssert(set.ContainsNone(str), message, set, str, "contains none of", false);
        }

        // might want to add to UnicodeSet
        private String getList(UnicodeSet set)
        {
            StringBuffer result = new StringBuffer();
            for (UnicodeSetIterator it = new UnicodeSetIterator(set); it.Next();)
            {
                result.Append(it.GetString());
            }
            return result.ToString();
        }
    }
}
