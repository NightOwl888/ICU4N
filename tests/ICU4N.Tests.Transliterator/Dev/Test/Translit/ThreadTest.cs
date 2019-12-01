using ICU4N.Text;
using J2N.Threading;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ICU4N.Dev.Test.Translit
{
    /// <summary>
    /// Test for ICU Ticket #7201.  With threading bugs in RuleBasedTransliterator, this
    /// test would reliably crash.
    /// </summary>
    public class ThreadTest : TestFmwk
    {
        private List<Worker> threads = new List<Worker>();
        private int iterationCount = 50000;

        [Test]
        public void TestThreads()
        {
            if (TestFmwk.GetExhaustiveness() >= 9)
            {
                // Exhaustive test.  Run longer.
                iterationCount = 1000000;
            }

            for (int i = 0; i < 8; i++)
            {
                Worker thread = new Worker(this);
                threads.Add(thread);
                thread.Start();
            }
            long expectedCount = 0;
            foreach (Worker thread in threads)
            {
#if !NETCOREAPP1_0
                try
                {
#endif
                thread.Join();
                if (expectedCount == 0)
                {
                    expectedCount = thread.count;
                }
                else
                {
                    if (expectedCount != thread.count)
                    {
                        Errln("Threads gave differing results.");
                    }
                }
#if !NETCOREAPP1_0
                }
                catch (ThreadInterruptedException e)
                {
                    Errln(e.ToString());
                }
#endif
            }
        }

        private static readonly String[] WORDS = { "edgar", "allen", "poe" };

        private class Worker : ThreadJob
        {
            public long count = 0;
            private readonly ThreadTest outerInstance;

            public Worker(ThreadTest outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override void Run()
            {
                Transliterator tx = Transliterator.GetInstance("Latin-Thai");
                for (int loop = 0; loop < outerInstance.iterationCount; loop++)
                {
                    foreach (String s in WORDS)
                    {
                        count += tx.Transliterate(s).Length;
                    }
                }
            }
        }

        // Test for ticket #10673, race in cache code in AnyTransliterator.
        // It's difficult to make the original unsafe code actually fail, but
        // this test will fairly reliably take the code path for races in
        // populating the cache.
        //
        [Test]
        public void TestAnyTranslit()
        {
            Transliterator tx = Transliterator.GetInstance("Any-Latin");
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < 8; i++)
            {
                threads.Add(new Thread(() => { tx.Transliterate("διαφορετικούς"); }));
            }
            foreach (Thread th in threads)
            {
                th.Start();
            }
            foreach (Thread th in threads)
            {
#if !NETCOREAPP1_0
                try
                {
#endif
                th.Join();
#if !NETCOREAPP1_0
                }
                catch (ThreadInterruptedException e)
                {
                    Errln("Uexpected exception: " + e);
                }
#endif
            }
        }
    }
}
