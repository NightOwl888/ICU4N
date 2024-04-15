using ICU4N.Support.Text;
using ICU4N.Text;
using J2N.IO;
using J2N.Numerics;
using J2N.Text;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;

namespace ICU4N.Impl
{
    // Normalizer2 implementation for the old UNORM_NONE.
    public sealed partial class NoopNormalizer2 : Normalizer2
    {
        // ICU4N specific: Moved Normalize(ICharSequence, StringBuilder) to Norm2AllModes.generated.tt
        #region Normalize(ICharSequence, StringBuilder)
        public override StringBuilder Normalize(string src, StringBuilder dest)
        {
            dest.Length = 0;
            return dest.Append(src);
        }



        public override StringBuilder Normalize(StringBuilder src, StringBuilder dest)
        {
            if (dest == src)
            {
                throw new ArgumentException($"'{nameof(src)}' cannot be the same instance as '{nameof(dest)}'");
            }
            dest.Length = 0;
            return dest.Append(src);
        }



        public override StringBuilder Normalize(ICharSequence src, StringBuilder dest)
        {
            if (src is StringBuilderCharSequence && ((StringBuilderCharSequence)src).Value == dest)
            {
                throw new ArgumentException($"'{nameof(src)}' cannot be the same instance as '{nameof(dest)}'");
            }
            dest.Length = 0;
            return dest.Append(src);
        }

#if FEATURE_SPAN


        public override StringBuilder Normalize(ReadOnlySpan<char> src, StringBuilder dest)
        {
            dest.Length = 0;
            return dest.Append(src);
        }
#endif
        #endregion

        // ICU4N specific: Moved Normalize(ICharSequence, IAppendable) to Norm2AllModes.generated.tt
        #region Normalize(ICharSequence, IAppendable)
        public override IAppendable Normalize(string src, IAppendable dest)
        {
            // ICU4N: Removed unnecessary try/catch for IOException
            return dest.Append(src);
        }



        public override IAppendable Normalize(StringBuilder src, IAppendable dest)
        {
            if (dest is StringBuilderCharSequence && ((StringBuilderCharSequence)dest).Value == src)
            {
                throw new ArgumentException($"'{nameof(src)}' cannot be the same instance as '{nameof(dest)}'");
            }
            // ICU4N: Removed unnecessary try/catch for IOException
            return dest.Append(src);
        }



        public override IAppendable Normalize(ICharSequence src, IAppendable dest)
        {
            if ((dest == src) || (src is StringBuilderCharSequence && dest is StringBuilderCharSequence
                && ((StringBuilderCharSequence)src).Value == ((StringBuilderCharSequence)dest).Value))
            {
                throw new ArgumentException($"'{nameof(src)}' cannot be the same instance as '{nameof(dest)}'");
            }
            // ICU4N: Removed unnecessary try/catch for IOException
            return dest.Append(src);
        }

#if FEATURE_SPAN


        public override IAppendable Normalize(ReadOnlySpan<char> src, IAppendable dest)
        {
            // ICU4N: Removed unnecessary try/catch for IOException
            return dest.Append(src);
        }
#endif
        #endregion Normalize(ICharSequence, IAppendable)

        // ICU4N specific: Moved NormalizeSecondAndAppend(StringBuilder, ICharSequence) to Norm2AllModes.generated.tt
        #region NormalizeSecondAndAppend(StringBuilder, ICharSequence)
        public override StringBuilder NormalizeSecondAndAppend(StringBuilder first, string second)
        {
            return first.Append(second);
        }



        public override StringBuilder NormalizeSecondAndAppend(StringBuilder first, StringBuilder second)
        {
            if (first == second)
            {
                throw new ArgumentException($"'{nameof(first)}' cannot be the same instance as '{nameof(second)}'");
            }
            return first.Append(charSequence: second);
        }



        public override StringBuilder NormalizeSecondAndAppend(StringBuilder first, ICharSequence second)
        {
            if (second is StringBuilderCharSequence && ((StringBuilderCharSequence)second).Value == first)
            {
                throw new ArgumentException($"'{nameof(first)}' cannot be the same instance as '{nameof(second)}'");
            }
            return first.Append(charSequence: second);
        }

#if FEATURE_SPAN


        public override StringBuilder NormalizeSecondAndAppend(StringBuilder first, ReadOnlySpan<char> second)
        {
            return first.Append(second);
        }
#endif
        #endregion NormalizeSecondAndAppend(StringBuilder, ICharSequence)

        // ICU4N specific: Moved Append(StringBuilder, ICharSequence) to Norm2AllModes.generated.tt
        #region Append(StringBuilder, ICharSequence)
        public override StringBuilder Append(StringBuilder first, string second)
        {
            return first.Append(second);
        }



        public override StringBuilder Append(StringBuilder first, StringBuilder second)
        {
            if (first == second)
            {
                throw new ArgumentException($"'{nameof(first)}' cannot be the same instance as '{nameof(second)}'");
            }
            return first.Append(second);
        }



        public override StringBuilder Append(StringBuilder first, ICharSequence second)
        {
            if (second is StringBuilderCharSequence && ((StringBuilderCharSequence)second).Value == first)
            {
                throw new ArgumentException($"'{nameof(first)}' cannot be the same instance as '{nameof(second)}'");
            }
            return first.Append(second);
        }

#if FEATURE_SPAN


        public override StringBuilder Append(StringBuilder first, ReadOnlySpan<char> second)
        {
            return first.Append(second);
        }
#endif
        #endregion

        // ICU4N specific: Moved IsNormalized(ICharSequence) to Norm2AllModes.generated.tt
        #region IsNormalized(ICharSequence)
        public override bool IsNormalized(string s) { return true; }



        public override bool IsNormalized(StringBuilder s) { return true; }



        public override bool IsNormalized(ICharSequence s) { return true; }

#if FEATURE_SPAN


