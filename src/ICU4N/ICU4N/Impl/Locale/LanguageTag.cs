using ICU4N.Support.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ICU4N.Impl.Locale
{
    public class LanguageTag
    {
        private static readonly bool JDKIMPL = false;

        //
        // static fields
        //
        public static readonly string SEP = "-"; // ICU4N TODO: API - rename to follow .NET Conventions
        public static readonly string PRIVATEUSE = "x";
        public static string UNDETERMINED = "und";
        public static readonly string PRIVUSE_VARIANT_PREFIX = "lvariant";

        //
        // Language subtag fields
        //
        private string _language = "";      // language subtag
        private string _script = "";        // script subtag
        private string _region = "";        // region subtag
        private string _privateuse = "";    // privateuse

        private IList<string> _extlangs = new List<string>();   // extlang subtags
        private IList<string> _variants = new List<string>();   // variant subtags
        private IList<string> _extensions = new List<string>(); // extensions

        // Map contains grandfathered tags and its preferred mappings from
        // http://www.ietf.org/rfc/rfc5646.txt
        private static readonly IDictionary<AsciiUtil.CaseInsensitiveKey, string[]> GRANDFATHERED = // ICU4N TODO: API - rename to follow .NET Conventions
            new Dictionary<AsciiUtil.CaseInsensitiveKey, string[]>();

        static LanguageTag()
        {
            // grandfathered = irregular           ; non-redundant tags registered
            //               / regular             ; during the RFC 3066 era
            //
            // irregular     = "en-GB-oed"         ; irregular tags do not match
            //               / "i-ami"             ; the 'langtag' production and
            //               / "i-bnn"             ; would not otherwise be
            //               / "i-default"         ; considered 'well-formed'
            //               / "i-enochian"        ; These tags are all valid,
            //               / "i-hak"             ; but most are deprecated
            //               / "i-klingon"         ; in favor of more modern
            //               / "i-lux"             ; subtags or subtag
            //               / "i-mingo"           ; combination
            //               / "i-navajo"
            //               / "i-pwn"
            //               / "i-tao"
            //               / "i-tay"
            //               / "i-tsu"
            //               / "sgn-BE-FR"
            //               / "sgn-BE-NL"
            //               / "sgn-CH-DE"
            //
            // regular       = "art-lojban"        ; these tags match the 'langtag'
            //               / "cel-gaulish"       ; production, but their subtags
            //               / "no-bok"            ; are not extended language
            //               / "no-nyn"            ; or variant subtags: their meaning
            //               / "zh-guoyu"          ; is defined by their registration
            //               / "zh-hakka"          ; and all of these are deprecated
            //               / "zh-min"            ; in favor of a more modern
            //               / "zh-min-nan"        ; subtag or sequence of subtags
            //               / "zh-xiang"

            string[][] entries = {
                // new string[] {"tag",         "preferred"},
                new string[] {"art-lojban",  "jbo"},
                new string[] {"cel-gaulish", "xtg-x-cel-gaulish"},   // fallback
                new string[]  {"en-GB-oed",   "en-GB-x-oed"},         // fallback
                new string[] {"i-ami",       "ami"},
                new string[] {"i-bnn",       "bnn"},
                new string[] {"i-default",   "en-x-i-default"},      // fallback
                new string[] {"i-enochian",  "und-x-i-enochian"},    // fallback
                new string[] {"i-hak",       "hak"},
                new string[] {"i-klingon",   "tlh"},
                new string[] {"i-lux",       "lb"},
                new string[] {"i-mingo",     "see-x-i-mingo"},       // fallback
                new string[] {"i-navajo",    "nv"},
                new string[] {"i-pwn",       "pwn"},
                new string[] {"i-tao",       "tao"},
                new string[] {"i-tay",       "tay"},
                new string[] {"i-tsu",       "tsu"},
                new string[] {"no-bok",      "nb"},
                new string[] {"no-nyn",      "nn"},
                new string[] {"sgn-BE-FR",   "sfb"},
                new string[] {"sgn-BE-NL",   "vgt"},
                new string[] {"sgn-CH-DE",   "sgg"},
                new string[] {"zh-guoyu",    "cmn"},
                new string[] {"zh-hakka",    "hak"},
                new string[] {"zh-min",      "nan-x-zh-min"},        // fallback
                new string[] {"zh-min-nan",  "nan"},
                new string[] {"zh-xiang",    "hsn"},
            };
            foreach (string[] e in entries)
            {
                GRANDFATHERED[new AsciiUtil.CaseInsensitiveKey(e[0])] = e;
            }
        }

        private LanguageTag()
        {
        }

        /// <summary>
        /// BNF in RFC5464
        /// </summary>
        /// <remarks>
        /// Language-Tag  = langtag             ; normal language tags
        ///               / privateuse          ; private use tag
        ///               / grandfathered       ; grandfathered tags
        ///
        ///
        /// langtag       = language
        ///                 ["-" script]
        ///                 ["-" region]
        ///                 *("-" variant)
        ///                 *("-" extension)
        ///                 ["-" privateuse]
        ///
        /// language      = 2*3ALPHA            ; shortest ISO 639 code
        ///                 ["-" extlang]       ; sometimes followed by
        ///                                     ; extended language subtags
        ///               / 4ALPHA              ; or reserved for future use
        ///               / 5*8ALPHA            ; or registered language subtag
        ///
        /// extlang       = 3ALPHA              ; selected ISO 639 codes
        ///                 *2("-" 3ALPHA)      ; permanently reserved
        ///
        /// script        = 4ALPHA              ; ISO 15924 code
        ///
        /// region        = 2ALPHA              ; ISO 3166-1 code
        ///               / 3DIGIT              ; UN M.49 code
        ///
        /// variant       = 5*8alphanum         ; registered variants
        ///               / (DIGIT 3alphanum)
        ///
        /// extension     = singleton 1*("-" (2*8alphanum))
        ///
        ///                                     ; Single alphanumerics
        ///                                     ; "x" reserved for private use
        /// singleton     = DIGIT               ; 0 - 9
        ///               / %x41-57             ; A - W
        ///               / %x59-5A             ; Y - Z
        ///               / %x61-77             ; a - w
        ///               / %x79-7A             ; y - z
        ///
        /// privateuse    = "x" 1*("-" (1*8alphanum))
        /// </remarks>
        public static LanguageTag Parse(string languageTag, ParseStatus sts)
        {
            if (sts == null)
            {
                sts = new ParseStatus();
            }
            else
            {
                sts.Reset();
            }

            StringTokenEnumerator itr;
            bool isGrandfathered = false;

            // Check if the tag is grandfathered
            string[] gfmap;
            if (GRANDFATHERED.TryGetValue(new AsciiUtil.CaseInsensitiveKey(languageTag), out gfmap) && gfmap != null)
            {
                // use preferred mapping
                itr = new StringTokenEnumerator(gfmap[1], SEP);
                isGrandfathered = true;
            }
            else
            {
                itr = new StringTokenEnumerator(languageTag, SEP);
            }

            // ICU4N: Move to the first element
            itr.MoveNext();
            LanguageTag tag = new LanguageTag();

            // langtag must start with either language or privateuse
            if (tag.ParseLanguage(itr, sts))
            {
                tag.ParseExtlangs(itr, sts);
                tag.ParseScript(itr, sts);
                tag.ParseRegion(itr, sts);
                tag.ParseVariants(itr, sts);
                tag.ParseExtensions(itr, sts);
            }
            tag.ParsePrivateuse(itr, sts);

            if (isGrandfathered)
            {
                // Grandfathered tag is replaced with a well-formed tag above.
                // However, the parsed length must be the original tag length.
                Debug.Assert(itr.IsDone);
                Debug.Assert(!sts.IsError);
                sts.ParseLength = languageTag.Length;
            }
            else if (!itr.IsDone && !sts.IsError)
            {
                string s = itr.Current;
                sts.ErrorIndex = itr.CurrentStart;
                if (s.Length == 0)
                {
                    sts.ErrorMessage = "Empty subtag";
                }
                else
                {
                    sts.ErrorMessage = "Invalid subtag: " + s;
                }
            }

            return tag;
        }

        //
        // Language subtag parsers
        //

        private bool ParseLanguage(StringTokenEnumerator itr, ParseStatus sts)
        {
            if (itr.IsDone || sts.IsError)
            {
                return false;
            }

            bool found = false;

            string s = itr.Current;
            if (IsLanguage(s))
            {
                found = true;
                _language = s;
                sts.ParseLength = itr.CurrentEnd;
                itr.MoveNext();
            }

            return found;
        }

        private bool ParseExtlangs(StringTokenEnumerator itr, ParseStatus sts)
        {
            if (itr.IsDone || sts.IsError)
            {
                return false;
            }

            bool found = false;

            while (!itr.IsDone)
            {
                string s = itr.Current;
                if (!IsExtlang(s))
                {
                    break;
                }
                found = true;
                if (!_extlangs.Any())
                {
                    _extlangs = new List<string>(3);
                }
                _extlangs.Add(s);
                sts.ParseLength = itr.CurrentEnd;
                itr.MoveNext();

                if (_extlangs.Count == 3)
                {
                    // Maximum 3 extlangs
                    break;
                }
            }

            return found;
        }

        private bool ParseScript(StringTokenEnumerator itr, ParseStatus sts)
        {
            if (itr.IsDone || sts.IsError)
            {
                return false;
            }

            bool found = false;

            string s = itr.Current;
            if (IsScript(s))
            {
                found = true;
                _script = s;
                sts.ParseLength = itr.CurrentEnd;
                itr.MoveNext();
            }

            return found;
        }

        private bool ParseRegion(StringTokenEnumerator itr, ParseStatus sts)
        {
            if (itr.IsDone || sts.IsError)
            {
                return false;
            }

            bool found = false;

            string s = itr.Current;
            if (IsRegion(s))
            {
                found = true;
                _region = s;
                sts.ParseLength = itr.CurrentEnd;
                itr.MoveNext();
            }

            return found;
        }

        private bool ParseVariants(StringTokenEnumerator itr, ParseStatus sts)
        {
            if (itr.IsDone || sts.IsError)
            {
                return false;
            }

            bool found = false;

            while (!itr.IsDone)
            {
                string s = itr.Current;
                if (!IsVariant(s))
                {
                    break;
                }
                found = true;
                if (!_variants.Any())
                {
                    _variants = new List<string>(3);
                }
                _variants.Add(s);
                sts.ParseLength = itr.CurrentEnd;
                itr.MoveNext();
            }

            return found;
        }

        private bool ParseExtensions(StringTokenEnumerator itr, ParseStatus sts)
        {
            if (itr.IsDone || sts.IsError)
            {
                return false;
            }

            bool found = false;

            while (!itr.IsDone)
            {
                string s = itr.Current;
                if (IsExtensionSingleton(s))
                {
                    int start = itr.CurrentStart;
                    string singleton = s;
                    StringBuilder sb = new StringBuilder(singleton);

                    itr.MoveNext();
                    while (!itr.IsDone)
                    {
                        s = itr.Current;
                        if (IsExtensionSubtag(s))
                        {
                            sb.Append(SEP).Append(s);
                            sts.ParseLength = itr.CurrentEnd;
                        }
                        else
                        {
                            break;
                        }
                        itr.MoveNext();
                    }

                    if (sts.ParseLength <= start)
                    {
                        sts.ErrorIndex = start;
                        sts.ErrorMessage = "Incomplete extension '" + singleton + "'";
                        break;
                    }

                    if (_extensions.Count == 0)
                    {
                        _extensions = new List<String>(4);
                    }
                    _extensions.Add(sb.ToString());
                    found = true;
                }
                else
                {
                    break;
                }
            }
            return found;
        }

        private bool ParsePrivateuse(StringTokenEnumerator itr, ParseStatus sts)
        {
            if (itr.IsDone || sts.IsError)
            {
                return false;
            }

            bool found = false;

            string s = itr.Current;
            if (IsPrivateusePrefix(s))
            {
                int start = itr.CurrentStart;
                StringBuilder sb = new StringBuilder(s);

                itr.MoveNext();
                while (!itr.IsDone)
                {
                    s = itr.Current;
                    if (!IsPrivateuseSubtag(s))
                    {
                        break;
                    }
                    sb.Append(SEP).Append(s);
                    sts.ParseLength = itr.CurrentEnd;

                    itr.MoveNext();
                }

                if (sts.ParseLength <= start)
                {
                    // need at least 1 private subtag
                    sts.ErrorIndex = start;
                    sts.ErrorMessage = "Incomplete privateuse";
                }
                else
                {
                    _privateuse = sb.ToString();
                    found = true;
                }
            }

            return found;
        }

        public static LanguageTag ParseLocale(BaseLocale baseLocale, LocaleExtensions localeExtensions)
        {
            LanguageTag tag = new LanguageTag();

            string language = baseLocale.GetLanguage();
            string script = baseLocale.GetScript();
            string region = baseLocale.GetRegion();
            string variant = baseLocale.GetVariant();

            bool hasSubtag = false;

            string privuseVar = null;   // store ill-formed variant subtags

            if (language.Length > 0 && IsLanguage(language))
            {
                // Convert a deprecated language code used by Java to
                // a new code
                if (language.Equals("iw"))
                {
                    language = "he";
                }
                else if (language.Equals("ji"))
                {
                    language = "yi";
                }
                else if (language.Equals("in"))
                {
                    language = "id";
                }
                tag._language = language;
            }

            if (script.Length > 0 && IsScript(script))
            {
                tag._script = CanonicalizeScript(script);
                hasSubtag = true;
            }

            if (region.Length > 0 && IsRegion(region))
            {
                tag._region = CanonicalizeRegion(region);
                hasSubtag = true;
            }

            // ICU4N TODO: Remove ?
            if (JDKIMPL)
            {
                // Special handling for no_NO_NY - use nn_NO for language tag
                if (tag._language.Equals("no") && tag._region.Equals("NO") && variant.Equals("NY")) // ICU4N TODO: Fix this handling for .NET (no-NO is not reliable across platforms)
                {
                    tag._language = "nn";
                    variant = "";
                }
            }

            if (variant.Length > 0)
            {
                List<string> variants = null;
                StringTokenEnumerator varitr = new StringTokenEnumerator(variant, BaseLocale.SEP);
                while (varitr.MoveNext())
                {
                    string var = varitr.Current;
                    if (!IsVariant(var))
                    {
                        break;
                    }
                    if (variants == null)
                    {
                        variants = new List<string>();
                    }
                    if (JDKIMPL)
                    {
                        variants.Add(var);  // Do not canonicalize!
                    }
                    else
                    {
                        variants.Add(CanonicalizeVariant(var));
                    }
                }
                if (variants != null)
                {
                    tag._variants = variants;
                    hasSubtag = true;
                }
                if (!varitr.IsDone)
                {
                    // ill-formed variant subtags
                    StringBuilder buf = new StringBuilder();
                    while (!varitr.IsDone)
                    {
                        string prvv = varitr.Current;
                        if (!IsPrivateuseSubtag(prvv))
                        {
                            // cannot use private use subtag - truncated
                            break;
                        }
                        if (buf.Length > 0)
                        {
                            buf.Append(SEP);
                        }
                        if (!JDKIMPL)
                        {
                            prvv = AsciiUtil.ToLowerString(prvv);
                        }
                        buf.Append(prvv);
                        varitr.MoveNext();
                    }
                    if (buf.Length > 0)
                    {
                        privuseVar = buf.ToString();
                    }
                }
            }

            List<string> extensions = null;
            string privateuse = null;

            var locextKeys = localeExtensions.Keys;
            foreach (char locextKey in locextKeys)
            {
                Extension ext = localeExtensions.GetExtension(locextKey);
                if (IsPrivateusePrefixChar(locextKey))
                {
                    privateuse = ext.Value;
                }
                else
                {
                    if (extensions == null)
                    {
                        extensions = new List<string>();
                    }
                    extensions.Add(locextKey.ToString() + SEP + ext.Value);
                }
            }

            if (extensions != null)
            {
                tag._extensions = extensions;
                hasSubtag = true;
            }

            // append ill-formed variant subtags to private use
            if (privuseVar != null)
            {
                if (privateuse == null)
                {
                    privateuse = PRIVUSE_VARIANT_PREFIX + SEP + privuseVar;
                }
                else
                {
                    privateuse = privateuse + SEP + PRIVUSE_VARIANT_PREFIX + SEP + privuseVar.Replace(BaseLocale.SEP, SEP);
                }
            }

            if (privateuse != null)
            {
                tag._privateuse = privateuse;
            }

            if (tag._language.Length == 0 && (hasSubtag || privateuse == null))
            {
                // use lang "und" when 1) no language is available AND
                // 2) any of other subtags other than private use are available or
                // no private use tag is available
                tag._language = UNDETERMINED;
            }

            return tag;
        }

        //
        // Getter methods for language subtag fields
        //

        public virtual string Language
        {
            get { return _language; }
        }

        public virtual IList<string> Extlangs
        {
            get { return _extlangs.ToUnmodifiableList(); }
        }

        public virtual string Script
        {
            get { return _script; }
        }

        public virtual string Region
        {
            get { return _region; }
        }

        public virtual IList<string> Variants
        {
            get { return _variants.ToUnmodifiableList(); }
        }

        public virtual IList<string> Extensions
        {
            get { return _extensions.ToUnmodifiableList(); }
        }

        public virtual string PrivateUse
        {
            get { return _privateuse; }
        }

        //
        // Language subtag syntax checking methods
        //

        public static bool IsLanguage(string s)
        {
            // language      = 2*3ALPHA            ; shortest ISO 639 code
            //                 ["-" extlang]       ; sometimes followed by
            //                                     ;   extended language subtags
            //               / 4ALPHA              ; or reserved for future use
            //               / 5*8ALPHA            ; or registered language subtag
            return (s.Length >= 2) && (s.Length <= 8) && AsciiUtil.IsAlphaString(s);
        }

        public static bool IsExtlang(string s)
        {
            // extlang       = 3ALPHA              ; selected ISO 639 codes
            //                 *2("-" 3ALPHA)      ; permanently reserved
            return (s.Length == 3) && AsciiUtil.IsAlphaString(s);
        }

        public static bool IsScript(string s)
        {
            // script        = 4ALPHA              ; ISO 15924 code
            return (s.Length == 4) && AsciiUtil.IsAlphaString(s);
        }

        public static bool IsRegion(string s)
        {
            // region        = 2ALPHA              ; ISO 3166-1 code
            //               / 3DIGIT              ; UN M.49 code
            return ((s.Length == 2) && AsciiUtil.IsAlphaString(s))
                    || ((s.Length == 3) && AsciiUtil.IsNumericString(s));
        }

        public static bool IsVariant(string s)
        {
            // variant       = 5*8alphanum         ; registered variants
            //               / (DIGIT 3alphanum)
            int len = s.Length;
            if (len >= 5 && len <= 8)
            {
                return AsciiUtil.IsAlphaNumericString(s);
            }
            if (len == 4)
            {
                return AsciiUtil.IsNumeric(s[0])
                        && AsciiUtil.IsAlphaNumeric(s[1])
                        && AsciiUtil.IsAlphaNumeric(s[2])
                        && AsciiUtil.IsAlphaNumeric(s[3]);
            }
            return false;
        }

        public static bool IsExtensionSingleton(string s)
        {
            // singleton     = DIGIT               ; 0 - 9
            //               / %x41-57             ; A - W
            //               / %x59-5A             ; Y - Z
            //               / %x61-77             ; a - w
            //               / %x79-7A             ; y - z

            return (s.Length == 1)
                    && AsciiUtil.IsAlphaString(s)
                    && !AsciiUtil.CaseIgnoreMatch(PRIVATEUSE, s);
        }

        public static bool IsExtensionSingletonChar(char c)
        {
            return IsExtensionSingleton(new string(new char[] { c }));
        }

        public static bool IsExtensionSubtag(string s)
        {
            // extension     = singleton 1*("-" (2*8alphanum))
            return (s.Length >= 2) && (s.Length <= 8) && AsciiUtil.IsAlphaNumericString(s);
        }

        public static bool IsPrivateusePrefix(string s)
        {
            // privateuse    = "x" 1*("-" (1*8alphanum))
            return (s.Length == 1)
                    && AsciiUtil.CaseIgnoreMatch(PRIVATEUSE, s);
        }

        public static bool IsPrivateusePrefixChar(char c)
        {
            return (AsciiUtil.CaseIgnoreMatch(PRIVATEUSE, new string(new char[] { c })));
        }

        public static bool IsPrivateuseSubtag(string s)
        {
            // privateuse    = "x" 1*("-" (1*8alphanum))
            return (s.Length >= 1) && (s.Length <= 8) && AsciiUtil.IsAlphaNumericString(s);
        }

        //
        // Language subtag canonicalization methods
        //

        public static string CanonicalizeLanguage(string s)
        {
            return AsciiUtil.ToLowerString(s);
        }

        public static string CanonicalizeExtlang(string s)
        {
            return AsciiUtil.ToLowerString(s);
        }

        public static string CanonicalizeScript(string s)
        {
            return AsciiUtil.ToTitleString(s);
        }

        public static string CanonicalizeRegion(string s)
        {
            return AsciiUtil.ToUpperString(s);
        }

        public static string CanonicalizeVariant(string s)
        {
            return AsciiUtil.ToLowerString(s);
        }

        public static string CanonicalizeExtension(string s)
        {
            return AsciiUtil.ToLowerString(s);
        }

        public static string CanonicalizeExtensionSingleton(string s)
        {
            return AsciiUtil.ToLowerString(s);
        }

        public static string CanonicalizeExtensionSubtag(string s)
        {
            return AsciiUtil.ToLowerString(s);
        }

        public static string CanonicalizePrivateuse(string s)
        {
            return AsciiUtil.ToLowerString(s);
        }

        public static string CanonicalizePrivateuseSubtag(string s)
        {
            return AsciiUtil.ToLowerString(s);
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (_language.Length > 0)
            {
                sb.Append(_language);

                foreach (string extlang in _extlangs)
                {
                    sb.Append(SEP).Append(extlang);
                }

                if (_script.Length > 0)
                {
                    sb.Append(SEP).Append(_script);
                }

                if (_region.Length > 0)
                {
                    sb.Append(SEP).Append(_region);
                }

                foreach (string variant in _variants)
                {
                    sb.Append(SEP).Append(variant);
                }

                foreach (string extension in _extensions)
                {
                    sb.Append(SEP).Append(extension);
                }
            }
            if (_privateuse.Length > 0)
            {
                if (sb.Length > 0)
                {
                    sb.Append(SEP);
                }
                sb.Append(_privateuse);
            }

            return sb.ToString();
        }
    }
}
