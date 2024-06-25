using ICU4N.Globalization;
using ICU4N.Support;
using ICU4N.Util;
using J2N;
using J2N.Collections.Concurrent;
using J2N.IO;
using J2N.Numerics;
using J2N.Text;
#if FEATURE_MICROSOFT_EXTENSIONS_CACHING
    using Microsoft.Extensions.Caching.Memory;
#else
    using System.Runtime.Caching;
#endif
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ICU4N.Impl
{
    /// <summary>
    /// This class reads the *.res resource bundle format.
    /// <para/>
    /// For the file format documentation see ICU4C's source/common/uresdata.h file.
    /// </summary>
    public sealed class ICUResourceBundleReader
    {
        /// <summary>
        /// File format version that this class understands.
        /// "ResB"
        /// </summary>
        private const int DATA_FORMAT = 0x52657342;
        private sealed class IsAcceptable : IAuthenticate
        {
            public bool IsDataVersionAcceptable(byte[] formatVersion)
            {
                return
                        (formatVersion[0] == 1 && (formatVersion[1] & 0xff) >= 1) ||
                        (2 <= formatVersion[0] && formatVersion[0] <= 3);
            }
        }
        private static readonly IsAcceptable IS_ACCEPTABLE = new IsAcceptable();

        /* indexes[] value names; indexes are generally 32-bit (Resource) indexes */
        /// <summary>
        /// [0] contains the length of indexes[]
        /// which is at most URES_INDEX_TOP of the latest format version
        /// formatVersion==1: all bits contain the length of indexes[]
        ///   but the length is much less than 0xff;
        /// formatVersion>1:
        ///   only bits  7..0 contain the length of indexes[],
        ///        bits 31..8 are reserved and set to 0
        /// formatVersion>=3:
        ///        bits 31..8 poolStringIndexLimit bits 23..0
        /// </summary>
        private const int URES_INDEX_LENGTH = 0;
        /// <summary>
        /// [1] contains the top of the key strings,
        ///     same as the bottom of resources or UTF-16 strings, rounded up
        /// </summary>
        private const int URES_INDEX_KEYS_TOP = 1;
        ///// <summary>[2] contains the top of all resources</summary>
        //private static readonly int URES_INDEX_RESOURCES_TOP    = 2;
        /// <summary>
        /// [3] contains the top of the bundle,
        ///     in case it were ever different from [2]
        /// </summary>
        private const int URES_INDEX_BUNDLE_TOP = 3;
        /// <summary>[4] max. length of any table</summary>
        private const int URES_INDEX_MAX_TABLE_LENGTH = 4;
        /// <summary>
        /// [5] attributes bit set, see URES_ATT_* (new in formatVersion 1.2)
        /// <para/>
        /// formatVersion>=3:
        ///   bits 31..16 poolStringIndex16Limit
        ///   bits 15..12 poolStringIndexLimit bits 27..24
        /// </summary>
        private const int URES_INDEX_ATTRIBUTES = 5;
        /// <summary>
        /// [6] top of the 16-bit units (UTF-16 string v2 UChars, URES_TABLE16, URES_ARRAY16),
        ///     rounded up (new in formatVersion 2.0, ICU 4.4)
        /// </summary>
        private const int URES_INDEX_16BIT_TOP = 6;
        /// <summary>[7] checksum of the pool bundle (new in formatVersion 2.0, ICU 4.4)</summary>
        private const int URES_INDEX_POOL_CHECKSUM = 7;
        //private const int URES_INDEX_TOP              = 8;

        /// <summary>
        /// Nofallback attribute, attribute bit 0 in indexes[URES_INDEX_ATTRIBUTES].
        /// New in formatVersion 1.2 (ICU 3.6).
        /// <para/>
        /// If set, then this resource bundle is a standalone bundle.
        /// If not set, then the bundle participates in locale fallback, eventually
        /// all the way to the root bundle.
        /// If indexes[] is missing or too short, then the attribute cannot be determined
        /// reliably. Dependency checking should ignore such bundles, and loading should
        /// use fallbacks.
        /// </summary>
        private const int URES_ATT_NO_FALLBACK = 1;
        /// <summary>
        /// Attributes for bundles that are, or use, a pool bundle.
        /// A pool bundle provides key strings that are shared among several other bundles
        /// to reduce their total size.
        /// New in formatVersion 2 (ICU 4.4).
        /// </summary>
        private const int URES_ATT_IS_POOL_BUNDLE = 2;
        private const int URES_ATT_USES_POOL_BUNDLE = 4;

        private static readonly CharBuffer EMPTY_16_BIT_UNITS = CharBuffer.Wrap(new char[] { '\0' });  // read-only

        /// <summary>
        /// Objects are always cached, and we are relying on virtual memory to manage the memory consumption
        /// rather than having a memory-sensitive cache. However, we are leaving this constant for parity with
        /// ICU4J.
        /// </summary>
        internal const int LARGE_SIZE = 24;

        private static readonly bool DEBUG = false;

        private int /* formatVersion, */ dataVersion;

        // See the ResourceData struct in ICU4C/source/common/uresdata.h.
        /// <summary>
        /// Buffer of all of the resource bundle bytes after the header.
        /// (equivalent of C++ pRoot)
        /// </summary>
        private ByteBuffer bytes;
        private byte[] keyBytes;
        private CharBuffer b16BitUnits;
        private ICUResourceBundleReader poolBundleReader;
        private int rootRes;
        private int localKeyLimit;
        private int poolStringIndexLimit;
        private int poolStringIndex16Limit;
        private bool noFallback; /* see URES_ATT_NO_FALLBACK */
        private bool isPoolBundle;
        private bool usesPoolBundle;
        private int poolCheckSum;

        private ResourceCache resourceCache;

        private static readonly SoftCache<ReaderCacheKey, ICUResourceBundleReader> CACHE = new SoftCache<ReaderCacheKey, ICUResourceBundleReader>();
        private static readonly ICUResourceBundleReader NULL_READER = new ICUResourceBundleReader();

        private class ReaderCacheKey
        {
            internal readonly string baseName;
            internal readonly string localeID;

            internal ReaderCacheKey(string baseName, string localeID)
            {
                this.baseName = (baseName == null) ? "" : baseName;
                this.localeID = (localeID == null) ? "" : localeID;
            }

            public override bool Equals(object obj)
            {
                if (this == obj)
                {
                    return true;
                }
                if (!(obj is ReaderCacheKey info))
                {
                    return false;
                }
                return this.baseName.Equals(info.baseName)
                        && this.localeID.Equals(info.localeID);
            }

            public override int GetHashCode()
            {
                return baseName.GetHashCode() ^ localeID.GetHashCode();
            }
        }

        // ICU4N: Factored out ReaderCache and changed to GetOrCreate() method that
        // uses a delegate to do all of this inline.

        /// <summary>
        /// Default constructor, just used for <see cref="NULL_READER"/>.
        /// </summary>
        private ICUResourceBundleReader()
        {
        }

        private ICUResourceBundleReader(ByteBuffer inBytes,
            string baseName, string localeID,
            Assembly assembly)
        {
            Init(inBytes);

            // set pool bundle if necessary
            if (usesPoolBundle)
            {
                poolBundleReader = GetReader(baseName, "pool", assembly);
                if (poolBundleReader == null || !poolBundleReader.isPoolBundle)
                {
                    throw new InvalidOperationException("pool.res is not a pool bundle");
                }
                if (poolBundleReader.poolCheckSum != poolCheckSum)
                {
                    throw new InvalidOperationException("pool.res has a different checksum than this bundle");
                }
            }
        }

        internal static ICUResourceBundleReader GetReader(string baseName, string localeID, Assembly root)
        {
            ReaderCacheKey info = new ReaderCacheKey(baseName, localeID);
            ICUResourceBundleReader reader = CACHE.GetOrCreate(info, (key) =>
            {
                string fullName = ICUResourceBundleReader.GetFullName(key.baseName, key.localeID);
                try
                {
                    ByteBuffer inBytes;
                    if (key.baseName != null && key.baseName.StartsWith(ICUData.IcuBaseName, StringComparison.Ordinal))
                    {
                        string itemPath = fullName.Substring(ICUData.IcuBaseName.Length + 1);
                        inBytes = ICUBinary.GetData(root, fullName, itemPath);
                        if (inBytes == null)
                        {
                            return NULL_READER;
                        }
                    }
                    else
                    {
                        // Closed by getByteBufferFromInputStreamAndCloseStream().
                        Stream stream = ICUData.GetStream(root, fullName);
                        if (stream == null)
                        {
                            return NULL_READER;
                        }
                        inBytes = ICUBinary.GetByteBufferFromStreamAndDisposeStream(stream);
                    }
                    return new ICUResourceBundleReader(inBytes, key.baseName, key.localeID, root);
                }
                catch (IOException ex)
                {
                    throw new ICUUncheckedIOException("Data file " + fullName + " is corrupt - " + ex.Message, ex);
                }
            });
            if (reader == NULL_READER)
            {
                return null;
            }
            return reader;
        }

        // See res_init() in ICU4C/source/common/uresdata.c.
        private void Init(ByteBuffer inBytes)
        {
            dataVersion = ICUBinary.ReadHeader(inBytes, DATA_FORMAT, IS_ACCEPTABLE);
            int majorFormatVersion = inBytes.Get(16);
            bytes = ICUBinary.SliceWithOrder(inBytes);
            int dataLength = bytes.Remaining;

            //if (DEBUG) Console.Out.WriteLine("The ByteBuffer is direct (memory-mapped): " + bytes.IsDirect); // ICU4N: IsDirect not supported by J2N
            if (DEBUG) Console.Out.WriteLine("The available bytes in the buffer before reading the data: " + dataLength);

            rootRes = bytes.GetInt32(0);

            // Bundles with formatVersion 1.1 and later contain an indexes[] array.
            // We need it so that we can read the key string bytes up front, for lookup performance.

            // read the variable-length indexes[] array
            int indexes0 = GetIndexesInt(URES_INDEX_LENGTH);
            int indexLength = indexes0 & 0xff;
            if (indexLength <= URES_INDEX_MAX_TABLE_LENGTH)
            {
                throw new ICUException("not enough indexes");
            }
            int bundleTop;
            if (dataLength < ((1 + indexLength) << 2) ||
                    dataLength < ((bundleTop = GetIndexesInt(URES_INDEX_BUNDLE_TOP)) << 2))
            {
                throw new ICUException("not enough bytes");
            }
            int maxOffset = bundleTop - 1;

            if (majorFormatVersion >= 3)
            {
                // In formatVersion 1, the indexLength took up this whole int.
                // In version 2, bits 31..8 were reserved and always 0.
                // In version 3, they contain bits 23..0 of the poolStringIndexLimit.
                // Bits 27..24 are in indexes[URES_INDEX_ATTRIBUTES] bits 15..12.
                poolStringIndexLimit = indexes0.TripleShift(8);
            }
            if (indexLength > URES_INDEX_ATTRIBUTES)
            {
                // determine if this resource bundle falls back to a parent bundle
                // along normal locale ID fallback
                int att = GetIndexesInt(URES_INDEX_ATTRIBUTES);
                noFallback = (att & URES_ATT_NO_FALLBACK) != 0;
                isPoolBundle = (att & URES_ATT_IS_POOL_BUNDLE) != 0;
                usesPoolBundle = (att & URES_ATT_USES_POOL_BUNDLE) != 0;
                poolStringIndexLimit |= (att & 0xf000) << 12;  // bits 15..12 -> 27..24
                poolStringIndex16Limit = att.TripleShift(16);
            }

            int keysBottom = 1 + indexLength;
            int keysTop = GetIndexesInt(URES_INDEX_KEYS_TOP);
            if (keysTop > keysBottom)
            {
                // Deserialize the key strings up front.
                // Faster table item search at the cost of slower startup and some heap memory.
                if (isPoolBundle)
                {
                    // Shift the key strings down:
                    // Pool bundle key strings are used with a 0-based index,
                    // unlike regular bundles' key strings for which indexes
                    // are based on the start of the bundle data.
                    keyBytes = new byte[(keysTop - keysBottom) << 2];
                    bytes.Position = (keysBottom << 2);
                }
                else
                {
                    localKeyLimit = keysTop << 2;
                    keyBytes = new byte[localKeyLimit];
                }
                bytes.Get(keyBytes);
            }

            // Read the array of 16-bit units.
            if (indexLength > URES_INDEX_16BIT_TOP)
            {
                int _16BitTop = GetIndexesInt(URES_INDEX_16BIT_TOP);
                if (_16BitTop > keysTop)
                {
                    int num16BitUnits = (_16BitTop - keysTop) * 2;
                    bytes.Position = (keysTop << 2);
                    b16BitUnits = bytes.AsCharBuffer();
                    b16BitUnits.Limit = num16BitUnits;
                    maxOffset |= num16BitUnits - 1;
                }
                else
                {
                    b16BitUnits = EMPTY_16_BIT_UNITS;
                }
            }
            else
            {
                b16BitUnits = EMPTY_16_BIT_UNITS;
            }

            if (indexLength > URES_INDEX_POOL_CHECKSUM)
            {
                poolCheckSum = GetIndexesInt(URES_INDEX_POOL_CHECKSUM);
            }

            if (!isPoolBundle || b16BitUnits.Length > 1)
            {
                resourceCache = new ResourceCache(maxOffset);
            }

            // Reset the position for future .asCharBuffer() etc.
            bytes.Position = 0;
        }

        private int GetIndexesInt(int i)
        {
            return bytes.GetInt32((1 + i) << 2);
        }

        internal VersionInfo Version => ICUBinary.GetVersionInfoFromCompactInt32(dataVersion);

        internal int RootResource => rootRes;

        internal bool NoFallback => noFallback;

        internal bool UsesPoolBundle => usesPoolBundle;

        internal static UResourceType RES_GET_TYPE(int res)
        {
            return (UResourceType)res.TripleShift(28);
        }
        private static int RES_GET_OFFSET(int res)
        {
            return res & 0x0fffffff;
        }
        private int GetResourceByteOffset(int offset)
        {
            return offset << 2;
        }
        /// <summary>get signed and unsigned integer values directly from the Resource handle</summary>
        internal static int RES_GET_INT(int res)
        {
            return (res << 4) >> 4;
        }
        internal static int RES_GET_UINT(int res) // ICU4N TODO: API Should this actually return uint?
        {
            return res & 0x0fffffff;
        }
        internal static bool URES_IS_ARRAY(UResourceType type)
        {
            return type == UResourceType.Array || type == UResourceType.Array16;
        }
        internal static bool URES_IS_TABLE(UResourceType type)
        {
            return type == UResourceType.Table || type == UResourceType.Table16 || type == UResourceType.Table32;
        }

        private static readonly byte[] emptyBytes = new byte[0];
        private static readonly ByteBuffer emptyByteBuffer = ByteBuffer.Allocate(0).AsReadOnlyBuffer();
        private static readonly char[] emptyChars = new char[0];
        private static readonly int[] emptyInts = new int[0];
        private static readonly string emptyString = "";
        private static readonly Array EMPTY_ARRAY = new Array();
        private static readonly Table EMPTY_TABLE = new Table();

        private char[] GetChars(int offset, int count)
        {
            char[] chars = new char[count];
            if (count <= 16)
            {
                for (int i = 0; i < count; offset += 2, ++i)
                {
                    chars[i] = bytes.GetChar(offset);
                }
            }
            else
            {
                CharBuffer temp = bytes.AsCharBuffer();
                temp.Position = (offset / 2);
                temp.Get(chars);
            }
            return chars;
        }
        private int GetInt32(int offset)
        {
            return bytes.GetInt32(offset);
        }
        private int[] GetInt32s(int offset, int count)
        {
            int[] ints = new int[count];
            if (count <= 16)
            {
                for (int i = 0; i < count; offset += 4, ++i)
                {
                    ints[i] = bytes.GetInt32(offset);
                }
            }
            else
            {
                Int32Buffer temp = bytes.AsInt32Buffer();
                temp.Position = (offset / 4);
                temp.Get(ints);
            }
            return ints;
        }
        private char[] GetTable16KeyOffsets(int offset)
        {
            int length = b16BitUnits[offset++];
            if (length > 0)
            {
                char[] result = new char[length];
                if (length <= 16)
                {
                    for (int i = 0; i < length; ++i)
                    {
                        result[i] = b16BitUnits[offset++];
                    }
                }
                else
                {
                    CharBuffer temp = b16BitUnits.Duplicate();
                    temp.Position = offset;
                    temp.Get(result);
                }
                return result;
            }
            else
            {
                return emptyChars;
            }
        }
        private char[] GetTableKeyOffsets(int offset)
        {
            int length = bytes.GetChar(offset);
            if (length > 0)
            {
                return GetChars(offset + 2, length);
            }
            else
            {
                return emptyChars;
            }
        }
        private int[] GetTable32KeyOffsets(int offset)
        {
            int length = GetInt32(offset);
            if (length > 0)
            {
                return GetInt32s(offset + 4, length);
            }
            else
            {
                return emptyInts;
            }
        }

        private static string MakeKeyStringFromBytes(byte[] keyBytes, int keyOffset)
        {
            StringBuilder sb = new StringBuilder();
            byte b;
            while ((b = keyBytes[keyOffset]) != 0)
            {
                ++keyOffset;
                sb.Append((char)b);
            }
            return sb.ToString();
        }
        private string GetKey16String(int keyOffset)
        {
            if (keyOffset < localKeyLimit)
            {
                return MakeKeyStringFromBytes(keyBytes, keyOffset);
            }
            else
            {
                return MakeKeyStringFromBytes(poolBundleReader.keyBytes, keyOffset - localKeyLimit);
            }
        }
        private string GetKey32String(int keyOffset)
        {
            if (keyOffset >= 0)
            {
                return MakeKeyStringFromBytes(keyBytes, keyOffset);
            }
            else
            {
                return MakeKeyStringFromBytes(poolBundleReader.keyBytes, keyOffset & 0x7fffffff);
            }
        }
        private void SetKeyFromKey16(int keyOffset, ResourceKey key)
        {
            if (keyOffset < localKeyLimit)
            {
                key.SetBytes(keyBytes, keyOffset);
            }
            else
            {
                key.SetBytes(poolBundleReader.keyBytes, keyOffset - localKeyLimit);
            }
        }
        private void SetKeyFromKey32(int keyOffset, ResourceKey key)
        {
            if (keyOffset >= 0)
            {
                key.SetBytes(keyBytes, keyOffset);
            }
            else
            {
                key.SetBytes(poolBundleReader.keyBytes, keyOffset & 0x7fffffff);
            }
        }
        private int CompareKeys(ReadOnlySpan<char> key, char keyOffset)
        {
            if (keyOffset < localKeyLimit)
            {
                return ICUBinary.CompareKeys(key, keyBytes, keyOffset);
            }
            else
            {
                return ICUBinary.CompareKeys(key, poolBundleReader.keyBytes, keyOffset - localKeyLimit);
            }
        }
        private int CompareKeys32(ReadOnlySpan<char> key, int keyOffset)
        {
            if (keyOffset >= 0)
            {
                return ICUBinary.CompareKeys(key, keyBytes, keyOffset);
            }
            else
            {
                return ICUBinary.CompareKeys(key, poolBundleReader.keyBytes, keyOffset & 0x7fffffff);
            }
        }

        /// <returns>A string from the local bundle's b16BitUnits at the local offset.</returns>
        internal string GetStringV2(int res)
        {
            // Use the pool bundle's resource cache for pool bundle strings;
            // use the local bundle's cache for local strings.
            // The cache requires a resource word with the proper type,
            // and with an offset that is local to this bundle so that the offset fits
            // within the maximum number of bits for which the cache was constructed.
            Debug.Assert(RES_GET_TYPE(res) == UResourceType.StringV2);
            int offset = RES_GET_OFFSET(res);
            Debug.Assert(offset != 0);  // handled by the caller
            object value = resourceCache.Get(res);
            if (value != null)
            {
                return (string)value;
            }
            string s;
            int first = b16BitUnits[offset];
            if ((first & 0xfffffc00) != 0xdc00)
            {  // C: if(!U16_IS_TRAIL(first)) {
                if (first == 0)
                {
                    return emptyString;  // Should not occur, but is not forbidden.
                }
                StringBuilder sb = new StringBuilder();
                sb.Append((char)first);
                char c;
                while ((c = b16BitUnits[++offset]) != 0)
                {
                    sb.Append(c);
                }
                s = sb.ToString();
            }
            else
            {
                int length;
                if (first < 0xdfef)
                {
                    length = first & 0x3ff;
                    ++offset;
                }
                else if (first < 0xdfff)
                {
                    length = ((first - 0xdfef) << 16) | b16BitUnits[offset + 1];
                    offset += 2;
                }
                else
                {
                    length = (b16BitUnits[offset + 1] << 16) | b16BitUnits[offset + 2];
                    offset += 3;
                }
                // Cast up to CharSequence to insulate against the CharBuffer.subSequence() return type change
                // which makes code compiled for a newer JDK (7 and up) not run on an older one (6 and below).
                s = ((ICharSequence)b16BitUnits).Subsequence(offset, length).ToString(); // ICU4N: Corrected 2nd parameter
            }
            return (string)resourceCache.GetOrAdd(res, s, s.Length * 2);
        }

        private string MakeStringFromBytes(int offset, int length)
        {
            if (length <= 16)
            {
                StringBuilder sb = new StringBuilder(length);
                for (int i = 0; i < length; offset += 2, ++i)
                {
                    sb.Append(bytes.GetChar(offset));
                }
                return sb.ToString();
            }
            else
            {
                ICharSequence cs = bytes.AsCharBuffer();
                offset /= 2;
                return cs.Subsequence(offset, length).ToString(); // ICU4N: Corrected 2nd parameter
            }
        }

        internal string GetString(int res)
        {
            int offset = RES_GET_OFFSET(res);
            if (res != offset /* RES_GET_TYPE(res) != URES_STRING */ &&
                    RES_GET_TYPE(res) != UResourceType.StringV2)
            {
                return null;
            }
            if (offset == 0)
            {
                return emptyString;
            }
            if (res != offset)
            {  // STRING_V2
                if (offset < poolStringIndexLimit)
                {
                    return poolBundleReader.GetStringV2(res);
                }
                else
                {
                    return GetStringV2(res - poolStringIndexLimit);
                }
            }
            object value = resourceCache.Get(res);
            if (value != null)
            {
                return (string)value;
            }
            offset = GetResourceByteOffset(offset);
            int length = GetInt32(offset);
            string s = MakeStringFromBytes(offset + 4, length);
            return (string)resourceCache.GetOrAdd(res, s, s.Length * 2);
        }

        /// <summary>
        /// CLDR string value "∅∅∅"=="\u2205\u2205\u2205" prevents fallback to the parent bundle.
        /// </summary>
        private bool IsNoInheritanceMarker(int res)
        {
            int offset = RES_GET_OFFSET(res);
            if (offset == 0)
            {
                // empty string
            }
            else if (res == offset)
            {
                offset = GetResourceByteOffset(offset);
                return GetInt32(offset) == 3 && bytes.GetChar(offset + 4) == 0x2205 &&
                        bytes.GetChar(offset + 6) == 0x2205 && bytes.GetChar(offset + 8) == 0x2205;
            }
            else if (RES_GET_TYPE(res) == UResourceType.StringV2)
            {
                if (offset < poolStringIndexLimit)
                {
                    return poolBundleReader.IsStringV2NoInheritanceMarker(offset);
                }
                else
                {
                    return IsStringV2NoInheritanceMarker(offset - poolStringIndexLimit);
                }
            }
            return false;
        }

        private bool IsStringV2NoInheritanceMarker(int offset)
        {
            int first = b16BitUnits[offset];
            if (first == 0x2205)
            {  // implicit length
                return b16BitUnits[offset + 1] == 0x2205 &&
                        b16BitUnits[offset + 2] == 0x2205 &&
                        b16BitUnits[offset + 3] == 0;
            }
            else if (first == 0xdc03)
            {  // explicit length 3 (should not occur)
                return b16BitUnits[offset + 1] == 0x2205 &&
                        b16BitUnits[offset + 2] == 0x2205 &&
                        b16BitUnits[offset + 3] == 0x2205;
            }
            else
            {
                // Assume that the string has not been stored with more length units than necessary.
                return false;
            }
        }

        internal string GetAlias(int res)
        {
            int offset = RES_GET_OFFSET(res);
            int length;
            if (RES_GET_TYPE(res) == UResourceType.Alias)
            {
                if (offset == 0)
                {
                    return emptyString;
                }
                else
                {
                    object value = resourceCache.Get(res);
                    if (value != null)
                    {
                        return (string)value;
                    }
                    offset = GetResourceByteOffset(offset);
                    length = GetInt32(offset);
                    string s = MakeStringFromBytes(offset + 4, length);
                    return (string)resourceCache.GetOrAdd(res, s, length * 2);
                }
            }
            else
            {
                return null;
            }
        }

        internal byte[] GetBinary(int res, byte[] ba)
        {
            int offset = RES_GET_OFFSET(res);
            int length;
            if (RES_GET_TYPE(res) == UResourceType.Binary)
            {
                if (offset == 0)
                {
                    return emptyBytes;
                }
                else
                {
                    offset = GetResourceByteOffset(offset);
                    length = GetInt32(offset);
                    if (length == 0)
                    {
                        return emptyBytes;
                    }
                    // Not cached: The array would have to be cloned anyway because
                    // the cache must not be writable via the returned reference.
                    if (ba == null || ba.Length != length)
                    {
                        ba = new byte[length];
                    }
                    offset += 4;
                    if (length <= 16)
                    {
                        for (int i = 0; i < length; ++i)
                        {
                            ba[i] = bytes.Get(offset++);
                        }
                    }
                    else
                    {
                        ByteBuffer temp = bytes.Duplicate();
                        temp.Position = offset;
                        temp.Get(ba);
                    }
                    return ba;
                }
            }
            else
            {
                return null;
            }
        }

        internal ByteBuffer GetBinary(int res)
        {
            int offset = RES_GET_OFFSET(res);
            int length;
            if (RES_GET_TYPE(res) == UResourceType.Binary)
            {
                if (offset == 0)
                {
                    // Don't just
                    //   return emptyByteBuffer;
                    // in case it matters whether the buffer's mark is defined or undefined.
                    return emptyByteBuffer.Duplicate();
                }
                else
                {
                    // Not cached: The returned buffer is small (shares its bytes with the bundle)
                    // and usually quickly discarded after use.
                    // Also, even a cached buffer would have to be cloned because it is mutable
                    // (position & mark).
                    offset = GetResourceByteOffset(offset);
                    length = GetInt32(offset);
                    if (length == 0)
                    {
                        return emptyByteBuffer.Duplicate();
                    }
                    offset += 4;
                    ByteBuffer result = bytes.Duplicate();
                    result.SetPosition(offset).SetLimit(offset + length);
                    result = ICUBinary.SliceWithOrder(result);
                    if (!result.IsReadOnly)
                    {
                        result = result.AsReadOnlyBuffer();
                    }
                    return result;
                }
            }
            else
            {
                return null;
            }
        }

        internal int[] GetInt32Vector(int res)
        {
            int offset = RES_GET_OFFSET(res);
            int length;
            if (RES_GET_TYPE(res) == UResourceType.Int32Vector)
            {
                if (offset == 0)
                {
                    return emptyInts;
                }
                else
                {
                    // Not cached: The array would have to be cloned anyway because
                    // the cache must not be writable via the returned reference.
                    offset = GetResourceByteOffset(offset);
                    length = GetInt32(offset);
                    return GetInt32s(offset + 4, length);
                }
            }
            else
            {
                return null;
            }
        }

        internal Array GetArray(int res)
        {
            UResourceType type = RES_GET_TYPE(res);
            if (!URES_IS_ARRAY(type))
            {
                return null;
            }
            int offset = RES_GET_OFFSET(res);
            if (offset == 0)
            {
                return EMPTY_ARRAY;
            }
            object value = resourceCache.Get(res);
            if (value != null)
            {
                return (Array)value;
            }
            Array array = (type == UResourceType.Array) ?
                    (Array)new Array32(this, offset) : new Array16(this, offset);
            return (Array)resourceCache.GetOrAdd(res, array, 0);
        }

        internal Table GetTable(int res)
        {
            UResourceType type = RES_GET_TYPE(res);
            if (!URES_IS_TABLE(type))
            {
                return null;
            }
            int offset = RES_GET_OFFSET(res);
            if (offset == 0)
            {
                return EMPTY_TABLE;
            }
            object value = resourceCache.Get(res);
            if (value != null)
            {
                return (Table)value;
            }
            Table table;
            int size;  // Use size = 0 to never use SoftReferences for Tables?
            if (type == UResourceType.Table)
            {
                table = new Table1632(this, offset);
                size = table.Length * 2;
            }
            else if (type == UResourceType.Table16)
            {
                table = new Table16(this, offset);
                size = table.Length * 2;
            }
            else /* type == ICUResourceBundle.TABLE32 */
            {
                table = new Table32(this, offset);
                size = table.Length * 4;
            }
            return (Table)resourceCache.GetOrAdd(res, table, size);
        }

        // ICUResource.Value --------------------------------------------------- ***

        /// <summary>
        /// From C++ uresdata.c gPublicTypes[URES_LIMIT].
        /// </summary>
        private static readonly UResourceType[] PUBLIC_TYPES = {
            UResourceType.String,
            UResourceType.Binary,
            UResourceType.Table,
            UResourceType.Alias,

            UResourceType.Table,     /* URES_TABLE32 */
            UResourceType.Table,     /* URES_TABLE16 */
            UResourceType.String,    /* URES_STRING_V2 */
            UResourceType.Int32,

            UResourceType.Array,
            UResourceType.Array,     /* URES_ARRAY16 */
            UResourceType.None,
            UResourceType.None,

            UResourceType.None,
            UResourceType.None,
            UResourceType.Int32Vector,
            UResourceType.None
        };

        internal class ReaderValue : ResourceValue
        {
            internal ICUResourceBundleReader reader;
            internal int res;

            public override UResourceType Type => PUBLIC_TYPES[(int)RES_GET_TYPE(res)];

            public override string GetString()
            {
                string s = reader.GetString(res);
                if (s == null)
                {
                    throw new UResourceTypeMismatchException("");
                }
                return s;
            }

            public override string GetAliasString()
            {
                string s = reader.GetAlias(res);
                if (s == null)
                {
                    throw new UResourceTypeMismatchException("");
                }
                return s;
            }

            public override int GetInt32()
            {
                if (RES_GET_TYPE(res) != UResourceType.Int32)
                {
                    throw new UResourceTypeMismatchException("");
                }
                return RES_GET_INT(res);
            }

            public override int GetUInt32()
            {
                if (RES_GET_TYPE(res) != UResourceType.Int32)
                {
                    throw new UResourceTypeMismatchException("");
                }
                return RES_GET_UINT(res);
            }

            public override int[] GetInt32Vector()
            {
                int[] iv = reader.GetInt32Vector(res);
                if (iv == null)
                {
                    throw new UResourceTypeMismatchException("");
                }
                return iv;
            }

            public override ByteBuffer GetBinary()
            {
                ByteBuffer bb = reader.GetBinary(res);
                if (bb == null)
                {
                    throw new UResourceTypeMismatchException("");
                }
                return bb;
            }

            public override IResourceArray GetArray()
            {
                var array = reader.GetArray(res);
                if (array == null)
                {
                    throw new UResourceTypeMismatchException("");
                }
                return array;
            }

            public override IResourceTable GetTable()
            {
                Table table = reader.GetTable(res);
                if (table == null)
                {
                    throw new UResourceTypeMismatchException("");
                }
                return table;
            }

            public override bool IsNoInheritanceMarker => reader.IsNoInheritanceMarker(res);

            public override string[] GetStringArray()
            {
                Array array = reader.GetArray(res);
                if (array == null)
                {
                    throw new UResourceTypeMismatchException("");
                }
                return GetStringArray(array);
            }

            public override string[] GetStringArrayOrStringAsArray()
            {
                Array array = reader.GetArray(res);
                if (array != null)
                {
                    return GetStringArray(array);
                }
                string s = reader.GetString(res);
                if (s != null)
                {
                    return new string[] { s };
                }
                throw new UResourceTypeMismatchException("");
            }

            public override string GetStringOrFirstOfArray()
            {
                string s = reader.GetString(res);
                if (s != null)
                {
                    return s;
                }
                Array array = reader.GetArray(res);
                if (array != null && array.Length > 0)
                {
                    int r = array.GetContainerResource(reader, 0);
                    s = reader.GetString(r);
                    if (s != null)
                    {
                        return s;
                    }
                }
                throw new UResourceTypeMismatchException("");
            }

            private string[] GetStringArray(Array array)
            {
                string[] result = new string[array.Length];
                for (int i = 0; i < array.Length; ++i)
                {
                    int r = array.GetContainerResource(reader, i);
                    string s = reader.GetString(r);
                    if (s == null)
                    {
                        throw new UResourceTypeMismatchException("");
                    }
                    result[i] = s;
                }
                return result;
            }
        }

        // Container value classes --------------------------------------------- ***

        internal class Container
        {
            protected int size;
            protected int itemsOffset;

            public int Length => size;

            internal virtual int GetContainerResource(ICUResourceBundleReader reader, int index)
            {
                return ICUResourceBundle.ResBogus;
            }
            protected virtual int GetContainer16Resource(ICUResourceBundleReader reader, int index)
            {
                if (index < 0 || size <= index)
                {
                    return ICUResourceBundle.ResBogus;
                }
                int res16 = reader.b16BitUnits[itemsOffset + index];
                if (res16 < reader.poolStringIndex16Limit)
                {
                    // Pool string, nothing to do.
                }
                else
                {
                    // Local string, adjust the 16-bit offset to a regular one,
                    // with a larger pool string index limit.
                    res16 = res16 - reader.poolStringIndex16Limit + reader.poolStringIndexLimit;
                }
                return ((int)UResourceType.StringV2 << 28) | res16;
            }
            protected virtual int GetContainer32Resource(ICUResourceBundleReader reader, int index)
            {
                if (index < 0 || size <= index)
                {
                    return ICUResourceBundle.ResBogus;
                }
                return reader.GetInt32(itemsOffset + 4 * index);
            }
            internal virtual int GetResource(ICUResourceBundleReader reader, string resKey)
            {
                return GetContainerResource(reader, int.Parse(resKey, CultureInfo.InvariantCulture));
            }
            internal Container()
            {
            }
        }
        internal class Array : Container, IResourceArray
        {
            internal Array() { }

            public virtual bool GetValue(int i, ResourceValue value)
            {
                if (0 <= i && i < size)
                {
                    ReaderValue readerValue = (ReaderValue)value;
                    readerValue.res = GetContainerResource(readerValue.reader, i);
                    return true;
                }
                return false;
            }
        }
        private sealed class Array32 : Array
        {
            internal override int GetContainerResource(ICUResourceBundleReader reader, int index)
            {
                return GetContainer32Resource(reader, index);
            }
            internal Array32(ICUResourceBundleReader reader, int offset)
            {
                offset = reader.GetResourceByteOffset(offset);
                size = reader.GetInt32(offset);
                itemsOffset = offset + 4;
            }
        }
        private sealed class Array16 : Array
        {
            internal override int GetContainerResource(ICUResourceBundleReader reader, int index)
            {
                return GetContainer16Resource(reader, index);
            }
            internal Array16(ICUResourceBundleReader reader, int offset)
            {
                size = reader.b16BitUnits[offset];
                itemsOffset = offset + 1;
            }
        }
        internal class Table : Container, IResourceTable
        {
            protected char[] keyOffsets;
            protected int[] key32Offsets;

            internal Table()
            {
            }
            internal string GetKey(ICUResourceBundleReader reader, int index)
            {
                if (index < 0 || size <= index)
                {
                    return null;
                }
                return keyOffsets != null ?
                            reader.GetKey16String(keyOffsets[index]) :
                            reader.GetKey32String(key32Offsets[index]);
            }
            private const int URESDATA_ITEM_NOT_FOUND = -1;
            internal int FindTableItem(ICUResourceBundleReader reader, ReadOnlySpan<char> key)
            {
                int mid, start, limit;
                int result;

                /* do a binary search for the key */
                start = 0;
                limit = size;
                while (start < limit)
                {
                    mid = (start + limit).TripleShift(1);
                    if (keyOffsets != null)
                    {
                        result = reader.CompareKeys(key, keyOffsets[mid]);
                    }
                    else
                    {
                        result = reader.CompareKeys32(key, key32Offsets[mid]);
                    }
                    if (result < 0)
                    {
                        limit = mid;
                    }
                    else if (result > 0)
                    {
                        start = mid + 1;
                    }
                    else
                    {
                        /* We found it! */
                        return mid;
                    }
                }
                return URESDATA_ITEM_NOT_FOUND;  /* not found or table is empty. */
            }

            internal override int GetResource(ICUResourceBundleReader reader, string resKey)
            {
                return GetContainerResource(reader, FindTableItem(reader, resKey.AsSpan()));
            }

            public virtual bool GetKeyAndValue(int i, ResourceKey key, ResourceValue value)
            {
                if (0 <= i && i < size)
                {
                    ReaderValue readerValue = (ReaderValue)value;
                    if (keyOffsets != null)
                    {
                        readerValue.reader.SetKeyFromKey16(keyOffsets[i], key);
                    }
                    else
                    {
                        readerValue.reader.SetKeyFromKey32(key32Offsets[i], key);
                    }
                    readerValue.res = GetContainerResource(readerValue.reader, i);
                    return true;
                }
                return false;
            }
        }
        private sealed class Table1632 : Table
        {
            internal override int GetContainerResource(ICUResourceBundleReader reader, int index)
            {
                return GetContainer32Resource(reader, index);
            }
            internal Table1632(ICUResourceBundleReader reader, int offset)
            {
                offset = reader.GetResourceByteOffset(offset);
                keyOffsets = reader.GetTableKeyOffsets(offset);
                size = keyOffsets.Length;
                itemsOffset = offset + 2 * ((size + 2) & ~1);  // Skip padding for 4-alignment.
            }
        }
        private sealed class Table16 : Table
        {
            internal override int GetContainerResource(ICUResourceBundleReader reader, int index)
            {
                return GetContainer16Resource(reader, index);
            }
            internal Table16(ICUResourceBundleReader reader, int offset)
            {
                keyOffsets = reader.GetTable16KeyOffsets(offset);
                size = keyOffsets.Length;
                itemsOffset = offset + 1 + size;
            }
        }
        private sealed class Table32 : Table
        {
            internal override int GetContainerResource(ICUResourceBundleReader reader, int index)
            {
                return GetContainer32Resource(reader, index);
            }
            internal Table32(ICUResourceBundleReader reader, int offset)
            {
                offset = reader.GetResourceByteOffset(offset);
                key32Offsets = reader.GetTable32KeyOffsets(offset);
                size = key32Offsets.Length;
                itemsOffset = offset + 4 * (1 + size);
            }
        }

        // Resource cache ------------------------------------------------------ ***

        /// <summary>
        /// Cache of some of one resource bundle's resources.
        /// Avoids creating multiple .NET objects for the same resource items,
        /// including multiple copies of their contents.
        /// </summary>
        /// <remarks>
        /// Mutable objects must not be cached and then returned to the caller
        /// because the cache must not be writable via the returned reference.
        /// <para/>
        /// Resources are mapped by their resource integers.
        /// Empty resources with offset 0 cannot be mapped.
        /// Integers need not and should not be cached.
        /// Multiple .res items may share resource offsets (genrb eliminates some duplicates).
        /// <para/>
        /// This cache uses int[] and Object[] arrays to minimize object creation
        /// and avoid auto-boxing.
        /// <para/>
        /// For few resources, a small table is used with binary search.
        /// When more resources are cached, then the data structure changes to be faster
        /// but also use more memory.
        /// </remarks>
        private sealed class ResourceCache
        {
            // ICU4N: To simulate "soft" references, we hold onto cache entries using a sliding expiration.
            private static readonly TimeSpan SlidingExpiration = new TimeSpan(hours: 0, minutes: 5, seconds: 0);

            // Number of items to be stored in a simple array with binary search and insertion sort.
            private const int SIMPLE_LENGTH = 32;

            // When more than SIMPLE_LENGTH items are cached,
            // then switch to a trie-like tree of levels with different array lengths.
            private const int ROOT_BITS = 7;
            private const int NEXT_BITS = 6;

            // Simple table, used when length >= 0.
            private int[] keys = new int[SIMPLE_LENGTH];
            private object[] values = new object[SIMPLE_LENGTH];
            private int length;

            // Trie-like tree of levels, used when length < 0.
            private readonly int maxOffsetBits;
            /// <summary>
            /// Number of bits in each level, each stored in a nibble.
            /// </summary>
            private readonly int levelBitsList;
            private Level rootLevel;

            // ICU4N: The lock was changed to a ReaderWriterLockSlim to allow multiple threads to read simultaneously.
            private readonly ReaderWriterLockSlim syncLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

            private static bool StoreDirectly(int size)
            {
                return size < LARGE_SIZE || CacheValue<object>.FutureInstancesWillBeStrong;
            }

            // ICU4N: syncLock is assumed to already have an open upgradable read lock
            private static object PutIfCleared(object[] values, int index, object item, int size, ReaderWriterLockSlim syncLock)
            {
                object value = values[index];
                if (!(value is SoftReference<object> softRefence))
                {
                    // The caller should be consistent for each resource,
                    // that is, create equivalent objects of equal size every time,
                    // but the CacheValue "strength" may change over time.
                    // assert size < LARGE_SIZE;
                    return value;
                }
                Debug.Assert(size >= LARGE_SIZE);
                if (softRefence.TryGetValue(out value))
                {
                    return value;
                }
                syncLock.EnterWriteLock();
                try
                {
                    // Prevent double entry
                    if (softRefence.TryGetValue(out value))
                        return value;

                    if (CacheValue<object>.FutureInstancesWillBeStrong)
                    {
                        values[index] = item;
                    }
                    else
                    {
#if FEATURE_MICROSOFT_EXTENSIONS_CACHING
                        values[index] = new SoftReference<object>(
                            item,
                            new MemoryCacheEntryOptions { SlidingExpiration = ResourceCache.SlidingExpiration });
#else
                        values[index] = new SoftReference<object>(
                            item,
                            new CacheItemPolicy { SlidingExpiration = ResourceCache.SlidingExpiration });
#endif
                    }
                }
                finally
                {
                    syncLock.ExitWriteLock();
                }
                return item;
            }

            private sealed class Level
            {
                private readonly int levelBitsList;
                private readonly int shift;
                private readonly int mask;
                private readonly int[] keys;
                private readonly object[] values;
                private readonly ReaderWriterLockSlim syncLock;

                internal Level(int levelBitsList, int shift, ReaderWriterLockSlim syncLock)
                {
                    this.levelBitsList = levelBitsList;
                    this.shift = shift;
                    this.syncLock = syncLock ?? throw new ArgumentNullException(nameof(syncLock));
                    int bits = levelBitsList & 0xf;
                    Debug.Assert(bits != 0);
                    int length = 1 << bits;
                    mask = length - 1;
                    keys = new int[length];
                    values = new object[length];
                }

                public object Get(int key)
                {
                    int index = (key >> shift) & mask;
                    syncLock.EnterReadLock();
                    try
                    {
                        int k = keys[index];
                        if (k == key)
                        {
                            return values[index];
                        }
                        if (k == 0)
                        {
                            Level level = (Level)values[index];
                            if (level != null)
                            {
                                return level.Get(key);
                            }
                        }
                        return null;
                    }
                    finally
                    {
                        syncLock.ExitReadLock();
                    }
                }

                public object GetOrAdd(int key, object item, int size)
                {
                    int index = (key >> shift) & mask;
                    syncLock.EnterUpgradeableReadLock();
                    try
                    {
                        int k = keys[index];
                        if (k == key)
                        {
                            return PutIfCleared(values, index, item, size, syncLock);
                        }
                        if (k == 0)
                        {
                            Level level2 = (Level)values[index];
                            if (level2 != null)
                            {
                                return level2.GetOrAdd(key, item, size);
                            }

                            syncLock.EnterWriteLock();
                            try
                            {
                                keys[index] = key;
#if FEATURE_MICROSOFT_EXTENSIONS_CACHING
                                values[index] = StoreDirectly(size) ? item
                                    : new SoftReference<object>(
                                        item,
                                        new MemoryCacheEntryOptions { SlidingExpiration = ResourceCache.SlidingExpiration });
#else
                                values[index] = StoreDirectly(size) ? item
                                    : new SoftReference<object>(
                                        item,
                                        new CacheItemPolicy { SlidingExpiration = ResourceCache.SlidingExpiration });
#endif
                            }
                            finally
                            {
                                syncLock.ExitWriteLock();
                            }
                            return item;
                        }
                        // Collision: Add a child level, move the old item there,
                        // and then insert the current item.
                        Level level = new Level(levelBitsList >> 4, shift + (levelBitsList & 0xf), syncLock);
                        int i = (k >> level.shift) & level.mask;
                        level.keys[i] = k;
                        level.values[i] = values[index];

                        syncLock.EnterWriteLock();
                        try
                        {
                            keys[index] = 0;
                            values[index] = level;
                        }
                        finally
                        {
                            syncLock.ExitWriteLock();
                        }
                        return level.GetOrAdd(key, item, size);
                    }
                    finally
                    {
                        syncLock.ExitUpgradeableReadLock();
                    }
                }
            }

            internal ResourceCache(int maxOffset)
            {
                Debug.Assert(maxOffset != 0);
                maxOffsetBits = 28;
                while (maxOffset <= 0x7ffffff)
                {
                    maxOffset <<= 1;
                    --maxOffsetBits;
                }
                int keyBits = maxOffsetBits + 2;  // +2 for mini type: at most 30 bits used in a key
                                                  // Precompute for each level the number of bits it handles.
                if (keyBits <= ROOT_BITS)
                {
                    levelBitsList = keyBits;
                }
                else if (keyBits < (ROOT_BITS + 3))
                {
                    levelBitsList = 0x30 | (keyBits - 3);
                }
                else
                {
                    levelBitsList = ROOT_BITS;
                    keyBits -= ROOT_BITS;
                    int shift = 4;
                    for (; ; )
                    {
                        if (keyBits <= NEXT_BITS)
                        {
                            levelBitsList |= keyBits << shift;
                            break;
                        }
                        else if (keyBits < (NEXT_BITS + 3))
                        {
                            levelBitsList |= (0x30 | (keyBits - 3)) << shift;
                            break;
                        }
                        else
                        {
                            levelBitsList |= NEXT_BITS << shift;
                            keyBits -= NEXT_BITS;
                            shift += 4;
                        }
                    }
                }
            }

            /// <summary>
            /// Turns a resource integer (with unused bits in the middle)
            /// into a key with fewer bits (at most keyBits).
            /// </summary>
            private int MakeKey(int res)
            {
                // It is possible for resources of different types in the 16-bit array
                // to share a start offset; distinguish between those with a 2-bit value,
                // as a tie-breaker in the bits just above the highest possible offset.
                // It is not possible for "regular" resources of different types
                // to share a start offset with each other,
                // but offsets for 16-bit and "regular" resources overlap;
                // use 2-bit value 0 for "regular" resources.
                UResourceType type = RES_GET_TYPE(res);
                int miniType =
                        (type == UResourceType.StringV2) ? 1 :
                            (type == UResourceType.Table16) ? 3 :
                                (type == UResourceType.Array16) ? 2 : 0;
                return RES_GET_OFFSET(res) | (miniType << maxOffsetBits);
            }

            private int FindSimple(int key)
            {
                // With Java 6, return Arrays.binarySearch(keys, 0, length, key).
                int start = 0;
                int limit = length;
                while ((limit - start) > 8)
                {
                    int mid = (start + limit) / 2;
                    if (key < keys[mid])
                    {
                        limit = mid;
                    }
                    else
                    {
                        start = mid;
                    }
                }
                // For a small number of items, linear search should be a little faster.
                while (start < limit)
                {
                    int k = keys[start];
                    if (key < k)
                    {
                        return ~start;
                    }
                    if (key == k)
                    {
                        return start;
                    }
                    ++start;
                }
                return ~start;
            }

            public object Get(int res)
            {
                syncLock.EnterReadLock();
                try
                {
                    // Integers and empty resources need not be cached.
                    // The cache itself uses res=0 for "no match".
                    Debug.Assert(RES_GET_OFFSET(res) != 0);
                    object value;
                    if (length >= 0)
                    {
                        int index = FindSimple(res);
                        if (index >= 0)
                        {
                            value = values[index];
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        value = rootLevel.Get(MakeKey(res));
                        if (value == null)
                        {
                            return null;
                        }
                    }
                    if (value is SoftReference<object> softReference)
                        softReference.TryGetValue(out value);
                    return value;  // null if the reference was cleared
                }
                finally
                {
                    syncLock.ExitReadLock();
                }
            }

            public object GetOrAdd(int res, object item, int size)
            {
                syncLock.EnterUpgradeableReadLock();
                try
                {
                    if (length >= 0)
                    {
                        int index = FindSimple(res);
                        if (index >= 0)
                        {
                            return PutIfCleared(values, index, item, size, syncLock);
                        }
                        else if (length < SIMPLE_LENGTH)
                        {
                            index = ~index;
                            syncLock.EnterWriteLock();
                            try
                            {
                                if (index < length)
                                {
                                    System.Array.Copy(keys, index, keys, index + 1, length - index);
                                    System.Array.Copy(values, index, values, index + 1, length - index);
                                }
                                ++length;
                                keys[index] = res;
#if FEATURE_MICROSOFT_EXTENSIONS_CACHING
                                values[index] = StoreDirectly(size) ? item
                                    : new SoftReference<object>(
                                        item,
                                        new MemoryCacheEntryOptions { SlidingExpiration = ResourceCache.SlidingExpiration });
#else
                                values[index] = StoreDirectly(size) ? item
                                    : new SoftReference<object>(
                                        item,
                                        new CacheItemPolicy { SlidingExpiration = ResourceCache.SlidingExpiration });
#endif
                            }
                            finally
                            {
                                syncLock.ExitWriteLock();
                            }
                            return item;
                        }
                        else /* not found && length == SIMPLE_LENGTH */
                        {
                            // Grow to become trie-like.
                            rootLevel = new Level(levelBitsList, 0, syncLock);
                            for (int i = 0; i < SIMPLE_LENGTH; ++i)
                            {
                                rootLevel.GetOrAdd(MakeKey(keys[i]), values[i], 0);
                            }
                            keys = null;
                            values = null;
                            length = -1;
                        }
                    }
                    return rootLevel.GetOrAdd(MakeKey(res), item, size);
                }
                finally
                {
                    syncLock.ExitUpgradeableReadLock();
                }
            }
        }

        private const string ICU_RESOURCE_SUFFIX = ".res";

        /// <summary>
        /// Gets the full name of the resource with suffix.
        /// </summary>
        public static string GetFullName(string baseName, string localeName)
        {
            if (baseName == null || baseName.Length == 0)
            {
                if (localeName.Length == 0)
                {
                    localeName = UCultureInfo.CurrentCulture.ToString();
                }
                return localeName + ICU_RESOURCE_SUFFIX;
            }
            else
            {
                if (baseName.IndexOf('.') == -1)
                {
                    if (baseName[baseName.Length - 1] != '/')
                    {
                        return baseName + "/" + localeName + ICU_RESOURCE_SUFFIX;
                    }
                    else
                    {
                        return baseName + localeName + ICU_RESOURCE_SUFFIX;
                    }
                }
                else
                {
                    baseName = baseName.Replace('.', '/');
                    if (localeName.Length == 0)
                    {
                        return baseName + ICU_RESOURCE_SUFFIX;
                    }
                    else
                    {
                        return baseName + "_" + localeName + ICU_RESOURCE_SUFFIX;
                    }
                }
            }
        }
    }
}
