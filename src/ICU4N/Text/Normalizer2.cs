using ICU4N.Impl;
using ICU4N.Support.Text;
using J2N.IO;
using J2N.Text;
using System;
using System.IO;
using System.Runtime.CompilerServices;
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
        /// would yield the same form as <see cref="Decompose"/>.
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
    /// <stable>ICU 4.4</stable>
    /// <author>Markus W. Scherer</author>
    public abstract partial class Normalizer2
    {
        internal const int CharStackBufferSize = 32;
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
        /// Same as GetInstance(null, "nfc", Normalizer2Mode.Compose).
        /// Returns an unmodifiable singleton instance.
        /// </summary>
        /// <returns>The requested <see cref="Normalizer2"/>, if successful.</returns>
        /// <stable>ICU 49</stable>
        public static Normalizer2 NFCInstance
            => Norm2AllModes.NFCInstance.Comp;

        /// <summary>
        /// Returns a <see cref="Normalizer2"/> instance for Unicode NFD normalization.
        /// Same as GetInstance(null, "nfc", Normalizer2Mode.Decompose).
        /// Returns an unmodifiable singleton instance.
        /// </summary>
        /// <returns>The requested <see cref="Normalizer2"/>, if successful.</returns>
        /// <stable>ICU 49</stable>
        public static Normalizer2 NFDInstance
            => Norm2AllModes.NFCInstance.Decomp;

        /// <summary>
        /// Returns a <see cref="Normalizer2"/> instance for Unicode NFKC normalization.
        /// Same as GetInstance(null, "nfkc", Normalizer2Mode.Compose).
        /// Returns an unmodifiable singleton instance.
        /// </summary>
        /// <returns>The requested <see cref="Normalizer2"/>, if successful.</returns>
        /// <stable>ICU 49</stable>
        public static Normalizer2 NFKCInstance
            => Norm2AllModes.NFKCInstance.Comp;

        /// <summary>
        /// Returns a <see cref="Normalizer2"/> instance for Unicode NFKD normalization.
        /// Same as GetInstance(null, "nfkc", Normalizer2Mode.Decompose).
        /// Returns an unmodifiable singleton instance.
        /// </summary>
        /// <returns>The requested <see cref="Normalizer2"/>, if successful.</returns>
        /// <stable>ICU 49</stable>
        public static Normalizer2 NFKDInstance
            => Norm2AllModes.NFKCInstance.Decomp;

        /// <summary>
        /// Returns a <see cref="Normalizer2"/> instance for Unicode NFKC_Casefold normalization.
        /// Same as GetInstance(null, "nfkc_cf", Normalizer2Mode.Compose).
        /// Returns an unmodifiable singleton instance.
        /// </summary>
        /// <returns>The requested <see cref="Normalizer2"/>, if successful.</returns>
        /// <stable>ICU 49</stable>
        public static Normalizer2 NFKCCaseFoldInstance
            => Norm2AllModes.NFKC_CFInstance.Comp;

        /// <summary>
        /// Returns a <see cref="Normalizer2"/> instance which uses the specified data file
        /// (an ICU data file if data=null, or else custom binary data)
        /// and which composes or decomposes text according to the specified mode.
        /// Returns an unmodifiable singleton instance.
        /// <list type="bullet">
        ///     <item><description>Use data=null for data files that are part of ICU's own data.</description></item>
        ///     <item><description>Use name="nfc" and COMPOSE/DECOMPOSE for Unicode standard NFC/NFD.</description></item>
        ///     <item><description>Use name="nfkc" and COMPOSE/DECOMPOSE for Unicode standard NFKC/NFKD.</description></item>
        ///     <item><description>Use name="nfkc_cf" and COMPOSE for Unicode standard NFKC_CF=NFKC_Casefold.</description></item>
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
        /// <stable>ICU 4.4</stable>
        public static Normalizer2 GetInstance(Stream data, string name, Normalizer2Mode mode)
        {
            ByteBuffer bytes = null;
            if (data != null)
            {
                // ICU4N: Removed unnecessary try/catch
                bytes = ICUBinary.GetByteBufferFromStreamAndDisposeStream(data);
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
        /// <returns>Normalized <paramref name="src"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="src"/> is <c>null</c>.</exception>
        /// <stable>ICU 4.4</stable>
        public virtual string Normalize(string src)
        {
            int spanLength = SpanQuickCheckYes(src); // ICU4N: SpanQuickCheckYes does the null check
            if (spanLength == src.Length)
            {
                return src;
            }
            ValueStringBuilder sb = src.Length <= CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                : new ValueStringBuilder(src.Length);
            try
            {
                if (spanLength != 0)
                {
                    sb.Append(src.AsSpan(0, spanLength - 0)); // ICU4N: Checked 2nd parameter math
                    NormalizeSecondAndAppend(ref sb, src.AsSpan(spanLength, src.Length - spanLength)); // ICU4N: Corrected 2nd substring parameter
                    return sb.ToString();
                }
                Normalize(src.AsSpan(), ref sb);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Returns the normalized form of the source <see cref="ReadOnlySpan{Char}"/>.
        /// </summary>
        /// <param name="src">Source <see cref="ReadOnlySpan{Char}"/>.</param>
        /// <returns>Normalized <paramref name="src"/>.</returns>
        /// <stable>ICU 4.4</stable>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual string Normalize(scoped ReadOnlySpan<char> src)
        {
            ValueStringBuilder sb = src.Length <= CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                : new ValueStringBuilder(src.Length);
            try
            {
                Normalize(src, ref sb);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Normalizes the form of the source <see cref="ReadOnlySpan{Char}"/>
        /// and places the result in <paramref name="destination"/>.
        /// </summary>
        /// <param name="source">Source <see cref="ReadOnlySpan{Char}"/>.</param>
        /// <param name="destination">The span in which to write the normalized value formatted as a span of characters.</param>
        /// <param name="charsLength">When this method returns <c>true</c>, contains the number of characters that are usable in destination;
        /// otherwise, this is the length of buffer that will need to be allocated to succeed in another attempt.</param>
        /// <returns>Normalized <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        /// <draft>ICU 60.1</draft>
        public virtual bool TryNormalize(string source, Span<char> destination, out int charsLength)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return TryNormalize(source.AsSpan(), destination, out charsLength);
        }

        /// <summary>
        /// Normalizes the form of the source <see cref="ReadOnlySpan{Char}"/>
        /// and places the result in <paramref name="destination"/>.
        /// </summary>
        /// <param name="source">Source <see cref="ReadOnlySpan{Char}"/>.</param>
        /// <param name="destination">The span in which to write the normalized value formatted as a span of characters.</param>
        /// <param name="charsLength">When this method returns <c>true</c>, contains the number of characters that are usable in destination;
        /// otherwise, this is the length of buffer that will need to be allocated to succeed in another attempt.</param>
        /// <returns>Normalized <paramref name="source"/>.</returns>
        /// <draft>ICU 60.1</draft>
        public abstract bool TryNormalize(scoped ReadOnlySpan<char> source, Span<char> destination, out int charsLength);

        /// <summary>
        /// Appends the normalized form of the <paramref name="second"/> string to the <paramref name="first"/> string
        /// (merging them at the boundary) and returns the <paramref name="first"/> string.
        /// The result is normalized if the <paramref name="first"/> string was normalized.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, will be normalized.</param>
        /// <param name="destination"></param>
        /// <param name="charsLength"></param>
        /// <returns><c>false</c> if <paramref name="destination"/> was not long enough to perform the
        /// concatenation; otherwise, <c>true</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="first"/> or <paramref name="second"/> is <c>null</c>.
        /// </exception>
        /// <draft>ICU 60.1</draft>
        public virtual bool TryNormalizeSecondAndConcat(string first, string second, Span<char> destination, out int charsLength)
        {
            if (first is null)
                throw new ArgumentNullException(nameof(first));
            if (second is null)
                throw new ArgumentNullException(nameof(second));

            return TryNormalizeSecondAndConcat(first.AsSpan(), second.AsSpan(), destination, out charsLength);
        }

        /// <summary>
        /// Appends the normalized form of the <paramref name="second"/> string to the <paramref name="first"/> string
        /// (merging them at the boundary) and returns the <paramref name="first"/> string.
        /// The result is normalized if the <paramref name="first"/> string was normalized.
        /// The <paramref name="first"/> and <paramref name="second"/> strings must be different references.
        /// The <paramref name="first"/> and <paramref name="destination"/> strings may be the same reference.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, will be normalized.</param>
        /// <param name="destination"></param>
        /// <param name="charsLength"></param>
        /// <returns><c>false</c> if <paramref name="destination"/> was not long enough to perform the
        /// concatenation; otherwise, <c>true</c>.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="second"/> and <paramref name="destination"/> refer to the same memory location.
        /// </exception>
        /// <draft>ICU 60.1</draft>
        public abstract bool TryNormalizeSecondAndConcat(ReadOnlySpan<char> first, ReadOnlySpan<char> second, Span<char> destination, out int charsLength);

        /// <summary>
        /// Appends the <paramref name="second"/> string to the <paramref name="first"/> string
        /// (merging them at the boundary) and puts the result into <paramref name="destination"/>.
        /// The result is normalized if both the strings were normalized.
        /// </summary>
        /// <param name="first">First string, should be normalized. This string may be a slice of
        /// <paramref name="destination"/>, which allows this method to be called recursively.</param>
        /// <param name="second">Second string, should be normalized.</param>
        /// <param name="destination">The span in which to write the normalized value formatted as a span of characters.</param>
        /// <param name="charsLength">Upon return, will contain the length of <paramref name="destination"/> after
        /// the operation. If the return value is <c>false</c>,
        /// this will contain the length of <paramref name="destination"/> that would need to be provided to make the
        /// operation succeed.</param>
        /// <returns><c>false</c> if <paramref name="destination"/> was not long enough to perform the
        /// concatenation; otherwise, <c>true</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="first"/> or <paramref name="second"/> is <c>null</c>.
        /// </exception>
        /// <draft>ICU 60.1</draft>
        public virtual bool TryConcat(string first, string second, Span<char> destination, out int charsLength)
        {
            if (first is null)
                throw new ArgumentNullException(nameof(first));
            if (second is null)
                throw new ArgumentNullException(nameof(second));

            return TryConcat(first.AsSpan(), second.AsSpan(), destination, out charsLength);
        }

        /// <summary>
        /// Appends the <paramref name="second"/> string to the <paramref name="first"/> string
        /// (merging them at the boundary) and puts the result into <paramref name="destination"/>.
        /// The result is normalized if both the strings were normalized.
        /// The <paramref name="second"/> and <paramref name="destination"/> strings must be different references.
        /// The <paramref name="first"/> and <paramref name="destination"/> strings may be the same reference.
        /// </summary>
        /// <param name="first">First string, should be normalized. This string may be a slice of
        /// <paramref name="destination"/>, which allows this method to be called recursively.</param>
        /// <param name="second">Second string, should be normalized.</param>
        /// <param name="destination">The span in which to write the normalized value formatted as a span of characters.</param>
        /// <param name="charsLength">Upon return, will contain the length of <paramref name="destination"/> after
        /// the operation. If the return value is <c>false</c>,
        /// this will contain the length of <paramref name="destination"/> that would need to be provided to make the
        /// operation succeed.</param>
        /// <returns><c>false</c> if <paramref name="destination"/> was not long enough to perform the
        /// concatenation; otherwise, <c>true</c>.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="second"/> and <paramref name="destination"/> refer to the same memory location.
        /// </exception>
        /// <draft>ICU 60.1</draft>
        public abstract bool TryConcat(ReadOnlySpan<char> first, ReadOnlySpan<char> second, Span<char> destination, out int charsLength);

        #region Normalize(ICharSequence, StringBuilder)

        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="src"/> or <paramref name="dest"/> is <c>null</c>.</exception>
        /// <stable>ICU 4.4</stable>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual StringBuilder Normalize(string src, StringBuilder dest)
        {
            if (src is null)
                throw new ArgumentNullException(nameof(src));
            if (dest is null)
                throw new ArgumentNullException(nameof(dest));

            return Normalize(src.AsSpan(), dest);
        }

        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="dest"/> is <c>null</c>.</exception>
        /// <stable>ICU 4.4</stable>
        public abstract StringBuilder Normalize(ReadOnlySpan<char> src, StringBuilder dest);


        /// <summary>
        /// Writes the normalized form of the source string to the destination string
        /// (replacing its contents) and returns the destination string.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="src"/> and <paramref name="dest"/> refer to the same memory location.
        /// </exception>
        /// <draft>ICU 60.1</draft>
        internal abstract void Normalize(scoped ReadOnlySpan<char> src, scoped ref ValueStringBuilder dest);

        #endregion Normalize(ICharSequence, StringBuilder)

        #region Normalize(ICharSequence, IAppendable)

        /// <summary>
        /// Writes the normalized form of the source string to the destination <see cref="IAppendable"/>
        /// and returns the destination <see cref="IAppendable"/>.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        /// <typeparam name="TAppendable">The implementation of <see cref="IAppendable"/> to use to write the output.</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="src"/> is <c>null</c>.</exception>
        /// <stable>ICU 4.6</stable>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual TAppendable Normalize<TAppendable>(string src, TAppendable dest) where TAppendable : IAppendable
        {
            if (src is null)
                throw new ArgumentNullException(nameof(src));
            if (dest is null)
                throw new ArgumentNullException(nameof(dest));

            return Normalize(src.AsSpan(), dest);
        }

        /// <summary>
        /// Writes the normalized form of the source string to the destination <see cref="IAppendable"/>
        /// and returns the destination <see cref="IAppendable"/>.
        /// </summary>
        /// <param name="src">Source string.</param>
        /// <param name="dest">Destination string; its contents is replaced with normalized <paramref name="src"/>.</param>
        /// <returns><paramref name="dest"/></returns>
        /// <typeparam name="TAppendable">The implementation of <see cref="IAppendable"/> to use to write the output.</typeparam>
        /// <stable>ICU 4.6</stable>
        public abstract TAppendable Normalize<TAppendable>(ReadOnlySpan<char> src, TAppendable dest) where TAppendable : IAppendable;

        #endregion Normalize(ICharSequence, IAppendable)

        #region NormalizeSecondAndAppend(StringBuilder, ICharSequence)

        /// <summary>
        /// Appends the normalized form of the <paramref name="second"/> string to the <paramref name="first"/> string
        /// (merging them at the boundary) and returns the <paramref name="first"/> string.
        /// The result is normalized if the <paramref name="first"/> string was normalized.
        /// The <paramref name="first"/> and <paramref name="second"/> strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, will be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="first"/> or <paramref name="second"/> is <c>null</c>.</exception>
        /// <stable>ICU 4.4</stable>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual StringBuilder NormalizeSecondAndAppend(
            StringBuilder first, string second)
        {
            if (first is null)
                throw new ArgumentNullException(nameof(first));
            if (second is null)
                throw new ArgumentNullException(nameof(second));

            return NormalizeSecondAndAppend(first, second.AsSpan());
        }

        /// <summary>
        /// Appends the normalized form of the <paramref name="second"/> string to the <paramref name="first"/> string
        /// (merging them at the boundary) and returns the <paramref name="first"/> string.
        /// The result is normalized if the <paramref name="first"/> string was normalized.
        /// The <paramref name="first"/> and <paramref name="second"/> strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, will be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="first"/> is <c>null</c>.</exception>
        /// <stable>ICU 4.4</stable>
        public abstract StringBuilder NormalizeSecondAndAppend(
            StringBuilder first, ReadOnlySpan<char> second);

        /// <summary>
        /// Appends the normalized form of the <paramref name="second"/> string to the <paramref name="first"/> string
        /// (merging them at the boundary) and returns the <paramref name="first"/> string.
        /// The result is normalized if the <paramref name="first"/> string was normalized.
        /// The <paramref name="first"/> and <paramref name="second"/> strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, will be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="first"/> and <paramref name="second"/> refer to the same memory location.
        /// </exception>
        /// <draft>ICU 60.1</draft>
        internal abstract void NormalizeSecondAndAppend(
            scoped ref ValueStringBuilder first, scoped ReadOnlySpan<char> second);

        #endregion

        #region Append(StringBuilder, ICharSequence)
        /// <summary>
        /// Appends the <paramref name="second"/> string to the <paramref name="first"/> string
        /// (merging them at the boundary) and returns the <paramref name="first"/> string.
        /// The result is normalized if both the strings were normalized.
        /// The <paramref name="first"/> and <paramref name="second"/> strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, should be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="first"/> or <paramref name="second"/> is <c>null</c>.</exception>
        /// <stable>ICU 4.4</stable>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual StringBuilder Append(StringBuilder first, string second)
        {
            if (first is null)
                throw new ArgumentNullException(nameof(first));
            if (second is null)
                throw new ArgumentNullException(nameof(second));

            return Append(first, second.AsSpan());
        }

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
        public abstract StringBuilder Append(StringBuilder first, ReadOnlySpan<char> second);

        /// <summary>
        /// Appends the <paramref name="second"/> string to the <paramref name="first"/> string
        /// (merging them at the boundary) and returns the <paramref name="first"/> string.
        /// The result is normalized if both the strings were normalized.
        /// The <paramref name="first"/> and <paramref name="second"/> strings must be different objects.
        /// </summary>
        /// <param name="first">First string, should be normalized.</param>
        /// <param name="second">Second string, should be normalized.</param>
        /// <returns><paramref name="first"/></returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="first"/> and <paramref name="second"/> refer to the same memory location.
        /// </exception>
        /// <stable>ICU 4.4</stable>
        internal abstract void Append(scoped ref ValueStringBuilder first, scoped ReadOnlySpan<char> second);

        #endregion Append(StringBuilder, ICharSequence)

        /// <summary>
        /// Gets the decomposition mapping of <paramref name="codePoint"/>.
        /// Roughly equivalent to normalizing the <see cref="string"/> form of <paramref name="codePoint"/>
        /// on a DECOMPOSE <see cref="Normalizer2"/> instance, but much faster, and except that this function
        /// returns <c>null</c> if <paramref name="codePoint"/> does not have a decomposition mapping in this instance's data.
        /// This function is independent of the mode of the <see cref="Normalizer2"/>.
        /// </summary>
        /// <param name="codePoint">Code point.</param>
        /// <returns><paramref name="codePoint"/>'s decomposition mapping, if any; otherwise null.</returns>
        /// <stable>ICU 4.6</stable>
        public abstract string GetDecomposition(int codePoint);

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
        public abstract bool TryGetDecomposition(int codePoint, Span<char> destination, out int charsLength);

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
        /// The default implementation returns <c>null</c>.
        /// </summary>
        /// <param name="codePoint">Code point.</param>
        /// <returns><paramref name="codePoint"/>'s raw decomposition mapping, if any; otherwise <c>null</c>.</returns>
        /// <stable>ICU 49</stable>
        public virtual string GetRawDecomposition(int codePoint) => null;

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
        public virtual bool TryGetRawDecomposition(int codePoint, Span<char> destination, out int charsLength)
        {
            charsLength = 0;
            return false;
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
        public virtual int ComposePair(int a, int b) => -1;

        /// <summary>
        /// Gets the combining class of <paramref name="codePoint"/>.
        /// The default implementation returns 0
        /// but all standard implementations return the Unicode Canonical_Combining_Class value.
        /// </summary>
        /// <param name="codePoint">Code point.</param>
        /// <returns><paramref name="codePoint"/>'s combining class.</returns>
        /// <stable>ICU 49</stable>
        public virtual int GetCombiningClass(int codePoint) => 0;

        #region IsNormailzed(ICharSequence)
        /// <summary>
        /// Tests if the string is normalized.
        /// Internally, in cases where the <see cref="QuickCheck(string)"/> method would return "maybe"
        /// (which is only possible for the two COMPOSE modes) this method
        /// resolves to "yes" or "no" to provide a definitive result,
        /// at the cost of doing more work in those cases.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>true if <paramref name="s"/> is normalized.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="s"/> is <c>null</c>.</exception>
        /// <stable>ICU 4.4</stable>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool IsNormalized(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            return IsNormalized(s.AsSpan());
        }

        /// <summary>
        /// Tests if the string is normalized.
        /// Internally, in cases where the <see cref="QuickCheck(ReadOnlySpan{char})"/> method would return "maybe"
        /// (which is only possible for the two COMPOSE modes) this method
        /// resolves to "yes" or "no" to provide a definitive result,
        /// at the cost of doing more work in those cases.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>true if <paramref name="s"/> is normalized.</returns>
        /// <stable>ICU 4.4</stable>
        public abstract bool IsNormalized(scoped ReadOnlySpan<char> s);

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
        /// <exception cref="ArgumentNullException"><paramref name="s"/> is <c>null</c>.</exception>
        /// <stable>ICU 4.4</stable>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual QuickCheckResult QuickCheck(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            return QuickCheck(s.AsSpan());
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
        public abstract QuickCheckResult QuickCheck(scoped ReadOnlySpan<char> s);

        #endregion QuickCheck(ICharSequence)

        #region SpanQuickCheckYes(ICharSequence)

        /// <summary>
        /// Returns the end of the normalized substring of the input string.
        /// In other words, with <c>end=SpanQuickCheckYes(s);</c>
        /// the substring <c>s.Substring(0, end)</c>
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual int SpanQuickCheckYes(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            return SpanQuickCheckYes(s.AsSpan());
        }

        /// <summary>
        /// Returns the end of the normalized substring of the input string.
        /// In other words, with <c>end=SpanQuickCheckYes(s);</c>
        /// the substring <c>s.Substring(0, end)</c>
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
        public abstract int SpanQuickCheckYes(ReadOnlySpan<char> s); // ICU4N TODO: Update docs because in .NET Substring uses length instead of end

        #endregion SpanQuickCheckYes(ICharSequence)

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
        /// <stable>ICU 4.4</stable>
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
        /// <stable>ICU 4.4</stable>
        public abstract bool IsInert(int character);
    }
}
