using ICU4N.Support.IO;
using ICU4N.Support.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections;

namespace ICU4N.Impl
{
    /// <summary>
    /// When iterating over the contents of a Trie2, an instance of TrieValueMapper may
    /// be used to remap the values from the Trie2.  The remapped values will be used
    /// both in determining the ranges of codepoints and as the value to be returned
    /// for each range.
    /// </summary>
    /// <remarks>
    /// Example of use:
    /// <code>
    /// public class MyValueMapper : IValueMapper
    /// {
    ///     public int Map(int input)
    ///     {
    ///         return input & 0x1f;
    ///     }
    /// }
    /// 
    /// foreach (Trie2EnumRange r in trie)
    /// {
    ///     // Do something with the range r
    /// }
    /// </code>
    /// </remarks>
    public interface IValueMapper
    {
        int Map(int originalVal);
    }

    /// <summary>
    /// This is the interface and common implementation of a Unicode Trie2.
    /// It is a kind of compressed table that maps from Unicode code points (0..0x10ffff)
    /// to 16- or 32-bit integer values.  It works best when there are ranges of
    /// characters with the same value, which is generally the case with Unicode
    /// character properties.
    /// <para/>
    /// This is the second common version of a Unicode trie (hence the name Trie2).
    /// </summary>
    public abstract class Trie2 : IEnumerable<Trie2.Range>
    {
        /**
     * Create a Trie2 from its serialized form.  Inverse of utrie2_serialize().
     *
     * Reads from the current position and leaves the buffer after the end of the trie.
     *
     * The serialized format is identical between ICU4C and ICU4J, so this function
     * will work with serialized Trie2s from either.
     *
     * The actual type of the returned Trie2 will be either Trie2_16 or Trie2_32, depending
     * on the width of the data.
     *
     * To obtain the width of the Trie2, check the actual class type of the returned Trie2.
     * Or use the createFromSerialized() function of Trie2_16 or Trie2_32, which will
     * return only Tries of their specific type/size.
     *
     * The serialized Trie2 on the stream may be in either little or big endian byte order.
     * This allows using serialized Tries from ICU4C without needing to consider the
     * byte order of the system that created them.
     *
     * @param bytes a byte buffer to the serialized form of a UTrie2.
     * @return An unserialized Trie2, ready for use.
     * @throws IllegalArgumentException if the stream does not contain a serialized Trie2.
     * @throws IOException if a read error occurs in the buffer.
     *
     */
        public static Trie2 CreateFromSerialized(ByteBuffer bytes)
        {
            //    From ICU4C utrie2_impl.h
            //    * Trie2 data structure in serialized form:
            //     *
            //     * UTrie2Header header;
            //     * uint16_t index[header.index2Length];
            //     * uint16_t data[header.shiftedDataLength<<2];  -- or uint32_t data[...]
            //     * @internal
            //     */
            //    typedef struct UTrie2Header {
            //        /** "Tri2" in big-endian US-ASCII (0x54726932) */
            //        uint32_t signature;

            //       /**
            //         * options bit field:
            //         * 15.. 4   reserved (0)
            //         *  3.. 0   UTrie2ValueBits valueBits
            //         */
            //        uint16_t options;
            //
            //        /** UTRIE2_INDEX_1_OFFSET..UTRIE2_MAX_INDEX_LENGTH */
            //        uint16_t indexLength;
            //
            //        /** (UTRIE2_DATA_START_OFFSET..UTRIE2_MAX_DATA_LENGTH)>>UTRIE2_INDEX_SHIFT */
            //        uint16_t shiftedDataLength;
            //
            //        /** Null index and data blocks, not shifted. */
            //        uint16_t index2NullOffset, dataNullOffset;
            //
            //        /**
            //         * First code point of the single-value range ending with U+10ffff,
            //         * rounded up and then shifted right by UTRIE2_SHIFT_1.
            //         */
            //        uint16_t shiftedHighStart;
            //    } UTrie2Header;

            ByteOrder outerByteOrder = bytes.Order;
            try
            {
                UTrie2Header header = new UTrie2Header();

                /* check the signature */
                header.signature = bytes.GetInt32();
                switch (header.signature)
                {
                    case 0x54726932:
                        // The buffer is already set to the trie data byte order.
                        break;
                    case 0x32697254:
                        // Temporarily reverse the byte order.
                        bool isBigEndian = outerByteOrder == ByteOrder.BIG_ENDIAN;
                        bytes.Order = isBigEndian ? ByteOrder.LITTLE_ENDIAN : ByteOrder.BIG_ENDIAN;
                        header.signature = 0x54726932;
                        break;
                    default:
                        throw new ArgumentException("Buffer does not contain a serialized UTrie2");
                }

                header.options = bytes.GetChar();
                header.indexLength = bytes.GetChar();
                header.shiftedDataLength = bytes.GetChar();
                header.index2NullOffset = bytes.GetChar();
                header.dataNullOffset = bytes.GetChar();
                header.shiftedHighStart = bytes.GetChar();

                // Trie2 data width - 0: 16 bits
                //                    1: 32 bits
                if ((header.options & UTRIE2_OPTIONS_VALUE_BITS_MASK) > 1)
                {
                    throw new ArgumentException("UTrie2 serialized format error.");
                }
                ValueWidth width;
                Trie2 This;
                if ((header.options & UTRIE2_OPTIONS_VALUE_BITS_MASK) == 0)
                {
                    width = ValueWidth.BITS_16;
                    This = new Trie2_16();
                }
                else
                {
                    width = ValueWidth.BITS_32;
                    This = new Trie2_32();
                }
                This.header = header;

                /* get the length values and offsets */
                This.indexLength = header.indexLength;
                This.dataLength = header.shiftedDataLength << UTRIE2_INDEX_SHIFT;
                This.index2NullOffset = header.index2NullOffset;
                This.dataNullOffset = header.dataNullOffset;
                This.highStart = header.shiftedHighStart << UTRIE2_SHIFT_1;
                This.highValueIndex = This.dataLength - UTRIE2_DATA_GRANULARITY;
                if (width == ValueWidth.BITS_16)
                {
                    This.highValueIndex += This.indexLength;
                }

                // Allocate the Trie2 index array. If the data width is 16 bits, the array also
                // includes the space for the data.

                int indexArraySize = This.indexLength;
                if (width == ValueWidth.BITS_16)
                {
                    indexArraySize += This.dataLength;
                }

                /* Read in the index */
                This.index = ICUBinary.GetChars(bytes, indexArraySize, 0);

                /* Read in the data. 16 bit data goes in the same array as the index.
                 * 32 bit data goes in its own separate data array.
                 */
                if (width == ValueWidth.BITS_16)
                {
                    This.data16 = This.indexLength;
                }
                else
                {
                    This.data32 = ICUBinary.GetInts(bytes, This.dataLength, 0);
                }

                switch (width)
                {
                    case ValueWidth.BITS_16:
                        This.data32 = null;
                        This.initialValue = This.index[This.dataNullOffset];
                        This.errorValue = This.index[This.data16 + UTRIE2_BAD_UTF8_DATA_OFFSET];
                        break;
                    case ValueWidth.BITS_32:
                        This.data16 = 0;
                        This.initialValue = This.data32[This.dataNullOffset];
                        This.errorValue = This.data32[UTRIE2_BAD_UTF8_DATA_OFFSET];
                        break;
                    default:
                        throw new ArgumentException("UTrie2 serialized format error.");
                }

                return This;
            }
            finally
            {
                bytes.Order = outerByteOrder;
            }
        }

