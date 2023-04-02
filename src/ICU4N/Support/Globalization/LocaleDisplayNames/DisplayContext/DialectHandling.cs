// Port of text.DisplayContext of ICU4J to DisplayContextOptions and enum properties

namespace ICU4N.Globalization
{
    // ICU4N TODO: update docs below from en_GB to en-GB ?
    /// <summary>
    /// Settings for dialect handling of display names.
    /// </summary>
    /// <draft>ICU 60</draft>
    public enum DialectHandling
    {
        /// <summary>
        /// Use standard names when generating a locale name,
        /// e.g. en_GB displays as 'English (United Kingdom)'.
        /// This is the default value.
        /// </summary>
        /// <draft>ICU 60</draft>
        StandardNames = 0,
        /// <summary>
        /// Use dialect names, when generating a locale name,
        /// e.g. en_GB displays as 'British English'.
        /// </summary>
        /// <draft>ICU 60</draft>
        DialectNames = 1,
    }
}
