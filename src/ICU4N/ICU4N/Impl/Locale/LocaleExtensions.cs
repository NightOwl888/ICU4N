using ICU4N.Support.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CaseInsensitiveChar = ICU4N.Impl.Locale.InternalLocaleBuilder.CaseInsensitiveChar;
using CaseInsensitiveString = ICU4N.Impl.Locale.InternalLocaleBuilder.CaseInsensitiveString;

namespace ICU4N.Impl.Locale
{
    public class LocaleExtensions
    {
        private IDictionary<char, Extension> _map;
        private String _id;

        private static readonly IDictionary<char, Extension> EMPTY_MAP =
            new SortedDictionary<char, Extension>().ToUnmodifiableDictionary();

        public static readonly LocaleExtensions EMPTY_EXTENSIONS; // ICU4N TODO: Rename to follow .NET Conventions
        public static readonly LocaleExtensions CALENDAR_JAPANESE;
        public static readonly LocaleExtensions NUMBER_THAI;

        static LocaleExtensions()
        {
            EMPTY_EXTENSIONS = new LocaleExtensions();
            EMPTY_EXTENSIONS._id = "";
            EMPTY_EXTENSIONS._map = EMPTY_MAP;

            CALENDAR_JAPANESE = new LocaleExtensions();
            CALENDAR_JAPANESE._id = "u-ca-japanese";
            CALENDAR_JAPANESE._map = new SortedDictionary<char, Extension>();
            CALENDAR_JAPANESE._map[UnicodeLocaleExtension.SINGLETON] = UnicodeLocaleExtension.CA_JAPANESE;

            NUMBER_THAI = new LocaleExtensions();
            NUMBER_THAI._id = "u-nu-thai";
            NUMBER_THAI._map = new SortedDictionary<char, Extension>();
            NUMBER_THAI._map[UnicodeLocaleExtension.SINGLETON] = UnicodeLocaleExtension.NU_THAI;
        }

        private LocaleExtensions()
        {
        }

        /*
         * Package local constructor, only used by InternalLocaleBuilder.
         */
        internal LocaleExtensions(IDictionary<CaseInsensitiveChar, string> extensions,
                ISet<CaseInsensitiveString> uattributes, IDictionary<CaseInsensitiveString, string> ukeywords)
        {
            bool hasExtension = (extensions != null && extensions.Count > 0);
            bool hasUAttributes = (uattributes != null && uattributes.Count > 0);
            bool hasUKeywords = (ukeywords != null && ukeywords.Count > 0);

            if (!hasExtension && !hasUAttributes && !hasUKeywords)
            {
                _map = EMPTY_MAP;
                _id = "";
                return;
            }

            // Build extension map
            _map = new SortedDictionary<char, Extension>();
            if (hasExtension)
            {
                foreach (var ext in extensions)
                {
                    char key = AsciiUtil.ToLower(ext.Key.Value);
                    string value = ext.Value;

                    if (LanguageTag.IsPrivateusePrefixChar(key))
                    {
                        // we need to exclude special variant in privuateuse, e.g. "x-abc-lvariant-DEF"
                        value = InternalLocaleBuilder.RemovePrivateuseVariant(value);
                        if (value == null)
                        {
                            continue;
                        }
                    }

                    Extension e = new Extension(key, AsciiUtil.ToLowerString(value));
                    _map[key] = e;
                }
            }

            if (hasUAttributes || hasUKeywords)
            {
                SortedSet<string> uaset = null;
                SortedDictionary<string, string> ukmap = null;

                if (hasUAttributes)
                {
                    uaset = new SortedSet<string>(StringComparer.Ordinal);
                    foreach (CaseInsensitiveString cis in uattributes)
                    {
                        uaset.Add(AsciiUtil.ToLowerString(cis.Value));
                    }
                }

                if (hasUKeywords)
                {
                    ukmap = new SortedDictionary<string, string>();
                    foreach (var kwd in ukeywords)
                    {
                        string key = AsciiUtil.ToLowerString(kwd.Key.Value);
                        string type = AsciiUtil.ToLowerString(kwd.Value);
                        ukmap[key] = type;
                    }
                }

                UnicodeLocaleExtension ule = new UnicodeLocaleExtension(uaset, ukmap);
                _map[UnicodeLocaleExtension.SINGLETON] = ule;
            }

            if (_map.Count == 0)
            {
                // this could happen when only privuateuse with special variant
                _map = EMPTY_MAP;
                _id = "";
            }
            else
            {
                _id = ToID(_map);
            }
        }

