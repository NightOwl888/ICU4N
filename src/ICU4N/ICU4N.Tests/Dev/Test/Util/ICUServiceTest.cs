using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Support.Collections;
using ICU4N.Dev.Test.Normalizers;
using ICU4N.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using ICUResourceBundleFactory = ICU4N.Impl.ICULocaleService.ICUResourceBundleFactory; // ICU4N TODO: API - de-nest ?
using IFactory = ICU4N.Impl.ICUService.IFactory; // ICU4N TODO: API - de-nest ?
using Key = ICU4N.Impl.ICUService.Key; // ICU4N TODO: API - de-nest ?
using LocaleKey = ICU4N.Impl.ICULocaleService.LocaleKey; // ICU4N TODO: API - de-nest ?
using LocaleKeyFactory = ICU4N.Impl.ICULocaleService.LocaleKeyFactory; // ICU4N TODO: API - de-nest ?
using IServiceListener = ICU4N.Impl.ICUService.IServiceListener; // ICU4N TODO: API - de-nest ?
using SimpleFactory = ICU4N.Impl.ICUService.SimpleFactory; // ICU4N TODO: API - de-nest ?
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.Util
{
    public class ICUServiceTest : TestFmwk
    {
        private string lrmsg(string message, object lhs, object rhs)
        {
            return message + " lhs: " + lhs + " rhs: " + rhs;
        }

        public void confirmBoolean(string message, bool val)
        {
            msg(message, val ? LOG : ERR, !val, true);
        }

        public void confirmEqual(string message, object lhs, object rhs)
        {
            msg(lrmsg(message, lhs, rhs), (lhs == null ? rhs == null : lhs.Equals(rhs)) ? LOG : ERR, true, true);
        }

        public void confirmIdentical(string message, object lhs, object rhs)
        {
            msg(lrmsg(message, lhs, rhs), lhs == rhs ? LOG : ERR, true, true);
        }

        public void confirmIdentical(string message, int lhs, int rhs)
        {
            msg(message + " lhs: " + lhs + " rhs: " + rhs, lhs == rhs ? LOG : ERR, true, true);
        }

        /**
         * Convenience override of getDisplayNames(ULocale, Comparator, string) that
         * uses the current default ULocale as the locale, the default collator for
         * the locale as the comparator to sort the display names, and null for
         * the matchID.
         */
        public SortedDictionary<string, string> GetDisplayNames(ICUService service)
        {
            ULocale locale = ULocale.GetDefault();
            //Collator col = Collator.getInstance(locale.toLocale());
            CompareInfo col = CompareInfo.GetCompareInfo(locale.ToLocale().Name);
            return service.GetDisplayNames(locale, col, null);
        }

        /**
         * Convenience override of getDisplayNames(ULocale, Comparator, string) that
         * uses the default collator for the locale as the comparator to
         * sort the display names, and null for the matchID.
         */
        public SortedDictionary<string, string> GetDisplayNames(ICUService service, ULocale locale)
        {
            //Collator col = Collator.getInstance(locale.toLocale());
            CompareInfo col = CompareInfo.GetCompareInfo(locale.ToLocale().Name);
            return service.GetDisplayNames(locale, col, null);
        }
        /**
         * Convenience override of getDisplayNames(ULocale, Comparator, string) that
         * uses the default collator for the locale as the comparator to
         * sort the display names.
         */
        public SortedDictionary<string, string> GetDisplayNames(ICUService service, ULocale locale, string matchID)
        {
            //Collator col = Collator.getInstance(locale.toLocale());
            CompareInfo col = CompareInfo.GetCompareInfo(locale.ToLocale().Name);
            return service.GetDisplayNames(locale, col, matchID);
        }

        // use locale keys
        internal sealed class TestService : ICUService
        {
            public TestService()
                    : base("Test Service")
            {
            }

            public override Key CreateKey(string id)
            {
                return LocaleKey.CreateWithCanonicalFallback(id, null); // no fallback locale
            }
        }

        private class AnonymousFactory : IFactory
        {
            private readonly Func<ICUService.Key, ICUService, object> create;
            private readonly Func<string, ULocale, string> getDisplayName;
            private readonly Action<IDictionary<string, IFactory>> updateVisibleIds;

            public AnonymousFactory(Func<ICUService.Key, ICUService, object> create,
                Func<string, ULocale, string> getDisplayName, Action<IDictionary<string, IFactory>> updateVisibleIds)
            {
                this.create = create;
                this.getDisplayName = getDisplayName;
                this.updateVisibleIds = updateVisibleIds;
            }

            public object Create(ICUService.Key key, ICUService service)
            {
                return create != null ? create(key, service) : new ULocale(key.CurrentID);
            }

            public string GetDisplayName(string id, ULocale locale)
            {
                return getDisplayName != null ? getDisplayName(id, locale) : string.Empty;
            }

            public void UpdateVisibleIDs(IDictionary<string, IFactory> result)
            {
                updateVisibleIds?.Invoke(result);
            }
        }

        private class AnonymousServiceListener : IServiceListener
        {
            private readonly Action<ICUService, int> serviceChanged;
            public int n;

            public AnonymousServiceListener(Action<ICUService, int> serviceChanged)
            {
                if (serviceChanged == null)
                    throw new ArgumentNullException(nameof(serviceChanged));
                this.serviceChanged = serviceChanged;
            }

            public override void ServiceChanged(ICUService service)
            {
                serviceChanged(service, n);
            }
        }

        [Test]
        public void TestAPI()
        {
            // create a service using locale keys,
            ICUService service = new TestService();

            Logln("service name:" + service.Name);

            // register an object with one locale,
            // search for an object with a more specific locale
            // should return the original object
            Integer singleton0 = new Integer(0);
            service.RegisterObject(singleton0, "en_US");
            object result = service.Get("en_US_FOO");
            confirmIdentical("1) en_US_FOO -> en_US", result, singleton0);

            // register a new object with the more specific locale
            // search for an object with that locale
            // should return the new object
            Integer singleton1 = new Integer(1);
            service.RegisterObject(singleton1, "en_US_FOO");
            result = service.Get("en_US_FOO");
            confirmIdentical("2) en_US_FOO -> en_US_FOO", result, singleton1);

            // search for an object that falls back to the first registered locale
            result = service.Get("en_US_BAR");
            confirmIdentical("3) en_US_BAR -> en_US", result, singleton0);

            // get a list of the factories, should be two
            IList<ICUService.IFactory> factories = service.Factories();
            confirmIdentical("4) factory size", factories.Count, 2);

            // register a new object with yet another locale
            // original factory list is unchanged
            Integer singleton2 = new Integer(2);
            service.RegisterObject(singleton2, "en");
            confirmIdentical("5) factory size", factories.Count, 2);

            // search for an object with the new locale
            // stack of factories is now en, en_US_FOO, en_US
            // search for en_US should still find en_US object
            result = service.Get("en_US_BAR");
            confirmIdentical("6) en_US_BAR -> en_US", result, singleton0);

            // register a new object with an old id, should hide earlier factory using this id, but leave it there
            Integer singleton3 = new Integer(3);
            service.RegisterObject(singleton3, "en_US");
            factories = service.Factories();
            confirmIdentical("9) factory size", factories.Count, 4);

            // should get data from that new factory
            result = service.Get("en_US_BAR");
            confirmIdentical("10) en_US_BAR -> (3)", result, singleton3);

            // remove new factory
            // should have fewer factories again
            service.UnregisterFactory((IFactory)factories[0]);
            factories = service.Factories();
            confirmIdentical("11) factory size", factories.Count, 3);

            // should get original data again after remove factory
            result = service.Get("en_US_BAR");
            confirmIdentical("12) en_US_BAR -> 0", result, singleton0);

            // shouldn't find unregistered ids
            result = service.Get("foo");
            confirmIdentical("13) foo -> null", result, null);

            // should find non-canonical strings
            string[] resultID = new string[1];
            result = service.Get("EN_us_fOo", resultID);
            confirmEqual("14) find non-canonical", resultID[0], "en_US_FOO");

            // should be able to register non-canonical strings and get them canonicalized
            service.RegisterObject(singleton3, "eN_ca_dUde");
            result = service.Get("En_Ca_DuDe", resultID);
            confirmEqual("15) register non-canonical", resultID[0], "en_CA_DUDE");

            // should be able to register invisible factories, these will not
            // be visible by default, but if you know the secret password you
            // can still access these services...
            Integer singleton4 = new Integer(4);
            service.RegisterObject(singleton4, "en_US_BAR", false);
            result = service.Get("en_US_BAR");
            confirmIdentical("17) get invisible", result, singleton4);

            // should not be able to locate invisible services
            var ids = service.GetVisibleIDs();
            confirmBoolean("18) find invisible", !ids.Contains("en_US_BAR"));

            service.Reset();

            // an anonymous factory than handles all ids
            //{
            //    Factory factory = new Factory() {
            //    @Override
            //    public object create(Key key, ICUService unusedService)
            //    {
            //        return new ULocale(key.currentID());
            //    }

            //    @Override
            //        public void updateVisibleIDs(Map unusedResult)
            //    {
            //    }

            //    @Override
            //        public string getDisplayName(string id, ULocale l)
            //    {
            //        return null;
            //    }
            //};

            {
                // an anonymous factory than handles all ids
                IFactory factory = new AnonymousFactory(null, null, null);
                service.RegisterFactory(factory);

                // anonymous factory will still handle the id
                result = service.Get(ULocale.US.ToString());
                confirmEqual("21) locale", result, ULocale.US);

                // still normalizes id
                result = service.Get("EN_US_BAR");
                confirmEqual("22) locale", result, new ULocale("en_US_BAR"));

                // we can override for particular ids
                service.RegisterObject(singleton3, "en_US_BAR");
                result = service.Get("en_US_BAR");
                confirmIdentical("23) override super", result, singleton3);

            }

            // empty service should not recognize anything
            service.Reset();
            result = service.Get("en_US");
            confirmIdentical("24) empty", result, null);

            // create a custom multiple key factory
            {
                string[] xids = { "en_US_VALLEY_GIRL",
                          "en_US_VALLEY_BOY",
                          "en_US_SURFER_GAL",
                          "en_US_SURFER_DUDE"
                };
                service.RegisterFactory(new TestLocaleKeyFactory(xids, "Later"));
            }

            // iterate over the visual ids returned by the multiple factory
            {
                ICollection<string> vids = service.GetVisibleIDs();
                var iter = vids.GetEnumerator();
                int count = 0;
                while (iter.MoveNext())
                {
                    ++count;
                    string id = (string)iter.Current;
                    Logln("  " + id + " --> " + service.Get(id));
                }
                // four visible ids
                confirmIdentical("25) visible ids", count, 4);
            }

            // iterate over the display names
            {
                var dids = GetDisplayNames(service, ULocale.GERMANY);
                var iter = dids.GetEnumerator();
                int count = 0;
                while (iter.MoveNext())
                {
                    ++count;
                    var e = iter.Current;
                    Logln("  " + e.Key + " -- > " + e.Value);
                }
                // four display names, in german
                confirmIdentical("26) display names", count, 4);
            }

            // no valid display name
            confirmIdentical("27) get display name", service.GetDisplayName("en_US_VALLEY_GEEK"), null);

            {
                string name = service.GetDisplayName("en_US_SURFER_DUDE", ULocale.US);
                confirmEqual("28) get display name", name, "English (United States, SURFER_DUDE)");
            }

            // register another multiple factory
            {
                string[] xids = {
                    "en_US_SURFER", "en_US_SURFER_GAL", "en_US_SILICON", "en_US_SILICON_GEEK"
                };
                service.RegisterFactory(new TestLocaleKeyFactory(xids, "Rad dude"));
            }

            // this time, we have seven display names
            // Rad dude's surfer gal 'replaces' later's surfer gal
            {
                var dids = GetDisplayNames(service);
                var iter = dids.GetEnumerator();
                int count = 0;
                while (iter.MoveNext())
                {
                    ++count;
                    var e = iter.Current;
                    Logln("  " + e.Key + " --> " + e.Value);
                }
                // seven display names, in spanish
                confirmIdentical("29) display names", count, 7);
            }

            // we should get the display name corresponding to the actual id
            // returned by the id we used.
            {
                string[] actualID = new string[1];
                string id = "en_us_surfer_gal";
                string gal = (string)service.Get(id, actualID);
                if (gal != null)
                {
                    Logln("actual id: " + actualID[0]);
                    string displayName = service.GetDisplayName(actualID[0], ULocale.US);
                    Logln("found actual: " + gal + " with display name: " + displayName);
                    confirmBoolean("30) found display name for actual", displayName != null);

                    displayName = service.GetDisplayName(id, ULocale.US);
                    Logln("found query: " + gal + " with display name: " + displayName);
                    // this is no longer a bug, we want to return display names for anything
                    // that a factory handles.  since we handle it, we should return a display
                    // name.  see jb3549
                    // confirmBoolean("31) found display name for query", displayName == null);
                }
                else
                {
                    Errln("30) service could not find entry for " + id);
                }

                // this should be handled by the 'dude' factory, since it overrides en_US_SURFER.
                id = "en_US_SURFER_BOZO";
                string bozo = (string)service.Get(id, actualID);
                if (bozo != null)
                {
                    string displayName = service.GetDisplayName(actualID[0], ULocale.US);
                    Logln("found actual: " + bozo + " with display name: " + displayName);
                    confirmBoolean("32) found display name for actual", displayName != null);

                    displayName = service.GetDisplayName(id, ULocale.US);
                    Logln("found actual: " + bozo + " with display name: " + displayName);
                    // see above and jb3549
                    // confirmBoolean("33) found display name for query", displayName == null);
                }
                else
                {
                    Errln("32) service could not find entry for " + id);
                }

                confirmBoolean("34) is default ", !service.IsDefault);
            }

            /*
            // disallow hiding for now

            // hiding factory should obscure 'sublocales'
            {
            string[] xids = {
            "en_US_VALLEY", "en_US_SILICON"
            };
            service.registerFactory(new TestHidingFactory(xids, "hiding"));
            }

            {
            Map dids = service.getDisplayNames();
            Iterator iter = dids.entrySet().iterator();
            int count = 0;
            while (iter.hasNext()) {
            ++count;
            Entry e = (Entry)iter.next();
            Logln("  " + e.getKey() + " -- > " + e.getValue());
            }
            confirmIdentical("35) hiding factory", count, 5);
            }
            */

            {
                var xids = service.GetVisibleIDs();
                var iter = xids.GetEnumerator();
                while (iter.MoveNext())
                {
                    string xid = (string)iter.Current;
                    Logln(xid + "?  " + service.Get(xid));
                }

                Logln("valleygirl?  " + service.Get("en_US_VALLEY_GIRL"));
                Logln("valleyboy?   " + service.Get("en_US_VALLEY_BOY"));
                Logln("valleydude?  " + service.Get("en_US_VALLEY_DUDE"));
                Logln("surfergirl?  " + service.Get("en_US_SURFER_GIRL"));
            }

            // resource bundle factory.
            service.Reset();
            service.RegisterFactory(new ICUResourceBundleFactory());

            // list all of the resources
            {
                Logln("all visible ids: " + service.GetVisibleIDs());
                /*
                Set xids = service.GetVisibleIDs();
                StringBuffer buf = new StringBuffer("{");
                bool notfirst = false;
                Iterator iter = xids.iterator();
                while (iter.hasNext()) {
                string xid = (string)iter.next();
                if (notfirst) {
                buf.append(", ");
                } else {
                notfirst = true;
                }
                buf.append(xid);
                }
                buf.append("}");
                Logln(buf.ToString());
                */
            }

            // list only the resources for es, default locale
            // since we're using the default Key, only "es" is matched
            {
                Logln("visible ids for es locale: " + service.GetVisibleIDs("es"));
            }

            // list only the spanish display names for es, spanish collation order
            // since we're using the default Key, only "es" is matched
            {
                Logln("display names: " + GetDisplayNames(service, new ULocale("es"), "es"));
            }

            // list the display names in reverse order
            {
                Logln("display names in reverse order: " +
                    service.GetDisplayNames(ULocale.US, new AnonymousComparer<object>(compare: (lhs, rhs) =>
                    {
                        return -StringComparer.OrdinalIgnoreCase.Compare((string)lhs, (string)rhs);
                    })));
                //                  service.GetDisplayNames(ULocale.US, new Comparator()
                //{
                //    @Override
                //                        public int compare(object lhs, object rhs)
                //    {
                //        return -string.CASE_INSENSITIVE_ORDER.compare((string)lhs, (string)rhs);
                //    }
                //}));
            }

            // get all the display names of these resources
            // this should be fast since the display names were cached.
            {
                Logln("service display names for de_DE");
                var names = GetDisplayNames(service, new ULocale("de_DE"));
                StringBuffer buf = new StringBuffer("{");
                var iter = names.GetEnumerator();
                while (iter.MoveNext())
                {
                    var e = iter.Current;
                    string name = (string)e.Key;
                    string id = (string)e.Value;
                    buf.Append("\n   " + name + " --> " + id);
                }
                buf.Append("\n}");
                Logln(buf.ToString());
            }

            CalifornioLanguageFactory califactory = new CalifornioLanguageFactory();
            service.RegisterFactory(califactory);
            // get all the display names of these resources
            {
                Logln("californio language factory");
                StringBuffer buf = new StringBuffer("{");
                string[] idNames = {
                    CalifornioLanguageFactory.californio,
                    CalifornioLanguageFactory.valley,
                    CalifornioLanguageFactory.surfer,
                    CalifornioLanguageFactory.geek
                };
                for (int i = 0; i < idNames.Length; ++i)
                {
                    string idName = idNames[i];
                    buf.Append("\n  --- " + idName + " ---");
                    var names = GetDisplayNames(service, new ULocale(idName));
                    var iter = names.GetEnumerator();
                    while (iter.MoveNext())
                    {
                        var e = iter.Current;
                        string name = (string)e.Key;
                        string id = (string)e.Value;
                        buf.Append("\n    " + name + " --> " + id);
                    }
                }
                buf.Append("\n}");
                Logln(buf.ToString());
            }

            // test notification
            // simple registration
            {
                Logln("simple registration notification");
                ICULocaleService ls = new ICULocaleService();
                IServiceListener l1 = new AnonymousServiceListener(serviceChanged: (s, n) =>
                {
                    Logln("listener 1 report " + n++ + " service changed: " + s);
                });
                //ServiceListener l1 = new ServiceListener()
                //{
                //            private int n;

                //            public void serviceChanged(ICUService s)
                //{
                //    Logln("listener 1 report " + n++ + " service changed: " + s);
                //}
                //        };
                ls.AddListener(l1);
                IServiceListener l2 = new AnonymousServiceListener(serviceChanged: (s, n) =>
                {
                    Logln("listener 2 report " + n++ + " service changed: " + s);
                });
                //        ServiceListener l2 = new ServiceListener()
                //        {
                //            private int n;

                //            public void serviceChanged(ICUService s)
                //{
                //    Logln("listener 2 report " + n++ + " service changed: " + s);
                //}
                //        };
                ls.AddListener(l2);
                Logln("registering foo... ");
                ls.RegisterObject("Foo", "en_FOO");
                Logln("registering bar... ");
                ls.RegisterObject("Bar", "en_BAR");
                Logln("getting foo...");
                Logln((string)ls.Get("en_FOO"));
                Logln("removing listener 2...");
                ls.RemoveListener(l2);
                Logln("registering baz...");
                ls.RegisterObject("Baz", "en_BAZ");
                Logln("removing listener 1");
                ls.RemoveListener(l1);
                Logln("registering burp...");
                ls.RegisterObject("Burp", "en_BURP");

                // should only get one notification even if register multiple times
                Logln("... trying multiple registration");
                ls.AddListener(l1);
                ls.AddListener(l1);
                ls.AddListener(l1);
                ls.AddListener(l2);
                ls.RegisterObject("Foo", "en_FOO");
                Logln("... registered foo");

                // since in a separate thread, we can callback and not deadlock
                IServiceListener l3 = new AnonymousServiceListener(serviceChanged: (s, n) =>
                {
                    Logln("listener 3 report " + n++ + " service changed...");
                    if (s.Get("en_BOINK") == null)
                    { // don't recurse on ourselves!!!
                        Logln("registering boink...");
                        s.RegisterObject("boink", "en_BOINK");
                    }
                });


                //ServiceListener l3 = new ServiceListener()
                //{
                //            private int n;
                //@Override
                //            public void serviceChanged(ICUService s)
                //{
                //    Logln("listener 3 report " + n++ + " service changed...");
                //    if (s.Get("en_BOINK") == null)
                //    { // don't recurse on ourselves!!!
                //        Logln("registering boink...");
                //        s.RegisterObject("boink", "en_BOINK");
                //    }
                //}
                //        };
                ls.AddListener(l3);
                Logln("registering boo...");
                ls.RegisterObject("Boo", "en_BOO");
                Logln("...done");

#if !NETCOREAPP1_0
                try
                {
#endif
                Thread.Sleep(100);
#if !NETCOREAPP1_0
                }
                catch (ThreadInterruptedException e)
                {
                }
#endif
            }
        }

        internal class TestLocaleKeyFactory : LocaleKeyFactory
        {
            protected readonly ISet<string> ids;
            protected readonly string factoryID;

            public TestLocaleKeyFactory(string[] ids, string factoryID)
                : base(VISIBLE, factoryID)
            {
                this.ids = new HashSet<string>(ids).ToUnmodifiableSet();
                this.factoryID = factoryID + ": ";
            }

            protected override object HandleCreate(ULocale loc, int kind, ICUService service)
            {
                return factoryID + loc.ToString();
            }

            protected override ICollection<string> GetSupportedIDs()
            {
                return ids;
            }
        }

        /*
        // Disallow hiding for now since it causes gnarly problems, like
        // how do you localize the hidden (but still exported) names.

        static class TestHidingFactory implements ICUService.Factory {
        protected final string[] ids;
        protected final string factoryID;

        public TestHidingFactory(string[] ids) {
        this(ids, "Hiding");
        }

        public TestHidingFactory(string[] ids, string factoryID) {
        this.ids = (string[])ids.clone();

        if (factoryID == null || factoryID.length() == 0) {
        this.factoryID = "";
        } else {
        this.factoryID = factoryID + ": ";
        }
        }

        public object create(Key key, ICUService service) {
        for (int i = 0; i < ids.length; ++i) {
        if (LocaleUtility.isFallbackOf(ids[i], key.currentID())) {
        return factoryID + key.canonicalID();
        }
        }
        return null;
        }

        public void updateVisibleIDs(Map result) {
        for (int i = 0; i < ids.length; ++i) {
        string id = ids[i];
        Iterator iter = result.keySet().iterator();
        while (iter.hasNext()) {
        if (LocaleUtility.isFallbackOf(id, (string)iter.next())) {
        iter.remove();
        }
        }
        result.put(id, this);
        }
        }

        public string getDisplayName(string id, ULocale locale) {
        return factoryID + new ULocale(id).GetDisplayName(locale);
        }
        }
        */

        internal class CalifornioLanguageFactory : ICUResourceBundleFactory
        {
            public static string californio = "en_US_CA";
            public static string valley = californio + "_VALLEY";
            public static string surfer = californio + "_SURFER";
            public static string geek = californio + "_GEEK";
            public static ISet<string> supportedIDs;
            static CalifornioLanguageFactory()
            {
                HashSet<string> result = new HashSet<string>();
                // ICU4N TODO: Finish implementation
                //result.UnionWith(ICUResourceBundle.GetAvailableLocaleNameSet());
                result.Add(californio);
                result.Add(valley);
                result.Add(surfer);
                result.Add(geek);
                supportedIDs = result.ToUnmodifiableSet();
            }

            protected override ICollection<string> GetSupportedIDs()
            {
                return supportedIDs;
            }

            public override string GetDisplayName(string id, ULocale locale)
            {
                string prefix = "";
                string suffix = "";
                string ls = locale.ToString();
                if (LocaleUtility.IsFallbackOf(californio, ls))
                {
                    if (ls.Equals(valley, StringComparison.OrdinalIgnoreCase))
                    {
                        prefix = "Like, you know, it's so totally ";
                    }
                    else if (ls.Equals(surfer, StringComparison.OrdinalIgnoreCase))
                    {
                        prefix = "Dude, its ";
                    }
                    else if (ls.Equals(geek, StringComparison.OrdinalIgnoreCase))
                    {
                        prefix = "I'd estimate it's approximately ";
                    }
                    else
                    {
                        prefix = "Huh?  Maybe ";
                    }
                }
                if (LocaleUtility.IsFallbackOf(californio, id))
                {
                    if (id.Equals(valley, StringComparison.OrdinalIgnoreCase))
                    {
                        suffix = "like the Valley, you know?  Let's go to the mall!";
                    }
                    else if (id.Equals(surfer, StringComparison.OrdinalIgnoreCase))
                    {
                        suffix = "time to hit those gnarly waves, Dude!!!";
                    }
                    else if (id.Equals(geek, StringComparison.OrdinalIgnoreCase))
                    {
                        suffix = "all systems go.  T-Minus 9, 8, 7...";
                    }
                    else
                    {
                        suffix = "No Habla Englais";
                    }
                }
                else
                {
                    suffix = base.GetDisplayName(id, locale);
                }

                return prefix + suffix;
            }
        }

        [Test]
        public void TestLocale()
        {
            ICULocaleService service = new ICULocaleService("test locale");
            service.RegisterObject("root", ULocale.ROOT);
            service.RegisterObject("german", "de");
            service.RegisterObject("german_Germany", ULocale.GERMANY);
            service.RegisterObject("japanese", "ja");
            service.RegisterObject("japanese_Japan", ULocale.JAPAN);

            object target = service.Get("de_US");
            confirmEqual("test de_US", "german", target);

            ULocale de = new ULocale("de");
            ULocale de_US = new ULocale("de_US");

            target = service.Get(de_US);
            confirmEqual("test de_US 2", "german", target);

            target = service.Get(de_US, LocaleKey.KIND_ANY);
            confirmEqual("test de_US 3", "german", target);

            target = service.Get(de_US, 1234);
            confirmEqual("test de_US 4", "german", target);

            ULocale[] actualReturn = new ULocale[1];
            target = service.Get(de_US, actualReturn);
            confirmEqual("test de_US 5", "german", target);
            confirmEqual("test de_US 6", actualReturn[0], de);

            actualReturn[0] = null;
            target = service.Get(de_US, LocaleKey.KIND_ANY, actualReturn);
            confirmEqual("test de_US 7", actualReturn[0], de);

            actualReturn[0] = null;
            target = service.Get(de_US, 1234, actualReturn);
            confirmEqual("test de_US 8", "german", target);
            confirmEqual("test de_US 9", actualReturn[0], de);

            service.RegisterObject("one/de_US", de_US, 1);
            service.RegisterObject("two/de_US", de_US, 2);

            target = service.Get(de_US, 1);
            confirmEqual("test de_US kind 1", "one/de_US", target);

            target = service.Get(de_US, 2);
            confirmEqual("test de_US kind 2", "two/de_US", target);

            target = service.Get(de_US);
            confirmEqual("test de_US kind 3", "german", target);

            LocaleKey lkey = LocaleKey.CreateWithCanonicalFallback("en", null, 1234);
            Logln("lkey prefix: " + lkey.Prefix);
            Logln("lkey descriptor: " + lkey.CurrentDescriptor());
            Logln("lkey current locale: " + lkey.CurrentLocale());

            lkey.Fallback();
            Logln("lkey descriptor 2: " + lkey.CurrentDescriptor());

            lkey.Fallback();
            Logln("lkey descriptor 3: " + lkey.CurrentDescriptor());

            target = service.Get("za_PPP");
            confirmEqual("test zappp", "root", target);

            ULocale loc = ULocale.GetDefault();
            ULocale.SetDefault(ULocale.JAPANESE);
            target = service.Get("za_PPP");
            confirmEqual("test with ja locale", "japanese", target);

            var ids = service.GetVisibleIDs();
            for (var iter = ids.GetEnumerator(); iter.MoveNext();)
            {
                Logln("id: " + iter.Current);
            }

            ULocale.SetDefault(loc);
            ids = service.GetVisibleIDs();
            for (var iter = ids.GetEnumerator(); iter.MoveNext();)
            {
                Logln("id: " + iter.Current);
            }

            target = service.Get("za_PPP");
            confirmEqual("test with en locale", "root", target);

            ULocale[] locales = service.GetAvailableULocales();
            confirmIdentical("test available locales", locales.Length, 6);
            Logln("locales: ");
            for (int i = 0; i < locales.Length; ++i)
            {
                Log("\n  [" + i + "] " + locales[i]);
            }
            Logln(" ");

            service.RegisterFactory(new ICUResourceBundleFactory());
            target = service.Get(ULocale.JAPAN);

            {
                int n = 0;
                var factories = service.Factories();
                var iter = factories.GetEnumerator();
                while (iter.MoveNext())
                {
                    Logln("[" + n++ + "] " + iter.Current);
                }
            }

            // list only the english display names for es, in reverse order
            // since we're using locale keys, we should get all and only the es locales
            // hmmm, the default toString function doesn't print in sorted order for TreeMap
            {
                var map = service.GetDisplayNames(ULocale.US,
                    new AnonymousComparer<object>(compare: (lhs, rhs) =>
                    {
                        return -StringComparer.OrdinalIgnoreCase.Compare((string)lhs, (string)rhs);
                    }),
                            //                    new Comparator() {
                            //                            @Override
                            //                            public int compare(object lhs, object rhs)
                            //    {
                            //        return -string.CASE_INSENSITIVE_ORDER.compare((string)lhs, (string)rhs);
                            //    }
                            //},
                            "es");

                Logln("es display names in reverse order " + map);
            }
        }

        internal class WrapFactory : IFactory
        {
            private readonly string greetingID;

            public WrapFactory(string greetingID)
            {
                this.greetingID = greetingID;
            }

            public object Create(Key key, ICUService serviceArg)
            {
                if (key.CurrentID.Equals(greetingID))
                {
                    object previous = serviceArg.GetKey(key, null, this);
                    return "A different greeting: \"" + previous + "\"";
                }
                return null;
            }

            public void UpdateVisibleIDs(IDictionary<string, ICUService.IFactory> result)
            {
                result["greeting"] = this;
            }

            public string GetDisplayName(string id, ULocale locale)
            {
                return "wrap '" + id + "'";
            }
        }

        [Test]
        public void TestWrapFactory()
        {
            string greeting = "Hello There";
            string greetingID = "greeting";

            ICUService service = new ICUService("wrap");
            service.RegisterObject(greeting, greetingID);

            Logln("test one: " + service.Get(greetingID));


            service.RegisterFactory(new WrapFactory(greetingID));

            confirmEqual("wrap test: ", service.Get(greetingID), "A different greeting: \"" + greeting + "\"");
        }

        // misc coverage tests
        [Test]
        public void TestCoverage()
        {
            // Key
            Key key = new Key("foobar");
            Logln("ID: " + key.ID);
            Logln("canonicalID: " + key.CanonicalID);
            Logln("currentID: " + key.CurrentID);
            Logln("has fallback: " + key.Fallback());

            // SimpleFactory
            object obj = new object();
            SimpleFactory sf = new SimpleFactory(obj, "object");
            try
            {
                sf = new SimpleFactory(null, null);
                Errln("didn't throw exception");
            }
            catch (ArgumentException e)
            {
                Logln("OK: " + e.ToString());
            }
            catch (Exception e)
            {
                Errln("threw wrong exception" + e);
            }
            Logln(sf.GetDisplayName("object", null));

            // ICUService
            ICUService service = new ICUService();
            service.RegisterFactory(sf);

            try
            {
                service.Get(null, null);
                Errln("didn't throw exception");
            }
            catch (ArgumentNullException e)
            {
                Logln("OK: " + e.ToString());
            }
            /*
            catch (Exception e) {
            Errln("threw wrong exception" + e);
            }
            */
            try
            {
                service.RegisterFactory(null);
                Errln("didn't throw exception");
            }
            catch (ArgumentNullException e)
            {
                Logln("OK: " + e.ToString());
            }
            catch (Exception e)
            {
                Errln("threw wrong exception" + e);
            }

            try
            {
                service.UnregisterFactory(null);
                Errln("didn't throw exception");
            }
            catch (ArgumentNullException e)
            {
                Logln("OK: " + e.ToString());
            }
            catch (Exception e)
            {
                Errln("threw wrong exception" + e);
            }

            Logln("object is: " + service.Get("object"));

            Logln("stats: " + service.Stats());

            // ICURWLock

            ICURWLock rwlock = new ICURWLock();
            rwlock.ResetStats();

            rwlock.AcquireRead();
            rwlock.ReleaseRead();

            rwlock.AcquireWrite();
            rwlock.ReleaseWrite();
            Logln("stats: " + rwlock.GetStats());
            Logln("stats: " + rwlock.ClearStats());
            rwlock.AcquireRead();
            rwlock.ReleaseRead();
            rwlock.AcquireWrite();
            rwlock.ReleaseWrite();
            Logln("stats: " + rwlock.GetStats());

            try
            {
                rwlock.ReleaseRead();
                Errln("no error thrown");
            }
            catch (Exception e)
            {
                Logln("OK: " + e.ToString());
            }

            try
            {
                rwlock.ReleaseWrite();
                Errln("no error thrown");
            }
            catch (Exception e)
            {
                Logln("OK: " + e.ToString());
            }

            // ICULocaleService

            // LocaleKey

            // LocaleKey lkey = LocaleKey.create("en_US", "ja_JP");
            // lkey = LocaleKey.create(null, null);
            LocaleKey lkey = LocaleKey.CreateWithCanonicalFallback("en_US", "ja_JP");
            Logln("lkey: " + lkey);

            lkey = LocaleKey.CreateWithCanonicalFallback(null, null);
            Logln("lkey from null,null: " + lkey);

            // LocaleKeyFactory
            LocaleKeyFactory lkf = new LKFSubclass(false);
            Logln("lkf: " + lkf);
            Logln("obj: " + lkf.Create(lkey, null));
            Logln(lkf.GetDisplayName("foo", null));
            Logln(lkf.GetDisplayName("bar", null));
            lkf.UpdateVisibleIDs(new Dictionary<string, IFactory>());

            LocaleKeyFactory invisibleLKF = new LKFSubclass(false);
            Logln("obj: " + invisibleLKF.Create(lkey, null));
            Logln(invisibleLKF.GetDisplayName("foo", null));
            Logln(invisibleLKF.GetDisplayName("bar", null));
            invisibleLKF.UpdateVisibleIDs(new Dictionary<string, IFactory>());

            // ResourceBundleFactory
            ICUResourceBundleFactory rbf = new ICUResourceBundleFactory();
            Logln("RB: " + rbf.Create(lkey, null));

            // ICUNotifier
            ICUNotifier nf = new ICUNSubclass();
            try
            {
                nf.AddListener(null);
                Errln("added null listener");
            }
            catch (ArgumentNullException e)
            {
                Logln(e.ToString());
            }
            catch (Exception e)
            {
                Errln("got wrong exception");
            }

            try
            {
                nf.AddListener(new WrongListener());
                Errln("added wrong listener");
            }
            catch (InvalidOperationException e)
            {
                Logln(e.ToString());
            }
            catch (Exception e)
            {
                Errln("got wrong exception");
            }

            try
            {
                nf.RemoveListener(null);
                Errln("removed null listener");
            }
            catch (ArgumentNullException e)
            {
                Logln(e.ToString());
            }
            catch (Exception e)
            {
                Errln("got wrong exception");
            }

            nf.RemoveListener(new MyListener());
            nf.NotifyChanged();
            nf.AddListener(new MyListener());
            nf.RemoveListener(new MyListener());
        }

        internal class MyListener : IEventListener
        {
        }

        internal class WrongListener : IEventListener
        {
        }

        internal class ICUNSubclass : ICUNotifier
        {
            protected override bool AcceptsListener(IEventListener l)
            {
                return l is MyListener;
            }

            // not used, just needed to implement abstract base
            protected override void NotifyListener(IEventListener l)
            {
            }
        }

        internal class LKFSubclass : LocaleKeyFactory
        {
            internal LKFSubclass(bool visible)
                : base(visible ? VISIBLE : INVISIBLE)
            {
            }

            protected override ICollection<string> GetSupportedIDs()
            {
                return new HashSet<string>();
            }
        }
    }
}
