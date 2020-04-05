using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Util;
using J2N.Text;
using J2N.Threading;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;

namespace ICU4N.Dev.Test.Util
{
    public class ICUServiceThreadTest : TestFmwk
    {
        private static readonly bool PRINTSTATS = false;

        private static readonly string[] countries = {
            "ab", "bc", "cd", "de", "ef", "fg", "gh", "ji", "ij", "jk"
        };
        private static readonly string[] languages = {
            "", "ZY", "YX", "XW", "WV", "VU", "UT", "TS", "SR", "RQ", "QP"
        };
        private static readonly string[] variants = {
            "", "", "", "GOLD", "SILVER", "BRONZE"
        };

        private class TestFactory : ICUSimpleFactory
        {
            internal TestFactory(string id)
            : base(new ULocale(id), id, true)
            {
            }

            public override string GetDisplayName(string idForDisplay, ULocale locale)
            {
                return (visible && idForDisplay.Equals(this.id)) ? "(" + locale.ToString() + ") " + idForDisplay : null;
            }

            public override string ToString()
            {
                return "Factory_" + id;
            }
        }
        /**
         * Convenience override of getDisplayNames(ULocale, Comparator, string) that
         * uses the default collator for the locale as the comparator to
         * sort the display names, and null for the matchID.
         */
        public static IDictionary<string, string> GetDisplayNames(ICUService service, ULocale locale)
        {
            //Collator col;
            //try
            //{
            //    col = Collator.getInstance(locale.ToLocale());
            //}
            //catch (MissingResourceException e)
            //{
            //    // if no collator resources, we can't collate
            //    col = null;
            //}
            CompareInfo col;
            try
            {
                col = CompareInfo.GetCompareInfo(locale.ToLocale().ToString());
            }
            catch (MissingManifestResourceException e)
            {
                // if no collator resources, we can't collate
                col = null;
            }

            return service.GetDisplayNames(locale, col, null);
        }
        private static readonly Random r = new Random(); // this is a multi thread test, can't 'unrandomize'

        private static string GetCLV()
        {
            string c = countries[r.Next(countries.Length)];
            string l = languages[r.Next(languages.Length)];
            string v = variants[r.Next(variants.Length)];

            // ICU4N TODO: Check conversion logic
            var sb = new StringBuilder(c);
            if (!string.IsNullOrEmpty(l))
            {
                sb.Append('_');
                sb.Append(l);
                if (!string.IsNullOrEmpty(v))
                {
                    sb.Append('_');
                    sb.Append(v);
                }
            }
            return sb.ToString();

            //return new CultureInfo(c + "-" + l + (string.IsNullOrEmpty(v) ? string.Empty : "-" + v)).ToString();
        }

        private static bool WAIT = true;
        private static bool GO = false;
        private static long TIME = 5000;

        public static void RunThreads()
        {
            RunThreads(TIME);
        }

        public static void RunThreads(long time)
        {
#if FEATURE_THREADINTERRUPT
            try
            {
#endif
                GO = true;
                WAIT = false;

                Thread.Sleep((int)time);

                WAIT = true;
                GO = false;

                Thread.Sleep(300);
#if FEATURE_THREADINTERRUPT
            }
                catch (ThreadInterruptedException e)
            {
            }
#endif
        }

        internal class TestThread : ThreadJob
        {
            //private final string name;
            protected ICUService service;
            private readonly long delay;

            public TestThread(string name, ICUService service, long delay)
            {
                //this.name = name + " ";
                this.service = service;
                this.delay = delay;
                this.IsBackground = (true);
            }

            public override void Run()
            {
                while (WAIT)
                {
                    Thread.Sleep(0);
                }

#if FEATURE_THREADINTERRUPT
                try
                {
#endif
                    while (GO)
                    {
                        Iterate();
                        if (delay > 0)
                        {
                            Thread.Sleep((int)delay);
                        }
                    }
#if FEATURE_THREADINTERRUPT
                }
                catch (ThreadInterruptedException e)
                {
                }
#endif
            }

            protected virtual void Iterate()
            {
            }

            /*
              public boolean logging() {
              return log != null;
              }

              public void log(string msg) {
              if (logging()) {
              log.log(name + msg);
              }
              }

              public void logln(string msg) {
              if (logging()) {
              log.logln(name + msg);
              }
              }

              public void err(string msg) {
              if (logging()) {
              log.err(name + msg);
              }
              }

              public void errln(string msg) {
              if (logging()) {
              log.errln(name + msg);
              }
              }

              public void warn(string msg) {
              if (logging()) {
              log.info(name + msg);
              }
              }

              public void warnln(string msg) {
              if (logging()) {
              log.infoln(name + msg);
              }
              }
            */
        }

