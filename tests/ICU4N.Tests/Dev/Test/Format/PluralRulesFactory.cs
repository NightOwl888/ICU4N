using ICU4N.Globalization;
using ICU4N.Text;

namespace ICU4N.Dev.Test.Format
{
    /// <author>markdavis</author>
    internal abstract class PluralRulesFactory : ICU4N.Text.PluralRulesFactory
    {
        internal static readonly PluralRulesFactory Normal = new PluralRulesFactoryVanilla();

        private PluralRulesFactory() { }

        private class PluralRulesFactoryVanilla : PluralRulesFactory
        {
            public override bool HasOverride(UCultureInfo locale)
            {
                return false;
            }

            public override PluralRules GetInstance(string localeName, PluralType ordinal)
            {
                return PluralRules.GetInstance(localeName, ordinal);
            }

            public override PluralRules GetInstance(UCultureInfo locale, PluralType ordinal)
            {
                return PluralRules.GetInstance(locale, ordinal);
            }

            public override UCultureInfo[] GetUCultures()
            {
                return PluralRules.GetUCultures();
            }

            public override UCultureInfo GetFunctionalEquivalent(UCultureInfo locale, out bool isAvailable)
            {
                return PluralRules.GetFunctionalEquivalent(locale, out isAvailable);
            }

            public override UCultureInfo GetFunctionalEquivalent(UCultureInfo locale)
            {
                return PluralRules.GetFunctionalEquivalent(locale);
            }
        }
    }
}
