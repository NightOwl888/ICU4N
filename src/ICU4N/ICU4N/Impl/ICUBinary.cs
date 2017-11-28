using ICU4N.Support;
using ICU4N.Support.IO;
using ICU4N.Support.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Reflection;
using System.Text;

namespace ICU4N.Impl
{
    /// <summary>
    /// Special interface for data authentication
    /// </summary>
    public interface IAuthenticate
    {
        /// <summary>
        /// Method used in ICUBinary.ReadHeader() to provide data format
        /// authentication.
        /// </summary>
        /// <param name="version">version of the current data</param>
        /// <returns>true if dataformat is an acceptable version, false otherwise.</returns>
        bool IsDataVersionAcceptable(byte[] version);
    }

    public sealed class ICUBinary
    {
        /**
     * Reads the ICU .dat package file format.
     * Most methods do not modify the ByteBuffer in any way,
     * not even its position or other state.
     */
        private sealed class DatPackageReader
        {
            /**
             * .dat package data format ID "CmnD".
             */
            private static readonly int DATA_FORMAT = 0x436d6e44;

            private sealed class IsAcceptable : IAuthenticate
            {

                public bool IsDataVersionAcceptable(byte[] version)
                {
                    return version[0] == 1;
                }
            }
            private static readonly IsAcceptable IS_ACCEPTABLE = new IsAcceptable();

            /**
             * Checks that the ByteBuffer contains a valid, usable ICU .dat package.
             * Moves the buffer position from 0 to after the data header.
             */
            internal static bool Validate(ByteBuffer bytes)
            {
                try
                {
                    ReadHeader(bytes, DATA_FORMAT, IS_ACCEPTABLE);
                }
                catch (IOException ignored)
                {
                    return false;
                }
                int count = bytes.GetInt32(bytes.Position);  // Do not move the position.
                if (count <= 0)
                {
                    return false;
                }
                // For each item, there is one ToC entry (8 bytes) and a name string
                // and a data item of at least 16 bytes.
                // (We assume no data item duplicate elimination for now.)
                if (bytes.Position + 4 + count * (8 + 16) > bytes.Capacity)
                {
                    return false;
                }
                if (!StartsWithPackageName(bytes, GetNameOffset(bytes, 0)) ||
                        !StartsWithPackageName(bytes, GetNameOffset(bytes, count - 1)))
                {
                    return false;
                }
                return true;
            }

            private static bool StartsWithPackageName(ByteBuffer bytes, int start)
            {
                // Compare all but the trailing 'b' or 'l' which depends on the platform.
                int length = ICUData.PACKAGE_NAME.Length - 1;
                for (int i = 0; i < length; ++i)
                {
                    if (bytes.Get(start + i) != ICUData.PACKAGE_NAME[i])
                    {
                        return false;
                    }
                }
                // Check for 'b' or 'l' followed by '/'.
                byte c = bytes.Get(start + length++);
                if ((c != 'b' && c != 'l') || bytes.Get(start + length) != '/')
                {
                    return false;
                }
                return true;
            }

            internal static ByteBuffer GetData(ByteBuffer bytes, string key) // ICU4N specific - changed key from ICharSequence to string
            {
                int index = BinarySearch(bytes, key);
                if (index >= 0)
                {
                    ByteBuffer data = bytes.Duplicate();
                    data.Position = GetDataOffset(bytes, index);
                    data.Limit = GetDataOffset(bytes, index + 1);
                    return ICUBinary.SliceWithOrder(data);
                }
                else
                {
                    return null;
                }
            }

            internal static void AddBaseNamesInFolder(ByteBuffer bytes, string folder, string suffix, ISet<string> names)
            {
                // Find the first data item name that starts with the folder name.
                int index = BinarySearch(bytes, folder);
                if (index < 0)
                {
                    index = ~index;  // Normal: Otherwise the folder itself is the name of a data item.
                }

                int @base = bytes.Position;
                int count = bytes.GetInt32(@base);
                StringBuilder sb = new StringBuilder();
                while (index < count && AddBaseName(bytes, index, folder, suffix, sb, names))
                {
                    ++index;
                }
            }

