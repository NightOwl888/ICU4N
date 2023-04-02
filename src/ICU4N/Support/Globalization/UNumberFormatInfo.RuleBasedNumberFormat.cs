using ICU4N.Impl;
using System;
using System.ComponentModel;
using System.Threading;
#nullable enable

namespace ICU4N.Globalization
{
    public sealed partial class UNumberFormatInfo
    {
        private Capitalization capitalization = Capitalization.None;
        internal CaseLocale caseLocale; // The CaseLocale value for capitalization

        internal CaseLocale CaseLocale => caseLocale;

        /// <summary>
        /// Gets or sets the capitalization display context for number formatting,
        /// such as <see cref="Capitalization.ForStandalone"/>.
        /// </summary>
        /// <draft>ICU 60.1</draft>
        public Capitalization Capitalization
        {
            get => capitalization;
            set
            {
                if (!capitalization.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(value), string.Format(SR.ArgumentOutOfRange_Enum, value, nameof(ICU4N.Globalization.Capitalization)));

                VerifyWritable();
                capitalization = value;
            }
        }

#if FEATURE_SPAN
        private NumberFormatRules? spellOut;
        private NumberFormatRules? ordinal;
        private NumberFormatRules? duration;
        private NumberFormatRules? numberingSystem;

        /// <summary>
        /// Gets the spellout <see cref="NumberFormatRules"/> instance for the rule-based number formatter
        /// that spells out a value in words.
        /// </summary>
        /// <draft>ICU 60.1</draft>
        public NumberFormatRules SpellOut
            => LazyInitializer.EnsureInitialized(ref spellOut, () => NumberFormatRules.GetInstance(CultureData.name, NumberPresentation.SpellOut));

        /// <summary>
        /// Gets the ordinal <see cref="NumberFormatRules"/> instance for the rule-based number formatter
        /// that attaches an ordinal suffix from the desired language to the end of the number (e.g. "123rd").
        /// </summary>
        /// <draft>ICU 60.1</draft>
        public NumberFormatRules Ordinal
            => LazyInitializer.EnsureInitialized(ref ordinal, () => NumberFormatRules.GetInstance(CultureData.name, NumberPresentation.Ordinal));

        /// <summary>
        /// Gets the duration <see cref="NumberFormatRules"/> instance for the rule-based number formatter
        /// that formats a duration in seconds as hours, minutes, and seconds.
        /// </summary>
        /// <draft>ICU 60.1</draft>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)] // ICU4N TODO: Do we need this? We have TimeSpan.ToString() which seems to cover. Need research.
        public NumberFormatRules Duration
            => LazyInitializer.EnsureInitialized(ref duration, () => NumberFormatRules.GetInstance(CultureData.name, NumberPresentation.Duration));

        /// <summary>
        /// Gets the numbering system <see cref="NumberFormatRules"/> instance for the rule-based number formatter
        /// that formats a number to an algorithmic numbering system, such as <c>%hebrew</c> for Hebrew numbers or <c>%roman-upper</c>
        /// for upper-case Roman numerals.
        /// </summary>
        /// <draft>ICU 60.1</draft>
        public NumberFormatRules NumberingSystem
            => LazyInitializer.EnsureInitialized(ref numberingSystem, () => NumberFormatRules.GetInstance(CultureData.name, NumberPresentation.NumberingSystem));
#endif

    }
}
