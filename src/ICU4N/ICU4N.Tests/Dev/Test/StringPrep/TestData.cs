using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Text;
using System;
using System.Text;

namespace ICU4N.Dev.Test.StringPrep
{
    /// <author>ram</author>
    public class TestData
    {
        public static readonly char[][] unicodeIn ={
            new char[] {
                (char)0x0644, (char)0x064A, (char)0x0647, (char)0x0645, (char)0x0627, (char)0x0628, (char)0x062A, (char)0x0643, (char)0x0644,
                (char)0x0645, (char)0x0648, (char)0x0634, (char)0x0639, (char)0x0631, (char)0x0628, (char)0x064A, (char)0x061F
            },
            new char[] {
                (char)0x4ED6, (char)0x4EEC, (char)0x4E3A, (char)0x4EC0, (char)0x4E48, (char)0x4E0D, (char)0x8BF4, (char)0x4E2D, (char)0x6587,

            },
            new char[] {
                (char)0x0050, (char)0x0072, (char)0x006F, (char)0x010D, (char)0x0070, (char)0x0072, (char)0x006F, (char)0x0073, (char)0x0074,
                (char)0x011B, (char)0x006E, (char)0x0065, (char)0x006D, (char)0x006C, (char)0x0075, (char)0x0076, (char)0x00ED, (char)0x010D,
                (char)0x0065, (char)0x0073, (char)0x006B, (char)0x0079,
            },
            new char[] {
                (char)0x05DC, (char)0x05DE, (char)0x05D4, (char)0x05D4, (char)0x05DD, (char)0x05E4, (char)0x05E9, (char)0x05D5, (char)0x05D8,
                (char)0x05DC, (char)0x05D0, (char)0x05DE, (char)0x05D3, (char)0x05D1, (char)0x05E8, (char)0x05D9, (char)0x05DD, (char)0x05E2,
                (char)0x05D1, (char)0x05E8, (char)0x05D9, (char)0x05EA,
            },
            new char[] {
                (char)0x092F, (char)0x0939, (char)0x0932, (char)0x094B, (char)0x0917, (char)0x0939, (char)0x093F, (char)0x0928, (char)0x094D,
                (char)0x0926, (char)0x0940, (char)0x0915, (char)0x094D, (char)0x092F, (char)0x094B, (char)0x0902, (char)0x0928, (char)0x0939,
                (char)0x0940, (char)0x0902, (char)0x092C, (char)0x094B, (char)0x0932, (char)0x0938, (char)0x0915, (char)0x0924, (char)0x0947,
                (char)0x0939, (char)0x0948, (char)0x0902,
            },
            new char[] {
                (char)0x306A, (char)0x305C, (char)0x307F, (char)0x3093, (char)0x306A, (char)0x65E5, (char)0x672C, (char)0x8A9E, (char)0x3092,
                (char)0x8A71, (char)0x3057, (char)0x3066, (char)0x304F, (char)0x308C, (char)0x306A, (char)0x3044, (char)0x306E, (char)0x304B,

            },
        /*  
            new char[] {
                (char)0xC138, (char)0xACC4, (char)0xC758, (char)0xBAA8, (char)0xB4E0, (char)0xC0AC, (char)0xB78C, (char)0xB4E4, (char)0xC774,
                (char)0xD55C, (char)0xAD6D, (char)0xC5B4, (char)0xB97C, (char)0xC774, (char)0xD574, (char)0xD55C, (char)0xB2E4, (char)0xBA74,
                (char)0xC5BC, (char)0xB9C8, (char)0xB098, (char)0xC88B, (char)0xC744, (char)0xAE4C,
            },
        */
            new char[] {
                (char)0x043F, (char)0x043E, (char)0x0447, (char)0x0435, (char)0x043C, (char)0x0443, (char)0x0436, (char)0x0435, (char)0x043E,
                (char)0x043D, (char)0x0438, (char)0x043D, (char)0x0435, (char)0x0433, (char)0x043E, (char)0x0432, (char)0x043E, (char)0x0440,
                (char)0x044F, (char)0x0442, (char)0x043F, (char)0x043E, (char)0x0440, (char)0x0443, (char)0x0441, (char)0x0441, (char)0x043A,
                (char)0x0438,
            },
            new char[] {
                (char)0x0050, (char)0x006F, (char)0x0072, (char)0x0071, (char)0x0075, (char)0x00E9, (char)0x006E, (char)0x006F, (char)0x0070,
                (char)0x0075, (char)0x0065, (char)0x0064, (char)0x0065, (char)0x006E, (char)0x0073, (char)0x0069, (char)0x006D, (char)0x0070,
                (char)0x006C, (char)0x0065, (char)0x006D, (char)0x0065, (char)0x006E, (char)0x0074, (char)0x0065, (char)0x0068, (char)0x0061,
                (char)0x0062, (char)0x006C, (char)0x0061, (char)0x0072, (char)0x0065, (char)0x006E, (char)0x0045, (char)0x0073, (char)0x0070,
                (char)0x0061, (char)0x00F1, (char)0x006F, (char)0x006C,
            },
            new char[] {
                (char)0x4ED6, (char)0x5011, (char)0x7232, (char)0x4EC0, (char)0x9EBD, (char)0x4E0D, (char)0x8AAA, (char)0x4E2D, (char)0x6587,

            },
            new char[] {
                (char)0x0054, (char)0x1EA1, (char)0x0069, (char)0x0073, (char)0x0061, (char)0x006F, (char)0x0068, (char)0x1ECD, (char)0x006B,
                (char)0x0068, (char)0x00F4, (char)0x006E, (char)0x0067, (char)0x0074, (char)0x0068, (char)0x1EC3, (char)0x0063, (char)0x0068,
                (char)0x1EC9, (char)0x006E, (char)0x00F3, (char)0x0069, (char)0x0074, (char)0x0069, (char)0x1EBF, (char)0x006E, (char)0x0067,
                (char)0x0056, (char)0x0069, (char)0x1EC7, (char)0x0074,
            },
            new char[] {
                (char)0x0033, (char)0x5E74, (char)0x0042, (char)0x7D44, (char)0x91D1, (char)0x516B, (char)0x5148, (char)0x751F,
            },
            new char[] {
                (char)0x5B89, (char)0x5BA4, (char)0x5948, (char)0x7F8E, (char)0x6075, (char)0x002D, (char)0x0077, (char)0x0069, (char)0x0074,
                (char)0x0068, (char)0x002D, (char)0x0053, (char)0x0055, (char)0x0050, (char)0x0045, (char)0x0052, (char)0x002D, (char)0x004D,
                (char)0x004F, (char)0x004E, (char)0x004B, (char)0x0045, (char)0x0059, (char)0x0053,
            },
            new char[] {
                (char)0x0048, (char)0x0065, (char)0x006C, (char)0x006C, (char)0x006F, (char)0x002D, (char)0x0041, (char)0x006E, (char)0x006F,
                (char)0x0074, (char)0x0068, (char)0x0065, (char)0x0072, (char)0x002D, (char)0x0057, (char)0x0061, (char)0x0079, (char)0x002D,
                (char)0x305D, (char)0x308C, (char)0x305E, (char)0x308C, (char)0x306E, (char)0x5834, (char)0x6240,
            },
            new char[] {
                (char)0x3072, (char)0x3068, (char)0x3064, (char)0x5C4B, (char)0x6839, (char)0x306E, (char)0x4E0B, (char)0x0032,
            },
            new char[] {
                (char)0x004D, (char)0x0061, (char)0x006A, (char)0x0069, (char)0x3067, (char)0x004B, (char)0x006F, (char)0x0069, (char)0x3059,
                (char)0x308B, (char)0x0035, (char)0x79D2, (char)0x524D,
            },
            new char[] {
                (char)0x30D1, (char)0x30D5, (char)0x30A3, (char)0x30FC, (char)0x0064, (char)0x0065, (char)0x30EB, (char)0x30F3, (char)0x30D0,

            },
            new char[] {
                (char)0x305D, (char)0x306E, (char)0x30B9, (char)0x30D4, (char)0x30FC, (char)0x30C9, (char)0x3067,
            },
            // test non-BMP code points
            new char[] {
                (char)0xD800, (char)0xDF00, (char)0xD800, (char)0xDF01, (char)0xD800, (char)0xDF02, (char)0xD800, (char)0xDF03, (char)0xD800, (char)0xDF05,
                (char)0xD800, (char)0xDF06, (char)0xD800, (char)0xDF07, (char)0xD800, (char)0xDF09, (char)0xD800, (char)0xDF0A, (char)0xD800, (char)0xDF0B,

            },
            new char[] {
                (char)0xD800, (char)0xDF0D, (char)0xD800, (char)0xDF0C, (char)0xD800, (char)0xDF1E, (char)0xD800, (char)0xDF0F, (char)0xD800, (char)0xDF16,
                (char)0xD800, (char)0xDF15, (char)0xD800, (char)0xDF14, (char)0xD800, (char)0xDF12, (char)0xD800, (char)0xDF10, (char)0xD800, (char)0xDF20,
                (char)0xD800, (char)0xDF21,

            },
            // Greek
            new char[] {
                (char)0x03b5, (char)0x03bb, (char)0x03bb, (char)0x03b7, (char)0x03bd, (char)0x03b9, (char)0x03ba, (char)0x03ac
            },
            // Maltese
            new char[] {
                (char)0x0062, (char)0x006f, (char)0x006e, (char)0x0121, (char)0x0075, (char)0x0073, (char)0x0061, (char)0x0127,
                (char)0x0127, (char)0x0061
            },
            // Russian
            new char[] {
                (char)0x043f, (char)0x043e, (char)0x0447, (char)0x0435, (char)0x043c, (char)0x0443, (char)0x0436, (char)0x0435,
                (char)0x043e, (char)0x043d, (char)0x0438, (char)0x043d, (char)0x0435, (char)0x0433, (char)0x043e, (char)0x0432,
                (char)0x043e, (char)0x0440, (char)0x044f, (char)0x0442, (char)0x043f, (char)0x043e, (char)0x0440, (char)0x0443,
                (char)0x0441, (char)0x0441, (char)0x043a, (char)0x0438
            },

        };

