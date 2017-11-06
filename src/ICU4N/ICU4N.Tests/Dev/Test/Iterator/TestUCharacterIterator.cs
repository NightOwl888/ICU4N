using ICU4N.Support.Text;
using ICU4N.TestFramework.Dev.Test;
using ICU4N.Text;
using NUnit.Framework;
using System;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Tests.Dev.Test.Iterator
{
    public class TestUCharacterIterator : TestFmwk
    {
        // constructor -----------------------------------------------------

        /**
         * Constructor
         */
        public TestUCharacterIterator()
        {
        }

        // public methods --------------------------------------------------

        /**
        * Testing cloning
        */
        [Test]
        public void TestClone()
        {
            UCharacterIterator iterator = UCharacterIterator.GetInstance("testing");
            UCharacterIterator cloned = (UCharacterIterator)iterator.Clone();
            int completed = 0;
            while (completed != UCharacterIterator.DONE)
            {
                completed = iterator.Next();
                if (completed != cloned.Next())
                {
                    Errln("Cloned operation failed");
                }
            }
        }
        public void getText(UCharacterIterator iterator, String result)
        {
            /* test getText */
            char[] buf = new char[1];
            for (; ; )
            {
                try
                {
                    iterator.GetText(buf);
                    break;
                }
                catch (IndexOutOfRangeException e)
                {
                    buf = new char[iterator.Length];
                }
            }
            if (result.CompareToOrdinal(new string(buf, 0, iterator.Length)) != 0)
            {
                Errln("getText failed for iterator");
            }
        }

        /**
         * Testing iteration
         */
        [Test]
        public void TestIteration()
        {
            UCharacterIterator iterator = UCharacterIterator.GetInstance(
                                                           ITERATION_STRING_);
            UCharacterIterator iterator2 = UCharacterIterator.GetInstance(
                                                           ITERATION_STRING_);
            iterator.SetToStart();
            if (iterator.Current != ITERATION_STRING_[0])
            {
                Errln("Iterator failed retrieving first character");
            }
            iterator.SetToLimit();
            if (iterator.Previous() != ITERATION_STRING_[
                                           ITERATION_STRING_.Length - 1])
            {
                Errln("Iterator failed retrieving last character");
            }
            if (iterator.Length != ITERATION_STRING_.Length)
            {
                Errln("Iterator failed determining begin and end index");
            }
            iterator2.Index = 0;
            iterator.Index = 0;
            int ch = 0;
            while (ch != UCharacterIterator.DONE)
            {
                int index = iterator2.Index;
                ch = iterator2.NextCodePoint();
                if (index != ITERATION_SUPPLEMENTARY_INDEX)
                {
                    if (ch != iterator.Next() &&
                        ch != UCharacterIterator.DONE)
                    {
                        Errln("Error mismatch in next() and nextCodePoint()");
                    }
                }
                else
                {
                    if (UTF16.GetLeadSurrogate(ch) != iterator.Next() ||
                        UTF16.GetTrailSurrogate(ch) != iterator.Next())
                    {
                        Errln("Error mismatch in next and nextCodePoint for " +
                              "supplementary characters");
                    }
                }
            }
            iterator.Index = ITERATION_STRING_.Length;
            iterator2.Index = ITERATION_STRING_.Length;
            while (ch != UCharacterIterator.DONE)
            {
                int index = iterator2.Index;
                ch = iterator2.PreviousCodePoint();
                if (index != ITERATION_SUPPLEMENTARY_INDEX)
                {
                    if (ch != iterator.Previous() &&
                        ch != UCharacterIterator.DONE)
                    {
                        Errln("Error mismatch in previous() and " +
                              "previousCodePoint()");
                    }
                }
                else
                {
                    if (UTF16.GetLeadSurrogate(ch) != iterator.Previous() ||
                        UTF16.GetTrailSurrogate(ch) != iterator.Previous())
                    {
                        Errln("Error mismatch in previous and " +
                              "previousCodePoint for supplementary characters");
                    }
                }
            }
        }

        //Tests for new API for utf-16 support
        [Test]
        public void TestIterationUChar32()
        {
            String text = "\u0061\u0062\ud841\udc02\u20ac\ud7ff\ud842\udc06\ud801\udc00\u0061";
            int c;
            int i;
            {
                UCharacterIterator iter = UCharacterIterator.GetInstance(text);

                String iterText = iter.GetText();
                if (!iterText.Equals(text))
                    Errln("iter.getText() failed");

                iter.Index = (1);
                if (iter.CurrentCodePoint() != UTF16.CharAt(text, 1))
                    Errln("Iterator didn't start out in the right place.");

                iter.SetToStart();
                c = iter.CurrentCodePoint();
                i = 0;
                i = iter.MoveCodePointIndex(1);
                c = iter.CurrentCodePoint();
                if (c != UTF16.CharAt(text, 1) || i != 1)
                    Errln("moveCodePointIndex(1) didn't work correctly expected " + Hex(c) + " got " + Hex(UTF16.CharAt(text, 1)) + " i= " + i);

                i = iter.MoveCodePointIndex(2);
                c = iter.CurrentCodePoint();
                if (c != UTF16.CharAt(text, 4) || i != 4)
                    Errln("moveCodePointIndex(2) didn't work correctly expected " + Hex(c) + " got " + Hex(UTF16.CharAt(text, 4)) + " i= " + i);

                i = iter.MoveCodePointIndex(-2);
                c = iter.CurrentCodePoint();
                if (c != UTF16.CharAt(text, 1) || i != 1)
                    Errln("moveCodePointIndex(-2) didn't work correctly expected " + Hex(c) + " got " + Hex(UTF16.CharAt(text, 1)) + " i= " + i);

                iter.SetToLimit();
                i = iter.MoveCodePointIndex(-2);
                c = iter.CurrentCodePoint();
                if (c != UTF16.CharAt(text, (text.Length - 3)) || i != (text.Length - 3))
                    Errln("moveCodePointIndex(-2) didn't work correctly expected " + Hex(c) + " got " + Hex(UTF16.CharAt(text, (text.Length - 3))) + " i= " + i);

                iter.SetToStart();
                c = iter.CurrentCodePoint();
                i = 0;

                //testing first32PostInc, nextCodePointPostInc, setTostart
                i = 0;
                iter.SetToStart();
                c = iter.Next();
                if (c != UTF16.CharAt(text, i))
                    Errln("first32PostInc failed.  Expected->" + Hex(UTF16.CharAt(text, i)) + " Got-> " + Hex(c));
                if (iter.Index != UTF16.GetCharCount(c) + i)
                    Errln("getIndex() after first32PostInc() failed");

                iter.SetToStart();
                i = 0;
                if (iter.Index != 0)
                    Errln("setToStart failed");

                Logln("Testing forward iteration...");
                do
                {
                    if (c != UCharacterIterator.DONE)
                        c = iter.NextCodePoint();

                    if (c != UTF16.CharAt(text, i))
                        Errln("Character mismatch at position " + i + ", iterator has " + Hex(c) + ", string has " + Hex(UTF16.CharAt(text, i)));

                    i += UTF16.GetCharCount(c);
                    if (iter.Index != i)
                        Errln("getIndex() aftr nextCodePointPostInc() isn't working right");
                    c = iter.CurrentCodePoint();
                    if (c != UCharacterIterator.DONE && c != UTF16.CharAt(text, i))
                        Errln("current() after nextCodePointPostInc() isn't working right");

                } while (c != UCharacterIterator.DONE);
                c = iter.NextCodePoint();
                if (c != UCharacterIterator.DONE)
                    Errln("nextCodePointPostInc() didn't return DONE at the beginning");


            }
        }

        class UCharIterator
        {

            public UCharIterator(int[] src, int len, int index)
            {

                s = src;
                length = len;
                i = index;
            }

            public int Current
            {
                get
                {
                    if (i < length)
                    {
                        return s[i];
                    }
                    else
                    {
                        return -1;
                    }
                }
            }

            public int Next()
            {
                if (i < length)
                {
                    return s[i++];
                }
                else
                {
                    return -1;
                }
            }

            public int Previous()
            {
                if (i > 0)
                {
                    return s[--i];
                }
                else
                {
                    return -1;
                }
            }

            public int Index
            {
                get { return i; }
            }

            private int[] s;
            private int length, i;
        }
        [Test]
        public void TestPreviousNext()
        {
            // src and expect strings
            char[] src ={
                UTF16.GetLeadSurrogate(0x2f999), UTF16.GetTrailSurrogate(0x2f999),
                UTF16.GetLeadSurrogate(0x1d15f), UTF16.GetTrailSurrogate(0x1d15f),
                (char)0xc4,
                (char)0x1ed0
            };
            // iterators
            UCharacterIterator iter1 = UCharacterIterator.GetInstance(new ReplaceableString(new String(src)));
            UCharacterIterator iter2 = UCharacterIterator.GetInstance(src/*char array*/);
            UCharacterIterator iter3 = UCharacterIterator.GetInstance(new StringCharacterIterator(new String(src)));
            UCharacterIterator iter4 = UCharacterIterator.GetInstance(new StringBuffer(new String(src)));
            previousNext(iter1);
            previousNext(iter2);
            previousNext(iter3);
            previousNext(iter4);
            getText(iter1, new String(src));
            getText(iter2, new String(src));
            getText(iter3, new String(src));
            /* getCharacterIterator */
            CharacterIterator citer1 = iter1.GetCharacterIterator();
            CharacterIterator citer2 = iter2.GetCharacterIterator();
            CharacterIterator citer3 = iter3.GetCharacterIterator();
            if (citer1.First() != iter1.Current)
            {
                Errln("getCharacterIterator for iter1 failed");
            }
            if (citer2.First() != iter2.Current)
            {
                Errln("getCharacterIterator for iter2 failed");
            }
            if (citer3.First() != iter3.Current)
            {
                Errln("getCharacterIterator for iter3 failed");
            }
            /* Test clone()  && moveIndex()*/
            try
            {
                UCharacterIterator clone1 = (UCharacterIterator)iter1.Clone();
                UCharacterIterator clone2 = (UCharacterIterator)iter2.Clone();
                UCharacterIterator clone3 = (UCharacterIterator)iter3.Clone();
                if (clone1.MoveIndex(3) != iter1.MoveIndex(3))
                {
                    Errln("moveIndex for iter1 failed");
                }
                if (clone2.MoveIndex(3) != iter2.MoveIndex(3))
                {
                    Errln("moveIndex for iter2 failed");
                }
                if (clone3.MoveIndex(3) != iter3.MoveIndex(3))
                {
                    Errln("moveIndex for iter1 failed");
                }
            }
            catch (Exception e)
            {
                Errln("could not clone the iterator");
            }
        }
        public void previousNext(UCharacterIterator iter)
        {

            int[] expect ={
            0x2f999,
            0x1d15f,
            0xc4,
            0x1ed0
        };

            // expected src indexes corresponding to expect indexes
            int[] expectIndex ={
            0,0,
            1,1,
            2,
            3,
            4 //needed
        };

            // initial indexes into the src and expect strings

            int SRC_MIDDLE = 4;
            int EXPECT_MIDDLE = 2;


            // movement vector
            // - for previous(), 0 for current(), + for next()
            // not const so that we can terminate it below for the error message
            String moves = "0+0+0--0-0-+++0--+++++++0--------";


            UCharIterator iter32 = new UCharIterator(expect, expect.Length,
                                                         EXPECT_MIDDLE);

            int c1, c2;
            char m;

            // initially set the indexes into the middle of the strings
            iter.Index = (SRC_MIDDLE);

            // move around and compare the iteration code points with
            // the expected ones
            int movesIndex = 0;
            while (movesIndex < moves.Length)
            {
                m = moves[movesIndex++];
                if (m == '-')
                {
                    c1 = iter.PreviousCodePoint();
                    c2 = iter32.Previous();
                }
                else if (m == '0')
                {
                    c1 = iter.CurrentCodePoint();
                    c2 = iter32.Current;
                }
                else
                {// m=='+'
                    c1 = iter.NextCodePoint();
                    c2 = iter32.Next();
                }

                // compare results
                if (c1 != c2)
                {
                    // copy the moves until the current (m) move, and terminate
                    String history = moves.Substring(0, movesIndex - 0);
                    Errln("error: mismatch in Normalizer iteration at " + history + ": "
                          + "got c1= " + Hex(c1) + " != expected c2= " + Hex(c2));
                    break;
                }

                // compare indexes
                if (expectIndex[iter.Index] != iter32.Index)
                {
                    // copy the moves until the current (m) move, and terminate
                    String history = moves.Substring(0, movesIndex - 0);
                    Errln("error: index mismatch in Normalizer iteration at "
                          + history + " : " + "Normalizer index " + iter.Index
                          + " expected " + expectIndex[iter32.Index]);
                    break;
                }
            }
        }
        [Test]
        public void TestUCharacterIteratorWrapper()
        {
            String source = "asdfasdfjoiuyoiuy2341235679886765";
            UCharacterIterator it = UCharacterIterator.GetInstance(source);
            CharacterIterator wrap_ci = it.GetCharacterIterator();
            CharacterIterator ci = new StringCharacterIterator(source);
            wrap_ci.SetIndex(10);
            ci.SetIndex(10);
            String moves = "0+0+0--0-0-+++0--+++++++0--------++++0000----0-";
            int c1, c2;
            char m;
            int movesIndex = 0;

            while (movesIndex < moves.Length)
            {
                m = moves[movesIndex++];
                if (m == '-')
                {
                    c1 = wrap_ci.Previous();
                    c2 = ci.Previous();
                }
                else if (m == '0')
                {
                    c1 = wrap_ci.Current;
                    c2 = ci.Current;
                }
                else
                {// m=='+'
                    c1 = wrap_ci.Next();
                    c2 = ci.Next();
                }

                // compare results
                if (c1 != c2)
                {
                    // copy the moves until the current (m) move, and terminate
                    String history = moves.Substring(0, movesIndex - 0);
                    Errln("error: mismatch in Normalizer iteration at " + history + ": "
                          + "got c1= " + Hex(c1) + " != expected c2= " + Hex(c2));
                    break;
                }

                // compare indexes
                if (wrap_ci.Index != ci.Index)
                {
                    // copy the moves until the current (m) move, and terminate
                    String history = moves.Substring(0, movesIndex - 0);
                    Errln("error: index mismatch in Normalizer iteration at "
                          + history + " : " + "Normalizer index " + wrap_ci.Index
                          + " expected " + ci.Index);
                    break;
                }
            }
            if (ci.First() != wrap_ci.First())
            {
                Errln("CharacterIteratorWrapper.First() failed. expected: " + ci.First() + " got: " + wrap_ci.First());
            }
            if (ci.Last() != wrap_ci.Last())
            {
                Errln("CharacterIteratorWrapper.Last() failed expected: " + ci.Last() + " got: " + wrap_ci.Last());
            }
            if (ci.BeginIndex != wrap_ci.BeginIndex)
            {
                Errln("CharacterIteratorWrapper.BeginIndex failed expected: " + ci.BeginIndex + " got: " + wrap_ci.BeginIndex);
            }
            if (ci.EndIndex != wrap_ci.EndIndex)
            {
                Errln("CharacterIteratorWrapper.EndIndex failed expected: " + ci.EndIndex + " got: " + wrap_ci.EndIndex);
            }
            try
            {
                CharacterIterator cloneWCI = (CharacterIterator)wrap_ci.Clone();
                if (wrap_ci.Index != cloneWCI.Index)
                {
                    Errln("CharacterIteratorWrapper.Clone() failed expected: " + wrap_ci.Index + " got: " + cloneWCI.Index);
                }
            }
            catch (Exception e)
            {
                Errln("CharacterIterator.Clone() failed");
            }
        }
        // private data members ---------------------------------------------

        private static String ITERATION_STRING_ =
                                                "Testing 1 2 3 \ud800\udc00 456";
        private static int ITERATION_SUPPLEMENTARY_INDEX = 14;

        [Test]
        public void TestJitterbug1952()
        {
            //test previous code point
            char[] src = new char[] { '\uDC00', '\uD800', '\uDC01', '\uD802', '\uDC02', '\uDC03' };
            UCharacterIterator iter = UCharacterIterator.GetInstance(src);
            iter.Index = 1;
            int ch;
            // this should never go into a infinite loop
            // if it does then we have a problem
            while ((ch = iter.PreviousCodePoint()) != UCharacterIterator.DONE)
            {
                if (ch != 0xDc00)
                {
                    Errln("iter.PreviousCodePoint() failed");
                }
            }
            iter.Index = (5);
            while ((ch = iter.NextCodePoint()) != UCharacterIterator.DONE)
            {
                if (ch != 0xDC03)
                {
                    Errln("iter.NextCodePoint() failed");
                }
            }
        }
    }
}
