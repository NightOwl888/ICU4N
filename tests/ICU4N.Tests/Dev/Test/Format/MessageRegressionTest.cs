using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using ICU4N.Text;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using Integer = J2N.Numerics.Int32;

namespace ICU4N.Dev.Test.Format
{
    public class MessageRegressionTest : TestFmwk
    {
        /* @bug 4074764
         * Null exception when formatting pattern with MessageFormat
         * with no parameters.
         */
        [Test]
        public void Test4074764()
        {
            String[] pattern = {"Message without param",
                "Message with param:{0}",
                "Longer Message with param {0}"};
            //difference between the two param strings are that
            //in the first one, the param position is within the
            //length of the string without param while it is not so
            //in the other case.

            MessageFormat messageFormatter = new MessageFormat("");

            try
            {
                //Apply pattern with param and print the result
                messageFormatter.ApplyPattern(pattern[1]);
                Object[] paramArray = { "BUG", new DateTime() };
                String tempBuffer = messageFormatter.Format(paramArray);
                if (!tempBuffer.Equals("Message with param:BUG"))
                    Errln("MessageFormat with one param test failed.");
                Logln("Formatted with one extra param : " + tempBuffer);

                //Apply pattern without param and print the result
                messageFormatter.ApplyPattern(pattern[0]);
                tempBuffer = messageFormatter.Format((object)null);
                if (!tempBuffer.Equals("Message without param"))
                    Errln("MessageFormat with no param test failed.");
                Logln("Formatted with no params : " + tempBuffer);

                tempBuffer = messageFormatter.Format(paramArray);
                if (!tempBuffer.Equals("Message without param"))
                    Errln("Formatted with arguments > subsitution failed. result = " + tempBuffer.ToString());
                Logln("Formatted with extra params : " + tempBuffer);
                //This statement gives an exception while formatting...
                //If we use pattern[1] for the message with param,
                //we get an NullPointerException in MessageFormat.java(617)
                //If we use pattern[2] for the message with param,
                //we get an StringArrayIndexOutOfBoundsException in MessageFormat.java(614)
                //Both are due to maxOffset not being reset to -1
                //in applyPattern() when the pattern does not
                //contain any param.
            }
            catch (Exception foo)
            {
                Errln("Exception when formatting with no params.");
            }
        }

