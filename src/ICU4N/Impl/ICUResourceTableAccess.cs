using ICU4N.Globalization;
using ICU4N.Util;
using System;

namespace ICU4N.Impl
{
    /// <summary>
    /// Static utility functions for probing resource tables, used by <see cref="UCultureInfo"/> and
    /// <see cref="CultureDisplayNames"/>.
    /// </summary>
    public static class ICUResourceTableAccess
    {
        /// <summary>
        /// Utility to fetch locale display data from resource bundle tables.  Convenience
        /// wrapper for <see cref="GetTableString(ICUResourceBundle, string, string, string, string)"/>.
        /// </summary>
        public static string GetTableString(string path, UCultureInfo locale, string tableName,
                string itemName, string defaultValue)
        {
            ICUResourceBundle bundle = (ICUResourceBundle)UResourceBundle.
                GetBundleInstance(path, locale.Name);
            return GetTableString(bundle, tableName, null, itemName, defaultValue);
        }

        /// <summary>
        /// Utility to fetch locale display data from resource bundle tables.  Uses fallback
        /// through the "Fallback" resource if available.
        /// </summary>
        public static string GetTableString(ICUResourceBundle bundle, string tableName,
            string subtableName, string item, string defaultValue)
        {
            string result = null;
            try
            {
                for (; ; )
                {
                    ICUResourceBundle table = bundle.FindWithFallback(tableName);
                    if (table == null)
                    {
                        return defaultValue;
                    }
                    ICUResourceBundle stable = table;
                    if (subtableName != null)
                    {
                        stable = table.FindWithFallback(subtableName);
                    }
                    if (stable != null)
                    {
                        result = stable.FindStringWithFallback(item);
                        if (result != null)
                        {
                            break; // possible real exception
                        }
                    }

                    // if we get here, stable was null, or there was no string for the item
                    if (subtableName == null)
                    {
                        // may be a deprecated code
                        string currentName = null;
                        if (tableName.Equals("Countries"))
                        {
                            currentName = LocaleIDs.GetCurrentCountryID(item);
                        }
                        else if (tableName.Equals("Languages"))
                        {
                            currentName = LocaleIDs.GetCurrentLanguageID(item);
                        }
                        if (currentName != null)
                        {
                            result = table.FindStringWithFallback(currentName);
                            if (result != null)
                            {
                                break; // possible real exception
                            }
                        }
                    }

                    // still can't figure it out? try the fallback mechanism
                    string fallbackLocale = table.FindStringWithFallback("Fallback"); // again, possible exception
                    if (fallbackLocale == null)
                    {
                        return defaultValue;
                    }

                    if (fallbackLocale.Length == 0)
                    {
                        fallbackLocale = "root";
                    }

                    if (fallbackLocale.Equals(table.UCulture.FullName))
                    {
                        return defaultValue;
                    }

                    bundle = (ICUResourceBundle)UResourceBundle.GetBundleInstance(
                            bundle.GetBaseName(), fallbackLocale);
                }
            }
            catch (Exception)
            {
                // If something is seriously wrong, we might call getString on a resource that is
                // not a string.  That will throw an exception, which we catch and ignore here.
            }

            // If the result is empty return item instead
            return ((result != null && result.Length > 0) ? result : defaultValue);
        }
    }
}
