using ICU4N.Support;
using ICU4N.Support.IO;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.IO;
using System.Text;

namespace ICU4N.Impl
{
    // Normalizer2 implementation for the old UNORM_NONE.
    public sealed class NoopNormalizer2 : Normalizer2
    {
        public override StringBuilder Normalize(string src, StringBuilder dest) // ICU4N TODO: API code generation for ICharSequence overloads
        {
            dest.Length = 0;
            return dest.Append(src);
        }

        public override StringBuilder Normalize(StringBuilder src, StringBuilder dest)
        {
            if (dest != src)
            {
                dest.Length = 0;
                return dest.Append(src);
            }
            else
            {
                throw new ArgumentException("'src' cannot be the same instance as 'dest'");
            }
        }

        public override StringBuilder Normalize(char[] src, StringBuilder dest)
        {
            dest.Length = 0;
            return dest.Append(src);
        }

        internal override StringBuilder Normalize(ICharSequence src, StringBuilder dest)
        {
            dest.Length = 0;
            return dest.Append(src.ToString());
        }

        internal override IAppendable Normalize(ICharSequence src, IAppendable dest)
        {
            if (dest != src)
            {
                try
                {
                    return dest.Append(src.ToString());
                }
                catch (IOException e)
                {
                    throw new ICUUncheckedIOException(e);  // Avoid declaring "throws IOException".
                }
            }
            else
            {
                throw new ArgumentException("'src' cannot be the same instance as 'dest'");
            }
        }

        public override StringBuilder NormalizeSecondAndAppend(StringBuilder first, string second)
        {
            return first.Append(second);
        }

        public override StringBuilder NormalizeSecondAndAppend(StringBuilder first, StringBuilder second)
        {
            if (first != second)
            {
                return first.Append(second.ToString());
            }
            else
            {
                throw new ArgumentException("'first' cannot be the same instance as 'second'");
            }
        }

        public override StringBuilder NormalizeSecondAndAppend(StringBuilder first, char[] second)
        {
            return first.Append(second);
        }

        internal override StringBuilder NormalizeSecondAndAppend(StringBuilder first, ICharSequence second)
        {
            if (second is StringBuilderCharSequence && ((StringBuilderCharSequence)second).StringBuilder == first)
            {
                throw new ArgumentException("'first' cannot be the same instance as 'second'");
            }
            return first.Append(second.ToString());
        }

        public override StringBuilder Append(StringBuilder first, string second)
        {
            return first.Append(second);
        }

        public override StringBuilder Append(StringBuilder first, StringBuilder second)
        {
            if (first != second)
            {
                return first.Append(second.ToString());
            }
            else
            {
                throw new ArgumentException("'first' cannot be the same instance as 'second'");
            }
        }

        public override StringBuilder Append(StringBuilder first, char[] second)
        {
            return first.Append(second);
        }

        internal override StringBuilder Append(StringBuilder first, ICharSequence second)
        {
            if (second is StringBuilderCharSequence && ((StringBuilderCharSequence)second).StringBuilder == first)
            {
                throw new ArgumentException("'first' cannot be the same instance as 'second'");
            }
            return first.Append(second.ToString());
        }

        public override string GetDecomposition(int c)
        {
            return null;
        }
        // No need to override the default GetRawDecomposition().

        public override bool IsNormalized(string s) { return true; }

        public override bool IsNormalized(StringBuilder s) { return true; }

        public override bool IsNormalized(char[] s) { return true; }

        internal override bool IsNormalized(ICharSequence s) { return true; }

        public override QuickCheckResult QuickCheck(string s) { return QuickCheckResult.Yes; }

        public override QuickCheckResult QuickCheck(StringBuilder s) { return QuickCheckResult.Yes; }

        public override QuickCheckResult QuickCheck(char[] s) { return QuickCheckResult.Yes; }

        internal override QuickCheckResult QuickCheck(ICharSequence s) { return QuickCheckResult.Yes; }

        public override int SpanQuickCheckYes(string s) { return s.Length; }

        public override int SpanQuickCheckYes(StringBuilder s) { return s.Length; }

        public override int SpanQuickCheckYes(char[] s) { return s.Length; }

        internal override int SpanQuickCheckYes(ICharSequence s) { return s.Length; }

        public override bool HasBoundaryBefore(int c) { return true; }

        public override bool HasBoundaryAfter(int c) { return true; }

        public override bool IsInert(int c) { return true; }
    }

