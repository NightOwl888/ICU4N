using ICU4N.Impl;
using ICU4N.Support.Collections;
using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.StringPrep
{
    /// <summary>limaoyu</summary>
    public class IDNAConformanceTest : TestFmwk
    {
        [Test]
        public void TestConformance()
        {

            SortedDictionary<long, IDictionary<string, string>> inputData = null;

            try
            {
                inputData = ReadInput.GetInputData();
            }
            catch (EncoderFallbackException e)
            {
                Errln(e.ToString());
                return;
            }
            catch (IOException e)
            {
                Errln(e.ToString());
                return;
            }

            var keyMap = inputData.Keys;
            foreach (var element in keyMap)
            {
                var tempHash = inputData.Get(element);

                //get all attributes from input data
                String passfail = (String)tempHash.Get("passfail");
                String desc = (String)tempHash.Get("desc");
                String type = (String)tempHash.Get("type");
                String namebase = (String)tempHash.Get("namebase");
                String nameutf8 = (String)tempHash.Get("nameutf8");
                String namezone = (String)tempHash.Get("namezone");
                String failzone1 = (String)tempHash.Get("failzone1");
                String failzone2 = (String)tempHash.Get("failzone2");

                //they maybe includes <*> style unicode
                namebase = StringReplace(namebase);
                namezone = StringReplace(namezone);

                String result = null;
                bool failed = false;

                if ("toascii".Equals(tempHash.Get("type")))
                {

                    //get the result
                    try
                    {
                        //by default STD3 rules are not used, but if the description
                        //includes UseSTD3ASCIIRules, we will set it.
                        if (desc.ToLowerInvariant().IndexOf(
                                "UseSTD3ASCIIRules".ToLowerInvariant(), StringComparison.Ordinal) == -1)
                        {
                            result = IDNA.ConvertIDNToASCII(namebase,
                                    IDNA2003Options.AllowUnassigned).ToString();
                        }
                        else
                        {
                            result = IDNA.ConvertIDNToASCII(namebase,
                                    IDNA2003Options.UseSTD3Rules).ToString();
                        }
                    }
                    catch (StringPrepParseException e2)
                    {
                        //Errln(e2.getMessage());
                        failed = true;
                    }


                    if ("pass".Equals(passfail))
                    {
                        if (!namezone.Equals(result))
                        {
                            PrintInfo(desc, namebase, nameutf8, namezone,
                                    failzone1, failzone2, result, type, passfail);
                            Errln("\t pass fail standard is pass, but failed");
                        }
                        else
                        {
                            PrintInfo(desc, namebase, nameutf8, namezone,
                                    failzone1, failzone2, result, type, passfail);
                            Logln("\tpassed");
                        }
                    }

                    if ("fail".Equals(passfail))
                    {
                        if (failed)
                        {
                            PrintInfo(desc, namebase, nameutf8, namezone,
                                    failzone1, failzone2, result, type, passfail);
                            Logln("passed");
                        }
                        else
                        {
                            PrintInfo(desc, namebase, nameutf8, namezone,
                                    failzone1, failzone2, result, type, passfail);
                            Errln("\t pass fail standard is fail, but no exception thrown out");
                        }
                    }
                }
                else if ("tounicode".Equals(tempHash.Get("type")))
                {
                    try
                    {
                        //by default STD3 rules are not used, but if the description
                        //includes UseSTD3ASCIIRules, we will set it.
                        if (desc.ToLowerInvariant().IndexOf(
                                "UseSTD3ASCIIRules".ToLowerInvariant(), StringComparison.Ordinal) == -1)
                        {
                            result = IDNA.ConvertIDNToUnicode(namebase,
                                    IDNA2003Options.AllowUnassigned).ToString();
                        }
                        else
                        {
                            result = IDNA.ConvertIDNToUnicode(namebase,
                                    IDNA2003Options.UseSTD3Rules).ToString();
                        }
                    }
                    catch (StringPrepParseException e2)
                    {
                        //Errln(e2.getMessage());
                        failed = true;
                    }
                    if ("pass".Equals(passfail))
                    {
                        if (!namezone.Equals(result))
                        {
                            PrintInfo(desc, namebase, nameutf8, namezone,
                                    failzone1, failzone2, result, type, passfail);

                            Errln("\t Did not get the expected result. Expected: " + Prettify(namezone) + " Got: " + Prettify(result));
                        }
                        else
                        {
                            PrintInfo(desc, namebase, nameutf8, namezone,
                                    failzone1, failzone2, result, type, passfail);
                            Logln("\tpassed");
                        }
                    }

                    if ("fail".Equals(passfail))
                    {
                        if (failed || namebase.Equals(result))
                        {
                            PrintInfo(desc, namebase, nameutf8, namezone,
                                    failzone1, failzone2, result, type, passfail);

                            Logln("\tpassed");
                        }
                        else
                        {
                            PrintInfo(desc, namebase, nameutf8, namezone,
                                    failzone1, failzone2, result, type, passfail);

                            Errln("\t pass fail standard is fail, but no exception thrown out");
                        }
                    }
                }
                else
                {
                    continue;
                }
            }
        }

        /**
         * Print log message.
         * @param desc
         * @param namebase
         * @param nameutf8
         * @param namezone
         * @param failzone1
         * @param failzone2
         * @param result
         */
        private void PrintInfo(String desc, String namebase,
                String nameutf8, String namezone, String failzone1,
                String failzone2, String result, String type, String passfail)
        {
            Logln("desc:\t" + desc);
            Log("\t");
            Logln("type:\t" + type);
            Log("\t");
            Logln("pass fail standard:\t" + passfail);
            Log("\t");
            Logln("namebase:\t" + namebase);
            Log("\t");
            Logln("nameutf8:\t" + nameutf8);
            Log("\t");
            Logln("namezone:\t" + namezone);
            Log("\t");
            Logln("failzone1:\t" + failzone1);
            Log("\t");
            Logln("failzone2:\t" + failzone2);
            Log("\t");
            Logln("result:\t" + result);
        }

        /**
         * Change unicode string from <00AD> to \u00AD, for the later is accepted
         * by Java
         * @param str String including <*> style unicode
         * @return \\u String
         */
        private static String StringReplace(String str)
        {

            StringBuffer result = new StringBuffer();
            char[] chars = str.ToCharArray();
            StringBuffer sbTemp = new StringBuffer();
            for (int i = 0; i < chars.Length; i++)
            {
                if ('<' == chars[i])
                {
                    sbTemp = new StringBuffer();
                    while ('>' != chars[i + 1])
                    {
                        sbTemp.Append(chars[++i]);
                    }
                    /*
                     * The unicode sometimes is larger then \uFFFF, so have to use
                     * UTF16.
                     */
                    int toBeInserted = int.Parse(sbTemp.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    if ((toBeInserted >> 16) == 0)
                    {
                        result.Append((char)toBeInserted);
                    }
                    else
                    {
                        String utf16String = UTF16.ValueOf(toBeInserted);
                        char[] charsTemp = utf16String.ToCharArray();
                        for (int j = 0; j < charsTemp.Length; j++)
                        {
                            result.Append(charsTemp[j]);
                        }
                    }
                }
                else if ('>' == chars[i])
                {//end when met with '>'
                    continue;
                }
                else
                {
                    result.Append(chars[i]);
                }

            }
            return result.ToString();
        }

        /**
         * This class is used to read test data from TestInput file.
         *
         * @author limaoyu
         *
         */
        public static class ReadInput
        {

            public static SortedDictionary<long, IDictionary<string, string>> GetInputData()
            {

                SortedDictionary<long, IDictionary<string, string>> result = new SortedDictionary<long, IDictionary<string, string>>();
                TextReader @in = TestUtil.GetDataReader("IDNATestInput.txt", "utf-8");
                try
                {
                    String tempStr = null;
                    int records = 0;
                    bool firstLine = true;
                    var hashItem = new Dictionary<string, string>();

                    while ((tempStr = @in.ReadLine()) != null)
                    {
                        //ignore the first line if it's "====="
                        if (firstLine)
                        {
                            if ("=====".Equals(tempStr))
                                continue;
                            firstLine = false;
                        }

                        //Ignore empty line
                        if ("".Equals(tempStr))
                        {
                            continue;
                        }

                        String attr = "";//attribute
                        String body = "";//value

                        //get attr and body from line input, and then set them into each hash item.
                        int postion = tempStr.IndexOf(':');
                        if (postion > -1)
                        {
                            attr = tempStr.Substring(0, postion).Trim(); // ICU4N: Checked 2nd parameter
                            body = tempStr.Substring(postion + 1).Trim();

                            //deal with combination lines when end with '\'
                            while (null != body && body.Length > 0
                                    && '\\' == body[body.Length - 1])
                            {
                                body = body.Substring(0, body.Length - 1); // ICU4N: Checked 2nd parameter
                                body += "\n";
                                tempStr = @in.ReadLine();
                                body += tempStr;
                            }
                        }
                        //push them to hash item
                        hashItem[attr] = body;

                        //if met "=====", it means this item is finished
                        if ("=====".Equals(tempStr))
                        {
                            //set them into result, using records number as key
                            result[(long)records] = hashItem;
                            //create another hash item and continue
                            hashItem = new Dictionary<string, string>();
                            records++;
                            continue;
                        }
                    }
                }
                finally
                {
                    @in.Dispose();
                }
                return result;
            }
        }
    }
}
