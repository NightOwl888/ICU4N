using ICU4N.Support.Text;
using J2N.Text;
using System;
using System.Collections;

namespace ICU4N.Impl
{
    /// <summary>
    /// Implement the <see cref="ICharacterEnumerator"/> interface on a <see cref="ICharSequence"/>.
    /// Intended for internal use by ICU only.
    /// </summary>
    internal class CharSequenceCharacterEnumerator : ICharacterEnumerator
    {
        private int index;
        private readonly ICharSequence seq;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="text">The <see cref="ICharSequence"/> to iterate over.</param>
        public CharSequenceCharacterEnumerator(ICharSequence text)
        {
            seq = text ?? throw new ArgumentNullException(nameof(text));
            index = 0;
        }

        /// <summary>
        /// Gets the begin index. Returns the index of the first character of the iteration.
        /// </summary>
        public int StartIndex => 0;

        /// <summary>
        /// Gets the end index. Returns the index of the last character of the iteration.
        /// <para/>
        /// IMPORTANT: This property has .NET semantics. The index is the last index, not
        /// one past the last index.
        /// </summary>
        public int EndIndex => Math.Max(seq.Length - 1, 0);

        /// <inheritdoc/>
        public int Length => seq.Length;

        /// <inheritdoc/>
        public int Index
        {
            get => index;
            set
            {
                if (value < 0 || value > seq.Length - 1)
                    throw new ArgumentOutOfRangeException(nameof(value));
                index = value;
            }
        }

        /// <summary>
        /// Returns the character at the current index.
        /// </summary>
        public char Current => seq[index];

        object IEnumerator.Current => Current;

        /// <summary>
        /// Decrements the current index.
        /// </summary>
        /// <returns><c>true</c> if successful; <c>false</c> if there are no more characters in the sequence.</returns>
        public bool MovePrevious()
        {
            if (index == 0)
                return false;

            --index;
            return true;
        }

        /// <summary>
        /// Sets the current position to the begin index.
        /// </summary>
        /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
        public bool MoveFirst()
        {
            if (seq.Length <= 0)
                return false;

            index = 0;
            return true;
        }

        /// <summary>
        /// Sets the current position to the end index.
        /// </summary>
        /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
        public bool MoveLast()
        {
            if (seq.Length <= 0)
                return false;

            index = seq.Length - 1;
            return true;
        }

        /// <inheritdoc/>
        public bool TrySetIndex(int value)
        {
            if (value < StartIndex)
            {
                index = StartIndex;
                return false;
            }
            if (value > EndIndex)
            {
                index = EndIndex;
                return false;
            }
            index = value;
            return true;
        }

        /// <summary>
        /// Increments the current index.
        /// </summary>
        /// <returns><c>true</c> if successful; <c>false</c> if there are no more characters in the sequence.</returns>
        public bool MoveNext()
        {
            if (index < seq.Length - 1)
            {
                ++index;
                return true;
            }
            return false;
        }

        void IEnumerator.Reset()
        {
            index = 0;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Nothing to do
        }

#if FEATURE_CLONEABLE
        /// <summary>
        /// Returns a new <see cref="CharSequenceCharacterEnumerator"/> with the same properties.
        /// </summary>
        /// <returns>A shallow copy of this character enumerator.</returns>
        /// <seealso cref="ICloneable"/>
#else
        /// <summary>
        /// Returns a new <see cref="CharSequenceCharacterEnumerator"/> with the same properties.
        /// </summary>
        /// <returns>A shallow copy of this character enumerator.</returns>
#endif
        public object Clone()
        {
            return new CharSequenceCharacterEnumerator(seq)
            {
                Index = index
            };
        }
    }
}
