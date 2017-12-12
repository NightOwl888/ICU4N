using ICU4N.Support.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Impl.Locale
{
    public class UnicodeLocaleExtension : Extension
    {
        public static readonly char SINGLETON = 'u'; // ICU4N TODO: Rename static members to follow .NET Conventions

        private static readonly SortedSet<string> EMPTY_SORTED_SET = new SortedSet<string>(StringComparer.Ordinal);
        private static readonly SortedDictionary<string, string> EMPTY_SORTED_MAP = new SortedDictionary<string, string>(StringComparer.Ordinal);

        private SortedSet<string> _attributes = EMPTY_SORTED_SET;
        private SortedDictionary<string, string> _keywords = EMPTY_SORTED_MAP;

        public static readonly UnicodeLocaleExtension CA_JAPANESE;
        public static readonly UnicodeLocaleExtension NU_THAI;

        static UnicodeLocaleExtension()
        {
            CA_JAPANESE = new UnicodeLocaleExtension();
            CA_JAPANESE._keywords = new SortedDictionary<string, string>(StringComparer.Ordinal);
            CA_JAPANESE._keywords["ca"] = "japanese";
            CA_JAPANESE._value = "ca-japanese";

            NU_THAI = new UnicodeLocaleExtension();
            NU_THAI._keywords = new SortedDictionary<string, string>(StringComparer.Ordinal);
            NU_THAI._keywords["nu"] = "thai";
            NU_THAI._value = "nu-thai";
        }

        private UnicodeLocaleExtension()
                : base(SINGLETON)
        {
        }

        internal UnicodeLocaleExtension(SortedSet<string> attributes, SortedDictionary<string, string> keywords)
                : this()
        {

            if (attributes != null && attributes.Count > 0)
            {
                _attributes = attributes;
            }
            if (keywords != null && keywords.Count > 0)
            {
                _keywords = keywords;
            }

            if (_attributes.Count > 0 || _keywords.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string attribute in _attributes)
                {
                    sb.Append(LanguageTag.SEP).Append(attribute);
                }
                foreach (var keyword in _keywords)
                {
                    string key = keyword.Key;
                    string value = keyword.Value;

                    sb.Append(LanguageTag.SEP).Append(key);
                    if (value.Length > 0)
                    {
                        sb.Append(LanguageTag.SEP).Append(value);
                    }
                }
                _value = sb.ToString(1, sb.Length - 1);   // skip leading '-'
            }
        }

        public virtual ISet<string> GetUnicodeLocaleAttributes() // ICU4N TODO: Make property
        {
            return _attributes.ToUnmodifiableSet();
        }

        public virtual ICollection<string> GetUnicodeLocaleKeys() // ICU4N TODO: Make property
        {
            return _keywords.Keys.ToUnmodifiableCollection();
        }

        public virtual string GetUnicodeLocaleType(string unicodeLocaleKey)
        {
            string result;
            _keywords.TryGetValue(unicodeLocaleKey, out result);
            return result;
        }

        public static bool IsSingletonChar(char c)
        {
            return (SINGLETON == AsciiUtil.ToLower(c));
        }

        public static bool IsAttribute(string s)
        {
            // 3*8alphanum
            return (s.Length >= 3) && (s.Length <= 8) && AsciiUtil.IsAlphaNumericString(s);
        }

        public static bool IsKey(string s)
        {
            // 2alphanum
            return (s.Length == 2) && AsciiUtil.IsAlphaNumericString(s);
        }

        public static bool IsTypeSubtag(string s)
        {
            // 3*8alphanum
            return (s.Length >= 3) && (s.Length <= 8) && AsciiUtil.IsAlphaNumericString(s);
        }

        public static bool IsType(string s)
        {
            // sequence of type subtags delimited by '-'
            int startIdx = 0;
            bool sawSubtag = false;
            while (true)
            {
                int idx = s.IndexOf(LanguageTag.SEP, startIdx);
                string subtag = idx < 0 ? s.Substring(startIdx) : s.Substring(startIdx, idx - startIdx); // ICU4N: Corrected 2nd parameter
                if (!IsTypeSubtag(subtag))
                {
                    return false;
                }
                sawSubtag = true;
                if (idx < 0)
                {
                    break;
                }
                startIdx = idx + 1;
            }
            return sawSubtag && startIdx < s.Length;
        }
    }
}