        /**
         * Get the UTrie version from an InputStream containing the serialized form
         * of either a Trie (version 1) or a Trie2 (version 2).
         *
         * @param is   an InputStream containing the serialized form
         *             of a UTrie, version 1 or 2.  The stream must support mark() and reset().
         *             The position of the input stream will be left unchanged.
         * @param littleEndianOk If FALSE, only big-endian (Java native) serialized forms are recognized.
         *                    If TRUE, little-endian serialized forms are recognized as well.
         * @return     the Trie version of the serialized form, or 0 if it is not
         *             recognized as a serialized UTrie
         * @throws     IOException on errors in reading from the input stream.
         */
        public static int GetVersion(Stream input, bool littleEndianOk)
        {
            // ICU4N TODO: Determine if using seekable streams is acceptable here
            if (!input.CanSeek)
            {
                throw new ArgumentException("Input stream must support Seek().");
            }
            //input.mark(4);
            byte[] sig = new byte[4];
            int read = input.Read(sig, 0, sig.Length);
            //input.reset();
            input.Seek(-4, SeekOrigin.Current);

            if (read != sig.Length)
            {
                return 0;
            }

            if (sig[0] == 'T' && sig[1] == 'r' && sig[2] == 'i' && sig[3] == 'e')
            {
                return 1;
            }
            if (sig[0] == 'T' && sig[1] == 'r' && sig[2] == 'i' && sig[3] == '2')
            {
                return 2;
            }
            if (littleEndianOk)
            {
                if (sig[0] == 'e' && sig[1] == 'i' && sig[2] == 'r' && sig[3] == 'T')
                {
                    return 1;
                }
                if (sig[0] == '2' && sig[1] == 'i' && sig[2] == 'r' && sig[3] == 'T')
                {
                    return 2;
                }
            }
            return 0;
        }

        /**
         * Get the value for a code point as stored in the Trie2.
         *
         * @param codePoint the code point
         * @return the value
         */
        abstract public int Get(int codePoint);