        public override bool IsNormalized(ReadOnlySpan<char> s) { return true; }
#endif
        #endregion

        // ICU4N specific: Moved QuickCheck(ICharSequence) to Norm2AllModes.generated.tt
        #region QuickCheck(ICharSequence)
        public override QuickCheckResult QuickCheck(string s) { return QuickCheckResult.Yes; }



        public override QuickCheckResult QuickCheck(StringBuilder s) { return QuickCheckResult.Yes; }



        public override QuickCheckResult QuickCheck(ICharSequence s) { return QuickCheckResult.Yes; }

#if FEATURE_SPAN


        public override QuickCheckResult QuickCheck(ReadOnlySpan<char> s) { return QuickCheckResult.Yes; }
#endif
        #endregion QuickCheck(ICharSequence)

        // ICU4N specific: Moved SpanQuickCheckYes(ICharSequence) to Norm2AllModes.generated.tt
        #region SpanQuickCheckYes(ICharSequence)
        public override int SpanQuickCheckYes(string s) { return s.Length; }



        public override int SpanQuickCheckYes(StringBuilder s) { return s.Length; }



        public override int SpanQuickCheckYes(ICharSequence s) { return s.Length; }

#if FEATURE_SPAN


        public override int SpanQuickCheckYes(ReadOnlySpan<char> s) { return s.Length; }
#endif 
        #endregion SpanQuickCheckYes(ICharSequence)

        public override string GetDecomposition(int c)
        {
            return null;
        }
        // No need to override the default GetRawDecomposition().

        public override bool HasBoundaryBefore(int c) { return true; }

        public override bool HasBoundaryAfter(int c) { return true; }

        public override bool IsInert(int c) { return true; }

#if FEATURE_SPAN
        public override bool TryNormalize(ReadOnlySpan<char> source, Span<char> destination, out int charsLength)
        {
            if (Unsafe.AreSame(ref MemoryMarshal.GetReference(source), ref MemoryMarshal.GetReference(destination)))
            {
                throw new ArgumentException($"'{nameof(source)}' cannot be the same reference as '{nameof(destination)}'");
            }
            bool success = source.TryCopyTo(destination);
            charsLength = source.Length;
            return success;
        }

        public override bool TryNormalizeSecondAndConcat(ReadOnlySpan<char> first, ReadOnlySpan<char> second, Span<char> destination, out int charsLength)
        {
            bool success = true;
            if (Unsafe.AreSame(ref MemoryMarshal.GetReference(second), ref MemoryMarshal.GetReference(destination)))
            {
                throw new ArgumentException($"'{nameof(second)}' cannot be the same reference as '{nameof(destination)}'");
            }
            if (!Unsafe.AreSame(ref MemoryMarshal.GetReference(first), ref MemoryMarshal.GetReference(destination)))
            {
                success = first.TryCopyTo(destination);
            }
            success = success && second.TryCopyTo(destination.Slice(first.Length));
            charsLength = first.Length + second.Length;
            return success;
        }

        public override bool TryConcat(ReadOnlySpan<char> first, ReadOnlySpan<char> second, Span<char> destination, out int charsLength)
        {
            bool success = true;
            if (Unsafe.AreSame(ref MemoryMarshal.GetReference(second), ref MemoryMarshal.GetReference(destination)))
            {
                throw new ArgumentException($"'{nameof(second)}' cannot be the same reference as '{nameof(destination)}'");
            }
            if (!Unsafe.AreSame(ref MemoryMarshal.GetReference(first), ref MemoryMarshal.GetReference(destination)))
            {
                success = first.TryCopyTo(destination);
            }
            success = success && second.TryCopyTo(destination.Slice(first.Length));
            charsLength = first.Length + second.Length;
            return success;
        }
#endif
    }

    // Intermediate class:
    // Has Normalizer2Impl and does boilerplate argument checking and setup.
    public abstract partial class Normalizer2WithImpl : Normalizer2
    {
        protected const int CharStackBufferSize = 64;

        public Normalizer2WithImpl(Normalizer2Impl ni)
        {
            Impl = ni ?? throw new ArgumentNullException(nameof(ni)); // ICU4N: Added guard clause
        }

        // normalize

#if FEATURE_SPAN

        /// <summary>
        /// Returns the normalized form of the source <see cref="ReadOnlySpan{Char}"/>.
        /// </summary>
        /// <param name="source">Source <see cref="ReadOnlySpan{Char}"/>.</param>
        /// <returns>Normalized <paramref name="source"/>.</returns>
        /// <draft>ICU 4.4</draft>
        public override string Normalize(ReadOnlySpan<char> source)
        {
            int length = source.Length;
            var buffer = length <= CharStackBufferSize
                ? new ValueReorderingBuffer(Impl, stackalloc char[CharStackBufferSize])
                : new ValueReorderingBuffer(Impl, ReadOnlySpan<char>.Empty, length);
            try
            {
                Normalize(source, ref buffer);
                return buffer.ToString();
            }
            finally
            {
                buffer.Dispose();
            }
        }

        /// <summary>
        /// Normalizes the form of the source <see cref="ReadOnlySpan{Char}"/>
        /// and places the result in <paramref name="destination"/>.
        /// </summary>
        /// <param name="source">Source <see cref="ReadOnlySpan{Char}"/>.</param>
        /// <param name="destination">The span in which to write the normalized value formatted as a span of characters.</param>
        /// <param name="charsLength">When this method returns <c>true</c>, contains the number of characters that are usable in destination;
        /// otherwise, this is the length of buffer that will need to be allocated to succeed in another attempt.</param>
        /// <returns>Normalized <paramref name="source"/>.</returns>
        /// <draft>ICU 60.1</draft>
        public override bool TryNormalize(ReadOnlySpan<char> source, Span<char> destination, out int charsLength)
        {
            if (Unsafe.AreSame(ref MemoryMarshal.GetReference(source), ref MemoryMarshal.GetReference(destination)))
            {
                throw new ArgumentException($"'{nameof(source)}' cannot be the same reference as '{nameof(destination)}'");
            }
            int length = source.Length;
            var buffer = length <= CharStackBufferSize
                ? new ValueReorderingBuffer(Impl, stackalloc char[CharStackBufferSize])
                : new ValueReorderingBuffer(Impl, ReadOnlySpan<char>.Empty, length);
            try
            {
                Normalize(source, ref buffer);
                return buffer.TryCopyTo(destination, out charsLength);
            }
            finally
            {
                buffer.Dispose();
            }
        }
#endif

#if FEATURE_SPAN
        

