﻿using System;
using System.Threading;

namespace ICU4N.Support
{
    /// <summary>
    /// NOTE: This was AtomicInteger in the JDK
    /// </summary>
#if FEATURE_SERIALIZABLE
    [Serializable]
#endif
    public class AtomicInt32
    {
        private int value;

        public AtomicInt32()
            : this(0)
        {
        }

        public AtomicInt32(int value_)
        {
            Interlocked.Exchange(ref value, value_);
        }

        public int IncrementAndGet()
        {
            return Interlocked.Increment(ref value);
        }

        public int GetAndIncrement()
        {
            return Interlocked.Increment(ref value) - 1;
        }

        public int DecrementAndGet()
        {
            return Interlocked.Decrement(ref value);
        }

        public int GetAndDecrement()
        {
            return Interlocked.Decrement(ref value) + 1;
        }

        public void Set(int value)
        {
            Interlocked.Exchange(ref this.value, value);
        }

        public int AddAndGet(int value)
        {
            return Interlocked.Add(ref this.value, value);
        }

        public int Get()
        {
            //ICU4N TODO: read operations atomic in 64 bit
            return value;
        }

        public bool CompareAndSet(int expect, int update)
        {
            int rc = Interlocked.CompareExchange(ref value, update, expect);
            return rc == expect;
        }

        public override string ToString()
        {
            return Get().ToString();
        }
    }
}