        /**
         * Get the trie value for a UTF-16 code unit.
         *
         * A Trie2 stores two distinct values for input in the lead surrogate
         * range, one for lead surrogates, which is the value that will be
         * returned by this function, and a second value that is returned
         * by Trie2.get().
         *
         * For code units outside of the lead surrogate range, this function
         * returns the same result as Trie2.get().
         *
         * This function, together with the alternate value for lead surrogates,
         * makes possible very efficient processing of UTF-16 strings without
         * first converting surrogate pairs to their corresponding 32 bit code point
         * values.
         *
         * At build-time, enumerate the contents of the Trie2 to see if there
         * is non-trivial (non-initialValue) data for any of the supplementary
         * code points associated with a lead surrogate.
         * If so, then set a special (application-specific) value for the
         * lead surrogate code _unit_, with Trie2Writable.setForLeadSurrogateCodeUnit().
         *
         * At runtime, use Trie2.getFromU16SingleLead(). If there is non-trivial
         * data and the code unit is a lead surrogate, then check if a trail surrogate
         * follows. If so, assemble the supplementary code point and look up its value
         * with Trie2.get(); otherwise reset the lead
         * surrogate's value or do a code point lookup for it.
         *
         * If there is only trivial data for lead and trail surrogates, then processing
         * can often skip them. For example, in normalization or case mapping
         * all characters that do not have any mappings are simply copied as is.
         *
         * @param c the code point or lead surrogate value.
         * @return the value
         */
        abstract public int GetFromU16SingleLead(char c);


        /**
         * Equals function.  Two Tries are equal if their contents are equal.
         * The type need not be the same, so a Trie2Writable will be equal to
         * (read-only) Trie2_16 or Trie2_32 so long as they are storing the same values.
         *
         */
        public override bool Equals(object other)
        {
            if (!(other is Trie2))
            {
                return false;
            }
            Trie2 OtherTrie = (Trie2)other;
            Range rangeFromOther;

            using (var otherIter = OtherTrie.GetEnumerator())
            {
                foreach (Trie2.Range rangeFromThis in this)
                {
                    if (otherIter.MoveNext() == false)
                    {
                        return false;
                    }
                    rangeFromOther = otherIter.Current;
                    if (!rangeFromThis.Equals(rangeFromOther))
                    {
                        return false;
                    }
                }
                if (otherIter.MoveNext())
                {
                    return false;
                }
            }

            if (errorValue != OtherTrie.errorValue ||
                initialValue != OtherTrie.initialValue)
            {
                return false;
            }

            return true;
        }


        public override int GetHashCode()
        {
            if (fHash == 0)
            {
                int hash = InitHash();
                foreach (Range r in this)
                {
                    hash = HashInt(hash, r.GetHashCode());
                }
                if (hash == 0)
                {
                    hash = 1;
                }
                fHash = hash;
            }
            return fHash;
        }

        /**
         * When iterating over the contents of a Trie2, Elements of this type are produced.
         * The iterator will return one item for each contiguous range of codepoints  having the same value.
         *
         * When iterating, the same Trie2EnumRange object will be reused and returned for each range.
         * If you need to retain complete iteration results, clone each returned Trie2EnumRange,
         * or save the range in some other way, before advancing to the next iteration step.
         */
        public class Range
        {
            public int StartCodePoint { get; set; }
            public int EndCodePoint { get; set; }    // Inclusive.
            public int Value { get; set; }
            public bool LeadSurrogate { get; set; }


            public override bool Equals(object other)
            {
                if (other == null || !(other.GetType().Equals(GetType())))
                {
                    return false;
                }
                Range tother = (Range)other;
                return this.StartCodePoint == tother.StartCodePoint &&
                       this.EndCodePoint == tother.EndCodePoint &&
                       this.Value == tother.Value &&
                       this.LeadSurrogate == tother.LeadSurrogate;
            }


            public override int GetHashCode()
            {
                int h = InitHash();
                h = HashUChar32(h, StartCodePoint);
                h = HashUChar32(h, EndCodePoint);
                h = HashInt(h, Value);
                h = HashByte(h, LeadSurrogate ? 1 : 0);
                return h;
            }
        }


        /**
         *  Create an iterator over the value ranges in this Trie2.
         *  Values from the Trie2 are not remapped or filtered, but are returned as they
         *  are stored in the Trie2.
         *
         * @return an Iterator
         */
    //    @Override
    //public Iterator<Range> iterator()
    //    {
    //        return iterator(defaultValueMapper);
    //    }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator(defaultValueMapper);
        }

        public IEnumerator<Trie2.Range> GetEnumerator()
        {
            return GetEnumerator(defaultValueMapper);
        }

        private class DefaultValueMapper : IValueMapper
        {
            public int Map(int originalVal)
            {
                return originalVal;
            }
        }

        private static IValueMapper defaultValueMapper = new DefaultValueMapper();

        /**
         * Create an iterator over the value ranges from this Trie2.
         * Values from the Trie2 are passed through a caller-supplied remapping function,
         * and it is the remapped values that determine the ranges that
         * will be produced by the iterator.
         *
         *
         * @param mapper provides a function to remap values obtained from the Trie2.
         * @return an Iterator
         */
        //public Iterator<Range> iterator(IValueMapper mapper)
        //{
        //    return new Trie2Iterator(mapper);
        //}

        public IEnumerator<Range> GetEnumerator(IValueMapper mapper)
        {
            return new Trie2Iterator(this, mapper);
        }


        /**
         * Create an iterator over the Trie2 values for the 1024=0x400 code points
         * corresponding to a given lead surrogate.
         * For example, for the lead surrogate U+D87E it will enumerate the values
         * for [U+2F800..U+2FC00[.
         * Used by data builder code that sets special lead surrogate code unit values
         * for optimized UTF-16 string processing.
         *
         * Do not modify the Trie2 during the iteration.
         *
         * Except for the limited code point range, this functions just like Trie2.iterator().
         *
         */
        public IEnumerator<Range> GetEnumeratorForLeadSurrogate(char lead, IValueMapper mapper)
        {
            return new Trie2Iterator(this, lead, mapper);
        }

