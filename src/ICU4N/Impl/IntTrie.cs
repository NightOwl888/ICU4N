using ICU4N.Support.Collections;
using ICU4N.Support.IO;
using ICU4N.Text;
using System;
using System.Diagnostics;

namespace ICU4N.Impl
{
    /// <summary>
    /// Trie implementation which stores data in int, 32 bits.
    /// </summary>
    /// <seealso cref="Trie"/>
    /// <author>synwee</author>
    /// <since>release 2.1, Jan 01 2002</since>
    // 2015-sep-03: Used only in CharsetSelector which could be switched to Trie2_32
    // as long as that does not load ICU4C selector data.
    public class Int32Trie : Trie
    {
        // public constructors ---------------------------------------------

        /// <summary>
        /// Creates a new Trie with the settings for the trie data.
        /// <para/>
        /// Unserialize the 32-bit-aligned input stream and use the data for the trie.
        /// </summary>
        /// <param name="bytes">File buffer to a ICU data file, containing the trie.</param>
        /// <param name="dataManipulate"><see cref="Trie.IDataManipulate"/> object which provides methods to parse the char data.</param>
        /// <exception cref="System.IO.IOException">Thrown when data reading fails.</exception>
        public Int32Trie(ByteBuffer bytes, IDataManipulate dataManipulate)
            : base(bytes, dataManipulate)
        {
            if (!IsInt32Trie)
            {
                throw new ArgumentException(
                                   "Data given does not belong to a int trie.");
            }
        }

        /// <summary>
        /// Make a dummy <see cref="Int32Trie"/>.
        /// </summary>
        /// <remarks>
        /// A dummy trie is an empty runtime trie, used when a real data trie cannot
        /// be loaded.
        /// <para/>
        /// The trie always returns the <paramref name="initialValue"/>,
        /// or the <paramref name="leadUnitValue"/> for lead surrogate code points.
        /// The Latin-1 part is always set up to be linear.
        /// </remarks>
        /// <param name="initialValue">The initial value that is set for all code points.</param>
        /// <param name="leadUnitValue">The value for lead surrogate code _units_ that do not
        /// have associated supplementary data.</param>
        /// <param name="dataManipulate"><see cref="Trie.IDataManipulate"/> object which provides methods to parse the char data.</param>
        public Int32Trie(int initialValue, int leadUnitValue, IDataManipulate dataManipulate)
                : base(new char[BMPIndexLength + SurrogateBlockCount], HeaderOptionsLatin1IsLinearMask, dataManipulate)
        {

            int dataLength, latin1Length, i, limit;
            char block;

            /* calculate the actual size of the dummy trie data */

            /* max(Latin-1, block 0) */
            dataLength = latin1Length = IndexStage1Shift <= 8 ? 256 : DataBlockLength;
            if (leadUnitValue != initialValue)
            {
                dataLength += DataBlockLength;
            }
            m_data_ = new int[dataLength];
            m_dataLength = dataLength;

            m_initialValue_ = initialValue;

            /* fill the index and data arrays */

            /* indexes are preset to 0 (block 0) */

            /* Latin-1 data */
            for (i = 0; i < latin1Length; ++i)
            {
                m_data_[i] = initialValue;
            }

            if (leadUnitValue != initialValue)
            {
                /* indexes for lead surrogate code units to the block after Latin-1 */
                block = (char)(latin1Length >> IndexStage2Shift);
                i = 0xd800 >> IndexStage1Shift;
                limit = 0xdc00 >> IndexStage1Shift;
                for (; i < limit; ++i)
                {
                    m_index[i] = block;
                }

                /* data for lead surrogate code units */
                limit = latin1Length + DataBlockLength;
                for (i = latin1Length; i < limit; ++i)
                {
                    m_data_[i] = leadUnitValue;
                }
            }
        }

        // public methods --------------------------------------------------

        /// <summary>
        /// Gets the value associated with the codepoint.
        /// If no value is associated with the codepoint, a default value will be
        /// returned.
        /// </summary>
        /// <param name="ch">Codepoint.</param>
        /// <returns>Offset to data.</returns>
        public int GetCodePointValue(int ch)
        {
            int offset;

            // fastpath for U+0000..U+D7FF
            if (0 <= ch && ch < UTF16.LeadSurrogateMinValue)
            {
                // copy of getRawOffset()
                offset = (m_index[ch >> IndexStage1Shift] << IndexStage2Shift)
                        + (ch & IndexStage3Mask);
                return m_data_[offset];
            }

            // handle U+D800..U+10FFFF
            offset = GetCodePointOffset(ch);
            return (offset >= 0) ? m_data_[offset] : m_initialValue_;
        }

        /// <summary>
        /// Gets the value to the data which this lead surrogate character points
        /// to. Returned data may contain folding offset information for the next
        /// trailing surrogate character.
        /// This method does not guarantee correct results for trail surrogates.
        /// </summary>
        /// <param name="ch">Lead surrogate character.</param>
        /// <returns>Data value.</returns>
        public int GetLeadValue(char ch)
        {
            return m_data_[GetLeadOffset(ch)];
        }

        /// <summary>
        /// Get the value associated with the BMP code point.
        /// Lead surrogate code points are treated as normal code points, with
        /// unfolded values that may differ from <see cref="GetLeadValue(char)"/> results.
        /// </summary>
        /// <param name="ch">The input BMP code point.</param>
        /// <returns>Trie data value associated with the BMP codepoint.</returns>
        public int GetBMPValue(char ch)
        {
            return m_data_[GetBMPOffset(ch)];
        }