        public static readonly String[] asciiIn = {
            "xn--egbpdaj6bu4bxfgehfvwxn",
            "xn--ihqwcrb4cv8a8dqg056pqjye",
            "xn--Proprostnemluvesky-uyb24dma41a",
            "xn--4dbcagdahymbxekheh6e0a7fei0b",
            "xn--i1baa7eci9glrd9b2ae1bj0hfcgg6iyaf8o0a1dig0cd",
            "xn--n8jok5ay5dzabd5bym9f0cm5685rrjetr6pdxa",
        /*  "xn--989aomsvi5e83db1d2a355cv1e0vak1dwrv93d5xbh15a0dt30a5jpsd879ccm6fea98c",*/
            "xn--b1abfaaepdrnnbgefbaDotcwatmq2g4l",
            "xn--PorqunopuedensimplementehablarenEspaol-fmd56a",
            "xn--ihqwctvzc91f659drss3x8bo0yb",
            "xn--TisaohkhngthchnitingVit-kjcr8268qyxafd2f1b9g",
            "xn--3B-ww4c5e180e575a65lsy2b",
            "xn---with-SUPER-MONKEYS-pc58ag80a8qai00g7n9n",
            "xn--Hello-Another-Way--fc4qua05auwb3674vfr0b",
            "xn--2-u9tlzr9756bt3uc0v",
            "xn--MajiKoi5-783gue6qz075azm5e",
            "xn--de-jg4avhby1noc0d",
            "xn--d9juau41awczczp",
            "XN--097CCDEKGHQJK",
            "XN--db8CBHEJLGH4E0AL",
            "xn--hxargifdar",                       // Greek
            "xn--bonusaa-5bb1da",                   // Maltese
            "xn--b1abfaaepdrnnbgefbadotcwatmq2g4l", // Russian (Cyrillic)
           };