        public override bool TryNormalizeSecondAndConcat(ReadOnlySpan<char> first, ReadOnlySpan<char> second, Span<char> destination, out int charsLength)
        {
            if (Unsafe.AreSame(ref MemoryMarshal.GetReference(second), ref MemoryMarshal.GetReference(destination)))
            {
                throw new ArgumentException($"'{nameof(second)}' cannot be the same reference as '{nameof(destination)}'");
            }
            bool success = true;
            if (!Unsafe.AreSame(ref MemoryMarshal.GetReference(first), ref MemoryMarshal.GetReference(destination)))
            {
                success = first.TryCopyTo(destination);
            }
            var buffer = new ValueReorderingBuffer(Impl, destination);
            NormalizeAndAppend(second, doNormalize: true, ref buffer);
            success = success && buffer.Length <= destination.Length;
            charsLength = buffer.Length;
            return success;
        }

        public override bool TryConcat(ReadOnlySpan<char> first, ReadOnlySpan<char> second, Span<char> destination, out int charsLength)
        {
            if (Unsafe.AreSame(ref MemoryMarshal.GetReference(second), ref MemoryMarshal.GetReference(destination)))
            {
                throw new ArgumentException($"'{nameof(second)}' cannot be the same reference as '{nameof(destination)}'");
            }
            bool success = true;
            if (!Unsafe.AreSame(ref MemoryMarshal.GetReference(first), ref MemoryMarshal.GetReference(destination)))
            {
                success = first.TryCopyTo(destination);
            }
            var buffer = new ValueReorderingBuffer(Impl, destination);
            NormalizeAndAppend(second, doNormalize: false, ref buffer);
            success = success && buffer.Length <= destination.Length;
            charsLength = buffer.Length;
            return success;
        }
#endif


        // ICU4N specific: Moved Normalize(ICharSequence, StringBuilder) to Norm2AllModes.generated.tt
        #region Normalize(ICharSequence, StringBuilder)
        public override StringBuilder Normalize(string src, StringBuilder dest)
        {
            dest.Length = 0;
            Normalize(src, new ReorderingBuffer(Impl, dest, src.Length));
            return dest;
        }



        public override StringBuilder Normalize(StringBuilder src, StringBuilder dest)
        {
            if (dest == src)
            {
                throw new ArgumentException($"'{nameof(src)}' cannot be the same instance as '{nameof(dest)}'");
            }
            dest.Length = 0;
            Normalize(src, new ReorderingBuffer(Impl, dest, src.Length));
            return dest;
        }



        public override StringBuilder Normalize(ICharSequence src, StringBuilder dest)
        {
            if (src is StringBuilderCharSequence && ((StringBuilderCharSequence)src).Value == dest)
            {
                throw new ArgumentException($"'{nameof(src)}' cannot be the same instance as '{nameof(dest)}'");
            }
            dest.Length = 0;
            Normalize(src, new ReorderingBuffer(Impl, dest, src.Length));
            return dest;
        }

#if FEATURE_SPAN

        // ICU4N TODO: Cascade this call to Normalize(ReadOnlySpan<char>, ref ValueStringBuilder) and append the result...?
        public override StringBuilder Normalize(ReadOnlySpan<char> src, StringBuilder dest)
        {
            dest.Length = 0;
            int length = src.Length;
            var buffer = length <= CharStackBufferSize
                ? new ValueReorderingBuffer(Impl, stackalloc char[CharStackBufferSize])
                : new ValueReorderingBuffer(Impl, ReadOnlySpan<char>.Empty, length);
            try
            {
                Normalize(src, ref buffer);
                dest.Length = 0;
                dest.Append(buffer.AsSpan());
            }
            finally
            {
                buffer.Dispose();
            }
            return dest;
        }
#endif
        #endregion Normalize(ICharSequence, StringBuilder)

        // ICU4N specific: Moved Normalize(ICharSequence, IAppendable) to Norm2AllModes.generated.tt
        #region Normalize(ICharSequence, IAppendable)
        public override IAppendable Normalize(string src, IAppendable dest)
        {
            ReorderingBuffer buffer = new ReorderingBuffer(Impl, dest, src.Length);
            Normalize(src, buffer);
            buffer.Flush();
            return dest;
        }



        public override IAppendable Normalize(StringBuilder src, IAppendable dest)
        {
            if (dest is StringBuilderCharSequence && ((StringBuilderCharSequence)dest).Value == src)
            {
                throw new ArgumentException($"'{nameof(src)}' cannot be the same instance as '{nameof(dest)}'");
            }
            ReorderingBuffer buffer = new ReorderingBuffer(Impl, dest, src.Length);
            Normalize(src, buffer);
            buffer.Flush();
            return dest;
        }



        public override IAppendable Normalize(ICharSequence src, IAppendable dest)
        {
            if ((dest == src) || (src is StringBuilderCharSequence && dest is StringBuilderCharSequence
                && ((StringBuilderCharSequence)src).Value == ((StringBuilderCharSequence)dest).Value))
            {
                throw new ArgumentException($"'{nameof(src)}' cannot be the same instance as '{nameof(dest)}'");
            }
            ReorderingBuffer buffer = new ReorderingBuffer(Impl, dest, src.Length);
            Normalize(src, buffer);
            buffer.Flush();
            return dest;
        }

#if FEATURE_SPAN


