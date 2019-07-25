namespace ICU4N.Impl
{
    /// <summary>
    /// Base class for cache implementations.
    /// </summary>
    /// <remarks>
    /// To use, instantiate a subclass of a concrete implementation class, where the subclass
    /// implements the <see cref="CreateInstance(TKey, TData)"/> method, and call Get() with the key and the data.
    /// The Get() call will use the data only if it needs to call <see cref="CreateInstance(TKey, TData)"/>,
    /// otherwise the data is ignored.
    /// </remarks>
    /// <typeparam name="TKey">Cache lookup key type.</typeparam>
    /// <typeparam name="TValue">Cache instance value type.</typeparam>
    /// <typeparam name="TData">Data type for creating a new instance value.</typeparam>
    /// <author>Markus Scherer, Mark Davis</author>
    public abstract class CacheBase<TKey, TValue, TData>
    {
        /// <summary>
        /// Retrieves an instance from the cache. Calls <see cref="CreateInstance(TKey, TData)"/> if the cache
        /// does not already contain an instance with this <paramref name="key"/>.
        /// Ignores <paramref name="data"/> if the cache already contains an instance with this <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Cache lookup key for the requested instance.</param>
        /// <param name="data">Data for <see cref="CreateInstance(TKey, TData)"/> if the instance is not already cached.</param>
        /// <returns>The requested instance.</returns>
        public abstract TValue GetInstance(TKey key, TData data);

        /// <summary>
        /// Creates an instance for the <paramref name="key"/> and <paramref name="data"/>. Must be overridden.
        /// </summary>
        /// <param name="key">Cache lookup key for the requested instance.</param>
        /// <param name="data">Data for the instance creation.</param>
        /// <returns>The requested instance.</returns>
        protected abstract TValue CreateInstance(TKey key, TData data);
    }
}
