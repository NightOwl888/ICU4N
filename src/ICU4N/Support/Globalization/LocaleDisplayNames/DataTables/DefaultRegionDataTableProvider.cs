// Port of text.LocaleDisplayNamesImpl.DataTables from ICU4J
// In .NET we use an interface to determine which data we are loading (so they cannot be plugged in to the wrong place).

using System.Reflection;

namespace ICU4N.Globalization
{
    internal class DefaultRegionDataTableProvider : IRegionDataTableProvider
    {
        private readonly IRegionDataTableProvider defaultDataTableProvider = ICURegionDataTableProvider.Instance;

        public Assembly Assembly => defaultDataTableProvider.Assembly;

        public bool HasData => defaultDataTableProvider.HasData;

        public IDataTable GetDataTable(UCultureInfo culture, bool nullIfNotFound)
        {
            if (HasData)
                return defaultDataTableProvider.GetDataTable(culture, nullIfNotFound);

            return new DataTable(nullIfNotFound);
        }
    }
}
