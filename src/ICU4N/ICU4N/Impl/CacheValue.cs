using ICU4N.Support;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Impl
{
    /// <summary>
    /// "Strength" of holding a value in CacheValue instances.
    /// The default strength is <see cref="Soft"/>
    /// </summary>
    public enum CacheValueStrength
    {
        /// <summary>
        /// Subsequent <c>GetInstance()</c>-created objects
        /// will hold direct references to their values.
        /// </summary>
        Strong,
        /**
         * Subsequent {@code getInstance()}-created objects
         * will hold {@link SoftReference}s to their values.
         */
        /// <summary>
        /// Subsequent <c>GetInstance()</c>-created objects
        /// will hold <see cref="SoftReference"/>{@link SoftReference}s to their values.
        /// </summary>
        Soft
    }

    public abstract class CacheValue<V> where V : class
    {
        private static volatile CacheValueStrength strength = CacheValueStrength.Soft;

        private static readonly CacheValue<V> NULL_VALUE = new NullValue();

        /**
         * Changes the "strength" of value references for subsequent {@code getInstance()} calls.
         */
        public static void SetStrength(CacheValueStrength strength) { CacheValue<V>.strength = strength; }

        /**
         * Returns true if the "strength" is set to {@code STRONG}.
         */
        public static bool FutureInstancesWillBeStrong { get { return strength == CacheValueStrength.Strong; } }

        /**
         * Returns a CacheValue instance that holds the value.
         * It holds it directly if the value is null or if the current "strength" is {@code STRONG}.
         * Otherwise, it holds it via a {@link Reference}.
         */
        public static CacheValue<V> GetInstance(V value)
        {
            if (value == null)
            {
                return NULL_VALUE;
            }
            return strength == CacheValueStrength.Strong ? (CacheValue <V> )new StrongValue(value) : new SoftValue(value);
        }

        /**
         * Distinguishes a null value from a Reference value that has been cleared.
         *
         * @return true if this object represents a null value.
         */
        public virtual bool IsNull { get { return false; } }
        /**
         * Returns the value (which can be null),
         * or null if it was held in a Reference and has been cleared.
         */
        public abstract V Get();
        /**
         * If the value was held via a {@link Reference} which has been cleared,
         * then it is replaced with a new {@link Reference} to the new value,
         * and the new value is returned.
         * The old and new values should be the same or equivalent.
         *
         * <p>Otherwise the old value is returned.
         *
         * @param value Replacement value, for when the current {@link Reference} has been cleared.
         * @return The old or new value.
         */
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
