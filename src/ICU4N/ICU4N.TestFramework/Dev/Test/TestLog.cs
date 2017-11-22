using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Dev.Test
{
    public abstract class TestLog
    {

        //    /**
        //     * Adds given string to the log if we are in verbose mode.
        //     */
        //    void log(string message);
        //
        //    void logln(string message);
        //
        //    /**
        //     * Report an error
        //     */
        //    void err(string message);
        //
        //    void errln(string message);
        //
        //    /**
        //     * Warn about missing tests or data.
        //     */
        //    void warn(string message);
        //    
        //    void warnln(string message);


        public static readonly int LOG = 0;
        public static readonly int WARN = 1;
        public static readonly int ERR = 2;

        //public static void msg(string message, int level, boolean incCount, boolean newln);
    }

}
