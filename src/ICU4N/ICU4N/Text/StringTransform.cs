namespace ICU4N.Text
{
    /// <summary>
    /// Provide a base class for Transforms that focuses just on the transformation of the text. APIs that take Transliterator, but only 
    /// depend on the text transformation should use this interface in the API instead.
    /// </summary>
    /// <stable>ICU 3.8</stable>
    /// <author>markdavis</author>
    public interface IStringTransform : ITransform<string, string>
    {
        /// <summary>
        /// Transform the text in some way, to be determined by the subclass.
        /// </summary>
        /// <param name="source">Text to be transformed (eg lowercased).</param>
        /// <returns>Result.</returns>
        /// <stable>ICU 3.8</stable>
        //string Transform(string source);
    }
}
