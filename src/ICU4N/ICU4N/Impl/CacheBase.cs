namespace ICU4N.Impl
{
    /// <summary>
    /// Base class for cache implementations.
    /// </summary>
    /// <remarks>
    /// To use, instantiate a subclass of a concrete implementation class, where the subclass
    /// implements the <see cref="CreateInstance(K, D)"/> method, and call Get() with the key and the data.
    /// The Get() call will use the data only if it needs to call <see cref="CreateInstance(K, D)"/>,
    /// otherwise the data is ignored.
    /// </remarks>
    /// <typeparam name="K">Cache lookup key type.</typeparam>
    /// <typeparam name="V">Cache instance value type.</typeparam>
    /// <typeparam name="D">Data type for creating a new instance value.</typeparam>
    /// <author>Markus Scherer, Mark Davis</author>
    public abstract class CacheBase<K, V, D> // ICU4N TODO: API - rename TKey, TValue, TData
    {
        /// <summary>
        /// Retrieves an instance from the cache. Calls <see cref="CreateInstance(K, D)"/> if the cache
        /// does not already contain an instance with this <paramref name="key"/>.
        /// Ignores <paramref name="data"/> if the cache already contains an instance with this <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Cache lookup key for the requested instance.</param>
        /// <param name="data">Data for <see cref="CreateInstance(K, D)"/> if the instance is not already cached.</param>
        /// <returns>The requested instance.</returns>
        public abstract V GetInstance(K key, D data);

        /// <summary>
        /// Creates an instance for the <paramref name="key"/> and <paramref name="data"/>. Must be overridden.
        /// </summary>
        /// <param name="key">Cache lookup key for the requested instance.</param>
        /// <param name="data">Data for the instance creation.</param>
        /// <returns>The requested instance.</returns>
        protected abstract V CreateInstance(K key, D data);
    }
}
