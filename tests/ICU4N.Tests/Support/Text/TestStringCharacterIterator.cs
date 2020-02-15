using ICU4N.Dev.Test;
using NUnit.Framework;
using System;

namespace ICU4N.Support.Text
{
    public class TestStringCharacterIterator : TestFmwk
    {
        /**
         * @tests java.text.StringCharacterIterator.StringCharacterIterator(String,
         *        int)
         */
        [Test]
        public void Test_ConstructorI()
        {
            assertNotNull(new StringCharacterIterator("value", 0));
            assertNotNull(new StringCharacterIterator("value", "value".Length));
            assertNotNull(new StringCharacterIterator("", 0));
            try
            {
                new StringCharacterIterator(null, 0);
                fail("Assert 0: no null pointer");
            }
            catch (ArgumentNullException e)
            {
                // expected
            }

            try
            {
                new StringCharacterIterator("value", -1);
                fail("Assert 1: no illegal argument");
            }
            catch (ArgumentException e)
            {
                // expected
            }

            try
            {
                new StringCharacterIterator("value", "value".Length + 1);
                fail("Assert 2: no illegal argument");
            }
            catch (ArgumentException e)
            {
                // expected
            }
        }

        /**
         * @tests java.text.StringCharacterIterator(String, int, int, int)
         */
        [Test]
        public void Test_ConstructorIII()
        {
            assertNotNull(new StringCharacterIterator("value", 0, "value".Length,
                    0));
            assertNotNull(new StringCharacterIterator("value", 0, "value".Length,
                    1));
            assertNotNull(new StringCharacterIterator("", 0, 0, 0));

            try
            {
                new StringCharacterIterator(null, 0, 0, 0);
                fail("no null pointer");
            }
            catch (ArgumentNullException e)
            {
                // Expected
            }

            try
            {
                new StringCharacterIterator("value", -1, "value".Length, 0);
                fail("no illegal argument: invalid begin");
            }
            catch (ArgumentException e)
            {
                // Expected
            }

            try
            {
                new StringCharacterIterator("value", 0, "value".Length + 1, 0);
                fail("no illegal argument: invalid end");
            }
            catch (ArgumentException e)
            {
                // Expected
            }

            try
            {
                new StringCharacterIterator("value", 2, 1, 0);
                fail("no illegal argument: start greater than end");
            }
            catch (ArgumentException e)
            {
                // Expected
            }

            try
            {
                new StringCharacterIterator("value", 2, 1, 2);
                fail("no illegal argument: start greater than end");
            }
            catch (ArgumentException e)
            {
                // Expected
            }

            try
            {
                new StringCharacterIterator("value", 2, 4, 1);
                fail("no illegal argument: location greater than start");
            }
            catch (ArgumentException e)
            {
                // Expected
            }

            try
            {
                new StringCharacterIterator("value", 0, 2, 3);
                fail("no illegal argument: location greater than start");
            }
            catch (ArgumentException e)
            {
                // Expected
            }
        }

        /**
         * @tests java.text.StringCharacterIterator.equals(Object)
         */
        [Test]
        public void Test_equalsLjava_lang_Object()
        {
            StringCharacterIterator sci0 = new StringCharacterIterator("fixture");
            assertEquals(sci0, sci0);
            assertFalse(sci0.Equals(null));
            assertFalse(sci0.Equals("fixture"));

            StringCharacterIterator sci1 = new StringCharacterIterator("fixture");
            assertEquals(sci0, sci1);

            sci1.Next();
            assertFalse(sci0.Equals(sci1));
            sci0.Next();
            assertEquals(sci0, sci1);

            StringCharacterIterator it1 = new StringCharacterIterator("testing", 2,
                    6, 4);
            StringCharacterIterator it2 = new StringCharacterIterator("xxstinx", 2,
                    6, 4);
            assertTrue("Range is equal", !it1.Equals(it2));
            StringCharacterIterator it3 = new StringCharacterIterator("testing", 2,
                    6, 2);
            it3.SetIndex(4);
            assertTrue("Not equal", it1.Equals(it3));
        }

