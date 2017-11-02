using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Impl
{
    public abstract class CacheBase<K, V, D>
    {
        public abstract V GetInstance(K key, D data);

        protected abstract V CreateInstance(K key, D data);
    }
}
