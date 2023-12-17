using ICU4N.Globalization;
using J2N.IO;
using J2N.Text;
using System;
using System.IO;
using System.Resources;
using System.Text;

namespace ICU4N.Impl
{
    /// <summary>
    /// Wrapper for the pnames.icu binary data file.  This data file is
    /// imported from icu4c.  It contains property and property value
    /// aliases from the UCD files PropertyAliases.txt and
    /// PropertyValueAliases.txt.  The file is built by the icu4c tool
    /// genpname.  It must be an ASCII big-endian file to be
    /// usable in ICU4N.
    /// </summary>
    /// <remarks>
    /// This class performs two functions.
    /// <list type="number">
    ///     <item><description>It can import the flat binary data into usable objects.</description></item>
    ///     <item><description>It provides an API to access the tree of objects.</description></item>
    /// </list>
    /// <para/>
    /// Needless to say, this class is tightly coupled to the binary format
    /// of icu4c's pnames.icu file.
    /// <para/>
    /// Each time a <see cref="UPropertyAliases"/> is constructed, the pnames.icu file is
    /// read, parsed, and data structures assembled.  Clients should create one
    /// singleton instance and cache it.
    /// </remarks>
    /// <author>Alan Liu</author>
    /// <since>ICU 2.4</since>
    public sealed partial class UPropertyAliases
    {
        // Byte offsets from the start of the data, after the generic header.
        private const int IX_VALUE_MAPS_OFFSET = 0;
        private const int IX_BYTE_TRIES_OFFSET = 1;
        private const int IX_NAME_GROUPS_OFFSET = 2;
        private const int IX_RESERVED3_OFFSET = 3;
        // private const int IX_RESERVED4_OFFSET=4;
        // private const int IX_TOTAL_SIZE=5;

        // Other values.
        // private const int IX_MAX_NAME_LENGTH=6;
        // private const int IX_RESERVED7=7;
        // private const int IX_COUNT=8;

        //----------------------------------------------------------------
        // Runtime data.  This is an unflattened representation of the
        // data in pnames.icu.

        private int[] valueMaps;
        private byte[] bytesTries;
        private string nameGroups;

        private sealed class IsAcceptable : IAuthenticate
        {
            // @Override when we switch to Java 6
            public bool IsDataVersionAcceptable(byte[] version)
            {
                return version[0] == 2;
            }
        }
        private static readonly IsAcceptable IS_ACCEPTABLE = new IsAcceptable();
        private const int DATA_FORMAT = 0x706E616D;  // "pnam"

        private void Load(ByteBuffer bytes)
        {
            //dataVersion=ICUBinary.readHeaderAndDataVersion(bytes, DATA_FORMAT, IS_ACCEPTABLE);
            ICUBinary.ReadHeader(bytes, DATA_FORMAT, IS_ACCEPTABLE);
            int indexesLength = bytes.GetInt32() / 4;  // inIndexes[IX_VALUE_MAPS_OFFSET]/4
            if (indexesLength < 8)
            {  // formatVersion 2 initially has 8 indexes
                throw new IOException("pnames.icu: not enough indexes");
            }
            int[]
            inIndexes = new int[indexesLength];
            inIndexes[0] = indexesLength * 4;
            for (int i = 1; i < indexesLength; ++i)
            {
                inIndexes[i] = bytes.GetInt32();
            }

            // Read the valueMaps.
            int offset = inIndexes[IX_VALUE_MAPS_OFFSET];
            int nextOffset = inIndexes[IX_BYTE_TRIES_OFFSET];
            int numInts = (nextOffset - offset) / 4;
            valueMaps = ICUBinary.GetInt32s(bytes, numInts, 0);

            // Read the bytesTries.
            offset = nextOffset;
            nextOffset = inIndexes[IX_NAME_GROUPS_OFFSET];
            int numBytes = nextOffset - offset;
            bytesTries = new byte[numBytes];
            bytes.Get(bytesTries);

            // Read the nameGroups and turn them from ASCII bytes into a .NET string.
            offset = nextOffset;
            nextOffset = inIndexes[IX_RESERVED3_OFFSET];
            numBytes = nextOffset - offset;
            StringBuilder sb = new StringBuilder(numBytes);
            for (int i = 0; i < numBytes; ++i)
            {
                sb.Append((char)bytes.Get());
            }
            nameGroups = sb.ToString();
        }

        private UPropertyAliases()
        {
            ByteBuffer bytes = ICUBinary.GetRequiredData("pnames.icu");
            Load(bytes);
        }

        private int FindProperty(int property)
        {
            int i = 1;  // valueMaps index, initially after numRanges
            for (int numRanges = valueMaps[0]; numRanges > 0; --numRanges)
            {
                // Read and skip the start and limit of this range.
                int start = valueMaps[i];
                int limit = valueMaps[i + 1];
                i += 2;
                if (property < start)
                {
                    break;
                }
                if (property < limit)
                {
                    return i + (property - start) * 2;
                }
                i += (limit - start) * 2;  // Skip all entries for this range.
            }
            return 0;
        }

