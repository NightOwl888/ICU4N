using ICU4N.Lang;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        // ICU4N specific - LabelToASCII(ICharSequence label, StringBuilder dest, Info info) moved to UTS46Extension.tt

        // ICU4N specific - LabelToUnicode(ICharSequence label, StringBuilder dest, Info info) moved to UTS46Extension.tt

        // ICU4N specific - NameToASCII(ICharSequence name, StringBuilder dest, Info info) moved to UTS46Extension.tt

        // ICU4N specific - NameToUnicode(ICharSequence name, StringBuilder dest, Info info) moved to UTS46Extension.tt




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

        // ICU4N specific - NIsASCIIString(ICharSequence dest) moved to UTS46Extension.tt

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
        //    Info info) moved to UTS46Extension.tt

        // ICU4N specific - ProcessUnicode(ICharSequence src,
        //    int labelStart, int mappingStart,
        //    bool isLabel, bool toASCII,
        //    StringBuilder dest,
        //    Info info) moved to UTS46Extension.tt



        // returns the new dest.Length
        private int MapDevChars(StringBuilder dest, int labelStart, int mappingStart)
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
                        dest.Delete(i, i + 1);
                        --length;
                        break;
                    default:
                        ++i;
                        break;
                }
            }
            if (didMapDevChars)
            {
                // Mapping deviation characters might have resulted in an un-NFC string.
                // We could use either the NFC or the UTS #46 normalizer.
                // By using the UTS #46 normalizer again, we avoid having to load a second .nrm data file.
                string normalized = uts46Norm2.Normalize(dest.SubSequence(labelStart, dest.Length));
                dest.Replace(labelStart, 0x7fffffff, normalized);
                return dest.Length;
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
        //    ICharSequence label, int labelLength) moved to UTS46Extension.tt

        // returns the new label length
        private int ProcessLabel(StringBuilder dest,
                     int labelStart, int labelLength,
                     bool toASCII,
                     IDNAInfo info)
        {
            StringBuilder fromPunycode;
            StringBuilder labelString;
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
                    fromPunycode = Punycode.Decode(dest.SubSequence(labelStart + 4, labelStart + labelLength), null);
                }
                catch (StringPrepParseException e)
                {
                    AddLabelError(info, IDNAError.Punycode);
                    return MarkBadACELabel(dest, labelStart, labelLength, toASCII, info);
                }
                // Check for NFC, and for characters that are not
                // valid or deviation characters according to the normalizer.
                // If there is something wrong, then the string will change.
                // Note that the normalizer passes through non-LDH ASCII and deviation characters.
                // Deviation characters are ok in Punycode even in transitional processing.
                // In the code further below, if we find non-LDH ASCII and we have UIDNA_USE_STD3_RULES
                // then we will set UIDNA_ERROR_INVALID_ACE_LABEL there too.
                bool isValid = uts46Norm2.IsNormalized(fromPunycode);
                if (!isValid)
                {
                    AddLabelError(info, IDNAError.InvalidAceLabel);
                    return MarkBadACELabel(dest, labelStart, labelLength, toASCII, info);
                }
                labelString = fromPunycode;
                labelStart = 0;
                labelLength = fromPunycode.Length;
            }
            else
            {
                wasPunycode = false;
                labelString = dest;
            }
            // Validity check
            if (labelLength == 0)
            {
                AddLabelError(info, IDNAError.EmptyLabel);
                return ReplaceLabel(dest, destLabelStart, destLabelLength, labelString, labelLength);
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
                        AddLabelError(info, IDNAError.LabelHasDot);
                        labelString[i] = '\ufffd';
                    }
                    else if (disallowNonLDHDot && asciiData[c] < 0)
                    {
                        AddLabelError(info, IDNAError.Disallowed);
                        labelString[i] = '\ufffd';
                    }
                }
                else
                {
                    oredChars |= c;
                    if (disallowNonLDHDot && IsNonASCIIDisallowedSTD3Valid(c))
                    {
                        AddLabelError(info, IDNAError.Disallowed);
                        labelString[i] = '\ufffd';
                    }
                    else if (c == 0xfffd)
                    {
                        AddLabelError(info, IDNAError.Disallowed);
                    }
                }
                ++i;
            } while (i < limit);
            // Check for a leading combining mark after other validity checks
            // so that we don't report IDNA.Error.DISALLOWED for the U+FFFD from here.
            int c2;
            // "Unsafe" is ok because unpaired surrogates were mapped to U+FFFD.
            c2 = labelString.CodePointAt(labelStart);
            if ((U_GET_GC_MASK(c2) & U_GC_M_MASK) != 0)
            {
                AddLabelError(info, IDNAError.LeadingCombiningMark);
                labelString[labelStart] = '\ufffd';
                if (c2 > 0xffff)
                {
                    // Remove c's trail surrogate.
                    labelString.Remove(labelStart + 1, 1);
                    --labelLength;
                    if (labelString == dest)
                    {
                        --destLabelLength;
                    }
                }
            }
            if (!HasCertainLabelErrors(info, severeErrors))
            {
                // Do contextual checks only if we do not have U+FFFD from a severe error
                // because U+FFFD can make these checks fail.
                if ((options & UTS46Options.CheckBiDi) != 0 && (!IsBiDi(info) || IsOkBiDi(info)))
                {
                    CheckLabelBiDi(labelString, labelStart, labelLength, info);
                }
                if ((options & UTS46Options.CheckContextJ) != 0 && (oredChars & 0x200c) == 0x200c &&
                    !IsLabelOkContextJ(labelString, labelStart, labelLength)
                )
                {
                    AddLabelError(info, IDNAError.ContextJ);
                }
                if ((options & UTS46Options.CheckContextO) != 0 && oredChars >= 0xb7)
                {
                    CheckLabelContextO(labelString, labelStart, labelLength, info);
                }
                if (toASCII)
                {
                    if (wasPunycode)
                    {
                        // Leave a Punycode label unchanged if it has no severe errors.
                        if (destLabelLength > 63)
                        {
                            AddLabelError(info, IDNAError.LabelTooLong);
                        }
                        return destLabelLength;
                    }
                    else if (oredChars >= 0x80)
                    {
                        // Contains non-ASCII characters.
                        StringBuilder punycode;
                        try
                        {
                            punycode = Punycode.Encode(labelString.SubSequence(labelStart, labelStart + labelLength), null);
                        }
                        catch (StringPrepParseException e)
                        {
                            throw new ICUException(e);  // unexpected
                        }
                        punycode.Insert(0, "xn--");
                        if (punycode.Length > 63)
                        {
                            AddLabelError(info, IDNAError.LabelTooLong);
                        }
                        return ReplaceLabel(dest, destLabelStart, destLabelLength,
                                            punycode, punycode.Length);
                    }
                    else
                    {
                        // all-ASCII label
                        if (labelLength > 63)
                        {
                            AddLabelError(info, IDNAError.LabelTooLong);
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
                    AddLabelError(info, IDNAError.InvalidAceLabel);
                    return MarkBadACELabel(dest, destLabelStart, destLabelLength, toASCII, info);
                }
            }
            return ReplaceLabel(dest, destLabelStart, destLabelLength, labelString, labelLength);
        }
        private int MarkBadACELabel(StringBuilder dest,
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
                        AddLabelError(info, IDNAError.LabelHasDot);
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
                    AddLabelError(info, IDNAError.LabelTooLong);
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

        // ICU4N specific - CheckLabelBiDi(ICharSequence label, int labelStart, int labelLength, Info info) moved to UTS46Extension.tt

        // ICU4N specific - IsASCIIOkBiDi(ICharSequence s, int length) moved to UTS46Extension.tt

        // ICU4N specific - IsLabelOkContextJ(ICharSequence label, int labelStart, int labelLength) moved to UTS46Extension.tt

        // ICU4N specific - CheckLabelContextO(ICharSequence label, int labelStart, int labelLength, Info info) moved to UTS46Extension.tt

        // TODO: make public(?) -- in C, these are public in uchar.h
        private static int U_MASK(int x)
        {
            return 1 << x;
        }
        private static int U_GET_GC_MASK(int c)
        {
            return (1 << UCharacter.GetType(c).ToInt32());
        }
        private static int U_GC_M_MASK =
            U_MASK(UCharacterCategory.NonSpacingMark.ToInt32()) |
            U_MASK(UCharacterCategory.EnclosingMark.ToInt32()) |
            U_MASK(UCharacterCategory.SpacingCombiningMark.ToInt32());
    }
}