        public override IAppendable Normalize(ReadOnlySpan<char> src, IAppendable dest)
        {
            int length = src.Length;
            var buffer = length <= CharStackBufferSize
                ? new ValueReorderingBuffer(Impl, stackalloc char[CharStackBufferSize])
                : new ValueReorderingBuffer(Impl, ReadOnlySpan<char>.Empty, length);
            try
            {
                Normalize(src, ref buffer);
                dest.Append(buffer.AsSpan());
                buffer.Flush();
            }
            finally
            {
                buffer.Dispose();
            }
            return dest;
        }
#endif
        #endregion Normalize(ICharSequence, IAppendable)

        // ICU4N specific: Moved Normalize(ICharSequence, ReorderingBuffer) to Norm2AllModes.generated.tt
        #region Normalize(ICharSequence, ReorderingBuffer)
        protected abstract void Normalize(string src, ReorderingBuffer buffer);



        protected abstract void Normalize(StringBuilder src, ReorderingBuffer buffer);



        protected abstract void Normalize(ICharSequence src, ReorderingBuffer buffer);

#if FEATURE_SPAN


        protected abstract void Normalize(ReadOnlySpan<char> src, ref ValueReorderingBuffer buffer);
#endif
        #endregion Normalize(ICharSequence, ReorderingBuffer)

        // normalize and append

        // ICU4N specific: Moved NormalizeSecondAndAppend(StringBuilder, ICharSequence) to Norm2AllModes.generated.tt
        #region NormalizeSecondAndAppend(StringBuilder, ICharSequence)
        public override StringBuilder NormalizeSecondAndAppend(StringBuilder first, string second)
        {
            return NormalizeSecondAndAppend(first, second, true);
        }



        public override StringBuilder NormalizeSecondAndAppend(StringBuilder first, StringBuilder second)
        {
            return NormalizeSecondAndAppend(first, second, true);
        }



        public override StringBuilder NormalizeSecondAndAppend(StringBuilder first, ICharSequence second)
        {
            return NormalizeSecondAndAppend(first, second, true);
        }

#if FEATURE_SPAN


        public override StringBuilder NormalizeSecondAndAppend(StringBuilder first, ReadOnlySpan<char> second)
        {
            return NormalizeSecondAndAppend(first, second, true);
        }
#endif
        #endregion NormalizeSecondAndAppend(StringBuilder, ICharSequence)

        // ICU4N specific: Moved Append(StringBuilder, ICharSequence) to Norm2AllModes.generated.tt
        #region Append(StringBuilder, ICharSequence)
        public override StringBuilder Append(StringBuilder first, string second)
        {
            return NormalizeSecondAndAppend(first, second, false);
        }



        public override StringBuilder Append(StringBuilder first, StringBuilder second)
        {
            return NormalizeSecondAndAppend(first, second, false);
        }



        public override StringBuilder Append(StringBuilder first, ICharSequence second)
        {
            return NormalizeSecondAndAppend(first, second, false);
        }

#if FEATURE_SPAN


        public override StringBuilder Append(StringBuilder first, ReadOnlySpan<char> second)
        {
            return NormalizeSecondAndAppend(first, second, false);
        }
#endif
        #endregion

        // ICU4N specific: Moved NormalizeSecondAndAppend(StringBuilder, ICharSequence, bool) to Norm2AllModes.generated.tt
        #region NormalizeSecondAndAppend(StringBuilder, ICharSequence, bool)
        public virtual StringBuilder NormalizeSecondAndAppend(StringBuilder first, string second, bool doNormalize)
        {
            NormalizeAndAppend(
                second, doNormalize,
                new ReorderingBuffer(Impl, first, first.Length + second.Length));
            return first;
        }



        public virtual StringBuilder NormalizeSecondAndAppend(StringBuilder first, StringBuilder second, bool doNormalize)
        {
            if (first == second)
            {
                throw new ArgumentException($"'{nameof(first)}' cannot be the same instance as '{nameof(second)}'");
            }
            NormalizeAndAppend(
                second, doNormalize,
                new ReorderingBuffer(Impl, first, first.Length + second.Length));
            return first;
        }



        public virtual StringBuilder NormalizeSecondAndAppend(StringBuilder first, ICharSequence second, bool doNormalize)
        {
            if (second is StringBuilderCharSequence && ((StringBuilderCharSequence)second).Value == first)
            {
                throw new ArgumentException($"'{nameof(first)}' cannot be the same instance as '{nameof(second)}'");
            }
            NormalizeAndAppend(
                second, doNormalize,
                new ReorderingBuffer(Impl, first, first.Length + second.Length));
            return first;
        }

#if FEATURE_SPAN


        public virtual StringBuilder NormalizeSecondAndAppend(StringBuilder first, ReadOnlySpan<char> second, bool doNormalize)
        {
            int length = first.Length + second.Length;
            var sb = length <= CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                : new ValueStringBuilder(length);
            try
            {
                sb.Append(first);
                var buffer = new ValueReorderingBuffer(Impl, ref sb, length);
                NormalizeAndAppend(second, doNormalize, ref buffer);
                first.Length = 0;
                first.Append(buffer.AsSpan());
            }
            finally
            {
                sb.Dispose();
            }
            return first;
        }
#endif
        #endregion NormalizeSecondAndAppend(StringBuilder, ICharSequence, bool)

        // ICU4N specific: Moved NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer) to Norm2AllModes.generated.tt
        #region NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer)
        protected abstract void NormalizeAndAppend(
            string src, bool doNormalize, ReorderingBuffer buffer);



        protected abstract void NormalizeAndAppend(
            StringBuilder src, bool doNormalize, ReorderingBuffer buffer);



