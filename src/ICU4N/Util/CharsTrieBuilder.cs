using J2N.IO;
using J2N.Text;
using System;
using System.Diagnostics;

namespace ICU4N.Util
{
    /// <summary>
    /// Builder class for <see cref="CharsTrie"/>.
    /// <para/>
    /// This class is not intended for public subclassing.
    /// </summary>
    /// <stable>ICU 4.8</stable>
    /// <author>Markus W. Scherer</author>
    public sealed partial class CharsTrieBuilder : StringTrieBuilder
    {
        /// <summary>
        /// Constructs an empty builder.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public CharsTrieBuilder()
#pragma warning disable 612, 618
            : base()
#pragma warning restore 612, 618
        { }

        /// <summary>
        /// Builds a <see cref="CharsTrie"/> for the <see cref="Add(string, int)"/>ed data.
        /// Once built, no further data can be <see cref="Add(string, int)"/>ed until <see cref="Clear()"/> is called.
        /// </summary>
        /// <remarks>
        /// A <see cref="CharsTrie"/> cannot be empty. At least one (string, value) pair
        /// must have been <see cref="Add(string, int)"/>ed.
        /// <para/>
        /// Multiple calls to <see cref="Build(TrieBuilderOption)"/> or <see cref="BuildCharSequence(TrieBuilderOption)"/> return tries or sequences
        /// which share the builder's char array, without rebuilding.
        /// After <see cref="Clear()"/> has been called, a new array will be used.
        /// </remarks>
        /// <param name="buildOption">Build option, see <see cref="TrieBuilderOption"/>.</param>
        /// <returns>A new <see cref="CharsTrie"/> for the <see cref="Add(string, int)"/>ed data.</returns>
        /// <stable>ICU 4.8</stable>
        public CharsTrie Build(TrieBuilderOption buildOption)
        {
            return new CharsTrie(BuildCharSequence(buildOption), 0);
        }

        /// <summary>
        /// Builds a <see cref="CharsTrie"/> for the <see cref="Add(string, int)"/>ed data and char-serializes it.
        /// Once built, no further data can be <see cref="Add(string, int)"/>ed until <see cref="Clear()"/> is called.
        /// </summary>
        /// <remarks>
        /// A <see cref="CharsTrie"/> cannot be empty. At least one (string, value) pair
        /// must have been <see cref="Add(string, int)"/>ed.
        /// <para/>
        /// Multiple calls to <see cref="Build(TrieBuilderOption)"/> or <see cref="BuildCharSequence(TrieBuilderOption)"/> return tries or sequences
        /// which share the builder's char array, without rebuilding.
        /// After <see cref="Clear()"/> has been called, a new array will be used.
        /// </remarks>
        /// <param name="buildOption">Build option, see <see cref="TrieBuilderOption"/>.</param>
        /// <returns>A <see cref="ICharSequence"/> with the char-serialized <see cref="CharsTrie"/> for the <see cref="Add(string, int)"/>ed data.</returns>
        /// <stable>ICU 4.8</stable>
        public ICharSequence BuildCharSequence(TrieBuilderOption buildOption)
        {
            BuildChars(buildOption);
            return CharBuffer.Wrap(chars, chars.Length - charsLength, charsLength);
        }

