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
        /// <draft>ICU 60</draft>
        None,
        /// <summary>
        /// The capitalization context if a date, date symbol or display name is to be
        /// formatted with capitalization appropriate for the middle of a sentence.
        /// </summary>
        /// <draft>ICU 60</draft>
        MiddleOfSentence,
        /// <summary>
        /// The capitalization context if a date, date symbol or display name is to be
        /// formatted with capitalization appropriate for the beginning of a sentence.
        /// </summary>
        /// <draft>ICU 60</draft>
        BeginningOfSentence,
        /// <summary>
        /// The capitalization context if a date, date symbol or display name is to be
        /// formatted with capitalization appropriate for a user-interface list or menu item.
        /// </summary>
        /// <draft>ICU 60</draft>
        UIListOrMenu,
        /// <summary>
        /// The capitalization context if a date, date symbol or display name is to be
        /// formatted with capitalization appropriate for stand-alone usage such as an
        /// isolated name on a calendar page.
        /// </summary>
        /// <draft>ICU 60</draft>
        Standalone,
    }
}
