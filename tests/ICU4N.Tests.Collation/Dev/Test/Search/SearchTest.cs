using ICU4N.Globalization;
using ICU4N.Text;
using ICU4N.Util;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Text;
using StringBuffer = System.Text.StringBuilder;


namespace ICU4N.Dev.Test.Search
{
    public class SearchTest : TestFmwk
    {
        //inner class
        internal class SearchData
        {
            internal SearchData(String text, String pattern,
                        String coll, CollationStrength strength, ElementComparisonType cmpType, String breaker,
                        int[] offset, int[] size)
            {

                this.text = text;
                this.pattern = pattern;
                this.collator = coll;
                this.strength = strength;
                this.cmpType = cmpType;
                this.breaker = breaker;
                this.offset = offset;
                this.size = size;
            }
            internal String text;
            internal String pattern;
            internal String collator;
            internal CollationStrength strength;
            internal ElementComparisonType cmpType;
            internal String breaker;
            internal int[] offset;
            internal int[] size;
        }

        RuleBasedCollator m_en_us_;
        RuleBasedCollator m_fr_fr_;
        RuleBasedCollator m_de_;
        RuleBasedCollator m_es_;
        BreakIterator m_en_wordbreaker_;
        BreakIterator m_en_characterbreaker_;

        // Just calling SearchData constructor, to make the test data source code
        // nice and short
        private static SearchData SD(String text, String pattern, String coll, CollationStrength strength,
                        ElementComparisonType cmpType, String breaker, int[] offset, int[] size)
        {
            return new SearchData(text, pattern, coll, strength, cmpType, breaker, offset, size);
        }

        // Just returning int[], to make the test data nice and short
        private static int[] IA(params int[] elements)
        {
            return elements;
        }

