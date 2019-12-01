using ICU4N.Util;
using J2N.IO;
using J2N.Text;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

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
    public sealed partial class ResourceKey : ICharSequence, IComparable<ResourceKey> // ICU4N: Renamed from Key
#if FEATURE_CLONEABLE
        , ICloneable
#endif
    {
        // Stores a reference to the resource bundle key string bytes array,
        // with an offset of the key, to avoid creating a String object
        // until one is really needed.
        // Alternatively, we could try to always just get the key String object,
        // and cache it in the reader, and see if that performs better or worse.
        private byte[] bytes;
        private int offset;
        private int length;
        private string s;

        /// <summary>
        /// Constructs an empty resource key string object.
        /// </summary>
        public ResourceKey()
        {
            s = "";
        }

        /// <summary>
        /// Constructs a resource key object equal to the given string.
        /// </summary>
        public ResourceKey(string s)
        {
            SetString(s);
        }

        private ResourceKey(byte[] keyBytes, int keyOffset, int keyLength)
        {
            bytes = keyBytes;
            offset = keyOffset;
            length = keyLength;
        }

        /// <summary>
        /// Mutates this key for a new NUL-terminated resource key string.
        /// The corresponding ASCII-character bytes are not copied and
        /// must not be changed during the lifetime of this key
        /// (or until the next <see cref="SetBytes(byte[], int)"/> call)
        /// and lifetimes of subSequences created from this key.
        /// </summary>
        /// <param name="keyBytes">New key string byte array.</param>
        /// <param name="keyOffset">New key string offset.</param>
        /// <returns>This.</returns>
        public ResourceKey SetBytes(byte[] keyBytes, int keyOffset)
        {
            bytes = keyBytes;
            offset = keyOffset;
            for (length = 0; keyBytes[keyOffset + length] != 0; ++length) { }
            s = null;
            return this;
        }

        /// <summary>
        /// Mutates this key to an empty resource key string.
        /// </summary>
        /// <returns>This.</returns>
        public ResourceKey SetToEmpty()
        {
            bytes = null;
            offset = length = 0;
            s = "";
            return this;
        }

        /// <summary>
        /// Mutates this key to be equal to the given string.
        /// </summary>
        /// <returns>This.</returns>
        public ResourceKey SetString(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                SetToEmpty();
            }
            else
            {
                bytes = new byte[s.Length];
                offset = 0;
                length = s.Length;
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
                this.s = s;
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
                Debug.Assert(0 <= i && i < length);
                return (char)bytes[offset + i];
            }
        }

        public int Length => length;

        public ICharSequence Subsequence(int startIndex, int length)
        {
            Debug.Assert(0 <= startIndex); // ICU4N: Changed to using length instead of end index
            Debug.Assert(0 <= length);
            return new ResourceKey(bytes, offset + startIndex, length);
        }

        bool ICharSequence.HasValue => s != string.Empty;

        /// <summary>
        /// Creates/caches/returns this resource key string as a .NET <see cref="string"/>.
        /// </summary>
        public override string ToString()
        {
            if (s == null)
            {
                s = InternalSubString(0, length);
            }
            return s;
        }

        private string InternalSubString(int start, int end)
        {
            StringBuilder sb = new StringBuilder(end - start);
            for (int i = start; i < end; ++i)
            {
                sb.Append((char)bytes[offset + i]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Creates a new .NET <see cref="string"/> for a sub-sequence of this resource key string.
        /// </summary>
        public string Substring(int start)
        {
            Debug.Assert(0 <= start && start < length);
            return InternalSubString(start, length);
        }

        /// <summary>
        /// Creates a new .NET <see cref="string"/> for a sub-sequence of this resource key string.
        /// </summary>
        public string Substring(int start, int end) // ICU4N TODO: API Change 2nd param behavior to be like .NET ?
        {
            Debug.Assert(0 <= start && start < length);
            Debug.Assert(start <= end && end <= length);
            return InternalSubString(start, end);
        }

        private bool RegionMatches(byte[] otherBytes, int otherOffset, int n)
        {
            for (int i = 0; i < n; ++i)
            {
                if (bytes[offset + i] != otherBytes[otherOffset + i])
                {
                    return false;
                }
            }
            return true;
        }

        // ICU4N specific - RegionMatches(int start, ICharSequence cs, int n) moved to UResourceExtension.tt

        public override bool Equals(object other)
        {
            if (other == null)
            {
                return false;
            }
            else if (this == other)
            {
                return true;
            }
            else if (other is ResourceKey)
            {
                ResourceKey otherKey = (ResourceKey)other;
                return length == otherKey.length &&
                        RegionMatches(otherKey.bytes, otherKey.offset, length);
            }
            else
            {
                return false;
            }
        }

        // ICU4N specific - ContentEquals(ICharSequence cs) moved to UResourceExtension.tt

        // ICU4N specific - StartsWith(ICharSequence cs) moved to UResourceExtension.tt

        // ICU4N specific - EndsWith(ICharSequence cs) moved to UResourceExtension.tt

        // ICU4N specific - RegionMatches(int start, ICharSequence cs) moved to UResourceExtension.tt

        public override int GetHashCode()
        {
            // Never return s.hashCode(), so that
            // Key.hashCode() is the same whether we have cached s or not.
            if (length == 0)
            {
                return 0;
            }

            int h = bytes[offset];
            for (int i = 1; i < length; ++i)
            {
                h = 37 * h + bytes[offset];
            }
            return h;
        }

        public int CompareTo(ResourceKey other)
        {
            return CompareTo((ICharSequence)other);
        }

        // ICU4N specific - CompareTo(ICharSequence cs) moved to UResourceExtension.tt
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
        bool GetKeyAndValue(int i, ResourceKey key, ResourceValue value);
    }

    /// <summary>
    /// Represents a resource bundle item's value.
    /// Avoids object creations as much as possible.
    /// Mutable, not thread-safe.
    /// </summary>
    public abstract class ResourceValue // ICU4N: Renamed from Value
    {
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
                    StringBuilder sb = new StringBuilder("[");
                    sb.Append(iv.Length).Append("]{");
                    if (iv.Length != 0)
                    {
                        sb.Append(iv[0]);
                        for (int i = 1; i < iv.Length; ++i)
                        {
                            sb.Append(", ").Append(iv[i]);
                        }
                    }
                    return sb.Append('}').ToString();
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
