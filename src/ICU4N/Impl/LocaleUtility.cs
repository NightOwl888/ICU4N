using ICU4N.Text;
using System;
using System.Globalization;
using System.Linq;

namespace ICU4N.Globalization // ICU4N: Moved from ICU4N.Impl namespace
{
    /// <summary>
    /// A class to hold utility functions missing from java.util.Locale.
    /// </summary>
     // ICU4N TODO: Move to Globalization namespace ?
    public class LocaleUtility // ICU4N TODO: Evaluate the need for this class, or whether UCultureInfo can serve as an all-inclusive culture object
    {
        private const int CharStackBufferSize = 32;

        /**
         * A helper function to convert a string of the form
         * aa_BB_CC to a locale object.  Why isn't this in Locale?
         */
        public static CultureInfo GetLocaleFromName(string name)
        {
            if (name.Equals("root", StringComparison.OrdinalIgnoreCase) || name.Equals("any", StringComparison.OrdinalIgnoreCase))
            {
                return CultureInfo.InvariantCulture;
            }

            // Strip off the config options
            int optionsIndex = name.IndexOf('@');
            if (optionsIndex > -1)
            {
                // ICU4N TODO: Need to convert calendar, currency, number, and collation options by
                // creating a custom CultureInfo subclass...where possible

                name = name.Substring(0, optionsIndex); // ICU4N: Checked 2nd parameter
            }

            string newName = name.Replace('_', '-').Trim('-');


            try
            {
                CultureInfo culture = new CultureInfo(newName);

#if FEATURE_CULTUREINFO_UNKNOWNLANGUAGE
                // ICU4N: In .NET Standard 1.x, some invalid cultures are allowed
                // to be created, but will be "unknown" languages. We need to manually
                // ignore these.
                if (culture.EnglishName.StartsWith("Unknown Language", StringComparison.Ordinal))
                {
                    return null;
                }
#endif
                return culture;
            }
            catch (CultureNotFoundException)
            {
                return null;
            }



            //// ICU4N TODO: Not sure what to do with "any". It is in the
            //// IANA subtag registry, but not supported in .NET.
            //if (name.Equals("any", StringComparison.OrdinalIgnoreCase))
            //{
            //    return CultureInfo.InvariantCulture;
            //}

            //return new CultureInfo(name);

            //string language = "";
            //string country = "";
            //string variant = "";

            //int i1 = name.indexOf('_');
            //if (i1 < 0)
            //{
            //    language = name;
            //}
            //else
            //{
            //    language = name.substring(0, i1);
            //    ++i1;
            //    int i2 = name.indexOf('_', i1);
            //    if (i2 < 0)
            //    {
            //        country = name.substring(i1);
            //    }
            //    else
            //    {
            //        country = name.substring(i1, i2);
            //        variant = name.substring(i2 + 1);
            //    }
            //}

            //return new Locale(language, country, variant);
        }

        /**
         * Compare two locale strings of the form aa_BB_CC, and
         * return true if parent is a 'strict' fallback of child, that is,
         * if child =~ "^parent(_.+)*" (roughly).
         */
        public static bool IsFallbackOf(string parent, string child)
        {
            if (!child.StartsWith(parent, StringComparison.Ordinal))
            {
                return false;
            }
            int i = parent.Length;
            return (i == child.Length ||
                    child[i] == '-');
        }

        /**
         * Compare two locales, and return true if the parent is a
         * 'strict' fallback of the child (parent string is a fallback
         * of child string).
         */
        public static bool IsFallbackOf(CultureInfo parent, CultureInfo child)
        {
            return IsFallbackOf(parent.ToString(), child.ToString());
        }


        //   /*
        //    * Convenience method that calls canonicalLocaleString(string) with
        //    * locale.toString();
        //    */
        //   /*public static string canonicalLocaleString(Locale locale) {
        //       return canonicalLocaleString(locale.toString());
        //   }*/

