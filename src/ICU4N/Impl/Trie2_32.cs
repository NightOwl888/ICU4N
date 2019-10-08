using ICU4N.Support.IO;
using System.IO;

namespace ICU4N.Impl
{
    /// <summary>
    /// A read-only <see cref="Trie2"/>, holding 32 bit data values.
    /// </summary>
    /// <remarks>
    /// A <see cref="Trie2"/> is a highly optimized data structure for mapping from Unicode
    /// code points (values ranging from 0 to 0x10ffff) to a 16 or 32 bit value.
    /// <para/>
    /// See class <see cref="Trie2"/> for descriptions of the API for accessing the contents of a trie.
    /// <para/>
    /// The fundamental data access methods are declared final in this class, with
    /// the intent that applications might gain a little extra performance, when compared
    /// with calling the same methods via the abstract <see cref="Trie2"/> base class.
    /// </remarks>
    /// <author>aheninger</author>
    public class Trie2_32 : Trie2
    {
        /// <summary>
        /// Internal constructor, not for general use.
        /// </summary>
        internal Trie2_32()
        {
        }

        /// <summary>
        /// Create a <see cref="Trie2"/> from its serialized form.  Inverse of utrie2_serialize().
        /// </summary>
        /// <remarks>
        /// The serialized format is identical between ICU4C, ICU4J, and ICU4N, so this function
        /// will work with serialized <see cref="Trie2"/>s from any.
        /// <para/>
        /// The serialized <see cref="Trie2"/> in the bytes may be in either little or big endian byte order.
        /// This allows using serialized Tries from ICU4C without needing to consider the
        /// byte order of the system that created them.
        /// </remarks>
        /// <param name="bytes">A byte buffer to the serialized form of a <see cref="Trie2"/>.</param>
        /// <returns>An unserialized <see cref="Trie2_32"/>, ready for use.</returns>
        /// <exception cref="System.ArgumentException">If the buffer does not contain a serialized <see cref="Trie2"/>.</exception>
        /// <exception cref="System.IO.IOException">If a read error occurs in the buffer.</exception>
        /// <exception cref="System.InvalidCastException">If the bytes contain a serialized <see cref="Trie2_16"/>.</exception>
        new public static Trie2_32 CreateFromSerialized(ByteBuffer bytes) // ICU4N TODO: API Create overload that accepts byte[]
        {
            return (Trie2_32)Trie2.CreateFromSerialized(bytes);
        }

        /// <summary>
        /// Get the value for a code point as stored in the <see cref="Trie2"/>.
        /// </summary>
        /// <param name="codePoint">The code point.</param>
        /// <returns>The value.</returns>
        public override sealed int Get(int codePoint)
        {
            int value;
            int ix;

            if (codePoint >= 0)
            {
                if (codePoint < 0x0d800 || (codePoint > 0x0dbff && codePoint <= 0x0ffff))
                {
                    // Ordinary BMP code point, excluding leading surrogates.
                    // BMP uses a single level lookup.  BMP index starts at offset 0 in the Trie2 index.
                    // 32 bit data is stored in the index array itself.
                    ix = index[codePoint >> UTRIE2_SHIFT_2];
                    ix = (ix << UTRIE2_INDEX_SHIFT) + (codePoint & UTRIE2_DATA_MASK);
                    value = data32[ix];
                    return value;
                }
                if (codePoint <= 0xffff)
                {
                    // Lead Surrogate Code Point.  A Separate index section is stored for
                    // lead surrogate code units and code points.
                    //   The main index has the code unit data.
                    //   For this function, we need the code point data.
                    // Note: this expression could be refactored for slightly improved efficiency, but
                    //       surrogate code points will be so rare in practice that it's not worth it.
                    ix = index[UTRIE2_LSCP_INDEX_2_OFFSET + ((codePoint - 0xd800) >> UTRIE2_SHIFT_2)];
                    ix = (ix << UTRIE2_INDEX_SHIFT) + (codePoint & UTRIE2_DATA_MASK);
                    value = data32[ix];
                    return value;
                }
                if (codePoint < highStart)
                {
                    // Supplemental code point, use two-level lookup.
                    ix = (UTRIE2_INDEX_1_OFFSET - UTRIE2_OMITTED_BMP_INDEX_1_LENGTH) + (codePoint >> UTRIE2_SHIFT_1);
                    ix = index[ix];
                    ix += (codePoint >> UTRIE2_SHIFT_2) & UTRIE2_INDEX_2_MASK;
                    ix = index[ix];
                    ix = (ix << UTRIE2_INDEX_SHIFT) + (codePoint & UTRIE2_DATA_MASK);
                    value = data32[ix];
                    return value;
                }
                if (codePoint <= 0x10ffff)
                {
                    value = data32[highValueIndex];
                    return value;
                }
            }

            // Fall through.  The code point is outside of the legal range of 0..0x10ffff.
            return errorValue;
        }

        /// <summary>
        /// Get a <see cref="Trie2"/> value for a UTF-16 code unit.
        /// </summary>
        /// <remarks>
        /// This function returns the same value as <see cref="Get(int)"/> if the input 
        /// character is outside of the lead surrogate range.
        /// <para/>
        /// There are two values stored in a <see cref="Trie2"/> for inputs in the lead
        /// surrogate range.  This function returns the alternate value,
        /// while <see cref="Trie2.Get(int)"/> returns the main value.
        /// </remarks>
        /// <param name="codeUnit">A 16 bit code unit or lead surrogate value.</param>
        /// <returns>The value.</returns>
        public override int GetFromU16SingleLead(char codeUnit)
        {
            int value;
            int ix;

            ix = index[codeUnit >> UTRIE2_SHIFT_2];
            ix = (ix << UTRIE2_INDEX_SHIFT) + (codeUnit & UTRIE2_DATA_MASK);
            value = data32[ix];
            return value;

        }

