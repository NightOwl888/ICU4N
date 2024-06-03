using J2N.IO;
using J2N.Text;
using System;
using System.Buffers;
using System.Diagnostics;

namespace ICU4N.Util
{
    /// <summary>
    /// Builder class for <see cref="BytesTrie"/>.
    /// <para/>
    /// This class is not intended for public subclassing.
    /// </summary>
    /// <stable>ICU 4.8</stable>
    /// <author>Markus W. Scherer</author>
    public sealed class BytesTrieBuilder : StringTrieBuilder
    {
        private const int CharStackBufferSize = 32;

        /// <summary>
        /// Constructs an empty builder.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public BytesTrieBuilder()
#pragma warning disable 612, 618
            : base()
#pragma warning restore 612, 618
        { }

        /// <summary>
        /// Adds a (byte sequence, value) pair.
        /// The byte sequence must be unique.
        /// Bytes 0..length-1 will be copied; the builder does not keep
        /// a reference to the input array.
        /// </summary>
        /// <param name="sequence">The array that contains the byte sequence, starting at index 0.</param>
        /// <param name="length">The length of the byte sequence.</param>
        /// <param name="value">The value associated with this byte sequence.</param>
        /// <returns>This.</returns>
        /// <stable>ICU 4.8</stable>
        public BytesTrieBuilder Add(byte[] sequence, int length, int value) // ICU4N TODO: API - add guard clauses for length and null
        {
            bool usePool = length <= CharStackBufferSize;
            char[] arrayToReturnToPool = usePool ? ArrayPool<char>.Shared.Rent(length) : null;
            Span<char> chars = usePool ? arrayToReturnToPool.AsSpan(0, length) : stackalloc char[length];
            try
            {
                for (int i = 0; i < length; i++)
                {
                    chars[i] = (char)(sequence[i] & 0xff);
                }

#pragma warning disable 612, 618
                AddImpl(chars, value);
#pragma warning restore 612, 618
            }
            finally
            {
                if (arrayToReturnToPool is not null)
                    ArrayPool<char>.Shared.Return(arrayToReturnToPool);
            }
            return this;
        }

        /// <summary>
        /// Builds a <see cref="BytesTrie"/> for the <see cref="Add(byte[], int, int)"/> appended data.
        /// Once built, no further data can be added until <see cref="Clear()"/> is called.
        ///
        /// <para/>A <see cref="BytesTrie"/> cannot be empty. At least one (byte sequence, value) pair
        /// must have been added.
        ///
        /// <para/>Multiple calls to <see cref="Build(TrieBuilderOption)"/> or <see cref="BuildByteBuffer(TrieBuilderOption)"/> return tries or buffers
        /// which share the builder's byte array, without rebuilding.
        /// <em>The byte array must not be modified via the <see cref="BuildByteBuffer(TrieBuilderOption)"/> result object.</em>
        /// After <see cref="Clear()"/> has been called, a new array will be used.
        /// </summary>
        /// <param name="buildOption">Build option, see <see cref="TrieBuilderOption"/>.</param>
        /// <returns> A new <see cref="BytesTrie"/> for the added data.</returns>
        /// <stable>ICU 4.8</stable>
        public BytesTrie Build(TrieBuilderOption buildOption)
        {
            BuildBytes(buildOption);
            return new BytesTrie(bytes, bytes.Length - bytesLength);
        }

        /// <summary>
        /// Builds a <see cref="BytesTrie"/> for the <see cref="Add(byte[], int, int)"/> appended data and byte-serializes it.
        /// Once built, no further data can be added until <see cref="Clear()"/> is called.
        /// <para/>A <see cref="BytesTrie"/> cannot be empty. At least one (byte sequence, value) pair
        /// must have been added.
        ///
        /// <para/>Multiple calls to <see cref="Build(TrieBuilderOption)"/> or <see cref="BuildByteBuffer(TrieBuilderOption)"/> return tries or buffers
        /// which share the builder's byte array, without rebuilding.
        /// <em>Do not modify the bytes in the buffer!</em>
        /// After <see cref="Clear()"/> has been called, a new array will be used.
        ///
        /// <para/>The serialized <see cref="BytesTrie"/> is accessible via the buffer's
        /// <see cref="ByteBuffer.Array"/>/<see cref="ByteBuffer.ArrayOffset"/>+Position or Remaining/Get(byte[]) etc.
        /// </summary>
        /// <param name="buildOption">Build option, see <see cref="TrieBuilderOption"/>.</param>
        /// <returns>A <see cref="ByteBuffer"/> with the byte-serialized <see cref="BytesTrie"/> for the added data.
        /// The buffer is not read-only and <see cref="ByteBuffer.Array"/> can be called.</returns>
        /// <stable>ICU 4.8</stable>
        public ByteBuffer BuildByteBuffer(TrieBuilderOption buildOption)
        {
            BuildBytes(buildOption);
            return ByteBuffer.Wrap(bytes, bytes.Length - bytesLength, bytesLength);
        }

