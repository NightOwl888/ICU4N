using ICU4N.Impl.Locale;
using System;
using System.Globalization;
using System.Threading;
#nullable enable

namespace ICU4N.Globalization
{
    public partial class UCultureInfo
    {

        /*new*/ public UCultureTypes CultureTypes => isNeutralCulture ? UCultureTypes.NeutralCultures : UCultureTypes.SpecificCultures;

        ///// <inheritdoc/>
        //public override Calendar Calendar => isInvariantCulture ? CultureInfo.InvariantCulture.Calendar : culture.Calendar;

        ///// <inheritdoc/>
        //public override CompareInfo CompareInfo => isInvariantCulture ? CultureInfo.InvariantCulture.CompareInfo : culture.CompareInfo;

        ///// <inheritdoc/>
        //public override DateTimeFormatInfo DateTimeFormat
        //{
        //    get => culture.DateTimeFormat;
        //    set => culture.DateTimeFormat = value;
        //}

        /// <inheritdoc/>
        // ICU4N TODO: Use InstalledUICulture for display name to mimic .NET?
        public /*override*/ string DisplayName => GetDisplayNameInternal(this, CurrentUICulture);

        /// <inheritdoc/>
        public /*override*/ string EnglishName
            => isInvariantCulture
                ? CultureInfo.InvariantCulture.EnglishName
                : GetDisplayName(localeID, English);

#if FEATURE_CULTUREINFO_IETFLANGUAGETAG
        public /*new*/ string IetfLanguageTag
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
        public /*override*/ bool IsNeutralCulture => isNeutralCulture;


//#if FEATURE_CULTUREINFO_KEYBOARDLAYOUTID
//        /// <inheritdoc/>
//        public override int KeyboardLayoutId => culture.KeyboardLayoutId;
//#endif
//#if FEATURE_CULTUREINFO_LCID
//        /// <inheritdoc/>
//        // ICU4N: According to the document at
//        // https://docs.microsoft.com/en-us/windows/win32/api/winnls/nf-winnls-localenametolcid
//        // the LCID is always 0x1000 when returning a CLDR culture.
//        public /*override*/ int LCID => isInvariantCulture ? CultureInfo.InvariantCulture.LCID : 0x1000;
//#endif

        /// <summary>
        /// <icu/> Returns the (normalized) base name for this locale,
        /// like <see cref="FullName"/>, but without keywords.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        // ICU4N specific: This was named getBaseName() in ICU4J
        public /*override*/ string Name => name ?? (name = GetName(localeID)); // always normalized

        /// <inheritdoc/>
        public /*override*/ string NativeName => isInvariantCulture ? CultureInfo.InvariantCulture.NativeName : GetDisplayName(localeID, localeID);

        /// <summary>
        /// Gets or sets a <see cref="UNumberFormatInfo"/> that defines the culturally appropriate format of
        /// displaying numbers, currency, and percentage.
        /// </summary>
        /// <value>A <see cref="UNumberFormatInfo"/> that defines the culturally appropriate format of
        /// displaying numbers, currency, and percentage.</value>
        /// <exception cref="ArgumentNullException">The property is set to <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="NumberFormat"/> property or any
        /// of the <see cref="UNumberFormatInfo"/> properties is set, and the <see cref="UCultureInfo"/> is read-only.</exception>
        public /*override*/ UNumberFormatInfo NumberFormat
        {
            get
            {
                if (numInfo == null)
                {
                    UNumberFormatInfo temp = new UNumberFormatInfo(cultureData);
                    temp.isReadOnly = isReadOnly;
                    Interlocked.CompareExchange(ref numInfo, temp, null);
                }
                return numInfo;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                VerifyWritable();
                numInfo = value;
            }
        }

        ///// <inheritdoc/>
        //public override Calendar[] OptionalCalendars => isInvariantCulture ? CultureInfo.InvariantCulture.OptionalCalendars : culture.OptionalCalendars;

        /// <summary>
        /// Returns the fallback locale (parent) for this locale. If this locale is root,
        /// returns <see cref="UCultureInfo.InvariantCulture"/>.
        /// </summary>
        /// <stable>ICU 3.2</stable>
        // ICU4N TODO: .NET compatibility tests don't pass.
        // Need to work out if GetFallback() (the original name of GetParent()) is indeed
        // the equivalent of CultureInfo.Parent or if it a different opertion that also
        // needs to be on the public API.
        internal /*override*/ UCultureInfo Parent => GetParent() ?? UCultureInfo.InvariantCulture;

