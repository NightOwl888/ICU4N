using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Text;
using ICU4N.Util;
using J2N;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using JCG = J2N.Collections.Generic;

namespace ICU4N.Dev.Test.Util
{
    /// <summary>
    /// General test of Currency
    /// </summary>
    public class CurrencyTest : TestFmwk
    {
        /**
         * Test of basic API.
         */
        [Test]
        public void TestAPI()
        {
            Currency usd = Currency.GetInstance("USD");
            /*int hash = */
            usd.GetHashCode();
            Currency jpy = Currency.GetInstance("JPY");
            if (usd.Equals(jpy))
            {
                Errln("FAIL: USD == JPY");
            }
            if (usd.Equals("abc"))
            {
                Errln("FAIL: USD == (String)");
            }
            if (usd.Equals(null))
            {
                Errln("FAIL: USD == (null)");
            }
            if (!usd.Equals(usd))
            {
                Errln("FAIL: USD != USD");
            }

            try
            {
                Currency nullCurrency = Currency.GetInstance((string)null);
                Errln("FAIL: Expected getInstance(null) to throw "
                        + "a NullPointerException, but returned " + nullCurrency);
            }
            catch (ArgumentNullException npe)
            {
                Logln("PASS: getInstance(null) threw a NullPointerException");
            }

            try
            {
                Currency bogusCurrency = Currency.GetInstance("BOGUS");
                Errln("FAIL: Expected getInstance(\"BOGUS\") to throw "
                        + "an IllegalArgumentException, but returned " + bogusCurrency);
            }
            catch (ArgumentException iae)
            {
                Logln("PASS: getInstance(\"BOGUS\") threw an IllegalArgumentException");
            }

            CultureInfo[] avail = Currency.GetCultures(UCultureTypes.AllCultures);
            if (avail == null)
            {
                Errln("FAIL: getAvailableLocales returned null");
            }

            try
            {
                usd.GetName(new UCultureInfo("en-US"), (CurrencyNameStyle)5, out bool _);
                Errln("expected getName with invalid type parameter to throw exception");
            }
            catch (Exception e)
            {
                Logln("PASS: getName failed as expected");
            }
        }

        /**
         * Test registration.
         */
        [Test]
        public void TestRegistration()
        {
            Currency jpy = Currency.GetInstance("JPY");
            Currency usd = Currency.GetInstance(new CultureInfo("en-US"));

            try
            {
                Currency.Unregister(null); // should fail, coverage
                Errln("expected unregister of null to throw exception");
            }
            catch (Exception e)
            {
                Logln("PASS: unregister of null failed as expected");
            }

            if (Currency.Unregister(""))
            { // coverage
                Errln("unregister before register erroneously succeeded");
            }

            UCultureInfo fu_FU = new UCultureInfo("fu_FU");

            Object key1 = Currency.RegisterInstance(jpy, new UCultureInfo("en-US"));
            Object key2 = Currency.RegisterInstance(jpy, fu_FU);

            Currency nus = Currency.GetInstance(new CultureInfo("en-US"));
            if (!nus.Equals(jpy))
            {
                Errln("expected " + jpy + " but got: " + nus);
            }

            // converage, make sure default factory works
            Currency nus1 = Currency.GetInstance(new CultureInfo("ja-JP"));
            if (!nus1.Equals(jpy))
            {
                Errln("expected " + jpy + " but got: " + nus1);
            }

            UCultureInfo[] locales = Currency.GetUCultures(UCultureTypes.AllCultures);
            bool found = false;
            for (int i = 0; i < locales.Length; ++i)
            {
                if (locales[i].Equals(fu_FU))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                Errln("did not find locale" + fu_FU + " in currency locales");
            }

            if (!Currency.Unregister(key1))
            {
                Errln("unable to unregister currency using key1");
            }
            if (!Currency.Unregister(key2))
            {
                Errln("unable to unregister currency using key2");
            }

            Currency nus2 = Currency.GetInstance(new CultureInfo("en-US"));
            if (!nus2.Equals(usd))
            {
                Errln("expected " + usd + " but got: " + nus2);
            }

            locales = Currency.GetUCultures(UCultureTypes.AllCultures);
            found = false;
            for (int i = 0; i < locales.Length; ++i)
            {
                if (locales[i].Equals(fu_FU))
                {
                    found = true;
                    break;
                }
            }
            if (found)
            {
                Errln("found locale" + fu_FU + " in currency locales after unregister");
            }

            CultureInfo[] locs = Currency.GetCultures(UCultureTypes.AllCultures);
            found = false;
            for (int i = 0; i < locs.Length; ++i)
            {
                if (locs[i].Equals(fu_FU))
                {
                    found = true;
                    break;
                }
            }
            if (found)
            {
                Errln("found locale" + fu_FU + " in currency locales after unregister");
            }
        }

        /**
         * Test names.
         */
        [Test]
        public void TestNames()
        {
            // Do a basic check of getName()
            // USD { "US$", "US Dollar"            } // 04/04/1792-
            UCultureInfo en = new UCultureInfo("en");
            bool isChoiceFormat;
            Currency usd = Currency.GetInstance("USD");
            // Warning: HARD-CODED LOCALE DATA in this test.  If it fails, CHECK
            // THE LOCALE DATA before diving into the code.
            assertEquals("USD.getName(SYMBOL_NAME)",
                    "$",
                    usd.GetName(en, Currency.SymbolName, out isChoiceFormat));
            assertEquals("USD.getName(LONG_NAME)",
                    "US Dollar",
                    usd.GetName(en, Currency.LongName, out isChoiceFormat));
            // TODO add more tests later
        }

