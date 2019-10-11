using ICU4N.Globalization;
using ICU4N.Support.Collections;
using ICU4N.Support.IO;
using ICU4N.Text;
using System;
using System.Diagnostics;

namespace ICU4N.Impl
{
    /// <summary>
    /// Character data in <see cref="Trie"/> have different user-specified format
    /// for different purposes.
    /// This interface specifies methods to be implemented in order for
    /// <see cref="Trie"/> to surrogate offset information encapsulated within
    /// the data.
    /// </summary>
    public interface ITrieDataManipulate // ICU4N: renamed from IDataManipulate
    {
        /// <summary>
        /// Called by <see cref="Trie"/> to extract from a lead surrogate's
        /// data.
        /// The index array offset of the indexes for that lead surrogate.
        /// </summary>
        /// <param name="value">Data value for a surrogate from the trie, including the folding offset.</param>
        /// <returns>Offset or 0 if there is no data for the lead surrogate.</returns>
        int GetFoldingOffset(int value);
    }

    /// <summary>
    /// A trie is a kind of compressed, serializable table of values
    /// associated with Unicode code points (0..0x10ffff).
    /// </summary>
    /// <remarks>
    /// This class defines the basic structure of a trie and provides methods
    /// to <b>retrieve the offsets to the actual data</b>.
    /// <para/>
    /// Data will be the form of an array of basic types, char or int.
    /// <para/>
    /// The actual data format will have to be specified by the user in the
    /// interface <see cref="ITrieDataManipulate"/>.
    /// <para/>
    /// This trie implementation is optimized for getting offset while walking
    /// forward through a UTF-16 string.
    /// Therefore, the simplest and fastest access macros are the
    /// fromLead() and fromOffsetTrail() methods.
    /// The fromBMP() method are a little more complicated; they get offsets even
    /// for lead surrogate codepoints, while the fromLead() method get special
    /// "folded" offsets for lead surrogate code units if there is relevant data
    /// associated with them.
    /// From such a folded offsets, an offset needs to be extracted to supply
    /// to the fromOffsetTrail() methods.
    /// To handle such supplementary codepoints, some offset information are kept
    /// in the data.
    /// <para/>
    /// Methods in <see cref="ITrieDataManipulate"/> are called to retrieve
    /// that offset from the folded value for the lead surrogate unit.
    /// <para/>
    /// For examples of use, see <see cref="CharTrie"/> or
    /// <see cref="Int32Trie"/>.
    /// </remarks>
    /// <author>synwee</author>
    /// <seealso cref="CharTrie"/>
    /// <seealso cref="Int32Trie"/>
    /// <since>release 2.1, Jan 01 2002</since>
    // ICU4N TODO: The above mentioned methods are nowhere to be found. Need to work out what they
    // are referring to so the docs can be updated.
    public abstract class Trie
    {
        // public class declaration ----------------------------------------

        // ICU4N specific - de-nested IDataManipulate and renamed ITrieDataManipulate

        // default implementation
        private class DefaultGetFoldingOffset : ITrieDataManipulate
        {
            public virtual int GetFoldingOffset(int value)
            {
                return value;
            }
        }

        // public methods --------------------------------------------------

        /// <summary>
        /// Determines if this trie has a linear latin 1 array. 
        /// Returns true if this trie has a linear latin 1 array, false otherwise.
        /// </summary>
        public bool IsLatin1Linear
        {
            get { return m_isLatin1Linear_; }
        }

        /// <summary>
        /// Checks if the argument <see cref="Trie"/> has the same data as this <see cref="Trie"/>.
        /// Attributes are checked but not the index data.
        /// </summary>
        /// <param name="other"><see cref="Trie"/> to check.</param>
        /// <returns>true if the argument <see cref="Trie"/> has the same data as this <see cref="Trie"/>, false otherwise.</returns>
        //CLOVER:OFF
        public override bool Equals(object other)
        {
            if (other == this)
            {
                return true;
            }
            if (!(other is Trie))
            {
                return false;
            }
            Trie othertrie = (Trie)other;
            return m_isLatin1Linear_ == othertrie.m_isLatin1Linear_
                   && m_options_ == othertrie.m_options_
                   && m_dataLength == othertrie.m_dataLength
                   && Arrays.Equals(m_index, othertrie.m_index);
        }

        public override int GetHashCode()
        {
            Debug.Assert(false, "hashCode not designed");
            return 42;
        }
        //CLOVER:ON

