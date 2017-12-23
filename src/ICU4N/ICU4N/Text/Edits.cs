using ICU4N.Support.Collections;
using ICU4N.Support.IO;
using System;
using System.Diagnostics;

namespace ICU4N.Text
{
    /// <summary>
    /// Records lengths of string edits but not replacement text.
    /// Supports replacements, insertions, deletions in linear progression.
    /// Does not support moving/reordering of text.
    /// </summary>
    /// <draft>ICU 59</draft>
    /// <provisional>This API might change or be removed in a future release.</provisional>
    public sealed class Edits
    {
        // 0000uuuuuuuuuuuu records u+1 unchanged text units.
        private static readonly int MAX_UNCHANGED_LENGTH = 0x1000;
        private static readonly int MAX_UNCHANGED = MAX_UNCHANGED_LENGTH - 1;

        // 0mmmnnnccccccccc with m=1..6 records ccc+1 replacements of m:n text units.
        private static readonly int MAX_SHORT_CHANGE_OLD_LENGTH = 6;
        private static readonly int MAX_SHORT_CHANGE_NEW_LENGTH = 7;
        private static readonly int SHORT_CHANGE_NUM_MASK = 0x1ff;
        private static readonly int MAX_SHORT_CHANGE = 0x6fff;

        // 0111mmmmmmnnnnnn records a replacement of m text units with n.
        // m or n = 61: actual length follows in the next edits array unit.
        // m or n = 62..63: actual length follows in the next two edits array units.
        // Bit 30 of the actual length is in the head unit.
        // Trailing units have bit 15 set.
        private static readonly int LENGTH_IN_1TRAIL = 61;
        private static readonly int LENGTH_IN_2TRAIL = 62;

        private static readonly int STACK_CAPACITY = 100;
        private char[] array;
        private int length;
        private int delta;
        private int numChanges;

        /// <summary>
        /// Constructs an empty object.
        /// </summary>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public Edits()
        {
            array = new char[STACK_CAPACITY];
        }

        /// <summary>
        /// Resets the data but may not release memory.
        /// </summary>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public void Reset()
        {
            length = delta = numChanges = 0;
        }

        private int LastUnit
        {
            get { return length > 0 ? array[length - 1] : 0xffff; }
            set { array[length - 1] = (char)value; }
        }

        /// <summary>
        /// Adds a record for an unchanged segment of text.
        /// Normally called from inside ICU string transformation functions, not user code.
        /// </summary>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public void AddUnchanged(int unchangedLength)
        {
            if (unchangedLength < 0)
            {
                throw new ArgumentException(
                        "addUnchanged(" + unchangedLength + "): length must not be negative");
            }
            // Merge into previous unchanged-text record, if any.
            int last = LastUnit;
            if (last < MAX_UNCHANGED)
            {
                int remaining = MAX_UNCHANGED - last;
                if (remaining >= unchangedLength)
                {
                    LastUnit = last + unchangedLength;
                    return;
                }
                LastUnit = MAX_UNCHANGED;
                unchangedLength -= remaining;
            }
            // Split large lengths into multiple units.
            while (unchangedLength >= MAX_UNCHANGED_LENGTH)
            {
                Append(MAX_UNCHANGED);
                unchangedLength -= MAX_UNCHANGED_LENGTH;
            }
            // Write a small (remaining) length.
            if (unchangedLength > 0)
            {
                Append(unchangedLength - 1);
            }
        }

