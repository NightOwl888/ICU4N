using ICU4N.Support.IO;
using ICU4N.Support.Text;
using ICU4N.TestFramework.Dev.Test;
using ICU4N.Util;
using NUnit.Framework;
using System;
using System.Text;

namespace ICU4N.Tests.Dev.Test.Util
{
    public class BytesTrieTest : TestFmwk
    {
        public BytesTrieTest() { }

        // All test functions have a TestNN prefix where NN is a double-digit number.
        // This is so that when tests are run in sorted order
        // the simpler ones are run first.
        // If there is a problem, the simpler ones are easier to step through.

        [Test]
        public void Test00Builder()
        {
            builder_.Clear();
            try
            {
                builder_.Build(StringTrieBuilder.Option.FAST);
                Errln("BytesTrieBuilder().build() did not throw IndexOutOfBoundsException");
                return;
            }
            catch (IndexOutOfRangeException e)
            {
                // good
            }
            try
            {
                byte[] equal = new byte[] { 0x3d };  // "="
                builder_.Add(equal, 1, 0).Add(equal, 1, 1);
                Errln("BytesTrieBuilder.add() did not detect duplicates");
                return;
            }
            catch (ArgumentException e)
            {
                // good
            }
        }

        private sealed class StringAndValue
        {
            public StringAndValue(String str, int val)
            {
                s = str;
                bytes = new byte[s.Length];
                for (int i = 0; i < bytes.Length; ++i)
                {
                    bytes[i] = (byte)s[i];
                }
                value = val;
            }

            public String s;
            public byte[] bytes;
            public int value;
        }
        // Note: C++ StringAndValue initializers converted to Java syntax
        // with Eclipse Find/Replace regular expressions:
        // Find:            \{ (".*", [-0-9xa-fA-F]+) \}
        // Replace with:    new StringAndValue($1)

        [Test]
        public void Test10Empty()
        {
            StringAndValue[] data ={
                new StringAndValue("", 0)
             };
            checkData(data);
        }

        [Test]
        public void Test11_a()
        {
            StringAndValue[] data ={
                new StringAndValue("a", 1)
             };
            checkData(data);
        }

        [Test]
        public void Test12_a_ab()
        {
            StringAndValue[] data ={
                new StringAndValue("a", 1),
            new StringAndValue("ab", 100)
             };
            checkData(data);
        }

        [Test]
        public void Test20ShortestBranch()
        {
            StringAndValue[] data ={
                new StringAndValue("a", 1000),
                new StringAndValue("b", 2000)
             };
            checkData(data);
        }

        [Test]
        public void Test21Branches()
        {
            StringAndValue[] data ={
                new StringAndValue("a", 0x10),
                new StringAndValue("cc", 0x40),
                new StringAndValue("e", 0x100),
                new StringAndValue("ggg", 0x400),
                new StringAndValue("i", 0x1000),
                new StringAndValue("kkkk", 0x4000),
                new StringAndValue("n", 0x10000),
                new StringAndValue("ppppp", 0x40000),
                new StringAndValue("r", 0x100000),
                new StringAndValue("sss", 0x200000),
                new StringAndValue("t", 0x400000),
                new StringAndValue("uu", 0x800000),
                new StringAndValue("vv", 0x7fffffff),
                new StringAndValue("zz", unchecked((int)0x80000000))
             };
            for (int length = 2; length <= data.Length; ++length)
            {
                Logln("TestBranches length=" + length);
                checkData(data, length);
            }
        }

