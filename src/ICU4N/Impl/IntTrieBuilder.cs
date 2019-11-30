using ICU4N.Globalization;
using ICU4N.Support;
using ICU4N.Support.Collections;
using ICU4N.Text;
using J2N.IO;
using System;
using System.IO;

namespace ICU4N.Impl
{
    /// <summary>
    /// Builder class to manipulate and generate a trie.
    /// </summary>
    /// <remarks>
    /// This is useful for ICU data in primitive types.
    /// Provides a compact way to store information that is indexed by Unicode 
    /// values, such as character properties, types, keyboard values, etc. This is 
    /// very useful when you have a block of Unicode data that contains significant 
    /// values while the rest of the Unicode data is unused in the application or 
    /// when you have a lot of redundance, such as where all 21,000 Han ideographs 
    /// have the same value.  However, lookup is much faster than a hash table.
    /// A trie of any primitive data type serves two purposes:
    /// <list type="bullet">
    ///     <item><description>Fast access of the indexed values.</description></item>
    ///     <item><description>Smaller memory footprint.</description></item>
    /// </list>
    /// This is a direct port from the ICU4C version
    /// </remarks>
    /// <author>Syn Wee Quek</author>
    public class Int32TrieBuilder : TrieBuilder
    {
        // public constructor ----------------------------------------------

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public Int32TrieBuilder(Int32TrieBuilder table)
            : base(table)
        {
            m_data_ = new int[m_dataCapacity_];
            System.Array.Copy(table.m_data_, 0, m_data_, 0, m_dataLength_);
            m_initialValue_ = table.m_initialValue_;
            m_leadUnitValue_ = table.m_leadUnitValue_;
        }

        /// <summary>
        /// Constructs a build table.
        /// </summary>
        /// <param name="aliasdata">Data to be filled into table.</param>
        /// <param name="maxdatalength">Maximum data length allowed in table.</param>
        /// <param name="initialvalue">Inital data value.</param>
        /// <param name="leadunitvalue"></param>
        /// <param name="latin1linear">Is latin 1 to be linear.</param>
        public Int32TrieBuilder(int[] aliasdata, int maxdatalength,
                              int initialvalue, int leadunitvalue,
                              bool latin1linear)
            : base()
        {
            if (maxdatalength < DataBlockLength || (latin1linear
                                                      && maxdatalength < 1024))
            {
                throw new ArgumentException(
                                                   "Argument maxdatalength is too small");
            }

            if (aliasdata != null)
            {
                m_data_ = aliasdata;
            }
            else
            {
                m_data_ = new int[maxdatalength];
            }

            // preallocate and reset the first data block (block index 0)
            int j = DataBlockLength;

            if (latin1linear)
            {
                // preallocate and reset the first block (number 0) and Latin-1 
                // (U+0000..U+00ff) after that made sure above that 
                // maxDataLength >= 1024
                // set indexes to point to consecutive data blocks
                int i = 0;
                do
                {
                    // do this at least for trie->index[0] even if that block is 
                    // only partly used for Latin-1
                    m_index_[i++] = j;
                    j += DataBlockLength;
                } while (i < (256 >> Shift));
            }

            m_dataLength_ = j;
            // reset the initially allocated blocks to the initial value
            Arrays.Fill(m_data_, 0, m_dataLength_, initialvalue);
            m_initialValue_ = initialvalue;
            m_leadUnitValue_ = leadunitvalue;
            m_dataCapacity_ = maxdatalength;
            m_isLatin1Linear_ = latin1linear;
            m_isCompacted_ = false;
        }

        // public methods -------------------------------------------------------

