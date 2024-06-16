using ICU4N.Impl;
using System;
using System.Runtime.CompilerServices;
#nullable enable

namespace ICU4N.Text
{
    /// <summary>
    /// IDNA error bit set values.
    /// When a domain name or label fails a processing step or does not meet the
    /// validity criteria, then one or more of these error bits are set.
    /// </summary>
    /// <stable>ICU 4.6</stable>
    // ICU4N: Converted this into a flags enum so we don't have to deal with heap objects,
    // such as ISet<T>.
    [Flags]
    public enum IDNAErrors
    {
        /// <summary>
        /// No errors. This is the default value.
        /// </summary>
        /// <draft>ICU 60.1</draft>
        None = 0,
        /// <summary>
        /// A non-final domain name label (or the whole domain name) is empty.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        EmptyLabel = 1,
        /// <summary>
        /// A domain name label is longer than 63 bytes.
        /// (See STD13/RFC1034 3.1. Name space specifications and terminology.)
        /// This is only checked in ToASCII operations, and only if the output label is all-ASCII.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        LabelTooLong = 2,
        /// <summary>
        /// A domain name is longer than 255 bytes in its storage form.
        /// (See STD13/RFC1034 3.1. Name space specifications and terminology.)
        /// This is only checked in ToASCII operations, and only if the output domain name is all-ASCII.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        DomainNameTooLong = 4,
        /// <summary>
        /// A label starts with a hyphen-minus ('-').
        /// </summary>
        /// <stable>ICU 4.6</stable>
        LeadingHyphen = 8,
        /// <summary>
        /// A label ends with a hyphen-minus ('-').
        /// </summary>
        /// <stable>ICU 4.6</stable>
        TrailingHyphen = 0x10, // 16
        /// <summary>
        /// A label contains hyphen-minus ('-') in the third and fourth positions.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        Hyphen_3_4 = 0x20, // 32
        /// <summary>
        /// A label starts with a combining mark.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        LeadingCombiningMark = 0x40, // 64
        /// <summary>
        /// A label or domain name contains disallowed characters.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        Disallowed = 0x80, // 128
        /// <summary>
        /// A label starts with "xn--" but does not contain valid Punycode.
        /// That is, an xn-- label failed Punycode decoding.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        Punycode = 0x100, // 256
        /// <summary>
        /// A label contains a dot=full stop.
        /// This can occur in an input string for a single-label function.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        LabelHasDot = 0x200, // 512
        /// <summary>
        /// An ACE label does not contain a valid label string.
        /// The label was successfully ACE (Punycode) decoded but the resulting
        /// string had severe validation errors. For example,
        /// it might contain characters that are not allowed in ACE labels,
        /// or it might not be normalized.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        InvalidAceLabel = 0x400, // 1024
        /// <summary>
        /// A label does not meet the IDNA BiDi requirements (for right-to-left characters).
        /// </summary>
        /// <stable>ICU 4.6</stable>
        BiDi = 0x800, // 2048,
        /// <summary>
        /// A label does not meet the IDNA CONTEXTJ requirements.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        ContextJ = 0x1000, // 4096
        /// <summary>
        /// A label does not meet the IDNA CONTEXTO requirements for punctuation characters.
        /// Some punctuation characters "Would otherwise have been DISALLOWED"
        /// but are allowed in certain contexts. (RFC 5892)
        /// </summary>
        /// <stable>ICU 49</stable>
        ContextOPunctuation = 0x2000, // 8192
        /// <summary>
        /// A label does not meet the IDNA CONTEXTO requirements for digits.
        /// Arabic-Indic Digits (U+066x) must not be mixed with Extended Arabic-Indic Digits (U+06Fx).
        /// </summary>
        /// <stable>ICU 49</stable>
        ContextODigits = 0x4000, // 16384
        /// <summary>
        /// The result of a conversion does not fit the supplied buffer. The <c>charsLength</c> or 
        /// <c>bytesLength</c> out parameter typically indicates the size of the buffer that is
        /// required for the operation to succeed.
        /// </summary>
        // ICU4N: Using the highest available int to avoid collisions with IDNA error flags that may be added
        // in the future, since U_BUFFER_OVERFLOW_ERROR (15) would collide with a combination of these values.
        BufferOverflow = 0x40000000,
    }


    /// <summary>
    /// Output container for IDNA processing errors.
    /// The <see cref="IDNAInfo"/> class is not suitable for subclassing.
    /// </summary>
    /// <stable>ICU 4.6</stable>
    public ref struct IDNAInfo
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public IDNAInfo()
        {
            errors = 0;
            labelErrors = 0;
            isTransDiff = false;
            isBiDi = false;
            isOkBiDi = true;
        }

        /// <summary>
        /// Were there IDNA processing errors?
        /// Returns true if there were processing errors.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public bool HasErrors => errors > 0;

        /// <summary>
        /// Gets or sets a flags enum indicating IDNA processing errors.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public IDNAErrors Errors
        {
            get => errors;
            set => errors = value;
        }

        /// <summary>
        /// Returns true if transitional and nontransitional processing produce different results.
        /// This is the case when the input label or domain name contains
        /// one or more deviation characters outside a Punycode label (see UTS #46).
        /// <list type="bullet">
        ///     <item><description>
        ///         With nontransitional processing, such characters are
        ///         copied to the destination string.
        ///     </description></item>
        ///     <item><description>
        ///         With transitional processing, such characters are
        ///         mapped (sharp s/sigma) or removed (joiner/nonjoiner).
        ///     </description></item>
        /// </list>
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public bool IsTransitionalDifferent => isTransDiff;

        // ICU4N: Since we are stack allocated, there is no reason for Reset()

