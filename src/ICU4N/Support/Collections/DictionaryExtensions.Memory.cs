using ICU4N.Impl.Locale;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ICU4N.Support.Collections
{
    internal static partial class DictionaryExtensions
    {
        public static bool ContainsKey<TValue>(this Dictionary<string, TValue> dictionary, ReadOnlySpan<char> key)
        {
            if (dictionary is null)
                throw new ArgumentNullException(nameof(dictionary));

            var comparer = dictionary.Comparer;
            if (!(comparer is AsciiStringComparer ascii))
            {
                throw new NotSupportedException("Only AsciiStringComparer is supported.");
            }

            int hashCode = ascii.GetHashCode(key);
            foreach (string itemKey in dictionary.Keys)
            {
                if (ascii.GetHashCode(itemKey) == hashCode && ascii.Equals(key, itemKey.AsSpan()))
                    return true;
            }
            return false;
        }

        public static bool TryGetValue<TValue>(this Dictionary<string, TValue> dictionary, ReadOnlySpan<char> key, [MaybeNullWhen(false)] out TValue value)
        {
            if (dictionary is null)
                throw new ArgumentNullException(nameof(dictionary));

            var comparer = dictionary.Comparer;
            if (!(comparer is AsciiStringComparer ascii))
            {
                throw new NotSupportedException("Only AsciiStringComparer is supported.");
            }

            int hashCode = ascii.GetHashCode(key);
            foreach (var kvp in dictionary)
            {
                if (ascii.GetHashCode(kvp.Key) == hashCode && ascii.Equals(key, kvp.Key.AsSpan()))
                {
                    value = kvp.Value;
                    return true;
                }
            }
            value = default;
            return false;
        }

        public static bool ContainsKey<TValue>(this Dictionary<AsciiCaseInsensitiveKey, TValue> dictionary, ReadOnlySpan<char> key)
        {
            if (dictionary is null)
                throw new ArgumentNullException(nameof(dictionary));

            int hashCode = AsciiUtil.GetHashCodeOrdinalIgnoreCase(key);
            foreach (AsciiCaseInsensitiveKey itemKey in dictionary.Keys)
            {
                if (itemKey.GetHashCode() == hashCode && itemKey.Equals(key))
                    return true;
            }
            return false;
        }

        public static bool TryGetValue<TValue>(this Dictionary<AsciiCaseInsensitiveKey, TValue> dictionary, ReadOnlySpan<char> key, [MaybeNullWhen(false)] out TValue value)
        {
            if (dictionary is null)
                throw new ArgumentNullException(nameof(dictionary));

            int hashCode = AsciiUtil.GetHashCodeOrdinalIgnoreCase(key);
            foreach (var kvp in dictionary)
            {
                AsciiCaseInsensitiveKey itemKey = kvp.Key;
                if (itemKey.GetHashCode() == hashCode && itemKey.Equals(key))
                {
                    value = kvp.Value;
                    return true;
                }
            }
            value = default;
            return false;
        }
    }
}
