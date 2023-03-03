using ICU4N.Dev.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Support.Text
{
    public class AttributedStringTest : TestFmwk
    {
        /**
        * @tests java.text.AttributedString#AttributedString(java.lang.String)
        */
        [Test]
        public void test_ConstructorLjava_lang_String()
        {
            String test = "Test string";
            AttributedString attrString = new AttributedString(test);
            AttributedCharacterIterator it = attrString.GetIterator();
            StringBuffer buf = new StringBuffer();
            buf.Append(it.First());
            char ch;
            while ((ch = it.Next()) != CharacterIterator.Done)
                buf.Append(ch);
            assertTrue("Wrong string: " + buf, buf.ToString().Equals(test, StringComparison.Ordinal));
        }

        /**
         * @tests java.text.AttributedString#AttributedString(AttributedCharacterIterator)
         */
        [Test]
        public void test_ConstructorLAttributedCharacterIterator()
        {
            //Regression for HARMONY-1354
            assertNotNull(string.Empty, new AttributedString(new testAttributedCharacterIterator()));
        }
        /**
         * @tests java.text.AttributedString#AttributedString(AttributedCharacterIterator, int, int)
         */
        [Test]
        public void test_ConstructorLAttributedCharacterIteratorII()
        {
            //Regression for HARMONY-1355
            assertNotNull(string.Empty, new AttributedString(new testAttributedCharacterIterator(), 0, 0));
        }

        private class testAttributedCharacterIterator : AttributedCharacterIterator
        {
            public override ICollection<AttributedCharacterIteratorAttribute> GetAllAttributeKeys()
            {
                return null;
            }
            public override object GetAttribute(AttributedCharacterIteratorAttribute attribute)
            {
                return null;
            }
            public override IDictionary<AttributedCharacterIteratorAttribute, object> GetAttributes()
            {
                return null;
            }
            public override int GetRunLimit<T>(ICollection<T> attributes)
            {
                return 0;
            }
            public override int GetRunLimit(AttributedCharacterIteratorAttribute attribute)
            {
                return 0;
            }
            public override int GetRunLimit()
            {
                return 0;
            }
            public override int GetRunStart<T>(ICollection<T> attributes)
            {
                return 0;
            }
            public override int GetRunStart(AttributedCharacterIteratorAttribute attribute)
            {
                return 0;
            }
            public override int GetRunStart()
            {
                return 0;
            }
            public override object Clone()
            {
                return null;
            }
            public override int Index => 0;
            public override int EndIndex => 0;
            public override int BeginIndex => 0;
            public override char SetIndex(int location)
            {
                return 'a';
            }
            public override char Previous()
            {
                return 'a';
            }
            public override char Next()
            {
                return 'a';
            }
            public override char Current => 'a';

            public override char Last()
            {
                return 'a';
            }
            public override char First()
            {
                return 'a';
            }
        }

        [Test]
        public void test_addAttributeLjava_text_AttributedCharacterIterator_AttributeLjava_lang_ObjectII()
        {
            AttributedString @as = new AttributedString("test");
            @as.AddAttribute(AttributedCharacterIteratorAttribute.Language, "a", 2,
                3);
            AttributedCharacterIterator it = @as.GetIterator();
            assertEquals("non-null value limit", 2, it
                    .GetRunLimit(AttributedCharacterIteratorAttribute.Language));

            @as = new AttributedString("test");
            @as.AddAttribute(AttributedCharacterIteratorAttribute.Language, null,
                2, 3);
            it = @as.GetIterator();
            assertEquals("null value limit", 4, it
                    .GetRunLimit(AttributedCharacterIteratorAttribute.Language));

            try
            {
                @as = new AttributedString("test");
                @as.AddAttribute(AttributedCharacterIteratorAttribute.Language,
                    null, -1, 3);
                fail("Expected IllegalArgumentException");
            }
            catch (ArgumentException e)
            {
                // Expected
            }

            // regression for Harmony-1244
            @as = new AttributedString("123", new Dictionary<AttributedCharacterIteratorAttribute, object>());
            try
            {
                @as.AddAttribute(null, new SortedSet<object>(), 0, 1);
                fail("should throw NullPointerException");
            }
            catch (ArgumentNullException e)
            {
                // expected
            }

            try
            {
                @as.AddAttribute(null, new SortedSet<object>(), -1, 1);
                fail("should throw NullPointerException");
            }
            catch (ArgumentNullException e)
            {
                // expected
            }
        }

        /**
         * @tests java.text.AttributedString.addAttribute(AttributedCharacterIterator, Object)
         */
        [Test]
        public void test_addAttributeLjava_text_AttributedCharacterIterator_AttributeLjava_lang_Object()
        {
            //regression for Harmony-1244
            AttributedString @as = new AttributedString("123", new Dictionary<AttributedCharacterIteratorAttribute, object>());
            try
            {
                @as.AddAttribute(null, new SortedSet<object>());
                fail("should throw NullPointerException");
            }
            catch (ArgumentNullException e)
            {
                // expected
            }
            try
            {
                @as.AddAttribute(null, null);
                fail("should throw NullPointerException");
            }
            catch (ArgumentNullException e)
            {
                // expected
            }
        }

#if FEATURE_IKVM

        [Test]
        public void TestListIteratorBehavior()
        {
            java.util.List list = ResetList();

            var it = list.listIterator();

            it.next(); // 0
            it.next(); // 1
            it.remove();
            assertEquals("invalid value", new java.lang.Integer(2), it.next()); // 2

            it.add(new java.lang.Integer(12));
            it.add(new java.lang.Integer(13));

            assertEquals("invalid value", new java.lang.Integer(3), it.next()); // 3
            assertEquals("invalid value", new java.lang.Integer(4), it.next()); // 4
            assertEquals("invalid value", new java.lang.Integer(5), it.next()); // 5
            assertEquals("invalid value", new java.lang.Integer(6), it.next()); // 6
            assertEquals("invalid value", new java.lang.Integer(7), it.next()); // 7
            assertEquals("invalid value", new java.lang.Integer(8), it.next()); // 8
            assertEquals("invalid value", new java.lang.Integer(9), it.next()); // 9
            it.add(new java.lang.Integer(20));
            assertFalse("End of list", it.hasNext());
            assertEquals("invalid end value", new java.lang.Integer(20), list.get(list.size() - 1));
        }

        private java.util.List ResetList()
        {
            var list = new java.util.ArrayList();
            for (int i = 0; i < 10; i++)
            {
                list.add(new java.lang.Integer(i));
            }
            return list;
        }
#endif
    }
}
