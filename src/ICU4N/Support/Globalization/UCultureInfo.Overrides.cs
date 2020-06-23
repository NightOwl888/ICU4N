using ICU4N.Impl.Locale;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ICU4N.Globalization
{
    public partial class UCultureInfo
    {
        /// <inheritdoc/>
        public override Calendar Calendar => isInvariantCulture ? CultureInfo.InvariantCulture.Calendar : culture.Calendar;

        /// <inheritdoc/>
        public override CompareInfo CompareInfo => isInvariantCulture ? CultureInfo.InvariantCulture.CompareInfo : culture.CompareInfo;

        /// <inheritdoc/>
        public override DateTimeFormatInfo DateTimeFormat
        {
            get => culture.DateTimeFormat;
            set => culture.DateTimeFormat = value;
        }

        /// <inheritdoc/>
        // ICU4N TODO: Use InstalledUICulture for display name to mimic .NET?
        public override string DisplayName => GetDisplayNameInternal(this, CurrentUICulture);

        /// <inheritdoc/>
        public override string EnglishName
            => isInvariantCulture
                ? CultureInfo.InvariantCulture.EnglishName
                : GetDisplayName(localeID, English);

#if FEATURE_CULTUREINFO_IETFLANGUAGETAG
        public new string IetfLanguageTag
#else
        public string IetfLanguageTag
#endif
        {
            get
            {
                if (languageTag == null)
                {
                    var tempLanguageTag = ToIetfLanguageTag();
                    // For .NET compatibility, remove the "und" from the language tag.
                    // Basically an optimized version of Regex.Replace Regex.Replace(tempLanguageTag, "^und-?", string.Empty)
                    languageTag = tempLanguageTag == LanguageTag.Undetermined ? string.Empty :
                        tempLanguageTag.StartsWith(UndeterminedWithSeparator, StringComparison.Ordinal)
                            ? tempLanguageTag.Substring(UndeterminedWithSeparator.Length)
                            : tempLanguageTag;
                }

                return languageTag;
            }
        }

        /// <inheritdoc/>
        public override bool IsNeutralCulture => isInvariantCulture ? CultureInfo.InvariantCulture.IsNeutralCulture : isNeutralCulture;


#if FEATURE_CULTUREINFO_KEYBOARDLAYOUTID
        /// <inheritdoc/>
        public override int KeyboardLayoutId => culture.KeyboardLayoutId;
#endif
#if FEATURE_CULTUREINFO_LCID
        /// <inheritdoc/>
        // ICU4N: According to the document at
        // https://docs.microsoft.com/en-us/windows/win32/api/winnls/nf-winnls-localenametolcid
        // the LCID is always 0x1000 when returning a CLDR culture.
        public override int LCID => isInvariantCulture ? base.LCID : 0x1000;
#endif

        /// <summary>
        /// <icu/> Returns the (normalized) base name for this locale,
        /// like <see cref="FullName"/>, but without keywords.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        // ICU4N specific: This was named getBaseName() in ICU4J
        public override string Name => GetName(localeID); // always normalized

        /// <inheritdoc/>
        public override string NativeName => isInvariantCulture ? CultureInfo.InvariantCulture.NativeName : GetDisplayName(localeID, localeID);

        /// <inheritdoc/>
        public override NumberFormatInfo NumberFormat
        {
            get => culture.NumberFormat;
            set => culture.NumberFormat = value;
        }

        /// <inheritdoc/>
        public override Calendar[] OptionalCalendars => isInvariantCulture ? CultureInfo.InvariantCulture.OptionalCalendars : culture.OptionalCalendars;

        /// <summary>
        /// Returns the fallback locale (parent) for this locale. If this locale is root,
        /// returns <c>null</c>.
        /// </summary>
        /// <stable>ICU 3.2</stable>
        public override CultureInfo Parent => GetParent() ?? UCultureInfo.InvariantCulture;

        /// <inheritdoc/>
        public override TextInfo TextInfo => isInvariantCulture ? base.TextInfo : culture.TextInfo;

        /// <summary>
        /// Returns a three-letter abbreviation for the language. If language is
        /// empty, returns the empty string. Otherwise, returns
        /// a lowercase ISO 639-2/T language code.
        /// </summary>
        /// <remarks>The ISO 639-2 language codes can be found on-line at
        /// <a href="ftp://dkuug.dk/i18n/iso-639-2.txt">ftp://dkuug.dk/i18n/iso-639-2.txt</a>.
        /// </remarks>
        /// <exception cref="System.Resources.MissingManifestResourceException">
        /// If the three-letter language abbreviation is not available for this locale.</exception>
        /// <stable>ICU 3.0</stable>
#if FEATURE_CULTUREINFO_THREELETTERISOLANGUAGENAME
        public override string ThreeLetterISOLanguageName     
#else
        public string ThreeLetterISOLanguageName
#endif
            => GetThreeLetterISOLanguageName(localeID); // ISO 639-2


#if FEATURE_CULTUREINFO_THREELETTERWINDOWSLANGUAGENAME
        /// <inheritdoc/>
        public override string ThreeLetterWindowsLanguageName => culture?.ThreeLetterWindowsLanguageName; // Windows API
#endif

        /// <inheritdoc/>
        public override string TwoLetterISOLanguageName
            => GetTwoLetterISOLanguageName(localeID); // ISO 639-1

        /// <summary>
        /// Refreshes cached culture-related information.
        /// This is to cover the <see cref="ToCultureInfo"/> API.
        /// </summary>
        public new void ClearCachedData()
        {
            // this.culture = null;
            baseLocale = null;
            extensions = null;
            keywords = null;
            unicodeLocales = null;
            languageTag = null;
        }

        /// <summary>
        /// Creates a <see cref="UCultureInfo"/> that represents the specific culture
        /// that is associated with the specified name.
        /// </summary>
        /// <param name="name">A predefined locale ID or the name of an existing <see cref="ToCultureInfo"/> object.
        /// <paramref name="name"/> is not case-sensitive.</param>
        /// <returns>A <see cref="UCultureInfo"/> objec that represents:
        /// <para/>
        /// The invariant culture, if <paramref name="name"/> is an empty string.
        /// <para/>
        /// -or-
        /// <para/>
        /// The specific culture associated with <paramref name="name"/>, if <paramref name="name"/> is a neutral culture.
        /// <para/>
        /// -or-
        /// <para/>
        /// The culture specified by <paramref name="name"/>, if <paramref name="name"/> is already a specific culture.
        /// </returns>
        // Return a specific culture. A tad irrelevent now since we always
        // return valid data for neutral locales.
        //
        // Note that there's interesting behavior that tries to find a
        // smaller name, ala RFC4647, if we can't find a bigger name.
        // That doesn't help with things like "zh" though, so the approach
        // is of questionable value
        public new static UCultureInfo CreateSpecificCulture(string name)
        {
            UCultureInfo culture;

            try
            {
                culture = new UCultureInfo(name);
            }
            catch (ArgumentException)
            {
                // When CultureInfo throws this exception, it may be because someone passed the form
                // like "az-az" because it came out of an http accept lang. We should try a little
                // parsing to perhaps fall back to "az" here and use *it* to create the neutral.
                culture = null;
                for (int idx = 0; idx < name.Length; idx++)
                {
                    if ('-' == name[idx] || '_' == name[idx])
                    {
                        try
                        {
                            culture = new UCultureInfo(name.Substring(0, idx));
                            break;
                        }
                        catch (ArgumentException)
                        {
                            // throw the original exception so the name in the string will be right
                            throw;
                        }
                    }
                }

                if (culture == null)
                {
                    // nothing to save here; throw the original exception
                    throw;
                }
            }

            // In the most common case, they've given us a specific culture, so we'll just return that.
            if (!culture.IsNeutralCulture)
            {
                return culture;
            }

            return new UCultureInfo(culture.FullName);
        }


        /// <summary>
        /// This is for compatibility with <see cref="ToCultureInfo"/> -- in actuality, since <see cref="UCultureInfo"/> is
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
            if (isInvariantCulture)
                return base.Equals(value);

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
            // Special case - compare against invariant culture
            if (isInvariantCulture)
                return base.GetHashCode();

            return localeID.GetHashCode();
        }

        /// <inheritdoc/>
        // ICU4N: Unfortunately, when DateTimeFormatInfo or NumberFormatInfo
        // are requested here, the return type must match because internally .NET
        // will try to cast to the type that was requested. This means there is no
        // way to customize the default string.Format() method for numeric and date
        // types without either wrapping those arguments in custom IFormattable types
        // to request something other than DateTimeFormatInfo or NumberFormatInfo.
        public override object GetFormat(Type formatType)
        {
            return culture.GetFormat(formatType);
        }

        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public override string ToString()
        {
            return localeID;
        }
    }
}
