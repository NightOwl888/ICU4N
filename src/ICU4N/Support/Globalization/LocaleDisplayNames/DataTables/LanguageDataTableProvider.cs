using ICU4N.Impl;
using System;
using System.Globalization;
using System.Reflection;

namespace ICU4N.Globalization
{
    /// <summary>
    /// Provides a base class for a language data provider, which can be used to provide
    /// custom distributions of language data with ICU4N.
    /// <para/>
    /// The data must reside in an embedded resource in a folder named <c>Impl\Data\lang\</c>
    /// within the assembly, so it is resolved as <c>[assembly name].Impl.Data.lang.[localeID].res</c>.
    /// </summary>
    /// <draft>ICU 60</draft>
    public abstract class LanguageDataTableProvider : ILanguageDataTableProvider
    {
        private readonly Assembly assembly;

        /// <summary>
        /// Initializes an instance of <see cref="LanguageDataTableProvider"/> with the <see cref="Assembly"/>
        /// where the resources reside.
        /// </summary>
        /// <param name="assembly">The assembly where the resource files can be located.</param>
        /// <draft>ICU 60</draft>
        protected LanguageDataTableProvider(Assembly assembly)
        {
            this.assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        /// <summary>
        /// Indicates whether data is available from this provider.
        /// If this returns <c>false</c>, a fallback provider will be used.
        /// </summary>
        /// <draft>ICU 60</draft>
        public virtual bool HasData => true;

        /// <summary>
        /// Gets the table data from the provider.
        /// </summary>
        /// <param name="culture">The culture of the table data to retrieve.</param>
        /// <param name="nullIfNotFound">If <c>true</c>, this method returns <c>null</c>
        /// when the data is not available in the provider. If <c>false</c>, the code
        /// that is passed in will be returned.</param>
        /// <returns>The table data.</returns>
        /// <draft>ICU 60</draft>
        public virtual IDataTable GetDataTable(CultureInfo culture, bool nullIfNotFound)
        {
            return new ICUDataTable(ICUData.IcuLanguageBaseName, culture, assembly, nullIfNotFound);
        }
    }
}
