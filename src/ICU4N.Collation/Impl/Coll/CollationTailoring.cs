using ICU4N.Globalization;
using ICU4N.Text;
using ICU4N.Util;
using System.Collections.Generic;
using System.Diagnostics;

namespace ICU4N.Impl.Coll
{
    /// <summary>
    /// Collation tailoring data &amp; settings.
    /// This is a container of values for a collation tailoring
    /// built from rules or deserialized from binary data.
    /// <para/>
    /// It is logically immutable: Do not modify its values.
    /// The fields are public for convenience.
    /// </summary>
    public sealed class CollationTailoring
    {
        internal CollationTailoring(SharedObject.Reference<CollationSettings> baseSettings)
        {
            if (baseSettings != null)
            {
                Debug.Assert(baseSettings.ReadOnly.ReorderCodes.Length == 0);
                Debug.Assert(baseSettings.ReadOnly.ReorderTable == null);
                Debug.Assert(baseSettings.ReadOnly.minHighNoReorder == 0);
                settings = (SharedObject.Reference<CollationSettings>)baseSettings.Clone();
            }
            else
            {
                settings = new SharedObject.Reference<CollationSettings>(new CollationSettings());
            }
        }

        internal void EnsureOwnedData()
        {
            if (ownedData == null)
            {
                Normalizer2Impl nfcImpl = Norm2AllModes.GetNFCInstance().Impl;
                ownedData = new CollationData(nfcImpl);
            }
            data = ownedData;
        }

        /** Not thread-safe, call only before sharing. */
        internal void SetRules(string r)
        {
            Debug.Assert(rules == null && rulesResource == null);
            rules = r;
        }
        /** Not thread-safe, call only before sharing. */
        internal void SetRulesResource(UResourceBundle res)
        {
            Debug.Assert(rules == null && rulesResource == null);
            rulesResource = res;
        }
        public string GetRules()
        {
            if (rules != null)
            {
                return rules;
            }
            if (rulesResource != null)
            {
                return rulesResource.GetString();
            }
            return "";
        }

        internal static VersionInfo MakeBaseVersion(VersionInfo ucaVersion)
        {
            return VersionInfo.GetInstance(
                    VersionInfo.CollationBuilderVersion.Major,
                    (ucaVersion.Major << 3) + ucaVersion.Minor,
                    ucaVersion.Milli << 6,
                    0);
        }
        internal void SetVersion(int baseVersion, int rulesVersion)
        {
            // See comments for version field.
            int r = (rulesVersion >> 16) & 0xff00;
            int s = (rulesVersion >> 16) & 0xff;
            int t = (rulesVersion >> 8) & 0xff;
            int q = rulesVersion & 0xff;
            version = (VersionInfo.CollationBuilderVersion.Major << 24) |
                    (baseVersion & 0xffc000) |  // UCA version u.v.w
                    ((r + (r >> 6)) & 0x3f00) |
                    (((s << 3) + (s >> 5) + t + (q << 4) + (q >> 4)) & 0xff);
        }
        internal int GetUCAVersion()
        {
            // Version second byte/bits 23..16 to bits 11..4,
            // third byte/bits 15..14 to bits 1..0.
            return ((version >> 12) & 0xff0) | ((version >> 14) & 3);
        }

        // data for sorting etc.
        private CollationData data;  // == base data or ownedData
        private SharedObject.Reference<CollationSettings> settings;  // reference-counted
                                                                    // In Java, deserialize the rules string from the resource bundle
                                                                    // only when it is used. (It can be large and is rarely used.)
        private string rules;
        private UResourceBundle rulesResource;
        // The locale is null (C++: bogus) when built from rules or constructed from a binary blob.
        // It can then be set by the service registration code which is thread-safe.
        private UCultureInfo actualCulture = UCultureInfo.InvariantCulture;
        // UCA version u.v.w & rules version r.s.t.q:
        // version[0]: builder version (runtime version is mixed in at runtime)
        // version[1]: bits 7..3=u, bits 2..0=v
        // version[2]: bits 7..6=w, bits 5..0=r
        // version[3]= (s<<5)+(s>>3)+t+(q<<4)+(q>>4)
        private int version = 0;

        // owned objects
        private CollationData ownedData;
        private Trie2_32 trie;
        private UnicodeSet unsafeBackwardSet;
        internal IDictionary<int, int> maxExpansions;

        /*
         * Not Cloneable: A CollationTailoring cannot be copied.
         * It is immutable, and the data trie cannot be copied either.
         */

        internal CollationData OwnedData
        {
            get => ownedData;
            set => ownedData = value;
        }

        internal Trie2_32 Trie
        {
            get => trie;
            set => trie = value;
        }

        internal UnicodeSet UnsafeBackwardSet
        {
            get => unsafeBackwardSet;
            set => unsafeBackwardSet = value;
        }

        public CollationData Data
        {
            get => data;
            set => data = value;
        }

        public SharedObject.Reference<CollationSettings> Settings
        {
            get => settings;
            set => settings = value;
        }

        public UCultureInfo ActualCulture
        {
            get => actualCulture;
            set => actualCulture = value;
        }

        public int Version
        {
            get => version;
            set => version = value;
        }

        public IDictionary<int, int> MaxExpansions
        {
            get => maxExpansions;
            set => maxExpansions = value;
        }
    }
}
