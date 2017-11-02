using ICU4N.Support.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.Text
{
    // from Apache Harmony

    internal interface IAppendable
    {
        IAppendable Append(char c);

        IAppendable Append(string csq);

        IAppendable Append(string csq, int start, int end);

        IAppendable Append(StringBuilder csq);

        IAppendable Append(StringBuilder csq, int start, int end);

        IAppendable Append(char[] csq);

        IAppendable Append(char[] csq, int start, int end);

        IAppendable Append(ICharSequence csq);

        IAppendable Append(ICharSequence csq, int start, int end);
    }
}
