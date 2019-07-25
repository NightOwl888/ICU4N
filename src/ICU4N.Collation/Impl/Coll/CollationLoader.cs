using ICU4N.Support.IO;
using ICU4N.Util;
using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;

namespace ICU4N.Impl.Coll
{
    /// <summary>
    /// Convenience string denoting the Collation data tree
    /// </summary>
    public static class CollationLoader
    {
        // ICU4N specific - just marking class static will suffice
        //// not implemented, all methods are static
        //private CollationLoader()
        //{
        //}

        private static volatile string rootRules = null;

        private static void LoadRootRules()
        {
            if (rootRules != null)
            {
                return;
            }
            lock (typeof(CollationLoader))
            {
                if (rootRules == null)
                {
                    UResourceBundle rootBundle = UResourceBundle.GetBundleInstance(
                        // ICU4N specific - passing in the current assembly to load resources from.
                        ICUData.ICU_COLLATION_BASE_NAME, ULocale.ROOT, CollationData.ICU_DATA_CLASS_LOADER);
                    rootRules = rootBundle.GetString("UCARules");
                }
            }
        }

        // C++: static void appendRootRules(UnicodeString &s)
        public static string RootRules
        {
            get
            {
                LoadRootRules();
                return rootRules;
            }
        }

        /**
         * Simpler/faster methods for ASCII than ones based on Unicode data.
         * TODO: There should be code like this somewhere already??
         */
        private sealed class ASCII
        {
            internal static string ToLower(string s)
            {
                for (int i = 0; i < s.Length; ++i)
                {
                    char c = s[i];
                    if ('A' <= c && c <= 'Z')
                    {
                        StringBuilder sb = new StringBuilder(s.Length);
                        sb.Append(s, 0, i - 0).Append((char)(c + 0x20));// ICU4N: Checked 3rd parameter
                        while (++i < s.Length)
                        {
                            c = s[i];
                            if ('A' <= c && c <= 'Z') { c = (char)(c + 0x20); }
                            sb.Append(c);
                        }
                        return sb.ToString();
                    }
                }
                return s;
            }
        }

        internal static string LoadRules(ULocale locale, string collationType)
        {
            UResourceBundle bundle = UResourceBundle.GetBundleInstance(
                    ICUData.ICU_COLLATION_BASE_NAME, locale);
            UResourceBundle data = ((ICUResourceBundle)bundle).GetWithFallback(
                    "collations/" + ASCII.ToLower(collationType));
            string rules = data.GetString("Sequence");
            return rules;
        }

        private static UResourceBundle FindWithFallback(UResourceBundle table, String entryName)
        {
            return ((ICUResourceBundle)table).FindWithFallback(entryName);
        }

