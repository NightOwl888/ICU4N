using ICU4N.Support;
using ICU4N.Util;
using System;

namespace ICU4N.Impl
{
    public sealed class ICUDebug
    {
        private static string parameters;
        private static bool debug;
        private static bool help;
        public static readonly string javaVersionString;
        public static readonly bool isJDK14OrHigher;
        public static readonly VersionInfo javaVersion;

        static ICUDebug()
        {
            parameters = SystemProperties.GetProperty("ICUDebug");
            debug = parameters != null;
            help = debug && (parameters.Equals("") || parameters.IndexOf("help") != -1);
            if (debug)
            {
                Console.Out.WriteLine("\nICUDebug=" + parameters);
            }
            // ICU4N TODO: Java? Need to translate
            javaVersionString = SystemProperties.GetProperty("java.version", "0");

            javaVersion = GetInstanceLenient(javaVersionString);

            VersionInfo java14Version = VersionInfo.GetInstance("1.4.0");

            isJDK14OrHigher = javaVersion.CompareTo(java14Version) >= 0;
        }

        public static VersionInfo GetInstanceLenient(string s)
        {
            // Extracting ASCII numbers up to 4 delimited by
            // any non digit characters
            int[] ver = new int[4];
            bool numeric = false;
            int i = 0, vidx = 0;
            while (i < s.Length)
            {
                char c = s[i++];
                if (c < '0' || c > '9')
                {
                    if (numeric)
                    {
                        if (vidx == 3)
                        {
                            // up to 4 numbers
                            break;
                        }
                        numeric = false;
                        vidx++;
                    }
                }
                else
                {
                    if (numeric)
                    {
                        ver[vidx] = ver[vidx] * 10 + (c - '0');
                        if (ver[vidx] > 255)
                        {
                            // VersionInfo does not support numbers
                            // greater than 255.  In such case, we
                            // ignore the number and the rest
                            ver[vidx] = 0;
                            break;
                        }
                    }
                    else
                    {
                        numeric = true;
                        ver[vidx] = c - '0';
                    }
                }
            }

            return VersionInfo.GetInstance(ver[0], ver[1], ver[2], ver[3]);
        }


        public static bool Enabled()
        {
            return debug;
        }

        public static bool Enabled(string arg)
        {
            if (debug)
            {
                bool result = parameters.IndexOf(arg) != -1;
                if (help) Console.Out.WriteLine("\nICUDebug.enabled(" + arg + ") = " + result);
                return result;
            }
            return false;
        }

        public static string Value(string arg)
        {
            string result = "false";
            if (debug)
            {
                int index = parameters.IndexOf(arg);
                if (index != -1)
                {
                    index += arg.Length;
                    if (parameters.Length > index && parameters[index] == '=')
                    {
                        index += 1;
                        int limit = parameters.IndexOf(",", index);
                        result = parameters.Substring(index, (limit == -1 ? parameters.Length : limit) - index); // ICU4N: Corrected 2nd parameter
                    }
                    else
                    {
                        result = "true";
                    }
                }

                if (help) Console.Out.WriteLine("\nICUDebug.value(" + arg + ") = " + result);
            }
            return result;
        }

        //    static public void main(String[] args) {
        //        // test
        //        String[] tests = {
        //            "1.3.0",
        //            "1.3.0_02",
        //            "1.3.1ea",
        //            "1.4.1b43",
        //            "___41___5",
        //            "x1.4.51xx89ea.7f",
        //            "1.6_2009",
        //            "10-100-1000-10000",
        //            "beta",
        //            "0",
        //        };
        //        for (int i = 0; i < tests.length; ++i) {
        //            System.out.println(tests[i] + " => " + getInstanceLenient(tests[i]));
        //        }
        //    }
    }
}
