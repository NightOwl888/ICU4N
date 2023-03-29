using J2N.Collections.Generic;
using J2N.Collections.Generic.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using JCG = J2N.Collections.Generic;
using CaseInsensitiveChar = ICU4N.Impl.Locale.InternalLocaleBuilder.CaseInsensitiveChar;
using CaseInsensitiveString = ICU4N.Impl.Locale.InternalLocaleBuilder.CaseInsensitiveString;
using ICU4N.Support.Collections;

namespace ICU4N.Impl.Locale
{
    public class LocaleExtensions
    {
        private IDictionary<char, Extension> _map;
        private string _id;
#if FEATURE_IREADONLYCOLLECTIONS
        private volatile IReadOnlyDictionary<char, string> _extensions;
#else
        private volatile IDictionary<char, string> _extensions;
#endif

        private static readonly IDictionary<char, Extension> EmptyMap =
            new JCG.SortedDictionary<char, Extension>().AsReadOnly();

        public static readonly LocaleExtensions EmptyExtensions = LoadEmptyExtensions();
        public static readonly LocaleExtensions CalendarJapanese = LoadCalendarJapanese();
        public static readonly LocaleExtensions NumberThai = LoadNumberThai();

        private static LocaleExtensions LoadEmptyExtensions() // ICU4N: Avoid static constructor
        {
            return new LocaleExtensions
            {
                _id = "",
                _map = EmptyMap
            };
        }
        private static LocaleExtensions LoadCalendarJapanese() // ICU4N: Avoid static constructor
        {
            return new LocaleExtensions
            {
                _id = "u-ca-japanese",
                _map = new JCG.SortedDictionary<char, Extension>
                {
                    [UnicodeLocaleExtension.Singleton] = UnicodeLocaleExtension.CalendarJapanese
                }
            };
        }
        private static LocaleExtensions LoadNumberThai() // ICU4N: Avoid static constructor
        {
            return new LocaleExtensions
            {
                _id = "u-nu-thai",
                _map = new JCG.SortedDictionary<char, Extension>
                {
                    [UnicodeLocaleExtension.Singleton] = UnicodeLocaleExtension.NumberThai
                }
            };
        }
        private LocaleExtensions()
        {
        }

        /// <summary>
        /// Internal constructor, only used by <see cref="InternalLocaleBuilder"/>.
        /// </summary>
        internal LocaleExtensions(IDictionary<CaseInsensitiveChar, string> extensions,
                ISet<CaseInsensitiveString> uattributes, IDictionary<CaseInsensitiveString, string> ukeywords)
        {
            bool hasExtension = (extensions != null && extensions.Count > 0);
            bool hasUAttributes = (uattributes != null && uattributes.Count > 0);
            bool hasUKeywords = (ukeywords != null && ukeywords.Count > 0);

            if (!hasExtension && !hasUAttributes && !hasUKeywords)
            {
                _map = EmptyMap;
                _id = "";
                return;
            }

            // Build extension map
            _map = new JCG.SortedDictionary<char, Extension>();
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

                    Extension e = new Extension(key, AsciiUtil.ToLower(value));
                    _map[key] = e;
                }
            }

            if (hasUAttributes || hasUKeywords)
            {
                JCG.SortedSet<string> uaset = null;
                JCG.SortedDictionary<string, string> ukmap = null;

                if (hasUAttributes)
                {
                    uaset = new JCG.SortedSet<string>(StringComparer.Ordinal);
                    foreach (CaseInsensitiveString cis in uattributes)
                    {
                        uaset.Add(AsciiUtil.ToLower(cis.Value));
                    }
                }

                if (hasUKeywords)
                {
                    ukmap = new JCG.SortedDictionary<string, string>(StringComparer.Ordinal);
                    foreach (var kwd in ukeywords)
                    {
                        string key = AsciiUtil.ToLower(kwd.Key.Value);
                        string type = AsciiUtil.ToLower(kwd.Value);
                        ukmap[key] = type;
                    }
                }

                UnicodeLocaleExtension ule = new UnicodeLocaleExtension(uaset, ukmap);
                _map[UnicodeLocaleExtension.Singleton] = ule;
            }

