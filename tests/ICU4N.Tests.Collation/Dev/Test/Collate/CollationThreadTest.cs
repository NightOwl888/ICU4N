using ICU4N.Text;
using J2N;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Threading;
using Random = System.Random;

namespace ICU4N.Dev.Test.Collate
{
    public class CollationThreadTest : TestFmwk
    {
        private static readonly String[] threadTestData = LoadCollationThreadTestData();
        private static string[] LoadCollationThreadTestData()
        {
            Collator collator = Collator.GetInstance(new CultureInfo("pl"));
            String[] temporaryData = {
                "Banc Se\u00F3kdyaaouq Pfuymjec",
                "BSH \u00F3y",
                "ABB - \u00F3g",
                "G\u00F3kpo Adhdoetpwtx Twxma, qm. Ilnudx",
                "G\u00F3bjh Zcgopqmjidw Dyhlu, ky. Npyamr",
                "G\u00F3dxb Slfduvgdwfi Qhreu, ao. Adyfqx",
                "G\u00F3ten Emrmbmttgne Rtpir, rx. Mgmpjy",
                "G\u00F3kjo Hciqkymfcds Jpudo, ti. Ueceedbm (tkvyj vplrnpoq)",
                "Przjrpnbhrflnoo Dbiccp Lnmikfhsuo\u00F3s Tgfhlpqoso / ZAD ENR",
                "Bang Nbygmoyc Nd\u00F3nipcryjtzm",
                "Citjk\u00EBd Qgmgvr Er. w u.x.",
                "Dyrscywp Kvoifmyxo Ivv\u00F3r Lbyxtrwnzp",
                "G\u00E9awk Ssqenl Pk. c r.g.",
                "Nesdo\u00E9 Ilwbay Z.U.",
                "Poczsb Lrdtqg",
                "Pocafu Tgbmpn - wwg zo Mpespnzdllqk",
                "Polyvmg Z.C.",
                "POLUHONANQ FO",
                "Polrpycn",
                "Poleeaw-Rqzghgnnj R.W.",
                "Polyto Sgrgcvncz",
                "Polixj Tyfc\u00F3vcga Gbkjxf\u00F3f Tuogcybbbkyd C.U.",
                "Poltmzzlrkwt",
                "Polefgb Oiqefrkq",
                "Polrfdk K\u00F3nvyrfot Xuzbzzn f Ujmfwkdbnzh E.U. Wxkfiwss",
                "Polxtcf Hfowus Zzobblfm N.I.",
                "POLJNXO ZVYU L.A.",
                "PP Lowyr Rmknyoew",
                "Pralpe",
                "Preyojy Qnrxr",
                "PRK -5",
                "PRONENC U.P.",
                "Prowwyq & Relnda Hxkvauksnn Znyord Tz. w t.o.",
                "Propydv Afobbmhpg",
                "Proimpoupvp",
                "Probfo Hfttyr",
                "Propgi Lutgumnj X.W. BL",
                "Prozkch K.E.",
                "Progiyvzr Erejqk T.W.",
                "Prooxwq-Ydglovgk J.J.",
                "PTU Ntcw Lwkxjk S.M. UYF",
                "PWN",
                "PWP",
                "PZU I.D. Tlpzmhax",
                "PZU ioii A.T. Yqkknryu - bipdq badtg 500/9",
                "Qumnl-Udffq",
                "Radmvv",
                "Railoggeqd Aewy Fwlmsp K.S. Ybrqjgyr",
                "Remhmxkx Ewuhxbg",
                "Renafwp Sapnqr io v z.n.",
                "Repqbpuuo",
                "Resflig",
                "Rocqz Mvwftutxozs VQ",
                "Rohkui",
                "RRC",
                "Samgtzg Fkbulcjaaqv Ollllq Ad. l l.v.",
                "Schelrlw Fu. t z.x.",
                "Schemxgoc Axvufoeuh",
                "Siezsxz Eb. n r.h",
                "Sikj Wyvuog",
                "Sobcwssf Oy. q o.s. Kwaxj",
                "Sobpxpoc Fb. w q.h. Elftx",
                "Soblqeqs Kpvppc RH - tbknhjubw siyaenc Njsjbpx Buyshpgyv",
                "Sofeaypq FJ",
                "Stacyok Qurqjw Hw. f c.h.",
                "STOWN HH",
                "Stopjhmq Prxhkakjmalkvdt Weqxejbyig Wgfplnvk D.C.",
                "STRHAEI Clydqr Ha. d z.j.",
                "Sun Clvaqupknlk",
                "TarfAml",
                "Tchukm Rhwcpcvj Cc. v y.a.",
                "Teco Nyxm Rsvzkx pm. J a.t.",
                "Tecdccaty",
                "Telruaet Nmyzaz Twwwuf",
                "Tellrwihv Xvtjle N.U.",
                "Telesjedc Boewsx A.F",
                "tellqfwiqkv dinjlrnyit yktdhlqquihzxr (ohvso)",
                "Tetft Kna Ab. j l.z.",
                "Thesch",
                "Totqucvhcpm Gejxkgrz Is. e k.i.",
                "Towajgixetj Ngaayjitwm fj csxm Mxebfj Sbocok X.H.",
                "Toyfon Meesp Neeban Jdsjmrn sz v z.w.",
                "TRAJQ NZHTA Li. n x.e. - Vghfmngh",
                "Triuiu",
                "Tripsq",
                "TU ENZISOP ZFYIPF V.U.",
                "TUiX Kscdw G.G.",
                "TVN G.A.",
                "Tycd",
                "Unibjqxv rdnbsn - ZJQNJ XCG / Wslqfrk",
                "Unilcs - hopef ps 20 nixi",
                "UPC Gwwmru Ds. g o.r.",
                "Vaidgoav",
                "Vatyqzcgqh Kjnnsy GQ WT",
                "Volhz",
                "Vos Jviggogjt Iyqhlm Ih. w j.y. (fbshoihdnb)",
                "WARMFC E.D.",
                "Wincqk Pqadskf",
                "WKRD",
                "Wolk Pyug",
                "WPRV",
                "WSiI",
                "Wurag XZ",
                "Zacrijl B.B.",
                "Zakja Tziaboysenum Squlslpp - Diifw V.D.",
                "Zakgat Meqivadj Nrpxlekmodx s Bbymjozge W.Y.",
                "Zjetxpbkpgj Mmhhgohasjtpkjd Uwucubbpdj K.N.",
                "ZREH"
            };
            Sort(temporaryData, collator);
            return temporaryData;
        }

