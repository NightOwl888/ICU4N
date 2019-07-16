using ICU4N.Support.Collections;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Resources;
using System.Text.RegularExpressions;

namespace ICU4N.Impl.Locale
{
    internal static class SpecialTypeExtensions
    {
        private static IDictionary<KeyTypeData.SpecialType, KeyTypeData.SpecialTypeHandler> map =
            new Dictionary<KeyTypeData.SpecialType, KeyTypeData.SpecialTypeHandler>
        {
            { KeyTypeData.SpecialType.CODEPOINTS, new KeyTypeData.CodepointsTypeHandler() },
            { KeyTypeData.SpecialType.REORDER_CODE, new KeyTypeData.ReorderCodeTypeHandler() },
            { KeyTypeData.SpecialType.RG_KEY_VALUE, new KeyTypeData.RgKeyValueTypeHandler() },
            { KeyTypeData.SpecialType.SUBDIVISION_CODE, new KeyTypeData.SubdivisionKeyValueTypeHandler() },
            { KeyTypeData.SpecialType.PRIVATE_USE, new KeyTypeData.PrivateUseKeyValueTypeHandler() },
        };

        public static KeyTypeData.SpecialTypeHandler GetHandler(this KeyTypeData.SpecialType specialType)
        {
            return map.Get(specialType);
        }
    }

    public class KeyTypeData
    {
        public enum ValueType // ICU4N TODO: API de-nest and rename KeyTypeDataValueType
        {
            single, multiple, incremental, any // ICU4N TODO: API rename elements for .NET conventions
        }

        internal abstract class SpecialTypeHandler
        {
            internal abstract bool IsWellFormed(string value); // doesn't test validity, just whether it is well formed.
            internal string Canonicalize(string value)
            {
                return AsciiUtil.ToLower(value);
            }
        }

        internal class CodepointsTypeHandler : SpecialTypeHandler
        {
            private static readonly Regex pat = new Regex("^[0-9a-fA-F]{4,6}(-[0-9a-fA-F]{4,6})*$", RegexOptions.Compiled);

            internal override bool IsWellFormed(string value)
            {
                return pat.IsMatch(value);
            }
        }

        internal class ReorderCodeTypeHandler : SpecialTypeHandler
        {
            private static readonly Regex pat = new Regex("^[a-zA-Z]{3,8}(-[a-zA-Z]{3,8})*$", RegexOptions.Compiled);

            internal override bool IsWellFormed(string value)
            {
                return pat.IsMatch(value);
            }
        }

        internal class RgKeyValueTypeHandler : SpecialTypeHandler
        {
            private static readonly Regex pat = new Regex("^([a-zA-Z]{2}|[0-9]{3})[zZ]{4}$", RegexOptions.Compiled);

            internal override bool IsWellFormed(string value)
            {
                return pat.IsMatch(value);
            }
        }

        internal class SubdivisionKeyValueTypeHandler : SpecialTypeHandler
        {
            private static readonly Regex pat = new Regex("^([a-zA-Z]{2}|[0-9]{3})$", RegexOptions.Compiled);

            internal override bool IsWellFormed(string value)
            {
                return pat.IsMatch(value);
            }
        }

        internal class PrivateUseKeyValueTypeHandler : SpecialTypeHandler
        {
            private static readonly Regex pat = new Regex("^[a-zA-Z0-9]{3,8}(-[a-zA-Z0-9]{3,8})*$", RegexOptions.Compiled);

            internal override bool IsWellFormed(string value)
            {
                return pat.IsMatch(value);
            }
        }

        internal enum SpecialType // ICU4N TODO: API - rename values for .NET conventions
        {
            CODEPOINTS,
            REORDER_CODE,
            RG_KEY_VALUE,
            SUBDIVISION_CODE,
            PRIVATE_USE
        }

        private class KeyData
        {
            public string LegacyId { get; private set; }
            public string BcpId { get; private set; }
            public IDictionary<string, Type> TypeMap { get; private set; }
            public ISet<SpecialType> SpecialTypes { get; private set; }

