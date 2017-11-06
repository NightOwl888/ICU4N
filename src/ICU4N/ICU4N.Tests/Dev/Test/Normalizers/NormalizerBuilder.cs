using ICU4N.Support.Collections;
using ICU4N.TestFramework.Dev.Test;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Tests.Dev.Test.Normalizers
{
    /**
     * Builds the normalization tables. This is a separate class so that it
     * can be unloaded once not needed.<br>
     * Copyright (C) 1998-2007 International Business Machines Corporation and
     * Unicode, Inc. All Rights Reserved.<br>
     * The Unicode Consortium makes no expressed or implied warranty of any
     * kind, and assumes no liability for errors or omissions.
     * No liability is assumed for incidental and consequential damages
     * in connection with or arising out of the use of the information here.
     * @author Mark Davis
     * Updates for supplementary code points:
     * Vladimir Weinstein & Markus Scherer
     */

    internal class NormalizerBuilder
    {
        //private static final String copyright = "Copyright (C) 1998-2003 International Business Machines Corporation and Unicode, Inc.";

        /**
         * Testing flags
         */

        private static readonly bool DEBUG = false;
        //private static readonly boolean GENERATING = false;

        /**
         * Constants for the data file version to use.
         */
        /*static final boolean NEW_VERSION = true;
        private static readonly String DIR = "D:\\UnicodeData\\" + (NEW_VERSION ? "WorkingGroups\\" : "");

        static readonly String UNIDATA_VERSION = NEW_VERSION ? "3.0.0d12" : "2.1.9";
        static readonly String EXCLUSIONS_VERSION = NEW_VERSION ? "1d4" : "1";

        public static readonly String UNICODE_DATA = DIR + "UnicodeData-" + UNIDATA_VERSION + ".txt";
        public static readonly String COMPOSITION_EXCLUSIONS = DIR + "CompositionExclusions-" + EXCLUSIONS_VERSION +".txt";
        */

        /**
         * Called exactly once by NormalizerData to build the static data
         */

        internal static NormalizerData Build(bool fullData)
        {
            try
            {
                IntHashtable canonicalClass = new IntHashtable(0);
                IntStringHashtable decompose = new IntStringHashtable(null);
                LongHashtable compose = new LongHashtable(NormalizerData.NOT_COMPOSITE);
                BitSet isCompatibility = new BitSet();
                BitSet isExcluded = new BitSet();
                if (fullData)
                {
                    //Console.Out.WriteLine("Building Normalizer Data from file.");
                    ReadExclusionList(isExcluded);
                    //Console.Out.WriteLine(isExcluded.get(0x00C0));
                    BuildDecompositionTables(canonicalClass, decompose, compose,
                      isCompatibility, isExcluded);
                }
                else
                {    // for use in Applets
                     //Console.Out.WriteLine("Building abridged data.");
                    SetMinimalDecomp(canonicalClass, decompose, compose,
                      isCompatibility, isExcluded);
                }
                return new NormalizerData(canonicalClass, decompose, compose,
                      isCompatibility, isExcluded);
            }
            catch (IOException e)
            {
                Console.Error.WriteLine("Can't load data file." + e + ", " + e.ToString());
                return null;
            }
        }

        // =============================================================
        // Building Decomposition Tables
        // =============================================================

        /**
         * Reads exclusion list and stores the data
         */
        private static void ReadExclusionList(BitSet isExcluded)
        {
            if (DEBUG) Console.Out.WriteLine("Reading Exclusions");

            TextReader @in = TestUtil.GetDataReader("Unicode.CompositionExclusions.txt");

            while (true)
            {
                // read a line, discarding comments and blank lines

                String line = @in.ReadLine();
                if (line == null) break;
                int comment = line.IndexOf('#');                    // strip comments
                if (comment != -1) line = line.Substring(0, comment - 0);
                if (line.Length == 0) continue;                   // ignore blanks
                if (line.IndexOf(' ') != -1)
                {
                    line = line.Substring(0, line.IndexOf(' ') - 0);
                }
                // store -1 in the excluded table for each character hit

                int value = int.Parse(line, NumberStyles.HexNumber);
                isExcluded.Set(value);
                //Console.Out.WriteLine("Excluding " + hex(value));
            }
            @in.Dispose();
            if (DEBUG) Console.Out.WriteLine("Done reading Exclusions");
        }

        /**
         * Builds a decomposition table from a UnicodeData file
         */
        private static void BuildDecompositionTables(
          IntHashtable canonicalClass, IntStringHashtable decompose,
          LongHashtable compose, BitSet isCompatibility, BitSet isExcluded)
        {
            if (DEBUG) Console.Out.WriteLine("Reading Unicode Character Database");
            //BufferedReader in = new BufferedReader(new FileReader(UNICODE_DATA), 64*1024);
            TextReader @in = null;
            try
            {
                @in = TestUtil.GetDataReader("Unicode.UnicodeData.txt");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Failed to read UnicodeData.txt");
                Environment.Exit(1);
            }

            int value;
            long pair;
            int counter = 0;
            while (true)
            {

                // read a line, discarding comments and blank lines

                String line = @in.ReadLine();
                if (line == null) break;
                int comment = line.IndexOf('#');                    // strip comments
                if (comment != -1) line = line.Substring(0, comment - 0);
                if (line.Length == 0) continue;
                if (DEBUG)
                {
                    counter++;
                    if ((counter & 0xFF) == 0) Console.Out.WriteLine("At: " + line);
                }

                // find the values of the particular fields that we need
                // Sample line: 00C0;LATIN ...A GRAVE;Lu;0;L;0041 0300;;;;N;LATIN ... GRAVE;;;00E0;

                int start = 0;
                int end = line.IndexOf(';'); // code
                value = int.Parse(line.Substring(start, end - start), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                if (true && value == '\u00c0')
                {
                    //Console.Out.WriteLine("debug: " + line);
                }
                end = line.IndexOf(';', start = end + 1); // name
                                                          /*String name = line.substring(start,end);*/
                end = line.IndexOf(';', start = end + 1); // general category
                end = line.IndexOf(';', start = end + 1); // canonical class

                // check consistency: canonical classes must be from 0 to 255

                int cc = int.Parse(line.Substring(start, end - start), CultureInfo.InvariantCulture);
                if (cc != (cc & 0xFF)) Console.Error.WriteLine("Bad canonical class at: " + line);
                canonicalClass.Put(value, cc);
                end = line.IndexOf(';', start = end + 1); // BIDI
                end = line.IndexOf(';', start = end + 1); // decomp

                // decomp requires more processing.
                // store whether it is canonical or compatibility.
                // store the decomp in one table, and the reverse mapping (from pairs) in another

                if (start != end)
                {
                    String segment = line.Substring(start, end - start);
                    bool compat = segment[0] == '<';
                    if (compat) isCompatibility.Set(value);
                    String decomp = fromHex(segment);

                    // a small snippet of code to generate the Applet data

                    /*if (GENERATING) {
                        if (value < 0xFF) {
                            Console.Out.WriteLine(
                                "\"\\u" + hex((char)value) + "\", "
                                + "\"\\u" + hex(decomp, "\\u") + "\", "
                                + (compat ? "\"K\"," : "\"\",")
                                + "// " + name);
                        }
                    }*/

                    // check consistency: all canon decomps must be singles or pairs!
                    int decompLen = UTF16Util.CountCodePoint(decomp);
                    if (decompLen < 1 || decompLen > 2 && !compat)
                    {
                        Console.Error.WriteLine("Bad decomp at: " + line);
                    }
                    decompose.Put(value, decomp);

                    // only compositions are canonical pairs
                    // skip if script exclusion

                    if (!compat && !isExcluded.Get(value))
                    {
                        int first = '\u0000';
                        int second = UTF16Util.NextCodePoint(decomp, 0);
                        if (decompLen > 1)
                        {
                            first = second;
                            second = UTF16Util.NextCodePoint(decomp,
                                UTF16Util.CodePointLength(first));
                        }

                        // store composition pair in single integer

                        pair = ((long)first << 32) | second;
                        if (DEBUG && value == '\u00C0')
                        {
                            Console.Out.WriteLine("debug2: " + line);
                        }
                        compose.Put(pair, value);
                    }
                    else if (DEBUG)
                    {
                        Console.Out.WriteLine("Excluding: " + decomp);
                    }
                }
            }
            @in.Dispose();
            if (DEBUG) Console.Out.WriteLine("Done reading Unicode Character Database");

            // add algorithmic Hangul decompositions
            // this is more compact if done at runtime, but for simplicity we
            // do it this way.

            if (DEBUG) Console.Out.WriteLine("Adding Hangul");

            for (int SIndex = 0; SIndex < SCount; ++SIndex)
            {
                int TIndex = SIndex % TCount;
                char first, second;
                if (TIndex != 0)
                { // triple
                    first = (char)(SBase + SIndex - TIndex);
                    second = (char)(TBase + TIndex);
                }
                else
                {
                    first = (char)(LBase + SIndex / NCount);
                    second = (char)(VBase + (SIndex % NCount) / TCount);
                }
                pair = ((long)first << 32) | second;
                value = SIndex + SBase;
                decompose.Put(value, Convert.ToString(first, CultureInfo.InvariantCulture) + second);
                compose.Put(pair, value);
            }
            if (DEBUG) Console.Out.WriteLine("Done adding Hangul");
        }

        /**
         * Hangul composition constants
         */
        static readonly int
            SBase = 0xAC00, LBase = 0x1100, VBase = 0x1161, TBase = 0x11A7,
            LCount = 19, VCount = 21, TCount = 28,
            NCount = VCount * TCount,   // 588
            SCount = LCount * NCount;   // 11172

        /**
         * For use in an applet: just load a minimal set of data.
         */
        private static void SetMinimalDecomp(IntHashtable canonicalClass, IntStringHashtable decompose,
          LongHashtable compose, BitSet isCompatibility, BitSet isExcluded)
        {
            String[] decomposeData = {
            "\u005E", "\u0020\u0302", "K",
            "\u005F", "\u0020\u0332", "K",
            "\u0060", "\u0020\u0300", "K",
            "\u00A0", "\u0020", "K",
            "\u00A8", "\u0020\u0308", "K",
            "\u00AA", "\u0061", "K",
            "\u00AF", "\u0020\u0304", "K",
            "\u00B2", "\u0032", "K",
            "\u00B3", "\u0033", "K",
            "\u00B4", "\u0020\u0301", "K",
            "\u00B5", "\u03BC", "K",
            "\u00B8", "\u0020\u0327", "K",
            "\u00B9", "\u0031", "K",
            "\u00BA", "\u006F", "K",
            "\u00BC", "\u0031\u2044\u0034", "K",
            "\u00BD", "\u0031\u2044\u0032", "K",
            "\u00BE", "\u0033\u2044\u0034", "K",
            "\u00C0", "\u0041\u0300", "",
            "\u00C1", "\u0041\u0301", "",
            "\u00C2", "\u0041\u0302", "",
            "\u00C3", "\u0041\u0303", "",
            "\u00C4", "\u0041\u0308", "",
            "\u00C5", "\u0041\u030A", "",
            "\u00C7", "\u0043\u0327", "",
            "\u00C8", "\u0045\u0300", "",
            "\u00C9", "\u0045\u0301", "",
            "\u00CA", "\u0045\u0302", "",
            "\u00CB", "\u0045\u0308", "",
            "\u00CC", "\u0049\u0300", "",
            "\u00CD", "\u0049\u0301", "",
            "\u00CE", "\u0049\u0302", "",
            "\u00CF", "\u0049\u0308", "",
            "\u00D1", "\u004E\u0303", "",
            "\u00D2", "\u004F\u0300", "",
            "\u00D3", "\u004F\u0301", "",
            "\u00D4", "\u004F\u0302", "",
            "\u00D5", "\u004F\u0303", "",
            "\u00D6", "\u004F\u0308", "",
            "\u00D9", "\u0055\u0300", "",
            "\u00DA", "\u0055\u0301", "",
            "\u00DB", "\u0055\u0302", "",
            "\u00DC", "\u0055\u0308", "",
            "\u00DD", "\u0059\u0301", "",
            "\u00E0", "\u0061\u0300", "",
            "\u00E1", "\u0061\u0301", "",
            "\u00E2", "\u0061\u0302", "",
            "\u00E3", "\u0061\u0303", "",
            "\u00E4", "\u0061\u0308", "",
            "\u00E5", "\u0061\u030A", "",
            "\u00E7", "\u0063\u0327", "",
            "\u00E8", "\u0065\u0300", "",
            "\u00E9", "\u0065\u0301", "",
            "\u00EA", "\u0065\u0302", "",
            "\u00EB", "\u0065\u0308", "",
            "\u00EC", "\u0069\u0300", "",
            "\u00ED", "\u0069\u0301", "",
            "\u00EE", "\u0069\u0302", "",
            "\u00EF", "\u0069\u0308", "",
            "\u00F1", "\u006E\u0303", "",
            "\u00F2", "\u006F\u0300", "",
            "\u00F3", "\u006F\u0301", "",
            "\u00F4", "\u006F\u0302", "",
            "\u00F5", "\u006F\u0303", "",
            "\u00F6", "\u006F\u0308", "",
            "\u00F9", "\u0075\u0300", "",
            "\u00FA", "\u0075\u0301", "",
            "\u00FB", "\u0075\u0302", "",
            "\u00FC", "\u0075\u0308", "",
            "\u00FD", "\u0079\u0301", "",
// EXTRAS, outside of Latin 1
            "\u1EA4", "\u00C2\u0301", "",
            "\u1EA5", "\u00E2\u0301", "",
            "\u1EA6", "\u00C2\u0300", "",
            "\u1EA7", "\u00E2\u0300", "",
        };

            int[] classData = {
            0x0300, 230,
            0x0301, 230,
            0x0302, 230,
            0x0303, 230,
            0x0304, 230,
            0x0305, 230,
            0x0306, 230,
            0x0307, 230,
            0x0308, 230,
            0x0309, 230,
            0x030A, 230,
            0x030B, 230,
            0x030C, 230,
            0x030D, 230,
            0x030E, 230,
            0x030F, 230,
            0x0310, 230,
            0x0311, 230,
            0x0312, 230,
            0x0313, 230,
            0x0314, 230,
            0x0315, 232,
            0x0316, 220,
            0x0317, 220,
            0x0318, 220,
            0x0319, 220,
            0x031A, 232,
            0x031B, 216,
            0x031C, 220,
            0x031D, 220,
            0x031E, 220,
            0x031F, 220,
            0x0320, 220,
            0x0321, 202,
            0x0322, 202,
            0x0323, 220,
            0x0324, 220,
            0x0325, 220,
            0x0326, 220,
            0x0327, 202,
            0x0328, 202,
            0x0329, 220,
            0x032A, 220,
            0x032B, 220,
            0x032C, 220,
            0x032D, 220,
            0x032E, 220,
            0x032F, 220,
            0x0330, 220,
            0x0331, 220,
            0x0332, 220,
            0x0333, 220,
            0x0334, 1,
            0x0335, 1,
            0x0336, 1,
            0x0337, 1,
            0x0338, 1,
            0x0339, 220,
            0x033A, 220,
            0x033B, 220,
            0x033C, 220,
            0x033D, 230,
            0x033E, 230,
            0x033F, 230,
            0x0340, 230,
            0x0341, 230,
            0x0342, 230,
            0x0343, 230,
            0x0344, 230,
            0x0345, 240,
            0x0360, 234,
            0x0361, 234
        };

            // build the same tables we would otherwise get from the
            // Unicode Character Database, just with limited data

            for (int i = 0; i < decomposeData.Length; i += 3)
            {
                char value = decomposeData[i][0];
                String decomp = decomposeData[i + 1];
                bool compat = decomposeData[i + 2].Equals("K");
                if (compat) isCompatibility.Set(value);
                decompose.Put(value, decomp);
                if (!compat)
                {
                    int first = '\u0000';
                    int second = UTF16Util.NextCodePoint(decomp, 0);
                    if (decomp.Length > 1)
                    {
                        first = second;
                        second = UTF16Util.NextCodePoint(decomp,
                            UTF16Util.CodePointLength(first));
                    }
                    long pair = (first << 16) | second;
                    compose.Put(pair, value);
                }
            }

            for (int i = 0; i < classData.Length;)
            {
                canonicalClass.Put(classData[i++], classData[i++]);
            }
        }

        /**
         * Utility: Parses a sequence of hex Unicode characters separated by spaces
         */
        static public String fromHex(String source)
        {
            StringBuffer result = new StringBuffer();
            for (int i = 0; i < source.Length; ++i)
            {
                char c = source[i];
                switch (c)
                {
                    case ' ': break; // ignore
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                        int end = 0;
                        int value = 0;
                        try
                        {
                            //Console.Out.WriteLine(source.substring(i, i + 4) + "************" + source);
                            end = source.IndexOf(' ', i);
                            if (end < 0)
                            {
                                end = source.Length;
                            }
                            value = int.Parse(source.Substring(i, end - i), NumberStyles.HexNumber);
                            UTF16Util.AppendCodePoint(result, value);
                        }
                        catch (Exception e)
                        {
                            Console.Out.WriteLine("i: " + i + ";end:" + end + "source:" + source);
                            //Console.Out.WriteLine(source.substring(i, i + 4) + "************" + source);
                            Environment.Exit(1);
                        }
                        //i+= 3; // skip rest of number
                        i = end;
                        break;
                    case '<':
                        int j = source.IndexOf('>', i); // skip <...>
                        if (j > 0)
                        {
                            i = j;
                            break;
                        } // else fall through--error
                        throw new ArgumentException("Bad hex value in " + source);
                    default:
                        throw new ArgumentException("Bad hex value in " + source);
                }
            }
            return result.ToString();
        }

        /**
         * Utility: Supplies a zero-padded hex representation of an integer (without 0x)
         */
        static public String hex(int i)
        {
            String result = Convert.ToString(i & 0xFFFFFFFFL, 16).ToUpperInvariant(); // ICU4N TODO: Check this conversion (16)
            return "00000000".Substring(result.Length, 8 - result.Length) + result;
        }

        /**
         * Utility: Supplies a zero-padded hex representation of a Unicode character (without 0x, \\u)
         */
        static public String hex(char i)
        {
            String result = Convert.ToString(i, 16).ToUpperInvariant(); // ICU4N TODO: Check this conversion (16)
            return "0000".Substring(result.Length, 4 - result.Length) + result;
        }

        /**
         * Utility: Supplies a zero-padded hex representation of a Unicode character (without 0x, \\u)
         */
        public static String hex(String s, String sep)
        {
            StringBuffer result = new StringBuffer();
            for (int i = 0; i < s.Length; ++i)
            {
                if (i != 0) result.Append(sep);
                result.Append(hex(s[i]));
            }
            return result.ToString();
        }
    }
}