        /* @bug 4058973
         * MessageFormat.toPattern has weird rounding behavior.
         *
         * ICU 4.8: This test is commented out because toPattern() has been changed to return
         * the original pattern string, rather than reconstituting a new (equivalent) one.
         * This trivially eliminates issues with rounding or any other pattern string differences.
         */
        /*public void Test4058973() {

            MessageFormat fmt = new MessageFormat("{0,choice,0#no files|1#one file|1< {0,number,integer} files}");
            String pat = fmt.ToPattern();
            if (!pat.Equals("{0,choice,0.0#no files|1.0#one file|1.0< {0,number,integer} files}")) {
                Errln("MessageFormat.toPattern failed");
            }
        }*/
        /* @bug 4031438
         * More robust message formats.
         */
        [Test]
        [Ignore("ICU4N TODO: Finish MessageFormat Implementation")]
        public void Test4031438()
        {
            String pattern1 = "Impossible {1} has occurred -- status code is {0} and message is {2}.";
            String pattern2 = "Double '' Quotes {0} test and quoted '{1}' test plus 'other {2} stuff'.";

            MessageFormat messageFormatter = new MessageFormat("");

            try
            {
                Logln("Apply with pattern : " + pattern1);
                messageFormatter.ApplyPattern(pattern1);
                Object[] paramArray = { (int)7 };
                String tempBuffer = messageFormatter.Format(paramArray);
                if (!tempBuffer.Equals("Impossible {1} has occurred -- status code is 7 and message is {2}."))
                    Errln("Tests arguments < substitution failed");
                Logln("Formatted with 7 : " + tempBuffer);
                ParsePosition status = new ParsePosition(0);
                Object[] objs = messageFormatter.Parse(tempBuffer, status);
                if (objs[paramArray.Length] != null)
                    Errln("Parse failed with more than expected arguments");
                for (int i = 0; i < objs.Length; i++)
                {
                    if (objs[i] != null && !objs[i].ToString().Equals(paramArray[i].ToString()))
                    {
                        Errln("Parse failed on object " + objs[i] + " at index : " + i);
                    }
                }
                tempBuffer = messageFormatter.Format((object)null);
                if (!tempBuffer.Equals("Impossible {1} has occurred -- status code is {0} and message is {2}."))
                    Errln("Tests with no arguments failed");
                Logln("Formatted with null : " + tempBuffer);
                Logln("Apply with pattern : " + pattern2);
                messageFormatter.ApplyPattern(pattern2);
                tempBuffer = messageFormatter.Format(paramArray);
                if (!tempBuffer.Equals("Double ' Quotes 7 test and quoted {1} test plus 'other {2} stuff'."))
                    Errln("quote format test (w/ params) failed.");
                Logln("Formatted with params : " + tempBuffer);
                tempBuffer = messageFormatter.Format((object)null);
                if (!tempBuffer.Equals("Double ' Quotes {0} test and quoted {1} test plus 'other {2} stuff'."))
                    Errln("quote format test (w/ null) failed.");
                Logln("Formatted with null : " + tempBuffer);
                Logln("toPattern : " + messageFormatter.ToPattern());
            }
            catch (Exception foo)
            {
                Warnln("Exception when formatting in bug 4031438. " + foo.Message);
            }
        }
        [Test]
        [Ignore("ICU4N TODO: Finish MessageFormat Implementation")]
        public void Test4052223()
        {
            ParsePosition pos = new ParsePosition(0);
            if (pos.ErrorIndex != -1)
            {
                Errln("ParsePosition.getErrorIndex initialization failed.");
            }
            MessageFormat fmt = new MessageFormat("There are {0} apples growing on the {1} tree.");
            String str = "There is one apple growing on the peach tree.";
            Object[] objs = fmt.Parse(str, pos);
            Logln("unparsable string , should fail at " + pos.ErrorIndex);
            if (pos.ErrorIndex == -1)
                Errln("Bug 4052223 failed : parsing string " + str);
            pos.ErrorIndex = (4);
            if (pos.ErrorIndex != 4)
                Errln("setErrorIndex failed, got " + pos.ErrorIndex + " instead of 4");

            if (objs != null)
            {
                Errln("objs should be null");
            }
            ChoiceFormat f = new ChoiceFormat(
                "-1#are negative|0#are no or fraction|1#is one|1.0<is 1+|2#are two|2<are more than 2.");
            pos.Index = (0); pos.ErrorIndex = (-1);
            /*Number*/
            object obj = f.Parse("are negative", pos);
            if (pos.ErrorIndex != -1 && Convert.ToDouble(obj) == -1.0)
                Errln("Parse with \"are negative\" failed, at " + pos.ErrorIndex);
            pos.Index = (0); pos.ErrorIndex = (-1);
            obj = f.Parse("are no or fraction ", pos);
            if (pos.ErrorIndex != -1 && Convert.ToDouble(obj) == 0.0)
                Errln("Parse with \"are no or fraction\" failed, at " + pos.ErrorIndex);
            pos.Index = (0); pos.ErrorIndex = (-1);
            obj = f.Parse("go postal", pos);
            if (pos.ErrorIndex == -1 && !double.IsNaN(Convert.ToDouble(obj)))
                Errln("Parse with \"go postal\" failed, at " + pos.ErrorIndex);
        }
        /* @bug 4104976
         * ChoiceFormat.Equals(null) throws NullPointerException
         */
        [Test]
        public void Test4104976()
        {
            double[] limits = { 1, 20 };
            String[] formats = { "xyz", "abc" };
            ChoiceFormat cf = new ChoiceFormat(limits, formats);
            try
            {
                Log("Compares to null is always false, returned : ");
                Logln(cf.Equals(null) ? "TRUE" : "FALSE");
            }
            catch (Exception foo)
            {
                Errln("ChoiceFormat.Equals(null) throws exception.");
            }
        }
        /* @bug 4106659
         * ChoiceFormat.ctor(double[], String[]) doesn't check
         * whether lengths of input arrays are equal.
         */
        [Test]
        public void Test4106659()
        {
            double[] limits = { 1, 2, 3 };
            String[] formats = { "one", "two" };
            ChoiceFormat cf = null;
            try
            {
                cf = new ChoiceFormat(limits, formats);
            }
            catch (Exception foo)
            {
                Logln("ChoiceFormat constructor should check for the array lengths");
                cf = null;
            }
            if (cf != null) Errln(cf.Format(5));
        }

        /* @bug 4106660
         * ChoiceFormat.ctor(double[], String[]) allows unordered double array.
         * This is not a bug, added javadoc to emphasize the use of limit
         * array must be in ascending order.
         */
        [Test]
        [Ignore("ICU4N TODO: Finish MessageFormat Implementation")]
        public void Test4106660()
        {
            double[] limits = { 3, 1, 2 };
            String[] formats = { "Three", "One", "Two" };
            ChoiceFormat cf = new ChoiceFormat(limits, formats);
            double d = 5.0;
            String str = cf.Format(d);
            if (!str.Equals("Two"))
                Errln("format(" + d + ") = " + cf.Format(d));
        }

        // ICU4N TODO: Serialization
        ///* @bug 4111739
        // * MessageFormat is incorrectly serialized/deserialized.
        // */
        //[Test]
        //public void Test4111739()
        //{
        //    MessageFormat format1 = null;
        //    MessageFormat format2 = null;
        //    ObjectOutputStream ostream = null;
        //    ByteArrayOutputStream baos = null;
        //    ObjectInputStream istream = null;

