using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.Text
{
    internal sealed class StringBuilderAppendable : IAppendable
    {
        private readonly StringBuilder stringBuilder;

        internal StringBuilderAppendable(StringBuilder stringBuilder)
        {
            if (stringBuilder == null)
                throw new ArgumentNullException(nameof(stringBuilder));
            this.stringBuilder = stringBuilder;
        }

        public StringBuilder StringBuilder { get { return stringBuilder; } }

        public IAppendable Append(char c)
        {
            stringBuilder.Append(c);
            return this;
        }

        public IAppendable Append(string csq)
        {
            stringBuilder.Append(csq);
            return this;
        }

        public IAppendable Append(string csq, int start, int end)
        {
            stringBuilder.Append(csq, start, end - start);
            return this;
        }

        public IAppendable Append(StringBuilder csq)
        {
            stringBuilder.Append(csq.ToString());
            return this;
        }

        public IAppendable Append(StringBuilder csq, int start, int end)
        {
            stringBuilder.Append(csq.ToString(start, end - start));
            return this;
        }

        public IAppendable Append(char[] csq)
        {
            stringBuilder.Append(csq);
            return this;
        }

        public IAppendable Append(char[] csq, int start, int end)
        {
            stringBuilder.Append(csq, start, end - start);
            return this;
        }

        IAppendable IAppendable.Append(ICharSequence csq)
        {
            stringBuilder.Append(csq);
            return this;
        }

        IAppendable IAppendable.Append(ICharSequence csq, int start, int end)
        {
            stringBuilder.Append(csq, start, end - start);
            return this;
        }

        public override string ToString()
        {
            return stringBuilder.ToString();
        }
    }
}