        private int FindPropertyValueNameGroup(int valueMapIndex, int value)
        {
            if (valueMapIndex == 0)
            {
                return 0;  // The property does not have named values.
            }
            ++valueMapIndex;  // Skip the BytesTrie offset.
            int numRanges = valueMaps[valueMapIndex++];
            if (numRanges < 0x10)
            {
                // Ranges of values.
                for (; numRanges > 0; --numRanges)
                {
                    // Read and skip the start and limit of this range.
                    int start = valueMaps[valueMapIndex];
                    int limit = valueMaps[valueMapIndex + 1];
                    valueMapIndex += 2;
                    if (value < start)
                    {
                        break;
                    }
                    if (value < limit)
                    {
                        return valueMaps[valueMapIndex + value - start];
                    }
                    valueMapIndex += limit - start;  // Skip all entries for this range.
                }
            }
            else
            {
                // List of values.
                int valuesStart = valueMapIndex;
                int nameGroupOffsetsStart = valueMapIndex + numRanges - 0x10;
                do
                {
                    int v = valueMaps[valueMapIndex];
                    if (value < v)
                    {
                        break;
                    }
                    if (value == v)
                    {
                        return valueMaps[nameGroupOffsetsStart + valueMapIndex - valuesStart];
                    }
                } while (++valueMapIndex < nameGroupOffsetsStart);
            }
            return 0;
        }

        // ICU4N specific method for getting property name without throwing exceptions
        private bool TryGetName(int nameGroupsIndex, int nameIndex, out string result)
        {
            result = null;
            int numNames = nameGroups[nameGroupsIndex++];
            if (nameIndex < 0 || numNames <= nameIndex)
            {
                return false;
            }
            // Skip nameIndex names.
            for (; nameIndex > 0; --nameIndex)
            {
                while (0 != nameGroups[nameGroupsIndex++]) { }
            }
            // Find the end of this name.
            int nameStart = nameGroupsIndex;
            while (0 != nameGroups[nameGroupsIndex])
            {
                ++nameGroupsIndex;
            }
            if (nameStart == nameGroupsIndex)
            {
                result = null;  // no name (Property[Value]Aliases.txt has "n/a")
                return true;
            }
            result = nameGroups.Substring(nameStart, nameGroupsIndex - nameStart); // ICU4N: Corrected 2nd parameter
            return true;
        }

        private string GetName(int nameGroupsIndex, int nameIndex)
        {
            string result;
            if (TryGetName(nameGroupsIndex, nameIndex, out result))
            {
                return result;
            }
            throw new IcuArgumentException("Invalid property (value) name choice");
        }

        private static int AsciiToLowercase(int c)
        {
            return 'A' <= c && c <= 'Z' ? c + 0x20 : c;
        }

        // ICU4N specific - ContainsName(BytesTrie trie, ICharSequence name) moved to UPropertyAliases.generated.tt

        //----------------------------------------------------------------
        // Public API

        /// <summary>
        /// public singleton instance
        /// </summary>
        public static UPropertyAliases Instance { get; private set; } = LoadSingletonInstance(); // ICU4N: Avoid static constructor by initializing inline

        private static UPropertyAliases LoadSingletonInstance()
        {
            try
            {
                return new UPropertyAliases();
            }
            catch (IOException e)
            {
                ////CLOVER:OFF
                MissingManifestResourceException mre = new MissingManifestResourceException(
                        "Could not construct UPropertyAliases. Missing pnames.icu", e);
                throw mre;
                ////CLOVER:ON
            }
        }

        /// <summary>
        /// Returns a property name given a <paramref name="property"/> enum.
        /// Multiple names may be available for each property;
        /// the <paramref name="nameChoice"/> selects among them.
        /// </summary>
        public string GetPropertyName(UProperty property, NameChoice nameChoice)
        {
            int valueMapIndex = FindProperty((int)property);
            if (valueMapIndex == 0)
            {
                throw new ArgumentException(
                        "Invalid property enum " + property + " (0x" + string.Format("{0:x2}", (int)property) + ")");
            }
            return GetName(valueMaps[valueMapIndex], (int)nameChoice);
        }

        /// <summary>
        /// Returns a property name given a <paramref name="property"/> enum.
        /// Multiple names may be available for each property;
        /// the <paramref name="nameChoice"/> selects among them.
        /// </summary>
        /// <stable>ICU4N 60.1.0</stable>
        public bool TryGetPropertyName(UProperty property, NameChoice nameChoice, out string result) // ICU4N TODO: Tests
        {
            result = null;
            int valueMapIndex = FindProperty((int)property);
            if (valueMapIndex == 0)
            {
                return false;
            }
            return TryGetName(valueMaps[valueMapIndex], (int)nameChoice, out result);
        }

