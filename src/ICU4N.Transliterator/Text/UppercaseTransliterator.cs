using ICU4N.Impl;
using ICU4N.Globalization;
using ICU4N.Util;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// A transliterator that performs locale-sensitive ToUpper()
    /// case mapping.
    /// </summary>
    internal class UppercaseTransliterator : Transliterator
    {
        /// <summary>
        /// Package accessible ID.
        /// </summary>
        internal const string _ID = "Any-Upper";
        // TODO: Add variants for tr/az, el, lt, default = default locale: ICU ticket #12720

        /// <summary>
        /// System registration hook.
        /// </summary>
        internal static void Register()
        {
            Transliterator.RegisterFactory(_ID, new Transliterator.Factory(getInstance: (id) =>
            {
                return new UppercaseTransliterator(new UCultureInfo("en_US"));
            }));
        }

        private readonly UCultureInfo locale;

        private readonly UCaseProperties csp;
        private ReplaceableContextEnumerator iter;
        private StringBuilder result;
        private CaseLocale caseLocale;

        /// <summary>
        /// Constructs a transliterator.
        /// </summary>
        public UppercaseTransliterator(UCultureInfo loc)
                : base(_ID, null)
        {
            locale = loc;
            csp = UCaseProperties.Instance;
            iter = new ReplaceableContextEnumerator();
            result = new StringBuilder();
            caseLocale = UCaseProperties.GetCaseLocale(locale);
        }

        /// <summary>
        /// Implements <see cref="Transliterator.HandleTransliterate(IReplaceable, TransliterationPosition, bool)"/>.
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
                    c = csp.ToFullUpper(c, iter, result, caseLocale);

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
                    else if (c <= UCaseProperties.MaxStringLength)
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

        // NOTE: normally this would be static, but because the results vary by locale....
        SourceTargetUtility sourceTargetUtility = null;

        /// <seealso cref="Transliterator.AddSourceTargetSet(UnicodeSet, UnicodeSet, UnicodeSet)"/>
#pragma warning disable 672
        public override void AddSourceTargetSet(UnicodeSet inputFilter, UnicodeSet sourceSet, UnicodeSet targetSet)
#pragma warning restore 672
        {
            lock (this)
            {
                if (sourceTargetUtility == null)
                {
                    sourceTargetUtility = new SourceTargetUtility(new StringTransform(transform: (source) =>
                    {
                        return UChar.ToUpper(locale, source);
                    }));
                }
            }
            sourceTargetUtility.AddSourceTargetSet(this, inputFilter, sourceSet, targetSet);
        }
    }
}
