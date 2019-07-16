using ICU4N.Text;
using System;
using System.Text;
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
    /// IDNA2003 implementation code, moved out of <see cref="Text.IDNA"/>
    /// while extending that class to support IDNA2008/UTS #46 as well.
    /// </summary>
    /// <author>Ram Viswanadha</author>
    public sealed class IDNA2003
    {
        /* IDNA ACE Prefix is "xn--" */
        private static char[] ACE_PREFIX = new char[] { (char)0x0078, (char)0x006E, (char)0x002d, (char)0x002d };
        //private static final int ACE_PREFIX_LENGTH      = ACE_PREFIX.Length;

        private static readonly int MAX_LABEL_LENGTH = 63;
        private static readonly int HYPHEN = 0x002D;
        private static readonly int CAPITAL_A = 0x0041;
        private static readonly int CAPITAL_Z = 0x005A;
        private static readonly int LOWER_CASE_DELTA = 0x0020;
        private static readonly int FULL_STOP = 0x002E;
        private static readonly int MAX_DOMAIN_NAME_LENGTH = 255;

        // The NamePrep profile object
        private static readonly StringPrep namePrep = StringPrep.GetInstance(StringPrepProfile.Rfc3491NamePrep);

        private static bool StartsWithPrefix(StringBuffer src)
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

        private static StringBuffer ToASCIILower(StringBuilder src) // ICU4N specific - changed src from ICharSequence to StringBuilder (only used in one place)
        {
            StringBuffer dest = new StringBuffer();
            for (int i = 0; i < src.Length; i++)
            {
                dest.Append(ToASCIILower(src[i]));
            }
            return dest;
        }

        private static int CompareCaseInsensitiveASCII(StringBuffer s1, StringBuffer s2)
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

        private static int GetSeparatorIndex(char[] src, int start, int limit)
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
            while((ch=iter.next())!= UCharacterIterator.DONE){
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

        /**
         * Ascertain if the given code point is a label separator as 
         * defined by the IDNA RFC
         * 
         * @param ch The code point to be ascertained
         * @return true if the char is a label separator
         * @stable ICU 2.8
         */
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

        public static StringBuffer ConvertToASCII(UCharacterIterator src, IDNA2003Options options)
        {

            bool[]
            caseFlags = null;

            // the source contains all ascii codepoints
            bool srcIsASCII = true;
            // assume the source contains all LDH codepoints
            bool srcIsLDH = true;

            //get the options
            bool useSTD3ASCIIRules = ((options & IDNA2003Options.UseSTD3Rules) != 0);
            int ch;
            // step 1
            while ((ch = src.MoveNext()) != UCharacterIterator.DONE)
            {
                if (ch > 0x7f)
                {
                    srcIsASCII = false;
                }
            }
            int failPos = -1;
            src.SetToStart();
            StringBuffer processOut = null;
            // step 2 is performed only if the source contains non ASCII
            if (!srcIsASCII)
            {
                // step 2
                processOut = namePrep.Prepare(src, (StringPrepOptions)options);
            }
            else
            {
                processOut = new StringBuffer(src.GetText());
            }
            int poLen = processOut.Length;

            if (poLen == 0)
            {
                throw new StringPrepParseException("Found zero length lable after NamePrep.", StringPrepErrorType.ZeroLengthLabel);
            }
            StringBuffer dest = new StringBuffer();

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
                        throw new StringPrepParseException("The input does not conform to the STD 3 ASCII rules",
                                                 StringPrepErrorType.STD3ASCIIRulesError,
                                                 processOut.ToString(),
                                                (failPos > 0) ? (failPos - 1) : failPos);
                    }
                    else if (processOut[0] == HYPHEN)
                    {
                        throw new StringPrepParseException("The input does not conform to the STD 3 ASCII rules",
                                                  StringPrepErrorType.STD3ASCIIRulesError, processOut.ToString(), 0);

                    }
                    else
                    {
                        throw new StringPrepParseException("The input does not conform to the STD 3 ASCII rules",
                                                 StringPrepErrorType.STD3ASCIIRulesError,
                                                 processOut.ToString(),
                                                 (poLen > 0) ? poLen - 1 : poLen);

                    }
                }
            }
            if (srcIsASCII)
            {
                dest = processOut;
            }
            else
            {
                // step 5 : verify the sequence does not begin with ACE prefix
                if (!StartsWithPrefix(processOut))
                {

                    //step 6: encode the sequence with punycode
                    caseFlags = new bool[poLen];

                    StringBuilder punyout = Punycode.Encode(processOut, caseFlags);

                    // convert all codepoints to lower case ASCII
                    StringBuffer lowerOut = ToASCIILower(punyout);

                    //Step 7: prepend the ACE prefix
                    dest.Append(ACE_PREFIX, 0, ACE_PREFIX.Length - 0); // ICU4N: Checked 3rd parameter
                                                                       //Step 6: copy the contents in b2 into dest
                    dest.Append(lowerOut);
                }
                else
                {

                    throw new StringPrepParseException("The input does not start with the ACE Prefix.",
                                             StringPrepErrorType.AcePrefixError, processOut.ToString(), 0);
                }
            }
            if (dest.Length > MAX_LABEL_LENGTH)
            {
                throw new StringPrepParseException("The labels in the input are too long. Length > 63.",
                                         StringPrepErrorType.LabelTooLongError, dest.ToString(), 0);
            }
            return dest;
        }

        public static StringBuffer ConvertIDNToASCII(string src, IDNA2003Options options)
        {
            char[] srcArr = src.ToCharArray();
            StringBuffer result = new StringBuffer();
            int sepIndex = 0;
            int oldSepIndex = 0;
            for (; ; )
            {
                sepIndex = GetSeparatorIndex(srcArr, sepIndex, srcArr.Length);
                string label = new string(srcArr, oldSepIndex, sepIndex - oldSepIndex);
                //make sure this is not a root label separator.
                if (!(label.Length == 0 && sepIndex == srcArr.Length))
                {
                    UCharacterIterator iter = UCharacterIterator.GetInstance(label);
                    result.Append(ConvertToASCII(iter, options));
                }
                if (sepIndex == srcArr.Length)
                {
                    break;
                }

                // increment the sepIndex to skip past the separator
                sepIndex++;
                oldSepIndex = sepIndex;
                result.Append((char)FULL_STOP);
            }
            if (result.Length > MAX_DOMAIN_NAME_LENGTH)
            {
                throw new StringPrepParseException("The output exceed the max allowed length.", StringPrepErrorType.DomainNameTooLongError);
            }
            return result;
        }

        public static StringBuffer ConvertToUnicode(UCharacterIterator src, IDNA2003Options options)
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
            int saveIndex = src.Index;
            // step 1: find out if all the codepoints in src are ASCII  
            while ((ch = src.MoveNext()) != UCharacterIterator.DONE)
            {
                if (ch > 0x7F)
                {
                    srcIsASCII = false;
                }/*else if((srcIsLDH = isLDHChar(ch))==false){
                failPos = src.getIndex();
            }*/
            }
            StringBuffer processOut;

            if (srcIsASCII == false)
            {
                try
                {
                    // step 2: process the string
                    src.Index = saveIndex;
                    processOut = namePrep.Prepare(src, (StringPrepOptions)options);
                }
                catch (StringPrepParseException ex)
                {
                    return new StringBuffer(src.GetText());
                }

            }
            else
            {
                //just point to source
                processOut = new StringBuffer(src.GetText());
            }
            // TODO:
            // The RFC states that 
            // <quote>
            // ToUnicode never fails. If any step fails, then the original input
            // is returned immediately in that step.
            // </quote>

            //step 3: verify ACE Prefix
            if (StartsWithPrefix(processOut))
            {
                StringBuffer decodeOut = null;

                //step 4: Remove the ACE Prefix
                string temp = processOut.ToString(ACE_PREFIX.Length, processOut.Length - ACE_PREFIX.Length);

                //step 5: Decode using punycode
                try
                {
                    decodeOut = new StringBuffer(Punycode.Decode(temp, caseFlags).ToString());
                }
                catch (StringPrepParseException e)
                {
                    decodeOut = null;
                }

                //step 6:Apply toASCII
                if (decodeOut != null)
                {
                    StringBuffer toASCIIOut = ConvertToASCII(UCharacterIterator.GetInstance(decodeOut), options);

                    //step 7: verify
                    if (CompareCaseInsensitiveASCII(processOut, toASCIIOut) != 0)
                    {
                        //                    throw new StringPrepParseException("The verification step prescribed by the RFC 3491 failed",
                        //                                             StringPrepParseException.VERIFICATION_ERROR); 
                        decodeOut = null;
                    }
                }

                //step 8: return output of step 5
                if (decodeOut != null)
                {
                    return decodeOut;
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

            return new StringBuffer(src.GetText());
        }

        public static StringBuffer ConvertIDNToUnicode(String src, IDNA2003Options options)
        {
            char[] srcArr = src.ToCharArray();
            StringBuffer result = new StringBuffer();
            int sepIndex = 0;
            int oldSepIndex = 0;
            for (; ; )
            {
                sepIndex = GetSeparatorIndex(srcArr, sepIndex, srcArr.Length);
                string label = new string(srcArr, oldSepIndex, sepIndex - oldSepIndex);
                if (label.Length == 0 && sepIndex != srcArr.Length)
                {
                    throw new StringPrepParseException("Found zero length lable after NamePrep.", StringPrepErrorType.ZeroLengthLabel);
                }
                UCharacterIterator iter = UCharacterIterator.GetInstance(label);
                result.Append(ConvertToUnicode(iter, options));
                if (sepIndex == srcArr.Length)
                {
                    break;
                }
                // Unlike the ToASCII operation we don't normalize the label separators
                result.Append(srcArr[sepIndex]);
                // increment the sepIndex to skip past the separator
                sepIndex++;
                oldSepIndex = sepIndex;
            }
            if (result.Length > MAX_DOMAIN_NAME_LENGTH)
            {
                throw new StringPrepParseException("The output exceed the max allowed length.", StringPrepErrorType.DomainNameTooLongError);
            }
            return result;
        }

        public static int Compare(string s1, string s2, IDNA2003Options options)
        {
            StringBuffer s1Out = ConvertIDNToASCII(s1, options);
            StringBuffer s2Out = ConvertIDNToASCII(s2, options);
            return CompareCaseInsensitiveASCII(s1Out, s2Out);
        }
    }
}
