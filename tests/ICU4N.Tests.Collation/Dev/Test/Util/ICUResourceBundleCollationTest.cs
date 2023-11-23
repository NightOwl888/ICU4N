using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Impl.Coll;
using ICU4N.Text;
using ICU4N.Util;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace ICU4N.Dev.Test.Util
{
    public sealed class ICUResourceBundleCollationTest : TestFmwk
    {
        private const String COLLATION_RESNAME = "collations";
        private const String COLLATION_KEYWORD = "collation";
        private const String DEFAULT_NAME = "default";
        private const String STANDARD_NAME = "standard";

        [Test]
        public void TestFunctionalEquivalent()
        {
            String[] collCases = {
               //  avail   locale                               equiv
                   "f",     "sv_US_CALIFORNIA",                 "sv",
                   "f",     "zh_TW@collation=stroke",           "zh@collation=stroke", /* alias of zh_Hant_TW */
                   "f",     "zh_Hant_TW@collation=stroke",      "zh@collation=stroke",
                   "f",     "sv_CN@collation=pinyin",           "sv",
                   "t",     "zh@collation=pinyin",              "zh",
                   "f",     "zh_CN@collation=pinyin",           "zh", /* alias of zh_Hans_CN */
                   "f",     "zh_Hans_CN@collation=pinyin",      "zh",
                   "f",     "zh_HK@collation=pinyin",           "zh", /* alias of zh_Hant_HK */
                   "f",     "zh_Hant_HK@collation=pinyin",      "zh",
                   "f",     "zh_HK@collation=stroke",           "zh@collation=stroke", /* alias of zh_Hant_HK */
                   "f",     "zh_Hant_HK@collation=stroke",      "zh@collation=stroke",
                   "f",     "zh_HK",                            "zh@collation=stroke", /* alias of zh_Hant_HK */
                   "f",     "zh_Hant_HK",                       "zh@collation=stroke",
                   "f",     "zh_MO",                            "zh@collation=stroke", /* alias of zh_Hant_MO */
                   "f",     "zh_Hant_MO",                       "zh@collation=stroke",
                   "f",     "zh_TW_STROKE",                     "zh@collation=stroke",
                   "f",     "zh_TW_STROKE@collation=big5han",   "zh@collation=big5han",
                   "f",     "sv_CN@calendar=japanese",          "sv",
                   "t",     "sv@calendar=japanese",             "sv",
                   "f",     "zh_TW@collation=big5han",          "zh@collation=big5han", /* alias of zh_Hant_TW */
                   "f",     "zh_Hant_TW@collation=big5han",     "zh@collation=big5han",
                   "f",     "zh_TW@collation=gb2312han",        "zh@collation=gb2312han", /* alias of zh_Hant_TW */
                   "f",     "zh_Hant_TW@collation=gb2312han",   "zh@collation=gb2312han",
                   "f",     "zh_CN@collation=big5han",          "zh@collation=big5han", /* alias of zh_Hans_CN */
                   "f",     "zh_Hans_CN@collation=big5han",     "zh@collation=big5han",
                   "f",     "zh_CN@collation=gb2312han",        "zh@collation=gb2312han", /* alias of zh_Hans_CN */
                   "f",     "zh_Hans_CN@collation=gb2312han",   "zh@collation=gb2312han",
                   "t",     "zh@collation=big5han",             "zh@collation=big5han",
                   "t",     "zh@collation=gb2312han",           "zh@collation=gb2312han",
                   "t",     "hi@collation=standard",            "hi",
                   "f",     "hi_AU@collation=standard;currency=CHF;calendar=buddhist",  "hi",
                   "f",     "sv_SE@collation=pinyin",           "sv", /* bug 4582 tests */
                   "f",     "sv_SE_BONN@collation=pinyin",      "sv",
                   "t",     "nl",                               "root",
                   "f",     "nl_NL",                            "root",
                   "f",     "nl_NL_EEXT",                       "root",
                   "t",     "nl@collation=stroke",              "root",
                   "f",     "nl_NL@collation=stroke",           "root",
                   "f",     "nl_NL_EEXT@collation=stroke",      "root",
               };

            Logln("Testing functional equivalents for collation...");
#if FEATURE_TYPEEXTENSIONS_GETTYPEINFO
            Assembly assembly = typeof(Collator).GetTypeInfo().Assembly;
#else
            Assembly assembly = typeof(Collator).Assembly;
#endif
            getFunctionalEquivalentTestCases(ICUData.IcuCollationBaseName, assembly,
               COLLATION_RESNAME, COLLATION_KEYWORD, true, collCases);
        }

        [Test]
        public void TestGetWithFallback()
        {
            /*
            UResourceBundle bundle =(UResourceBundle) UResourceBundle.getBundleInstance("com/ibm/icu/dev/data/testdata","te_IN");
            String key = bundle.getStringWithFallback("Keys/collation");
            if(!key.equals("COLLATION")){
                Errln("Did not get the expected result from getStringWithFallback method.");
            }
            String type = bundle.getStringWithFallback("Types/collation/direct");
            if(!type.equals("DIRECT")){
                Errln("Did not get the expected result form getStringWithFallback method.");
            }
            */
            ICUResourceBundle bundle = null;
            String key = null;
            try
            {
                bundle = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.IcuCollationBaseName, UCultureInfo.Canonicalize("de__PHONEBOOK"));

                if (!bundle.UCulture.FullName.Equals("de"))
                {
                    Errln("did not get the expected bundle");
                }
                key = bundle.GetStringWithFallback("collations/collation/default");
                if (!key.Equals("phonebook"))
                {
                    Errln("Did not get the expected result from getStringWithFallback method.");
                }

            }
            catch (MissingManifestResourceException ex)
            {
                Logln("got the expected exception");
            }


            bundle = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.IcuCollationBaseName, "fr_FR");
            key = bundle.GetStringWithFallback("collations/default");
            if (!key.Equals("standard"))
            {
                Errln("Did not get the expected result from getStringWithFallback method.");
            }
        }

        [Test]
        public void TestKeywordValues()
        {
            String[] kwVals;
            bool foundStandard = false;
            int n;

            Logln("Testing getting collation values:");
            kwVals = ICUResourceBundle.GetKeywordValues(ICUData.IcuCollationBaseName, COLLATION_RESNAME, ICUResourceBundle.IcuDataAssembly);
            for (n = 0; n < kwVals.Length; n++)
            {
                Logln(n.ToString(CultureInfo.InvariantCulture) + ": " + kwVals[n]);
                if (DEFAULT_NAME.Equals(kwVals[n]))
                {
                    Errln("getKeywordValues for collation returned 'default' in the list.");
                }
                else if (STANDARD_NAME.Equals(kwVals[n]))
                {
                    if (foundStandard == false)
                    {
                        foundStandard = true;
                        Logln("found 'standard'");
                    }
                    else
                    {
                        Errln("Error - 'standard' is in the keyword list twice!");
                    }
                }
            }

            if (foundStandard == false)
            {
                Errln("Error - 'standard' was not in the collation tree as a keyword.");
            }
            else
            {
                Logln("'standard' was found as a collation keyword.");
            }
        }

        [Test]
        public void TestOpen()
        {
            UResourceBundle bundle = UResourceBundle.GetBundleInstance(ICUData.IcuCollationBaseName, "en_US_POSIX");
            if (bundle == null)
            {
                Errln("could not load the stream");
            }
        }

        private void getFunctionalEquivalentTestCases(String path, Assembly cl, String resName, String keyword,
                bool truncate, String[] testCases)
        {
            //String F_STR = "f";
            String T_STR = "t";

            Logln("Testing functional equivalents...");
            for (int i = 0; i < testCases.Length; i += 3)
            {
                bool expectAvail = T_STR.Equals(testCases[i + 0]);
                UCultureInfo inLocale = new UCultureInfo(testCases[i + 1]);
                UCultureInfo expectLocale = new UCultureInfo(testCases[i + 2]);

                Logln(((int)(i / 3)).ToString(CultureInfo.InvariantCulture) + ": " + expectAvail.ToString() + "\t\t" +
                        inLocale.ToString() + "\t\t" + expectLocale.ToString());

                UCultureInfo equivLocale = ICUResourceBundle.GetFunctionalEquivalent(path, cl, resName, keyword, inLocale, out bool gotAvail, truncate);

                if ((gotAvail != expectAvail) || !equivLocale.Equals(expectLocale))
                {
                    Errln(((int)(i / 3)).ToString(CultureInfo.InvariantCulture) + ":  Error, expected  Equiv=" + expectAvail.ToString() + "\t\t" +
                            inLocale.ToString() + "\t\t--> " + expectLocale.ToString() + ",  but got " + gotAvail.ToString() + " " +
                            equivLocale.ToString());
                }
            }
        }
    }
}