        /*public final void print()
          {
          int i = 0;
          int oldvalue = m_index_[i];
          int count = 0;
          System.out.println("index length " + m_indexLength_ 
          + " --------------------------");
          while (i < m_indexLength_) {
          if (m_index_[i] != oldvalue) {
          System.out.println("index has " + count + " counts of " 
          + Integer.toHexString(oldvalue));
          count = 0;
          oldvalue = m_index_[i];
          }
          count ++;
          i ++;
          }
          System.out.println("index has " + count + " counts of " 
          + Integer.toHexString(oldvalue));
          i = 0;
          oldvalue = m_data_[i];
          count = 0;
          System.out.println("data length " + m_dataLength_ 
          + " --------------------------");
          while (i < m_dataLength_) {
          if (m_data_[i] != oldvalue) {
          if ((oldvalue & 0xf1000000) == 0xf1000000) {
          int temp = oldvalue & 0xffffff; 
          temp += 0x320;
          oldvalue = 0xf1000000 | temp;
          }
          if ((oldvalue & 0xf2000000) == 0xf2000000) {
          int temp = oldvalue & 0xffffff; 
          temp += 0x14a;
          oldvalue = 0xf2000000 | temp;
          }
          System.out.println("data has " + count + " counts of " 
          + Integer.toHexString(oldvalue));
          count = 0;
          oldvalue = m_data_[i];
          }
          count ++;
          i ++;
          }
          if ((oldvalue & 0xf1000000) == 0xf1000000) {
          int temp = oldvalue & 0xffffff; 
          temp += 0x320;
          oldvalue = 0xf1000000 | temp;
          }
          if ((oldvalue & 0xf2000000) == 0xf2000000) {
          int temp = oldvalue & 0xffffff; 
          temp += 0x14a;
          oldvalue = 0xf2000000 | temp;
          }
          System.out.println("data has " + count + " counts of " 
          + Integer.toHexString(oldvalue));
          }
        */

        /// <summary>
        /// Gets a 32 bit data from the table data.
        /// </summary>
        /// <param name="ch">Codepoint which data is to be retrieved.</param>
        /// <returns>The 32 bit data.</returns>
        public virtual int GetValue(int ch)
        {
            // valid, uncompacted trie and valid c?
            if (m_isCompacted_ || ch > UChar.MaxValue || ch < 0)
            {
                return 0;
            }

            int block = m_index_[ch >> Shift];
            return m_data_[Math.Abs(block) + (ch & Mask)];
        }

        /// <summary>
        /// Get a 32 bit data from the table data.
        /// </summary>
        /// <param name="ch">Code point for which data is to be retrieved.</param>
        /// <param name="inBlockZero">Output parameter, inBlockZero[0] returns true if the
        /// char maps into block zero, otherwise false.</param>
        /// <returns>The 32 bit data value.</returns>
        public virtual int GetValue(int ch, bool[] inBlockZero)
        {
            // valid, uncompacted trie and valid c?
            if (m_isCompacted_ || ch > UChar.MaxValue || ch < 0)
            {
                if (inBlockZero != null)
                {
                    inBlockZero[0] = true;
                }
                return 0;
            }

            int block = m_index_[ch >> Shift];
            if (inBlockZero != null)
            {
                inBlockZero[0] = (block == 0);
            }
            return m_data_[Math.Abs(block) + (ch & Mask)];
        }

        /// <summary>
        /// Sets a 32 bit data in the table data.
        /// </summary>
        /// <param name="ch">Codepoint which data is to be set.</param>
        /// <param name="value">Value to set.</param>
        /// <returns>true if the set is successful, otherwise
        /// if the table has been compacted return false</returns>
        public virtual bool SetValue(int ch, int value)
        {
            // valid, uncompacted trie and valid c? 
            if (m_isCompacted_ || ch > UChar.MaxValue || ch < 0)
            {
                return false;
            }

            int block = GetDataBlock(ch);
            if (block < 0)
            {
                return false;
            }

            m_data_[block + (ch & Mask)] = value;
            return true;
        }

