using System;
using System.Diagnostics;

namespace ICU4N.Impl
{
    /// <summary>
    /// A <see cref="Trie2Writable"/> is a modifiable, or build-time <see cref="Trie2"/>.
    /// Functions for reading data from the <see cref="Trie"/> are all from class <see cref="Trie2"/>.
    /// </summary>
    public class Trie2Writable : Trie2
    {
        /// <summary>
        /// Create a new, empty, writable <see cref="Trie2"/>. 32-bit data values are used.
        /// </summary>
        /// <param name="initialValueP">The initial value that is set for all code points.</param>
        /// <param name="errorValueP">The value for out-of-range code points and illegal UTF-8.</param>
        public Trie2Writable(int initialValueP, int errorValueP)
        {
            // This constructor corresponds to utrie2_open() in ICU4C.
            Init(initialValueP, errorValueP);
        }


        private void Init(int initialValueP, int errorValueP)
        {
            this.initialValue = initialValueP;
            this.errorValue = errorValueP;
            this.highStart = 0x110000;

            this.data = new int[UNEWTRIE2_INITIAL_DATA_LENGTH];
            this.dataCapacity = UNEWTRIE2_INITIAL_DATA_LENGTH;
            this.initialValue = initialValueP;
            this.errorValue = errorValueP;
            this.highStart = 0x110000;
            this.firstFreeBlock = 0;  /* no free block in the list */
            this.isCompacted = false;

            /*
             * preallocate and reset
             * - ASCII
             * - the bad-UTF-8-data block
             * - the null data block
             */
            int i, j;
            for (i = 0; i < 0x80; ++i)
            {
                data[i] = initialValue;
            }
            for (; i < 0xc0; ++i)
            {
                data[i] = errorValue;
            }
            for (i = UNEWTRIE2_DATA_NULL_OFFSET; i < UNEWTRIE2_DATA_START_OFFSET; ++i)
            {
                data[i] = initialValue;
            }
            dataNullOffset = UNEWTRIE2_DATA_NULL_OFFSET;
            dataLength = UNEWTRIE2_DATA_START_OFFSET;

            /* set the index-2 indexes for the 2=0x80>>UTRIE2_SHIFT_2 ASCII data blocks */
            for (i = 0, j = 0; j < 0x80; ++i, j += UTRIE2_DATA_BLOCK_LENGTH)
            {
                index2[i] = j;
                map[i] = 1;
            }

            /* reference counts for the bad-UTF-8-data block */
            for (; j < 0xc0; ++i, j += UTRIE2_DATA_BLOCK_LENGTH)
            {
                map[i] = 0;
            }

            /*
             * Reference counts for the null data block: all blocks except for the ASCII blocks.
             * Plus 1 so that we don't drop this block during compaction.
             * Plus as many as needed for lead surrogate code points.
             */
            /* i==newTrie->dataNullOffset */
            map[i++] =
                (0x110000 >> UTRIE2_SHIFT_2) -
                (0x80 >> UTRIE2_SHIFT_2) +
                1 +
                UTRIE2_LSCP_INDEX_2_LENGTH;
            j += UTRIE2_DATA_BLOCK_LENGTH;
            for (; j < UNEWTRIE2_DATA_START_OFFSET; ++i, j += UTRIE2_DATA_BLOCK_LENGTH)
            {
                map[i] = 0;
            }

            /*
             * set the remaining indexes in the BMP index-2 block
             * to the null data block
             */
            for (i = 0x80 >> UTRIE2_SHIFT_2; i < UTRIE2_INDEX_2_BMP_LENGTH; ++i)
            {
                index2[i] = UNEWTRIE2_DATA_NULL_OFFSET;
            }

            /*
             * Fill the index gap with impossible values so that compaction
             * does not overlap other index-2 blocks with the gap.
             */
            for (i = 0; i < UNEWTRIE2_INDEX_GAP_LENGTH; ++i)
            {
                index2[UNEWTRIE2_INDEX_GAP_OFFSET + i] = -1;
            }

            /* set the indexes in the null index-2 block */
            for (i = 0; i < UTRIE2_INDEX_2_BLOCK_LENGTH; ++i)
            {
                index2[UNEWTRIE2_INDEX_2_NULL_OFFSET + i] = UNEWTRIE2_DATA_NULL_OFFSET;
            }
            index2NullOffset = UNEWTRIE2_INDEX_2_NULL_OFFSET;
            index2Length = UNEWTRIE2_INDEX_2_START_OFFSET;

            /* set the index-1 indexes for the linear index-2 block */
            for (i = 0, j = 0;
                i < UTRIE2_OMITTED_BMP_INDEX_1_LENGTH;
                ++i, j += UTRIE2_INDEX_2_BLOCK_LENGTH
            )
            {
                index1[i] = j;
            }

            /* set the remaining index-1 indexes to the null index-2 block */
            for (; i < UNEWTRIE2_INDEX_1_LENGTH; ++i)
            {
                index1[i] = UNEWTRIE2_INDEX_2_NULL_OFFSET;
            }

            /*
             * Preallocate and reset data for U+0080..U+07ff,
             * for 2-byte UTF-8 which will be compacted in 64-blocks
             * even if UTRIE2_DATA_BLOCK_LENGTH is smaller.
             */
            for (i = 0x80; i < 0x800; i += UTRIE2_DATA_BLOCK_LENGTH)
            {
                Set(i, initialValue);
            }

        }

        /// <summary>
        /// Create a new build time (modifiable) <see cref="Trie2"/> whose contents are the same as the source <see cref="Trie2"/>.
        /// </summary>
        /// <param name="source">The source <see cref="Trie2"/>.  Its contents will be copied into the new <see cref="Trie2"/>.</param>
        public Trie2Writable(Trie2 source)
        {
            Init(source.initialValue, source.errorValue);

            foreach (Range r in source)
            {
                SetRange(r, true);
            }
        }