        /**
         * Create an iterator over the Trie2 values for the 1024=0x400 code points
         * corresponding to a given lead surrogate.
         * For example, for the lead surrogate U+D87E it will enumerate the values
         * for [U+2F800..U+2FC00[.
         * Used by data builder code that sets special lead surrogate code unit values
         * for optimized UTF-16 string processing.
         *
         * Do not modify the Trie2 during the iteration.
         *
         * Except for the limited code point range, this functions just like Trie2.iterator().
         *
         */
        public IEnumerator<Range> GetEnumeratorForLeadSurrogate(char lead)
        {
            return new Trie2Iterator(this, lead, defaultValueMapper);
        }

        // ICU4N specific - de-nested IValueMapper


        /**
          * Serialize a trie2 Header and Index onto an OutputStream.  This is
          * common code used for  both the Trie2_16 and Trie2_32 serialize functions.
          * @param dos the stream to which the serialized Trie2 data will be written.
          * @return the number of bytes written.
          */
        protected int SerializeHeader(DataOutputStream dos)
        {
            // Write the header.  It is already set and ready to use, having been
            //  created when the Trie2 was unserialized or when it was frozen.
            int bytesWritten = 0;

            dos.WriteInt32(header.signature);
            dos.WriteInt16(header.options);
            dos.WriteInt16(header.indexLength);
            dos.WriteInt16(header.shiftedDataLength);
            dos.WriteInt16(header.index2NullOffset);
            dos.WriteInt16(header.dataNullOffset);
            dos.WriteInt16(header.shiftedHighStart);
            bytesWritten += 16;

            // Write the index
            int i;
            for (i = 0; i < header.indexLength; i++)
            {
                dos.WriteChar(index[i]);
            }
            bytesWritten += header.indexLength;
            return bytesWritten;
        }


        /**
         * Struct-like class for holding the results returned by a UTrie2 CharSequence iterator.
         * The iteration walks over a CharSequence, and for each Unicode code point therein
         * returns the character and its associated Trie2 value.
         */
        public class CharSequenceValues // ICU4N TODO: API De-nest
        {
            /** string index of the current code point. */
            public int Index { get; set; }
            /** The code point at index.  */
            public int CodePoint { get; set; }
            /** The Trie2 value for the current code point */
            public int Value { get; set; }
        }

        /// <summary>
        /// Create an iterator that will produce the values from the Trie2 for
        /// the sequence of code points in an input text.
        /// </summary>
        /// <param name="text">A text string to be iterated over.</param>
        /// <param name="index">The starting iteration position within the input text.</param>
        /// <returns>The <see cref="CharSequenceEnumerator"/>.</returns>
        public virtual CharSequenceEnumerator GetCharSequenceEnumerator(string text, int index) // ICU4N specific
        {
            return new CharSequenceEnumerator(this, text.ToCharSequence(), index);
        }

        /// <summary>
        /// Create an iterator that will produce the values from the Trie2 for
        /// the sequence of code points in an input text.
        /// </summary>
        /// <param name="text">A text string to be iterated over.</param>
        /// <param name="index">The starting iteration position within the input text.</param>
        /// <returns>The <see cref="CharSequenceEnumerator"/>.</returns>
        public virtual CharSequenceEnumerator GetCharSequenceEnumerator(StringBuilder text, int index) // ICU4N specific
        {
            return new CharSequenceEnumerator(this, text.ToCharSequence(), index);
        }

        /// <summary>
        /// Create an iterator that will produce the values from the Trie2 for
        /// the sequence of code points in an input text.
        /// </summary>
        /// <param name="text">A text string to be iterated over.</param>
        /// <param name="index">The starting iteration position within the input text.</param>
        /// <returns>The <see cref="CharSequenceEnumerator"/>.</returns>
        public virtual CharSequenceEnumerator GetCharSequenceEnumerator(char[] text, int index) // ICU4N specific
        {
            return new CharSequenceEnumerator(this, text.ToCharSequence(), index);
        }

        /// <summary>
        /// Create an iterator that will produce the values from the Trie2 for
        /// the sequence of code points in an input text.
        /// </summary>
        /// <param name="text">A text string to be iterated over.</param>
        /// <param name="index">The starting iteration position within the input text.</param>
        /// <returns>The <see cref="CharSequenceEnumerator"/>.</returns>
        internal virtual CharSequenceEnumerator GetCharSequenceEnumerator(ICharSequence text, int index)
        {
            return new CharSequenceEnumerator(this, text, index);
        }

        // TODO:  Survey usage of the equivalent of CharSequenceIterator in ICU4C
        //        and if there is none, remove it from here.
        //        Don't waste time testing and maintaining unused code.