        private static void Scramble(String[] data, System.Random r)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                int ix = r.Next(data.Length);
                String s = data[i];
                data[i] = data[ix];
                data[ix] = s;
            }
        }

        private static void Sort(String[] data, Collator collator)
        {
            Array.Sort(data, collator);
        }

        private static bool VerifySort(String[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (!data[i].Equals(threadTestData[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private class Control
        {
            private bool go;
            internal String fail;

            internal void Start()
            {
                lock (this)
                {
                    go = true;
                    Monitor.PulseAll(this);
                }
            }

            internal void Stop()
            {
                lock (this)
                {
                    go = false;
                    Monitor.PulseAll(this);
                }
            }

            internal bool Go()
            {
                return go;
            }

            internal void Fail(String msg)
            {
                fail = msg;
                Stop();
            }
        }

        private class Test //implements Runnable
        {
            private String[] data;
            private Collator collator;
            private String name;
            private Control control;
            private System.Random r;

            internal Test(String name, String[] data, Collator collator, System.Random r, Control control)
            {
                this.name = name;
                this.data = data;
                this.collator = collator;
                this.control = control;
                this.r = r;
            }

            public void Run()
            {
                try
                {
                    lock (control)
                    {
                        while (!control.Go())
                        {
                            Monitor.Wait(control);
                        }
                    }

                    while (control.Go())
                    {
                        Scramble(this.data, r);
                        Sort(this.data, this.collator);
                        if (!VerifySort(this.data))
                        {
                            control.Fail(name + ": incorrect sort");
                        }
                    }
                }
#if !NETCOREAPP1_0
                catch (ThreadInterruptedException e)
                {
                    // die
                }
#endif
                catch (IndexOutOfRangeException e)
                {
                    control.Fail(name + " " + e.ToString());
                }
            }
        }

        private void RunThreads(Thread[] threads, Control control)
        {
            for (int i = 0; i < threads.Length; ++i)
            {
                threads[i].Start();
            }

#if !NETCOREAPP1_0
            try
            {
#endif
                control.Start();

                long stopTime = Time.CurrentTimeMilliseconds() + 5000;
                do
                {
                    Thread.Sleep(100);
                } while (control.Go() && Time.CurrentTimeMilliseconds() < stopTime);

                control.Stop();

                for (int i = 0; i < threads.Length; ++i)
                {
                    threads[i].Join();
                }
#if !NETCOREAPP1_0
            }
            catch (ThreadInterruptedException e)
            {
                // die
            }
#endif

            if (control.fail != null)
            {
                Errln(control.fail);
            }
        }

        [Test]
        public void TestThreads()
        {
            Collator theCollator = Collator.GetInstance(new CultureInfo("pl"));
            Random r = new Random();
            Control control = new Control();

            Thread[] threads = new Thread[10];
            for (int i = 0; i < threads.Length; ++i)
            {
                Collator coll;
                //try
                //{
                coll = (Collator)theCollator.Clone();
                //}
                //catch (CloneNotSupportedException e)
                //{
                //    // should not happen, if it does we'll get an exception right away
                //    Errln("could not clone");
                //    return;
                //}
                Test test = new Test("Collation test thread" + i, (string[])threadTestData.Clone(), coll,
                        r, control);
                threads[i] = new Thread(() => test.Run());
            }

            RunThreads(threads, control);
        }

        [Test]
        public void TestFrozen()
        {
            Collator theCollator = Collator.GetInstance(new CultureInfo("pl"));
            theCollator.Freeze();
            Random r = new Random();
            Control control = new Control();

            Thread[] threads = new Thread[10];
            for (int i = 0; i < threads.Length; ++i)
            {
                Test test = new Test("Frozen collation test thread " + i, (string[])threadTestData.Clone(), theCollator,
                        r, control);
                threads[i] = new Thread(() => test.Run());
            }

            RunThreads(threads, control);
        }
    }
}
