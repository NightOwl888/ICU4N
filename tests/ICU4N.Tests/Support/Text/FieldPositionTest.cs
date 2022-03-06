using ICU4N.Dev.Test;
using NUnit.Framework;

namespace ICU4N.Support.Text
{
    public class FieldPositionTest : TestFmwk
    {
        internal class FormatField : ICU4N.Support.Text.FormatField
        {

            private static readonly long serialVersionUID = 276966692217360283L;

            /// <summary>
            /// Constructs a new instance of <see cref="FormatField"/> with the given field name.
            /// </summary>
            /// <param name="fieldName">the field name</param>
            internal FormatField(string fieldName)
                : base(fieldName)
            {
            }
        }

        // Mocks the Java DateFormat static values (since we don't have a DateFormat)
        private readonly static FormatField ERA = new FormatField("era");
        private readonly static FormatField MONTH = new FormatField("month");
        private readonly static FormatField MINUTE = new FormatField("minute");
        private readonly static FormatField DAY_OF_WEEK_IN_MONTH = new FormatField("day of week in month");
        private readonly static FormatField AM_PM = new FormatField("am pm");
        private readonly static FormatField HOUR1 = new FormatField("hour1");
        private readonly static FormatField TIME_ZONE = new FormatField("time zone");

        private const int MONTH_FIELD = 2;
        private const int HOUR1_FIELD = 15;
        private const int TIMEZONE_FIELD = 17;

        /**
         * @tests java.text.FieldPosition#FieldPosition(int)
         */
        [Test]
        public void Test_ConstructorI()
        {
            // Test for constructor java.text.FieldPosition(int)
            FieldPosition fpos = new FieldPosition(/*DateFormat.*/MONTH_FIELD);
            assertEquals("Test1: Constructor failed to set field identifier!",
                    /*DateFormat.*/MONTH_FIELD, fpos.Field);
            assertNull("Constructor failed to set field attribute!", fpos
                    .FieldAttribute);
        }

        /**
         * @tests java.text.FieldPosition#FieldPosition(java.text.Format$Field)
         */
        [Test]
        public void Test_ConstructorLjava_text_Format_Field()
        {
            // Test for constructor java.text.FieldPosition(Format.Field)
            

            FieldPosition fpos = new FieldPosition(MONTH /*DateFormat.Field.MONTH*/);
            assertSame("Constructor failed to set field attribute!",
                    MONTH /*DateFormat.Field.MONTH*/, fpos.FieldAttribute);
            assertEquals("Test1: Constructor failed to set field identifier!", -1,
                    fpos.Field);
        }

        /**
         * @tests java.text.FieldPosition#FieldPosition(java.text.Format$Field, int)
         */
        [Test]
        public void Test_ConstructorLjava_text_Format_FieldI()
        {
            // Test for constructor java.text.FieldPosition(Format.Field, int)
            FieldPosition fpos = new FieldPosition(/*DateFormat.Field.*/MONTH,
                    /*DateFormat.*/MONTH_FIELD);
            assertSame("Constructor failed to set field attribute!",
                    /*DateFormat.Field.*/MONTH, fpos.FieldAttribute);
            assertEquals("Test1: Constructor failed to set field identifier!",
                    /*DateFormat.*/MONTH_FIELD, fpos.Field);

            // test special cases
            FieldPosition fpos2 = new FieldPosition(/*DateFormat.Field.*/HOUR1,
                    /*DateFormat.*/HOUR1_FIELD);
            assertSame("Constructor failed to set field attribute!",
                    /*DateFormat.Field.*/HOUR1, fpos2.FieldAttribute);
            assertEquals("Test2: Constructor failed to set field identifier!",
                    /*DateFormat.*/HOUR1_FIELD, fpos2.Field);

            FieldPosition fpos3 = new FieldPosition(/*DateFormat.Field.*/TIME_ZONE,
                    /*DateFormat.*/MONTH_FIELD);
            assertSame("Constructor failed to set field attribute!",
                    /*DateFormat.Field.*/TIME_ZONE, fpos3.FieldAttribute);
            assertEquals("Test3: Constructor failed to set field identifier!",
                    /*DateFormat.*/MONTH_FIELD, fpos3.Field);
        }

        /**
         * @tests java.text.FieldPosition#equals(java.lang.Object)
         */
        [Test]
        public void Test_equalsLjava_lang_Object()
        {
            // Test for method boolean
            // java.text.FieldPosition.equals(java.lang.Object)
            FieldPosition fpos = new FieldPosition(1);
            FieldPosition fpos1 = new FieldPosition(1);
            assertTrue("Identical objects were not equal!", fpos.Equals(fpos1));

            FieldPosition fpos2 = new FieldPosition(2);
            assertTrue("Objects with a different ID should not be equal!", !fpos
                    .Equals(fpos2));

            fpos.BeginIndex=(1);
            fpos1.BeginIndex=(2);
            assertTrue("Objects with a different beginIndex were still equal!",
                    !fpos.Equals(fpos1));
            fpos1.BeginIndex=(1);
            fpos1.EndIndex=(2);
            assertTrue("Objects with a different endIndex were still equal!", !fpos
                    .Equals(fpos1));

            FieldPosition fpos3 = new FieldPosition(/*DateFormat.Field.*/ERA, 1);
            assertTrue("Objects with a different attribute should not be equal!",
                    !fpos.Equals(fpos3));
            FieldPosition fpos4 = new FieldPosition(/*DateFormat.Field.*/AM_PM, 1);
            assertTrue("Objects with a different attribute should not be equal!",
                    !fpos3.Equals(fpos4));
        }