            internal KeyData(string legacyId, string bcpId, IDictionary<string, Type> typeMap,
                    ISet<SpecialType> specialTypes)
            {
                this.LegacyId = legacyId;
                this.BcpId = bcpId;
                this.TypeMap = typeMap;
                this.SpecialTypes = specialTypes;
            }
        }

        private class Type
        {
            internal string legacyId; // ICU4N TODO: API - change to properties
            internal string bcpId;

            internal Type(string legacyId, string bcpId)
            {
                this.legacyId = legacyId;
                this.bcpId = bcpId;
            }
        }

        public static string ToBcpKey(string key)
        {
            key = AsciiUtil.ToLower(key);
            KeyData keyData = KEYMAP.Get(key);
            if (keyData != null)
            {
                return keyData.BcpId;
            }
            return null;
        }

        public static string ToLegacyKey(string key)
        {
            key = AsciiUtil.ToLower(key);
            KeyData keyData = KEYMAP.Get(key);
            if (keyData != null)
            {
                return keyData.LegacyId;
            }
            return null;
        }

        public static string ToBcpType(string key, string type,
            out bool isKnownKey, out bool isSpecialType)
        {
            isKnownKey = false;
            isSpecialType = false;

            key = AsciiUtil.ToLower(key);
            type = AsciiUtil.ToLower(type);

            KeyData keyData = KEYMAP.Get(key);
            if (keyData != null)
            {
                isKnownKey = true;
                Type t = keyData.TypeMap.Get(type);
                if (t != null)
                {
                    return t.bcpId;
                }
                if (keyData.SpecialTypes != null)
                {
                    foreach (SpecialType st in keyData.SpecialTypes)
                    {
                        if (st.GetHandler().IsWellFormed(type))
                        {
                            isSpecialType = true;
                            return st.GetHandler().Canonicalize(type);
                        }
                    }
                }
            }
            return null;
        }


        public static string ToLegacyType(string key, string type,
            out bool isKnownKey, out bool isSpecialType)
        {
            isKnownKey = false;
            isSpecialType = false;

            key = AsciiUtil.ToLower(key);
            type = AsciiUtil.ToLower(type);

            KeyData keyData = KEYMAP.Get(key);
            if (keyData != null)
            {
                isKnownKey = true;
                Type t = keyData.TypeMap.Get(type);
                if (t != null)
                {
                    return t.legacyId;
                }
                if (keyData.SpecialTypes != null)
                {
                    foreach (SpecialType st in keyData.SpecialTypes)
                    {
                        if (st.GetHandler().IsWellFormed(type))
                        {
                            isSpecialType = true;
                            return st.GetHandler().Canonicalize(type);
                        }
                    }
                }
            }
            return null;
        }