        //    try
        //    {
        //        baos = new ByteArrayOutputStream();
        //        ostream = new ObjectOutputStream(baos);
        //    }
        //    catch (IOException e)
        //    {
        //        Errln("Unexpected exception : " + e.getMessage());
        //        return;
        //    }

        //    try
        //    {
        //        format1 = new MessageFormat("pattern{0}");
        //        ostream.writeObject(format1);
        //        ostream.flush();

        //        byte bytes[] = baos.toByteArray();

        //        istream = new ObjectInputStream(new ByteArrayInputStream(bytes));
        //        format2 = (MessageFormat)istream.readObject();
        //    }
        //    catch (Exception e)
        //    {
        //        Errln("Unexpected exception : " + e.getMessage());
        //    }

        //    if (!format1.Equals(format2))
        //    {
        //        Errln("MessageFormats before and after serialization are not" +
        //            " equal\nformat1 = " + format1 + "(" + format1.ToPattern() + ")\nformat2 = " +
        //            format2 + "(" + format2.ToPattern() + ")");
        //    }
        //    else
        //    {
        //        Logln("Serialization for MessageFormat is OK.");
        //    }
        //}
        /* @bug 4114743
         * MessageFormat.applyPattern allows illegal patterns.
         */
        [Test]
        public void Test4114743()
        {
            String originalPattern = "initial pattern";
            MessageFormat mf = new MessageFormat(originalPattern);
            String illegalPattern = "ab { '}' de";
            try
            {
                mf.ApplyPattern(illegalPattern);
                Errln("illegal pattern: \"" + illegalPattern + "\"");
            }
            catch (ArgumentException foo)
            {
                if (illegalPattern.Equals(mf.ToPattern()))
                    Errln("pattern after: \"" + mf.ToPattern() + "\"");
            }
        }

        /* @bug 4116444
         * MessageFormat.parse has different behavior in case of null.
         */
        [Test]
        [Ignore("ICU4N TODO: Finish MessageFormat Implementation")]
        public void Test4116444()
        {
            String[] patterns = { "", "one", "{0,date,short}" };
            MessageFormat mf = new MessageFormat("");

            for (int i = 0; i < patterns.Length; i++)
            {
                String pattern = patterns[i];
                mf.ApplyPattern(pattern);
                try
                {
                    Object[] array = mf.Parse(null, new ParsePosition(0));
                    Logln("pattern: \"" + pattern + "\"");
                    Log(" parsedObjects: ");
                    if (array != null)
                    {
                        Log("{");
                        for (int j = 0; j < array.Length; j++)
                        {
                            if (array[j] != null)
                                Err("\"" + array[j].ToString() + "\"");
                            else
                                Log("null");
                            if (j < array.Length - 1) Log(",");
                        }
                        Log("}");
                    }
                    else
                    {
                        Log("null");
                    }
                    Logln("");
                }
                catch (Exception e)
                {
                    Errln("pattern: \"" + pattern + "\"");
                    Errln("  Exception: " + e.Message);
                }
            }

        }
        /* @bug 4114739 (FIX and add javadoc)
         * MessageFormat.format has undocumented behavior about empty format objects.
         */
        [Test]
        public void Test4114739()
        {

            MessageFormat mf = new MessageFormat("<{0}>");
            Object[] objs1 = null;
            Object[] objs2 = { };
            Object[] objs3 = { null };
            try
            {
                Logln("pattern: \"" + mf.ToPattern() + "\"");
                Log("format(null) : ");
                Logln("\"" + mf.Format(objs1) + "\"");
                Log("format({})   : ");
                Logln("\"" + mf.Format(objs2) + "\"");
                Log("format({null}) :");
                Logln("\"" + mf.Format(objs3) + "\"");
            }
            catch (Exception e)
            {
                Errln("Exception thrown for null argument tests.");
            }
        }

