// ICU4N TODO: Port issues

//using ICU4N.Impl;
//using ICU4N.Support.IO;
//using ICU4N.Support.Text;
//using ICU4N.Util;
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Resources;
//using System.Text;

//namespace ICU4N.Text
//{
//    internal sealed class BreakIteratorFactory : BreakIterator.BreakIteratorServiceShim
//    {

//        public override object RegisterInstance(BreakIterator iter, ULocale locale, int kind)
//        {
//            iter.SetText(new StringCharacterIterator(""));
//            return service.registerObject(iter, locale, kind);
//        }

//        public override bool Unregister(object key)
//        {
//            if (service.isDefault())
//            {
//                return false;
//            }
//            return service.unregisterFactory((Factory)key);
//        }

//        public override CultureInfo[] GetAvailableLocales()
//        {
//            if (service == null)
//            {
//                return ICUResourceBundle.getAvailableLocales();
//            }
//            else
//            {
//                return service.getAvailableLocales();
//            }
//        }

//        public override ULocale[] GetAvailableULocales()
//        {
//            if (service == null)
//            {
//                return ICUResourceBundle.getAvailableULocales();
//            }
//            else
//            {
//                return service.getAvailableULocales();
//            }
//        }

//        public override BreakIterator CreateBreakIterator(ULocale locale, int kind)
//        {
//            // TODO: convert to ULocale when service switches over
//            if (service.isDefault())
//            {
//                return CreateBreakInstance(locale, kind);
//            }
//            ULocale[] actualLoc = new ULocale[1];
//            BreakIterator iter = (BreakIterator)service.get(locale, kind, actualLoc);
//            iter.SetLocale(actualLoc[0], actualLoc[0]); // services make no distinction between actual & valid
//            return iter;
//        }

//        private class BFService : ICULocaleService
//        {
//            internal BFService()
//                : base("BreakIterator")
//            {


//                RegisterFactory(new RBBreakIteratorFactory());

//                MarkDefault();
//            }

//            /**
//             * createBreakInstance() returns an appropriate BreakIterator for any locale.
//             * It falls back to root if there is no specific data.
//             *
//             * <p>Without this override, the service code would fall back to the default locale
//             * which is not desirable for an algorithm with a good Unicode default,
//             * like break iteration.
//             */
//            public override string ValidateFallbackLocale()
//            {
//                return "";
//            }
//        }

//        internal class RBBreakIteratorFactory : ICUResourceBundleFactory
//        {
//            protected override Object HandleCreate(ULocale loc, int kind, ICUService srvc)
//            {
//                return CreateBreakInstance(loc, kind);
//            }
//        }

//        internal static readonly ICULocaleService service = new BFService();


//        /** KIND_NAMES are the resource key to be used to fetch the name of the
//         *             pre-compiled break rules.  The resource bundle name is "boundaries".
//         *             The value for each key will be the rules to be used for the
//         *             specified locale - "word" -> "word_th" for Thai, for example.
//         */
//        private static readonly string[] KIND_NAMES = {
//            "grapheme", "word", "line", "sentence", "title"
//    };


//        private static BreakIterator CreateBreakInstance(ULocale locale, int kind)
//        {

//            RuleBasedBreakIterator iter = null;
//            ICUResourceBundle rb = ICUResourceBundle.
//                    getBundleInstance(ICUData.ICU_BRKITR_BASE_NAME, locale,
//                            ICUResourceBundle.OpenType.LOCALE_ROOT);

//            //
//            //  Get the binary rules.
//            //
//            ByteBuffer bytes = null;
//            string typeKeyExt = null;
//            if (kind == BreakIterator.KIND_LINE)
//            {
//                String lbKeyValue = locale.GetKeywordValue("lb");
//                if (lbKeyValue != null && (lbKeyValue.Equals("strict") || lbKeyValue.Equals("normal") || lbKeyValue.Equals("loose")))
//                {
//                    typeKeyExt = "_" + lbKeyValue;
//                }
//            }

//            try
//            {
//                string typeKey = (typeKeyExt == null) ? KIND_NAMES[kind] : KIND_NAMES[kind] + typeKeyExt;
//                string brkfname = rb.getStringWithFallback("boundaries/" + typeKey);
//                string rulesFileName = ICUData.ICU_BRKITR_NAME + '/' + brkfname;
//                bytes = ICUBinary.GetData(rulesFileName);
//            }
//            catch (Exception e)
//            {
//                throw new MissingManifestResourceException(e.ToString(), e /*, "", ""*/);
//            }

//            //
//            // Create a normal RuleBasedBreakIterator.
//            //
//            try
//            {
//                iter = RuleBasedBreakIterator.GetInstanceFromCompiledRules(bytes);
//            }
//            catch (IOException e)
//            {
//                // Shouldn't be possible to get here.
//                // If it happens, the compiled rules are probably corrupted in some way.
//                Assert.Fail(e);
//            }
//            // TODO: Determine valid and actual locale correctly.
//            ULocale uloc = ULocale.ForLocale(rb.getLocale());
//            iter.SetLocale(uloc, uloc);
//            iter.BreakType = kind;

//            // filtered break
//            if (kind == BreakIterator.KIND_SENTENCE)
//            {
//                string ssKeyword = locale.GetKeywordValue("ss");
//                if (ssKeyword != null && ssKeyword.Equals("standard"))
//                {
//                    ULocale @base = new ULocale(locale.GetBaseName());
//                    return FilteredBreakIteratorBuilder.GetInstance(@base).WrapIteratorWithFilter(iter);
//                }
//            }

//            return iter;

//        }
//    }
//}
