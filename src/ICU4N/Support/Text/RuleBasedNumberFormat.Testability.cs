using System.Collections.Generic;

namespace ICU4N.Text
{
#if FEATURE_LEGACY_NUMBER_FORMAT
    public
#else
    internal
#endif
        partial class RuleBasedNumberFormat
    {
        internal NFRuleSet[] RuleSets => ruleSets;

        internal IDictionary<string, NFRuleSet> RuleSetsMap => ruleSetsMap;

        internal string LenientParseRules => lenientParseRules;

        internal string PostProcessRules => postProcessRules;

        internal string[] PublicRuleSetNames => publicRuleSetNames;
    }
}