        /// <summary>
        /// Gets the serialized data file size of the <see cref="Trie"/> in terms of the 
        /// number of bytes. This is used during trie data reading for size checking purposes.
        /// </summary>
        public virtual int SerializedDataSize
        {
            get
            {
                // includes signature, option, dataoffset and datalength output
                int result = (4 << 2);
                result += (m_dataOffset << 1);
                if (IsCharTrie)
                {
                    result += (m_dataLength << 1);
                }
                else if (IsInt32Trie)
                {
                    result += (m_dataLength << 2);
                }
                return result;
            }
        }

        // protected constructor -------------------------------------------

        /// <summary>
        /// Trie constructor for <see cref="CharTrie"/> use.
        /// </summary>
        /// <param name="bytes">Data of an ICU data file, containing the trie.</param>
        /// <param name="dataManipulate">Object containing the information to parse the trie data.</param>
        protected Trie(ByteBuffer bytes, ITrieDataManipulate dataManipulate) // ICU4N TODO: API Change to use byte[]
        {
            // Magic number to authenticate the data.
            int signature = bytes.GetInt32();
            m_options_ = bytes.GetInt32();

            if (!CheckHeader(signature))
            {
                throw new ArgumentException("ICU data file error: Trie header authentication failed, please check if you have the most updated ICU data file");
            }

            if (dataManipulate != null)
            {
                m_dataManipulate = dataManipulate;
            }
            else
            {
                m_dataManipulate = new DefaultGetFoldingOffset();
            }
            m_isLatin1Linear_ = (m_options_ &
                                 HeaderOptionsLatin1IsLinearMask) != 0;
            m_dataOffset = bytes.GetInt32();
            m_dataLength = bytes.GetInt32();
            Unserialize(bytes);
        }

        /// <summary>
        /// Trie constructor.
        /// </summary>
        /// <param name="index">Array to be used for index.</param>
        /// <param name="options">Options used by the trie.</param>
        /// <param name="dataManipulate">Object containing the information to parse the trie data.</param>
        protected Trie(char[] index, int options, ITrieDataManipulate dataManipulate) // ICU4N TODO: API - change to use [Flags] enum for options ?
        {
            m_options_ = options;
            if (dataManipulate != null)
            {
                m_dataManipulate = dataManipulate;
            }
            else
            {
                m_dataManipulate = new DefaultGetFoldingOffset();
            }
            m_isLatin1Linear_ = (m_options_ &
                                 HeaderOptionsLatin1IsLinearMask) != 0;
            m_index = index;
            m_dataOffset = m_index.Length;
        }


        // protected data members ------------------------------------------

        /// <summary>
        /// Lead surrogate code points' index displacement in the index array.
        /// <code>
        /// 0x10000-0xd800=0x2800
        /// 0x2800 >> <see cref="IndexStage1Shift"/>
        /// </code>
        /// </summary>
        protected internal const int LeadIndexOffset = 0x2800 >> 5;
        /// <summary>
        /// Shift size for shifting right the input index. 1..9
        /// </summary>
        protected internal const int IndexStage1Shift = 5;

        /// <summary>
        /// Shift size for shifting left the index array values.
        /// Increases possible data size with 16-bit index values at the cost
        /// of compactability.
        /// This requires blocks of stage 2 data to be aligned by
        /// <see cref="TrieBuilder.DataGranularity"/>.
        /// <code>
        /// 0..<see cref="IndexStage1Shift"/>
        /// </code>
        /// </summary>
        protected internal const int IndexStage2Shift = 2;
        /// <summary>
        /// Number of data values in a stage 2 (data array) block.
        /// </summary>
        protected internal const int DataBlockLength = 1 << IndexStage1Shift;
        /// <summary>
        /// Mask for getting the lower bits from the input index.
        /// <see cref="DataBlockLength"/> - 1.
        /// </summary>
        protected internal const int IndexStage3Mask = DataBlockLength - 1;
        /// <summary>Number of bits of a trail surrogate that are used in index table lookups.</summary>
        protected internal const int SurrogateBlockBits = 10 - IndexStage1Shift;
        /// <summary>
        /// Number of index (stage 1) entries per lead surrogate.
        /// Same as number of index entries for 1024 trail surrogates,
        /// ==0x400>><see cref="IndexStage1Shift"/>
        /// </summary>
        protected internal const int SurrogateBlockCount = (1 << SurrogateBlockBits);
        /// <summary>Length of the BMP portion of the index (stage 1) array.</summary>
        protected internal const int BMPIndexLength = 0x10000 >> IndexStage1Shift;
        /// <summary>
        /// Surrogate mask to use when shifting offset to retrieve supplementary
        /// values.
        /// </summary>
        protected internal const int SurrogateMask = 0x3FF;
        /// <summary>
        /// Index or UTF16 characters
        /// </summary>
        protected internal char[] m_index;
        /// <summary>
        /// Internal TrieValue which handles the parsing of the data value.
        /// This class is to be implemented by the user.
        /// </summary>
        protected internal ITrieDataManipulate m_dataManipulate;
        /// <summary>
        /// Start index of the data portion of the trie. <see cref="CharTrie"/> combines
        /// index and data into a char array, so this is used to indicate the
        /// initial offset to the data portion.
        /// Note this index always points to the initial value.
        /// </summary>
        protected internal int m_dataOffset;
        /// <summary>
        /// Length of the data array
        /// </summary>
        protected int m_dataLength;