        static SearchData[] BASIC = {
            SD("xxxxxxxxxxxxxxxxxxxx", "fisher", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("silly spring string", "string", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(13, -1), IA(6)),
            SD("silly spring string string", "string", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(13, 20, -1), IA(6, 6)),
            SD("silly string spring string", "string", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(6, 20, -1), IA(6, 6)),
            SD("string spring string", "string", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, 14, -1), IA(6, 6)),
            SD("Scott Ganyo", "c", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(1, -1), IA(1)),
            SD("Scott Ganyo", " ", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(5, -1), IA(1)),
            SD("\u0300\u0325", "\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("a\u0300\u0325", "\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("a\u0300\u0325", "\u0300\u0325", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("a\u0300b", "\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("\u00c9", "e", null, CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(1)),
        };

        SearchData[] BREAKITERATOREXACT = {
            SD("foxy fox", "fox", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, "characterbreaker", IA(0, 5, -1), IA(3, 3)),
            SD("foxy fox", "fox", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, "wordbreaker", IA(5, -1), IA(3)),
            SD("This is a toe T\u00F6ne", "toe", "de", CollationStrength.Primary, ElementComparisonType.StandardElementComparison, "characterbreaker", IA(10, 14, -1), IA(3, 2)),
            SD("This is a toe T\u00F6ne", "toe", "de", CollationStrength.Primary, ElementComparisonType.StandardElementComparison, "wordbreaker", IA(10, -1), IA(3)),
            SD("Channel, another channel, more channels, and one last Channel", "Channel", "es", CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, "wordbreaker", IA(0, 54, -1), IA(7, 7)),
            /* jitterbug 1745 */
            SD("testing that \u00e9 does not match e", "e", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, "characterbreaker", IA(1, 17, 30, -1), IA(1, 1, 1)),
            SD("testing that string ab\u00e9cd does not match e", "e", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, "characterbreaker", IA(1, 28, 41, -1), IA(1, 1, 1)),
            SD("\u00c9", "e", "fr", CollationStrength.Primary, ElementComparisonType.StandardElementComparison, "characterbreaker", IA(0, -1), IA(1)),
        };

        SearchData[] BREAKITERATORCANONICAL = {
            SD("foxy fox", "fox", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, "characterbreaker", IA(0, 5, -1), IA(3, 3)),
            SD("foxy fox", "fox", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, "wordbreaker", IA(5, -1), IA(3)),
            SD("This is a toe T\u00F6ne", "toe", "de", CollationStrength.Primary, ElementComparisonType.StandardElementComparison, "characterbreaker", IA(10, 14, -1), IA(3, 2)),
            SD("This is a toe T\u00F6ne", "toe", "de", CollationStrength.Primary, ElementComparisonType.StandardElementComparison, "wordbreaker", IA(10, -1), IA(3)),
            SD("Channel, another channel, more channels, and one last Channel", "Channel", "es", CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, "wordbreaker", IA(0, 54, -1), IA(7, 7)),
            /* jitterbug 1745 */
            SD("testing that \u00e9 does not match e", "e", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, "characterbreaker", IA(1, 17, 30, -1), IA(1, 1, 1)),
            SD("testing that string ab\u00e9cd does not match e", "e", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, "characterbreaker", IA(1, 28, 41, -1), IA(1, 1, 1)),
            SD("\u00c9", "e", "fr", CollationStrength.Primary, ElementComparisonType.StandardElementComparison, "characterbreaker", IA(0, -1), IA(1)),
        };

        SearchData[] BASICCANONICAL = {
            SD("xxxxxxxxxxxxxxxxxxxx", "fisher", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("silly spring string", "string", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(13, -1), IA(6)),
            SD("silly spring string string", "string", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(13, 20, -1), IA(6, 6)),
            SD("silly string spring string", "string", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(6, 20, -1), IA(6, 6)),
            SD("string spring string", "string", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, 14, -1), IA(6, 6)),
            SD("Scott Ganyo", "c", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(1, -1), IA(1)),
            SD("Scott Ganyo", " ", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(5, -1), IA(1)),

            SD("\u0300\u0325", "\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("a\u0300\u0325", "\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("a\u0300\u0325", "\u0300\u0325", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("a\u0300b", "\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("a\u0300\u0325b", "\u0300b", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("\u0325\u0300A\u0325\u0300", "\u0300A\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("\u0325\u0300A\u0325\u0300", "\u0325A\u0325", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("a\u0300\u0325b\u0300\u0325c \u0325b\u0300 \u0300b\u0325", "\u0300b\u0325", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            SD("\u00c4\u0323", "A\u0323\u0308", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(2)),
            SD("\u0308\u0323", "\u0323\u0308", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(2)),
        };

        SearchData[] COLLATOR = {
            /* english */
            SD("fox fpx", "fox", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(3)),
            /* tailored */
            SD("fox fpx", "fox", null, CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(0, 4, -1), IA(3, 3)),
        };

        String TESTCOLLATORRULE = "& o,O ; p,P";
        String EXTRACOLLATIONRULE = " & ae ; \u00e4 & AE ; \u00c4 & oe ; \u00f6 & OE ; \u00d6 & ue ; \u00fc & UE ; \u00dc";

        SearchData[] COLLATORCANONICAL = {
            /* english */
            SD("fox fpx", "fox", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(3)),
            /* tailored */
            SD("fox fpx", "fox", null, CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(0, 4, -1), IA(3, 3)),
        };

        SearchData[] COMPOSITEBOUNDARIES = {
            SD("\u00C0", "A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("A\u00C0C", "A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(1)),
            SD("\u00C0A", "A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(1, -1), IA(1)),
            SD("B\u00C0", "A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("\u00C0B", "A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("\u00C0", "\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            /* first one matches only because it's at the start of the text */
            SD("\u0300\u00C0", "\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(1)),

            /* \\u0300 blocked by \\u0300 */
            SD("\u00C0\u0300", "\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            /* A + 030A + 0301 */
            SD("\u01FA", "\u01FA", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(1)),
            SD("\u01FA", "A\u030A\u0301", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(1)),

            SD("\u01FA", "\u030A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("\u01FA", "A\u030A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            SD("\u01FA", "\u030AA", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            SD("\u01FA", "\u0301", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            /* blocked accent */
            SD("\u01FA", "A\u0301", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("\u01FA", "\u0301A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            SD("\u01FA", "\u030A\u0301", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("A\u01FA", "A\u030A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("\u01FAA", "\u0301A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            SD("\u0F73", "\u0F73", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(1)),

            SD("\u0F73", "\u0F71", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("\u0F73", "\u0F72", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            SD("\u0F73", "\u0F71\u0F72", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(1)),

            SD("A\u0F73", "A\u0F71", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("\u0F73A", "\u0F72A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("\u01FA A\u0301\u030A A\u030A\u0301 A\u030A \u01FA", "A\u030A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(10, -1), IA(2)),
        };

        SearchData[] COMPOSITEBOUNDARIESCANONICAL = {
            SD("\u00C0", "A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("A\u00C0C", "A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(1)),
            SD("\u00C0A", "A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(1, -1), IA(1)),
            SD("B\u00C0", "A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("\u00C0B", "A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("\u00C0", "\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            /* first one matches only because it's at the start of the text */
            SD("\u0300\u00C0", "\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(1)),

            /* \u0300 blocked by \u0300 */
            SD("\u00C0\u0300", "\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            /* A + 030A + 0301 */
            SD("\u01FA", "\u01FA", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(1)),
            SD("\u01FA", "A\u030A\u0301", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(1)),

            SD("\u01FA", "\u030A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("\u01FA", "A\u030A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            SD("\u01FA", "\u030AA", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            SD("\u01FA", "\u0301", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            /* blocked accent */
            SD("\u01FA", "A\u0301", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("\u01FA", "\u0301A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            SD("\u01FA", "\u030A\u0301", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("A\u01FA", "A\u030A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("\u01FAA", "\u0301A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            SD("\u0F73", "\u0F73", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(1)),

            SD("\u0F73", "\u0F71", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("\u0F73", "\u0F72", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            SD("\u0F73", "\u0F71\u0F72", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(1)),

            SD("A\u0F73", "A\u0F71", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("\u0F73A", "\u0F72A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            SD("\u01FA A\u0301\u030A A\u030A\u0301 A\u030A \u01FA", "A\u030A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(10, -1), IA(2)),
        };

        SearchData[] SUPPLEMENTARY = {
            SD("abc \uD800\uDC00 \uD800\uDC01 \uD801\uDC00 \uD800\uDC00abc abc\uD800\uDC00 \uD800\uD800\uDC00 \uD800\uDC00\uDC00",
                    "\uD800\uDC00", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(4, 13, 22, 26, 29, -1), IA(2, 2, 2, 2, 2)),
            SD("and\uD834\uDDB9this sentence", "\uD834\uDDB9", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(3, -1), IA(2)),
            SD("and \uD834\uDDB9 this sentence", " \uD834\uDDB9 ", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(3, -1), IA(4)),
            SD("and-\uD834\uDDB9-this sentence", "-\uD834\uDDB9-", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(3, -1), IA(4)),
            SD("and,\uD834\uDDB9,this sentence", ",\uD834\uDDB9,", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(3, -1), IA(4)),
            SD("and?\uD834\uDDB9?this sentence", "?\uD834\uDDB9?", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(3, -1), IA(4)),
        };

        String CONTRACTIONRULE = "&z = ab/c < AB < X\u0300 < ABC < X\u0300\u0315";

        SearchData[] CONTRACTION = {
            /* common discontiguous */
            SD("A\u0300\u0315", "\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            SD("A\u0300\u0315", "\u0300\u0315", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            /* contraction prefix */
            SD("AB\u0315C", "A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            SD("AB\u0315C", "AB", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("AB\u0315C", "\u0315", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            /*
             * discontiguous problem here for backwards iteration. accents not found because discontiguous stores all
             * information
             */
            SD("X\u0300\u0319\u0315", "\u0319", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            /* ends not with a contraction character */
            SD("X\u0315\u0300D", "\u0300\u0315", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("X\u0315\u0300D", "X\u0300\u0315", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(3)),
            SD("X\u0300\u031A\u0315D", "X\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            /* blocked discontiguous */
            SD("X\u0300\u031A\u0315D", "\u031A\u0315D", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            /*
             * "ab" generates a contraction that's an expansion. The "z" matches the first CE of the expansion but the
             * match fails because it ends in the middle of an expansion...
             */
            SD("ab", "z", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
        };

        SearchData[] CONTRACTIONCANONICAL = {
            /* common discontiguous */
            SD("A\u0300\u0315", "\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("A\u0300\u0315", "\u0300\u0315", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            /* contraction prefix */
            SD("AB\u0315C", "A", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            SD("AB\u0315C", "AB", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("AB\u0315C", "\u0315", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            /*
             * discontiguous problem here for backwards iteration. forwards gives 0, 4 but backwards give 1, 3
             */
            /*
             * {"X\u0300\u0319\u0315", "\u0319", null, CollationStrength.Tertiary, SearchIteratorElementComparisonType.StandardElementComparison, null, {0, -1), {4}),
             */

            /* ends not with a contraction character */
            SD("X\u0315\u0300D", "\u0300\u0315", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("X\u0315\u0300D", "X\u0300\u0315", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(3)),

            SD("X\u0300\u031A\u0315D", "X\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            /* blocked discontiguous */
            SD("X\u0300\u031A\u0315D", "\u031A\u0315D", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),

            /*
             * "ab" generates a contraction that's an expansion. The "z" matches the first CE of the expansion but the
             * match fails because it ends in the middle of an expansion...
             */
            SD("ab", "z", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(2)),
        };

        SearchData[] MATCH = {
            SD("a busy bee is a very busy beeee", "bee", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(7, 26, -1), IA(3, 3)),
            /*  012345678901234567890123456789012345678901234567890 */
            SD("a busy bee is a very busy beeee with no bee life", "bee", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(7, 26, 40, -1), IA(3, 3, 3)),
        };

        String IGNORABLERULE = "&a = \u0300";

        SearchData[] IGNORABLE = {
            /*
             * This isn't much of a test when matches have to be on grapheme boundiaries. The match at 0 only works because it's
             * at the start of the text.
             */
            SD("\u0300\u0315 \u0300\u0315 ", "\u0300", null, CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(2)),
        };

        SearchData[] DIACTRICMATCH = {
            SD("\u0061\u0061\u00E1", "\u0061\u00E1", null, CollationStrength.Secondary, ElementComparisonType.StandardElementComparison, null, IA(1, -1), IA(2)),
            SD("\u0020\u00C2\u0303\u0020\u0041\u0061\u1EAA\u0041\u0302\u0303\u00C2\u0303\u1EAB\u0061\u0302\u0303\u00E2\u0303\uD806\uDC01\u0300\u0020", "\u00C2\u0303",
                null, CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(1, 4, 5, 6, 7, 10, 12, 13, 16, -1), IA(2, 1, 1, 1, 3, 2, 1, 3, 2)),
            SD("\u03BA\u03B1\u03B9\u0300\u0020\u03BA\u03B1\u1F76", "\u03BA\u03B1\u03B9", null, CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(0, 5, -1), IA(4, 3)),
        };

        SearchData[] NORMCANONICAL = {
            SD("\u0300\u0325", "\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("\u0300\u0325", "\u0325", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("a\u0300\u0325", "\u0325\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("a\u0300\u0325", "\u0300\u0325", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("a\u0300\u0325", "\u0325", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("a\u0300\u0325", "\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
        };

        SearchData[] NORMEXACT = {
            SD("a\u0300\u0325", "a\u0325\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, -1), IA(3)),
        };

        SearchData[] NONNORMEXACT = {
            SD("a\u0300\u0325", "\u0325\u0300", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
        };

        SearchData[] OVERLAP = {
            SD("abababab", "abab", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, 2, 4, -1), IA(4, 4, 4)),
        };

        SearchData[] NONOVERLAP = {
            SD("abababab", "abab", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, 4, -1), IA(4, 4)),
        };

        SearchData[] OVERLAPCANONICAL = {
            SD("abababab", "abab", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, 2, 4, -1), IA(4, 4, 4)),
        };

        SearchData[] NONOVERLAPCANONICAL = {
            SD("abababab", "abab", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, 4, -1), IA(4, 4)),
        };

        SearchData[] PATTERNCANONICAL = {
            SD("The quick brown fox jumps over the lazy foxes", "the", null, CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(0, 31, -1), IA(3, 3)),
            SD("The quick brown fox jumps over the lazy foxes", "fox", null, CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(16, 40, -1), IA(3, 3)),
        };

        SearchData[] PATTERN = {
            SD("The quick brown fox jumps over the lazy foxes", "the", null, CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(0, 31, -1), IA(3, 3)),
            SD("The quick brown fox jumps over the lazy foxes", "fox", null, CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(16, 40, -1), IA(3, 3)),
        };

        const String PECHE_WITH_ACCENTS = "un p\u00E9ch\u00E9, "
                                    + "\u00E7a p\u00E8che par, "
                                    + "p\u00E9cher, "
                                    + "une p\u00EAche, "
                                    + "un p\u00EAcher, "
                                    + "j\u2019ai p\u00EAch\u00E9, "
                                    + "un p\u00E9cheur, "
                                    + "\u201Cp\u00E9che\u201D, "
                                    + "decomp peche\u0301, "
                                    + "base peche";
        // in the above, the interesting words and their offsets are:
        //    3 pe<301>che<301>
        //    13 pe<300>che
        //    24 pe<301>cher
        //    36 pe<302>che
        //    46 pe<302>cher
        //    59 pe<302>che<301>
        //    69 pe<301>cheur
        //    79 pe<301>che
        //    94 peche<+301>
        //    107 peche

        SearchData[] STRENGTH = {
            /*  012345678901234567890123456789012345678901234567890123456789 */
            SD("The quick brown fox jumps over the lazy foxes", "fox", "en", CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(16, 40, -1), IA(3, 3)),
            SD("The quick brown fox jumps over the lazy foxes", "fox", "en", CollationStrength.Primary, ElementComparisonType.StandardElementComparison, "wordbreaker", IA(16, -1), IA(3)),
            SD("blackbirds Pat p\u00E9ch\u00E9 p\u00EAche p\u00E9cher p\u00EAcher Tod T\u00F6ne black Tofu blackbirds Ton PAT toehold blackbird black-bird pat toe big Toe",
                    "peche", "fr", CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(15, 21, 27, 34, -1), IA(5, 5, 5, 5)),
            SD("This is a toe T\u00F6ne", "toe", "de", CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(10, 14, -1), IA(3, 2)),
            SD("A channel, another CHANNEL, more Channels, and one last channel...", "channel", "es", CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(2, 19, 33, 56, -1), IA(7, 7, 7, 7)),
            SD("\u00c0 should match but not A", "A\u0300", "en", CollationStrength.Identical, ElementComparisonType.StandardElementComparison,  null, IA(0, -1), IA(1, 0)),

            /* some tests for modified element comparison, ticket #7093 */
            SD(PECHE_WITH_ACCENTS, "peche", "en", CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(3, 13, 24, 36, 46, 59, 69, 79, 94, 107, -1), IA(5, 5, 5, 5, 5, 5, 5, 5, 6, 5)),
            SD(PECHE_WITH_ACCENTS, "peche", "en", CollationStrength.Primary, ElementComparisonType.StandardElementComparison, "wordbreaker", IA(3, 13, 36, 59, 79, 94, 107, -1), IA(5, 5, 5, 5, 5, 6, 5)),
            SD(PECHE_WITH_ACCENTS, "peche", "en", CollationStrength.Secondary, ElementComparisonType.StandardElementComparison, null, IA(107, -1), IA(5)),
            SD(PECHE_WITH_ACCENTS, "peche", "en", CollationStrength.Secondary, ElementComparisonType.PatternBaseWeightIsWildcard, null, IA(3, 13, 24, 36, 46, 59, 69, 79, 94, 107, -1), IA(5, 5, 5, 5, 5, 5, 5, 5, 6, 5)),
            SD(PECHE_WITH_ACCENTS, "peche", "en", CollationStrength.Secondary, ElementComparisonType.PatternBaseWeightIsWildcard, "wordbreaker", IA(3, 13, 36, 59, 79, 94, 107, -1), IA(5, 5, 5, 5, 5, 6, 5)),
            SD(PECHE_WITH_ACCENTS, "p\u00E9che", "en", CollationStrength.Secondary, ElementComparisonType.StandardElementComparison, null, IA(24, 69, 79, -1), IA(5, 5, 5)),
            SD(PECHE_WITH_ACCENTS, "p\u00E9che", "en", CollationStrength.Secondary, ElementComparisonType.StandardElementComparison, "wordbreaker", IA(79, -1), IA(5)),
            SD(PECHE_WITH_ACCENTS, "p\u00E9che", "en", CollationStrength.Secondary, ElementComparisonType.PatternBaseWeightIsWildcard, null, IA(3, 24, 69, 79, -1), IA(5, 5, 5, 5)),
            SD(PECHE_WITH_ACCENTS, "p\u00E9che", "en", CollationStrength.Secondary, ElementComparisonType.PatternBaseWeightIsWildcard, "wordbreaker", IA(3, 79, -1), IA(5, 5)),
            SD(PECHE_WITH_ACCENTS, "p\u00E9che", "en", CollationStrength.Secondary, ElementComparisonType.AnyBaseWeightIsWildcard, null, IA(3, 24, 69, 79, 94, 107, -1), IA(5, 5, 5, 5, 6, 5)),
            SD(PECHE_WITH_ACCENTS, "p\u00E9che", "en", CollationStrength.Secondary, ElementComparisonType.AnyBaseWeightIsWildcard, "wordbreaker", IA(3, 79, 94, 107, -1), IA(5, 5, 6, 5)),
            SD(PECHE_WITH_ACCENTS, "pech\u00E9", "en", CollationStrength.Secondary, ElementComparisonType.PatternBaseWeightIsWildcard, null, IA(3, 59, 94, -1), IA(5, 5, 6)),
            SD(PECHE_WITH_ACCENTS, "pech\u00E9", "en", CollationStrength.Secondary, ElementComparisonType.PatternBaseWeightIsWildcard, "wordbreaker", IA(3, 59, 94, -1), IA(5, 5, 6)),
            SD(PECHE_WITH_ACCENTS, "pech\u00E9", "en", CollationStrength.Secondary, ElementComparisonType.AnyBaseWeightIsWildcard, null, IA(3, 13, 24, 36, 46, 59, 69, 79, 94, 107, -1), IA(5, 5, 5, 5, 5, 5, 5, 5, 6, 5)),
            SD(PECHE_WITH_ACCENTS, "pech\u00E9", "en", CollationStrength.Secondary, ElementComparisonType.AnyBaseWeightIsWildcard, "wordbreaker", IA(3, 13, 36, 59, 79, 94, 107, -1), IA(5, 5, 5, 5, 5, 6, 5)),
            SD(PECHE_WITH_ACCENTS, "peche\u0301", "en", CollationStrength.Secondary, ElementComparisonType.PatternBaseWeightIsWildcard, null, IA(3, 59, 94, -1), IA(5, 5, 6)),
            SD(PECHE_WITH_ACCENTS, "peche\u0301", "en", CollationStrength.Secondary, ElementComparisonType.PatternBaseWeightIsWildcard, "wordbreaker", IA(3, 59, 94, -1), IA(5, 5, 6)),
            SD(PECHE_WITH_ACCENTS, "peche\u0301", "en", CollationStrength.Secondary, ElementComparisonType.AnyBaseWeightIsWildcard, null, IA(3, 13, 24, 36, 46, 59, 69, 79, 94, 107, -1), IA(5, 5, 5, 5, 5, 5, 5, 5, 6, 5)),
            SD(PECHE_WITH_ACCENTS, "peche\u0301", "en", CollationStrength.Secondary, ElementComparisonType.AnyBaseWeightIsWildcard, "wordbreaker", IA(3, 13, 36, 59, 79, 94, 107, -1), IA(5, 5, 5, 5, 5, 6, 5)),

            /* more tests for modified element comparison (with fr), ticket #7093 */
            SD(PECHE_WITH_ACCENTS, "peche", "fr", CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(3, 13, 24, 36, 46, 59, 69, 79, 94, 107, -1), IA(5, 5, 5, 5, 5, 5, 5, 5, 6, 5)),
            SD(PECHE_WITH_ACCENTS, "peche", "fr", CollationStrength.Primary, ElementComparisonType.StandardElementComparison, "wordbreaker", IA(3, 13, 36, 59, 79, 94, 107, -1), IA(5, 5, 5, 5, 5, 6, 5)),
            SD(PECHE_WITH_ACCENTS, "peche", "fr", CollationStrength.Secondary, ElementComparisonType.StandardElementComparison, null, IA(107, -1), IA(5)),
            SD(PECHE_WITH_ACCENTS, "peche", "fr", CollationStrength.Secondary, ElementComparisonType.PatternBaseWeightIsWildcard, null, IA(3, 13, 24, 36, 46, 59, 69, 79, 94, 107, -1), IA(5, 5, 5, 5, 5, 5, 5, 5, 6, 5)),
            SD(PECHE_WITH_ACCENTS, "peche", "fr", CollationStrength.Secondary, ElementComparisonType.PatternBaseWeightIsWildcard, "wordbreaker", IA(3, 13, 36, 59, 79, 94, 107, -1), IA(5, 5, 5, 5, 5, 6, 5)),
            SD(PECHE_WITH_ACCENTS, "p\u00E9che", "fr", CollationStrength.Secondary, ElementComparisonType.StandardElementComparison, null, IA(24, 69, 79, -1), IA(5, 5, 5)),
            SD(PECHE_WITH_ACCENTS, "p\u00E9che", "fr", CollationStrength.Secondary, ElementComparisonType.StandardElementComparison, "wordbreaker", IA(79, -1), IA(5)),
            SD(PECHE_WITH_ACCENTS, "p\u00E9che", "fr", CollationStrength.Secondary, ElementComparisonType.PatternBaseWeightIsWildcard, null, IA(3, 24, 69, 79, -1), IA(5, 5, 5, 5)),
            SD(PECHE_WITH_ACCENTS, "p\u00E9che", "fr", CollationStrength.Secondary, ElementComparisonType.PatternBaseWeightIsWildcard, "wordbreaker", IA(3, 79, -1), IA(5, 5)),
            SD(PECHE_WITH_ACCENTS, "p\u00E9che", "fr", CollationStrength.Secondary, ElementComparisonType.AnyBaseWeightIsWildcard, null, IA(3, 24, 69, 79, 94, 107, -1), IA(5, 5, 5, 5, 6, 5)),
            SD(PECHE_WITH_ACCENTS, "p\u00E9che", "fr", CollationStrength.Secondary, ElementComparisonType.AnyBaseWeightIsWildcard, "wordbreaker", IA(3, 79, 94, 107, -1), IA(5, 5, 6, 5)),
            SD(PECHE_WITH_ACCENTS, "pech\u00E9", "fr", CollationStrength.Secondary, ElementComparisonType.PatternBaseWeightIsWildcard, null, IA(3, 59, 94, -1), IA(5, 5, 6)),
            SD(PECHE_WITH_ACCENTS, "pech\u00E9", "fr", CollationStrength.Secondary, ElementComparisonType.PatternBaseWeightIsWildcard, "wordbreaker", IA(3, 59, 94, -1), IA(5, 5, 6)),
            SD(PECHE_WITH_ACCENTS, "pech\u00E9", "fr", CollationStrength.Secondary, ElementComparisonType.AnyBaseWeightIsWildcard, null, IA(3, 13, 24, 36, 46, 59, 69, 79, 94, 107, -1), IA(5, 5, 5, 5, 5, 5, 5, 5, 6, 5)),
            SD(PECHE_WITH_ACCENTS, "pech\u00E9", "fr", CollationStrength.Secondary, ElementComparisonType.AnyBaseWeightIsWildcard, "wordbreaker", IA(3, 13, 36, 59, 79, 94, 107, -1), IA(5, 5, 5, 5, 5, 6, 5)),
            SD(PECHE_WITH_ACCENTS, "peche\u0301", "fr", CollationStrength.Secondary, ElementComparisonType.PatternBaseWeightIsWildcard, null, IA(3, 59, 94, -1), IA(5, 5, 6)),
            SD(PECHE_WITH_ACCENTS, "peche\u0301", "fr", CollationStrength.Secondary, ElementComparisonType.PatternBaseWeightIsWildcard, "wordbreaker", IA(3, 59, 94, -1), IA(5, 5, 6)),
            SD(PECHE_WITH_ACCENTS, "peche\u0301", "fr", CollationStrength.Secondary, ElementComparisonType.AnyBaseWeightIsWildcard, null, IA(3, 13, 24, 36, 46, 59, 69, 79, 94, 107, -1), IA(5, 5, 5, 5, 5, 5, 5, 5, 6, 5)),
            SD(PECHE_WITH_ACCENTS, "peche\u0301", "fr", CollationStrength.Secondary, ElementComparisonType.AnyBaseWeightIsWildcard, "wordbreaker", IA(3, 13, 36, 59, 79, 94, 107, -1), IA(5, 5, 5, 5, 5, 6, 5)),

        };

        SearchData[] STRENGTHCANONICAL = {
            /*  012345678901234567890123456789012345678901234567890123456789 */
            SD("The quick brown fox jumps over the lazy foxes", "fox", "en", CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(16, 40, -1), IA(3, 3)),
            SD("The quick brown fox jumps over the lazy foxes", "fox", "en", CollationStrength.Primary, ElementComparisonType.StandardElementComparison, "wordbreaker", IA(16, -1), IA(3)),
            SD("blackbirds Pat p\u00E9ch\u00E9 p\u00EAche p\u00E9cher p\u00EAcher Tod T\u00F6ne black Tofu blackbirds Ton PAT toehold blackbird black-bird pat toe big Toe",
                    "peche", "fr", CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(15, 21, 27, 34, -1), IA(5, 5, 5, 5)),
            SD("This is a toe T\u00F6ne", "toe", "de", CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(10, 14, -1), IA(3, 2)),
            SD("A channel, another CHANNEL, more Channels, and one last channel...", "channel", "es", CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(2, 19, 33, 56, -1), IA(7, 7, 7, 7)),
        };

        SearchData[] SUPPLEMENTARYCANONICAL = {
            /*  012345678901234567890123456789012345678901234567890012345678901234567890123456789012345678901234567890012345678901234567890123456789 */
            SD("abc \uD800\uDC00 \uD800\uDC01 \uD801\uDC00 \uD800\uDC00abc abc\uD800\uDC00 \uD800\uD800\uDC00 \uD800\uDC00\uDC00", "\uD800\uDC00",
                null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(4, 13, 22, 26, 29, -1), IA(2, 2, 2, 2, 2)),
            SD("and\uD834\uDDB9this sentence", "\uD834\uDDB9", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(3, -1), IA(2)),
            SD("and \uD834\uDDB9 this sentence", " \uD834\uDDB9 ", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(3, -1), IA(4)),
            SD("and-\uD834\uDDB9-this sentence", "-\uD834\uDDB9-", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(3, -1), IA(4)),
            SD("and,\uD834\uDDB9,this sentence", ",\uD834\uDDB9,", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(3, -1), IA(4)),
            SD("and?\uD834\uDDB9?this sentence", "?\uD834\uDDB9?", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(3, -1), IA(4)),
        };

        static SearchData[] VARIABLE = {
            /*  012345678901234567890123456789012345678901234567890123456789 */
            SD("blackbirds black blackbirds blackbird black-bird", "blackbird", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(0, 17, 28, 38, -1), IA(9, 9, 9, 10)),

            /*
             * to see that it doesn't go into an infinite loop if the start of text is a ignorable character
             */
            SD(" on", "go", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
            SD("abcdefghijklmnopqrstuvwxyz", "   ",
                null, CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null,
                IA(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, -1),
                IA(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)),

            /* testing tightest match */
            SD(" abc  a bc   ab c    a  bc     ab  c", "abc", null, CollationStrength.Quaternary, ElementComparisonType.StandardElementComparison, null, IA(1, -1), IA(3)),
            /*  012345678901234567890123456789012345678901234567890123456789 */
            SD(" abc  a bc   ab c    a  bc     ab  c", "abc", null, CollationStrength.Secondary, ElementComparisonType.StandardElementComparison, null, IA(1, 6, 13, 21, 31, -1), IA(3, 4, 4, 5, 5)),

            /* totally ignorable text */
            SD("           ---------------", "abc", null, CollationStrength.Secondary, ElementComparisonType.StandardElementComparison, null, IA(-1), IA(0)),
        };

        static SearchData[] TEXTCANONICAL = {
            SD("the foxy brown fox", "fox", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(4, 15, -1), IA(3, 3)),
            SD("the quick brown fox", "fox", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(16, -1), IA(3)),
        };

        static SearchData[] INDICPREFIXMATCH = {
            SD("\u0915\u0020\u0915\u0901\u0020\u0915\u0902\u0020\u0915\u0903\u0020\u0915\u0940\u0020\u0915\u093F\u0020\u0915\u0943\u0020\u0915\u093C\u0020\u0958",
                    "\u0915", null, CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(0, 2, 5, 8, 11, 14, 17, 20, 23,-1), IA(1, 2, 2, 2, 1, 1, 1, 2, 1)),
            SD("\u0915\u0924\u0020\u0915\u0924\u0940\u0020\u0915\u0924\u093F\u0020\u0915\u0924\u0947\u0020\u0915\u0943\u0924\u0020\u0915\u0943\u0924\u0947",
                    "\u0915\u0924", null, CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(0, 3, 7, 11, -1), IA(2, 2, 2, 2)),
            SD("\u0915\u0924\u0020\u0915\u0924\u0940\u0020\u0915\u0924\u093F\u0020\u0915\u0924\u0947\u0020\u0915\u0943\u0924\u0020\u0915\u0943\u0924\u0947",
                    "\u0915\u0943\u0924", null, CollationStrength.Primary, ElementComparisonType.StandardElementComparison, null, IA(15, 19, -1), IA(3, 3)),
        };

        /**
         * Constructor
         */
        public SearchTest()
        {

        }

        [SetUp]
        public void Init()
        {
            m_en_us_ = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US"));
            m_fr_fr_ = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("fr-FR"));
            m_de_ = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("de-DE"));
            m_es_ = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("es-ES"));
            m_en_wordbreaker_ = BreakIterator.GetWordInstance();
            m_en_characterbreaker_ = BreakIterator.GetCharacterInstance();
            String rules = m_de_.GetRules() + EXTRACOLLATIONRULE;
            m_de_ = new RuleBasedCollator(rules);
            rules = m_es_.GetRules() + EXTRACOLLATIONRULE;
            m_es_ = new RuleBasedCollator(rules);

        }

        RuleBasedCollator getCollator(String collator)
        {
            if (collator == null)
            {
                return m_en_us_;
            }
            if (collator.Equals("fr"))
            {
                return m_fr_fr_;
            }
            else if (collator.Equals("de"))
            {
                return m_de_;
            }
            else if (collator.Equals("es"))
            {
                return m_es_;
            }
            else
            {
                return m_en_us_;
            }
        }

        BreakIterator getBreakIterator(String breaker)
        {
            if (breaker == null)
            {
                return null;
            }
            if (breaker.Equals("wordbreaker"))
            {
                return m_en_wordbreaker_;
            }
            else
            {
                return m_en_characterbreaker_;
            }
        }

        bool assertCanonicalEqual(SearchData search)
        {
            Collator collator = getCollator(search.collator);
            BreakIterator breaker = getBreakIterator(search.breaker);
            StringSearch strsrch;

            String text = search.text;
            String pattern = search.pattern;

            if (breaker != null)
            {
                breaker.SetText(text);
            }
            collator.Strength = (search.strength);
            collator.Decomposition = (NormalizationMode.CanonicalDecomposition);
            try
            {
                strsrch = new StringSearch(pattern, new StringCharacterEnumerator(text), (RuleBasedCollator)collator, breaker);
                strsrch.ElementComparisonType = (search.cmpType);
                strsrch.IsCanonical = (true);
            }
            catch (Exception e)
            {
                Errln("Error opening string search" + e.Message);
                return false;
            }

            if (!assertEqualWithStringSearch(strsrch, search))
            {
                collator.Strength = (CollationStrength.Tertiary);
                collator.Decomposition = (NormalizationMode.NoDecomposition);
                return false;
            }
            collator.Strength = (CollationStrength.Tertiary);
            collator.Decomposition = (NormalizationMode.NoDecomposition);
            return true;
        }

        bool assertEqual(SearchData search)
        {
            Collator collator = getCollator(search.collator);
            BreakIterator breaker = getBreakIterator(search.breaker);
            StringSearch strsrch;

            String text = search.text;
            String pattern = search.pattern;

            if (breaker != null)
            {
                breaker.SetText(text);
            }
            collator.Strength = (search.strength);
            try
            {
                strsrch = new StringSearch(pattern, new StringCharacterEnumerator(text), (RuleBasedCollator)collator, breaker);
                strsrch.ElementComparisonType = (search.cmpType);
            }
            catch (Exception e)
            {
                Errln("Error opening string search " + e.Message);
                return false;
            }

            if (!assertEqualWithStringSearch(strsrch, search))
            {
                collator.Strength = (CollationStrength.Tertiary);
                return false;
            }
            collator.Strength = (CollationStrength.Tertiary);
            return true;
        }

        bool assertEqualWithAttribute(SearchData search, bool canonical, bool overlap)
        {
            Collator collator = getCollator(search.collator);
            BreakIterator breaker = getBreakIterator(search.breaker);
            StringSearch strsrch;

            String text = search.text;
            String pattern = search.pattern;

            if (breaker != null)
            {
                breaker.SetText(text);
            }
            collator.Strength = (search.strength);
            try
            {
                strsrch = new StringSearch(pattern, new StringCharacterEnumerator(text), (RuleBasedCollator)collator, breaker);
                strsrch.IsCanonical = (canonical);
                strsrch.IsOverlapping = (overlap);
                strsrch.ElementComparisonType = (search.cmpType);
            }
            catch (Exception e)
            {
                Errln("Error opening string search " + e.Message);
                return false;
            }

            if (!assertEqualWithStringSearch(strsrch, search))
            {
                collator.Strength = (CollationStrength.Tertiary);
                return false;
            }
            collator.Strength = (CollationStrength.Tertiary);
            return true;
        }

        bool assertEqualWithStringSearch(StringSearch strsrch, SearchData search)
        {
            int count = 0;
            int matchindex = search.offset[count];
            String matchtext;

            if (strsrch.MatchStart != SearchIterator.Done ||
                strsrch.MatchLength != 0)
            {
                Errln("Error with the initialization of match start and length");
            }
            // start of following matches
            while (matchindex >= 0)
            {
                int matchlength = search.size[count];
                strsrch.Next();
                //int x = strsrch.MatchStart;
                if (matchindex != strsrch.MatchStart ||
                    matchlength != strsrch.MatchLength)
                {
                    Errln("Text: " + search.text);
                    Errln("Searching forward for pattern: " + strsrch.Pattern);
                    Errln("Expected offset,len " + matchindex + ", " + matchlength + "; got " + strsrch.MatchStart + ", " + strsrch.MatchLength);
                    return false;
                }
                count++;

                matchtext = strsrch.GetMatchedText();
                String targetText = search.text;
                if (matchlength > 0 &&
                    targetText.Substring(matchindex, /*matchindex +*/ matchlength).CompareTo(matchtext) != 0) // ICU4N: Corrected 2nd param
                {
                    Errln("Error getting following matched text");
                }

                matchindex = search.offset[count];
            }
            strsrch.Next();
            if (strsrch.MatchStart != SearchIterator.Done ||
                strsrch.MatchLength != 0)
            {
                Errln("Text: " + search.text);
                Errln("Searching forward for pattern: " + strsrch.Pattern);
                Errln("Expected DONE offset,len -1, 0; got " + strsrch.MatchStart + ", " + strsrch.MatchLength);
                return false;
            }
            // start of preceding matches
            count = count == 0 ? 0 : count - 1;
            matchindex = search.offset[count];
            while (matchindex >= 0)
            {
                int matchlength = search.size[count];
                strsrch.Previous();
                if (matchindex != strsrch.MatchStart ||
                    matchlength != strsrch.MatchLength)
                {
                    Errln("Text: " + search.text);
                    Errln("Searching backward for pattern: " + strsrch.Pattern);
                    Errln("Expected offset,len " + matchindex + ", " + matchlength + "; got " + strsrch.MatchStart + ", " + strsrch.MatchLength);
                    return false;
                }

                matchtext = strsrch.GetMatchedText();
                String targetText = search.text;
                if (matchlength > 0 &&
                    targetText.Substring(matchindex, /*matchindex +*/ matchlength).CompareTo(matchtext) != 0) // ICU4N: Corrected 2nd param
                {
                    Errln("Error getting following matched text");
                }

                matchindex = count > 0 ? search.offset[count - 1] : -1;
                count--;
            }
            strsrch.Previous();
            if (strsrch.MatchStart != SearchIterator.Done ||
                strsrch.MatchLength != 0)
            {
                Errln("Text: " + search.text);
                Errln("Searching backward for pattern: " + strsrch.Pattern);
                Errln("Expected DONE offset,len -1, 0; got " + strsrch.MatchStart + ", " + strsrch.MatchLength);
                return false;
            }
            return true;
        }

        [Test]
        public void TestConstructor()
        {
            String pattern = "pattern";
            String text = "text";
            StringCharacterEnumerator textiter = new StringCharacterEnumerator(text);
            Collator defaultcollator = Collator.GetInstance();
            BreakIterator breaker = BreakIterator.GetCharacterInstance();
            breaker.SetText(text);
            StringSearch search = new StringSearch(pattern, text);
            if (!search.Pattern.Equals(pattern)
                || !search.Target.Equals(textiter)
                || !search.Collator.Equals(defaultcollator)
                /*|| !search.BreakIterator.Equals(breaker)*/)
            {
                Errln("StringSearch(String, String) error");
            }
            search = new StringSearch(pattern, textiter, m_fr_fr_);
            if (!search.Pattern.Equals(pattern)
                || !search.Target.Equals(textiter)
                || !search.Collator.Equals(m_fr_fr_)
                /*|| !search.BreakIterator.Equals(breaker)*/)
            {
                Errln("StringSearch(String, StringCharacterIterator, "
                      + "RuleBasedCollator) error");
            }
            var de = new CultureInfo("de-DE");
            breaker = BreakIterator.GetCharacterInstance(de);
            breaker.SetText(text);
            search = new StringSearch(pattern, textiter, de);
            if (!search.Pattern.Equals(pattern)
                || !search.Target.Equals(textiter)
                || !search.Collator.Equals(Collator.GetInstance(de))
                /*|| !search.BreakIterator.Equals(breaker)*/)
            {
                Errln("StringSearch(String, StringCharacterIterator, Locale) "
                      + "error");
            }

            search = new StringSearch(pattern, textiter, m_fr_fr_,
                                      m_en_wordbreaker_);
            if (!search.Pattern.Equals(pattern)
                || !search.Target.Equals(textiter)
                || !search.Collator.Equals(m_fr_fr_)
                || !search.BreakIterator.Equals(m_en_wordbreaker_))
            {
                Errln("StringSearch(String, StringCharacterIterator, Locale) "
                      + "error");
            }
        }

        [Test]
        public void TestBasic()
        {
            for (int count = 0; count < BASIC.Length; count++)
            {
                if (!assertEqual(BASIC[count]))
                {
                    Errln("Error at test number " + count);
                }
            }
        }

        [Test]
        public void TestBreakIterator()
        {

            String text = BREAKITERATOREXACT[0].text;
            String pattern = BREAKITERATOREXACT[0].pattern;
            StringSearch strsrch = null;
            try
            {
                strsrch = new StringSearch(pattern, new StringCharacterEnumerator(text), m_en_us_, null);
            }
            catch (Exception e)
            {
                Errln("Error opening string search");
                return;
            }

            strsrch.SetBreakIterator(null);
            if (strsrch.BreakIterator != null)
            {
                Errln("Error usearch_getBreakIterator returned wrong object");
            }

            strsrch.SetBreakIterator(m_en_characterbreaker_);
            if (!strsrch.BreakIterator.Equals(m_en_characterbreaker_))
            {
                Errln("Error usearch_getBreakIterator returned wrong object");
            }

            strsrch.SetBreakIterator(m_en_wordbreaker_);
            if (!strsrch.BreakIterator.Equals(m_en_wordbreaker_))
            {
                Errln("Error usearch_getBreakIterator returned wrong object");
            }

            int count = 0;
            while (count < 4)
            {
                // special purposes for tests numbers 0-3
                SearchData search = BREAKITERATOREXACT[count];
                RuleBasedCollator collator = getCollator(search.collator);
                BreakIterator breaker = getBreakIterator(search.breaker);
                //StringSearch      strsrch;

                text = search.text;
                pattern = search.pattern;
                if (breaker != null)
                {
                    breaker.SetText(text);
                }
                collator.Strength = (search.strength);
                strsrch = new StringSearch(pattern, new StringCharacterEnumerator(text), collator, breaker);
                if (strsrch.BreakIterator != breaker)
                {
                    Errln("Error setting break iterator");
                }
                if (!assertEqualWithStringSearch(strsrch, search))
                {
                    collator.Strength = (CollationStrength.Tertiary);
                }
                search = BREAKITERATOREXACT[count + 1];
                breaker = getBreakIterator(search.breaker);
                if (breaker != null)
                {
                    breaker.SetText(text);
                }
                strsrch.SetBreakIterator(breaker);
                if (strsrch.BreakIterator != breaker)
                {
                    Errln("Error setting break iterator");
                }
                strsrch.Reset();
                if (!assertEqualWithStringSearch(strsrch, search))
                {
                    Errln("Error at test number " + count);
                }
                count += 2;
            }
            for (count = 0; count < BREAKITERATOREXACT.Length; count++)
            {
                if (!assertEqual(BREAKITERATOREXACT[count]))
                {
                    Errln("Error at test number " + count);
                }
            }
        }

        [Test]
        public void TestBreakIteratorCanonical()
        {
            int count = 0;
            while (count < 4)
            {
                // special purposes for tests numbers 0-3
                SearchData search = BREAKITERATORCANONICAL[count];

                String text = search.text;
                String pattern = search.pattern;
                RuleBasedCollator collator = getCollator(search.collator);
                collator.Strength = (search.strength);

                BreakIterator breaker = getBreakIterator(search.breaker);
                StringSearch strsrch = null;
                try
                {
                    strsrch = new StringSearch(pattern, new StringCharacterEnumerator(text), collator, breaker);
                }
                catch (Exception e)
                {
                    Errln("Error creating string search data");
                    return;
                }
                strsrch.IsCanonical = (true);
                if (!strsrch.BreakIterator.Equals(breaker))
                {
                    Errln("Error setting break iterator");
                    return;
                }
                if (!assertEqualWithStringSearch(strsrch, search))
                {
                    collator.Strength = (CollationStrength.Tertiary);
                    return;
                }
                search = BREAKITERATOREXACT[count + 1];
                breaker = getBreakIterator(search.breaker);
                breaker.SetText(strsrch.Target);
                strsrch.SetBreakIterator(breaker);
                if (!strsrch.BreakIterator.Equals(breaker))
                {
                    Errln("Error setting break iterator");
                    return;
                }
                strsrch.Reset();
                strsrch.IsCanonical = (true);
                if (!assertEqualWithStringSearch(strsrch, search))
                {
                    Errln("Error at test number " + count);
                    return;
                }
                count += 2;
            }

            for (count = 0; count < BREAKITERATORCANONICAL.Length; count++)
            {
                if (!assertEqual(BREAKITERATORCANONICAL[count]))
                {
                    Errln("Error at test number " + count);
                    return;
                }
            }
        }

        [Test]
        public void TestCanonical()
        {
            for (int count = 0; count < BASICCANONICAL.Length; count++)
            {
                if (!assertCanonicalEqual(BASICCANONICAL[count]))
                {
                    Errln("Error at test number " + count);
                }
            }
        }

        [Test]
        public void TestCollator()
        {
            // test collator that thinks "o" and "p" are the same thing
            String text = COLLATOR[0].text;
            String pattern = COLLATOR[0].pattern;
            StringSearch strsrch = null;
            try
            {
                strsrch = new StringSearch(pattern, new StringCharacterEnumerator(text), m_en_us_, null);
            }
            catch (Exception e)
            {
                Errln("Error opening string search ");
                return;
            }
            if (!assertEqualWithStringSearch(strsrch, COLLATOR[0]))
            {
                return;
            }
            String rules = TESTCOLLATORRULE;
            RuleBasedCollator tailored = null;
            try
            {
                tailored = new RuleBasedCollator(rules);
                tailored.Strength = (COLLATOR[1].strength);
            }
            catch (Exception e)
            {
                Errln("Error opening rule based collator ");
                return;
            }

            strsrch.SetCollator(tailored);
            if (!strsrch.Collator.Equals(tailored))
            {
                Errln("Error setting rule based collator");
            }
            strsrch.Reset();
            if (!assertEqualWithStringSearch(strsrch, COLLATOR[1]))
            {
                return;
            }
            strsrch.SetCollator(m_en_us_);
            strsrch.Reset();
            if (!strsrch.Collator.Equals(m_en_us_))
            {
                Errln("Error setting rule based collator");
            }
            if (!assertEqualWithStringSearch(strsrch, COLLATOR[0]))
            {
                Errln("Error searching collator test");
            }
        }

        [Test]
        public void TestCollatorCanonical()
        {
            /* test collator that thinks "o" and "p" are the same thing */
            String text = COLLATORCANONICAL[0].text;
            String pattern = COLLATORCANONICAL[0].pattern;

            StringSearch strsrch = null;
            try
            {
                strsrch = new StringSearch(pattern, new StringCharacterEnumerator(text), m_en_us_, null);
                strsrch.IsCanonical = (true);
            }
            catch (Exception e)
            {
                Errln("Error opening string search ");
            }

            if (!assertEqualWithStringSearch(strsrch, COLLATORCANONICAL[0]))
            {
                return;
            }

            String rules = TESTCOLLATORRULE;
            RuleBasedCollator tailored = null;
            try
            {
                tailored = new RuleBasedCollator(rules);
                tailored.Strength = (COLLATORCANONICAL[1].strength);
                tailored.Decomposition = (NormalizationMode.CanonicalDecomposition);
            }
            catch (Exception e)
            {
                Errln("Error opening rule based collator ");
            }

            strsrch.SetCollator(tailored);
            if (!strsrch.Collator.Equals(tailored))
            {
                Errln("Error setting rule based collator");
            }
            strsrch.Reset();
            strsrch.IsCanonical = (true);
            if (!assertEqualWithStringSearch(strsrch, COLLATORCANONICAL[1]))
            {
                Logln("COLLATORCANONICAL[1] failed");  // Error should already be reported.
            }
            strsrch.SetCollator(m_en_us_);
            strsrch.Reset();
            if (!strsrch.Collator.Equals(m_en_us_))
            {
                Errln("Error setting rule based collator");
            }
            if (!assertEqualWithStringSearch(strsrch, COLLATORCANONICAL[0]))
            {
                Logln("COLLATORCANONICAL[0] failed");  // Error should already be reported.
            }
        }

        [Test]
        public void TestCompositeBoundaries()
        {
            for (int count = 0; count < COMPOSITEBOUNDARIES.Length; count++)
            {
                // Logln("composite " + count);
                if (!assertEqual(COMPOSITEBOUNDARIES[count]))
                {
                    Errln("Error at test number " + count);
                }
            }
        }

        [Test]
        public void TestCompositeBoundariesCanonical()
        {
            for (int count = 0; count < COMPOSITEBOUNDARIESCANONICAL.Length; count++)
            {
                // Logln("composite " + count);
                if (!assertCanonicalEqual(COMPOSITEBOUNDARIESCANONICAL[count]))
                {
                    Errln("Error at test number " + count);
                }
            }
        }

        [Test]
        public void TestContraction()
        {
            String rules = CONTRACTIONRULE;
            RuleBasedCollator collator = null;
            try
            {
                collator = new RuleBasedCollator(rules);
                collator.Strength = (CollationStrength.Tertiary);
                collator.Decomposition = (NormalizationMode.CanonicalDecomposition);
            }
            catch (Exception e)
            {
                Errln("Error opening collator ");
            }
            String text = "text";
            String pattern = "pattern";
            StringSearch strsrch = null;
            try
            {
                strsrch = new StringSearch(pattern, new StringCharacterEnumerator(text), collator, null);
            }
            catch (Exception e)
            {
                Errln("Error opening string search ");
            }

            for (int count = 0; count < CONTRACTION.Length; count++)
            {
                text = CONTRACTION[count].text;
                pattern = CONTRACTION[count].pattern;
                strsrch.SetTarget(new StringCharacterEnumerator(text));
                strsrch.Pattern = (pattern);
                if (!assertEqualWithStringSearch(strsrch, CONTRACTION[count]))
                {
                    Errln("Error at test number " + count);
                }
            }
        }

        [Test]
        public void TestContractionCanonical()
        {
            String rules = CONTRACTIONRULE;
            RuleBasedCollator collator = null;
            try
            {
                collator = new RuleBasedCollator(rules);
                collator.Strength = (CollationStrength.Tertiary);
                collator.Decomposition = (NormalizationMode.CanonicalDecomposition);
            }
            catch (Exception e)
            {
                Errln("Error opening collator ");
            }
            String text = "text";
            String pattern = "pattern";
            StringSearch strsrch = null;
            try
            {
                strsrch = new StringSearch(pattern, new StringCharacterEnumerator(text), collator, null);
                strsrch.IsCanonical = (true);
            }
            catch (Exception e)
            {
                Errln("Error opening string search");
            }

            for (int count = 0; count < CONTRACTIONCANONICAL.Length; count++)
            {
                text = CONTRACTIONCANONICAL[count].text;
                pattern = CONTRACTIONCANONICAL[count].pattern;
                strsrch.SetTarget(new StringCharacterEnumerator(text));
                strsrch.Pattern = (pattern);
                if (!assertEqualWithStringSearch(strsrch, CONTRACTIONCANONICAL[count]))
                {
                    Errln("Error at test number " + count);
                }
            }
        }

        [Test]
        public void TestGetMatch()
        {
            SearchData search = MATCH[0];
            String text = search.text;
            String pattern = search.pattern;

            StringSearch strsrch = null;
            try
            {
                strsrch = new StringSearch(pattern, new StringCharacterEnumerator(text), m_en_us_, null);
            }
            catch (Exception e)
            {
                Errln("Error opening string search ");
                return;
            }

            int count = 0;
            int matchindex = search.offset[count];
            String matchtext;
            while (matchindex >= 0)
            {
                int matchlength = search.size[count];
                strsrch.Next();
                if (matchindex != strsrch.MatchStart ||
                    matchlength != strsrch.MatchLength)
                {
                    Errln("Text: " + search.text);
                    Errln("Pattern: " + strsrch.Pattern);
                    Errln("Error match found at " + strsrch.MatchStart + ", " + strsrch.MatchLength);
                    return;
                }
                count++;

                matchtext = strsrch.GetMatchedText();
                if (matchtext.Length != matchlength)
                {
                    Errln("Error getting match text");
                }
                matchindex = search.offset[count];
            }
            strsrch.Next();
            if (strsrch.MatchStart != StringSearch.Done ||
                strsrch.MatchLength != 0)
            {
                Errln("Error end of match not found");
            }
            matchtext = strsrch.GetMatchedText();
            if (matchtext != null)
            {
                Errln("Error getting null matches");
            }
        }

        [Test]
        public void TestGetSetAttribute()
        {
            String pattern = "pattern";
            String text = "text";
            StringSearch strsrch = null;
            try
            {
                strsrch = new StringSearch(pattern, new StringCharacterEnumerator(text), m_en_us_, null);
            }
            catch (Exception e)
            {
                Errln("Error opening search");
                return;
            }

            if (strsrch.IsOverlapping)
            {
                Errln("Error default overlaping should be false");
            }
            strsrch.IsOverlapping = (true);
            if (!strsrch.IsOverlapping)
            {
                Errln("Error setting overlap true");
            }
            strsrch.IsOverlapping = (false);
            if (strsrch.IsOverlapping)
            {
                Errln("Error setting overlap false");
            }

            strsrch.IsCanonical = (true);
            if (!strsrch.IsCanonical)
            {
                Errln("Error setting canonical match true");
            }
            strsrch.IsCanonical = (false);
            if (strsrch.IsCanonical)
            {
                Errln("Error setting canonical match false");
            }

            if (strsrch.ElementComparisonType != ElementComparisonType.StandardElementComparison)
            {
                Errln("Error default element comparison type should be SearchIteratorElementComparisonType.StandardElementComparison");
            }
            strsrch.ElementComparisonType = (ElementComparisonType.PatternBaseWeightIsWildcard);
            if (strsrch.ElementComparisonType != ElementComparisonType.PatternBaseWeightIsWildcard)
            {
                Errln("Error setting element comparison type SearchIteratorElementComparisonType.PatternBaseWeightIsWildcard");
            }
        }

        [Test]
        public void TestGetSetOffset()
        {
            String pattern = "1234567890123456";
            String text = "12345678901234567890123456789012";
            StringSearch strsrch = null;
            try
            {
                strsrch = new StringSearch(pattern, new StringCharacterEnumerator(text), m_en_us_, null);
            }
            catch (Exception e)
            {
                Errln("Error opening search");

                return;
            }

            /* testing out of bounds error */
            try
            {
                strsrch.SetIndex(-1);
                Errln("Error expecting set offset error");
            }
            catch (ArgumentOutOfRangeException e)
            {
                Logln("PASS: strsrch.setIndex(-1) failed as expected");
            }

            try
            {
                strsrch.SetIndex(128);
                Errln("Error expecting set offset error");
            }
            catch (ArgumentOutOfRangeException e)
            {
                Logln("PASS: strsrch.setIndex(128) failed as expected");
            }

            for (int index = 0; index < BASIC.Length; index++)
            {
                SearchData search = BASIC[index];

                text = search.text;
                pattern = search.pattern;
                strsrch.SetTarget(new StringCharacterEnumerator(text));
                strsrch.Pattern = (pattern);
                strsrch.Collator.Strength = (search.strength);
                strsrch.Reset();

                int count = 0;
                int matchindex = search.offset[count];

                while (matchindex >= 0)
                {
                    int matchlength = search.size[count];
                    strsrch.Next();
                    if (matchindex != strsrch.MatchStart ||
                        matchlength != strsrch.MatchLength)
                    {
                        Errln("Text: " + text);
                        Errln("Pattern: " + strsrch.Pattern);
                        Errln("Error match found at " + strsrch.MatchStart + ", " + strsrch.MatchLength);
                        return;
                    }
                    matchindex = search.offset[count + 1] == -1 ? -1 :
                                 search.offset[count + 2];
                    if (search.offset[count + 1] != -1)
                    {
                        strsrch.SetIndex(search.offset[count + 1] + 1);
                        if (strsrch.Index != search.offset[count + 1] + 1)
                        {
                            Errln("Error setting offset\n");
                            return;
                        }
                    }

                    count += 2;
                }
                strsrch.Next();
                if (strsrch.MatchStart != StringSearch.Done)
                {
                    Errln("Text: " + text);
                    Errln("Pattern: " + strsrch.Pattern);
                    Errln("Error match found at " + strsrch.MatchStart + ", " + strsrch.MatchLength);
                    return;
                }
            }
            strsrch.Collator.Strength = (CollationStrength.Tertiary);
        }

        [Test]
        public void TestGetSetOffsetCanonical()
        {

            String text = "text";
            String pattern = "pattern";
            StringSearch strsrch = null;
            try
            {
                strsrch = new StringSearch(pattern, new StringCharacterEnumerator(text), m_en_us_, null);
            }
            catch (Exception e)
            {
                Errln("Fail to open StringSearch!");
                return;
            }
            strsrch.IsCanonical = (true);
            //TODO: setCanonical is not sufficient for canonical match. See #10725
            strsrch.Collator.Decomposition = (NormalizationMode.CanonicalDecomposition);
            /* testing out of bounds error */
            try
            {
                strsrch.SetIndex(-1);
                Errln("Error expecting set offset error");
            }
            catch (ArgumentOutOfRangeException e)
            {
                Logln("PASS: strsrch.setIndex(-1) failed as expected");
            }
            try
            {
                strsrch.SetIndex(128);
                Errln("Error expecting set offset error");
            }
            catch (ArgumentOutOfRangeException e)
            {
                Logln("PASS: strsrch.setIndex(128) failed as expected");
            }

            for (int index = 0; index < BASICCANONICAL.Length; index++)
            {
                SearchData search = BASICCANONICAL[index];
                text = search.text;
                pattern = search.pattern;
                strsrch.SetTarget(new StringCharacterEnumerator(text));
                strsrch.Pattern = (pattern);
                int count = 0;
                int matchindex = search.offset[count];
                while (matchindex >= 0)
                {
                    int matchlength = search.size[count];
                    strsrch.Next();
                    if (matchindex != strsrch.MatchStart ||
                        matchlength != strsrch.MatchLength)
                    {
                        Errln("Text: " + text);
                        Errln("Pattern: " + strsrch.Pattern);
                        Errln("Error match found at " + strsrch.MatchStart + ", " + strsrch.MatchLength);
                        return;
                    }
                    matchindex = search.offset[count + 1] == -1 ? -1 :
                                 search.offset[count + 2];
                    if (search.offset[count + 1] != -1)
                    {
                        strsrch.SetIndex(search.offset[count + 1] + 1);
                        if (strsrch.Index != search.offset[count + 1] + 1)
                        {
                            Errln("Error setting offset");
                            return;
                        }
                    }

                    count += 2;
                }
                strsrch.Next();
                if (strsrch.MatchStart != StringSearch.Done)
                {
                    Errln("Text: " + text);
                    Errln("Pattern: %s" + strsrch.Pattern);
                    Errln("Error match found at " + strsrch.MatchStart + ", " + strsrch.MatchLength);
                    return;
                }
            }
            strsrch.Collator.Strength = (CollationStrength.Tertiary);
            strsrch.Collator.Decomposition = (NormalizationMode.NoDecomposition);
        }

        [Test]
        public void TestIgnorable()
        {
            String rules = IGNORABLERULE;
            int count = 0;
            RuleBasedCollator collator = null;
            try
            {
                collator = new RuleBasedCollator(rules);
                collator.Strength = (IGNORABLE[count].strength);
                collator.Decomposition = (Collator.CanonicalDecomposition);
            }
            catch (Exception e)
            {
                Errln("Error opening collator ");
                return;
            }
            String pattern = "pattern";
            String text = "text";
            StringSearch strsrch = null;
            try
            {
                strsrch = new StringSearch(pattern, new StringCharacterEnumerator(text), collator, null);
            }
            catch (Exception e)
            {
                Errln("Error opening string search ");
                return;
            }

            for (; count < IGNORABLE.Length; count++)
            {
                text = IGNORABLE[count].text;
                pattern = IGNORABLE[count].pattern;
                strsrch.SetTarget(new StringCharacterEnumerator(text));
                strsrch.Pattern = (pattern);
                if (!assertEqualWithStringSearch(strsrch, IGNORABLE[count]))
                {
                    Errln("Error at test number " + count);
                }
            }
        }

        [Test]
        public void TestInitialization()
        {
            String pattern;
            String text;
            String temp = "a";
            StringSearch result;

            /* simple test on the pattern ce construction */
            pattern = temp + temp;
            text = temp + temp + temp;
            try
            {
                result = new StringSearch(pattern, new StringCharacterEnumerator(text), m_en_us_, null);
            }
            catch (Exception e)
            {
                Errln("Error opening search ");
                return;
            }

            /* testing if an extremely large pattern will fail the initialization */
            pattern = "";
            for (int count = 0; count < 512; count++)
            {
                pattern += temp;
            }
            try
            {
                result = new StringSearch(pattern, new StringCharacterEnumerator(text), m_en_us_, null);
                Logln("pattern:" + result.Pattern);
            }
            catch (Exception e)
            {
                Errln("Fail: an extremely large pattern will fail the initialization");
                return;
            }
        }

        [Test]
        public void TestNormCanonical()
        {
            m_en_us_.Decomposition = (Collator.CanonicalDecomposition);
            for (int count = 0; count < NORMCANONICAL.Length; count++)
            {
                if (!assertCanonicalEqual(NORMCANONICAL[count]))
                {
                    Errln("Error at test number " + count);
                }
            }
            m_en_us_.Decomposition = (Collator.NoDecomposition);
        }

        [Test]
        public void TestNormExact()
        {
            int count;

            m_en_us_.Decomposition = (Collator.CanonicalDecomposition);
            for (count = 0; count < BASIC.Length; count++)
            {
                if (!assertEqual(BASIC[count]))
                {
                    Errln("Error at test number " + count);
                }
            }
            for (count = 0; count < NORMEXACT.Length; count++)
            {
                if (!assertEqual(NORMEXACT[count]))
                {
                    Errln("Error at test number " + count);
                }
            }
            m_en_us_.Decomposition = (Collator.NoDecomposition);
            for (count = 0; count < NONNORMEXACT.Length; count++)
            {
                if (!assertEqual(NONNORMEXACT[count]))
                {
                    Errln("Error at test number " + count);
                }
            }
        }

        [Test]
        public void TestOpenClose()
        {
            StringSearch result;
            BreakIterator breakiter = m_en_wordbreaker_;
            String pattern = "";
            String text = "";
            String temp = "a";
            StringCharacterEnumerator chariter = new StringCharacterEnumerator(text);

            /* testing null arguments */
            try
            {
                result = new StringSearch(pattern, new StringCharacterEnumerator(text), null, null);
                Errln("Error: null arguments should produce an error");
            }
            catch (Exception e)
            {
                Logln("PASS: null arguments failed as expected");
            }

            chariter.Reset(text);
            try
            {
                result = new StringSearch(pattern, chariter, null, null);
                Errln("Error: null arguments should produce an error");
            }
            catch (Exception e)
            {
                Logln("PASS: null arguments failed as expected");
            }

            //text = String.valueOf(0x1);
            text = Convert.ToString(0x1, CultureInfo.InvariantCulture); // ICU4N TODO: Check this
            try
            {
                result = new StringSearch(pattern, new StringCharacterEnumerator(text), null, null);
                Errln("Error: Empty pattern should produce an error");
            }
            catch (Exception e)
            {
                Logln("PASS: Empty pattern failed as expected");
            }

            chariter.Reset(text);
            try
            {
                result = new StringSearch(pattern, chariter, null, null);
                Errln("Error: Empty pattern should produce an error");
            }
            catch (Exception e)
            {
                Logln("PASS: Empty pattern failed as expected");
            }

            text = "";
            pattern = temp;
            try
            {
                result = new StringSearch(pattern, new StringCharacterEnumerator(text), null, null);
                Errln("Error: Empty text should produce an error");
            }
            catch (Exception e)
            {
                Logln("PASS: Empty text failed as expected");
            }

            chariter.Reset(text);
            try
            {
                result = new StringSearch(pattern, chariter, null, null);
                Errln("Error: Empty text should produce an error");
            }
            catch (Exception e)
            {
                Logln("PASS: Empty text failed as expected");
            }

            text += temp;
            try
            {
                result = new StringSearch(pattern, new StringCharacterEnumerator(text), null, null);
                Errln("Error: null arguments should produce an error");
            }
            catch (Exception e)
            {
                Logln("PASS: null arguments failed as expected");
            }

            chariter.Reset(text);
            try
            {
                result = new StringSearch(pattern, chariter, null, null);
                Errln("Error: null arguments should produce an error");
            }
            catch (Exception e)
            {
                Logln("PASS: null arguments failed as expected");
            }

            try
            {
                result = new StringSearch(pattern, new StringCharacterEnumerator(text), m_en_us_, null);
            }
            catch (Exception e)
            {
                Errln("Error: null break iterator is valid for opening search");
            }

            try
            {
                result = new StringSearch(pattern, chariter, m_en_us_, null);
            }
            catch (Exception e)
            {
                Errln("Error: null break iterator is valid for opening search");
            }

            try
            {
                result = new StringSearch(pattern, new StringCharacterEnumerator(text), new CultureInfo("en"));
            }
            catch (Exception e)
            {
                Errln("Error: null break iterator is valid for opening search");
            }

            try
            {
                result = new StringSearch(pattern, chariter, new CultureInfo("en"));
            }
            catch (Exception e)
            {
                Errln("Error: null break iterator is valid for opening search");
            }

            try
            {
                result = new StringSearch(pattern, new StringCharacterEnumerator(text), m_en_us_, breakiter);
            }
            catch (Exception e)
            {
                Errln("Error: Break iterator is valid for opening search");
            }

            try
            {
                result = new StringSearch(pattern, chariter, m_en_us_, null);
                Logln("pattern:" + result.Pattern);
            }
            catch (Exception e)
            {
                Errln("Error: Break iterator is valid for opening search");
            }
        }

        [Test]
        public void TestOverlap()
        {
            int count;

            for (count = 0; count < OVERLAP.Length; count++)
            {
                if (!assertEqualWithAttribute(OVERLAP[count], false, true))
                {
                    Errln("Error at overlap test number " + count);
                }
            }

            for (count = 0; count < NONOVERLAP.Length; count++)
            {
                if (!assertEqual(NONOVERLAP[count]))
                {
                    Errln("Error at non overlap test number " + count);
                }
            }

            for (count = 0; count < OVERLAP.Length && count < NONOVERLAP.Length; count++)
            {
                SearchData search = (OVERLAP[count]);
                String text = search.text;
                String pattern = search.pattern;

                RuleBasedCollator collator = getCollator(search.collator);
                StringSearch strsrch = null;
                try
                {
                    strsrch = new StringSearch(pattern, new StringCharacterEnumerator(text), collator, null);
                }
                catch (Exception e)
                {
                    Errln("error open StringSearch");
                    return;
                }

                strsrch.IsOverlapping = (true);
                if (!strsrch.IsOverlapping)
                {
                    Errln("Error setting overlap option");
                }
                if (!assertEqualWithStringSearch(strsrch, search))
                {
                    return;
                }

                search = NONOVERLAP[count];
                strsrch.IsOverlapping = (false);
                if (strsrch.IsOverlapping)
                {
                    Errln("Error setting overlap option");
                }
                strsrch.Reset();
                if (!assertEqualWithStringSearch(strsrch, search))
                {
                    Errln("Error at test number " + count);
                }
            }
        }

        [Test]
        public void TestOverlapCanonical()
        {
            int count;

            for (count = 0; count < OVERLAPCANONICAL.Length; count++)
            {
                if (!assertEqualWithAttribute(OVERLAPCANONICAL[count], true, true))
                {
                    Errln("Error at overlap test number %d" + count);
                }
            }

            for (count = 0; count < NONOVERLAP.Length; count++)
            {
                if (!assertCanonicalEqual(NONOVERLAPCANONICAL[count]))
                {
                    Errln("Error at non overlap test number %d" + count);
                }
            }

            for (count = 0; count < OVERLAPCANONICAL.Length && count < NONOVERLAPCANONICAL.Length; count++)
            {
                SearchData search = OVERLAPCANONICAL[count];
                RuleBasedCollator collator = getCollator(search.collator);
                StringSearch strsrch = new StringSearch(search.pattern, new StringCharacterEnumerator(search.text), collator, null);
                strsrch.IsCanonical = (true);
                strsrch.IsOverlapping = (true);
                if (strsrch.IsOverlapping != true)
                {
                    Errln("Error setting overlap option");
                }
                if (!assertEqualWithStringSearch(strsrch, search))
                {
                    strsrch = null;
                    return;
                }
                search = NONOVERLAPCANONICAL[count];
                strsrch.IsOverlapping = (false);
                if (strsrch.IsOverlapping != false)
                {
                    Errln("Error setting overlap option");
                }
                strsrch.Reset();
                if (!assertEqualWithStringSearch(strsrch, search))
                {
                    strsrch = null;
                    Errln("Error at test number %d" + count);
                }
            }
        }

        [Test]
        public void TestPattern()
        {
            m_en_us_.Strength = (PATTERN[0].strength);
            StringSearch strsrch = new StringSearch(PATTERN[0].pattern, new StringCharacterEnumerator(PATTERN[0].text), m_en_us_, null);

            if (strsrch.Pattern != PATTERN[0].pattern)
            {
                Errln("Error setting pattern");
            }
            if (!assertEqualWithStringSearch(strsrch, PATTERN[0]))
            {
                m_en_us_.Strength = (CollationStrength.Tertiary);
                if (strsrch != null)
                {
                    strsrch = null;
                }
                return;
            }

            strsrch.Pattern = (PATTERN[1].pattern);
            if (PATTERN[1].pattern != strsrch.Pattern)
            {
                Errln("Error setting pattern");
                m_en_us_.Strength = (CollationStrength.Tertiary);
                if (strsrch != null)
                {
                    strsrch = null;
                }
                return;
            }
            strsrch.Reset();

            if (!assertEqualWithStringSearch(strsrch, PATTERN[1]))
            {
                m_en_us_.Strength = (CollationStrength.Tertiary);
                if (strsrch != null)
                {
                    strsrch = null;
                }
                return;
            }

            strsrch.Pattern = (PATTERN[0].pattern);
            if (PATTERN[0].pattern != strsrch.Pattern)
            {
                Errln("Error setting pattern");
                m_en_us_.Strength = (CollationStrength.Tertiary);
                if (strsrch != null)
                {
                    strsrch = null;
                }
                return;
            }
            strsrch.Reset();

            if (!assertEqualWithStringSearch(strsrch, PATTERN[0]))
            {
                m_en_us_.Strength = (CollationStrength.Tertiary);
                if (strsrch != null)
                {
                    strsrch = null;
                }
                return;
            }
            /* enormous pattern size to see if this crashes */
            String pattern = "";
            for (int templength = 0; templength != 512; templength++)
            {
                pattern += 0x61;
            }
            try
            {
                strsrch.Pattern = (pattern);
            }
            catch (Exception e)
            {
                Errln("Error setting pattern with size 512");
            }

            m_en_us_.Strength = (CollationStrength.Tertiary);
            if (strsrch != null)
            {
                strsrch = null;
            }
        }

        [Test]
        public void TestPatternCanonical()
        {
            //StringCharacterIterator text = new StringCharacterIterator(PATTERNCANONICAL[0].text);
            m_en_us_.Strength = (PATTERNCANONICAL[0].strength);
            StringSearch strsrch = new StringSearch(PATTERNCANONICAL[0].pattern, new StringCharacterEnumerator(PATTERNCANONICAL[0].text),
                                                    m_en_us_, null);
            strsrch.IsCanonical = (true);

            if (PATTERNCANONICAL[0].pattern != strsrch.Pattern)
            {
                Errln("Error setting pattern");
            }
            if (!assertEqualWithStringSearch(strsrch, PATTERNCANONICAL[0]))
            {
                m_en_us_.Strength = (CollationStrength.Tertiary);
                strsrch = null;
                return;
            }

            strsrch.Pattern = (PATTERNCANONICAL[1].pattern);
            if (PATTERNCANONICAL[1].pattern != strsrch.Pattern)
            {
                Errln("Error setting pattern");
                m_en_us_.Strength = (CollationStrength.Tertiary);
                strsrch = null;
                return;
            }
            strsrch.Reset();
            strsrch.IsCanonical = (true);

            if (!assertEqualWithStringSearch(strsrch, PATTERNCANONICAL[1]))
            {
                m_en_us_.Strength = (CollationStrength.Tertiary);
                strsrch = null;
                return;
            }

            strsrch.Pattern = (PATTERNCANONICAL[0].pattern);
            if (PATTERNCANONICAL[0].pattern != strsrch.Pattern)
            {
                Errln("Error setting pattern");
                m_en_us_.Strength = (CollationStrength.Tertiary);
                strsrch = null;
                return;
            }

            strsrch.Reset();
            strsrch.IsCanonical = (true);
            if (!assertEqualWithStringSearch(strsrch, PATTERNCANONICAL[0]))
            {
                m_en_us_.Strength = (CollationStrength.Tertiary);
                strsrch = null;
                return;
            }
        }

        [Test]
        public void TestReset()
        {
            StringCharacterEnumerator text = new StringCharacterEnumerator("fish fish");
            String pattern = "s";

            StringSearch strsrch = new StringSearch(pattern, text, m_en_us_, null);
            strsrch.IsOverlapping = (true);
            strsrch.IsCanonical = (true);
            strsrch.SetIndex(9);
            strsrch.Reset();
            if (strsrch.IsCanonical || strsrch.IsOverlapping ||
                strsrch.Index != 0 || strsrch.MatchLength != 0 ||
                strsrch.MatchStart != SearchIterator.Done)
            {
                Errln("Error resetting string search");
            }

            strsrch.Previous();
            if (strsrch.MatchStart != 7 || strsrch.MatchLength != 1)
            {
                Errln("Error resetting string search\n");
            }
        }

        [Test]
        public void TestSetMatch()
        {
            for (int count = 0; count < MATCH.Length; count++)
            {
                SearchData search = MATCH[count];
                StringSearch strsrch = new StringSearch(search.pattern, new StringCharacterEnumerator(search.text),
                                                        m_en_us_, null);

                int size = 0;
                while (search.offset[size] != -1)
                {
                    size++;
                }

                if (strsrch.First() != search.offset[0])
                {
                    Errln("Error getting first match");
                }
                if (strsrch.Last() != search.offset[size - 1])
                {
                    Errln("Error getting last match");
                }

                int index = 0;
                while (index < size)
                {
                    if (index + 2 < size)
                    {
                        if (strsrch.Following(search.offset[index + 2] - 1) != search.offset[index + 2])
                        {
                            Errln("Error getting following match at index " + (search.offset[index + 2] - 1));
                        }
                    }
                    if (index + 1 < size)
                    {
                        if (strsrch.Preceding(search.offset[index + 1] + search.size[index + 1] + 1) != search.offset[index + 1])
                        {
                            Errln("Error getting preceeding match at index " + (search.offset[index + 1] + 1));
                        }
                    }
                    index += 2;
                }

                if (strsrch.Following(search.text.Length) != SearchIterator.Done)
                {
                    Errln("Error expecting out of bounds match");
                }
                if (strsrch.Preceding(0) != SearchIterator.Done)
                {
                    Errln("Error expecting out of bounds match");
                }
            }
        }

        [Test]
        public void TestStrength()
        {
            for (int count = 0; count < STRENGTH.Length; count++)
            {
                if (!assertEqual(STRENGTH[count]))
                {
                    Errln("Error at test number " + count);
                }
            }
        }

        [Test]
        public void TestStrengthCanonical()
        {
            for (int count = 0; count < STRENGTHCANONICAL.Length; count++)
            {
                if (!assertCanonicalEqual(STRENGTHCANONICAL[count]))
                {
                    Errln("Error at test number" + count);
                }
            }
        }

        [Test]
        public void TestSupplementary()
        {
            for (int count = 0; count < SUPPLEMENTARY.Length; count++)
            {
                if (!assertEqual(SUPPLEMENTARY[count]))
                {
                    Errln("Error at test number " + count);
                }
            }
        }

        [Test]
        public void TestSupplementaryCanonical()
        {
            for (int count = 0; count < SUPPLEMENTARYCANONICAL.Length; count++)
            {
                if (!assertCanonicalEqual(SUPPLEMENTARYCANONICAL[count]))
                {
                    Errln("Error at test number" + count);
                }
            }
        }

        [Test]
        public void TestText()
        {
            SearchData[] TEXT = {
            SD("the foxy brown fox", "fox", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(4, 15, -1), IA(3, 3)),
            SD("the quick brown fox", "fox", null, CollationStrength.Tertiary, ElementComparisonType.StandardElementComparison, null, IA(16, -1), IA(3))
        };
            StringCharacterEnumerator t = new StringCharacterEnumerator(TEXT[0].text);
            StringSearch strsrch = new StringSearch(TEXT[0].pattern, t, m_en_us_, null);

            if (!t.Equals(strsrch.Target))
            {
                Errln("Error setting text");
            }
            if (!assertEqualWithStringSearch(strsrch, TEXT[0]))
            {
                Errln("Error at assertEqualWithStringSearch");
                return;
            }

            t = new StringCharacterEnumerator(TEXT[1].text);
            strsrch.SetTarget(t);
            if (!t.Equals(strsrch.Target))
            {
                Errln("Error setting text");
                return;
            }

            if (!assertEqualWithStringSearch(strsrch, TEXT[1]))
            {
                Errln("Error at assertEqualWithStringSearch");
                return;
            }
        }

        [Test]
        public void TestTextCanonical()
        {
            StringCharacterEnumerator t = new StringCharacterEnumerator(TEXTCANONICAL[0].text);
            StringSearch strsrch = new StringSearch(TEXTCANONICAL[0].pattern, t, m_en_us_, null);
            strsrch.IsCanonical = (true);

            if (!t.Equals(strsrch.Target))
            {
                Errln("Error setting text");
            }
            if (!assertEqualWithStringSearch(strsrch, TEXTCANONICAL[0]))
            {
                strsrch = null;
                return;
            }

            t = new StringCharacterEnumerator(TEXTCANONICAL[1].text);
            strsrch.SetTarget(t);
            if (!t.Equals(strsrch.Target))
            {
                Errln("Error setting text");
                strsrch = null;
                return;
            }

            if (!assertEqualWithStringSearch(strsrch, TEXTCANONICAL[1]))
            {
                strsrch = null;
                return;
            }

            t = new StringCharacterEnumerator(TEXTCANONICAL[0].text);
            strsrch.SetTarget(t);
            if (!t.Equals(strsrch.Target))
            {
                Errln("Error setting text");
                strsrch = null;
                return;
            }

            if (!assertEqualWithStringSearch(strsrch, TEXTCANONICAL[0]))
            {
                Errln("Error at assertEqualWithStringSearch");
                strsrch = null;
                return;
            }
        }

        [Test]
        public void TestVariable()
        {
            m_en_us_.IsAlternateHandlingShifted = (true);
            for (int count = 0; count < VARIABLE.Length; count++)
            {
                // Logln("variable" + count);
                if (!assertEqual(VARIABLE[count]))
                {
                    Errln("Error at test number " + count);
                }
            }
            m_en_us_.IsAlternateHandlingShifted = (false);
        }

        [Test]
        public void TestVariableCanonical()
        {
            m_en_us_.IsAlternateHandlingShifted = (true);
            for (int count = 0; count < VARIABLE.Length; count++)
            {
                // Logln("variable " + count);
                if (!assertCanonicalEqual(VARIABLE[count]))
                {
                    Errln("Error at test number " + count);
                }
            }
            m_en_us_.IsAlternateHandlingShifted = (false);
        }


        internal class TestSearch : SearchIterator
        {
            internal String pattern;
            internal String text;

            internal TestSearch(StringCharacterEnumerator target, BreakIterator breaker,
                           String pattern)
                    : base(target, breaker)
            {
                this.pattern = pattern;
                StringBuffer buffer = new StringBuffer();
                do
                {
                    buffer.Append(m_targetText.Current);
                } while (m_targetText.MoveNext());
                text = buffer.ToString();
                m_targetText.Index = m_targetText.StartIndex;
            }

            protected override int HandleNext(int start)
            {
                int match = text.IndexOf(pattern, start, StringComparison.Ordinal);
                if (match < 0)
                {
                    m_targetText.MoveLast();
                    return Done;
                }
                m_targetText.TrySetIndex(match);
                MatchLength = (pattern.Length);
                return match;
            }

            protected override int HandlePrevious(int start)
            {
                int match = text.LastIndexOf(pattern, start /*- 1*/, StringComparison.Ordinal);
                if (match < 0)
                {
                    m_targetText.TrySetIndex(0);
                    return Done;
                }
                m_targetText.TrySetIndex(match);
                MatchLength = (pattern.Length);
                return match;
            }

            public override int Index
            {
                get
                {
                    int result = m_targetText.Index;
                    if (result < 0 || result >= text.Length)
                    {
                        return Done;
                    }
                    return result;
                }
            }
        }

        [Test]
        public void TestSubClass()
        {

            TestSearch search = new TestSearch(
                    new StringCharacterEnumerator("abc abcd abc"),
                    null, "abc");
            int[] expected = { 0, 4, 9 };
            for (int i = 0; i < expected.Length; i++)
            {
                if (search.Next() != expected[i])
                {
                    Errln("Error getting next match");
                }
                if (search.MatchLength != search.pattern.Length)
                {
                    Errln("Error getting next match length");
                }
            }
            if (search.Next() != SearchIterator.Done)
            {
                Errln("Error should have reached the end of the iteration");
            }
            for (int i = expected.Length - 1; i >= 0; i--)
            {
                if (search.Previous() != expected[i])
                {
                    Errln("Error getting next match");
                }
                if (search.MatchLength != search.pattern.Length)
                {
                    Errln("Error getting next match length");
                }
            }
            if (search.Previous() != SearchIterator.Done)
            {
                Errln("Error should have reached the start of the iteration");
            }
        }

        //Test for ticket 5024
        [Test]
        public void TestDiactricMatch()
        {
            String pattern = "pattern";
            String text = "text";
            StringSearch strsrch = null;
            try
            {
                strsrch = new StringSearch(pattern, text);
            }
            catch (Exception e)
            {
                Errln("Error opening string search ");
                return;
            }

            for (int count = 0; count < DIACTRICMATCH.Length; count++)
            {
                strsrch.SetCollator(getCollator(DIACTRICMATCH[count].collator));
                strsrch.Collator.Strength = (DIACTRICMATCH[count].strength);
                strsrch.SetBreakIterator(getBreakIterator(DIACTRICMATCH[count].breaker));
                strsrch.Reset();
                text = DIACTRICMATCH[count].text;
                pattern = DIACTRICMATCH[count].pattern;
                strsrch.SetTarget(new StringCharacterEnumerator(text));
                strsrch.Pattern = (pattern);
                if (!assertEqualWithStringSearch(strsrch, DIACTRICMATCH[count]))
                {
                    Errln("Error at test number " + count);
                }
            }
        }


        const String scKoText =
            " " +
    /*01*/  "\uAC00 " +                   // simple LV Hangul
                                          /*03*/  "\uAC01 " +                   // simple LVT Hangul
                                                                                /*05*/  "\uAC0F " +                   // LVTT, last jamo expands for search
                                                                                                                      /*07*/  "\uAFFF " +                   // LLVVVTT, every jamo expands for search
                                                                                                                                                            /*09*/  "\u1100\u1161\u11A8 " +       // 0xAC01 as conjoining jamo
                                                                                                                                                                                                  /*13*/  "\u1100\u1161\u1100 " +       // 0xAC01 as basic conjoining jamo (per search rules)
                                                                                                                                                                                                                                        /*17*/  "\u3131\u314F\u3131 " +       // 0xAC01 as compatibility jamo
                                                                                                                                                                                                                                                                              /*21*/  "\u1100\u1161\u11B6 " +       // 0xAC0F as conjoining jamo; last expands for search
                                                                                                                                                                                                                                                                                                                    /*25*/  "\u1100\u1161\u1105\u1112 " + // 0xAC0F as basic conjoining jamo; last expands for search
                                                                                                                                                                                                                                                                                                                                                          /*30*/  "\u1101\u1170\u11B6 " +       // 0xAFFF as conjoining jamo; all expand for search
                                                                                                                                                                                                                                                                                                                                                                                                /*34*/  "\u00E6 " +                   // small letter ae, expands
                                                                                                                                                                                                                                                                                                                                                                                                                                      /*36*/  "\u1E4D " +                   // small letter o with tilde and acute, decomposes
            "";

        internal const String scKoPat0 = "\uAC01";
        internal const String scKoPat1 = "\u1100\u1161\u11A8"; // 0xAC01 as conjoining jamo
        internal const String scKoPat2 = "\uAC0F";
        internal const String scKoPat3 = "\u1100\u1161\u1105\u1112"; // 0xAC0F as basic conjoining jamo
        internal const String scKoPat4 = "\uAFFF";
        internal const String scKoPat5 = "\u1101\u1170\u11B6"; // 0xAFFF as conjoining jamo

        internal static int[] scKoSrchOff01 = { 3, 9, 13 };
        internal static int[] scKoSrchOff23 = { 5, 21, 25 };
        internal static int[] scKoSrchOff45 = { 7, 30 };

        internal static int[] scKoStndOff01 = { 3, 9 };
        internal static int[] scKoStndOff2 = { 5, 21 };
        internal static int[] scKoStndOff3 = { 25 };
        internal static int[] scKoStndOff45 = { 7, 30 };

        internal class PatternAndOffsets
        {
            private String pattern;
            private int[] offsets;
            internal PatternAndOffsets(String pat, int[] offs)
            {
                pattern = pat;
                offsets = offs;
            }
            public String getPattern() { return pattern; }
            public int[] getOffsets() { return offsets; }
        }
        PatternAndOffsets[] scKoSrchPatternsOffsets = {
            new PatternAndOffsets(scKoPat0, scKoSrchOff01 ),
            new PatternAndOffsets(scKoPat1, scKoSrchOff01 ),
            new PatternAndOffsets(scKoPat2, scKoSrchOff23 ),
            new PatternAndOffsets(scKoPat3, scKoSrchOff23 ),
            new PatternAndOffsets(scKoPat4, scKoSrchOff45 ),
            new PatternAndOffsets(scKoPat5, scKoSrchOff45 ),
        };
        PatternAndOffsets[] scKoStndPatternsOffsets = {
            new PatternAndOffsets(scKoPat0, scKoStndOff01 ),
            new PatternAndOffsets(scKoPat1, scKoStndOff01 ),
            new PatternAndOffsets(scKoPat2, scKoStndOff2  ),
            new PatternAndOffsets(scKoPat3, scKoStndOff3  ),
            new PatternAndOffsets(scKoPat4, scKoStndOff45 ),
            new PatternAndOffsets(scKoPat5, scKoStndOff45 ),
        };

        internal class TUSCItem
        {
            private String localeString;
            private String text;
            private PatternAndOffsets[] patternsAndOffsets;
            internal TUSCItem(String locStr, String txt, PatternAndOffsets[] patsAndOffs)
            {
                localeString = locStr;
                text = txt;
                patternsAndOffsets = patsAndOffs;
            }
            public String getLocaleString() { return localeString; }
            public String getText() { return text; }
            public PatternAndOffsets[] getPatternsAndOffsets() { return patternsAndOffsets; }
        }

        [Test]
        public void TestUsingSearchCollator()
        {

            TUSCItem[] tuscItems = {
            new TUSCItem( "root", scKoText, scKoStndPatternsOffsets ),
            new TUSCItem( "root@collation=search", scKoText, scKoSrchPatternsOffsets ),
            new TUSCItem( "ko@collation=search", scKoText, scKoSrchPatternsOffsets ),
        };

            String dummyPat = "a";

            foreach (TUSCItem tuscItem in tuscItems)
            {
                String localeString = tuscItem.getLocaleString();
                UCultureInfo uloc = new UCultureInfo(localeString);
                RuleBasedCollator col = null;
                try
                {
                    col = (RuleBasedCollator)Collator.GetInstance(uloc);
                }
                catch (Exception e)
                {
                    Errln("Error: in locale " + localeString + ", err in Collator.getInstance");
                    continue;
                }
                StringCharacterEnumerator ci = new StringCharacterEnumerator(tuscItem.getText());
                StringSearch srch = new StringSearch(dummyPat, ci, col);
                foreach (PatternAndOffsets patternAndOffsets in tuscItem.getPatternsAndOffsets())
                {
                    srch.Pattern = (patternAndOffsets.getPattern());
                    int[] offsets = patternAndOffsets.getOffsets();
                    int ioff, noff = offsets.Length;
                    int offset;

                    srch.Reset();
                    ioff = 0;
                    while (true)
                    {
                        offset = srch.Next();
                        if (offset == SearchIterator.Done)
                        {
                            break;
                        }
                        if (ioff < noff)
                        {
                            if (offset != offsets[ioff])
                            {
                                Errln("Error: in locale " + localeString + ", expected SearchIterator.Next() " + offsets[ioff] + ", got " + offset);
                                //ioff = noff;
                                //break;
                            }
                            ioff++;
                        }
                        else
                        {
                            Errln("Error: in locale " + localeString + ", SearchIterator.Next() returned more matches than expected");
                        }
                    }
                    if (ioff < noff)
                    {
                        Errln("Error: in locale " + localeString + ", SearchIterator.Next() returned fewer matches than expected");
                    }

                    srch.Reset();
                    ioff = noff;
                    while (true)
                    {
                        offset = srch.Previous();
                        if (offset == SearchIterator.Done)
                        {
                            break;
                        }
                        if (ioff > 0)
                        {
                            ioff--;
                            if (offset != offsets[ioff])
                            {
                                Errln("Error: in locale " + localeString + ", expected SearchIterator.previous() " + offsets[ioff] + ", got " + offset);
                                //ioff = 0;
                                // break;
                            }
                        }
                        else
                        {
                            Errln("Error: in locale " + localeString + ", expected SearchIterator.previous() returned more matches than expected");
                        }
                    }
                    if (ioff > 0)
                    {
                        Errln("Error: in locale " + localeString + ", expected SearchIterator.previous() returned fewer matches than expected");
                    }
                }
            }
        }

        [Test]
        public void TestIndicPrefixMatch()
        {
            for (int count = 0; count < INDICPREFIXMATCH.Length; count++)
            {
                if (!assertEqual(INDICPREFIXMATCH[count]))
                {
                    Errln("Error at test number" + count);
                }
            }
        }


        // Test case for ticket#12555
        [Test]
        public void TestLongPattern()
        {
            StringBuilder pattern = new StringBuilder();
            for (int i = 0; i < 255; i++)
            {
                pattern.Append('a');
            }
            // appends a character producing multiple ce32 at
            // index 256.
            pattern.Append('á');

            ICharacterEnumerator target = new StringCharacterEnumerator("not important");
            try
            {
                StringSearch ss = new StringSearch(pattern.ToString(), target, new CultureInfo("en"));
                assertNotNull("Non-null StringSearch instance", ss);
            }
            catch (Exception e)
            {
                Errln("Error initializing a new StringSearch object");
            }
        }
    }
}