        /// <summary>
        /// Adds a record for a text replacement/insertion/deletion.
        /// Normally called from inside ICU string transformation functions, not user code.
        /// </summary>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public void AddReplace(int oldLength, int newLength)
        {
            if (oldLength < 0 || newLength < 0)
            {
                throw new ArgumentException(
                        "AddReplace(" + oldLength + ", " + newLength +
                        "): both lengths must be non-negative");
            }
            if (oldLength == 0 && newLength == 0)
            {
                return;
            }
            ++numChanges;
            int newDelta = newLength - oldLength;
            if (newDelta != 0)
            {
                if ((newDelta > 0 && delta >= 0 && newDelta > (int.MaxValue - delta)) ||
                        (newDelta < 0 && delta < 0 && newDelta < (int.MinValue - delta)))
                {
                    // Integer overflow or underflow.
                    throw new IndexOutOfRangeException();
                }
                delta += newDelta;
            }

            if (0 < oldLength && oldLength <= MAX_SHORT_CHANGE_OLD_LENGTH &&
                    newLength <= MAX_SHORT_CHANGE_NEW_LENGTH)
            {
                // Merge into previous same-lengths short-replacement record, if any.
                int u = (oldLength << 12) | (newLength << 9);
                int last = LastUnit;
                if (MAX_UNCHANGED < last && last < MAX_SHORT_CHANGE &&
                        (last & ~SHORT_CHANGE_NUM_MASK) == u &&
                        (last & SHORT_CHANGE_NUM_MASK) < SHORT_CHANGE_NUM_MASK)
                {
                    LastUnit = last + 1;
                    return;
                }
                Append(u);
                return;
            }

            int head = 0x7000;
            if (oldLength < LENGTH_IN_1TRAIL && newLength < LENGTH_IN_1TRAIL)
            {
                head |= oldLength << 6;
                head |= newLength;
                Append(head);
            }
            else if ((array.Length - length) >= 5 || GrowArray())
            {
                int limit = length + 1;
                if (oldLength < LENGTH_IN_1TRAIL)
                {
                    head |= oldLength << 6;
                }
                else if (oldLength <= 0x7fff)
                {
                    head |= LENGTH_IN_1TRAIL << 6;
                    array[limit++] = (char)(0x8000 | oldLength);
                }
                else
                {
                    head |= (LENGTH_IN_2TRAIL + (oldLength >> 30)) << 6;
                    array[limit++] = (char)(0x8000 | (oldLength >> 15));
                    array[limit++] = (char)(0x8000 | oldLength);
                }
                if (newLength < LENGTH_IN_1TRAIL)
                {
                    head |= newLength;
                }
                else if (newLength <= 0x7fff)
                {
                    head |= LENGTH_IN_1TRAIL;
                    array[limit++] = (char)(0x8000 | newLength);
                }
                else
                {
                    head |= LENGTH_IN_2TRAIL + (newLength >> 30);
                    array[limit++] = (char)(0x8000 | (newLength >> 15));
                    array[limit++] = (char)(0x8000 | newLength);
                }
                array[length] = (char)head;
                length = limit;
            }
        }

        private void Append(int r)
        {
            if (length < array.Length || GrowArray())
            {
                array[length++] = (char)r;
            }
        }

        private bool GrowArray()
        {
            int newCapacity;
            if (array.Length == STACK_CAPACITY)
            {
                newCapacity = 2000;
            }
            else if (array.Length == int.MaxValue)
            {
                throw new BufferOverflowException();
            }
            else if (array.Length >= (int.MaxValue / 2))
            {
                newCapacity = int.MinValue;
            }
            else
            {
                newCapacity = 2 * array.Length;
            }
            // Grow by at least 5 units so that a maximal change record will fit.
            if ((newCapacity - array.Length) < 5)
            {
                throw new BufferOverflowException();
            }
            //array = Arrays.copyOf(array, newCapacity);
            array = array.CopyOf(newCapacity);
            return true;
        }

        /// <summary>
        /// How much longer is the new text compared with the old text?
        /// Returns new length minus old length.
        /// </summary>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public int LengthDelta { get { return delta; } }

        /// <summary>
        /// Returns true if there are any change edits.
        /// </summary>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public bool HasChanges { get { return numChanges != 0; } }

        /// <summary>
        /// Gets the number of change edits.
        /// </summary>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public int NumberOfChanges { get { return numChanges; } }

