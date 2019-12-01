using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using ICU4N.Text;
using J2N;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Lang
{
    /// <summary>
    /// General test of UnicodeSet
    /// </summary>
    public class UnicodeSetTest : TestFmwk
    {
        const String NOT = "%%%%";

        private static bool IsCccValue(int ccc)
        {
            switch (ccc)
            {
                case 0:
                case 1:
                case 7:
                case 8:
                case 9:
                case 200:
                case 202:
                case 216:
                case 218:
                case 220:
                case 222:
                case 224:
                case 226:
                case 228:
                case 230:
                case 232:
                case 233:
                case 234:
                case 240:
                    return true;
                default:
                    return false;
            }
        }

        [Test]
        public void TestPropertyAccess()
        {
            int count = 0;
            // test to see that all of the names work
            for (UProperty propNum = UProperty.Binary_Start; propNum < UProperty.Int_Limit; ++propNum)
            {
                count++;
                //Skipping tests in the non-exhaustive mode to shorten the test time ticket#6475
                if (TestFmwk.GetExhaustiveness() <= 5 && count % 5 != 0)
                {
                    continue;
                }
                if (propNum >= UProperty.Binary_Limit && propNum < UProperty.Int_Start)
                { // skip the gap
                    propNum = UProperty.Int_Start;
                }
                for (NameChoice nameChoice = NameChoice.Short; nameChoice <= NameChoice.Long; ++nameChoice)
                {
                    String propName;
                    try
                    {
                        propName = UChar.GetPropertyName(propNum, nameChoice);
                        if (propName == null)
                        {
                            if (nameChoice == NameChoice.Short) continue; // allow non-existent short names
                            throw new NullReferenceException();
                        }
                    }
                    catch (Exception e1)
                    {
                        Errln("Can't get property name for: "
                                + "Property (" + propNum + ")"
                                + ", NameChoice: " + nameChoice + ", "
                                + e1.GetType().Name);
                        continue;
                    }
                    Logln("Property (" + propNum + "): " + propName);
                    for (int valueNum = UChar.GetIntPropertyMinValue(propNum); valueNum <= UChar.GetIntPropertyMaxValue(propNum); ++valueNum)
                    {
                        String valueName;
                        try
                        {
                            valueName = UChar.GetPropertyValueName(propNum, valueNum, nameChoice);
                            if (valueName == null)
                            {
                                if (nameChoice == NameChoice.Short) continue; // allow non-existent short names
                                if ((propNum == UProperty.Canonical_Combining_Class ||
                                        propNum == UProperty.Lead_Canonical_Combining_Class ||
                                        propNum == UProperty.Trail_Canonical_Combining_Class) &&
                                        !IsCccValue(valueNum))
                                {
                                    // Only a few of the canonical combining classes have names.
                                    // Otherwise they are just integer values.
                                    continue;
                                }
                                else
                                {
                                    throw new NullReferenceException();
                                }
                            }
                        }
                        catch (Exception e1)
                        {
                            Errln("Can't get property value name for: "
                                    + "Property (" + propNum + "): " + propName + ", "
                                    + "Value (" + valueNum + ") "
                                    + ", NameChoice: " + nameChoice + ", "
                                    + e1.GetType().Name);
                            continue;
                        }
                        Logln("Value (" + valueNum + "): " + valueName);
                        UnicodeSet testSet;
                        try
                        {
                            testSet = new UnicodeSet("[:" + propName + "=" + valueName + ":]");
                        }
                        catch (Exception e)
                        {
                            Errln("Can't create UnicodeSet for: "
                                    + "Property (" + propNum + "): " + propName + ", "
                                    + "Value (" + valueNum + "): " + valueName + ", "
                                    + e.GetType().Name);
                            continue;
                        }
                        UnicodeSet collectedErrors = new UnicodeSet();
                        for (UnicodeSetIterator it = new UnicodeSetIterator(testSet); it.Next();)
                        {
                            int value = UChar.GetIntPropertyValue(it.Codepoint, propNum);
                            if (value != valueNum)
                            {
                                collectedErrors.Add(it.Codepoint);
                            }
                        }
                        if (collectedErrors.Count != 0)
                        {
                            Errln("Property Value Differs: "
                                    + "Property (" + propNum + "): " + propName + ", "
                                    + "Value (" + valueNum + "): " + valueName + ", "
                                    + "Differing values: " + collectedErrors.ToPattern(true));
                        }
                    }
                }
            }
        }


        /**
         * Test toPattern().
         */
        [Test]
        public void TestToPattern()
        {
            // Test that toPattern() round trips with syntax characters
            // and whitespace.
            for (int i = 0; i < OTHER_TOPATTERN_TESTS.Length; ++i)
            {
                checkPat(OTHER_TOPATTERN_TESTS[i], new UnicodeSet(OTHER_TOPATTERN_TESTS[i]));
            }
            for (int i = 0; i <= 0x10FFFF; ++i)
            {
                if ((i <= 0xFF && !UChar.IsLetter(i)) || UChar.IsWhiteSpace(i))
                {
                    // check various combinations to make sure they all work.
                    if (i != 0 && !toPatternAux(i, i)) continue;
                    if (!toPatternAux(0, i)) continue;
                    if (!toPatternAux(i, 0xFFFF)) continue;
                }
            }

            // Test pattern behavior of multicharacter strings.
            UnicodeSet s = new UnicodeSet("[a-z {aa} {ab}]");
            expectToPattern(s, "[a-z{aa}{ab}]",
                    new String[] { "aa", "ab", NOT, "ac" });
            s.Add("ac");
            expectToPattern(s, "[a-z{aa}{ab}{ac}]",
                    new String[] { "aa", "ab", "ac", NOT, "xy" });

            s.ApplyPattern("[a-z {\\{l} {r\\}}]");
            expectToPattern(s, "[a-z{r\\}}{\\{l}]",
                    new String[] { "{l", "r}", NOT, "xy" });
            s.Add("[]");
            expectToPattern(s, "[a-z{\\[\\]}{r\\}}{\\{l}]",
                    new String[] { "{l", "r}", "[]", NOT, "xy" });

            s.ApplyPattern("[a-z {\u4E01\u4E02}{\\n\\r}]");
            expectToPattern(s, "[a-z{\\u000A\\u000D}{\\u4E01\\u4E02}]",
                    new String[] { "\u4E01\u4E02", "\n\r" });

            s.Clear();
            s.Add("abc");
            s.Add("abc");
            expectToPattern(s, "[{abc}]",
                    new String[] { "abc", NOT, "ab" });

            // JB#3400: For 2 character ranges prefer [ab] to [a-b]
            s.Clear();
            s.Add('a', 'b');
            expectToPattern(s, "[ab]", null);

            // Cover applyPattern, applyPropertyAlias
            s.Clear();
            s.ApplyPattern("[ab ]", true);
            expectToPattern(s, "[ab]", new String[] { "a", NOT, "ab", " " });
            s.Clear();
            s.ApplyPattern("[ab ]", false);
            expectToPattern(s, "[\\ ab]", new String[] { "a", "\u0020", NOT, "ab" });

            s.Clear();
            s.ApplyPropertyAlias("nv", "0.5");
            s.RetainAll(new UnicodeSet("[:age=6.0:]"));  // stabilize this test
            expectToPattern(s, "[\\u00BD\\u0B73\\u0D74\\u0F2A\\u2CFD\\uA831\\U00010141\\U00010175\\U00010176\\U00010E7B]", null);
            // Unicode 5.1 adds Malayalam 1/2 (\u0D74)
            // Unicode 5.2 adds U+A831 NORTH INDIC FRACTION ONE HALF and U+10E7B RUMI FRACTION ONE HALF
            // Unicode 6.0 adds U+0B73 ORIYA FRACTION ONE HALF

            s.Clear();
            s.ApplyPropertyAlias("gc", "Lu");
            // TODO expectToPattern(s, what?)

            // RemoveAllStrings()
            s.Clear();
            s.ApplyPattern("[a-z{abc}{def}]");
            expectToPattern(s, "[a-z{abc}{def}]", null);
            s.RemoveAllStrings();
            expectToPattern(s, "[a-z]", null);
        }

        static String[] OTHER_TOPATTERN_TESTS = {
                "[[:latin:]&[:greek:]]",
                "[[:latin:]-[:greek:]]",
                "[:nonspacing mark:]"
            };


        public bool toPatternAux(int start, int end)
        {
            // use Integer.toString because Utility.hex doesn't handle ints
            String source = "0x" + Convert.ToString(start, 16).ToUpperInvariant();
            if (start != end) source += "..0x" + Convert.ToString(end, 16).ToUpperInvariant();
            UnicodeSet testSet = new UnicodeSet();
            testSet.Add(start, end);
            return checkPat(source, testSet);
        }

        bool checkPat(String source, UnicodeSet testSet)
        {
            String pat = "";
            try
            {
                // What we want to make sure of is that a pattern generated
                // by toPattern(), with or without escaped unprintables, can
                // be passed back into the UnicodeSet constructor.
                String pat0 = testSet.ToPattern(true);
                if (!checkPat(source + " (escaped)", testSet, pat0)) return false;

                //String pat1 = unescapeLeniently(pat0);
                //if (!checkPat(source + " (in code)", testSet, pat1)) return false;

                String pat2 = testSet.ToPattern(false);
                if (!checkPat(source, testSet, pat2)) return false;

                //String pat3 = unescapeLeniently(pat2);
                //if (!checkPat(source + " (in code)", testSet, pat3)) return false;

                //Logln(source + " => " + pat0 + ", " + pat1 + ", " + pat2 + ", " + pat3);
                Logln(source + " => " + pat0 + ", " + pat2);
            }
            catch (Exception e)
            {
                Errln("EXCEPTION in toPattern: " + source + " => " + pat);
                return false;
            }
            return true;
        }

        bool checkPat(String source, UnicodeSet testSet, String pat)
        {
            UnicodeSet testSet2 = new UnicodeSet(pat);
            if (!testSet2.Equals(testSet))
            {
                Errln("Fail toPattern: " + source + "; " + pat + " => " +
                        testSet2.ToPattern(false) + ", expected " +
                        testSet.ToPattern(false));
                return false;
            }
            return true;
        }

        // NOTE: copied the following from Utility. There ought to be a version in there with a flag
        // that does the Java stuff

        public static int unescapeAt(String s, int[] offset16)
        {
            int c;
            int result = 0;
            int n = 0;
            int minDig = 0;
            int maxDig = 0;
            int bitsPerDigit = 4;
            int dig;
            int i;

            /* Check that offset is in range */
            int offset = offset16[0];
            int length = s.Length;
            if (offset < 0 || offset >= length)
            {
                return -1;
            }

            /* Fetch first UChar after '\\' */
            c = UTF16.CharAt(s, offset);
            offset += UTF16.GetCharCount(c);

            /* Convert hexadecimal and octal escapes */
            switch (c)
            {
                case 'u':
                    minDig = maxDig = 4;
                    break;
                /*
             case 'U':
             minDig = maxDig = 8;
             break;
             case 'x':
             minDig = 1;
             maxDig = 2;
             break;
                 */
                default:
                    dig = UChar.Digit(c, 8);
                    if (dig >= 0)
                    {
                        minDig = 1;
                        maxDig = 3;
                        n = 1; /* Already have first octal digit */
                        bitsPerDigit = 3;
                        result = dig;
                    }
                    break;
            }
            if (minDig != 0)
            {
                while (offset < length && n < maxDig)
                {
                    // TEMPORARY
                    // TODO: Restore the char32-based code when UCharacter.digit
                    // is working (Bug 66).

                    //c = UTF16.CharAt(s, offset);
                    //dig = UCharacter.Digit(c, (bitsPerDigit == 3) ? 8 : 16);
                    c = s[offset];
                    dig = J2N.Character.Digit((char)c, (bitsPerDigit == 3) ? 8 : 16);
                    if (dig < 0)
                    {
                        break;
                    }
                    result = (result << bitsPerDigit) | dig;
                    //offset += UTF16.GetCharCount(c);
                    ++offset;
                    ++n;
                }
                if (n < minDig)
                {
                    return -1;
                }
                offset16[0] = offset;
                return result;
            }

            /* Convert C-style escapes in table */
            for (i = 0; i < UNESCAPE_MAP.Length; i += 2)
            {
                if (c == UNESCAPE_MAP[i])
                {
                    offset16[0] = offset;
                    return UNESCAPE_MAP[i + 1];
                }
                else if (c < UNESCAPE_MAP[i])
                {
                    break;
                }
            }

            /* If no special forms are recognized, then consider
             * the backslash to generically escape the next character. */
            offset16[0] = offset;
            return c;
        }

        /* This map must be in ASCENDING ORDER OF THE ESCAPE CODE */
        static private readonly char[] UNESCAPE_MAP = {
                /*"   (char)0x22, (char)0x22 */
                /*'   (char)0x27, (char)0x27 */
                /*?   (char)0x3F, (char)0x3F */
                /*\   (char)0x5C, (char)0x5C */
                /*a*/ (char)0x61, (char)0x07,
                /*b*/ (char)0x62, (char)0x08,
                /*f*/ (char)0x66, (char)0x0c,
                /*n*/ (char)0x6E, (char)0x0a,
                /*r*/ (char)0x72, (char)0x0d,
                /*t*/ (char)0x74, (char)0x09,
                /*v*/ (char)0x76, (char)0x0b
            };

        /**
         * Convert all escapes in a given string using unescapeAt().
         * Leave invalid escape sequences unchanged.
         */
        public static String unescapeLeniently(String s)
        {
            StringBuffer buf = new StringBuffer();
            int[] pos = new int[1];
            for (int i = 0; i < s.Length;)
            {
                char c = s[i++];
                if (c == '\\')
                {
                    pos[0] = i;
                    int e = unescapeAt(s, pos);
                    if (e < 0)
                    {
                        buf.Append(c);
                    }
                    else
                    {
                        UTF16.Append(buf, e);
                        i = pos[0];
                    }
                }
                else
                {
                    buf.Append(c);
                }
            }
            return buf.ToString();
        }

        [Test]
        public void TestPatterns()
        {
            UnicodeSet set = new UnicodeSet();
            expectPattern(set, "[[a-m]&[d-z]&[k-y]]", "km");
            expectPattern(set, "[[a-z]-[m-y]-[d-r]]", "aczz");
            expectPattern(set, "[a\\-z]", "--aazz");
            expectPattern(set, "[-az]", "--aazz");
            expectPattern(set, "[az-]", "--aazz");
            expectPattern(set, "[[[a-z]-[aeiou]i]]", "bdfnptvz");

            // Throw in a test of complement
            set.Complement();
            String exp = '\u0000' + "aeeoouu" + (char)('z' + 1) + '\uFFFF';
            expectPairs(set, exp);
        }

        [Test]
        public void TestCategories()
        {
            int failures = 0;
            UnicodeSet set = new UnicodeSet("[:Lu:]");
            expectContainment(set, "ABC", "abc");

            // Make sure generation of L doesn't pollute cached Lu set
            // First generate L, then Lu
            // not used int TOP = 0x200; // Don't need to go over the whole range:
            set = new UnicodeSet("[:L:]");
            for (int i = 0; i < 0x200; ++i)
            {
                bool l = UChar.IsLetter(i);
                if (l != set.Contains((char)i))
                {
                    Errln("FAIL: L contains " + (char)i + " = " +
                            set.Contains((char)i));
                    if (++failures == 10) break;
                }
            }

            set = new UnicodeSet("[:Lu:]");
            for (int i = 0; i < 0x200; ++i)
            {
                bool lu = (UChar.GetUnicodeCategory(i) == UUnicodeCategory.UppercaseLetter);
                if (lu != set.Contains((char)i))
                {
                    Errln("FAIL: Lu contains " + (char)i + " = " +
                            set.Contains((char)i));
                    if (++failures == 20) break;
                }
            }
        }

        [Test]
        public void TestAddRemove()
        {
            UnicodeSet set = new UnicodeSet();
            set.Add('a', 'z');
            expectPairs(set, "az");
            set.Remove('m', 'p');
            expectPairs(set, "alqz");
            set.Remove('e', 'g');
            expectPairs(set, "adhlqz");
            set.Remove('d', 'i');
            expectPairs(set, "acjlqz");
            set.Remove('c', 'r');
            expectPairs(set, "absz");
            set.Add('f', 'q');
            expectPairs(set, "abfqsz");
            set.Remove('a', 'g');
            expectPairs(set, "hqsz");
            set.Remove('a', 'z');
            expectPairs(set, "");

            // Try removing an entire set from another set
            expectPattern(set, "[c-x]", "cx");
            UnicodeSet set2 = new UnicodeSet();
            expectPattern(set2, "[f-ky-za-bc[vw]]", "acfkvwyz");
            set.RemoveAll(set2);
            expectPairs(set, "deluxx");

            // Try adding an entire set to another set
            expectPattern(set, "[jackiemclean]", "aacceein");
            expectPattern(set2, "[hitoshinamekatajamesanderson]", "aadehkmort");
            set.AddAll(set2);
            expectPairs(set, "aacehort");

            // Test commutativity
            expectPattern(set, "[hitoshinamekatajamesanderson]", "aadehkmort");
            expectPattern(set2, "[jackiemclean]", "aacceein");
            set.AddAll(set2);
            expectPairs(set, "aacehort");
        }

        /**
         * Make sure minimal representation is maintained.
         */
        [Test]
        public void TestMinimalRep()
        {
            // This is pretty thoroughly tested by checkCanonicalRep()
            // run against the exhaustive operation results.  Use the code
            // here for debugging specific spot problems.

            // 1 overlap against 2
            UnicodeSet set = new UnicodeSet("[h-km-q]");
            UnicodeSet set2 = new UnicodeSet("[i-o]");
            set.AddAll(set2);
            expectPairs(set, "hq");
            // right
            set.ApplyPattern("[a-m]");
            set2.ApplyPattern("[e-o]");
            set.AddAll(set2);
            expectPairs(set, "ao");
            // left
            set.ApplyPattern("[e-o]");
            set2.ApplyPattern("[a-m]");
            set.AddAll(set2);
            expectPairs(set, "ao");
            // 1 overlap against 3
            set.ApplyPattern("[a-eg-mo-w]");
            set2.ApplyPattern("[d-q]");
            set.AddAll(set2);
            expectPairs(set, "aw");
        }

        [Test]
        public void TestAPI()
        {
            // default ct
            UnicodeSet set = new UnicodeSet();
            if (!set.IsEmpty || set.RangeCount != 0)
            {
                Errln("FAIL, set should be empty but isn't: " +
                        set);
            }

            // clear(), isEmpty()
            set.Add('a');
            if (set.IsEmpty)
            {
                Errln("FAIL, set shouldn't be empty but is: " +
                        set);
            }
            set.Clear();
            if (!set.IsEmpty)
            {
                Errln("FAIL, set should be empty but isn't: " +
                        set);
            }

            // Count
            set.Clear();
            if (set.Count != 0)
            {
                Errln("FAIL, size should be 0, but is " + set.Count +
                        ": " + set);
            }
            set.Add('a');
            if (set.Count != 1)
            {
                Errln("FAIL, size should be 1, but is " + set.Count +
                        ": " + set);
            }
            set.Add('1', '9');
            if (set.Count != 10)
            {
                Errln("FAIL, size should be 10, but is " + set.Count +
                        ": " + set);
            }
            set.Clear();
            set.Complement();
            if (set.Count != 0x110000)
            {
                Errln("FAIL, size should be 0x110000, but is" + set.Count);
            }

            // contains(first, last)
            set.Clear();
            set.ApplyPattern("[A-Y 1-8 b-d l-y]");
            for (int i = 0; i < set.RangeCount; ++i)
            {
                int a2 = set.GetRangeStart(i);
                int b2 = set.GetRangeEnd(i);
                if (!set.Contains(a2, b2))
                {
                    Errln("FAIL, should contain " + (char)a2 + '-' + (char)b2 +
                            " but doesn't: " + set);
                }
                if (set.Contains((char)(a2 - 1), b2))
                {
                    Errln("FAIL, shouldn't contain " +
                            (char)(a2 - 1) + '-' + (char)b2 +
                            " but does: " + set);
                }
                if (set.Contains(a2, (char)(b2 + 1)))
                {
                    Errln("FAIL, shouldn't contain " +
                            (char)a2 + '-' + (char)(b2 + 1) +
                            " but does: " + set);
                }
            }

            // Ported InversionList test.
            UnicodeSet a = new UnicodeSet((char)3, (char)10);
            UnicodeSet b = new UnicodeSet((char)7, (char)15);
            UnicodeSet c = new UnicodeSet();

            Logln("a [3-10]: " + a);
            Logln("b [7-15]: " + b);
            c.Set(a); c.AddAll(b);
            UnicodeSet exp = new UnicodeSet((char)3, (char)15);
            if (c.Equals(exp))
            {
                Logln("c.Set(a).Add(b): " + c);
            }
            else
            {
                Errln("FAIL: c.Set(a).Add(b) = " + c + ", expect " + exp);
            }
            c.Complement();
            exp.Set((char)0, (char)2);
            exp.Add((char)16, UnicodeSet.MaxValue);
            if (c.Equals(exp))
            {
                Logln("c.Complement(): " + c);
            }
            else
            {
                Errln(Utility.Escape("FAIL: c.Complement() = " + c + ", expect " + exp));
            }
            c.Complement();
            exp.Set((char)3, (char)15);
            if (c.Equals(exp))
            {
                Logln("c.Complement(): " + c);
            }
            else
            {
                Errln("FAIL: c.Complement() = " + c + ", expect " + exp);
            }
            c.Set(a); c.ComplementAll(b);
            exp.Set((char)3, (char)6);
            exp.Add((char)11, (char)15);
            if (c.Equals(exp))
            {
                Logln("c.Set(a).Complement(b): " + c);
            }
            else
            {
                Errln("FAIL: c.Set(a).Complement(b) = " + c + ", expect " + exp);
            }

            exp.Set(c);
            c = bitsToSet(setToBits(c));
            if (c.Equals(exp))
            {
                Logln("bitsToSet(setToBits(c)): " + c);
            }
            else
            {
                Errln("FAIL: bitsToSet(setToBits(c)) = " + c + ", expect " + exp);
            }

            // Additional tests for coverage JB#2118
            //UnicodeSet::complement(class UnicodeString const &)
            //UnicodeSet::complementAll(class UnicodeString const &)
            //UnicodeSet::containsNone(class UnicodeSet const &)
            //UnicodeSet::containsNone(long,long)
            //UnicodeSet::containsSome(class UnicodeSet const &)
            //UnicodeSet::containsSome(long,long)
            //UnicodeSet::removeAll(class UnicodeString const &)
            //UnicodeSet::retain(long)
            //UnicodeSet::retainAll(class UnicodeString const &)
            //UnicodeSet::serialize(unsigned short *,long,enum UErrorCode &)
            //UnicodeSetIterator::getString(void)
            set.Clear();
            set.Complement("ab");
            exp.ApplyPattern("[{ab}]");
            if (!set.Equals(exp)) { Errln("FAIL: complement(\"ab\")"); return; }

            UnicodeSetIterator iset = new UnicodeSetIterator(set);
            if (!iset.Next() || iset.Codepoint != UnicodeSetIterator.IsString)
            {
                Errln("FAIL: UnicodeSetIterator.next/IS_STRING");
            }
            else if (!iset.String.Equals("ab"))
            {
                Errln("FAIL: UnicodeSetIterator.string");
            }

            set.Add((char)0x61, (char)0x7A);
            set.ComplementAll("alan");
            exp.ApplyPattern("[{ab}b-kmo-z]");
            if (!set.Equals(exp)) { Errln("FAIL: complementAll(\"alan\")"); return; }

            exp.ApplyPattern("[a-z]");
            if (set.ContainsNone(exp)) { Errln("FAIL: containsNone(UnicodeSet)"); }
            if (!set.ContainsSome(exp)) { Errln("FAIL: containsSome(UnicodeSet)"); }
            exp.ApplyPattern("[aln]");
            if (!set.ContainsNone(exp)) { Errln("FAIL: containsNone(UnicodeSet)"); }
            if (set.ContainsSome(exp)) { Errln("FAIL: containsSome(UnicodeSet)"); }

            if (set.ContainsNone((char)0x61, (char)0x7A))
            {
                Errln("FAIL: containsNone(char, char)");
            }
            if (!set.ContainsSome((char)0x61, (char)0x7A))
            {
                Errln("FAIL: containsSome(char, char)");
            }
            if (!set.ContainsNone((char)0x41, (char)0x5A))
            {
                Errln("FAIL: containsNone(char, char)");
            }
            if (set.ContainsSome((char)0x41, (char)0x5A))
            {
                Errln("FAIL: containsSome(char, char)");
            }

            set.RemoveAll("liu");
            exp.ApplyPattern("[{ab}b-hj-kmo-tv-z]");
            if (!set.Equals(exp)) { Errln("FAIL: removeAll(\"liu\")"); return; }

            set.RetainAll("star");
            exp.ApplyPattern("[rst]");
            if (!set.Equals(exp)) { Errln("FAIL: retainAll(\"star\")"); return; }

            set.Retain((char)0x73);
            exp.ApplyPattern("[s]");
            if (!set.Equals(exp)) { Errln("FAIL: retain('s')"); return; }

            // ICU 2.6 coverage tests
            // public final UnicodeSet retain(String s);
            // public final UnicodeSet remove(int c);
            // public final UnicodeSet remove(String s);
            // public int hashCode();
            set.ApplyPattern("[a-z{ab}{cd}]");
            set.Retain("cd");
            exp.ApplyPattern("[{cd}]");
            if (!set.Equals(exp)) { Errln("FAIL: retain(\"cd\")"); return; }

            set.ApplyPattern("[a-z{ab}{cd}]");
            set.Remove((char)0x63);
            exp.ApplyPattern("[abd-z{ab}{cd}]");
            if (!set.Equals(exp)) { Errln("FAIL: remove('c')"); return; }

            set.Remove("cd");
            exp.ApplyPattern("[abd-z{ab}]");
            if (!set.Equals(exp)) { Errln("FAIL: remove(\"cd\")"); return; }

            if (set.GetHashCode() != exp.GetHashCode())
            {
                Errln("FAIL: hashCode() unequal");
            }
            exp.Clear();
            if (set.GetHashCode() == exp.GetHashCode())
            {
                Errln("FAIL: hashCode() equal");
            }

            {
                //Cover addAll(Collection) and addAllTo(Collection)
                //  Seems that there is a bug in addAll(Collection) operation
                //    Ram also add a similar test to UtilityTest.java
                Logln("Testing addAll(Collection) ... ");
                String[] array = { "a", "b", "c", "de" };
                List<string> list = array.ToList();
                ISet<string> aset = new HashSet<string>(list);
                Logln(" *** The source set's size is: " + aset.Count);

                set.Clear();
                set.AddAll(aset);
                if (set.Count != aset.Count)
                {
                    Errln("FAIL: After addAll, the UnicodeSet size expected " + aset.Count +
                            ", " + set.Count + " seen instead!");
                }
                else
                {
                    Logln("OK: After addAll, the UnicodeSet size got " + set.Count);
                }

                List<string> list2 = new List<string>();
                set.AddAllTo(list2);

                //verify the result
                Log(" *** The elements are: ");
                String s = set.ToPattern(true);
                Logln(s);
                foreach (var item in list2)
                {
                    Log(item.ToString() + "  ");
                }
                Logln("");  // a new line
            }

        }

        [Test]
        public void TestStrings()
        {
            //  Object[][] testList = {
            //  {I_EQUALS,  UnicodeSet.fromAll("abc"),
            //  new UnicodeSet("[a-c]")},
            //
            //  {I_EQUALS,  UnicodeSet.from("ch").Add('a','z').Add("ll"),
            //  new UnicodeSet("[{ll}{ch}a-z]")},
            //
            //  {I_EQUALS,  UnicodeSet.from("ab}c"),
            //  new UnicodeSet("[{ab\\}c}]")},
            //
            //  {I_EQUALS,  new UnicodeSet('a','z').Add('A', 'Z').Retain('M','m').Complement('X'),
            //  new UnicodeSet("[[a-zA-Z]&[M-m]-[X]]")},
            //  };
            //
            //  for (int i = 0; i < testList.Length; ++i) {
            //  expectRelation(testList[i][0], testList[i][1], testList[i][2], "(" + i + ")");
            //  }

            UnicodeSet[][] testList = {
                    new UnicodeSet[] {UnicodeSet.FromAll("abc"), new UnicodeSet("[a-c]")},

                    new UnicodeSet[]  {UnicodeSet.From("ch").Add('a','z').Add("ll"), new UnicodeSet("[{ll}{ch}a-z]")},

                    new UnicodeSet[]  {UnicodeSet.From("ab}c"), new UnicodeSet("[{ab\\}c}]")},

                    new UnicodeSet[]  {new UnicodeSet('a','z').Add('A', 'Z').Retain('M','m').Complement('X'), new UnicodeSet("[[a-zA-Z]&[M-m]-[X]]")},
                };

            for (int i = 0; i < testList.Length; ++i)
            {
                if (!testList[i][0].Equals(testList[i][1]))
                {
                    Errln("FAIL: sets unequal; see source code (" + i + ")");
                }
            }
        }

        static readonly int?
            I_ANY = new int?(SortedSetRelation.ANY),
            I_CONTAINS = new int?(SortedSetRelation.CONTAINS),
            I_DISJOINT = new int?(SortedSetRelation.DISJOINT),
            I_NO_B = new int?(SortedSetRelation.NO_B),
            I_ISCONTAINED = new int?(SortedSetRelation.ISCONTAINED),
            I_EQUALS = new int?(SortedSetRelation.EQUALS),
            I_NO_A = new int?(SortedSetRelation.NO_A),
            I_NONE = new int?(SortedSetRelation.NONE);

        [Test]
        public void TestSetRelation()
        {

            String[] choices = { "a", "b", "cd", "ef" };
            int limit = 1 << choices.Length;

            SortedSet<string> iset = new SortedSet<string>();
            SortedSet<string> jset = new SortedSet<string>();

            for (int i = 0; i < limit; ++i)
            {
                pick(i, choices, iset);
                for (int j = 0; j < limit; ++j)
                {
                    pick(j, choices, jset);
                    checkSetRelation(iset, jset, "(" + i + ")");
                }
            }
        }

        [Test]
        public void TestSetSpeed()
        {
            // skip unless verbose
            if (!IsVerbose()) return;

            SetSpeed2(100);
            SetSpeed2(1000);
        }

        public void SetSpeed2(int size)
        {

            SortedSet<int> iset = new SortedSet<int>();
            SortedSet<int> jset = new SortedSet<int>();

            for (int i = 0; i < size * 2; i += 2)
            { // only even values
                iset.Add(i);
                jset.Add(i);
            }

            int iterations = 1000000 / size;

            Logln("Timing comparison of Java vs Utility");
            Logln("For about " + size + " objects that are almost all the same.");

            CheckSpeed(iset, jset, "when a = b", iterations);

            iset.Add(size + 1);    // add odd value in middle

            CheckSpeed(iset, jset, "when a contains b", iterations);
            CheckSpeed(jset, iset, "when b contains a", iterations);

            jset.Add(size - 1);    // add different odd value in middle

            CheckSpeed(jset, iset, "when a, b are disjoint", iterations);
        }

        void CheckSpeed<T>(SortedSet<T> iset, SortedSet<T> jset, String message, int iterations) where T : IComparable<T>
        {
            CheckSpeed2(iset, jset, message, iterations);
            CheckSpeed3(iset, jset, message, iterations);
        }

        void CheckSpeed2<T>(SortedSet<T> iset, SortedSet<T> jset, String message, int iterations) where T : IComparable<T>
        {
            bool x;
            bool y;

            // make sure code is loaded:
            //x = iset.ContainsAll(jset);
            x = iset.IsSupersetOf(jset);
            y = SortedSetRelation.HasRelation(iset, SortedSetFilter.Contains, jset);
            if (x != y) Errln("FAIL contains comparison");

            double start = Time.CurrentTimeMilliseconds();
            for (int i = 0; i < iterations; ++i)
            {
                //x |= iset.ContainsAll(jset);
                x |= iset.IsSupersetOf(jset);
            }
            double middle = Time.CurrentTimeMilliseconds();
            for (int i = 0; i < iterations; ++i)
            {
                y |= SortedSetRelation.HasRelation(iset, SortedSetFilter.Contains, jset);
            }
            double end = Time.CurrentTimeMilliseconds();

            double jtime = (middle - start) / iterations;
            double utime = (end - middle) / iterations;

            //NumberFormat nf = NumberFormat.getPercentInstance();
            Logln("Test contains: " + message + ": Java: " + jtime
                    + ", Utility: " + utime + ", u:j: " + string.Format("{0:p}", utime / jtime)); //nf.format(utime / jtime));
        }

        void CheckSpeed3<T>(SortedSet<T> iset, SortedSet<T> jset, String message, int iterations) where T : IComparable<T>
        {
            bool x;
            bool y;

            // make sure code is loaded:
            x = iset.Equals(jset);
            y = SortedSetRelation.HasRelation(iset, SortedSetFilter.Equals, jset);
            if (x != y) Errln("FAIL equality comparison");


            double start = Time.CurrentTimeMilliseconds();
            for (int i = 0; i < iterations; ++i)
            {
                x |= iset.Equals(jset);
            }
            double middle = Time.CurrentTimeMilliseconds();
            for (int i = 0; i < iterations; ++i)
            {
                y |= SortedSetRelation.HasRelation(iset, SortedSetFilter.Equals, jset);
            }
            double end = Time.CurrentTimeMilliseconds();

            double jtime = (middle - start) / iterations;
            double utime = (end - middle) / iterations;

            //NumberFormat nf = NumberFormat.getPercentInstance();
            Logln("Test equals:   " + message + ": Java: " + jtime
                    + ", Utility: " + utime + ", u:j: " + string.Format("{0:p}", utime / jtime)); //nf.format(utime / jtime));
        }

        void pick<T>(int bits, T[] examples, SortedSet<T> output)
        {
            output.Clear();
            for (int k = 0; k < 32; ++k)
            {
                if (((1 << k) & bits) != 0) output.Add(examples[k]);
            }
        }

        public static readonly String[] RELATION_NAME = {
        "both-are-null",
        "a-is-null",
        "equals",
        "is-contained-in",
        "b-is-null",
        "is-disjoint_with",
        "contains",
        "any", };

        bool DumbHasRelation<T>(ICollection<T> A, int filter, ICollection<T> B)
        {
            ISet<T> ab = new SortedSet<T>(A);
            //ab.RetainAll(B);
            ab.IntersectWith(B);
            if (ab.Count > 0 && (filter & SortedSetRelation.A_AND_B) == 0) return false;

            // A - B size == A.size - A&B.size
            if (A.Count > ab.Count && (filter & SortedSetRelation.A_NOT_B) == 0) return false;

            // B - A size == B.size - A&B.size
            if (B.Count > ab.Count && (filter & SortedSetRelation.B_NOT_A) == 0) return false;


            return true;
        }

        void checkSetRelation<T>(SortedSet<T> a, SortedSet<T> b, String message) where T : IComparable<T>
        {
            for (int i = 0; i < 8; ++i)
            {

                bool hasRelation = SortedSetRelation.HasRelation(a, (SortedSetFilter)i, b);
                bool dumbHasRelation = DumbHasRelation(a, i, b);

                Logln(message + " " + hasRelation + ":\t" + a + "\t" + RELATION_NAME[i] + "\t" + b);

                if (hasRelation != dumbHasRelation)
                {
                    Errln("FAIL: " +
                            message + " " + dumbHasRelation + ":\t" + a + "\t" + RELATION_NAME[i] + "\t" + b);
                }
            }
            Logln("");
        }

        /**
         * Test the [:Latin:] syntax.
         */
        [Test]
        public void TestScriptSet()
        {

            expectContainment("[:Latin:]", "aA", CharsToUnicodeString("\\u0391\\u03B1"));

            expectContainment("[:Greek:]", CharsToUnicodeString("\\u0391\\u03B1"), "aA");

            /* Jitterbug 1423 */
            expectContainment("[[:Common:][:Inherited:]]", CharsToUnicodeString("\\U00003099\\U0001D169\\u0000"), "aA");

        }

        /**
         * Test the [:Latin:] syntax.
         */
        [Test]
        public void TestPropertySet()
        {
            String[] DATA = {
                // Pattern, Chars IN, Chars NOT in

                "[:Latin:]",
                "aA",
                "\u0391\u03B1",

                "[\\p{Greek}]",
                "\u0391\u03B1",
                "aA",

                "\\P{ GENERAL Category = upper case letter }",
                "abc",
                "ABC",

                // Combining class: @since ICU 2.2
                // Check both symbolic and numeric
                "\\p{ccc=Nukta}",
                "\u0ABC",
                "abc",

                "\\p{Canonical Combining Class = 11}",
                "\u05B1",
                "\u05B2",

                "[:c c c = iota subscript :]",
                "\u0345",
                "xyz",

                // Bidi class: @since ICU 2.2
                "\\p{bidiclass=lefttoright}",
                "abc",
                "\u0671\u0672",

                // Binary properties: @since ICU 2.2
                "\\p{ideographic}",
                "\u4E0A",
                "x",

                "[:math=false:]",
                "q)*(", // )(and * were removed from math in Unicode 4.0.1
                "+<>^",

                // JB#1767 \N{}, \p{ASCII}
                "[:Ascii:]",
                "abc\u0000\u007F",
                "\u0080\u4E00",

                "[\\N{ latin small letter  a  }[:name= latin small letter z:]]",
                "az",
                "qrs",

                // JB#2015
                "[:any:]",
                "a\\U0010FFFF",
                "",

                "[:nv=0.5:]",
                "\u00BD\u0F2A",
                "\u00BC",

                // JB#2653: Age
                "[:Age=1.1:]",
                "\u03D6", // 1.1
                "\u03D8\u03D9", // 3.2

                "[:Age=3.1:]",
                "\\u1800\\u3400\\U0002f800",
                "\\u0220\\u034f\\u30ff\\u33ff\\ufe73\\U00010000\\U00050000",

                // JB#2350: Case_Sensitive
                "[:Case Sensitive:]",
                "A\u1FFC\\U00010410",
                ";\u00B4\\U00010500",


                // Regex compatibility test
                "[-b]", // leading '-' is literal
                "-b",
                "ac",

                "[^-b]", // leading '-' is literal
                "ac",
                "-b",

                "[b-]", // trailing '-' is literal
                "-b",
                "ac",

                "[^b-]", // trailing '-' is literal
                "ac",
                "-b",

                "[a-b-]", // trailing '-' is literal
                "ab-",
                "c=",

                "[[a-q]&[p-z]-]", // trailing '-' is literal
                "pq-",
                "or=",

                "[\\s|\\)|:|$|\\>]", // from regex tests
                "s|):$>",
                "\\abc",

                "[\uDC00cd]", // JB#2906: isolated trail at start
                "cd\uDC00",
                "ab\uD800\\U00010000",

                "[ab\uD800]", // JB#2906: isolated trail at start
                "ab\uD800",
                "cd\uDC00\\U00010000",

                "[ab\uD800cd]", // JB#2906: isolated lead in middle
                "abcd\uD800",
                "ef\uDC00\\U00010000",

                "[ab\uDC00cd]", // JB#2906: isolated trail in middle
                "abcd\uDC00",
                "ef\uD800\\U00010000",

                "[:^lccc=0:]", // Lead canonical class
                "\u0300\u0301",
                "abcd\u00c0\u00c5",

                "[:^tccc=0:]", // Trail canonical class
                "\u0300\u0301\u00c0\u00c5",
                "abcd",

                "[[:^lccc=0:][:^tccc=0:]]", // Lead and trail canonical class
                "\u0300\u0301\u00c0\u00c5",
                "abcd",

                "[[:^lccc=0:]-[:^tccc=0:]]", // Stuff that starts with an accent but ends with a base (none right now)
                "",
                "abcd\u0300\u0301\u00c0\u00c5",

                "[[:ccc=0:]-[:lccc=0:]-[:tccc=0:]]", // Weirdos. Complete canonical class is zero, but both lead and trail are not
                "\u0F73\u0F75\u0F81",
                "abcd\u0300\u0301\u00c0\u00c5",

                "[:Assigned:]",
                "A\\uE000\\uF8FF\\uFDC7\\U00010000\\U0010FFFD",
                "\\u0888\\uFDD3\\uFFFE\\U00050005",

                // Script_Extensions, new in Unicode 6.0
                "[:scx=Arab:]",
                "\\u061E\\u061F\\u0620\\u0621\\u063F\\u0640\\u0650\\u065E\\uFDF1\\uFDF2\\uFDF3",
                "\\u061D\\uFDEF\\uFDFE",

                // U+FDF2 has Script=Arabic and also Arab in its Script_Extensions,
                // so scx-sc is missing U+FDF2.
                "[[:Script_Extensions=Arabic:]-[:Arab:]]",
                "\\u0640\\u064B\\u0650\\u0655",
                "\\uFDF2"
        };

            for (int i = 0; i < DATA.Length; i += 3)
            {
                expectContainment(DATA[i], DATA[i + 1], DATA[i + 2]);
            }
        }

        [Test]
        public void TestUnicodeSetStrings()
        {
            UnicodeSet uset = new UnicodeSet("[a{bc}{cd}pqr\u0000]");
            Logln(uset + " ~ " + uset.GetRegexEquivalent());
            string[][] testStrings = {
                new string[] {"x", "none"},
                new string[] {"bc", "all"},
                new string[] {"cdbca", "all"},
                new string[] {"a", "all"},
                new string[] {"bcx", "some"},
                new string[] {"ab", "some"},
                new string[] {"acb", "some"},
                new string[] {"bcda", "some"},
                new string[] {"dccbx", "none"},
        };
            for (int i = 0; i < testStrings.Length; ++i)
            {
                check(uset, testStrings[i][0], testStrings[i][1]);
            }
        }


        private void check(UnicodeSet uset, String @string, String desiredStatus)
        {
            bool shouldContainAll = desiredStatus.Equals("all");
            bool shouldContainNone = desiredStatus.Equals("none");
            if (uset.ContainsAll(@string) != shouldContainAll)
            {
                Errln("containsAll " + @string + " should be " + shouldContainAll);
            }
            else
            {
                Logln("containsAll " + @string + " = " + shouldContainAll);
            }
            if (uset.ContainsNone(@string) != shouldContainNone)
            {
                Errln("containsNone " + @string + " should be " + shouldContainNone);
            }
            else
            {
                Logln("containsNone " + @string + " = " + shouldContainNone);
            }
        }

        /**
         * Test cloning of UnicodeSet
         */
        [Test]
        public void TestClone()
        {
            UnicodeSet s = new UnicodeSet("[abcxyz]");
            UnicodeSet t = (UnicodeSet)s.Clone();
            expectContainment(t, "abc", "def");
        }

        /**
         * Test the indexOf() and charAt() methods.
         */
        [Test]
        public void TestIndexOf()
        {
            UnicodeSet set = new UnicodeSet("[a-cx-y3578]");
            for (int i = 0; i < set.Count; ++i)
            {
                int c1 = set[i];
                if (set.IndexOf(c1) != i)
                {
                    Errln("FAIL: charAt(" + i + ") = " + c1 +
                            " => indexOf() => " + set.IndexOf(c1));
                }
            }
            int c = set[set.Count];
            if (c != -1)
            {
                Errln("FAIL: charAt(<out of range>) = " +
                        Utility.Escape(c.ToString(CultureInfo.InvariantCulture)));
            }
            int j = set.IndexOf('q');
            if (j != -1)
            {
                Errln("FAIL: indexOf('q') = " + j);
            }
        }

        [Test]
        public void TestContainsString()
        {
            UnicodeSet x = new UnicodeSet("[a{bc}]");
            if (x.Contains("abc")) Errln("FAIL");
        }

        [Test]
        public void TestExhaustive()
        {
            // exhaustive tests. Simulate UnicodeSets with integers.
            // That gives us very solid tests (except for large memory tests).

            char limit = (char)128;

            for (char i = (char)0; i < limit; ++i)
            {
                Logln("Testing " + i + ", " + bitsToSet(i));
                _testComplement(i);

                // AS LONG AS WE ARE HERE, check roundtrip
                checkRoundTrip(bitsToSet(i));

                for (char j = (char)0; j < limit; ++j)
                {
                    _testAdd(i, j);
                    _testXor(i, j);
                    _testRetain(i, j);
                    _testRemove(i, j);
                }
            }
        }

        /**
         * Make sure each script name and abbreviated name can be used
         * to construct a UnicodeSet.
         */
        [Test]
        public void TestScriptNames()
        {
            for (int i = 0; i < UScript.CodeLimit; ++i)
            {
                for (int j = 0; j < 2; ++j)
                {
                    String pat = "";
                    try
                    {
                        String name =
                                (j == 0) ? UScript.GetName(i) : UScript.GetShortName(i);
                        pat = "[:" + name + ":]";
                        UnicodeSet set = new UnicodeSet(pat);
                        Logln("Ok: " + pat + " -> " + set.ToPattern(false));
                    }
                    catch (ArgumentException e)
                    {
                        if (pat.Length == 0)
                        {
                            Errln("FAIL (in UScript): No name for script " + i);
                        }
                        else
                        {
                            Errln("FAIL: Couldn't create " + pat);
                        }
                    }
                }
            }
        }

        /**
         * Test closure API.
         */
        [Test]
        public void TestCloseOver()
        {
            String CASE = (UnicodeSet.Case).ToString();
            String[] DATA = {
                // selector, input, output
                CASE,
                "[aq\u00DF{Bc}{bC}{Fi}]",
                "[aAqQ\u00DF\u1E9E\uFB01{ss}{bc}{fi}]", // U+1E9E LATIN CAPITAL LETTER SHARP S is new in Unicode 5.1

                CASE,
                "[\u01F1]", // 'DZ'
                "[\u01F1\u01F2\u01F3]",

                CASE,
                "[\u1FB4]",
                "[\u1FB4{\u03AC\u03B9}]",

                CASE,
                "[{F\uFB01}]",
                "[\uFB03{ffi}]",

                CASE,
                "[a-z]","[A-Za-z\u017F\u212A]",
                CASE,
                "[abc]","[A-Ca-c]",
                CASE,
                "[ABC]","[A-Ca-c]",
            };

            UnicodeSet s = new UnicodeSet();
            UnicodeSet t = new UnicodeSet();
            for (int i = 0; i < DATA.Length; i += 3)
            {
                PatternOptions selector = (PatternOptions)Enum.Parse(typeof(PatternOptions), DATA[i]);
                String pat = DATA[i + 1];
                String exp = DATA[i + 2];
                s.ApplyPattern(pat);
                s.CloseOver(selector);
                t.ApplyPattern(exp);
                if (s.Equals(t))
                {
                    Logln("Ok: " + pat + ".CloseOver(" + selector + ") => " + exp);
                }
                else
                {
                    Errln("FAIL: " + pat + ".CloseOver(" + selector + ") => " +
                            s.ToPattern(true) + ", expected " + exp);
                }
            }

            // Test the pattern API
            s.ApplyPattern("[abc]", UnicodeSet.Case);
            expectContainment(s, "abcABC", "defDEF");
            s = new UnicodeSet("[^abc]", UnicodeSet.Case);
            expectContainment(s, "defDEF", "abcABC");
        }

        [Test]
        public void TestEscapePattern()
        {
            // The following pattern must contain at least one range "c-d"
            // where c or d is a Pattern_White_Space.
            String pattern =
                    "[\\uFEFF \\u200E-\\u20FF \\uFFF9-\\uFFFC \\U0001D173-\\U0001D17A \\U000F0000-\\U000FFFFD ]";
            String exp =
                    "[\\u200E-\\u20FF\\uFEFF\\uFFF9-\\uFFFC\\U0001D173-\\U0001D17A\\U000F0000-\\U000FFFFD]";
            // We test this with two passes; in the second pass we
            // pre-unescape the pattern.  Since U+200E is Pattern_White_Space,
            // this fails -- which is what we expect.
            for (int pass = 1; pass <= 2; ++pass)
            {
                String pat = pattern;
                if (pass == 2)
                {
                    pat = Utility.Unescape(pat);
                }
                // Pattern is only good for pass 1
                bool isPatternValid = (pass == 1);

                UnicodeSet set = null;
                try
                {
                    set = new UnicodeSet(pat);
                }
                catch (ArgumentException e)
                {
                    set = null;
                }
                if ((set != null) != isPatternValid)
                {
                    Errln("FAIL: applyPattern(" +
                            Utility.Escape(pat) + ") => " + set);
                    continue;
                }
                if (set == null)
                {
                    continue;
                }
                if (set.Contains((char)0x0644))
                {
                    Errln("FAIL: " + Utility.Escape(pat) + " contains(U+0664)");
                }

                String newpat = set.ToPattern(true);
                if (newpat.Equals(exp))
                {
                    Logln(Utility.Escape(pat) + " => " + newpat);
                }
                else
                {
                    Errln("FAIL: " + Utility.Escape(pat) + " => " + newpat);
                }

                for (int i = 0; i < set.RangeCount; ++i)
                {
                    StringBuffer str = new StringBuffer("Range ");
                    str.Append((char)(0x30 + i))
                    .Append(": ");
                    UTF16.Append(str, set.GetRangeStart(i));
                    str.Append(" - ");
                    UTF16.Append(str, set.GetRangeEnd(i));
                    String s = Utility.Escape(str.ToString() + " (" + set.GetRangeStart(i) + " - " +
                            set.GetRangeEnd(i) + ")");
                    if (set.GetRangeStart(i) < 0)
                    {
                        Errln("FAIL: " + s);
                    }
                    else
                    {
                        Logln(s);
                    }
                }
            }
        }

        [Test]
        public void TestSymbolTable()
        {
            // Multiple test cases can be set up here.  Each test case
            // is terminated by null:
            // var, value, var, value,..., input pat., exp. output pat., null
            String[] DATA = {
                    "us", "a-z", "[0-1$us]", "[0-1a-z]", null,
                    "us", "[a-z]", "[0-1$us]", "[0-1[a-z]]", null,
                    "us", "\\[a\\-z\\]", "[0-1$us]", "[-01\\[\\]az]", null
            };

            for (int i = 0; i < DATA.Length; ++i)
            {
                TokenSymbolTable sym = new TokenSymbolTable();

                // Set up variables
                while (DATA[i + 2] != null)
                {
                    sym.Add(DATA[i], DATA[i + 1]);
                    i += 2;
                }

                // Input pattern and expected output pattern
                String inpat = DATA[i], exppat = DATA[i + 1];
                i += 2;

                ParsePosition pos = new ParsePosition(0);
                UnicodeSet us = new UnicodeSet(inpat, pos, sym);

                // results
                if (pos.Index != inpat.Length)
                {
                    Errln("Failed to read to end of string \""
                            + inpat + "\": read to "
                            + pos.Index + ", length is "
                            + inpat.Length);
                }

                UnicodeSet us2 = new UnicodeSet(exppat);
                if (!us.Equals(us2))
                {
                    Errln("Failed, got " + us + ", expected " + us2);
                }
                else
                {
                    Logln("Ok, got " + us);
                }

                //cover Unicode(String,ParsePosition,SymbolTable,int)
                ParsePosition inpos = new ParsePosition(0);
                UnicodeSet inSet = new UnicodeSet(inpat, inpos, sym, UnicodeSet.IgnoreSpace);
                UnicodeSet expSet = new UnicodeSet(exppat);
                if (!inSet.Equals(expSet))
                {
                    Errln("FAIL: Failed, got " + inSet + ", expected " + expSet);
                }
                else
                {
                    Logln("OK: got " + inSet);
                }
            }
        }

        /**
         * Test that Posix style character classes [:digit:], etc.
         *   have the Unicode definitions from TR 18.
         */
        [Test]
        public void TestPosixClasses()
        {
            expectEqual("POSIX alpha", "[:alpha:]", "\\p{Alphabetic}");
            expectEqual("POSIX lower", "[:lower:]", "\\p{lowercase}");
            expectEqual("POSIX upper", "[:upper:]", "\\p{Uppercase}");
            expectEqual("POSIX punct", "[:punct:]", "\\p{gc=Punctuation}");
            expectEqual("POSIX digit", "[:digit:]", "\\p{gc=DecimalNumber}");
            expectEqual("POSIX xdigit", "[:xdigit:]", "[\\p{DecimalNumber}\\p{HexDigit}]");
            expectEqual("POSIX alnum", "[:alnum:]", "[\\p{Alphabetic}\\p{DecimalNumber}]");
            expectEqual("POSIX space", "[:space:]", "\\p{Whitespace}");
            expectEqual("POSIX blank", "[:blank:]", "[\\p{Whitespace}-[\\u000a\\u000B\\u000c\\u000d\\u0085\\p{LineSeparator}\\p{ParagraphSeparator}]]");
            expectEqual("POSIX cntrl", "[:cntrl:]", "\\p{Control}");
            expectEqual("POSIX graph", "[:graph:]", "[^\\p{Whitespace}\\p{Control}\\p{Surrogate}\\p{Unassigned}]");
            expectEqual("POSIX print", "[:print:]", "[[:graph:][:blank:]-[\\p{Control}]]");
        }

        [Test]
        public void TestHangulSyllable()
        {
            UnicodeSet lvt = new UnicodeSet("[:Hangul_Syllable_Type=LVT_Syllable:]");
            assertNotEquals("LVT count", new UnicodeSet(), lvt);
            Logln(lvt + ": " + lvt.Count);
            UnicodeSet lv = new UnicodeSet("[:Hangul_Syllable_Type=LV_Syllable:]");
            assertNotEquals("LV count", new UnicodeSet(), lv);
            Logln(lv + ": " + lv.Count);
        }

        /**
         * Test that frozen classes disallow changes. For 4217
         */
        [Test]
        public void TestFrozen()
        {
            UnicodeSet test = new UnicodeSet("[[:whitespace:]A]");
            test.Freeze();
            checkModification(test, true);
            checkModification(test, false);
        }

        /**
         * Test Generic support
         */
        [Test]
        public void TestGenerics()
        {
            UnicodeSet set1 = new UnicodeSet("[a-b d-g {ch} {zh}]").Freeze();
            UnicodeSet set2 = new UnicodeSet("[e-f {ch}]").Freeze();
            UnicodeSet set3 = new UnicodeSet("[d m-n {dh}]").Freeze();
            // A useful range of sets for testing, including both characters and strings
            // set 1 contains set2
            // set 1 is overlaps with set 3
            // set 2 is disjoint with set 3

            //public Iterator<String> iterator() {

            List<String> oldList = new List<String>();
            for (UnicodeSetIterator it = new UnicodeSetIterator(set1); it.Next();)
            {
                oldList.Add(it.GetString());
            }

            List<String> list1 = new List<String>();
            foreach (String s in set1)
            {
                list1.Add(s);
            }
            assertEquals("iteration test", oldList, list1);

            //addAllTo(Iterable<T>, U)
            list1.Clear();
            set1.AddAllTo(list1);
            assertEquals("iteration test", oldList, list1);

            list1 = set1.AddAllTo(new List<String>());
            assertEquals("addAllTo", oldList, list1);

            List<String> list2 = set2.AddAllTo(new List<String>());
            List<String> list3 = set3.AddAllTo(new List<String>());

            // put them into different order, to check that order doesn't matter
            SortedSet<string> sorted1 = set1.AddAllTo(new SortedSet<string>());
            SortedSet<string> sorted2 = set2.AddAllTo(new SortedSet<string>());
            SortedSet<string> sorted3 = set3.AddAllTo(new SortedSet<string>());

            //containsAll(Collection<String> collection)
            assertTrue("containsAll", set1.ContainsAll(list1));
            assertTrue("containsAll", set1.ContainsAll(sorted1));
            assertTrue("containsAll", set1.ContainsAll(list2));
            assertTrue("containsAll", set1.ContainsAll(sorted2));
            assertFalse("containsAll", set1.ContainsAll(list3));
            assertFalse("containsAll", set1.ContainsAll(sorted3));
            assertFalse("containsAll", set2.ContainsAll(list3));
            assertFalse("containsAll", set2.ContainsAll(sorted3));

            //containsSome(Collection<String>)
            assertTrue("containsSome", set1.ContainsSome(list1));
            assertTrue("containsSome", set1.ContainsSome(sorted1));
            assertTrue("containsSome", set1.ContainsSome(list2));
            assertTrue("containsSome", set1.ContainsSome(sorted2));
            assertTrue("containsSome", set1.ContainsSome(list3));
            assertTrue("containsSome", set1.ContainsSome(sorted3));
            assertFalse("containsSome", set2.ContainsSome(list3));
            assertFalse("containsSome", set2.ContainsSome(sorted3));

            //containsNone(Collection<String>)
            assertFalse("containsNone", set1.ContainsNone(list1));
            assertFalse("containsNone", set1.ContainsNone(sorted1));
            assertFalse("containsNone", set1.ContainsNone(list2));
            assertFalse("containsNone", set1.ContainsNone(sorted2));
            assertFalse("containsNone", set1.ContainsNone(list3));
            assertFalse("containsNone", set1.ContainsNone(sorted3));
            assertTrue("containsNone", set2.ContainsNone(list3));
            assertTrue("containsNone", set2.ContainsNone(sorted3));

            //addAll(String...)
            UnicodeSet other3 = new UnicodeSet().AddAll("d", "m", "n", "dh");
            assertEquals("addAll", set3, other3);

            //removeAll(Collection<String>)
            UnicodeSet mod1 = new UnicodeSet(set1).RemoveAll(set2);
            UnicodeSet mod2 = new UnicodeSet(set1).RemoveAll(list2);
            assertEquals("remove all", mod1, mod2);

            //retainAll(Collection<String>)
            mod1 = new UnicodeSet(set1).RetainAll(set2);
            mod2 = new UnicodeSet(set1).RetainAll(set2.AddAllTo(new List<String>()));
            assertEquals("remove all", mod1, mod2);
        }

        private class AnonymousUnicodeSetComparer : IComparer<UnicodeSet>
        {
            private readonly Func<UnicodeSet, UnicodeSet, int> compare;

            public AnonymousUnicodeSetComparer(Func<UnicodeSet, UnicodeSet, int> compare)
            {
                this.compare = compare;
            }

            public int Compare(UnicodeSet x, UnicodeSet y)
            {
                return compare(x, y);
            }
        }

        [Test]
        public void TestComparison()
        {
            UnicodeSet set1 = new UnicodeSet("[a-b d-g {ch} {zh}]").Freeze();
            UnicodeSet set2 = new UnicodeSet("[c-e {ch}]").Freeze();
            UnicodeSet set3 = new UnicodeSet("[d m-n z {dh}]").Freeze();

            //compareTo(UnicodeSet)
            // do indirectly, by sorting
            List<UnicodeSet> unsorted = new UnicodeSet[] { set3, set2, set1 }.ToList();
            List<UnicodeSet> goalShortest = new UnicodeSet[] { set2, set3, set1 }.ToList();
            List<UnicodeSet> goalLongest = new UnicodeSet[] { set1, set3, set2 }.ToList();
            List<UnicodeSet> goalLex = new UnicodeSet[] { set1, set2, set3 }.ToList();

            List<UnicodeSet> sorted = new List<UnicodeSet>(new SortedSet<UnicodeSet>(unsorted));
            assertNotEquals("compareTo-shorter-first", unsorted, sorted);
            assertEquals("compareTo-shorter-first", goalShortest, sorted);

            SortedSet<UnicodeSet> sorted1 = new SortedSet<UnicodeSet>(new AnonymousUnicodeSetComparer(
                compare: (o1, o2) =>
                {
                    // TODO Auto-generated method stub
                    return o1.CompareTo(o2, ComparisonStyle.LongerFirst);
                }));

            //sorted1.AddAll(unsorted);
            sorted1.UnionWith(unsorted);
            sorted = new List<UnicodeSet>(sorted1);
            assertNotEquals("compareTo-longer-first", unsorted, sorted);
            assertEquals("compareTo-longer-first", goalLongest, sorted);

            sorted1 = new SortedSet<UnicodeSet>(new AnonymousUnicodeSetComparer(
                compare: (o1, o2) =>
                {
                    // TODO Auto-generated method stub
                    return o1.CompareTo(o2, ComparisonStyle.Lexicographic);
                }));
            //sorted1.AddAll(unsorted);
            sorted1.UnionWith(unsorted);
            sorted = new List<UnicodeSet>(sorted1);
            assertNotEquals("compareTo-lex", unsorted, sorted);
            assertEquals("compareTo-lex", goalLex, sorted);

            //compare(String, int)
            // make a list of interesting combinations
            List<String> sources = new string[] { "\u0000", "a", "b", "\uD7FF", "\uD800", "\uDBFF", "\uDC00", "\uDFFF", "\uE000", "\uFFFD", "\uFFFF" }.ToList();
            SortedSet<String> target = new SortedSet<String>(StringComparer.Ordinal);
            foreach (String s in sources)
            {
                target.Add(s);
                foreach (String t in sources)
                {
                    target.Add(s + t);
                    foreach (String u in sources)
                    {
                        target.Add(s + t + u);
                    }
                }
            }
            // now compare all the combinations. If any of them is a code point, use it.
            int maxErrorCount = 0;
            //compare:
            foreach (String last in target)
            {
                foreach (String curr in target)
                {
                    int lastCount = J2N.Character.CodePointCount(last, 0, last.Length);
                    int currCount = J2N.Character.CodePointCount(curr, 0, curr.Length);
                    int comparison;
                    if (lastCount == 1)
                    {
                        comparison = UnicodeSet.Compare(last.CodePointAt(0), curr);
                    }
                    else if (currCount == 1)
                    {
                        comparison = UnicodeSet.Compare(last, curr.CodePointAt(0));
                    }
                    else
                    {
                        continue;
                    }
                    // ICU4N specific - CompareTo implementations are not guaranteed to return the
                    // same number, only the same sign. So, we must normalize the values to -1, 0, and 1
                    // using Number.Signum() in order to compare them correctly.
                    if (Number.Signum(comparison) != Number.Signum(last.CompareToOrdinal(curr)))
                    {
                        // repeat for debugging
                        if (lastCount == 1)
                        {
                            comparison = UnicodeSet.Compare(last.CodePointAt(0), curr);
                        }
                        else if (currCount == 1)
                        {
                            comparison = UnicodeSet.Compare(last, curr.CodePointAt(0));
                        }
                        if (maxErrorCount++ > 10)
                        {
                            Errln(maxErrorCount + " Failure in comparing " + last + " & " + curr + "\tOmitting others...");
                            goto compare_break;
                        }
                        Errln(maxErrorCount + " Failure in comparing " + last + " & " + curr);
                    }
                }
            }
            compare_break: { }

            //compare(Iterable<T>, Iterable<T>)
            int max = 10;
            List<String> test1 = new List<String>(max);
            List<String> test2 = new List<String>(max);
            for (int i = 0; i <= max; ++i)
            {
                test1.Add("a" + i);
                test2.Add("a" + (max - i)); // add in reverse order
            }
            assertNotEquals("compare iterable test", test1, test2);
            SortedSet<ICharSequence> sortedTest1 = new SortedSet<ICharSequence>(test1.Select((s) => s.ToCharSequence()));
            SortedSet<ICharSequence> sortedTest2 = new SortedSet<ICharSequence>(test2.Select((s) => s.ToCharSequence()));
            assertEquals("compare iterable test", sortedTest1, sortedTest2);
        }

        [Test]
        public void TestRangeConstructor()
        {
            UnicodeSet w = new UnicodeSet().AddAll(3, 5);
            UnicodeSet s = new UnicodeSet(3, 5);
            assertEquals("new constructor", w, s);

            w = new UnicodeSet().AddAll(3, 5).AddAll(7, 7);
            UnicodeSet t = new UnicodeSet(3, 5, 7, 7);
            assertEquals("new constructor", w, t);
            // check to make sure right exceptions are thrown
            Type expected = typeof(ArgumentException);
            Type actual;

            try
            {
                actual = null;
                UnicodeSet u = new UnicodeSet(5);
            }
            catch (ArgumentException e)
            {
                actual = e.GetType();
            }
            assertEquals("exception if odd", expected, actual);

            try
            {
                actual = null;
                UnicodeSet u = new UnicodeSet(3, 2, 7, 9);
            }
            catch (ArgumentException e)
            {
                actual = e.GetType();
            }
            assertEquals("exception for start/end problem", expected, actual);

            try
            {
                actual = null;
                UnicodeSet u = new UnicodeSet(3, 5, 6, 9);
            }
            catch (ArgumentException e)
            {
                actual = e.GetType();
            }
            assertEquals("exception for end/start problem", expected, actual);

            CheckRangeSpeed(10000, new UnicodeSet("[:whitespace:]"));
            CheckRangeSpeed(1000, new UnicodeSet("[:letter:]"));
        }

        /**
         * @param iterations
         * @param testSet
         */
        private void CheckRangeSpeed(int iterations, UnicodeSet testSet)
        {
            testSet.Complement().Complement();
            String testPattern = testSet.ToString();
            // fill a set of pairs from the pattern
            int[] pairs = new int[testSet.RangeCount * 2];
            int j = 0;
            for (UnicodeSetIterator it = new UnicodeSetIterator(testSet); it.NextRange();)
            {
                pairs[j++] = it.Codepoint;
                pairs[j++] = it.CodepointEnd;
            }
            UnicodeSet fromRange = new UnicodeSet(testSet);
            assertEquals("from range vs pattern", testSet, fromRange);

            // ICU4N specific - switched to Stopwatch because static timers do not have
            // enough precision in .NET to run this test.
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < iterations; ++i)
            {
                fromRange = new UnicodeSet(testSet);
            }

            sw.Stop();
            double rangeConstructorTime = sw.ElapsedMilliseconds;
            sw.Restart();

            for (int i = 0; i < iterations; ++i)
            {
                new UnicodeSet(testPattern);
            }

            sw.Stop();
            double patternConstructorTime = sw.ElapsedMilliseconds;

            String message = "Range constructor:\t" + rangeConstructorTime + ";\tPattern constructor:\t" + patternConstructorTime + "\t\t"
                    + string.Format("{0:p}", rangeConstructorTime / patternConstructorTime - 1);// percent.format(rangeConstructorTime / patternConstructorTime - 1);
            if (rangeConstructorTime < 2 * patternConstructorTime)
            {
                Logln(message);
            }
            else
            {
                Errln(message);
            }
        }

        //NumberFormat percent = NumberFormat.getPercentInstance();
        //    {
        //        percent.setMaximumFractionDigits(2);
        //    }
        // ****************************************
        // UTILITIES
        // ****************************************

        public void checkModification(UnicodeSet original, bool isFrozen)
        {
            //main:
            for (int i = 0; ; ++i)
            {
                UnicodeSet test = (UnicodeSet)(isFrozen ? original.Clone() : original.CloneAsThawed());
                bool gotException = true;
                bool checkEquals = true;
                try
                {
                    switch (i)
                    {
                        case 0: test.Add(0); break;
                        case 1: test.Add(0, 1); break;
                        case 2: test.Add("a"); break;
                        case 3: List<string> a = new List<string>(); a.Add("a"); test.AddAll(a); break;
                        case 4: test.AddAll("ab"); break;
                        case 5: test.AddAll(new UnicodeSet("[ab]")); break;
                        case 6: test.ApplyInt32PropertyValue(0, 0); break;
                        case 7: test.ApplyPattern("[ab]"); break;
                        case 8: test.ApplyPattern("[ab]", true); break;
                        case 9: test.ApplyPattern("[ab]", 0); break;
                        case 10: test.ApplyPropertyAlias("hex", "true"); break;
                        case 11: test.ApplyPropertyAlias("hex", "true", null); break;
                        case 12: test.CloseOver(UnicodeSet.Case); break;
                        case 13: test.Compact(); checkEquals = false; break;
                        case 14: test.Complement(0); break;
                        case 15: test.Complement(0, 0); break;
                        case 16: test.Complement("ab"); break;
                        case 17: test.ComplementAll("ab"); break;
                        case 18: test.ComplementAll(new UnicodeSet("[ab]")); break;
                        case 19: test.Remove(' '); break;
                        case 20: test.Remove(' ', 'a'); break;
                        case 21: test.Remove(" "); break;
                        case 22: test.RemoveAll(" a"); break;
                        case 23: test.RemoveAll(new UnicodeSet("[\\ a]")); break;
                        case 24: test.Retain(' '); break;
                        case 25: test.Retain(' ', 'a'); break;
                        case 26: test.Retain(" "); break;
                        case 27: test.RetainAll(" a"); break;
                        case 28: test.RetainAll(new UnicodeSet("[\\ a]")); break;
                        case 29: test.Set(0, 1); break;
                        case 30: test.Set(new UnicodeSet("[ab]")); break;

                        default: goto main_continue; // so we don't keep having to change the endpoint, and gaps are not skipped.
                        case 35: return;
                    }
                    gotException = false;
                }
                catch (NotSupportedException e)
                {
                    // do nothing
                }
                if (isFrozen && !gotException) Errln(i + ") attempt to modify frozen object didn't result in an exception");
                if (!isFrozen && gotException) Errln(i + ") attempt to modify thawed object did result in an exception");
                if (checkEquals)
                {
                    if (test.Equals(original))
                    {
                        if (!isFrozen) Errln(i + ") attempt to modify thawed object didn't change the object");
                    }
                    else
                    { // unequal
                        if (isFrozen) Errln(i + ") attempt to modify frozen object changed the object");
                    }
                }
                main_continue: { }
            }
        }

        // Following cod block is commented out to eliminate PrettyPrinter depenencies

        //    String[] prettyData = {
        //            "[\\uD7DE-\\uD90C \\uDCB5-\\uDD9F]", // special case
        //            "[:any:]",
        //            "[:whitespace:]",
        //            "[:linebreak=AL:]",
        //    };
        //
        //    public void TestPrettyPrinting() {
        //        try{
        //            PrettyPrinter pp = new PrettyPrinter();
        //
        //            int i = 0;
        //            for (; i < prettyData.Length; ++i) {
        //                UnicodeSet test = new UnicodeSet(prettyData[i]);
        //                checkPrettySet(pp, i, test);
        //            }
        //            Random random = new Random(0);
        //            UnicodeSet test = new UnicodeSet();
        //
        //            // To keep runtimes under control, make the number of random test cases
        //            //   to try depends on the test framework exhaustive setting.
        //            //  params.inclusions = 5:   default exhaustive value
        //            //  params.inclusions = 10:  max exhaustive value.
        //            int iterations = 50;
        //            if (params.inclusion > 5) {
        //                iterations = (params.inclusion-5) * 200;
        //            }
        //            for (; i < iterations; ++i) {
        //                double start = random.nextGaussian() * 0x10000;
        //                if (start < 0) start = - start;
        //                if (start > 0x10FFFF) {
        //                    start = 0x10FFFF;
        //                }
        //                double end = random.nextGaussian() * 0x100;
        //                if (end < 0) end = -end;
        //                end = start + end;
        //                if (end > 0x10FFFF) {
        //                    end = 0x10FFFF;
        //                }
        //                test.Complement((int)start, (int)end);
        //                checkPrettySet(pp, i, test);
        //            }
        //        }catch(Exception ex){
        //            warnln("Could not load Collator");
        //        }
        //    }
        //
        //    private void checkPrettySet(PrettyPrinter pp, int i, UnicodeSet test) {
        //        String pretty = pp.ToPattern(test);
        //        UnicodeSet retry = new UnicodeSet(pretty);
        //        if (!test.Equals(retry)) {
        //            Errln(i + ". Failed test: " + test + " != " + pretty);
        //        } else {
        //            Logln(i + ". Worked for " + truncate(test.ToString()) + " => " + truncate(pretty));
        //        }
        //    }
        //
        //    private String truncate(String string) {
        //        if (string.Length <= 100) return string;
        //        return string.substring(0,97) + "...";
        //    }

        public class TokenSymbolTable : ISymbolTable
        {
            IDictionary<string, char[]> contents = new Dictionary<string, char[]>();


            /**
             * (Non-SymbolTable API) Add the given variable and value to
             * the table.  Variable should NOT contain leading '$'.
             */
            public void Add(String var, String value)
            {
                char[] buffer = new char[value.Length];
                //value.getChars(0, value.Length, buffer, 0);
                value.CopyTo(0, buffer, 0, value.Length);
                Add(var, buffer);
            }

            /**
             * (Non-SymbolTable API) Add the given variable and value to
             * the table.  Variable should NOT contain leading '$'.
             */
            public void Add(String var, char[] body)
            {
                AbstractTestLog.Logln("TokenSymbolTable: add \"" + var + "\" => \"" +
                        new String(body) + "\"");
                contents[var] = body;
            }

            /* (non-Javadoc)
             * @see com.ibm.icu.text.SymbolTable#lookup(java.lang.String)
             */

            public char[] Lookup(String s)
            {
                AbstractTestLog.Logln("TokenSymbolTable: lookup \"" + s + "\" => \"" +
                new String((char[])contents.Get(s)) + "\"");
                return (char[])contents.Get(s);
            }

            /* (non-Javadoc)
             * @see com.ibm.icu.text.SymbolTable#lookupMatcher(int)
             */
            public IUnicodeMatcher LookupMatcher(int ch)
            {
                return null;
            }

            /* (non-Javadoc)
             * @see com.ibm.icu.text.SymbolTable#parseReference(java.lang.String,
            java.text.ParsePosition, int)
             */
            public String ParseReference(String text, ParsePosition pos, int
                    limit)
            {
                int cp;
                int start = pos.Index;
                int i;
                for (i = start; i < limit; i += UTF16.GetCharCount(cp))
                {
                    cp = UTF16.CharAt(text, i);
                    if (!UChar.IsUnicodeIdentifierPart(cp))
                    {
                        break;
                    }
                }
                AbstractTestLog.Logln("TokenSymbolTable: parse \"" + text + "\" from " +
                start + " to " + i +
                " => \"" + text.Substring(start, i - start) + "\""); // ICU4N: Corrected 2nd parameter
                pos.Index = (i);
                return text.Substring(start, i - start); // ICU4N: Corrected 2nd parameter
            }
        }

        [Test]
        public void TestSurrogate()
        {
            String[] DATA = {
                // These should all behave identically
                "[abc\\uD800\\uDC00]",
                "[abc\uD800\uDC00]",
                "[abc\\U00010000]",
        };
            for (int i = 0; i < DATA.Length; ++i)
            {
                Logln("Test pattern " + i + " :" + Utility.Escape(DATA[i]));
                UnicodeSet set = new UnicodeSet(DATA[i]);
                expectContainment(set,
                        CharsToUnicodeString("abc\\U00010000"),
                        "\uD800;\uDC00"); // split apart surrogate-pair
                if (set.Count != 4)
                {
                    Errln(Utility.Escape("FAIL: " + DATA[i] + ".Count == " +
                            set.Count + ", expected 4"));
                }
            }
        }

        [Test]
        public void TestContains()
        {
            int limit = 256; // combinations to test
            for (int i = 0; i < limit; ++i)
            {
                Logln("Trying: " + i);
                UnicodeSet x = bitsToSet(i);
                for (int j = 0; j < limit; ++j)
                {
                    UnicodeSet y = bitsToSet(j);
                    bool containsNone = (i & j) == 0;
                    bool containsAll = (i & j) == j;
                    bool equals = i == j;
                    if (containsNone != x.ContainsNone(y))
                    {
                        x.ContainsNone(y); // repeat for debugging
                        Errln("FAILED: " + x + " containsSome " + y);
                    }
                    if (containsAll != x.ContainsAll(y))
                    {
                        x.ContainsAll(y); // repeat for debugging
                        Errln("FAILED: " + x + " containsAll " + y);
                    }
                    if (equals != x.Equals(y))
                    {
                        x.Equals(y); // repeat for debugging
                        Errln("FAILED: " + x + " equals " + y);
                    }
                }
            }
        }

        void _testComplement(int a)
        {
            UnicodeSet x = bitsToSet(a);
            UnicodeSet z = bitsToSet(a);
            z.Complement();
            int c = setToBits(z);
            if (c != (~a))
            {
                Errln("FAILED: add: ~" + x + " != " + z);
                Errln("FAILED: add: ~" + a + " != " + c);
            }
            checkCanonicalRep(z, "complement " + a);
        }

        void _testAdd(int a, int b)
        {
            UnicodeSet x = bitsToSet(a);
            UnicodeSet y = bitsToSet(b);
            UnicodeSet z = bitsToSet(a);
            z.AddAll(y);
            int c = setToBits(z);
            if (c != (a | b))
            {
                Errln(Utility.Escape("FAILED: add: " + x + " | " + y + " != " + z));
                Errln("FAILED: add: " + a + " | " + b + " != " + c);
            }
            checkCanonicalRep(z, "add " + a + "," + b);
        }

        void _testRetain(int a, int b)
        {
            UnicodeSet x = bitsToSet(a);
            UnicodeSet y = bitsToSet(b);
            UnicodeSet z = bitsToSet(a);
            z.RetainAll(y);
            int c = setToBits(z);
            if (c != (a & b))
            {
                Errln("FAILED: retain: " + x + " & " + y + " != " + z);
                Errln("FAILED: retain: " + a + " & " + b + " != " + c);
            }
            checkCanonicalRep(z, "retain " + a + "," + b);
        }

        void _testRemove(int a, int b)
        {
            UnicodeSet x = bitsToSet(a);
            UnicodeSet y = bitsToSet(b);
            UnicodeSet z = bitsToSet(a);
            z.RemoveAll(y);
            int c = setToBits(z);
            if (c != (a & ~b))
            {
                Errln("FAILED: remove: " + x + " &~ " + y + " != " + z);
                Errln("FAILED: remove: " + a + " &~ " + b + " != " + c);
            }
            checkCanonicalRep(z, "remove " + a + "," + b);
        }

        void _testXor(int a, int b)
        {
            UnicodeSet x = bitsToSet(a);
            UnicodeSet y = bitsToSet(b);
            UnicodeSet z = bitsToSet(a);
            z.ComplementAll(y);
            int c = setToBits(z);
            if (c != (a ^ b))
            {
                Errln("FAILED: complement: " + x + " ^ " + y + " != " + z);
                Errln("FAILED: complement: " + a + " ^ " + b + " != " + c);
            }
            checkCanonicalRep(z, "complement " + a + "," + b);
        }

        /**
         * Check that ranges are monotonically increasing and non-
         * overlapping.
         */
        void checkCanonicalRep(UnicodeSet set, String msg)
        {
            int n = set.RangeCount;
            if (n < 0)
            {
                Errln("FAIL result of " + msg +
                        ": range count should be >= 0 but is " +
                        n + " for " + Utility.Escape(set.ToString()));
                return;
            }
            int last = 0;
            for (int i = 0; i < n; ++i)
            {
                int start = set.GetRangeStart(i);
                int end = set.GetRangeEnd(i);
                if (start > end)
                {
                    Errln("FAIL result of " + msg +
                            ": range " + (i + 1) +
                            " start > end: " + start + ", " + end +
                            " for " + Utility.Escape(set.ToString()));
                }
                if (i > 0 && start <= last)
                {
                    Errln("FAIL result of " + msg +
                            ": range " + (i + 1) +
                            " overlaps previous range: " + start + ", " + end +
                            " for " + Utility.Escape(set.ToString()));
                }
                last = end;
            }
        }

        /**
         * Convert a bitmask to a UnicodeSet.
         */
        UnicodeSet bitsToSet(int a)
        {
            UnicodeSet result = new UnicodeSet();
            for (int i = 0; i < 32; ++i)
            {
                if ((a & (1 << i)) != 0)
                {
                    result.Add((char)i, (char)i);
                }
            }

            return result;
        }

        /**
         * Convert a UnicodeSet to a bitmask.  Only the characters
         * U+0000 to U+0020 are represented in the bitmask.
         */
        static int setToBits(UnicodeSet x)
        {
            int result = 0;
            for (int i = 0; i < 32; ++i)
            {
                if (x.Contains((char)i))
                {
                    result |= (1 << i);
                }
            }
            return result;
        }

        /**
         * Return the representation of an inversion list based UnicodeSet
         * as a pairs list.  Ranges are listed in ascending Unicode order.
         * For example, the set [a-zA-M3] is represented as "33AMaz".
         */
        static String getPairs(UnicodeSet set)
        {
            StringBuffer pairs = new StringBuffer();
            for (int i = 0; i < set.RangeCount; ++i)
            {
                int start = set.GetRangeStart(i);
                int end = set.GetRangeEnd(i);
                if (end > 0xFFFF)
                {
                    end = 0xFFFF;
                    i = set.RangeCount; // Should be unnecessary
                }
                pairs.Append((char)start).Append((char)end);
            }
            return pairs.ToString();
        }

        /**
         * Test function. Make sure that the sets have the right relation
         */

        void expectRelation(Object relationObj, Object set1Obj, Object set2Obj, String message)
        {
            int relation = ((int)relationObj);
            UnicodeSet set1 = (UnicodeSet)set1Obj;
            UnicodeSet set2 = (UnicodeSet)set2Obj;

            // by-the-by, check the iterator
            checkRoundTrip(set1);
            checkRoundTrip(set2);

            bool contains = set1.ContainsAll(set2);
            bool isContained = set2.ContainsAll(set1);
            bool disjoint = set1.ContainsNone(set2);
            bool equals = set1.Equals(set2);

            UnicodeSet intersection = new UnicodeSet(set1).RetainAll(set2);
            UnicodeSet minus12 = new UnicodeSet(set1).RemoveAll(set2);
            UnicodeSet minus21 = new UnicodeSet(set2).RemoveAll(set1);

            // test basic properties

            if (contains != (intersection.Count == set2.Count))
            {
                Errln("FAIL contains1" + set1.ToPattern(true) + ", " + set2.ToPattern(true));
            }

            if (contains != (intersection.Equals(set2)))
            {
                Errln("FAIL contains2" + set1.ToPattern(true) + ", " + set2.ToPattern(true));
            }

            if (isContained != (intersection.Count == set1.Count))
            {
                Errln("FAIL isContained1" + set1.ToPattern(true) + ", " + set2.ToPattern(true));
            }

            if (isContained != (intersection.Equals(set1)))
            {
                Errln("FAIL isContained2" + set1.ToPattern(true) + ", " + set2.ToPattern(true));
            }

            if ((contains && isContained) != equals)
            {
                Errln("FAIL equals" + set1.ToPattern(true) + ", " + set2.ToPattern(true));
            }

            if (disjoint != (intersection.Count == 0))
            {
                Errln("FAIL disjoint" + set1.ToPattern(true) + ", " + set2.ToPattern(true));
            }

            // Now see if the expected relation is true
            int status = (minus12.Count != 0 ? 4 : 0)
                    | (intersection.Count != 0 ? 2 : 0)
                    | (minus21.Count != 0 ? 1 : 0);

            if (status != relation)
            {
                Errln("FAIL relation incorrect" + message
                        + "; desired = " + RELATION_NAME[relation]
                                + "; found = " + RELATION_NAME[status]
                                        + "; set1 = " + set1.ToPattern(true)
                                        + "; set2 = " + set2.ToPattern(true)
                        );
            }
        }

        /**
         * Basic consistency check for a few items.
         * That the iterator works, and that we can create a pattern and
         * get the same thing back
         */

        void checkRoundTrip(UnicodeSet s)
        {
            String pat = s.ToPattern(false);
            UnicodeSet t = copyWithIterator(s, false);
            checkEqual(s, t, "iterator roundtrip");

            t = copyWithIterator(s, true); // try range
            checkEqual(s, t, "iterator roundtrip");

            t = new UnicodeSet(pat);
            checkEqual(s, t, "toPattern(false)");

            pat = s.ToPattern(true);
            t = new UnicodeSet(pat);
            checkEqual(s, t, "toPattern(true)");
        }

        UnicodeSet copyWithIterator(UnicodeSet s, bool withRange)
        {
            UnicodeSet t = new UnicodeSet();
            UnicodeSetIterator it = new UnicodeSetIterator(s);
            if (withRange)
            {
                while (it.NextRange())
                {
                    if (it.Codepoint == UnicodeSetIterator.IsString)
                    {
                        t.Add(it.String);
                    }
                    else
                    {
                        t.Add(it.Codepoint, it.CodepointEnd);
                    }
                }
            }
            else
            {
                while (it.Next())
                {
                    if (it.Codepoint == UnicodeSetIterator.IsString)
                    {
                        t.Add(it.String);
                    }
                    else
                    {
                        t.Add(it.Codepoint);
                    }
                }
            }
            return t;
        }

        bool checkEqual(UnicodeSet s, UnicodeSet t, String message)
        {
            if (!s.Equals(t))
            {
                Errln("FAIL " + message
                        + "; source = " + s.ToPattern(true)
                        + "; result = " + t.ToPattern(true)
                        );
                return false;
            }
            return true;
        }

        void expectEqual(String name, String pat1, String pat2)
        {
            UnicodeSet set1, set2;
            try
            {
                set1 = new UnicodeSet(pat1);
                set2 = new UnicodeSet(pat2);
            }
            catch (ArgumentException e)
            {
                Errln("FAIL: Couldn't create UnicodeSet from pattern for \"" + name + "\": " + e.ToString());
                return;
            }
            if (!set1.Equals(set2))
            {
                Errln("FAIL: Sets built from patterns differ for \"" + name + "\"");
            }
        }

        /**
         * Expect the given set to contain the characters in charsIn and
         * to not contain those in charsOut.
         */
        void expectContainment(String pat, String charsIn, String charsOut)
        {
            UnicodeSet set;
            try
            {
                set = new UnicodeSet(pat);
            }
            catch (ArgumentException e)
            {
                Errln("FAIL: Couldn't create UnicodeSet from pattern \"" +
                        pat + "\": " + e.ToString());
                return;
            }
            expectContainment(set, charsIn, charsOut);
        }

        /**
         * Expect the given set to contain the characters in charsIn and
         * to not contain those in charsOut.
         */
        void expectContainment(UnicodeSet set, String charsIn, String charsOut)
        {
            StringBuffer bad = new StringBuffer();
            if (charsIn != null)
            {
                charsIn = Utility.Unescape(charsIn);
                for (int i = 0; i < charsIn.Length;)
                {
                    int c = UTF16.CharAt(charsIn, i);
                    i += UTF16.GetCharCount(c);
                    if (!set.Contains(c))
                    {
                        UTF16.Append(bad, c);
                    }
                }
                if (bad.Length > 0)
                {
                    Errln(Utility.Escape("FAIL: set " + set + " does not contain " + bad +
                            ", expected containment of " + charsIn));
                }
                else
                {
                    Logln(Utility.Escape("Ok: set " + set + " contains " + charsIn));
                }
            }
            if (charsOut != null)
            {
                charsOut = Utility.Unescape(charsOut);
                bad.Length = (0);
                for (int i = 0; i < charsOut.Length;)
                {
                    int c = UTF16.CharAt(charsOut, i);
                    i += UTF16.GetCharCount(c);
                    if (set.Contains(c))
                    {
                        UTF16.Append(bad, c);
                    }
                }
                if (bad.Length > 0)
                {
                    Errln(Utility.Escape("FAIL: set " + set + " contains " + bad +
                            ", expected non-containment of " + charsOut));
                }
                else
                {
                    Logln(Utility.Escape("Ok: set " + set + " does not contain " + charsOut));
                }
            }
        }

        void expectPattern(UnicodeSet set,
                String pattern,
                String expectedPairs)
        {
            set.ApplyPattern(pattern);
            if (!getPairs(set).Equals(expectedPairs))
            {
                Errln("FAIL: applyPattern(\"" + pattern +
                        "\") => pairs \"" +
                        Utility.Escape(getPairs(set)) + "\", expected \"" +
                        Utility.Escape(expectedPairs) + "\"");
            }
            else
            {
                Logln("Ok:   applyPattern(\"" + pattern +
                        "\") => pairs \"" +
                        Utility.Escape(getPairs(set)) + "\"");
            }
        }

        void expectToPattern(UnicodeSet set,
                String expPat,
                String[] expStrings)
        {
            String pat = set.ToPattern(true);
            if (pat.Equals(expPat))
            {
                Logln("Ok:   toPattern() => \"" + pat + "\"");
            }
            else
            {
                Errln("FAIL: toPattern() => \"" + pat + "\", expected \"" + expPat + "\"");
                return;
            }
            if (expStrings == null)
            {
                return;
            }
            bool @in = true;
            for (int i = 0; i < expStrings.Length; ++i)
            {
                if (expStrings[i] == NOT)
                { // sic; pointer comparison
                    @in = false;
                    continue;
                }
                bool contained = set.Contains(expStrings[i]);
                if (contained == @in)
                {
                    Logln("Ok: " + expPat +
                            (contained ? " contains {" : " does not contain {") +
                            Utility.Escape(expStrings[i]) + "}");
                }
                else
                {
                    Errln("FAIL: " + expPat +
                            (contained ? " contains {" : " does not contain {") +
                            Utility.Escape(expStrings[i]) + "}");
                }
            }
        }

        void expectPairs(UnicodeSet set, String expectedPairs)
        {
            if (!getPairs(set).Equals(expectedPairs))
            {
                Errln("FAIL: Expected pair list \"" +
                        Utility.Escape(expectedPairs) + "\", got \"" +
                        Utility.Escape(getPairs(set)) + "\"");
            }
        }
        static String CharsToUnicodeString(String s)
        {
            return Utility.Unescape(s);
        }

        /* Test the method public UnicodeSet getSet() */
        [Test]
        public void TestGetSet()
        {
            UnicodeSetIterator us = new UnicodeSetIterator();
            try
            {
                var _ = us.Set;
            }
            catch (Exception e)
            {
                Errln("UnicodeSetIterator.Set was not suppose to given an " + "an exception.");
            }
        }

        /* Tests the method public UnicodeSet add(Collection<?> source) */
        [Test]
        public void TestAddCollection()
        {
            UnicodeSet us = new UnicodeSet();
            ICollection<string> s = null;
            try
            {
                us.Add(s);
                Errln("UnicodeSet.Add(Collection<string>) was suppose to return an exception for a null parameter.");
            }
            catch (Exception e)
            {
            }
        }

        [Test]
        public void TestConstants()
        {
            assertEquals("Empty", new UnicodeSet(), UnicodeSet.Empty);
            assertEquals("All", new UnicodeSet(0, 0x10FFFF), UnicodeSet.AllCodePoints);
        }

        [Test]
        public void TestIteration()
        {
            UnicodeSet us1 = new UnicodeSet("[abcM{xy}]");
            assertEquals("", "M, a-c", string.Join(", ", us1.Ranges));

            // Sample code
            foreach (UnicodeSetEntryRange range in us1.Ranges)
            {
                // do something with code points between range.CodepointEnd and range.CodepointEnd;
            }
            foreach (String s in us1.Strings)
            {
                // do something with each string;
            }

            String[] tests = {
                "[M-Qzab{XY}{ZW}]",
                "[]",
                "[a]",
                "[a-c]",
                "[{XY}]",
        };
            foreach (String test in tests)
            {
                UnicodeSet us = new UnicodeSet(test);
                UnicodeSetIterator it = new UnicodeSetIterator(us);
                foreach (UnicodeSetEntryRange range in us.Ranges)
                {
                    String title = range.ToString();
                    Logln(title);
                    it.NextRange();
                    assertEquals(title, it.Codepoint, range.Codepoint);
                    assertEquals(title, it.CodepointEnd, range.CodepointEnd);
                }
                foreach (String s in us.Strings)
                {
                    it.NextRange();
                    assertEquals("strings", it.String, s);
                }
                assertFalse("", it.Next());
            }
        }

        // ICU4N TODO: Need tests for string, StringBuilder, and char[] when those methods are implemented
        [Test]
        public void TestReplaceAndDelete()
        {
            UnicodeSetSpanner m;

            m = new UnicodeSetSpanner(new UnicodeSet("[._]"));
            assertEquals("", "abc", m.DeleteFrom("_._a_._b_._c_._"));
            assertEquals("", "_.__.__.__._", m.DeleteFrom("_._a_._b_._c_._", SpanCondition.NotContained));

            assertEquals("", "a_._b_._c", m.Trim("_._a_._b_._c_._"));
            assertEquals("", "a_._b_._c_._", m.Trim("_._a_._b_._c_._", TrimOption.Leading));
            assertEquals("", "_._a_._b_._c", m.Trim("_._a_._b_._c_._", TrimOption.Trailing));

            assertEquals("", "a??b??c", m.ReplaceFrom("a_._b_._c", "??", CountMethod.WholeSpan));
            assertEquals("", "a??b??c", m.ReplaceFrom(m.Trim("_._a_._b_._c_._"), "??", CountMethod.WholeSpan));
            assertEquals("", "XYXYXYaXYXYXYbXYXYXYcXYXYXY", m.ReplaceFrom("_._a_._b_._c_._", "XY"));
            assertEquals("", "XYaXYbXYcXY", m.ReplaceFrom("_._a_._b_._c_._", "XY", CountMethod.WholeSpan));

            m = new UnicodeSetSpanner(new UnicodeSet("\\p{uppercase}"));
            assertEquals("", "TQBF", m.DeleteFrom("The Quick Brown Fox.", SpanCondition.NotContained));

            m = new UnicodeSetSpanner(m.UnicodeSet.AddAll(new UnicodeSet("\\p{lowercase}")));
            assertEquals("", "TheQuickBrownFox", m.DeleteFrom("The Quick Brown Fox.", SpanCondition.NotContained));

            m = new UnicodeSetSpanner(new UnicodeSet("[{ab}]"));
            assertEquals("", "XXc acb", m.ReplaceFrom("ababc acb", "X"));
            assertEquals("", "Xc acb", m.ReplaceFrom("ababc acb", "X", CountMethod.WholeSpan));
            assertEquals("", "ababX", m.ReplaceFrom("ababc acb", "X", CountMethod.WholeSpan, SpanCondition.NotContained));
        }

        [Test]
        public void TestCodePoints()
        {
            // test supplemental code points and strings clusters
            checkCodePoints("x\u0308", "z\u0308", CountMethod.MinElements, SpanCondition.Simple, null, 1);
            checkCodePoints("𣿡", "𣿢", CountMethod.MinElements, SpanCondition.Simple, null, 1);
            checkCodePoints("👦", "👧", CountMethod.MinElements, SpanCondition.Simple, null, 1);
        }

        private void checkCodePoints(String a, String b, CountMethod quantifier, SpanCondition spanCondition,
                String expectedReplaced, int expectedCount)
        {
            String ab = a + b;
            UnicodeSetSpanner m = new UnicodeSetSpanner(new UnicodeSet("[{" + a + "}]"));
            assertEquals("new UnicodeSetSpanner(\"[{" + a + "}]\").countIn(\"" + ab + "\")",
                    expectedCount,
                    callCountIn(m, ab, quantifier, spanCondition)
                    );

            if (expectedReplaced == null)
            {
                expectedReplaced = "-" + b;
            }
            assertEquals("new UnicodeSetSpanner(\"[{" + a + "}]\").ReplaceFrom(\"" + ab + "\", \"-\")",
                    expectedReplaced, m.ReplaceFrom(ab, "-", quantifier));
        }

        [Test]
        public void TestCountIn()
        {
            UnicodeSetSpanner m = new UnicodeSetSpanner(new UnicodeSet("[ab]"));
            checkCountIn(m, CountMethod.MinElements, SpanCondition.Simple, "abc", 2);
            checkCountIn(m, CountMethod.WholeSpan, SpanCondition.Simple, "abc", 1);
            checkCountIn(m, CountMethod.MinElements, SpanCondition.NotContained, "acccb", 3);
        }

        internal void checkCountIn(UnicodeSetSpanner m, CountMethod countMethod, SpanCondition spanCondition, String target, int expected)
        {
            String message = "countIn " + countMethod + ", " + spanCondition;
            assertEquals(message, callCountIn(m, target, countMethod, spanCondition), expected);
        }

        internal int callCountIn(UnicodeSetSpanner m, String ab, CountMethod countMethod, SpanCondition spanCondition)
        {
            return spanCondition != SpanCondition.Simple ? m.CountIn(ab, countMethod, spanCondition)
                    : countMethod != CountMethod.MinElements ? m.CountIn(ab, countMethod)
                            : m.CountIn(ab);
        }

        [Test]
        public void TestForSpanGaps()
        {
            String[] items = { "a", "b", "c", "{ab}", "{bc}", "{cd}", "{abc}", "{bcd}" };
            int limit = 1 << items.Length;
            // build long string for testing
            StringBuilder longBuffer = new StringBuilder();
            for (int i = 1; i < limit; ++i)
            {
                longBuffer.Append("x");
                longBuffer.Append(getCombinations(items, i));
            }
            String longString = longBuffer.ToString();
            longString = longString.Replace("{", "").Replace("}", "");

            long start = Time.NanoTime();
            for (int i = 1; i < limit; ++i)
            {
                UnicodeSet us = new UnicodeSet("[" + getCombinations(items, i) + "]");
                int problemFound = checkSpan(longString, us, SpanCondition.Simple);
                if (problemFound >= 0)
                {
                    assertEquals("Testing " + longString + ", found gap at", -1, problemFound);
                    break;
                }
            }
            long end = Time.NanoTime();
            Logln("Time for SIMPLE   :\t" + (end - start));
            start = Time.NanoTime();
            for (int i = 1; i < limit; ++i)
            {
                UnicodeSet us = new UnicodeSet("[" + getCombinations(items, i) + "]");
                int problemFound = checkSpan(longString, us, SpanCondition.Contained);
                if (problemFound >= 0)
                {
                    assertEquals("Testing " + longString + ", found gap at", -1, problemFound);
                    break;
                }
            }
            end = Time.NanoTime();
            Logln("Time for CONTAINED:\t" + (end - start));
        }

        /**
         * Check that there are no gaps, when we alternate spanning. That is, there
         * should only be a zero length span at the very start.
         */
        private int checkSpan(String longString, UnicodeSet us, SpanCondition spanCondition)
        {
            int start = 0;
            while (start < longString.Length)
            {
                int limit = us.Span(longString, start, spanCondition);
                if (limit == longString.Length)
                {
                    break;
                }
                else if (limit == start && start != 0)
                {
                    return start;
                }
                start = limit;
                limit = us.Span(longString, start, SpanCondition.NotContained);
                if (limit == start)
                {
                    return start;
                }
                start = limit;
            }
            return -1; // all ok
        }

        private String getCombinations(String[] items, int bitset)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; bitset != 0; ++i)
            {
                int other = bitset & (1 << i);
                if (other != 0)
                {
                    bitset ^= other;
                    result.Append(items[i]);
                }
            }
            return result.ToString();
        }

        [Test]
        public void TestCharSequenceArgs()
        {
            // statics
            assertEquals("CharSequence from", new UnicodeSet("[{abc}]"), UnicodeSet.From(new StringBuilder("abc")));
            assertEquals("CharSequence fromAll", new UnicodeSet("[a-c]"), UnicodeSet.FromAll(new StringBuilder("abc")));
            assertEquals("CharSequence compare", 1.0f, Math.Sign(UnicodeSet.Compare(new StringBuilder("abc"), 0x61)));
            assertEquals("CharSequence compare", -1.0f, Math.Sign(UnicodeSet.Compare(0x61, new StringBuilder("abc"))));
            assertEquals("CharSequence compare", 0.0f, Math.Sign(UnicodeSet.Compare(new StringBuilder("a"), 0x61)));
            assertEquals("CharSequence compare", 0.0f, Math.Sign(UnicodeSet.Compare(0x61, new StringBuilder("a"))));
            assertEquals("CharSequence getSingleCodePoint", 0x1F466, UnicodeSet.GetSingleCodePoint(new StringBuilder("👦")));

            // iterables/arrays
            IEnumerable<StringBuilder> iterable = new List<StringBuilder> { new StringBuilder("A"), new StringBuilder("B") };
            assertEquals("CharSequence containsAll", true, new UnicodeSet("[AB]").ContainsAll(iterable));
            assertEquals("CharSequence containsAll", false, new UnicodeSet("[a-cA]").ContainsAll(iterable));
            assertEquals("CharSequence containsNone", true, new UnicodeSet("[a-c]").ContainsNone(iterable));
            assertEquals("CharSequence containsNone", false, new UnicodeSet("[a-cA]").ContainsNone(iterable));
            assertEquals("CharSequence containsSome", true, new UnicodeSet("[a-cA]").ContainsSome(iterable));
            assertEquals("CharSequence containsSome", false, new UnicodeSet("[a-c]").ContainsSome(iterable));
            assertEquals("CharSequence addAll", new UnicodeSet("[a-cAB]"), new UnicodeSet("[a-cA]").AddAll(new StringBuilder("A"), new StringBuilder("B")));
            assertEquals("CharSequence removeAll", new UnicodeSet("[a-c]"), new UnicodeSet("[a-cA]").RemoveAll(iterable));
            assertEquals("CharSequence retainAll", new UnicodeSet("[A]"), new UnicodeSet("[a-cA]").RetainAll(iterable));

            // UnicodeSet results
            assertEquals("CharSequence add", new UnicodeSet("[Aa-c{abc}{qr}]"), new UnicodeSet("[a-cA{qr}]").Add(new StringBuilder("abc")));
            assertEquals("CharSequence retain", new UnicodeSet("[{abc}]"), new UnicodeSet("[a-cA{abc}{qr}]").Retain(new StringBuilder("abc")));
            assertEquals("CharSequence remove", new UnicodeSet("[Aa-c{qr}]"), new UnicodeSet("[a-cA{abc}{qr}]").Remove(new StringBuilder("abc")));
            assertEquals("CharSequence complement", new UnicodeSet("[Aa-c{qr}]"), new UnicodeSet("[a-cA{abc}{qr}]").Complement(new StringBuilder("abc")));
            assertEquals("CharSequence complement", new UnicodeSet("[Aa-c{abc}{qr}]"), new UnicodeSet("[a-cA{qr}]").Complement(new StringBuilder("abc")));

            assertEquals("CharSequence addAll", new UnicodeSet("[a-cABC]"), new UnicodeSet("[a-cA]").AddAll(new StringBuilder("ABC")));
            assertEquals("CharSequence retainAll", new UnicodeSet("[a-c]"), new UnicodeSet("[a-cA]").RetainAll(new StringBuilder("abcB")));
            assertEquals("CharSequence removeAll", new UnicodeSet("[Aab]"), new UnicodeSet("[a-cA]").RemoveAll(new StringBuilder("cC")));
            assertEquals("CharSequence complementAll", new UnicodeSet("[ABbc]"), new UnicodeSet("[a-cA]").ComplementAll(new StringBuilder("aB")));

            // containment
            assertEquals("CharSequence contains", true, new UnicodeSet("[a-cA{ab}]").Contains(new StringBuilder("ab")));
            assertEquals("CharSequence containsNone", false, new UnicodeSet("[a-cA]").ContainsNone(new StringBuilder("ab")));
            assertEquals("CharSequence containsSome", true, new UnicodeSet("[a-cA{ab}]").ContainsSome(new StringBuilder("ab")));

            // spanning
            assertEquals("CharSequence span", 3, new UnicodeSet("[a-cA]").Span(new StringBuilder("abc"), SpanCondition.Simple));
            assertEquals("CharSequence span", 3, new UnicodeSet("[a-cA]").Span(new StringBuilder("abc"), 1, SpanCondition.Simple));
            assertEquals("CharSequence spanBack", 0, new UnicodeSet("[a-cA]").SpanBack(new StringBuilder("abc"), SpanCondition.Simple));
            assertEquals("CharSequence spanBack", 0, new UnicodeSet("[a-cA]").SpanBack(new StringBuilder("abc"), 1, SpanCondition.Simple));

            // internal
            int outCount;
            assertEquals("CharSequence matchesAt", 2, new UnicodeSet("[a-cA]").MatchesAt(new StringBuilder("abc"), 1));
            assertEquals("CharSequence spanAndCount", 3, new UnicodeSet("[a-cA]").SpanAndCount(new StringBuilder("abc"), 1, SpanCondition.Simple, out outCount));
            assertEquals("CharSequence findIn", 3, new UnicodeSet("[a-cA]").FindIn(new StringBuilder("abc"), 1, true));
            assertEquals("CharSequence findLastIn", -1, new UnicodeSet("[a-cA]").FindLastIn(new StringBuilder("abc"), 1, true));
            assertEquals("CharSequence add", "c", new UnicodeSet("[abA]").StripFrom(new StringBuilder("abc"), true));
        }

        [Test]
        public void TestAStringRange()
        {
            string[][] tests = {
                    new string[] {"[{ax}-{bz}]", "[{ax}{ay}{az}{bx}{by}{bz}]"},
                    new string[] {"[{a}-{c}]", "[a-c]"},
                    //new string[] {"[a-{c}]", "[a-c]"}, // don't handle these yet: enable once we do
                    //new string[] {"[{a}-c]", "[a-c]"}, // don't handle these yet: enable once we do
                    new string[] {"[{ax}-{by}-{cz}]", "Error: '-' not after char, string, or set at \"[{ax}-{by}-{|cz}]\""},
                    new string[] {"[{a}-{bz}]", "Error: Range must have equal-length strings at \"[{a}-{bz}|]\""},
                    new string[] {"[{ax}-{b}]", "Error: Range must have equal-length strings at \"[{ax}-{b}|]\""},
                    new string[] {"[{ax}-bz]", "Error: Invalid range at \"[{ax}-b|z]\""},
                    new string[] {"[ax-{bz}]", "Error: Range must have 2 valid strings at \"[ax-{bz}|]\""},
                    new string[] {"[{bx}-{az}]", "Error: Range must have xᵢ ≤ yᵢ for each index i at \"[{bx}-{az}|]\""},
            };
            int i = 0;
            foreach (String[] test in tests)
            {
                String expected = test[1];
                if (test[1].StartsWith("[", StringComparison.Ordinal))
                {
                    expected = new UnicodeSet(expected).ToPattern(false);
                }
                String actual;
                try
                {
                    actual = new UnicodeSet(test[0]).ToPattern(false);
                }
                catch (Exception e)
                {
                    actual = e.Message;
                }
                assertEquals("StringRange " + i, expected, actual);
                ++i;
            }
        }

        [Test]
        public void TestAddAll_CharacterSequences()
        {
            UnicodeSet unicodeSet = new UnicodeSet();
            unicodeSet.AddAll("a", "b");
            assertEquals("Wrong UnicodeSet pattern", "[ab]", unicodeSet.ToPattern(true));
            unicodeSet.AddAll("b", "x");
            assertEquals("Wrong UnicodeSet pattern", "[abx]", unicodeSet.ToPattern(true));
            unicodeSet.AddAll(new ICharSequence[] { new StringBuilder("foo").ToCharSequence(), new StringBuffer("bar").ToCharSequence() });
            assertEquals("Wrong UnicodeSet pattern", "[abx{bar}{foo}]", unicodeSet.ToPattern(true));
        }

        [Test]
        public void TestCompareTo()
        {
            ISet<String> test_set = new HashSet<string>();
            assertEquals("UnicodeSet not empty", 0, UnicodeSet.Empty.CompareTo(test_set));
            assertEquals("UnicodeSet comparison wrong",
                    0, UnicodeSet.FromAll("a").CompareTo(new string[] { "a" }));

            // Longer is bigger
            assertTrue("UnicodeSet is empty",
                    UnicodeSet.AllCodePoints.CompareTo(test_set) > 0);
            assertTrue("UnicodeSet not empty",
                    UnicodeSet.Empty.CompareTo(new string[] { "a" }) < 0);

            // Equal length compares on first difference.
            assertTrue("UnicodeSet comparison wrong",
                    UnicodeSet.FromAll("a").CompareTo(new string[] { "b" }) < 0);
            assertTrue("UnicodeSet comparison wrong",
                    UnicodeSet.FromAll("ab").CompareTo(new string[] { "a", "c" }) < 0);
            assertTrue("UnicodeSet comparison wrong",
                    UnicodeSet.FromAll("b").CompareTo(new string[] { "a" }) > 0);
        }

        [Test]
        public void TestUnusedCcc()
        {
            // All numeric ccc values 0..255 are valid, but many are unused.
            UnicodeSet ccc2 = new UnicodeSet("[:ccc=2:]");
            assertTrue("[:ccc=2:] -> empty set", ccc2.IsEmpty);

            UnicodeSet ccc255 = new UnicodeSet("[:ccc=255:]");
            assertTrue("[:ccc=255:] -> empty set", ccc255.IsEmpty);

            // Non-integer values and values outside 0..255 are invalid.
            try
            {
                new UnicodeSet("[:ccc=-1:]");
                fail("[:ccc=-1:] -> illegal argument");
            }
            catch (ArgumentException expected)
            {
            }

            try
            {
                new UnicodeSet("[:ccc=256:]");
                fail("[:ccc=256:] -> illegal argument");
            }
            catch (ArgumentException expected)
            {
            }

            try
            {
                new UnicodeSet("[:ccc=1.1:]");
                fail("[:ccc=1.1:] -> illegal argument");
            }
            catch (ArgumentException expected)
            {
            }
        }
    }
}
