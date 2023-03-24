using ICU4N.Text;
using System;
using System.Threading;
#nullable enable

namespace ICU4N.Globalization
{
    public sealed partial class UNumberFormatInfo
    {
        private Capitalization capitalization = Capitalization.None;

        /// <summary>
        /// Gets or sets the capitalization display context for number formatting,
        /// such as <see cref="DisplayContext.CapitalizationForStandalone"/>.
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

        public NumberFormatRules SpellOut
            => LazyInitializer.EnsureInitialized(ref spellOut, () => NumberFormatRules.GetInstance(CultureData.name, NumberPresentation.SpellOut));

        public NumberFormatRules Ordinal
            => LazyInitializer.EnsureInitialized(ref ordinal, () => NumberFormatRules.GetInstance(CultureData.name, NumberPresentation.Ordinal));

        // ICU4N TODO: Do we need this? We have TimeSpan.ToString() which seems to cover. Need research.
        internal NumberFormatRules Duration
            => LazyInitializer.EnsureInitialized(ref duration, () => NumberFormatRules.GetInstance(CultureData.name, NumberPresentation.Duration));

        public NumberFormatRules NumberingSystem
            => LazyInitializer.EnsureInitialized(ref numberingSystem, () => NumberFormatRules.GetInstance(CultureData.name, NumberPresentation.NumberingSystem));
#endif

    }
}