        /// <summary>
        /// Serializes the build table with 32 bit data.
        /// </summary>
        /// <param name="datamanipulate">Builder raw fold method implementation.</param>
        /// <param name="triedatamanipulate">Result trie fold method.</param>
        /// <returns>A new trie.</returns>
        public virtual Int32Trie Serialize(ITrieBuilderDataManipulate datamanipulate,
                                 ITrieDataManipulate triedatamanipulate)
        {
            if (datamanipulate == null)
            {
                throw new ArgumentException("Parameters can not be null");
            }
            // fold and compact if necessary, also checks that indexLength is 
            // within limits 
            if (!m_isCompacted_)
            {
                // compact once without overlap to improve folding
                Compact(false);
                // fold the supplementary part of the index array
                Fold(datamanipulate);
                // compact again with overlap for minimum data array length
                Compact(true);
                m_isCompacted_ = true;
            }
            // is dataLength within limits? 
            if (m_dataLength_ >= MaxDataLength)
            {
                throw new IndexOutOfRangeException("Data length too small");
            }

            char[] index = new char[m_indexLength_];
            int[] data = new int[m_dataLength_];
            // write the index (stage 1) array and the 32-bit data (stage 2) array
            // write 16-bit index values shifted right by INDEX_SHIFT_ 
            for (int i = 0; i < m_indexLength_; i++)
            {
                index[i] = (char)(m_index_[i].TripleShift(IndexShift));
            }
            // write 32-bit data values
            System.Array.Copy(m_data_, 0, data, 0, m_dataLength_);

            int options = Shift | (IndexShift << OptionsIndexShift);
            options |= OptionsDataIs32Bit;
            if (m_isLatin1Linear_)
            {
                options |= OptionsLatin1IsLinear;
            }
            return new Int32Trie(index, data, m_initialValue_, options,
                               triedatamanipulate);
        }

        /// <summary>
        /// Serializes the build table to an output stream.
        /// <para/>
        /// Compacts the build-time trie after all values are set, and then
        /// writes the serialized form onto an output stream.
        /// <para/>
        /// After this, this build-time Trie can only be serialized again and/or closed;
        /// no further values can be added.
        /// <para/>
        /// This function is the rough equivalent of utrie_seriaize() in ICU4C.
        /// </summary>
        /// <param name="os">The output stream to which the seriaized trie will be written.
        /// If nul, the function still returns the size of the serialized Trie.</param>
        /// <param name="reduceTo16Bits">If true, reduce the data size to 16 bits.  The resulting
        /// serialized form can then be used to create a <see cref="CharTrie"/>.</param>
        /// <param name="datamanipulate">Builder raw fold method implementation.</param>
        /// <returns>The number of bytes written to the output stream.</returns>
        public virtual int Serialize(Stream os, bool reduceTo16Bits,
            ITrieBuilderDataManipulate datamanipulate)
        {
            if (datamanipulate == null)
            {
                throw new ArgumentException("Parameters can not be null");
            }

            // fold and compact if necessary, also checks that indexLength is 
            // within limits 
            if (!m_isCompacted_)
            {
                // compact once without overlap to improve folding
                Compact(false);
                // fold the supplementary part of the index array
                Fold(datamanipulate);
                // compact again with overlap for minimum data array length
                Compact(true);
                m_isCompacted_ = true;
            }

            // is dataLength within limits? 
            int length;
            if (reduceTo16Bits)
            {
                length = m_dataLength_ + m_indexLength_;
            }
            else
            {
                length = m_dataLength_;
            }
            if (length >= MaxDataLength)
            {
                throw new IndexOutOfRangeException("Data length too small");
            }

            //  struct UTrieHeader {
            //      int32_t   signature;
            //      int32_t   options  (a bit field)
            //      int32_t   indexLength
            //      int32_t   dataLength
            length = Trie.HeaderLength + 2 * m_indexLength_;
            if (reduceTo16Bits)
            {
                length += 2 * m_dataLength_;
            }
            else
            {
                length += 4 * m_dataLength_;
            }

            if (os == null)
            {
                // No output stream.  Just return the length of the serialized Trie, in bytes.
                return length;
            }

            DataOutputStream dos = new DataOutputStream(os);
            dos.WriteInt32(Trie.HeaderSignature);

            int options = Trie.IndexStage1Shift | (Trie.IndexStage2Shift << Trie.HeaderOptionsIndexShift);
            if (!reduceTo16Bits)
            {
                options |= Trie.HeaderOptionsDataIs32Bit;
            }
            if (m_isLatin1Linear_)
            {
                options |= Trie.HeaderOptionsLatin1IsLinearMask;
            }
            dos.WriteInt32(options);

            dos.WriteInt32(m_indexLength_);
            dos.WriteInt32(m_dataLength_);

            /* write the index (stage 1) array and the 16/32-bit data (stage 2) array */
            if (reduceTo16Bits)
            {
                /* write 16-bit index values shifted right by UTRIE_INDEX_SHIFT, after adding indexLength */
                for (int i = 0; i < m_indexLength_; i++)
                {
                    int v = (m_index_[i] + m_indexLength_).TripleShift(Trie.IndexStage2Shift);
                    dos.WriteChar(v);
                }

                /* write 16-bit data values */
                for (int i = 0; i < m_dataLength_; i++)
                {
                    int v = m_data_[i] & 0x0000ffff;
                    dos.WriteChar(v);
                }
            }
            else
            {
                /* write 16-bit index values shifted right by UTRIE_INDEX_SHIFT */
                for (int i = 0; i < m_indexLength_; i++)
                {
                    int v = (m_index_[i]).TripleShift(Trie.IndexStage2Shift);
                    dos.WriteChar(v);
                }

                /* write 32-bit data values */
                for (int i = 0; i < m_dataLength_; i++)
                {
                    dos.WriteInt32(m_data_[i]);
                }
            }

            return length;

        }

