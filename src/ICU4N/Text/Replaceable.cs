using System;

namespace ICU4N.Text
{
    /// <summary>
    /// <see cref="IReplaceable"/> is an interface representing a
    /// string of characters that supports the replacement of a range of
    /// itself with a new string of characters.  It is used by APIs that
    /// change a piece of text while retaining metadata.  Metadata is data
    /// other than the Unicode characters returned by <see cref="Char32At(int)"/>.  One
    /// example of metadata is style attributes; another is an edit
    /// history, marking each character with an author and revision number.
    /// </summary>
    /// <remarks>
    /// An implicit aspect of the <see cref="IReplaceable"/> API is that
    /// during a replace operation, new characters take on the metadata of
    /// the old characters.  For example, if the string "the <b>bold</b>
    /// font" has range (4, 8) replaced with "strong", then it becomes "the
    /// <b>strong</b> font".
    /// <para/>
    /// <see cref="IReplaceable"/> specifies ranges using a start
    /// offset and a limit offset.  The range of characters thus specified
    /// includes the characters at offset start..limit-1.  That is, the
    /// start offset is inclusive, and the limit offset is exclusive.
    /// <para/>
    /// <see cref="IReplaceable"/> also includes API to access characters
    /// in the string: <see cref="Length"/>, <see cref="this[int]"/>,
    /// and <see cref="Char32At(int)"/>.
    /// <para/>
    /// For an implementation to support metadata, typical behavior of
    /// <see cref="Replace(int, int, ReadOnlySpan{char})"/> is the following:
    /// <list type="bullet">
    ///     <item><description>
    ///         Set the metadata of the new text to the metadata of the first
    ///         character replaced
    ///     </description></item>
    ///     <item><description>
    ///         If no characters are replaced, use the metadata of the
    ///         previous character
    ///     </description></item>
    ///     <item><description>
    ///         If there is no previous character (i.e. start == 0), use the
    ///         following character
    ///     </description></item>
    ///     <item><description>
    ///         If there is no following character (i.e. the replaceable was
    ///         empty), use default metadata
    ///     </description></item>
    ///     <item><description>
    ///         If the code point U+FFFF is seen, it should be interpreted as
    ///         a special marker having no metadata
    ///     </description></item>
    /// </list>
    /// If this is not the behavior, the implementation should document any differences.
    /// </remarks>
    /// <author>Alan Liu</author>
    /// <stable>ICU 2.0</stable>
    public interface IReplaceable
    {
        /// <summary>
        /// Gets the number of 16-bit code units in the text.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        int Length { get; }

        /// <summary>
        /// Gets the 16-bit code unit at the given offset into the text.
        /// </summary>
        /// <remarks>
        /// NOTE: This is equivalent to charAt(index) in ICU4J.
        /// </remarks>
        /// <param name="index">An integer between 0 and <see cref="Length"/>-1 inclusive.</param>
        /// <returns>16-bit code unit of text at given offset.</returns>
        /// <stable>ICU 2.0</stable>
        char this[int index] { get; }

        /// <summary>
        /// Returns the 32-bit code point at the given 16-bit offset into
        /// the text.  This assumes the text is stored as 16-bit code units
        /// with surrogate pairs intermixed.  If the offset of a leading or
        /// trailing code unit of a surrogate pair is given, return the
        /// code point of the surrogate pair.
        /// <para/>
        /// Most implementations can return
        /// <c>UTF16.CharAt(this, offset)</c>.
        /// </summary>
        /// <param name="index">An integer between 0 and <see cref="Length"/>-1 inclusive.</param>
        /// <returns>32-bit code point of text at given offset.</returns>
        /// <stable>ICU 2.0</stable>
        int Char32At(int index);

        /// <summary>
        /// Copies characters from this object into the destination
        /// character array.  The first character to be copied is at index
        /// <paramref name="sourceIndex"/>; the last character to be copied is at
        /// index <paramref name="count"/>-<paramref name="sourceIndex"/>.
        /// The characters are copied into the subarray of <paramref name="destination"/>
        /// starting at index <paramref name="destinationIndex"/> and ending at index
        /// <paramref name="destinationIndex"/>+<paramref name="count"/>.
        /// </summary>
        /// <remarks>
        /// NOTE: This is roughly equivalent to GetChars(int srcStart, int srcLimit, char dst[], int dstStart)
        /// in ICU4J with one important difference - the final parameter is the total
        /// count of characters to be copied (srcLimit - srcStart). This is to 
        /// make the function compatible with the <see cref="string.CopyTo(int, char[], int, int)"/> implementation in .NET.
        /// </remarks>
        /// <param name="sourceIndex">The index of the first character in this instance to copy.</param>
        /// <param name="destination">An array of Unicode characters to which characters in this instance are copied.</param>
        /// <param name="destinationIndex">The index in <paramref name="destination"/> at which the copy operation begins.</param>
        /// <param name="count">The number of characters in this instance to copy to <paramref name="destination"/>.</param>
        /// <stable>ICU 2.0</stable>
        void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count);

