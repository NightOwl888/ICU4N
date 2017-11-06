using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ICU4N.Support
{
    public static class Time
    {
        public static long NanoTime()
        {
            return (Stopwatch.GetTimestamp() / Stopwatch.Frequency) * 1000000000;
        }

        public static long CurrentTimeMilliseconds()
        {
            return (Stopwatch.GetTimestamp() / Stopwatch.Frequency) * 1000;
        }
    }
}