        /**
         * An iterator that operates over an input CharSequence, and for each Unicode code point
         * in the input returns the associated value from the Trie2.
         *
         * The iterator can move forwards or backwards, and can be reset to an arbitrary index.
         *
         * Note that Trie2_16 and Trie2_32 subclass Trie2.CharSequenceIterator.  This is done
         * only for performance reasons.  It does require that any changes made here be propagated
         * into the corresponding code in the subclasses.
         */
        public class CharSequenceEnumerator : IEnumerator<CharSequenceValues> //: Iterator<CharSequenceValues> 
        {
            /// <summary>
            /// Internal constructor.
            /// </summary>
            internal CharSequenceEnumerator(Trie2 outerInstance, ICharSequence t, int index)
            {
                this.outerInstance = outerInstance;
                text = t;
                textLength = text.Length;
                Set(index);
            }
            private readonly Trie2 outerInstance;

            private ICharSequence text;
            private int textLength;
            private int index;
            private Trie2.CharSequenceValues fResults = new Trie2.CharSequenceValues();


            public virtual void Set(int i)
            {
                if (i < 0 || i > textLength)
                {
                    throw new IndexOutOfRangeException();
                }
                index = i;
            }


            protected virtual bool HasNext
            {
                get { return index < textLength; }
            }


            protected virtual bool HasPrevious
            {
                get { return index > 0; }
            }

            public CharSequenceValues Current
            {
                get { return fResults; }
            }

            object IEnumerator.Current
            {
                get { return fResults; }
            }

            protected virtual Trie2.CharSequenceValues Next()
            {
                int c = Character.CodePointAt(text, index);
                int val = outerInstance.Get(c);

                fResults.Index = index;
                fResults.CodePoint = c;
                fResults.Value = val;
                index++;
                if (c >= 0x10000)
                {
                    index++;
                }
                return fResults;
            }


            protected virtual Trie2.CharSequenceValues Previous()
            {
                int c = Character.CodePointBefore(text, index);
                int val = outerInstance.Get(c);
                index--;
                if (c >= 0x10000)
                {
                    index--;
                }
                fResults.Index = index;
                fResults.CodePoint = c;
                fResults.Value = val;
                return fResults;
            }

            public virtual bool MoveNext()
            {
                if (!HasNext)
                    return false;
                Next();
                return true;
            }

            public virtual bool MovePrevious()
            {
                if (!HasPrevious)
                    return false;
                Previous();
                return true;
            }

            public virtual void Reset()
            {
                throw new NotSupportedException();
            }

            public void Dispose()
            {
                // Nothing to do
            }

            // ICU4N specific - Remove not supported by the interface
        }


        //--------------------------------------------------------------------------------
        //
        // Below this point are internal implementation items.  No further public API.
        //
        //--------------------------------------------------------------------------------


        /**
         * Selectors for the width of a UTrie2 data value.
         */
        internal enum ValueWidth
        {
            BITS_16,
            BITS_32
        }

        /**
        * Trie2 data structure in serialized form:
        *
        * UTrie2Header header;
        * uint16_t index[header.index2Length];
        * uint16_t data[header.shiftedDataLength<<2];  -- or uint32_t data[...]
        *
        * For Java, this is read from the stream into an instance of UTrie2Header.
        * (The C version just places a struct over the raw serialized data.)
        *
        * @internal
*/
        internal class UTrie2Header
        {
            /** "Tri2" in big-endian US-ASCII (0x54726932) */
            internal int signature;

            /**
             * options bit field (uint16_t):
             * 15.. 4   reserved (0)
             *  3.. 0   UTrie2ValueBits valueBits
             */
            internal int options;

            /** UTRIE2_INDEX_1_OFFSET..UTRIE2_MAX_INDEX_LENGTH  (uint16_t) */
            internal int indexLength;

            /** (UTRIE2_DATA_START_OFFSET..UTRIE2_MAX_DATA_LENGTH)>>UTRIE2_INDEX_SHIFT  (uint16_t) */
            internal int shiftedDataLength;

            /** Null index and data blocks, not shifted.  (uint16_t) */
            internal int index2NullOffset, dataNullOffset;

            /**
             * First code point of the single-value range ending with U+10ffff,
             * rounded up and then shifted right by UTRIE2_SHIFT_1.  (uint16_t)
             */
            internal int shiftedHighStart;
        }

        //
        //  Data members of UTrie2.
        //
        internal UTrie2Header header;
        internal char[] index;           // Index array.  Includes data for 16 bit Tries.
        internal int data16;            // Offset to data portion of the index array, if 16 bit data.
                                        //    zero if 32 bit data.
        internal int[] data32;          // NULL if 16b data is used via index

        internal int indexLength;
        internal int dataLength;
        internal int index2NullOffset;  // 0xffff if there is no dedicated index-2 null block
        internal int initialValue;

        /** Value returned for out-of-range code points and illegal UTF-8. */
        internal int errorValue;

        /* Start of the last range which ends at U+10ffff, and its value. */
        internal int highStart;
        internal int highValueIndex;

        internal int dataNullOffset;

        internal int fHash;              // Zero if not yet computed.
                                         //  Shared by Trie2Writable, Trie2_16, Trie2_32.
                                         //  Thread safety:  if two racing threads compute
                                         //     the same hash on a frozen Trie2, no damage is done.


