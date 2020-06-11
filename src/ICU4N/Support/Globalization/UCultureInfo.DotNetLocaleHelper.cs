using ICU4N.Impl;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ICU4N.Globalization
{
    public partial class UCultureInfo
    {
        /// <summary>
        /// .NET Locale Helper
        /// </summary>
        internal static class DotNetLocaleHelper
        {
            /*
             *  Java
             * 6 locales.  When an ICU locale matches <minumum base> with
             * <keyword>/<value>, the ICU locale is mapped to <Java> locale.
             * For example, both ja_JP@calendar=japanese and ja@calendar=japanese
             * are mapped to Java locale "ja_JP_JP".  ICU locale "nn" is mapped
             * to Java locale "no_NO_NY".
             */

            ///// <summary>
            ///// This table is used for mapping between ICU and special .NET locales.
            ///// When an ICU locale matches &lt;minumum base&gt; with
            ///// &lt;keyword>/&tl;value>, the ICU locale is mapped to &lt;.NET> locale.
            ///// </summary>
            //private static readonly string[][] NET_MAPDATA = { // ICU4N TODO: Do we need different values for .NET Framework/.NET Standard ?
            //    //  { <Java>,       <ICU base>, <keyword>,  <value>,    <minimum base>
            //    new string[] { "ja-JP",   "ja_JP",    "calendar", "japanese", "ja"},
            //    //new string[] { "nn-NO",   "nn_NO",    null,       null,       "nn"},
            //    new string[] { "th-TH",   "th_TH",    "numbers",  "thai",     "th"},
            //};

            private static readonly string[][] NET_MAPDATA = { // ICU4N TODO: Do we need different values for .NET Framework/.NET Standard ?
                //  { <Java>,       <ICU base>, <keyword>,  <value>,    <minimum base>
                //new string[] { "ja-JP",   "ja_JP",    "calendar", "japanese", "ja"},
                //new string[] { "nn-NO",   "nn_NO",    null,       null,       "nn"},
                new string[] { "th-TH",   "th_TH",    "numbers",  "thai", "",     "th"},
            };

            private static readonly IDictionary<System.Type, string> DOTNET_CALENDARS = new Dictionary<System.Type, string>
            {
                { typeof(JapaneseCalendar), "japanese" },
                { typeof(ThaiBuddhistCalendar), "buddhist" },
                { typeof(ChineseLunisolarCalendar), "chinese" },
                { typeof(PersianCalendar), "persian" },
                { typeof(HijriCalendar), "islamic" },
                { typeof(HebrewCalendar), "hebrew" },
                { typeof(TaiwanCalendar), "taiwan" },
            };


            public static UCultureInfo ToUCultureInfo(CultureInfo loc)
            {
                if (loc == CultureInfo.InvariantCulture)
                {
                    return new UCultureInfo("");
                }

                var name = loc.Name;

                // Convert from RFC 4646, ISO 639, ISO 639-2, and ISO 15924 to ICU  format 


                // ICU4N TODO: Need to append currency, number, and collation data
                //name = name.Replace('-', '_');
                var segments = name.Split(new char[] { '-', '_' });
                string newName = "";
                for (int i = 0; i < segments.Length; i++)
                {
                    if (newName.Length > 0)
                        newName += '_';
                    if (i == 0)
                        newName += segments[i].ToLowerInvariant();
                    else
                        newName += segments[i].ToUpperInvariant(); // Special case - .NET makes the variant lower case, but ULocale expects upper case
                }

                // Special cases...

                string language = segments.Length > 0 ? segments[0] : "";
                string country = segments.Length > 1 ? segments[1] : "";
                string variant = segments.Length > 2 ? segments[2] : "";

                // .NET doesn't recognize no-NO-NY any more than ICU does, but if it is input,
                // we need to patch it (at least for the tests)

                if (language.Equals("no") && country.Equals("NO") && variant.Equals("NY", StringComparison.OrdinalIgnoreCase))
                {
                    newName = "nn_NO";
                }

                // Calander info
                var calandarType = loc.Calendar.GetType();
                if (!calandarType.Equals(typeof(GregorianCalendar)) &&
                    DOTNET_CALENDARS.TryGetValue(calandarType, out string calendar))
                {
                    string sep = newName.Contains("@") ? ";" : "@";
                    newName += string.Concat(sep, "calandar=", calendar);
                }

                //for (int i = 0; i < NET_MAPDATA.Length; i++)
                //{
                //    if (newName.StartsWith(NET_MAPDATA[i][1], StringComparison.Ordinal) & NET_MAPDATA[i][2] != null)
                //    {
                //        string sep = newName.Contains("@") ? ";" : "@";
                //        newName += string.Concat(sep, NET_MAPDATA[i][2], "=", NET_MAPDATA[i][3]);
                //    }
                //}


                return new UCultureInfo(newName, loc);
            }

            public static CultureInfo ToCultureInfo(UCultureInfo uloc)
            {
                var name = uloc.FullName;

                if (name.Equals("root", StringComparison.OrdinalIgnoreCase) || name.Equals("any", StringComparison.OrdinalIgnoreCase))
                {
                    return CultureInfo.InvariantCulture;
                }

                //// Strip off the config options
                //int optionsIndex = name.IndexOf('@');
                //if (optionsIndex > -1)
                //{
                //    // ICU4N TODO: Need to convert calendar, currency, number, and collation options by
                //    // creating a custom CultureInfo subclass...where possible

                //    name = name.Substring(0, optionsIndex); // ICU4N: Checked 2nd parameter
                //}

                //string newName = name.Replace('_', '-').Trim('-');

                // ICU4N special cases...

                // ICU4N TODO: Check whether accuracy can be improved by using UCultureInfo.ToLanguageTag()

                var parser = new LocaleIDParser(uloc.FullName);

                var newName = parser.GetBaseName().Replace('_', '-');
                var language = parser.GetLanguage();
                var country = parser.GetCountry();
                var variant = parser.GetVariant();

                // .NET doesn't recognize no-NO-NY any more than ICU does, but if it is input,
                // we need to patch it (at least for the tests)

                if (language.Equals("no") && country.Equals("NO") && variant.Equals("NY"))
                {
                    newName = "nn-NO";
                }


                try
                {
                    CultureInfo culture = new CultureInfo(newName);

                    //#if FEATURE_CULTUREINFO_UNKNOWNLANGUAGE
                    //                    // ICU4N: In .NET Standard 1.x, some invalid cultures are allowed
                    //                    // to be created, but will be "unknown" languages. We need to manually
                    //                    // ignore these.
                    //                    if (culture.EnglishName.StartsWith("Unknown Language", StringComparison.Ordinal))
                    //                    {
                    //                        return null;
                    //                    }
                    //#endif
                    return culture;
                }
                catch (CultureNotFoundException)
                {
                    return null;
                }
            }

            public static CultureInfo CurrentCulture
            {
                get => CultureInfo.CurrentCulture;
                set
                {
#if FEATURE_CULTUREINFO_CURRENTCULTURE_SETTER
                    CultureInfo.CurrentCulture = value;
#else
                    System.Threading.Thread.CurrentThread.CurrentCulture = value;
#endif
                }
            }

            public static CultureInfo CurrentUICulture
            {
                get => CultureInfo.CurrentUICulture;
                set
                {
#if FEATURE_CULTUREINFO_CURRENTCULTURE_SETTER
                    CultureInfo.CurrentUICulture = value;
#else
                    System.Threading.Thread.CurrentThread.CurrentUICulture = value;
#endif
                }
            }

            //public static CultureInfo GetDefault(Category category)
            //{
            //    CultureInfo loc = CultureInfo.CurrentCulture;
            //    switch (category)
            //    {
            //        case Category.DISPLAY:
            //            loc = CultureInfo.CurrentUICulture;
            //            break;
            //        case Category.FORMAT:
            //            loc = CultureInfo.CurrentCulture;
            //            break;
            //    }
            //    return loc;
            //}

            //            public static void SetDefault(Category category, CultureInfo newLocale)
            //            {
            //                switch (category)
            //                {
            //                    case Category.DISPLAY:
            //#if FEATURE_CULTUREINFO_CURRENTCULTURE_SETTER
            //                        CultureInfo.CurrentUICulture = newLocale;
            //#else
            //                        System.Threading.Thread.CurrentThread.CurrentUICulture = newLocale;
            //#endif
            //                        break;
            //                    case Category.FORMAT:
            //#if FEATURE_CULTUREINFO_CURRENTCULTURE_SETTER
            //                        CultureInfo.CurrentCulture = newLocale;
            //#else
            //                        System.Threading.Thread.CurrentThread.CurrentCulture = newLocale;
            //#endif
            //                        break;
            //                }
            //            }

            //            // Returns true if the given Locale matches the original
            //            // default locale initialized by JVM by checking user.XXX
            //            // system properties. When the system properties are not accessible,
            //            // this method returns false.
            //            public static bool IsOriginalDefaultLocale(CultureInfo loc)
            //            {
            //#if FEATURE_CULTUREINFO_DEFAULTTHREADCURRENTCULTURE
            //                return loc.Equals(CultureInfo.DefaultThreadCurrentCulture);
            //#else
            //                // ICU4N HACK: For .NET Framework 4.0, we need to read the internal state of
            //                // defaultThreadCurrentCulture by reading the private field through reflection.
            //                // See: https://rastating.github.io/setting-default-currentculture-in-all-versions-of-net/
            //                CultureInfo defaultThreadCurrentCulture = typeof(CultureInfo).InvokeMember("s_userDefaultCulture",
            //                            BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Static,
            //                            null,
            //                            loc,
            //                            new object[0]) as CultureInfo;

            //                return loc.Equals(defaultThreadCurrentCulture);
            //#endif
            //            }

        }
    }
}