        /**
         * @tests java.text.FieldPosition#getBeginIndex()
         */
        [Test]
        public void Test_getBeginIndex()
        {
            // Test for method int java.text.FieldPosition.getBeginIndex()
            FieldPosition fpos = new FieldPosition(1);
            fpos.EndIndex=(3);
            fpos.BeginIndex=(2);
            assertEquals("getBeginIndex should have returned 2",
                    2, fpos.BeginIndex);
        }

        /**
         * @tests java.text.FieldPosition#getEndIndex()
         */
        [Test]
        public void Test_getEndIndex()
        {
            // Test for method int java.text.FieldPosition.getEndIndex()
            FieldPosition fpos = new FieldPosition(1);
            fpos.BeginIndex=(2);
            fpos.EndIndex=(3);
            assertEquals("getEndIndex should have returned 3",
                    3, fpos.EndIndex);
        }

        /**
         * @tests java.text.FieldPosition#getField()
         */
        [Test]
        public void Test_getField()
        {
            // Test for method int java.text.FieldPosition.getField()
            FieldPosition fpos = new FieldPosition(65);
            assertEquals("FieldPosition(65) should have caused getField to return 65",
                    65, fpos.Field);
            FieldPosition fpos2 = new FieldPosition(/*DateFormat.Field.*/MINUTE);
            assertEquals("FieldPosition(DateFormat.Field.MINUTE) should have caused getField to return -1",
                    -1, fpos2.Field);
        }

        /**
         * @tests java.text.FieldPosition#getFieldAttribute()
         */
        [Test]
        public void Test_getFieldAttribute()
        {
            // Test for method int java.text.FieldPosition.getFieldAttribute()
            FieldPosition fpos = new FieldPosition(/*DateFormat.Field.*/ TIME_ZONE);
            assertTrue(
                    "FieldPosition(DateFormat.Field.TIME_ZONE) should have caused getFieldAttribute to return DateFormat.Field.TIME_ZONE",
                    fpos.FieldAttribute == /*DateFormat.Field.*/TIME_ZONE);

            FieldPosition fpos2 = new FieldPosition(/*DateFormat.*/TIMEZONE_FIELD);
            assertNull(
                    "FieldPosition(DateFormat.TIMEZONE_FIELD) should have caused getFieldAttribute to return null",
                    fpos2.FieldAttribute);
        }

        /**
         * @tests java.text.FieldPosition#hashCode()
         */
        [Test]
        public void Test_hashCode()
        {
            // Test for method int java.text.FieldPosition.hashCode()
            FieldPosition fpos = new FieldPosition(1);
            fpos.BeginIndex=(5);
            fpos.EndIndex=(110);
            fpos.GetHashCode();

            FieldPosition fpos2 = new FieldPosition(
                    /*DateFormat.Field.*/DAY_OF_WEEK_IN_MONTH);
            fpos2.BeginIndex=(5);
            fpos2.EndIndex=(110);
            fpos2.GetHashCode();
        }

        /**
         * @tests java.text.FieldPosition#setBeginIndex(int)
         */
        [Test]
        public void Test_setBeginIndexI()
        {
            // Test for method void java.text.FieldPosition.setBeginIndex(int)
            FieldPosition fpos = new FieldPosition(1);
            fpos.BeginIndex=(2);
            fpos.EndIndex=(3);
            assertEquals("beginIndex should have been set to 2",
                    2, fpos.BeginIndex);
        }

        /**
         * @tests java.text.FieldPosition#setEndIndex(int)
         */
        [Test]
        public void Test_setEndIndexI()
        {
            // Test for method void java.text.FieldPosition.setEndIndex(int)
            FieldPosition fpos = new FieldPosition(1);
            fpos.EndIndex=(3);
            fpos.BeginIndex=(2);
            assertEquals("EndIndex should have been set to 3",
                    3, fpos.EndIndex);
        }

        /**
         * @tests java.text.FieldPosition#toString()
         */
        [Test]
        public void Test_toString()
        {
            // Test for method java.lang.String java.text.FieldPosition.toString()
            FieldPosition fpos = new FieldPosition(1);
            fpos.BeginIndex=(2);
            fpos.EndIndex=(3);
            assertEquals(
                    "ToString returned the wrong value:",
                    "FieldPosition[attribute=null, field=1, beginIndex=2, endIndex=3]",
                    fpos.ToString());

            FieldPosition fpos2 = new FieldPosition(/*DateFormat.Field.*/ERA);
            fpos2.BeginIndex=(4);
            fpos2.EndIndex=(5);
            assertEquals("ToString returned the wrong value:",
                    "FieldPosition[attribute=" + /*DateFormat.Field.*/ERA
                            + ", field=-1, beginIndex=4, endIndex=5]", fpos2
                            .ToString());
        }
    }
}
