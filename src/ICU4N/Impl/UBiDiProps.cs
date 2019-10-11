using ICU4N.Globalization;
using ICU4N.Support;
using ICU4N.Support.IO;
using ICU4N.Text;
using ICU4N.Util;
using System.IO;

namespace ICU4N.Impl
{
    /// <summary>
    /// Low-level Unicode bidi/shaping properties access.
    /// .NET port of ubidi_props.h/.c.
    /// </summary>
    /// <author>Markus W. Scherer</author>
    /// <created>2005jan16</created>
    public sealed class UBiDiProps
    {
        // constructors etc. --------------------------------------------------- ***

        // port of ubidi_openProps()
        private UBiDiProps()
        {
            ByteBuffer bytes = ICUBinary.GetData(DATA_FILE_NAME);
            ReadData(bytes);
        }

        private void ReadData(ByteBuffer bytes)
        {
            // read the header
            ICUBinary.ReadHeader(bytes, FMT, new IsAcceptable());

            // read indexes[]
            int i, count;
            count = bytes.GetInt32();
            if (count < IX_TOP)
            {
                throw new IOException("indexes[0] too small in " + DATA_FILE_NAME);
            }
            indexes = new int[count];

            indexes[0] = count;
            for (i = 1; i < count; ++i)
            {
                indexes[i] = bytes.GetInt32();
            }

            // read the trie
            trie = Trie2_16.CreateFromSerialized(bytes);
            int expectedTrieLength = indexes[IX_TRIE_SIZE];
            int trieLength = trie.SerializedLength;
            if (trieLength > expectedTrieLength)
            {
                throw new IOException(DATA_FILE_NAME + ": not enough bytes for the trie");
            }
            // skip padding after trie bytes
            ICUBinary.SkipBytes(bytes, expectedTrieLength - trieLength);

            // read mirrors[]
            count = indexes[IX_MIRROR_LENGTH];
            if (count > 0)
            {
                mirrors = ICUBinary.GetInt32s(bytes, count, 0);
            }

            // read jgArray[]
            count = indexes[IX_JG_LIMIT] - indexes[IX_JG_START];
            jgArray = new byte[count];
            bytes.Get(jgArray);

            // read jgArray2[]
            count = indexes[IX_JG_LIMIT2] - indexes[IX_JG_START2];
            jgArray2 = new byte[count];
            bytes.Get(jgArray2);
        }

        // implement ICUBinary.Authenticate
        private sealed class IsAcceptable : IAuthenticate
        {
            public bool IsDataVersionAcceptable(byte[] version)
            {
                return version[0] == 2;
            }
        }

        // set of property starts for UnicodeSet ------------------------------- ***

        public void AddPropertyStarts(UnicodeSet set)
        {
            int i, length;
            int c, start, limit;

            byte prev, jg;

            /* add the start code point of each same-value range of the trie */
            using (var trieIterator = trie.GetEnumerator())
            {
                Trie2Range range;
                while (trieIterator.MoveNext() && !(range = trieIterator.Current).IsLeadSurrogate)
                {
                    set.Add(range.StartCodePoint);
                }
            }

            /* add the code points from the bidi mirroring table */
            length = indexes[IX_MIRROR_LENGTH];
            for (i = 0; i < length; ++i)
            {
                c = GetMirrorCodePoint(mirrors[i]);
                set.Add(c, c + 1);
            }

            /* add the code points from the Joining_Group array where the value changes */
            start = indexes[IX_JG_START];
            limit = indexes[IX_JG_LIMIT];
            byte[] jga = jgArray;
            for (; ; )
            {
                length = limit - start;
                prev = 0;
                for (i = 0; i < length; ++i)
                {
                    jg = jga[i];
                    if (jg != prev)
                    {
                        set.Add(start);
                        prev = jg;
                    }
                    ++start;
                }
                if (prev != 0)
                {
                    /* add the limit code point if the last value was not 0 (it is now start==limit) */
                    set.Add(limit);
                }
                if (limit == indexes[IX_JG_LIMIT])
                {
                    /* switch to the second Joining_Group range */
                    start = indexes[IX_JG_START2];
                    limit = indexes[IX_JG_LIMIT2];
                    jga = jgArray2;
                }
                else
                {
                    break;
                }
            }

            /* add code points with hardcoded properties, plus the ones following them */

            /* (none right now) */
        }

        // property access functions ------------------------------------------- ***

        public int GetMaxValue(UProperty which)
        {
            int max;

            max = indexes[IX_MAX_VALUES];
            switch (which)
            {
                case UProperty.Bidi_Class:
                    return (max & CLASS_MASK);
                case UProperty.Joining_Group:
                    return (max & MAX_JG_MASK) >> MAX_JG_SHIFT;
                case UProperty.Joining_Type:
                    return (max & JT_MASK) >> JT_SHIFT;
                case UProperty.Bidi_Paired_Bracket_Type:
                    return (max & BPT_MASK) >> BPT_SHIFT;
                default:
                    return -1; /* undefined */
            }
        }

        public UCharacterDirection GetClass(int c)
        {
            return (UCharacterDirection)GetClassFromProps(trie.Get(c));
        }

        public bool IsMirrored(int c)
        {
            return GetFlagFromProps(trie.Get(c), IS_MIRRORED_SHIFT);
        }

