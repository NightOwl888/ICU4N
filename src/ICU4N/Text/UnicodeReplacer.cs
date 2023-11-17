namespace ICU4N.Text
{
    /// <summary>
    /// <see cref="IUnicodeReplacer"/> defines a protocol for objects that
    /// replace a range of characters in a <see cref="IReplaceable"/> string with output
    /// text.  The replacement is done via the <see cref="IReplaceable"/> API so as to
    /// preserve out-of-band data.
    /// </summary>
    /// <author>Alan Liu</author>
    internal interface IUnicodeReplacer
    {
        /// <summary>
        /// Replace characters in '<paramref name="text"/>' from '<paramref name="start"/>' to '<paramref name="limit"/>' with the
        /// output text of this object.  Return the '<paramref name="cursor"/>' parameter to
        /// give the cursor position and return the length of the
        /// replacement text.
        /// </summary>
        /// <param name="text">The text to be matched.</param>
        /// <param name="start">Inclusive start index of <paramref name="text"/> to be replaced.</param>
        /// <param name="limit">Exclusive end index of <paramref name="text"/> to be replaced;
        /// must be greater than or equal to start.</param>
        /// <param name="cursor">Output parameter for the cursor position.
        /// Not all replacer objects will provide this, but in a complete
        /// tree of replacer objects, representing the entire output side
        /// of a transliteration rule, at least one must return it.
        /// </param>
        /// <returns>The number of 16-bit code units in the text replacing
        /// the characters at offsets start..(limit-1) in text.</returns>
        int Replace(IReplaceable text,
                                    int start,
                                    int limit,
                                    out int cursor); // ICU4N: Changed cursor from int[] to out int

        /// <summary>
        /// Returns a string representation of this replacer.  If the
        /// result of calling this function is passed to the appropriate
        /// parser, typically TransliteratorParser, it will produce another
        /// replacer that is equal to this one.
        /// </summary>
        /// <param name="escapeUnprintable">If TRUE then convert unprintable
        /// character to their hex escape representations, \\uxxxx or
        /// \\Uxxxxxxxx.  Unprintable characters are defined by
        /// <see cref="Impl.Utility.IsUnprintable(int)"/></param>
        /// <returns></returns>
        string ToReplacerPattern(bool escapeUnprintable);

        /// <summary>
        /// Union the set of all characters that may output by this object
        /// into the given set.
        /// </summary>
        /// <param name="toUnionTo">The set into which to union the output characters.</param>
        void AddReplacementSetTo(UnicodeSet toUnionTo);
    }
}
