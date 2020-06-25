namespace ICU4N.Globalization
{
    /// <summary>
    /// Provides culture display name data to ICU.
    /// </summary>
    public interface ICultureDisplayNamesFactory
    {
        /// <summary>
        /// Gets the culture display names for a specific culture with the provided <paramref name="options"/>.
        /// </summary>
        /// <param name="culture">The <see cref="UCultureInfo"/> to get the data for.</param>
        /// <param name="options">A set of <see cref="DisplayContextOptions"/> to control the display name formatting.</param>
        /// <returns>A <see cref="CultureDisplayNames"/> instance that provides display names for the specified
        /// <paramref name="culture"/> and <paramref name="options"/>.</returns>
        CultureDisplayNames GetCultureDisplayNames(UCultureInfo culture, DisplayContextOptions options);
    }
}
