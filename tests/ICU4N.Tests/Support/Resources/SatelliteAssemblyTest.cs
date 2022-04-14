#if FEATURE_SATELLITE_ASSEMBLY_TESTS

using ICU4N.Dev.Test;
using ICU4N.Globalization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ICU4N.Support.Resources
{
    public class SatelliteAssemblyTest : TestFmwk
    {
        private static readonly IList<string> localeCultures = new List<string>();
        private static readonly IList<string> collCultures = new List<string>();
        private static readonly IList<string> currCultures = new List<string>();
        private static readonly IList<string> regionCultures = new List<string>();
        private static readonly IList<string> langCultures = new List<string>();
        private static readonly IList<string> translitCultures = new List<string>();

        static SatelliteAssemblyTest()
        {
            var assembly = typeof(ICU4N.Impl.ICUConfig).Assembly.GetSatelliteAssembly(CultureInfo.InvariantCulture);

            LoadManifest(assembly, "data.fullLocaleNames.lst", localeCultures);
            LoadManifest(assembly, "data.coll.fullLocaleNames.lst", collCultures);
            LoadManifest(assembly, "data.curr.fullLocaleNames.lst", currCultures);
            LoadManifest(assembly, "data.region.fullLocaleNames.lst", regionCultures);
            LoadManifest(assembly, "data.lang.fullLocaleNames.lst", langCultures);
            LoadManifest(assembly, "data.translit.fullLocaleNames.lst", translitCultures);
        }

        // ICU4N TODO: Move special case converter to common location
        public static string GetDotNetLocaleName(string baseName)
        {
            return baseName == "root" ? "" : new LocaleIDParser(baseName).GetName();
        }

        public static void LoadManifest(Assembly assembly, string manifestName, IList<string> result)
        {
            using var locales = assembly.GetManifestResourceStream(manifestName);
            string line;
            using var reader = new StreamReader(locales);
            while ((line = reader.ReadLine()) != null)
                result.Add(GetDotNetLocaleName(line.Trim()));
        }

        public static IEnumerable<TestCaseData> LocaleCultures
        {
            get
            {
                foreach (var culture in localeCultures)
                    yield return new TestCaseData(culture);
            }
        }
        public static IEnumerable<TestCaseData> CollationCultures
        {
            get
            {
                foreach (var culture in collCultures)
                    yield return new TestCaseData(culture);
            }
        }
        public static IEnumerable<TestCaseData> CurrencyCultures
        {
            get
            {
                foreach (var culture in currCultures)
                    yield return new TestCaseData(culture);
            }
        }
        public static IEnumerable<TestCaseData> RegionCultures
        {
            get
            {
                foreach (var culture in regionCultures)
                    yield return new TestCaseData(culture);
            }
        }
        public static IEnumerable<TestCaseData> LanguageCultures
        {
            get
            {
                foreach (var culture in langCultures)
                    yield return new TestCaseData(culture);
            }
        }
        public static IEnumerable<TestCaseData> TransliteratorCultures
        {
            get
            {
                foreach (var culture in translitCultures)
                    yield return new TestCaseData(culture);
            }
        }


        [Test]
        public void TestLoadInvariantAssembly()
        {
            var assembly = typeof(ICU4N.Impl.ICUConfig).Assembly.GetSatelliteAssembly(CultureInfo.InvariantCulture);
            assertNotNull(string.Empty, assembly);
        }

        [Test]
        [TestCaseSource("LocaleCultures")]
        public void TestLoadLocaleAssemblies(string culture)
        {
            var assembly = typeof(ICU4N.Impl.ICUConfig).Assembly.GetSatelliteAssembly(new ResourceCultureInfo(culture));
            assertNotNull(string.Empty, assembly);
        }

//        [Test]
//        public void TestMissingSatelliteAssembly()
//        {
//            var culture = "af-NA";
//#if NET46_OR_GREATER || NETCOREAPP1_0_OR_GREATER
//            var appDirectory = AppContext.BaseDirectory;
//#else
//            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
//#endif
//            var filePath = Path.Combine(appDirectory, culture, "ICU4N.resources.dll");
//            var tempFilePath = filePath.Replace(".dll", ".dll.bak");
//            File.Move(filePath, tempFilePath);

//            try
//            {
//                var assembly = typeof(ICU4N.ICUConfig).Assembly.GetSatelliteAssembly(new ResourceCultureInfo(culture));
//                fail($"Expected {typeof(FileNotFoundException)}");
//            }
//            catch (Exception e) when (!(e is AssertionException))
//            {
//                assertEquals($"Expected {typeof(FileNotFoundException)}, but was {e.GetType()}.", typeof(FileNotFoundException), e.GetType());
//                // expected
//            }
//            finally
//            {
//                File.Move(tempFilePath, filePath);
//            }
//        }
    }
}
#endif