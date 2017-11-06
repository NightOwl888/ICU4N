using ICU4N.Support.Threading;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.TestFramework.Dev.Test
{
    public sealed class TestUtil
    {
        /**
     * Path to test data in icu4jtest.jar
     */
        public static readonly String DATA_PATH = "ICU4N.TestFramework.Dev.Data.";

        /**
         * Return an input stream on the data file at path 'name' rooted at the data path
         */
        public static Stream GetDataStream(String name)
        {
            Stream input = null;
            try
            {
                input = typeof(TestUtil).GetTypeInfo().Assembly.GetManifestResourceStream(DATA_PATH + name);
            }
            catch (Exception t)
            {
                throw new IOException("data resource '" + name + "' not found");
            }
            finally
            {
                if (input == null)
                    throw new IOException("data resource '" + name + "' not found");
            }
            return input;
        }

        /**
         * Return a buffered reader on the data file at path 'name' rooted at the data path.
         */
        public static TextReader GetDataReader(String name, String charset)
        {
            Stream input = GetDataStream(name);
            StreamReader isr =
                    charset == null
                        ? new StreamReader(input)
                        : new StreamReader(input, Encoding.GetEncoding(charset));
            return isr;
        }

        /**
         * Return a buffered reader on the data file at path 'name' rooted at the data path,
         * using the provided encoding.
         */
        public static TextReader GetDataReader(String name)
        {
            return GetDataReader(name, null);
        }

        static readonly char[] DIGITS =
            {
            '0',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9',
            'A',
            'B',
            'C',
            'D',
            'E',
            'F',
            'G',
            'H',
            'I',
            'J',
            'K',
            'L',
            'M',
            'N',
            'O',
            'P',
            'Q',
            'R',
            'S',
            'T',
            'U',
            'V',
            'W',
            'X',
            'Y',
            'Z' };
        /**
         * Return true if the character is NOT printable ASCII.  The tab,
         * newline and linefeed characters are considered unprintable.
         */
        public static bool IsUnprintable(int c)
        {
            return !(c >= 0x20 && c <= 0x7E);
        }
        /**
         * Escape unprintable characters using <backslash>uxxxx notation
         * for U+0000 to U+FFFF and <backslash>Uxxxxxxxx for U+10000 and
         * above.  If the character is printable ASCII, then do nothing
         * and return FALSE.  Otherwise, append the escaped notation and
         * return TRUE.
         */
        public static bool EscapeUnprintable(StringBuffer result, int c)
        {
            if (IsUnprintable(c))
            {
                result.Append('\\');
                if ((c & ~0xFFFF) != 0)
                {
                    result.Append('U');
                    result.Append(DIGITS[0xF & (c >> 28)]);
                    result.Append(DIGITS[0xF & (c >> 24)]);
                    result.Append(DIGITS[0xF & (c >> 20)]);
                    result.Append(DIGITS[0xF & (c >> 16)]);
                }
                else
                {
                    result.Append('u');
                }
                result.Append(DIGITS[0xF & (c >> 12)]);
                result.Append(DIGITS[0xF & (c >> 8)]);
                result.Append(DIGITS[0xF & (c >> 4)]);
                result.Append(DIGITS[0xF & c]);
                return true;
            }
            return false;
        }

        internal class Lock
        {
            private int count;

            internal void Inc()
            {
                lock (this)
                    ++count;
            }

            internal void Dec()
            {
                lock (this)
                    --count;
            }

            internal int Count
            {
                get
                {
                    lock (this)
                        return count;
                }
            }

            internal void Go()
            {
#if !NETSTANDARD1_3
                try
                {
#endif
                while (Count > 0)
                {
                    lock (this)
                    {
                        Monitor.PulseAll(this);
                    }
                    Thread.Sleep(50);
                }
#if !NETSTANDARD1_3
                }
                catch (ThreadInterruptedException e)
                {
                }
#endif
            }
        }

        internal class TestThread : ThreadWrapper
        {
            Lock @lock;
            Action target;

            internal TestThread(Lock @lock, Action target)
            {
                this.@lock = @lock;
                this.target = target;

                @lock.Inc();
            }

            public override void Run()
            {
#if !NETSTANDARD1_3
                try
                {
#endif
                lock (@lock)
                {
                    Monitor.Wait(@lock);
                }
                target();
#if !NETSTANDARD1_3
                }
                catch (ThreadInterruptedException e)
                {
                }
#endif

                @lock.Dec();
            }
        }

        public static void RunUntilDone(Action[] targets)
        {
            if (targets == null)
            {
                throw new ArgumentException("targets is null");
            }
            if (targets.Length == 0)
            {
                return;
            }

            Lock @lock = new Lock();
            for (int i = 0; i < targets.Length; ++i)
            {
                new TestThread(@lock, targets[i]).Start();
            }

            @lock.Go();
        }
        public static TextReader OpenUTF8Reader(String dir, String filename)
        {
            return OpenReader(dir, filename, Encoding.UTF8);
        }
        public static TextReader OpenReader(String dir, String filename, Encoding encoding)
        {
            //File file = new File(dir + filename);
            return
                new StreamReader(
                    new FileStream(Path.Combine(dir, filename), FileMode.Open, FileAccess.Read, FileShare.Read),
                    encoding);
        }

        // ICU4N NOTE: Not applicable
        //    public enum JavaVendor
        //{
        //    Unknown,
        //    Oracle,
        //    IBM,
        //    Android
        //}

        //public static JavaVendor getJavaVendor()
        //{
        //    JavaVendor vendor = JavaVendor.Unknown;
        //    String javaVendorProp = System.getProperty("java.vendor", "").toLowerCase(Locale.US).trim();
        //    if (javaVendorProp.startsWith("ibm"))
        //    {
        //        vendor = JavaVendor.IBM;
        //    }
        //    else if (javaVendorProp.startsWith("sun") || javaVendorProp.startsWith("oracle"))
        //    {
        //        vendor = JavaVendor.Oracle;
        //    }
        //    else if (javaVendorProp.contains("android"))
        //    {
        //        vendor = JavaVendor.Android;
        //    }
        //    return vendor;
        //}

        //public static int getJavaVersion()
        //{
        //    int ver = -1;
        //    String verstr = System.getProperty("java.version");
        //    if (verstr != null)
        //    {
        //        String majorVerStr = null;
        //        if (verstr.StartsWith("1.", StringComparison.Ordinal))
        //        {
        //            String[] numbers = verstr.split("\\.");
        //            if (numbers.length > 1)
        //            {
        //                majorVerStr = numbers[1];
        //            }
        //        }
        //        else
        //        {
        //            String[] numbers = verstr.split("\\.|-");
        //            if (numbers.length > 0)
        //            {
        //                majorVerStr = numbers[0];
        //            }
        //        }
        //        if (majorVerStr != null)
        //        {
        //            try
        //            {
        //                ver = Integer.parseInt(majorVerStr);
        //            }
        //            catch (FormatException e)
        //            {
        //                ver = -1;
        //            }
        //        }
        //    }
        //    return ver;
        //}
    }
}
