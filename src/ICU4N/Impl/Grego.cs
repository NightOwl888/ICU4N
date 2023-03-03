using System.Globalization;

namespace ICU4N.Impl
{
    /// <summary>
    /// A utility class providing proleptic Gregorian calendar functions
    /// used by time zone and calendar code.
    /// <para/>
    /// Note:  Unlike GregorianCalendar, all computations performed by this
    /// class occur in the pure proleptic GregorianCalendar.
    /// </summary>
    public static class Grego
    {
        // Max/min milliseconds 
        public const long MinMilliseconds = -184303902528000000L;
        public const long MaxMilliseconds = 183882168921600000L;

        public const int MillisecondsPerSecond = 1000;
        public const int MillisecondsPerMinute = 60 * MillisecondsPerSecond;
        public const int MillisecondsPerHour = 60 * MillisecondsPerMinute;
        public const int MillisecondsPerDay = 24 * MillisecondsPerHour;

        //  January 1, 1 CE Gregorian
        private const int JULIAN_1_CE = 1721426;

        //  January 1, 1970 CE Gregorian
        private const int JULIAN_1970_CE = 2440588;

        private static readonly int[] MONTH_LENGTH = new int[] {
            31,28,31,30,31,30,31,31,30,31,30,31,
            31,29,31,30,31,30,31,31,30,31,30,31
        };

        private static readonly int[] DAYS_BEFORE = new int[] {
            0,31,59,90,120,151,181,212,243,273,304,334,
            0,31,60,91,121,152,182,213,244,274,305,335
        };

        /// <summary>
        /// Return <c>true</c> if the given year is a leap year.
        /// </summary>
        /// <param name="year">Gregorian year, with 0 == 1 BCE, -1 == 2 BCE, etc.</param>
        /// <returns><c>true</c> if the year is a leap year.</returns>
        public static bool IsLeapYear(int year)
        {
            // year&0x3 == year%4
            return ((year & 0x3) == 0) && ((year % 100 != 0) || (year % 400 == 0));
        }

        /// <summary>
        /// Return the number of days in the given month.
        /// </summary>
        /// <param name="year">Gregorian year, with 0 == 1 BCE, -1 == 2 BCE, etc.</param>
        /// <param name="month">0-based month, with 0==Jan</param>
        /// <returns>The number of days in the given month.</returns>
        public static int MonthLength(int year, int month) // ICU4N TODO: Make the month 1-based so this isn't so confusing
        {
            return MONTH_LENGTH[month + (IsLeapYear(year) ? 12 : 0)];
        }

        /// <summary>
        /// Return the length of a previous month of the Gregorian calendar.
        /// </summary>
        /// <param name="year">Gregorian year, with 0 == 1 BCE, -1 == 2 BCE, etc.</param>
        /// <param name="month">0-based month, with 0==Jan.</param>
        /// <returns>The number of days in the month previous to the given month.</returns>
        public static int PreviousMonthLength(int year, int month)
        {
            return (month > 0) ? MonthLength(year, month - 1) : 31;
        }

        /// <summary>
        /// Convert a year, month, and day-of-month, given in the proleptic
        /// Gregorian calendar, to 1970 epoch days.
        /// </summary>
        /// <param name="year">Gregorian year, with 0 == 1 BCE, -1 == 2 BCE, etc.</param>
        /// <param name="month">0-based month, with 0==Jan</param>
        /// <param name="dayOfMonth">1-based day of month</param>
        /// <returns>The day number, with day 0 == Jan 1 1970.</returns>
        public static long FieldsToDay(int year, int month, int dayOfMonth) // ICU4N TODO: Make the month 1-based so this isn't so confusing
        {
            int y = year - 1;
            long julian =
                365 * y + FloorDivide(y, 4) + (JULIAN_1_CE - 3) +    // Julian cal
                FloorDivide(y, 400) - FloorDivide(y, 100) + 2 +   // => Gregorian cal
                DAYS_BEFORE[month + (IsLeapYear(year) ? 12 : 0)] + dayOfMonth; // => month/dom
            return julian - JULIAN_1970_CE; // JD => epoch day
        }

        /// <summary>
        /// Return the day of week on the 1970-epoch day.
        /// </summary>
        /// <param name="day">The 1970-epoch day (integral value).</param>
        /// <returns>The day of week.</returns>
        public static int DayOfWeek(long day)
        {
            long[] remainder = new long[1];
            FloorDivide(day + 5 /* Calendar.THURSDAY */, 7, remainder);
            int dayOfWeek = (int)remainder[0];
            dayOfWeek = (dayOfWeek == 0) ? 7 : dayOfWeek;
            return dayOfWeek;
        }

