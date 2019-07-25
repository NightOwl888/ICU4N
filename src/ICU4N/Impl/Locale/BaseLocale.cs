using ICU4N.Support.Text;
using System;
using System.Text;

namespace ICU4N.Impl.Locale
{
    public sealed class BaseLocale
    {
        private static readonly bool JDKIMPL = false;

        public static readonly string SEP = "_";

        private static readonly Cache CACHE = new Cache();
        public static readonly BaseLocale ROOT = BaseLocale.GetInstance("", "", "", "");

        private string _language = "";
        private string _script = "";
        private string _region = "";
        private string _variant = "";

        private volatile int _hash = 0;

        private BaseLocale(string language, string script, string region, string variant)
        {
            if (language != null)
            {
                _language = AsciiUtil.ToLower(language).Intern();
            }
            if (script != null)
            {
                _script = AsciiUtil.ToTitle(script).Intern();
            }
            if (region != null)
            {
                _region = AsciiUtil.ToUpper(region).Intern();
            }
            if (variant != null)
            {
                if (JDKIMPL)
                {
                    // preserve upper/lower cases
                    _variant = variant.Intern();
                }
                else
                {
                    _variant = AsciiUtil.ToUpper(variant).Intern();
                }
            }
        }

        public static BaseLocale GetInstance(string language, string script, string region, string variant)
        {
            if (JDKIMPL)
            {
                // JDK uses deprecated ISO639.1 language codes for he, yi and id
                if (AsciiUtil.CaseIgnoreMatch(language, "he"))
                {
                    language = "iw";
                }
                else if (AsciiUtil.CaseIgnoreMatch(language, "yi"))
                {
                    language = "ji";
                }
                else if (AsciiUtil.CaseIgnoreMatch(language, "id"))
                {
                    language = "in";
                }
            }
            Key key = new Key(language, script, region, variant);
            BaseLocale baseLocale = CACHE.Get(key);
            return baseLocale;
        }

        // ICU4N TODO: API Holding out hope that we can patch CultureInfo with extension methods
        // similar to these. For now, we should not make these properties.
        public string GetLanguage()
        {
            return _language;
        }

        public string GetScript()
        {
            return _script;
        }

        public string GetRegion()
        {
            return _region;
        }