        /* @bug 4113018
         * MessageFormat.applyPattern works wrong with illegal patterns.
         */
        [Test]
        public void Test4113018()
        {
            String originalPattern = "initial pattern";
            MessageFormat mf = new MessageFormat(originalPattern);
            String illegalPattern = "format: {0, xxxYYY}";
            Logln("pattern before: \"" + mf.ToPattern() + "\"");
            Logln("illegal pattern: \"" + illegalPattern + "\"");
            try
            {
                mf.ApplyPattern(illegalPattern);
                Errln("Should have thrown IllegalArgumentException for pattern : " + illegalPattern);
            }
            catch (ArgumentException e)
            {
                if (illegalPattern.Equals(mf.ToPattern()))
                    Errln("pattern after: \"" + mf.ToPattern() + "\"");
            }
        }
        /* @bug 4106661
         * ChoiceFormat is silent about the pattern usage in javadoc.
         */
        [Test]
        [Ignore("ICU4N TODO: Finish MessageFormat Implementation")]
        public void Test4106661()
        {
            ChoiceFormat fmt = new ChoiceFormat(
              "-1#are negative| 0#are no or fraction | 1#is one |1.0<is 1+ |2#are two |2<are more than 2.");
            Logln("Formatter Pattern : " + fmt.ToPattern());

            Logln("Format with -INF : " + fmt.Format(double.NegativeInfinity));
            Logln("Format with -1.0 : " + fmt.Format(-1.0));
            Logln("Format with 0 : " + fmt.Format(0));
            Logln("Format with 0.9 : " + fmt.Format(0.9));
            Logln("Format with 1.0 : " + fmt.Format(1));
            Logln("Format with 1.5 : " + fmt.Format(1.5));
            Logln("Format with 2 : " + fmt.Format(2));
            Logln("Format with 2.1 : " + fmt.Format(2.1));
            Logln("Format with NaN : " + fmt.Format(Double.NaN));
            Logln("Format with +INF : " + fmt.Format(double.PositiveInfinity));
        }
        /* @bug 4094906
         * ChoiceFormat should accept \u221E as eq. to INF.
         */
        [Test]
        [Ignore("ICU4N TODO: Finish MessageFormat Implementation")]
        public void Test4094906()
        {
            ChoiceFormat fmt = new ChoiceFormat(
              "-\u221E<are negative|0<are no or fraction|1#is one|1.0<is 1+|\u221E<are many.");
            if (!fmt.ToPattern().StartsWith("-\u221E<are negative|0.0<are no or fraction|1.0#is one|1.0<is 1+|\u221E<are many.", StringComparison.Ordinal))
                Errln("Formatter Pattern : " + fmt.ToPattern());
            Logln("Format with -INF : " + fmt.Format(double.NegativeInfinity));
            Logln("Format with -1.0 : " + fmt.Format(-1.0));
            Logln("Format with 0 : " + fmt.Format(0));
            Logln("Format with 0.9 : " + fmt.Format(0.9));
            Logln("Format with 1.0 : " + fmt.Format(1));
            Logln("Format with 1.5 : " + fmt.Format(1.5));
            Logln("Format with 2 : " + fmt.Format(2));
            Logln("Format with +INF : " + fmt.Format(double.PositiveInfinity));
        }

        /* @bug 4118592
         * MessageFormat.parse fails with ChoiceFormat.
         */
        [Test]
        public void Test4118592()
        {
            MessageFormat mf = new MessageFormat("");
            String pattern = "{0,choice,1#YES|2#NO}";
            String prefix = "";
            for (int i = 0; i < 5; i++)
            {
                String formatted = prefix + "YES";
                mf.ApplyPattern(prefix + pattern);
                prefix += "x";
                Object[] objs = mf.Parse(formatted, new ParsePosition(0));
                Logln(i + ". pattern :\"" + mf.ToPattern() + "\"");
                Log(" \"" + formatted + "\" parsed as ");
                if (objs == null) Logln("  null");
                else Logln("  " + objs[0]);
            }
        }
        /* @bug 4118594
         * MessageFormat.parse fails for some patterns.
         */
        [Test]
        [Ignore("ICU4N TODO: Finish MessageFormat Implementation")]
        public void Test4118594()
        {
            MessageFormat mf = new MessageFormat("{0}, {0}, {0}");
            String forParsing = "x, y, z";
            Object[] objs = mf.Parse(forParsing, new ParsePosition(0));
            Logln("pattern: \"" + mf.ToPattern() + "\"");
            Logln("text for parsing: \"" + forParsing + "\"");
            if (!objs[0].ToString().Equals("z"))
                Errln("argument0: \"" + objs[0] + "\"");
            mf.SetCulture(new CultureInfo("en-us"));
            mf.ApplyPattern("{0,number,#.##}, {0,number,#.#}");
            Object[] oldobjs = { 3.1415d };
            String result = mf.Format(oldobjs);
            Logln("pattern: \"" + mf.ToPattern() + "\"");
            Logln("text for parsing: \"" + result + "\"");
            // result now equals "3.14, 3.1"
            if (!result.Equals("3.14, 3.1"))
                Errln("result = " + result);
            Object[] newobjs = mf.Parse(result, new ParsePosition(0));
            // newobjs now equals {new Double(3.1)}
            if (Convert.ToDouble(newobjs[0]) != 3.1) // was (Double) [alan]
                Errln("newobjs[0] = " + newobjs[0]);
        }
        /* @bug 4105380
         * When using ChoiceFormat, MessageFormat is not good for I18n.
         */
        [Test]
        [Ignore("ICU4N TODO: Finish MessageFormat Implementation")]
        public void Test4105380()
        {
            String patternText1 = "The disk \"{1}\" contains {0}.";
            String patternText2 = "There are {0} on the disk \"{1}\"";
            MessageFormat form1 = new MessageFormat(patternText1);
            MessageFormat form2 = new MessageFormat(patternText2);
            double[] filelimits = { 0, 1, 2 };
            String[] filepart = { "no files", "one file", "{0,number} files" };
            ChoiceFormat fileform = new ChoiceFormat(filelimits, filepart);
            form1.SetFormat(1, fileform);
            form2.SetFormat(0, fileform);
            Object[] testArgs = { 12373L, "MyDisk" };
            Logln(form1.Format(testArgs));
            Logln(form2.Format(testArgs));
        }
        /* @bug 4120552
         * MessageFormat.parse incorrectly sets errorIndex.
         */
        [Test]
        public void Test4120552()
        {
            MessageFormat mf = new MessageFormat("pattern");
            String[] texts = new string[] { "pattern", "pat", "1234" };
            Logln("pattern: \"" + mf.ToPattern() + "\"");
            for (int i = 0; i < texts.Length; i++)
            {
                ParsePosition pp = new ParsePosition(0);
                Object[] objs = mf.Parse(texts[i], pp);
                Log("  text for parsing: \"" + texts[i] + "\"");
                if (objs == null)
                {
                    Logln("  (incorrectly formatted string)");
                    if (pp.ErrorIndex == -1)
                        Errln("Incorrect error index: " + pp.ErrorIndex);
                }
                else
                {
                    Logln("  (correctly formatted string)");
                }
            }
        }

