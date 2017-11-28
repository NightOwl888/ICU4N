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
    /// Round trip test of Transliterator
    /// </summary>
    public class ReplaceableTest : TestFmwk
    {
        [Test]
        public void Test()
        {
            Check("Lower", "ABCD", "1234");
            Check("Upper", "abcd\u00DF", "123455"); // must map 00DF to SS
            Check("Title", "aBCD", "1234");
            Check("NFC", "A\u0300E\u0300", "13");
            Check("NFD", "\u00C0\u00C8", "1122");
            Check("*(x) > A $1 B", "wxy", "11223");
            Check("*(x)(y) > A $2 B $1 C $2 D", "wxyz", "113322334");
            Check("*(x)(y)(z) > A $3 B $2 C $1 D", "wxyzu", "114433225");
            // TODO Revisit the following in 2.6 or later.
            Check("*x > a", "xyz", "223"); // expect "123"?
            Check("*x > a", "wxy", "113"); // expect "123"?
            Check("*x > a", "\uFFFFxy", "_33"); // expect "_23"?
            Check("*(x) > A $1 B", "\uFFFFxy", "__223");
        }

        internal void Check(String transliteratorName, String test, String shouldProduceStyles)
        {
            TestReplaceable tr = new TestReplaceable(test, null);
            String original = tr.ToString();

            Transliterator t;
            if (transliteratorName.StartsWith("*", StringComparison.Ordinal))
            {
                transliteratorName = transliteratorName.Substring(1);
                t = Transliterator.CreateFromRules("test", transliteratorName,
                                                   Transliterator.FORWARD);
            }
            else
            {
                t = Transliterator.GetInstance(transliteratorName);
            }
            t.Transliterate(tr);
            String newStyles = tr.GetStyles();
            if (!newStyles.Equals(shouldProduceStyles))
            {
                Errln("FAIL Styles: " + transliteratorName + " ( "
                    + original + " ) => " + tr.ToString() + "; should be {" + shouldProduceStyles + "}!");
            }
            else
            {
                Logln("OK: " + transliteratorName + " ( " + original + " ) => " + tr.ToString());
            }

            if (!tr.HasMetaData || tr.Chars.HasMetaData
                || tr.Styles.HasMetaData)
            {
                Errln("Fail hasMetaData()");
            }
        }


        /**
         * This is a test class that simulates styled text.
         * It associates a style number (0..65535) with each character,
         * and maintains that style in the normal fashion:
         * When setting text from raw string or characters,<br>
         * Set the styles to the style of the first character replaced.<br>
         * If no characters are replaced, use the style of the previous character.<br>
         * If at start, use the following character<br>
         * Otherwise use NO_STYLE.
         */
        internal class TestReplaceable : IReplaceable
        {
            internal ReplaceableString Chars { get; set; }
            internal ReplaceableString Styles { get; set; }

            static readonly char NO_STYLE = '_';

            static readonly char NO_STYLE_MARK = (char)0xFFFF;

            internal TestReplaceable(String text, String styles)
            {
                Chars = new ReplaceableString(text);
                StringBuffer s = new StringBuffer();
                for (int i = 0; i < text.Length; ++i)
                {
                    if (styles != null && i < styles.Length)
                    {
                        s.Append(styles[i]);
                    }
                    else
                    {
                        if (text[i] == NO_STYLE_MARK)
                        {
                            s.Append(NO_STYLE);
                        }
                        else
                        {
                            s.Append((char)(i + '1'));
                        }
                    }
                }
                this.Styles = new ReplaceableString(s.ToString());
            }

            public String GetStyles()
            {
                return Styles.ToString();
            }

            public override String ToString()
            {
                return Chars.ToString() + "{" + Styles.ToString() + "}";
            }

            public String Substring(int start, int length)
            {
                return Chars.Substring(start, length); // ICU4N: Warning this has .NET semantics, just like string.Substring(int, int)
            }

            public int Length
            {
                get { return Chars.Length; }
            }

            public char this[int offset]
            {
                get { return Chars[offset]; }
            }

            public int Char32At(int offset)
            {
                return Chars.Char32At(offset);
            }

            //@Override
            //public void getChars(int srcStart, int srcLimit, char dst[], int dstStart)
            //{
            //    Chars.getChars(srcStart, srcLimit, dst, dstStart);
            //}

            public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
            {
                Chars.CopyTo(sourceIndex, destination, destinationIndex, count);
            }

            public void Replace(int start, int limit, string text)
            {
                if (Substring(start, limit - start).Equals(text)) return; // NO ACTION! // ICU4N: Corrected 2nd substring parameter
                if (DEBUG) Console.Out.Write(Utility.Escape(ToString() + " -> replace(" + start +
                                                "," + limit + "," + text) + ") -> ");
                Chars.Replace(start, limit, text);
                fixStyles(start, limit, text.Length);
                if (DEBUG) Console.Out.Write(Utility.Escape(ToString()));
            }

            public void Replace(int start, int limit, char[] charArray,
                                int charsStart, int charsLen)
            {
                if (Substring(start, limit - start).Equals(new String(charArray, charsStart, charsLen - charsStart))) return; // NO ACTION! // ICU4N: Corrected 2nd substring parameter
                this.Chars.Replace(start, limit, charArray, charsStart, charsLen);
                fixStyles(start, limit, charsLen);
            }

            void fixStyles(int start, int limit, int newLen)
            {
                char newStyle = NO_STYLE;
                if (start != limit && Styles[start] != NO_STYLE)
                {
                    newStyle = Styles[start];
                }
                else if (start > 0 && this[start - 1] != NO_STYLE_MARK)
                {
                    newStyle = Styles[start - 1];
                }
                else if (limit < Styles.Length)
                {
                    newStyle = Styles[limit];
                }
                // dumb implementation for now.
                StringBuffer s = new StringBuffer();
                for (int i = 0; i < newLen; ++i)
                {
                    // this doesn't really handle an embedded NO_STYLE_MARK
                    // in the middle of a long run of characters right -- but
                    // that case shouldn't happen anyway
                    if (this[start + i] == NO_STYLE_MARK)
                    {
                        s.Append(NO_STYLE);
                    }
                    else
                    {
                        s.Append(newStyle);
                    }
                }
                Styles.Replace(start, limit, s.ToString());
            }

            public void Copy(int start, int limit, int dest)
            {
                Chars.Copy(start, limit, dest);
                Styles.Copy(start, limit, dest);
            }

            public bool HasMetaData
            {
                get { return true; }
            }

            internal static readonly bool DEBUG = false;
        }

        [Test]
        public void Test5789()
        {
            String rules =
                "IETR > IET | \\' R; # (1) do split ietr between t and r\r\n" +
                "I[EH] > I; # (2) friedrich";
            Transliterator trans = Transliterator.CreateFromRules("foo", rules, Transliterator.FORWARD);
            String result = trans.Transliterate("BLENKDIETRICH");
            assertEquals("Rule breakage", "BLENKDIET'RICH", result);
        }
    }
}
