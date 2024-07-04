using ICU4N.Support.Collections;
using ICU4N.Text;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using JCG = J2N.Collections.Generic;
#nullable enable

namespace ICU4N.Impl.Locale
{
    public sealed class InternalLocaleBuilder
    {
        private const int CharStackBufferSize = 32;

        private string _language = "";
        private string _script = "";
        private string _region = "";
        private string _variant = "";

        private static readonly char PRIVUSE_KEY = LanguageTag.Private_Use[0];

        private IDictionary<char, string?>? _extensions;
        private ISet<string>? _uattributes;
        private Dictionary<string, string>? _ukeywords;

        public InternalLocaleBuilder()
        {
        }

        public InternalLocaleBuilder SetLanguage(string language)
        {
            if (language == null || language.Length == 0)
            {
                _language = "";
            }
            else
            {
                if (!LanguageTag.IsLanguage(language))
                {
                    throw new FormatException("Ill-formed language: " + language/*, 0*/);
                }
                _language = language;
            }
            return this;
        }

        public InternalLocaleBuilder SetScript(string script)
        {
            if (script == null || script.Length == 0)
            {
                _script = "";
            }
            else
            {
                if (!LanguageTag.IsScript(script))
                {
                    throw new FormatException("Ill-formed script: " + script/*, 0*/);
                }
                _script = script;
            }
            return this;
        }

        public InternalLocaleBuilder SetRegion(string region)
        {
            if (region == null || region.Length == 0)
            {
                _region = "";
            }
            else
            {
                if (!LanguageTag.IsRegion(region))
                {
                    throw new FormatException("Ill-formed region: " + region/*, 0*/);
                }
                _region = region;
            }
            return this;
        }

        public InternalLocaleBuilder SetVariant(string variant)
        {
            if (variant == null || variant.Length == 0)
            {
                _variant = "";
            }
            else
            {
                // normalize separators to "_"
                string var = variant.Replace(LanguageTag.Separator, BaseLocale.Separator);
                int errIdx = CheckVariants(var, BaseLocale.Separator);
                if (errIdx != -1)
                {
                    throw new FormatException("Ill-formed variant: " + variant /*, errIdx*/);
                }
                _variant = var;
            }
            return this;
        }

        public InternalLocaleBuilder AddUnicodeLocaleAttribute(string attribute)
        {
            if (attribute == null || !UnicodeLocaleExtension.IsAttribute(attribute))
            {
                throw new FormatException("Ill-formed Unicode locale attribute: " + attribute);
            }
            // Use case insensitive string to prevent duplication
            if (_uattributes == null)
            {
                _uattributes = new JCG.HashSet<string>(4, AsciiStringComparer.OrdinalIgnoreCase);
            }
            _uattributes.Add(attribute);
            return this;
        }

        public InternalLocaleBuilder RemoveUnicodeLocaleAttribute(string attribute)
        {
            if (attribute == null || !UnicodeLocaleExtension.IsAttribute(attribute))
            {
                throw new FormatException("Ill-formed Unicode locale attribute: " + attribute);
            }
            if (_uattributes != null)
            {
                _uattributes.Remove(attribute);
            }
            return this;
        }

        public InternalLocaleBuilder SetUnicodeLocaleKeyword(string key, string type)
        {
            if (!UnicodeLocaleExtension.IsKey(key))
            {
                throw new FormatException("Ill-formed Unicode locale keyword key: " + key);
            }

            if (type == null)
            {
                if (_ukeywords != null)
                {
                    // null type is used for remove the key
                    _ukeywords.Remove(key);
                }
            }
            else
            {
                if (type.Length != 0)
                {
                    // normalize separator to "-"
                    string tp = type.Replace(BaseLocale.Separator, LanguageTag.Separator);
                    // validate
                    StringTokenEnumerator itr = new StringTokenEnumerator(tp.AsSpan(), LanguageTag.Separator);
                    while (itr.MoveNext())
                    {
                        ReadOnlySpan<char> s = itr.Current;
                        if (!UnicodeLocaleExtension.IsTypeSubtag(s))
                        {
                            throw new FormatException("Ill-formed Unicode locale keyword type: " + type /*, itr.CurrentStart*/);
                        }
                    }
                }
                if (_ukeywords == null)
                {
                    _ukeywords = new Dictionary<string, string>(4, AsciiStringComparer.OrdinalIgnoreCase);
                }
                _ukeywords[key] = type;
            }
            return this;
        }

