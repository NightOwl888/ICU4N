using System.Globalization;
using System.Reflection;

namespace ICU4N.Globalization
{
    /// <summary>
    /// Provides a contract supplying region/country data from an external source.
    /// The data must reside in an embedded resource in a folder named <c>Impl\Data\region\</c>
    /// within the assembly, so it is resolved as <c>[assembly name].Impl.Data.region.[localeID].res</c>.
    /// </summary>
    /// <draft>ICU 60</draft>
    public interface IRegionDataTableProvider
    {
        /// <summary>
        /// The assembly where the resource files can be located.
        /// </summary>
        /// <draft>ICU 60</draft>
        Assembly Assembly { get; }

        /// <summary>
        /// Indicates whether data is available from this provider.
        /// If this returns <c>false</c>, a fallback provider will be used.
        /// </summary>
        /// <draft>ICU 60</draft>
        bool HasData { get; }

        /// <summary>
        /// Gets the table data from the provider.
        /// </summary>
        /// <param name="culture">The culture of the table data to retrieve.</param>
        /// <param name="nullIfNotFound">If <c>true</c>, this method returns <c>null</c>
        /// when the data is not available in the provider. If <c>false</c>, the code
        /// that is passed in will be returned.</param>
        /// <returns>The table data.</returns>
        /// <draft>ICU 60</draft>
        IDataTable GetDataTable(CultureInfo culture, bool nullIfNotFound);
    }
}
