using ICU4N.Text;
using J2N.IO;
using System;

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
    }

    // Intermediate class:
    // Has Normalizer2Impl and does boilerplate argument checking and setup.
    public abstract partial class Normalizer2WithImpl : Normalizer2
    {
        public Normalizer2WithImpl(Normalizer2Impl ni)
        {
            Impl = ni;
        }

        // normalize

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
