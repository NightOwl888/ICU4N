using ICU4N.Impl;
using J2N;
using J2N.IO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Character = J2N.Character;

namespace ICU4N.Dev.Test.Util
{
    public class Trie2Test : TestFmwk
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Trie2Test()
        {
        }

        private class Trie2ValueMapper : IValueMapper
        {
            private readonly Func<int, int> map;
            public Trie2ValueMapper(Func<int, int> map)
            {
                this.map = map;
            }

            public int Map(int v)
            {
                return map(v);
            }
        }



        // public methods -----------------------------------------------

        //
        //  TestAPI.  Check that all API methods can be called, and do at least some minimal
        //            operation correctly.  This is not a full test of correct behavior.
        //
        [Test]
        public void TestTrie2API()
        {
            // Trie2.createFromSerialized()
            //   This function is well exercised by TestRanges().

            // Trie2.getVersion(InputStream is, boolean anyEndianOk)
            //

            try
            {
                Trie2Writable trie = new Trie2Writable(0, 0);
                MemoryStream os = new MemoryStream();
                trie.ToTrie2_16().Serialize(os);
                MemoryStream @is = new MemoryStream(os.ToArray());
                assertEquals(null, 2, Trie2.GetVersion(@is, true));
            }
            catch (IOException e)
            {
                Errln(where() + e.ToString());
            }

            // Equals & hashCode
            //
            {
                Trie2Writable trieWA = new Trie2Writable(0, 0);
                Trie2Writable trieWB = new Trie2Writable(0, 0);
                Trie2 trieA = trieWA;
                Trie2 trieB = trieWB;
                assertTrue("", trieA.Equals(trieB));
                assertEquals("", trieA, trieB);
                assertEquals("", trieA.GetHashCode(), trieB.GetHashCode());
                trieWA.Set(500, 2);
                assertNotEquals("", trieA, trieB);
                // Note that the hash codes do not strictly need to be different,
                //   but it's highly likely that something is wrong if they are the same.
                assertNotEquals("", trieA.GetHashCode(), trieB.GetHashCode());
                trieWB.Set(500, 2);
                trieA = trieWA.ToTrie2_16();
                assertEquals("", trieA, trieB);
                assertEquals("", trieA.GetHashCode(), trieB.GetHashCode());
            }

            //
            // Iterator creation
            //
            {
                Trie2Writable trie = new Trie2Writable(17, 0);
                IEnumerator<Trie2Range> it;
                using (it = trie.GetEnumerator())
                {
                    it.MoveNext();
                    Trie2Range r = it.Current;
                    assertEquals("", 0, r.StartCodePoint);
                    assertEquals("", 0x10ffff, r.EndCodePoint);
                    assertEquals("", 17, r.Value);
                    assertEquals("", false, r.IsLeadSurrogate);

                    it.MoveNext();
                    r = it.Current;
                    assertEquals("", 0xd800, r.StartCodePoint);
                    assertEquals("", 0xdbff, r.EndCodePoint);
                    assertEquals("", 17, r.Value);
                    assertEquals("", true, r.IsLeadSurrogate);


                    int i = 0;
                    foreach (Trie2Range rr in trie)
                    {
                        switch (i)
                        {
                            case 0:
                                assertEquals("", 0, rr.StartCodePoint);
                                assertEquals("", 0x10ffff, rr.EndCodePoint);
                                assertEquals("", 17, rr.Value);
                                assertEquals("", false, rr.IsLeadSurrogate);
                                break;
                            case 1:
                                assertEquals("", 0xd800, rr.StartCodePoint);
                                assertEquals("", 0xdbff, rr.EndCodePoint);
                                assertEquals("", 17, rr.Value);
                                assertEquals("", true, rr.IsLeadSurrogate);
                                break;
                            default:
                                Errln(where() + " Unexpected iteration result");
                                break;
                        }
                        i++;
                    }
                }
            }

            // Iteration with a value mapping function
            //
            {
                Trie2Writable trie = new Trie2Writable(0xbadfeed, 0);
                trie.Set(0x10123, 42);

                IValueMapper vm = new Trie2ValueMapper(map: (v) =>
                {
                    if (v == 0xbadfeed)
                    {
                        v = 42;
                    }
                    return v;
                });

                using (IEnumerator<Trie2Range> it2 = trie.GetEnumerator(vm))
                {
                    it2.MoveNext();
                    Trie2Range r = it2.Current;
                    assertEquals("", 0, r.StartCodePoint);
                    assertEquals("", 0x10ffff, r.EndCodePoint);
                    assertEquals("", 42, r.Value);
                    assertEquals("", false, r.IsLeadSurrogate);
                }
            }



            // Iteration over a leading surrogate range.
            //
            {
                Trie2Writable trie = new Trie2Writable(0xdefa17, 0);
                trie.Set(0x2f810, 10);
                using (IEnumerator<Trie2Range> it = trie.GetEnumeratorForLeadSurrogate((char)0xd87e))
                {
                    it.MoveNext();
                    Trie2Range r = it.Current;
                    assertEquals("", 0x2f800, r.StartCodePoint);
                    assertEquals("", 0x2f80f, r.EndCodePoint);
                    assertEquals("", 0xdefa17, r.Value);
                    assertEquals("", false, r.IsLeadSurrogate);

                    it.MoveNext();
                    r = it.Current;
                    assertEquals("", 0x2f810, r.StartCodePoint);
                    assertEquals("", 0x2f810, r.EndCodePoint);
                    assertEquals("", 10, r.Value);
                    assertEquals("", false, r.IsLeadSurrogate);

                    it.MoveNext();
                    r = it.Current;
                    assertEquals("", 0x2f811, r.StartCodePoint);
                    assertEquals("", 0x2fbff, r.EndCodePoint);
                    assertEquals("", 0xdefa17, r.Value);
                    assertEquals("", false, r.IsLeadSurrogate);

                    assertFalse("", it.MoveNext());
                }
            }

            // Iteration over a leading surrogate range with a ValueMapper.
            //
            {
                Trie2Writable trie = new Trie2Writable(0xdefa17, 0);

                trie.Set(0x2f810, 10);

                IValueMapper m = new Trie2ValueMapper(map: (@in) =>
                {
                    if (@in == 10)
                    {
                        @in = 0xdefa17;
                    }
                    return @in;
                });


                using (IEnumerator<Trie2Range> it = trie.GetEnumeratorForLeadSurrogate((char)0xd87e, m))
                {
                    it.MoveNext();
                    Trie2Range r = it.Current;
                    assertEquals("", 0x2f800, r.StartCodePoint);
                    assertEquals("", 0x2fbff, r.EndCodePoint);
                    assertEquals("", 0xdefa17, r.Value);
                    assertEquals("", false, r.IsLeadSurrogate);

                    assertFalse("", it.MoveNext());
                }
            }

            // Trie2.serialize()
            //     Test the implementation in Trie2, which is used with Read Only Tries.
            //
            {
                Trie2Writable trie = new Trie2Writable(101, 0);
                trie.SetRange(0xf000, 0x3c000, 200, true);
                trie.Set(0xffee, 300);
                Trie2_16 frozen16 = trie.ToTrie2_16();
                Trie2_32 frozen32 = trie.ToTrie2_32();
                assertEquals("", trie, frozen16);
                assertEquals("", trie, frozen32);
                assertEquals("", frozen16, frozen32);
                MemoryStream os = new MemoryStream();
                try
                {
                    frozen16.Serialize(os);
                    Trie2 unserialized16 = Trie2.CreateFromSerialized(ByteBuffer.Wrap(os.ToArray()));
                    assertEquals("", trie, unserialized16);
                    assertEquals("", typeof(Trie2_16), unserialized16.GetType());

                    os.Seek(0, SeekOrigin.Begin);
                    frozen32.Serialize(os);
                    Trie2 unserialized32 = Trie2.CreateFromSerialized(ByteBuffer.Wrap(os.ToArray()));
                    assertEquals("", trie, unserialized32);
                    assertEquals("", typeof(Trie2_32), unserialized32.GetType());
                }
                catch (IOException e)
                {
                    Errln(where() + " Unexpected exception:  " + e);
                }


            }
        }