        /// <summary>
        /// Set a value in a range of code points [start..limit].
        /// All code points c with start &lt;= c &lt; limit will get the value if
        /// overwrite is true or if the old value is 0.
        /// </summary>
        /// <param name="start">The first code point to get the value.</param>
        /// <param name="limit">One past the last code point to get the value.</param>
        /// <param name="value">The value.</param>
        /// <param name="overwrite">Flag for whether old non-initial values are to be overwritten.</param>
        /// <returns>false if a failure occurred (illegal argument or data array overrun).</returns>
        public virtual bool SetRange(int start, int limit, int value,
                                bool overwrite)
        {
            // repeat value in [start..limit[
            // mark index values for repeat-data blocks by setting bit 31 of the 
            // index values fill around existing values if any, if(overwrite)

            // valid, uncompacted trie and valid indexes?
            if (m_isCompacted_ || start < UChar.MinValue
                || start > UChar.MaxValue || limit < UChar.MinValue
                || limit > (UChar.MaxValue + 1) || start > limit)
            {
                return false;
            }

            if (start == limit)
            {
                return true; // nothing to do
            }

            if ((start & Mask) != 0)
            {
                // set partial block at [start..following block boundary[
                int block = GetDataBlock(start);
                if (block < 0)
                {
                    return false;
                }

                int nextStart = (start + DataBlockLength) & ~Mask;
                if (nextStart <= limit)
                {
                    FillBlock(block, start & Mask, DataBlockLength,
                              value, overwrite);
                    start = nextStart;
                }
                else
                {
                    FillBlock(block, start & Mask, limit & Mask,
                              value, overwrite);
                    return true;
                }
            }

            // number of positions in the last, partial block
            int rest = limit & Mask;

            // round down limit to a block boundary 
            limit &= ~Mask;

            // iterate over all-value blocks 
            int repeatBlock = 0;
            if (value == m_initialValue_)
            {
                // repeatBlock = 0; assigned above
            }
            else
            {
                repeatBlock = -1;
            }
            while (start < limit)
            {
                // get index value 
                int block = m_index_[start >> Shift];
                if (block > 0)
                {
                    // already allocated, fill in value
                    FillBlock(block, 0, DataBlockLength, value, overwrite);
                }
                else if (m_data_[-block] != value && (block == 0 || overwrite))
                {
                    // set the repeatBlock instead of the current block 0 or range 
                    // block 
                    if (repeatBlock >= 0)
                    {
                        m_index_[start >> Shift] = -repeatBlock;
                    }
                    else
                    {
                        // create and set and fill the repeatBlock
                        repeatBlock = GetDataBlock(start);
                        if (repeatBlock < 0)
                        {
                            return false;
                        }

                        // set the negative block number to indicate that it is a 
                        // repeat block
                        m_index_[start >> Shift] = -repeatBlock;
                        FillBlock(repeatBlock, 0, DataBlockLength, value, true);
                    }
                }

                start += DataBlockLength;
            }

            if (rest > 0)
            {
                // set partial block at [last block boundary..limit[
                int block = GetDataBlock(start);
                if (block < 0)
                {
                    return false;
                }
                FillBlock(block, 0, rest, value, overwrite);
            }

            return true;
        }