        /**
         * @bug 4142938
         * MessageFormat handles single quotes in pattern wrong.
         * This is actually a problem in ChoiceFormat; it doesn't
         * understand single quotes.
         */
        [Test]
        public void Test4142938()
        {
            String pat = "''Vous'' {0,choice,0#n''|1#}avez s\u00E9lectionne\u00E9 " +
                "{0,choice,0#aucun|1#{0}} client{0,choice,0#s|1#|2#s} " +
                "personnel{0,choice,0#s|1#|2#s}.";
            MessageFormat mf = new MessageFormat(pat);

            String[] PREFIX = {
            "'Vous' n'avez s\u00E9lectionne\u00E9 aucun clients personnels.",
            "'Vous' avez s\u00E9lectionne\u00E9 ",
            "'Vous' avez s\u00E9lectionne\u00E9 "
        };
            String[] SUFFIX = {
            null,
            " client personnel.",
            " clients personnels."
        };

            for (int i = 0; i < 3; i++)
            {
                String @out = mf.Format(new Object[] { Integer.GetInstance(i) });
                if (SUFFIX[i] == null)
                {
                    if (!@out.Equals(PREFIX[i]))
                        Errln("" + i + ": Got \"" + @out + "\"; Want \"" + PREFIX[i] + "\"");
                }
                else
                {
                    if (!@out.StartsWith(PREFIX[i], StringComparison.Ordinal) ||
                        !@out.EndsWith(SUFFIX[i], StringComparison.Ordinal))
                        Errln("" + i + ": Got \"" + @out + "\"; Want \"" + PREFIX[i] + "\"...\"" +
                              SUFFIX[i] + "\"");
                }
            }
        }

        /**
         * @bug 4142938
         * Test the applyPattern and toPattern handling of single quotes
         * by ChoiceFormat.  (This is in here because this was a bug reported
         * against MessageFormat.)  The single quote is used to quote the
         * pattern characters '|', '#', '&lt;', and '\u2264'.  Two quotes in a row
         * is a quote literal.
         */
        [Test]
        [Ignore("ICU4N TODO: Finish MessageFormat Implementation")]
        public void TestChoicePatternQuote()
        {
            String[] DATA = {
            // Pattern                  0 value           1 value
            "0#can''t|1#can",           "can't",          "can",
            "0#'pound(#)=''#'''|1#xyz", "pound(#)='#'",   "xyz",
            "0#'1<2 | 1\u22641'|1#''",  "1<2 | 1\u22641", "'",
        };
            for (int i = 0; i < DATA.Length; i += 3)
            {
                try
                {
                    ChoiceFormat cf = new ChoiceFormat(DATA[i]);
                    for (int j = 0; j <= 1; ++j)
                    {
                        String @out = cf.Format(j);
                        if (!@out.Equals(DATA[i + 1 + j]))
                            Errln("Fail: Pattern \"" + DATA[i] + "\" x " + j + " -> " +
                                  @out + "; want \"" + DATA[i + 1 + j] + '"');
                    }
                    String pat = cf.ToPattern();
                    String pat2 = new ChoiceFormat(pat).ToPattern();
                    if (!pat.Equals(pat2))
                        Errln("Fail: Pattern \"" + DATA[i] + "\" x toPattern -> \"" + pat + '"');
                    else
                        Logln("Ok: Pattern \"" + DATA[i] + "\" x toPattern -> \"" + pat + '"');
                }
                catch (ArgumentException e)
                {
                    Errln("Fail: Pattern \"" + DATA[i] + "\" -> " + e);
                }
            }
        }