        protected abstract void NormalizeAndAppend(
            ICharSequence src, bool doNormalize, ReorderingBuffer buffer);

#if FEATURE_SPAN


        protected abstract void NormalizeAndAppend(
            ReadOnlySpan<char> src, bool doNormalize, ref ValueReorderingBuffer buffer);
#endif 
        #endregion NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer)

        public override string GetDecomposition(int c)
        {
            return Impl.GetDecomposition(c);
        }

        public override string GetRawDecomposition(int c)
        {
            return Impl.GetRawDecomposition(c);
        }

        public override int ComposePair(int a, int b)
        {
            return Impl.ComposePair(a, b);
        }


        public override int GetCombiningClass(int c)
        {
            return Impl.GetCC(Impl.GetNorm16(c));
        }

        // quick checks

        // ICU4N specific: Moved IsNormalized(ICharSequence) to Norm2AllModes.generated.tt
        #region IsNormalized(ICharSequence)
        public override bool IsNormalized(string s)
        {
            return s.Length == SpanQuickCheckYes(s);
        }



        public override bool IsNormalized(StringBuilder s)
        {
            return s.Length == SpanQuickCheckYes(s);
        }



        public override bool IsNormalized(ICharSequence s)
        {
            return s.Length == SpanQuickCheckYes(s);
        }

#if FEATURE_SPAN


        public override bool IsNormalized(ReadOnlySpan<char> s)
        {
            return s.Length == SpanQuickCheckYes(s);
        }
#endif
        #endregion

        // ICU4N specific: Moved QuickCheck(ICharSequence s) to Norm2AllModes.generated.tt
        #region QuickCheck(ICharSequence s)
        public override QuickCheckResult QuickCheck(string s)
        {
            return IsNormalized(s) ? QuickCheckResult.Yes : QuickCheckResult.No;
        }



        public override QuickCheckResult QuickCheck(StringBuilder s)
        {
            return IsNormalized(s) ? QuickCheckResult.Yes : QuickCheckResult.No;
        }



        public override QuickCheckResult QuickCheck(ICharSequence s)
        {
            return IsNormalized(s) ? QuickCheckResult.Yes : QuickCheckResult.No;
        }

#if FEATURE_SPAN


        public override QuickCheckResult QuickCheck(ReadOnlySpan<char> s)
        {
            return IsNormalized(s) ? QuickCheckResult.Yes : QuickCheckResult.No;
        }
#endif 
        #endregion QuickCheck(ICharSequence s)


        public abstract int GetQuickCheck(int c);

        public Normalizer2Impl Impl { get; private set; }
    }

    public sealed partial class DecomposeNormalizer2 : Normalizer2WithImpl
    {
        public DecomposeNormalizer2(Normalizer2Impl ni)
            : base(ni)
        {
        }

        // ICU4N specific: Moved Normalize(ICharSequence, ReorderingBuffer) to Norm2AllModes.generated.tt
        #region Normalize(ICharSequence, ReorderingBuffer)
        protected override void Normalize(string src, ReorderingBuffer buffer)
        {
            Impl.Decompose(src, 0, src.Length, buffer); // ICU4N: Checked 3rd parameter
        }



        protected override void Normalize(StringBuilder src, ReorderingBuffer buffer)
        {
            Impl.Decompose(src, 0, src.Length, buffer); // ICU4N: Checked 3rd parameter
        }



        protected override void Normalize(ICharSequence src, ReorderingBuffer buffer)
        {
            Impl.Decompose(src, 0, src.Length, buffer); // ICU4N: Checked 3rd parameter
        }

#if FEATURE_SPAN


        protected override void Normalize(ReadOnlySpan<char> src, ref ValueReorderingBuffer buffer)
        {
            Impl.Decompose(src, ref buffer);
        }
#endif
        #endregion Normalize(ICharSequence, ReorderingBuffer)

        // ICU4N specific: Moved NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer) to Norm2AllModes.generated.tt
        #region NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer)
        protected override void NormalizeAndAppend(string src, bool doNormalize, ReorderingBuffer buffer)
        {
            Impl.DecomposeAndAppend(src, doNormalize, buffer);
        }



        protected override void NormalizeAndAppend(StringBuilder src, bool doNormalize, ReorderingBuffer buffer)
        {
            Impl.DecomposeAndAppend(src, doNormalize, buffer);
        }



        protected override void NormalizeAndAppend(ICharSequence src, bool doNormalize, ReorderingBuffer buffer)
        {
            Impl.DecomposeAndAppend(src, doNormalize, buffer);
        }

#if FEATURE_SPAN


        protected override void NormalizeAndAppend(ReadOnlySpan<char> src, bool doNormalize, ref ValueReorderingBuffer buffer)
        {
            Impl.DecomposeAndAppend(src, doNormalize, ref buffer);
        }
#endif
        #endregion NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer)

        // ICU4N specific: Moved SpanQuickCheckYes(ICharSequence) to Norm2AllModes.generated.tt
        #region SpanQuickCheckYes(ICharSequence)
        public override int SpanQuickCheckYes(string s)
        {
            return Impl.DecomposeQuickCheck(s, 0, s.Length); // ICU4N: Changed to a separate method so we can use a ref struct for a buffer
        }



        public override int SpanQuickCheckYes(StringBuilder s)
        {
            return Impl.DecomposeQuickCheck(s, 0, s.Length); // ICU4N: Changed to a separate method so we can use a ref struct for a buffer
        }



        public override int SpanQuickCheckYes(ICharSequence s)
        {
            return Impl.DecomposeQuickCheck(s, 0, s.Length); // ICU4N: Changed to a separate method so we can use a ref struct for a buffer
        }

#if FEATURE_SPAN


