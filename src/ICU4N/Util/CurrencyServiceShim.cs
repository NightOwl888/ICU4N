using ICU4N.Globalization;
using ICU4N.Impl;
using System.Globalization;

namespace ICU4N.Util
{
    /// <summary>
    /// This is a package-access implementation of registration for
    /// currency.  The shim is instantiated by reflection in <see cref="Currency"/>, all
    /// dependencies on ICUService are located in this file. This structure
    /// is to allow ICU4J to be built without service registration support.
    /// </summary>
    internal sealed class CurrencyServiceShim : Currency.ServiceShim
    {
        internal override CultureInfo[] GetCultures(UCultureTypes types)
        {
            if (service.IsDefault)
            {
                return ICUResourceBundle.GetCultures(types);
            }
            return service.GetCultures(types);
        }

        internal override UCultureInfo[] GetUCultures(UCultureTypes types)
        {
            if (service.IsDefault)
            {
                return ICUResourceBundle.GetUCultures(types);
            }
            return service.GetUCultures(types);
        }

        internal override Currency CreateInstance(UCultureInfo loc)
        {
            // TODO: convert to ULocale when service switches over

            if (service.IsDefault)
            {
                return Currency.CreateCurrency(loc);
            }
            Currency curr = (Currency)service.Get(loc);
            return curr;
        }

        internal override object RegisterInstance(Currency currency, UCultureInfo locale)
        {
            return service.RegisterObject(currency, locale);
        }

        internal override bool Unregister(object registryKey)
        {
            return service.UnregisterFactory((IServiceFactory)registryKey);
        }

        private class CFService : ICULocaleService
        {
            internal CFService()
                : base("Currency")
            {
                RegisterFactory(new CurrencyFactory());
                MarkDefault();
            }

            internal class CurrencyFactory : ICUResourceBundleFactory
            {
                protected override object HandleCreate(UCultureInfo loc, int kind, ICUService srvc)
                {
                    return Currency.CreateCurrency(loc);
                }
            }
        }
        private static readonly ICULocaleService service = new CFService();
    }
}
