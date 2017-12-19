using ICU4N.Impl;
using ICU4N.Text;
using System;
using System.Collections.Generic;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.StringPrep
{
    /// <author>ram</author>
    public class IDNAReference
    {
        private static char[] ACE_PREFIX = new char[] { (char)0x0078, (char)0x006E, (char)0x002d, (char)0x002d };
        private static readonly int ACE_PREFIX_LENGTH = 4;

        private static readonly int MAX_LABEL_LENGTH = 63;
        private static readonly int HYPHEN = 0x002D;
        private static readonly int CAPITAL_A = 0x0041;
        private static readonly int CAPITAL_Z = 0x005A;
        private static readonly int LOWER_CASE_DELTA = 0x0020;
        private static readonly int FULL_STOP = 0x002E;

        public const IDNA2003Options DEFAULT = IDNA2003Options.Default; // 0x0000;
        public const IDNA2003Options ALLOW_UNASSIGNED = IDNA2003Options.AllowUnassigned; // 0x0001;
        public const IDNA2003Options USE_STD3_RULES = IDNA2003Options.UseSTD3Rules; // 0x0002;
        public static readonly NamePrepTransform transform = NamePrepTransform.GetInstance();

        public static bool IsReady
        {
            get { return transform.IsReady; }
        }

        private static bool StartsWithPrefix(StringBuffer src)
        {
            bool startsWithPrefix = true;

            if (src.Length < ACE_PREFIX_LENGTH)
            {
                return false;
            }
            for (int i = 0; i < ACE_PREFIX_LENGTH; i++)
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

        private static StringBuffer ToASCIILower(StringBuffer src)
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
                if (NamePrepTransform.IsLabelSeparator(src[start]))
                {
                    return start;
                }
            }
            // we have not found the separator just return length
            return start;
        }

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

        public static StringBuffer ConvertToASCII(String src, IDNA2003Options options)
        {
            UCharacterIterator iter = UCharacterIterator.GetInstance(src);
            return ConvertToASCII(iter, options);
        }
        public static StringBuffer ConvertToASCII(StringBuffer src, IDNA2003Options options)
        {
            UCharacterIterator iter = UCharacterIterator.GetInstance(src);
            return ConvertToASCII(iter, options);
        }
        public static StringBuffer ConvertToASCII(UCharacterIterator srcIter, IDNA2003Options options)
        {

            char[]
            caseFlags = null;

            // the source contains all ascii codepoints
            bool srcIsASCII = true;
            // assume the source contains all LDH codepoints
            bool srcIsLDH = true;

            //get the options
            bool useSTD3ASCIIRules = ((options & USE_STD3_RULES) != 0);

            int ch;
            // step 1
            while ((ch = srcIter.Next()) != UCharacterIterator.DONE)
            {
                if (ch > 0x7f)
                {
                    srcIsASCII = false;
                }
            }
            int failPos = -1;
            srcIter.SetToStart();
            StringBuffer processOut = null;
            // step 2 is performed only if the source contains non ASCII
            if (!srcIsASCII)
            {
                // step 2
                processOut = transform.Prepare(srcIter, (StringPrepOptions)options);
            }
            else
            {
                processOut = new StringBuffer(srcIter.GetText());
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
                    StringBuffer punyout = PunycodeReference.Encode(processOut, caseFlags);

                    // convert all codepoints to lower case ASCII
                    StringBuffer lowerOut = ToASCIILower(punyout);

                    //Step 7: prepend the ACE prefix
                    dest.Append(ACE_PREFIX, 0, ACE_PREFIX_LENGTH - 0); // ICU4N: Checked 3rd parameter
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
                throw new StringPrepParseException("The labels in the input are too long. Length > 64.",
                                        StringPrepErrorType.LabelTooLongError, dest.ToString(), 0);
            }
            return dest;
        }

        public static StringBuffer ConvertIDNtoASCII(UCharacterIterator iter, IDNA2003Options options)
        {
            return ConvertIDNToASCII(iter.GetText(), options);
        }
        public static StringBuffer ConvertIDNtoASCII(StringBuffer str, IDNA2003Options options)
        {
            return ConvertIDNToASCII(str.ToString(), options);
        }
        public static StringBuffer ConvertIDNToASCII(String src, IDNA2003Options options)
        {
            char[] srcArr = src.ToCharArray();
            StringBuffer result = new StringBuffer();
            int sepIndex = 0;
            int oldSepIndex = 0;
            for (; ; )
            {
                sepIndex = GetSeparatorIndex(srcArr, sepIndex, srcArr.Length);
                String label = new String(srcArr, oldSepIndex, sepIndex - oldSepIndex);
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
            return result;
        }

        public static StringBuffer ConvertToUnicode(String src, IDNA2003Options options)
        {
            UCharacterIterator iter = UCharacterIterator.GetInstance(src);
            return ConvertToUnicode(iter, options);
        }
        public static StringBuffer ConvertToUnicode(StringBuffer src, IDNA2003Options options)
        {
            UCharacterIterator iter = UCharacterIterator.GetInstance(src);
            return ConvertToUnicode(iter, options);
        }
        public static StringBuffer ConvertToUnicode(UCharacterIterator iter, IDNA2003Options options)
        {

            // the source contains all ascii codepoints
            bool srcIsASCII = true;

            int ch;
            int saveIndex = iter.Index;
            // step 1: find out if all the codepoints in src are ASCII
            while ((ch = iter.Next()) != UCharacterIterator.DONE)
            {
                if (ch > 0x7F)
                {
                    srcIsASCII = false;
                    break;
                }
            }

            // The RFC states that
            // <quote>
            // ToUnicode never fails. If any step fails, then the original input
            // is returned immediately in that step.
            // </quote>
            do
            {
                StringBuffer processOut;
                if (srcIsASCII == false)
                {
                    // step 2: process the string
                    iter.Index = (saveIndex);
                    try
                    {
                        processOut = transform.Prepare(iter, (StringPrepOptions)options);
                    }
                    catch (StringPrepParseException e)
                    {
                        break;
                    }
                }
                else
                {
                    // just point to source
                    processOut = new StringBuffer(iter.GetText());
                }

                // step 3: verify ACE Prefix
                if (StartsWithPrefix(processOut))
                {

                    // step 4: Remove the ACE Prefix
                    String temp = processOut.ToString(ACE_PREFIX_LENGTH, processOut.Length - ACE_PREFIX_LENGTH);

                    // step 5: Decode using punycode
                    StringBuffer decodeOut = null;
                    try
                    {
                        decodeOut = PunycodeReference.Decode(new StringBuffer(temp), null);
                    }
                    catch (StringPrepParseException e)
                    {
                        break;
                    }

                    // step 6:Apply toASCII
                    StringBuffer toASCIIOut = ConvertToASCII(decodeOut, options);

                    // step 7: verify
                    if (CompareCaseInsensitiveASCII(processOut, toASCIIOut) != 0)
                    {
                        break;
                    }
                    // step 8: return output of step 5
                    return decodeOut;
                }
            } while (false);

            return new StringBuffer(iter.GetText());
        }

        public static StringBuffer ConvertIDNToUnicode(UCharacterIterator iter, IDNA2003Options options)
        {
            return ConvertIDNToUnicode(iter.GetText(), options);
        }
        public static StringBuffer ConvertIDNToUnicode(StringBuffer str, IDNA2003Options options)
        {
            return ConvertIDNToUnicode(str.ToString(), options);
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
                String label = new String(srcArr, oldSepIndex, sepIndex - oldSepIndex);
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
                // increment the sepIndex to skip past the separator
                sepIndex++;
                oldSepIndex = sepIndex;
                result.Append((char)FULL_STOP);
            }
            return result;
        }
        //  TODO: optimize
        public static int Compare(StringBuffer s1, StringBuffer s2, IDNA2003Options options)
        {
            if (s1 == null || s2 == null)
            {
                throw new ArgumentException("One of the source buffers is null");
            }
            StringBuffer s1Out = ConvertIDNToASCII(s1.ToString(), options);
            StringBuffer s2Out = ConvertIDNToASCII(s2.ToString(), options);
            return CompareCaseInsensitiveASCII(s1Out, s2Out);
        }
        //  TODO: optimize
        public static int Compare(String s1, String s2, IDNA2003Options options)
        {
            if (s1 == null || s2 == null)
            {
                throw new ArgumentException("One of the source buffers is null");
            }
            StringBuffer s1Out = ConvertIDNToASCII(s1, options);
            StringBuffer s2Out = ConvertIDNToASCII(s2, options);
            return CompareCaseInsensitiveASCII(s1Out, s2Out);
        }
        //  TODO: optimize
        public static int Compare(UCharacterIterator i1, UCharacterIterator i2, IDNA2003Options options)
        {
            if (i1 == null || i2 == null)
            {
                throw new ArgumentException("One of the source buffers is null");
            }
            StringBuffer s1Out = ConvertIDNToASCII(i1.GetText(), options);
            StringBuffer s2Out = ConvertIDNToASCII(i2.GetText(), options);
            return CompareCaseInsensitiveASCII(s1Out, s2Out);
        }

    }
}