        [Test]
        public void testGetName_Locale_Int_String_BooleanArray()
        {
            Currency currency = Currency.GetInstance(new UCultureInfo("zh_Hans_CN"));
            bool isChoiceFormat;
            CurrencyNameStyle nameStyle = Currency.LongName;
            String pluralCount = "";
            String ulocaleName =
                    currency.GetName(new UCultureInfo("en-CA"), nameStyle, pluralCount, out isChoiceFormat);
            assertEquals("currency name mismatch", "Chinese Yuan", ulocaleName);
            String localeName = currency.GetName(new CultureInfo("en-CA"), nameStyle, pluralCount, out isChoiceFormat);
            assertEquals("currency name mismatch", ulocaleName, localeName);
        }

        [Test]
        public void TestCoverage()
        {
            Currency usd = Currency.GetInstance("USD");
            assertEquals("USD.getSymbol()",
                    "$",
                    usd.GetSymbol());
        }

        // A real test of the CurrencyDisplayNames class.
        [Test]
        public void TestCurrencyDisplayNames()
        {
            if (!CurrencyDisplayNames.HasData)
            {
                Errln("hasData() should return true.");
            }

            // with substitute
            CurrencyDisplayNames cdn = CurrencyDisplayNames.GetInstance(new UCultureInfo("de-DE"));
            assertEquals("de_USD_name", "US-Dollar", cdn.GetName("USD"));
            assertEquals("de_USD_symbol", "$", cdn.GetSymbol("USD"));
            assertEquals("de_USD_plural_other", "US-Dollar", cdn.GetPluralName("USD", "other"));
            // unknown plural category, substitute "other"
            assertEquals("de_USD_plural_foo", "US-Dollar", cdn.GetPluralName("USD", "foo"));

            cdn = CurrencyDisplayNames.GetInstance(new UCultureInfo("en-US"));
            assertEquals("en-US_USD_name", "US Dollar", cdn.GetName("USD"));
            assertEquals("en-US_USD_symbol", "$", cdn.GetSymbol("USD"));
            assertEquals("en-US_USD_plural_one", "US dollar", cdn.GetPluralName("USD", "one"));
            assertEquals("en-US_USD_plural_other", "US dollars", cdn.GetPluralName("USD", "other"));

            assertEquals("en-US_FOO_name", "FOO", cdn.GetName("FOO"));
            assertEquals("en-US_FOO_symbol", "FOO", cdn.GetSymbol("FOO"));
            assertEquals("en-US_FOO_plural_other", "FOO", cdn.GetPluralName("FOO", "other"));

            assertEquals("en-US bundle", "en", cdn.UCulture.ToString());

            cdn = CurrencyDisplayNames.GetInstance(new UCultureInfo("zz-Gggg-YY"));
            assertEquals("bundle from current locale", "en", cdn.UCulture.ToString());

            // with no substitute
            cdn = CurrencyDisplayNames.GetInstance(new UCultureInfo("de-DE"), true);
            assertNotNull("have currency data for Germany", cdn);

            // known currency, behavior unchanged
            assertEquals("de_USD_name", "US-Dollar", cdn.GetName("USD"));
            assertEquals("de_USD_symbol", "$", cdn.GetSymbol("USD"));
            assertEquals("de_USD_plural_other", "US-Dollar", cdn.GetPluralName("USD", "other"));

            // known currency but unknown plural category
            assertNull("de_USD_plural_foo", cdn.GetPluralName("USD", "foo"));

            // unknown currency, get null
            assertNull("de_FOO_name", cdn.GetName("FOO"));
            assertNull("de_FOO_symbol", cdn.GetSymbol("FOO"));
            assertNull("de_FOO_plural_other", cdn.GetPluralName("FOO", "other"));
            assertNull("de_FOO_plural_foo", cdn.GetPluralName("FOO", "foo"));

            // unknown locale with no substitute
            cdn = CurrencyDisplayNames.GetInstance(new UCultureInfo("zz-Gggg-YY"), true);
            String ln = "";
            if (cdn != null)
            {
                ln = " (" + cdn.UCulture.ToString() + ")";
            }
            assertNull("no fallback from unknown locale" + ln, cdn);

            // Locale version
            cdn = CurrencyDisplayNames.GetInstance(new CultureInfo("de-DE"), true);
            assertNotNull("have currency data for Germany (Java Locale)", cdn);
            assertEquals("de_USD_name (Locale)", "US-Dollar", cdn.GetName("USD"));
            assertNull("de_FOO_name (Locale)", cdn.GetName("FOO"));
        }