        //   /*
        //    * You'd think that Locale canonicalizes, since it munges the
        //    * renamed languages, but it doesn't quite.  It forces the region
        //    * to be upper case but doesn't do anything about the language or
        //    * variant.  Our canonical form is 'lower_UPPER_UPPER'.  
        //    */
        //   /*public static string canonicalLocaleString(string id) {
        //       if (id != null) {
        //           int x = id.indexOf('_');
        //           if (x == -1) {
        //               id = id.toLowerCase(Locale.ENGLISH);
        //           } else {
        //               StringBuffer buf = new StringBuffer();
        //               buf.append(id.substring(0, x).toLowerCase(Locale.ENGLISH));
        //               buf.append(id.substring(x).toUpperCase(Locale.ENGLISH));

        //               int len = buf.length();
        //               int n = len;
        //               while (--n >= 0 && buf.charAt(n) == '_') {
        //               }
        //               if (++n != len) {
        //                   buf.delete(n, len);
        //               }
        //               id = buf.toString();
        //           }
        //       }
        //       return id;
        //   }*/

        //        /**
        //         * Fallback from the given locale name by removing the rightmost _-delimited
        //         * element. If there is none, return the root locale ("", "", ""). If this
        //         * is the root locale, return null. NOTE: The string "root" is not
        //         * recognized; do not use it.
        //         * 
        //         * @return a new Locale that is a fallback from the given locale, or null.
        //         */
        //        public static CultureInfo Fallback(CultureInfo loc)
        //        {
        //            if (loc.Equals(CultureInfo.InvariantCulture))
        //            {
        //                return null;
        //            }

        //#if FEATURE_CULTUREINFO_UNKNOWNLANGUAGE
        //            // ICU4N: In .NET Standard 1.x, some invalid cultures are allowed
        //            // to be created, but will be "unknown" languages. We need to manually
        //            // ignore these.
        //            if (loc.EnglishName.StartsWith("Unknown Language", StringComparison.Ordinal))
        //            {
        //                return CultureInfo.InvariantCulture;
        //            }
        //#endif
        //            // ICU4N: We use the original ICU fallback scheme rather than
        //            // simply using loc.Parent.

        //            // Split the locale into parts and remove the rightmost part
        //            string[] parts = loc.Name.Split('-');
        //            if (parts.Length == 1)
        //            {
        //                return null; // All parts were empty
        //            }
        //            string culture = parts[0];
        //            for (int i = 1; i < parts.Length - 1; i++)
        //            {
        //                culture += '-' + parts[i];
        //            }
        //            return new CultureInfo(culture);
        //        }

#nullable enable

        /**
         * Fallback from the given locale name by removing the rightmost _-delimited
         * element. If there is none, return the root locale ("", "", ""). If this
         * is the root locale, return null. NOTE: The string "root" is not
         * recognized; do not use it.
         * 
         * @return a new Locale that is a fallback from the given locale, or null.
         */
        public static CultureInfo? Fallback(CultureInfo loc) // ICU4N TODO: API - remove
        {
            if (CultureInfo.InvariantCulture.Equals(loc))
                return null;

#if FEATURE_CULTUREINFO_UNKNOWNLANGUAGE
            // ICU4N: In .NET Standard 1.x, some invalid cultures are allowed
            // to be created, but will be "unknown" languages. We need to manually
            // ignore these.
            if (loc.EnglishName.StartsWith("Unknown Language", StringComparison.Ordinal))
            {
                return null;
            }
#endif
            // ICU4N: We use the original ICU fallback scheme rather than
            // simply using loc.Parent.
            string? fallbackLocaleID = FallbackAsString(loc.Name, separator: '-');
            return string.IsNullOrEmpty(fallbackLocaleID) ? null : new CultureInfo(fallbackLocaleID);
        }

