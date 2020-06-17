using J2N.Collections.Concurrent;
using System.Globalization;

namespace ICU4N.Globalization
{
    /// <summary>
    /// Extensions to <see cref="CultureInfo"/>.
    /// </summary>
    public static class CultureInfoExtensions
    {
        private static readonly LurchTable<CultureInfo, UCultureInfo> uCultureInfoCache = new LurchTable<CultureInfo, UCultureInfo>(LurchTableOrder.Access, limit: 64, comparer: null);

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

            if (culture is UCultureInfo uCulture)
                return uCulture;

            return uCultureInfoCache.GetOrAdd(culture, (key) => UCultureInfo.DotNetLocaleHelper.ToUCultureInfo(key));
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
