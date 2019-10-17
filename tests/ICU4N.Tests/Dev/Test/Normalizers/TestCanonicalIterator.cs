using ICU4N.Impl;
using ICU4N.Globalization;
using ICU4N.Support;
using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Normalizers
{
    public class TestCanonicalIterator : TestFmwk
    {
        const bool SHOW_NAMES = false;

        static readonly string[][] testArray = {
           new string[] {"\u00C5d\u0307\u0327", "A\u030Ad\u0307\u0327, A\u030Ad\u0327\u0307, A\u030A\u1E0B\u0327, "
                + "A\u030A\u1E11\u0307, \u00C5d\u0307\u0327, \u00C5d\u0327\u0307, "
                + "\u00C5\u1E0B\u0327, \u00C5\u1E11\u0307, \u212Bd\u0307\u0327, "
                + "\u212Bd\u0327\u0307, \u212B\u1E0B\u0327, \u212B\u1E11\u0307"},
            new string[] {"\u010d\u017E", "c\u030Cz\u030C, c\u030C\u017E, \u010Dz\u030C, \u010D\u017E"},
            new string[] {"x\u0307\u0327", "x\u0307\u0327, x\u0327\u0307, \u1E8B\u0327"},
        };

        [Test]
        public void TestExhaustive()
        {
            int counter = 0;
            CanonicalEnumerator it = new CanonicalEnumerator("");
            /*
            CanonicalIterator slowIt = new CanonicalIterator("");
            slowIt.SKIP_ZEROS = false;
            */
            //Transliterator name = Transliterator.getInstance("[^\\u0020-\\u007F] name");
            //Set itSet = new TreeSet();
            //Set slowItSet = new TreeSet();


            for (int i = 0; i < 0x10FFFF; ++i)
            {

                // skip characters we know don't have decomps
                UUnicodeCategory type = UChar.GetUnicodeCategory(i);
                if (type == UUnicodeCategory.OtherNotAssigned || type == UUnicodeCategory.PrivateUse
                    || type == UUnicodeCategory.Surrogate) continue;

                if ((++counter % 5000) == 0) Logln("Testing " + Utility.Hex(i, 0));

                string s = UTF16.ValueOf(i);
                CharacterTest(s, i, it);

                CharacterTest(s + "\u0345", i, it);
            }
        }

        public int TestSpeed()
        {
            // skip unless verbose
            if (!IsVerbose()) return 0;

            string s = "\uAC01\u0345";

            CanonicalEnumerator it = new CanonicalEnumerator(s);
            double start, end;
            int x = 0; // just to keep code from optimizing away.
            int iterations = 10000;
            double slowDelta = 0;

            /*
            CanonicalIterator slowIt = new CanonicalIterator(s);
            slowIt.SKIP_ZEROS = false;

            start = System.currentTimeMillis();
            for (int i = 0; i < iterations; ++i) {
                slowIt.setSource(s);
                while (true) {
                    String item = slowIt.next();
                    if (item == null) break;
                    x += item.length();
                }
            }
            end = System.currentTimeMillis();
            double slowDelta = (end-start) / iterations;
            Logln("Slow iteration: " + slowDelta);
            */

            start = Time.CurrentTimeMilliseconds();
            for (int i = 0; i < iterations; ++i)
            {
                it.SetSource(s);
                while (it.MoveNext())
                {
                    string item = it.Current;
                    x += item.Length;
                }
            }
            end = Time.CurrentTimeMilliseconds();
            double fastDelta = (end - start) / iterations;
            Logln("Fast iteration: " + fastDelta + (slowDelta != 0 ? ", " + (fastDelta / slowDelta) : ""));


            return x;
        }

        [Test]
        public void TestBasic()
        {
            //      This is not interesting anymore as the data is already built
            //      beforehand

            //        check build
            //        UnicodeSet ss = CanonicalIterator.getSafeStart();
            //        Logln("Safe Start: " + ss.toPattern(true));
            //        ss = CanonicalIterator.getStarts('a');
            //        expectEqual("Characters with 'a' at the start of their decomposition: ", "", CanonicalIterator.getStarts('a'),
            //            new UnicodeSet("[\u00E0-\u00E5\u0101\u0103\u0105\u01CE\u01DF\u01E1\u01FB"
            //            + "\u0201\u0203\u0227\u1E01\u1EA1\u1EA3\u1EA5\u1EA7\u1EA9\u1EAB\u1EAD\u1EAF\u1EB1\u1EB3\u1EB5\u1EB7]")
            //                );

            // check permute
            // NOTE: we use a TreeSet below to sort the output, which is not guaranteed to be sorted!

            ISet<string> results = new SortedSet<string>(StringComparer.Ordinal);
            CanonicalEnumerator.Permute("ABC", false, results);
            expectEqual("Simple permutation ", "", CollectionToString(results), "ABC, ACB, BAC, BCA, CAB, CBA");

            // try samples
            ISet<string> set = new SortedSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < testArray.Length; ++i)
            {
                //Logln("Results for: " + name.transliterate(testArray[i]));
                CanonicalEnumerator it = new CanonicalEnumerator(testArray[i][0]);
                // int counter = 0;
                set.Clear();
                string first = null;
                while (it.MoveNext())
                {
                    string result = it.Current;
                    if (first == null)
                    {
                        first = result;
                    }
                    set.Add(result); // sort them
                                     //Logln(++counter + ": " + hex.transliterate(result));
                                     //Logln(" = " + name.transliterate(result));
                }
                //while (true)
                //{
                //    String result = it.Next();
                //    if (first == null)
                //    {
                //        first = result;
                //    }
                //    if (result == null) break;
                //    set.Add(result); // sort them
                //                     //Logln(++counter + ": " + hex.transliterate(result));
                //                     //Logln(" = " + name.transliterate(result));
                //}
                expectEqual(i + ": ", testArray[i][0], CollectionToString(set), testArray[i][1]);
                it.Reset();
                it.MoveNext();
                if (!it.Current.Equals(first))
                {
                    Errln("CanonicalIterator.reset() failed");
                }
                if (!it.Source.Equals(Normalizer.Normalize(testArray[i][0], NormalizerMode.NFD)))
                {
                    Errln("CanonicalIterator.getSource() does not return NFD of input source");
                }
            }
        }

        private void expectEqual(string message, string item, object a, object b)
        {
            if (!a.Equals(b))
            {
                Errln("FAIL: " + message + GetReadable(item));
                Errln("\t" + GetReadable(a));
                Errln("\t" + GetReadable(b));
            }
            else
            {
                Logln("Checked: " + message + GetReadable(item));
                Logln("\t" + GetReadable(a));
                Logln("\t" + GetReadable(b));
            }
        }

        //Transliterator name = null;
        //Transliterator hex = null;

        public string GetReadable(object obj)
        {
            if (obj == null) return "null";
            string s = obj.ToString();
            if (s.Length == 0) return "";
            // set up for readable display
            //if (name == null) name = Transliterator.getInstance("[^\\ -\\u007F] name");
            //if (hex == null) hex = Transliterator.getInstance("[^\\ -\\u007F] hex");
            return "[" + (SHOW_NAMES ? Hex(s) + "; " : "") + Hex(s) + "]";
        }

        private void CharacterTest(string s, int ch, CanonicalEnumerator it)
        {
            int mixedCounter = 0;
            int lastMixedCounter = -1;
            bool gotDecomp = false;
            bool gotComp = false;
            bool gotSource = false;
            string decomp = Normalizer.Decompose(s, false);
            string comp = Normalizer.Compose(s, false);

            // skip characters that don't have either decomp.
            // need quick test for this!
            if (s.Equals(decomp) && s.Equals(comp)) return;

            it.SetSource(s);

            while (it.MoveNext())
            {
                string item = it.Current;
                //if (item == null) break;
                if (item.Equals(s)) gotSource = true;
                if (item.Equals(decomp)) gotDecomp = true;
                if (item.Equals(comp)) gotComp = true;
                if ((mixedCounter & 0x7F) == 0 && (ch < 0xAD00 || ch > 0xAC00 + 11172))
                {
                    if (lastMixedCounter != mixedCounter)
                    {
                        Logln("");
                        lastMixedCounter = mixedCounter;
                    }
                    Logln("\t" + mixedCounter + "\t" + Hex(item)
                    + (item.Equals(s) ? "\t(*original*)" : "")
                    + (item.Equals(decomp) ? "\t(*decomp*)" : "")
                    + (item.Equals(comp) ? "\t(*comp*)" : "")
                    );
                }

            }

            // check that zeros optimization doesn't mess up.
            /*
            if (true) {
                it.reset();
                itSet.clear();
                while (true) {
                    String item = it.next();
                    if (item == null) break;
                    itSet.add(item);
                }
                slowIt.setSource(s);
                slowItSet.clear();
                while (true) {
                    String item = slowIt.next();
                    if (item == null) break;
                    slowItSet.add(item);
                }
                if (!itSet.equals(slowItSet)) {
                    errln("Zero optimization failure with " + getReadable(s));
                }
            }
            */

            mixedCounter++;
            if (!gotSource || !gotDecomp || !gotComp)
            {
                Errln("FAIL CanonicalIterator: " + s + " decomp: " + decomp + " comp: " + comp);
                it.Reset();
                //for (string item = it.Next(); item != null; item = it.Next())
                while (it.MoveNext())
                {
                    string item = it.Current;
                    Err(item + "    ");
                }
                Errln("");
            }
        }

        internal static string CollectionToString<T>(ICollection<T> col)
        {
            StringBuffer result = new StringBuffer();
            using (var it = col.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    if (result.Length != 0) result.Append(", ");
                    result.Append(it.Current.ToString());
                }
            }
            return result.ToString();
        }
    }
}
