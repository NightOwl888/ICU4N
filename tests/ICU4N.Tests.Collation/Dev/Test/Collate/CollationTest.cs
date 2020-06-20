using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Impl.Coll;
using ICU4N.Support;
using ICU4N.Text;
using ICU4N.Util;
using J2N;
using J2N.Numerics;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ICU4N.Dev.Test.Collate
{
    /// <summary>
    /// CollationTest.cs, ported from collationtest.cpp
    /// C++ version created on: 2012apr27
    /// created by: Markus W. Scherer
    /// </summary>
    public class CollationTest : TestFmwk
    {
        public CollationTest()
        {
        }

        // Fields
        internal Normalizer2 fcd, nfd;
        internal Collator coll;
        internal String fileLine;
        internal int fileLineNumber;
        internal String fileTestName;

        // package private methods ----------------------------------------------

        internal static void DoTest(TestFmwk test, RuleBasedCollator col, String source,
                           String target, int result)
        {
            DoTestVariant(test, col, source, target, result);
            if (result == -1)
            {
                DoTestVariant(test, col, target, source, 1);
            }
            else if (result == 1)
            {
                DoTestVariant(test, col, target, source, -1);
            }
            else
            {
                DoTestVariant(test, col, target, source, 0);
            }

            CollationElementIterator iter = col.GetCollationElementIterator(source);
            BackAndForth(test, iter);
            iter.SetText(target);
            BackAndForth(test, iter);
        }

        /**
         * Return an integer array containing all of the collation orders
         * returned by calls to next on the specified iterator
         */
        internal static int[] GetOrders(CollationElementIterator iter)
        {
            int maxSize = 100;
            int size = 0;
            int[] orders = new int[maxSize];

            int order;
            while ((order = iter.Next()) != CollationElementIterator.NullOrder)
            {
                if (size == maxSize)
                {
                    maxSize *= 2;
                    int[] temp = new int[maxSize];
                    System.Array.Copy(orders, 0, temp, 0, size);
                    orders = temp;
                }
                orders[size++] = order;
            }

            if (maxSize > size)
            {
                int[] temp = new int[size];
                System.Array.Copy(orders, 0, temp, 0, size);
                orders = temp;
            }
            return orders;
        }

        internal static void BackAndForth(TestFmwk test, CollationElementIterator iter)
        {
            // Run through the iterator forwards and stick it into an array
            iter.Reset();
            int[] orders = GetOrders(iter);

            // Now go through it backwards and make sure we get the same values
            int index = orders.Length;
            int o;

            // reset the iterator
            iter.Reset();

            while ((o = iter.Previous()) != CollationElementIterator.NullOrder)
            {
                if (o != orders[--index])
                {
                    if (o == 0)
                    {
                        index++;
                    }
                    else
                    {
                        while (index > 0 && orders[index] == 0)
                        {
                            index--;
                        }
                        if (o != orders[index])
                        {
                            TestFmwk.Errln("Mismatch at index " + index + ": 0x"
                                + Utility.Hex(orders[index]) + " vs 0x" + Utility.Hex(o));
                            break;
                        }
                    }
                }
            }

            while (index != 0 && orders[index - 1] == 0)
            {
                index--;
            }

            if (index != 0)
            {
                String msg = "Didn't get back to beginning - index is ";
                TestFmwk.Errln(msg + index);

                iter.Reset();
                TestFmwk.Err("next: ");
                while ((o = iter.Next()) != CollationElementIterator.NullOrder)
                {
                    String hexString = "0x" + Utility.Hex(o) + " ";
                    TestFmwk.Err(hexString);
                }
                TestFmwk.Errln("");
                TestFmwk.Err("prev: ");
                while ((o = iter.Previous()) != CollationElementIterator.NullOrder)
                {
                    String hexString = "0x" + Utility.Hex(o) + " ";
                    TestFmwk.Err(hexString);
                }
                TestFmwk.Errln("");
            }
        }

        internal static String AppendCompareResult(int result, String target)
        {
            if (result == -1)
            {
                target += "LESS";
            }
            else if (result == 0)
            {
                target += "EQUAL";
            }
            else if (result == 1)
            {
                target += "GREATER";
            }
            else
            {
                String huh = "?";
                target += huh + result;
            }
            return target;
        }

        internal static String Prettify(CollationKey key)
        {
            byte[] bytes = key.ToByteArray();
            return Prettify(bytes, bytes.Length);
        }

        internal static String Prettify(RawCollationKey key)
        {
            return Prettify(key.Bytes, key.Length);
        }

        internal static String Prettify(byte[] skBytes, int length)
        {
            StringBuilder target = new StringBuilder(length * 3 + 2).Append('[');

            for (int i = 0; i < length; i++)
            {
                String numStr = (skBytes[i] & 0xff).ToHexString();
                if (numStr.Length < 2)
                {
                    target.Append('0');
                }
                target.Append(numStr).Append(' ');
            }
            target.Append(']');
            return target.ToString();
        }

        private static void DoTestVariant(TestFmwk test,
                                          RuleBasedCollator myCollation,
                                          String source, String target, int result)
        {
            int compareResult = myCollation.Compare(source, target);
            if (compareResult != result)
            {

                // !!! if not mod build, error, else nothing.
                // warnln if not build, error, else always print warning.
                // do we need a 'quiet warning?' (err or log).  Hmmm,
                // would it work to have the 'verbose' flag let you
                // suppress warnings?  Are there ever some warnings you
                // want to suppress, and others you don't?
                TestFmwk.Errln("Comparing \"" + Utility.Hex(source) + "\" with \""
                        + Utility.Hex(target) + "\" expected " + result
                        + " but got " + compareResult);
            }
            CollationKey ssk = myCollation.GetCollationKey(source);
            CollationKey tsk = myCollation.GetCollationKey(target);
            compareResult = ssk.CompareTo(tsk);
            if (compareResult != result)
            {
                TestFmwk.Errln("Comparing CollationKeys of \"" + Utility.Hex(source)
                + "\" with \"" + Utility.Hex(target)
                + "\" expected " + result + " but got "
                + compareResult);
            }
            RawCollationKey srsk = new RawCollationKey();
            myCollation.GetRawCollationKey(source, srsk);
            RawCollationKey trsk = new RawCollationKey();
            myCollation.GetRawCollationKey(target, trsk);
            compareResult = ssk.CompareTo(tsk);
            if (compareResult != result)
            {
                TestFmwk.Errln("Comparing RawCollationKeys of \""
                        + Utility.Hex(source)
                        + "\" with \"" + Utility.Hex(target)
                        + "\" expected " + result + " but got "
                        + compareResult);
            }
        }

        [Test]
        public void TestMinMax()
        {
            SetRootCollator();
            RuleBasedCollator rbc = (RuleBasedCollator)coll;

            String s = "\uFFFE\uFFFF";
            long[] ces;

            ces = rbc.InternalGetCEs(s.AsCharSequence());
            if (ces.Length != 2)
            {
                Errln("expected 2 CEs for <FFFE, FFFF>, got " + ces.Length);
                return;
            }

            long ce = ces[0];
            long expected = Collation.MakeCE(Collation.MergeSeparatorPrimary);
            if (ce != expected)
            {
                Errln("CE(U+fffe)=0x" + Utility.Hex(ce) + " != 02..");
            }

            ce = ces[1];
            expected = Collation.MakeCE(Collation.MaxPrimary);
            if (ce != expected)
            {
                Errln("CE(U+ffff)=0x" + Utility.Hex(ce) + " != max..");
            }
        }

        [Test]
        public void TestImplicits()
        {
            CollationData cd = CollationRoot.Data;

            // Implicit primary weights should be assigned for the following sets,
            // and sort in ascending order by set and then code point.
            // See http://www.unicode.org/reports/tr10/#Implicit_Weights
            // core Han Unified Ideographs
            UnicodeSet coreHan = new UnicodeSet("[\\p{unified_ideograph}&"
                                     + "[\\p{Block=CJK_Unified_Ideographs}"
                                     + "\\p{Block=CJK_Compatibility_Ideographs}]]");
            // all other Unified Han ideographs
            UnicodeSet otherHan = new UnicodeSet("[\\p{unified ideograph}-"
                                     + "[\\p{Block=CJK_Unified_Ideographs}"
                                     + "\\p{Block=CJK_Compatibility_Ideographs}]]");

            UnicodeSet unassigned = new UnicodeSet("[[:Cn:][:Cs:][:Co:]]");
            unassigned.Remove(0xfffe, 0xffff);  // These have special CLDR root mappings.

            // Starting with CLDR 26/ICU 54, the root Han order may instead be
            // the Unihan radical-stroke order.
            // The tests should pass either way, so we only test the order of a small set of Han characters
            // whose radical-stroke order is the same as their code point order.
            UnicodeSet someHanInCPOrder = new UnicodeSet(
                    "[\\u4E00-\\u4E16\\u4E18-\\u4E2B\\u4E2D-\\u4E3C\\u4E3E-\\u4E48" +
                    "\\u4E4A-\\u4E60\\u4E63-\\u4E8F\\u4E91-\\u4F63\\u4F65-\\u50F1\\u50F3-\\u50F6]");
            UnicodeSet inOrder = new UnicodeSet(someHanInCPOrder);
            inOrder.AddAll(unassigned).Freeze();

            UnicodeSet[] sets = { coreHan, otherHan, unassigned };
            int prev = 0;
            long prevPrimary = 0;
            UTF16CollationIterator ci = new UTF16CollationIterator(cd, false, "".AsCharSequence(), 0);
            for (int i = 0; i < sets.Length; ++i)
            {
                UnicodeSetIterator iter = new UnicodeSetIterator(sets[i]);
                while (iter.Next())
                {
                    String s = iter.GetString();
                    int c = s.CodePointAt(0);
                    ci.SetText(false, s.AsCharSequence(), 0);
                    long ce = ci.NextCE();
                    long ce2 = ci.NextCE();
                    if (ce == Collation.NoCE || ce2 != Collation.NoCE)
                    {
                        Errln("CollationIterator.nextCE(0x" + Utility.Hex(c)
                                + ") did not yield exactly one CE");
                        continue;

                    }
                    if ((ce & 0xffffffffL) != Collation.CommonSecondaryAndTertiaryCE)
                    {
                        Errln("CollationIterator.nextCE(U+" + Utility.Hex(c, 4)
                                + ") has non-common sec/ter weights: 0x" + Utility.Hex(ce & 0xffffffffL, 8));
                        continue;
                    }
                    long primary = ce.TripleShift(32);
                    if (!(primary > prevPrimary) && inOrder.Contains(c) && inOrder.Contains(prev))
                    {
                        Errln("CE(U+" + Utility.Hex(c) + ")=0x" + Utility.Hex(primary)
                                + ".. not greater than CE(U+" + Utility.Hex(prev)
                                + ")=0x" + Utility.Hex(prevPrimary) + "..");

                    }
                    prev = c;
                    prevPrimary = primary;
                }
            }
        }

        // ICU4C: TestNulTerminated / renamed for ICU4J
        [Test]
        public void TestSubSequence()
        {
            CollationData data = CollationRoot.Data;
            string s = "abab"; // { 0x61, 0x62, 0x61, 0x62 }

            UTF16CollationIterator ci1 = new UTF16CollationIterator(data, false, s.AsCharSequence(), 0);
            UTF16CollationIterator ci2 = new UTF16CollationIterator(data, false, s.AsCharSequence(), 2);

            for (int i = 0; i < 2; ++i)
            {
                long ce1 = ci1.NextCE();
                long ce2 = ci2.NextCE();

                if (ce1 != ce2)
                {
                    Errln("CollationIterator.nextCE(with start position at 0) != "
                          + "nextCE(with start position at 2) at CE " + i);
                }
            }
        }


        // ICU4C: TestIllegalUTF8 / not applicable to ICU4J


        private static void AddLeadSurrogatesForSupplementary(UnicodeSet src, UnicodeSet dest)
        {
            for (int c = 0x10000; c < 0x110000;)
            {
                int next = c + 0x400;
                if (src.ContainsSome(c, next - 1))
                {
                    dest.Add(UTF16.GetLeadSurrogate(c));
                }
                c = next;
            }
        }

        [Test]
        public void TestShortFCDData()
        {
            UnicodeSet expectedLccc = new UnicodeSet("[:^lccc=0:]");
            expectedLccc.Add(0xdc00, 0xdfff);   // add all trail surrogates
            AddLeadSurrogatesForSupplementary(expectedLccc, expectedLccc);

            UnicodeSet lccc = new UnicodeSet(); // actual
            for (int c = 0; c <= 0xffff; ++c)
            {
                if (CollationFCD.HasLccc(c))
                {
                    lccc.Add(c);
                }
            }

            UnicodeSet diff = new UnicodeSet(expectedLccc);
            diff.RemoveAll(lccc);
            diff.Remove(0x10000, 0x10ffff);  // hasLccc() only works for the BMP

            String empty = "[]";
            String diffString;

            diffString = diff.ToPattern(true);
            assertEquals("CollationFCD::hasLccc() expected-actual", empty, diffString);

            diff = lccc;
            diff.RemoveAll(expectedLccc);
            diffString = diff.ToPattern(true);
            assertEquals("CollationFCD::hasLccc() actual-expected", empty, diffString);

            UnicodeSet expectedTccc = new UnicodeSet("[:^tccc=0:]");
            AddLeadSurrogatesForSupplementary(expectedLccc, expectedTccc);
            AddLeadSurrogatesForSupplementary(expectedTccc, expectedTccc);

            UnicodeSet tccc = new UnicodeSet(); // actual
            for (int c = 0; c <= 0xffff; ++c)
            {
                if (CollationFCD.HasTccc(c))
                {
                    tccc.Add(c);
                }
            }

            diff = new UnicodeSet(expectedTccc);
            diff.RemoveAll(tccc);
            diff.Remove(0x10000, 0x10ffff); // hasTccc() only works for the BMP
            assertEquals("CollationFCD::hasTccc() expected-actual", empty, diffString);

            diff = tccc;
            diff.RemoveAll(expectedTccc);
            diffString = diff.ToPattern(true);
            assertEquals("CollationFCD::hasTccc() actual-expected", empty, diffString);
        }

        private class CodePointIterator
        {
            internal int[] cp;
            internal int length;
            internal int pos;

            internal CodePointIterator(int[] cp)
            {
                this.cp = cp;
                this.length = cp.Length;
                this.pos = 0;
            }

            internal void ResetToStart()
            {
                pos = 0;
            }

            internal int Next()
            {
                return (pos < length) ? cp[pos++] : Collation.SentinelCodePoint;
            }

            internal int Previous()
            {
                return (pos > 0) ? cp[--pos] : Collation.SentinelCodePoint;
            }

            internal int Length
            {
                get { return length; }
            }

            internal int Index
            {
                get { return pos; }
            }
        }

        private void CheckFCD(String name, CollationIterator ci, CodePointIterator cpi)
        {
            // Iterate forward to the limit.
            for (; ; )
            {
                int c1 = ci.NextCodePoint();
                int c2 = cpi.Next();
                if (c1 != c2)
                {
                    Errln(name + ".nextCodePoint(to limit, 1st pass) = U+" + Utility.Hex(c1)
                            + " != U+" + Utility.Hex(c1) + " at " + cpi.Index);
                    return;
                }
                if (c1 < 0)
                {
                    break;
                }
            }

            // Iterate backward most of the way.
            for (int n = (cpi.Length * 2) / 3; n > 0; --n)
            {
                int c1 = ci.PreviousCodePoint();
                int c2 = cpi.Previous();
                if (c1 != c2)
                {
                    Errln(name + ".previousCodePoint() = U+" + Utility.Hex(c1) +
                            " != U+" + Utility.Hex(c2) + " at " + cpi.Index);
                    return;
                }
            }

            // Forward again.
            for (; ; )
            {
                int c1 = ci.NextCodePoint();
                int c2 = cpi.Next();
                if (c1 != c2)
                {
                    Errln(name + ".nextCodePoint(to limit again) = U+" + Utility.Hex(c1)
                            + " != U+" + Utility.Hex(c2) + " at " + cpi.Index);
                    return;
                }
                if (c1 < 0)
                {
                    break;
                }
            }

            // Iterate backward to the start.
            for (; ; )
            {
                int c1 = ci.PreviousCodePoint();
                int c2 = cpi.Previous();
                if (c1 != c2)
                {
                    Errln(name + ".nextCodePoint(to start) = U+" + Utility.Hex(c1)
                            + " != U+" + Utility.Hex(c2) + " at " + cpi.Index);
                    return;
                }
                if (c1 < 0)
                {
                    break;
                }
            }
        }

        [Test]
        public void TestFCD()
        {
            CollationData data = CollationRoot.Data;

            // Input string, not FCD.
            StringBuilder buf = new StringBuilder();
            buf.Append("\u0308\u00e1\u0062\u0301\u0327\u0430\u0062")
                .AppendCodePoint(0x1D15F)   // MUSICAL SYMBOL QUARTER NOTE=1D158 1D165, ccc=0, 216
                .Append("\u0327\u0308")     // ccc=202, 230
                .AppendCodePoint(0x1D16D)   // MUSICAL SYMBOL COMBINING AUGMENTATION DOT, ccc=226
                .AppendCodePoint(0x1D15F)
                .AppendCodePoint(0x1D16D)
                .Append("\uac01")
                .Append("\u00e7")           // Character with tccc!=0 decomposed together with mis-ordered sequence.
                .AppendCodePoint(0x1D16D).AppendCodePoint(0x1D165)
                .Append("\u00e1")           // Character with tccc!=0 decomposed together with decomposed sequence.
                .Append("\u0f73\u0f75")     // Tibetan composite vowels must be decomposed.
                .Append("\u4e00\u0f81");
            String s = buf.ToString();

            // Expected code points.
            int[] cp = {
            0x308, 0xe1, 0x62, 0x327, 0x301, 0x430, 0x62,
            0x1D158, 0x327, 0x1D165, 0x1D16D, 0x308,
            0x1D15F, 0x1D16D,
            0xac01,
            0x63, 0x327, 0x1D165, 0x1D16D,
            0x61,
            0xf71, 0xf71, 0xf72, 0xf74, 0x301,
            0x4e00, 0xf71, 0xf80
        };

            FCDUTF16CollationIterator u16ci = new FCDUTF16CollationIterator(data, false, s.AsCharSequence(), 0);
            CodePointIterator cpi = new CodePointIterator(cp);
            CheckFCD("FCDUTF16CollationIterator", u16ci, cpi);

            cpi.ResetToStart();
            UCharacterIterator iter = UCharacterIterator.GetInstance(s);
            FCDIterCollationIterator uici = new FCDIterCollationIterator(data, false, iter, 0);
            CheckFCD("FCDIterCollationIterator", uici, cpi);
        }

        private void CheckAllocWeights(CollationWeights cw, long lowerLimit, long upperLimit,
                int n, int someLength, int minCount)
        {

            if (!cw.AllocWeights(lowerLimit, upperLimit, n))
            {
                Errln("CollationWeights::allocWeights(0x"
                        + Utility.Hex(lowerLimit) + ",0x"
                        + Utility.Hex(upperLimit) + ","
                        + n + ") = false");
                return;
            }
            long previous = lowerLimit;
            int count = 0; // number of weights that have someLength
            for (int i = 0; i < n; ++i)
            {
                long w = cw.NextWeight();
                if (w == 0xffffffffL)
                {
                    Errln("CollationWeights::allocWeights(0x"
                            + Utility.Hex(lowerLimit) + ",0x"
                            + Utility.Hex(upperLimit) + ",0x"
                            + n + ").nextWeight() returns only "
                            + i + " weights");
                    return;
                }
                if (!(previous < w && w < upperLimit))
                {
                    Errln("CollationWeights::allocWeights(0x"
                            + Utility.Hex(lowerLimit) + ",0x"
                            + Utility.Hex(upperLimit) + ","
                            + n + ").nextWeight() number "
                            + (i + 1) + " -> 0x" + Utility.Hex(w)
                            + " not between "
                            + Utility.Hex(previous) + " and "
                            + Utility.Hex(upperLimit));
                    return;
                }
                if (CollationWeights.LengthOfWeight(w) == someLength)
                {
                    ++count;
                }
            }
            if (count < minCount)
            {
                Errln("CollationWeights::allocWeights(0x"
                        + Utility.Hex(lowerLimit) + ",0x"
                        + Utility.Hex(upperLimit) + ","
                        + n + ").nextWeight() returns only "
                        + count + " < " + minCount + " weights of length "
                        + someLength);

            }
        }

        [Test]
        public void TestCollationWeights()
        {
            CollationWeights cw = new CollationWeights();

            // Non-compressible primaries use 254 second bytes 02..FF.
            Logln("CollationWeights.initForPrimary(non-compressible)");
            cw.InitForPrimary(false);
            // Expect 1 weight 11 and 254 weights 12xx.
            CheckAllocWeights(cw, 0x10000000L, 0x13000000L, 255, 1, 1);
            CheckAllocWeights(cw, 0x10000000L, 0x13000000L, 255, 2, 254);
            // Expect 255 two-byte weights from the ranges 10ff, 11xx, 1202.
            CheckAllocWeights(cw, 0x10fefe40L, 0x12030300L, 260, 2, 255);
            // Expect 254 two-byte weights from the ranges 10ff and 11xx.
            CheckAllocWeights(cw, 0x10fefe40L, 0x12030300L, 600, 2, 254);
            // Expect 254^2=64516 three-byte weights.
            // During computation, there should be 3 three-byte ranges
            // 10ffff, 11xxxx, 120202.
            // The middle one should be split 64515:1,
            // and the newly-split-off range and the last ranged lengthened.
            CheckAllocWeights(cw, 0x10fffe00L, 0x12020300L, 1 + 64516 + 254 + 1, 3, 64516);
            // Expect weights 1102 & 1103.
            CheckAllocWeights(cw, 0x10ff0000L, 0x11040000L, 2, 2, 2);
            // Expect weights 102102 & 102103.
            CheckAllocWeights(cw, 0x1020ff00L, 0x10210400L, 2, 3, 2);

            // Compressible primaries use 251 second bytes 04..FE.
            Logln("CollationWeights.initForPrimary(compressible)");
            cw.InitForPrimary(true);
            // Expect 1 weight 11 and 251 weights 12xx.
            CheckAllocWeights(cw, 0x10000000L, 0x13000000L, 252, 1, 1);
            CheckAllocWeights(cw, 0x10000000L, 0x13000000L, 252, 2, 251);
            // Expect 252 two-byte weights from the ranges 10fe, 11xx, 1204.
            CheckAllocWeights(cw, 0x10fdfe40L, 0x12050300L, 260, 2, 252);
            // Expect weights 1104 & 1105.
            CheckAllocWeights(cw, 0x10fe0000L, 0x11060000L, 2, 2, 2);
            // Expect weights 102102 & 102103.
            CheckAllocWeights(cw, 0x1020ff00L, 0x10210400L, 2, 3, 2);

            // Secondary and tertiary weights use only bytes 3 & 4.
            Logln("CollationWeights.initForSecondary()");
            cw.InitForSecondary();
            // Expect weights fbxx and all four fc..ff.
            CheckAllocWeights(cw, 0xfb20L, 0x10000L, 20, 3, 4);

            Logln("CollationWeights.initForTertiary()");
            cw.InitForTertiary();
            // Expect weights 3dxx and both 3e & 3f.
            CheckAllocWeights(cw, 0x3d02L, 0x4000L, 10, 3, 2);
        }

        private static bool IsValidCE(CollationRootElements re, CollationData data, long p, long s, long ctq)
        {
            long p1 = p.TripleShift(24);
            long p2 = (p.TripleShift(16)) & 0xff;
            long p3 = (p.TripleShift(8)) & 0xff;
            long p4 = p & 0xff;
            long s1 = s.TripleShift(8);
            long s2 = s & 0xff;
            // ctq = Case, Tertiary, Quaternary
            long c = (ctq & Collation.CaseMask).TripleShift(14);
            long t = ctq & Collation.OnlyTertiaryMask;
            long t1 = t.TripleShift(8);
            long t2 = t & 0xff;
            long q = ctq & Collation.QuaternaryMask;
            // No leading zero bytes.
            if ((p != 0 && p1 == 0) || (s != 0 && s1 == 0) || (t != 0 && t1 == 0))
            {
                return false;
            }
            // No intermediate zero bytes.
            if (p1 != 0 && p2 == 0 && (p & 0xffff) != 0)
            {
                return false;
            }
            if (p2 != 0 && p3 == 0 && p4 != 0)
            {
                return false;
            }
            // Minimum & maximum lead bytes.
            if ((p1 != 0 && p1 <= Collation.MergeSeparatorByte)
                    || s1 == Collation.LevelSeparatorByte
                    || t1 == Collation.LevelSeparatorByte || t1 > 0x3f)
            {
                return false;
            }
            if (c > 2)
            {
                return false;
            }
            // The valid byte range for the second primary byte depends on compressibility.
            if (p2 != 0)
            {
                if (data.IsCompressibleLeadByte((int)p1))
                {
                    if (p2 <= Collation.PrimaryCompressionLowByte
                            || Collation.PrimaryCompressionHighByte <= p2)
                    {
                        return false;
                    }
                }
                else
                {
                    if (p2 <= Collation.LevelSeparatorByte)
                    {
                        return false;
                    }
                }
            }
            // Other bytes just need to avoid the level separator.
            // Trailing zeros are ok.
            // assert (Collation.LEVEL_SEPARATOR_BYTE == 1);
            if (p3 == Collation.LevelSeparatorByte || p4 == Collation.LevelSeparatorByte
                    || s2 == Collation.LevelSeparatorByte || t2 == Collation.LevelSeparatorByte)
            {
                return false;
            }
            // Well-formed CEs.
            if (p == 0)
            {
                if (s == 0)
                {
                    if (t == 0)
                    {
                        // Completely ignorable CE.
                        // Quaternary CEs are not supported.
                        if (c != 0 || q != 0)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        // Tertiary CE.
                        if (t < re.TertiaryBoundary || c != 2)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    // Secondary CE.
                    if (s < re.SecondaryBoundary || t == 0 || t >= re.TertiaryBoundary)
                    {
                        return false;
                    }
                }
            }
            else
            {
                // Primary CE.
                if (s == 0 || (Collation.CommonWeight16 < s && s <= re.LastCommonSecondary)
                        || s >= re.SecondaryBoundary)
                {
                    return false;
                }
                if (t == 0 || t >= re.TertiaryBoundary)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsValidCE(CollationRootElements re, CollationData data, long ce)
        {
            long p = ce.TripleShift(32);
            long secTer = ce & 0xffffffffL;
            return IsValidCE(re, data, p, secTer.TripleShift(16), secTer & 0xffff);
        }

        private class RootElementsIterator
        {
            internal CollationData data;
            internal long[] elements;
            internal int length;

            internal long pri;
            internal long secTer;
            internal int index;

            internal RootElementsIterator(CollationData root)
            {
                data = root;
                elements = root.RootElements;
                length = elements.Length;
                pri = 0;
                secTer = 0;
                index = (int)elements[CollationRootElements.IX_FIRST_TERTIARY_INDEX];
            }

            internal bool Next()
            {
                if (index >= length)
                {
                    return false;
                }
                long p = elements[index];
                if (p == CollationRootElements.PrimarySentinel)
                {
                    return false;
                }
                if ((p & CollationRootElements.SecondaryTertiaryDeltaFlag) != 0)
                {
                    ++index;
                    secTer = p & ~CollationRootElements.SecondaryTertiaryDeltaFlag;
                    return true;
                }
                if ((p & CollationRootElements.PrimaryStepMask) != 0)
                {
                    // End of a range, enumerate the primaries in the range.
                    int step = (int)p & CollationRootElements.PrimaryStepMask;
                    p &= 0xffffff00;
                    if (pri == p)
                    {
                        // Finished the range, return the next CE after it.
                        ++index;
                        return Next();
                    }
                    Debug.Assert(pri < p);
                    // Return the next primary in this range.
                    bool isCompressible = data.IsCompressiblePrimary(pri);
                    if ((pri & 0xffff) == 0)
                    {
                        pri = Collation.IncTwoBytePrimaryByOffset(pri, isCompressible, step);
                    }
                    else
                    {
                        pri = Collation.IncThreeBytePrimaryByOffset(pri, isCompressible, step);
                    }
                    return true;
                }
                // Simple primary CE.
                ++index;
                pri = p;
                // Does this have an explicit below-common sec/ter unit,
                // or does it imply a common one?
                if (index == length)
                {
                    secTer = Collation.CommonSecondaryAndTertiaryCE;
                }
                else
                {
                    secTer = elements[index];
                    if ((secTer & CollationRootElements.SecondaryTertiaryDeltaFlag) == 0)
                    {
                        // No sec/ter delta.
                        secTer = Collation.CommonSecondaryAndTertiaryCE;
                    }
                    else
                    {
                        secTer &= ~CollationRootElements.SecondaryTertiaryDeltaFlag;
                        if (secTer > Collation.CommonSecondaryAndTertiaryCE)
                        {
                            // Implied sec/ter.
                            secTer = Collation.CommonSecondaryAndTertiaryCE;
                        }
                        else
                        {
                            // Explicit sec/ter below common/common.
                            ++index;
                        }
                    }
                }
                return true;
            }

            internal long getPrimary()
            {
                return pri;
            }

            internal long getSecTer()
            {
                return secTer;
            }
        }

        [Test]
        public void TestRootElements()
        {
            CollationData root = CollationRoot.Data;

            CollationRootElements rootElements = new CollationRootElements(root.RootElements);
            RootElementsIterator iter = new RootElementsIterator(root);

            // We check each root CE for validity,
            // and we also verify that there is a tailoring gap between each two CEs.
            CollationWeights cw1c = new CollationWeights(); // compressible primary weights
            CollationWeights cw1u = new CollationWeights(); // uncompressible primary weights
            CollationWeights cw2 = new CollationWeights();
            CollationWeights cw3 = new CollationWeights();

            cw1c.InitForPrimary(true);
            cw1u.InitForPrimary(false);
            cw2.InitForSecondary();
            cw3.InitForTertiary();

            // Note: The root elements do not include Han-implicit or unassigned-implicit CEs,
            // nor the special merge-separator CE for U+FFFE.
            long prevPri = 0;
            long prevSec = 0;
            long prevTer = 0;

            while (iter.Next())
            {
                long pri = iter.getPrimary();
                long secTer = iter.getSecTer();
                // CollationRootElements CEs must have 0 case and quaternary bits.
                if ((secTer & Collation.CaseAndQuaternaryMask) != 0)
                {
                    Errln("CollationRootElements CE has non-zero case and/or quaternary bits: "
                            + "0x" + Utility.Hex(pri, 8) + " 0x" + Utility.Hex(secTer, 8));
                }
                long sec = secTer.TripleShift(16);
                long ter = secTer & Collation.OnlyTertiaryMask;
                long ctq = ter;
                if (pri == 0 && sec == 0 && ter != 0)
                {
                    // Tertiary CEs must have uppercase bits,
                    // but they are not stored in the CollationRootElements.
                    ctq |= 0x8000;
                }
                if (!IsValidCE(rootElements, root, pri, sec, ctq))
                {
                    Errln("invalid root CE 0x"
                            + Utility.Hex(pri, 8) + " 0x" + Utility.Hex(secTer, 8));
                }
                else
                {
                    if (pri != prevPri)
                    {
                        long newWeight = 0;
                        if (prevPri == 0 || prevPri >= Collation.FFFD_Primary)
                        {
                            // There is currently no tailoring gap after primary ignorables,
                            // and we forbid tailoring after U+FFFD and U+FFFF.
                        }
                        else if (root.IsCompressiblePrimary(prevPri))
                        {
                            if (!cw1c.AllocWeights(prevPri, pri, 1))
                            {
                                Errln("no primary/compressible tailoring gap between "
                                        + "0x" + Utility.Hex(prevPri, 8)
                                        + " and 0x" + Utility.Hex(pri, 8));
                            }
                            else
                            {
                                newWeight = cw1c.NextWeight();
                            }
                        }
                        else
                        {
                            if (!cw1u.AllocWeights(prevPri, pri, 1))
                            {
                                Errln("no primary/uncompressible tailoring gap between "
                                        + "0x" + Utility.Hex(prevPri, 8)
                                        + " and 0x" + Utility.Hex(pri, 8));
                            }
                            else
                            {
                                newWeight = cw1u.NextWeight();
                            }
                        }
                        if (newWeight != 0 && !(prevPri < newWeight && newWeight < pri))
                        {
                            Errln("mis-allocated primary weight, should get "
                                    + "0x" + Utility.Hex(prevPri, 8)
                                    + " < 0x" + Utility.Hex(newWeight, 8)
                                    + " < 0x" + Utility.Hex(pri, 8));
                        }
                    }
                    else if (sec != prevSec)
                    {
                        long lowerLimit = prevSec == 0 ?
                                rootElements.SecondaryBoundary - 0x100 : prevSec;
                        if (!cw2.AllocWeights(lowerLimit, sec, 1))
                        {
                            Errln("no secondary tailoring gap between "
                                    + "0x" + Utility.Hex(lowerLimit)
                                    + " and 0x" + Utility.Hex(sec));
                        }
                        else
                        {
                            long newWeight = cw2.NextWeight();
                            if (!(prevSec < newWeight && newWeight < sec))
                            {
                                Errln("mis-allocated secondary weight, should get "
                                        + "0x" + Utility.Hex(lowerLimit)
                                        + " < 0x" + Utility.Hex(newWeight)
                                        + " < 0x" + Utility.Hex(sec));
                            }
                        }
                    }
                    else if (ter != prevTer)
                    {
                        long lowerLimit = prevTer == 0 ?
                                rootElements.TertiaryBoundary - 0x100 : prevTer;
                        if (!cw3.AllocWeights(lowerLimit, ter, 1))
                        {
                            Errln("no tertiary tailoring gap between "
                                    + "0x" + Utility.Hex(lowerLimit)
                                    + " and 0x" + Utility.Hex(ter));
                        }
                        else
                        {
                            long newWeight = cw3.NextWeight();
                            if (!(prevTer < newWeight && newWeight < ter))
                            {
                                Errln("mis-allocated tertiary weight, should get "
                                        + "0x" + Utility.Hex(lowerLimit)
                                        + " < 0x" + Utility.Hex(newWeight)
                                        + " < 0x" + Utility.Hex(ter));
                            }
                        }
                    }
                    else
                    {
                        Errln("duplicate root CE 0x"
                                + Utility.Hex(pri, 8) + " 0x" + Utility.Hex(secTer, 8));
                    }
                }
                prevPri = pri;
                prevSec = sec;
                prevTer = ter;
            }
        }

        [Test]
        public void TestTailoredElements()
        {
            CollationData root = CollationRoot.Data;
            CollationRootElements rootElements = new CollationRootElements(root.RootElements);

            ISet<String> prevLocales = new HashSet<String>();
            prevLocales.Add("");
            prevLocales.Add("root");
            prevLocales.Add("root@collation=standard");

            long[] ces;
            UCultureInfo[] locales = Collator.GetAvailableULocales();
            String localeID = "root";
            int locIdx = 0;

            for (; locIdx < locales.Length; localeID = locales[locIdx++].FullName)
            {
                UCultureInfo locale = new UCultureInfo(localeID);
                String[] types = Collator.GetKeywordValuesForLocale("collation", locale, false);
                for (int typeIdx = 0; typeIdx < types.Length; ++typeIdx)
                {
                    String type = types[typeIdx];  // first: default type
                    if (type.StartsWith("private-", StringComparison.Ordinal))
                    {
                        Errln("Collator.getKeywordValuesForLocale(" + localeID +
                                ") returns private collation keyword: " + type);
                    }
                    UCultureInfo localeWithType = locale.SetKeywordValue("collation", type);
                    Collator coll = Collator.GetInstance(localeWithType);
                    UCultureInfo actual = coll.ActualCulture;
                    if (prevLocales.Contains(actual.FullName))
                    {
                        continue;
                    }
                    prevLocales.Add(actual.FullName);
                    Logln("TestTailoredElements(): requested " + localeWithType.FullName
                            + " -> actual " + actual.FullName);
                    if (!(coll is RuleBasedCollator))
                    {
                        continue;
                    }
                    RuleBasedCollator rbc = (RuleBasedCollator)coll;

                    // Note: It would be better to get tailored strings such that we can
                    // identify the prefix, and only get the CEs for the prefix+string,
                    // not also for the prefix.
                    // There is currently no API for that.
                    // It would help in an unusual case where a contraction starting in the prefix
                    // extends past its end, and we do not see the intended mapping.
                    // For example, for a mapping p|st, if there is also a contraction ps,
                    // then we get CEs(ps)+CEs(t), rather than CEs(p|st).
                    UnicodeSet tailored = coll.GetTailoredSet();
                    UnicodeSetIterator iter = new UnicodeSetIterator(tailored);
                    while (iter.Next())
                    {
                        String s = iter.GetString();
                        ces = rbc.InternalGetCEs(s.AsCharSequence());
                        for (int i = 0; i < ces.Length; ++i)
                        {
                            long ce = ces[i];
                            if (!IsValidCE(rootElements, root, ce))
                            {
                                Logln(Prettify(s));
                                Errln("invalid tailored CE 0x" + Utility.Hex(ce, 16)
                                        + " at CE index " + i + " from string:");
                            }
                        }
                    }
                }
            }
        }

        private static bool IsSpace(char c)
        {
            return (c == 0x09 || c == 0x20 || c == 0x3000);
        }

        private static bool IsSectionStarter(char c)
        {
            return (c == '%' || c == '*' || c == '@');
        }

        private int SkipSpaces(int i)
        {
            while (IsSpace(fileLine[i]))
            {
                ++i;
            }
            return i;
        }

        private String PrintSortKey(byte[] p)
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < p.Length; ++i)
            {
                if (i > 0)
                {
                    s.Append(' ');
                }
                byte b = p[i];
                if (b == 0)
                {
                    s.Append('.');
                }
                else if (b == 1)
                {
                    s.Append('|');
                }
                else
                {
                    s.Append(string.Format("{0:x2}", b & 0xff));
                }
            }
            return s.ToString();
        }

        private String PrintCollationKey(CollationKey key)
        {
            byte[] p = key.ToByteArray();
            return PrintSortKey(p);
        }

        private bool ReadNonEmptyLine(TextReader input)
        {
            for (; ; )
            {
                String line = input.ReadLine();
                if (line == null)
                {
                    fileLine = null;
                    return false;
                }
                if (fileLineNumber == 0 && line.Length != 0 && line[0] == '\uFEFF')
                {
                    line = line.Substring(1);  // Remove the BOM.
                }
                ++fileLineNumber;
                // Strip trailing comments and spaces
                int idx = line.IndexOf('#');
                if (idx < 0)
                {
                    idx = line.Length;
                }
                while (idx > 0 && IsSpace(line[idx - 1]))
                {
                    --idx;
                }
                if (idx != 0)
                {
                    fileLine = idx < line.Length ? line.Substring(0, idx - 0) : line; // ICU4N: Checked 2nd parameter
                    return true;
                }
                // Empty line, continue.
            }
        }

        private int ParseString(int start, out String prefix, out String s)
        {
            int length = fileLine.Length;
            int i;
            for (i = start; i < length && !IsSpace(fileLine[i]); ++i)
            {
            }
            int pipeIndex = fileLine.IndexOf('|', start);
            if (pipeIndex >= 0 && pipeIndex < i)
            {
                String tmpPrefix = Utility.Unescape(fileLine.Substring(start, pipeIndex - start)); // ICU4N: Corrected 2nd parameter
                if (tmpPrefix.Length == 0)
                {
                    prefix = null;
                    Logln(fileLine);
                    throw new FormatException("empty prefix on line " + fileLineNumber/*, fileLineNumber*/);
                }
                prefix = tmpPrefix;
                start = pipeIndex + 1;
            }
            else
            {
                prefix = null;
            }

            String tmp = Utility.Unescape(fileLine.Substring(start, i - start)); // ICU4N: Corrected 2nd parameter
            if (tmp.Length == 0)
            {
                s = null;
                Logln(fileLine);
                throw new FormatException("empty string on line " + fileLineNumber/*, fileLineNumber*/);
            }
            s = tmp;
            return i;
        }

        private CollationSortKeyLevel ParseRelationAndString(out String s)
        {
            CollationSortKeyLevel relation = CollationSortKeyLevel.Unspecified;
            int start;
            if (fileLine[0] == '<')
            {
                char second = fileLine[1];
                start = 2;
                switch (second)
                {
                    case (char)0x31:  // <1
                        relation = CollationSortKeyLevel.Primary;
                        break;
                    case (char)0x32:  // <2
                        relation = CollationSortKeyLevel.Secondary;
                        break;
                    case (char)0x33:  // <3
                        relation = CollationSortKeyLevel.Tertiary;
                        break;
                    case (char)0x34:  // <4
                        relation = CollationSortKeyLevel.Quaternary;
                        break;
                    case (char)0x63:  // <c
                        relation = CollationSortKeyLevel.Case;
                        break;
                    case (char)0x69:  // <i
                        relation = CollationSortKeyLevel.Identical;
                        break;
                    default:  // just <
                        relation = CollationSortKeyLevel.Unspecified;
                        start = 1;
                        break;
                }
            }
            else if (fileLine[0] == '=')
            {
                relation = CollationSortKeyLevel.Zero;
                start = 1;
            }
            else
            {
                start = 0;
            }

            if (start == 0 || !IsSpace(fileLine[start]))
            {
                Logln(fileLine);
                throw new FormatException("no relation (= < <1 <2 <c <3 <4 <i) at beginning of line "
                                            + fileLineNumber/*, fileLineNumber*/);
            }

            start = SkipSpaces(start);
            String prefixOut;
            start = ParseString(start, out prefixOut, out s);
            if (prefixOut != null)
            {
                Logln(fileLine);
                throw new FormatException("prefix string not allowed for test string: on line "
                                            + fileLineNumber/*, fileLineNumber*/);
            }
            if (start < fileLine.Length)
            {
                Logln(fileLine);
                throw new FormatException("unexpected line contents after test string on line "
                                            + fileLineNumber/*, fileLineNumber*/);
            }

            return relation;
        }

        private void ParseAndSetAttribute()
        {
            // Parse attributes even if the Collator could not be created,
            // in order to report syntax errors.
            int start = SkipSpaces(1);
            int equalPos = fileLine.IndexOf('=');
            if (equalPos < 0)
            {
                if (fileLine.RegionMatches(start, "reorder", 0, 7, StringComparison.Ordinal))
                {
                    ParseAndSetReorderCodes(start + 7);
                    return;
                }
                Logln(fileLine);
                throw new FormatException("missing '=' on line " + fileLineNumber/*, fileLineNumber*/);
            }

            String attrString = fileLine.Substring(start, equalPos - start); // ICU4N: Corrected 2nd parameter
            String valueString = fileLine.Substring(equalPos + 1);
            if (attrString.Equals("maxVariable"))
            {
                int max;
                if (valueString.Equals("space"))
                {
                    max = ReorderCodes.Space;
                }
                else if (valueString.Equals("punct"))
                {
                    max = ReorderCodes.Punctuation;
                }
                else if (valueString.Equals("symbol"))
                {
                    max = ReorderCodes.Symbol;
                }
                else if (valueString.Equals("currency"))
                {
                    max = ReorderCodes.Currency;
                }
                else
                {
                    Logln(fileLine);
                    throw new FormatException("invalid attribute value name on line "
                                                + fileLineNumber/*, fileLineNumber*/);
                }
                if (coll != null)
                {
                    coll.MaxVariable = max;
                }
                fileLine = null;
                return;
            }

            bool parsed = true;
            RuleBasedCollator rbc = (RuleBasedCollator)coll;
            if (attrString.Equals("backwards"))
            {
                if (valueString.Equals("on"))
                {
                    if (rbc != null) rbc.IsFrenchCollation = (true);
                }
                else if (valueString.Equals("off"))
                {
                    if (rbc != null) rbc.IsFrenchCollation = (false);
                }
                else if (valueString.Equals("default"))
                {
                    if (rbc != null) rbc.SetFrenchCollationToDefault();
                }
                else
                {
                    parsed = false;
                }
            }
            else if (attrString.Equals("alternate"))
            {
                if (valueString.Equals("non-ignorable"))
                {
                    if (rbc != null) rbc.IsAlternateHandlingShifted = (false);
                }
                else if (valueString.Equals("shifted"))
                {
                    if (rbc != null) rbc.IsAlternateHandlingShifted = (true);
                }
                else if (valueString.Equals("default"))
                {
                    if (rbc != null) rbc.SetAlternateHandlingToDefault();
                }
                else
                {
                    parsed = false;
                }
            }
            else if (attrString.Equals("caseFirst"))
            {
                if (valueString.Equals("upper"))
                {
                    if (rbc != null) rbc.IsUpperCaseFirst = (true);
                }
                else if (valueString.Equals("lower"))
                {
                    if (rbc != null) rbc.IsLowerCaseFirst = (true);
                }
                else if (valueString.Equals("default"))
                {
                    if (rbc != null) rbc.SetCaseFirstToDefault();
                }
                else
                {
                    parsed = false;
                }
            }
            else if (attrString.Equals("caseLevel"))
            {
                if (valueString.Equals("on"))
                {
                    if (rbc != null) rbc.IsCaseLevel = (true);
                }
                else if (valueString.Equals("off"))
                {
                    if (rbc != null) rbc.IsCaseLevel = (false);
                }
                else if (valueString.Equals("default"))
                {
                    if (rbc != null) rbc.SetCaseLevelToDefault();
                }
                else
                {
                    parsed = false;
                }
            }
            else if (attrString.Equals("strength"))
            {
                if (valueString.Equals("primary"))
                {
                    if (rbc != null) rbc.Strength = CollationStrength.Primary;//  (Collator.PRIMARY);
                }
                else if (valueString.Equals("secondary"))
                {
                    if (rbc != null) rbc.Strength = CollationStrength.Secondary; // (Collator.SECONDARY);
                }
                else if (valueString.Equals("tertiary"))
                {
                    if (rbc != null) rbc.Strength = CollationStrength.Tertiary; // (Collator.TERTIARY);
                }
                else if (valueString.Equals("quaternary"))
                {
                    if (rbc != null) rbc.Strength = CollationStrength.Quaternary;// (Collator.QUATERNARY);
                }
                else if (valueString.Equals("identical"))
                {
                    if (rbc != null) rbc.Strength = CollationStrength.Identical; // (Collator.IDENTICAL);
                }
                else if (valueString.Equals("default"))
                {
                    if (rbc != null) rbc.SetStrengthToDefault();
                }
                else
                {
                    parsed = false;
                }
            }
            else if (attrString.Equals("numeric"))
            {
                if (valueString.Equals("on"))
                {
                    if (rbc != null) rbc.IsNumericCollation = (true);
                }
                else if (valueString.Equals("off"))
                {
                    if (rbc != null) rbc.IsNumericCollation = (false);
                }
                else if (valueString.Equals("default"))
                {
                    if (rbc != null) rbc.SetNumericCollationToDefault();
                }
                else
                {
                    parsed = false;
                }
            }
            else
            {
                Logln(fileLine);
                throw new FormatException("invalid attribute name on line "
                                            + fileLineNumber/*, fileLineNumber*/);
            }
            if (!parsed)
            {
                Logln(fileLine);
                throw new FormatException(
                        "invalid attribute value name or attribute=value combination on line "
                        + fileLineNumber/*, fileLineNumber*/);
            }

            fileLine = null;
        }

        private void ParseAndSetReorderCodes(int start)
        {
            IList<int> reorderCodes = new List<int>();
            while (start < fileLine.Length)
            {
                start = SkipSpaces(start);
                int limit = start;
                while (limit < fileLine.Length && !IsSpace(fileLine[limit]))
                {
                    ++limit;
                }
                String name = fileLine.Substring(start, limit - start); // ICU4N: Corrected 2nd parameter
                int code = CollationRuleParser.GetReorderCode(name);
                if (code < -1)
                {
                    if (name.Equals("default", StringComparison.OrdinalIgnoreCase))
                    {
                        code = ReorderCodes.Default;  // -1
                    }
                    else
                    {
                        Logln(fileLine);
                        throw new FormatException("invalid reorder code '" + name + "' on line "
                                                    + fileLineNumber/*, fileLineNumber*/);
                    }
                }
                reorderCodes.Add(code);
                start = limit;
            }
            if (coll != null)
            {
                int[] reorderCodesArray = new int[reorderCodes.Count];
                //System.Array.Copy(reorderCodes.getBuffer(), 0,
                //                    reorderCodesArray, 0, reorderCodes.Count);
                reorderCodes.CopyTo(reorderCodesArray, 0);
                coll.SetReorderCodes(reorderCodesArray);
            }

            fileLine = null;
        }

        private void BuildTailoring(TextReader input)
        {
            StringBuilder rules = new StringBuilder();
            while (ReadNonEmptyLine(input) && !IsSectionStarter(fileLine[0]))
            {
                rules.Append(Utility.Unescape(fileLine));
            }

            try
            {
                coll = new RuleBasedCollator(rules.ToString());
            }
            catch (Exception e)
            {
                Logln(rules.ToString());
                Errln("RuleBasedCollator(rules) failed - " + e.ToString());
                coll = null;
            }
        }

        private void SetRootCollator()
        {
            coll = Collator.GetInstance(UCultureInfo.InvariantCulture);
        }

        private void SetLocaleCollator()
        {
            coll = null;
            UCultureInfo locale = null;
            if (fileLine.Length > 9)
            {
                String localeID = fileLine.Substring(9); // "@ locale <langTag>"
                try
                {
                    locale = new UCultureInfo(localeID);  // either locale ID or language tag
                }
                catch (IllformedLocaleException e)
                {
                    locale = null;
                }
            }
            if (locale == null)
            {
                Logln(fileLine);
                Errln("invalid language tag on line " + fileLineNumber);
                return;
            }

            Logln("creating a collator for locale ID " + locale.FullName);
            try
            {
                coll = Collator.GetInstance(locale);
            }
            catch (Exception e)
            {
                Errln("unable to create a collator for locale " + locale +
                        " on line " + fileLineNumber + " - " + e);
            }
        }

        private bool NeedsNormalization(String s)
        {
            if (!fcd.IsNormalized(s))
            {
                return true;
            }
            // In some sequences with Tibetan composite vowel signs,
            // even if the string passes the FCD check,
            // those composites must be decomposed.
            // Check if s contains 0F71 immediately followed by 0F73 or 0F75 or 0F81.
            int index = 0;
            while ((index = s.IndexOf((char)0xf71, index)) >= 0)
            {
                if (++index < s.Length)
                {
                    char c = s[index];
                    if (c == 0xf73 || c == 0xf75 || c == 0xf81)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool GetCollationKey(String norm, String line, String s, out CollationKey keyOut)
        {
            CollationKey key = coll.GetCollationKey(s);
            keyOut = key;

            byte[] keyBytes = key.ToByteArray();
            if (keyBytes.Length == 0 || keyBytes[keyBytes.Length - 1] != 0)
            {
                Logln(fileTestName);
                Logln(line);
                Logln(PrintCollationKey(key));
                Errln("Collator(" + norm + ").GetSortKey() wrote an empty or unterminated key");
                return false;
            }

            int numLevels = (int)coll.Strength;
            if (numLevels < (int)CollationStrength.Identical)
            {
                ++numLevels;
            }
            else
            {
                numLevels = 5;
            }
            if (((RuleBasedCollator)coll).IsCaseLevel)
            {
                ++numLevels;
            }
            int numLevelSeparators = 0;
            for (int i = 0; i < (keyBytes.Length - 1); ++i)
            {
                byte b = keyBytes[i];
                if (b == 0)
                {
                    Logln(fileTestName);
                    Logln(line);
                    Logln(PrintCollationKey(key));
                    Errln("Collator(" + norm + ").GetSortKey() contains a 00 byte");
                    return false;
                }
                if (b == 1)
                {
                    ++numLevelSeparators;
                }
            }
            if (numLevelSeparators != ((int)numLevels - 1))
            {
                Logln(fileTestName);
                Logln(line);
                Logln(PrintCollationKey(key));
                Errln("Collator(" + norm + ").GetSortKey() has "
                        + numLevelSeparators + " level separators for "
                        + numLevels + " levels");
                return false;
            }

            // No nextSortKeyPart support in ICU4J

            return true;
        }

        /**
         * Changes the key to the merged segments of the U+FFFE-separated substrings of s.
         * Leaves key unchanged if s does not contain U+FFFE.
         * @return true if the key was successfully changed
         */
        private bool GetMergedCollationKey(String s, ref CollationKey key)
        {
            CollationKey mergedKey = null;
            int sLength = s.Length;
            int segmentStart = 0;
            for (int i = 0; ;)
            {
                if (i == sLength)
                {
                    if (segmentStart == 0)
                    {
                        // s does not contain any U+FFFE.
                        return false;
                    }
                }
                else if (s[i] != '\uFFFE')
                {
                    ++i;
                    continue;
                }
                // Get the sort key for another segment and merge it into mergedKey.
                CollationKey tmpKey = coll.GetCollationKey(s.Substring(segmentStart, i - segmentStart)); // ICU4N: Corrected 2nd parameter
                if (mergedKey == null)
                {
                    mergedKey = tmpKey;
                }
                else
                {
                    mergedKey = mergedKey.Merge(tmpKey);
                }
                if (i == sLength)
                {
                    break;
                }
                segmentStart = ++i;
            }
            key = mergedKey;
            return true;
        }

        private static CollationSortKeyLevel GetDifferenceLevel(CollationKey prevKey, CollationKey key,
                int order, bool collHasCaseLevel)
        {
            if (order == Collation.Equal)
            {
                return CollationSortKeyLevel.Unspecified; // Collation.NO_LEVEL;
            }
            byte[] prevBytes = prevKey.ToByteArray();
            byte[] bytes = key.ToByteArray();
            CollationSortKeyLevel level = CollationSortKeyLevel.Primary; // Collation.PRIMARY_LEVEL;
            for (int i = 0; ; ++i)
            {
                byte b = prevBytes[i];
                if (b != bytes[i])
                {
                    break;
                }
                if (b == Collation.LevelSeparatorByte)
                {
                    ++level;
                    if (level == CollationSortKeyLevel.Case && !collHasCaseLevel)
                    {
                        ++level;
                    }
                }
            }
            return level;
        }

        private bool CheckCompareTwo(String norm, String prevFileLine, String prevString, String s,
                                        int expectedOrder, CollationSortKeyLevel expectedLevel)
        {
            // Get the sort keys first, for error debug output.
            CollationKey prevKeyOut = null;
            CollationKey prevKey;
            if (!GetCollationKey(norm, fileLine, prevString, out prevKeyOut))
            {
                return false;
            }
            prevKey = prevKeyOut;

            CollationKey keyOut;
            CollationKey key;
            if (!GetCollationKey(norm, fileLine, s, out keyOut))
            {
                return false;
            }
            key = keyOut;

            int order = coll.Compare(prevString, s);
            if (order != expectedOrder)
            {
                Logln(fileTestName);
                Logln(prevFileLine);
                Logln(fileLine);
                Logln(PrintCollationKey(prevKey));
                Logln(PrintCollationKey(key));
                Errln("line " + fileLineNumber
                        + " Collator(" + norm + ").compare(previous, current) wrong order: "
                        + order + " != " + expectedOrder);
                return false;
            }
            order = coll.Compare(s, prevString);
            if (order != -expectedOrder)
            {
                Logln(fileTestName);
                Logln(prevFileLine);
                Logln(fileLine);
                Logln(PrintCollationKey(prevKey));
                Logln(PrintCollationKey(key));
                Errln("line " + fileLineNumber
                        + " Collator(" + norm + ").compare(current, previous) wrong order: "
                        + order + " != " + -expectedOrder);
                return false;
            }

            order = prevKey.CompareTo(key);
            if (order != expectedOrder)
            {
                Logln(fileTestName);
                Logln(prevFileLine);
                Logln(fileLine);
                Logln(PrintCollationKey(prevKey));
                Logln(PrintCollationKey(key));
                Errln("line " + fileLineNumber
                        + " Collator(" + norm + ").GetSortKey(previous, current).compareTo() wrong order: "
                        + order + " != " + expectedOrder);
                return false;
            }
            bool collHasCaseLevel = ((RuleBasedCollator)coll).IsCaseLevel;
            CollationSortKeyLevel level = GetDifferenceLevel(prevKey, key, order, collHasCaseLevel);
            if (order != Collation.Equal && expectedLevel != CollationSortKeyLevel.Unspecified)
            {
                if (level != expectedLevel)
                {
                    Logln(fileTestName);
                    Logln(prevFileLine);
                    Logln(fileLine);
                    Logln(PrintCollationKey(prevKey));
                    Logln(PrintCollationKey(key));
                    Errln("line " + fileLineNumber
                            + " Collator(" + norm + ").GetSortKey(previous, current).compareTo()="
                            + order + " wrong level: " + level + " != " + expectedLevel);
                    return false;
                }
            }

            // If either string contains U+FFFE, then their sort keys must compare the same as
            // the merged sort keys of each string's between-FFFE segments.
            //
            // It is not required that
            //   sortkey(str1 + "\uFFFE" + str2) == mergeSortkeys(sortkey(str1), sortkey(str2))
            // only that those two methods yield the same order.
            //
            // Use bit-wise OR so that getMergedCollationKey() is always called for both strings.
            CollationKey outPrevKey = prevKey;
            CollationKey outKey = key;
            if (GetMergedCollationKey(prevString, ref outPrevKey) | GetMergedCollationKey(s, ref outKey))
            {
                prevKey = outPrevKey;
                key = outKey;
                order = prevKey.CompareTo(key);
                if (order != expectedOrder)
                {
                    Logln(fileTestName);
                    Errln("line " + fileLineNumber
                            + " Collator(" + norm + ").getCollationKey"
                            + "(previous, current segments between U+FFFE)).merge().compareTo() wrong order: "
                            + order + " != " + expectedOrder);
                    Logln(prevFileLine);
                    Logln(fileLine);
                    Logln(PrintCollationKey(prevKey));
                    Logln(PrintCollationKey(key));
                    return false;
                }
                CollationSortKeyLevel mergedLevel = GetDifferenceLevel(prevKey, key, order, collHasCaseLevel);
                if (order != Collation.Equal && expectedLevel != CollationSortKeyLevel.Unspecified)
                {
                    if (mergedLevel != level)
                    {
                        Logln(fileTestName);
                        Errln("line " + fileLineNumber
                            + " Collator(" + norm + ").getCollationKey"
                            + "(previous, current segments between U+FFFE)).merge().compareTo()="
                            + order + " wrong level: " + mergedLevel + " != " + level);
                        Logln(prevFileLine);
                        Logln(fileLine);
                        Logln(PrintCollationKey(prevKey));
                        Logln(PrintCollationKey(key));
                        return false;
                    }
                }
            }
            return true;
        }

        private void CheckCompareStrings(TextReader input)
        {
            String prevFileLine = "(none)";
            String prevString = "";
            String sOut;
            while (ReadNonEmptyLine(input) && !IsSectionStarter(fileLine[0]))
            {
                // Parse the line even if it will be ignored (when we do not have a Collator)
                // in order to report syntax issues.
                CollationSortKeyLevel relation;
                try
                {
                    relation = ParseRelationAndString(out sOut);
                }
                catch (FormatException pe)
                {
                    Errln(pe.ToString());
                    break;
                }
                if (coll == null)
                {
                    // We were unable to create the Collator but continue with tests.
                    // Ignore test data for this Collator.
                    // The next Collator creation might work.
                    continue;
                }
                String s = sOut;
                int expectedOrder = (relation == CollationSortKeyLevel.Zero) ? Collation.Equal : Collation.Less;
                CollationSortKeyLevel expectedLevel = relation;
                bool isOk = true;
                if (!NeedsNormalization(prevString) && !NeedsNormalization(s))
                {
                    coll.Decomposition = NormalizationMode.NoDecomposition;//  (Collator.NO_DECOMPOSITION);
                    isOk = CheckCompareTwo("normalization=off", prevFileLine, prevString, s,
                                            expectedOrder, expectedLevel);
                }
                if (isOk)
                {
                    coll.Decomposition = NormalizationMode.CanonicalDecomposition;// (Collator.CANONICAL_DECOMPOSITION);
                    isOk = CheckCompareTwo("normalization=on", prevFileLine, prevString, s,
                                            expectedOrder, expectedLevel);
                }
                if (isOk && (!nfd.IsNormalized(prevString) || !nfd.IsNormalized(s)))
                {
                    String pn = nfd.Normalize(prevString);
                    String n = nfd.Normalize(s);
                    isOk = CheckCompareTwo("NFD input", prevFileLine, pn, n,
                                            expectedOrder, expectedLevel);
                }
                prevFileLine = fileLine;
                prevString = s;
            }
        }

        [Test]
        public void TestDataDriven()
        {
            nfd = Normalizer2.GetNFDInstance();
            fcd = Norm2AllModes.GetFCDNormalizer2();

            TextReader input = null;

            try
            {
                input = TestUtil.GetDataReader("collationtest.txt", "UTF-8");

                // Read a new line if necessary.
                // Sub-parsers leave the first line set that they do not handle.
                while (fileLine != null || ReadNonEmptyLine(input))
                {
                    if (!IsSectionStarter(fileLine[0]))
                    {
                        Logln(fileLine);
                        Errln("syntax error on line " + fileLineNumber);
                        return;
                    }
                    if (fileLine.StartsWith("** test: ", StringComparison.Ordinal))
                    {
                        fileTestName = fileLine;
                        Logln(fileLine);
                        fileLine = null;
                    }
                    else if (fileLine.Equals("@ root"))
                    {
                        SetRootCollator();
                        fileLine = null;
                    }
                    else if (fileLine.StartsWith("@ locale ", StringComparison.Ordinal))
                    {
                        SetLocaleCollator();
                        fileLine = null;
                    }
                    else if (fileLine.Equals("@ rules"))
                    {
                        BuildTailoring(input);
                    }
                    else if (fileLine[0] == '%'
                          && fileLine.Length > 1 && IsSpace(fileLine[1]))
                    {
                        ParseAndSetAttribute();
                    }
                    else if (fileLine.Equals("* compare"))
                    {
                        CheckCompareStrings(input);
                    }
                    else
                    {
                        Logln(fileLine);
                        Errln("syntax error on line " + fileLineNumber);
                        return;
                    }
                }
            }
            catch (FormatException pe)
            {
                Errln(pe.ToString());
            }
            catch (IOException e)
            {
                Errln(e.Message);
            }
            finally
            {
                try
                {
                    if (input != null)
                    {
                        input.Dispose();
                    }
                }
                catch (IOException e)
                {
                    e.PrintStackTrace();
                }
            }
        }
    }
}
