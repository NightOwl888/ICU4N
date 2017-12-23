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
    /// This is the interface and common implementation of a Unicode <see cref="Trie2"/>.
    /// It is a kind of compressed table that maps from Unicode code points (0..0x10ffff)
    /// to 16- or 32-bit integer values.  It works best when there are ranges of
    /// characters with the same value, which is generally the case with Unicode
    /// character properties.
    /// <para/>
    /// This is the second common version of a Unicode trie (hence the name Trie2).
    /// </summary>
    public abstract class Trie2 : IEnumerable<Trie2.Range>
    {
        /// <summary>
        /// Create a <see cref="Trie2"/> from its serialized form.  Inverse of utrie2_serialize().
        /// </summary>
        /// <remarks>
        /// Reads from the current position and leaves the buffer after the end of the trie.
        /// <para/>
        /// The serialized format is identical between ICU4C, ICU4J, and ICU4N, so this function
        /// will work with serialized <see cref="Trie2"/>s from any.
        /// <para/>
        /// The actual type of the returned <see cref="Trie2"/> will be either <see cref="Trie2_16"/> or <see cref="Trie2_32"/>, depending
        /// on the width of the data.
        /// <para/>
        /// To obtain the width of the <see cref="Trie2"/>, check the actual class type of the returned <see cref="Trie2"/>.
        /// Or use the <see cref="Trie2_16.CreateFromSerialized(ByteBuffer)"/> or <see cref="Trie2_32.CreateFromSerialized(ByteBuffer)"/> method, which will
        /// return only <see cref="Trie"/>s of their specific type/size.
        /// <para/>
        /// The serialized <see cref="Trie2"/> on the stream may be in either little or big endian byte order.
        /// This allows using serialized <see cref="Trie"/>s from ICU4C without needing to consider the
        /// byte order of the system that created them.
        /// </remarks>
        /// <param name="bytes">A byte buffer to the serialized form of a UTrie2.</param>
        /// <returns>An unserialized <see cref="Trie2"/>, ready for use.</returns>
        /// <exception cref="ArgumentException">If the stream does not contain a serialized <see cref="Trie2"/>.</exception>
        /// <exception cref="IOException">If a read error occurs in the buffer.</exception>
        public static Trie2 CreateFromSerialized(ByteBuffer bytes) // ICU4N TODO: API Create overload that accepts byte[]
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

        /// <summary>
        /// Get the UTrie version from a <see cref="Stream"/> containing the serialized form
        /// of either a <see cref="Trie"/> (version 1) or a <see cref="Trie2"/> (version 2).
        /// </summary>
        /// <param name="input">A <see cref="Stream"/> containing the serialized form
        /// of a UTrie, version 1 or 2.  The stream must be seekable.
        /// The position of the input stream will be left unchanged.</param>
        /// <param name="littleEndianOk">If FALSE, only big-endian (Java native) serialized forms are recognized.
        /// If TRUE, little-endian serialized forms are recognized as well.</param>
        /// <returns>The Trie version of the serialized form, or 0 if it is not
        /// recognized as a serialized UTrie.</returns>
        /// <exception cref="IOException">On errors in reading from the input stream.</exception>
        public static int GetVersion(Stream input, bool littleEndianOk)
        {
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

        /// <summary>
        /// Get the value for a code point as stored in the <see cref="Trie2"/>.
        /// </summary>
        /// <param name="codePoint">The code point.</param>
        /// <returns>The value.</returns>
        abstract public int Get(int codePoint);

        /// <summary>
        /// Get the trie value for a UTF-16 code unit.
        /// </summary>
        /// <remarks>
        /// A <see cref="Trie2"/> stores two distinct values for input in the lead surrogate
        /// range, one for lead surrogates, which is the value that will be
        /// returned by this function, and a second value that is returned
        /// by <see cref="Trie2.Get(int)"/>.
        /// <para/>
        /// For code units outside of the lead surrogate range, this function
        /// returns the same result as <see cref="Trie2.Get(int)"/>.
        /// <para/>
        /// This function, together with the alternate value for lead surrogates,
        /// makes possible very efficient processing of UTF-16 strings without
        /// first converting surrogate pairs to their corresponding 32 bit code point
        /// values.
        /// <para/>
        /// At build-time, enumerate the contents of the <see cref="Trie2"/> to see if there
        /// is non-trivial (non-initialValue) data for any of the supplementary
        /// code points associated with a lead surrogate.
        /// If so, then set a special (application-specific) value for the
        /// lead surrogate code _unit_, with <see cref="Trie2Writable.SetForLeadSurrogateCodeUnit(char, int)"/>.
        /// <para/>
        /// At runtime, use <see cref="Trie2.GetFromU16SingleLead(char)"/>. If there is non-trivial
        /// data and the code unit is a lead surrogate, then check if a trail surrogate
        /// follows. If so, assemble the supplementary code point and look up its value
        /// with <see cref="Trie2.Get(int)"/>; otherwise reset the lead
        /// surrogate's value or do a code point lookup for it.
        /// <para/>
        /// If there is only trivial data for lead and trail surrogates, then processing
        /// can often skip them. For example, in normalization or case mapping
        /// all characters that do not have any mappings are simply copied as is.
        /// </remarks>
        /// <param name="c">The code point or lead surrogate value.</param>
        /// <returns>The value.</returns>
        abstract public int GetFromU16SingleLead(char c);

        /// <summary>
        /// Equals function.  Two <see cref="Trie"/>s are equal if their contents are equal.
        /// The type need not be the same, so a <see cref="Trie2Writable"/> will be equal to
        /// (read-only) <see cref="Trie2_16"/> or <see cref="Trie2_32"/> so long as they are storing the same values.
        /// </summary>
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

        /// <summary>
        /// When iterating over the contents of a <see cref="Trie2"/>, Elements of this type are produced.
        /// The iterator will return one item for each contiguous range of codepoints  having the same value.
        /// </summary>
        /// <remarks>
        /// When iterating, the same <see cref="Trie2EnumRange"/> object will be reused and returned for each range.
        /// If you need to retain complete iteration results, clone each returned <see cref="Trie2EnumRange"/>,
        /// or save the range in some other way, before advancing to the next iteration step.
        /// </remarks>
        public class Range // ICU4N TODO: API De-nest ?
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator(defaultValueMapper);
        }

        /// <summary>
        /// Create an enumerator over the value ranges in this <see cref="Trie2"/>.
        /// Values from the <see cref="Trie2"/> are not remapped or filtered, but are returned as they
        /// are stored in the <see cref="Trie2"/>.
        /// <para/>
        /// Note that this method was named iterator() in ICU4J.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator<Trie2.Range> GetEnumerator()
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

        /// <summary>
        /// Create an enumerator over the value ranges from this <see cref="Trie2"/>.
        /// Values from the <see cref="Trie2"/> are passed through a caller-supplied remapping function,
        /// and it is the remapped values that determine the ranges that
        /// will be produced by the iterator.
        /// <para/>
        /// Note that this method was named iterator(ValueMapper) in ICU4J.
        /// </summary>
        /// <param name="mapper">Provides a function to remap values obtained from the <see cref="Trie2"/>.</param>
        /// <returns>An enumerator.</returns>
        public virtual IEnumerator<Range> GetEnumerator(IValueMapper mapper)
        {
            return new Trie2Iterator(this, mapper);
        }

        /// <summary>
        /// Create an enumerator over the <see cref="Trie2"/> values for the 1024=0x400 code points
        /// corresponding to a given <paramref name="lead"/> surrogate.
        /// <para/>
        /// For example, for the lead surrogate U+D87E it will enumerate the values
        /// for [U+2F800..U+2FC00[.
        /// <para/>
        /// Note that this method was named iteratorForLeadSurrogate(char, ValueMapper) in ICU4J.
        /// </summary>
        /// <remarks>
        /// Used by data builder code that sets special lead surrogate code unit values
        /// for optimized UTF-16 string processing.
        /// <para/>
        /// Do not modify the <see cref="Trie2"/> during the iteration.
        /// <para/>
        /// Except for the limited code point range, this functions just like <see cref="Trie2.GetEnumerator()"/>.
        /// </remarks>
        public virtual IEnumerator<Range> GetEnumeratorForLeadSurrogate(char lead, IValueMapper mapper)
        {
            return new Trie2Iterator(this, lead, mapper);
        }

        /// <summary>
        /// Create an enumerator over the <see cref="Trie2"/> values for the 1024=0x400 code points
        /// corresponding to a given lead surrogate.
        /// <para/>
        /// For example, for the lead surrogate U+D87E it will enumerate the values
        /// for [U+2F800..U+2FC00[.
        /// <para/>
        /// Note that this method was named iteratorForLeadSurrogate(char) in ICU4J.
        /// </summary>
        /// <remarks>
        /// Used by data builder code that sets special lead surrogate code unit values
        /// for optimized UTF-16 string processing.
        /// <para/>
        /// Do not modify the <see cref="Trie2"/> during the iteration.
        /// <para/>
        /// Except for the limited code point range, this functions just like <see cref="Trie2.GetEnumerator()"/>.
        /// </remarks>
        public virtual IEnumerator<Range> GetEnumeratorForLeadSurrogate(char lead)
        {
            return new Trie2Iterator(this, lead, defaultValueMapper);
        }

        // ICU4N specific - de-nested IValueMapper

        /// <summary>
        /// Serialize a <see cref="Trie2"/> Header and Index onto a <see cref="DataOutputStream"/>.  This is
        /// common code used for both the <see cref="Trie2_16"/> and <see cref="Trie2_32"/> serialize functions.
        /// </summary>
        /// <param name="dos">The stream to which the serialized <see cref="Trie2"/> data will be written.</param>
        /// <returns>The number of bytes written.</returns>
        protected virtual int SerializeHeader(DataOutputStream dos) // ICU4N TODO: API Can this be converted to BinaryWriter or Stream (as was Trie2_16)?
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

        /// <summary>
        /// Struct-like class for holding the results returned by a UTrie2 <see cref="ICharSequence"/> iterator.
        /// The iteration walks over a <see cref="ICharSequence"/>, and for each Unicode code point therein
        /// returns the character and its associated <see cref="Trie2"/> value.
        /// </summary>
        public class CharSequenceValues // ICU4N TODO: API De-nest ?
        {
            /// <summary>String index of the current code point.</summary>
            public int Index { get; set; }
            /// <summary>The code point at index.</summary>
            public int CodePoint { get; set; }
            /// <summary>The <see cref="Trie2"/> value for the current code point.</summary>
            public int Value { get; set; }
        }

        /// <summary>
        /// Create an enumerator that will produce the values from the <see cref="Trie2"/> for
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
        /// Create an enumerator that will produce the values from the <see cref="Trie2"/> for
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
        /// Create an enumerator that will produce the values from the <see cref="Trie2"/> for
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

        /// <summary>
        /// An enumerator that operates over an input <see cref="ICharSequence"/>, and for each Unicode code point
        /// in the input returns the associated value from the <see cref="Trie2"/>.
        /// </summary>
        /// <remarks>
        /// This iterator can move forwards or backwards, and can be reset to an arbitrary index.
        /// <para/>
        /// Note that <see cref="Trie2_16"/> and <see cref="Trie2_32"/> subclass <see cref="Trie2.CharSequenceEnumerator"/>.  This is done
        /// only for performance reasons.  It does require that any changes made here be propagated
        /// into the corresponding code in the subclasses.
        /// </remarks>
        public class CharSequenceEnumerator : IEnumerator<CharSequenceValues> // ICU4N TODO: De-nest ?
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


        /// <summary>
        /// Selectors for the width of a UTrie2 data value.
        /// </summary>
        internal enum ValueWidth
        {
            BITS_16,
            BITS_32
        }

        /// <summary>
        /// Trie2 data structure in serialized form:
        /// <code>
        /// UTrie2Header header;
        /// uint16_t index[header.index2Length];
        /// uint16_t data[header.shiftedDataLength&lt;&lt;2];  -- or uint32_t data[...]
        /// </code>
        /// For .NET, this is read from the stream into an instance of UTrie2Header.
        /// (The C version just places a struct over the raw serialized data.)
        /// </summary>
        /// <internal/>
        internal class UTrie2Header
        {
            /// <summary>"Tri2" in big-endian US-ASCII (0x54726932)</summary>
            internal int signature;

            /// <summary>
            /// options bit field (uint16_t):
            /// <code>
            /// 15.. 4   reserved (0)
            /// 3.. 0   UTrie2ValueBits valueBits
            /// </code>
            /// </summary>
            internal int options;

            /// <summary>UTRIE2_INDEX_1_OFFSET..UTRIE2_MAX_INDEX_LENGTH  (uint16_t)</summary>
            internal int indexLength;

            /// <summary>(UTRIE2_DATA_START_OFFSET..UTRIE2_MAX_DATA_LENGTH)>>UTRIE2_INDEX_SHIFT  (uint16_t)</summary>
            internal int shiftedDataLength;

            /// <summary>Null index and data blocks, not shifted.  (uint16_t)</summary>
            internal int index2NullOffset, dataNullOffset;

            /// <summary>
            /// First code point of the single-value range ending with U+10ffff,
            /// rounded up and then shifted right by UTRIE2_SHIFT_1.  (uint16_t)
            /// </summary>
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

        /// <summary>Value returned for out-of-range code points and illegal UTF-8.</summary>
        internal int errorValue;

        /// <summary>Start of the last range which ends at U+10ffff, and its value.</summary>
        internal int highStart;
        internal int highValueIndex;

        internal int dataNullOffset;

        internal int fHash;              // Zero if not yet computed.
                                         //  Shared by Trie2Writable, Trie2_16, Trie2_32.
                                         //  Thread safety:  if two racing threads compute
                                         //     the same hash on a frozen Trie2, no damage is done.


        /// <summary>
        /// <see cref="Trie2"/> constants, defining shift widths, index array lengths, etc.
        /// <para/>
        /// These are needed for the runtime macros but users can treat these as
        /// implementation details and skip to the actual public API further below.
        /// </summary>
        internal static readonly int UTRIE2_OPTIONS_VALUE_BITS_MASK = 0x000f;


        /// <summary>Shift size for getting the index-1 table offset.</summary>
        internal static readonly int UTRIE2_SHIFT_1 = 6 + 5;

        /// <summary>Shift size for getting the index-2 table offset.</summary>
        internal static readonly int UTRIE2_SHIFT_2 = 5;

        /// <summary>
        /// Difference between the two shift sizes,
        /// for getting an index-1 offset from an index-2 offset. 6=11-5
        /// </summary>
        internal static readonly int UTRIE2_SHIFT_1_2 = UTRIE2_SHIFT_1 - UTRIE2_SHIFT_2;

        /// <summary>
        /// Number of index-1 entries for the BMP. 32=0x20
        /// This part of the index-1 table is omitted from the serialized form.
        /// </summary>
        internal static readonly int UTRIE2_OMITTED_BMP_INDEX_1_LENGTH = 0x10000 >> UTRIE2_SHIFT_1;

        /// <summary>Number of code points per index-1 table entry. 2048=0x800</summary>
        internal static readonly int UTRIE2_CP_PER_INDEX_1_ENTRY = 1 << UTRIE2_SHIFT_1;

        /// <summary>Number of entries in an index-2 block. 64=0x40</summary>
        internal static readonly int UTRIE2_INDEX_2_BLOCK_LENGTH = 1 << UTRIE2_SHIFT_1_2;

        /// <summary>Mask for getting the lower bits for the in-index-2-block offset.</summary>
        internal static readonly int UTRIE2_INDEX_2_MASK = UTRIE2_INDEX_2_BLOCK_LENGTH - 1;

        /// <summary>Number of entries in a data block. 32=0x20</summary>
        internal static readonly int UTRIE2_DATA_BLOCK_LENGTH = 1 << UTRIE2_SHIFT_2;

        /// <summary>Mask for getting the lower bits for the in-data-block offset.</summary>
        internal static readonly int UTRIE2_DATA_MASK = UTRIE2_DATA_BLOCK_LENGTH - 1;

        /// <summary>
        /// Shift size for shifting left the index array values.
        /// Increases possible data size with 16-bit index values at the cost
        /// of compactability.
        /// This requires data blocks to be aligned by <see cref="UTRIE2_DATA_GRANULARITY"/>.
        /// </summary>
        internal static readonly int UTRIE2_INDEX_SHIFT = 2;

        /// <summary>The alignment size of a data block. Also the granularity for compaction.</summary>
        internal static readonly int UTRIE2_DATA_GRANULARITY = 1 << UTRIE2_INDEX_SHIFT;

        /* Fixed layout of the first part of the index array. ------------------- */

        /// <summary>
        /// The BMP part of the index-2 table is fixed and linear and starts at offset 0.
        /// Length=2048=0x800=0x10000>><see cref="UTRIE2_SHIFT_2"/>.
        /// </summary>
        internal static readonly int UTRIE2_INDEX_2_OFFSET = 0;

        /// <summary>
        /// The part of the index-2 table for U+D800..U+DBFF stores values for
        /// lead surrogate code _units_ not code _points_.
        /// Values for lead surrogate code _points_ are indexed with this portion of the table.
        /// Length=32=0x20=0x400>><see cref="UTRIE2_SHIFT_2"/>. (There are 1024=0x400 lead surrogates.)
        /// </summary>
        internal static readonly int UTRIE2_LSCP_INDEX_2_OFFSET = 0x10000 >> UTRIE2_SHIFT_2;
        internal static readonly int UTRIE2_LSCP_INDEX_2_LENGTH = 0x400 >> UTRIE2_SHIFT_2;

        /// <summary>Count the lengths of both BMP pieces. 2080=0x820</summary>
        internal static readonly int UTRIE2_INDEX_2_BMP_LENGTH = UTRIE2_LSCP_INDEX_2_OFFSET + UTRIE2_LSCP_INDEX_2_LENGTH;

        /// <summary>
        /// The 2-byte UTF-8 version of the index-2 table follows at offset 2080=0x820.
        /// Length 32=0x20 for lead bytes C0..DF, regardless of <see cref="UTRIE2_SHIFT_2"/>.
        /// </summary>
        internal static readonly int UTRIE2_UTF8_2B_INDEX_2_OFFSET = UTRIE2_INDEX_2_BMP_LENGTH;
        internal static readonly int UTRIE2_UTF8_2B_INDEX_2_LENGTH = 0x800 >> 6;  /* U+0800 is the first code point after 2-byte UTF-8 */

        /// <summary>
        /// The index-1 table, only used for supplementary code points, at offset 2112=0x840.
        /// Variable length, for code points up to highStart, where the last single-value range starts.
        /// Maximum length 512=0x200=0x100000>><see cref="UTRIE2_SHIFT_1"/>.
        /// <para/>
        /// (For 0x100000 supplementary code points U+10000..U+10ffff.)
        /// <para/>
        /// The part of the index-2 table for supplementary code points starts
        /// after this index-1 table.
        /// <para/>
        /// Both the index-1 table and the following part of the index-2 table
        /// are omitted completely if there is only BMP data.
        /// </summary>
        internal static readonly int UTRIE2_INDEX_1_OFFSET = UTRIE2_UTF8_2B_INDEX_2_OFFSET + UTRIE2_UTF8_2B_INDEX_2_LENGTH;
        internal static readonly int UTRIE2_MAX_INDEX_1_LENGTH = 0x100000 >> UTRIE2_SHIFT_1;

        /*
         * Fixed layout of the first part of the data array. -----------------------
         * Starts with 4 blocks (128=0x80 entries) for ASCII.
         */

        /// <summary>
        /// The illegal-UTF-8 data block follows the ASCII block, at offset 128=0x80.
        /// Used with linear access for single bytes 0..0xbf for simple error handling.
        /// Length 64=0x40, not <see cref="UTRIE2_DATA_BLOCK_LENGTH"/>.
        /// </summary>
        internal static readonly int UTRIE2_BAD_UTF8_DATA_OFFSET = 0x80;

        /// <summary>The start of non-linear-ASCII data blocks, at offset 192=0xc0.</summary>
        internal static readonly int UTRIE2_DATA_START_OFFSET = 0xc0;

        /* Building a Trie2 ---------------------------------------------------------- */

        /*
         * These definitions are mostly needed by utrie2_builder.c, but also by
         * utrie2_get32() and utrie2_enum().
         */

        /// <summary>
        /// At build time, leave a gap in the index-2 table,
        /// at least as long as the maximum lengths of the 2-byte UTF-8 index-2 table
        /// and the supplementary index-1 table.
        /// Round up to <see cref="UTRIE2_INDEX_2_BLOCK_LENGTH"/> for proper compacting.
        /// </summary>
        internal static readonly int UNEWTRIE2_INDEX_GAP_OFFSET = UTRIE2_INDEX_2_BMP_LENGTH;
        internal static readonly int UNEWTRIE2_INDEX_GAP_LENGTH =
            ((UTRIE2_UTF8_2B_INDEX_2_LENGTH + UTRIE2_MAX_INDEX_1_LENGTH) + UTRIE2_INDEX_2_MASK) &
            ~UTRIE2_INDEX_2_MASK;

        /// <summary>
        /// Maximum length of the build-time index-2 array.
        /// Maximum number of Unicode code points (0x110000) shifted right by <see cref="UTRIE2_SHIFT_2"/>,
        /// plus the part of the index-2 table for lead surrogate code points,
        /// plus the build-time index gap,
        /// plus the null index-2 block.
        /// </summary>
        internal static readonly int UNEWTRIE2_MAX_INDEX_2_LENGTH =
            (0x110000 >> UTRIE2_SHIFT_2) +
            UTRIE2_LSCP_INDEX_2_LENGTH +
            UNEWTRIE2_INDEX_GAP_LENGTH +
            UTRIE2_INDEX_2_BLOCK_LENGTH;

        internal static readonly int UNEWTRIE2_INDEX_1_LENGTH = 0x110000 >> UTRIE2_SHIFT_1;

        /// <summary>
        /// Maximum length of the build-time data array.
        /// One entry per 0x110000 code points, plus the illegal-UTF-8 block and the null block,
        /// plus values for the 0x400 surrogate code units.
        /// </summary>
        internal static readonly int UNEWTRIE2_MAX_DATA_LENGTH = (0x110000 + 0x40 + 0x40 + 0x400);

        /// <summary>
        /// Implementation class for an enumerator over a <see cref="Trie2"/>.
        /// <para/>
        /// Iteration over a <see cref="Trie2"/> first returns all of the ranges that are indexed by code points,
        /// then returns the special alternate values for the lead surrogates.
        /// </summary>
        /// <internal/>
        public class Trie2Iterator : IEnumerator<Range> // ICU4N TODO: API - de-nest ? Rename Trie2Enumerator
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

            /// <summary>
            /// The main Next() function for <see cref="Trie2"/> iterators
            /// </summary>
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

            /// <summary>
            /// Find the last lead surrogate in a contiguous range  with the
            /// same <see cref="Trie2"/> value as the input character.
            /// </summary>
            /// <remarks>
            /// Use the alternate Lead Surrogate values from the Trie2,
            /// not the code-point values.
            /// <para/>
            /// Note: <see cref="Trie2_16"/> and <see cref="Trie2_32"/> override this implementation with optimized versions,
            ///       meaning that the implementation here is only being used with
            ///       <see cref="Trie2Writable"/>.  The code here is logically correct with any type
            ///       of <see cref="Trie2"/>, however.      
            /// </remarks>
            /// <param name="startingLS">The character to begin with.</param>
            /// <returns>The last contiguous character with the same value.</returns>
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
            /// <summary>The starting code point for the next range to be returned.</summary>
            private int nextStart;
            /// <summary>
            /// The upper limit for the last normal range to be returned.  Normally 0x110000, but
            /// may be lower when iterating over the code points for a single lead surrogate.
            /// </summary>
            private int limitCP;

            /// <summary>
            /// True while iterating over the the <see cref="Trie2"/> values for code points.
            /// False while iterating over the alternate values for lead surrogates.
            /// </summary>
            private bool doingCodePoints = true;
 
            /// <summary>
            /// True if the iterator should iterate the special values for lead surrogates in
            /// addition to the normal values for code points.
            /// </summary>
            private bool doLeadSurrogates = true;
        }

        /// <summary>
        /// Find the last character in a contiguous range of characters with the
        /// same <see cref="Trie2"/> value as the input character.
        /// </summary>
        /// <param name="start">The character to begin with.</param>
        /// <param name="limitp"></param>
        /// <param name="val"></param>
        /// <returns>The last contiguous character with the same value.</returns>
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