        [Test]
        public void Test22LongSequence()
        {
            StringAndValue[] data ={
                new StringAndValue("a", -1),
                // sequence of linear-match nodes
                new StringAndValue("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", -2),
                // more than 256 bytes
                new StringAndValue(
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", -3)
             };
            checkData(data);
        }

        [Test]
        public void Test23LongBranch()
        {
            // Split-branch and interesting compact-integer values.
            StringAndValue[] data ={
                new StringAndValue("a", -2),
                new StringAndValue("b", -1),
                new StringAndValue("c", 0),
                new StringAndValue("d2", 1),
                new StringAndValue("f", 0x3f),
                new StringAndValue("g", 0x40),
                new StringAndValue("h", 0x41),
                new StringAndValue("j23", 0x1900),
                new StringAndValue("j24", 0x19ff),
                new StringAndValue("j25", 0x1a00),
                new StringAndValue("k2", 0x1a80),
                new StringAndValue("k3", 0x1aff),
                new StringAndValue("l234567890", 0x1b00),
                new StringAndValue("l234567890123", 0x1b01),
                new StringAndValue("nnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnn", 0x10ffff),
                new StringAndValue("oooooooooooooooooooooooooooooooooooooooooooooooooooooo", 0x110000),
                new StringAndValue("pppppppppppppppppppppppppppppppppppppppppppppppppppppp", 0x120000),
                new StringAndValue("r", 0x333333),
                new StringAndValue("s2345", 0x4444444),
                new StringAndValue("t234567890", 0x77777777),
                new StringAndValue("z", unchecked((int)0x80000001))
             };
            checkData(data);
        }

        [Test]
        public void Test24ValuesForState()
        {
            // Check that saveState() and resetToState() interact properly
            // with next() and current().
            StringAndValue[] data ={
                new StringAndValue("a", -1),
            new StringAndValue("ab", -2),
            new StringAndValue("abc", -3),
            new StringAndValue("abcd", -4),
            new StringAndValue("abcde", -5),
            new StringAndValue("abcdef", -6)
             };
            checkData(data);
        }

        [Test]
        public void Test30Compact()
        {
            // Duplicate trailing strings and values provide opportunities for compacting.
            StringAndValue[] data ={
                new StringAndValue("+", 0),
            new StringAndValue("+august", 8),
            new StringAndValue("+december", 12),
            new StringAndValue("+july", 7),
            new StringAndValue("+june", 6),
            new StringAndValue("+november", 11),
            new StringAndValue("+october", 10),
            new StringAndValue("+september", 9),
            new StringAndValue("-", 0),
            new StringAndValue("-august", 8),
            new StringAndValue("-december", 12),
            new StringAndValue("-july", 7),
            new StringAndValue("-june", 6),
            new StringAndValue("-november", 11),
            new StringAndValue("-october", 10),
            new StringAndValue("-september", 9),
            // The l+n branch (with its sub-nodes) is a duplicate but will be written
            // both times because each time it follows a different linear-match node.
            new StringAndValue("xjuly", 7),
            new StringAndValue("xjune", 6)
             };
            checkData(data);
        }

        public BytesTrie buildMonthsTrie(StringTrieBuilder.Option buildOption)
        {
            // All types of nodes leading to the same value,
            // for code coverage of recursive functions.
            // In particular, we need a lot of branches on some single level
            // to exercise a split-branch node.
            StringAndValue[] data ={
                new StringAndValue("august", 8),
            new StringAndValue("jan", 1),
            new StringAndValue("jan.", 1),
            new StringAndValue("jana", 1),
            new StringAndValue("janbb", 1),
            new StringAndValue("janc", 1),
            new StringAndValue("janddd", 1),
            new StringAndValue("janee", 1),
            new StringAndValue("janef", 1),
            new StringAndValue("janf", 1),
            new StringAndValue("jangg", 1),
            new StringAndValue("janh", 1),
            new StringAndValue("janiiii", 1),
            new StringAndValue("janj", 1),
            new StringAndValue("jankk", 1),
            new StringAndValue("jankl", 1),
            new StringAndValue("jankmm", 1),
            new StringAndValue("janl", 1),
            new StringAndValue("janm", 1),
            new StringAndValue("jannnnnnnnnnnnnnnnnnnnnnnnnnnnn", 1),
            new StringAndValue("jano", 1),
            new StringAndValue("janpp", 1),
            new StringAndValue("janqqq", 1),
            new StringAndValue("janr", 1),
            new StringAndValue("januar", 1),
            new StringAndValue("january", 1),
            new StringAndValue("july", 7),
            new StringAndValue("jun", 6),
            new StringAndValue("jun.", 6),
            new StringAndValue("june", 6)
             };
            return buildTrie(data, data.Length, buildOption);
        }

        [Test]
        public void Test40GetUniqueValue()
        {
            BytesTrie trie = buildMonthsTrie(StringTrieBuilder.Option.FAST);
            long uniqueValue;
            if ((uniqueValue = trie.GetUniqueValue()) != 0)
            {
                Errln("unique value at root");
            }
            trie.Next('j');
            trie.Next('a');
            trie.Next('n');
            // getUniqueValue() directly after next()
            if ((uniqueValue = trie.GetUniqueValue()) != ((1 << 1) | 1))
            {
                Errln("not unique value 1 after \"jan\": instead " + uniqueValue);
            }
            trie.First('j');
            trie.Next('u');
            if ((uniqueValue = trie.GetUniqueValue()) != 0)
            {
                Errln("unique value after \"ju\"");
            }
            if (trie.Next('n') != BytesTrieResult.INTERMEDIATE_VALUE || 6 != trie.GetValue())
            {
                Errln("not normal value 6 after \"jun\"");
            }
            // getUniqueValue() after getValue()
            if ((uniqueValue = trie.GetUniqueValue()) != ((6 << 1) | 1))
            {
                Errln("not unique value 6 after \"jun\"");
            }
            // getUniqueValue() from within a linear-match node
            trie.First('a');
            trie.Next('u');
            if ((uniqueValue = trie.GetUniqueValue()) != ((8 << 1) | 1))
            {
                Errln("not unique value 8 after \"au\"");
            }
        }

        [Test]
        public void Test41GetNextBytes()
        {
            BytesTrie trie = buildMonthsTrie(StringTrieBuilder.Option.SMALL);
            StringBuilder buffer = new StringBuilder();
            int count = trie.GetNextBytes(buffer);
            if (count != 2 || !"aj".ContentEquals(buffer))
            {
                Errln("months getNextBytes()!=[aj] at root");
            }
            trie.Next('j');
            trie.Next('a');
            trie.Next('n');
            // getNextBytes() directly after next()
            buffer.Length = (0);
            count = trie.GetNextBytes(buffer);
            if (count != 20 || !".abcdefghijklmnopqru".ContentEquals(buffer))
            {
                Errln("months getNextBytes()!=[.abcdefghijklmnopqru] after \"jan\"");
            }
            // getNextBytes() after getValue()
            trie.GetValue();  // next() had returned BytesTrieResult.INTERMEDIATE_VALUE.
            buffer.Length = (0);
            count = trie.GetNextBytes(buffer);
            if (count != 20 || !".abcdefghijklmnopqru".ContentEquals(buffer))
            {
                Errln("months getNextBytes()!=[.abcdefghijklmnopqru] after \"jan\"+getValue()");
            }
            // getNextBytes() from a linear-match node
            trie.Next('u');
            buffer.Length = (0);
            count = trie.GetNextBytes(buffer);
            if (count != 1 || !"a".ContentEquals(buffer))
            {
                Errln("months getNextBytes()!=[a] after \"janu\"");
            }
            trie.Next('a');
            buffer.Length = (0);
            count = trie.GetNextBytes(buffer);
            if (count != 1 || !"r".ContentEquals(buffer))
            {
                Errln("months getNextBytes()!=[r] after \"janua\"");
            }
            trie.Next('r');
            trie.Next('y');
            // getNextBytes() after a final match
            buffer.Length = (0);
            count = trie.GetNextBytes(buffer);
            if (count != 0 || buffer.Length != 0)
            {
                Errln("months getNextBytes()!=[] after \"january\"");
            }
        }

        [Test]
        public void Test50IteratorFromBranch()
        {
            BytesTrie trie = buildMonthsTrie(StringTrieBuilder.Option.FAST);
            // Go to a branch node.
            trie.Next('j');
            trie.Next('a');
            trie.Next('n');
            BytesTrie.Enumerator iter = (BytesTrie.Enumerator)trie.GetEnumerator();
            // Expected data: Same as in buildMonthsTrie(), except only the suffixes
            // following "jan".
            StringAndValue[] data ={
                new StringAndValue("", 1),
            new StringAndValue(".", 1),
            new StringAndValue("a", 1),
            new StringAndValue("bb", 1),
            new StringAndValue("c", 1),
            new StringAndValue("ddd", 1),
            new StringAndValue("ee", 1),
            new StringAndValue("ef", 1),
            new StringAndValue("f", 1),
            new StringAndValue("gg", 1),
            new StringAndValue("h", 1),
            new StringAndValue("iiii", 1),
            new StringAndValue("j", 1),
            new StringAndValue("kk", 1),
            new StringAndValue("kl", 1),
            new StringAndValue("kmm", 1),
            new StringAndValue("l", 1),
            new StringAndValue("m", 1),
            new StringAndValue("nnnnnnnnnnnnnnnnnnnnnnnnnnnn", 1),
            new StringAndValue("o", 1),
            new StringAndValue("pp", 1),
            new StringAndValue("qqq", 1),
            new StringAndValue("r", 1),
            new StringAndValue("uar", 1),
            new StringAndValue("uary", 1)
             };
            checkIterator(iter, data);
            // Reset, and we should get the same result.
            Logln("after iter.Reset()");
            checkIterator(iter.Reset(), data);
        }

        [Test]
        public void Test51IteratorFromLinearMatch()
        {
            BytesTrie trie = buildMonthsTrie(StringTrieBuilder.Option.SMALL);
            // Go into a linear-match node.
            trie.Next('j');
            trie.Next('a');
            trie.Next('n');
            trie.Next('u');
            trie.Next('a');
            BytesTrie.Enumerator iter = trie.GetEnumerator();
            // Expected data: Same as in buildMonthsTrie(), except only the suffixes
            // following "janua".
            StringAndValue[] data ={
                new StringAndValue("r", 1),
            new StringAndValue("ry", 1)
             };
            checkIterator(iter, data);
            // Reset, and we should get the same result.
            Logln("after iter.Reset()");
            checkIterator(iter.Reset(), data);
        }

        [Test]
        public void Test52TruncatingIteratorFromRoot()
        {
            BytesTrie trie = buildMonthsTrie(StringTrieBuilder.Option.FAST);
            BytesTrie.Enumerator iter = trie.GetEnumerator(4);
            // Expected data: Same as in buildMonthsTrie(), except only the first 4 characters
            // of each string, and no string duplicates from the truncation.
            StringAndValue[] data ={
                new StringAndValue("augu", -1),
            new StringAndValue("jan", 1),
            new StringAndValue("jan.", 1),
            new StringAndValue("jana", 1),
            new StringAndValue("janb", -1),
            new StringAndValue("janc", 1),
            new StringAndValue("jand", -1),
            new StringAndValue("jane", -1),
            new StringAndValue("janf", 1),
            new StringAndValue("jang", -1),
            new StringAndValue("janh", 1),
            new StringAndValue("jani", -1),
            new StringAndValue("janj", 1),
            new StringAndValue("jank", -1),
            new StringAndValue("janl", 1),
            new StringAndValue("janm", 1),
            new StringAndValue("jann", -1),
            new StringAndValue("jano", 1),
            new StringAndValue("janp", -1),
            new StringAndValue("janq", -1),
            new StringAndValue("janr", 1),
            new StringAndValue("janu", -1),
            new StringAndValue("july", 7),
            new StringAndValue("jun", 6),
            new StringAndValue("jun.", 6),
            new StringAndValue("june", 6)
             };
            checkIterator(iter, data);
            // Reset, and we should get the same result.
            Logln("after iter.Reset()");
            checkIterator(iter.Reset(), data);
        }

        [Test]
        public void Test53TruncatingIteratorFromLinearMatchShort()
        {
            StringAndValue[] data ={
                new StringAndValue("abcdef", 10),
            new StringAndValue("abcdepq", 200),
            new StringAndValue("abcdeyz", 3000)
             };
            BytesTrie trie = buildTrie(data, data.Length, StringTrieBuilder.Option.FAST);
            // Go into a linear-match node.
            trie.Next('a');
            trie.Next('b');
            // Truncate within the linear-match node.
            BytesTrie.Enumerator iter = trie.GetEnumerator(2);
            StringAndValue[] expected ={
                new StringAndValue("cd", -1)
             };
            checkIterator(iter, expected);
            // Reset, and we should get the same result.
            Logln("after iter.Reset()");
            checkIterator(iter.Reset(), expected);
        }

        [Test]
        public void Test54TruncatingIteratorFromLinearMatchLong()
        {
            StringAndValue[] data ={
                new StringAndValue("abcdef", 10),
            new StringAndValue("abcdepq", 200),
            new StringAndValue("abcdeyz", 3000)
             };
            BytesTrie trie = buildTrie(data, data.Length, StringTrieBuilder.Option.FAST);
            // Go into a linear-match node.
            trie.Next('a');
            trie.Next('b');
            trie.Next('c');
            // Truncate after the linear-match node.
            BytesTrie.Enumerator iter = trie.GetEnumerator(3);
            StringAndValue[] expected ={
                new StringAndValue("def", 10),
            new StringAndValue("dep", -1),
            new StringAndValue("dey", -1)
             };
            checkIterator(iter, expected);
            // Reset, and we should get the same result.
            Logln("after iter.Reset()");
            checkIterator(iter.Reset(), expected);
        }

        [Test]
        public void Test59IteratorFromBytes()
        {
            StringAndValue[] data ={
                new StringAndValue("mm", 3),
            new StringAndValue("mmm", 33),
            new StringAndValue("mmnop", 333)
             };
            builder_.Clear();
            foreach (StringAndValue item in data)
            {
                builder_.Add(item.bytes, item.bytes.Length, item.value);
            }
            ByteBuffer trieBytes = builder_.BuildByteBuffer(StringTrieBuilder.Option.FAST);
            checkIterator(
                BytesTrie.GetEnumerator(trieBytes.Array, trieBytes.ArrayOffset + trieBytes.Position, 0),
                data);
        }

        private void checkData(StringAndValue[] data)
        {
            checkData(data, data.Length);
        }

        private void checkData(StringAndValue[] data, int dataLength)
        {
            Logln("checkData(dataLength=" + dataLength + ", fast)");
            checkData(data, dataLength, StringTrieBuilder.Option.FAST);
            Logln("checkData(dataLength=" + dataLength + ", small)");
            checkData(data, dataLength, StringTrieBuilder.Option.SMALL);
        }

        private void checkData(StringAndValue[] data, int dataLength, StringTrieBuilder.Option buildOption)
        {
            BytesTrie trie = buildTrie(data, dataLength, buildOption);
            checkFirst(trie, data, dataLength);
            checkNext(trie, data, dataLength);
            checkNextWithState(trie, data, dataLength);
            checkNextString(trie, data, dataLength);
            checkIterator(trie, data, dataLength);
        }

        private BytesTrie buildTrie(StringAndValue[] data, int dataLength,
                                    StringTrieBuilder.Option buildOption)
        {
            // Add the items to the trie builder in an interesting (not trivial, not random) order.
            int index, step;
            if ((dataLength & 1) != 0)
            {
                // Odd number of items.
                index = dataLength / 2;
                step = 2;
            }
            else if ((dataLength % 3) != 0)
            {
                // Not a multiple of 3.
                index = dataLength / 5;
                step = 3;
            }
            else
            {
                index = dataLength - 1;
                step = -1;
            }
            builder_.Clear();
            for (int i = 0; i < dataLength; ++i)
            {
                builder_.Add(data[index].bytes, data[index].bytes.Length, data[index].value);
                index = (index + step) % dataLength;
            }
            BytesTrie trie = builder_.Build(buildOption);
            try
            {
                builder_.Add(/* "zzz" */ new byte[] { 0x7a, 0x7a, 0x7a }, 0, 999);
                Errln("builder.build().add(zzz) did not throw InvalidOperationException");
            }
            catch (InvalidOperationException e)
            {
                // good
            }
            ByteBuffer trieBytes = builder_.BuildByteBuffer(buildOption);
            Logln("serialized trie size: " + trieBytes.Remaining + " bytes\n");
            // Tries from either build() method should be identical but
            // BytesTrie does not implement equals().
            // We just return either one.
            if ((dataLength & 1) != 0)
            {
                return trie;
            }
            else
            {
                return new BytesTrie(trieBytes.Array, trieBytes.ArrayOffset + trieBytes.Position);
            }
        }

        private void checkFirst(BytesTrie trie, StringAndValue[] data, int dataLength)
        {
            for (int i = 0; i < dataLength; ++i)
            {
                if (data[i].s.Length == 0)
                {
                    continue;  // skip empty string
                }
                int c = data[i].bytes[0];
                BytesTrieResult firstResult = trie.First(c);
                int firstValue = firstResult.HasValue() ? trie.GetValue() : -1;
                int nextC = data[i].s.Length > 1 ? data[i].bytes[1] : 0;
                BytesTrieResult nextResult = trie.Next(nextC);
                if (firstResult != trie.Reset().Next(c) ||
                   firstResult != trie.Current ||
                   firstValue != (firstResult.HasValue() ? trie.GetValue() : -1) ||
                   nextResult != trie.Next(nextC)
                )
                {
                    Errln(string.Format("trie.First({0})!=trie.Reset().Next(same) for {1}",
                                        (char)c, data[i].s));
                }
            }
            trie.Reset();
        }

        private void checkNext(BytesTrie trie, StringAndValue[] data, int dataLength)
        {
            BytesTrie.State state = new BytesTrie.State();
            for (int i = 0; i < dataLength; ++i)
            {
                int stringLength = data[i].s.Length;
                BytesTrieResult result;
                if (!(result = trie.Next(data[i].bytes, 0, stringLength)).HasValue() ||
                    result != trie.Current
                )
                {
                    Errln("trie does not seem to contain " + data[i].s);
                }
                else if (trie.GetValue() != data[i].value)
                {
                    Errln(string.Format("trie value for {0} is {1:d}=0x{2:x} instead of expected {3:d}=0x{4:x}",
                                        data[i].s,
                                        trie.GetValue(), trie.GetValue(),
                                        data[i].value, data[i].value));
                }
                else if (result != trie.Current || trie.GetValue() != data[i].value)
                {
                    Errln("trie value for " + data[i].s + " changes when repeating current()/getValue()");
                }
                trie.Reset();
                result = trie.Current;
                for (int j = 0; j < stringLength; ++j)
                {
                    if (!result.HasNext())
                    {
                        Errln(String.Format("trie.Current!=hasNext before end of {0} (at index {1:d})",
                                            data[i].s, j));
                        break;
                    }
                    if (result == BytesTrieResult.INTERMEDIATE_VALUE)
                    {
                        trie.GetValue();
                        if (trie.Current != BytesTrieResult.INTERMEDIATE_VALUE)
                        {
                            Errln(String.Format("trie.GetValue().Current!=BytesTrieResult.INTERMEDIATE_VALUE " +
                                                "before end of {0} (at index {1:d})", data[i].s, j));
                            break;
                        }
                    }
                    result = trie.Next(data[i].bytes[j]);
                    if (!result.Matches())
                    {
                        Errln(String.Format("trie.Next()=BytesTrieResult.NO_MATCH " +
                                            "before end of {0} (at index {1:d})", data[i].s, j));
                        break;
                    }
                    if (result != trie.Current)
                    {
                        Errln(String.Format("trie.Next()!=following current() " +
                                            "before end of {0} (at index {1:d})", data[i].s, j));
                        break;
                    }
                }
                if (!result.HasValue())
                {
                    Errln("trie.Next()!=hasValue at the end of " + data[i].s);
                    continue;
                }
                trie.GetValue();
                if (result != trie.Current)
                {
                    Errln("trie.Current != current()+getValue()+current() after end of " +
                          data[i].s);
                }
                // Compare the final current() with whether next() can actually continue.
                trie.SaveState(state);
                bool nextContinues = false;
                for (int c = 0x20; c < 0x7f; ++c)
                {
                    if (trie.ResetToState(state).Next(c).Matches())
                    {
                        nextContinues = true;
                        break;
                    }
                }
                if ((result == BytesTrieResult.INTERMEDIATE_VALUE) != nextContinues)
                {
                    Errln("(trie.Current==BytesTrieResult.INTERMEDIATE_VALUE) contradicts " +
                          "(trie.Next(some UChar)!=BytesTrieResult.NO_MATCH) after end of " + data[i].s);
                }
                trie.Reset();
            }
        }

        private void checkNextWithState(BytesTrie trie, StringAndValue[] data, int dataLength)
        {
            BytesTrie.State noState = new BytesTrie.State(), state = new BytesTrie.State();
            for (int i = 0; i < dataLength; ++i)
            {
                if ((i & 1) == 0)
                {
                    try
                    {
                        trie.ResetToState(noState);
                        Errln("trie.ResetToState(noState) should throw an ArgumentException");
                    }
                    catch (ArgumentException e)
                    {
                        // good
                    }
                }
                byte[] expectedString = data[i].bytes;
                int stringLength = data[i].s.Length;
                int partialLength = stringLength / 3;
                for (int j = 0; j < partialLength; ++j)
                {
                    if (!trie.Next(expectedString[j]).Matches())
                    {
                        Errln("trie.Next()=BytesTrieResult.NO_MATCH for a prefix of " + data[i].s);
                        return;
                    }
                }
                trie.SaveState(state);
                BytesTrieResult resultAtState = trie.Current;
                BytesTrieResult result;
                int valueAtState = -99;
                if (resultAtState.HasValue())
                {
                    valueAtState = trie.GetValue();
                }
                result = trie.Next(0);  // mismatch
                if (result != BytesTrieResult.NO_MATCH || result != trie.Current)
                {
                    Errln("trie.Next(0) matched after part of " + data[i].s);
                }
                if (resultAtState != trie.ResetToState(state).Current ||
                    (resultAtState.HasValue() && valueAtState != trie.GetValue())
                )
                {
                    Errln("trie.Next(part of " + data[i].s + ") changes current()/getValue() after " +
                          "saveState/next(0)/resetToState");
                }
                else if (!(result = trie.Next(expectedString, partialLength, stringLength)).HasValue() ||
                        result != trie.Current)
                {
                    Errln("trie.Next(rest of " + data[i].s + ") does not seem to contain " + data[i].s + " after " +
                          "saveState/next(0)/resetToState");
                }
                else if (!(result = trie.ResetToState(state).
                                  Next(expectedString, partialLength, stringLength)).HasValue() ||
                        result != trie.Current)
                {
                    Errln("trie does not seem to contain " + data[i].s +
                          " after saveState/next(rest)/resetToState");
                }
                else if (trie.GetValue() != data[i].value)
                {
                    Errln(String.Format("trie value for {0} is {1:d}=0x{2:x} instead of expected {3:d}=0x{4:x}",
                                        data[i].s,
                                        trie.GetValue(), trie.GetValue(),
                                        data[i].value, data[i].value));
                }
                trie.Reset();
            }
        }

        // next(string) is also tested in other functions,
        // but here we try to go partway through the string, and then beyond it.
        private void checkNextString(BytesTrie trie, StringAndValue[] data, int dataLength)
        {
            for (int i = 0; i < dataLength; ++i)
            {
                byte[] expectedString = data[i].bytes;
                int stringLength = data[i].s.Length;
                if (!trie.Next(expectedString, 0, stringLength / 2).Matches())
                {
                    Errln("trie.Next(up to middle of string)=BytesTrieResult.NO_MATCH for " + data[i].s);
                    continue;
                }
                // Test that we stop properly at the end of the string.
                trie.Next(expectedString, stringLength / 2, stringLength);
                if (trie.Next(0).Matches())
                {
                    Errln("trie.Next(string+NUL)!=BytesTrieResult.NO_MATCH for " + data[i].s);
                }
                trie.Reset();
            }
        }

        private void checkIterator(BytesTrie trie, StringAndValue[] data, int dataLength)
        {
            checkIterator(trie.GetEnumerator(), data, dataLength);
        }

        private void checkIterator(BytesTrie.Enumerator iter, StringAndValue[] data)
        {
            checkIterator(iter, data, data.Length);
        }

        private void checkIterator(BytesTrie.Enumerator iter, StringAndValue[] data, int dataLength)
        {
            for (int i = 0; i < dataLength; ++i)
            {
                if (!iter.MoveNext())
                {
                    Errln("trie iterator MoveNext()=false for item " + i + ": " + data[i].s);
                    break;
                }
                BytesTrie.Entry entry = iter.Current;
                StringBuilder bytesString = new StringBuilder();
                for (int j = 0; j < entry.BytesLength; ++j)
                {
                    bytesString.Append((char)(entry.ByteAt(j) & 0xff));
                }
                if (!data[i].s.ContentEquals(bytesString))
                {
                    Errln(String.Format("trie iterator next().getString()={0} but expected {1} for item {2:d}",
                                        bytesString, data[i].s, i));
                }
                if (entry.value != data[i].value)
                {
                    Errln(String.Format("trie iterator next().GetValue()={0:d}=0x{1:x} but expected {2:d}=0x{3:x} for item {4:d}: {5}",
                                        entry.value, entry.value,
                                        data[i].value, data[i].value,
                                        i, data[i].s));
                }
            }
            if (iter.MoveNext())
            {
                Errln("trie iterator MoveNext()=true after all items");
            }
            // ICU4N specific - this test doesn't apply in .NET
            //try
            //{
            //    iter.Next();
            //    Errln("trie iterator next() did not throw NoSuchElementException after all items");
            //}
            //catch (NoSuchElementException e)
            //{
            //    // good
            //}
        }

        private BytesTrieBuilder builder_ = new BytesTrieBuilder();
    }
}
