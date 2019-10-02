using ICU4N.Support.Text;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ICU4N.Support.IO
{
    /// <summary>
    /// A buffer of chars.
    /// </summary>
    /// <remarks>
    /// A char buffer can be created in either one of the following ways:
    /// <list type="bullet">
    ///     <item><description><see cref="Allocate(int)"/> a new char array and create a buffer based on it;</description></item>
    ///     <item><description><see cref="Wrap(char[])"/> an existing char array to create a new buffer;</description></item>
    ///     <item><description><see cref="Wrap(ICharSequence)"/> an existing char sequence to create a new buffer;</description></item>
    ///     <item><description>Use <see cref="ByteBuffer.AsCharBuffer()"/> to create a char buffer based on a byte buffer.</description></item>
    /// </list>
    /// </remarks>
    internal abstract class CharBuffer : Buffer, IComparable<CharBuffer>, ICharSequence // TODO: API Make this public when ICharSequence is made public
    {
        /// <summary>
        /// Creates a char buffer based on a newly allocated char array.
        /// </summary>
        /// <param name="capacity">The capacity of the new buffer.</param>
        /// <returns>The created char buffer.</returns>
        /// <exception cref="ArgumentException">If <paramref name="capacity"/> is less than 0.</exception>
        public static CharBuffer Allocate(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentException();
            }
            return new ReadWriteCharArrayBuffer(capacity);
        }

        /// <summary>
        /// Creates a new char buffer by wrapping the given char array.
        /// <para/>
        /// Calling this method has the same effect as
        /// <c>Wrap(array, 0, array.Length)</c>.
        /// </summary>
        /// <param name="array">The char array which the new buffer will be based on.</param>
        /// <returns>The created char buffer.</returns>
        public static CharBuffer Wrap(char[] array)
        {
            return Wrap(array, 0, array.Length);
        }

        /// <summary>
        /// Creates a new char buffer by wrapping the given char array.
        /// <para/>
        /// The new buffer's position will be <paramref name="start"/>, limit will be
        /// <c>start + length</c>, capacity will be the length of the array.
        /// </summary>
        /// <param name="array">The char array which the new buffer will be based on.</param>
        /// <param name="start">The start index, must not be negative and not greater than
        /// <c>array.Length</c>.</param>
        /// <param name="length">The length, must not be negative and not greater than
        /// <c>array.Length - start</c>.</param>
        /// <returns>The created char buffer.</returns>
        /// <exception cref="IndexOutOfRangeException">If either <paramref name="start"/> or <paramref name="length"/> is invalid.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="array"/> is <c>null</c>.</exception>
        public static CharBuffer Wrap(char[] array, int start, int length)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            int len = array.Length;
            if ((start < 0) || (length < 0) || (long)start + (long)length > len)
            {
                throw new IndexOutOfRangeException();
            }

            CharBuffer buf = new ReadWriteCharArrayBuffer(array);
            buf.position = start;
            buf.limit = start + length;

            return buf;
        }

        /// <summary>
        /// Creates a new char buffer by wrapping the given char sequence.
        /// <para/>
        /// Calling this method has the same effect as
        /// <c>Wrap(characterSequence, 0, characterSequence.Length)</c>.
        /// </summary>
        /// <param name="characterSequence">The char sequence which the new buffer will be based on.</param>
        /// <returns>The created char buffer.</returns>
        public static CharBuffer Wrap(ICharSequence characterSequence)
        {
            return new CharSequenceAdapter(characterSequence);
        }

        /// <summary>
        /// Creates a new char buffer by wrapping the given char sequence, <paramref name="characterSequence"/>.
        /// <para/>
        /// The new buffer's position will be <paramref name="start"/>, limit will be
        /// <paramref name="end"/>, capacity will be the length of the char sequence. The new
        /// buffer is read-only.
        /// </summary>
        /// <param name="characterSequence">The char sequence which the new buffer will be based on.</param>
        /// <param name="start">The start index, must not be negative and not greater than <c>characterSequence.Length</c>.</param>
        /// <param name="end">The end index, must be no less than <paramref name="start"/> and no
        /// greater than <c>characterSequence.Length</c>.</param>
        /// <returns>The created char buffer.</returns>
        /// <exception cref="IndexOutOfRangeException">If either <paramref name="start"/> or <paramref name="end"/> is invalid.</exception>
        public static CharBuffer Wrap(ICharSequence characterSequence, int start, int end)
        {
            if (characterSequence == null)
            {
                throw new ArgumentNullException(nameof(characterSequence));
            }
            if (start < 0 || end < start || end > characterSequence.Length)
            {
                throw new IndexOutOfRangeException();
            }

            CharBuffer result = new CharSequenceAdapter(characterSequence);
            result.position = start;
            result.limit = end;
            return result;
        }

        /// <summary>
        /// Constructs a <see cref="CharBuffer"/> with given capacity.
        /// </summary>
        /// <param name="capacity">The capacity of the buffer.</param>
        internal CharBuffer(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Gets the char array which this buffer is based on, if there is one.
        /// </summary>
        /// <exception cref="ReadOnlyBufferException">If this buffer is based on an array, but it is read-only.</exception>
        /// <exception cref="NotSupportedException">If this buffer is not based on an array.</exception>
        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        public char[] Array
        {
            get { return ProtectedArray; }
        }

        /// <summary>
        /// Gets the offset of the char array which this buffer is based on, if
        /// there is one.
        /// <para/>
        /// The offset is the index of the array corresponds to the zero position of
        /// the buffer.
        /// </summary>
        /// <exception cref="ReadOnlyBufferException">If this buffer is based on an array but it is read-only.</exception>
        /// <exception cref="NotSupportedException">If this buffer is not based on an array.</exception>
        public int ArrayOffset
        {
            get { return ProtectedArrayOffset; }
        }

        /// <summary>
        /// Returns a read-only buffer that shares its content with this buffer.
        /// <para/>
        /// The returned buffer is guaranteed to be a new instance, even if this
        /// buffer is read-only itself. The new buffer's position, limit, capacity
        /// and mark are the same as this buffer's.
        /// <para/>
        /// The new buffer shares its content with this buffer, which means this
        /// buffer's change of content will be visible to the new buffer. The two
        /// buffer's position, limit and mark are independent.
        /// </summary>
        /// <returns>A read-only version of this buffer.</returns>
        public abstract CharBuffer AsReadOnlyBuffer();

        /// <summary>
        /// Returns the character located at the specified <paramref name="index"/> in the buffer. The
        /// <paramref name="index"/> value is referenced from the current buffer position.
        /// </summary>
        /// <param name="index">The index referenced from the current buffer position. It must
        /// not be less than zero but less than the value obtained from a
        /// call to <see cref="Buffer.Remaining"/>.</param>
        /// <returns>The character located at the specified <paramref name="index"/> (referenced from the
        /// current position) in the buffer.</returns>
        /// <exception cref="IndexOutOfRangeException">If the <paramref name="index"/> is invalid.</exception>
        public char this[int index]
        {
            get
            {
                if (index < 0 || index >= Remaining)
                {
                    throw new IndexOutOfRangeException();
                }
                return Get(position + index);
            }
        }

        /// <summary>
        /// Compacts this char buffer.
        /// <para/>
        /// The remaining chars will be moved to the head of the buffer,
        /// starting from position zero. Then the position is set to
        /// <see cref="Buffer.Remaining"/>; the limit is set to capacity; the mark is cleared.
        /// </summary>
        /// <returns>This buffer.</returns>
        /// <exception cref="ReadOnlyBufferException">If no changes may be made to the contents of this buffer.</exception>
        public abstract CharBuffer Compact();

        /// <summary>
        /// Compare the remaining chars of this buffer to another char
        /// buffer's remaining chars.
        /// </summary>
        /// <param name="otherBuffer">Another char buffer.</param>
        /// <returns>A negative value if this is less than <paramref name="otherBuffer"/>; 0 if
        /// this equals to <paramref name="otherBuffer"/>; a positive valie if this is
        /// greater than <paramref name="otherBuffer"/>.</returns>
        public virtual int CompareTo(CharBuffer otherBuffer)
        {
            int compareRemaining = (Remaining < otherBuffer.Remaining) ? Remaining
                    : otherBuffer.Remaining;
            int thisPos = position;
            int otherPos = otherBuffer.position;
            char thisByte, otherByte;
            while (compareRemaining > 0)
            {
                thisByte = Get(thisPos);
                otherByte = otherBuffer.Get(otherPos);
                if (thisByte != otherByte)
                {
                    return thisByte < otherByte ? -1 : 1;
                }
                thisPos++;
                otherPos++;
                compareRemaining--;
            }
            return Remaining - otherBuffer.Remaining;
        }

        /// <summary>
        /// Returns a duplicated buffer that shares its content with this buffer.
        /// <para/>
        /// The duplicated buffer's initial position, limit, capacity and mark are
        /// the same as this buffer's. The duplicated buffer's read-only property and
        /// byte order are the same as this buffer's, too.
        /// <para/>
        /// The new buffer shares its content with this buffer, which means either
        /// buffer's change of content will be visible to the other. The two buffer's
        /// position, limit and mark are independent.
        /// </summary>
        /// <returns>A duplicated buffer that shares its content with this buffer.</returns>
        public abstract CharBuffer Duplicate();

        /// <summary>
        /// Checks whether this char buffer is equal to another object.
        /// <para/>
        /// If <paramref name="other"/> is not a char buffer then <c>false</c> is returned. Two
        /// char buffers are equal if and only if their remaining chars are exactly
        /// the same. Position, limit, capacity and mark are not considered.
        /// </summary>
        /// <param name="other">The object to compare with this char buffer.</param>
        /// <returns><c>true</c> if this char buffer is equal to <paramref name="other"/>, <c>false</c> otherwise.</returns>
        public override bool Equals(object other)
        {
            if (!(other is CharBuffer))
            {
                return false;
            }
            CharBuffer otherBuffer = (CharBuffer)other;

            if (Remaining != otherBuffer.Remaining)
            {
                return false;
            }

            int myPosition = position;
            int otherPosition = otherBuffer.position;
            bool equalSoFar = true;
            while (equalSoFar && (myPosition < limit))
            {
                equalSoFar = Get(myPosition++) == otherBuffer.Get(otherPosition++);
            }

            return equalSoFar;
        }

        /// <summary>
        /// Returns the char at the current position and increases the position by 1.
        /// </summary>
        /// <returns>The char at the current position.</returns>
        /// <exception cref="BufferUnderflowException">If the position is equal or greater than limit.</exception>
        public abstract char Get();

        /// <summary>
        /// Reads chars from the current position into the specified char array and
        /// increases the position by the number of chars read.
        /// <para/>
        /// Calling this method has the same effect as <c>Get(destination, 0, destination.Length)</c>.
        /// </summary>
        /// <param name="destination">The destination char array.</param>
        /// <returns>This buffer.</returns>
        /// <exception cref="BufferUnderflowException">If <c>destination.Length</c> is greater than <see cref="Buffer.Remaining"/>.</exception>
        public virtual CharBuffer Get(char[] destination)
        {
            return Get(destination, 0, destination.Length);
        }

        /// <summary>
        /// Reads chars from the current position into the specified char array,
        /// starting from the specified offset, and increases the position by the
        /// number of chars read.
        /// </summary>
        /// <param name="destination">The target char array.</param>
        /// <param name="offset">The offset of the char array, must not be negative and not
        /// greater than <c>destination.Length</c>.</param>
        /// <param name="length">The number of chars to read, must be no less than zero and no
        /// greater than <c>destination.Length - offset</c>.</param>
        /// <returns>This buffer.</returns>
        /// <exception cref="IndexOutOfRangeException">If either <paramref name="offset"/> or <paramref name="length"/> is invalid.</exception>
        /// <exception cref="BufferUnderflowException">If <paramref name="length"/> is greater than <see cref="Buffer.Remaining"/>.</exception>
        public virtual CharBuffer Get(char[] destination, int offset, int length)
        {
            int len = destination.Length;
            if ((offset < 0) || (length < 0) || (long)offset + (long)length > len)
            {
                throw new IndexOutOfRangeException();
            }

            if (length > Remaining)
            {
                throw new BufferUnderflowException();
            }
            for (int i = offset; i < offset + length; i++)
            {
                destination[i] = Get();
            }
            return this;
        }

        /// <summary>
        /// Returns a char at the specified <paramref name="index"/>; the position is not changed.
        /// </summary>
        /// <param name="index">The index, must not be negative and less than limit.</param>
        /// <returns>A char at the specified <paramref name="index"/>.</returns>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="index"/> is invalid.</exception>
        public abstract char Get(int index);

        /// <summary>
        /// Indicates whether this buffer is based on a char array and is read/write.
        /// <para/>
        /// Returns <c>true</c> if this buffer is based on a byte array and provides
        /// read/write access, <c>false</c> otherwise.
        /// </summary>
        public bool HasArray
        {
            get { return ProtectedHasArray; }
        }

        /// <summary>
        /// Calculates this buffer's hash code from the remaining chars. The
        /// position, limit, capacity and mark don't affect the hash code.
        /// </summary>
        /// <returns>The hash code calculated from the remaining chars.</returns>
        public override int GetHashCode()
        {
            int myPosition = position;
            int hash = 0;
            while (myPosition < limit)
            {
                hash = hash + Get(myPosition++);
            }
            return hash;
        }

        /// <summary>
        /// Indicates whether this buffer is direct. A direct buffer will try its
        /// best to take advantage of native memory APIs and it may not stay in the
        /// heap, so it is not affected by garbage collection.
        /// <para/>
        /// A char buffer is direct if it is based on a byte buffer and the byte
        /// buffer is direct.
        /// </summary>
        public abstract bool IsDirect { get; }

        /// <summary>
        /// Gets the number of remaining chars.
        /// </summary>
        public int Length
        {
            get { return Remaining; }
        }

        /// <summary>
        /// Gets the byte order used by this buffer when converting chars from/to
        /// bytes.
        /// <para/>
        /// If this buffer is not based on a byte buffer, then this always returns
        /// the platform's native byte order.
        /// </summary>
        public abstract ByteOrder Order { get; }

        /// <summary>
        /// Child class implements this method to realize <see cref="Array"/>.
        /// </summary>
        /// <seealso cref="Array"/>
        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        protected abstract char[] ProtectedArray { get; }

        /// <summary>
        /// Child class implements this method to realize <see cref="ArrayOffset"/>.
        /// </summary>
        /// <seealso cref="ArrayOffset"/>
        protected abstract int ProtectedArrayOffset { get; }

        /// <summary>
        /// Child class implements this method to realize <see cref="HasArray"/>.
        /// </summary>
        /// <seealso cref="HasArray"/>
        protected abstract bool ProtectedHasArray { get; }

        /// <summary>
        /// Writes the given char to the current position and increases the position
        /// by 1.
        /// </summary>
        /// <param name="c">The char to write.</param>
        /// <returns>This buffer.</returns>
        /// <exception cref="BufferOverflowException">If position is equal or greater than limit.</exception>
        /// <exception cref="ReadOnlyBufferException">If no changes may be made to the contents of this buffer.</exception>
        public abstract CharBuffer Put(char c);

        /// <summary>
        /// Writes chars from the given char array <paramref name="source"/> to the current position and
        /// increases the position by the number of chars written.
        /// <para/>
        /// Calling this method has the same effect as
        /// <c>Put(source, 0, source.Length)</c>.
        /// </summary>
        /// <param name="source">The source char array.</param>
        /// <returns>This buffer.</returns>
        /// <exception cref="BufferOverflowException">If <see cref="Buffer.Remaining"/> is less than <c>source.Length</c>.</exception>
        /// <exception cref="ReadOnlyBufferException">If no changes may be made to the contents of this buffer.</exception>
        public CharBuffer Put(char[] source)
        {
            return Put(source, 0, source.Length);
        }

        /// <summary>
        /// Writes chars from the given char array <paramref name="source"/>, starting from the specified <paramref name="offset"/>,
        /// to the current position and increases the position by the number of chars
        /// written.
        /// </summary>
        /// <param name="source">The source char array.</param>
        /// <param name="offset">The offset of char array, must not be negative and not greater
        /// than <c>source.Length</c>.</param>
        /// <param name="length">The number of chars to write, must be no less than zero and no
        /// greater than <c>source.Length - offset</c>.</param>
        /// <returns>This buffer.</returns>
        /// <exception cref="BufferOverflowException">If <see cref="Buffer.Remaining"/> is less than <paramref name="length"/>.</exception>
        /// <exception cref="IndexOutOfRangeException">If either <paramref name="offset"/> or <paramref name="length"/> is invalid.</exception>
        /// <exception cref="ReadOnlyBufferException">If no changes may be made to the contents of this buffer.</exception>
        public virtual CharBuffer Put(char[] source, int offset, int length)
        {
            int len = source.Length;
            if ((offset < 0) || (length < 0) || (long)offset + (long)length > len)
            {
                throw new IndexOutOfRangeException();
            }

            if (length > Remaining)
            {
                throw new BufferOverflowException();
            }
            for (int i = offset; i < offset + length; i++)
            {
                Put(source[i]);
            }
            return this;
        }

        /// <summary>
        /// Writes all the remaining chars of the <paramref name="src"/> char buffer to this
        /// buffer's current position, and increases both buffers' position by the
        /// number of chars copied.
        /// </summary>
        /// <param name="src">The source char buffer.</param>
        /// <returns>This buffer.</returns>
        /// <exception cref="BufferOverflowException">If <c>src.Remaining</c> is greater than this buffer's <see cref="Buffer.Remaining"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="src"/> is this buffer.</exception>
        /// <exception cref="ReadOnlyBufferException">If no changes may be made to the contents of this buffer.</exception>
        public virtual CharBuffer Put(CharBuffer src)
        {
            if (src == this)
            {
                throw new ArgumentException();
            }
            if (src.Remaining > Remaining)
            {
                throw new BufferOverflowException();
            }

            char[] contents = new char[src.Remaining];
            src.Get(contents);
            Put(contents);
            return this;
        }

        /// <summary>
        /// Writes a char to the specified index of this buffer; the position is not
        /// changed.
        /// </summary>
        /// <param name="index">The index, must be no less than zero and less than the limit.</param>
        /// <param name="c">The char to write.</param>
        /// <returns>This buffer.</returns>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="index"/> is invalid.</exception>
        /// <exception cref="ReadOnlyBufferException">If no changes may be made to the contents of this buffer.</exception>
        public abstract CharBuffer Put(int index, char c);

        /// <summary>
        /// Writes all chars of the given string to the current position of this
        /// buffer, and increases the position by the length of string.
        /// <para/>
        /// Calling this method has the same effect as
        /// <c>Put(str, 0, str.Length)</c>.
        /// </summary>
        /// <param name="str">The string to write.</param>
        /// <returns>This buffer.</returns>
        /// <exception cref="BufferOverflowException">If <see cref="Buffer.Remaining"/> is less than the length of string.</exception>
        /// <exception cref="ReadOnlyBufferException">If no changes may be made to the contents of this buffer.</exception>
        public CharBuffer Put(string str)
        {
            return Put(str, 0, str.Length);
        }

        /// <summary>
        /// Writes chars of the given string to the current position of this buffer,
        /// and increases the position by the number of chars written.
        /// </summary>
        /// <param name="str">The string to write.</param>
        /// <param name="start">The first char to write, must not be negative and not greater
        /// than <c>str.Length</c>.</param>
        /// <param name="end">The last char to write (excluding), must be less than
        /// <paramref name="start"/> and not greater than <c>str.Length</c>.</param>
        /// <returns>This buffer.</returns>
        /// <exception cref="BufferOverflowException">If <see cref="Buffer.Remaining"/> is less than <c>end - start</c>.</exception>
        /// <exception cref="IndexOutOfRangeException">If either <paramref name="start"/> or <paramref name="end"/> is invalid.</exception>
        /// <exception cref="ReadOnlyBufferException">If no changes may be made to the contents of this buffer.</exception>
        public virtual CharBuffer Put(string str, int start, int end)
        {
            int length = str.Length;
            if (start < 0 || end < start || end > length)
            {
                throw new IndexOutOfRangeException();
            }

            if (end - start > Remaining)
            {
                throw new BufferOverflowException();
            }
            for (int i = start; i < end; i++)
            {
                Put(str[i]);
            }
            return this;
        }

        /// <summary>
        /// Returns a sliced buffer that shares its content with this buffer.
        /// <para/>
        /// The sliced buffer's capacity will be this buffer's <see cref="Buffer.Remaining"/>,
        /// and its zero position will correspond to this buffer's current position.
        /// The new buffer's position will be 0, limit will be its capacity, and its
        /// mark is cleared. The new buffer's read-only property and byte order are
        /// same as this buffer.
        /// <para/>
        /// The new buffer shares its content with this buffer, which means either
        /// buffer's change of content will be visible to the other. The two buffer's
        /// position, limit and mark are independent.
        /// </summary>
        /// <returns>A sliced buffer that shares its content with this buffer.</returns>
        public abstract CharBuffer Slice();

        /// <summary>
        /// Returns a new char buffer representing a sub-sequence of this buffer's
        /// current remaining content.
        /// <para/>
        /// The new buffer's position will be <c>Position + start</c>, limit will
        /// be <c>Position + end</c>, capacity will be the same as this buffer.
        /// The new buffer's read-only property and byte order are the same as this
        /// buffer.
        /// <para/>
        /// The new buffer shares its content with this buffer, which means either
        /// buffer's change of content will be visible to the other. The two buffer's
        /// position, limit and mark are independent.
        /// </summary>
        /// <param name="start">
        /// The start index of the sub-sequence, referenced from the
        /// current buffer position. Must not be less than zero and not
        /// greater than the value obtained from a call to <see cref="Buffer.Remaining"/>.
        /// </param>
        /// <param name="end">
        /// The end index of the sub-sequence, referenced from the current
        /// buffer position. Must not be less than <paramref name="start"/> and not
        /// be greater than the value obtained from a call to
        /// <see cref="Buffer.Remaining"/>.
        /// </param>
        /// <returns>A new char buffer represents a sub-sequence of this buffer's
        /// current remaining content.</returns>
        /// <exception cref="IndexOutOfRangeException">If either <paramref name="start"/> or <paramref name="end"/> is invalid.</exception>
        public abstract ICharSequence SubSequence(int start, int end);

        /// <summary>
        /// Returns a string representing the current remaining chars of this buffer.
        /// </summary>
        /// <returns>A string representing the current remaining chars of this buffer.</returns>
        public override string ToString()
        {
            StringBuilder strbuf = new StringBuilder();
            for (int i = position; i < limit; i++)
            {
                strbuf.Append(Get(i));
            }
            return strbuf.ToString();
        }

        /// <summary>
        /// Writes the given char <paramref name="c"/> to the current position and increases the position
        /// by 1.
        /// </summary>
        /// <param name="c">The char to write.</param>
        /// <returns>This buffer.</returns>
        /// <exception cref="BufferOverflowException">If position is equal or greater than limit.</exception>
        /// <exception cref="ReadOnlyBufferException">If no changes may be made to the contents of this buffer.</exception>
        public virtual CharBuffer Append(char c)
        {
            return Put(c);
        }

        /// <summary>
        /// Writes all chars of the given character sequence <paramref name="csq"/> to the
        /// current position of this buffer, and increases the position by the length
        /// of the csq.
        /// <para/>
        /// Calling this method has the same effect as <c>Append(csq.ToString())</c>.
        /// If the <see cref="ICharSequence"/> is <c>null</c> the string "null" will be
        /// written to the buffer.
        /// </summary>
        /// <param name="csq">The <see cref="ICharSequence"/> to write.</param>
        /// <returns>This buffer.</returns>
        /// <exception cref="BufferOverflowException">If <c>Remaining</c> is less than the length of <paramref name="csq"/>.</exception>
        /// <exception cref="ReadOnlyBufferException">If no changes may be made to the contents of this buffer.</exception>
        public virtual CharBuffer Append(ICharSequence csq) 
        {
            if (csq != null)
            {
                return Put(csq.ToString());
            }
            return Put("null"); //$NON-NLS-1$
        }

        /// <summary>
        /// Writes chars of the given <see cref="ICharSequence"/> to the current position of
        /// this buffer, and increases the position by the number of chars written.
        /// </summary>
        /// <param name="csq">The <see cref="ICharSequence"/> to write.</param>
        /// <param name="start">The first char to write, must not be negative and not greater
        /// than <c>csq.Length</c>.</param>
        /// <param name="end">The last char to write (excluding), must be less than
        /// <paramref name="start"/> and not greater than <c>csq.Length</c>.</param>
        /// <returns>This buffer.</returns>
        /// <exception cref="BufferOverflowException">If <c>Remaining</c> is less than <c>end - start</c>.</exception>
        /// <exception cref="IndexOutOfRangeException">If either <paramref name="start"/> or <paramref name="end"/> is invalid.</exception>
        /// <exception cref="ReadOnlyBufferException">If no changes may be made to the contents of this buffer.</exception>
        public virtual CharBuffer Append(ICharSequence csq, int start, int end) 
        {
            if (csq == null)
            {
                csq = "null".ToCharSequence(); //$NON-NLS-1$
            }
            ICharSequence cs = csq.SubSequence(start, end);
            if (cs.Length > 0)
            {
                return Put(cs.ToString());
            }
            return this;
        }

        /// <summary>
        /// Reads characters from this buffer and puts them into <paramref name="target"/>. The
        /// number of chars that are copied is either the number of remaining chars
        /// in this buffer or the number of remaining chars in <paramref name="target"/>,
        /// whichever is smaller.
        /// </summary>
        /// <param name="target">The target char buffer.</param>
        /// <returns>The number of chars copied or -1 if there are no chars left to be
        /// read from this buffer.</returns>
        /// <exception cref="ArgumentException">If <paramref name="target"/> is this buffer.</exception>
        /// <exception cref="System.IO.IOException">If an I/O error occurs.</exception>
        /// <exception cref="ReadOnlyBufferException">If no changes may be made to the contents of <paramref name="target"/>.</exception>
        public virtual int Read(CharBuffer target)
        {
            int remaining = Remaining;
            if (target == this)
            {
                if (remaining == 0)
                {
                    return -1;
                }
                throw new ArgumentException();
            }
            if (remaining == 0)
            {
                return limit > 0 && target.Remaining == 0 ? 0 : -1;
            }
            remaining = Math.Min(target.Remaining, remaining);
            if (remaining > 0)
            {
                char[] chars = new char[remaining];
                Get(chars);
                target.Put(chars);
            }
            return remaining;
        }
    }
}
