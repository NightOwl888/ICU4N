using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    /// <author>markdavis</author>
#if FEATURE_SERIALIZABLE
    [Serializable]
#endif
    internal class PluralRulesSerialProxy
    {
        //private static readonly long serialVersionUID = 42L;
        private readonly string data;
        internal PluralRulesSerialProxy(string rules)
        {
            data = rules;
        }
        private object ReadResolve()
        {
            return PluralRules.CreateRules(data);
        }
    }
}