        private bool IsInNullBlock(int c, bool forLSCP)
        {
            int i2, block;

            if (char.IsHighSurrogate((char)c) && forLSCP)
            {
                i2 = (UTRIE2_LSCP_INDEX_2_OFFSET - (0xd800 >> UTRIE2_SHIFT_2)) +
                    (c >> UTRIE2_SHIFT_2);
            }
            else
            {
                i2 = index1[c >> UTRIE2_SHIFT_1] +
                    ((c >> UTRIE2_SHIFT_2) & UTRIE2_INDEX_2_MASK);
            }
            block = index2[i2];
            return (block == dataNullOffset);
        }

        private int AllocIndex2Block()
        {
            int newBlock, newTop;

            newBlock = index2Length;
            newTop = newBlock + UTRIE2_INDEX_2_BLOCK_LENGTH;
            if (newTop > index2.Length)
            {
                throw new InvalidOperationException("Internal error in Trie2 creation.");
                /*
                 * Should never occur.
                 * Either UTRIE2_MAX_BUILD_TIME_INDEX_LENGTH is incorrect,
                 * or the code writes more values than should be possible.
                 */
            }
            index2Length = newTop;
            System.Array.Copy(index2, index2NullOffset, index2, newBlock, UTRIE2_INDEX_2_BLOCK_LENGTH);
            return newBlock;
        }

        private int GetIndex2Block(int c, bool forLSCP)
        {
            int i1, i2;

            if (c >= 0xd800 && c < 0xdc00 && forLSCP)
            {
                return UTRIE2_LSCP_INDEX_2_OFFSET;
            }

            i1 = c >> UTRIE2_SHIFT_1;
            i2 = index1[i1];
            if (i2 == index2NullOffset)
            {
                i2 = AllocIndex2Block();
                index1[i1] = i2;
            }
            return i2;
        }

        private int AllocDataBlock(int copyBlock)
        {
            int newBlock, newTop;

            if (firstFreeBlock != 0)
            {
                /* get the first free block */
                newBlock = firstFreeBlock;
                firstFreeBlock = -map[newBlock >> UTRIE2_SHIFT_2];
            }
            else
            {
                /* get a new block from the high end */
                newBlock = dataLength;
                newTop = newBlock + UTRIE2_DATA_BLOCK_LENGTH;
                if (newTop > dataCapacity)
                {
                    /* out of memory in the data array */
                    int capacity;
                    int[] newData;

                    if (dataCapacity < UNEWTRIE2_MEDIUM_DATA_LENGTH)
                    {
                        capacity = UNEWTRIE2_MEDIUM_DATA_LENGTH;
                    }
                    else if (dataCapacity < UNEWTRIE2_MAX_DATA_LENGTH)
                    {
                        capacity = UNEWTRIE2_MAX_DATA_LENGTH;
                    }
                    else
                    {
                        /*
                         * Should never occur.
                         * Either UNEWTRIE2_MAX_DATA_LENGTH is incorrect,
                         * or the code writes more values than should be possible.
                         */
                        throw new InvalidOperationException("Internal error in Trie2 creation.");
                    }
                    newData = new int[capacity];
                    System.Array.Copy(data, 0, newData, 0, dataLength);
                    data = newData;
                    dataCapacity = capacity;
                }
                dataLength = newTop;
            }
            System.Array.Copy(data, copyBlock, data, newBlock, UTRIE2_DATA_BLOCK_LENGTH);
            map[newBlock >> UTRIE2_SHIFT_2] = 0;
            return newBlock;
        }


        /* call when the block's reference counter reaches 0 */
        private void ReleaseDataBlock(int block)
        {
            /* put this block at the front of the free-block chain */
            map[block >> UTRIE2_SHIFT_2] = -firstFreeBlock;
            firstFreeBlock = block;
        }


        private bool IsWritableBlock(int block)
        {
            return (block != dataNullOffset && 1 == map[block >> UTRIE2_SHIFT_2]);
        }

        private void SetIndex2Entry(int i2, int block)
        {
            int oldBlock;
            ++map[block >> UTRIE2_SHIFT_2];  /* increment first, in case block==oldBlock! */
            oldBlock = index2[i2];
            if (0 == --map[oldBlock >> UTRIE2_SHIFT_2])
            {
                ReleaseDataBlock(oldBlock);
            }
            index2[i2] = block;
        }

        /// <summary>
        /// No error checking for illegal arguments.
        /// </summary>
        /// <internal/>
        private int GetDataBlock(int c, bool forLSCP)
        {
            int i2, oldBlock, newBlock;

            i2 = GetIndex2Block(c, forLSCP);

            i2 += (c >> UTRIE2_SHIFT_2) & UTRIE2_INDEX_2_MASK;
            oldBlock = index2[i2];
            if (IsWritableBlock(oldBlock))
            {
                return oldBlock;
            }

            /* allocate a new data block */
            newBlock = AllocDataBlock(oldBlock);
            SetIndex2Entry(i2, newBlock);
            return newBlock;
        }

        /// <summary>
        /// Set a value for a code point.
        /// </summary>
        /// <param name="c">The code point.</param>
        /// <param name="value">The value.</param>
        /// <returns>This.</returns>
        public virtual Trie2Writable Set(int c, int value)
        {
            if (c < 0 || c > 0x10ffff)
            {
                throw new ArgumentException("Invalid code point.");
            }
            Set(c, true, value);
            fHash = 0;
            return this;
        }

        private Trie2Writable Set(int c, bool forLSCP, int value)
        {
            int block;
            if (isCompacted)
            {
                Uncompact();
            }
            block = GetDataBlock(c, forLSCP);
            data[block + (c & UTRIE2_DATA_MASK)] = value;
            return this;
        }