        private static void InitFromResourceBundle()
        {
            UResourceBundle keyTypeDataRes = UResourceBundle.GetBundleInstance(
                    ICUData.ICU_BASE_NAME,
                    "keyTypeData",
                    ICUResourceBundle.ICU_DATA_CLASS_LOADER);

            GetKeyInfo(keyTypeDataRes.Get("keyInfo"));
            GetTypeInfo(keyTypeDataRes.Get("typeInfo"));

            UResourceBundle keyMapRes = keyTypeDataRes.Get("keyMap");
            UResourceBundle typeMapRes = keyTypeDataRes.Get("typeMap");

            // alias data is optional
            UResourceBundle typeAliasRes = null;
            UResourceBundle bcpTypeAliasRes = null;

            try
            {
                typeAliasRes = keyTypeDataRes.Get("typeAlias");
            }
            catch (MissingManifestResourceException e)
            {
                // fall through
            }

            try
            {
                bcpTypeAliasRes = keyTypeDataRes.Get("bcpTypeAlias");
            }
            catch (MissingManifestResourceException e)
            {
                // fall through
            }

            // iterate through keyMap resource
            using (UResourceBundleEnumerator keyMapItr = keyMapRes.GetEnumerator())
            {
                IDictionary<string, IList<string>> _Bcp47Keys = new Dictionary<string, IList<string>>(); // ICU4N NOTE: As long as we don't delete, Dictionary keeps insertion order the same as LinkedHashMap

                while (keyMapItr.MoveNext())
                {
                    UResourceBundle keyMapEntry = keyMapItr.Current;
                    string legacyKeyId = keyMapEntry.Key;
                    string bcpKeyId = keyMapEntry.GetString();

                    bool hasSameKey = false;
                    if (bcpKeyId.Length == 0)
                    {
                        // Empty value indicates that BCP key is same with the legacy key.
                        bcpKeyId = legacyKeyId;
                        hasSameKey = true;
                    }
                    IList<string> _bcp47Types = new List<string>(); // ICU4N: Mimic LinkedHashSet with List by ensuring no duplicates are added
                    _Bcp47Keys[bcpKeyId] = _bcp47Types.ToUnmodifiableList();

                    bool isTZ = legacyKeyId.Equals("timezone");

                    // reverse type alias map
                    IDictionary<string, ISet<string>> typeAliasMap = null;
                    if (typeAliasRes != null)
                    {
                        UResourceBundle typeAliasResByKey = null;
                        try
                        {
                            typeAliasResByKey = typeAliasRes.Get(legacyKeyId);
                        }
                        catch (MissingManifestResourceException e)
                        {
                            // fall through
                        }
                        if (typeAliasResByKey != null)
                        {
                            typeAliasMap = new Dictionary<string, ISet<string>>();
                            using (UResourceBundleEnumerator typeAliasResItr = typeAliasResByKey.GetEnumerator())
                            {
                                while (typeAliasResItr.MoveNext())
                                {
                                    UResourceBundle typeAliasDataEntry = typeAliasResItr.Current;
                                    string from = typeAliasDataEntry.Key;
                                    string to = typeAliasDataEntry.GetString();
                                    if (isTZ)
                                    {
                                        from = from.Replace(':', '/');
                                    }
                                    ISet<string> aliasSet = typeAliasMap.Get(to);
                                    if (aliasSet == null)
                                    {
                                        aliasSet = new HashSet<string>();
                                        typeAliasMap[to] = aliasSet;
                                    }
                                    aliasSet.Add(from);
                                }
                            }
                        }
                    }

                    // reverse bcp type alias map
                    IDictionary<string, ISet<string>> bcpTypeAliasMap = null;
                    if (bcpTypeAliasRes != null)
                    {
                        UResourceBundle bcpTypeAliasResByKey = null;
                        try
                        {
                            bcpTypeAliasResByKey = bcpTypeAliasRes.Get(bcpKeyId);
                        }
                        catch (MissingManifestResourceException e)
                        {
                            // fall through
                        }
                        if (bcpTypeAliasResByKey != null)
                        {
                            bcpTypeAliasMap = new Dictionary<string, ISet<string>>();
                            using (UResourceBundleEnumerator bcpTypeAliasResItr = bcpTypeAliasResByKey.GetEnumerator())
                            {
                                while (bcpTypeAliasResItr.MoveNext())
                                {
                                    UResourceBundle bcpTypeAliasDataEntry = bcpTypeAliasResItr.Current;
                                    string from = bcpTypeAliasDataEntry.Key;
                                    string to = bcpTypeAliasDataEntry.GetString();
                                    ISet<string> aliasSet = bcpTypeAliasMap.Get(to);
                                    if (aliasSet == null)
                                    {
                                        aliasSet = new HashSet<string>();
                                        bcpTypeAliasMap[to] = aliasSet;
                                    }
                                    aliasSet.Add(from);
                                }
                            }
                        }
                    }

                    IDictionary<string, Type> typeDataMap = new Dictionary<string, Type>();
                    ISet<SpecialType> specialTypeSet = null;

                    // look up type map for the key, and walk through the mapping data
                    UResourceBundle typeMapResByKey = null;
                    try
                    {
                        typeMapResByKey = typeMapRes.Get(legacyKeyId);
                    }
                    catch (MissingManifestResourceException e)
                    {
                        // type map for each key must exist
                        Debug.Assert(false);
                    }
                    if (typeMapResByKey != null)
                    {
                        using (UResourceBundleEnumerator typeMapResByKeyItr = typeMapResByKey.GetEnumerator())
                            while (typeMapResByKeyItr.MoveNext())
                            {
                                UResourceBundle typeMapEntry = typeMapResByKeyItr.Current;
                                string legacyTypeId = typeMapEntry.Key;
                                string bcpTypeId = typeMapEntry.GetString();

                                // special types
                                char first = legacyTypeId[0];
                                bool isSpecialType = '9' < first && first < 'a' && bcpTypeId.Length == 0;
                                if (isSpecialType)
                                {
                                    if (specialTypeSet == null)
                                    {
                                        specialTypeSet = new HashSet<SpecialType>();
                                    }
                                    specialTypeSet.Add((SpecialType)Enum.Parse(typeof(SpecialType), legacyTypeId, true));
                                    if (!_bcp47Types.Contains(legacyTypeId)) // ICU4N: Mimic LinkedHashSet with a List by not allowing duplicates
                                        _bcp47Types.Add(legacyTypeId);
                                    continue;
                                }

                                if (isTZ)
                                {
                                    // a timezone key uses a colon instead of a slash in the resource.
                                    // e.g. America:Los_Angeles
                                    legacyTypeId = legacyTypeId.Replace(':', '/');
                                }

                                bool hasSameType = false;
                                if (bcpTypeId.Length == 0)
                                {
                                    // Empty value indicates that BCP type is same with the legacy type.
                                    bcpTypeId = legacyTypeId;
                                    hasSameType = true;
                                }
                                if (!_bcp47Types.Contains(legacyTypeId)) // ICU4N: Mimic LinkedHashSet with a List by not allowing duplicates
                                    _bcp47Types.Add(bcpTypeId);

                                // Note: legacy type value should never be
                                // equivalent to bcp type value of a different
                                // type under the same key. So we use a single
                                // map for lookup.
                                Type t = new Type(legacyTypeId, bcpTypeId);
                                typeDataMap[AsciiUtil.ToLower(legacyTypeId)] = t;
                                if (!hasSameType)
                                {
                                    typeDataMap[AsciiUtil.ToLower(bcpTypeId)] = t;
                                }

                                // Also put aliases in the map
                                if (typeAliasMap != null)
                                {
                                    ISet<string> typeAliasSet = typeAliasMap.Get(legacyTypeId);
                                    if (typeAliasSet != null)
                                    {
                                        foreach (string alias in typeAliasSet)
                                        {
                                            typeDataMap[AsciiUtil.ToLower(alias)] = t;
                                        }
                                    }
                                }
                                if (bcpTypeAliasMap != null)
                                {
                                    ISet<string> bcpTypeAliasSet = bcpTypeAliasMap.Get(bcpTypeId);
                                    if (bcpTypeAliasSet != null)
                                    {
                                        foreach (string alias in bcpTypeAliasSet)
                                        {
                                            typeDataMap[AsciiUtil.ToLower(alias)] = t;
                                        }
                                    }
                                }
                            }
                    }

                    KeyData keyData = new KeyData(legacyKeyId, bcpKeyId, typeDataMap, specialTypeSet);

                    KEYMAP[AsciiUtil.ToLower(legacyKeyId)] = keyData;
                    if (!hasSameKey)
                    {
                        KEYMAP[AsciiUtil.ToLower(bcpKeyId)] = keyData;
                    }
                }
            
                BCP47_KEYS = _Bcp47Keys.ToUnmodifiableDictionary();
            }
        }

