using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ICU4N.Globalization
{
    public partial class UCultureInfo
    {
        private readonly CultureInfo culture;

        /// <inheritdoc/>
        public override Calendar Calendar => culture.Calendar;

        /// <inheritdoc/>
        public override CompareInfo CompareInfo => culture.CompareInfo;

        /// <inheritdoc/>
        public override DateTimeFormatInfo DateTimeFormat
        {
            get => culture.DateTimeFormat;
            set => culture.DateTimeFormat = value;
        }

        /// <inheritdoc/>
        // ICU4N TODO: use ULocale display name
        public override string DisplayName => culture.DisplayName;

        /// <inheritdoc/>
        // ICU4N TODO: use ULocale display name in english
        public override string EnglishName => culture.EnglishName;

        /// <inheritdoc/>
        public override bool IsNeutralCulture => culture.IsNeutralCulture;


#if FEATURE_CULTUREINFO_KEYBOARDLAYOUTID
        /// <inheritdoc/>
        public override int KeyboardLayoutId => culture.KeyboardLayoutId;
#endif
#if FEATURE_CULTUREINFO_LCID
        /// <inheritdoc/>
        public override int LCID => culture.LCID; // .NET only property
#endif

        /// <summary>
        /// <icu/> Returns the (normalized) base name for this locale,
        /// like <see cref="FullName"/>, but without keywords.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        // ICU4N specific: This was named getBaseName() in ICU4J
        public override string Name => GetName(localeID); // always normalized

        /// <inheritdoc/>
        public override string NativeName => culture.NativeName;

        /// <inheritdoc/>
        public override NumberFormatInfo NumberFormat
        {
            get => culture.NumberFormat;
            set => culture.NumberFormat = value;
        }

        /// <inheritdoc/>
        public override Calendar[] OptionalCalendars => culture.OptionalCalendars;

        /// <inheritdoc/>
        public override CultureInfo Parent => culture.Parent; // ICU4N TODO: How to implement this?

        /// <inheritdoc/>
        public override TextInfo TextInfo => base.TextInfo;

#if FEATURE_CULTUREINFO_THREELETTERISOLANGUAGENAME
        /// <inheritdoc/>
        public override string ThreeLetterISOLanguageName => culture.ThreeLetterISOLanguageName; // ISO 639-2
#endif
#if FEATURE_CULTUREINFO_THREELETTERWINDOWSLANGUAGENAME
        /// <inheritdoc/>
        public override string ThreeLetterWindowsLanguageName => culture.ThreeLetterWindowsLanguageName; // Windows API
#endif

        /// <inheritdoc/>
        public override string TwoLetterISOLanguageName => culture.TwoLetterISOLanguageName; // ISO 639-1


        /// <summary>
        /// This is for compatibility with <see cref="CultureInfo"/> -- in actuality, since <see cref="UCultureInfo"/> is
        /// immutable, there is no reason to clone it, so this API returns 'this'.
        /// </summary>
        /// <returns>This object.</returns>
        /// <stable>ICU 3.0</stable>
        public override object Clone()
        {
            return this; // ICU4N TODO: UCultureInfo is not immutable, so we will need a real clone implementation
        }

        /// <summary>
        /// Returns <c>true</c> if the other object is another <see cref="UCultureInfo"/> with the
        /// same full name.
        /// Note that since names are not canonicalized, two <see cref="UCultureInfo"/>s that
        /// function identically might not compare equal.
        /// </summary>
        /// <param name="value"></param>
        /// <returns><c>true</c> if this <see cref="UCultureInfo"/> is equal to the specified <paramref name="value"/>.</returns>
        /// <stable>ICU 3.0</stable>
        public override bool Equals(object value)
        {
            if (ReferenceEquals(this, value))
                return true;

            // Special case - compare against invariant culture
            if (InvariantCulture.Equals(culture) && InvariantCulture.Equals(value))
                return true;

            if (value is UCultureInfo uCulture)
                return localeID.Equals(uCulture.localeID);

            return false;
        }

        /// <summary>
        /// Returns the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        /// <stable>ICU 3.0</stable>
        public override int GetHashCode()
        {
            return localeID.GetHashCode();
        }

        /// <inheritdoc/>
        // ICU4N TODO: Create specialized format types for ICU ?
        public override object GetFormat(Type formatType)
        {
            return culture.GetFormat(formatType);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return localeID;
        }
    }
}
