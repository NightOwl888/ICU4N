using ICU4N.Text;
using J2N.Text;
using System;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Impl
{
    /// <summary>
    /// Options for <see cref="IDNA2003"/>.
    /// </summary>
    [Flags]
    public enum IDNA2003Options
    {
        /// <summary>
        /// Default options value: None of the other options are set.
        /// For use in static worker and factory methods.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        Default = 0,
        /// <summary>
        /// Option to allow unassigned code points in domain names and labels.
        /// For use in static worker and factory methods.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        AllowUnassigned = 1,
        /// <summary>
        /// Option to check whether the input conforms to the STD3 ASCII rules,
        /// for example the restriction of labels to LDH characters
        /// (ASCII Letters, Digits and Hyphen-Minus).
        /// For use in static worker and factory methods.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        UseSTD3Rules = 2,
    }

    /// <summary>
    /// IDNA2003 implementation code, moved out of <see cref="ICU4N.Text.IDNA"/>
    /// while extending that class to support IDNA2008/UTS #46 as well.
    /// </summary>
    /// <author>Ram Viswanadha</author>
    public sealed class IDNA2003
    {
        private const int CharStackBufferSize = 64;

        /* IDNA ACE Prefix is "xn--" */
        private static readonly char[] ACE_PREFIX = new char[] { (char)0x0078, (char)0x006E, (char)0x002d, (char)0x002d };
        //private static final int ACE_PREFIX_LENGTH      = ACE_PREFIX.Length;

        private const int MAX_LABEL_LENGTH = 63;
        private const int HYPHEN = 0x002D;
        private const int CAPITAL_A = 0x0041;
        private const int CAPITAL_Z = 0x005A;
        private const int LOWER_CASE_DELTA = 0x0020;
        private const int FULL_STOP = 0x002E;
        private const int MAX_DOMAIN_NAME_LENGTH = 255;

        // The NamePrep profile object
        private static readonly StringPrep namePrep = StringPrep.GetInstance(StringPrepProfile.Rfc3491NamePrep);

        private static bool StartsWithPrefix(ReadOnlySpan<char> src)
        {
            bool startsWithPrefix = true;

            if (src.Length < ACE_PREFIX.Length)
            {
                return false;
            }
            for (int i = 0; i < ACE_PREFIX.Length; i++)
            {
                if (ToASCIILower(src[i]) != ACE_PREFIX[i])
                {
                    startsWithPrefix = false;
                }
            }
            return startsWithPrefix;
        }

        private static char ToASCIILower(char ch)
        {
            if (CAPITAL_A <= ch && ch <= CAPITAL_Z)
            {
                return (char)(ch + LOWER_CASE_DELTA);
            }
            return ch;
        }

        private static void ToASCIILower(ReadOnlySpan<char> source, ref ValueStringBuilder destination)
        {
            for (int i = 0; i < source.Length; i++)
            {
                destination.Append(ToASCIILower(source[i]));
            }
        }

        private static int CompareCaseInsensitiveASCII(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2)
        {
            char c1, c2;
            int rc;
            for (int i = 0;/* no condition */; i++)
            {
                /* If we reach the ends of both strings then they match */
                if (i == s1.Length)
                {
                    return 0;
                }

                c1 = s1[i];
                c2 = s2[i];

                /* Case-insensitive comparison */
                if (c1 != c2)
                {
                    rc = ToASCIILower(c1) - ToASCIILower(c2);
                    if (rc != 0)
                    {
                        return rc;
                    }
                }
            }
        }

        private static int GetSeparatorIndex(ReadOnlySpan<char> src, int start, int limit)
        {
            for (; start < limit; start++)
            {
                if (IsLabelSeparator(src[start]))
                {
                    return start;
                }
            }
            // we have not found the separator just return length
            return start;
        }

        /*
        private static int getSeparatorIndex(UCharacterIterator iter){
            int currentIndex = iter.getIndex();
            int separatorIndex = 0;
            int ch;
            while((ch=iter.next())!= UCharacterIterator.Done){
                if(isLabelSeparator(ch)){
                    separatorIndex = iter.getIndex();
                    iter.setIndex(currentIndex);
                    return separatorIndex;
                }
            }
            // reset index
            iter.setIndex(currentIndex);
            // we have not found the separator just return the length

        }
        */


        private static bool IsLDHChar(int ch)
        {
            // high runner case
            if (ch > 0x007A)
            {
                return false;
            }
            //[\\u002D \\u0030-\\u0039 \\u0041-\\u005A \\u0061-\\u007A]
            if ((ch == 0x002D) ||
                (0x0030 <= ch && ch <= 0x0039) ||
                (0x0041 <= ch && ch <= 0x005A) ||
                (0x0061 <= ch && ch <= 0x007A)
              )
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Ascertain if the given code point is a label separator as
        /// defined by the IDNA RFC
        /// </summary>
        /// <param name="ch">The code point to be ascertained</param>
        /// <returns>true if the char is a label separator</returns>
        /// <stable>ICU 2.8</stable>
        private static bool IsLabelSeparator(int ch)
        {
            switch (ch)
            {
                case 0x002e:
                case 0x3002:
                case 0xFF0E:
                case 0xFF61:
                    return true;
                default:
                    return false;
            }
        }

        public static string ConvertToASCII(ReadOnlySpan<char> source, IDNA2003Options options)
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                if (!TryConvertToASCII(source, ref sb, options, out StringPrepErrorType errorType, out string rules, out int errorPosition))
                    ThrowHelper.ThrowStringPrepFormatException(errorType, rules, errorPosition);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        public static bool TryConvertToASCII(ReadOnlySpan<char> source, Span<char> destination, out int charsLength, IDNA2003Options options, out StringPrepErrorType errorType)
        {
            ValueStringBuilder sb = new ValueStringBuilder(destination);
            try
            {
                bool success = TryConvertToASCII(source, ref sb, options, out errorType, out _, out _);
                if (!sb.FitsInitialBuffer(out charsLength) && success)
                {
                    errorType = StringPrepErrorType.BufferOverflowError;
                    return false;
                }
                return success;
            }
            finally
            {
                sb.Dispose();
            }
        }

        // ICU4N: Factored out UCharacterIterator. Ported from usprep.cpp/usprep_prepare().
        internal static bool TryConvertToASCII(ReadOnlySpan<char> src, ref ValueStringBuilder destination, IDNA2003Options options, out StringPrepErrorType errorType, out string rules, out int errorPosition)
        {
            bool[] caseFlags = null;

            // the source contains all ascii codepoints
            bool srcIsASCII = true;
            // assume the source contains all LDH codepoints
            bool srcIsLDH = true;

            //get the options
            bool useSTD3ASCIIRules = ((options & IDNA2003Options.UseSTD3Rules) != 0);
            int ch;
            // step 1
            for (int i = 0; i < src.Length; i++)
            {
                ch = src[i];
                if (ch > 0x7f)
                {
                    srcIsASCII = false;
                    break;
                }
            }
            int failPos = -1;
            ValueStringBuilder processOut = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            ValueStringBuilder dest = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                // step 2 is performed only if the source contains non ASCII
                if (!srcIsASCII)
                {
                    // step 2
                    if (!namePrep.TryPrepare(src, ref processOut, (StringPrepOptions)options, out errorType, out rules, out errorPosition))
                        return false;
                }
                else
                {
                    processOut.Append(src);
                }
                int poLen = processOut.Length;

                if (poLen == 0)
                {
                    errorType = StringPrepErrorType.ZeroLengthLabel;
                    rules = string.Empty;
                    errorPosition = 0;
                    return false;
                }

                // reset the variable to verify if output of prepare is ASCII or not
                srcIsASCII = true;

                // step 3 & 4
                for (int j = 0; j < poLen; j++)
                {
                    ch = processOut[j];
                    if (ch > 0x7F)
                    {
                        srcIsASCII = false;
                    }
                    else if (IsLDHChar(ch) == false)
                    {
                        // here we do not assemble surrogates
                        // since we know that LDH code points
                        // are in the ASCII range only
                        srcIsLDH = false;
                        failPos = j;
                    }
                }

                if (useSTD3ASCIIRules == true)
                {
                    // verify 3a and 3b
                    if (srcIsLDH == false /* source contains some non-LDH characters */
                        || processOut[0] == HYPHEN
                        || processOut[processOut.Length - 1] == HYPHEN)
                    {

                        /* populate the parseError struct */
                        if (srcIsLDH == false)
                        {
                            errorType = StringPrepErrorType.STD3ASCIIRulesError;
                            rules = processOut.AsSpan().ToString();
                            errorPosition = (failPos > 0) ? (failPos - 1) : failPos;
                            return false;
                        }
                        else if (processOut[0] == HYPHEN)
                        {
                            errorType = StringPrepErrorType.STD3ASCIIRulesError;
                            rules = processOut.AsSpan().ToString();
                            errorPosition = 0;
                            return false;
                        }
                        else
                        {
                            errorType = StringPrepErrorType.STD3ASCIIRulesError;
                            rules = processOut.AsSpan().ToString();
                            errorPosition = (poLen > 0) ? poLen - 1 : poLen;
                            return false;
                        }
                    }
                }
                if (srcIsASCII)
                {
                    dest.Append(processOut.AsSpan()); //dest = processOut;
                }
                else
                {
                    // step 5 : verify the sequence does not begin with ACE prefix
                    if (!StartsWithPrefix(processOut.AsSpan()))
                    {
                        //step 6: encode the sequence with punycode
                        caseFlags = new bool[poLen];

                        ValueStringBuilder punyOut = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
                        ValueStringBuilder lowerOut = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
                        try
                        {
                            if (!Punycode.TryEncode(processOut.AsSpan(), ref punyOut, caseFlags, out errorType))
                            {
                                rules = string.Empty;
                                errorPosition = 0;
                                return false;
                            }

                            // convert all codepoints to lower case ASCII
                            ToASCIILower(punyOut.AsSpan(), ref lowerOut);

                            //Step 7: prepend the ACE prefix
                            dest.Append(ACE_PREFIX, 0, ACE_PREFIX.Length - 0); // ICU4N: Checked 3rd parameter

                            //Step 6: copy the contents in b2 into dest
                            dest.Append(lowerOut.AsSpan());
                        }
                        finally
                        {
                            lowerOut.Dispose();
                            punyOut.Dispose();
                        }
                    }
                    else
                    {
                        errorType = StringPrepErrorType.AcePrefixError;
                        rules = processOut.AsSpan().ToString();
                        errorPosition = 0;
                        return false;
                    }
                }
                if (dest.Length > MAX_LABEL_LENGTH)
                {
                    errorType = StringPrepErrorType.LabelTooLongError;
                    rules = processOut.AsSpan().ToString();
                    errorPosition = 0;
                    return false;
                }
                destination.Append(dest.AsSpan());

                errorType = (StringPrepErrorType)(-1);
                rules = default;
                errorPosition = -1;
                return true;
            }
            finally
            {
                processOut.Dispose();
                dest.Dispose();
            }
        }

        public static string ConvertIDNToASCII(ReadOnlySpan<char> source, IDNA2003Options options)
        {
            ValueStringBuilder sb = source.Length <= CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                : new ValueStringBuilder(source.Length);
            try
            {
                if (!TryConvertIDNToASCII(source, ref sb, options, out StringPrepErrorType errorType, out string rules, out int errorPosition))
                    ThrowHelper.ThrowStringPrepFormatException(errorType, rules, errorPosition);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        public static bool TryConvertIDNToASCII(ReadOnlySpan<char> source, Span<char> destination, out int charsLength, IDNA2003Options options, out StringPrepErrorType errorType)
        {
            ValueStringBuilder sb = new ValueStringBuilder(destination);
            try
            {
                bool success = TryConvertIDNToASCII(source, ref sb, options, out errorType, out _, out _);
                if (!sb.FitsInitialBuffer(out charsLength) && success)
                {
                    errorType = StringPrepErrorType.BufferOverflowError;
                    return false;
                }
                return success;
            }
            finally
            {
                sb.Dispose();
            }
        }

        internal static bool TryConvertIDNToASCII(ReadOnlySpan<char> src, ref ValueStringBuilder destination, IDNA2003Options options, out StringPrepErrorType errorType, out string rules, out int errorPosition)
        {
            int sepIndex = 0;
            int oldSepIndex = 0;
            for (; ; )
            {
                sepIndex = GetSeparatorIndex(src, sepIndex, src.Length);
                ReadOnlySpan<char> label = src.Slice(oldSepIndex, sepIndex - oldSepIndex);
                //make sure this is not a root label separator.
                if (!(label.Length == 0 && sepIndex == src.Length))
                {
                    if (!TryConvertToASCII(label, ref destination, options, out errorType, out rules, out errorPosition))
                        return false;
                }
                if (sepIndex == src.Length)
                {
                    break;
                }

                // increment the sepIndex to skip past the separator
                sepIndex++;
                oldSepIndex = sepIndex;
                destination.Append((char)FULL_STOP);
            }
            if (destination.Length > MAX_DOMAIN_NAME_LENGTH)
            {
                errorType = StringPrepErrorType.DomainNameTooLongError;
                rules = string.Empty;
                errorPosition = 0;
                return false;
            }

            errorType = (StringPrepErrorType)(-1);
            rules = default;
            errorPosition = 0;
            return true;//return result;
        }

        public static string ConvertToUnicode(ReadOnlySpan<char> source, IDNA2003Options options)
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                if (!TryConvertToUnicode(source, ref sb, options, out StringPrepErrorType errorType, out string rules, out int errorPosition))
                    ThrowHelper.ThrowStringPrepFormatException(errorType, rules, errorPosition);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        public static bool TryConvertToUnicode(ReadOnlySpan<char> source, Span<char> destination, out int charsLength, IDNA2003Options options, out StringPrepErrorType errorType)
        {
            ValueStringBuilder sb = new ValueStringBuilder(destination);
            try
            {
                bool success = TryConvertToUnicode(source, ref sb, options, out errorType, out _, out _);
                if (!sb.FitsInitialBuffer(out charsLength) && success)
                {
                    errorType = StringPrepErrorType.BufferOverflowError;
                    return false;
                }
                return success;
            }
            finally
            {
                sb.Dispose();
            }
        }

        // ICU4N: Factored out UCharacterIterator. Ported from usprep.cpp/_internal_toUnicode().
        internal static bool TryConvertToUnicode(ReadOnlySpan<char> src, ref ValueStringBuilder destination, IDNA2003Options options, out StringPrepErrorType errorType, out string rules, out int errorPosition)
        {
            bool[] caseFlags = null;

            // the source contains all ascii codepoints
            bool srcIsASCII = true;
            // assume the source contains all LDH codepoints
            //bool srcIsLDH = true; 

            //get the options
            //bool useSTD3ASCIIRules = ((options & USE_STD3_RULES) != 0);

            //int failPos = -1;
            int ch;
            int srcLength = src.Length;
            // step 1: find out if all the codepoints in src are ASCII
            for (int i = 0; i < srcLength; i++)
            {
                ch = src[i];
                if (ch > 0x7f)
                {
                    srcIsASCII = false;
                    break;
                }
            }
            ValueStringBuilder processOut = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                if (srcIsASCII == false)
                {
                    // step 2: process the string
                    if (!namePrep.TryPrepare(src, ref processOut, (StringPrepOptions)options, out _, out _, out _))
                    {
                        destination.Append(src);
                        errorType = (StringPrepErrorType)(-1);
                        rules = default;
                        errorPosition = -1;
                        return true;
                    }
                }
                else
                {
                    //just point to source
                    processOut.Append(src);
                }
                // TODO:
                // The RFC states that 
                // <quote>
                // ToUnicode never fails. If any step fails, then the original input
                // is returned immediately in that step.
                // </quote>

                //step 3: verify ACE Prefix
                if (StartsWithPrefix(processOut.AsSpan()))
                {
                    //OpenStringBuilder decodeOut = null;
                    ValueStringBuilder decodeOut = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
                    try
                    {
                        //step 4: Remove the ACE Prefix
                        ReadOnlySpan<char> temp = processOut.AsSpan(ACE_PREFIX.Length, processOut.Length - ACE_PREFIX.Length);

                        //step 5: Decode using punycode
                        bool success = Punycode.TryDecode(temp, ref decodeOut, caseFlags, out _);

                        //step 6:Apply toASCII
                        if (success)
                        {
                            ValueStringBuilder toASCIIOut = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
                            try
                            {
                                if (!TryConvertToASCII(decodeOut.AsSpan(), ref toASCIIOut, options, out errorType, out rules, out errorPosition))
                                {
                                    return false;
                                }

                                //step 7: verify
                                if (CompareCaseInsensitiveASCII(processOut.AsSpan(), toASCIIOut.AsSpan()) != 0)
                                {
                                    //                    throw new StringPrepParseException("The verification step prescribed by the RFC 3491 failed",
                                    //                                             StringPrepParseException.VERIFICATION_ERROR); 
                                    success = false;
                                }
                            }
                            finally
                            {
                                toASCIIOut.Dispose();
                            }
                        }

                        //step 8: return output of step 5
                        if (success)
                        {
                            destination.Append(decodeOut.AsSpan());
                            errorType = (StringPrepErrorType)(-1);
                            rules = default;
                            errorPosition = -1;
                            return true;
                        }
                    }
                    finally
                    {
                        decodeOut.Dispose();
                    }
                }

                //        }else{
                //            // verify that STD3 ASCII rules are satisfied
                //            if(useSTD3ASCIIRules == true){
                //                if( srcIsLDH == false /* source contains some non-LDH characters */
                //                    || processOut.charAt(0) ==  HYPHEN 
                //                    || processOut.charAt(processOut.Length-1) == HYPHEN){
                //    
                //                    if(srcIsLDH==false){
                //                        throw new StringPrepParseException("The input does not conform to the STD 3 ASCII rules",
                //                                                 StringPrepParseException.STD3_ASCII_RULES_ERROR,processOut.toString(),
                //                                                 (failPos>0) ? (failPos-1) : failPos);
                //                    }else if(processOut.charAt(0) == HYPHEN){
                //                        throw new StringPrepParseException("The input does not conform to the STD 3 ASCII rules",
                //                                                 StringPrepParseException.STD3_ASCII_RULES_ERROR,
                //                                                 processOut.toString(),0);
                //         
                //                    }else{
                //                        throw new StringPrepParseException("The input does not conform to the STD 3 ASCII rules",
                //                                                 StringPrepParseException.STD3_ASCII_RULES_ERROR,
                //                                                 processOut.toString(),
                //                                                 processOut.Length);
                //    
                //                    }
                //                }
                //            }
                //            // just return the source
                //            return new StringBuffer(src.getText());
                //        }  
            }
            finally
            {
                processOut.Dispose();
            }

            destination.Append(src);//return new StringBuffer(src.GetText());
            errorType = (StringPrepErrorType)(-1);
            rules = default;
            errorPosition = -1;
            return true;
        }


        public static string ConvertIDNToUnicode(ReadOnlySpan<char> source, IDNA2003Options options)
        {
            ValueStringBuilder sb = source.Length <= CharStackBufferSize
                ? new ValueStringBuilder(stackalloc char[CharStackBufferSize])
                : new ValueStringBuilder(source.Length);
            try
            {
                if (!TryConvertIDNToUnicode(source, ref sb, options, out StringPrepErrorType errorType, out string rules, out int errorPosition))
                    ThrowHelper.ThrowStringPrepFormatException(errorType, rules, errorPosition);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        public static bool TryConvertIDNToUnicode(ReadOnlySpan<char> source, Span<char> destination, out int charsLength, IDNA2003Options options, out StringPrepErrorType errorType)
        {
            ValueStringBuilder sb = new ValueStringBuilder(destination);
            try
            {
                bool success = TryConvertIDNToUnicode(source, ref sb, options, out errorType, out _, out _);
                if (!sb.FitsInitialBuffer(out charsLength) && success)
                {
                    errorType = StringPrepErrorType.BufferOverflowError;
                    return false;
                }
                return success;
            }
            finally
            {
                sb.Dispose();
            }
        }

        internal static bool TryConvertIDNToUnicode(ReadOnlySpan<char> src, ref ValueStringBuilder destination, IDNA2003Options options, out StringPrepErrorType errorType, out string rules, out int errorPosition)
        {
            int sepIndex = 0;
            int oldSepIndex = 0;
            for (; ; )
            {
                sepIndex = GetSeparatorIndex(src, sepIndex, src.Length);
                ReadOnlySpan<char> label = src.Slice(oldSepIndex, sepIndex - oldSepIndex);
                if (label.Length == 0 && sepIndex != src.Length)
                {
                    errorType = StringPrepErrorType.ZeroLengthLabel;
                    rules = string.Empty;
                    errorPosition = 0;
                    return false;
                }
                if (!TryConvertToUnicode(label, ref destination, options, out errorType, out rules, out errorPosition))
                    return false;
                if (sepIndex == src.Length)
                {
                    break;
                }
                // Unlike the ToASCII operation we don't normalize the label separators
                destination.Append(src[sepIndex]);
                // increment the sepIndex to skip past the separator
                sepIndex++;
                oldSepIndex = sepIndex;
            }
            if (destination.Length > MAX_DOMAIN_NAME_LENGTH)
            {
                errorType = StringPrepErrorType.DomainNameTooLongError;
                rules = string.Empty;
                errorPosition = 0;
                return false;
            }
            
            errorType = (StringPrepErrorType)(-1);
            rules = default;
            errorPosition = -1;
            return true;//return result;
        }

        // ICU4N TODO: API - Comparers in .NET never throw exceptions. But need some sort of
        // plan if the text cannot be converted as to what value to return if one or the other
        // string is not convertible to ASCII.
        public static int Compare(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2, IDNA2003Options options)
        {
            StringPrepErrorType errorType;
            string rules;
            int errorPosition;
            ValueStringBuilder s1Out = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            ValueStringBuilder s2Out = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                if (!TryConvertIDNToASCII(s1, ref s1Out, options, out errorType, out rules, out errorPosition))
                    ThrowHelper.ThrowStringPrepFormatException(errorType, rules, errorPosition);
                if (!TryConvertIDNToASCII(s2, ref s2Out, options, out errorType, out rules, out errorPosition))
                    ThrowHelper.ThrowStringPrepFormatException(errorType, rules, errorPosition);
                return CompareCaseInsensitiveASCII(s1Out.AsSpan(), s2Out.AsSpan());
            }
            finally
            {
                s2Out.Dispose();
                s1Out.Dispose();
            }
        }
    }
}
