using ICU4N.Dev.Test;
using ICU4N.Support.Globalization;
using NUnit.Framework;

namespace ICU4N.Globalization
{
    public class CultureInfoExtensionsTest : TestFmwk
    {
        [Test]
        public void TestToUCultureInfoAllCultures()
        {
            var cultures = CultureInfoUtil.GetAllCultures();
            foreach (var culture in cultures)
            {
                Assert.IsNotNull(culture.ToUCultureInfo().EnsureCultureInfoInitialized());
            }
        }


        //[Test]
        //public void TestAllCultureInfos()
        //{
        //    var cultures = CultureInfoUtil.GetNeutralAndSpecificCultures();
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