        public static readonly String[] domainNames = {
            "slip129-37-118-146.nc.us.ibm.net",
            "saratoga.pe.utexas.edu",
            "dial-120-45.ots.utexas.edu",
            "woo-085.dorms.waller.net",
            "hd30-049.hil.compuserve.com",
            "pem203-31.pe.ttu.edu",
            "56K-227.MaxTNT3.pdq.net",
            "dial-36-2.ots.utexas.edu",
            "slip129-37-23-152.ga.us.ibm.net",
            "ts45ip119.cadvision.com",
            "sdn-ts-004txaustP05.dialsprint.net",
            "bar-tnt1s66.erols.com",
            "101.st-louis-15.mo.dial-access.att.net",
            "h92-245.Arco.COM",
            "dial-13-2.ots.utexas.edu",
            "net-redynet29.datamarkets.com.ar",
            "ccs-shiva28.reacciun.net.ve",
            "7.houston-11.tx.dial-access.att.net",
            "ingw129-37-120-26.mo.us.ibm.net",
            "dialup6.austintx.com",
            "dns2.tpao.gov.tr",
            "slip129-37-119-194.nc.us.ibm.net",
            "cs7.dillons.co.uk.203.119.193.in-addr.arpa",
            "swprd1.innovplace.saskatoon.sk.ca",
            "bikini.bologna.maraut.it",
            "node91.subnet159-198-79.baxter.com",
            "cust19.max5.new-york.ny.ms.uu.net",
            "balexander.slip.andrew.cmu.edu",
            "pool029.max2.denver.co.dynip.alter.net",
            "cust49.max9.new-york.ny.ms.uu.net",
            "s61.abq-dialin2.hollyberry.com",

        };

