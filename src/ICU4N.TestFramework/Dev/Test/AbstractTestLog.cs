using ICU4N.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ICU4N.Dev.Test
{
    public abstract class AbstractTestLog : TestLog
    {
        /**
         * Returns true if ICU_Version &lt; major.minor.
         */
        static public bool IsICUVersionBefore(int major, int minor)
        {
            return IsICUVersionBefore(major, minor, 0);
        }

        /**
         * Returns true if ICU_Version &lt; major.minor.milli.
         */
        static public bool IsICUVersionBefore(int major, int minor, int milli)
        {
            return VersionInfo.IcuVersion.CompareTo(VersionInfo.GetInstance(major, minor, milli)) < 0;
        }

        /**
         * Returns true if ICU_Version >= major.minor.
         */
        static public bool isICUVersionAtLeast(int major, int minor)
        {
            return isICUVersionAtLeast(major, minor, 0);
        }

        /**
         * Returns true if ICU_Version >= major.minor.milli.
         */
        static public bool isICUVersionAtLeast(int major, int minor, int milli)
        {
            return !IsICUVersionBefore(major, minor, milli);
        }

        /**
         * Add a message.
         */
        public static void Log(string message)
        {
            // TODO(stuartg): turned off - causing OOM running under ant
            // Probably temporary - must decide what to do with these
            //System.out.print(message);
            //msg(message, LOG, true, false);
        }

        /**
         * Add a message and newline.
         */
        public static void Logln(string message)
        {
            // TODO(stuartg): turned off - causing OOM running under ant
            // Probably temporary - must decide what to do with these
            //System.out.println(message);
            //msg(message, LOG, true, true);
        }

        /**
         * Report an error.
         */
        public static void Err(string message)
        {
            Assert.Fail(message);
            //msg(message, ERR, true, false);
        }

        /**
         * Report an error and newline.
         */
        public static void Errln(string message)
        {
            Assert.Fail(message);
            //msg(message, ERR, true, true);
        }

        /**
         * Report a warning (generally missing tests or data).
         */
        public static void Warn(string message)
        {
            Assert.Fail(message);
            // TODO(stuartg): turned off - causing OOM running under ant
            //System.out.print(message);
            //msg(message, WARN, true, false);
        }

        /**
         * Report a warning (generally missing tests or data) and newline.
         */
        public static void Warnln(string message)
        {
            Assert.Fail(message);
            // TODO(stuartg): turned off - causing OOM running under ant
            //System.out.println(message);
            //msg(message, WARN, true, true);
        }

        /**
         * Vector for logging.  Callers can force the logging system to
         * not increment the error or warning level by passing false for incCount.
         *
         * @param message the message to output.
         * @param level the message level, either LOG, WARN, or ERR.
         * @param incCount if true, increments the warning or error count
         * @param newln if true, forces a newline after the message
         */
        //public abstract void msg(string message, int level, boolean incCount, boolean newln);

        public bool IsDateAtLeast(int year, int month, int day)
        {
            DateTime now = new DateTime();
            Calendar c = new GregorianCalendar();
            DateTime dt = c.ToDateTime(year, month, day, 0, 0, 0, 0);
            if (now.CompareTo(dt) >= 0)
            {
                return true;
            }
            return false;
        }
    }

}