        public InternalLocaleBuilder SetExtension(char singleton, string? value)
        {
            // validate key
            bool isBcpPrivateuse = LanguageTag.IsPrivateusePrefixChar(singleton);
            if (!isBcpPrivateuse && !LanguageTag.IsExtensionSingletonChar(singleton))
            {
                throw new FormatException("Ill-formed extension key: " + singleton);
            }

            bool remove = (value == null || value.Length == 0);
            char key = singleton;

            if (remove)
            {
                if (UnicodeLocaleExtension.IsSingletonChar(key))
                {
                    // clear entire Unicode locale extension
                    if (_uattributes != null)
                    {
                        _uattributes.Clear();
                    }
                    if (_ukeywords != null)
                    {
                        _ukeywords.Clear();
                    }
                }
                else
                {
                    if (_extensions != null && _extensions.ContainsKey(key))
                    {
                        _extensions.Remove(key);
                    }
                }
            }
            else
            {
                // validate value
                int valueLength = value!.Length;
                bool usePool = valueLength > CharStackBufferSize;
                char[]? pooledArray = usePool ? ArrayPool<char>.Shared.Rent(valueLength) : null;
                try
                {
                    Span<char> val = usePool ? pooledArray.AsSpan(valueLength) : stackalloc char[valueLength];
                    value.AsSpan().CopyTo(val);
                    val.Replace(BaseLocale.Separator, LanguageTag.Separator);

                    StringTokenEnumerator itr = new StringTokenEnumerator(val, LanguageTag.Separator);
                    while (itr.MoveNext())
                    {
                        ReadOnlySpan<char> s = itr.Current;
                        bool validSubtag;
                        if (isBcpPrivateuse)
                        {
                            validSubtag = LanguageTag.IsPrivateuseSubtag(s);
                        }
                        else
                        {
                            validSubtag = LanguageTag.IsExtensionSubtag(s);
                        }
                        if (!validSubtag)
                        {
                            throw new FormatException($"Ill-formed extension value: {s.ToString()}" /*, itr.CurrentStart*/);
                        }
                    }

                    if (UnicodeLocaleExtension.IsSingletonChar(key))
                    {
                        SetUnicodeLocaleExtension(val);
                    }
                    else
                    {
                        if (_extensions == null)
                        {
                            _extensions = new Dictionary<char, string?>(4, AsciiCharComparer.OrdinalIgnoreCase);
                        }
                        _extensions[key] = val.ToString();
                    }
                }
                finally
                {
                    if (pooledArray is not null)
                        ArrayPool<char>.Shared.Return(pooledArray);
                }
            }
            return this;
        }

