using ICU4N.Support.Text;
using System;

namespace ICU4N.Text
{
    /// <summary>
	/// Normalization filtered by a <see cref="UnicodeSet"/>.
	/// Normalizes portions of the text contained in the filter set and leaves
	/// portions not contained in the filter set unchanged.
	/// Filtering is done via <c>UnicodeSet.Span(..., SpanCondition.Simple)</c>.
	/// Not-in-the-filter text is treated as "is normalized" and "quick check yes".
	/// This class implements all of (and only) the <see cref="Normalizer2"/> API.
	/// An instance of this class is unmodifiable/immutable.
	/// </summary>
    /// <stable>ICU 4.4</stable>
    /// <author>Markus W. Scherer</author>
    public partial class FilteredNormalizer2 : Normalizer2
    {
        /// <summary>
        /// Constructs a filtered normalizer wrapping any <see cref="Normalizer2"/> instance
        /// and a filter set.
        /// Both are aliased and must not be modified or deleted while this object
        /// is used.
        /// The filter set should be frozen; otherwise the performance will suffer greatly.
        /// </summary>
        /// <param name="n2">Wrapped <see cref="Normalizer2"/> instance.</param>
        /// <param name="filterSet"><see cref="UnicodeSet"/> which determines the characters to be normalized.</param>
        /// <stable>ICU 4.4</stable>
        public FilteredNormalizer2(Normalizer2 n2, UnicodeSet filterSet)
        {
            norm2 = n2;
            set = filterSet;
        }

        // ICU4N specific - Normalize(
        //    ICharSequence src, StringBuilder dest) moved to FilteredNormalizerExtension.tt

        // ICU4N specific - Normalize(
        //    ICharSequence src, IAppendable dest) moved to FilteredNormalizerExtension.tt

        // ICU4N specific - NormalizeSecondAndAppend(
        //    StringBuilder first, ICharSequence second) moved to FilteredNormalizerExtension.tt

        // ICU4N specific - Append(
        //    StringBuilder first, ICharSequence second) moved to FilteredNormalizerExtension.tt


        /// <summary>
        /// Gets the decomposition mapping of <paramref name="codePoint"/>.
        /// Roughly equivalent to normalizing the <see cref="string"/> form of <paramref name="codePoint"/>
        /// on a DECOMPOSE Normalizer2 instance, but much faster, and except that this function
        /// returns null if c does not have a decomposition mapping in this instance's data.
        /// This function is independent of the mode of the Normalizer2.
        /// </summary>
        /// <param name="codePoint">Code point.</param>
        /// <returns><paramref name="codePoint"/>'s decomposition mapping, if any; otherwise null.</returns>
        /// <stable>ICU 4.6</stable>
        public override string GetDecomposition(int codePoint)
        {
            return set.Contains(codePoint) ? norm2.GetDecomposition(codePoint) : null;
        }

        /// <summary>
        /// Gets the raw decomposition mapping of <paramref name="codePoint"/>.
        /// </summary>
        /// <remarks>
        /// This is similar to the <see cref="GetDecomposition"/> method but returns the
        /// raw decomposition mapping as specified in UnicodeData.txt or
        /// (for custom data) in the mapping files processed by the gennorm2 tool.
        /// By contrast, <see cref="GetDecomposition"/> returns the processed,
        /// recursively-decomposed version of this mapping.
        /// <para/>
        /// When used on a standard NFKC Normalizer2 instance,
        /// <see cref="GetRawDecomposition"/> returns the Unicode Decomposition_Mapping (dm) property.
        /// <para/>
        /// When used on a standard NFC Normalizer2 instance,
        /// it returns the Decomposition_Mapping only if the Decomposition_Type (dt) is Canonical (Can);
        /// in this case, the result contains either one or two code points (=1..4 .NET chars).
        /// <para/>
        /// This function is independent of the mode of the Normalizer2.
        /// The default implementation returns null.
        /// </remarks>
        /// <param name="codePoint">Code point.</param>
        /// <returns><paramref name="codePoint"/>'s raw decomposition mapping, if any; otherwise null.</returns>
        /// <stable>ICU 49</stable>
        public override string GetRawDecomposition(int codePoint)
        {
            return set.Contains(codePoint) ? norm2.GetRawDecomposition(codePoint) : null;
        }