    // Intermediate class:
    // Has Normalizer2Impl and does boilerplate argument checking and setup.
    public abstract class Normalizer2WithImpl : Normalizer2
    {
        public Normalizer2WithImpl(Normalizer2Impl ni)
        {
            impl = ni;
        }

        // normalize

        public override StringBuilder Normalize(string src, StringBuilder dest)
        {
            dest.Length = 0;
            Normalize(src, new ReorderingBuffer(impl, dest, src.Length));
            return dest;
        }

        public override StringBuilder Normalize(StringBuilder src, StringBuilder dest)
        {
            if (dest == src)
            {
                throw new ArgumentException("'src' cannot be the same StringBuilder instance as 'dest'");
            }
            dest.Length = 0;
            Normalize(src, new ReorderingBuffer(impl, dest, src.Length));
            return dest;
        }

        public override StringBuilder Normalize(char[] src, StringBuilder dest)
        {
            dest.Length = 0;
            Normalize(src, new ReorderingBuffer(impl, dest, src.Length));
            return dest;
        }

        internal override StringBuilder Normalize(ICharSequence src, StringBuilder dest)
        {
            if (src is StringBuilderCharSequence && ((StringBuilderCharSequence)src).StringBuilder == dest)
            {
                throw new ArgumentException("'src' cannot be the same StringBuilder instance as 'dest'");
            }
            dest.Length = 0;
            Normalize(src, new ReorderingBuffer(impl, dest, src.Length));
            return dest;
        }

        internal override IAppendable Normalize(ICharSequence src, IAppendable dest)
        {
            if (dest == src)
            {
                throw new ArgumentException("'src' cannot be the same instance as 'dest'");
            }
            if (src is StringBuilderCharSequence && dest is StringBuilderAppendable &&
                ((StringBuilderCharSequence)src).StringBuilder == ((StringBuilderAppendable)dest).StringBuilder)
            {
                throw new ArgumentException("'src' cannot be the same StringBuilder instance as 'dest'");
            }
            ReorderingBuffer buffer = new ReorderingBuffer(impl, dest, src.Length);
            Normalize(src, buffer);
            buffer.Flush();
            return dest;
        }

        protected abstract void Normalize(string src, ReorderingBuffer buffer);

        protected abstract void Normalize(StringBuilder src, ReorderingBuffer buffer);

        protected abstract void Normalize(char[] src, ReorderingBuffer buffer);

        internal abstract void Normalize(ICharSequence src, ReorderingBuffer buffer);

        // normalize and append

        public override StringBuilder NormalizeSecondAndAppend(StringBuilder first, string second)
        {
            return NormalizeSecondAndAppend(first, second, true);
        }

        public override StringBuilder NormalizeSecondAndAppend(StringBuilder first, StringBuilder second)
        {
            return NormalizeSecondAndAppend(first, second, true);
        }

        public override StringBuilder NormalizeSecondAndAppend(StringBuilder first, char[] second)
        {
            return NormalizeSecondAndAppend(first, second, true);
        }

        internal override StringBuilder NormalizeSecondAndAppend(StringBuilder first, ICharSequence second)
        {
            return NormalizeSecondAndAppend(first, second, true);
        }

        public override StringBuilder Append(StringBuilder first, string second)
        {
            return NormalizeSecondAndAppend(first, second, false);
        }

        public override StringBuilder Append(StringBuilder first, StringBuilder second)
        {
            return NormalizeSecondAndAppend(first, second, false);
        }

        public override StringBuilder Append(StringBuilder first, char[] second)
        {
            return NormalizeSecondAndAppend(first, second, false);
        }

        internal override StringBuilder Append(StringBuilder first, ICharSequence second)
        {
            return NormalizeSecondAndAppend(first, second, false);
        }

        public virtual StringBuilder NormalizeSecondAndAppend(
            StringBuilder first, string second, bool doNormalize)
        {
            NormalizeAndAppend(
                second, doNormalize,
                new ReorderingBuffer(impl, first, first.Length + second.Length));
            return first;
        }

        public virtual StringBuilder NormalizeSecondAndAppend(
            StringBuilder first, StringBuilder second, bool doNormalize)
        {
            if (first == second)
            {
                throw new ArgumentException("'first' cannot be the same instance as 'second'");
            }
            NormalizeAndAppend(
                second, doNormalize,
                new ReorderingBuffer(impl, first, first.Length + second.Length));
            return first;
        }

        public virtual StringBuilder NormalizeSecondAndAppend(
            StringBuilder first, char[] second, bool doNormalize)
        {
            NormalizeAndAppend(
                second, doNormalize,
                new ReorderingBuffer(impl, first, first.Length + second.Length));
            return first;
        }