        // Coverage-only test of CurrencyData
        [Test]
        public void TestCurrencyData()
        {
            DefaultCurrencyDisplayInfo info_fallback = (DefaultCurrencyDisplayInfo)DefaultCurrencyDisplayInfo.GetWithFallback(true);
            if (info_fallback == null)
            {
                Errln("getWithFallback() returned null.");
                return;
            }

            DefaultCurrencyDisplayInfo info_nofallback = (DefaultCurrencyDisplayInfo)DefaultCurrencyDisplayInfo.GetWithFallback(false);
            if (info_nofallback == null)
            {
                Errln("getWithFallback() returned null.");
                return;
            }

            if (!info_fallback.GetName("isoCode").Equals("isoCode") || info_nofallback.GetName("isoCode") != null)
            {
                Errln("Error calling getName().");
                return;
            }

            if (!info_fallback.GetPluralName("isoCode", "type").Equals("isoCode") || info_nofallback.GetPluralName("isoCode", "type") != null)
            {
                Errln("Error calling getPluralName().");
                return;
            }

            if (!info_fallback.GetSymbol("isoCode").Equals("isoCode") || info_nofallback.GetSymbol("isoCode") != null)
            {
                Errln("Error calling getSymbol().");
                return;
            }

            if (info_fallback.SymbolMap.Count != 0)
            {
                Errln("symbolMap() should return empty map.");
                return;
            }

            if (info_fallback.NameMap.Count != 0)
            {
                Errln("nameMap() should return empty map.");
                return;
            }

            if (info_fallback.GetUnitPatterns().Count != 0 || info_nofallback.GetUnitPatterns() != null)
            {
                Errln("Error calling getUnitPatterns().");
                return;
            }

            if (!info_fallback.GetSpacingInfo().Equals((CurrencySpacingInfo.Default)) ||
                    info_nofallback.GetSpacingInfo() != null)
            {
                Errln("Error calling getSpacingInfo().");
                return;
            }

            if (info_fallback.UCulture != UCultureInfo.InvariantCulture)
            {
                Errln("Error calling getLocale().");
                return;
            }

            if (info_fallback.GetFormatInfo("isoCode") != null)
            {
                Errln("Error calling getFormatInfo().");
                return;
            }
        }

        // A real test of CurrencyMetaInfo.
        [Test]
        public void testCurrencyMetaInfoRanges()
        {
            CurrencyMetaInfo metainfo = CurrencyMetaInfo.GetInstance(true);
            assertNotNull("have metainfo", metainfo);

            CurrencyFilter filter = CurrencyFilter.OnRegion("DE"); // must be capitalized
            IList<CurrencyInfo> currenciesInGermany = metainfo.CurrencyInfo(filter);
            Logln("currencies: " + currenciesInGermany.Count);

            // ICU4N TODO: When we have TimeZone and SimpleDateFormat, we can switch this to using the local
            // ICU4N classes instead of those of .NET

            //com.ibm.icu.text.DateFormat fmt = new com.ibm.icu.text.SimpleDateFormat("yyyy-MM-dd HH:mm:ss.SSS z");
            //fmt.setTimeZone(com.ibm.icu.util.TimeZone.getTimeZone("GMT"));

            //java.util.Date demLastDate = new java.util.Date(java.lang.Long.MAX_VALUE);
            //java.util.Date eurFirstDate = new java.util.Date(java.lang.Long.MIN_VALUE);
            DateTime demLastDate = JavaDateToDotNetDateTime(long.MaxValue);
            DateTime eurFirstDate = JavaDateToDotNetDateTime(long.MinValue);
            foreach (CurrencyInfo info in currenciesInGermany)
            {
                Logln(info.ToString());
                //Logln("from: " + fmt.format(info.From) + info.From.ToHexString());
                //Logln("  to: " + fmt.format(info.To) + info.To.ToHexString());
                Logln("from: " + JavaDateToDotNetDateTimeOffset(info.From).ToString("yyyy-MM-dd HH:mm:ss.fff zzz ") + info.From.ToHexString());
                Logln("  to: " + JavaDateToDotNetDateTimeOffset(info.To).ToString("yyyy-MM-dd HH:mm:ss.fff zzz ") + info.To.ToHexString());
                if (info.Code.Equals("DEM", StringComparison.Ordinal))
                {
                    //demLastDate = new java.util.Date(info.To);
                    demLastDate = JavaDateToDotNetDateTime(info.To);
                }
                else if (info.Code.Equals("EUR", StringComparison.Ordinal))
                {
                    //eurFirstDate = new java.util.Date(info.From);
                    eurFirstDate = JavaDateToDotNetDateTime(info.From);
                }
            }

            // the Euro and Deutschmark overlapped for several years
            //assertEquals("DEM available at last date", 2, metainfo.CurrencyInfo(filter.WithDate(JavaDateToDotNetDateTime(demLastDate.getTime()))).Count);
            assertEquals("DEM available at last date", 2, metainfo.CurrencyInfo(filter.WithDate(demLastDate)).Count);

            // demLastDate + 1 millisecond is not the start of the last day, we consider it the next day, so...
            //java.util.Date demLastDatePlus1ms = new java.util.Date(demLastDate.getTime() + 1);
            //assertEquals("DEM not available after very start of last date", 1, metainfo.CurrencyInfo(filter.WithDate(JavaDateToDotNetDateTime(demLastDatePlus1ms.getTime()))).Count);

            DateTime demLastDatePlus1ms = demLastDate.AddMilliseconds(1);
            assertEquals("DEM not available after very start of last date", 1, metainfo.CurrencyInfo(filter.WithDate(demLastDatePlus1ms)).Count);

            // both available for start of euro
            //assertEquals("EUR available on start of first date", 2, metainfo.CurrencyInfo(filter.WithDate(JavaDateToDotNetDateTime(eurFirstDate.getTime()))).Count);
            assertEquals("EUR available on start of first date", 2, metainfo.CurrencyInfo(filter.WithDate(eurFirstDate)).Count);

            // but not one millisecond before the start of the first day
            //java.util.Date eurFirstDateMinus1ms = new java.util.Date(eurFirstDate.getTime() - 1);
            //assertEquals("EUR not avilable before very start of first date", 1, metainfo.CurrencyInfo(filter.WithDate(JavaDateToDotNetDateTime(eurFirstDateMinus1ms.getTime()))).Count);

            DateTime eurFirstDateMinus1ms = eurFirstDate.AddMilliseconds(-1);
            assertEquals("EUR not avilable before very start of first date", 1, metainfo.CurrencyInfo(filter.WithDate(eurFirstDateMinus1ms)).Count);

            // end time is last millisecond of day
            //java.util.GregorianCalendar cal = new java.util.GregorianCalendar();
            //cal.setTimeZone(java.util.TimeZone.getTimeZone("GMT"));
            //cal.setTime(demLastDate);
            //assertEquals("hour is 23", 23, cal.get(java.util.GregorianCalendar.HOUR_OF_DAY));
            //assertEquals("minute is 59", 59, cal.get(java.util.GregorianCalendar.MINUTE));
            //assertEquals("second is 59", 59, cal.get(java.util.GregorianCalendar.SECOND));
            //assertEquals("millisecond is 999", 999, cal.get(java.util.GregorianCalendar.MILLISECOND));

            assertEquals("hour is 23", 23, demLastDate.Hour);
            assertEquals("minute is 59", 59, demLastDate.Minute);
            assertEquals("second is 59", 59, demLastDate.Second);
            assertEquals("millisecond is 999", 999, demLastDate.Millisecond);

            //// start time is first millisecond of day
            //cal.setTime(eurFirstDate);
            //assertEquals("hour is 0", 0, cal.get(java.util.GregorianCalendar.HOUR_OF_DAY));
            //assertEquals("minute is 0", 0, cal.get(java.util.GregorianCalendar.MINUTE));
            //assertEquals("second is 0", 0, cal.get(java.util.GregorianCalendar.SECOND));
            //assertEquals("millisecond is 0", 0, cal.get(java.util.GregorianCalendar.MILLISECOND));

            assertEquals("hour is 0", 0, eurFirstDate.Hour);
            assertEquals("minute is 0", 0, eurFirstDate.Minute);
            assertEquals("second is 0", 0, eurFirstDate.Second);
            assertEquals("millisecond is 0", 0, eurFirstDate.Millisecond);
        }

