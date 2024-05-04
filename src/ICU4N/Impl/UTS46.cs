using ICU4N.Globalization;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using J2N;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Impl
{
    /// <summary>
    /// Options for <see cref="UTS46"/>/<see cref="IDNA"/>.
    /// </summary>
    [Flags]
    public enum UTS46Options
    {
        /// <summary>
        /// Default options value: None of the other options are set.
        /// For use in static worker and factory methods.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        Default = 0,
        /// <summary>
        /// Option to check whether the input conforms to the STD3 ASCII rules,
        /// for example the restriction of labels to LDH characters
        /// (ASCII Letters, Digits and Hyphen-Minus).
        /// For use in static worker and factory methods.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        UseSTD3Rules = 2,
        /// <summary>
        /// IDNA option to check for whether the input conforms to the BiDi rules.
        /// For use in static worker and factory methods.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        CheckBiDi = 4,
        /// <summary>
        /// IDNA option to check for whether the input conforms to the CONTEXTJ rules.
        /// For use in static worker and factory methods.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        CheckContextJ = 8,
        /// <summary>
        /// IDNA option for nontransitional processing in ToASCII().
        /// For use in static worker and factory methods.
        /// <para/>
        /// By default, ToASCII() uses transitional processing.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        NontransitionalToASCII = 0x10,
        /// <summary>
        /// IDNA option for nontransitional processing in ToUnicode().
        /// For use in static worker and factory methods.
        /// <para/>
        /// By default, ToUnicode() uses transitional processing.
        /// </summary>
        /// <stable>ICU 4.6</stable>
        NontransitionalToUnicode = 0x20,
        /// <summary>
        /// IDNA option to check for whether the input conforms to the CONTEXTO rules.
        /// For use in static worker and factory methods.
        /// <para/>
        /// This is for use by registries for IDNA2008 conformance.
        /// UTS #46 does not require the CONTEXTO check.
        /// </summary>
        /// <stable>ICU 49</stable>
        CheckContextO = 0x40,
    }

    // Note about tests for IDNA.Error.DOMAIN_NAME_TOO_LONG:
    //
    // The domain name length limit is 255 octets in an internal DNS representation
    // where the last ("root") label is the empty label
    // represented by length byte 0 alone.
    // In a conventional string, this translates to 253 characters, or 254
    // if there is a trailing dot for the root label.

    /// <summary>
    /// UTS #46 (IDNA2008) implementation.
    /// </summary>
    /// <author>Markus Scherer</author>
    /// <since>2010jul09</since>
    public sealed partial class UTS46 : IDNA
    {
        public UTS46(UTS46Options options)
#pragma warning disable 612, 618
            : base()
#pragma warning restore 612, 618
        {
            this.options = options;
        }

        // ICU4N specific - LabelToASCII(ICharSequence label, StringBuilder dest, Info info) moved to UTS46.generated.tt

        public override StringBuilder LabelToASCII(ReadOnlySpan<char> label, StringBuilder dest, IDNAInfo info)
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                Process(label, true, true, ref sb, info);
                dest.Length = 0;
                return dest.Append(sb.AsSpan());
            }
            finally
            {
                sb.Dispose();
            }
        }

        internal override void LabelToASCII(ReadOnlySpan<char> label, ref ValueStringBuilder dest, IDNAInfo info)
            => Process(label, true, true, ref dest, info);

        // ICU4N specific - LabelToUnicode(ICharSequence label, StringBuilder dest, Info info) moved to UTS46.generated.tt

        public override StringBuilder LabelToUnicode(ReadOnlySpan<char> label, StringBuilder dest, IDNAInfo info)
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                Process(label, true, false, ref sb, info);
                dest.Length = 0;
                return dest.Append(sb.AsSpan());
            }
            finally
            {
                sb.Dispose();
            }
        }
        internal override void LabelToUnicode(ReadOnlySpan<char> label, ref ValueStringBuilder dest, IDNAInfo info)
            =>  Process(label, true, false, ref dest, info);

        // ICU4N specific - NameToASCII(ICharSequence name, StringBuilder dest, Info info) moved to UTS46.generated.tt


        public override StringBuilder NameToASCII(ReadOnlySpan<char> name, StringBuilder dest, IDNAInfo info)
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                Process(name, false, true, ref sb, info);
                dest.Length = 0;
                dest.Append(sb.AsSpan());
                if (dest.Length >= 254 && !info.Errors.Contains(IDNAError.DomainNameTooLong) &&
                    IsASCIIString(sb.AsSpan()) &&
                    (dest.Length > 254 || dest[253] != '.')
                )
                {
#pragma warning disable 612, 618
                    AddError(info, IDNAError.DomainNameTooLong);
#pragma warning restore 612, 618
                }
                return dest;
            }
            finally
            {
                sb.Dispose();
            }
        }
        internal override void NameToASCII(ReadOnlySpan<char> name, ref ValueStringBuilder dest, IDNAInfo info)
        {
            Process(name, false, true, ref dest, info);
            if (dest.Length >= 254 && !info.Errors.Contains(IDNAError.DomainNameTooLong) &&
                IsASCIIString(dest.AsSpan()) &&
                (dest.Length > 254 || dest[253] != '.')
            )
            {
#pragma warning disable 612, 618
                AddError(info, IDNAError.DomainNameTooLong);
#pragma warning restore 612, 618
            }
        }

        // ICU4N specific - NameToUnicode(ICharSequence name, StringBuilder dest, Info info) moved to UTS46.generated.tt

        public override StringBuilder NameToUnicode(ReadOnlySpan<char> name, StringBuilder dest, IDNAInfo info)
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                Process(name, false, false, ref sb, info);
                dest.Length = 0;
                return dest.Append(sb.AsSpan());
            }
            finally
            {
                sb.Dispose();
            }
        }
        internal override void NameToUnicode(ReadOnlySpan<char> name, ref ValueStringBuilder dest, IDNAInfo info)
            => Process(name, false, false, ref dest, info);


        private static readonly Normalizer2 uts46Norm2 =
            Normalizer2.GetInstance(null, "uts46", Normalizer2Mode.Compose);  // uts46.nrm
        internal readonly UTS46Options options;

        // Severe errors which usually result in a U+FFFD replacement character in the result string.
        private static readonly ISet<IDNAError> severeErrors = new HashSet<IDNAError>
        {
            IDNAError.LeadingCombiningMark,
            IDNAError.Disallowed,
            IDNAError.Punycode,
            IDNAError.LabelHasDot,
            IDNAError.InvalidAceLabel
        };

        // ICU4N specific - IsASCIIString(ICharSequence dest) moved to UTS46.generated.tt

        private static bool IsASCIIString(ReadOnlySpan<char> dest)
        {
            int length = dest.Length;
            for (int i = 0; i < length; ++i)
            {
                if (dest[i] > 0x7f)
                {
                    return false;
                }
            }
            return true;
        }

        // UTS #46 data for ASCII characters.
        // The normalizer (using uts46.nrm) maps uppercase ASCII letters to lowercase
        // and passes through all other ASCII characters.
        // If USE_STD3_RULES is set, then non-LDH characters are disallowed
        // using this data.
        // The ASCII fastpath also uses this data.
        // Values: -1=disallowed  0==valid  1==mapped (lowercase)
        private static readonly sbyte[] asciiData ={
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            // 002D..002E; valid  #  HYPHEN-MINUS..FULL STOP
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  0,  0, -1,
            // 0030..0039; valid  #  DIGIT ZERO..DIGIT NINE
             0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1,
            // 0041..005A; mapped  #  LATIN CAPITAL LETTER A..LATIN CAPITAL LETTER Z
            -1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1,
             1,  1,  1,  1,  1,  1,  1,  1,  1,  1,  1, -1, -1, -1, -1, -1,
            // 0061..007A; valid  #  LATIN SMALL LETTER A..LATIN SMALL LETTER Z
            -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
             0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1
        };

        // ICU4N specific - Process(ICharSequence src,
        //    bool isLabel, bool toASCII,
        //    StringBuilder dest,
        //    Info info) moved to UTS46.generated.tt

        private void Process(ReadOnlySpan<char> src,
            bool isLabel, bool toASCII,
            ref ValueStringBuilder dest,
            IDNAInfo info)
        {
            // uts46Norm2.normalize() would do all of this error checking and setup,
            // but with the ASCII fastpath we do not always call it, and do not
            // call it first.
            if (MemoryHelper.AreSame(dest.RawChars, src))
            {
                throw new ArgumentException($"'{nameof(src)}' cannot be the same reference as '{nameof(dest)}'");
            }
            // Arguments are fine, reset output values.
            dest.Delete(0, 0x7fffffff - 0); // ICU4N: Corrected 2nd parameter
#pragma warning disable 612, 618
            ResetInfo(info);
#pragma warning restore 612, 618
            int srcLength = src.Length;
            if (srcLength == 0)
            {
#pragma warning disable 612, 618
                AddError(info, IDNAError.EmptyLabel);
#pragma warning restore 612, 618
                return;
            }
            // ASCII fastpath
            bool disallowNonLDHDot = (options & UTS46Options.UseSTD3Rules) != 0;
            int labelStart = 0;
            int i;
            for (i = 0; ; ++i)
            {
                if (i == srcLength)
                {
                    if (toASCII)
                    {
                        if ((i - labelStart) > 63)
                        {
#pragma warning disable 612, 618
                            AddLabelError(info, IDNAError.LabelTooLong);
#pragma warning restore 612, 618
                        }
                        // There is a trailing dot if labelStart==i.
                        if (!isLabel && i >= 254 && (i > 254 || labelStart < i))
                        {
#pragma warning disable 612, 618
                            AddError(info, IDNAError.DomainNameTooLong);
                        }
                    }
                    PromoteAndResetLabelErrors(info);
#pragma warning restore 612, 618
                    return;
                }
                char c = src[i];
                if (c > 0x7f)
                {
                    break;
                }
                int cData = asciiData[c];
                if (cData > 0)
                {
                    dest.Append((char)(c + 0x20));  // Lowercase an uppercase ASCII letter.
                }
                else if (cData < 0 && disallowNonLDHDot)
                {
                    break;  // Replacing with U+FFFD can be complicated for toASCII.
                }
                else
                {
                    dest.Append(c);
                    if (c == '-')
                    {  // hyphen
                        if (i == (labelStart + 3) && src[i - 1] == '-')
                        {
                            // "??--..." is Punycode or forbidden.
                            ++i;  // '-' was copied to dest already
                            break;
                        }
#pragma warning disable 612, 618
                        if (i == labelStart)
                        {
                            // label starts with "-"
                            AddLabelError(info, IDNAError.LeadingHyphen);
                        }
                        if ((i + 1) == srcLength || src[i + 1] == '.')
                        {
                            // label ends with "-"
                            AddLabelError(info, IDNAError.TrailingHyphen);
                        }
#pragma warning restore 612, 618
                    }
                    else if (c == '.')
                    {  // dot
                        if (isLabel)
                        {
                            // Replacing with U+FFFD can be complicated for toASCII.
                            ++i;  // '.' was copied to dest already
                            break;
                        }
                        if (i == labelStart)
                        {
#pragma warning disable 612, 618
                            AddLabelError(info, IDNAError.EmptyLabel);
#pragma warning restore 612, 618
                        }
                        if (toASCII && (i - labelStart) > 63)
                        {
#pragma warning disable 612, 618
                            AddLabelError(info, IDNAError.LabelTooLong);
                        }
                        PromoteAndResetLabelErrors(info);
#pragma warning restore 612, 618
                        labelStart = i + 1;
                    }
                }
            }
#pragma warning disable 612, 618
            PromoteAndResetLabelErrors(info);
            ProcessUnicode(src, labelStart, i, isLabel, toASCII, ref dest, info);
            if (IsBiDi(info) && !HasCertainErrors(info, severeErrors) &&
                (!IsOkBiDi(info) || (labelStart > 0 && !IsASCIIOkBiDi(dest.AsSpan(), labelStart)))
            )
            {
                AddError(info, IDNAError.BiDi);
#pragma warning restore 612, 618
            }
        }

        // ICU4N specific - ProcessUnicode(ICharSequence src,
        //    int labelStart, int mappingStart,
        //    bool isLabel, bool toASCII,
        //    StringBuilder dest,
        //    Info info) moved to UTS46.generated.tt

        private void ProcessUnicode(ReadOnlySpan<char> src,
            int labelStart, int mappingStart,
            bool isLabel, bool toASCII,
            ref ValueStringBuilder dest,
            IDNAInfo info)
        {
            if (mappingStart == 0)
            {
                uts46Norm2.Normalize(src, ref dest);
            }
            else
            {
                uts46Norm2.NormalizeSecondAndAppend(ref dest, src.Slice(mappingStart, src.Length - mappingStart)); // ICU4N: Corrected 2nd parameter
            }
            bool doMapDevChars =
                toASCII ? (options & UTS46Options.NontransitionalToASCII) == 0 :
                          (options & UTS46Options.NontransitionalToUnicode) == 0;
            int destLength = dest.Length;
            int labelLimit = labelStart;
            while (labelLimit < destLength)
            {
                char c = dest[labelLimit];
                if (c == '.' && !isLabel)
                {
                    int labelLength = labelLimit - labelStart;
                    int newLength = ProcessLabel(ref dest, labelStart, labelLength,
                                                    toASCII, info);
#pragma warning disable 612, 618
                    PromoteAndResetLabelErrors(info);
#pragma warning restore 612, 618
                    destLength += newLength - labelLength;
                    labelLimit = labelStart += newLength + 1;
                }
                else if (0xdf <= c && c <= 0x200d && (c == 0xdf || c == 0x3c2 || c >= 0x200c))
                {
#pragma warning disable 612, 618
                    SetTransitionalDifferent(info);
#pragma warning restore 612, 618
                    if (doMapDevChars)
                    {
                        destLength = MapDevChars(ref dest, labelStart, labelLimit);
                        // Do not increment labelLimit in case c was removed.
                        // All deviation characters have been mapped, no need to check for them again.
                        doMapDevChars = false;
                    }
                    else
                    {
                        ++labelLimit;
                    }
                }
                else
                {
                    ++labelLimit;
                }
            }
            // Permit an empty label at the end (0<labelStart==labelLimit==destLength is ok)
            // but not an empty label elsewhere nor a completely empty domain name.
            // processLabel() sets UIDNA_ERROR_EMPTY_LABEL when labelLength==0.
            if (0 == labelStart || labelStart < labelLimit)
            {
                ProcessLabel(ref dest, labelStart, labelLimit - labelStart, toASCII, info);
#pragma warning disable 612, 618
                PromoteAndResetLabelErrors(info);
#pragma warning restore 612, 618
            }
        }


        // returns the new dest.Length
        private int MapDevChars(ref ValueStringBuilder dest, int labelStart, int mappingStart)
        {
            int length = dest.Length;
            bool didMapDevChars = false;
            for (int i = mappingStart; i < length;)
            {
                char c = dest[i];
                switch ((int)c)
                {
                    case 0xdf:
                        // Map sharp s to ss.
                        didMapDevChars = true;
                        dest[i++] = 's';
                        dest.Insert(i++, 's');
                        ++length;
                        break;
                    case 0x3c2:  // Map final sigma to nonfinal sigma.
                        didMapDevChars = true;
                        dest[i++] = '\u03c3';
                        break;
                    case 0x200c:  // Ignore/remove ZWNJ.
                    case 0x200d:  // Ignore/remove ZWJ.
                        didMapDevChars = true;
                        dest.Delete(i, 1); // ICU4N: Corrected 2nd parameter
                        --length;
                        break;
                    default:
                        ++i;
                        break;
                }
            }
            if (didMapDevChars)
            {
                int estimatedLength = (dest.Length - labelStart) + 16;
                var normalized = estimatedLength <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[estimatedLength])
                    : new ValueStringBuilder(estimatedLength);
                try
                {
                    // Mapping deviation characters might have resulted in an un-NFC string.
                    // We could use either the NFC or the UTS #46 normalizer.
                    // By using the UTS #46 normalizer again, we avoid having to load a second .nrm data file.
                    uts46Norm2.Normalize(dest.AsSpan(labelStart, dest.Length - labelStart), ref normalized); // ICU4N: Corrected 2nd parameter
                    unsafe
                    {
                        dest.Replace(labelStart, 0x7fffffff - labelStart, new ReadOnlySpan<char>(normalized.GetCharsPointer(), normalized.Length)); // ICU4N: Corrected 2nd parameter
                    }
                    return dest.Length;
                }
                finally
                {
                    normalized.Dispose();
                }
            }
            return length;
        }
        // Some non-ASCII characters are equivalent to sequences with
        // non-LDH ASCII characters. To find them:
        // grep disallowed_STD3_valid IdnaMappingTable.txt (or uts46.txt)
        private static bool IsNonASCIIDisallowedSTD3Valid(int c)
        {
            return c == 0x2260 || c == 0x226E || c == 0x226F;
        }

        // ICU4N specific - ReplaceLabel(StringBuilder dest, int destLabelStart, int destLabelLength,
        //    ICharSequence label, int labelLength) moved to UTS46.generated.tt

        // Replace the label in dest with the label string, if the label was modified.
        // If label==dest then the label was modified in-place and labelLength
        // is the new label length, different from label.Length.
        // If label!=dest then labelLength==label.Length.
        // Returns labelLength (= the new label length).
        private static int ReplaceLabel(ref ValueStringBuilder dest, int destLabelStart, int destLabelLength,
            ReadOnlySpan<char> label, int labelLength)
        {
            if (!MemoryHelper.AreSame(label, dest.RawChars))
            {
                dest.Delete(destLabelStart, destLabelLength); // ICU4N: Corrected 2nd parameter of Delete
                dest.Insert(destLabelStart, label);
                // or dest.Replace(destLabelStart, destLabelLength, label.ToString());
                // which would create a String rather than moving characters in the StringBuilder.
            }
            return labelLength;
        }

        // returns the new label length
        private int ProcessLabel(ref ValueStringBuilder dest,
                     int labelStart, int labelLength,
                     bool toASCII,
                     IDNAInfo info)
        {
            ValueStringBuilder fromPunycode = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            Span<char> labelString;
            int destLabelStart = labelStart;
            int destLabelLength = labelLength;
            bool wasPunycode;
            if (labelLength >= 4 &&
                dest[labelStart] == 'x' && dest[labelStart + 1] == 'n' &&
                dest[labelStart + 2] == '-' && dest[labelStart + 3] == '-'
            )
            {
                // Label starts with "xn--", try to un-Punycode it.
                wasPunycode = true;
                try
                {
                    Punycode.Decode(dest.AsSpan(labelStart + 4, labelLength - 4), ref fromPunycode, null); // ICU4N: (labelStart + labelLength) - (labelStart + 4) == (labelLength - 4)
                }
                catch (StringPrepParseException)
                {
#pragma warning disable 612, 618
                    AddLabelError(info, IDNAError.Punycode);
#pragma warning restore 612, 618
                    return MarkBadACELabel(ref dest, labelStart, labelLength, toASCII, info);
                }
                // Check for NFC, and for characters that are not
                // valid or deviation characters according to the normalizer.
                // If there is something wrong, then the string will change.
                // Note that the normalizer passes through non-LDH ASCII and deviation characters.
                // Deviation characters are ok in Punycode even in transitional processing.
                // In the code further below, if we find non-LDH ASCII and we have UIDNA_USE_STD3_RULES
                // then we will set UIDNA_ERROR_INVALID_ACE_LABEL there too.
                bool isValid = uts46Norm2.IsNormalized(fromPunycode.AsSpan());
                if (!isValid)
                {
#pragma warning disable 612, 618
                    AddLabelError(info, IDNAError.InvalidAceLabel);
#pragma warning restore 612, 618
                    return MarkBadACELabel(ref dest, labelStart, labelLength, toASCII, info);
                }
                unsafe
                {
                    labelString = new Span<char>(fromPunycode.GetCharsPointer(), fromPunycode.Length);
                }
                labelStart = 0;
                labelLength = fromPunycode.Length;
            }
            else
            {
                wasPunycode = false;
                unsafe
                {
                    labelString = new Span<char>(dest.GetCharsPointer(), dest.Length); //dest;
                }
            }
            // Validity check
            if (labelLength == 0)
            {
#pragma warning disable 612, 618
                AddLabelError(info, IDNAError.EmptyLabel);
                return ReplaceLabel(ref dest, destLabelStart, destLabelLength, labelString, labelLength);
            }
            // labelLength>0
            if (labelLength >= 4 && labelString[labelStart + 2] == '-' && labelString[labelStart + 3] == '-')
            {
                // label starts with "??--"
                AddLabelError(info, IDNAError.Hyphen_3_4);
            }
            if (labelString[labelStart] == '-')
            {
                // label starts with "-"
                AddLabelError(info, IDNAError.LeadingHyphen);
            }
            if (labelString[labelStart + labelLength - 1] == '-')
            {
                // label ends with "-"
                AddLabelError(info, IDNAError.TrailingHyphen);
            }
#pragma warning restore 612, 618
            // If the label was not a Punycode label, then it was the result of
            // mapping, normalization and label segmentation.
            // If the label was in Punycode, then we mapped it again above
            // and checked its validity.
            // Now we handle the STD3 restriction to LDH characters (if set)
            // and we look for U+FFFD which indicates disallowed characters
            // in a non-Punycode label or U+FFFD itself in a Punycode label.
            // We also check for dots which can come from the input to a single-label function.
            // Ok to cast away const because we own the UnicodeString.
            int i = labelStart;
            int limit = labelStart + labelLength;
            char oredChars = (char)0;
            // If we enforce STD3 rules, then ASCII characters other than LDH and dot are disallowed.
            bool disallowNonLDHDot = (options & UTS46Options.UseSTD3Rules) != 0;
            do
            {
                char c = labelString[i];
                if (c <= 0x7f)
                {
                    if (c == '.')
                    {
#pragma warning disable 612, 618
                        AddLabelError(info, IDNAError.LabelHasDot);
#pragma warning restore 612, 618
                        labelString[i] = '\ufffd';
                    }
                    else if (disallowNonLDHDot && asciiData[c] < 0)
                    {
#pragma warning disable 612, 618
                        AddLabelError(info, IDNAError.Disallowed);
#pragma warning restore 612, 618
                        labelString[i] = '\ufffd';
                    }
                }
                else
                {
                    oredChars |= c;
                    if (disallowNonLDHDot && IsNonASCIIDisallowedSTD3Valid(c))
                    {
#pragma warning disable 612, 618
                        AddLabelError(info, IDNAError.Disallowed);
#pragma warning restore 612, 618
                        labelString[i] = '\ufffd';
                    }
                    else if (c == 0xfffd)
                    {
#pragma warning disable 612, 618
                        AddLabelError(info, IDNAError.Disallowed);
#pragma warning restore 612, 618
                    }
                }
                ++i;
            } while (i < limit);
            // Check for a leading combining mark after other validity checks
            // so that we don't report IDNA.Error.DISALLOWED for the U+FFFD from here.
            int c2;
            // "Unsafe" is ok because unpaired surrogates were mapped to U+FFFD.
            c2 = ((ReadOnlySpan<char>)labelString).CodePointAt(labelStart);
            if ((U_GET_GC_MASK(c2) & U_GC_M_MASK) != 0)
            {
#pragma warning disable 612, 618
                AddLabelError(info, IDNAError.LeadingCombiningMark);
#pragma warning restore 612, 618
                labelString[labelStart] = '\ufffd';
                if (c2 > 0xffff)
                {
                    if (MemoryHelper.AreSame<char>(labelString, dest.RawChars))
                    {
                        // Remove c's trail surrogate.
                        dest.Remove(labelStart + 1, 1);
                        --labelLength;
                        --destLabelLength;
                    }
                    else
                    {
                        // Remove c's trail surrogate.
                        fromPunycode.Remove(labelStart + 1, 1);
                        --labelLength;
                    }
                }
            }
#pragma warning disable 612, 618
            if (!HasCertainLabelErrors(info, severeErrors))
#pragma warning restore 612, 618
            {
                // Do contextual checks only if we do not have U+FFFD from a severe error
                // because U+FFFD can make these checks fail.
                if ((options & UTS46Options.CheckBiDi) != 0 &&
#pragma warning disable 612, 618
                    (!IsBiDi(info) || IsOkBiDi(info)))
#pragma warning restore 612, 618
                {
                    CheckLabelBiDi(labelString.Slice(labelStart, labelLength), info);
                }
                if ((options & UTS46Options.CheckContextJ) != 0 && (oredChars & 0x200c) == 0x200c &&
                    !IsLabelOkContextJ(labelString.Slice(labelStart, labelLength))
                )
                {
#pragma warning disable 612, 618
                    AddLabelError(info, IDNAError.ContextJ);
#pragma warning restore 612, 618
                }
                if ((options & UTS46Options.CheckContextO) != 0 && oredChars >= 0xb7)
                {
                    CheckLabelContextO(labelString.Slice(labelStart, labelLength), info);
                }
                if (toASCII)
                {
                    if (wasPunycode)
                    {
                        // Leave a Punycode label unchanged if it has no severe errors.
                        if (destLabelLength > 63)
                        {
#pragma warning disable 612, 618
                            AddLabelError(info, IDNAError.LabelTooLong);
#pragma warning restore 612, 618
                        }
                        return destLabelLength;
                    }
                    else if (oredChars >= 0x80)
                    {
                        // Contains non-ASCII characters.
                        ValueStringBuilder punycode = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
                        try
                        {
                            Punycode.Encode(labelString.Slice(labelStart, labelLength), ref punycode, null); // ICU4N: (labelStart + labelLength) - labelStart == labelLength
                        }
                        catch (StringPrepParseException e)
                        {
                            throw new ICUException(e);  // unexpected
                        }
                        punycode.Insert(0, "xn--");
                        if (punycode.Length > 63)
                        {
#pragma warning disable 612, 618
                            AddLabelError(info, IDNAError.LabelTooLong);
#pragma warning restore 612, 618
                        }
                        unsafe
                        {
                            ReadOnlySpan<char> punycodeSpan = new ReadOnlySpan<char>(punycode.GetCharsPointer(), punycode.Length);
                            return ReplaceLabel(ref dest, destLabelStart, destLabelLength,
                                            punycodeSpan, punycode.Length);
                        }
                    }
                    else
                    {
                        // all-ASCII label
                        if (labelLength > 63)
                        {
#pragma warning disable 612, 618
                            AddLabelError(info, IDNAError.LabelTooLong);
#pragma warning restore 612, 618
                        }
                    }
                }
            }
            else
            {
                // If a Punycode label has severe errors,
                // then leave it but make sure it does not look valid.
                if (wasPunycode)
                {
#pragma warning disable 612, 618
                    AddLabelError(info, IDNAError.InvalidAceLabel);
#pragma warning restore 612, 618
                    return MarkBadACELabel(ref dest, destLabelStart, destLabelLength, toASCII, info);
                }
            }
            return ReplaceLabel(ref dest, destLabelStart, destLabelLength, labelString, labelLength);
        }

        private int MarkBadACELabel(ref ValueStringBuilder dest,
                        int labelStart, int labelLength,
                        bool toASCII, IDNAInfo info)
        {
            bool disallowNonLDHDot = (options & UTS46Options.UseSTD3Rules) != 0;
            bool isASCII = true;
            bool onlyLDH = true;
            int i = labelStart + 4;  // After the initial "xn--".
            int limit = labelStart + labelLength;
            do
            {
                char c = dest[i];
                if (c <= 0x7f)
                {
                    if (c == '.')
                    {
#pragma warning disable 612, 618
                        AddLabelError(info, IDNAError.LabelHasDot);
#pragma warning restore 612, 618
                        dest[i] = '\ufffd';
                        isASCII = onlyLDH = false;
                    }
                    else if (asciiData[c] < 0)
                    {
                        onlyLDH = false;
                        if (disallowNonLDHDot)
                        {
                            dest[i] = '\ufffd';
                            isASCII = false;
                        }
                    }
                }
                else
                {
                    isASCII = onlyLDH = false;
                }
            } while (++i < limit);
            if (onlyLDH)
            {
                dest.Insert(labelStart + labelLength, '\ufffd');
                ++labelLength;
            }
            else
            {
                if (toASCII && isASCII && labelLength > 63)
                {
#pragma warning disable 612, 618
                    AddLabelError(info, IDNAError.LabelTooLong);
#pragma warning restore 612, 618
                }
            }
            return labelLength;
        }

        private static readonly int L_MASK = U_MASK(UCharacterDirection.LeftToRight.ToInt32());
        private static readonly int R_AL_MASK =
            U_MASK(UCharacterDirection.RightToLeft.ToInt32()) |
            U_MASK(UCharacterDirection.RightToLeftArabic.ToInt32());
        private static readonly int L_R_AL_MASK = L_MASK | R_AL_MASK;

        private static readonly int R_AL_AN_MASK = R_AL_MASK | U_MASK(UCharacterDirection.ArabicNumber.ToInt32());

        private static readonly int EN_AN_MASK =
            U_MASK(UCharacterDirection.EuropeanNumber.ToInt32()) |
            U_MASK(UCharacterDirection.ArabicNumber.ToInt32());
        private static readonly int R_AL_EN_AN_MASK = R_AL_MASK | EN_AN_MASK;
        private static readonly int L_EN_MASK = L_MASK | U_MASK(UCharacterDirection.EuropeanNumber.ToInt32());

        private static readonly int ES_CS_ET_ON_BN_NSM_MASK =
            U_MASK(UCharacterDirection.EuropeanNumberSeparator.ToInt32()) |
            U_MASK(UCharacterDirection.CommonNumberSeparator.ToInt32()) |
            U_MASK(UCharacterDirection.EuropeanNumberTerminator.ToInt32()) |
            U_MASK(UCharacterDirection.OtherNeutral.ToInt32()) |
            U_MASK(UCharacterDirection.BoundaryNeutral.ToInt32()) |
            U_MASK(UCharacterDirection.DirNonSpacingMark.ToInt32());
        private static readonly int L_EN_ES_CS_ET_ON_BN_NSM_MASK = L_EN_MASK | ES_CS_ET_ON_BN_NSM_MASK;
        private static readonly int R_AL_AN_EN_ES_CS_ET_ON_BN_NSM_MASK = R_AL_MASK | EN_AN_MASK | ES_CS_ET_ON_BN_NSM_MASK;

        // ICU4N specific - CheckLabelBiDi(ICharSequence label, int labelStart, int labelLength, Info info) moved to UTS46.generated.tt

        // We scan the whole label and check both for whether it contains RTL characters
        // and whether it passes the BiDi Rule.
        // In a BiDi domain name, all labels must pass the BiDi Rule, but we might find
        // that a domain name is a BiDi domain name (has an RTL label) only after
        // processing several earlier labels.
        private void CheckLabelBiDi(ReadOnlySpan<char> label, /*int labelStart, int labelLength,*/ IDNAInfo info)
        {
            int labelStart = 0, labelLength = label.Length;

            // IDNA2008 BiDi rule
            // Get the directionality of the first character.
            int c;
            int i = labelStart;
            c = Character.CodePointAt(label, i);
            i += Character.CharCount(c);
            int firstMask = U_MASK(UBiDiProps.Instance.GetClass(c).ToInt32());
            // 1. The first character must be a character with BIDI property L, R
            // or AL.  If it has the R or AL property, it is an RTL label; if it
            // has the L property, it is an LTR label.
            if ((firstMask & ~L_R_AL_MASK) != 0)
            {
#pragma warning disable 612, 618
                SetNotOkBiDi(info);
#pragma warning restore 612, 618
            }
            // Get the directionality of the last non-NSM character.
            int lastMask;
            int labelLimit = labelStart + labelLength;
            for (; ; )
            {
                if (i >= labelLimit)
                {
                    lastMask = firstMask;
                    break;
                }
                c = Character.CodePointBefore(label, labelLimit);
                labelLimit -= Character.CharCount(c);
                UCharacterDirection dir = UBiDiProps.Instance.GetClass(c);
                if (dir != UCharacterDirection.DirNonSpacingMark)
                {
                    lastMask = U_MASK(dir.ToInt32());
                    break;
                }
            }
            // 3. In an RTL label, the end of the label must be a character with
            // BIDI property R, AL, EN or AN, followed by zero or more
            // characters with BIDI property NSM.
            // 6. In an LTR label, the end of the label must be a character with
            // BIDI property L or EN, followed by zero or more characters with
            // BIDI property NSM.
            if ((firstMask & L_MASK) != 0 ?
                    (lastMask & ~L_EN_MASK) != 0 :
                    (lastMask & ~R_AL_EN_AN_MASK) != 0
            )
            {
#pragma warning disable 612, 618
                SetNotOkBiDi(info);
#pragma warning restore 612, 618
            }
            // Add the directionalities of the intervening characters.
            int mask = firstMask | lastMask;
            while (i < labelLimit)
            {
                c = Character.CodePointAt(label, i);
                i += Character.CharCount(c);
                mask |= U_MASK(UBiDiProps.Instance.GetClass(c).ToInt32());
            }
            if ((firstMask & L_MASK) != 0)
            {
                // 5. In an LTR label, only characters with the BIDI properties L, EN,
                // ES, CS, ET, ON, BN and NSM are allowed.
                if ((mask & ~L_EN_ES_CS_ET_ON_BN_NSM_MASK) != 0)
                {
#pragma warning disable 612, 618
                    SetNotOkBiDi(info);
#pragma warning restore 612, 618
                }
            }
            else
            {
                // 2. In an RTL label, only characters with the BIDI properties R, AL,
                // AN, EN, ES, CS, ET, ON, BN and NSM are allowed.
                if ((mask & ~R_AL_AN_EN_ES_CS_ET_ON_BN_NSM_MASK) != 0)
                {
#pragma warning disable 612, 618
                    SetNotOkBiDi(info);
#pragma warning restore 612, 618
                }
                // 4. In an RTL label, if an EN is present, no AN may be present, and
                // vice versa.
                if ((mask & EN_AN_MASK) == EN_AN_MASK)
                {
#pragma warning disable 612, 618
                    SetNotOkBiDi(info);
#pragma warning restore 612, 618
                }
            }
            // An RTL label is a label that contains at least one character of type
            // R, AL or AN. [...]
            // A "BIDI domain name" is a domain name that contains at least one RTL
            // label. [...]
            // The following rule, consisting of six conditions, applies to labels
            // in BIDI domain names.
            if ((mask & R_AL_AN_MASK) != 0)
            {
#pragma warning disable 612, 618
                SetBiDi(info);
#pragma warning restore 612, 618
            }
        }

        // ICU4N specific - IsASCIIOkBiDi(ICharSequence s, int length) moved to UTS46.generated.tt

        // Special code for the ASCII prefix of a BiDi domain name.
        // The ASCII prefix is all-LTR.

        // IDNA2008 BiDi rule, parts relevant to ASCII labels:
        // 1. The first character must be a character with BIDI property L [...]
        // 5. In an LTR label, only characters with the BIDI properties L, EN,
        // ES, CS, ET, ON, BN and NSM are allowed.
        // 6. In an LTR label, the end of the label must be a character with
        // BIDI property L or EN [...]

        // UTF-16 version, called for mapped ASCII prefix.
        // Cannot contain uppercase A-Z.
        // s[length-1] must be the trailing dot.
        private static bool IsASCIIOkBiDi(ReadOnlySpan<char> s, int length)
        {
            int labelStart = 0;
            for (int i = 0; i < length; ++i)
            {
                char c = s[i];
                if (c == '.')
                {  // dot
                    if (i > labelStart)
                    {
                        c = s[i - 1];
                        if (!('a' <= c && c <= 'z') && !('0' <= c && c <= '9'))
                        {
                            // Last character in the label is not an L or EN.
                            return false;
                        }
                    }
                    labelStart = i + 1;
                }
                else if (i == labelStart)
                {
                    if (!('a' <= c && c <= 'z'))
                    {
                        // First character in the label is not an L.
                        return false;
                    }
                }
                else
                {
                    if (c <= 0x20 && (c >= 0x1c || (9 <= c && c <= 0xd)))
                    {
                        // Intermediate character in the label is a B, S or WS.
                        return false;
                    }
                }
            }
            return true;
        }

        // ICU4N specific - IsLabelOkContextJ(ICharSequence label, int labelStart, int labelLength) moved to UTS46.generated.tt

        private bool IsLabelOkContextJ(ReadOnlySpan<char> label)
        {
            // [IDNA2008-Tables]
            // 200C..200D  ; CONTEXTJ    # ZERO WIDTH NON-JOINER..ZERO WIDTH JOINER
            int labelStart = 0, labelLength = label.Length;
            int labelLimit = labelStart + labelLength;
            for (int i = labelStart; i < labelLimit; ++i)
            {
                if (label[i] == 0x200c)
                {
                    // Appendix A.1. ZERO WIDTH NON-JOINER
                    // Rule Set:
                    //  False;
                    //  If Canonical_Combining_Class(Before(cp)) .eq.  Virama Then True;
                    //  If RegExpMatch((Joining_Type:{L,D})(Joining_Type:T)*\u200C
                    //     (Joining_Type:T)*(Joining_Type:{R,D})) Then True;
                    if (i == labelStart)
                    {
                        return false;
                    }
                    int c;
                    int j = i;
                    c = Character.CodePointBefore(label, j);
                    j -= Character.CharCount(c);
                    if (uts46Norm2.GetCombiningClass(c) == 9)
                    {
                        continue;
                    }
                    // check precontext (Joining_Type:{L,D})(Joining_Type:T)*
                    for (; ; )
                    {
                        /* UJoiningType */
                        int type = UBiDiProps.Instance.GetJoiningType(c);
                        if (type == JoiningType.Transparent)
                        {
                            if (j == 0)
                            {
                                return false;
                            }
                            c = Character.CodePointBefore(label, j);
                            j -= Character.CharCount(c);
                        }
                        else if (type == JoiningType.LeftJoining || type == JoiningType.DualJoining)
                        {
                            break;  // precontext fulfilled
                        }
                        else
                        {
                            return false;
                        }
                    }
                    // check postcontext (Joining_Type:T)*(Joining_Type:{R,D})
                    for (j = i + 1; ;)
                    {
                        if (j == labelLimit)
                        {
                            return false;
                        }
                        c = Character.CodePointAt(label, j);
                        j += Character.CharCount(c);
                        /* UJoiningType */
                        int type = UBiDiProps.Instance.GetJoiningType(c);
                        if (type == JoiningType.Transparent)
                        {
                            // just skip this character
                        }
                        else if (type == JoiningType.RightJoining || type == JoiningType.DualJoining)
                        {
                            break;  // postcontext fulfilled
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else if (label[i] == 0x200d)
                {
                    // Appendix A.2. ZERO WIDTH JOINER (U+200D)
                    // Rule Set:
                    //  False;
                    //  If Canonical_Combining_Class(Before(cp)) .eq.  Virama Then True;
                    if (i == labelStart)
                    {
                        return false;
                    }
                    int c = Character.CodePointBefore(label, i);
                    if (uts46Norm2.GetCombiningClass(c) != 9)
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        // ICU4N specific - CheckLabelContextO(ICharSequence label, int labelStart, int labelLength, Info info) moved to UTS46.generated.tt

        private void CheckLabelContextO(ReadOnlySpan<char> label, IDNAInfo info)
        {
            int labelStart = 0, labelLength = label.Length;
            int labelEnd = labelStart + labelLength - 1;  // inclusive
            int arabicDigits = 0;  // -1 for 066x, +1 for 06Fx
            for (int i = labelStart; i <= labelEnd; ++i)
            {
                int c = label[i];
                if (c < 0xb7)
                {
                    // ASCII fastpath
                }
                else if (c <= 0x6f9)
                {
                    if (c == 0xb7)
                    {
                        // Appendix A.3. MIDDLE DOT (U+00B7)
                        // Rule Set:
                        //  False;
                        //  If Before(cp) .eq.  U+006C And
                        //     After(cp) .eq.  U+006C Then True;
                        if (!(labelStart < i && label[i - 1] == 'l' &&
                             i < labelEnd && label[i + 1] == 'l'))
                        {
#pragma warning disable 612, 618
                            AddLabelError(info, IDNAError.ContextOPunctuation);
#pragma warning restore 612, 618
                        }
                    }
                    else if (c == 0x375)
                    {
                        // Appendix A.4. GREEK LOWER NUMERAL SIGN (KERAIA) (U+0375)
                        // Rule Set:
                        //  False;
                        //  If Script(After(cp)) .eq.  Greek Then True;
                        if (!(i < labelEnd &&
                             UScript.Greek == UScript.GetScript(Character.CodePointAt(label, i + 1))))
                        {
#pragma warning disable 612, 618
                            AddLabelError(info, IDNAError.ContextOPunctuation);
#pragma warning restore 612, 618
                        }
                    }
                    else if (c == 0x5f3 || c == 0x5f4)
                    {
                        // Appendix A.5. HEBREW PUNCTUATION GERESH (U+05F3)
                        // Rule Set:
                        //  False;
                        //  If Script(Before(cp)) .eq.  Hebrew Then True;
                        //
                        // Appendix A.6. HEBREW PUNCTUATION GERSHAYIM (U+05F4)
                        // Rule Set:
                        //  False;
                        //  If Script(Before(cp)) .eq.  Hebrew Then True;
                        if (!(labelStart < i &&
                             UScript.Hebrew == UScript.GetScript(Character.CodePointBefore(label, i))))
                        {
#pragma warning disable 612, 618
                            AddLabelError(info, IDNAError.ContextOPunctuation);
#pragma warning restore 612, 618
                        }
                    }
                    else if (0x660 <= c /* && c<=0x6f9 */)
                    {
                        // Appendix A.8. ARABIC-INDIC DIGITS (0660..0669)
                        // Rule Set:
                        //  True;
                        //  For All Characters:
                        //    If cp .in. 06F0..06F9 Then False;
                        //  End For;
                        //
                        // Appendix A.9. EXTENDED ARABIC-INDIC DIGITS (06F0..06F9)
                        // Rule Set:
                        //  True;
                        //  For All Characters:
                        //    If cp .in. 0660..0669 Then False;
                        //  End For;
                        if (c <= 0x669)
                        {
                            if (arabicDigits > 0)
                            {
#pragma warning disable 612, 618
                                AddLabelError(info, IDNAError.ContextODigits);
#pragma warning restore 612, 618
                            }
                            arabicDigits = -1;
                        }
                        else if (0x6f0 <= c)
                        {
                            if (arabicDigits < 0)
                            {
#pragma warning disable 612, 618
                                AddLabelError(info, IDNAError.ContextODigits);
#pragma warning restore 612, 618
                            }
                            arabicDigits = 1;
                        }
                    }
                }
                else if (c == 0x30fb)
                {
                    // Appendix A.7. KATAKANA MIDDLE DOT (U+30FB)
                    // Rule Set:
                    //  False;
                    //  For All Characters:
                    //    If Script(cp) .in. {Hiragana, Katakana, Han} Then True;
                    //  End For;
                    for (int j = labelStart; ; j += Character.CharCount(c))
                    {
                        if (j > labelEnd)
                        {
#pragma warning disable 612, 618
                            AddLabelError(info, IDNAError.ContextOPunctuation);
#pragma warning restore 612, 618
                            break;
                        }
                        c = Character.CodePointAt(label, j);
                        int script = UScript.GetScript(c);
                        if (script == UScript.Hiragana || script == UScript.Katakana || script == UScript.Han)
                        {
                            break;
                        }
                    }
                }
            }
        }

        // TODO: make public(?) -- in C, these are public in uchar.h
        private static int U_MASK(int x)
        {
            return 1 << x;
        }
        private static int U_GET_GC_MASK(int c)
        {
            return (1 << UChar.GetUnicodeCategory(c).ToInt32());
        }
        private static int U_GC_M_MASK =
            U_MASK(UUnicodeCategory.NonSpacingMark.ToInt32()) |
            U_MASK(UUnicodeCategory.EnclosingMark.ToInt32()) |
            U_MASK(UUnicodeCategory.SpacingCombiningMark.ToInt32());
    }
}
