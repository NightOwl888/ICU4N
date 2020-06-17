using ICU4N.Impl;
using System;

namespace ICU4N.Globalization
{
    internal class DefaultCultureDisplayNamesFactory : ICultureDisplayNamesFactory
    {
        private static readonly CacheBase<Tuple<UCultureInfo, DisplayContextOptions>, CultureDisplayNames> cache
            = new SoftCache<Tuple<UCultureInfo, DisplayContextOptions>, CultureDisplayNames>();

        public CultureDisplayNames GetCultureDisplayNames(UCultureInfo uCulture, DisplayContextOptions options)
        {
            return cache.GetOrCreate(new Tuple<UCultureInfo, DisplayContextOptions>(uCulture, options), (key) =>
            {
                return new DataTableCultureDisplayNames(key.Item1, key.Item2);
            });
        }
    }
}
