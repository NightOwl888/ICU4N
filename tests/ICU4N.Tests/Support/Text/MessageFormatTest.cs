using ICU4N.Dev.Test;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Threading;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Support.Text
{
    public class MessageFormatTest : TestFmwk
    {
        private MessageFormat format1, format2, format3;

        private CultureInfo defaultLocale;

        [Test]
        public virtual void TestBasic()
        {
            var pattern = "{0,choice,0#|1#{1}|2#{1} to {2}}";
            var format = new MessageFormat(pattern, CultureInfo.InvariantCulture);
            object[] args = new object[] { 2, "Any", "Hex Escape" };

            var result = format.Format(args);
        }

        //    //private void checkSerialization(MessageFormat format)
        //    //{
        //    //    try
        //    //    {
        //    //        MemoryStream @out = new MemoryStream();
        //    //        //ObjectOutputStream out = new ObjectOutputStream(ba);
        //    //        @out.writeObject(format);
        //    //        @out.close();
        //    //        ObjectInputStream in = new ObjectInputStream(
        //    //                new ByteArrayInputStream(ba.toByteArray()));
        //    //        MessageFormat read = (MessageFormat) in.readObject();
        //    //        assertTrue("Not equal: " + format.toPattern(), format.equals(read));
        //    //    }
        //    //    catch (IOException e)
        //    //    {
        //    //        fail("Format: " + format.toPattern()
        //    //                + " caused IOException: " + e);
        //    //    }
        //    //    catch (ClassNotFoundException e)
        //    //    {
        //    //        fail("Format: " + format.toPattern()
        //    //                + " caused ClassNotFoundException: " + e);
        //    //    }
        //    //}

        //    ///**
        //    // * @tests java.text.MessageFormat#MessageFormat(java.lang.String,
        //    // *        java.util.Locale)
        //    // */
        //    // [Test]
        //    //public void Test_ConstructorLjava_lang_StringLjava_util_Locale()
        //    //{
        //    //    // Test for method java.text.MessageFormat(java.lang.String,
        //    //    // java.util.Locale)
        //    //    CultureInfo mk = new CultureInfo("mk-MK");
        //    //    MessageFormat format = new MessageFormat(
        //    //            "Date: {0,date} Currency: {1, number, currency} Integer: {2, number, integer}",
        //    //            mk);

        //    //    assertTrue("Wrong locale1", format.Culture.Equals(mk));
        //    //    assertTrue("Wrong locale2", format.GetFormats()[0].Equals(DateFormat
        //    //            .getDateInstance(DateFormat.DEFAULT, mk)));
        //    //    assertTrue("Wrong locale3", format.GetFormats()[1].Equals(NumberFormat
        //    //            .getCurrencyInstance(mk)));
        //    //    assertTrue("Wrong locale4", format.GetFormats()[2].Equals(NumberFormat
        //    //            .getIntegerInstance(mk)));
        //    //}

        //    /**
        //     * @tests java.text.MessageFormat#MessageFormat(java.lang.String)
        //     */
        //    [Test]
        //    public void Test_ConstructorLjava_lang_String()
        //    {
        //        // Test for method java.text.MessageFormat(java.lang.String)
        //        MessageFormat format = new MessageFormat(
        //            " jkl {1,choice,0#low|1#high} mnop {0}");
        //        //"abc {4,time} def {3,date} ghi {2,number} jkl {1,choice,0#low|1#high} mnop {0}");
        //        assertTrue("Not a MessageFormat",
        //                format.GetType() == typeof(MessageFormat));
        //        Formatter[] formats = format.GetFormats();
        //        assertNotNull("null formats", formats);
        //        assertTrue("Wrong format count: " + formats.Length, formats.Length >= 5);
        //        //assertTrue("Wrong time format", formats[0].Equals(DateFormat
        //        //        .GetTimeInstance()));
        //        //assertTrue("Wrong date format", formats[1].Equals(DateFormat
        //        //        .GetDateInstance()));
        //        //assertTrue("Wrong number format", formats[2].Equals(NumberFormat
        //        //        .GetInstance()));
        //        assertTrue("Wrong choice format", formats[3].Equals(new ChoiceFormat(
        //                "0.0#low|1.0#high")));
        //        assertNull("Wrong string format", formats[4]);

        //        DateTime date = new DateTime();
        //        FieldPosition pos = new FieldPosition(-1);
        //        StringBuffer buffer = new StringBuffer();
        //        format.Format(new Object[] { "123", new Double(1.6), new Double(7.2),
        //            date, date
        //            }, buffer, pos);
        //        String result = buffer.ToString();
        //        buffer.Length = (0);
        //        //buffer.Append("abc ");
        //        //buffer.Append(DateFormat.GetTimeInstance().format(date));
        //        //buffer.Append(" def ");
        //        //buffer.Append(DateFormat.GetDateInstance().format(date));
        //        //buffer.Append(" ghi ");
        //        //buffer.Append(NumberFormat.GetInstance().format(new Double(7.2)));
        //        buffer.Append(" jkl high mnop 123");
        //        assertTrue("Wrong answer:\n" + result + "\n" + buffer, result
        //                .Equals(buffer.ToString()));

        //        assertEquals("Simple string", "Test message", new MessageFormat("Test message").Format(

        //                new Object[0]));

        //        result = new MessageFormat("Don't").Format(new Object[0]);
        //        assertTrue("Should not throw IllegalArgumentException: " + result,
        //                    "Dont".Equals(result));

        //        try
        //        {
        //            new MessageFormat("Invalid {1,foobar} format descriptor!");
        //            fail("Expected test_ConstructorLjava_lang_String to throw IAE.");
        //        }
        //        catch (ArgumentException ex)
        //        {
        //            // expected
        //        }

        //        try
        //        {
        //            new MessageFormat(
        //                    "Invalid {1,date,invalid-spec} format descriptor!");
        //        }
        //        catch (ArgumentException ex)
        //        {
        //            // expected
        //        }

        //        //CheckSerialization(new MessageFormat(""));
        //        //CheckSerialization(new MessageFormat("noargs"));
        //        //CheckSerialization(new MessageFormat("{0}"));
        //        //CheckSerialization(new MessageFormat("a{0}"));
        //        //CheckSerialization(new MessageFormat("{0}b"));
        //        //CheckSerialization(new MessageFormat("a{0}b"));

        //        // Regression for HARMONY-65
        //        try
        //        {
        //            new MessageFormat("{0,number,integer");
        //            fail("Assert 0: Failed to detect unmatched brackets.");
        //        }
        //        catch (ArgumentException e)
        //        {
        //            // expected
        //        }
        //    }

        //    /**
        //     * @tests java.text.MessageFormat#applyPattern(java.lang.String)
        //     */
        //    [Test]
        //    public void Test_applyPatternLjava_lang_String()
        //    {
        //        // Test for method void
        //        // java.text.MessageFormat.applyPattern(java.lang.String)
        //        MessageFormat format = new MessageFormat("test");
        //        format.ApplyPattern("xx {0}");
        //        assertEquals("Invalid number", "xx 46", format.Format(
        //                new Object[] { new Integer(46) }));
        //        DateTime date = new DateTime();
        //        string result = format.Format(new object[] { date });
        //        string expected = "xx " + DateFormat.GetInstance().Format(date);
        //        assertTrue("Invalid date:\n" + result + "\n" + expected, result
        //                .Equals(expected));
        //        format = new MessageFormat("{0,date}{1,time}{2,number,integer}");
        //        format.ApplyPattern("nothing");
        //        assertEquals("Found formats", "nothing", format.ToPattern());

        //        format.ApplyPattern("{0}");
        //        assertNull("Wrong format", format.GetFormats()[0]);
        //        assertEquals("Wrong pattern", "{0}", format.ToPattern());

        //        format.ApplyPattern("{0, \t\u001ftime }");
        //        assertTrue("Wrong time format", format.GetFormats()[0]
        //                .Equals(DateFormat.GetTimeInstance()));
        //        assertEquals("Wrong time pattern", "{0,time}", format.ToPattern());
        //        format.ApplyPattern("{0,Time, Short\n}");
        //        assertTrue("Wrong short time format", format.GetFormats()[0]
        //                .Equals(DateFormat.GetTimeInstance(DateFormat.SHORT)));
        //        assertEquals("Wrong short time pattern",
        //                "{0,time,short}", format.ToPattern());
        //        format.ApplyPattern("{0,TIME,\nmedium  }");
        //        assertTrue("Wrong medium time format", format.GetFormats()[0]
        //                .Equals(DateFormat.getTimeInstance(DateFormat.MEDIUM)));
        //        assertEquals("Wrong medium time pattern",
        //                "{0,time}", format.ToPattern());
        //        format.ApplyPattern("{0,time,LONG}");
        //        assertTrue("Wrong long time format", format.GetFormats()[0]
        //                .Equals(DateFormat.getTimeInstance(DateFormat.LONG)));
        //        assertEquals("Wrong long time pattern",
        //                "{0,time,long}", format.ToPattern());
        //        format.SetCulture(new CultureInfo("fr")/*Locale.FRENCH*/); // use French since English has the
        //                                                                   // same LONG and FULL time patterns
        //        format.ApplyPattern("{0,time, Full}");
        //        assertTrue("Wrong full time format", format.GetFormats()[0]
        //                .Equals(DateFormat.getTimeInstance(DateFormat.FULL,
        //                        new CultureInfo("fr")/*Locale.FRENCH*/)));
        //        assertEquals("Wrong full time pattern",
        //                "{0,time,full}", format.ToPattern());
        //        format.SetCulture(CultureInfo.CurrentCulture);

        //        format.ApplyPattern("{0, date}");
        //        assertTrue("Wrong date format", format.GetFormats()[0]
        //                .Equals(DateFormat.GetDateInstance()));
        //        assertEquals("Wrong date pattern", "{0,date}", format.ToPattern());
        //        format.ApplyPattern("{0, date, short}");
        //        assertTrue("Wrong short date format", format.GetFormats()[0]
        //                .Equals(DateFormat.GetDateInstance(DateFormat.SHORT)));
        //        assertEquals("Wrong short date pattern",
        //                "{0,date,short}", format.ToPattern());
        //        format.ApplyPattern("{0, date, medium}");
        //        assertTrue("Wrong medium date format", format.GetFormats()[0]
        //                .Equals(DateFormat.GetDateInstance(DateFormat.MEDIUM)));
        //        assertEquals("Wrong medium date pattern",
        //                "{0,date}", format.ToPattern());
        //        format.ApplyPattern("{0, date, long}");
        //        assertTrue("Wrong long date format", format.GetFormats()[0]
        //                .Equals(DateFormat.GetDateInstance(DateFormat.LONG)));
        //        assertEquals("Wrong long date pattern",
        //                "{0,date,long}", format.ToPattern());
        //        format.ApplyPattern("{0, date, full}");
        //        assertTrue("Wrong full date format", format.GetFormats()[0]
        //                .Equals(DateFormat.GetDateInstance(DateFormat.FULL)));
        //        assertEquals("Wrong full date pattern",
        //                "{0,date,full}", format.ToPattern());

        //        format.ApplyPattern("{0, date, MMM d {hh:mm:ss}}");
        //        assertEquals("Wrong time/date format", " MMM d {hh:mm:ss}", ((SimpleDateFormat)(format
        //                .GetFormats()[0])).toPattern());
        //        assertEquals("Wrong time/date pattern",
        //                "{0,date, MMM d {hh:mm:ss}}", format.ToPattern());

        //        format.ApplyPattern("{0, number}");
        //        assertTrue("Wrong number format", format.GetFormats()[0]
        //                .Equals(NumberFormat.GetNumberInstance()));
        //        assertEquals("Wrong number pattern",
        //                "{0,number}", format.ToPattern());
        //        format.ApplyPattern("{0, number, currency}");
        //        assertTrue("Wrong currency number format", format.GetFormats()[0]
        //                .Equals(NumberFormat.GetCurrencyInstance()));
        //        assertEquals("Wrong currency number pattern",
        //                "{0,number,currency}", format.ToPattern());
        //        format.ApplyPattern("{0, number, percent}");
        //        assertTrue("Wrong percent number format", format.GetFormats()[0]
        //                .Equals(NumberFormat.GetPercentInstance()));
        //        assertEquals("Wrong percent number pattern",
        //                "{0,number,percent}", format.ToPattern());
        //        format.ApplyPattern("{0, number, integer}");
        //        NumberFormat nf = NumberFormat.GetInstance();
        //        nf.setMaximumFractionDigits(0);
        //        nf.setParseIntegerOnly(true);
        //        assertTrue("Wrong integer number format", format.GetFormats()[0]
        //                .Equals(nf));
        //        assertEquals("Wrong integer number pattern",
        //                "{0,number,integer}", format.ToPattern());

        //        format.ApplyPattern("{0, number, {'#'}##0.0E0}");

        //        /*
        //         * TODO validate these assertions 
        //         * String actual = ((DecimalFormat)(format.getFormats()[0])).toPattern(); 
        //         * assertEquals("Wrong pattern number format", "' {#}'##0.0E0", actual); 
        //         * assertEquals("Wrong pattern number pattern", "{0,number,' {#}'##0.0E0}", format.toPattern());
        //         * 
        //         */

        //        format.ApplyPattern("{0, choice,0#no|1#one|2#{1,number}}");
        //        assertEquals("Wrong choice format",

        //                        "0.0#no|1.0#one|2.0#{1,number}", ((ChoiceFormat)format.GetFormats()[0]).ToPattern());
        //        assertEquals("Wrong choice pattern",
        //                "{0,choice,0.0#no|1.0#one|2.0#{1,number}}", format.ToPattern());
        //        assertEquals("Wrong formatted choice", "3.6", format.Format(
        //                new Object[] { J2N.Numerics.Int32.GetInstance(2), J2N.Numerics.Single.GetInstance(3.6f) }));

        //        try
        //        {
        //            format.ApplyPattern("WRONG MESSAGE FORMAT {0,number,{}");
        //            fail("Expected IllegalArgumentException for invalid pattern");
        //        }
        //        catch (ArgumentException e)
        //        {
        //        }

        //        // Regression for HARMONY-65
        //        MessageFormat mf = new MessageFormat("{0,number,integer}");
        //        String badpattern = "{0,number,#";
        //        try
        //        {
        //            mf.ApplyPattern(badpattern);
        //            fail("Assert 0: Failed to detect unmatched brackets.");
        //        }
        //        catch (ArgumentException e)
        //        {
        //            // expected
        //        }
        //    }

        //    /**
        //     * @tests java.text.MessageFormat#clone()
        //     */
        //    [Test]
        //    public void Test_clone()
        //    {
        //        // Test for method java.lang.Object java.text.MessageFormat.clone()
        //        MessageFormat format = new MessageFormat("'{'choice'}'{0}");
        //        MessageFormat clone = (MessageFormat)format.Clone();
        //        assertTrue("Clone not equal", format.Equals(clone));
        //        assertEquals("Wrong answer",
        //                "{choice}{0}", format.Format(new object[] { }));
        //        clone.SetFormat(0, DateFormat.GetInstance());
        //        assertTrue("Clone shares format data", !format.Equals(clone));
        //        format = (MessageFormat)clone.Clone();
        //        Formatter[] formats = clone.GetFormats();
        //        ((SimpleDateFormat)formats[0]).ApplyPattern("adk123");
        //        assertTrue("Clone shares format data", !format.Equals(clone));
        //    }

        //    /**
        //     * @tests java.text.MessageFormat#equals(java.lang.Object)
        //     */
        //    [Test]
        //    public void Test_equalsLjava_lang_Object()
        //    {
        //        // Test for method boolean
        //        // java.text.MessageFormat.equals(java.lang.Object)
        //        MessageFormat format1 = new MessageFormat("{0}");
        //        MessageFormat format2 = new MessageFormat("{1}");
        //        assertTrue("Should not be equal", !format1.Equals(format2));
        //        format2.ApplyPattern("{0}");
        //        assertTrue("Should be equal", format1.Equals(format2));
        //        SimpleDateFormat date = (SimpleDateFormat)DateFormat.GetTimeInstance();
        //        format1.SetFormat(0, DateFormat.GetTimeInstance());
        //        format2.SetFormat(0, new SimpleDateFormat(date.ToPattern()));
        //        assertTrue("Should be equal2", format1.Equals(format2));
        //    }

        //    /**
        //     * @tests java.text.MessageFormat#hashCode()
        //     */
        //    [Test]
        //    public void Test_hashCode()
        //    {
        //        // Test for method
        //        // int java.text.MessageFormat.hashCode()
        //        assertEquals("Should be equal", 3648, new MessageFormat("rr", null).GetHashCode());
        //    }

        //    /**
        //     * @tests java.text.MessageFormat#formatToCharacterIterator(java.lang.Object)
        //     */
        //    [Test]
        //    //FIXME This test fails on Harmony ClassLibrary
        //    public void failing_test_formatToCharacterIteratorLjava_lang_Object()
        //    {
        //        // Test for method formatToCharacterIterator(java.lang.Object)
        //        new Support_MessageFormat("test_formatToCharacterIteratorLjava_lang_Object").t_formatToCharacterIterator();
        //    }

        //    /**
        //     * @tests java.text.MessageFormat#format(java.lang.Object[],
        //     *        java.lang.StringBuffer, java.text.FieldPosition)
        //     */
        //    [Test]
        //    public void Test_format_Ljava_lang_ObjectLjava_lang_StringBufferLjava_text_FieldPosition()
        //    {
        //        // Test for method java.lang.StringBuffer
        //        // java.text.MessageFormat.format(java.lang.Object [],
        //        // java.lang.StringBuffer, java.text.FieldPosition)
        //        MessageFormat format = new MessageFormat("{1,number,integer}");
        //        StringBuffer buffer = new StringBuffer();
        //        format.Format(new object[] { "0", new Double(53.863) }, buffer,
        //                new FieldPosition(0));
        //        assertEquals("Wrong result", "54", buffer.ToString());
        //        format
        //                .ApplyPattern("{0,choice,0#zero|1#one '{1,choice,2#two {2,time}}'}");
        //        DateTime date = new DateTime();
        //        string expected = "one two "
        //                + DateFormat.GetTimeInstance().Format(date);
        //        string result = format.Format(new object[] { new Double(1.6),
        //            new Integer(3), date });
        //        assertTrue("Choice not recursive:\n" + expected + "\n" + result,
        //                expected.Equals(result));
        //    }

        //    /**
        //     * @tests java.text.MessageFormat#format(java.lang.Object,
        //     *        java.lang.StringBuffer, java.text.FieldPosition)
        //     */
        //    public void Test_formatLjava_lang_ObjectLjava_lang_StringBufferLjava_text_FieldPosition()
        //    {
        //        // Test for method java.lang.StringBuffer
        //        // java.text.MessageFormat.format(java.lang.Object,
        //        // java.lang.StringBuffer, java.text.FieldPosition)
        //        new Support_MessageFormat(
        //                "test_formatLjava_lang_ObjectLjava_lang_StringBufferLjava_text_FieldPosition")
        //                .t_format_with_FieldPosition();
        //    }

        //    /**
        //     * @tests java.text.MessageFormat#getFormats()
        //     */
        //    [Test]
        //    public void Test_getFormats()
        //    {
        //        // Test for method java.text.Format []
        //        // java.text.MessageFormat.getFormats()

        //        // test with repeating formats and max argument index < max offset
        //        Formatter[] formats = format1.GetFormats();
        //        Formatter[] correctFormats = new Formatter[] {
        //            NumberFormat.GetCurrencyInstance(),
        //            DateFormat.GetTimeInstance(),
        //            NumberFormat.GetPercentInstance(), null,
        //            new ChoiceFormat("0#off|1#on"), DateFormat.GetDateInstance(), };

        //        assertEquals("Test1:Returned wrong number of formats:",
        //                correctFormats.Length, formats.Length);
        //        for (int i = 0; i < correctFormats.Length; i++)
        //        {
        //            assertEquals("Test1:wrong format for pattern index " + i + ":",
        //                    correctFormats[i], formats[i]);
        //        }

        //        // test with max argument index > max offset
        //        formats = format2.GetFormats();
        //        correctFormats = new Formatter[] { NumberFormat.GetCurrencyInstance(),
        //            DateFormat.GetTimeInstance(),
        //            NumberFormat.GetPercentInstance(), null,
        //            new ChoiceFormat("0#off|1#on"), DateFormat.GetDateInstance() };

        //        assertEquals("Test2:Returned wrong number of formats:",
        //                correctFormats.Length, formats.Length);
        //        for (int i = 0; i < correctFormats.Length; i++)
        //        {
        //            assertEquals("Test2:wrong format for pattern index " + i + ":",
        //                    correctFormats[i], formats[i]);
        //        }

        //        // test with argument number being zero
        //        formats = format3.GetFormats();
        //        assertEquals("Test3: Returned wrong number of formats:", 0,
        //                formats.Length);
        //    }

        //    /**
        //     * @tests java.text.MessageFormat#getFormatsByArgumentIndex()
        //     */
        //    [Test]
        //    public void Test_getFormatsByArgumentIndex()
        //    {
        //        // Test for method java.text.Format [] test_getFormatsByArgumentIndex()

        //        // test with repeating formats and max argument index < max offset
        //        Formatter[] formats = format1.GetFormatsByArgumentIndex();
        //        Formatter[] correctFormats = new Formatter[] { DateFormat.GetDateInstance(),
        //            new ChoiceFormat("0#off|1#on"), DateFormat.GetTimeInstance(),
        //            NumberFormat.GetCurrencyInstance(), null };

        //        assertEquals("Test1:Returned wrong number of formats:",
        //                correctFormats.Length, formats.Length);
        //        for (int i = 0; i < correctFormats.Length; i++)
        //        {
        //            assertEquals("Test1:wrong format for argument index " + i + ":",
        //                    correctFormats[i], formats[i]);
        //        }

        //        // test with max argument index > max offset
        //        formats = format2.GetFormatsByArgumentIndex();
        //        correctFormats = new Formatter[] { DateFormat.GetDateInstance(),
        //            new ChoiceFormat("0#off|1#on"), null,
        //            NumberFormat.GetCurrencyInstance(), null, null, null, null,
        //            DateFormat.GetTimeInstance() };

        //        assertEquals("Test2:Returned wrong number of formats:",
        //                correctFormats.Length, formats.Length);
        //        for (int i = 0; i < correctFormats.Length; i++)
        //        {
        //            assertEquals("Test2:wrong format for argument index " + i + ":",
        //                    correctFormats[i], formats[i]);
        //        }

        //        // test with argument number being zero
        //        formats = format3.GetFormatsByArgumentIndex();
        //        assertEquals("Test3: Returned wrong number of formats:", 0,
        //                formats.Length);
        //    }

        //    /**
        //     * @tests java.text.MessageFormat#setFormatByArgumentIndex(int,
        //     *        java.text.Format)
        //     */
        //    [Test]
        //    public void Test_setFormatByArgumentIndexILjava_text_Format()
        //    {
        //        // test for method setFormatByArgumentIndex(int, Format)
        //        MessageFormat f1 = (MessageFormat)format1.Clone();
        //        f1.SetFormatByArgumentIndex(0, DateFormat.GetTimeInstance());
        //        f1.SetFormatByArgumentIndex(4, new ChoiceFormat("1#few|2#ok|3#a lot"));

        //        // test with repeating formats and max argument index < max offset
        //        // compare getFormatsByArgumentIndex() results after calls to
        //        // setFormatByArgumentIndex()
        //        Formatter[] formats = f1.GetFormatsByArgumentIndex();

        //        Formatter[] correctFormats = new Formatter[] { DateFormat.getTimeInstance(),
        //            new ChoiceFormat("0#off|1#on"), DateFormat.getTimeInstance(),
        //            NumberFormat.GetCurrencyInstance(),
        //            new ChoiceFormat("1#few|2#ok|3#a lot") };

        //        assertEquals("Test1A:Returned wrong number of formats:",
        //                correctFormats.Length, formats.Length);
        //        for (int i = 0; i < correctFormats.Length; i++)
        //        {
        //            assertEquals("Test1B:wrong format for argument index " + i + ":",
        //                    correctFormats[i], formats[i]);
        //        }

        //        // compare getFormats() results after calls to
        //        // setFormatByArgumentIndex()
        //        formats = f1.GetFormats();

        //        correctFormats = new Formatter[] { NumberFormat.getCurrencyInstance(),
        //            DateFormat.GetTimeInstance(), DateFormat.getTimeInstance(),
        //            new ChoiceFormat("1#few|2#ok|3#a lot"),
        //            new ChoiceFormat("0#off|1#on"), DateFormat.getTimeInstance(), };

        //        assertEquals("Test1C:Returned wrong number of formats:",
        //                correctFormats.Length, formats.Length);
        //        for (int i = 0; i < correctFormats.Length; i++)
        //        {
        //            assertEquals("Test1D:wrong format for pattern index " + i + ":",
        //                    correctFormats[i], formats[i]);
        //        }

        //        // test setting argumentIndexes that are not used
        //        MessageFormat f2 = (MessageFormat)format2.Clone();
        //        f2.SetFormatByArgumentIndex(2, NumberFormat.GetPercentInstance());
        //        f2.SetFormatByArgumentIndex(4, DateFormat.GetTimeInstance());

        //        formats = f2.GetFormatsByArgumentIndex();
        //        correctFormats = format2.GetFormatsByArgumentIndex();

        //        assertEquals("Test2A:Returned wrong number of formats:",
        //                correctFormats.Length, formats.Length);
        //        for (int i = 0; i < correctFormats.Length; i++)
        //        {
        //            assertEquals("Test2B:wrong format for argument index " + i + ":",
        //                    correctFormats[i], formats[i]);
        //        }

        //        formats = f2.GetFormats();
        //        correctFormats = format2.GetFormats();

        //        assertEquals("Test2C:Returned wrong number of formats:",
        //                correctFormats.Length, formats.Length);
        //        for (int i = 0; i < correctFormats.Length; i++)
        //        {
        //            assertEquals("Test2D:wrong format for pattern index " + i + ":",
        //                    correctFormats[i], formats[i]);
        //        }

        //        // test exceeding the argumentIndex number
        //        MessageFormat f3 = (MessageFormat)format3.Clone();
        //        f3.SetFormatByArgumentIndex(1, NumberFormat.getCurrencyInstance());

        //        formats = f3.GetFormatsByArgumentIndex();
        //        assertEquals("Test3A:Returned wrong number of formats:", 0,
        //                formats.Length);

        //        formats = f3.GetFormats();
        //        assertEquals("Test3B:Returned wrong number of formats:", 0,
        //                formats.Length);
        //    }

        //    /**
        //     * @tests java.text.MessageFormat#setFormatsByArgumentIndex(java.text.Format[])
        //     */
        //    [Test]
        //    public void Test_setFormatsByArgumentIndex_Ljava_text_Format()
        //    {
        //        // test for method setFormatByArgumentIndex(Format[])
        //        MessageFormat f1 = (MessageFormat)format1.Clone();

        //        // test with repeating formats and max argument index < max offset
        //        // compare getFormatsByArgumentIndex() results after calls to
        //        // setFormatsByArgumentIndex(Format[])
        //        Formatter[] correctFormats = new Formatter[] { DateFormat.GetTimeInstance(),
        //            new ChoiceFormat("0#off|1#on"), DateFormat.GetTimeInstance(),
        //            NumberFormat.GetCurrencyInstance(),
        //            new ChoiceFormat("1#few|2#ok|3#a lot") };

        //        f1.SetFormatsByArgumentIndex(correctFormats);
        //        Formatter[] formats = f1.GetFormatsByArgumentIndex();

        //        assertEquals("Test1A:Returned wrong number of formats:",
        //                correctFormats.Length, formats.Length);
        //        for (int i = 0; i < correctFormats.Length; i++)
        //        {
        //            assertEquals("Test1B:wrong format for argument index " + i + ":",
        //                    correctFormats[i], formats[i]);
        //        }

        //        // compare getFormats() results after calls to
        //        // setFormatByArgumentIndex()
        //        formats = f1.GetFormats();
        //        correctFormats = new Formatter[] { NumberFormat.GetCurrencyInstance(),
        //            DateFormat.GetTimeInstance(), DateFormat.GetTimeInstance(),
        //            new ChoiceFormat("1#few|2#ok|3#a lot"),
        //            new ChoiceFormat("0#off|1#on"), DateFormat.GetTimeInstance(), };

        //        assertEquals("Test1C:Returned wrong number of formats:",
        //                correctFormats.Length, formats.Length);
        //        for (int i = 0; i < correctFormats.Length; i++)
        //        {
        //            assertEquals("Test1D:wrong format for pattern index " + i + ":",
        //                    correctFormats[i], formats[i]);
        //        }

        //        // test setting argumentIndexes that are not used
        //        MessageFormat f2 = (MessageFormat)format2.Clone();
        //        Formatter[] inputFormats = new Formatter[] { DateFormat.GetDateInstance(),
        //            new ChoiceFormat("0#off|1#on"),
        //            NumberFormat.GetPercentInstance(),
        //            NumberFormat.GetCurrencyInstance(),
        //            DateFormat.GetTimeInstance(), null, null, null,
        //            DateFormat.GetTimeInstance() };
        //        f2.SetFormatsByArgumentIndex(inputFormats);

        //        formats = f2.GetFormatsByArgumentIndex();
        //        correctFormats = format2.GetFormatsByArgumentIndex();

        //        assertEquals("Test2A:Returned wrong number of formats:",
        //                correctFormats.Length, formats.Length);
        //        for (int i = 0; i < correctFormats.Length; i++)
        //        {
        //            assertEquals("Test2B:wrong format for argument index " + i + ":",
        //                    correctFormats[i], formats[i]);
        //        }

        //        formats = f2.GetFormats();
        //        correctFormats = new Formatter[] { NumberFormat.GetCurrencyInstance(),
        //            DateFormat.GetTimeInstance(), DateFormat.GetDateInstance(),
        //            null, new ChoiceFormat("0#off|1#on"),
        //            DateFormat.GetDateInstance() };

        //        assertEquals("Test2C:Returned wrong number of formats:",
        //                correctFormats.Length, formats.Length);
        //        for (int i = 0; i < correctFormats.Length; i++)
        //        {
        //            assertEquals("Test2D:wrong format for pattern index " + i + ":",
        //                    correctFormats[i], formats[i]);
        //        }

        //        // test exceeding the argumentIndex number
        //        MessageFormat f3 = (MessageFormat)format3.Clone();
        //        f3.SetFormatsByArgumentIndex(inputFormats);

        //        formats = f3.GetFormatsByArgumentIndex();
        //        assertEquals("Test3A:Returned wrong number of formats:", 0,
        //                formats.Length);

        //        formats = f3.GetFormats();
        //        assertEquals("Test3B:Returned wrong number of formats:", 0,
        //                formats.Length);

        //    }

        //    /**
        //     * @tests java.text.MessageFormat#parse(java.lang.String,
        //     *        java.text.ParsePosition)
        //     */
        //    [Test]
        //    public void Test_parseLjava_lang_StringLjava_text_ParsePosition()
        //    {
        //        MessageFormat format = new MessageFormat("date is {0,date,MMM d, yyyy}");
        //        ParsePosition pos = new ParsePosition(2);
        //        Object[] result = (Object[])format
        //                .Parse("xxdate is Feb 28, 1999", pos);
        //        assertTrue("No result: " + result.Length, result.Length >= 1);
        //        assertTrue("Wrong answer", ((DateTime)result[0])
        //                .Equals(new DateTime(1999, 2 /*Calendar.FEBRUARY*/, 28)));

        //        MessageFormat mf = new MessageFormat("vm={0},{1},{2}");
        //        result = mf.Parse("vm=win,foo,bar", new ParsePosition(0));
        //        assertTrue("Invalid parse", result[0].Equals("win")
        //                && result[1].Equals("foo") && result[2].Equals("bar"));

        //        mf = new MessageFormat("{0}; {0}; {0}");
        //        String parse = "a; b; c";
        //        result = mf.Parse(parse, new ParsePosition(0));
        //        assertEquals("Wrong variable result", "c", result[0]);

        //        mf = new MessageFormat("before {0}, after {1,number}");
        //        parse = "before you, after 42";
        //        pos.Index = (0);
        //        pos.ErrorIndex = (8);
        //        result = mf.Parse(parse, pos);
        //        assertEquals(string.Empty, 2, result.Length);
        //    }

        //    /**
        //     * @tests java.text.MessageFormat#setLocale(java.util.Locale)
        //     */
        //    [Test]
        //    public void Test_setLocaleLjava_util_Locale()
        //    {
        //        // Test for method void
        //        // java.text.MessageFormat.setLocale(java.util.Locale)
        //        MessageFormat format = new MessageFormat("date {0,date}");
        //        format.SetCulture(new CultureInfo("zh-Hans") /*Locale.CHINA*/);
        //        assertEquals("Wrong locale1", new CultureInfo("zh-Hans") /*Locale.CHINA*/, format.Culture);
        //        format.ApplyPattern("{1,date}");
        //        assertEquals("Wrong locale3", DateFormat.GetDateInstance(DateFormat.DEFAULT,
        //                new CultureInfo("zh-Hans") /*Locale.CHINA*/), format.GetFormats()[0]);
        //    }

        //    /**
        //     * @tests java.text.MessageFormat#toPattern()
        //     */
        //    [Test]
        //    public void Test_toPattern()
        //    {
        //        // Test for method java.lang.String java.text.MessageFormat.toPattern()
        //        String pattern = "[{0}]";
        //        MessageFormat mf = new MessageFormat(pattern);
        //        assertTrue("Wrong pattern", mf.ToPattern().Equals(pattern));

        //        // Regression for HARMONY-59
        //        new MessageFormat("CHOICE {1,choice}").ToPattern();
        //    }

        //    /**
        //     * Sets up the fixture, for example, open a network connection. This method
        //     * is called before a test is executed.
        //     */
        //    [SetUp]
        //    protected void SetUp()
        //    {
        //        defaultLocale = CultureInfo.CurrentCulture;
        //        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

        //        // test with repeating formats and max argument index < max offset
        //        String pattern = "A {3, number, currency} B {2, time} C {0, number, percent} D {4}  E {1,choice,0#off|1#on} F {0, date}";
        //        format1 = new MessageFormat(pattern);

        //        // test with max argument index > max offset
        //        pattern = "A {3, number, currency} B {8, time} C {0, number, percent} D {6}  E {1,choice,0#off|1#on} F {0, date}";
        //        format2 = new MessageFormat(pattern);

        //        // test with argument number being zero
        //        pattern = "A B C D E F";
        //        format3 = new MessageFormat(pattern);
        //    }

        //    /**
        //     * Tears down the fixture, for example, close a network connection. This
        //     * method is called after a test is executed.
        //     */
        //    [TearDown]
        //    protected void tearDown()
        //    {
        //        Thread.CurrentThread.CurrentCulture = defaultLocale;
        //    }

        //    /**
        //     * @tests java.text.MessageFormat(java.util.Locale)
        //     */
        //    [Test]
        //    public void Test_ConstructorLjava_util_Locale()
        //    {
        //        // Regression for HARMONY-65
        //        try
        //        {
        //            new MessageFormat("{0,number,integer", new CultureInfo("en-US") /*Locale.US*/);
        //            fail("Assert 0: Failed to detect unmatched brackets.");
        //        }
        //        catch (ArgumentException e)
        //        {
        //            // expected
        //        }
        //    }

        //    /**
        //     * @tests java.text.MessageFormat#parse(java.lang.String)
        //     */
        //    [Test]
        //    public void Test_parse()
        //    {
        //        // Regression for HARMONY-63
        //        MessageFormat mf = new MessageFormat("{0,number,#,####}", new CultureInfo("en-US") /*Locale.US*/);
        //        Object[] res = mf.Parse("1,00,00");
        //        assertEquals("Assert 0: incorrect size of parsed data ", 1, res.Length);
        //        assertEquals("Assert 1: parsed value incorrectly", new Long(10000), (Long)res[0]);
        //    }

        //    [Test]
        //    public void Test_format_Object()
        //    {
        //        // Regression for HARMONY-1875
        //        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-CA") /*Locale.CANADA*/;
        //        //TimeZone.setDefault(TimeZone.getTimeZone("UTC"));
        //        String pat = "text here {0, date, yyyyyyyyy } and here";
        //        String etalon = "text here  000002007  and here";
        //        MessageFormat obj = new MessageFormat(pat);
        //        assertEquals(string.Empty, etalon, obj.Format(new Object[] { new DateTime(1198141737640L) })); // ICU4N TODO: Convert to Ticks

        //        assertEquals(string.Empty, "{0}", MessageFormat.Format("{0}", (Object[])null));
        //        assertEquals(string.Empty, "nullABC", MessageFormat.Format("{0}{1}", new String[] { null, "ABC" }));
        //    }

        //    [Test]
        //    public void TestHARMONY5323()
        //    {
        //        Object[] messageArgs = new Object[11];
        //        for (int i = 0; i < messageArgs.Length; i++)
        //            messageArgs[i] = "dumb" + i;

        //        String res = MessageFormat.Format("bgcolor=\"{10}\"", messageArgs);
        //        assertEquals(string.Empty, res, "bgcolor=\"dumb10\"");
        //    }
    }
}
