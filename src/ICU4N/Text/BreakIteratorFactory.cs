using ICU4N.Globalization;
using ICU4N.Impl;
using J2N.IO;
using J2N.Text;
using System;
using System.Globalization;
using System.IO;
using System.Resources;

namespace ICU4N.Text
{
    /// <author>Ram</author>
    internal sealed class BreakIteratorFactory : BreakIterator.BreakIteratorServiceShim
    {

        public override object RegisterInstance(BreakIterator iter, UCultureInfo locale, int kind)
        {
            iter.SetText(new StringCharacterIterator(""));
            return service.RegisterObject(iter, locale, kind);
        }

        public override bool Unregister(object key)
        {
            if (service.IsDefault)
            {
                return false;
            }
            return service.UnregisterFactory((IServiceFactory)key);
        }

        public override CultureInfo[] GetCultures(UCultureTypes types) // ICU4N: Renamed from GetAvailableLocales
        {
            if (service == null)
            {
                return ICUResourceBundle.GetCultures(types);
            }
            else
            {
                return service.GetCultures(types);
            }
        }

        public override UCultureInfo[] GetUCultures(UCultureTypes types) // ICU4N: Renamed from GetAvailableLocales
        {
            if (service == null)
            {
                return ICUResourceBundle.GetUCultures(types);
            }
            else
            {
                return service.GetUCultures(types);
            }
        }

        public override BreakIterator CreateBreakIterator(UCultureInfo locale, int kind)
        {
            // TODO: convert to UCultureInfo when service switches over
            if (service.IsDefault)
            {
                return CreateBreakInstance(locale, kind);
            }
            BreakIterator iter = (BreakIterator)service.Get(locale, kind, out UCultureInfo actualLoc);
            iter.SetCulture(actualLoc, actualLoc); // services make no distinction between actual & valid
            return iter;
        }

        private class BFService : ICULocaleService
        {
            internal BFService()
                : base("BreakIterator")
            {


                RegisterFactory(new RBBreakIteratorFactory());

                MarkDefault();
            }

            /// <summary>
            /// <see cref="CreateBreakInstance(UCultureInfo, int)"/> returns an appropriate <see cref="BreakIterator"/> for any locale.
            /// It falls back to root if there is no specific data.
            /// <para/>
            /// Without this override, the service code would fall back to the default locale
            /// which is not desirable for an algorithm with a good Unicode default,
            /// like break iteration.
            /// </summary>
            public override string ValidateFallbackLocale()
            {
                return "";
            }
        }

        internal class RBBreakIteratorFactory : ICUResourceBundleFactory
        {
            protected override object HandleCreate(UCultureInfo loc, int kind, ICUService srvc)
            {
                return CreateBreakInstance(loc, kind);
            }
        }

        internal static readonly ICULocaleService service = new BFService();

        /// <summary>
        /// KIND_NAMES are the resource key to be used to fetch the name of the
        ///             pre-compiled break rules.  The resource bundle name is "boundaries".
        ///             The value for each key will be the rules to be used for the
        ///             specified locale - "word" -> "word_th" for Thai, for example.            
        /// </summary>
        private static readonly string[] KIND_NAMES = {
                "grapheme", "word", "line", "sentence", "title"
        };


        private static BreakIterator CreateBreakInstance(UCultureInfo locale, int kind)
        {

            RuleBasedBreakIterator iter = null;
            ICUResourceBundle rb = ICUResourceBundle.
                    GetBundleInstance(ICUData.IcuBreakIteratorBaseName, locale,
                            OpenType.LocaleRoot);

            //
            //  Get the binary rules.
            //
            ByteBuffer bytes = null;
            string typeKeyExt = null;
            if (kind == BreakIterator.KIND_LINE)
            {
                locale.Keywords.TryGetValue("lb", out string lbKeyValue);
                if (lbKeyValue != null && (lbKeyValue.Equals("strict") || lbKeyValue.Equals("normal") || lbKeyValue.Equals("loose")))
                {
                    typeKeyExt = "_" + lbKeyValue;
                }
            }

            try
            {
                string typeKey = (typeKeyExt == null) ? KIND_NAMES[kind] : KIND_NAMES[kind] + typeKeyExt;
                string brkfname = rb.GetStringWithFallback("boundaries/" + typeKey);
                string rulesFileName = ICUData.IcuBreakIteratorName + '/' + brkfname;
                bytes = ICUBinary.GetData(rulesFileName);
            }
            catch (Exception e)
            {
                throw new MissingManifestResourceException(e.ToString(), e /*, "", ""*/);
            }

            //
            // Create a normal RuleBasedBreakIterator.
            //
            try
            {
#pragma warning disable 612, 618
                iter = RuleBasedBreakIterator.GetInstanceFromCompiledRules(bytes);
#pragma warning restore 612, 618
            }
            catch (IOException e)
            {
                // Shouldn't be possible to get here.
                // If it happens, the compiled rules are probably corrupted in some way.
                Assert.Fail(e);
            }
            // TODO: Determine valid and actual locale correctly.
            UCultureInfo uloc = rb.Culture.ToUCultureInfo();
            iter.SetCulture(uloc, uloc);
            iter.BreakType = kind;

            // filtered break
            if (kind == BreakIterator.KIND_SENTENCE)
            {
                if (locale.Keywords.TryGetValue("ss", out string ssKeyword) && ssKeyword != null && ssKeyword.Equals("standard"))
                {
                    UCultureInfo @base = new UCultureInfo(locale.Name);
                    return FilteredBreakIteratorBuilder.GetInstance(@base).WrapIteratorWithFilter(iter);
                }
            }

            return iter;

        }
    }
}
