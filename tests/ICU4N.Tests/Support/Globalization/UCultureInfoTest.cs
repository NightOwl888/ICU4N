using ICU4N.Dev.Test;
using ICU4N.Text;
using J2N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ICU4N.Globalization
{
    public class UCultureInfoTest : TestFmwk
    {
        // ================= Infrastructure =================

        /**
         * Compare two locale IDs.  If they are equal, return 0.  If `string'
         * starts with `prefix' plus an additional element, that is, string ==
         * prefix + '_' + x, then return 1.  Otherwise return a value &lt; 0.
         */
        internal static int loccmp(String str, String prefix)
        {
            int slen = str.Length,
                    plen = prefix.Length;
            /* 'root' is "less than" everything */
            if (prefix.Equals("root"))
            {
                return str.Equals("root") ? 0 : 1;
            }
            // ON JAVA (only -- not on C -- someone correct me if I'm wrong)
            // consider "" to be an alternate name for "root".
            if (plen == 0)
            {
                return slen == 0 ? 0 : 1;
            }
            if (!str.StartsWith(prefix, StringComparison.Ordinal)) return -1; /* mismatch */
            if (slen == plen) return 0;
            if (str[plen] == '_') return 1;
            return -2; /* false match, e.g. "en_USX" cmp "en_US" */
        }

        /**
         * Check the relationship between requested locales, and report problems.
         * The caller specifies the expected relationships between requested
         * and valid (expReqValid) and between valid and actual (expValidActual).
         * Possible values are:
         * "gt" strictly greater than, e.g., en_US > en
         * "ge" greater or equal,      e.g., en >= en
         * "eq" equal,                 e.g., en == en
         */
        internal void checklocs(String label,
                String req,
                CultureInfo validLoc,
                CultureInfo actualLoc,
                String expReqValid,
                String expValidActual)
        {
            String valid = validLoc.ToString();
            String actual = actualLoc.ToString();
            int reqValid = loccmp(req, valid);
            int validActual = loccmp(valid, actual);
            bool reqOK = (expReqValid.Equals("gt") && reqValid > 0) ||
                    (expReqValid.Equals("ge") && reqValid >= 0) ||
                    (expReqValid.Equals("eq") && reqValid == 0);
            bool valOK = (expValidActual.Equals("gt") && validActual > 0) ||
                    (expValidActual.Equals("ge") && validActual >= 0) ||
                    (expValidActual.Equals("eq") && validActual == 0);
            if (reqOK && valOK)
            {
                Logln("Ok: " + label + "; req=" + req + ", valid=" + valid +
                        ", actual=" + actual);
            }
            else
            {
                Errln("FAIL: " + label + "; req=" + req + ", valid=" + valid +
                        ", actual=" + actual +
                        (reqOK ? "" : "\n  req !" + expReqValid + " valid") +
                        (valOK ? "" : "\n  val !" + expValidActual + " actual"));
            }
        }

        /**
         * Interface used by checkService defining a protocol to create an
         * object, given a requested locale.
         */
        internal interface IServiceFacade
        {
            object Create(UCultureInfo requestedLocale);
        }

        /**
         * Interface used by checkService defining a protocol to get a
         * contained subobject, given its parent object.
         */
        internal interface ISubObject
        {
            object Get(object parent);
        }

        /**
         * Interface used by checkService defining a protocol to register
         * and unregister a service object prototype.
         */
        internal interface IRegistrar
        {
            object Register(UCultureInfo loc, object prototype);
            bool Unregister(object key);
        }

        /////**
        //// * Use reflection to call getLocale() on the given object to
        //// * determine both the valid and the actual locale.  Verify these
        //// * for correctness.
        //// */
        ////internal void checkObject(String requestedLocale, Object obj,
        ////        String expReqValid, String expValidActual)
        ////{
        ////    Type[] getLocaleParams = new Type[] { typeof(ULocale.Type) };
        ////    try
        ////    {
        ////        Type cls = obj.GetType();
        ////        MethodInfo getLocale = cls.GetMethod("GetLocale", getLocaleParams); // ICU4N TODO: API - it would probably make sense to name this similarly to .NET so we can reuse this
        ////        ULocale valid = (ULocale)getLocale.Invoke(obj, new Object[] {
        ////            ULocale.VALID_LOCALE });
        ////        ULocale actual = (ULocale)getLocale.Invoke(obj, new Object[] {
        ////            ULocale.ACTUAL_LOCALE });
        ////        checklocs(cls.Name, requestedLocale,
        ////                valid.ToLocale(), actual.ToLocale(),
        ////                expReqValid, expValidActual);
        ////    }

        ////    // Make the following exceptions _specific_ -- do not
        ////    // catch(Exception), since that will catch the exception
        ////    // that Errln throws.
        ////    catch (MissingMethodException e1)
        ////    {
        ////        // no longer an error, Currency has no getLocale
        ////        // Errln("FAIL: reflection failed: " + e1);
        ////    }
        ////    catch (MethodAccessException e2)
        ////    {
        ////        Errln("FAIL: reflection failed: " + e2);
        ////    }
        ////    catch (InvalidOperationException e3)
        ////    {
        ////        Errln("FAIL: reflection failed: " + e3);
        ////    }
        ////    catch (ArgumentException e4)
        ////    {
        ////        Errln("FAIL: reflection failed: " + e4);
        ////    }
        ////    catch (TargetInvocationException e5)
        ////    {
        ////        // no longer an error, Currency has no getLocale
        ////        // Errln("FAIL: reflection failed: " + e5);
        ////    }
        ////}

        /////**
        //// * Verify the correct getLocale() behavior for the given service.
        //// * @param requestedLocale the locale to request.  This MUST BE
        //// * FAKE.  In other words, it should be something like
        //// * en_US_FAKEVARIANT so this method can verify correct fallback
        //// * behavior.
        //// * @param svc a factory object that can create the object to be
        //// * tested.  This isn't necessary here (one could just pass in the
        //// * object) but is required for the overload of this method that
        //// * takes a Registrar.
        //// */
        ////internal void checkService(String requestedLocale, IServiceFacade svc)
        ////{
        ////    checkService(requestedLocale, svc, null, null);
        ////}

        /////**
        //// * Verify the correct getLocale() behavior for the given service.
        //// * @param requestedLocale the locale to request.  This MUST BE
        //// * FAKE.  In other words, it should be something like
        //// * en_US_FAKEVARIANT so this method can verify correct fallback
        //// * behavior.
        //// * @param svc a factory object that can create the object to be
        //// * tested.
        //// * @param sub an object that can be used to retrieve a subobject
        //// * which should also be tested.  May be null.
        //// * @param reg an object that supplies the registration and
        //// * unregistration functionality to be tested.  May be null.
        //// */
        ////internal void checkService(String requestedLocale, IServiceFacade svc,
        ////        ISubObject sub, IRegistrar reg)
        ////{
        ////    UCultureInfo req = new UCultureInfo(requestedLocale);
        ////    Object obj = svc.Create(req);
        ////    checkObject(requestedLocale, obj, "gt", "ge");
        ////    if (sub != null)
        ////    {
        ////        Object subobj = sub.Get(obj);
        ////        checkObject(requestedLocale, subobj, "gt", "ge");
        ////    }
        ////    if (reg != null)
        ////    {
        ////        Logln("Info: Registering service");
        ////        Object key = reg.Register(req, obj);
        ////        Object objReg = svc.Create(req);
        ////        checkObject(requestedLocale, objReg, "eq", "eq");
        ////        if (sub != null)
        ////        {
        ////            Object subobj = sub.Get(obj);
        ////            // Assume subobjects don't come from services, so
        ////            // their metadata should be structured normally.
        ////            checkObject(requestedLocale, subobj, "gt", "ge");
        ////        }
        ////        Logln("Info: Unregistering service");
        ////        if (!reg.Unregister(key))
        ////        {
        ////            Errln("FAIL: unregister failed");
        ////        }
        ////        Object objUnreg = svc.Create(req);
        ////        checkObject(requestedLocale, objUnreg, "gt", "ge");
        ////    }
        ////}
        private const int LOCALE_SIZE = 9;
        private static readonly string[][] rawData2 = new string[][]{
                /* language code */
                new string[]{   "en",   "fr",   "ca",   "el",   "no",   "zh",   "de",   "es",  "ja"    },
                /* script code */
                new string[]{   "",     "",     "",     "",     "",     "Hans", "", "", ""  },
                /* country code */
                new string[]{   "US",   "FR",   "ES",   "GR",   "NO",   "CN", "DE", "", "JP"    },
                /* variant code */
                new string[]{   "",     "",     "",     "",     "NY",   "", "", "", ""      },
                /* full name */
                new string[]{   "en_US",    "fr_FR",    "ca_ES",
                    "el_GR",    "no_NO_NY", "zh_Hans_CN",
                    "de_DE@collation=phonebook", "es@collation=traditional",  "ja_JP@calendar=japanese" },
                /* ISO-3 language */
                new string[]{   "eng",  "fra",  "cat",  "ell",  "nor",  "zho", "deu", "spa", "jpn"   },
                /* ISO-3 country */
                new string[]{   "USA",  "FRA",  "ESP",  "GRC",  "NOR",  "CHN", "DEU", "", "JPN"   },
                /* LCID */
                new string[]{   "409", "40c", "403", "408", "814",  "804", "407", "a", "411"     },

                /* display language (English) */
                new string[]{   "English",  "French",   "Catalan", "Greek",    "Norwegian", "Chinese", "German", "Spanish", "Japanese"    },
                /* display script code (English) */
                new string[]{   "",     "",     "",     "",     "",     "Simplified Han", "", "", ""       },
                /* display country (English) */
                new string[]{   "United States",    "France",   "Spain",  "Greece",   "Norway", "China", "Germany", "", "Japan"       },
                /* display variant (English) */
                new string[]{   "",     "",     "",     "",     "NY",  "", "", "", ""       },
                /* display name (English) */
                new string[]{   "English (United States)", "French (France)", "Catalan (Spain)",
                    "Greek (Greece)", "Norwegian (Norway, NY)", "Chinese (Simplified Han, China)",
                    "German (Germany, Collation=Phonebook Sort Order)", "Spanish (Collation=Traditional)", "Japanese (Japan, Calendar=Japanese Calendar)" },

                /* display language (French) */
                new string[]{   "anglais",  "fran\\u00E7ais",   "catalan", "grec",    "norv\\u00E9gien",    "chinois", "allemand", "espagnol", "japonais"     },
                /* display script code (French) */
                new string[]{   "",     "",     "",     "",     "",     "Hans", "", "", ""         },
                /* display country (French) */
                new string[]{   "\\u00C9tats-Unis",    "France",   "Espagne",  "Gr\\u00E8ce",   "Norv\\u00E8ge",    "Chine", "Allemagne", "", "Japon"       },
                /* display variant (French) */
                new string[]{   "",     "",     "",     "",     "NY",   "", "", "", ""       },
                /* display name (French) */
                new string[]{   "anglais (\\u00C9tats-Unis)", "fran\\u00E7ais (France)", "catalan (Espagne)",
                    "grec (Gr\\u00E8ce)", "norv\\u00E9gien (Norv\\u00E8ge, NY)",  "chinois (Hans, Chine)",
                    "allemand (Allemagne, Ordonnancement=Ordre de l'annuaire)", "espagnol (Ordonnancement=Ordre traditionnel)", "japonais (Japon, Calendrier=Calendrier japonais)" },

                /* display language (Catalan) */
                new string[]{   "angl\\u00E8s", "franc\\u00E8s", "catal\\u00E0", "grec",  "noruec", "xin\\u00E9s", "alemany", "espanyol", "japon\\u00E8s"    },
                /* display script code (Catalan) */
                new string[]{   "",     "",     "",     "",     "",     "Hans", "", "", ""         },
                /* display country (Catalan) */
                new string[]{   "Estats Units", "Fran\\u00E7a", "Espanya",  "Gr\\u00E8cia", "Noruega",  "Xina", "Alemanya", "", "Jap\\u00F3"    },
                /* display variant (Catalan) */
                new string[]{   "", "", "",                    "", "NY",    "", "", "", ""    },
                /* display name (Catalan) */
                new string[]{   "angl\\u00E8s (Estats Units)", "franc\\u00E8s (Fran\\u00E7a)", "catal\\u00E0 (Espanya)",
                    "grec (Gr\\u00E8cia)", "noruec (Noruega, NY)", "xin\\u00E9s (Hans, Xina)",
                    "alemany (Alemanya, COLLATION=PHONEBOOK)", "espanyol (COLLATION=TRADITIONAL)", "japon\\u00E8s (Jap\\u00F3, CALENDAR=JAPANESE)" },

                /* display language (Greek) */
                new string[]{
                    "\\u0391\\u03b3\\u03b3\\u03bb\\u03b9\\u03ba\\u03ac",
                    "\\u0393\\u03b1\\u03bb\\u03bb\\u03b9\\u03ba\\u03ac",
                    "\\u039a\\u03b1\\u03c4\\u03b1\\u03bb\\u03b1\\u03bd\\u03b9\\u03ba\\u03ac",
                    "\\u0395\\u03bb\\u03bb\\u03b7\\u03bd\\u03b9\\u03ba\\u03ac",
                    "\\u039d\\u03bf\\u03c1\\u03b2\\u03b7\\u03b3\\u03b9\\u03ba\\u03ac",
                    "\\u039A\\u03B9\\u03BD\\u03B5\\u03B6\\u03B9\\u03BA\\u03AC",
                    "\\u0393\\u03B5\\u03C1\\u03BC\\u03B1\\u03BD\\u03B9\\u03BA\\u03AC",
                    "\\u0399\\u03C3\\u03C0\\u03B1\\u03BD\\u03B9\\u03BA\\u03AC",
                    "\\u0399\\u03B1\\u03C0\\u03C9\\u03BD\\u03B9\\u03BA\\u03AC"
                },
                /* display script code (Greek) */
                new string[]{   "",     "",     "",     "",     "",     "Hans", "", "", ""         },
                /* display country (Greek) */
                new string[]{
                    "\\u0397\\u03bd\\u03c9\\u03bc\\u03ad\\u03bd\\u03b5\\u03c2 \\u03a0\\u03bf\\u03bb\\u03b9\\u03c4\\u03b5\\u03af\\u03b5\\u03c2",
                    "\\u0393\\u03b1\\u03bb\\u03bb\\u03af\\u03b1",
                    "\\u0399\\u03c3\\u03c0\\u03b1\\u03bd\\u03af\\u03b1",
                    "\\u0395\\u03bb\\u03bb\\u03ac\\u03b4\\u03b1",
                    "\\u039d\\u03bf\\u03c1\\u03b2\\u03b7\\u03b3\\u03af\\u03b1",
                    "\\u039A\\u03AF\\u03BD\\u03B1",
                    "\\u0393\\u03B5\\u03C1\\u03BC\\u03B1\\u03BD\\u03AF\\u03B1",
                    "",
                    "\\u0399\\u03B1\\u03C0\\u03C9\\u03BD\\u03AF\\u03B1"
                },
                /* display variant (Greek) */
                new string[]{   "", "", "", "", "NY", "", "", "", ""    }, /* TODO: currently there is no translation for NY in Greek fix this test when we have it */
                /* display name (Greek) */
                new string[]{
                    "\\u0391\\u03b3\\u03b3\\u03bb\\u03b9\\u03ba\\u03ac (\\u0397\\u03bd\\u03c9\\u03bc\\u03ad\\u03bd\\u03b5\\u03c2 \\u03a0\\u03bf\\u03bb\\u03b9\\u03c4\\u03b5\\u03af\\u03b5\\u03c2)",
                    "\\u0393\\u03b1\\u03bb\\u03bb\\u03b9\\u03ba\\u03ac (\\u0393\\u03b1\\u03bb\\u03bb\\u03af\\u03b1)",
                    "\\u039a\\u03b1\\u03c4\\u03b1\\u03bb\\u03b1\\u03bd\\u03b9\\u03ba\\u03ac (\\u0399\\u03c3\\u03c0\\u03b1\\u03bd\\u03af\\u03b1)",
                    "\\u0395\\u03bb\\u03bb\\u03b7\\u03bd\\u03b9\\u03ba\\u03ac (\\u0395\\u03bb\\u03bb\\u03ac\\u03b4\\u03b1)",
                    "\\u039d\\u03bf\\u03c1\\u03b2\\u03b7\\u03b3\\u03b9\\u03ba\\u03ac (\\u039d\\u03bf\\u03c1\\u03b2\\u03b7\\u03b3\\u03af\\u03b1, NY)",
                    "\\u039A\\u03B9\\u03BD\\u03B5\\u03B6\\u03B9\\u03BA\\u03AC (Hans, \\u039A\\u03AF\\u03BD\\u03B1)",
                    "\\u0393\\u03B5\\u03C1\\u03BC\\u03B1\\u03BD\\u03B9\\u03BA\\u03AC (\\u0393\\u03B5\\u03C1\\u03BC\\u03B1\\u03BD\\u03AF\\u03B1, COLLATION=PHONEBOOK)",
                    "\\u0399\\u03C3\\u03C0\\u03B1\\u03BD\\u03B9\\u03BA\\u03AC (COLLATION=TRADITIONAL)",
                    "\\u0399\\u03B1\\u03C0\\u03C9\\u03BD\\u03B9\\u03BA\\u03AC (\\u0399\\u03B1\\u03C0\\u03C9\\u03BD\\u03AF\\u03B1, CALENDAR=JAPANESE)"
                }
        };
        //    private const int ENGLISH = 0;
        //    private const int FRENCH = 1;
        //    private const int CATALAN = 2;
        //    private const int GREEK = 3;
        //    private const int NORWEGIAN = 4;
        private const int LANG = 0;
        private const int SCRIPT = 1;
        private const int CTRY = 2;
        private const int VAR = 3;
        private const int NAME = 4;
        //    private const int LANG3 = 5;
        //    private const int CTRY3 = 6;
        //    private const int LCID = 7;
        //    private const int DLANG_EN = 8;
        //    private const int DSCRIPT_EN = 9;
        //    private const int DCTRY_EN = 10;
        //    private const int DVAR_EN = 11;
        //    private const int DNAME_EN = 12;
        //    private const int DLANG_FR = 13;
        //    private const int DSCRIPT_FR = 14;
        //    private const int DCTRY_FR = 15;
        //    private const int DVAR_FR = 16;
        //    private const int DNAME_FR = 17;
        //    private const int DLANG_CA = 18;
        //    private const int DSCRIPT_CA = 19;
        //    private const int DCTRY_CA = 20;
        //    private const int DVAR_CA = 21;
        //    private const int DNAME_CA = 22;
        //    private const int DLANG_EL = 23;
        //    private const int DSCRIPT_EL = 24;
        //    private const int DCTRY_EL = 25;
        //    private const int DVAR_EL = 26;
        //    private const int DNAME_EL = 27;



        [Test]
        public void TestBasicGetters()
        {
            int i;
            Logln("Testing Basic Getters\n");
            for (i = 0; i < LOCALE_SIZE; i++)
            {
                String testLocale = (rawData2[NAME][i]);
                Logln("Testing " + testLocale + ".....\n");

                String lang = UCultureInfo.GetLanguage(testLocale);
                if (0 != lang.CompareToOrdinal(rawData2[LANG][i]))
                {
                    Errln("  Language code mismatch: " + lang + " versus " + rawData2[LANG][i]);
                }

                String ctry = UCultureInfo.GetCountry(testLocale);
                if (0 != ctry.CompareToOrdinal(rawData2[CTRY][i]))
                {
                    Errln("  Country code mismatch: " + ctry + " versus " + rawData2[CTRY][i]);
                }

                String var = UCultureInfo.GetVariant(testLocale);
                if (0 != var.CompareToOrdinal(rawData2[VAR][i]))
                {
                    Errln("  Variant code mismatch: " + var + " versus " + rawData2[VAR][i]);
                }

                String name = UCultureInfo.GetFullName(testLocale);
                if (0 != name.CompareToOrdinal(rawData2[NAME][i]))
                {
                    Errln("  Name mismatch: " + name + " versus " + rawData2[NAME][i]);
                }

            }
        }

        [Test]
        public void TestPrefixes()
        {
            // POSIX ids are no longer handled by getName, so POSIX failures are ignored
            string[][] testData = new string[][]{
                    /* null canonicalize() column means "expect same as getName()" */
                    new string[]{"sv", "", "FI", "AL", "sv-fi-al", "sv_FI_AL", null},
                    new string[]{"en", "", "GB", "", "en-gb", "en_GB", null},
                    new string[]{"i-hakka", "", "MT", "XEMXIJA", "i-hakka_MT_XEMXIJA", "i-hakka_MT_XEMXIJA", null},
                    new string[]{"i-hakka", "", "CN", "", "i-hakka_CN", "i-hakka_CN", null},
                    new string[]{"i-hakka", "", "MX", "", "I-hakka_MX", "i-hakka_MX", null},
                    new string[]{"x-klingon", "", "US", "SANJOSE", "X-KLINGON_us_SANJOSE", "x-klingon_US_SANJOSE", null},

                    new string[]{"de", "", "", "1901", "de-1901", "de__1901", null},
                    new string[]{"mr", "", "", "", "mr.utf8", "mr.utf8", "mr"},
                    new string[]{"de", "", "TV", "", "de-tv.koi8r", "de_TV.koi8r", "de_TV"},
                    new string[]{"x-piglatin", "", "ML", "", "x-piglatin_ML.MBE", "x-piglatin_ML.MBE", "x-piglatin_ML"},  /* Multibyte English */
                    new string[]{"i-cherokee", "","US", "", "i-Cherokee_US.utf7", "i-cherokee_US.utf7", "i-cherokee_US"},
                    new string[]{"x-filfli", "", "MT", "FILFLA", "x-filfli_MT_FILFLA.gb-18030", "x-filfli_MT_FILFLA.gb-18030", "x-filfli_MT_FILFLA"},
                    new string[]{"no", "", "NO", "NY_B", "no-no-ny.utf32@B", "no_NO_NY.utf32@B", "no_NO_NY_B"},
                    new string[]{"no", "", "NO", "B",  "no-no.utf32@B", "no_NO.utf32@B", "no_NO_B"},
                    new string[]{"no", "", "",   "NY", "no__ny", "no__NY", null},
                    new string[]{"no", "", "",   "NY", "no@ny", "no@ny", "no__NY"},
                    new string[]{"el", "Latn", "", "", "el-latn", "el_Latn", null},
                    new string[]{"en", "Cyrl", "RU", "", "en-cyrl-ru", "en_Cyrl_RU", null},
                    new string[]{"zh", "Hant", "TW", "STROKE", "zh-hant_TW_STROKE", "zh_Hant_TW_STROKE", "zh_Hant_TW@collation=stroke"},
                    new string[]{"zh", "Hant", "CN", "STROKE", "zh-hant_CN_STROKE", "zh_Hant_CN_STROKE", "zh_Hant_CN@collation=stroke"},
                    new string[]{"zh", "Hant", "TW", "PINYIN", "zh-hant_TW_PINYIN", "zh_Hant_TW_PINYIN", "zh_Hant_TW@collation=pinyin"},
                    new string[]{"qq", "Qqqq", "QQ", "QQ", "qq_Qqqq_QQ_QQ", "qq_Qqqq_QQ_QQ", null},
                    new string[]{"qq", "Qqqq", "", "QQ", "qq_Qqqq__QQ", "qq_Qqqq__QQ", null},
                    new string[]{"ab", "Cdef", "GH", "IJ", "ab_cdef_gh_ij", "ab_Cdef_GH_IJ", null}, /* total garbage */

                    // odd cases
                    new string[]{"", "", "", "", "@FOO=bar", "@foo=bar", null},
                    new string[]{"", "", "", "", "_@FOO=bar", "@foo=bar", null},
                    new string[]{"", "", "", "", "__@FOO=bar", "@foo=bar", null},
                    new string[]{"", "", "", "FOO", "__foo@FOO=bar", "__FOO@foo=bar", null}, // we have some of these prefixes
            };

            string loc, buf, buf1;
            string[] testTitles = {
                    "ULocale.getLanguage()",
                    "ULocale.getScript()",
                    "ULocale.getCountry()",
                    "ULocale.getVariant()",
                    "name",
                    "ULocale.GetFullName()",
                    "canonicalize()",
            };
            UCultureInfo uloc;

            for (int row = 0; row < testData.Length; row++)
            {
                loc = testData[row][NAME];
                Logln("Test #" + row + ": " + loc);

                uloc = new UCultureInfo(loc);

                for (int n = 0; n <= (NAME + 2); n++)
                {
                    if (n == NAME) continue;

                    switch (n)
                    {
                        case LANG:
                            buf = UCultureInfo.GetLanguage(loc);
                            buf1 = uloc.Language;
                            break;

                        case SCRIPT:
                            buf = UCultureInfo.GetScript(loc);
                            buf1 = uloc.Script;
                            break;

                        case CTRY:
                            buf = UCultureInfo.GetCountry(loc);
                            buf1 = uloc.Country;
                            break;

                        case VAR:
                            buf = UCultureInfo.GetVariant(loc);
                            buf1 = buf;
                            break;

                        case NAME + 1:
                            buf = UCultureInfo.GetFullName(loc);
                            buf1 = uloc.FullName;
                            break;

                        case NAME + 2:
                            buf = UCultureInfo.Canonicalize(loc);
                            buf1 = UCultureInfo.CreateCanonical(loc).FullName;
                            break;

                        default:
                            buf = "**??";
                            buf1 = buf;
                            break;
                    }

                    Logln("#" + row + ": " + testTitles[n] + " on " + loc + ": -> [" + buf + "]");

                    string expected = testData[row][n];
                    if (expected == null && n == (NAME + 2))
                    {
                        expected = testData[row][NAME + 1];
                    }

                    // ignore POSIX failures in getName, we don't spec behavior in this case
                    if (n == NAME + 1 &&
                            (expected.IndexOf('.') != -1 ||
                            expected.IndexOf('@') != -1))
                    {
                        continue;
                    }

                    if (buf.CompareToOrdinal(expected) != 0)
                    {
                        Errln("#" + row + ": " + testTitles[n] + " on " + loc + ": -> [" + buf + "] (expected '" + expected + "'!)");
                    }
                    if (buf1.CompareToOrdinal(expected) != 0)
                    {
                        Errln("#" + row + ": " + testTitles[n] + " on UCultureInfo object " + loc + ": -> [" + buf1 + "] (expected '" + expected + "'!)");
                    }
                }
            }
        }

        [Test]
        public void TestUldnWithGarbage()
        {
            LocaleDisplayNames ldn = LocaleDisplayNames.GetInstance(new CultureInfo("en-US"), DisplayContext.DialectNames);
            String badLocaleID = "english (United States) [w";
            String expectedResult = "english [united states] [w"; // case changed from input
            String result = ldn.LocaleDisplayName(badLocaleID);
            if (result.CompareToOrdinal(expectedResult) != 0)
            {
                Errln("FAIL: LocaleDisplayNames.localeDisplayName(String) for bad locale ID \"" + badLocaleID + "\", expected \"" + expectedResult + "\", got \"" + result + "\"");
            }
            UCultureInfo badLocale = new UCultureInfo(badLocaleID);
            result = ldn.LocaleDisplayName(badLocale);
            if (result.CompareToOrdinal(expectedResult) != 0)
            {
                Errln("FAIL: LocaleDisplayNames.localeDisplayName(ULocale) for bad locale ID \"" + badLocaleID + "\", expected \"" + expectedResult + "\", got \"" + result + "\"");
            }
        }

        // ICU4N TODO: Finish implementation
        //[Test]
        //public void TestObsoleteNames()
        //{
        //    string[][] tests = new string[][]{
        //            /* locale, language3, language2, Country3, country2 */
        //            new string[]{ "eng_USA", "eng", "en", "USA", "US" },
        //            new string[]{ "kok",  "kok", "kok", "", "" },
        //            new string[]{ "in",  "ind", "in", "", "" },
        //            new string[]{ "id",  "ind", "id", "", "" }, /* NO aliasing */
        //            new string[]{ "sh",  "srp", "sh", "", "" },
        //            new string[]{ "zz_CS",  "", "zz", "SCG", "CS" },
        //            new string[]{ "zz_FX",  "", "zz", "FXX", "FX" },
        //            new string[]{ "zz_RO",  "", "zz", "ROU", "RO" },
        //            new string[]{ "zz_TP",  "", "zz", "TMP", "TP" },
        //            new string[]{ "zz_TL",  "", "zz", "TLS", "TL" },
        //            new string[]{ "zz_ZR",  "", "zz", "ZAR", "ZR" },
        //            new string[]{ "zz_FXX",  "", "zz", "FXX", "FX" }, /* no aliasing. Doesn't go to PS(PSE). */
        //            new string[]{ "zz_ROM",  "", "zz", "ROU", "RO" },
        //            new string[]{ "zz_ROU",  "", "zz", "ROU", "RO" },
        //            new string[]{ "zz_ZAR",  "", "zz", "ZAR", "ZR" },
        //            new string[]{ "zz_TMP",  "", "zz", "TMP", "TP" },
        //            new string[]{ "zz_TLS",  "", "zz", "TLS", "TL" },
        //            new string[]{ "zz_YUG",  "", "zz", "YUG", "YU" },
        //            new string[]{ "mlt_PSE", "mlt", "mt", "PSE", "PS" },
        //            new string[]{ "iw", "heb", "iw", "", "" },
        //            new string[]{ "ji", "yid", "ji", "", "" },
        //            new string[]{ "jw", "jaw", "jw", "", "" },
        //            new string[]{ "sh", "srp", "sh", "", "" },
        //            new string[]{ "", "", "", "", "" }
        //    };

        //    for (int i = 0; i < tests.Length; i++)
        //    {
        //        String locale = tests[i][0];
        //        Logln("** Testing : " + locale);
        //        String buff, buff1;
        //        UCultureInfo uloc = new UCultureInfo(locale);

        //        buff = UCultureInfo.GetISO3Language(locale);
        //        if (buff.CompareToOrdinal(tests[i][1]) != 0)
        //        {
        //            Errln("FAIL: ULocale.getISO3Language(" + locale + ")==" +
        //                    buff + ",\t expected " + tests[i][1]);
        //        }
        //        else
        //        {
        //            Logln("   ULocale.getISO3Language(" + locale + ")==" + buff);
        //        }

        //        buff1 = uloc.GetISO3Language();
        //        if (buff1.CompareToOrdinal(tests[i][1]) != 0)
        //        {
        //            Errln("FAIL: ULocale.getISO3Language(" + locale + ")==" +
        //                    buff + ",\t expected " + tests[i][1]);
        //        }
        //        else
        //        {
        //            Logln("   ULocale.getISO3Language(" + locale + ")==" + buff);
        //        }

        //        buff = UCultureInfo.GetLanguage(locale);
        //        if (buff.CompareToOrdinal(tests[i][2]) != 0)
        //        {
        //            Errln("FAIL: ULocale.getLanguage(" + locale + ")==" +
        //                    buff + ",\t expected " + tests[i][2]);
        //        }
        //        else
        //        {
        //            Logln("   ULocale.getLanguage(" + locale + ")==" + buff);
        //        }

        //        buff = UCultureInfo.GetISO3Country(locale);
        //        if (buff.CompareToOrdinal(tests[i][3]) != 0)
        //        {
        //            Errln("FAIL: ULocale.getISO3Country(" + locale + ")==" +
        //                    buff + ",\t expected " + tests[i][3]);
        //        }
        //        else
        //        {
        //            Logln("   ULocale.getISO3Country(" + locale + ")==" + buff);
        //        }

        //        buff1 = uloc.GetISO3Country();
        //        if (buff1.CompareToOrdinal(tests[i][3]) != 0)
        //        {
        //            Errln("FAIL: ULocale.getISO3Country(" + locale + ")==" +
        //                    buff + ",\t expected " + tests[i][3]);
        //        }
        //        else
        //        {
        //            Logln("   ULocale.getISO3Country(" + locale + ")==" + buff);
        //        }

        //        buff = UCultureInfo.GetCountry(locale);
        //        if (buff.CompareToOrdinal(tests[i][4]) != 0)
        //        {
        //            Errln("FAIL: ULocale.getCountry(" + locale + ")==" +
        //                    buff + ",\t expected " + tests[i][4]);
        //        }
        //        else
        //        {
        //            Logln("   ULocale.getCountry(" + locale + ")==" + buff);
        //        }
        //    }

        //    if (UCultureInfo.GetLanguage("iw_IL").CompareToOrdinal(UCultureInfo.GetLanguage("he_IL")) == 0)
        //    {
        //        Errln("he,iw ULocale.getLanguage mismatch");
        //    }

        //    String buff2 = UCultureInfo.GetLanguage("kok_IN");
        //    if (buff2.CompareToOrdinal("kok") != 0)
        //    {
        //        Errln("ULocale.getLanguage(\"kok\") failed. Expected: kok Got: " + buff2);
        //    }
        //}

        [Test]
        public void TestCanonicalization()
        {
            string[][] testCases = new string[][]{
                new string[]{ "ca_ES_PREEURO", "ca_ES_PREEURO", "ca_ES@currency=ESP" },
                new string[]{ "de_AT_PREEURO", "de_AT_PREEURO", "de_AT@currency=ATS" },
                new string[]{ "de_DE_PREEURO", "de_DE_PREEURO", "de_DE@currency=DEM" },
                new string[]{ "de_LU_PREEURO", "de_LU_PREEURO", "de_LU@currency=EUR" },
                new string[]{ "el_GR_PREEURO", "el_GR_PREEURO", "el_GR@currency=GRD" },
                new string[]{ "en_BE_PREEURO", "en_BE_PREEURO", "en_BE@currency=BEF" },
                new string[]{ "en_IE_PREEURO", "en_IE_PREEURO", "en_IE@currency=IEP" },
                new string[]{ "es_ES_PREEURO", "es_ES_PREEURO", "es_ES@currency=ESP" },
                new string[]{ "eu_ES_PREEURO", "eu_ES_PREEURO", "eu_ES@currency=ESP" },
                new string[]{ "fi_FI_PREEURO", "fi_FI_PREEURO", "fi_FI@currency=FIM" },
                new string[]{ "fr_BE_PREEURO", "fr_BE_PREEURO", "fr_BE@currency=BEF" },
                new string[]{ "fr_FR_PREEURO", "fr_FR_PREEURO", "fr_FR@currency=FRF" },
                new string[]{ "fr_LU_PREEURO", "fr_LU_PREEURO", "fr_LU@currency=LUF" },
                new string[]{ "ga_IE_PREEURO", "ga_IE_PREEURO", "ga_IE@currency=IEP" },
                new string[]{ "gl_ES_PREEURO", "gl_ES_PREEURO", "gl_ES@currency=ESP" },
                new string[]{ "it_IT_PREEURO", "it_IT_PREEURO", "it_IT@currency=ITL" },
                new string[]{ "nl_BE_PREEURO", "nl_BE_PREEURO", "nl_BE@currency=BEF" },
                new string[]{ "nl_NL_PREEURO", "nl_NL_PREEURO", "nl_NL@currency=NLG" },
                new string[]{ "pt_PT_PREEURO", "pt_PT_PREEURO", "pt_PT@currency=PTE" },
                new string[]{ "de__PHONEBOOK", "de__PHONEBOOK", "de@collation=phonebook" },
                new string[]{ "de_PHONEBOOK", "de__PHONEBOOK", "de@collation=phonebook" },
                new string[]{ "en_GB_EURO", "en_GB_EURO", "en_GB@currency=EUR" },
                new string[]{ "en_GB@EURO", null, "en_GB@currency=EUR" }, /* POSIX ID */
                new string[]{ "es__TRADITIONAL", "es__TRADITIONAL", "es@collation=traditional" },
                new string[]{ "hi__DIRECT", "hi__DIRECT", "hi@collation=direct" },
                new string[]{ "ja_JP_TRADITIONAL", "ja_JP_TRADITIONAL", "ja_JP@calendar=japanese" },
                new string[]{ "th_TH_TRADITIONAL", "th_TH_TRADITIONAL", "th_TH@calendar=buddhist" },
                new string[]{ "zh_TW_STROKE", "zh_TW_STROKE", "zh_TW@collation=stroke" },
                new string[]{ "zh__PINYIN", "zh__PINYIN", "zh@collation=pinyin" },
                new string[]{ "zh@collation=pinyin", "zh@collation=pinyin", "zh@collation=pinyin" },
                new string[]{ "zh_CN@collation=pinyin", "zh_CN@collation=pinyin", "zh_CN@collation=pinyin" },
                new string[]{ "zh_CN_CA@collation=pinyin", "zh_CN_CA@collation=pinyin", "zh_CN_CA@collation=pinyin" },
                new string[]{ "en_US_POSIX", "en_US_POSIX", "en_US_POSIX" },
                new string[]{ "hy_AM_REVISED", "hy_AM_REVISED", "hy_AM_REVISED" },
                new string[]{ "no_NO_NY", "no_NO_NY", "no_NO_NY" /* not: "nn_NO" [alan ICU3.0] */ },
                new string[]{ "no@ny", null, "no__NY" /* not: "nn" [alan ICU3.0] */ }, /* POSIX ID */
                new string[]{ "no-no.utf32@B", null, "no_NO_B" /* not: "nb_NO_B" [alan ICU3.0] */ }, /* POSIX ID */
                new string[]{ "qz-qz@Euro", null, "qz_QZ@currency=EUR" }, /* qz-qz uses private use iso codes */
                new string[]{ "en-BOONT", "en__BOONT", "en__BOONT" }, /* registered name */
                new string[]{ "de-1901", "de__1901", "de__1901" }, /* registered name */
                new string[]{ "de-1906", "de__1906", "de__1906" }, /* registered name */
                new string[]{ "sr-SP-Cyrl", "sr_SP_CYRL", "sr_Cyrl_RS" }, /* .NET name */
                new string[]{ "sr-SP-Latn", "sr_SP_LATN", "sr_Latn_RS" }, /* .NET name */
                new string[]{ "sr_YU_CYRILLIC", "sr_YU_CYRILLIC", "sr_Cyrl_RS" }, /* Linux name */
                new string[]{ "uz-UZ-Cyrl", "uz_UZ_CYRL", "uz_Cyrl_UZ" }, /* .NET name */
                new string[]{ "uz-UZ-Latn", "uz_UZ_LATN", "uz_Latn_UZ" }, /* .NET name */
                new string[]{ "zh-CHS", "zh_CHS", "zh_Hans" }, /* .NET name */
                new string[]{ "zh-CHT", "zh_CHT", "zh_Hant" }, /* .NET name This may change back to zh_Hant */

                /* posix behavior that used to be performed by getName */
                new string[]{ "mr.utf8", null, "mr" },
                new string[]{ "de-tv.koi8r", null, "de_TV" },
                new string[]{ "x-piglatin_ML.MBE", null, "x-piglatin_ML" },
                new string[]{ "i-cherokee_US.utf7", null, "i-cherokee_US" },
                new string[]{ "x-filfli_MT_FILFLA.gb-18030", null, "x-filfli_MT_FILFLA" },
                new string[]{ "no-no-ny.utf8@B", null, "no_NO_NY_B" /* not: "nn_NO" [alan ICU3.0] */ }, /* @ ignored unless variant is empty */

                /* fleshing out canonicalization */
                /* sort keywords, ';' is separator so not present at end in canonical form */
                new string[]{ "en_Hant_IL_VALLEY_GIRL@currency=EUR;calendar=Japanese;", "en_Hant_IL_VALLEY_GIRL@calendar=Japanese;currency=EUR", "en_Hant_IL_VALLEY_GIRL@calendar=Japanese;currency=EUR" },
                /* already-canonical ids are not changed */
                new string[]{ "en_Hant_IL_VALLEY_GIRL@calendar=Japanese;currency=EUR", "en_Hant_IL_VALLEY_GIRL@calendar=Japanese;currency=EUR", "en_Hant_IL_VALLEY_GIRL@calendar=Japanese;currency=EUR" },
                /* PRE_EURO and EURO conversions don't affect other keywords */
                /* not in spec
               new string[]{ "es_ES_PREEURO@CALendar=Japanese", "es_ES_PREEURO@calendar=Japanese", "es_ES@calendar=Japanese;currency=ESP" },
               new string[]{ "es_ES_EURO@SHOUT=zipeedeedoodah", "es_ES_EURO@shout=zipeedeedoodah", "es_ES@currency=EUR;shout=zipeedeedoodah" },
                 */
                /* currency keyword overrides PRE_EURO and EURO currency */
                /* not in spec
               new string[]{ "es_ES_PREEURO@currency=EUR", "es_ES_PREEURO@currency=EUR", "es_ES@currency=EUR" },
               new string[]{ "es_ES_EURO@currency=ESP", "es_ES_EURO@currency=ESP", "es_ES@currency=ESP" },
                 */
                /* norwegian is just too weird, if we handle things in their full generality */
                /* this is a negative test to show that we DO NOT handle 'lang=no,var=NY' specially. */
                new string[]{ "no-Hant-GB_NY@currency=$$$", "no_Hant_GB_NY@currency=$$$", "no_Hant_GB_NY@currency=$$$" /* not: "nn_Hant_GB@currency=$$$" [alan ICU3.0] */ },

                /* test cases reflecting internal resource bundle usage */
                /* root is just a language */
                new string[]{ "root@kw=foo", "root@kw=foo", "root@kw=foo" },
                /* level 2 canonicalization should not touch basename when there are keywords and it is null */
                new string[]{ "@calendar=gregorian", "@calendar=gregorian", "@calendar=gregorian" },
        };

            for (int i = 0; i < testCases.Length; i++)
            {
                String[] testCase = testCases[i];
                String source = testCase[0];
                String level1Expected = testCase[1];
                String level2Expected = testCase[2];

                if (level1Expected != null)
                { // null means we have no expectations for how this case is handled
                    String level1 = UCultureInfo.GetFullName(source);
                    if (!level1.Equals(level1Expected))
                    {
                        Errln("UCultureInfo.GetFullName error for: '" + source +
                                "' expected: '" + level1Expected + "' but got: '" + level1 + "'");
                    }
                    else
                    {
                        Logln("UCultureInfo.GetFullName for: '" + source + "' returned: '" + level1 + "'");
                    }
                }
                else
                {
                    Logln("UCultureInfo.GetFullName skipped: '" + source + "'");
                }

                if (level2Expected != null)
                {
                    String level2 = UCultureInfo.Canonicalize(source);
                    if (!level2.Equals(level2Expected))
                    {
                        Errln("ULocale.getName error for: '" + source +
                                "' expected: '" + level2Expected + "' but got: '" + level2 + "'");
                    }
                    else
                    {
                        Logln("Ulocale.canonicalize for: '" + source + "' returned: '" + level2 + "'");
                    }
                }
                else
                {
                    Logln("ULocale.canonicalize skipped: '" + source + "'");
                }
            }
        }

        [Test]
        public void TestSomething()
        {

        }
    }
}