        /**
         * Trie2 constants, defining shift widths, index array lengths, etc.
         *
         * These are needed for the runtime macros but users can treat these as
         * implementation details and skip to the actual public API further below.
         */

        internal static readonly int UTRIE2_OPTIONS_VALUE_BITS_MASK = 0x000f;


        /** Shift size for getting the index-1 table offset. */
        internal static readonly int UTRIE2_SHIFT_1 = 6 + 5;

        /** Shift size for getting the index-2 table offset. */
        internal static readonly int UTRIE2_SHIFT_2 = 5;

        /**
         * Difference between the two shift sizes,
         * for getting an index-1 offset from an index-2 offset. 6=11-5
         */
        internal static readonly int UTRIE2_SHIFT_1_2 = UTRIE2_SHIFT_1 - UTRIE2_SHIFT_2;

        /**
         * Number of index-1 entries for the BMP. 32=0x20
         * This part of the index-1 table is omitted from the serialized form.
         */
        internal static readonly int UTRIE2_OMITTED_BMP_INDEX_1_LENGTH = 0x10000 >> UTRIE2_SHIFT_1;

        /** Number of code points per index-1 table entry. 2048=0x800 */
        internal static readonly int UTRIE2_CP_PER_INDEX_1_ENTRY = 1 << UTRIE2_SHIFT_1;

        /** Number of entries in an index-2 block. 64=0x40 */
        internal static readonly int UTRIE2_INDEX_2_BLOCK_LENGTH = 1 << UTRIE2_SHIFT_1_2;

        /** Mask for getting the lower bits for the in-index-2-block offset. */
        internal static readonly int UTRIE2_INDEX_2_MASK = UTRIE2_INDEX_2_BLOCK_LENGTH - 1;

        /** Number of entries in a data block. 32=0x20 */
        internal static readonly int UTRIE2_DATA_BLOCK_LENGTH = 1 << UTRIE2_SHIFT_2;

        /** Mask for getting the lower bits for the in-data-block offset. */
        internal static readonly int UTRIE2_DATA_MASK = UTRIE2_DATA_BLOCK_LENGTH - 1;

        /**
         * Shift size for shifting left the index array values.
         * Increases possible data size with 16-bit index values at the cost
         * of compactability.
         * This requires data blocks to be aligned by UTRIE2_DATA_GRANULARITY.
         */
        internal static readonly int UTRIE2_INDEX_SHIFT = 2;

        /** The alignment size of a data block. Also the granularity for compaction. */
        internal static readonly int UTRIE2_DATA_GRANULARITY = 1 << UTRIE2_INDEX_SHIFT;

        /* Fixed layout of the first part of the index array. ------------------- */

        /**
         * The BMP part of the index-2 table is fixed and linear and starts at offset 0.
         * Length=2048=0x800=0x10000>>UTRIE2_SHIFT_2.
         */
        internal static readonly int UTRIE2_INDEX_2_OFFSET = 0;

        /**
         * The part of the index-2 table for U+D800..U+DBFF stores values for
         * lead surrogate code _units_ not code _points_.
         * Values for lead surrogate code _points_ are indexed with this portion of the table.
         * Length=32=0x20=0x400>>UTRIE2_SHIFT_2. (There are 1024=0x400 lead surrogates.)
         */
        internal static readonly int UTRIE2_LSCP_INDEX_2_OFFSET = 0x10000 >> UTRIE2_SHIFT_2;
        internal static readonly int UTRIE2_LSCP_INDEX_2_LENGTH = 0x400 >> UTRIE2_SHIFT_2;

        /** Count the lengths of both BMP pieces. 2080=0x820 */
        internal static readonly int UTRIE2_INDEX_2_BMP_LENGTH = UTRIE2_LSCP_INDEX_2_OFFSET + UTRIE2_LSCP_INDEX_2_LENGTH;

        /**
         * The 2-byte UTF-8 version of the index-2 table follows at offset 2080=0x820.
         * Length 32=0x20 for lead bytes C0..DF, regardless of UTRIE2_SHIFT_2.
         */
        internal static readonly int UTRIE2_UTF8_2B_INDEX_2_OFFSET = UTRIE2_INDEX_2_BMP_LENGTH;
        internal static readonly int UTRIE2_UTF8_2B_INDEX_2_LENGTH = 0x800 >> 6;  /* U+0800 is the first code point after 2-byte UTF-8 */

        /**
         * The index-1 table, only used for supplementary code points, at offset 2112=0x840.
         * Variable length, for code points up to highStart, where the last single-value range starts.
         * Maximum length 512=0x200=0x100000>>UTRIE2_SHIFT_1.
         * (For 0x100000 supplementary code points U+10000..U+10ffff.)
         *
         * The part of the index-2 table for supplementary code points starts
         * after this index-1 table.
         *
         * Both the index-1 table and the following part of the index-2 table
         * are omitted completely if there is only BMP data.
         */
        internal static readonly int UTRIE2_INDEX_1_OFFSET = UTRIE2_UTF8_2B_INDEX_2_OFFSET + UTRIE2_UTF8_2B_INDEX_2_LENGTH;
        internal static readonly int UTRIE2_MAX_INDEX_1_LENGTH = 0x100000 >> UTRIE2_SHIFT_1;

