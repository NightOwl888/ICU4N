// Port of text.DisplayContext of ICU4J to DisplayContextOptions and enum properties

namespace ICU4N.Globalization
{
    /// <summary>
    /// Settings for capitalization of a date, date symbol, or display name based on context.
    /// </summary>
    public enum Capitalization
    {
        /// <summary>
        /// The capitalization context to be used is unknown (this is the default value).
        /// </summary>
        /// <draft>ICU 60.1</draft>
        None = 0,
        /// <summary>
        /// The capitalization context if a date, date symbol or display name is to be
        /// formatted with capitalization appropriate for the middle of a sentence.
        /// </summary>
        /// <draft>ICU 60.1</draft>
        ForMiddleOfSentence = 1,
        /// <summary>
        /// The capitalization context if a date, date symbol or display name is to be
        /// formatted with capitalization appropriate for the beginning of a sentence.
        /// </summary>
        /// <draft>ICU 60.1</draft>
        ForBeginningOfSentence = 2,
        /// <summary>
        /// The capitalization context if a date, date symbol or display name is to be
        /// formatted with capitalization appropriate for a user-interface list or menu item.
        /// </summary>
        /// <draft>ICU 60.1</draft>
        ForUIListOrMenu = 3,
        /// <summary>
        /// The capitalization context if a date, date symbol or display name is to be
        /// formatted with capitalization appropriate for stand-alone usage such as an
        /// isolated name on a calendar page.
        /// </summary>
        /// <draft>ICU 60.1</draft>
        ForStandalone = 4,
    }

    /// <summary>
    /// Extensions for <see cref="Capitalization"/>.
    /// </summary>
    internal static class CapitalizationExtensions
    {
        /// <summary>
        /// Returns a boolean telling whether a given integral value, or its name as a string,
        /// exists in the <see cref="Capitalization"/> enumeration.
        /// </summary>
        /// <param name="capitalization">This <see cref="Capitalization"/>.</param>
        /// <returns><c>true</c> if a given integral value, or its name as a string, exists in
        /// a specified enumeration; <c>false</c> otherwise.</returns>
        internal static bool IsDefined(this Capitalization capitalization)
            => capitalization >= Capitalization.None && capitalization <= Capitalization.ForStandalone;
    }
}