        public static readonly String[] domainNames1Uni = {
            "\u0917\u0928\u0947\u0936.sanjose.ibm.com",
            "www.\u0121.com",
            //"www.\u00E0\u00B3\u00AF.com",
            "www.\u00C2\u00A4.com",
            "www.\u00C2\u00A3.com",
            // "\\u0025", //'%' (0x0025) produces U_IDNA_STD3_ASCII_RULES_ERROR
            // "\\u005C\\u005C", //'\' (0x005C) produces U_IDNA_STD3_ASCII_RULES_ERROR
            //"@",
            //"\\u002F",
            //"www.\\u0021.com",
            //"www.\\u0024.com",
            //"\\u003f",
            // These yeild U_IDNA_PROHIBITED_ERROR
            //"\\u00CF\\u0082.com",
            //"\\u00CE\\u00B2\\u00C3\\u009Fss.com",
            //"\\u00E2\\u0098\\u00BA.com",
            "\u00C3\u00BC.com"
        };
        public static readonly String[] domainNamesToASCIIOut = {
            "xn--31b8a2bwd.sanjose.ibm.com",
            "www.xn--vea.com",
            //"www.xn--3 -iia80t.com",
            "www.xn--bba7j.com",
            "www.xn--9a9j.com",
           // "\u0025",
           // "\u005C\u005C",
           // "@",
           // "\u002F",
           // "www.\u0021.com",
           // "www.\u0024.com",
           // "\u003f",
            "xn--14-ria7423a.com"

        };

        public static readonly String[] domainNamesToUnicodeOut = {
            "\u0917\u0928\u0947\u0936.sanjose.ibm.com",
            "www.\u0121.com",
            //"www.\u00E0\u0033\u0020\u0304.com",
            "www.\u00E2\u00A4.com",
            "www.\u00E2\u00A3.com",
           // "\u0025",
           // "\u005C\u005C",
           // "@",
           // "\u002F",
           // "www.\u0021.com",
           // "www.\u0024.com",
           // "\u003f",
            "\u00E3\u0031\u2044\u0034.com"

        };


        public class ErrorCase
        {