        /// <summary>
        /// Uncompact a compacted <see cref="Trie2Writable"/>.
        /// </summary>
        /// <remarks>
        /// This is needed if a the <see cref="Trie2Writable"/> was compacted in preparation for creating a read-only
        /// <see cref="Trie2"/>, and then is subsequently altered.
        /// <para/>
        /// The structure is a bit awkward - it would be cleaner to leave the original
        /// <see cref="Trie2"/> unaltered - but compacting in place was taken directly from the ICU4C code.
        /// <para/>
        /// The approach is to create a new (uncompacted) <see cref="Trie2Writable"/> from this one, then transfer
        /// the guts from the new to the old.
        /// </remarks>
        private void Uncompact()
        {
            Trie2Writable tempTrie = new Trie2Writable(this);

            // Members from Trie2Writable
            this.index1 = tempTrie.index1;
            this.index2 = tempTrie.index2;
            this.data = tempTrie.data;
            this.index2Length = tempTrie.index2Length;
            this.dataCapacity = tempTrie.dataCapacity;
            this.isCompacted = tempTrie.isCompacted;

            // Members From Trie2
            this.header = tempTrie.header;
            this.index = tempTrie.index;
            this.data16 = tempTrie.data16;
            this.data32 = tempTrie.data32;
            this.indexLength = tempTrie.indexLength;
            this.dataLength = tempTrie.dataLength;
            this.index2NullOffset = tempTrie.index2NullOffset;
            this.initialValue = tempTrie.initialValue;
            this.errorValue = tempTrie.errorValue;
            this.highStart = tempTrie.highStart;
            this.highValueIndex = tempTrie.highValueIndex;
            this.dataNullOffset = tempTrie.dataNullOffset;
        }


        private void WriteBlock(int block, int value)
        {
            int limit = block + UTRIE2_DATA_BLOCK_LENGTH;
            while (block < limit)
            {
                data[block++] = value;
            }
        }

        /// <summary>
        /// <paramref name="initialValue"/> is ignored if <paramref name="overwrite"/>=TRUE.
        /// </summary>
        /// <internal/>
        private void FillBlock(int block, /*UChar32*/ int start, /*UChar32*/ int limit,
                  int value, int initialValue, bool overwrite)
        {
            int i;
            int pLimit = block + limit;
            if (overwrite)
            {
                for (i = block + start; i < pLimit; i++)
                {
                    data[i] = value;
                }
            }
            else
            {
                for (i = block + start; i < pLimit; i++)
                {
                    if (data[i] == initialValue)
                    {
                        data[i] = value;
                    }
                }
            }
        }

        /// <summary>
        /// Set a value in a range of code points [start..end].
        /// <para/>
        /// All code points c with start&lt;=c&lt;=end will get the value if
        /// overwrite is TRUE or if the old value is the initial value.
        /// </summary>
        /// <param name="start">The first code point to get the value.</param>
        /// <param name="end">The last code point to get the value (inclusive).</param>
        /// <param name="value">The value.</param>
        /// <param name="overwrite">Flag for whether old non-initial values are to be overwritten.</param>
        /// <returns>This.</returns>
        public virtual Trie2Writable SetRange(int start, int end,
                              int value, bool overwrite)
        {
            /*
             * repeat value in [start..end]
             * mark index values for repeat-data blocks by setting bit 31 of the index values
             * fill around existing values if any, if(overwrite)
             */
            int block, rest, repeatBlock;
            int /*UChar32*/ limit;

            if (start > 0x10ffff || start < 0 || end > 0x10ffff || end < 0 || start > end)
            {
                throw new ArgumentException("Invalid code point range.");
            }
            if (!overwrite && value == initialValue)
            {
                return this; /* nothing to do */
            }
            fHash = 0;
            if (isCompacted)
            {
                this.Uncompact();
            }

            limit = end + 1;
            if ((start & UTRIE2_DATA_MASK) != 0)
            {
                int  /*UChar32*/ nextStart;

                /* set partial block at [start..following block boundary[ */
                block = GetDataBlock(start, true);

                nextStart = (start + UTRIE2_DATA_BLOCK_LENGTH) & ~UTRIE2_DATA_MASK;
                if (nextStart <= limit)
                {
                    FillBlock(block, start & UTRIE2_DATA_MASK, UTRIE2_DATA_BLOCK_LENGTH,
                              value, initialValue, overwrite);
                    start = nextStart;
                }
                else
                {
                    FillBlock(block, start & UTRIE2_DATA_MASK, limit & UTRIE2_DATA_MASK,
                              value, initialValue, overwrite);
                    return this;
                }
            }

            /* number of positions in the last, partial block */
            rest = limit & UTRIE2_DATA_MASK;

            /* round down limit to a block boundary */
            limit &= ~UTRIE2_DATA_MASK;

            /* iterate over all-value blocks */
            if (value == initialValue)
            {
                repeatBlock = dataNullOffset;
            }
            else
            {
                repeatBlock = -1;
            }

            while (start < limit)
            {
                int i2;
                bool setRepeatBlock = false;

                if (value == initialValue && IsInNullBlock(start, true))
                {
                    start += UTRIE2_DATA_BLOCK_LENGTH; /* nothing to do */
                    continue;
                }

                /* get index value */
                i2 = GetIndex2Block(start, true);
                i2 += (start >> UTRIE2_SHIFT_2) & UTRIE2_INDEX_2_MASK;
                block = index2[i2];
                if (IsWritableBlock(block))
                {
                    /* already allocated */
                    if (overwrite && block >= UNEWTRIE2_DATA_0800_OFFSET)
                    {
                        /*
                         * We overwrite all values, and it's not a
                         * protected (ASCII-linear or 2-byte UTF-8) block:
                         * replace with the repeatBlock.
                         */
                        setRepeatBlock = true;
                    }
                    else
                    {
                        /* !overwrite, or protected block: just write the values into this block */
                        FillBlock(block,
                                  0, UTRIE2_DATA_BLOCK_LENGTH,
                                  value, initialValue, overwrite);
                    }
                }
                else if (data[block] != value && (overwrite || block == dataNullOffset))
                {
                    /*
                     * Set the repeatBlock instead of the null block or previous repeat block:
                     *
                     * If !isWritableBlock() then all entries in the block have the same value
                     * because it's the null block or a range block (the repeatBlock from a previous
                     * call to utrie2_setRange32()).
                     * No other blocks are used multiple times before compacting.
                     *
                     * The null block is the only non-writable block with the initialValue because
                     * of the repeatBlock initialization above. (If value==initialValue, then
                     * the repeatBlock will be the null data block.)
                     *
                     * We set our repeatBlock if the desired value differs from the block's value,
                     * and if we overwrite any data or if the data is all initial values
                     * (which is the same as the block being the null block, see above).
                     */
                    setRepeatBlock = true;
                }
                if (setRepeatBlock)
                {
                    if (repeatBlock >= 0)
                    {
                        SetIndex2Entry(i2, repeatBlock);
                    }
                    else
                    {
                        /* create and set and fill the repeatBlock */
                        repeatBlock = GetDataBlock(start, true);
                        WriteBlock(repeatBlock, value);
                    }
                }

                start += UTRIE2_DATA_BLOCK_LENGTH;
            }

            if (rest > 0)
            {
                /* set partial block at [last block boundary..limit[ */
                block = GetDataBlock(start, true);
                FillBlock(block, 0, rest, value, initialValue, overwrite);
            }

            return this;
        }

