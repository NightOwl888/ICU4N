using ICU4N.Impl;
using ICU4N.Impl.Coll;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Text;
using IFactory = ICU4N.Impl.ICUService.IFactory;
using CollatorFactory = ICU4N.Text.Collator.CollatorFactory;
using LocaleKeyFactory = ICU4N.Impl.ICULocaleService.LocaleKeyFactory;

namespace ICU4N.Text
{
    internal sealed class CollatorServiceShim : Collator.ServiceShim
    {
        internal override Collator GetInstance(ULocale locale)
        {
            // use service cache, it's faster than instantiation
            //          if (service.isDefault()) {
            //              return new RuleBasedCollator(locale);
            //          }
            //try
            //{
            ULocale[] actualLoc = new ULocale[1];
            Collator coll = (Collator)service.Get(locale, actualLoc);
            if (coll == null)
            {
                ///CLOVER:OFF
                //Can't really change coll after it's been initialized
                throw new MissingManifestResourceException("Could not locate Collator data");
                ///CLOVER:ON
            }
            return (Collator)coll.Clone();
            //}
            //catch (CloneNotSupportedException e)
            //{
            //    ///CLOVER:OFF
            //    throw new ICUCloneNotSupportedException(e);
            //    ///CLOVER:ON
            //}
        }

        internal override object RegisterInstance(Collator collator, ULocale locale)
        {
            // Set the collator locales while registering so that getInstance()
            // need not guess whether the collator's locales are already set properly
            // (as they are by the data loader).
            collator.SetLocale(locale, locale);
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

            protected override object HandleCreate(ULocale loc, int kind, ICUService srvc)
            {
                object coll = @delegate.CreateCollator(loc);
                return coll;
            }

            public override string GetDisplayName(string id, ULocale displayLocale)
            {
                ULocale objectLocale = new ULocale(id);
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
            return service.UnregisterFactory((IFactory)registryKey);
        }

        internal override CultureInfo[] GetAvailableLocales()
        {
            // TODO rewrite this to just wrap getAvailableULocales later
            CultureInfo[] result;
            if (service.IsDefault)
            {
                result = ICUResourceBundle.GetAvailableLocales(ICUData.ICU_COLLATION_BASE_NAME,
                        ICUResourceBundle.ICU_DATA_CLASS_LOADER);
            }
            else
            {
                result = service.GetAvailableLocales();
            }
            return result;
        }

        internal override ULocale[] GetAvailableULocales()
        {
            ULocale[] result;
            if (service.IsDefault)
            {
                result = ICUResourceBundle.GetAvailableULocales(ICUData.ICU_COLLATION_BASE_NAME,
                        ICUResourceBundle.ICU_DATA_CLASS_LOADER);
            }
            else
            {
                result = service.GetAvailableULocales();
            }
            return result;
        }

        internal override string GetDisplayName(ULocale objectLocale, ULocale displayLocale)
        {
            string id = objectLocale.GetName();
            return service.GetDisplayName(id, displayLocale);
        }

        private class CService : ICULocaleService
        {
            private class CollatorFactory : ICUResourceBundleFactory
            {
                internal CollatorFactory()
                    : base(ICUData.ICU_COLLATION_BASE_NAME)
                {
                }

                protected override object HandleCreate(ULocale uloc, int kind, ICUService srvc)
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

            /**
             * makeInstance() returns an appropriate Collator for any locale.
             * It falls back to root if there is no specific data.
             *
             * <p>Without this override, the service code would fall back to the default locale
             * which is not desirable for an algorithm with a good Unicode default,
             * like collation.
             */
            public override string ValidateFallbackLocale()
            {
                return "";
            }

            ///CLOVER:OFF
            // The following method can not be reached by testing
            protected override object HandleDefault(Key key, string[] actualIDReturn)
            {
                if (actualIDReturn != null)
                {
                    actualIDReturn[0] = "root";
                }
                try
                {
                    return MakeInstance(ULocale.ROOT);
                }
                catch (MissingManifestResourceException e)
                {
                    return null;
                }
            }
            ///CLOVER:ON
        }

        // Ported from C++ Collator::makeInstance().
        internal static Collator MakeInstance(ULocale desiredLocale)
        {
            //Output<ULocale> validLocale = new Output<ULocale>(ULocale.ROOT);
            ULocale validLocale = ULocale.ROOT;
            CollationTailoring t =
                CollationLoader.LoadTailoring(desiredLocale, out validLocale);
            return new RuleBasedCollator(t, validLocale);
        }

        private static ICULocaleService service = new CService();
    }
}