            public char[] unicode;
            public String ascii;
            public Exception expected;
            public bool useSTD3ASCIIRules;
            public bool testToUnicode;
            public bool testLabel;
            internal ErrorCase(char[] uniIn, String asciiIn, Exception ex,
                       bool std3, bool testToUni, bool testlabel)
            {
                unicode = uniIn;
                ascii = asciiIn;
                expected = ex;
                useSTD3ASCIIRules = std3;
                testToUnicode = testToUni;
                testLabel = testlabel;

            }
        }
        public static readonly ErrorCase[] errorCases = {


            new ErrorCase( new char[]{
                (char)0x0077, (char)0x0077, (char)0x0077, (char)0x002e, /* www. */
                (char)0xC138, (char)0xACC4, (char)0xC758, (char)0xBAA8, (char)0xB4E0, (char)0xC0AC, (char)0xB78C, (char)0xB4E4, (char)0xC774,
                (char)0x070F,/*prohibited*/
                (char)0xD55C, (char)0xAD6D, (char)0xC5B4, (char)0xB97C, (char)0xC774, (char)0xD574, (char)0xD55C, (char)0xB2E4, (char)0xBA74,
                (char)0x002e, (char)0x0063, (char)0x006f, (char)0x006d, /* com. */
           
            },
            "www.xn--8mb5595fsoa28orucya378bqre2tcwop06c5qbw82a1rffmae0361dea96b.com",
            new StringPrepParseException("", StringPrepErrorType.ProhibitedError),
            false, true, true),

            new ErrorCase( new char[]{
                    (char)0x0077, (char)0x0077, (char)0x0077, (char)0x002e, /* www. */
                    (char)0xC138, (char)0xACC4, (char)0xC758, (char)0xBAA8, (char)0xB4E0, (char)0xC0AC, (char)0xB78C, (char)0xB4E4, (char)0xC774,
                    (char)0x0221, (char)0x0234/*Unassigned code points*/,
                    (char)0x002e, (char)0x0063, (char)0x006f, (char)0x006d, /* com. */
               
                },
                "www.xn--6la2bz548fj1gua391bf1gb1n59ab29a7ia.com",

                new StringPrepParseException("", StringPrepErrorType.UnassignedError),
                false, true, true
            ),
           new ErrorCase( new char[]{
                    (char)0x0077, (char)0x0077, (char)0x0077, (char)0x002e, /* www. */
                    (char)0xC138, (char)0xACC4, (char)0xC758, (char)0xBAA8, (char)0xB4E0, (char)0xC0AC, (char)0xB78C, (char)0xB4E4, (char)0xC774,
                    (char)0x0644, (char)0x064A, (char)0x0647,/*Arabic code points. Cannot mix RTL with LTR*/
                    (char)0xD55C, (char)0xAD6D, (char)0xC5B4, (char)0xB97C, (char)0xC774, (char)0xD574, (char)0xD55C, (char)0xB2E4, (char)0xBA74,
                    (char)0x002e, (char)0x0063, (char)0x006f, (char)0x006d, /* com. */
               
                },
                "www.xn--ghBGI4851OiyA33VqrD6Az86C4qF83CtRv93D5xBk15AzfG0nAgA0578DeA71C.com",
                new StringPrepParseException("", StringPrepErrorType.CheckBiDiError),
                false, true, true
            ),
            new ErrorCase( new char[]{
                    (char)0x0077, (char)0x0077, (char)0x0077, (char)0x002e, /* www. */
                    /* labels cannot begin with an HYPHEN */
                    (char)0x002D, (char)0xACC4, (char)0xC758, (char)0xBAA8, (char)0xB4E0, (char)0xC0AC, (char)0xB78C, (char)0xB4E4, (char)0xC774,
                    (char)0x002E,
                    (char)0xD55C, (char)0xAD6D, (char)0xC5B4, (char)0xB97C, (char)0xC774, (char)0xD574, (char)0xD55C, (char)0xB2E4, (char)0xBA74,
                    (char)0x002e, (char)0x0063, (char)0x006f, (char)0x006d, /* com. */
               
            
                },
                "www.xn----b95Ew8SqA315Ao5FbuMlnNmhA.com",
                new StringPrepParseException("", StringPrepErrorType.STD3ASCIIRulesError),
                true, true, false
            ),
            new ErrorCase( new char[]{ 
                    /* correct ACE-prefix followed by unicode */
                    (char)0x0077, (char)0x0077, (char)0x0077, (char)0x002e, /* www. */
                    (char)0x0078, (char)0x006e, (char)0x002d,(char)0x002d,  /* ACE Prefix */
                    (char)0x002D, (char)0xACC4, (char)0xC758, (char)0xBAA8, (char)0xB4E0, (char)0xC0AC, (char)0xB78C, (char)0xB4E4, (char)0xC774,
                    (char)0x002D,
                    (char)0xD55C, (char)0xAD6D, (char)0xC5B4, (char)0xB97C, (char)0xC774, (char)0xD574, (char)0xD55C, (char)0xB2E4, (char)0xBA74,
                    (char)0x002e, (char)0x0063, (char)0x006f, (char)0x006d, /* com. */
               
            
                },
                /* wrong ACE-prefix followed by valid ACE-encoded ASCII */ 
                "www.XY-----b91I0V65S96C2A355Cw1E5yCeQr19CsnP1mFfmAE0361DeA96B.com",
                new StringPrepParseException("", StringPrepErrorType.AcePrefixError),
                false, false, false
            ),
            /* cannot verify U_IDNA_VERIFICATION_ERROR */

            new ErrorCase( new char[]{
                (char)0x0077, (char)0x0077, (char)0x0077, (char)0x002e, /* www. */
                (char)0xC138, (char)0xACC4, (char)0xC758, (char)0xBAA8, (char)0xB4E0, (char)0xC0AC, (char)0xB78C, (char)0xB4E4, (char)0xC774,
                (char)0xD55C, (char)0xAD6D, (char)0xC5B4, (char)0xB97C, (char)0xC774, (char)0xD574, (char)0xD55C, (char)0xB2E4, (char)0xBA74,
                (char)0xC5BC, (char)0xB9C8, (char)0xB098, (char)0xC88B, (char)0xC744, (char)0xAE4C,
                (char)0x002e, (char)0x0063, (char)0x006f, (char)0x006d, /* com. */
           
              },
              "www.xn--989AoMsVi5E83Db1D2A355Cv1E0vAk1DwRv93D5xBh15A0Dt30A5JpSD879Ccm6FeA98C.com",
              new StringPrepParseException("", StringPrepErrorType.LabelTooLongError),
              false, true, true
            ),
            new ErrorCase( new char[]{
                (char)0x0077, (char)0x0077, (char)0x0077, (char)0x002e, /* www. */
                (char)0x0030, (char)0x0644, (char)0x064A, (char)0x0647, (char)0x0031, /* Arabic code points squashed between EN codepoints */
                (char)0x002e, (char)0x0063, (char)0x006f, (char)0x006d, /* com. */
           
              },
              "www.xn--01-tvdmo.com",
              new StringPrepParseException("", StringPrepErrorType.CheckBiDiError),
              false, true, true
            ),

            new ErrorCase( new char[]{
                (char)0x0077, (char)0x0077, (char)0x0077, (char)0x002e, // www. 
                (char)0x206C, (char)0x0644, (char)0x064A, (char)0x0647, (char)0x206D, // Arabic code points squashed between BN codepoints 
                (char)0x002e, (char)0x0063, (char)0x006f, (char)0x006d, // com. 
           
              },
              "www.XN--ghbgi278xia.com",
              new StringPrepParseException("", StringPrepErrorType.ProhibitedError),
              false, true, true
            ),
            new ErrorCase( new char[] {
                (char)0x0077, (char)0x0077, (char)0x0077, (char)0x002e, // www. 
                (char)0x002D, (char)0x0041, (char)0x0042, (char)0x0043, (char)0x0044, (char)0x0045, // HYPHEN at the start of label 
                (char)0x002e, (char)0x0063, (char)0x006f, (char)0x006d, // com. 
           
              },
              "www.-abcde.com",
              new StringPrepParseException("", StringPrepErrorType.STD3ASCIIRulesError),
              true, false /* ToUnicode preserves casing for this case */, false
            ),
            new ErrorCase( new char[] {
                (char)0x0077, (char)0x0077, (char)0x0077, (char)0x002e, // www. 
                (char)0x0041, (char)0x0042, (char)0x0043, (char)0x0044, (char)0x0045,(char)0x002D, // HYPHEN at the end of the label
                (char)0x002e, (char)0x0063, (char)0x006f, (char)0x006d, // com. 
           
              },
              "www.abcde-.com",
              new StringPrepParseException("", StringPrepErrorType.STD3ASCIIRulesError),
              true, false /* ToUnicode preserves casing for this case */, false
            ),
            new ErrorCase( new char[]{
                (char)0x0077, (char)0x0077, (char)0x0077, (char)0x002e, // www. 
                (char)0x0041, (char)0x0042, (char)0x0043, (char)0x0044, (char)0x0045,(char)0x0040, // Containing non LDH code point
                (char)0x002e, (char)0x0063, (char)0x006f, (char)0x006d, // com. 
           
              },
              "www.abcde@.com",
              new StringPrepParseException("", StringPrepErrorType.STD3ASCIIRulesError),
              true, false /* ToUnicode preserves casing for this case */, false
            ),
            new ErrorCase( new char[]{
                (char)0x0077, (char)0x0077, (char)0x0077, (char)0x002e, // www. 
                 // zero length label
                (char)0x002e, (char)0x0063, (char)0x006f, (char)0x006d, // com. 
              },
              "www..com",
              new StringPrepParseException("", StringPrepErrorType.ZeroLengthLabel),
              true, true, false
            ),
        };


