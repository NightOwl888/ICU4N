using J2N.Threading;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace ICU4N.Dev.Test.Lang
{
    /// <author>aheninger</author>
    public class UCharacterThreadTest : TestFmwk
    {
        // constructor -----------------------------------------------------------

        /**
        * Private constructor to prevent initialisation
        */
        public UCharacterThreadTest()
        {
        }

        // public methods --------------------------------------------------------

        //
        //  Test multi-threaded parallel calls to UCharacter.getName(codePoint)
        //  Regression test for ticket 6264.
        //
        [Test]
        public void TestUCharactersGetName()
        {
            List<GetNameThread> threads = new List<GetNameThread>();
            for (int t = 0; t < 20; t++)
            {
                int codePoint = 47 + t;
                String correctName = UChar.GetName(codePoint);
                GetNameThread thread = new GetNameThread(codePoint, correctName);
                thread.Start();
                threads.Add(thread);
            }
            foreach (var thread in threads)
            {
                thread.Join();
                if (!thread.correctName.Equals(thread.actualName))
                {
                    Errln("FAIL, expected \"" + thread.correctName + "\", got \"" + thread.actualName + "\"");
                }
            }
        }

        private class GetNameThread : ThreadJob
        {
            internal readonly int codePoint;
            internal readonly String correctName;
            internal String actualName;

            internal GetNameThread(int codePoint, String correctName)
            {
                this.codePoint = codePoint;
                this.correctName = correctName;
            }

            public override void Run()
            {
                for (int i = 0; i < 10000; i++)
                {
                    actualName = UChar.GetName(codePoint);
                    if (!correctName.Equals(actualName))
                    {
                        break;
                    }
                }
            }
        }
    }
}
