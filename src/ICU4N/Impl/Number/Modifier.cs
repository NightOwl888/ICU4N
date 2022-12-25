using ICU4N.Text;

namespace ICU4N.Numerics
{
    /// <summary>
    /// An <see cref="IModifier"/> is an object that can be passed through the formatting pipeline until it is finally applied to the string
    /// builder. An <see cref="IModifier"/> usually contains a prefix and a suffix that are applied, but it could contain something else,
    /// like a <see cref="SimpleFormatter"/> pattern.
    /// <para/>
    /// A <see cref="IModifier"/> is usually immutable, except in cases such as <see cref="MutablePatternModifier"/>, which are mutable for performance
    /// reasons.
    /// </summary>
    internal interface IModifier // ICU4N TODO: API - this was public in ICU4J
    {
        /// <summary>
        /// Apply this <see cref="IModifier"/> to the string builder.
        /// </summary>
        /// <param name="output">The string builder to which to apply this modifier.</param>
        /// <param name="leftIndex">The left index of the string within the builder. Equal to 0 when only one number is being formatted.</param>
        /// <param name="rightIndex">The right index of the string within the string builder. Equal to length when only one number is being
        /// formatted.</param>
        /// <returns>The number of characters (UTF-16 code units) that were added to the string builder.</returns>
        public int Apply(NumberStringBuilder output, int leftIndex, int rightIndex);

        /// <summary>
        /// Gets the length of the prefix. This information can be used in combination with <see cref="Apply(NumberStringBuilder, int, int)"/>
        /// to extract the prefix and suffix strings. Returns the number of characters (UTF-16 code units) in the prefix.
        /// </summary>
        public int PrefixLength { get; }

        /// <summary>
        /// Gets the number of code points in the modifier, prefix plus suffix.
        /// </summary>
        public int CodePointCount { get; }

        /// <summary>
        /// Gets whether this modifier is strong. If a modifier is strong, it should always be applied immediately and not allowed
        /// to bubble up. With regard to padding, strong modifiers are considered to be on the inside of the prefix and
        /// suffix.
        /// </summary>
        public bool IsStrong { get; }
    }
}
