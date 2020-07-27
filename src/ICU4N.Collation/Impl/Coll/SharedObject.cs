using System.Threading;

namespace ICU4N.Impl.Coll
{
    /// <summary>
    /// Base class for shared, reference-counted, auto-deleted objects.
    /// .NET subclasses are mutable and must implement <see cref="Clone()"/>.
    ///
    /// <para/>In C++, the SharedObject base class is used for both memory and ownership management.
    /// In .NET, memory management (deletion after last reference is gone)
    /// is up to the garbage collector,
    /// but the reference counter is still used to see whether the referent is the sole owner.
    ///
    /// <para/>Usage:
    /// <code>
    /// public class S : SharedObject
    /// {
    ///     public override void Clone() { ... }
    /// }
    ///
    /// // Either use the nest class Reference (which costs an extra allocation),
    /// // or duplicate its code in the class that uses S
    /// // (which duplicates code and is more error-prone).
    /// public class U
    /// {
    ///     // For read-only access, use s.ReadOnly().
    ///     // For writable access, use S ownedS = s.CopyOnWrite();
    ///     private SharedObject.Reference&lt;S&gt; s;
    ///     // Returns a writable version of s.
    ///     // If there is exactly one owner, then s itself is returned.
    ///     // If there are multiple owners, then s is replaced with a clone,
    ///     // and that is returned.
    ///     private S GetOwnedS()
    ///     {
    ///         return s.CopyOnWrite();
    ///     }
    ///     
    ///     public object Clone()
    ///     {
    ///         ...
    ///         c.s = s.Clone();
    ///         ...
    ///     }
    /// }
    ///
    /// public class V
    /// {
    ///     // For read-only access, use s directly.
    ///     // For writable access, use S ownedS = GetOwnedS();
    ///     private S s;
    ///     // Returns a writable version of s.
    ///     // If there is exactly one owner, then s itself is returned.
    ///     // If there are multiple owners, then s is replaced with a clone,
    ///     // and that is returned.
    ///     private S GetOwnedS()
    ///     {
    ///         if(s.GetRefCount() > 1)
    ///         {
    ///             S ownedS = s.Clone();
    ///             s.RemoveRef();
    ///             s = ownedS;
    ///             ownedS.AddRef();
    ///         }
    ///         return s;
    ///     }
    ///     
    ///     public U Clone()
    ///     {
    ///         ...
    ///         s.AddRef();
    ///         ...
    ///     }
    ///     
    ///     ~V()
    ///     {
    ///         ...
    ///         if(s != null) {
    ///             s.RemoveRef();
    ///             s = null;
    ///         }
    ///         ...
    ///     }
    /// }
    /// </code>
    ///
    /// Either use only .NET memory management, or use AddRef()/RemoveRef().
    /// Sharing requires reference-counting.
    /// </summary>
    // TODO: Consider making this more widely available inside ICU,
    // or else adopting a different model.
    public class SharedObject
#if FEATURE_CLONEABLE
        : ICloneable
#endif
    {
        /// <summary>
        /// Similar to a smart pointer, basically a port of the static methods of C++ SharedObject.
        /// </summary>
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
                Reference<T> c = (Reference<T>)base.MemberwiseClone();
                if (@ref != null)
                {
                    @ref.AddRef();
                }
                return c;
            }

            public T ReadOnly => @ref;

            /// <summary>
            /// Returns a writable version of the reference.
            /// If there is exactly one owner, then the reference itself is returned.
            /// If there are multiple owners, then the reference is replaced with a clone,
            /// and that is returned.
            /// </summary>
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

            ~Reference()
            {
                Clear();
            }
        }

        /// <summary>Initializes refCount to 0.</summary>
        public SharedObject() { }

        /// <summary>Initializes refCount to 0.</summary>
        public virtual object Clone()
        {
            SharedObject c = (SharedObject)base.MemberwiseClone();
            c.refCount = 0;
            return c;
        }

        /// <summary>
        /// Increments the number of references to this object. Thread-safe.
        /// </summary>
        public void AddRef() => Interlocked.Increment(ref refCount); // ICU4N: using Interlocked instead of AtomicInt32

        /// <summary>
        /// Decrements the number of references to this object,
        /// and auto-deletes "this" if the number becomes 0. Thread-safe.
        /// </summary>
        // Deletion in .NET is up to the garbage collector.
        public void RemoveRef() => Interlocked.Decrement(ref refCount); // ICU4N: using Interlocked instead of AtomicInt32

        /// <summary>
        /// Returns the reference counter. Uses a memory barrier.
        /// </summary>
        public int RefCount => Interlocked.CompareExchange(ref refCount, 0, 0); // ICU4N: using Interlocked instead of AtomicInt32

        public void DeleteIfZeroRefCount()
        {
            // Deletion in .NET is up to the garbage collector.
        }

        private int refCount; // ICU4N: using Interlocked instead of AtomicInt32
    }
}