        public virtual ICollection<char> Keys
        {
            get { return _map.Keys.ToUnmodifiableCollection(); }
        }

        public virtual Extension GetExtension(char key)
        {
            Extension result;
            _map.TryGetValue(AsciiUtil.ToLower(key), out result);
            return result;
        }

        public virtual string GetExtensionValue(char key)
        {
            Extension ext;
            if (!_map.TryGetValue(AsciiUtil.ToLower(key), out ext) || ext == null)
            {
                return null;
            }
            return ext.Value;
        }

        public virtual ISet<string> GetUnicodeLocaleAttributes()
        {
            Extension ext;
            if (!_map.TryGetValue(UnicodeLocaleExtension.SINGLETON, out ext) || ext == null)
            {
                return new HashSet<string>();
            }
            Debug.Assert(ext is UnicodeLocaleExtension);
            return ((UnicodeLocaleExtension)ext).GetUnicodeLocaleAttributes();
        }

        public virtual ICollection<string> GetUnicodeLocaleKeys()
        {
            Extension ext;
            if (!_map.TryGetValue(UnicodeLocaleExtension.SINGLETON, out ext) || ext == null)
            {
                return new HashSet<string>();
            }
            Debug.Assert(ext is UnicodeLocaleExtension);
            return ((UnicodeLocaleExtension)ext).GetUnicodeLocaleKeys();
        }

        public virtual string GetUnicodeLocaleType(string unicodeLocaleKey)
        {
            Extension ext;
            if (!_map.TryGetValue(UnicodeLocaleExtension.SINGLETON, out ext) || ext == null)
            {
                return null;
            }
            Debug.Assert(ext is UnicodeLocaleExtension);
            return ((UnicodeLocaleExtension)ext).GetUnicodeLocaleType(AsciiUtil.ToLowerString(unicodeLocaleKey));
        }

        public virtual bool IsEmpty
        {
            get { return _map.Count == 0; }
        }

        public static bool IsValidKey(char c)
        {
            return LanguageTag.IsExtensionSingletonChar(c) || LanguageTag.IsPrivateusePrefixChar(c);
        }

        public static bool IsValidUnicodeLocaleKey(string ukey)
        {
            return UnicodeLocaleExtension.IsKey(ukey);
        }

        private static string ToID(IDictionary<char, Extension> map)
        {
            StringBuilder buf = new StringBuilder();
            Extension privuse = null;
            foreach (var entry in map)
            {
                char singleton = entry.Key;
                Extension extension = entry.Value;
                if (LanguageTag.IsPrivateusePrefixChar(singleton))
                {
                    privuse = extension;
                }
                else
                {
                    if (buf.Length > 0)
                    {
                        buf.Append(LanguageTag.SEP);
                    }
                    buf.Append(extension);
                }
            }
            if (privuse != null)
            {
                if (buf.Length > 0)
                {
                    buf.Append(LanguageTag.SEP);
                }
                buf.Append(privuse);
            }
            return buf.ToString();
        }


        public override string ToString()
        {
            return _id;
        }

        public virtual string ID
        {
            get { return _id; }
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override bool Equals(object other)
        {
            if (this == other)
            {
                return true;
            }
            if (!(other is LocaleExtensions))
            {
                return false;
            }
            return this._id.Equals(((LocaleExtensions)other)._id);
        }
    }
}
