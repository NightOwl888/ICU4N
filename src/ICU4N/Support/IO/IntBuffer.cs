using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ICU4N.Support.IO
{
    /// <summary>
    /// A buffer of <see cref="int"/>s.
    /// </summary>
    /// <remarks>
    /// A <see cref="int"/> buffer can be created in either of the following ways:
    /// <list type="bullet">
    ///     <item><description><see cref="Allocate"/> a new <see cref="int"/> array and create a buffer based on it;</description></item>
    ///     <item><description><see cref="Wrap"/> an existing <see cref="int"/> array to create a new buffer;</description></item>
    ///     <item><description>Use <see cref="ByteBuffer.AsInt32Buffer"/> to create a <see cref="int"/> buffer based on a byte buffer.</description></item>
    /// </list>
    /// </remarks>
    public abstract class Int32Buffer : Buffer, IComparable<Int32Buffer>
    {
        /**
     * Creates an int buffer based on a newly allocated int array.
     * 
     * @param capacity
     *            the capacity of the new buffer.
     * @return the created int buffer.
     * @throws IllegalArgumentException
     *             if {@code capacity} is less than zero.
     */
        public static Int32Buffer Allocate(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentException();
            }
            return new ReadWriteInt32ArrayBuffer(capacity);
        }

        /**
         * Creates a new int buffer by wrapping the given int array.
         * <p>
         * Calling this method has the same effect as
         * {@code wrap(array, 0, array.length)}.
         *
         * @param array
         *            the int array which the new buffer will be based on.
         * @return the created int buffer.
         */
        public static Int32Buffer Wrap(int[] array)
        {
            return Wrap(array, 0, array.Length);
        }

        /**
         * Creates a new int buffer by wrapping the given int array.
         * <p>
         * The new buffer's position will be {@code start}, limit will be
         * {@code start + len}, capacity will be the length of the array.
         *
         * @param array
         *            the int array which the new buffer will be based on.
         * @param start
         *            the start index, must not be negative and not greater than
         *            {@code array.length}
         * @param len
         *            the length, must not be negative and not greater than
         *            {@code array.length - start}.
         * @return the created int buffer.
         * @exception IndexOutOfBoundsException
         *                if either {@code start} or {@code len} is invalid.
         */
        public static Int32Buffer Wrap(int[] array, int start, int len)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (start < 0 || len < 0 || (long)len + (long)start > array.Length)
            {
                throw new IndexOutOfRangeException();
            }

            Int32Buffer buf = new ReadWriteInt32ArrayBuffer(array);
            buf.position = start;
            buf.limit = start + len;

            return buf;
        }

        /**
         * Constructs a {@code IntBuffer} with given capacity.
         *
         * @param capacity
         *            the capacity of the buffer.
         */
        internal Int32Buffer(int capacity)
            : base(capacity)
        {
        }

        /**
         * Returns the int array which this buffer is based on, if there is one.
         * 
         * @return the int array which this buffer is based on.
         * @exception ReadOnlyBufferException
         *                if this buffer is based on an array, but it is read-only.
         * @exception UnsupportedOperationException
         *                if this buffer is not based on an array.
         */
        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        public int[] Array
        {
            get { return ProtectedArray; }
        }

        /**
         * Returns the offset of the int array which this buffer is based on, if
         * there is one.
         * <p>
         * The offset is the index of the array corresponds to the zero position of
         * the buffer.
         *
         * @return the offset of the int array which this buffer is based on.
         * @exception ReadOnlyBufferException
         *                if this buffer is based on an array, but it is read-only.
         * @exception UnsupportedOperationException
         *                if this buffer is not based on an array.
         */
        public int ArrayOffset
        {
            get { return ProtectedArrayOffset; }
        }

        /**
         * Returns a read-only buffer that shares its content with this buffer.
         * <p>
         * The returned buffer is guaranteed to be a new instance, even this buffer
         * is read-only itself. The new buffer's position, limit, capacity and mark
         * are the same as this buffer's.
         * <p>
         * The new buffer shares its content with this buffer, which means this
         * buffer's change of content will be visible to the new buffer. The two
         * buffer's position, limit and mark are independent.
         *
         * @return a read-only version of this buffer.
         */
        public abstract Int32Buffer AsReadOnlyBuffer();

        /**
         * Compacts this int buffer.
         * <p>
         * The remaining ints will be moved to the head of the buffer, starting from
         * position zero. Then the position is set to {@code remaining()}; the
         * limit is set to capacity; the mark is cleared.
         *
         * @return this buffer.
         * @exception ReadOnlyBufferException
         *                if no changes may be made to the contents of this buffer.
         */
        public abstract Int32Buffer Compact();

        /**
         * Compares the remaining ints of this buffer to another int buffer's
         * remaining ints.
         * 
         * @param otherBuffer
         *            another int buffer.
         * @return a negative value if this is less than {@code other}; 0 if this
         *         equals to {@code other}; a positive value if this is greater
         *         than {@code other}.
         * @exception ClassCastException
         *                if {@code other} is not an int buffer.
         */
        public virtual int CompareTo(Int32Buffer otherBuffer)
        {
            int compareRemaining = (Remaining < otherBuffer.Remaining) ? Remaining
                    : otherBuffer.Remaining;
            int thisPos = position;
            int otherPos = otherBuffer.position;
            int thisByte, otherByte;
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

        /**
         * Returns a duplicated buffer that shares its content with this buffer.
         * <p>
         * The duplicated buffer's position, limit, capacity and mark are the same
         * as this buffer. The duplicated buffer's read-only property and byte order
         * are the same as this buffer's.
         * <p>
         * The new buffer shares its content with this buffer, which means either
         * buffer's change of content will be visible to the other. The two buffer's
         * position, limit and mark are independent.
         *
         * @return a duplicated buffer that shares its content with this buffer.
         */
        public abstract Int32Buffer Duplicate();

        /**
         * Checks whether this int buffer is equal to another object.
         * <p>
         * If {@code other} is not a int buffer then {@code false} is returned. Two
         * int buffers are equal if and only if their remaining ints are exactly the
         * same. Position, limit, capacity and mark are not considered.
         *
         * @param other
         *            the object to compare with this int buffer.
         * @return {@code true} if this int buffer is equal to {@code other},
         *         {@code false} otherwise.
         */
        public override bool Equals(object other)
        {
            if (!(other is Int32Buffer))
            {
                return false;
            }
            Int32Buffer otherBuffer = (Int32Buffer)other;

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

        /**
         * Returns the int at the current position and increases the position by 1.
         * 
         * @return the int at the current position.
         * @exception BufferUnderflowException
         *                if the position is equal or greater than limit.
         */
        public abstract int Get();

        /**
         * Reads ints from the current position into the specified int array and
         * increases the position by the number of ints read.
         * <p>
         * Calling this method has the same effect as
         * {@code get(dest, 0, dest.length)}.
         *
         * @param dest
         *            the destination int array.
         * @return this buffer.
         * @exception BufferUnderflowException
         *                if {@code dest.length} is greater than {@code remaining()}.
         */
        public virtual Int32Buffer Get(int[] dest)
        {
            return Get(dest, 0, dest.Length);
        }

        /**
         * Reads ints from the current position into the specified int array,
         * starting from the specified offset, and increases the position by the
         * number of ints read.
         * 
         * @param dest
         *            the target int array.
         * @param off
         *            the offset of the int array, must not be negative and not
         *            greater than {@code dest.length}.
         * @param len
         *            the number of ints to read, must be no less than zero and not
         *            greater than {@code dest.length - off}.
         * @return this buffer.
         * @exception IndexOutOfBoundsException
         *                if either {@code off} or {@code len} is invalid.
         * @exception BufferUnderflowException
         *                if {@code len} is greater than {@code remaining()}.
         */
        public virtual Int32Buffer Get(int[] dest, int off, int len)
        {
            int length = dest.Length;
            if (off < 0 || len < 0 || (long)len + (long)off > length)
            {
                throw new IndexOutOfRangeException();
            }
            if (len > Remaining)
            {
                throw new BufferUnderflowException();
            }
            for (int i = off; i < off + len; i++)
            {
                dest[i] = Get();
            }
            return this;
        }

        /**
         * Returns an int at the specified index; the position is not changed.
         * 
         * @param index
         *            the index, must not be negative and less than limit.
         * @return an int at the specified index.
         * @exception IndexOutOfBoundsException
         *                if index is invalid.
         */
        public abstract int Get(int index);

        /**
         * Indicates whether this buffer is based on a int array and is read/write.
         *
         * @return {@code true} if this buffer is based on a int array and provides
         *         read/write access, {@code false} otherwise.
         */
        public bool HasArray
        {
            get { return ProtectedHasArray; }
        }

        /**
         * Calculates this buffer's hash code from the remaining chars. The
         * position, limit, capacity and mark don't affect the hash code.
         *
         * @return the hash code calculated from the remaining ints.
         */
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

        /**
         * Indicates whether this buffer is direct. A direct buffer will try its
         * best to take advantage of native memory APIs and it may not stay in the
         * Java heap, so it is not affected by garbage collection.
         * <p>
         * An int buffer is direct if it is based on a byte buffer and the byte
         * buffer is direct.
         *
         * @return {@code true} if this buffer is direct, {@code false} otherwise.
         */
        public abstract bool IsDirect { get; }

        /**
         * Returns the byte order used by this buffer when converting ints from/to
         * bytes.
         * <p>
         * If this buffer is not based on a byte buffer, then always return the
         * platform's native byte order.
         *
         * @return the byte order used by this buffer when converting ints from/to
         *         bytes.
         */
        public abstract ByteOrder Order { get; }

        /**
         * Child class implements this method to realize {@code array()}.
         *
         * @return see {@code array()}
         */
        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        protected abstract int[] ProtectedArray { get; }

        /**
         * Child class implements this method to realize {@code arrayOffset()}.
         *
         * @return see {@code arrayOffset()}
         */
        protected abstract int ProtectedArrayOffset { get; }

        /**
         * Child class implements this method to realize {@code hasArray()}.
         *
         * @return see {@code hasArray()}
         */
        protected abstract bool ProtectedHasArray { get; }

        /**
         * Writes the given int to the current position and increases the position
         * by 1.
         * 
         * @param i
         *            the int to write.
         * @return this buffer.
         * @exception BufferOverflowException
         *                if position is equal or greater than limit.
         * @exception ReadOnlyBufferException
         *                if no changes may be made to the contents of this buffer.
         */
        public abstract Int32Buffer Put(int i);

        /**
         * Writes ints from the given int array to the current position and
         * increases the position by the number of ints written.
         * <p>
         * Calling this method has the same effect as
         * {@code put(src, 0, src.length)}.
         *
         * @param src
         *            the source int array.
         * @return this buffer.
         * @exception BufferOverflowException
         *                if {@code remaining()} is less than {@code src.length}.
         * @exception ReadOnlyBufferException
         *                if no changes may be made to the contents of this buffer.
         */
        public Int32Buffer Put(int[] src)
        {
            return Put(src, 0, src.Length);
        }

        /**
         * Writes ints from the given int array, starting from the specified offset,
         * to the current position and increases the position by the number of ints
         * written.
         * 
         * @param src
         *            the source int array.
         * @param off
         *            the offset of int array, must not be negative and not greater
         *            than {@code src.length}.
         * @param len
         *            the number of ints to write, must be no less than zero and not
         *            greater than {@code src.length - off}.
         * @return this buffer.
         * @exception BufferOverflowException
         *                if {@code remaining()} is less than {@code len}.
         * @exception IndexOutOfBoundsException
         *                if either {@code off} or {@code len} is invalid.
         * @exception ReadOnlyBufferException
         *                if no changes may be made to the contents of this buffer.
         */
        public virtual Int32Buffer Put(int[] src, int off, int len)
        {
            int length = src.Length;
            if (off < 0 || len < 0 || (long)len + (long)off > length)
            {
                throw new IndexOutOfRangeException();
            }

            if (len > Remaining)
            {
                throw new BufferOverflowException();
            }
            for (int i = off; i < off + len; i++)
            {
                Put(src[i]);
            }
            return this;
        }

        /**
         * Writes all the remaining ints of the {@code src} int buffer to this
         * buffer's current position, and increases both buffers' position by the
         * number of ints copied.
         * 
         * @param src
         *            the source int buffer.
         * @return this buffer.
         * @exception BufferOverflowException
         *                if {@code src.remaining()} is greater than this buffer's
         *                {@code remaining()}.
         * @exception IllegalArgumentException
         *                if {@code src} is this buffer.
         * @exception ReadOnlyBufferException
         *                if no changes may be made to the contents of this buffer.
         */
        public virtual Int32Buffer Put(Int32Buffer src)
        {
            if (src == this)
            {
                throw new ArgumentException();
            }
            if (src.Remaining > Remaining)
            {
                throw new BufferOverflowException();
            }
            int[] contents = new int[src.Remaining];
            src.Get(contents);
            Put(contents);
            return this;
        }

        /**
         * Write a int to the specified index of this buffer; the position is not
         * changed.
         * 
         * @param index
         *            the index, must not be negative and less than the limit.
         * @param i
         *            the int to write.
         * @return this buffer.
         * @exception IndexOutOfBoundsException
         *                if index is invalid.
         * @exception ReadOnlyBufferException
         *                if no changes may be made to the contents of this buffer.
         */
        public abstract Int32Buffer Put(int index, int i);

        /**
         * Returns a sliced buffer that shares its content with this buffer.
         * <p>
         * The sliced buffer's capacity will be this buffer's {@code remaining()},
         * and its zero position will correspond to this buffer's current position.
         * The new buffer's position will be 0, limit will be its capacity, and its
         * mark is cleared. The new buffer's read-only property and byte order are
         * same as this buffer's.
         * <p>
         * The new buffer shares its content with this buffer, which means either
         * buffer's change of content will be visible to the other. The two buffer's
         * position, limit and mark are independent.
         * 
         * @return a sliced buffer that shares its content with this buffer.
         */
        public abstract Int32Buffer Slice();

        /**
         * Returns a string represents of the state of this int buffer.
         * 
         * @return a string represents of the state of this int buffer.
         */
        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(GetType().Name);
            buf.Append(", status: capacity="); //$NON-NLS-1$
            buf.Append(Capacity);
            buf.Append(" position="); //$NON-NLS-1$
            buf.Append(Position);
            buf.Append(" limit="); //$NON-NLS-1$
            buf.Append(Limit);
            return buf.ToString();
        }
    }
}
