using ICU4N.Text;
using J2N.Threading;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
#if FEATURE_THREADINTERRUPT
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
#if FEATURE_THREADINTERRUPT
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
#if FEATURE_THREADINTERRUPT
                try
                {
#endif
                    th.Join();
#if FEATURE_THREADINTERRUPT
                }
                catch (ThreadInterruptedException e)
                {
                    Errln("Uexpected exception: " + e);
                }
#endif
            }
        }

        // Test for ICU4N#113, race in cache code in AnyTransliterator.
        [Test]
        public void TestConcurrentTransliterationCyrillic()
        {
            Transliterator tx = Transliterator.GetInstance(@"Any-Latin;Latin-ASCII;[\u0000-\u0020\u007f-\uffff] Remove");

            //string result1 = tx.Transliterate("WБ1 289");

            const int jobCount = 2000;
            var results = new ConcurrentBag<string>();
            using var countdown = new CountdownEvent(jobCount);

            for (int i = 0; i < jobCount; i++)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        // Perform transliteration
                        string result = tx.Transliterate("WБ1 289");
                        results.Add(result);
                    }
                    finally
                    {
                        countdown.Signal();
                    }
                });
            }

            // Wait for all threads to complete
            countdown.Wait();

            // Aggregate and print results
            string actual = string.Join(",", results.Distinct());
            Assert.AreEqual("WB1289", actual);
        }
    }
}
