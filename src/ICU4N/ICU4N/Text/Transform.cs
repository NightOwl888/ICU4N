namespace ICU4N.Text
{
    /// <summary>
    /// Provide an interface for Transforms that focuses just on the transformation of the text.
    /// APIs that take Transliterator or <see cref="IStringTransform"/>, but only depend on the transformation should use this interface in the API instead.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    /// <author>markdavis</author>
    /// <stable>ICU 4.4</stable>
    public interface ITransform<TSource, TDestination>
    {
        /// <summary>
        /// Transform the input in some way, to be determined by the subclass.
        /// </summary>
        /// <param name="source">Source to be transformed (eg lowercased).</param>
        /// <returns>Result.</returns>
        /// <stable>ICU 4.4</stable>
        TDestination Transform(TSource source);
    }
}