        ///// <inheritdoc/>
        //public override TextInfo TextInfo => isInvariantCulture ? base.TextInfo : culture.TextInfo;

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
        public /*override*/ string ThreeLetterISOLanguageName
#else
        public string ThreeLetterISOLanguageName
#endif
            => GetThreeLetterISOLanguageName(localeID); // ISO 639-2


#if FEATURE_CULTUREINFO_THREELETTERWINDOWSLANGUAGENAME
        /// <inheritdoc/>
        public /*override*/ string? ThreeLetterWindowsLanguageName => culture?.ThreeLetterWindowsLanguageName; // Windows API
#endif

        /// <inheritdoc/>
        public /*override*/ string TwoLetterISOLanguageName
            => GetTwoLetterISOLanguageName(localeID); // ISO 639-1

        /// <summary>
        /// Refreshes cached culture-related information.
        /// This is to cover the <see cref="ToCultureInfo"/> API.
        /// </summary>
        public /*new*/ void ClearCachedData()
        {
            // this.culture = null;
            name = null;
            baseLocale = null;
            extensions = null;
            keywords = null;
            unicodeLocales = null;
            languageTag = null;
            UCultureData.ClearCachedData();
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
        public /*new*/ static UCultureInfo CreateSpecificCulture(string name)
        {
            UCultureInfo? culture;

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
        /// Creates a writable copy of the current <see cref="UCultureInfo"/>.
        /// </summary>
        /// <returns>A copy of the current <see cref="UCultureInfo"/>.</returns>
        /// <remarks>
        /// The clone is writable even if the original <see cref="UCultureInfo"/> is read-only.
        /// Therefore, the properties of the clone can be modified.
        /// <para/>
        /// A shallow copy of an object is a copy of the object only. If the object contains
        /// references to other objects, the shallow copy does not create copies of the referred
        /// objects. It refers to the original objects instead. In contrast, a deep copy of an object
        /// creates a copy of the object and a copy of everything directly or indirectly referenced by
        /// that object.
        /// <para/>
        /// The <see cref="Clone()"/> method creates an enhanced shallow copy. The objects returned by
        /// the <see cref="NumberFormat"/>, DateTimeFormat, TextInfo, and Calendar properties are also copied.
        /// Consequently, the cloned <see cref="UCultureInfo"/> object can modify its copied properties without
        /// affecting the original <see cref="UCultureInfo"/> object.
        /// </remarks>
        /// <draft>ICU 60.1</draft>
        public /*override*/ object Clone()
        {
            // ICU4N: UCultureInfo is not immutable, so we need a real clone implementation unlike in ICU4J
            UCultureInfo ci = (UCultureInfo)MemberwiseClone();
            ci.isReadOnly = false;

            // If this is exactly our type, we can make certain optimizations so that we don't allocate NumberFormatInfo or DTFI unless
            // they've already been allocated.  If this is a derived type, we'll take a more generic codepath.
            //if (!_isInherited)
            //{
                //if (dateTimeInfo != null)
                //{
                //    ci.dateTimeInfo = (DateTimeFormatInfo)dateTimeInfo.Clone();
                //}
                if (numInfo != null)
                {
                    ci.numInfo = (UNumberFormatInfo)numInfo.Clone();
                }
            //}
            //else
            //{
            //    ci.DateTimeFormat = (UDateTimeFormatInfo)this.DateTimeFormat.Clone();
            //    ci.NumberFormat = (UNumberFormatInfo)this.NumberFormat.Clone();
            //}

            //if (textInfo != null)
            //{
            //    ci.textInfo = (UTextInfo)textInfo.Clone();
            //}

            //if (dateTimeInfo != null && dateTimeInfo.Calendar == calendar)
            //{
            //    // Usually when we access CultureInfo.DateTimeFormat first time, we create the DateTimeFormatInfo object
            //    // using CultureInfo.Calendar. i.e. CultureInfo.DateTimeInfo.Calendar == CultureInfo.calendar.
            //    // When cloning CultureInfo, if we know it's still the case that CultureInfo.DateTimeInfo.Calendar == CultureInfo.calendar
            //    // then we can keep the same behavior for the cloned object and no need to create another calendar object.
            //    ci.calendar = ci.DateTimeFormat.Calendar;
            //}
            //else if (calendar != null)
            //{
            //    ci.calendar = (UCalendar)calendar.Clone();
            //}

            return ci;
        }

        /// <summary>
        /// Returns a read-only wrapper around the specified <see cref="UCultureInfo"/> object.
        /// </summary>
        /// <param name="ci">The <see cref="UCultureInfo"/> object to wrap.</param>
        /// <returns>A read-only <see cref="UCultureInfo"/> wrapper around ci.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="ci"/> is <c>null</c>.</exception>
        /// <remarks>
        /// This wrapper prevents any modifications to <paramref name="ci"/>, or the objects returned by
        /// the DateTimeFormat and <see cref="NumberFormat"/> properties.
        /// </remarks>
        /// <draft>ICU 60.1</draft>
        public static UCultureInfo ReadOnly(UCultureInfo ci)
        {
            if (ci == null)
            {
                throw new ArgumentNullException(nameof(ci));
            }

            if (ci.IsReadOnly)
            {
                return ci;
            }
            UCultureInfo newInfo = (UCultureInfo)ci.MemberwiseClone();

            //if (!ci.IsNeutralCulture)
            {
                // If this is exactly our type, we can make certain optimizations so that we don't allocate NumberFormatInfo or DTFI unless
                // they've already been allocated.  If this is a derived type, we'll take a more generic codepath.
                //if (!ci._isInherited)
                //{
                    //if (ci.dateTimeInfo != null)
                    //{
                    //    newInfo.dateTimeInfo = UDateTimeFormatInfo.ReadOnly(ci.dateTimeInfo);
                    //}
                    if (ci.numInfo != null)
                    {
                        newInfo.numInfo = UNumberFormatInfo.ReadOnly(ci.numInfo);
                    }
                //}
                //else
                //{
                //    newInfo.DateTimeFormat = DateTimeFormatInfo.ReadOnly(ci.DateTimeFormat);
                //    newInfo.NumberFormat = NumberFormatInfo.ReadOnly(ci.NumberFormat);
                //}
            }

            //if (ci.textInfo != null)
            //{
            //    newInfo.textInfo = UTextInfo.ReadOnly(ci.textInfo);
            //}

            //if (ci.calendar != null)
            //{
            //    newInfo.calendar = UCalendar.ReadOnly(ci.calendar);
            //}

            // Don't set the read-only flag too early.
            // We should set the read-only flag here.  Otherwise, info.DateTimeFormat will not be able to set.
            newInfo.isReadOnly = true;

            return newInfo;
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="UCultureInfo"/> is read-only.
        /// </summary>
        /// <value><c>true</c> if the current <see cref="UCultureInfo"/> is read-only; otherwise, <c>false</c>.
        /// The default is <c>false</c>.</value>
        /// <remarks>
        /// If the <see cref="UCultureInfo"/> is read-only, the DateTimeFormat and <see cref="NumberFormat"/>
        /// instances are also read-only.
        /// </remarks>
        /// <draft>ICU 60.1</draft>
        public bool IsReadOnly => isReadOnly;

        private void VerifyWritable()
        {
            if (isReadOnly)
            {
                throw new InvalidOperationException(SR.InvalidOperation_ReadOnly);
            }
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
        public override bool Equals(object? value)
        {
            if (ReferenceEquals(this, value))
                return true;

            // Special case - compare against invariant culture
            if (isInvariantCulture && value is CultureInfo cultureInfo)
                return CultureInfo.InvariantCulture.Equals(cultureInfo);

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
                return CultureInfo.InvariantCulture.GetHashCode();

            return localeID.GetHashCode();
        }

        //// ICU4N: Unfortunately, when DateTimeFormatInfo or NumberFormatInfo
        //// are requested here, the return type must match because internally .NET
        //// will try to cast to the type that was requested. This means there is no
        //// way to customize the default string.Format() method for numeric and date
        //// types without either wrapping those arguments in custom IFormattable types
        //// to request something other than DateTimeFormatInfo or NumberFormatInfo.

        /// <inheritdoc/>
        public /*override*/ object? GetFormat(Type? formatType)
        {
            if (formatType == typeof(UNumberFormatInfo))
            {
                return NumberFormat;
            }
            if (formatType == typeof(NumberFormatInfo))
            {
                return culture.NumberFormat;
            }
            if (formatType == typeof(DateTimeFormatInfo))
            {
                return culture.DateTimeFormat;
            }

            return null;
        }

        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public override string ToString()
        {
            return localeID;
        }

        private static class SR
        {
            public const string InvalidOperation_ReadOnly = "Instance is read-only.";
        }
    }
}