        /**
         * @tests java.text.StringCharacterIterator.clone()
         */
        [Test]
        public void Test_clone()
        {
            StringCharacterIterator sci0 = new StringCharacterIterator("fixture");
            assertSame(sci0, sci0);
            StringCharacterIterator sci1 = (StringCharacterIterator)sci0.Clone();
            assertNotSame(sci0, sci1);
            assertEquals(sci0, sci1);

            StringCharacterIterator it = new StringCharacterIterator("testing", 2,
                    6, 4);
            StringCharacterIterator clone = (StringCharacterIterator)it.Clone();
            assertTrue("Clone not equal", it.Equals(clone));
        }

        /**
         * @tests java.text.StringCharacterIterator.Current
         */
        [Test]
        public void Test_current()
        {
            StringCharacterIterator fixture = new StringCharacterIterator("fixture");
            assertEquals('f', fixture.Current);
            fixture.Next();
            assertEquals('i', fixture.Current);

            StringCharacterIterator it =
                new StringCharacterIterator("testing", 2, 6, 4);
            assertEquals("Wrong current char", 'i', it.Current);
        }

        /**
         * @tests java.text.StringCharacterIterator.First()
         */
        [Test]
        public void Test_first()
        {
            StringCharacterIterator fixture = new StringCharacterIterator("fixture");
            assertEquals('f', fixture.First());
            fixture.Next();
            assertEquals('f', fixture.First());
            fixture = new StringCharacterIterator("fixture", 1);
            assertEquals('f', fixture.First());
            fixture = new StringCharacterIterator("fixture", 1, "fixture".Length,
                    2);
            assertEquals('i', fixture.First());

            StringCharacterIterator it1 =
                new StringCharacterIterator("testing", 2, 6, 4);
            assertEquals("Wrong first char", 's', it1.First());
            assertEquals("Wrong next char", 't', it1.Next());
            it1 = new StringCharacterIterator("testing", 2, 2, 2);
            assertTrue("Not DONE", it1.First() == CharacterIterator.Done);
        }

        /**
         * @tests java.text.StringCharacterIterator.getBeginIndex()
         */
        [Test]
        public void Test_getBeginIndex()
        {
            StringCharacterIterator fixture = new StringCharacterIterator("fixture");
            assertEquals(0, fixture.BeginIndex);
            fixture = new StringCharacterIterator("fixture", 1);
            assertEquals(0, fixture.BeginIndex);
            fixture = new StringCharacterIterator("fixture", 1, "fixture".Length,
                    2);
            assertEquals(1, fixture.BeginIndex);

            StringCharacterIterator it1 =
                new StringCharacterIterator("testing", 2, 6, 4);
            assertEquals("Wrong begin index 2", 2, it1.BeginIndex);
        }

        /**
         * @tests java.text.StringCharacterIterator.getEndIndex()
         */
        [Test]
        public void Test_getEndIndex()
        {
            StringCharacterIterator fixture = new StringCharacterIterator("fixture");
            assertEquals("fixture".Length, fixture.EndIndex);
            fixture = new StringCharacterIterator("fixture", 1);
            assertEquals("fixture".Length, fixture.EndIndex);
            fixture = new StringCharacterIterator("fixture", 1, "fixture".Length,
                    2);
            assertEquals("fixture".Length, fixture.EndIndex);
            fixture = new StringCharacterIterator("fixture", 1, 4, 2);
            assertEquals(4, fixture.EndIndex);

            StringCharacterIterator it1 =
                new StringCharacterIterator("testing", 2, 6, 4);
            assertEquals("Wrong end index 6", 6, it1.EndIndex);
        }