        /// <summary>
        /// Set extension/private subtags in a single string representation
        /// </summary>
        public InternalLocaleBuilder SetExtensions(string? subtags)
        {
            if (subtags == null || subtags.Length == 0)
            {
                ClearExtensions();
                return this;
            }
            subtags = subtags.Replace(BaseLocale.Separator, LanguageTag.Separator);
            StringTokenEnumerator itr = new StringTokenEnumerator(subtags.AsSpan(), LanguageTag.Separator);

            List<string>? extensions = null;
            string? privateuse = null;

            int parsed = 0;
            int start;

            // Move to first element
            itr.MoveNext();

            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                // Make a list of extension subtags
                while (!itr.IsDone)
                {
                    ReadOnlySpan<char> s = itr.Current;
                    if (LanguageTag.IsExtensionSingleton(s))
                    {
                        start = itr.Current.StartIndex;
                        ReadOnlySpan<char> singleton = s;
                        sb.Length = 0;
                        sb.Append(singleton);

                        itr.MoveNext();
                        while (!itr.IsDone)
                        {
                            s = itr.Current;
                            if (LanguageTag.IsExtensionSubtag(s))
                            {
                                sb.Append(LanguageTag.Separator);
                                sb.Append(s);
                                parsed = itr.Current.StartIndex + s.Length;
                            }
                            else
                            {
                                break;
                            }
                            itr.MoveNext();
                        }

                        if (parsed < start)
                        {
                            throw new FormatException($"Incomplete extension '{singleton.ToString()}'"/*, start*/);
                        }

                        if (extensions == null)
                        {
                            extensions = new List<string>(4);
                        }
                        extensions.Add(sb.AsSpan().ToString());
                    }
                    else
                    {
                        break;
                    }
                }
                if (!itr.IsDone)
                {
                    ReadOnlySpan<char> s = itr.Current;
                    if (LanguageTag.IsPrivateusePrefix(s))
                    {
                        start = itr.Current.StartIndex;
                        sb.Length = 0;
                        sb.Append(s);

                        itr.MoveNext();
                        while (!itr.IsDone)
                        {
                            s = itr.Current;
                            if (!LanguageTag.IsPrivateuseSubtag(s))
                            {
                                break;
                            }
                            sb.Append(LanguageTag.Separator);
                            sb.Append(s);
                            parsed = itr.Current.StartIndex + s.Length;

                            itr.MoveNext();
                        }
                        if (parsed <= start)
                        {
                            throw new FormatException("Incomplete privateuse:" + subtags.Substring(start) /*, start*/);
                        }
                        else
                        {
                            privateuse = sb.AsSpan().ToString();
                        }
                    }
                }

                if (!itr.IsDone)
                {
                    throw new FormatException("Ill-formed extension subtags:" + subtags.Substring(itr.Current.StartIndex)/*, itr.CurrentStart*/);
                }

                return SetExtensions(extensions, privateuse);
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Set a list of BCP47 extensions and private use subtags.
        /// BCP47 extensions are already validated and well-formed, but may contain duplicates.
        /// </summary>
        private InternalLocaleBuilder SetExtensions(IList<string>? bcpExtensions, string? privateuse)
        {
            ClearExtensions();

            if (bcpExtensions != null && bcpExtensions.Count > 0)
            {
                var processedExtensions = new JCG.HashSet<char>(bcpExtensions.Count, AsciiCharComparer.OrdinalIgnoreCase);
                foreach (string bcpExt in bcpExtensions)
                {
                    //CaseInsensitiveChar key = new CaseInsensitiveChar(bcpExt[0]);
                    char key = bcpExt[0];
                    // ignore duplicates
                    if (!processedExtensions.Contains(key))
                    {
                        // each extension string contains singleton, e.g. "a-abc-def"
                        if (UnicodeLocaleExtension.IsSingletonChar(key))
                        {
                            SetUnicodeLocaleExtension(bcpExt.AsSpan(2));
                        }
                        else
                        {
                            if (_extensions == null)
                            {
                                _extensions = new Dictionary<char, string?>(4, AsciiCharComparer.OrdinalIgnoreCase);
                            }
                            _extensions[key] = bcpExt.Substring(2);
                        }
                    }
                }
            }
            if (privateuse != null && privateuse.Length > 0)
            {
                // privateuse string contains prefix, e.g. "x-abc-def"
                if (_extensions == null)
                {
                    _extensions = new Dictionary<char, string?>(1, AsciiCharComparer.OrdinalIgnoreCase);
                }
                _extensions[privateuse[0]] = privateuse.Substring(2);
            }

            return this;
        }

        /// <summary>
        /// Reset Builder's internal state with the given language tag
        /// </summary>
        public InternalLocaleBuilder SetLanguageTag(LanguageTag langtag)
        {
            Clear();
            if (langtag.Extlangs.Count > 0)
            {
                _language = langtag.Extlangs[0];
            }
            else
            {
                string language = langtag.Language;
                if (!language.Equals(LanguageTag.Undetermined, StringComparison.Ordinal))
                {
                    _language = language;
                }
            }
            _script = langtag.Script;
            _region = langtag.Region;

            IList<string> bcpVariants = langtag.Variants;
            if (bcpVariants.Count > 0)
            {
                ValueStringBuilder var = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
                try
                {
                    var.Append(bcpVariants[0]);
                    for (int i = 1; i < bcpVariants.Count; i++)
                    {
                        var.Append(BaseLocale.Separator);
                        var.Append(bcpVariants[i]);
                    }
                    _variant = var.ToString();
                }
                finally
                {
                    var.Dispose();
                }
            }

            SetExtensions(langtag.Extensions, langtag.PrivateUse);

            return this;
        }

        public InternalLocaleBuilder SetLocale(BaseLocale @base, LocaleExtensions extensions)
        {
            string language = @base.Language;
            string script = @base.Script;
            string region = @base.Region;
            string variant = @base.Variant;

#if JDKIMPL
            // ICU4N TODO: Remove ?
            // Special backward compatibility support

            // Exception 1 - ja_JP_JP
            if (language.Equals("ja", StringComparison.Ordinal) && region.Equals("JP", StringComparison.Ordinal) && variant.Equals("JP", StringComparison.Ordinal))
            {
                // When locale ja_JP_JP is created, ca-japanese is always there.
                // The builder ignores the variant "JP"
                Debug.Assert("japanese".Equals(extensions?.GetUnicodeLocaleType("ca") ?? string.Empty));
                variant = "";
            }
            // Exception 2 - th_TH_TH
            else if (language.Equals("th", StringComparison.Ordinal) && region.Equals("TH", StringComparison.Ordinal) && variant.Equals("TH", StringComparison.Ordinal))
            {
                // When locale th_TH_TH is created, nu-thai is always there.
                // The builder ignores the variant "TH"
                Debug.Assert("thai".Equals(extensions?.GetUnicodeLocaleType("nu") ?? string.Empty));
                variant = "";
            }
            // Exception 3 - no_NO_NY
            else if (language.Equals("no", StringComparison.Ordinal) && region.Equals("NO", StringComparison.Ordinal) && variant.Equals("NY", StringComparison.Ordinal)) // ICU4N TODO: Fix this handling for .NET (no-NO is not reliable across platforms)
            {
                // no_NO_NY is a valid locale and used by Java 6 or older versions.
                // The build ignores the variant "NY" and change the language to "nn".
                language = "nn";
                variant = "";
            }
#endif

            // Validate base locale fields before updating internal state.
            // LocaleExtensions always store validated/canonicalized values,
            // so no checks are necessary.
            if (language.Length > 0 && !LanguageTag.IsLanguage(language))
            {
                throw new FormatException("Ill-formed language: " + language);
            }

            if (script.Length > 0 && !LanguageTag.IsScript(script))
            {
                throw new FormatException("Ill-formed script: " + script);
            }

            if (region.Length > 0 && !LanguageTag.IsRegion(region))
            {
                throw new FormatException("Ill-formed region: " + region); // ICU4N TODO: API - Port LocaleSyntaxException (instead of FormatException)
            }

            if (variant.Length > 0)
            {
                int errIdx = CheckVariants(variant, BaseLocale.Separator);
                if (errIdx != -1)
                {
                    throw new FormatException("Ill-formed variant: " + variant/*, errIdx*/);
                }
            }

            // The input locale is validated at this point.
            // Now, updating builder's internal fields.
            _language = language;
            _script = script;
            _region = region;
            _variant = variant;
            ClearExtensions();

            var extKeys = (extensions == null) ? null : extensions.Keys;
            if (extKeys != null)
            {
                // map extensions back to builder's internal format
                foreach (char key in extKeys)
                {
                    Extension e = extensions!.GetExtension(key);
                    if (e is UnicodeLocaleExtension ue)
                    {
                        foreach (string uatr in ue.UnicodeLocaleAttributes)
                        {
                            if (_uattributes == null)
                            {
                                _uattributes = new JCG.HashSet<string>(4, AsciiStringComparer.OrdinalIgnoreCase);
                            }
                            _uattributes.Add(uatr);
                        }
                        foreach (string ukey in ue.UnicodeLocaleKeys)
                        {
                            if (_ukeywords == null)
                            {
                                _ukeywords = new Dictionary<string, string>(4, AsciiStringComparer.OrdinalIgnoreCase);
                            }
                            _ukeywords[ukey] = ue.GetUnicodeLocaleType(ukey);
                        }
                    }
                    else
                    {
                        if (_extensions == null)
                        {
                            _extensions = new Dictionary<char, string?>(4, AsciiCharComparer.OrdinalIgnoreCase);
                        }
                        _extensions[key] = e.Value;
                    }
                }
            }
            return this;
        }

        public InternalLocaleBuilder Clear()
        {
            _language = "";
            _script = "";
            _region = "";
            _variant = "";
            ClearExtensions();
            return this;
        }

        public InternalLocaleBuilder ClearExtensions()
        {
            if (_extensions != null)
            {
                _extensions.Clear();
            }
            if (_uattributes != null)
            {
                _uattributes.Clear();
            }
            if (_ukeywords != null)
            {
                _ukeywords.Clear();
            }
            return this;
        }

        public BaseLocale GetBaseLocale()
        {
            string language = _language;
            string script = _script;
            string region = _region;
            string variant = _variant;

            // Special private use subtag sequence identified by "lvariant" will be
            // interpreted as Java variant.
            if (_extensions != null)
            {
                if (_extensions.TryGetValue(PRIVUSE_KEY, out string? privuse) && privuse != null)
                {
                    StringTokenEnumerator itr = new StringTokenEnumerator(privuse.AsSpan(), LanguageTag.Separator);
                    bool sawPrefix = false;
                    int privVarStart = -1;
                    while (itr.MoveNext())
                    {
                        if (sawPrefix)
                        {
                            privVarStart = itr.Current.StartIndex;
                            break;
                        }
                        if (AsciiUtil.CaseIgnoreMatch(itr.Current, LanguageTag.PrivateUse_Variant_Prefix.AsSpan()))
                        {
                            sawPrefix = true;
                        }
                    }
                    if (privVarStart != -1)
                    {
                        ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
                        try
                        {
                            sb.Append(variant);
                            if (sb.Length != 0)
                            {
                                sb.Append(BaseLocale.Separator);
                            }
                            var privateuse = sb.AppendSpan(privuse.Length - privVarStart);
                            privuse.AsSpan(privVarStart).CopyTo(privateuse);
                            privateuse.Replace(LanguageTag.Separator, BaseLocale.Separator);
                            variant = sb.ToString();
                        }
                        finally
                        {
                            sb.Dispose();
                        }
                    }
                }
            }

            return BaseLocale.GetInstance(language, script, region, variant);
        }

        public LocaleExtensions GetLocaleExtensions()
        {
            if ((_extensions == null || _extensions.Count == 0)
                    && (_uattributes == null || _uattributes.Count == 0)
                    && (_ukeywords == null || _ukeywords.Count == 0))
            {
                return LocaleExtensions.EmptyExtensions;
            }

            return new LocaleExtensions(_extensions, _uattributes, _ukeywords);
        }

        /// <summary>
        /// Remove special private use subtag sequence identified by "lvariant"
        /// and return the rest. Only used by LocaleExtensions.
        /// </summary>
        internal static string? RemovePrivateuseVariant(string? privuseVal)
        {
            StringTokenEnumerator itr = new StringTokenEnumerator(privuseVal.AsSpan(), LanguageTag.Separator);

            // Note: privateuse value "abc-lvariant" is unchanged
            // because no subtags after "lvariant".

            int prefixStart = -1;
            bool sawPrivuseVar = false;

            while (itr.MoveNext())
            {
                if (prefixStart != -1)
                {
                    // Note: privateuse value "abc-lvariant" is unchanged
                    // because no subtags after "lvariant".
                    sawPrivuseVar = true;
                    break;
                }
                if (AsciiUtil.CaseIgnoreMatch(itr.Current, LanguageTag.PrivateUse_Variant_Prefix.AsSpan()))
                {
                    prefixStart = itr.Current.StartIndex;
                }
            }
            if (!sawPrivuseVar)
            {
                return privuseVal;
            }

            Debug.Assert(prefixStart == 0 || prefixStart > 1);
            return (prefixStart == 0) ? null : privuseVal!.Substring(0, prefixStart - 1); // ICU4N: Checked 2nd parameter
        }

        /// <summary>
        /// Check if the given variant subtags separated by the given
        /// separator are valid.
        /// </summary>
        private int CheckVariants(string variants, char sep)
        {
            StringTokenEnumerator itr = new StringTokenEnumerator(variants.AsSpan(), sep);
            while (itr.MoveNext())
            {
                ReadOnlySpan<char> s = itr.Current;
                if (!LanguageTag.IsVariant(s))
                {
                    return itr.Current.StartIndex;
                }
            }
            return -1;
        }

        /// <summary>
        /// Private methods parsing Unicode Locale Extension subtags.
        /// Duplicated attributes/keywords will be ignored.
        /// The input must be a valid extension subtags (excluding singleton).
        /// </summary>
        private void SetUnicodeLocaleExtension(ReadOnlySpan<char> subtags)
        {
            // wipe out existing attributes/keywords
            if (_uattributes != null)
            {
                _uattributes.Clear();
            }
            if (_ukeywords != null)
            {
                _ukeywords.Clear();
            }

            StringTokenEnumerator itr = new StringTokenEnumerator(subtags, LanguageTag.Separator);

            // parse attributes
            while (itr.MoveNext())
            {
                if (!UnicodeLocaleExtension.IsAttribute(itr.Current))
                {
                    break;
                }
                if (_uattributes == null)
                {
                    _uattributes = new JCG.HashSet<string>(4, AsciiStringComparer.OrdinalIgnoreCase);
                }
                _uattributes.Add(itr.Current.Text.ToString());
            }

            // parse keywords
            string? key = null;
            string type;
            int typeStart = -1;
            int typeEnd = -1;
            while (!itr.IsDone)
            {
                if (key != null)
                {
                    if (UnicodeLocaleExtension.IsKey(itr.Current))
                    {
                        // next keyword - emit previous one
                        Debug.Assert(typeStart == -1 || typeEnd != -1);
                        type = (typeStart == -1) ? "" : subtags.Slice(typeStart, typeEnd - typeStart).ToString(); // ICU4N: Corrected 2nd parameter
                        if (_ukeywords == null)
                        {
                            _ukeywords = new Dictionary<string, string>(4, AsciiStringComparer.OrdinalIgnoreCase);
                        }
                        _ukeywords[key] = type;

                        // reset keyword info
                        ReadOnlySpan<char> tmpKey = itr.Current;
                        key = _ukeywords.ContainsKey(tmpKey) ? null : tmpKey.ToString();
                        typeStart = typeEnd = -1;
                    }
                    else
                    {
                        if (typeStart == -1)
                        {
                            typeStart = itr.Current.StartIndex;
                        }
                        typeEnd = itr.Current.StartIndex + itr.Current.Text.Length;
                    }
                }
                else if (UnicodeLocaleExtension.IsKey(itr.Current))
                {
                    // 1. first keyword or
                    // 2. next keyword, but previous one was duplicate

                    if (_ukeywords != null && _ukeywords.ContainsKey(itr.Current))
                    {
                        // duplicate
                        key = null;
                    }
                    else
                    {
                        key = itr.Current.Text.ToString();
                    }
                }

                if (!itr.HasNext)
                {
                    if (key != null)
                    {
                        // last keyword
                        Debug.Assert(typeStart == -1 || typeEnd != -1);
                        type = (typeStart == -1) ? "" : subtags.Slice(typeStart, typeEnd - typeStart).ToString(); // ICU4N: Corrected 2nd parameter
                        if (_ukeywords == null)
                        {
                            _ukeywords = new Dictionary<string, string>(4, AsciiStringComparer.OrdinalIgnoreCase);
                        }
                        _ukeywords[key] = type;
                    }
                    break;
                }

                itr.MoveNext();
            }
        }
    }
}
