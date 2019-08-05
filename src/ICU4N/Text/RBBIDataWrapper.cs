using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Support.IO;
using System;
using System.IO;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// Internal class used for Rule Based Break Iterators
    /// <para/>
    /// This class provides access to the compiled break rule data, as
    /// it is stored in a .brk file.
    /// </summary>
    internal sealed class RBBIDataWrapper
    {
        //
        // These fields are the ready-to-use compiled rule data, as
        //   read from the file.
        //
        internal RBBIDataHeader fHeader;
        internal short[] fFTable;
        internal short[] fRTable;
        internal short[] fSFTable;
        internal short[] fSRTable;
        internal Trie2 fTrie;
        internal string fRuleSource;
        internal int[] fStatusTable;

        private bool isBigEndian;

        internal static readonly int DATA_FORMAT = 0x42726b20;     // "Brk "
        internal static readonly int FORMAT_VERSION = 0x04000000;  // 4.0.0.0

        private sealed class IsAcceptable : IAuthenticate
        {
            public bool IsDataVersionAcceptable(byte[] version)
            {
                int intVersion = (version[0] << 24) + (version[1] << 16) + (version[2] << 8) + version[3];
                return intVersion == FORMAT_VERSION;
            }
        }
        private static readonly IsAcceptable IS_ACCEPTABLE = new IsAcceptable();

        //
        // Indexes to fields in the ICU4C style binary form of the RBBI Data Header
        //   Used by the rule compiler when flattening the data.
        //
        internal readonly static int DH_SIZE = 24;
        internal readonly static int DH_MAGIC = 0;
        internal readonly static int DH_FORMATVERSION = 1;
        internal readonly static int DH_LENGTH = 2;
        internal readonly static int DH_CATCOUNT = 3;
        internal readonly static int DH_FTABLE = 4;
        internal readonly static int DH_FTABLELEN = 5;
        internal readonly static int DH_RTABLE = 6;
        internal readonly static int DH_RTABLELEN = 7;
        internal readonly static int DH_SFTABLE = 8;
        internal readonly static int DH_SFTABLELEN = 9;
        internal readonly static int DH_SRTABLE = 10;
        internal readonly static int DH_SRTABLELEN = 11;
        internal readonly static int DH_TRIE = 12;
        internal readonly static int DH_TRIELEN = 13;
        internal readonly static int DH_RULESOURCE = 14;
        internal readonly static int DH_RULESOURCELEN = 15;
        internal readonly static int DH_STATUSTABLE = 16;
        internal readonly static int DH_STATUSTABLELEN = 17;


        // Index offsets to the fields in a state table row.
        //    Corresponds to struct RBBIStateTableRow in the C version.
        //
        internal readonly static int ACCEPTING = 0;
        internal readonly static int LOOKAHEAD = 1;
        internal readonly static int TAGIDX = 2;
        internal readonly static int RESERVED = 3;
        internal readonly static int NEXTSTATES = 4;

        // Index offsets to header fields of a state table
        //     struct RBBIStateTable {...   in the C version.
        //
        internal static readonly int NUMSTATES = 0;
        internal static readonly int ROWLEN = 2;
        internal static readonly int FLAGS = 4;
        //ivate static readonly int RESERVED_2 = 6;
        private static readonly int ROW_DATA = 8;

        //  Bit selectors for the "FLAGS" field of the state table header
        //     enum RBBIStateTableFlags in the C version.
        //
        internal readonly static int RBBI_LOOKAHEAD_HARD_BREAK = 1;
        internal readonly static int RBBI_BOF_REQUIRED = 2;

        /**
         * Data Header.  A struct-like class with the fields from the RBBI data file header.
         */
        internal sealed class RBBIDataHeader
        {
            internal int fMagic;         //  == 0xbla0
            internal byte[] fFormatVersion; //  For ICU 3.4 and later.
            internal int fLength;        //  Total length in bytes of this RBBI Data,
                                         //      including all sections, not just the header.
            internal int fCatCount;      //  Number of character categories.

            //
            //  Offsets and sizes of each of the subsections within the RBBI data.
            //  All offsets are bytes from the start of the RBBIDataHeader.
            //  All sizes are in bytes.
            //
            internal int fFTable;         //  forward state transition table.
            internal int fFTableLen;
            internal int fRTable;         //  Offset to the reverse state transition table.
            internal int fRTableLen;
            internal int fSFTable;        //  safe point forward transition table
            internal int fSFTableLen;
            internal int fSRTable;        //  safe point reverse transition table
            internal int fSRTableLen;
            internal int fTrie;           //  Offset to Trie data for character categories
            internal int fTrieLen;
            internal int fRuleSource;     //  Offset to the source for for the break
            internal int fRuleSourceLen;  //    rules.  Stored UChar *.
            internal int fStatusTable;    // Offset to the table of rule status values
            internal int fStatusTableLen;

            public RBBIDataHeader()
            {
                fMagic = 0;
                fFormatVersion = new byte[4];
            }
        }

        /// <summary>
        /// RBBI State Table Indexing Function.  Given a <paramref name="state"/> number, return the
        /// array index of the start of the state table row for that state.
        /// </summary>
        internal int GetRowIndex(int state)
        {
            return ROW_DATA + state * (fHeader.fCatCount + 4);
        }

        internal RBBIDataWrapper()
        {
        }

        /// <summary>
        /// Get an <see cref="RBBIDataWrapper"/> from an InputStream onto a pre-compiled set
        /// of RBBI rules.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        internal static RBBIDataWrapper Get(ByteBuffer bytes)
        {
            RBBIDataWrapper This = new RBBIDataWrapper();

            ICUBinary.ReadHeader(bytes, DATA_FORMAT, IS_ACCEPTABLE);
            This.isBigEndian = bytes.Order == ByteOrder.BIG_ENDIAN;

            // Read in the RBBI data header...
            This.fHeader = new RBBIDataHeader();
            This.fHeader.fMagic = bytes.GetInt32();
            This.fHeader.fFormatVersion[0] = bytes.Get();
            This.fHeader.fFormatVersion[1] = bytes.Get();
            This.fHeader.fFormatVersion[2] = bytes.Get();
            This.fHeader.fFormatVersion[3] = bytes.Get();
            This.fHeader.fLength = bytes.GetInt32();
            This.fHeader.fCatCount = bytes.GetInt32();
            This.fHeader.fFTable = bytes.GetInt32();
            This.fHeader.fFTableLen = bytes.GetInt32();
            This.fHeader.fRTable = bytes.GetInt32();
            This.fHeader.fRTableLen = bytes.GetInt32();
            This.fHeader.fSFTable = bytes.GetInt32();
            This.fHeader.fSFTableLen = bytes.GetInt32();
            This.fHeader.fSRTable = bytes.GetInt32();
            This.fHeader.fSRTableLen = bytes.GetInt32();
            This.fHeader.fTrie = bytes.GetInt32();
            This.fHeader.fTrieLen = bytes.GetInt32();
            This.fHeader.fRuleSource = bytes.GetInt32();
            This.fHeader.fRuleSourceLen = bytes.GetInt32();
            This.fHeader.fStatusTable = bytes.GetInt32();
            This.fHeader.fStatusTableLen = bytes.GetInt32();
            ICUBinary.SkipBytes(bytes, 6 * 4);    // uint32_t  fReserved[6];


            if (This.fHeader.fMagic != 0xb1a0 || !IS_ACCEPTABLE.IsDataVersionAcceptable(This.fHeader.fFormatVersion))
            {
                throw new IOException("Break Iterator Rule Data Magic Number Incorrect, or unsupported data version.");
            }

            // Current position in the buffer.
            int pos = 24 * 4;     // offset of end of header, which has 24 fields, all int32_t (4 bytes)

            //
            // Read in the Forward state transition table as an array of shorts.
            //

            //   Quick Sanity Check
            if (This.fHeader.fFTable < pos || This.fHeader.fFTable > This.fHeader.fLength)
            {
                throw new IOException("Break iterator Rule data corrupt");
            }

            //    Skip over any padding preceding this table
            ICUBinary.SkipBytes(bytes, This.fHeader.fFTable - pos);
            pos = This.fHeader.fFTable;

            This.fFTable = ICUBinary.GetShorts(
                    bytes, This.fHeader.fFTableLen / 2, This.fHeader.fFTableLen & 1);
            pos += This.fHeader.fFTableLen;

            //
            // Read in the Reverse state table
            //

            // Skip over any padding in the file
            ICUBinary.SkipBytes(bytes, This.fHeader.fRTable - pos);
            pos = This.fHeader.fRTable;

            // Create & fill the table itself.
            This.fRTable = ICUBinary.GetShorts(
                    bytes, This.fHeader.fRTableLen / 2, This.fHeader.fRTableLen & 1);
            pos += This.fHeader.fRTableLen;

            //
            // Read in the Safe Forward state table
            //
            if (This.fHeader.fSFTableLen > 0)
            {
                // Skip over any padding in the file
                ICUBinary.SkipBytes(bytes, This.fHeader.fSFTable - pos);
                pos = This.fHeader.fSFTable;

                // Create & fill the table itself.
                This.fSFTable = ICUBinary.GetShorts(
                        bytes, This.fHeader.fSFTableLen / 2, This.fHeader.fSFTableLen & 1);
                pos += This.fHeader.fSFTableLen;
            }

            //
            // Read in the Safe Reverse state table
            //
            if (This.fHeader.fSRTableLen > 0)
            {
                // Skip over any padding in the file
                ICUBinary.SkipBytes(bytes, This.fHeader.fSRTable - pos);
                pos = This.fHeader.fSRTable;

                // Create & fill the table itself.
                This.fSRTable = ICUBinary.GetShorts(
                        bytes, This.fHeader.fSRTableLen / 2, This.fHeader.fSRTableLen & 1);
                pos += This.fHeader.fSRTableLen;
            }

            // Rule Compatibility Hacks
            //    If a rule set includes reverse rules but does not explicitly include safe reverse rules,
            //    the reverse rules are to be treated as safe reverse rules.

            if (This.fSRTable == null && This.fRTable != null)
            {
                This.fSRTable = This.fRTable;
                This.fRTable = null;
            }

            //
            // Unserialize the Character categories TRIE
            //     Because we can't be absolutely certain where the Trie deserialize will
            //     leave the buffer, leave position unchanged.
            //     The seek to the start of the next item following the TRIE will get us
            //     back in sync.
            //
            ICUBinary.SkipBytes(bytes, This.fHeader.fTrie - pos);  // seek buffer from end of
            pos = This.fHeader.fTrie;               // previous section to the start of the trie

            bytes.Mark();                           // Mark position of start of TRIE in the input
                                                    //  and tell Java to keep the mark valid so long
                                                    //  as we don't go more than 100 bytes past the
                                                    //  past the end of the TRIE.

            This.fTrie = Trie2.CreateFromSerialized(bytes);  // Deserialize the TRIE, leaving buffer
                                                             //  at an unknown position, preceding the
                                                             //  padding between TRIE and following section.

            bytes.Reset();                          // Move buffer back to marked position at
                                                    //   the start of the serialized TRIE.  Now our
                                                    //   "pos" variable and the buffer are in
                                                    //   agreement.

            //
            // Read the Rule Status Table
            //
            if (pos > This.fHeader.fStatusTable)
            {
                throw new IOException("Break iterator Rule data corrupt");
            }
            ICUBinary.SkipBytes(bytes, This.fHeader.fStatusTable - pos);
            pos = This.fHeader.fStatusTable;
            This.fStatusTable = ICUBinary.GetInts(
                    bytes, This.fHeader.fStatusTableLen / 4, This.fHeader.fStatusTableLen & 3);
            pos += This.fHeader.fStatusTableLen;

            //
            // Put the break rule source into a String
            //
            if (pos > This.fHeader.fRuleSource)
            {
                throw new IOException("Break iterator Rule data corrupt");
            }
            ICUBinary.SkipBytes(bytes, This.fHeader.fRuleSource - pos);
            pos = This.fHeader.fRuleSource;
            This.fRuleSource = ICUBinary.GetString(
                    bytes, This.fHeader.fRuleSourceLen / 2, This.fHeader.fRuleSourceLen & 1);

            if (RuleBasedBreakIterator.fDebugEnv != null && RuleBasedBreakIterator.fDebugEnv.IndexOf("data", StringComparison.Ordinal) >= 0)
            {
                This.Dump(Console.Out);
            }
            return This;
        }

        ///CLOVER:OFF
        //  Getters for fields from the state table header
        //
        private int GetStateTableNumStates(short[] table)
        {
            if (isBigEndian)
            {
                return (table[NUMSTATES] << 16) | (table[NUMSTATES + 1] & 0xffff);
            }
            else
            {
                return (table[NUMSTATES + 1] << 16) | (table[NUMSTATES] & 0xffff);
            }
        }
        ///CLOVER:ON

        internal int GetStateTableFlags(short[] table)
        {
            // This works for up to 15 flags bits.
            return table[isBigEndian ? FLAGS + 1 : FLAGS];
        }

        //CLOVER:OFF
        /// <summary>Debug function to display the break iterator data.</summary>
        internal void Dump(TextWriter output)
        {
            if (fFTable.Length == 0)
            {
                // There is no table. Fail early for testing purposes.
                throw new InvalidOperationException();
            }
            output.WriteLine("RBBI Data Wrapper dump ...");
            output.WriteLine();
            output.WriteLine("Forward State Table");
            DumpTable(output, fFTable);
            output.WriteLine("Reverse State Table");
            DumpTable(output, fRTable);
            output.WriteLine("Forward Safe Points Table");
            DumpTable(output, fSFTable);
            output.WriteLine("Reverse Safe Points Table");
            DumpTable(output, fSRTable);

            DumpCharCategories(output);
            output.WriteLine("Source Rules: " + fRuleSource);

        }
        //CLOVER:ON

        //CLOVER:OFF
        /// <summary>Fixed width int-to-string conversion.</summary>
        static public string Int32ToString(int n, int width)
        {
            StringBuilder dest = new StringBuilder(width);
            dest.Append(n);
            while (dest.Length < width)
            {
                dest.Insert(0, ' ');
            }
            return dest.ToString();
        }
        //CLOVER:ON

        //CLOVER:OFF
        /// <summary>Fixed width int-to-string conversion.</summary>
        static public string Int32ToHexString(int n, int width)
        {
            StringBuilder dest = new StringBuilder(width);
            dest.Append((n).ToHexString());
            while (dest.Length < width)
            {
                dest.Insert(0, ' ');
            }
            return dest.ToString();
        }
        //CLOVER:ON

        //CLOVER:OFF
        /// <summary>Dump a state table.  (A full set of RBBI rules has 4 state tables.)</summary>
        private void DumpTable(TextWriter output, short[] table)
        {
            if (table == null || table.Length == 0)
            {
                output.WriteLine("  -- null -- ");
            }
            else
            {
                int n;
                int state;
                StringBuilder header = new StringBuilder(" Row  Acc Look  Tag");
                for (n = 0; n < fHeader.fCatCount; n++)
                {
                    header.Append(Int32ToString(n, 5));
                }
                output.WriteLine(header.ToString());
                for (n = 0; n < header.Length; n++)
                {
                    output.Write("-");
                }
                output.WriteLine();
                for (state = 0; state < GetStateTableNumStates(table); state++)
                {
                    DumpRow(output, table, state);
                }
                output.WriteLine();
            }
        }
        //CLOVER:ON

        //CLOVER:OFF
        /// <summary>
        /// Dump (for debug) a single row of an RBBI state table
        /// </summary>
        private void DumpRow(TextWriter output, short[] table, int state)
        {
            StringBuilder dest = new StringBuilder(fHeader.fCatCount * 5 + 20);
            dest.Append(Int32ToString(state, 4));
            int row = GetRowIndex(state);
            if (table[row + ACCEPTING] != 0)
            {
                dest.Append(Int32ToString(table[row + ACCEPTING], 5));
            }
            else
            {
                dest.Append("     ");
            }
            if (table[row + LOOKAHEAD] != 0)
            {
                dest.Append(Int32ToString(table[row + LOOKAHEAD], 5));
            }
            else
            {
                dest.Append("     ");
            }
            dest.Append(Int32ToString(table[row + TAGIDX], 5));

            for (int col = 0; col < fHeader.fCatCount; col++)
            {
                dest.Append(Int32ToString(table[row + NEXTSTATES + col], 5));
            }

            output.WriteLine(dest);
        }
        ///CLOVER:ON

        ///CLOVER:OFF
        private void DumpCharCategories(TextWriter output)
        {
            int n = fHeader.fCatCount;
            string[] catStrings = new string[n + 1];
            int rangeStart = 0;
            int rangeEnd = 0;
            int lastCat = -1;
            int char32;
            int category;
            int[] lastNewline = new int[n + 1];

            for (category = 0; category <= fHeader.fCatCount; category++)
            {
                catStrings[category] = "";
            }
            output.WriteLine("\nCharacter Categories");
            output.WriteLine("--------------------");
            for (char32 = 0; char32 <= 0x10ffff; char32++)
            {
                category = fTrie.Get(char32);
                category &= ~0x4000;            // Mask off dictionary bit.
                if (category < 0 || category > fHeader.fCatCount)
                {
                    output.WriteLine("Error, bad category " + (category).ToHexString() +
                        " for char " + (char32).ToHexString());
                    break;
                }
                if (category == lastCat)
                {
                    rangeEnd = char32;
                }
                else
                {
                    if (lastCat >= 0)
                    {
                        if (catStrings[lastCat].Length > lastNewline[lastCat] + 70)
                        {
                            lastNewline[lastCat] = catStrings[lastCat].Length + 10;
                            catStrings[lastCat] += "\n       ";
                        }

                        catStrings[lastCat] += " " + (rangeStart).ToHexString();
                        if (rangeEnd != rangeStart)
                        {
                            catStrings[lastCat] += "-" + (rangeEnd).ToHexString();
                        }
                    }
                    lastCat = category;
                    rangeStart = rangeEnd = char32;
                }
            }
            catStrings[lastCat] += " " + (rangeStart).ToHexString();
            if (rangeEnd != rangeStart)
            {
                catStrings[lastCat] += "-" + (rangeEnd).ToHexString();
            }

            for (category = 0; category <= fHeader.fCatCount; category++)
            {
                output.WriteLine(Int32ToString(category, 5) + "  " + catStrings[category]);
            }
            output.WriteLine();
        }
        //CLOVER:ON

        /*static RBBIDataWrapper get(String name) throws IOException {
            String  fullName = "data/" + name;
            InputStream is = ICUData.getRequiredStream(fullName);
            return get(is);
        }

        public static void main(String[] args) {
            String s;
            if (args.length == 0) {
                s = "char";
            } else {
                s = args[0];
            }
            System.out.println("RBBIDataWrapper.main(" + s + ") ");

            String versionedName = ICUResourceBundle.ICU_BUNDLE+"/"+ s + ".brk";

            try {
                RBBIDataWrapper This = RBBIDataWrapper.get(versionedName);
                This.dump();
            }
           catch (Exception e) {
               System.out.println("Exception: " + e.toString());
           }

        }*/
    }
}
