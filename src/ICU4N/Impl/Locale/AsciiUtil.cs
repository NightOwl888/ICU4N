using J2N.Text;
using System.Text;

namespace ICU4N.Impl.Locale
{
    public sealed class AsciiUtil
    {
        public static bool CaseIgnoreMatch(string s1, string s2)
        {
            //if (Utility.SameObjects(s1, s2))
            if (ReferenceEquals(s1, s2))
            {
                return true;
            }
            int len = s1.Length;
            if (len != s2.Length)
            {
                return false;
            }
            int i = 0;
            while (i < len)
            {
                char c1 = s1[i];
                char c2 = s2[i];
                if (c1 != c2 && ToLower(c1) != ToLower(c2))
                {
                    break;
                }
                i++;
            }
            return (i == len);
        }

        public static int CaseIgnoreCompare(string s1, string s2)
        {
            //if (Utility.SameObjects(s1, s2))
            if (ReferenceEquals(s1, s2))
            {
                return 0;
            }
            return AsciiUtil.ToLower(s1).CompareToOrdinal(AsciiUtil.ToLower(s2));
        }


        public static char ToUpper(char c)
        {
            if (c >= 'a' && c <= 'z')
            {
                c -= (char)0x20;
            }
            return c;
        }

        public static char ToLower(char c)
        {
            if (c >= 'A' && c <= 'Z')
            {
                c += (char)0x20;
            }
            return c;
        }

        public static string ToLower(string s) // ICU4N specific - renamed from ToLowerString()
        {
            int idx = 0;
            for (; idx < s.Length; idx++)
            {
                char c = s[idx];
                if (c >= 'A' && c <= 'Z')
                {
                    break;
                }
            }
            if (idx == s.Length)
            {
                return s;
            }
            StringBuilder buf = new StringBuilder(s.Substring(0, idx - 0)); // ICU4N: Checked 2nd parameter
            for (; idx < s.Length; idx++)
            {
                buf.Append(ToLower(s[idx]));
            }
            return buf.ToString();
        }

        public static string ToUpper(string s) // ICU4N specific - renamed from ToUpperString()
        {
            int idx = 0;
            for (; idx < s.Length; idx++)
            {
                char c = s[idx];
                if (c >= 'a' && c <= 'z')
                {
                    break;
                }
            }
            if (idx == s.Length)
            {
                return s;
            }
            StringBuilder buf = new StringBuilder(s.Substring(0, idx - 0)); // ICU4N: Checked 2nd parameter
            for (; idx < s.Length; idx++)
            {
                buf.Append(ToUpper(s[idx]));
            }
            return buf.ToString();
        }

        public static string ToTitle(string s) // ICU4N specific - renamed from ToTitleString()
        {
            if (s.Length == 0)
            {
                return s;
            }
            int idx = 0;
            char c = s[idx];
            if (!(c >= 'a' && c <= 'z'))
            {
                for (idx = 1; idx < s.Length; idx++)
                {
                    if (c >= 'A' && c <= 'Z')
                    {
                        break;
                    }
                }
            }
            if (idx == s.Length)
            {
                return s;
            }
            StringBuilder buf = new StringBuilder(s.Substring(0, idx - 0)); // ICU4N: Checked 2nd parameter
            if (idx == 0)
            {
                buf.Append(ToUpper(s[idx]));
                idx++;
            }
            for (; idx < s.Length; idx++)
            {
                buf.Append(ToLower(s[idx]));
            }
            return buf.ToString();
        }

        public static bool IsAlpha(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        }

        public static bool IsAlpha(string s) // ICU4N specific - renamed from ToAlphaString()
        {
            bool b = true;
            for (int i = 0; i < s.Length; i++)
            {
                if (!IsAlpha(s[i]))
                {
                    b = false;
                    break;
                }
            }
            return b;
        }

        public static bool IsNumeric(char c)
        {
            return (c >= '0' && c <= '9');
        }

        public static bool IsNumeric(string s) // ICU4N specific - renamed from IsNumericString()
        {
            bool b = true;
            for (int i = 0; i < s.Length; i++)
            {
                if (!IsNumeric(s[i]))
                {
                    b = false;
                    break;
                }
            }
            return b;
        }

        public static bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsNumeric(c);
        }

        public static bool IsAlphaNumeric(string s) // ICU4N specific - renamed from IsAlphaNumericString()
        {
            bool b = true;
            for (int i = 0; i < s.Length; i++)
            {
                if (!IsAlphaNumeric(s[i]))
                {
                    b = false;
                    break;
                }
            }
            return b;
        }
    }
    public class AsciiCaseInsensitiveKey // ICU4N specific - renamed from CaseInsensitiveKey
    {
        private readonly string _key;
        private readonly int _hash;

        public AsciiCaseInsensitiveKey(string key)
        {
            _key = key;
            _hash = AsciiUtil.ToLower(key).GetHashCode();
        }

        public override bool Equals(object o)
        {
            if (ReferenceEquals(this, o))
                return true;

            if (o is AsciiCaseInsensitiveKey other)
                return AsciiUtil.CaseIgnoreMatch(_key, other._key);

            return false;
        }

        public override int GetHashCode()
        {
            return _hash;
        }
    }
}
