using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Impl.Coll;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace ICU4N.Text
{
    internal sealed class CollatorServiceShim : Collator.ServiceShim
    {
        internal override Collator GetInstance(UCultureInfo locale)
        {
            // use service cache, it's faster than instantiation
            //          if (service.isDefault()) {
            //              return new RuleBasedCollator(locale);
            //          }
            //try
            //{
            Collator coll = (Collator)service.Get(locale, out _);
            if (coll == null)
            {
                ////CLOVER:OFF
                //Can't really change coll after it's been initialized
                throw new MissingManifestResourceException("Could not locate Collator data");
                ////CLOVER:ON
            }
            return (Collator)coll.Clone();
            //}
            //catch (CloneNotSupportedException e)
            //{
            //    ////CLOVER:OFF
            //    throw new ICUCloneNotSupportedException(e);
            //    ////CLOVER:ON
            //}
        }

        internal override object RegisterInstance(Collator collator, UCultureInfo locale)
        {
            // Set the collator locales while registering so that getInstance()
            // need not guess whether the collator's locales are already set properly
            // (as they are by the data loader).
            collator.SetCulture(locale, locale);
            return service.RegisterObject(collator, locale);
        }

        private class CFactory : LocaleKeyFactory
        {
            CollatorFactory @delegate;

            internal CFactory(CollatorFactory fctry)
                : base(fctry.Visible)
            {
                this.@delegate = fctry;
            }

            protected override object HandleCreate(UCultureInfo loc, int kind, ICUService srvc)
            {
                object coll = @delegate.CreateCollator(loc);
                return coll;
            }

            public override string GetDisplayName(string id, UCultureInfo displayLocale)
            {
                UCultureInfo objectLocale = new UCultureInfo(id);
                return @delegate.GetDisplayName(objectLocale, displayLocale);
            }

            protected override ICollection<string> GetSupportedIDs() // ICU4N specific - return type changed from ISet<string> to ICollection<string>
            {
                return @delegate.GetSupportedLocaleIDs();
            }
        }

        internal override object RegisterFactory(CollatorFactory f)
        {
            return service.RegisterFactory(new CFactory(f));
        }

        internal override bool Unregister(object registryKey)
        {
            return service.UnregisterFactory((IServiceFactory)registryKey);
        }

        internal override CultureInfo[] GetCultures(UCultureTypes types) // ICU4N: Renamed from GetAvailableLocales
        {
            // TODO rewrite this to just wrap getAvailableULocales later
            CultureInfo[] result;
            if (service.IsDefault)
            {
                result = ICUResourceBundle.GetCultures(
                    ICUData.IcuCollationBaseName,
                    CollationData.IcuDataAssembly,
                    types);
            }
            else
            {
                result = service.GetCultures(types);
            }
            return result;
        }

        internal override UCultureInfo[] GetUCultures(UCultureTypes types) // ICU4N: Renamed from GetAvailableULocales
        {
            UCultureInfo[] result;
            if (service.IsDefault)
            {
                result = ICUResourceBundle.GetUCultures(
                    ICUData.IcuCollationBaseName,
                    CollationData.IcuDataAssembly,
                    types);
            }
            else
            {
                result = service.GetUCultures(types);
            }
            return result;
        }

        internal override string GetDisplayName(UCultureInfo objectLocale, UCultureInfo displayLocale)
        {
            string id = objectLocale.FullName;
            return service.GetDisplayName(id, displayLocale);
        }

        private class CService : ICULocaleService
        {
            private class CollatorFactory : ICUResourceBundleFactory
            {
                internal CollatorFactory()
                    : base(ICUData.IcuCollationBaseName)
                {
                }

                protected override object HandleCreate(UCultureInfo uloc, int kind, ICUService srvc)
                {
                    return CollatorServiceShim.MakeInstance(uloc);
                }
            }


            internal CService()
                        : base("Collator")
            {
                this.RegisterFactory(new CollatorFactory());
                MarkDefault();
            }

            /// <summary>
            /// <see cref="MakeInstance(UCultureInfo)"/> returns an appropriate <see cref="Collator"/> for any locale.
            /// It falls back to root if there is no specific data.
            /// <para/>
            /// Without this override, the service code would fall back to the default locale
            /// which is not desirable for an algorithm with a good Unicode default,
            /// like collation.
            /// </summary>
            public override string ValidateFallbackLocale()
            {
                return "";
            }

            ////CLOVER:OFF
            // The following method can not be reached by testing
            protected override object HandleDefault(ICUServiceKey key, out string actualIDReturn)
            {
                actualIDReturn = "root";
                try
                {
                    return MakeInstance(UCultureInfo.InvariantCulture);
                }
                catch (MissingManifestResourceException)
                {
                    return null;
                }
            }
            ////CLOVER:ON
        }

        // Ported from C++ Collator::makeInstance().
        internal static Collator MakeInstance(UCultureInfo desiredLocale)
        {
            CollationTailoring t =
                CollationLoader.LoadTailoring(desiredLocale, out UCultureInfo validLocale);
            return new RuleBasedCollator(t, validLocale);
        }

        private static readonly ICULocaleService service = new CService();
    }
}