        /**
         * @bug 4112104
         * MessageFormat.Equals(null) throws a NullPointerException.  The JLS states
         * that it should return false.
         */
        [Test]
        public void Test4112104()
        {
            MessageFormat format = new MessageFormat("");
            try
            {
                // This should NOT throw an exception
                if (format.Equals(null))
                {
                    // It also should return false
                    Errln("MessageFormat.Equals(null) returns false");
                }
            }
            catch (ArgumentNullException e)
            {
                Errln("MessageFormat.Equals(null) throws " + e);
            }
        }

        /**
         * @bug 4169959
         * MessageFormat does not format null objects. CANNOT REPRODUCE THIS BUG.
         */
        [Test]
        public void Test4169959()
        {
            // This works
            Logln(MessageFormat.Format("This will {0}", new Object[] { "work" }));

            // This fails
            Logln(MessageFormat.Format("This will {0}", new Object[] { null }));
        }

        [Test]
        public void Test4232154()
        {
            bool gotException = false;
            try
            {
                new MessageFormat("The date is {0:date}");
            }
            catch (Exception e)
            {
                gotException = true;
                if (!(e is ArgumentException))
                {
                    throw new Exception("got wrong exception type");
                }
                if ("argument number too large at ".Equals(e.Message))
                {
                    throw new Exception("got wrong exception message");
                }
            }
            if (!gotException)
            {
                throw new Exception("didn't get exception for invalid input");
            }
        }

        [Test]
        public void Test4293229()
        {
            MessageFormat format = new MessageFormat("'''{'0}'' '''{0}'''");
            Object[] args = { null };
            String expected = "'{0}' '{0}'";
            String result = format.Format(args);
            if (!result.Equals(expected))
            {
                throw new Exception("wrong format result - expected \"" +
                        expected + "\", got \"" + result + "\"");
            }
        }