        internal virtual StringBuilder NormalizeSecondAndAppend(
            StringBuilder first, ICharSequence second, bool doNormalize)
        {
            NormalizeAndAppend(
                second, doNormalize,
                new ReorderingBuffer(impl, first, first.Length + second.Length));
            return first;
        }

        protected abstract void NormalizeAndAppend(
            string src, bool doNormalize, ReorderingBuffer buffer);

        protected abstract void NormalizeAndAppend(
            StringBuilder src, bool doNormalize, ReorderingBuffer buffer);

        protected abstract void NormalizeAndAppend(
            char[] src, bool doNormalize, ReorderingBuffer buffer);

        internal abstract void NormalizeAndAppend(
            ICharSequence src, bool doNormalize, ReorderingBuffer buffer);

        public override string GetDecomposition(int c)
        {
            return impl.GetDecomposition(c);
        }

        public override string GetRawDecomposition(int c)
        {
            return impl.GetRawDecomposition(c);
        }

        public override int ComposePair(int a, int b)
        {
            return impl.ComposePair(a, b);
        }


        public override int GetCombiningClass(int c)
        {
            return impl.GetCC(impl.GetNorm16(c));
        }

        // quick checks

        public override bool IsNormalized(string s)
        {
            return s.Length == SpanQuickCheckYes(s);
        }

        public override bool IsNormalized(StringBuilder s)
        {
            return s.Length == SpanQuickCheckYes(s);
        }

        public override bool IsNormalized(char[] s)
        {
            return s.Length == SpanQuickCheckYes(s);
        }

        internal override bool IsNormalized(ICharSequence s)
        {
            return s.Length == SpanQuickCheckYes(s);
        }

        public override QuickCheckResult QuickCheck(string s)
        {
            return IsNormalized(s) ? QuickCheckResult.Yes : QuickCheckResult.No;
        }

        public override QuickCheckResult QuickCheck(StringBuilder s)
        {
            return IsNormalized(s) ? QuickCheckResult.Yes : QuickCheckResult.No;
        }

        public override QuickCheckResult QuickCheck(char[] s)
        {
            return IsNormalized(s) ? QuickCheckResult.Yes : QuickCheckResult.No;
        }

        internal override QuickCheckResult QuickCheck(ICharSequence s)
        {
            return IsNormalized(s) ? QuickCheckResult.Yes : QuickCheckResult.No;
        }

        public abstract int GetQuickCheck(int c);

        public readonly Normalizer2Impl impl;
    }

    public sealed class DecomposeNormalizer2 : Normalizer2WithImpl
    {
        public DecomposeNormalizer2(Normalizer2Impl ni)
            : base(ni)
        {
        }

        protected override void Normalize(string src, ReorderingBuffer buffer)
        {
            impl.Decompose(src, 0, src.Length, buffer);
        }

        protected override void Normalize(StringBuilder src, ReorderingBuffer buffer)
        {
            impl.Decompose(src, 0, src.Length, buffer);
        }

        protected override void Normalize(char[] src, ReorderingBuffer buffer)
        {
            impl.Decompose(src, 0, src.Length, buffer);
        }

        internal override void Normalize(ICharSequence src, ReorderingBuffer buffer)
        {
            impl.Decompose(src, 0, src.Length, buffer);
        }

        protected override void NormalizeAndAppend(
            string src, bool doNormalize, ReorderingBuffer buffer)
        {
            impl.DecomposeAndAppend(src, doNormalize, buffer);
        }

        protected override void NormalizeAndAppend(
            StringBuilder src, bool doNormalize, ReorderingBuffer buffer)
        {
            impl.DecomposeAndAppend(src, doNormalize, buffer);
        }

        protected override void NormalizeAndAppend(
            char[] src, bool doNormalize, ReorderingBuffer buffer)
        {
            impl.DecomposeAndAppend(src, doNormalize, buffer);
        }

        internal override void NormalizeAndAppend(ICharSequence src, bool doNormalize, ReorderingBuffer buffer)
        {
            impl.DecomposeAndAppend(src, doNormalize, buffer);
        }

        public override int SpanQuickCheckYes(string s)
        {
            return impl.Decompose(s, 0, s.Length, null);
        }

        public override int SpanQuickCheckYes(StringBuilder s)
        {
            return impl.Decompose(s, 0, s.Length, null);
        }

        public override int SpanQuickCheckYes(char[] s)
        {
            return impl.Decompose(s, 0, s.Length, null);
        }

