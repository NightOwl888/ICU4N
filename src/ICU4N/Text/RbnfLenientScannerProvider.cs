using ICU4N.Globalization;
using System;

namespace ICU4N.Text
{
    /// <summary>
    /// A provider for an <see cref="IRbnfLenientScanner"/>.
    /// </summary>
    [Obsolete("ICU 54")]
    internal interface IRbnfLenientScannerProvider
    {
        /// <summary>
        /// Returns a scanner appropriate for the given locale, with optional extra data
        /// in the form of collation rules.
        /// </summary>
        /// <param name="locale">The locale to provide the default lenient rules.</param>
        /// <param name="extras">Extra collation rules.</param>
        /// <returns>The lenient scanner, or <c>null</c>.</returns>
        [Obsolete("ICU 54")] 
        IRbnfLenientScanner Get(UCultureInfo locale, string extras);
    }
}
