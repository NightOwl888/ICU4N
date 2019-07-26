using ICU4N.Dev.Util;
using ICU4N.Impl;
using ICU4N.Globalization;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Character = ICU4N.Support.Character;
using Double = ICU4N.Support.Double;
using Integer = ICU4N.Support.Integer;
using ICU4N.Support;

namespace ICU4N.Dev.Test.Translit
{
    internal static class UnicodeMapExtensions
    {
        public static UnicodeMap<Double> Put(this UnicodeMap<Double> map, int codePoint, double value)
        {
            return map.Put(codePoint, new Double(value));
        }

        public static UnicodeMap<Double> Put(this UnicodeMap<Double> map, string str, double value)
        {
            return map.Put(str, new Double(value));
        }

        public static UnicodeMap<Integer> Put(this UnicodeMap<Integer> map, int codePoint, int value)
        {
            return map.Put(codePoint, new Integer(value));
        }

        public static UnicodeMap<Integer> Put(this UnicodeMap<Integer> map, string str, int value)
        {
            return map.Put(str, new Integer(value));
        }

        public static UnicodeMap<Character> Put(this UnicodeMap<Character> map, int codePoint, char value)
        {
            return map.Put(codePoint, new Character(value));
        }

        public static UnicodeMap<Character> Put(this UnicodeMap<Character> map, string str, char value)
        {
            return map.Put(str, new Character(value));
        }

        public static UnicodeMap<Double> PutAll(this UnicodeMap<Double> map, int startCodePoint, int endCodePoint, double value)
        {
            return map.PutAll(startCodePoint, endCodePoint, new Double(value));
        }

        public static UnicodeMap<Integer> PutAll(this UnicodeMap<Integer> map, int startCodePoint, int endCodePoint, int value)
        {
            return map.PutAll(startCodePoint, endCodePoint, new Integer(value));
        }

        public static UnicodeMap<Character> PutAll(this UnicodeMap<Character> map, int startCodePoint, int endCodePoint, char value)
        {
            return map.PutAll(startCodePoint, endCodePoint, new Character(value));
        }
    }

    /// <summary>
    /// General test of UnicodeSet
    /// </summary>
    public class UnicodeMapTest : TestFmwk
    {
        internal static readonly int MODIFY_TEST_LIMIT = 32;
        internal static readonly int MODIFY_TEST_ITERATIONS = 100000;



        [Test]
        public void TestIterations()
        {
            UnicodeMap<Double> foo = new UnicodeMap<Double>();
            checkToString(foo, "");
            foo.Put(3, 6d).Put(5, 10d);
            checkToString(foo, "0003=6.0\n0005=10.0\n");
            foo.Put(0x10FFFF, 666d);
            checkToString(foo, "0003=6.0\n0005=10.0\n10FFFF=666.0\n");
            foo.Put("neg", -555d);
            checkToString(foo, "0003=6.0\n0005=10.0\n10FFFF=666.0\n006E,0065,0067=-555.0\n");

            double i = 0;
            foreach (var entryRange in foo.GetEntryRanges())
            {
                i += entryRange.Value.Value;
            }
            assertEquals("EntryRange<T>", 127d, i);
        }

        public void checkToString(UnicodeMap<Double> foo, String expected)
        {
            assertEquals("EntryRange<T>", expected, string.Join("\n", foo.GetEntryRanges().Select(r => r.ToString()).ToArray()) + (foo.Count == 0 ? "" : "\n"));
            assertEquals("EntryRange<T>", expected, foo.ToString());
        }

        [Test]
        public void TestRemove()
        {
            UnicodeMap<Double> foo = new UnicodeMap<Double>()
            .PutAll(0x20, 0x29, -2d)
            .Put("abc", 3d)
            .Put("xy", 2d)
            .Put("mark", 4d)
            .Freeze();
            UnicodeMap<Double> fii = new UnicodeMap<Double>()
            .PutAll(0x21, 0x25, -2d)
            .PutAll(0x26, 0x28, -3d)
            .Put("abc", 3d)
            .Put("mark", 999d)
            .Freeze();

            UnicodeMap<Double> afterFiiRemoval = new UnicodeMap<Double>()
            .Put(0x20, -2d)
            .PutAll(0x26, 0x29, -2d)
            .Put("xy", 2d)
            .Put("mark", 4d)
            .Freeze();

            UnicodeMap<Double> afterFiiRetained = new UnicodeMap<Double>()
            .PutAll(0x21, 0x25, -2d)
            .Put("abc", 3d)
            .Freeze();

            UnicodeMap<Double> test = new UnicodeMap<Double>().PutAll(foo)
                    .RemoveAll(fii);
            assertEquals("removeAll", afterFiiRemoval, test);

            test = new UnicodeMap<Double>().PutAll(foo)
                    .RetainAll(fii);
            assertEquals("retainAll", afterFiiRetained, test);
        }

