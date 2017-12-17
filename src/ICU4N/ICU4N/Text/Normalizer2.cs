using ICU4N.Impl;
using ICU4N.Support.IO;
using ICU4N.Support.Text;
using ICU4N.Util;
using System;
using System.IO;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
	/// Constants for normalization modes.
	/// For details about standard Unicode normalization forms
	/// and about the algorithms which are also used with custom mapping tables
	/// see http://www.unicode.org/unicode/reports/tr15/
	/// </summary>
	public enum Normalizer2Mode
    {
        /// <summary>
        /// Decomposition followed by composition.
        /// Same as standard NFC when using an "nfc" instance.
        /// Same as standard NFKC when using an "nfkc" instance.
        /// For details about standard Unicode normalization forms
        /// see http://www.unicode.org/unicode/reports/tr15/
        /// </summary>
        Compose = 0,

        /// <summary>
        /// Map, and reorder canonically.
        /// Same as standard NFD when using an "nfc" instance.
        /// Same as standard NFKD when using an "nfkc" instance.
        /// For details about standard Unicode normalization forms
        /// see http://www.unicode.org/unicode/reports/tr15/
        /// </summary>
        Decompose = 1,

        /// <summary>
        /// "Fast C or D" form.
        /// If a string is in this form, then further decomposition <i>without reordering</i>
        /// would yield the same form as DECOMPOSE.
        /// Text in "Fast C or D" form can be processed efficiently with data tables
        /// that are "canonically closed", that is, that provide equivalent data for
        /// equivalent text, without having to be fully normalized.
        /// <para/>
        /// Not a standard Unicode normalization form.
        /// <para/>
        /// Not a unique form: Different FCD strings can be canonically equivalent.
        /// <para/>
        /// For details see http://www.unicode.org/notes/tn5/#FCD
        /// </summary>
        FCD = 2,

        /// <summary>
        /// Compose only contiguously.
        /// Also known as "FCC" or "Fast C Contiguous".
        /// The result will often but not always be in NFC.
        /// The result will conform to FCD which is useful for processing.
        /// <para/>
        /// Not a standard Unicode normalization form.
        /// <para/>
        /// For details see http://www.unicode.org/notes/tn5/#FCC
        /// </summary>
        ComposeContiguous = 3
    }

    /// <summary>
    /// Unicode normalization functionality for standard Unicode normalization or
    /// for using custom mapping tables.
    /// All instances of this class are unmodifiable/immutable.
    /// The Normalizer2 class is not intended for public subclassing.
    /// </summary>
    /// <remarks>
    /// The primary functions are to produce a normalized string and to detect whether
    /// a string is already normalized.
    /// <para/>
    /// The most commonly used normalization forms are those defined in
    /// http://www.unicode.org/unicode/reports/tr15/
    /// However, this API supports additional normalization forms for specialized purposes.
    /// For example, NFKC_Casefold is provided via GetInstance("nfkc_cf", COMPOSE)
    /// and can be used in implementations of UTS #46.
    /// <para/>
    /// Not only are the standard compose and decompose modes supplied,
    /// but additional modes are provided as documented in the Mode enum.
    /// <para/>
    /// Some of the functions in this class identify normalization boundaries.
    /// At a normalization boundary, the portions of the string
    /// before it and starting from it do not interact and can be handled independently.
    /// <para/>
    /// The <see cref="SpanQuickCheckYes(string)"/> stops at a normalization boundary.
    /// When the goal is a normalized string, then the text before the boundary
    /// can be copied, and the remainder can be processed with <see cref="NormalizeSecondAndAppend(StringBuilder, string)"/>.
    /// <para/>
    /// The <see cref="HasBoundaryBefore(int)"/>, <see cref="HasBoundaryAfter(int)"/> and <see cref="IsInert(int)"/> functions test whether
    /// a character is guaranteed to be at a normalization boundary,
    /// regardless of context.
    /// This is used for moving from one normalization boundary to the next
    /// or preceding boundary, and for performing iterative normalization.
    /// <para/>
    /// Iterative normalization is useful when only a small portion of a
    /// longer string needs to be processed.
    /// For example, in ICU, iterative normalization is used by the NormalizationTransliterator
    /// (to avoid replacing already-normalized text) and ucol_nextSortKeyPart()
    /// (to process only the substring for which sort key bytes are computed).
    /// <para/>
    /// The set of normalization boundaries returned by these functions may not be
    /// complete: There may be more boundaries that could be returned.
    /// Different functions may return different boundaries.
    /// </remarks>
    public abstract class Normalizer2
    {
        internal IntPtr normalizerReference;

        /// <summary>
        /// Sole constructor.  (For invocation by subclass constructors,
        /// typically implicit.)
        /// <para/>
        /// This API is ICU internal only.
        /// </summary>
        internal Normalizer2()
        {
        }

        /// <summary>
        /// Returns a <see cref="Normalizer2"/> instance for Unicode NFC normalization.
        /// Same as GetInstance(null, "nfc", UNormalization2Mode.UNORM2_COMPOSE).
        /// Returns an unmodifiable singleton instance.
        /// </summary>
        /// <returns>The requested <see cref="Normalizer2"/>, if successful.</returns>
        public static Normalizer2 GetNFCInstance()
        {
            return Norm2AllModes.GetNFCInstance().Comp;
        }

        /// <summary>
        /// Returns a <see cref="Normalizer2"/> instance for Unicode NFD normalization.
        /// Same as GetInstance(null, "nfc", UNormalization2Mode.UNORM2_DECOMPOSE).
        /// Returns an unmodifiable singleton instance.
        /// </summary>
        /// <returns>The requested <see cref="Normalizer2"/>, if successful.</returns>
        public static Normalizer2 GetNFDInstance()
        {
            return Norm2AllModes.GetNFCInstance().Decomp;
        }

        /// <summary>
        /// Returns a <see cref="Normalizer2"/> instance for Unicode NFKC normalization.
        /// Same as GetInstance(null, "nfkc", UNormalization2Mode.UNORM2_COMPOSE).
        /// Returns an unmodifiable singleton instance.
        /// </summary>
        /// <returns>The requested <see cref="Normalizer2"/>, if successful.</returns>
        public static Normalizer2 GetNFKCInstance()
        {
            return Norm2AllModes.GetNFKCInstance().Comp;
        }

        /// <summary>
        /// Returns a <see cref="Normalizer2"/> instance for Unicode NFKD normalization.
        /// Same as GetInstance(null, "nfkc", UNormalization2Mode.UNORM2_DECOMPOSE).
        /// Returns an unmodifiable singleton instance.
        /// </summary>
        /// <returns>The requested <see cref="Normalizer2"/>, if successful.</returns>
        public static Normalizer2 GetNFKDInstance()
        {
            return Norm2AllModes.GetNFKCInstance().Decomp;
        }

        /// <summary>
        /// Returns a <see cref="Normalizer2"/> instance for Unicode NFKC_Casefold normalization.
        /// Same as GetInstance(null, "nfkc_cf", UNormalization2Mode.UNORM2_COMPOSE).
        /// Returns an unmodifiable singleton instance.
        /// </summary>
        /// <returns>The requested <see cref="Normalizer2"/>, if successful.</returns>
        public static Normalizer2 GetNFKCCasefoldInstance()
        {
            return Norm2AllModes.GetNFKC_CFInstance().Comp;
        }

        /// <summary>
        /// Returns a <see cref="Normalizer2"/> instance which uses the specified data file
        /// (an ICU data file if data=null, or else custom binary data)
        /// and which composes or decomposes text according to the specified mode.
        /// Returns an unmodifiable singleton instance.
        /// <list type="bullet">
        ///		<item><description>Use data=null for data files that are part of ICU's own data.</description></item>
        ///		<item><description>Use name="nfc" and COMPOSE/DECOMPOSE for Unicode standard NFC/NFD.</description></item>
        ///		<item><description>Use name="nfkc" and COMPOSE/DECOMPOSE for Unicode standard NFKC/NFKD.</description></item>
        ///		<item><description>Use name="nfkc_cf" and COMPOSE for Unicode standard NFKC_CF=NFKC_Casefold.</description></item>
        /// </list>
        /// <para/>
        /// If data!=null, then the binary data is read once and cached using the provided
        /// name as the key.
        /// If you know or expect the data to be cached already, you can use data!=null
        /// for non-ICU data as well.
        /// </summary>
        /// <param name="data">The binary, big-endian normalization (.nrm file) data, or null for ICU data.</param>
        /// <param name="name">"nfc" or "nfkc" or "nfkc_cf" or name of custom data file.</param>
        /// <param name="mode">Normalization mode (compose or decompose etc.)</param>
        /// <returns>The requested <see cref="Normalizer2"/>, if successful.</returns>
        public static Normalizer2 GetInstance(Stream data, string name, Normalizer2Mode mode)
        {
            ByteBuffer bytes = null;
            if (data != null)
            {
                try
                {
                    bytes = ICUBinary.GetByteBufferFromInputStreamAndCloseStream(data);
                }
                catch (IOException e)
                {
                    throw new ICUUncheckedIOException(e);
                }
            }
            Norm2AllModes all2Modes = Norm2AllModes.GetInstance(bytes, name);
            switch (mode)
            {
                case Normalizer2Mode.Compose: return all2Modes.Comp;
                case Normalizer2Mode.Decompose: return all2Modes.Decomp;
                case Normalizer2Mode.FCD: return all2Modes.Fcd;
                case Normalizer2Mode.ComposeContiguous: return all2Modes.Fcc;
                default: return null;  // will not occur
            }
        }

        /// <summary>
        /// Returns the normalized form of the source string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <returns>Normalized src.</returns>
        public virtual string Normalize(string src)
        {
            int spanLength = SpanQuickCheckYes(src);
            if (spanLength == src.Length)
            {
                return src;
            }
            StringBuilder sb = new StringBuilder(src.Length).Append(src, 0, spanLength - 0); // ICU4N: Checked 3rd parameter math
            return NormalizeSecondAndAppend(sb, src.Substring(spanLength, src.Length - spanLength)).ToString(); // ICU4N: Corrected 2nd substring parameter
        }

        /// <summary>
        /// Returns the normalized form of the source <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="src">Source <see cref="StringBuilder"/>.</param>
        /// <returns>Normalized src.</returns>
        public virtual string Normalize(StringBuilder src)
        {
            return Normalize(src, new StringBuilder(src.Length)).ToString();
        }

        /// <summary>
        /// Returns the normalized form of the source <see cref="T:char[]"/>.
        /// </summary>
        /// <param name="src">Source <see cref="T:char[]"/>.</param>
        /// <returns>Normalized src.</returns>
        public virtual string Normalize(char[] src)
        {
            return Normalize(src, new StringBuilder(src.Length)).ToString();
        }

        /// <summary>
        /// Returns the normalized form of the source <see cref="ICharSequence"/>.
        /// </summary>
        /// <param name="src">Source <see cref="ICharSequence"/>.</param>
        /// <returns>Normalized src.</returns>
        internal virtual string Normalize(ICharSequence src)
        {
            if (src is StringCharSequence)
            {
                // Fastpath: Do not construct a new string if the src is a string
                // and is already normalized.
                return Normalize(((StringCharSequence)src).String);
            }
            return Normalize(src, new StringBuilder(src.Length)).ToString();
        }

        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        public abstract StringBuilder Normalize(string src, StringBuilder dest);

        /// <summary>
        /// Writes the normalized form of the source <see cref="StringBuilder"/> to the destination string
        /// (replacing its contents) and returns the destination string.
        /// The source and destination strings must be different objects.
        /// </summary>
        /// <param name="src">Source <see cref="StringBuilder"/>.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        public abstract StringBuilder Normalize(StringBuilder src, StringBuilder dest);

        /// <summary>
        /// Writes the normalized form of the source <see cref="T:char[]"/> to the destination string
        /// (replacing its contents) and returns the destination string.
        /// The source and destination strings must be different objects.
        /// </summary>
        /// <param name="src">Source <see cref="T:char[]"/>.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        public abstract StringBuilder Normalize(char[] src, StringBuilder dest);

        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        internal abstract StringBuilder Normalize(ICharSequence src, StringBuilder dest);

        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        internal abstract IAppendable Normalize(ICharSequence src, IAppendable dest);

        /// <summary>
        /// Appends the normalized form of the second string to the first string
        /// (merging them at the boundary) and returns the first string.
        /// The result is normalized if the first string was normalized.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, will be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        public abstract StringBuilder NormalizeSecondAndAppend(
            StringBuilder first, string second);

        /// <summary>
        /// Appends the normalized form of the second string to the first string
        /// (merging them at the boundary) and returns the first string.
        /// The result is normalized if the first string was normalized.
        /// The first and second strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, will be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        public abstract StringBuilder NormalizeSecondAndAppend(
            StringBuilder first, StringBuilder second);

        /// <summary>
        /// Appends the normalized form of the second string to the first string
        /// (merging them at the boundary) and returns the first string.
        /// The result is normalized if the first string was normalized.
        /// The first and second strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, will be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        public abstract StringBuilder NormalizeSecondAndAppend(
            StringBuilder first, char[] second);

        /// <summary>
        /// Appends the normalized form of the second string to the first string
        /// (merging them at the boundary) and returns the first string.
        /// The result is normalized if the first string was normalized.
        /// The first and second strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, will be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        internal abstract StringBuilder NormalizeSecondAndAppend(
            StringBuilder first, ICharSequence second);

        /// <summary>
        /// Appends the second string to the first string
        /// (merging them at the boundary) and returns the first string.
        /// The result is normalized if both the strings were normalized.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, should be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        public abstract StringBuilder Append(StringBuilder first, string second);

        /// <summary>
        /// Appends the second string to the first string
        /// (merging them at the boundary) and returns the first string.
        /// The result is normalized if both the strings were normalized.
        /// The first and second strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, should be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        public abstract StringBuilder Append(StringBuilder first, StringBuilder second);

        /// <summary>
        /// Appends the second string to the first string
        /// (merging them at the boundary) and returns the first string.
        /// The result is normalized if both the strings were normalized.
        /// The first and second strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, should be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        public abstract StringBuilder Append(StringBuilder first, char[] second);

        /// <summary>
        /// Appends the second string to the first string
        /// (merging them at the boundary) and returns the first string.
        /// The result is normalized if both the strings were normalized.
        /// The first and second strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, should be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        internal abstract StringBuilder Append(StringBuilder first, ICharSequence second);

        /// <summary>
        /// Gets the decomposition mapping of <paramref name="codePoint"/>.
        /// Roughly equivalent to normalizing the <see cref="string"/> form of <paramref name="codePoint"/>
        /// on a DECOMPOSE <see cref="Normalizer2"/> instance, but much faster, and except that this function
        /// returns null if c does not have a decomposition mapping in this instance's data.
        /// This function is independent of the mode of the Normalizer2.
        /// </summary>
        /// <param name="codePoint">Code point.</param>
        /// <returns><paramref name="codePoint"/>'s decomposition mapping, if any; otherwise null.</returns>
        public abstract string GetDecomposition(int codePoint);

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
        /// When used on a standard NFKC <see cref="Normalizer2"/> instance,
        /// <see cref="GetRawDecomposition"/> returns the Unicode Decomposition_Mapping (dm) property.
        /// <para/>
        /// When used on a standard NFC <see cref="Normalizer2"/> instance,
        /// it returns the Decomposition_Mapping only if the Decomposition_Type (dt) is Canonical (Can);
        /// in this case, the result contains either one or two code points (=1..4 .NET chars).
        /// <para/>
        /// This function is independent of the mode of the <see cref="Normalizer2"/>.
        /// The default implementation returns null.
        /// </remarks>
        /// <param name="codePoint">Code point.</param>
        /// <returns><paramref name="codePoint"/>'s raw decomposition mapping, if any; otherwise null.</returns>
        public virtual string GetRawDecomposition(int codePoint) { return null; }

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
        public virtual int ComposePair(int a, int b) { return -1; }

        /// <summary>
        /// Gets the combining class of <paramref name="codePoint"/>.
        /// The default implementation returns 0
        /// but all standard implementations return the Unicode Canonical_Combining_Class value.
        /// </summary>
        /// <param name="codePoint">Code point.</param>
        /// <returns><paramref name="codePoint"/>'s combining class.</returns>
        public virtual int GetCombiningClass(int codePoint) { return 0; }

        /// <summary>
        /// Tests if the string is normalized.
        /// Internally, in cases where the <see cref="QuickCheck(string)"/> method would return "maybe"
        /// (which is only possible for the two COMPOSE modes) this method
        /// resolves to "yes" or "no" to provide a definitive result,
        /// at the cost of doing more work in those cases.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>true if s is normalized.</returns>
        public abstract bool IsNormalized(string s);

        /// <summary>
        /// Tests if the <see cref="StringBuilder"/> is normalized.
        /// Internally, in cases where the <see cref="QuickCheck(StringBuilder)"/> method would return "maybe"
        /// (which is only possible for the two COMPOSE modes) this method
        /// resolves to "yes" or "no" to provide a definitive result,
        /// at the cost of doing more work in those cases.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>true if s is normalized.</returns>
        public abstract bool IsNormalized(StringBuilder s);

        /// <summary>
        /// Tests if the <see cref="T:char[]"/> is normalized.
        /// Internally, in cases where the <see cref="QuickCheck(char[])"/> method would return "maybe"
        /// (which is only possible for the two COMPOSE modes) this method
        /// resolves to "yes" or "no" to provide a definitive result,
        /// at the cost of doing more work in those cases.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>true if s is normalized.</returns>
        public abstract bool IsNormalized(char[] s);

        /// <summary>
        /// Tests if the string is normalized.
        /// Internally, in cases where the <see cref="QuickCheck(ICharSequence)"/> method would return "maybe"
        /// (which is only possible for the two COMPOSE modes) this method
        /// resolves to "yes" or "no" to provide a definitive result,
        /// at the cost of doing more work in those cases.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>true if s is normalized.</returns>
        internal abstract bool IsNormalized(ICharSequence s);

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
        public abstract NormalizerQuickCheckResult QuickCheck(string s);

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
        public abstract NormalizerQuickCheckResult QuickCheck(StringBuilder s);

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
        public abstract NormalizerQuickCheckResult QuickCheck(char[] s);

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
        internal abstract NormalizerQuickCheckResult QuickCheck(ICharSequence s);

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
        internal abstract int SpanQuickCheckYes(ICharSequence s);

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
        public abstract bool HasBoundaryBefore(int character);

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
        public abstract bool HasBoundaryAfter(int character);

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
        public abstract bool IsInert(int character);
    }
}