        private static DateTime JavaDateToDotNetDateTime(long getTimeResult)
        {
            if (getTimeResult < DateTimeOffsetUtil.MinMilliseconds)
                getTimeResult = DateTimeOffsetUtil.MinMilliseconds;
            if (getTimeResult > DateTimeOffsetUtil.MaxMilliseconds)
                getTimeResult = DateTimeOffsetUtil.MaxMilliseconds;

            return new DateTime(DateTimeOffsetUtil.GetTicksFromUnixTimeMilliseconds(getTimeResult), DateTimeKind.Utc);
        }

        private static DateTimeOffset JavaDateToDotNetDateTimeOffset(long getTimeResult)
        {
            if (getTimeResult < DateTimeOffsetUtil.MinMilliseconds)
                getTimeResult = DateTimeOffsetUtil.MinMilliseconds;
            if (getTimeResult > DateTimeOffsetUtil.MaxMilliseconds)
                getTimeResult = DateTimeOffsetUtil.MaxMilliseconds;

            return new DateTimeOffset(DateTimeOffsetUtil.GetTicksFromUnixTimeMilliseconds(getTimeResult), TimeSpan.Zero);
        }

        [Test]
        public void testCurrencyMetaInfoRangesWithLongs()
        {
            CurrencyMetaInfo metainfo = CurrencyMetaInfo.GetInstance(true);
            assertNotNull("have metainfo", metainfo);

            CurrencyFilter filter = CurrencyFilter.OnRegion("DE"); // must be capitalized
            IList<CurrencyInfo> currenciesInGermany = metainfo.CurrencyInfo(filter);
            CurrencyFilter filter_br = CurrencyFilter.OnRegion("BR"); // must be capitalized
            IList<CurrencyInfo> currenciesInBrazil = metainfo.CurrencyInfo(filter_br);
            Logln("currencies Germany: " + currenciesInGermany.Count);
            Logln("currencies Brazil: " + currenciesInBrazil.Count);
            long demFirstDate = long.MinValue;
            long demLastDate = long.MaxValue;
            long eurFirstDate = long.MinValue;
            CurrencyInfo demInfo = null;
            foreach (CurrencyInfo info in currenciesInGermany)
            {
                Logln(info.ToString());
                if (info.Code.Equals("DEM", StringComparison.Ordinal))
                {
                    demInfo = info;
                    demFirstDate = info.From;
                    demLastDate = info.To;
                }
                else if (info.Code.Equals("EUR", StringComparison.Ordinal))
                {
                    eurFirstDate = info.From;
                }
            }
            // the Euro and Deutschmark overlapped for several years
            assertEquals("DEM available at last date", 2, metainfo.CurrencyInfo(filter.WithDate(demLastDate)).Count);

            // demLastDate + 1 millisecond is not the start of the last day, we consider it the next day, so...
            long demLastDatePlus1ms = demLastDate + 1;
            assertEquals("DEM not available after very start of last date", 1, metainfo.CurrencyInfo(filter.WithDate(demLastDatePlus1ms)).Count);

            // both available for start of euro
            assertEquals("EUR available on start of first date", 2, metainfo.CurrencyInfo(filter.WithDate(eurFirstDate)).Count);

            // but not one millisecond before the start of the first day
            long eurFirstDateMinus1ms = eurFirstDate - 1;
            assertEquals("EUR not avilable before very start of first date", 1,
                         metainfo.CurrencyInfo(filter.WithDate(eurFirstDateMinus1ms)).Count);

            // Deutschmark available from first millisecond on
            assertEquals("Millisecond of DEM Big Bang", 1,
                         metainfo.CurrencyInfo(CurrencyFilter.OnDate(demFirstDate).WithRegion("DE")).Count);

            assertEquals("From Deutschmark to Euro", 2,
                         metainfo.CurrencyInfo(CurrencyFilter.OnDateRange(demFirstDate, eurFirstDate).WithRegion("DE")).Count);

            assertEquals("all Tender for Brazil", 7,
                    metainfo.CurrencyInfo(CurrencyFilter.OnTender().WithRegion("BR")).Count);

            assertTrue("No legal tender", demInfo.IsTender);
        }

