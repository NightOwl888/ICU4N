using System;

namespace ICU4N.Impl
{
    /// <summary>
    /// Base class for cache implementations.
    /// </summary>
    /// <remarks>
    /// To use, instantiate a subclass of a concrete implementation class, where the subclass
    /// implements the <see cref="GetOrCreate(TKey, Func{TKey, TValue})"/> method.
    /// The <see cref="GetOrCreate(TKey, Func{TKey, TValue})"/> call will use the <c>valueFactory</c>
    /// only if the value doesn't already exist, otherwise the parameter is ignored.
    /// </remarks>
    /// <typeparam name="TKey">Cache lookup key type.</typeparam>
    /// <typeparam name="TValue">Cache instance value type.</typeparam>
    /// <author>Markus Scherer, Mark Davis, Shad Storhaug</author>
    public abstract class CacheBase<TKey, TValue>
    {
        /// <summary>
        /// Retrieves an instance from the cache. Calls <paramref name="valueFactory"/> if the cache does
        /// not already contain an instance with this <paramref name="key"/>.
        /// Ignores <paramref name="valueFactory"/> if the cache already contains an instance with this <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Cache lookup key for the requested instance.</param>
        /// <param name="valueFactory">Delegate to invoke if the instance is not already cached.</param>
        /// <returns>The value that was retrieved from the cache, or the newly created value returned from <paramref name="valueFactory"/>.</returns>
        public abstract TValue GetOrCreate(TKey key, Func<TKey, TValue> valueFactory);
    }
}
