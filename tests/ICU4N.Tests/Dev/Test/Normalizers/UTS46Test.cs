using ICU4N;
using ICU4N.Impl;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using ICU4N.Text;
using J2N.Collections;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

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
            ValueStringBuilder result = new ValueStringBuilder(stackalloc char[32]);
            try
            {
                Span<char> resultSpan = stackalloc char[32];

                String input = "www.eXample.cOm";
                String expected = "www.example.com";

                if (!trans.TryNameToASCII(input, ref result, out IDNAInfo info) || info.HasErrors || !UTF16Plus.Equal(result.AsSpan(), expected.AsSpan()))
                {
                    Errln(String.Format(StringFormatter.CurrentCulture, "T.TryNameToASCII(www.example.com) info.errors={0} result matches={1}",
                                        info.Errors, UTF16Plus.Equal(result.AsSpan(), expected.AsSpan())));
                }

                if (!trans.TryNameToASCII(input, resultSpan, out int charsLength, out info) || info.HasErrors || !UTF16Plus.Equal(resultSpan.Slice(0, charsLength), expected.AsSpan()))
                {
                    Errln(String.Format(StringFormatter.CurrentCulture, "T.TryNameToASCII(www.example.com) info.errors={0} result matches={1}",
                                        info.Errors, UTF16Plus.Equal(result.AsSpan(), expected.AsSpan())));
                }


                input = "xn--bcher.de-65a";
                expected = "xn--bcher\uFFFDde-65a";

                result.Length = 0;
                nontrans.TryLabelToASCII(input, ref result, out info);
                //if (!info.Errors.SetEquals(new HashSet<IDNAError> { IDNAError.LabelHasDot, IDNAError.InvalidAceLabel }) ||
                if (!sameErrors(info.Errors, IDNAErrors.LabelHasDot | IDNAErrors.InvalidAceLabel) ||
                    !UTF16Plus.Equal(result.AsSpan(), expected.AsSpan()))
                {
                    Errln(String.Format(StringFormatter.CurrentCulture, "N.LabelToASCII(label-with-dot) failed with errors {0}",
                                        info.Errors));
                }

                nontrans.TryLabelToASCII(input, resultSpan, out charsLength, out info);
                if (!sameErrors(info.Errors, IDNAErrors.LabelHasDot | IDNAErrors.InvalidAceLabel) ||
                    !UTF16Plus.Equal(resultSpan.Slice(0, charsLength), expected.AsSpan()))
                {
                    Errln(String.Format(StringFormatter.CurrentCulture, "N.TryLabelToASCII(label-with-dot) failed with errors {0}",
                                        info.Errors));
                }

                // .NET API tests that are not parallel to C++ tests
                // because the C++ specifics (error codes etc.) do not apply here.
                result.Length = 0;
                bool success = trans.TryNameToUnicode("fA\u00DF.de", ref result, out info);
                string resultString = result.ToString();
                if (!success || info.HasErrors || !resultString.Equals("fass.de"))
                {
                    Errln(String.Format(StringFormatter.CurrentCulture, "T.NameToUnicode(fA\u00DF.de) info.errors={0} result matches={1}",
                                        info.Errors, resultString.Equals("fass.de")));
                }

                if (!trans.TryNameToUnicode("fA\u00DF.de", resultSpan, out charsLength, out info) || info.HasErrors || !UTF16Plus.Equal(resultSpan.Slice(0, charsLength), "fass.de"))
                {
                    Errln(String.Format(StringFormatter.CurrentCulture, "T.TryNameToUnicode(fA\u00DF.de) info.errors={0} result matches={1}",
                                        info.Errors, resultString.Equals("fass.de")));
                }

                try
                {
                    nontrans.TryLabelToUnicode(resultSpan, resultSpan, out _, out _);
                    Errln("N.labelToUnicode(result, result) did not throw an Exception");
                }
                catch (Exception e)
                {
                    // as expected (should be an ArgumentException, or an ICU version of it)
                }

                try
                {
                    nontrans.TryLabelToUnicode(result.AsSpan(), ref result, out _);
                    Errln("N.labelToUnicode(result, result) did not throw an Exception");
                }
                catch (Exception e)
                {
                    // as expected (should be an ArgumentException, or an ICU version of it)
                }
            }
            finally
            {
                result.Dispose();
            }
        }

        [Test]
        public void TestNotSTD3()
        {
            IDNA not3 = IDNA.GetUTS46Instance(UTS46Options.CheckBiDi);
            String input = "\u0000A_2+2=4\n.e\u00DFen.net";
            Span<char> resultSpan = stackalloc char[64];
            ValueStringBuilder result = new ValueStringBuilder(stackalloc char[64]);
            try
            {
                not3.TryNameToUnicode(input, ref result, out IDNAInfo info);
                if (!UTF16Plus.Equal(result.AsSpan(), "\u0000a_2+2=4\n.essen.net") ||
                    info.HasErrors
                )
                {
                    Errln(String.Format(StringFormatter.CurrentCulture, "notSTD3.TryNameToUnicode(non-LDH ASCII) unexpected errors {0} string {1}",
                                        info.Errors, Prettify(result.AsSpan())));
                }
                not3.TryNameToUnicode(input, resultSpan, out int charsLength, out info);
                if (!UTF16Plus.Equal(resultSpan.Slice(0, charsLength), "\u0000a_2+2=4\n.essen.net") ||
                    info.HasErrors
                )
                {
                    Errln(String.Format(StringFormatter.CurrentCulture, "notSTD3.TryNameToUnicode(non-LDH ASCII) unexpected errors {0} string {1}",
                                        info.Errors, Prettify(resultSpan.Slice(0, charsLength))));
                }


                // A space (BiDi class WS) is not allowed in a BiDi domain name.
                input = "a z.xn--4db.edu";
                result.Length = 0;
                not3.TryNameToASCII(input, ref result, out info);
                if (!UTF16Plus.Equal(result.AsSpan(), input.AsSpan()) || !sameErrors(info.Errors, IDNAErrors.BiDi))
                {
                    Errln("notSTD3.TryNameToASCII(ASCII-with-space.alef.edu) failed");
                }
                not3.TryNameToASCII(input, resultSpan, out charsLength, out info);
                if (!UTF16Plus.Equal(resultSpan.Slice(0, charsLength), input.AsSpan()) || !sameErrors(info.Errors, IDNAErrors.BiDi))
                {
                    Errln("notSTD3.TryNameToASCII(ASCII-with-space.alef.edu) failed");
                }

                // Characters that are canonically equivalent to sequences with non-LDH ASCII.
                input = "a\u2260b\u226Ec\u226Fd";
                result.Length = 0;
                not3.TryNameToUnicode(input, ref result, out info);
                if (!UTF16Plus.Equal(result.AsSpan(), input.AsSpan()) || info.HasErrors)
                {
                    Errln(String.Format(StringFormatter.CurrentCulture, "notSTD3.TryNameToUnicode(equiv to non-LDH ASCII) unexpected errors {0} string {1}",
                                        info.Errors, Prettify(result.AsSpan())));
                }
                if (!not3.TryNameToUnicode(input, resultSpan, out charsLength, out info) || !UTF16Plus.Equal(resultSpan.Slice(0, charsLength), input.AsSpan()) || info.HasErrors)
                {
                    Errln(String.Format(StringFormatter.CurrentCulture, "notSTD3.TryNameToUnicode(equiv to non-LDH ASCII) unexpected errors {0} string {1}",
                                        info.Errors, Prettify(resultSpan.Slice(0, charsLength))));
                }
            }
            finally
            {
                result.Dispose();
            }
        }

        private static readonly IDictionary<string, IDNAErrors> errorNamesToErrors = new SortedDictionary<string, IDNAErrors>(StringComparer.Ordinal)
            {
                { "UIDNA_ERROR_EMPTY_LABEL", IDNAErrors.EmptyLabel },
                { "UIDNA_ERROR_LABEL_TOO_LONG", IDNAErrors.LabelTooLong },
                { "UIDNA_ERROR_DOMAIN_NAME_TOO_LONG", IDNAErrors.DomainNameTooLong },
                { "UIDNA_ERROR_LEADING_HYPHEN", IDNAErrors.LeadingHyphen },
                { "UIDNA_ERROR_TRAILING_HYPHEN", IDNAErrors.TrailingHyphen },
                { "UIDNA_ERROR_HYPHEN_3_4", IDNAErrors.Hyphen_3_4 },
                { "UIDNA_ERROR_LEADING_COMBINING_MARK", IDNAErrors.LeadingCombiningMark },
                { "UIDNA_ERROR_DISALLOWED", IDNAErrors.Disallowed },
                { "UIDNA_ERROR_PUNYCODE", IDNAErrors.Punycode },
                { "UIDNA_ERROR_LABEL_HAS_DOT", IDNAErrors.LabelHasDot },
                { "UIDNA_ERROR_INVALID_ACE_LABEL", IDNAErrors.InvalidAceLabel },
                { "UIDNA_ERROR_BIDI", IDNAErrors.BiDi },
                { "UIDNA_ERROR_CONTEXTJ", IDNAErrors.ContextJ },
                { "UIDNA_ERROR_CONTEXTO_PUNCTUATION", IDNAErrors.ContextOPunctuation },
                { "UIDNA_ERROR_CONTEXTO_DIGITS", IDNAErrors.ContextODigits },
            };

        private sealed class TestCase
        {
            internal TestCase()
            {
                errors = IDNAErrors.None;
            }
            internal void Set(string[] data)
            {
                s = data[0];
                o = data[1];
                u = data[2];
                errors = IDNAErrors.None;
                if (data[3].Length != 0)
                {
                    foreach (string e in Regex.Split(data[3], "\\|"))
                    {
                        errors |= errorNamesToErrors.Get(e);
                    }
                }
            }
            // Input string and options string (Nontransitional/Transitional/Both).
            internal string s, o;
            // Expected Unicode result string.
            internal string u;
            internal IDNAErrors errors;
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
            const int StackBufferSize = 512;

            Span<char> aTBuf = stackalloc char[StackBufferSize], uTBuf = stackalloc char[StackBufferSize];
            Span<char> aNBuf = stackalloc char[StackBufferSize], uNBuf = stackalloc char[StackBufferSize];
            ReadOnlySpan<char> aT, uT;
            ReadOnlySpan<char> aN, uN;
            IDNAInfo aTInfo, uTInfo;
            IDNAInfo aNInfo, uNInfo;

            Span<char> aTuNBuf = stackalloc char[StackBufferSize], uTaNBuf = stackalloc char[StackBufferSize];
            Span<char> aNuNBuf = stackalloc char[StackBufferSize], uNaNBuf = stackalloc char[StackBufferSize];
            ReadOnlySpan<char> aTuN, uTaN;
            ReadOnlySpan<char> aNuN, uNaN;
            IDNAInfo aTuNInfo, uTaNInfo;
            IDNAInfo aNuNInfo, uNaNInfo;

            Span<char> aTLBuf = stackalloc char[StackBufferSize], uTLBuf = stackalloc char[StackBufferSize];
            Span<char> aNLBuf = stackalloc char[StackBufferSize], uNLBuf = stackalloc char[StackBufferSize];
            ReadOnlySpan<char> aTL, uTL;
            ReadOnlySpan<char> aNL, uNL;
            IDNAInfo aTLInfo, uTLInfo;
            IDNAInfo aNLInfo, uNLInfo;

            IDNAErrors uniErrors;
            int charsLength;

            try
            {
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
                        unsafe // We know this is safe, but we need to provide the IDNAInfo outside of this block
                        {
#pragma warning disable CS9080 // Use of variable in this context may expose referenced variables outside of thier declaration scope
                            trans.TryNameToASCII(input, aTBuf, out charsLength, out aTInfo);
                            aT = aTBuf.Slice(0, charsLength);
                            trans.TryNameToUnicode(input, uTBuf, out charsLength, out uTInfo);
                            uT = uTBuf.Slice(0, charsLength);
                            nontrans.TryNameToASCII(input, aNBuf, out charsLength, out aNInfo);
                            aN = aNBuf.Slice(0, charsLength);
                            nontrans.TryNameToUnicode(input, uNBuf, out charsLength, out uNInfo);
                            uN = uNBuf.Slice(0, charsLength);
#pragma warning restore CS9080 // Use of variable in this context may expose referenced variables outside of thier declaration scope
                        }
                    }
                    catch (Exception e)
                    {
                        Errln(String.Format("first-level processing [{0}/{1}] {2} - {3}",
                                            i, testCase.o, testCase.s, e));
                        continue;
                    }
                    // ToUnicode does not set length-overflow errors.
                    uniErrors = testCase.errors & ~lengthOverflowErrors;
                    char mode = testCase.o[0];
                    if (mode == 'B' || mode == 'N')
                    {
                        if (!sameErrors(uNInfo, uniErrors))
                        {
                            Errln(String.Format(StringFormatter.CurrentCulture, "N.nameToUnicode([{0}] {1}) unexpected errors {2}",
                                                i, testCase.s, uNInfo.Errors));
                            continue;
                        }
                        if (!UTF16Plus.Equal(uN, expected))
                        {
                            Errln(String.Format("N.nameToUnicode([{0}] {1}) unexpected string {2}",
                                                i, testCase.s, Prettify(uN)));
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
                                                i, testCase.s, Prettify(uT)));
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
                                            i, testCase.s, aNInfo.Errors, Prettify(aN)));
                        continue;
                    }
                    if (!hasCertainErrors(aTInfo, severeErrors) && !IsASCII(aT))
                    {
                        Errln(String.Format("T.nameToASCII([{0}] {1}) (errors {2}) result is not ASCII {3}",
                                            i, testCase.s, aTInfo.Errors, Prettify(aT)));
                        continue;
                    }
                    if (IsVerbose())
                    {
                        char m = mode == 'B' ? mode : 'N';
                        Logln(String.Format("{0}.nameToASCII([{1}] {2}) (errors {3}) result string: {4}",
                                            m, i, testCase.s, aNInfo.Errors, Prettify(aN)));
                        if (mode != 'B')
                        {
                            Logln(String.Format("T.nameToASCII([{0}] {1}) (errors {2}) result string: {3}",
                                                i, testCase.s, aTInfo.Errors, Prettify(aT)));
                        }
                    }
                    // second-level processing
                    try
                    {
                        unsafe // We know this is safe, but we need to provide the IDNAInfo outside of this block
                        {
#pragma warning disable CS9080 // Use of variable in this context may expose referenced variables outside of thier declaration scope
                            nontrans.TryNameToUnicode(aT, aTuNBuf, out charsLength, out aTuNInfo);
                            aTuN = aTuNBuf.Slice(0, charsLength);
                            nontrans.TryNameToASCII(uT, uTaNBuf, out charsLength, out uTaNInfo);
                            uTaN = uTaNBuf.Slice(0, charsLength);
                            nontrans.TryNameToUnicode(aN, aNuNBuf, out charsLength, out aNuNInfo);
                            aNuN = aNuNBuf.Slice(0, charsLength);
                            nontrans.TryNameToASCII(uN, uNaNBuf, out charsLength, out uNaNInfo);
                            uNaN = uNaNBuf.Slice(0, charsLength);
#pragma warning restore CS9080 // Use of variable in this context may expose referenced variables outside of thier declaration scope
                        }
                    }
                    catch (Exception e)
                    {
                        Errln(String.Format("second-level processing [{0}/{1}] {2} - {3}",
                                            i, testCase.o, testCase.s, e));
                        continue;
                    }
                    if (!UTF16Plus.Equal(aN, uNaN))
                    {
                        Errln(String.Format(StringFormatter.CurrentCulture, "N.nameToASCII([{0}] {1})!=N.nameToUnicode().N.nameToASCII() " +
                                            "(errors {2}) {3} vs. {4}",
                                            i, testCase.s, aNInfo.Errors,
                                            Prettify(aN), Prettify(uNaN)));
                        continue;
                    }
                    if (!UTF16Plus.Equal(aT, uTaN))
                    {
                        Errln(String.Format(StringFormatter.CurrentCulture, "T.nameToASCII([{0}] {1})!=T.nameToUnicode().N.nameToASCII() " +
                                            "(errors {2}) {3} vs. {4}",
                                            i, testCase.s, aNInfo.Errors,
                                            Prettify(aT), Prettify(uTaN)));
                        continue;
                    }
                    if (!UTF16Plus.Equal(uN, aNuN))
                    {
                        Errln(String.Format(StringFormatter.CurrentCulture, "N.nameToUnicode([{0}] {1})!=N.nameToASCII().N.nameToUnicode() " +
                                            "(errors {2}) {3} vs. {4}",
                                            i, testCase.s, uNInfo.Errors, Prettify(uN), Prettify(aNuN)));
                        continue;
                    }
                    if (!UTF16Plus.Equal(uT, aTuN))
                    {
                        Errln(String.Format(StringFormatter.CurrentCulture, "T.nameToUnicode([{0}] {1})!=T.nameToASCII().N.nameToUnicode() " +
                                            "(errors {2}) {3} vs. {4}",
                                            i, testCase.s, uNInfo.Errors,
                                            Prettify(uT), Prettify(aTuN)));
                        continue;
                    }
                    // labelToUnicode
                    try
                    {
                        unsafe // We know this is safe, but we need to provide the IDNAInfo outside of this block
                        {
#pragma warning disable CS9080 // Use of variable in this context may expose referenced variables outside of thier declaration scope
                            trans.TryLabelToASCII(input, aTLBuf, out charsLength, out aTLInfo);
                            aTL = aTLBuf.Slice(0, charsLength);
                            trans.TryLabelToUnicode(input, uTLBuf, out charsLength, out uTLInfo);
                            uTL = uTLBuf.Slice(0, charsLength);
                            nontrans.TryLabelToASCII(input, aNLBuf, out charsLength, out aNLInfo);
                            aNL = aNLBuf.Slice(0, charsLength);
                            nontrans.TryLabelToUnicode(input, uNLBuf, out charsLength, out uNLInfo);
                            uNL = uNLBuf.Slice(0, charsLength);
#pragma warning restore CS9080 // Use of variable in this context may expose referenced variables outside of thier declaration scope
                        }
                    }
                    catch (Exception e)
                    {
                        Errln(String.Format("labelToXYZ processing [{0}/{1}] {2} - {3}",
                                            i, testCase.o, testCase.s, e));
                        continue;
                    }
                    if (aN.IndexOf(".", StringComparison.Ordinal) < 0)
                    {
                        if (!UTF16Plus.Equal(aN, aNL) || !sameErrors(aNInfo, aNLInfo))
                        {
                            Errln(String.Format(StringFormatter.CurrentCulture, "N.nameToASCII([{0}] {1})!=N.labelToASCII() " +
                                                "(errors {2} vs {3}) {4} vs. {5}",
                                                i, testCase.s, aNInfo.Errors, aNLInfo.Errors,
                                                Prettify(aN), Prettify(aNL)));
                            continue;
                        }
                    }
                    else
                    {
                        if (!hasError(aNLInfo, IDNAErrors.LabelHasDot))
                        {
                            Errln(String.Format(StringFormatter.CurrentCulture, "N.labelToASCII([{0}] {1}) errors {2} missing UIDNA_ERROR_LABEL_HAS_DOT",
                                                i, testCase.s, aNLInfo.Errors));
                            continue;
                        }
                    }
                    if (aT.IndexOf(".", StringComparison.Ordinal) < 0)
                    {
                        if (!UTF16Plus.Equal(aT, aTL) || !sameErrors(aTInfo, aTLInfo))
                        {
                            Errln(String.Format(StringFormatter.CurrentCulture, "T.nameToASCII([{0}] {1})!=T.labelToASCII() " +
                                                "(errors {2} vs {3}) {4} vs. {5}",
                                                i, testCase.s, aTInfo.Errors, aTLInfo.Errors,
                                                Prettify(aT), Prettify(aTL)));
                            continue;
                        }
                    }
                    else
                    {
                        if (!hasError(aTLInfo, IDNAErrors.LabelHasDot))
                        {
                            Errln(String.Format(StringFormatter.CurrentCulture, "T.labelToASCII([{0}] {1}) errors {2} missing UIDNA_ERROR_LABEL_HAS_DOT",
                                                i, testCase.s, aTLInfo.Errors));
                            continue;
                        }
                    }
                    if (uN.IndexOf(".", StringComparison.Ordinal) < 0)
                    {
                        if (!UTF16Plus.Equal(uN, uNL) || !sameErrors(uNInfo, uNLInfo))
                        {
                            Errln(String.Format(StringFormatter.CurrentCulture, "N.nameToUnicode([{0}] {1})!=N.labelToUnicode() " +
                                                "(errors {2} vs {3}) {4} vs. {5}",
                                                i, testCase.s, uNInfo.Errors, uNLInfo.Errors,
                                                Prettify(uN), Prettify(uNL)));
                            continue;
                        }
                    }
                    else
                    {
                        if (!hasError(uNLInfo, IDNAErrors.LabelHasDot))
                        {
                            Errln(String.Format(StringFormatter.CurrentCulture, "N.labelToUnicode([{0}] {1}) errors {2} missing UIDNA_ERROR_LABEL_HAS_DOT",
                                                i, testCase.s, uNLInfo.Errors));
                            continue;
                        }
                    }
                    if (uT.IndexOf(".", StringComparison.Ordinal) < 0)
                    {
                        if (!UTF16Plus.Equal(uT, uTL) || !sameErrors(uTInfo, uTLInfo))
                        {
                            Errln(String.Format(StringFormatter.CurrentCulture, "T.nameToUnicode([{0}] {1})!=T.labelToUnicode() " +
                                                "(errors {2} vs {3}) {4} vs. {5}",
                                                i, testCase.s, uTInfo.Errors, uTLInfo.Errors,
                                                Prettify(uT), Prettify(uTL)));
                            continue;
                        }
                    }
                    else
                    {
                        if (!hasError(uTLInfo, IDNAErrors.LabelHasDot))
                        {
                            Errln(String.Format(StringFormatter.CurrentCulture, "T.labelToUnicode([{0}] {1}) errors {2} missing UIDNA_ERROR_LABEL_HAS_DOT",
                                                i, testCase.s, uTLInfo.Errors));
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
            finally
            {
                //aT.Dispose(); uT.Dispose();
                //aN.Dispose(); uN.Dispose();
                //aTuN.Dispose(); uTaN.Dispose();
                //aNuN.Dispose(); uNaN.Dispose();
                //aTL.Dispose(); uTL.Dispose();
                //aNL.Dispose(); uNL.Dispose();
            }
        }

        [Test]
        public void TestSomeCases2()
        {
            const int StackBufferSize = 32;

            ValueStringBuilder aT = new ValueStringBuilder(stackalloc char[StackBufferSize]), uT = new ValueStringBuilder(stackalloc char[StackBufferSize]);
            ValueStringBuilder aN = new ValueStringBuilder(stackalloc char[StackBufferSize]), uN = new ValueStringBuilder(stackalloc char[StackBufferSize]);
            IDNAInfo aTInfo, uTInfo;
            IDNAInfo aNInfo, uNInfo;

            ValueStringBuilder aTuN = new ValueStringBuilder(stackalloc char[StackBufferSize]), uTaN = new ValueStringBuilder(stackalloc char[StackBufferSize]);
            ValueStringBuilder aNuN = new ValueStringBuilder(stackalloc char[StackBufferSize]), uNaN = new ValueStringBuilder(stackalloc char[StackBufferSize]);
            IDNAInfo aTuNInfo, uTaNInfo;
            IDNAInfo aNuNInfo, uNaNInfo;

            ValueStringBuilder aTL = new ValueStringBuilder(stackalloc char[StackBufferSize]), uTL = new ValueStringBuilder(stackalloc char[StackBufferSize]);
            ValueStringBuilder aNL = new ValueStringBuilder(stackalloc char[StackBufferSize]), uNL = new ValueStringBuilder(stackalloc char[StackBufferSize]);
            IDNAInfo aTLInfo, uTLInfo;
            IDNAInfo aNLInfo, uNLInfo;

            IDNAErrors uniErrors;

            try
            {
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
                        unsafe // We know this is safe, but we need to provide the IDNAInfo outside of this block
                        {
#pragma warning disable CS9080, CS9091 // Use of variable in this context may expose referenced variables outside of thier declaration scope
                            trans.TryNameToASCII(input, ref aT, out aTInfo);
                            trans.TryNameToUnicode(input, ref uT, out uTInfo);
                            nontrans.TryNameToASCII(input, ref aN, out aNInfo);
                            nontrans.TryNameToUnicode(input, ref uN, out uNInfo);
#pragma warning restore CS9080, CS9091 // Use of variable in this context may expose referenced variables outside of thier declaration scope
                        }
                    }
                    catch (Exception e)
                    {
                        Errln(String.Format("first-level processing [{0}/{1}] {2} - {3}",
                                            i, testCase.o, testCase.s, e));
                        continue;
                    }
                    // ToUnicode does not set length-overflow errors.
                    uniErrors = testCase.errors & ~lengthOverflowErrors;
                    char mode = testCase.o[0];
                    if (mode == 'B' || mode == 'N')
                    {
                        if (!sameErrors(uNInfo, uniErrors))
                        {
                            Errln(String.Format(StringFormatter.CurrentCulture, "N.nameToUnicode([{0}] {1}) unexpected errors {2}",
                                                i, testCase.s, uNInfo.Errors));
                            continue;
                        }
                        if (!UTF16Plus.Equal(uN.AsSpan(), expected))
                        {
                            Errln(String.Format("N.nameToUnicode([{0}] {1}) unexpected string {2}",
                                                i, testCase.s, Prettify(uN.AsSpan())));
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
                        if (!UTF16Plus.Equal(uT.AsSpan(), expected))
                        {
                            Errln(String.Format("T.nameToUnicode([{0}] {1}) unexpected string {2}",
                                                i, testCase.s, Prettify(uT.AsSpan())));
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
                    if (!hasCertainErrors(aNInfo, severeErrors) && !IsASCII(aN.AsSpan()))
                    {
                        Errln(String.Format("N.nameToASCII([{0}] {1}) (errors {2}) result is not ASCII {3}",
                                            i, testCase.s, aNInfo.Errors, Prettify(aN.AsSpan())));
                        continue;
                    }
                    if (!hasCertainErrors(aTInfo, severeErrors) && !IsASCII(aT.AsSpan()))
                    {
                        Errln(String.Format("T.nameToASCII([{0}] {1}) (errors {2}) result is not ASCII {3}",
                                            i, testCase.s, aTInfo.Errors, Prettify(aT.AsSpan())));
                        continue;
                    }
                    if (IsVerbose())
                    {
                        char m = mode == 'B' ? mode : 'N';
                        Logln(String.Format("{0}.nameToASCII([{1}] {2}) (errors {3}) result string: {4}",
                                            m, i, testCase.s, aNInfo.Errors, Prettify(aN.AsSpan())));
                        if (mode != 'B')
                        {
                            Logln(String.Format("T.nameToASCII([{0}] {1}) (errors {2}) result string: {3}",
                                                i, testCase.s, aTInfo.Errors, Prettify(aT.AsSpan())));
                        }
                    }
                    // second-level processing
                    try
                    {
                        unsafe // We know this is safe, but we need to provide the IDNAInfo outside of this block
                        {
#pragma warning disable CS9080, CS9091 // Use of variable in this context may expose referenced variables outside of thier declaration scope
                            nontrans.TryNameToUnicode(aT.AsSpan(), ref aTuN, out aTuNInfo);
                            nontrans.TryNameToASCII(uT.AsSpan(), ref uTaN, out uTaNInfo);
                            nontrans.TryNameToUnicode(aN.AsSpan(), ref aNuN, out aNuNInfo);
                            nontrans.TryNameToASCII(uN.AsSpan(), ref uNaN, out uNaNInfo);
#pragma warning restore CS9080, CS9091 // Use of variable in this context may expose referenced variables outside of thier declaration scope
                        }
                    }
                    catch (Exception e)
                    {
                        Errln(String.Format("second-level processing [{0}/{1}] {2} - {3}",
                                            i, testCase.o, testCase.s, e));
                        continue;
                    }
                    if (!UTF16Plus.Equal(aN.AsSpan(), uNaN.AsSpan()))
                    {
                        Errln(String.Format(StringFormatter.CurrentCulture, "N.nameToASCII([{0}] {1})!=N.nameToUnicode().N.nameToASCII() " +
                                            "(errors {2}) {3} vs. {4}",
                                            i, testCase.s, aNInfo.Errors,
                                            Prettify(aN.AsSpan()), Prettify(uNaN.AsSpan())));
                        continue;
                    }
                    if (!UTF16Plus.Equal(aT.AsSpan(), uTaN.AsSpan()))
                    {
                        Errln(String.Format(StringFormatter.CurrentCulture, "T.nameToASCII([{0}] {1})!=T.nameToUnicode().N.nameToASCII() " +
                                            "(errors {2}) {3} vs. {4}",
                                            i, testCase.s, aNInfo.Errors,
                                            Prettify(aT.AsSpan()), Prettify(uTaN.AsSpan())));
                        continue;
                    }
                    if (!UTF16Plus.Equal(uN.AsSpan(), aNuN.AsSpan()))
                    {
                        Errln(String.Format(StringFormatter.CurrentCulture, "N.nameToUnicode([{0}] {1})!=N.nameToASCII().N.nameToUnicode() " +
                                            "(errors {2}) {3} vs. {4}",
                                            i, testCase.s, uNInfo.Errors, Prettify(uN.AsSpan()), Prettify(aNuN.AsSpan())));
                        continue;
                    }
                    if (!UTF16Plus.Equal(uT.AsSpan(), aTuN.AsSpan()))
                    {
                        Errln(String.Format(StringFormatter.CurrentCulture, "T.nameToUnicode([{0}] {1})!=T.nameToASCII().N.nameToUnicode() " +
                                            "(errors {2}) {3} vs. {4}",
                                            i, testCase.s, uNInfo.Errors,
                                            Prettify(uT.AsSpan()), Prettify(aTuN.AsSpan())));
                        continue;
                    }
                    // labelToUnicode
                    try
                    {
                        unsafe // We know this is safe, but we need to provide the IDNAInfo outside of this block
                        {
#pragma warning disable CS9080, CS9091 // Use of variable in this context may expose referenced variables outside of thier declaration scope
                            trans.TryLabelToASCII(input, ref aTL, out aTLInfo);
                            trans.TryLabelToUnicode(input, ref uTL, out uTLInfo);
                            nontrans.TryLabelToASCII(input, ref aNL, out aNLInfo);
                            nontrans.TryLabelToUnicode(input, ref uNL, out uNLInfo);
#pragma warning restore CS9080, CS9091 // Use of variable in this context may expose referenced variables outside of thier declaration scope
                        }
                    }
                    catch (Exception e)
                    {
                        Errln(String.Format("labelToXYZ processing [{0}/{1}] {2} - {3}",
                                            i, testCase.o, testCase.s, e));
                        continue;
                    }
                    if (aN.IndexOf(".", StringComparison.Ordinal) < 0)
                    {
                        if (!UTF16Plus.Equal(aN.AsSpan(), aNL.AsSpan()) || !sameErrors(aNInfo, aNLInfo))
                        {
                            Errln(String.Format(StringFormatter.CurrentCulture, "N.nameToASCII([{0}] {1})!=N.labelToASCII() " +
                                                "(errors {2} vs {3}) {4} vs. {5}",
                                                i, testCase.s, aNInfo.Errors, aNLInfo.Errors,
                                                Prettify(aN.AsSpan()), Prettify(aNL.AsSpan())));
                            continue;
                        }
                    }
                    else
                    {
                        if (!hasError(aNLInfo, IDNAErrors.LabelHasDot))
                        {
                            Errln(String.Format(StringFormatter.CurrentCulture, "N.labelToASCII([{0}] {1}) errors {2} missing UIDNA_ERROR_LABEL_HAS_DOT",
                                                i, testCase.s, aNLInfo.Errors));
                            continue;
                        }
                    }
                    if (aT.IndexOf(".", StringComparison.Ordinal) < 0)
                    {
                        if (!UTF16Plus.Equal(aT.AsSpan(), aTL.AsSpan()) || !sameErrors(aTInfo, aTLInfo))
                        {
                            Errln(String.Format(StringFormatter.CurrentCulture, "T.nameToASCII([{0}] {1})!=T.labelToASCII() " +
                                                "(errors {2} vs {3}) {4} vs. {5}",
                                                i, testCase.s, aTInfo.Errors, aTLInfo.Errors,
                                                Prettify(aT.AsSpan()), Prettify(aTL.AsSpan())));
                            continue;
                        }
                    }
                    else
                    {
                        if (!hasError(aTLInfo, IDNAErrors.LabelHasDot))
                        {
                            Errln(String.Format(StringFormatter.CurrentCulture, "T.labelToASCII([{0}] {1}) errors {2} missing UIDNA_ERROR_LABEL_HAS_DOT",
                                                i, testCase.s, aTLInfo.Errors));
                            continue;
                        }
                    }
                    if (uN.IndexOf(".", StringComparison.Ordinal) < 0)
                    {
                        if (!UTF16Plus.Equal(uN.AsSpan(), uNL.AsSpan()) || !sameErrors(uNInfo, uNLInfo))
                        {
                            Errln(String.Format(StringFormatter.CurrentCulture, "N.nameToUnicode([{0}] {1})!=N.labelToUnicode() " +
                                                "(errors {2} vs {3}) {4} vs. {5}",
                                                i, testCase.s, uNInfo.Errors, uNLInfo.Errors,
                                                Prettify(uN.AsSpan()), Prettify(uNL.AsSpan())));
                            continue;
                        }
                    }
                    else
                    {
                        if (!hasError(uNLInfo, IDNAErrors.LabelHasDot))
                        {
                            Errln(String.Format(StringFormatter.CurrentCulture, "N.labelToUnicode([{0}] {1}) errors {2} missing UIDNA_ERROR_LABEL_HAS_DOT",
                                                i, testCase.s, uNLInfo.Errors));
                            continue;
                        }
                    }
                    if (uT.IndexOf(".", StringComparison.Ordinal) < 0)
                    {
                        if (!UTF16Plus.Equal(uT.AsSpan(), uTL.AsSpan()) || !sameErrors(uTInfo, uTLInfo))
                        {
                            Errln(String.Format(StringFormatter.CurrentCulture, "T.nameToUnicode([{0}] {1})!=T.labelToUnicode() " +
                                                "(errors {2} vs {3}) {4} vs. {5}",
                                                i, testCase.s, uTInfo.Errors, uTLInfo.Errors,
                                                Prettify(uT.AsSpan()), Prettify(uTL.AsSpan())));
                            continue;
                        }
                    }
                    else
                    {
                        if (!hasError(uTLInfo, IDNAErrors.LabelHasDot))
                        {
                            Errln(String.Format(StringFormatter.CurrentCulture, "T.labelToUnicode([{0}] {1}) errors {2} missing UIDNA_ERROR_LABEL_HAS_DOT",
                                                i, testCase.s, uTLInfo.Errors));
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
                        if (!UTF16Plus.Equal(aN.AsSpan(), aT.AsSpan()) || !UTF16Plus.Equal(uN.AsSpan(), uT.AsSpan()) ||
                            !UTF16Plus.Equal(aNL.AsSpan(), aTL.AsSpan()) || !UTF16Plus.Equal(uNL.AsSpan(), uTL.AsSpan()) ||
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
                        if (UTF16Plus.Equal(aN.AsSpan(), aT.AsSpan()) || UTF16Plus.Equal(uN.AsSpan(), uT.AsSpan()) ||
                            UTF16Plus.Equal(aNL.AsSpan(), aTL.AsSpan()) || UTF16Plus.Equal(uNL.AsSpan(), uTL.AsSpan())
                        )
                        {
                            Errln(String.Format("N.process([{0}] {1}) vs. T.process() same result strings",
                                                i, testCase.s));
                            continue;
                        }
                    }
                }
            }
            finally
            {
                aT.Dispose(); uT.Dispose();
                aN.Dispose(); uN.Dispose();
                aTuN.Dispose(); uTaN.Dispose();
                aNuN.Dispose(); uNaN.Dispose();
                aTL.Dispose(); uTL.Dispose();
                aNL.Dispose(); uNL.Dispose();
            }
        }

        private void CheckIdnaTestResult(String line, String type,
                String expected, ReadOnlySpan<char> result, IDNAInfo info)
        {
            // An error in toUnicode or toASCII is indicated by a value in square brackets,
            // such as "[B5 B6]".
            bool expectedHasErrors = !string.IsNullOrEmpty(expected) && expected[0] == '[';
            if (expectedHasErrors != info.HasErrors)
            {
                Errln(String.Format(StringFormatter.CurrentCulture,
                        "{0}  expected errors {1} != {2} = actual has errors: {3}\n    {4}",
                        type, expectedHasErrors, info.HasErrors, info.Errors, line));
            }
            if (!expectedHasErrors && !UTF16Plus.Equal(expected, result))
            {
                Errln(String.Format("{0}  expected != actual\n    {1}", type, line));
                Errln("    " + expected);
                Errln("    " + result.ToString());
            }
        }

        [Test]
        public void IdnaTest()
        {
            TextReader idnaTestFile = TestUtil.GetDataReader("unicode.IdnaTest.txt", "UTF-8");
            Regex semi = new Regex(";", RegexOptions.Compiled);
            ValueStringBuilder
                uN = new ValueStringBuilder(stackalloc char[32]),
                aN = new ValueStringBuilder(stackalloc char[32]),
                aT = new ValueStringBuilder(stackalloc char[32]);
            Span<char>
                uNSpan = stackalloc char[256],
                aNSpan = stackalloc char[256],
                aTSpan = stackalloc char[256];
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
                    //StringBuilder uN, aN, aT;
                    //IDNAInfo uNInfo, aNInfo, aTInfo;
                    uN.Length = 0;
                    aN.Length = 0;
                    aT.Length = 0;

                    nontrans.TryNameToUnicode(source16, ref uN, out IDNAInfo uNInfo);
                    CheckIdnaTestResult(line, "toUnicodeNontrans", unicode16, uN.AsSpan(), uNInfo);
                    if (typeChar == 'T' || typeChar == 'B')
                    {
                        trans.TryNameToASCII(source16, ref aT, out IDNAInfo aTInfo);
                        CheckIdnaTestResult(line, "toASCIITrans", ascii16, aT.AsSpan(), aTInfo);
                    }
                    if (typeChar == 'N' || typeChar == 'B')
                    {
                        nontrans.TryNameToASCII(source16, ref aN, out IDNAInfo aNInfo);
                        CheckIdnaTestResult(line, "toASCIINontrans", ascii16, aN.AsSpan(), aNInfo);
                    }

                    nontrans.TryNameToUnicode(source16, uNSpan, out int charsLength, out uNInfo);
                    CheckIdnaTestResult(line, "toUnicodeNontrans", unicode16, uNSpan.Slice(0, charsLength), uNInfo);
                    if (typeChar == 'T' || typeChar == 'B')
                    {
                        trans.TryNameToASCII(source16, aTSpan, out charsLength, out IDNAInfo aTInfo);
                        CheckIdnaTestResult(line, "toASCIITrans", ascii16, aTSpan.Slice(0, charsLength), aTInfo);
                    }
                    if (typeChar == 'N' || typeChar == 'B')
                    {
                        nontrans.TryNameToASCII(source16, aNSpan, out charsLength, out IDNAInfo aNInfo);
                        CheckIdnaTestResult(line, "toASCIINontrans", ascii16, aNSpan.Slice(0, charsLength), aNInfo);
                    }
                }
            }
            finally
            {
                uN.Dispose();
                aT.Dispose();
                aN.Dispose();
                idnaTestFile.Dispose();
            }
        }

        [Test] // ICU4N specific
        public void TestBufferOverflow()
        {
            Span<char> longBuffer = stackalloc char[512];
            Span<char> shortBuffer = stackalloc char[4];

            TestCase testCase = new TestCase();
            for (int i = 0; i < testCases.Length; ++i)
            {
                testCase.Set(testCases[i]);
                string input = testCase.s;

                // Exclude any tests with strings that will fit the short buffer.
                // Since these methods may remove chars, we add 5 just to be sure.
                if (input.Length <= shortBuffer.Length + 5)
                    continue;

                // TryLabelToASCII
                {
                    trans.TryLabelToASCII(input, longBuffer, out int longBufferLength, out IDNAInfo info);
                    if ((info.errors & IDNAErrors.BufferOverflow) != 0)
                    {
                        Errln($"IDNA.TryLabelToASCII was not suppose to return a {IDNAErrors.BufferOverflow} when there is a long enough buffer.");
                    }
                    bool success = trans.TryLabelToASCII(input, shortBuffer, out int shortBufferLength, out info);
                    if (success || (info.errors & IDNAErrors.BufferOverflow) == 0) 
                    {
                        Errln($"IDNA.TryLabelToASCII was suppose to return a {IDNAErrors.BufferOverflow} when the buffer is too short.");
                    }
                    if (shortBufferLength < longBufferLength)
                    {
                        Errln($"IDNA.TryLabelToASCII was suppose to return a buffer size large enough to fit the text.");
                    }
                }

                // TryLabelToUnicode
                {
                    trans.TryLabelToUnicode(input, longBuffer, out int longBufferLength, out IDNAInfo info);
                    if ((info.errors & IDNAErrors.BufferOverflow) != 0)
                    {
                        Errln($"IDNA.TryLabelToUnicode was not suppose to return a {IDNAErrors.BufferOverflow} when there is a long enough buffer.");
                    }
                    bool success = trans.TryLabelToUnicode(input, shortBuffer, out int shortBufferLength, out info);
                    if (success || (info.errors & IDNAErrors.BufferOverflow) == 0)
                    {
                        Errln($"IDNA.TryLabelToUnicode was suppose to return a {IDNAErrors.BufferOverflow} when the buffer is too short.");
                    }
                    if (shortBufferLength < longBufferLength)
                    {
                        Errln($"IDNA.TryLabelToUnicode was suppose to return a buffer size large enough to fit the text.");
                    }
                }

                // TryNameToASCII
                {
                    trans.TryNameToASCII(input, longBuffer, out int longBufferLength, out IDNAInfo info);
                    if ((info.errors & IDNAErrors.BufferOverflow) != 0)
                    {
                        Errln($"IDNA.TryNameToASCII was not suppose to return a {IDNAErrors.BufferOverflow} when there is a long enough buffer.");
                    }
                    bool success = trans.TryNameToASCII(input, shortBuffer, out int shortBufferLength, out info);
                    if (success || (info.errors & IDNAErrors.BufferOverflow) == 0)
                    {
                        Errln($"IDNA.TryNameToASCII was suppose to return a {IDNAErrors.BufferOverflow} when the buffer is too short.");
                    }
                    if (shortBufferLength < longBufferLength)
                    {
                        Errln($"IDNA.TryNameToASCII was suppose to return a buffer size large enough to fit the text.");
                    }
                }

                // TryNameToUnicode
                {
                    trans.TryNameToUnicode(input, longBuffer, out int longBufferLength, out IDNAInfo info);
                    if ((info.errors & IDNAErrors.BufferOverflow) != 0)
                    {
                        Errln($"IDNA.TryNameToUnicode was not suppose to return a {IDNAErrors.BufferOverflow} when there is a long enough buffer.");
                    }
                    bool success = trans.TryNameToUnicode(input, shortBuffer, out int shortBufferLength, out info);
                    if (success || (info.errors & IDNAErrors.BufferOverflow) == 0)
                    {
                        Errln($"IDNA.TryNameToUnicode was suppose to return a {IDNAErrors.BufferOverflow} when the buffer is too short.");
                    }
                    if (shortBufferLength < longBufferLength)
                    {
                        Errln($"IDNA.TryNameToUnicode was suppose to return a buffer size large enough to fit the text.");
                    }
                }
            }
        }

        private readonly IDNA trans, nontrans;

        private const IDNAErrors severeErrors =
            IDNAErrors.LeadingCombiningMark
            | IDNAErrors.Disallowed
            | IDNAErrors.Punycode
            | IDNAErrors.LabelHasDot
            | IDNAErrors.InvalidAceLabel;

        private const IDNAErrors lengthOverflowErrors =
            IDNAErrors.LabelTooLong
            | IDNAErrors.DomainNameTooLong;

        private bool hasError(IDNAInfo info, IDNAErrors error)
        {
            return (info.Errors & error) != 0;
        }
        // assumes that certainErrors is not empty
        private bool hasCertainErrors(IDNAErrors errors, IDNAErrors certainErrors)
        {
            return errors != IDNAErrors.None && (errors & certainErrors) != 0; //errors.Overlaps(certainErrors);
        }
        private bool hasCertainErrors(IDNAInfo info, IDNAErrors certainErrors)
        {
            return hasCertainErrors(info.Errors, certainErrors);
        }
        private bool sameErrors(IDNAErrors a, IDNAErrors b)
        {
            return a == b;
        }
        private bool sameErrors(IDNAInfo a, IDNAInfo b)
        {
            return sameErrors(a.Errors, b.Errors);
        }
        private bool sameErrors(IDNAInfo a, IDNAErrors b)
        {
            return sameErrors(a.Errors, b);
        }

        private static bool IsASCII(ReadOnlySpan<char> str)
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
