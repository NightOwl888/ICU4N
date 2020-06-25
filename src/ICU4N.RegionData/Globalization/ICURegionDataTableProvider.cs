using System.Reflection;

namespace ICU4N.Globalization
{
    /// <summary>
    /// The data table provider for region/country data that is read from embedded resources
    /// inside of this assembly.
    /// <para/>
    /// The data must reside in an embedded resource in a folder named <c>Impl\Data\lang\</c>
    /// within the assembly, so it is resolved as <c>[assembly name].Impl.Data.lang.[localeID].res</c>.
    /// </summary>
    public class ICURegionDataTableProvider : RegionDataTableProvider
    {
#if FEATURE_TYPEEXTENSIONS_GETTYPEINFO
        public ICURegionDataTableProvider()

            : base(typeof(ICURegionDataTableProvider).GetTypeInfo().Assembly)
#else
        private ICURegionDataTableProvider()
            : base(typeof(ICURegionDataTableProvider).Assembly)
#endif
        { }

        /// <summary>
        /// The singleton instance of the region/country data table provider.
        /// </summary>
        public static ICURegionDataTableProvider Instance => new ICURegionDataTableProvider();
    }
}
