using J2N.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ICU4N.Globalization
{
    /// <summary>
    /// Extensions to <see cref="CultureInfo"/>.
    /// </summary>
    public static class CultureInfoExtensions
    {
        private static readonly LurchTable<CultureInfo, UCultureInfo> uCultureInfoCache
            = new LurchTable<CultureInfo, UCultureInfo>(LurchTableOrder.Access, limit: 64, comparer: CultureInfoEqualityComparer.Instance);

        static CultureInfoExtensions()
        {
            // ICU4N: We need to ensure the calendar data is loaded before dealing with a Lazy<T> here, as the calendar data requires a call
            // back to ToUCultureInfo(), which leads to infinite recursion for Chinese cultures that are aliased to lookup calendars.
            UCultureInfo.DotNetLocaleHelper.EnsureInitialized();
        }

        /// <summary>
        /// <icu/> Returns a <see cref="UCultureInfo"/> object for a <see cref="CultureInfo"/>.
        /// The <see cref="UCultureInfo"/> is canonicalized.
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> instance.</param>
        /// <returns>Returns the closest matching <see cref="UCultureInfo"/> instance for the provided <see cref="CultureInfo"/>.</returns>
        public static UCultureInfo ToUCultureInfo(this CultureInfo culture)
        {
            if (culture == null)
                return null;

            //if (culture is UCultureInfo uCulture)
            //    return uCulture;

            return uCultureInfoCache.GetOrAdd(culture, (key) => UCultureInfo.DotNetLocaleHelper.ToUCultureInfo(key));
        }

        // ICU4N: For now, we are just comparing using CultureInfo.Equals() as well as comparing
        // both calendar properties. Ideally, we would compare each writable/overridable property
        // for equality, but this is all we support right now.
        private class CultureInfoEqualityComparer : IEqualityComparer<CultureInfo>
        {
            public static IEqualityComparer<CultureInfo> Instance { get; } = new CultureInfoEqualityComparer();

            public bool Equals(CultureInfo x, CultureInfo y)
            {
                if (x is null)
                    return y is null;

                return x.Equals(y)
                    && (x.Calendar is null ? y.Calendar is null : x.Calendar.Equals(y.Calendar))
                    && (x.DateTimeFormat is null ? y.DateTimeFormat is null : 
                        (x.DateTimeFormat.Calendar is null ? y.DateTimeFormat is null : x.DateTimeFormat.Calendar.Equals(y.DateTimeFormat.Calendar)));
            }

            public int GetHashCode(CultureInfo obj)
            {
                return obj.GetHashCode()
                    ^ (!(obj.Calendar is null) ? obj.Calendar.GetHashCode() : 31)
                    ^ (!(obj.DateTimeFormat is null) && !(obj.DateTimeFormat.Calendar is null) ? obj.DateTimeFormat.Calendar.GetHashCode() : 31);
            }
        }

        internal static bool IsMatch(this CultureInfo culture, UCultureTypes types)
        {
#if FEATURE_CULTUREINFO_GETCULTURES
            return ((int)culture.CultureTypes & (int)types) != 0;
#else
            return culture.IsNeutralCulture && types.HasFlag(UCultureTypes.NeutralCultures)
                || !culture.IsNeutralCulture && types.HasFlag(UCultureTypes.SpecificCultures);
#endif
        }

        ///// <summary>
        ///// Returns the language code for this <paramref name="culture"/>, which will either be the empty string
        ///// or a lowercase ISO 639 code.
        ///// </summary>
        ///// <param name="culture"></param>
        ///// <returns></returns>
        //// ICU4N TODO: Add seealso's
        //public static string GetLanguage(this CultureInfo culture)
        //{
        //    return Base(culture).GetLanguage();
        //}

        //public static string GetScript(this CultureInfo culture)
        //{
        //    return Base(culture).GetScript();
        //}

        //public static string GetCountry(this CultureInfo culture)
        //{
        //    return Base(culture).GetRegion();
        //}

        //public static string GetVariant(this CultureInfo culture)
        //{
        //    return Base(culture).GetVariant();
        //}

        //private static readonly LurchTable<string, BaseLocale> baseLocales = new LurchTable<string, BaseLocale>(LurchTableOrder.Access, limit: 64, comparer: null);

        //internal static BaseLocale Base(this CultureInfo culture)
        //{
        //    if (culture == null)
        //        throw new ArgumentNullException(nameof(culture));

        //    string localeID = culture.ToString();
        //    return baseLocales.GetOrAdd(localeID, (key) =>
        //    {
        //        string language, script, region, variant;
        //        language = script = region = variant = string.Empty;
        //        if (!culture.Equals(CultureInfo.InvariantCulture))
        //        {
        //            LocaleIDParser lp = new LocaleIDParser(localeID);
        //            language = lp.GetLanguage();
        //            script = lp.GetScript();
        //            region = lp.GetCountry();
        //            variant = lp.GetVariant();
        //        }
        //        return BaseLocale.GetInstance(language, script, region, variant);
        //    });
        //}
    }
}