            private static int BinarySearch(ByteBuffer bytes, string key) // ICU4N specific - changed key from ICharSequence to string
            {
                int @base = bytes.Position;
                int count = bytes.GetInt32(@base);

                // Do a binary search for the key.
                int start = 0;
                int limit = count;
                while (start < limit)
                {
                    int mid = (start + limit).TripleShift(1);
                    int nameOffset = GetNameOffset(bytes, mid);
                    // Skip "icudt54b/".
                    nameOffset += ICUData.PACKAGE_NAME.Length + 1;
                    int result = CompareKeys(key, bytes, nameOffset);
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
                        // We found it!
                        return mid;
                    }
                }
                return ~start;  // Not found or table is empty.
            }

            private static int GetNameOffset(ByteBuffer bytes, int index)
            {
                int @base = bytes.Position;
                bool checkCount = 0 <= index && index < bytes.GetInt32(@base);
                Debug.Assert(checkCount);  // count
                                           // The count integer (4 bytes)
                                           // is followed by count (nameOffset, dataOffset) integer pairs (8 bytes per pair).
                return @base + bytes.GetInt32(@base + 4 + index * 8);
            }

            private static int GetDataOffset(ByteBuffer bytes, int index)
            {
                int @base = bytes.position;
                int count = bytes.GetInt32(@base);
                if (index == count)
                {
                    // Return the limit of the last data item.
                    return bytes.Capacity;
                }
                Debug.Assert(0 <= index && index < count);
                // The count integer (4 bytes)
                // is followed by count (nameOffset, dataOffset) integer pairs (8 bytes per pair).
                // The dataOffset follows the nameOffset (skip another 4 bytes).
                return @base + bytes.GetInt32(@base + 4 + 4 + index * 8);
            }

            static bool AddBaseName(ByteBuffer bytes, int index,
                    string folder, string suffix, StringBuilder sb, ISet<string> names)
            {
                int offset = GetNameOffset(bytes, index);
                // Skip "icudt54b/".
                offset += ICUData.PACKAGE_NAME.Length + 1;
                if (folder.Length != 0)
                {
                    // Test name.startsWith(folder + '/').
                    for (int i = 0; i < folder.Length; ++i, ++offset)
                    {
                        if (bytes.Get(offset) != folder[i])
                        {
                            return false;
                        }
                    }
                    if (bytes.Get(offset++) != '/')
                    {
                        return false;
                    }
                }
                // Collect the NUL-terminated name and test for a subfolder, then test for the suffix.
                sb.Length=0;
                byte b;
                while ((b = bytes.Get(offset++)) != 0)
                {
                    char c = (char)b;
                    if (c == '/')
                    {
                        return true;  // Skip subfolder contents.
                    }
                    sb.Append(c);
                }
                int nameLimit = sb.Length - suffix.Length;
                if (sb.LastIndexOf(suffix, nameLimit) >= 0)
                {
                    names.Add(sb.ToString(0, nameLimit - 0));
                }
                return true;
            }
        }

        private abstract class DataFile
        {
            protected readonly string itemPath;

            internal DataFile(string item)
            {
                itemPath = item;
            }

            public override string ToString()
            {
                return itemPath;
            }

            internal abstract ByteBuffer GetData(string requestedPath);

            /**
             * @param folder The relative ICU data folder, like "" or "coll".
             * @param suffix Usually ".res".
             * @param names File base names relative to the folder are added without the suffix,
             *        for example "de_CH".
             */
            internal abstract void AddBaseNamesInFolder(string folder, string suffix, ISet<string> names);
        }

        private sealed class SingleDataFile : DataFile
        {
            private readonly FileInfo path;

            internal SingleDataFile(string item, FileInfo path)
                    : base(item)
            {
                this.path = path;
            }

            public override string ToString()
            {
                return path.ToString();
            }

            internal override ByteBuffer GetData(string requestedPath)
            {
                if (requestedPath.Equals(itemPath))
                {
                    return MapFile(path);
                }
                else
                {
                    return null;
                }
            }


            internal override void AddBaseNamesInFolder(string folder, string suffix, ISet<string> names)
            {
                if (itemPath.Length > folder.Length + suffix.Length &&
                        itemPath.StartsWith(folder, StringComparison.Ordinal) &&
                        itemPath.EndsWith(suffix, StringComparison.Ordinal) &&
                        itemPath[folder.Length] == '/' &&
                        itemPath.IndexOf('/', folder.Length + 1) < 0)
                {
                    names.Add(itemPath.Substring(folder.Length + 1,
                            (itemPath.Length - suffix.Length) - (folder.Length + 1))); // ICU4N: Corrected 2nd parameter
                }
            }
        }

