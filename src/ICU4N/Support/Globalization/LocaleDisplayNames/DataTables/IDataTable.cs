using System.Globalization;

namespace ICU4N.Globalization
{
    /// <summary>
    /// Provides a contract for supplying ICU data from an external source.
    /// </summary>
    /// <draft>ICU 60</draft>
    public interface IDataTable
    {
        /// <summary>
        /// The culture of the current data set.
        /// </summary>
        /// <draft>ICU 60</draft>
        CultureInfo CultureInfo { get; }

        /// <summary>
        /// Gets a data table from the current set with the supplied
        /// <paramref name="tableName"/> and <paramref name="code"/>.
        /// </summary>
        /// <draft>ICU 60</draft>
        string Get(string tableName, string code);

        /// <summary>
        /// Gets a data table from the current set with the supplied
        /// <paramref name="tableName"/>, <paramref name="subTableName"/> and <paramref name="code"/>.
        /// </summary>
        /// <draft>ICU 60</draft>
        string Get(string tableName, string subTableName, string code);
    }
}
