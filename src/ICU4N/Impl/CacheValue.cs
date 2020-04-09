using ICU4N.Support;
#if FEATURE_MICROSOFT_EXTENSIONS_CACHING
    using Microsoft.Extensions.Caching.Memory;
#else
    using System.Runtime.Caching;
#endif
using System;
using System.Threading;

namespace ICU4N.Impl
{
    /// <summary>
    /// "Strength" of holding a value in CacheValue instances.
    /// The default strength is <see cref="Soft"/>
    /// </summary>
    public enum CacheValueStrength
    {
        /// <summary>
        /// Subsequent <see cref="CacheValue{TValue}.GetInstance(Func{TValue})"/>-created objects
        /// will hold direct references to their values.
        /// </summary>
        Strong = 1,
        /// <summary>
        /// Subsequent <see cref="CacheValue{TValue}.GetInstance(Func{TValue})"/>-created objects
        /// will use <see cref="MemoryCache"/> to hold their values, each with a sliding expiration of 5 minutes.
        /// </summary>
        Soft = 0 // ICU4N specific - assigning value explicitly to 0, the default for value types in .NET
    }

    /// <summary>
    /// Value type for cache items:
    /// Holds a value via a direct reference.
    /// </summary>
    /// <remarks>
    /// The value is immutable.
    /// <para/>
    /// Null values are supported. They can be distinguished from cleared values
    /// via <see cref="IsNull"/>.
    /// </remarks>
    /// <typeparam name="TValue">Cache instance value type.</typeparam>
    public abstract class CacheValue<TValue> where TValue : class
    {
        private static readonly CacheValue<TValue> NULL_VALUE = new NullValue();
        private static volatile CacheValueStrength strength = CacheValueStrength.Soft;

        /// <summary>
        /// Gets or Sets the "strength" of value references for subsequent <see cref="GetInstance(Func{TValue})"/> calls.
        /// </summary>
        public static CacheValueStrength Strength
        {
            get => strength;
            set => strength = value;
        }

        /// <summary>
        /// Returns <c>true</c> if the <see cref="Strength"/> is set to <see cref="CacheValueStrength.Strong"/>.
        /// </summary>
        public static bool FutureInstancesWillBeStrong => strength == CacheValueStrength.Strong;

        /// <summary>
        /// Returns a <see cref="CacheValue{TValue}"/> instance that holds the value.
        /// </summary>
        /// <param name="createValue">A delegate used to create the value. Note this method is called immediately and may be called
        /// periodically if <see cref="Strength"/> is <see cref="CacheValueStrength.Soft"/>.</param>
        /// <returns>A <see cref="CacheValue{TValue}"/> instance that holds the value.</returns>
        public static CacheValue<TValue> GetInstance(Func<TValue> createValue)
        {
            TValue value;
            if (createValue == null || (value = createValue()) is null)
            {
                return NULL_VALUE;
            }
            return strength == CacheValueStrength.Strong ? (CacheValue<TValue>)new StrongValue(value) : new SoftValue(value, createValue);
        }

        /// <summary>
        /// Returns true if this object represents a null value.
        /// </summary>
        public virtual bool IsNull => false;

        /// <summary>
        /// Returns the value (which can be null).
        /// </summary>
        public abstract TValue Get();


        private sealed class NullValue : CacheValue<TValue>
        {
            public override bool IsNull => true;

            public override TValue Get() => default;
        }

        private sealed class StrongValue : CacheValue<TValue>
        {
            private readonly TValue value;

            internal StrongValue(TValue value)
            { this.value = value; }

            public override TValue Get() => value;
        }

        // ICU4N: Utilize ReaderWriterLockSlim to get better throughput on reads
        private sealed class SoftValue : CacheValue<TValue>
        {
            private SoftReference<TValue> reference;
            private readonly ReaderWriterLockSlim syncLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            private readonly Func<TValue> createValue;
            private static readonly TimeSpan SlidingExpiration = new TimeSpan(hours: 0, minutes: 5, seconds: 0);

            public SoftValue(TValue initialValue, Func<TValue> createValue)
            {
                this.createValue = createValue ?? throw new ArgumentNullException(nameof(createValue));
                this.reference = CreateReference(initialValue);
            }

            private SoftReference<TValue> CreateReference(TValue value)
            {
#if FEATURE_MICROSOFT_EXTENSIONS_CACHING
                return new SoftReference<TValue>(value, new MemoryCacheEntryOptions { SlidingExpiration = SlidingExpiration });
#else
                return new SoftReference<TValue>(value, new CacheItemPolicy { SlidingExpiration = SlidingExpiration });
#endif
            }

            public override TValue Get()
            {
                syncLock.EnterUpgradeableReadLock();
                try
                {
                    if (reference.TryGetValue(out TValue value))
                        return value;

                    syncLock.EnterWriteLock();
                    try
                    {
                        // Double check another thread didn't beat us
                        if (reference.TryGetValue(out value))
                            return value;

                        value = createValue();
                        reference = CreateReference(value);
                        return value;
                    }
                    finally
                    {
                        syncLock.ExitWriteLock();
                    }
                }
                finally
                {
                    syncLock.ExitUpgradeableReadLock();
                }
            }
        }
    }
}
