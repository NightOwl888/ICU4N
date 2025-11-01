using ICU4N.Impl;
using ICU4N.Support.Text;
using J2N;
using J2N.Text;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace ICU4N.Text
{
    /// <author>Alan Liu, Markus Scherer</author>
    internal sealed class NormalizationTransliterator : Transliterator
    {
        private readonly Normalizer2 norm2;

        /// <summary>
        /// System registration hook.
        /// </summary>
        internal static void Register()
        {
            NfcTransliteratorFactory.Register();
            NfdTransliteratorFactory.Register();
            NfkcTransliteratorFactory.Register();
            NfkdTransliteratorFactory.Register();
            FcdTransliteratorFactory.Register();
            FccTransliteratorFactory.Register();

            Transliterator.RegisterSpecialInverse("NFC", "NFD", true);
            Transliterator.RegisterSpecialInverse("NFKC", "NFKD", true);
            Transliterator.RegisterSpecialInverse("FCC", "NFD", false);
            Transliterator.RegisterSpecialInverse("FCD", "FCD", false);
        }

        #region Factories
        // ICU4N: These were anonymous classes in Java. The closest equivalent in .NET
        // (a generic Factory class that accepts a Func<T>) performs poorly.
        // So, we use explicitly delcared classes instead. Moving the Register()
        // part into the class so it is all in context.

        private sealed class NfcTransliteratorFactory : ITransliteratorFactory
        {
            public static void Register()
                => RegisterFactory("Any-NFC", new NfcTransliteratorFactory());

            public Transliterator GetInstance(string id)
                => new NormalizationTransliterator("NFC", Normalizer2.NFCInstance);
        }

        private sealed class NfdTransliteratorFactory : ITransliteratorFactory
        {
            public static void Register()
                => RegisterFactory("Any-NFD", new NfdTransliteratorFactory());

            public Transliterator GetInstance(string id)
                => new NormalizationTransliterator("NFD", Normalizer2.NFDInstance);
        }

        private sealed class NfkcTransliteratorFactory : ITransliteratorFactory
        {
            public static void Register()
                => RegisterFactory("Any-NFKC", new NfkcTransliteratorFactory());

            public Transliterator GetInstance(string id)
                => new NormalizationTransliterator("NFKC", Normalizer2.NFKCInstance);
        }

        private sealed class NfkdTransliteratorFactory : ITransliteratorFactory
        {
            public static void Register()
                => RegisterFactory("Any-NFKD", new NfkdTransliteratorFactory());

            public Transliterator GetInstance(string id)
                => new NormalizationTransliterator("NFKD", Normalizer2.NFKDInstance);
        }

        private sealed class FcdTransliteratorFactory : ITransliteratorFactory
        {
            public static void Register()
                => RegisterFactory("Any-FCD", new FcdTransliteratorFactory());

            public Transliterator GetInstance(string id)
                => new NormalizationTransliterator("FCD", Norm2AllModes.FCDNormalizer2);
        }

        private sealed class FccTransliteratorFactory : ITransliteratorFactory
        {
            public static void Register()
                => RegisterFactory("Any-FCC", new FccTransliteratorFactory());

            public Transliterator GetInstance(string id)
                => new NormalizationTransliterator("FCC", Norm2AllModes.NFCInstance.Fcc);
        }

        #endregion Factories

        /**
         * Constructs a transliterator.
         */
        private NormalizationTransliterator(string id, Normalizer2 n2)
                : base(id, null)
        {
            norm2 = n2;
        }

        /// <summary>
        /// Implements <see cref="Transliterator.HandleTransliterate(IReplaceable, TransliterationPosition, bool)"/>.
        /// </summary>
        protected override void HandleTransliterate(IReplaceable text,
                TransliterationPosition offsets, bool isIncremental)
        {
            // start and limit of the input range
            int start = offsets.Start;
            int limit = offsets.Limit;
            if (start >= limit)
            {
                return;
            }

            /*
             * Normalize as short chunks at a time as possible even in
             * bulk mode, so that styled text is minimally disrupted.
             * In incremental mode, a chunk that ends with offsets.limit
             * must not be normalized.
             *
             * If it was known that the input text is not styled, then
             * a bulk mode normalization could be used.
             * (For details, see the comment in the C++ version.)
             */
            ValueStringBuilder normalized = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            ValueStringBuilder segment = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                int c = text.Char32At(start);
                do
                {
                    int prev = start;
                    // Skip at least one character so we make progress.
                    // c holds the character at start.
                    segment.Length = 0;
                    do
                    {
                        segment.AppendCodePoint(c);
                        start += Character.CharCount(c);
                    } while (start < limit && !norm2.HasBoundaryBefore(c = text.Char32At(start)));
                    if (start == limit && isIncremental && !norm2.HasBoundaryAfter(c))
                    {
                        // stop in incremental mode when we reach the input limit
                        // in case there are additional characters that could change the
                        // normalization result
                        start = prev;
                        break;
                    }
                    norm2.Normalize(segment.AsSpan(), ref normalized);
                    if (!UTF16Plus.Equal(segment.AsSpan(), normalized.AsSpan()))
                    {
                        // replace the input chunk with its normalized form
                        text.Replace(prev, start - prev, normalized.AsSpan()); // ICU4N: Corrected 2nd parameter

                        // update all necessary indexes accordingly
                        int delta = normalized.Length - (start - prev);
                        start += delta;
                        limit += delta;
                    }
                } while (start < limit);
            }
            finally
            {
                segment.Dispose();
                normalized.Dispose();
            }

            offsets.Start = start;
            offsets.ContextLimit += limit - offsets.Limit;
            offsets.Limit = limit;
        }

        internal static readonly ConcurrentDictionary<Normalizer2, Lazy<SourceTargetUtility>> SOURCE_CACHE = new ConcurrentDictionary<Normalizer2, Lazy<SourceTargetUtility>>();

        // TODO Get rid of this if Normalizer2 becomes a Transform
        internal class NormalizingTransform : ITransform<string, string>
        {
            internal readonly Normalizer2 norm2;
            public NormalizingTransform(Normalizer2 norm2)
            {
                this.norm2 = norm2;
            }

            public virtual string Transform(string source)
            {
                return norm2.Normalize(source);
            }
        }

        /// <seealso cref="Transliterator.AddSourceTargetSet(UnicodeSet, UnicodeSet, UnicodeSet)"/>
#pragma warning disable 672
        public override void AddSourceTargetSet(UnicodeSet inputFilter, UnicodeSet sourceSet, UnicodeSet targetSet)
#pragma warning restore 672
        {
            SourceTargetUtility cache = SOURCE_CACHE.GetOrAdd(norm2, (norm2) =>
                new Lazy<SourceTargetUtility>(() => new SourceTargetUtility(new NormalizingTransform(norm2), norm2))).Value;
            cache.AddSourceTargetSet(this, inputFilter, sourceSet, targetSet);
        }
    }
}
