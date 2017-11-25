using ICU4N.Lang;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace ICU4N.Dev.Test.Collate
{
    /// <summary>
    /// UCAConformanceTest performs conformance tests defined in the data
    /// files. ICU ships with stub data files, as the whole test are too
    /// long. To do the whole test, download the test files.
    /// </summary>
    public class UCAConformanceTest : TestFmwk
    {
        public UCAConformanceTest()
        {
        }

        [SetUp]
        public void init()
        {
            UCA = (RuleBasedCollator)Collator.GetInstance(ULocale.ROOT);
            comparer = new UTF16.StringComparer(true, false, UTF16.StringComparer.FOLD_CASE_DEFAULT);
        }

        private RuleBasedCollator UCA;
        private RuleBasedCollator rbUCA;
        private UTF16.StringComparer comparer;
        private bool isAtLeastUCA62 = UCharacter.GetUnicodeVersion().CompareTo(VersionInfo.UNICODE_6_2) >= 0;

        [Test]
        public void TestTableNonIgnorable()
        {
            setCollNonIgnorable(UCA);
            openTestFile("NON_IGNORABLE");
            conformanceTest(UCA);
        }

        [Test]
        public void TestTableShifted()
        {
            setCollShifted(UCA);
            openTestFile("SHIFTED");
            conformanceTest(UCA);
        }

        [Test]
        public void TestRulesNonIgnorable()
        {
            if (logKnownIssue("cldrbug:6745", "UCARules.txt has problems"))
            {
                return;
            }
            initRbUCA();
            if (rbUCA == null)
            {
                return;
            }

            setCollNonIgnorable(rbUCA);
            openTestFile("NON_IGNORABLE");
            conformanceTest(rbUCA);
        }

        [Test]
        public void TestRulesShifted()
        {
            Logln("This test is currently disabled, as it is impossible to "
                    + "wholly represent fractional UCA using tailoring rules.");
            return;
            /*
             * initRbUCA(); if(rbUCA == null) { return; }
             *
             * setCollShifted(rbUCA); openTestFile("SHIFTED"); testConformance(rbUCA);
             */
        }

        TextReader @in;

        private void openTestFile(String type)
        {
            String collationTest = "CollationTest_";
            String ext = ".txt";
            try
            {
                @in = TestUtil.GetDataReader(collationTest + type + "_SHORT" + ext);
            }
            catch (Exception e)
            {
                try
                {
                    @in = TestUtil.GetDataReader(collationTest + type + ext);
                }
                catch (Exception e1)
                {
                    try
                    {
                        @in = TestUtil.GetDataReader(collationTest + type + "_STUB" + ext);
                        Logln("INFO: Working with the stub file.\n" + "If you need the full conformance test, please\n"
                                + "download the appropriate data files from:\n"
                                + "http://unicode.org/cldr/trac/browser/trunk/common/uca");
                    }
                    catch (Exception e11)
                    {
                        Errln("ERROR: Could not find any of the test files");
                    }
                }
            }
        }

        private void setCollNonIgnorable(RuleBasedCollator coll)
        {
            if (coll != null)
            {
                coll.Decomposition = (Collator.CANONICAL_DECOMPOSITION);
                coll.IsLowerCaseFirst = (false);
                coll.IsCaseLevel = (false);
                coll.Strength = (isAtLeastUCA62 ? Collator.IDENTICAL : Collator.TERTIARY);
                coll.IsAlternateHandlingShifted = (false);
            }
        }

        private void setCollShifted(RuleBasedCollator coll)
        {
            if (coll != null)
            {
                coll.Decomposition = (Collator.CANONICAL_DECOMPOSITION);
                coll.IsLowerCaseFirst = (false);
                coll.IsCaseLevel = (false);
                coll.Strength = (isAtLeastUCA62 ? Collator.IDENTICAL : Collator.QUATERNARY);
                coll.IsAlternateHandlingShifted = (true);
            }
        }

        private void initRbUCA()
        {
            if (rbUCA == null)
            {
                String ucarules = UCA.GetRules(true);
                try
                {
                    rbUCA = new RuleBasedCollator(ucarules);
                }
                catch (Exception e)
                {
                    Errln("Failure creating UCA rule-based collator: " + e);
                }
            }
        }

        private String parseString(String line)
        {
            int i = 0, value;
            StringBuilder result = new StringBuilder(), buffer = new StringBuilder();

            for (; ; )
            {
                while (i < line.Length && char.IsWhiteSpace(line[i]))
                {
                    i++;
                }
                while (i < line.Length && char.IsLetterOrDigit(line[i]))
                {
                    buffer.Append(line[i]);
                    i++;
                }
                if (buffer.Length == 0)
                {
                    // We hit something that was not whitespace/letter/digit.
                    // Should be ';' or end of string.
                    return result.ToString();
                }
                /* read one code point */
                value = int.Parse(buffer.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                buffer.Length = (0);
                result.AppendCodePoint(value);
            }

        }

        private static readonly int IS_SHIFTED = 1;
        private static readonly int FROM_RULES = 2;

        private static bool skipLineBecauseOfBug(String s, int flags)
        {
            // Add temporary exceptions here if there are ICU bugs, until we can fix them.
            // For examples see the ICU 52 version of this file.
            return false;
        }

        private static int normalizeResult(int result)
        {
            return result < 0 ? -1 : result == 0 ? 0 : 1;
        }

        private void conformanceTest(RuleBasedCollator coll)
        {
            if (@in == null || coll == null)
            {
                return;
            }
            int skipFlags = 0;
            if (coll.IsAlternateHandlingShifted)
            {
                skipFlags |= IS_SHIFTED;
            }
            if (coll == rbUCA)
            {
                skipFlags |= FROM_RULES;
            }

            Logln("-prop:ucaconfnosortkeys=1 turns off getSortKey() in UCAConformanceTest");
            bool withSortKeys = GetProperty("ucaconfnosortkeys") == null;

            int lineNo = 0;

            String line = null, oldLine = null, buffer = null, oldB = null;
            RawCollationKey sk1 = new RawCollationKey(), sk2 = new RawCollationKey();
            RawCollationKey oldSk = null, newSk = sk1;

            try
            {
                while ((line = @in.ReadLine()) != null)
                {
                    lineNo++;
                    if (line.Length == 0 || line[0] == '#')
                    {
                        continue;
                    }
                    buffer = parseString(line);

                    if (skipLineBecauseOfBug(buffer, skipFlags))
                    {
                        Logln("Skipping line " + lineNo + " because of a known bug");
                        continue;
                    }

                    if (withSortKeys)
                    {
                        coll.GetRawCollationKey(buffer, newSk);
                    }
                    if (oldSk != null)
                    {
                        bool ok = true;
                        int skres = withSortKeys ? oldSk.CompareTo(newSk) : 0;
                        int cmpres = coll.Compare(oldB, buffer);
                        int cmpres2 = coll.Compare(buffer, oldB);

                        if (cmpres != -cmpres2)
                        {
                            Errln(String.Format(
                                    "Compare result not symmetrical on line {0}: "
                                            + "previous vs. current ({1}) / current vs. previous ({2})",
                                    lineNo, cmpres, cmpres2));
                            ok = false;
                        }

                        // TODO: Compare with normalization turned off if the input passes the FCD test.

                        if (withSortKeys && cmpres != normalizeResult(skres))
                        {
                            Errln("Difference between coll.compare (" + cmpres + ") and sortkey compare (" + skres
                                    + ") on line " + lineNo);
                            ok = false;
                        }

                        int res = cmpres;
                        if (res == 0 && !isAtLeastUCA62)
                        {
                            // Up to UCA 6.1, the collation test files use a custom tie-breaker,
                            // comparing the raw input strings.
                            res = comparer.Compare(oldB, buffer);
                            // Starting with UCA 6.2, the collation test files use the standard UCA tie-breaker,
                            // comparing the NFD versions of the input strings,
                            // which we do via setting strength=identical.
                        }
                        if (res > 0)
                        {
                            Errln("Line " + lineNo + " is not greater or equal than previous line");
                            ok = false;
                        }

                        if (!ok)
                        {
                            Errln("  Previous data line " + oldLine);
                            Errln("  Current data line  " + line);
                            if (withSortKeys)
                            {
                                Errln("  Previous key: " + CollationTest.Prettify(oldSk));
                                Errln("  Current key:  " + CollationTest.Prettify(newSk));
                            }
                        }
                    }

                    oldSk = newSk;
                    oldB = buffer;
                    oldLine = line;
                    if (oldSk == sk1)
                    {
                        newSk = sk2;
                    }
                    else
                    {
                        newSk = sk1;
                    }
                }
            }
            catch (Exception e)
            {
                Errln("Unexpected exception " + e);
            }
            finally
            {
                try
                {
                    @in.Dispose();
                }
                catch (IOException ignored)
                {
                }
                @in = null;
            }
        }
    }
}