        internal static ISet<string> DEPRECATED_KEYS = new HashSet<string>(); // default for no resources
        internal static IDictionary<string, ValueType> VALUE_TYPES = new Dictionary<string, ValueType>(); // default for no resources
        internal static IDictionary<string, ISet<string>> DEPRECATED_KEY_TYPES = new Dictionary<string, ISet<string>>(); // default for no resources

        private enum KeyInfoType { deprecated, valueType }
        private enum TypeInfoType { deprecated }

        /** Reads
            keyInfo{
                deprecated{
                            kh{"true"}
                            vt{"true"}
                }
                valueType{
                            ca{"incremental"}
                            h0{"single"}
                            kr{"multiple"}
                            vt{"multiple"}
                            x0{"any"}
                }
            }
         */
        private static void GetKeyInfo(UResourceBundle keyInfoRes)
        {
            ISet<string> _deprecatedKeys = new HashSet<string>();
            IDictionary<string, ValueType> _valueTypes = new Dictionary<string, ValueType>(); // ICU4N NOTE: As long as we don't delete, Dictionary keeps insertion order the same as LinkedHashMap
            foreach (var keyInfoEntry in keyInfoRes)
            {
                string key = keyInfoEntry.Key;
                KeyInfoType keyInfo = (KeyInfoType)Enum.Parse(typeof(KeyInfoType), key, true);
                foreach (var keyInfoEntry2 in keyInfoEntry)
                {
                    string key2 = keyInfoEntry2.Key;
                    string value2 = keyInfoEntry2.GetString();
                    switch (keyInfo)
                    {
                        case KeyInfoType.deprecated:
                            _deprecatedKeys.Add(key2);
                            break;
                        case KeyInfoType.valueType:
                            _valueTypes[key2] = (ValueType)Enum.Parse(typeof(ValueType), value2, true);
                            break;
                    }
                }
            }
            DEPRECATED_KEYS = _deprecatedKeys.ToUnmodifiableSet();
            VALUE_TYPES = _valueTypes.ToUnmodifiableDictionary();
        }