        /// <summary>
        /// Returns a value name given a <paramref name="property"/> enum and a <paramref name="value"/> enum.
        /// Multiple names may be available for each value;
        /// the <paramref name="nameChoice"/> selects among them.
        /// </summary>
        /// <seealso cref="TryGetPropertyValueName(UProperty, int, NameChoice, out string)"/>
        public string GetPropertyValueName(UProperty property, int value, NameChoice nameChoice) // ICU4N TODO: API - make value into enum ?
        {
            int valueMapIndex = FindProperty((int)property);
            if (valueMapIndex == 0)
            {
                throw new ArgumentException(
                        "Invalid property enum " + property + " (0x" + string.Format("{0:x2}", property) + ")");
            }
            int nameGroupOffset = FindPropertyValueNameGroup(valueMaps[valueMapIndex + 1], value);
            if (nameGroupOffset == 0)
            {
                throw new ArgumentException(
                        "Property " + property + " (0x" + string.Format("{0:x2}", (int)property) +
                        ") does not have named values");
            }
            return GetName(nameGroupOffset, (int)nameChoice);
        }

        // ICU4N specific method for getting the property name without throwing any exceptions
        /// <summary>
        /// Gets a value name given a <paramref name="property"/> enum and a <paramref name="value"/> enum.
        /// Multiple names may be available for each value;
        /// the <paramref name="nameChoice"/> selects among them.
        /// <para/>
        /// This method is equivalent to <see cref="GetPropertyValueName(UProperty, int, NameChoice)"/>
        /// but will return a true/false result rather than throwing exceptions.
        /// </summary>
        /// <seealso cref="GetPropertyValueName(UProperty, int, NameChoice)"/>
         // ICU4N TODO: API - make value into enum ?
        public bool TryGetPropertyValueName(UProperty property, int value, NameChoice nameChoice, out string result) // ICU4N TODO: Tests
        {
            result = null;
            int valueMapIndex = FindProperty((int)property);
            if (valueMapIndex == 0)
            {
                return false;
            }
            int nameGroupOffset = FindPropertyValueNameGroup(valueMaps[valueMapIndex + 1], value);
            if (nameGroupOffset == 0)
            {
                return false;
            }
            return TryGetName(nameGroupOffset, (int)nameChoice, out result);
        }

        // ICU4N specific - GetPropertyOrValueEnum(int bytesTrieOffset, ICharSequence alias) moved to UPropertyAliases.generated.tt

        // ICU4N specific - GetPropertyEnum(ICharSequence alias) moved to UPropertyAliases.generated.tt

        // ICU4N specific - GetPropertyValueEnum(UProperty property, ICharSequence alias) moved to UPropertyAliases.generated.tt

        /// <summary>
        /// Returns a value enum given a property enum and one of its value names. Does not throw.
        /// </summary>
        /// <returns>value enum, or <see cref="UPropertyConstants.Undefined"/> if not defined for that property</returns>
        [Obsolete("ICU4N 60.1.0 - Use TryGetPropertyValueEnum instead.")]
        internal int GetPropertyValueEnumNoThrow(int property, ICharSequence alias) // ICU4N specific - marked internal, since the functionality is obsolete
        {
            int valueMapIndex = FindProperty(property);
            if (valueMapIndex == 0)
            {
                return (int)UPropertyConstants.Undefined;
            }
            valueMapIndex = valueMaps[valueMapIndex + 1];
            if (valueMapIndex == 0)
            {
                return (int)UPropertyConstants.Undefined;
            }
            // valueMapIndex is the start of the property's valueMap,
            // where the first word is the BytesTrie offset.
            return GetPropertyOrValueEnum(valueMaps[valueMapIndex], alias);
        }

        /// <summary>
        /// Compare two property names, returning &lt;0, 0, or >0.  The
        /// comparison is that described as "loose" matching in the
        /// Property*Aliases.txt files.
        /// </summary>
        public static int Compare(string stra, string strb)
        {
            // Note: This implementation is a literal copy of
            // uprv_comparePropertyNames.  It can probably be improved.
            int istra = 0, istrb = 0, rc;
            int cstra = 0, cstrb = 0;
            for (; ; )
            {
                /* Ignore delimiters '-', '_', and ASCII White_Space */
                while (istra < stra.Length)
                {
                    cstra = stra[istra];
                    switch (cstra)
                    {
                        case '-':
                        case '_':
                        case ' ':
                        case '\t':
                        case '\n':
                        case 0xb/*\v*/:
                        case '\f':
                        case '\r':
                            ++istra;
                            continue;
                    }
                    break;
                }

                while (istrb < strb.Length)
                {
                    cstrb = strb[istrb];
                    switch (cstrb)
                    {
                        case '-':
                        case '_':
                        case ' ':
                        case '\t':
                        case '\n':
                        case 0xb/*\v*/:
                        case '\f':
                        case '\r':
                            ++istrb;
                            continue;
                    }
                    break;
                }

                /* If we reach the ends of both strings then they match */
                bool endstra = istra == stra.Length;
                bool endstrb = istrb == strb.Length;
                if (endstra)
                {
                    if (endstrb) return 0;
                    cstra = 0;
                }
                else if (endstrb)
                {
                    cstrb = 0;
                }

                rc = AsciiToLowercase(cstra) - AsciiToLowercase(cstrb);
                if (rc != 0)
                {
                    return rc;
                }

                ++istra;
                ++istrb;
            }
        }
    }
}
