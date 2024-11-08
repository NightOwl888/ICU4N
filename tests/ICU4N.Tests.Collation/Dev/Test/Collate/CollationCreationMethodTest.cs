using ICU4N.Globalization;
using ICU4N.Text;
using J2N;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Text;
using Random = System.Random;

namespace ICU4N.Dev.Test.Collate
{
    /// <summary>
    /// CollationCreationMethodTest checks to ensure that the collators act the same whether they are created by choosing a
    /// locale and loading the data from file, or by using rules.
    /// </summary>
    /// <author>Brian Rower - IBM - August 2008</author>
    public class CollationCreationMethodTest : TestFmwk
    {
        [Test]
        public void TestRuleVsLocaleCreationMonkey()
        {
            //create a RBC from a collator reader by reading in a locale collation file
            //also create one simply from a rules string (which should be
            //pulled from the locale collation file)
            //and then do crazy monkey testing on it to make sure they are the same.
            int x, y, z;
            Random r = CreateRandom();
            String randString1;
            CollationKey key1;
            CollationKey key2;


            CultureInfo[] locales = Collator.GetCultures(UCultureTypes.AllCultures);

            RuleBasedCollator localeCollator;
            RuleBasedCollator ruleCollator;

            for (z = 0; z < 60; z++)
            {
                x = r.Next(locales.Length);
                CultureInfo locale = locales[x];

                try
                {
                    //this is making the assumption that the only type of collator that will be made is RBC
                    localeCollator = (RuleBasedCollator)Collator.GetInstance(locale);
                    Logln("Rules for " + locale + " are: " + localeCollator.GetRules());
                    ruleCollator = new RuleBasedCollator(localeCollator.GetRules());
                }
                catch (Exception e)
                {
                    Warnln("ERROR: in creation of collator of locale " + locale.DisplayName + ": " + e);
                    return;
                }

                //do it several times for each collator
                int n = 3;
                for (y = 0; y < n; y++)
                {

                    randString1 = GenerateNewString(r);

                    key1 = localeCollator.GetCollationKey(randString1);
                    key2 = ruleCollator.GetCollationKey(randString1);

                    Report(locale.DisplayName, randString1, key1, key2);
                }
            }
        }

        private String GenerateNewString(Random r)
        {
            int maxCodePoints = 40;
            byte[] c = new byte[r.Next(maxCodePoints) * 2]; //two bytes for each code point
            int x;
            int z;
            String s = "";

            for (x = 0; x < c.Length / 2; x = x + 2) //once around for each UTF-16 character
            {
                z = r.Next(0x7fff); //the code point...

                c[x + 1] = (byte)z;
                c[x] = (byte)(z >>> 4);
            }
            try
            {
                //s = new String(c, "UTF-16BE");
                s = Encoding.GetEncoding("UTF-16BE").GetString(c);
            }
            catch (Exception e)
            {
                Warnln("Error creating random strings");
            }
            return s;
        }

        private void Report(String localeName, String string1, CollationKey k1, CollationKey k2)
        {
            if (!k1.Equals(k2))
            {
                StringBuilder msg = new StringBuilder();
                msg.Append("With ").Append(localeName).Append(" collator\n and input string: ").Append(string1).Append('\n');
                msg.Append(" failed to produce identical keys on both collators\n");
                msg.Append("  localeCollator key: ").Append(CollationTest.Prettify(k1)).Append('\n');
                msg.Append("  ruleCollator   key: ").Append(CollationTest.Prettify(k2)).Append('\n');
                Errln(msg.ToString());
            }
        }
    }
}