        private int GetMirror(int c, int props)
        {
            int delta = GetMirrorDeltaFromProps(props);
            if (delta != ESC_MIRROR_DELTA)
            {
                return c + delta;
            }
            else
            {
                /* look for mirror code point in the mirrors[] table */
                int m;
                int i, length;
                int c2;

                length = indexes[IX_MIRROR_LENGTH];

                /* linear search */
                for (i = 0; i < length; ++i)
                {
                    m = mirrors[i];
                    c2 = GetMirrorCodePoint(m);
                    if (c == c2)
                    {
                        /* found c, return its mirror code point using the index in m */
                        return GetMirrorCodePoint(mirrors[GetMirrorIndex(m)]);
                    }
                    else if (c < c2)
                    {
                        break;
                    }
                }

                /* c not found, return it itself */
                return c;
            }
        }

        public int GetMirror(int c)
        {
            int props = trie.Get(c);
            return GetMirror(c, props);
        }

        public bool IsBidiControl(int c)
        {
            return GetFlagFromProps(trie.Get(c), BIDI_CONTROL_SHIFT);
        }

        public bool IsJoinControl(int c)
        {
            return GetFlagFromProps(trie.Get(c), JOIN_CONTROL_SHIFT);
        }

        public int GetJoiningType(int c)
        {
            return (trie.Get(c) & JT_MASK) >> JT_SHIFT;
        }

        public int GetJoiningGroup(int c)
        {
            int start, limit;

            start = indexes[IX_JG_START];
            limit = indexes[IX_JG_LIMIT];
            if (start <= c && c < limit)
            {
                return jgArray[c - start] & 0xff;
            }
            start = indexes[IX_JG_START2];
            limit = indexes[IX_JG_LIMIT2];
            if (start <= c && c < limit)
            {
                return jgArray2[c - start] & 0xff;
            }
            return UChar.JoiningGroup.NoJoiningGroup;
        }

        public int GetPairedBracketType(int c)
        {
            return (trie.Get(c) & BPT_MASK) >> BPT_SHIFT;
        }

        public int GetPairedBracket(int c)
        {
            int props = trie.Get(c);
            if ((props & BPT_MASK) == 0)
            {
                return c;
            }
            else
            {
                return GetMirror(c, props);
            }
        }

        // data members -------------------------------------------------------- ***
        private int[] indexes;
        private int[] mirrors;
        private byte[] jgArray;
        private byte[] jgArray2;

        private Trie2_16 trie;

        // data format constants ----------------------------------------------- ***
        private const string DATA_NAME = "ubidi";
        private const string DATA_TYPE = "icu";
        private const string DATA_FILE_NAME = DATA_NAME + "." + DATA_TYPE;

        /* format "BiDi" */
        private const int FMT = 0x42694469;

        /* indexes into indexes[] */
        //private const int IX_INDEX_TOP=0;
        //private const int IX_LENGTH=1;
        private const int IX_TRIE_SIZE = 2;
        private const int IX_MIRROR_LENGTH = 3;

        private const int IX_JG_START = 4;
        private const int IX_JG_LIMIT = 5;
        private const int IX_JG_START2 = 6;  /* new in format version 2.2, ICU 54 */
        private const int IX_JG_LIMIT2 = 7;

        private const int IX_MAX_VALUES = 15;
        private const int IX_TOP = 16;

        // definitions for 16-bit bidi/shaping properties word ----------------- ***

        /* CLASS_SHIFT=0, */     /* bidi class: 5 bits (4..0) */
        private const int JT_SHIFT = 5;           /* joining type: 3 bits (7..5) */

        private const int BPT_SHIFT = 8;          /* Bidi_Paired_Bracket_Type(bpt): 2 bits (9..8) */

        private const int JOIN_CONTROL_SHIFT = 10;
        private const int BIDI_CONTROL_SHIFT = 11;

        private const int IS_MIRRORED_SHIFT = 12;         /* 'is mirrored' */
        private const int MIRROR_DELTA_SHIFT = 13;        /* bidi mirroring delta: 3 bits (15..13) */

        private const int MAX_JG_SHIFT = 16;              /* max JG value in indexes[MAX_VALUES_INDEX] bits 23..16 */

        private const int CLASS_MASK = 0x0000001f;
        private const int JT_MASK = 0x000000e0;
        private const int BPT_MASK = 0x00000300;

        private const int MAX_JG_MASK = 0x00ff0000;

        private static int GetClassFromProps(int props)
        {
            return props & CLASS_MASK;
        }
        private static bool GetFlagFromProps(int props, int shift)
        {
            return ((props >> shift) & 1) != 0;
        }
        private static int GetMirrorDeltaFromProps(int props)
        {
            return (short)props >> MIRROR_DELTA_SHIFT;
        }

        private const int ESC_MIRROR_DELTA = -4;
        //private const int MIN_MIRROR_DELTA=-3;
        //private const int MAX_MIRROR_DELTA=3;

        // definitions for 32-bit mirror table entry --------------------------- ***

        /* the source Unicode code point takes 21 bits (20..0) */
        private const int MIRROR_INDEX_SHIFT = 21;
        //private const int MAX_MIRROR_INDEX=0x7ff;

        private static int GetMirrorCodePoint(int m)
        {
            return m & 0x1fffff;
        }
        private static int GetMirrorIndex(int m)
        {
            return m.TripleShift(MIRROR_INDEX_SHIFT);
        }

        /// <summary>
        /// public singleton instance
        /// </summary>
        public static UBiDiProps Instance { get; private set; } = LoadSingletonInstance(); // ICU4N: Avoid static constructor by initializing inline

        // This static initializer block must be placed after
        // other static member initialization
        private static UBiDiProps LoadSingletonInstance()
        {
            try
            {
                return new UBiDiProps();
            }
            catch (IOException e)
            {
                throw new ICUUncheckedIOException(e);
            }
        }
    }
}