        // protected data member ------------------------------------------------

        protected int[] m_data_;
        protected int m_initialValue_;

        //  private data member ------------------------------------------------

        private int m_leadUnitValue_;

        // private methods ------------------------------------------------------

        private int AllocDataBlock()
        {
            int newBlock = m_dataLength_;
            int newTop = newBlock + DataBlockLength;
            if (newTop > m_dataCapacity_)
            {
                // out of memory in the data array
                return -1;
            }
            m_dataLength_ = newTop;
            return newBlock;
        }

        /// <summary>
        /// No error checking for illegal arguments.
        /// </summary>
        /// <param name="ch">codepoint to look for</param>
        /// <returns>-1 if no new data block available (out of memory in data array).</returns>
        private int GetDataBlock(int ch)
        {
            ch >>= Shift;
            int indexValue = m_index_[ch];
            if (indexValue > 0)
            {
                return indexValue;
            }

            // allocate a new data block
            int newBlock = AllocDataBlock();
            if (newBlock < 0)
            {
                // out of memory in the data array 
                return -1;
            }
            m_index_[ch] = newBlock;

            // copy-on-write for a block from a setRange()
            System.Array.Copy(m_data_, Math.Abs(indexValue), m_data_, newBlock,
                             DataBlockLength << 2);
            return newBlock;
        }

