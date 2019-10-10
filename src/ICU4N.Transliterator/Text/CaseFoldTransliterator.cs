using ICU4N.Impl;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// A transliterator that performs locale-sensitive ToLower()
    /// case mapping.
    /// </summary>
    internal class CaseFoldTransliterator : Transliterator
    {
        /// <summary>
        /// Package accessible ID.
        /// </summary>
        internal const string _ID = "Any-CaseFold";

        // TODO: Add variants for tr, az, lt, default = default locale

        /// <summary>
        /// System registration hook.
        /// </summary>
        internal static void Register()
        {
            Transliterator.RegisterFactory(_ID, new Transliterator.Factory(getInstance: (id) =>
            {
                return new CaseFoldTransliterator();
            }));

            Transliterator.RegisterSpecialInverse("CaseFold", "Upper", false);
        }

        private readonly UCaseProps csp;
        private ReplaceableContextIterator iter;
        private StringBuilder result;

        /// <summary>
        /// Constructs a transliterator.
        /// </summary>
        public CaseFoldTransliterator()
                : base(_ID, null)
        {
            csp = UCaseProps.Instance;
            iter = new ReplaceableContextIterator();
            result = new StringBuilder();
        }

        /// <summary>
        /// Implements <see cref="Transliterator.HandleTransliterate(IReplaceable, TransliterationPosition, bool)"/>
        /// </summary>
        protected override void HandleTransliterate(IReplaceable text,
                                           TransliterationPosition offsets, bool isIncremental)
        {
            lock (this)
            {
                if (csp == null)
                {
                    return;
                }

                if (offsets.Start >= offsets.Limit)
                {
                    return;
                }

                iter.SetText(text);
                result.Length = 0;
                int c, delta;

                // Walk through original string
                // If there is a case change, modify corresponding position in replaceable

                iter.SetIndex(offsets.Start);
                iter.SetLimit(offsets.Limit);
                iter.SetContextLimits(offsets.ContextStart, offsets.ContextLimit);
                while ((c = iter.NextCaseMapCP()) >= 0)
                {
                    c = csp.ToFullFolding(c, result, 0); // toFullFolding(int c, StringBuffer out, int options)

                    if (iter.DidReachLimit && isIncremental)
                    {
                        // the case mapping function tried to look beyond the context limit
                        // wait for more input
                        offsets.Start = iter.CaseMapCPStart;
                        return;
                    }

                    /* decode the result */
                    if (c < 0)
                    {
                        /* c mapped to itself, no change */
                        continue;
                    }
                    else if (c <= UCaseProps.MaxStringLength)
                    {
                        /* replace by the mapping string */
                        delta = iter.Replace(result.ToString());
                        result.Length = 0;
                    }
                    else
                    {
                        /* replace by single-code point mapping */
                        delta = iter.Replace(UTF16.ValueOf(c));
                    }

                    if (delta != 0)
                    {
                        offsets.Limit += delta;
                        offsets.ContextLimit += delta;
                    }
                }
                offsets.Start = offsets.Limit;
            }
        }

        internal static SourceTargetUtility sourceTargetUtility = null;

        /// <seealso cref="Transliterator.AddSourceTargetSet(UnicodeSet, UnicodeSet, UnicodeSet)"/>
#pragma warning disable 672
        public override void AddSourceTargetSet(UnicodeSet inputFilter, UnicodeSet sourceSet, UnicodeSet targetSet)
#pragma warning restore 672
        {
            lock (typeof(UppercaseTransliterator))
            {
                if (sourceTargetUtility == null)
                {
                    sourceTargetUtility = new SourceTargetUtility(new StringTransform(transform: (source) =>
                    {
                        return UChar.FoldCase(source, true);
                    }));
                }
            }
            sourceTargetUtility.AddSourceTargetSet(this, inputFilter, sourceSet, targetSet);
        }
    }
}
