﻿using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// Post processor for RBNF output.
    /// </summary>
    internal interface IRbnfPostProcessor
    {
        /// <summary>
        /// Initialization routine for this instance, called once
        /// immediately after first construction and never again.
        /// </summary>
        /// <param name="formatter">The formatter that will be using this post-processor.</param>
        /// <param name="rules">The special rules for this post-procesor.</param>
        void Init(RuleBasedNumberFormat formatter, string rules);

        /// <summary>
        /// Work routine.  Post process the output, which was generated by the
        /// ruleset with the given name.
        /// </summary>
        /// <param name="output">The output of the main RBNF processing.</param>
        /// <param name="ruleSet">The rule set originally invoked to generate the output.</param>
        void Process(StringBuilder output, NFRuleSet ruleSet);
    }
}