        [Test]
        public void TestTrie2WritableAPI()
        {
            //
            //   Trie2Writable methods.  Check that all functions are present and
            //      nominally working.  Not an in-depth test.
            //

            // Trie2Writable constructor
            Trie2 t1 = new Trie2Writable(6, 666);

            // Constructor from another Trie2
            Trie2 t2 = new Trie2Writable(t1);
            assertTrue("", t1.Equals(t2));

            // Set / Get
            Trie2Writable t1w = new Trie2Writable(10, 666);
            t1w.Set(0x4567, 99);
            assertEquals("", 10, t1w.Get(0x4566));
            assertEquals("", 99, t1w.Get(0x4567));
            assertEquals("", 666, t1w.Get(-1));
            assertEquals("", 666, t1w.Get(0x110000));


            // SetRange
            t1w = new Trie2Writable(10, 666);
            t1w.SetRange(13 /*start*/, 6666 /*end*/, 7788 /*value*/, false  /*overwrite */);
            t1w.SetRange(6000, 7000, 9900, true);
            assertEquals("", 10, t1w.Get(12));
            assertEquals("", 7788, t1w.Get(13));
            assertEquals("", 7788, t1w.Get(5999));
            assertEquals("", 9900, t1w.Get(6000));
            assertEquals("", 9900, t1w.Get(7000));
            assertEquals("", 10, t1w.Get(7001));
            assertEquals("", 666, t1w.Get(0x110000));

            // setRange from a Trie2.Range
            //    (Ranges are more commonly created by iterating over a Trie2,
            //     but create one by hand here)
            Trie2Range r = new Trie2Range();
            r.StartCodePoint = 50;
            r.EndCodePoint = 52;
            r.Value = 0x12345678;
            r.IsLeadSurrogate = false;
            t1w = new Trie2Writable(0, 0xbad);
            t1w.SetRange(r, true);
            assertEquals(null, 0, t1w.Get(49));
            assertEquals("", 0x12345678, t1w.Get(50));
            assertEquals("", 0x12345678, t1w.Get(52));
            assertEquals("", 0, t1w.Get(53));


            // setForLeadSurrogateCodeUnit / getFromU16SingleLead
            t1w = new Trie2Writable(10, 0xbad);
            assertEquals("", 10, t1w.GetFromU16SingleLead((char)0x0d801));
            t1w.SetForLeadSurrogateCodeUnit((char)0xd801, 5000);
            t1w.Set(0xd801, 6000);
            assertEquals("", 5000, t1w.GetFromU16SingleLead((char)0x0d801));
            assertEquals("", 6000, t1w.Get(0x0d801));

            // get().  Is covered by nearly every other test.


            // Trie2_16 getAsFrozen_16()
            t1w = new Trie2Writable(10, 666);
            t1w.Set(42, 5555);
            t1w.Set(0x1ff00, 224);
            Trie2_16 t1_16 = t1w.ToTrie2_16();
            assertTrue("", t1w.Equals(t1_16));
            // alter the writable Trie2 and then re-freeze.
            t1w.Set(152, 129);
            t1_16 = t1w.ToTrie2_16();
            assertTrue("", t1w.Equals(t1_16));
            assertEquals("", 129, t1w.Get(152));

            // Trie2_32 getAsFrozen_32()
            //
            t1w = new Trie2Writable(10, 666);
            t1w.Set(42, 5555);
            t1w.Set(0x1ff00, 224);
            Trie2_32 t1_32 = t1w.ToTrie2_32();
            assertTrue("", t1w.Equals(t1_32));
            // alter the writable Trie2 and then re-freeze.
            t1w.Set(152, 129);
            assertNotEquals("", t1_32, t1w);
            t1_32 = t1w.ToTrie2_32();
            assertTrue("", t1w.Equals(t1_32));
            assertEquals("", 129, t1w.Get(152));


            // serialize(OutputStream os, ValueWidth width)
            //
            MemoryStream os = new MemoryStream();
            t1w = new Trie2Writable(0, 0xbad);
            t1w.Set(0x41, 0x100);
            t1w.Set(0xc2, 0x200);
            t1w.Set(0x404, 0x300);
            t1w.Set(0xd903, 0x500);
            t1w.Set(0xdd29, 0x600);
            t1w.Set(0x1055d3, 0x700);
            t1w.SetForLeadSurrogateCodeUnit((char)0xda1a, 0x800);
            try
            {
                // Serialize to 16 bits.
                int serializedLen = t1w.ToTrie2_16().Serialize(os);
                // Fragile test.  Serialized length could change with changes to compaction.
                //                But it should not change unexpectedly.
                assertEquals("", 3508, serializedLen);
                Trie2 t1ws16 = Trie2.CreateFromSerialized(ByteBuffer.Wrap(os.ToArray()));
                assertEquals("", t1ws16.GetType(), typeof(Trie2_16));
                assertEquals("", t1w, t1ws16);

                // Serialize to 32 bits
                os.Seek(0, SeekOrigin.Begin);
                serializedLen = t1w.ToTrie2_32().Serialize(os);
                // Fragile test.  Serialized length could change with changes to compaction.
                //                But it should not change unexpectedly.
                assertEquals("", 4332, serializedLen);
                Trie2 t1ws32 = Trie2.CreateFromSerialized(ByteBuffer.Wrap(os.ToArray()));
                assertEquals("", t1ws32.GetType(), typeof(Trie2_32));
                assertEquals("", t1w, t1ws32);
            }
            catch (IOException e)
            {
                Errln(where() + e.ToString());
            }


        }

