using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ICU4N.Support
{
    internal static class IntegerExtensions
    {
        /// <summary>
        /// Converts an <see cref="int"/> into an <see cref="Enum"/> of type <typeparamref name="T"/>.
        /// If the <see cref="Enum"/> defines the <see cref="FlagsAttribute"/>, the <see cref="int"/>
        /// is assumed to be a bitmask that may contain multiple flags. If not, the <see cref="int"/>
        /// must exactly match only one <see cref="Enum"/> symbol's value (not including a symbol 
        /// representing 0), or an <see cref="ArgumentOutOfRangeException"/>is thrown.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="Enum"/>.</typeparam>
        /// <param name="options">An <see cref="int"/> representing one or more symbols in the <see cref="Enum"/>.</param>
        /// <param name="defaultValue">The default to use if <paramref name="options"/> doesn't match any symbols in <typeparamref name="T"/>.</param>
        /// <returns>The symbol or symbols of <typeparamref name="T"/> that are set by <paramref name="options"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <typeparamref name="T"/> doesn't define the 
        /// <see cref="FlagsAttribute"/> and <paramref name="options"/> matches more than one enum symbol.</exception>
        public static T AsFlagsToEnum<T>(this int options, T defaultValue) where T : Enum
        {
#if FEATURE_TYPE_GETCUSTOMATTRIBUTE_GENERIC
            bool isFlagsEnum = typeof(T).GetCustomAttribute<FlagsAttribute>(true) != null;
#else
            bool isFlagsEnum = typeof(T).GetCustomAttributes(true).Any(a => typeof(FlagsAttribute).Equals(a.GetType()));
#endif
            int result = 0;
            bool isSet = false;
            foreach (int option in Enum.GetValues(typeof(T)))
            {
                int temp = result;
                result |= (option & options);
                if (!isFlagsEnum && result != temp)
                {
                    if (!isSet)
                        isSet = true;
                    else
                        throw new ArgumentOutOfRangeException(nameof(options), "A non-[Flags] enum can only be set to a single value, but " +
                            $"'{options.ToString()}' matches {BuildAsFlagsToEnumMultipleValueError<T>(options)}");
                }
            }
            return result > 0 ? (T)(object)result : defaultValue;
        }

        /// <summary>
        /// Converts an <see cref="int"/> into an <see cref="Enum"/> of type <typeparamref name="T"/>.
        /// If the <see cref="Enum"/> defines the <see cref="FlagsAttribute"/>, the <see cref="int"/>
        /// is assumed to be a bitmask that may contain multiple flags. If not, the <see cref="int"/>
        /// must exactly match only one <see cref="Enum"/> symbol's value (not including a symbol 
        /// representing 0), or an <see cref="ArgumentOutOfRangeException"/>is thrown.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="Enum"/>.</typeparam>
        /// <param name="options">An <see cref="int"/> representing one or more symbols in the <see cref="Enum"/>.</param>
        /// <returns>The symbol or symbols of <typeparamref name="T"/> that are set by <paramref name="options"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <typeparamref name="T"/> doesn't define the 
        /// <see cref="FlagsAttribute"/> and <paramref name="options"/> matches more than one enum symbol.</exception>
        public static T AsFlagsToEnum<T>(this int options) where T : Enum
        {
            return AsFlagsToEnum(options, default(T));
        }

        private static string BuildAsFlagsToEnumMultipleValueError<T>(int options)
        {
            var matches = new List<int>();
            foreach (int option in Enum.GetValues(typeof(T)))
            {
                int temp = (option & options);
                if (temp > 0) // Matches if greater than 0
                    matches.Add(temp);
            }
            var sb = new StringBuilder();
            for (int i = 0; i < matches.Count; i++)
            {
                string name = Enum.GetName(typeof(T), matches[i]);
                if (i > 0 && matches.Count > 2)
                    sb.Append(", ");
                else
                    sb.Append(" ");
                if (i == matches.Count - 1 && matches.Count > 1)
                    sb.Append("and ");
                sb.Append(name).Append(" (Numeric value: ").Append(matches[i]).Append(")");
            }
            return sb.ToString();
        }
    }
}