        public override int SpanQuickCheckYes(ReadOnlySpan<char> s)
        {
            return Impl.DecomposeQuickCheck(s); // ICU4N: Changed to a separate method so we can use a ref struct for a buffer
        }
#endif 
        #endregion

        public override int GetQuickCheck(int c)
        {
            return Impl.IsDecompYes(Impl.GetNorm16(c)) ? 1 : 0;
        }

        public override bool HasBoundaryBefore(int c) { return Impl.HasDecompBoundaryBefore(c); }

        public override bool HasBoundaryAfter(int c) { return Impl.HasDecompBoundaryAfter(c); }

        public override bool IsInert(int c) { return Impl.IsDecompInert(c); }
    }

    public sealed partial class ComposeNormalizer2 : Normalizer2WithImpl
    {
        public ComposeNormalizer2(Normalizer2Impl ni, bool fcc)
            : base(ni)
        {
            onlyContiguous = fcc;
        }

        // ICU4N specific: Moved Normalize(ICharSequence, ReorderingBuffer) to Norm2AllModes.generated.tt
        #region Normalize(ICharSequence, ReorderingBuffer)
        protected override void Normalize(string src, ReorderingBuffer buffer)
        {
            Impl.Compose(src, 0, src.Length, onlyContiguous, true, buffer); // ICU4N: Checked 3rd parameter
        }



        protected override void Normalize(StringBuilder src, ReorderingBuffer buffer)
        {
            Impl.Compose(src, 0, src.Length, onlyContiguous, true, buffer); // ICU4N: Checked 3rd parameter
        }



        protected override void Normalize(ICharSequence src, ReorderingBuffer buffer)
        {
            Impl.Compose(src, 0, src.Length, onlyContiguous, true, buffer); // ICU4N: Checked 3rd parameter
        }

#if FEATURE_SPAN


        protected override void Normalize(ReadOnlySpan<char> src, ref ValueReorderingBuffer buffer)
        {
            Impl.Compose(src, onlyContiguous, true, ref buffer);
        }
#endif 
        #endregion Normalize(ICharSequence, ReorderingBuffer)

        // ICU4N specific: Moved NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer) to Norm2AllModes.generated.tt
        #region NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer)
        protected override void NormalizeAndAppend(
            string src, bool doNormalize, ReorderingBuffer buffer)
        {
            Impl.ComposeAndAppend(src, doNormalize, onlyContiguous, buffer);
        }



        protected override void NormalizeAndAppend(
            StringBuilder src, bool doNormalize, ReorderingBuffer buffer)
        {
            Impl.ComposeAndAppend(src, doNormalize, onlyContiguous, buffer);
        }



        protected override void NormalizeAndAppend(
            ICharSequence src, bool doNormalize, ReorderingBuffer buffer)
        {
            Impl.ComposeAndAppend(src, doNormalize, onlyContiguous, buffer);
        }

#if FEATURE_SPAN


        protected override void NormalizeAndAppend(
            ReadOnlySpan<char> src, bool doNormalize, ref ValueReorderingBuffer buffer)
        {
            Impl.ComposeAndAppend(src, doNormalize, onlyContiguous, ref buffer);
        }
#endif 
        #endregion NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer)

        // ICU4N specific: Moved IsNormalized(ICharSequence) to Norm2AllModes.generated.tt
        #region IsNormalized(ICharSequence)
        public override bool IsNormalized(string s)
        {
            // 5: small destCapacity for substring normalization
            return Impl.Compose(s, 0, s.Length,
                                onlyContiguous, false,
                                new ReorderingBuffer(Impl, new StringBuilder(), 5)); // ICU4N: Checked 3rd parameter
        }



        public override bool IsNormalized(StringBuilder s)
        {
            // 5: small destCapacity for substring normalization
            return Impl.Compose(s, 0, s.Length,
                                onlyContiguous, false,
                                new ReorderingBuffer(Impl, new StringBuilder(), 5)); // ICU4N: Checked 3rd parameter
        }



        public override bool IsNormalized(ICharSequence s)
        {
            // 5: small destCapacity for substring normalization
            return Impl.Compose(s, 0, s.Length,
                                onlyContiguous, false,
                                new ReorderingBuffer(Impl, new StringBuilder(), 5)); // ICU4N: Checked 3rd parameter
        }

#if FEATURE_SPAN


        public override bool IsNormalized(ReadOnlySpan<char> s)
        {
            // 5: small destCapacity for substring normalization
            var buffer = new ValueReorderingBuffer(Impl, stackalloc char[5]);
            try
            {
                return Impl.Compose(s, onlyContiguous, false, ref buffer);
            }
            finally
            {
                buffer.Dispose();
            }
        }
#endif 
        #endregion IsNormalized(ICharSequence)

        // ICU4N specific: Moved QuickCheck(ICharSequence) to Norm2AllModes.generated.tt
        #region QuickCheck(ICharSequence)
        public override QuickCheckResult QuickCheck(string s)
        {
            int spanLengthAndMaybe = Impl.ComposeQuickCheck(s, 0, s.Length, onlyContiguous, false);
            if ((spanLengthAndMaybe & 1) != 0)
            {
                return QuickCheckResult.Maybe;
            }
            else if ((spanLengthAndMaybe.TripleShift(1)) == s.Length)
            {
                return QuickCheckResult.Yes;
            }
            else
            {
                return QuickCheckResult.No;
            }
        }



        public override QuickCheckResult QuickCheck(StringBuilder s)
        {
            int spanLengthAndMaybe = Impl.ComposeQuickCheck(s, 0, s.Length, onlyContiguous, false);
            if ((spanLengthAndMaybe & 1) != 0)
            {
                return QuickCheckResult.Maybe;
            }
            else if ((spanLengthAndMaybe.TripleShift(1)) == s.Length)
            {
                return QuickCheckResult.Yes;
            }
            else
            {
                return QuickCheckResult.No;
            }
        }