        [Test]
        public void TestWithTender()
        {
            CurrencyMetaInfo metainfo = CurrencyMetaInfo.GetInstance();
            if (metainfo == null)
            {
                Errln("Unable to get CurrencyMetaInfo instance.");
                return;
            }
            CurrencyFilter filter =
                    CurrencyFilter.OnRegion("CH");
            IList<string> currencies = metainfo.Currencies(filter);
            assertTrue("More than one currency for switzerland", currencies.Count > 1);
            assertEquals(
                    "With tender",
                    new String[] { "CHF", "CHE", "CHW" },
                    metainfo.Currencies(filter.WithTender()));
        }

        // Coverage-only test of the CurrencyMetaInfo class
        [Test]
        public void TestCurrencyMetaInfo2()
        {
            CurrencyMetaInfo metainfo = CurrencyMetaInfo.GetInstance();
            if (metainfo == null)
            {
                Errln("Unable to get CurrencyMetaInfo instance.");
                return;
            }

            if (!CurrencyMetaInfo.HasData)
            {
                Errln("hasData() should note return false.");
                return;
            }

            CurrencyFilter filter;
            CurrencyInfo info;
            CurrencyDigits digits;

            { // CurrencyFilter
                filter = CurrencyFilter.OnCurrency("currency");
                CurrencyFilter filter2 = CurrencyFilter.OnCurrency("test");
                if (filter == null)
                {
                    Errln("Unable to create CurrencyFilter.");
                    return;
                }

                if (filter.Equals(new Object()))
                {
                    Errln("filter should not equal to Object");
                    return;
                }

                if (filter.Equals(filter2))
                {
                    Errln("filter should not equal filter2");
                    return;
                }

                if (filter.GetHashCode() == 0)
                {
                    Errln("Error getting filter hashcode");
                    return;
                }

                if (filter.ToString() == null)
                {
                    Errln("Error calling toString()");
                    return;
                }
            }

            { // CurrencyInfo
                info = new CurrencyInfo("region", "code", 0, 1, 1, false);
                if (info == null)
                {
                    Errln("Error creating CurrencyInfo.");
                    return;
                }

                if (info.ToString() == null)
                {
                    Errln("Error calling toString()");
                    return;
                }
            }

            { // CurrencyDigits
                digits = metainfo.CurrencyDigits("isoCode");
                if (digits == null)
                {
                    Errln("Unable to get CurrencyDigits.");
                    return;
                }

                if (digits.ToString() == null)
                {
                    Errln("Error calling toString()");
                    return;
                }
            }
        }

        [Test]
        public void TestCurrencyKeyword()
        {
            UCultureInfo locale = new UCultureInfo("th_TH@collation=traditional;currency=QQQ");
            Currency currency = Currency.GetInstance(locale);
            String result = currency.CurrencyCode;
            if (!"QQQ".Equals(result))
            {
                Errln("got unexpected currency: " + result);
            }
        }

