using ICU4N.Impl.Locale;
using ICU4N.Support.Collections;
using System;
using System.Collections.Generic;

namespace ICU4N.Globalization
{
    internal static partial class ResourceUtil
    {
        // Special cases for locales that .NET either doesn't recognize, or has remapped (probably for legacy reasons with NLS)
        private static readonly Dictionary<string, string> NeutralCultureSubstitutions = new Dictionary<string, string>(AsciiStringComparer.Ordinal)
        {
            // ICU baseLocale       .NET culture
            ["qu"]                  = "quz",
            ["yue"]                 = "zh",
            ["yue-Hans"]            = "zh-Hans",
            ["yue-Hant"]            = "zh-Hant",
        };

        public static string GetDotNetNeutralCultureName(ReadOnlySpan<char> icuBaseName)
            => GetDotNetNeutralCultureName(icuBaseName, NeutralCultureSubstitutions);

        public static string GetDotNetNeutralCultureName(ReadOnlySpan<char> icuBaseName, Dictionary<string, string> substitutions)
        {
            if (icuBaseName.IsEmpty) return string.Empty;
            if (icuBaseName.Equals("root", StringComparison.Ordinal)) return string.Empty;
            if (icuBaseName.Equals("any", StringComparison.Ordinal)) return string.Empty;

            using var parser = new LocaleIDParser(stackalloc char[32], icuBaseName);
            ReadOnlySpan<char> neutralCulture = parser.GetNeutralCultureAsSpan(separator: '-');

            if (substitutions.TryGetValue(neutralCulture, out string substitutionValue))
            {
                return substitutionValue;
            }
            return neutralCulture.ToString();
        }
    }
}
