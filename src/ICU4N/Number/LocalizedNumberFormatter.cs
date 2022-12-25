using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Text;
using Number = J2N.Numerics.Number;

namespace ICU4N.Numerics
{
    /// <summary>
    /// A <see cref="NumberFormatter"/> that has a locale associated with it; this means .Format() methods are available.
    /// </summary>
    /// <draft>ICU 60</draft>
    /// <provisional>This API might change or be removed in a future release.</provisional>
    /// <seealso cref="NumberFormatter"/>
    internal class LocalizedNumberFormatter : NumberFormatterSettings<LocalizedNumberFormatter>
    {
        //internal static readonly AtomicLongFieldUpdater<LocalizedNumberFormatter> callCount = AtomicLongFieldUpdater
        //    .NewUpdater(typeof(LocalizedNumberFormatter), "callCountInternal");

    volatile long callCountInternal; // do not access directly; use callCount instead
        volatile LocalizedNumberFormatter savedWithUnit;
        volatile NumberFormatterImpl compiled;

        internal LocalizedNumberFormatter(NumberFormatterSettings/*<?>*/ parent, int key, object value)
            : base(parent, key, value)
        {
        }

        /**
         * Format the given byte, short, int, or long to a string using the settings specified in the NumberFormatter fluent
         * setting chain.
         *
         * @param input
         *            The number to format.
         * @return A FormattedNumber object; call .toString() to get the string.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public FormattedNumber Format(long input)
        {
            return Format(new DecimalQuantity_DualStorageBCD(input));
        }

        /**
         * Format the given float or double to a string using the settings specified in the NumberFormatter fluent setting
         * chain.
         *
         * @param input
         *            The number to format.
         * @return A FormattedNumber object; call .toString() to get the string.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public FormattedNumber Format(double input)
        {
            return Format(new DecimalQuantity_DualStorageBCD(input));
        }

        /**
         * Format the given {@link BigInteger}, {@link BigDecimal}, or other {@link Number} to a string using the settings
         * specified in the NumberFormatter fluent setting chain.
         *
         * @param input
         *            The number to format.
         * @return A FormattedNumber object; call .toString() to get the string.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public FormattedNumber Format(Number input)
        {
            return Format(new DecimalQuantity_DualStorageBCD(input));
        }

        /**
         * Format the given {@link Measure} or {@link CurrencyAmount} to a string using the settings specified in the
         * NumberFormatter fluent setting chain.
         *
         * <p>
         * The unit specified here overrides any unit that may have been specified in the setter chain. This method is
         * intended for cases when each input to the number formatter has a different unit.
         *
         * @param input
         *            The number to format.
         * @return A FormattedNumber object; call .toString() to get the string.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public FormattedNumber Format(Measure input)
        {
            MeasureUnit unit = input.Unit;
            Number number = input.Number;
            // Use this formatter if possible
            //if (Utility.equals(resolve().unit, unit))
            if (EqualityComparer<MeasureUnit>.Default.Equals(Resolve().unit, unit))
            {
                return Format(number);
            }
            // This mechanism saves the previously used unit, so if the user calls this method with the
            // same unit multiple times in a row, they get a more efficient code path.
            LocalizedNumberFormatter withUnit = savedWithUnit;
            //if (withUnit == null || !Utility.equals(withUnit.resolve().unit, unit))
            if (withUnit == null || !EqualityComparer<MeasureUnit>.Default.Equals(withUnit.Resolve().unit, unit)) // !Utility.equals(withUnit.resolve().unit, unit))
            {
                withUnit = new LocalizedNumberFormatter(this, KEY_UNIT, unit);
                savedWithUnit = withUnit;
            }
            return withUnit.Format(number);
        }

        /**
         * This is the core entrypoint to the number formatting pipeline. It performs self-regulation: a static code path
         * for the first few calls, and compiling a more efficient data structure if called repeatedly.
         *
         * <p>
         * This function is very hot, being called in every call to the number formatting pipeline.
         *
         * @param fq
         *            The quantity to be formatted.
         * @return The formatted number result.
         *
         * @internal
         * @deprecated ICU 60 This API is ICU internal only.
         */
        [Obsolete("ICU 60 This API is ICU internal only.")]
    public FormattedNumber Format(IDecimalQuantity fq)
        {
            MacroProps macros = Resolve();
            // NOTE: In Java, the atomic increment logic is slightly different than ICU4C.
            // It seems to be more efficient to make just one function call instead of two.
            // Further benchmarking is required.
            long currentCount = callCount.IncrementAndGet(this);
            NumberStringBuilder str = new NumberStringBuilder();
            MicroProps micros;
            if (currentCount == macros.threshold.ToInt64())
            {
                compiled = NumberFormatterImpl.FromMacros(macros);
                micros = compiled.Apply(fq, str);
            }
            else if (compiled != null)
            {
                micros = compiled.Apply(fq, str);
            }
            else
            {
                micros = NumberFormatterImpl.ApplyStatic(macros, fq, str);
            }
            return new FormattedNumber(str, fq, micros);
        }

        internal override LocalizedNumberFormatter Create(int key, object value)
        {
            return new LocalizedNumberFormatter(this, key, value);
        }
    }
}
