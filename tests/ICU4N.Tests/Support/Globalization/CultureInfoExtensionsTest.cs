using ICU4N.Dev.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ICU4N.Globalization
{
    public class CultureInfoExtensionsTest : TestFmwk
    {
        [Test]
        public void TestToUCultureInfoAllCultures()
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            foreach (var culture in cultures)
            {
                Assert.IsNotNull(culture.ToUCultureInfo().culture);
            }
        }

        //[Test]
        //public void TestAllCultureInfos()
        //{
        //    var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures | CultureTypes.NeutralCultures);
        //    foreach (var culture in cultures)
        //    {
        //        string localeID = culture.ToString();

        //        string country = culture.GetCountry();
        //        string script = culture.GetScript();
        //        string variant = culture.GetVariant();
        //        string language = culture.GetLanguage();


        //    }
        //}
    }
}
