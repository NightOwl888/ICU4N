// Port of text.LocaleDisplayNamesImpl.ICUDataTable from ICU4J

using ICU4N.Impl;
using ICU4N.Util;
using System.Globalization;
using System.Reflection;

namespace ICU4N.Globalization
{
    internal class ICUDataTable : DataTable
    {
        private readonly ICUResourceBundle bundle;

        public ICUDataTable(string path, UCultureInfo culture, Assembly assembly, bool nullIfNotFound)
            : base(nullIfNotFound)
        {
            this.bundle = (ICUResourceBundle)UResourceBundle.GetBundleInstance(
                    path, culture.Name, assembly);
        }

        public override CultureInfo CultureInfo => bundle.Culture;

        public override string Get(string tableName, string subTableName, string code)
        {
            return ICUResourceTableAccess.GetTableString(bundle, tableName, subTableName,
                code, nullIfNotFound ? null : code);
        }
    }
}