        // This test basically ensures that the tests defined above also work with
        // valid named arguments.
        [Test]
        [Ignore("ICU4N TODO: Finish MessageFormat Implementation")]
        public void TestBugTestsWithNamesArguments()
        {

            { // Taken from Test4031438().
                String pattern1 = "Impossible {arg1} has occurred -- status code is {arg0} and message is {arg2}.";
                String pattern2 = "Double '' Quotes {ARG_ZERO} test and quoted '{ARG_ONE}' test plus 'other {ARG_TWO} stuff'.";

                MessageFormat messageFormatter = new MessageFormat("");

                try
                {
                    Logln("Apply with pattern : " + pattern1);
                    messageFormatter.ApplyPattern(pattern1);
                    IDictionary<string, object> paramsMap = new Dictionary<string, object>();
                    paramsMap["arg0"] = 7;
                    String tempBuffer = messageFormatter.Format(paramsMap);
                    if (!tempBuffer.Equals("Impossible {arg1} has occurred -- status code is 7 and message is {arg2}."))
                        Errln("Tests arguments < substitution failed");
                    Logln("Formatted with 7 : " + tempBuffer);
                    ParsePosition status = new ParsePosition(0);
                    var objs = messageFormatter.ParseToMap(tempBuffer, status);
                    if (objs.Get("arg1") != null || objs.Get("arg2") != null)
                        Errln("Parse failed with more than expected arguments");
                    //for (Iterator keyIter = objs.keySet().iterator();
                    //     keyIter.hasNext();)
                    //{
                    //    String key = (String)keyIter.next();
                    // ICU4N: Using KVP instead of explicit lookups is much faster
                    foreach (var pair in objs)
                    {
                        if (pair.Value != null && !pair.Value.ToString().Equals(paramsMap.Get(pair.Key).ToString()))
                        {
                            Errln("Parse failed on object " + pair.Value + " with argument name : " + pair.Key);
                        }
                    }
                    tempBuffer = messageFormatter.Format((object)null);
                    if (!tempBuffer.Equals("Impossible {arg1} has occurred -- status code is {arg0} and message is {arg2}."))
                        Errln("Tests with no arguments failed");
                    Logln("Formatted with null : " + tempBuffer);
                    Logln("Apply with pattern : " + pattern2);
                    messageFormatter.ApplyPattern(pattern2);
                    paramsMap.Clear();
                    paramsMap["ARG_ZERO"] = 7;
                    tempBuffer = messageFormatter.Format(paramsMap);
                    if (!tempBuffer.Equals("Double ' Quotes 7 test and quoted {ARG_ONE} test plus 'other {ARG_TWO} stuff'."))
                        Errln("quote format test (w/ params) failed.");
                    Logln("Formatted with params : " + tempBuffer);
                    tempBuffer = messageFormatter.Format((object)null);
                    if (!tempBuffer.Equals("Double ' Quotes {ARG_ZERO} test and quoted {ARG_ONE} test plus 'other {ARG_TWO} stuff'."))
                        Errln("quote format test (w/ null) failed.");
                    Logln("Formatted with null : " + tempBuffer);
                    Logln("toPattern : " + messageFormatter.ToPattern());
                }
                catch (Exception foo)
                {
                    Warnln("Exception when formatting in bug 4031438. " + foo.Message);
                }
            }
            { // Taken from Test4052223().
                ParsePosition pos = new ParsePosition(0);
                if (pos.ErrorIndex != -1)
                {
                    Errln("ParsePosition.getErrorIndex initialization failed.");
                }
                MessageFormat fmt = new MessageFormat("There are {numberOfApples} apples growing on the {whatKindOfTree} tree.");
                String str = "There is one apple growing on the peach tree.";
                var objs = fmt.ParseToMap(str, pos);
                Logln("unparsable string , should fail at " + pos.ErrorIndex);
                if (pos.ErrorIndex == -1)
                    Errln("Bug 4052223 failed : parsing string " + str);
                pos.ErrorIndex = (4);
                if (pos.ErrorIndex != 4)
                    Errln("setErrorIndex failed, got " + pos.ErrorIndex + " instead of 4");
                if (objs != null)
                    Errln("unparsable string, should return null");
            }
            // ICU4N TODO: Serialization
            //{ // Taken from Test4111739().
            //    MessageFormat format1 = null;
            //    MessageFormat format2 = null;
            //    ObjectOutputStream ostream = null;
            //    ByteArrayOutputStream baos = null;
            //    ObjectInputStream istream = null;

            //    try
            //    {
            //        baos = new ByteArrayOutputStream();
            //        ostream = new ObjectOutputStream(baos);
            //    }
            //    catch (IOException e)
            //    {
            //        Errln("Unexpected exception : " + e.getMessage());
            //        return;
            //    }

            //    try
            //    {
            //        format1 = new MessageFormat("pattern{argument}");
            //        ostream.writeObject(format1);
            //        ostream.flush();

            //        byte bytes[] = baos.toByteArray();

            //        istream = new ObjectInputStream(new ByteArrayInputStream(bytes));
            //        format2 = (MessageFormat)istream.readObject();
            //    }
            //    catch (Exception e)
            //    {
            //        Errln("Unexpected exception : " + e.Message);
            //    }

            //    if (!format1.Equals(format2))
            //    {
            //        Errln("MessageFormats before and after serialization are not" +
            //            " equal\nformat1 = " + format1 + "(" + format1.ToPattern() + ")\nformat2 = " +
            //            format2 + "(" + format2.ToPattern() + ")");
            //    }
            //    else
            //    {
            //        Logln("Serialization for MessageFormat is OK.");
            //    }
            //}
            { // Taken from Test4116444().
                String[] patterns = { "", "one", "{namedArgument,date,short}" };
                MessageFormat mf = new MessageFormat("");

                for (int i = 0; i < patterns.Length; i++)
                {
                    String pattern = patterns[i];
                    mf.ApplyPattern(pattern);
                    try
                    {
                        var objs = mf.ParseToMap(null, new ParsePosition(0));
                        Logln("pattern: \"" + pattern + "\"");
                        Log(" parsedObjects: ");
                        if (objs != null)
                        {
                            Log("{");
                            //for (Iterator keyIter = objs.keySet().iterator();
                            //     keyIter.hasNext();)
                            //{
                            //    String key = (String)keyIter.next();
                            bool first = true;
                            // ICU4N: Using KVP instead of explicit lookups is much faster
                            foreach (var pair in objs)
                            {
                                if (first)
                                    first = false;
                                else
                                    Log(",");
                                if (pair.Value != null)
                                {
                                    Err("\"" + pair.Value.ToString() + "\"");
                                }
                                else
                                {
                                    Log("null");
                                }
                                //if (keyIter.hasNext())
                                //{
                                //    Log(",");
                                //}
                            }
                            Log("}");
                        }
                        else
                        {
                            Log("null");
                        }
                        Logln("");
                    }
                    catch (Exception e)
                    {
                        Errln("pattern: \"" + pattern + "\"");
                        Errln("  Exception: " + e.Message);
                    }
                }
            }
            { // Taken from Test4114739().
                MessageFormat mf = new MessageFormat("<{arg}>");
                IDictionary<string, object> objs1 = null;
                IDictionary<string, object> objs2 = new Dictionary<string, object>();
                IDictionary<string, object> objs3 = new Dictionary<string, object>();
                objs3["arg"] = null;
                try
                {
                    Logln("pattern: \"" + mf.ToPattern() + "\"");
                    Log("format(null) : ");
                    Logln("\"" + mf.Format(objs1) + "\"");
                    Log("format({})   : ");
                    Logln("\"" + mf.Format(objs2) + "\"");
                    Log("format({null}) :");
                    Logln("\"" + mf.Format(objs3) + "\"");
                }
                catch (Exception e)
                {
                    Errln("Exception thrown for null argument tests.");
                }
            }
            { // Taken from Test4118594().
                String argName = "something_stupid";
                MessageFormat mf = new MessageFormat("{" + argName + "}, {" + argName + "}, {" + argName + "}");
                String forParsing = "x, y, z";
                var objs = mf.ParseToMap(forParsing, new ParsePosition(0));
                Logln("pattern: \"" + mf.ToPattern() + "\"");
                Logln("text for parsing: \"" + forParsing + "\"");
                if (!objs.Get(argName).ToString().Equals("z"))
                    Errln("argument0: \"" + objs.Get(argName) + "\"");
                mf.SetCulture(new CultureInfo("en-us"));
                mf.ApplyPattern("{" + argName + ",number,#.##}, {" + argName + ",number,#.#}");
                var oldobjs = new Dictionary<string, object>();
                oldobjs[argName] = 3.1415d;
                String result = mf.Format(oldobjs);
                Logln("pattern: \"" + mf.ToPattern() + "\"");
                Logln("text for parsing: \"" + result + "\"");
                // result now equals "3.14, 3.1"
                if (!result.Equals("3.14, 3.1"))
                    Errln("result = " + result);
                var newobjs = mf.ParseToMap(result, new ParsePosition(0));
                // newobjs now equals {new Double(3.1)}
                if (Convert.ToDouble(newobjs.Get(argName)) != 3.1) // was (Double) [alan]
                    Errln("newobjs.get(argName) = " + newobjs.Get(argName));
            }
            { // Taken from Test4105380().
                String patternText1 = "The disk \"{diskName}\" contains {numberOfFiles}.";
                String patternText2 = "There are {numberOfFiles} on the disk \"{diskName}\"";
                MessageFormat form1 = new MessageFormat(patternText1);
                MessageFormat form2 = new MessageFormat(patternText2);
                double[] filelimits = { 0, 1, 2 };
                String[] filepart = { "no files", "one file", "{numberOfFiles,number} files" };
                ChoiceFormat fileform = new ChoiceFormat(filelimits, filepart);
                form1.SetFormat(1, fileform);
                form2.SetFormat(0, fileform);
                var testArgs = new Dictionary<string, object>();
                testArgs["diskName"] = "MyDisk";
                testArgs["numberOfFiles"] = 12373L;
                Logln(form1.Format(testArgs));
                Logln(form2.Format(testArgs));
            }
            { // Taken from test4293229().
                MessageFormat format = new MessageFormat("'''{'myNamedArgument}'' '''{myNamedArgument}'''");
                var args = new Dictionary<string, object>();
                String expected = "'{myNamedArgument}' '{myNamedArgument}'";
                String result = format.Format(args);
                if (!result.Equals(expected))
                {
                    throw new Exception("wrong format result - expected \"" +
                            expected + "\", got \"" + result + "\"");
                }
            }
        }
        //ICU4N TODO: Serialization
        //private MessageFormat serializeAndDeserialize(MessageFormat original)
        //{
        //    try
        //    {
        //        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        //        ObjectOutputStream ostream = new ObjectOutputStream(baos);
        //        ostream.writeObject(original);
        //        ostream.flush();
        //        byte bytes[] = baos.toByteArray();

