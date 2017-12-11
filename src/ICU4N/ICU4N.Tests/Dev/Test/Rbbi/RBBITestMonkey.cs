using ICU4N.Lang;
using ICU4N.Support;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Rbbi
{
    /// <summary>
    /// Monkey tests for RBBI.  These tests have independent implementations of
    /// the Unicode TR boundary rules, and compare results between these and ICU's
    /// implementation, using random data.
    /// <para/>
    /// Tests cover Grapheme Cluster (char), Word and Line breaks
    /// <para/>
    /// Ported from ICU4C, original code in file source/test/intltest/rbbitst.cpp
    /// </summary>
    public class RBBITestMonkey : TestFmwk
    {
        //
        //     class RBBIMonkeyKind
        //
        //        Monkey Test for Break Iteration
        //        Abstract interface class.   Concrete derived classes independently
        //        implement the break rules for different iterator types.
        //
        //        The Monkey Test itself uses doesn't know which type of break iterator it is
        //        testing, but works purely in terms of the interface defined here.
        //
        internal abstract class RBBIMonkeyKind
        {

            // Return a List of UnicodeSets, representing the character classes used
            //   for this type of iterator.
            internal abstract IList<object> CharClasses { get; }

            // Set the test text on which subsequent calls to next() will operate
            internal abstract void SetText(StringBuffer text);

            // Find the next break position, starting from the specified position.
            // Return -1 after reaching end of string.
            internal abstract int Next(int i);

            // A Character Property, one of the constants defined in class UProperty.
            //   The value of this property will be displayed for the characters
            //    near any test failure.
            internal UProperty fCharProperty;
        }

        //
        // Data for Extended Pictographic scraped from CLDR common/properties/ExtendedPictographic.txt, 13267
        //
        static String gExtended_Pict = "[" +
                "\\U0001F774-\\U0001F77F\\U00002700-\\U00002701\\U00002703-\\U00002704\\U0000270E\\U00002710-\\U00002711\\U00002765-\\U00002767" +
                "\\U0001F030-\\U0001F093\\U0001F094-\\U0001F09F\\U0001F10D-\\U0001F10F\\U0001F12F\\U0001F16C-\\U0001F16F\\U0001F1AD-\\U0001F1E5" +
                "\\U0001F260-\\U0001F265\\U0001F203-\\U0001F20F\\U0001F23C-\\U0001F23F\\U0001F249-\\U0001F24F\\U0001F252-\\U0001F25F" +
                "\\U0001F266-\\U0001F2FF\\U0001F7D5-\\U0001F7FF\\U0001F000-\\U0001F003\\U0001F005-\\U0001F02B\\U0001F02C-\\U0001F02F" +
                "\\U0001F322-\\U0001F323\\U0001F394-\\U0001F395\\U0001F398\\U0001F39C-\\U0001F39D\\U0001F3F1-\\U0001F3F2\\U0001F3F6" +
                "\\U0001F4FE\\U0001F53E-\\U0001F548\\U0001F54F\\U0001F568-\\U0001F56E\\U0001F571-\\U0001F572\\U0001F57B-\\U0001F586" +
                "\\U0001F588-\\U0001F589\\U0001F58E-\\U0001F58F\\U0001F591-\\U0001F594\\U0001F597-\\U0001F5A3\\U0001F5A6-\\U0001F5A7" +
                "\\U0001F5A9-\\U0001F5B0\\U0001F5B3-\\U0001F5BB\\U0001F5BD-\\U0001F5C1\\U0001F5C5-\\U0001F5D0\\U0001F5D4-\\U0001F5DB" +
                "\\U0001F5DF-\\U0001F5E0\\U0001F5E2\\U0001F5E4-\\U0001F5E7\\U0001F5E9-\\U0001F5EE\\U0001F5F0-\\U0001F5F2\\U0001F5F4-\\U0001F5F9" +
                "\\U00002605\\U00002607-\\U0000260D\\U0000260F-\\U00002610\\U00002612\\U00002616-\\U00002617\\U00002619-\\U0000261C" +
                "\\U0000261E-\\U0000261F\\U00002621\\U00002624-\\U00002625\\U00002627-\\U00002629\\U0000262B-\\U0000262D\\U00002630-\\U00002637" +
                "\\U0000263B-\\U00002647\\U00002654-\\U0000265F\\U00002661-\\U00002662\\U00002664\\U00002667\\U00002669-\\U0000267A" +
                "\\U0000267C-\\U0000267E\\U00002680-\\U00002691\\U00002695\\U00002698\\U0000269A\\U0000269D-\\U0000269F\\U000026A2-\\U000026A9" +
                "\\U000026AC-\\U000026AF\\U000026B2-\\U000026BC\\U000026BF-\\U000026C3\\U000026C6-\\U000026C7\\U000026C9-\\U000026CD" +
                "\\U000026D0\\U000026D2\\U000026D5-\\U000026E8\\U000026EB-\\U000026EF\\U000026F6\\U000026FB-\\U000026FC\\U000026FE-\\U000026FF" +
                "\\U00002388\\U0001FA00-\\U0001FFFD\\U0001F0A0-\\U0001F0AE\\U0001F0B1-\\U0001F0BF\\U0001F0C1-\\U0001F0CF\\U0001F0D1-\\U0001F0F5" +
                "\\U0001F0AF-\\U0001F0B0\\U0001F0C0\\U0001F0D0\\U0001F0F6-\\U0001F0FF\\U0001F80C-\\U0001F80F\\U0001F848-\\U0001F84F" +
                "\\U0001F85A-\\U0001F85F\\U0001F888-\\U0001F88F\\U0001F8AE-\\U0001F8FF\\U0001F900-\\U0001F90B\\U0001F91F\\U0001F928-\\U0001F92F" +
                "\\U0001F931-\\U0001F932\\U0001F94C\\U0001F95F-\\U0001F96B\\U0001F992-\\U0001F997\\U0001F9D0-\\U0001F9E6\\U0001F90C-\\U0001F90F" +
                "\\U0001F93F\\U0001F94D-\\U0001F94F\\U0001F96C-\\U0001F97F\\U0001F998-\\U0001F9BF\\U0001F9C1-\\U0001F9CF\\U0001F9E7-\\U0001F9FF" +
                "\\U0001F6C6-\\U0001F6CA\\U0001F6D3-\\U0001F6D4\\U0001F6E6-\\U0001F6E8\\U0001F6EA\\U0001F6F1-\\U0001F6F2\\U0001F6F7-\\U0001F6F8" +
                "\\U0001F6D5-\\U0001F6DF\\U0001F6ED-\\U0001F6EF\\U0001F6F9-\\U0001F6FF" +
                "]";


        /**
         * Monkey test subclass for testing Character (Grapheme Cluster) boundaries.
         * Note: As of Unicode 6.1, fPrependSet is empty, so don't add it to fSets
         */
        internal class RBBICharMonkey : RBBIMonkeyKind
        {
            List<object> fSets;

            internal UnicodeSet fCRLFSet;
            internal UnicodeSet fControlSet;
            internal UnicodeSet fExtendSet;
            internal UnicodeSet fRegionalIndicatorSet;
            internal UnicodeSet fPrependSet;
            internal UnicodeSet fSpacingSet;
            internal UnicodeSet fLSet;
            internal UnicodeSet fVSet;
            internal UnicodeSet fTSet;
            internal UnicodeSet fLVSet;
            internal UnicodeSet fLVTSet;
            internal UnicodeSet fHangulSet;
            internal UnicodeSet fEmojiModifierSet;
            internal UnicodeSet fEmojiBaseSet;
            internal UnicodeSet fZWJSet;
            internal UnicodeSet fExtendedPictSet;
            internal UnicodeSet fEBGSet;
            internal UnicodeSet fEmojiNRKSet;
            internal UnicodeSet fAnySet;


            internal StringBuffer fText;


            internal RBBICharMonkey()
            {
                fText = null;
                fCharProperty = UProperty.GRAPHEME_CLUSTER_BREAK;
                fCRLFSet = new UnicodeSet("[\\r\\n]");
                fControlSet = new UnicodeSet("[\\p{Grapheme_Cluster_Break = Control}]");
                fExtendSet = new UnicodeSet("[\\p{Grapheme_Cluster_Break = Extend}]");
                fZWJSet = new UnicodeSet("[\\p{Grapheme_Cluster_Break = ZWJ}]");
                fRegionalIndicatorSet = new UnicodeSet("[\\p{Grapheme_Cluster_Break = Regional_Indicator}]");
                fPrependSet = new UnicodeSet("[\\p{Grapheme_Cluster_Break = Prepend}]");
                fSpacingSet = new UnicodeSet("[\\p{Grapheme_Cluster_Break = SpacingMark}]");
                fLSet = new UnicodeSet("[\\p{Grapheme_Cluster_Break = L}]");
                fVSet = new UnicodeSet("[\\p{Grapheme_Cluster_Break = V}]");
                fTSet = new UnicodeSet("[\\p{Grapheme_Cluster_Break = T}]");
                fLVSet = new UnicodeSet("[\\p{Grapheme_Cluster_Break = LV}]");
                fLVTSet = new UnicodeSet("[\\p{Grapheme_Cluster_Break = LVT}]");
                fHangulSet = new UnicodeSet();
                fHangulSet.AddAll(fLSet);
                fHangulSet.AddAll(fVSet);
                fHangulSet.AddAll(fTSet);
                fHangulSet.AddAll(fLVSet);
                fHangulSet.AddAll(fLVTSet);

                fEmojiBaseSet = new UnicodeSet("[\\p{Grapheme_Cluster_Break = EB}]");
                fEmojiModifierSet = new UnicodeSet("[\\p{Grapheme_Cluster_Break = EM}]");
                fExtendedPictSet = new UnicodeSet(gExtended_Pict);
                fEBGSet = new UnicodeSet("[\\p{Grapheme_Cluster_Break = EBG}]");
                fEmojiNRKSet = new UnicodeSet("[[\\p{Emoji}]-[\\p{Grapheme_Cluster_Break = Regional_Indicator}*#0-9©®™〰〽]]");
                fAnySet = new UnicodeSet("[\\u0000-\\U0010ffff]");


                fSets = new List<object>();
                fSets.Add(fCRLFSet);
                fSets.Add(fControlSet);
                fSets.Add(fExtendSet);
                fSets.Add(fRegionalIndicatorSet);
                if (!fPrependSet.IsEmpty)
                {
                    fSets.Add(fPrependSet);
                }
                fSets.Add(fSpacingSet);
                fSets.Add(fHangulSet);
                fSets.Add(fAnySet);
                fSets.Add(fEmojiBaseSet);
                fSets.Add(fEmojiModifierSet);
                fSets.Add(fZWJSet);
                fSets.Add(fExtendedPictSet);
                fSets.Add(fEBGSet);
                fSets.Add(fEmojiNRKSet);
            }


            internal override void SetText(StringBuffer s)
            {
                fText = s;
            }

            internal override IList<object> CharClasses
            {
                get { return fSets; }
            }

            internal override int Next(int prevPos)
            {
                int    /*p0,*/ p1, p2, p3;    // Indices of the significant code points around the
                                              //   break position being tested.  The candidate break
                                              //   location is before p2.

                int breakPos = -1;

                int c0, c1, c2, c3;     // The code points at p0, p1, p2 & p3.
                int cBase;              // for (X Extend*) patterns, the X character.

                // Previous break at end of string.  return DONE.
                if (prevPos >= fText.Length)
                {
                    return -1;
                }
                /* p0 = */
                p1 = p2 = p3 = prevPos;
                c3 = UTF16.CharAt(fText, prevPos);
                c0 = c1 = c2 = cBase = 0;

                // Loop runs once per "significant" character position in the input text.
                for (; ; )
                {
                    // Move all of the positions forward in the input string.
                    /* p0 = p1;*/
                    c0 = c1;
                    p1 = p2; c1 = c2;
                    p2 = p3; c2 = c3;

                    // Advance p3 by one codepoint
                    p3 = MoveIndex32(fText, p3, 1);
                    c3 = (p3 >= fText.Length) ? -1 : UTF16.CharAt(fText, p3);

                    if (p1 == p2)
                    {
                        // Still warming up the loop.  (won't work with zero length strings, but we don't care)
                        continue;
                    }
                    if (p2 == fText.Length)
                    {
                        // Reached end of string.  Always a break position.
                        break;
                    }

                    // Rule  GB3   CR x LF
                    //     No Extend or Format characters may appear between the CR and LF,
                    //     which requires the additional check for p2 immediately following p1.
                    //
                    if (c1 == 0x0D && c2 == 0x0A && p1 == (p2 - 1))
                    {
                        continue;
                    }

                    // Rule (GB4).   ( Control | CR | LF ) <break>
                    if (fControlSet.Contains(c1) ||
                            c1 == 0x0D ||
                            c1 == 0x0A)
                    {
                        break;
                    }

                    // Rule (GB5)    <break>  ( Control | CR | LF )
                    //
                    if (fControlSet.Contains(c2) ||
                            c2 == 0x0D ||
                            c2 == 0x0A)
                    {
                        break;
                    }


                    // Rule (GB6)  L x ( L | V | LV | LVT )
                    if (fLSet.Contains(c1) &&
                            (fLSet.Contains(c2) ||
                                    fVSet.Contains(c2) ||
                                    fLVSet.Contains(c2) ||
                                    fLVTSet.Contains(c2)))
                    {
                        continue;
                    }

                    // Rule (GB7)    ( LV | V )  x  ( V | T )
                    if ((fLVSet.Contains(c1) || fVSet.Contains(c1)) &&
                            (fVSet.Contains(c2) || fTSet.Contains(c2)))
                    {
                        continue;
                    }

                    // Rule (GB8)    ( LVT | T)  x T
                    if ((fLVTSet.Contains(c1) || fTSet.Contains(c1)) &&
                            fTSet.Contains(c2))
                    {
                        continue;
                    }

                    // Rule (GB9)    x (Extend | ZWJ)
                    if (fExtendSet.Contains(c2) || fZWJSet.Contains(c2))
                    {
                        if (!fExtendSet.Contains(c1))
                        {
                            cBase = c1;
                        }
                        continue;
                    }

                    // Rule (GB9a)   x  SpacingMark
                    if (fSpacingSet.Contains(c2))
                    {
                        continue;
                    }

                    // Rule (GB9b)   Prepend x
                    if (fPrependSet.Contains(c1))
                    {
                        continue;
                    }
                    // Rule (GB10)   (Emoji_Base | EBG) Extend* x Emoji_Modifier
                    if ((fEmojiBaseSet.Contains(c1) || fEBGSet.Contains(c1)) && fEmojiModifierSet.Contains(c2))
                    {
                        continue;
                    }
                    if ((fEmojiBaseSet.Contains(cBase) || fEBGSet.Contains(cBase)) &&
                            fExtendSet.Contains(c1) && fEmojiModifierSet.Contains(c2))
                    {
                        continue;
                    }

                    // Rule (GB11)   (Extended_Pictographic | Emoji) ZWJ x (Extended_Pictographic | Emoji)
                    if ((fExtendedPictSet.Contains(c0) || fEmojiNRKSet.Contains(c0)) && fZWJSet.Contains(c1) &&
                            (fExtendedPictSet.Contains(c2) || fEmojiNRKSet.Contains(c2)))
                    {
                        continue;
                    }
                    if ((fExtendedPictSet.Contains(cBase) || fEmojiNRKSet.Contains(cBase)) && fExtendSet.Contains(c0) && fZWJSet.Contains(c1) &&
                            (fExtendedPictSet.Contains(c2) || fEmojiNRKSet.Contains(c2)))
                    {
                        continue;
                    }

                    // Rule (GB12-13)   Regional_Indicator x Regional_Indicator
                    //                  Note: The first if condition is a little tricky. We only need to force
                    //                      a break if there are three or more contiguous RIs. If there are
                    //                      only two, a break following will occur via other rules, and will include
                    //                      any trailing extend characters, which is needed behavior.
                    if (fRegionalIndicatorSet.Contains(c0) && fRegionalIndicatorSet.Contains(c1)
                            && fRegionalIndicatorSet.Contains(c2))
                    {
                        break;
                    }
                    if (fRegionalIndicatorSet.Contains(c1) && fRegionalIndicatorSet.Contains(c2))
                    {
                        continue;
                    }

                    // Rule (GB999)  Any  <break>  Any
                    break;
                }

                breakPos = p2;
                return breakPos;
            }
        }


        /**
         *
         * Word Monkey Test Class
         *
         *
         *
         */
        internal class RBBIWordMonkey : RBBIMonkeyKind
        {
            List<object> fSets;
            StringBuffer fText;

            internal UnicodeSet fCRSet;
            internal UnicodeSet fLFSet;
            internal UnicodeSet fNewlineSet;
            internal UnicodeSet fRegionalIndicatorSet;
            internal UnicodeSet fKatakanaSet;
            internal UnicodeSet fHebrew_LetterSet;
            internal UnicodeSet fALetterSet;
            internal UnicodeSet fSingle_QuoteSet;
            internal UnicodeSet fDouble_QuoteSet;
            internal UnicodeSet fMidNumLetSet;
            internal UnicodeSet fMidLetterSet;
            internal UnicodeSet fMidNumSet;
            internal UnicodeSet fNumericSet;
            internal UnicodeSet fFormatSet;
            internal UnicodeSet fExtendSet;
            internal UnicodeSet fExtendNumLetSet;
            internal UnicodeSet fOtherSet;
            internal UnicodeSet fDictionarySet;
            internal UnicodeSet fEBaseSet;
            internal UnicodeSet fEBGSet;
            internal UnicodeSet fEModifierSet;
            internal UnicodeSet fZWJSet;
            internal UnicodeSet fExtendedPictSet;
            internal UnicodeSet fEmojiNRKSet;


            internal RBBIWordMonkey()
            {
                fCharProperty = UProperty.WORD_BREAK;

                fCRSet = new UnicodeSet("[\\p{Word_Break = CR}]");
                fLFSet = new UnicodeSet("[\\p{Word_Break = LF}]");
                fNewlineSet = new UnicodeSet("[\\p{Word_Break = Newline}]");
                fRegionalIndicatorSet = new UnicodeSet("[\\p{Word_Break = Regional_Indicator}]");
                fKatakanaSet = new UnicodeSet("[\\p{Word_Break = Katakana}]");
                fHebrew_LetterSet = new UnicodeSet("[\\p{Word_Break = Hebrew_Letter}]");
                fALetterSet = new UnicodeSet("[\\p{Word_Break = ALetter}]");
                fSingle_QuoteSet = new UnicodeSet("[\\p{Word_Break = Single_Quote}]");
                fDouble_QuoteSet = new UnicodeSet("[\\p{Word_Break = Double_Quote}]");
                fMidNumLetSet = new UnicodeSet("[\\p{Word_Break = MidNumLet}]");
                fMidLetterSet = new UnicodeSet("[\\p{Word_Break = MidLetter}]");
                fMidNumSet = new UnicodeSet("[\\p{Word_Break = MidNum}]");
                fNumericSet = new UnicodeSet("[\\p{Word_Break = Numeric}]");
                fFormatSet = new UnicodeSet("[\\p{Word_Break = Format}]");
                fExtendNumLetSet = new UnicodeSet("[\\p{Word_Break = ExtendNumLet}]");
                fExtendSet = new UnicodeSet("[\\p{Word_Break = Extend}]");
                fEBaseSet = new UnicodeSet("[\\p{Word_Break = EB}]");
                fEBGSet = new UnicodeSet("[\\p{Word_Break = EBG}]");
                fEModifierSet = new UnicodeSet("[\\p{Word_Break = EM}]");
                fZWJSet = new UnicodeSet("[\\p{Word_Break = ZWJ}]");
                fExtendedPictSet = new UnicodeSet(gExtended_Pict);
                fEmojiNRKSet = new UnicodeSet("[[\\p{Emoji}]-[\\p{Grapheme_Cluster_Break = Regional_Indicator}*#0-9©®™〰〽]]");

                fDictionarySet = new UnicodeSet("[[\\uac00-\\ud7a3][:Han:][:Hiragana:]]");
                fDictionarySet.AddAll(fKatakanaSet);
                fDictionarySet.AddAll(new UnicodeSet("[\\p{LineBreak = Complex_Context}]"));

                fALetterSet.RemoveAll(fDictionarySet);

                fOtherSet = new UnicodeSet();
                fOtherSet.Complement();
                fOtherSet.RemoveAll(fCRSet);
                fOtherSet.RemoveAll(fLFSet);
                fOtherSet.RemoveAll(fNewlineSet);
                fOtherSet.RemoveAll(fALetterSet);
                fOtherSet.RemoveAll(fSingle_QuoteSet);
                fOtherSet.RemoveAll(fDouble_QuoteSet);
                fOtherSet.RemoveAll(fKatakanaSet);
                fOtherSet.RemoveAll(fHebrew_LetterSet);
                fOtherSet.RemoveAll(fMidLetterSet);
                fOtherSet.RemoveAll(fMidNumSet);
                fOtherSet.RemoveAll(fNumericSet);
                fOtherSet.RemoveAll(fFormatSet);
                fOtherSet.RemoveAll(fExtendSet);
                fOtherSet.RemoveAll(fExtendNumLetSet);
                fOtherSet.RemoveAll(fRegionalIndicatorSet);
                fOtherSet.RemoveAll(fEBaseSet);
                fOtherSet.RemoveAll(fEBGSet);
                fOtherSet.RemoveAll(fEModifierSet);
                fOtherSet.RemoveAll(fZWJSet);
                fOtherSet.RemoveAll(fExtendedPictSet);
                fOtherSet.RemoveAll(fEmojiNRKSet);

                // Inhibit dictionary characters from being tested at all.
                // remove surrogates so as to not generate higher CJK characters
                fOtherSet.RemoveAll(new UnicodeSet("[[\\p{LineBreak = Complex_Context}][:Line_Break=Surrogate:]]"));
                fOtherSet.RemoveAll(fDictionarySet);

                fSets = new List<object>();
                fSets.Add(fCRSet);
                fSets.Add(fLFSet);
                fSets.Add(fNewlineSet);
                fSets.Add(fRegionalIndicatorSet);
                fSets.Add(fHebrew_LetterSet);
                fSets.Add(fALetterSet);
                //fSets.Add(fKatakanaSet);  // Omit Katakana from fSets, which omits Katakana characters
                // from the test data. They are all in the dictionary set,
                // which this (old, to be retired) monkey test cannot handle.
                fSets.Add(fSingle_QuoteSet);
                fSets.Add(fDouble_QuoteSet);
                fSets.Add(fMidLetterSet);
                fSets.Add(fMidNumLetSet);
                fSets.Add(fMidNumSet);
                fSets.Add(fNumericSet);
                fSets.Add(fFormatSet);
                fSets.Add(fExtendSet);
                fSets.Add(fExtendNumLetSet);
                fSets.Add(fRegionalIndicatorSet);
                fSets.Add(fEBaseSet);
                fSets.Add(fEBGSet);
                fSets.Add(fEModifierSet);
                fSets.Add(fZWJSet);
                fSets.Add(fExtendedPictSet);
                fSets.Add(fEmojiNRKSet);
                fSets.Add(fOtherSet);
            }


            internal override IList<object> CharClasses
            {
                get { return fSets; }
            }

            internal override void SetText(StringBuffer s)
            {
                fText = s;
            }

            internal override int Next(int prevPos)
            {
                int    /*p0,*/ p1, p2, p3;      // Indices of the significant code points around the
                                                //   break position being tested.  The candidate break
                                                //   location is before p2.
                int breakPos = -1;

                int c0, c1, c2, c3;   // The code points at p0, p1, p2 & p3.

                // Previous break at end of string.  return DONE.
                if (prevPos >= fText.Length)
                {
                    return -1;
                }
                /*p0 =*/
                p1 = p2 = p3 = prevPos;
                c3 = UTF16.CharAt(fText, prevPos);
                c0 = c1 = c2 = 0;



                // Loop runs once per "significant" character position in the input text.
                for (; ; )
                {
                    // Move all of the positions forward in the input string.
                    /*p0 = p1;*/
                    c0 = c1;
                    p1 = p2; c1 = c2;
                    p2 = p3; c2 = c3;

                    // Advance p3 by    X(Extend | Format)*   Rule 4
                    //    But do not advance over Extend & Format following a new line. (Unicode 5.1 change)
                    do
                    {
                        p3 = MoveIndex32(fText, p3, 1);
                        c3 = -1;
                        if (p3 >= fText.Length)
                        {
                            break;
                        }
                        c3 = UTF16.CharAt(fText, p3);
                        if (fCRSet.Contains(c2) || fLFSet.Contains(c2) || fNewlineSet.Contains(c2))
                        {
                            break;
                        }
                    }
                    while (SetContains(fFormatSet, c3) || SetContains(fExtendSet, c3) || SetContains(fZWJSet, c3));

                    if (p1 == p2)
                    {
                        // Still warming up the loop.  (won't work with zero length strings, but we don't care)
                        continue;
                    }
                    if (p2 == fText.Length)
                    {
                        // Reached end of string.  Always a break position.
                        break;
                    }

                    // Rule (3)   CR x LF
                    //     No Extend or Format characters may appear between the CR and LF,
                    //     which requires the additional check for p2 immediately following p1.
                    //
                    if (c1 == 0x0D && c2 == 0x0A)
                    {
                        continue;
                    }

                    // Rule (3a)  Break before and after newlines (including CR and LF)
                    //
                    if (fCRSet.Contains(c1) || fLFSet.Contains(c1) || fNewlineSet.Contains(c1))
                    {
                        break;
                    }
                    if (fCRSet.Contains(c2) || fLFSet.Contains(c2) || fNewlineSet.Contains(c2))
                    {
                        break;
                    }

                    // Rule (3c)    ZWJ x (Extended_Pictographic | Emoji).
                    //              Not ignoring extend chars, so peek into input text to
                    //              get the potential ZWJ, the character immediately preceding c2.
                    if (fZWJSet.Contains(fText.CodePointBefore(p2)) && (fExtendedPictSet.Contains(c2) || fEmojiNRKSet.Contains(c2)))
                    {
                        continue;
                    }

                    // Rule (5).   (ALetter | Hebrew_Letter) x (ALetter | Hebrew_Letter)
                    if ((fALetterSet.Contains(c1) || fHebrew_LetterSet.Contains(c1)) &&
                            (fALetterSet.Contains(c2) || fHebrew_LetterSet.Contains(c2)))
                    {
                        continue;
                    }

                    // Rule (6)  (ALetter | Hebrew_Letter)  x  (MidLetter | MidNumLet | Single_Quote) (ALetter | Hebrew_Letter)
                    //
                    if ((fALetterSet.Contains(c1) || fHebrew_LetterSet.Contains(c1)) &&
                            (fMidLetterSet.Contains(c2) || fMidNumLetSet.Contains(c2) || fSingle_QuoteSet.Contains(c2)) &&
                            (SetContains(fALetterSet, c3) || SetContains(fHebrew_LetterSet, c3)))
                    {
                        continue;
                    }

                    // Rule (7)  (ALetter | Hebrew_Letter) (MidLetter | MidNumLet | Single_Quote)  x  (ALetter | Hebrew_Letter)
                    if ((fALetterSet.Contains(c0) || fHebrew_LetterSet.Contains(c0)) &&
                            (fMidLetterSet.Contains(c1) || fMidNumLetSet.Contains(c1) || fSingle_QuoteSet.Contains(c1)) &&
                            (fALetterSet.Contains(c2) || fHebrew_LetterSet.Contains(c2)))
                    {
                        continue;
                    }

                    // Rule (7a)     Hebrew_Letter x Single_Quote
                    if (fHebrew_LetterSet.Contains(c1) && fSingle_QuoteSet.Contains(c2))
                    {
                        continue;
                    }

                    // Rule (7b)    Hebrew_Letter x Double_Quote Hebrew_Letter
                    if (fHebrew_LetterSet.Contains(c1) && fDouble_QuoteSet.Contains(c2) && SetContains(fHebrew_LetterSet, c3))
                    {
                        continue;
                    }

                    // Rule (7c)    Hebrew_Letter Double_Quote x Hebrew_Letter
                    if (fHebrew_LetterSet.Contains(c0) && fDouble_QuoteSet.Contains(c1) && fHebrew_LetterSet.Contains(c2))
                    {
                        continue;
                    }

                    //  Rule (8)    Numeric x Numeric
                    if (fNumericSet.Contains(c1) &&
                            fNumericSet.Contains(c2))
                    {
                        continue;
                    }

                    // Rule (9)    (ALetter | Hebrew_Letter) x Numeric
                    if ((fALetterSet.Contains(c1) || fHebrew_LetterSet.Contains(c1)) &&
                            fNumericSet.Contains(c2))
                    {
                        continue;
                    }

                    // Rule (10)    Numeric x (ALetter | Hebrew_Letter)
                    if (fNumericSet.Contains(c1) &&
                            (fALetterSet.Contains(c2) || fHebrew_LetterSet.Contains(c2)))
                    {
                        continue;
                    }

                    // Rule (11)   Numeric (MidNum | MidNumLet | Single_Quote)  x  Numeric
                    if (fNumericSet.Contains(c0) &&
                            (fMidNumSet.Contains(c1) || fMidNumLetSet.Contains(c1) || fSingle_QuoteSet.Contains(c1)) &&
                            fNumericSet.Contains(c2))
                    {
                        continue;
                    }

                    // Rule (12)  Numeric x (MidNum | MidNumLet | SingleQuote) Numeric
                    if (fNumericSet.Contains(c1) &&
                            (fMidNumSet.Contains(c2) || fMidNumLetSet.Contains(c2) || fSingle_QuoteSet.Contains(c2)) &&
                            SetContains(fNumericSet, c3))
                    {
                        continue;
                    }

                    // Rule (13)  Katakana x Katakana
                    //            Note: matches UAX 29 rules, but doesn't come into play for ICU because
                    //                  all Katakana are handled by the dictionary breaker.
                    if (fKatakanaSet.Contains(c1) &&
                            fKatakanaSet.Contains(c2))
                    {
                        continue;
                    }

                    // Rule 13a    (ALetter | Hebrew_Letter | Numeric | KataKana | ExtendNumLet) x ExtendNumLet
                    if ((fALetterSet.Contains(c1) || fHebrew_LetterSet.Contains(c1) || fNumericSet.Contains(c1) ||
                            fKatakanaSet.Contains(c1) || fExtendNumLetSet.Contains(c1)) &&
                            fExtendNumLetSet.Contains(c2))
                    {
                        continue;
                    }

                    // Rule 13b   ExtendNumLet x (ALetter | Hebrew_Letter | Numeric | Katakana)
                    if (fExtendNumLetSet.Contains(c1) &&
                            (fALetterSet.Contains(c2) || fHebrew_LetterSet.Contains(c2) ||
                                    fNumericSet.Contains(c2) || fKatakanaSet.Contains(c2)))
                    {
                        continue;
                    }


                    // Rule 14 (E_Base | EBG) x E_Modifier
                    if ((fEBaseSet.Contains(c1) || fEBGSet.Contains(c1)) && fEModifierSet.Contains(c2))
                    {
                        continue;
                    }

                    // Rule 15 - 17   Group piars of Regional Indicators
                    if (fRegionalIndicatorSet.Contains(c0) && fRegionalIndicatorSet.Contains(c1))
                    {
                        break;
                    }
                    if (fRegionalIndicatorSet.Contains(c1) && fRegionalIndicatorSet.Contains(c2))
                    {
                        continue;
                    }

                    // Rule 999.  Break found here.
                    break;
                }

                breakPos = p2;
                return breakPos;
            }

        }


        internal class RBBILineMonkey : RBBIMonkeyKind
        {

            internal List<object> fSets;

            // UnicodeSets for each of the Line Breaking character classes.
            // Order matches that of Unicode UAX 14, Table 1, which makes it a little easier
            // to verify that they are all accounted for.

            internal UnicodeSet fBK;
            internal UnicodeSet fCR;
            internal UnicodeSet fLF;
            internal UnicodeSet fCM;
            internal UnicodeSet fNL;
            internal UnicodeSet fSG;
            internal UnicodeSet fWJ;
            internal UnicodeSet fZW;
            internal UnicodeSet fGL;
            internal UnicodeSet fSP;
            internal UnicodeSet fB2;
            internal UnicodeSet fBA;
            internal UnicodeSet fBB;
            internal UnicodeSet fHY;
            internal UnicodeSet fCB;
            internal UnicodeSet fCL;
            internal UnicodeSet fCP;
            internal UnicodeSet fEX;
            internal UnicodeSet fIN;
            internal UnicodeSet fNS;
            internal UnicodeSet fOP;
            internal UnicodeSet fQU;
            internal UnicodeSet fIS;
            internal UnicodeSet fNU;
            internal UnicodeSet fPO;
            internal UnicodeSet fPR;
            internal UnicodeSet fSY;
            internal UnicodeSet fAI;
            internal UnicodeSet fAL;
            internal UnicodeSet fCJ;
            internal UnicodeSet fH2;
            internal UnicodeSet fH3;
            internal UnicodeSet fHL;
            internal UnicodeSet fID;
            internal UnicodeSet fJL;
            internal UnicodeSet fJV;
            internal UnicodeSet fJT;
            internal UnicodeSet fRI;
            internal UnicodeSet fXX;
            internal UnicodeSet fEB;
            internal UnicodeSet fEM;
            internal UnicodeSet fZWJ;
            internal UnicodeSet fExtendedPict;
            internal UnicodeSet fEmojiNRK;

            internal StringBuffer fText;
            internal int fOrigPositions;



            internal RBBILineMonkey()
            {
                fCharProperty = UProperty.LINE_BREAK;
                fSets = new List<object>();

                fBK = new UnicodeSet("[\\p{Line_Break=BK}]");
                fCR = new UnicodeSet("[\\p{Line_break=CR}]");
                fLF = new UnicodeSet("[\\p{Line_break=LF}]");
                fCM = new UnicodeSet("[\\p{Line_break=CM}]");
                fNL = new UnicodeSet("[\\p{Line_break=NL}]");
                fSG = new UnicodeSet("[\\ud800-\\udfff]");
                fWJ = new UnicodeSet("[\\p{Line_break=WJ}]");
                fZW = new UnicodeSet("[\\p{Line_break=ZW}]");
                fGL = new UnicodeSet("[\\p{Line_break=GL}]");
                fSP = new UnicodeSet("[\\p{Line_break=SP}]");
                fB2 = new UnicodeSet("[\\p{Line_break=B2}]");
                fBA = new UnicodeSet("[\\p{Line_break=BA}]");
                fBB = new UnicodeSet("[\\p{Line_break=BB}]");
                fHY = new UnicodeSet("[\\p{Line_break=HY}]");
                fCB = new UnicodeSet("[\\p{Line_break=CB}]");
                fCL = new UnicodeSet("[\\p{Line_break=CL}]");
                fCP = new UnicodeSet("[\\p{Line_break=CP}]");
                fEX = new UnicodeSet("[\\p{Line_break=EX}]");
                fIN = new UnicodeSet("[\\p{Line_break=IN}]");
                fNS = new UnicodeSet("[\\p{Line_break=NS}]");
                fOP = new UnicodeSet("[\\p{Line_break=OP}]");
                fQU = new UnicodeSet("[\\p{Line_break=QU}]");
                fIS = new UnicodeSet("[\\p{Line_break=IS}]");
                fNU = new UnicodeSet("[\\p{Line_break=NU}]");
                fPO = new UnicodeSet("[\\p{Line_break=PO}]");
                fPR = new UnicodeSet("[\\p{Line_break=PR}]");
                fSY = new UnicodeSet("[\\p{Line_break=SY}]");
                fAI = new UnicodeSet("[\\p{Line_break=AI}]");
                fAL = new UnicodeSet("[\\p{Line_break=AL}]");
                fCJ = new UnicodeSet("[\\p{Line_break=CJ}]");
                fH2 = new UnicodeSet("[\\p{Line_break=H2}]");
                fH3 = new UnicodeSet("[\\p{Line_break=H3}]");
                fHL = new UnicodeSet("[\\p{Line_break=HL}]");
                fID = new UnicodeSet("[\\p{Line_break=ID}]");
                fJL = new UnicodeSet("[\\p{Line_break=JL}]");
                fJV = new UnicodeSet("[\\p{Line_break=JV}]");
                fJT = new UnicodeSet("[\\p{Line_break=JT}]");
                fRI = new UnicodeSet("[\\p{Line_break=RI}]");
                fXX = new UnicodeSet("[\\p{Line_break=XX}]");
                fEB = new UnicodeSet("[\\p{Line_break=EB}]");
                fEM = new UnicodeSet("[\\p{Line_break=EM}]");
                fZWJ = new UnicodeSet("[\\p{Line_break=ZWJ}]");
                fEmojiNRK = new UnicodeSet("[[\\p{Emoji}]-[\\p{Line_break=RI}*#0-9©®™〰〽]]");
                fExtendedPict = new UnicodeSet(gExtended_Pict);


                // Remove dictionary characters.
                // The monkey test reference implementation of line break does not replicate the dictionary behavior,
                // so dictionary characters are omitted from the monkey test data.
                UnicodeSet dictionarySet = new UnicodeSet(
                        "[[:LineBreak = Complex_Context:] & [[:Script = Thai:][:Script = Lao:][:Script = Khmer:] [:script = Myanmar:]]]");

                fAL.AddAll(fXX);     // Default behavior for XX is identical to AL
                fAL.AddAll(fAI);     // Default behavior for AI is identical to AL
                fAL.AddAll(fSG);     // Default behavior for SG (unpaired surrogates) is AL

                fNS.AddAll(fCJ);     // Default behavior for CJ is identical to NS.
                fCM.AddAll(fZWJ);    // ZWJ behaves as a CM.

                fSets.Add(fBK);
                fSets.Add(fCR);
                fSets.Add(fLF);
                fSets.Add(fCM);
                fSets.Add(fNL);
                fSets.Add(fWJ);
                fSets.Add(fZW);
                fSets.Add(fGL);
                fSets.Add(fSP);
                fSets.Add(fB2);
                fSets.Add(fBA);
                fSets.Add(fBB);
                fSets.Add(fHY);
                fSets.Add(fCB);
                fSets.Add(fCL);
                fSets.Add(fCP);
                fSets.Add(fEX);
                fSets.Add(fIN);
                fSets.Add(fJL);
                fSets.Add(fJT);
                fSets.Add(fJV);
                fSets.Add(fNS);
                fSets.Add(fOP);
                fSets.Add(fQU);
                fSets.Add(fIS);
                fSets.Add(fNU);
                fSets.Add(fPO);
                fSets.Add(fPR);
                fSets.Add(fSY);
                fSets.Add(fAI);
                fSets.Add(fAL);
                fSets.Add(fH2);
                fSets.Add(fH3);
                fSets.Add(fHL);
                fSets.Add(fID);
                fSets.Add(fWJ);
                fSets.Add(fRI);
                fSets.Add(fSG);
                fSets.Add(fEB);
                fSets.Add(fEM);
                fSets.Add(fZWJ);
                fSets.Add(fExtendedPict);
                fSets.Add(fEmojiNRK);
            }

            internal override void SetText(StringBuffer s)
            {
                fText = s;
            }




            internal override int Next(int startPos)
            {
                int pos;       //  Index of the char following a potential break position
                int thisChar;  //  Character at above position "pos"

                int prevPos;   //  Index of the char preceding a potential break position
                int prevChar;  //  Character at above position.  Note that prevChar
                               //   and thisChar may not be adjacent because combining
                               //   characters between them will be ignored.
                int prevCharX2; //  Character before prevChar, more contex for LB 21a

                int nextPos;   //  Index of the next character following pos.
                               //     Usually skips over combining marks.
                int tPos;      //  temp value.
                int[] matchVals = null;       // Number  Expression Match Results


                if (startPos >= fText.Length)
                {
                    return -1;
                }


                // Initial values for loop.  Loop will run the first time without finding breaks,
                //                           while the invalid values shift out and the "this" and
                //                           "prev" positions are filled in with good values.
                pos = prevPos = -1;    // Invalid value, serves as flag for initial loop iteration.
                thisChar = prevChar = prevCharX2 = 0;
                nextPos = startPos;


                // Loop runs once per position in the test text, until a break position
                //  is found.  In each iteration, we are testing for a possible break
                //  just preceding the character at index "pos".  The character preceding
                //  this char is at postion "prevPos"; because of combining sequences,
                //  "prevPos" can be arbitrarily far before "pos".
                for (; ; )
                {
                    // Advance to the next position to be tested.
                    prevCharX2 = prevChar;
                    prevPos = pos;
                    prevChar = thisChar;
                    pos = nextPos;
                    nextPos = MoveIndex32(fText, pos, 1);

                    // Rule LB2 - Break at end of text.
                    if (pos >= fText.Length)
                    {
                        break;
                    }

                    // Rule LB 9 - adjust for combining sequences.
                    //             We do this rule out-of-order because the adjustment does
                    //             not effect the way that rules LB 3 through LB 6 match,
                    //             and doing it here rather than after LB 6 is substantially
                    //             simpler when combining sequences do occur.


                    // LB 9         Keep combining sequences together.
                    //              advance over any CM class chars at "pos",
                    //              result is "nextPos" for the following loop iteration.
                    thisChar = UTF16.CharAt(fText, pos);
                    if (!(fSP.Contains(thisChar) || fBK.Contains(thisChar) || thisChar == 0x0d ||
                            thisChar == 0x0a || fNL.Contains(thisChar) || fZW.Contains(thisChar)))
                    {
                        for (; ; )
                        {
                            if (nextPos == fText.Length)
                            {
                                break;
                            }
                            int nextChar = UTF16.CharAt(fText, nextPos);
                            if (!fCM.Contains(nextChar))
                            {
                                break;
                            }
                            nextPos = MoveIndex32(fText, nextPos, 1);
                        }
                    }

                    // LB 9 Treat X CM* as if it were X
                    //        No explicit action required.

                    // LB 10     Treat any remaining combining mark as AL
                    if (fCM.Contains(thisChar))
                    {
                        thisChar = 'A';
                    }


                    // If the loop is still warming up - if we haven't shifted the initial
                    //   -1 positions out of prevPos yet - loop back to advance the
                    //    position in the input without any further looking for breaks.
                    if (prevPos == -1)
                    {
                        continue;
                    }

                    // LB 4  Always break after hard line breaks,
                    if (fBK.Contains(prevChar))
                    {
                        break;
                    }

                    // LB 5  Break after CR, LF, NL, but not inside CR LF
                    if (fCR.Contains(prevChar) && fLF.Contains(thisChar))
                    {
                        continue;
                    }
                    if (fCR.Contains(prevChar) ||
                            fLF.Contains(prevChar) ||
                            fNL.Contains(prevChar))
                    {
                        break;
                    }

                    // LB 6  Don't break before hard line breaks
                    if (fBK.Contains(thisChar) || fCR.Contains(thisChar) ||
                            fLF.Contains(thisChar) || fNL.Contains(thisChar))
                    {
                        continue;
                    }


                    // LB 7  Don't break before spaces or zero-width space.
                    if (fSP.Contains(thisChar))
                    {
                        continue;
                    }

                    if (fZW.Contains(thisChar))
                    {
                        continue;
                    }

                    // LB 8  Break after zero width space
                    if (fZW.Contains(prevChar))
                    {
                        break;
                    }

                    // LB 8a:  ZWJ x (ID | Extended_Pictographic | Emoji)
                    //       The monkey test's way of ignoring combining characters doesn't work
                    //       for this rule. ZWJ is also a CM. Need to get the actual character
                    //       preceding "thisChar", not ignoring combining marks, possibly ZWJ.
                    {
                        int prevC = fText.CodePointBefore(pos);
                        if (fZWJ.Contains(prevC) && (fID.Contains(thisChar) || fExtendedPict.Contains(thisChar) || fEmojiNRK.Contains(thisChar)))
                        {
                            continue;
                        }
                    }

                    //  LB 9, 10  Already done, at top of loop.
                    //


                    // LB 11
                    //    x  WJ
                    //    WJ  x
                    if (fWJ.Contains(thisChar) || fWJ.Contains(prevChar))
                    {
                        continue;
                    }


                    // LB 12
                    //        GL x
                    if (fGL.Contains(prevChar))
                    {
                        continue;
                    }

                    // LB 12a
                    //    [^SP BA HY] x GL
                    if (!(fSP.Contains(prevChar) ||
                            fBA.Contains(prevChar) ||
                            fHY.Contains(prevChar)) && fGL.Contains(thisChar))
                    {
                        continue;
                    }



                    // LB 13  Don't break before closings.
                    //       NU x CL, NU x CP  and NU x IS are not matched here so that they will
                    //       fall into LB 17 and the more general number regular expression.
                    //
                    if (!fNU.Contains(prevChar) && fCL.Contains(thisChar) ||
                            !fNU.Contains(prevChar) && fCP.Contains(thisChar) ||
                            fEX.Contains(thisChar) ||
                            !fNU.Contains(prevChar) && fIS.Contains(thisChar) ||
                            !fNU.Contains(prevChar) && fSY.Contains(thisChar))
                    {
                        continue;
                    }

                    // LB 14  Don't break after OP SP*
                    //       Scan backwards, checking for this sequence.
                    //       The OP char could include combining marks, so we actually check for
                    //           OP CM* SP* x
                    tPos = prevPos;
                    if (fSP.Contains(prevChar))
                    {
                        while (tPos > 0 && fSP.Contains(UTF16.CharAt(fText, tPos)))
                        {
                            tPos = MoveIndex32(fText, tPos, -1);
                        }
                    }
                    while (tPos > 0 && fCM.Contains(UTF16.CharAt(fText, tPos)))
                    {
                        tPos = MoveIndex32(fText, tPos, -1);
                    }
                    if (fOP.Contains(UTF16.CharAt(fText, tPos)))
                    {
                        continue;
                    }

                    // LB 15 Do not break within "[
                    //       QU CM* SP* x OP
                    if (fOP.Contains(thisChar))
                    {
                        // Scan backwards from prevChar to see if it is preceded by QU CM* SP*
                        tPos = prevPos;
                        while (tPos > 0 && fSP.Contains(UTF16.CharAt(fText, tPos)))
                        {
                            tPos = MoveIndex32(fText, tPos, -1);
                        }
                        while (tPos > 0 && fCM.Contains(UTF16.CharAt(fText, tPos)))
                        {
                            tPos = MoveIndex32(fText, tPos, -1);
                        }
                        if (fQU.Contains(UTF16.CharAt(fText, tPos)))
                        {
                            continue;
                        }
                    }

                    // LB 16   (CL | CP) SP* x NS
                    if (fNS.Contains(thisChar))
                    {
                        tPos = prevPos;
                        while (tPos > 0 && fSP.Contains(UTF16.CharAt(fText, tPos)))
                        {
                            tPos = MoveIndex32(fText, tPos, -1);
                        }
                        while (tPos > 0 && fCM.Contains(UTF16.CharAt(fText, tPos)))
                        {
                            tPos = MoveIndex32(fText, tPos, -1);
                        }
                        if (fCL.Contains(UTF16.CharAt(fText, tPos)) || fCP.Contains(UTF16.CharAt(fText, tPos)))
                        {
                            continue;
                        }
                    }


                    // LB 17        B2 SP* x B2
                    if (fB2.Contains(thisChar))
                    {
                        tPos = prevPos;
                        while (tPos > 0 && fSP.Contains(UTF16.CharAt(fText, tPos)))
                        {
                            tPos = MoveIndex32(fText, tPos, -1);
                        }
                        while (tPos > 0 && fCM.Contains(UTF16.CharAt(fText, tPos)))
                        {
                            tPos = MoveIndex32(fText, tPos, -1);
                        }
                        if (fB2.Contains(UTF16.CharAt(fText, tPos)))
                        {
                            continue;
                        }
                    }

                    // LB 18    break after space
                    if (fSP.Contains(prevChar))
                    {
                        break;
                    }

                    // LB 19
                    //    x   QU
                    //    QU  x
                    if (fQU.Contains(thisChar) || fQU.Contains(prevChar))
                    {
                        continue;
                    }

                    // LB 20  Break around a CB
                    if (fCB.Contains(thisChar) || fCB.Contains(prevChar))
                    {
                        break;
                    }

                    // LB 21
                    if (fBA.Contains(thisChar) ||
                            fHY.Contains(thisChar) ||
                            fNS.Contains(thisChar) ||
                            fBB.Contains(prevChar))
                    {
                        continue;
                    }

                    // LB 21a, HL (HY | BA) x
                    if (fHL.Contains(prevCharX2) && (fHY.Contains(prevChar) || fBA.Contains(prevChar)))
                    {
                        continue;
                    }

                    // LB 21b, SY x HL
                    if (fSY.Contains(prevChar) && fHL.Contains(thisChar))
                    {
                        continue;
                    }

                    // LB 22
                    if (fAL.Contains(prevChar) && fIN.Contains(thisChar) ||
                            fEX.Contains(prevChar) && fIN.Contains(thisChar) ||
                            fHL.Contains(prevChar) && fIN.Contains(thisChar) ||
                            (fID.Contains(prevChar) || fEB.Contains(prevChar) || fEM.Contains(prevChar)) && fIN.Contains(thisChar) ||
                            fIN.Contains(prevChar) && fIN.Contains(thisChar) ||
                            fNU.Contains(prevChar) && fIN.Contains(thisChar))
                    {
                        continue;
                    }

                    // LB 23    (AL | HL) x NU
                    //          NU x (AL | HL)
                    if ((fAL.Contains(prevChar) || fHL.Contains(prevChar)) && fNU.Contains(thisChar))
                    {
                        continue;
                    }
                    if (fNU.Contains(prevChar) && (fAL.Contains(thisChar) || fHL.Contains(thisChar)))
                    {
                        continue;
                    }

                    // LB 23a Do not break between numeric prefixes and ideographs, or between ideographs and numeric postfixes.
                    //      PR x (ID | EB | EM)
                    //     (ID | EB | EM) x PO
                    if (fPR.Contains(prevChar) &&
                            (fID.Contains(thisChar) || fEB.Contains(thisChar) || fEM.Contains(thisChar)))
                    {
                        continue;
                    }
                    if ((fID.Contains(prevChar) || fEB.Contains(prevChar) || fEM.Contains(prevChar)) &&
                            fPO.Contains(thisChar))
                    {
                        continue;
                    }

                    // LB 24  Do not break between prefix and letters or ideographs.
                    //         (PR | PO) x (AL | HL)
                    //         (AL | HL) x (PR | PO)
                    if ((fPR.Contains(prevChar) || fPO.Contains(prevChar)) &&
                            (fAL.Contains(thisChar) || fHL.Contains(thisChar)))
                    {
                        continue;
                    }
                    if ((fAL.Contains(prevChar) || fHL.Contains(prevChar)) &&
                            (fPR.Contains(thisChar) || fPO.Contains(thisChar)))
                    {
                        continue;
                    }


                    // LB 25    Numbers
                    matchVals = LBNumberCheck(fText, prevPos, matchVals);
                    if (matchVals[0] != -1)
                    {
                        // Matched a number.  But could have been just a single digit, which would
                        //    not represent a "no break here" between prevChar and thisChar
                        int numEndIdx = matchVals[1];  // idx of first char following num
                        if (numEndIdx > pos)
                        {
                            // Number match includes at least the two chars being checked
                            if (numEndIdx > nextPos)
                            {
                                // Number match includes additional chars.  Update pos and nextPos
                                //   so that next loop iteration will continue at the end of the number,
                                //   checking for breaks between last char in number & whatever follows.
                                nextPos = numEndIdx;
                                pos = numEndIdx;
                                do
                                {
                                    pos = MoveIndex32(fText, pos, -1);
                                    thisChar = UTF16.CharAt(fText, pos);
                                }
                                while (fCM.Contains(thisChar));
                            }
                            continue;
                        }
                    }


                    // LB 26  Do not break Korean Syllables
                    if (fJL.Contains(prevChar) && (fJL.Contains(thisChar) ||
                            fJV.Contains(thisChar) ||
                            fH2.Contains(thisChar) ||
                            fH3.Contains(thisChar)))
                    {
                        continue;
                    }

                    if ((fJV.Contains(prevChar) || fH2.Contains(prevChar)) &&
                            (fJV.Contains(thisChar) || fJT.Contains(thisChar)))
                    {
                        continue;
                    }

                    if ((fJT.Contains(prevChar) || fH3.Contains(prevChar)) &&
                            fJT.Contains(thisChar))
                    {
                        continue;
                    }

                    // LB 27 Treat a Korean Syllable Block the same as ID
                    if ((fJL.Contains(prevChar) || fJV.Contains(prevChar) ||
                            fJT.Contains(prevChar) || fH2.Contains(prevChar) || fH3.Contains(prevChar)) &&
                            fIN.Contains(thisChar))
                    {
                        continue;
                    }
                    if ((fJL.Contains(prevChar) || fJV.Contains(prevChar) ||
                            fJT.Contains(prevChar) || fH2.Contains(prevChar) || fH3.Contains(prevChar)) &&
                            fPO.Contains(thisChar))
                    {
                        continue;
                    }
                    if (fPR.Contains(prevChar) && (fJL.Contains(thisChar) || fJV.Contains(thisChar) ||
                            fJT.Contains(thisChar) || fH2.Contains(thisChar) || fH3.Contains(thisChar)))
                    {
                        continue;
                    }



                    // LB 28 Do not break between alphabetics
                    if ((fAL.Contains(prevChar) || fHL.Contains(prevChar)) && (fAL.Contains(thisChar) || fHL.Contains(thisChar)))
                    {
                        continue;
                    }

                    // LB 29  Do not break between numeric punctuation and alphabetics
                    if (fIS.Contains(prevChar) && (fAL.Contains(thisChar) || fHL.Contains(thisChar)))
                    {
                        continue;
                    }

                    // LB 30    Do not break between letters, numbers, or ordinary symbols and opening or closing punctuation.
                    //          (AL | NU) x OP
                    //          CP x (AL | NU)
                    if ((fAL.Contains(prevChar) || fHL.Contains(prevChar) || fNU.Contains(prevChar)) && fOP.Contains(thisChar))
                    {
                        continue;
                    }
                    if (fCP.Contains(prevChar) && (fAL.Contains(thisChar) || fHL.Contains(thisChar) || fNU.Contains(thisChar)))
                    {
                        continue;
                    }

                    // LB 30a   Break between pairs of Regional Indicators.
                    //             RI RI <break> RI
                    //             RI    x    RI
                    if (fRI.Contains(prevCharX2) && fRI.Contains(prevChar) && fRI.Contains(thisChar))
                    {
                        break;
                    }
                    if (fRI.Contains(prevChar) && fRI.Contains(thisChar))
                    {
                        continue;
                    }

                    // LB30b    Emoji Base x Emoji Modifier
                    if (fEB.Contains(prevChar) && fEM.Contains(thisChar))
                    {
                        continue;
                    }
                    // LB 31    Break everywhere else
                    break;
                }

                return pos;
            }



            // Match the following regular expression in the input text.
            //    ((PR | PO) CM*)? ((OP | HY) CM*)? NU CM* ((NU | IS | SY) CM*) * ((CL | CP) CM*)?  (PR | PO) CM*)?
            //      0    0   1       3    3    4              7    7    7    7      9    9    9     11   11    (match states)
            //  retVals array  [0]  index of the start of the match, or -1 if no match
            //                 [1]  index of first char following the match.
            //  Can not use Java regex because need supplementary character support,
            //     and because Unicode char properties version must be the same as in
            //     the version of ICU being tested.
            private int[] LBNumberCheck(StringBuffer s, int startIdx, int[] retVals)
            {
                if (retVals == null)
                {
                    retVals = new int[2];
                }
                retVals[0] = -1;  // Indicates no match.
                int matchState = 0;
                int idx = startIdx;

                //matchLoop:
                for (idx = startIdx; idx < s.Length; idx = MoveIndex32(s, idx, 1))
                {
                    int c = UTF16.CharAt(s, idx);
                    int cLBType = UCharacter.GetInt32PropertyValue(c, UProperty.LINE_BREAK);
                    switch (matchState)
                    {
                        case 0:
                            if (cLBType == UCharacter.LineBreak.PREFIX_NUMERIC ||
                            cLBType == UCharacter.LineBreak.POSTFIX_NUMERIC)
                            {
                                matchState = 1;
                                break;
                            }
                            if (cLBType == UCharacter.LineBreak.OPEN_PUNCTUATION)
                            {
                                matchState = 4;
                                break;
                            }
                            if (cLBType == UCharacter.LineBreak.HYPHEN)
                            {
                                matchState = 4;
                                break;
                            }
                            if (cLBType == UCharacter.LineBreak.NUMERIC)
                            {
                                matchState = 7;
                                break;
                            }
                            goto matchLoop_break;   /* No Match  */

                        case 1:
                            if (cLBType == UCharacter.LineBreak.COMBINING_MARK || cLBType == UCharacter.LineBreak.ZWJ)
                            {
                                matchState = 1;
                                break;
                            }
                            if (cLBType == UCharacter.LineBreak.OPEN_PUNCTUATION)
                            {
                                matchState = 4;
                                break;
                            }
                            if (cLBType == UCharacter.LineBreak.HYPHEN)
                            {
                                matchState = 4;
                                break;
                            }
                            if (cLBType == UCharacter.LineBreak.NUMERIC)
                            {
                                matchState = 7;
                                break;
                            }
                            goto matchLoop_break;   /* No Match  */


                        case 4:
                            if (cLBType == UCharacter.LineBreak.COMBINING_MARK || cLBType == UCharacter.LineBreak.ZWJ)
                            {
                                matchState = 4;
                                break;
                            }
                            if (cLBType == UCharacter.LineBreak.NUMERIC)
                            {
                                matchState = 7;
                                break;
                            }
                            goto matchLoop_break;   /* No Match  */
                                                    //    ((PR | PO) CM*)? ((OP | HY) CM*)? NU CM* ((NU | IS | SY) CM*) * (CL CM*)?  (PR | PO) CM*)?
                                                    //      0    0   1       3    3    4              7    7    7    7      9   9     11   11    (match states)

                        case 7:
                            if (cLBType == UCharacter.LineBreak.COMBINING_MARK || cLBType == UCharacter.LineBreak.ZWJ)
                            {
                                matchState = 7;
                                break;
                            }
                            if (cLBType == UCharacter.LineBreak.NUMERIC)
                            {
                                matchState = 7;
                                break;
                            }
                            if (cLBType == UCharacter.LineBreak.INFIX_NUMERIC)
                            {
                                matchState = 7;
                                break;
                            }
                            if (cLBType == UCharacter.LineBreak.BREAK_SYMBOLS)
                            {
                                matchState = 7;
                                break;
                            }
                            if (cLBType == UCharacter.LineBreak.CLOSE_PUNCTUATION)
                            {
                                matchState = 9;
                                break;
                            }
                            if (cLBType == UCharacter.LineBreak.CLOSE_PARENTHESIS)
                            {
                                matchState = 9;
                                break;
                            }
                            if (cLBType == UCharacter.LineBreak.POSTFIX_NUMERIC)
                            {
                                matchState = 11;
                                break;
                            }
                            if (cLBType == UCharacter.LineBreak.PREFIX_NUMERIC)
                            {
                                matchState = 11;
                                break;
                            }

                            goto matchLoop_break;    // Match Complete.
                        case 9:
                            if (cLBType == UCharacter.LineBreak.COMBINING_MARK || cLBType == UCharacter.LineBreak.ZWJ)
                            {
                                matchState = 9;
                                break;
                            }
                            if (cLBType == UCharacter.LineBreak.POSTFIX_NUMERIC)
                            {
                                matchState = 11;
                                break;
                            }
                            if (cLBType == UCharacter.LineBreak.PREFIX_NUMERIC)
                            {
                                matchState = 11;
                                break;
                            }
                            goto matchLoop_break;    // Match Complete.
                        case 11:
                            if (cLBType == UCharacter.LineBreak.COMBINING_MARK || cLBType == UCharacter.LineBreak.ZWJ)
                            {
                                matchState = 11;
                                break;
                            }
                            goto matchLoop_break;    // Match Complete.
                    }
                }
                matchLoop_break: { }
                if (matchState > 4)
                {
                    retVals[0] = startIdx;
                    retVals[1] = idx;
                }
                return retVals;
            }


            internal override IList<object> CharClasses
            {
                get { return fSets; }
            }



        }


        /**
         *
         * Sentence Monkey Test Class
         *
         *
         *
         */
        internal class RBBISentenceMonkey : RBBIMonkeyKind
        {
            internal List<object> fSets;
            internal StringBuffer fText;

            internal UnicodeSet fSepSet;
            internal UnicodeSet fFormatSet;
            internal UnicodeSet fSpSet;
            internal UnicodeSet fLowerSet;
            internal UnicodeSet fUpperSet;
            internal UnicodeSet fOLetterSet;
            internal UnicodeSet fNumericSet;
            internal UnicodeSet fATermSet;
            internal UnicodeSet fSContinueSet;
            internal UnicodeSet fSTermSet;
            internal UnicodeSet fCloseSet;
            internal UnicodeSet fOtherSet;
            internal UnicodeSet fExtendSet;



            internal RBBISentenceMonkey()
            {
                fCharProperty = UProperty.SENTENCE_BREAK;

                fSets = new List<object>();

                //  Separator Set Note:  Beginning with Unicode 5.1, CR and LF were removed from the separator
                //                       set and made into character classes of their own.  For the monkey impl,
                //                       they remain in SEP, since Sep always appears with CR and LF in the rules.
                fSepSet = new UnicodeSet("[\\p{Sentence_Break = Sep} \\u000a \\u000d]");
                fFormatSet = new UnicodeSet("[\\p{Sentence_Break = Format}]");
                fSpSet = new UnicodeSet("[\\p{Sentence_Break = Sp}]");
                fLowerSet = new UnicodeSet("[\\p{Sentence_Break = Lower}]");
                fUpperSet = new UnicodeSet("[\\p{Sentence_Break = Upper}]");
                fOLetterSet = new UnicodeSet("[\\p{Sentence_Break = OLetter}]");
                fNumericSet = new UnicodeSet("[\\p{Sentence_Break = Numeric}]");
                fATermSet = new UnicodeSet("[\\p{Sentence_Break = ATerm}]");
                fSContinueSet = new UnicodeSet("[\\p{Sentence_Break = SContinue}]");
                fSTermSet = new UnicodeSet("[\\p{Sentence_Break = STerm}]");
                fCloseSet = new UnicodeSet("[\\p{Sentence_Break = Close}]");
                fExtendSet = new UnicodeSet("[\\p{Sentence_Break = Extend}]");
                fOtherSet = new UnicodeSet();


                fOtherSet.Complement();
                fOtherSet.RemoveAll(fSepSet);
                fOtherSet.RemoveAll(fFormatSet);
                fOtherSet.RemoveAll(fSpSet);
                fOtherSet.RemoveAll(fLowerSet);
                fOtherSet.RemoveAll(fUpperSet);
                fOtherSet.RemoveAll(fOLetterSet);
                fOtherSet.RemoveAll(fNumericSet);
                fOtherSet.RemoveAll(fATermSet);
                fOtherSet.RemoveAll(fSContinueSet);
                fOtherSet.RemoveAll(fSTermSet);
                fOtherSet.RemoveAll(fCloseSet);
                fOtherSet.RemoveAll(fExtendSet);

                fSets.Add(fSepSet);
                fSets.Add(fFormatSet);

                fSets.Add(fSpSet);
                fSets.Add(fLowerSet);
                fSets.Add(fUpperSet);
                fSets.Add(fOLetterSet);
                fSets.Add(fNumericSet);
                fSets.Add(fATermSet);
                fSets.Add(fSContinueSet);
                fSets.Add(fSTermSet);
                fSets.Add(fCloseSet);
                fSets.Add(fOtherSet);
                fSets.Add(fExtendSet);
            }


            internal override IList<object> CharClasses
            {
                get { return fSets; }
            }

            internal override void SetText(StringBuffer s)
            {
                fText = s;
            }


            //      moveBack()   Find the "significant" code point preceding the index i.
            //      Skips over ($Extend | $Format)*
            //
            private int MoveBack(int i)
            {

                if (i <= 0)
                {
                    return -1;
                }

                int c;
                int j = i;
                do
                {
                    j = MoveIndex32(fText, j, -1);
                    c = UTF16.CharAt(fText, j);
                }
                while (j > 0 && (fFormatSet.Contains(c) || fExtendSet.Contains(c)));
                return j;
            }


            internal int MoveForward(int i)
            {
                if (i >= fText.Length)
                {
                    return fText.Length;
                }
                int c;
                int j = i;
                do
                {
                    j = MoveIndex32(fText, j, 1);
                    c = CAt(j);
                }
                while (c >= 0 && (fFormatSet.Contains(c) || fExtendSet.Contains(c)));
                return j;

            }

            internal int CAt(int pos)
            {
                if (pos < 0 || pos >= fText.Length)
                {
                    return -1;
                }
                return UTF16.CharAt(fText, pos);
            }

            internal override int Next(int prevPos)
            {
                int    /*p0,*/ p1, p2, p3;      // Indices of the significant code points around the
                                                //   break position being tested.  The candidate break
                                                //   location is before p2.
                int breakPos = -1;

                int c0, c1, c2, c3;         // The code points at p0, p1, p2 & p3.
                int c;

                // Prev break at end of string.  return DONE.
                if (prevPos >= fText.Length)
                {
                    return -1;
                }
                /*p0 =*/
                p1 = p2 = p3 = prevPos;
                c3 = UTF16.CharAt(fText, prevPos);
                c0 = c1 = c2 = 0;

                // Loop runs once per "significant" character position in the input text.
                for (; ; )
                {
                    // Move all of the positions forward in the input string.
                    /*p0 = p1;*/
                    c0 = c1;
                    p1 = p2; c1 = c2;
                    p2 = p3; c2 = c3;

                    // Advancd p3 by  X(Extend | Format)*   Rule 4
                    p3 = MoveForward(p3);
                    c3 = CAt(p3);

                    // Rule (3) CR x LF
                    if (c1 == 0x0d && c2 == 0x0a && p2 == (p1 + 1))
                    {
                        continue;
                    }

                    // Rule (4)    Sep  <break>
                    if (fSepSet.Contains(c1))
                    {
                        p2 = p1 + 1;   // Separators don't combine with Extend or Format
                        break;
                    }

                    if (p2 >= fText.Length)
                    {
                        // Reached end of string.  Always a break position.
                        break;
                    }

                    if (p2 == prevPos)
                    {
                        // Still warming up the loop.  (won't work with zero length strings, but we don't care)
                        continue;
                    }

                    // Rule (6).   ATerm x Numeric
                    if (fATermSet.Contains(c1) && fNumericSet.Contains(c2))
                    {
                        continue;
                    }

                    // Rule (7).  (Upper | Lower) ATerm  x  Uppper
                    if ((fUpperSet.Contains(c0) || fLowerSet.Contains(c0)) &&
                            fATermSet.Contains(c1) && fUpperSet.Contains(c2))
                    {
                        continue;
                    }

                    // Rule (8)  ATerm Close* Sp*  x  (not (OLettter | Upper | Lower | Sep))* Lower
                    //           Note:  Sterm | ATerm are added to the negated part of the expression by a
                    //                  note to the Unicode 5.0 documents.
                    int p8 = p1;
                    while (p8 > 0 && fSpSet.Contains(CAt(p8)))
                    {
                        p8 = MoveBack(p8);
                    }
                    while (p8 > 0 && fCloseSet.Contains(CAt(p8)))
                    {
                        p8 = MoveBack(p8);
                    }
                    if (fATermSet.Contains(CAt(p8)))
                    {
                        p8 = p2;
                        for (; ; )
                        {
                            c = CAt(p8);
                            if (c == -1 || fOLetterSet.Contains(c) || fUpperSet.Contains(c) ||
                                    fLowerSet.Contains(c) || fSepSet.Contains(c) ||
                                    fATermSet.Contains(c) || fSTermSet.Contains(c))
                            {
                                break;
                            }
                            p8 = MoveForward(p8);
                        }
                        if (p8 < fText.Length && fLowerSet.Contains(CAt(p8)))
                        {
                            continue;
                        }
                    }

                    // Rule 8a  (STerm | ATerm) Close* Sp* x (SContinue | Sterm | ATerm)
                    if (fSContinueSet.Contains(c2) || fSTermSet.Contains(c2) || fATermSet.Contains(c2))
                    {
                        p8 = p1;
                        while (SetContains(fSpSet, CAt(p8)))
                        {
                            p8 = MoveBack(p8);
                        }
                        while (SetContains(fCloseSet, CAt(p8)))
                        {
                            p8 = MoveBack(p8);
                        }
                        c = CAt(p8);
                        if (SetContains(fSTermSet, c) || SetContains(fATermSet, c))
                        {
                            continue;
                        }
                    }


                    // Rule (9)  (STerm | ATerm) Close*  x  (Close | Sp | Sep | CR | LF)
                    int p9 = p1;
                    while (p9 > 0 && fCloseSet.Contains(CAt(p9)))
                    {
                        p9 = MoveBack(p9);
                    }
                    c = CAt(p9);
                    if ((fSTermSet.Contains(c) || fATermSet.Contains(c)))
                    {
                        if (fCloseSet.Contains(c2) || fSpSet.Contains(c2) || fSepSet.Contains(c2))
                        {
                            continue;
                        }
                    }

                    // Rule (10)  (Sterm | ATerm) Close* Sp*  x  (Sp | Sep | CR | LF)
                    int p10 = p1;
                    while (p10 > 0 && fSpSet.Contains(CAt(p10)))
                    {
                        p10 = MoveBack(p10);
                    }
                    while (p10 > 0 && fCloseSet.Contains(CAt(p10)))
                    {
                        p10 = MoveBack(p10);
                    }
                    if (fSTermSet.Contains(CAt(p10)) || fATermSet.Contains(CAt(p10)))
                    {
                        if (fSpSet.Contains(c2) || fSepSet.Contains(c2))
                        {
                            continue;
                        }
                    }

                    // Rule (11)  (STerm | ATerm) Close* Sp*   <break>
                    int p11 = p1;
                    if (p11 > 0 && fSepSet.Contains(CAt(p11)))
                    {
                        p11 = MoveBack(p11);
                    }
                    while (p11 > 0 && fSpSet.Contains(CAt(p11)))
                    {
                        p11 = MoveBack(p11);
                    }
                    while (p11 > 0 && fCloseSet.Contains(CAt(p11)))
                    {
                        p11 = MoveBack(p11);
                    }
                    if (fSTermSet.Contains(CAt(p11)) || fATermSet.Contains(CAt(p11)))
                    {
                        break;
                    }

                    //  Rule (12)  Any x Any
                    continue;
                }
                breakPos = p2;
                return breakPos;
            }



        }


        /**
         * Move an index into a string by n code points.
         *   Similar to UTF16.moveCodePointOffset, but without the exceptions, which were
         *   complicating usage.
         * @param s   a Text string
         * @param pos The starting code unit index into the text string
         * @param amt The amount to adjust the string by.
         * @return    The adjusted code unit index, pinned to the string's length, or
         *            unchanged if input index was outside of the string.
         */
        internal static int MoveIndex32(StringBuffer s, int pos, int amt)
        {
            int i;
            char c;
            if (amt > 0)
            {
                for (i = 0; i < amt; i++)
                {
                    if (pos >= s.Length)
                    {
                        return s.Length;
                    }
                    c = s[pos];
                    pos++;
                    if (UTF16.IsLeadSurrogate(c) && pos < s.Length)
                    {
                        c = s[pos];
                        if (UTF16.IsTrailSurrogate(c))
                        {
                            pos++;
                        }
                    }
                }
            }
            else
            {
                for (i = 0; i > amt; i--)
                {
                    if (pos <= 0)
                    {
                        return 0;
                    }
                    pos--;
                    c = s[pos];
                    if (UTF16.IsTrailSurrogate(c) && pos >= 0)
                    {
                        c = s[pos];
                        if (UTF16.IsLeadSurrogate(c))
                        {
                            pos--;
                        }
                    }
                }
            }
            return pos;
        }

        /**
         * No-exceptions form of UnicodeSet.Contains(c).
         *    Simplifies loops that terminate with an end-of-input character value.
         * @param s  A unicode set
         * @param c  A code point value
         * @return   true if the set contains c.
         */
        internal static bool SetContains(UnicodeSet s, int c)
        {
            if (c < 0 || c > UTF16.CODEPOINT_MAX_VALUE)
            {
                return false;
            }
            return s.Contains(c);
        }


        /**
         * return the index of the next code point in the input text.
         * @param i the preceding index
         */
        internal static int NextCP(StringBuffer s, int i)
        {
            if (i == -1)
            {
                // End of Input indication.  Continue to return end value.
                return -1;
            }
            int retVal = i + 1;
            if (retVal > s.Length)
            {
                return -1;
            }
            int c = UTF16.CharAt(s, i);
            if (c >= UTF16.SUPPLEMENTARY_MIN_VALUE && UTF16.IsLeadSurrogate(s[i]))
            {
                retVal++;
            }
            return retVal;
        }


        /**
         * random number generator.  Not using Java's built-in Randoms for two reasons:
         *    1.  Using this code allows obtaining the same sequences as those from the ICU4C monkey test.
         *    2.  We need to get and restore the seed from values occurring in the middle
         *        of a long sequence, to more easily reproduce failing cases.
         */
        private static int m_seed = 1;
        private static int m_rand()
        {
            m_seed = m_seed * 1103515245 + 12345;
            return (m_seed.TripleShift(16)) % 32768;
        }

        // Helper function for formatting error output.
        //   Append a string into a fixed-size field in a StringBuffer.
        //   Blank-pad the string if it is shorter than the field.
        //   Truncate the source string if it is too long.
        //
        private static void appendToBuf(StringBuffer dest, String src, int fieldLen)
        {
            int appendLen = src.Length;
            if (appendLen >= fieldLen)
            {
                dest.Append(src.Substring(0, fieldLen - 0)); // ICU4N: Checked 2nd parameter
            }
            else
            {
                dest.Append(src);
                while (appendLen < fieldLen)
                {
                    dest.Append(' ');
                    appendLen++;
                }
            }
        }

        // Helper function for formatting error output.
        // Display a code point in "\\uxxxx" or "\Uxxxxxxxx" format
        private static void appendCharToBuf(StringBuffer dest, int c, int fieldLen)
        {
            String hexChars = "0123456789abcdef";
            if (c < 0x10000)
            {
                dest.Append("\\u");
                for (int bn = 12; bn >= 0; bn -= 4)
                {
                    dest.Append(hexChars[((c) >> bn) & 0xf]);
                }
                appendToBuf(dest, " ", fieldLen - 6);
            }
            else
            {
                dest.Append("\\U");
                for (int bn = 28; bn >= 0; bn -= 4)
                {
                    dest.Append(hexChars[((c) >> bn) & 0xf]);
                }
                appendToBuf(dest, " ", fieldLen - 10);

            }
        }

        /**
         *  Run a RBBI monkey test.  Common routine, for all break iterator types.
         *    Parameters:
         *       bi      - the break iterator to use
         *       mk      - MonkeyKind, abstraction for obtaining expected results
         *       name    - Name of test (char, word, etc.) for use in error messages
         *       seed    - Seed for starting random number generator (parameter from user)
         *       numIterations
         */
        internal void RunMonkey(BreakIterator bi, RBBIMonkeyKind mk, String name, int seed, int numIterations)
        {
            int TESTSTRINGLEN = 500;
            StringBuffer testText = new StringBuffer();
            int numCharClasses;
            IList<object> chClasses;
            int[] expected = new int[TESTSTRINGLEN * 2 + 1];
            int expectedCount = 0;
            bool[] expectedBreaks = new bool[TESTSTRINGLEN * 2 + 1];
            bool[] forwardBreaks = new bool[TESTSTRINGLEN * 2 + 1];
            bool[] reverseBreaks = new bool[TESTSTRINGLEN * 2 + 1];
            bool[] isBoundaryBreaks = new bool[TESTSTRINGLEN * 2 + 1];
            bool[] followingBreaks = new bool[TESTSTRINGLEN * 2 + 1];
            bool[] precedingBreaks = new bool[TESTSTRINGLEN * 2 + 1];
            int i;
            int loopCount = 0;
            bool printTestData = false;
            bool printBreaksFromBI = false;

            m_seed = seed;

            numCharClasses = mk.CharClasses.Count;
            chClasses = mk.CharClasses;

            // Verify that the character classes all have at least one member.
            for (i = 0; i < numCharClasses; i++)
            {
                UnicodeSet s = (UnicodeSet)chClasses[i];
                if (s == null || s.Count == 0)
                {
                    Errln("Character Class " + i + " is null or of zero size.");
                    return;
                }
            }

            //--------------------------------------------------------------------------------------------
            //
            //  Debugging settings.  Comment out everything in the following block for normal operation
            //
            //--------------------------------------------------------------------------------------------
            // numIterations = -1;
            // numIterations = 10000;   // Same as exhaustive.
            // RuleBasedBreakIterator_New.fTrace = true;
            // m_seed = 859056465;
            // TESTSTRINGLEN = 50;
            // printTestData = true;
            // printBreaksFromBI = true;
            // ((RuleBasedBreakIterator_New)bi).dump();

            //--------------------------------------------------------------------------------------------
            //
            //  End of Debugging settings.
            //
            //--------------------------------------------------------------------------------------------

            int dotsOnLine = 0;
            while (loopCount < numIterations || numIterations == -1)
            {
                if (numIterations == -1 && loopCount % 10 == 0)
                {
                    // If test is running in an infinite loop, display a periodic tic so
                    //   we can tell that it is making progress.
                    Console.Out.Write(".");
                    if (dotsOnLine++ >= 80)
                    {
                        Console.Out.WriteLine();
                        dotsOnLine = 0;
                    }
                }
                // Save current random number seed, so that we can recreate the random numbers
                //   for this loop iteration in event of an error.
                seed = m_seed;

                testText.Length = (0);
                // Populate a test string with data.
                if (printTestData)
                {
                    Console.Out.WriteLine("Test Data string ...");
                }
                for (i = 0; i < TESTSTRINGLEN; i++)
                {
                    int aClassNum = m_rand() % numCharClasses;
                    UnicodeSet classSet = (UnicodeSet)chClasses[aClassNum];
                    int charIdx = m_rand() % classSet.Count;
                    int c = classSet[charIdx];
                    if (c < 0)
                    {   // TODO:  deal with sets containing strings.
                        Errln("c < 0");
                    }
                    UTF16.AppendCodePoint(testText, c);
                    if (printTestData)
                    {
                        Console.Out.Write((c).ToHexString() + " ");
                    }
                }
                if (printTestData)
                {
                    Console.Out.WriteLine();
                }

                Arrays.Fill(expected, 0);
                Arrays.Fill(expectedBreaks, false);
                Arrays.Fill(forwardBreaks, false);
                Arrays.Fill(reverseBreaks, false);
                Arrays.Fill(isBoundaryBreaks, false);
                Arrays.Fill(followingBreaks, false);
                Arrays.Fill(precedingBreaks, false);

                // Calculate the expected results for this test string.
                mk.SetText(testText);
                expectedCount = 0;
                expectedBreaks[0] = true;
                expected[expectedCount++] = 0;
                int breakPos = 0;
                int lastBreakPos = -1;
                for (; ; )
                {
                    lastBreakPos = breakPos;
                    breakPos = mk.Next(breakPos);
                    if (breakPos == -1)
                    {
                        break;
                    }
                    if (breakPos > testText.Length)
                    {
                        Errln("breakPos > testText.Length");
                    }
                    if (lastBreakPos >= breakPos)
                    {
                        Errln("Next() not increasing.");
                        // break;
                    }
                    expectedBreaks[breakPos] = true;
                    expected[expectedCount++] = breakPos;
                }

                // Find the break positions using forward iteration
                if (printBreaksFromBI)
                {
                    Console.Out.WriteLine("Breaks from BI...");
                }
                bi.SetText(testText.ToString());
                for (i = bi.First(); i != BreakIterator.DONE; i = bi.Next())
                {
                    if (i < 0 || i > testText.Length)
                    {
                        Errln(name + " break monkey test: Out of range value returned by breakIterator::next()");
                        break;
                    }
                    if (printBreaksFromBI)
                    {
                        Console.Out.Write((i).ToHexString() + " ");
                    }
                    forwardBreaks[i] = true;
                }
                if (printBreaksFromBI)
                {
                    Console.Out.WriteLine();
                }

                // Find the break positions using reverse iteration
                for (i = bi.Last(); i != BreakIterator.DONE; i = bi.Previous())
                {
                    if (i < 0 || i > testText.Length)
                    {
                        Errln(name + " break monkey test: Out of range value returned by breakIterator.Next()" + name);
                        break;
                    }
                    reverseBreaks[i] = true;
                }

                // Find the break positions using isBoundary() tests.
                for (i = 0; i <= testText.Length; i++)
                {
                    isBoundaryBreaks[i] = bi.IsBoundary(i);
                }

                // Find the break positions using the following() function.
                lastBreakPos = 0;
                followingBreaks[0] = true;
                for (i = 0; i < testText.Length; i++)
                {
                    breakPos = bi.Following(i);
                    if (breakPos <= i ||
                            breakPos < lastBreakPos ||
                            breakPos > testText.Length ||
                            breakPos > lastBreakPos && lastBreakPos > i)
                    {
                        Errln(name + " break monkey test: " +
                                "Out of range value returned by BreakIterator::following().\n" +
                                "index=" + i + "following returned=" + breakPos +
                                "lastBreak=" + lastBreakPos);
                        precedingBreaks[i] = !expectedBreaks[i];   // Forces an error.
                    }
                    else
                    {
                        followingBreaks[breakPos] = true;
                        lastBreakPos = breakPos;
                    }
                }

                // Find the break positions using the preceding() function.
                lastBreakPos = testText.Length;
                precedingBreaks[testText.Length] = true;
                for (i = testText.Length; i > 0; i--)
                {
                    breakPos = bi.Preceding(i);
                    if (breakPos >= i ||
                            breakPos > lastBreakPos ||
                            breakPos < 0 ||
                            breakPos < lastBreakPos && lastBreakPos < i)
                    {
                        Errln(name + " break monkey test: " +
                                "Out of range value returned by BreakIterator::preceding().\n" +
                                "index=" + i + "preceding returned=" + breakPos +
                                "lastBreak=" + lastBreakPos);
                        precedingBreaks[i] = !expectedBreaks[i];   // Forces an error.
                    }
                    else
                    {
                        precedingBreaks[breakPos] = true;
                        lastBreakPos = breakPos;
                    }
                }



                // Compare the expected and actual results.
                for (i = 0; i <= testText.Length; i++)
                {
                    String errorType = null;
                    if (forwardBreaks[i] != expectedBreaks[i])
                    {
                        errorType = "next()";
                    }
                    else if (reverseBreaks[i] != forwardBreaks[i])
                    {
                        errorType = "previous()";
                    }
                    else if (isBoundaryBreaks[i] != expectedBreaks[i])
                    {
                        errorType = "isBoundary()";
                    }
                    else if (followingBreaks[i] != expectedBreaks[i])
                    {
                        errorType = "following()";
                    }
                    else if (precedingBreaks[i] != expectedBreaks[i])
                    {
                        errorType = "preceding()";
                    }

                    if (errorType != null)
                    {
                        // Format a range of the test text that includes the failure as
                        //  a data item that can be included in the rbbi test data file.

                        // Start of the range is the last point where expected and actual results
                        //   both agreed that there was a break position.
                        int startContext = i;
                        int count = 0;
                        for (; ; )
                        {
                            if (startContext == 0) { break; }
                            startContext--;
                            if (expectedBreaks[startContext])
                            {
                                if (count == 2) break;
                                count++;
                            }
                        }

                        // End of range is two expected breaks past the start position.
                        int endContext = i + 1;
                        int ci;
                        for (ci = 0; ci < 2; ci++)
                        {  // Number of items to include in error text.
                            for (; ; )
                            {
                                if (endContext >= testText.Length) { break; }
                                if (expectedBreaks[endContext - 1])
                                {
                                    if (count == 0) break;
                                    count--;
                                }
                                endContext++;
                            }
                        }

                        // Format looks like   "<data><>\uabcd\uabcd<>\U0001abcd...</data>"
                        StringBuffer errorText = new StringBuffer();

                        int c;    // Char from test data
                        for (ci = startContext; ci <= endContext && ci != -1; ci = NextCP(testText, ci))
                        {
                            if (ci == i)
                            {
                                // This is the location of the error.
                                errorText.Append("<?>---------------------------------\n");
                            }
                            else if (expectedBreaks[ci])
                            {
                                // This a non-error expected break position.
                                errorText.Append("------------------------------------\n");
                            }
                            if (ci < testText.Length)
                            {
                                c = UTF16.CharAt(testText, ci);
                                appendCharToBuf(errorText, c, 11);
                                String gc = UCharacter.GetPropertyValueName(UProperty.GENERAL_CATEGORY, UCharacter.GetType(c).ToIcuValue(), NameChoice.Short);
                                appendToBuf(errorText, gc, 8);
                                int extraProp = UCharacter.GetInt32PropertyValue(c, mk.fCharProperty);
                                String extraPropValue =
                                        UCharacter.GetPropertyValueName(mk.fCharProperty, extraProp, NameChoice.Long);
                                appendToBuf(errorText, extraPropValue, 20);

                                String charName = UCharacter.GetExtendedName(c);
                                appendToBuf(errorText, charName, 40);
                                errorText.Append('\n');
                            }
                        }
                        if (ci == testText.Length && ci != -1)
                        {
                            errorText.Append("<>");
                        }
                        errorText.Append("</data>\n");

                        // Output the error
                        Errln(name + " break monkey test error.  " +
                                (expectedBreaks[i] ? "Break expected but not found." : "Break found but not expected.") +
                                "\nOperation = " + errorType + "; random seed = " + seed + ";  buf Idx = " + i + "\n" +
                                errorText);
                        break;
                    }
                }

                loopCount++;
            }
        }

        [Test]
        public void TestCharMonkey()
        {

            int loopCount = 500;
            int seed = 1;

            if (TestFmwk.GetExhaustiveness() >= 9)
            {
                loopCount = 10000;
            }

            RBBICharMonkey m = new RBBICharMonkey();
            BreakIterator bi = BreakIterator.GetCharacterInstance(new CultureInfo("en-US"));
            RunMonkey(bi, m, "char", seed, loopCount);
        }

        [Test]
        public void TestWordMonkey()
        {

            int loopCount = 500;
            int seed = 1;

            if (TestFmwk.GetExhaustiveness() >= 9)
            {
                loopCount = 10000;
            }

            Logln("Word Break Monkey Test");
            RBBIWordMonkey m = new RBBIWordMonkey();
            BreakIterator bi = BreakIterator.GetWordInstance(new CultureInfo("en-US"));
            RunMonkey(bi, m, "word", seed, loopCount);
        }

        [Test]
        public void TestLineMonkey()
        {
            int loopCount = 500;
            int seed = 1;

            if (TestFmwk.GetExhaustiveness() >= 9)
            {
                loopCount = 10000;
            }

            Logln("Line Break Monkey Test");
            RBBILineMonkey m = new RBBILineMonkey();
            BreakIterator bi = BreakIterator.GetLineInstance(new CultureInfo("en-US"));
            RunMonkey(bi, m, "line", seed, loopCount);
        }

        [Test]
        public void TestSentMonkey()
        {

            int loopCount = 500;
            int seed = 1;

            if (TestFmwk.GetExhaustiveness() >= 9)
            {
                loopCount = 3000;
            }

            Logln("Sentence Break Monkey Test");
            RBBISentenceMonkey m = new RBBISentenceMonkey();
            BreakIterator bi = BreakIterator.GetSentenceInstance(new CultureInfo("en-US"));
            RunMonkey(bi, m, "sent", seed, loopCount);
        }
        //
        //  Round-trip monkey tests.
        //  Verify that break iterators created from the rule source from the default
        //    break iterators still pass the monkey test for the iterator type.
        //
        //  This is a major test for the Rule Compiler.  The default break iterators are built
        //  from pre-compiled binary rule data that was created using ICU4C; these
        //  round-trip rule recompile tests verify that the Java rule compiler can
        //  rebuild break iterators from the original source rules.
        //
        [Test]
        public void TestRTCharMonkey()
        {

            int loopCount = 200;
            int seed = 1;

            if (TestFmwk.GetExhaustiveness() >= 9)
            {
                loopCount = 2000;
            }

            RBBICharMonkey m = new RBBICharMonkey();
            BreakIterator bi = BreakIterator.GetCharacterInstance(new CultureInfo("en-US"));
            String rules = bi.ToString();
            BreakIterator rtbi = new RuleBasedBreakIterator(rules);
            RunMonkey(rtbi, m, "char", seed, loopCount);
        }

        [Test]
        public void TestRTWordMonkey()
        {

            int loopCount = 200;
            int seed = 1;

            if (TestFmwk.GetExhaustiveness() >= 9)
            {
                loopCount = 2000;
            }
            Logln("Word Break Monkey Test");
            RBBIWordMonkey m = new RBBIWordMonkey();
            BreakIterator bi = BreakIterator.GetWordInstance(new CultureInfo("en-US"));
            String rules = bi.ToString();
            BreakIterator rtbi = new RuleBasedBreakIterator(rules);
            RunMonkey(rtbi, m, "word", seed, loopCount);
        }

        [Test]
        public void TestRTLineMonkey()
        {
            int loopCount = 200;
            int seed = 1;

            if (TestFmwk.GetExhaustiveness() >= 9)
            {
                loopCount = 2000;
            }

            Logln("Line Break Monkey Test");
            RBBILineMonkey m = new RBBILineMonkey();
            BreakIterator bi = BreakIterator.GetLineInstance(new CultureInfo("en-US"));
            string rules = bi.ToString();
            BreakIterator rtbi = new RuleBasedBreakIterator(rules);
            RunMonkey(rtbi, m, "line", seed, loopCount);
        }

        [Test]
        public void TestRTSentMonkey()
        {

            int loopCount = 200;
            int seed = 1;

            if (TestFmwk.GetExhaustiveness() >= 9)
            {
                loopCount = 1000;
            }

            Logln("Sentence Break Monkey Test");
            RBBISentenceMonkey m = new RBBISentenceMonkey();
            BreakIterator bi = BreakIterator.GetSentenceInstance(new CultureInfo("en-US"));
            String rules = bi.ToString();
            BreakIterator rtbi = new RuleBasedBreakIterator(rules);
            RunMonkey(rtbi, m, "sent", seed, loopCount);
        }
    }
}
