using ICU4N.Globalization;
using ICU4N.Text;
using ICU4N.Util;
using System;

namespace ICU4N.Numerics
{
    internal class CustomSymbolCurrency : Currency // ICU4N TODO: API - this was public in ICU4J
    {
        //private static final long serialVersionUID = 2497493016770137670L;
        // TODO: Serialization methods?

        private readonly string symbol1;
        private readonly string symbol2;

        public static Currency Resolve(Currency currency, UCultureInfo locale, DecimalFormatSymbols symbols) // ICU4N NOTE: locale is never referenced here
        {
            if (currency == null)
            {
                currency = symbols.Currency;
            }
            string currency1Sym = symbols.CurrencySymbol;
            string currency2Sym = symbols.InternationalCurrencySymbol;
            if (currency == null)
            {
                return new CustomSymbolCurrency("XXX", currency1Sym, currency2Sym);
            }
            if (!currency.Equals(symbols.Currency))
            {
                return currency;
            }
            string currency1 = currency.GetName(symbols.UCulture, Currency.SymbolName, out bool _);
            string currency2 = currency.CurrencyCode;
            if (!currency1.Equals(currency1Sym, StringComparison.Ordinal) || !currency2.Equals(currency2Sym, StringComparison.Ordinal))
            {
                return new CustomSymbolCurrency(currency2, currency1Sym, currency2Sym);
            }
            return currency;
        }

        public CustomSymbolCurrency(string isoCode, string currency1Sym, string currency2Sym)
            : base(isoCode)
        {
            this.symbol1 = currency1Sym ?? throw new ArgumentNullException(nameof(currency1Sym));
            this.symbol2 = currency2Sym ?? throw new ArgumentNullException(nameof(currency2Sym));
        }

        public override string GetName(UCultureInfo locale, CurrencyNameStyle nameStyle, out bool isChoiceFormat)
        {
            if (nameStyle == CurrencyNameStyle.SymbolName)
            {
                isChoiceFormat = false;
                return symbol1;
            }
            return base.GetName(locale, nameStyle, out isChoiceFormat);
        }

        public override string GetName(
            UCultureInfo locale, CurrencyNameStyle nameStyle, string pluralCount, out bool isChoiceFormat)
        {
            if (nameStyle == CurrencyNameStyle.PluralLongName &&
#pragma warning disable CS0618 // Type or member is obsolete
                subType.Equals("XXX", StringComparison.Ordinal))
#pragma warning restore CS0618 // Type or member is obsolete
            {
                // Plural in absence of a currency should return the symbol
                isChoiceFormat = false;
                return symbol1;
            }
            return base.GetName(locale, nameStyle, pluralCount, out isChoiceFormat);
        }

        public override string CurrencyCode => symbol2;

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ symbol1.GetHashCode() ^ symbol2.GetHashCode();
        }

        public override bool Equals(object other)
        {
            if (other is null) return false;
            if (!(other is CustomSymbolCurrency otherCurrency)) return false;

            return base.Equals(other)
                    && otherCurrency.symbol1.Equals(symbol1, StringComparison.Ordinal)
                    && otherCurrency.symbol2.Equals(symbol2, StringComparison.Ordinal);
        }
    }
}