        /// <summary>
        /// Set the values from a <see cref="Trie2.Range"/>.
        /// </summary>
        /// <remarks>
        /// All code points within the range will get the value if
        /// overwrite is TRUE or if the old value is the initial value.
        /// <para/>
        /// Ranges with the lead surrogate flag set will set the alternate
        /// lead-surrogate values in the Trie, rather than the code point values.
        /// <para/>
        /// This function is intended to work with the ranges produced when iterating
        /// the contents of a source Trie.
        /// </remarks>
        /// <param name="range">Contains the range of code points and the value to be set.</param>
        /// <param name="overwrite">Flag for whether old non-initial values are to be overwritten.</param>
        /// <returns>This.</returns>
        public virtual Trie2Writable SetRange(Trie2.Range range, bool overwrite)
        {
            fHash = 0;
            if (range.LeadSurrogate)
            {
                for (int c = range.StartCodePoint; c <= range.EndCodePoint; c++)
                {
                    if (overwrite || GetFromU16SingleLead((char)c) == this.initialValue)
                    {
                        SetForLeadSurrogateCodeUnit((char)c, range.Value);
                    }
                }
            }
            else
            {
                SetRange(range.StartCodePoint, range.EndCodePoint, range.Value, overwrite);
            }
            return this;
        }

        /// <summary>
        /// Set a value for a UTF-16 code unit.
        /// </summary>
        /// <remarks>
        /// Note that a <see cref="Trie2"/> stores separate values for 
        /// supplementary code points in the lead surrogate range
        /// (accessed via the plain <see cref="Set(int, int)"/> and <see cref="Get(int)"/> interfaces)
        /// and for lead surrogate code units.
        /// <para/>
        /// The lead surrogate code unit values are set via this function and
        /// read by the function <see cref="GetFromU16SingleLead(char)"/>
        /// <para/>
        /// For code units outside of the lead surrogate range, this function
        /// behaves identically to <see cref="Set(int, int)"/>.
        /// </remarks>
        /// <param name="codeUnit">A UTF-16 code unit.</param>
        /// <param name="value">The value to be stored in the <see cref="Trie2"/>.</param>
        /// <returns></returns>
        public virtual Trie2Writable SetForLeadSurrogateCodeUnit(char codeUnit, int value)
        {
            fHash = 0;
            Set(codeUnit, false, value);
            return this;
        }

        /// <summary>
        /// Get the value for a code point as stored in the <see cref="Trie2"/>.
        /// </summary>
        /// <param name="codePoint">The code point.</param>
        /// <returns>The value.</returns>
        public override int Get(int codePoint)
        {
            if (codePoint < 0 || codePoint > 0x10ffff)
            {
                return errorValue;
            }
            else
            {
                return Get(codePoint, true);
            }
        }

        private int Get(int c, bool fromLSCP)
        {
            int i2, block;

            if (c >= highStart && (!(c >= 0xd800 && c < 0xdc00) || fromLSCP))
            {
                return data[dataLength - UTRIE2_DATA_GRANULARITY];
            }

            if ((c >= 0xd800 && c < 0xdc00) && fromLSCP)
            {
                i2 = (UTRIE2_LSCP_INDEX_2_OFFSET - (0xd800 >> UTRIE2_SHIFT_2)) +
                    (c >> UTRIE2_SHIFT_2);
            }
            else
            {
                i2 = index1[c >> UTRIE2_SHIFT_1] +
                    ((c >> UTRIE2_SHIFT_2) & UTRIE2_INDEX_2_MASK);
            }
            block = index2[i2];
            return data[block + (c & UTRIE2_DATA_MASK)];
        }

        /// <summary>
        /// Get a trie value for a UTF-16 code unit.
        /// </summary>
        /// <remarks>
        /// This function returns the same value as <see cref="Get(int)"/> if the input 
        /// character is outside of the lead surrogate range.
        /// <para/>
        /// There are two values stored in a Trie for inputs in the lead
        /// surrogate range.  This function returns the alternate value,
        /// while <see cref="Trie2.Get(int)"/> returns the main value.
        /// </remarks>
        /// <param name="c">The code point or lead surrogate value.</param>
        /// <returns>The value.</returns>
        public override int GetFromU16SingleLead(char c)
        {
            return Get(c, false);
        }

        /* compaction --------------------------------------------------------------- */

        private bool EqualInt(int[] a, int s, int t, int length)
        {
            for (int i = 0; i < length; i++)
            {
                if (a[s + i] != a[t + i])
                {
                    return false;
                }
            }
            return true;
        }


        private int FindSameIndex2Block(int index2Length, int otherBlock)
        {
            int block;

            /* ensure that we do not even partially get past index2Length */
            index2Length -= UTRIE2_INDEX_2_BLOCK_LENGTH;

            for (block = 0; block <= index2Length; ++block)
            {
                if (EqualInt(index2, block, otherBlock, UTRIE2_INDEX_2_BLOCK_LENGTH))
                {
                    return block;
                }
            }
            return -1;
        }


        private int FindSameDataBlock(int dataLength, int otherBlock, int blockLength)
        {
            int block;

            /* ensure that we do not even partially get past dataLength */
            dataLength -= blockLength;

            for (block = 0; block <= dataLength; block += UTRIE2_DATA_GRANULARITY)
            {
                if (EqualInt(data, block, otherBlock, blockLength))
                {
                    return block;
                }
            }
            return -1;
        }

