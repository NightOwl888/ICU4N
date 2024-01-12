using ICU4N.Support.Text;
using ICU4N.Text;
using J2N.IO;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace ICU4N.Impl
{
    // Normalizer2 implementation for the old UNORM_NONE.
    public sealed partial class NoopNormalizer2 : Normalizer2
    {
        // ICU4N specific: Moved Normalize(ICharSequence, StringBuilder) to Norm2AllModes.generated.tt

        // ICU4N specific: Moved Normalize(ICharSequence, IAppendable) to Norm2AllModes.generated.tt

        // ICU4N specific: Moved NormalizeSecondAndAppend(StringBuilder, ICharSequence) to Norm2AllModes.generated.tt

        // ICU4N specific: Moved Append(StringBuilder, ICharSequence) to Norm2AllModes.generated.tt

        // ICU4N specific: Moved IsNormalized(ICharSequence) to Norm2AllModes.generated.tt

        // ICU4N specific: Moved QuickCheck(ICharSequence) to Norm2AllModes.generated.tt

        // ICU4N specific: Moved SpanQuickCheckYes(ICharSequence) to Norm2AllModes.generated.tt

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

        // ICU4N specific: Moved Normalize(ICharSequence, IAppendable) to Norm2AllModes.generated.tt

        // ICU4N specific: Moved Normalize(ICharSequence, ReorderingBuffer) to Norm2AllModes.generated.tt

        // normalize and append

        // ICU4N specific: Moved NormalizeSecondAndAppend(StringBuilder, ICharSequence) to Norm2AllModes.generated.tt

        // ICU4N specific: Moved Append(StringBuilder, ICharSequence) to Norm2AllModes.generated.tt

        // ICU4N specific: Moved NormalizeSecondAndAppend(StringBuilder, ICharSequence, bool) to Norm2AllModes.generated.tt

        // ICU4N specific: Moved NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer) to Norm2AllModes.generated.tt


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

        // ICU4N specific: Moved QuickCheck(ICharSequence s) to Norm2AllModes.generated.tt


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

        // ICU4N specific: Moved NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer) to Norm2AllModes.generated.tt

        // ICU4N specific: Moved SpanQuickCheckYes(ICharSequence) to Norm2AllModes.generated.tt

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

        // ICU4N specific: Moved NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer) to Norm2AllModes.generated.tt

        // ICU4N specific: Moved IsNormalized(ICharSequence) to Norm2AllModes.generated.tt

        // ICU4N specific: Moved QuickCheck(ICharSequence) to Norm2AllModes.generated.tt

        // ICU4N specific: Moved SpanQuickCheckYes(ICharSequence) to Norm2AllModes.generated.tt

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

        // ICU4N specific: Moved NormalizeAndAppend(ICharSequence, bool, ReorderingBuffer) to Norm2AllModes.generated.tt

        // ICU4N specific: Moved SpanQuickCheckYes(ICharSequence) to Norm2AllModes.generated.tt


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
        public static Norm2AllModes GetNFCInstance()
        {
            return GetInstanceFromSingleton(NFCSingleton.Instance);
        }
        public static Norm2AllModes GetNFKCInstance()
        {
            return GetInstanceFromSingleton(NFKCSingleton.Instance);
        }
        public static Norm2AllModes GetNFKC_CFInstance()
        {
            return GetInstanceFromSingleton(NFKC_CFSingleton.Instance);
        }
        // For use in properties APIs.
        public static Normalizer2WithImpl GetN2WithImpl(int index)
        {
            switch (index)
            {
                case 0: return GetNFCInstance().Decomp;  // NFD
                case 1: return GetNFKCInstance().Decomp; // NFKD
                case 2: return GetNFCInstance().Comp;    // NFC
                case 3: return GetNFKCInstance().Comp;   // NFKC
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
        public static Normalizer2 GetFCDNormalizer2()
        {
            return GetNFCInstance().Fcd;
        }

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