        internal class RegisterFactoryThread : TestThread
        {
            internal RegisterFactoryThread(string name, ICUService service, long delay)
                : base("REG " + name, service, delay)
            {
            }

            protected override void Iterate()
            {
                IServiceFactory f = new TestFactory(GetCLV());
                service.RegisterFactory(f);
                TestFmwk.Logln(f.ToString());
            }
        }

        internal class UnregisterFactoryThread : TestThread
        {
            private Random r;
            IList<IServiceFactory> factories;

            internal UnregisterFactoryThread(string name, ICUService service, long delay)

        : base("UNREG " + name, service, delay)
            {
                r = new Random();
                factories = service.Factories();
            }

            protected override void Iterate()
            {
                int s = factories.Count;
                if (s == 0)
                {
                    factories = service.Factories();
                }
                else
                {
                    int n = r.Next(s);
                    //IFactory f = (IFactory)factories.RemoveAt(n);
                    IServiceFactory f = factories[n];
                    factories.Remove(f);
                    bool success = service.UnregisterFactory(f);
                    TestFmwk.Logln("factory: " + f + (success ? " succeeded." : " *** failed."));
                }
            }
        }

        internal class UnregisterFactoryListThread : TestThread
        {
            internal IServiceFactory[] factories;
            internal int n;

            internal UnregisterFactoryListThread(string name, ICUService service, long delay, IServiceFactory[]
             factories)
                 : base("UNREG " + name, service, delay)
            {

                this.factories = factories;
            }

            protected override void Iterate()
            {
                if (n < factories.Length)
                {
                    IServiceFactory f = factories[n++];
                    bool success = service.UnregisterFactory(f);
                    TestFmwk.Logln("factory: " + f + (success ? " succeeded." : " *** failed."));
                }
            }
        }


        internal class GetVisibleThread : TestThread
        {
            internal GetVisibleThread(string name, ICUService service, long delay)
                : base("VIS " + name, service, delay)
            {
            }

            protected override void Iterate()
            {
                var ids = service.GetVisibleIDs();
                var iter = ids.GetEnumerator();
                int n = 10;
                while (--n >= 0 && iter.MoveNext())
                {
                    string id = (string)iter.Current;
                    object result = service.Get(id);
                    TestFmwk.Logln("iter: " + n + " id: " + id + " result: " + result);
                }
            }
        }

        internal class GetDisplayThread : TestThread
        {
            internal ULocale locale;

            internal GetDisplayThread(string name, ICUService service, long delay, ULocale locale)
                : base("DIS " + name, service, delay)
            {

                this.locale = locale;
            }

            protected override void Iterate()
            {
                var names = GetDisplayNames(service, locale);
                var iter = names.GetEnumerator();
                int n = 10;
                while (--n >= 0 && iter.MoveNext())
                {
                    var e = iter.Current;
                    string dname = (string)e.Key;
                    string id = (string)e.Value;
                    object result = service.Get(id);

                    // Note: IllegalMonitorStateException is thrown by the code
                    // below on IBM JRE5 for AIX 64bit.  For some reason, converting
                    // int to string out of this statement resolves the issue.

                    string num = (n).ToString(CultureInfo.InvariantCulture);
                    TestFmwk.Logln(" iter: " + num +
                            " dname: " + dname +
                            " id: " + id +
                            " result: " + result);
                }
            }
        }

        internal class GetThread : TestThread
        {
            private string[] actualID;

            internal GetThread(string name, ICUService service, long delay)
                : base("GET " + name, service, delay)
            {

                actualID = new string[1];
            }

            protected override void Iterate()
            {
                string id = GetCLV();
                object o = service.Get(id, actualID);
                if (o != null)
                {
                    TestFmwk.Logln(" id: " + id + " actual: " + actualID[0] + " result: " + o);
                }
            }
        }

        internal class GetListThread : TestThread
        {
            private readonly string[] list;
            private int n;

            internal GetListThread(string name, ICUService service, long delay, string[] list)
                : base("GETL " + name, service, delay)
            {

                this.list = list;
            }

            protected override void Iterate()
            {
                if (--n < 0)
                {
                    n = list.Length - 1;
                }
                string id = list[n];
                object o = service.Get(id);
                TestFmwk.Logln(" id: " + id + " result: " + o);
            }
        }

        // return a collection of unique factories, might be fewer than requested
        internal ICollection<IServiceFactory> GetFactoryCollection(int requested)
        {
            var locales = new HashSet<string>();
            for (int i = 0; i < requested; ++i)
            {
                locales.Add(GetCLV());
            }
            var factories = new List<IServiceFactory>(locales.Count);
            var iter = locales.GetEnumerator();
            while (iter.MoveNext())
            {
                factories.Add(new TestFactory((string)iter.Current));
            }
            return factories;
        }

