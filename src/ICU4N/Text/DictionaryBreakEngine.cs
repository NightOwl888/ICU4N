using ICU4N.Impl;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using J2N.Collections;
using System.Diagnostics;

namespace ICU4N.Text
{
    internal abstract class DictionaryBreakEngine : ILanguageBreakEngine
    {
        /// <summary>
        /// Helper class for improving readability of the Thai/Lao/Khmer word break
        /// algorithm.
        /// </summary>
        internal class PossibleWord
        {
            // List size, limited by the maximum number of words in the dictionary
            // that form a nested sequence.
            private const int POSSIBLE_WORD_LIST_MAX = 20;
            //list of word candidate lengths, in increasing length order
            private readonly int[] lengths;
            private int count;      // Count of candidates // ICU4N: Converted from int[] to int since it is an out parameter. We still need it declared here, though.
            private int prefix;     // The longest match with a dictionary word
            private int offset;     // Offset in the text of these candidates
            private int mark;       // The preferred candidate's offset
            private int current;    // The candidate we're currently looking at

            // Default constructor
            public PossibleWord()
            {
                lengths = new int[POSSIBLE_WORD_LIST_MAX];
                offset = -1;
            }

            // Fill the list of candidates if needed, select the longest, and return the number found
            public virtual int Candidates(CharacterIterator fIter, DictionaryMatcher dict, int rangeEnd)
            {
                int start = fIter.Index;
                if (start != offset)
                {
                    offset = start;
                    prefix = dict.Matches(fIter, rangeEnd - start, lengths, out count, lengths.Length);
                    // Dictionary leaves text after longest prefix, not longest word. Back up.
                    if (count <= 0)
                    {
                        fIter.SetIndex(start);
                    }
                }
                if (count > 0)
                {
                    fIter.SetIndex(start + lengths[count - 1]);
                }
                current = count - 1;
                mark = current;
                return count;
            }

            // Select the currently marked candidate, point after it in the text, and invalidate self
            public virtual int AcceptMarked(CharacterIterator fIter)
            {
                fIter.SetIndex(offset + lengths[mark]);
                return lengths[mark];
            }

            // Backup from the current candidate to the next shorter one; return true if that exists
            // and point the text after it
            public virtual bool BackUp(CharacterIterator fIter)
            {
                if (current > 0)
                {
                    fIter.SetIndex(offset + lengths[--current]);
                    return true;
                }
                return false;
            }

            // Return the longest prefix this candidate location shares with a dictionary word
            public virtual int LongestPrefix => prefix;

            // Mark the current candidate as the one we like
            public virtual void MarkCurrent()
            {
                mark = current;
            }
        }

        /// <summary>
        /// A deque-like structure holding raw <see cref="int"/>s.
        /// Partial, limited implementation, only what is needed by the dictionary implementation.
        /// For internal use only.
        /// </summary>
        /// <internal/>
        internal class DequeI
#if FEATURE_CLONEABLE
            : ICloneable
#endif
        {
            private int[] data = new int[50];
            private int lastIdx = 4;   // or base of stack. Index of element.
            private int firstIdx = 4;  // or Top of Stack. Index of element + 1.

            public virtual object Clone()
            {
                DequeI result = (DequeI)base.MemberwiseClone();
                data = (int[])data.Clone();
                return result;
            }

            internal virtual int Count => firstIdx - lastIdx;

            internal virtual bool IsEmpty => Count == 0;

            private void Grow()
            {
                int[] newData = new int[data.Length * 2];
                System.Array.Copy(data, 0, newData, 0, data.Length);
                data = newData;
            }

            internal void Offer(int v)
            {
                // Note that the actual use cases of offer() add at most one element.
                //   We make no attempt to handle more than a few.
                Debug.Assert(lastIdx > 0);
                data[--lastIdx] = v;
            }

            internal virtual void Push(int v)
            {
                if (firstIdx >= data.Length)
                {
                    Grow();
                }
                data[firstIdx++] = v;
            }

            internal virtual int Pop()
            {
                Debug.Assert(Count > 0);
                return data[--firstIdx];
            }

            internal virtual int Peek()
            {
                Debug.Assert(Count > 0);
                return data[firstIdx - 1];
            }

            internal virtual int PeekLast()
            {
                Debug.Assert(Count > 0);
                return data[lastIdx];
            }

            internal virtual int PollLast()
            {
                Debug.Assert(Count > 0);
                return data[lastIdx++];
            }

            internal virtual bool Contains(int v)
            {
                for (int i = lastIdx; i < firstIdx; i++)
                {
                    if (data[i] == v)
                    {
                        return true;
                    }
                }
                return false;
            }

            internal virtual int ElementAt(int i)
            {
                Debug.Assert(i < Count);
                return data[lastIdx + i];
            }

            internal virtual void RemoveAllElements()
            {
                lastIdx = firstIdx = 4;
            }
        }

        internal UnicodeSet fSet = new UnicodeSet();
        private readonly BitSet fTypes = new BitSet(32);

        /// <param name="breakTypes">The types of break iterators that can use this engine.
        /// For example, <see cref="BreakIterator.KIND_LINE"/>.</param>
        public DictionaryBreakEngine(params int[] breakTypes)
        {
            foreach (int type in breakTypes)
            {
                fTypes.Set(type);
            }
        }

        public virtual bool Handles(int c, int breakType)
        {
            return fTypes.Get(breakType) &&  // this type can use us
                    fSet.Contains(c);        // we recognize the character
        }

        public virtual int FindBreaks(CharacterIterator text, int startPos, int endPos,
                int breakType, DequeI foundBreaks)
        {
            int result = 0;

            // Find the span of characters included in the set.
            //   The span to break begins at the current position int the text, and
            //   extends towards the start or end of the text, depending on 'reverse'.

            int start = text.Index;
            int current;
            int rangeStart;
            int rangeEnd;
            int c = CharacterIteration.Current32(text);
            while ((current = text.Index) < endPos && fSet.Contains(c))
            {
                CharacterIteration.Next32(text);
                c = CharacterIteration.Current32(text);
            }
            rangeStart = start;
            rangeEnd = current;

            // if (breakType >= 0 && breakType < 32 && (((uint32_t)1 << breakType) & fTypes)) {
            // TODO: Why does icu4c have this?
            result = DivideUpDictionaryRange(text, rangeStart, rangeEnd, foundBreaks);
            text.SetIndex(current);

            return result;
        }

        internal virtual void SetCharacters(UnicodeSet set)
        {
            fSet = new UnicodeSet(set);
            fSet.Compact();
        }

        /// <summary>
        /// Divide up a range of known dictionary characters handled by this break engine.
        /// </summary>
        /// <param name="text">A <see cref="CharacterIterator"/> representing the text.</param>
        /// <param name="rangeStart">The start of the range of dictionary characters.</param>
        /// <param name="rangeEnd">The end of the range of dictionary characters.</param>
        /// <param name="foundBreaks">Output of break positions. Positions are pushed.
        /// Pre-existing contents of the output stack are unaltered.</param>
        /// <returns>The number of breaks found.</returns>
        public abstract int DivideUpDictionaryRange(CharacterIterator text,
                                             int rangeStart,
                                             int rangeEnd,
                                             DequeI foundBreaks);
    }
}
