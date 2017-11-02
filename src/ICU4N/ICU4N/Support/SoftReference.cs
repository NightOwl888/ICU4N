using ICU4N.Support.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support
{
    internal class SoftReference<T> : Reference<T> where T : class
    {
        private ReferenceQueue<T> queue;
        private T value;

        public SoftReference(T val)
        {
            this.value = val;
        }

        public SoftReference(T val, ReferenceQueue<T> queue)
        {
            this.value = val;
            this.queue = queue;
        }

        public void Clear()
        {
            this.value = default(T);
        }

        public bool Enqueue()
        {
            if (this.queue == null)
            {
                return false;
            }
            return this.queue.Enqueue(this);
        }

        public override T Get()
        {
            return this.value;
        }
    }
}
