using ICU4N.Impl;
using ICU4N.Lang;
using ICU4N.Support;
using ICU4N.Support.Globalization;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Lang
{
    /// <summary>
    /// Testing character casing
    /// <para/>
    /// Mostly following the test cases in strcase.cpp for ICU
    /// </summary>
    /// <author>Syn Wee Quek</author>
    /// <since>march 14 2002</since>
    public sealed class UCharacterCaseTest : TestFmwk
    {
        // constructor -----------------------------------------------------------

        /**
         * Constructor
         */
        public UCharacterCaseTest()
        {
        }

        // public methods --------------------------------------------------------

        /**
         * Testing the uppercase and lowercase function of UCharacter
         */
        [Test]
        public void TestCharacter()
        {
            for (int i = 0; i < CHARACTER_LOWER_.Length; i++)
            {
                if (UCharacter.IsLetter(CHARACTER_LOWER_[i]) &&
                    !UCharacter.IsLowerCase(CHARACTER_LOWER_[i]))
                {
                    Errln("FAIL isLowerCase test for \\u" +
                          Hex(CHARACTER_LOWER_[i]));
                    break;
                }
                if (UCharacter.IsLetter(CHARACTER_UPPER_[i]) &&
                    !(UCharacter.IsUpperCase(CHARACTER_UPPER_[i]) ||
                      UCharacter.IsTitleCase(CHARACTER_UPPER_[i])))
                {
                    Errln("FAIL isUpperCase test for \\u" +
                          Hex(CHARACTER_UPPER_[i]));
                    break;
                }
                if (CHARACTER_LOWER_[i] !=
                    UCharacter.ToLower(CHARACTER_UPPER_[i]) ||
                    (CHARACTER_UPPER_[i] !=
                    UCharacter.ToUpper(CHARACTER_LOWER_[i]) &&
                    CHARACTER_UPPER_[i] !=
                    UCharacter.ToTitleCase(CHARACTER_LOWER_[i])))
                {
                    Errln("FAIL case conversion test for \\u" +
                          Hex(CHARACTER_UPPER_[i]) +
                          " to \\u" + Hex(CHARACTER_LOWER_[i]));
                    break;
                }
                if (CHARACTER_LOWER_[i] !=
                    UCharacter.ToLower(CHARACTER_LOWER_[i]))
                {
                    Errln("FAIL lower case conversion test for \\u" +
                          Hex(CHARACTER_LOWER_[i]));
                    break;
                }
                if (CHARACTER_UPPER_[i] !=
                    UCharacter.ToUpper(CHARACTER_UPPER_[i]) &&
                    CHARACTER_UPPER_[i] !=
                    UCharacter.ToTitleCase(CHARACTER_UPPER_[i]))
                {
                    Errln("FAIL upper case conversion test for \\u" +
                          Hex(CHARACTER_UPPER_[i]));
                    break;
                }
                Logln("Ok    \\u" + Hex(CHARACTER_UPPER_[i]) + " and \\u" +
                      Hex(CHARACTER_LOWER_[i]));
            }
        }

        [Test]
        public void TestFolding()
        {
            // test simple case folding
            for (int i = 0; i < FOLDING_SIMPLE_.Length; i += 3)
            {
                if (UCharacter.FoldCase(FOLDING_SIMPLE_[i], true) !=
                    FOLDING_SIMPLE_[i + 1])
                {
                    Errln("FAIL: foldCase(\\u" + Hex(FOLDING_SIMPLE_[i]) +
                          ", true) should be \\u" + Hex(FOLDING_SIMPLE_[i + 1]));
                }
                if (UCharacter.FoldCase(FOLDING_SIMPLE_[i],
                                        FoldCase.Default) !=
                                        FOLDING_SIMPLE_[i + 1])
                {
                    Errln("FAIL: foldCase(\\u" + Hex(FOLDING_SIMPLE_[i]) +
                          ", UCharacter.FOLD_CASE_DEFAULT) should be \\u"
                          + Hex(FOLDING_SIMPLE_[i + 1]));
                }
                if (UCharacter.FoldCase(FOLDING_SIMPLE_[i], false) !=
                    FOLDING_SIMPLE_[i + 2])
                {
                    Errln("FAIL: foldCase(\\u" + Hex(FOLDING_SIMPLE_[i]) +
                          ", false) should be \\u" + Hex(FOLDING_SIMPLE_[i + 2]));
                }
                if (UCharacter.FoldCase(FOLDING_SIMPLE_[i],
                                        FoldCase.ExcludeSpecialI) !=
                                        FOLDING_SIMPLE_[i + 2])
                {
                    Errln("FAIL: foldCase(\\u" + Hex(FOLDING_SIMPLE_[i]) +
                          ", UCharacter.FOLD_CASE_EXCLUDE_SPECIAL_I) should be \\u"
                          + Hex(FOLDING_SIMPLE_[i + 2]));
                }
            }

            // Test full string case folding with default option and separate
            // buffers
            if (!FOLDING_DEFAULT_[0].Equals(UCharacter.FoldCase(FOLDING_MIXED_[0], true)))
            {
                Errln("FAIL: foldCase(" + Prettify(FOLDING_MIXED_[0]) +
                      ", true)=" + Prettify(UCharacter.FoldCase(FOLDING_MIXED_[0], true)) +
                      " should be " + Prettify(FOLDING_DEFAULT_[0]));
            }

            if (!FOLDING_DEFAULT_[0].Equals(UCharacter.FoldCase(FOLDING_MIXED_[0], FoldCase.Default)))
            {
                Errln("FAIL: foldCase(" + Prettify(FOLDING_MIXED_[0]) +
                      ", UCharacter.FOLD_CASE_DEFAULT)=" + Prettify(UCharacter.FoldCase(FOLDING_MIXED_[0], FoldCase.Default))
                      + " should be " + Prettify(FOLDING_DEFAULT_[0]));
            }

            if (!FOLDING_EXCLUDE_SPECIAL_I_[0].Equals(
                                UCharacter.FoldCase(FOLDING_MIXED_[0], false)))
            {
                Errln("FAIL: foldCase(" + Prettify(FOLDING_MIXED_[0]) +
                      ", false)=" + Prettify(UCharacter.FoldCase(FOLDING_MIXED_[0], false))
                      + " should be " + Prettify(FOLDING_EXCLUDE_SPECIAL_I_[0]));
            }

            if (!FOLDING_EXCLUDE_SPECIAL_I_[0].Equals(
                                        UCharacter.FoldCase(FOLDING_MIXED_[0], FoldCase.ExcludeSpecialI)))
            {
                Errln("FAIL: foldCase(" + Prettify(FOLDING_MIXED_[0]) +
                      ", UCharacter.FOLD_CASE_EXCLUDE_SPECIAL_I)=" + Prettify(UCharacter.FoldCase(FOLDING_MIXED_[0], FoldCase.ExcludeSpecialI))
                      + " should be " + Prettify(FOLDING_EXCLUDE_SPECIAL_I_[0]));
            }

            if (!FOLDING_DEFAULT_[1].Equals(UCharacter.FoldCase(FOLDING_MIXED_[1], true)))
            {
                Errln("FAIL: foldCase(" + Prettify(FOLDING_MIXED_[1]) +
                      ", true)=" + Prettify(UCharacter.FoldCase(FOLDING_MIXED_[1], true))
                      + " should be " + Prettify(FOLDING_DEFAULT_[1]));
            }

            if (!FOLDING_DEFAULT_[1].Equals(UCharacter.FoldCase(FOLDING_MIXED_[1], FoldCase.Default)))
            {
                Errln("FAIL: foldCase(" + Prettify(FOLDING_MIXED_[1]) +
                             ", UCharacter.FOLD_CASE_DEFAULT)=" + Prettify(UCharacter.FoldCase(FOLDING_MIXED_[1], FoldCase.Default))
                             + " should be " + Prettify(FOLDING_DEFAULT_[1]));
            }

            // alternate handling for dotted I/dotless i (U+0130, U+0131)
            if (!FOLDING_EXCLUDE_SPECIAL_I_[1].Equals(
                            UCharacter.FoldCase(FOLDING_MIXED_[1], false)))
            {
                Errln("FAIL: foldCase(" + Prettify(FOLDING_MIXED_[1]) +
                      ", false)=" + Prettify(UCharacter.FoldCase(FOLDING_MIXED_[1], false))
                      + " should be " + Prettify(FOLDING_EXCLUDE_SPECIAL_I_[1]));
            }

            if (!FOLDING_EXCLUDE_SPECIAL_I_[1].Equals(
                                    UCharacter.FoldCase(FOLDING_MIXED_[1], FoldCase.ExcludeSpecialI)))
            {
                Errln("FAIL: foldCase(" + Prettify(FOLDING_MIXED_[1]) +
                      ", UCharacter.FOLD_CASE_EXCLUDE_SPECIAL_I)=" + Prettify(UCharacter.FoldCase(FOLDING_MIXED_[1], FoldCase.ExcludeSpecialI))
                      + " should be "
                      + Prettify(FOLDING_EXCLUDE_SPECIAL_I_[1]));
            }
        }

        /**
         * Testing the strings case mapping methods
         */
        [Test]
        public void TestUpper()
        {
            // uppercase with root locale and in the same buffer
            if (!UPPER_ROOT_.Equals(UCharacter.ToUpper(UPPER_BEFORE_)))
            {
                Errln("Fail " + UPPER_BEFORE_ + " after uppercase should be " +
                      UPPER_ROOT_ + " instead got " +
                      UCharacter.ToUpper(UPPER_BEFORE_));
            }

            // uppercase with turkish locale and separate buffers
            if (!UPPER_TURKISH_.Equals(UCharacter.ToUpper(TURKISH_LOCALE_,
                                                             UPPER_BEFORE_)))
            {
                Errln("Fail " + UPPER_BEFORE_ +
                      " after turkish-sensitive uppercase should be " +
                      UPPER_TURKISH_ + " instead of " +
                      UCharacter.ToUpper(TURKISH_LOCALE_, UPPER_BEFORE_));
            }

            // uppercase a short string with root locale
            if (!UPPER_MINI_UPPER_.Equals(UCharacter.ToUpper(UPPER_MINI_)))
            {
                Errln("error in toUpper(root locale)=\"" + UPPER_MINI_ +
                      "\" expected \"" + UPPER_MINI_UPPER_ + "\"");
            }

            if (!SHARED_UPPERCASE_TOPKAP_.Equals(
                           UCharacter.ToUpper(SHARED_LOWERCASE_TOPKAP_)))
            {
                Errln("toUpper failed: expected \"" +
                      SHARED_UPPERCASE_TOPKAP_ + "\", got \"" +
                      UCharacter.ToUpper(SHARED_LOWERCASE_TOPKAP_) + "\".");
            }

            if (!SHARED_UPPERCASE_TURKISH_.Equals(
                      UCharacter.ToUpper(TURKISH_LOCALE_,
                                             SHARED_LOWERCASE_TOPKAP_)))
            {
                Errln("toUpper failed: expected \"" +
                      SHARED_UPPERCASE_TURKISH_ + "\", got \"" +
                      UCharacter.ToUpper(TURKISH_LOCALE_,
                                         SHARED_LOWERCASE_TOPKAP_) + "\".");
            }

            if (!SHARED_UPPERCASE_GERMAN_.Equals(
                    UCharacter.ToUpper(GERMAN_LOCALE_,
                                           SHARED_LOWERCASE_GERMAN_)))
            {
                Errln("toUpper failed: expected \"" + SHARED_UPPERCASE_GERMAN_
                      + "\", got \"" + UCharacter.ToUpper(GERMAN_LOCALE_,
                                            SHARED_LOWERCASE_GERMAN_) + "\".");
            }

            if (!SHARED_UPPERCASE_GREEK_.Equals(
                    UCharacter.ToUpper(SHARED_LOWERCASE_GREEK_)))
            {
                Errln("toLower failed: expected \"" + SHARED_UPPERCASE_GREEK_ +
                      "\", got \"" + UCharacter.ToUpper(
                                            SHARED_LOWERCASE_GREEK_) + "\".");
            }
        }

        [Test]
        public void TestLower()
        {
            if (!LOWER_ROOT_.Equals(UCharacter.ToLower(LOWER_BEFORE_)))
            {
                Errln("Fail " + LOWER_BEFORE_ + " after lowercase should be " +
                      LOWER_ROOT_ + " instead of " +
                      UCharacter.ToLower(LOWER_BEFORE_));
            }

            // lowercase with turkish locale
            if (!LOWER_TURKISH_.Equals(UCharacter.ToLower(TURKISH_LOCALE_,
                                                              LOWER_BEFORE_)))
            {
                Errln("Fail " + LOWER_BEFORE_ +
                      " after turkish-sensitive lowercase should be " +
                      LOWER_TURKISH_ + " instead of " +
                      UCharacter.ToLower(TURKISH_LOCALE_, LOWER_BEFORE_));
            }
            if (!SHARED_LOWERCASE_ISTANBUL_.Equals(
                         UCharacter.ToLower(SHARED_UPPERCASE_ISTANBUL_)))
            {
                Errln("1. toLower failed: expected \"" +
                      SHARED_LOWERCASE_ISTANBUL_ + "\", got \"" +
                  UCharacter.ToLower(SHARED_UPPERCASE_ISTANBUL_) + "\".");
            }

            if (!SHARED_LOWERCASE_TURKISH_.Equals(
                    UCharacter.ToLower(TURKISH_LOCALE_,
                                           SHARED_UPPERCASE_ISTANBUL_)))
            {
                Errln("2. toLower failed: expected \"" +
                      SHARED_LOWERCASE_TURKISH_ + "\", got \"" +
                      UCharacter.ToLower(TURKISH_LOCALE_,
                                    SHARED_UPPERCASE_ISTANBUL_) + "\".");
            }
            if (!SHARED_LOWERCASE_GREEK_.Equals(
                    UCharacter.ToLower(GREEK_LOCALE_,
                                           SHARED_UPPERCASE_GREEK_)))
            {
                Errln("toLower failed: expected \"" + SHARED_LOWERCASE_GREEK_ +
                      "\", got \"" + UCharacter.ToLower(GREEK_LOCALE_,
                                            SHARED_UPPERCASE_GREEK_) + "\".");
            }
        }

        [Test]
        public void TestTitleRegression()
        {
            bool isIgnorable = UCharacter.HasBinaryProperty('\'', UProperty.Case_Ignorable);
            assertTrue("Case Ignorable check of ASCII apostrophe", isIgnorable);
            assertEquals("Titlecase check",
                    "The Quick Brown Fox Can't Jump Over The Lazy Dogs.",
                    UCharacter.ToTitleCase(ULocale.ENGLISH, "THE QUICK BROWN FOX CAN'T JUMP OVER THE LAZY DOGS.", null));
        }

        [Test]
        public void TestTitle()
        {
            try
            {
                for (int i = 0; i < TITLE_DATA_.Length;)
                {
                    String test = TITLE_DATA_[i++];
                    String expected = TITLE_DATA_[i++];
                    ULocale locale = new ULocale(TITLE_DATA_[i++]);
                    int breakType = int.Parse(TITLE_DATA_[i++], CultureInfo.InvariantCulture);
                    String optionsString = TITLE_DATA_[i++];
                    BreakIterator iter =
                        breakType >= 0 ?
                            BreakIterator.GetBreakInstance(locale, breakType) :
                            breakType == -2 ?
                                // Open a trivial break iterator that only delivers { 0, length }
                                // or even just { 0 } as boundaries.
                                new RuleBasedBreakIterator(".*;") :
                                null;
                    int options = 0;
                    if (optionsString.IndexOf('L') >= 0)
                    {
                        options |= UCharacter.TITLECASE_NO_LOWERCASE;
                    }
                    if (optionsString.IndexOf('A') >= 0)
                    {
                        options |= UCharacter.TITLECASE_NO_BREAK_ADJUSTMENT;
                    }
                    String result = UCharacter.ToTitleCase(locale, test, iter, options);
                    if (!expected.Equals(result))
                    {
                        Errln("titlecasing for " + Prettify(test) + " (options " + options + ") should be " +
                              Prettify(expected) + " but got " +
                              Prettify(result));
                    }
                    if (options == 0)
                    {
                        result = UCharacter.ToTitleCase(locale, test, iter);
                        if (!expected.Equals(result))
                        {
                            Errln("titlecasing for " + Prettify(test) + " should be " +
                                  Prettify(expected) + " but got " +
                                  Prettify(result));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Warnln("Could not find data for BreakIterators");
            }
        }

        // Not a [Test]. See ICU4C intltest strcase.cpp TestCasingImpl().
        void TestCasingImpl(String input, String output, CaseMap.Title toTitle, CultureInfo locale)
        {
            String result = toTitle.Apply(locale, null, input, new StringBuilder(), null).ToString();
            assertEquals("toTitle(" + input + ')', output, result);
        }

        [Test]
        public void TestTitleOptions()
        {
            CultureInfo root = CultureInfo.InvariantCulture;
            // New options in ICU 60.
            TestCasingImpl("ʻcAt! ʻeTc.", "ʻCat! ʻetc.",
                    CaseMap.ToTitle().WholeString(), root);
            TestCasingImpl("a ʻCaT. A ʻdOg! ʻeTc.", "A ʻCaT. A ʻdOg! ʻETc.",
                    CaseMap.ToTitle().Sentences().NoLowercase(), root);
            TestCasingImpl("49eRs", "49ers",
                    CaseMap.ToTitle().WholeString(), root);
            TestCasingImpl("«丰(aBc)»", "«丰(abc)»",
                    CaseMap.ToTitle().WholeString(), root);
            TestCasingImpl("49eRs", "49Ers",
                    CaseMap.ToTitle().WholeString().AdjustToCased(), root);
            TestCasingImpl("«丰(aBc)»", "«丰(Abc)»",
                    CaseMap.ToTitle().WholeString().AdjustToCased(), root);
            TestCasingImpl(" john. Smith", " John. Smith",
                    CaseMap.ToTitle().WholeString().NoLowercase(), root);
            TestCasingImpl(" john. Smith", " john. smith",
                    CaseMap.ToTitle().WholeString().NoBreakAdjustment(), root);
            TestCasingImpl("«ijs»", "«IJs»",
                    CaseMap.ToTitle().WholeString(), new CultureInfo("nl-BE"));
            TestCasingImpl("«ijs»", "«İjs»",
                    CaseMap.ToTitle().WholeString(), new CultureInfo("tr-DE"));

            // Test conflicting settings.
            // If & when we add more options, then the ORed combinations may become
            // indistinguishable from valid values.
            try
            {
                CaseMap.ToTitle().NoBreakAdjustment().AdjustToCased().
                        Apply(root, null, "", new StringBuilder(), null);
                fail("CaseMap.toTitle(multiple adjustment options) " +
                        "did not throw an ArgumentException");
            }
            catch (ArgumentException expected)
            {
            }
            try
            {
                CaseMap.ToTitle().WholeString().Sentences().
                        Apply(root, null, "", new StringBuilder(), null);
                fail("CaseMap.toTitle(multiple iterator options) " +
                        "did not throw an ArgumentException");
            }
            catch (ArgumentException expected)
            {
            }
            BreakIterator iter = BreakIterator.GetCharacterInstance(root);
            try
            {
                CaseMap.ToTitle().WholeString().Apply(root, iter, "", new StringBuilder(), null);
                fail("CaseMap.toTitle(iterator option + iterator) " +
                        "did not throw an ArgumentException");
            }
            catch (ArgumentException expected)
            {
            }
        }

        [Test]
        public void TestDutchTitle()
        {
            ULocale LOC_DUTCH = new ULocale("nl");
            int options = 0;
            options |= UCharacter.TITLECASE_NO_LOWERCASE;
            BreakIterator iter = BreakIterator.GetWordInstance(LOC_DUTCH);

            assertEquals("Dutch titlecase check in English",
                    "Ijssel Igloo Ijmuiden",
                    UCharacter.ToTitleCase(ULocale.ENGLISH, "ijssel igloo IJMUIDEN", null));

            assertEquals("Dutch titlecase check in Dutch",
                    "IJssel Igloo IJmuiden",
                    UCharacter.ToTitleCase(LOC_DUTCH, "ijssel igloo IJMUIDEN", null));

            // Also check the behavior using Java Locale
            assertEquals("Dutch titlecase check in English (Java Locale)",
                    "Ijssel Igloo Ijmuiden",
                    UCharacter.ToTitleCase(new CultureInfo("en") /* Locale.ENGLISH */, "ijssel igloo IJMUIDEN", null));

            assertEquals("Dutch titlecase check in Dutch (Java Locale)",
                    "IJssel Igloo IJmuiden",
                    UCharacter.ToTitleCase(DUTCH_LOCALE_, "ijssel igloo IJMUIDEN", null));

            iter.SetText("ijssel igloo IjMUIdEN iPoD ijenough");
            assertEquals("Dutch titlecase check in Dutch with nolowercase option",
                    "IJssel Igloo IJMUIdEN IPoD IJenough",
                    UCharacter.ToTitleCase(LOC_DUTCH, "ijssel igloo IjMUIdEN iPoD ijenough", iter, options));
        }

        [Test]
        public void TestSpecial()
        {
            for (int i = 0; i < SPECIAL_LOCALES_.Length; i++)
            {
                int j = i * 3;
                CultureInfo locale = SPECIAL_LOCALES_[i];
                String str = SPECIAL_DATA_[j];
                if (locale != null)
                {
                    if (!SPECIAL_DATA_[j + 1].Equals(
                         UCharacter.ToLower(locale, str)))
                    {
                        Errln("error lowercasing special characters " +
                            Hex(str) + " expected " + Hex(SPECIAL_DATA_[j + 1])
                            + " for locale " + locale.ToString() + " but got " +
                            Hex(UCharacter.ToLower(locale, str)));
                    }
                    if (!SPECIAL_DATA_[j + 2].Equals(
                         UCharacter.ToUpper(locale, str)))
                    {
                        Errln("error uppercasing special characters " +
                            Hex(str) + " expected " + SPECIAL_DATA_[j + 2]
                            + " for locale " + locale.ToString() + " but got " +
                            Hex(UCharacter.ToUpper(locale, str)));
                    }
                }
                else
                {
                    if (!SPECIAL_DATA_[j + 1].Equals(
                         UCharacter.ToLower(str)))
                    {
                        Errln("error lowercasing special characters " +
                            Hex(str) + " expected " + SPECIAL_DATA_[j + 1] +
                            " but got " +
                            Hex(UCharacter.ToLower(locale, str)));
                    }
                    if (!SPECIAL_DATA_[j + 2].Equals(
                         UCharacter.ToUpper(locale, str)))
                    {
                        Errln("error uppercasing special characters " +
                            Hex(str) + " expected " + SPECIAL_DATA_[j + 2] +
                            " but got " +
                            Hex(UCharacter.ToUpper(locale, str)));
                    }
                }
            }

            // turkish & azerbaijani dotless i & dotted I
            // remove dot above if there was a capital I before and there are no
            // more accents above
            if (!SPECIAL_DOTTED_LOWER_TURKISH_.Equals(UCharacter.ToLower(
                                            TURKISH_LOCALE_, SPECIAL_DOTTED_)))
            {
                Errln("error in dots.toLower(tr)=\"" + SPECIAL_DOTTED_ +
                      "\" expected \"" + SPECIAL_DOTTED_LOWER_TURKISH_ +
                      "\" but got " + UCharacter.ToLower(TURKISH_LOCALE_,
                                                             SPECIAL_DOTTED_));
            }
            if (!SPECIAL_DOTTED_LOWER_GERMAN_.Equals(UCharacter.ToLower(
                                                 GERMAN_LOCALE_, SPECIAL_DOTTED_)))
            {
                Errln("error in dots.toLower(de)=\"" + SPECIAL_DOTTED_ +
                      "\" expected \"" + SPECIAL_DOTTED_LOWER_GERMAN_ +
                      "\" but got " + UCharacter.ToLower(GERMAN_LOCALE_,
                                                             SPECIAL_DOTTED_));
            }

            // lithuanian dot above in uppercasing
            if (!SPECIAL_DOT_ABOVE_UPPER_LITHUANIAN_.Equals(
                 UCharacter.ToUpper(LITHUANIAN_LOCALE_, SPECIAL_DOT_ABOVE_)))
            {
                Errln("error in dots.toUpper(lt)=\"" + SPECIAL_DOT_ABOVE_ +
                      "\" expected \"" + SPECIAL_DOT_ABOVE_UPPER_LITHUANIAN_ +
                      "\" but got " + UCharacter.ToLower(LITHUANIAN_LOCALE_,
                                                             SPECIAL_DOT_ABOVE_));
            }
            if (!SPECIAL_DOT_ABOVE_UPPER_GERMAN_.Equals(UCharacter.ToUpper(
                                            GERMAN_LOCALE_, SPECIAL_DOT_ABOVE_)))
            {
                Errln("error in dots.toUpper(de)=\"" + SPECIAL_DOT_ABOVE_ +
                      "\" expected \"" + SPECIAL_DOT_ABOVE_UPPER_GERMAN_ +
                      "\" but got " + UCharacter.ToLower(GERMAN_LOCALE_,
                                                             SPECIAL_DOT_ABOVE_));
            }

            // lithuanian adds dot above to i in lowercasing if there are more
            // above accents
            if (!SPECIAL_DOT_ABOVE_LOWER_LITHUANIAN_.Equals(
                UCharacter.ToLower(LITHUANIAN_LOCALE_,
                                       SPECIAL_DOT_ABOVE_UPPER_)))
            {
                Errln("error in dots.toLower(lt)=\"" + SPECIAL_DOT_ABOVE_UPPER_ +
                      "\" expected \"" + SPECIAL_DOT_ABOVE_LOWER_LITHUANIAN_ +
                      "\" but got " + UCharacter.ToLower(LITHUANIAN_LOCALE_,
                                                       SPECIAL_DOT_ABOVE_UPPER_));
            }
            if (!SPECIAL_DOT_ABOVE_LOWER_GERMAN_.Equals(
                UCharacter.ToLower(GERMAN_LOCALE_,
                                       SPECIAL_DOT_ABOVE_UPPER_)))
            {
                Errln("error in dots.toLower(de)=\"" + SPECIAL_DOT_ABOVE_UPPER_ +
                      "\" expected \"" + SPECIAL_DOT_ABOVE_LOWER_GERMAN_ +
                      "\" but got " + UCharacter.ToLower(GERMAN_LOCALE_,
                                                       SPECIAL_DOT_ABOVE_UPPER_));
            }
        }

        /**
         * Tests for case mapping in the file SpecialCasing.txt
         * This method reads in SpecialCasing.txt file for testing purposes.
         * A default path is provided relative to the src path, however the user
         * could set a system property to change the directory path.<br>
         * e.g. java -DUnicodeData="data_dir_path" com.ibm.dev.test.lang.UCharacterTest
         */
        [Test]
        public void TestSpecialCasingTxt()
        {
            try
            {
                // reading in the SpecialCasing file
                TextReader input = TestUtil.GetDataReader(
                                                      "unicode/SpecialCasing.txt");
                while (true)
                {
                    String s = input.ReadLine();
                    if (s == null)
                    {
                        break;
                    }
                    if (s.Length == 0 || s[0] == '#')
                    {
                        continue;
                    }

                    String[] chstr = GetUnicodeStrings(s);
                    StringBuffer strbuffer = new StringBuffer(chstr[0]);
                    StringBuffer lowerbuffer = new StringBuffer(chstr[1]);
                    StringBuffer upperbuffer = new StringBuffer(chstr[3]);
                    CultureInfo locale = null;
                    for (int i = 4; i < chstr.Length; i++)
                    {
                        String condition = chstr[i];
                        if (char.IsLower(chstr[i][0]))
                        {
                            // specified locale
                            locale = new CultureInfo(chstr[i]);
                        }
                        else if (condition.CompareToOrdinalIgnoreCase("Not_Before_Dot")
                                                          == 0)
                        {
                            // turns I into dotless i
                        }
                        else if (condition.CompareToOrdinalIgnoreCase(
                                                          "More_Above") == 0)
                        {
                            strbuffer.Append((char)0x300);
                            lowerbuffer.Append((char)0x300);
                            upperbuffer.Append((char)0x300);
                        }
                        else if (condition.CompareToOrdinalIgnoreCase(
                                                    "After_Soft_Dotted") == 0)
                        {
                            strbuffer.Insert(0, 'i');
                            lowerbuffer.Insert(0, 'i');
                            String lang = "";
                            if (locale != null)
                            {
                                lang = locale.GetLanguage();
                            }
                            if (lang.Equals("tr") || lang.Equals("az"))
                            {
                                // this is to be removed when 4.0 data comes out
                                // and upperbuffer.insert uncommented
                                // see jitterbug 2344
                                chstr[i] = "After_I";
                                strbuffer.Remove(0, 1);
                                lowerbuffer.Remove(0, 1);
                                i--;
                                continue;
                                // upperbuffer.insert(0, '\u0130');
                            }
                            else
                            {
                                upperbuffer.Insert(0, 'I');
                            }
                        }
                        else if (condition.CompareToOrdinalIgnoreCase(
                                                          "Final_Sigma") == 0)
                        {
                            strbuffer.Insert(0, 'c');
                            lowerbuffer.Insert(0, 'c');
                            upperbuffer.Insert(0, 'C');
                        }
                        else if (condition.CompareToOrdinalIgnoreCase("After_I") == 0)
                        {
                            strbuffer.Insert(0, 'I');
                            lowerbuffer.Insert(0, 'i');
                            String lang = "";
                            if (locale != null)
                            {
                                lang = locale.GetLanguage();
                            }
                            if (lang.Equals("tr") || lang.Equals("az"))
                            {
                                upperbuffer.Insert(0, 'I');
                            }
                        }
                    }
                    chstr[0] = strbuffer.ToString();
                    chstr[1] = lowerbuffer.ToString();
                    chstr[3] = upperbuffer.ToString();
                    if (locale == null)
                    {
                        if (!UCharacter.ToLower(chstr[0]).Equals(chstr[1]))
                        {
                            Errln(s);
                            Errln("Fail: toLowerCase for character " +
                                  Utility.Escape(chstr[0]) + ", expected "
                                  + Utility.Escape(chstr[1]) + " but resulted in " +
                                  Utility.Escape(UCharacter.ToLower(chstr[0])));
                        }
                        if (!UCharacter.ToUpper(chstr[0]).Equals(chstr[3]))
                        {
                            Errln(s);
                            Errln("Fail: toUpperCase for character " +
                                  Utility.Escape(chstr[0]) + ", expected "
                                  + Utility.Escape(chstr[3]) + " but resulted in " +
                                  Utility.Escape(UCharacter.ToUpper(chstr[0])));
                        }
                    }
                    else
                    {
                        if (!UCharacter.ToLower(locale, chstr[0]).Equals(
                                                                       chstr[1]))
                        {
                            Errln(s);
                            Errln("Fail: toLowerCase for character " +
                                  Utility.Escape(chstr[0]) + ", expected "
                                  + Utility.Escape(chstr[1]) + " but resulted in " +
                                  Utility.Escape(UCharacter.ToLower(locale,
                                                                        chstr[0])));
                        }
                        if (!UCharacter.ToUpper(locale, chstr[0]).Equals(
                                                                       chstr[3]))
                        {
                            Errln(s);
                            Errln("Fail: toUpperCase for character " +
                                  Utility.Escape(chstr[0]) + ", expected "
                                  + Utility.Escape(chstr[3]) + " but resulted in " +
                                  Utility.Escape(UCharacter.ToUpper(locale,
                                                                        chstr[0])));
                        }
                    }
                }
                input.Dispose();
            }
            catch (Exception e)
            {
                e.PrintStackTrace();
            }
        }

        [Test]
        public void TestUpperLower()
        {
            int[] upper = {0x0041, 0x0042, 0x00b2, 0x01c4, 0x01c6, 0x01c9, 0x01c8,
                        0x01c9, 0x000c};
            int[] lower = {0x0061, 0x0062, 0x00b2, 0x01c6, 0x01c6, 0x01c9, 0x01c9,
                        0x01c9, 0x000c};
            String upperTest = "abcdefg123hij.?:klmno";
            String lowerTest = "ABCDEFG123HIJ.?:KLMNO";

            // Checks LetterLike Symbols which were previously a source of
            // confusion [Bertrand A. D. 02/04/98]
            for (int i = 0x2100; i < 0x2138; i++)
            {
                /* Unicode 5.0 adds lowercase U+214E (TURNED SMALL F) to U+2132 (TURNED CAPITAL F) */
                if (i != 0x2126 && i != 0x212a && i != 0x212b && i != 0x2132)
                {
                    if (i != UCharacter.ToLower(i))
                    { // itself
                        Errln("Failed case conversion with itself: \\u"
                                + Utility.Hex(i, 4));
                    }
                    if (i != UCharacter.ToLower(i))
                    {
                        Errln("Failed case conversion with itself: \\u"
                                + Utility.Hex(i, 4));
                    }
                }
            }
            for (int i = 0; i < upper.Length; i++)
            {
                if (UCharacter.ToLower(upper[i]) != lower[i])
                {
                    Errln("FAILED UCharacter.tolower() for \\u"
                            + Utility.Hex(upper[i], 4)
                            + " Expected \\u" + Utility.Hex(lower[i], 4)
                            + " Got \\u"
                            + Utility.Hex(UCharacter.ToLower(upper[i]), 4));
                }
            }
            Logln("testing upper lower");
            for (int i = 0; i < upperTest.Length; i++)
            {
                Logln("testing to upper to lower");
                if (UCharacter.IsLetter(upperTest[i]) &&
                    !UCharacter.IsLowerCase(upperTest[i]))
                {
                    Errln("Failed isLowerCase test at \\u"
                            + Utility.Hex(upperTest[i], 4));
                }
                else if (UCharacter.IsLetter(lowerTest[i])
                         && !UCharacter.IsUpperCase(lowerTest[i]))
                {
                    Errln("Failed isUpperCase test at \\u"
                          + Utility.Hex(lowerTest[i], 4));
                }
                else if (upperTest[i]
                                != UCharacter.ToLower(lowerTest[i]))
                {
                    Errln("Failed case conversion from \\u"
                            + Utility.Hex(lowerTest[i], 4) + " To \\u"
                            + Utility.Hex(upperTest[i], 4));
                }
                else if (lowerTest[i]
                        != UCharacter.ToUpper(upperTest[i]))
                {
                    Errln("Failed case conversion : \\u"
                            + Utility.Hex(upperTest[i], 4) + " To \\u"
                            + Utility.Hex(lowerTest[i], 4));
                }
                else if (upperTest[i]
                        != UCharacter.ToLower(upperTest[i]))
                {
                    Errln("Failed case conversion with itself: \\u"
                            + Utility.Hex(upperTest[i]));
                }
                else if (lowerTest[i]
                        != UCharacter.ToUpper(lowerTest[i]))
                {
                    Errln("Failed case conversion with itself: \\u"
                            + Utility.Hex(lowerTest[i]));
                }
            }
            Logln("done testing upper Lower");
        }

        private void AssertGreekUpper(String s, String expected)
        {
            assertEquals("toUpper/Greek(" + s + ')', expected, UCharacter.ToUpper(GREEK_LOCALE_, s));
        }

        [Test]
        public void TestGreekUpper()
        {
            // http://bugs.icu-project.org/trac/ticket/5456
            AssertGreekUpper("άδικος, κείμενο, ίριδα", "ΑΔΙΚΟΣ, ΚΕΙΜΕΝΟ, ΙΡΙΔΑ");
            // https://bugzilla.mozilla.org/show_bug.cgi?id=307039
            // https://bug307039.bmoattachments.org/attachment.cgi?id=194893
            AssertGreekUpper("Πατάτα", "ΠΑΤΑΤΑ");
            AssertGreekUpper("Αέρας, Μυστήριο, Ωραίο", "ΑΕΡΑΣ, ΜΥΣΤΗΡΙΟ, ΩΡΑΙΟ");
            AssertGreekUpper("Μαΐου, Πόρος, Ρύθμιση", "ΜΑΪΟΥ, ΠΟΡΟΣ, ΡΥΘΜΙΣΗ");
            AssertGreekUpper("ΰ, Τηρώ, Μάιος", "Ϋ, ΤΗΡΩ, ΜΑΪΟΣ");
            AssertGreekUpper("άυλος", "ΑΫΛΟΣ");
            AssertGreekUpper("ΑΫΛΟΣ", "ΑΫΛΟΣ");
            AssertGreekUpper("Άκλιτα ρήματα ή άκλιτες μετοχές", "ΑΚΛΙΤΑ ΡΗΜΑΤΑ Ή ΑΚΛΙΤΕΣ ΜΕΤΟΧΕΣ");
            // http://www.unicode.org/udhr/d/udhr_ell_monotonic.html
            AssertGreekUpper("Επειδή η αναγνώριση της αξιοπρέπειας", "ΕΠΕΙΔΗ Η ΑΝΑΓΝΩΡΙΣΗ ΤΗΣ ΑΞΙΟΠΡΕΠΕΙΑΣ");
            AssertGreekUpper("νομικού ή διεθνούς", "ΝΟΜΙΚΟΥ Ή ΔΙΕΘΝΟΥΣ");
            // http://unicode.org/udhr/d/udhr_ell_polytonic.html
            AssertGreekUpper("Ἐπειδὴ ἡ ἀναγνώριση", "ΕΠΕΙΔΗ Η ΑΝΑΓΝΩΡΙΣΗ");
            AssertGreekUpper("νομικοῦ ἢ διεθνοῦς", "ΝΟΜΙΚΟΥ Ή ΔΙΕΘΝΟΥΣ");
            // From Google bug report
            AssertGreekUpper("Νέο, Δημιουργία", "ΝΕΟ, ΔΗΜΙΟΥΡΓΙΑ");
            // http://crbug.com/234797
            AssertGreekUpper("Ελάτε να φάτε τα καλύτερα παϊδάκια!", "ΕΛΑΤΕ ΝΑ ΦΑΤΕ ΤΑ ΚΑΛΥΤΕΡΑ ΠΑΪΔΑΚΙΑ!");
            AssertGreekUpper("Μαΐου, τρόλεϊ", "ΜΑΪΟΥ, ΤΡΟΛΕΪ");
            AssertGreekUpper("Το ένα ή το άλλο.", "ΤΟ ΕΝΑ Ή ΤΟ ΑΛΛΟ.");
            // http://multilingualtypesetting.co.uk/blog/greek-typesetting-tips/
            AssertGreekUpper("ρωμέικα", "ΡΩΜΕΪΚΑ");
            AssertGreekUpper("ή.", "Ή.");
        }

        private sealed class EditChange
        {
            internal bool Change { get; set; }
            internal int OldLength { get; set; }
            internal int NewLength { get; set; }
            internal EditChange(bool change, int oldLength, int newLength)
            {
                this.Change = change;
                this.OldLength = oldLength;
                this.NewLength = newLength;
            }
        }

        private static String PrintOneEdit(Edits.Enumerator ei)
        {
            if (ei.HasChange)
            {
                return "" + ei.OldLength + "->" + ei.NewLength;
            }
            else
            {
                return "" + ei.OldLength + "=" + ei.NewLength;
            }
        }

        /**
         * Maps indexes according to the expected edits.
         * A destination index can occur multiple times when there are source deletions.
         * Map according to the last occurrence, normally in a non-empty destination span.
         * Simplest is to search from the back.
         */
        private static int SrcIndexFromDest(
                EditChange[] expected, int srcLength, int destLength, int index)
        {
            int srcIndex = srcLength;
            int destIndex = destLength;
            int i = expected.Length;
            while (index < destIndex && i > 0)
            {
                --i;
                int prevSrcIndex = srcIndex - expected[i].OldLength;
                int prevDestIndex = destIndex - expected[i].NewLength;
                if (index == prevDestIndex)
                {
                    return prevSrcIndex;
                }
                else if (index > prevDestIndex)
                {
                    if (expected[i].Change)
                    {
                        // In a change span, map to its end.
                        return srcIndex;
                    }
                    else
                    {
                        // In an unchanged span, offset within it.
                        return prevSrcIndex + (index - prevDestIndex);
                    }
                }
                srcIndex = prevSrcIndex;
                destIndex = prevDestIndex;
            }
            // index is outside the string.
            return srcIndex;
        }

        private static int DestIndexFromSrc(
                EditChange[] expected, int srcLength, int destLength, int index)
        {
            int srcIndex = srcLength;
            int destIndex = destLength;
            int i = expected.Length;
            while (index < srcIndex && i > 0)
            {
                --i;
                int prevSrcIndex = srcIndex - expected[i].OldLength;
                int prevDestIndex = destIndex - expected[i].NewLength;
                if (index == prevSrcIndex)
                {
                    return prevDestIndex;
                }
                else if (index > prevSrcIndex)
                {
                    if (expected[i].Change)
                    {
                        // In a change span, map to its end.
                        return destIndex;
                    }
                    else
                    {
                        // In an unchanged span, offset within it.
                        return prevDestIndex + (index - prevSrcIndex);
                    }
                }
                srcIndex = prevSrcIndex;
                destIndex = prevDestIndex;
            }
            // index is outside the string.
            return destIndex;
        }

        private void CheckEqualEdits(String name, Edits e1, Edits e2)
        {
            Edits.Enumerator ei1 = e1.GetFineEnumerator();
            Edits.Enumerator ei2 = e2.GetFineEnumerator();
            for (int i = 0; ; ++i)
            {
                bool ei1HasNext = ei1.MoveNext();
                bool ei2HasNext = ei2.MoveNext();
                assertEquals(name + " next()[" + i + "]", ei1HasNext, ei2HasNext);
                assertEquals(name + " edit[" + i + "]", PrintOneEdit(ei1), PrintOneEdit(ei2));
                if (!ei1HasNext || !ei2HasNext)
                {
                    break;
                }
            }
        }

        private static void CheckEditsIter(
                String name, Edits.Enumerator ei1, Edits.Enumerator ei2,  // two equal iterators
                EditChange[] expected, bool withUnchanged)
        {
            assertFalse(name, ei2.FindSourceIndex(-1));
            assertFalse(name, ei2.FindDestinationIndex(-1));

            int expSrcIndex = 0;
            int expDestIndex = 0;
            int expReplIndex = 0;
            for (int expIndex = 0; expIndex < expected.Length; ++expIndex)
            {
                EditChange expect = expected[expIndex];
                String msg = name + ' ' + expIndex;
                if (withUnchanged || expect.Change)
                {
                    assertTrue(msg, ei1.MoveNext());
                    assertEquals(msg, expect.Change, ei1.HasChange);
                    assertEquals(msg, expect.OldLength, ei1.OldLength);
                    assertEquals(msg, expect.NewLength, ei1.NewLength);
                    assertEquals(msg, expSrcIndex, ei1.SourceIndex);
                    assertEquals(msg, expDestIndex, ei1.DestinationIndex);
                    assertEquals(msg, expReplIndex, ei1.ReplacementIndex);
                }

                if (expect.OldLength > 0)
                {
                    assertTrue(msg, ei2.FindSourceIndex(expSrcIndex));
                    assertEquals(msg, expect.Change, ei2.HasChange);
                    assertEquals(msg, expect.OldLength, ei2.OldLength);
                    assertEquals(msg, expect.NewLength, ei2.NewLength);
                    assertEquals(msg, expSrcIndex, ei2.SourceIndex);
                    assertEquals(msg, expDestIndex, ei2.DestinationIndex);
                    assertEquals(msg, expReplIndex, ei2.ReplacementIndex);
                    if (!withUnchanged)
                    {
                        // For some iterators, move past the current range
                        // so that findSourceIndex() has to look before the current index.
                        ei2.MoveNext();
                        ei2.MoveNext();
                    }
                }

                if (expect.NewLength > 0)
                {
                    assertTrue(msg, ei2.FindDestinationIndex(expDestIndex));
                    assertEquals(msg, expect.Change, ei2.HasChange);
                    assertEquals(msg, expect.OldLength, ei2.OldLength);
                    assertEquals(msg, expect.NewLength, ei2.NewLength);
                    assertEquals(msg, expSrcIndex, ei2.SourceIndex);
                    assertEquals(msg, expDestIndex, ei2.DestinationIndex);
                    assertEquals(msg, expReplIndex, ei2.ReplacementIndex);
                    if (!withUnchanged)
                    {
                        // For some iterators, move past the current range
                        // so that findSourceIndex() has to look before the current index.
                        ei2.MoveNext();
                        ei2.MoveNext();
                    }
                }

                expSrcIndex += expect.OldLength;
                expDestIndex += expect.NewLength;
                if (expect.Change)
                {
                    expReplIndex += expect.NewLength;
                }
            }

            {
                String msg = name + " end";
                assertFalse(msg, ei1.MoveNext());
                assertFalse(msg, ei1.HasChange);
                assertEquals(msg, 0, ei1.OldLength);
                assertEquals(msg, 0, ei1.NewLength);
                assertEquals(msg, expSrcIndex, ei1.SourceIndex);
                assertEquals(msg, expDestIndex, ei1.DestinationIndex);
                assertEquals(msg, expReplIndex, ei1.ReplacementIndex);

                assertFalse(name, ei2.FindSourceIndex(expSrcIndex));
                assertFalse(name, ei2.FindDestinationIndex(expDestIndex));

                // Check mapping of all indexes against a simple implementation
                // that works on the expected changes.
                // Iterate once forward, once backward, to cover more runtime conditions.
                int srcLength = expSrcIndex;
                int destLength = expDestIndex;
                List<int> srcIndexes = new List<int>();
                List<int> destIndexes = new List<int>();
                srcIndexes.Add(-1);
                destIndexes.Add(-1);
                int srcIndex = 0;
                int destIndex = 0;
                for (int i = 0; i < expected.Length; ++i)
                {
                    if (expected[i].OldLength > 0)
                    {
                        srcIndexes.Add(srcIndex);
                        if (expected[i].OldLength > 1)
                        {
                            srcIndexes.Add(srcIndex + 1);
                            if (expected[i].OldLength > 2)
                            {
                                srcIndexes.Add(srcIndex + expected[i].OldLength - 1);
                            }
                        }
                    }
                    if (expected[i].NewLength > 0)
                    {
                        destIndexes.Add(destIndex);
                        if (expected[i].NewLength > 1)
                        {
                            destIndexes.Add(destIndex + 1);
                            if (expected[i].NewLength > 2)
                            {
                                destIndexes.Add(destIndex + expected[i].NewLength - 1);
                            }
                        }
                    }
                    srcIndex += expected[i].OldLength;
                    destIndex += expected[i].NewLength;
                }
                srcIndexes.Add(srcLength);
                destIndexes.Add(destLength);
                srcIndexes.Add(srcLength + 1);
                destIndexes.Add(destLength + 1);
                destIndexes.Reverse();
                // Zig-zag across the indexes to stress next() <-> previous().
                for (int i = 0; i < srcIndexes.Count; ++i)
                {
                    foreach (int j in ZIG_ZAG)
                    {
                        if ((i + j) < srcIndexes.Count)
                        {
                            int si = srcIndexes[i + j];
                            assertEquals(name + " destIndexFromSrc(" + si + "):",
                                    DestIndexFromSrc(expected, srcLength, destLength, si),
                                    ei2.DestinationIndexFromSourceIndex(si));
                        }
                    }
                }
                for (int i = 0; i < destIndexes.Count; ++i)
                {
                    foreach (int j in ZIG_ZAG)
                    {
                        if ((i + j) < destIndexes.Count)
                        {
                            int di = destIndexes[i + j];
                            assertEquals(name + " srcIndexFromDest(" + di + "):",
                                    SrcIndexFromDest(expected, srcLength, destLength, di),
                                    ei2.SourceIndexFromDestinationIndex(di));
                        }
                    }
                }
            }
        }

        private static readonly int[] ZIG_ZAG = { 0, 1, 2, 3, 2, 1 };

        [Test]
        public void TestEdits()
        {
            Edits edits = new Edits();
            assertFalse("new Edits hasChanges", edits.HasChanges);
            assertEquals("new Edits numberOfChanges", 0, edits.NumberOfChanges);
            assertEquals("new Edits", 0, edits.LengthDelta);
            edits.AddUnchanged(1);  // multiple unchanged ranges are combined
            edits.AddUnchanged(10000);  // too long, and they are split
            edits.AddReplace(0, 0);
            edits.AddUnchanged(2);
            assertFalse("unchanged 10003 hasChanges", edits.HasChanges);
            assertEquals("unchanged 10003 numberOfChanges", 0, edits.NumberOfChanges);
            assertEquals("unchanged 10003", 0, edits.LengthDelta);
            edits.AddReplace(2, 1);  // multiple short equal-lengths edits are compressed
            edits.AddUnchanged(0);
            edits.AddReplace(2, 1);
            edits.AddReplace(2, 1);
            edits.AddReplace(0, 10);
            edits.AddReplace(100, 0);
            edits.AddReplace(3000, 4000);  // variable-length encoding
            edits.AddReplace(100000, 100000);
            assertTrue("some edits hasChanges", edits.HasChanges);
            assertEquals("some edits numberOfChanges", 7, edits.NumberOfChanges);
            assertEquals("some edits", -3 + 10 - 100 + 1000, edits.LengthDelta);

            EditChange[] coarseExpectedChanges = new EditChange[] {
                new EditChange(false, 10003, 10003),
                new EditChange(true, 103106, 104013)
        };
            CheckEditsIter("coarse",
                    edits.GetCoarseEnumerator(), edits.GetCoarseEnumerator(),
                    coarseExpectedChanges, true);
            CheckEditsIter("coarse changes",
                    edits.GetCoarseChangesEnumerator(), edits.GetCoarseChangesEnumerator(),
                    coarseExpectedChanges, false);

            EditChange[] fineExpectedChanges = new EditChange[] {
                new EditChange(false, 10003, 10003),
                new EditChange(true, 2, 1),
                new EditChange(true, 2, 1),
                new EditChange(true, 2, 1),
                new EditChange(true, 0, 10),
                new EditChange(true, 100, 0),
                new EditChange(true, 3000, 4000),
                new EditChange(true, 100000, 100000)
        };
            CheckEditsIter("fine",
                    edits.GetFineEnumerator(), edits.GetFineEnumerator(),
                    fineExpectedChanges, true);
            CheckEditsIter("fine changes",
                    edits.GetFineChangesEnumerator(), edits.GetFineChangesEnumerator(),
                    fineExpectedChanges, false);

            edits.Reset();
            assertFalse("reset hasChanges", edits.HasChanges);
            assertEquals("reset numberOfChanges", 0, edits.NumberOfChanges);
            assertEquals("reset", 0, edits.LengthDelta);
            Edits.Enumerator ei = edits.GetCoarseChangesEnumerator();
            assertFalse("reset then iterator", ei.MoveNext());
        }

        [Test]
        public void TestEditsFindFwdBwd()
        {
            // Some users need index mappings to be efficient when they are out of order.
            // The most interesting failure case for this test is it taking a very long time.
            Edits e = new Edits();
            int N = 200000;
            for (int i = 0; i < N; ++i)
            {
                e.AddUnchanged(1);
                e.AddReplace(3, 1);
            }
            Edits.Enumerator iter = e.GetFineEnumerator();
            for (int i = 0; i <= N; i += 2)
            {
                assertEquals("ascending", i * 2, iter.SourceIndexFromDestinationIndex(i));
                assertEquals("ascending", i * 2 + 1, iter.SourceIndexFromDestinationIndex(i + 1));
            }
            for (int i = N; i >= 0; i -= 2)
            {
                assertEquals("descending", i * 2 + 1, iter.SourceIndexFromDestinationIndex(i + 1));
                assertEquals("descending", i * 2, iter.SourceIndexFromDestinationIndex(i));
            }
        }

        [Test]
        public void TestMergeEdits()
        {
            Edits ab = new Edits(), bc = new Edits(), ac = new Edits(), expected_ac = new Edits();

            // Simple: Two parallel non-changes.
            ab.AddUnchanged(2);
            bc.AddUnchanged(2);
            expected_ac.AddUnchanged(2);

            // Simple: Two aligned changes.
            ab.AddReplace(3, 2);
            bc.AddReplace(2, 1);
            expected_ac.AddReplace(3, 1);

            // Unequal non-changes.
            ab.AddUnchanged(5);
            bc.AddUnchanged(3);
            expected_ac.AddUnchanged(3);
            // ab ahead by 2

            // Overlapping changes accumulate until they share a boundary.
            ab.AddReplace(4, 3);
            bc.AddReplace(3, 2);
            ab.AddReplace(4, 3);
            bc.AddReplace(3, 2);
            ab.AddReplace(4, 3);
            bc.AddReplace(3, 2);
            bc.AddUnchanged(4);
            expected_ac.AddReplace(14, 8);
            // bc ahead by 2

            // Balance out intermediate-string lengths.
            ab.AddUnchanged(2);
            expected_ac.AddUnchanged(2);

            // Insert something and delete it: Should disappear.
            ab.AddReplace(0, 5);
            ab.AddReplace(0, 2);
            bc.AddReplace(7, 0);

            // Parallel change to make a new boundary.
            ab.AddReplace(1, 2);
            bc.AddReplace(2, 3);
            expected_ac.AddReplace(1, 3);

            // Multiple ab deletions should remain separate at the boundary.
            ab.AddReplace(1, 0);
            ab.AddReplace(2, 0);
            ab.AddReplace(3, 0);
            expected_ac.AddReplace(1, 0);
            expected_ac.AddReplace(2, 0);
            expected_ac.AddReplace(3, 0);

            // Unequal non-changes can be split for another boundary.
            ab.AddUnchanged(2);
            bc.AddUnchanged(1);
            expected_ac.AddUnchanged(1);
            // ab ahead by 1

            // Multiple bc insertions should create a boundary and remain separate.
            bc.AddReplace(0, 4);
            bc.AddReplace(0, 5);
            bc.AddReplace(0, 6);
            expected_ac.AddReplace(0, 4);
            expected_ac.AddReplace(0, 5);
            expected_ac.AddReplace(0, 6);
            // ab ahead by 1

            // Multiple ab deletions in the middle of a bc change are merged.
            bc.AddReplace(2, 2);
            // bc ahead by 1
            ab.AddReplace(1, 0);
            ab.AddReplace(2, 0);
            ab.AddReplace(3, 0);
            ab.AddReplace(4, 1);
            expected_ac.AddReplace(11, 2);

            // Multiple bc insertions in the middle of an ab change are merged.
            ab.AddReplace(5, 6);
            bc.AddReplace(3, 3);
            // ab ahead by 3
            bc.AddReplace(0, 4);
            bc.AddReplace(0, 5);
            bc.AddReplace(0, 6);
            bc.AddReplace(3, 7);
            expected_ac.AddReplace(5, 25);

            // Delete around a deletion.
            ab.AddReplace(4, 4);
            ab.AddReplace(3, 0);
            ab.AddUnchanged(2);
            bc.AddReplace(2, 2);
            bc.AddReplace(4, 0);
            expected_ac.AddReplace(9, 2);

            // Insert into an insertion.
            ab.AddReplace(0, 2);
            bc.AddReplace(1, 1);
            bc.AddReplace(0, 8);
            bc.AddUnchanged(4);
            expected_ac.AddReplace(0, 10);
            // bc ahead by 3

            // Balance out intermediate-string lengths.
            ab.AddUnchanged(3);
            expected_ac.AddUnchanged(3);

            // Deletions meet insertions.
            // Output order is arbitrary in principle, but we expect insertions first
            // and want to keep it that way.
            ab.AddReplace(2, 0);
            ab.AddReplace(4, 0);
            ab.AddReplace(6, 0);
            bc.AddReplace(0, 1);
            bc.AddReplace(0, 3);
            bc.AddReplace(0, 5);
            expected_ac.AddReplace(0, 1);
            expected_ac.AddReplace(0, 3);
            expected_ac.AddReplace(0, 5);
            expected_ac.AddReplace(2, 0);
            expected_ac.AddReplace(4, 0);
            expected_ac.AddReplace(6, 0);

            // End with a non-change, so that further edits are never reordered.
            ab.AddUnchanged(1);
            bc.AddUnchanged(1);
            expected_ac.AddUnchanged(1);

            ac.MergeAndAppend(ab, bc);
            CheckEqualEdits("ab+bc", expected_ac, ac);

            // Append more Edits.
            Edits ab2 = new Edits(), bc2 = new Edits();
            ab2.AddUnchanged(5);
            bc2.AddReplace(1, 2);
            bc2.AddUnchanged(4);
            expected_ac.AddReplace(1, 2);
            expected_ac.AddUnchanged(4);
            ac.MergeAndAppend(ab2, bc2);
            CheckEqualEdits("ab2+bc2", expected_ac, ac);

            // Append empty edits.
            Edits empty = new Edits();
            ac.MergeAndAppend(empty, empty);
            CheckEqualEdits("empty+empty", expected_ac, ac);

            // Error: Append more edits with mismatched intermediate-string lengths.
            Edits mismatch = new Edits();
            mismatch.AddReplace(1, 1);
            try
            {
                ac.MergeAndAppend(ab2, mismatch);
                fail("ab2+mismatch did not yield ArgumentException");
            }
            catch (ArgumentException expected)
            {
            }
            try
            {
                ac.MergeAndAppend(mismatch, bc2);
                fail("mismatch+bc2 did not yield ArgumentException");
            }
            catch (ArgumentException expected)
            {
            }
        }

        [Test]
        public void TestCaseMapWithEdits()
        {
            StringBuilder sb = new StringBuilder();
            Edits edits = new Edits();

            sb = CaseMap.ToLower().OmitUnchangedText().Apply(TURKISH_LOCALE_, "IstanBul", sb, edits);
            assertEquals("toLower(Istanbul)", "ıb", sb.ToString());
            EditChange[] lowerExpectedChanges = new EditChange[] {
                new EditChange(true, 1, 1),
                new EditChange(false, 4, 4),
                new EditChange(true, 1, 1),
                new EditChange(false, 2, 2)
        };
            CheckEditsIter("toLower(Istanbul)",
                    edits.GetFineEnumerator(), edits.GetFineEnumerator(),
                    lowerExpectedChanges, true);

            sb.Delete(0, sb.Length);
            edits.Reset();
            sb = CaseMap.ToUpper().OmitUnchangedText().Apply(GREEK_LOCALE_, "Πατάτα", sb, edits);
            assertEquals("toUpper(Πατάτα)", "ΑΤΑΤΑ", sb.ToString());
            EditChange[] upperExpectedChanges = new EditChange[] {
                new EditChange(false, 1, 1),
                new EditChange(true, 1, 1),
                new EditChange(true, 1, 1),
                new EditChange(true, 1, 1),
                new EditChange(true, 1, 1),
                new EditChange(true, 1, 1)
        };
            CheckEditsIter("toUpper(Πατάτα)",
                    edits.GetFineEnumerator(), edits.GetFineEnumerator(),
                    upperExpectedChanges, true);

            sb.Delete(0, sb.Length);
            edits.Reset();
            sb = CaseMap.ToTitle().OmitUnchangedText().NoBreakAdjustment().NoLowercase().Apply(
                    DUTCH_LOCALE_, null, "IjssEL IglOo", sb, edits);
            assertEquals("toTitle(IjssEL IglOo)", "J", sb.ToString());
            EditChange[] titleExpectedChanges = new EditChange[] {
                new EditChange(false, 1, 1),
                new EditChange(true, 1, 1),
                new EditChange(false, 10, 10)
        };
            CheckEditsIter("toTitle(IjssEL IglOo)",
                    edits.GetFineEnumerator(), edits.GetFineEnumerator(),
                    titleExpectedChanges, true);

            sb.Delete(0, sb.Length);
            edits.Reset();
            sb = CaseMap.ToFold().OmitUnchangedText().Turkic().Apply("IßtanBul", sb, edits);
            assertEquals("fold(IßtanBul)", "ıssb", sb.ToString());
            EditChange[] foldExpectedChanges = new EditChange[] {
                new EditChange(true, 1, 1),
                new EditChange(true, 1, 2),
                new EditChange(false, 3, 3),
                new EditChange(true, 1, 1),
                new EditChange(false, 2, 2)
        };
            CheckEditsIter("fold(IßtanBul)",
                    edits.GetFineEnumerator(), edits.GetFineEnumerator(),
                    foldExpectedChanges, true);
        }

        [Test]
        public void TestCaseMapToString()
        {
            // String apply(..., CharSequence)
            // Omit unchanged text.
            assertEquals("toLower(Istanbul)", "ıb",
                    CaseMap.ToLower().OmitUnchangedText().Apply(TURKISH_LOCALE_, "IstanBul"));
            assertEquals("toUpper(Πατάτα)", "ΑΤΑΤΑ",
                    CaseMap.ToUpper().OmitUnchangedText().Apply(GREEK_LOCALE_, "Πατάτα"));
            assertEquals("toTitle(IjssEL IglOo)", "J",
                    CaseMap.ToTitle().OmitUnchangedText().NoBreakAdjustment().NoLowercase().Apply(
                            DUTCH_LOCALE_, null, "IjssEL IglOo"));
            assertEquals("fold(IßtanBul)", "ıssb",
                    CaseMap.ToFold().OmitUnchangedText().Turkic().Apply("IßtanBul"));

            // Return the whole result string.
            assertEquals("toLower(Istanbul)", "ıstanbul",
                    CaseMap.ToLower().Apply(TURKISH_LOCALE_, "IstanBul"));
            assertEquals("toUpper(Πατάτα)", "ΠΑΤΑΤΑ",
                    CaseMap.ToUpper().Apply(GREEK_LOCALE_, "Πατάτα"));
            assertEquals("toTitle(IjssEL IglOo)", "IJssEL IglOo",
                    CaseMap.ToTitle().NoBreakAdjustment().NoLowercase().Apply(
                            DUTCH_LOCALE_, null, "IjssEL IglOo"));
            assertEquals("fold(IßtanBul)", "ısstanbul",
                    CaseMap.ToFold().Turkic().Apply("IßtanBul"));
        }

        // private data members - test data --------------------------------------

        private static readonly CultureInfo TURKISH_LOCALE_ = new CultureInfo("tr-TR");
        private static readonly CultureInfo GERMAN_LOCALE_ = new CultureInfo("de-DE");
        private static readonly CultureInfo GREEK_LOCALE_ = new CultureInfo("el-GR");
        private static readonly CultureInfo ENGLISH_LOCALE_ = new CultureInfo("en-US");
        private static readonly CultureInfo LITHUANIAN_LOCALE_ = new CultureInfo("lt-LT");
        private static readonly CultureInfo DUTCH_LOCALE_ = new CultureInfo("nl");

        private static readonly int[] CHARACTER_UPPER_ =
                          {0x41, 0x0042, 0x0043, 0x0044, 0x0045, 0x0046, 0x0047,
                       0x00b1, 0x00b2, 0xb3, 0x0048, 0x0049, 0x004a, 0x002e,
                       0x003f, 0x003a, 0x004b, 0x004c, 0x4d, 0x004e, 0x004f,
                       0x01c4, 0x01c8, 0x000c, 0x0000};
        private static readonly int[] CHARACTER_LOWER_ =
                          {0x61, 0x0062, 0x0063, 0x0064, 0x0065, 0x0066, 0x0067,
                       0x00b1, 0x00b2, 0xb3, 0x0068, 0x0069, 0x006a, 0x002e,
                       0x003f, 0x003a, 0x006b, 0x006c, 0x6d, 0x006e, 0x006f,
                       0x01c6, 0x01c9, 0x000c, 0x0000};

        /*
         * CaseFolding.txt says about i and its cousins:
         *   0049; C; 0069; # LATIN CAPITAL LETTER I
         *   0049; T; 0131; # LATIN CAPITAL LETTER I
         *
         *   0130; F; 0069 0307; # LATIN CAPITAL LETTER I WITH DOT ABOVE
         *   0130; T; 0069; # LATIN CAPITAL LETTER I WITH DOT ABOVE
         * That's all.
         * See CaseFolding.txt and the Unicode Standard for how to apply the case foldings.
         */
        private static readonly int[] FOLDING_SIMPLE_ = {
        // input, default, exclude special i
        0x61,   0x61,  0x61,
        0x49,   0x69,  0x131,
        0x130,  0x130, 0x69,
        0x131,  0x131, 0x131,
        0xdf,   0xdf,  0xdf,
        0xfb03, 0xfb03, 0xfb03,
        0x1040e,0x10436,0x10436,
        0x5ffff,0x5ffff,0x5ffff
    };
        private static readonly String[] FOLDING_MIXED_ =
                              {"\u0061\u0042\u0130\u0049\u0131\u03d0\u00df\ufb03\ud93f\udfff",
                           "A\u00df\u00b5\ufb03\uD801\uDC0C\u0130\u0131"};
        private static readonly String[] FOLDING_DEFAULT_ =
                 {"\u0061\u0062\u0069\u0307\u0069\u0131\u03b2\u0073\u0073\u0066\u0066\u0069\ud93f\udfff",
          "ass\u03bcffi\uD801\uDC34i\u0307\u0131"};
        private static readonly String[] FOLDING_EXCLUDE_SPECIAL_I_ =
             {"\u0061\u0062\u0069\u0131\u0131\u03b2\u0073\u0073\u0066\u0066\u0069\ud93f\udfff",
          "ass\u03bcffi\uD801\uDC34i\u0131"};
        /**
         * "IESUS CHRISTOS"
         */
        private static readonly String SHARED_UPPERCASE_GREEK_ =
            "\u0399\u0395\u03a3\u03a5\u03a3\u0020\u03a7\u03a1\u0399\u03a3\u03a4\u039f\u03a3";
        /**
         * "iesus christos"
         */
        private static readonly String SHARED_LOWERCASE_GREEK_ =
            "\u03b9\u03b5\u03c3\u03c5\u03c2\u0020\u03c7\u03c1\u03b9\u03c3\u03c4\u03bf\u03c2";
        private static readonly String SHARED_LOWERCASE_TURKISH_ =
            "\u0069\u0073\u0074\u0061\u006e\u0062\u0075\u006c\u002c\u0020\u006e\u006f\u0074\u0020\u0063\u006f\u006e\u0073\u0074\u0061\u006e\u0074\u0131\u006e\u006f\u0070\u006c\u0065\u0021";
        private static readonly String SHARED_UPPERCASE_TURKISH_ =
            "\u0054\u004f\u0050\u004b\u0041\u0050\u0049\u0020\u0050\u0041\u004c\u0041\u0043\u0045\u002c\u0020\u0130\u0053\u0054\u0041\u004e\u0042\u0055\u004c";
        private static readonly String SHARED_UPPERCASE_ISTANBUL_ =
                                              "\u0130STANBUL, NOT CONSTANTINOPLE!";
        private static readonly String SHARED_LOWERCASE_ISTANBUL_ =
                                              "i\u0307stanbul, not constantinople!";
        private static readonly String SHARED_LOWERCASE_TOPKAP_ =
                                              "topkap\u0131 palace, istanbul";
        private static readonly String SHARED_UPPERCASE_TOPKAP_ =
                                              "TOPKAPI PALACE, ISTANBUL";
        private static readonly String SHARED_LOWERCASE_GERMAN_ =
                                              "S\u00FC\u00DFmayrstra\u00DFe";
        private static readonly String SHARED_UPPERCASE_GERMAN_ =
                                              "S\u00DCSSMAYRSTRASSE";

        private static readonly String UPPER_BEFORE_ =
             "\u0061\u0042\u0069\u03c2\u00df\u03c3\u002f\ufb03\ufb03\ufb03\ud93f\udfff";
        private static readonly String UPPER_ROOT_ =
             "\u0041\u0042\u0049\u03a3\u0053\u0053\u03a3\u002f\u0046\u0046\u0049\u0046\u0046\u0049\u0046\u0046\u0049\ud93f\udfff";
        private static readonly String UPPER_TURKISH_ =
             "\u0041\u0042\u0130\u03a3\u0053\u0053\u03a3\u002f\u0046\u0046\u0049\u0046\u0046\u0049\u0046\u0046\u0049\ud93f\udfff";
        private static readonly String UPPER_MINI_ = "\u00df\u0061";
        private static readonly String UPPER_MINI_UPPER_ = "\u0053\u0053\u0041";

        private static readonly String LOWER_BEFORE_ =
                          "\u0061\u0042\u0049\u03a3\u00df\u03a3\u002f\ud93f\udfff";
        private static readonly String LOWER_ROOT_ =
                          "\u0061\u0062\u0069\u03c3\u00df\u03c2\u002f\ud93f\udfff";
        private static readonly String LOWER_TURKISH_ =
                          "\u0061\u0062\u0131\u03c3\u00df\u03c2\u002f\ud93f\udfff";

        /**
         * each item is an array with input string, result string, locale ID, break iterator, options
         * the break iterator is specified as an int, same as in BreakIterator.KIND_*:
         * 0=KIND_CHARACTER  1=KIND_WORD  2=KIND_LINE  3=KIND_SENTENCE  4=KIND_TITLE  -1=default (NULL=words)  -2=no breaks (.*)
         * options: T=U_FOLD_CASE_EXCLUDE_SPECIAL_I  L=U_TITLECASE_NO_LOWERCASE  A=U_TITLECASE_NO_BREAK_ADJUSTMENT
         * see ICU4C source/test/testdata/casing.txt
         */
        private static readonly String[] TITLE_DATA_ = {
        "\u0061\u0042\u0020\u0069\u03c2\u0020\u00df\u03c3\u002f\ufb03\ud93f\udfff",
        "\u0041\u0042\u0020\u0049\u03a3\u0020\u0053\u0073\u03a3\u002f\u0046\u0066\u0069\ud93f\udfff",
        "",
        "0",
        "",

        "\u0061\u0042\u0020\u0069\u03c2\u0020\u00df\u03c3\u002f\ufb03\ud93f\udfff",
        "\u0041\u0062\u0020\u0049\u03c2\u0020\u0053\u0073\u03c3\u002f\u0046\u0066\u0069\ud93f\udfff",
        "",
        "1",
        "",

        "\u02bbaMeLikA huI P\u016b \u02bb\u02bb\u02bbiA", "\u02bbAmelika Hui P\u016b \u02bb\u02bb\u02bbIa", // titlecase first _cased_ letter, j4933
        "",
        "-1",
        "",

        " tHe QUIcK bRoWn", " The Quick Brown",
        "",
        "4",
        "",

        "\u01c4\u01c5\u01c6\u01c7\u01c8\u01c9\u01ca\u01cb\u01cc",
        "\u01c5\u01c5\u01c5\u01c8\u01c8\u01c8\u01cb\u01cb\u01cb", // UBRK_CHARACTER
        "",
        "0",
        "",

        "\u01c9ubav ljubav", "\u01c8ubav Ljubav", // Lj vs. L+j
        "",
        "-1",
        "",

        "'oH dOn'T tItLeCaSe AfTeR lEtTeR+'",  "'Oh Don't Titlecase After Letter+'",
        "",
        "-1",
        "",

        "a \u02bbCaT. A \u02bbdOg! \u02bbeTc.",
        "A \u02bbCat. A \u02bbDog! \u02bbEtc.",
        "",
        "-1",
        "", // default

        "a \u02bbCaT. A \u02bbdOg! \u02bbeTc.",
        "A \u02bbcat. A \u02bbdog! \u02bbetc.",
        "",
        "-1",
        "A", // U_TITLECASE_NO_BREAK_ADJUSTMENT

        "a \u02bbCaT. A \u02bbdOg! \u02bbeTc.",
        "A \u02bbCaT. A \u02bbdOg! \u02bbETc.",
        "",
        "3",
        "L", // UBRK_SENTENCE and U_TITLECASE_NO_LOWERCASE


        "\u02bbcAt! \u02bbeTc.",
        "\u02bbCat! \u02bbetc.",
        "",
        "-2",
        "", // -2=Trivial break iterator

        "\u02bbcAt! \u02bbeTc.",
        "\u02bbcat! \u02bbetc.",
        "",
        "-2",
        "A", // U_TITLECASE_NO_BREAK_ADJUSTMENT

        "\u02bbcAt! \u02bbeTc.",
        "\u02bbCAt! \u02bbeTc.",
        "",
        "-2",
        "L", // U_TITLECASE_NO_LOWERCASE

        "\u02bbcAt! \u02bbeTc.",
        "\u02bbcAt! \u02bbeTc.",
        "",
        "-2",
        "AL", // Both options

        // Test case for ticket #7251: UCharacter.toTitleCase() throws OutOfMemoryError
        // when TITLECASE_NO_LOWERCASE encounters a single-letter word
        "a b c",
        "A B C",
        "",
        "1",
        "L" // U_TITLECASE_NO_LOWERCASE
    };


        /**
         * <p>basic string, lower string, upper string, title string</p>
         */
        private static readonly String[] SPECIAL_DATA_ = {
        UTF16.ValueOf(0x1043C) + UTF16.ValueOf(0x10414),
        UTF16.ValueOf(0x1043C) + UTF16.ValueOf(0x1043C),
        UTF16.ValueOf(0x10414) + UTF16.ValueOf(0x10414),
        "ab'cD \uFB00i\u0131I\u0130 \u01C7\u01C8\u01C9 " +
                         UTF16.ValueOf(0x1043C) + UTF16.ValueOf(0x10414),
        "ab'cd \uFB00i\u0131ii\u0307 \u01C9\u01C9\u01C9 " +
                              UTF16.ValueOf(0x1043C) + UTF16.ValueOf(0x1043C),
        "AB'CD FFIII\u0130 \u01C7\u01C7\u01C7 " +
                              UTF16.ValueOf(0x10414) + UTF16.ValueOf(0x10414),
        // sigmas followed/preceded by cased letters
        "i\u0307\u03a3\u0308j \u0307\u03a3\u0308j i\u00ad\u03a3\u0308 \u0307\u03a3\u0308 ",
        "i\u0307\u03c3\u0308j \u0307\u03c3\u0308j i\u00ad\u03c2\u0308 \u0307\u03c3\u0308 ",
        "I\u0307\u03a3\u0308J \u0307\u03a3\u0308J I\u00ad\u03a3\u0308 \u0307\u03a3\u0308 "
    };
        private static readonly CultureInfo[] SPECIAL_LOCALES_ = {
        null,
        ENGLISH_LOCALE_,
        null,
    };

        private static readonly String SPECIAL_DOTTED_ =
                "I \u0130 I\u0307 I\u0327\u0307 I\u0301\u0307 I\u0327\u0307\u0301";
        private static readonly String SPECIAL_DOTTED_LOWER_TURKISH_ =
                "\u0131 i i i\u0327 \u0131\u0301\u0307 i\u0327\u0301";
        private static readonly String SPECIAL_DOTTED_LOWER_GERMAN_ =
                "i i\u0307 i\u0307 i\u0327\u0307 i\u0301\u0307 i\u0327\u0307\u0301";
        private static readonly String SPECIAL_DOT_ABOVE_ =
                "a\u0307 \u0307 i\u0307 j\u0327\u0307 j\u0301\u0307";
        private static readonly String SPECIAL_DOT_ABOVE_UPPER_LITHUANIAN_ =
                "A\u0307 \u0307 I J\u0327 J\u0301\u0307";
        private static readonly String SPECIAL_DOT_ABOVE_UPPER_GERMAN_ =
                "A\u0307 \u0307 I\u0307 J\u0327\u0307 J\u0301\u0307";
        private static readonly String SPECIAL_DOT_ABOVE_UPPER_ =
                "I I\u0301 J J\u0301 \u012e \u012e\u0301 \u00cc\u00cd\u0128";
        private static readonly String SPECIAL_DOT_ABOVE_LOWER_LITHUANIAN_ =
                "i i\u0307\u0301 j j\u0307\u0301 \u012f \u012f\u0307\u0301 i\u0307\u0300i\u0307\u0301i\u0307\u0303";
        private static readonly String SPECIAL_DOT_ABOVE_LOWER_GERMAN_ =
                "i i\u0301 j j\u0301 \u012f \u012f\u0301 \u00ec\u00ed\u0129";

        // private methods -------------------------------------------------------

        /**
         * Converting the hex numbers represented between ';' to Unicode strings
         * @param str string to break up into Unicode strings
         * @return array of Unicode strings ending with a null
         */
        private String[] GetUnicodeStrings(String str)
        {
            List<String> v = new List<String>(10);
            int start = 0;
            for (int casecount = 4; casecount > 0; casecount--)
            {
                int end = str.IndexOf("; ", start);
                String casestr = str.Substring(start, end - start); // ICU4N: Corrected 2nd parameter
                StringBuffer buffer = new StringBuffer();
                int spaceoffset = 0;
                while (spaceoffset < casestr.Length)
                {
                    int nextspace = casestr.IndexOf(' ', spaceoffset);
                    if (nextspace == -1)
                    {
                        nextspace = casestr.Length;
                    }
                    buffer.Append((char)Convert.ToInt32(
                                         casestr.Substring(spaceoffset, nextspace - spaceoffset), // ICU4N: Corrected 2nd parameter
                                                          16));
                    spaceoffset = nextspace + 1;
                }
                start = end + 2;
                v.Add(buffer.ToString());
            }
            int comments = str.IndexOf(" #", start);
            if (comments != -1 && comments != start)
            {
                if (str[comments - 1] == ';')
                {
                    comments--;
                }
                String conditions = str.Substring(start, comments - start); // ICU4N: Corrected 2nd parameter
                int offset = 0;
                while (offset < conditions.Length)
                {
                    int spaceoffset = conditions.IndexOf(' ', offset);
                    if (spaceoffset == -1)
                    {
                        spaceoffset = conditions.Length;
                    }
                    v.Add(conditions.Substring(offset, spaceoffset - offset)); // ICU4N: Corrected 2nd parameter
                    offset = spaceoffset + 1;
                }
            }
            int size = v.Count;
            String[] result = new String[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = v[i];
            }
            return result;
        }
    }
}
