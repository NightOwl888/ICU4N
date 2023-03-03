using System;

namespace ICU4N.Impl
{
    /// <summary>
    /// Convenience Methods
    /// </summary>
    public static class Pair
    {
        /// <summary>
        /// Creates a pair object.
        /// </summary>
        /// <param name="first">The first object.</param>
        /// <param name="second">The second object.</param>
        /// <returns>The pair object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="first"/> or <paramref name="second"/> is <c>null</c>.</exception>
        public static Pair<TFirst, TSecond> Of<TFirst, TSecond>(TFirst first, TSecond second)
        {
            if (first is null)
                throw new ArgumentNullException(nameof(first));
            if (second is null)
                throw new ArgumentNullException(nameof(second));

            return new Pair<TFirst, TSecond>(first, second);
        }
    }

    /// <typeparam name="TFirst">The first object type.</typeparam>
    /// <typeparam name="TSecond">The second object type.</typeparam>
    public class Pair<TFirst, TSecond>
    {
        private readonly TFirst first;
        private readonly TSecond second;

        public TFirst First => first;
        public TSecond Second => second;

        /// <summary>
        /// Creates a pair object.
        /// </summary>
        /// <param name="first">The first object.</param>
        /// <param name="second">The second object.</param>
        /// <exception cref="ArgumentNullException"><paramref name="first"/> or <paramref name="second"/> is <c>null</c>.</exception>
        protected internal Pair(TFirst first, TSecond second)
        {
            this.first = first ?? throw new ArgumentNullException(nameof(first));
            this.second = second ?? throw new ArgumentNullException(nameof(second));
        }

        public override bool Equals(object other)
        {
            if (other == this)
            {
                return true;
            }
            if (!(other is Pair<TFirst, TSecond> rhs))
            {
                return false;
            }
            return first.Equals(rhs.first) && second.Equals(rhs.second);
        }

        public override int GetHashCode()
        {
            return first.GetHashCode() * 37 + second.GetHashCode();
        }
    }
}