        /// <summary>
        /// Compact a folded build-time trie.
        /// The compaction
        /// <list type="bullet">
        ///     <item><description>removes blocks that are identical with earlier ones</description></item>
        ///     <item><description>overlaps adjacent blocks as much as possible (if overlap == true)</description></item>
        ///     <item><description>moves blocks in steps of the data granularity</description></item>
        ///     <item><description>moves and overlaps blocks that overlap with multiple values in the overlap region</description></item>
        /// </list>
        /// It does not
        /// <list type="bullet">
        ///     <item><description>try to move and overlap blocks that are not already adjacent</description></item>
        /// </list>
        /// </summary>
        /// <param name="overlap">flag</param>
        private void Compact(bool overlap)
        {
            if (m_isCompacted_)
            {
                return; // nothing left to do
            }

            // compaction
            // initialize the index map with "block is used/unused" flags
            FindUnusedBlocks();

            // if Latin-1 is preallocated and linear, then do not compact Latin-1 
            // data
            int overlapStart = DataBlockLength;
            if (m_isLatin1Linear_ && Shift <= 8)
            {
                overlapStart += 256;
            }

            int newStart = DataBlockLength;
            int i;
            for (int start = newStart; start < m_dataLength_;)
            {
                // start: index of first entry of current block
                // newStart: index where the current block is to be moved
                //           (right after current end of already-compacted data)
                // skip blocks that are not used 
                if (m_map_[start.TripleShift(Shift)] < 0)
                {
                    // advance start to the next block 
                    start += DataBlockLength;
                    // leave newStart with the previous block!
                    continue;
                }
                // search for an identical block
                if (start >= overlapStart)
                {
                    i = FindSameDataBlock(m_data_, newStart, start,
                                              overlap ? DataGranularity : DataBlockLength);
                    if (i >= 0)
                    {
                        // found an identical block, set the other block's index 
                        // value for the current block
                        m_map_[start.TripleShift(Shift)] = i;
                        // advance start to the next block
                        start += DataBlockLength;
                        // leave newStart with the previous block!
                        continue;
                    }
                }
                // see if the beginning of this block can be overlapped with the 
                // end of the previous block
                if (overlap && start >= overlapStart)
                {
                    /* look for maximum overlap (modulo granularity) with the previous, adjacent block */
                    for (i = DataBlockLength - DataGranularity;
                        i > 0 && !EqualInt32(m_data_, newStart - i, start, i);
                        i -= DataGranularity) { }
                }
                else
                {
                    i = 0;
                }
                if (i > 0)
                {
                    // some overlap
                    m_map_[start.TripleShift(Shift)] = newStart - i;
                    // move the non-overlapping indexes to their new positions
                    start += i;
                    for (i = DataBlockLength - i; i > 0; --i)
                    {
                        m_data_[newStart++] = m_data_[start++];
                    }
                }
                else if (newStart < start)
                {
                    // no overlap, just move the indexes to their new positions
                    m_map_[start.TripleShift(Shift)] = newStart;
                    for (i = DataBlockLength; i > 0; --i)
                    {
                        m_data_[newStart++] = m_data_[start++];
                    }
                }
                else
                { // no overlap && newStart==start
                    m_map_[start.TripleShift(Shift)] = start;
                    newStart += DataBlockLength;
                    start = newStart;
                }
            }
            // now adjust the index (stage 1) table
            for (i = 0; i < m_indexLength_; ++i)
            {
                m_index_[i] = m_map_[Math.Abs(m_index_[i]).TripleShift(Shift)];
            }
            m_dataLength_ = newStart;
        }

        /// <summary>
        /// Find the same data block.
        /// </summary>
        /// <param name="data">array</param>
        /// <param name="dataLength"></param>
        /// <param name="otherBlock"></param>
        /// <param name="step"></param>
        private static int FindSameDataBlock(int[] data, int dataLength,
                                                   int otherBlock, int step)
        {
            // ensure that we do not even partially get past dataLength
            dataLength -= DataBlockLength;

            for (int block = 0; block <= dataLength; block += step)
            {
                if (EqualInt32(data, block, otherBlock, DataBlockLength))
                {
                    return block;
                }
            }
            return -1;
        }

