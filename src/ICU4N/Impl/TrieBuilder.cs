using ICU4N.Globalization;
using ICU4N.Support.Collections;
using System;

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
    public class TrieBuilder
    {
        // public data member ----------------------------------------------

        /// <summary>
        /// Number of data values in a stage 2 (data array) block. 2, 4, 8, .., 
        /// 0x200
        /// </summary>
        public static readonly int DATA_BLOCK_LENGTH = 1 << Trie.INDEX_STAGE_1_SHIFT_;

        // public class declaration ----------------------------------------

        /// <summary>
        /// Character data in <see cref="Trie"/> have different user-specified format
        /// for different purposes.
        /// This interface specifies methods to be implemented in order for
        /// <see cref="Trie"/>, to surrogate offset information encapsulated within 
        /// the data.
        /// </summary>
        public interface IDataManipulate
        {
            /// <summary>
            /// Build-time trie callback function, used with serialize().
            /// This function calculates a lead surrogate's value including a
            /// folding offset from the 1024 supplementary code points 
            /// [start..start+1024[ .
            /// It is U+10000 &lt;= start &lt;= U+10fc00 and (start&amp;0x3ff)==0.
            /// The folding offset is provided by the caller. 
            /// It is offset=UTRIE_BMP_INDEX_LENGTH+n*UTRIE_SURROGATE_BLOCK_COUNT 
            /// with n=0..1023. 
            /// Instead of the offset itself, n can be stored in 10 bits - or fewer 
            /// if it can be assumed that few lead surrogates have associated data.
            /// The returned value must be
            /// <list type="bullet">
            ///     <item><description>not zero if and only if there is relevant data for the
            ///     corresponding 1024 supplementary code points</description></item>
            ///     <item><description>such that UTrie.GetFoldingOffset(UNewTrieGetFoldedValue(..., offset))==offset</description></item>
            /// </list>
            /// </summary>
            /// <returns>a folded value, or 0 if there is no relevant data for the
            /// lead surrogate.</returns>
            int GetFoldedValue(int start, int offset);
        }

        // public methods ----------------------------------------------------

        /// <summary>
        /// Checks if the character belongs to a zero block in the trie.
        /// </summary>
        /// <param name="ch">Codepoint which data is to be retrieved.</param>
        /// <returns>true if ch is in the zero block.</returns>
        public virtual bool IsInZeroBlock(int ch)
        {
            // valid, uncompacted trie and valid c?
            if (m_isCompacted_ || ch > UChar.MaxValue
                || ch < UChar.MinValue)
            {
                return true;
            }

            return m_index_[ch >> SHIFT_] == 0;
        }

        // package private method -----------------------------------------------

        // protected data member -----------------------------------------------

        /// <summary>
        /// Index values at build-time are 32 bits wide for easier processing.
        /// Bit 31 is set if the data block is used by multiple index values 
        /// (from setRange()).
        /// </summary>
        protected int[] m_index_;
        protected int m_indexLength_;
        protected int m_dataCapacity_;
        protected int m_dataLength_;
        protected bool m_isLatin1Linear_;
        protected bool m_isCompacted_;
        /// <summary>
        /// Map of adjusted indexes, used in utrie_compact().
        /// Maps from original indexes to new ones.
        /// </summary>
        protected int[] m_map_;

        /// <summary>
        /// Shift size for shifting right the input index. 1..9 
        /// </summary>
        protected static readonly int SHIFT_ = Trie.INDEX_STAGE_1_SHIFT_;
        /// <summary>
        /// Length of the index (stage 1) array before folding.
        /// Maximum number of Unicode code points (0x110000) shifted right by 
        /// SHIFT.
        /// </summary>
        protected static readonly int MAX_INDEX_LENGTH_ = (0x110000 >> SHIFT_);
        /// <summary>
        /// Length of the BMP portion of the index (stage 1) array.
        /// </summary>
        protected static readonly int BMP_INDEX_LENGTH_ = 0x10000 >> SHIFT_;
        /// <summary>
        /// Number of index (stage 1) entries per lead surrogate.
        /// Same as number of indexe entries for 1024 trail surrogates,
        /// ==0x400>>UTRIE_SHIFT
        /// 10 - SHIFT == Number of bits of a trail surrogate that are used in 
        /// index table lookups.
        /// </summary>
        protected static readonly int SURROGATE_BLOCK_COUNT_ = 1 << (10 - SHIFT_);
        /// <summary>
        /// Mask for getting the lower bits from the input index.
        /// DATA_BLOCK_LENGTH - 1.
        /// </summary>
        protected static readonly int MASK_ = Trie.INDEX_STAGE_3_MASK_;
        /// <summary>
        /// Shift size for shifting left the index array values.
        /// Increases possible data size with 16-bit index values at the cost
        /// of compactability.
        /// This requires blocks of stage 2 data to be aligned by UTRIE_DATA_GRANULARITY.
        /// 0..UTRIE_SHIFT
        /// </summary>
        protected static readonly int INDEX_SHIFT_ = Trie.INDEX_STAGE_2_SHIFT_;
        /// <summary>
        /// Maximum length of the runtime data (stage 2) array.
        /// Limited by 16-bit index values that are left-shifted by INDEX_SHIFT_.
        /// </summary>
        protected static readonly int MAX_DATA_LENGTH_ = (0x10000 << INDEX_SHIFT_);
        /// <summary>
        /// Shifting to position the index value in options
        /// </summary>
        protected static readonly int OPTIONS_INDEX_SHIFT_ = 4;
        /// <summary>
        /// If set, then the data (stage 2) array is 32 bits wide.
        /// </summary>
        protected static readonly int OPTIONS_DATA_IS_32_BIT_ = 0x100;
        /// <summary>
        /// If set, then Latin-1 data (for U+0000..U+00ff) is stored in the data 
        /// (stage 2) array as a simple, linear array at data + DATA_BLOCK_LENGTH.
        /// </summary>
        protected static readonly int OPTIONS_LATIN1_IS_LINEAR_ = 0x200;
        /// <summary>
        /// The alignment size of a stage 2 data block. Also the granularity for 
        /// compaction. 
        /// </summary>
        protected static readonly int DATA_GRANULARITY_ = 1 << INDEX_SHIFT_;

        // protected constructor ----------------------------------------------

        protected TrieBuilder()
        {
            m_index_ = new int[MAX_INDEX_LENGTH_];
            m_map_ = new int[MAX_BUILD_TIME_DATA_LENGTH_ >> SHIFT_];
            m_isLatin1Linear_ = false;
            m_isCompacted_ = false;
            m_indexLength_ = MAX_INDEX_LENGTH_;
        }

        protected TrieBuilder(TrieBuilder table)
        {
            m_index_ = new int[MAX_INDEX_LENGTH_];
            m_indexLength_ = table.m_indexLength_;
            System.Array.Copy(table.m_index_, 0, m_index_, 0, m_indexLength_);
            m_dataCapacity_ = table.m_dataCapacity_;
            m_dataLength_ = table.m_dataLength_;
            m_map_ = new int[table.m_map_.Length];
            System.Array.Copy(table.m_map_, 0, m_map_, 0, m_map_.Length);
            m_isLatin1Linear_ = table.m_isLatin1Linear_;
            m_isCompacted_ = table.m_isCompacted_;
        }

        // protected functions ------------------------------------------------

        /// <summary>
        /// Compare two sections of an array for equality.
        /// </summary>
        protected static bool EqualInt32(int[] array, int start1, int start2, int length)
        {
            while (length > 0 && array[start1] == array[start2])
            {
                ++start1;
                ++start2;
                --length;
            }
            return length == 0;
        }

        /// <summary>
        /// Set a value in the trie index map to indicate which data block
        /// is referenced and which one is not.
        /// utrie_compact() will remove data blocks that are not used at all.
        /// Set
        /// <list type="bullet">
        ///     <item><description>0 if it is used</description></item>
        ///     <item><description>-1 if it is not used</description></item>
        /// </list>
        /// </summary>
        protected virtual void FindUnusedBlocks()
        {
            // fill the entire map with "not used" 
            Arrays.Fill(m_map_, 0xff);

            // mark each block that _is_ used with 0
            for (int i = 0; i < m_indexLength_; ++i)
            {
                m_map_[Math.Abs(m_index_[i]) >> SHIFT_] = 0;
            }

            // never move the all-initial-value block 0
            m_map_[0] = 0;
        }

        /// <summary>
        /// Finds the same index block as the <paramref name="otherBlock"/>.
        /// </summary>
        /// <param name="index">index array.</param>
        /// <param name="indexLength">size of index</param>
        /// <param name="otherBlock"></param>
        /// <returns>Same index block.</returns>
        protected static int FindSameIndexBlock(int[] index, int indexLength,
                                                      int otherBlock)
        {
            for (int block = BMP_INDEX_LENGTH_; block < indexLength;
                 block += SURROGATE_BLOCK_COUNT_)
            {
                if (EqualInt32(index, block, otherBlock, SURROGATE_BLOCK_COUNT_))
                {
                    return block;
                }
            }
            return indexLength;
        }

        // private data member ------------------------------------------------

        /// <summary>
        /// Maximum length of the build-time data (stage 2) array.
        /// The maximum length is 0x110000 + DATA_BLOCK_LENGTH + 0x400.
        /// (Number of Unicode code points + one all-initial-value block +
        /// possible duplicate entries for 1024 lead surrogates.)
        /// </summary>
        private static readonly int MAX_BUILD_TIME_DATA_LENGTH_ =
            0x110000 + DATA_BLOCK_LENGTH + 0x400;
    }
}