        //        ObjectInputStream istream = new ObjectInputStream(new ByteArrayInputStream(bytes));
        //        MessageFormat reconstituted = (MessageFormat)istream.readObject();
        //        return reconstituted;
        //    }
        //    catch (IOException e)
        //    {
        //        throw new RuntimeException(e);
        //    }
        //    catch (ClassNotFoundException e)
        //    {
        //        throw new RuntimeException(e);
        //    }
        //}

        //[Test]
        //    public void TestSerialization()
        //{
        //    MessageFormat format1 = null;
        //    MessageFormat format2 = null;

        //    format1 = new MessageFormat("", new UCultureInfo("de"));
        //    format2 = serializeAndDeserialize(format1);
        //    assertEquals("MessageFormats (empty pattern) before and after serialization are not equal", format1, format2);

        //    format1.ApplyPattern("ab{1}cd{0,number}ef{3,date}gh");
        //    format1.setFormat(2, null);
        //    format1.setFormatByArgumentIndex(1, NumberFormat.GetInstance(new UCultureInfo("en")));
        //    format2 = serializeAndDeserialize(format1);
        //    assertEquals("MessageFormats (with custom formats) before and after serialization are not equal", format1, format2);
        //    assertEquals(
        //            "MessageFormat (with custom formats) does not " +
        //            "format correctly after serialization",
        //            "ab3.3cd4,4ef***gh",
        //            format2.Format(new Object[] { 4.4, 3.3, "+++", "***" }));
        //}
    }
}
