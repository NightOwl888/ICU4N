using ICU4N.Support.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Impl.Locale
{
    public class UnicodeLocaleExtension : Extension
    {
        public const char Singleton = 'u';

        private static readonly SortedSet<string> EMPTY_SORTED_SET = new SortedSet<string>(StringComparer.Ordinal);
        private static readonly SortedDictionary<string, string> EMPTY_SORTED_MAP = new SortedDictionary<string, string>(StringComparer.Ordinal);

        private SortedSet<string> _attributes = EMPTY_SORTED_SET;
        private SortedDictionary<string, string> _keywords = EMPTY_SORTED_MAP;

        public static readonly UnicodeLocaleExtension CalendarJapanese;
        public static readonly UnicodeLocaleExtension NumberThai;

        static UnicodeLocaleExtension() // ICU4N TODO: Avoid static constructor
        {
            CalendarJapanese = new UnicodeLocaleExtension();
            CalendarJapanese._keywords = new SortedDictionary<string, string>(StringComparer.Ordinal);
            CalendarJapanese._keywords["ca"] = "japanese";
            CalendarJapanese.m_value = "ca-japanese";

            NumberThai = new UnicodeLocaleExtension();
            NumberThai._keywords = new SortedDictionary<string, string>(StringComparer.Ordinal);
            NumberThai._keywords["nu"] = "thai";
            NumberThai.m_value = "nu-thai";
        }

        private UnicodeLocaleExtension()
            : base(Singleton)
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
                    sb.Append(LanguageTag.Separator).Append(attribute);
                }
                foreach (var keyword in _keywords)
                {
                    string key = keyword.Key;
                    string value = keyword.Value;

                    sb.Append(LanguageTag.Separator).Append(key);
                    if (value.Length > 0)
                    {
                        sb.Append(LanguageTag.Separator).Append(value);
                    }
                }
                m_value = sb.ToString(1, sb.Length - 1);   // skip leading '-'
            }
        }

        public virtual ISet<string> GetUnicodeLocaleAttributes() // ICU4N TODO: API Make property
        {
            return _attributes.ToUnmodifiableSet();
        }

        public virtual ICollection<string> GetUnicodeLocaleKeys() // ICU4N TODO: API Make property
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
            return (Singleton == AsciiUtil.ToLower(c));
        }

        public static bool IsAttribute(string s)
        {
            // 3*8alphanum
            return (s.Length >= 3) && (s.Length <= 8) && AsciiUtil.IsAlphaNumeric(s);
        }

        public static bool IsKey(string s)
        {
            // 2alphanum
            return (s.Length == 2) && AsciiUtil.IsAlphaNumeric(s);
        }

        public static bool IsTypeSubtag(string s)
        {
            // 3*8alphanum
            return (s.Length >= 3) && (s.Length <= 8) && AsciiUtil.IsAlphaNumeric(s);
        }

        public static bool IsType(string s)
        {
            // sequence of type subtags delimited by '-'
            int startIdx = 0;
            bool sawSubtag = false;
            while (true)
            {
                int idx = s.IndexOf(LanguageTag.Separator, startIdx, StringComparison.Ordinal);
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
