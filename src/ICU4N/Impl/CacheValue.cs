namespace ICU4N.Impl
{
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
    /// <typeparam name="V">Cache instance value type.</typeparam>
    // ICU4N: Factored out "strength", ResetIfCleared and SoftValue, as there is no way to achieve this in .NET. Instead,
    // all references are always either strong or NULL.
    public abstract class CacheValue<V> where V : class
    {
        private static readonly CacheValue<V> NULL_VALUE = new NullValue();

        /// <summary>
        /// Returns a <see cref="CacheValue{V}"/> instance that holds the value.
        /// </summary>
        public static CacheValue<V> GetInstance(V value)
        {
            if (value == null)
            {
                return NULL_VALUE;
            }
            return new StrongValue(value);
        }

        /// <summary>
        /// Returns true if this object represents a null value.
        /// </summary>
        public virtual bool IsNull => false;

        /// <summary>
        /// Returns the value (which can be null).
        /// </summary>
        public abstract V Get();

        private sealed class NullValue : CacheValue<V>
        {
            public override bool IsNull => true;

            public override V Get() { return default(V); }
        }

        private sealed class StrongValue : CacheValue<V>
        {
            private readonly V value;

            internal StrongValue(V value)
            { this.value = value; }

            public override V Get() { return value; }
        }
    }
}