        /** Reads:
            typeInfo{
                deprecated{
                            co{
                                direct{"true"}
                            }
                            tz{
                                camtr{"true"}
                            }
                }
            }
         */
        private static void GetTypeInfo(UResourceBundle typeInfoRes)
        {
            IDictionary<string, ISet<string>> _deprecatedKeyTypes = new Dictionary<string, ISet<string>>();  // ICU4N NOTE: As long as we don't delete, Dictionary keeps insertion order the same as LinkedHashMap
            foreach (var keyInfoEntry in typeInfoRes)
            {
                string key = keyInfoEntry.Key;
                TypeInfoType typeInfo = (TypeInfoType)Enum.Parse(typeof(TypeInfoType), key, true);
                foreach (var keyInfoEntry2 in keyInfoEntry)
                {
                    string key2 = keyInfoEntry2.Key;
                    ISet<string> _deprecatedTypes = new HashSet<string>(); // ICU4N TODO: LinkedHashSet...?
                    foreach (var keyInfoEntry3 in keyInfoEntry2)
                    {
                        string key3 = keyInfoEntry3.Key;
                        switch (typeInfo)
                        { // allow for expansion
                            case TypeInfoType.deprecated:
                                _deprecatedTypes.Add(key3);
                                break;
                        }
                    }
                    _deprecatedKeyTypes[key2] = _deprecatedTypes.ToUnmodifiableSet();
                }
            }
            DEPRECATED_KEY_TYPES = _deprecatedKeyTypes.ToUnmodifiableDictionary();
        }


        //
        // Note:    The key-type data is currently read from ICU resource bundle keyTypeData.res.
        //          In future, we may import the data into code like below directly from CLDR to
        //          avoid cyclic dependency between ULocale and UResourceBundle. For now, the code
        //          below is just for proof of concept, and commented out.
        //

