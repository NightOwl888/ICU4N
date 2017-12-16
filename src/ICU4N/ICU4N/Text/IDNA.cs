using ICU4N.Impl;
using ICU4N.Support.Text;
using System;
using System.Collections.Generic;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    public abstract partial class IDNA
    {
        /** 
         * Default options value: None of the other options are set.
         * For use in static worker and factory methods.
         * @stable ICU 2.8
         */
        public static readonly int DEFAULT = 0;
        /** 
         * Option to allow unassigned code points in domain names and labels.
         * For use in static worker and factory methods.
         * <p>This option is ignored by the UTS46 implementation.
         * (UTS #46 disallows unassigned code points.)
         * @deprecated ICU 55 Use UTS 46 instead via {@link #getUTS46Instance(int)}.
         */
        [Obsolete("ICU 55 Use UTS 46 instead via GetUtS46Instance(int)")]
        public static readonly int ALLOW_UNASSIGNED = 1;
        /** 
         * Option to check whether the input conforms to the STD3 ASCII rules,
         * for example the restriction of labels to LDH characters
         * (ASCII Letters, Digits and Hyphen-Minus).
         * For use in static worker and factory methods.
         * @stable ICU 2.8
         */
        public static readonly int USE_STD3_RULES = 2;
        /**
         * IDNA option to check for whether the input conforms to the BiDi rules.
         * For use in static worker and factory methods.
         * <p>This option is ignored by the IDNA2003 implementation.
         * (IDNA2003 always performs a BiDi check.)
         * @stable ICU 4.6
         */
        public static readonly int CHECK_BIDI = 4;
        /**
         * IDNA option to check for whether the input conforms to the CONTEXTJ rules.
         * For use in static worker and factory methods.
         * <p>This option is ignored by the IDNA2003 implementation.
         * (The CONTEXTJ check is new in IDNA2008.)
         * @stable ICU 4.6
         */
        public static readonly int CHECK_CONTEXTJ = 8;
        /**
         * IDNA option for nontransitional processing in ToASCII().
         * For use in static worker and factory methods.
         * <p>By default, ToASCII() uses transitional processing.
         * <p>This option is ignored by the IDNA2003 implementation.
         * (This is only relevant for compatibility of newer IDNA implementations with IDNA2003.)
         * @stable ICU 4.6
         */
        public static readonly int NONTRANSITIONAL_TO_ASCII = 0x10;
        /**
         * IDNA option for nontransitional processing in ToUnicode().
         * For use in static worker and factory methods.
         * <p>By default, ToUnicode() uses transitional processing.
         * <p>This option is ignored by the IDNA2003 implementation.
         * (This is only relevant for compatibility of newer IDNA implementations with IDNA2003.)
         * @stable ICU 4.6
         */
        public static readonly int NONTRANSITIONAL_TO_UNICODE = 0x20;
        /**
         * IDNA option to check for whether the input conforms to the CONTEXTO rules.
         * For use in static worker and factory methods.
         * <p>This option is ignored by the IDNA2003 implementation.
         * (The CONTEXTO check is new in IDNA2008.)
         * <p>This is for use by registries for IDNA2008 conformance.
         * UTS #46 does not require the CONTEXTO check.
         * @stable ICU 49
         */
        public static readonly int CHECK_CONTEXTO = 0x40;

        /**
         * Returns an IDNA instance which implements UTS #46.
         * Returns an unmodifiable instance, owned by the caller.
         * Cache it for multiple operations, and delete it when done.
         * The instance is thread-safe, that is, it can be used concurrently.
         * <p>
         * UTS #46 defines Unicode IDNA Compatibility Processing,
         * updated to the latest version of Unicode and compatible with both
         * IDNA2003 and IDNA2008.
         * <p>
         * The worker functions use transitional processing, including deviation mappings,
         * unless NONTRANSITIONAL_TO_ASCII or NONTRANSITIONAL_TO_UNICODE
         * is used in which case the deviation characters are passed through without change.
         * <p>
         * Disallowed characters are mapped to U+FFFD.
         * <p>
         * Operations with the UTS #46 instance do not support the
         * ALLOW_UNASSIGNED option.
         * <p>
         * By default, the UTS #46 implementation allows all ASCII characters (as valid or mapped).
         * When the USE_STD3_RULES option is used, ASCII characters other than
         * letters, digits, hyphen (LDH) and dot/full stop are disallowed and mapped to U+FFFD.
         *
         * @param options Bit set to modify the processing and error checking.
         * @return the UTS #46 IDNA instance, if successful
         * @stable ICU 4.6
         */
        public static IDNA GetUTS46Instance(int options)
        {
            return new UTS46(options);
        }

        // ICU4N specific - LabelToASCII(ICharSequence label, StringBuilder dest, Info info) moved to IDNAExtension.tt

        // ICU4N specific - LabelToUnicode(ICharSequence label, StringBuilder dest, Info info) moved to IDNAExtension.tt

        // ICU4N specific - NameToASCII(ICharSequence name, StringBuilder dest, Info info) moved to IDNAExtension.tt

        // ICU4N specific - NameToUnicode(ICharSequence name, StringBuilder dest, Info info) moved to IDNAExtension.tt

        /**
         * Output container for IDNA processing errors.
         * The Info class is not suitable for subclassing.
         * @stable ICU 4.6
         */
        public sealed class Info
        {
            /**
             * Constructor.
             * @stable ICU 4.6
             */
            public Info()
            {
                errors = new HashSet<Error>();
                labelErrors = new HashSet<Error>();
                isTransDiff = false;
                isBiDi = false;
                isOkBiDi = true;
            }
            /**
             * Were there IDNA processing errors?
             * @return true if there were processing errors
             * @stable ICU 4.6
             */
            public bool HasErrors { get { return errors.Count > 0; } }
            /**
             * Returns a set indicating IDNA processing errors.
             * @return set of processing errors (modifiable, and not null)
             * @stable ICU 4.6
             */
            public ISet<Error> Errors { get { return errors; } }
            /**
             * Returns true if transitional and nontransitional processing produce different results.
             * This is the case when the input label or domain name contains
             * one or more deviation characters outside a Punycode label (see UTS #46).
             * <ul>
             * <li>With nontransitional processing, such characters are
             * copied to the destination string.
             * <li>With transitional processing, such characters are
             * mapped (sharp s/sigma) or removed (joiner/nonjoiner).
             * </ul>
             * @return true if transitional and nontransitional processing produce different results
             * @stable ICU 4.6
             */
            public bool IsTransitionalDifferent { get { return isTransDiff; } }

            internal void Reset()
            {
                errors.Clear();
                labelErrors.Clear();
                isTransDiff = false;
                isBiDi = false;
                isOkBiDi = true;
            }

            internal ISet<Error> errors, labelErrors;
            internal bool isTransDiff;
            internal bool isBiDi;
            internal bool isOkBiDi;
        }

        // The following protected methods give IDNA subclasses access to the private IDNAInfo fields.
        // The IDNAInfo also provides intermediate state that is publicly invisible,
        // avoiding the allocation of another worker object.
        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        protected static void ResetInfo(Info info)
        {
            info.Reset();
        }
        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        protected static bool HasCertainErrors(Info info, ISet<Error> errors)
        {
            return info.errors.Count > 0 && info.errors.Overlaps(errors);
        }
        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        protected static bool HasCertainLabelErrors(Info info, ISet<Error> errors)
        {
            return info.labelErrors.Count > 0 && info.labelErrors.Overlaps(errors);
        }
        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        protected static void AddLabelError(Info info, Error error)
        {
            info.labelErrors.Add(error);
        }
        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        protected static void PromoteAndResetLabelErrors(Info info)
        {
            if (info.labelErrors.Count > 0)
            {
                info.errors.UnionWith(info.labelErrors);
                info.labelErrors.Clear();
            }
        }
        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        protected static void AddError(Info info, Error error)
        {
            info.errors.Add(error);
        }
        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        protected static void SetTransitionalDifferent(Info info)
        {
            info.isTransDiff = true;
        }
        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        protected static void SetBiDi(Info info)
        {
            info.isBiDi = true;
        }
        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        protected static bool IsBiDi(Info info)
        {
            return info.isBiDi;
        }
        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        protected static void SetNotOkBiDi(Info info)
        {
            info.isOkBiDi = false;
        }
        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        protected static bool IsOkBiDi(Info info)
        {
            return info.isOkBiDi;
        }

        /**
         * IDNA error bit set values.
         * When a domain name or label fails a processing step or does not meet the
         * validity criteria, then one or more of these error bits are set.
         * @stable ICU 4.6
         */
        public enum Error // ICU4N TODO: De-nest and rename to follow .NET Conventions.
        {
            /**
             * A non-final domain name label (or the whole domain name) is empty.
             * @stable ICU 4.6
             */
            EMPTY_LABEL,
            /**
             * A domain name label is longer than 63 bytes.
             * (See STD13/RFC1034 3.1. Name space specifications and terminology.)
             * This is only checked in ToASCII operations, and only if the output label is all-ASCII.
             * @stable ICU 4.6
             */
            LABEL_TOO_LONG,
            /**
             * A domain name is longer than 255 bytes in its storage form.
             * (See STD13/RFC1034 3.1. Name space specifications and terminology.)
             * This is only checked in ToASCII operations, and only if the output domain name is all-ASCII.
             * @stable ICU 4.6
             */
            DOMAIN_NAME_TOO_LONG,
            /**
             * A label starts with a hyphen-minus ('-').
             * @stable ICU 4.6
             */
            LEADING_HYPHEN,
            /**
             * A label ends with a hyphen-minus ('-').
             * @stable ICU 4.6
             */
            TRAILING_HYPHEN,
            /**
             * A label contains hyphen-minus ('-') in the third and fourth positions.
             * @stable ICU 4.6
             */
            HYPHEN_3_4,
            /**
             * A label starts with a combining mark.
             * @stable ICU 4.6
             */
            LEADING_COMBINING_MARK,
            /**
             * A label or domain name contains disallowed characters.
             * @stable ICU 4.6
             */
            DISALLOWED,
            /**
             * A label starts with "xn--" but does not contain valid Punycode.
             * That is, an xn-- label failed Punycode decoding.
             * @stable ICU 4.6
             */
            PUNYCODE,
            /**
             * A label contains a dot=full stop.
             * This can occur in an input string for a single-label function.
             * @stable ICU 4.6
             */
            LABEL_HAS_DOT,
            /**
             * An ACE label does not contain a valid label string.
             * The label was successfully ACE (Punycode) decoded but the resulting
             * string had severe validation errors. For example,
             * it might contain characters that are not allowed in ACE labels,
             * or it might not be normalized.
             * @stable ICU 4.6
             */
            INVALID_ACE_LABEL,
            /**
             * A label does not meet the IDNA BiDi requirements (for right-to-left characters).
             * @stable ICU 4.6
             */
            BIDI,
            /**
             * A label does not meet the IDNA CONTEXTJ requirements.
             * @stable ICU 4.6
             */
            CONTEXTJ,
            /**
             * A label does not meet the IDNA CONTEXTO requirements for punctuation characters.
             * Some punctuation characters "Would otherwise have been DISALLOWED"
             * but are allowed in certain contexts. (RFC 5892)
             * @stable ICU 49
             */
            CONTEXTO_PUNCTUATION,
            /**
             * A label does not meet the IDNA CONTEXTO requirements for digits.
             * Arabic-Indic Digits (U+066x) must not be mixed with Extended Arabic-Indic Digits (U+06Fx).
             * @stable ICU 49
             */
            CONTEXTO_DIGITS
        }

        /**
         * Sole constructor. (For invocation by subclass constructors, typically implicit.)
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        protected IDNA()
        {
        }

        /* IDNA2003 API ------------------------------------------------------------- */

        /**
         * IDNA2003: This function implements the ToASCII operation as defined in the IDNA RFC.
         * This operation is done on <b>single labels</b> before sending it to something that expects
         * ASCII names. A label is an individual part of a domain name. Labels are usually
         * separated by dots; e.g." "www.example.com" is composed of 3 labels 
         * "www","example", and "com".
         *
         * @param src       The input string to be processed
         * @param options   A bit set of options:
         *  - IDNA.DEFAULT              Use default options, i.e., do not process unassigned code points
         *                              and do not use STD3 ASCII rules
         *                              If unassigned code points are found the operation fails with 
         *                              StringPrepParseException.
         *
         *  - IDNA.ALLOW_UNASSIGNED     Unassigned values can be converted to ASCII for query operations
         *                              If this option is set, the unassigned code points are in the input 
         *                              are treated as normal Unicode code points.
         *                          
         *  - IDNA.USE_STD3_RULES       Use STD3 ASCII rules for host name syntax restrictions
         *                              If this option is set and the input does not satisfy STD3 rules,  
         *                              the operation will fail with ParseException
         * @return StringBuffer the converted String
         * @throws StringPrepParseException When an error occurs for parsing a string.
         * @deprecated ICU 55 Use UTS 46 instead via {@link #getUTS46Instance(int)}.
         */
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(int).")]
        public static StringBuffer ConvertToASCII(string src, int options)
        {
            UCharacterIterator iter = UCharacterIterator.GetInstance(src);
            return ConvertToASCII(iter, options);
        }

        /**
         * IDNA2003: This function implements the ToASCII operation as defined in the IDNA RFC.
         * This operation is done on <b>single labels</b> before sending it to something that expects
         * ASCII names. A label is an individual part of a domain name. Labels are usually
         * separated by dots; e.g." "www.example.com" is composed of 3 labels 
         * "www","example", and "com".
         *
         * @param src       The input string as StringBuffer to be processed
         * @param options   A bit set of options:
         *  - IDNA.DEFAULT              Use default options, i.e., do not process unassigned code points
         *                              and do not use STD3 ASCII rules
         *                              If unassigned code points are found the operation fails with 
         *                              ParseException.
         *
         *  - IDNA.ALLOW_UNASSIGNED     Unassigned values can be converted to ASCII for query operations
         *                              If this option is set, the unassigned code points are in the input 
         *                              are treated as normal Unicode code points.
         *                          
         *  - IDNA.USE_STD3_RULES       Use STD3 ASCII rules for host name syntax restrictions
         *                              If this option is set and the input does not satisfy STD3 rules,  
         *                              the operation will fail with ParseException
         * @return StringBuffer the converted String
         * @deprecated ICU 55 Use UTS 46 instead via {@link #getUTS46Instance(int)}.
         */
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(int).")]
        public static StringBuffer ConvertToASCII(StringBuffer src, int options)
        {
            UCharacterIterator iter = UCharacterIterator.GetInstance(src);
            return ConvertToASCII(iter, options);
        }

        /**
         * IDNA2003: This function implements the ToASCII operation as defined in the IDNA RFC.
         * This operation is done on <b>single labels</b> before sending it to something that expects
         * ASCII names. A label is an individual part of a domain name. Labels are usually
         * separated by dots; e.g." "www.example.com" is composed of 3 labels 
         * "www","example", and "com".
         *
         * @param src       The input string as UCharacterIterator to be processed
         * @param options   A bit set of options:
         *  - IDNA.DEFAULT              Use default options, i.e., do not process unassigned code points
         *                              and do not use STD3 ASCII rules
         *                              If unassigned code points are found the operation fails with 
         *                              ParseException.
         *
         *  - IDNA.ALLOW_UNASSIGNED     Unassigned values can be converted to ASCII for query operations
         *                              If this option is set, the unassigned code points are in the input 
         *                              are treated as normal Unicode code points.
         *                          
         *  - IDNA.USE_STD3_RULES       Use STD3 ASCII rules for host name syntax restrictions
         *                              If this option is set and the input does not satisfy STD3 rules,  
         *                              the operation will fail with ParseException
         * @return StringBuffer the converted String
         * @deprecated ICU 55 Use UTS 46 instead via {@link #getUTS46Instance(int)}.
         */
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(int).")]
        public static StringBuffer ConvertToASCII(UCharacterIterator src, int options)
        {
            return IDNA2003.ConvertToASCII(src, options);
        }

        /**
         * IDNA2003: Convenience function that implements the IDNToASCII operation as defined in the IDNA RFC.
         * This operation is done on complete domain names, e.g: "www.example.com". 
         * It is important to note that this operation can fail. If it fails, then the input 
         * domain name cannot be used as an Internationalized Domain Name and the application
         * should have methods defined to deal with the failure.
         * 
         * <b>Note:</b> IDNA RFC specifies that a conformant application should divide a domain name
         * into separate labels, decide whether to apply allowUnassigned and useSTD3ASCIIRules on each, 
         * and then convert. This function does not offer that level of granularity. The options once  
         * set will apply to all labels in the domain name
         *
         * @param src       The input string as UCharacterIterator to be processed
         * @param options   A bit set of options:
         *  - IDNA.DEFAULT              Use default options, i.e., do not process unassigned code points
         *                              and do not use STD3 ASCII rules
         *                              If unassigned code points are found the operation fails with 
         *                              ParseException.
         *
         *  - IDNA.ALLOW_UNASSIGNED     Unassigned values can be converted to ASCII for query operations
         *                              If this option is set, the unassigned code points are in the input 
         *                              are treated as normal Unicode code points.
         *                          
         *  - IDNA.USE_STD3_RULES       Use STD3 ASCII rules for host name syntax restrictions
         *                              If this option is set and the input does not satisfy STD3 rules,  
         *                              the operation will fail with ParseException
         * @return StringBuffer the converted String
         * @deprecated ICU 55 Use UTS 46 instead via {@link #getUTS46Instance(int)}.
         */
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(int).")]
        public static StringBuffer ConvertIDNToASCII(UCharacterIterator src, int options)
        {
            return ConvertIDNToASCII(src.GetText(), options);
        }

        /**
         * IDNA2003: Convenience function that implements the IDNToASCII operation as defined in the IDNA RFC.
         * This operation is done on complete domain names, e.g: "www.example.com". 
         * It is important to note that this operation can fail. If it fails, then the input 
         * domain name cannot be used as an Internationalized Domain Name and the application
         * should have methods defined to deal with the failure.
         * 
         * <b>Note:</b> IDNA RFC specifies that a conformant application should divide a domain name
         * into separate labels, decide whether to apply allowUnassigned and useSTD3ASCIIRules on each, 
         * and then convert. This function does not offer that level of granularity. The options once  
         * set will apply to all labels in the domain name
         *
         * @param src       The input string as a StringBuffer to be processed
         * @param options   A bit set of options:
         *  - IDNA.DEFAULT              Use default options, i.e., do not process unassigned code points
         *                              and do not use STD3 ASCII rules
         *                              If unassigned code points are found the operation fails with 
         *                              ParseException.
         *
         *  - IDNA.ALLOW_UNASSIGNED     Unassigned values can be converted to ASCII for query operations
         *                              If this option is set, the unassigned code points are in the input 
         *                              are treated as normal Unicode code points.
         *                          
         *  - IDNA.USE_STD3_RULES       Use STD3 ASCII rules for host name syntax restrictions
         *                              If this option is set and the input does not satisfy STD3 rules,  
         *                              the operation will fail with ParseException
         * @return StringBuffer the converted String
         * @deprecated ICU 55 Use UTS 46 instead via {@link #getUTS46Instance(int)}.
         */
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(int).")]
        public static StringBuffer ConvertIDNToASCII(StringBuffer src, int options)
        {
            return ConvertIDNToASCII(src.ToString(), options);
        }

        /**
         * IDNA2003: Convenience function that implements the IDNToASCII operation as defined in the IDNA RFC.
         * This operation is done on complete domain names, e.g: "www.example.com". 
         * It is important to note that this operation can fail. If it fails, then the input 
         * domain name cannot be used as an Internationalized Domain Name and the application
         * should have methods defined to deal with the failure.
         * 
         * <b>Note:</b> IDNA RFC specifies that a conformant application should divide a domain name
         * into separate labels, decide whether to apply allowUnassigned and useSTD3ASCIIRules on each, 
         * and then convert. This function does not offer that level of granularity. The options once  
         * set will apply to all labels in the domain name
         *
         * @param src       The input string to be processed
         * @param options   A bit set of options:
         *  - IDNA.DEFAULT              Use default options, i.e., do not process unassigned code points
         *                              and do not use STD3 ASCII rules
         *                              If unassigned code points are found the operation fails with 
         *                              ParseException.
         *
         *  - IDNA.ALLOW_UNASSIGNED     Unassigned values can be converted to ASCII for query operations
         *                              If this option is set, the unassigned code points are in the input 
         *                              are treated as normal Unicode code points.
         *                          
         *  - IDNA.USE_STD3_RULES       Use STD3 ASCII rules for host name syntax restrictions
         *                              If this option is set and the input does not satisfy STD3 rules,  
         *                              the operation will fail with ParseException
         * @return StringBuffer the converted String
         * @deprecated ICU 55 Use UTS 46 instead via {@link #getUTS46Instance(int)}.
         */
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(int).")]
        public static StringBuffer ConvertIDNToASCII(string src, int options)
        {
            return IDNA2003.ConvertIDNToASCII(src, options);
        }


        /**
         * IDNA2003: This function implements the ToUnicode operation as defined in the IDNA RFC.
         * This operation is done on <b>single labels</b> before sending it to something that expects
         * Unicode names. A label is an individual part of a domain name. Labels are usually
         * separated by dots; for e.g." "www.example.com" is composed of 3 labels 
         * "www","example", and "com".
         * 
         * @param src       The input string to be processed
         * @param options   A bit set of options:
         *  - IDNA.DEFAULT              Use default options, i.e., do not process unassigned code points
         *                              and do not use STD3 ASCII rules
         *                              If unassigned code points are found the operation fails with 
         *                              ParseException.
         *
         *  - IDNA.ALLOW_UNASSIGNED     Unassigned values can be converted to ASCII for query operations
         *                              If this option is set, the unassigned code points are in the input 
         *                              are treated as normal Unicode code points.
         *                          
         *  - IDNA.USE_STD3_RULES       Use STD3 ASCII rules for host name syntax restrictions
         *                              If this option is set and the input does not satisfy STD3 rules,  
         *                              the operation will fail with ParseException
         * @return StringBuffer the converted String
         * @deprecated ICU 55 Use UTS 46 instead via {@link #getUTS46Instance(int)}.
         */
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(int).")]
        public static StringBuffer ConvertToUnicode(string src, int options)
        {
            UCharacterIterator iter = UCharacterIterator.GetInstance(src);
            return ConvertToUnicode(iter, options);
        }

        /**
         * IDNA2003: This function implements the ToUnicode operation as defined in the IDNA RFC.
         * This operation is done on <b>single labels</b> before sending it to something that expects
         * Unicode names. A label is an individual part of a domain name. Labels are usually
         * separated by dots; for e.g." "www.example.com" is composed of 3 labels 
         * "www","example", and "com".
         * 
         * @param src       The input string as StringBuffer to be processed
         * @param options   A bit set of options:
         *  - IDNA.DEFAULT              Use default options, i.e., do not process unassigned code points
         *                              and do not use STD3 ASCII rules
         *                              If unassigned code points are found the operation fails with 
         *                              ParseException.
         *
         *  - IDNA.ALLOW_UNASSIGNED     Unassigned values can be converted to ASCII for query operations
         *                              If this option is set, the unassigned code points are in the input 
         *                              are treated as normal Unicode code points.
         *                          
         *  - IDNA.USE_STD3_RULES       Use STD3 ASCII rules for host name syntax restrictions
         *                              If this option is set and the input does not satisfy STD3 rules,  
         *                              the operation will fail with ParseException
         * @return StringBuffer the converted String
         * @deprecated ICU 55 Use UTS 46 instead via {@link #getUTS46Instance(int)}.
         */
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(int).")]
        public static StringBuffer ConvertToUnicode(StringBuffer src, int options)
        {
            UCharacterIterator iter = UCharacterIterator.GetInstance(src);
            return ConvertToUnicode(iter, options);
        }

        /**
         * IDNA2003: Function that implements the ToUnicode operation as defined in the IDNA RFC.
         * This operation is done on <b>single labels</b> before sending it to something that expects
         * Unicode names. A label is an individual part of a domain name. Labels are usually
         * separated by dots; for e.g." "www.example.com" is composed of 3 labels 
         * "www","example", and "com".
         * 
         * @param src       The input string as UCharacterIterator to be processed
         * @param options   A bit set of options:
         *  - IDNA.DEFAULT              Use default options, i.e., do not process unassigned code points
         *                              and do not use STD3 ASCII rules
         *                              If unassigned code points are found the operation fails with 
         *                              ParseException.
         *
         *  - IDNA.ALLOW_UNASSIGNED     Unassigned values can be converted to ASCII for query operations
         *                              If this option is set, the unassigned code points are in the input 
         *                              are treated as normal Unicode code points.
         *                          
         *  - IDNA.USE_STD3_RULES       Use STD3 ASCII rules for host name syntax restrictions
         *                              If this option is set and the input does not satisfy STD3 rules,  
         *                              the operation will fail with ParseException
         * @return StringBuffer the converted String
         * @deprecated ICU 55 Use UTS 46 instead via {@link #getUTS46Instance(int)}.
         */
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(int).")]
        public static StringBuffer ConvertToUnicode(UCharacterIterator src, int options)
        {
            return IDNA2003.ConvertToUnicode(src, options);
        }

        /**
         * IDNA2003: Convenience function that implements the IDNToUnicode operation as defined in the IDNA RFC.
         * This operation is done on complete domain names, e.g: "www.example.com". 
         *
         * <b>Note:</b> IDNA RFC specifies that a conformant application should divide a domain name
         * into separate labels, decide whether to apply allowUnassigned and useSTD3ASCIIRules on each, 
         * and then convert. This function does not offer that level of granularity. The options once  
         * set will apply to all labels in the domain name
         *
         * @param src       The input string as UCharacterIterator to be processed
         * @param options   A bit set of options:
         *  - IDNA.DEFAULT              Use default options, i.e., do not process unassigned code points
         *                              and do not use STD3 ASCII rules
         *                              If unassigned code points are found the operation fails with 
         *                              ParseException.
         *
         *  - IDNA.ALLOW_UNASSIGNED     Unassigned values can be converted to ASCII for query operations
         *                              If this option is set, the unassigned code points are in the input 
         *                              are treated as normal Unicode code points.
         *                          
         *  - IDNA.USE_STD3_RULES       Use STD3 ASCII rules for host name syntax restrictions
         *                              If this option is set and the input does not satisfy STD3 rules,  
         *                              the operation will fail with ParseException
         * @return StringBuffer the converted String
         * @deprecated ICU 55 Use UTS 46 instead via {@link #getUTS46Instance(int)}.
         */
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(int).")]
        public static StringBuffer ConvertIDNToUnicode(UCharacterIterator src, int options)
        {
            return ConvertIDNToUnicode(src.GetText(), options);
        }

        /**
         * IDNA2003: Convenience function that implements the IDNToUnicode operation as defined in the IDNA RFC.
         * This operation is done on complete domain names, e.g: "www.example.com". 
         *
         * <b>Note:</b> IDNA RFC specifies that a conformant application should divide a domain name
         * into separate labels, decide whether to apply allowUnassigned and useSTD3ASCIIRules on each, 
         * and then convert. This function does not offer that level of granularity. The options once  
         * set will apply to all labels in the domain name
         *
         * @param src       The input string as StringBuffer to be processed
         * @param options   A bit set of options:
         *  - IDNA.DEFAULT              Use default options, i.e., do not process unassigned code points
         *                              and do not use STD3 ASCII rules
         *                              If unassigned code points are found the operation fails with 
         *                              ParseException.
         *
         *  - IDNA.ALLOW_UNASSIGNED     Unassigned values can be converted to ASCII for query operations
         *                              If this option is set, the unassigned code points are in the input 
         *                              are treated as normal Unicode code points.
         *                          
         *  - IDNA.USE_STD3_RULES       Use STD3 ASCII rules for host name syntax restrictions
         *                              If this option is set and the input does not satisfy STD3 rules,  
         *                              the operation will fail with ParseException
         * @return StringBuffer the converted String
         * @deprecated ICU 55 Use UTS 46 instead via {@link #getUTS46Instance(int)}.
         */
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(int).")]
        public static StringBuffer ConvertIDNToUnicode(StringBuffer src, int options)
        {
            return ConvertIDNToUnicode(src.ToString(), options);
        }

        /**
         * IDNA2003: Convenience function that implements the IDNToUnicode operation as defined in the IDNA RFC.
         * This operation is done on complete domain names, e.g: "www.example.com". 
         *
         * <b>Note:</b> IDNA RFC specifies that a conformant application should divide a domain name
         * into separate labels, decide whether to apply allowUnassigned and useSTD3ASCIIRules on each, 
         * and then convert. This function does not offer that level of granularity. The options once  
         * set will apply to all labels in the domain name
         *
         * @param src       The input string to be processed
         * @param options   A bit set of options:
         *  - IDNA.DEFAULT              Use default options, i.e., do not process unassigned code points
         *                              and do not use STD3 ASCII rules
         *                              If unassigned code points are found the operation fails with 
         *                              ParseException.
         *
         *  - IDNA.ALLOW_UNASSIGNED     Unassigned values can be converted to ASCII for query operations
         *                              If this option is set, the unassigned code points are in the input 
         *                              are treated as normal Unicode code points.
         *                          
         *  - IDNA.USE_STD3_RULES       Use STD3 ASCII rules for host name syntax restrictions
         *                              If this option is set and the input does not satisfy STD3 rules,  
         *                              the operation will fail with ParseException
         * @return StringBuffer the converted String
         * @deprecated ICU 55 Use UTS 46 instead via {@link #getUTS46Instance(int)}.
         */
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(int).")]
        public static StringBuffer ConvertIDNToUnicode(string src, int options)
        {
            return IDNA2003.ConvertIDNToUnicode(src, options);
        }

        /**
         * IDNA2003: Compare two IDN strings for equivalence.
         * This function splits the domain names into labels and compares them.
         * According to IDN RFC, whenever two labels are compared, they are 
         * considered equal if and only if their ASCII forms (obtained by 
         * applying toASCII) match using an case-insensitive ASCII comparison.
         * Two domain names are considered a match if and only if all labels 
         * match regardless of whether label separators match.
         * 
         * @param s1        First IDN string as StringBuffer
         * @param s2        Second IDN string as StringBuffer
         * @param options   A bit set of options:
         *  - IDNA.DEFAULT              Use default options, i.e., do not process unassigned code points
         *                              and do not use STD3 ASCII rules
         *                              If unassigned code points are found the operation fails with 
         *                              ParseException.
         *
         *  - IDNA.ALLOW_UNASSIGNED    Unassigned values can be converted to ASCII for query operations
         *                              If this option is set, the unassigned code points are in the input 
         *                              are treated as normal Unicode code points.
         *                          
         *  - IDNA.USE_STD3_RULES      Use STD3 ASCII rules for host name syntax restrictions
         *                              If this option is set and the input does not satisfy STD3 rules,  
         *                              the operation will fail with ParseException
         * @return 0 if the strings are equal, &gt; 0 if s1 &gt; s2 and &lt; 0 if s1 &lt; s2
         * @deprecated ICU 55 Use UTS 46 instead via {@link #getUTS46Instance(int)}.
         */
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(int).")]
        public static int Compare(StringBuffer s1, StringBuffer s2, int options)
        {
            if (s1 == null || s2 == null)
            {
                throw new ArgumentException("One of the source buffers is null");
            }
            return IDNA2003.Compare(s1.ToString(), s2.ToString(), options);
        }

        /**
         * IDNA2003: Compare two IDN strings for equivalence.
         * This function splits the domain names into labels and compares them.
         * According to IDN RFC, whenever two labels are compared, they are 
         * considered equal if and only if their ASCII forms (obtained by 
         * applying toASCII) match using an case-insensitive ASCII comparison.
         * Two domain names are considered a match if and only if all labels 
         * match regardless of whether label separators match.
         * 
         * @param s1        First IDN string 
         * @param s2        Second IDN string
         * @param options   A bit set of options:
         *  - IDNA.DEFAULT              Use default options, i.e., do not process unassigned code points
         *                              and do not use STD3 ASCII rules
         *                              If unassigned code points are found the operation fails with 
         *                              ParseException.
         *
         *  - IDNA.ALLOW_UNASSIGNED    Unassigned values can be converted to ASCII for query operations
         *                              If this option is set, the unassigned code points are in the input 
         *                              are treated as normal Unicode code points.
         *                          
         *  - IDNA.USE_STD3_RULES      Use STD3 ASCII rules for host name syntax restrictions
         *                              If this option is set and the input does not satisfy STD3 rules,  
         *                              the operation will fail with ParseException
         * @return 0 if the strings are equal, &gt; 0 if s1 &gt; s2 and &lt; 0 if s1 &lt; s2
         * @deprecated ICU 55 Use UTS 46 instead via {@link #getUTS46Instance(int)}.
         */
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(int).")]
        public static int Compare(string s1, string s2, int options)
        {
            if (s1 == null || s2 == null)
            {
                throw new ArgumentException("One of the source buffers is null");
            }
            return IDNA2003.Compare(s1, s2, options);
        }
        /**
         * IDNA2003: Compare two IDN strings for equivalence.
         * This function splits the domain names into labels and compares them.
         * According to IDN RFC, whenever two labels are compared, they are 
         * considered equal if and only if their ASCII forms (obtained by 
         * applying toASCII) match using an case-insensitive ASCII comparison.
         * Two domain names are considered a match if and only if all labels 
         * match regardless of whether label separators match.
         * 
         * @param s1        First IDN string as UCharacterIterator
         * @param s2        Second IDN string as UCharacterIterator
         * @param options   A bit set of options:
         *  - IDNA.DEFAULT              Use default options, i.e., do not process unassigned code points
         *                              and do not use STD3 ASCII rules
         *                              If unassigned code points are found the operation fails with 
         *                              ParseException.
         *
         *  - IDNA.ALLOW_UNASSIGNED     Unassigned values can be converted to ASCII for query operations
         *                              If this option is set, the unassigned code points are in the input 
         *                              are treated as normal Unicode code points.
         *                          
         *  - IDNA.USE_STD3_RULES       Use STD3 ASCII rules for host name syntax restrictions
         *                              If this option is set and the input does not satisfy STD3 rules,  
         *                              the operation will fail with ParseException
         * @return 0 if the strings are equal, &gt; 0 if i1 &gt; i2 and &lt; 0 if i1 &lt; i2
         * @deprecated ICU 55 Use UTS 46 instead via {@link #getUTS46Instance(int)}.
         */
        [Obsolete("ICU 55 Use UTS 46 instead via GetUTS46Instance(int).")]
        public static int Compare(UCharacterIterator s1, UCharacterIterator s2, int options)
        {
            if (s1 == null || s2 == null)
            {
                throw new ArgumentException("One of the source buffers is null");
            }
            return IDNA2003.Compare(s1.GetText(), s2.GetText(), options);
        }
    }
}
