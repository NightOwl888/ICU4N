using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Globalization;
using Double = J2N.Numerics.Double;
using Long = J2N.Numerics.Int64;
using Number = J2N.Numerics.Number;

namespace ICU4N.Dev.Test.Format
{
    /// <summary>
    /// Performs round-trip tests for NumberFormat
    /// </summary>
    public class NumberFormatRoundTripTest : TestFmwk
    {
        public double MAX_ERROR = 1e-14;
        public double max_numeric_error = 0.0;
        public double min_numeric_error = 1.0;
        public bool verbose = false;
        public bool STRING_COMPARE = false;
        public bool EXACT_NUMERIC_COMPARE = false;
        public bool DEBUG = false;
        public bool quick = true;

        [Test]
        public void TestNumberFormatRoundTrip()
        {

            NumberFormat fmt = null;

            Logln("Default Locale");

            Logln("Default Number format");
            fmt = NumberFormat.GetInstance();
            _test(fmt);

            Logln("Currency Format");
            fmt = NumberFormat.GetCurrencyInstance();
            _test(fmt);

            Logln("Percent Format");
            fmt = NumberFormat.GetPercentInstance();
            _test(fmt);


            int locCount = 0;
            CultureInfo[] loc = NumberFormat.GetCultures(Globalization.UCultureTypes.AllCultures);
            if (quick)
            {
                if (locCount > 5)
                    locCount = 5;
                Logln("Quick mode: only _testing first 5 Locales");
            }
            for (int i = 0; i < locCount; ++i)
            {
                Logln(loc[i].DisplayName);

                fmt = NumberFormat.GetInstance(loc[i]);
                _test(fmt);

                fmt = NumberFormat.GetCurrencyInstance(loc[i]);
                _test(fmt);

                fmt = NumberFormat.GetPercentInstance(loc[i]);
                _test(fmt);
            }

            Logln("Numeric error " + min_numeric_error + " to " + max_numeric_error);
        }

        /**
         * Return a random value from -range..+range.
         */
        private Random random;
        public double randomDouble(double range)
        {
            if (random == null)
            {
                random = CreateRandom(); // use test framework's random seed
            }
            return random.NextDouble() * range;
        }

        private void _test(NumberFormat fmt)
        {

            _test(fmt, double.NaN);
            _test(fmt, double.PositiveInfinity);
            _test(fmt, double.NegativeInfinity);

            _test(fmt, 500);
            _test(fmt, 0);
            _test(fmt, -0);
            _test(fmt, 0.0);
            double negZero = 0.0;
            negZero /= -1.0;
            _test(fmt, negZero);
            _test(fmt, 9223372036854775808.0d);
            _test(fmt, -9223372036854775809.0d);
            //_test(fmt, 6.936065876100493E74d);

            //    _test(fmt, 6.212122845281909E48d);
            for (int i = 0; i < 10; ++i)
            {

                _test(fmt, randomDouble(1));

                _test(fmt, randomDouble(10000));

                _test(fmt, Math.Floor((randomDouble(10000))));

                _test(fmt, randomDouble(1e50));

                _test(fmt, randomDouble(1e-50));

                _test(fmt, randomDouble(1e100));

                _test(fmt, randomDouble(1e75));

                _test(fmt, randomDouble(1e308) / ((DecimalFormat)fmt).Multiplier);

                _test(fmt, randomDouble(1e75) / ((DecimalFormat)fmt).Multiplier);

                _test(fmt, randomDouble(1e65) / ((DecimalFormat)fmt).Multiplier);

                _test(fmt, randomDouble(1e-292));

                _test(fmt, randomDouble(1e-78));

                _test(fmt, randomDouble(1e-323));

                _test(fmt, randomDouble(1e-100));

                _test(fmt, randomDouble(1e-78));
            }
        }

        private void _test(NumberFormat fmt, double value)
        {
            _test(fmt, (Number)Double.GetInstance(value));
        }

        private void _test(NumberFormat fmt, long value)
        {
            _test(fmt, (Number)Long.GetInstance(value));
        }

        private void _test(NumberFormat fmt, Number value)
        {
            Logln("test data = " + value);
            fmt.MaximumFractionDigits=(999);
            String s, s2;
            if (value.GetType().FullName.Equals("J2N.Numerics.Double", StringComparison.OrdinalIgnoreCase))
                s = fmt.Format(value.ToDouble());
            else
                s = fmt.Format(value.ToInt64());

            Number n = Double.GetInstance(0);
            bool show = verbose;
            if (DEBUG)
                Logln(
                /*value.getString(temp) +*/ " F> " + s);
            try
            {
                n = fmt.Parse(s);
            }
            catch (FormatException e)
            {
                Console.Out.WriteLine(e);
            }

            if (DEBUG)
                Logln(s + " P> " /*+ n.getString(temp)*/);

            if (value.GetType().FullName.Equals("J2N.Numerics.Double", StringComparison.OrdinalIgnoreCase))
                s2 = fmt.Format(n.ToDouble());
            else
                s2 = fmt.Format(n.ToInt64());

            if (DEBUG)
                Logln(/*n.getString(temp) +*/ " F> " + s2);

            if (STRING_COMPARE)
            {
                if (!s.Equals(s2))
                {
                    Errln("*** STRING ERROR \"" + s + "\" != \"" + s2 + "\"");
                    show = true;
                }
            }

            if (EXACT_NUMERIC_COMPARE)
            {
                if (value != n)
                {
                    Errln("*** NUMERIC ERROR");
                    show = true;
                }
            }
            else
            {
                // Compute proportional error
                double error = proportionalError(value, n);

                if (error > MAX_ERROR)
                {
                    Errln("*** NUMERIC ERROR " + error);
                    show = true;
                }

                if (error > max_numeric_error)
                    max_numeric_error = error;
                if (error < min_numeric_error)
                    min_numeric_error = error;
            }

            if (show)
                Logln(
                /*value.getString(temp) +*/ value.GetType().FullName + " F> " + s + " P> " +
                /*n.getString(temp) +*/ n.GetType().FullName + " F> " + s2);

        }

        private double proportionalError(Number a, Number b)
        {
            double aa, bb;

            if (a.GetType().FullName.Equals("J2N.Numerics.Double", StringComparison.OrdinalIgnoreCase))
                aa = a.ToDouble();
            else
                aa = a.ToInt64();

            if (a.GetType().FullName.Equals("J2N.Numerics.Double", StringComparison.OrdinalIgnoreCase))
                bb = b.ToDouble();
            else
                bb = b.ToInt64();

            double error = aa - bb;
            if (aa != 0 && bb != 0)
                error /= aa;

            return Math.Abs(error);
        }
    }
}