        [Test]
        public void TestCharSequenceIterator()
        {
            String text = "abc123\ud800\udc01 ";    // Includes a Unicode supplemental character
            String vals = "LLLNNNX?S";

            Trie2Writable tw = new Trie2Writable(0, 666);
            tw.SetRange('a', 'z', 'L', false);
            tw.SetRange('1', '9', 'N', false);
            tw.Set(' ', 'S');
            tw.Set(0x10001, 'X');

            using (Trie2CharSequenceEnumerator it = tw.GetCharSequenceEnumerator(text, 0))
            {

                // Check forwards iteration.
                Trie2CharSequenceValues ir;
                int i;
                for (i = 0; it.MoveNext(); i++)
                {
                    ir = it.Current;
                    int expectedCP = Character.CodePointAt(text, i);
                    assertEquals("" + " i=" + i, expectedCP, ir.CodePoint);
                    assertEquals("" + " i=" + i, i, ir.Index);
                    assertEquals("" + " i=" + i, vals[i], ir.Value);
                    if (expectedCP >= 0x10000)
                    {
                        i++;
                    }
                }
                assertEquals("", text.Length, i);

                // Check reverse iteration, starting at an intermediate point.
                it.Set(5);
                for (i = 5; it.MovePrevious();)
                {
                    ir = it.Current;
                    int expectedCP = Character.CodePointBefore(text, i);
                    i -= (expectedCP < 0x10000 ? 1 : 2);
                    assertEquals("" + " i=" + i, expectedCP, ir.CodePoint);
                    assertEquals("" + " i=" + i, i, ir.Index);
                    assertEquals("" + " i=" + i, vals[i], ir.Value);
                }
                assertEquals("", 0, i);
            }
        }


