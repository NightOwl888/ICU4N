﻿using ICU4N.Impl;
using ICU4N.Support.Text;
using J2N;
using J2N.Text;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Text;
#nullable enable

namespace ICU4N.Text
{
    internal class CjkBreakEngine : DictionaryBreakEngine
    {
        private const int CharStackBufferSize = 64;
        private const int Int32StackBufferSize = 64;

        // ICU4N: Avoid static constructor by initializing inline
        private static readonly UnicodeSet fHangulWordSet = new UnicodeSet("[\\uac00-\\ud7a3]").Freeze();
        private static readonly UnicodeSet fHanWordSet = new UnicodeSet("[:Han:]").Freeze();
        private static readonly UnicodeSet fKatakanaWordSet = new UnicodeSet("[[:Katakana:]\\uff9e\\uff9f]").Freeze();
        private static readonly UnicodeSet fHiraganaWordSet = new UnicodeSet("[:Hiragana:]").Freeze();
        private readonly DictionaryMatcher fDictionary;

        public CjkBreakEngine(bool korean)
            : base(BreakIterator.KIND_WORD)
        {
            fDictionary = DictionaryData.LoadDictionaryFor("Hira");
            if (korean)
            {
                SetCharacters(fHangulWordSet);
            }
            else
            { //Chinese and Japanese
                UnicodeSet cjSet = new UnicodeSet();
                cjSet.AddAll(fHanWordSet);
                cjSet.AddAll(fKatakanaWordSet);
                cjSet.AddAll(fHiraganaWordSet);
                cjSet.Add(0xFF70); // HALFWIDTH KATAKANA-HIRAGANA PROLONGED SOUND MARK
                cjSet.Add(0x30FC); // KATAKANA-HIRAGANA PROLONGED SOUND MARK
                SetCharacters(cjSet);
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is CjkBreakEngine other)
            {
                return this.fSet.Equals(other.fSet);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        private const int kMaxKatakanaLength = 8;
        private const int kMaxKatakanaGroupLength = 20;
        private const int maxSnlp = 255;
        private const int kint32max = int.MaxValue;
        private static int GetKatakanaCost(int wordlength)
        {
            Span<int> katakanaCost = stackalloc int[] { 8192, 984, 408, 240, 204, 252, 300, 372, 480 };
            return (wordlength > kMaxKatakanaLength) ? 8192 : katakanaCost[wordlength];
        }

        private static bool IsKatakana(int value)
        {
            return (value >= 0x30A1 && value <= 0x30FE && value != 0x30FB) ||
                    (value >= 0xFF66 && value <= 0xFF9F);
        }

        public override int DivideUpDictionaryRange(CharacterIterator inText, int startPos, int endPos,
                DequeI foundBreaks)
        {
            if (startPos >= endPos)
            {
                return 0;
            }

            inText.SetIndex(startPos);

            int inputLength = endPos - startPos;
            int[] charPositions = new int[inputLength + 1];
            StringBuilder s = new StringBuilder(inputLength  + 1);
            inText.SetIndex(startPos);
            while (inText.Index < endPos)
            {
                s.Append(inText.Current);
                inText.Next();
            }
            string prenormstr = s.ToString();
#pragma warning disable 612, 618
            bool isNormalized = Normalizer.QuickCheck(prenormstr, NormalizerMode.NFKC) == QuickCheckResult.Yes ||
                               Normalizer.IsNormalized(prenormstr, NormalizerMode.NFKC, 0);
#pragma warning restore 612, 618
            CharacterIterator text;
            int numChars = 0;
            if (isNormalized)
            {
                text = new StringCharacterIterator(prenormstr);
                int index = 0;
                charPositions[0] = 0;
                while (index < prenormstr.Length)
                {
                    int codepoint = prenormstr.CodePointAt(index);
                    index += Character.CharCount(codepoint);
                    numChars++;
                    charPositions[numChars] = index;
                }
            }
            else
            {
#pragma warning disable 612, 618
                string normStr = Normalizer.Normalize(prenormstr, NormalizerMode.NFKC);
                text = new StringCharacterIterator(normStr);
                charPositions = new int[normStr.Length + 1];
                Normalizer normalizer = new Normalizer(prenormstr, NormalizerMode.NFKC, 0);
                int index = 0;
                charPositions[0] = 0;
                while (index < normalizer.EndIndex)
                {
                    normalizer.Next();
                    numChars++;
                    index = normalizer.Index;
                    charPositions[numChars] = index;
                }
#pragma warning restore 612, 618
            }

            // From here on out, do the algorithm. Note that our indices
            // refer to indices within the normalized string.
            int[] bestSnlp = new int[numChars + 1];
            bestSnlp[0] = 0;
            for (int i = 1; i <= numChars; i++)
            {
                bestSnlp[i] = kint32max;
            }

            int[] prev = new int[numChars + 1];
            for (int i = 0; i <= numChars; i++)
            {
                prev[i] = -1;
            }

            int maxWordSize = 20;
            int[] values = new int[numChars];
            int[] lengths = new int[numChars];
            // dynamic programming to find the best segmentation
            bool is_prev_katakana = false;
            for (int i = 0; i < numChars; i++)
            {
                text.SetIndex(i);
                if (bestSnlp[i] == kint32max)
                {
                    continue;
                }

                int maxSearchLength = (i + maxWordSize < numChars) ? maxWordSize : (numChars - i);
                fDictionary.Matches(text, maxSearchLength, lengths, out int count, maxSearchLength, values);

                // if there are no single character matches found in the dictionary
                // starting with this character, treat character as a 1-character word
                // with the highest value possible (i.e. the least likely to occur).
                // Exclude Korean characters from this treatment, as they should be
                // left together by default.
                text.SetIndex(i);  // fDictionary.matches() advances the text position; undo that.
                if ((count == 0 || lengths[0] != 1) && CharacterIteration.Current32(text) != CharacterIteration.Done32 && !fHangulWordSet.Contains(CharacterIteration.Current32(text)))
                {
                    values[count] = maxSnlp;
                    lengths[count] = 1;
                    count++;
                }

                for (int j = 0; j < count; j++)
                {
                    int newSnlp = bestSnlp[i] + values[j];
                    if (newSnlp < bestSnlp[lengths[j] + i])
                    {
                        bestSnlp[lengths[j] + i] = newSnlp;
                        prev[lengths[j] + i] = i;
                    }
                }

                // In Japanese, single-character Katakana words are pretty rare.
                // So we apply the following heuristic to Katakana: any continuous
                // run of Katakana characters is considered a candidate word with
                // a default cost specified in the katakanaCost table according
                // to its length.
                bool is_katakana = IsKatakana(CharacterIteration.Current32(text));
                if (!is_prev_katakana && is_katakana)
                {
                    int j = i + 1;
                    CharacterIteration.Next32(text);
                    while (j < numChars && (j - i) < kMaxKatakanaGroupLength && IsKatakana(CharacterIteration.Current32(text)))
                    {
                        CharacterIteration.Next32(text);
                        ++j;
                    }

                    if ((j - i) < kMaxKatakanaGroupLength)
                    {
                        int newSnlp = bestSnlp[i] + GetKatakanaCost(j - i);
                        if (newSnlp < bestSnlp[j])
                        {
                            bestSnlp[j] = newSnlp;
                            prev[j] = i;
                        }
                    }
                }
                is_prev_katakana = is_katakana;
            }

            int[] t_boundary = new int[numChars + 1];
            int numBreaks = 0;
            if (bestSnlp[numChars] == kint32max)
            {
                t_boundary[numBreaks] = numChars;
                numBreaks++;
            }
            else
            {
                for (int i = numChars; i > 0; i = prev[i])
                {
                    t_boundary[numBreaks] = i;
                    numBreaks++;
                }
                Assert.Assrt(prev[t_boundary[numBreaks - 1]] == 0);
            }

            if (foundBreaks.Count == 0 || foundBreaks.Peek() < startPos)
            {
                t_boundary[numBreaks++] = 0;
            }

            int correctedNumBreaks = 0;
            for (int i = numBreaks - 1; i >= 0; i--)
            {
                int pos = charPositions[t_boundary[i]] + startPos;
                if (!(foundBreaks.Contains(pos) || pos == startPos))
                {
                    foundBreaks.Push(charPositions[t_boundary[i]] + startPos);
                    correctedNumBreaks++;
                }
            }

            if (!foundBreaks.IsEmpty && foundBreaks.Peek() == endPos)
            {
                foundBreaks.Pop();
                correctedNumBreaks--;
            }
            if (!foundBreaks.IsEmpty)
                inText.SetIndex(foundBreaks.Peek());
            return correctedNumBreaks;
        }
    }
}