        /// <summary>
        /// Find the start of the last range in the trie by enumerating backward.
        /// Indexes for supplementary code points higher than this will be omitted.
        /// </summary>
        private int FindHighStart(int highValue)
        {
            int value;
            int c, prev;
            int i1, i2, j, i2Block, prevI2Block, block, prevBlock;


            /* set variables for previous range */
            if (highValue == initialValue)
            {
                prevI2Block = index2NullOffset;
                prevBlock = dataNullOffset;
            }
            else
            {
                prevI2Block = -1;
                prevBlock = -1;
            }
            prev = 0x110000;

            /* enumerate index-2 blocks */
            i1 = UNEWTRIE2_INDEX_1_LENGTH;
            c = prev;
            while (c > 0)
            {
                i2Block = index1[--i1];
                if (i2Block == prevI2Block)
                {
                    /* the index-2 block is the same as the previous one, and filled with highValue */
                    c -= UTRIE2_CP_PER_INDEX_1_ENTRY;
                    continue;
                }
                prevI2Block = i2Block;
                if (i2Block == index2NullOffset)
                {
                    /* this is the null index-2 block */
                    if (highValue != initialValue)
                    {
                        return c;
                    }
                    c -= UTRIE2_CP_PER_INDEX_1_ENTRY;
                }
                else
                {
                    /* enumerate data blocks for one index-2 block */
                    for (i2 = UTRIE2_INDEX_2_BLOCK_LENGTH; i2 > 0;)
                    {
                        block = index2[i2Block + --i2];
                        if (block == prevBlock)
                        {
                            /* the block is the same as the previous one, and filled with highValue */
                            c -= UTRIE2_DATA_BLOCK_LENGTH;
                            continue;
                        }
                        prevBlock = block;
                        if (block == dataNullOffset)
                        {
                            /* this is the null data block */
                            if (highValue != initialValue)
                            {
                                return c;
                            }
                            c -= UTRIE2_DATA_BLOCK_LENGTH;
                        }
                        else
                        {
                            for (j = UTRIE2_DATA_BLOCK_LENGTH; j > 0;)
                            {
                                value = data[block + --j];
                                if (value != highValue)
                                {
                                    return c;
                                }
                                --c;
                            }
                        }
                    }
                }
            }

            /* deliver last range */
            return 0;
        }

        /// <summary>
        /// Compact a build-time trie.
        /// </summary>
        /// <remarks>
        /// The compaction
        /// <list type="bullet">
        ///     <item><description>removes blocks that are identical with earlier ones</description></item>
        ///     <item><description>overlaps adjacent blocks as much as possible (if overlap==TRUE)</description></item>
        ///     <item><description>moves blocks in steps of the data granularity</description></item>
        ///     <item><description>moves and overlaps blocks that overlap with multiple values in the overlap region</description></item>
        /// </list>
        /// It does not
        /// <list type="bullet">
        ///     <item><description>try to move and overlap blocks that are not already adjacent</description></item>
        /// </list>
        /// </remarks>
        private void CompactData()
        {
            int start, newStart, movedStart;
            int blockLength, overlap;
            int i, mapIndex, blockCount;

            /* do not compact linear-ASCII data */
            newStart = UTRIE2_DATA_START_OFFSET;
            for (start = 0, i = 0; start < newStart; start += UTRIE2_DATA_BLOCK_LENGTH, ++i)
            {
                map[i] = start;
            }

            /*
             * Start with a block length of 64 for 2-byte UTF-8,
             * then switch to UTRIE2_DATA_BLOCK_LENGTH.
             */
            blockLength = 64;
            blockCount = blockLength >> UTRIE2_SHIFT_2;
            for (start = newStart; start < dataLength;)
            {
                /*
                 * start: index of first entry of current block
                 * newStart: index where the current block is to be moved
                 *           (right after current end of already-compacted data)
                 */
                if (start == UNEWTRIE2_DATA_0800_OFFSET)
                {
                    blockLength = UTRIE2_DATA_BLOCK_LENGTH;
                    blockCount = 1;
                }

                /* skip blocks that are not used */
                if (map[start >> UTRIE2_SHIFT_2] <= 0)
                {
                    /* advance start to the next block */
                    start += blockLength;

                    /* leave newStart with the previous block! */
                    continue;
                }

                /* search for an identical block */
                movedStart = FindSameDataBlock(newStart, start, blockLength);
                if (movedStart >= 0)
                {
                    /* found an identical block, set the other block's index value for the current block */
                    for (i = blockCount, mapIndex = start >> UTRIE2_SHIFT_2; i > 0; --i)
                    {
                        map[mapIndex++] = movedStart;
                        movedStart += UTRIE2_DATA_BLOCK_LENGTH;
                    }

                    /* advance start to the next block */
                    start += blockLength;

                    /* leave newStart with the previous block! */
                    continue;
                }

                /* see if the beginning of this block can be overlapped with the end of the previous block */
                /* look for maximum overlap (modulo granularity) with the previous, adjacent block */
                for (overlap = blockLength - UTRIE2_DATA_GRANULARITY;
                    overlap > 0 && !EqualInt(data, (newStart - overlap), start, overlap);
                    overlap -= UTRIE2_DATA_GRANULARITY) { }

                if (overlap > 0 || newStart < start)
                {
                    /* some overlap, or just move the whole block */
                    movedStart = newStart - overlap;
                    for (i = blockCount, mapIndex = start >> UTRIE2_SHIFT_2; i > 0; --i)
                    {
                        map[mapIndex++] = movedStart;
                        movedStart += UTRIE2_DATA_BLOCK_LENGTH;
                    }

                    /* move the non-overlapping indexes to their new positions */
                    start += overlap;
                    for (i = blockLength - overlap; i > 0; --i)
                    {
                        data[newStart++] = data[start++];
                    }
                }
                else /* no overlap && newStart==start */
                {
                    for (i = blockCount, mapIndex = start >> UTRIE2_SHIFT_2; i > 0; --i)
                    {
                        map[mapIndex++] = start;
                        start += UTRIE2_DATA_BLOCK_LENGTH;
                    }
                    newStart = start;
                }
            }

            /* now adjust the index-2 table */
            for (i = 0; i < index2Length; ++i)
            {
                if (i == UNEWTRIE2_INDEX_GAP_OFFSET)
                {
                    /* Gap indexes are invalid (-1). Skip over the gap. */
                    i += UNEWTRIE2_INDEX_GAP_LENGTH;
                }
                index2[i] = map[index2[i] >> UTRIE2_SHIFT_2];
            }
            dataNullOffset = map[dataNullOffset >> UTRIE2_SHIFT_2];

            /* ensure dataLength alignment */
            while ((newStart & (UTRIE2_DATA_GRANULARITY - 1)) != 0)
            {
                data[newStart++] = initialValue;
            }

            if (UTRIE2_DEBUG)
            {
                /* we saved some space */
                Console.Out.WriteLine("compacting UTrie2: count of 32-bit data words {0}->{1}",
                    dataLength, newStart);
            }

            dataLength = newStart;
        }

