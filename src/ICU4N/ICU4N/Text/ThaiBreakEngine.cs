using ICU4N.Lang;
using ICU4N.Support.Text;

namespace ICU4N.Text
{
    internal class ThaiBreakEngine : DictionaryBreakEngine
    {
        // Constants for ThaiBreakIterator
        // How many words in a row are "good enough"?
        private static readonly byte THAI_LOOKAHEAD = 3;
        // Will not combine a non-word with a preceding dictionary word longer than this
        private static readonly byte THAI_ROOT_COMBINE_THRESHOLD = 3;
        // Will not combine a non-word that shares at least this much prefix with a
        // dictionary word with a preceding word
        private static readonly byte THAI_PREFIX_COMBINE_THRESHOLD = 3;
        // Ellision character
        private static readonly char THAI_PAIYANNOI = (char)0x0E2F;
        // Repeat character
        private static readonly char THAI_MAIYAMOK = (char)0x0E46;
        // Minimum word size
        private static readonly byte THAI_MIN_WORD = 2;
        // Minimum number of characters for two words
        private static readonly byte THAI_MIN_WORD_SPAN = (byte)(THAI_MIN_WORD * 2);

        private DictionaryMatcher fDictionary;
        private static UnicodeSet fThaiWordSet;
        private static UnicodeSet fEndWordSet;
        private static UnicodeSet fBeginWordSet;
        private static UnicodeSet fSuffixSet;
        private static UnicodeSet fMarkSet;

        static ThaiBreakEngine()
        {
            // Initialize UnicodeSets
            fThaiWordSet = new UnicodeSet();
            fMarkSet = new UnicodeSet();
            fBeginWordSet = new UnicodeSet();
            fSuffixSet = new UnicodeSet();

            fThaiWordSet.ApplyPattern("[[:Thai:]&[:LineBreak=SA:]]");
            fThaiWordSet.Compact();

            fMarkSet.ApplyPattern("[[:Thai:]&[:LineBreak=SA:]&[:M:]]");
            fMarkSet.Add(0x0020);
            fEndWordSet = new UnicodeSet(fThaiWordSet);
            fEndWordSet.Remove(0x0E31); // MAI HAN-AKAT
            fEndWordSet.Remove(0x0E40, 0x0E44); // SARA E through SARA AI MAIMALAI
            fBeginWordSet.Add(0x0E01, 0x0E2E); //KO KAI through HO NOKHUK
            fBeginWordSet.Add(0x0E40, 0x0E44); // SARA E through SARA AI MAIMALAI
            fSuffixSet.Add(THAI_PAIYANNOI);
            fSuffixSet.Add(THAI_MAIYAMOK);

            // Compact for caching
            fMarkSet.Compact();
            fEndWordSet.Compact();
            fBeginWordSet.Compact();
            fSuffixSet.Compact();

            // Freeze the static UnicodeSet
            fThaiWordSet.Freeze();
            fMarkSet.Freeze();
            fEndWordSet.Freeze();
            fBeginWordSet.Freeze();
            fSuffixSet.Freeze();
        }

        public ThaiBreakEngine()
            : base(BreakIterator.KIND_WORD, BreakIterator.KIND_LINE)
        {
            SetCharacters(fThaiWordSet);
            // Initialize dictionary
            fDictionary = DictionaryData.LoadDictionaryFor("Thai");
        }