        // protected methods -----------------------------------------------

        /// <summary>
        /// Gets the offset to the data which the surrogate pair points to.
        /// </summary>
        /// <param name="lead">Lead surrogate.</param>
        /// <param name="trail">Trailing surrogate.</param>
        /// <returns>Offset to data.</returns>
        protected abstract int GetSurrogateOffset(char lead, char trail);

        /// <summary>
        /// Gets the value at the argument index.
        /// </summary>
        /// <param name="index">Value at index will be retrieved.</param>
        /// <returns>32 bit value.</returns>
        protected internal abstract int this[int index] { get; } // ICU4N: Was GetValue(int) in ICU4J

        /// <summary>
        /// Gets the default initial 32 bit value.
        /// </summary>
        protected internal abstract int InitialValue { get; }

        /// <summary>
        /// Gets the offset to the data which the index ch after variable offset
        /// points to.
        /// </summary>
        /// <remarks>
        /// Note for locating a non-supplementary character data offset, calling
        /// <see cref="GetRawOffset(int, char)"/> will do. Otherwise if it is a supplementary character formed by
        /// surrogates lead and trail. Then we would have to call <see cref="GetRawOffset(int, char)"/>
        /// with GetFoldingIndexOffset(). See <see cref="GetSurrogateOffset(char, char)"/>.
        /// </remarks>
        /// <param name="offset">Index offset which <paramref name="ch"/> is to start from.</param>
        /// <param name="ch">Index to be used after offset.</param>
        /// <returns>Offset to the data.</returns>
        // ICU4N TODO: no GetFoldingIndexOffset() method in project - need to work out what this was
        protected int GetRawOffset(int offset, char ch)
        {
            return (m_index[offset + (ch >> IndexStage1Shift)]
                    << IndexStage2Shift)
                    + (ch & IndexStage3Mask);
        }

        /// <summary>
        /// Gets the offset to data which the BMP character points to
        /// Treats a lead surrogate as a normal code point.
        /// </summary>
        /// <param name="ch">BMP character.</param>
        /// <returns>Offset to data.</returns>
        protected int GetBMPOffset(char ch)
        {
            return (ch >= UTF16.LeadSurrogateMinValue
                    && ch <= UTF16.LeadSurrogateMaxValue)
                    ? GetRawOffset(LeadIndexOffset, ch)
                    : GetRawOffset(0, ch);
            // using a getRawOffset(ch) makes no diff
        }

        /// <summary>
        /// Gets the offset to the data which this lead surrogate character points
        /// to. Data at the returned offset may contain folding offset information for
        /// the next trailing surrogate character.
        /// </summary>
        /// <param name="ch">Lead surrogate character.</param>
        /// <returns>Offset to data.</returns>
        protected int GetLeadOffset(char ch)
        {
            return GetRawOffset(0, ch);
        }

