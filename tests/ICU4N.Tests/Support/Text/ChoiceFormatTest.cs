using ICU4N.Dev.Test;
using ICU4N.Text;
using J2N.Text;
using NUnit.Framework;
using System;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Support.Text
{
    [TestFixture]
    public class ChoiceFormatTest : TestFmwk
    {
        double[] limits = new double[] { 0, 1, ChoiceFormat.NextDouble(1),
            ChoiceFormat.NextDouble(2) };

        string[] formats = new String[] { "Less than one", "one",
            "Between one and two", "Greater than two" };

        ChoiceFormat f1;

        public ChoiceFormatTest()
        {
            f1 = new ChoiceFormat(limits, formats);
        }

        /**
         * @tests java.text.ChoiceFormat#ChoiceFormat(double[], java.lang.String[])
         */
        [Test]
        public void test_Constructor_D_Ljava_lang_String()
        {
            // Test for method java.text.ChoiceFormat(double [], java.lang.String
            // [])
            String formattedString;
            double[] appleLimits = { 1, 2, 3, 4, 5 };
            String[] appleFormats = { "Tiny Apple", "Small Apple", "Medium Apple",
                "Large Apple", "Huge Apple" };
            ChoiceFormat cf = new ChoiceFormat(appleLimits, appleFormats);

            formattedString = cf.Format(double.NegativeInfinity);
            assertTrue("a) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Tiny Apple"));
            formattedString = cf.Format(0.5d);
            assertTrue("b) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Tiny Apple"));
            formattedString = cf.Format(1d);
            assertTrue("c) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Tiny Apple"));
            formattedString = cf.Format(1.5d);
            assertTrue("d) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Tiny Apple"));
            formattedString = cf.Format(2d);
            assertTrue("e) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Small Apple"));
            formattedString = cf.Format(2.5d);
            assertTrue("f) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Small Apple"));
            formattedString = cf.Format(3d);
            assertTrue("g) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Medium Apple"));
            formattedString = cf.Format(4d);
            assertTrue("h) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Large Apple"));
            formattedString = cf.Format(5d);
            assertTrue("i) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Huge Apple"));
            formattedString = cf.Format(5.5d);
            assertTrue("j) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Huge Apple"));
            formattedString = cf.Format(6.0d);
            assertTrue("k) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Huge Apple"));
            formattedString = cf.Format(double.PositiveInfinity);
            assertTrue("l) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Huge Apple"));
        }

        /**
         * @tests java.text.ChoiceFormat#ChoiceFormat(java.lang.String)
         */
        [Test]
        [Ignore("ICU4N TODO: Implement DecimalFormat & RuleBasedNumberFormat")]
        public void test_ConstructorLjava_lang_String()
        {
            // Test for method java.text.ChoiceFormat(java.lang.String)
            String formattedString;
            String patternString = "-2#Inverted Orange| 0#No Orange| 0<Almost No Orange| 1#Normal Orange| 2#Expensive Orange";
            ChoiceFormat cf = new ChoiceFormat(patternString);

            formattedString = cf.Format(double.NegativeInfinity);
            assertTrue("a) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Inverted Orange"));
            formattedString = cf.Format(-3);
            assertTrue("b) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Inverted Orange"));
            formattedString = cf.Format(-2);
            assertTrue("c) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Inverted Orange"));
            formattedString = cf.Format(-1);
            assertTrue("d) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Inverted Orange"));
            formattedString = cf.Format(-0);
            assertTrue("e) Incorrect format returned: " + formattedString,
                    formattedString.Equals("No Orange"));
            formattedString = cf.Format(0);
            assertTrue("f) Incorrect format returned: " + formattedString,
                    formattedString.Equals("No Orange"));
            formattedString = cf.Format(0.1);
            assertTrue("g) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Almost No Orange"));
            formattedString = cf.Format(1);
            assertTrue("h) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Normal Orange"));
            formattedString = cf.Format(1.5);
            assertTrue("i) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Normal Orange"));
            formattedString = cf.Format(2);
            assertTrue("j) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Expensive Orange"));
            formattedString = cf.Format(3);
            assertTrue("k) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Expensive Orange"));
            formattedString = cf.Format(double.PositiveInfinity);
            assertTrue("l) Incorrect format returned: " + formattedString,
                    formattedString.Equals("Expensive Orange"));

        }

        /**
         * @tests java.text.ChoiceFormat#applyPattern(java.lang.String)
         */
        [Test]
        [Ignore("ICU4N TODO: Implement DecimalFormat & RuleBasedNumberFormat")]
        public void test_applyPatternLjava_lang_String()
        {
            // Test for method void
            // java.text.ChoiceFormat.applyPattern(java.lang.String)
            ChoiceFormat f = (ChoiceFormat)f1.Clone();
            f.ApplyPattern("0#0|1#1");
            assertTrue("Incorrect limits", Array.Equals(f.GetLimits(),
                    new double[] { 0, 1 }));
            assertTrue("Incorrect formats", Array.Equals(f.GetFormats(),
                    new string[] { "0", "1" }));

            //Regression for Harmony 540
            double[] choiceLimits = { -1, 0, 1, ChoiceFormat.NextDouble(1) };
            String[] choiceFormats = { "is negative", "is zero or fraction",
                "is one", "is more than 1" };

            f = new ChoiceFormat("");
            f.ApplyPattern("-1#is negative|0#is zero or fraction|1#is one|1<is more than 1");
            assertTrue("Incorrect limits", Array.Equals(f.GetLimits(),
                    choiceLimits));
            assertTrue("Incorrect formats", Array.Equals(f.GetFormats(),
                    choiceFormats));

            f = new ChoiceFormat("");
            try
            {
                f.ApplyPattern("-1#is negative|0#is zero or fraction|-1#is one|1<is more than 1");
                fail("Expected IllegalArgumentException");
            }
            catch (ArgumentException e)
            {
                // Expected
            }

            f = new ChoiceFormat("");
            try
            {
                f.ApplyPattern("-1is negative|0#is zero or fraction|1#is one|1<is more than 1");
                fail("Expected IllegalArgumentException");
            }
            catch (ArgumentException e)
            {
                // Expected
            }

            f = new ChoiceFormat("");
            f.ApplyPattern("-1<is negative|0#is zero or fraction|1#is one|1<is more than 1");
            choiceLimits[0] = ChoiceFormat.NextDouble(-1);
            assertTrue("Incorrect limits", Array.Equals(f.GetLimits(),
                    choiceLimits));
            assertTrue("Incorrect formats", Array.Equals(f.GetFormats(),
                    choiceFormats));

            f = new ChoiceFormat("");
            f.ApplyPattern("-1#is negative|0#is zero or fraction|1#is one|1<is more than 1");
            String str = "org.apache.harmony.tests.java.text.ChoiceFormat";
            f.ApplyPattern(str);
            String ptrn = f.ToPattern();
            assertEquals("Return value should be empty string for invalid pattern",
                    0, ptrn.Length);
        }

        /**
         * @tests java.text.ChoiceFormat#clone()
         */
        [Test]
        public void test_clone()
        {
            // Test for method java.lang.Object java.text.ChoiceFormat.clone()
            ChoiceFormat f = (ChoiceFormat)f1.Clone();
            assertTrue("Not equal", f.Equals(f1));
            f.SetChoices(new double[] { 0, 1, 2 }, new String[] { "0", "1", "2" });
            assertTrue("Equal", !f.Equals(f1));
        }

        /**
         * @tests java.text.ChoiceFormat#equals(java.lang.Object)
         */
        [Test]
        [Ignore("ICU4N TODO: Implement DecimalFormat & RuleBasedNumberFormat")]
        public void test_equalsLjava_lang_Object()
        {
            // Test for method boolean
            // java.text.ChoiceFormat.equals(java.lang.Object)

            String patternString = "-2#Inverted Orange| 0#No Orange| 0<Almost No Orange| 1#Normal Orange| 2#Expensive Orange";
            double[] appleLimits = { 1, 2, 3, 4, 5 };
            String[] appleFormats = { "Tiny Apple", "Small Apple", "Medium Apple",
                "Large Apple", "Huge Apple" };
            double[] orangeLimits = { -2, 0, ChoiceFormat.NextDouble(0), 1, 2 };
            String[] orangeFormats = { "Inverted Orange", "No Orange",
                "Almost No Orange", "Normal Orange", "Expensive Orange" };

            ChoiceFormat appleChoiceFormat = new ChoiceFormat(appleLimits,
                    appleFormats);
            ChoiceFormat orangeChoiceFormat = new ChoiceFormat(orangeLimits,
                    orangeFormats);
            ChoiceFormat orangeChoiceFormat2 = new ChoiceFormat(patternString);
            ChoiceFormat hybridChoiceFormat = new ChoiceFormat(appleLimits,
                    orangeFormats);

            assertTrue("Apples should not equal oranges", !appleChoiceFormat
                    .Equals(orangeChoiceFormat));
            assertTrue("Different limit list--should not appear as equal",
                    !orangeChoiceFormat.Equals(hybridChoiceFormat));
            assertTrue("Different format list--should not appear as equal",
                    !appleChoiceFormat.Equals(hybridChoiceFormat));
            assertTrue("Should be equal--identical format", appleChoiceFormat
                    .Equals(appleChoiceFormat));
            assertTrue("Should be equals--same limits, same formats",
                    orangeChoiceFormat.Equals(orangeChoiceFormat2));

            ChoiceFormat f2 = new ChoiceFormat(
                    "0#Less than one|1#one|1<Between one and two|2<Greater than two");
            assertTrue("Not equal", f1.Equals(f2));
        }

        /**
         * @tests java.text.ChoiceFormat#format(double, java.lang.StringBuffer,
         *        java.text.FieldPosition)
         */
        [Test]
        [Ignore("ICU4N TODO: Implement DecimalFormat & RuleBasedNumberFormat")]
        public void test_formatDLjava_lang_StringBufferLjava_text_FieldPosition()
        {
            // Test for method java.lang.StringBuffer
            // java.text.ChoiceFormat.format(double, java.lang.StringBuffer,
            // java.text.FieldPosition)
            FieldPosition field = new FieldPosition(0);
            StringBuffer buf = new StringBuffer();
            String r = f1.Format(-1, buf, field).ToString();
            assertEquals("Wrong choice for -1", "Less than one", r);
            buf.Length = 0;
            r = f1.Format(0, buf, field).ToString();
            assertEquals("Wrong choice for 0", "Less than one", r);
            buf.Length = 0;
            r = f1.Format(1, buf, field).ToString();
            assertEquals("Wrong choice for 1", "one", r);
            buf.Length = 0;
            r = f1.Format(2, buf, field).ToString();
            assertEquals("Wrong choice for 2", "Between one and two", r);
            buf.Length = 0;
            r = f1.Format(3, buf, field).ToString();
            assertEquals("Wrong choice for 3", "Greater than two", r);

            // Regression test for HARMONY-1081
            assertEquals("", 0, new ChoiceFormat("|").Format(double.NaN, new StringBuffer(), new FieldPosition(6)).Length);
            assertEquals("", 0, new ChoiceFormat("|").Format(1, new StringBuffer(), new FieldPosition(6)).Length);
            assertEquals("", "Less than one", f1.Format(double.NaN, new StringBuffer(), field).ToString());
        }

        /**
         * @tests java.text.ChoiceFormat#format(long, java.lang.StringBuffer,
         *        java.text.FieldPosition)
         */
        [Test]
        public void test_formatJLjava_lang_StringBufferLjava_text_FieldPosition()
        {
            // Test for method java.lang.StringBuffer
            // java.text.ChoiceFormat.format(long, java.lang.StringBuffer,
            // java.text.FieldPosition)
            FieldPosition field = new FieldPosition(0);
            StringBuffer buf = new StringBuffer();
            String r = f1.Format(0.5, buf, field).ToString();
            assertEquals("Wrong choice for 0.5", "Less than one", r);
            buf.Length=(0);
            r = f1.Format(1.5, buf, field).ToString();
            assertEquals("Wrong choice for 1.5", "Between one and two", r);
            buf.Length=(0);
            r = f1.Format(2.5, buf, field).ToString();
            assertEquals("Wrong choice for 2.5", "Greater than two", r);
        }

        /**
         * @tests java.text.ChoiceFormat#getFormats()
         */
        [Test]
        public void test_getFormats()
        {
            // Test for method java.lang.Object []
            // java.text.ChoiceFormat.getFormats()
            String[] orgFormats = (String[])formats.Clone();
            String[] f = (String[])f1.GetFormats();
            assertTrue("Wrong formats", f.Equals(formats));
            f[0] = "Modified";
            assertTrue("Formats copied", !f.Equals(orgFormats));
        }

        /**
         * @tests java.text.ChoiceFormat#getLimits()
         */
        [Test]
        public void test_getLimits()
        {
            // Test for method double [] java.text.ChoiceFormat.getLimits()
            double[] orgLimits = (double[])limits.Clone();
            double[] l = f1.GetLimits();
            assertTrue("Wrong limits", l.Equals(limits));
            l[0] = 3.14527;
            assertTrue("Limits copied", !l.Equals(orgLimits));
        }

        /**
         * @tests java.text.ChoiceFormat#hashCode()
         */
        [Test]
        [Ignore("ICU4N TODO: Implement DecimalFormat & RuleBasedNumberFormat")]
        public void test_hashCode()
        {
            // Test for method int java.text.ChoiceFormat.hashCode()
            ChoiceFormat f2 = new ChoiceFormat(
                    "0#Less than one|1#one|1<Between one and two|2<Greater than two");
            assertTrue("Different hash", f1.GetHashCode() == f2.GetHashCode());
        }

        /**
         * @tests java.text.ChoiceFormat#nextDouble(double)
         */
        [Test]
        public void test_nextDoubleD()
        {
            // Test for method double java.text.ChoiceFormat.nextDouble(double)
            assertTrue("Not greater 5", ChoiceFormat.NextDouble(5) > 5);
            assertTrue("Not greater 0", ChoiceFormat.NextDouble(0) > 0);
            assertTrue("Not greater -5", ChoiceFormat.NextDouble(-5) > -5);
            assertTrue("Not NaN", double.IsNaN(ChoiceFormat.NextDouble(double.NaN)));
        }

        /**
         * @tests java.text.ChoiceFormat#nextDouble(double, boolean)
         */
        [Test]
        public void test_nextDoubleDZ()
        {
            // Test for method double java.text.ChoiceFormat.nextDouble(double,
            // boolean)
            assertTrue("Not greater 0", ChoiceFormat.NextDouble(0, true) > 0);
            assertTrue("Not less 0", ChoiceFormat.NextDouble(0, false) < 0);
        }

        /**
         * @tests java.text.ChoiceFormat#parse(java.lang.String,
         *        java.text.ParsePosition)
         */
        [Test]
        [Ignore("ICU4N TODO: Implement DecimalFormat & RuleBasedNumberFormat")]
        public void test_parseLjava_lang_StringLjava_text_ParsePosition()
        {
            // Test for method java.lang.Number
            // java.text.ChoiceFormat.parse(java.lang.String,
            // java.text.ParsePosition)
            ChoiceFormat format = new ChoiceFormat("1#one|2#two|3#three");
            assertEquals("Case insensitive", 0, (int)format
                    .Parse("One", new ParsePosition(0)));

            ParsePosition pos = new ParsePosition(0);
            //Number result = f1.Parse("Greater than two", pos);
            double result = f1.Parse("Greater than two", pos);
            //assertTrue("Not a Double1", result is Double); // In .NET, double is a value type
            assertTrue("Wrong value ~>2", result == ChoiceFormat
                    .NextDouble(2));
            assertEquals("Wrong position ~16", 16, pos.Index);
            pos = new ParsePosition(0);
            assertTrue("Incorrect result", double.IsNaN(f1.Parse("12one", pos)));
            assertEquals("Wrong position ~0", 0, pos.Index);
            pos = new ParsePosition(2);
            result = f1.Parse("12one and two", pos);
            //assertTrue("Not a Double2", result is Double); // In .NET, double is a value type
            assertEquals("Ignored parse position", 1.0D, result, 0.0D);
            assertEquals("Wrong position ~5", 5, pos.Index);
        }

        /**
         * @tests java.text.ChoiceFormat#previousDouble(double)
         */
        [Test]
        public void test_previousDoubleD()
        {
            // Test for method double java.text.ChoiceFormat.previousDouble(double)
            assertTrue("Not less 5", ChoiceFormat.PreviousDouble(5) < 5);
            assertTrue("Not less 0", ChoiceFormat.PreviousDouble(0) < 0);
            assertTrue("Not less -5", ChoiceFormat.PreviousDouble(-5) < -5);
            assertTrue("Not NaN", double.IsNaN(ChoiceFormat
                    .PreviousDouble(double.NaN)));
        }

        /**
         * @tests java.text.ChoiceFormat#setChoices(double[], java.lang.String[])
         */
        [Test]
        public void test_setChoices_D_Ljava_lang_String()
        {
            // Test for method void java.text.ChoiceFormat.setChoices(double [],
            // java.lang.String [])
            ChoiceFormat f = (ChoiceFormat)f1.Clone();
            double[] l = new double[] { 0, 1 };
            String[] fs = new String[] { "0", "1" };
            f.SetChoices(l, fs);
            assertTrue("Limits copied", f.GetLimits() == l);
            assertTrue("Formats copied", f.GetFormats() == fs);
        }


        /**
         * @tests java.text.ChoiceFormat#toPattern()
         */
        [Test]
        [Ignore("ICU4N TODO: Implement DecimalFormat & RuleBasedNumberFormat")]
        public void test_toPattern()
        {
            // Regression for HARMONY-59
            ChoiceFormat cf = new ChoiceFormat("");
            assertEquals("", "", cf.ToPattern());

            cf = new ChoiceFormat("-1#NEGATIVE_ONE|0#ZERO|1#ONE|1<GREATER_THAN_ONE");
            assertEquals("", "-1.0#NEGATIVE_ONE|0.0#ZERO|1.0#ONE|1.0<GREATER_THAN_ONE",
                    cf.ToPattern());

            MessageFormat mf = new MessageFormat("CHOICE {1,choice}");
            String ptrn = mf.ToPattern();
            assertEquals("Unused message format returning incorrect pattern", "CHOICE {1,choice,}", ptrn
                    );

            String pattern = f1.ToPattern();
            assertTrue(
                    "Wrong pattern: " + pattern,
                    pattern
                            .Equals("0.0#Less than one|1.0#one|1.0<Between one and two|2.0<Greater than two"));

            cf = new ChoiceFormat(
                    "-1#is negative| 0#is zero or fraction | 1#is one |1.0<is 1+|2#is two |2<is more than 2.");
            String str = "org.apache.harmony.tests.java.lang.share.MyResources2";
            cf.ApplyPattern(str);
            ptrn = cf.ToPattern();
            assertEquals("Return value should be empty string for invalid pattern",
                    0, ptrn.Length);
        }

        /**
         * @tests java.text.ChoiceFormat#format(long)
         */
        [Test]
        [Ignore("ICU4N TODO: Implement DecimalFormat & RuleBasedNumberFormat")]
        public void test_formatL()
        {
            ChoiceFormat fmt = new ChoiceFormat(
                    "-1#NEGATIVE_ONE|0#ZERO|1#ONE|1<GREATER_THAN_ONE");

            assertEquals("", "NEGATIVE_ONE", fmt.Format(long.MinValue));
            assertEquals("", "NEGATIVE_ONE", fmt.Format(-1));
            assertEquals("", "ZERO", fmt.Format(0));
            assertEquals("", "ONE", fmt.Format(1));
            assertEquals("", "GREATER_THAN_ONE", fmt.Format(long.MaxValue));
        }

        /**
         * @tests java.text.ChoiceFormat#format(double)
         */
        [Test]
        [Ignore("ICU4N TODO: Implement DecimalFormat & RuleBasedNumberFormat")]
        public void test_formatD()
        {
            ChoiceFormat fmt = new ChoiceFormat(
                    "-1#NEGATIVE_ONE|0#ZERO|1#ONE|1<GREATER_THAN_ONE");
            assertEquals("", "NEGATIVE_ONE", fmt.Format(double.NegativeInfinity));
            assertEquals("", "NEGATIVE_ONE", fmt.Format(-999999999D));
            assertEquals("", "NEGATIVE_ONE", fmt.Format(-1.1));
            assertEquals("", "NEGATIVE_ONE", fmt.Format(-1.0));
            assertEquals("", "NEGATIVE_ONE", fmt.Format(-0.9));
            assertEquals("", "ZERO", fmt.Format(0.0));
            assertEquals("", "ZERO", fmt.Format(0.9));
            assertEquals("", "ONE", fmt.Format(1.0));
            assertEquals("", "GREATER_THAN_ONE", fmt.Format(1.1));
            assertEquals("", "GREATER_THAN_ONE", fmt.Format(999999999D));
            assertEquals("", "GREATER_THAN_ONE", fmt.Format(double.PositiveInfinity));
        }
    }
}
