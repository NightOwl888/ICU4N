using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Text;
using ICU4N.Util;
using J2N.IO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace ICU4N.Dev.Test.Util
{
    public sealed class ICUResourceBundleTest : TestFmwk
    {
        private static readonly Assembly testLoader =
#if FEATURE_TYPEEXTENSIONS_GETTYPEINFO
            typeof(ICUResourceBundleTest).GetTypeInfo().Assembly; //ICUResourceBundleTest.class.getClassLoader();
#else
            typeof(ICUResourceBundleTest).Assembly; //ICUResourceBundleTest.class.getClassLoader();
#endif

        // ICU4N TODO: Finish implementation
        //        [Test]
        //    public void TestGetResources()
        //        {
        //            try
        //            {
        //                // It does not work well in eclipse plug-in test because of class loader configuration??
        //                // For now, specify resource path explicitly in this test case
        //                //Enumeration en = testLoader.getResources("META-INF");
        //                Enumeration en = testLoader.GetResources("com.ibm.icu.dev.data");
        //                for (; en.hasMoreElements();)
        //                {
        //                    URL url = (URL)en.nextElement();
        //                    if (url == null)
        //                    {
        //                        warnln("could not load resource data");
        //                        return;
        //                    }
        //                    URLConnection c = url.openConnection();

        //                    if (c instanceof JarURLConnection) {
        //                    JarURLConnection jc = (JarURLConnection)c;
        //                    JarEntry je = jc.getJarEntry();
        //                    Logln("jar entry: " + je.toString());
        //                } else {
        //                    BufferedReader br = new BufferedReader(
        //                            new InputStreamReader(c.getInputStream()));
        //                    Logln("input stream:");
        //                    try
        //                    {
        //                        string line = null;
        //                        int n = 0;
        //                        while ((line = br.readLine()) != null)
        //                        {
        //                            Logln("  " + ++n + ": " + line);
        //                        }
        //                    }
        //                    finally
        //                    {
        //                        br.close();
        //                    }
        //                }
        //            }
        //        }catch(SecurityException ex) {
        //            warnln("could not load resource data: " + ex);
        //        ex.printStackTrace();
        //    }catch(NullPointerException ex) {
        //        // thrown by ibm 1.4.2 windows jvm security manager
        //        warnln("could not load resource data: " + ex);
        //}catch(Exception ex){
        //        ex.printStackTrace();
        //            Errln("Unexpected exception: "+ ex);
        //        }
        //    }
        //    [Test]
        //    public void TestResourceBundleWrapper()
        //{
        //    UResourceBundle bundle = UResourceBundle.GetBundleInstance("ICU4N.Impl.Data.HolidayBundle", "da_DK");
        //    Object o = bundle.GetObject("holidays");
        //    if (o is Holiday[] ){
        //        Logln("wrapper mechanism works for Weekend data");
        //    }else{
        //        Errln("Did not get the expected output for Weekend data");
        //    }

        //    bundle = UResourceBundle.GetBundleInstance(ICUData.ICU_BASE_NAME, "bogus");
        //    if (bundle is UResourceBundle && bundle.GetULocale().GetName().Equals("en_US")){
        //        Logln("wrapper mechanism works for bogus locale");
        //    }else{
        //        Errln("wrapper mechanism failed for bogus locale.");
        //    }

        //    try
        //    {
        //        bundle = UResourceBundle.GetBundleInstance("bogus", "bogus");
        //        if (bundle != null)
        //        {
        //            Errln("Did not get the expected exception");
        //        }
        //    }
        //    catch (MissingManifestResourceException ex)
        //    {
        //        Logln("got the expected exception");
        //    }


        //}
        [Test]
        public void TestJB3879()
        {
            // this tests tests loading of root bundle when a resource bundle
            // for the default locale is requested
            try
            {
                UResourceBundle bundle = UResourceBundle.GetBundleInstance("Dev/Data/TestData", UCultureInfo.CurrentCulture, testLoader);
                if (bundle == null)
                {
                    Errln("could not create the resource bundle");
                }
            }
            catch (MissingManifestResourceException ex)
            {
                Warnln("could not load test data: " + ex.ToString());
            }
        }
        [Test]
        public void TestOpen()
        {
            UResourceBundle bundle = UResourceBundle.GetBundleInstance(ICUData.IcuBaseName, "en_US_POSIX");

            if (bundle == null)
            {
                Errln("could not create the resource bundle");
            }

            UResourceBundle obj = bundle.Get("NumberElements").Get("latn").Get("patterns");

            int size = obj.Length;
            UResourceType type = obj.Type;
            if (type == UResourceType.Table)
            {
                UResourceBundle sub;
                for (int i = 0; i < size; i++)
                {
                    sub = obj.Get(i);
                    string temp = sub.GetString();
                    if (temp.Length == 0)
                    {
                        Errln("Failed to get the items from number patterns table in bundle: " +
                                bundle.UCulture.Name);
                    }
                    //System.out.println("\""+prettify(temp)+"\"");
                }
            }

            obj = bundle.Get("NumberElements").Get("latn").Get("symbols");

            size = obj.Length;
            type = obj.Type;
            if (type == UResourceType.Table)
            {
                UResourceBundle sub;
                for (int i = 0; i < size; i++)
                {
                    sub = obj.Get(i);
                    string temp = sub.GetString();
                    if (temp.Length == 0)
                    {
                        Errln("Failed to get the items from number symbols table in bundle: " +
                                bundle.UCulture.Name);
                    }
                    // System.out.println("\""+prettify(temp)+"\"");
                }
            }

            if (bundle == null)
            {
                Errln("could not create the resource bundle");
            }

            bundle = UResourceBundle.GetBundleInstance(ICUData.IcuBaseName, "zzz_ZZ_very_very_very_long_bogus_bundle");
            if (!bundle.UCulture.Equals(UCultureInfo.CurrentCulture))
            {
                Errln("UResourceBundle did not load the default bundle when bundle was not found. Default: " + UCultureInfo.CurrentCulture +
                            ", Bundle locale: " + bundle.UCulture);
            }
        }

        [Test]
        public void TestBasicTypes()
        {
            UResourceBundle bundle = null;
            try
            {
                bundle = UResourceBundle.GetBundleInstance("Dev/Data/TestData", "testtypes", testLoader);
            }
            catch (MissingManifestResourceException e)
            {
                Warnln("could not load test data: " + e.ToString());
                return;
            }
            {
                string expected = "abc\u0000def";
                UResourceBundle sub = bundle.Get("zerotest");
                if (!expected.Equals(sub.GetString()))
                {
                    Errln("Did not get the expected string for key zerotest in bundle testtypes");
                }
                sub = bundle.Get("emptyexplicitstring");
                expected = "";
                if (!expected.Equals(sub.GetString()))
                {
                    Errln("Did not get the expected string for key emptyexplicitstring in bundle testtypes");
                }
                sub = bundle.Get("emptystring");
                expected = "";
                if (!expected.Equals(sub.GetString()))
                {
                    Errln("Did not get the expected string for key emptystring in bundle testtypes");
                }
            }
            {
                int expected = 123;
                UResourceBundle sub = bundle.Get("onehundredtwentythree");
                if (expected != sub.GetInt32())
                {
                    Errln("Did not get the expected int value for key onehundredtwentythree in bundle testtypes");
                }
                sub = bundle.Get("emptyint");
                expected = 0;
                if (expected != sub.GetInt32())
                {
                    Errln("Did not get the expected int value for key emptyint in bundle testtypes");
                }
            }
            {
                int expected = 1;
                UResourceBundle sub = bundle.Get("one");
                if (expected != sub.GetInt32())
                {
                    Errln("Did not get the expected int value for key one in bundle testtypes");
                }
            }
            {
                int expected = -1;
                UResourceBundle sub = bundle.Get("minusone");
                int got = sub.GetInt32();
                if (expected != got)
                {
                    Errln("Did not get the expected int value for key minusone in bundle testtypes");
                }
                expected = 0xFFFFFFF;
                got = sub.GetUInt32();
                if (expected != got)
                {
                    Errln("Did not get the expected int value for key minusone in bundle testtypes");
                }
            }
            {
                int expected = 1;
                UResourceBundle sub = bundle.Get("plusone");
                if (expected != sub.GetInt32())
                {
                    Errln("Did not get the expected int value for key minusone in bundle testtypes");
                }

            }
            {
                int[] expected = new int[] { 1, 2, 3, -3, 4, 5, 6, 7 };
                UResourceBundle sub = bundle.Get("integerarray");
                if (!Utility.ArrayEquals(expected, sub.GetInt32Vector()))
                {
                    Errln("Did not get the expected int vector value for key integerarray in bundle testtypes");
                }
                sub = bundle.Get("emptyintv");
                expected = new int[0];
                if (!Utility.ArrayEquals(expected, sub.GetInt32Vector()))
                {
                    Errln("Did not get the expected int vector value for key emptyintv in bundle testtypes");
                }

            }
            {
                UResourceBundle sub = bundle.Get("binarytest");
                ByteBuffer got = sub.GetBinary();
                if (got.Remaining != 15)
                {
                    Errln("Did not get the expected length for the binary ByteBuffer");
                }
                for (int i = 0; i < got.Remaining; i++)
                {
                    byte b = got.Get();
                    if (b != i)
                    {
                        Errln("Did not get the expected value for binary buffer at index: " + i);
                    }
                }
                sub = bundle.Get("emptybin");
                got = sub.GetBinary();
                if (got.Remaining != 0)
                {
                    Errln("Did not get the expected length for the emptybin ByteBuffer");
                }

            }
            {
                UResourceBundle sub = bundle.Get("emptyarray");
                string key = sub.Key;
                if (!key.Equals("emptyarray"))
                {
                    Errln("Did not get the expected key for emptytable item");
                }
                if (sub.Length != 0)
                {
                    Errln("Did not get the expected length for emptytable item");
                }
            }
            {
                UResourceBundle sub = bundle.Get("menu");
                string key = sub.Key;
                if (!key.Equals("menu"))
                {
                    Errln("Did not get the expected key for menu item");
                }
                UResourceBundle sub1 = sub.Get("file");
                key = sub1.Key;
                if (!key.Equals("file"))
                {
                    Errln("Did not get the expected key for file item");
                }
                UResourceBundle sub2 = sub1.Get("open");
                key = sub2.Key;
                if (!key.Equals("open"))
                {
                    Errln("Did not get the expected key for file item");
                }
                string value = sub2.GetString();
                if (!value.Equals("Open"))
                {
                    Errln("Did not get the expected value for key for oen item");
                }

                sub = bundle.Get("emptytable");
                key = sub.Key;
                if (!key.Equals("emptytable"))
                {
                    Errln("Did not get the expected key for emptytable item");
                }
                if (sub.Length != 0)
                {
                    Errln("Did not get the expected length for emptytable item");
                }
                sub = bundle.Get("menu").Get("file");
                int size = sub.Length;
                string expected;
                for (int i = 0; i < size; i++)
                {
                    sub1 = sub.Get(i);

                    switch (i)
                    {
                        case 0:
                            expected = "exit";
                            break;
                        case 1:
                            expected = "open";
                            break;
                        case 2:
                            expected = "save";
                            break;
                        default:
                            expected = "";
                            break;
                    }
                    string got = sub1.Key;
                    if (!expected.Equals(got))
                    {
                        Errln("Did not get the expected key at index" + i + ". Expected: " + expected + " Got: " + got);
                    }
                    else
                    {
                        Logln("Got the expected key at index: " + i);
                    }
                }
            }

        }
        private sealed class TestCase
        {
            internal string key;
            internal int value;
            internal TestCase(string key, int value)
            {
                this.key = key;
                this.value = value;
            }
        }
        [Test]
        public void TestTable32()
        {
            TestCase[] arr = new TestCase[]{
                new TestCase  ( "ooooooooooooooooo", 0 ),
                new TestCase  ( "oooooooooooooooo1", 1 ),
                new TestCase  ( "ooooooooooooooo1o", 2 ),
                new TestCase  ( "oo11ooo1ooo11111o", 25150 ),
                new TestCase  ( "oo11ooo1ooo111111", 25151 ),
                new TestCase  ( "o1111111111111111", 65535 ),
                new TestCase  ( "1oooooooooooooooo", 65536 ),
                new TestCase  ( "1ooooooo11o11ooo1", 65969 ),
                new TestCase  ( "1ooooooo11o11oo1o", 65970 ),
                new TestCase  ( "1ooooooo111oo1111", 65999 )
            };
            UResourceBundle bundle = null;
            try
            {
                bundle = UResourceBundle.GetBundleInstance("Dev/Data/TestData", "testtable32", testLoader);
            }
            catch (MissingManifestResourceException ex)
            {
                Warnln("could not load resource data: " + ex.ToString());
                return;
            }

            if (bundle.Type != UResourceType.Table)
            {
                Errln("Could not get the correct type for bundle testtable32");
            }

            int size = bundle.Length;
            if (size != 66000)
            {
                Errln("Could not get the correct size for bundle testtable32");
            }

            int number = -1;

            // get the items by index
            for (int i = 0; i < size; i++)
            {
                UResourceBundle item = bundle.Get(i);
                string key = item.Key;
                int parsedNumber = parseTable32Key(key);
                switch (item.Type)
                {
                    case UResourceType.String:
                        string value = item.GetString();
                        number = UTF16.CharAt(value, 0);
                        break;
                    case UResourceType.Int32:
                        number = item.GetInt32();
                        break;
                    default:
                        Errln("Got unexpected resource type in testtable32");
                        break;
                }
                if (number != parsedNumber)
                {
                    Errln("Did not get expected value in testtypes32 for key" +
                          key + ". Expected: " + parsedNumber + " Got:" + number);
                }

            }

            // search for some items by key
            for (int i = 0; i < arr.Length; i++)
            {
                UResourceBundle item = bundle.Get(arr[i].key);
                switch (item.Type)
                {
                    case UResourceType.String:
                        string value = item.GetString();
                        number = UTF16.CharAt(value, 0);
                        break;
                    case UResourceType.Int32:
                        number = item.GetInt32();
                        break;
                    default:
                        Errln("Got unexpected resource type in testtable32");
                        break;
                }

                if (number != arr[i].value)
                {
                    Errln("Did not get expected value in testtypes32 for key" +
                          arr[i].key + ". Expected: " + arr[i].value + " Got:" + number);
                }
            }
        }
        private static int parseTable32Key(string key)
        {
            int number;
            char c;

            number = 0;
            for (int i = 0; i < key.Length; i++)
            {
                c = key[i];
                number <<= 1;
                if (c == '1')
                {
                    number |= 1;
                }
            }
            return number;
        }

        [Test]
        public void TestAliases()
        {
            string simpleAlias = "Open";

            UResourceBundle rb = UResourceBundle.GetBundleInstance("Dev/Data/TestData", "testaliases", testLoader);
            if (rb == null)
            {
                Warnln("could not load testaliases data");
                return;
            }
            UResourceBundle sub = rb.Get("simplealias");
            string s1 = sub.GetString("simplealias");
            if (s1.Equals(simpleAlias))
            {
                Logln("Alias mechanism works for simplealias");
            }
            else
            {
                Errln("Did not get the expected output for simplealias");
            }
            {
                try
                {
                    rb = UResourceBundle.GetBundleInstance("Dev/Data/TestData", "testaliases", testLoader);
                    sub = rb.Get("nonexisting");
                    Errln("Did not get the expected exception for nonexisting");
                }
                catch (MissingManifestResourceException ex)
                {
                    Logln("Alias mechanism works for nonexisting alias");
                }
            }
            {
                rb = UResourceBundle.GetBundleInstance("Dev/Data/TestData", "testaliases", testLoader);
                sub = rb.Get("referencingalias");
                s1 = sub.GetString();
                if (s1.Equals("H:mm:ss"))
                {
                    Logln("Alias mechanism works for referencingalias");
                }
                else
                {
                    Errln("Did not get the expected output for referencingalias");
                }
            }
            {
                UResourceBundle rb1 = UResourceBundle.GetBundleInstance("Dev/Data/TestData", "testaliases", testLoader);
                if (rb1 != rb)
                {
                    Errln("Caching of the resource bundle failed");
                }
                else
                {
                    Logln("Caching of resource bundle passed");
                }
                sub = rb1.Get("testGetStringByKeyAliasing");

                s1 = sub.Get("KeyAlias0PST").GetString();
                if (s1.Equals("America/Los_Angeles"))
                {
                    Logln("Alias mechanism works for KeyAlias0PST");
                }
                else
                {
                    Errln("Did not get the expected output for KeyAlias0PST");
                }

                s1 = sub.GetString("KeyAlias1PacificStandardTime");
                if (s1.Equals("Pacific Standard Time"))
                {
                    Logln("Alias mechanism works for KeyAlias1PacificStandardTime");
                }
                else
                {
                    Errln("Did not get the expected output for KeyAlias1PacificStandardTime");
                }
                s1 = sub.GetString("KeyAlias2PDT");
                if (s1.Equals("PDT"))
                {
                    Logln("Alias mechanism works for KeyAlias2PDT");
                }
                else
                {
                    Errln("Did not get the expected output for KeyAlias2PDT");
                }

                s1 = sub.GetString("KeyAlias3LosAngeles");
                if (s1.Equals("Los Angeles"))
                {
                    Logln("Alias mechanism works for KeyAlias3LosAngeles. Got: " + s1);
                }
                else
                {
                    Errln("Did not get the expected output for KeyAlias3LosAngeles. Got: " + s1);
                }
            }
            {
                sub = rb.Get("testGetStringByIndexAliasing");
                s1 = sub.GetString(0);
                if (s1.Equals("America/Los_Angeles"))
                {
                    Logln("Alias mechanism works for testGetStringByIndexAliasing/0. Got: " + s1);
                }
                else
                {
                    Errln("Did not get the expected output for testGetStringByIndexAliasing/0. Got: " + s1);
                }
                s1 = sub.GetString(1);
                if (s1.Equals("Pacific Standard Time"))
                {
                    Logln("Alias mechanism works for testGetStringByIndexAliasing/1");
                }
                else
                {
                    Errln("Did not get the expected output for testGetStringByIndexAliasing/1");
                }
                s1 = sub.GetString(2);
                if (s1.Equals("PDT"))
                {
                    Logln("Alias mechanism works for testGetStringByIndexAliasing/2");
                }
                else
                {
                    Errln("Did not get the expected output for testGetStringByIndexAliasing/2");
                }

                s1 = sub.GetString(3);
                if (s1.Equals("Los Angeles"))
                {
                    Logln("Alias mechanism works for testGetStringByIndexAliasing/3. Got: " + s1);
                }
                else
                {
                    Errln("Did not get the expected output for testGetStringByIndexAliasing/3. Got: " + s1);
                }
            }

            // Note: Following test cases are no longer working because collation data is now in the collation module
            //        {
            //            sub = rb.get("testAliasToTree" );
            //
            //            ByteBuffer buf = sub.get("standard").get("%%CollationBin").GetBinary();
            //            if(buf==null){
            //                Errln("Did not get the expected output for %%CollationBin");
            //            }
            //        }
            //
            //        rb = (UResourceBundle) UResourceBundle.GetBundleInstance(ICUResourceBundle.ICU_COLLATION_BASE_NAME,"zh_TW");
            //        UResourceBundle b = (UResourceBundle) rb.getObject("collations");
            //        if(b != null){
            //            if(b.get(0).getKey().Equals( "default")){
            //                Logln("Alias mechanism works");
            //            }else{
            //                Errln("Alias mechanism failed for zh_TW collations");
            //            }
            //        }else{
            //            Errln("Did not get the expected object for collations");
            //        }

            // Test case for #7996
            {
                UResourceBundle bundle = UResourceBundle.GetBundleInstance("Dev/Data/TestData", "te", testLoader);
                UResourceBundle table = bundle.Get("tableT7996");
                try
                {
                    string s = table.GetString("a7996");
                    Logln("Alias in nested table referring one in sh worked - " + s);
                }
                catch (MissingManifestResourceException e)
                {
                    Errln("Alias in nested table referring one in sh failed");
                }

                try
                {
                    string s = ((ICUResourceBundle)table).GetStringWithFallback("b7996");
                    Logln("Alias with /LOCALE/ in nested table in root referring back to another key in the current locale bundle worked - " + s);
                }
                catch (MissingManifestResourceException e)
                {
                    Errln("Alias with /LOCALE/ in nested table in root referring back to another key in the current locale bundle failed");
                }
            }

        }
        [Test]
        public void TestAlias()
        {
            Logln("Testing %%ALIAS");
            UResourceBundle rb = UResourceBundle.GetBundleInstance(ICUData.IcuBaseName, "iw_IL");
            UResourceBundle b = rb.Get("NumberElements");
            if (b != null)
            {
                if (b.Length > 0)
                {
                    Logln("%%ALIAS mechanism works");
                }
                else
                {
                    Errln("%%ALIAS mechanism failed for iw_IL NumberElements");
                }
            }
            else
            {
                Errln("%%ALIAS mechanism failed for iw_IL");
            }
        }
        [Test]
        public void TestXPathAlias()
        {
            UResourceBundle rb = UResourceBundle.GetBundleInstance("Dev/Data/TestData", "te_IN", testLoader);
            UResourceBundle b = rb.Get("aliasClient");
            string result = b.GetString();
            string expResult = "correct";

            if (!result.Equals(expResult))
            {
                Errln("Did not get the expected result for XPath style alias");
            }
            try
            {
                UResourceBundle c = rb.Get("rootAliasClient");
                result = c.GetString();
                expResult = "correct";
                if (!result.Equals(expResult))
                {
                    Errln("Did not get the expected result for XPath style alias for rootAliasClient");
                }
            }
            catch (MissingManifestResourceException ex)
            {
                Errln("Could not get rootAliasClient");
            }
        }
        [Test]
        public void TestCircularAliases()
        {
            try
            {
                UResourceBundle rb = UResourceBundle.GetBundleInstance("Dev/Data/TestData", "testaliases", testLoader);
                UResourceBundle sub = rb.Get("aaa");
                string s1 = sub.GetString();
                if (s1 != null)
                {
                    Errln("Did not get the expected exception");
                }
            }
            catch (ArgumentException ex)
            {
                Logln("got expected exception for circular references");
            }
            catch (MissingManifestResourceException ex)
            {
                Warnln("could not load resource data: " + ex.ToString());
            }
        }

        [Test]
        public void TestPreventFallback()
        {
            string noFallbackResource = "string_in_te_no_te_IN_fallback";
            ICUResourceBundle rb = (ICUResourceBundle)UResourceBundle.GetBundleInstance("Dev/Data/TestData", "te_IN_NE", testLoader);
            try
            {
                rb.GetStringWithFallback(noFallbackResource);
                fail("Expected MissingManifestResourceException.");
            }
            catch (MissingManifestResourceException e)
            {
                // Expected
            }
            rb.GetStringWithFallback("string_only_in_te");
            rb = (ICUResourceBundle)UResourceBundle.GetBundleInstance("Dev/Data/TestData", "te", testLoader);
            rb.GetStringWithFallback(noFallbackResource);
        }

        [Test]
        public void TestGetWithFallback()
        {
            /*
            UResourceBundle bundle =(UResourceBundle) UResourceBundle.GetBundleInstance("Dev/Data/TestData","te_IN");
            string key = bundle.getStringWithFallback("Keys/collation");
            if(!key.Equals("COLLATION")){
                Errln("Did not get the expected result from getStringWithFallback method.");
            }
            string type = bundle.getStringWithFallback("Types/collation/direct");
            if(!type.Equals("DIRECT")){
                Errln("Did not get the expected result form getStringWithFallback method.");
            }
            */
            ICUResourceBundle bundle = null;

            bundle = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.IcuBaseName, "fr_FR");
            ICUResourceBundle b1 = bundle.GetWithFallback("calendar");
            string defaultCal = b1.GetStringWithFallback("default");
            if (!defaultCal.Equals("gregorian"))
            {
                Errln("Did not get the expected default calendar string: Expected: gregorian, Got: " + defaultCal);
            }
            ICUResourceBundle b2 = b1.GetWithFallback(defaultCal);
            ICUResourceBundle b3 = b2.GetWithFallback("monthNames");
            ICUResourceBundle b4 = b3.GetWithFallback("format");
            ICUResourceBundle b5 = b4.GetWithFallback("narrow");
            if (b5.Length != 12)
            {
                Errln("Did not get the expected size for the monthNames");
            }
        }

        private const string CALENDAR_RESNAME = "calendar";
        private const string CALENDAR_KEYWORD = "calendar";

        [Test]
        public void TestLocaleDisplayNames()
        {
            UCultureInfo[] locales = UCultureInfo.GetCultures(UCultureTypes.AllCultures);

            ISet<string> localCountryExceptions = new HashSet<string>();
            if (logKnownIssue("cldrbug:8903",
                    "No localized region name for lrc_IQ, lrc_IR, nus_SS, nds_DE, ti_ER, ti_ET"))
            {
                localCountryExceptions.Add("lrc_IQ");
                localCountryExceptions.Add("lrc_IR");
                localCountryExceptions.Add("nus_SS");
                localCountryExceptions.Add("nds_DE");
                localCountryExceptions.Add("nds_NL");
                localCountryExceptions.Add("ti_ER");
                localCountryExceptions.Add("ti_ET");
            }

            ISet<string> localLangExceptions = new HashSet<string>();
            if (logKnownIssue("cldrbug:8903", "No localized language name for nmg, nds"))
            {
                localLangExceptions.Add("nmg");
                localLangExceptions.Add("nds");
            }

            for (int i = 0; i < locales.Length; ++i)
            {
                if (!hasLocalizedCountryFor(new UCultureInfo("en"), locales[i]))
                {
                    Errln("Could not get English localized country for " + locales[i]);
                }
                if (!hasLocalizedLanguageFor(new UCultureInfo("en"), locales[i]))
                {
                    Errln("Could not get English localized language for " + locales[i]);
                }

                if (!hasLocalizedCountryFor(locales[i], locales[i])
                        && !localCountryExceptions.Contains(locales[i].ToString()))
                {
                    Errln("Could not get native localized country for " + locales[i]);
                    hasLocalizedCountryFor(locales[i], locales[i]);
                }
                if (!hasLocalizedLanguageFor(locales[i], locales[i])
                        && !localLangExceptions.Contains(locales[i].Language))
                {
                    Errln("Could not get native localized language for " + locales[i]);
                }

                Logln(locales[i] + "\t" + locales[i].GetDisplayName(new UCultureInfo("en")) + "\t" + locales[i].GetDisplayName(locales[i]));
            }
        }

        private static bool hasLocalizedLanguageFor(UCultureInfo locale, UCultureInfo otherLocale)
        {
            string lang = otherLocale.Language;
            string localizedVersion = otherLocale.GetDisplayLanguage(locale);
            return !lang.Equals(localizedVersion);
        }

        private static bool hasLocalizedCountryFor(UCultureInfo locale, UCultureInfo otherLocale)
        {
            string country = otherLocale.Country;
            if (country.Equals("")) return true;
            string localizedVersion = otherLocale.GetDisplayCountry(locale);
            return !country.Equals(localizedVersion);
        }

        [Test]
        public void TestFunctionalEquivalent()
        {
            string[] calCases = {
       //  avail    locale                              equiv
           "t",     "en_US_POSIX",                      "en@calendar=gregorian",
           "f",     "ja_JP_TOKYO",                      "ja@calendar=gregorian",
           "f",     "ja_JP_TOKYO@calendar=japanese",    "ja@calendar=japanese",
           "t",     "sr@calendar=gregorian",            "sr@calendar=gregorian",
           "t",     "en",                               "en@calendar=gregorian",
           "t",     "th_TH",                            "th@calendar=buddhist",
           "t",     "th_TH@calendar=gregorian",         "th@calendar=gregorian",
           "f",     "th_TH_Bangkok",                    "th@calendar=buddhist",
       };

            Logln("Testing functional equivalents for calendar...");

#if FEATURE_TYPEEXTENSIONS_GETTYPEINFO
            Assembly assembly = typeof(BreakIterator).GetTypeInfo().Assembly;
#else
            Assembly assembly = typeof(BreakIterator).Assembly;
#endif
            getFunctionalEquivalentTestCases(ICUData.IcuBaseName,
                                             //typeof(Calendar).GetTypeInfo().Assembly, // ICU4N TODO: If we ever port the Calendar type, we should reference it here
                                             assembly,
                       CALENDAR_RESNAME, CALENDAR_KEYWORD, false, calCases);

            Logln("Testing error conditions:");
            try
            {
                ICUResourceBundle.GetFunctionalEquivalent(ICUData.IcuBreakIteratorBaseName, assembly, "calendar",
                              "calendar", new UCultureInfo("ar_EG@calendar=islamic"), out bool isAvailable, true);
                Errln("Err: expected MissingManifestResourceException");
            }
            catch (MissingManifestResourceException t)
            {
                Logln("expected MissingManifestResourceException caught (PASS): " + t.ToString());
            }
        }

        private void getFunctionalEquivalentTestCases(string path, Assembly cl, string resName, string keyword,
                bool truncate, string[] testCases)
        {
            //string F_STR = "f";
            string T_STR = "t";

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

        [Test]
        public void TestNorwegian()
        {
            try
            {
                UResourceBundle rb = UResourceBundle.GetBundleInstance(ICUData.IcuRegionBaseName, "no_NO_NY");
                UResourceBundle sub = rb.Get("Countries");
                string s1 = sub.GetString("NO");
                if (s1.Equals("Noreg"))
                {
                    Logln("got expected output ");
                }
                else
                {
                    Errln("did not get the expected result");
                }
            }
            catch (ArgumentException ex)
            {
                Errln("Caught an unexpected expected");
            }
        }
        [Test]
        public void TestJB4102()
        {
            try
            {
                ICUResourceBundle root = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.IcuBaseName, "root");
                ICUResourceBundle t = null;
                // AmPmMarkers now exist in root/islamic calendar, so this test is rendered useless.
                //          try{
                //              t = root.getWithFallback("calendar/islamic-civil/AmPmMarkers");
                //              Errln("Second resource does not exist. How did it get here?\n");
                //          }catch(MissingManifestResourceException ex){
                //              Logln("Got the expected exception");
                //          }
                try
                {
                    t = root.GetWithFallback("calendar/islamic-civil/eras/abbreviated/0/mikimaus/pera");
                    Errln("Second resource does not exist. How did it get here?\n");
                }
                catch (MissingManifestResourceException ex)
                {
                    Logln("Got the expected exception");
                }
                if (t != null)
                {
                    Errln("t is not null!");
                }
            }
            catch (MissingManifestResourceException e)
            {
                Warnln("Could not load the locale data: " + e.ToString());
            }
        }

        [Test]
        public void TestCLDRStyleAliases()
        {
            string result = null;
            string expected = null;
            string[] expects = new string[] { "", "a41", "a12", "a03", "ar4" };

            Logln("Testing CLDR style aliases......\n");

            UResourceBundle rb = UResourceBundle.GetBundleInstance("Dev/Data/TestData", "te_IN_REVISED", testLoader);
            ICUResourceBundle alias = (ICUResourceBundle)rb.Get("a");

            for (int i = 1; i < 5; i++)
            {
                string resource = "a" + i;
                UResourceBundle a = (alias).GetWithFallback(resource);
                result = a.GetString();
                if (result.Equals(expected))
                {
                    Errln("CLDR style aliases failed resource with name " + resource + "resource, exp " + expects[i] + " , got " + result);
                }
            }

        }

        private class CoverageStub : UResourceBundle
        {
            public override UCultureInfo UCulture => UCultureInfo.InvariantCulture;
            protected internal override string LocaleID => null;
            protected internal override string BaseName => null;
            public override UResourceBundle Parent => null;

            public override IEnumerable<string> GetKeys() { return null; }
            protected override object HandleGetObject(string aKey) { return null; }
        }

        [Test]
        public void TestCoverage()
        {
            UResourceBundle bundle;
            bundle = UResourceBundle.GetBundleInstance(ICUData.IcuBaseName);
            if (bundle == null)
            {
                Errln("UResourceBundle.GetBundleInstance(String baseName) failed");
            }
            bundle = null;
            bundle = UResourceBundle.GetBundleInstance(UCultureInfo.CurrentCulture);
            if (bundle == null)
            {
                Errln("UResourceBundle.GetBundleInstance(UCultureInfo) failed");
                return;
            }
            if (new UResourceTypeMismatchException("coverage") == null)
            {
                Errln("Create UResourceTypeMismatchException error");
            }
            CoverageStub stub = new CoverageStub();

            if (!stub.Culture.Equals(CultureInfo.InvariantCulture))
            {
                Errln("UResourceBundle.getLoclae(CultureInfo) should delegate to (UCultureInfo)");
            }
        }
        [Test]
        [Ignore("ICU4N TOOD: Fix this")]
        public void TestJavaULocaleBundleLoading()
        {
            string baseName = "com.ibm.icu.dev.data.resources.TestDataElements";
            string locName = "en_Latn_US";
            UResourceBundle bundle = UResourceBundle.GetBundleInstance(baseName, locName, testLoader);
            string fromRoot = bundle.GetString("from_root");
            if (!fromRoot.Equals("This data comes from root"))
            {
                Errln("Did not get the expected string for from_root");
            }
            string fromEn = bundle.GetString("from_en");
            if (!fromEn.Equals("This data comes from en"))
            {
                Errln("Did not get the expected string for from_en");
            }
            string fromEnLatn = bundle.GetString("from_en_Latn");
            if (!fromEnLatn.Equals("This data comes from en_Latn"))
            {
                Errln("Did not get the expected string for from_en_Latn");
            }
            string fromEnLatnUs = bundle.GetString("from_en_Latn_US");
            if (!fromEnLatnUs.Equals("This data comes from en_Latn_US"))
            {
                Errln("Did not get the expected string for from_en_Latn_US");
            }
            UResourceBundle bundle1 = UResourceBundle.GetBundleInstance(baseName, new UCultureInfo(locName), testLoader);
            if (!bundle1.Equals(bundle))
            {
                Errln("Did not get the expected bundle for " + baseName + "." + locName);
            }
            if (bundle1 != bundle)
            {
                Errln("Did not load the bundle from cache");
            }

            UResourceBundle bundle2 = UResourceBundle.GetBundleInstance(baseName, "en_IN", testLoader);
            if (!bundle2.Culture.ToString().Equals("en"))
            {
                Errln("Did not get the expected fallback locale. Expected: en Got: " + bundle2.Culture.ToString());
            }
            UResourceBundle bundle3 = UResourceBundle.GetBundleInstance(baseName, "te_IN", testLoader);
            if (!bundle3.Culture.ToString().Equals("te"))
            {
                Errln("Did not get the expected fallback locale. Expected: te Got: " + bundle2.Culture.ToString());
            }
            // non-existent bundle .. should return default
            UResourceBundle defaultBundle = UResourceBundle.GetBundleInstance(baseName, "hi_IN", testLoader);
            UCultureInfo defaultLocale = UCultureInfo.CurrentCulture;
            if (!defaultBundle.UCulture.Equals(defaultLocale))
            {
                Errln("Did not get the default bundle for non-existent bundle");
            }
            // non-existent bundle, non-existent default locale
            // so return the root bundle.
            using (var context = new ThreadCultureChange("fr_CA", "fr_CA"))
            {
                UResourceBundle root = UResourceBundle.GetBundleInstance(baseName, "hi_IN", testLoader);
                if (!root.UCulture.ToString().Equals(""))
                {
                    Errln("Did not get the root bundle for non-existent default bundle for non-existent bundle");
                }
            } //reset the default
            using (var keys = bundle.GetKeys().GetEnumerator())
            {
                int i = 0;
                while (keys.MoveNext())
                {
                    Logln("key: " + keys.Current);
                    i++;
                }
                if (i != 4)
                {
                    Errln("Did not get the expected number of keys: got " + i + ", expected 4");
                }
            }
            UResourceBundle bundle4 = UResourceBundle.GetBundleInstance(baseName, "fr_Latn_FR", testLoader);
            if (bundle4 == null)
            {
                Errln("Could not load bundle fr_Latn_FR");
            }
        }
        [Test]
        public void TestAliasFallback()
        {
            try
            {
                UCultureInfo loc = new UCultureInfo("en_US");
                ICUResourceBundle b = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.IcuBaseName, loc);
                ICUResourceBundle b1 = b.GetWithFallback("calendar/hebrew/monthNames/format/abbreviated");
                if (b1 != null)
                {
                    Logln("loaded data for abbreviated month names: " + b1.Key);
                }
            }
            catch (MissingManifestResourceException ex)
            {
                Warnln("Failed to load data for abbreviated month names");
            }
        }
        private ISet<string> SetFromEnumeration(IEnumerable<string> e)
        {
            SortedSet<string> set = new SortedSet<string>();
            foreach (var item in e)
            {
                set.Add(item);
            }
            return set;
        }
        /**
         * Test ICUResourceBundle.getKeys() for a whole bundle (top-level resource).
         * JDK JavaDoc for ResourceBundle.getKeys() says that it returns
         * "an Enumeration of the keys contained in this ResourceBundle and its parent bundles."
         */
        [Test]
        public void TestICUGetKeysAtTopLevel()
        {
            string baseName = "Dev/Data/TestData";
            UResourceBundle te_IN = UResourceBundle.GetBundleInstance(baseName, "te_IN", testLoader);
            UResourceBundle te = UResourceBundle.GetBundleInstance(baseName, "te", testLoader);
            ISet<string> te_set = SetFromEnumeration(te.GetKeys());
            ISet<string> te_IN_set = SetFromEnumeration(te_IN.GetKeys());
            assertTrue("te.getKeys().contains(string_only_in_Root)", te_set.Contains("string_only_in_Root"));
            assertTrue("te.getKeys().contains(string_only_in_te)", te_set.Contains("string_only_in_te"));
            assertFalse("te.getKeys().contains(string_only_in_te_IN)", te_set.Contains("string_only_in_te_IN"));
            assertTrue("te_IN.getKeys().contains(string_only_in_Root)", te_IN_set.Contains("string_only_in_Root"));
            assertTrue("te_IN.getKeys().contains(string_only_in_te)", te_IN_set.Contains("string_only_in_te"));
            assertTrue("te_IN.getKeys().contains(string_only_in_te_IN)", te_IN_set.Contains("string_only_in_te_IN"));
            // TODO: Check for keys of alias resource items
        }
        /**
         * Test ICUResourceBundle.getKeys() for a resource item (not a whole bundle/top-level resource).
         * This does not take parent bundles into account.
         */
        [Test]
        public void TestICUGetKeysForResourceItem()
        {
            string baseName = "Dev/Data/TestData";
            UResourceBundle te = UResourceBundle.GetBundleInstance(baseName, "te", testLoader);
            UResourceBundle tagged_array_in_Root_te = te.Get("tagged_array_in_Root_te");
            ISet<string> keys = SetFromEnumeration(tagged_array_in_Root_te.GetKeys());
            assertTrue("tagged_array_in_Root_te.getKeys().contains(tag0)", keys.Contains("tag0"));
            assertTrue("tagged_array_in_Root_te.getKeys().contains(tag1)", keys.Contains("tag1"));
            assertFalse("tagged_array_in_Root_te.getKeys().contains(tag7)", keys.Contains("tag7"));
            assertFalse("tagged_array_in_Root_te.getKeys().contains(tag12)", keys.Contains("tag12"));
            UResourceBundle array_in_Root_te = te.Get("array_in_Root_te");
            assertFalse("array_in_Root_te.GetKeys().GetEnumerator().MoveNext()", array_in_Root_te.GetKeys().GetEnumerator().MoveNext());
            UResourceBundle string_in_Root_te = te.Get("string_in_Root_te");
            assertFalse("string_in_Root_te.GetKeys().GetEnumerator().MoveNext()", string_in_Root_te.GetKeys().GetEnumerator().MoveNext());
        }

        /*
         * UResouceBundle should be able to load a resource bundle even if
         * a similarly named class (only case differences) exists in the
         * same package.  See Ticket#6844
         */
        [Test]
        [Ignore("ICU4N TOOD: Fix this")]
        public void TestT6844()
        {
            try
            {
                UResourceBundle rb1
                    = UResourceBundle.GetBundleInstance("com.ibm.icu.dev.data.resources.TestMessages", UCultureInfo.CurrentCulture, testLoader);
                assertEquals("bundleContainer in TestMessages", "TestMessages.class", rb1.GetString("bundleContainer"));

                UResourceBundle rb2
                    = UResourceBundle.GetBundleInstance("com.ibm.icu.dev.data.resources.testmessages", UCultureInfo.CurrentCulture, testLoader);
                assertEquals("bundleContainer in testmessages", "testmessages.properties", rb2.GetString("bundleContainer"));
            }
            catch (Exception t)
            {
                Errln(t.ToString());
            }
        }

        [Test]
        [Ignore("ICU4N TOOD: Fix this")]
        public void TestUResourceBundleCoverage()
        {
            CultureInfo locale = null;
            UCultureInfo ulocale = null;
            string baseName = null;
            UResourceBundle rb1, rb2, rb3, rb4, rb5, rb6, rb7;

            rb1 = UResourceBundle.GetBundleInstance(ulocale);
            rb2 = UResourceBundle.GetBundleInstance(baseName);
            rb3 = UResourceBundle.GetBundleInstance(baseName, ulocale);
            rb4 = UResourceBundle.GetBundleInstance(baseName, locale);

            rb5 = UResourceBundle.GetBundleInstance(baseName, ulocale, testLoader);
            rb6 = UResourceBundle.GetBundleInstance(baseName, locale, testLoader);
            try
            {
                rb7 = UResourceBundle.GetBundleInstance("bogus", CultureInfo.CurrentCulture, testLoader);
                Errln("Should have thrown exception with bogus baseName.");
            }
            catch (MissingManifestResourceException ex)
            {
            }
            if (rb1 == null || rb2 == null || rb3 == null || rb4 == null || rb5 == null || rb6 == null)
            {
                Errln("Error getting resource bundle.");
            }

            rb7 = UResourceBundle.GetBundleInstance("com.ibm.icu.dev.data.resources.TestDataElements", CultureInfo.CurrentCulture, testLoader);

            try
            {
                rb1.GetBinary();
                Errln("getBinary() call should have thrown UResourceTypeMismatchException.");
            }
            catch (UResourceTypeMismatchException ex)
            {
            }
            try
            {
                rb1.GetStringArray();
                Errln("getStringArray() call should have thrown UResourceTypeMismatchException.");
            }
            catch (UResourceTypeMismatchException ex)
            {
            }
            try
            {
                byte[] ba = { 0x00 };
                rb1.GetBinary(ba);
                Errln("getBinary(byte[]) call should have thrown UResourceTypeMismatchException.");
            }
            catch (UResourceTypeMismatchException ex)
            {
            }
            try
            {
                rb1.GetInt32();
                Errln("getInt() call should have thrown UResourceTypeMismatchException.");
            }
            catch (UResourceTypeMismatchException ex)
            {
            }
            try
            {
                rb1.GetInt32Vector();
                Errln("getIntVector() call should have thrown UResourceTypeMismatchException.");
            }
            catch (UResourceTypeMismatchException ex)
            {
            }
            try
            {
                rb1.GetUInt32();
                Errln("getUInt() call should have thrown UResourceTypeMismatchException.");
            }
            catch (UResourceTypeMismatchException ex)
            {
            }
            if (rb1.Version != null)
            {
                Errln("getVersion() call should have returned null.");
            }
            if (rb7.Type != UResourceType.None)
            {
                Errln("getType() call should have returned NONE.");
            }
            if (rb7.Key != null)
            {
                Errln("getKey() call should have returned null.");
            }
            if (((ICUResourceBundle)rb1).FindTopLevel(0) == null)
            {
                Errln("Error calling findTopLevel().");
            }
            if (ICUResourceBundle.GetFullLocaleNameSet() == null)
            {
                Errln("Error calling getFullLocaleNameSet().");
            }
            UResourceBundleEnumerator itr = rb1.GetEnumerator();
            while (itr.MoveNext())
            {
            }
            // ICU4N specific - we don't check for a NoSuchElementException in .NET, we simply
            // check to see if the enum returns false. In the case of NextString(), the intended
            // usage is to use itr.Current.GetString() instead.

            //    try
            //    {
            //        itr.next();
            //        Errln("NoSuchElementException exception should have been thrown.");
            //    }
            //    catch (NoSuchElementException ex)
            //    {
            //    }
            //    try
            //    {
            //        itr.nextString();
            //        Errln("NoSuchElementException exception should have been thrown.");
            //    }
            //    catch (NoSuchElementException ex)
            //    {
            //    }
        }

        [Test]
        public void TestAddLocaleIDsFromSatelliteFolderNames()
        {
            // Load raw list data for the root path
            var expected = new HashSet<string>();
            string baseName = ICUData.IcuBaseName.EndsWith("/", StringComparison.Ordinal) ? ICUData.IcuBaseName : ICUData.IcuBaseName + "/";
            ICUResourceBundle.AddLocaleIDsFromListFile(baseName, ICUResourceBundle.IcuDataAssembly, expected);
            expected.Remove("root");

            // Run the scan of directories to get the full list of cultures
            var cultures = new HashSet<string>();
            ICUResourceBundle.AddLocaleIDsFromSatelliteAndGACFolderNames($"data/{ICUData.PackageName}/", ICUResourceBundle.IcuDataAssembly, cultures);

            // Exclude the originals so we can see what went wrong.
            expected.ExceptWith(cultures);
            assertFalse($"Missing culture values: {string.Join(", ", expected)}", expected.Count > 0);
        }
    }
}
