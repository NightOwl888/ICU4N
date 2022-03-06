using ICU4N.Dev.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Support.Text
{
    public class Support_Format : TestFmwk
    {
        protected String text;

        public Support_Format(String p1)
        //    : base(p1)
        {
        }

        // From TestCase in Harmony
        public virtual void RunTest()
        {
        }

        internal virtual void t_FormatWithField(int count, Formatter format, Object obj,
                String text, FormatField field, int begin, int end)
        {
            StringBuffer buffer = new StringBuffer();
            FieldPosition pos = new FieldPosition(field);
            format.Format(obj, buffer, pos);

            // System.out.println(buffer);
            // System.out.println(pos);

            if (text == null)
            {
                assertEquals("Test " + count + ": incorrect formatted text",
                        this.text, buffer.ToString());
            }
            else
            {
                assertEquals("Test " + count + ": incorrect formatted text", text,
                        buffer.ToString());
            }

            assertEquals("Test " + count + ": incorrect begin index for field "
                    + field, begin, pos.BeginIndex);
            assertEquals("Test " + count + ": incorrect end index for field "
                    + field, end, pos.EndIndex);
        }

        internal virtual void t_Format(int count, Object obj, Formatter format,
                IList<FieldContainer> expectedResults)
        {
            // System.out.println(format.format(object));
            IList<FieldContainer> results = FindFields(format.FormatToCharacterIterator(obj));
            assertTrue("Test " + count
                    + ": Format returned incorrect CharacterIterator for "
                    + format.Format(obj), Compare(results, expectedResults));
        }

        /**
         * compares two vectors regardless of the order of their elements
         */
        protected static bool Compare(IList<FieldContainer> vector1, IList<FieldContainer> vector2)
        {
            return vector1.Count == vector2.Count && !vector1.Except(vector2).Any(); //  vector1.containsAll(vector2);
        }

        /**
         * finds attributes with regards to char index in this
         * AttributedCharacterIterator, and puts them in a vector
         * 
         * @param iterator
         * @return a vector, each entry in this vector are of type FieldContainer ,
         *         which stores start and end indexes and an attribute this range
         *         has
         */
        protected static IList<FieldContainer> FindFields(AttributedCharacterIterator iterator)
        {
            List<FieldContainer> result = new List<FieldContainer>();
            while (iterator.Index != iterator.EndIndex)
            {
                int start = iterator.GetRunStart();
                int end = iterator.GetRunLimit();

                using var it = iterator.GetAttributes().Keys.GetEnumerator();
                while (it.MoveNext())
                {
                    AttributedCharacterIteratorAttribute attribute = it.Current;
                    Object value = iterator.GetAttribute(attribute);
                    result.Add(new FieldContainer(start, end, attribute, value));
                    // System.out.println(start + " " + end + ": " + attribute + ",
                    // " + value );
                    // System.out.println("v.add(new FieldContainer(" + start +"," +
                    // end +"," + attribute+ "," + value+ "));");
                }
                iterator.SetIndex(end);
            }
            return result;
        }

        public class FieldContainer
        {
            int start, end;

            AttributedCharacterIteratorAttribute attribute;

            Object value;

            // called from support_decimalformat and support_simpledateformat tests
            public FieldContainer(int start, int end,
                    AttributedCharacterIteratorAttribute attribute)
                : this(start, end, attribute, attribute)
            {
            }

            // called from support_messageformat tests
            public FieldContainer(int start, int end, AttributedCharacterIteratorAttribute attribute, int value)
                : this(start, end, attribute, (object)J2N.Numerics.Int32.GetInstance(value))
            {
            }

            // called from support_messageformat tests
            public FieldContainer(int start, int end, AttributedCharacterIteratorAttribute attribute,
                    object value)
            {
                this.start = start;
                this.end = end;
                this.attribute = attribute;
                this.value = value;
            }

            public override bool Equals(Object obj)
            {
                if (!(obj is FieldContainer)) {
                    return false;
                }

                FieldContainer fc = (FieldContainer)obj;
                return (start == fc.start && end == fc.end
                        && attribute == fc.attribute && value.Equals(fc.value));
            }

            public override int GetHashCode() // ICU4N: Added to keep the compiler from complaining
            {
                return base.GetHashCode();
            }
        }
    }
}