        /*
         * Fixed layout of the first part of the data array. -----------------------
         * Starts with 4 blocks (128=0x80 entries) for ASCII.
         */

        /**
         * The illegal-UTF-8 data block follows the ASCII block, at offset 128=0x80.
         * Used with linear access for single bytes 0..0xbf for simple error handling.
         * Length 64=0x40, not UTRIE2_DATA_BLOCK_LENGTH.
         */
        internal static readonly int UTRIE2_BAD_UTF8_DATA_OFFSET = 0x80;

        /** The start of non-linear-ASCII data blocks, at offset 192=0xc0. */
        internal static readonly int UTRIE2_DATA_START_OFFSET = 0xc0;

        /* Building a Trie2 ---------------------------------------------------------- */

        /*
         * These definitions are mostly needed by utrie2_builder.c, but also by
         * utrie2_get32() and utrie2_enum().
         */

        /*
         * At build time, leave a gap in the index-2 table,
         * at least as long as the maximum lengths of the 2-byte UTF-8 index-2 table
         * and the supplementary index-1 table.
         * Round up to UTRIE2_INDEX_2_BLOCK_LENGTH for proper compacting.
         */
        internal static readonly int UNEWTRIE2_INDEX_GAP_OFFSET = UTRIE2_INDEX_2_BMP_LENGTH;
        internal static readonly int UNEWTRIE2_INDEX_GAP_LENGTH =
            ((UTRIE2_UTF8_2B_INDEX_2_LENGTH + UTRIE2_MAX_INDEX_1_LENGTH) + UTRIE2_INDEX_2_MASK) &
            ~UTRIE2_INDEX_2_MASK;

        /**
         * Maximum length of the build-time index-2 array.
         * Maximum number of Unicode code points (0x110000) shifted right by UTRIE2_SHIFT_2,
         * plus the part of the index-2 table for lead surrogate code points,
         * plus the build-time index gap,
         * plus the null index-2 block.
         */
        internal static readonly int UNEWTRIE2_MAX_INDEX_2_LENGTH =
            (0x110000 >> UTRIE2_SHIFT_2) +
            UTRIE2_LSCP_INDEX_2_LENGTH +
            UNEWTRIE2_INDEX_GAP_LENGTH +
            UTRIE2_INDEX_2_BLOCK_LENGTH;

        internal static readonly int UNEWTRIE2_INDEX_1_LENGTH = 0x110000 >> UTRIE2_SHIFT_1;

        /**
         * Maximum length of the build-time data array.
         * One entry per 0x110000 code points, plus the illegal-UTF-8 block and the null block,
         * plus values for the 0x400 surrogate code units.
         */
        internal static readonly int UNEWTRIE2_MAX_DATA_LENGTH = (0x110000 + 0x40 + 0x40 + 0x400);



        /**
         * Implementation class for an iterator over a Trie2.
         *
         *   Iteration over a Trie2 first returns all of the ranges that are indexed by code points,
         *   then returns the special alternate values for the lead surrogates
         *
         * @internal
         */
        public class Trie2Iterator : IEnumerator<Range> //: Iterator<Range>
        {
            private readonly Trie2 outerInstance;
            private Range current = null;

            // The normal constructor that configures the iterator to cover the complete
            //   contents of the Trie2
            internal Trie2Iterator(Trie2 outerInstance, IValueMapper vm)
            {
                this.outerInstance = outerInstance;
                mapper = vm;
                nextStart = 0;
                limitCP = 0x110000;
                doLeadSurrogates = true;
            }

            // An alternate constructor that configures the iterator to cover only the
            //   code points corresponding to a particular Lead Surrogate value.
            internal Trie2Iterator(Trie2 outerInstance, char leadSurrogate, IValueMapper vm)
            {
                if (leadSurrogate < 0xd800 || leadSurrogate > 0xdbff)
                {
                    throw new ArgumentException("Bad lead surrogate value.");
                }
                this.outerInstance = outerInstance;
                mapper = vm;
                nextStart = (leadSurrogate - 0xd7c0) << 10;
                limitCP = nextStart + 0x400;
                doLeadSurrogates = false;   // Do not iterate over lead the special lead surrogate
                                            //   values after completing iteration over code points.
            }