        //
        //  Port of Tests from ICU4C ...
        //
        //     setRanges array elements are
        //        {start Code point, limit CP, value, overwrite}
        //
        //     There must be an entry with limit 0 and with the intialValue.
        //     It may be preceded by an entry with negative limit and the errorValue.
        //
        //     checkRanges array elemets are
        //        { limit code point, value}
        //
        //     The expected value range is from the previous boundary's limit to before
        //        this boundary's limit

        //
        String[] trieNames = { "setRanges1", "setRanges2", "setRanges3", "setRangesEmpty", "setRangesSingleValue" };
        /* set consecutive ranges, even with value 0 */



        private static int[][] setRanges1 ={
         new int[] { 0,        0,        0,      0 },
         new int[] { 0,        0x40,     0,      0 },
         new int[] { 0x40,     0xe7,     0x1234, 0 },
         new int[] { 0xe7,     0x3400,   0,      0 },
         new int[] { 0x3400,   0x9fa6,   0x6162, 0 },
         new int[] { 0x9fa6,   0xda9e,   0x3132, 0 },
         new int[] { 0xdada,   0xeeee,   0x87ff, 0 },
         new int[] { 0xeeee,   0x11111,  1,      0 },
         new int[] { 0x11111,  0x44444,  0x6162, 0 },
         new int[] { 0x44444,  0x60003,  0,      0 },
         new int[] { 0xf0003,  0xf0004,  0xf,    0 },
         new int[] { 0xf0004,  0xf0006,  0x10,   0 },
         new int[] { 0xf0006,  0xf0007,  0x11,   0 },
         new int[] { 0xf0007,  0xf0040,  0x12,   0 },
         new int[] { 0xf0040,  0x110000, 0,      0 }
     };

