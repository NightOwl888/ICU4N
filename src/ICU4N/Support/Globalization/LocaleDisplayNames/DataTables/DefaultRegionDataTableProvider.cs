﻿// Port of text.LocaleDisplayNamesImpl.DataTables from ICU4J
// In .NET we use an interface to determine which data we are loading (so they cannot be plugged in to the wrong place).

using ICU4N.Impl;
using System;
using System.Globalization;
using System.Reflection;

namespace ICU4N.Globalization
{
    internal class DefaultRegionDataTableProvider : IRegionDataTableProvider
    {
        private const string InstancePropertyMethodName = "get_Instance";
        private readonly IRegionDataTableProvider defaultDataTableProvider = Load(DataTableCultureDisplayNames.DefaultRegionDataTableProvider);

        public Assembly Assembly => defaultDataTableProvider != null
            ? defaultDataTableProvider.Assembly
            : ICUResourceBundle.IcuDataAssembly;

        public bool HasData => defaultDataTableProvider != null && defaultDataTableProvider.HasData;

        public IDataTable GetDataTable(UCultureInfo culture, bool nullIfNotFound)
        {
            if (HasData)
                return defaultDataTableProvider.GetDataTable(culture, nullIfNotFound);

            return new DataTable(nullIfNotFound);
        }

        private static IRegionDataTableProvider Load(string className)
        {
            try
            {
                Type type = Type.GetType(className);
                if (type == null)
                    return null;

#if FEATURE_TYPEEXTENSIONS_GETTYPEINFO
                return (IRegionDataTableProvider)Activator.CreateInstance(type);
#else
                return (IRegionDataTableProvider)type.GetMethod(InstancePropertyMethodName).Invoke(null, null);
#endif
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