        /// <summary>
        /// Fallback from the given locale name by removing the rightmost <c>_</c>-delimited or <c>-</c>-delimited
        /// element. If there is none, returns <see cref="UCultureInfo.InvariantCulture"/>. If <paramref name="loc"/>
        /// is <see cref="UCultureInfo.InvariantCulture"/>, returns <c>null</c>.
        /// <para/>
        /// <b>NOTE:</b> This differs from UCultureInfo.GetParentString() in that it skips the script tag.
        /// </summary>
        /// <param name="loc">The culture to fallback for.</param>
        /// <returns>A new <see cref="UCultureInfo"/> that is a fallback from <paramref name="loc"/>,
        /// <see cref="UCultureInfo.InvariantCulture"/> if <paramref name="loc"/> is a neutral culture,
        /// or <c>null</c> if <paramref name="loc"/> is the <see cref="UCultureInfo.InvariantCulture"/>.</returns>
        public static UCultureInfo? Fallback(UCultureInfo loc)
        {
            if (UCultureInfo.InvariantCulture.Equals(loc))
                return null;

#if FEATURE_CULTUREINFO_UNKNOWNLANGUAGE
            // ICU4N: In .NET Standard 1.x, some invalid cultures are allowed
            // to be created, but will be "unknown" languages. We need to manually
            // ignore these.
            if (loc.EnglishName.StartsWith("Unknown Language", StringComparison.Ordinal))
            {
                return null;
            }
#endif

            string? fallbackLocaleID = FallbackAsString(loc.Name);
            return string.IsNullOrEmpty(fallbackLocaleID) ? null : new UCultureInfo(fallbackLocaleID!);
        }


        private static string? FallbackAsString(string name, char separator = '_')
        {
            // ICU4N: Using LocaleIDParser for more accurate results
            using var parser = new LocaleIDParser(stackalloc char[CharStackBufferSize], name);
            int bufferLength = name.Length + 5;
            Span<char> result = bufferLength <= CharStackBufferSize ? stackalloc char[bufferLength] : new char[bufferLength];
            int totalLength = 0, lastLength = 0;

            parser.GetLanguage(result, out int languageLength);
            if (languageLength > 0)
            {
                totalLength += languageLength;
                lastLength = languageLength;
            }
            //parser.GetScript(result.Slice(totalLength + 1), out int scriptLength);
            //if (scriptLength > 0)
            //{
            //    result[totalLength] = separator;
            //    totalLength += scriptLength + 1;
            //    lastLength = scriptLength + 1;
            //}
            parser.GetCountry(result.Slice(totalLength + 1), out int countryLength);
            if (countryLength > 0)
            {
                result[totalLength] = separator;
                totalLength += countryLength + 1;
                lastLength = countryLength + 1;
            }
            parser.GetVariant(result.Slice(totalLength + 1), out int variantLength);
            if (variantLength > 0)
            {
                result[totalLength] = separator;
                totalLength += variantLength + 1;
                lastLength = variantLength + 1;
            }

            totalLength -= lastLength; // Remove the last segment
            if (totalLength == 0) return null;

            return result.Slice(0, totalLength).ToString();
        }

#nullable restore

        /// <summary>
        /// Fallback from the given ICU locale name 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static CultureInfo Fallback(string name) // ICU4N TODO: API - remove
        {
            //if (name.Equals("root", StringComparison.OrdinalIgnoreCase) || name.Equals("any", StringComparison.OrdinalIgnoreCase))
            //{
            //    return CultureInfo.InvariantCulture;
            //}

            //// Strip off the config options
            //int optionsIndex = name.IndexOf('@');
            //if (optionsIndex > -1)
            //{
            //    // ICU4N TODO: Need to convert calendar, currency, number, and collation options by
            //    // creating a custom CultureInfo subclass...where possible

            //    name = name.Substring(0, optionsIndex); // ICU4N: Checked 2nd parameter
            //}

            // There should be no more than 3...
            string[] segments = name.Split(new char[] { '_', '-' });

            CultureInfo culture = null;

            // Fallback to more general culture...
            for (int i = Math.Max(3, segments.Length); i > 0; i--)
            {
                try
                {
                    string newName = string.Join("-", segments.Take(i));
                    culture = new CultureInfo(newName);

#if FEATURE_CULTUREINFO_UNKNOWNLANGUAGE
                    // ICU4N: In .NET Standard 1.x, some invalid cultures are allowed
                    // to be created, but will be "unknown" languages. We need to manually
                    // ignore these.
                    if (culture.EnglishName.StartsWith("Unknown Language", StringComparison.Ordinal))
                    {
                        continue;
                    }
#endif
                    break;
                }
                catch (CultureNotFoundException)
                {
                    continue;
                }
            }

            //if (culture == null)
            //{
            //    // Hopefully we don't get here...the only logical fallback is InvariantCulture
            //    culture = CultureInfo.InvariantCulture;
            //}

            return culture;
        }
    }
}
