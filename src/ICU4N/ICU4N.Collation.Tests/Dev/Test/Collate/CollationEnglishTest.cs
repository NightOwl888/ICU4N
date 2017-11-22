using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Globalization;

namespace ICU4N.Dev.Test.Collate
{
    /// <summary>
    /// Port From:   ICU4C v2.1 : Collate/CollationEnglishTest
    /// Source File: $ICU4CRoot/source/test/intltest/encoll.cpp
    /// </summary>
    public class CollationEnglishTest : TestFmwk
    {
        private static char[][] testSourceCases = {
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */},
            new char[] {(char)0x0062 /* 'b' */, (char)0x006C /* 'l' */, (char)0x0061 /* 'a' */, (char)0x0063 /* 'c' */, (char)0x006B /* 'k' */, (char)0x002D /* '-' */, (char)0x0062 /* 'b' */, (char)0x0069 /* 'i' */, (char)0x0072 /* 'r' */, (char)0x0064 /* 'd' */},
            new char[] {(char)0x0062 /* 'b' */, (char)0x006C /* 'l' */, (char)0x0061 /* 'a' */, (char)0x0063 /* 'c' */, (char)0x006B /* 'k' */, (char)0x0020 /* ' ' */, (char)0x0062 /* 'b' */, (char)0x0069 /* 'i' */, (char)0x0072 /* 'r' */, (char)0x0064 /* 'd' */},
            new char[] {(char)0x0062 /* 'b' */, (char)0x006C /* 'l' */, (char)0x0061 /* 'a' */, (char)0x0063 /* 'c' */, (char)0x006B /* 'k' */, (char)0x002D /* '-' */, (char)0x0062 /* 'b' */, (char)0x0069 /* 'i' */, (char)0x0072 /* 'r' */, (char)0x0064 /* 'd' */},
            new char[] {(char)0x0048 /* 'H' */, (char)0x0065 /* 'e' */, (char)0x006C /* 'l' */, (char)0x006C /* 'l' */, (char)0x006F /* 'o' */},
            new char[] {(char)0x0041 /* 'A' */, (char)0x0042 /* 'B' */, (char)0x0043 /* 'C' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0062 /* 'b' */, (char)0x006C /* 'l' */, (char)0x0061 /* 'a' */, (char)0x0063 /* 'c' */, (char)0x006B /* 'k' */, (char)0x0062 /* 'b' */, (char)0x0069 /* 'i' */, (char)0x0072 /* 'r' */, (char)0x0064 /* 'd' */},
            new char[] {(char)0x0062 /* 'b' */, (char)0x006C /* 'l' */, (char)0x0061 /* 'a' */, (char)0x0063 /* 'c' */, (char)0x006B /* 'k' */, (char)0x002D /* '-' */, (char)0x0062 /* 'b' */, (char)0x0069 /* 'i' */, (char)0x0072 /* 'r' */, (char)0x0064 /* 'd' */},
            new char[] {(char)0x0062 /* 'b' */, (char)0x006C /* 'l' */, (char)0x0061 /* 'a' */, (char)0x0063 /* 'c' */, (char)0x006B /* 'k' */, (char)0x002D /* '-' */, (char)0x0062 /* 'b' */, (char)0x0069 /* 'i' */, (char)0x0072 /* 'r' */, (char)0x0064 /* 'd' */},
            new char[] {(char)0x0070 /* 'p' */, (char)0x00EA, (char)0x0063 /* 'c' */, (char)0x0068 /* 'h' */, (char)0x0065 /* 'e' */},
            new char[] {(char)0x0070 /* 'p' */, (char)0x00E9, (char)0x0063 /* 'c' */, (char)0x0068 /* 'h' */, (char)0x00E9},
            new char[] {(char)0x00C4, (char)0x0042 /* 'B' */, (char)0x0308, (char)0x0043 /* 'C' */, (char)0x0308},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0308, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0070 /* 'p' */, (char)0x00E9, (char)0x0063 /* 'c' */, (char)0x0068 /* 'h' */, (char)0x0065 /* 'e' */, (char)0x0072 /* 'r' */},
            new char[] {(char)0x0072 /* 'r' */, (char)0x006F /* 'o' */, (char)0x006C /* 'l' */, (char)0x0065 /* 'e' */, (char)0x0073 /* 's' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0041 /* 'A' */},
            new char[] {(char)0x0041 /* 'A' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */},
            new char[] {(char)0x0074 /* 't' */, (char)0x0063 /* 'c' */, (char)0x006F /* 'o' */, (char)0x006D /* 'm' */, (char)0x0070 /* 'p' */, (char)0x0061 /* 'a' */, (char)0x0072 /* 'r' */, (char)0x0065 /* 'e' */, (char)0x0070 /* 'p' */, (char)0x006C /* 'l' */, (char)0x0061 /* 'a' */, (char)0x0069 /* 'i' */, (char)0x006E /* 'n' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0023 /* '#' */, (char)0x0062 /* 'b' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0023 /* '#' */, (char)0x0062 /* 'b' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0041 /* 'A' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */, (char)0x0064 /* 'd' */, (char)0x0061 /* 'a' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */, (char)0x0064 /* 'd' */, (char)0x0061 /* 'a' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */, (char)0x0064 /* 'd' */, (char)0x0061 /* 'a' */},
            new char[] {(char)0x00E6, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */, (char)0x0064 /* 'd' */, (char)0x0061 /* 'a' */},
            new char[] {(char)0x00E4, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */, (char)0x0064 /* 'd' */, (char)0x0061 /* 'a' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0063 /* 'c' */, (char)0x0048 /* 'H' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0308, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0074 /* 't' */, (char)0x0068 /* 'h' */, (char)0x0069 /* 'i' */, (char)0x0302, (char)0x0073 /* 's' */},
            new char[] {(char)0x0070 /* 'p' */, (char)0x00EA, (char)0x0063 /* 'c' */, (char)0x0068 /* 'h' */, (char)0x0065 /* 'e' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x00E6, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x00E6, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0070 /* 'p' */, (char)0x00E9, (char)0x0063 /* 'c' */, (char)0x0068 /* 'h' */, (char)0x00E9}                                            // 49
        };

        private static char[][] testTargetCases = {
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0062 /* 'b' */, (char)0x006C /* 'l' */, (char)0x0061 /* 'a' */, (char)0x0063 /* 'c' */, (char)0x006B /* 'k' */, (char)0x0062 /* 'b' */, (char)0x0069 /* 'i' */, (char)0x0072 /* 'r' */, (char)0x0064 /* 'd' */},
            new char[] {(char)0x0062 /* 'b' */, (char)0x006C /* 'l' */, (char)0x0061 /* 'a' */, (char)0x0063 /* 'c' */, (char)0x006B /* 'k' */, (char)0x002D /* '-' */, (char)0x0062 /* 'b' */, (char)0x0069 /* 'i' */, (char)0x0072 /* 'r' */, (char)0x0064 /* 'd' */},
            new char[] {(char)0x0062 /* 'b' */, (char)0x006C /* 'l' */, (char)0x0061 /* 'a' */, (char)0x0063 /* 'c' */, (char)0x006B /* 'k' */},
            new char[] {(char)0x0068 /* 'h' */, (char)0x0065 /* 'e' */, (char)0x006C /* 'l' */, (char)0x006C /* 'l' */, (char)0x006F /* 'o' */},
            new char[] {(char)0x0041 /* 'A' */, (char)0x0042 /* 'B' */, (char)0x0043 /* 'C' */},
            new char[] {(char)0x0041 /* 'A' */, (char)0x0042 /* 'B' */, (char)0x0043 /* 'C' */},
            new char[] {(char)0x0062 /* 'b' */, (char)0x006C /* 'l' */, (char)0x0061 /* 'a' */, (char)0x0063 /* 'c' */, (char)0x006B /* 'k' */, (char)0x0062 /* 'b' */, (char)0x0069 /* 'i' */, (char)0x0072 /* 'r' */, (char)0x0064 /* 'd' */, (char)0x0073 /* 's' */},
            new char[] {(char)0x0062 /* 'b' */, (char)0x006C /* 'l' */, (char)0x0061 /* 'a' */, (char)0x0063 /* 'c' */, (char)0x006B /* 'k' */, (char)0x0062 /* 'b' */, (char)0x0069 /* 'i' */, (char)0x0072 /* 'r' */, (char)0x0064 /* 'd' */, (char)0x0073 /* 's' */},
            new char[] {(char)0x0062 /* 'b' */, (char)0x006C /* 'l' */, (char)0x0061 /* 'a' */, (char)0x0063 /* 'c' */, (char)0x006B /* 'k' */, (char)0x0062 /* 'b' */, (char)0x0069 /* 'i' */, (char)0x0072 /* 'r' */, (char)0x0064 /* 'd' */},
            new char[] {(char)0x0070 /* 'p' */, (char)0x00E9, (char)0x0063 /* 'c' */, (char)0x0068 /* 'h' */, (char)0x00E9},
            new char[] {(char)0x0070 /* 'p' */, (char)0x00E9, (char)0x0063 /* 'c' */, (char)0x0068 /* 'h' */, (char)0x0065 /* 'e' */, (char)0x0072 /* 'r' */},
            new char[] {(char)0x00C4, (char)0x0042 /* 'B' */, (char)0x0308, (char)0x0043 /* 'C' */, (char)0x0308},
            new char[] {(char)0x0041 /* 'A' */, (char)0x0308, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0070 /* 'p' */, (char)0x00E9, (char)0x0063 /* 'c' */, (char)0x0068 /* 'h' */, (char)0x0065 /* 'e' */},
            new char[] {(char)0x0072 /* 'r' */, (char)0x006F /* 'o' */, (char)0x0302, (char)0x006C /* 'l' */, (char)0x0065 /* 'e' */},
            new char[] {(char)0x0041 /* 'A' */, (char)0x00E1, (char)0x0063 /* 'c' */, (char)0x0064 /* 'd' */},
            new char[] {(char)0x0041 /* 'A' */, (char)0x00E1, (char)0x0063 /* 'c' */, (char)0x0064 /* 'd' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0054 /* 'T' */, (char)0x0043 /* 'C' */, (char)0x006F /* 'o' */, (char)0x006D /* 'm' */, (char)0x0070 /* 'p' */, (char)0x0061 /* 'a' */, (char)0x0072 /* 'r' */, (char)0x0065 /* 'e' */, (char)0x0050 /* 'P' */, (char)0x006C /* 'l' */, (char)0x0061 /* 'a' */, (char)0x0069 /* 'i' */, (char)0x006E /* 'n' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0042 /* 'B' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0023 /* '#' */, (char)0x0042 /* 'B' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0026 /* '&' */, (char)0x0062 /* 'b' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0023 /* '#' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */, (char)0x0064 /* 'd' */, (char)0x0061 /* 'a' */},
            new char[] {(char)0x00C4, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */, (char)0x0064 /* 'd' */, (char)0x0061 /* 'a' */},
            new char[] {(char)0x00E4, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */, (char)0x0064 /* 'd' */, (char)0x0061 /* 'a' */},
            new char[] {(char)0x00C4, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */, (char)0x0064 /* 'd' */, (char)0x0061 /* 'a' */},
            new char[] {(char)0x00C4, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */, (char)0x0064 /* 'd' */, (char)0x0061 /* 'a' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0023 /* '#' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x003D /* '=' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0064 /* 'd' */},
            new char[] {(char)0x00E4, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0043 /* 'C' */, (char)0x0048 /* 'H' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x00E4, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0074 /* 't' */, (char)0x0068 /* 'h' */, (char)0x00EE, (char)0x0073 /* 's' */},
            new char[] {(char)0x0070 /* 'p' */, (char)0x00E9, (char)0x0063 /* 'c' */, (char)0x0068 /* 'h' */, (char)0x00E9},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0042 /* 'B' */, (char)0x0043 /* 'C' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0062 /* 'b' */, (char)0x0064 /* 'd' */},
            new char[] {(char)0x00E4, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x00C6, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0042 /* 'B' */, (char)0x0064 /* 'd' */},
            new char[] {(char)0x00E4, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x00C6, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0042 /* 'B' */, (char)0x0064 /* 'd' */},
            new char[] {(char)0x00E4, (char)0x0062 /* 'b' */, (char)0x0063 /* 'c' */},
            new char[] {(char)0x0070 /* 'p' */, (char)0x00EA, (char)0x0063 /* 'c' */, (char)0x0068 /* 'h' */, (char)0x0065 /* 'e' */}
        };                                           // 49

        private static int[] results = {
            //-1:LESS; 0:EQUAL; 1:GREATER
                -1,
                -1, /*Collator::GREATER,*/
                -1,
                1,
                1,
                0,
                -1,
                -1,
                -1,
                -1, /*Collator::GREATER,*/                                                          /* 10 */
                1,
                -1,
                0,
                -1,
                1,
                1,
                1,
                -1,
                -1,
                -1,                                                             /* 20 */
                -1,
                -1,
                -1,
                1,
                1,
                1,
                /* Test Tertiary  > 26 */
                -1,
                -1,
                1,
                -1,                                                             /* 30 */
                1,
                0,
                1,
                -1,
                -1,
                -1,
                /* test identical > 36 */
                0,
                0,
                /* test primary > 38 */
                0,
                0,                                                            /* 40 */
                -1,
                0,
                0,
                /* test secondary > 43 */
                -1,
                -1,
                0,
                -1,
                -1,
                -1                                                                  // 49
            };

        private static char[][] testBugs = {
            new char[] {(char)0x61},
            new char[] {(char)0x41},
            new char[] {(char)0x65},
            new char[] {(char)0x45},
            new char[] {(char)0x00e9},
            new char[] {(char)0x00e8},
            new char[] {(char)0x00ea},
            new char[] {(char)0x00eb},
            new char[] {(char)0x65, (char)0x61},
            new char[] {(char)0x78}
        };

        // (char)0x0300 is grave, (char)0x0301 is acute
        // the order of elements in this array must be different than the order in CollationFrenchTest
        private static char[][] testAcute = {
            new char[] {(char)0x65, (char)0x65},
            new char[] {(char)0x65, (char)0x65, (char)0x0301},
            new char[] {(char)0x65, (char)0x65, (char)0x0301, (char)0x0300},
            new char[] {(char)0x65, (char)0x65, (char)0x0300},
            new char[] {(char)0x65, (char)0x65, (char)0x0300, (char)0x0301},
            new char[] {(char)0x65, (char)0x0301, (char)0x65},
            new char[] {(char)0x65, (char)0x0301, (char)0x65, (char)0x0301},
            new char[] {(char)0x65, (char)0x0301, (char)0x65, (char)0x0301, (char)0x0300},
            new char[] {(char)0x65, (char)0x0301, (char)0x65, (char)0x0300},
            new char[] {(char)0x65, (char)0x0301, (char)0x65, (char)0x0300, (char)0x0301},
            new char[] {(char)0x65, (char)0x0301, (char)0x0300, (char)0x65},
            new char[] {(char)0x65, (char)0x0301, (char)0x0300, (char)0x65, (char)0x0301},
            new char[] {(char)0x65, (char)0x0301, (char)0x0300, (char)0x65, (char)0x0301, (char)0x0300},
            new char[] {(char)0x65, (char)0x0301, (char)0x0300, (char)0x65, (char)0x0300},
            new char[] {(char)0x65, (char)0x0301, (char)0x0300, (char)0x65, (char)0x0300, (char)0x0301},
            new char[] {(char)0x65, (char)0x0300, (char)0x65},
            new char[] {(char)0x65, (char)0x0300, (char)0x65, (char)0x0301},
            new char[] {(char)0x65, (char)0x0300, (char)0x65, (char)0x0301, (char)0x0300},
            new char[] {(char)0x65, (char)0x0300, (char)0x65, (char)0x0300},
            new char[] {(char)0x65, (char)0x0300, (char)0x65, (char)0x0300, (char)0x0301},
            new char[] {(char)0x65, (char)0x0300, (char)0x0301, (char)0x65},
            new char[] {(char)0x65, (char)0x0300, (char)0x0301, (char)0x65, (char)0x0301},
            new char[] {(char)0x65, (char)0x0300, (char)0x0301, (char)0x65, (char)0x0301, (char)0x0300},
            new char[] {(char)0x65, (char)0x0300, (char)0x0301, (char)0x65, (char)0x0300},
            new char[] {(char)0x65, (char)0x0300, (char)0x0301, (char)0x65, (char)0x0300, (char)0x0301}
        };

        private static char[][] testMore = {
            new char[] {(char)0x0061 /* 'a' */, (char)0x0065 /* 'e' */},
            new char[] { (char)0x00E6},
            new char[] { (char)0x00C6},
            new char[] {(char)0x0061 /* 'a' */, (char)0x0066 /* 'f' */},
            new char[] {(char)0x006F /* 'o' */, (char)0x0065 /* 'e' */},
            new char[] { (char)0x0153},
            new char[] { (char)0x0152},
            new char[] {(char)0x006F /* 'o' */, (char)0x0066 /* 'f' */},
        };

        private Collator myCollation = null;

        public CollationEnglishTest()
        {
        }

        [SetUp]
        public void Init()
        {
            myCollation = Collator.GetInstance(new CultureInfo("en"));
        }

        //performs test with strength PRIMARY
        [Test]
        public void TestPrimary()
        {
            int i;
            myCollation.Strength = CollationStrength.Primary;
            for (i = 38; i < 43; i++)
            {
                DoTest(testSourceCases[i], testTargetCases[i], results[i]);
            }
        }

        //perform test with strength SECONDARY
        [Test]
        public void TestSecondary()
        {
            int i;
            myCollation.Strength = CollationStrength.Secondary;
            for (i = 43; i < 49; i++)
            {
                DoTest(testSourceCases[i], testTargetCases[i], results[i]);
            }

            //test acute and grave ordering (compare to french collation)
            int j;
            int expected;
            for (i = 0; i < testAcute.Length; i++)
            {
                for (j = 0; j < testAcute.Length; j++)
                {
                    Logln("i = " + i + "; j = " + j);
                    if (i < j)
                        expected = -1;
                    else if (i == j)
                        expected = 0;
                    else // (i >  j)
                        expected = 1;
                    DoTest(testAcute[i], testAcute[j], expected);
                }
            }
        }

        //perform test with strength TERTIARY
        [Test]
        public void TestTertiary()
        {
            int i = 0;
            myCollation.Strength = CollationStrength.Tertiary;// (Collator.TERTIARY);
                                                              //for (i = 0; i < 38 ; i++)  //attention: there is something wrong with 36, 37.
            for (i = 0; i < 38; i++)
            {
                DoTest(testSourceCases[i], testTargetCases[i], results[i]);
            }

            int j = 0;
            for (i = 0; i < 10; i++)
            {
                for (j = i + 1; j < 10; j++)
                {
                    DoTest(testBugs[i], testBugs[j], -1);
                }
            }

            //test more interesting cases
            int expected;
            for (i = 0; i < testMore.Length; i++)
            {
                for (j = 0; j < testMore.Length; j++)
                {
                    if (i < j)
                        expected = -1;
                    else if (i == j)
                        expected = 0;
                    else // (i >  j)
                        expected = 1;
                    DoTest(testMore[i], testMore[j], expected);
                }
            }
        }

        // main test routine, tests rules defined by the "en" locale
        private void DoTest(char[] source, char[] target, int result)
        {

            String s = new String(source);
            String t = new String(target);
            int compareResult = myCollation.Compare(s, t);
            SortKey sortKey1, sortKey2;
            sortKey1 = myCollation.GetSortKey(s);
            sortKey2 = myCollation.GetSortKey(t);
            int keyResult = sortKey1.CompareTo(sortKey2);
            ReportCResult(s, t, sortKey1, sortKey2, compareResult, keyResult, compareResult, result);

        }

        private void ReportCResult(String source, String target, SortKey sourceKey, SortKey targetKey,
                                    int compareResult, int keyResult, int incResult, int expectedResult)
        {
            if (expectedResult < -1 || expectedResult > 1)
            {
                Errln("***** invalid call to reportCResult ****");
                return;
            }

            bool ok1 = (compareResult == expectedResult);
            bool ok2 = (keyResult == expectedResult);
            bool ok3 = (incResult == expectedResult);

            if (ok1 && ok2 && ok3 && !IsVerbose())
            {
                return;
            }
            else
            {
                String msg1 = ok1 ? "Ok: compare(\"" : "FAIL: compare(\"";
                String msg2 = "\", \"";
                String msg3 = "\") returned ";
                String msg4 = "; expected ";

                String sExpect = "";
                String sResult = "";
                sResult = CollationTest.AppendCompareResult(compareResult, sResult);
                sExpect = CollationTest.AppendCompareResult(expectedResult, sExpect);
                if (ok1)
                {
                    Logln(msg1 + source + msg2 + target + msg3 + sResult);
                }
                else
                {
                    Errln(msg1 + source + msg2 + target + msg3 + sResult + msg4 + sExpect);
                }

                msg1 = ok2 ? "Ok: key(\"" : "FAIL: key(\"";
                msg2 = "\").compareTo(key(\"";
                msg3 = "\")) returned ";
                sResult = CollationTest.AppendCompareResult(keyResult, sResult);
                if (ok2)
                {
                    Logln(msg1 + source + msg2 + target + msg3 + sResult);
                }
                else
                {
                    Errln(msg1 + source + msg2 + target + msg3 + sResult + msg4 + sExpect);
                    msg1 = "  ";
                    msg2 = " vs. ";
                    Errln(msg1 + CollationTest.Prettify(sourceKey) + msg2 + CollationTest.Prettify(targetKey));
                }

                msg1 = ok3 ? "Ok: incCompare(\"" : "FAIL: incCompare(\"";
                msg2 = "\", \"";
                msg3 = "\") returned ";

                sResult = CollationTest.AppendCompareResult(incResult, sResult);

                if (ok3)
                {
                    Logln(msg1 + source + msg2 + target + msg3 + sResult);
                }
                else
                {
                    Errln(msg1 + source + msg2 + target + msg3 + sResult + msg4 + sExpect);
                }
            }
        }
    }
}