        internal override int SpanQuickCheckYes(ICharSequence s)
        {
            return impl.Decompose(s, 0, s.Length, null);
        }

        public override int GetQuickCheck(int c)
        {
            return impl.IsDecompYes(impl.GetNorm16(c)) ? 1 : 0;
        }

        public override bool HasBoundaryBefore(int c) { return impl.HasDecompBoundaryBefore(c); }

        public override bool HasBoundaryAfter(int c) { return impl.HasDecompBoundaryAfter(c); }

        public override bool IsInert(int c) { return impl.IsDecompInert(c); }
    }

    public sealed class ComposeNormalizer2 : Normalizer2WithImpl
    {
        public ComposeNormalizer2(Normalizer2Impl ni, bool fcc)
            : base(ni)
        {
            onlyContiguous = fcc;
        }

        protected override void Normalize(string src, ReorderingBuffer buffer)
        {
            impl.Compose(src, 0, src.Length, onlyContiguous, true, buffer);
        }

        protected override void Normalize(StringBuilder src, ReorderingBuffer buffer)
        {
            impl.Compose(src, 0, src.Length, onlyContiguous, true, buffer);
        }

        protected override void Normalize(char[] src, ReorderingBuffer buffer)
        {
            impl.Compose(src, 0, src.Length, onlyContiguous, true, buffer);
        }

        internal override void Normalize(ICharSequence src, ReorderingBuffer buffer)
        {
            impl.Compose(src, 0, src.Length, onlyContiguous, true, buffer);
        }

        protected override void NormalizeAndAppend(
            string src, bool doNormalize, ReorderingBuffer buffer)
        {
            impl.ComposeAndAppend(src, doNormalize, onlyContiguous, buffer);
        }

        protected override void NormalizeAndAppend(
            StringBuilder src, bool doNormalize, ReorderingBuffer buffer)
        {
            impl.ComposeAndAppend(src, doNormalize, onlyContiguous, buffer);
        }

        protected override void NormalizeAndAppend(
            char[] src, bool doNormalize, ReorderingBuffer buffer)
        {
            impl.ComposeAndAppend(src, doNormalize, onlyContiguous, buffer);
        }

        internal override void NormalizeAndAppend(
            ICharSequence src, bool doNormalize, ReorderingBuffer buffer)
        {
            impl.ComposeAndAppend(src, doNormalize, onlyContiguous, buffer);
        }

        public override bool IsNormalized(string s)
        {
            // 5: small destCapacity for substring normalization
            return impl.Compose(s, 0, s.Length,
                                onlyContiguous, false,
                                new ReorderingBuffer(impl, new StringBuilder(), 5));
        }

        public override bool IsNormalized(StringBuilder s)
        {
            // 5: small destCapacity for substring normalization
            return impl.Compose(s, 0, s.Length,
                                onlyContiguous, false,
                                new ReorderingBuffer(impl, new StringBuilder(), 5));
        }

        public override bool IsNormalized(char[] s)
        {
            // 5: small destCapacity for substring normalization
            return impl.Compose(s, 0, s.Length,
                                onlyContiguous, false,
                                new ReorderingBuffer(impl, new StringBuilder(), 5));
        }

        internal override bool IsNormalized(ICharSequence s)
        {
            // 5: small destCapacity for substring normalization
            return impl.Compose(s, 0, s.Length,
                                onlyContiguous, false,
                                new ReorderingBuffer(impl, new StringBuilder(), 5));
        }