        private static int[][] checkRanges1 = {
         new int[] { 0,        0 },
         new int[] { 0x40,     0 },
         new int[] { 0xe7,     0x1234 },
         new int[] { 0x3400,   0 },
         new int[] { 0x9fa6,   0x6162 },
         new int[] { 0xda9e,   0x3132 },
         new int[] { 0xdada,   0 },
         new int[] { 0xeeee,   0x87ff },
         new int[] { 0x11111,  1 },
         new int[] { 0x44444,  0x6162 },
         new int[] { 0xf0003,  0 },
         new int[] { 0xf0004,  0xf },
         new int[] { 0xf0006,  0x10 },
         new int[] { 0xf0007,  0x11 },
         new int[] { 0xf0040,  0x12 },
         new int[] { 0x110000, 0 }
     };

        /* set some interesting overlapping ranges */
        private static int[][] setRanges2 ={
         new int[] { 0,        0,        0,      0 },
         new int[] { 0x21,     0x7f,     0x5555, 1 },
         new int[] { 0x2f800,  0x2fedc,  0x7a,   1 },
         new int[] { 0x72,     0xdd,     3,      1 },
         new int[] { 0xdd,     0xde,     4,      0 },
         new int[] { 0x201,    0x240,    6,      1 },  /* 3 consecutive blocks with the same pattern but */
         new int[] { 0x241,    0x280,    6,      1 },  /* discontiguous value ranges, testing utrie2_enum() */
         new int[] { 0x281,    0x2c0,    6,      1 },
         new int[] { 0x2f987,  0x2fa98,  5,      1 },
         new int[] { 0x2f777,  0x2f883,  0,      1 },
         new int[] { 0x2f900,  0x2ffaa,  1,      0 },
         new int[] { 0x2ffaa,  0x2ffab,  2,      1 },
         new int[] { 0x2ffbb,  0x2ffc0,  7,      1 }
     };

        private static int[][] checkRanges2 ={
         new int[] { 0,        0 },
         new int[] { 0x21,     0 },
         new int[] { 0x72,     0x5555 },
         new int[] { 0xdd,     3 },
         new int[] { 0xde,     4 },
         new int[] { 0x201,    0 },
         new int[] { 0x240,    6 },
         new int[] { 0x241,    0 },
         new int[] { 0x280,    6 },
         new int[] { 0x281,    0 },
         new int[] { 0x2c0,    6 },
         new int[] { 0x2f883,  0 },
         new int[] { 0x2f987,  0x7a },
         new int[] { 0x2fa98,  5 },
         new int[] { 0x2fedc,  0x7a },
         new int[] { 0x2ffaa,  1 },
         new int[] { 0x2ffab,  2 },
         new int[] { 0x2ffbb,  0 },
         new int[] { 0x2ffc0,  7 },
         new int[] { 0x110000, 0 }
     };

        /*
             private static int[] [] checkRanges2_d800={
                 new int[] { 0x10000,  0 },
                 new int[] { 0x10400,  0 }
             };

             private static int[][] checkRanges2_d87e={
                 new int[] { 0x2f800,  6 },
                 new int[] { 0x2f883,  0 },
                 new int[] { 0x2f987,  0x7a },
                 new int[] { 0x2fa98,  5 },
                 new int[] { 0x2fc00,  0x7a }
             };

             private static int[][] checkRanges2_d87f={
                 new int[] { 0x2fc00,  0 },
                 new int[] { 0x2fedc,  0x7a },
                 new int[] { 0x2ffaa,  1 },
                 new int[] { 0x2ffab,  2 },
                 new int[] { 0x2ffbb,  0 },
                 new int[] { 0x2ffc0,  7 },
                 new int[] { 0x30000,  0 }
             };

             private static int[][]  checkRanges2_dbff={
                 new int[] { 0x10fc00, 0 },
                 new int[] { 0x110000, 0 }
             };
        */

        /* use a non-zero initial value */
        private static int[][] setRanges3 ={
         new int[] { 0,        0,        9, 0 },     // non-zero initial value.
         new int[] { 0x31,     0xa4,     1, 0 },
         new int[] { 0x3400,   0x6789,   2, 0 },
         new int[] { 0x8000,   0x89ab,   9, 1 },
         new int[] { 0x9000,   0xa000,   4, 1 },
         new int[] { 0xabcd,   0xbcde,   3, 1 },
         new int[] { 0x55555,  0x110000, 6, 1 },  /* highStart<U+ffff with non-initialValue */
         new int[] { 0xcccc,   0x55555,  6, 1 }
     };