        public override QuickCheckResult QuickCheck(ICharSequence s)
        {
            int spanLengthAndMaybe = Impl.ComposeQuickCheck(s, 0, s.Length, onlyContiguous, false);
            if ((spanLengthAndMaybe & 1) != 0)
            {
                return QuickCheckResult.Maybe;
            }
            else if ((spanLengthAndMaybe.TripleShift(1)) == s.Length)
            {
                return QuickCheckResult.Yes;
            }
            else
            {
                return QuickCheckResult.No;
            }
        }

#if FEATURE_SPAN


        public override QuickCheckResult QuickCheck(ReadOnlySpan<char> s)
        {
            int spanLengthAndMaybe = Impl.ComposeQuickCheck(s, onlyContiguous, false);
            if ((spanLengthAndMaybe & 1) != 0)
            {
                return QuickCheckResult.Maybe;
            }
            else if ((spanLengthAndMaybe.TripleShift(1)) == s.Length)
            {
                return QuickCheckResult.Yes;
            }
            else
            {
                return QuickCheckResult.No;
            }
        }
#endif 
        #endregion QuickCheck(ICharSequence)

        // ICU4N specific: Moved SpanQuickCheckYes(ICharSequence) to Norm2AllModes.generated.tt
        #region SpanQuickCheckYes(ICharSequence)
        public override int SpanQuickCheckYes(string s)
        {
            return Impl.ComposeQuickCheck(s, 0, s.Length, onlyContiguous, true).TripleShift(1); // ICU4N: Checked 3rd parameter
        }



        public override int SpanQuickCheckYes(StringBuilder s)
        {
            return Impl.ComposeQuickCheck(s, 0, s.Length, onlyContiguous, true).TripleShift(1); // ICU4N: Checked 3rd parameter
        }



        public override int SpanQuickCheckYes(ICharSequence s)
        {
            return Impl.ComposeQuickCheck(s, 0, s.Length, onlyContiguous, true).TripleShift(1); // ICU4N: Checked 3rd parameter
        }

#if FEATURE_SPAN


        public override int SpanQuickCheckYes(ReadOnlySpan<char> s)
        {
            return Impl.ComposeQuickCheck(s, onlyContiguous, true).TripleShift(1); // ICU4N: Checked 3rd parameter
        }
#endif 
        #endregion SpanQuickCheckYes(ICharSequence)

        public override int GetQuickCheck(int c)
        {
            return Impl.GetCompQuickCheck(Impl.GetNorm16(c));
        }

        public override bool HasBoundaryBefore(int c) { return Impl.HasCompBoundaryBefore(c); }

        public override bool HasBoundaryAfter(int c)
        {
            return Impl.HasCompBoundaryAfter(c, onlyContiguous);
        }

        public override bool IsInert(int c)
        {
            return Impl.IsCompInert(c, onlyContiguous);
        }

        private readonly bool onlyContiguous;
    }

    public sealed partial class FCDNormalizer2 : Normalizer2WithImpl
    {
        public FCDNormalizer2(Normalizer2Impl ni)
            : base(ni)
        {
        }

        // ICU4N specific: Moved Normalize(ICharSequence, ReorderingBuffer) to Norm2AllModes.generated.tt
        #region Normalize(ICharSequence, ReorderingBuffer)
        protected override void Normalize(string src, ReorderingBuffer buffer)
        {
            Impl.MakeFCD(src, 0, src.Length, buffer); // ICU4N: Checked 3rd parameter
        }



        protected override void Normalize(StringBuilder src, ReorderingBuffer buffer)
        {
            Impl.MakeFCD(src, 0, src.Length, buffer); // ICU4N: Checked 3rd parameter
        }



        protected override void Normalize(ICharSequence src, ReorderingBuffer buffer)
        {
            Impl.MakeFCD(src, 0, src.Length, buffer); // ICU4N: Checked 3rd parameter
        }

#if FEATURE_SPAN


        protected override void Normalize(ReadOnlySpan<char> src, ref ValueReorderingBuffer buffer)
        {
            Impl.MakeFCD(src, ref buffer);
        }
#endif 
        #endregion Normalize(ICharSequence, ReorderingBuffer)

        // ICU4N specific: Moved NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer) to Norm2AllModes.generated.tt
        #region NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer)
        protected override void NormalizeAndAppend(
            string src, bool doNormalize, ReorderingBuffer buffer)
        {
            Impl.MakeFCDAndAppend(src, doNormalize, buffer);
        }



        protected override void NormalizeAndAppend(
            StringBuilder src, bool doNormalize, ReorderingBuffer buffer)
        {
            Impl.MakeFCDAndAppend(src, doNormalize, buffer);
        }



        protected override void NormalizeAndAppend(
            ICharSequence src, bool doNormalize, ReorderingBuffer buffer)
        {
            Impl.MakeFCDAndAppend(src, doNormalize, buffer);
        }

#if FEATURE_SPAN


        protected override void NormalizeAndAppend(
            ReadOnlySpan<char> src, bool doNormalize, ref ValueReorderingBuffer buffer)
        {
            Impl.MakeFCDAndAppend(src, doNormalize, ref buffer);
        }
#endif
        #endregion NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer)

        // ICU4N specific: Moved SpanQuickCheckYes(ICharSequence) to Norm2AllModes.generated.tt
        #region SpanQuickCheckYes(ICharSequence)
        public override int SpanQuickCheckYes(string s)
        {
            return Impl.MakeFCDQuickCheck(s, 0, s.Length); // ICU4N: Checked 3rd parameter
        }



        public override int SpanQuickCheckYes(StringBuilder s)
        {
            return Impl.MakeFCDQuickCheck(s, 0, s.Length); // ICU4N: Checked 3rd parameter
        }



        public override int SpanQuickCheckYes(ICharSequence s)
        {
            return Impl.MakeFCDQuickCheck(s, 0, s.Length); // ICU4N: Checked 3rd parameter
        }

#if FEATURE_SPAN


