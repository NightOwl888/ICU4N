using ICU4N.Impl;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UTF16Plus = ICU4N.Impl.Normalizer2Impl.UTF16Plus; // ICU4N TODO: De-nest?

namespace ICU4N.Dev.Test.Normalizers
{
    /// <summary>
    /// UTS #46 (IDNA2008) test.
    /// </summary>
    /// <author>Markus Scherer</author>
    /// <since>2010jul10</since>
    public class UTS46Test : TestFmwk
    {
        public UTS46Test()
        {
            UTS46Options commonOptions =
                UTS46Options.UseSTD3Rules | UTS46Options.CheckBiDi |
                UTS46Options.CheckContextJ | UTS46Options.CheckContextO;
            trans = IDNA.GetUTS46Instance(commonOptions);
            nontrans = IDNA.GetUTS46Instance(commonOptions |
                                           UTS46Options.NontransitionalToASCII | UTS46Options.NontransitionalToUnicode);
        }

        [Test]
        public void TestAPI()
        {
            StringBuilder result = new StringBuilder();
            IDNA.Info info = new IDNA.Info();
            String input = "www.eXample.cOm";
            String expected = "www.example.com";
            trans.NameToASCII(input, result, info);
            if (info.HasErrors || !UTF16Plus.Equal(result, expected))
            {
                Errln(String.Format("T.nameToASCII(www.example.com) info.errors={0} result matches={1}",
                                    CollectionUtil.ToString(info.Errors), UTF16Plus.Equal(result, expected)));
            }
            input = "xn--bcher.de-65a";
            expected = "xn--bcher\uFFFDde-65a";
            nontrans.LabelToASCII(input, result, info);
            if (!info.Errors.SetEquals(new HashSet<IDNAError> { IDNAError.LabelHasDot, IDNAError.InvalidAceLabel }) ||
                !UTF16Plus.Equal(result, expected)
            )
            {
                Errln(String.Format("N.labelToASCII(label-with-dot) failed with errors {0}",
                                    CollectionUtil.ToString(info.Errors)));
            }
            // Java API tests that are not parallel to C++ tests
            // because the C++ specifics (error codes etc.) do not apply here.
            String resultString = trans.NameToUnicode("fA\u00DF.de", result, info).ToString();
            if (info.HasErrors || !resultString.Equals("fass.de"))
            {
                Errln(String.Format("T.nameToUnicode(fA\u00DF.de) info.errors={0} result matches={1}",
                                    CollectionUtil.ToString(info.Errors), resultString.Equals("fass.de")));
            }
            try
            {
                nontrans.LabelToUnicode(result, result, info);
                Errln("N.labelToUnicode(result, result) did not throw an Exception");
            }
            catch (Exception e)
            {
                // as expected (should be an IllegalArgumentException, or an ICU version of it)
            }
        }

        [Test]
        public void TestNotSTD3()
        {
            IDNA not3 = IDNA.GetUTS46Instance(UTS46Options.CheckBiDi);
            String input = "\u0000A_2+2=4\n.e\u00DFen.net";
            StringBuilder result = new StringBuilder();
            IDNA.Info info = new IDNA.Info();
            if (!not3.NameToUnicode(input, result, info).ToString().Equals("\u0000a_2+2=4\n.essen.net") ||
                info.HasErrors
            )
            {
                Errln(String.Format("notSTD3.nameToUnicode(non-LDH ASCII) unexpected errors {0} string {1}",
                                    CollectionUtil.ToString(info.Errors), Prettify(result.ToString())));
            }
            // A space (BiDi class WS) is not allowed in a BiDi domain name.
            input = "a z.xn--4db.edu";
            not3.NameToASCII(input, result, info);
            if (!UTF16Plus.Equal(result, input) || !info.Errors.SetEquals(new HashSet<IDNAError> { IDNAError.BiDi }))
            {
                Errln("notSTD3.nameToASCII(ASCII-with-space.alef.edu) failed");
            }
            // Characters that are canonically equivalent to sequences with non-LDH ASCII.
            input = "a\u2260b\u226Ec\u226Fd";
            not3.NameToUnicode(input, result, info);
            if (!UTF16Plus.Equal(result, input) || info.HasErrors)
            {
                Errln(String.Format("notSTD3.nameToUnicode(equiv to non-LDH ASCII) unexpected errors {0} string {1}",
                                    CollectionUtil.ToString(info.Errors), Prettify(result.ToString())));
            }
        }

        private static readonly IDictionary<string, IDNAError> errorNamesToErrors;
        static UTS46Test()
        {
            errorNamesToErrors = new SortedDictionary<string, IDNAError>(StringComparer.Ordinal)
            {
                { "UIDNA_ERROR_EMPTY_LABEL", IDNAError.EmptyLabel },
                { "UIDNA_ERROR_LABEL_TOO_LONG", IDNAError.LabelTooLong },
                { "UIDNA_ERROR_DOMAIN_NAME_TOO_LONG", IDNAError.DomainNameTooLong },
                { "UIDNA_ERROR_LEADING_HYPHEN", IDNAError.LeadingHyphen },
                { "UIDNA_ERROR_TRAILING_HYPHEN", IDNAError.TrailingHyphen },
                { "UIDNA_ERROR_HYPHEN_3_4", IDNAError.Hyphen_3_4 },
                { "UIDNA_ERROR_LEADING_COMBINING_MARK", IDNAError.LeadingCombiningMark },
                { "UIDNA_ERROR_DISALLOWED", IDNAError.Disallowed },
                { "UIDNA_ERROR_PUNYCODE", IDNAError.Punycode },
                { "UIDNA_ERROR_LABEL_HAS_DOT", IDNAError.LabelHasDot },
                { "UIDNA_ERROR_INVALID_ACE_LABEL", IDNAError.InvalidAceLabel },
                { "UIDNA_ERROR_BIDI", IDNAError.BiDi },
                { "UIDNA_ERROR_CONTEXTJ", IDNAError.ContextJ },
                { "UIDNA_ERROR_CONTEXTO_PUNCTUATION", IDNAError.ContextOPunctuation },
                { "UIDNA_ERROR_CONTEXTO_DIGITS", IDNAError.ContextODigits },
            };
        }