        public static int[] DayToFields(long day, int[] fields)
        {
            if (fields == null || fields.Length < 5)
            {
                fields = new int[5];
            }
            // Convert from 1970 CE epoch to 1 CE epoch (Gregorian calendar)
            day += JULIAN_1970_CE - JULIAN_1_CE;

            long[] rem = new long[1];
            long n400 = FloorDivide(day, 146097, rem);
            long n100 = FloorDivide(rem[0], 36524, rem);
            long n4 = FloorDivide(rem[0], 1461, rem);
            long n1 = FloorDivide(rem[0], 365, rem);

            int year = (int)(400 * n400 + 100 * n100 + 4 * n4 + n1);
            int dayOfYear = (int)rem[0];
            if (n100 == 4 || n1 == 4)
            {
                dayOfYear = 365;    // Dec 31 at end of 4- or 400-yr cycle
            }
            else
            {
                ++year;
            }

            bool isLeap = IsLeapYear(year);
            int correction = 0;
            int march1 = isLeap ? 60 : 59;  // zero-based DOY for March 1
            if (dayOfYear >= march1)
            {
                correction = isLeap ? 1 : 2;
            }
            int month = (12 * (dayOfYear + correction) + 6) / 367;  // zero-based month
            int dayOfMonth = dayOfYear - DAYS_BEFORE[isLeap ? month + 12 : month] + 1; // one-based DOM
            int dayOfWeek = (int)((day + 2) % 7);  // day 0 is Monday(2)
            if (dayOfWeek < 1 /* Sunday */)
            {
                dayOfWeek += 7;
            }
            dayOfYear++; // 1-based day of year

            fields[0] = year;
            fields[1] = month;
            fields[2] = dayOfMonth;
            fields[3] = dayOfWeek;
            fields[4] = dayOfYear;

            return fields;
        }

        /// <summary>
        /// Convert long time to date/time fields.
        /// <para/>
        /// result[0] : year
        /// result[1] : month
        /// result[2] : dayOfMonth
        /// result[3] : dayOfWeek
        /// result[4] : dayOfYear
        /// result[5] : millisecond in day
        /// </summary>
        public static int[] TimeToFields(long time, int[] fields)
        {
            if (fields == null || fields.Length < 6)
            {
                fields = new int[6];
            }
            long[] remainder = new long[1];
            long day = FloorDivide(time, 24 * 60 * 60 * 1000 /* milliseconds per day */, remainder);
            DayToFields(day, fields);
            fields[5] = (int)remainder[0];
            return fields;
        }

        public static long FloorDivide(long numerator, long denominator)
        {
            // We do this computation in order to handle
            // a numerator of Long.MIN_VALUE correctly
            return (numerator >= 0) ?
                numerator / denominator :
                ((numerator + 1) / denominator) - 1;
        }

        private static long FloorDivide(long numerator, long denominator, long[] remainder)
        {
            if (numerator >= 0)
            {
                remainder[0] = numerator % denominator;
                return numerator / denominator;
            }
            long quotient = ((numerator + 1) / denominator) - 1;
            remainder[0] = numerator - (quotient * denominator);
            return quotient;
        }

        /// <summary>
        /// Returns the ordinal number for the specified day of week in the month.
        /// The valid return value is 1, 2, 3, 4 or -1.
        /// </summary>
        public static int GetDayOfWeekInMonth(int year, int month, int dayOfMonth)
        {
            int weekInMonth = (dayOfMonth + 6) / 7;
            if (weekInMonth == 4)
            {
                if (dayOfMonth + 7 > MonthLength(year, month))
                {
                    weekInMonth = -1;
                }
            }
            else if (weekInMonth == 5)
            {
                weekInMonth = -1;
            }
            return weekInMonth;
        }

        /// <summary>
        /// Convenient method for formatting time to ISO 8601 style
        /// date string.
        /// </summary>
        /// <param name="time">Long time.</param>
        /// <returns>ISO-8601 date string.</returns>
        public static string TimeToString(long time)
        {
            int[] fields = TimeToFields(time, null);
            int millis = fields[5];
            int hour = millis / MillisecondsPerHour;
            millis = millis % MillisecondsPerHour;
            int min = millis / MillisecondsPerMinute;
            millis = millis % MillisecondsPerMinute;
            int sec = millis / MillisecondsPerSecond;
            millis = millis % MillisecondsPerSecond;

            return string.Format(CultureInfo.InvariantCulture, "{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}.{6:000}Z",
                    fields[0], fields[1] + 1, fields[2], hour, min, sec, millis);
        }
    }
}