        /// <summary>
        /// Performs pairwise composition of <paramref name="a"/> &amp; <paramref name="b"/> and returns the composite if there is one.
        /// </summary>
        /// <remarks>
        /// Returns a composite code point c only if c has a two-way mapping to a+b.
        /// In standard Unicode normalization, this means that
        /// c has a canonical decomposition to <paramref name="a"/>+<paramref name="b"/>
        /// and c does not have the Full_Composition_Exclusion property.
        /// <para/>
        /// This function is independent of the mode of the Normalizer2.
        /// The default implementation returns a negative value.
        /// </remarks>
        /// <param name="a">A (normalization starter) code point.</param>
        /// <param name="b">Another code point.</param>
        /// <returns>The non-negative composite code point if there is one; otherwise a negative value.</returns>
        /// <stable>ICU 49</stable>
        public override int ComposePair(int a, int b)
        {
            return (set.Contains(a) && set.Contains(b)) ? norm2.ComposePair(a, b) : -1;
        }

        /// <summary>
        /// Gets the combining class of <paramref name="codePoint"/>.
        /// The default implementation returns 0
        /// but all standard implementations return the Unicode Canonical_Combining_Class value.
        /// </summary>
        /// <param name="codePoint">Code point.</param>
        /// <returns><paramref name="codePoint"/>'s combining class.</returns>
        /// <stable>ICU 49</stable>
        public override int GetCombiningClass(int codePoint)
        {
            return set.Contains(codePoint) ? norm2.GetCombiningClass(codePoint) : 0;
        }

        // ICU4N specific - IsNormalized(ICharSequence s) moved to FilteredNormalizerExtension.tt

        // ICU4N specific - QuickCheck(ICharSequence s) moved to FilteredNormalizerExtension.tt

        // ICU4N specific - SpanQuickCheckYes(ICharSequence s) moved to FilteredNormalizerExtension.tt

        /// <summary>
        /// Tests if the <paramref name="character"/> always has a normalization boundary before it,
        /// regardless of context.
        /// If true, then the character does not normalization-interact with
        /// preceding characters.
        /// In other words, a string containing this character can be normalized
        /// by processing portions before this character and starting from this
        /// character independently.
        /// This is used for iterative normalization. See the class documentation for details.
        /// </summary>
        /// <param name="character">Character to test.</param>
        /// <returns>true if <paramref name="character"/> has a normalization boundary before it.</returns>
        /// <stable>ICU 4.4</stable>
        public override bool HasBoundaryBefore(int character)
        {
            return !set.Contains(character) || norm2.HasBoundaryBefore(character);
        }

        /// <summary>
        /// Tests if the <paramref name="character"/> always has a normalization boundary after it,
        /// regardless of context.
        /// If true, then the character does not normalization-interact with
        /// following characters.
        /// In other words, a string containing this character can be normalized
        /// by processing portions up to this character and after this
        /// character independently.
        /// This is used for iterative normalization. See the class documentation for details.
        /// <para/>
        /// Note that this operation may be significantly slower than <see cref="HasBoundaryBefore"/>.
        /// </summary>
        /// <param name="character">Character to test.</param>
        /// <returns>true if <paramref name="character"/> has a normalization boundary after it.</returns>
        /// <stable>ICU 4.4</stable>
        public override bool HasBoundaryAfter(int character)
        {
            return !set.Contains(character) || norm2.HasBoundaryAfter(character);
        }

        /// <summary>
        /// Tests if the <paramref name="character"/> is normalization-inert.
        /// If true, then the character does not change, nor normalization-interact with
        /// preceding or following characters.
        /// In other words, a string containing this character can be normalized
        /// by processing portions before this character and after this
        /// character independently.
        /// This is used for iterative normalization. See the class documentation for details.
        /// <para/>
        /// Note that this operation may be significantly slower than <see cref="HasBoundaryBefore"/>.
        /// </summary>
        /// <param name="character">Character to test.</param>
        /// <returns>true if <paramref name="character"/> is normalization-inert.</returns>
        /// <stable>ICU 4.4</stable>
        public override bool IsInert(int character)
        {
            return !set.Contains(character) || norm2.IsInert(character);
        }

        // ICU4N specific - Normalize(ICharSequence src, IAppendable dest,
        //    SpanCondition spanCondition) moved to FilteredNormalizerExtension.tt

        // ICU4N specific - NormalizeSecondAndAppend(StringBuilder first, ICharSequence second,
        //    bool doNormalize) moved to FilteredNormalizerExtension.tt

        private Normalizer2 norm2;
        private UnicodeSet set;
    }
}
