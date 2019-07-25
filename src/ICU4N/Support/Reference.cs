using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support
{
    internal abstract class Reference<T>
    {
        protected Reference()
        {
        }

        public abstract T Get();

        public virtual void Dequeue()
        {
            return;
        }
    }
}