        // ICU4N TODO: Missing dependency on SimpleDateFormat
        // This test can be changed to use the local one once it is ported.
#if FEATURE_IKVM
        [Test]
        public void TestAvailableCurrencyCodes()
        {
            string[][] tests = new string[][] {
                new string[] { "eo_AM", "1950-01-05" },
                new string[] { "eo_AM", "1969-12-31", "SUR" },
                new string[] { "eo_AM", "1991-12-26", "RUR" },
                new string[] { "eo_AM", "2000-12-23", "AMD" },
                new string[] { "eo_AD", "2000-12-23", "EUR", "ESP", "FRF", "ADP" },
                new string[] { "eo_AD", "1969-12-31", "ESP", "FRF", "ADP" },
                new string[] { "eo_AD", "1950-01-05", "ESP", "ADP" },
                new string[] { "eo_AD", "1900-01-17", "ESP" },
                new string[] { "eo_UA", "1994-12-25" },
                new string[] { "eo_QQ", "1969-12-31" },
                new string[] { "eo_AO", "2000-12-23", "AOA" },
                new string[] { "eo_AO", "1995-12-25", "AOR", "AON" },
                new string[] { "eo_AO", "1990-12-26", "AON", "AOK" },
                new string[] { "eo_AO", "1979-12-29", "AOK" },
                new string[] { "eo_AO", "1969-12-31" },
                new string[] { "eo_DE@currency=DEM", "2000-12-23", "EUR", "DEM" },
                new string[] { "eo-DE-u-cu-dem", "2000-12-23", "EUR", "DEM" },
                new string[] { "en_US", null, "USD", "USN" },
                new string[] { "en_US_PREEURO", null, "USD", "USN" },
                new string[] { "en_US_Q", null, "USD", "USN" },
            };

            com.ibm.icu.text.DateFormat fmt = new com.ibm.icu.text.SimpleDateFormat("yyyy-MM-dd", com.ibm.icu.util.ULocale.US);
            foreach (String[] test in tests)
            {
                UCultureInfo locale = new UCultureInfo(test[0]);
                String timeString = test[1];
                DateTime date;
                if (timeString == null)
                {
                    date = DateTime.UtcNow;
                    timeString = "today";
                }
                else
                {
                    
                    try
                    {
                        var dateJava = fmt.parse(timeString);
                        date = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(DateTimeOffsetUtil.GetTicksFromUnixTimeMilliseconds(dateJava.getTime()), DateTimeKind.Utc), TimeZoneInfo.Local);
                    }
                    catch (Exception e)
                    {
                        fail("could not parse date: " + timeString);
                        continue;
                    }
                }
                String[] expected = null;
                if (test.Length > 2)
                {
                    expected = new String[test.Length - 2];
                    Array.Copy(test, 2, expected, 0, expected.Length);
                }
                String[] actual = Currency.GetAvailableCurrencyCodes(locale, date);

                // Order is not important as of 4.4.  We never documented that it was.
                ISet<string> expectedSet = new JCG.HashSet<string>();
                if (expected != null)
                {
                    expectedSet.UnionWith(expected);
                }
                ISet<string> actualSet = new JCG.HashSet<string>();
                if (actual != null)
                {
                    actualSet.UnionWith(actual);
                }
                assertEquals(locale + " on " + timeString, expectedSet, actualSet);

                // With Java Locale
                // Note: skip this test on Java 6 or older when keywords are available
                //if (locale.getKeywords() == null || TestUtil.getJavaVendor() == JavaVendor.Android || TestUtil.getJavaVersion() >= 7)
                {
                    CultureInfo javaloc = locale.ToCultureInfo();// .toLocale();
                    String[] actualWithJavaLocale = Currency.GetAvailableCurrencyCodes(javaloc, date);
                    // should be exactly same with the ULocale version
                    bool same = true;
                    if (actual == null)
                    {
                        if (actualWithJavaLocale != null)
                        {
                            same = false;
                        }
                    }
                    else
                    {
                        if (actualWithJavaLocale == null || actual.Length != actualWithJavaLocale.Length)
                        {
                            same = false;
                        }
                        else
                        {
                            same = true;
                            for (int i = 0; i < actual.Length; i++)
                            {
                                if (!actual[i].Equals(actualWithJavaLocale[i]))
                                {
                                    same = false;
                                    break;
                                }
                            }
                        }
                    }
                    assertTrue("getAvailableCurrencyCodes with ULocale vs Locale", same);
                }
            }
        }
#endif

        [Test]
        public void TestDeprecatedCurrencyFormat()
        {
            // bug 5952
            CultureInfo locale = new CultureInfo("sr-QQ");
            DecimalFormatSymbols icuSymbols = new DecimalFormatSymbols(locale);
            String symbol = icuSymbols.CurrencySymbol;
            Currency currency = icuSymbols.Currency;
            String expectCur = null;
            String expectSym = "\u00A4";
            if (!symbol.ToString().Equals(expectSym, StringComparison.Ordinal) || currency != null)
            {
                Errln("for " + locale + " expected " + expectSym + "/" + expectCur + " but got " + symbol + "/" + currency);
            }
            else
            {
                Logln("for " + locale + " expected " + expectSym + "/" + expectCur + " and got " + symbol + "/" + currency);
            }
        }