        //    private static final string[][] TYPE_DATA_CA = {
        //     // {<legacy type>, <bcp type - if different>},
        //        {"buddhist", null},
        //        {"chinese", null},
        //        {"coptic", null},
        //        {"dangi", null},
        //        {"ethiopic", null},
        //        {"ethiopic-amete-alem", "ethioaa"},
        //        {"gregorian", "gregory"},
        //        {"hebrew", null},
        //        {"indian", null},
        //        {"islamic", null},
        //        {"islamic-civil", null},
        //        {"islamic-rgsa", null},
        //        {"islamic-tbla", null},
        //        {"islamic-umalqura", null},
        //        {"iso8601", null},
        //        {"japanese", null},
        //        {"persian", null},
        //        {"roc", null},
        //    };
        //
        //    private static final string[][] TYPE_DATA_KS = {
        //     // {<legacy type>, <bcp type - if different>},
        //        {"identical", "identic"},
        //        {"primary", "level1"},
        //        {"quaternary", "level4"},
        //        {"secondary", "level2"},
        //        {"tertiary", "level3"},
        //    };
        //
        //    private static final string[][] TYPE_ALIAS_KS = {
        //     // {<legacy alias>, <legacy canonical>},
        //        {"quarternary", "quaternary"},
        //    };
        //
        //    private static final string[][] BCP_TYPE_ALIAS_CA = {
        //     // {<bcp deprecated>, <bcp preferred>
        //        {"islamicc", "islamic-civil"},
        //    };
        //
        //    private static final Object[][] KEY_DATA = {
        //     // {<legacy key>, <bcp key - if different>, <type map>, <type alias>, <bcp type alias>},
        //        {"calendar", "ca", TYPE_DATA_CA, null, BCP_TYPE_ALIAS_CA},
        //        {"colstrength", "ks", TYPE_DATA_KS, TYPE_ALIAS_KS, null},
        //    };

        private static readonly object[][] KEY_DATA = { };