        /// <summary>
        /// Get the value associated with a pair of surrogates.
        /// </summary>
        /// <param name="lead">A lead surrogate.</param>
        /// <param name="trail">A trail surrogate.</param>
        public int GetSurrogateValue(char lead, char trail)
        {
            if (!UTF16.IsLeadSurrogate(lead) || !UTF16.IsTrailSurrogate(trail))
            {
                throw new ArgumentException(
                    "Argument characters do not form a supplementary character");
            }
            // get fold position for the next trail surrogate
            int offset = GetSurrogateOffset(lead, trail);

            // get the real data from the folded lead/trail units
            if (offset > 0)
            {
                return m_data_[offset];
            }

            // return m_initialValue_ if there is an error
            return m_initialValue_;
        }

        /// <summary>
        /// Get a value from a folding offset (from the value of a lead surrogate)
        /// and a trail surrogate.
        /// </summary>
        /// <param name="leadvalue">The value of a lead surrogate that contains the folding offset.</param>
        /// <param name="trail">Trail surrogate.</param>
        /// <returns>Trie data value associated with the trail character.</returns>
        public int GetTrailValue(int leadvalue, char trail)
        {
            if (m_dataManipulate == null)
            {
                throw new InvalidOperationException(
                                 "The field DataManipulate in this Trie is null"); // ICU4N: Was originally NullPointerException
            }
            int offset = m_dataManipulate.GetFoldingOffset(leadvalue);
            if (offset > 0)
            {
                return m_data_[GetRawOffset(offset,
                                             (char)(trail & SurrogateMask))];
            }
            return m_initialValue_;
        }

        /// <summary>
        /// Gets the latin 1 fast path value.
        /// <para/>
        /// Note this only works if latin 1 characters have their own linear array.
        /// </summary>
        /// <param name="ch">Latin 1 characters.</param>
        /// <returns>Value associated with latin character.</returns>
        public int GetLatin1LinearValue(char ch)
        {
            return m_data_[IndexStage3Mask + 1 + ch];
        }

        ////CLOVER:OFF
        /// <summary>
        /// Checks if the argument <see cref="Trie"/> has the same data as this <see cref="Trie"/>.
        /// </summary>
        /// <param name="other">Other Trie to check.</param>
        /// <returns>true if the argument Trie has the same data as this Trie, false otherwise.</returns>
        public override bool Equals(Object other)
        {
            bool result = base.Equals(other);
            if (result && other is Int32Trie)
            {
                Int32Trie othertrie = (Int32Trie)other;
                if (m_initialValue_ != othertrie.m_initialValue_
                    || !Arrays.Equals(m_data_, othertrie.m_data_))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            Debug.Assert(false, "hashCode not designed");
            return 42;
        }
        ////CLOVER:ON

        ///// protected methods -----------------------------------------------

        /// <summary>
        /// Parses the input stream and stores its trie content into a index and
        /// data array
        /// </summary>
        /// <param name="bytes">Data buffer containing trie data.</param>
        protected override sealed void Unserialize(ByteBuffer bytes)
        {
            base.Unserialize(bytes);
            // one used for initial value
            m_data_ = ICUBinary.GetInt32s(bytes, m_dataLength, 0);
            m_initialValue_ = m_data_[0];
        }

        /// <summary>
        /// Gets the offset to the data which the surrogate pair points to.
        /// </summary>
        /// <param name="lead">Lead surrogate.</param>
        /// <param name="trail">Trailing surrogate.</param>
        /// <returns>Offset to data.</returns>
        protected override sealed int GetSurrogateOffset(char lead, char trail)
        {
            if (m_dataManipulate == null)
            {
                throw new InvalidOperationException(
                                 "The field DataManipulate in this Trie is null"); // ICU4N: Originally this was NullPointerException
            }
            // get fold position for the next trail surrogate
            int offset = m_dataManipulate.GetFoldingOffset(GetLeadValue(lead));

            // get the real data from the folded lead/trail units
            if (offset > 0)
            {
                return GetRawOffset(offset, (char)(trail & SurrogateMask));
            }

            // return -1 if there is an error, in this case we return the default
            // value: m_initialValue_
            return -1;
        }

        /// <summary>
        /// Gets the value at the argument index.
        /// For use internally in <see cref="TrieEnumerator"/>
        /// </summary>
        /// <param name="index">Value at index will be retrieved.</param>
        /// <returns>32 bit value</returns>
        /// <seealso cref="TrieEnumerator"/>
        protected internal override sealed int this[int index]
        {
            get { return m_data_[index]; }
        }

        /// <summary>
        /// Gets the default initial 32 bit value.
        /// </summary>
        protected internal override sealed int InitialValue
        {
            get { return m_initialValue_; }
        }

        // package private methods -----------------------------------------

        /// <summary>
        /// Internal constructor for builder use.
        /// </summary>
        /// <param name="index">The index array to be slotted into this trie.</param>
        /// <param name="data">The data array to be slotted into this trie.</param>
        /// <param name="initialvalue">The initial value for this trie.</param>
        /// <param name="options">Trie options to use.</param>
        /// <param name="datamanipulate">Folding implementation.</param>
        internal Int32Trie(char[] index, int[] data, int initialvalue, int options,
                IDataManipulate datamanipulate)
                : base(index, options, datamanipulate)
        {
            m_data_ = data;
            m_dataLength = m_data_.Length;
            m_initialValue_ = initialvalue;
        }

        // private data members --------------------------------------------

        /// <summary>
        /// Default value
        /// </summary>
        private int m_initialValue_;
        /// <summary>
        /// Array of char data
        /// </summary>
        private int[] m_data_;
    }
}
