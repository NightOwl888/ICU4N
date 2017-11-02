using ICU4N.Support.Text;
using System;
using System.Diagnostics.CodeAnalysis;

namespace ICU4N.Support.IO
{
    /// <summary>
    /// This class wraps a char sequence to be a char buffer.
    /// <para/>
    /// Implementation notice:
    /// <list type="bullet">
    ///     <item><description>Char sequence based buffer is always readonly.</description></item>
    /// </list>
    /// </summary>
    internal sealed class CharSequenceAdapter : CharBuffer
    {
        internal static CharSequenceAdapter Copy(CharSequenceAdapter other)
        {
            CharSequenceAdapter buf = new CharSequenceAdapter(other.sequence);
            buf.limit = other.limit;
            buf.position = other.position;
            buf.mark = other.mark;
            return buf;
        }

        internal readonly ICharSequence sequence;

        internal CharSequenceAdapter(ICharSequence chseq)
                : base(chseq.Length)
        {
            sequence = chseq;
        }

        public override CharBuffer AsReadOnlyBuffer()
        {
            return Duplicate();
        }

        public override CharBuffer Compact()
        {
            throw new ReadOnlyBufferException();
        }

        public override CharBuffer Duplicate()
        {
            return Copy(this);
        }

        public override char Get()
        {
            if (position == limit)
            {
                throw new BufferUnderflowException();
            }
            return sequence[position++];
        }

        public override char Get(int index)
        {
            if (index < 0 || index >= limit)
            {
                throw new IndexOutOfRangeException();
            }
            return sequence[index];
        }

        public override sealed CharBuffer Get(char[] dest, int off, int len)
        {
            int length = dest.Length;
            if ((off < 0) || (len < 0) || (long)off + (long)len > length)
            {
                throw new IndexOutOfRangeException();
            }
            if (len > Remaining)
            {
                throw new BufferUnderflowException();
            }
            int newPosition = position + len;
            //sequence.ToString().GetChars(position, newPosition, dest, off);
            sequence.ToString().CopyTo(position, dest, off, len);
            position = newPosition;
            return this;
        }

        public override bool IsDirect
        {
            get { return false; }
        }

        public override bool IsReadOnly
        {
            get { return true; }
        }

        public override ByteOrder Order
        {
            get { return ByteOrder.NativeOrder; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819", Justification = "design requires some writable array properties")]
        protected override char[] ProtectedArray
        {
            get { throw new NotSupportedException(); }
        }

        protected override int ProtectedArrayOffset
        {
            get { throw new NotSupportedException(); }
        }

        protected override bool ProtectedHasArray
        {
            get { return false; }
        }

        public override CharBuffer Put(char c)
        {
            throw new ReadOnlyBufferException();
        }

        public override CharBuffer Put(int index, char c)
        {
            throw new ReadOnlyBufferException();
        }

        public override sealed CharBuffer Put(char[] src, int off, int len)
        {
            if ((off < 0) || (len < 0) || (long)off + (long)len > src.Length)
            {
                throw new IndexOutOfRangeException();
            }

            if (len > Remaining)
            {
                throw new BufferOverflowException();
            }

            throw new ReadOnlyBufferException();
        }

        public override CharBuffer Put(String src, int start, int end)
        {
            if ((start < 0) || (end < 0)
                    || (long)start + (long)end > src.Length)
            {
                throw new IndexOutOfRangeException();
            }
            throw new ReadOnlyBufferException();
        }

        public override CharBuffer Slice()
        {
            return new CharSequenceAdapter(sequence.SubSequence(position, limit));
        }

        public override ICharSequence SubSequence(int start, int end)
        {
            if (end < start || start < 0 || end > Remaining)
            {
                throw new IndexOutOfRangeException();
            }

            CharSequenceAdapter result = Copy(this);
            result.position = position + start;
            result.limit = position + end;
            return result;
        }
    }
}
