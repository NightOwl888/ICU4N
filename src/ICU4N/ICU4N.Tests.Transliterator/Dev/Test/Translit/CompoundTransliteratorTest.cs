using ICU4N.Impl;
using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Translit
{
    /// <summary>
    /// General test of CompoundTransliterator
    /// </summary>
    public class CompoundTransliteratorTest : TestFmwk
    {
        [Test]
        public void TestConstruction()
        {
            Logln("Testing the construction of the compound Transliterator");
            String[] names = { "Greek-Latin", "Latin-Devanagari", "Devanagari-Latin", "Latin-Greek" };

            try
            {
                Transliterator.GetInstance(names[0]);
                Transliterator.GetInstance(names[1]);
                Transliterator.GetInstance(names[2]);
                Transliterator.GetInstance(names[3]);
            }
            catch (ArgumentException ex)
            {
                Errln("FAIL: Transliterator construction failed" + ex.ToString());
                throw ex;
            }

            string[] IDs ={
            names[0],
            names[0]+";"+names[3],
            names[3]+";"+names[1]+";"+names[2],
            names[0]+";"+names[1]+";"+names[2]+";"+names[3]
        };


            for (int i = 0; i < 4; i++)
            {
                try
                {
                    Transliterator.GetInstance(IDs[i]);
                }
                catch (ArgumentException ex1)
                {
                    Errln("FAIL: construction using CompoundTransliterator(String ID) failed for " + IDs[i]);
                    throw ex1;
                }

                try
                {
                    Transliterator.GetInstance(IDs[i], Transliterator.Forward);
                }
                catch (ArgumentException ex2)
                {
                    Errln("FAIL: construction using CompoundTransliterator(String ID, int direction=FORWARD) failed for " + IDs[i]);
                    throw ex2;
                }

                try
                {
                    Transliterator.GetInstance(IDs[i], Transliterator.Reverse);
                }
                catch (ArgumentException ex3)
                {
                    Errln("FAIL: construction using CompoundTransliterator(String ID, int direction=REVERSE) failed for " + IDs[i]);
                    throw ex3;
                }

                //            try{
                //                CompoundTransliterator cpdtrans=new CompoundTransliterator(IDs[i], Transliterator.FORWARD, null);
                //                cpdtrans = null;
                //            }catch(IllegalArgumentException ex4) {
                //                Errln("FAIL: construction using CompoundTransliterator(String ID, int direction=FORWARD," +
                //                        "UnicodeFilter adoptedFilter=0) failed for " + IDs[i]);
                //                throw ex4;
                //            }
                //
                //
                //            try{
                //                CompoundTransliterator cpdtrans2=new CompoundTransliterator(transarray[i], null);
                //                cpdtrans2 = null;
                //            }catch(IllegalArgumentException ex5) {
                //                Errln("FAIL: Construction using CompoundTransliterator(Transliterator transliterators[]," +
                //                       "UnicodeFilter adoptedFilter = 0)  failed");
                //                throw ex5;
                //            }


            }

        }

        [Test]
        public void TestGetTransliterator()
        {
            Logln("Testing the getTransliterator() API of CompoundTransliterator");
            String ID = "Latin-Greek;Greek-Latin;Latin-Devanagari;Devanagari-Latin;Latin-Cyrillic;Cyrillic-Latin;Any-Hex;Hex-Any";
            Transliterator ct1 = null;
            try
            {
                //ct1=new CompoundTransliterator(ID);
                ct1 = Transliterator.GetInstance(ID);
            }
            catch (ArgumentException iae)
            {
                Errln("CompoundTransliterator construction failed for ID=" + ID);
                throw iae;
            }
            //int count=ct1.getCount();
            Transliterator[] elems = ct1.GetElements();
            int count = elems.Length;
            String[] array = split(ID, ';');
            if (count != array.Length)
            {
                Errln("Error: getCount() failed. Expected:" + array.Length + " got:" + count);
            }
            for (int i = 0; i < count; i++)
            {
                //String child= ct1.getTransliterator(i).getID();
                String child = elems[i].ID;
                if (!child.Equals(array[i]))
                {
                    Errln("Error getTransliterator() failed: Expected->" + array[i] + " Got->" + child);
                }
                else
                {
                    Logln("OK: getTransliterator() passed: Expected->" + array[i] + " Got->" + child);
                }
            }


        }


        [Test]
        public void TestTransliterate()
        {
            Logln("Testing the handleTransliterate() API of CompoundTransliterator");
            Transliterator ct1 = null;
            try
            {
                ct1 = Transliterator.GetInstance("Any-Hex;Hex-Any");
            }
            catch (ArgumentException iae)
            {
                Errln("FAIL: construction using CompoundTransliterator(String ID) failed for " + "Any-Hex;Hex-Any");
                throw iae;
            }

            String s = "abcabc";
            expect(ct1, s, s);
            Transliterator.Position index = new Transliterator.Position();
            ReplaceableString rsource2 = new ReplaceableString(s);
            String expectedResult = s;
            ct1.Transliterate(rsource2, index);
            ct1.FinishTransliteration(rsource2, index);
            String result = rsource2.ToString();
            expectAux(ct1.ID + ":ReplaceableString, index(0,0,0,0)", s + "->" + rsource2, result.Equals(expectedResult), expectedResult);

            Transliterator.Position index2 = new Transliterator.Position(1, 3, 2, 3);
            ReplaceableString rsource3 = new ReplaceableString(s);
            ct1.Transliterate(rsource3, index2);
            ct1.FinishTransliteration(rsource3, index2);
            result = rsource3.ToString();
            expectAux(ct1.ID + ":String, index2(1,2,2,3)", s + "->" + rsource3, result.Equals(expectedResult), expectedResult);


            String[] Data ={
                     //ID, input string, transliterated string
                     "Any-Hex;Hex-Any;Any-Hex",     "hello",  "\\u0068\\u0065\\u006C\\u006C\\u006F",
                     "Any-Hex;Hex-Any",                 "hello! How are you?",  "hello! How are you?",
                     "Devanagari-Latin;Latin-Devanagari",       "\u092D\u0948'\u0930'\u0935",  "\u092D\u0948\u0930\u0935", // quotes lost
                     "Latin-Cyrillic;Cyrillic-Latin",           "a'b'k'd'e'f'g'h'i'j'Shch'shch'zh'h", "a'b'k'd'e'f'g'h'i'j'Shch'shch'zh'h",
                     "Latin-Greek;Greek-Latin",                 "ABGabgAKLMN", "ABGabgAKLMN",
                     //"Latin-Arabic;Arabic-Latin",               "Ad'r'a'b'i'k'dh'dd'gh", "Adrabikdhddgh",
                     "Hiragana-Katakana",                       "\u3041\u308f\u3099\u306e\u304b\u3092\u3099",
                                                                         "\u30A1\u30f7\u30ce\u30ab\u30fa",
                     "Hiragana-Katakana;Katakana-Hiragana",     "\u3041\u308f\u3099\u306e\u304b\u3051",
                                                                         "\u3041\u308f\u3099\u306e\u304b\u3051",
                     "Katakana-Hiragana;Hiragana-Katakana",     "\u30A1\u30f7\u30ce\u30f5\u30f6",
                                                                         "\u30A1\u30f7\u30ce\u30ab\u30b1",
                     "Latin-Katakana;Katakana-Latin",                   "vavivuvevohuzizuzoninunasesuzezu",
                                                                         "vavivuvevohuzizuzoninunasesuzezu",
                };
            Transliterator ct2 = null;
            for (int i = 0; i < Data.Length; i += 3)
            {
                try
                {
                    ct2 = Transliterator.GetInstance(Data[i + 0]);
                }
                catch (ArgumentException iae2)
                {
                    Errln("FAIL: CompoundTransliterator construction failed for " + Data[i + 0]);
                    throw iae2;
                }
                expect(ct2, Data[i + 1], Data[i + 2]);
            }

        }


        //======================================================================
        // Support methods
        //======================================================================

        /**
        * Splits a string,
       */
        private static String[] split(String s, char divider)
        {

            // see how many there are
            int count = 1;
            for (int i = 0; i < s.Length; ++i)
            {
                if (s[i] == divider) ++count;
            }

            {
                // make an array with them
                String[] result = new String[count];
                int last = 0;
                int current = 0;
                int i;
                for (i = 0; i < s.Length; ++i)
                {
                    if (s[i] == divider)
                    {
                        result[current++] = s.Substring(last, i - last); // ICU4N: Corrected 2nd parameter
                        last = i + 1;
                    }
                }
                result[current++] = s.Substring(last, i - last); // ICU4N: Corrected 2nd parameter
                return result;
            }
        }

        private void expect(Transliterator t, String source, String expectedResult)
        {
            String result = t.Transliterate(source);
            expectAux(t.ID + ":String", source, result, expectedResult);

            ReplaceableString rsource = new ReplaceableString(source);
            t.Transliterate(rsource);
            result = rsource.ToString();
            expectAux(t.ID + ":Replaceable", source, result, expectedResult);

            // Test keyboard (incremental) transliteration -- this result
            // must be the same after we finalize (see below).
            rsource.Replace(0, rsource.Length, "");
            Transliterator.Position index = new Transliterator.Position();
            StringBuffer log = new StringBuffer();

            for (int i = 0; i < source.Length; ++i)
            {
                if (i != 0)
                {
                    log.Append(" + ");
                }
                log.Append(source[i]).Append(" -> ");
                t.Transliterate(rsource, index,
                                source[i] + "");
                // Append the string buffer with a vertical bar '|' where
                // the committed index is.
                String s = rsource.ToString();
                log.Append(s.Substring(0, index.Start)). // ICU4N: Checked 2nd parameter
                    Append('|').
                    Append(s.Substring(index.Start));
            }

            // As a final step in keyboard transliteration, we must call
            // transliterate to finish off any pending partial matches that
            // were waiting for more input.
            t.FinishTransliteration(rsource, index);
            result = rsource.ToString();
            log.Append(" => ").Append(rsource.ToString());
            expectAux(t.ID + ":Keyboard", log.ToString(),
                     result.Equals(expectedResult),
                     expectedResult);

        }
        private void expectAux(String tag, String source,
                      String result, String expectedResult)
        {
            expectAux(tag, source + " -> " + result,
                     result.Equals(expectedResult),
                     expectedResult);
        }

        private void expectAux(String tag, String summary, bool pass, String expectedResult)
        {
            if (pass)
            {
                Logln("(" + tag + ") " + Utility.Escape(summary));
            }
            else
            {
                Errln("FAIL: (" + tag + ") "
                    + Utility.Escape(summary)
                    + ", expected " + Utility.Escape(expectedResult));
            }
        }
    }
}