        /**
         * @tests java.text.StringCharacterIterator.Index
         */
        [Test]
        public void TestGetIndex()
        {
            StringCharacterIterator fixture = new StringCharacterIterator("fixture");
            assertEquals(0, fixture.Index);
            fixture = new StringCharacterIterator("fixture", 1);
            assertEquals(1, fixture.Index);
            fixture = new StringCharacterIterator("fixture", 1, "fixture".Length,
                    2);
            assertEquals(2, fixture.Index);
            fixture = new StringCharacterIterator("fixture", 1, 4, 2);
            assertEquals(2, fixture.Index);
        }

        /**
         * @tests java.text.StringCharacterIterator.Last()
         */
        [Test]
        public void TestLast()
        {
            StringCharacterIterator fixture = new StringCharacterIterator("fixture");
            assertEquals('e', fixture.Last());
            fixture.Next();
            assertEquals('e', fixture.Last());
            fixture = new StringCharacterIterator("fixture", 1);
            assertEquals('e', fixture.Last());
            fixture = new StringCharacterIterator("fixture", 1, "fixture".Length,
                    2);
            assertEquals('e', fixture.Last());
            fixture = new StringCharacterIterator("fixture", 1, 4, 2);
            assertEquals('t', fixture.Last());
        }

        /**
         * @tests java.text.StringCharacterIterator.Next()
         */
        [Test]
        public void Test_next()
        {
            StringCharacterIterator fixture = new StringCharacterIterator("fixture");
            assertEquals(0, fixture.Index);
            assertEquals('i', fixture.Next());
            assertEquals(1, fixture.Index);
            assertEquals('x', fixture.Next());
            assertEquals(2, fixture.Index);
            assertEquals('t', fixture.Next());
            assertEquals(3, fixture.Index);
            assertEquals('u', fixture.Next());
            assertEquals(4, fixture.Index);
            assertEquals('r', fixture.Next());
            assertEquals(5, fixture.Index);
            assertEquals('e', fixture.Next());
            assertEquals(6, fixture.Index);
            assertEquals(CharacterIterator.Done, fixture.Next());
            assertEquals(7, fixture.Index);
            assertEquals(CharacterIterator.Done, fixture.Next());
            assertEquals(7, fixture.Index);
            assertEquals(CharacterIterator.Done, fixture.Next());
            assertEquals(7, fixture.Index);

            StringCharacterIterator it1 = new StringCharacterIterator("testing", 2,
                    6, 3);
            char result = it1.Next();
            assertEquals("Wrong next char1", 'i', result);
            assertEquals("Wrong next char2", 'n', it1.Next());
            assertTrue("Wrong next char3", it1.Next() == CharacterIterator.Done);
            assertTrue("Wrong next char4", it1.Next() == CharacterIterator.Done);
            int index = it1.Index;
            assertEquals("Wrong index", 6, index);
            assertTrue("Wrong current char",
                       it1.Current == CharacterIterator.Done);
        }

        /**
         * @tests java.text.StringCharacterIterator.Previous()
         */
        [Test]
        public void Test_previous()
        {
            StringCharacterIterator fixture = new StringCharacterIterator("fixture");
            assertEquals(CharacterIterator.Done, fixture.Previous());
            assertEquals('i', fixture.Next());
            assertEquals('x', fixture.Next());
            assertEquals('t', fixture.Next());
            assertEquals('u', fixture.Next());
            assertEquals('r', fixture.Next());
            assertEquals('e', fixture.Next());
            assertEquals(CharacterIterator.Done, fixture.Next());
            assertEquals(CharacterIterator.Done, fixture.Next());
            assertEquals(CharacterIterator.Done, fixture.Next());
            assertEquals(7, fixture.Index);
            assertEquals('e', fixture.Previous());
            assertEquals(6, fixture.Index);
            assertEquals('r', fixture.Previous());
            assertEquals(5, fixture.Index);
            assertEquals('u', fixture.Previous());
            assertEquals(4, fixture.Index);
            assertEquals('t', fixture.Previous());
            assertEquals(3, fixture.Index);
            assertEquals('x', fixture.Previous());
            assertEquals(2, fixture.Index);
            assertEquals('i', fixture.Previous());
            assertEquals(1, fixture.Index);
            assertEquals('f', fixture.Previous());
            assertEquals(0, fixture.Index);
            assertEquals(CharacterIterator.Done, fixture.Previous());
            assertEquals(0, fixture.Index);

            StringCharacterIterator it1 =
                new StringCharacterIterator("testing", 2, 6, 4);
            assertEquals("Wrong previous char1", 't', it1.Previous());
            assertEquals("Wrong previous char2", 's', it1.Previous());
            assertTrue("Wrong previous char3",
                       it1.Previous() == CharacterIterator.Done);
            assertTrue("Wrong previous char4",
                       it1.Previous() == CharacterIterator.Done);
            assertEquals("Wrong index", 2, it1.Index);
            assertEquals("Wrong current char", 's', it1.Current);
        }