            /**
             *  The main next() function for Trie2 iterators
             *
             */
                private Range Next()
            {
                //if (!hasNext())
                //{
                //    throw new NoSuchElementException();
                //}
                if (nextStart >= limitCP)
                {
                    // Switch over from iterating normal code point values to
                    //   doing the alternate lead-surrogate values.
                    doingCodePoints = false;
                    nextStart = 0xd800;
                }
                int endOfRange = 0;
                int val = 0;
                int mappedVal = 0;

                if (doingCodePoints)
                {
                    // Iteration over code point values.
                    val = outerInstance.Get(nextStart);
                    mappedVal = mapper.Map(val);
                    endOfRange = outerInstance.RangeEnd(nextStart, limitCP, val);
                    // Loop once for each range in the Trie2 with the same raw (unmapped) value.
                    // Loop continues so long as the mapped values are the same.
                    for (; ; )
                    {
                        if (endOfRange >= limitCP - 1)
                        {
                            break;
                        }
                        val = outerInstance.Get(endOfRange + 1);
                        if (mapper.Map(val) != mappedVal)
                        {
                            break;
                        }
                        endOfRange = outerInstance.RangeEnd(endOfRange + 1, limitCP, val);
                    }
                }
                else
                {
                    // Iteration over the alternate lead surrogate values.
                    val = outerInstance.GetFromU16SingleLead((char)nextStart);
                    mappedVal = mapper.Map(val);
                    endOfRange = RangeEndLS((char)nextStart);
                    // Loop once for each range in the Trie2 with the same raw (unmapped) value.
                    // Loop continues so long as the mapped values are the same.
                    for (; ; )
                    {
                        if (endOfRange >= 0xdbff)
                        {
                            break;
                        }
                        val = outerInstance.GetFromU16SingleLead((char)(endOfRange + 1));
                        if (mapper.Map(val) != mappedVal)
                        {
                            break;
                        }
                        endOfRange = RangeEndLS((char)(endOfRange + 1));
                    }
                }
                returnValue.StartCodePoint = nextStart;
                returnValue.EndCodePoint = endOfRange;
                returnValue.Value = mappedVal;
                returnValue.LeadSurrogate = !doingCodePoints;
                nextStart = endOfRange + 1;
                return returnValue;
            }

            /**
             *
             */
                private bool HasNext
            {
                get { return doingCodePoints && (doLeadSurrogates || nextStart < limitCP) || nextStart < 0xdc00; }
            }

            public virtual Range Current
            {
                get { return current; }
            }

            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            public virtual bool MoveNext()
            {
                if (!HasNext)
                    return false;
                current = Next();
                return true;
            }

            public virtual void Reset()
            {
                throw new NotSupportedException();
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            public virtual void Dispose(bool disposing)
            {
                // Nothing to do
            }

            // ICU4N specific - Remove() not supported in .NET



            /**
             * Find the last lead surrogate in a contiguous range  with the
             * same Trie2 value as the input character.
             *
             * Use the alternate Lead Surrogate values from the Trie2,
             * not the code-point values.
             *
             * Note: Trie2_16 and Trie2_32 override this implementation with optimized versions,
             *       meaning that the implementation here is only being used with
             *       Trie2Writable.  The code here is logically correct with any type
             *       of Trie2, however.
             *
             * @param c  The character to begin with.
             * @return   The last contiguous character with the same value.
             */
            private int RangeEndLS(char startingLS)
            {
                if (startingLS >= 0xdbff)
                {
                    return 0xdbff;
                }

                int c;
                int val = outerInstance.GetFromU16SingleLead(startingLS);
                for (c = startingLS + 1; c <= 0x0dbff; c++)
                {
                    if (outerInstance.GetFromU16SingleLead((char)c) != val)
                    {
                        break;
                    }
                }
                return c - 1;
            }

            

            //
            //   Iteration State Variables
            //
            private IValueMapper mapper;
            private Range returnValue = new Range();
            // The starting code point for the next range to be returned.
            private int nextStart;
            // The upper limit for the last normal range to be returned.  Normally 0x110000, but
            //   may be lower when iterating over the code points for a single lead surrogate.
            private int limitCP;

            // True while iterating over the the Trie2 values for code points.
            // False while iterating over the alternate values for lead surrogates.
            private bool doingCodePoints = true;

            // True if the iterator should iterate the special values for lead surrogates in
            //   addition to the normal values for code points.
            private bool doLeadSurrogates = true;
        }

        /**
         * Find the last character in a contiguous range of characters with the
         * same Trie2 value as the input character.
         *
         * @param c  The character to begin with.
         * @return   The last contiguous character with the same value.
         */
        internal virtual int RangeEnd(int start, int limitp, int val)
        {
            int c;
            int limit = Math.Min(highStart, limitp);

            for (c = start + 1; c < limit; c++)
            {
                if (Get(c) != val)
                {
                    break;
                }
            }
            if (c >= highStart)
            {
                c = limitp;
            }
            return c - 1;
        }


        //
        //  Hashing implementation functions.  FNV hash.  Respected public domain algorithm.
        //
        private static int InitHash()
        {
            return unchecked((int)0x811c9DC5);  // unsigned 2166136261
        }

        private static int HashByte(int h, int b)
        {
            h = h * 16777619;
            h = h ^ b;
            return h;
        }

        private static int HashUChar32(int h, int c)
        {
            h = Trie2.HashByte(h, c & 255);
            h = Trie2.HashByte(h, (c >> 8) & 255);
            h = Trie2.HashByte(h, c >> 16);
            return h;
        }

        private static int HashInt(int h, int i)
        {
            h = Trie2.HashByte(h, i & 255);
            h = Trie2.HashByte(h, (i >> 8) & 255);
            h = Trie2.HashByte(h, (i >> 16) & 255);
            h = Trie2.HashByte(h, (i >> 24) & 255);
            return h;
        }
    }
}
