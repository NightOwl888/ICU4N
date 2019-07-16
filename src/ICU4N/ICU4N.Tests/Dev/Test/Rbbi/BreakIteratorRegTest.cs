using ICU4N.Support.Text;
using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Linq;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Rbbi
{
    public class BreakIteratorRegTest : TestFmwk
    {
        [Test]
        public void TestRegUnreg()
        {
            CultureInfo thailand_locale = new CultureInfo("th-TH");
            // ICU4N: Arbitrary locales are not allowed in .NET
            //CultureInfo foo_locale = new CultureInfo("fu-FU");
            BreakIterator jwbi = BreakIterator.GetWordInstance(new CultureInfo("ja"));
            BreakIterator uwbi = BreakIterator.GetWordInstance(new CultureInfo("en-US"));
            BreakIterator usbi = BreakIterator.GetSentenceInstance(new CultureInfo("en-US"));
            BreakIterator twbi = BreakIterator.GetWordInstance(thailand_locale);
            BreakIterator rwbi = BreakIterator.GetWordInstance(CultureInfo.InvariantCulture);  // (new Locale("", "", ""));

            BreakIterator sbi = (BreakIterator)usbi.Clone();
            // todo: this will cause the test to fail, no way to set a breakiterator to null text so can't fix yet.
            // String text = "This is some test, by golly. Boy, they don't make tests like they used to, do they?  This here test ain't worth $2.50.  Nope.";
            // sbi.setText(text);

            assertTrue(!BreakIterator.Unregister(""), "unregister before register"); // coverage

            // ICU4N: Arbitrary locales are not allowed in .NET
            //object key0 = BreakIterator.RegisterInstance((BreakIterator)twbi.Clone(), foo_locale, BreakIterator.KIND_WORD);
            object key1 = BreakIterator.RegisterInstance(sbi, new CultureInfo("en-US"), BreakIterator.KIND_WORD);
            object key2 = BreakIterator.RegisterInstance((BreakIterator)twbi.Clone(), new CultureInfo("en-US"), BreakIterator.KIND_WORD);

            {
                BreakIterator test0 = BreakIterator.GetWordInstance(new CultureInfo("ja"));
                BreakIterator test1 = BreakIterator.GetWordInstance(new CultureInfo("en-US"));
                BreakIterator test2 = BreakIterator.GetSentenceInstance(new CultureInfo("en-US"));
                BreakIterator test3 = BreakIterator.GetWordInstance(thailand_locale);
                // ICU4N: Arbitrary locales are not allowed in .NET
                //BreakIterator test4 = BreakIterator.GetWordInstance(foo_locale);

                assertEqual(test0, jwbi, "japan word == japan word");
                assertEqual(test1, twbi, "us word == thai word");
                assertEqual(test2, usbi, "us sentence == us sentence");
                assertEqual(test3, twbi, "thai word == thai word");
                // ICU4N: Arbitrary locales are not allowed in .NET
                //assertEqual(test4, twbi, "foo word == thai word");
            }

            //Locale[] locales = BreakIterator.getAvailableLocales();

            assertTrue(BreakIterator.Unregister(key2), "unregister us word (thai word)");
            assertTrue(!BreakIterator.Unregister(key2), "unregister second time");
            bool error = false;
            try
            {
                BreakIterator.Unregister(null);
            }
            catch (ArgumentException e)
            {
                error = true;
            }

            assertTrue(error, "unregister null");

            {
                CharacterIterator sci = BreakIterator.GetWordInstance(new CultureInfo("en-US")).Text;
                int len = sci.EndIndex - sci.BeginIndex;
                assertEqual(len, 0, "us word text: " + getString(sci));
            }

            // ICU4N: Arbitrary locales are not allowed in .NET
            //assertTrue((BreakIterator.GetAvailableLocales().ToList()).Contains(foo_locale), "foo_locale");
            //assertTrue(BreakIterator.Unregister(key0), "unregister foo word (thai word)");
            //assertTrue(!(BreakIterator.GetAvailableLocales().ToList()).Contains(foo_locale), "no foo_locale");
            assertEqual(BreakIterator.GetWordInstance(new CultureInfo("en-US")), usbi, "us word == us sentence");

            assertTrue(BreakIterator.Unregister(key1), "unregister us word (us sentence)");
            {
                BreakIterator test0 = BreakIterator.GetWordInstance(new CultureInfo("ja"));
                BreakIterator test1 = BreakIterator.GetWordInstance(new CultureInfo("en-US"));
                BreakIterator test2 = BreakIterator.GetSentenceInstance(new CultureInfo("en-US"));
                BreakIterator test3 = BreakIterator.GetWordInstance(thailand_locale);
                // ICU4N: Arbitrary locales are not allowed in .NET
                //BreakIterator test4 = BreakIterator.GetWordInstance(foo_locale);

                assertEqual(test0, jwbi, "japanese word break");
                assertEqual(test1, uwbi, "us sentence-word break");
                assertEqual(test2, usbi, "us sentence break");
                assertEqual(test3, twbi, "thai word break");
                // ICU4N: Arbitrary locales are not allowed in .NET
                //assertEqual(test4, rwbi, "root word break");

                CharacterIterator sci = test1.Text;
                int len = sci.EndIndex - sci.BeginIndex;
                assertEqual(len, 0, "us sentence-word break text: " + getString(sci));
            }
        }

        private void assertEqual(Object lhs, Object rhs, String msg_)
        {
            msg(msg_, lhs.Equals(rhs) ? LOG : ERR, true, true);
        }

        private void assertEqual(int lhs, int rhs, String msg_)
        {
            msg(msg_, lhs == rhs ? LOG : ERR, true, true);
        }

        private void assertTrue(bool arg, String msg_)
        {
            msg(msg_, arg ? LOG : ERR, true, true);
        }

        private static String getString(CharacterIterator ci)
        {
            StringBuffer buf = new StringBuffer(ci.EndIndex - ci.BeginIndex + 2);
            buf.Append("'");
            for (char c = ci.MoveFirst(); c != CharacterIterator.DONE; c = ci.MoveNext())
            {
                buf.Append(c);
            }
            buf.Append("'");
            return buf.ToString();
        }
    }
}