        private void BuildBytes(TrieBuilderOption buildOption)
        {
            // Create and byte-serialize the trie for the elements.
            if (bytes == null)
            {
                bytes = new byte[1024];
            }
#pragma warning disable 612, 618
            BuildImpl(buildOption);
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Removes all (byte sequence, value) pairs.
        /// New data can then be added and a new trie can be built.
        /// </summary>
        /// <returns>This.</returns>
        /// <stable>ICU 4.8</stable>
        public BytesTrieBuilder Clear()
        {
#pragma warning disable 612, 618
            ClearImpl();
#pragma warning restore 612, 618
            bytes = null;
            bytesLength = 0;
            return this;
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal override bool MatchNodesCanHaveValues /*const*/ => false; // ICU4N specific - marked internal instead of protected, since the functionality is obsolete

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal override int MaxBranchLinearSubNodeLength /*const*/ => BytesTrie.kMaxBranchLinearSubNodeLength; // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal override int MinLinearMatch /*const*/ => BytesTrie.kMinLinearMatch; // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal override int MaxLinearMatchLength /*const*/ => BytesTrie.kMaxLinearMatchLength; // ICU4N specific - marked internal instead of protected, since the functionality is obsolete

        private void EnsureCapacity(int length)
        {
            if (length > bytes.Length)
            {
                int newCapacity = bytes.Length;
                do
                {
                    newCapacity *= 2;
                } while (newCapacity <= length);
                byte[] newBytes = new byte[newCapacity];
                System.Array.Copy(bytes, bytes.Length - bytesLength,
                                 newBytes, newBytes.Length - bytesLength, bytesLength);
                bytes = newBytes;
            }
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal override int Write(int b) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            int newLength = bytesLength + 1;
            EnsureCapacity(newLength);
            bytesLength = newLength;
            bytes[bytes.Length - bytesLength] = (byte)b;
            return bytesLength;
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal override int Write(int offset, int length) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            int newLength = bytesLength + length;
            EnsureCapacity(newLength);
            bytesLength = newLength;
            int bytesOffset = bytes.Length - bytesLength;
            while (length > 0)
            {
                bytes[bytesOffset++] = (byte)strings[offset++];
                --length;
            }
            return bytesLength;
        }
        private int Write(byte[] b, int length)
        {
            int newLength = bytesLength + length;
            EnsureCapacity(newLength);
            bytesLength = newLength;
            System.Array.Copy(b, 0, bytes, bytes.Length - bytesLength, length);
            return bytesLength;
        }

        // For writeValueAndFinal() and writeDeltaTo().
        private readonly byte[] intBytes = new byte[5];

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal override int WriteValueAndFinal(int i, bool isFinal) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            if (0 <= i && i <= BytesTrie.kMaxOneByteValue)
            {
                return Write(((BytesTrie.kMinOneByteValueLead + i) << 1) | (isFinal ? 1 : 0));
            }
            int length = 1;
            if (i < 0 || i > 0xffffff)
            {
                intBytes[0] = (byte)BytesTrie.kFiveByteValueLead;
                intBytes[1] = (byte)(i >> 24);
                intBytes[2] = (byte)(i >> 16);
                intBytes[3] = (byte)(i >> 8);
                intBytes[4] = (byte)i;
                length = 5;
                // } else if(i<=BytesTrie.kMaxOneByteValue) {
                //     intBytes[0]=(byte)(BytesTrie.kMinOneByteValueLead+i);
            }
            else
            {
                if (i <= BytesTrie.kMaxTwoByteValue)
                {
                    intBytes[0] = (byte)(BytesTrie.kMinTwoByteValueLead + (i >> 8));
                }
                else
                {
                    if (i <= BytesTrie.kMaxThreeByteValue)
                    {
                        intBytes[0] = (byte)(BytesTrie.kMinThreeByteValueLead + (i >> 16));
                    }
                    else
                    {
                        intBytes[0] = (byte)BytesTrie.kFourByteValueLead;
                        intBytes[1] = (byte)(i >> 16);
                        length = 2;
                    }
                    intBytes[length++] = (byte)(i >> 8);
                }
                intBytes[length++] = (byte)i;
            }
            intBytes[0] = (byte)((intBytes[0] << 1) | (isFinal ? 1 : 0));
            return Write(intBytes, length);
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal override int WriteValueAndType(bool hasValue, int value, int node) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            int offset = Write(node);
            if (hasValue)
            {
                offset = WriteValueAndFinal(value, false);
            }
            return offset;
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal override int WriteDeltaTo(int jumpTarget) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            int i = bytesLength - jumpTarget;
            Debug.Assert(i >= 0);
            if (i <= BytesTrie.kMaxOneByteDelta)
            {
                return Write(i);
            }
            int length;
            if (i <= BytesTrie.kMaxTwoByteDelta)
            {
                intBytes[0] = (byte)(BytesTrie.kMinTwoByteDeltaLead + (i >> 8));
                length = 1;
            }
            else
            {
                if (i <= BytesTrie.kMaxThreeByteDelta)
                {
                    intBytes[0] = (byte)(BytesTrie.kMinThreeByteDeltaLead + (i >> 16));
                    length = 2;
                }
                else
                {
                    if (i <= 0xffffff)
                    {
                        intBytes[0] = (byte)BytesTrie.kFourByteDeltaLead;
                        length = 3;
                    }
                    else
                    {
                        intBytes[0] = (byte)BytesTrie.kFiveByteDeltaLead;
                        intBytes[1] = (byte)(i >> 24);
                        length = 4;
                    }
                    intBytes[1] = (byte)(i >> 16);
                }
                intBytes[1] = (byte)(i >> 8);
            }
            intBytes[length++] = (byte)i;
            return Write(intBytes, length);
        }

        // Byte serialization of the trie.
        // Grows from the back: bytesLength measures from the end of the buffer!
        private byte[] bytes;
        private int bytesLength;
    }
}
