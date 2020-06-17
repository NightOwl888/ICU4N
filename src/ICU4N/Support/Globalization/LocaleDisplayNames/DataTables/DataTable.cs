// Port of text.LocaleDisplayNamesImpl.DataTable from ICU4J

using System.Globalization;

namespace ICU4N.Globalization
{
    internal class DataTable : IDataTable
    {
        protected readonly bool nullIfNotFound;

        public DataTable(bool nullIfNotFound)
        {
            this.nullIfNotFound = nullIfNotFound;
        }

        public virtual CultureInfo CultureInfo => CultureInfo.InvariantCulture;

        public virtual string Get(string tableName, string code)
        {
            return Get(tableName, null, code);
        }

        public virtual string Get(string tableName, string subTableName, string code)
        {
            return nullIfNotFound ? null : code;
        }
    }
}