        private static void InitFromTables()
        {
            foreach (object[] keyDataEntry in KEY_DATA)
            {
                string legacyKeyId = (string)keyDataEntry[0];
                string bcpKeyId = (string)keyDataEntry[1];
                string[][] typeData = (string[][])keyDataEntry[2];
                string[][] typeAliasData = (string[][])keyDataEntry[3];
                string[][] bcpTypeAliasData = (string[][])keyDataEntry[4];

                bool hasSameKey = false;
                if (bcpKeyId == null)
                {
                    bcpKeyId = legacyKeyId;
                    hasSameKey = true;
                }

                // reverse type alias map
                IDictionary<string, ISet<string>> typeAliasMap = null;
                if (typeAliasData != null)
                {
                    typeAliasMap = new Dictionary<string, ISet<string>>();
                    foreach (string[] typeAliasDataEntry in typeAliasData)
                    {
                        string from = typeAliasDataEntry[0];
                        string to = typeAliasDataEntry[1];
                        ISet<string> aliasSet;
                        if (!typeAliasMap.TryGetValue(to, out aliasSet) || aliasSet == null)
                        {
                            aliasSet = new HashSet<string>();
                            typeAliasMap[to] = aliasSet;
                        }
                        aliasSet.Add(from);
                    }
                }

                // BCP type alias map data
                IDictionary<string, ISet<string>> bcpTypeAliasMap = null;
                if (bcpTypeAliasData != null)
                {
                    bcpTypeAliasMap = new Dictionary<string, ISet<string>>();
                    foreach (string[] bcpTypeAliasDataEntry in bcpTypeAliasData)
                    {
                        string from = bcpTypeAliasDataEntry[0];
                        string to = bcpTypeAliasDataEntry[1];
                        ISet<string> aliasSet;
                        if (!bcpTypeAliasMap.TryGetValue(to, out aliasSet) || aliasSet == null)
                        {
                            aliasSet = new HashSet<string>();
                            bcpTypeAliasMap[to] = aliasSet;
                        }
                        aliasSet.Add(from);
                    }
                }

                // Type map data
                Debug.Assert(typeData != null);
                IDictionary<string, Type> typeDataMap = new Dictionary<string, Type>();
                ISet<SpecialType> specialTypeSet = null;

                foreach (string[] typeDataEntry in typeData)
                {
                    string legacyTypeId = typeDataEntry[0];
                    string bcpTypeId = typeDataEntry[1];

                    // special types
                    bool isSpecialType = false;
                    foreach (SpecialType st in Enum.GetValues(typeof(SpecialType)))
                    {
                        if (legacyTypeId.Equals(st.ToString()))
                        {
                            isSpecialType = true;
                            if (specialTypeSet == null)
                            {
                                specialTypeSet = new HashSet<SpecialType>();
                            }
                            specialTypeSet.Add(st);
                            break;
                        }
                    }
                    if (isSpecialType)
                    {
                        continue;
                    }

                    bool hasSameType = false;
                    if (bcpTypeId == null)
                    {
                        bcpTypeId = legacyTypeId;
                        hasSameType = true;
                    }

                    // Note: legacy type value should never be
                    // equivalent to bcp type value of a different
                    // type under the same key. So we use a single
                    // map for lookup.
                    Type t = new Type(legacyTypeId, bcpTypeId);
                    typeDataMap[AsciiUtil.ToLower(legacyTypeId)] = t;
                    if (!hasSameType)
                    {
                        typeDataMap[AsciiUtil.ToLower(bcpTypeId)] = t;
                    }

                    // Also put aliases in the index
                    ISet<string> typeAliasSet;
                    if (typeAliasMap.TryGetValue(legacyTypeId, out typeAliasSet) && typeAliasSet != null)
                    {
                        foreach (string alias in typeAliasSet)
                        {
                            typeDataMap[AsciiUtil.ToLower(alias)] = t;
                        }
                    }
                    ISet<string> bcpTypeAliasSet;
                    if (!bcpTypeAliasMap.TryGetValue(bcpTypeId, out bcpTypeAliasSet) && bcpTypeAliasSet != null)
                    {
                        foreach (string alias in bcpTypeAliasSet)
                        {
                            typeDataMap[AsciiUtil.ToLower(alias)] = t;
                        }
                    }
                }

                ISet<SpecialType> specialTypes = null;
                if (specialTypeSet != null)
                {
                    specialTypes = new HashSet<SpecialType>(specialTypeSet);
                }

                KeyData keyData = new KeyData(legacyKeyId, bcpKeyId, typeDataMap, specialTypes);

                KEYMAP[AsciiUtil.ToLower(legacyKeyId)] = keyData;
                if (!hasSameKey)
                {
                    KEYMAP[AsciiUtil.ToLower(bcpKeyId)] = keyData;
                }
            }
        }

        private static readonly IDictionary<string, KeyData> KEYMAP = new Dictionary<string, KeyData>();
        private static IDictionary<string, IList<string>> BCP47_KEYS;

        static KeyTypeData()
        {
            // InitFromTables();
            InitFromResourceBundle();
        }

        public static ICollection<string> GetBcp47Keys()
        {
            return BCP47_KEYS.Keys;
        }

        public static IList<string> GetBcp47KeyTypes(string key)
        {
            return BCP47_KEYS.Get(key);
        }

        public static bool IsDeprecated(string key)
        {
            return DEPRECATED_KEYS.Contains(key);
        }

        public static bool IsDeprecated(string key, string type)
        {
            ISet<string> deprecatedTypes = DEPRECATED_KEY_TYPES.Get(key);
            if (deprecatedTypes == null)
            {
                return false;
            }
            return deprecatedTypes.Contains(type);
        }

        public static ValueType GetValueType(string key)
        {
            return VALUE_TYPES.Get(key); // Defaults to ValueType.Single
        }
    }
}
