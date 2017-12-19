using ICU4N.Impl;
using ICU4N.Support.IO;
using ICU4N.Util;

namespace ICU4N.Text
{
    internal static class DictionaryData
    {
        public static readonly int TRIE_TYPE_BYTES = 0;
        public static readonly int TRIE_TYPE_UCHARS = 1;
        public static readonly int TRIE_TYPE_MASK = 7;
        public static readonly int TRIE_HAS_VALUES = 8;
        public static readonly int TRANSFORM_NONE = 0;
        public static readonly int TRANSFORM_TYPE_OFFSET = 0x1000000;
        public static readonly int TRANSFORM_TYPE_MASK = 0x7f000000;
        public static readonly int TRANSFORM_OFFSET_MASK = 0x1fffff;

        public static readonly int IX_STRING_TRIE_OFFSET = 0;
        public static readonly int IX_RESERVED1_OFFSET = 1;
        public static readonly int IX_RESERVED2_OFFSET = 2;
        public static readonly int IX_TOTAL_SIZE = 3;
        public static readonly int IX_TRIE_TYPE = 4;
        public static readonly int IX_TRANSFORM = 5;
        public static readonly int IX_RESERVED6 = 6;
        public static readonly int IX_RESERVED7 = 7;
        public static readonly int IX_COUNT = 8;

        private static readonly int DATA_FORMAT_ID = 0x44696374;

        public static DictionaryMatcher LoadDictionaryFor(string dictType)
        {
            ICUResourceBundle rb = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.ICU_BRKITR_BASE_NAME); // com/ibm/icu/impl/data/icudt60b/brkitr
            string dictFileName = rb.GetStringWithFallback("dictionaries/" + dictType);

            // ICU4N TODO: Possibly rename the above and use this syntax instead...?
            //var rm = new ResourceManager(ICUData.ICU_BRKITR_BASE_NAME, typeof(DictionaryData).GetTypeInfo().Assembly);
            //string dictFileName = rm.GetString("dictionaries_" + dictType);

            dictFileName = ICUData.ICU_BRKITR_NAME + '/' + dictFileName;
            ByteBuffer bytes = ICUBinary.GetRequiredData(dictFileName);
            ICUBinary.ReadHeader(bytes, DATA_FORMAT_ID, null);
            int[] indexes = new int[IX_COUNT];
            // TODO: read indexes[IX_STRING_TRIE_OFFSET] first, then read a variable-length indexes[]
            for (int i = 0; i < IX_COUNT; i++)
            {
                indexes[i] = bytes.GetInt32();
            }
            int offset = indexes[IX_STRING_TRIE_OFFSET];
            Assert.Assrt(offset >= (4 * IX_COUNT));
            if (offset > (4 * IX_COUNT))
            {
                int diff = offset - (4 * IX_COUNT);
                ICUBinary.SkipBytes(bytes, diff);
            }
            int trieType = indexes[IX_TRIE_TYPE] & TRIE_TYPE_MASK;
            int totalSize = indexes[IX_TOTAL_SIZE] - offset;
            DictionaryMatcher m = null;
            if (trieType == TRIE_TYPE_BYTES)
            {
                int transform = indexes[IX_TRANSFORM];
                byte[] data = new byte[totalSize];
                bytes.Get(data);
                m = new BytesDictionaryMatcher(data, transform);
            }
            else if (trieType == TRIE_TYPE_UCHARS)
            {
                Assert.Assrt(totalSize % 2 == 0);
                string data = ICUBinary.GetString(bytes, totalSize / 2, totalSize & 1);
                m = new CharsDictionaryMatcher(data);
            }
            else
            {
                m = null;
            }
            return m;
        }
    }
}
