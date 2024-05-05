using ICU4N.Support.Text;
using ICU4N.Text;
using J2N.IO;
using J2N.Numerics;
using J2N.Text;
using System;
using System.Text;
#nullable enable

namespace ICU4N.Impl
{
    // Normalizer2 implementation for the old UNORM_NONE.
    public sealed partial class NoopNormalizer2 : Normalizer2
    {
        #region Normalize(ICharSequence, StringBuilder)
        public override StringBuilder Normalize(string src, StringBuilder dest)
        {
            if (src is null)
                throw new ArgumentNullException(nameof(src));
            if (dest is null)
                throw new ArgumentNullException(nameof(dest));

            dest.Length = 0;
            return dest.Append(src);
        }

        public override StringBuilder Normalize(ReadOnlySpan<char> src, StringBuilder dest)
        {
            if (dest is null)
                throw new ArgumentNullException(nameof(dest));

            dest.Length = 0;
            return dest.Append(src);
        }

        internal override void Normalize(ReadOnlySpan<char> src, ref ValueStringBuilder dest)
        {
            if (MemoryHelper.AreSame(src, dest.RawChars))
            {
                throw new ArgumentException($"'{nameof(src)}' cannot be the same memory location as '{nameof(dest)}'");
            }

            dest.Length = 0;
            dest.Append(src);
        }

        #endregion

        #region Normalize(ICharSequence, IAppendable)
        public override TAppendable Normalize<TAppendable>(string src, TAppendable dest)
        {
            if (src is null)
                throw new ArgumentNullException(nameof(src));
            if (dest is null)
                throw new ArgumentNullException(nameof(dest));

            // ICU4N: Removed unnecessary try/catch for IOException
            dest.Append(src);
            return dest;
        }

        public override TAppendable Normalize<TAppendable>(ReadOnlySpan<char> src, TAppendable dest)
        {
            if (dest is null)
                throw new ArgumentNullException(nameof(dest));

            // ICU4N: Removed unnecessary try/catch for IOException
            return dest.Append(src);
        }

        #endregion Normalize(ICharSequence, IAppendable)

        #region NormalizeSecondAndAppend(StringBuilder, ICharSequence)
        public override StringBuilder NormalizeSecondAndAppend(StringBuilder first, string second)
        {
            if (first is null)
                throw new ArgumentNullException(nameof(first));
            if (second is null)
                throw new ArgumentNullException(nameof(second));

            return first.Append(second);
        }

        public override StringBuilder NormalizeSecondAndAppend(StringBuilder first, ReadOnlySpan<char> second)
        {
            if (first is null)
                throw new ArgumentNullException(nameof(first));

            return first.Append(second);
        }

        internal override void NormalizeSecondAndAppend(ref ValueStringBuilder first, ReadOnlySpan<char> second)
        {
            if (MemoryHelper.AreSame(first.RawChars, second))
            {
                throw new ArgumentException($"'{nameof(first)}' cannot be the same memory location as '{nameof(second)}'");
            }

            first.Append(second);
        }

        #endregion NormalizeSecondAndAppend(StringBuilder, ICharSequence)

        #region Append(StringBuilder, ICharSequence)
        public override StringBuilder Append(StringBuilder first, string second)
        {
            if (first is null)
                throw new ArgumentNullException(nameof(first));
            if (second is null)
                throw new ArgumentNullException(nameof(second));

            return first.Append(second);
        }

        public override StringBuilder Append(StringBuilder first, ReadOnlySpan<char> second)
        {
            if (first is null)
                throw new ArgumentNullException(nameof(first));

            return first.Append(second);
        }

        internal override void Append(ref ValueStringBuilder first, ReadOnlySpan<char> second)
        {
            if (MemoryHelper.AreSame(first.RawChars, second))
            {
                throw new ArgumentException($"'{nameof(first)}' cannot be the same memory location as '{nameof(second)}'");
            }

            first.Append(second);
        }

        #endregion

        #region IsNormalized(ICharSequence)
        public override bool IsNormalized(string s) => true;

        public override bool IsNormalized(ReadOnlySpan<char> s) => true;

        #endregion

