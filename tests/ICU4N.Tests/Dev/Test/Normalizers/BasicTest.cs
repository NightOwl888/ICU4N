using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Support.Text;
using ICU4N.Text;
using J2N;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using Random = System.Random;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Normalizers
{
    public class BasicTest : TestFmwk
    {
        string[][] canonTests = {
            // Input                Decomposed              Composed
            new string[] { "cat",                "cat",                  "cat"               },
            new string[] { "\u00e0ardvark",      "a\u0300ardvark",       "\u00e0ardvark",    },

            new string[] { "\u1e0a",             "D\u0307",              "\u1e0a"            }, // D-dot_above
            new string[] { "D\u0307",            "D\u0307",              "\u1e0a"            }, // D dot_above

            new string[] { "\u1e0c\u0307",       "D\u0323\u0307",        "\u1e0c\u0307"      }, // D-dot_below dot_above
            new string[] { "\u1e0a\u0323",       "D\u0323\u0307",        "\u1e0c\u0307"      }, // D-dot_above dot_below
            new string[] { "D\u0307\u0323",      "D\u0323\u0307",        "\u1e0c\u0307"      }, // D dot_below dot_above

            new string[] { "\u1e10\u0307\u0323", "D\u0327\u0323\u0307",  "\u1e10\u0323\u0307"}, // D dot_below cedilla dot_above
            new string[] { "D\u0307\u0328\u0323","D\u0328\u0323\u0307",  "\u1e0c\u0328\u0307"}, // D dot_above ogonek dot_below

            new string[] { "\u1E14",             "E\u0304\u0300",        "\u1E14"            }, // E-macron-grave
            new string[] { "\u0112\u0300",       "E\u0304\u0300",        "\u1E14"            }, // E-macron + grave
            new string[] { "\u00c8\u0304",       "E\u0300\u0304",        "\u00c8\u0304"      }, // E-grave + macron

            new string[] { "\u212b",             "A\u030a",              "\u00c5"            }, // angstrom_sign
            new string[] { "\u00c5",             "A\u030a",              "\u00c5"            }, // A-ring

            new string[] { "\u00c4ffin",         "A\u0308ffin",          "\u00c4ffin"        },
            new string[] { "\u00c4\uFB03n",      "A\u0308\uFB03n",       "\u00c4\uFB03n"     },

            new string[] { "\u00fdffin",         "y\u0301ffin",          "\u00fdffin"        }, //updated with 3.0
            new string[] { "\u00fd\uFB03n",      "y\u0301\uFB03n",       "\u00fd\uFB03n"     }, //updated with 3.0

            new string[] { "Henry IV",           "Henry IV",             "Henry IV"          },
            new string[] { "Henry \u2163",       "Henry \u2163",         "Henry \u2163"      },

            new string[] { "\u30AC",             "\u30AB\u3099",         "\u30AC"            }, // ga (Katakana)
            new string[] { "\u30AB\u3099",       "\u30AB\u3099",         "\u30AC"            }, // ka + ten
            new string[] { "\uFF76\uFF9E",       "\uFF76\uFF9E",         "\uFF76\uFF9E"      }, // hw_ka + hw_ten
            new string[] { "\u30AB\uFF9E",       "\u30AB\uFF9E",         "\u30AB\uFF9E"      }, // ka + hw_ten
            new string[] { "\uFF76\u3099",       "\uFF76\u3099",         "\uFF76\u3099"      }, // hw_ka + ten

            new string[] { "A\u0300\u0316", "A\u0316\u0300", "\u00C0\u0316" },
            new string[] {"\\U0001d15e\\U0001d157\\U0001d165\\U0001d15e","\\U0001D157\\U0001D165\\U0001D157\\U0001D165\\U0001D157\\U0001D165", "\\U0001D157\\U0001D165\\U0001D157\\U0001D165\\U0001D157\\U0001D165"},
        };

        string[][] compatTests = {
                // Input                Decomposed              Composed
            new string[] { "cat",                 "cat",                     "cat"           },
            new string[] { "\uFB4f",             "\u05D0\u05DC",         "\u05D0\u05DC",     }, // Alef-Lamed vs. Alef, Lamed

            new string[] { "\u00C4ffin",         "A\u0308ffin",          "\u00C4ffin"        },
            new string[] { "\u00C4\uFB03n",      "A\u0308ffin",          "\u00C4ffin"        }, // ffi ligature -> f + f + i

            new string[] { "\u00fdffin",         "y\u0301ffin",          "\u00fdffin"        },        //updated for 3.0
            new string[] { "\u00fd\uFB03n",      "y\u0301ffin",          "\u00fdffin"        }, // ffi ligature -> f + f + i

            new string[] { "Henry IV",           "Henry IV",             "Henry IV"          },
            new string[] { "Henry \u2163",       "Henry IV",             "Henry IV"          },

            new string[] { "\u30AC",             "\u30AB\u3099",         "\u30AC"            }, // ga (Katakana)
            new string[] { "\u30AB\u3099",       "\u30AB\u3099",         "\u30AC"            }, // ka + ten

            new string[] { "\uFF76\u3099",       "\u30AB\u3099",         "\u30AC"            }, // hw_ka + ten

            /* These two are broken in Unicode 2.1.2 but fixed in 2.1.5 and later*/
            new string[] { "\uFF76\uFF9E",       "\u30AB\u3099",         "\u30AC"            }, // hw_ka + hw_ten
            new string[] { "\u30AB\uFF9E",       "\u30AB\u3099",         "\u30AC"            }, // ka + hw_ten

        };

        // With Canonical decomposition, Hangul syllables should get decomposed
        // into Jamo, but Jamo characters should not be decomposed into
        // conjoining Jamo
        string[][] hangulCanon = {
            // Input                Decomposed              Composed
            new string[] { "\ud4db",             "\u1111\u1171\u11b6",   "\ud4db"        },
            new string[] { "\u1111\u1171\u11b6", "\u1111\u1171\u11b6",   "\ud4db"        },
        };

        // With compatibility decomposition turned on,
        // it should go all the way down to conjoining Jamo characters.
        // THIS IS NO LONGER TRUE IN UNICODE v2.1.8, SO THIS TEST IS OBSOLETE
        string[][] hangulCompat = {
            // Input        Decomposed                          Composed
            // { "\ud4db",     "\u1111\u116e\u1175\u11af\u11c2",   "\ud478\u1175\u11af\u11c2"  },
        };

        [Test]
        public void TestHangulCompose()
        {
            // Make sure that the static composition methods work
            Logln("Canonical composition...");
            staticTest(NormalizerMode.NFC, hangulCanon, 2);
            Logln("Compatibility composition...");
            staticTest(NormalizerMode.NFKC, hangulCompat, 2);
            // Now try iterative composition....
            Logln("Iterative composition...");
            Normalizer norm = new Normalizer("", NormalizerMode.NFC, 0);
            iterateTest(norm, hangulCanon, 2);

            norm.SetMode(NormalizerMode.NFKD);
            iterateTest(norm, hangulCompat, 2);

            // And finally, make sure you can do it in reverse too
            Logln("Reverse iteration...");
            norm.SetMode(NormalizerMode.NFC);
            backAndForth(norm, hangulCanon);
        }

        [Test]
        public void TestHangulDecomp()
        {
            // Make sure that the static decomposition methods work
            Logln("Canonical decomposition...");
            staticTest(NormalizerMode.NFD, hangulCanon, 1);
            Logln("Compatibility decomposition...");
            staticTest(NormalizerMode.NFKD, hangulCompat, 1);

            // Now the iterative decomposition methods...
            Logln("Iterative decomposition...");
            Normalizer norm = new Normalizer("", NormalizerMode.NFD, 0);
            iterateTest(norm, hangulCanon, 1);

            norm.SetMode(NormalizerMode.NFKD);
            iterateTest(norm, hangulCompat, 1);

            // And finally, make sure you can do it in reverse too
            Logln("Reverse iteration...");
            norm.SetMode(NormalizerMode.NFD);
            backAndForth(norm, hangulCanon);
        }
        [Test]
        public void TestNone()
        {
            Normalizer norm = new Normalizer("", NormalizerMode.None, 0);
            iterateTest(norm, canonTests, 0);
            staticTest(NormalizerMode.None, canonTests, 0);
        }
        [Test]
        public void TestDecomp()
        {
            Normalizer norm = new Normalizer("", NormalizerMode.NFD, 0);
            iterateTest(norm, canonTests, 1);
            staticTest(NormalizerMode.NFD, canonTests, 1);
            decomposeTest(NormalizerMode.NFD, canonTests, 1);
        }

        [Test]
        public void TestCompatDecomp()
        {
            Normalizer norm = new Normalizer("", NormalizerMode.NFKD, 0);
            iterateTest(norm, compatTests, 1);
            staticTest(NormalizerMode.NFKD, compatTests, 1);
            decomposeTest(NormalizerMode.NFKD, compatTests, 1);
        }

        [Test]
        public void TestCanonCompose()
        {
            Normalizer norm = new Normalizer("", NormalizerMode.NFC, 0);
            staticTest(NormalizerMode.NFC, canonTests, 2);
            iterateTest(norm, canonTests, 2);
            composeTest(NormalizerMode.NFC, canonTests, 2);
        }

        [Test]
        public void TestCompatCompose()
        {
            Normalizer norm = new Normalizer("", NormalizerMode.NFKC, 0);
            iterateTest(norm, compatTests, 2);
            staticTest(NormalizerMode.NFKC, compatTests, 2);
            composeTest(NormalizerMode.NFKC, compatTests, 2);
        }

        [Test]
        public void TestExplodingBase()
        {
            // \u017f - Latin small letter long s
            // \u0307 - combining dot above
            // \u1e61 - Latin small letter s with dot above
            // \u1e9b - Latin small letter long s with dot above
            string[][] canon = {
            // Input                Decomposed              Composed
            new string[] { "Tschu\u017f",        "Tschu\u017f",          "Tschu\u017f"    },
            new string[] { "Tschu\u1e9b",        "Tschu\u017f\u0307",    "Tschu\u1e9b"    },
        };
            string[][] compat = {
            // Input                Decomposed              Composed
            new string[] { "\u017f",        "s",              "s"           },
            new string[] { "\u1e9b",        "s\u0307",        "\u1e61"      },
        };

            staticTest(NormalizerMode.NFD, canon, 1);
            staticTest(NormalizerMode.NFC, canon, 2);

            staticTest(NormalizerMode.NFKD, compat, 1);
            staticTest(NormalizerMode.NFKC, compat, 2);

        }

        /**
         * The Tibetan vowel sign AA, 0f71, was messed up prior to
         * Unicode version 2.1.9.
         * Once 2.1.9 or 3.0 is released, uncomment this test.
         */
        [Test]
        public void TestTibetan()
        {
            string[][] decomp = {
                new string[] { "\u0f77", "\u0f77", "\u0fb2\u0f71\u0f80" }
            };
            string[][] compose = {
                new string[] { "\u0fb2\u0f71\u0f80", "\u0fb2\u0f71\u0f80", "\u0fb2\u0f71\u0f80" }
            };

            staticTest(NormalizerMode.NFD, decomp, 1);
            staticTest(NormalizerMode.NFKD, decomp, 2);
            staticTest(NormalizerMode.NFC, compose, 1);
            staticTest(NormalizerMode.NFKC, compose, 2);
        }

        /**
         * Make sure characters in the CompositionExclusion.txt list do not get
         * composed to.
         */
        [Test]
        public void TestCompositionExclusion()
        {
            // This list is generated from CompositionExclusion.txt.
            // Update whenever the normalizer tables are updated.  Note
            // that we test all characters listed, even those that can be
            // derived from the Unicode DB and are therefore commented
            // out.
            string EXCLUDED =
                    "\u0340\u0341\u0343\u0344\u0374\u037E\u0387\u0958" +
                    "\u0959\u095A\u095B\u095C\u095D\u095E\u095F\u09DC" +
                    "\u09DD\u09DF\u0A33\u0A36\u0A59\u0A5A\u0A5B\u0A5E" +
                    "\u0B5C\u0B5D\u0F43\u0F4D\u0F52\u0F57\u0F5C\u0F69" +
                    "\u0F73\u0F75\u0F76\u0F78\u0F81\u0F93\u0F9D\u0FA2" +
                    "\u0FA7\u0FAC\u0FB9\u1F71\u1F73\u1F75\u1F77\u1F79" +
                    "\u1F7B\u1F7D\u1FBB\u1FBE\u1FC9\u1FCB\u1FD3\u1FDB" +
                    "\u1FE3\u1FEB\u1FEE\u1FEF\u1FF9\u1FFB\u1FFD\u2000" +
                    "\u2001\u2126\u212A\u212B\u2329\u232A\uF900\uFA10" +
                    "\uFA12\uFA15\uFA20\uFA22\uFA25\uFA26\uFA2A\uFB1F" +
                    "\uFB2A\uFB2B\uFB2C\uFB2D\uFB2E\uFB2F\uFB30\uFB31" +
                    "\uFB32\uFB33\uFB34\uFB35\uFB36\uFB38\uFB39\uFB3A" +
                    "\uFB3B\uFB3C\uFB3E\uFB40\uFB41\uFB43\uFB44\uFB46" +
                    "\uFB47\uFB48\uFB49\uFB4A\uFB4B\uFB4C\uFB4D\uFB4E";
            for (int i = 0; i < EXCLUDED.Length; ++i)
            {
                string a = Convert.ToString(EXCLUDED[i]);
                string b = Normalizer.Normalize(a, NormalizerMode.NFKD);
                string c = Normalizer.Normalize(b, NormalizerMode.NFC);
                if (c.Equals(a))
                {
                    Errln("FAIL: " + Hex(a) + " x DECOMP_COMPAT => " +
                          Hex(b) + " x COMPOSE => " +
                          Hex(c));
                }
                else if (IsVerbose())
                {
                    Logln("Ok: " + Hex(a) + " x DECOMP_COMPAT => " +
                          Hex(b) + " x COMPOSE => " +
                          Hex(c));
                }
            }
            // The following method works too, but it is somewhat
            // incestuous.  It uses UInfo, which is the same database that
            // NormalizerBuilder uses, so if something is wrong with
            // UInfo, the following test won't show it.  All it will show
            // is that NormalizerBuilder has been run with whatever the
            // current UInfo is.
            //
            // We comment this out in favor of the test above, which
            // provides independent verification (but also requires
            // independent updating).
            //      Logln("---");
            //      UInfo uinfo = new UInfo();
            //      for (int i=0; i<=0xFFFF; ++i) {
            //          if (!uinfo.isExcludedComposition((char)i) ||
            //              (!uinfo.hasCanonicalDecomposition((char)i) &&
            //               !uinfo.hasCompatibilityDecomposition((char)i))) continue;
            //          string a = Convert.ToString((char)i);
            //          string b = Normalizer.Normalize(a,Normalizer.DECOMP_COMPAT,0);
            //          string c = Normalizer.Normalize(b,Normalizer.COMPOSE,0);
            //          if (c.Equals(a)) {
            //              Errln("FAIL: " + Hex(a) + " x DECOMP_COMPAT => " +
            //                    Hex(b) + " x COMPOSE => " +
            //                    Hex(c));
            //          } else if (isVerbose()) {
            //              Logln("Ok: " + Hex(a) + " x DECOMP_COMPAT => " +
            //                    Hex(b) + " x COMPOSE => " +
            //                    Hex(c));
            //          }
            //      }
        }

        /**
         * Test for a problem that showed up just before ICU 1.6 release
         * having to do with combining characters with an index of zero.
         * Such characters do not participate in any canonical
         * decompositions.  However, having an index of zero means that
         * they all share one typeMask[] entry, that is, they all have to
         * map to the same canonical class, which is not the case, in
         * reality.
         */
        [Test]
        public void TestZeroIndex()
        {
            string[] DATA = {
                // Expect col1 x COMPOSE_COMPAT => col2
                // Expect col2 x DECOMP => col3
                "A\u0316\u0300", "\u00C0\u0316", "A\u0316\u0300",
                "A\u0300\u0316", "\u00C0\u0316", "A\u0316\u0300",
                "A\u0327\u0300", "\u00C0\u0327", "A\u0327\u0300",
                "c\u0321\u0327", "c\u0321\u0327", "c\u0321\u0327",
                "c\u0327\u0321", "\u00E7\u0321", "c\u0327\u0321",
            };

            for (int i = 0; i < DATA.Length; i += 3)
            {
                string a = DATA[i];
                string b = Normalizer.Normalize(a, NormalizerMode.NFKC);
                string exp = DATA[i + 1];
                if (b.Equals(exp))
                {
                    Logln("Ok: " + Hex(a) + " x COMPOSE_COMPAT => " + Hex(b));
                }
                else
                {
                    Errln("FAIL: " + Hex(a) + " x COMPOSE_COMPAT => " + Hex(b) +
                          ", expect " + Hex(exp));
                }
                a = Normalizer.Normalize(b, NormalizerMode.NFD);
                exp = DATA[i + 2];
                if (a.Equals(exp))
                {
                    Logln("Ok: " + Hex(b) + " x DECOMP => " + Hex(a));
                }
                else
                {
                    Errln("FAIL: " + Hex(b) + " x DECOMP => " + Hex(a) +
                          ", expect " + Hex(exp));
                }
            }
        }

        /**
         * Test for a problem found by Verisign.  Problem is that
         * characters at the start of a string are not put in canonical
         * order correctly by compose() if there is no starter.
         */
        [Test]
        public void TestVerisign()
        {
            string[]
            inputs = {
                "\u05b8\u05b9\u05b1\u0591\u05c3\u05b0\u05ac\u059f",
                "\u0592\u05b7\u05bc\u05a5\u05b0\u05c0\u05c4\u05ad"
            };
            string[]
            outputs = {
                "\u05b1\u05b8\u05b9\u0591\u05c3\u05b0\u05ac\u059f",
                "\u05b0\u05b7\u05bc\u05a5\u0592\u05c0\u05ad\u05c4"
            };

            for (int i = 0; i < inputs.Length; ++i)
            {
                string input = inputs[i];
                string output = outputs[i];
                string result = Normalizer.Decompose(input, false);
                if (!result.Equals(output))
                {
                    Errln("FAIL input: " + Hex(input));
                    Errln(" decompose: " + Hex(result));
                    Errln("  expected: " + Hex(output));
                }
                result = Normalizer.Compose(input, false);
                if (!result.Equals(output))
                {
                    Errln("FAIL input: " + Hex(input));
                    Errln("   compose: " + Hex(result));
                    Errln("  expected: " + Hex(output));
                }
            }

        }
        [Test]
        public void TestQuickCheckResultNO()
        {
            char[] CPNFD =  {
                                (char)0x00C5, (char)0x0407, (char)0x1E00, (char)0x1F57, (char)0x220C,
                                (char)0x30AE, (char)0xAC00, (char)0xD7A3, (char)0xFB36, (char)0xFB4E};
            char[] CPNFC =  {
                                (char)0x0340, (char)0x0F93, (char)0x1F77, (char)0x1FBB, (char)0x1FEB,
                                (char)0x2000, (char)0x232A, (char)0xF900, (char)0xFA1E, (char)0xFB4E};
            char[] CPNFKD =  {
                                (char)0x00A0, (char)0x02E4, (char)0x1FDB, (char)0x24EA, (char)0x32FE,
                                (char)0xAC00, (char)0xFB4E, (char)0xFA10, (char)0xFF3F, (char)0xFA2D};
            char[] CPNFKC =  {
                                (char)0x00A0, (char)0x017F, (char)0x2000, (char)0x24EA, (char)0x32FE,
                                (char)0x33FE, (char)0xFB4E, (char)0xFA10, (char)0xFF3F, (char)0xFA2D};


            int SIZE = 10;

            int count = 0;
            for (; count < SIZE; count++)
            {
                if (Normalizer.QuickCheck(Convert.ToString(CPNFD[count]),
                        NormalizerMode.NFD, 0) != QuickCheckResult.No)
                {
                    Errln("ERROR in NFD quick check at U+" +
                           (CPNFD[count]).ToHexString());
                    return;
                }
                if (Normalizer.QuickCheck(Convert.ToString(CPNFC[count]),
                            NormalizerMode.NFC, 0) != QuickCheckResult.No)
                {
                    Errln("ERROR in NFC quick check at U+" +
                           (CPNFC[count]).ToHexString());
                    return;
                }
                if (Normalizer.QuickCheck(Convert.ToString(CPNFKD[count]),
                                    NormalizerMode.NFKD, 0) != QuickCheckResult.No)
                {
                    Errln("ERROR in NFKD quick check at U+" +
                           (CPNFKD[count]).ToHexString());
                    return;
                }
                if (Normalizer.QuickCheck(Convert.ToString(CPNFKC[count]),
                                             NormalizerMode.NFKC, 0) != QuickCheckResult.No)
                {
                    Errln("ERROR in NFKC quick check at U+" +
                           (CPNFKC[count]).ToHexString());
                    return;
                }
                // for improving coverage
                if (Normalizer.QuickCheck(Convert.ToString(CPNFKC[count]),
                                             NormalizerMode.NFKC) != QuickCheckResult.No)
                {
                    Errln("ERROR in NFKC quick check at U+" +
                           (CPNFKC[count]).ToHexString());
                    return;
                }
            }
        }


        [Test]
        public void TestQuickCheckResultYES()
        {
            char[] CPNFD = {
                                (char)0x00C6, (char)0x017F, (char)0x0F74, (char)0x1000, (char)0x1E9A,
                                (char)0x2261, (char)0x3075, (char)0x4000, (char)0x5000, (char)0xF000};
            char[] CPNFC = {
                                (char)0x0400, (char)0x0540, (char)0x0901, (char)0x1000, (char)0x1500,
                                (char)0x1E9A, (char)0x3000, (char)0x4000, (char)0x5000, (char)0xF000};
            char[] CPNFKD = {
                                (char)0x00AB, (char)0x02A0, (char)0x1000, (char)0x1027, (char)0x2FFB,
                                (char)0x3FFF, (char)0x4FFF, (char)0xA000, (char)0xF000, (char)0xFA27};
            char[] CPNFKC = {
                                (char)0x00B0, (char)0x0100, (char)0x0200, (char)0x0A02, (char)0x1000,
                                (char)0x2010, (char)0x3030, (char)0x4000, (char)0xA000, (char)0xFA0E};

            int SIZE = 10;
            int count = 0;

            char cp = (char)0;
            while (cp < 0xA0)
            {
                if (Normalizer.QuickCheck(Convert.ToString(cp), NormalizerMode.NFD, 0)
                                                != QuickCheckResult.Yes)
                {
                    Errln("ERROR in NFD quick check at U+" +
                                                          (cp).ToHexString());
                    return;
                }
                if (Normalizer.QuickCheck(Convert.ToString(cp), NormalizerMode.NFC, 0)
                                                 != QuickCheckResult.Yes)
                {
                    Errln("ERROR in NFC quick check at U+" +
                                                          (cp).ToHexString());
                    return;
                }
                if (Normalizer.QuickCheck(Convert.ToString(cp), NormalizerMode.NFKD, 0)
                                                 != QuickCheckResult.Yes)
                {
                    Errln("ERROR in NFKD quick check at U+" +
                                                          (cp).ToHexString());
                    return;
                }
                if (Normalizer.QuickCheck(Convert.ToString(cp), NormalizerMode.NFKC, 0)
                                                 != QuickCheckResult.Yes)
                {
                    Errln("ERROR in NFKC quick check at U+" +
                                                           (cp).ToHexString());
                    return;
                }
                // improve the coverage
                if (Normalizer.QuickCheck(Convert.ToString(cp), NormalizerMode.NFKC)
                                                 != QuickCheckResult.Yes)
                {
                    Errln("ERROR in NFKC quick check at U+" +
                                                           (cp).ToHexString());
                    return;
                }
                cp++;
            }

            for (; count < SIZE; count++)
            {
                if (Normalizer.QuickCheck(Convert.ToString(CPNFD[count]),
                                             NormalizerMode.NFD, 0) != QuickCheckResult.Yes)
                {
                    Errln("ERROR in NFD quick check at U+" +
                                                 (CPNFD[count]).ToHexString());
                    return;
                }
                if (Normalizer.QuickCheck(Convert.ToString(CPNFC[count]),
                                             NormalizerMode.NFC, 0) != QuickCheckResult.Yes)
                {
                    Errln("ERROR in NFC quick check at U+" +
                                                 (CPNFC[count]).ToHexString());
                    return;
                }
                if (Normalizer.QuickCheck(Convert.ToString(CPNFKD[count]),
                                             NormalizerMode.NFKD, 0) != QuickCheckResult.Yes)
                {
                    Errln("ERROR in NFKD quick check at U+" +
                                        (CPNFKD[count]).ToHexString());
                    return;
                }
                if (Normalizer.QuickCheck(Convert.ToString(CPNFKC[count]),
                                             NormalizerMode.NFKC, 0) != QuickCheckResult.Yes)
                {
                    Errln("ERROR in NFKC quick check at U+" +
                            (CPNFKC[count]).ToHexString());
                    return;
                }
                // improve the coverage
                if (Normalizer.QuickCheck(Convert.ToString(CPNFKC[count]),
                                             NormalizerMode.NFKC) != QuickCheckResult.Yes)
                {
                    Errln("ERROR in NFKC quick check at U+" +
                            (CPNFKC[count]).ToHexString());
                    return;
                }
            }
        }
        [Test]
        public void TestBengali()
        {
            string input = "\u09bc\u09be\u09cd\u09be";
            string output = Normalizer.Normalize(input, NormalizerMode.NFC);
            if (!input.Equals(output))
            {
                Errln("ERROR in NFC of string");
            }
        }
        [Test]
        public void TestQuickCheckResultMAYBE()
        {

            char[]
            CPNFC = {
                (char)0x0306, (char)0x0654, (char)0x0BBE, (char)0x102E, (char)0x1161,
                                (char)0x116A, (char)0x1173, (char)0x1175, (char)0x3099, (char)0x309A};
            char[]
            CPNFKC = {
                (char)0x0300, (char)0x0654, (char)0x0655, (char)0x09D7, (char)0x0B3E,
                                (char)0x0DCF, (char)0xDDF, (char)0x102E, (char)0x11A8, (char)0x3099};


            int SIZE = 10;

            int count = 0;

            /* NFD and NFKD does not have any MAYBE codepoints */
            for (; count < SIZE; count++)
            {
                if (Normalizer.QuickCheck(Convert.ToString(CPNFC[count]),
                                            NormalizerMode.NFC, 0) != QuickCheckResult.Maybe)
                {
                    Errln("ERROR in NFC quick check at U+" +
                                                (CPNFC[count]).ToHexString());
                    return;
                }
                if (Normalizer.QuickCheck(Convert.ToString(CPNFKC[count]),
                                           NormalizerMode.NFKC, 0) != QuickCheckResult.Maybe)
                {
                    Errln("ERROR in NFKC quick check at U+" +
                                                (CPNFKC[count]).ToHexString());
                    return;
                }
                if (Normalizer.QuickCheck(new char[] { CPNFC[count] },
                                            NormalizerMode.NFC, 0) != QuickCheckResult.Maybe)
                {
                    Errln("ERROR in NFC quick check at U+" +
                                                (CPNFC[count]).ToHexString());
                    return;
                }
                if (Normalizer.QuickCheck(new char[] { CPNFKC[count] },
                                           NormalizerMode.NFKC, 0) != QuickCheckResult.Maybe)
                {
                    Errln("ERROR in NFKC quick check at U+" +
                                                (CPNFKC[count]).ToHexString());
                    return;
                }
                if (Normalizer.QuickCheck(new char[] { CPNFKC[count] },
                                           NormalizerMode.None, 0) != QuickCheckResult.Yes)
                {
                    Errln("ERROR in NONE quick check at U+" +
                                                (CPNFKC[count]).ToHexString());
                    return;
                }
            }
        }

        [Test]
        public void TestQuickCheckStringResult()
        {
            int count;
            string d;
            string c;

            for (count = 0; count < canonTests.Length; count++)
            {
                d = canonTests[count][1];
                c = canonTests[count][2];
                if (Normalizer.QuickCheck(d, NormalizerMode.NFD, 0)
                                                != QuickCheckResult.Yes)
                {
                    Errln("ERROR in NFD quick check for string at count " + count);
                    return;
                }

                if (Normalizer.QuickCheck(c, NormalizerMode.NFC, 0)
                                                == QuickCheckResult.No)
                {
                    Errln("ERROR in NFC quick check for string at count " + count);
                    return;
                }
            }

            for (count = 0; count < compatTests.Length; count++)
            {
                d = compatTests[count][1];
                c = compatTests[count][2];
                if (Normalizer.QuickCheck(d, NormalizerMode.NFKD, 0)
                                                != QuickCheckResult.Yes)
                {
                    Errln("ERROR in NFKD quick check for string at count " + count);
                    return;
                }

                if (Normalizer.QuickCheck(c, NormalizerMode.NFKC, 0)
                                                != QuickCheckResult.Yes)
                {
                    Errln("ERROR in NFKC quick check for string at count " + count);
                    return;
                }
            }
        }

        static int qcToInt(QuickCheckResult qc)
        {
            if (qc == QuickCheckResult.No)
            {
                return 0;
            }
            else if (qc == QuickCheckResult.Yes)
            {
                return 1;
            }
            else /* NormalizerQuickCheckResult.Maybe */
            {
                return 2;
            }
        }

        [Test]
        public void TestQuickCheckPerCP()
        {
            int c, lead, trail;
            string s, nfd;
            int lccc1, lccc2, tccc1, tccc2;
            int qc1, qc2;

            if (
                UChar.GetIntPropertyMaxValue(UProperty.NFD_Quick_Check) != 1 || // YES
                UChar.GetIntPropertyMaxValue(UProperty.NFKD_Quick_Check) != 1 ||
                UChar.GetIntPropertyMaxValue(UProperty.NFC_Quick_Check) != 2 || // MAYBE
                UChar.GetIntPropertyMaxValue(UProperty.NFKC_Quick_Check) != 2 ||
                UChar.GetIntPropertyMaxValue(UProperty.Lead_Canonical_Combining_Class) != UChar.GetIntPropertyMaxValue(UProperty.Canonical_Combining_Class) ||
                UChar.GetIntPropertyMaxValue(UProperty.Trail_Canonical_Combining_Class) != UChar.GetIntPropertyMaxValue(UProperty.Canonical_Combining_Class)
            )
            {
                Errln("wrong result from one of the u_getIntPropertyMaxValue(UCHAR_NF*_QUICK_CHECK) or UCHAR_*_CANONICAL_COMBINING_CLASS");
            }

            /*
             * compare the quick check property values for some code points
             * to the quick check results for checking same-code point strings
             */
            c = 0;
            while (c < 0x110000)
            {
                s = UTF16.ValueOf(c);

                qc1 = UChar.GetIntPropertyValue(c, UProperty.NFC_Quick_Check);
                qc2 = qcToInt(Normalizer.QuickCheck(s, NormalizerMode.NFC));
                if (qc1 != qc2)
                {
                    Errln("getIntPropertyValue(NFC)=" + qc1 + " != " + qc2 + "=quickCheck(NFC) for U+" + (c).ToHexString());
                }

                qc1 = UChar.GetIntPropertyValue(c, UProperty.NFD_Quick_Check);
                qc2 = qcToInt(Normalizer.QuickCheck(s, NormalizerMode.NFD));
                if (qc1 != qc2)
                {
                    Errln("getIntPropertyValue(NFD)=" + qc1 + " != " + qc2 + "=quickCheck(NFD) for U+" + (c).ToHexString());
                }

                qc1 = UChar.GetIntPropertyValue(c, UProperty.NFKC_Quick_Check);
                qc2 = qcToInt(Normalizer.QuickCheck(s, NormalizerMode.NFKC));
                if (qc1 != qc2)
                {
                    Errln("getIntPropertyValue(NFKC)=" + qc1 + " != " + qc2 + "=quickCheck(NFKC) for U+" + (c).ToHexString());
                }

                qc1 = UChar.GetIntPropertyValue(c, UProperty.NFKD_Quick_Check);
                qc2 = qcToInt(Normalizer.QuickCheck(s, NormalizerMode.NFKD));
                if (qc1 != qc2)
                {
                    Errln("getIntPropertyValue(NFKD)=" + qc1 + " != " + qc2 + "=quickCheck(NFKD) for U+" + (c).ToHexString());
                }

                nfd = Normalizer.Normalize(s, NormalizerMode.NFD);
                lead = UTF16.CharAt(nfd, 0);
                trail = UTF16.CharAt(nfd, nfd.Length - 1);

                lccc1 = UChar.GetIntPropertyValue(c, UProperty.Lead_Canonical_Combining_Class);
                lccc2 = UChar.GetCombiningClass(lead);
                tccc1 = UChar.GetIntPropertyValue(c, UProperty.Trail_Canonical_Combining_Class);
                tccc2 = UChar.GetCombiningClass(trail);

                if (lccc1 != lccc2)
                {
                    Errln("getIntPropertyValue(lccc)=" + lccc1 + " != " + lccc2 + "=getCombiningClass(lead) for U+" + (c).ToHexString());
                }
                if (tccc1 != tccc2)
                {
                    Errln("getIntPropertyValue(tccc)=" + tccc1 + " != " + tccc2 + "=getCombiningClass(trail) for U+" + (c).ToHexString());
                }

                /* skip some code points */
                c = (20 * c) / 19 + 1;
            }
        }

        //------------------------------------------------------------------------
        // Internal utilities
        //
        //------------------------------------------------------------------------
        // Internal utilities
        //

        /*    private void backAndForth(Normalizer iter, string input)
            {
                iter.SetText(input);

                // Run through the iterator forwards and stick it into a StringBuffer
                StringBuffer forward =  new StringBuffer();
                for (int ch = iter.First(); ch != Normalizer.Done; ch = iter.Next()) {
                    forward.Append(ch);
                }

                // Now do it backwards
                StringBuffer reverse = new StringBuffer();
                for (int ch = iter.Last(); ch != Normalizer.Done; ch = iter.Previous()) {
                    reverse.insert(0, ch);
                }

                if (!forward.toString().Equals(reverse.toString())) {
                    Errln("FAIL: Forward/reverse mismatch for input " + Hex(input)
                          + ", forward: " + Hex(forward) + ", backward: "+Hex(reverse));
                } else if (isVerbose()) {
                    Logln("Ok: Forward/reverse for input " + Hex(input)
                          + ", forward: " + Hex(forward) + ", backward: "+Hex(reverse));
                }
            }*/

        private void backAndForth(Normalizer iter, string[][] tests)
        {
            for (int i = 0; i < tests.Length; i++)
            {
                iter.SetText(tests[i][0]);

                // Run through the iterator forwards and stick it into a
                // StringBuffer
                StringBuffer forward = new StringBuffer();
                for (int ch = iter.First(); ch != Normalizer.Done; ch = iter.Next())
                {
                    forward.Append(ch);
                }

                // Now do it backwards
                StringBuffer reverse = new StringBuffer();
                for (int ch = iter.Last(); ch != Normalizer.Done; ch = iter.Previous())
                {
                    reverse.Insert(0, ch);
                }

                if (!forward.ToString().Equals(reverse.ToString()))
                {
                    Errln("FAIL: Forward/reverse mismatch for input "
                        + Hex(tests[i][0]) + ", forward: " + Hex(forward)
                        + ", backward: " + Hex(reverse));
                }
                else if (IsVerbose())
                {
                    Logln("Ok: Forward/reverse for input " + Hex(tests[i][0])
                          + ", forward: " + Hex(forward) + ", backward: "
                          + Hex(reverse));
                }
            }
        }

        private void staticTest(NormalizerMode mode,
                                 string[][] tests, int outCol)
        {
            for (int i = 0; i < tests.Length; i++)
            {
                string input = Utility.Unescape(tests[i][0]);
                string expect = Utility.Unescape(tests[i][outCol]);

                Logln("Normalizing '" + input + "' (" + Hex(input) + ")");

                string output2 = Normalizer.Normalize(input, mode);

                if (!output2.Equals(expect))
                {
                    Errln("FAIL: case " + i
                        + " expected '" + expect + "' (" + Hex(expect) + ")"
                        + " but got '" + output2 + "' (" + Hex(output2) + ")");
                }
            }
            char[] output = new char[1];
            for (int i = 0; i < tests.Length; i++)
            {
                char[] input = Utility.Unescape(tests[i][0]).ToCharArray();
                string expect = Utility.Unescape(tests[i][outCol]);

                Logln("Normalizing '" + new string(input) + "' (" +
                            Hex(new string(input)) + ")");
                int reqLength = 0;
                while (true)
                {
                    try
                    {
                        reqLength = Normalizer.Normalize(input, output, mode, 0);
                        if (reqLength <= output.Length)
                        {
                            break;
                        }
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        output = new char[int.Parse(e.Message, CultureInfo.InvariantCulture)];
                        continue;
                    }
                }
                if (!expect.Equals(new string(output, 0, reqLength)))
                {
                    Errln("FAIL: case " + i
                        + " expected '" + expect + "' (" + Hex(expect) + ")"
                        + " but got '" + new string(output)
                        + "' (" + Hex(new string(output)) + ")");
                }
            }
        }
        private void decomposeTest(NormalizerMode mode,
                                 string[][] tests, int outCol)
        {
            for (int i = 0; i < tests.Length; i++)
            {
                string input = Utility.Unescape(tests[i][0]);
                string expect = Utility.Unescape(tests[i][outCol]);

                Logln("Normalizing '" + input + "' (" + Hex(input) + ")");

                string output2 = Normalizer.Decompose(input, mode == NormalizerMode.NFKD);

                if (!output2.Equals(expect))
                {
                    Errln("FAIL: case " + i
                        + " expected '" + expect + "' (" + Hex(expect) + ")"
                        + " but got '" + output2 + "' (" + Hex(output2) + ")");
                }
            }
            char[] output = new char[1];
            for (int i = 0; i < tests.Length; i++)
            {
                char[] input = Utility.Unescape(tests[i][0]).ToCharArray();
                string expect = Utility.Unescape(tests[i][outCol]);

                Logln("Normalizing '" + new string(input) + "' (" +
                            Hex(new string(input)) + ")");
                int reqLength = 0;
                while (true)
                {
                    try
                    {
                        reqLength = Normalizer.Decompose(input, output, mode == NormalizerMode.NFKD, 0);
                        if (reqLength <= output.Length)
                        {
                            break;
                        }
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        output = new char[int.Parse(e.Message, CultureInfo.InvariantCulture)];
                        continue;
                    }
                }
                if (!expect.Equals(new string(output, 0, reqLength)))
                {
                    Errln("FAIL: case " + i
                        + " expected '" + expect + "' (" + Hex(expect) + ")"
                        + " but got '" + new string(output)
                        + "' (" + Hex(new string(output)) + ")");
                }
            }
            output = new char[1];
            for (int i = 0; i < tests.Length; i++)
            {
                char[] input = Utility.Unescape(tests[i][0]).ToCharArray();
                string expect = Utility.Unescape(tests[i][outCol]);

                Logln("Normalizing '" + new string(input) + "' (" +
                            Hex(new string(input)) + ")");
                int reqLength = 0;
                while (true)
                {
                    try
                    {
                        reqLength = Normalizer.Decompose(input, 0, input.Length, output, 0, output.Length, mode == NormalizerMode.NFKD, 0);
                        if (reqLength <= output.Length)
                        {
                            break;
                        }
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        output = new char[int.Parse(e.Message, CultureInfo.InvariantCulture)];
                        continue;
                    }
                }
                if (!expect.Equals(new string(output, 0, reqLength)))
                {
                    Errln("FAIL: case " + i
                        + " expected '" + expect + "' (" + Hex(expect) + ")"
                        + " but got '" + new string(output)
                        + "' (" + Hex(new string(output)) + ")");
                }
                char[] output2 = new char[reqLength * 2];
                System.Array.Copy(output, 0, output2, 0, reqLength);
                int retLength = Normalizer.Decompose(input, 0, input.Length, output2, reqLength, output2.Length, mode == NormalizerMode.NFKC, 0);
                if (retLength != reqLength)
                {
                    Logln("FAIL: Normalizer.compose did not return the expected length. Expected: " + reqLength + " Got: " + retLength);
                }
            }
        }

        private void composeTest(NormalizerMode mode,
                                 string[][] tests, int outCol)
        {
            for (int i = 0; i < tests.Length; i++)
            {
                string input = Utility.Unescape(tests[i][0]);
                string expect = Utility.Unescape(tests[i][outCol]);

                Logln("Normalizing '" + input + "' (" + Hex(input) + ")");

                string output2 = Normalizer.Compose(input, mode == NormalizerMode.NFKC);

                if (!output2.Equals(expect))
                {
                    Errln("FAIL: case " + i
                        + " expected '" + expect + "' (" + Hex(expect) + ")"
                        + " but got '" + output2 + "' (" + Hex(output2) + ")");
                }
            }
            char[] output = new char[1];
            for (int i = 0; i < tests.Length; i++)
            {
                char[] input = Utility.Unescape(tests[i][0]).ToCharArray();
                string expect = Utility.Unescape(tests[i][outCol]);

                Logln("Normalizing '" + new string(input) + "' (" +
                            Hex(new string(input)) + ")");
                int reqLength = 0;
                while (true)
                {
                    try
                    {
                        reqLength = Normalizer.Compose(input, output, mode == NormalizerMode.NFKC, 0);
                        if (reqLength <= output.Length)
                        {
                            break;
                        }
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        output = new char[int.Parse(e.Message, CultureInfo.InvariantCulture)];
                        continue;
                    }
                }
                if (!expect.Equals(new string(output, 0, reqLength)))
                {
                    Errln("FAIL: case " + i
                        + " expected '" + expect + "' (" + Hex(expect) + ")"
                        + " but got '" + new string(output)
                        + "' (" + Hex(new string(output)) + ")");
                }
            }
            output = new char[1];
            for (int i = 0; i < tests.Length; i++)
            {
                char[] input = Utility.Unescape(tests[i][0]).ToCharArray();
                string expect = Utility.Unescape(tests[i][outCol]);

                Logln("Normalizing '" + new string(input) + "' (" +
                            Hex(new string(input)) + ")");
                int reqLength = 0;
                while (true)
                {
                    try
                    {
                        reqLength = Normalizer.Compose(input, 0, input.Length, output, 0, output.Length, mode == NormalizerMode.NFKC, 0);
                        if (reqLength <= output.Length)
                        {
                            break;
                        }
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        output = new char[int.Parse(e.Message, CultureInfo.InvariantCulture)];
                        continue;
                    }
                }
                if (!expect.Equals(new string(output, 0, reqLength)))
                {
                    Errln("FAIL: case " + i
                        + " expected '" + expect + "' (" + Hex(expect) + ")"
                        + " but got '" + new string(output)
                        + "' (" + Hex(new string(output)) + ")");
                }

                char[] output2 = new char[reqLength * 2];
                System.Array.Copy(output, 0, output2, 0, reqLength);
                int retLength = Normalizer.Compose(input, 0, input.Length, output2, reqLength, output2.Length, mode == NormalizerMode.NFKC, 0);
                if (retLength != reqLength)
                {
                    Logln("FAIL: Normalizer.compose did not return the expected length. Expected: " + reqLength + " Got: " + retLength);
                }
            }
        }
        private void iterateTest(Normalizer iter, string[][] tests, int outCol)
        {
            for (int i = 0; i < tests.Length; i++)
            {
                string input = Utility.Unescape(tests[i][0]);
                string expect = Utility.Unescape(tests[i][outCol]);

                Logln("Normalizing '" + input + "' (" + Hex(input) + ")");

                iter.SetText(input);
                assertEqual(expect, iter, "case " + i + " ");
            }
        }

        private void assertEqual(string expected, Normalizer iter, string msg)
        {
            int index = 0;
            int ch;
            UCharacterIterator cIter = UCharacterIterator.GetInstance(expected);

            while ((ch = iter.Next()) != Normalizer.Done)
            {
                if (index >= expected.Length)
                {
                    Errln("FAIL: " + msg + "Unexpected character '" + (char)ch
                            + "' (" + Hex(ch) + ")"
                            + " at index " + index);
                    break;
                }
                int want = UTF16.CharAt(expected, index);
                if (ch != want)
                {
                    Errln("FAIL: " + msg + "got '" + (char)ch
                            + "' (" + Hex(ch) + ")"
                            + " but expected '" + want + "' (" + Hex(want) + ")"
                            + " at index " + index);
                }
                index += UTF16.GetCharCount(ch);
            }
            if (index < expected.Length)
            {
                Errln("FAIL: " + msg + "Only got " + index + " chars, expected "
                + expected.Length);
            }

            cIter.SetToLimit();
            while ((ch = iter.Previous()) != Normalizer.Done)
            {
                int want = cIter.PreviousCodePoint();
                if (ch != want)
                {
                    Errln("FAIL: " + msg + "got '" + (char)ch
                            + "' (" + Hex(ch) + ")"
                            + " but expected '" + want + "' (" + Hex(want) + ")"
                            + " at index " + index);
                }
            }
        }
        //--------------------------------------------------------------------------

        // NOTE: These tests are used for quick debugging so are not ported
        // to ICU4C tsnorm.cpp in intltest
        //

        [Test]
        public void TestDebugStatic()
        {
            string @in = Utility.Unescape("\\U0001D157\\U0001D165");
            if (!Normalizer.IsNormalized(@in, NormalizerMode.NFC, 0))
            {
                Errln("isNormalized failed");
            }

            string input = "\uAD8B\uAD8B\uAD8B\uAD8B" +
                "\\U0001d15e\\U0001d157\\U0001d165\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e" +
                "\\U0001d15e\\U0001d157\\U0001d165\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e" +
                "\\U0001d15e\\U0001d157\\U0001d165\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e" +
                "\\U0001d157\\U0001d165\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e" +
                "\\U0001d157\\U0001d165\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e" +
                "aaaaaaaaaaaaaaaaaazzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz" +
                "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb" +
                "ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc" +
                "ddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd" +
                "\uAD8B\uAD8B\uAD8B\uAD8B" +
                "d\u031B\u0307\u0323";
            string expect = "\u1100\u116F\u11AA\u1100\u116F\u11AA\u1100\u116F" +
                            "\u11AA\u1100\u116F\u11AA\uD834\uDD57\uD834\uDD65" +
                            "\uD834\uDD57\uD834\uDD65\uD834\uDD57\uD834\uDD65" +
                            "\uD834\uDD57\uD834\uDD65\uD834\uDD57\uD834\uDD65" +
                            "\uD834\uDD57\uD834\uDD65\uD834\uDD57\uD834\uDD65" +
                            "\uD834\uDD57\uD834\uDD65\uD834\uDD57\uD834\uDD65" +
                            "\uD834\uDD57\uD834\uDD65\uD834\uDD57\uD834\uDD65" +
                            "\uD834\uDD57\uD834\uDD65\uD834\uDD57\uD834\uDD65" +
                            "\uD834\uDD57\uD834\uDD65\uD834\uDD57\uD834\uDD65" +
                            "\uD834\uDD57\uD834\uDD65\uD834\uDD57\uD834\uDD65" +
                            "\uD834\uDD57\uD834\uDD65\uD834\uDD57\uD834\uDD65" +
                            "\uD834\uDD57\uD834\uDD65\uD834\uDD57\uD834\uDD65" +
                            "\uD834\uDD57\uD834\uDD65\uD834\uDD57\uD834\uDD65" +
                            "\uD834\uDD57\uD834\uDD65\uD834\uDD57\uD834\uDD65" +
                            "\uD834\uDD57\uD834\uDD65\uD834\uDD57\uD834\uDD65" +
                            "\uD834\uDD57\uD834\uDD65\uD834\uDD57\uD834\uDD65" +
                            "\uD834\uDD57\uD834\uDD65aaaaaaaaaaaaaaaaaazzzzzz" +
                            "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz" +
                            "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb" +
                            "bbbbbbbbbbbbbbbbbbbbbbbbccccccccccccccccccccccccccccc" +
                            "cccccccccccccccccccccccccccccccccccccccccccccccc" +
                            "ddddddddddddddddddddddddddddddddddddddddddddddddddddd" +
                            "dddddddddddddddddddddddd" +
                            "\u1100\u116F\u11AA\u1100\u116F\u11AA\u1100\u116F" +
                            "\u11AA\u1100\u116F\u11AA\u0064\u031B\u0323\u0307";
            string output = Normalizer.Normalize(Utility.Unescape(input),
                            NormalizerMode.NFD);
            if (!expect.Equals(output))
            {
                Errln("FAIL expected: " + Hex(expect) + " got: " + Hex(output));
            }



        }
        [Test]
        public void TestDebugIter()
        {
            string src = Utility.Unescape("\\U0001d15e\\U0001d157\\U0001d165\\U0001d15e");
            string expected = Utility.Unescape("\\U0001d15e\\U0001d157\\U0001d165\\U0001d15e");
            Normalizer iter = new Normalizer(new StringCharacterIterator(Utility.Unescape(src)),
                                                    NormalizerMode.None, 0);
            int index = 0;
            int ch;
            UCharacterIterator cIter = UCharacterIterator.GetInstance(expected);

            while ((ch = iter.Next()) != Normalizer.Done)
            {
                if (index >= expected.Length)
                {
                    Errln("FAIL: " + "Unexpected character '" + (char)ch
                            + "' (" + Hex(ch) + ")"
                            + " at index " + index);
                    break;
                }
                int want = UTF16.CharAt(expected, index);
                if (ch != want)
                {
                    Errln("FAIL: " + "got '" + (char)ch
                            + "' (" + Hex(ch) + ")"
                            + " but expected '" + want + "' (" + Hex(want) + ")"
                            + " at index " + index);
                }
                index += UTF16.GetCharCount(ch);
            }
            if (index < expected.Length)
            {
                Errln("FAIL: " + "Only got " + index + " chars, expected "
                + expected.Length);
            }

            cIter.SetToLimit();
            while ((ch = iter.Previous()) != Normalizer.Done)
            {
                int want = cIter.PreviousCodePoint();
                if (ch != want)
                {
                    Errln("FAIL: " + "got '" + (char)ch
                            + "' (" + Hex(ch) + ")"
                            + " but expected '" + want + "' (" + Hex(want) + ")"
                            + " at index " + index);
                }
            }
        }
        [Test]
        public void TestDebugIterOld()
        {
            string input = "\\U0001D15E";
            string expected = "\uD834\uDD57\uD834\uDD65";
            string expectedReverse = "\uD834\uDD65\uD834\uDD57";
            int index = 0;
            int ch;
            Normalizer iter = new Normalizer(new StringCharacterIterator(Utility.Unescape(input)),
                                                    NormalizerMode.NFKC, 0);
            StringBuffer got = new StringBuffer();
            for (ch = iter.First(); ch != Normalizer.Done; ch = iter.Next())
            {
                if (index >= expected.Length)
                {
                    Errln("FAIL: " + "Unexpected character '" + (char)ch +
                           "' (" + Hex(ch) + ")" + " at index " + index);
                    break;
                }
                got.Append(UChar.ConvertFromUtf32(ch));
                index++;
            }
            if (!expected.Equals(got.ToString()))
            {
                Errln("FAIL: " + "got '" + got + "' (" + Hex(got) + ")"
                        + " but expected '" + expected + "' ("
                        + Hex(expected) + ")");
            }
            if (got.Length < expected.Length)
            {
                Errln("FAIL: " + "Only got " + index + " chars, expected "
                               + expected.Length);
            }

            Logln("Reverse Iteration\n");
            iter.SetIndexOnly(iter.EndIndex);
            got.Length = 0;
            for (ch = iter.Previous(); ch != Normalizer.Done; ch = iter.Previous())
            {
                if (index >= expected.Length)
                {
                    Errln("FAIL: " + "Unexpected character '" + (char)ch
                                   + "' (" + Hex(ch) + ")" + " at index " + index);
                    break;
                }
                got.Append(UChar.ConvertFromUtf32(ch));
            }
            if (!expectedReverse.Equals(got.ToString()))
            {
                Errln("FAIL: " + "got '" + got + "' (" + Hex(got) + ")"
                               + " but expected '" + expected
                               + "' (" + Hex(expected) + ")");
            }
            if (got.Length < expected.Length)
            {
                Errln("FAIL: " + "Only got " + index + " chars, expected "
                          + expected.Length);
            }

        }
        //--------------------------------------------------------------------------
        // helper class for TestPreviousNext()
        // simple UTF-32 character iterator
        class UCharIterator
        {

            public UCharIterator(int[] src, int len, int index)
            {

                s = src;
                length = len;
                i = index;
            }

            public int Current
            {
                get
                {
                    if (i < length)
                    {
                        return s[i];
                    }
                    else
                    {
                        return -1;
                    }
                }
            }

            public int Next()
            {
                if (i < length)
                {
                    return s[i++];
                }
                else
                {
                    return -1;
                }
            }

            public int Previous()
            {
                if (i > 0)
                {
                    return s[--i];
                }
                else
                {
                    return -1;
                }
            }

            public int Index
            {
                get { return i; }
            }

            private int[] s;
            private int length, i;
        }
        [Test]
        public void TestPreviousNext()
        {
            // src and expect strings
            char[] src ={
                UTF16.GetLeadSurrogate(0x2f999), UTF16.GetTrailSurrogate(0x2f999),
                UTF16.GetLeadSurrogate(0x1d15f), UTF16.GetTrailSurrogate(0x1d15f),
                (char)0xc4,
                (char)0x1ed0
            };
            int[] expect ={
                0x831d,
                0x1d158, 0x1d165,
                0x41, 0x308,
                0x4f, 0x302, 0x301
            };

            // expected src indexes corresponding to expect indexes
            int[] expectIndex ={
                0,
                2, 2,
                4, 4,
                5, 5, 5,
                6 // behind last character
            };

            // initial indexes into the src and expect strings

            int SRC_MIDDLE = 4;
            int EXPECT_MIDDLE = 3;


            // movement vector
            // - for previous(), 0 for current(), + for next()
            // not const so that we can terminate it below for the error message
            string moves = "0+0+0--0-0-+++0--+++++++0--------";

            // iterators
            Normalizer iter = new Normalizer(new string(src),
                                                    NormalizerMode.NFD, 0);
            UCharIterator iter32 = new UCharIterator(expect, expect.Length,
                                                         EXPECT_MIDDLE);

            int c1, c2;
            char m;

            // initially set the indexes into the middle of the strings
            iter.SetIndexOnly(SRC_MIDDLE);

            // move around and compare the iteration code points with
            // the expected ones
            int movesIndex = 0;
            while (movesIndex < moves.Length)
            {
                m = moves[movesIndex++];
                if (m == '-')
                {
                    c1 = iter.Previous();
                    c2 = iter32.Previous();
                }
                else if (m == '0')
                {
                    c1 = iter.Current;
                    c2 = iter32.Current;
                }
                else /* m=='+' */
                {
                    c1 = iter.Next();
                    c2 = iter32.Next();
                }

                // compare results
                if (c1 != c2)
                {
                    // copy the moves until the current (m) move, and terminate
                    string history = moves.Substring(0, movesIndex); // ICU4N: Checked 2nd parameter
                    Errln("error: mismatch in Normalizer iteration at " + history + ": "
                          + "got c1= " + Hex(c1) + " != expected c2= " + Hex(c2));
                    break;
                }

                // compare indexes
                if (iter.Index != expectIndex[iter32.Index])
                {
                    // copy the moves until the current (m) move, and terminate
                    string history = moves.Substring(0, movesIndex); // ICU4N: Checked 2nd parameter
                    Errln("error: index mismatch in Normalizer iteration at "
                          + history + " : " + "Normalizer index " + iter.Index
                          + " expected " + expectIndex[iter32.Index]);
                    break;
                }
            }
        }
        // Only in ICU4j
        [Test]
        public void TestPreviousNextJCI()
        {
            // src and expect strings
            char[] src = {
                UTF16.GetLeadSurrogate(0x2f999), UTF16.GetTrailSurrogate(0x2f999),
                UTF16.GetLeadSurrogate(0x1d15f), UTF16.GetTrailSurrogate(0x1d15f),
                (char)0xc4,
                (char)0x1ed0
            };
            int[] expect ={
                0x831d,
                0x1d158, 0x1d165,
                0x41, 0x308,
                0x4f, 0x302, 0x301
            };

            // expected src indexes corresponding to expect indexes
            int[] expectIndex ={
                0,
                2, 2,
                4, 4,
                5, 5, 5,
                6 // behind last character
            };

            // initial indexes into the src and expect strings

            int SRC_MIDDLE = 4;
            int EXPECT_MIDDLE = 3;


            // movement vector
            // - for previous(), 0 for current(), + for next()
            // not const so that we can terminate it below for the error message
            string moves = "0+0+0--0-0-+++0--+++++++0--------";

            // iterators
            StringCharacterIterator text = new StringCharacterIterator(new string(src));
            Normalizer iter = new Normalizer(text, NormalizerMode.NFD, 0);
            UCharIterator iter32 = new UCharIterator(expect, expect.Length,
                                                         EXPECT_MIDDLE);

            int c1, c2;
            char m;

            // initially set the indexes into the middle of the strings
            iter.SetIndexOnly(SRC_MIDDLE);

            // move around and compare the iteration code points with
            // the expected ones
            int movesIndex = 0;
            while (movesIndex < moves.Length)
            {
                m = moves[movesIndex++];
                if (m == '-')
                {
                    c1 = iter.Previous();
                    c2 = iter32.Previous();
                }
                else if (m == '0')
                {
                    c1 = iter.Current;
                    c2 = iter32.Current;
                }
                else /* m=='+' */
                {
                    c1 = iter.Next();
                    c2 = iter32.Next();
                }

                // compare results
                if (c1 != c2)
                {
                    // copy the moves until the current (m) move, and terminate
                    string history = moves.Substring(0, movesIndex); // ICU4N: Checked 2nd parameter
                    Errln("error: mismatch in Normalizer iteration at " + history + ": "
                          + "got c1= " + Hex(c1) + " != expected c2= " + Hex(c2));
                    break;
                }

                // compare indexes
                if (iter.Index != expectIndex[iter32.Index])
                {
                    // copy the moves until the current (m) move, and terminate
                    string history = moves.Substring(0, movesIndex); // ICU4N: Checked 2nd parameter
                    Errln("error: index mismatch in Normalizer iteration at "
                          + history + " : " + "Normalizer index " + iter.Index
                          + " expected " + expectIndex[iter32.Index]);
                    break;
                }
            }
        }

        // test APIs that are not otherwise used - improve test coverage
        [Test]
        public void TestNormalizerAPI()
        {
            try
            {
                // instantiate a Normalizer from a CharacterIterator
                string s = Utility.Unescape("a\u0308\uac00\\U0002f800");
                // make s a bit longer and more interesting
                UCharacterIterator iter = UCharacterIterator.GetInstance(s + s);
                Normalizer norm = new Normalizer(iter, NormalizerMode.NFC /*, 0*/); // ICU4N specific - to specify default options, leave them out of the constructor
                if (norm.Next() != 0xe4)
                {
                    Errln("error in Normalizer(CharacterIterator).Next()");
                }

                // test clone(), ==, and hashCode()
                Normalizer clone = (Normalizer)norm.Clone();
                if (clone.Equals(norm))
                {
                    Errln("error in Normalizer(Normalizer(CharacterIterator)).clone()!=norm");
                }

                if (clone.Length != norm.Length)
                {
                    Errln("error in Normalizer.getBeginIndex()");
                }
                // clone must have the same hashCode()
                //if(clone.hashCode()!=norm.hashCode()) {
                //    Errln("error in Normalizer(Normalizer(CharacterIterator)).clone().hashCode()!=copy.hashCode()");
                //}
                if (clone.Next() != 0xac00)
                {
                    Errln("error in Normalizer(Normalizer(CharacterIterator)).Next()");
                }
                int ch = clone.Next();
                if (ch != 0x4e3d)
                {
                    Errln("error in Normalizer(Normalizer(CharacterIterator)).clone().Next()");
                }
                // position changed, must change hashCode()
                if (clone.GetHashCode() == norm.GetHashCode())
                {
                    Errln("error in Normalizer(Normalizer(CharacterIterator)).clone().Next().hashCode()==copy.hashCode()");
                }

                // test compose() and decompose()
                StringBuffer tel;
                string nfkc, nfkd;
                tel = new StringBuffer("\u2121\u2121\u2121\u2121\u2121\u2121\u2121\u2121\u2121\u2121");
                tel.Insert(1, (char)0x0301);

                nfkc = Normalizer.Compose(tel.ToString(), true);
                nfkd = Normalizer.Decompose(tel.ToString(), true);
                if (
                    !nfkc.Equals(Utility.Unescape("TE\u0139TELTELTELTELTELTELTELTELTEL")) ||
                    !nfkd.Equals(Utility.Unescape("TEL\u0301TELTELTELTELTELTELTELTELTEL"))
                )
                {
                    Errln("error in Normalizer::(de)compose(): wrong result(s)");
                }

                // test setIndex()
                ch = norm.SetIndex(3);
                if (ch != 0x4e3d)
                {
                    Errln("error in Normalizer(CharacterIterator).setIndex(3)");
                }

                // test setText(CharacterIterator) and getText()
                string @out, out2;
                clone.SetText(iter);

                @out = clone.GetText();
                out2 = iter.GetText();
                if (!@out.Equals(out2) ||
                    clone.StartIndex != 0 ||
                    clone.EndIndex != iter.Length
                    )
                {
                    Errln("error in Normalizer::setText() or Normalizer::getText()");
                }

                char[] fillIn1 = new char[clone.Length];
                char[] fillIn2 = new char[iter.Length];
                int len = clone.GetText(fillIn1);
                iter.GetText(fillIn2, 0);
                if (!Utility.ArrayRegionMatches(fillIn1, 0, fillIn2, 0, len))
                {
                    Errln("error in Normalizer.GetText(). Normalizer: " +
                                    Utility.Hex(new string(fillIn1)) +
                                    " Iter: " + Utility.Hex(new string(fillIn2)));
                }

                clone.SetText(fillIn1);
                len = clone.GetText(fillIn2);
                if (!Utility.ArrayRegionMatches(fillIn1, 0, fillIn2, 0, len))
                {
                    Errln("error in Normalizer.SetText() or Normalizer.GetText()" +
                                    Utility.Hex(new string(fillIn1)) +
                                    " Iter: " + Utility.Hex(new string(fillIn2)));
                }

                // test setText(UChar *), getUMode() and setMode()
                clone.SetText(s);
                clone.SetIndexOnly(1);
                clone.SetMode(NormalizerMode.NFD);
                if (clone.GetMode() != NormalizerMode.NFD)
                {
                    Errln("error in Normalizer::setMode() or Normalizer::getMode()");
                }
                if (clone.Next() != 0x308 || clone.Next() != 0x1100)
                {
                    Errln("error in Normalizer::setText() or Normalizer::setMode()");
                }

                // test last()/previous() with an internal buffer overflow
                StringBuffer buf = new StringBuffer("aaaaaaaaaa");
                buf[10 - 1] = '\u0308';
                clone.SetText(buf);
                if (clone.Last() != 0x308)
                {
                    Errln("error in Normalizer(10*U+0308).last()");
                }

                // test UNORM_NONE
                norm.SetMode(NormalizerMode.None);
                if (norm.First() != 0x61 || norm.Next() != 0x308 || norm.Last() != 0x2f800)
                {
                    Errln("error in Normalizer(UNORM_NONE).first()/next()/last()");
                }
                @out = Normalizer.Normalize(s, NormalizerMode.None);
                if (!@out.Equals(s))
                {
                    Errln("error in Normalizer::normalize(UNORM_NONE)");
                }
                ch = 0x1D15E;
                string exp = "\\U0001D157\\U0001D165";
                string ns = Normalizer.Normalize(ch, NormalizerMode.NFC);
                if (!ns.Equals(Utility.Unescape(exp)))
                {
                    Errln("error in Normalizer.Normalize(int,Mode)");
                }
                ns = Normalizer.Normalize(ch, NormalizerMode.NFC, 0);
                if (!ns.Equals(Utility.Unescape(exp)))
                {
                    Errln("error in Normalizer.Normalize(int,Mode,int)");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [Test]
        public void TestConcatenate()
        {

            object[][] cases = new object[][]{
                /* mode, left, right, result */
                new object[]{
                    NormalizerMode.NFC,
                    "re",
                    "\u0301sum\u00e9",
                    "r\u00e9sum\u00e9"
                },
                new object[]{
                    NormalizerMode.NFC,
                    "a\u1100",
                    "\u1161bcdefghijk",
                    "a\uac00bcdefghijk"
                },
                /* ### TODO: add more interesting cases */
                new object[]{
                    NormalizerMode.NFD,
                    "\u03B1\u0345",
                    "\u0C4D\uD804\uDCBA\uD834\uDD69",  // 0C4D 110BA 1D169
                    "\u03B1\uD834\uDD69\uD804\uDCBA\u0C4D\u0345"  // 03B1 1D169 110BA 0C4D 0345
                }
            };

            string left, right, expect, result;
            NormalizerMode mode;
            int i;

            /* test concatenation */
            for (i = 0; i < cases.Length; ++i)
            {
                mode = (NormalizerMode)cases[i][0];

                left = (string)cases[i][1];
                right = (string)cases[i][2];
                expect = (string)cases[i][3];
                {
                    result = Normalizer.Concatenate(left, right, mode, 0);
                    if (!result.Equals(expect))
                    {
                        Errln("error in Normalizer.Concatenate(), cases[] failed"
                              + ", result==expect: expected: "
                              + Hex(expect) + " =========> got: " + Hex(result));
                    }
                }
                {
                    result = Normalizer.Concatenate(left.ToCharArray(), right.ToCharArray(), mode, 0);
                    if (!result.Equals(expect))
                    {
                        Errln("error in Normalizer.Concatenate(), cases[] failed"
                              + ", result==expect: expected: "
                              + Hex(expect) + " =========> got: " + Hex(result));
                    }
                }
            }

            mode = NormalizerMode.NFC; // (Normalizer.Mode)cases2[0][0];
            char[] destination = "My resume is here".ToCharArray();
            left = "resume";
            right = "re\u0301sum\u00e9 is HERE";
            expect = "My r\u00e9sum\u00e9 is HERE";

            // Concatenates 're' with '\u0301sum\u00e9 is HERE' and places the result at
            // position 3 of string 'My resume is here'.
            Normalizer.Concatenate(left.ToCharArray(), 0, 2, right.ToCharArray(), 2, 15,
                                             destination, 3, 17, mode, 0);
            if (!new string(destination).Equals(expect))
            {
                Errln("error in Normalizer.Concatenate(), cases2[] failed"
                      + ", result==expect: expected: "
                      + Hex(expect) + " =========> got: " + Hex(destination));
            }

            // Error case when result of concatenation won't fit into destination array.
            try
            {
                Normalizer.Concatenate(left.ToCharArray(), 0, 2, right.ToCharArray(), 2, 15,
                                             destination, 3, 16, mode, 0);
            }
            catch (IndexOutOfRangeException e)
            {
                assertTrue("Normalizer.Concatenate() failed", e.Message.Equals("14"));
                return;
            }
            fail("Normalizer.Concatenate() tested for failure but passed");
        }

        private readonly int RAND_MAX = 0x7fff;

        [Test]
        public void TestCheckFCD()
        {
            char[] FAST = {(char)0x0001, (char)0x0002, (char)0x0003, (char)0x0004, (char)0x0005, (char)0x0006, (char)0x0007,
                     (char)0x0008, (char)0x0009, (char)0x000A};

            char[] FALSE = {(char)0x0001, (char)0x0002, (char)0x02EA, (char)0x03EB, (char)0x0300, (char)0x0301,
                      (char)0x02B9, (char)0x0314, (char)0x0315, (char)0x0316};

            char[] TRUE = {(char)0x0030, (char)0x0040, (char)0x0440, (char)0x056D, (char)0x064F, (char)0x06E7,
                     (char)0x0050, (char)0x0730, (char)0x09EE, (char)0x1E10};

            char[][] datastr = { new char[] {(char)0x0061, (char)0x030A, (char)0x1E05, (char)0x0302, (char)0 },
                         new char[]  { (char)0x0061, (char)0x030A, (char)0x00E2, (char)0x0323, (char)0 },
                         new char[]  { (char)0x0061, (char)0x0323, (char)0x00E2, (char)0x0323, (char)0 },
                         new char[]  { (char)0x0061, (char)0x0323, (char)0x1E05, (char)0x0302, (char)0 }
                        };
            QuickCheckResult[] result = { QuickCheckResult.Yes, QuickCheckResult.No, QuickCheckResult.No, QuickCheckResult.Yes };

            char[] datachar =  {        (char)0x60, (char)0x61, (char)0x62, (char)0x63, (char)0x64, (char)0x65, (char)0x66, (char)0x67, (char)0x68, (char)0x69,
                                (char)0x6a,
                                (char)0xe0, (char)0xe1, (char)0xe2, (char)0xe3, (char)0xe4, (char)0xe5, (char)0xe6, (char)0xe7, (char)0xe8, (char)0xe9,
                                (char)0xea,
                                (char)0x0300, (char)0x0301, (char)0x0302, (char)0x0303, (char)0x0304, (char)0x0305, (char)0x0306,
                                (char)0x0307, (char)0x0308, (char)0x0309, (char)0x030a,
                                (char)0x0320, (char)0x0321, (char)0x0322, (char)0x0323, (char)0x0324, (char)0x0325, (char)0x0326,
                                (char)0x0327, (char)0x0328, (char)0x0329, (char)0x032a,
                                (char)0x1e00, (char)0x1e01, (char)0x1e02, (char)0x1e03, (char)0x1e04, (char)0x1e05, (char)0x1e06,
                                (char)0x1e07, (char)0x1e08, (char)0x1e09, (char)0x1e0a
                       };

            int count = 0;

            if (Normalizer.QuickCheck(FAST, 0, FAST.Length, NormalizerMode.FCD, 0) != QuickCheckResult.Yes)
                Errln("Normalizer.QuickCheck(FCD) failed: expected value for fast Normalizer.quickCheck is NormalizerQuickCheckResult.Yes\n");
            if (Normalizer.QuickCheck(FALSE, 0, FALSE.Length, NormalizerMode.FCD, 0) != QuickCheckResult.No)
                Errln("Normalizer.QuickCheck(FCD) failed: expected value for error Normalizer.quickCheck is NormalizerQuickCheckResult.No\n");
            if (Normalizer.QuickCheck(TRUE, 0, TRUE.Length, NormalizerMode.FCD, 0) != QuickCheckResult.Yes)
                Errln("Normalizer.QuickCheck(FCD) failed: expected value for correct Normalizer.quickCheck is NormalizerQuickCheckResult.Yes\n");


            while (count < 4)
            {
                QuickCheckResult fcdresult = Normalizer.QuickCheck(datastr[count], 0, datastr[count].Length, NormalizerMode.FCD, 0);
                if (result[count] != fcdresult)
                {
                    Errln("Normalizer.QuickCheck(FCD) failed: Data set " + count
                            + " expected value " + result[count]);
                }
                count++;
            }

            /* random checks of long strings */
            //srand((unsigned)time( NULL ));
            Random rand = CreateRandom(); // use test framework's random

            for (count = 0; count < 50; count++)
            {
                int size = 0;
                QuickCheckResult testresult = QuickCheckResult.Yes;
                char[] data = new char[20];
                char[] norm = new char[100];
                char[] nfd = new char[100];
                int normStart = 0;
                int nfdsize = 0;
                while (size != 19)
                {
                    data[size] = datachar[rand.Next(RAND_MAX) * 50 / RAND_MAX];
                    Logln("0x" + data[size]);
                    normStart += Normalizer.Normalize(data, size, size + 1,
                                                        norm, normStart, 100,
                                                        NormalizerMode.NFD, 0);
                    size++;
                }
                Logln("\n");

                nfdsize = Normalizer.Normalize(data, 0, size, nfd, 0, nfd.Length, NormalizerMode.NFD, 0);
                //    nfdsize = unorm_normalize(data, size, UNORM_NFD, UCOL_IGNORE_HANGUL,
                //                      nfd, 100, &status);
                if (nfdsize != normStart || Utility.ArrayRegionMatches(nfd, 0, norm, 0, nfdsize) == false)
                {
                    testresult = QuickCheckResult.No;
                }
                if (testresult == QuickCheckResult.Yes)
                {
                    Logln("result NormalizerQuickCheckResult.Yes\n");
                }
                else
                {
                    Logln("result NormalizerQuickCheckResult.No\n");
                }

                if (Normalizer.QuickCheck(data, 0, data.Length, NormalizerMode.FCD, 0) != testresult)
                {
                    Errln("Normalizer.QuickCheck(FCD) failed: expected " + testresult + " for random data: " + Hex(new string(data)));
                }
            }
        }


        // reference implementation of Normalizer::compare
        private int ref_norm_compare(string s1, string s2, int options)
        {
            string t1, t2, r1, r2;

            // ICU4N specific - don't need the shift outside of Normalizer,
            // as we have fixed this by using separate enums
            //int normOptions = options >> Normalizer.COMPARE_NORM_OPTIONS_SHIFT;
            var normalizerVersion = options.AsFlagsToEnum<NormalizerUnicodeVersion>();
            var foldCase = options.AsFlagsToEnum<FoldCase>();

            if ((options & Normalizer.COMPARE_IGNORE_CASE) != 0)
            {
                // NFD(toCasefold(NFD(X))) = NFD(toCasefold(NFD(Y)))
                r1 = Normalizer.Decompose(s1, false, normalizerVersion);
                r2 = Normalizer.Decompose(s2, false, normalizerVersion);
                r1 = UChar.FoldCase(r1, foldCase);
                r2 = UChar.FoldCase(r2, foldCase);
            }
            else
            {
                r1 = s1;
                r2 = s2;
            }

            t1 = Normalizer.Decompose(r1, false, normalizerVersion);
            t2 = Normalizer.Decompose(r2, false, normalizerVersion);

            if ((options & Normalizer.COMPARE_CODE_POINT_ORDER) != 0)
            {
                UTF16.StringComparer comp
                        = new UTF16.StringComparer(true, false,
                                         UTF16.StringComparer.FoldCaseDefault);
                return comp.Compare(t1, t2);
            }
            else
            {
                return t1.CompareToOrdinal(t2);
            }

        }

        // test wrapper for Normalizer::compare, sets UNORM_INPUT_IS_FCD appropriately
        private int norm_compare(string s1, string s2, int options)
        {
            // ICU4N specific - don't need the shift outside of Normalizer,
            // as we have fixed this by using separate enums
            //int normOptions = options >> Normalizer.COMPARE_NORM_OPTIONS_SHIFT;

            var unicodeVersion = options.AsFlagsToEnum<NormalizerUnicodeVersion>();


            if (QuickCheckResult.Yes == Normalizer.QuickCheck(s1, NormalizerMode.FCD, unicodeVersion) &&
                QuickCheckResult.Yes == Normalizer.QuickCheck(s2, NormalizerMode.FCD, unicodeVersion))
            {
                options |= (int)NormalizerComparison.InputIsFCD; //Normalizer.INPUT_IS_FCD;
            }

            // Look out! The above code may changes the options that go into Compare
            var normalizerComparison = options.AsFlagsToEnum<NormalizerComparison>();
            var foldCase = options.AsFlagsToEnum<FoldCase>();

            int cmpStrings = Normalizer.Compare(s1, s2, normalizerComparison, foldCase, unicodeVersion);
            int cmpArrays = Normalizer.Compare(
                    s1.ToCharArray(), 0, s1.Length,
                    s2.ToCharArray(), 0, s2.Length, normalizerComparison, foldCase, unicodeVersion);
            assertEquals("compare strings == compare char arrays", cmpStrings, cmpArrays);
            return cmpStrings;
        }

        // reference implementation of UnicodeString::caseCompare
        private int ref_case_compare(string s1, string s2, int options)
        {
            string t1, t2;

            t1 = s1;
            t2 = s2;

            t1 = UChar.FoldCase(t1, ((options & Normalizer.FOLD_CASE_EXCLUDE_SPECIAL_I) == 0));
            t2 = UChar.FoldCase(t2, ((options & Normalizer.FOLD_CASE_EXCLUDE_SPECIAL_I) == 0));

            if ((options & Normalizer.COMPARE_CODE_POINT_ORDER) != 0)
            {
                UTF16.StringComparer comp
                        = new UTF16.StringComparer(true, false,
                                        UTF16.StringComparer.FoldCaseDefault);
                return comp.Compare(t1, t2);
            }
            else
            {
                return t1.CompareToOrdinal(t2);
            }

        }

        // reduce an integer to -1/0/1
        private static int sign(int value)
        {
            if (value == 0)
            {
                return 0;
            }
            else
            {
                return (value >> 31) | 1;
            }
        }
        private static string signString(int value)
        {
            if (value < 0)
            {
                return "<0";
            }
            else if (value == 0)
            {
                return "=0";
            }
            else /* value>0 */
            {
                return ">0";
            }
        }
        // test Normalizer::compare and unorm_compare (thinly wrapped by the former)
        // by comparing it with its semantic equivalent
        // since we trust the pieces, this is sufficient

        // test each string with itself and each other
        // each time with all options
        private string[] strings = new string[]{
                // some cases from NormalizationTest.txt
                // 0..3
                "D\u031B\u0307\u0323",
                "\u1E0C\u031B\u0307",
                "D\u031B\u0323\u0307",
                "d\u031B\u0323\u0307",

                // 4..6
                "\u00E4",
                "a\u0308",
                "A\u0308",

                // Angstrom sign = A ring
                // 7..10
                "\u212B",
                "\u00C5",
                "A\u030A",
                "a\u030A",

                // 11.14
                "a\u059A\u0316\u302A\u032Fb",
                "a\u302A\u0316\u032F\u059Ab",
                "a\u302A\u0316\u032F\u059Ab",
                "A\u059A\u0316\u302A\u032Fb",

                // from ICU case folding tests
                // 15..20
                "A\u00df\u00b5\ufb03\\U0001040c\u0131",
                "ass\u03bcffi\\U00010434i",
                "\u0061\u0042\u0131\u03a3\u00df\ufb03\ud93f\udfff",
                "\u0041\u0062\u0069\u03c3\u0073\u0053\u0046\u0066\u0049\ud93f\udfff",
                "\u0041\u0062\u0131\u03c3\u0053\u0073\u0066\u0046\u0069\ud93f\udfff",
                "\u0041\u0062\u0069\u03c3\u0073\u0053\u0046\u0066\u0049\ud93f\udffd",

                //     U+d800 U+10001   see implementation comment in unorm_cmpEquivFold
                // vs. U+10000          at bottom - code point order
                // 21..22
                "\ud800\ud800\udc01",
                "\ud800\udc00",

                // other code point order tests from ustrtest.cpp
                // 23..31
                "\u20ac\ud801",
                "\u20ac\ud800\udc00",
                "\ud800",
                "\ud800\uff61",
                "\udfff",
                "\uff61\udfff",
                "\uff61\ud800\udc02",
                "\ud800\udc02",
                "\ud84d\udc56",

                // long strings, see cnormtst.c/TestNormCoverage()
                // equivalent if case-insensitive
                // 32..33
                "\uAD8B\uAD8B\uAD8B\uAD8B"+
                "\\U0001d15e\\U0001d157\\U0001d165\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e"+
                "\\U0001d15e\\U0001d157\\U0001d165\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e"+
                "\\U0001d15e\\U0001d157\\U0001d165\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e"+
                "\\U0001d157\\U0001d165\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e"+
                "\\U0001d157\\U0001d165\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e"+
                "aaaaaaaaaaaaaaaaaazzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz"+
                "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"+
                "ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc"+
                "ddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd"+
                "\uAD8B\uAD8B\uAD8B\uAD8B"+
                "d\u031B\u0307\u0323",

                "\u1100\u116f\u11aa\uAD8B\uAD8B\u1100\u116f\u11aa"+
                "\\U0001d157\\U0001d165\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e"+
                "\\U0001d157\\U0001d165\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e"+
                "\\U0001d157\\U0001d165\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e"+
                "\\U0001d15e\\U0001d157\\U0001d165\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e"+
                "\\U0001d15e\\U0001d157\\U0001d165\\U0001d15e\\U0001d15e\\U0001d15e\\U0001d15e"+
                "aaaaaaaaaaAAAAAAAAZZZZZZZZZZZZZZZZzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz"+
                "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"+
                "ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc"+
                "ddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd"+
                "\u1100\u116f\u11aa\uAD8B\uAD8B\u1100\u116f\u11aa"+
                "\u1E0C\u031B\u0307",

                // some strings that may make a difference whether the compare function
                // case-folds or decomposes first
                // 34..41
                "\u0360\u0345\u0334",
                "\u0360\u03b9\u0334",

                "\u0360\u1f80\u0334",
                "\u0360\u03b1\u0313\u03b9\u0334",

                "\u0360\u1ffc\u0334",
                "\u0360\u03c9\u03b9\u0334",

                "a\u0360\u0345\u0360\u0345b",
                "a\u0345\u0360\u0345\u0360b",

                // interesting cases for canonical caseless match with turkic i handling
                // 42..43
                "\u00cc",
                "\u0069\u0300",

                // strings with post-Unicode 3.2 normalization or normalization corrections
                // 44..45
                "\u00e4\u193b\\U0002f868",
                "\u0061\u193b\u0308\u36fc",


    };

        // all combinations of options
        // UNORM_INPUT_IS_FCD is set automatically if both input strings fulfill FCD conditions
        sealed class Temp
        {
            internal int options;
            internal string name;
            public Temp(int opt, string str)
            {
                options = opt;
                name = str;
            }

        }
        // set UNORM_UNICODE_3_2 in one additional combination

        private Temp[] opt = new Temp[]{
                    new Temp(0,"default"),
                    new Temp(Normalizer.COMPARE_CODE_POINT_ORDER, "code point order" ),
                    new Temp(Normalizer.COMPARE_IGNORE_CASE, "ignore case" ),
                    new Temp(Normalizer.COMPARE_CODE_POINT_ORDER|Normalizer.COMPARE_IGNORE_CASE, "code point order & ignore case" ),
                    new Temp(Normalizer.COMPARE_IGNORE_CASE|Normalizer.FOLD_CASE_EXCLUDE_SPECIAL_I, "ignore case & special i"),
                    new Temp(Normalizer.COMPARE_CODE_POINT_ORDER|Normalizer.COMPARE_IGNORE_CASE|Normalizer.FOLD_CASE_EXCLUDE_SPECIAL_I, "code point order & ignore case & special i"),

                    // ICU4N: Shifting no longer necessary as it just complicates testing
                    new Temp(Normalizer.Unicode3_2, "Unicode 3.2")
                    //new Temp(Normalizer.Unicode3_2 << Normalizer.COMPARE_NORM_OPTIONS_SHIFT, "Unicode 3.2")
            };


        [Test]
        public void TestCompareDebug()
        {

            string[] s = new string[100]; // at least as many items as in strings[] !


            int i, j, k, count = strings.Length;
            int result, refResult;

            // create the UnicodeStrings
            for (i = 0; i < count; ++i)
            {
                s[i] = Utility.Unescape(strings[i]);
            }
            UTF16.StringComparer comp = new UTF16.StringComparer(true, false,
                                         UTF16.StringComparer.FoldCaseDefault);
            // test them each with each other

            i = 42;
            j = 43;
            k = 2;
            // test Normalizer::compare
            result = norm_compare(s[i], s[j], opt[k].options);
            refResult = ref_norm_compare(s[i], s[j], opt[k].options);
            if (sign(result) != sign(refResult))
            {
                Errln("Normalizer::compare( " + i + ", " + j + ", " + k + "( " + opt[k].name + "))=" + result + " should be same sign as " + refResult);
            }

            // test UnicodeString::caseCompare - same internal implementation function
            if (0 != (opt[k].options & Normalizer.COMPARE_IGNORE_CASE))
            {
                //    result=s[i]. (s[j], opt[k].options);
                if ((opt[k].options & Normalizer.FOLD_CASE_EXCLUDE_SPECIAL_I) == 0)
                {
                    comp.IgnoreCase = true;
                    comp.IgnoreCaseOption = UTF16.StringComparer.FoldCaseDefault;
                }
                else
                {
                    comp.IgnoreCase = true;
                    comp.IgnoreCaseOption = UTF16.StringComparer.FoldCaseExcludeSpecialI;
                }

                result = comp.Compare(s[i], s[j]);
                refResult = ref_case_compare(s[i], s[j], opt[k].options);
                if (sign(result) != sign(refResult))
                {
                    Errln("Normalizer::compare( " + i + ", " + j + ", " + k + "( " + opt[k].name + "))=" + result + " should be same sign as " + refResult);
                }
            }
            string value1 = "\u00dater\u00fd";
            string value2 = "\u00fater\u00fd";
            if (Normalizer.Compare(value1, value2, 0) != 0)
            {
                var foldCase = Normalizer.COMPARE_IGNORE_CASE.AsFlagsToEnum<FoldCase>();
                //if (Normalizer.Compare(value1, value2, Normalizer.COMPARE_IGNORE_CASE) == 0)
                if (Normalizer.Compare(value1, value2, NormalizerComparison.Default, foldCase) == 0)
                {

                }
            }
        }

        [Test]
        public void TestCompare()
        {

            string[] s = new string[100]; // at least as many items as in strings[] !

            int i, j, k, count = strings.Length;
            int result, refResult;

            // create the UnicodeStrings
            for (i = 0; i < count; ++i)
            {
                s[i] = Utility.Unescape(strings[i]);
            }
            UTF16.StringComparer comp = new UTF16.StringComparer();
            // test them each with each other
            for (i = 0; i < count; ++i)
            {
                for (j = i; j < count; ++j)
                {
                    for (k = 0; k < opt.Length; ++k)
                    {
                        // test Normalizer::compare
                        result = norm_compare(s[i], s[j], opt[k].options);
                        refResult = ref_norm_compare(s[i], s[j], opt[k].options);
                        if (sign(result) != sign(refResult))
                        {
                            Errln("Normalizer::compare( " + i + ", " + j + ", " + k + "( " + opt[k].name + "))=" + result + " should be same sign as " + refResult);
                        }

                        // test UnicodeString::caseCompare - same internal implementation function
                        if (0 != (opt[k].options & Normalizer.COMPARE_IGNORE_CASE))
                        {
                            //    result=s[i]. (s[j], opt[k].options);
                            if ((opt[k].options & Normalizer.FOLD_CASE_EXCLUDE_SPECIAL_I) == 0)
                            {
                                comp.IgnoreCase = true;
                                comp.IgnoreCaseOption = UTF16.StringComparer.FoldCaseDefault;
                            }
                            else
                            {
                                comp.IgnoreCase = true;
                                comp.IgnoreCaseOption = UTF16.StringComparer.FoldCaseExcludeSpecialI;
                            }

                            comp.CodePointCompare = (opt[k].options & Normalizer.COMPARE_CODE_POINT_ORDER) != 0;
                            // result=comp.caseCompare(s[i],s[j], opt[k].options);
                            result = comp.Compare(s[i], s[j]);
                            refResult = ref_case_compare(s[i], s[j], opt[k].options);
                            if (sign(result) != sign(refResult))
                            {
                                Errln("Normalizer::compare( " + i + ", " + j + ", " + k + "( " + opt[k].name + "))=" + result + " should be same sign as " + refResult);
                            }
                        }
                    }
                }
            }

            // test cases with i and I to make sure Turkic works
            char[] iI = { (char)0x49, (char)0x69, (char)0x130, (char)0x131 };
            UnicodeSet set = new UnicodeSet(), iSet = new UnicodeSet();
            Normalizer2Impl nfcImpl = Norm2AllModes.GetNFCInstance().Impl;
            nfcImpl.EnsureCanonIterData();

            string s1, s2;

            // collect all sets into one for contiguous output
            for (i = 0; i < iI.Length; ++i)
            {
                if (nfcImpl.GetCanonStartSet(iI[i], iSet))
                {
                    set.AddAll(iSet);
                }
            }

            // test all of these precomposed characters
            Normalizer2 nfcNorm2 = Normalizer2.GetNFCInstance();
            UnicodeSetIterator it = new UnicodeSetIterator(set);
            int c;
            while (it.Next() && (c = it.Codepoint) != UnicodeSetIterator.IsString)
            {
                s1 = UTF16.ValueOf(c);
                s2 = nfcNorm2.GetDecomposition(c);
                for (k = 0; k < opt.Length; ++k)
                {
                    // test Normalizer::compare

                    result = norm_compare(s1, s2, opt[k].options);
                    refResult = ref_norm_compare(s1, s2, opt[k].options);
                    if (sign(result) != sign(refResult))
                    {
                        Errln("Normalizer.compare(U+" + Hex(c) + " with its NFD, " + opt[k].name + ")"
                              + signString(result) + " should be " + signString(refResult));
                    }

                    // test UnicodeString::caseCompare - same internal implementation function
                    if ((opt[k].options & Normalizer.COMPARE_IGNORE_CASE) > 0)
                    {
                        if ((opt[k].options & Normalizer.FOLD_CASE_EXCLUDE_SPECIAL_I) == 0)
                        {
                            comp.IgnoreCase = true;
                            comp.IgnoreCaseOption = UTF16.StringComparer.FoldCaseDefault;
                        }
                        else
                        {
                            comp.IgnoreCase = true;
                            comp.IgnoreCaseOption = UTF16.StringComparer.FoldCaseExcludeSpecialI;
                        }

                        comp.CodePointCompare = (opt[k].options & Normalizer.COMPARE_CODE_POINT_ORDER) != 0;

                        result = comp.Compare(s1, s2);
                        refResult = ref_case_compare(s1, s2, opt[k].options);
                        if (sign(result) != sign(refResult))
                        {
                            Errln("UTF16.compare(U+" + Hex(c) + " with its NFD, "
                                  + opt[k].name + ")" + signString(result) + " should be " + signString(refResult));
                        }
                    }
                }
            }

            // test getDecomposition() for some characters that do not decompose
            if (nfcNorm2.GetDecomposition(0x20) != null ||
                nfcNorm2.GetDecomposition(0x4e00) != null ||
                nfcNorm2.GetDecomposition(0x20002) != null
            )
            {
                Errln("NFC.getDecomposition() returns TRUE for characters which do not have decompositions");
            }

            // test getRawDecomposition() for some characters that do not decompose
            if (nfcNorm2.GetRawDecomposition(0x20) != null ||
                nfcNorm2.GetRawDecomposition(0x4e00) != null ||
                nfcNorm2.GetRawDecomposition(0x20002) != null
            )
            {
                Errln("getRawDecomposition() returns TRUE for characters which do not have decompositions");
            }

            // test composePair() for some pairs of characters that do not compose
            if (nfcNorm2.ComposePair(0x20, 0x301) >= 0 ||
                nfcNorm2.ComposePair(0x61, 0x305) >= 0 ||
                nfcNorm2.ComposePair(0x1100, 0x1160) >= 0 ||
                nfcNorm2.ComposePair(0xac00, 0x11a7) >= 0
            )
            {
                Errln("NFC.composePair() incorrectly composes some pairs of characters");
            }

            // test FilteredNormalizer2.getDecomposition()
            UnicodeSet filter = new UnicodeSet("[^\u00a0-\u00ff]");
            FilteredNormalizer2 fn2 = new FilteredNormalizer2(nfcNorm2, filter);
            if (fn2.GetDecomposition(0xe4) != null || !"A\u0304".Equals(fn2.GetDecomposition(0x100)))
            {
                Errln("FilteredNormalizer2(NFC, ^A0-FF).getDecomposition() failed");
            }

            // test FilteredNormalizer2.getRawDecomposition()
            if (fn2.GetRawDecomposition(0xe4) != null || !"A\u0304".Equals(fn2.GetRawDecomposition(0x100)))
            {
                Errln("FilteredNormalizer2(NFC, ^A0-FF).getRawDecomposition() failed");
            }

            // test FilteredNormalizer2::composePair()
            if (0x100 != fn2.ComposePair(0x41, 0x304) ||
                fn2.ComposePair(0xc7, 0x301) >= 0 // unfiltered result: U+1E08
            )
            {
                Errln("FilteredNormalizer2(NFC, ^A0-FF).composePair() failed");
            }
        }

        // verify that case-folding does not un-FCD strings
        int countFoldFCDExceptions(int foldingOptions)
        {
            string s, d;
            int c;
            int count;
            int/*unsigned*/ cc, trailCC, foldCC, foldTrailCC;
            QuickCheckResult qcResult;
            UUnicodeCategory category;
            bool isNFD;


            Logln("Test if case folding may un-FCD a string (folding options 0x)" + Hex(foldingOptions));

            count = 0;
            for (c = 0; c <= 0x10ffff; ++c)
            {
                category = UChar.GetUnicodeCategory(c);
                if (category == UUnicodeCategory.OtherNotAssigned)
                {
                    continue; // skip unassigned code points
                }
                if (c == 0xac00)
                {
                    c = 0xd7a3; // skip Hangul - no case folding there
                    continue;
                }
                // skip Han blocks - no case folding there either
                if (c == 0x3400)
                {
                    c = 0x4db5;
                    continue;
                }
                if (c == 0x4e00)
                {
                    c = 0x9fa5;
                    continue;
                }
                if (c == 0x20000)
                {
                    c = 0x2a6d6;
                    continue;
                }

                s = UTF16.ValueOf(c);

                // get leading and trailing cc for c
                d = Normalizer.Decompose(s, false);
                isNFD = s == d;
                cc = UChar.GetCombiningClass(UTF16.CharAt(d, 0));
                trailCC = UChar.GetCombiningClass(UTF16.CharAt(d, d.Length - 1));

                // get leading and trailing cc for the case-folding of c
                UChar.FoldCase(s, (foldingOptions == 0));
                d = Normalizer.Decompose(s, false);
                foldCC = UChar.GetCombiningClass(UTF16.CharAt(d, 0));
                foldTrailCC = UChar.GetCombiningClass(UTF16.CharAt(d, d.Length - 1));

                qcResult = Normalizer.QuickCheck(s, NormalizerMode.FCD, 0);


                // bad:
                // - character maps to empty string: adjacent characters may then need reordering
                // - folding has different leading/trailing cc's, and they don't become just 0
                // - folding itself is not FCD
                if (qcResult != QuickCheckResult.Yes ||
                    s.Length == 0 ||
                    (cc != foldCC && foldCC != 0) || (trailCC != foldTrailCC && foldTrailCC != 0)
                )
                {
                    ++count;
                    Errln("U+" + Hex(c) + ": case-folding may un-FCD a string (folding options 0x" + Hex(foldingOptions) + ")");
                    //Errln("  cc %02x trailCC %02x    foldCC(U+%04lx) %02x foldTrailCC(U+%04lx) %02x   quickCheck(folded)=%d", cc, trailCC, UTF16.CharAt(d,0), foldCC, UTF16.CharAt(d,d.Length-1), foldTrailCC, qcResult);
                    continue;
                }

                // also bad:
                // if a code point is in NFD but its case folding is not, then
                // unorm_compare will also fail
                if (isNFD && QuickCheckResult.Yes != Normalizer.QuickCheck(s, NormalizerMode.NFD, 0))
                {
                    ++count;
                    Errln("U+" + Hex(c) + ": case-folding may un-FCD a string (folding options 0x" + Hex(foldingOptions) + ")");
                }
            }

            Logln("There are " + Hex(count) + " code points for which case-folding may un-FCD a string (folding options" + foldingOptions + "x)");
            return count;
        }

        [Test]
        public void TestFindFoldFCDExceptions()
        {
            int count;

            count = countFoldFCDExceptions(0);
            count += countFoldFCDExceptions(Normalizer.FOLD_CASE_EXCLUDE_SPECIAL_I);
            if (count > 0)
            {
                //*
                //* If case-folding un-FCDs any strings, then unorm_compare() must be
                //* re-implemented.
                //* It currently assumes that one can check for FCD then case-fold
                //* and then still have FCD strings for raw decomposition without reordering.
                //*
                Errln("error: There are " + count + " code points for which case-folding" +
                      " may un-FCD a string for all folding options.\n See comment" +
                      " in BasicNormalizerTest::FindFoldFCDExceptions()!");
            }
        }

        [Test]
        public void TestCombiningMarks()
        {
            string src = "\u0f71\u0f72\u0f73\u0f74\u0f75";
            string expected = "\u0F71\u0F71\u0F71\u0F72\u0F72\u0F74\u0F74";
            string result = Normalizer.Decompose(src, false);
            if (!expected.Equals(result))
            {
                Errln("Reordering of combining marks failed. Expected: " + Utility.Hex(expected) + " Got: " + Utility.Hex(result));
            }
        }

        /*
         * Re-enable this test when UTC fixes UAX 21
        [Test]
        public void TestUAX21Failure(){
            final string[][] cases = new string[][]{
                    {"\u0061\u0345\u0360\u0345\u0062", "\u0061\u0360\u0345\u0345\u0062"},
                    {"\u0061\u0345\u0345\u0360\u0062", "\u0061\u0360\u0345\u0345\u0062"},
                    {"\u0061\u0345\u0360\u0362\u0360\u0062", "\u0061\u0362\u0360\u0360\u0345\u0062"},
                    {"\u0061\u0360\u0345\u0360\u0362\u0062", "\u0061\u0362\u0360\u0360\u0345\u0062"},
                    {"\u0061\u0345\u0360\u0362\u0361\u0062", "\u0061\u0362\u0360\u0361\u0345\u0062"},
                    {"\u0061\u0361\u0345\u0360\u0362\u0062", "\u0061\u0362\u0361\u0360\u0345\u0062"},
            };
            for(int i = 0; i< cases.Length; i++){
                string s1 =cases[0][0];
                string s2 = cases[0][1];
                if( (Normalizer.compare(s1,s2,Normalizer.FOLD_CASE_DEFAULT ==0)//case sensitive compare
                    &&
                    (Normalizer.compare(s1,s2,Normalizer.COMPARE_IGNORE_CASE)!=0)){
                    Errln("Normalizer.compare() failed for s1: "
                            + Utility.Hex(s1) +" s2: " + Utility.Hex(s2));
                }
            }
        }
        */

        sealed class TestStruct
        {
            internal int c;
            internal string s;
            internal TestStruct(int cp, string src)
            {
                c = cp;
                s = src;
            }
        }

        [Test]
        public void TestFCNFKCClosure()
        {
            TestStruct[] tests = new TestStruct[]{
                new TestStruct( 0x00C4, "" ),
                new TestStruct( 0x00E4, "" ),
                new TestStruct( 0x037A, "\u0020\u03B9" ),
                new TestStruct( 0x03D2, "\u03C5" ),
                new TestStruct( 0x20A8, "\u0072\u0073" ) ,
                new TestStruct( 0x210B, "\u0068" ),
                new TestStruct( 0x210C, "\u0068" ),
                new TestStruct( 0x2121, "\u0074\u0065\u006C" ),
                new TestStruct( 0x2122, "\u0074\u006D" ),
                new TestStruct( 0x2128, "\u007A" ),
                new TestStruct( 0x1D5DB,"\u0068" ),
                new TestStruct( 0x1D5ED,"\u007A" ),
                new TestStruct( 0x0061, "" )
            };


            for (int i = 0; i < tests.Length; ++i)
            {
                string result = Normalizer.GetFC_NFKC_Closure(tests[i].c);
                if (!result.Equals(tests[i].s))
                {
                    Errln("getFC_NFKC_Closure(U+" + (tests[i].c).ToHexString() + ") is wrong");
                }
            }

            /* error handling */

            int length = Normalizer.GetFC_NFKC_Closure(0x5c, null);
            if (length != 0)
            {
                Errln("getFC_NFKC_Closure did not perform error handling correctly");
            }
        }
        [Test]
        public void TestBugJ2324()
        {
            /* string[] input = new string[]{
                                 //"\u30FD\u3099",
                                 "\u30FA\u309A",
                                 "\u30FB\u309A",
                                 "\u30FC\u309A",
                                 "\u30FE\u309A",
                                 "\u30FD\u309A",

             };*/
            string troublesome = "\u309A";
            for (int i = 0x3000; i < 0x3100; i++)
            {
                string input = ((char)i) + troublesome;
                try
                {
                    /*  string result =*/
                    Normalizer.Compose(input, false);
                }
                catch (IndexOutOfRangeException e)
                {
                    Errln("compose() failed for input: " + Utility.Hex(input) + " Exception: " + e.ToString());
                }
            }

        }

        const int D = 0, C = 1, KD = 2, KC = 3, FCD = 4, NONE = 5;

        private static UnicodeSet[] initSkippables(UnicodeSet[] skipSets)
        {
            skipSets[D].ApplyPattern("[[:NFD_QC=Yes:]&[:ccc=0:]]", false);
            skipSets[C].ApplyPattern("[[:NFC_QC=Yes:]&[:ccc=0:]-[:HST=LV:]]", false);
            skipSets[KD].ApplyPattern("[[:NFKD_QC=Yes:]&[:ccc=0:]]", false);
            skipSets[KC].ApplyPattern("[[:NFKC_QC=Yes:]&[:ccc=0:]-[:HST=LV:]]", false);

            // Remove from the NFC and NFKC sets all those characters that change
            // when a back-combining character is added.
            // First, get all of the back-combining characters and their combining classes.
            UnicodeSet combineBack = new UnicodeSet("[:NFC_QC=Maybe:]");
            int numCombineBack = combineBack.Count;
            int[] combineBackCharsAndCc = new int[numCombineBack * 2];
            UnicodeSetIterator iter = new UnicodeSetIterator(combineBack);
            for (int i = 0; i < numCombineBack; ++i)
            {
                iter.Next();
                int c = iter.Codepoint;
                combineBackCharsAndCc[2 * i] = c;
                combineBackCharsAndCc[2 * i + 1] = UChar.GetCombiningClass(c);
            }

            // We need not look at control codes, Han characters nor Hangul LVT syllables because they
            // do not combine forward. LV syllables are already removed.
            UnicodeSet notInteresting = new UnicodeSet("[[:C:][:Unified_Ideograph:][:HST=LVT:]]");
            UnicodeSet unsure = ((UnicodeSet)(skipSets[C].Clone())).RemoveAll(notInteresting);
            // System.out.format("unsure.size()=%d\n", unsure.size());

            // For each character about which we are unsure, see if it changes when we add
            // one of the back-combining characters.
            Normalizer2 norm2 = Normalizer2.GetNFCInstance();
            StringBuilder s = new StringBuilder();
            iter.Reset(unsure);
            while (iter.Next())
            {
                int c = iter.Codepoint;
                s.Delete(0, 0x7fffffff - 0).AppendCodePoint(c); // ICU4N: Corrected 2nd parameter of Delete
                int cLength = s.Length;
                int tccc = UChar.GetIntPropertyValue(c, UProperty.Trail_Canonical_Combining_Class);
                for (int i = 0; i < numCombineBack; ++i)
                {
                    // If c's decomposition ends with a character with non-zero combining class, then
                    // c can only change if it combines with a character with a non-zero combining class.
                    int cc2 = combineBackCharsAndCc[2 * i + 1];
                    if (tccc == 0 || cc2 != 0)
                    {
                        int c2 = combineBackCharsAndCc[2 * i];
                        s.AppendCodePoint(c2);
                        if (!norm2.IsNormalized(s))
                        {
                            // System.out.format("remove U+%04x (tccc=%d) + U+%04x (cc=%d)\n", c, tccc, c2, cc2);
                            skipSets[C].Remove(c);
                            skipSets[KC].Remove(c);
                            break;
                        }
                        s.Delete(cLength, 0x7fffffff - cLength); // ICU4N: Corrected 2nd parameter of Delete
                    }
                }
            }
            return skipSets;
        }

        private static string[] kModeStrings = {
            "D", "C", "KD", "KC"
        };

        [Test]
        public void TestSkippable()
        {
            UnicodeSet[] skipSets = new UnicodeSet[] {
                new UnicodeSet(), //NFD
                new UnicodeSet(), //NFC
                new UnicodeSet(), //NFKD
                new UnicodeSet()  //NFKC
            };
            UnicodeSet[] expectSets = new UnicodeSet[] {
                new UnicodeSet(),
                new UnicodeSet(),
                new UnicodeSet(),
                new UnicodeSet()
            };
            StringBuilder s, pattern;

            // build NF*Skippable sets from runtime data
            skipSets[D].ApplyPattern("[:NFD_Inert:]");
            skipSets[C].ApplyPattern("[:NFC_Inert:]");
            skipSets[KD].ApplyPattern("[:NFKD_Inert:]");
            skipSets[KC].ApplyPattern("[:NFKC_Inert:]");

            expectSets = initSkippables(expectSets);
            if (expectSets[D].Contains(0x0350))
            {
                Errln("expectSets[D] contains 0x0350");
            }
            for (int i = 0; i < expectSets.Length; ++i)
            {
                if (!skipSets[i].Equals(expectSets[i]))
                {
                    string ms = kModeStrings[i];
                    Errln("error: TestSkippable skipSets[" + ms + "]!=expectedSets[" + ms + "]\n");
                    // Note: This used to depend on hardcoded UnicodeSet patterns generated by
                    // Mark's unicodetools.com.ibm.text.UCD.NFSkippable, by
                    // running com.ibm.text.UCD.Main with the option NFSkippable.
                    // Since ICU 4.6/Unicode 6, we are generating the
                    // expectSets ourselves in initSkippables().

                    s = new StringBuilder();

                    s.Append("\n\nskip=       ");
                    s.Append(skipSets[i].ToPattern(true));
                    s.Append("\n\n");

                    s.Append("skip-expect=");
                    pattern = new StringBuilder(((UnicodeSet)skipSets[i].Clone()).RemoveAll(expectSets[i]).ToPattern(true));
                    s.Append(pattern);

                    pattern.Delete(0, pattern.Length - 0); // ICU4N: Corrected 2nd parameter of Delete
                    s.Append("\n\nexpect-skip=");
                    pattern = new StringBuilder(((UnicodeSet)expectSets[i].Clone()).RemoveAll(skipSets[i]).ToPattern(true));
                    s.Append(pattern);
                    s.Append("\n\n");

                    pattern.Delete(0, pattern.Length - 0); // ICU4N: Corrected 2nd parameter of Delete
                    s.Append("\n\nintersection(expect,skip)=");
                    UnicodeSet intersection = ((UnicodeSet)expectSets[i].Clone()).RetainAll(skipSets[i]);
                    pattern = new StringBuilder(intersection.ToPattern(true));
                    s.Append(pattern);
                    // Special: test coverage for append(char).
                    s.Append('\n');
                    s.Append('\n');

                    Errln(s.ToString());
                }
            }
        }

        [Test]
        public void TestBugJ2068()
        {
            string sample = "The quick brown fox jumped over the lazy dog";
            UCharacterIterator text = UCharacterIterator.GetInstance(sample);
            Normalizer norm = new Normalizer(text, NormalizerMode.NFC, 0);
            text.Index = 4;
            if (text.Current == norm.Current)
            {
                Errln("Normalizer is not cloning the UCharacterIterator");
            }
        }
        [Test]
        public void TestGetCombiningClass()
        {
            for (int i = 0; i < 0x10FFFF; i++)
            {
                int cc = UChar.GetCombiningClass(i);
                if (0xD800 <= i && i <= 0xDFFF && cc > 0)
                {
                    cc = UChar.GetCombiningClass(i);
                    Errln("CC: " + cc + " for codepoint: " + Utility.Hex(i, 8));
                }
            }
        }

        [Test]
        public void TestSerializedSet()
        {
            USerializedSet sset = new USerializedSet();
            UnicodeSet set = new UnicodeSet();
            int start, end;

            char[] serialized = {
                    (char)0x8007,  // length
                    (char)3,  // bmpLength
                    (char)0xc0, (char)0xfe, (char)0xfffc,
                    (char)1, (char)9, (char)0x10, (char)0xfffc
                };
            sset.GetSet(serialized, 0);

            // collect all sets into one for contiguous output
            int[] startEnd = new int[2];
            int count = sset.CountRanges();
            for (int j = 0; j < count; ++j)
            {
                sset.GetRange(j, startEnd);
                set.Add(startEnd[0], startEnd[1]);
            }

            // test all of these characters
            UnicodeSetIterator it = new UnicodeSetIterator(set);
            while (it.NextRange() && it.Codepoint != UnicodeSetIterator.IsString)
            {
                start = it.Codepoint;
                end = it.CodepointEnd;
                while (start <= end)
                {
                    if (!sset.Contains(start))
                    {
                        Errln("USerializedSet.contains failed for " + Utility.Hex(start, 8));
                    }
                    ++start;
                }
            }
        }

        [Test]
        public void TestReturnFailure()
        {
            char[] term = { 'r', '\u00e9', 's', 'u', 'm', '\u00e9' };
            char[] decomposed_term = new char[10 + term.Length + 2];
            int rc = Normalizer.Decompose(term, 0, term.Length, decomposed_term, 0, decomposed_term.Length, true, 0);
            int rc1 = Normalizer.Decompose(term, 0, term.Length, decomposed_term, 10, decomposed_term.Length, true, 0);
            if (rc != rc1)
            {
                Errln("Normalizer decompose did not return correct length");
            }
        }

        private sealed class TestCompositionCase
        {
            public NormalizerMode mode;
            public int options;
            public string input, expect;
            internal TestCompositionCase(NormalizerMode mode, int options, string input, string expect)
            {
                this.mode = mode;
                this.options = options;
                this.input = input;
                this.expect = expect;
            }
        }

        [Test]
        public void TestComposition()
        {
            TestCompositionCase[] cases = new TestCompositionCase[]{
            /*
             * special cases for UAX #15 bug
             * see Unicode Corrigendum #5: Normalization Idempotency
             * at http://unicode.org/versions/corrigendum5.html
             * (was Public Review Issue #29)
             */
            new TestCompositionCase(NormalizerMode.NFC, 0, "\u1100\u0300\u1161\u0327",      "\u1100\u0300\u1161\u0327"),
            new TestCompositionCase(NormalizerMode.NFC, 0, "\u1100\u0300\u1161\u0327\u11a8","\u1100\u0300\u1161\u0327\u11a8"),
            new TestCompositionCase(NormalizerMode.NFC, 0, "\uac00\u0300\u0327\u11a8",      "\uac00\u0327\u0300\u11a8"),
            new TestCompositionCase(NormalizerMode.NFC, 0, "\u0b47\u0300\u0b3e",            "\u0b47\u0300\u0b3e"),

            /* TODO: add test cases for UNORM_FCC here (j2151) */
        };

            string output;
            int i;

            for (i = 0; i < cases.Length; ++i)
            {
                output = Normalizer.Normalize(cases[i].input, cases[i].mode, cases[i].options.AsFlagsToEnum<NormalizerUnicodeVersion>());
                if (!output.Equals(cases[i].expect))
                {
                    Errln("unexpected result for case " + i);
                }
            }
        }

        [Test]
        public void TestGetDecomposition()
        {
            Normalizer2 n2 = Normalizer2.GetInstance(null, "nfc", Normalizer2Mode.ComposeContiguous);
            string decomp = n2.GetDecomposition(0x20);
            assertEquals("fcc.getDecomposition(space) failed", null, decomp);
            decomp = n2.GetDecomposition(0xe4);
            assertEquals("fcc.getDecomposition(a-umlaut) failed", "a\u0308", decomp);
            decomp = n2.GetDecomposition(0xac01);
            assertEquals("fcc.getDecomposition(Hangul syllable U+AC01) failed", "\u1100\u1161\u11a8", decomp);
        }

        [Test]
        public void TestGetRawDecomposition()
        {
            Normalizer2 n2 = Normalizer2.GetNFKCInstance();
            /*
             * Raw decompositions from NFKC data are the Unicode Decomposition_Mapping values,
             * without recursive decomposition.
             */

            string decomp = n2.GetRawDecomposition(0x20);
            assertEquals("nfkc.getRawDecomposition(space) failed", null, decomp);
            decomp = n2.GetRawDecomposition(0xe4);
            assertEquals("nfkc.getRawDecomposition(a-umlaut) failed", "a\u0308", decomp);
            /* U+1E08 LATIN CAPITAL LETTER C WITH CEDILLA AND ACUTE */
            decomp = n2.GetRawDecomposition(0x1e08);
            assertEquals("nfkc.getRawDecomposition(c-cedilla-acute) failed", "\u00c7\u0301", decomp);
            /* U+212B ANGSTROM SIGN */
            decomp = n2.GetRawDecomposition(0x212b);
            assertEquals("nfkc.getRawDecomposition(angstrom sign) failed", "\u00c5", decomp);
            decomp = n2.GetRawDecomposition(0xac00);
            assertEquals("nfkc.getRawDecomposition(Hangul syllable U+AC00) failed", "\u1100\u1161", decomp);
            /* A Hangul LVT syllable has a raw decomposition of an LV syllable + T. */
            decomp = n2.GetRawDecomposition(0xac01);
            assertEquals("nfkc.getRawDecomposition(Hangul syllable U+AC01) failed", "\uac00\u11a8", decomp);
        }

        [Test]
        public void TestCustomComp()
        {
            string[][] pairs ={
            new string[] { "\\uD801\\uE000\\uDFFE", "" },
            new string[] { "\\uD800\\uD801\\uE000\\uDFFE\\uDFFF", "\\uD7FF\\uFFFF" },
            new string[] { "\\uD800\\uD801\\uDFFE\\uDFFF", "\\uD7FF\\U000107FE\\uFFFF" },
            new string[] { "\\uE001\\U000110B9\\u0345\\u0308\\u0327", "\\uE002\\U000110B9\\u0327\\u0345" },
            new string[] { "\\uE010\\U000F0011\\uE012", "\\uE011\\uE012" },
            new string[] { "\\uE010\\U000F0011\\U000F0011\\uE012", "\\uE011\\U000F0010" },
            new string[] { "\\uE111\\u1161\\uE112\\u1162", "\\uAE4C\\u1102\\u0062\\u1162" },
            new string[] { "\\uFFF3\\uFFF7\\U00010036\\U00010077", "\\U00010037\\U00010037\\uFFF6\\U00010037" }
        };
            Normalizer2 customNorm2;
            customNorm2 =
                Normalizer2.GetInstance(
                    //BasicTest.class.getResourceAsStream("/com/ibm/icu/dev/data/testdata/testnorm.nrm"),
                    typeof(BasicTest).GetTypeInfo().Assembly.GetManifestResourceStream("ICU4N.Dev.Data.TestData.testnorm.nrm"),
                        "testnorm",
                        Normalizer2Mode.Compose);
            for (int i = 0; i < pairs.Length; ++i)
            {
                string[] pair = pairs[i];
                string input = Utility.Unescape(pair[0]);
                string expected = Utility.Unescape(pair[1]);
                string result = customNorm2.Normalize(input);
                if (!result.Equals(expected))
                {
                    Errln("custom compose Normalizer2 did not normalize input " + i + " as expected");
                }
            }
        }

        [Test]
        public void TestCustomFCC()
        {
            string[][] pairs ={
                new string[] { "\\uD801\\uE000\\uDFFE", "" },
                new string[] { "\\uD800\\uD801\\uE000\\uDFFE\\uDFFF", "\\uD7FF\\uFFFF" },
                new string[] { "\\uD800\\uD801\\uDFFE\\uDFFF", "\\uD7FF\\U000107FE\\uFFFF" },
                // The following expected result is different from CustomComp
                // because of only-contiguous composition.
                new string[] { "\\uE001\\U000110B9\\u0345\\u0308\\u0327", "\\uE001\\U000110B9\\u0327\\u0308\\u0345" },
                new string[] { "\\uE010\\U000F0011\\uE012", "\\uE011\\uE012" },
                new string[] { "\\uE010\\U000F0011\\U000F0011\\uE012", "\\uE011\\U000F0010" },
                new string[] { "\\uE111\\u1161\\uE112\\u1162", "\\uAE4C\\u1102\\u0062\\u1162" },
                new string[] { "\\uFFF3\\uFFF7\\U00010036\\U00010077", "\\U00010037\\U00010037\\uFFF6\\U00010037" }
            };
            Normalizer2 customNorm2;
            customNorm2 =
                Normalizer2.GetInstance(
                    //BasicTest.class.getResourceAsStream("/com/ibm/icu/dev/data/testdata/testnorm.nrm"),
                    typeof(BasicTest).GetTypeInfo().Assembly.GetManifestResourceStream("ICU4N.Dev.Data.TestData.testnorm.nrm"),
                        "testnorm",
                        Normalizer2Mode.ComposeContiguous);
            for (int i = 0; i < pairs.Length; ++i)
            {
                string[] pair = pairs[i];
                string input = Utility.Unescape(pair[0]);
                string expected = Utility.Unescape(pair[1]);
                string result = customNorm2.Normalize(input);
                if (!result.Equals(expected))
                {
                    Errln("custom FCC Normalizer2 did not normalize input " + i + " as expected");
                }
            }
        }

        [Test]
        public void TestCanonIterData()
        {
            // For now, just a regression test.
            Normalizer2Impl impl = Norm2AllModes.GetNFCInstance().Impl.EnsureCanonIterData();
            // U+0FB5 TIBETAN SUBJOINED LETTER SSA is the trailing character
            // in some decomposition mappings where there is a composition exclusion.
            // In fact, U+0FB5 is normalization-inert (NFC_QC=Yes, NFD_QC=Yes, ccc=0)
            // but it is not a segment starter because it occurs in a decomposition mapping.
            if (impl.IsCanonSegmentStarter(0xfb5))
            {
                Errln("isCanonSegmentStarter(U+0fb5)=true is wrong");
            }
            // For [:Segment_Starter:] to work right, not just the property function has to work right,
            // UnicodeSet also needs a correct range starts set.
            UnicodeSet segStarters = new UnicodeSet("[:Segment_Starter:]").Freeze();
            if (segStarters.Contains(0xfb5))
            {
                Errln("[:Segment_Starter:].Contains(U+0fb5)=true is wrong");
            }
            // Try characters up to Kana and miscellaneous CJK but below Han (for expediency).
            for (int c = 0; c <= 0x33ff; ++c)
            {
                bool isStarter = impl.IsCanonSegmentStarter(c);
                bool isContained = segStarters.Contains(c);
                if (isStarter != isContained)
                {
                    Errln(string.Format(
                            "discrepancy: isCanonSegmentStarter(U+%04x)=%5b != " +
                            "[:Segment_Starter:].Contains(same)",
                            c, isStarter));
                }
            }
        }

        [Test]
        public void TestFilteredNormalizer2()
        {
            Normalizer2 nfcNorm2 = Normalizer2.GetNFCInstance();
            UnicodeSet filter = new UnicodeSet("[^\u00a0-\u00ff\u0310-\u031f]");
            FilteredNormalizer2 fn2 = new FilteredNormalizer2(nfcNorm2, filter);
            int c;
            for (c = 0; c <= 0x3ff; ++c)
            {
                int expectedCC = filter.Contains(c) ? nfcNorm2.GetCombiningClass(c) : 0;
                int cc = fn2.GetCombiningClass(c);
                assertEquals(
                        "FilteredNormalizer2(NFC, ^A0-FF,310-31F).getCombiningClass(U+" + Hex(c) +
                        ")==filtered NFC.getCC()",
                        expectedCC, cc);
            }

            // More coverage.
            StringBuilder sb = new StringBuilder();
            assertEquals("filtered normalize()", "ää\u0304",
                    fn2.Normalize("a\u0308ä\u0304", sb).ToString());
            assertTrue("filtered hasBoundaryAfter()", fn2.HasBoundaryAfter('ä'));
            assertTrue("filtered isInert()", fn2.IsInert(0x0313));
        }

        [Test]
        public void TestFilteredAppend()
        {
            Normalizer2 nfcNorm2 = Normalizer2.GetNFCInstance();
            UnicodeSet filter = new UnicodeSet("[^\u00a0-\u00ff\u0310-\u031f]");
            FilteredNormalizer2 fn2 = new FilteredNormalizer2(nfcNorm2, filter);

            // Append two strings that each contain a character outside the filter set.
            StringBuilder sb = new StringBuilder("a\u0313a");
            string second = "\u0301\u0313";
            assertEquals("append()", "a\u0313á\u0313", fn2.Append(sb, second).ToString());

            // Same, and also normalize the second string.
            sb.Replace(0, 0x7fffffff - 0, "a\u0313a"); // ICU4N: Checked 2nd parameter
            assertEquals(
                "normalizeSecondAndAppend()",
                "a\u0313á\u0313", fn2.NormalizeSecondAndAppend(sb, second).ToString());

            // Normalizer2.Normalize(string) uses spanQuickCheckYes() and normalizeSecondAndAppend().
            assertEquals("normalize()", "a\u0313á\u0313", fn2.Normalize("a\u0313a\u0301\u0313"));
        }

        [Test]
        public void TestGetEasyToUseInstance()
        {
            // Test input string:
            // U+00A0 -> <noBreak> 0020
            // U+00C7 0301 = 1E08 = 0043 0327 0301
            string @in = "\u00A0\u00C7\u0301";
            Normalizer2 n2 = Normalizer2.GetNFCInstance();
            string @out = n2.Normalize(@in);
            assertEquals(
                    "getNFCInstance() did not return an NFC instance " +
                    "(normalizes to " + Prettify(@out) + ')',
                    "\u00A0\u1E08", @out);

            n2 = Normalizer2.GetNFDInstance();
            @out = n2.Normalize(@in);
            assertEquals(
                    "getNFDInstance() did not return an NFD instance " +
                    "(normalizes to " + Prettify(@out) + ')',
                    "\u00A0C\u0327\u0301", @out);

            n2 = Normalizer2.GetNFKCInstance();
            @out = n2.Normalize(@in);
            assertEquals(
                    "getNFKCInstance() did not return an NFKC instance " +
                    "(normalizes to " + Prettify(@out) + ')',
                    " \u1E08", @out);

            n2 = Normalizer2.GetNFKDInstance();
            @out = n2.Normalize(@in);
            assertEquals(
                    "getNFKDInstance() did not return an NFKD instance " +
                    "(normalizes to " + Prettify(@out) + ')',
                    " C\u0327\u0301", @out);

            n2 = Normalizer2.GetNFKCCasefoldInstance();
            @out = n2.Normalize(@in);
            assertEquals(
                    "getNFKCCasefoldInstance() did not return an NFKC_Casefold instance " +
                    "(normalizes to " + Prettify(@out) + ')',
                    " \u1E09", @out);
        }

        [Test]
        public void TestLowMappingToEmpty_D()
        {
            Normalizer2 n2 = Normalizer2.GetInstance(null, "nfkc_cf", Normalizer2Mode.Decompose);
            checkLowMappingToEmpty(n2);

            string sh = "\u00AD";
            assertFalse("soft hyphen is not normalized", n2.IsNormalized(sh));
            string result = n2.Normalize(sh);
            assertTrue("soft hyphen normalizes to empty", result == string.Empty);
            assertEquals("soft hyphen QC=No", QuickCheckResult.No, n2.QuickCheck(sh));
            assertEquals("soft hyphen spanQuickCheckYes", 0, n2.SpanQuickCheckYes(sh));

            string s = "\u00ADÄ\u00AD\u0323";
            result = n2.Normalize(s);
            assertEquals("normalize string with soft hyphens", "a\u0323\u0308", result);
        }

        [Test]
        public void TestLowMappingToEmpty_FCD()
        {
            Normalizer2 n2 = Normalizer2.GetInstance(null, "nfkc_cf", Normalizer2Mode.FCD);
            checkLowMappingToEmpty(n2);

            string sh = "\u00AD";
            assertTrue("soft hyphen is FCD", n2.IsNormalized(sh));

            string s = "\u00ADÄ\u00AD\u0323";
            string result = n2.Normalize(s);
            assertEquals("normalize string with soft hyphens", "\u00ADa\u0323\u0308", result);
        }

        private void checkLowMappingToEmpty(Normalizer2 n2)
        {
            string mapping = n2.GetDecomposition(0xad);
            assertNotNull("getDecomposition(soft hyphen)", mapping);
            assertTrue("soft hyphen maps to empty", mapping == string.Empty);
            assertFalse("soft hyphen has no boundary before", n2.HasBoundaryBefore(0xad));
            assertFalse("soft hyphen has no boundary after", n2.HasBoundaryAfter(0xad));
            assertFalse("soft hyphen is not inert", n2.IsInert(0xad));
        }

        [Test]
        public void TestNormalizeIllFormedText()
        {
            Normalizer2 nfkc_cf = Normalizer2.GetNFKCCasefoldInstance();
            // Normalization behavior for ill-formed text is not defined.
            // ICU currently treats ill-formed sequences as normalization-inert
            // and copies them unchanged.
            string src = "  A\uD800ÄA\u0308\uD900A\u0308\u00ad\u0323\uDBFFÄ\u0323," +
                    "\u00ad\uDC00\u1100\u1161가\u11A8가\u3133  \uDFFF";
            string expected = "  a\uD800ää\uD900ạ\u0308\uDBFFạ\u0308,\uDC00가각갃  \uDFFF";
            string result = nfkc_cf.Normalize(src);
            assertEquals("normalize", expected, result);
        }

        [Test]
        public void TestComposeJamoTBase()
        {
            // Algorithmic composition of Hangul syllables must not combine with JAMO_T_BASE = U+11A7
            // which is not a conjoining Jamo Trailing consonant.
            Normalizer2 nfkc = Normalizer2.GetNFKCInstance();
            string s = "\u1100\u1161\u11A7\u1100\u314F\u11A7가\u11A7";
            string expected = "가\u11A7가\u11A7가\u11A7";
            string result = nfkc.Normalize(s);
            assertEquals("normalize(LV+11A7)", expected, result);
            assertFalse("isNormalized(LV+11A7)", nfkc.IsNormalized(s));
            assertTrue("isNormalized(normalized)", nfkc.IsNormalized(result));
        }

        [Test]
        public void TestComposeBoundaryAfter()
        {
            Normalizer2 nfkc = Normalizer2.GetNFKCInstance();
            // U+02DA and U+FB2C do not have compose-boundaries-after.
            string s = "\u02DA\u0339 \uFB2C\u05B6";
            string expected = " \u0339\u030A \u05E9\u05B6\u05BC\u05C1";
            string result = nfkc.Normalize(s);
            assertEquals("nfkc", expected, result);
            assertFalse("U+02DA boundary-after", nfkc.HasBoundaryAfter(0x2DA));
            assertFalse("U+FB2C boundary-after", nfkc.HasBoundaryAfter(0xFB2C));
        }

        [Test]
        public void TestNFC()
        {
            // Coverage tests.
            Normalizer2 nfc = Normalizer2.GetNFCInstance();
            assertTrue("nfc.hasBoundaryAfter(space)", nfc.HasBoundaryAfter(' '));
            assertFalse("nfc.hasBoundaryAfter(ä)", nfc.HasBoundaryAfter('ä'));
        }

        [Test]
        public void TestNFD()
        {
            // Coverage tests.
            Normalizer2 nfd = Normalizer2.GetNFDInstance();
            assertTrue("nfd.hasBoundaryAfter(space)", nfd.HasBoundaryAfter(' '));
            assertFalse("nfd.hasBoundaryAfter(ä)", nfd.HasBoundaryAfter('ä'));
        }

        [Test]
        public void TestFCD()
        {
            // Coverage tests.
            Normalizer2 fcd = Normalizer2.GetInstance(null, "nfc", Normalizer2Mode.FCD);
            assertTrue("fcd.hasBoundaryAfter(space)", fcd.HasBoundaryAfter(' '));
            assertFalse("fcd.hasBoundaryAfter(ä)", fcd.HasBoundaryAfter('ä'));
            assertTrue("fcd.isInert(space)", fcd.IsInert(' '));
            assertFalse("fcd.isInert(ä)", fcd.IsInert('ä'));

            // This implementation method is unreachable via public API.
            FCDNormalizer2 impl = (FCDNormalizer2)fcd;
            assertEquals("fcd impl.getQuickCheck(space)", 1, impl.GetQuickCheck(' '));
            assertEquals("fcd impl.getQuickCheck(ä)", 0, impl.GetQuickCheck('ä'));
        }

        [Test]
        public void TestNoneNormalizer()
        {
            // Use the deprecated Mode Normalizer.NONE for coverage of the internal NoopNormalizer2
            // as far as its methods are reachable that way.
            assertEquals("NONE.Concatenate()", "ä\u0327",
                    Normalizer.Concatenate("ä", "\u0327", NormalizerMode.None, 0));
            assertTrue("NONE.IsNormalized()", Normalizer.IsNormalized("ä\u0327", NormalizerMode.None, 0));
        }

        [Test]
        public void TestNoopNormalizer2()
        {
            // Use the internal class directly for coverage of methods that are not publicly reachable.
            Normalizer2 noop = Norm2AllModes.NoopNormalizer2;
            assertEquals("noop.normalizeSecondAndAppend()", "ä\u0327",
                    noop.NormalizeSecondAndAppend(new StringBuilder("ä"), "\u0327").ToString());
            assertEquals("noop.getDecomposition()", null, noop.GetDecomposition('ä'));
            assertTrue("noop.hasBoundaryAfter()", noop.HasBoundaryAfter(0x0308));
            assertTrue("noop.isInert()", noop.IsInert(0x0308));
        }

        /*
         * Abstract class Normalizer2 has non-abstract methods which are overwritten by
         * its derived classes. To test these methods a derived class is defined here.
         */
        public class TestNormalizer2 : Normalizer2
        {

            public TestNormalizer2() { }

            public override StringBuffer Normalize(string src, StringBuffer dest) { return null; }

            public override StringBuffer Normalize(StringBuffer src, StringBuffer dest) { return null; }

            public override StringBuffer Normalize(char[] src, StringBuffer dest) { return null; }

            internal override StringBuilder Normalize(ICharSequence src, StringBuilder dest) { return null; }

            internal override IAppendable Normalize(string src, IAppendable dest) { return null; }
            internal override IAppendable Normalize(StringBuffer src, IAppendable dest) { return null; }
            internal override IAppendable Normalize(char[] src, IAppendable dest) { return null; }
            internal override IAppendable Normalize(ICharSequence src, IAppendable dest) { return null; }

            public override StringBuffer NormalizeSecondAndAppend(StringBuffer first, string second) { return null; }

            public override StringBuffer NormalizeSecondAndAppend(StringBuffer first, StringBuffer second) { return null; }

            public override StringBuffer NormalizeSecondAndAppend(StringBuffer first, char[] second) { return null; }

            internal override StringBuilder NormalizeSecondAndAppend(StringBuilder first, ICharSequence second) { return null; }

            public override StringBuffer Append(StringBuffer first, string second) { return null; }

            public override StringBuffer Append(StringBuffer first, StringBuffer second) { return null; }

            public override StringBuffer Append(StringBuffer first, char[] second) { return null; }

            internal override StringBuilder Append(StringBuilder first, ICharSequence second) { return null; }

            public override string GetDecomposition(int c) { return null; }

            public override bool IsNormalized(string s) { return false; }

            public override bool IsNormalized(StringBuffer s) { return false; }

            public override bool IsNormalized(char[] s) { return false; }

            internal override bool IsNormalized(ICharSequence s) { return false; }

            public override QuickCheckResult QuickCheck(string s) { return (QuickCheckResult)(-1); }

            public override QuickCheckResult QuickCheck(StringBuffer s) { return (QuickCheckResult)(-1); }

            public override QuickCheckResult QuickCheck(char[] s) { return (QuickCheckResult)(-1); }

            internal override QuickCheckResult QuickCheck(ICharSequence s) { return (QuickCheckResult)(-1); }

            public override int SpanQuickCheckYes(string s) { return 0; }

            public override int SpanQuickCheckYes(StringBuffer s) { return 0; }

            public override int SpanQuickCheckYes(char[] s) { return 0; }

            internal override int SpanQuickCheckYes(ICharSequence s) { return 0; }

            public override bool HasBoundaryBefore(int c) { return false; }

            public override bool HasBoundaryAfter(int c) { return false; }

            public override bool IsInert(int c) { return false; }
        }

        TestNormalizer2 tnorm2 = new TestNormalizer2();
        [Test]
        public void TestGetRawDecompositionBase()
        {
            int c = 'à';
            assertEquals("Unexpected value returned from Normalizer2.getRawDecomposition()",
                         null, tnorm2.GetRawDecomposition(c));
        }

        [Test]
        public void TestComposePairBase()
        {
            int a = 'a';
            int b = '\u0300';
            assertEquals("Unexpected value returned from Normalizer2.composePair()",
                         -1, tnorm2.ComposePair(a, b));
        }

        [Test]
        public void TestGetCombiningClassBase()
        {
            int c = '\u00e0';
            assertEquals("Unexpected value returned from Normalizer2.getCombiningClass()",
                         0, tnorm2.GetCombiningClass(c));
        }
    }
}
