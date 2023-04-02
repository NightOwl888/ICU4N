#nullable enable

namespace ICU4N.Text
{
    internal abstract partial class NFSubstitution
    {
        internal NFRuleSet? RuleSet => ruleSet;
        internal DecimalFormat? NumberFormat => numberFormat;
    }

    internal partial class FractionalPartSubstitution
    {
        internal bool ByDigits => byDigits;
        internal bool UseSpaces => useSpaces;
    }

    internal partial class ModulusSubstitution
    {
        internal long Divisor => divisor;
        internal NFRule? RuleToUse => ruleToUse;
    }

    internal partial class MultiplierSubstitution
    {
        internal long Divisor => divisor;
    }

    internal partial class NumeratorSubstitution
    {
        internal double Denominator => denominator;
        internal bool WithZeros => withZeros;
    }
}
