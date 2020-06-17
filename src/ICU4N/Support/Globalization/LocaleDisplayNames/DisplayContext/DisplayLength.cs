// Port of text.DisplayContext of ICU4J to DisplayContextOptions and enum properties

namespace ICU4N.Globalization
{
    /// <summary>
    /// Settings for display length of text formatting.
    /// </summary>
    /// <draft>ICU 60</draft>
    public enum DisplayLength
    {
        /// <summary>
        /// Use full names when generating a locale name,
        /// e.g. "United States" for US.
        /// This is the default behavior.
        /// </summary>
        /// <draft>ICU 60</draft>
        Full,
        /// <summary>
        /// Use short names when generating a locale name,
        /// e.g. "U.S." for US.
        /// </summary>
        /// <draft>ICU 60</draft>
        Short,
    }
}