        /// <summary>
        /// Access to the list of edits.
        /// </summary>
        /// <seealso cref="GetCoarseEnumerator()"/>
        /// <seealso cref="GetFineEnumerator()"/>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public sealed class Enumerator
        {
            private readonly char[] array;
            private int index;
            private readonly int length;
            /// <summary>
            /// 0 if we are not within compressed equal-length changes.
            /// Otherwise the number of remaining changes, including the current one.
            /// </summary>
            private int remaining;
            private readonly bool onlyChanges_, coarse;

            private int dir;  // iteration direction: back(<0), initial(0), forward(>0)
            private bool changed;
            private int oldLength_, newLength_;
            private int srcIndex, replIndex, destIndex;

            internal Enumerator(char[] a, int len, bool oc, bool crs)
            {
                array = a;
                length = len;
                onlyChanges_ = oc;
                coarse = crs;
            }

            private int ReadLength(int head)
            {
                if (head < LENGTH_IN_1TRAIL)
                {
                    return head;
                }
                else if (head < LENGTH_IN_2TRAIL)
                {
                    Debug.Assert(index < length);
                    Debug.Assert(array[index] >= 0x8000);
                    return array[index++] & 0x7fff;
                }
                else
                {
                    Debug.Assert((index + 2) <= length);
                    Debug.Assert(array[index] >= 0x8000);
                    Debug.Assert(array[index + 1] >= 0x8000);
                    int len = ((head & 1) << 30) |
                            ((array[index] & 0x7fff) << 15) |
                            (array[index + 1] & 0x7fff);
                    index += 2;
                    return len;
                }
            }

            private void UpdateNextIndexes()
            {
                srcIndex += oldLength_;
                if (changed)
                {
                    replIndex += newLength_;
                }
                destIndex += newLength_;
            }

            private void UpdatePreviousIndexes()
            {
                srcIndex -= oldLength_;
                if (changed)
                {
                    replIndex -= newLength_;
                }
                destIndex -= newLength_;
            }

            private bool NoNext()
            {
                // No change before or beyond the string.
                dir = 0;
                changed = false;
                oldLength_ = newLength_ = 0;
                return false;
            }

            /// <summary>
            /// Advances to the next edit.
            /// </summary>
            /// <returns>true if there is another edit.</returns>
            /// <draft>ICU 59</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public bool MoveNext()
            {
                return MoveNext(onlyChanges_);
            }

            private bool MoveNext(bool onlyChanges)
            {
                // Forward iteration: Update the string indexes to the limit of the current span,
                // and post-increment-read array units to assemble a new span.
                // Leaves the array index one after the last unit of that span.
                if (dir > 0)
                {
                    UpdateNextIndexes();
                }
                else
                {
                    if (dir < 0)
                    {
                        // Turn around from previous() to next().
                        // Post-increment-read the same span again.
                        if (remaining > 0)
                        {
                            // Fine-grained iterator:
                            // Stay on the current one of a sequence of compressed changes.
                            ++index;  // next() rests on the index after the sequence unit.
                            dir = 1;
                            return true;
                        }
                    }
                    dir = 1;
                }
                if (remaining >= 1)
                {
                    // Fine-grained iterator: Continue a sequence of compressed changes.
                    if (remaining > 1)
                    {
                        --remaining;
                        return true;
                    }
                    remaining = 0;
                }
                if (index >= length)
                {
                    return NoNext();
                }
                int u = array[index++];
                if (u <= MAX_UNCHANGED)
                {
                    // Combine adjacent unchanged ranges.
                    changed = false;
                    oldLength_ = u + 1;
                    while (index < length && (u = array[index]) <= MAX_UNCHANGED)
                    {
                        ++index;
                        oldLength_ += u + 1;
                    }
                    newLength_ = oldLength_;
                    if (onlyChanges)
                    {
                        UpdateNextIndexes();
                        if (index >= length)
                        {
                            return NoNext();
                        }
                        // already fetched u > MAX_UNCHANGED at index
                        ++index;
                    }
                    else
                    {
                        return true;
                    }
                }
                changed = true;
                if (u <= MAX_SHORT_CHANGE)
                {
                    int oldLen = u >> 12;
                    int newLen = (u >> 9) & MAX_SHORT_CHANGE_NEW_LENGTH;
                    int num = (u & SHORT_CHANGE_NUM_MASK) + 1;
                    if (coarse)
                    {
                        oldLength_ = num * oldLen;
                        newLength_ = num * newLen;
                    }
                    else
                    {
                        // Split a sequence of changes that was compressed into one unit.
                        oldLength_ = oldLen;
                        newLength_ = newLen;
                        if (num > 1)
                        {
                            remaining = num;  // This is the first of two or more changes.
                        }
                        return true;
                    }
                }
                else
                {
                    Debug.Assert(u <= 0x7fff);
                    oldLength_ = ReadLength((u >> 6) & 0x3f);
                    newLength_ = ReadLength(u & 0x3f);
                    if (!coarse)
                    {
                        return true;
                    }
                }
                // Combine adjacent changes.
                while (index < length && (u = array[index]) > MAX_UNCHANGED)
                {
                    ++index;
                    if (u <= MAX_SHORT_CHANGE)
                    {
                        int num = (u & SHORT_CHANGE_NUM_MASK) + 1;
                        oldLength_ += (u >> 12) * num;
                        newLength_ += ((u >> 9) & MAX_SHORT_CHANGE_NEW_LENGTH) * num;
                    }
                    else
                    {
                        Debug.Assert(u <= 0x7fff);
                        oldLength_ += ReadLength((u >> 6) & 0x3f);
                        newLength_ += ReadLength(u & 0x3f);
                    }
                }
                return true;
            }

            private bool Previous()
            {
                // Backward iteration: Pre-decrement-read array units to assemble a new span,
                // then update the string indexes to the start of that span.
                // Leaves the array index on the head unit of that span.
                if (dir >= 0)
                {
                    if (dir > 0)
                    {
                        // Turn around from next() to previous().
                        // Set the string indexes to the span limit and
                        // pre-decrement-read the same span again.
                        if (remaining > 0)
                        {
                            // Fine-grained iterator:
                            // Stay on the current one of a sequence of compressed changes.
                            --index;  // previous() rests on the sequence unit.
                            dir = -1;
                            return true;
                        }
                        UpdateNextIndexes();
                    }
                    dir = -1;
                }
                if (remaining > 0)
                {
                    // Fine-grained iterator: Continue a sequence of compressed changes.
                    int u2 = array[index];
                    Debug.Assert(MAX_UNCHANGED < u2 && u2 <= MAX_SHORT_CHANGE);
                    if (remaining <= (u2 & SHORT_CHANGE_NUM_MASK))
                    {
                        ++remaining;
                        UpdatePreviousIndexes();
                        return true;
                    }
                    remaining = 0;
                }
                if (index <= 0)
                {
                    return NoNext();
                }
                int u = array[--index];
                if (u <= MAX_UNCHANGED)
                {
                    // Combine adjacent unchanged ranges.
                    changed = false;
                    oldLength_ = u + 1;
                    while (index > 0 && (u = array[index - 1]) <= MAX_UNCHANGED)
                    {
                        --index;
                        oldLength_ += u + 1;
                    }
                    newLength_ = oldLength_;
                    // No need to handle onlyChanges as long as previous() is called only from findIndex().
                    UpdatePreviousIndexes();
                    return true;
                }
                changed = true;
                if (u <= MAX_SHORT_CHANGE)
                {
                    int oldLen = u >> 12;
                    int newLen = (u >> 9) & MAX_SHORT_CHANGE_NEW_LENGTH;
                    int num = (u & SHORT_CHANGE_NUM_MASK) + 1;
                    if (coarse)
                    {
                        oldLength_ = num * oldLen;
                        newLength_ = num * newLen;
                    }
                    else
                    {
                        // Split a sequence of changes that was compressed into one unit.
                        oldLength_ = oldLen;
                        newLength_ = newLen;
                        if (num > 1)
                        {
                            remaining = 1;  // This is the last of two or more changes.
                        }
                        UpdatePreviousIndexes();
                        return true;
                    }
                }
                else
                {
                    if (u <= 0x7fff)
                    {
                        // The change is encoded in u alone.
                        oldLength_ = ReadLength((u >> 6) & 0x3f);
                        newLength_ = ReadLength(u & 0x3f);
                    }
                    else
                    {
                        // Back up to the head of the change, read the lengths,
                        // and reset the index to the head again.
                        Debug.Assert(index > 0);
                        while ((u = array[--index]) > 0x7fff) { }
                        Debug.Assert(u > MAX_SHORT_CHANGE);
                        int headIndex = index++;
                        oldLength_ = ReadLength((u >> 6) & 0x3f);
                        newLength_ = ReadLength(u & 0x3f);
                        index = headIndex;
                    }
                    if (!coarse)
                    {
                        UpdatePreviousIndexes();
                        return true;
                    }
                }
                // Combine adjacent changes.
                while (index > 0 && (u = array[index - 1]) > MAX_UNCHANGED)
                {
                    --index;
                    if (u <= MAX_SHORT_CHANGE)
                    {
                        int num = (u & SHORT_CHANGE_NUM_MASK) + 1;
                        oldLength_ += (u >> 12) * num;
                        newLength_ += ((u >> 9) & MAX_SHORT_CHANGE_NEW_LENGTH) * num;
                    }
                    else if (u <= 0x7fff)
                    {
                        // Read the lengths, and reset the index to the head again.
                        int headIndex = index++;
                        oldLength_ += ReadLength((u >> 6) & 0x3f);
                        newLength_ += ReadLength(u & 0x3f);
                        index = headIndex;
                    }
                }
                UpdatePreviousIndexes();
                return true;
            }

            /// <summary>
            /// Finds the edit that contains the source index.
            /// The source index may be found in a non-change
            /// even if normal iteration would skip non-changes.
            /// Normal iteration can continue from a found edit.
            /// <para/>
            /// The iterator state before this search logically does not matter.
            /// (It may affect the performance of the search.)
            /// <para/>
            /// The iterator state after this search is undefined
            /// if the source index is out of bounds for the source string.
            /// </summary>
            /// <param name="i">Source index.</param>
            /// <returns>true if the edit for the source index was found.</returns>
            /// <draft>ICU 59</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public bool FindSourceIndex(int i)
            {
                return FindIndex(i, true) == 0;
            }

            /// <summary>
            /// Finds the edit that contains the destination index.
            /// The destination index may be found in a non-change
            /// even if normal iteration would skip non-changes.
            /// Normal iteration can continue from a found edit.
            /// <para/>
            /// The iterator state before this search logically does not matter.
            /// (It may affect the performance of the search.)
            /// <para/>
            /// The iterator state after this search is undefined
            /// if the source index is out of bounds for the source string.
            /// </summary>
            /// <param name="i">Destination index.</param>
            /// <returns>true if the edit for the destination index was found.</returns>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public bool FindDestinationIndex(int i)
            {
                return FindIndex(i, false) == 0;
            }

            /// <returns>-1: error or i&lt;0; 0: found; 1: i>=string length</returns>
            private int FindIndex(int i, bool findSource)
            {
                if (i < 0) { return -1; }
                int spanStart, spanLength;
                if (findSource)
                {  // find source index
                    spanStart = srcIndex;
                    spanLength = oldLength_;
                }
                else
                {  // find destination index
                    spanStart = destIndex;
                    spanLength = newLength_;
                }
                if (i < spanStart)
                {
                    if (i >= (spanStart / 2))
                    {
                        // Search backwards.
                        for (; ; )
                        {
                            bool hasPrevious = Previous();
                            Debug.Assert(hasPrevious);  // because i>=0 and the first span starts at 0
                            spanStart = findSource ? srcIndex : destIndex;
                            if (i >= spanStart)
                            {
                                // The index is in the current span.
                                return 0;
                            }
                            if (remaining > 0)
                            {
                                // Is the index in one of the remaining compressed edits?
                                // spanStart is the start of the current span, first of the remaining ones.
                                spanLength = findSource ? oldLength_ : newLength_;
                                int u = array[index];
                                Debug.Assert(MAX_UNCHANGED < u && u <= MAX_SHORT_CHANGE);
                                int num = (u & SHORT_CHANGE_NUM_MASK) + 1 - remaining;
                                int len = num * spanLength;
                                if (i >= (spanStart - len))
                                {
                                    int n = ((spanStart - i - 1) / spanLength) + 1;
                                    // 1 <= n <= num
                                    srcIndex -= n * oldLength_;
                                    replIndex -= n * newLength_;
                                    destIndex -= n * newLength_;
                                    remaining += n;
                                    return 0;
                                }
                                // Skip all of these edits at once.
                                srcIndex -= num * oldLength_;
                                replIndex -= num * newLength_;
                                destIndex -= num * newLength_;
                                remaining = 0;
                            }
                        }
                    }
                    // Reset the iterator to the start.
                    dir = 0;
                    index = remaining = oldLength_ = newLength_ = srcIndex = replIndex = destIndex = 0;
                }
                else if (i < (spanStart + spanLength))
                {
                    // The index is in the current span.
                    return 0;
                }
                while (MoveNext(false))
                {
                    if (findSource)
                    {
                        spanStart = srcIndex;
                        spanLength = oldLength_;
                    }
                    else
                    {
                        spanStart = destIndex;
                        spanLength = newLength_;
                    }
                    if (i < (spanStart + spanLength))
                    {
                        // The index is in the current span.
                        return 0;
                    }
                    if (remaining > 1)
                    {
                        // Is the index in one of the remaining compressed edits?
                        // spanStart is the start of the current span, first of the remaining ones.
                        int len = remaining * spanLength;
                        if (i < (spanStart + len))
                        {
                            int n = (i - spanStart) / spanLength;  // 1 <= n <= remaining - 1
                            srcIndex += n * oldLength_;
                            replIndex += n * newLength_;
                            destIndex += n * newLength_;
                            remaining -= n;
                            return 0;
                        }
                        // Make next() skip all of these edits at once.
                        oldLength_ *= remaining;
                        newLength_ *= remaining;
                        remaining = 0;
                    }
                }
                return 1;
            }

            /// <summary>
            /// Returns the destination index corresponding to the given source index.
            /// If the source index is inside a change edit (not at its start),
            /// then the destination index at the end of that edit is returned,
            /// since there is no information about index mapping inside a change edit.
            /// <para/>
            /// (This means that indexes to the start and middle of an edit,
            /// for example around a grapheme cluster, are mapped to indexes
            /// encompassing the entire edit.
            /// The alternative, mapping an interior index to the start,
            /// would map such an interval to an empty one.)
            /// <para/>
            /// This operation will usually but not always modify this object.
            /// The iterator state after this search is undefined.
            /// </summary>
            /// <param name="i">Source index.</param>
            /// <returns>Destination index; undefined if i is not 0..string length.</returns>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public int DestinationIndexFromSourceIndex(int i)
            {
                int where = FindIndex(i, true);
                if (where < 0)
                {
                    // Error or before the string.
                    return 0;
                }
                if (where > 0 || i == srcIndex)
                {
                    // At or after string length, or at start of the found span.
                    return destIndex;
                }
                if (changed)
                {
                    // In a change span, map to its end.
                    return destIndex + newLength_;
                }
                else
                {
                    // In an unchanged span, offset 1:1 within it.
                    return destIndex + (i - srcIndex);
                }
            }

            /// <summary>
            /// Returns the source index corresponding to the given destination index.
            /// If the destination index is inside a change edit (not at its start),
            /// then the source index at the end of that edit is returned,
            /// since there is no information about index mapping inside a change edit.
            /// <para/>
            /// (This means that indexes to the start and middle of an edit,
            /// for example around a grapheme cluster, are mapped to indexes
            /// encompassing the entire edit.
            /// The alternative, mapping an interior index to the start,
            /// would map such an interval to an empty one.)
            /// <para/>
            /// This operation will usually but not always modify this object.
            /// The iterator state after this search is undefined.
            /// </summary>
            /// <param name="i">Destination index.</param>
            /// <returns>Source index; undefined if i is not 0..string length.</returns>
            /// <draft>ICU 60</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public int SourceIndexFromDestinationIndex(int i)
            {
                int where = FindIndex(i, false);
                if (where < 0)
                {
                    // Error or before the string.
                    return 0;
                }
                if (where > 0 || i == destIndex)
                {
                    // At or after string length, or at start of the found span.
                    return srcIndex;
                }
                if (changed)
                {
                    // In a change span, map to its end.
                    return srcIndex + oldLength_;
                }
                else
                {
                    // In an unchanged span, offset within it.
                    return srcIndex + (i - destIndex);
                }
            }

            /// <summary>
            /// Returns true if this edit replaces <see cref="OldLength"/> units with <see cref="NewLength"/> different ones.
            /// false if <see cref="OldLength"/> units remain unchanged.
            /// </summary>
            /// <draft>ICU 59</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public bool HasChange { get { return changed; } }
            /// <summary>
            /// Gets the number of units in the original string which are replaced or remain unchanged.
            /// </summary>
            /// <draft>ICU 59</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public int OldLength { get { return oldLength_; } }
            /// <summary>
            /// Gets the number of units in the modified string, if <see cref="HasChange"/> is true.
            /// Same as <see cref="OldLength"/> if <see cref="HasChange"/> is false.
            /// </summary>
            /// <draft>ICU 59</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public int NewLength { get { return newLength_; } }

            /**
             * @return the current index into the source string
             * @draft ICU 59
             * @provisional This API might change or be removed in a future release.
             */
            /// <summary>
            /// Gets the number of units in the original string which are replaced or remain unchanged.
            /// </summary>
            /// <draft>ICU 59</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public int SourceIndex { get { return srcIndex; } }
            /// <summary>
            /// Gets the current index into the replacement-characters-only string,
            /// not counting unchanged spans.
            /// </summary>
            /// <draft>ICU 59</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public int ReplacementIndex { get { return replIndex; } }
            /// <summary>
            /// Gets the current index into the full destination string.
            /// </summary>
            /// <draft>ICU 59</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public int DestinationIndex { get { return destIndex; } }
        };