        private sealed class TestCase
        {
            internal TestCase()
            {
                errors = new HashSet<IDNAError>();
            }
            internal void Set(string[] data)
            {
                s = data[0];
                o = data[1];
                u = data[2];
                errors.Clear();
                if (data[3].Length != 0)
                {
                    foreach (string e in Regex.Split(data[3], "\\|"))
                    {
                        errors.Add(errorNamesToErrors.Get(e));
                    }
                }
            }
            // Input string and options string (Nontransitional/Transitional/Both).
            internal string s, o;
            // Expected Unicode result string.
            internal string u;
            internal ISet<IDNAError> errors;
        };

        private static readonly string[][] testCases ={
            new string[] { "www.eXample.cOm", "B",  // all ASCII
              "www.example.com", "" },
            new string[] { "B\u00FCcher.de", "B",  // u-umlaut
              "b\u00FCcher.de", "" },
            new string[] { "\u00D6BB", "B",  // O-umlaut
              "\u00F6bb", "" },
            new string[] { "fa\u00DF.de", "N",  // sharp s
              "fa\u00DF.de", "" },
            new string[] { "fa\u00DF.de", "T",  // sharp s
              "fass.de", "" },
            new string[] { "XN--fA-hia.dE", "B",  // sharp s in Punycode
              "fa\u00DF.de", "" },
            new string[] { "\u03B2\u03CC\u03BB\u03BF\u03C2.com", "N",  // Greek with final sigma
              "\u03B2\u03CC\u03BB\u03BF\u03C2.com", "" },
            new string[] { "\u03B2\u03CC\u03BB\u03BF\u03C2.com", "T",  // Greek with final sigma
              "\u03B2\u03CC\u03BB\u03BF\u03C3.com", "" },
            new string[] { "xn--nxasmm1c", "B",  // Greek with final sigma in Punycode
              "\u03B2\u03CC\u03BB\u03BF\u03C2", "" },
            new string[] { "www.\u0DC1\u0DCA\u200D\u0DBB\u0DD3.com", "N",  // "Sri" in "Sri Lanka" has a ZWJ
              "www.\u0DC1\u0DCA\u200D\u0DBB\u0DD3.com", "" },
            new string[] { "www.\u0DC1\u0DCA\u200D\u0DBB\u0DD3.com", "T",  // "Sri" in "Sri Lanka" has a ZWJ
              "www.\u0DC1\u0DCA\u0DBB\u0DD3.com", "" },
            new string[] { "www.xn--10cl1a0b660p.com", "B",  // "Sri" in Punycode
              "www.\u0DC1\u0DCA\u200D\u0DBB\u0DD3.com", "" },
            new string[] { "\u0646\u0627\u0645\u0647\u200C\u0627\u06CC", "N",  // ZWNJ
              "\u0646\u0627\u0645\u0647\u200C\u0627\u06CC", "" },
            new string[] { "\u0646\u0627\u0645\u0647\u200C\u0627\u06CC", "T",  // ZWNJ
              "\u0646\u0627\u0645\u0647\u0627\u06CC", "" },
            new string[] { "xn--mgba3gch31f060k.com", "B",  // ZWNJ in Punycode
              "\u0646\u0627\u0645\u0647\u200C\u0627\u06CC.com", "" },
            new string[] { "a.b\uFF0Ec\u3002d\uFF61", "B",
              "a.b.c.d.", "" },
            new string[] { "U\u0308.xn--tda", "B",  // U+umlaut.u-umlaut
              "\u00FC.\u00FC", "" },
            new string[] { "xn--u-ccb", "B",  // u+umlaut in Punycode
              "xn--u-ccb\uFFFD", "UIDNA_ERROR_INVALID_ACE_LABEL" },
            new string[] { "a\u2488com", "B",  // contains 1-dot
              "a\uFFFDcom", "UIDNA_ERROR_DISALLOWED" },
            new string[] { "xn--a-ecp.ru", "B",  // contains 1-dot in Punycode
              "xn--a-ecp\uFFFD.ru", "UIDNA_ERROR_INVALID_ACE_LABEL" },
            new string[] { "xn--0.pt", "B",  // invalid Punycode
              "xn--0\uFFFD.pt", "UIDNA_ERROR_PUNYCODE" },
            new string[] { "xn--a.pt", "B",  // U+0080
              "xn--a\uFFFD.pt", "UIDNA_ERROR_INVALID_ACE_LABEL" },
            new string[] { "xn--a-\u00C4.pt", "B",  // invalid Punycode
              "xn--a-\u00E4.pt", "UIDNA_ERROR_PUNYCODE" },
            new string[] { "\u65E5\u672C\u8A9E\u3002\uFF2A\uFF30", "B",  // Japanese with fullwidth ".jp"
              "\u65E5\u672C\u8A9E.jp", "" },
            new string[] { "\u2615", "B", "\u2615", "" },  // Unicode 4.0 HOT BEVERAGE
            // some characters are disallowed because they are canonically equivalent
            // to sequences with non-LDH ASCII
            new string[] { "a\u2260b\u226Ec\u226Fd", "B",
              "a\uFFFDb\uFFFDc\uFFFDd", "UIDNA_ERROR_DISALLOWED" },
            // many deviation characters, test the special mapping code
            new string[] { "1.a\u00DF\u200C\u200Db\u200C\u200Dc\u00DF\u00DF\u00DF\u00DFd"+
              "\u03C2\u03C3\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DFe"+
              "\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DFx"+
              "\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DFy"+
              "\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u0302\u00DFz", "N",
              "1.a\u00DF\u200C\u200Db\u200C\u200Dc\u00DF\u00DF\u00DF\u00DFd"+
              "\u03C2\u03C3\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DFe"+
              "\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DFx"+
              "\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DFy"+
              "\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u0302\u00DFz",
              "UIDNA_ERROR_LABEL_TOO_LONG|UIDNA_ERROR_CONTEXTJ" },
            new string[] { "1.a\u00DF\u200C\u200Db\u200C\u200Dc\u00DF\u00DF\u00DF\u00DFd"+
              "\u03C2\u03C3\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DFe"+
              "\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DFx"+
              "\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DFy"+
              "\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u00DF\u0302\u00DFz", "T",
              "1.assbcssssssssd"+
              "\u03C3\u03C3sssssssssssssssse"+
              "ssssssssssssssssssssx"+
              "ssssssssssssssssssssy"+
              "sssssssssssssss\u015Dssz", "UIDNA_ERROR_LABEL_TOO_LONG" },
            // "xn--bss" with deviation characters
            new string[] { "\u200Cx\u200Dn\u200C-\u200D-b\u00DF", "N",
              "\u200Cx\u200Dn\u200C-\u200D-b\u00DF", "UIDNA_ERROR_CONTEXTJ" },
            new string[] { "\u200Cx\u200Dn\u200C-\u200D-b\u00DF", "T",
              "\u5919", "" },
            // "xn--bssffl" written as:
            // 02E3 MODIFIER LETTER SMALL X
            // 034F COMBINING GRAPHEME JOINER (ignored)
            // 2115 DOUBLE-STRUCK CAPITAL N
            // 200B ZERO WIDTH SPACE (ignored)
            // FE63 SMALL HYPHEN-MINUS
            // 00AD SOFT HYPHEN (ignored)
            // FF0D FULLWIDTH HYPHEN-MINUS
            // 180C MONGOLIAN FREE VARIATION SELECTOR TWO (ignored)
            // 212C SCRIPT CAPITAL B
            // FE00 VARIATION SELECTOR-1 (ignored)
            // 017F LATIN SMALL LETTER LONG S
            // 2064 INVISIBLE PLUS (ignored)
            // 1D530 MATHEMATICAL FRAKTUR SMALL S
            // E01EF VARIATION SELECTOR-256 (ignored)
            // FB04 LATIN SMALL LIGATURE FFL
            new string[] { "\u02E3\u034F\u2115\u200B\uFE63\u00AD\uFF0D\u180C"+
              "\u212C\uFE00\u017F\u2064"+"\uD835\uDD30\uDB40\uDDEF"/*1D530 E01EF*/+"\uFB04", "B",
              "\u5921\u591E\u591C\u5919", "" },
            new string[] { "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890123456789012345678901234567890123456789012345678901", "B",
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890123456789012345678901234567890123456789012345678901", "" },
            new string[] { "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890123456789012345678901234567890123456789012345678901.", "B",
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890123456789012345678901234567890123456789012345678901.", "" },
            // Domain name >256 characters, forces slow path in UTF-8 processing.
            new string[] { "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "12345678901234567890123456789012345678901234567890123456789012", "B",
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "12345678901234567890123456789012345678901234567890123456789012",
              "UIDNA_ERROR_DOMAIN_NAME_TOO_LONG" },
            new string[] { "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890123456789012345678901234567890123456789\u05D0", "B",
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890123456789012345678901234567890123456789\u05D0",
              "UIDNA_ERROR_DOMAIN_NAME_TOO_LONG|UIDNA_ERROR_BIDI" },
            new string[] { "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890123456789012345678901234567890123456789012345678901234."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890", "B",
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890123456789012345678901234567890123456789012345678901234."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890",
              "UIDNA_ERROR_LABEL_TOO_LONG" },
            new string[] { "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890123456789012345678901234567890123456789012345678901234."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890.", "B",
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890123456789012345678901234567890123456789012345678901234."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890.",
              "UIDNA_ERROR_LABEL_TOO_LONG" },
            new string[] { "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890123456789012345678901234567890123456789012345678901234."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890123456789012345678901234567890123456789012345678901", "B",
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890123456789012345678901234567890123456789012345678901234."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890123456789012345678901234567890123456789012345678901",
              "UIDNA_ERROR_LABEL_TOO_LONG|UIDNA_ERROR_DOMAIN_NAME_TOO_LONG" },
            // label length 63: xn--1234567890123456789012345678901234567890123456789012345-9te
            new string[] { "\u00E41234567890123456789012345678901234567890123456789012345", "B",
              "\u00E41234567890123456789012345678901234567890123456789012345", "" },
            new string[] { "1234567890\u00E41234567890123456789012345678901234567890123456", "B",
              "1234567890\u00E41234567890123456789012345678901234567890123456", "UIDNA_ERROR_LABEL_TOO_LONG" },
            new string[] { "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890\u00E4123456789012345678901234567890123456789012345."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890123456789012345678901234567890123456789012345678901", "B",
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890\u00E4123456789012345678901234567890123456789012345."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890123456789012345678901234567890123456789012345678901", "" },
            new string[] { "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890\u00E4123456789012345678901234567890123456789012345."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890123456789012345678901234567890123456789012345678901.", "B",
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890\u00E4123456789012345678901234567890123456789012345."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890123456789012345678901234567890123456789012345678901.", "" },
            new string[] { "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890\u00E4123456789012345678901234567890123456789012345."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "12345678901234567890123456789012345678901234567890123456789012", "B",
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890\u00E4123456789012345678901234567890123456789012345."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "12345678901234567890123456789012345678901234567890123456789012",
              "UIDNA_ERROR_DOMAIN_NAME_TOO_LONG" },
            new string[] { "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890\u00E41234567890123456789012345678901234567890123456."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890", "B",
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890\u00E41234567890123456789012345678901234567890123456."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890",
              "UIDNA_ERROR_LABEL_TOO_LONG" },
            new string[] { "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890\u00E41234567890123456789012345678901234567890123456."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890.", "B",
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890\u00E41234567890123456789012345678901234567890123456."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "123456789012345678901234567890123456789012345678901234567890.",
              "UIDNA_ERROR_LABEL_TOO_LONG" },
            new string[] { "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890\u00E41234567890123456789012345678901234567890123456."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890123456789012345678901234567890123456789012345678901", "B",
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890\u00E41234567890123456789012345678901234567890123456."+
              "123456789012345678901234567890123456789012345678901234567890123."+
              "1234567890123456789012345678901234567890123456789012345678901",
              "UIDNA_ERROR_LABEL_TOO_LONG|UIDNA_ERROR_DOMAIN_NAME_TOO_LONG" },
            // hyphen errors and empty-label errors
            // Ticket #10883: ToUnicode also checks for empty labels.
            new string[] { ".", "B", ".", "UIDNA_ERROR_EMPTY_LABEL" },
            new string[] { "\uFF0E", "B", ".", "UIDNA_ERROR_EMPTY_LABEL" },
            // "xn---q----jra"=="-q--a-umlaut-"
            new string[] { "a.b..-q--a-.e", "B", "a.b..-q--a-.e",
              "UIDNA_ERROR_EMPTY_LABEL|UIDNA_ERROR_LEADING_HYPHEN|UIDNA_ERROR_TRAILING_HYPHEN|"+
              "UIDNA_ERROR_HYPHEN_3_4" },
            new string[] { "a.b..-q--\u00E4-.e", "B", "a.b..-q--\u00E4-.e",
              "UIDNA_ERROR_EMPTY_LABEL|UIDNA_ERROR_LEADING_HYPHEN|UIDNA_ERROR_TRAILING_HYPHEN|"+
              "UIDNA_ERROR_HYPHEN_3_4" },
            new string[] { "a.b..xn---q----jra.e", "B", "a.b..-q--\u00E4-.e",
              "UIDNA_ERROR_EMPTY_LABEL|UIDNA_ERROR_LEADING_HYPHEN|UIDNA_ERROR_TRAILING_HYPHEN|"+
              "UIDNA_ERROR_HYPHEN_3_4" },
            new string[] { "a..c", "B", "a..c", "UIDNA_ERROR_EMPTY_LABEL" },
            new string[] { "a.xn--.c", "B", "a..c", "UIDNA_ERROR_EMPTY_LABEL" },
            new string[] { "a.-b.", "B", "a.-b.", "UIDNA_ERROR_LEADING_HYPHEN" },
            new string[] { "a.b-.c", "B", "a.b-.c", "UIDNA_ERROR_TRAILING_HYPHEN" },
            new string[] { "a.-.c", "B", "a.-.c", "UIDNA_ERROR_LEADING_HYPHEN|UIDNA_ERROR_TRAILING_HYPHEN" },
            new string[] { "a.bc--de.f", "B", "a.bc--de.f", "UIDNA_ERROR_HYPHEN_3_4" },
            new string[] { "\u00E4.\u00AD.c", "B", "\u00E4..c", "UIDNA_ERROR_EMPTY_LABEL" },
            new string[] { "\u00E4.xn--.c", "B", "\u00E4..c", "UIDNA_ERROR_EMPTY_LABEL" },
            new string[] { "\u00E4.-b.", "B", "\u00E4.-b.", "UIDNA_ERROR_LEADING_HYPHEN" },
            new string[] { "\u00E4.b-.c", "B", "\u00E4.b-.c", "UIDNA_ERROR_TRAILING_HYPHEN" },
            new string[] { "\u00E4.-.c", "B", "\u00E4.-.c", "UIDNA_ERROR_LEADING_HYPHEN|UIDNA_ERROR_TRAILING_HYPHEN" },
            new string[] { "\u00E4.bc--de.f", "B", "\u00E4.bc--de.f", "UIDNA_ERROR_HYPHEN_3_4" },
            new string[] { "a.b.\u0308c.d", "B", "a.b.\uFFFDc.d", "UIDNA_ERROR_LEADING_COMBINING_MARK" },
            new string[] { "a.b.xn--c-bcb.d", "B",
              "a.b.xn--c-bcb\uFFFD.d", "UIDNA_ERROR_LEADING_COMBINING_MARK|UIDNA_ERROR_INVALID_ACE_LABEL" },
            // BiDi
            new string[] { "A0", "B", "a0", "" },
            new string[] { "0A", "B", "0a", "" },  // all-LTR is ok to start with a digit (EN)
            new string[] { "0A.\u05D0", "B",  // ASCII label does not start with L/R/AL
              "0a.\u05D0", "UIDNA_ERROR_BIDI" },
            new string[] { "c.xn--0-eha.xn--4db", "B",  // 2nd label does not start with L/R/AL
              "c.0\u00FC.\u05D0", "UIDNA_ERROR_BIDI" },
            new string[] { "b-.\u05D0", "B",  // label does not end with L/EN
              "b-.\u05D0", "UIDNA_ERROR_TRAILING_HYPHEN|UIDNA_ERROR_BIDI" },
            new string[] { "d.xn----dha.xn--4db", "B",  // 2nd label does not end with L/EN
              "d.\u00FC-.\u05D0", "UIDNA_ERROR_TRAILING_HYPHEN|UIDNA_ERROR_BIDI" },
            new string[] { "a\u05D0", "B", "a\u05D0", "UIDNA_ERROR_BIDI" },  // first dir != last dir
            new string[] { "\u05D0\u05C7", "B", "\u05D0\u05C7", "" },
            new string[] { "\u05D09\u05C7", "B", "\u05D09\u05C7", "" },
            new string[] { "\u05D0a\u05C7", "B", "\u05D0a\u05C7", "UIDNA_ERROR_BIDI" },  // first dir != last dir
            new string[] { "\u05D0\u05EA", "B", "\u05D0\u05EA", "" },
            new string[] { "\u05D0\u05F3\u05EA", "B", "\u05D0\u05F3\u05EA", "" },
            new string[] { "a\u05D0Tz", "B", "a\u05D0tz", "UIDNA_ERROR_BIDI" },  // mixed dir
            new string[] { "\u05D0T\u05EA", "B", "\u05D0t\u05EA", "UIDNA_ERROR_BIDI" },  // mixed dir
            new string[] { "\u05D07\u05EA", "B", "\u05D07\u05EA", "" },
            new string[] { "\u05D0\u0667\u05EA", "B", "\u05D0\u0667\u05EA", "" },  // Arabic 7 in the middle
            new string[] { "a7\u0667z", "B", "a7\u0667z", "UIDNA_ERROR_BIDI" },  // AN digit in LTR
            new string[] { "a7\u0667", "B", "a7\u0667", "UIDNA_ERROR_BIDI" },  // AN digit in LTR
            new string[] { "\u05D07\u0667\u05EA", "B",  // mixed EN/AN digits in RTL
              "\u05D07\u0667\u05EA", "UIDNA_ERROR_BIDI" },
            new string[] { "\u05D07\u0667", "B",  // mixed EN/AN digits in RTL
              "\u05D07\u0667", "UIDNA_ERROR_BIDI" },
            // ZWJ
            new string[] { "\u0BB9\u0BCD\u200D", "N", "\u0BB9\u0BCD\u200D", "" },  // Virama+ZWJ
            new string[] { "\u0BB9\u200D", "N", "\u0BB9\u200D", "UIDNA_ERROR_CONTEXTJ" },  // no Virama
            new string[] { "\u200D", "N", "\u200D", "UIDNA_ERROR_CONTEXTJ" },  // no Virama
            // ZWNJ
            new string[] { "\u0BB9\u0BCD\u200C", "N", "\u0BB9\u0BCD\u200C", "" },  // Virama+ZWNJ
            new string[] { "\u0BB9\u200C", "N", "\u0BB9\u200C", "UIDNA_ERROR_CONTEXTJ" },  // no Virama
            new string[] { "\u200C", "N", "\u200C", "UIDNA_ERROR_CONTEXTJ" },  // no Virama
            new string[] { "\u0644\u0670\u200C\u06ED\u06EF", "N",  // Joining types D T ZWNJ T R
              "\u0644\u0670\u200C\u06ED\u06EF", "" },
            new string[] { "\u0644\u0670\u200C\u06EF", "N",  // D T ZWNJ R
              "\u0644\u0670\u200C\u06EF", "" },
            new string[] { "\u0644\u200C\u06ED\u06EF", "N",  // D ZWNJ T R
              "\u0644\u200C\u06ED\u06EF", "" },
            new string[] { "\u0644\u200C\u06EF", "N",  // D ZWNJ R
              "\u0644\u200C\u06EF", "" },
            new string[] { "\u0644\u0670\u200C\u06ED", "N",  // D T ZWNJ T
              "\u0644\u0670\u200C\u06ED", "UIDNA_ERROR_BIDI|UIDNA_ERROR_CONTEXTJ" },
            new string[] { "\u06EF\u200C\u06EF", "N",  // R ZWNJ R
              "\u06EF\u200C\u06EF", "UIDNA_ERROR_CONTEXTJ" },
            new string[] { "\u0644\u200C", "N",  // D ZWNJ
              "\u0644\u200C", "UIDNA_ERROR_BIDI|UIDNA_ERROR_CONTEXTJ" },
            new string[] { "\u0660\u0661", "B",  // Arabic-Indic Digits alone
              "\u0660\u0661", "UIDNA_ERROR_BIDI" },
            new string[] { "\u06F0\u06F1", "B",  // Extended Arabic-Indic Digits alone
              "\u06F0\u06F1", "" },
            new string[] { "\u0660\u06F1", "B",  // Mixed Arabic-Indic Digits
              "\u0660\u06F1", "UIDNA_ERROR_CONTEXTO_DIGITS|UIDNA_ERROR_BIDI" },
            // All of the CONTEXTO "Would otherwise have been DISALLOWED" characters
            // in their correct contexts,
            // then each in incorrect context.
            new string[] { "l\u00B7l\u4E00\u0375\u03B1\u05D0\u05F3\u05F4\u30FB", "B",
              "l\u00B7l\u4E00\u0375\u03B1\u05D0\u05F3\u05F4\u30FB", "UIDNA_ERROR_BIDI" },
            new string[] { "l\u00B7", "B",
              "l\u00B7", "UIDNA_ERROR_CONTEXTO_PUNCTUATION" },
            new string[] { "\u00B7l", "B",
              "\u00B7l", "UIDNA_ERROR_CONTEXTO_PUNCTUATION" },
            new string[] { "\u0375", "B",
              "\u0375", "UIDNA_ERROR_CONTEXTO_PUNCTUATION" },
            new string[] { "\u03B1\u05F3", "B",
              "\u03B1\u05F3", "UIDNA_ERROR_CONTEXTO_PUNCTUATION|UIDNA_ERROR_BIDI" },
            new string[] { "\u05F4", "B",
              "\u05F4", "UIDNA_ERROR_CONTEXTO_PUNCTUATION" },
            new string[] { "l\u30FB", "B",
              "l\u30FB", "UIDNA_ERROR_CONTEXTO_PUNCTUATION" },
            // { "", "B",
            //   "", "" },
        };

        [Test]
        public void TestSomeCases()
        {
            StringBuilder aT = new StringBuilder(), uT = new StringBuilder();
            StringBuilder aN = new StringBuilder(), uN = new StringBuilder();
            IDNA.Info aTInfo = new IDNA.Info(), uTInfo = new IDNA.Info();
            IDNA.Info aNInfo = new IDNA.Info(), uNInfo = new IDNA.Info();

            StringBuilder aTuN = new StringBuilder(), uTaN = new StringBuilder();
            StringBuilder aNuN = new StringBuilder(), uNaN = new StringBuilder();
            IDNA.Info aTuNInfo = new IDNA.Info(), uTaNInfo = new IDNA.Info();
            IDNA.Info aNuNInfo = new IDNA.Info(), uNaNInfo = new IDNA.Info();

            StringBuilder aTL = new StringBuilder(), uTL = new StringBuilder();
            StringBuilder aNL = new StringBuilder(), uNL = new StringBuilder();
            IDNA.Info aTLInfo = new IDNA.Info(), uTLInfo = new IDNA.Info();
            IDNA.Info aNLInfo = new IDNA.Info(), uNLInfo = new IDNA.Info();

            ISet<IDNAError> uniErrors = new HashSet<IDNAError>();

            TestCase testCase = new TestCase();
            int i;
            for (i = 0; i < testCases.Length; ++i)
            {
                testCase.Set(testCases[i]);
                String input = testCase.s;
                String expected = testCase.u;
                // ToASCII/ToUnicode, transitional/nontransitional
                try
                {
                    trans.NameToASCII(input, aT, aTInfo);
                    trans.NameToUnicode(input, uT, uTInfo);
                    nontrans.NameToASCII(input, aN, aNInfo);
                    nontrans.NameToUnicode(input, uN, uNInfo);
                }
                catch (Exception e)
                {
                    Errln(String.Format("first-level processing [{0}/{1}] {2} - {3}",
                                        i, testCase.o, testCase.s, e));
                    continue;
                }
                // ToUnicode does not set length-overflow errors.
                uniErrors.Clear();
                uniErrors.UnionWith(testCase.errors);
                uniErrors.ExceptWith(lengthOverflowErrors);
                char mode = testCase.o[0];
                if (mode == 'B' || mode == 'N')
                {
                    if (!sameErrors(uNInfo, uniErrors))
                    {
                        Errln(String.Format("N.nameToUnicode([{0}] {1}) unexpected errors {2}",
                                            i, testCase.s, CollectionUtil.ToString(uNInfo.Errors)));
                        continue;
                    }
                    if (!UTF16Plus.Equal(uN, expected))
                    {
                        Errln(String.Format("N.nameToUnicode([{0}] {1}) unexpected string {2}",
                                            i, testCase.s, Prettify(uN.ToString())));
                        continue;
                    }
                    if (!sameErrors(aNInfo, testCase.errors))
                    {
                        Errln(String.Format("N.nameToASCII([{0}] {1}) unexpected errors {2}",
                                            i, testCase.s, aNInfo.Errors));
                        continue;
                    }
                }
                if (mode == 'B' || mode == 'T')
                {
                    if (!sameErrors(uTInfo, uniErrors))
                    {
                        Errln(String.Format("T.nameToUnicode([{0}] {1}) unexpected errors {2}",
                                            i, testCase.s, uTInfo.Errors));
                        continue;
                    }
                    if (!UTF16Plus.Equal(uT, expected))
                    {
                        Errln(String.Format("T.nameToUnicode([{0}] {1}) unexpected string {2}",
                                            i, testCase.s, Prettify(uT.ToString())));
                        continue;
                    }
                    if (!sameErrors(aTInfo, testCase.errors))
                    {
                        Errln(String.Format("T.nameToASCII([{0}] {1}) unexpected errors {2}",
                                            i, testCase.s, aTInfo.Errors));
                        continue;
                    }
                }
                // ToASCII is all-ASCII if no severe errors
                if (!hasCertainErrors(aNInfo, severeErrors) && !IsASCII(aN))
                {
                    Errln(String.Format("N.nameToASCII([{0}] {1}) (errors {2}) result is not ASCII {3}",
                                        i, testCase.s, aNInfo.Errors, Prettify(aN.ToString())));
                    continue;
                }
                if (!hasCertainErrors(aTInfo, severeErrors) && !IsASCII(aT))
                {
                    Errln(String.Format("T.nameToASCII([{0}] {1}) (errors {2}) result is not ASCII {3}",
                                        i, testCase.s, aTInfo.Errors, Prettify(aT.ToString())));
                    continue;
                }
                if (IsVerbose())
                {
                    char m = mode == 'B' ? mode : 'N';
                    Logln(String.Format("{0}.nameToASCII([{1}] {2}) (errors {3}) result string: {4}",
                                        m, i, testCase.s, aNInfo.Errors, Prettify(aN.ToString())));
                    if (mode != 'B')
                    {
                        Logln(String.Format("T.nameToASCII([{0}] {1}) (errors {2}) result string: {3}",
                                            i, testCase.s, aTInfo.Errors, Prettify(aT.ToString())));
                    }
                }
                // second-level processing
                try
                {
                    nontrans.NameToUnicode(aT, aTuN, aTuNInfo);
                    nontrans.NameToASCII(uT, uTaN, uTaNInfo);
                    nontrans.NameToUnicode(aN, aNuN, aNuNInfo);
                    nontrans.NameToASCII(uN, uNaN, uNaNInfo);
                }
                catch (Exception e)
                {
                    Errln(String.Format("second-level processing [{0}/{1}] {2} - {3}",
                                        i, testCase.o, testCase.s, e));
                    continue;
                }
                if (!UTF16Plus.Equal(aN, uNaN))
                {
                    Errln(String.Format("N.nameToASCII([{0}] {1})!=N.nameToUnicode().N.nameToASCII() " +
                                        "(errors {2}) {3} vs. {4}",
                                        i, testCase.s, CollectionUtil.ToString(aNInfo.Errors),
                                        Prettify(aN.ToString()), Prettify(uNaN.ToString())));
                    continue;
                }
                if (!UTF16Plus.Equal(aT, uTaN))
                {
                    Errln(String.Format("T.nameToASCII([{0}] {1})!=T.nameToUnicode().N.nameToASCII() " +
                                        "(errors {2}) {3} vs. {4}",
                                        i, testCase.s, CollectionUtil.ToString(aNInfo.Errors),
                                        Prettify(aT.ToString()), Prettify(uTaN.ToString())));
                    continue;
                }
                if (!UTF16Plus.Equal(uN, aNuN))
                {
                    Errln(String.Format("N.nameToUnicode([{0}] {1})!=N.nameToASCII().N.nameToUnicode() " +
                                        "(errors {2}) {3} vs. {4}",
                                        i, testCase.s, CollectionUtil.ToString(uNInfo.Errors), Prettify(uN.ToString()), Prettify(aNuN.ToString())));
                    continue;
                }
                if (!UTF16Plus.Equal(uT, aTuN))
                {
                    Errln(String.Format("T.nameToUnicode([{0}] {1})!=T.nameToASCII().N.nameToUnicode() " +
                                        "(errors {2}) {3} vs. {4}",
                                        i, testCase.s, CollectionUtil.ToString(uNInfo.Errors),
                                        Prettify(uT.ToString()), Prettify(aTuN.ToString())));
                    continue;
                }
                // labelToUnicode
                try
                {
                    trans.LabelToASCII(input, aTL, aTLInfo);
                    trans.LabelToUnicode(input, uTL, uTLInfo);
                    nontrans.LabelToASCII(input, aNL, aNLInfo);
                    nontrans.LabelToUnicode(input, uNL, uNLInfo);
                }
                catch (Exception e)
                {
                    Errln(String.Format("labelToXYZ processing [{0}/{1}] {2} - {3}",
                                        i, testCase.o, testCase.s, e));
                    continue;
                }
                if (aN.IndexOf(".") < 0)
                {
                    if (!UTF16Plus.Equal(aN, aNL) || !sameErrors(aNInfo, aNLInfo))
                    {
                        Errln(String.Format("N.nameToASCII([{0}] {1})!=N.labelToASCII() " +
                                            "(errors {2} vs {3}) {4} vs. {5}",
                                            i, testCase.s, CollectionUtil.ToString(aNInfo.Errors), CollectionUtil.ToString(aNLInfo.Errors),
                                            Prettify(aN.ToString()), Prettify(aNL.ToString())));
                        continue;
                    }
                }
                else
                {
                    if (!hasError(aNLInfo, IDNAError.LabelHasDot))
                    {
                        Errln(String.Format("N.labelToASCII([{0}] {1}) errors {2} missing UIDNA_ERROR_LABEL_HAS_DOT",
                                            i, testCase.s, CollectionUtil.ToString(aNLInfo.Errors)));
                        continue;
                    }
                }
                if (aT.IndexOf(".") < 0)
                {
                    if (!UTF16Plus.Equal(aT, aTL) || !sameErrors(aTInfo, aTLInfo))
                    {
                        Errln(String.Format("T.nameToASCII([{0}] {1})!=T.labelToASCII() " +
                                            "(errors {2} vs {3}) {4} vs. {5}",
                                            i, testCase.s, CollectionUtil.ToString(aTInfo.Errors), CollectionUtil.ToString(aTLInfo.Errors),
                                            Prettify(aT.ToString()), Prettify(aTL.ToString())));
                        continue;
                    }
                }
                else
                {
                    if (!hasError(aTLInfo, IDNAError.LabelHasDot))
                    {
                        Errln(String.Format("T.labelToASCII([{0}] {1}) errors {2} missing UIDNA_ERROR_LABEL_HAS_DOT",
                                            i, testCase.s, CollectionUtil.ToString(aTLInfo.Errors)));
                        continue;
                    }
                }
                if (uN.IndexOf(".") < 0)
                {
                    if (!UTF16Plus.Equal(uN, uNL) || !sameErrors(uNInfo, uNLInfo))
                    {
                        Errln(String.Format("N.nameToUnicode([{0}] {1})!=N.labelToUnicode() " +
                                            "(errors {2} vs {3}) {4} vs. {5}",
                                            i, testCase.s, CollectionUtil.ToString(uNInfo.Errors), CollectionUtil.ToString(uNLInfo.Errors),
                                            Prettify(uN.ToString()), Prettify(uNL.ToString())));
                        continue;
                    }
                }
                else
                {
                    if (!hasError(uNLInfo, IDNAError.LabelHasDot))
                    {
                        Errln(String.Format("N.labelToUnicode([{0}] {1}) errors {2} missing UIDNA_ERROR_LABEL_HAS_DOT",
                                            i, testCase.s, CollectionUtil.ToString(uNLInfo.Errors)));
                        continue;
                    }
                }
                if (uT.IndexOf(".") < 0)
                {
                    if (!UTF16Plus.Equal(uT, uTL) || !sameErrors(uTInfo, uTLInfo))
                    {
                        Errln(String.Format("T.nameToUnicode([{0}] {1})!=T.labelToUnicode() " +
                                            "(errors {2} vs {3}) {4} vs. {5}",
                                            i, testCase.s, CollectionUtil.ToString(uTInfo.Errors), CollectionUtil.ToString(uTLInfo.Errors),
                                            Prettify(uT.ToString()), Prettify(uTL.ToString())));
                        continue;
                    }
                }
                else
                {
                    if (!hasError(uTLInfo, IDNAError.LabelHasDot))
                    {
                        Errln(String.Format("T.labelToUnicode([{0}] {1}) errors {2} missing UIDNA_ERROR_LABEL_HAS_DOT",
                                            i, testCase.s, CollectionUtil.ToString(uTLInfo.Errors)));
                        continue;
                    }
                }
                // Differences between transitional and nontransitional processing
                if (mode == 'B')
                {
                    if (aNInfo.IsTransitionalDifferent ||
                        aTInfo.IsTransitionalDifferent ||
                        uNInfo.IsTransitionalDifferent ||
                        uTInfo.IsTransitionalDifferent ||
                        aNLInfo.IsTransitionalDifferent ||
                        aTLInfo.IsTransitionalDifferent ||
                        uNLInfo.IsTransitionalDifferent ||
                        uTLInfo.IsTransitionalDifferent
                    )
                    {
                        Errln(String.Format("B.process([{0}] {1}) isTransitionalDifferent()", i, testCase.s));
                        continue;
                    }
                    if (!UTF16Plus.Equal(aN, aT) || !UTF16Plus.Equal(uN, uT) ||
                        !UTF16Plus.Equal(aNL, aTL) || !UTF16Plus.Equal(uNL, uTL) ||
                        !sameErrors(aNInfo, aTInfo) || !sameErrors(uNInfo, uTInfo) ||
                        !sameErrors(aNLInfo, aTLInfo) || !sameErrors(uNLInfo, uTLInfo)
                    )
                    {
                        Errln(String.Format("N.process([{0}] {1}) vs. T.process() different errors or result strings",
                                            i, testCase.s));
                        continue;
                    }
                }
                else
                {
                    if (!aNInfo.IsTransitionalDifferent ||
                        !aTInfo.IsTransitionalDifferent ||
                        !uNInfo.IsTransitionalDifferent ||
                        !uTInfo.IsTransitionalDifferent ||
                        !aNLInfo.IsTransitionalDifferent ||
                        !aTLInfo.IsTransitionalDifferent ||
                        !uNLInfo.IsTransitionalDifferent ||
                        !uTLInfo.IsTransitionalDifferent
                    )
                    {
                        Errln(String.Format("{0}.process([{1}] {2}) !isTransitionalDifferent()",
                                            testCase.o, i, testCase.s));
                        continue;
                    }
                    if (UTF16Plus.Equal(aN, aT) || UTF16Plus.Equal(uN, uT) ||
                        UTF16Plus.Equal(aNL, aTL) || UTF16Plus.Equal(uNL, uTL)
                    )
                    {
                        Errln(String.Format("N.process([{0}] {1}) vs. T.process() same result strings",
                                            i, testCase.s));
                        continue;
                    }
                }
            }
        }

        private void CheckIdnaTestResult(String line, String type,
                String expected, StringBuilder result, IDNA.Info info)
        {
            // An error in toUnicode or toASCII is indicated by a value in square brackets,
            // such as "[B5 B6]".
            bool expectedHasErrors = !string.IsNullOrEmpty(expected) && expected[0] == '[';
            if (expectedHasErrors != info.HasErrors)
            {
                Errln(String.Format(
                        "{0}  expected errors {1} != {2} = actual has errors: {3}\n    {4}",
                        type, expectedHasErrors, info.HasErrors, CollectionUtil.ToString(info.Errors), line));
            }
            if (!expectedHasErrors && !UTF16Plus.Equal(expected, result))
            {
                Errln(String.Format("{0}  expected != actual\n    {1}", type, line));
                Errln("    " + expected);
                Errln("    " + result);
            }
        }

        [Test]
        public void IdnaTest()
        {
            TextReader idnaTestFile = TestUtil.GetDataReader("unicode.IdnaTest.txt", "UTF-8");
            Regex semi = new Regex(";", RegexOptions.Compiled);
            try
            {
                string line;
                while ((line = idnaTestFile.ReadLine()) != null)
                {
                    // Remove trailing comments and whitespace.
                    int commentStart = line.IndexOf('#');
                    if (commentStart >= 0)
                    {
                        line = line.Substring(0, commentStart); // ICU4N: Checked 2nd parameter
                    }
                    String[] fields = semi.Split(line);
                    if (fields.Length <= 1)
                    {
                        continue;  // Skip empty and comment-only lines.
                    }

                    // Column 1: type - T for transitional, N for nontransitional, B for both
                    String type = fields[0].Trim();
                    char typeChar;
                    if (type.Length != 1 ||
                            ((typeChar = type[0]) != 'B' && typeChar != 'N' && typeChar != 'T'))
                    {
                        Errln("empty or unknown type field: " + line);
                        return;
                    }

                    // Column 2: source - the source string to be tested
                    String source16 = Utility.Unescape(fields[1].Trim());

                    // Column 3: toUnicode - the result of applying toUnicode to the source.
                    // A blank value means the same as the source value.
                    String unicode16 = Utility.Unescape(fields[2].Trim());
                    if (string.IsNullOrEmpty(unicode16))
                    {
                        unicode16 = source16;
                    }

                    // Column 4: toASCII - the result of applying toASCII to the source, using the specified type.
                    // A blank value means the same as the toUnicode value.
                    String ascii16 = Utility.Unescape(fields[3].Trim());
                    if (string.IsNullOrEmpty(ascii16))
                    {
                        ascii16 = unicode16;
                    }

                    // Column 5: NV8 - present if the toUnicode value would not be a valid domain name under IDNA2008. Not a normative field.
                    // Ignored as long as we do not implement and test vanilla IDNA2008.

                    // ToASCII/ToUnicode, transitional/nontransitional
                    StringBuilder uN, aN, aT;
                    IDNA.Info uNInfo, aNInfo, aTInfo;
                    nontrans.NameToUnicode(source16, uN = new StringBuilder(), uNInfo = new IDNA.Info());
                    CheckIdnaTestResult(line, "toUnicodeNontrans", unicode16, uN, uNInfo);
                    if (typeChar == 'T' || typeChar == 'B')
                    {
                        trans.NameToASCII(source16, aT = new StringBuilder(), aTInfo = new IDNA.Info());
                        CheckIdnaTestResult(line, "toASCIITrans", ascii16, aT, aTInfo);
                    }
                    if (typeChar == 'N' || typeChar == 'B')
                    {
                        nontrans.NameToASCII(source16, aN = new StringBuilder(), aNInfo = new IDNA.Info());
                        CheckIdnaTestResult(line, "toASCIINontrans", ascii16, aN, aNInfo);
                    }
                }
            }
            finally
            {
                idnaTestFile.Dispose();
            }
        }

        private readonly IDNA trans, nontrans;

        private static readonly ISet<IDNAError> severeErrors = new HashSet<IDNAError>
        {
            IDNAError.LeadingCombiningMark,
            IDNAError.Disallowed,
            IDNAError.Punycode,
            IDNAError.LabelHasDot,
            IDNAError.InvalidAceLabel
        };
        private static readonly ISet<IDNAError> lengthOverflowErrors = new HashSet<IDNAError>
        {
            IDNAError.LabelTooLong,
            IDNAError.DomainNameTooLong
        };

        private bool hasError(IDNA.Info info, IDNAError error)
        {
            return info.Errors.Contains(error);
        }
        // assumes that certainErrors is not empty
        private bool hasCertainErrors(ISet<IDNAError> errors, ISet<IDNAError> certainErrors)
        {
            return errors.Count > 0 && errors.Overlaps(certainErrors);
        }
        private bool hasCertainErrors(IDNA.Info info, ISet<IDNAError> certainErrors)
        {
            return hasCertainErrors(info.Errors, certainErrors);
        }
        private bool sameErrors(ISet<IDNAError> a, ISet<IDNAError> b)
        {
            return a.SetEquals(b);
        }
        private bool sameErrors(IDNA.Info a, IDNA.Info b)
        {
            return sameErrors(a.Errors, b.Errors);
        }
        private bool sameErrors(IDNA.Info a, ISet<IDNAError> b)
        {
            return sameErrors(a.Errors, b);
        }

        private static bool IsASCII(string str)
        {
            int length = str.Length;
            for (int i = 0; i < length; ++i)
            {
                if (str[i] >= 0x80)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsASCII(StringBuilder str)
        {
            int length = str.Length;
            for (int i = 0; i < length; ++i)
            {
                if (str[i] >= 0x80)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsASCII(ICharSequence str)
        {
            int length = str.Length;
            for (int i = 0; i < length; ++i)
            {
                if (str[i] >= 0x80)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
