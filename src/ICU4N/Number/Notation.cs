﻿using J2N.Numerics;
using System;
using System.Collections.Generic;
using System.Text;
using static ICU4N.Numerics.NumberFormatter;
using static ICU4N.Text.CompactDecimalFormat;

namespace ICU4N.Numerics
{
    /// <summary>
    /// A class that defines the notation style to be used when formatting numbers in <see cref="NumberFormatter"/>.
    /// </summary>
    /// <draft>ICU 60</draft>
    /// <provisional>This API might change or be removed in a future release.</provisional>
    /// <seealso cref="NumberFormatter"/>
    internal class Notation // ICU4N TODO: API - this was public in ICU4J
    {
        // TODO: Support engineering intervals other than 3?
        private static readonly ScientificNotation SCIENTIFIC = new ScientificNotation(1, false, 1, SignDisplay.Auto);
        private static readonly ScientificNotation ENGINEERING = new ScientificNotation(3, false, 1, SignDisplay.Auto);
        private static readonly CompactNotation COMPACT_SHORT = new CompactNotation(CompactStyle.Short);
        private static readonly CompactNotation COMPACT_LONG = new CompactNotation(CompactStyle.Long);
        private static readonly SimpleNotation SIMPLE = new SimpleNotation();

        /* package-private */
        internal Notation()
        {
        }

        /**
         * Print the number using scientific notation (also known as scientific form, standard index form, or standard form
         * in the UK). The format for scientific notation varies by locale; for example, many Western locales display the
         * number in the form "#E0", where the number is displayed with one digit before the decimal separator, zero or more
         * digits after the decimal separator, and the corresponding power of 10 displayed after the "E".
         *
         * <para/>
         * Example outputs in <em>en-US</em> when printing 8.765E4 through 8.765E-3:
         *
         * <pre>
         * 8.765E4
         * 8.765E3
         * 8.765E2
         * 8.765E1
         * 8.765E0
         * 8.765E-1
         * 8.765E-2
         * 8.765E-3
         * 0E0
         * </pre>
         *
         * @return A ScientificNotation for chaining or passing to the NumberFormatter notation() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public static ScientificNotation Scientific => SCIENTIFIC;

        /**
         * Print the number using engineering notation, a variant of scientific notation in which the exponent must be
         * divisible by 3.
         *
         * <para/>
         * Example outputs in <em>en-US</em> when printing 8.765E4 through 8.765E-3:
         *
         * <pre>
         * 87.65E3
         * 8.765E3
         * 876.5E0
         * 87.65E0
         * 8.765E0
         * 876.5E-3
         * 87.65E-3
         * 8.765E-3
         * 0E0
         * </pre>
         *
         * @return A ScientificNotation for chaining or passing to the NumberFormatter notation() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public static ScientificNotation Engineering => ENGINEERING;

        /**
         * Print the number using short-form compact notation.
         *
         * <para/>
         * <em>Compact notation</em>, defined in Unicode Technical Standard #35 Part 3 Section 2.4.1, prints numbers with
         * localized prefixes or suffixes corresponding to different powers of ten. Compact notation is similar to
         * engineering notation in how it scales numbers.
         *
         * <para/>
         * Compact notation is ideal for displaying large numbers (over ~1000) to humans while at the same time minimizing
         * screen real estate.
         *
         * <para/>
         * In short form, the powers of ten are abbreviated. In <em>en-US</em>, the abbreviations are "K" for thousands, "M"
         * for millions, "B" for billions, and "T" for trillions. Example outputs in <em>en-US</em> when printing 8.765E7
         * through 8.765E0:
         *
         * <pre>
         * 88M
         * 8.8M
         * 876K
         * 88K
         * 8.8K
         * 876
         * 88
         * 8.8
         * </pre>
         *
         * <para/>
         * When compact notation is specified without an explicit rounding strategy, numbers are rounded off to the closest
         * integer after scaling the number by the corresponding power of 10, but with a digit shown after the decimal
         * separator if there is only one digit before the decimal separator. The default compact notation rounding strategy
         * is equivalent to:
         *
         * <pre>
         * Rounder.integer().withMinDigits(2)
         * </pre>
         *
         * @return A CompactNotation for passing to the NumberFormatter notation() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public static CompactNotation CompactShort => COMPACT_SHORT;

        /**
         * Print the number using long-form compact notation. For more information on compact notation, see
         * {@link #compactShort}.
         *
         * <para/>
         * In long form, the powers of ten are spelled out fully. Example outputs in <em>en-US</em> when printing 8.765E7
         * through 8.765E0:
         *
         * <pre>
         * 88 million
         * 8.8 million
         * 876 thousand
         * 88 thousand
         * 8.8 thousand
         * 876
         * 88
         * 8.8
         * </pre>
         *
         * @return A CompactNotation for passing to the NumberFormatter notation() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public static CompactNotation CompactLong => COMPACT_LONG;

        /**
         * Print the number using simple notation without any scaling by powers of ten. This is the default behavior.
         *
         * <para/>
         * Since this is the default behavior, this method needs to be called only when it is necessary to override a
         * previous setting.
         *
         * <para/>
         * Example outputs in <em>en-US</em> when printing 8.765E7 through 8.765E0:
         *
         * <pre>
         * 87,650,000
         * 8,765,000
         * 876,500
         * 87,650
         * 8,765
         * 876.5
         * 87.65
         * 8.765
         * </pre>
         *
         * @return A SimpleNotation for passing to the NumberFormatter notation() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public static SimpleNotation Simple => SIMPLE;
    }
}
