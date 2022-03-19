namespace ICU4N.Resources
{
    /// <summary>
    /// The location where the resource files can be found in an assembly.
    /// </summary>
    public enum ResourceLocation
    {
        /// <summary>
        /// The resources are in the main assembly. The files should be placed in a directory named
        /// <c>/data[/&lt;feature&gt;]</c> so they are resolved as
        /// <c>&lt;assembly name&gt;.data[.&lt;feature&gt;].&lt;file name&gt;.&lt;extension&gt;</c>.
        /// </summary>
        MainAssembly = 0,

        /// <summary>
        /// The resources are in a satellite assembly. The embedded files should be named
        /// <c>data[.&lt;feature&gt;].&lt;file name&gt;.&lt;extension&gt;</c>.
        /// </summary>
        Satellite = 1
    }
}
