using ICU4N.Impl;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace ICU4N.Globalization
{
    public partial class UCultureInfo
    {
        /// <summary>
        /// .NET Locale Helper
        /// </summary>
        internal static class DotNetLocaleHelper
        {
            // Local properties for LazyInitializer
            private static bool calendarsInitialized;
            private static object calendarsSyncLock = new object();

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



            private static readonly IDictionary<Type, string> DotNetCalendars = new Dictionary<Type, string>
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

            /// <summary>
            /// A mapping of ICU baseName to default calendar. Since in .NET the calendar can be set or changed
            /// in a subclass of CulureInfo, we load the entire list of non-gregorian calendars here so we can
            /// detect any variant from the ICU default based on the CultureInfo that is presented.
            /// </summary>
            private static IDictionary<string, string> IcuNonGregorianDefaultCalendars;

            internal static IDictionary<string, string> LoadNonGregorianDefaultCalendars()
            {
                var result = new Dictionary<string, string>(StringComparer.Ordinal);
                var supportedCultures = UCultureInfo.GetCultures(UCultureTypes.AllCultures);
                foreach (var culture in supportedCultures)
                {
                    var defaultCalendar = GetDefaultCalendarFromBundle(culture.Name);
                    if (!defaultCalendar.Equals("gregorian", StringComparison.Ordinal))
                    {
                        result[culture.Name] = defaultCalendar;
                    }
                }
                return result;
            }

            static DotNetLocaleHelper()
            {
                EnsureInitialized(); 
            }

            internal static void EnsureInitialized()
            {
                LazyInitializer.EnsureInitialized(ref IcuNonGregorianDefaultCalendars, ref calendarsInitialized, ref calendarsSyncLock, LoadNonGregorianDefaultCalendars);
            }


            public static UCultureInfo ToUCultureInfo(CultureInfo culture)
            {
                if (CultureInfo.InvariantCulture.Equals(culture))
                {
                    return UCultureInfo.InvariantCulture;
                }

                var collationName = culture.CompareInfo?.Name;
#if FEATURE_SPAN
                using var parser = new LocaleIDParser(stackalloc char[CharStackBufferSize], string.Empty);
#else
                using var parser = new LocaleIDParser(string.Empty);
#endif
                bool collationFound = false;

                if (!string.IsNullOrEmpty(collationName))
                {
                    // First check whether this is an alternate sort order
                    // https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo?view=netcore-3.1#alternate-sort-orders
                    foreach (var collation in CollationMapData)
                    {
                        if (collation[0].Equals(collationName, StringComparison.Ordinal))
                        {
                            collationFound = true;
                            parser.Reset(collation[1]);
                            if (collation[2] != null)
                                parser.SetKeywordValue(collation[2], collation[3]);
                            break;
                        }
                    }
                }

                // Convert from RFC 4646, ISO 639, ISO 639-2, and ISO 15924 to ICU format 

                // If collation wasn't found, use the culture name (base name) to get a new parser.
                // Map any unsupported names from .NET to their nearest equivalent in ICU.
                if (!collationFound)
                {
                    var name = UnsupportedDotNetNames.TryGetValue(culture.Name, out string baseName) ? baseName : culture.Name;
                    parser.Reset(name);
                }

                // ***********************************************************************************************************************************
                // * During Initialization - The default calendar is undefined, and implicitly loads the default calendar from the CLDR data.
                //
                // * Base Name Match Found in IcuNonGregorianDefaultCalendars - The default calendar is the value in IcuNonGregorianDefaultCalendars.
                //
                // * Base Name Match Not Found in IcuNonGregorianDefaultCalendars - The default calendar is "gregorian".
                //
                // Calendar is only specified in the locale string if it differs from the ICU default value.
                // ***********************************************************************************************************************************

                if (calendarsInitialized)
                {
                    // Calander formatting info
                    // The user may have set CultureInfo.DateTimeFormat.Calendar explicitly, in which
                    // case it will be different from the CultureInfo.Calendar property. We cover this
                    // by checking both cases.
                    var defaultCalendar = GetDefaultCalendar(parser.GetBaseName());
                    var calendarType = culture.Calendar.GetType();
                    var formattingCalendarType = culture.DateTimeFormat.Calendar.GetType();
                    if (DotNetCalendars.TryGetValue(calendarType, out string calendar) && !calendar.Equals(defaultCalendar, StringComparison.Ordinal))
                        parser.SetKeywordValue("calendar", calendar);
                    else if (DotNetCalendars.TryGetValue(formattingCalendarType, out string formattingCalendar) && !calendar.Equals(formattingCalendar, StringComparison.Ordinal))
                        parser.SetKeywordValue("calendar", formattingCalendar);
                }

                // ICU4N TODO: Need to append currency, number
                // but this isn't important until we provide string formatting support.

                return new UCultureInfo(parser.GetFullName(), culture);
            }

            public static CultureInfo ToCultureInfo(UCultureInfo uloc)
            {
                var baseName = uloc.Name;

                if (baseName.Equals("root", StringComparison.OrdinalIgnoreCase) || baseName.Equals("any", StringComparison.OrdinalIgnoreCase))
                {
                    return CultureInfo.InvariantCulture;
                }

                bool isCollationValue = false;
                uloc.Keywords.TryGetValue("collation", out string collationValue);
                foreach (var collation in CollationMapData)
                {
                    if (baseName.Equals(collation[1], StringComparison.Ordinal) || baseName.Equals(collation[4], StringComparison.Ordinal))
                    {
                        if (collation[2] == null || collationValue != null && collationValue.Equals(collation[3]))
                        {
                            baseName = collation[0];
                            isCollationValue = true;
                            break;
                        }
                    }
                }

                try
                {
                    // ICU4N TODO: Apply calendars, numbers, etc before returning
                    if (isCollationValue)
                        return new CultureInfo(baseName);
                    else
                        return new CultureInfo(ICUBaseNameToCultureInfoName(baseName));
                }
                catch (CultureNotFoundException)
                {
                    // Fallback to original base name(collation not supported)
                    //baseName = uloc.Name;
                }


                try
                {
                    // ICU4N TODO: Apply calendars, numbers, etc before returning
                    return new CultureInfo(ICUBaseNameToCultureInfoName(uloc.Name));
                }
                catch (CultureNotFoundException)
                {
                    // Fallback
                    return ToCultureInfo(uloc.GetParent() ?? UCultureInfo.InvariantCulture);
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
                // This method is called during static initialization recursively to load the list of calendars.
                // During this stage we don't care about the current culture's calendar, as we are loading the default directly
                // from resources. Once initialized, then we can move on to the real lookup.
                if (!calendarsInitialized)
                    return string.Empty; // Use the .NET calendar explicitly, whatever that may be

                // Use gregorian calendar if the name doesn't exist in the database
                return IcuNonGregorianDefaultCalendars.TryGetValue(baseName, out string result) ? result : "gregorian";
            }


            /// <summary>
            /// Gets the default ICU calendar for the <paramref name="baseName"/>.
            /// </summary>
            private static string GetDefaultCalendarFromBundle(string baseName)
            {
                var rb = ICUResourceBundle.GetBundleInstance(ICUData.IcuBundle, baseName, baseName, ICUResourceBundle.IcuDataAssembly, OpenType.LocaleDefaultRoot);
                return rb.GetStringWithFallback("calendar/default");
            }

            private static string ICUBaseNameToCultureInfoName(string baseName)
            {
#if FEATURE_SPAN
                using var parser = new LocaleIDParser(stackalloc char[CharStackBufferSize], baseName);
#else
                using var parser = new LocaleIDParser(baseName);
#endif
                return parser.GetName();
            }
        }
    }
}