        private void CompactIndex2()
        {
            int i, start, newStart, movedStart, overlap;

            /* do not compact linear-BMP index-2 blocks */
            newStart = UTRIE2_INDEX_2_BMP_LENGTH;
            for (start = 0, i = 0; start < newStart; start += UTRIE2_INDEX_2_BLOCK_LENGTH, ++i)
            {
                map[i] = start;
            }

            /* Reduce the index table gap to what will be needed at runtime. */
            newStart += UTRIE2_UTF8_2B_INDEX_2_LENGTH + ((highStart - 0x10000) >> UTRIE2_SHIFT_1);

            for (start = UNEWTRIE2_INDEX_2_NULL_OFFSET; start < index2Length;)
            {
                /*
                 * start: index of first entry of current block
                 * newStart: index where the current block is to be moved
                 *           (right after current end of already-compacted data)
                 */

                /* search for an identical block */
                if ((movedStart = FindSameIndex2Block(newStart, start))
                     >= 0
                )
                {
                    /* found an identical block, set the other block's index value for the current block */
                    map[start >> UTRIE2_SHIFT_1_2] = movedStart;

                    /* advance start to the next block */
                    start += UTRIE2_INDEX_2_BLOCK_LENGTH;

                    /* leave newStart with the previous block! */
                    continue;
                }

                /* see if the beginning of this block can be overlapped with the end of the previous block */
                /* look for maximum overlap with the previous, adjacent block */
                for (overlap = UTRIE2_INDEX_2_BLOCK_LENGTH - 1;
                    overlap > 0 && !EqualInt(index2, newStart - overlap, start, overlap);
                    --overlap) { }

                if (overlap > 0 || newStart < start)
                {
                    /* some overlap, or just move the whole block */
                    map[start >> UTRIE2_SHIFT_1_2] = newStart - overlap;

                    /* move the non-overlapping indexes to their new positions */
                    start += overlap;
                    for (i = UTRIE2_INDEX_2_BLOCK_LENGTH - overlap; i > 0; --i)
                    {
                        index2[newStart++] = index2[start++];
                    }
                }
                else /* no overlap && newStart==start */
                {
                    map[start >> UTRIE2_SHIFT_1_2] = start;
                    start += UTRIE2_INDEX_2_BLOCK_LENGTH;
                    newStart = start;
                }
            }

            /* now adjust the index-1 table */
            for (i = 0; i < UNEWTRIE2_INDEX_1_LENGTH; ++i)
            {
                index1[i] = map[index1[i] >> UTRIE2_SHIFT_1_2];
            }
            index2NullOffset = map[index2NullOffset >> UTRIE2_SHIFT_1_2];

            /*
             * Ensure data table alignment:
             * Needs to be granularity-aligned for 16-bit trie
             * (so that dataMove will be down-shiftable),
             * and 2-aligned for uint32_t data.
             */
            while ((newStart & ((UTRIE2_DATA_GRANULARITY - 1) | 1)) != 0)
            {
                /* Arbitrary value: 0x3fffc not possible for real data. */
                index2[newStart++] = 0x0000ffff << UTRIE2_INDEX_SHIFT;
            }

            if (UTRIE2_DEBUG)
            {
                /* we saved some space */
                Console.Out.WriteLine("compacting UTrie2: count of 16-bit index-2 words {0}->{1}",
                        index2Length, newStart);
            }

            index2Length = newStart;
        }

        private void CompactTrie()
        {
            int localHighStart;
            int suppHighStart;
            int highValue;

            /* find highStart and round it up */
            highValue = Get(0x10ffff);
            localHighStart = FindHighStart(highValue);
            localHighStart = (localHighStart + (UTRIE2_CP_PER_INDEX_1_ENTRY - 1)) & ~(UTRIE2_CP_PER_INDEX_1_ENTRY - 1);
            if (localHighStart == 0x110000)
            {
                highValue = errorValue;
            }

            /*
             * Set trie->highStart only after utrie2_get32(trie, highStart).
             * Otherwise utrie2_get32(trie, highStart) would try to read the highValue.
             */
            this.highStart = localHighStart;

            if (UTRIE2_DEBUG)
            {
                Console.Out.WriteLine("UTrie2: highStart U+{0:x4}  highValue 0x{1:x}  initialValue 0x{1:x4}",
                    highStart, highValue, initialValue);
            }

            if (highStart < 0x110000)
            {
                /* Blank out [highStart..10ffff] to release associated data blocks. */
                suppHighStart = highStart <= 0x10000 ? 0x10000 : highStart;
                SetRange(suppHighStart, 0x10ffff, initialValue, true);
            }

            CompactData();
            if (highStart > 0x10000)
            {
                CompactIndex2();
            }
            else
            {
                if (UTRIE2_DEBUG)
                {
                    Console.Out.WriteLine("UTrie2: highStart U+{0:x4}  count of 16-bit index-2 words {1}->{2}",
                            highStart, index2Length, UTRIE2_INDEX_1_OFFSET);
                }
            }

            /*
             * Store the highValue in the data array and round up the dataLength.
             * Must be done after compactData() because that assumes that dataLength
             * is a multiple of UTRIE2_DATA_BLOCK_LENGTH.
             */
            data[dataLength++] = highValue;
            while ((dataLength & (UTRIE2_DATA_GRANULARITY - 1)) != 0)
            {
                data[dataLength++] = initialValue;
            }

            isCompacted = true;
        }