        /**
         * @tests java.text.StringCharacterIterator.setIndex(int)
         */
        [Test]
        public void Test_setIndex()
        {
            StringCharacterIterator fixture = new StringCharacterIterator("fixture");
            while (fixture.Next() != CharacterIterator.Done)
            {
                // empty
            }
            assertEquals("fixture".Length, fixture.Index);
            fixture.SetIndex(0);
            assertEquals(0, fixture.Index);
            assertEquals('f', fixture.Current);
            fixture.SetIndex("fixture".Length - 1);
            assertEquals('e', fixture.Current);
            try
            {
                fixture.SetIndex(-1);
                fail("no illegal argument");
            }
            catch (ArgumentException e)
            {
                // expected
            }

            try
            {
                fixture.SetIndex("fixture".Length + 1);
                fail("no illegal argument");
            }
            catch (ArgumentException e)
            {
                // expected
            }
        }

        /**
         * @tests java.text.StringCharacterIterator.setText(String)
         */
        [Test]
        public void Test_setText()
        {
            StringCharacterIterator fixture = new StringCharacterIterator("fixture");
            fixture.SetText("fix");
            assertEquals('f', fixture.Current);
            assertEquals('x', fixture.Last());

            try
            {
                fixture.SetText(null);
                fail("no null pointer");
            }
            catch (ArgumentNullException e)
            {
                // expected
            }
        }

        /**
         * @tests java.text.StringCharacterIterator#StringCharacterIterator(java.lang.String)
         */
        [Test]
        public void Test_ConstructorLjava_lang_String()
        {
            assertNotNull(new StringCharacterIterator("value"));
            assertNotNull(new StringCharacterIterator(""));
            try
            {
                new StringCharacterIterator(null);
                fail("Assert 0: no null pointer");
            }
            catch (ArgumentNullException e)
            {
                // expected
            }

            StringCharacterIterator it = new StringCharacterIterator("testing");
            assertEquals("Wrong begin index", 0, it.BeginIndex);
            assertEquals("Wrong end index", 7, it.EndIndex);
            assertEquals("Wrong current index", 0, it.Index);
            assertEquals("Wrong current char", 't', it.Current);
            assertEquals("Wrong next char", 'e', it.Next());
        }

        /**
         * @tests java.text.StringCharacterIterator#StringCharacterIterator(java.lang.String,
         *        int)
         */
        [Test]
        public void Test_ConstructorLjava_lang_StringI()
        {
            StringCharacterIterator it = new StringCharacterIterator("testing", 3);
            assertEquals("Wrong begin index", 0, it.BeginIndex);
            assertEquals("Wrong end index", 7, it.EndIndex);
            assertEquals("Wrong current index", 3, it.Index);
            assertEquals("Wrong current char", 't', it.Current);
            assertEquals("Wrong next char", 'i', it.Next());
        }

