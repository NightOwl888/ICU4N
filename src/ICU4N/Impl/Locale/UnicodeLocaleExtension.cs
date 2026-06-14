using ICU4N.Text;
using J2N.Collections.Generic.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using JCG = J2N.Collections.Generic;

namespace ICU4N.Impl.Locale
{
    public class UnicodeLocaleExtension : Extension
    {
        private const int CharStackBufferSize = 32;

        public const char Singleton = 'u';

        private static readonly JCG.SortedSet<string> EMPTY_SORTED_SET = new JCG.SortedSet<string>(StringComparer.Ordinal);
        private static readonly JCG.SortedDictionary<string, string> EMPTY_SORTED_MAP = new JCG.SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private JCG.SortedSet<string> _attributes = EMPTY_SORTED_SET;
        private JCG.SortedDictionary<string, string> _keywords = EMPTY_SORTED_MAP;

        public static readonly UnicodeLocaleExtension CalendarJapanese = LoadCalendarJapanese();
        public static readonly UnicodeLocaleExtension NumberThai = LoadNumberThai();

        private static UnicodeLocaleExtension LoadCalendarJapanese() // ICU4N: Avoid static constructor
        {
            return new UnicodeLocaleExtension
            {
                _keywords = new JCG.SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["ca"] = "japanese"
                },
                Value = "ca-japanese"
            };
        }
        private static UnicodeLocaleExtension LoadNumberThai() // ICU4N: Avoid static constructor
        {
            return new UnicodeLocaleExtension
            {
                _keywords = new JCG.SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["nu"] = "thai"
                },
                Value = "nu-thai"
            };
        }

        private UnicodeLocaleExtension()
            : base(Singleton)
        {
        }

        internal UnicodeLocaleExtension(JCG.SortedSet<string> attributes, JCG.SortedDictionary<string, string> keywords)
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
                ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
                try
                {
                    foreach (string attribute in _attributes)
                    {
                        sb.Append(LanguageTag.Separator);
                        sb.Append(attribute);
                    }
                    foreach (var keyword in _keywords)
                    {
                        string key = keyword.Key;
                        string value = keyword.Value;

                        sb.Append(LanguageTag.Separator);
                        sb.Append(key);
                        if (value.Length > 0)
                        {
                            sb.Append(LanguageTag.Separator);
                            sb.Append(value);
                        }
                    }
                    Value = sb.AsSpan(1, sb.Length - 1).ToString();   // skip leading '-'
                }
                finally
                {
                    sb.Dispose();
                }
            }
        }

        public virtual ISet<string> UnicodeLocaleAttributes => _attributes.AsReadOnly(); // ICU4N TODO: API Make return type IReadOnlyCollection ?

        public virtual ICollection<string> UnicodeLocaleKeys => _keywords.Keys.AsReadOnly(); // ICU4N TODO: API Make return type IReadOnlyCollection

#if FEATURE_IREADONLYCOLLECTIONS
        public virtual IReadOnlyDictionary<string, string> UnicodeLocales
#else
        public virtual IDictionary<string, string> UnicodeLocales
#endif
            => JCG.Extensions.DictionaryExtensions.AsReadOnly(_keywords);

        public virtual string GetUnicodeLocaleType(string unicodeLocaleKey)
        {
            _keywords.TryGetValue(unicodeLocaleKey, out string result);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSingletonChar(char c)
        {
            return (Singleton == AsciiUtil.ToLower(c));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAttribute(string s)
        {
            // 3*8alphanum
            return (s.Length >= 3) && (s.Length <= 8) && AsciiUtil.IsAlphaNumeric(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAttribute(ReadOnlySpan<char> s)
        {
            // 3*8alphanum
            return (s.Length >= 3) && (s.Length <= 8) && AsciiUtil.IsAlphaNumeric(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKey(string s)
        {
            // 2alphanum
            return (s.Length == 2) && AsciiUtil.IsAlphaNumeric(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKey(ReadOnlySpan<char> s)
        {
            // 2alphanum
            return (s.Length == 2) && AsciiUtil.IsAlphaNumeric(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsTypeSubtag(string s)
        {
            // 3*8alphanum
            return (s.Length >= 3) && (s.Length <= 8) && AsciiUtil.IsAlphaNumeric(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsTypeSubtag(ReadOnlySpan<char> s)
        {
            // 3*8alphanum
            return (s.Length >= 3) && (s.Length <= 8) && AsciiUtil.IsAlphaNumeric(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsType(string s)
        {
            return IsType(s.AsSpan());
        }

        public static bool IsType(ReadOnlySpan<char> s)
        {
            // sequence of type subtags delimited by '-'
            int startIdx = 0;
            bool sawSubtag = false;
            while (true)
            {
                int idx = s.IndexOf(LanguageTag.Separator, startIdx);
                ReadOnlySpan<char> subtag = idx < 0 ? s.Slice(startIdx) : s.Slice(startIdx, idx - startIdx); // ICU4N: Corrected 2nd parameter
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