        [Test]
        public void TestGetKeywordValues()
        {

            string[][] PREFERRED = new string[][] {
                new string[] { "root",                 },
                new string[] { "und",                  },
                new string[] { "und_ZZ",          "XAG", "XAU", "XBA", "XBB", "XBC", "XBD", "XDR", "XPD", "XPT", "XSU", "XTS", "XUA", "XXX"},
                new string[] { "en_US",           "USD", "USN"},
                new string[] { "en_029",               },
                new string[] { "en_TH",           "THB"},
                new string[] { "de",              "EUR"},
                new string[] { "de_DE",           "EUR"},
                new string[] { "de_ZZ",           "XAG", "XAU", "XBA", "XBB", "XBC", "XBD", "XDR", "XPD", "XPT", "XSU", "XTS", "XUA", "XXX"},
                new string[] { "ar",              "EGP"},
                new string[] { "ar_PS",           "ILS", "JOD"},
                new string[] { "en@currency=CAD",     "USD", "USN"},
                new string[] { "fr@currency=ZZZ",     "EUR"},
                new string[] { "de_DE@currency=DEM",  "EUR"},
                new string[] { "en_US@rg=THZZZZ",     "THB"},
                new string[] { "de@rg=USZZZZ",        "USD", "USN"},
                new string[] { "en_US@currency=CAD;rg=THZZZZ",  "THB"},
            };

            String[] ALL = Currency.GetKeywordValuesForLocale("currency", UCultureInfo.CurrentCulture, false);
            HashSet<string> ALLSET = new HashSet<string>();
            for (int i = 0; i < ALL.Length; i++)
            {
                ALLSET.Add(ALL[i]);
            }

            for (int i = 0; i < PREFERRED.Length; i++)
            {
                UCultureInfo loc = new UCultureInfo(PREFERRED[i][0]);
                String[] expected = new String[PREFERRED[i].Length - 1];
                Array.Copy(PREFERRED[i], 1, expected, 0, expected.Length);
                String[] pref = Currency.GetKeywordValuesForLocale("currency", loc, true);
                assertEquals(loc.ToString(), expected, pref);

                String[] all = Currency.GetKeywordValuesForLocale("currency", loc, false);
                // The items in the two collections should match (ignore order,
                // behavior change from 4.3.3)
                ISet<String> returnedSet = new HashSet<String>();
                returnedSet.UnionWith(all);
                assertEquals(loc.ToString(), ALLSET, returnedSet);
            }
        }

        [Test]
        public void TestIsAvailable()
        {
            //Date d1995 = new Date(788918400000L);   // 1995-01-01 00:00 GMT
            //Date d2000 = new Date(946684800000L);   // 2000-01-01 00:00 GMT
            //Date d2005 = new Date(1104537600000L);  // 2005-01-01 00:00 GMT
            DateTime d1995 = new DateTime(DateTimeOffsetUtil.GetTicksFromUnixTimeMilliseconds(788918400000L));   // 1995-01-01 00:00 GMT
            DateTime d2000 = new DateTime(DateTimeOffsetUtil.GetTicksFromUnixTimeMilliseconds(946684800000L));   // 2000-01-01 00:00 GMT
            DateTime d2005 = new DateTime(DateTimeOffsetUtil.GetTicksFromUnixTimeMilliseconds(1104537600000L));  // 2005-01-01 00:00 GMT

            assertTrue("USD all time", Currency.IsAvailable("USD", null, null));
            assertTrue("USD before 1995", Currency.IsAvailable("USD", null, d1995));
            assertTrue("USD 1995-2005", Currency.IsAvailable("USD", d1995, d2005));
            assertTrue("USD after 2005", Currency.IsAvailable("USD", d2005, null));
            assertTrue("USD on 2005-01-01", Currency.IsAvailable("USD", d2005, d2005));

            assertTrue("usd all time", Currency.IsAvailable("usd", null, null));

            assertTrue("DEM all time", Currency.IsAvailable("DEM", null, null));
            assertTrue("DEM before 1995", Currency.IsAvailable("DEM", null, d1995));
            assertTrue("DEM 1995-2000", Currency.IsAvailable("DEM", d1995, d2000));
            assertTrue("DEM 1995-2005", Currency.IsAvailable("DEM", d1995, d2005));
            assertFalse("DEM after 2005", Currency.IsAvailable("DEM", d2005, null));
            assertTrue("DEM on 2000-01-01", Currency.IsAvailable("DEM", d2000, d2000));
            assertFalse("DEM on 2005-01-01", Currency.IsAvailable("DEM", d2005, d2005));
            assertTrue("CHE all the time", Currency.IsAvailable("CHE", null, null));

            assertFalse("XXY unknown code", Currency.IsAvailable("XXY", null, null));

            assertFalse("USDOLLAR invalid code", Currency.IsAvailable("USDOLLAR", null, null));

            // illegal argument combination
            try
            {
                Currency.IsAvailable("USD", d2005, d1995);
                Errln("Expected IllegalArgumentException, because lower range is after upper range");
            }
            catch (ArgumentException e)
            {
                Logln("IllegalArgumentException, because lower range is after upper range");
            }
        }

        /**
         * Test case for getAvailableCurrencies()
         */
        [Test]
        public void TestGetAvailableCurrencies()
        {
            ISet<Currency> avail1 = Currency.GetAvailableCurrencies();

            // returned set must be modifiable - add one more currency
            avail1.Add(Currency.GetInstance("ZZZ"));    // ZZZ is not defined by ISO 4217

            ISet<Currency> avail2 = Currency.GetAvailableCurrencies();
            assertTrue("avail1 does not contain all currencies in avail2", avail1.IsSupersetOf(avail2));
            assertTrue("avail1 must have one more currency", (avail1.Count - avail2.Count == 1));
        }

