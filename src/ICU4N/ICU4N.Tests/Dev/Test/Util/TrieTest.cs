using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Text;
using ICU4N.Util;
using NUnit.Framework;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Util
{
    /// <summary>
    /// Testing class for Trie. Tests here will be simple, since both CharTrie and
    /// IntTrie are very similar and are heavily used in other parts of ICU4N.
    /// Codes using Tries are expected to have detailed tests.
    /// </summary>
    /// <author>Syn Wee Quek</author>
    /// <since>release 2.1 Jan 01 2002</since>
    public sealed class TrieTest : TestFmwk
    {
        // constructor ---------------------------------------------------

        /**
        * Constructor
        */
        public TrieTest()
        {
        }

        // public methods -----------------------------------------------

        /**
         * Values for setting possibly overlapping, out-of-order ranges of values
         */
        private sealed class SetRange
        {
            internal SetRange(int start, int limit, int value, bool overwrite)
            {
                this.start = start;
                this.limit = limit;
                this.value = value;
                this.overwrite = overwrite;
            }

            internal int start, limit;
            internal int value;
            internal bool overwrite;
        }

        /**
         * Values for testing:
         * value is set from the previous boundary's limit to before
         * this boundary's limit
         */
        private sealed class CheckRange
        {
            internal CheckRange(int limit, int value)
            {
                this.Limit = limit;
                this.Value = value;
            }

            internal int Limit { get; set; }
            internal int Value { get; set; }
        }

        private sealed class _testFoldedValue
                                        : TrieBuilder.IDataManipulate
        {
            public _testFoldedValue(Int32TrieBuilder builder)
            {
                m_builder_ = builder;
            }

            public int GetFoldedValue(int start, int offset)
            {
                int foldedValue = 0;
                int limit = start + 0x400;
                while (start < limit)
                {
                    int value = m_builder_.GetValue(start);
                    if (m_builder_.IsInZeroBlock(start))
                    {
                        start += TrieBuilder.DATA_BLOCK_LENGTH;
                    }
                    else
                    {
                        foldedValue |= value;
                        ++start;
                    }
                }

                if (foldedValue != 0)
                {
                    return (offset << 16) | foldedValue;
                }
                return 0;
            }

            private Int32TrieBuilder m_builder_;
        }

        private sealed class _testFoldingOffset
                                                    : Trie.IDataManipulate
        {
            public int GetFoldingOffset(int value)
            {
                return value.TripleShift(16);
            }
        }

        private sealed class _testEnumValue : TrieIterator
        {
            public _testEnumValue(Trie data)
                    : base(data)
            {
            }

            protected override int Extract(int value)
            {
                return value ^ 0x5555;
            }
        }

        private void _testTrieIteration(Int32Trie trie, CheckRange[] checkRanges,
                                        int countCheckRanges)
        {
            // write a string
            int countValues = 0;
            StringBuffer s = new StringBuffer();
            int[] values = new int[30];
            for (int i = 0; i < countCheckRanges; ++i)
            {
                int c = checkRanges[i].Limit;
                if (c != 0)
                {
                    --c;
                    UTF16.Append(s, c);
                    values[countValues++] = checkRanges[i].Value;
                }
            }

            {
                int limit = s.Length;
                // try forward
                int p = 0;
                int i = 0;
                while (p < limit)
                {
                    int c = UTF16.CharAt(s, p);
                    p += UTF16.GetCharCount(c);
                    int value = trie.GetCodePointValue(c);
                    if (value != values[i])
                    {
                        Errln("wrong value from UTRIE_NEXT(U+"
                              + (c).ToHexString() + "): 0x"
                              + (value).ToHexString() + " instead of 0x"
                              + (values[i]).ToHexString());
                    }
                    // unlike the c version lead is 0 if c is non-supplementary
                    char lead = UTF16.GetLeadSurrogate(c);
                    char trail = UTF16.GetTrailSurrogate(c);
                    if (lead == 0
                        ? trail != s[p - 1]
                        : !UTF16.IsLeadSurrogate(lead)
                          || !UTF16.IsTrailSurrogate(trail) || lead != s[p - 2]
                          || trail != s[p - 1])
                    {
                        Errln("wrong (lead, trail) from UTRIE_NEXT(U+"
                              + (c).ToHexString());
                        continue;
                    }
                    if (lead != 0)
                    {
                        value = trie.GetLeadValue(lead);
                        value = trie.GetTrailValue(value, trail);
                        if (value != trie.GetSurrogateValue(lead, trail)
                            && value != values[i])
                        {
                            Errln("wrong value from getting supplementary "
                                  + "values (U+"
                                  + (c).ToHexString() + "): 0x"
                                  + (value).ToHexString() + " instead of 0x"
                                  + (values[i]).ToHexString());
                        }
                    }
                    ++i;
                }
            }
        }

        private void _testTrieRanges(SetRange[] setRanges, int countSetRanges,
                                     CheckRange[] checkRanges, int countCheckRanges,
                                     bool latin1Linear)
        {
            Int32TrieBuilder newTrie = new Int32TrieBuilder(null, 2000,
                                                        checkRanges[0].Value,
                                                        checkRanges[0].Value,
                                                        latin1Linear);

            // set values from setRanges[]
            bool ok = true;
            for (int i = 0; i < countSetRanges; ++i)
            {
                int start = setRanges[i].start;
                int limit = setRanges[i].limit;
                int value = setRanges[i].value;
                bool overwrite = setRanges[i].overwrite;
                if ((limit - start) == 1 && overwrite)
                {
                    ok &= newTrie.SetValue(start, value);
                }
                else
                {
                    ok &= newTrie.SetRange(start, limit, value, overwrite);
                }
            }
            if (!ok)
            {
                Errln("setting values into a trie failed");
                return;
            }

            {
                // verify that all these values are in the new Trie
                int start = 0;
                for (int i = 0; i < countCheckRanges; ++i)
                {
                    int limit = checkRanges[i].Limit;
                    int value = checkRanges[i].Value;

                    while (start < limit)
                    {
                        if (value != newTrie.GetValue(start))
                        {
                            Errln("newTrie [U+"
                                  + (start).ToHexString() + "]==0x"
                                  + (newTrie.GetValue(start).ToHexString())
                                  + " instead of 0x" + (value).ToHexString());
                        }
                        ++start;
                    }
                }

                Int32Trie trie = newTrie.Serialize(new _testFoldedValue(newTrie),
                                                 new _testFoldingOffset());

                // test linear Latin-1 range from utrie_getData()
                if (latin1Linear)
                {
                    start = 0;
                    for (int i = 0; i < countCheckRanges && start <= 0xff; ++i)
                    {
                        int limit = checkRanges[i].Limit;
                        int value = checkRanges[i].Value;

                        while (start < limit && start <= 0xff)
                        {
                            if (value != trie.GetLatin1LinearValue((char)start))
                            {
                                Errln("IntTrie.getLatin1LinearValue[U+"
                                      + (start).ToHexString() + "]==0x"
                                      + (
                                                trie.GetLatin1LinearValue((char)start).ToHexString())
                                      + " instead of 0x" + (value).ToHexString());
                            }
                            ++start;
                        }
                    }
                }

                if (latin1Linear != trie.IsLatin1Linear)
                {
                    Errln("trie serialization did not preserve "
                          + "Latin-1-linearity");
                }

                // verify that all these values are in the serialized Trie
                start = 0;
                for (int i = 0; i < countCheckRanges; ++i)
                {
                    int limit = checkRanges[i].Limit;
                    int value = checkRanges[i].Value;

                    if (start == 0xd800)
                    {
                        // skip surrogates
                        start = limit;
                        continue;
                    }

                    while (start < limit)
                    {
                        if (start <= 0xffff)
                        {
                            int value2 = trie.GetBMPValue((char)start);
                            if (value != value2)
                            {
                                Errln("serialized trie.getBMPValue(U+"
                                      + (start).ToHexString() + " == 0x"
                                      + (value2).ToHexString() + " instead of 0x"
                                      + (value).ToHexString());
                            }
                            if (!UTF16.IsLeadSurrogate((char)start))
                            {
                                value2 = trie.GetLeadValue((char)start);
                                if (value != value2)
                                {
                                    Errln("serialized trie.getLeadValue(U+"
                                      + (start).ToHexString() + " == 0x"
                                      + (value2).ToHexString() + " instead of 0x"
                                      + (value).ToHexString());
                                }
                            }
                        }

                        {
                            int value2 = trie.GetCodePointValue(start);
                            if (value != value2)
                            {
                                Errln("serialized trie.getCodePointValue(U+"
                                      + (start).ToHexString() + ")==0x"
                                      + (value2).ToHexString() + " instead of 0x"
                                      + (value).ToHexString());
                            }
                            ++start;
                        }
                    }
                }


                // enumerate and verify all ranges

                int enumRanges = 1;
                TrieIterator iter = new _testEnumValue(trie);
                while (iter.MoveNext())
                {
                    RangeValueEnumeratorElement result = iter.Current;
                    if (result.Start != checkRanges[enumRanges - 1].Limit
                        || result.Limit != checkRanges[enumRanges].Limit
                        || (result.Value ^ 0x5555) != checkRanges[enumRanges].Value)
                    {
                        Errln("utrie_enum() delivers wrong range [U+"
                              + (result.Start).ToHexString() + "..U+"
                              + (result.Limit).ToHexString() + "].0x"
                              + (result.Value ^ 0x5555).ToHexString()
                              + " instead of [U+"
                              + (checkRanges[enumRanges - 1].Limit).ToHexString()
                              + "..U+"
                              + (checkRanges[enumRanges].Limit).ToHexString()
                              + "].0x"
                              + (checkRanges[enumRanges].Value).ToHexString());
                    }
                    enumRanges++;
                }

                // test linear Latin-1 range
                if (trie.IsLatin1Linear)
                {
                    for (start = 0; start < 0x100; ++start)
                    {
                        if (trie.GetLatin1LinearValue((char)start)
                            != trie.GetLeadValue((char)start))
                        {
                            Errln("trie.getLatin1LinearValue[U+"
                                  + (start).ToHexString() + "]=0x"
                                  + (
                                                trie.GetLatin1LinearValue((char)start).ToHexString())
                                  + " instead of 0x"
                                  + (
                                                trie.GetLeadValue((char)start)).ToHexString());
                        }
                    }
                }

                _testTrieIteration(trie, checkRanges, countCheckRanges);
            }
        }

        private void _testTrieRanges2(SetRange[] setRanges,
                                      int countSetRanges,
                                      CheckRange[] checkRanges,
                                      int countCheckRanges)
        {
            _testTrieRanges(setRanges, countSetRanges, checkRanges, countCheckRanges,
                            false);

            _testTrieRanges(setRanges, countSetRanges, checkRanges, countCheckRanges,
                            true);
        }

        private void _testTrieRanges4(SetRange[] setRanges, int countSetRanges,
                                      CheckRange[] checkRanges,
                                      int countCheckRanges)
        {
            _testTrieRanges2(setRanges, countSetRanges, checkRanges,
                             countCheckRanges);
        }

        // test data ------------------------------------------------------------

        /**
         * set consecutive ranges, even with value 0
         */
        private static SetRange[] setRanges1 ={
            new SetRange(0,      0x20,       0,      false),
            new SetRange(0x20,   0xa7,       0x1234, false),
            new SetRange(0xa7,   0x3400,     0,      false),
            new SetRange(0x3400, 0x9fa6,     0x6162, false),
            new SetRange(0x9fa6, 0xda9e,     0x3132, false),
            // try to disrupt _testFoldingOffset16()
            new SetRange(0xdada, 0xeeee,     0x87ff, false),
            new SetRange(0xeeee, 0x11111,    1,      false),
            new SetRange(0x11111, 0x44444,   0x6162, false),
            new SetRange(0x44444, 0x60003,   0,      false),
            new SetRange(0xf0003, 0xf0004,   0xf,    false),
            new SetRange(0xf0004, 0xf0006,   0x10,   false),
            new SetRange(0xf0006, 0xf0007,   0x11,   false),
            new SetRange(0xf0007, 0xf0020,   0x12,   false),
            new SetRange(0xf0020, 0x110000,  0,      false)
        };

        private static CheckRange[] checkRanges1 ={
            new CheckRange(0,      0), // dummy start range to make _testEnumRange() simpler
            new CheckRange(0x20,   0),
            new CheckRange(0xa7,   0x1234),
            new CheckRange(0x3400, 0),
            new CheckRange(0x9fa6, 0x6162),
            new CheckRange(0xda9e, 0x3132),
            new CheckRange(0xdada, 0),
            new CheckRange(0xeeee, 0x87ff),
            new CheckRange(0x11111,1),
            new CheckRange(0x44444,0x6162),
            new CheckRange(0xf0003,0),
            new CheckRange(0xf0004,0xf),
            new CheckRange(0xf0006,0x10),
            new CheckRange(0xf0007,0x11),
            new CheckRange(0xf0020,0x12),
            new CheckRange(0x110000, 0)
        };

        /**
         * set some interesting overlapping ranges
         */
        private static SetRange[] setRanges2 ={
            new SetRange(0x21,   0x7f,       0x5555, true),
            new SetRange(0x2f800,0x2fedc,    0x7a,   true),
            new SetRange(0x72,   0xdd,       3,      true),
            new SetRange(0xdd,   0xde,       4,      false),
            new SetRange(0x2f987,0x2fa98,    5,      true),
            new SetRange(0x2f777,0x2f833,    0,      true),
            new SetRange(0x2f900,0x2ffee,    1,      false),
            new SetRange(0x2ffee,0x2ffef,    2,      true)
        };

        private static CheckRange[] checkRanges2 ={
            // dummy start range to make _testEnumRange() simpler
            new CheckRange(0,      0),
            new CheckRange(0x21,   0),
            new CheckRange(0x72,   0x5555),
            new CheckRange(0xdd,   3),
            new CheckRange(0xde,   4),
            new CheckRange(0x2f833,0),
            new CheckRange(0x2f987,0x7a),
            new CheckRange(0x2fa98,5),
            new CheckRange(0x2fedc,0x7a),
            new CheckRange(0x2ffee,1),
            new CheckRange(0x2ffef,2),
            new CheckRange(0x110000, 0)
        };

        /**
         * use a non-zero initial value
         */
        private static SetRange[] setRanges3 ={
            new SetRange(0x31,   0xa4,   1,  false),
            new SetRange(0x3400, 0x6789, 2,  false),
            new SetRange(0x30000,0x34567,9,  true),
            new SetRange(0x45678,0x56789,3,  true)
        };

        private static CheckRange[] checkRanges3 ={
            // dummy start range, also carries the initial value
            new CheckRange(0,      9),
            new CheckRange(0x31,   9),
            new CheckRange(0xa4,   1),
            new CheckRange(0x3400, 9),
            new CheckRange(0x6789, 2),
            new CheckRange(0x45678,9),
            new CheckRange(0x56789,3),
            new CheckRange(0x110000,9)
        };

        [Test]
        public void TestIntTrie()
        {
            _testTrieRanges4(setRanges1, setRanges1.Length, checkRanges1,
                             checkRanges1.Length);
            _testTrieRanges4(setRanges2, setRanges2.Length, checkRanges2,
                             checkRanges2.Length);
            _testTrieRanges4(setRanges3, setRanges3.Length, checkRanges3,
                             checkRanges3.Length);
        }

        private class DummyGetFoldingOffset : Trie.IDataManipulate
        {
            public virtual int GetFoldingOffset(int value)
            {
                return -1; /* never get non-initialValue data for supplementary code points */
            }
        }

        [Test]
        public void TestDummyCharTrie()
        {
            CharTrie trie;
            int initialValue = 0x313, leadUnitValue = 0xaffe;
            int value;
            int c;
            trie = new CharTrie(initialValue, leadUnitValue, new DummyGetFoldingOffset());

            /* test that all code points have initialValue */
            for (c = 0; c <= 0x10ffff; ++c)
            {
                value = trie.GetCodePointValue(c);
                if (value != initialValue)
                {
                    Errln("CharTrie/dummy.getCodePointValue(c)(U+" + Hex(c) + ")=0x" + Hex(value) + " instead of 0x" + Hex(initialValue));
                }
            }

            /* test that the lead surrogate code units have leadUnitValue */
            for (c = 0xd800; c <= 0xdbff; ++c)
            {
                value = trie.GetLeadValue((char)c);
                if (value != leadUnitValue)
                {
                    Errln("CharTrie/dummy.getLeadValue(c)(U+" + Hex(c) + ")=0x" + Hex(value) + " instead of 0x" + Hex(leadUnitValue));
                }
            }
        }

        [Test]
        public void TestDummyIntTrie()
        {
            Int32Trie trie;
            int initialValue = 0x01234567, leadUnitValue = unchecked((int)0x89abcdef);
            int value;
            int c;
            trie = new Int32Trie(initialValue, leadUnitValue, new DummyGetFoldingOffset());

            /* test that all code points have initialValue */
            for (c = 0; c <= 0x10ffff; ++c)
            {
                value = trie.GetCodePointValue(c);
                if (value != initialValue)
                {
                    Errln("IntTrie/dummy.getCodePointValue(c)(U+" + Hex(c) + ")=0x" + Hex(value) + " instead of 0x" + Hex(initialValue));
                }
            }

            /* test that the lead surrogate code units have leadUnitValue */
            for (c = 0xd800; c <= 0xdbff; ++c)
            {
                value = trie.GetLeadValue((char)c);
                if (value != leadUnitValue)
                {
                    Errln("IntTrie/dummy.getLeadValue(c)(U+" + Hex(c) + ")=0x" + Hex(value) + " instead of 0x" + Hex(leadUnitValue));
                }
            }
        }
    }
}
