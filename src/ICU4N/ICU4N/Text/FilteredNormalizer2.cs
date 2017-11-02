using ICU4N.Support.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
	/// Normalization filtered by a <see cref="UnicodeSet"/>.
	/// Normalizes portions of the text contained in the filter set and leaves
	/// portions not contained in the filter set unchanged.
	/// Filtering is done via UnicodeSet.Span(..., UnicodeSet.SpanCondition.SIMPLE).
	/// Not-in-the-filter text is treated as "is normalized" and "quick check yes".
	/// This class implements all of (and only) the Normalizer2 API.
	/// An instance of this class is unmodifiable/immutable.
	/// </summary>
    /// <stable>ICU 4.4</stable>
    /// <author>Markus W. Scherer</author>
    public class FilteredNormalizer2 : Normalizer2
    {
        /// <summary>
        /// Constructs a filtered normalizer wrapping any <see cref="Normalizer2"/> instance
        /// and a filter set.
        /// Both are aliased and must not be modified or deleted while this object
        /// is used.
        /// The filter set should be frozen; otherwise the performance will suffer greatly.
        /// </summary>
        /// <param name="n2">Wrapped Normalizer2 instance.</param>
        /// <param name="filterSet">UnicodeSet which determines the characters to be normalized.</param>
        /// <stable>ICU 4.4</stable>
        public FilteredNormalizer2(Normalizer2 n2, UnicodeSet filterSet)
        {
            norm2 = n2;
            set = filterSet;
        }

        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        public override StringBuilder Normalize(string src, StringBuilder dest)
        {
            dest.Length = 0;
            Normalize(src.ToCharSequence(), dest.ToAppendable(), UnicodeSet.SpanCondition.SIMPLE);
            return dest;
        }

        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        public override StringBuilder Normalize(StringBuilder src, StringBuilder dest)
        {
            if (src == dest)
            {
                throw new ArgumentException("'src' cannot be the same StringBuilder instance as 'dest'");
            }
            dest.Length = 0;
            Normalize(src.ToCharSequence(), dest.ToAppendable(), UnicodeSet.SpanCondition.SIMPLE);
            return dest;
        }

        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        public override StringBuilder Normalize(char[] src, StringBuilder dest)
        {
            dest.Length = 0;
            Normalize(src.ToCharSequence(), dest.ToAppendable(), UnicodeSet.SpanCondition.SIMPLE);
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
        internal override StringBuilder Normalize(ICharSequence src, StringBuilder dest)
        {
            if (src is StringBuilderCharSequence && ((StringBuilderCharSequence)src).StringBuilder == dest)
            {
                throw new ArgumentException("'src' cannot be the same StringBuilder instance as 'dest'");
            }
            dest.Length = 0;
            Normalize(src, dest.ToAppendable(), UnicodeSet.SpanCondition.SIMPLE);
            return dest;
        }

        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        /// <stable>ICU 4.6</stable>
        internal override IAppendable Normalize(ICharSequence src, IAppendable dest)
        {
            if (dest == src)
            {
                throw new ArgumentException();
            }
            return Normalize(src, dest, UnicodeSet.SpanCondition.SIMPLE);
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
                StringBuilder first, string second)
        {
            return NormalizeSecondAndAppend(first, second.ToCharSequence(), true);
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
                StringBuilder first, StringBuilder second)
        {
            return NormalizeSecondAndAppend(first, second.ToCharSequence(), true);
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
            StringBuilder first, char[] second)
        {
            return NormalizeSecondAndAppend(first, second.ToCharSequence(), true);
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
        internal override StringBuilder NormalizeSecondAndAppend(
            StringBuilder first, ICharSequence second)
        {
            return NormalizeSecondAndAppend(first, second, true);
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
        public override StringBuilder Append(StringBuilder first, string second)
        {
            return NormalizeSecondAndAppend(first, second.ToCharSequence(), false);
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
        public override StringBuilder Append(StringBuilder first, StringBuilder second)
        {
            return NormalizeSecondAndAppend(first, second.ToCharSequence(), false);
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
        public override StringBuilder Append(StringBuilder first, char[] second)
        {
            return NormalizeSecondAndAppend(first, second.ToCharSequence(), false);
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
        internal override StringBuilder Append(StringBuilder first, ICharSequence second)
        {
            return NormalizeSecondAndAppend(first, second, false);
        }

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
        public override string GetDecomposition(int c)
        {
            return set.Contains(c) ? norm2.GetDecomposition(c) : null;
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
        public override string GetRawDecomposition(int c)
        {
            return set.Contains(c) ? norm2.GetRawDecomposition(c) : null;
        }

        /// <summary>
        /// Performs pairwise composition of a &amp; b and returns the composite if there is one.
        /// </summary>
        /// <remarks>
        /// Returns a composite code point c only if c has a two-way mapping to a+b.
        /// In standard Unicode normalization, this means that
        /// c has a canonical decomposition to a+b
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
        public override int GetCombiningClass(int c)
        {
            return set.Contains(c) ? norm2.GetCombiningClass(c) : 0;
        }

        /// <summary>
        /// Tests if the string is normalized.
        /// Internally, in cases where the <see cref="QuickCheck(string)"/> method would return "maybe"
        /// (which is only possible for the two COMPOSE modes) this method
        /// resolves to "yes" or "no" to provide a definitive result,
        /// at the cost of doing more work in those cases.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>true if s is normalized.</returns>
        public override bool IsNormalized(string s)
        {
            UnicodeSet.SpanCondition spanCondition = UnicodeSet.SpanCondition.SIMPLE;
            for (int prevSpanLimit = 0; prevSpanLimit < s.Length;)
            {
                int spanLimit = set.Span(s, prevSpanLimit, spanCondition);
                if (spanCondition == UnicodeSet.SpanCondition.NOT_CONTAINED)
                {
                    spanCondition = UnicodeSet.SpanCondition.SIMPLE;
                }
                else
                {
                    if (!norm2.IsNormalized(s.Substring(prevSpanLimit, spanLimit - prevSpanLimit)))
                    {
                        return false;
                    }
                    spanCondition = UnicodeSet.SpanCondition.NOT_CONTAINED;
                }
                prevSpanLimit = spanLimit;
            }
            return true;
        }

        /// <summary>
        /// Tests if the string is normalized.
        /// Internally, in cases where the <see cref="QuickCheck(StringBuilder)"/> method would return "maybe"
        /// (which is only possible for the two COMPOSE modes) this method
        /// resolves to "yes" or "no" to provide a definitive result,
        /// at the cost of doing more work in those cases.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>true if s is normalized.</returns>
        public override bool IsNormalized(StringBuilder s)
        {
            UnicodeSet.SpanCondition spanCondition = UnicodeSet.SpanCondition.SIMPLE;
            for (int prevSpanLimit = 0; prevSpanLimit < s.Length;)
            {
                int spanLimit = set.Span(s, prevSpanLimit, spanCondition);
                if (spanCondition == UnicodeSet.SpanCondition.NOT_CONTAINED)
                {
                    spanCondition = UnicodeSet.SpanCondition.SIMPLE;
                }
                else
                {
                    if (!norm2.IsNormalized(s.ToString(prevSpanLimit, spanLimit - prevSpanLimit)))
                    {
                        return false;
                    }
                    spanCondition = UnicodeSet.SpanCondition.NOT_CONTAINED;
                }
                prevSpanLimit = spanLimit;
            }
            return true;
        }

        /// <summary>
        /// Tests if the string is normalized.
        /// Internally, in cases where the <see cref="QuickCheck(char[])"/> method would return "maybe"
        /// (which is only possible for the two COMPOSE modes) this method
        /// resolves to "yes" or "no" to provide a definitive result,
        /// at the cost of doing more work in those cases.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>true if s is normalized.</returns>
        public override bool IsNormalized(char[] s)
        {
            UnicodeSet.SpanCondition spanCondition = UnicodeSet.SpanCondition.SIMPLE;
            for (int prevSpanLimit = 0; prevSpanLimit < s.Length;)
            {
                int spanLimit = set.Span(s, prevSpanLimit, spanCondition);
                if (spanCondition == UnicodeSet.SpanCondition.NOT_CONTAINED)
                {
                    spanCondition = UnicodeSet.SpanCondition.SIMPLE;
                }
                else
                {
                    if (!norm2.IsNormalized(new string(s, prevSpanLimit, spanLimit - prevSpanLimit)))
                    {
                        return false;
                    }
                    spanCondition = UnicodeSet.SpanCondition.NOT_CONTAINED;
                }
                prevSpanLimit = spanLimit;
            }
            return true;
        }

        /// <summary>
        /// Tests if the string is normalized.
        /// Internally, in cases where the <see cref="QuickCheck(ICharSequence)"/> method would return "maybe"
        /// (which is only possible for the two COMPOSE modes) this method
        /// resolves to "yes" or "no" to provide a definitive result,
        /// at the cost of doing more work in those cases.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>true if s is normalized.</returns>
        /// <stable>ICU 4.4</stable>
        internal override bool IsNormalized(ICharSequence s)
        {
            UnicodeSet.SpanCondition spanCondition = UnicodeSet.SpanCondition.SIMPLE;
            for (int prevSpanLimit = 0; prevSpanLimit < s.Length;)
            {
                int spanLimit = set.Span(s, prevSpanLimit, spanCondition);
                if (spanCondition == UnicodeSet.SpanCondition.NOT_CONTAINED)
                {
                    spanCondition = UnicodeSet.SpanCondition.SIMPLE;
                }
                else
                {
                    if (!norm2.IsNormalized(s.SubSequence(prevSpanLimit, spanLimit)))
                    {
                        return false;
                    }
                    spanCondition = UnicodeSet.SpanCondition.NOT_CONTAINED;
                }
                prevSpanLimit = spanLimit;
            }
            return true;
        }

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
        public override NormalizerQuickCheckResult QuickCheck(string s)
        {
            NormalizerQuickCheckResult result = NormalizerQuickCheckResult.Yes;
            UnicodeSet.SpanCondition spanCondition = UnicodeSet.SpanCondition.SIMPLE;
            for (int prevSpanLimit = 0; prevSpanLimit < s.Length;)
            {
                int spanLimit = set.Span(s, prevSpanLimit, spanCondition);
                if (spanCondition == UnicodeSet.SpanCondition.NOT_CONTAINED)
                {
                    spanCondition = UnicodeSet.SpanCondition.SIMPLE;
                }
                else
                {
                    NormalizerQuickCheckResult qcResult =
                        norm2.QuickCheck(s.Substring(prevSpanLimit, spanLimit - prevSpanLimit));
                    if (qcResult == NormalizerQuickCheckResult.No)
                    {
                        return qcResult;
                    }
                    else if (qcResult == NormalizerQuickCheckResult.Maybe)
                    {
                        result = qcResult;
                    }
                    spanCondition = UnicodeSet.SpanCondition.NOT_CONTAINED;
                }
                prevSpanLimit = spanLimit;
            }
            return result;
        }

        /// <summary>
        /// Tests if the string is normalized.
        /// For the two COMPOSE modes, the result could be "maybe" in cases that
        /// would take a little more work to resolve definitively.
        /// Use <see cref="SpanQuickCheckYes(StringBuilder)"/> and
        /// <see cref="NormalizeSecondAndAppend(StringBuilder, StringBuilder)"/> for a faster
        /// combination of quick check + normalization, to avoid
        /// re-checking the "yes" prefix.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>The quick check result.</returns>
        public override NormalizerQuickCheckResult QuickCheck(StringBuilder s)
        {
            NormalizerQuickCheckResult result = NormalizerQuickCheckResult.Yes;
            UnicodeSet.SpanCondition spanCondition = UnicodeSet.SpanCondition.SIMPLE;
            for (int prevSpanLimit = 0; prevSpanLimit < s.Length;)
            {
                int spanLimit = set.Span(s, prevSpanLimit, spanCondition);
                if (spanCondition == UnicodeSet.SpanCondition.NOT_CONTAINED)
                {
                    spanCondition = UnicodeSet.SpanCondition.SIMPLE;
                }
                else
                {
                    NormalizerQuickCheckResult qcResult =
                        norm2.QuickCheck(s.ToString(prevSpanLimit, spanLimit - prevSpanLimit));
                    if (qcResult == NormalizerQuickCheckResult.No)
                    {
                        return qcResult;
                    }
                    else if (qcResult == NormalizerQuickCheckResult.Maybe)
                    {
                        result = qcResult;
                    }
                    spanCondition = UnicodeSet.SpanCondition.NOT_CONTAINED;
                }
                prevSpanLimit = spanLimit;
            }
            return result;
        }

        /// <summary>
        /// Tests if the string is normalized.
        /// For the two COMPOSE modes, the result could be "maybe" in cases that
        /// would take a little more work to resolve definitively.
        /// Use <see cref="SpanQuickCheckYes(char[])"/> and
        /// <see cref="NormalizeSecondAndAppend(StringBuilder, char[])"/> for a faster
        /// combination of quick check + normalization, to avoid
        /// re-checking the "yes" prefix.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>The quick check result.</returns>
        public override NormalizerQuickCheckResult QuickCheck(char[] s)
        {
            NormalizerQuickCheckResult result = NormalizerQuickCheckResult.Yes;
            UnicodeSet.SpanCondition spanCondition = UnicodeSet.SpanCondition.SIMPLE;
            for (int prevSpanLimit = 0; prevSpanLimit < s.Length;)
            {
                int spanLimit = set.Span(s, prevSpanLimit, spanCondition);
                if (spanCondition == UnicodeSet.SpanCondition.NOT_CONTAINED)
                {
                    spanCondition = UnicodeSet.SpanCondition.SIMPLE;
                }
                else
                {
                    NormalizerQuickCheckResult qcResult =
                        norm2.QuickCheck(new string(s, prevSpanLimit, spanLimit - prevSpanLimit));
                    if (qcResult == NormalizerQuickCheckResult.No)
                    {
                        return qcResult;
                    }
                    else if (qcResult == NormalizerQuickCheckResult.Maybe)
                    {
                        result = qcResult;
                    }
                    spanCondition = UnicodeSet.SpanCondition.NOT_CONTAINED;
                }
                prevSpanLimit = spanLimit;
            }
            return result;
        }

        /// <summary>
        /// Tests if the string is normalized.
        /// For the two COMPOSE modes, the result could be "maybe" in cases that
        /// would take a little more work to resolve definitively.
        /// Use <see cref="SpanQuickCheckYes(ICharSequence)"/> and
        /// <see cref="NormalizeSecondAndAppend(StringBuilder, ICharSequence)"/> for a faster
        /// combination of quick check + normalization, to avoid
        /// re-checking the "yes" prefix.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>The quick check result.</returns>
        /// <stable>ICU 4.4</stable>
        internal override NormalizerQuickCheckResult QuickCheck(ICharSequence s)
        {
            NormalizerQuickCheckResult result = NormalizerQuickCheckResult.Yes;
            UnicodeSet.SpanCondition spanCondition = UnicodeSet.SpanCondition.SIMPLE;
            for (int prevSpanLimit = 0; prevSpanLimit < s.Length;)
            {
                int spanLimit = set.Span(s, prevSpanLimit, spanCondition);
                if (spanCondition == UnicodeSet.SpanCondition.NOT_CONTAINED)
                {
                    spanCondition = UnicodeSet.SpanCondition.SIMPLE;
                }
                else
                {
                    NormalizerQuickCheckResult qcResult =
                        norm2.QuickCheck(s.SubSequence(prevSpanLimit, spanLimit));
                    if (qcResult == NormalizerQuickCheckResult.No)
                    {
                        return qcResult;
                    }
                    else if (qcResult == NormalizerQuickCheckResult.Maybe)
                    {
                        result = qcResult;
                    }
                    spanCondition = UnicodeSet.SpanCondition.NOT_CONTAINED;
                }
                prevSpanLimit = spanLimit;
            }
            return result;
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
        /// copy that prefix and use <see cref="NormalizeSecondAndAppend(StringBuilder, string)"/> for the remainder.
        /// </remarks>
        /// <param name="s">Input string.</param>
        /// <returns>"yes" span end index.</returns>
        /// <stable>ICU 4.4</stable>
        public override int SpanQuickCheckYes(string s)
        {
            UnicodeSet.SpanCondition spanCondition = UnicodeSet.SpanCondition.SIMPLE;
            for (int prevSpanLimit = 0; prevSpanLimit < s.Length;)
            {
                int spanLimit = set.Span(s, prevSpanLimit, spanCondition);
                if (spanCondition == UnicodeSet.SpanCondition.NOT_CONTAINED)
                {
                    spanCondition = UnicodeSet.SpanCondition.SIMPLE;
                }
                else
                {
                    int yesLimit =
                        prevSpanLimit +
                        norm2.SpanQuickCheckYes(s.Substring(prevSpanLimit, spanLimit - prevSpanLimit));
                    if (yesLimit < spanLimit)
                    {
                        return yesLimit;
                    }
                    spanCondition = UnicodeSet.SpanCondition.NOT_CONTAINED;
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
        /// copy that prefix and use <see cref="NormalizeSecondAndAppend(StringBuilder, StringBuilder)"/> for the remainder.
        /// </remarks>
        /// <param name="s">Input string.</param>
        /// <returns>"yes" span end index.</returns>
        /// <stable>ICU 4.4</stable>
        public override int SpanQuickCheckYes(StringBuilder s)
        {
            UnicodeSet.SpanCondition spanCondition = UnicodeSet.SpanCondition.SIMPLE;
            for (int prevSpanLimit = 0; prevSpanLimit < s.Length;)
            {
                int spanLimit = set.Span(s, prevSpanLimit, spanCondition);
                if (spanCondition == UnicodeSet.SpanCondition.NOT_CONTAINED)
                {
                    spanCondition = UnicodeSet.SpanCondition.SIMPLE;
                }
                else
                {
                    int yesLimit =
                        prevSpanLimit +
                        norm2.SpanQuickCheckYes(s.ToString(prevSpanLimit, spanLimit - prevSpanLimit));
                    if (yesLimit < spanLimit)
                    {
                        return yesLimit;
                    }
                    spanCondition = UnicodeSet.SpanCondition.NOT_CONTAINED;
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
        /// copy that prefix and use <see cref="NormalizeSecondAndAppend(StringBuilder, char[])"/> for the remainder.
        /// </remarks>
        /// <param name="s">Input string.</param>
        /// <returns>"yes" span end index.</returns>
        /// <stable>ICU 4.4</stable>
        public override int SpanQuickCheckYes(char[] s)
        {
            UnicodeSet.SpanCondition spanCondition = UnicodeSet.SpanCondition.SIMPLE;
            for (int prevSpanLimit = 0; prevSpanLimit < s.Length;)
            {
                int spanLimit = set.Span(s, prevSpanLimit, spanCondition);
                if (spanCondition == UnicodeSet.SpanCondition.NOT_CONTAINED)
                {
                    spanCondition = UnicodeSet.SpanCondition.SIMPLE;
                }
                else
                {
                    int yesLimit =
                        prevSpanLimit +
                        norm2.SpanQuickCheckYes(new string(s, prevSpanLimit, spanLimit - prevSpanLimit));
                    if (yesLimit < spanLimit)
                    {
                        return yesLimit;
                    }
                    spanCondition = UnicodeSet.SpanCondition.NOT_CONTAINED;
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
        /// copy that prefix and use <see cref="NormalizeSecondAndAppend(StringBuilder, ICharSequence)"/> for the remainder.
        /// </remarks>
        /// <param name="s">Input string.</param>
        /// <returns>"yes" span end index.</returns>
        /// <stable>ICU 4.4</stable>
        internal override int SpanQuickCheckYes(ICharSequence s)
        {
            UnicodeSet.SpanCondition spanCondition = UnicodeSet.SpanCondition.SIMPLE;
            for (int prevSpanLimit = 0; prevSpanLimit < s.Length;)
            {
                int spanLimit = set.Span(s, prevSpanLimit, spanCondition);
                if (spanCondition == UnicodeSet.SpanCondition.NOT_CONTAINED)
                {
                    spanCondition = UnicodeSet.SpanCondition.SIMPLE;
                }
                else
                {
                    int yesLimit =
                        prevSpanLimit +
                        norm2.SpanQuickCheckYes(s.SubSequence(prevSpanLimit, spanLimit));
                    if (yesLimit < spanLimit)
                    {
                        return yesLimit;
                    }
                    spanCondition = UnicodeSet.SpanCondition.NOT_CONTAINED;
                }
                prevSpanLimit = spanLimit;
            }
            return s.Length;
        }

        /// <summary>
        /// Tests if the character always has a normalization boundary before it,
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
        public override bool HasBoundaryBefore(int c)
        {
            return !set.Contains(c) || norm2.HasBoundaryBefore(c);
        }

        /// <summary>
        /// Tests if the character always has a normalization boundary after it,
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
        public override bool HasBoundaryAfter(int c)
        {
            return !set.Contains(c) || norm2.HasBoundaryAfter(c);
        }

        /// <summary>
        /// Tests if the character is normalization-inert.
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
        public override bool IsInert(int c)
        {
            return !set.Contains(c) || norm2.IsInert(c);
        }

        // Internal: No argument checking, and appends to dest.
        // Pass as input spanCondition the one that is likely to yield a non-zero
        // span length at the start of src.
        // For set=[:age=3.2:], since almost all common characters were in Unicode 3.2,
        // UnicodeSet.SpanCondition.SIMPLE should be passed in for the start of src
        // and UnicodeSet.SpanCondition.NOT_CONTAINED should be passed in if we continue after
        // an in-filter prefix.
        private IAppendable Normalize(ICharSequence src, IAppendable dest,
                                     UnicodeSet.SpanCondition spanCondition)
        {
            // Don't throw away destination buffer between iterations.
            StringBuilder tempDest = new StringBuilder();
            try
            {
                for (int prevSpanLimit = 0; prevSpanLimit < src.Length;)
                {
                    int spanLimit = set.Span(src, prevSpanLimit, spanCondition);
                    int spanLength = spanLimit - prevSpanLimit;
                    if (spanCondition == UnicodeSet.SpanCondition.NOT_CONTAINED)
                    {
                        if (spanLength != 0)
                        {
                            dest.Append(src, prevSpanLimit, spanLimit);
                        }
                        spanCondition = UnicodeSet.SpanCondition.SIMPLE;
                    }
                    else
                    {
                        if (spanLength != 0)
                        {
                            // Not norm2.normalizeSecondAndAppend() because we do not want
                            // to modify the non-filter part of dest.
                            dest.Append(norm2.Normalize(src.SubSequence(prevSpanLimit, spanLimit), tempDest));
                        }
                        spanCondition = UnicodeSet.SpanCondition.NOT_CONTAINED;
                    }
                    prevSpanLimit = spanLimit;
                }
            }
            catch (IOException e)
            {
                throw new ICUUncheckedIOException(e);
            }
            return dest;
        }

        private StringBuilder NormalizeSecondAndAppend(StringBuilder first, ICharSequence second,
                                                       bool doNormalize)
        {
            if (second is StringBuilderCharSequence && ((StringBuilderCharSequence)second).StringBuilder == first)
            {
                throw new ArgumentException();
            }
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
            int prefixLimit = set.Span(second, 0, UnicodeSet.SpanCondition.SIMPLE);
            if (prefixLimit != 0)
            {
                ICharSequence prefix = second.SubSequence(0, prefixLimit);
                int suffixStart = set.SpanBack(first, 0x7fffffff, UnicodeSet.SpanCondition.SIMPLE);
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
                            first.ToString(suffixStart, first.Length - suffixStart));
                    if (doNormalize)
                    {
                        norm2.NormalizeSecondAndAppend(middle, prefix);
                    }
                    else
                    {
                        norm2.Append(middle, prefix);
                    }
                    first.Delete(suffixStart, 0x7fffffff).Append(middle);
                }
            }
            if (prefixLimit < second.Length)
            {
                ICharSequence rest = second.SubSequence(prefixLimit, second.Length);
                if (doNormalize)
                {
                    Normalize(rest, first.ToAppendable(), UnicodeSet.SpanCondition.NOT_CONTAINED);
                }
                else
                {
                    first.Append(rest);
                }
            }
            return first;
        }

        private Normalizer2 norm2;
        private UnicodeSet set;
    }
}
