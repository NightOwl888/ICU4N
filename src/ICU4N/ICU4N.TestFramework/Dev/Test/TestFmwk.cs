using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using ICU4N.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using StringBuffer = System.Text.StringBuilder;

#if FEATURE_STACKTRACE
using System.Diagnostics;
#endif

namespace ICU4N.TestFramework.Dev.Test
{
    /// <summary>
    /// TestFmwk is a base class for tests that can be run conveniently from the
    /// command line as well as in Visual Studio.
    /// <para/>
    /// Sub-classes implement a set of methods named Test <something>. Each of these
    /// methods performs some test. Test methods should indicate errors by calling
    /// either err or Errln. This will increment the errorCount field and may
    /// optionally print a message to the log. Debugging information may also be
    /// added to the log via the log and logln methods. These methods will add their
    /// arguments to the log only if the test is being run in verbose mode.
    /// </summary>
    public abstract class TestFmwk : AbstractTestLog
    {
        private static readonly object syncLock = new object();

        /**
     * The default time zone for all of our tests. Used in @Before
     */
        // ICU4N NOTE: In .NET, there is no way to set the time zone for the entire app, so
        // we need to use another approach than setting it for the test fixture.
        public readonly static TimeZoneInfo DEFAULT_TIME_ZONE = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"); //TimeZoneInfo.getTimeZone("America/Los_Angeles");

        /**
         * The default locale used for all of our tests. Used in @Before
         */

        private readonly static CultureInfo defaultLocale = new CultureInfo("en-US");

        private static readonly string EXHAUSTIVENESS = "ICU.exhaustive";
        private static readonly int DEFAULT_EXHAUSTIVENESS = 0;
        private static readonly int MAX_EXHAUSTIVENESS = 10;

        private static readonly string LOGGING_LEVEL = "ICU.logging";
        private static readonly int DEFAULT_LOGGING_LEVEL = 0;
        private static readonly int MAX_LOGGING_LEVEL = 3;

        public static readonly int LOGGING_NONE = 0;
        public static readonly int LOGGING_WARN = 1;
        public static readonly int LOGGING_INFO = 2;
        public static readonly int LOGGING_DEBUG = 3;

        private static readonly string SEED = "ICU.seed";
        private static readonly string SECURITY_POLICY = "ICU.securitypolicy";

        private static readonly TestParams testParams;
        static TestFmwk()
        {
            testParams = TestParams.Create();
        }

        protected TestFmwk()
        {
        }

        [SetUp]
        public virtual void testInitialize()
        {
#if NETSTANDARD
            CultureInfo.CurrentCulture = defaultLocale;
#else
            System.Threading.Thread.CurrentThread.CurrentCulture = defaultLocale;
#endif

            //TimeZone.setDefault(defaultTimeZone);

            //if (getParams().testSecurityManager != null)
            //{
            //    System.setSecurityManager(getParams().testSecurityManager);
            //}
        }

        [TearDown]
        public virtual void testTeardown()
        {
            //if (getParams().testSecurityManager != null)
            //{
            //    System.setSecurityManager(getParams().originalSecurityManager);
            //}
        }

        private static TestParams GetParams()
        {
            //return paramsReference.get();
            return testParams;
        }

        protected static bool IsVerbose()
        {
            return GetParams().GetLoggingLevel() >= LOGGING_INFO;
        }

        /**
         * 0 = fewest tests, 5 is normal build, 10 is most tests
         */
        protected static int GetExhaustiveness()
        {
            return GetParams().inclusion;
        }

        protected static bool isQuick()
        {
            return GetParams().GetInclusion() == 0;
        }

        // use this instead of new random so we get a consistent seed
        // for our tests
        protected Random createRandom()
        {
            return new Random(GetParams().GetSeed());
        }

        /**
         * Integer Random number generator, produces positive int values.
         * Similar to C++ std::minstd_rand, with the same algorithm & constants.
         * Provided for compatibility with ICU4C.
         * Get & set of the seed allows for reproducible monkey tests.
         */
        public class ICU_Rand // ICU4N: made public because of accessiblity issues
        {
            private int fLast;

            public ICU_Rand(int seed)
            {
                Seed(seed);
            }

            public int Next()
            {
                fLast = (int)((fLast * 48271L) % 2147483647L);
                return fLast;
            }