        #region QuickCheck(ICharSequence)
        public override QuickCheckResult QuickCheck(string s) => QuickCheckResult.Yes;

        public override QuickCheckResult QuickCheck(ReadOnlySpan<char> s) => QuickCheckResult.Yes;

        #endregion QuickCheck(ICharSequence)

        #region SpanQuickCheckYes(ICharSequence)
        public override int SpanQuickCheckYes(string s) => s?.Length ?? 0;

        public override int SpanQuickCheckYes(ReadOnlySpan<char> s) => s.Length;

        #endregion SpanQuickCheckYes(ICharSequence)

        public override string? GetDecomposition(int c)
        {
            return null;
        }

        public override bool TryGetDecomposition(int codePoint, Span<char> destination, out int charsLength)
        {
            charsLength = 0;
            return false;
        }

        // No need to override the default GetRawDecomposition().

        // No need to override the default TryGetRawDecomposition()

        public override bool HasBoundaryBefore(int c) => true;

        public override bool HasBoundaryAfter(int c) => true;

        public override bool IsInert(int c) => true;

        public override bool TryNormalize(ReadOnlySpan<char> source, Span<char> destination, out int charsLength)
        {
            if (MemoryHelper.AreSame(source, destination))
            {
                throw new ArgumentException($"'{nameof(source)}' cannot be the same memory location as '{nameof(destination)}'");
            }
            bool success = source.TryCopyTo(destination);
            charsLength = source.Length;
            return success;
        }

        public override bool TryNormalizeSecondAndConcat(ReadOnlySpan<char> first, ReadOnlySpan<char> second, Span<char> destination, out int charsLength)
        {
            bool success = true;
            if (MemoryHelper.AreSame(second, destination))
            {
                throw new ArgumentException($"'{nameof(second)}' cannot be the same memory location as '{nameof(destination)}'");
            }
            if (!MemoryHelper.AreSame(first, destination))
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
            if (MemoryHelper.AreSame(second, destination))
            {
                throw new ArgumentException($"'{nameof(second)}' cannot be the same memory location as '{nameof(destination)}'");
            }
            if (!MemoryHelper.AreSame(first, destination))
            {
                success = first.TryCopyTo(destination);
            }
            success = success && second.TryCopyTo(destination.Slice(first.Length));
            charsLength = first.Length + second.Length;
            return success;
        }
    }

    // Intermediate class:
    // Has Normalizer2Impl and does boilerplate argument checking and setup.
    public abstract partial class Normalizer2WithImpl : Normalizer2
    {
        public Normalizer2WithImpl(Normalizer2Impl ni)
        {
            Impl = ni ?? throw new ArgumentNullException(nameof(ni)); // ICU4N: Added guard clause
        }

        // normalize

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
                : new ValueReorderingBuffer(Impl, length);
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
            if (MemoryHelper.AreSame(source, destination))
            {
                throw new ArgumentException($"'{nameof(source)}' cannot be the same memory location as '{nameof(destination)}'");
            }
            int length = source.Length;
            var buffer = length <= CharStackBufferSize
                ? new ValueReorderingBuffer(Impl, stackalloc char[CharStackBufferSize])
                : new ValueReorderingBuffer(Impl, length);
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

        public override bool TryNormalizeSecondAndConcat(ReadOnlySpan<char> first, ReadOnlySpan<char> second, Span<char> destination, out int charsLength)
        {
            if (MemoryHelper.AreSame(second, destination))
            {
                throw new ArgumentException($"'{nameof(second)}' cannot be the same memory location as '{nameof(destination)}'");
            }
            bool success = true;
            if (!MemoryHelper.AreSame(first, destination))
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
            if (MemoryHelper.AreSame(second, destination))
            {
                throw new ArgumentException($"'{nameof(second)}' cannot be the same memory location as '{nameof(destination)}'");
            }
            bool success = true;
            if (!MemoryHelper.AreSame(first, destination))
            {
                success = first.TryCopyTo(destination);
            }
            var buffer = new ValueReorderingBuffer(Impl, destination);
            NormalizeAndAppend(second, doNormalize: false, ref buffer);
            success = success && buffer.Length <= destination.Length;
            charsLength = buffer.Length;
            return success;
        }

