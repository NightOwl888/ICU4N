using Double = J2N.Numerics.Double;

namespace ICU4N.Util
{
    /// <summary>
    /// An amount of currency, consisting of a Number and a <see cref="Currency"/>.
    /// <see cref="CurrencyAmount"/> objects are immutable.
    /// </summary>
    /// <seealso cref="J2N.Numerics.Number"/>
    /// <seealso cref="Currency"/>
    /// <author>Alan Liu</author>
    /// <stable>ICU 3.0</stable>
    // ICU4N TODO: API - Make generic? Generally subclasses will only be 1 numeric type
    internal class CurrencyAmount : Measure // ICU4N TODO: API - this was public in ICU4J
    {
        /// <summary>
        /// Constructs a new object given a number and a currency.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="currency">The currency.</param>
        /// <stable>ICU 3.0</stable>
        public CurrencyAmount(J2N.Numerics.Number number, Currency currency)
            : base(number, currency)
        {
        }

        /// <summary>
        /// Constructs a new object given a double value and a currency.
        /// </summary>
        /// <param name="number">A <see cref="double"/> value.</param>
        /// <param name="currency">The currency.</param>
        /// <stable>ICU 3.0</stable>
        public CurrencyAmount(double number, Currency currency)
            : base(Double.GetInstance(number), currency)
        {
        }

        /////**
        //// * Constructs a new object given a number and a Java currency.
        //// * @param number the number
        //// * @param currency the currency
        //// * @draft ICU 60
        //// */
        ////public CurrencyAmount(Number number, java.util.Currency currency)
        ////{
        ////    this(number, Currency.fromJavaCurrency(currency));
        ////}

        /////**
        //// * Constructs a new object given a double value and a Java currency.
        //// * @param number a double value
        //// * @param currency the currency
        //// * @draft ICU 60
        //// */
        ////public CurrencyAmount(double number, java.util.Currency currency)
        ////{
        ////    this(number, Currency.fromJavaCurrency(currency));
        ////}

        /// <summary>
        /// Gets the currency of this object.
        /// </summary>
        /// <stable>ICU 3.0</stable>
        public Currency Currency => (Currency)Unit;
    }
}