        internal IDNAErrors errors, labelErrors;
        internal bool isTransDiff;
        internal bool isBiDi;
        internal bool isOkBiDi;
    }

    /// <summary>
    /// Abstract base class for IDNA processing.
    /// See <a href="http://www.unicode.org/reports/tr46/">http://www.unicode.org/reports/tr46/</a>
    /// and <a href="http://www.ietf.org/rfc/rfc3490.txt">http://www.ietf.org/rfc/rfc3490.txt</a>
    /// </summary>
    /// <remarks>
    /// The IDNA class is not intended for public subclassing.
    /// The non-static methods implement UTS #46 and IDNA2008.
    /// IDNA2008 is implemented according to UTS #46, see <see cref="GetUTS46Instance(UTS46Options)"/>.
    /// <para/>
    /// IDNA2003 is obsolete. The static methods implement IDNA2003. They are all deprecated.
    /// <para/>
    /// IDNA2003 API Overview:
    /// <para/>
    /// The static IDNA API methods implement the IDNA protocol as defined in the
    /// <a href="http://www.ietf.org/rfc/rfc3490.txt">IDNA RFC</a>.
    /// The draft defines 2 operations: ToASCII and ToUnicode. Domain labels 
    /// containing non-ASCII code points are required to be processed by
    /// ToASCII operation before passing it to resolver libraries. Domain names
    /// that are obtained from resolver libraries are required to be processed by
    /// ToUnicode operation before displaying the domain name to the user
    /// IDNA requires that implementations process input strings with
    /// <a href="http://www.ietf.org/rfc/rfc3491.txt">Nameprep</a>,
    /// which is a profile of <a href="http://www.ietf.org/rfc/rfc3454.txt">Stringprep</a>,
    /// and then with <a href="http://www.ietf.org/rfc/rfc3492.txt">Punycode</a>.
    /// Implementations of IDNA MUST fully implement Nameprep and Punycode;
    /// neither Nameprep nor Punycode are optional.
    /// The input and output of ToASCII and ToUnicode operations are Unicode 
    /// and are designed to be chainable, i.e., applying ToASCII or ToUnicode operations
    /// multiple times to an input string will yield the same result as applying the operation
    /// once.
    /// <code>
    /// ToUnicode(ToUnicode(ToUnicode...(ToUnicode(string)))) == ToUnicode(string) 
    /// ToASCII(ToASCII(ToASCII...(ToASCII(string))) == ToASCII(string)
    /// </code>
    /// </remarks>
    /// <author>Ram Viswanadha, Markus Scherer</author>
    /// <stable>ICU 2.8</stable>
    public abstract partial class IDNA
    {
        internal const int CharStackBufferSize = 64;

        // ICU4N specific - options moved to UTS46Options and IDNA2003Options
        // [Flags] enums

        /// <summary>
        /// Returns an IDNA instance which implements UTS #46.
        /// Returns an unmodifiable instance, owned by the caller.
        /// Cache it for multiple operations, and delete it when done.
        /// The instance is thread-safe, that is, it can be used concurrently.
        /// </summary>
        /// <remarks>
        /// UTS #46 defines Unicode IDNA Compatibility Processing,
        /// updated to the latest version of Unicode and compatible with both
        /// IDNA2003 and IDNA2008.
        /// <para/>
        /// The worker functions use transitional processing, including deviation mappings,
        /// unless <see cref="UTS46Options.NontransitionalToASCII"/> or <see cref="UTS46Options.NontransitionalToUnicode"/>
        /// is used in which case the deviation characters are passed through without change.
        /// <para/>
        /// Disallowed characters are mapped to U+FFFD.
        /// <para/>
        /// Operations with the UTS #46 instance do not support the
        /// <see cref="IDNA2003Options.AllowUnassigned"/> option.
        /// <para/>
        /// By default, the UTS #46 implementation allows all ASCII characters (as valid or mapped).
        /// When the <see cref="UTS46Options.UseSTD3Rules"/> option is used, ASCII characters other than
        /// letters, digits, hyphen (LDH) and dot/full stop are disallowed and mapped to U+FFFD.
        /// </remarks>
        /// <param name="options">Bit set to modify the processing and error checking.</param>
        /// <returns>The UTS #46 IDNA instance, if successful.</returns>
        /// <stable>ICU 4.6</stable>
        public static IDNA GetUTS46Instance(UTS46Options options)
        {
            return new UTS46(options);
        }

        /// <summary>
        /// Converts a single domain name label into its ASCII form for DNS lookup.
        /// If any processing step fails, then the result will be <c>false</c> and
        /// <paramref name="destination"/> might not be an ASCII string.
        /// The label might be modified according to the types of errors.
        /// Labels with severe errors will be left in (or turned into) their Unicode form.
        /// </summary>
        /// <param name="label">Input domain name label.</param>
        /// <param name="destination">Destination span.</param>
        /// <param name="charsLength">Upon return, will contain the length of <paramref name="destination"/> after
        /// the operation. If the return value is <c>false</c> and <see cref="IDNAInfo.Errors"/> includes
        /// <see cref="IDNAErrors.BufferOverflow"/>, this will contain the length of <paramref name="destination"/>
        /// that would need to be provided to make the operation succeed.</param>
        /// <param name="info">Output container of IDNA processing details.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        /// <stable>ICU 4.6</stable>
        public virtual bool TryLabelToASCII(string label, Span<char> destination, out int charsLength, out IDNAInfo info)
        {
            if (label is null)
                throw new ArgumentNullException(nameof(label));

            return TryLabelToASCII(label.AsSpan(), destination, out charsLength, out info);
        }

        /// <summary>
        /// Converts a single domain name label into its ASCII form for DNS lookup.
        /// If any processing step fails, then the result will be <c>false</c> and
        /// <paramref name="destination"/> might not be an ASCII string.
        /// The label might be modified according to the types of errors.
        /// Labels with severe errors will be left in (or turned into) their Unicode form.
        /// </summary>
        /// <param name="label">Input domain name label.</param>
        /// <param name="destination">Destination span.</param>
        /// <param name="charsLength">Upon return, will contain the length of <paramref name="destination"/> after
        /// the operation. If the return value is <c>false</c> and <see cref="IDNAInfo.Errors"/> includes
        /// <see cref="IDNAErrors.BufferOverflow"/>, this will contain the length of <paramref name="destination"/>
        /// that would need to be provided to make the operation succeed.</param>
        /// <param name="info">Output container of IDNA processing details.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        /// <stable>ICU 4.6</stable>
        public abstract bool TryLabelToASCII(ReadOnlySpan<char> label, Span<char> destination, out int charsLength, out IDNAInfo info);

        /// <summary>
        /// Converts a single domain name label into its ASCII form for DNS lookup.
        /// If any processing step fails, then the result will be <c>false</c> and
        /// <paramref name="destination"/> might not be an ASCII string.
        /// The label might be modified according to the types of errors.
        /// Labels with severe errors will be left in (or turned into) their Unicode form.
        /// </summary>
        /// <param name="label">Input domain name label.</param>
        /// <param name="destination">Destination string object.</param>
        /// <param name="info">Output container of IDNA processing details.</param>
        /// <draft>ICU 60.1</draft>
        internal virtual bool TryLabelToASCII(string label, ref ValueStringBuilder destination, out IDNAInfo info)
        {
            if (label is null)
                throw new ArgumentNullException(nameof(label));

            return TryLabelToASCII(label.AsSpan(), ref destination, out info);
        }

        /// <summary>
        /// Converts a single domain name label into its ASCII form for DNS lookup.
        /// If any processing step fails, then the result will be <c>false</c> and
        /// <paramref name="destination"/> might not be an ASCII string.
        /// The label might be modified according to the types of errors.
        /// Labels with severe errors will be left in (or turned into) their Unicode form.
        /// </summary>
        /// <param name="label">Input domain name label.</param>
        /// <param name="destination">Destination string object.</param>
        /// <param name="info">Output container of IDNA processing details.</param>
        /// <draft>ICU 60.1</draft>
        internal abstract bool TryLabelToASCII(ReadOnlySpan<char> label, ref ValueStringBuilder destination, out IDNAInfo info);

        /// <summary>
        /// Converts a single domain name label into its Unicode form for human-readable display.
        /// If any processing step fails, then the result will be <c>false</c>.
        /// The label might be modified according to the types of errors.
        /// </summary>
        /// <param name="label">Input domain name label.</param>
        /// <param name="destination">Destination span.</param>
        /// <param name="charsLength">Upon return, will contain the length of <paramref name="destination"/> after
        /// the operation. If the return value is <c>false</c> and <see cref="IDNAInfo.Errors"/> includes
        /// <see cref="IDNAErrors.BufferOverflow"/>, this will contain the length of <paramref name="destination"/>
        /// that would need to be provided to make the operation succeed.</param>
        /// <param name="info">Output container of IDNA processing details.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        /// <stable>ICU 4.6</stable>
        public virtual bool TryLabelToUnicode(string label, Span<char> destination, out int charsLength, out IDNAInfo info)
        {
            if (label is null)
                throw new ArgumentNullException(nameof(label));

            return TryLabelToUnicode(label.AsSpan(), destination, out charsLength, out info);
        }

        /// <summary>
        /// Converts a single domain name label into its Unicode form for human-readable display.
        /// If any processing step fails, then the result will be <c>false</c>.
        /// The label might be modified according to the types of errors.
        /// </summary>
        /// <param name="label">Input domain name label.</param>
        /// <param name="destination">Destination span.</param>
        /// <param name="charsLength">Upon return, will contain the length of <paramref name="destination"/> after
        /// the operation. If the return value is <c>false</c> and <see cref="IDNAInfo.Errors"/> includes
        /// <see cref="IDNAErrors.BufferOverflow"/>, this will contain the length of <paramref name="destination"/>
        /// that would need to be provided to make the operation succeed.</param>
        /// <param name="info">Output container of IDNA processing details.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        /// <stable>ICU 4.6</stable>
        public abstract bool TryLabelToUnicode(ReadOnlySpan<char> label, Span<char> destination, out int charsLength, out IDNAInfo info);


        /// <summary>
        /// Converts a single domain name label into its Unicode form for human-readable display.
        /// If any processing step fails, then the result will be <c>false</c>.
        /// The label might be modified according to the types of errors.
        /// </summary>
        /// <param name="label">Input domain name label.</param>
        /// <param name="destination">Destination string object.</param>
        /// <param name="info">Output container of IDNA processing details.</param>
        /// <draft>ICU 60.1</draft>
        internal virtual bool TryLabelToUnicode(string label, ref ValueStringBuilder destination, out IDNAInfo info)
        {
            if (label is null)
                throw new ArgumentNullException(nameof(label));

            return TryLabelToUnicode(label.AsSpan(), ref destination, out info);
        }

        /// <summary>
        /// Converts a single domain name label into its Unicode form for human-readable display.
        /// If any processing step fails, then the result will be <c>false</c>.
        /// The label might be modified according to the types of errors.
        /// </summary>
        /// <param name="label">Input domain name label.</param>
        /// <param name="destination">Destination string object.</param>
        /// <param name="info">Output container of IDNA processing details.</param>
        /// <draft>ICU 60.1</draft>
        internal abstract bool TryLabelToUnicode(ReadOnlySpan<char> label, ref ValueStringBuilder destination, out IDNAInfo info);

        /// <summary>
        /// Converts a whole domain name into its ASCII form for DNS lookup.
        /// If any processing step fails, then the result will be <c>false</c> and
        /// <paramref name="destination"/> might not be an ASCII string.
        /// The domain name might be modified according to the types of errors.
        /// Labels with severe errors will be left in (or turned into) their Unicode form.
        /// </summary>
        /// <param name="name">Input domain name.</param>
        /// <param name="destination">Destination span.</param>
        /// <param name="charsLength">Upon return, will contain the length of <paramref name="destination"/> after
        /// the operation. If the return value is <c>false</c> and <see cref="IDNAInfo.Errors"/> includes
        /// <see cref="IDNAErrors.BufferOverflow"/>, this will contain the length of <paramref name="destination"/>
        /// that would need to be provided to make the operation succeed.</param>
        /// <param name="info">Output container of IDNA processing details.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        /// <stable>ICU 4.6</stable>
        public virtual bool TryNameToASCII(string name, Span<char> destination, out int charsLength, out IDNAInfo info)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            return TryNameToASCII(name.AsSpan(), destination, out charsLength, out info);
        }

        /// <summary>
        /// Converts a whole domain name into its ASCII form for DNS lookup.
        /// If any processing step fails, then the result will be <c>false</c> and
        /// <paramref name="destination"/> might not be an ASCII string.
        /// The domain name might be modified according to the types of errors.
        /// Labels with severe errors will be left in (or turned into) their Unicode form.
        /// </summary>
        /// <param name="name">Input domain name.</param>
        /// <param name="destination">Destination span.</param>
        /// <param name="charsLength">Upon return, will contain the length of <paramref name="destination"/> after
        /// the operation. If the return value is <c>false</c> and <see cref="IDNAInfo.Errors"/> includes
        /// <see cref="IDNAErrors.BufferOverflow"/>, this will contain the length of <paramref name="destination"/>
        /// that would need to be provided to make the operation succeed.</param>
        /// <param name="info">Output container of IDNA processing details.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        /// <stable>ICU 4.6</stable>
        public abstract bool TryNameToASCII(ReadOnlySpan<char> name, Span<char> destination, out int charsLength, out IDNAInfo info);

        /// <summary>
        /// Converts a whole domain name into its ASCII form for DNS lookup.
        /// If any processing step fails, then the result will be <c>false</c> and
        /// <paramref name="destination"/> might not be an ASCII string.
        /// The domain name might be modified according to the types of errors.
        /// Labels with severe errors will be left in (or turned into) their Unicode form.
        /// </summary>
        /// <param name="name">Input domain name.</param>
        /// <param name="destination">Destination string object.</param>
        /// <param name="info">Output container of IDNA processing details.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        /// <draft>ICU 60.1</draft>
        internal virtual bool TryNameToASCII(string name, ref ValueStringBuilder destination, out IDNAInfo info)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            return TryNameToASCII(name.AsSpan(), ref destination, out info);
        }

        /// <summary>
        /// Converts a whole domain name into its ASCII form for DNS lookup.
        /// If any processing step fails, then the result will be <c>false</c> and
        /// <paramref name="destination"/> might not be an ASCII string.
        /// The domain name might be modified according to the types of errors.
        /// Labels with severe errors will be left in (or turned into) their Unicode form.
        /// </summary>
        /// <param name="name">Input domain name.</param>
        /// <param name="destination">Destination string object.</param>
        /// <param name="info">Output container of IDNA processing details.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        /// <draft>ICU 60.1</draft>
        internal abstract bool TryNameToASCII(ReadOnlySpan<char> name, ref ValueStringBuilder destination, out IDNAInfo info);

        /// <summary>
        /// Converts a whole domain name into its Unicode form for human-readable display.
        /// If any processing step fails, then the result will be <c>false</c>.
        /// The domain name might be modified according to the types of errors.
        /// </summary>
        /// <param name="name">Input domain name.</param>
        /// <param name="destination">Destination span.</param>
        /// <param name="charsLength">Upon return, will contain the length of <paramref name="destination"/> after
        /// the operation. If the return value is <c>false</c> and <see cref="IDNAInfo.Errors"/> includes
        /// <see cref="IDNAErrors.BufferOverflow"/>, this will contain the length of <paramref name="destination"/>
        /// that would need to be provided to make the operation succeed.</param>
        /// <param name="info">Output container of IDNA processing details.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        /// <stable>ICU 4.6</stable>
        public virtual bool TryNameToUnicode(string name, Span<char> destination, out int charsLength, out IDNAInfo info)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            return TryNameToUnicode(name.AsSpan(), destination, out charsLength, out info);
        }

        /// <summary>
        /// Converts a whole domain name into its Unicode form for human-readable display.
        /// If any processing step fails, then the result will be <c>false</c>.
        /// The domain name might be modified according to the types of errors.
        /// </summary>
        /// <param name="name">Input domain name.</param>
        /// <param name="destination">Destination span.</param>
        /// <param name="charsLength">Upon return, will contain the length of <paramref name="destination"/> after
        /// the operation. If the return value is <c>false</c> and <see cref="IDNAInfo.Errors"/> includes
        /// <see cref="IDNAErrors.BufferOverflow"/>, this will contain the length of <paramref name="destination"/>
        /// that would need to be provided to make the operation succeed.</param>
        /// <param name="info">Output container of IDNA processing details.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        /// <stable>ICU 4.6</stable>
        public abstract bool TryNameToUnicode(ReadOnlySpan<char> name, Span<char> destination, out int charsLength, out IDNAInfo info);

        /// <summary>
        /// Converts a whole domain name into its Unicode form for human-readable display.
        /// If any processing step fails, then the result will be <c>false</c>.
        /// The domain name might be modified according to the types of errors.
        /// </summary>
        /// <param name="name">Input domain name.</param>
        /// <param name="destination">Destination string object.</param>
        /// <param name="info">Output container of IDNA processing details.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        /// <draft>ICU 60.1</draft>
        internal virtual bool TryNameToUnicode(string name, ref ValueStringBuilder destination, out IDNAInfo info)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            return TryNameToUnicode(name.AsSpan(), ref destination, out info);
        }

        /// <summary>
        /// Converts a whole domain name into its Unicode form for human-readable display.
        /// If any processing step fails, then the result will be <c>false</c>.
        /// The domain name might be modified according to the types of errors.
        /// </summary>
        /// <param name="name">Input domain name.</param>
        /// <param name="destination">Destination string object.</param>
        /// <param name="info">Output container of IDNA processing details.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        /// <draft>ICU 60.1</draft>
        internal abstract bool TryNameToUnicode(ReadOnlySpan<char> name, ref ValueStringBuilder destination, out IDNAInfo info);

        // ICU4N specific - De-nested Info class and renamed IDNAInfo

        // The following protected methods give IDNA subclasses access to the private IDNAInfo fields.
        // The IDNAInfo also provides intermediate state that is publicly invisible,
        // avoiding the allocation of another worker object.

        // ICU4N: ResetInfo() is unnecessary because we are always allocating on the stack

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static bool HasCertainErrors(ref IDNAInfo info, IDNAErrors errors) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            return info.errors != IDNAErrors.None && (info.Errors & errors) != 0;
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static bool HasCertainLabelErrors(ref IDNAInfo info, IDNAErrors errors) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            return info.labelErrors != IDNAErrors.None && (info.labelErrors & errors) != 0;
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void AddLabelError(ref IDNAInfo info, IDNAErrors error) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            info.labelErrors |= error;
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void PromoteAndResetLabelErrors(ref IDNAInfo info) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            if (info.labelErrors > 0)
            {
                info.errors |= info.labelErrors;
                info.labelErrors = IDNAErrors.None;
            }
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void AddError(ref IDNAInfo info, IDNAErrors error) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            info.errors |= error;
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void SetTransitionalDifferent(ref IDNAInfo info) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            info.isTransDiff = true;
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void SetBiDi(ref IDNAInfo info) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            info.isBiDi = true;
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static bool IsBiDi(ref IDNAInfo info) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            return info.isBiDi;
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static void SetNotOkBiDi(ref IDNAInfo info) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            info.isOkBiDi = false;
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#if FEATURE_METHODIMPLOPTIONS_AGRESSIVEINLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        internal static bool IsOkBiDi(ref IDNAInfo info) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            return info.isOkBiDi;
        }

        // ICU4N specific - de-nested Error enum and renamed IDNAErrors

        /// <summary>
        /// Sole constructor. (For invocation by subclass constructors, typically implicit.)
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal IDNA() // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
        }

        /* IDNA2003 API ------------------------------------------------------------- */

        /// <summary>
        /// IDNA2003: This function implements the ToASCII operation as defined in the IDNA RFC.
        /// This operation is done on <b>single labels</b> before sending it to something that expects
        /// ASCII names. A label is an individual part of a domain name. Labels are usually
        /// separated by dots; e.g." "www.example.com" is composed of 3 labels 
        /// "www","example", and "com".
        /// </summary>
        /// <param name="source">The input string to be processed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.AllowUnassigned"/></term>
        ///         <description>
        ///             Unassigned values can be converted to ASCII for query operations
        ///             If this option is set, the unassigned code points are in the input
        ///             are treated as normal Unicode code points.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.UseSTD3Rules"/></term>
        ///         <description>
        ///             Use STD3 ASCII rules for host name syntax restrictions
        ///             If this option is set and the input does not satisfy STD3 rules,
        ///             the operation will fail with <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns>The converted <see cref="string"/>.</returns>
        /// <exception cref="StringPrepFormatException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static string ConvertToASCII(string source, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            return ConvertToASCII(source.AsSpan(), options);
        }

        /// <summary>
        /// IDNA2003: This function implements the ToASCII operation as defined in the IDNA RFC.
        /// This operation is done on <b>single labels</b> before sending it to something that expects
        /// ASCII names. A label is an individual part of a domain name. Labels are usually
        /// separated by dots; e.g." "www.example.com" is composed of 3 labels 
        /// "www","example", and "com".
        /// </summary>
        /// <param name="source">The input string to be processed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.AllowUnassigned"/></term>
        ///         <description>
        ///             Unassigned values can be converted to ASCII for query operations
        ///             If this option is set, the unassigned code points are in the input
        ///             are treated as normal Unicode code points.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.UseSTD3Rules"/></term>
        ///         <description>
        ///             Use STD3 ASCII rules for host name syntax restrictions
        ///             If this option is set and the input does not satisfy STD3 rules,
        ///             the operation will fail with <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns>The converted <see cref="string"/>.</returns>
        /// <exception cref="StringPrepFormatException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static string ConvertToASCII(ReadOnlySpan<char> source, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            return IDNA2003.ConvertToASCII(source, options);
        }

        /// <summary>
        /// IDNA2003: This function implements the ToASCII operation as defined in the IDNA RFC.
        /// This operation is done on <b>single labels</b> before sending it to something that expects
        /// ASCII names. A label is an individual part of a domain name. Labels are usually
        /// separated by dots; e.g." "www.example.com" is composed of 3 labels 
        /// "www","example", and "com".
        /// </summary>
        /// <param name="source">The input string to be processed.</param>
        /// <param name="destination">The span in which to write the converted value.</param>
        /// <param name="charsLength">Upon return, will contain the length of <paramref name="destination"/> after
        /// the operation. If the return value is <c>false</c>,
        /// this will contain the length of <paramref name="destination"/> that would need to be provided to make the
        /// operation succeed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.AllowUnassigned"/></term>
        ///         <description>
        ///             Unassigned values can be converted to ASCII for query operations
        ///             If this option is set, the unassigned code points are in the input
        ///             are treated as normal Unicode code points.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.UseSTD3Rules"/></term>
        ///         <description>
        ///             Use STD3 ASCII rules for host name syntax restrictions
        ///             If this option is set and the input does not satisfy STD3 rules,
        ///             the operation will fail with <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <param name="errorType">Upon unsuccessful return (<c>false</c>), will contain the type of error that occurred.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static bool TryConvertToASCII(ReadOnlySpan<char> source, Span<char> destination, out int charsLength, IDNA2003Options options, out StringPrepErrorType errorType)
        {
            return IDNA2003.TryConvertToASCII(source, destination, out charsLength, options, out errorType);
        }

        /// <summary>
        /// IDNA2003: Convenience function that implements the IDNToASCII operation as defined in the IDNA RFC.
        /// This operation is done on complete domain names, e.g: "www.example.com". 
        /// It is important to note that this operation can fail. If it fails, then the input 
        /// domain name cannot be used as an Internationalized Domain Name and the application
        /// should have methods defined to deal with the failure.
        /// <para/>
        /// <b>Note:</b> IDNA RFC specifies that a conformant application should divide a domain name
        /// into separate labels, decide whether to apply allowUnassigned and useSTD3ASCIIRules on each,
        /// and then convert. This function does not offer that level of granularity. The options once  
        /// set will apply to all labels in the domain name.
        /// </summary>
        /// <param name="source">The input string to be processed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.AllowUnassigned"/></term>
        ///         <description>
        ///             Unassigned values can be converted to ASCII for query operations
        ///             If this option is set, the unassigned code points are in the input
        ///             are treated as normal Unicode code points.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.UseSTD3Rules"/></term>
        ///         <description>
        ///             Use STD3 ASCII rules for host name syntax restrictions
        ///             If this option is set and the input does not satisfy STD3 rules,
        ///             the operation will fail with <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns>The converted <see cref="string"/>.</returns>
        /// <exception cref="StringPrepFormatException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static string ConvertIDNToASCII(string source, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            return ConvertIDNToASCII(source.AsSpan(), options);
        }

        /// <summary>
        /// IDNA2003: Convenience function that implements the IDNToASCII operation as defined in the IDNA RFC.
        /// This operation is done on complete domain names, e.g: "www.example.com". 
        /// It is important to note that this operation can fail. If it fails, then the input 
        /// domain name cannot be used as an Internationalized Domain Name and the application
        /// should have methods defined to deal with the failure.
        /// <para/>
        /// <b>Note:</b> IDNA RFC specifies that a conformant application should divide a domain name
        /// into separate labels, decide whether to apply allowUnassigned and useSTD3ASCIIRules on each,
        /// and then convert. This function does not offer that level of granularity. The options once
        /// set will apply to all labels in the domain name.
        /// </summary>
        /// <param name="source">The input string to be processed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.AllowUnassigned"/></term>
        ///         <description>
        ///             Unassigned values can be converted to ASCII for query operations
        ///             If this option is set, the unassigned code points are in the input
        ///             are treated as normal Unicode code points.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.UseSTD3Rules"/></term>
        ///         <description>
        ///             Use STD3 ASCII rules for host name syntax restrictions
        ///             If this option is set and the input does not satisfy STD3 rules,
        ///             the operation will fail with <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns>The converted <see cref="string"/>.</returns>
        /// <exception cref="StringPrepFormatException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static string ConvertIDNToASCII(ReadOnlySpan<char> source, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            return IDNA2003.ConvertIDNToASCII(source, options);
        }

        /// <summary>
        /// IDNA2003: Convenience function that implements the IDNToASCII operation as defined in the IDNA RFC.
        /// This operation is done on complete domain names, e.g: "www.example.com". 
        /// It is important to note that this operation can fail. If it fails, then the input 
        /// domain name cannot be used as an Internationalized Domain Name and the application
        /// should have methods defined to deal with the failure.
        /// <para/>
        /// <b>Note:</b> IDNA RFC specifies that a conformant application should divide a domain name
        /// into separate labels, decide whether to apply allowUnassigned and useSTD3ASCIIRules on each,
        /// and then convert. This function does not offer that level of granularity. The options once
        /// set will apply to all labels in the domain name.
        /// </summary>
        /// <param name="source">The input string to be processed.</param>
        /// <param name="destination">The span in which to write the converted value.</param>
        /// <param name="charsLength">Upon return, will contain the length of <paramref name="destination"/> after
        /// the operation. If the return value is <c>false</c>,
        /// this will contain the length of <paramref name="destination"/> that would need to be provided to make the
        /// operation succeed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.AllowUnassigned"/></term>
        ///         <description>
        ///             Unassigned values can be converted to ASCII for query operations
        ///             If this option is set, the unassigned code points are in the input
        ///             are treated as normal Unicode code points.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.UseSTD3Rules"/></term>
        ///         <description>
        ///             Use STD3 ASCII rules for host name syntax restrictions
        ///             If this option is set and the input does not satisfy STD3 rules,
        ///             the operation will fail with <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <param name="errorType">Upon unsuccessful return (<c>false</c>), will contain the type of error that occurred.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static bool TryConvertIDNToASCII(ReadOnlySpan<char> source, Span<char> destination, out int charsLength, IDNA2003Options options, out StringPrepErrorType errorType)
        {
            return IDNA2003.TryConvertIDNToASCII(source, destination, out charsLength, options, out errorType);
        }

        /// <summary>
        /// IDNA2003: This function implements the ToUnicode operation as defined in the IDNA RFC.
        /// This operation is done on <b>single labels</b> before sending it to something that expects
        /// Unicode names. A label is an individual part of a domain name. Labels are usually
        /// separated by dots; for e.g." "www.example.com" is composed of 3 labels 
        /// "www","example", and "com".
        /// </summary>
        /// <param name="source">The input string to be processed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.AllowUnassigned"/></term>
        ///         <description>
        ///             Unassigned values can be converted to ASCII for query operations
        ///             If this option is set, the unassigned code points are in the input
        ///             are treated as normal Unicode code points.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.UseSTD3Rules"/></term>
        ///         <description>
        ///             Use STD3 ASCII rules for host name syntax restrictions
        ///             If this option is set and the input does not satisfy STD3 rules,
        ///             the operation will fail with <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns>The converted <see cref="string"/>.</returns>
        /// <exception cref="StringPrepFormatException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static string ConvertToUnicode(string source, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            return ConvertToUnicode(source.AsSpan(), options);
        }

        /// <summary>
        /// IDNA2003: This function implements the ToUnicode operation as defined in the IDNA RFC.
        /// This operation is done on <b>single labels</b> before sending it to something that expects
        /// Unicode names. A label is an individual part of a domain name. Labels are usually
        /// separated by dots; for e.g." "www.example.com" is composed of 3 labels 
        /// "www","example", and "com".
        /// </summary>
        /// <param name="source">The input string to be processed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.AllowUnassigned"/></term>
        ///         <description>
        ///             Unassigned values can be converted to ASCII for query operations
        ///             If this option is set, the unassigned code points are in the input
        ///             are treated as normal Unicode code points.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.UseSTD3Rules"/></term>
        ///         <description>
        ///             Use STD3 ASCII rules for host name syntax restrictions
        ///             If this option is set and the input does not satisfy STD3 rules,
        ///             the operation will fail with <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns>The converted <see cref="string"/>.</returns>
        /// <exception cref="StringPrepFormatException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static string ConvertToUnicode(ReadOnlySpan<char> source, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            return IDNA2003.ConvertToUnicode(source, options);
        }

        /// <summary>
        /// IDNA2003: This function implements the ToUnicode operation as defined in the IDNA RFC.
        /// This operation is done on <b>single labels</b> before sending it to something that expects
        /// Unicode names. A label is an individual part of a domain name. Labels are usually
        /// separated by dots; for e.g." "www.example.com" is composed of 3 labels 
        /// "www","example", and "com".
        /// </summary>
        /// <param name="source">The input string to be processed.</param>
        /// <param name="destination">The span in which to write the converted value.</param>
        /// <param name="charsLength">Upon return, will contain the length of <paramref name="destination"/> after
        /// the operation. If the return value is <c>false</c>,
        /// this will contain the length of <paramref name="destination"/> that would need to be provided to make the
        /// operation succeed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.AllowUnassigned"/></term>
        ///         <description>
        ///             Unassigned values can be converted to ASCII for query operations
        ///             If this option is set, the unassigned code points are in the input
        ///             are treated as normal Unicode code points.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.UseSTD3Rules"/></term>
        ///         <description>
        ///             Use STD3 ASCII rules for host name syntax restrictions
        ///             If this option is set and the input does not satisfy STD3 rules,
        ///             the operation will fail with <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <param name="errorType">Upon unsuccessful return (<c>false</c>), will contain the type of error that occurred.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static bool TryConvertToUnicode(ReadOnlySpan<char> source, Span<char> destination, out int charsLength, IDNA2003Options options, out StringPrepErrorType errorType)
        {
            return IDNA2003.TryConvertToUnicode(source, destination, out charsLength, options, out errorType);
        }

        /// <summary>
        /// IDNA2003: Convenience function that implements the IDNToUnicode operation as defined in the IDNA RFC.
        /// This operation is done on complete domain names, e.g: "www.example.com".
        /// <para/>
        /// <b>Note:</b> IDNA RFC specifies that a conformant application should divide a domain name
        /// into separate labels, decide whether to apply allowUnassigned and useSTD3ASCIIRules on each, 
        /// and then convert. This function does not offer that level of granularity. The options once
        /// set will apply to all labels in the domain name.
        /// </summary>
        /// <param name="source">The input string to be processed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.AllowUnassigned"/></term>
        ///         <description>
        ///             Unassigned values can be converted to ASCII for query operations
        ///             If this option is set, the unassigned code points are in the input
        ///             are treated as normal Unicode code points.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.UseSTD3Rules"/></term>
        ///         <description>
        ///             Use STD3 ASCII rules for host name syntax restrictions
        ///             If this option is set and the input does not satisfy STD3 rules,
        ///             the operation will fail with <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns>The converted <see cref="string"/>.</returns>
        /// <exception cref="StringPrepFormatException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static string ConvertIDNToUnicode(string source, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            return ConvertIDNToUnicode(source.AsSpan(), options);
        }

        /// <summary>
        /// IDNA2003: Convenience function that implements the IDNToUnicode operation as defined in the IDNA RFC.
        /// This operation is done on complete domain names, e.g: "www.example.com".
        /// <para/>
        /// <b>Note:</b> IDNA RFC specifies that a conformant application should divide a domain name
        /// into separate labels, decide whether to apply allowUnassigned and useSTD3ASCIIRules on each, 
        /// and then convert. This function does not offer that level of granularity. The options once
        /// set will apply to all labels in the domain name.
        /// </summary>
        /// <param name="source">The input string to be processed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.AllowUnassigned"/></term>
        ///         <description>
        ///             Unassigned values can be converted to ASCII for query operations
        ///             If this option is set, the unassigned code points are in the input
        ///             are treated as normal Unicode code points.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.UseSTD3Rules"/></term>
        ///         <description>
        ///             Use STD3 ASCII rules for host name syntax restrictions
        ///             If this option is set and the input does not satisfy STD3 rules,
        ///             the operation will fail with <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns>The converted <see cref="string"/>.</returns>
        /// <exception cref="StringPrepFormatException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static string ConvertIDNToUnicode(ReadOnlySpan<char> source, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            return IDNA2003.ConvertIDNToUnicode(source, options);
        }

        /// <summary>
        /// IDNA2003: Convenience function that implements the IDNToUnicode operation as defined in the IDNA RFC.
        /// This operation is done on complete domain names, e.g: "www.example.com".
        /// <para/>
        /// <b>Note:</b> IDNA RFC specifies that a conformant application should divide a domain name
        /// into separate labels, decide whether to apply allowUnassigned and useSTD3ASCIIRules on each, 
        /// and then convert. This function does not offer that level of granularity. The options once
        /// set will apply to all labels in the domain name.
        /// </summary>
        /// <param name="source">The input string to be processed.</param>
        /// <param name="destination">The span in which to write the converted value.</param>
        /// <param name="charsLength">Upon return, will contain the length of <paramref name="destination"/> after
        /// the operation. If the return value is <c>false</c>,
        /// this will contain the length of <paramref name="destination"/> that would need to be provided to make the
        /// operation succeed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.AllowUnassigned"/></term>
        ///         <description>
        ///             Unassigned values can be converted to ASCII for query operations
        ///             If this option is set, the unassigned code points are in the input
        ///             are treated as normal Unicode code points.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.UseSTD3Rules"/></term>
        ///         <description>
        ///             Use STD3 ASCII rules for host name syntax restrictions
        ///             If this option is set and the input does not satisfy STD3 rules,
        ///             the operation will fail with <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <param name="errorType">Upon unsuccessful return (<c>false</c>), will contain the type of error that occurred.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static bool TryConvertIDNToUnicode(ReadOnlySpan<char> source, Span<char> destination, out int charsLength, IDNA2003Options options, out StringPrepErrorType errorType)
        {
            return IDNA2003.TryConvertIDNToUnicode(source, destination, out charsLength, options, out errorType);
        }

        /// <summary>
        /// IDNA2003: Compare two IDN strings for equivalence.
        /// This function splits the domain names into labels and compares them.
        /// According to IDN RFC, whenever two labels are compared, they are 
        /// considered equal if and only if their ASCII forms (obtained by 
        /// applying ToASCII) match using an case-insensitive ASCII comparison.
        /// Two domain names are considered a match if and only if all labels 
        /// match regardless of whether label separators match.
        /// </summary>
        /// <param name="s1">First IDN string.</param>
        /// <param name="s2">Second IDN string.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.AllowUnassigned"/></term>
        ///         <description>
        ///             Unassigned values can be converted to ASCII for query operations
        ///             If this option is set, the unassigned code points are in the input
        ///             are treated as normal Unicode code points.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.UseSTD3Rules"/></term>
        ///         <description>
        ///             Use STD3 ASCII rules for host name syntax restrictions
        ///             If this option is set and the input does not satisfy STD3 rules,
        ///             the operation will fail with <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns>0 if the strings are equal, &gt; 0 if s1 &gt; s2 and &lt; 0 if s1 &lt; s2.</returns>
        /// <exception cref="StringPrepFormatException">When an error occurs for parsing a string.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="s1"/> or <paramref name="s2"/> is <c>null</c>.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static int Compare(string s1, string s2, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            if (s1 is null)
                throw new ArgumentNullException(nameof(s1));
            if (s2 is null)
                throw new ArgumentNullException(nameof(s2));

            return Compare(s1.AsSpan(), s2.AsSpan(), options);
        }

        /// <summary>
        /// IDNA2003: Compare two IDN strings for equivalence.
        /// This function splits the domain names into labels and compares them.
        /// According to IDN RFC, whenever two labels are compared, they are 
        /// considered equal if and only if their ASCII forms (obtained by 
        /// applying ToASCII) match using an case-insensitive ASCII comparison.
        /// Two domain names are considered a match if and only if all labels 
        /// match regardless of whether label separators match.
        /// </summary>
        /// <param name="s1">First IDN string.</param>
        /// <param name="s2">Second IDN string.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.AllowUnassigned"/></term>
        ///         <description>
        ///             Unassigned values can be converted to ASCII for query operations
        ///             If this option is set, the unassigned code points are in the input
        ///             are treated as normal Unicode code points.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IDNA2003Options.UseSTD3Rules"/></term>
        ///         <description>
        ///             Use STD3 ASCII rules for host name syntax restrictions
        ///             If this option is set and the input does not satisfy STD3 rules,
        ///             the operation will fail with <see cref="StringPrepFormatException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns>0 if the strings are equal, &gt; 0 if s1 &gt; s2 and &lt; 0 if s1 &lt; s2.</returns>
        /// <exception cref="StringPrepFormatException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static int Compare(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            return IDNA2003.Compare(s1, s2, options);
        }
    }
}
