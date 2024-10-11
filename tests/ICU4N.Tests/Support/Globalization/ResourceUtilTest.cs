//using ICU4N.Dev.Test;
//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using JCG = J2N.Collections.Generic;

//namespace ICU4N.Globalization
//{
//    public class ResourceUtilTest : TestFmwk
//    {
//        [Test]
//        public void TestGetDotNetNeutralCultureName_AllCultures_AreDotNetNeutralCultures()
//        {
//            var neutralCultures = new HashSet<string>(CultureInfo.GetCultures(CultureTypes.NeutralCultures).Select(c => c.Name));
//            var icuLocales = new HashSet<string>(UCultureInfo.GetCultures(UCultureTypes.AllCultures).Select(c => c.Name));

//            assertTrue("No ICU locales found", icuLocales.Count > 0);

//            var notContained = new JCG.HashSet<string>();

//            foreach (var locale in icuLocales)
//            {
//                string culture = ResourceUtil.GetDotNetNeutralCultureName(locale.AsSpan());

//                if (!neutralCultures.Contains(culture))
//                {
//                    notContained.Add(culture);
//                }
//            }

//            assertTrue($".NET doesn't contain the neutral culture(s) {notContained}", notContained.Count == 0);
//        }
//    }
//}
