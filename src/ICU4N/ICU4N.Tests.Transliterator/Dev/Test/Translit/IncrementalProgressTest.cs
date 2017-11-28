using ICU4N.Impl;
using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Collections;

namespace ICU4N.Dev.Test.Translit
{
    /// <summary>
    /// Check to see that incremental gets at least part way through a reasonable string.
    /// </summary>
    public class IncrementalProgressTest : TestFmwk
    {
        public static IEnumerable TestData
        {
            get
            {
                String latinTest = "The Quick Brown Fox.";
                String devaTest = Transliterator.GetInstance("Latin-Devanagari").Transliterate(latinTest);
                String kataTest = Transliterator.GetInstance("Latin-Katakana").Transliterate(latinTest);
                // Labels have to be valid transliterator source names.
                yield return new TestCaseData("Any", latinTest);
                yield return new TestCaseData("Latin", latinTest);
                yield return new TestCaseData("Halfwidth", latinTest);
                yield return new TestCaseData("Devanagari", devaTest);
                yield return new TestCaseData("Katakana", kataTest);
            }
        }

        public void CheckIncrementalAux(Transliterator t, String input)
        {
            IReplaceable test = new ReplaceableString(input);
            Transliterator.Position pos = new Transliterator.Position(0, test.Length, 0, test.Length);
            t.Transliterate(test, pos);
            bool gotError = false;

            // we have a few special cases. Any-Remove (pos.start = 0, but also = limit) and U+XXXXX?X?
            if (pos.Start == 0 && pos.Limit != 0 && !t.ID.Equals("Hex-Any/Unicode"))
            {
                Errln("No Progress, " + t.ID + ": " + UtilityExtensions.FormatInput(test, pos));
                gotError = true;
            }
            else
            {
                Logln("PASS Progress, " + t.ID + ": " + UtilityExtensions.FormatInput(test, pos));
            }
            t.FinishTransliteration(test, pos);
            if (pos.Start != pos.Limit)
            {
                Errln("Incomplete, " + t.ID + ":  " + UtilityExtensions.FormatInput(test, pos));
                gotError = true;
            }
            if (!gotError)
            {
                //Errln("FAIL: Did not get expected error");
            }
        }

        [Test, TestCaseSource(typeof(IncrementalProgressTest), "TestData")]
        public void TestIncrementalProgress(string lang, string text)
        {
            var targets = Transliterator.GetAvailableTargets(lang);
            foreach (string target in targets)
            {
                var variants = Transliterator.GetAvailableVariants(lang, target);
                foreach (var variant in variants)
                {
                    String id = lang + "-" + target + "/" + variant;
                    Logln("id: " + id);

                    Transliterator t = Transliterator.GetInstance(id);
                    CheckIncrementalAux(t, text);

                    String rev = t.Transliterate(text);

                    // Special treatment: This transliterator has no registered inverse, skip for now.
                    if (id.Equals("Devanagari-Arabic/"))
                        continue;

                    Transliterator inv = t.GetInverse();
                    CheckIncrementalAux(inv, rev);
                }
            }
        }
    }
}