        public static CollationTailoring LoadTailoring(ULocale locale, out ULocale outValidLocale)
        {

            // Java porting note: ICU4J getWithFallback/getStringWithFallback currently does not
            // work well when alias table is involved in a resource path, unless full path is specified.
            // For now, collation resources does not contain such data, so the code below should work fine.

            CollationTailoring root = CollationRoot.Root;
            string localeName = locale.GetName();
            if (localeName.Length == 0 || localeName.Equals("root"))
            {
                outValidLocale = ULocale.ROOT;
                return root;
            }

            UResourceBundle bundle = null;
            try
            {
                bundle = ICUResourceBundle.GetBundleInstance(
                        ICUData.ICU_COLLATION_BASE_NAME, locale,
                        // ICU4N specific - need to pass in this assembly
                        // name for the resources to be resolved here.
                        CollationData.ICU_DATA_CLASS_LOADER, 
                        ICUResourceBundle.OpenType.LOCALE_ROOT);
            }
            catch (MissingManifestResourceException)
            {
                outValidLocale = ULocale.ROOT;
                return root;
            }

            ULocale validLocale = bundle.GetULocale();
            // Normalize the root locale. See
            // http://bugs.icu-project.org/trac/ticket/10715
            string validLocaleName = validLocale.GetName();
            if (validLocaleName.Length == 0 || validLocaleName.Equals("root"))
            {
                validLocale = ULocale.ROOT;
            }
            outValidLocale = validLocale;

            // There are zero or more tailorings in the collations table.
            UResourceBundle collations;
            try
            {
                collations = bundle.Get("collations");
                if (collations == null)
                {
                    return root;
                }
            }
            catch (MissingManifestResourceException)
            {
                return root;
            }

            // Fetch the collation type from the locale ID and the default type from the data.
            string type = locale.GetKeywordValue("collation");
            string defaultType = "standard";

            string defT = ((ICUResourceBundle)collations).FindStringWithFallback("default");
            if (defT != null)
            {
                defaultType = defT;
            }

            if (type == null || type.Equals("default"))
            {
                type = defaultType;
            }
            else
            {
                type = ASCII.ToLower(type);
            }

            // Load the collations/type tailoring, with type fallback.

            // Java porting note: typeFallback is used for setting U_USING_DEFAULT_WARNING in
            // ICU4C, but not used by ICU4J

            // boolean typeFallback = false;
            UResourceBundle data = FindWithFallback(collations, type);
            if (data == null &&
                    type.Length > 6 && type.StartsWith("search", StringComparison.Ordinal))
            {
                // fall back from something like "searchjl" to "search"
                // typeFallback = true;
                type = "search";
                data = FindWithFallback(collations, type);
            }

            if (data == null && !type.Equals(defaultType))
            {
                // fall back to the default type
                // typeFallback = true;
                type = defaultType;
                data = FindWithFallback(collations, type);
            }

            if (data == null && !type.Equals("standard"))
            {
                // fall back to the "standard" type
                // typeFallback = true;
                type = "standard";
                data = FindWithFallback(collations, type);
            }

            if (data == null)
            {
                return root;
            }

            // Is this the same as the root collator? If so, then use that instead.
            ULocale actualLocale = data.GetULocale();
            // http://bugs.icu-project.org/trac/ticket/10715 ICUResourceBundle(root).getULocale() != ULocale.ROOT
            // Therefore not just if (actualLocale.equals(ULocale.ROOT) && type.equals("standard")) {
            string actualLocaleName = actualLocale.GetName();
            if (actualLocaleName.Length == 0 || actualLocaleName.Equals("root"))
            {
                actualLocale = ULocale.ROOT;
                if (type.Equals("standard"))
                {
                    return root;
                }
            }

            CollationTailoring t = new CollationTailoring(root.Settings);
            t.ActualLocale = actualLocale;

            // deserialize
            UResourceBundle binary = data.Get("%%CollationBin");
            ByteBuffer inBytes = binary.GetBinary();
            try
            {
                CollationDataReader.Read(root, inBytes, t);
            }
            catch (IOException e)
            {
                throw new ICUUncheckedIOException("Failed to load collation tailoring data for locale:"
                        + actualLocale + " type:" + type, e);
            }

            // Try to fetch the optional rules string.
            try
            {
                t.SetRulesResource(data.Get("Sequence"));
            }
            catch (MissingManifestResourceException)
            {
            }

            // Set the collation types on the informational locales,
            // except when they match the default types (for brevity and backwards compatibility).
            // For the valid locale, suppress the default type.
            if (!type.Equals(defaultType))
            {
                outValidLocale = validLocale.SetKeywordValue("collation", type);
            }

            // For the actual locale, suppress the default type *according to the actual locale*.
            // For example, zh has default=pinyin and contains all of the Chinese tailorings.
            // zh_Hant has default=stroke but has no other data.
            // For the valid locale "zh_Hant" we need to suppress stroke.
            // For the actual locale "zh" we need to suppress pinyin instead.
            if (!actualLocale.Equals(validLocale))
            {
                // Opening a bundle for the actual locale should always succeed.
                UResourceBundle actualBundle = UResourceBundle.GetBundleInstance(
                        ICUData.ICU_COLLATION_BASE_NAME, actualLocale);
                defT = ((ICUResourceBundle)actualBundle).FindStringWithFallback("collations/default");
                if (defT != null)
                {
                    defaultType = defT;
                }
            }

            if (!type.Equals(defaultType))
            {
                t.ActualLocale = t.ActualLocale.SetKeywordValue("collation", type);
            }

            // if (typeFallback) {
            //     ICU4C implementation sets U_USING_DEFAULT_WARNING here
            // }

            return t;
        }
    }
}