        /// <summary>
        /// Produce an optimized, read-only <see cref="Trie2_16"/> from this writable <see cref="Trie"/>.
        /// The data values outside of the range that will fit in a 16 bit
        /// unsigned value will be truncated.
        /// </summary>
        public virtual Trie2_16 ToTrie2_16()
        {
            Trie2_16 frozenTrie = new Trie2_16();
            Freeze(frozenTrie, ValueWidth.BITS_16);
            return frozenTrie;
        }

        /// <summary>
        /// Produce an optimized, read-only <see cref="Trie2_32"/> from this writable <see cref="Trie"/>.
        /// </summary>
        public virtual Trie2_32 ToTrie2_32()
        {
            Trie2_32 frozenTrie = new Trie2_32();
            Freeze(frozenTrie, ValueWidth.BITS_32);
            return frozenTrie;
        }

        /// <summary>
        /// Maximum length of the runtime index array.
        /// Limited by its own 16-bit index values, and by uint16_t UTrie2Header.indexLength.
        /// (The actual maximum length is lower,
        /// (0x110000>><see cref="Trie2.UTRIE2_SHIFT_2"/>)+<see cref="Trie2.UTRIE2_UTF8_2B_INDEX_2_LENGTH"/>+<see cref="Trie2.UTRIE2_MAX_INDEX_1_LENGTH"/>.)
        /// </summary>  
        private const int UTRIE2_MAX_INDEX_LENGTH = 0xffff;

        /// <summary>
        /// Maximum length of the runtime data array.
        /// Limited by 16-bit index values that are left-shifted by <see cref="Trie2.UTRIE2_INDEX_SHIFT"/>,
        /// and by uint16_t UTrie2Header.shiftedDataLength.
        /// </summary>
        private const int UTRIE2_MAX_DATA_LENGTH = 0xffff << UTRIE2_INDEX_SHIFT;

        /// <summary>Compact the data and then populate an optimized read-only Trie.</summary>
        private void Freeze(Trie2 dest, ValueWidth valueBits)
        {
            int i;
            int allIndexesLength;
            int dataMove;  /* >0 if the data is moved to the end of the index array */


            /* compact if necessary */
            if (!isCompacted)
            {
                CompactTrie();
            }

            if (highStart <= 0x10000)
            {
                allIndexesLength = UTRIE2_INDEX_1_OFFSET;
            }
            else
            {
                allIndexesLength = index2Length;
            }
            if (valueBits == ValueWidth.BITS_16)
            {
                dataMove = allIndexesLength;
            }
            else
            {
                dataMove = 0;
            }

            /* are indexLength and dataLength within limits? */
            if ( /* for unshifted indexLength */
                allIndexesLength > UTRIE2_MAX_INDEX_LENGTH ||
                /* for unshifted dataNullOffset */
                (dataMove + dataNullOffset) > 0xffff ||
                /* for unshifted 2-byte UTF-8 index-2 values */
                (dataMove + UNEWTRIE2_DATA_0800_OFFSET) > 0xffff ||
                /* for shiftedDataLength */
                (dataMove + dataLength) > UTRIE2_MAX_DATA_LENGTH)
            {
                throw new NotSupportedException("Trie2 data is too large.");
            }

            /* calculate the sizes of, and allocate, the index and data arrays */
            int indexLength = allIndexesLength;
            if (valueBits == ValueWidth.BITS_16)
            {
                indexLength += dataLength;
            }
            else
            {
                dest.data32 = new int[dataLength];
            }
            dest.index = new char[indexLength];

            dest.indexLength = allIndexesLength;
            dest.dataLength = dataLength;
            if (highStart <= 0x10000)
            {
                dest.index2NullOffset = 0xffff;
            }
            else
            {
                dest.index2NullOffset = UTRIE2_INDEX_2_OFFSET + index2NullOffset;
            }
            dest.initialValue = initialValue;
            dest.errorValue = errorValue;
            dest.highStart = highStart;
            dest.highValueIndex = dataMove + dataLength - UTRIE2_DATA_GRANULARITY;
            dest.dataNullOffset = (dataMove + dataNullOffset);

            // Create a header and set the its fields.
            //   (This is only used in the event that we serialize the Trie, but is
            //    convenient to do here.)
            dest.header = new Trie2.UTrie2Header();
            dest.header.signature = 0x54726932; /* "Tri2" */
            dest.header.options = valueBits == ValueWidth.BITS_16 ? 0 : 1;
            dest.header.indexLength = dest.indexLength;
            dest.header.shiftedDataLength = dest.dataLength >> UTRIE2_INDEX_SHIFT;
            dest.header.index2NullOffset = dest.index2NullOffset;
            dest.header.dataNullOffset = dest.dataNullOffset;
            dest.header.shiftedHighStart = dest.highStart >> UTRIE2_SHIFT_1;



            /* write the index-2 array values shifted right by UTRIE2_INDEX_SHIFT, after adding dataMove */
            int destIdx = 0;
            for (i = 0; i < UTRIE2_INDEX_2_BMP_LENGTH; i++)
            {
                dest.index[destIdx++] = (char)((index2[i] + dataMove) >> UTRIE2_INDEX_SHIFT);
            }
            if (UTRIE2_DEBUG)
            {
                Console.Out.WriteLine("\n\nIndex2 for BMP limit is " + string.Format("{0:x4}", destIdx));
            }

            /* write UTF-8 2-byte index-2 values, not right-shifted */
            for (i = 0; i < (0xc2 - 0xc0); ++i)
            {                                  /* C0..C1 */
                dest.index[destIdx++] = (char)(dataMove + UTRIE2_BAD_UTF8_DATA_OFFSET);
            }
            for (; i < (0xe0 - 0xc0); ++i)
            {                                     /* C2..DF */
                dest.index[destIdx++] = (char)(dataMove + index2[i << (6 - UTRIE2_SHIFT_2)]);
            }
            if (UTRIE2_DEBUG)
            {
                Console.Out.WriteLine("Index2 for UTF-8 2byte values limit is " + string.Format("{0:x4}", destIdx));
            }

            if (highStart > 0x10000)
            {
                int index1Length = (highStart - 0x10000) >> UTRIE2_SHIFT_1;
                int index2Offset = UTRIE2_INDEX_2_BMP_LENGTH + UTRIE2_UTF8_2B_INDEX_2_LENGTH + index1Length;

                /* write 16-bit index-1 values for supplementary code points */
                //p=(uint32_t *)newTrie->index1+UTRIE2_OMITTED_BMP_INDEX_1_LENGTH;
                for (i = 0; i < index1Length; i++)
                {
                    //*dest16++=(uint16_t)(UTRIE2_INDEX_2_OFFSET + *p++);
                    dest.index[destIdx++] = (char)(UTRIE2_INDEX_2_OFFSET + index1[i + UTRIE2_OMITTED_BMP_INDEX_1_LENGTH]);
                }
                if (UTRIE2_DEBUG)
                {
                    Console.Out.WriteLine("Index 1 for supplementals, limit is " + string.Format("{0:x4}", destIdx));
                }

                /*
                 * write the index-2 array values for supplementary code points,
                 * shifted right by UTRIE2_INDEX_SHIFT, after adding dataMove
                 */
                for (i = 0; i < index2Length - index2Offset; i++)
                {
                    dest.index[destIdx++] = (char)((dataMove + index2[index2Offset + i]) >> UTRIE2_INDEX_SHIFT);
                }
                if (UTRIE2_DEBUG)
                {
                    Console.Out.WriteLine("Index 2 for supplementals, limit is " + string.Format("{0:x4}", destIdx));
                }
            }

            /* write the 16/32-bit data array */
            switch (valueBits)
            {
                case ValueWidth.BITS_16:
                    /* write 16-bit data values */
                    Debug.Assert(destIdx == dataMove);
                    dest.data16 = destIdx;
                    for (i = 0; i < dataLength; i++)
                    {
                        dest.index[destIdx++] = (char)data[i];
                    }
                    break;
                case ValueWidth.BITS_32:
                    /* write 32-bit data values */
                    for (i = 0; i < dataLength; i++)
                    {
                        dest.data32[i] = this.data[i];
                    }
                    break;
            }
            // The writable, but compressed, Trie2 stays around unless the caller drops its references to it.
        }


