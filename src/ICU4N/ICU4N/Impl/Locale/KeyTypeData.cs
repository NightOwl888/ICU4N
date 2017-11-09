using ICU4N.Support.Collections;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
                return AsciiUtil.ToLowerString(value);
            }
        }

        internal class CodepointsTypeHandler : SpecialTypeHandler
        {
            private static readonly Regex pat = new Regex("[0-9a-fA-F]{4,6}(-[0-9a-fA-F]{4,6})*", RegexOptions.Compiled);

            internal override bool IsWellFormed(string value)
            {
                return pat.IsMatch(value);
            }
        }

        internal class ReorderCodeTypeHandler : SpecialTypeHandler
        {
            private static readonly Regex pat = new Regex("[a-zA-Z]{3,8}(-[a-zA-Z]{3,8})*", RegexOptions.Compiled);

            internal override bool IsWellFormed(string value)
            {
                return pat.IsMatch(value);
            }
        }

        internal class RgKeyValueTypeHandler : SpecialTypeHandler
        {
            private static readonly Regex pat = new Regex("([a-zA-Z]{2}|[0-9]{3})[zZ]{4}", RegexOptions.Compiled);

            internal override bool IsWellFormed(string value)
            {
                return pat.IsMatch(value);
            }
        }

        internal class SubdivisionKeyValueTypeHandler : SpecialTypeHandler
        {
            private static readonly Regex pat = new Regex("([a-zA-Z]{2}|[0-9]{3})", RegexOptions.Compiled);

            internal override bool IsWellFormed(string value)
            {
                return pat.IsMatch(value);
            }
        }

        internal class PrivateUseKeyValueTypeHandler : SpecialTypeHandler
        {
            private static readonly Regex pat = new Regex("[a-zA-Z0-9]{3,8}(-[a-zA-Z0-9]{3,8})*", RegexOptions.Compiled);

            internal override bool IsWellFormed(string value)
            {
                return pat.IsMatch(value);
            }
        }

        internal enum SpecialType
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
            public IList<SpecialType> SpecialTypes { get; private set; }

            internal KeyData(string legacyId, string bcpId, IDictionary<string, Type> typeMap,
                    IList<SpecialType> specialTypes)
            {
                this.LegacyId = legacyId;
                this.BcpId = bcpId;
                this.TypeMap = typeMap;
                this.SpecialTypes = specialTypes;
            }
        }

        private class Type
        {
            internal string legacyId;
            internal string bcpId;

            internal Type(string legacyId, string bcpId)
            {
                this.legacyId = legacyId;
                this.bcpId = bcpId;
            }
        }

        public static string ToBcpKey(string key)
        {
            key = AsciiUtil.ToLowerString(key);
            KeyData keyData = KEYMAP.Get(key);
            if (keyData != null)
            {
                return keyData.BcpId;
            }
            return null;
        }

        public static string ToLegacyKey(string key)
        {
            key = AsciiUtil.ToLowerString(key);
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

            key = AsciiUtil.ToLowerString(key);
            type = AsciiUtil.ToLowerString(type);

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

            key = AsciiUtil.ToLowerString(key);
            type = AsciiUtil.ToLowerString(type);

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
            // ICU4N TODO: Finish implementation

            //    UResourceBundle keyTypeDataRes = UResourceBundle.getBundleInstance(
            //            ICUData.ICU_BASE_NAME,
            //            "keyTypeData",
            //            ICUResourceBundle.ICU_DATA_CLASS_LOADER);

            //    getKeyInfo(keyTypeDataRes.get("keyInfo"));
            //    getTypeInfo(keyTypeDataRes.get("typeInfo"));

            //    UResourceBundle keyMapRes = keyTypeDataRes.get("keyMap");
            //    UResourceBundle typeMapRes = keyTypeDataRes.get("typeMap");

            //    // alias data is optional
            //    UResourceBundle typeAliasRes = null;
            //    UResourceBundle bcpTypeAliasRes = null;

            //    try
            //    {
            //        typeAliasRes = keyTypeDataRes.get("typeAlias");
            //    }
            //    catch (MissingResourceException e)
            //    {
            //        // fall through
            //    }

            //    try
            //    {
            //        bcpTypeAliasRes = keyTypeDataRes.get("bcpTypeAlias");
            //    }
            //    catch (MissingResourceException e)
            //    {
            //        // fall through
            //    }

            //    // iterate through keyMap resource
            //    UResourceBundleIterator keyMapItr = keyMapRes.getIterator();
            //    Map<String, Set<String>> _Bcp47Keys = new LinkedHashMap<String, Set<String>>();

            //    while (keyMapItr.hasNext())
            //    {
            //        UResourceBundle keyMapEntry = keyMapItr.next();
            //        String legacyKeyId = keyMapEntry.getKey();
            //        String bcpKeyId = keyMapEntry.getString();

            //        bool hasSameKey = false;
            //        if (bcpKeyId.length() == 0)
            //        {
            //            // Empty value indicates that BCP key is same with the legacy key.
            //            bcpKeyId = legacyKeyId;
            //            hasSameKey = true;
            //        }
            //        final LinkedHashSet<String> _bcp47Types = new LinkedHashSet<String>();
            //        _Bcp47Keys.put(bcpKeyId, Collections.unmodifiableSet(_bcp47Types));

            //        bool isTZ = legacyKeyId.equals("timezone");

            //        // reverse type alias map
            //        Map<String, Set<String>> typeAliasMap = null;
            //        if (typeAliasRes != null)
            //        {
            //            UResourceBundle typeAliasResByKey = null;
            //            try
            //            {
            //                typeAliasResByKey = typeAliasRes.get(legacyKeyId);
            //            }
            //            catch (MissingResourceException e)
            //            {
            //                // fall through
            //            }
            //            if (typeAliasResByKey != null)
            //            {
            //                typeAliasMap = new HashMap<String, Set<String>>();
            //                UResourceBundleIterator typeAliasResItr = typeAliasResByKey.getIterator();
            //                while (typeAliasResItr.hasNext())
            //                {
            //                    UResourceBundle typeAliasDataEntry = typeAliasResItr.next();
            //                    String from = typeAliasDataEntry.getKey();
            //                    String to = typeAliasDataEntry.getString();
            //                    if (isTZ)
            //                    {
            //                        from = from.replace(':', '/');
            //                    }
            //                    Set<String> aliasSet = typeAliasMap.get(to);
            //                    if (aliasSet == null)
            //                    {
            //                        aliasSet = new HashSet<String>();
            //                        typeAliasMap.put(to, aliasSet);
            //                    }
            //                    aliasSet.add(from);
            //                }
            //            }
            //        }

            //        // reverse bcp type alias map
            //        Map<String, Set<String>> bcpTypeAliasMap = null;
            //        if (bcpTypeAliasRes != null)
            //        {
            //            UResourceBundle bcpTypeAliasResByKey = null;
            //            try
            //            {
            //                bcpTypeAliasResByKey = bcpTypeAliasRes.get(bcpKeyId);
            //            }
            //            catch (MissingResourceException e)
            //            {
            //                // fall through
            //            }
            //            if (bcpTypeAliasResByKey != null)
            //            {
            //                bcpTypeAliasMap = new HashMap<String, Set<String>>();
            //                UResourceBundleIterator bcpTypeAliasResItr = bcpTypeAliasResByKey.getIterator();
            //                while (bcpTypeAliasResItr.hasNext())
            //                {
            //                    UResourceBundle bcpTypeAliasDataEntry = bcpTypeAliasResItr.next();
            //                    String from = bcpTypeAliasDataEntry.getKey();
            //                    String to = bcpTypeAliasDataEntry.getString();
            //                    Set<String> aliasSet = bcpTypeAliasMap.get(to);
            //                    if (aliasSet == null)
            //                    {
            //                        aliasSet = new HashSet<String>();
            //                        bcpTypeAliasMap.put(to, aliasSet);
            //                    }
            //                    aliasSet.add(from);
            //                }
            //            }
            //        }

            //        Map<String, Type> typeDataMap = new HashMap<String, Type>();
            //        EnumSet<SpecialType> specialTypeSet = null;

            //        // look up type map for the key, and walk through the mapping data
            //        UResourceBundle typeMapResByKey = null;
            //        try
            //        {
            //            typeMapResByKey = typeMapRes.get(legacyKeyId);
            //        }
            //        catch (MissingResourceException e)
            //        {
            //            // type map for each key must exist
            //            assert false;
            //        }
            //        if (typeMapResByKey != null)
            //        {
            //            UResourceBundleIterator typeMapResByKeyItr = typeMapResByKey.getIterator();
            //            while (typeMapResByKeyItr.hasNext())
            //            {
            //                UResourceBundle typeMapEntry = typeMapResByKeyItr.next();
            //                String legacyTypeId = typeMapEntry.getKey();
            //                String bcpTypeId = typeMapEntry.getString();

            //                // special types
            //                final char first = legacyTypeId.charAt(0);
            //                final bool isSpecialType = '9' < first && first < 'a' && bcpTypeId.length() == 0;
            //                if (isSpecialType)
            //                {
            //                    if (specialTypeSet == null)
            //                    {
            //                        specialTypeSet = EnumSet.noneOf(SpecialType.class);
            //                        }
            //                        specialTypeSet.add(SpecialType.valueOf(legacyTypeId));
            //                        _bcp47Types.add(legacyTypeId);
            //                        continue;
            //                    }

            //                    if (isTZ) {
            //                        // a timezone key uses a colon instead of a slash in the resource.
            //                        // e.g. America:Los_Angeles
            //                        legacyTypeId = legacyTypeId.replace(':', '/');
            //                    }

            //                    bool hasSameType = false;
            //                    if (bcpTypeId.length() == 0) {
            //                        // Empty value indicates that BCP type is same with the legacy type.
            //                        bcpTypeId = legacyTypeId;
            //                        hasSameType = true;
            //                    }
            //                    _bcp47Types.add(bcpTypeId);

            //                    // Note: legacy type value should never be
            //                    // equivalent to bcp type value of a different
            //                    // type under the same key. So we use a single
            //                    // map for lookup.
            //                    Type t = new Type(legacyTypeId, bcpTypeId);
            //typeDataMap.put(AsciiUtil.toLowerString(legacyTypeId), t);
            //                    if (!hasSameType) {
            //                        typeDataMap.put(AsciiUtil.toLowerString(bcpTypeId), t);
            //                    }

            //                    // Also put aliases in the map
            //                    if (typeAliasMap != null) {
            //                        Set<String> typeAliasSet = typeAliasMap.get(legacyTypeId);
            //                        if (typeAliasSet != null) {
            //                            for (String alias : typeAliasSet) {
            //                                typeDataMap.put(AsciiUtil.toLowerString(alias), t);
            //                            }
            //                        }
            //                    }
            //                    if (bcpTypeAliasMap != null) {
            //                        Set<String> bcpTypeAliasSet = bcpTypeAliasMap.get(bcpTypeId);
            //                        if (bcpTypeAliasSet != null) {
            //                            for (String alias : bcpTypeAliasSet) {
            //                                typeDataMap.put(AsciiUtil.toLowerString(alias), t);
            //                            }
            //                        }
            //                    }
            //                }
            //            }

            //            KeyData keyData = new KeyData(legacyKeyId, bcpKeyId, typeDataMap, specialTypeSet);

            //KEYMAP.put(AsciiUtil.toLowerString(legacyKeyId), keyData);
            //            if (!hasSameKey) {
            //                KEYMAP.put(AsciiUtil.toLowerString(bcpKeyId), keyData);
            //            }
            //        }
            //        BCP47_KEYS = Collections.unmodifiableMap(_Bcp47Keys);
        }

        internal static ISet<string> DEPRECATED_KEYS = new HashSet<string>(); // default for no resources
        internal static IDictionary<string, ValueType> VALUE_TYPES = new Dictionary<string, ValueType>(); // default for no resources
        internal static IDictionary<string, ISet<string>> DEPRECATED_KEY_TYPES = new Dictionary<string, ISet<string>>(); // default for no resources

        private enum KeyInfoType { deprecated, valueType }
        private enum TypeInfoType { deprecated }

        // ICU4N TODO: Finish resource loading

