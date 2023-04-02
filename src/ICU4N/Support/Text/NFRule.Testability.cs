namespace ICU4N.Text
{
    internal sealed partial class NFRule
    {
        //internal int BaseValue

        internal int Radix => radix;
        internal short Exponent => exponent;

        //internal char DecimalPo
        internal string RuleText => ruleText;
        internal PluralFormat RulePatternFormat => rulePatternFormat;

        internal NFSubstitution Sub1 => sub1;
        internal NFSubstitution Sub2 => sub2;

    }
}