            if (_map.Count == 0)
            {
                // this could happen when only privuateuse with special variant
                _map = EmptyMap;
                _id = "";
            }
            else
            {
                _id = ToID(_map);
            }
        }

        public virtual ICollection<char> Keys => _map.Keys.AsReadOnly();

        public virtual Extension GetExtension(char key)
        {
            _map.TryGetValue(AsciiUtil.ToLower(key), out Extension result);
            return result;
        }

        public virtual string GetExtensionValue(char key)
        {
            if (!_map.TryGetValue(AsciiUtil.ToLower(key), out Extension ext) || ext == null)
            {
                return null;
            }
            return ext.Value;
        }

        // ICU4N specific - Expose the dictionary so it can be on the public API of UCultureInfo
#if FEATURE_IREADONLYCOLLECTIONS
        public virtual IReadOnlyDictionary<char, string> Extensions
#else
        public virtual IDictionary<char, string> Extensions
#endif
        {
            get
            {
                if (_extensions == null)
                {
                    var extensions = new JCG.SortedDictionary<char, string>();
                    foreach (var key in _map.Keys)
                        extensions[key] = GetExtensionValue(key);
                    _extensions = extensions.AsReadOnly();
                }
                return _extensions;
            }
        }

        public virtual ISet<string> UnicodeLocaleAttributes
        {
            get
            {
                if (!_map.TryGetValue(UnicodeLocaleExtension.Singleton, out Extension ext) || ext == null)
                {
                    return Collection.EmptySet<string>();
                }
                Debug.Assert(ext is UnicodeLocaleExtension);
                return ((UnicodeLocaleExtension)ext).UnicodeLocaleAttributes;
            }
        }

        public virtual ICollection<string> UnicodeLocaleKeys
        {
            get
            {
                if (!_map.TryGetValue(UnicodeLocaleExtension.Singleton, out Extension ext) || ext == null)
                {
                    return Collection.EmptySet<string>();
                }
                Debug.Assert(ext is UnicodeLocaleExtension);
                return ((UnicodeLocaleExtension)ext).UnicodeLocaleKeys;
            }
        }

        // ICU4N specific - Expose the dictionary so it can be on the public API of UCultureInfo
#if FEATURE_IREADONLYCOLLECTIONS
        public virtual IReadOnlyDictionary<string, string> UnicodeLocales
#else
        public virtual IDictionary<string, string> UnicodeLocales
#endif
        {
            get
            {
                if (!_map.TryGetValue(UnicodeLocaleExtension.Singleton, out Extension ext) || ext == null)
                {
                    return new JCG.Dictionary<string, string>().AsReadOnly();
                }
                Debug.Assert(ext is UnicodeLocaleExtension);
                return ((UnicodeLocaleExtension)ext).UnicodeLocales;
            }
        }

        public virtual string GetUnicodeLocaleType(string unicodeLocaleKey)
        {
            if (!_map.TryGetValue(UnicodeLocaleExtension.Singleton, out Extension ext) || ext == null)
            {
                return null;
            }
            Debug.Assert(ext is UnicodeLocaleExtension);
            return ((UnicodeLocaleExtension)ext).GetUnicodeLocaleType(AsciiUtil.ToLower(unicodeLocaleKey));
        }

        public virtual bool IsEmpty => _map.Count == 0;

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
                        buf.Append(LanguageTag.Separator);
                    }
                    buf.Append(extension);
                }
            }
            if (privuse != null)
            {
                if (buf.Length > 0)
                {
                    buf.Append(LanguageTag.Separator);
                }
                buf.Append(privuse);
            }
            return buf.ToString();
        }


        public override string ToString()
        {
            return _id;
        }

        public virtual string ID => _id;

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