        /// <summary>
        /// Serialize a <see cref="Trie2_32"/> onto an <see cref="Stream"/>.
        /// </summary>
        /// <remarks>
        /// A <see cref="Trie2"/> can be serialized multiple times.
        /// The serialized data is compatible with ICU4C UTrie2 serialization.
        /// <see cref="Trie2"/> serialization is unrelated to .NET object serialization.
        /// </remarks>
        /// <param name="os">The stream to which the serialized Trie2 data will be written.</param>
        /// <returns>The number of bytes written.</returns>
        /// <exception cref="IOException">On an error writing to the <see cref="Stream"/>.</exception>
        public int Serialize(Stream os)
        {
            DataOutputStream dos = new DataOutputStream(os);
            int bytesWritten = 0;

            bytesWritten += SerializeHeader(dos);
            for (int i = 0; i < dataLength; i++)
            {
                dos.WriteInt32(data32[i]);
            }
            bytesWritten += dataLength * 4;
            return bytesWritten;
        }

        /// <summary>
        /// Gets the number of bytes of the serialized trie.
        /// </summary>
        /// <returns>The number of bytes of the serialized trie.</returns>
        public int SerializedLength => 16 + header.indexLength * 2 + dataLength * 4;

        /// <summary>
        /// Given a starting code point, find the last in a range of code points,
        /// all with the same value.
        /// </summary>
        /// <remarks>
        /// This function is part of the implementation of iterating over the
        /// <see cref="Trie2"/>'s contents.
        /// </remarks>
        /// <param name="startingCP">The code point at which to begin looking.</param>
        /// <param name="limit"></param>
        /// <param name="value"></param>
        /// <returns>The last code point with the same value as the starting code point.</returns>
        internal override int RangeEnd(int startingCP, int limit, int value)
        {
            int cp = startingCP;
            int block = 0;
            int index2Block = 0;

            // Loop runs once for each of
            //   - a partial data block
            //   - a reference to the null (default) data block.
            //   - a reference to the index2 null block

            //outerLoop:
            for (; ; )
            {
                if (cp >= limit)
                {
                    break;
                }
                if (cp < 0x0d800 || (cp > 0x0dbff && cp <= 0x0ffff))
                {
                    // Ordinary BMP code point, excluding leading surrogates.
                    // BMP uses a single level lookup.  BMP index starts at offset 0 in the Trie2 index.
                    // 16 bit data is stored in the index array itself.
                    index2Block = 0;
                    block = index[cp >> UTRIE2_SHIFT_2] << UTRIE2_INDEX_SHIFT;
                }
                else if (cp < 0xffff)
                {
                    // Lead Surrogate Code Point, 0xd800 <= cp < 0xdc00
                    index2Block = UTRIE2_LSCP_INDEX_2_OFFSET;
                    block = index[index2Block + ((cp - 0xd800) >> UTRIE2_SHIFT_2)] << UTRIE2_INDEX_SHIFT;
                }
                else if (cp < highStart)
                {
                    // Supplemental code point, use two-level lookup.
                    int ix = (UTRIE2_INDEX_1_OFFSET - UTRIE2_OMITTED_BMP_INDEX_1_LENGTH) + (cp >> UTRIE2_SHIFT_1);
                    index2Block = index[ix];
                    block = index[index2Block + ((cp >> UTRIE2_SHIFT_2) & UTRIE2_INDEX_2_MASK)] << UTRIE2_INDEX_SHIFT;
                }
                else
                {
                    // Code point above highStart.
                    if (value == data32[highValueIndex])
                    {
                        cp = limit;
                    }
                    break;
                }

                if (index2Block == index2NullOffset)
                {
                    if (value != initialValue)
                    {
                        break;
                    }
                    cp += UTRIE2_CP_PER_INDEX_1_ENTRY;
                }
                else if (block == dataNullOffset)
                {
                    // The block at dataNullOffset has all values == initialValue.
                    // Because Trie2 iteration always proceeds in ascending order, we will always
                    //   encounter a null block at its beginning, and can skip over
                    //   a number of code points equal to the length of the block.
                    if (value != initialValue)
                    {
                        break;
                    }
                    cp += UTRIE2_DATA_BLOCK_LENGTH;
                }
                else
                {
                    // Current position refers to an ordinary data block.
                    // Walk over the data entries, checking the values.
                    int startIx = block + (cp & UTRIE2_DATA_MASK);
                    int limitIx = block + UTRIE2_DATA_BLOCK_LENGTH;
                    for (int ix = startIx; ix < limitIx; ix++)
                    {
                        if (data32[ix] != value)
                        {
                            // We came to an entry with a different value.
                            //   We are done.
                            cp += (ix - startIx);
                            goto outerLoop_break;
                        }
                    }
                    // The ordinary data block contained our value until its end.
                    //  Advance the current code point, and continue the outer loop.
                    cp += limitIx - startIx;
                }
            }
            outerLoop_break: { }
            if (cp > limit)
            {
                cp = limit;
            }

            return cp - 1;
        }
    }
}