        #region Normalize(ICharSequence, StringBuilder)
        public override StringBuilder Normalize(string src, StringBuilder dest)
        {
            if (src is null)
                throw new ArgumentNullException(nameof(src));
            if (dest is null)
                throw new ArgumentNullException(nameof(dest));

            dest.Length = 0;
            int length = src.Length;
            var buffer = length <= CharStackBufferSize
                ? new ValueReorderingBuffer(Impl, stackalloc char[CharStackBufferSize])
                : new ValueReorderingBuffer(Impl, length);
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

        // ICU4N TODO: Cascade this call to Normalize(ReadOnlySpan<char>, ref ValueStringBuilder) and append the result...?
        public override StringBuilder Normalize(ReadOnlySpan<char> src, StringBuilder dest)
        {
            if (dest is null)
                throw new ArgumentNullException(nameof(dest));

            dest.Length = 0;
            int length = src.Length;
            var buffer = length <= CharStackBufferSize
                ? new ValueReorderingBuffer(Impl, stackalloc char[CharStackBufferSize])
                : new ValueReorderingBuffer(Impl, length);
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

        internal override void Normalize(ReadOnlySpan<char> src, ref ValueStringBuilder dest)
        {
            if (MemoryHelper.AreSame(src, dest.RawChars))
            {
                throw new ArgumentException($"'{nameof(src)}' cannot be the same memory location as '{nameof(dest)}'");
            }

            dest.Length = 0;
            int length = src.Length;
            var buffer = length <= CharStackBufferSize
                ? new ValueReorderingBuffer(Impl, stackalloc char[CharStackBufferSize])
                : new ValueReorderingBuffer(Impl, length);
            try
            {
                Normalize(src, ref buffer);
                dest.Length = 0;
                unsafe
                {
                    dest.Append(new ReadOnlySpan<char>(buffer.GetCharsPointer(), buffer.Length));
                }
            }
            finally
            {
                buffer.Dispose();
            }
        }

        #endregion Normalize(ICharSequence, StringBuilder)

        #region Normalize(ICharSequence, IAppendable)
        public override TAppendable Normalize<TAppendable>(string src, TAppendable dest)
        {
            if (src is null)
                throw new ArgumentNullException(nameof(src));

            int length = src.Length;
            var buffer = length <= CharStackBufferSize
                ? new ValueReorderingBuffer(Impl, stackalloc char[CharStackBufferSize])
                : new ValueReorderingBuffer(Impl, length);

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

        public override TAppendable Normalize<TAppendable>(ReadOnlySpan<char> src, TAppendable dest)
        {
            int length = src.Length;
            var buffer = length <= CharStackBufferSize
                ? new ValueReorderingBuffer(Impl, stackalloc char[CharStackBufferSize])
                : new ValueReorderingBuffer(Impl, length);
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

        #endregion Normalize(ICharSequence, IAppendable)

        #region Normalize(ICharSequence, ReorderingBuffer)
        protected abstract void Normalize(string src, ref ValueReorderingBuffer buffer);

        protected abstract void Normalize(ReadOnlySpan<char> src, ref ValueReorderingBuffer buffer);

        #endregion Normalize(ICharSequence, ReorderingBuffer)

        // normalize and append

        #region NormalizeSecondAndAppend(StringBuilder, ICharSequence)
        public override StringBuilder NormalizeSecondAndAppend(StringBuilder first, string second)
        {
            return NormalizeSecondAndAppend(first, second, doNormalize: true);
        }

        public override StringBuilder NormalizeSecondAndAppend(StringBuilder first, ReadOnlySpan<char> second)
        {
            return NormalizeSecondAndAppend(first, second, doNormalize: true);
        }

        internal override void NormalizeSecondAndAppend(ref ValueStringBuilder first, ReadOnlySpan<char> second)
        {
            NormalizeSecondAndAppend(ref first, second, doNormalize: true);
        }

        #endregion NormalizeSecondAndAppend(StringBuilder, ICharSequence)

        #region Append(StringBuilder, ICharSequence)
        public override StringBuilder Append(StringBuilder first, string second)
        {
            return NormalizeSecondAndAppend(first, second, false);
        }

        public override StringBuilder Append(StringBuilder first, ReadOnlySpan<char> second)
        {
            return NormalizeSecondAndAppend(first, second, false);
        }

        internal override void Append(ref ValueStringBuilder first, ReadOnlySpan<char> second)
        {
            NormalizeSecondAndAppend(ref first, second, false);
        }

        #endregion

        #region NormalizeSecondAndAppend(StringBuilder, ICharSequence, bool)
        public virtual StringBuilder NormalizeSecondAndAppend(StringBuilder first, string second, bool doNormalize)
        {
            if (first is null)
                throw new ArgumentNullException(nameof(first));
            if (second is null)
                throw new ArgumentNullException(nameof(second));

            int length = first.Length + second.Length;
            var sb = length <= CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                : new ValueStringBuilder(length);
            try
            {
                sb.Append(first);
                var buffer = new ValueReorderingBuffer(Impl, ref sb, length);
                NormalizeAndAppend(second.AsSpan(), doNormalize, ref buffer);
                first.Length = 0;
                first.Append(buffer.AsSpan());
            }
            finally
            {
                sb.Dispose();
            }
            return first;
        }

        public virtual StringBuilder NormalizeSecondAndAppend(StringBuilder first, ReadOnlySpan<char> second, bool doNormalize)
        {
            if (first is null)
                throw new ArgumentNullException(nameof(first));

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

        internal virtual void NormalizeSecondAndAppend(ref ValueStringBuilder first, ReadOnlySpan<char> second, bool doNormalize)
        {
            if (MemoryHelper.AreSame(first.RawChars, second))
            {
                throw new ArgumentException($"'{nameof(first)}' cannot be the same memory location as '{nameof(second)}'");
            }

            int length = first.Length + second.Length;
            var sb = length <= CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                : new ValueStringBuilder(length);
            try
            {
                sb.Append(first.AsSpan());
                var buffer = new ValueReorderingBuffer(Impl, ref sb, length);
                NormalizeAndAppend(second, doNormalize, ref buffer);
                first.Length = 0;
                unsafe
                {
                    first.Append(new ReadOnlySpan<char>(buffer.GetCharsPointer(), buffer.Length));
                }
            }
            finally
            {
                sb.Dispose();
            }
        }

        #endregion NormalizeSecondAndAppend(StringBuilder, ICharSequence, bool)

        #region NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer)
        protected abstract void NormalizeAndAppend(
            string src, bool doNormalize, ref ValueReorderingBuffer buffer);

        protected abstract void NormalizeAndAppend(
            ReadOnlySpan<char> src, bool doNormalize, ref ValueReorderingBuffer buffer);

        #endregion NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer)

        public override string? GetDecomposition(int c)
        {
            return Impl.GetDecomposition(c);
        }

        public override bool TryGetDecomposition(int codePoint, Span<char> destination, out int charsLength)
        {
            return Impl.TryGetDecomposition(codePoint, destination, out charsLength);
        }

        public override string? GetRawDecomposition(int c)
        {
            return Impl.GetRawDecomposition(c);
        }

        public override bool TryGetRawDecomposition(int codePoint, Span<char> destination, out int charsLength)
        {
            return Impl.TryGetRawDecomposition(codePoint, destination, out charsLength);
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

        #region IsNormalized(ICharSequence)
        public override bool IsNormalized(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            return s.Length == SpanQuickCheckYes(s);
        }

        public override bool IsNormalized(ReadOnlySpan<char> s)
        {
            return s.Length == SpanQuickCheckYes(s);
        }

        #endregion

        #region QuickCheck(ICharSequence s)
        public override QuickCheckResult QuickCheck(string s)
        {
            return IsNormalized(s) ? QuickCheckResult.Yes : QuickCheckResult.No;
        }

        public override QuickCheckResult QuickCheck(ReadOnlySpan<char> s)
        {
            return IsNormalized(s) ? QuickCheckResult.Yes : QuickCheckResult.No;
        }

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

        #region Normalize(ICharSequence, ReorderingBuffer)
        protected override void Normalize(string src, ref ValueReorderingBuffer buffer)
        {
            if (src is null)
                throw new ArgumentNullException(nameof(src));

            Impl.Decompose(src.AsSpan(), ref buffer); // ICU4N: Checked 3rd parameter
        }

        protected override void Normalize(ReadOnlySpan<char> src, ref ValueReorderingBuffer buffer)
        {
            if (MemoryHelper.AreSame(src, buffer.RawChars))
            {
                throw new ArgumentException($"'{nameof(src)}' cannot be the same memory location as '{nameof(buffer)}'");
            }

            Impl.Decompose(src, ref buffer);
        }

        #endregion Normalize(ICharSequence, ReorderingBuffer)

        #region NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer)
        protected override void NormalizeAndAppend(string src, bool doNormalize, ref ValueReorderingBuffer buffer)
        {
            if (src is null)
                throw new ArgumentNullException(nameof(src));

            Impl.DecomposeAndAppend(src.AsSpan(), doNormalize, ref buffer);
        }

        protected override void NormalizeAndAppend(ReadOnlySpan<char> src, bool doNormalize, ref ValueReorderingBuffer buffer)
        {
            if (MemoryHelper.AreSame(src, buffer.RawChars))
            {
                throw new ArgumentException($"'{nameof(src)}' cannot be the same memory location as '{nameof(buffer)}'");
            }

            Impl.DecomposeAndAppend(src, doNormalize, ref buffer);
        }

        #endregion NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer)

        #region SpanQuickCheckYes(ICharSequence)
        public override int SpanQuickCheckYes(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            return Impl.DecomposeQuickCheck(s.AsSpan()); // ICU4N: Changed to a separate method so we can use a ref struct for a buffer
        }

        public override int SpanQuickCheckYes(ReadOnlySpan<char> s)
        {
            return Impl.DecomposeQuickCheck(s); // ICU4N: Changed to a separate method so we can use a ref struct for a buffer
        }

        #endregion

        public override int GetQuickCheck(int c)
        {
            return Impl.IsDecompYes(Impl.GetNorm16(c)) ? 1 : 0;
        }

        public override bool HasBoundaryBefore(int c) => Impl.HasDecompBoundaryBefore(c);

        public override bool HasBoundaryAfter(int c) => Impl.HasDecompBoundaryAfter(c);

        public override bool IsInert(int c) => Impl.IsDecompInert(c);
    }

    public sealed partial class ComposeNormalizer2 : Normalizer2WithImpl
    {
        public ComposeNormalizer2(Normalizer2Impl ni, bool fcc)
            : base(ni)
        {
            onlyContiguous = fcc;
        }

        #region Normalize(ICharSequence, ReorderingBuffer)
        protected override void Normalize(string src, ref ValueReorderingBuffer buffer)
        {
            if (src is null)
                throw new ArgumentNullException(nameof(src));

            Impl.Compose(src.AsSpan(), onlyContiguous, doCompose: true, ref buffer);
        }

        protected override void Normalize(ReadOnlySpan<char> src, ref ValueReorderingBuffer buffer)
        {
            if (MemoryHelper.AreSame(src, buffer.RawChars))
            {
                throw new ArgumentException($"'{nameof(src)}' cannot be the same memory location as '{nameof(buffer)}'");
            }

            Impl.Compose(src, onlyContiguous, doCompose: true, ref buffer);
        }

        #endregion Normalize(ICharSequence, ReorderingBuffer)

        #region NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer)
        protected override void NormalizeAndAppend(
            string src, bool doNormalize, ref ValueReorderingBuffer buffer)
        {
            if (src is null)
                throw new ArgumentNullException(nameof(src));

            Impl.ComposeAndAppend(src.AsSpan(), doNormalize, onlyContiguous, ref buffer);
        }

        protected override void NormalizeAndAppend(
            ReadOnlySpan<char> src, bool doNormalize, ref ValueReorderingBuffer buffer)
        {
            if (MemoryHelper.AreSame(src, buffer.RawChars))
            {
                throw new ArgumentException($"'{nameof(src)}' cannot be the same memory location as '{nameof(buffer)}'");
            }

            Impl.ComposeAndAppend(src, doNormalize, onlyContiguous, ref buffer);
        }

        #endregion NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer)

        #region IsNormalized(ICharSequence)
        public override bool IsNormalized(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            // 5: small destCapacity for substring normalization
            var buffer = new ValueReorderingBuffer(Impl, stackalloc char[5]);
            try
            {
                return Impl.Compose(s.AsSpan(), onlyContiguous, doCompose: false, ref buffer);
            }
            finally
            {
                buffer.Dispose();
            }
        }

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

        #endregion IsNormalized(ICharSequence)

        #region QuickCheck(ICharSequence)
        public override QuickCheckResult QuickCheck(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            int spanLengthAndMaybe = Impl.ComposeQuickCheck(s.AsSpan(), onlyContiguous, false);
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

        #endregion QuickCheck(ICharSequence)

        #region SpanQuickCheckYes(ICharSequence)
        public override int SpanQuickCheckYes(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            return Impl.ComposeQuickCheck(s.AsSpan(), onlyContiguous, true).TripleShift(1); // ICU4N: Checked 3rd parameter
        }

        public override int SpanQuickCheckYes(ReadOnlySpan<char> s)
        {
            return Impl.ComposeQuickCheck(s, onlyContiguous, true).TripleShift(1); // ICU4N: Checked 3rd parameter
        }

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

        #region Normalize(ICharSequence, ReorderingBuffer)
        protected override void Normalize(string src, ref ValueReorderingBuffer buffer)
        {
            if (src is null)
                throw new ArgumentNullException(nameof(src));

            Impl.MakeFCD(src.AsSpan(), ref buffer);
        }

        protected override void Normalize(ReadOnlySpan<char> src, ref ValueReorderingBuffer buffer)
        {
            if (MemoryHelper.AreSame(src, buffer.RawChars))
            {
                throw new ArgumentException($"'{nameof(src)}' cannot be the same memory location as '{nameof(buffer)}'");
            }

            Impl.MakeFCD(src, ref buffer);
        }

        #endregion Normalize(ICharSequence, ReorderingBuffer)

        #region NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer)
        protected override void NormalizeAndAppend(
            string src, bool doNormalize, ref ValueReorderingBuffer buffer)
        {
            if (src is null)
                throw new ArgumentNullException(nameof(src));

            Impl.MakeFCDAndAppend(src.AsSpan(), doNormalize, ref buffer);
        }

        protected override void NormalizeAndAppend(
            ReadOnlySpan<char> src, bool doNormalize, ref ValueReorderingBuffer buffer)
        {
            Impl.MakeFCDAndAppend(src, doNormalize, ref buffer);
        }

        #endregion NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer)

        #region SpanQuickCheckYes(ICharSequence)
        public override int SpanQuickCheckYes(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            return Impl.MakeFCDQuickCheck(s.AsSpan()); // ICU4N: Checked 3rd parameter
        }

        public override int SpanQuickCheckYes(ReadOnlySpan<char> s)
        {
            return Impl.MakeFCDQuickCheck(s); // ICU4N: Checked 3rd parameter
        }

        #endregion SpanQuickCheckYes(ICharSequence)


        public override int GetQuickCheck(int c)
        {
            return Impl.IsDecompYes(Impl.GetNorm16(c)) ? 1 : 0;
        }

        public override bool HasBoundaryBefore(int c) => Impl.HasFCDBoundaryBefore(c);

        public override bool HasBoundaryAfter(int c) => Impl.HasFCDBoundaryAfter(c);

        public override bool IsInert(int c) => Impl.IsFCDInert(c);
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
            return singleton.allModes!;
        }
        public static Norm2AllModes NFCInstance
            => GetInstanceFromSingleton(NFCSingleton.Instance);

        public static Norm2AllModes NFKCInstance
            => GetInstanceFromSingleton(NFKCSingleton.Instance);
        public static Norm2AllModes NFKC_CFInstance
            => GetInstanceFromSingleton(NFKC_CFSingleton.Instance);
        // For use in properties APIs.
        public static Normalizer2WithImpl? GetN2WithImpl(int index)
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
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            if (bytes == null)
            {
                Norm2AllModesSingleton? singleton;
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
                    return singleton.allModes!;
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

            internal Norm2AllModes? allModes;
            internal Exception? exception;
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