        public override int SpanQuickCheckYes(ReadOnlySpan<char> s)
        {
            return Impl.MakeFCDQuickCheck(s); // ICU4N: Checked 3rd parameter
        }
#endif
        #endregion SpanQuickCheckYes(ICharSequence)


        public override int GetQuickCheck(int c)
        {
            return Impl.IsDecompYes(Impl.GetNorm16(c)) ? 1 : 0;
        }

        public override bool HasBoundaryBefore(int c) { return Impl.HasFCDBoundaryBefore(c); }

        public override bool HasBoundaryAfter(int c) { return Impl.HasFCDBoundaryAfter(c); }

        public override bool IsInert(int c) { return Impl.IsFCDInert(c); }
    }

    public sealed class Norm2AllModes
    {
        // Public API dispatch via Normalizer2 subclasses -------------------------- ***

        // ICU4N: De-nested NoopNormalizer2

        // ICU4N: De-nested Normalizer2WithImpl

        // ICU4N: De-nested DecomposeNormalizer2

        // ICU4N: De-nested ComposeNormalizer2

        // ICU4N: De-nested FCDNormalizer2


        // instance cache ---------------------------------------------------------- ***

        private Norm2AllModes(Normalizer2Impl ni)
        {
            Impl = ni;
            Comp = new ComposeNormalizer2(ni, false);
            Decomp = new DecomposeNormalizer2(ni);
            Fcd = new FCDNormalizer2(ni);
            Fcc = new ComposeNormalizer2(ni, true);
        }

        public Normalizer2Impl Impl { get; private set; }
        public ComposeNormalizer2 Comp { get; private set; }
        public DecomposeNormalizer2 Decomp { get; private set; }
        public FCDNormalizer2 Fcd { get; private set; }
        public ComposeNormalizer2 Fcc { get; private set; }

        private static Norm2AllModes GetInstanceFromSingleton(Norm2AllModesSingleton singleton)
        {
            if (singleton.exception != null)
            {
                throw singleton.exception;
            }
            return singleton.allModes;
        }
        public static Norm2AllModes NFCInstance
            => GetInstanceFromSingleton(NFCSingleton.Instance);

        public static Norm2AllModes NFKCInstance
            => GetInstanceFromSingleton(NFKCSingleton.Instance);
        public static Norm2AllModes NFKC_CFInstance
            => GetInstanceFromSingleton(NFKC_CFSingleton.Instance);
        // For use in properties APIs.
        public static Normalizer2WithImpl GetN2WithImpl(int index)
        {
            switch (index)
            {
                case 0: return NFCInstance.Decomp;  // NFD
                case 1: return NFKCInstance.Decomp; // NFKD
                case 2: return NFCInstance.Comp;    // NFC
                case 3: return NFKCInstance.Comp;   // NFKC
                default: return null;
            }
        }
        public static Norm2AllModes GetInstance(ByteBuffer bytes, string name) // ICU4N TODO: API - Eliminate ByteBuffer
        {
            if (bytes == null)
            {
                Norm2AllModesSingleton singleton;
                if (name.Equals("nfc", StringComparison.OrdinalIgnoreCase))
                {
                    singleton = NFCSingleton.Instance;
                }
                else if (name.Equals("nfkc", StringComparison.OrdinalIgnoreCase))
                {
                    singleton = NFKCSingleton.Instance;
                }
                else if (name.Equals("nfkc_cf", StringComparison.OrdinalIgnoreCase))
                {
                    singleton = NFKC_CFSingleton.Instance;
                }
                else
                {
                    singleton = null;
                }
                if (singleton != null)
                {
                    if (singleton.exception != null)
                    {
                        throw singleton.exception;
                    }
                    return singleton.allModes;
                }
            }
            return cache.GetOrCreate(name, (key) =>
            {
                Normalizer2Impl impl;
                if (bytes == null)
                {
                    impl = new Normalizer2Impl().Load(key + ".nrm");
                }
                else
                {
                    impl = new Normalizer2Impl().Load(bytes);
                }
                return new Norm2AllModes(impl);
            });
        }

        private static readonly CacheBase<string, Norm2AllModes> cache = new SoftCache<string, Norm2AllModes>();

        // ICU4N: Factored out Norm2SoftCache and changed to GetOrCreate() method that
        // uses a delegate to do all of this inline.

        public static readonly NoopNormalizer2 NoopNormalizer2 = new NoopNormalizer2();

        /// <summary>
        /// Gets the FCD normalizer, with the FCD data initialized.
        /// </summary>
        /// <returns>FCD normalizer.</returns>
        public static Normalizer2 FCDNormalizer2
            => NFCInstance.Fcd;
        internal sealed class Norm2AllModesSingleton
        {
            public Norm2AllModesSingleton(string name)
            {
                try
                {
                    Normalizer2Impl impl = new Normalizer2Impl().Load(name + ".nrm");
                    allModes = new Norm2AllModes(impl);
                }
                catch (Exception e)
                {
                    exception = e;
                }
            }

            internal Norm2AllModes allModes;
            internal Exception exception;
        }
        internal sealed class NFCSingleton
        {
            private static readonly Norm2AllModesSingleton instance = new Norm2AllModesSingleton("nfc");
            public static Norm2AllModesSingleton Instance => instance;
        }
        internal sealed class NFKCSingleton
        {
            private static readonly Norm2AllModesSingleton instance = new Norm2AllModesSingleton("nfkc");
            public static Norm2AllModesSingleton Instance => instance;
        }
        internal sealed class NFKC_CFSingleton
        {
            private static readonly Norm2AllModesSingleton instance = new Norm2AllModesSingleton("nfkc_cf");
            public static Norm2AllModesSingleton Instance => instance;
        }
    }
}
