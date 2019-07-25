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
        // ICU4N specific - we don't provide a Transform method definition because
        // our inherited interface does that already.
    }
}
