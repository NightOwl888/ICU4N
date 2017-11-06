using ICU4N.Support.Collections;
using ICU4N.Support.Threading;
using ICU4N.TestFramework.Dev.Test;
using ICU4N.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Tests.Dev.Test.Util
{
    /// <summary>
    /// Testing class for VersionInfo
    /// </summary>
    /// <author>Syn Wee Quek</author>
    /// <since>release 2.1 March 01 2002</since>
    public sealed class VersionInfoTest : TestFmwk
    {
        // constructor ---------------------------------------------------

        /**
        * Constructor
        */
        public VersionInfoTest()
        {
        }

        // public methods -----------------------------------------------

        /**
         * Test that the instantiation works
         */
        [Test]
        public void TestInstance()
        {
            for (int i = 0; i < INSTANCE_INVALID_STRING_.Length; i++)
            {
                try
                {
                    VersionInfo.GetInstance(INSTANCE_INVALID_STRING_[i]);
                    Errln("\"" + INSTANCE_INVALID_STRING_[i] +
                          "\" should produce an exception");
                }
                catch (Exception e)
                {
                    Logln("PASS: \"" + INSTANCE_INVALID_STRING_[i] +
                          "\" failed as expected");
                }
            }
            for (int i = 0; i < INSTANCE_VALID_STRING_.Length; i++)
            {
                try
                {
                    VersionInfo.GetInstance(INSTANCE_VALID_STRING_[i]);
                }
                catch (Exception e)
                {
                    Errln("\"" + INSTANCE_VALID_STRING_[i] +
                          "\" should produce an valid version");
                }
            }
            for (int i = 0; i < INSTANCE_INVALID_INT_.Length; i++)
            {
                try
                {
                    GetInstance(INSTANCE_INVALID_INT_[i]);
                    Errln("invalid ints should produce an exception");
                }
                catch (Exception e)
                {
                    Logln("PASS: \"" + Arrays.ToString(INSTANCE_INVALID_INT_[i]) +
                          "\" failed as expected");
                }
            }
            for (int i = 0; i < INSTANCE_VALID_INT_.Length; i++)
            {
                try
                {
                    GetInstance(INSTANCE_VALID_INT_[i]);
                }
                catch (Exception e)
                {
                    Errln("valid ints should not produce an exception");
                }
            }
        }

        /**
         * Test that the comparison works
         */
        [Test]
        public void TestCompare()
        {
            for (int i = 0; i < COMPARE_NOT_EQUAL_STRING_.Length; i += 2)
            {
                VersionInfo v1 =
                            VersionInfo.GetInstance(COMPARE_NOT_EQUAL_STRING_[i]);
                VersionInfo v2 =
                        VersionInfo.GetInstance(COMPARE_NOT_EQUAL_STRING_[i + 1]);
                if (v1.CompareTo(v2) == 0)
                {
                    Errln(COMPARE_NOT_EQUAL_STRING_[i] + " should not equal " +
                          COMPARE_NOT_EQUAL_STRING_[i + 1]);
                }
            }
            for (int i = 0; i < COMPARE_NOT_EQUAL_INT_.Length; i += 2)
            {
                VersionInfo v1 = GetInstance(COMPARE_NOT_EQUAL_INT_[i]);
                VersionInfo v2 = GetInstance(COMPARE_NOT_EQUAL_INT_[i + 1]);
                if (v1.CompareTo(v2) == 0)
                {
                    Errln(Arrays.ToString(COMPARE_NOT_EQUAL_INT_[i]) + " should not equal " +
                          Arrays.ToString(COMPARE_NOT_EQUAL_INT_[i + 1]));
                }
            }
            for (int i = 0; i < COMPARE_EQUAL_STRING_.Length - 1; i++)
            {
                VersionInfo v1 =
                            VersionInfo.GetInstance(COMPARE_EQUAL_STRING_[i]);
                VersionInfo v2 =
                        VersionInfo.GetInstance(COMPARE_EQUAL_STRING_[i + 1]);
                if (v1.CompareTo(v2) != 0)
                {
                    Errln(COMPARE_EQUAL_STRING_[i] + " should equal " +
                          COMPARE_EQUAL_STRING_[i + 1]);
                }
            }
            for (int i = 0; i < COMPARE_EQUAL_INT_.Length - 1; i++)
            {
                VersionInfo v1 = GetInstance(COMPARE_EQUAL_INT_[i]);
                VersionInfo v2 = GetInstance(COMPARE_EQUAL_INT_[i + 1]);
                if (v1.CompareTo(v2) != 0)
                {
                    Errln(Arrays.ToString(COMPARE_EQUAL_INT_[i]) + " should equal " +
                            Arrays.ToString(COMPARE_EQUAL_INT_[i + 1]));
                }
            }
            for (int i = 0; i < COMPARE_LESS_.Length - 1; i++)
            {
                VersionInfo v1 = VersionInfo.GetInstance(COMPARE_LESS_[i]);
                VersionInfo v2 = VersionInfo.GetInstance(COMPARE_LESS_[i + 1]);
                if (v1.CompareTo(v2) >= 0)
                {
                    Errln(COMPARE_LESS_[i] + " should be less than " +
                          COMPARE_LESS_[i + 1]);
                }
                if (v2.CompareTo(v1) <= 0)
                {
                    Errln(COMPARE_LESS_[i + 1] + " should be greater than " +
                          COMPARE_LESS_[i]);
                }
            }
        }

        /**
         * Test that the getter function works
         */
        [Test]
        public void TestGetter()
        {
            for (int i = 0; i < GET_STRING_.Length; i++)
            {
                VersionInfo v = VersionInfo.GetInstance(GET_STRING_[i]);
                if (v.Major != GET_RESULT_[i << 2] ||
                    v.Minor != GET_RESULT_[(i << 2) + 1] ||
                    v.Milli != GET_RESULT_[(i << 2) + 2] ||
                    v.Micro != GET_RESULT_[(i << 2) + 3])
                {
                    Errln(GET_STRING_[i] + " should return major=" +
                          GET_RESULT_[i << 2] + " minor=" +
                          GET_RESULT_[(i << 2) + 1] + " milli=" +
                          GET_RESULT_[(i << 2) + 2] + " micro=" +
                          GET_RESULT_[(i << 2) + 3]);
                }
                v = GetInstance(GET_INT_[i]);
                if (v.Major != GET_RESULT_[i << 2] ||
                    v.Minor != GET_RESULT_[(i << 2) + 1] ||
                    v.Milli != GET_RESULT_[(i << 2) + 2] ||
                    v.Micro != GET_RESULT_[(i << 2) + 3])
                {
                    Errln(GET_STRING_[i] + " should return major=" +
                          GET_RESULT_[i << 2] + " minor=" +
                          GET_RESULT_[(i << 2) + 1] + " milli=" +
                          GET_RESULT_[(i << 2) + 2] + " micro=" +
                          GET_RESULT_[(i << 2) + 3]);
                }
            }
        }

        /**
         * Test toString()
         */
        [Test]
        public void TesttoString()
        {
            for (int i = 0; i < TOSTRING_STRING_.Length; i++)
            {
                VersionInfo v = VersionInfo.GetInstance(TOSTRING_STRING_[i]);
                if (!v.ToString().Equals(TOSTRING_RESULT_[i]))
                {
                    Errln("toString() for " + TOSTRING_STRING_[i] +
                          " should produce " + TOSTRING_RESULT_[i]);
                }
                v = GetInstance(TOSTRING_INT_[i]);
                if (!v.ToString().Equals(TOSTRING_RESULT_[i]))
                {
                    Errln("toString() for " + Arrays.ToString(TOSTRING_INT_[i]) +
                          " should produce " + TOSTRING_RESULT_[i]);
                }
            }
        }

        /**
         * Test Comparable interface
         */
        [Test]
        public void TestComparable()
        {
            for (int i = 0; i < COMPARE_NOT_EQUAL_STRING_.Length; i += 2)
            {
                VersionInfo v1 = VersionInfo.GetInstance(COMPARE_NOT_EQUAL_STRING_[i]);
                VersionInfo v2 = VersionInfo.GetInstance(COMPARE_NOT_EQUAL_STRING_[i + 1]);
                if (v1.CompareTo(v2) == 0)
                {
                    Errln(COMPARE_NOT_EQUAL_STRING_[i] + " should not equal " +
                          COMPARE_NOT_EQUAL_STRING_[i + 1]);
                }
            }
            for (int i = 0; i < COMPARE_EQUAL_STRING_.Length - 1; i++)
            {
                VersionInfo v1 = VersionInfo.GetInstance(COMPARE_EQUAL_STRING_[i]);
                VersionInfo v2 = VersionInfo.GetInstance(COMPARE_EQUAL_STRING_[i + 1]);
                if (v1.CompareTo(v2) != 0)
                {
                    Errln(COMPARE_EQUAL_STRING_[i] + " should equal " +
                          COMPARE_EQUAL_STRING_[i + 1]);
                }
            }
        }

        /**
         * Test equals and hashCode
         */
        [Test]
        public void TestEqualsAndHashCode()
        {
            VersionInfo v1234a = VersionInfo.GetInstance(1, 2, 3, 4);
            VersionInfo v1234b = VersionInfo.GetInstance(1, 2, 3, 4);
            VersionInfo v1235 = VersionInfo.GetInstance(1, 2, 3, 5);

            assertEquals("v1234a and v1234b", v1234a, v1234b);
            assertEquals("v1234a.hashCode() and v1234b.hashCode()", v1234a.GetHashCode(), v1234b.GetHashCode());
            assertNotEquals("v1234a and v1235", v1234a, v1235);
        }
        // private methods --------------------------------------------------

        /**
         * int array versioninfo creation
         */
        private static VersionInfo GetInstance(int[] data)
        {
            switch (data.Length)
            {
                case 1:
                    return VersionInfo.GetInstance(data[0]);
                case 2:
                    return VersionInfo.GetInstance(data[0], data[1]);
                case 3:
                    return VersionInfo.GetInstance(data[0], data[1], data[2]);
                default:
                    return VersionInfo.GetInstance(data[0], data[1], data[2],
                                                   data[3]);
            }
        }

        // private data members --------------------------------------------

        /**
         * Test instance data
         */
        private static readonly String[] INSTANCE_INVALID_STRING_ = {
            "a",
            "-1",
            "-1.0",
            "-1.0.0",
            "-1.0.0.0",
            "0.-1",
            "0.0.-1",
            "0.0.0.-1",
            "256",
            "256.0",
            "256.0.0",
            "256.0.0.0",
            "0.256",
            "0.0.256",
            "0.0.0.256",
            "1.2.3.4.5"
        };
        private static readonly String[] INSTANCE_VALID_STRING_ = {
            "255",
            "255.255",
            "255.255.255",
            "255.255.255.255"
        };
        private static readonly int[][] INSTANCE_INVALID_INT_ = {
            new int[] {-1},
            new int[] {-1, 0},
            new int[] {-1, 0, 0},
            new int[] {-1, 0, 0, 0},
            new int[] {0, -1},
            new int[] {0, 0, -1},
            new int[] {0, 0, 0, -1},
            new int[] {256},
            new int[] {256, 0},
            new int[] {256, 0, 0},
            new int[] {256, 0, 0, 0},
            new int[] {0, 256},
            new int[] {0, 0, 256},
            new int[] {0, 0, 0, 256},
        };
        private static readonly int[][] INSTANCE_VALID_INT_ = {
            new int[] {255},
            new int[] {255, 255},
            new int[] {255, 255, 255},
            new int[] {255, 255, 255, 255}
        };

        /**
         * Test compare data
         */
        private static readonly String[] COMPARE_NOT_EQUAL_STRING_ = {
            "2.0.0.0", "3.0.0.0"
        };
        private static readonly int[][] COMPARE_NOT_EQUAL_INT_ = {
            new int[] {2, 0, 0, 0}, new int[] {3, 0, 0, 0}
        };
        private static readonly String[] COMPARE_EQUAL_STRING_ = {
            "2.0.0.0", "2.0.0", "2.0", "2"
        };
        private static readonly int[][] COMPARE_EQUAL_INT_ = {
            new int[] {2}, new int[] {2, 0}, new int[] {2, 0, 0}, new int[] {2, 0, 0, 0}
        };
        private static readonly String[] COMPARE_LESS_ = {
            "0", "0.0.0.1", "0.0.1", "0.1", "1", "2", "2.1", "2.1.1", "2.1.1.1"
        };

        /**
         * Test Getter data
         */
        private static readonly String[] GET_STRING_ = {
            "0",
            "1.1",
            "2.1.255",
            "3.1.255.100"
        };
        private static readonly int[][] GET_INT_ = {
            new int[] {0},
            new int[] {1, 1},
            new int[] {2, 1, 255},
            new int[] {3, 1, 255, 100}
        };
        private static readonly int[] GET_RESULT_ = {
            0, 0, 0, 0,
            1, 1, 0, 0,
            2, 1, 255, 0,
            3, 1, 255, 100
        };

        /**
         * Test toString data
         */
        private static readonly String[] TOSTRING_STRING_ = {
            "0",
            "1.1",
            "2.1.255",
            "3.1.255.100"
        };
        private static readonly int[][] TOSTRING_INT_ = {
            new int[] {0},
            new int[] {1, 1},
            new int[] {2, 1, 255},
            new int[] {3, 1, 255, 100}
        };
        private static readonly String[] TOSTRING_RESULT_ = {
            "0.0.0.0",
            "1.1.0.0",
            "2.1.255.0",
            "3.1.255.100"
        };

        /*
         * Test case for multi-threading problem reported by ticket#7880
         */
        [Test]
        public void TestMultiThread()
        {
            int numThreads = 20;
            GetInstanceWorker[] workers = new GetInstanceWorker[numThreads];
            VersionInfo[][] results = Arrays.NewRectangularArray<VersionInfo>(numThreads, 255);// VersionInfo[numThreads][255];

            // Create workers
            for (int i = 0; i < workers.Length; i++)
            {
                workers[i] = new GetInstanceWorker(i, results[i]);
            }

            // Start workers
            for (int i = 0; i < workers.Length; i++)
            {
                workers[i].Start();
            }

            // Wait for the completion
            for (int i = 0; i < workers.Length; i++)
            {
#if !NETCOREAPP1_0
                        try
                        {
#endif
                workers[i].Join();
#if !NETCOREAPP1_0
                        }
                        catch (ThreadInterruptedException e)
                        {
                            Errln("A problem in thread execution. " + e.getMessage());
                        }
#endif
            }


            // Check if singleton for each
            for (int i = 1; i < results.Length; i++)
            {
                for (int j = 0; j < results[0].Length; j++)
                {
                    if (results[0][j] != results[i][j])
                    {
                        Errln("Different instance at index " + j + " Thread#" + i);
                    }
                }
            }
        }

        private class GetInstanceWorker : ThreadWrapper
        {
            private VersionInfo[] results;

            internal GetInstanceWorker(int serialNumber, VersionInfo[] results)
                : base("GetInstnaceWorker#" + serialNumber)
            {
                this.results = results;
            }

            public override void Run()
            {
                for (int i = 0; i < results.Length; i++)
                {
                    results[i] = VersionInfo.GetInstance(i);
                }
            }
        }
    }
}