        public override bool Equals(object obj)
        {
            // Normally is a singleton, but it's possible to have duplicates
            //   during initialization. All are equivalent.
            return obj is ThaiBreakEngine;
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        public override bool Handles(int c, int breakType)
        {
            if (breakType == BreakIterator.KIND_WORD || breakType == BreakIterator.KIND_LINE)
            {
                int script = UCharacter.GetInt32PropertyValue(c, UProperty.Script);
                return (script == UScript.Thai);
            }
            return false;
        }

        public override int DivideUpDictionaryRange(CharacterIterator fIter, int rangeStart, int rangeEnd,
                DequeI foundBreaks)
        {

            if ((rangeEnd - rangeStart) < THAI_MIN_WORD_SPAN)
            {
                return 0;  // Not enough characters for word
            }
            int wordsFound = 0;
            int wordLength;
            PossibleWord[] words = new PossibleWord[THAI_LOOKAHEAD];
            for (int i = 0; i < THAI_LOOKAHEAD; i++)
            {
                words[i] = new PossibleWord();
            }

            int uc;
            fIter.SetIndex(rangeStart);
            int current;
            while ((current = fIter.Index) < rangeEnd)
            {
                wordLength = 0;

                //Look for candidate words at the current position
                int candidates = words[wordsFound % THAI_LOOKAHEAD].Candidates(fIter, fDictionary, rangeEnd);

                // If we found exactly one, use that
                if (candidates == 1)
                {
                    wordLength = words[wordsFound % THAI_LOOKAHEAD].AcceptMarked(fIter);
                    wordsFound += 1;
                }

                // If there was more than one, see which one can take us forward the most words
                else if (candidates > 1)
                {
                    // If we're already at the end of the range, we're done
                    if (fIter.Index < rangeEnd)
                    {
                        //foundBest:
                        do
                        {
                            int wordsMatched = 1;
                            if (words[(wordsFound + 1) % THAI_LOOKAHEAD].Candidates(fIter, fDictionary, rangeEnd) > 0)
                            {
                                if (wordsMatched < 2)
                                {
                                    // Followed by another dictionary word; mark first word as a good candidate
                                    words[wordsFound % THAI_LOOKAHEAD].MarkCurrent();
                                    wordsMatched = 2;
                                }

                                // If we're already at the end of the range, we're done
                                if (fIter.Index >= rangeEnd)
                                {
                                    goto foundBest_break;
                                }

                                // See if any of the possible second words is followed by a third word
                                do
                                {
                                    // If we find a third word, stop right away
                                    if (words[(wordsFound + 2) % THAI_LOOKAHEAD].Candidates(fIter, fDictionary, rangeEnd) > 0)
                                    {
                                        words[wordsFound % THAI_LOOKAHEAD].MarkCurrent();
                                        goto foundBest_break;
                                    }
                                } while (words[(wordsFound + 1) % THAI_LOOKAHEAD].BackUp(fIter));
                            }
                        }
                        while (words[wordsFound % THAI_LOOKAHEAD].BackUp(fIter));
                        // foundBest: end of loop
                        foundBest_break: { }
                    }
                    wordLength = words[wordsFound % THAI_LOOKAHEAD].AcceptMarked(fIter);
                    wordsFound += 1;
                }

                // We come here after having either found a word or not. We look ahead to the
                // next word. If it's not a dictionary word, we will combine it with the word we
                // just found (if there is one), but only if the preceding word does not exceed
                // the threshold.
                // The text iterator should now be positioned at the end of the word we found.
                if (fIter.Index < rangeEnd && wordLength < THAI_ROOT_COMBINE_THRESHOLD)
                {
                    // If it is a dictionary word, do nothing. If it isn't, then if there is
                    // no preceding word, or the non-word shares less than the minimum threshold
                    // of characters with a dictionary word, then scan to resynchronize
                    if (words[wordsFound % THAI_LOOKAHEAD].Candidates(fIter, fDictionary, rangeEnd) <= 0 &&
                            (wordLength == 0 ||
                                    words[wordsFound % THAI_LOOKAHEAD].LongestPrefix < THAI_PREFIX_COMBINE_THRESHOLD))
                    {
                        // Look for a plausible word boundary
                        int remaining = rangeEnd - (current + wordLength);
                        int pc = fIter.Current;
                        int chars = 0;
                        for (; ; )
                        {
                            fIter.MoveNext();
                            uc = fIter.Current;
                            chars += 1;
                            if (--remaining <= 0)
                            {
                                break;
                            }
                            if (fEndWordSet.Contains(pc) && fBeginWordSet.Contains(uc))
                            {
                                // Maybe. See if it's in the dictionary.
                                // Note: In the original Apple code, checked that the next
                                // two characters after uc were not 0x0E4C THANTHAKHAT before
                                // checking the dictionary. That is just a performance filter,
                                // but it's not clear it's faster than checking the trie
                                int candidate = words[(wordsFound + 1) % THAI_LOOKAHEAD].Candidates(fIter, fDictionary, rangeEnd);
                                fIter.SetIndex(current + wordLength + chars);
                                if (candidate > 0)
                                {
                                    break;
                                }
                            }
                            pc = uc;
                        }

                        // Bump the word count if there wasn't already one
                        if (wordLength <= 0)
                        {
                            wordsFound += 1;
                        }

                        // Update the length with the passed-over characters
                        wordLength += chars;
                    }
                    else
                    {
                        // Backup to where we were for next iteration
                        fIter.SetIndex(current + wordLength);
                    }
                }

                // Never stop before a combining mark.
                int currPos;
                while ((currPos = fIter.Index) < rangeEnd && fMarkSet.Contains(fIter.Current))
                {
                    fIter.MoveNext();
                    wordLength += fIter.Index - currPos;
                }

                // Look ahead for possible suffixes if a dictionary word does not follow.
                // We do this in code rather than using a rule so that the heuristic
                // resynch continues to function. For example, one of the suffix characters 
                // could be a typo in the middle of a word.
                if (fIter.Index < rangeEnd && wordLength > 0)
                {
                    if (words[wordsFound % THAI_LOOKAHEAD].Candidates(fIter, fDictionary, rangeEnd) <= 0 &&
                            fSuffixSet.Contains(uc = fIter.Current))
                    {
                        if (uc == THAI_PAIYANNOI)
                        {
                            if (!fSuffixSet.Contains(fIter.MovePrevious()))
                            {
                                // Skip over previous end and PAIYANNOI
                                fIter.MoveNext();
                                fIter.MoveNext();
                                wordLength += 1;
                                uc = fIter.Current;
                            }
                            else
                            {
                                // Restore prior position
                                fIter.MoveNext();
                            }
                        }
                        if (uc == THAI_MAIYAMOK)
                        {
                            if (fIter.MovePrevious() != THAI_MAIYAMOK)
                            {
                                // Skip over previous end and MAIYAMOK
                                fIter.MoveNext();
                                fIter.MoveNext();
                                wordLength += 1;
                            }
                            else
                            {
                                // restore prior position
                                fIter.MoveNext();
                            }
                        }
                    }
                    else
                    {
                        fIter.SetIndex(current + wordLength);
                    }
                }

                // Did we find a word on this iteration? If so, push it on the break stack
                if (wordLength > 0)
                {
                    foundBreaks.Push(current + wordLength);
                }
            }

            // Don't return a break for the end of the dictionary range if there is one there
            if (foundBreaks.Peek() >= rangeEnd)
            {
                foundBreaks.Pop();
                wordsFound -= 1;
            }

            return wordsFound;
        }
    }
}