//        /** Reads
//keyInfo{
//deprecated{
//            kh{"true"}
//            vt{"true"}
//}
//valueType{
//            ca{"incremental"}
//            h0{"single"}
//            kr{"multiple"}
//            vt{"multiple"}
//            x0{"any"}
//}
//}
//         */
//        private static void GetKeyInfo(UResourceBundle keyInfoRes)
//        {
//            Set<String> _deprecatedKeys = new LinkedHashSet<String>();
//            Map<String, ValueType> _valueTypes = new LinkedHashMap<String, ValueType>();
//            for (UResourceBundleIterator keyInfoIt = keyInfoRes.getIterator(); keyInfoIt.hasNext();)
//            {
//                UResourceBundle keyInfoEntry = keyInfoIt.next();
//                String key = keyInfoEntry.getKey();
//                KeyInfoType keyInfo = KeyInfoType.valueOf(key);
//                for (UResourceBundleIterator keyInfoIt2 = keyInfoEntry.getIterator(); keyInfoIt2.hasNext();)
//                {
//                    UResourceBundle keyInfoEntry2 = keyInfoIt2.next();
//                    String key2 = keyInfoEntry2.getKey();
//                    String value2 = keyInfoEntry2.getString();
//                    switch (keyInfo)
//                    {
//                        case deprecated:
//                            _deprecatedKeys.add(key2);
//                            break;
//                        case valueType:
//                            _valueTypes.put(key2, ValueType.valueOf(value2));
//                            break;
//                    }
//                }
//            }
//            DEPRECATED_KEYS = Collections.unmodifiableSet(_deprecatedKeys);
//            VALUE_TYPES = Collections.unmodifiableMap(_valueTypes);
//        }