        [Test]
        public void TestAMonkey()
        {
            SortedDictionary<String, Integer> stayWithMe = new SortedDictionary<String, Integer>(OneFirstComparator);

            UnicodeMap<Integer> me = new UnicodeMap<Integer>().PutAll(stayWithMe);
            // check one special case, removal near end
            me.PutAll(0x10FFFE, 0x10FFFF, 666);
            me.Remove(0x10FFFF);

            int iterations = 100000;
            SortedDictionary<String, Integer> test = new SortedDictionary<string, Integer>(StringComparer.Ordinal);

            Random rand = new Random(0);
            String other;
            Integer value;
            // try modifications
            for (int i = 0; i < iterations; ++i)
            {
                switch (i == 0 ? 0 : rand.Next(20))
                {
                    case 0:
                        Logln("clear");
                        stayWithMe.Clear();
                        me.Clear();
                        break;
                    case 1:
                        fillRandomMap(rand, 5, test);
                        Logln("putAll\t" + test);
                        stayWithMe.PutAll(test);
                        me.PutAll(test);
                        break;
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                        other = GetRandomKey(rand);
                        //                if (other.equals("\uDBFF\uDFFF") && me.containsKey(0x10FFFF) && me.get(0x10FFFF).equals(me.get(0x10FFFE))) {
                        //                    System.out.println("Remove\t" + other + "\n" + me);
                        //                }
                        Logln("remove\t" + other);
                        stayWithMe.Remove(other);
                        try
                        {
                            me.Remove(other);
                        }
                        catch (ArgumentException e)
                        {
                            Errln("remove\t" + other + "\tfailed: " + e.ToString() + "\n" + me);
                            me.Clear();
                            stayWithMe.Clear();
                        }
                        break;
                    default:
                        other = GetRandomKey(rand);
                        value = new Integer(rand.Next(50) + 50);
                        Logln("put\t" + other + " = " + value);
                        stayWithMe[other] = value;
                        me.Put(other, value);
                        break;
                }
                checkEquals(me, stayWithMe);
            }
        }

        /**
         * @param rand
         * @param nextInt
         * @param test
         * @return
         */
        private SortedDictionary<String, Integer> fillRandomMap(Random rand, int max, SortedDictionary<String, Integer> test)
        {
            test.Clear();
            max = rand.Next(max);
            for (int i = 0; i < max; ++i)
            {
                test[GetRandomKey(rand)] = new Integer(rand.Next(50) + 50);
            }
            return test;
        }

        ISet<KeyValuePair<string, Integer>> temp = new HashSet<KeyValuePair<string, Integer>>();
        /**
         * @param me
         * @param stayWithMe
         */
        private void checkEquals(UnicodeMap<Integer> me, SortedDictionary<String, Integer> stayWithMe)
        {
            temp.Clear();
            foreach (var e in me.EntrySet())
            {
                temp.Add(e);
            }
            ISet<KeyValuePair<String, Integer>> entrySet = new HashSet<KeyValuePair<string, Integer>>(stayWithMe);
            if (!entrySet.SetEquals(temp))
            {
                Logln(me.EntrySet().ToString());
                Logln(me.ToString());
                assertEquals("are in parallel", entrySet, temp);
                // we failed. Reset and start again
                entrySet.Clear();
                temp.Clear();
                return;
            }
            foreach (String key in stayWithMe.Keys)
            {
                assertEquals("containsKey", stayWithMe.ContainsKey(key), me.ContainsKey(key));
                Integer value = stayWithMe.Get(key);
                assertEquals("get", value, me.Get(key));
                assertEquals("containsValue", stayWithMe.ContainsValue(value), me.ContainsValue(value));
                int cp = UnicodeSet.GetSingleCodePoint(key);
                if (cp != int.MaxValue)
                {
                    assertEquals("get", value, me.Get(cp));
                }
            }
            // ICU4N TODO: complete implementation
            //ISet<String> nonCodePointStrings = stayWithMe.tailMap("").keySet();
            //if (nonCodePointStrings.Count == 0) nonCodePointStrings = null; // for parallel api
            //assertEquals("getNonRangeStrings", nonCodePointStrings, me.GetNonRangeStrings());

            SortedSet<Integer> values = new SortedSet<Integer>(stayWithMe.Values);
            SortedSet<Integer> myValues = new SortedSet<Integer>(me.Values());
            assertEquals("values", myValues, values);

            foreach (String key in stayWithMe.Keys)
            {
                assertEquals("containsKey", stayWithMe.ContainsKey(key), me.ContainsKey(key));
            }
        }

