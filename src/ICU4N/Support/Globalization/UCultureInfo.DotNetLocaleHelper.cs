using ICU4N.Impl;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Globalization;

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
            private static readonly string[][] NET_MAPDATA = { // ICU4N TODO: Do we need different values for .NET Framework/.NET Standard ?
                //             { <.NET>,     <ICU base>, <keyword>,  <value>,    <minimum base>
                //new string[] { "ja-JP",   "ja_JP",    "calendar", "japanese", "ja"},
                //new string[] { "nn-NO",   "nn_NO",    null,       null,       "nn"},
                //new string[] { "th-TH",     "th_TH",    "numbers",  "thai",     "th"},
            };

            private static readonly IDictionary<string, string> UnsupportedDotNetNames = new Dictionary<string, string>
            {
                // <.NET>      = <ICU Base>
                ["ku"]         = "ckb",
                ["ku-Arab-IQ"] = "ckb_IQ",
                ["ku-Arab-IR"] = "ckb_IR",
                ["prs-AF"]     = "fa_AF",
                ["quz"]        = "qu",
                ["quz-BO"]     = "qu_BO",
                ["quz-EC"]     = "qu_EC",
                ["quz-PE"]     = "qu_PE",
            };


            private static readonly string[][] CollationMapData =
            {
                //             <DotNet>          <ICU base>, <keyword>,       <value>,           <minimum base>
                new string[] { "es-ES_tradnl",   "es_ES",    "collation",     "traditional",     "es"      },
                new string[] { "zh-TW_pronun",   "zh_TW",    "collation",     "zhuyin",          "zh_TW"   },
                new string[] { "zh-CN_stroke",   "zh_CN",    "collation",     "stroke",          "zh"      },
                
                new string[] { "zh-SG_stroke",   "zh_SG",    "collation",     "stroke",          "zh_SG"   },
                new string[] { "zh-MO",          "zh_MO",    "collation",     "pinyin",          "zh_MO"   },
                new string[] { "zh-MO_stroke",   "zh_MO",    null,            null,              "zh_MO"   },
                
                new string[] { "de-DE_phoneb",   "de_DE",    "collation",     "phonebook",       "de"      },

                // Map this only .NET > ICU
                new string[] { "hu-HU_technl",   "hu_HU",     null,           null,              "hu"      }, // Not supported in ICU ?
                new string[] { "ka-GE_modern",   "ka_GE",     null,           null,              "ka"      }, // This is the standard..? Traditional is apparently not supported by ICU, so we map both to standard (modern).


                //// Map this only .NET > ICU (stroke is the default)
                //new string[] { "zh-HK_stroke", "zh_HK", null, null, "zh_HK" }, // This maps to both LCID 0x00000c04 and 0x00020c04 (the latter of which is an obsolete Windows locale)
                
                //new string[] { "ja-JP_unicod", "ja_JP", "collation", "unihan", "ja_JP" }, // Obsolete Windows locale (may throw CultureNotFoundException)
                //new string[] { "ko-KR_unicod", "ko_KR", "collation", "unihan", "ko_KR" }, // Obsolete Windows locale (may throw CultureNotFoundException)
            };



            private static readonly IDictionary<Type, string> DOTNET_CALENDARS = new Dictionary<Type, string>
            {
                { typeof(GregorianCalendar), "gregorian" },
                { typeof(JapaneseCalendar), "japanese" },
                { typeof(ThaiBuddhistCalendar), "buddhist" },
                { typeof(ChineseLunisolarCalendar), "chinese" },
                { typeof(PersianCalendar), "persian" },
                { typeof(HijriCalendar), "islamic" },
                { typeof(HebrewCalendar), "hebrew" },
                { typeof(TaiwanCalendar), "taiwan" },
                { typeof(UmAlQuraCalendar), "islamic-umalqura" }
            };


            public static UCultureInfo ToUCultureInfo(CultureInfo culture)
            {
                if (CultureInfo.InvariantCulture.Equals(culture))
                {
                    return new UCultureInfo("", culture);
                }

                var collationName = culture.CompareInfo?.Name;
                LocaleIDParser parser = null;

                if (!string.IsNullOrEmpty(collationName))
                {
                    // First check whether this is an alternate sort order
                    // https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo?view=netcore-3.1#alternate-sort-orders
                    foreach (var collation in CollationMapData)
                    {
                        if (collation[0].Equals(collationName, StringComparison.Ordinal))
                        {
                            parser = new LocaleIDParser(collation[1]);
                            if (collation[2] != null)
                                parser.SetKeywordValue(collation[2], collation[3]);
                            break;
                        }
                    }
                }

                // Convert from RFC 4646, ISO 639, ISO 639-2, and ISO 15924 to ICU format 

                // If collation wasn't found, use the culture name (base name) to get a new parser.
                // Map any unsupported names from .NET to their nearest equivalent in ICU.
                if (parser == null)
                {
                    var name = UnsupportedDotNetNames.TryGetValue(culture.Name, out string baseName) ? baseName : culture.Name;
                    parser = new LocaleIDParser(name);
                }

                // Calander formatting info
                // The user may have set CultureInfo.DateTimeFormat.Calendar explicitly, in which
                // case it will be different from the CultureInfo.Calendar property. We cover this
                // by checking both cases.
                var defaultCalendar = GetDefaultCalendar(parser.GetBaseName());
                var calendarType = culture.Calendar.GetType();
                var formattingCalendarType = culture.DateTimeFormat.Calendar.GetType();
                if (DOTNET_CALENDARS.TryGetValue(calendarType, out string calendar) && calendar != defaultCalendar)
                    parser.SetKeywordValue("calendar", calendar);
                else if (DOTNET_CALENDARS.TryGetValue(formattingCalendarType, out string formattingCalendar) && formattingCalendar != defaultCalendar)
                    parser.SetKeywordValue("calendar", formattingCalendar);

                // ICU4N TODO: Need to append currency, number
                // but this isn't important until we provide string formatting support.

                return new UCultureInfo(parser.GetName(), culture);
            }

            public static CultureInfo ToCultureInfo(UCultureInfo uloc)
            {
                var baseName = uloc.Name;

                if (baseName.Equals("root", StringComparison.OrdinalIgnoreCase) || baseName.Equals("any", StringComparison.OrdinalIgnoreCase))
                {
                    return CultureInfo.InvariantCulture;
                }

                uloc.Keywords.TryGetValue("collation", out string collationValue);
                foreach (var collation in CollationMapData)
                {
                    if (baseName.Equals(collation[1], StringComparison.Ordinal) || baseName.Equals(collation[4], StringComparison.Ordinal))
                    {
                        if (collation[2] == null || collationValue != null && collationValue.Equals(collation[3]))
                        {
                            baseName = collation[0];
                            break;
                        }
                    }
                }

                var parser = new LocaleIDParser(baseName);
                const int language = 0;
                const int country = 2;
                const int variant = 3;
                string[] parts = parser.GetLanguageScriptCountryVariant();
                string languageString = parts[language];
                string countryString = parts[country];
                string variantString = parts[variant];

                try
                {
                    // ICU4N TODO: Apply calendars, numbers, etc before returning
                    return new CultureInfo(
                        languageString +
                        (countryString.Length > 0 ? '-' + countryString : "") +
                        (variantString.Length > 0 ? '-' + variantString : ""));
                }
                catch (CultureNotFoundException)
                {
                    // Fallback to original base name(collation not supported)
                    //baseName = uloc.Name;
                }


                try
                {
                    return new CultureInfo(uloc.IetfLanguageTag);
                }
                catch (CultureNotFoundException)
                {
                    // Fallback
                    return ToCultureInfo(uloc.GetParent() ?? UCultureInfo.InvariantCulture.ToUCultureInfo());
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


            /// <summary>
            /// Gets the default ICU calendar for the <paramref name="baseName"/>.
            /// </summary>
            private static string GetDefaultCalendar(string baseName)
            {
                var rb = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.IcuBundle, baseName, ICUResourceBundle.IcuDataAssembly);
                return rb.GetStringWithFallback("calendar/default");
            }
        }
    }
}