        /**
         * @tests java.text.StringCharacterIterator#StringCharacterIterator(java.lang.String,
         *        int, int, int)
         */
        [Test]
        public void Test_ConstructorLjava_lang_StringIII()
        {
            StringCharacterIterator it = new StringCharacterIterator("testing", 2,
                    6, 4);
            assertEquals("Wrong begin index", 2, it.BeginIndex);
            assertEquals("Wrong end index", 6, it.EndIndex);
            assertEquals("Wrong current index", 4, it.Index);
            assertEquals("Wrong current char", 'i', it.Current);
            assertEquals("Wrong next char", 'n', it.Next());
        }

        /**
         * @tests java.text.StringCharacterIterator#getIndex()
         */
        [Test]
        public void Test_getIndex()
        {
            StringCharacterIterator it1 = new StringCharacterIterator("testing", 2,
                    6, 4);
            assertEquals("Wrong index 4", 4, it1.Index);
            it1.Next();
            assertEquals("Wrong index 5", 5, it1.Index);
            it1.Last();
            assertEquals("Wrong index 4/2", 5, it1.Index);
        }

        /**
         * @tests java.text.StringCharacterIterator#hashCode()
         */
        [Test]
        public void Test_hashCode()
        {
            StringCharacterIterator it1 = new StringCharacterIterator("testing", 2,
                    6, 4);
            StringCharacterIterator it2 = new StringCharacterIterator("xxstinx", 2,
                    6, 4);
            assertTrue("Hash is equal", it1.GetHashCode() != it2.GetHashCode());
            StringCharacterIterator it3 = new StringCharacterIterator("testing", 2,
                    6, 2);
            assertTrue("Hash equal1", it1.GetHashCode() != it3.GetHashCode());
            it3 = new StringCharacterIterator("testing", 0, 6, 4);
            assertTrue("Hash equal2", it1.GetHashCode() != it3.GetHashCode());
            it3 = new StringCharacterIterator("testing", 2, 5, 4);
            assertTrue("Hash equal3", it1.GetHashCode() != it3.GetHashCode());
            it3 = new StringCharacterIterator("froging", 2, 6, 4);
            assertTrue("Hash equal4", it1.GetHashCode() != it3.GetHashCode());

            StringCharacterIterator sci0 = new StringCharacterIterator("fixture");
            assertEquals(sci0.GetHashCode(), sci0.GetHashCode());

            StringCharacterIterator sci1 = new StringCharacterIterator("fixture");
            assertEquals(sci0.GetHashCode(), sci1.GetHashCode());

            sci1.Next();
            sci0.Next();
            assertEquals(sci0.GetHashCode(), sci1.GetHashCode());
        }

        /**
         * @tests java.text.StringCharacterIterator#last()
         */
        [Test]
        public void Test_last()
        {
            StringCharacterIterator it1 = new StringCharacterIterator("testing", 2,
                    6, 3);
            assertEquals("Wrong last char", 'n', it1.Last());
            assertEquals("Wrong previous char", 'i', it1.Previous());
            it1 = new StringCharacterIterator("testing", 2, 2, 2);
            assertTrue("Not DONE", it1.Last() == CharacterIterator.Done);
        }

        /**
         * @tests java.text.StringCharacterIterator#setIndex(int)
         */
        [Test]
        public void Test_setIndexI()
        {
            StringCharacterIterator it1 = new StringCharacterIterator("testing", 2,
                    6, 4);
            assertEquals("Wrong result1", 's', it1.SetIndex(2));
            char result = it1.Next();
            assertTrue("Wrong next char: " + result, result == 't');
            assertTrue("Wrong result2", it1.SetIndex(6) == CharacterIterator.Done);
            assertEquals("Wrong previous char", 'n', it1.Previous());
        }

        /**
         * @tests java.text.StringCharacterIterator#setText(java.lang.String)
         */
        [Test]
        public void Test_setTextLjava_lang_String()
        {
            StringCharacterIterator it1 = new StringCharacterIterator("testing", 2,
                    6, 4);
            it1.SetText("frog");
            assertEquals("Wrong begin index", 0, it1.BeginIndex);
            assertEquals("Wrong end index", 4, it1.EndIndex);
            assertEquals("Wrong current index", 0, it1.Index);
        }
    }
}