        public sealed class ConformanceTestCase
        {
            internal String comment;
            internal String input;
            internal String output;
            internal String profile;
            internal UTS46Options flags;
            internal Exception expected;
            private static byte[] GetBytes(String @in)
            {
                if (@in == null)
                {
                    return null;
                }
                byte[] bytes = new byte[@in.Length];
                for (int i = 0; i < @in.Length; i++)
                {
                    bytes[i] = (byte)@in[i];
                }
                return bytes;
            }
            internal ConformanceTestCase(String comt, String @in, String @out,
                                 String prof, UTS46Options flg, Exception ex)
            {

                try
                {
                    comment = comt;
                    byte[] bytes = GetBytes(@in);
                    input = Encoding.UTF8.GetString(bytes);
                    bytes = GetBytes(@out);
                    output = (bytes == null) ? null : Encoding.UTF8.GetString(bytes); //new String(bytes, "UTF-8");
                    profile = prof;
                    flags = flg;
                    expected = ex;
                }
                catch (Exception e)
                {
                    e.PrintStackTrace();
                    throw new Exception();
                }
            }
        }

        public static readonly ConformanceTestCase[] conformanceTestCases =
               {

                 new ConformanceTestCase(
                   "Case folding ASCII U+0043 U+0041 U+0046 U+0045",
                   "\u0043\u0041\u0046\u0045", "\u0063\u0061\u0066\u0065",
                   "Nameprep", UTS46Options.Default,
                   null

                 ),
                 new ConformanceTestCase(
                   "Case folding 8bit U+00DF (german sharp s)",
                   "\u00C3\u009F", "\u0073\u0073",
                   "Nameprep", UTS46Options.Default,
                   null
                 ),
                 new ConformanceTestCase(
                   "Non-ASCII multibyte space character U+1680",
                   "\u00E1\u009A\u0080", null,
                   "Nameprep", UTS46Options.Default,
                   new StringPrepParseException("", StringPrepErrorType.ProhibitedError)
                 ),
                 new ConformanceTestCase(
                   "Non-ASCII 8bit control character U+0085",
                   "\u00C2\u0085", null,
                   "Nameprep", UTS46Options.Default,
                   new StringPrepParseException("", StringPrepErrorType.ProhibitedError)
                 ),
                 new ConformanceTestCase(
                   "Non-ASCII multibyte control character U+180E",
                   "\u00E1\u00A0\u008E", null,
                   "Nameprep", UTS46Options.Default,
                   new StringPrepParseException("", StringPrepErrorType.ProhibitedError)
                 ),
                 new ConformanceTestCase(
                   "Non-ASCII control character U+1D175",
                   "\u00F0\u009D\u0085\u00B5", null,
                   "Nameprep", UTS46Options.Default,
                   new StringPrepParseException("", StringPrepErrorType.ProhibitedError)
                 ),
                 new ConformanceTestCase(
                   "Plane 0 private use character U+F123",
                   "\u00EF\u0084\u00A3", null,
                   "Nameprep", UTS46Options.Default,
                   new StringPrepParseException("", StringPrepErrorType.ProhibitedError)
                 ),
                 new ConformanceTestCase(
                   "Plane 15 private use character U+F1234",
                   "\u00F3\u00B1\u0088\u00B4", null,
                   "Nameprep", UTS46Options.Default,
                   new StringPrepParseException("", StringPrepErrorType.ProhibitedError)
                 ),
                 new ConformanceTestCase(
                   "Plane 16 private use character U+10F234",
                   "\u00F4\u008F\u0088\u00B4", null,
                   "Nameprep", UTS46Options.Default,
                   new StringPrepParseException("", StringPrepErrorType.ProhibitedError)
                 ),
                 new ConformanceTestCase(
                   "Non-character code point U+8FFFE",
                   "\u00F2\u008F\u00BF\u00BE", null,
                   "Nameprep", UTS46Options.Default,
                   new StringPrepParseException("", StringPrepErrorType.ProhibitedError)
                 ),
                 new ConformanceTestCase(
                   "Non-character code point U+10FFFF",
                   "\u00F4\u008F\u00BF\u00BF", null,
                   "Nameprep", UTS46Options.Default,
                   new StringPrepParseException("", StringPrepErrorType.ProhibitedError)
                 ),
             /* 
                 {
                   "Surrogate code U+DF42",
                   "\u00ED\u00BD\u0082", null, "Nameprep", InternationalizedDomainNames.DEFAULT,
                   U_IDNA_PROHIBITED_ERROR
                 },
            */
                 new ConformanceTestCase(
                   "Non-plain text character U+FFFD",
                   "\u00EF\u00BF\u00BD", null,
                   "Nameprep", UTS46Options.Default,
                   new StringPrepParseException("", StringPrepErrorType.ProhibitedError)
                 ),
                 new ConformanceTestCase(
                   "Ideographic description character U+2FF5",
                   "\u00E2\u00BF\u00B5", null,
                   "Nameprep", UTS46Options.Default,
                   new StringPrepParseException("", StringPrepErrorType.ProhibitedError)
                 ),
                 new ConformanceTestCase(
                   "Display property character U+0341",
                   "\u00CD\u0081", "\u00CC\u0081",
                   "Nameprep", UTS46Options.Default,
                   null

                 ),

                 new ConformanceTestCase(
                   "Left-to-right mark U+200E",
                   "\u00E2\u0080\u008E", "\u00CC\u0081",
                   "Nameprep", UTS46Options.Default,
                   new StringPrepParseException("", StringPrepErrorType.ProhibitedError)
                 ),
                 new ConformanceTestCase(

                   "Deprecated U+202A",
                   "\u00E2\u0080\u00AA", "\u00CC\u0081",
                   "Nameprep", UTS46Options.Default,
                   new StringPrepParseException("", StringPrepErrorType.ProhibitedError)
                 ),
                 new ConformanceTestCase(
                   "Language tagging character U+E0001",
                   "\u00F3\u00A0\u0080\u0081", "\u00CC\u0081",
                   "Nameprep", UTS46Options.Default,
                   new StringPrepParseException("", StringPrepErrorType.ProhibitedError)
                 ),
                 new ConformanceTestCase(
                   "Language tagging character U+E0042",
                   "\u00F3\u00A0\u0081\u0082", null,
                   "Nameprep", UTS46Options.Default,
                   new StringPrepParseException("", StringPrepErrorType.ProhibitedError)
                 ),
                 new ConformanceTestCase(
                   "Bidi: RandALCat character U+05BE and LCat characters",
                   "\u0066\u006F\u006F\u00D6\u00BE\u0062\u0061\u0072", null,
                   "Nameprep", UTS46Options.Default,
                   new StringPrepParseException("", StringPrepErrorType.CheckBiDiError)
                 ),
                 new ConformanceTestCase(
                   "Bidi: RandALCat character U+FD50 and LCat characters",
                   "\u0066\u006F\u006F\u00EF\u00B5\u0090\u0062\u0061\u0072", null,
                   "Nameprep", UTS46Options.Default ,
                   new StringPrepParseException("", StringPrepErrorType.CheckBiDiError)
                 ),
                 new ConformanceTestCase(
                   "Bidi: RandALCat character U+FB38 and LCat characters",
                   "\u0066\u006F\u006F\u00EF\u00B9\u00B6\u0062\u0061\u0072", "\u0066\u006F\u006F \u00d9\u008e\u0062\u0061\u0072",
                   "Nameprep", UTS46Options.Default,
                   null
                 ),
                 new ConformanceTestCase(
                   "Bidi: RandALCat without trailing RandALCat U+0627 U+0031",
                   "\u00D8\u00A7\u0031", null,
                   "Nameprep", UTS46Options.Default,
                   new StringPrepParseException("", StringPrepErrorType.CheckBiDiError)
                 ),
                 new ConformanceTestCase(
                   "Bidi: RandALCat character U+0627 U+0031 U+0628",
                   "\u00D8\u00A7\u0031\u00D8\u00A8", "\u00D8\u00A7\u0031\u00D8\u00A8",
                   "Nameprep", UTS46Options.Default,
                   null
                 ),
                 new ConformanceTestCase(
                   "Unassigned code point U+E0002",
                   "\u00F3\u00A0\u0080\u0082", null,
                   "Nameprep", UTS46Options.Default,
                   new StringPrepParseException("", StringPrepErrorType.UnassignedError)
                 ),

            /*  // Invalid UTF-8
                 {
                   "Larger test (shrinking)",
                   "X\u00C2\u00AD\u00C3\u00DF\u00C4\u00B0\u00E2\u0084\u00A1\u006a\u00cc\u008c\u00c2\u00a0\u00c2"
                   "\u00aa\u00ce\u00b0\u00e2\u0080\u0080", "xssi\u00cc\u0087""tel\u00c7\u00b0 a\u00ce\u00b0 ",
                   "Nameprep",
                   InternationalizedDomainNames.DEFAULT, U_ZERO_ERROR
                 },
                {

                   "Larger test (expanding)",
                   "X\u00C3\u00DF\u00e3\u008c\u0096\u00C4\u00B0\u00E2\u0084\u00A1\u00E2\u0092\u009F\u00E3\u008c\u0080",
                   "xss\u00e3\u0082\u00ad\u00e3\u0083\u00ad\u00e3\u0083\u00a1\u00e3\u0083\u00bc\u00e3\u0083\u0088"
                   "\u00e3\u0083\u00ab""i\u00cc\u0087""tel\u0028""d\u0029\u00e3\u0082\u00a2\u00e3\u0083\u0091"
                   "\u00e3\u0083\u00bc\u00e3\u0083\u0088"
                   "Nameprep",
                   InternationalizedDomainNames.DEFAULT, U_ZERO_ERROR
                 },
              */
            };
    }
}