        public override QuickCheckResult QuickCheck(string s)
        {
            int spanLengthAndMaybe = impl.ComposeQuickCheck(s, 0, s.Length, onlyContiguous, false);
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
            int spanLengthAndMaybe = impl.ComposeQuickCheck(s, 0, s.Length, onlyContiguous, false);
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

        public override QuickCheckResult QuickCheck(char[] s)
        {
            int spanLengthAndMaybe = impl.ComposeQuickCheck(s, 0, s.Length, onlyContiguous, false);
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

        internal override QuickCheckResult QuickCheck(ICharSequence s)
        {
            int spanLengthAndMaybe = impl.ComposeQuickCheck(s, 0, s.Length, onlyContiguous, false);
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

        public override int SpanQuickCheckYes(string s)
        {
            return impl.ComposeQuickCheck(s, 0, s.Length, onlyContiguous, true).TripleShift(1);
        }

        public override int SpanQuickCheckYes(StringBuilder s)
        {
            return impl.ComposeQuickCheck(s, 0, s.Length, onlyContiguous, true).TripleShift(1);
        }

        public override int SpanQuickCheckYes(char[] s)
        {
            return impl.ComposeQuickCheck(s, 0, s.Length, onlyContiguous, true).TripleShift(1);
        }

        internal override int SpanQuickCheckYes(ICharSequence s)
        {
            return impl.ComposeQuickCheck(s, 0, s.Length, onlyContiguous, true).TripleShift(1);
        }

        public override int GetQuickCheck(int c)
        {
            return impl.GetCompQuickCheck(impl.GetNorm16(c));
        }

        public override bool HasBoundaryBefore(int c) { return impl.HasCompBoundaryBefore(c); }

        public override bool HasBoundaryAfter(int c)
        {
            return impl.HasCompBoundaryAfter(c, onlyContiguous);
        }

        public override bool IsInert(int c)
        {
            return impl.IsCompInert(c, onlyContiguous);
        }

        private readonly bool onlyContiguous;
    }

    public sealed class FCDNormalizer2 : Normalizer2WithImpl
    {
        public FCDNormalizer2(Normalizer2Impl ni)
            : base(ni)
        {
        }

        protected override void Normalize(string src, ReorderingBuffer buffer)
        {
            impl.MakeFCD(src, 0, src.Length, buffer);
        }

        protected override void Normalize(StringBuilder src, ReorderingBuffer buffer)
        {
            impl.MakeFCD(src, 0, src.Length, buffer);
        }

        protected override void Normalize(char[] src, ReorderingBuffer buffer)
        {
            impl.MakeFCD(src, 0, src.Length, buffer);
        }

        internal override void Normalize(ICharSequence src, ReorderingBuffer buffer)
        {
            impl.MakeFCD(src, 0, src.Length, buffer);
        }

        protected override void NormalizeAndAppend(
            string src, bool doNormalize, ReorderingBuffer buffer)
        {
            impl.MakeFCDAndAppend(src, doNormalize, buffer);
        }

        protected override void NormalizeAndAppend(
            StringBuilder src, bool doNormalize, ReorderingBuffer buffer)
        {
            impl.MakeFCDAndAppend(src, doNormalize, buffer);
        }

        protected override void NormalizeAndAppend(
            char[] src, bool doNormalize, ReorderingBuffer buffer)
        {
            impl.MakeFCDAndAppend(src, doNormalize, buffer);
        }

        internal override void NormalizeAndAppend(
            ICharSequence src, bool doNormalize, ReorderingBuffer buffer)
        {
            impl.MakeFCDAndAppend(src, doNormalize, buffer);
        }

        public override int SpanQuickCheckYes(string s)
        {
            return impl.MakeFCD(s, 0, s.Length, null);
        }

        public override int SpanQuickCheckYes(StringBuilder s)
        {
            return impl.MakeFCD(s, 0, s.Length, null);
        }

        public override int SpanQuickCheckYes(char[] s)
        {
            return impl.MakeFCD(s, 0, s.Length, null);
        }

        internal override int SpanQuickCheckYes(ICharSequence s)
        {
            return impl.MakeFCD(s, 0, s.Length, null);
        }

        public override int GetQuickCheck(int c)
        {
            return impl.IsDecompYes(impl.GetNorm16(c)) ? 1 : 0;
        }

        public override bool HasBoundaryBefore(int c) { return impl.HasFCDBoundaryBefore(c); }

        public override bool HasBoundaryAfter(int c) { return impl.HasFCDBoundaryAfter(c); }

        public override bool IsInert(int c) { return impl.IsFCDInert(c); }
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
        public static Norm2AllModes GetInstance(ByteBuffer bytes, string name)
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
            return cache.GetInstance(name, bytes);
        }

        private static CacheBase<string, Norm2AllModes, ByteBuffer> cache = new Norm2SoftCache();

        private class Norm2SoftCache : SoftCache<string, Norm2AllModes, ByteBuffer>
        {
            protected override Norm2AllModes CreateInstance(string key, ByteBuffer bytes)
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
            }
        }

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
            public static Norm2AllModesSingleton Instance
            {
                get { return instance; }
            }
        }
        internal sealed class NFKCSingleton
        {
            private static readonly Norm2AllModesSingleton instance = new Norm2AllModesSingleton("nfkc");
            public static Norm2AllModesSingleton Instance
            {
                get { return instance; }
            }
        }
        internal sealed class NFKC_CFSingleton
        {
            private static readonly Norm2AllModesSingleton instance = new Norm2AllModesSingleton("nfkc_cf");
            public static Norm2AllModesSingleton Instance
            {
                get { return instance; }
            }
        }
    }
}
