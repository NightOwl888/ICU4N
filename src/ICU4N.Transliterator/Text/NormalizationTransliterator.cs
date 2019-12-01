using ICU4N.Impl;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using J2N;
using System.Collections.Generic;
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
            Transliterator.RegisterFactory("Any-NFC", new Transliterator.Factory(getInstance: (id) =>
            {
                return new NormalizationTransliterator("NFC", Normalizer2.GetNFCInstance());
            }));
            Transliterator.RegisterFactory("Any-NFD", new Transliterator.Factory(getInstance: (id) =>
            {
                return new NormalizationTransliterator("NFD", Normalizer2.GetNFDInstance());
            }));
            Transliterator.RegisterFactory("Any-NFKC", new Transliterator.Factory(getInstance: (id) =>
            {
                return new NormalizationTransliterator("NFKC", Normalizer2.GetNFKCInstance());
            }));
            Transliterator.RegisterFactory("Any-NFKD", new Transliterator.Factory(getInstance: (id) =>
            {
                return new NormalizationTransliterator("NFKD", Normalizer2.GetNFKDInstance());
            }));
            Transliterator.RegisterFactory("Any-FCD", new Transliterator.Factory(getInstance: (id) =>
            {
                return new NormalizationTransliterator("FCD", Norm2AllModes.GetFCDNormalizer2());
            }));
            Transliterator.RegisterFactory("Any-FCC", new Transliterator.Factory(getInstance: (id) =>
            {
                return new NormalizationTransliterator("FCC", Norm2AllModes.GetNFCInstance().Fcc);
            }));
            Transliterator.RegisterSpecialInverse("NFC", "NFD", true);
            Transliterator.RegisterSpecialInverse("NFKC", "NFKD", true);
            Transliterator.RegisterSpecialInverse("FCC", "NFD", false);
            Transliterator.RegisterSpecialInverse("FCD", "FCD", false);
        }

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
            StringBuilder segment = new StringBuilder();
            StringBuilder normalized = new StringBuilder();
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
                norm2.Normalize(segment, normalized);
                if (!UTF16Plus.Equal(segment, normalized))
                {
                    // replace the input chunk with its normalized form
                    text.Replace(prev, start, normalized.ToString());

                    // update all necessary indexes accordingly
                    int delta = normalized.Length - (start - prev);
                    start += delta;
                    limit += delta;
                }
            } while (start < limit);

            offsets.Start = start;
            offsets.ContextLimit += limit - offsets.Limit;
            offsets.Limit = limit;
        }

        internal static readonly IDictionary<Normalizer2, SourceTargetUtility> SOURCE_CACHE = new Dictionary<Normalizer2, SourceTargetUtility>();

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
            SourceTargetUtility cache;
            lock (SOURCE_CACHE)
            {
                //String id = getID();
                cache = SOURCE_CACHE.Get(norm2);
                if (cache == null)
                {
                    cache = new SourceTargetUtility(new NormalizingTransform(norm2), norm2);
                    SOURCE_CACHE[norm2] = cache;
                }
            }
            cache.AddSourceTargetSet(this, inputFilter, sourceSet, targetSet);
        }
    }
}
