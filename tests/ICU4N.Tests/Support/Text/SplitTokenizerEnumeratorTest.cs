using J2N;
using ICU4N.Dev.Test;
using ICU4N.Support;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JCG = J2N.Collections.Generic;
using ICU4N.Impl;

namespace ICU4N.Text
{
    public class SplitTokenizerEnumeratorTest : TestFmwk
    {

        [Test]
        public void TestBasicSplitAndTrim()
        {
            ReadOnlySpan<char> text = " A test ; to split ".AsSpan();
            var target = text.AsTokens(";", " ");

            assertTrue("Expected token not found", target.MoveNext());
            assertTrue("'A test' not found", "A test".AsSpan().SequenceEqual(target.Current));

            assertTrue("Expected token not found", target.MoveNext());
            assertTrue("'to split' not found", "to split".AsSpan().SequenceEqual(target.Current));

            assertFalse("Expected end of enumeration", target.MoveNext());
        }

        [Test]
        public void TestBasicSplitAndTrimStart()
        {
            ReadOnlySpan<char> text = " A test ; to split ".AsSpan();
            var target = text.AsTokens(";", " ", TrimBehavior.Start);

            assertTrue("Expected token not found", target.MoveNext());
            assertTrue("'A test' not found", "A test ".AsSpan().SequenceEqual(target.Current));

            assertTrue("Expected token not found", target.MoveNext());
            assertTrue("'to split' not found", "to split ".AsSpan().SequenceEqual(target.Current));

            assertFalse("Expected end of enumeration", target.MoveNext());
        }

        [Test]
        public void TestBasicSplitAndTrimEnd()
        {
            ReadOnlySpan<char> text = " A test ; to split ".AsSpan();
            var target = text.AsTokens(";", " ", TrimBehavior.End);

            assertTrue("Expected token not found", target.MoveNext());
            assertTrue("'A test' not found", " A test".AsSpan().SequenceEqual(target.Current));

            assertTrue("Expected token not found", target.MoveNext());
            assertTrue("'to split' not found", " to split".AsSpan().SequenceEqual(target.Current));

            assertFalse("Expected end of enumeration", target.MoveNext());
        }

        [Test]
        public void TestMultiLevelDelimiters()
        {
            string text = " fooName: fooValue ; barName: barValue;" + Environment.NewLine +
                " bazName : " + Environment.NewLine +
                " bazValue | rule2Name: rule2Value";

            JCG.Dictionary<string, string> rule1ElementsExpected = new JCG.Dictionary<string, string>
            {
                ["fooName"] = "fooValue",
                ["barName"] = "barValue",
                ["bazName"] = "bazValue",
            };
            JCG.Dictionary<string, string> rule2ElementsExpected = new JCG.Dictionary<string, string>
            {
                ["rule2Name"] = "rule2Value",
            };


            JCG.Dictionary<string, string> rule1Elements = new JCG.Dictionary<string, string>();
            JCG.Dictionary<string, string> rule2Elements = new JCG.Dictionary<string, string>();

            int ruleNumber = 0;

            foreach (var rule in text.AsTokens('|', SplitTokenizerEnumerator.PatternWhiteSpace))
            {
                ruleNumber++;
                foreach (var rule2 in rule.Text.AsTokens(';', SplitTokenizerEnumerator.PatternWhiteSpace))
                {
                    string name, value;
                    var iter = rule2.Text.AsTokens(':', SplitTokenizerEnumerator.PatternWhiteSpace);
                    assertTrue("missing name", iter.MoveNext());
                    name = iter.Current.Text.ToString();
                    assertTrue("missing value", iter.MoveNext());
                    value = iter.Current.Text.ToString();


                    if (ruleNumber == 1)
                    {
                        rule1Elements[name] = value;
                    }
                    else
                    {
                        rule2Elements[name] = value;
                    }
                }
            }

            assertEquals("rule 1 rules mismatch", rule1ElementsExpected, rule1Elements);
            assertEquals("rule 2 rules mismatch", rule2ElementsExpected, rule2Elements);
        }

        [Test]
        public void TestMultipleDelimiters()
        {
            string text = " fooName= fooValue , barName= barValue!" + Environment.NewLine +
                    " bazName = " + Environment.NewLine +
                    " bazValue % rule2Name= rule2Value";

            string[] expectedTokens = new string[] { "fooName", "fooValue", "barName", "barValue", "bazName", "bazValue", "rule2Name", "rule2Value" };
            string[] expectedDelimiters = new string[] { "=", ",", "=", "!", "=", "%", "=", "" };

            int index = 0;
            foreach (var token in text.AsTokens(new char[] { '=', ',', '!', '%' }, SplitTokenizerEnumerator.PatternWhiteSpace))
            {
                string actualToken = token.Text.ToString();
                string expectedToken = expectedTokens[index];
                assertEquals("mismatched token", actualToken, expectedToken);

                string actualDelimiter = token.Delimiter.ToString();
                string expectedDelimiter = expectedDelimiters[index];
                assertEquals("mismatched delimiter", actualDelimiter, expectedDelimiter);
                index++;
            }

        }
    }
}

