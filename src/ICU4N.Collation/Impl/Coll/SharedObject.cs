using ICU4N.Support;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Impl.Coll
{
    public class SharedObject
#if FEATURE_CLONEABLE
        : ICloneable
#endif
    {
        /**
         * Similar to a smart pointer, basically a port of the static methods of C++ SharedObject.
         */
        public sealed class Reference<T>
#if FEATURE_CLONEABLE
        : ICloneable
#endif
            where T : SharedObject
        {
            private T @ref;

            public Reference(T r)
            {
                @ref = r;
                if (r != null)
                {
                    r.AddRef();
                }
            }

            public object Clone()
            {
                Reference<T> c;
                //try
                //{
                c = (Reference<T>)base.MemberwiseClone();
                //}
                //catch (CloneNotSupportedException e)
                //{
                //    // Should never happen.
                //    throw new ICUCloneNotSupportedException(e);
                //}
                if (@ref != null)
                {
                    @ref.AddRef();
                }
                return c;
            }

            public T ReadOnly { get { return @ref; } }

            /**
             * Returns a writable version of the reference.
             * If there is exactly one owner, then the reference itself is returned.
             * If there are multiple owners, then the reference is replaced with a clone,
             * and that is returned.
             */
            public T CopyOnWrite()
            {
                T r = @ref;
                if (r.RefCount <= 1) { return r; }
                T r2 = (T)r.Clone();
                r.RemoveRef();
                @ref = r2;
                r2.AddRef();
                return r2;
            }

            public void Clear()
            {
                if (@ref != null)
                {
                    @ref.RemoveRef();
                    @ref = null;
                }
            }

            //    protected override void Finalize()
            //    {
            //        super.finalize();
            //        clear();
            //}

            ~Reference()
            {
                Clear();
            }
        }

        /** Initializes refCount to 0. */
        public SharedObject() { }

        /** Initializes refCount to 0. */
        public virtual object Clone()
        {
            SharedObject c;
            //try
            //{
            c = (SharedObject)base.MemberwiseClone();
            //}
            //catch (CloneNotSupportedException e)
            //{
            //    // Should never happen.
            //    throw new ICUCloneNotSupportedException(e);
            //}
            c.refCount = new AtomicInt32();
            return c;
        }

        /**
         * Increments the number of references to this object. Thread-safe.
         */
        public void AddRef() { refCount.IncrementAndGet(); }
        /**
         * Decrements the number of references to this object,
         * and auto-deletes "this" if the number becomes 0. Thread-safe.
         */
        public void RemoveRef()
        {
            // Deletion in Java is up to the garbage collector.
            refCount.DecrementAndGet();
        }

        /**
         * Returns the reference counter. Uses a memory barrier.
         */
        public int RefCount { get { return refCount.Get(); } }

        public void DeleteIfZeroRefCount()
        {
            // Deletion in .NET is up to the garbage collector.
        }

        private AtomicInt32 refCount = new AtomicInt32();
    }
}