        private static int[][] checkRanges3 ={
         new int[] { 0,        9 },  /* non-zero initialValue */
         new int[] { 0x31,     9 },
         new int[] { 0xa4,     1 },
         new int[] { 0x3400,   9 },
         new int[] { 0x6789,   2 },
         new int[] { 0x9000,   9 },
         new int[] { 0xa000,   4 },
         new int[] { 0xabcd,   9 },
         new int[] { 0xbcde,   3 },
         new int[] { 0xcccc,   9 },
         new int[] { 0x110000, 6 }
     };

        /* empty or single-value tries, testing highStart==0 */
        private static int[][] setRangesEmpty ={
         new int[] { 0,        0,        3, 0 }         // Only the element with the initial value.
     };

        private static int[][] checkRangesEmpty ={
         new int[] { 0,        3 },
         new int[] { 0x110000, 3 }
     };

        private static int[][] setRangesSingleValue ={
         new int[] { 0,        0,        3,  0 },   // Initial value = 3
         new int[] { 0,        0x110000, 5, 1 },
     };

        private static int[][] checkRangesSingleValue ={
         new int[] { 0,        3 },
         new int[] { 0x110000, 5 }
     };


        //
        // Create a test Trie2 from a setRanges test array.
        //    Range data ported from C.
        //
        private Trie2Writable genTrieFromSetRanges(int[][] ranges)
        {
            int i = 0;
            int initialValue = 0;
            int errorValue = 0x0bad;

            if (ranges[i][1] < 0)
            {
                errorValue = ranges[i][2];
                i++;
            }
            initialValue = ranges[i++][2];
            Trie2Writable trie = new Trie2Writable(initialValue, errorValue);

            for (; i < ranges.Length; i++)
            {
                int rangeStart = ranges[i][0];
                int rangeEnd = ranges[i][1] - 1;
                int value = ranges[i][2];
                bool overwrite = (ranges[i][3] != 0);
                trie.SetRange(rangeStart, rangeEnd, value, overwrite);
            }

            // Insert some non-default values for lead surrogates.
            //   TODO:  this should be represented in the data.
            trie.SetForLeadSurrogateCodeUnit((char)0xd800, 90);
            trie.SetForLeadSurrogateCodeUnit((char)0xd999, 94);
            trie.SetForLeadSurrogateCodeUnit((char)0xdbff, 99);

            return trie;
        }


        //
        //  Check the expected values from a single Trie2.
        //
        private void trieGettersTest(String testName,
                                     Trie2 trie,         // The Trie2 to test.
                                     int[][] checkRanges)  // Expected data.
                                                           //   Tuples of (value, high limit code point)
                                                           //   High limit is first code point following the range
                                                           //   with the indicated value.
                                                           //      (Structures copied from ICU4C tests.)
        {
            int countCheckRanges = checkRanges.Length;

            int initialValue, errorValue;
            int value, value2;
            int start, limit;
            int i, countSpecials;

            countSpecials = 0;  /*getSpecialValues(checkRanges, countCheckRanges, &initialValue, &errorValue);*/
            errorValue = 0x0bad;
            initialValue = 0;
            if (checkRanges[countSpecials][0] == 0)
            {
                initialValue = checkRanges[countSpecials][1];
                countSpecials++;
            }

            start = 0;
            for (i = countSpecials; i < countCheckRanges; ++i)
            {
                limit = checkRanges[i][0];
                value = checkRanges[i][1];

                while (start < limit)
                {
                    value2 = trie.Get(start);
                    if (value != value2)
                    {
                        // The redundant if, outside of the assert, is for speed.
                        // It makes a significant difference for this test.
                        assertEquals("wrong value for " + testName + " of " + (start).ToHexString(), value, value2);
                    }
                    ++start;
                }
            }


            if (!testName.StartsWith("dummy", StringComparison.Ordinal) && !testName.StartsWith("trie1", StringComparison.Ordinal))
            {
                /* Test values for lead surrogate code units.
                 * For non-lead-surrogate code units,  getFromU16SingleLead() and get()
                 *   should be the same.
                 */
                for (start = 0xd7ff; start < 0xdc01; ++start)
                {
                    switch (start)
                    {
                        case 0xd7ff:
                        case 0xdc00:
                            value = trie.Get(start);
                            break;
                        case 0xd800:
                            value = 90;
                            break;
                        case 0xd999:
                            value = 94;
                            break;
                        case 0xdbff:
                            value = 99;
                            break;
                        default:
                            value = initialValue;
                            break;
                    }
                    value2 = trie.GetFromU16SingleLead((char)start);
                    if (value2 != value)
                    {
                        Errln(where() + " testName: " + testName + " getFromU16SingleLead() failed." +
                                "char, exected, actual = " + (start).ToHexString() + ", " +
                                (value).ToHexString() + ", " + (value2).ToHexString());
                    }
                }
            }

            /* test errorValue */
            value = trie.Get(-1);
            value2 = trie.Get(0x110000);
            if (value != errorValue || value2 != errorValue)
            {
                Errln("trie2.Get() error value test.  Expected, actual1, actual2 = " +
                        errorValue + ", " + value + ", " + value2);
            }

            // Check that Trie enumeration produces the same contents as simple get()
            foreach (Trie2Range range in trie)
            {
                for (int cp = range.StartCodePoint; cp <= range.EndCodePoint; cp++)
                {
                    if (range.IsLeadSurrogate)
                    {
                        assertTrue(testName, cp >= (char)0xd800 && cp < (char)0xdc00);
                        assertEquals(testName, range.Value, trie.GetFromU16SingleLead((char)cp));
                    }
                    else
                    {
                        assertEquals(testName, range.Value, trie.Get(cp));
                    }
                }
            }
        }