        private void BuildChars(TrieBuilderOption buildOption)
        {
            // Create and char-serialize the trie for the elements.
            if (chars == null)
            {
                chars = new char[1024];
            }
#pragma warning disable 612, 618
            BuildImpl(buildOption);
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Removes all (string, value) pairs.
        /// New data can then be <see cref="Add(string, int)"/>ed and a new trie can be built.
        /// </summary>
        /// <returns>This.</returns>
        /// <stable>ICU 4.8</stable>
        public CharsTrieBuilder Clear()
        {
#pragma warning disable 612, 618
            ClearImpl();
#pragma warning restore 612, 618
            chars = null;
            charsLength = 0;
            return this;
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected override bool MatchNodesCanHaveValues /*const*/ { get { return true; } }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected override int MaxBranchLinearSubNodeLength /*const*/ { get { return CharsTrie.kMaxBranchLinearSubNodeLength; } }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected override int MinLinearMatch /*const*/{ get { return CharsTrie.kMinLinearMatch; } }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected override int MaxLinearMatchLength /*const*/{ get { return CharsTrie.kMaxLinearMatchLength; } }

        private void EnsureCapacity(int length)
        {
            if (length > chars.Length)
            {
                int newCapacity = chars.Length;
                do
                {
                    newCapacity *= 2;
                } while (newCapacity <= length);
                char[] newChars = new char[newCapacity];
                System.Array.Copy(chars, chars.Length - charsLength,
                                 newChars, newChars.Length - charsLength, charsLength);
                chars = newChars;
            }
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected override int Write(int unit)
        {
            int newLength = charsLength + 1;
            EnsureCapacity(newLength);
            charsLength = newLength;
            chars[chars.Length - charsLength] = (char)unit;
            return charsLength;
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected override int Write(int offset, int length)
        {
            int newLength = charsLength + length;
            EnsureCapacity(newLength);
            charsLength = newLength;
            int charsOffset = chars.Length - charsLength;
            while (length > 0)
            {
                chars[charsOffset++] = strings[offset++];
                --length;
            }
            return charsLength;
        }
        private int Write(char[] s, int length)
        {
            int newLength = charsLength + length;
            EnsureCapacity(newLength);
            charsLength = newLength;
            System.Array.Copy(s, 0, chars, chars.Length - charsLength, length);
            return charsLength;
        }

        // For writeValueAndFinal(), writeValueAndType() and writeDeltaTo().
        private readonly char[] intUnits = new char[3];

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected override int WriteValueAndFinal(int i, bool isFinal)
        {
            if (0 <= i && i <= CharsTrie.kMaxOneUnitValue)
            {
                return Write(i | (isFinal ? CharsTrie.kValueIsFinal : 0));
            }
            int length;
            if (i < 0 || i > CharsTrie.kMaxTwoUnitValue)
            {
                intUnits[0] = (char)(CharsTrie.kThreeUnitValueLead);
                intUnits[1] = (char)(i >> 16);
                intUnits[2] = (char)i;
                length = 3;
                // } else if(i<=CharsTrie.kMaxOneUnitValue) {
                //     intUnits[0]=(char)(i);
                //     length=1;
            }
            else
            {
                intUnits[0] = (char)(CharsTrie.kMinTwoUnitValueLead + (i >> 16));
                intUnits[1] = (char)i;
                length = 2;
            }
            intUnits[0] = (char)(intUnits[0] | (isFinal ? CharsTrie.kValueIsFinal : 0));
            return Write(intUnits, length);
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected override int WriteValueAndType(bool hasValue, int value, int node)
        {
            if (!hasValue)
            {
                return Write(node);
            }
            int length;
            if (value < 0 || value > CharsTrie.kMaxTwoUnitNodeValue)
            {
                intUnits[0] = (char)(CharsTrie.kThreeUnitNodeValueLead);
                intUnits[1] = (char)(value >> 16);
                intUnits[2] = (char)value;
                length = 3;
            }
            else if (value <= CharsTrie.kMaxOneUnitNodeValue)
            {
                intUnits[0] = (char)((value + 1) << 6);
                length = 1;
            }
            else
            {
                intUnits[0] = (char)(CharsTrie.kMinTwoUnitNodeValueLead + ((value >> 10) & 0x7fc0));
                intUnits[1] = (char)value;
                length = 2;
            }
            intUnits[0] |= (char)node;
            return Write(intUnits, length);
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected override int WriteDeltaTo(int jumpTarget)
        {
            int i = charsLength - jumpTarget;
            Debug.Assert(i >= 0);
            if (i <= CharsTrie.kMaxOneUnitDelta)
            {
                return Write(i);
            }
            int length;
            if (i <= CharsTrie.kMaxTwoUnitDelta)
            {
                intUnits[0] = (char)(CharsTrie.kMinTwoUnitDeltaLead + (i >> 16));
                length = 1;
            }
            else
            {
                intUnits[0] = (char)(CharsTrie.kThreeUnitDeltaLead);
                intUnits[1] = (char)(i >> 16);
                length = 2;
            }
            intUnits[length++] = (char)i;
            return Write(intUnits, length);
        }

        // char serialization of the trie.
        // Grows from the back: charsLength measures from the end of the buffer!
        private char[] chars;
        private int charsLength;
    }
}