        private class OneFirstComparer : IComparer<string>
        {
            public int Compare(string o1, string o2)
            {
                int cp1 = UnicodeSet.GetSingleCodePoint(o1);
                int cp2 = UnicodeSet.GetSingleCodePoint(o2);
                int result = cp1 - cp2;
                if (result != 0)
                {
                    return result;
                }
                if (cp1 == int.MaxValue)
                {
                    return o1.CompareToOrdinal(o2);
                }
                return 0;
            }
        }

        internal static IComparer<String> OneFirstComparator = new OneFirstComparer();

        /**
         * @param rand
         * @param others
         * @return
         */
        private String GetRandomKey(Random rand)
        {
            int r = rand.Next(30);
            if (r == 0)
            {
                return UTF16.ValueOf(r);
            }
            else if (r < 10)
            {
                return UTF16.ValueOf('A' - 1 + r);
            }
            else if (r < 20)
            {
                return UTF16.ValueOf(0x10FFFF - (r - 10));
                //        } else if (r == 20) {
                //            return "";
            }
            return "a" + UTF16.ValueOf(r + 'a' - 1);
        }

        [Test]
        public void TestModify()
        {
            Random random = new Random(0);
            UnicodeMap<string> unicodeMap = new UnicodeMap<string>();
            Dictionary<int, string> hashMap = new Dictionary<int, string>();
            String[] values = { null, "the", "quick", "brown", "fox" };
            for (int count = 1; count <= MODIFY_TEST_ITERATIONS; ++count)
            {
                String value = values[random.Next(values.Length)];
                int start = random.Next(MODIFY_TEST_LIMIT); // test limited range
                int end = random.Next(MODIFY_TEST_LIMIT);
                if (start > end)
                {
                    int temp = start;
                    start = end;
                    end = temp;
                }
                int modCount = count & 0xFF;
                if (modCount == 0 && IsVerbose())
                {
                    Logln("***" + count);
                    Logln(unicodeMap.ToString());
                }
                unicodeMap.PutAll(start, end, value);
                if (modCount == 1 && IsVerbose())
                {
                    Logln(">>>\t" + Utility.Hex(start) + ".." + Utility.Hex(end) + "\t" + value);
                    Logln(unicodeMap.ToString());
                }
                for (int i = start; i <= end; ++i)
                {
                    hashMap[i] = value;
                }
                if (!hasSameValues(unicodeMap, hashMap))
                {
                    Errln("Failed at " + count);
                }
            }
        }

        private bool hasSameValues(UnicodeMap<string> unicodeMap, IDictionary<int, string> hashMap)
        {
            for (int i = 0; i < MODIFY_TEST_LIMIT; ++i)
            {
                Object unicodeMapValue = unicodeMap.GetValue(i);
                Object hashMapValue = hashMap.Get(i);
                if (unicodeMapValue != hashMapValue)
                {
                    return false;
                }
            }
            return true;
        }

        [Test]
        public void TestCloneAsThawed11721()
        {
            UnicodeMap<Integer> test = new UnicodeMap<Integer>().Put("abc", 3).Freeze();
            UnicodeMap<Integer> copy = test.CloneAsThawed();
            copy.Put("def", 4);
            assertEquals("original-abc", new Integer(3), test.Get("abc"));
            assertNull("original-def", test.Get("def"));
            assertEquals("copy-def", new Integer(4), copy.Get("def"));
        }