        // Was testTrieRanges in ICU4C.  Renamed to not conflict with ICU4J test framework.
        private void checkTrieRanges(String testName, String serializedName, bool withClone,
                int[][] setRanges, int[][] checkRanges)
        {
            string ns =
#if FEATURE_TYPEEXTENSIONS_GETTYPEINFO
            typeof(Trie2Test).GetTypeInfo().Namespace;
#else
            typeof(Trie2Test).Namespace;
#endif

            // Run tests against Tries that were built by ICU4C and serialized.
            String fileName16 = ns + ".Trie2Test." + serializedName + ".16.tri2";
            String fileName32 = ns + ".Trie2Test." + serializedName + ".32.tri2";

#if FEATURE_TYPEEXTENSIONS_GETTYPEINFO
            Assembly assembly = typeof(Trie2Test).GetTypeInfo().Assembly;
#else
            Assembly assembly = typeof(Trie2Test).Assembly;
#endif

            Stream @is = assembly.GetManifestResourceStream(fileName16);
            Trie2 trie16;
            try
            {
                trie16 = Trie2.CreateFromSerialized(ICUBinary.GetByteBufferFromStreamAndDisposeStream(@is));
            }
            finally
            {
                @is.Dispose();
            }
            trieGettersTest(testName, trie16, checkRanges);

            @is = assembly.GetManifestResourceStream(fileName32);
            Trie2 trie32;
            try
            {
                trie32 = Trie2.CreateFromSerialized(ICUBinary.GetByteBufferFromStreamAndDisposeStream(@is));
            }
            finally
            {
                @is.Dispose();
            }
            trieGettersTest(testName, trie32, checkRanges);

            // Run the same tests against locally contructed Tries.
            Trie2Writable trieW = genTrieFromSetRanges(setRanges);
            trieGettersTest(testName, trieW, checkRanges);
            assertEquals("", trieW, trie16);   // Locally built tries must be
            assertEquals("", trieW, trie32);   //   the same as those imported from ICU4C


            Trie2_32 trie32a = trieW.ToTrie2_32();
            trieGettersTest(testName, trie32a, checkRanges);

            Trie2_16 trie16a = trieW.ToTrie2_16();
            trieGettersTest(testName, trie16a, checkRanges);

        }

        // Was "TrieTest" in trie2test.c
        [Test]
        public void TestRanges()
        {
            checkTrieRanges("set1", "setRanges1", false, setRanges1, checkRanges1);
            checkTrieRanges("set2-overlap", "setRanges2", false, setRanges2, checkRanges2);
            checkTrieRanges("set3-initial-9", "setRanges3", false, setRanges3, checkRanges3);
            checkTrieRanges("set-empty", "setRangesEmpty", false, setRangesEmpty, checkRangesEmpty);
            checkTrieRanges("set-single-value", "setRangesSingleValue", false, setRangesSingleValue,
                     checkRangesSingleValue);
            checkTrieRanges("set2-overlap.withClone", "setRanges2", true, setRanges2, checkRanges2);
        }


        private String where()
        {
            // ICU4N TODO: finish
            return string.Empty;
            //StackTraceElement[] st = new Throwable().getStackTrace();
            //String w = "File: " + st[1].getFileName() + ", Line " + st[1].getLineNumber();
            //return w;
        }
    }
}
