using ICU4N.Text;
using ICU4N.Util;
using J2N.IO;
using J2N.Text;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
#nullable enable

namespace ICU4N.Impl
{
    // ICU resource bundle key and value types.
    // ICU4N port note: These types were originally nested within a UResource class.

    /// <summary>
    /// Represents a resource bundle item's key string.
    /// Avoids object creations as much as possible.
    /// Mutable, not thread-safe.
    /// For permanent storage, use <see cref="Clone()"/> or <see cref="ToString()"/>.
    /// </summary>
    public sealed partial class ResourceKey : IComparable<ResourceKey> // ICU4N: Renamed from Key
#if FEATURE_CLONEABLE
        , ICloneable
#endif
    {
        // Stores a reference to the resource bundle key string bytes array,
        // with an offset of the key, to avoid creating a String object
        // until one is really needed.
        // Alternatively, we could try to always just get the key String object,
        // and cache it in the reader, and see if that performs better or worse.
        private ReadOnlyMemory<byte> bytes;
        private object? bytesReference;
        private string? s;

        /// <summary>
        /// Constructs an empty resource key string object.
        /// </summary>
        public ResourceKey()
        {
            s = string.Empty;
        }

        /// <summary>
        /// Constructs a resource key object equal to the given string.
        /// </summary>
        public ResourceKey(string s)
        {
            SetValue(s);
        }

        /// <summary>
        /// Constructs a resource key object equal to the given string.
        /// </summary>
        public ResourceKey(ReadOnlySpan<char> s)
        {
            SetValue(s);
        }

        /// <summary>
        /// Mutates this key for a new NUL-terminated resource key string.
        /// The corresponding ASCII-character bytes are not copied and
        /// must not be changed during the lifetime of this key
        /// (or until the next <see cref="SetValue(byte[], int)"/> call)
        /// and lifetimes of subSequences created from this key.
        /// </summary>
        /// <param name="bytes">New key string byte array.</param>
        /// <param name="start">New key string offset.</param>
        /// <returns>This.</returns>
        public ResourceKey SetValue(byte[] bytes, int start) // ICU4N: Renamed from SetBytes()
        {
            if (bytes is null)
                throw new ArgumentNullException(nameof(bytes));

            // Find the null terminator
            int length;
            for (length = 0; bytes[start + length] != 0; ++length) { /* intentionally empty */ }
            this.bytes = bytes.AsMemory(start, length);
            this.bytesReference = bytes;
            s = null;
            return this;
        }

        /// <summary>
        /// Mutates this key to an empty resource key string.
        /// </summary>
        /// <returns>This.</returns>
        public ResourceKey SetToEmpty()
        {
            bytes = default;
            s = string.Empty;
            return this;
        }

        /// <summary>
        /// Mutates this key to be equal to the given string.
        /// </summary>
        /// <returns>This.</returns>
        public ResourceKey SetValue(string? s) // ICU4N: Renamed from SetString()
        {
            if (string.IsNullOrEmpty(s))
            {
                SetToEmpty();
            }
            else
            {
                int length = s!.Length;
                var bytes = new byte[length];
                for (int i = 0; i < length; ++i)
                {
                    char c = s[i];
                    if (c <= 0x7f)
                    {
                        bytes[i] = (byte)c;
                    }
                    else
                    {
                        throw new ArgumentException('\"' + s + "\" is not an ASCII string");
                    }
                }
                this.bytes = bytes.AsMemory();
                this.bytesReference = bytes;
                this.s = s;
            }
            return this;
        }

        /// <summary>
        /// Mutates this key to be equal to the given string.
        /// </summary>
        /// <returns>This.</returns>
        public ResourceKey SetValue(ReadOnlySpan<char> s)
        {
            if (s.IsEmpty)
            {
                SetToEmpty();
            }
            else
            {
                int length = s.Length;
                var bytes = new byte[length];
                for (int i = 0; i < length; ++i)
                {
                    char c = s[i];
                    if (c <= 0x7f)
                    {
                        bytes[i] = (byte)c;
                    }
                    else
                    {
                        throw new ArgumentException('\"' + s.ToString() + "\" is not an ASCII string");
                    }
                }
                this.bytes = bytes.AsMemory();
                this.bytesReference = bytes;
                this.s = null;
            }
            return this;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// Does not clone the byte array.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            return base.MemberwiseClone();
        }

        public char this[int i]
        {
            get
            {
                Debug.Assert(0 <= i && i < bytes.Length);
                return (char)bytes.Span[i];
            }
        }

        public int Length => bytes.Length;

        /// <summary>
        /// Creates/caches/returns this resource key string as a .NET <see cref="string"/>.
        /// </summary>
        public override string ToString()
        {
            if (s == null)
            {
                s = InternalSubString(0, Length);
            }
            return s;
        }

        public void CopyTo(int sourceIndex, Span<char> destination, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), SR.Arg_NegativeArgCount);
            }

            if ((uint)sourceIndex > (uint)Length)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex), SR.ArgumentOutOfRange_Index);
            }

            if (sourceIndex > Length - count)
            {
                throw new ArgumentException(SR.Arg_LongerThanSrcString);
            }

            if (destination.Length < count)
            {
                throw new ArgumentException(SR.Arg_LongerThanDestSpan);
            }

            var span = bytes.Slice(sourceIndex, count).Span;
            for (int i = 0; i < count; i++)
            {
                destination[i] = (char)span[i];
            }
        }

        private string InternalSubString(int start, int length)
        {
            int end = length - start;
            const int CharStackBufferSize = 32;
            using ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            var bytesSpan = bytes.Span;
            for (int i = start; i < end; ++i)
            {
                sb.Append((char)bytesSpan[i]);
            }
            return sb.ToString();
        }

        // ICU4N specific - RegionMatches(int start, ICharSequence cs, int n) replacd with SequenceEqual(int sourceIndex, ReadOnlySpan<char> other, int count)

        public override bool Equals(object? other)
        {
            if (other is null)
            {
                return false;
            }
            else if (this == other)
            {
                return true;
            }
            else if (other is ResourceKey otherKey)
            {
                return this.bytes.Span.SequenceEqual(otherKey.bytes.Span);
            }
            else
            {
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SequenceEqual(string other)
        {
            int csLength = other?.Length ?? 0;
            return Length == csLength && SequenceEqual(sourceIndex: 0, other!, count: csLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SequenceEqual(ReadOnlySpan<char> other)
        {
            int csLength = other.Length;
            return Length == csLength && SequenceEqual(sourceIndex: 0, other, count: csLength);
        }

        public bool SequenceEqual(int sourceIndex, string other, int count)
        {
            if (other is null)
                throw new ArgumentNullException(nameof(other));

            return SequenceEqual(sourceIndex, other.AsSpan(), count);
        }

        public bool SequenceEqual(int sourceIndex, ReadOnlySpan<char> other, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), SR.Arg_NegativeArgCount);
            }

            int len = Length;
            if ((uint)sourceIndex > (uint)len)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex), SR.ArgumentOutOfRange_Index);
            }

            if (sourceIndex > len - count)
            {
                throw new ArgumentException(SR.Arg_LongerThanSrcString);
            }

            var bytesSpan = bytes.Span;
            for (int i = 0; i < count; ++i)
            {
                if (bytesSpan[sourceIndex + i] != other[i])
                {
                    return false;
                }
            }
            return true;
        }

        public bool StartsWith(string cs)
        {
            // ICU4N: Added guard clause
            if (cs is null)
                throw new ArgumentNullException(nameof(cs));

            int csLength = cs.Length;
            return csLength <= Length && SequenceEqual(0, cs, csLength);
        }


        public bool StartsWith(ReadOnlySpan<char> cs)
        {
            int csLength = cs.Length;
            return csLength <= Length && SequenceEqual(0, cs, csLength);
        }

        public bool EndsWith(string cs)
        {
            // ICU4N: Added guard clause
            if (cs is null)
                throw new ArgumentNullException(nameof(cs));

            int length = Length;
            int csLength = cs.Length;
            return csLength <= length && SequenceEqual(length - csLength, cs, csLength);
        }


        public bool EndsWith(ReadOnlySpan<char> cs)
        {
            int length = Length;
            int csLength = cs.Length;
            return csLength <= length && SequenceEqual(length - csLength, cs, csLength);
        }

        public override int GetHashCode()
        {
            // Never return s.hashCode(), so that
            // Key.hashCode() is the same whether we have cached s or not.
            int length = bytes.Length;
            if (bytes.Length == 0)
            {
                return 0;
            }

            var bytesSpan = bytes.Span;

            int h = bytesSpan[0];
            for (int i = 1; i < length; ++i)
            {
                h = 37 * h + bytesSpan[i]; // ICU4N: Fixed bug in hashcode calculation (i rather than offset)
            }
            return h;
        }

        public int CompareTo(ResourceKey? other)
        {
            if (other is null) return 1; // ICU4N: Using 1 if other is null as specified here: https://stackoverflow.com/a/4852537

            int length = Length;
            int otherLength = other.Length;
            int minLength = length <= otherLength ? length : otherLength;
            for (int i = 0; i < minLength; ++i)
            {
                int diff = this[i] - other[i];
                if (diff != 0)
                {
                    return diff;
                }
            }
            return length - otherLength;
        }

        public int CompareTo(string? other)
        {
            if (other is null) return 1; // ICU4N: Using 1 if other is null as specified here: https://stackoverflow.com/a/4852537

            return CompareTo(other.AsSpan());
        }

        public int CompareTo(ReadOnlySpan<char> other)
        {
            int length = Length;
            int otherLength = other.Length;
            int minLength = length <= otherLength ? length : otherLength;
            for (int i = 0; i < minLength; ++i)
            {
                int diff = this[i] - other[i];
                if (diff != 0)
                {
                    return diff;
                }
            }
            return length - otherLength;
        }

        private static class SR
        {
            public static string Arg_NegativeArgCount = "Argument count must not be negative.";
            public static string ArgumentOutOfRange_Index = "Index was out of range. Must be non-negative and less than the size of the collection.";
            public static string Arg_LongerThanSrcString = "Source string was not long enough. Check sourceIndex and count.";
            public static string Arg_LongerThanDestSpan = "Destination span was not long enough.";
        }
    }

    /// <summary>
    /// Interface for iterating over a resource bundle array resource.
    /// Does not use .NET enumerator to reduce object creations.
    /// </summary>
    public interface IResourceArray // ICU4N: Renamed from IArray
    {
        /// <summary>
        /// Gets the number of items in the array resource.
        /// </summary>
        int Length { get; }
        /// <summary>
        /// Returns true if i is non-negative and less than <see cref="Length"/>.
        /// </summary>
        /// <param name="i">Array item index.</param>
        /// <param name="value">Output-only, receives the value of the i'th item.</param>
        /// <returns>true if i is non-negative and less than <see cref="Length"/>.</returns>
        bool GetValue(int i, ResourceValue value);
    }

    /// <summary>
    /// Interface for iterating over a resource bundle table resource.
    /// Does not use .NET enumerator to reduce object creations.
    /// </summary>
    public interface IResourceTable // ICU4N: Renamed from ITable
    {
        /// <summary>
        /// Gets the number of items in the table resource.
        /// </summary>
        int Length { get; }
        /// <summary>
        /// Returns true if i is non-negative and less than <see cref="Length"/>.
        /// </summary>
        /// <param name="i">Array item index.</param>
        /// <param name="key">Output-only, receives the key of the i'th item.</param>
        /// <param name="value">Output-only, receives the value of the i'th item.</param>
        /// <returns>true if i is non-negative and less than <see cref="Length"/>.</returns>
        bool GetKeyAndValue(int i, ResourceKey key, ResourceValue value); // ICU4N TODO: API - make output params to make this design more clear ?
    }

    /// <summary>
    /// Represents a resource bundle item's value.
    /// Avoids object creations as much as possible.
    /// Mutable, not thread-safe.
    /// </summary>
    public abstract class ResourceValue // ICU4N: Renamed from Value
    {
        private const int CharStackBufferSize = 32;

        protected ResourceValue() { }

        /// <summary>
        /// Gets ICU resource type like <see cref="UResourceBundle.Type"/>
        /// for example, <see cref="UResourceType.String"/>.
        /// </summary>
        public abstract UResourceType Type { get; }

        /// <seealso cref="UResourceBundle.GetString()"/>
        /// <exception cref="UResourceTypeMismatchException">If this is not a string resource.</exception>
        public abstract string GetString();

        /// <exception cref="UResourceTypeMismatchException">If this is not an alias resource.</exception>
        public abstract string GetAliasString();

        /// <seealso cref="UResourceBundle.GetInt32()"/>
        /// <exception cref="UResourceTypeMismatchException">If this is not an integer resource.</exception>
        public abstract int GetInt32();

        /// <seealso cref="UResourceBundle.GetUInt32()"/>
        /// <exception cref="UResourceTypeMismatchException">If this is not an integer resource.</exception>
        public abstract int GetUInt32();

        /// <seealso cref="UResourceBundle.GetInt32Vector()"/>
        /// <exception cref="UResourceTypeMismatchException">If this is not an intvector resource.</exception>
        public abstract int[] GetInt32Vector();

        /// <seealso cref="UResourceBundle.GetBinary()"/>
        /// <exception cref="UResourceTypeMismatchException">If this is not a binary-blob resource.</exception>
        public abstract ByteBuffer GetBinary(); // ICU4N TODO: API Find an alternative than ByteBuffer for binary data (byte[] ?)

        /// <exception cref="UResourceTypeMismatchException">If this is not an array resource.</exception>
        public abstract IResourceArray GetArray();

        /// <exception cref="UResourceTypeMismatchException">If this is not a table resource.</exception>
        public abstract IResourceTable GetTable();

        /// <summary>
        /// Is this a no-fallback/no-inheritance marker string?
        /// Such a marker is used for CLDR no-fallback data values of "∅∅∅"
        /// when enumerating tables with fallback from the specific resource bundle to root.
        /// Returns true if this is a no-inheritance marker string.
        /// </summary>
        public abstract bool IsNoInheritanceMarker { get; }

        /// <summary>
        /// The array of strings in this array resource.
        /// </summary>
        /// <seealso cref="UResourceBundle.GetStringArray()"/>
        /// <exception cref="UResourceTypeMismatchException">If this is not an array resource
        /// or if any of the array items is not a string.</exception>
        public abstract string[] GetStringArray();

        /// <summary>
        /// Same as
        /// <code>
        /// if (Type == UResourceType.String)
        /// {
        ///     return new string[] { GetString(); }
        /// }
        /// else
        /// {
        ///     return GetStringArray();
        /// }
        /// </code>
        /// </summary>
        /// <seealso cref="GetString()"/>
        /// <seealso cref="GetStringArray()"/>
        /// <exception cref="UResourceTypeMismatchException">If this is
        /// neither a string resource nor an array resource containing strings.</exception>
        public abstract string[] GetStringArrayOrStringAsArray();

        /// <summary>
        /// Same as
        /// <code>
        /// if (Type == UResourceType.String)
        /// {
        ///     return GetString();
        /// }
        /// else
        /// {
        ///     return GetStringArray()[0];
        /// }
        /// </code>
        /// </summary>
        /// <seealso cref="GetString()"/>
        /// <seealso cref="GetStringArray()"/>
        /// <exception cref="UResourceTypeMismatchException">If this is
        /// neither a string resource nor an array resource containing strings.</exception>
        public abstract string GetStringOrFirstOfArray();

        /// <summary>
        /// Only for debugging.
        /// </summary>
        public override string ToString()
        {
            switch (Type)
            {
                case UResourceType.String:
                    return GetString();
                case UResourceType.Int32:
                    return GetInt32().ToString(CultureInfo.InvariantCulture);
                case UResourceType.Int32Vector:
                    int[] iv = GetInt32Vector();
                    ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
                    try
                    {
                        sb.Append("[");
                        sb.Append(iv.Length);
                        sb.Append("]{");
                        if (iv.Length != 0)
                        {
                            sb.Append(iv[0]);
                            for (int i = 1; i < iv.Length; ++i)
                            {
                                sb.Append(", ");
                                sb.Append(iv[i]);
                            }
                        }
                        sb.Append('}');
                        return sb.ToString();
                    }
                    finally
                    {
                        sb.Dispose();
                    }
                case UResourceType.Binary:
                    return "(binary blob)";
                case UResourceType.Array:
                    return "(array)";
                case UResourceType.Table:
                    return "(table)";
                default:  // should not occur
                    return "???";
            }
        }
    }

    /// <summary>
    /// Sink for ICU resource bundle contents.
    /// </summary>
    public abstract class ResourceSink // ICU4N: Renamed from Sink
    {
        /// <summary>
        /// Called once for each bundle (child-parent-...-root).
        /// The value is normally an array or table resource,
        /// and implementations of this method normally iterate over the
        /// tree of resource items stored there.
        /// </summary>
        /// <param name="key">Initially the key string of the enumeration-start resource.
        /// Empty if the enumeration starts at the top level of the bundle.
        /// Reuse for output values from <see cref="IResourceArray"/> and <see cref="IResourceTable"/> getters.</param>
        /// <param name="value">Call <see cref="ResourceValue.GetArray()"/> or <see cref="ResourceValue.GetTable()"/> as appropriate.
        /// Then reuse for output values from <see cref="IResourceArray"/> and <see cref="IResourceTable"/> getters.</param>
        /// <param name="noFallback">true if the bundle has no parent;
        /// that is, its top-level table has the nofallback attribute,
        /// or it is the root bundle of a locale tree.</param>
        public abstract void Put(ResourceKey key, ResourceValue value, bool noFallback);
    }
}