        private static readonly int LIMIT = 0x15; // limit to make testing more realistic in terms of collisions
        private static readonly int ITERATIONS = 1000000;
        private static readonly bool SHOW_PROGRESS = false;
        private static readonly bool DEBUG = false;

        SortedSet<string> log = new SortedSet<string>();
        static string[] TEST_VALUES = { "A", "B", "C", "D", "E", "F" };
        static Random random = new Random(12345);

        [Test]
        public void TestUnicodeMapRandom()
        {
            // do random change to both, then compare
            var random = new Random(12345); // reproducible results
            Logln("Comparing against HashMap");
            UnicodeMap<string> map1 = new UnicodeMap<string>();
            IDictionary<Integer, string> map2 = new Dictionary<Integer, string>();
            for (int counter = 0; counter < ITERATIONS; ++counter)
            {
                int start = random.Next(LIMIT);
                string value = TEST_VALUES[random.Next(TEST_VALUES.Length)];
                string logline = Utility.Hex(start) + "\t" + value;
                if (SHOW_PROGRESS) Logln(counter + "\t" + logline);
                log.Add(logline);
                if (DEBUG && counter == 144)
                {
                    Console.Out.WriteLine(" debug");
                }
                map1.Put(start, value);
                map2[new Integer(start)] = value;
                check(map1, map2, counter);
            }
            checkNext(map1, map2, LIMIT);
        }

        private static readonly int SET_LIMIT = 0x10FFFF;
        private static readonly UProperty propEnum = UProperty.General_Category;

        [Test]
        public void TestUnicodeMapGeneralCategory()
        {
            Logln("Setting General Category");
            UnicodeMap<String> map1 = new UnicodeMap<string>();
            IDictionary<Integer, String> map2 = new Dictionary<Integer, String>();
            //Map<Integer, String> map3 = new TreeMap<Integer, String>();
            map1 = new UnicodeMap<String>();
            map2 = new SortedDictionary<Integer, String>();

            for (int cp = 0; cp <= SET_LIMIT; ++cp)
            {
                int enumValue = UChar.GetInt32PropertyValue(cp, propEnum);
                //if (enumValue <= 0) continue; // for smaller set
                String value = UChar.GetPropertyValueName(propEnum, enumValue, NameChoice.Long);
                map1.Put(cp, value);
                map2[new Integer(cp)] = value;
            }
            checkNext(map1, map2, int.MaxValue);

            Logln("Comparing General Category");
            check(map1, map2, -1);
            Logln("Comparing Values");
            ISet<String> values1 = new SortedSet<String>(StringComparer.Ordinal); map1.GetAvailableValues(values1);
            ISet<String> values2 = new SortedSet<String>(map2.Values.Distinct(), StringComparer.Ordinal); // ICU4N NOTE: Added Distinct()
            if (!TestBoilerplate<string>.VerifySetsIdentical(this, values1, values2))
            {
                throw new ArgumentException("Halting");
            }
            Logln("Comparing Sets");
            foreach (string value in values1)
            {
                Logln(value == null ? "null" : value);
                UnicodeSet set1 = map1.KeySet(value);
                UnicodeSet set2 = TestBoilerplate<string>.GetSet(map2, value);
                if (!TestBoilerplate<string>.VerifySetsIdentical(this, set1, set2))
                {
                    throw new ArgumentException("Halting");
                }
            }
        }

        [Test]
        public void TestAUnicodeMap2()
        {
            UnicodeMap<object> foo = new UnicodeMap<object>();

            int hash = foo.GetHashCode(); // make sure doesn't NPE

            var fii = foo.StringKeys(); // make sure doesn't NPE
        }

        [Test]
        public void TestAUnicodeMapInverse()
        {
            UnicodeMap<Character> foo1 = new UnicodeMap<Character>()
                    .PutAll('a', 'z', 'b')
                    .Put("ab", 'c')
                    .Put('x', 'b')
                    .Put("xy", 'c')
                    ;
            IDictionary<Character, UnicodeSet> target = new Dictionary<Character, UnicodeSet>();
            foo1.AddInverseTo(target);
            UnicodeMap<Character> reverse = new UnicodeMap<Character>().PutAllInverse(target);
            assertEquals("", foo1, reverse);
        }

