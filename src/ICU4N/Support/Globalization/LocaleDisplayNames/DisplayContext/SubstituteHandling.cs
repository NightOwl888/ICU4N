// Port of text.DisplayContext of ICU4J to DisplayContextOptions and enum properties

namespace ICU4N.Globalization
{
    /// <summary>
    /// Settings for fallback substitute handling of localized text
    /// when the underlying data set does not contain the value.
    /// </summary>
    /// <draft>ICU 60</draft>
    public enum SubstituteHandling
    {
        /// <summary>
        /// Returns a fallback value (e.g., the input code) when no data is available.
        /// This is the default behavior.
        /// </summary>
        /// <draft>ICU 60</draft>
        Substitute,
        /// <summary>
        /// Returns a null value when no data is available.
        /// </summary>
        /// <draft>ICU 60</draft>
        NoSubstitute
    }
}
