using ICU4N.Support.Collections;
using ICU4N.Text;
using J2N.Collections.Generic.Extensions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
#nullable enable

namespace ICU4N.Impl.Locale
{
    public ref struct LanguageTag
    {
        private const int CharStackBufferSize = 32;

        public bool IsDefault =>_language == string.Empty &&
            _script == string.Empty &&
            _region == string.Empty &&
            _privateuse == string.Empty &&
            _extlangs == Arrays.Empty<string>() &&
            _variants == Arrays.Empty<string>() &&
            _extensions == Arrays.Empty<string>();

        //
        // static fields
        //
        public const char Separator = '-';
        public const string Private_Use = "x";
        public const string Undetermined = "und";
        public const string PrivateUse_Variant_Prefix = "lvariant";

        //
        // Language subtag fields
        //
        private string _language = string.Empty;      // language subtag
        private string _script = string.Empty;        // script subtag
        private string _region = string.Empty;        // region subtag
        private string _privateuse = string.Empty;    // privateuse


        private IList<string> _extlangs = Arrays.Empty<string>();   // extlang subtags
        private IList<string> _variants = Arrays.Empty<string>();   // variant subtags
        private IList<string> _extensions = Arrays.Empty<string>(); // extensions


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

        // Map contains grandfathered tags and its preferred mappings from
        // http://www.ietf.org/rfc/rfc5646.txt
        private static readonly Dictionary<AsciiCaseInsensitiveKey, string> Grandfathered = new Dictionary<AsciiCaseInsensitiveKey, string>()
        {
            // {"tag",         "preferred"},
            {new AsciiCaseInsensitiveKey("art-lojban"),  "jbo"},
            {new AsciiCaseInsensitiveKey("cel-gaulish"), "xtg-x-cel-gaulish"},   // fallback
            {new AsciiCaseInsensitiveKey("en-GB-oed"),   "en-GB-x-oed"},         // fallback
            {new AsciiCaseInsensitiveKey("i-ami"),       "ami"},
            {new AsciiCaseInsensitiveKey("i-bnn"),       "bnn"},
            {new AsciiCaseInsensitiveKey("i-default"),   "en-x-i-default"},      // fallback
            {new AsciiCaseInsensitiveKey("i-enochian"),  "und-x-i-enochian"},    // fallback
            {new AsciiCaseInsensitiveKey("i-hak"),       "hak"},
            {new AsciiCaseInsensitiveKey("i-klingon"),   "tlh"},
            {new AsciiCaseInsensitiveKey("i-lux"),       "lb"},
            {new AsciiCaseInsensitiveKey("i-mingo"),     "see-x-i-mingo"},       // fallback
            {new AsciiCaseInsensitiveKey("i-navajo"),    "nv"},
            {new AsciiCaseInsensitiveKey("i-pwn"),       "pwn"},
            {new AsciiCaseInsensitiveKey("i-tao"),       "tao"},
            {new AsciiCaseInsensitiveKey("i-tay"),       "tay"},
            {new AsciiCaseInsensitiveKey("i-tsu"),       "tsu"},
            {new AsciiCaseInsensitiveKey("no-bok"),      "nb"},
            {new AsciiCaseInsensitiveKey("no-nyn"),      "nn"},
            {new AsciiCaseInsensitiveKey("sgn-BE-FR"),   "sfb"},
            {new AsciiCaseInsensitiveKey("sgn-BE-NL"),   "vgt"},
            {new AsciiCaseInsensitiveKey("sgn-CH-DE"),   "sgg"},
            {new AsciiCaseInsensitiveKey("zh-guoyu"),    "cmn"},
            {new AsciiCaseInsensitiveKey("zh-hakka"),    "hak"},
            {new AsciiCaseInsensitiveKey("zh-min"),      "nan-x-zh-min"},        // fallback
            {new AsciiCaseInsensitiveKey("zh-min-nan"),  "nan"},
            {new AsciiCaseInsensitiveKey("zh-xiang"),    "hsn"},
        };

        public LanguageTag()
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
        public static bool TryParse(ReadOnlySpan<char> languageTag, out LanguageTag result, out ParseStatus status)
        {
            var sts = new ParseStatus();

            StringTokenEnumerator itr;
            bool isGrandfathered = false;

            // Check if the tag is grandfathered
            if (Grandfathered.TryGetValue(languageTag, out string? gf))
            {
                // use preferred mapping
                itr = new StringTokenEnumerator(gf.AsSpan(), Separator);
                isGrandfathered = true;
            }
            else
            {
                itr = new StringTokenEnumerator(languageTag, Separator);
            }

            // ICU4N: Move to the first element
            itr.MoveNext();
            LanguageTag tag = new LanguageTag();

            // langtag must start with either language or privateuse
            if (tag.ParseLanguage(ref itr, ref sts))
            {
                tag.ParseExtlangs(ref itr, ref sts);
                tag.ParseScript(ref itr, ref sts);
                tag.ParseRegion(ref itr, ref sts);
                tag.ParseVariants(ref itr, ref sts);
                tag.ParseExtensions(ref itr, ref sts);
            }
            tag.ParsePrivateuse(ref itr, ref sts);

            if (isGrandfathered)
            {
                // Grandfathered tag is replaced with a well-formed tag above.
                // However, the parsed length must be the original tag length.
                Debug.Assert(itr.Current.Text.IsEmpty);
                Debug.Assert(!sts.IsError);
                sts.ParseLength = languageTag.Length;
            }
            else if (!itr.IsDone && !sts.IsError)
            {
                ReadOnlySpan<char> s = itr.Current;
                sts.ErrorIndex = itr.Current.StartIndex;
                if (s.Length == 0)
                {
                    sts.ErrorMessage = "Empty subtag";
                }
                else
                {
                    sts.ErrorMessage = $"Invalid subtag: {s.ToString()}";
                }
            }

            status = sts;
            result = tag;
            return !sts.IsError;
        }

        //
        // Language subtag parsers
        //

        private bool ParseLanguage(ref StringTokenEnumerator itr, ref ParseStatus sts)
        {
            if (itr.IsDone || sts.IsError)
            {
                return false;
            }

            bool found = false;

            ReadOnlySpan<char> s = itr.Current;
            if (IsLanguage(s))
            {
                found = true;
                _language = s.ToString();
                sts.ParseLength = itr.Current.StartIndex + s.Length;
                itr.MoveNext();
            }

            return found;
        }

        private bool ParseExtlangs(ref StringTokenEnumerator itr, ref ParseStatus sts)
        {
            if (itr.IsDone || sts.IsError)
            {
                return false;
            }

            bool found = false;

            while (!itr.IsDone)
            {
                ReadOnlySpan<char> s = itr.Current;
                if (!IsExtlang(s))
                {
                    break;
                }
                found = true;
                if (_extlangs.Count == 0)
                {
                    _extlangs = new List<string>(3);
                }
                _extlangs.Add(s.ToString());
                sts.ParseLength = itr.Current.StartIndex + s.Length;
                itr.MoveNext();

                if (_extlangs.Count == 3)
                {
                    // Maximum 3 extlangs
                    break;
                }
            }

            return found;
        }

        private bool ParseScript(ref StringTokenEnumerator itr, ref ParseStatus sts)
        {
            if (itr.IsDone || sts.IsError)
            {
                return false;
            }

            bool found = false;

            ReadOnlySpan<char> s = itr.Current;
            if (IsScript(s))
            {
                found = true;
                _script = s.ToString();
                sts.ParseLength = itr.Current.StartIndex + s.Length;
                itr.MoveNext();
            }

            return found;
        }

        private bool ParseRegion(ref StringTokenEnumerator itr, ref ParseStatus sts)
        {
            if (itr.IsDone || sts.IsError)
            {
                return false;
            }

            bool found = false;

            ReadOnlySpan<char> s = itr.Current;
            if (IsRegion(s))
            {
                found = true;
                _region = s.ToString();
                sts.ParseLength = itr.Current.StartIndex + s.Length;
                itr.MoveNext();
            }

            return found;
        }

        private bool ParseVariants(ref StringTokenEnumerator itr, ref ParseStatus sts)
        {
            if (itr.IsDone || sts.IsError)
            {
                return false;
            }

            bool found = false;

            while (!itr.IsDone)
            {
                ReadOnlySpan<char> s = itr.Current;
                if (!IsVariant(s))
                {
                    break;
                }
                found = true;
                if (_variants.Count == 0)
                {
                    _variants = new List<string>(3);
                }
                _variants.Add(s.ToString());
                sts.ParseLength = itr.Current.StartIndex + s.Length;
                itr.MoveNext();
            }

            return found;
        }

        private bool ParseExtensions(ref StringTokenEnumerator itr, ref ParseStatus sts)
        {
            if (itr.IsDone || sts.IsError)
            {
                return false;
            }

            bool found = false;
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                while (!itr.IsDone)
                {
                    ReadOnlySpan<char> s = itr.Current;
                    if (IsExtensionSingleton(s))
                    {
                        int start = itr.Current.StartIndex;
                        ReadOnlySpan<char> singleton = s;
                        sb.Length = 0;
                        sb.Append(singleton);

                        itr.MoveNext();
                        while (!itr.IsDone)
                        {
                            s = itr.Current;
                            if (IsExtensionSubtag(s))
                            {
                                sb.Append(Separator);
                                sb.Append(s);
                                sts.ParseLength = itr.Current.StartIndex + s.Length;
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
                            sts.ErrorMessage = $"Incomplete extension '{singleton.ToString()}'";
                            break;
                        }

                        if (_extensions.Count == 0)
                        {
                            _extensions = new List<string>(4);
                        }
                        _extensions.Add(sb.ToString());
                        found = true;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            finally
            {
                sb.Dispose();
            }
            return found;
        }

        private bool ParsePrivateuse(ref StringTokenEnumerator itr, ref ParseStatus sts)
        {
            if (itr.IsDone || sts.IsError)
            {
                return false;
            }

            bool found = false;
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                ReadOnlySpan<char> s = itr.Current;
                if (IsPrivateusePrefix(s))
                {
                    int start = itr.Current.StartIndex;
                    sb.Append(s);

                    itr.MoveNext();
                    while (!itr.IsDone)
                    {
                        s = itr.Current;
                        if (!IsPrivateuseSubtag(s))
                        {
                            break;
                        }
                        sb.Append(Separator);
                        sb.Append(s);
                        sts.ParseLength = itr.Current.StartIndex + s.Length;

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
            }
            finally
            {
                sb.Dispose();
            }

            return found;
        }

        public static void ParseLocale(BaseLocale baseLocale, LocaleExtensions localeExtensions, out LanguageTag result)
        {
            LanguageTag tag = new LanguageTag();

            string language = baseLocale.Language;
            string script = baseLocale.Script;
            string region = baseLocale.Region;
            ReadOnlySpan<char> variant = baseLocale.Variant.AsSpan();

            bool hasSubtag = false;

            string? privuseVar = null;   // store ill-formed variant subtags

            if (language.Length > 0 && IsLanguage(language))
            {
                // Convert a deprecated language code used by Java to
                // a new code
                if (language.Equals("iw", StringComparison.Ordinal))
                {
                    language = "he";
                }
                else if (language.Equals("ji", StringComparison.Ordinal))
                {
                    language = "yi";
                }
                else if (language.Equals("in", StringComparison.Ordinal))
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

#if JDKIMPL
            // ICU4N TODO: Remove ?
            // Special handling for no_NO_NY - use nn_NO for language tag
            if (tag._language.Equals("no", StringComparison.Ordinal) &&
                tag._region.Equals("NO", StringComparison.Ordinal) &&
                variant.Equals("NY", StringComparison.Ordinal)) // ICU4N TODO: Fix this handling for .NET (no-NO is not reliable across platforms)
            {
                tag._language = "nn";
                variant = ReadOnlySpan<char>.Empty;
            }
#endif

            if (variant.Length > 0)
            {
                Span<char> stackBuffer = stackalloc char[CharStackBufferSize];
                List<string>? variants = null;
                StringTokenEnumerator varitr = new StringTokenEnumerator(variant, BaseLocale.Separator);
                varitr.MoveNext();
                while (!varitr.IsDone)
                {
                    scoped ReadOnlySpan<char> var = varitr.Current;
                    if (!IsVariant(var))
                    {
                        break;
                    }
                    if (variants == null)
                    {
                        variants = new List<string>();
                    }
#if JDKIMPL
                    variants.Add(var.ToString());  // Do not canonicalize!
#else
                    if (var.Length <= stackBuffer.Length)
                    {
                        variants.Add(CanonicalizeVariant(var, stackBuffer).ToString());
                    }
                    else // rare
                    {
                        char[] heapBuffer = ArrayPool<char>.Shared.Rent(var.Length);
                        try
                        {
                            variants.Add(CanonicalizeVariant(var, heapBuffer).ToString());
                        }
                        finally
                        {
                            ArrayPool<char>.Shared.Return(heapBuffer);
                        }
                    }
#endif
                    varitr.MoveNext();
                }
                if (variants != null)
                {
                    tag._variants = variants;
                    hasSubtag = true;
                }
                if (!varitr.IsDone)
                {
                    // ill-formed variant subtags
                    ValueStringBuilder buf = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
                    try
                    {
                        while (!varitr.IsDone)
                        {
                            scoped ReadOnlySpan<char> prvv = varitr.Current;
                            if (!IsPrivateuseSubtag(prvv))
                            {
                                // cannot use private use subtag - truncated
                                break;
                            }
                            if (buf.Length > 0)
                            {
                                buf.Append(Separator);
                            }
#if !JDKIMPL
                            if (prvv.Length <= stackBuffer.Length)
                            {
                                prvv = AsciiUtil.ToLower(prvv, stackBuffer);
                            }
                            else // rare
                            {
                                char[] heapBuffer = ArrayPool<char>.Shared.Rent(prvv.Length);
                                try
                                {
                                    prvv = AsciiUtil.ToLower(prvv, heapBuffer);
                                }
                                finally
                                {
                                    ArrayPool<char>.Shared.Return(heapBuffer);
                                }
                            }
#endif
                            buf.Append(prvv);
                            varitr.MoveNext();
                        }
                        if (buf.Length > 0)
                        {
                            privuseVar = buf.ToString(); // ICU4N: This should be a rare case
                        }
                    }
                    finally
                    {
                        buf.Dispose();
                    }
                }
            }

            List<string>? extensions = null;
            string? privateuse = null;
            Span<char> concatBuffer = stackalloc char[2];

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
                    concatBuffer[0] = locextKey;
                    concatBuffer[1] = Separator;
                    extensions.Add(StringHelper.Concat(concatBuffer, ext.Value.AsSpan()));
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
                int length = privateuse is null
                    ? PrivateUse_Variant_Prefix.Length + 1 + privuseVar.Length
                    : privateuse.Length + 1 + PrivateUse_Variant_Prefix.Length + 1 + privuseVar.Length;

                ValueStringBuilder sb = length <= CharStackBufferSize
                    ? new ValueStringBuilder(stackalloc char[length])
                    : new ValueStringBuilder(length);
                try
                {
                    if (privateuse is null)
                    {
                        sb.Append(PrivateUse_Variant_Prefix);
                        sb.Append(Separator);
                        sb.Append(privuseVar);
                    }
                    else
                    {
                        sb.Append(privateuse);
                        sb.Append(Separator);
                        sb.Append(PrivateUse_Variant_Prefix);
                        sb.Append(Separator);
                        var privuse = sb.AppendSpan(privuseVar.Length);
                        privuseVar.AsSpan().CopyTo(privuse);
                        privuse.Replace(BaseLocale.Separator, Separator);
                    }
                    privateuse = sb.ToString();
                }
                finally
                {
                    sb.Dispose();
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
                tag._language = Undetermined;
            }

            result = tag;
        }

        //
        // Getter methods for language subtag fields
        //

        public string Language => _language;

        public IList<string> Extlangs
#if FEATURE_ILIST_ASREADONLY
            => System.Collections.Generic.CollectionExtensions.AsReadOnly(_extlangs);
#else
            => _extlangs.AsReadOnly();
#endif

        public string Script => _script;

        public string Region => _region;

        public IList<string> Variants
#if FEATURE_ILIST_ASREADONLY
            => System.Collections.Generic.CollectionExtensions.AsReadOnly(_variants);
#else
            => _variants.AsReadOnly();
#endif

        public IList<string> Extensions
#if FEATURE_ILIST_ASREADONLY
            => System.Collections.Generic.CollectionExtensions.AsReadOnly(_extensions);
#else
            => _extensions.AsReadOnly();
#endif

        public string PrivateUse => _privateuse;

        //
        // Language subtag syntax checking methods
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLanguage(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));
            // language      = 2*3ALPHA            ; shortest ISO 639 code
            //                 ["-" extlang]       ; sometimes followed by
            //                                     ;   extended language subtags
            //               / 4ALPHA              ; or reserved for future use
            //               / 5*8ALPHA            ; or registered language subtag
            return (s.Length >= 2) && (s.Length <= 8) && AsciiUtil.IsAlpha(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLanguage(ReadOnlySpan<char> s)
        {
            // language      = 2*3ALPHA            ; shortest ISO 639 code
            //                 ["-" extlang]       ; sometimes followed by
            //                                     ;   extended language subtags
            //               / 4ALPHA              ; or reserved for future use
            //               / 5*8ALPHA            ; or registered language subtag
            return (s.Length >= 2) && (s.Length <= 8) && AsciiUtil.IsAlpha(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsExtlang(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));
            // extlang       = 3ALPHA              ; selected ISO 639 codes
            //                 *2("-" 3ALPHA)      ; permanently reserved
            return (s.Length == 3) && AsciiUtil.IsAlpha(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsExtlang(ReadOnlySpan<char> s)
        {
            // extlang       = 3ALPHA              ; selected ISO 639 codes
            //                 *2("-" 3ALPHA)      ; permanently reserved
            return (s.Length == 3) && AsciiUtil.IsAlpha(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsScript(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));
            // script        = 4ALPHA              ; ISO 15924 code
            return (s.Length == 4) && AsciiUtil.IsAlpha(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsScript(ReadOnlySpan<char> s)
        {
            // script        = 4ALPHA              ; ISO 15924 code
            return (s.Length == 4) && AsciiUtil.IsAlpha(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsRegion(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));
            // region        = 2ALPHA              ; ISO 3166-1 code
            //               / 3DIGIT              ; UN M.49 code
            return ((s.Length == 2) && AsciiUtil.IsAlpha(s))
                    || ((s.Length == 3) && AsciiUtil.IsNumeric(s));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsRegion(ReadOnlySpan<char> s)
        {
            // region        = 2ALPHA              ; ISO 3166-1 code
            //               / 3DIGIT              ; UN M.49 code
            return ((s.Length == 2) && AsciiUtil.IsAlpha(s))
                    || ((s.Length == 3) && AsciiUtil.IsNumeric(s));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsVariant(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));
            return IsVariant(s.AsSpan());
        }

        public static bool IsVariant(ReadOnlySpan<char> s)
        {
            // variant       = 5*8alphanum         ; registered variants
            //               / (DIGIT 3alphanum)
            int len = s.Length;
            if (len >= 5 && len <= 8)
            {
                return AsciiUtil.IsAlphaNumeric(s);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsExtensionSingleton(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            // singleton     = DIGIT               ; 0 - 9
            //               / %x41-57             ; A - W
            //               / %x59-5A             ; Y - Z
            //               / %x61-77             ; a - w
            //               / %x79-7A             ; y - z

            return (s.Length == 1)
                    && AsciiUtil.IsAlpha(s)
                    && !AsciiUtil.CaseIgnoreMatch(Private_Use, s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsExtensionSingleton(ReadOnlySpan<char> s)
        {
            // singleton     = DIGIT               ; 0 - 9
            //               / %x41-57             ; A - W
            //               / %x59-5A             ; Y - Z
            //               / %x61-77             ; a - w
            //               / %x79-7A             ; y - z

            return (s.Length == 1)
                    && AsciiUtil.IsAlpha(s)
                    && !AsciiUtil.CaseIgnoreMatch(Private_Use.AsSpan(), s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsExtensionSingletonChar(char c)
        {
            return IsExtensionSingleton(stackalloc char[1] { c });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsExtensionSubtag(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            // extension     = singleton 1*("-" (2*8alphanum))
            return (s.Length >= 2) && (s.Length <= 8) && AsciiUtil.IsAlphaNumeric(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsExtensionSubtag(ReadOnlySpan<char> s)
        {
            // extension     = singleton 1*("-" (2*8alphanum))
            return (s.Length >= 2) && (s.Length <= 8) && AsciiUtil.IsAlphaNumeric(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPrivateusePrefix(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            // privateuse    = "x" 1*("-" (1*8alphanum))
            return (s.Length == 1)
                    && AsciiUtil.CaseIgnoreMatch(Private_Use, s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPrivateusePrefix(ReadOnlySpan<char> s)
        {
            // privateuse    = "x" 1*("-" (1*8alphanum))
            return (s.Length == 1)
                    && AsciiUtil.CaseIgnoreMatch(Private_Use.AsSpan(), s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPrivateusePrefixChar(char c)
        {
            Span<char> buffer = stackalloc char[1] { c };
            return (AsciiUtil.CaseIgnoreMatch(Private_Use.AsSpan(), buffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPrivateuseSubtag(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            // privateuse    = "x" 1*("-" (1*8alphanum))
            return (s.Length >= 1) && (s.Length <= 8) && AsciiUtil.IsAlphaNumeric(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPrivateuseSubtag(ReadOnlySpan<char> s)
        {
            // privateuse    = "x" 1*("-" (1*8alphanum))
            return (s.Length >= 1) && (s.Length <= 8) && AsciiUtil.IsAlphaNumeric(s);
        }

        //
        // Language subtag canonicalization methods
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> CanonicalizeLanguage(ReadOnlySpan<char> s, Span<char> buffer)
        {
            return AsciiUtil.ToLower(s, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> CanonicalizeExtlang(ReadOnlySpan<char> s, Span<char> buffer)
        {
            return AsciiUtil.ToLower(s, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> CanonicalizeScript(ReadOnlySpan<char> s, Span<char> buffer)
        {
            return AsciiUtil.ToTitle(s, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> CanonicalizeRegion(ReadOnlySpan<char> s, Span<char> buffer)
        {
            return AsciiUtil.ToUpper(s, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> CanonicalizeVariant(ReadOnlySpan<char> s, Span<char> buffer)
        {
            return AsciiUtil.ToLower(s, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> CanonicalizeExtension(ReadOnlySpan<char> s, Span<char> buffer)
        {
            return AsciiUtil.ToLower(s, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> CanonicalizeExtensionSingleton(ReadOnlySpan<char> s, Span<char> buffer)
        {
            return AsciiUtil.ToLower(s, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> CanonicalizeExtensionSubtag(ReadOnlySpan<char> s, Span<char> buffer)
        {
            return AsciiUtil.ToLower(s, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> CanonicalizePrivateuse(ReadOnlySpan<char> s, Span<char> buffer)
        {
            return AsciiUtil.ToLower(s, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> CanonicalizePrivateuseSubtag(ReadOnlySpan<char> s, Span<char> buffer)
        {
            return AsciiUtil.ToLower(s, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CanonicalizeLanguage(string s)
        {
            return AsciiUtil.ToLower(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CanonicalizeExtlang(string s)
        {
            return AsciiUtil.ToLower(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CanonicalizeScript(string s)
        {
            return AsciiUtil.ToTitle(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CanonicalizeRegion(string s)
        {
            return AsciiUtil.ToUpper(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CanonicalizeVariant(string s)
        {
            return AsciiUtil.ToLower(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CanonicalizeExtension(string s)
        {
            return AsciiUtil.ToLower(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CanonicalizeExtensionSingleton(string s)
        {
            return AsciiUtil.ToLower(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CanonicalizeExtensionSubtag(string s)
        {
            return AsciiUtil.ToLower(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CanonicalizePrivateuse(string s)
        {
            return AsciiUtil.ToLower(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CanonicalizePrivateuseSubtag(string s)
        {
            return AsciiUtil.ToLower(s);
        }


        public override string ToString()
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                if (_language.Length > 0)
                {
                    sb.Append(_language);

                    foreach (string extlang in _extlangs)
                    {
                        sb.Append(Separator);
                        sb.Append(extlang);
                    }

                    if (_script.Length > 0)
                    {
                        sb.Append(Separator);
                        sb.Append(_script);
                    }

                    if (_region.Length > 0)
                    {
                        sb.Append(Separator);
                        sb.Append(_region);
                    }

                    foreach (string variant in _variants)
                    {
                        sb.Append(Separator);
                        sb.Append(variant);
                    }

                    foreach (string extension in _extensions)
                    {
                        sb.Append(Separator);
                        sb.Append(extension);
                    }
                }
                if (_privateuse.Length > 0)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(Separator);
                    }
                    sb.Append(_privateuse);
                }

                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }
    }
}
