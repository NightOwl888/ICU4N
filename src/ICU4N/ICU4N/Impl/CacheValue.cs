using ICU4N.Support;
using ICU4N.Util;

namespace ICU4N.Impl
{
    /// <summary>
    /// "Strength" of holding a value in CacheValue instances.
    /// The default strength is <see cref="Soft"/>
    /// </summary>
    public enum CacheValueStrength
    {
        /// <summary>
        /// Subsequent <see cref="CacheValue{V}.GetInstance(V)"/>-created objects
        /// will hold direct references to their values.
        /// </summary>
        Strong = 1,
        /// <summary>
        /// Subsequent <c>GetInstance()</c>-created objects
        /// will hold <see cref="Support.SoftReference{T}"/>s to their values.
        /// </summary>
        Soft = 0 // ICU4N specific - assigning value explicitly to 0, the default for value types in .NET
    }

    /// <summary>
    /// Value type for cache items:
    /// Holds a value either via a direct reference or via a <see cref="Reference{T}"/>,
    /// depending on the current "strength" when <see cref="GetInstance(V)"/> was called.
    /// </summary>
    /// <remarks>
    /// The value is <i>conceptually</i> immutable.
    /// If it is held via a direct reference, then it is actually immutable.
    /// <para/>
    /// A <see cref="Reference{T}"/> may be cleared (garbage-collected),
    /// after which <see cref="Get()"/> returns null.
    /// It can then be reset via <see cref="ResetIfCleared(V)"/>.
    /// The new value should be the same as, or equivalent to, the old value.
    /// <para/>
    /// Null values are supported. They can be distinguished from cleared values
    /// via <see cref="IsNull"/>.
    /// </remarks>
    /// <typeparam name="V">Cache instance value type.</typeparam>
    public abstract class CacheValue<V> where V : class
    {
        private static volatile CacheValueStrength strength = CacheValueStrength.Soft;

        private static readonly CacheValue<V> NULL_VALUE = new NullValue();

        /// <summary>
        /// Gets or Sets the "strength" of value references for subsequent <see cref="GetInstance(V)"/> calls.
        /// </summary>
        public static void SetStrength(CacheValueStrength strength) { CacheValue<V>.strength = strength; } // ICU4N TODO: API - make property Strength

        /// <summary>
        /// Returns true if the "strength" is set to <see cref="CacheValueStrength.Strong"/>.
        /// </summary>
        public static bool FutureInstancesWillBeStrong { get { return strength == CacheValueStrength.Strong; } }

        /// <summary>
        /// Returns a <see cref="CacheValue{V}"/> instance that holds the value.
        /// It holds it directly if the value is null or if the current "strength" is <see cref="CacheValueStrength.Strong"/>.
        /// Otherwise, it holds it via a <see cref="Reference{T}"/>.
        /// </summary>
        public static CacheValue<V> GetInstance(V value)
        {
            if (value == null)
            {
                return NULL_VALUE;
            }
            return strength == CacheValueStrength.Strong ? (CacheValue <V> )new StrongValue(value) : new SoftValue(value);
        }

        /// <summary>
        /// Distinguishes a null value from a <see cref="Reference{T}"/> value that has been cleared.
        /// Returns true if this object represents a null value.
        /// </summary>
        public virtual bool IsNull { get { return false; } }

        /// <summary>
        /// Returns the value (which can be null),
        /// or null if it was held in a <see cref="Reference{T}"/> and has been cleared.
        /// </summary>
        public abstract V Get();

        /// <summary>
        /// If the value was held via a <see cref="Reference{T}"/> which has been cleared,
        /// then it is replaced with a new <see cref="Reference{T}"/> to the new value,
        /// and the new value is returned.
        /// The old and new values should be the same or equivalent.
        /// <para/>
        /// Otherwise the old value is returned.
        /// </summary>
        /// <param name="value">Replacement value, for when the current <see cref="Reference{T}"/> has been cleared.</param>
        /// <returns>The old or new value.</returns>
        public abstract V ResetIfCleared(V value);

        private sealed class NullValue : CacheValue<V>
        {
            public override bool IsNull { get { return true; } }

            public override V Get() { return default(V); }

            public override V ResetIfCleared(V value)
            {
                if (value != null)
                {
                    throw new ICUException("resetting a null value to a non-null value");
                }
                return default(V);
            }
        }

        private sealed class StrongValue : CacheValue<V>
        {
            private V value;

            internal StrongValue(V value)
            { this.value = value; }

            public override V Get() { return value; }

            public override V ResetIfCleared(V value)
            {
                // value and this.value should be equivalent, but
                // we do not require equals() to be implemented appropriately.
                return this.value;
            }
        }

        private sealed class SoftValue : CacheValue<V>
        {
            private volatile Reference<V> reference;  // volatile for unsynchronized get()

            internal SoftValue(V value)
            { reference = new SoftReference<V>(value); }

            public override V Get() { return reference.Get(); }

            public override V ResetIfCleared(V value)
            {
                lock (this)
                {
                    V oldValue = reference.Get();
                    if (oldValue == null)
                    {
                        reference = new SoftReference<V>(value);
                        return value;
                    }
                    else
                    {
                        // value and oldValue should be equivalent, but
                        // we do not require equals() to be implemented appropriately.
                        return oldValue;
                    }
                }
            }
        }
    }
}