        /// <summary>
        /// Fold the normalization data for supplementary code points into
        /// a compact area on top of the BMP-part of the trie index,
        /// with the lead surrogates indexing this compact area.
        /// <para/>
        /// Duplicate the index values for lead surrogates:
        /// From inside the BMP area, where some may be overridden with folded values,
        /// to just after the BMP area, where they can be retrieved for
        /// code point lookups.
        /// </summary>
        /// <param name="manipulate">Fold implementation.</param>
        private void Fold(ITrieBuilderDataManipulate manipulate)
        {
            int[] leadIndexes = new int[SurrogateBlockCount];
            int[] index = m_index_;
            // copy the lead surrogate indexes into a temporary array
            System.Array.Copy(index, 0xd800 >> Shift, leadIndexes, 0,
                             SurrogateBlockCount);

            // set all values for lead surrogate code *units* to leadUnitValue
            // so that by default runtime lookups will find no data for associated
            // supplementary code points, unless there is data for such code points
            // which will result in a non-zero folding value below that is set for
            // the respective lead units
            // the above saved the indexes for surrogate code *points*
            // fill the indexes with simplified code from utrie_setRange32()
            int block = 0;
            if (m_leadUnitValue_ == m_initialValue_)
            {
                // leadUnitValue == initialValue, use all-initial-value block
                // block = 0; if block here left empty
            }
            else
            {
                // create and fill the repeatBlock
                block = AllocDataBlock();
                if (block < 0)
                {
                    // data table overflow
                    throw new InvalidOperationException("Internal error: Out of memory space");
                }
                FillBlock(block, 0, DataBlockLength, m_leadUnitValue_, true);
                // negative block number to indicate that it is a repeat block
                block = -block;
            }
            for (int c = (0xd800 >> Shift); c < (0xdc00 >> Shift); ++c)
            {
                m_index_[c] = block;
            }

            // Fold significant index values into the area just after the BMP 
            // indexes.
            // In case the first lead surrogate has significant data,
            // its index block must be used first (in which case the folding is a 
            // no-op).
            // Later all folded index blocks are moved up one to insert the copied
            // lead surrogate indexes.
            int indexLength = BMPIndexLength;
            // search for any index (stage 1) entries for supplementary code points 
            for (int c = 0x10000; c < 0x110000;)
            {
                if (index[c >> Shift] != 0)
                {
                    // there is data, treat the full block for a lead surrogate
                    c &= ~0x3ff;
                    // is there an identical index block?
                    block = FindSameIndexBlock(index, indexLength, c >> Shift);

                    // get a folded value for [c..c+0x400[ and,
                    // if different from the value for the lead surrogate code 
                    // point, set it for the lead surrogate code unit

                    int value = manipulate.GetFoldedValue(c,
                                                          block + SurrogateBlockCount);
                    if (value != GetValue(UTF16.GetLeadSurrogate(c)))
                    {
                        if (!SetValue(UTF16.GetLeadSurrogate(c), value))
                        {
                            // data table overflow 
                            throw new IndexOutOfRangeException(
                                                                     "Data table overflow");
                        }
                        // if we did not find an identical index block...
                        if (block == indexLength)
                        {
                            // move the actual index (stage 1) entries from the 
                            // supplementary position to the new one
                            System.Array.Copy(index, c >> Shift, index, indexLength,
                                     SurrogateBlockCount);
                            indexLength += SurrogateBlockCount;
                        }
                    }
                    c += 0x400;
                }
                else
                {
                    c += DataBlockLength;
                }
            }

            // index array overflow?
            // This is to guarantee that a folding offset is of the form
            // UTRIE_BMP_INDEX_LENGTH+n*UTRIE_SURROGATE_BLOCK_COUNT with n=0..1023.
            // If the index is too large, then n>=1024 and more than 10 bits are 
            // necessary.
            // In fact, it can only ever become n==1024 with completely unfoldable 
            // data and the additional block of duplicated values for lead 
            // surrogates.
            if (indexLength >= MaxIndexLength)
            {
                throw new IndexOutOfRangeException("Index table overflow");
            }
            // make space for the lead surrogate index block and insert it between 
            // the BMP indexes and the folded ones
            System.Array.Copy(index, BMPIndexLength, index,
                     BMPIndexLength + SurrogateBlockCount,
                     indexLength - BMPIndexLength);
            System.Array.Copy(leadIndexes, 0, index, BMPIndexLength,
                             SurrogateBlockCount);
            indexLength += SurrogateBlockCount;
            m_indexLength_ = indexLength;
        }

        private void FillBlock(int block, int start, int limit, int value,
                               bool overwrite)
        {
            limit += block;
            block += start;
            if (overwrite)
            {
                while (block < limit)
                {
                    m_data_[block++] = value;
                }
            }
            else
            {
                while (block < limit)
                {
                    if (m_data_[block] == m_initialValue_)
                    {
                        m_data_[block] = value;
                    }
                    ++block;
                }
            }
        }
    }
}
