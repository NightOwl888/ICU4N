using ICU4N.Impl;
using System;
using System.Collections.Generic;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    /// <summary>
    /// IDNA error bit set values.
    /// When a domain name or label fails a processing step or does not meet the
    /// validity criteria, then one or more of these error bits are set.
    /// </summary>
    /// <stable>ICU 4.6</stable>
    public enum IDNAError
    {
        /// <summary>
        /// A non-final domain name label (or the whole domain name) is empty.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        EmptyLabel,
        /// <summary>
        /// A domain name label is longer than 63 bytes.
        /// (See STD13/RFC1034 3.1. Name space specifications and terminology.)
        /// This is only checked in ToASCII operations, and only if the output label is all-ASCII.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        LabelTooLong,
        /// <summary>
        /// A domain name is longer than 255 bytes in its storage form.
        /// (See STD13/RFC1034 3.1. Name space specifications and terminology.)
        /// This is only checked in ToASCII operations, and only if the output domain name is all-ASCII.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        DomainNameTooLong,
        /// <summary>
        /// A label starts with a hyphen-minus ('-').
        /// </summary>
        /// <stable>ICU 4.6</stable>
        LeadingHyphen,
        /// <summary>
        /// A label ends with a hyphen-minus ('-').
        /// </summary>
        /// <stable>ICU 4.6</stable>
        TrailingHyphen,
        /// <summary>
        /// A label contains hyphen-minus ('-') in the third and fourth positions.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        Hyphen_3_4,
        /// <summary>
        /// A label starts with a combining mark.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        LeadingCombiningMark,
        /// <summary>
        /// A label or domain name contains disallowed characters.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        Disallowed,
        /// <summary>
        /// A label starts with "xn--" but does not contain valid Punycode.
        /// That is, an xn-- label failed Punycode decoding.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        Punycode,
        /// <summary>
        /// A label contains a dot=full stop.
        /// This can occur in an input string for a single-label function.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        LabelHasDot,
        /// <summary>
        /// An ACE label does not contain a valid label string.
        /// The label was successfully ACE (Punycode) decoded but the resulting
        /// string had severe validation errors. For example,
        /// it might contain characters that are not allowed in ACE labels,
        /// or it might not be normalized.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        InvalidAceLabel,
        /// <summary>
        /// A label does not meet the IDNA BiDi requirements (for right-to-left characters).
        /// </summary>
        /// <stable>ICU 4.6</stable>
        BiDi,
        /// <summary>
        /// A label does not meet the IDNA CONTEXTJ requirements.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        ContextJ,
        /// <summary>
        /// A label does not meet the IDNA CONTEXTO requirements for punctuation characters.
        /// Some punctuation characters "Would otherwise have been DISALLOWED"
        /// but are allowed in certain contexts. (RFC 5892)
        /// </summary>
        /// <stable>ICU 49</stable>
        ContextOPunctuation,
        /// <summary>
        /// A label does not meet the IDNA CONTEXTO requirements for digits.
        /// Arabic-Indic Digits (U+066x) must not be mixed with Extended Arabic-Indic Digits (U+06Fx).
        /// </summary>
        /// <stable>ICU 49</stable>
        ContextODigits
    }

    /// <summary>
    /// Output container for IDNA processing errors.
    /// The <see cref="IDNAInfo"/> class is not suitable for subclassing.
    /// </summary>
    /// <stable>ICU 4.6</stable>
    public sealed class IDNAInfo
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public IDNAInfo()
        {
            errors = new HashSet<IDNAError>();
            labelErrors = new HashSet<IDNAError>();
            isTransDiff = false;
            isBiDi = false;
            isOkBiDi = true;
        }

        /// <summary>
        /// Were there IDNA processing errors?
        /// Returns true if there were processing errors.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public bool HasErrors { get { return errors.Count > 0; } }
        /// <summary>
        /// Returns a set indicating IDNA processing errors (modifiable, and not null).
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public ISet<IDNAError> Errors { get { return errors; } }
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
        public bool IsTransitionalDifferent { get { return isTransDiff; } }

        internal void Reset()
        {
            errors.Clear();
            labelErrors.Clear();
            isTransDiff = false;
            isBiDi = false;
            isOkBiDi = true;
        }

        internal ISet<IDNAError> errors, labelErrors;
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
    /// ToUnicode operation before displaying the domain name to the user.
    /// IDNA requires that implementations process input strings with 
    /// <a href="http://www.ietf.org/rfc/rfc3491.txt">Nameprep</a>, 
    /// which is a profile of <a href="http://www.ietf.org/rfc/rfc3454.txt">Stringprep</a> , 
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

        // ICU4N specific - LabelToASCII(ICharSequence label, StringBuilder dest, Info info) moved to IDNAExtension.tt

        // ICU4N specific - LabelToUnicode(ICharSequence label, StringBuilder dest, Info info) moved to IDNAExtension.tt

        // ICU4N specific - NameToASCII(ICharSequence name, StringBuilder dest, Info info) moved to IDNAExtension.tt

        // ICU4N specific - NameToUnicode(ICharSequence name, StringBuilder dest, Info info) moved to IDNAExtension.tt

        // ICU4N specific - De-nested Info class and renamed IDNAInfo


        // The following protected methods give IDNA subclasses access to the private IDNAInfo fields.
        // The IDNAInfo also provides intermediate state that is publicly invisible,
        // avoiding the allocation of another worker object.

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal static void ResetInfo(IDNAInfo info) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            info.Reset();
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal static bool HasCertainErrors(IDNAInfo info, ISet<IDNAError> errors) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            return info.errors.Count > 0 && info.errors.Overlaps(errors);
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal static bool HasCertainLabelErrors(IDNAInfo info, ISet<IDNAError> errors) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            return info.labelErrors.Count > 0 && info.labelErrors.Overlaps(errors);
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal static void AddLabelError(IDNAInfo info, IDNAError error) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            info.labelErrors.Add(error);
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal static void PromoteAndResetLabelErrors(IDNAInfo info) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            if (info.labelErrors.Count > 0)
            {
                info.errors.UnionWith(info.labelErrors);
                info.labelErrors.Clear();
            }
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal static void AddError(IDNAInfo info, IDNAError error) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            info.errors.Add(error);
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal static void SetTransitionalDifferent(IDNAInfo info) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            info.isTransDiff = true;
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal static void SetBiDi(IDNAInfo info) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            info.isBiDi = true;
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal static bool IsBiDi(IDNAInfo info) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            return info.isBiDi;
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal static void SetNotOkBiDi(IDNAInfo info) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            info.isOkBiDi = false;
        }
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal static bool IsOkBiDi(IDNAInfo info) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            return info.isOkBiDi;
        }

        // ICU4N specific - de-nested Error enum and renamed IDNAError

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
        /// <param name="src">The input string to be processed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepParseException"/>.
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
        ///             the operation will fail with <see cref="StringPrepParseException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns><see cref="StringBuffer"/> the converted <see cref="string"/>.</returns>
        /// <exception cref="StringPrepParseException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static StringBuffer ConvertToASCII(string src, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            UCharacterIterator iter = UCharacterIterator.GetInstance(src);
            return ConvertToASCII(iter, options);
        }

        /// <summary>
        /// IDNA2003: This function implements the ToASCII operation as defined in the IDNA RFC.
        /// This operation is done on <b>single labels</b> before sending it to something that expects
        /// ASCII names. A label is an individual part of a domain name. Labels are usually
        /// separated by dots; e.g." "www.example.com" is composed of 3 labels 
        /// "www","example", and "com".
        /// </summary>
        /// <param name="src">The input string as <see cref="StringBuffer"/> to be processed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepParseException"/>.
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
        ///             the operation will fail with <see cref="StringPrepParseException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns><see cref="StringBuffer"/> the converted <see cref="string"/>.</returns>
        /// <exception cref="StringPrepParseException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static StringBuffer ConvertToASCII(StringBuffer src, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            UCharacterIterator iter = UCharacterIterator.GetInstance(src);
            return ConvertToASCII(iter, options);
        }

        /// <summary>
        /// IDNA2003: This function implements the ToASCII operation as defined in the IDNA RFC.
        /// This operation is done on <b>single labels</b> before sending it to something that expects
        /// ASCII names. A label is an individual part of a domain name. Labels are usually
        /// separated by dots; e.g." "www.example.com" is composed of 3 labels 
        /// "www","example", and "com".
        /// </summary>
        /// <param name="src">The input string as <see cref="UCharacterIterator"/> to be processed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepParseException"/>.
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
        ///             the operation will fail with <see cref="StringPrepParseException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns><see cref="StringBuffer"/> the converted <see cref="string"/>.</returns>
        /// <exception cref="StringPrepParseException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static StringBuffer ConvertToASCII(UCharacterIterator src, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            return IDNA2003.ConvertToASCII(src, options);
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
        /// <param name="src">The input string as <see cref="UCharacterIterator"/> to be processed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepParseException"/>.
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
        ///             the operation will fail with <see cref="StringPrepParseException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns><see cref="StringBuffer"/> the converted <see cref="string"/>.</returns>
        /// <exception cref="StringPrepParseException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static StringBuffer ConvertIDNToASCII(UCharacterIterator src, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            return ConvertIDNToASCII(src.GetText(), options);
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
        /// <param name="src">The input string as <see cref="StringBuffer"/> to be processed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepParseException"/>.
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
        ///             the operation will fail with <see cref="StringPrepParseException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns><see cref="StringBuffer"/> the converted <see cref="string"/>.</returns>
        /// <exception cref="StringPrepParseException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static StringBuffer ConvertIDNToASCII(StringBuffer src, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            return ConvertIDNToASCII(src.ToString(), options);
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
        /// <param name="src">The input string to be processed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepParseException"/>.
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
        ///             the operation will fail with <see cref="StringPrepParseException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns><see cref="StringBuffer"/> the converted <see cref="string"/>.</returns>
        /// <exception cref="StringPrepParseException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static StringBuffer ConvertIDNToASCII(string src, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            return IDNA2003.ConvertIDNToASCII(src, options);
        }

        /// <summary>
        /// IDNA2003: This function implements the ToUnicode operation as defined in the IDNA RFC.
        /// This operation is done on <b>single labels</b> before sending it to something that expects
        /// Unicode names. A label is an individual part of a domain name. Labels are usually
        /// separated by dots; for e.g." "www.example.com" is composed of 3 labels 
        /// "www","example", and "com".
        /// </summary>
        /// <param name="src">The input string to be processed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepParseException"/>.
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
        ///             the operation will fail with <see cref="StringPrepParseException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns><see cref="StringBuffer"/> the converted <see cref="string"/>.</returns>
        /// <exception cref="StringPrepParseException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static StringBuffer ConvertToUnicode(string src, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            UCharacterIterator iter = UCharacterIterator.GetInstance(src);
            return ConvertToUnicode(iter, options);
        }

        /// <summary>
        /// IDNA2003: This function implements the ToUnicode operation as defined in the IDNA RFC.
        /// This operation is done on <b>single labels</b> before sending it to something that expects
        /// Unicode names. A label is an individual part of a domain name. Labels are usually
        /// separated by dots; for e.g." "www.example.com" is composed of 3 labels 
        /// "www","example", and "com".
        /// </summary>
        /// <param name="src">The input string as <see cref="StringBuffer"/> to be processed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepParseException"/>.
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
        ///             the operation will fail with <see cref="StringPrepParseException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns><see cref="StringBuffer"/> the converted <see cref="string"/>.</returns>
        /// <exception cref="StringPrepParseException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static StringBuffer ConvertToUnicode(StringBuffer src, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            UCharacterIterator iter = UCharacterIterator.GetInstance(src);
            return ConvertToUnicode(iter, options);
        }

        /// <summary>
        /// IDNA2003: This function implements the ToUnicode operation as defined in the IDNA RFC.
        /// This operation is done on <b>single labels</b> before sending it to something that expects
        /// Unicode names. A label is an individual part of a domain name. Labels are usually
        /// separated by dots; for e.g." "www.example.com" is composed of 3 labels 
        /// "www","example", and "com".
        /// </summary>
        /// <param name="src">The input string as <see cref="UCharacterIterator"/> to be processed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepParseException"/>.
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
        ///             the operation will fail with <see cref="StringPrepParseException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns><see cref="StringBuffer"/> the converted <see cref="string"/>.</returns>
        /// <exception cref="StringPrepParseException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static StringBuffer ConvertToUnicode(UCharacterIterator src, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            return IDNA2003.ConvertToUnicode(src, options);
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
        /// <param name="src">The input string as <see cref="UCharacterIterator"/> to be processed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepParseException"/>.
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
        ///             the operation will fail with <see cref="StringPrepParseException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns><see cref="StringBuffer"/> the converted <see cref="string"/>.</returns>
        /// <exception cref="StringPrepParseException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static StringBuffer ConvertIDNToUnicode(UCharacterIterator src, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            return ConvertIDNToUnicode(src.GetText(), options);
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
        /// <param name="src">The input string as <see cref="StringBuffer"/> to be processed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepParseException"/>.
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
        ///             the operation will fail with <see cref="StringPrepParseException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns><see cref="StringBuffer"/> the converted <see cref="string"/>.</returns>
        /// <exception cref="StringPrepParseException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static StringBuffer ConvertIDNToUnicode(StringBuffer src, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            return ConvertIDNToUnicode(src.ToString(), options);
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
        /// <param name="src">The input string to be processed.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepParseException"/>.
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
        ///             the operation will fail with <see cref="StringPrepParseException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns><see cref="StringBuffer"/> the converted <see cref="string"/>.</returns>
        /// <exception cref="StringPrepParseException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static StringBuffer ConvertIDNToUnicode(string src, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            return IDNA2003.ConvertIDNToUnicode(src, options);
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
        /// <param name="s1">First IDN string as <see cref="StringBuffer"/>.</param>
        /// <param name="s2">Second IDN string as <see cref="StringBuffer"/>.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepParseException"/>.
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
        ///             the operation will fail with <see cref="StringPrepParseException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns>0 if the strings are equal, &gt; 0 if s1 &gt; s2 and &lt; 0 if s1 &lt; s2.</returns>
        /// <exception cref="StringPrepParseException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static int Compare(StringBuffer s1, StringBuffer s2, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            if (s1 == null || s2 == null)
            {
                throw new ArgumentException("One of the source buffers is null");
            }
            return IDNA2003.Compare(s1.ToString(), s2.ToString(), options);
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
        ///             <see cref="StringPrepParseException"/>.
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
        ///             the operation will fail with <see cref="StringPrepParseException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns>0 if the strings are equal, &gt; 0 if s1 &gt; s2 and &lt; 0 if s1 &lt; s2.</returns>
        /// <exception cref="StringPrepParseException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static int Compare(string s1, string s2, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            if (s1 == null || s2 == null)
            {
                throw new ArgumentException("One of the source buffers is null");
            }
            return IDNA2003.Compare(s1, s2, options);
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
        /// <param name="s1">First IDN string as <see cref="UCharacterIterator"/>.</param>
        /// <param name="s2">Second IDN string as <see cref="UCharacterIterator"/>.</param>
        /// <param name="options">A bit set of options:
        /// <list type="table">
        ///     <item>
        ///         <term><see cref="IDNA2003Options.Default"/></term>
        ///         <description>
        ///             Use default options, i.e., do not process unassigned code points
        ///             and do not use STD3 ASCII rules
        ///             If unassigned code points are found the operation fails with 
        ///             <see cref="StringPrepParseException"/>.
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
        ///             the operation will fail with <see cref="StringPrepParseException"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </param>
        /// <returns>0 if the strings are equal, &gt; 0 if s1 &gt; s2 and &lt; 0 if s1 &lt; s2.</returns>
        /// <exception cref="StringPrepParseException">When an error occurs for parsing a string.</exception>
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(IDNAOptions).")]
        internal static int Compare(UCharacterIterator s1, UCharacterIterator s2, IDNA2003Options options) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            if (s1 == null || s2 == null)
            {
                throw new ArgumentException("One of the source buffers is null");
            }
            return IDNA2003.Compare(s1.GetText(), s2.GetText(), options);
        }
    }
}
