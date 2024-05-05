using ICU4N.Support.Text;
using J2N.Text;
using System;
using System.Text;

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
            norm2 = n2 ?? throw new ArgumentNullException(nameof(n2));
            set = filterSet ?? throw new ArgumentNullException(nameof(filterSet));
        }

        #region Normalize(ICharSequence, StringBuilder)
        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        /// <stable>ICU 4.4</stable>
        public override StringBuilder Normalize(string src, StringBuilder dest)
        {
            dest.Length = 0;
            Normalize(src, dest, SpanCondition.Simple);
            return dest;
        }

        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        /// <stable>ICU 4.4</stable>
        public override StringBuilder Normalize(ReadOnlySpan<char> src, StringBuilder dest)
        {
            dest.Length = 0;
            Normalize(src, dest, SpanCondition.Simple);
            return dest;
        }

        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        /// <stable>ICU 4.4</stable>
        internal override void Normalize(ReadOnlySpan<char> src, ref ValueStringBuilder dest)
        {
            if (MemoryHelper.AreSame(src, dest.RawChars))
            {
                throw new ArgumentException($"'{nameof(src)}' cannot be the same memory location as '{nameof(dest)}'");
            }

            dest.Length = 0;
            Normalize(src, ref dest, SpanCondition.Simple);
        }

        #endregion Normalize(ICharSequence, StringBuilder)

        #region Normalize(ICharSequence, IAppendable)
        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        /// <typeparam name="TAppendable">The implementation of <see cref="IAppendable"/> to use to write the output.</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="src"/> is <c>null</c>.</exception>
        /// <stable>ICU 4.6</stable>
        public override TAppendable Normalize<TAppendable>(string src, TAppendable dest)
        {
            return Normalize(src, dest, SpanCondition.Simple);
        }

        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        /// <typeparam name="TAppendable">The implementation of <see cref="IAppendable"/> to use to write the output.</typeparam>
        /// <stable>ICU 4.6</stable>
        public override TAppendable Normalize<TAppendable>(ReadOnlySpan<char> src, TAppendable dest)
        {
            return Normalize(src, dest, SpanCondition.Simple);
        }

        #endregion Normalize(ICharSequence, IAppendable)

        #region NormalizeSecondAndAppend(StringBuilder, ICharSequence)
        /// <summary>
        /// Appends the normalized form of the second string to the first string
        /// (merging them at the boundary) and returns the first string.
        /// The result is normalized if the first string was normalized.
        /// The first and second strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, will be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        public override StringBuilder NormalizeSecondAndAppend(
            StringBuilder first, string second)
        {
            return NormalizeSecondAndAppend(first, second, true);
        }

        /// <summary>
        /// Appends the normalized form of the second string to the first string
        /// (merging them at the boundary) and returns the first string.
        /// The result is normalized if the first string was normalized.
        /// The first and second strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, will be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        public override StringBuilder NormalizeSecondAndAppend(
            StringBuilder first, ReadOnlySpan<char> second)
        {
            return NormalizeSecondAndAppend(first, second, true);
        }

        internal override void NormalizeSecondAndAppend(
            ref ValueStringBuilder first, ReadOnlySpan<char> second)
        {
            NormalizeSecondAndAppend(ref first, second, true);
        }

        #endregion NormalizeSecondAndAppend(StringBuilder, ICharSequence)

        #region Append(StringBuilder, ICharSequence)
        /// <summary>
        /// Appends the second string to the first string
        /// (merging them at the boundary) and returns the first string.
        /// The result is normalized if both the strings were normalized.
        /// The first and second strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, should be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        /// <stable>ICU 4.4</stable>
        public override StringBuilder Append(StringBuilder first, string second)
        {
            return NormalizeSecondAndAppend(first, second, false);
        }

        /// <summary>
        /// Appends the second string to the first string
        /// (merging them at the boundary) and returns the first string.
        /// The result is normalized if both the strings were normalized.
        /// The first and second strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, should be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        /// <stable>ICU 4.4</stable>
        public override StringBuilder Append(StringBuilder first, ReadOnlySpan<char> second)
        {
            return NormalizeSecondAndAppend(first, second, false);
        }

        internal override void Append(ref ValueStringBuilder first, ReadOnlySpan<char> second)
        {
            if (MemoryHelper.AreSame(first.RawChars, second))
            {
                throw new ArgumentException($"'{nameof(first)}' cannot be the same memory location as '{nameof(second)}'");
            }

            NormalizeSecondAndAppend(ref first, second);
        }

        #endregion Append(StringBuilder, ICharSequence)

        /// <summary>
        /// Gets the decomposition mapping of <paramref name="codePoint"/>.
        /// Roughly equivalent to normalizing the <see cref="string"/> form of <paramref name="codePoint"/>
        /// on a DECOMPOSE <see cref="Normalizer2"/> instance, but much faster, and except that this function
        /// returns null if c does not have a decomposition mapping in this instance's data.
        /// This function is independent of the mode of the <see cref="Normalizer2"/>.
        /// </summary>
        /// <param name="codePoint">Code point.</param>
        /// <returns><paramref name="codePoint"/>'s decomposition mapping, if any; otherwise <c>null</c>.</returns>
        /// <stable>ICU 4.6</stable>
        public override string GetDecomposition(int codePoint)
        {
            return set.Contains(codePoint) ? norm2.GetDecomposition(codePoint) : null;
        }

        /// <summary>
        /// Gets the decomposition mapping of <paramref name="codePoint"/>.
        /// Roughly equivalent to normalizing the <see cref="string"/> form of <paramref name="codePoint"/>
        /// on a DECOMPOSE <see cref="Normalizer2"/> instance, but much faster, and except that this function
        /// returns <c>false</c> if <paramref name="codePoint"/> does not have a decomposition mapping in this instance's data.
        /// This function is independent of the mode of the <see cref="Normalizer2"/>.
        /// </summary>
        /// <param name="codePoint">Code point.</param>
        /// <param name="destination">Upon return, will contain the decomposition.</param>
        /// <param name="charsLength">Upon return, will contain the length of the decomposition (whether successuful or not).
        /// If the value is 0, it means there is not a valid decomposition value. If the value is greater than 0 and
        /// the method returns <c>false</c>, it means that there was not enough space allocated and the number indicates
        /// the minimum number of chars required.</param>
        /// <returns><c>true</c> if the decomposition was succssfully written to <paramref name="destination"/>; otherwise, <c>false</c>.</returns>
        /// <draft>ICU 60.1</draft>
        public override bool TryGetDecomposition(int codePoint, Span<char> destination, out int charsLength)
        {
            charsLength = 0;
            return set.Contains(codePoint) ? norm2.TryGetDecomposition(codePoint, destination, out charsLength) : false;
        }

        /// <summary>
        /// Gets the raw decomposition mapping of <paramref name="codePoint"/>.
        /// <para/>
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
        /// The default implementation returns <c>null</c>.
        /// </summary>
        /// <param name="codePoint">Code point.</param>
        /// <returns><paramref name="codePoint"/>'s raw decomposition mapping, if any; otherwise <c>null</c>.</returns>
        /// <stable>ICU 49</stable>
        public override string GetRawDecomposition(int codePoint)
        {
            return set.Contains(codePoint) ? norm2.GetRawDecomposition(codePoint) : null;
        }

        /// <summary>
        /// Gets the raw decomposition mapping of <paramref name="codePoint"/>.
        /// <para/>
        /// This is similar to the <see cref="GetDecomposition"/> method but returns the
        /// raw decomposition mapping as specified in UnicodeData.txt or
        /// (for custom data) in the mapping files processed by the gennorm2 tool.
        /// By contrast, <see cref="GetDecomposition"/> returns the processed,
        /// recursively-decomposed version of this mapping.
        /// <para/>
        /// When used on a standard NFKC <see cref="Normalizer2"/> instance,
        /// <see cref="GetRawDecomposition"/> returns the Unicode Decomposition_Mapping (dm) property.
        /// <para/>
        /// When used on a standard NFC <see cref="Normalizer2"/> instance,
        /// it returns the Decomposition_Mapping only if the Decomposition_Type (dt) is Canonical (Can);
        /// in this case, the result contains either one or two code points (=1..4 .NET chars).
        /// <para/>
        /// This function is independent of the mode of the <see cref="Normalizer2"/>.
        /// The default implementation returns <c>false</c>.
        /// </summary>
        /// <param name="codePoint">Code point.</param>
        /// <param name="destination">Upon return, will contain the raw decomposition.</param>
        /// <param name="charsLength">Upon return, will contain the length of the decomposition (whether successuful or not).
        /// If the value is 0, it means there is not a valid decomposition value. If the value is greater than 0 and
        /// the method returns <c>false</c>, it means that there was not enough space allocated and the number indicates
        /// the minimum number of chars required.</param>
        /// <returns><c>true</c> if the decomposition was succssfully written to <paramref name="destination"/>; otherwise, <c>false</c>.</returns>
        /// <draft>ICU 60.1</draft>
        public override bool TryGetRawDecomposition(int codePoint, Span<char> destination, out int charsLength)
        {
            charsLength = 0;
            return set.Contains(codePoint) ? norm2.TryGetRawDecomposition(codePoint, destination, out charsLength) : false;
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

        #region IsNormalized(ICharSequence)
        /// <summary>
        /// Tests if the string is normalized.
        /// Internally, in cases where the <see cref="QuickCheck(string)"/> method would return "maybe"
        /// (which is only possible for the two COMPOSE modes) this method
        /// resolves to "yes" or "no" to provide a definitive result,
        /// at the cost of doing more work in those cases.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>true if s is normalized.</returns>
        /// <stable>ICU 4.4</stable>
        public override bool IsNormalized(string s)
        {
            SpanCondition spanCondition = SpanCondition.Simple;
            for (int prevSpanLimit = 0; prevSpanLimit < s.Length;)
            {
                int spanLimit = set.Span(s, prevSpanLimit, spanCondition);
                if (spanCondition == SpanCondition.NotContained)
                {
                    spanCondition = SpanCondition.Simple;
                }
                else
                {
                    if (!norm2.IsNormalized(s.AsSpan(prevSpanLimit, spanLimit - prevSpanLimit))) // ICU4N: Corrected 2nd parameter
                    {
                        return false;
                    }
                    spanCondition = SpanCondition.NotContained;
                }
                prevSpanLimit = spanLimit;
            }
            return true;
        }

        /// <summary>
        /// Tests if the string is normalized.
        /// Internally, in cases where the <see cref="QuickCheck(ReadOnlySpan{char})"/> method would return "maybe"
        /// (which is only possible for the two COMPOSE modes) this method
        /// resolves to "yes" or "no" to provide a definitive result,
        /// at the cost of doing more work in those cases.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>true if s is normalized.</returns>
        /// <stable>ICU 4.4</stable>
        public override bool IsNormalized(ReadOnlySpan<char> s)
        {
            SpanCondition spanCondition = SpanCondition.Simple;
            for (int prevSpanLimit = 0; prevSpanLimit < s.Length;)
            {
                int spanLimit = set.Span(s, prevSpanLimit, spanCondition);
                if (spanCondition == SpanCondition.NotContained)
                {
                    spanCondition = SpanCondition.Simple;
                }
                else
                {
                    if (!norm2.IsNormalized(s.Slice(prevSpanLimit, spanLimit - prevSpanLimit))) // ICU4N: Corrected 2nd parameter
                    {
                        return false;
                    }
                    spanCondition = SpanCondition.NotContained;
                }
                prevSpanLimit = spanLimit;
            }
            return true;
        }

        #endregion IsNormalized(ICharSequence)

        #region QuickCheck(ICharSequence)
        /// <summary>
        /// Tests if the string is normalized.
        /// For the two COMPOSE modes, the result could be "maybe" in cases that
        /// would take a little more work to resolve definitively.
        /// Use <see cref="SpanQuickCheckYes(string)"/> and
        /// <see cref="NormalizeSecondAndAppend(StringBuilder, string)"/> for a faster
        /// combination of quick check + normalization, to avoid
        /// re-checking the "yes" prefix.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>The quick check result.</returns>
        /// <stable>ICU 4.4</stable>
        public override QuickCheckResult QuickCheck(string s)
        {
            QuickCheckResult result = QuickCheckResult.Yes;
            SpanCondition spanCondition = SpanCondition.Simple;
            for (int prevSpanLimit = 0; prevSpanLimit < s.Length;)
            {
                int spanLimit = set.Span(s, prevSpanLimit, spanCondition);
                if (spanCondition == SpanCondition.NotContained)
                {
                    spanCondition = SpanCondition.Simple;
                }
                else
                {
                    QuickCheckResult qcResult = norm2.QuickCheck(s.AsSpan(prevSpanLimit, spanLimit - prevSpanLimit)); // ICU4N: Corrected 2nd parameter
                    if (qcResult == QuickCheckResult.No)
                    {
                        return qcResult;
                    }
                    else if (qcResult == QuickCheckResult.Maybe)
                    {
                        result = qcResult;
                    }
                    spanCondition = SpanCondition.NotContained;
                }
                prevSpanLimit = spanLimit;
            }
            return result;
        }

        /// <summary>
        /// Tests if the string is normalized.
        /// For the two COMPOSE modes, the result could be "maybe" in cases that
        /// would take a little more work to resolve definitively.
        /// Use <see cref="SpanQuickCheckYes(ReadOnlySpan{char})"/> and
        /// <see cref="NormalizeSecondAndAppend(StringBuilder, ReadOnlySpan{char})"/> for a faster
        /// combination of quick check + normalization, to avoid
        /// re-checking the "yes" prefix.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>The quick check result.</returns>
        /// <stable>ICU 4.4</stable>
        public override QuickCheckResult QuickCheck(ReadOnlySpan<char> s)
        {
            QuickCheckResult result = QuickCheckResult.Yes;
            SpanCondition spanCondition = SpanCondition.Simple;
            for (int prevSpanLimit = 0; prevSpanLimit < s.Length;)
            {
                int spanLimit = set.Span(s, prevSpanLimit, spanCondition);
                if (spanCondition == SpanCondition.NotContained)
                {
                    spanCondition = SpanCondition.Simple;
                }
                else
                {
                    QuickCheckResult qcResult = norm2.QuickCheck(s.Slice(prevSpanLimit, spanLimit - prevSpanLimit)); // ICU4N: Corrected 2nd parameter
                    if (qcResult == QuickCheckResult.No)
                    {
                        return qcResult;
                    }
                    else if (qcResult == QuickCheckResult.Maybe)
                    {
                        result = qcResult;
                    }
                    spanCondition = SpanCondition.NotContained;
                }
                prevSpanLimit = spanLimit;
            }
            return result;
        }

        #endregion QuickCheck(ICharSequence)

        #region SpanQuickCheckYes(ICharSequence)
        /// <summary>
        /// Returns the end of the normalized substring of the input string.
        /// In other words, with <c>end=SpanQuickCheckYes(s);</c>
        /// the substring <c>s.SubString(0, end)</c>
        /// will pass the quick check with a "yes" result.
        /// </summary>
        /// <remarks>
        /// The returned end index is usually one or more characters before the
        /// "no" or "maybe" character: The end index is at a normalization boundary.
        /// (See the class documentation for more about normalization boundaries.)
        /// <para/>
        /// When the goal is a normalized string and most input strings are expected
        /// to be normalized already, then call this method,
        /// and if it returns a prefix shorter than the input string,
        /// copy that prefix and use <see cref="NormalizeSecondAndAppend(StringBuilder, string)"/> for the remainder.
        /// </remarks>
        /// <param name="s">Input string.</param>
        /// <returns>"yes" span end index.</returns>
        /// <stable>ICU 4.4</stable>
        public override int SpanQuickCheckYes(string s)
        {
            SpanCondition spanCondition = SpanCondition.Simple;
            for (int prevSpanLimit = 0; prevSpanLimit < s.Length;)
            {
                int spanLimit = set.Span(s, prevSpanLimit, spanCondition);
                if (spanCondition == SpanCondition.NotContained)
                {
                    spanCondition = SpanCondition.Simple;
                }
                else
                {
                    int yesLimit =
                        prevSpanLimit + norm2.SpanQuickCheckYes(s.AsSpan(prevSpanLimit, spanLimit - prevSpanLimit)); // ICU4N: Corrected 2nd parameter
                    if (yesLimit < spanLimit)
                    {
                        return yesLimit;
                    }
                    spanCondition = SpanCondition.NotContained;
                }
                prevSpanLimit = spanLimit;
            }
            return s.Length;
        }

        /// <summary>
        /// Returns the end of the normalized substring of the input string.
        /// In other words, with <c>end=SpanQuickCheckYes(s);</c>
        /// the substring <c>s.SubString(0, end)</c>
        /// will pass the quick check with a "yes" result.
        /// </summary>
        /// <remarks>
        /// The returned end index is usually one or more characters before the
        /// "no" or "maybe" character: The end index is at a normalization boundary.
        /// (See the class documentation for more about normalization boundaries.)
        /// <para/>
        /// When the goal is a normalized string and most input strings are expected
        /// to be normalized already, then call this method,
        /// and if it returns a prefix shorter than the input string,
        /// copy that prefix and use <see cref="NormalizeSecondAndAppend(StringBuilder, ReadOnlySpan{char})"/> for the remainder.
        /// </remarks>
        /// <param name="s">Input string.</param>
        /// <returns>"yes" span end index.</returns>
        /// <stable>ICU 4.4</stable>
        public override int SpanQuickCheckYes(ReadOnlySpan<char> s)
        {
            SpanCondition spanCondition = SpanCondition.Simple;
            for (int prevSpanLimit = 0; prevSpanLimit < s.Length;)
            {
                int spanLimit = set.Span(s, prevSpanLimit, spanCondition);
                if (spanCondition == SpanCondition.NotContained)
                {
                    spanCondition = SpanCondition.Simple;
                }
                else
                {
                    int yesLimit =
                        prevSpanLimit + norm2.SpanQuickCheckYes(s.Slice(prevSpanLimit, spanLimit - prevSpanLimit)); // ICU4N: Corrected 2nd parameter
                    if (yesLimit < spanLimit)
                    {
                        return yesLimit;
                    }
                    spanCondition = SpanCondition.NotContained;
                }
                prevSpanLimit = spanLimit;
            }
            return s.Length;
        }

        #endregion SpanQuickCheckYes(ICharSequence)

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

        #region Normalize(ICharSequence, IAppendable, SpanCondition)
        // Internal: No argument checking, and appends to dest.
        // Pass as input spanCondition the one that is likely to yield a non-zero
        // span length at the start of src.
        // For set=[:age=3.2:], since almost all common characters were in Unicode 3.2,
        // <see cref="SpanCondition.Simple"/> should be passed in for the start of src
        // and <see cref="SpanCondition.NotContained"/> should be passed in if we continue after
        // an in-filter prefix.
        private StringBuilder Normalize(string src, StringBuilder dest,
                                     SpanCondition spanCondition)
        {
            // Don't throw away destination buffer between iterations.
            StringBuilder tempDest = new StringBuilder();
            // ICU4N: Removed unnecessary try/catch for IOException
            for (int prevSpanLimit = 0; prevSpanLimit < src.Length;)
            {
                int spanLimit = set.Span(src, prevSpanLimit, spanCondition);
                int spanLength = spanLimit - prevSpanLimit;
                if (spanCondition == SpanCondition.NotContained)
                {
                    if (spanLength != 0)
                    {
                        dest.Append(src.AsSpan(prevSpanLimit, spanLimit - prevSpanLimit)); // ICU4N: Changed 3rd parameter
                    }
                    spanCondition = SpanCondition.Simple;
                }
                else
                {
                    if (spanLength != 0)
                    {
                        // Not norm2.normalizeSecondAndAppend() because we do not want
                        // to modify the non-filter part of dest.
                        dest.Append(norm2.Normalize(src.AsSpan(prevSpanLimit, spanLimit - prevSpanLimit), tempDest)); // ICU4N: Changed 2nd parameter
                    }
                    spanCondition = SpanCondition.NotContained;
                }
                prevSpanLimit = spanLimit;
            }
            return dest;
        }

        // Internal: No argument checking, and appends to dest.
        // Pass as input spanCondition the one that is likely to yield a non-zero
        // span length at the start of src.
        // For set=[:age=3.2:], since almost all common characters were in Unicode 3.2,
        // <see cref="SpanCondition.Simple"/> should be passed in for the start of src
        // and <see cref="SpanCondition.NotContained"/> should be passed in if we continue after
        // an in-filter prefix.
        private StringBuilder Normalize(ReadOnlySpan<char> src, StringBuilder dest,
                                     SpanCondition spanCondition)
        {
            // Don't throw away destination buffer between iterations.
            StringBuilder tempDest = new StringBuilder();
            // ICU4N: Removed unnecessary try/catch for IOException
            for (int prevSpanLimit = 0; prevSpanLimit < src.Length;)
            {
                int spanLimit = set.Span(src, prevSpanLimit, spanCondition);
                int spanLength = spanLimit - prevSpanLimit;
                if (spanCondition == SpanCondition.NotContained)
                {
                    if (spanLength != 0)
                    {
                        dest.Append(src.Slice(prevSpanLimit, spanLimit - prevSpanLimit)); // ICU4N: Changed 3rd parameter
                    }
                    spanCondition = SpanCondition.Simple;
                }
                else
                {
                    if (spanLength != 0)
                    {
                        // Not norm2.normalizeSecondAndAppend() because we do not want
                        // to modify the non-filter part of dest.
                        dest.Append(norm2.Normalize(src.Slice(prevSpanLimit, spanLimit - prevSpanLimit), tempDest)); // ICU4N: Changed 2nd parameter
                    }
                    spanCondition = SpanCondition.NotContained;
                }
                prevSpanLimit = spanLimit;
            }
            return dest;
        }

        // ICU4N TODO: Make the below implementation authoritive and cascade all calls to this

        // Internal: No argument checking, and appends to dest.
        // Pass as input spanCondition the one that is likely to yield a non-zero
        // span length at the start of src.
        // For set=[:age=3.2:], since almost all common characters were in Unicode 3.2,
        // <see cref="SpanCondition.Simple"/> should be passed in for the start of src
        // and <see cref="SpanCondition.NotContained"/> should be passed in if we continue after
        // an in-filter prefix.
        private void Normalize(ReadOnlySpan<char> src, ref ValueStringBuilder dest,
                                     SpanCondition spanCondition)
        {
            // Don't throw away destination buffer between iterations.
            StringBuilder tempDest = new StringBuilder();
            // ICU4N: Removed unnecessary try/catch for IOException
            for (int prevSpanLimit = 0; prevSpanLimit < src.Length;)
            {
                int spanLimit = set.Span(src, prevSpanLimit, spanCondition);
                int spanLength = spanLimit - prevSpanLimit;
                if (spanCondition == SpanCondition.NotContained)
                {
                    if (spanLength != 0)
                    {
                        dest.Append(src.Slice(prevSpanLimit, spanLimit - prevSpanLimit)); // ICU4N: Changed 3rd parameter
                    }
                    spanCondition = SpanCondition.Simple;
                }
                else
                {
                    if (spanLength != 0)
                    {
                        // Not norm2.normalizeSecondAndAppend() because we do not want
                        // to modify the non-filter part of dest.
                        dest.Append(norm2.Normalize(src.Slice(prevSpanLimit, spanLimit - prevSpanLimit), tempDest)); // ICU4N: Changed 2nd parameter
                    }
                    spanCondition = SpanCondition.NotContained;
                }
                prevSpanLimit = spanLimit;
            }
        }



        // Internal: No argument checking, and appends to dest.
        // Pass as input spanCondition the one that is likely to yield a non-zero
        // span length at the start of src.
        // For set=[:age=3.2:], since almost all common characters were in Unicode 3.2,
        // <see cref="SpanCondition.Simple"/> should be passed in for the start of src
        // and <see cref="SpanCondition.NotContained"/> should be passed in if we continue after
        // an in-filter prefix.
        private TAppendable Normalize<TAppendable>(string src, TAppendable dest,
                                     SpanCondition spanCondition) where TAppendable : IAppendable
        {
            // Don't throw away destination buffer between iterations.
            StringBuilder tempDest = new StringBuilder();
            // ICU4N: Removed unnecessary try/catch for IOException
            for (int prevSpanLimit = 0; prevSpanLimit < src.Length;)
            {
                int spanLimit = set.Span(src, prevSpanLimit, spanCondition);
                int spanLength = spanLimit - prevSpanLimit;
                if (spanCondition == SpanCondition.NotContained)
                {
                    if (spanLength != 0)
                    {
                        dest.Append(src.AsSpan(prevSpanLimit, spanLimit - prevSpanLimit)); // ICU4N: Changed 3rd parameter
                    }
                    spanCondition = SpanCondition.Simple;
                }
                else
                {
                    if (spanLength != 0)
                    {
                        // Not norm2.normalizeSecondAndAppend() because we do not want
                        // to modify the non-filter part of dest.
                        dest.Append(norm2.Normalize(src.AsSpan(prevSpanLimit, spanLimit - prevSpanLimit), tempDest)); // ICU4N: Changed 2nd parameter
                    }
                    spanCondition = SpanCondition.NotContained;
                }
                prevSpanLimit = spanLimit;
            }
            return dest;
        }

        // Internal: No argument checking, and appends to dest.
        // Pass as input spanCondition the one that is likely to yield a non-zero
        // span length at the start of src.
        // For set=[:age=3.2:], since almost all common characters were in Unicode 3.2,
        // <see cref="SpanCondition.Simple"/> should be passed in for the start of src
        // and <see cref="SpanCondition.NotContained"/> should be passed in if we continue after
        // an in-filter prefix.
        private TAppendable Normalize<TAppendable>(ReadOnlySpan<char> src, TAppendable dest,
                                     SpanCondition spanCondition) where TAppendable : IAppendable
        {
            // Don't throw away destination buffer between iterations.
            StringBuilder tempDest = new StringBuilder();
            // ICU4N: Removed unnecessary try/catch for IOException
            for (int prevSpanLimit = 0; prevSpanLimit < src.Length;)
            {
                int spanLimit = set.Span(src, prevSpanLimit, spanCondition);
                int spanLength = spanLimit - prevSpanLimit;
                if (spanCondition == SpanCondition.NotContained)
                {
                    if (spanLength != 0)
                    {
                        dest.Append(src.Slice(prevSpanLimit, spanLimit - prevSpanLimit)); // ICU4N: Changed 3rd parameter
                    }
                    spanCondition = SpanCondition.Simple;
                }
                else
                {
                    if (spanLength != 0)
                    {
                        // Not norm2.normalizeSecondAndAppend() because we do not want
                        // to modify the non-filter part of dest.
                        dest.Append(norm2.Normalize(src.Slice(prevSpanLimit, spanLimit - prevSpanLimit), tempDest)); // ICU4N: Changed 2nd parameter
                    }
                    spanCondition = SpanCondition.NotContained;
                }
                prevSpanLimit = spanLimit;
            }
            return dest;
        }

        #endregion Normalize(ICharSequence, IAppendable, SpanCondition)

        #region NormalizeSecondAndAppend(StringBuilder, ICharSequence, bool)
        private StringBuilder NormalizeSecondAndAppend(StringBuilder first, string second,
                                                       bool doNormalize)
        {
            if (first.Length == 0)
            {
                if (doNormalize)
                {
                    return Normalize(second, first);
                }
                else
                {
                    return first.Append(second);
                }
            }
            // merge the in-filter suffix of the first string with the in-filter prefix of the second
            int prefixLimit = set.Span(second, 0, SpanCondition.Simple);
            if (prefixLimit != 0)
            {
                var prefix = second.AsSpan(0, prefixLimit - 0); // ICU4N: Checked 2nd parameter
                int suffixStart = set.SpanBack(first, 0x7fffffff, SpanCondition.Simple);
                if (suffixStart == 0)
                {
                    if (doNormalize)
                    {
                        norm2.NormalizeSecondAndAppend(first, prefix);
                    }
                    else
                    {
                        norm2.Append(first, prefix);
                    }
                }
                else
                {
                    StringBuilder middle = new StringBuilder(
                            first.ToString(suffixStart, first.Length - suffixStart)); // ICU4N: Changed 2nd parameter
                    if (doNormalize)
                    {
                        norm2.NormalizeSecondAndAppend(middle, prefix);
                    }
                    else
                    {
                        norm2.Append(middle, prefix);
                    }
                    first.Delete(suffixStart, 0x7fffffff - suffixStart).Append(middle); // ICU4N: Corrected 2nd parameter of Delete
                }
            }
            if (prefixLimit < second.Length)
            {
                var rest = second.AsSpan(prefixLimit, second.Length - prefixLimit); // ICU4N: Corrected 2nd parameter
                if (doNormalize)
                {
                    Normalize(rest, first, SpanCondition.NotContained);
                }
                else
                {
                    first.Append(rest);
                }
            }
            return first;
        }

        private StringBuilder NormalizeSecondAndAppend(StringBuilder first, ReadOnlySpan<char> second,
                                                       bool doNormalize)
        {
            if (first.Length == 0)
            {
                if (doNormalize)
                {
                    return Normalize(second, first);
                }
                else
                {
                    return first.Append(second);
                }
            }
            // merge the in-filter suffix of the first string with the in-filter prefix of the second
            int prefixLimit = set.Span(second, 0, SpanCondition.Simple);
            if (prefixLimit != 0)
            {
                var prefix = second.Slice(0, prefixLimit - 0); // ICU4N: Checked 2nd parameter
                int suffixStart = set.SpanBack(first, 0x7fffffff, SpanCondition.Simple);
                if (suffixStart == 0)
                {
                    if (doNormalize)
                    {
                        norm2.NormalizeSecondAndAppend(first, prefix);
                    }
                    else
                    {
                        norm2.Append(first, prefix);
                    }
                }
                else
                {
                    StringBuilder middle = new StringBuilder(
                            first.ToString(suffixStart, first.Length - suffixStart)); // ICU4N: Changed 2nd parameter
                    if (doNormalize)
                    {
                        norm2.NormalizeSecondAndAppend(middle, prefix);
                    }
                    else
                    {
                        norm2.Append(middle, prefix);
                    }
                    first.Delete(suffixStart, 0x7fffffff - suffixStart).Append(middle); // ICU4N: Corrected 2nd parameter of Delete
                }
            }
            if (prefixLimit < second.Length)
            {
                var rest = second.Slice(prefixLimit, second.Length - prefixLimit); // ICU4N: Corrected 2nd parameter
                if (doNormalize)
                {
                    Normalize(rest, first, SpanCondition.NotContained);
                }
                else
                {
                    first.Append(rest);
                }
            }
            return first;
        }


        private void NormalizeSecondAndAppend(ref ValueStringBuilder first, ReadOnlySpan<char> second,
                                               bool doNormalize)
        {
            if (MemoryHelper.AreSame(first.RawChars, second))
            {
                throw new ArgumentException($"'{nameof(first)}' cannot be the same memory location as '{nameof(second)}'");
            }
            if (first.Length == 0)
            {
                if (doNormalize)
                {
                    Normalize(second, ref first);
                    return;
                }
                else
                {
                    first.Append(second);
                    return;
                }
            }
            // merge the in-filter suffix of the first string with the in-filter prefix of the second
            int prefixLimit = set.Span(second, 0, SpanCondition.Simple);
            if (prefixLimit != 0)
            {
                var prefix = second.Slice(0, prefixLimit - 0); // ICU4N: Checked 2nd parameter
                int suffixStart = set.SpanBack(first.AsSpan(), 0x7fffffff, SpanCondition.Simple);
                if (suffixStart == 0)
                {
                    if (doNormalize)
                    {
                        norm2.NormalizeSecondAndAppend(ref first, prefix);
                    }
                    else
                    {
                        norm2.Append(ref first, prefix);
                    }
                }
                else
                {
                    int middleLength = (first.Length - suffixStart) + prefix.Length + 16;
                    ValueStringBuilder middle = middleLength <= CharStackBufferSize
                        ? new ValueStringBuilder(stackalloc char[middleLength])
                        : new ValueStringBuilder(middleLength);
                    try
                    {
                        middle.Append(first.AsSpan(suffixStart, first.Length - suffixStart)); // ICU4N: Changed 2nd parameter
                        if (doNormalize)
                        {
                            norm2.NormalizeSecondAndAppend(ref middle, prefix);
                        }
                        else
                        {
                            norm2.Append(ref middle, prefix);
                        }
                        first.Delete(suffixStart, 0x7fffffff - suffixStart); // ICU4N: Corrected 2nd parameter of Delete
                        unsafe
                        {
                            first.Append(new ReadOnlySpan<char>(middle.GetCharsPointer(), middle.Length));
                        }
                    }
                    finally
                    {
                        middle.Dispose();
                    }
                }
            }
            if (prefixLimit < second.Length)
            {
                var rest = second.Slice(prefixLimit, second.Length - prefixLimit); // ICU4N: Corrected 2nd parameter
                if (doNormalize)
                {
                    Normalize(rest, ref first, SpanCondition.NotContained);
                }
                else
                {
                    first.Append(rest);
                }
            }
        }

        #endregion NormalizeSecondAndAppend(StringBuilder, ICharSequence, bool)

        public override bool TryNormalize(ReadOnlySpan<char> source, Span<char> destination, out int charsLength)
        {
            throw new NotImplementedException(); // ICU4N TODO: Implement
        }

        public override bool TryNormalizeSecondAndConcat(ReadOnlySpan<char> first, ReadOnlySpan<char> second, Span<char> destination, out int charsLength)
        {
            throw new NotImplementedException(); // ICU4N TODO: Implement
        }

        public override bool TryConcat(ReadOnlySpan<char> first, ReadOnlySpan<char> second, Span<char> destination, out int charsLength)
        {
            throw new NotImplementedException(); // ICU4N TODO: Implement
        }

        private Normalizer2 norm2;
        private UnicodeSet set;
    }
}
