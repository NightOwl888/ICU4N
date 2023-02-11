using ICU4N.Globalization;
using ICU4N.Text;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCG = J2N.Collections.Generic;
using static ICU4N.Text.PluralRules;
using ICU4N.Impl;
using Double = J2N.Numerics.Double;
using Integer = J2N.Numerics.Int32;
using System.Text.RegularExpressions;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Format
{
    /// <author>dougfelt (Doug Felt)</author>
    /// <author>markdavis (Mark Davis) [for fractional support]</author>
    public class PluralRulesTest : TestFmwk
    {
        private readonly PluralRulesFactory factory = PluralRulesFactory.Normal;

        [Test]
        public void TestOverUnderflow()
        {
            Logln(Convert.ToString(long.MaxValue + 1d, CultureInfo.InvariantCulture));
            foreach (double[] testDouble in new double[][] {
                new double[] { 1E18, 0, 0, 1E18 }, // check overflow
                new double[] { 10000000000000.1d, 1, 1, 10000000000000d }, new double[] { -0.00001d, 1, 5, 0 }, new double[] { 1d, 0, 0, 1 },
                new double[] { 1.1d, 1, 1, 1 }, new double[] { 12345d, 0, 0, 12345 }, new double[] { 12345.678912d, 678912, 6, 12345 },
                new double[] { 12345.6789123d, 678912, 6, 12345 }, // we only go out 6 digits
                new double[] { 1E18, 0, 0, 1E18 }, // check overflow
                new double[] { 1E19, 0, 0, 1E18 }, // check overflow
        })
            {
                FixedDecimal fd = new FixedDecimal(testDouble[0]);
                assertEquals(testDouble[0] + "=doubleValue()", testDouble[0], fd.ToDouble());
                assertEquals(testDouble[0] + " decimalDigits", (int)testDouble[1], fd.DecimalDigits);
                assertEquals(testDouble[0] + " visibleDecimalDigitCount", (int)testDouble[2], fd.VisibleDecimalDigitCount);
                assertEquals(testDouble[0] + " decimalDigitsWithoutTrailingZeros", (int)testDouble[1],
                        fd.DecimalDigitsWithoutTrailingZeros);
                assertEquals(testDouble[0] + " visibleDecimalDigitCountWithoutTrailingZeros", (int)testDouble[2],
                        fd.VisibleDecimalDigitCountWithoutTrailingZeros);
                assertEquals(testDouble[0] + " integerValue", (long)testDouble[3], fd.IntegerValue);
            }

            foreach (UCultureInfo locale in new UCultureInfo[] { new UCultureInfo("en"), new UCultureInfo("cy"), new UCultureInfo("ar") })
            {
                PluralRules rules = factory.ForLocale(locale);

                assertEquals(locale + " NaN", "other", rules.Select(double.NaN));
                assertEquals(locale + " ∞", "other", rules.Select(double.PositiveInfinity));
                assertEquals(locale + " -∞", "other", rules.Select(double.NegativeInfinity));
            }
        }

        [Test]
        public void TestSyntaxRestrictions()
        {
            object[][] shouldFail = new object[][] {
                new object[] { "a:n in 3..10,13..19" },

                // = and != always work
                new object[] { "a:n=1" },
                new object[] { "a:n=1,3" },
                new object[] { "a:n!=1" },
                new object[] { "a:n!=1,3" },

                // with spacing
                new object[] { "a: n = 1" },
                new object[] { "a: n = 1, 3" },
                new object[] { "a: n != 1" },
                new object[] { "a: n != 1, 3" },
                new object[] { "a: n ! = 1" },
                new object[] { "a: n ! = 1, 3" },
                new object[] { "a: n = 1 , 3" },
                new object[] { "a: n != 1 , 3" },
                new object[] { "a: n ! = 1 , 3" },
                new object[] { "a: n = 1 .. 3" },
                new object[] { "a: n != 1 .. 3" },
                new object[] { "a: n ! = 1 .. 3" },

                // more complicated
                new object[] { "a:n in 3 .. 10 , 13 .. 19" },

                // singles have special exceptions
                new object[] { "a: n is 1" },
                new object[] { "a: n is not 1" },
                new object[] { "a: n not is 1", typeof(FormatException) }, // hacked to fail
                new object[] { "a: n in 1" },
                new object[] { "a: n not in 1" },

                // multiples also have special exceptions
                // TODO enable the following once there is an update to CLDR
                // new object[] {"a: n is 1,3", FormatException)},
                new object[] { "a: n is not 1,3", typeof(FormatException) }, // hacked to fail
                new object[] { "a: n not is 1,3", typeof(FormatException) }, // hacked to fail
                new object[] { "a: n in 1,3" },
                new object[] { "a: n not in 1,3" },

                // disallow not with =
                new object[] { "a: n not= 1", typeof(FormatException) }, // hacked to fail
                new object[] { "a: n not= 1,3", typeof(FormatException) }, // hacked to fail

                // disallow double negatives
                new object[] { "a: n ! is not 1", typeof(FormatException) },
                new object[] { "a: n ! is not 1", typeof(FormatException) },
                new object[] { "a: n not not in 1", typeof(FormatException) },
                new object[] { "a: n is not not 1", typeof(FormatException) },

                // disallow screwy cases
                new object[] { null, typeof(ArgumentNullException) }, new object[] { "djkl;", typeof(FormatException) },
                new object[] { "a: n = 1 .", typeof(FormatException) }, new object[] { "a: n = 1 ..", typeof(FormatException) },
                new object[] { "a: n = 1 2", typeof(FormatException) }, new object[] { "a: n = 1 ,", typeof(FormatException) },
                new object[] { "a:n in 3 .. 10 , 13 .. 19 ,", typeof(FormatException) }, };
            foreach (object[] shouldFailTest in shouldFail)
            {
                string rules = (string)shouldFailTest[0];
                Type exception = shouldFailTest.Length < 2 ? null : (Type)shouldFailTest[1];
                Type actualException = null;
                try
                {
#if FEATURE_SPAN
                    if (rules == null)
                    {
                        // Special case: when using ReadOnlySpan<char> .NET implicitly converts null to empty, which is a valid
                        // case. So, we don't get an exception on these platforms as a result.
                        continue;
                    }
#endif
                    PluralRules.ParseDescription(rules);
                }
                catch (Exception e)
                {
                    actualException = e.GetType();
                }
                assertEquals("Exception " + rules, exception, actualException);
            }
        }

        [Test]
        // ICU4N: Added to ensure our source and context parameters are populated on TryParseDescription()
        [TestCase("a: n not is 1", "is", "n not is 1")] // Unexpected token
        [TestCase("a: n is not 1,3", "is not <range>", "n is not 1,3")]
        [TestCase("a: n not is 1,3", "is", "n not is 1,3")] // Missing token
        [TestCase("a: n not= 1", "=", "n not= 1")] // Unexpected token
        [TestCase("a: n not= 1,3", "=", "n not= 1,3")] // Unexpected token
        [TestCase("a: n ! is not 1", "is", "n ! is not 1")] // Unexpected token
        [TestCase("a: n not not in 1", "not", "n not not in 1")] // Unexpected token
        [TestCase("a: n is not not 1", "not", "n is not not 1")] // Unparsable number
        [TestCase("djkl;", null, "djkl")] // Missing colon (error message adds it)
        [TestCase("a: n = 1 .", null, "n = 1 .")] // Missing token
        [TestCase("a: n = 1 ..", null, "n = 1 ..")] // Missing token
        [TestCase("a: n = 1 2", "2", "n = 1 2")] // Unexpected token
        [TestCase("a: n = 1 ,", ",", "n = 1 ,")] // Unexpected token
        [TestCase("a:n in 3 .. 10 , 13 .. 19 ,", ",", "n in 3 .. 10 , 13 .. 19 ,")] // Unexpected token
        public void TestExceptionMessages(string rules, string expectedSource, string expectedContext)
        {
#if FEATURE_SPAN
            PluralRules.TryParseDescription(rules, out PluralRules _, out ReadOnlySpan<char> source, out ReadOnlySpan<char> context);
            assertEquals("source incorrect for " + rules, expectedSource ?? string.Empty, new string(source));
            assertEquals("context incorrect for " + rules, expectedContext ?? string.Empty, new string(context));
#else
            PluralRules.TryParseDescription(rules, out PluralRules _, out string source, out string context);
            assertEquals("source incorrect for " + rules, expectedSource, source);
            assertEquals("context incorrect for " + rules, expectedContext, context);
#endif
        }

        [Test]
        public void TestSamples()
        {
            String description = "one: n is 3 or f is 5 @integer  3,19, @decimal 3.50 ~ 3.53,   …; other:  @decimal 99.0~99.2, 999.0, …";
            PluralRules test = PluralRules.CreateRules(description);

            checkNewSamples(description, test, "one", PluralRulesSampleType.Integer, "@integer 3, 19", true,
                    new FixedDecimal(3));
            checkNewSamples(description, test, "one", PluralRulesSampleType.Decimal, "@decimal 3.50~3.53, …", false,
                    new FixedDecimal(3.5, 2));
            checkOldSamples(description, test, "one", PluralRulesSampleType.Integer, 3d, 19d);
            checkOldSamples(description, test, "one", PluralRulesSampleType.Decimal, 3.5d, 3.51d, 3.52d, 3.53d);

            checkNewSamples(description, test, "other", PluralRulesSampleType.Integer, "", true, null);
            checkNewSamples(description, test, "other", PluralRulesSampleType.Decimal, "@decimal 99.0~99.2, 999.0, …",
                    false, new FixedDecimal(99d, 1));
            checkOldSamples(description, test, "other", PluralRulesSampleType.Integer);
            checkOldSamples(description, test, "other", PluralRulesSampleType.Decimal, 99d, 99.1, 99.2d, 999d);
        }

        internal void checkOldSamples(string description, PluralRules rules, string keyword, PluralRulesSampleType sampleType,
                params double[] expected)
        {
            ICollection<double> oldSamples = rules.GetSamples(keyword, sampleType);
            if (!assertEquals("getOldSamples; " + keyword + "; " + description, new JCG.HashSet<double>(expected),
                    oldSamples))
            {
                rules.GetSamples(keyword, sampleType);
            }
        }

        internal void checkNewSamples(string description, PluralRules test, string keyword, PluralRulesSampleType sampleType,
                string samplesString, bool isBounded, FixedDecimal firstInRange)
        {
            String title = description + ", " + sampleType;
            FixedDecimalSamples samples = test.GetDecimalSamples(keyword, sampleType);
            if (samples != null)
            {
                assertEquals("samples; " + title, samplesString, samples.ToString());
                assertEquals("bounded; " + title, isBounded, samples.bounded);
                assertEquals("first; " + title, firstInRange, samples.Samples.First().start);
            }
            assertEquals("limited: " + title, isBounded, test.IsLimited(keyword, sampleType));
        }

        private static readonly string[] parseTestData = {
    "a: n is 1", "a:1", "a: n mod 10 is 2", "a:2,12,22",
            "a: n is not 1", "a:0,2,3,4,5", "a: n mod 3 is not 1", "a:0,2,3,5,6,8,9", "a: n in 2..5", "a:2,3,4,5",
            "a: n within 2..5", "a:2,3,4,5", "a: n not in 2..5", "a:0,1,6,7,8", "a: n not within 2..5", "a:0,1,6,7,8",
            "a: n mod 10 in 2..5", "a:2,3,4,5,12,13,14,15,22,23,24,25", "a: n mod 10 within 2..5",
            "a:2,3,4,5,12,13,14,15,22,23,24,25", "a: n mod 10 is 2 and n is not 12", "a:2,22,32,42",
            "a: n mod 10 in 2..3 or n mod 10 is 5", "a:2,3,5,12,13,15,22,23,25",
            "a: n mod 10 within 2..3 or n mod 10 is 5", "a:2,3,5,12,13,15,22,23,25", "a: n is 1 or n is 4 or n is 23",
            "a:1,4,23", "a: n mod 2 is 1 and n is not 3 and n in 1..11", "a:1,5,7,9,11",
            "a: n mod 2 is 1 and n is not 3 and n within 1..11", "a:1,5,7,9,11",
            "a: n mod 2 is 1 or n mod 5 is 1 and n is not 6", "a:1,3,5,7,9,11,13,15,16",
            "a: n in 2..5; b: n in 5..8; c: n mod 2 is 1", "a:2,3,4,5;b:6,7,8;c:1,9,11",
            "a: n within 2..5; b: n within 5..8; c: n mod 2 is 1", "a:2,3,4,5;b:6,7,8;c:1,9,11",
            "a: n in 2,4..6; b: n within 7..9,11..12,20", "a:2,4,5,6;b:7,8,9,11,12,20",
            "a: n in 2..8,12 and n not in 4..6", "a:2,3,7,8,12", "a: n mod 10 in 2,3,5..7 and n is not 12",
            "a:2,3,5,6,7,13,15,16,17", "a: n in 2..6,3..7", "a:2,3,4,5,6,7", };

        private string[] getTargetStrings(string targets)
        {
            List<string> list = new List<string>(50);
            String[] valSets = Utility.Split(targets, ';');
            for (int i = 0; i < valSets.Length; ++i)
            {
                String[] temp = Utility.Split(valSets[i], ':');
                String key = temp[0].Trim();
                String[] vals = Utility.Split(temp[1], ',');
                for (int j = 0; j < vals.Length; ++j)
                {
                    String valString = vals[j].Trim();
                    int val = Integer.Parse(valString, CultureInfo.InvariantCulture);
                    while (list.Count <= val)
                    {
                        list.Add(null);
                    }
                    if (list[val] != null)
                    {
                        fail("test data error, key: " + list[val] + " already set for: " + val);
                    }
                    list[val] = key;
                }
            }

            string[] result = list.ToArray();
            for (int i = 0; i < result.Length; ++i)
            {
                if (result[i] == null)
                {
                    result[i] = "other";
                }
            }
            return result;
        }

        private void checkTargets(PluralRules rules, string[] targets)
        {
            for (int i = 0; i < targets.Length; ++i)
            {
                assertEquals("value " + i, targets[i], rules.Select(i));
            }
        }

        [Test]
        public void TestParseEmpty()
        {
            PluralRules rules = PluralRules.ParseDescription("a:n");
            assertEquals("empty", "a", rules.Select(0));
        }

        [Test]
        public void TestParsing()
        {
            for (int i = 0; i < parseTestData.Length; i += 2)
            {
                String pattern = parseTestData[i];
                String expected = parseTestData[i + 1];

                Logln("pattern[" + i + "] " + pattern);
                try
                {
                    PluralRules rules = PluralRules.CreateRules(pattern);
                    String[] targets = getTargetStrings(expected);
                    checkTargets(rules, targets);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    //e.printStackTrace();
                    //throw new Exception(e.getMessage());
                    throw;
                }
            }
        }

        private static string[][] operandTestData = new string[][] { new string[] { "a: n 3", "FAIL" },
            new string[] { "a: n=1,2; b: n != 3..5; c:n!=5", "a:1,2; b:6,7; c:3,4" },
            new string[] { "a: n=1,2; b: n!=3..5; c:n!=5", "a:1,2; b:6,7; c:3,4" },
            new string[] { "a: t is 1", "a:1.1,1.1000,99.100; other:1.2,1.0" }, new string[] { "a: f is 1", "a:1.1; other:1.1000,99.100" },
            new string[] { "a: i is 2; b:i is 3", "b: 3.5; a: 2.5" }, new string[] { "a: f is 0; b:f is 50", "a: 1.00; b: 1.50" },
            new string[] { "a: v is 1; b:v is 2", "a: 1.0; b: 1.00" }, new string[] { "one: n is 1 AND v is 0", "one: 1 ; other: 1.00,1.0" }, // English
                                                                                                                    // rules
            new string[] { "one: v is 0 and i mod 10 is 1 or f mod 10 is 1", "one: 1, 1.1, 3.1; other: 1.0, 3.2, 5" }, // Last
                                                                                                          // visible
                                                                                                          // digit
            new string[] { "one: j is 0", "one: 0; other: 0.0, 1.0, 3" }, // Last visible digit
    // one → n is 1; few → n in 2..4;
    };

        [Test]
        public void TestOperands()
        {
            foreach (String[] pair in operandTestData)
            {
                String pattern = pair[0].Trim();
                String categoriesAndExpected = pair[1].Trim();

                // Logln("pattern[" + i + "] " + pattern);
                bool FAIL_EXPECTED = categoriesAndExpected.Equals("fail", StringComparison.OrdinalIgnoreCase);
                try
                {
                    Logln(pattern);
                    PluralRules rules = PluralRules.CreateRules(pattern);
                    if (FAIL_EXPECTED)
                    {
                        assertNull("Should fail with 'null' return.", rules);
                    }
                    else
                    {
                        Logln(rules == null ? "null rules" : rules.ToString());
                        checkCategoriesAndExpected(pattern, categoriesAndExpected, rules);
                    }
                }
                catch (Exception e)
                {
                    if (!FAIL_EXPECTED)
                    {
                        Console.WriteLine(e.ToString());
                        throw;
                        //e.printStackTrace();
                        //throw new RuntimeException(e.getMessage());
                    }
                }
            }
        }

        [Test]
        public void TestUniqueRules()
        {
            //main: 
            foreach (UCultureInfo locale in factory.GetUCultures())
            {
                PluralRules rules = factory.ForLocale(locale);
                IDictionary<String, PluralRules> keywordToRule = new Dictionary<String, PluralRules>();
                ICollection<FixedDecimalSamples> samples = new JCG.LinkedHashSet<FixedDecimalSamples>();

                foreach (String keyword in rules.Keywords)
                {
                    foreach (PluralRulesSampleType sampleType in Enum.GetValues(typeof(PluralRulesSampleType)))
                    {
                        FixedDecimalSamples samples2 = rules.GetDecimalSamples(keyword, sampleType);
                        if (samples2 != null)
                        {
                            samples.Add(samples2);
                        }
                    }
                    if (keyword.Equals("other", StringComparison.Ordinal))
                    {
                        continue;
                    }
                    String rules2 = keyword + ":" + rules.GetRules(keyword);
                    PluralRules singleRule = PluralRules.CreateRules(rules2);
                    if (singleRule == null)
                    {
                        Errln("Can't generate single rule for " + rules2);
                        PluralRules.CreateRules(rules2); // for debugging
                                                         //continue main;
                        goto main_continue;
                    }
                    keywordToRule[keyword] = singleRule;
                main_continue: { /* intentionally empty */ }
                }
                IDictionary<FixedDecimal, string> collisionTest = new JCG.SortedDictionary<FixedDecimal, string>();
                foreach (FixedDecimalSamples sample3 in samples)
                {
                    ICollection<FixedDecimalRange> samples2 = sample3.Samples;
                    if (samples2 == null)
                    {
                        continue;
                    }
                    foreach (FixedDecimalRange sample in samples2)
                    {
                        for (int i = 0; i < 1; ++i)
                        {
                            FixedDecimal item = i == 0 ? sample.start : sample.end;
                            collisionTest.Clear();
                            foreach (var entry in keywordToRule)
                            {
                                PluralRules rule = entry.Value;
                                String foundKeyword = rule.Select(item);
                                if (foundKeyword.Equals("other", StringComparison.Ordinal))
                                {
                                    continue;
                                }
                                if (collisionTest.TryGetValue(item, out string old) || old != null)
                                {
                                    Errln(locale + "\tNon-unique rules: " + item + " => " + old + " & " + foundKeyword);
                                    rule.Select(item);
                                }
                                else
                                {
                                    collisionTest[item] = foundKeyword;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void checkCategoriesAndExpected(String title1, String categoriesAndExpected, PluralRules rules)
        {
            foreach (String categoryAndExpected in Regex.Split(categoriesAndExpected, "\\s*;\\s*"))
            {
                String[] categoryFromExpected = Regex.Split(categoryAndExpected, "\\s*:\\s*");
                String expected = categoryFromExpected[0];
                foreach (String value in Regex.Split(categoryFromExpected[1], "\\s*,\\s*"))
                {
                    if (value.StartsWith("@", StringComparison.Ordinal) || value.Equals("…", StringComparison.Ordinal) || value.Equals("null", StringComparison.Ordinal))
                    {
                        continue;
                    }
                    String[] values = Regex.Split(value, "\\s*~\\s*");
                    checkValue(title1, rules, expected, values[0]);
                    if (values.Length > 1)
                    {
                        checkValue(title1, rules, expected, values[1]);
                    }
                }
            }
        }

        public void checkValue(String title1, PluralRules rules, String expected, String value)
        {
            double number = Double.Parse(value, CultureInfo.InvariantCulture);
            int decimalPos = value.IndexOf('.') + 1;
            int countVisibleFractionDigits;
            int fractionaldigits;
            if (decimalPos == 0)
            {
                countVisibleFractionDigits = fractionaldigits = 0;
            }
            else
            {
                countVisibleFractionDigits = value.Length - decimalPos;
                fractionaldigits = Integer.Parse(value.Substring(decimalPos), CultureInfo.InvariantCulture);
            }
            String result = rules.Select(number, countVisibleFractionDigits, fractionaldigits);
            UCultureInfo locale = null;
            assertEquals(getAssertMessage(title1, locale, rules, expected) + "; value: " + value, expected, result);
        }

        private static string[][] equalityTestData = new string[][] {
            // once we add fractions, we had to retract the "test all possibilities" for equality,
            // so we only have a limited set of equality tests now.
            new string[] { "c: n%11!=5", "c: n mod 11 is not 5" }, new string[] { "c: n is not 7", "c: n != 7" }, new string[] { "a:n in 2;", "a: n = 2" },
            new string[] { "b:n not in 5;", "b: n != 5" },

            // new string[] { "a: n is 5",
            // "a: n in 2..6 and n not in 2..4 and n is not 6" },
            // new string[] { "a: n in 2..3",
            // "a: n is 2 or n is 3",
            // "a: n is 3 and n in 2..5 or n is 2" },
            // new string[] { "a: n is 12; b:n mod 10 in 2..3",
            // "b: n mod 10 in 2..3 and n is not 12; a: n in 12..12",
            // "b: n is 13; a: n is 12; b: n mod 10 is 2 or n mod 10 is 3" },
        };

        private static string[][] inequalityTestData = new string[][] { new string[] { "a: n mod 8 is 3", "a: n mod 7 is 3" },
            new string[] { "a: n mod 3 is 2 and n is not 5", "a: n mod 6 is 2 or n is 8 or n is 11" },
            // the following are currently inequal, but we may make them equal in the future.
            new string[] { "a: n in 2..5", "a: n in 2..4,5" }, };

        private void compareEquality(String id, Object[] objects, bool shouldBeEqual)
        {
            for (int i = 0; i < objects.Length; ++i)
            {
                Object lhs = objects[i];
                int start = shouldBeEqual ? i : i + 1;
                for (int j = start; j < objects.Length; ++j)
                {
                    Object rhs = objects[j];
                    if (rhs == null || shouldBeEqual != lhs.Equals(rhs))
                    {
                        String msg = shouldBeEqual ? "should be equal" : "should not be equal";
                        fail(id + " " + msg + " (" + i + ", " + j + "):\n    " + lhs + "\n    " + rhs);
                    }
                    // assertEquals("obj " + i + " and " + j, lhs, rhs);
                }
            }
        }

        private void compareEqualityTestSets(String[][] sets, bool shouldBeEqual)
        {
            for (int i = 0; i < sets.Length; ++i)
            {
                String[] patterns = sets[i];
                PluralRules[] rules = new PluralRules[patterns.Length];
                for (int j = 0; j < patterns.Length; ++j)
                {
                    rules[j] = PluralRules.CreateRules(patterns[j]);
                }
                compareEquality("test " + i, rules, shouldBeEqual);
            }
        }

        [Test]
        public void TestEquality()
        {
            compareEqualityTestSets(equalityTestData, true);
        }

        [Test]
        public void TestInequality()
        {
            compareEqualityTestSets(inequalityTestData, false);
        }

        [Test]
        public void TestBuiltInRules()
        {
            // spot check
            PluralRules rules = factory.ForLocale(new UCultureInfo("en-US"));
            assertEquals("us 0", PluralRules.KeywordOther, rules.Select(0));
            assertEquals("us 1", PluralRules.KeywordOne, rules.Select(1));
            assertEquals("us 2", PluralRules.KeywordOther, rules.Select(2));

            rules = factory.ForLocale(new UCultureInfo("ja-JP"));
            assertEquals("ja 0", PluralRules.KeywordOther, rules.Select(0));
            assertEquals("ja 1", PluralRules.KeywordOther, rules.Select(1));
            assertEquals("ja 2", PluralRules.KeywordOther, rules.Select(2));

            rules = factory.ForLocale(UCultureInfo.CreateCanonical("ru"));
            assertEquals("ru 0", PluralRules.KeywordMany, rules.Select(0));
            assertEquals("ru 1", PluralRules.KeywordOne, rules.Select(1));
            assertEquals("ru 2", PluralRules.KeywordFew, rules.Select(2));
        }

        [Test]
        public void TestFunctionalEquivalent()
        {
            // spot check
            UCultureInfo unknown = UCultureInfo.CreateCanonical("zz_ZZ");
            UCultureInfo un_equiv = PluralRules.GetFunctionalEquivalent(unknown);
            assertEquals("unknown locales have root", UCultureInfo.InvariantCulture, un_equiv);

            UCultureInfo jp_equiv = PluralRules.GetFunctionalEquivalent(new UCultureInfo("ja-JP"));
            UCultureInfo cn_equiv = PluralRules.GetFunctionalEquivalent(new UCultureInfo("zh_Hans_CN"));
            assertEquals("japan and china equivalent locales", jp_equiv, cn_equiv);

            UCultureInfo russia = UCultureInfo.CreateCanonical("ru_RU");
            UCultureInfo ru_ru_equiv = PluralRules.GetFunctionalEquivalent(russia, out bool available);
            assertFalse("ru_RU not listed", available);

            UCultureInfo russian = UCultureInfo.CreateCanonical("ru");
            UCultureInfo ru_equiv = PluralRules.GetFunctionalEquivalent(russian, out available);
            assertTrue("ru listed", available);
            assertEquals("ru and ru_RU equivalent locales", ru_ru_equiv, ru_equiv);
        }

        [Test]
        public void TestAvailableULocales()
        {
            UCultureInfo[] locales = factory.GetUCultures();
            ISet<UCultureInfo> localeSet = new HashSet<UCultureInfo>();
            localeSet.UnionWith(locales);

            assertEquals("locales are unique in list", locales.Length, localeSet.Count);
        }

        /*
         * Test the method public static PluralRules parseDescription(String description)
         */
        [Test]
        public void TestParseDescription()
        {
            try
            {
                if (PluralRules.Default != PluralRules.ParseDescription(""))
                {
                    Errln("PluralRules.parseDescription(String) was suppose "
                            + "to return PluralRules.DEFAULT when String is of " + "length 0.");
                }
            }
            catch (Exception e)
            {
                Errln("PluralRules.parseDescription(String) was not suppose " + "to return an exception.");
            }
        }

        /*
         * Tests the method public static PluralRules createRules(String description)
         */
        [Test]
        public void TestCreateRules()
        {
            try
            {
                if (PluralRules.CreateRules(null) != null)
                {
                    Errln("PluralRules.createRules(String) was suppose to "
                            + "return null for an invalid String descrtiption.");
                }
            }
            catch (Exception e)
            {
            }
        }

        /*
         * Tests the method public int hashCode()
         */
        [Test]
        public void TestHashCode()
        {
            // Bad test, breaks whenever PluralRules implementation changes.
            // PluralRules pr = PluralRules.DEFAULT;
            // if (106069776 != pr.hashCode()) {
            // Errln("PluralRules.hashCode() was suppose to return 106069776 " + "when PluralRules.DEFAULT.");
            // }
        }

        /*
         * Tests the method public boolean equals(PluralRules rhs)
         */
        [Test]
        public void TestEquals()
        {
            PluralRules pr = PluralRules.Default;

            if (pr.Equals((PluralRules)null))
            {
                Errln("PluralRules.equals(PluralRules) was supposed to return false " + "when passing null.");
            }
        }

        private void assertRuleValue(String rule, double value)
        {
            assertRuleKeyValue("a:" + rule, "a", value);
        }

        private void assertRuleKeyValue(String rule, String key, double value)
        {
            PluralRules pr = PluralRules.CreateRules(rule);
            assertEquals(rule, value, pr.GetUniqueKeywordValue(key));
        }

        /*
         * Tests getUniqueKeywordValue()
         */
        [Test]
        public void TestGetUniqueKeywordValue()
        {
            assertRuleKeyValue("a: n is 1", "not_defined", PluralRules.NoUniqueValue); // key not defined
            assertRuleValue("n within 2..2", 2);
            assertRuleValue("n is 1", 1);
            assertRuleValue("n in 2..2", 2);
            assertRuleValue("n in 3..4", PluralRules.NoUniqueValue);
            assertRuleValue("n within 3..4", PluralRules.NoUniqueValue);
            assertRuleValue("n is 2 or n is 2", 2);
            assertRuleValue("n is 2 and n is 2", 2);
            assertRuleValue("n is 2 or n is 3", PluralRules.NoUniqueValue);
            assertRuleValue("n is 2 and n is 3", PluralRules.NoUniqueValue);
            assertRuleValue("n is 2 or n in 2..3", PluralRules.NoUniqueValue);
            assertRuleValue("n is 2 and n in 2..3", 2);
            assertRuleKeyValue("a: n is 1", "other", PluralRules.NoUniqueValue); // key matches default rule
            assertRuleValue("n in 2,3", PluralRules.NoUniqueValue);
            assertRuleValue("n in 2,3..6 and n not in 2..3,5..6", 4);
        }

        /**
         * The version in PluralFormatUnitTest is not really a test, and it's in the wrong place anyway, so I'm putting a
         * variant of it here.
         */
        [Test]
        public void TestGetSamples()
        {
            ISet<UCultureInfo> uniqueRuleSet = new HashSet<UCultureInfo>();
            foreach (UCultureInfo locale in factory.GetUCultures())
            {
                uniqueRuleSet.Add(PluralRules.GetFunctionalEquivalent(locale));
            }
            foreach (UCultureInfo locale in uniqueRuleSet)
            {
                PluralRules rules = factory.ForLocale(locale);
                Logln("\nlocale: " + (locale == UCultureInfo.InvariantCulture ? "root" : locale.ToString()) + ", rules: " + rules);
                ICollection<String> keywords = rules.Keywords;
                foreach (String keyword in keywords)
                {
                    ICollection<double> list = rules.GetSamples(keyword);
                    Logln("keyword: " + keyword + ", samples: " + list);
                    // with fractions, the samples can be empty and thus the list null. In that case, however, there will be
                    // FixedDecimal values.
                    // So patch the test for that.
                    if (list.Count == 0)
                    {
                        // when the samples (meaning integer samples) are null, then then integerSamples must be, and the
                        // decimalSamples must not be
                        FixedDecimalSamples integerSamples = rules.GetDecimalSamples(keyword, PluralRulesSampleType.Integer);
                        FixedDecimalSamples decimalSamples = rules.GetDecimalSamples(keyword, PluralRulesSampleType.Decimal);
                        assertTrue(getAssertMessage("List is not null", locale, rules, keyword), integerSamples == null
                                && decimalSamples != null && decimalSamples.Samples.Count != 0);
                    }
                    else
                    {
                        if (!assertTrue(getAssertMessage("Test getSamples.isEmpty", locale, rules, keyword),
                                list.Count > 0))
                        {
                            rules.GetSamples(keyword);
                        }
                        if (rules.ToString().Contains(": j"))
                        {
                            // hack until we remove j
                        }
                        else
                        {
                            foreach (double value in list)
                            {
                                assertEquals(getAssertMessage("Match keyword", locale, rules, keyword) + "; value '"
                                        + value + "'", keyword, rules.Select(value));
                            }
                        }
                    }
                }

                assertNull(locale + ", list is null", rules.GetSamples("@#$%^&*"));
                assertNull(locale + ", list is null", rules.GetSamples("@#$%^&*", PluralRulesSampleType.Decimal));
            }
        }

        public String getAssertMessage(String message, UCultureInfo locale, PluralRules rules, String keyword)
        {
            String ruleString = "";
            if (keyword != null)
            {
                if (keyword.Equals("other", StringComparison.Ordinal))
                {
                    foreach (String keyword2 in rules.Keywords)
                    {
                        ruleString += " NOR " + rules.GetRules(keyword2).Split('@')[0];
                    }
                }
                else
                {
                    String rule = rules.GetRules(keyword);
                    ruleString = rule == null ? null : rule.Split('@')[0];
                }
                ruleString = "; rule: '" + keyword + ": " + ruleString + "'";
                // !keyword.equals("other") ? "'; keyword: '" + keyword + "'; rule: '" + rules.getRules(keyword) + "'"
                // : "'; keyword: '" + keyword + "'; rules: '" + rules.toString() + "'";
            }
            return message + (locale == null ? "" : "; locale: '" + locale + "'") + ruleString;
        }

        /**
         * Returns the empty set if the keyword is not defined, null if there are an unlimited number of values for the
         * keyword, or the set of values that trigger the keyword.
         */
        [Test]
        public void TestGetAllKeywordValues()
        {
            // data is pairs of strings, the rule, and the expected values as arguments
            String[] data = {
                "other: ; a: n mod 3 is 0",
                "a: null",
                "a: n in 2..5 and n within 5..8",
                "a: 5",
                "a: n in 2..5",
                "a: 2,3,4,5; other: null",
                "a: n not in 2..5",
                "a: null; other: null",
                "a: n within 2..5",
                "a: 2,3,4,5; other: null",
                "a: n not within 2..5",
                "a: null; other: null",
                "a: n in 2..5 or n within 6..8",
                "a: 2,3,4,5,6,7,8", // ignore 'other' here on out, always null
                "a: n in 2..5 and n within 6..8",
                "a: null",
                // we no longer support 'degenerate' rules
                // "a: n within 2..5 and n within 6..8", "a:", // our sampling catches these
                // "a: n within 2..5 and n within 5..8", "a: 5", // ''
                // "a: n within 1..2 and n within 2..3 or n within 3..4 and n within 4..5", "a: 2,4",
                // "a: n mod 3 is 0 and n within 0..5", "a: 0,3",
                "a: n within 1..2 and n within 2..3 or n within 3..4 and n within 4..5 or n within 5..6 and n within 6..7",
                "a: 2,4,6", // but not this...
                "a: n mod 3 is 0 and n within 1..2", "a: null", "a: n mod 3 is 0 and n within 0..6", "a: 0,3,6",
                "a: n mod 3 is 0 and n in 3..12", "a: 3,6,9,12", "a: n in 2,4..6 and n is not 5", "a: 2,4,6", };

            for (int i = 0; i < data.Length; i += 2)
            {
                String ruleDescription = data[i];
                String result = data[i + 1];

                PluralRules p = PluralRules.CreateRules(ruleDescription);
                if (p == null)
                { // for debugging
                    PluralRules.CreateRules(ruleDescription);
                }
                foreach (String ruleResult in result.Split(';').TrimEnd())
                {
                    String[] ruleAndValues = ruleResult.Split(':').TrimEnd();
                    String keyword = ruleAndValues[0].Trim();
                    String valueList = ruleAndValues.Length < 2 ? null : ruleAndValues[1];
                    if (valueList != null)
                    {
                        valueList = valueList.Trim();
                    }
                    ICollection<double> values;
                    if (valueList == null || valueList.Length == 0)
                    {
                        values = new HashSet<double>(); //Collections.EMPTY_SET;
                    }
                    else if ("null".Equals(valueList, StringComparison.Ordinal))
                    {
                        values = null;
                    }
                    else
                    {
                        values = new JCG.SortedSet<double>();
                        foreach (String value in valueList.Split(',').TrimEnd())
                        {
                            values.Add(Double.Parse(value, CultureInfo.InvariantCulture));
                        }
                    }

                    ICollection<double> results = p.GetAllKeywordValues(keyword);
                    assertEquals(keyword + " in " + ruleDescription, (ISet<double>)values, results == null ? null : new HashSet<double>(results));

                    if (results != null)
                    {
                        try
                        {
                            results.Add(PluralRules.NoUniqueValue);
                            fail("returned set is modifiable");
                        }
                        catch (NotSupportedException e)
                        {
                            // pass
                        }
                    }
                }
            }
        }

        [Test]
        public void TestOrdinal()
        {
            PluralRules pr = factory.ForLocale(new UCultureInfo("en"), PluralType.Ordinal);
            assertEquals("PluralRules(en-ordinal).select(2)", "two", pr.Select(2));
        }

        [Test]
        public void TestBasicFraction()
        {
            string[][] tests = new string[][] { new string[] { "en", "one: j is 1" }, new string[] { "1", "0", "1", "one" }, new string[] { "1", "2", "1.00", "other" }, };
            UCultureInfo locale = null;
            NumberFormat nf = null;
            PluralRules pr = null;

            foreach (String[] row in tests)
            {
                switch (row.Length)
                {
                    case 2:
                        locale = UCultureInfo.GetCultureInfoByIetfLanguageTag(row[0]);
                        nf = NumberFormat.GetInstance(locale);
                        pr = PluralRules.CreateRules(row[1]);
                        break;
                    case 4:
                        double n = Double.Parse(row[0], CultureInfo.InvariantCulture);
                        int minFracDigits = Integer.Parse(row[1], CultureInfo.InvariantCulture);
                        nf.MinimumFractionDigits = (minFracDigits);
                        String expectedFormat = row[2];
                        String expectedKeyword = row[3];

                        UFieldPosition pos = new UFieldPosition();
                        String formatted = nf.Format(1.0, new StringBuffer(), pos).ToString();
                        int countVisibleFractionDigits = pos.CountVisibleFractionDigits;
                        long fractionDigits = pos.FractionDigits;
                        String keyword = pr.Select(n, countVisibleFractionDigits, fractionDigits);
                        assertEquals("Formatted " + n + "\t" + minFracDigits, expectedFormat, formatted);
                        assertEquals("Keyword " + n + "\t" + minFracDigits, expectedKeyword, keyword);
                        break;
                    default:
                        throw new Exception();
                }
            }
        }

        [Test]
        public void TestLimitedAndSamplesConsistency()
        {
            foreach (UCultureInfo locale in PluralRules.GetUCultures())
            {
                UCultureInfo loc2 = PluralRules.GetFunctionalEquivalent(locale);
                if (!loc2.Equals(locale))
                {
                    continue; // only need "unique" rules
                }
                foreach (PluralType type in Enum.GetValues(typeof(PluralType)))
                {
                    PluralRules rules = PluralRules.ForLocale(locale, type);
                    foreach (PluralRulesSampleType sampleType in Enum.GetValues(typeof(PluralRulesSampleType)))
                    {
                        if (type == PluralType.Ordinal)
                        {
                            logKnownIssue("10783", "Fix issues with isLimited vs computeLimited on ordinals");
                            continue;
                        }
                        foreach (String keyword in rules.Keywords)
                        {
                            bool isLimited = rules.IsLimited(keyword, sampleType);
                            bool computeLimited = rules.ComputeLimited(keyword, sampleType);
                            if (!keyword.Equals("other"))
                            {
                                assertEquals(getAssertMessage("computeLimited == isLimited", locale, rules, keyword),
                                        computeLimited, isLimited);
                            }
                            ICollection<double> samples = rules.GetSamples(keyword, sampleType);
                            assertNotNull(getAssertMessage("Samples must not be null", locale, rules, keyword), samples);
                            /* FixedDecimalSamples decimalSamples = */
                            rules.GetDecimalSamples(keyword, sampleType);
                            // assertNotNull(getAssertMessage("Decimal samples must be null if unlimited", locale, rules,
                            // keyword), decimalSamples);
                        }
                    }
                }
            }
        }

        [Test]
        public void TestKeywords()
        {
            ISet<string> possibleKeywords = new JCG.LinkedHashSet<string>(new string[] { "zero", "one", "two", "few", "many", "other" });
            Object[][][] tests = new object[][][] {
                // format is locale, explicits, then triples of keyword, status, unique value.
                new object[][] { new object[] { "en", null }, new object[] { "one", PluralRulesKeywordStatus.Unique, 1.0d }, new object[] { "other", PluralRulesKeywordStatus.Unbounded, null } },
                new object[][]{ new object[] { "pl", null },new object[] { "one", PluralRulesKeywordStatus.Unique, 1.0d },new object[] { "few", PluralRulesKeywordStatus.Unbounded, null },
                       new object[] { "many", PluralRulesKeywordStatus.Unbounded, null },
                       new object[] { "other", PluralRulesKeywordStatus.Suppressed, null, PluralRulesKeywordStatus.Unbounded, null } // note that it is
                                                                                                   // suppressed in
                                                                                                   // INTEGER but not
                                                                                                   // DECIMAL
                },new object[][] { new object[] { "en", new HashSet<double>(new double[] { 1.0d }) }, // check that 1 is suppressed
                       new object[] { "one", PluralRulesKeywordStatus.Suppressed, null },new object[]  { "other", PluralRulesKeywordStatus.Unbounded, null } }, };
            double? uniqueValue = null;
            foreach (Object[][] test in tests)
            {
                UCultureInfo locale = new UCultureInfo((String)test[0][0]);
                // NumberType numberType = (NumberType) test[1];
                ISet<double> explicits = (ISet<double>)test[0][1];
                PluralRules pluralRules = factory.ForLocale(locale);
                JCG.LinkedHashSet<string> remaining = new JCG.LinkedHashSet<string>(possibleKeywords);
                for (int i = 1; i < test.Length; ++i)
                {
                    object[] row = test[i];
                    string keyword = (string)row[0];
                    PluralRulesKeywordStatus statusExpected = (PluralRulesKeywordStatus)row[1];
                    double? uniqueExpected = (double?)row[2];
                    remaining.Remove(keyword);
                    PluralRulesKeywordStatus status = pluralRules.GetKeywordStatus(keyword, 0, explicits, out uniqueValue);
                    assertEquals(getAssertMessage("Unique Value", locale, pluralRules, keyword), uniqueExpected.GetValueOrDefault(),
                            uniqueValue.GetValueOrDefault());
                    assertEquals(getAssertMessage("Keyword Status", locale, pluralRules, keyword), statusExpected, status);
                    if (row.Length > 3)
                    {
                        statusExpected = (PluralRulesKeywordStatus)row[3];
                        uniqueExpected = (double?)row[4];
                        status = pluralRules.GetKeywordStatus(keyword, 0, explicits, out uniqueValue, PluralRulesSampleType.Decimal);
                        assertEquals(getAssertMessage("Unique Value - decimal", locale, pluralRules, keyword),
                                uniqueExpected.GetValueOrDefault(), uniqueValue.GetValueOrDefault());
                        assertEquals(getAssertMessage("Keyword Status - decimal", locale, pluralRules, keyword),
                                statusExpected, status);
                    }
                }
                foreach (String keyword in remaining)
                {
                    PluralRulesKeywordStatus status = pluralRules.GetKeywordStatus(keyword, 0, null, out uniqueValue);
                    assertEquals("Invalid keyword " + keyword, status, PluralRulesKeywordStatus.Invalid);
                    assertFalse("Invalid keyword " + keyword, uniqueValue.HasValue);
                }
            }
        }

        // ICU4N: de-nested StandardPluralCategories
        //        }

        //    enum StandardPluralCategories
        //{
        //    zero, one, two, few, many, other;
        //        /**
        //         *
        //         */
        //        private static final Set<StandardPluralCategories> ALL = Collections.unmodifiableSet(EnumSet
        //                .allOf(StandardPluralCategories.class));

        ///**
        // * Return a mutable set
        // *
        // * @param source
        // * @return
        // */
        //static final EnumSet<StandardPluralCategories> getSet(Collection<String> source) {

        //    EnumSet<StandardPluralCategories> result = EnumSet.noneOf(StandardPluralCategories.class);
        //for (String s : source)
        //{
        //    result.add(StandardPluralCategories.valueOf(s));
        //}
        //return result;
        //        }

        //        static final Comparator<Set<StandardPluralCategories>> SHORTEST_FIRST = new Comparator<Set<StandardPluralCategories>>() {
        //            @Override
        //            public int compare(Set<StandardPluralCategories> arg0, Set<StandardPluralCategories> arg1)
        //{
        //    int diff = arg0.size() - arg1.size();
        //    if (diff != 0)
        //    {
        //        return diff;
        //    }
        //    // otherwise first...
        //    // could be optimized, but we don't care here.
        //    for (StandardPluralCategories value : ALL) {
        //    if (arg0.contains(value))
        //    {
        //        if (!arg1.contains(value))
        //        {
        //            return 1;
        //        }
        //    }
        //    else if (arg1.contains(value))
        //    {
        //        return -1;
        //    }

        //}
        //return 0;
        //            }

        //        };
        //    }

        [Test]
        public void TestLocales()
        {
            // ICU4N TODO: issues with generic porting
            //if (false)
            //{
            //    generateLOCALE_SNAPSHOT();
            //}
            foreach (String test1 in LOCALE_SNAPSHOT)
            {
                var test = test1.Trim();
                String[] parts = Regex.Split(test, "\\s*;\\s*");
                foreach (String localeString in Regex.Split(parts[0], "\\s*,\\s*"))
                {
                    UCultureInfo locale = new UCultureInfo(localeString);
                    if (factory.HasOverride(locale))
                    {
                        continue; // skip for now
                    }
                    PluralRules rules = factory.ForLocale(locale);
                    for (int i = 1; i < parts.Length; ++i)
                    {
                        checkCategoriesAndExpected(localeString, parts[i], rules);
                    }
                }
            }
        }

        private static IComparer<PluralRules> PLURAL_RULE_COMPARATOR = Comparer<PluralRules>.Create((o1, o2) => o1.CompareTo(o2));//  new Comparator<PluralRules>() {
                                                                                                                                  //        @Override
                                                                                                                                  //        public int compare(PluralRules o1, PluralRules o2)
                                                                                                                                  //{
                                                                                                                                  //    return o1.compareTo(o2);
                                                                                                                                  //}
                                                                                                                                  //    };

        // ICU4N TODO: issues with generic porting
        //private void generateLOCALE_SNAPSHOT()
        //{
        //    //Comparator c = new CollectionUtilities.CollectionComparator<Comparable>();
        //    //var c = JCG.SetEqualityComparer<StandardPluralCategories>.Default;
        //    //var c = Comparer<ISet<StandardPluralCategories>>.Default;
        //    var c = new CollectionUtilities.SetComparer<PluralRules>();
        //    Relation<ISet<StandardPluralCategories>, PluralRules> setsToRules = Relation.Of(
        //            new JCG.SortedDictionary<ISet<StandardPluralCategories>, ISet<PluralRules>>(c), typeof(JCG.SortedDictionary<ISet<StandardPluralCategories>, ISet<PluralRules>>), PLURAL_RULE_COMPARATOR);
        //    Relation<PluralRules, UCultureInfo> data = Relation.Of(
        //            new JCG.SortedDictionary<PluralRules, ISet<UCultureInfo>>(PLURAL_RULE_COMPARATOR), typeof(JCG.SortedDictionary<PluralRules, ISet<UCultureInfo>>));
        //    foreach (UCultureInfo locale in PluralRules.GetUCultures())
        //    {
        //        PluralRules pr = PluralRules.ForLocale(locale);
        //        ISet<StandardPluralCategories> set = getCanonicalSet(pr.Keywords);
        //        setsToRules.Put(set, pr);
        //        data.Put(pr, locale);
        //    }
        //    foreach (var entry1 in setsToRules.KeyValues)
        //    {
        //        ISet<StandardPluralCategories> set = entry1.Key;
        //        ISet<PluralRules> rules = entry1.Value;
        //        Console.Out.WriteLine("\n        // " + set);
        //        foreach (PluralRules rule in rules)
        //        {
        //            ISet<UCultureInfo> locales = data[rule];
        //            Console.Out.Write("        \"" + string.Join(",", locales.Select(l => l.ToString())));
        //            foreach (StandardPluralCategories spc in set)
        //            {
        //                String keyword = spc.ToString();
        //                FixedDecimalSamples samples = rule.GetDecimalSamples(keyword, PluralRulesSampleType.Integer);
        //                Console.Out.Write("; " + spc + ": " + samples);
        //            }
        //            Console.Out.WriteLine("\",");
        //        }
        //    }
        //}

        /**
         * @param keywords
         * @return
         */
        private ISet<StandardPluralCategories> getCanonicalSet(ICollection<string> keywords)
        {
            ISet<StandardPluralCategories> result = new HashSet<StandardPluralCategories>();// EnumSet.noneOf(StandardPluralCategories.class);
            foreach (String s in keywords)
            {
                result.Add((StandardPluralCategories)Enum.Parse(typeof(StandardPluralCategories),s, ignoreCase: true));
            }
            return result;
        }

        static string[] LOCALE_SNAPSHOT = new string[] {
            // [other]
            "bm,bo,dz,id,ig,ii,in,ja,jbo,jv,jw,kde,kea,km,ko,lkt,lo,ms,my,nqo,root,sah,ses,sg,th,to,vi,wo,yo,zh; other: @integer 0~15, 100, 1000, 10000, 100000, 1000000, …",

            // [one, other]
            "am,bn,fa,gu,hi,kn,mr,zu; one: @integer 0, 1; other: @integer 2~17, 100, 1000, 10000, 100000, 1000000, …",
            "ff,fr,hy,kab; one: @integer 0, 1; other: @integer 2~17, 100, 1000, 10000, 100000, 1000000, …",
            "ast,ca,de,en,et,fi,fy,gl,it,ji,nl,sv,sw,ur,yi; one: @integer 1; other: @integer 0, 2~16, 100, 1000, 10000, 100000, 1000000, …",
            "pt; one: @integer 1; other: @integer 0, 2~16, 100, 1000, 10000, 100000, 1000000, …",
            "si; one: @integer 0, 1; other: @integer 2~17, 100, 1000, 10000, 100000, 1000000, …",
            "ak,bh,guw,ln,mg,nso,pa,ti,wa; one: @integer 0, 1; other: @integer 2~17, 100, 1000, 10000, 100000, 1000000, …",
            "tzm; one: @integer 0, 1, 11~24; other: @integer 2~10, 100~106, 1000, 10000, 100000, 1000000, …",
            "af,asa,az,bem,bez,bg,brx,cgg,chr,ckb,dv,ee,el,eo,es,eu,fo,fur,gsw,ha,haw,hu,jgo,jmc,ka,kaj,kcg,kk,kkj,kl,ks,ksb,ku,ky,lb,lg,mas,mgo,ml,mn,nah,nb,nd,ne,nn,nnh,no,nr,ny,nyn,om,or,os,pap,ps,rm,rof,rwk,saq,seh,sn,so,sq,ss,ssy,st,syr,ta,te,teo,tig,tk,tn,tr,ts,ug,uz,ve,vo,vun,wae,xh,xog; one: @integer 1; other: @integer 0, 2~16, 100, 1000, 10000, 100000, 1000000, …",
            "pt_PT; one: @integer 1; other: @integer 0, 2~16, 100, 1000, 10000, 100000, 1000000, …",
            "da; one: @integer 1; other: @integer 0, 2~16, 100, 1000, 10000, 100000, 1000000, …",
            "is; one: @integer 1, 21, 31, 41, 51, 61, 71, 81, 101, 1001, …; other: @integer 0, 2~16, 100, 1000, 10000, 100000, 1000000, …",
            "mk; one: @integer 1, 11, 21, 31, 41, 51, 61, 71, 101, 1001, …; other: @integer 0, 2~10, 12~17, 100, 1000, 10000, 100000, 1000000, …",
            "fil,tl; one: @integer 0~3, 5, 7, 8, 10~13, 15, 17, 18, 20, 21, 100, 1000, 10000, 100000, 1000000, …; other: @integer 4, 6, 9, 14, 16, 19, 24, 26, 104, 1004, …",

            // [zero, one, other]
            "lag; zero: @integer 0; one: @integer 1; other: @integer 2~17, 100, 1000, 10000, 100000, 1000000, …",
            "lv,prg; zero: @integer 0, 10~20, 30, 40, 50, 60, 100, 1000, 10000, 100000, 1000000, …; one: @integer 1, 21, 31, 41, 51, 61, 71, 81, 101, 1001, …; other: @integer 2~9, 22~29, 102, 1002, …",
            "ksh; zero: @integer 0; one: @integer 1; other: @integer 2~17, 100, 1000, 10000, 100000, 1000000, …",

            // [one, two, other]
            "iu,kw,naq,se,sma,smi,smj,smn,sms; one: @integer 1; two: @integer 2; other: @integer 0, 3~17, 100, 1000, 10000, 100000, 1000000, …",

            // [one, few, other]
            "shi; one: @integer 0, 1; few: @integer 2~10; other: @integer 11~26, 100, 1000, 10000, 100000, 1000000, …",
            "mo,ro; one: @integer 1; few: @integer 0, 2~16, 101, 1001, …; other: @integer 20~35, 100, 1000, 10000, 100000, 1000000, …",
            "bs,hr,sh,sr; one: @integer 1, 21, 31, 41, 51, 61, 71, 81, 101, 1001, …; few: @integer 2~4, 22~24, 32~34, 42~44, 52~54, 62, 102, 1002, …; other: @integer 0, 5~19, 100, 1000, 10000, 100000, 1000000, …",

            // [one, two, few, other]
            "gd; one: @integer 1, 11; two: @integer 2, 12; few: @integer 3~10, 13~19; other: @integer 0, 20~34, 100, 1000, 10000, 100000, 1000000, …",
            "sl; one: @integer 1, 101, 201, 301, 401, 501, 601, 701, 1001, …; two: @integer 2, 102, 202, 302, 402, 502, 602, 702, 1002, …; few: @integer 3, 4, 103, 104, 203, 204, 303, 304, 403, 404, 503, 504, 603, 604, 703, 704, 1003, …; other: @integer 0, 5~19, 100, 1000, 10000, 100000, 1000000, …",

            // [one, two, many, other]
            "he,iw; one: @integer 1; two: @integer 2; many: @integer 20, 30, 40, 50, 60, 70, 80, 90, 100, 1000, 10000, 100000, 1000000, …; other: @integer 0, 3~17, 101, 1001, …",

            // [one, few, many, other]
            "cs,sk; one: @integer 1; few: @integer 2~4; many: null; other: @integer 0, 5~19, 100, 1000, 10000, 100000, 1000000, …",
            "be; one: @integer 1, 21, 31, 41, 51, 61, 71, 81, 101, 1001, …; few: @integer 2~4, 22~24, 32~34, 42~44, 52~54, 62, 102, 1002, …; many: @integer 0, 5~19, 100, 1000, 10000, 100000, 1000000, …; other: null",
            "lt; one: @integer 1, 21, 31, 41, 51, 61, 71, 81, 101, 1001, …; few: @integer 2~9, 22~29, 102, 1002, …; many: null; other: @integer 0, 10~20, 30, 40, 50, 60, 100, 1000, 10000, 100000, 1000000, …",
            "mt; one: @integer 1; few: @integer 0, 2~10, 102~107, 1002, …; many: @integer 11~19, 111~117, 1011, …; other: @integer 20~35, 100, 1000, 10000, 100000, 1000000, …",
            "pl; one: @integer 1; few: @integer 2~4, 22~24, 32~34, 42~44, 52~54, 62, 102, 1002, …; many: @integer 0, 5~19, 100, 1000, 10000, 100000, 1000000, …; other: null",
            "ru,uk; one: @integer 1, 21, 31, 41, 51, 61, 71, 81, 101, 1001, …; few: @integer 2~4, 22~24, 32~34, 42~44, 52~54, 62, 102, 1002, …; many: @integer 0, 5~19, 100, 1000, 10000, 100000, 1000000, …; other: null",

            // [one, two, few, many, other]
            "br; one: @integer 1, 21, 31, 41, 51, 61, 81, 101, 1001, …; two: @integer 2, 22, 32, 42, 52, 62, 82, 102, 1002, …; few: @integer 3, 4, 9, 23, 24, 29, 33, 34, 39, 43, 44, 49, 103, 1003, …; many: @integer 1000000, …; other: @integer 0, 5~8, 10~20, 100, 1000, 10000, 100000, …",
            "ga; one: @integer 1; two: @integer 2; few: @integer 3~6; many: @integer 7~10; other: @integer 0, 11~25, 100, 1000, 10000, 100000, 1000000, …",
            "gv; one: @integer 1, 11, 21, 31, 41, 51, 61, 71, 101, 1001, …; two: @integer 2, 12, 22, 32, 42, 52, 62, 72, 102, 1002, …; few: @integer 0, 20, 40, 60, 80, 100, 120, 140, 1000, 10000, 100000, 1000000, …; many: null; other: @integer 3~10, 13~19, 23, 103, 1003, …",

            // [zero, one, two, few, many, other]
            "ar; zero: @integer 0; one: @integer 1; two: @integer 2; few: @integer 3~10, 103~110, 1003, …; many: @integer 11~26, 111, 1011, …; other: @integer 100~102, 200~202, 300~302, 400~402, 500~502, 600, 1000, 10000, 100000, 1000000, …",
            "cy; zero: @integer 0; one: @integer 1; two: @integer 2; few: @integer 3; many: @integer 6; other: @integer 4, 5, 7~20, 100, 1000, 10000, 100000, 1000000, …", };

        // ICU4N TODO: Serialization
        //private < T extends Serializable> T serializeAndDeserialize(T original, Output<Integer> size) {
        //    try
        //    {
        //        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        //        ObjectOutputStream ostream = new ObjectOutputStream(baos);
        //        ostream.writeObject(original);
        //        ostream.flush();
        //        byte bytes[] = baos.toByteArray();
        //        size.value = bytes.length;
        //        ObjectInputStream istream = new ObjectInputStream(new ByteArrayInputStream(bytes));
        //        T reconstituted = (T)istream.readObject();
        //        return reconstituted;
        //    }
        //    catch (IOException e)
        //    {
        //        throw new RuntimeException(e);
        //    }
        //    catch (ClassNotFoundException e)
        //    {
        //        throw new RuntimeException(e);
        //    }
        //}

        [Test]
        [Ignore("ICU4N TODO: Serialization")]
        public void TestSerialization()
        {
            //    Output<Integer> size = new Output<Integer>();
            //    int max = 0;
            //    for (ULocale locale : PluralRules.getAvailableULocales()) {
            //    PluralRules item = PluralRules.forLocale(locale);
            //    PluralRules item2 = serializeAndDeserialize(item, size);
            //    Logln(locale + "\tsize:\t" + size.value);
            //    max = Math.max(max, size.value);
            //    if (!assertEquals(locale + "\tPlural rules before and after serialization", item, item2))
            //    {
            //        // for debugging
            //        PluralRules item3 = serializeAndDeserialize(item, size);
            //        item.equals(item3);
            //    }
            //}
            //Logln("max \tsize:\t" + max);
        }

        //    public static class FixedDecimalHandler implements SerializableTestUtility.Handler
        //{
        //    @Override
        //        public Object[] getTestObjects()
        //{
        //    FixedDecimal items[] = { new FixedDecimal(3d), new FixedDecimal(3d, 2), new FixedDecimal(3.1d, 1),
        //                    new FixedDecimal(3.1d, 2), };
        //    return items;
        //}

        //@Override
        //        public boolean hasSameBehavior(Object a, Object b)
        //{
        //    FixedDecimal a1 = (FixedDecimal)a;
        //    FixedDecimal b1 = (FixedDecimal)b;
        //    return a1.equals(b1);
        //}
        //    }

        [Test]
        [Ignore("ICU4N TODO: Serialization")]
        public void TestSerial()
        {
            //    PluralRules s = PluralRules.forLocale(ULocale.ENGLISH);
            //    checkStreamingEquality(s);
        }

        //public void checkStreamingEquality(PluralRules s)
        //{
        //    try
        //    {
        //        ByteArrayOutputStream byteOut = new ByteArrayOutputStream();
        //        ObjectOutputStream objectOutputStream = new ObjectOutputStream(byteOut);
        //        objectOutputStream.writeObject(s);
        //        objectOutputStream.close();
        //        byte[] contents = byteOut.toByteArray();
        //        Logln(s.getClass() + ": " + showBytes(contents));
        //        ByteArrayInputStream byteIn = new ByteArrayInputStream(contents);
        //        ObjectInputStream objectInputStream = new ObjectInputStream(byteIn);
        //        Object obj = objectInputStream.readObject();
        //        assertEquals("Streamed Object equals ", s, obj);
        //    }
        //    catch (Exception e)
        //    {
        //        assertNull("TestSerial", e);
        //    }
        //}

        ///**
        // * @param contents
        // * @return
        // */
        //private String showBytes(byte[] contents)
        //{
        //    StringBuilder b = new StringBuilder('[');
        //    for (int i = 0; i < contents.length; ++i)
        //    {
        //        int item = contents[i] & 0xFF;
        //        if (item >= 0x20 && item <= 0x7F)
        //        {
        //            b.append((char)item);
        //        }
        //        else
        //        {
        //            b.append('(').append(Utility.hex(item, 2)).append(')');
        //        }
        //    }
        //    return b.append(']').toString();
        //}

        [Test]
        public void TestJavaLocaleFactory()
        {
            PluralRules rulesU0 = PluralRules.ForLocale(new UCultureInfo("fr-FR"));
            PluralRules rulesJ0 = PluralRules.ForLocale(new CultureInfo("fr-FR"));
            assertEquals("forLocale()", rulesU0, rulesJ0);

            PluralRules rulesU1 = PluralRules.ForLocale(new UCultureInfo("fr-FR"), PluralType.Ordinal);
            PluralRules rulesJ1 = PluralRules.ForLocale(new CultureInfo("fr-FR"), PluralType.Ordinal);
            assertEquals("forLocale() with type", rulesU1, rulesJ1);
        }

        /// <summary>
        /// ICU4N specific - Confirms that the SimpleTokenizerEnumerator provides similar behavior as SimpleTokenizer
        /// </summary>
        [Test]
        public void TestSimpleTokenizerEnumerator()
        {
            string text = " fooName= fooValue , barName= barValue!" + Environment.NewLine +
                " bazName = " + Environment.NewLine +
                " bazValue % rule2Name= rule2Value";

            string[] expectedTokens = new string[] { "fooName", "fooValue", "barName", "barValue", "bazName", "bazValue", "rule2Name", "rule2Value" };
            string[] expectedDelimiters = new string[] { "=", ",", "=", "!", "=", "%", "=" };
            // For comparison, we use the SimpleTokenizer.
            // It works a bit differently - it includes all of the delimiters inline, but
            // our new approach puts the delimiters in a separate property.
            string[] actual1 = PluralRules.SimpleTokenizer.Split(text);

            for (int i = 0; i < expectedTokens.Length; i++)
            {
                int actualIndex = i * 2;
                string actualToken = actual1[actualIndex];
                string expectedToken = expectedTokens[i];
                assertEquals("mismatched token", actualToken, expectedToken);

                actualIndex++;
                if (actualIndex < actual1.Length)
                {
                    string actualDelimiter = actual1[actualIndex];
                    string expectedDelimiter = expectedDelimiters[i];
                    assertEquals("mismatched delimiter", actualDelimiter, expectedDelimiter);
                }
            }

#if FEATURE_SPAN
            var iter = new PluralRules.SimpleTokenizerEnumerator(text);
            for (int i = 0; i < expectedTokens.Length; i++)
            {
                assertTrue("missing token", iter.MoveNext());

                string actualToken = new string(iter.Current);
                string expectedToken = expectedTokens[i];
                assertEquals("mismatched token", actualToken, expectedToken);

                if (iter.MoveNext())
                {
                    string actualDelimiter = new string(iter.Current);
                    string expectedDelimiter = expectedDelimiters[i];
                    assertEquals("mismatched delimiter", actualDelimiter, expectedDelimiter);
                }
            }
#endif
        }

#if FEATURE_SPAN
        [Test]
        public void TestSimpleTokenizerEnumerator2()
        {
            string text = "i % 10 = 2..4";
            string[] expectedTokens = new string[] { "i", "%", "10", "=", "2", ".", ".", "4" };
            var iter = new PluralRules.SimpleTokenizerEnumerator(text);

            for (int i = 0; i < expectedTokens.Length; i++)
            {
                assertTrue("mising token on HasNext", iter.HasNext);
                assertTrue("mising token on MoveNext()", iter.MoveNext());

                string actualToken = new string(iter.Current);
                string expectedToken = expectedTokens[i];

                assertEquals("mismatched token", actualToken, expectedToken);
            }
        }
#endif
    }

    internal enum StandardPluralCategories
    {
        zero, one, two, few, many, other
    }

    internal static class StandardPluralCategoriesExtensions
    {
        public static ISet<StandardPluralCategories> All()
        {
            return new HashSet<StandardPluralCategories>((StandardPluralCategories[])Enum.GetValues(typeof(StandardPluralCategories)));
        }

        public static ISet<StandardPluralCategories> GetSet(ICollection<string> source)
        {
            var result = new HashSet<StandardPluralCategories>();
            foreach (string s in source)
                result.Add((StandardPluralCategories)Enum.Parse(typeof(StandardPluralCategories), s, ignoreCase: true));
            return result;
        }

        public static readonly IComparer<ISet<StandardPluralCategories>> SHORTEST_FIRST = Comparer<ISet<StandardPluralCategories>>.Create((arg0, arg1) =>
        {
            int diff = arg0.Count - arg1.Count;
            if (diff != 0)
                return diff;
            // otherwise first...
            // could be optimized, but we don't care here.
            foreach (StandardPluralCategories value in All())
            {
                if (arg0.Contains(value))
                {
                    if (!arg1.Contains(value))
                        return 1;
                }
                else if (arg1.Contains(value))
                    return -1;
            }

            return 0;
        });
    }
}