        private sealed class PackageDataFile : DataFile
        {
            /**
             * .dat package bytes, or null if not a .dat package.
             * position() is after the header.
             * Do not modify the position or other state, for thread safety.
             */
            private readonly ByteBuffer pkgBytes;

            internal PackageDataFile(string item, ByteBuffer bytes)
                    : base(item)
            {
                pkgBytes = bytes;
            }


            internal override ByteBuffer GetData(string requestedPath)
            {
                return DatPackageReader.GetData(pkgBytes, requestedPath);
            }

            internal override void AddBaseNamesInFolder(string folder, string suffix, ISet<string> names)
            {
                DatPackageReader.AddBaseNamesInFolder(pkgBytes, folder, suffix, names);
            }
        }

        private static readonly IList<DataFile> icuDataFiles = new List<DataFile>();

        static ICUBinary()
        {
            // ICU4N TODO: Fix path
            // Normally com.ibm.icu.impl.ICUBinary.dataPath.
            string dataPath = ICUConfig.Get(typeof(ICUBinary).GetTypeInfo().Name + "_DataPath");
            if (dataPath != null)
            {
                AddDataFilesFromPath(dataPath, icuDataFiles);
            }
        }

        private static void AddDataFilesFromPath(string dataPath, IList<DataFile> files)
        {
            // Split the path and find files in each location.
            // This splitting code avoids the regex pattern compilation in string.split()
            // and its array allocation.
            // (There is no simple by-character split()
            // and the StringTokenizer "is discouraged in new code".)
            int pathStart = 0;
            while (pathStart < dataPath.Length)
            {
                int sepIndex = dataPath.IndexOf(Path.DirectorySeparatorChar, pathStart);
                int pathLimit;
                if (sepIndex >= 0)
                {
                    pathLimit = sepIndex;
                }
                else
                {
                    pathLimit = dataPath.Length;
                }
                string path = dataPath.Substring(pathStart, pathLimit - pathStart).Trim(); // ICU4N: Corrected 2nd parameter
                if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    path = path.Substring(0, (path.Length - 1) - 0); // ICU4N: Checked 2nd parameter
                }
                if (path.Length != 0)
                {
                    AddDataFilesFromFolder(new DirectoryInfo(path), new StringBuilder(), icuDataFiles);
                }
                if (sepIndex < 0)
                {
                    break;
                }
                pathStart = sepIndex + 1;
            }
        }

        private static void AddDataFilesFromFolder(DirectoryInfo folder, StringBuilder itemPath,
                IList<DataFile> dataFiles)
        {
            FileInfo[] files = folder.GetFiles();
            DirectoryInfo[] folders = folder.GetDirectories();
            if ((files == null || files.Length == 0) && (folders == null || folders.Length == 0))
            {
                return;
            }
            int folderPathLength = itemPath.Length;
            if (folderPathLength > 0)
            {
                // The item path must use the ICU file separator character,
                // not the platform-dependent File.separatorChar,
                // so that the enumerated item paths match the paths requested by ICU code.
                itemPath.Append('/');
                ++folderPathLength;
            }
            foreach (DirectoryInfo folder2 in folders)
            {
                // TODO: Within a folder, put all single files before all .dat packages?
                AddDataFilesFromFolder(folder2, itemPath, dataFiles);
            }

            foreach (FileInfo file in files)
            {
                string fileName = file.Name;
                if (fileName.EndsWith(".txt", StringComparison.Ordinal))
                {
                    continue;
                }
                itemPath.Append(fileName);
                if (fileName.EndsWith(".dat", StringComparison.Ordinal))
                {
                    ByteBuffer pkgBytes = MapFile(file);
                    if (pkgBytes != null && DatPackageReader.Validate(pkgBytes))
                    {
                        dataFiles.Add(new PackageDataFile(itemPath.ToString(), pkgBytes));
                    }
                }
                else
                {
                    dataFiles.Add(new SingleDataFile(itemPath.ToString(), file));
                }
                itemPath.Length = folderPathLength;
            }
        }

        /**
         * Compares the length-specified input key with the
         * NUL-terminated table key. (ASCII)
         */
        internal static int CompareKeys(string key, ByteBuffer bytes, int offset) // ICU4N specific: Changed key from ICharSequence to string
        {
            for (int i = 0; ; ++i, ++offset)
            {
                int c2 = bytes.Get(offset);
                if (c2 == 0)
                {
                    if (i == key.Length)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;  // key > table key because key is longer.
                    }
                }
                else if (i == key.Length)
                {
                    return -1;  // key < table key because key is shorter.
                }
                int diff = key[i] - c2;
                if (diff != 0)
                {
                    return diff;
                }
            }
        }

        internal static int CompareKeys(string key, byte[] bytes, int offset) // ICU4N specific: Changed key from ICharSequence to string
        {
            for (int i = 0; ; ++i, ++offset)
            {
                int c2 = bytes[offset];
                if (c2 == 0)
                {
                    if (i == key.Length)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;  // key > table key because key is longer.
                    }
                }
                else if (i == key.Length)
                {
                    return -1;  // key < table key because key is shorter.
                }
                int diff = key[i] - c2;
                if (diff != 0)
                {
                    return diff;
                }
            }
        }

        // ICU4N specific - de-nested IAuthenticate


        // public methods --------------------------------------------------------

        /**
         * Loads an ICU binary data file and returns it as a ByteBuffer.
         * The buffer contents is normally read-only, but its position etc. can be modified.
         *
         * @param itemPath Relative ICU data item path, for example "root.res" or "coll/ucadata.icu".
         * @return The data as a read-only ByteBuffer,
         *         or null if the resource could not be found.
         */
        public static ByteBuffer GetData(string itemPath)
        {
            return GetData(null, null, itemPath, false);
        }

        /**
         * Loads an ICU binary data file and returns it as a ByteBuffer.
         * The buffer contents is normally read-only, but its position etc. can be modified.
         *
         * @param loader Used for loader.getResourceAsStream() unless the data is found elsewhere.
         * @param resourceName Resource name for use with the loader.
         * @param itemPath Relative ICU data item path, for example "root.res" or "coll/ucadata.icu".
         * @return The data as a read-only ByteBuffer,
         *         or null if the resource could not be found.
         */
        public static ByteBuffer GetData(Assembly loader, string resourceName, string itemPath)
        {
            return GetData(loader, resourceName, itemPath, false);
        }

        /**
         * Loads an ICU binary data file and returns it as a ByteBuffer.
         * The buffer contents is normally read-only, but its position etc. can be modified.
         *
         * @param itemPath Relative ICU data item path, for example "root.res" or "coll/ucadata.icu".
         * @return The data as a read-only ByteBuffer.
         * @throws MissingResourceException if required==true and the resource could not be found
         */
        public static ByteBuffer GetRequiredData(string itemPath)
        {
            return GetData(null, null, itemPath, true);
        }

        /**
         * Loads an ICU binary data file and returns it as a ByteBuffer.
         * The buffer contents is normally read-only, but its position etc. can be modified.
         *
         * @param loader Used for loader.getResourceAsStream() unless the data is found elsewhere.
         * @param resourceName Resource name for use with the loader.
         * @param itemPath Relative ICU data item path, for example "root.res" or "coll/ucadata.icu".
         * @return The data as a read-only ByteBuffer.
         * @throws MissingResourceException if required==true and the resource could not be found
         */
        public static ByteBuffer GetRequiredData(Assembly loader, string resourceName,
                string itemPath)
        {
            return GetData(loader, resourceName, itemPath, true);
        }

        /**
         * Loads an ICU binary data file and returns it as a ByteBuffer.
         * The buffer contents is normally read-only, but its position etc. can be modified.
         *
         * @param loader Used for loader.getResourceAsStream() unless the data is found elsewhere.
         * @param resourceName Resource name for use with the loader.
         * @param itemPath Relative ICU data item path, for example "root.res" or "coll/ucadata.icu".
         * @param required If the resource cannot be found,
         *        this method returns null (!required) or throws an exception (required).
         * @return The data as a read-only ByteBuffer,
         *         or null if required==false and the resource could not be found.
         * @throws MissingResourceException if required==true and the resource could not be found
         */
        private static ByteBuffer GetData(Assembly loader, string resourceName,
                string itemPath, bool required)
        {
            ByteBuffer bytes = GetDataFromFile(itemPath);
            if (bytes != null)
            {
                return bytes;
            }
            if (loader == null)
            {
                loader = typeof(ICUData).GetTypeInfo().Assembly;
            }
            if (resourceName == null)
            {
                resourceName = ICUData.ICU_BASE_NAME + '/' + itemPath;
            }
            ByteBuffer buffer = null;
            try
            {
                // Closed by getByteBufferFromInputStreamAndCloseStream().
                Stream @is = ICUData.GetStream(loader, resourceName, required);
                if (@is == null)
                {
                    return null;
                }
                buffer = GetByteBufferFromInputStreamAndCloseStream(@is);
            }
            catch (IOException e)
            {
                throw new ICUUncheckedIOException(e);
            }
            return buffer;
        }

        private static ByteBuffer GetDataFromFile(string itemPath)
        {
            foreach (DataFile dataFile in icuDataFiles)
            {
                ByteBuffer data = dataFile.GetData(itemPath);
                if (data != null)
                {
                    return data;
                }
            }
            return null;
        }

        //@SuppressWarnings("resource")  // Closing a file closes its channel.
        private static ByteBuffer MapFile(FileInfo path)
        {
            MemoryMappedFile file;
            try
            {
                //file = new FileStream(path.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                file = MemoryMappedFile.CreateFromFile(path.FullName);
                var channel = file.CreateViewAccessor();
                //FileChannel channel = file.getChannel();
                ByteBuffer bytes = null;
                try
                {
                    //bytes = channel.map(FileChannel.MapMode.READ_ONLY, 0, channel.size());
                    bytes = new MemoryMappedFileByteBuffer(channel, 0, 0);
                }
                finally
                {
                    file.Dispose();
                    channel.Dispose();
                }
                return bytes;
            }
            catch (FileNotFoundException ignored)
            {
                Console.Error.WriteLine(ignored);
            }
            catch (IOException ignored)
            {
                Console.Error.WriteLine(ignored);
            }
            return null;
        }

        /**
         * @param folder The relative ICU data folder, like "" or "coll".
         * @param suffix Usually ".res".
         * @param names File base names relative to the folder are added without the suffix,
         *        for example "de_CH".
         */
        public static void AddBaseNamesInFileFolder(string folder, string suffix, ISet<string> names)
        {
            foreach (DataFile dataFile in icuDataFiles)
            {
                dataFile.AddBaseNamesInFolder(folder, suffix, names);
            }
        }

        /**
         * Same as readHeader(), but returns a VersionInfo rather than a compact int.
         */
        public static VersionInfo ReadHeaderAndDataVersion(ByteBuffer bytes,
                                                                 int dataFormat,
                                                                 IAuthenticate authenticate)
        {
            return GetVersionInfoFromCompactInt(ReadHeader(bytes, dataFormat, authenticate));
        }

        /**
         * Reads an ICU data header, checks the data format, and returns the data version.
         *
         * <p>Assumes that the ByteBuffer position is 0 on input.
         * The buffer byte order is set according to the data.
         * The buffer position is advanced past the header (including UDataInfo and comment).
         *
         * <p>See C++ ucmndata.h and unicode/udata.h.
         *
         * @return dataVersion
         * @throws IOException if this is not a valid ICU data item of the expected dataFormat
         */
        public static int ReadHeader(ByteBuffer bytes, int dataFormat, IAuthenticate authenticate)
        {
            Debug.Assert(bytes != null && bytes.Position == 0);
            byte magic1 = bytes.Get(2);
            byte magic2 = bytes.Get(3);
            if (magic1 != MAGIC1 || magic2 != MAGIC2)
            {
                throw new IOException(MAGIC_NUMBER_AUTHENTICATION_FAILED_);
            }

            byte isBigEndian = bytes.Get(8);
            byte charsetFamily = bytes.Get(9);
            byte sizeofUChar = bytes.Get(10);
            if (isBigEndian < 0 || 1 < isBigEndian ||
                    charsetFamily != CHAR_SET_ || sizeofUChar != CHAR_SIZE_)
            {
                throw new IOException(HEADER_AUTHENTICATION_FAILED_);
            }
            bytes.Order = isBigEndian != 0 ? ByteOrder.BIG_ENDIAN : ByteOrder.LITTLE_ENDIAN;

            int headerSize = bytes.GetChar(0);
            int sizeofUDataInfo = bytes.GetChar(4);
            if (sizeofUDataInfo < 20 || headerSize < (sizeofUDataInfo + 4))
            {
                throw new IOException("Internal Error: Header size error");
            }
            // TODO: Change Authenticate to take int major, int minor, int milli, int micro
            // to avoid array allocation.
            byte[] formatVersion = new byte[] {
            bytes.Get(16), bytes.Get(17), bytes.Get(18), bytes.Get(19)
        };
            if (bytes.Get(12) != (byte)(dataFormat >> 24) ||
                    bytes.Get(13) != (byte)(dataFormat >> 16) ||
                    bytes.Get(14) != (byte)(dataFormat >> 8) ||
                    bytes.Get(15) != (byte)dataFormat ||
                    (authenticate != null && !authenticate.IsDataVersionAcceptable(formatVersion)))
            {
                // "; data format %02x%02x%02x%02x, format version %d.%d.%d.%d"

                throw new IOException(HEADER_AUTHENTICATION_FAILED_ +
                string.Format("; data format {0:x2}{1:x2}{2:x2}{3:x2}, format version {4}.{5}.{6}.{7}",
                        bytes.Get(12), bytes.Get(13), bytes.Get(14), bytes.Get(15),
                        formatVersion[0] & 0xff, formatVersion[1] & 0xff,
                        formatVersion[2] & 0xff, formatVersion[3] & 0xff));
            }

            bytes.Position = headerSize;
            return  // dataVersion
                    (bytes.Get(20) << 24) |
                    ((bytes.Get(21) & 0xff) << 16) |
                    ((bytes.Get(22) & 0xff) << 8) |
                    (bytes.Get(23) & 0xff);
        }

        /**
         * Writes an ICU data header.
         * Does not write a copyright string.
         *
         * @return The length of the header (number of bytes written).
         * @throws IOException from the DataOutputStream
         */
        public static int WriteHeader(int dataFormat, int formatVersion, int dataVersion,
                DataOutputStream dos)
        {
            // ucmndata.h MappedData
            dos.WriteChar(32);  // headerSize
            dos.WriteByte(MAGIC1);
            dos.WriteByte(MAGIC2);
            // unicode/udata.h UDataInfo
            dos.WriteChar(20);  // sizeof(UDataInfo)
            dos.WriteChar(0);  // reservedWord
            dos.WriteByte(1);  // isBigEndian
            dos.WriteByte(CHAR_SET_);  // charsetFamily
            dos.WriteByte(CHAR_SIZE_);  // sizeofUChar
            dos.WriteByte(0);  // reservedByte
            dos.WriteInt32(dataFormat);
            dos.WriteInt32(formatVersion);
            dos.WriteInt32(dataVersion);
            // 8 bytes padding for 32 bytes headerSize (multiple of 16).
            dos.WriteInt64(0);
            Debug.Assert(dos.Length == 32);
            return 32;
        }

        public static void SkipBytes(ByteBuffer bytes, int skipLength)
        {
            if (skipLength > 0)
            {
                bytes.Position = bytes.Position + skipLength;
            }
        }

        public static string GetString(ByteBuffer bytes, int length, int additionalSkipLength)
        {
            ICharSequence cs = bytes.AsCharBuffer();
            string s = cs.SubSequence(0, length).ToString();
            SkipBytes(bytes, length * 2 + additionalSkipLength);
            return s;
        }

        public static char[] GetChars(ByteBuffer bytes, int length, int additionalSkipLength)
        {
            char[] dest = new char[length];
            bytes.AsCharBuffer().Get(dest);
            SkipBytes(bytes, length * 2 + additionalSkipLength);
            return dest;
        }

        public static short[] GetShorts(ByteBuffer bytes, int length, int additionalSkipLength) // ICU4N TODO: Rename GetInt16s
        {
            short[] dest = new short[length];
            bytes.AsInt16Buffer().Get(dest);
            SkipBytes(bytes, length * 2 + additionalSkipLength);
            return dest;
        }

        public static int[] GetInts(ByteBuffer bytes, int length, int additionalSkipLength) // ICU4N TODO: Rename GetInt32s
        {
            int[] dest = new int[length];
            bytes.AsInt32Buffer().Get(dest);
            SkipBytes(bytes, length * 4 + additionalSkipLength);
            return dest;
        }

        public static long[] GetLongs(ByteBuffer bytes, int length, int additionalSkipLength) // ICU4N TODO: Rename GetInt64s
        {
            long[] dest = new long[length];
            bytes.AsInt64Buffer().Get(dest);
            SkipBytes(bytes, length * 8 + additionalSkipLength);
            return dest;
        }

        /**
         * Same as ByteBuffer.slice() plus preserving the byte order.
         */
        public static ByteBuffer SliceWithOrder(ByteBuffer bytes)
        {
            ByteBuffer b = bytes.Slice();
            return b.SetOrder(bytes.Order);
        }

        /**
         * Reads the entire contents from the stream into a byte array
         * and wraps it into a ByteBuffer. Closes the InputStream at the end.
         */
        public static ByteBuffer GetByteBufferFromInputStreamAndCloseStream(Stream input)
        {
            try
            {
                // is.available() may return 0, or 1, or the total number of bytes in the stream,
                // or some other number.
                // Do not try to use is.available() == 0 to find the end of the stream!
                byte[] bytes;
                long avail = input.Length;
                if (avail > 32)
                {
                    // There are more bytes available than just the ICU data header length.
                    // With luck, it is the total number of bytes.
                    bytes = new byte[avail];
                }
                else
                {
                    bytes = new byte[128];  // empty .res files are even smaller
                }
                // Call is.read(...) until one returns a negative value.
                int length = 0;
                for (; ; )
                {
                    if (length < bytes.Length)
                    {
                        int numRead = input.Read(bytes, length, bytes.Length - length);
                        if (numRead <= 0) // ICU4N specific - In .NET, 0 rather than -1 is returned when complete
                        {
                            break;  // end of stream
                        }
                        length += numRead;
                    }
                    else
                    {
                        // See if we are at the end of the stream before we grow the array.
                        int nextByte = input.ReadByte();
                        if (nextByte < 0)
                        {
                            break;
                        }
                        int capacity = 2 * bytes.Length;
                        if (capacity < 128)
                        {
                            capacity = 128;
                        }
                        else if (capacity < 0x4000)
                        {
                            capacity *= 2;  // Grow faster until we reach 16kB.
                        }
                        // TODO Java 6 replace new byte[] and arraycopy(): bytes = Arrays.copyOf(bytes, capacity);
                        byte[] newBytes = new byte[capacity];
                        System.Array.Copy(bytes, 0, newBytes, 0, length);
                        bytes = newBytes;
                        bytes[length++] = (byte)nextByte;
                    }
                }
                return ByteBuffer.Wrap(bytes, 0, length);
            }
            finally
            {
                input.Dispose();
            }
        }

        /**
         * Returns a VersionInfo for the bytes in the compact version integer.
         */
        public static VersionInfo GetVersionInfoFromCompactInt(int version) // ICU4N TODO: API - Rename GetVersionInfoFromCompactInt32
        {
            return VersionInfo.GetInstance(
                    (version.TripleShift(24)), (version >> 16) & 0xff, (version >> 8) & 0xff, version & 0xff);
        }

        /**
         * Returns an array of the bytes in the compact version integer.
         */
        public static byte[] GetVersionByteArrayFromCompactInt(int version) // ICU4N TODO: API - Rename GetVersionByteArrayFromCompactInt32
        {
            return new byte[] {
                (byte)(version >> 24),
                (byte)(version >> 16),
                (byte)(version >> 8),
                (byte)(version)
        };
        }

        // private variables -------------------------------------------------

        /**
        * Magic numbers to authenticate the data file
        */
        private static readonly byte MAGIC1 = (byte)0xda;
        private static readonly byte MAGIC2 = (byte)0x27;

        /**
        * File format authentication values
        */
        private static readonly byte CHAR_SET_ = 0;
        private static readonly byte CHAR_SIZE_ = 2;

        /**
        * Error messages
        */
        private static readonly string MAGIC_NUMBER_AUTHENTICATION_FAILED_ =
                               "ICU data file error: Not an ICU data file";
        private static readonly string HEADER_AUTHENTICATION_FAILED_ =
            "ICU data file error: Header authentication failed, please check if you have a valid ICU data file";

    }
}