        /**
         * Test case for getNumericCode()
         */
        [Test]
        public void TestGetNumericCode()
        {
            object[][] NUMCODE_TESTDATA = new object[][] {
                new object[] { "USD", 840},
                new object[] { "Usd", 840},   /* mixed casing */
                new object[] { "EUR", 978},
                new object[] { "JPY", 392},
                new object[] { "XFU", 0},     /* XFU: no numeric code */
                new object[] { "ZZZ", 0},     /* ZZZ: undefined ISO currency code */
            };

            foreach (object[] data in NUMCODE_TESTDATA)
            {
                Currency cur = Currency.GetInstance((String)data[0]);
                int numCode = cur.GetNumericCode();
                int expected = ((int)data[1]);
                if (numCode != expected)
                {
                    Errln("FAIL: getNumericCode returned " + numCode + " for "
                            + cur.CurrencyCode + " - expected: " + expected);
                }
            }
        }

        /**
         * Test case for getDisplayName()
         */
        [Test]
        public void TestGetDisplayName()
        {
            string[][] DISPNAME_TESTDATA = new string[][] {
                new string[] { "USD", "US Dollar"},
                new string[] { "EUR", "Euro"},
                new string[] { "JPY", "Japanese Yen"},
            };

            CultureInfo defLocale = CultureInfo.CurrentCulture;
            CultureInfo jaJP = new CultureInfo("ja-JP");
            CultureInfo root = CultureInfo.InvariantCulture;

            foreach (String[] data in DISPNAME_TESTDATA)
            {
                Currency cur = Currency.GetInstance(data[0]);
                assertEquals("getDisplayName() for " + data[0], data[1], cur.GetDisplayName());
                assertEquals("getDisplayName() for " + data[0] + " in locale " + defLocale, data[1], cur.GetDisplayName(defLocale));

                // ICU has localized display name for ja
                assertNotEquals("getDisplayName() for " + data[0] + " in locale " + jaJP, data[1], cur.GetDisplayName(jaJP));

                // root locale does not have any localized display names,
                // so the currency code itself should be returned
                assertEquals("getDisplayName() for " + data[0] + " in locale " + root, data[0], cur.GetDisplayName(root));
            }
        }

        [Test]
        public void TestCurrencyInfoCtor()
        {
            new CurrencyInfo("region", "code", 0, 0, 1);
        }

        /**
         * Class CurrencyMetaInfo has methods which are overwritten by its derived classes.
         * A derived class is defined here for the purpose of testing these methods.
         * Since the creator of CurrencyMetaInfo is defined as 'protected', no instance of
         * this class can be created directly.
         */
        public class TestCurrencyMetaInfo : CurrencyMetaInfo
        {
        }

        internal readonly TestCurrencyMetaInfo tcurrMetaInfo = new TestCurrencyMetaInfo();

        /*
         *
         * Test methods of base class CurrencyMetaInfo. ICU4J only creates subclasses,
         * never an instance of the base class.
         */
        [Test]
        public void TestCurrMetaInfoBaseClass()
        {
            CurrencyFilter usFilter = CurrencyFilter.OnRegion("US");

            assertEquals("Empty list expected", 0, tcurrMetaInfo.CurrencyInfo(usFilter).Count);
            assertEquals("Empty list expected", 0, tcurrMetaInfo.Currencies(usFilter).Count);
            assertEquals("Empty list expected", 0, tcurrMetaInfo.Regions(usFilter).Count);

            assertEquals("Iso format for digits expected",
                         "CurrencyDigits(fractionDigits='2',roundingIncrement='0')",
                         tcurrMetaInfo.CurrencyDigits("isoCode").ToString());
        }

        /**
         * Test cases for rounding and fractions.
         */
        [Test]
        public void testGetDefaultFractionDigits_CurrencyUsage()
        {
            Currency currency = Currency.GetInstance(new UCultureInfo("zh_Hans_CN"));
            int cashFractionDigits = currency.GetDefaultFractionDigits(CurrencyUsage.Cash);
            assertEquals("number of digits in fraction incorrect", 2, cashFractionDigits);
        }

        [Test]
        public void testGetRoundingIncrement()
        {
            Currency currency = Currency.GetInstance(new UCultureInfo("ja-JP"));
            // It appears as though this always returns 0 irrespective of the currency.
            double roundingIncrement = currency.GetRoundingIncrement();
            assertEquals("Rounding increment not zero", 0.0, roundingIncrement, 0.0);
        }
        [Test]
        public void testGetRoundingIncrement_CurrencyUsage()
        {
            Currency currency = Currency.GetInstance(new UCultureInfo("ja-JP"));
            // It appears as though this always returns 0 irrespective of the currency or usage.
            double roundingIncrement = currency.GetRoundingIncrement(CurrencyUsage.Cash);
            assertEquals("Rounding increment not zero", 0.0, roundingIncrement, 0.0);
        }

        [Test]
        public void TestCurrencyDataCtor()// throws Exception
        {
            CheckDefaultPrivateConstructor(typeof(CurrencyData));
        }
    }
}
