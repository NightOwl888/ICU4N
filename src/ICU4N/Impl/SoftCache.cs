using System;
using System.Collections.Concurrent;

namespace ICU4N.Impl
{
    /// <summary>
    /// Generic, thread-safe cache implementation.
    /// </summary>
    /// <remarks>
    /// To use, create a long-lived instance (either a static reference or singleton lifestyle), then call
    /// <see cref="GetOrCreate(TKey, Func{TKey, TValue})"/>, which serves as a getter for the cached value.
    /// The second argument is a delegate that is used to create the value when the cache item
    /// is missing.
    /// <para/>
    /// The cache holds on to its dictionary of keys to cache values forever.
    /// <para/>
    /// The value can be <c>null</c> if the <c>valueFactory</c> parameter of <see cref="GetOrCreate(TKey, Func{TKey, TValue})"/>
    /// is <c>null</c> or returns <c>null</c>.
    /// <para/>
    /// The value can either be a direct reference or a <see cref="CacheValue{TValue}"/>. To actually make the cache hold "soft" references,
    /// the return value of <c>valueFactory</c> can be instantiated with <see cref="CacheValue{TValue}.GetInstance(Func{TValue})"/>. If
    /// <see cref="CacheValue{TValue}.Strength"/> is <see cref="CacheValueStrength.Soft"/>, its <see cref="Func{TValue}"/> parameter is not <c>null</c>
    /// and does not return <c>null</c>, the <see cref="CacheValue{TValue}"/> will be a "soft" reference, meaning that its value is kept in
    /// memory for a period of 5 minutes after its last access.
    /// </remarks>
    /// <typeparam name="TKey">Cache lookup key type.</typeparam>
    /// <typeparam name="TValue">Cache instance value type (must not be a <see cref="CacheValue{V}"/>).</typeparam>
    /// <author>Markus Scherer, Mark Davis, Shad Storhaug</author>
    // ICU4N: Refactored to use atomic calls and not require subclasses in order to utilze. Instead, we simply
    // pass a delegate to the GetOrCreate() method that loads the cache value if it was not previously cached.
    public class SoftCache<TKey, TValue> : CacheBase<TKey, TValue> where TValue : class
    {
        private readonly ConcurrentDictionary<TKey, object> map = new ConcurrentDictionary<TKey, object>();

        /// <inheritdoc/>
        public override TValue GetOrCreate(TKey key, Func<TKey, TValue> valueFactory)
        {
            object mapValue = map.GetOrAdd(key, (k) =>
            {
                // We had never cached an instance for this key.

                if (valueFactory is null)
                    return CacheValue<TValue>.GetInstance(null);

                return valueFactory(k) ?? (object)CacheValue<TValue>.GetInstance(null);
            });
            if (!(mapValue is CacheValue<TValue> cv))
            {
                // The value was stored directly.
                return (TValue)mapValue;
            }
            if (cv.IsNull)
            {
                return null;
            }
            return cv.Get();
        }
    }
}
