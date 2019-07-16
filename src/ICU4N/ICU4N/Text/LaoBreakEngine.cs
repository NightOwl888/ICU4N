using ICU4N.Lang;
using ICU4N.Support.Text;
using System;

namespace ICU4N.Text
{
    internal class LaoBreakEngine : DictionaryBreakEngine
    {
        // Constants for LaoBreakIterator
        // How many words in a row are "good enough"?
        private static readonly byte LAO_LOOKAHEAD = 3;
        // Will not combine a non-word with a preceding dictionary word longer than this
        private static readonly byte LAO_ROOT_COMBINE_THRESHOLD = 3;
        // Will not combine a non-word that shares at least this much prefix with a
        // dictionary word with a preceding word
        private static readonly byte LAO_PREFIX_COMBINE_THRESHOLD = 3;
        // Minimum word size
        private static readonly byte LAO_MIN_WORD = 2;

        private DictionaryMatcher fDictionary;
        private static UnicodeSet fLaoWordSet;
        private static UnicodeSet fEndWordSet;
        private static UnicodeSet fBeginWordSet;
        private static UnicodeSet fMarkSet;

        static LaoBreakEngine()
        {
            // Initialize UnicodeSets
            fLaoWordSet = new UnicodeSet();
            fMarkSet = new UnicodeSet();
            fBeginWordSet = new UnicodeSet();

            fLaoWordSet.ApplyPattern("[[:Laoo:]&[:LineBreak=SA:]]");
            fLaoWordSet.Compact();

            fMarkSet.ApplyPattern("[[:Laoo:]&[:LineBreak=SA:]&[:M:]]");
            fMarkSet.Add(0x0020);
            fEndWordSet = new UnicodeSet(fLaoWordSet);
            fEndWordSet.Remove(0x0EC0, 0x0EC4); // prefix vowels
            fBeginWordSet.Add(0x0E81, 0x0EAE); // basic consonants (including holes for corresponding Thai characters)
            fBeginWordSet.Add(0x0EDC, 0x0EDD); // digraph consonants (no Thai equivalent)
            fBeginWordSet.Add(0x0EC0, 0x0EC4); // prefix vowels

            // Compact for caching
            fMarkSet.Compact();
            fEndWordSet.Compact();
            fBeginWordSet.Compact();

            // Freeze the static UnicodeSet
            fLaoWordSet.Freeze();
            fMarkSet.Freeze();
            fEndWordSet.Freeze();
            fBeginWordSet.Freeze();
        }

        public LaoBreakEngine()
            : base(BreakIterator.KIND_WORD, BreakIterator.KIND_LINE)
        {
            SetCharacters(fLaoWordSet);
            // Initialize dictionary
            fDictionary = DictionaryData.LoadDictionaryFor("Laoo");
        }

        public override bool Equals(Object obj)
        {
            // Normally is a singleton, but it's possible to have duplicates
            //   during initialization. All are equivalent.
            return obj is LaoBreakEngine;
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
                return (script == UScript.Lao);
            }
            return false;
        }

        public override int DivideUpDictionaryRange(CharacterIterator fIter, int rangeStart, int rangeEnd,
                DequeI foundBreaks)
        {


            if ((rangeEnd - rangeStart) < LAO_MIN_WORD)
            {
                return 0;  // Not enough characters for word
            }
            int wordsFound = 0;
            int wordLength;
            int current;
            PossibleWord[] words = new PossibleWord[LAO_LOOKAHEAD];
            for (int i = 0; i < LAO_LOOKAHEAD; i++)
            {
                words[i] = new PossibleWord();
            }
            int uc;

            fIter.SetIndex(rangeStart);
            while ((current = fIter.Index) < rangeEnd)
            {
                wordLength = 0;

                //Look for candidate words at the current position
                int candidates = words[wordsFound % LAO_LOOKAHEAD].Candidates(fIter, fDictionary, rangeEnd);

                // If we found exactly one, use that
                if (candidates == 1)
                {
                    wordLength = words[wordsFound % LAO_LOOKAHEAD].AcceptMarked(fIter);
                    wordsFound += 1;
                }

                // If there was more than one, see which one can take us forward the most words
                else if (candidates > 1)
                {
                    bool foundBest = false;
                    // If we're already at the end of the range, we're done
                    if (fIter.Index < rangeEnd)
                    {
                        do
                        {
                            int wordsMatched = 1;
                            if (words[(wordsFound + 1) % LAO_LOOKAHEAD].Candidates(fIter, fDictionary, rangeEnd) > 0)
                            {
                                if (wordsMatched < 2)
                                {
                                    // Followed by another dictionary word; mark first word as a good candidate
                                    words[wordsFound % LAO_LOOKAHEAD].MarkCurrent();
                                    wordsMatched = 2;
                                }

                                // If we're already at the end of the range, we're done
                                if (fIter.Index >= rangeEnd)
                                {
                                    break;
                                }

                                // See if any of the possible second words is followed by a third word
                                do
                                {
                                    // If we find a third word, stop right away
                                    if (words[(wordsFound + 2) % LAO_LOOKAHEAD].Candidates(fIter, fDictionary, rangeEnd) > 0)
                                    {
                                        words[wordsFound % LAO_LOOKAHEAD].MarkCurrent();
                                        foundBest = true;
                                        break;
                                    }
                                } while (words[(wordsFound + 1) % LAO_LOOKAHEAD].BackUp(fIter));
                            }
                        } while (words[wordsFound % LAO_LOOKAHEAD].BackUp(fIter) && !foundBest);
                    }
                    wordLength = words[wordsFound % LAO_LOOKAHEAD].AcceptMarked(fIter);
                    wordsFound += 1;
                }

                // We come here after having either found a word or not. We look ahead to the
                // next word. If it's not a dictionary word, we will combine it with the word we
                // just found (if there is one), but only if the preceding word does not exceed
                // the threshold.
                // The text iterator should now be positioned at the end of the word we found.
                if (fIter.Index < rangeEnd && wordLength < LAO_ROOT_COMBINE_THRESHOLD)
                {
                    // If it is a dictionary word, do nothing. If it isn't, then if there is
                    // no preceding word, or the non-word shares less than the minimum threshold
                    // of characters with a dictionary word, then scan to resynchronize
                    if (words[wordsFound % LAO_LOOKAHEAD].Candidates(fIter, fDictionary, rangeEnd) <= 0 &&
                            (wordLength == 0 ||
                                    words[wordsFound % LAO_LOOKAHEAD].LongestPrefix < LAO_PREFIX_COMBINE_THRESHOLD))
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
                                int candidate = words[(wordsFound + 1) % LAO_LOOKAHEAD].Candidates(fIter, fDictionary, rangeEnd);
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
                // NOT CURRENTLY APPLICABLE TO LAO

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