        public string GetVariant()
        {
            return _variant;
        }


        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (!(obj is BaseLocale))
            {
                return false;
            }
            BaseLocale other = (BaseLocale)obj;
            return GetHashCode() == other.GetHashCode()
                    && _language.Equals(other._language)
                    && _script.Equals(other._script)
                    && _region.Equals(other._region)
                    && _variant.Equals(other._variant);
        }


        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            if (_language.Length > 0)
            {
                buf.Append("language=");
                buf.Append(_language);
            }
            if (_script.Length > 0)
            {
                if (buf.Length > 0)
                {
                    buf.Append(", ");
                }
                buf.Append("script=");
                buf.Append(_script);
            }
            if (_region.Length > 0)
            {
                if (buf.Length > 0)
                {
                    buf.Append(", ");
                }
                buf.Append("region=");
                buf.Append(_region);
            }
            if (_variant.Length > 0)
            {
                if (buf.Length > 0)
                {
                    buf.Append(", ");
                }
                buf.Append("variant=");
                buf.Append(_variant);
            }
            return buf.ToString();
        }


        public override int GetHashCode()
        {
            int h = _hash;
            if (h == 0)
            {
                // Generating a hash value from language, script, region and variant
                for (int i = 0; i < _language.Length; i++)
                {
                    h = 31 * h + _language[i];
                }
                for (int i = 0; i < _script.Length; i++)
                {
                    h = 31 * h + _script[i];
                }
                for (int i = 0; i < _region.Length; i++)
                {
                    h = 31 * h + _region[i];
                }
                for (int i = 0; i < _variant.Length; i++)
                {
                    h = 31 * h + _variant[i];
                }
                _hash = h;
            }
            return h;
        }

        private class Key : IComparable<Key>
        {
            private string _lang = "";
            private string _scrt = "";
            private string _regn = "";
            private string _vart = "";

            private volatile int _hash; // Default to 0

            internal string Lang
            {
                get { return _lang; }
            }

            internal string Scrt
            {
                get { return _scrt; }
            }

            internal string Regn
            {
                get { return _regn; }
            }

            internal string Vart
            {
                get { return _vart; }
            }

            public Key(string language, string script, string region, string variant)
            {
                if (language != null)
                {
                    _lang = language;
                }
                if (script != null)
                {
                    _scrt = script;
                }
                if (region != null)
                {
                    _regn = region;
                }
                if (variant != null)
                {
                    _vart = variant;
                }
            }

            public override bool Equals(object obj)
            {
                if (JDKIMPL)
                {
                    return (this == obj) ||
                            (obj is Key)
                            && AsciiUtil.CaseIgnoreMatch(((Key)obj)._lang, this._lang)
                            && AsciiUtil.CaseIgnoreMatch(((Key)obj)._scrt, this._scrt)
                            && AsciiUtil.CaseIgnoreMatch(((Key)obj)._regn, this._regn)
                            && ((Key)obj)._vart.Equals(_vart); // variant is case sensitive in JDK!
                }
                return (this == obj) ||
                        (obj is Key)
                        && AsciiUtil.CaseIgnoreMatch(((Key)obj)._lang, this._lang)
                        && AsciiUtil.CaseIgnoreMatch(((Key)obj)._scrt, this._scrt)
                        && AsciiUtil.CaseIgnoreMatch(((Key)obj)._regn, this._regn)
                        && AsciiUtil.CaseIgnoreMatch(((Key)obj)._vart, this._vart);
            }

            public virtual int CompareTo(Key other)
            {
                int res = AsciiUtil.CaseIgnoreCompare(this._lang, other._lang);
                if (res == 0)
                {
                    res = AsciiUtil.CaseIgnoreCompare(this._scrt, other._scrt);
                    if (res == 0)
                    {
                        res = AsciiUtil.CaseIgnoreCompare(this._regn, other._regn);
                        if (res == 0)
                        {
                            if (JDKIMPL)
                            {
                                res = this._vart.CompareToOrdinal(other._vart);
                            }
                            else
                            {
                                res = AsciiUtil.CaseIgnoreCompare(this._vart, other._vart);
                            }
                        }
                    }
                }
                return res;
            }

            public override int GetHashCode()
            {
                int h = _hash;
                if (h == 0)
                {
                    // Generating a hash value from language, script, region and variant
                    for (int i = 0; i < _lang.Length; i++)
                    {
                        h = 31 * h + AsciiUtil.ToLower(_lang[i]);
                    }
                    for (int i = 0; i < _scrt.Length; i++)
                    {
                        h = 31 * h + AsciiUtil.ToLower(_scrt[i]);
                    }
                    for (int i = 0; i < _regn.Length; i++)
                    {
                        h = 31 * h + AsciiUtil.ToLower(_regn[i]);
                    }
                    for (int i = 0; i < _vart.Length; i++)
                    {
                        if (JDKIMPL)
                        {
                            h = 31 * h + _vart[i];
                        }
                        else
                        {
                            h = 31 * h + AsciiUtil.ToLower(_vart[i]);
                        }
                    }
                    _hash = h;
                }
                return h;
            }

            public static Key Normalize(Key key)
            {
                string lang = AsciiUtil.ToLower(key._lang).Intern();
                string scrt = AsciiUtil.ToTitle(key._scrt).Intern();
                string regn = AsciiUtil.ToUpper(key._regn).Intern();
                string vart;
                if (JDKIMPL)
                {
                    // preserve upper/lower cases
                    vart = key._vart.Intern();
                }
                else
                {
                    vart = AsciiUtil.ToUpper(key._vart).Intern();
                }
                return new Key(lang, scrt, regn, vart);
            }
        }

        private class Cache : LocaleObjectCache<Key, BaseLocale>
        {
            public Cache()
            {
            }

            protected override Key NormalizeKey(Key key)
            {
                return Key.Normalize(key);
            }

            protected override BaseLocale CreateObject(Key key)
            {
                return new BaseLocale(key.Lang, key.Scrt, key.Regn, key.Vart);
            }
        }
    }
}
