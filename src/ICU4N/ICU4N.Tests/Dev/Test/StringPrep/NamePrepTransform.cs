using ICU4N.Impl;
using ICU4N.Lang;
using ICU4N.Text;
using System;
using System.Reflection;
using System.Security;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.StringPrep
{
    /// <author>ram</author>
    public class NamePrepTransform
    {
        private static readonly NamePrepTransform transform = new NamePrepTransform();

        private UnicodeSet labelSeparatorSet;
        private UnicodeSet prohibitedSet;
        private UnicodeSet unassignedSet;
        private MapTransform mapTransform;
        public const StringPrepOptions NONE = StringPrepOptions.Default;
        public const StringPrepOptions ALLOW_UNASSIGNED = StringPrepOptions.AllowUnassigned;

        private NamePrepTransform()
        {
            // load the resource bundle
            //ICUResourceBundle bundle = (ICUResourceBundle)ICUResourceBundle.GetBundleInstance("com/ibm/icu/dev/data/testdata", "idna_rules", typeof(NamePrepTransform).GetTypeInfo().Assembly, true);
            ICUResourceBundle bundle = (ICUResourceBundle)ICUResourceBundle.GetBundleInstance("Dev/Data/TestData", "idna_rules", typeof(NamePrepTransform).GetTypeInfo().Assembly, true);
            String mapRules = bundle.GetString("MapNoNormalization");
            mapRules += bundle.GetString("MapNFKC");
            // disable
            mapTransform = new MapTransform("CaseMap", mapRules, 0 /*Transliterator.FORWARD*/);
            labelSeparatorSet = new UnicodeSet(bundle.GetString("LabelSeparatorSet"));
            prohibitedSet = new UnicodeSet(bundle.GetString("ProhibitedSet"));
            unassignedSet = new UnicodeSet(bundle.GetString("UnassignedSet"));
        }

        public static NamePrepTransform GetInstance()
        {
            return transform;
        }
        public static bool IsLabelSeparator(int ch)
        {
            return transform.labelSeparatorSet.Contains(ch);
        }

        /*
          1) Map -- For each character in the input, check if it has a mapping
             and, if so, replace it with its mapping.  

          2) Normalize -- Possibly normalize the result of step 1 using Unicode
             normalization. 

          3) Prohibit -- Check for any characters that are not allowed in the
             output.  If any are found, return an error.  

          4) Check bidi -- Possibly check for right-to-left characters, and if
             any are found, make sure that the whole string satisfies the
             requirements for bidirectional strings.  If the string does not
             satisfy the requirements for bidirectional strings, return an
             error.  
             [Unicode3.2] defines several bidirectional categories; each character
              has one bidirectional category assigned to it.  For the purposes of
              the requirements below, an "RandALCat character" is a character that
              has Unicode bidirectional categories "R" or "AL"; an "LCat character"
              is a character that has Unicode bidirectional category "L".  Note


              that there are many characters which fall in neither of the above
              definitions; Latin digits (<U+0030> through <U+0039>) are examples of
              this because they have bidirectional category "EN".

              In any profile that specifies bidirectional character handling, all
              three of the following requirements MUST be met:

              1) The characters in section 5.8 MUST be prohibited.

              2) If a string contains any RandALCat character, the string MUST NOT
                 contain any LCat character.

              3) If a string contains any RandALCat character, a RandALCat
                 character MUST be the first character of the string, and a
                 RandALCat character MUST be the last character of the string.
       */

        public bool IsReady
        {
            get { return mapTransform.IsReady; }
        }

        public StringBuffer Prepare(UCharacterIterator src,
                                           StringPrepOptions options)
        {
            return Prepare(src.GetText(), options);
        }

        private String Map(String src, StringPrepOptions options)
        {
            // map 
            bool allowUnassigned = ((options & ALLOW_UNASSIGNED) > 0);
            // disable test
            String caseMapOut = mapTransform.Transliterate(src);
            UCharacterIterator iter = UCharacterIterator.GetInstance(caseMapOut);
            int ch;
            while ((ch = iter.NextCodePoint()) != UCharacterIterator.DONE)
            {
                if (transform.unassignedSet.Contains(ch) == true && allowUnassigned == false)
                {
                    throw new StringPrepParseException("An unassigned code point was found in the input",
                                             StringPrepParseException.UNASSIGNED_ERROR);
                }
            }
            return caseMapOut;
        }
        public StringBuffer Prepare(String src, StringPrepOptions options)
        {

            int ch;
            String mapOut = Map(src, options);
            UCharacterIterator iter = UCharacterIterator.GetInstance(mapOut);

            UnicodeDirection direction = UnicodeDirection.CharDirectionCount,
            firstCharDir = UnicodeDirection.CharDirectionCount;
            int rtlPos = -1, ltrPos = -1;
            bool rightToLeft = false, leftToRight = false;

            while ((ch = iter.NextCodePoint()) != UCharacterIterator.DONE)
            {


                if (transform.prohibitedSet.Contains(ch) == true && ch != 0x0020)
                {
                    throw new StringPrepParseException("A prohibited code point was found in the input",
                                             StringPrepParseException.PROHIBITED_ERROR,
                                             iter.GetText(), iter.Index);
                }

                direction = (UnicodeDirection)UCharacter.GetDirection(ch); // ICU4N TODO: Remove the cast
                if (firstCharDir == UnicodeDirection.CharDirectionCount)
                {
                    firstCharDir = direction;
                }
                if (direction == UnicodeDirection.LeftToRight)
                {
                    leftToRight = true;
                    ltrPos = iter.Index - 1;
                }
                if (direction == UnicodeDirection.RightToLeft || direction == UnicodeDirection.RightToLeftArabic)
                {
                    rightToLeft = true;
                    rtlPos = iter.Index - 1;
                }
            }

            // satisfy 2
            if (leftToRight == true && rightToLeft == true)
            {
                throw new StringPrepParseException("The input does not conform to the rules for BiDi code points.",
                                         StringPrepParseException.CHECK_BIDI_ERROR, iter.GetText(), (rtlPos > ltrPos) ? rtlPos : ltrPos);
            }

            //satisfy 3
            if (rightToLeft == true &&
                !((firstCharDir == UnicodeDirection.RightToLeft || firstCharDir == UnicodeDirection.RightToLeftArabic) &&
                (direction == UnicodeDirection.RightToLeft || direction == UnicodeDirection.RightToLeftArabic))
               )
            {
                throw new StringPrepParseException("The input does not conform to the rules for BiDi code points.",
                                          StringPrepParseException.CHECK_BIDI_ERROR, iter.GetText(), (rtlPos > ltrPos) ? rtlPos : ltrPos);
            }

            return new StringBuffer(mapOut);

        }

        private class MapTransform
        {
            private Object translitInstance;
            private MethodInfo translitMethod;
            private bool isReady;

            internal MapTransform(String id, String rule, int direction)
            {
                isReady = Initialize(id, rule, direction);
            }

            internal bool Initialize(String id, String rule, int direction)
            {
                try
                {
                    Type cls = Type.GetType("ICU4N.Text.Transliterator, ICU4N.Transliterator");
                    MethodInfo createMethod = cls.GetMethod("CreateFromRules", new Type[] { typeof(String), typeof(String), typeof(int) });
                    translitInstance = createMethod.Invoke(null, new object[] { id, rule, direction });
                    translitMethod = cls.GetMethod("Transliterate", new Type[] { typeof(String) });
                }
                catch (Exception e)
                {
                    return false;
                }
                return true;
            }

            internal bool IsReady
            {
                get { return isReady; }
            }

            internal String Transliterate(String text)
            {
                if (!isReady)
                {
                    throw new InvalidOperationException("Transliterator is not ready");
                }
                String result = null;
                try
                {
                    result = (String)translitMethod.Invoke(translitInstance, new object[] { text });
                }
                catch (TargetInvocationException ite)
                {
                    throw new Exception(ite.ToString(), ite);
                }
                catch (SecurityException iae)
                {
                    throw new Exception(iae.ToString(), iae);
                }
                return result;
            }
        }
    }
}
