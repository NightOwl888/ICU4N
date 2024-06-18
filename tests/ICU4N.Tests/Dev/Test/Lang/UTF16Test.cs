using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Text;
using J2N;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Globalization;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Lang
{
    /// <summary>
    /// Testing class for UTF16
    /// </summary>
    /// <author>Syn Wee Quek</author>
    /// <since>feb 09 2001</since>
    public sealed class UTF16Test : TestFmwk
    {
        // constructor ===================================================

        /**
         * Constructor
         */
        public UTF16Test()
        {
        }

        // public methods ================================================

        /**
         * Testing UTF16 class methods append
         */
        [Test]
        public void TestAppend()
        {
            StringBuffer strbuff = new StringBuffer("this is a string ");
            char[] array = new char[UChar.MaxValue >> 2];
            int strsize = strbuff.Length;
            int arraysize = strsize;

            if (0 != strsize)
            {
                //strbuff.GetChars(0, strsize, array, 0);
                strbuff.CopyTo(0, array, 0, strsize);
            }
            for (int i = 1; i < UChar.MaxValue; i += 100)
            {
                UTF16.Append(strbuff, i);
                arraysize = UTF16.Append(array, arraysize, i);

                String arraystr = new String(array, 0, arraysize);
                if (!arraystr.Equals(strbuff.ToString()))
                {
                    Errln("FAIL Comparing char array append and string append " +
                          "with 0x" + i.ToHexString());
                }

                // this is to cater for the combination of 0xDBXX 0xDC50 which
                // forms a supplementary character
                if (i == 0xDC51)
                {
                    strsize--;
                }

                if (UTF16.CountCodePoint(strbuff) != strsize + (i / 100) + 1)
                {
                    Errln("FAIL Counting code points in string appended with " +
                          " 0x" + (i).ToHexString());
                    break;
                }
            }

            // coverage for new 1.5 - cover only so no real test
            strbuff = new StringBuffer();
            UTF16.AppendCodePoint(strbuff, 0x10000);
            if (strbuff.Length != 2)
            {
                Errln("fail appendCodePoint");
            }
        }

        /**
         * Testing UTF16 class methods bounds
         */
        [Test]
        public void TestBounds()
        {
            StringBuffer strbuff =
          //0     12345     6     7     8     9
          new StringBuffer("\udc000123\ud800\udc00\ud801\udc01\ud802");
            String str = strbuff.ToString();
            char[] array = str.ToCharArray();
            int[] boundtype = {UTF16.SingleCharBoundary,
               UTF16.SingleCharBoundary,
               UTF16.SingleCharBoundary,
               UTF16.SingleCharBoundary,
               UTF16.SingleCharBoundary,
               UTF16.LeadSurrogateBoundary,
               UTF16.TrailSurrogateBoundary,
               UTF16.LeadSurrogateBoundary,
               UTF16.TrailSurrogateBoundary,
               UTF16.SingleCharBoundary};
            int length = str.Length;
            for (int i = 0; i < length; i++)
            {
                if (UTF16.Bounds(str, i) != boundtype[i])
                {
                    Errln("FAIL checking bound type at index " + i);
                }
                if (UTF16.Bounds(strbuff, i) != boundtype[i])
                {
                    Errln("FAIL checking bound type at index " + i);
                }
                if (UTF16.Bounds(array, 0, length, i) != boundtype[i])
                {
                    Errln("FAIL checking bound type at index " + i);
                }
            }
            // does not straddle between supplementary character
            int start = 4;
            int limit = 9;
            int[] subboundtype1 = {UTF16.SingleCharBoundary,
                   UTF16.LeadSurrogateBoundary,
                   UTF16.TrailSurrogateBoundary,
                   UTF16.LeadSurrogateBoundary,
                   UTF16.TrailSurrogateBoundary};
            try
            {
                UTF16.Bounds(array, start, limit, -1);
                Errln("FAIL Out of bounds index in bounds should fail");
            }
            catch (Exception e)
            {
                // getting rid of warnings
                Console.Out.Write("");
            }

            for (int i = 0; i < limit - start; i++)
            {
                if (UTF16.Bounds(array, start, limit, i) != subboundtype1[i])
                {
                    Errln("FAILED Subarray bounds in [" + start + ", " + limit +
                "] expected " + subboundtype1[i] + " at offset " + i);
                }
            }

            // starts from the mid of a supplementary character
            int[] subboundtype2 = {UTF16.SingleCharBoundary,
                   UTF16.LeadSurrogateBoundary,
                   UTF16.TrailSurrogateBoundary};

            start = 6;
            limit = 9;
            for (int i = 0; i < limit - start; i++)
            {
                if (UTF16.Bounds(array, start, limit, i) != subboundtype2[i])
                {
                    Errln("FAILED Subarray bounds in [" + start + ", " + limit +
                "] expected " + subboundtype2[i] + " at offset " + i);
                }
            }

            // ends in the mid of a supplementary character
            int[] subboundtype3 = {UTF16.LeadSurrogateBoundary,
                   UTF16.TrailSurrogateBoundary,
                   UTF16.SingleCharBoundary};
            start = 5;
            limit = 8;
            for (int i = 0; i < limit - start; i++)
            {
                if (UTF16.Bounds(array, start, limit, i) != subboundtype3[i])
                {
                    Errln("FAILED Subarray bounds in [" + start + ", " + limit +
                "] expected " + subboundtype3[i] + " at offset " + i);
                }
            }
        }

        /**
         * Testing UTF16 class methods charAt and charAtCodePoint
         */
        [Test]
        public void TestCharAt()
        {
            using ValueStringBuilder strbuff =
                new ValueStringBuilder(stackalloc char[32]);
            strbuff.Append("12345\ud800\udc0167890\ud800\udc02");
            if (UTF16.CharAt(strbuff.AsSpan(), 0) != '1' || UTF16.CharAt(strbuff.AsSpan(), 2) != '3'
                || UTF16.CharAt(strbuff.AsSpan(), 5) != 0x10001 ||
                  UTF16.CharAt(strbuff.AsSpan(), 6) != 0x10001 ||
                  UTF16.CharAt(strbuff.AsSpan(), 12) != 0x10002 ||
                  UTF16.CharAt(strbuff.AsSpan(), 13) != 0x10002)
            {
                Errln("FAIL Getting character from string buffer error");
            }
            String str = strbuff.ToString();
            if (UTF16.CharAt(str, 0) != '1' || UTF16.CharAt(str, 2) != '3' ||
                UTF16.CharAt(str, 5) != 0x10001 || UTF16.CharAt(str, 6) != 0x10001
                || UTF16.CharAt(str, 12) != 0x10002 ||
                UTF16.CharAt(str, 13) != 0x10002)
            {
                Errln("FAIL Getting character from string error");
            }
            char[] array = str.ToCharArray();
            int start = 0;
            int limit = str.Length;
            if (UTF16.CharAt(array, start, limit, 0) != '1' ||
                UTF16.CharAt(array, start, limit, 2) != '3' ||
                UTF16.CharAt(array, start, limit, 5) != 0x10001 ||
                UTF16.CharAt(array, start, limit, 6) != 0x10001 ||
                UTF16.CharAt(array, start, limit, 12) != 0x10002 ||
                UTF16.CharAt(array, start, limit, 13) != 0x10002)
            {
                Errln("FAIL Getting character from array error");
            }
            // check the sub array here.
            start = 6;
            limit = 13;
            try
            {
                UTF16.CharAt(array, start, limit, -1);
                Errln("FAIL out of bounds error expected");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            try
            {
                UTF16.CharAt(array, start, limit, 8);
                Errln("FAIL out of bounds error expected");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            if (UTF16.CharAt(array, start, limit, 0) != 0xdc01)
            {
                Errln("FAIL Expected result in subarray 0xdc01");
            }
            if (UTF16.CharAt(array, start, limit, 6) != 0xd800)
            {
                Errln("FAIL Expected result in subarray 0xd800");
            }
            ReplaceableString replaceable = new ReplaceableString(str);
            if (UTF16.CharAt(replaceable, 0) != '1' ||
                UTF16.CharAt(replaceable, 2) != '3' ||
                UTF16.CharAt(replaceable, 5) != 0x10001 ||
                UTF16.CharAt(replaceable, 6) != 0x10001 ||
                UTF16.CharAt(replaceable, 12) != 0x10002 ||
                UTF16.CharAt(replaceable, 13) != 0x10002)
            {
                Errln("FAIL Getting character from replaceable error");
            }

            StringBuffer strbuffer = new StringBuffer("0xD805");
            UTF16.CharAt(strbuffer.AsCharSequence(), 0);
        }

        /**
         * Testing UTF16 class methods countCodePoint
         */
        [Test]
        public void TestCountCodePoint()
        {
            StringBuffer strbuff = new StringBuffer("");
            char[] array = null;
            if (UTF16.CountCodePoint(strbuff) != 0 ||
            UTF16.CountCodePoint("") != 0 ||
            UTF16.CountCodePoint(array, 0, 0) != 0)
            {
                Errln("FAIL Counting code points for empty strings");
            }

            strbuff = new StringBuffer("this is a string ");
            String str = strbuff.ToString();
            array = str.ToCharArray();
            int size = str.Length;

            if (UTF16.CountCodePoint(array, 0, 0) != 0)
            {
                Errln("FAIL Counting code points for 0 offset array");
            }

            if (UTF16.CountCodePoint(str) != size ||
            UTF16.CountCodePoint(strbuff) != size ||
            UTF16.CountCodePoint(array, 0, size) != size)
            {
                Errln("FAIL Counting code points");
            }

            UTF16.Append(strbuff, 0x10000);
            str = strbuff.ToString();
            array = str.ToCharArray();
            if (UTF16.CountCodePoint(str) != size + 1 ||
            UTF16.CountCodePoint(strbuff) != size + 1 ||
            UTF16.CountCodePoint(array, 0, size + 1) != size + 1 ||
            UTF16.CountCodePoint(array, 0, size + 2) != size + 1)
            {
                Errln("FAIL Counting code points");
            }
            UTF16.Append(strbuff, 0x61);
            str = strbuff.ToString();
            array = str.ToCharArray();
            if (UTF16.CountCodePoint(str) != size + 2 ||
            UTF16.CountCodePoint(strbuff) != size + 2 ||
            UTF16.CountCodePoint(array, 0, size + 1) != size + 1 ||
            UTF16.CountCodePoint(array, 0, size + 2) != size + 1 ||
            UTF16.CountCodePoint(array, 0, size + 3) != size + 2)
            {
                Errln("FAIL Counting code points");
            }
        }

        /**
         * Testing UTF16 class methods delete
         */
        [Test]
        public void TestDelete()
        {                                        //01234567890123456
            StringBuffer strbuff = new StringBuffer("these are strings");
            int size = strbuff.Length;
            char[] array = strbuff.ToString().ToCharArray();

            UTF16.Delete(strbuff, 3);
            UTF16.Delete(strbuff, 3);
            UTF16.Delete(strbuff, 3);
            UTF16.Delete(strbuff, 3);
            UTF16.Delete(strbuff, 3);
            UTF16.Delete(strbuff, 3);
            try
            {
                UTF16.Delete(strbuff, strbuff.Length);
                Errln("FAIL deleting out of bounds character should fail");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            UTF16.Delete(strbuff, strbuff.Length - 1);
            if (!strbuff.ToString().Equals("the string"))
            {
                Errln("FAIL expected result after deleting characters is " +
                  "\"the string\"");
            }

            size = UTF16.Delete(array, size, 3);
            size = UTF16.Delete(array, size, 3);
            size = UTF16.Delete(array, size, 3);
            size = UTF16.Delete(array, size, 3);
            size = UTF16.Delete(array, size, 3);
            size = UTF16.Delete(array, size, 3);
            try
            {
                UTF16.Delete(array, size, size);
                Errln("FAIL deleting out of bounds character should fail");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            size = UTF16.Delete(array, size, size - 1);
            String str = new String(array, 0, size);
            if (!str.Equals("the string"))
            {
                Errln("FAIL expected result after deleting characters is " +
                  "\"the string\"");
            }
            //012345678     9     01     2      3     4
            strbuff = new StringBuffer("string: \ud800\udc00 \ud801\udc01 \ud801\udc01");
            size = strbuff.Length;
            array = strbuff.ToString().ToCharArray();

            UTF16.Delete(strbuff, 8);
            UTF16.Delete(strbuff, 8);
            UTF16.Delete(strbuff, 9);
            UTF16.Delete(strbuff, 8);
            UTF16.Delete(strbuff, 9);
            UTF16.Delete(strbuff, 6);
            UTF16.Delete(strbuff, 6);
            if (!strbuff.ToString().Equals("string"))
            {
                Errln("FAIL expected result after deleting characters is \"string\"");
            }

            size = UTF16.Delete(array, size, 8);
            size = UTF16.Delete(array, size, 8);
            size = UTF16.Delete(array, size, 9);
            size = UTF16.Delete(array, size, 8);
            size = UTF16.Delete(array, size, 9);
            size = UTF16.Delete(array, size, 6);
            size = UTF16.Delete(array, size, 6);
            str = new String(array, 0, size);
            if (!str.Equals("string"))
            {
                Errln("FAIL expected result after deleting characters is \"string\"");
            }
        }

        /**
         * Testing findOffsetFromCodePoint and findCodePointOffset
         */
        [Test]
        public void TestfindOffset()
        {
            // jitterbug 47
            String str = "a\uD800\uDC00b";
            StringBuffer strbuff = new StringBuffer(str);
            char[] array = str.ToCharArray();
            int limit = str.Length;
            if (UTF16.FindCodePointOffset(str, 0) != 0 ||
            UTF16.FindOffsetFromCodePoint(str, 0) != 0 ||
            UTF16.FindCodePointOffset(strbuff, 0) != 0 ||
            UTF16.FindOffsetFromCodePoint(strbuff, 0) != 0 ||
            UTF16.FindCodePointOffset(array, 0, limit, 0) != 0 ||
            UTF16.FindOffsetFromCodePoint(array, 0, limit, 0) != 0)
            {
                Errln("FAIL Getting the first codepoint offset to a string with " +
                  "supplementary characters");
            }
            if (UTF16.FindCodePointOffset(str, 1) != 1 ||
            UTF16.FindOffsetFromCodePoint(str, 1) != 1 ||
            UTF16.FindCodePointOffset(strbuff, 1) != 1 ||
            UTF16.FindOffsetFromCodePoint(strbuff, 1) != 1 ||
            UTF16.FindCodePointOffset(array, 0, limit, 1) != 1 ||
            UTF16.FindOffsetFromCodePoint(array, 0, limit, 1) != 1)
            {
                Errln("FAIL Getting the second codepoint offset to a string with " +
                  "supplementary characters");
            }
            if (UTF16.FindCodePointOffset(str, 2) != 1 ||
            UTF16.FindOffsetFromCodePoint(str, 2) != 3 ||
            UTF16.FindCodePointOffset(strbuff, 2) != 1 ||
            UTF16.FindOffsetFromCodePoint(strbuff, 2) != 3 ||
            UTF16.FindCodePointOffset(array, 0, limit, 2) != 1 ||
            UTF16.FindOffsetFromCodePoint(array, 0, limit, 2) != 3)
            {
                Errln("FAIL Getting the third codepoint offset to a string with " +
                  "supplementary characters");
            }
            if (UTF16.FindCodePointOffset(str, 3) != 2 ||
            UTF16.FindOffsetFromCodePoint(str, 3) != 4 ||
            UTF16.FindCodePointOffset(strbuff, 3) != 2 ||
            UTF16.FindOffsetFromCodePoint(strbuff, 3) != 4 ||
            UTF16.FindCodePointOffset(array, 0, limit, 3) != 2 ||
            UTF16.FindOffsetFromCodePoint(array, 0, limit, 3) != 4)
            {
                Errln("FAIL Getting the last codepoint offset to a string with " +
                  "supplementary characters");
            }
            if (UTF16.FindCodePointOffset(str, 4) != 3 ||
            UTF16.FindCodePointOffset(strbuff, 4) != 3 ||
            UTF16.FindCodePointOffset(array, 0, limit, 4) != 3)
            {
                Errln("FAIL Getting the length offset to a string with " +
                  "supplementary characters");
            }
            try
            {
                UTF16.FindCodePointOffset(str, 5);
                Errln("FAIL Getting the a non-existence codepoint to a string " +
                  "with supplementary characters");
            }
            catch (Exception e)
            {
                // this is a success
                Logln("Passed out of bounds codepoint offset");
            }
            try
            {
                UTF16.FindOffsetFromCodePoint(str, 4);
                Errln("FAIL Getting the a non-existence codepoint to a string " +
                  "with supplementary characters");
            }
            catch (Exception e)
            {
                // this is a success
                Logln("Passed out of bounds codepoint offset");
            }
            try
            {
                UTF16.FindCodePointOffset(strbuff, 5);
                Errln("FAIL Getting the a non-existence codepoint to a string " +
                  "with supplementary characters");
            }
            catch (Exception e)
            {
                // this is a success
                Logln("Passed out of bounds codepoint offset");
            }
            try
            {
                UTF16.FindOffsetFromCodePoint(strbuff, 4);
                Errln("FAIL Getting the a non-existence codepoint to a string " +
                  "with supplementary characters");
            }
            catch (Exception e)
            {
                // this is a success
                Logln("Passed out of bounds codepoint offset");
            }
            try
            {
                UTF16.FindCodePointOffset(array, 0, limit, 5);
                Errln("FAIL Getting the a non-existence codepoint to a string " +
                  "with supplementary characters");
            }
            catch (Exception e)
            {
                // this is a success
                Logln("Passed out of bounds codepoint offset");
            }
            try
            {
                UTF16.FindOffsetFromCodePoint(array, 0, limit, 4);
                Errln("FAIL Getting the a non-existence codepoint to a string " +
                  "with supplementary characters");
            }
            catch (Exception e)
            {
                // this is a success
                Logln("Passed out of bounds codepoint offset");
            }

            if (UTF16.FindCodePointOffset(array, 1, 3, 0) != 0 ||
            UTF16.FindOffsetFromCodePoint(array, 1, 3, 0) != 0 ||
            UTF16.FindCodePointOffset(array, 1, 3, 1) != 0 ||
            UTF16.FindCodePointOffset(array, 1, 3, 2) != 1 ||
            UTF16.FindOffsetFromCodePoint(array, 1, 3, 1) != 2)
            {
                Errln("FAIL Getting valid codepoint offset in sub array");
            }
        }

        /**
         * Testing UTF16 class methods getCharCount, *Surrogate
         */
        [Test]
        public void TestGetCharCountSurrogate()
        {
            if (UTF16.GetCharCount(0x61) != 1 ||
            UTF16.GetCharCount(0x10000) != 2)
            {
                Errln("FAIL getCharCount result failure");
            }
            if (UTF16.GetLeadSurrogate(0x61) != 0 ||
            UTF16.GetTrailSurrogate(0x61) != 0x61 ||
            UTF16.IsLeadSurrogate((char)0x61) ||
            UTF16.IsTrailSurrogate((char)0x61) ||
            UTF16.GetLeadSurrogate(0x10000) != 0xd800 ||
            UTF16.GetTrailSurrogate(0x10000) != 0xdc00 ||
            UTF16.IsLeadSurrogate((char)0xd800) != true ||
            UTF16.IsTrailSurrogate((char)0xd800) ||
            UTF16.IsLeadSurrogate((char)0xdc00) ||
            UTF16.IsTrailSurrogate((char)0xdc00) != true)
            {
                Errln("FAIL *Surrogate result failure");
            }

            if (UTF16.IsSurrogate((char)0x61) || !UTF16.IsSurrogate((char)0xd800)
                || !UTF16.IsSurrogate((char)0xdc00))
            {
                Errln("FAIL isSurrogate result failure");
            }
        }

        /**
         * Testing UTF16 class method insert
         */
        [Test]
        public void TestInsert()
        {
            StringBuffer strbuff = new StringBuffer("0123456789");
            char[] array = new char[128];
            int srcEnd = strbuff.Length;
            if (0 != srcEnd)
            {
                //strbuff.getChars(0, srcEnd, array, 0);
                strbuff.CopyTo(0, array, 0, srcEnd);
            }
            int length = 10;
            UTF16.Insert(strbuff, 5, 't');
            UTF16.Insert(strbuff, 5, 's');
            UTF16.Insert(strbuff, 5, 'e');
            UTF16.Insert(strbuff, 5, 't');
            if (!(strbuff.ToString().Equals("01234test56789")))
            {
                Errln("FAIL inserting \"test\"");
            }
            length = UTF16.Insert(array, length, 5, 't');
            length = UTF16.Insert(array, length, 5, 's');
            length = UTF16.Insert(array, length, 5, 'e');
            length = UTF16.Insert(array, length, 5, 't');
            String str = new String(array, 0, length);
            if (!(str.Equals("01234test56789")))
            {
                Errln("FAIL inserting \"test\"");
            }
            UTF16.Insert(strbuff, 0, 0x10000);
            UTF16.Insert(strbuff, 11, 0x10000);
            UTF16.Insert(strbuff, strbuff.Length, 0x10000);
            if (!(strbuff.ToString().Equals(
                        "\ud800\udc0001234test\ud800\udc0056789\ud800\udc00")))
            {
                Errln("FAIL inserting supplementary characters");
            }
            length = UTF16.Insert(array, length, 0, 0x10000);
            length = UTF16.Insert(array, length, 11, 0x10000);
            length = UTF16.Insert(array, length, length, 0x10000);
            str = new String(array, 0, length);
            if (!(str.Equals(
                 "\ud800\udc0001234test\ud800\udc0056789\ud800\udc00")))
            {
                Errln("FAIL inserting supplementary characters");
            }

            try
            {
                UTF16.Insert(strbuff, -1, 0);
                Errln("FAIL invalid insertion offset");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            try
            {
                UTF16.Insert(strbuff, 64, 0);
                Errln("FAIL invalid insertion offset");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            try
            {
                UTF16.Insert(array, length, -1, 0);
                Errln("FAIL invalid insertion offset");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            try
            {
                UTF16.Insert(array, length, 64, 0);
                Errln("FAIL invalid insertion offset");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            try
            {
                // exceeded array size
                UTF16.Insert(array, array.Length, 64, 0);
                Errln("FAIL invalid insertion offset");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
        }

        /*
         * Testing moveCodePointOffset APIs
         */

        //
        //   checkMoveCodePointOffset
        //      Run a single test case through each of the moveCodePointOffset() functions.
        //          Parameters -
        //              s               The string to work in.
        //              startIdx        The starting position within the string.
        //              amount          The number of code points to move.
        //              expectedResult  The string index after the move, or -1 if the
        //                              function should throw an exception.
        private void CheckMoveCodePointOffset(String s, int startIdx, int amount, int expectedResult)
        {
            // Test with the String flavor of moveCodePointOffset
            try
            {
                int result = UTF16.MoveCodePointOffset(s, startIdx, amount);
                if (result != expectedResult)
                {
                    Errln("FAIL: UTF16.MoveCodePointOffset(String \"" + s + "\", " + startIdx + ", " + amount + ")" +
                            " returned " + result + ", expected result was " +
                            (expectedResult == -1 ? "exception" : expectedResult.ToString(CultureInfo.InvariantCulture)));
                }
            }
            catch (IndexOutOfRangeException e)
            {
                if (expectedResult != -1)
                {
                    Errln("FAIL: UTF16.MoveCodePointOffset(String \"" + s + "\", " + startIdx + ", " + amount + ")" +
                            " returned exception" + ", expected result was " + expectedResult);
                }
            }

            // Test with the StringBuffer flavor of moveCodePointOffset
            StringBuffer sb = new StringBuffer(s);
            try
            {
                int result = UTF16.MoveCodePointOffset(sb, startIdx, amount);
                if (result != expectedResult)
                {
                    Errln("FAIL: UTF16.MoveCodePointOffset(StringBuffer \"" + s + "\", " + startIdx + ", " + amount + ")" +
                            " returned " + result + ", expected result was " +
                            (expectedResult == -1 ? "exception" : expectedResult.ToString(CultureInfo.InvariantCulture)));
                }
            }
            catch (IndexOutOfRangeException e)
            {
                if (expectedResult != -1)
                {
                    Errln("FAIL: UTF16.MoveCodePointOffset(StringBuffer \"" + s + "\", " + startIdx + ", " + amount + ")" +
                            " returned exception" + ", expected result was " + expectedResult);
                }
            }

            // Test with the char[] flavor of moveCodePointOffset
            char[] ca = s.ToCharArray();
            try
            {
                int result = UTF16.MoveCodePointOffset(ca, 0, s.Length, startIdx, amount);
                if (result != expectedResult)
                {
                    Errln("FAIL: UTF16.MoveCodePointOffset(char[] \"" + s + "\", 0, " + s.Length
                            + ", " + startIdx + ", " + amount + ")" +
                            " returned " + result + ", expected result was " +
                            (expectedResult == -1 ? "exception" : expectedResult.ToString(CultureInfo.InvariantCulture)));
                }
            }
            catch (IndexOutOfRangeException e)
            {
                if (expectedResult != -1)
                {
                    Errln("FAIL: UTF16.MoveCodePointOffset(char[] \"" + s + "\", 0, " + s.Length
                            + ", " + startIdx + ", " + amount + ")" +
                            " returned exception" + ", expected result was " + expectedResult);
                }
            }

            // Put the test string into the interior of a char array,
            //   run test on the subsection of the array.
            char[] ca2 = new char[s.Length + 2];
            ca2[0] = (char)0xd800;
            ca2[s.Length + 1] = (char)0xd8ff;
            //s.getChars(0, s.Length, ca2, 1);
            s.CopyTo(0, ca2, 1, s.Length);
            try
            {
                int result = UTF16.MoveCodePointOffset(ca2, 1, s.Length + 1, startIdx, amount);
                if (result != expectedResult)
                {
                    Errln("UTF16.MoveCodePointOffset(char[] \"" + "." + s + ".\", 1, " + (s.Length + 1)
                            + ", " + startIdx + ", " + amount + ")" +
                             " returned " + result + ", expected result was " +
                            (expectedResult == -1 ? "exception" : expectedResult.ToString(CultureInfo.InvariantCulture)));
                }
            }
            catch (IndexOutOfRangeException e)
            {
                if (expectedResult != -1)
                {
                    Errln("UTF16.MoveCodePointOffset(char[] \"" + "." + s + ".\", 1, " + (s.Length + 1)
                            + ", " + startIdx + ", " + amount + ")" +
                            " returned exception" + ", expected result was " + expectedResult);
                }
            }

        }


        [Test]
        public void TestMoveCodePointOffset()
        {
            // checkMoveCodePointOffset(String, startIndex, amount, expected );  expected=-1 for exception.

            // No Supplementary chars
            CheckMoveCodePointOffset("abc", 1, 1, 2);
            CheckMoveCodePointOffset("abc", 1, -1, 0);
            CheckMoveCodePointOffset("abc", 1, -2, -1);
            CheckMoveCodePointOffset("abc", 1, 2, 3);
            CheckMoveCodePointOffset("abc", 1, 3, -1);
            CheckMoveCodePointOffset("abc", 1, 0, 1);

            CheckMoveCodePointOffset("abc", 3, 0, 3);
            CheckMoveCodePointOffset("abc", 4, 0, -1);
            CheckMoveCodePointOffset("abc", 0, 0, 0);
            CheckMoveCodePointOffset("abc", -1, 0, -1);

            CheckMoveCodePointOffset("", 0, 0, 0);
            CheckMoveCodePointOffset("", 0, -1, -1);
            CheckMoveCodePointOffset("", 0, 1, -1);

            CheckMoveCodePointOffset("a", 0, 0, 0);
            CheckMoveCodePointOffset("a", 1, 0, 1);
            CheckMoveCodePointOffset("a", 0, 1, 1);
            CheckMoveCodePointOffset("a", 1, -1, 0);


            // Supplementary in middle of string
            CheckMoveCodePointOffset("a\ud800\udc00b", 0, 1, 1);
            CheckMoveCodePointOffset("a\ud800\udc00b", 0, 2, 3);
            CheckMoveCodePointOffset("a\ud800\udc00b", 0, 3, 4);
            CheckMoveCodePointOffset("a\ud800\udc00b", 0, 4, -1);

            CheckMoveCodePointOffset("a\ud800\udc00b", 4, -1, 3);
            CheckMoveCodePointOffset("a\ud800\udc00b", 4, -2, 1);
            CheckMoveCodePointOffset("a\ud800\udc00b", 4, -3, 0);
            CheckMoveCodePointOffset("a\ud800\udc00b", 4, -4, -1);

            // Supplementary at start of string
            CheckMoveCodePointOffset("\ud800\udc00ab", 0, 1, 2);
            CheckMoveCodePointOffset("\ud800\udc00ab", 1, 1, 2);
            CheckMoveCodePointOffset("\ud800\udc00ab", 2, 1, 3);
            CheckMoveCodePointOffset("\ud800\udc00ab", 2, -1, 0);
            CheckMoveCodePointOffset("\ud800\udc00ab", 1, -1, 0);
            CheckMoveCodePointOffset("\ud800\udc00ab", 0, -1, -1);


            // Supplementary at end of string
            CheckMoveCodePointOffset("ab\ud800\udc00", 1, 1, 2);
            CheckMoveCodePointOffset("ab\ud800\udc00", 2, 1, 4);
            CheckMoveCodePointOffset("ab\ud800\udc00", 3, 1, 4);
            CheckMoveCodePointOffset("ab\ud800\udc00", 4, 1, -1);

            CheckMoveCodePointOffset("ab\ud800\udc00", 5, -2, -1);
            CheckMoveCodePointOffset("ab\ud800\udc00", 4, -1, 2);
            CheckMoveCodePointOffset("ab\ud800\udc00", 3, -1, 2);
            CheckMoveCodePointOffset("ab\ud800\udc00", 2, -1, 1);
            CheckMoveCodePointOffset("ab\ud800\udc00", 1, -1, 0);

            // Unpaired surrogate in middle
            CheckMoveCodePointOffset("a\ud800b", 0, 1, 1);
            CheckMoveCodePointOffset("a\ud800b", 1, 1, 2);
            CheckMoveCodePointOffset("a\ud800b", 2, 1, 3);

            CheckMoveCodePointOffset("a\udc00b", 0, 1, 1);
            CheckMoveCodePointOffset("a\udc00b", 1, 1, 2);
            CheckMoveCodePointOffset("a\udc00b", 2, 1, 3);

            CheckMoveCodePointOffset("a\udc00\ud800b", 0, 1, 1);
            CheckMoveCodePointOffset("a\udc00\ud800b", 1, 1, 2);
            CheckMoveCodePointOffset("a\udc00\ud800b", 2, 1, 3);
            CheckMoveCodePointOffset("a\udc00\ud800b", 3, 1, 4);

            CheckMoveCodePointOffset("a\ud800b", 1, -1, 0);
            CheckMoveCodePointOffset("a\ud800b", 2, -1, 1);
            CheckMoveCodePointOffset("a\ud800b", 3, -1, 2);

            CheckMoveCodePointOffset("a\udc00b", 1, -1, 0);
            CheckMoveCodePointOffset("a\udc00b", 2, -1, 1);
            CheckMoveCodePointOffset("a\udc00b", 3, -1, 2);

            CheckMoveCodePointOffset("a\udc00\ud800b", 1, -1, 0);
            CheckMoveCodePointOffset("a\udc00\ud800b", 2, -1, 1);
            CheckMoveCodePointOffset("a\udc00\ud800b", 3, -1, 2);
            CheckMoveCodePointOffset("a\udc00\ud800b", 4, -1, 3);

            // Unpaired surrogate at start
            CheckMoveCodePointOffset("\udc00ab", 0, 1, 1);
            CheckMoveCodePointOffset("\ud800ab", 0, 2, 2);
            CheckMoveCodePointOffset("\ud800\ud800ab", 0, 3, 3);
            CheckMoveCodePointOffset("\udc00\udc00ab", 0, 4, 4);

            CheckMoveCodePointOffset("\udc00ab", 2, -1, 1);
            CheckMoveCodePointOffset("\ud800ab", 1, -1, 0);
            CheckMoveCodePointOffset("\ud800ab", 1, -2, -1);
            CheckMoveCodePointOffset("\ud800\ud800ab", 2, -1, 1);
            CheckMoveCodePointOffset("\udc00\udc00ab", 2, -2, 0);
            CheckMoveCodePointOffset("\udc00\udc00ab", 2, -3, -1);

            // Unpaired surrogate at end
            CheckMoveCodePointOffset("ab\udc00\udc00ab", 3, 1, 4);
            CheckMoveCodePointOffset("ab\udc00\udc00ab", 2, 1, 3);
            CheckMoveCodePointOffset("ab\udc00\udc00ab", 1, 1, 2);

            CheckMoveCodePointOffset("ab\udc00\udc00ab", 4, -1, 3);
            CheckMoveCodePointOffset("ab\udc00\udc00ab", 3, -1, 2);
            CheckMoveCodePointOffset("ab\udc00\udc00ab", 2, -1, 1);


            //01234567890     1     2     3     45678901234
            String str = "0123456789\ud800\udc00\ud801\udc010123456789";
            int[] move1 = { 1,  2,  3,  4,  5,  6,  7,  8,  9, 10,
                       12, 12, 14, 14, 15, 16, 17, 18, 19, 20,
                       21, 22, 23, 24};
            int[] move2 = { 2,  3,  4,  5,  6,  7,  8,  9, 10, 12,
                       14, 14, 15, 15, 16, 17, 18, 19, 20, 21,
                       22, 23, 24, -1};
            int[] move3 = { 3,  4,  5,  6,  7,  8,  9, 10, 12, 14,
                       15, 15, 16, 16, 17, 18, 19, 20, 21, 22,
                       23, 24, -1, -1};
            int size = str.Length;
            for (int i = 0; i < size; i++)
            {
                CheckMoveCodePointOffset(str, i, 1, move1[i]);
                CheckMoveCodePointOffset(str, i, 2, move2[i]);
                CheckMoveCodePointOffset(str, i, 3, move3[i]);
            }

            char[] strarray = str.ToCharArray();
            if (UTF16.MoveCodePointOffset(strarray, 9, 13, 0, 2) != 3)
            {
                Errln("FAIL: Moving offset 0 by 2 codepoint in subarray [9, 13] " +
                "expected result 3");
            }
            if (UTF16.MoveCodePointOffset(strarray, 9, 13, 1, 2) != 4)
            {
                Errln("FAIL: Moving offset 1 by 2 codepoint in subarray [9, 13] " +
                "expected result 4");
            }
            if (UTF16.MoveCodePointOffset(strarray, 11, 14, 0, 2) != 3)
            {
                Errln("FAIL: Moving offset 0 by 2 codepoint in subarray [11, 14] "
                        + "expected result 3");
            }
        }

        /**
         * Testing UTF16 class methods setCharAt
         */
        [Test]
        public void TestSetCharAt()
        {
            StringBuffer strbuff = new StringBuffer("012345");
            char[] array = new char[128];
            int srcEnd = strbuff.Length;
            if (0 != srcEnd)
            {
                //strbuff.getChars(0, srcEnd, array, 0);
                strbuff.CopyTo(0, array, 0, srcEnd);
            }
            int length = 6;
            for (int i = 0; i < length; i++)
            {
                UTF16.SetCharAt(strbuff, i, '0');
                UTF16.SetCharAt(array, length, i, '0');
            }
            String str = new String(array, 0, length);
            if (!(strbuff.ToString().Equals("000000")) ||
            !(str.Equals("000000")))
            {
                Errln("FAIL: setChar to '0' failed");
            }
            UTF16.SetCharAt(strbuff, 0, 0x10000);
            UTF16.SetCharAt(strbuff, 4, 0x10000);
            UTF16.SetCharAt(strbuff, 7, 0x10000);
            if (!(strbuff.ToString().Equals(
                        "\ud800\udc0000\ud800\udc000\ud800\udc00")))
            {
                Errln("FAIL: setChar to 0x10000 failed");
            }
            length = UTF16.SetCharAt(array, length, 0, 0x10000);
            length = UTF16.SetCharAt(array, length, 4, 0x10000);
            length = UTF16.SetCharAt(array, length, 7, 0x10000);
            str = new String(array, 0, length);
            if (!(str.Equals("\ud800\udc0000\ud800\udc000\ud800\udc00")))
            {
                Errln("FAIL: setChar to 0x10000 failed");
            }
            UTF16.SetCharAt(strbuff, 0, '0');
            UTF16.SetCharAt(strbuff, 1, '1');
            UTF16.SetCharAt(strbuff, 2, '2');
            UTF16.SetCharAt(strbuff, 4, '3');
            UTF16.SetCharAt(strbuff, 4, '4');
            UTF16.SetCharAt(strbuff, 5, '5');
            if (!strbuff.ToString().Equals("012345"))
            {
                Errln("Fail converting supplementaries in StringBuffer to BMP " +
                  "characters");
            }
            length = UTF16.SetCharAt(array, length, 0, '0');
            length = UTF16.SetCharAt(array, length, 1, '1');
            length = UTF16.SetCharAt(array, length, 2, '2');
            length = UTF16.SetCharAt(array, length, 4, '3');
            length = UTF16.SetCharAt(array, length, 4, '4');
            length = UTF16.SetCharAt(array, length, 5, '5');
            str = new String(array, 0, length);
            if (!str.Equals("012345"))
            {
                Errln("Fail converting supplementaries in array to BMP " +
                  "characters");
            }
            try
            {
                UTF16.SetCharAt(strbuff, -1, 0);
                Errln("FAIL: setting character at invalid offset");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            try
            {
                UTF16.SetCharAt(array, length, -1, 0);
                Errln("FAIL: setting character at invalid offset");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            try
            {
                UTF16.SetCharAt(strbuff, length, 0);
                Errln("FAIL: setting character at invalid offset");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            try
            {
                UTF16.SetCharAt(array, length, length, 0);
                Errln("FAIL: setting character at invalid offset");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
        }

        /**
         * Testing UTF16 valueof APIs
         */
        [Test]
        public void TestValueOf()
        {
            if (UChar.ConvertToUtf32('\ud800', '\udc00') != 0x10000)
            {
                Errln("FAIL: getCodePoint('\ud800','\udc00')");
            }
            if (!UTF16.ValueOf(0x61).Equals("a") ||
            !UTF16.ValueOf(0x10000).Equals("\ud800\udc00"))
            {
                Errln("FAIL: valueof(char32)");
            }
            String str = "01234\ud800\udc0056789";
            StringBuffer strbuff = new StringBuffer(str);
            char[] array = str.ToCharArray();
            int length = str.Length;

            String[] expected = {"0", "1", "2", "3", "4", "\ud800\udc00",
                 "\ud800\udc00", "5", "6", "7", "8", "9"};
            for (int i = 0; i < length; i++)
            {
                if (!UTF16.ValueOf(str, i).Equals(expected[i]) ||
                        !UTF16.ValueOf(strbuff, i).Equals(expected[i]) ||
                        !UTF16.ValueOf(array, 0, length, i).Equals(expected[i]))
                {
                    Errln("FAIL: valueOf() expected " + expected[i]);
                }
            }
            try
            {
                UTF16.ValueOf(str, -1);
                Errln("FAIL: out of bounds error expected");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            try
            {
                UTF16.ValueOf(strbuff, -1);
                Errln("FAIL: out of bounds error expected");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            try
            {
                UTF16.ValueOf(array, 0, length, -1);
                Errln("FAIL: out of bounds error expected");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            try
            {
                UTF16.ValueOf(str, length);
                Errln("FAIL: out of bounds error expected");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            try
            {
                UTF16.ValueOf(strbuff, length);
                Errln("FAIL: out of bounds error expected");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            try
            {
                UTF16.ValueOf(array, 0, length, length);
                Errln("FAIL: out of bounds error expected");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            if (!UTF16.ValueOf(array, 6, length, 0).Equals("\udc00") ||
            !UTF16.ValueOf(array, 0, 6, 5).Equals("\ud800"))
            {
                Errln("FAIL: error getting partial supplementary character");
            }
            try
            {
                UTF16.ValueOf(array, 3, 5, -1);
                Errln("FAIL: out of bounds error expected");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            try
            {
                UTF16.ValueOf(array, 3, 5, 3);
                Errln("FAIL: out of bounds error expected");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
        }

        [Test]
        public void TestValueOf_Int32_Span_Int32()
        {
            Span<char> actual = stackalloc char[2];
            int length;
            length = UTF16.ValueOf(0x61, actual, 0);
            if (!System.MemoryExtensions.Equals(actual.Slice(0, length), "a".AsSpan(), StringComparison.Ordinal))
            { 
                Errln("FAIL: valueof(char32, destination, destinationIndex)");
            }
            length = UTF16.ValueOf(0x10000, actual, 0);
            if (!System.MemoryExtensions.Equals(actual.Slice(0, length), "\ud800\udc00".AsSpan(), StringComparison.Ordinal))
            {
                Errln("FAIL: valueof(char32, destination, destinationIndex)");
            }
            try
            {
                UTF16.ValueOf(0x61, actual, 3);
                Errln("FAIL: out of bounds error expected");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            try
            {
                UTF16.ValueOf(0x61, actual, -1);
                Errln("FAIL: out of bounds error expected");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            try
            {
                UTF16.ValueOf(UTF16.CodePointMinValue - 1, actual, 0);
                Errln("FAIL: out of bounds error expected");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
            try
            {
                UTF16.ValueOf(UTF16.CodePointMaxValue + 1, actual, 0);
                Errln("FAIL: out of bounds error expected");
            }
            catch (Exception e)
            {
                Console.Out.Write("");
            }
        }

        [Test]
        public void TestIndexOf()
        {
            //012345678901234567890123456789012345
            String test1 = "test test ttest tetest testesteststt";
            String test2 = "test";
            int testChar1 = 0x74;
            int testChar2 = 0x20402;
            // int    testChar3 = 0xdc02;
            // int    testChar4 = 0xd841;
            String test3 = "\ud841\udc02\u0071\udc02\ud841\u0071\ud841\udc02\u0071\u0072\ud841\udc02\u0071\ud841\udc02\u0071\udc02\ud841\u0073";
            String test4 = UChar.ConvertFromUtf32(testChar2);

            if (UTF16.IndexOf(test1, test2, StringComparison.Ordinal) != 0 ||
                UTF16.IndexOf(test1, test2, 0, StringComparison.Ordinal) != 0)
            {
                Errln("indexOf failed: expected to find '" + test2 +
                      "' at position 0 in text '" + test1 + "'");
            }
            if (UTF16.IndexOf(test1, testChar1) != 0 ||
                UTF16.IndexOf(test1, testChar1) != 0)
            {
                Errln("indexOf failed: expected to find 0x" +
                      (testChar1).ToHexString() +
                      " at position 0 in text '" + test1 + "'");
            }
            if (UTF16.IndexOf(test3, testChar2) != 0 ||
                UTF16.IndexOf(test3, testChar2, 0) != 0)
            {
                Errln("indexOf failed: expected to find 0x" +
                      (testChar2).ToHexString() +
                      " at position 0 in text '" + Utility.Hex(test3) + "'");
            }
            String test5 = "\ud841\ud841\udc02";
            if (UTF16.IndexOf(test5, testChar2) != 1 ||
                UTF16.IndexOf(test5, testChar2, 0) != 1)
            {
                Errln("indexOf failed: expected to find 0x" +
                      (testChar2).ToHexString() +
                      " at position 0 in text '" + Utility.Hex(test3) + "'");
            }
            if (UTF16.LastIndexOf(test1, test2, StringComparison.Ordinal) != 29 ||
                UTF16.LastIndexOf(test1, test2, test1.Length, StringComparison.Ordinal) != 29)
            {
                Errln("LastIndexOf failed: expected to find '" + test2 +
                      "' at position 29 in text '" + test1 + "'");
            }
            if (UTF16.LastIndexOf(test1, testChar1) != 35 ||
                UTF16.LastIndexOf(test1, testChar1, test1.Length) != 35)
            {
                Errln("LastIndexOf failed: expected to find 0x" +
                      (testChar1).ToHexString() +
                      " at position 35 in text '" + test1 + "'");
            }
            if (UTF16.LastIndexOf(test3, testChar2) != 13 ||
                UTF16.LastIndexOf(test3, testChar2, test3.Length) != 13)
            {
                Errln("LastIndexOf failed: expected to find 0x" +
                      (testChar2).ToHexString() +
                      " at position 13 in text '" + Utility.Hex(test3) + "'");
            }
            // ****************************************************************
            // ICU4N specific - need to test LastIndexOf to ensure an 
            // ArgumentOutofRangeException happens when the startIndex is 
            // negative or > string.Length
            // ****************************************************************

            AssertThrows<ArgumentOutOfRangeException>(() => UTF16.LastIndexOf(test1, test2, test1.Length + 1, StringComparison.Ordinal));
            AssertThrows<ArgumentOutOfRangeException>(() => UTF16.LastIndexOf(test1, testChar1, test1.Length + 1));
            AssertThrows<ArgumentOutOfRangeException>(() => UTF16.LastIndexOf(test3, testChar2, test3.Length + 1));
            AssertThrows<ArgumentOutOfRangeException>(() => UTF16.LastIndexOf(test1, test2, -1, StringComparison.Ordinal));
            AssertThrows<ArgumentOutOfRangeException>(() => UTF16.LastIndexOf(test1, testChar1, -1));
            AssertThrows<ArgumentOutOfRangeException>(() => UTF16.LastIndexOf(test3, testChar2, -1));

            // ****************************************************************
            // ICU4N specific - .NET bug: LastIndexOf overloads that accept
            // char will throw an ArgumentOutofRangeException when source.Length 
            // is passed as startIndex. This ensures that case is correctly handled.
            // ****************************************************************

            AssertDoesNotThrow(() => UTF16.LastIndexOf(test1, testChar1, test1.Length));
            AssertDoesNotThrow(() => UTF16.LastIndexOf(test3, testChar2, test3.Length));

            // ****************************************************************
            // ICU4N specific - need to test SafeLastIndexOf to ensure an 
            // ArgumentOutofRangeException does not happen when the startIndex is 
            // negative or >= string.Length
            // ****************************************************************

            if (UTF16.SafeLastIndexOf(test1, test2, test1.Length) != 29)
            {
                Errln("LastIndexOf failed: expected to find '" + test2 +
                      "' at position 29 in text '" + test1 + "'");
            }
            if (UTF16.SafeLastIndexOf(test1, testChar1, test1.Length) != 35)
            {
                Errln("LastIndexOf failed: expected to find 0x" +
                      (testChar1).ToHexString() +
                      " at position 35 in text '" + test1 + "'");
            }
            if (UTF16.SafeLastIndexOf(test3, testChar2, test3.Length) != 13)
            {
                Errln("LastIndexOf failed: expected to find 0x" +
                      (testChar2).ToHexString() +
                      " at position 13 in text '" + Utility.Hex(test3) + "'");
            }

            // ****************************************************************
            int occurrences = 0;
            for (int startPos = 0; startPos != -1 && startPos < test1.Length;)
            {
                startPos = UTF16.IndexOf(test1, test2, startPos, StringComparison.Ordinal);
                if (startPos >= 0)
                {
                    ++occurrences;
                    startPos += 4;
                }
            }
            if (occurrences != 6)
            {
                Errln("indexOf failed: expected to find 6 occurrences, found "
                      + occurrences);
            }

            occurrences = 0;
            for (int startPos = 10; startPos != -1 && startPos < test1.Length;)
            {
                startPos = UTF16.IndexOf(test1, test2, startPos, StringComparison.Ordinal);
                if (startPos >= 0)
                {
                    ++occurrences;
                    startPos += 4;
                }
            }
            if (occurrences != 4)
            {
                Errln("indexOf with starting offset failed: expected to find 4 occurrences, found "
                      + occurrences);
            }

            occurrences = 0;
            for (int startPos = 0;
             startPos != -1 && startPos < test3.Length;)
            {
                startPos = UTF16.IndexOf(test3, test4, startPos, StringComparison.Ordinal);
                if (startPos != -1)
                {
                    ++occurrences;
                    startPos += 2;
                }
            }
            if (occurrences != 4)
            {
                Errln("indexOf failed: expected to find 4 occurrences, found "
              + occurrences);
            }

            occurrences = 0;
            for (int startPos = 10;
                 startPos != -1 && startPos < test3.Length;)
            {
                startPos = UTF16.IndexOf(test3, test4, startPos, StringComparison.Ordinal);
                if (startPos != -1)
                {
                    ++occurrences;
                    startPos += 2;
                }
            }
            if (occurrences != 2)
            {
                Errln("indexOf failed: expected to find 2 occurrences, found "
                      + occurrences);
            }

            occurrences = 0;
            for (int startPos = 0;
             startPos != -1 && startPos < test1.Length;)
            {
                startPos = UTF16.IndexOf(test1, testChar1, startPos);
                if (startPos != -1)
                {
                    ++occurrences;
                    startPos += 1;
                }
            }
            if (occurrences != 16)
            {
                Errln("indexOf with character failed: expected to find 16 occurrences, found "
                      + occurrences);
            }

            occurrences = 0;
            for (int startPos = 10;
             startPos != -1 && startPos < test1.Length;)
            {
                startPos = UTF16.IndexOf(test1, testChar1, startPos);
                if (startPos != -1)
                {
                    ++occurrences;
                    startPos += 1;
                }
            }
            if (occurrences != 12)
            {
                Errln("indexOf with character & start offset failed: expected to find 12 occurrences, found "
              + occurrences);
            }

            occurrences = 0;
            for (int startPos = 0;
             startPos != -1 && startPos < test3.Length;)
            {
                startPos = UTF16.IndexOf(test3, testChar2, startPos);
                if (startPos != -1)
                {
                    ++occurrences;
                    startPos += 1;
                }
            }
            if (occurrences != 4)
            {
                Errln("indexOf failed: expected to find 4 occurrences, found "
                      + occurrences);
            }

            occurrences = 0;
            for (int startPos = 5; startPos != -1 && startPos < test3.Length;)
            {
                startPos = UTF16.IndexOf(test3, testChar2, startPos);
                if (startPos != -1)
                {
                    ++occurrences;
                    startPos += 1;
                }
            }
            if (occurrences != 3)
            {
                Errln("indexOf with character & start & end offsets failed: expected to find 2 occurrences, found "
              + occurrences);
            }

            // ****************************************************************
            // ICU4N specific - behavior of LastIndexOf in .NET differs from Java.
            // In Java, the last index looks for the beginning of the matching string
            // EVEN IF the end of the string goes past the search position.
            // In .NET, the entire match string must be between 0 and startIndex
            // for there to be a match. We want this method to behave like it
            // does in .NET, and it is not used elsewhere in ICU, so the test
            // was changed to match the different behavior.
            // ****************************************************************

            occurrences = 0;
            for (int startPos = 32; startPos != -1;)
            {
                startPos = UTF16.LastIndexOf(test1, test2, startPos, StringComparison.Ordinal);
                if (startPos != -1)
                {
                    ++occurrences;
                    startPos -= 5;
                }
            }
            if (occurrences != 3) // ICU4N: 3 results rather than 6
            {
                Errln("lastIndexOf with starting and ending offsets failed: expected to find 3 occurrences, found "
                      + occurrences);
            }
            occurrences = 0;
            for (int startPos = 32; startPos > -1;) // ICU4N: ensure we exit if we go below 0
            {
                startPos = UTF16.LastIndexOf(test1, testChar1, startPos);
                if (startPos != -1)
                {
                    ++occurrences;
                    startPos -= 5;
                }
            }
            if (occurrences != 7)
            {
                Errln("lastIndexOf with character & start & end offsets failed: expected to find 7 occurrences, found "
              + occurrences);
            }

            //testing UChar32
            occurrences = 0;
            for (int startPos = test3.Length; startPos > -1;) // ICU4N: ensure we exit if we go below 0
            {
                startPos = UTF16.LastIndexOf(test3, testChar2, Math.Max(startPos - 5, 0)); // ICU4N: ensure we don't pass less than 0 for startIndex
                if (startPos != -1)
                {
                    ++occurrences;
                }
            }
            if (occurrences != 3)
            {
                Errln("lastIndexOf with character & start & end offsets failed: expected to find 3 occurrences, found "
              + occurrences);
            }

            // testing supplementary
            for (int i = 0; i < INDEXOF_SUPPLEMENTARY_CHAR_.Length; i++)
            {
                int ch = INDEXOF_SUPPLEMENTARY_CHAR_[i];
                for (int j = 0; j < INDEXOF_SUPPLEMENTARY_CHAR_INDEX_[i].Length;
                 j++)
                {
                    int index = 0;
                    int expected = INDEXOF_SUPPLEMENTARY_CHAR_INDEX_[i][j];
                    if (j > 0)
                    {
                        index = INDEXOF_SUPPLEMENTARY_CHAR_INDEX_[i][j - 1] + 1;
                    }
                    if (UTF16.IndexOf(INDEXOF_SUPPLEMENTARY_STRING_, ch, index) !=
                        expected ||
                        UTF16.IndexOf(INDEXOF_SUPPLEMENTARY_STRING_,
                              UChar.ConvertFromUtf32(ch), index, StringComparison.Ordinal) !=
                        expected)
                    {
                        Errln("Failed finding index for supplementary 0x" +
                          (ch).ToHexString());
                    }
                    index = INDEXOF_SUPPLEMENTARY_STRING_.Length;
                    if (j < INDEXOF_SUPPLEMENTARY_CHAR_INDEX_[i].Length - 1)
                    {
                        index = INDEXOF_SUPPLEMENTARY_CHAR_INDEX_[i][j + 1] - 1;
                    }
                    if (UTF16.LastIndexOf(INDEXOF_SUPPLEMENTARY_STRING_, ch,
                                  index) != expected ||
                        UTF16.LastIndexOf(INDEXOF_SUPPLEMENTARY_STRING_,
                                  UChar.ConvertFromUtf32(ch), index, StringComparison.Ordinal)
                        != expected)
                    {
                        Errln("Failed finding last index for supplementary 0x" +
                              (ch).ToHexString());
                    }
                }
            }

            for (int i = 0; i < INDEXOF_SUPPLEMENTARY_STR_INDEX_.Length; i++)
            {
                int index = 0;
                int expected = INDEXOF_SUPPLEMENTARY_STR_INDEX_[i];
                if (i > 0)
                {
                    index = INDEXOF_SUPPLEMENTARY_STR_INDEX_[i - 1] + 1;
                }
                if (UTF16.IndexOf(INDEXOF_SUPPLEMENTARY_STRING_,
                          INDEXOF_SUPPLEMENTARY_STR_, index, StringComparison.Ordinal) != expected)
                {
                    Errln("Failed finding index for supplementary string " +
                          Hex(INDEXOF_SUPPLEMENTARY_STRING_));
                }
                index = INDEXOF_SUPPLEMENTARY_STRING_.Length;
                if (i < INDEXOF_SUPPLEMENTARY_STR_INDEX_.Length - 1)
                {
                    index = INDEXOF_SUPPLEMENTARY_STR_INDEX_[i + 1] - 1;
                }
                if (UTF16.LastIndexOf(INDEXOF_SUPPLEMENTARY_STRING_,
                                      INDEXOF_SUPPLEMENTARY_STR_, index, StringComparison.Ordinal) != expected)
                {
                    Errln("Failed finding last index for supplementary string " +
                          Hex(INDEXOF_SUPPLEMENTARY_STRING_));
                }
            }
        }

        [Test]
        public void TestReplace()
        {
            String test1 = "One potato, two potato, three potato, four\n";
            String test2 = "potato";
            String test3 = "MISSISSIPPI";

            String result = UTF16.Replace(test1, test2, test3);
            String expectedValue =
                "One MISSISSIPPI, two MISSISSIPPI, three MISSISSIPPI, four\n";
            if (!result.Equals(expectedValue))
            {
                Errln("findAndReplace failed: expected \"" + expectedValue +
                      "\", got \"" + test1 + "\".");
            }
            result = UTF16.Replace(test1, test3, test2);
            expectedValue = test1;
            if (!result.Equals(expectedValue))
            {
                Errln("findAndReplace failed: expected \"" + expectedValue +
                      "\", got \"" + test1 + "\".");
            }

            result = UTF16.Replace(test1, ',', 'e');
            expectedValue = "One potatoe two potatoe three potatoe four\n";
            if (!result.Equals(expectedValue))
            {
                Errln("findAndReplace failed: expected \"" + expectedValue +
                      "\", got \"" + test1 + "\".");
            }

            result = UTF16.Replace(test1, ',', 0x10000);
            expectedValue = "One potato\ud800\udc00 two potato\ud800\udc00 three potato\ud800\udc00 four\n";
            if (!result.Equals(expectedValue))
            {
                Errln("findAndReplace failed: expected \"" + expectedValue +
                      "\", got \"" + test1 + "\".");
            }

            result = UTF16.Replace(test1, "potato", "\ud800\udc00\ud801\udc01");
            expectedValue = "One \ud800\udc00\ud801\udc01, two \ud800\udc00\ud801\udc01, three \ud800\udc00\ud801\udc01, four\n";
            if (!result.Equals(expectedValue))
            {
                Errln("findAndReplace failed: expected \"" + expectedValue +
                      "\", got \"" + test1 + "\".");
            }

            String test4 = "\ud800\ud800\udc00\ud800\udc00\udc00\ud800\ud800\udc00\ud800\udc00\udc00";
            result = UTF16.Replace(test4, 0xd800, 'A');
            expectedValue = "A\ud800\udc00\ud800\udc00\udc00A\ud800\udc00\ud800\udc00\udc00";
            if (!result.Equals(expectedValue))
            {
                Errln("findAndReplace failed: expected \"" + expectedValue +
                      "\", got \"" + test1 + "\".");
            }

            result = UTF16.Replace(test4, 0xdC00, 'A');
            expectedValue = "\ud800\ud800\udc00\ud800\udc00A\ud800\ud800\udc00\ud800\udc00A";
            if (!result.Equals(expectedValue))
            {
                Errln("findAndReplace failed: expected \"" + expectedValue +
                      "\", got \"" + test1 + "\".");
            }

            result = UTF16.Replace(test4, 0x10000, 'A');
            expectedValue = "\ud800AA\udc00\ud800AA\udc00";
            if (!result.Equals(expectedValue))
            {
                Errln("findAndReplace failed: expected \"" + expectedValue +
                      "\", got \"" + test1 + "\".");
            }
        }

        [Test]
        public void TestReverse() // ICU4N TODO: API - Reversing a StringBuilder is nonsensical in .NET, so we can eliminate this API. Better to reverse ValueStringBuilder/OpenStringBuilder/Span<char> instead.
        {
            StringBuffer test = new StringBuffer(
                             "backwards words say to used I");

            StringBuffer result = UTF16.Reverse(test);
            if (!result.ToString().Equals("I desu ot yas sdrow sdrawkcab"))
            {
                Errln("reverse() failed:  Expected \"I desu ot yas sdrow sdrawkcab\",\n got \""
              + result + "\"");
            }
            StringBuffer testbuffer = new StringBuffer();
            UTF16.Append(testbuffer, 0x2f999);
            UTF16.Append(testbuffer, 0x1d15f);
            UTF16.Append(testbuffer, 0x00c4);
            UTF16.Append(testbuffer, 0x1ed0);
            result = UTF16.Reverse(testbuffer);
            string resultString = result.ToString();
            if (result[0] != 0x1ed0 ||
                result[1] != 0xc4 ||
                UTF16.CharAt(resultString, 2) != 0x1d15f ||
                UTF16.CharAt(resultString, 4) != 0x2f999)
            {
                Errln("reverse() failed with supplementary characters");
            }
        }

        /**
         * Testing the setter and getter apis for StringComparator
         */
        [Test]
        public void TestStringComparator()
        {
            UTF16.StringComparer compare = new UTF16.StringComparer();
            if (compare.CodePointCompare != false)
            {
                Errln("Default string comparator should be code unit compare");
            }
            if (compare.IgnoreCase != false)
            {
                Errln("Default string comparator should be case sensitive compare");
            }
            if (compare.IgnoreCaseOption
                != UTF16.StringComparer.FoldCaseDefault)
            {
                Errln("Default string comparator should have fold case default compare");
            }

            compare.CodePointCompare = true;
            if (compare.CodePointCompare != true)
            {
                Errln("Error setting code point compare");
            }
            compare.CodePointCompare = false;
            if (compare.CodePointCompare != false)
            {
                Errln("Error setting code point compare");
            }
            compare.IgnoreCase = true;
            compare.IgnoreCaseOption = UTF16.StringComparer.FoldCaseDefault;
            if (compare.IgnoreCase != true
                || compare.IgnoreCaseOption
            != UTF16.StringComparer.FoldCaseDefault)
            {
                Errln("Error setting ignore case and options");
            }

            compare.IgnoreCase = false;
            compare.IgnoreCaseOption = UTF16.StringComparer.FoldCaseExcludeSpecialI;
            if (compare.IgnoreCase != false
                || compare.IgnoreCaseOption
            != UTF16.StringComparer.FoldCaseExcludeSpecialI)
            {
                Errln("Error setting ignore case and options");
            }
            compare.IgnoreCase = true;
            compare.IgnoreCaseOption = UTF16.StringComparer.FoldCaseExcludeSpecialI;
            if (compare.IgnoreCase != true
                || compare.IgnoreCaseOption
            != UTF16.StringComparer.FoldCaseExcludeSpecialI)
            {
                Errln("Error setting ignore case and options");
            }
            compare.IgnoreCase = false;
            compare.IgnoreCaseOption = UTF16.StringComparer.FoldCaseDefault;
            if (compare.IgnoreCase != false
                || compare.IgnoreCaseOption
            != UTF16.StringComparer.FoldCaseDefault)
            {
                Errln("Error setting ignore case and options");
            }
        }

        [Test]
        public void TestCodePointCompare()
        {
            // these strings are in ascending order
            String[] str = {"\u0061", "\u20ac\ud801", "\u20ac\ud800\udc00",
                        "\ud800", "\ud800\uff61", "\udfff",
                        "\uff61\udfff", "\uff61\ud800\udc02", "\ud800\udc02",
                        "\ud84d\udc56"};
            UTF16.StringComparer cpcompare
                = new UTF16.StringComparer(true, false,
                         UTF16.StringComparer.FoldCaseDefault);
            UTF16.StringComparer cucompare
                = new UTF16.StringComparer();
            for (int i = 0; i < str.Length - 1; ++i)
            {
                if (cpcompare.Compare(str[i], str[i + 1]) >= 0)
                {
                    Errln("error: compare() in code point order fails for string "
                          + Utility.Hex(str[i]) + " and "
                          + Utility.Hex(str[i + 1]));
                }
                // ICU4N: Cannot compare exact values of CompareTo, since
                // only the sign is guaranteed to be the same. The actual
                // values are up to the implementation.

                // test code unit compare
                if (Math.Sign(cucompare.Compare(str[i], str[i + 1]))
                    != Math.Sign(str[i].CompareToOrdinal(str[i + 1])))
                {
                    Errln("error: compare() in code unit order fails for string "
                          + Utility.Hex(str[i]) + " and "
                          + Utility.Hex(str[i + 1]));
                }
            }
        }

        [Test]
        public void TestCaseCompare()
        {
            String mixed = "\u0061\u0042\u0131\u03a3\u00df\ufb03\ud93f\udfff";
            String otherDefault = "\u0041\u0062\u0131\u03c3\u0073\u0053\u0046\u0066\u0049\ud93f\udfff";
            String otherExcludeSpecialI = "\u0041\u0062\u0131\u03c3\u0053\u0073\u0066\u0046\u0069\ud93f\udfff";
            String different = "\u0041\u0062\u0131\u03c3\u0073\u0053\u0046\u0066\u0049\ud93f\udffd";

            UTF16.StringComparer compare = new UTF16.StringComparer();
            compare.IgnoreCase = true;
            compare.IgnoreCaseOption = UTF16.StringComparer.FoldCaseDefault;
            // test u_strcasecmp()
            int result = compare.Compare(mixed, otherDefault);
            if (result != 0)
            {
                Errln("error: default compare(mixed, other) = " + result
                      + " instead of 0");
            }

            // test u_strcasecmp() - exclude special i
            compare.IgnoreCase = true;
            compare.IgnoreCaseOption = UTF16.StringComparer.FoldCaseExcludeSpecialI;
            result = compare.Compare(mixed, otherExcludeSpecialI);
            if (result != 0)
            {
                Errln("error: exclude_i compare(mixed, other) = " + result
                      + " instead of 0");
            }

            // test u_strcasecmp()
            compare.IgnoreCase = true;
            compare.IgnoreCaseOption = UTF16.StringComparer.FoldCaseDefault;
            result = compare.Compare(mixed, different);
            if (result <= 0)
            {
                Errln("error: default compare(mixed, different) = " + result
                      + " instead of positive");
            }

            // test substrings - stop before the sharp s (U+00df)
            compare.IgnoreCase = true;
            compare.IgnoreCaseOption = UTF16.StringComparer.FoldCaseDefault;
            result = compare.Compare(mixed.Substring(0, 4), // ICU4N: Checked 2nd parameter
                                     different.Substring(0, 4)); // ICU4N: Checked 2nd parameter
            if (result != 0)
            {
                Errln("error: default compare(mixed substring, different substring) = "
              + result + " instead of 0");
            }
            // test substrings - stop in the middle of the sharp s (U+00df)
            compare.IgnoreCase = true;
            compare.IgnoreCaseOption = UTF16.StringComparer.FoldCaseDefault;
            result = compare.Compare(mixed.Substring(0, 5), // ICU4N: Checked 2nd parameter
                                     different.Substring(0, 5)); // ICU4N: Checked 2nd parameter
            if (result <= 0)
            {
                Errln("error: default compare(mixed substring, different substring) = "
              + result + " instead of positive");
            }
        }

        [Test]
        public void TestHasMoreCodePointsThan()
        {
            String str = "\u0061\u0062\ud800\udc00\ud801\udc01\u0063\ud802\u0064"
            + "\udc03\u0065\u0066\ud804\udc04\ud805\udc05\u0067";
            int length = str.Length;
            while (length >= 0)
            {
                for (int i = 0; i <= length; ++i)
                {
                    String s = str.Substring(0, i); // ICU4N: Checked 2nd parameter
                    for (int number = -1; number <= ((length - i) + 2); ++number)
                    {
                        bool flag = UTF16.HasMoreCodePointsThan(s, number);
                        if (flag != (UTF16.CountCodePoint(s) > number))
                        {
                            Errln("hasMoreCodePointsThan(" + Utility.Hex(s)
                                  + ", " + number + ") = " + flag + " is wrong");
                        }
                    }
                }
                --length;
            }

            // testing for null bad input
            for (length = -1; length <= 1; ++length)
            {
                for (int i = 0; i <= length; ++i)
                {
                    for (int number = -2; number <= 2; ++number)
                    {
                        bool flag = UTF16.HasMoreCodePointsThan((String)null,
                                                                   number);
                        if (flag != (UTF16.CountCodePoint((String)null) > number))
                        {
                            Errln("hasMoreCodePointsThan(null, " + number + ") = "
                      + flag + " is wrong");
                        }
                    }
                }
            }

            length = str.Length;
            while (length >= 0)
            {
                for (int i = 0; i <= length; ++i)
                {
                    ReadOnlySpan<char> s = str.AsSpan(0, i); // ICU4N: Checked 2nd parameter
                    for (int number = -1; number <= ((length - i) + 2); ++number)
                    {
                        bool flag = UTF16.HasMoreCodePointsThan(s, number);
                        if (flag != (UTF16.CountCodePoint(s) > number))
                        {
                            Errln("hasMoreCodePointsThan(" + Utility.Hex(s)
                                  + ", " + number + ") = " + flag + " is wrong");
                        }
                    }
                }
                --length;
            }

            // testing for null bad input
            for (length = -1; length <= 1; ++length)
            {
                for (int i = 0; i <= length; ++i)
                {
                    for (int number = -2; number <= 2; ++number)
                    {
                        bool flag = UTF16.HasMoreCodePointsThan(
                                       (StringBuffer)null, number);
                        if (flag
                            != (UTF16.CountCodePoint((StringBuffer)null) > number))
                        {
                            Errln("hasMoreCodePointsThan(null, " + number + ") = "
                              + flag + " is wrong");
                        }
                    }
                }
            }

            char[] strarray = str.ToCharArray();
            while (length >= 0)
            {
                for (int limit = 0; limit <= length; ++limit)
                {
                    for (int start = 0; start <= limit; ++start)
                    {
                        for (int number = -1; number <= ((limit - start) + 2);
                             ++number)
                        {
                            bool flag = UTF16.HasMoreCodePointsThan(strarray,
                                       start, limit, number);
                            if (flag != (UTF16.CountCodePoint(strarray, start,
                                                              limit) > number))
                            {
                                Errln("hasMoreCodePointsThan("
                                      + Utility.Hex(str.Substring(start, limit - start)) // ICU4N: Corrected 2nd substring parameter
                                      + ", " + start + ", " + limit + ", " + number
                                      + ") = " + flag + " is wrong");
                            }
                        }
                    }
                }
                --length;
            }

            // testing for null bad input
            for (length = -1; length <= 1; ++length)
            {
                for (int i = 0; i <= length; ++i)
                {
                    for (int number = -2; number <= 2; ++number)
                    {
                        bool flag = UTF16.HasMoreCodePointsThan(
                                       (StringBuffer)null, number);
                        if (flag
                            != (UTF16.CountCodePoint((StringBuffer)null) > number))
                        {
                            Errln("hasMoreCodePointsThan(null, " + number + ") = "
                              + flag + " is wrong");
                        }
                    }
                }
            }

            // bad input
            try
            {
                UTF16.HasMoreCodePointsThan(strarray, -2, -1, 5);
                Errln("hasMoreCodePointsThan(chararray) with negative indexes has to throw an exception");
            }
            catch (Exception e)
            {
                Logln("PASS: UTF16.hasMoreCodePointsThan failed as expected");
            }
            try
            {
                UTF16.HasMoreCodePointsThan(strarray, 5, 2, 5);
                Errln("hasMoreCodePointsThan(chararray) with limit less than start index has to throw an exception");
            }
            catch (Exception e)
            {
                Logln("PASS: UTF16.hasMoreCodePointsThan failed as expected");
            }
            try
            {
                if (UTF16.HasMoreCodePointsThan(strarray, -2, 2, 5))
                {
                    Errln("hasMoreCodePointsThan(chararray) with negative start indexes can't return true");
                }
            }
            catch (Exception e)
            {
            }
        }

        [Test]
        public void TestUtilities()
        {
            String[] tests = {
                "a",
                "\uFFFF",
                "ðŸ˜€",
                "\uD800",
                "\uDC00",
                "\uDBFF\uDfff",
                "",
                "\u0000",
                "\uDC00\uD800",
                "ab",
                "ðŸ˜€a",
                null,
        };
            UTF16.StringComparer sc = new UTF16.StringComparer(true, false, 0);
            foreach (String item1 in tests)
            {
                String nonNull1 = item1 ?? "";
                int count = UTF16.CountCodePoint(nonNull1);
                int expected = count == 0 || count > 1 ? -1 : nonNull1.CodePointAt(0);
                assertEquals("codepoint test " + Utility.Hex(nonNull1), expected, UTF16.GetSingleCodePoint(item1));
                if (expected == -1)
                {
                    continue;
                }
                foreach (String item2 in tests)
                {
                    String nonNull2 = item2 ?? "";
                    int scValue = sc.Compare(nonNull1, nonNull2).Signum();
                    int fValue = UTF16.CompareCodePoint(expected, item2).Signum();
                    assertEquals("comparison " + Utility.Hex(nonNull1) + ", " + Utility.Hex(nonNull2), scValue, fValue);
                }
            }
        }

        [Test]
        public void TestNewString()
        {
            int[] codePoints = {
                    UChar.ToCodePoint(UChar.MinHighSurrogate, UChar.MaxLowSurrogate),
                    UChar.ToCodePoint(UChar.MaxHighSurrogate, UChar.MinLowSurrogate),
                    UChar.MaxHighSurrogate,
                    'A',
                    -1,
                };


            String cpString = "" +
                UChar.MinHighSurrogate +
                UChar.MaxLowSurrogate +
                UChar.MaxHighSurrogate +
                UChar.MinLowSurrogate +
                UChar.MaxHighSurrogate +
                'A';

            int[][] tests = {
                    new int[] { 0, 1, 0, 2 },
                    new int[] { 0, 2, 0, 4 },
                    new int[] { 1, 1, 2, 2 },
                    new int[] { 1, 2, 2, 3 },
                    new int[] { 1, 3, 2, 4 },
                    new int[] { 2, 2, 4, 2 },
                    new int[] { 2, 3, 0, -1 },
                    new int[] { 4, 5, 0, -1 },
                    new int[] { 3, -1, 0, -1 }
                };

            for (int i = 0; i < tests.Length; ++i)
            {
                int[] t = tests[i];
                int s = t[0];
                int c = t[1];
                int rs = t[2];
                int rc = t[3];

                Exception e = null;
                try
                {
                    String str = UTF16.NewString(codePoints, s, c);
                    if (rc == -1 || !str.Equals(cpString.Substring(rs, rc))) // ICU4N: (rs + rc) - rs == rc
                    {
                        Errln("failed codePoints iter: " + i + " start: " + s + " len: " + c);
                    }
                    continue;
                }
                catch (IndexOutOfRangeException e1)
                {
                    e = e1;
                }
                catch (ArgumentException e2)
                {
                    e = e2;
                }
                if (rc != -1)
                {
                    Errln(e.ToString());
                }
            }
        }

        // private data members ----------------------------------------------

        private const String INDEXOF_SUPPLEMENTARY_STRING_ =
            "\ud841\udc02\u0071\udc02\ud841\u0071\ud841\udc02\u0071\u0072" +
            "\ud841\udc02\u0071\ud841\udc02\u0071\udc02\ud841\u0073";
        private readonly static int[] INDEXOF_SUPPLEMENTARY_CHAR_ =
            {0x71, 0xd841, 0xdc02,
                UTF16Util.GetRawSupplementary((char)0xd841,
                 (char)0xdc02)};
        private readonly static int[][] INDEXOF_SUPPLEMENTARY_CHAR_INDEX_ =
        {
            new int[] {2, 5, 8, 12, 15},
            new int[] {4, 17},
            new int[] {3, 16},
            new int[] {0, 6, 10, 13}
        };
        private const String INDEXOF_SUPPLEMENTARY_STR_ = "\udc02\ud841";
        private readonly static int[] INDEXOF_SUPPLEMENTARY_STR_INDEX_ =
                {3, 16};

        // private methods ---------------------------------------------------
    }
}
