using ICU4N.Support.Collections;
using ICU4N.Text;
using ICU4N.Util;
using NUnit.Framework;
using System.Collections.Generic;

namespace ICU4N.Dev.Test.Util
{
    public class LocaleAliasCollationTest : TestFmwk
    {
        private static readonly ULocale[][] _LOCALES = {
                new ULocale[] {new ULocale("en", "RH"), new ULocale("en", "ZW")},
                new ULocale[] {new ULocale("in"), new ULocale("id")},
                new ULocale[] {new ULocale("in", "ID"), new ULocale("id", "ID")},
                new ULocale[] {new ULocale("iw"), new ULocale("he")},
                new ULocale[] {new ULocale("iw", "IL"), new ULocale("he", "IL")},
                new ULocale[] {new ULocale("ji"), new ULocale("yi")},

                new ULocale[] {new ULocale("en", "BU"), new ULocale("en", "MM")},
                new ULocale[] {new ULocale("en", "DY"), new ULocale("en", "BJ")},
                new ULocale[] {new ULocale("en", "HV"), new ULocale("en", "BF")},
                new ULocale[] {new ULocale("en", "NH"), new ULocale("en", "VU")},
                new ULocale[] {new ULocale("en", "TP"), new ULocale("en", "TL")},
                new ULocale[] {new ULocale("en", "ZR"), new ULocale("en", "CD")}
        };

        private static readonly int _LOCALE_NUMBER = _LOCALES.Length;
        private ULocale[] available = null;
        private Dictionary<object, object> availableMap = new Dictionary<object, object>();
        private static readonly ULocale _DEFAULT_LOCALE = ULocale.US;

        public LocaleAliasCollationTest()
        {
        }

        [SetUp]
        public void Init()
        {
            available = ULocale.GetAvailableLocales();
            for (int i = 0; i < available.Length; i++)
            {
                availableMap[available[i].ToString()] = "";
            }
        }

        [Test]
        public void TestCollation()
        {
            ULocale defLoc = ULocale.GetDefault();
            ULocale.SetDefault(_DEFAULT_LOCALE);
            for (int i = 0; i < _LOCALE_NUMBER; i++)
            {
                ULocale oldLoc = _LOCALES[i][0];
                ULocale newLoc = _LOCALES[i][1];
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
            ULocale.SetDefault(defLoc);
        }
    }
}