        /// <summary>
        /// Replaces a substring of this object with the given text.
        /// <para/>
        /// Implementations must ensure that if the text input
        /// is equal to the replacement text, that replace has no
        /// effect. That is, any metadata
        /// should be unaffected. In addition, implementers are encouraged to
        /// check for initial and trailing identical characters, and make a
        /// smaller replacement if possible. This will preserve as much
        /// metadata as possible.
        /// <para/>
        /// NOTE: The 2nd parameter differs from ICU4J in that it is a count rather than an exclusive
        /// end index. To translate from Java, use <c>limit - start</c> to resolve <paramref name="count"/>.
        /// </summary>
        /// <param name="startIndex">The beginning index, inclusive; <c>0 &lt;= <paramref name="startIndex"/></c>.</param>
        /// <param name="count">The ending index, exclusive; <c>0 &lt;= <paramref name="count"/></c>.</param>
        /// <param name="text">The text to replace characters beginning at <paramref name="startIndex"/>.</param>
        /// <stable>ICU 2.0</stable>
        void Replace(int startIndex, int count, string text);

        /// <summary>
        /// Replaces a substring of this object with the given text.
        /// <para/>
        /// Implementations must ensure that if the text input
        /// is equal to the replacement text, that replace has no
        /// effect. That is, any metadata
        /// should be unaffected. In addition, implementers are encouraged to
        /// check for initial and trailing identical characters, and make a
        /// smaller replacement if possible. This will preserve as much
        /// metadata as possible.
        /// <para/>
        /// NOTE: The 2nd parameter differs from ICU4J in that it is a count rather than an exclusive
        /// end index. To translate from Java, use <c>limit - start</c> to resolve <paramref name="count"/>.
        /// <para/>
        /// This overload can be used in place of the Replace(int, int, char[], int, int) overload in ICU4J
        /// by slicing into the passed in text (i.e. <c>text.Slice(0, 3)</c>).
        /// </summary>
        /// <param name="startIndex">The beginning index, inclusive; <c>0 &lt;= <paramref name="startIndex"/></c>.</param>
        /// <param name="count">The ending index, exclusive; <c>0 &lt;= <paramref name="count"/></c>.</param>
        /// <param name="text">The text to replace characters beginning at <paramref name="startIndex"/>.</param>
        /// <stable>ICU 2.0</stable>
        void Replace(int startIndex, int count, ReadOnlySpan<char> text);

        /// <summary>
        /// Copies a substring of this object, retaining metadata.
        /// This method is used to duplicate or reorder substrings.
        /// The destination index must not overlap the source range.
        /// <para/>
        /// NOTE: This method has .NET semantics. That is, the 2nd parameter
        /// is a length rather than an exclusive end index (or limit).
        /// To translate from Java, use <c>limit - start</c> to resolve
        /// <paramref name="length"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="HasMetaData"/> returns false, implementations
        /// may use the naive implementation:
        /// 
        /// <code>
        /// char[] text = new char[length];
        /// CopyTo(startIndex, text, 0, length);
        /// Replace(destinationIndex, destinationIndex, text, 0, length);
        /// </code>
        /// </remarks>
        /// <param name="startIndex">The beginning index, inclusive; <c>0 &lt;= <paramref name="startIndex"/></c>.</param>
        /// <param name="length">The length; <c><paramref name="startIndex"/> + <paramref name="length"/> &lt;= <see cref="Length"/></c>.</param>
        /// <param name="destinationIndex">The destination index.  <paramref name="length"/> characters from
        /// <paramref name="startIndex"/> will be copied to <paramref name="destinationIndex"/>.
        /// Implementations of this method may assume that <c><paramref name="destinationIndex"/> &lt;= <paramref name="startIndex"/> ||
        /// <paramref name="destinationIndex"/> &gt;= <paramref name="startIndex"/> + <paramref name="length"/></c>.
        /// </param>
        /// <stable>ICU 2.0</stable>
        void Copy(int startIndex, int length, int destinationIndex); // ICU4N: Changed 2nd parameter from limit to length (.NET convention)

        /// <summary>
        /// Returns true if this object contains metadata.  If a
        /// <see cref="IReplaceable"/> object has metadata, calls to the <see cref="IReplaceable"/> API
        /// must be made so as to preserve metadata.  If it does not, calls
        /// to the <see cref="IReplaceable"/> API may be optimized to improve performance.
        /// </summary>
        /// <stable>ICU 2.2</stable>
        bool HasMetaData { get; }
    }
}
