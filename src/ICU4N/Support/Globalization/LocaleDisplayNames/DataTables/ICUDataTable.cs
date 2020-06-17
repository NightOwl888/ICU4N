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

        public ICUDataTable(string path, CultureInfo culture, Assembly assembly, bool nullIfNotFound)
            : base(nullIfNotFound)
        {
            this.bundle = (ICUResourceBundle)UResourceBundle.GetBundleInstance(
                    path, culture.ToUCultureInfo().Name, assembly);
        }

        public override CultureInfo CultureInfo => bundle.GetLocale(); // ICU4N TODO: Rename GetCulture

        public override string Get(string tableName, string subTableName, string code)
        {
            return ICUResourceTableAccess.GetTableString(bundle, tableName, subTableName,
                code, nullIfNotFound ? null : code);
        }
    }
}