//        /** Reads:
//typeInfo{
//deprecated{
//            co{
//                direct{"true"}
//            }
//            tz{
//                camtr{"true"}
//            }
//}
//}
//         */
//        private static void getTypeInfo(UResourceBundle typeInfoRes)
//        {
//            Map<String, Set<String>> _deprecatedKeyTypes = new LinkedHashMap<String, Set<String>>();
//            for (UResourceBundleIterator keyInfoIt = typeInfoRes.getIterator(); keyInfoIt.hasNext();)
//            {
//                UResourceBundle keyInfoEntry = keyInfoIt.next();
//                String key = keyInfoEntry.getKey();
//                TypeInfoType typeInfo = TypeInfoType.valueOf(key);
//                for (UResourceBundleIterator keyInfoIt2 = keyInfoEntry.getIterator(); keyInfoIt2.hasNext();)
//                {
//                    UResourceBundle keyInfoEntry2 = keyInfoIt2.next();
//                    String key2 = keyInfoEntry2.getKey();
//                    Set<String> _deprecatedTypes = new LinkedHashSet<String>();
//                    for (UResourceBundleIterator keyInfoIt3 = keyInfoEntry2.getIterator(); keyInfoIt3.hasNext();)
//                    {
//                        UResourceBundle keyInfoEntry3 = keyInfoIt3.next();
//                        String key3 = keyInfoEntry3.getKey();
//                        switch (typeInfo)
//                        { // allow for expansion
//                            case deprecated:
//                                _deprecatedTypes.add(key3);
//                                break;
//                        }
//                    }
//                    _deprecatedKeyTypes.put(key2, Collections.unmodifiableSet(_deprecatedTypes));
//                }
//            }
//            DEPRECATED_KEY_TYPES = Collections.unmodifiableMap(_deprecatedKeyTypes);
//        }

        //
        // Note:    The key-type data is currently read from ICU resource bundle keyTypeData.res.
        //          In future, we may import the data into code like below directly from CLDR to
        //          avoid cyclic dependency between ULocale and UResourceBundle. For now, the code
        //          below is just for proof of concept, and commented out.
        //

        //    private static final String[][] TYPE_DATA_CA = {
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
        //    private static final String[][] TYPE_DATA_KS = {
        //     // {<legacy type>, <bcp type - if different>},
        //        {"identical", "identic"},
        //        {"primary", "level1"},
        //        {"quaternary", "level4"},
        //        {"secondary", "level2"},
        //        {"tertiary", "level3"},
        //    };
        //
        //    private static final String[][] TYPE_ALIAS_KS = {
        //     // {<legacy alias>, <legacy canonical>},
        //        {"quarternary", "quaternary"},
        //    };
        //
        //    private static final String[][] BCP_TYPE_ALIAS_CA = {
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
                            typeAliasMap[to]= aliasSet;
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
                            aliasSet = new HashSet<String>();
                            bcpTypeAliasMap[to]= aliasSet;
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
                    typeDataMap[AsciiUtil.ToLowerString(legacyTypeId)] = t;
                    if (!hasSameType)
                    {
                        typeDataMap[AsciiUtil.ToLowerString(bcpTypeId)] = t;
                    }

                    // Also put aliases in the index
                    ISet<string> typeAliasSet;
                    if (typeAliasMap.TryGetValue(legacyTypeId, out typeAliasSet) && typeAliasSet != null)
                    {
                        foreach (string alias in typeAliasSet)
                        {
                            typeDataMap[AsciiUtil.ToLowerString(alias)] = t;
                        }
                    }
                    ISet<string> bcpTypeAliasSet;
                    if (!bcpTypeAliasMap.TryGetValue(bcpTypeId, out bcpTypeAliasSet) && bcpTypeAliasSet != null)
                    {
                        foreach (string alias in bcpTypeAliasSet)
                        {
                            typeDataMap[AsciiUtil.ToLowerString(alias)] = t;
                        }
                    }
                }

                IList<SpecialType> specialTypes = null;
                if (specialTypeSet != null)
                {
                    specialTypes = specialTypeSet.ToList();
                }

                KeyData keyData = new KeyData(legacyKeyId, bcpKeyId, typeDataMap, specialTypes);

                KEYMAP[AsciiUtil.ToLowerString(legacyKeyId)] = keyData;
                if (!hasSameKey)
                {
                    KEYMAP[AsciiUtil.ToLowerString(bcpKeyId)] = keyData;
                }
            }
        }

        private static readonly IDictionary<string, KeyData> KEYMAP = new Dictionary<string, KeyData>();
        private static IDictionary<string, ISet<string>> BCP47_KEYS;

        static KeyTypeData()
        {
            InitFromTables();
            //InitFromResourceBundle(); // ICU4N TODO: Finish resource bundle implementation and enable
        }

        public static ICollection<string> GetBcp47Keys()
        {
            return BCP47_KEYS.Keys;
        }

        public static ISet<string> GetBcp47KeyTypes(string key)
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
            ValueType type = VALUE_TYPES.Get(key);
            return type; // Defaults to ValueType.Single
            //return type == null ? ValueType.single : type;
        }
    }
}