        /// <summary>
        /// Returns an <see cref="Enumerator"/> for coarse-grained changes for simple string updates.
        /// Skips non-changes.
        /// </summary>
        /// <returns>An <see cref="Enumerator"/> that merges adjacent changes.</returns>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public Enumerator GetCoarseChangesEnumerator()
        {
            return new Enumerator(array, length, true, true);
        }
        /// <summary>
        /// Returns an <see cref="Enumerator"/> for coarse-grained changes and non-changes for simple string updates.
        /// </summary>
        /// <returns>An <see cref="Enumerator"/> that merges adjacent changes.</returns>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public Enumerator GetCoarseEnumerator()
        {
            return new Enumerator(array, length, false, true);
        }

        /// <summary>
        /// Returns an <see cref="Enumerator"/> for fine-grained changes for modifying styled text.
        /// Skips non-changes.
        /// </summary>
        /// <returns>An <see cref="Enumerator"/> that separates adjacent changes.</returns>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public Enumerator GetFineChangesEnumerator()
        {
            return new Enumerator(array, length, true, false);
        }

        /// <summary>
        /// Returns an <see cref="Enumerator"/> for fine-grained changes and non-changes for modifying styled text.
        /// </summary>
        /// <returns>An <see cref="Enumerator"/> that separates adjacent changes.</returns>
        /// <draft>ICU 59</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public Enumerator GetFineEnumerator()
        {
            return new Enumerator(array, length, false, false);
        }

