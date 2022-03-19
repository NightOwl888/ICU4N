namespace ICU4N.Support.Globalization
{
    internal static class ResourceUtil
    {
        /// <summary>
        /// Change from JDK-style resource path (/data/icudt60b/brkitr) to .NET style (data.brkitr).
        /// This method also removes the version number from the path so we don't have to change it internally
        /// from one release to the next.
        /// </summary>
        public static string ConvertResourceName(string name)
        {
            return name.Replace('/', '.').Replace("." + ICU4N.Impl.ICUData.PackageName, "");
        }
    }
}
