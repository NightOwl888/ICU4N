using ICU4N.Impl;
using ICU4N.Lang;
using ICU4N.Util;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// A transliterator that converts all letters (as defined by
    /// <see cref="UCharacter.IsLetter(int)"/>) to lower case, except for those
    /// letters preceded by non-letters.  The latter are converted to title
    /// case using <see cref="UCharacter.ToTitleCase(int)"/>.
    /// </summary>
    /// <author>Alan Liu</author>
    internal class TitlecaseTransliterator : Transliterator
    {
        internal static readonly string _ID = "Any-Title";
        // TODO: Add variants for tr/az, lt, default = default locale: ICU ticket #12720

        /// <summary>
        /// System registration hook.
        /// </summary>
        internal static void Register()
        {
            Transliterator.RegisterFactory(_ID, new Transliterator.Factory(getInstance: (id) =>
            {
                return new TitlecaseTransliterator(ULocale.US);
            }));

            RegisterSpecialInverse("Title", "Lower", false);
        }

        private readonly ULocale locale;

        private readonly UCaseProps csp;
        private ReplaceableContextIterator iter;
        private StringBuilder result;
        private int caseLocale;

        /// <summary>
        /// Constructs a transliterator.
        /// </summary>
        public TitlecaseTransliterator(ULocale loc)
            : base(_ID, null)
        {
            locale = loc;
            // Need to look back 2 characters in the case of "can't"
            MaximumContextLength = 2;
            csp = UCaseProps.Instance;
            iter = new ReplaceableContextIterator();
            result = new StringBuilder();
            caseLocale = UCaseProps.GetCaseLocale(locale);
        }

        /// <summary>
        /// Implements <see cref="Transliterator.HandleTransliterate(IReplaceable, TransliterationPosition, bool)"/>.
        /// </summary>
        protected override void HandleTransliterate(IReplaceable text,
                                           TransliterationPosition offsets, bool isIncremental)
        {
            lock (this)
            {
                // TODO reimplement, see ustrcase.c
                // using a real word break iterator
                //   instead of just looking for a transition between cased and uncased characters
                // call CaseMapTransliterator::handleTransliterate() for lowercasing? (set fMap)
                // needs to take isIncremental into account because case mappings are context-sensitive
                //   also detect when lowercasing function did not finish because of context

                if (offsets.Start >= offsets.Limit)
                {
                    return;
                }

                // case type: >0 cased (UCaseProps.LOWER etc.)  ==0 uncased  <0 case-ignorable
                int type;

                // Our mode; we are either converting letter toTitle or
                // toLower.
                bool doTitle = true;

                // Determine if there is a preceding context of cased case-ignorable*,
                // in which case we want to start in toLower mode.  If the
                // prior context is anything else (including empty) then start
                // in toTitle mode.
                int c, start;
                for (start = offsets.Start - 1; start >= offsets.ContextStart; start -= UTF16.GetCharCount(c))
                {
                    c = text.Char32At(start);
                    type = csp.GetTypeOrIgnorable(c);
                    if (type > 0)
                    { // cased
                        doTitle = false;
                        break;
                    }
                    else if (type == 0)
                    { // uncased but not ignorable
                        break;
                    }
                    // else (type<0) case-ignorable: continue
                }

                // Convert things after a cased character toLower; things
                // after a uncased, non-case-ignorable character toTitle.  Case-ignorable
                // characters are copied directly and do not change the mode.

                iter.SetText(text);
                iter.SetIndex(offsets.Start);
                iter.SetLimit(offsets.Limit);
                iter.SetContextLimits(offsets.ContextStart, offsets.ContextLimit);

                result.Length = 0;

                // Walk through original string
                // If there is a case change, modify corresponding position in replaceable
                int delta;

                while ((c = iter.NextCaseMapCP()) >= 0)
                {
                    type = csp.GetTypeOrIgnorable(c);
                    if (type >= 0)
                    { // not case-ignorable
                        if (doTitle)
                        {
                            c = csp.ToFullTitle(c, iter, result, caseLocale);
                        }
                        else
                        {
                            c = csp.ToFullLower(c, iter, result, caseLocale);
                        }
                        doTitle = type == 0; // doTitle=isUncased

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
                        else if (c <= UCaseProps.MAX_STRING_LENGTH)
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
                }
                offsets.Start = offsets.Limit;
            }
        }

        // NOTE: normally this would be static, but because the results vary by locale....
        SourceTargetUtility sourceTargetUtility = null;

        /// <seealso cref="Transliterator.AddSourceTargetSet(UnicodeSet, UnicodeSet, UnicodeSet)"/>
        public override void AddSourceTargetSet(UnicodeSet inputFilter, UnicodeSet sourceSet, UnicodeSet targetSet)
        {
            lock (this)
            {
                if (sourceTargetUtility == null)
                {
                    sourceTargetUtility = new SourceTargetUtility(new StringTransform(transform: (source) =>
                    {
                        return UCharacter.ToTitleCase(locale, source, null);
                    }));
                }
            }
            sourceTargetUtility.AddSourceTargetSet(this, inputFilter, sourceSet, targetSet);
        }
    }
}