        internal void RegisterFactories(ICUService service, ICollection<IServiceFactory> c)
        {
            using (var iter = c.GetEnumerator())
            {
                while (iter.MoveNext())
                {
                    service.RegisterFactory((IServiceFactory)iter.Current);
                }
            }
        }

        internal ICUService StableService()
        {
            if (stableService == null)
            {
                stableService = new ICULocaleService();
                RegisterFactories(stableService, GetFactoryCollection(50));
            }
            if (PRINTSTATS) stableService.Stats();  // Enable the stats collection
            return stableService;
        }
        private ICUService stableService;

        // run multiple get on a stable service
        [Test]
        public void Test00_ConcurrentGet()
        {
            for (int i = 0; i < 10; ++i)
            {
                new GetThread("[" + (i).ToString(CultureInfo.InvariantCulture) + "]", StableService(), 0).Start();
            }
            RunThreads();
            if (PRINTSTATS) Console.Out.WriteLine(stableService.Stats());
        }

        // run multiple getVisibleID on a stable service
        [Test]
        public void Test01_ConcurrentGetVisible()
        {
            for (int i = 0; i < 10; ++i)
            {
                new GetVisibleThread("[" + (i).ToString(CultureInfo.InvariantCulture) + "]", StableService(), 0).Start();
            }
            RunThreads();
            if (PRINTSTATS) Console.Out.WriteLine(stableService.Stats());
        }

        // run multiple getDisplayName on a stable service
        [Test]
        public void Test02_ConcurrentGetDisplay()
        {
            string[] localeNames = {
                "en", "es", "de", "fr", "zh", "it", "no", "sv"
            };
            for (int i = 0; i < localeNames.Length; ++i)
            {
                string locale = localeNames[i];
                new GetDisplayThread("[" + locale + "]",
                                     StableService(),
                                     0,
                                     new ULocale(locale)).Start();
            }
            RunThreads();
            if (PRINTSTATS) Console.Out.WriteLine(stableService.Stats());
        }

        // run register/unregister on a service
        [Test]
        public void Test03_ConcurrentRegUnreg()
        {
            ICUService service = new ICULocaleService();
            if (PRINTSTATS) service.Stats();    // Enable the stats collection
            for (int i = 0; i < 5; ++i)
            {
                new RegisterFactoryThread("[" + i + "]", service, 0).Start();
            }
            for (int i = 0; i < 5; ++i)
            {
                new UnregisterFactoryThread("[" + i + "]", service, 0).Start();
            }
            RunThreads();
            if (PRINTSTATS) Console.Out.WriteLine(service.Stats());
        }

        [Test]
        public void Test04_WitheringService()
        {
            ICUService service = new ICULocaleService();
            if (PRINTSTATS) service.Stats();    // Enable the stats collection

            var fc = GetFactoryCollection(50);
            RegisterFactories(service, fc);

            IServiceFactory[] factories = (IServiceFactory[])fc.ToArray();
            var comp = new AnonymousComparer<object>(compare: (lhs, rhs) =>
            {
                return lhs.ToString().CompareToOrdinal(rhs.ToString());
            });
            //    Comparator comp = new Comparator() {
            //                @Override
            //                public int compare(object lhs, object rhs)
            //    {
            //        return lhs.toString().compareTo(rhs.toString());
            //    }
            //};
            Array.Sort(factories, comp);

            new GetThread("", service, 0).Start();
            new UnregisterFactoryListThread("", service, 3, factories).Start();

            RunThreads(2000);
            if (PRINTSTATS) Console.Out.WriteLine(service.Stats());
        }

        // "all hell breaks loose"
        // one register and one unregister thread, delay 500ms
        // two display threads with different locales, delay 500ms;
        // one visible id thread, delay 50ms
        // fifteen get threads, delay 0
        // run for ten seconds
        [Test]
        public void Test05_ConcurrentEverything()
        {
            ICUService service = new ICULocaleService();
            if (PRINTSTATS) service.Stats();    // Enable the stats collection

            new RegisterFactoryThread("", service, 500).Start();

            for (int i = 0; i < 15; ++i)
            {
                new GetThread("[" + (i).ToString(CultureInfo.InvariantCulture) + "]", service, 0).Start();
            }

            new GetVisibleThread("", service, 50).Start();

            string[] localeNames = {
                "en", "de"
            };
            for (int i = 0; i < localeNames.Length; ++i)
            {
                string locale = localeNames[i];
                new GetDisplayThread("[" + locale + "]",
                                     StableService(),
                                     500,
                                     new ULocale(locale)).Start();
            }

            new UnregisterFactoryThread("", service, 500).Start();

            // yoweee!!!
            RunThreads(9500);
            if (PRINTSTATS) Console.Out.WriteLine(service.Stats());
        }
    }
}
