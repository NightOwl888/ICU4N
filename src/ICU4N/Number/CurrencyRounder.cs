using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Numerics
{
    /// <summary>
    /// A class that defines a rounding strategy parameterized by a currency to be used when formatting numbers in
    /// <see cref="NumberFormatter"/>.
    /// <para/>
    /// To create a <see cref="CurrencyRounder"/>, use one of the factory methods on <see cref="Rounder"/>.
    /// </summary>
    /// <draft>ICU 60</draft>
    /// <provisional>This API might change or be removed in a future release.</provisional>
    /// <seealso cref="NumberFormatter"/>
    internal abstract class CurrencyRounder : Rounder // ICU4N TODO: API - this was public in ICU4J
    {
        /* package-private */
        internal CurrencyRounder()
        {
        }

        /**
         * Associates a currency with this rounding strategy.
         *
         * <p>
         * <strong>Calling this method is <em>not required</em></strong>, because the currency specified in unit() or via a
         * CurrencyAmount passed into format(Measure) is automatically applied to currency rounding strategies. However,
         * this method enables you to override that automatic association.
         *
         * <p>
         * This method also enables numbers to be formatted using currency rounding rules without explicitly using a
         * currency format.
         *
         * @param currency
         *            The currency to associate with this rounding strategy.
         * @return A Rounder for chaining or passing to the NumberFormatter rounding() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public Rounder WithCurrency(Currency currency)
        {
            if (currency != null)
            {
                return ConstructFromCurrency(this, currency);
            }
            else
            {
                throw new ArgumentException("Currency must not be null"); // ICU4N TODO: Exception type
            }
        }
    }
}
