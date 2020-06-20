using ICU4N.Globalization;
using ICU4N.Support;
using ICU4N.Support.Collections;
using ICU4N.Text;
using ICU4N.Util;
using NUnit.Framework;
using System.Collections.Generic;

namespace ICU4N.Dev.Test.Util
{
    public class LocaleAliasCollationTest : TestFmwk
    {
        private static readonly UCultureInfo[][] _LOCALES = {
                new UCultureInfo[] {new UCultureInfo("en_RH"), new UCultureInfo("en_ZW")},
                new UCultureInfo[] {new UCultureInfo("in"), new UCultureInfo("id")},
                new UCultureInfo[] {new UCultureInfo("in_ID"), new UCultureInfo("id_ID")},
                new UCultureInfo[] {new UCultureInfo("iw"), new UCultureInfo("he")},
                new UCultureInfo[] {new UCultureInfo("iw_IL"), new UCultureInfo("he_IL")},
                new UCultureInfo[] {new UCultureInfo("ji"), new UCultureInfo("yi")},

                new UCultureInfo[] {new UCultureInfo("en_BU"), new UCultureInfo("en_MM")},
                new UCultureInfo[] {new UCultureInfo("en_DY"), new UCultureInfo("en_BJ")},
                new UCultureInfo[] {new UCultureInfo("en_HV"), new UCultureInfo("en_BF")},
                new UCultureInfo[] {new UCultureInfo("en_NH"), new UCultureInfo("en_VU")},
                new UCultureInfo[] {new UCultureInfo("en_TP"), new UCultureInfo("en_TL")},
                new UCultureInfo[] {new UCultureInfo("en_ZR"), new UCultureInfo("en_CD")}
        };

        private static readonly int _LOCALE_NUMBER = _LOCALES.Length;
        private UCultureInfo[] available = null;
        private Dictionary<object, object> availableMap = new Dictionary<object, object>();
        private static readonly UCultureInfo _DEFAULT_LOCALE = new UCultureInfo("en_US");

        public LocaleAliasCollationTest()
        {
        }

        [SetUp]
        public void Init()
        {
            available = UCultureInfo.GetCultures();
            for (int i = 0; i < available.Length; i++)
            {
                availableMap[available[i].ToString()] = "";
            }
        }

        [Test]
        public void TestCollation()
        {
            using (new ThreadCultureChange(_DEFAULT_LOCALE, _DEFAULT_LOCALE))
            {

                for (int i = 0; i < _LOCALE_NUMBER; i++)
                {
                    UCultureInfo oldLoc = _LOCALES[i][0];
                    UCultureInfo newLoc = _LOCALES[i][1];
                    if (availableMap.Get(_LOCALES[i][1]) == null)
                    {
                        Logln(_LOCALES[i][1] + " is not available. Skipping!");
                        continue;
                    }
                    Collator c1 = Collator.GetInstance(oldLoc);
                    Collator c2 = Collator.GetInstance(newLoc);

                    if (!c1.Equals(c2))
                    {
                        Errln("CollationTest: c1!=c2: newLoc= " + newLoc + " oldLoc= " + oldLoc);
                    }

                    Logln("Collation old:" + oldLoc + "   new:" + newLoc);
                }
            }
        }
    }
}