            public void Seed(int seed)
            {
                if (seed <= 0)
                {
                    seed = 1;
                }
                seed %= 2147483647;   // = 0x7FFFFFFF
                fLast = seed > 0 ? seed : 1;
            }

            public int GetSeed()
            {
                return fLast;
            }

        }

        static readonly string ICU_TRAC_URL = "http://bugs.icu-project.org/trac/ticket/";
        static readonly string CLDR_TRAC_URL = "http://unicode.org/cldr/trac/ticket/";
        static readonly string CLDR_TICKET_PREFIX = "cldrbug:";

        /**
         * Log the known issue.
         * This method returns true unless -prop:logKnownIssue=no is specified
         * in the argument list.
         *
         * @param ticket A ticket number string. For an ICU ticket, use numeric characters only,
         * such as "10245". For a CLDR ticket, use prefix "cldrbug:" followed by ticket number,
         * such as "cldrbug:5013".
         * @param comment Additional comment, or null
         * @return true unless -prop:logKnownIssue=no is specified in the test command line argument.
         */
        protected static bool logKnownIssue(string ticket, string comment)
        {
            if (!GetBooleanProperty("logKnownIssue", true))
            {
                return false;
            }

            StringBuffer descBuf = new StringBuffer();
            // TODO(junit) : what to do about this?
            //GetParams().stack.appendPath(descBuf);
            if (comment != null && comment.Length > 0)
            {
                descBuf.Append(" (" + comment + ")");
            }
            string description = descBuf.ToString();

            string ticketLink = "Unknown Ticket";
            if (ticket != null && ticket.Length > 0)
            {
                bool isCldr = false;
                ticket = ticket.ToLowerInvariant();
                if (ticket.StartsWith(CLDR_TICKET_PREFIX))
                {
                    isCldr = true;
                    ticket = ticket.Substring(CLDR_TICKET_PREFIX.Length);
                }
                ticketLink = (isCldr ? CLDR_TRAC_URL : ICU_TRAC_URL) + ticket;
            }

            if (GetParams().knownIssues == null)
            {
                GetParams().knownIssues = new SortedDictionary<string, List<string>>();
            }
            List<string> lines = GetParams().knownIssues.Get(ticketLink);
            if (lines == null)
            {
                lines = new List<string>();
                GetParams().knownIssues[ticketLink] = lines;
            }
            if (!lines.Contains(description))
            {
                lines.Add(description);
            }

            return true;
        }

        protected static string GetProperty(string key)
        {
            return GetParams().GetProperty(key);
        }

        protected static bool GetBooleanProperty(string key)
        {
            return GetParams().GetBooleanProperty(key);
        }

        protected static bool GetBooleanProperty(string key, bool defVal)
        {
            return GetParams().GetBooleanProperty(key, defVal);
        }

        protected static int GetIntProperty(string key, int defVal)
        {
            return GetParams().GetIntProperty(key, defVal);
        }

        protected static int GetIntProperty(string key, int defVal, int maxVal)
        {
            return GetParams().GetIntProperty(key, defVal, maxVal);
        }

        protected static TimeZoneInfo safeGetTimeZone(string id)
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(id);
            if (tz == null)
            {
                // should never happen
                Errln("FAIL: TimeZone.GetTimeZone(" + id + ") => null");
            }
            if (!tz.Id.Equals(id))
            {
                Warnln("FAIL: TimeZone.GetTimeZone(" + id + ") => " + tz.Id);
            }
            return tz;
        }


        // Utility Methods

        protected static string Hex(char[] s)
        {
            StringBuffer result = new StringBuffer();
            for (int i = 0; i < s.Length; ++i)
            {
                if (i != 0) result.Append(',');
                result.Append(Hex(s[i]));
            }
            return result.ToString();
        }

        protected static string Hex(byte[] s)
        {
            StringBuffer result = new StringBuffer();
            for (int i = 0; i < s.Length; ++i)
            {
                if (i != 0) result.Append(',');
                result.Append(Hex(s[i]));
            }
            return result.ToString();
        }

        protected static string Hex(char ch)
        {
            StringBuffer result = new StringBuffer();
            string foo = Convert.ToString(ch, 16).ToUpper();
            for (int i = foo.Length; i < 4; ++i)
            {
                result.Append('0');
            }
            return result + foo;
        }

        protected static string Hex(int ch)
        {
            StringBuffer result = new StringBuffer();
            string foo = Convert.ToString(ch, 16).ToUpper();
            for (int i = foo.Length; i < 4; ++i)
            {
                result.Append('0');
            }
            return result + foo;
        }