        /// <summary>
        /// Internal trie getter from a code point.
        /// Could be faster(?) but longer with
        /// <code>
        ///     if((c32)&lt;=0xd7ff) { (result)=_TRIE_GET_RAW(trie, data, 0, c32); }
        /// </code>
        /// Gets the offset to data which the codepoint points to.
        /// </summary>
        /// <param name="ch">Codepoint.</param>
        /// <returns>Offset to data.</returns>
        protected int GetCodePointOffset(int ch)
        {
            // if ((ch >> 16) == 0) slower
            if (ch < 0)
            {
                return -1;
            }
            else if (ch < UTF16.LeadSurrogateMinValue)
            {
                // fastpath for the part of the BMP below surrogates (D800) where getRawOffset() works
                return GetRawOffset(0, (char)ch);
            }
            else if (ch < UTF16.SupplementaryMinValue)
            {
                // BMP codepoint
                return GetBMPOffset((char)ch);
            }
            else if (ch <= UChar.MaxValue)
            {
                // look at the construction of supplementary characters
                // trail forms the ends of it.
                return GetSurrogateOffset(UTF16.GetLeadSurrogate(ch),
                                          (char)(ch & SurrogateMask));
            }
            else
            {
                // return -1 if there is an error, in this case we return
                return -1;
            }
        }

        /// <summary>
        /// Parses the byte buffer and creates the trie index with it.
        /// <para/>
        /// The position of the input <see cref="ByteBuffer"/> must be right after the trie header.
        /// <para/>
        /// This is overwritten by the child classes.
        /// </summary>
        /// <param name="bytes">Buffer containing trie data.</param>
        protected virtual void Unserialize(ByteBuffer bytes)
        {
            m_index = ICUBinary.GetChars(bytes, m_dataOffset, 0);
        }

        /// <summary>
        /// Determines if this is a 32 bit trie. Returns true if options specifies this is a 32 bit trie.
        /// </summary>
        protected bool IsInt32Trie
        {
            get { return (m_options_ & HeaderOptionsDataIs32Bit) != 0; }
        }

        /// <summary>
        /// Determines if this is a 16 bit trie. Returns true if this is a 16 bit trie.
        /// </summary>
        protected bool IsCharTrie
        {
            get { return (m_options_ & HeaderOptionsDataIs32Bit) == 0; }
        }

        // private data members --------------------------------------------

        //  struct UTrieHeader {
        //      int32_t   signature;
        //      int32_t   options  (a bit field)
        //      int32_t   indexLength
        //      int32_t   dataLength

        /// <summary>
        /// Size of <see cref="Trie"/> header in bytes
        /// </summary>
        protected internal const int HeaderLength = 4 * 4;
        /// <summary>
        /// Latin 1 option mask
        /// </summary>
        protected internal const int HeaderOptionsLatin1IsLinearMask = 0x200; // ICU4N TODO: API - make into [Flags] enum
        /// <summary>
        /// Constant number to authenticate the byte block
        /// </summary>
        protected internal const int HeaderSignature = 0x54726965;
        /// <summary>
        /// Header option formatting
        /// </summary>
        private const int HeaderOptionsShiftMask = 0xF;
        protected internal const int HeaderOptionsIndexShift = 4; // ICU4N TODO: API - make into [Flags] enum
        protected internal const int HeaderOptionsDataIs32Bit = 0x100; // ICU4N TODO: API - make into [Flags] enum

        /// <summary>
        /// Flag indicator for Latin quick access data block
        /// </summary>
        private bool m_isLatin1Linear_;

        /// <summary>
        /// Trie options field.
        /// </summary>
        /// <remarks>
        /// Options bit field:
        /// <list type="bullet">
        ///     <item><description>9  1 = Latin-1 data is stored linearly at data + <see cref="DataBlockLength"/></description></item>
        ///     <item><description>8  0 = 16-bit data, 1=32-bit data</description></item>
        ///     <item><description>7..4  <see cref="IndexStage1Shift"/>   // 0..<see cref="IndexStage2Shift"/></description></item>
        ///     <item><description>3..0  <see cref="IndexStage2Shift"/>   // 1..9</description></item>
        /// </list>
        /// </remarks>
        private int m_options_;

        // private methods ---------------------------------------------------

        /// <summary>
        /// Authenticates raw data header.
        /// Checking the header information, signature and options.
        /// </summary>
        /// <param name="signature">This contains the options and type of a <see cref="Trie"/>.</param>
        /// <returns>true if the header is authenticated valid.</returns>
        private bool CheckHeader(int signature)
        {
            // check the signature
            // Trie in big-endian US-ASCII (0x54726965).
            // Magic number to authenticate the data.
            if (signature != HeaderSignature)
            {
                return false;
            }

            if ((m_options_ & HeaderOptionsShiftMask) !=
                                                        IndexStage1Shift ||
                ((m_options_ >> HeaderOptionsIndexShift) &
                                                    HeaderOptionsShiftMask)
                                                     != IndexStage2Shift)
            {
                return false;
            }
            return true;
        }
    }
}
