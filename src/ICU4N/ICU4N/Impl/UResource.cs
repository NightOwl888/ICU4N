using ICU4N.Support.IO;
using ICU4N.Support.Text;
using ICU4N.Util;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace ICU4N.Impl
{
    // Class UResource is named consistently with the public class UResourceBundle,
    // in case we want to make it public at some point.

    /// <summary>
    /// ICU resource bundle key and value types.
    /// </summary>
    public sealed partial class UResource
    {
        /**
         * Represents a resource bundle item's key string.
         * Avoids object creations as much as possible.
         * Mutable, not thread-safe.
         * For permanent storage, use clone() or toString().
         */
        public sealed partial class Key : ICharSequence, IComparable<Key>
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

            /**
             * Constructs an empty resource key string object.
             */
            public Key()
            {
                s = "";
            }

            /**
             * Constructs a resource key object equal to the given string.
             */
            public Key(string s)
            {
                SetString(s);
            }

            private Key(byte[] keyBytes, int keyOffset, int keyLength)
            {
                bytes = keyBytes;
                offset = keyOffset;
                length = keyLength;
            }

            /**
             * Mutates this key for a new NUL-terminated resource key string.
             * The corresponding ASCII-character bytes are not copied and
             * must not be changed during the lifetime of this key
             * (or until the next setBytes() call)
             * and lifetimes of subSequences created from this key.
             *
             * @param keyBytes new key string byte array
             * @param keyOffset new key string offset
             */
            public Key SetBytes(byte[] keyBytes, int keyOffset)
            {
                bytes = keyBytes;
                offset = keyOffset;
                for (length = 0; keyBytes[keyOffset + length] != 0; ++length) { }
                s = null;
                return this;
            }

            /**
             * Mutates this key to an empty resource key string.
             */
            public Key SetToEmpty()
            {
                bytes = null;
                offset = length = 0;
                s = "";
                return this;
            }

            /**
             * Mutates this key to be equal to the given string.
             */
            public Key SetString(string s)
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

            /**
             * {@inheritDoc}
             * Does not clone the byte array.
             */
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

            public int Length
            {
                get { return length; }
            }

            public ICharSequence SubSequence(int start, int end)
            {
                Debug.Assert(0 <= start && start < length);
                Debug.Assert(start <= end && end <= length);
                return new Key(bytes, offset + start, end - start);
            }

            /**
             * Creates/caches/returns this resource key string as a Java String.
             */
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

            /**
             * Creates a new Java String for a sub-sequence of this resource key string.
             */
            public string Substring(int start)
            {
                Debug.Assert(0 <= start && start < length);
                return InternalSubString(start, length);
            }

            /**
             * Creates a new Java String for a sub-sequence of this resource key string.
             */
            public string Substring(int start, int end) // ICU4N TODO: Change 2nd param behavior to be like .NET ?
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
                else if (other is Key)
                {
                    Key otherKey = (Key)other;
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

            public int CompareTo(Key other)
            {
                return CompareTo((ICharSequence)other);
            }

            // ICU4N specific - CompareTo(ICharSequence cs) moved to UResourceExtension.tt
        }

        /**
         * Interface for iterating over a resource bundle array resource.
         * Does not use Java Iterator to reduce object creations.
         */
        public interface IArray
        {
            /**
             * @return The number of items in the array resource.
             */
            int Length { get; }
            /**
             * @param i Array item index.
             * @param value Output-only, receives the value of the i'th item.
             * @return true if i is non-negative and less than getSize().
             */
            bool GetValue(int i, Value value);
        }

        /**
         * Interface for iterating over a resource bundle table resource.
         * Does not use Java Iterator to reduce object creations.
         */
        public interface ITable
        {
            /**
             * @return The number of items in the array resource.
             */
            int Length { get; }
            /**
             * @param i Array item index.
             * @param key Output-only, receives the key of the i'th item.
             * @param value Output-only, receives the value of the i'th item.
             * @return true if i is non-negative and less than getSize().
             */
            bool GetKeyAndValue(int i, Key key, Value value);
        }

        /**
         * Represents a resource bundle item's value.
         * Avoids object creations as much as possible.
         * Mutable, not thread-safe.
         */
        public abstract class Value
        {
            protected Value() { }

            /**
             * @return ICU resource type like {@link UResourceBundle#getType()},
             *     for example, {@link UResourceBundle#STRING}
             */
            public abstract int Type { get; }

            /**
             * @see UResourceBundle#getString()
             * @throws UResourceTypeMismatchException if this is not a string resource
             */
            public abstract string GetString();

            /**
             * @throws UResourceTypeMismatchException if this is not an alias resource
             */
            public abstract string GetAliasString();

            /**
             * @see UResourceBundle#getInt()
             * @throws UResourceTypeMismatchException if this is not an integer resource
             */
            public abstract int GetInt32();

            /**
             * @see UResourceBundle#getUInt()
             * @throws UResourceTypeMismatchException if this is not an integer resource
             */
            public abstract int GetUInt32();

            /**
             * @see UResourceBundle#getIntVector()
             * @throws UResourceTypeMismatchException if this is not an intvector resource
             */
            public abstract int[] GetInt32Vector();

            /**
             * @see UResourceBundle#getBinary()
             * @throws UResourceTypeMismatchException if this is not a binary-blob resource
             */
            public abstract ByteBuffer GetBinary(); // ICU4N TODO: Find an alternative than ByteBuffer for binary data (byte[] ?)

            /**
             * @throws UResourceTypeMismatchException if this is not an array resource
             */
            public abstract IArray GetArray();

            /**
             * @throws UResourceTypeMismatchException if this is not a table resource
             */
            public abstract ITable GetTable();

            /**
             * Is this a no-fallback/no-inheritance marker string?
             * Such a marker is used for CLDR no-fallback data values of "∅∅∅"
             * when enumerating tables with fallback from the specific resource bundle to root.
             *
             * @return true if this is a no-inheritance marker string
             */
            public abstract bool IsNoInheritanceMarker { get; }

            /**
             * @return the array of strings in this array resource.
             * @see UResourceBundle#getStringArray()
             * @throws UResourceTypeMismatchException if this is not an array resource
             *     or if any of the array items is not a string
             */
            public abstract string[] GetStringArray();

            /**
             * Same as
             * <pre>
             * if (getType() == STRING) {
             *     return new String[] { getString(); }
             * } else {
             *     return getStringArray();
             * }
             * </pre>
             *
             * @see #getString()
             * @see #getStringArray()
             * @throws UResourceTypeMismatchException if this is
             *     neither a string resource nor an array resource containing strings
             */
            public abstract string[] GetStringArrayOrStringAsArray();

            /**
             * Same as
             * <pre>
             * if (getType() == STRING) {
             *     return getString();
             * } else {
             *     return getStringArray()[0];
             * }
             * </pre>
             *
             * @see #getString()
             * @see #getStringArray()
             * @throws UResourceTypeMismatchException if this is
             *     neither a string resource nor an array resource containing strings
             */
            public abstract string GetStringOrFirstOfArray();

            /**
             * Only for debugging.
             */
            public override string ToString()
            {
                switch (Type)
                {
                    case UResourceBundle.STRING:
                        return GetString();
                    case UResourceBundle.INT32:
                        return GetInt32().ToString(CultureInfo.InvariantCulture);
                    case UResourceBundle.INT32_VECTOR:
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
                    case UResourceBundle.BINARY:
                        return "(binary blob)";
                    case UResourceBundle.ARRAY:
                        return "(array)";
                    case UResourceBundle.TABLE:
                        return "(table)";
                    default:  // should not occur
                        return "???";
                }
            }
        }

        /**
         * Sink for ICU resource bundle contents.
         */
        public abstract class Sink
        {
            /**
             * Called once for each bundle (child-parent-...-root).
             * The value is normally an array or table resource,
             * and implementations of this method normally iterate over the
             * tree of resource items stored there.
             *
             * @param key Initially the key string of the enumeration-start resource.
             *     Empty if the enumeration starts at the top level of the bundle.
             *     Reuse for output values from Array and Table getters.
             * @param value Call getArray() or getTable() as appropriate.
             *     Then reuse for output values from Array and Table getters.
             * @param noFallback true if the bundle has no parent;
             *     that is, its top-level table has the nofallback attribute,
             *     or it is the root bundle of a locale tree.
             */
            public abstract void Put(Key key, Value value, bool noFallback);
        }
    }
}