        /// <summary>Start with allocation of 16k data entries.</summary>
        private const int UNEWTRIE2_INITIAL_DATA_LENGTH = 1 << 14;

        /// <summary>Grow about 8x each time.</summary>
        private const int UNEWTRIE2_MEDIUM_DATA_LENGTH = 1 << 17;

        /// <summary>The null index-2 block, following the gap in the index-2 table.</summary>
        private const int UNEWTRIE2_INDEX_2_NULL_OFFSET = UNEWTRIE2_INDEX_GAP_OFFSET + UNEWTRIE2_INDEX_GAP_LENGTH;

        /// <summary>The start of allocated index-2 blocks.</summary>
        private const int UNEWTRIE2_INDEX_2_START_OFFSET = UNEWTRIE2_INDEX_2_NULL_OFFSET + UTRIE2_INDEX_2_BLOCK_LENGTH;

        /// <summary>
        /// The null data block.
        /// Length 64=0x40 even if <see cref="Trie2.UTRIE2_DATA_BLOCK_LENGTH"/> is smaller,
        /// to work with 6-bit trail bytes from 2-byte UTF-8.
        /// </summary>
        private const int UNEWTRIE2_DATA_NULL_OFFSET = UTRIE2_DATA_START_OFFSET;

        /// <summary>The start of allocated data blocks.</summary>
        private const int UNEWTRIE2_DATA_START_OFFSET = UNEWTRIE2_DATA_NULL_OFFSET + 0x40;

        /// <summary>
        /// The start of data blocks for U+0800 and above.
        /// Below, compaction uses a block length of 64 for 2-byte UTF-8.
        /// From here on, compaction uses <see cref="Trie2.UTRIE2_DATA_BLOCK_LENGTH"/>.
        /// Data values for 0x780 code points beyond ASCII.
        /// </summary>
        private const int UNEWTRIE2_DATA_0800_OFFSET = UNEWTRIE2_DATA_START_OFFSET + 0x780;

        //
        // Private data members.  From struct UNewTrie2 in ICU4C
        //
        private int[] index1 = new int[UNEWTRIE2_INDEX_1_LENGTH];
        private int[] index2 = new int[UNEWTRIE2_MAX_INDEX_2_LENGTH];
        private int[] data;

        private int index2Length;
        private int dataCapacity;
        private int firstFreeBlock;
        new private int index2NullOffset; // ICU4N TODO: Check this out - why are we overriding here?
        private bool isCompacted;

        /// <summary>
        /// Multi-purpose per-data-block table.
        /// </summary>
        /// <remarks>
        /// Before compacting:
        /// <para/>
        /// Per-data-block reference counters/free-block list.
        /// <list type="table">
        ///     <item><term>0</term><description>unused</description></item>
        ///     <item><term>>0</term><description>reference counter (number of index-2 entries pointing here)</description></item>
        ///     <item><term>&lt;0</term><description>next free data block in free-block list</description></item>
        /// </list>
        /// <para/>
        /// While compacting:
        /// <list type="bullet">
        ///     <item><description>Map of adjusted indexes, used in <see cref="CompactData()"/> and <see cref="CompactIndex2()"/>.</description></item>
        ///     <item><description>Maps from original indexes to new ones.</description></item>
        /// </list>
        /// </remarks>
        private int[] map = new int[UNEWTRIE2_MAX_DATA_LENGTH >> UTRIE2_SHIFT_2];

        private bool UTRIE2_DEBUG = false;
    }
}
