﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using J2N.Text;
using System.Text;

namespace ICU4N.Text
{
    public abstract partial class Normalizer2
    {

        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        /// <stable>ICU 4.4</stable>
        public abstract StringBuilder Normalize(string src, StringBuilder dest);

        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        /// <stable>ICU 4.4</stable>
        public abstract StringBuilder Normalize(StringBuilder src, StringBuilder dest);

        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        /// <stable>ICU 4.4</stable>
        public abstract StringBuilder Normalize(char[] src, StringBuilder dest);

        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        /// <stable>ICU 4.4</stable>
        public abstract StringBuilder Normalize(ICharSequence src, StringBuilder dest);

        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        /// <stable>ICU 4.6</stable>
        public abstract IAppendable Normalize(string src, IAppendable dest);

        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        /// <stable>ICU 4.6</stable>
        public abstract IAppendable Normalize(StringBuilder src, IAppendable dest);

        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        /// <stable>ICU 4.6</stable>
        public abstract IAppendable Normalize(char[] src, IAppendable dest);

        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        /// <stable>ICU 4.6</stable>
        public abstract IAppendable Normalize(ICharSequence src, IAppendable dest);

        /// <summary>
        /// Appends the normalized form of the <paramref name="second"/> string to the <paramref name="first"/> string
        /// (merging them at the boundary) and returns the <paramref name="first"/> string.
        /// The result is normalized if the <paramref name="first"/> string was normalized.
        /// The <paramref name="first"/> and <paramref name="second"/> strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, will be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        /// <stable>ICU 4.4</stable>
        public abstract StringBuilder NormalizeSecondAndAppend(
            StringBuilder first, string second);

        /// <summary>
        /// Appends the normalized form of the <paramref name="second"/> string to the <paramref name="first"/> string
        /// (merging them at the boundary) and returns the <paramref name="first"/> string.
        /// The result is normalized if the <paramref name="first"/> string was normalized.
        /// The <paramref name="first"/> and <paramref name="second"/> strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, will be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        /// <stable>ICU 4.4</stable>
        public abstract StringBuilder NormalizeSecondAndAppend(
            StringBuilder first, StringBuilder second);

        /// <summary>
        /// Appends the normalized form of the <paramref name="second"/> string to the <paramref name="first"/> string
        /// (merging them at the boundary) and returns the <paramref name="first"/> string.
        /// The result is normalized if the <paramref name="first"/> string was normalized.
        /// The <paramref name="first"/> and <paramref name="second"/> strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, will be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        /// <stable>ICU 4.4</stable>
        public abstract StringBuilder NormalizeSecondAndAppend(
            StringBuilder first, char[] second);

        /// <summary>
        /// Appends the normalized form of the <paramref name="second"/> string to the <paramref name="first"/> string
        /// (merging them at the boundary) and returns the <paramref name="first"/> string.
        /// The result is normalized if the <paramref name="first"/> string was normalized.
        /// The <paramref name="first"/> and <paramref name="second"/> strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, will be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        /// <stable>ICU 4.4</stable>
        public abstract StringBuilder NormalizeSecondAndAppend(
            StringBuilder first, ICharSequence second);

        /// <summary>
        /// Appends the <paramref name="second"/> string to the <paramref name="first"/> string
        /// (merging them at the boundary) and returns the <paramref name="first"/> string.
        /// The result is normalized if both the strings were normalized.
        /// The <paramref name="first"/> and <paramref name="second"/> strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, should be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        /// <stable>ICU 4.4</stable>
        public abstract StringBuilder Append(StringBuilder first, string second);

        /// <summary>
        /// Appends the <paramref name="second"/> string to the <paramref name="first"/> string
        /// (merging them at the boundary) and returns the <paramref name="first"/> string.
        /// The result is normalized if both the strings were normalized.
        /// The <paramref name="first"/> and <paramref name="second"/> strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, should be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        /// <stable>ICU 4.4</stable>
        public abstract StringBuilder Append(StringBuilder first, StringBuilder second);

        /// <summary>
        /// Appends the <paramref name="second"/> string to the <paramref name="first"/> string
        /// (merging them at the boundary) and returns the <paramref name="first"/> string.
        /// The result is normalized if both the strings were normalized.
        /// The <paramref name="first"/> and <paramref name="second"/> strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, should be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        /// <stable>ICU 4.4</stable>
        public abstract StringBuilder Append(StringBuilder first, char[] second);

        /// <summary>
        /// Appends the <paramref name="second"/> string to the <paramref name="first"/> string
        /// (merging them at the boundary) and returns the <paramref name="first"/> string.
        /// The result is normalized if both the strings were normalized.
        /// The <paramref name="first"/> and <paramref name="second"/> strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, should be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        /// <stable>ICU 4.4</stable>
        public abstract StringBuilder Append(StringBuilder first, ICharSequence second);

        /// <summary>
        /// Tests if the string is normalized.
        /// Internally, in cases where the <see cref="QuickCheck(string)"/> method would return "maybe"
        /// (which is only possible for the two COMPOSE modes) this method
        /// resolves to "yes" or "no" to provide a definitive result,
        /// at the cost of doing more work in those cases.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>true if <paramref name="s"/> is normalized.</returns>
        /// <stable>ICU 4.4</stable>
        public abstract bool IsNormalized(string s);

        /// <summary>
        /// Tests if the string is normalized.
        /// Internally, in cases where the <see cref="QuickCheck(StringBuilder)"/> method would return "maybe"
        /// (which is only possible for the two COMPOSE modes) this method
        /// resolves to "yes" or "no" to provide a definitive result,
        /// at the cost of doing more work in those cases.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>true if <paramref name="s"/> is normalized.</returns>
        /// <stable>ICU 4.4</stable>
        public abstract bool IsNormalized(StringBuilder s);

        /// <summary>
        /// Tests if the string is normalized.
        /// Internally, in cases where the <see cref="QuickCheck(char[])"/> method would return "maybe"
        /// (which is only possible for the two COMPOSE modes) this method
        /// resolves to "yes" or "no" to provide a definitive result,
        /// at the cost of doing more work in those cases.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>true if <paramref name="s"/> is normalized.</returns>
        /// <stable>ICU 4.4</stable>
        public abstract bool IsNormalized(char[] s);

        /// <summary>
        /// Tests if the string is normalized.
        /// Internally, in cases where the <see cref="QuickCheck(ICharSequence)"/> method would return "maybe"
        /// (which is only possible for the two COMPOSE modes) this method
        /// resolves to "yes" or "no" to provide a definitive result,
        /// at the cost of doing more work in those cases.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>true if <paramref name="s"/> is normalized.</returns>
        /// <stable>ICU 4.4</stable>
        public abstract bool IsNormalized(ICharSequence s);

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
        public abstract QuickCheckResult QuickCheck(string s);

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
        /// <stable>ICU 4.4</stable>
        public abstract QuickCheckResult QuickCheck(StringBuilder s);

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
        /// <stable>ICU 4.4</stable>
        public abstract QuickCheckResult QuickCheck(char[] s);

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
        public abstract QuickCheckResult QuickCheck(ICharSequence s);

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
        public abstract int SpanQuickCheckYes(string s);

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
        public abstract int SpanQuickCheckYes(StringBuilder s);

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
        public abstract int SpanQuickCheckYes(char[] s);

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
        public abstract int SpanQuickCheckYes(ICharSequence s);

    }
}