        private void checkNext(UnicodeMap<String> map1, IDictionary<Integer, string> map2, int limit)
        {
            Logln("Comparing nextRange");
            IDictionary<Integer, string> localMap = new SortedDictionary<Integer, string>();
            UnicodeMapIterator<String> mi = new UnicodeMapIterator<String>(map1);
            while (mi.NextRange())
            {
                Logln(Utility.Hex(mi.Codepoint) + ".." + Utility.Hex(mi.CodepointEnd) + " => " + mi.Value);
                for (int i = mi.Codepoint; i <= mi.CodepointEnd; ++i)
                {
                    //if (i >= limit) continue;
                    localMap[new Integer(i)] = mi.Value;
                }
            }
            checkMap(map2, localMap);

            Logln("Comparing next");
            mi.Reset();
            localMap = new SortedDictionary<Integer, string>();
            //        String lastValue = null;
            while (mi.Next())
            {
                //            if (!UnicodeMap.areEqual(lastValue, mi.value)) {
                //                // System.out.println("Change: " + Utility.hex(mi.codepoint) + " => " + mi.value);
                //                lastValue = mi.value;
                //            }
                //if (mi.codepoint >= limit) continue;
                localMap[new Integer(mi.Codepoint)] = mi.Value;
            }
            checkMap(map2, localMap);
        }

        public void check(UnicodeMap<String> map1, IDictionary<Integer, String> map2, int counter)
        {
            for (int i = 0; i < LIMIT; ++i)
            {
                String value1 = map1.GetValue(i);
                String value2 = map2.Get(new Integer(i));
                if (!UnicodeMap<string>.AreEqual(value1, value2))
                {
                    Errln(counter + " Difference at " + Utility.Hex(i)
                         + "\t UnicodeMap: " + value1
                         + "\t HashMap: " + value2);
                    Errln("UnicodeMap: " + map1);
                    Errln("Log: " + TestBoilerplate<string>.Show(log));
                    Errln("HashMap: " + TestBoilerplate<string>.Show(map2));
                }
            }
        }

        internal void checkMap(IDictionary<Integer, string> m1, IDictionary<Integer, string> m2)
        {
            if (CollectionUtil.Equals(m1, m2)) return;
            StringBuilder buffer = new StringBuilder();
            ICollection<KeyValuePair<Integer, string>> m1entries = m1;
            ICollection<KeyValuePair<Integer, string>> m2entries = m2;
            getEntries("\r\nIn First, and not Second", m1entries, m2entries, buffer, 20);
            getEntries("\r\nIn Second, and not First", m2entries, m1entries, buffer, 20);
            Errln(buffer.ToString());
        }

        private class EntryComparer : IComparer<KeyValuePair<Integer, string>>
        {
            public int Compare(KeyValuePair<Integer, string> o1, KeyValuePair<Integer, string> o2)
            {
                //if (o1 == o2) return 0;
                //if (o1 == null) return -1;
                //if (o2 == null) return 1;
                if (ReferenceEquals(o1, 02)) return 0;
                var a = o1;
                var b = o2;
                int result = CompareInteger(a.Key, b.Key);
                if (result != 0) return result;
                return CompareString(a.Value, b.Value);
            }

            private int CompareString(string o1, string o2)
            {
                if (o1 == o2) return 0;
                if (o1 == null) return -1;
                if (o2 == null) return 1;
                return o1.CompareToOrdinal(o2);
            }

            private int CompareInteger(Integer o1, Integer o2)
            {
                if (o1 == o2) return 0;
                if (o1 == null) return -1;
                if (o2 == null) return 1;
                return o1.CompareTo(o2);
            }
        }

        static IComparer<KeyValuePair<Integer, String>> ENTRY_COMPARATOR = new EntryComparer();


        private void getEntries(String title, ICollection<KeyValuePair<Integer, String>> m1entries, ICollection<KeyValuePair<Integer, String>> m2entries, StringBuilder buffer, int limit)
        {
            ISet<KeyValuePair<Integer, String>> m1_m2 = new SortedSet<KeyValuePair<Integer, String>>(ENTRY_COMPARATOR);
            m1_m2.UnionWith(m1entries);
            m1_m2.ExceptWith(m2entries);
            buffer.Append(title + ": " + m1_m2.Count + "\r\n");
            foreach (var entry in m1_m2)
            {
                if (limit-- < 0) return;
                buffer.Append(entry.Key).Append(" => ")
                 .Append(entry.Value).Append("\r\n");
            }
        }
    }
}