        protected static string Hex(string s)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < s.Length; ++i)
            {
                if (i != 0)
                    result.Append(',');
                result.Append(Hex(s[i]));
            }
            return result.ToString();
        }

        protected static string Hex(StringBuffer s)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < s.Length; ++i)
            {
                if (i != 0)
                    result.Append(',');
                result.Append(Hex(s[i]));
            }
            return result.ToString();
        }

        internal static string Hex(ICharSequence s)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < s.Length; ++i)
            {
                if (i != 0)
                    result.Append(',');
                result.Append(Hex(s[i]));
            }
            return result.ToString();
        }

        protected static string Prettify(string s)
        {
            return Prettify(s.ToCharSequence());
        }

        internal static string Prettify(ICharSequence s)
        {
            StringBuilder result = new StringBuilder();
            int ch;
            for (int i = 0; i < s.Length; i += Character.CharCount(ch))
            {
                ch = Character.CodePointAt(s, i);
                if (ch > 0xfffff)
                {
                    result.Append("\\U00");
                    result.Append(Hex(ch));
                }
                else if (ch > 0xffff)
                {
                    result.Append("\\U000");
                    result.Append(Hex(ch));
                }
                else if (ch < 0x20 || 0x7e < ch)
                {
                    result.Append("\\u");
                    result.Append(Hex(ch));
                }
                else
                {
                    result.Append((char)ch);
                }

            }
            return result.ToString();
        }

        private static GregorianCalendar cal;

        /**
         * Return a Date given a year, month, and day of month. This is similar to
         * new Date(y-1900, m, d). It uses the default time zone at the time this
         * method is first called.
         *
         * @param year
         *            use 2000 for 2000, unlike new Date()
         * @param month
         *            use Calendar.JANUARY etc.
         * @param dom
         *            day of month, 1-based
         * @return a Date object for the given y/m/d
         */
        protected static DateTime GetDate(int year, int month,
                int dom)
        {
            lock (syncLock)
            {
                if (cal == null)
                {
                    cal = new GregorianCalendar();
                }
                //cal.clear();
                //cal.set(year, month, dom);
                return cal.ToDateTime(year, month, dom, 0, 0, 0, 0);
            }
        }

        private class TestParams
        {

            internal int inclusion;
            internal int seed;
            internal int loggingLevel;

            internal string policyFileName;
            //internal SecurityManager testSecurityManager;
            //internal SecurityManager originalSecurityManager;

            internal IDictionary<string, List<string>> knownIssues;

            internal IDictionary<string, string> props;



            private TestParams()
            {
            }

            internal static TestParams Create()
            {
                TestParams @params = new TestParams();
                //Properties props = System.GetProperties();
                IDictionary<string, string> props = new Dictionary<string, string>();
                @params.ParseProperties(props);
                return @params;
            }

            private void ParseProperties(IDictionary<string, string> props)
            {
                this.props = props;

                inclusion = GetIntProperty(EXHAUSTIVENESS, DEFAULT_EXHAUSTIVENESS, MAX_EXHAUSTIVENESS);
                seed = GetIntProperty(SEED, unchecked((int)DateTime.Now.Ticks));
                loggingLevel = GetIntProperty(LOGGING_LEVEL, DEFAULT_LOGGING_LEVEL, MAX_LOGGING_LEVEL);

                //policyFileName = GetProperty(SECURITY_POLICY);
                //if (policyFileName != null)
                //{
                //    string originalPolicyFileName = System.GetProperty("java.security.policy");
                //    originalSecurityManager = System.GetSecurityManager();
                //    System.setProperty("java.security.policy", policyFileName);
                //    Policy.GetPolicy().refresh();
                //    testSecurityManager = new SecurityManager();
                //    System.setProperty("java.security.policy", originalPolicyFileName == null ? "" : originalPolicyFileName);
                //}
            }

            public string GetProperty(string key)
            {
                string val = null;
                if (key != null && key.Length > 0)
                {
                    //val = props.GetProperty(key);
                    props.TryGetValue(key, out val);
                }
                return val;
            }

            public bool GetBooleanProperty(string key)
            {
                return GetBooleanProperty(key, false);
            }

            public bool GetBooleanProperty(string key, bool defVal)
            {
                string s = GetProperty(key);
                if (s == null)
                {
                    return defVal;
                }
                if (s.Equals("yes", StringComparison.OrdinalIgnoreCase) || s.Equals("true", StringComparison.OrdinalIgnoreCase) || s.Equals("1"))
                {
                    return true;
                }
                return false;
            }

            public int GetIntProperty(string key, int defVal)
            {
                return GetIntProperty(key, defVal, -1);
            }

            public int GetIntProperty(string key, int defVal, int maxVal)
            {
                string s = GetProperty(key);
                if (s == null)
                {
                    return defVal;
                }
                return (maxVal == -1) ? int.Parse(s, CultureInfo.InvariantCulture) : Math.Max(int.Parse(s, CultureInfo.InvariantCulture), maxVal);
            }

            public long GetLongProperty(string key, long defVal)
            {
                string s = GetProperty(key);
                if (s == null)
                {
                    return defVal;
                }
                return long.Parse(s, CultureInfo.InvariantCulture);
            }

            public int GetInclusion()
            {
                return inclusion;
            }

            public int GetSeed()
            {
                return seed;
            }

            public int GetLoggingLevel()
            {
                return loggingLevel;
            }
        }

        /**
         * Check the given array to see that all the strings in the expected array
         * are present.
         *
         * @param msg
         *            string message, for log output
         * @param array
         *            array of strings to check
         * @param expected
         *            array of strings we expect to see, or null
         * @return the length of 'array', or -1 on error
         */
        protected static int CheckArray(string msg, string[] array, string[] expected)
        {
            int explen = (expected != null) ? expected.Length : 0;
            if (!(explen >= 0 && explen < 31))
            { // [sic] 31 not 32
                Errln("Internal error");
                return -1;
            }
            int i = 0;
            StringBuffer buf = new StringBuffer();
            int seenMask = 0;
            for (; i < array.Length; ++i)
            {
                string s = array[i];
                if (i != 0)
                    buf.Append(", ");
                buf.Append(s);
                // check expected list
                for (int j = 0, bit = 1; j < explen; ++j, bit <<= 1)
                {
                    if ((seenMask & bit) == 0)
                    {
                        if (s.Equals(expected[j]))
                        {
                            seenMask |= bit;
                            Logln("Ok: \"" + s + "\" seen");
                        }
                    }
                }
            }
            Logln(msg + " = [" + buf + "] (" + i + ")");
            // did we see all expected strings?
            if (((1 << explen) - 1) != seenMask)
            {
                for (int j = 0, bit = 1; j < expected.Length; ++j, bit <<= 1)
                {
                    if ((seenMask & bit) == 0)
                    {
                        Errln("\"" + expected[j] + "\" not seen");
                    }
                }
            }
            return array.Length;
        }

        /**
         * Check the given array to see that all the locales in the expected array
         * are present.
         *
         * @param msg
         *            string message, for log output
         * @param array
         *            array of locales to check
         * @param expected
         *            array of locales names we expect to see, or null
         * @return the length of 'array'
         */
        protected static int CheckArray(string msg, CultureInfo[] array, string[] expected)
        {
            string[] strs = new string[array.Length];
            for (int i = 0; i < array.Length; ++i)
            {
                strs[i] = array[i].ToString();
            }
            return CheckArray(msg, strs, expected);
        }

        /**
         * Check the given array to see that all the locales in the expected array
         * are present.
         *
         * @param msg
         *            string message, for log output
         * @param array
         *            array of locales to check
         * @param expected
         *            array of locales names we expect to see, or null
         * @return the length of 'array'
         */
        protected static int CheckArray(string msg, ULocale[] array, string[] expected)
        {
            string[] strs = new string[array.Length];
            for (int i = 0; i < array.Length; ++i)
            {
                strs[i] = array[i].ToString();
            }
            return CheckArray(msg, strs, expected);
        }

        // JUnit-like assertions.

        protected static bool assertTrue(string message, bool condition)
        {
            return handleAssert(condition, message, "true", null);
        }

        protected static bool assertFalse(string message, bool condition)
        {
            return handleAssert(!condition, message, "false", null);
        }

        protected static bool assertEquals(string message, bool expected,
                bool actual)
        {
            return handleAssert(expected == actual, message, 
                    Convert.ToString(expected, CultureInfo.InvariantCulture), Convert.ToString(actual, CultureInfo.InvariantCulture));
        }

        protected static bool assertEquals(string message, long expected, long actual)
        {
            return handleAssert(expected == actual, message, 
                    Convert.ToString(expected, CultureInfo.InvariantCulture), Convert.ToString(actual, CultureInfo.InvariantCulture));
        }

        // do NaN and range calculations to precision of float, don't rely on
        // promotion to double
        protected static bool assertEquals(string message, float expected,
                float actual, double error)
        {
            bool result = float.IsInfinity(expected)
                    ? expected == actual
                    : !(Math.Abs(expected - actual) > error); // handles NaN
            return handleAssert(result, message, Convert.ToString(expected, CultureInfo.InvariantCulture)
                    + (error == 0 ? "" : " (within " + error + ")"), 
                    Convert.ToString(actual, CultureInfo.InvariantCulture));
        }

        // ICU4N specific - The overload that accepts object will not work for
        // floating point numbers, so we need one that accepts no error (delta) value
        protected static bool assertEquals(string message, float expected,
                float actual)
        {
            return assertEquals(message, expected, actual, 0);
        }

        protected static bool assertEquals(string message, double expected,
                double actual, double error)
        {
            bool result = double.IsInfinity(expected)
                    ? expected == actual
                    : !(Math.Abs(expected - actual) > error); // handles NaN
            return handleAssert(result, message, Convert.ToString(expected, CultureInfo.InvariantCulture)
                    + (error == 0 ? "" : " (within " + error + ")"), 
                    Convert.ToString(actual, CultureInfo.InvariantCulture));
        }

        // ICU4N specific - The overload that accepts object will not work for
        // floating point numbers, so we need one that accepts no error (delta) value
        protected static bool assertEquals(string message, double expected,
                double actual)
        {
            return assertEquals(message, expected, actual, 0);
        }

        protected static bool assertEquals<T>(string message, T[] expected, T[] actual)
        {
            // Use toString on a List to get useful, readable messages
            string expectedString = expected == null ? "null" : Arrays.ToString(expected);
            string actualString = actual == null ? "null" : Arrays.ToString(actual);
            return assertEquals(message, expectedString, actualString);
        }

        // ICU4N specific overload for optimizing ICollection<T> comparisons
        protected static bool assertEquals<T>(string message, ICollection<T> expected, ICollection<T> actual)
        {
            bool result = expected == null ? actual == null : CollectionUtil.Equals(expected, actual);
            return handleAssert(result, message, stringFor(expected),
                    stringFor(actual));
        }

        // ICU4N specific overload for handling ICharSequence
        internal static bool assertEquals(string message, ICharSequence expected, object actual)
        {
            bool result = expected == null ? actual == null : expected.Equals(actual);
            return handleAssert(result, message, stringFor(expected),
                    stringFor(actual));
        }

        // ICU4N specific overload for handling ICharSequence
        internal static bool assertEquals(string message, object expected, ICharSequence actual)
        {
            bool result = expected == null ? actual == null : actual.Equals(expected);
            return handleAssert(result, message, stringFor(expected),
                    stringFor(actual));
        }

        protected static bool assertEquals(string message, object expected, object actual)
        {
            if (expected is ICharSequence)
                return assertEquals(message, (ICharSequence)expected, actual);
            if (actual is ICharSequence)
                return assertEquals(message, expected, (ICharSequence)actual);

            bool result = expected == null ? actual == null : CollectionUtil.Equals(expected, actual);
            return handleAssert(result, message, stringFor(expected),
                    stringFor(actual));
        }

        // ICU4N specific overload for optimizing ICollection<T> comparisons
        protected static bool assertNotEquals<T>(string message, ICollection<T> expected,
            ICollection<T> actual)
        {
            bool result = !(expected == null ? actual == null : CollectionUtil.Equals(expected, actual));
            return handleAssert(result, message, stringFor(expected),
                    stringFor(actual), "not equal to", true);
        }

        // ICU4N specific overload for handling ICharSequence
        internal static bool assertNotEquals(string message, ICharSequence expected,
            object actual)
        {
            bool result = !(expected == null ? actual == null : expected
                    .Equals(actual));
            return handleAssert(result, message, stringFor(expected),
                    stringFor(actual), "not equal to", true);
        }

        // ICU4N specific overload for handling ICharSequence
        internal static bool assertNotEquals(string message, object expected,
            ICharSequence actual)
        {
            bool result = !(expected == null ? actual == null : actual.Equals(expected));
            return handleAssert(result, message, stringFor(expected),
                    stringFor(actual), "not equal to", true);
        }

        protected static bool assertNotEquals(string message, object expected,
                object actual)
        {
            bool result = !(expected == null ? actual == null : CollectionUtil.Equals(expected, actual));
            return handleAssert(result, message, stringFor(expected),
                    stringFor(actual), "not equal to", true);
        }

        protected bool assertSame(string message, object expected, object actual)
        {
            return handleAssert(expected == actual, message, stringFor(expected),
                    stringFor(actual), "==", false);
        }

        protected static bool assertNotSame(string message, object expected,
                object actual)
        {
            return handleAssert(expected != actual, message, stringFor(expected),
                    stringFor(actual), "!=", true);
        }

        protected static bool assertNull(string message, object actual)
        {
            return handleAssert(actual == null, message, null, stringFor(actual));
        }

        protected static bool assertNotNull(string message, object actual)
        {
            return handleAssert(actual != null, message, null, stringFor(actual),
                    "!=", true);
        }

        protected static void fail()
        {
            fail("");
        }

        protected static void fail(string message)
        {
            if (message == null)
            {
                message = "";
            }
            if (!message.Equals(""))
            {
                message = ": " + message;
            }
            Errln(SourceLocation() + message);
        }

        private static bool handleAssert(bool result, string message,
                string expected, string actual)
        {
            return handleAssert(result, message, expected, actual, null, false);
        }

        public static bool handleAssert(bool result, string message,
                object expected, object actual, string relation, bool flip)
        {
            if (!result || IsVerbose())
            {
                if (message == null)
                {
                    message = "";
                }
                if (!message.Equals(""))
                {
                    message = ": " + message;
                }
                relation = relation == null ? ", got " : " " + relation + " ";
                if (result)
                {
                    Logln("OK " + message + ": "
                            + (flip ? expected + relation + actual : expected));
                }
                else
                {
                    // assert must assume errors are true errors and not just warnings
                    // so cannot warnln here
                    Errln(message
                            + ": expected"
                            + (flip ? relation + expected : " " + expected
                                    + (actual != null ? relation + actual : "")));
                }
            }
            return result;
        }

        private static string stringFor(object obj)
        {
            if (obj == null)
            {
                return "null";
            }
            if (obj is string || obj is StringCharSequence)
            {
                return "\"" + obj + '"';
            }
            // ICU4N: Handle deep collection strings for nested collection types
            return obj.GetType().Name + "<" + CollectionUtil.ToString(obj) + ">";
            //return obj.GetType().Name + "<" + obj + ">";
        }

        private static readonly Regex METHOD_NAME_REGEX = new Regex(@"at\s+(?<fullyQualifiedMethod>.*\.(?<method>[\w`]+))\(", RegexOptions.Compiled);
        private static readonly Regex FILE_NAME_REGEX = new Regex(@"(?<=in)\s+(?<filename>.*)", RegexOptions.Compiled);

        // Return the source code location of the caller located callDepth frames up the stack.
        protected static string SourceLocation()
        {
#if !FEATURE_STACKTRACE
            var sourceLines =
                Environment.StackTrace
                    .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Reverse().ToList();

            using (var iter = sourceLines.GetEnumerator())
            {
                while (iter.MoveNext())
                {
                    var line = iter.Current;

                    if (line.Contains("System.Environment.GetStackTrace") || line.Contains("get_StackTrace"))
                        continue;

                    if (line.TrimStart().StartsWith("at NUnit.") || line.TrimStart().StartsWith("at Microsoft.VisualStudio") || line.Contains("Invoke"))
                        continue;

                    if (line.Contains("TestFmwk.cs") || line.Contains("AbstractTestLog.cs"))
                        continue;

                    var match = METHOD_NAME_REGEX.Match(line);

                    if (match.Success)
                    {
                        var methodName = match.Groups["method"].Value;
                        if (methodName.StartsWith("Test", StringComparison.Ordinal) || methodName.StartsWith("test", StringComparison.Ordinal) || methodName.Equals("Main"))
                        {
                            var matchFileName = FILE_NAME_REGEX.Match(line);
                            if (matchFileName.Success)
                            {
                                return matchFileName.Groups["filename"].Value;
                            }

                            // If the regex fails, just return the line as is.
                            return line;
                        }
                    }
                }
            }
#else
            // Walk up the stack to the first call site outside this file
            StackTrace trace = new StackTrace(true);
            foreach (var frame in trace.GetFrames())
            {
                string source = frame.GetFileName();
                if (source != null && !source.EndsWith("TestFmwk.cs", StringComparison.Ordinal) && !source.EndsWith("AbstractTestLog.cs", StringComparison.Ordinal))
                {
                    string methodName = frame.GetMethod().Name;
                    if (methodName != null &&
                           (methodName.StartsWith("Test", StringComparison.Ordinal) || methodName.StartsWith("test", StringComparison.Ordinal) || methodName.Equals("Main")))
                    {
                        return "(" + source + ":" + frame.GetFileLineNumber() + ") ";
                    }
                }
            }
#endif
            throw new Exception();
        }

        protected static bool CheckDefaultPrivateConstructor(string fullyQualifiedClassName)
        {
            return CheckDefaultPrivateConstructor(Type.GetType(fullyQualifiedClassName));
        }

        protected static bool CheckDefaultPrivateConstructor(Type classToBeTested)
        {
            // ICU4N TODO: Finish
            throw new NotImplementedException();
            //ConstructorInfo constructor = classToBeTested.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly, null, null, null);

            //// Check that the constructor is private.
            ////bool isPrivate = Modifier.isPrivate(constructor.GetModifiers());
            //bool isPrivate = constructor.IsPrivate;

            //// Call the constructor for coverage.
            ////constructor.setAccessible(true);
            //constructor.Invoke(new object[0]);

            //if (!isPrivate)
            //{
            //    Errln("Default private constructor for class: " + classToBeTested.Name + " is not private.");
            //}
            //return isPrivate;
        }

        /**
         * Tests the toString method on a private or hard-to-reach class.  Assumes constructor of the class does not
         * take any arguments.
         * @param fullyQualifiedClassName
         * @return The output of the toString method.
         * @throws Exception
         */
        protected static string InvokeToString(string fullyQualifiedClassName)
        {
            return InvokeToString(fullyQualifiedClassName, new Type[] { }, new Object[] { });
        }

        /**
         * Tests the toString method on a private or hard-to-reach class.  Assumes constructor of the class does not
         * take any arguments.
         * @param classToBeTested
         * @return The output of the toString method.
         * @throws Exception
         */
        protected static string InvokeToString(Type classToBeTested)
        {
            return InvokeToString(classToBeTested, new Type[] { }, new Object[] { });
        }

        /**
         * Tests the toString method on a private or hard-to-reach class.  Allows you to specify the argument types for
         * the constructor.
         * @param fullyQualifiedClassName
         * @return The output of the toString method.
         * @throws Exception
         */
        protected static string InvokeToString(string fullyQualifiedClassName,
                Type[] constructorParamTypes, Object[] constructorParams)
        {
            return InvokeToString(Type.GetType(fullyQualifiedClassName), constructorParamTypes, constructorParams);
        }

        /**
         * Tests the toString method on a private or hard-to-reach class.  Allows you to specify the argument types for
         * the constructor.
         * @param classToBeTested
         * @return The output of the toString method.
         * @throws Exception
         */
        protected static string InvokeToString(Type classToBeTested,
                Type[] constructorParamTypes, object[] constructorParams)
        {
            // ICU4N TODO: Finish
            throw new NotImplementedException();
            //ConstructorInfo constructor = classToBeTested.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly, null, constructorParamTypes, null);
            ////constructor.setAccessible(true);
            //object obj = constructor.Invoke(constructorParams);
            //MethodInfo toStringMethod = classToBeTested.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            ////toStringMethod.setAccessible(true);
            //return (string)toStringMethod.Invoke(obj, new object[0]);
        }


        // End JUnit-like assertions

        // TODO (sgill): added to keep errors away
        /* (non-Javadoc)
         * @see com.ibm.icu.dev.test.TestLog#msg(java.lang.string, int, bool, bool)
         */
        //@Override
        protected static void msg(string message, int level, bool incCount, bool newln)
        {
            if (level == TestLog.WARN || level == TestLog.ERR)
            {
                Assert.Fail(message);
            }
            // TODO(stuartg): turned off - causing OOM running under ant
            //        while (level > 0) {
            //            System.out.print(" ");
            //            level--;
            //        }
            //        System.out.print(message);
            //        if (newln) {
            //            System.out.println();
            //        }
        }
    }
}