        /// <summary>
        /// Merges the two input Edits and appends the result to this object.
        /// </summary>
        /// <remarks>
        /// Consider two string transformations (for example, normalization and case mapping)
        /// where each records Edits in addition to writing an output string.
        /// <para/>
        /// Edits <paramref name="ab"/> reflect how substrings of input string a
        /// map to substrings of intermediate string b.
        /// <para/>
        /// Edits <paramref name="bc"/> reflect how substrings of intermediate string b
        /// map to substrings of output string c.
        /// <para/>
        /// This function merges <paramref name="ab"/> and <paramref name="bc"/> such that the additional edits
        /// recorded in this object reflect how substrings of input string a
        /// map to substrings of output string c.
        /// <para/>
        /// If unrelated <see cref="Edits"/> are passed in where the output string of the first
        /// has a different length than the input string of the second,
        /// then an <see cref="ArgumentException"/> is thrown.
        /// </remarks>
        /// <param name="ab">Reflects how substrings of input string a
        /// map to substrings of intermediate string b.</param>
        /// <param name="bc">Reflects how substrings of intermediate string b
        /// map to substrings of output string c.</param>
        /// <returns>This, with the merged edits appended.</returns>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public Edits MergeAndAppend(Edits ab, Edits bc)
        {
            // Picture string a --(Edits ab)--> string b --(Edits bc)--> string c.
            // Parallel iteration over both Edits.
            Enumerator abIter = ab.GetFineEnumerator();
            Enumerator bcIter = bc.GetFineEnumerator();
            bool abHasNext = true, bcHasNext = true;
            // Copy iterator state into local variables, so that we can modify and subdivide spans.
            // ab old & new length, bc old & new length
            int aLength = 0, ab_bLength = 0, bc_bLength = 0, cLength = 0;
            // When we have different-intermediate-length changes, we accumulate a larger change.
            int pending_aLength = 0, pending_cLength = 0;
            for (; ; )
            {
                // At this point, for each of the two iterators:
                // Either we are done with the locally cached current edit,
                // and its intermediate-string length has been reset,
                // or we will continue to work with a truncated remainder of this edit.
                //
                // If the current edit is done, and the iterator has not yet reached the end,
                // then we fetch the next edit. This is true for at least one of the iterators.
                //
                // Normally it does not matter whether we fetch from ab and then bc or vice versa.
                // However, the result is observably different when
                // ab deletions meet bc insertions at the same intermediate-string index.
                // Some users expect the bc insertions to come first, so we fetch from bc first.
                if (bc_bLength == 0)
                {
                    if (bcHasNext && (bcHasNext = bcIter.MoveNext()))
                    {
                        bc_bLength = bcIter.OldLength;
                        cLength = bcIter.NewLength;
                        if (bc_bLength == 0)
                        {
                            // insertion
                            if (ab_bLength == 0 || !abIter.HasChange)
                            {
                                AddReplace(pending_aLength, pending_cLength + cLength);
                                pending_aLength = pending_cLength = 0;
                            }
                            else
                            {
                                pending_cLength += cLength;
                            }
                            continue;
                        }
                    }
                    // else see if the other iterator is done, too.
                }
                if (ab_bLength == 0)
                {
                    if (abHasNext && (abHasNext = abIter.MoveNext()))
                    {
                        aLength = abIter.OldLength;
                        ab_bLength = abIter.NewLength;
                        if (ab_bLength == 0)
                        {
                            // deletion
                            if (bc_bLength == bcIter.OldLength || !bcIter.HasChange)
                            {
                                AddReplace(pending_aLength + aLength, pending_cLength);
                                pending_aLength = pending_cLength = 0;
                            }
                            else
                            {
                                pending_aLength += aLength;
                            }
                            continue;
                        }
                    }
                    else if (bc_bLength == 0)
                    {
                        // Both iterators are done at the same time:
                        // The intermediate-string lengths match.
                        break;
                    }
                    else
                    {
                        throw new ArgumentException(
                                "The ab output string is shorter than the bc input string.");
                    }
                }
                if (bc_bLength == 0)
                {
                    throw new ArgumentException(
                            "The bc input string is shorter than the ab output string.");
                }
                //  Done fetching: ab_bLength > 0 && bc_bLength > 0

                // The current state has two parts:
                // - Past: We accumulate a longer ac edit in the "pending" variables.
                // - Current: We have copies of the current ab/bc edits in local variables.
                //   At least one side is newly fetched.
                //   One side might be a truncated remainder of an edit we fetched earlier.

                if (!abIter.HasChange && !bcIter.HasChange)
                {
                    // An unchanged span all the way from string a to string c.
                    if (pending_aLength != 0 || pending_cLength != 0)
                    {
                        AddReplace(pending_aLength, pending_cLength);
                        pending_aLength = pending_cLength = 0;
                    }
                    int unchangedLength = aLength <= cLength ? aLength : cLength;
                    AddUnchanged(unchangedLength);
                    ab_bLength = aLength -= unchangedLength;
                    bc_bLength = cLength -= unchangedLength;
                    // At least one of the unchanged spans is now empty.
                    continue;
                }
                if (!abIter.HasChange && bcIter.HasChange)
                {
                    // Unchanged a->b but changed b->c.
                    if (ab_bLength >= bc_bLength)
                    {
                        // Split the longer unchanged span into change + remainder.
                        AddReplace(pending_aLength + bc_bLength, pending_cLength + cLength);
                        pending_aLength = pending_cLength = 0;
                        aLength = ab_bLength -= bc_bLength;
                        bc_bLength = 0;
                        continue;
                    }
                    // Handle the shorter unchanged span below like a change.
                }
                else if (abIter.HasChange && !bcIter.HasChange)
                {
                    // Changed a->b and then unchanged b->c.
                    if (ab_bLength <= bc_bLength)
                    {
                        // Split the longer unchanged span into change + remainder.
                        AddReplace(pending_aLength + aLength, pending_cLength + ab_bLength);
                        pending_aLength = pending_cLength = 0;
                        cLength = bc_bLength -= ab_bLength;
                        ab_bLength = 0;
                        continue;
                    }
                    // Handle the shorter unchanged span below like a change.
                }
                else
                {  // both abIter.hasChange() && bcIter.hasChange()
                    if (ab_bLength == bc_bLength)
                    {
                        // Changes on both sides up to the same position. Emit & reset.
                        AddReplace(pending_aLength + aLength, pending_cLength + cLength);
                        pending_aLength = pending_cLength = 0;
                        ab_bLength = bc_bLength = 0;
                        continue;
                    }
                }
                // Accumulate the a->c change, reset the shorter side,
                // keep a remainder of the longer one.
                pending_aLength += aLength;
                pending_cLength += cLength;
                if (ab_bLength < bc_bLength)
                {
                    bc_bLength -= ab_bLength;
                    cLength = ab_bLength = 0;
                }
                else
                {  // ab_bLength > bc_bLength
                    ab_bLength -= bc_bLength;
                    aLength = bc_bLength = 0;
                }
            }
            if (pending_aLength != 0 || pending_cLength != 0)
            {
                AddReplace(pending_aLength, pending_cLength);
            }
            return this;
        }
    }
}
