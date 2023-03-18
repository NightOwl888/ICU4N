using System.Collections.Generic;

namespace ICU4N.Text
{
    internal sealed partial class NFRuleSet
    {
        internal NFRule[] Rules => rules;
        //internal NFRule[] NonNumericalRules => nonNumericalRules; // Already internal
        internal List<NFRule> FractionRules => fractionRules;

    }
}
