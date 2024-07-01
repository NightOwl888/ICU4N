using ICU4N.Impl.Locale;
using ICU4N.Util;
using System;

namespace ICU4N.Globalization
{
    /**
     * <code>Builder</code> is used to build instances of <code>UCultureInfo</code>
     * from values configured by the setters.  Unlike the <code>UCultureInfo</code>
     * constructors, the <code>Builder</code> checks if a value configured by a
     * setter satisfies the syntax requirements defined by the <code>UCultureInfo</code>
     * class.  A <code>UCultureInfo</code> object created by a <code>Builder</code> is
     * well-formed and can be transformed to a well-formed IETF BCP 47 language tag
     * without losing information.
     *
     * <para/><b>Note:</b> The <code>UCultureInfo</code> class does not provide any
     * syntactic restrictions on variant, while BCP 47 requires each variant
     * subtag to be 5 to 8 alphanumerics or a single numeric followed by 3
     * alphanumerics.  The method <code>setVariant</code> throws
     * <code>IllformedLocaleException</code> for a variant that does not satisfy
     * this restriction. If it is necessary to support such a variant, use a
     * UCultureInfo constructor.  However, keep in mind that a <code>UCultureInfo</code>
     * object created this way might lose the variant information when
     * transformed to a BCP 47 language tag.
     *
     * <para/>The following example shows how to create a <code>Locale</code> object
     * with the <code>Builder</code>.
     * <blockquote>
     * <pre>
     *     UCultureInfo aLocale = new UCultureInfoBuilder().SetLanguage("sr").SetScript("Latn").SetRegion("RS").Build();
     * </pre>
     * </blockquote>
     *
     * <para/>Builders can be reused; <code>Clear()</code> resets all
     * fields to their default values.
     *
     * @see UCultureInfo.IetfLanguageTag
     *
     * @stable ICU 4.2
     */
    // ICU4N TODO: API - need to analyze what to do with this.
    // AFAIK, it doesn't correspond with any type in .NET, but it does exist in the JDK
    // as a nested type of Locale.
    internal sealed class UCultureInfoBuilder
    {

        private readonly InternalLocaleBuilder _locbld;

        /**
         * Constructs an empty Builder. The default value of all
         * fields, extensions, and private use information is the
         * empty string.
         *
         * @stable ICU 4.2
         */
        public UCultureInfoBuilder()
        {
            _locbld = new InternalLocaleBuilder();
        }

        /**
         * Resets the <code>Builder</code> to match the provided
         * <code>locale</code>.  Existing state is discarded.
         *
         * <para/>All fields of the locale must be well-formed, see {@link Locale}.
         *
         * <para/>Locales with any ill-formed fields cause
         * <code>IllformedLocaleException</code> to be thrown.
         *
         * @param locale the locale
         * @return This builder.
         * @throws IllformedLocaleException if <code>locale</code> has
         * any ill-formed fields.
         * @throws NullPointerException if <code>locale</code> is null.
         *
         * @stable ICU 4.2
         */
        public UCultureInfoBuilder SetCulture(UCultureInfo locale)
        {
            try
            {
                _locbld.SetLocale(locale.Base, locale.LocaleExtensions);
            }
            catch (FormatException e)
            {
                throw new IllformedLocaleException(e.ToString(), e);
            }
            return this;
        }

        /**
         * Resets the Builder to match the provided IETF BCP 47
         * language tag.  Discards the existing state.  Null and the
         * empty string cause the builder to be reset, like {@link
         * #clear}.  Grandfathered tags (see {@link
         * UCultureInfo.GetCultureInfoByIetfLanguageTag(string)}) are converted to their canonical
         * form before being processed.  Otherwise, the language tag
         * must be well-formed (see {@link UCultureInfo}) or an exception is
         * thrown (unlike <code>UCultureInfo.GetCultureInfoByIetfLanguageTag(string)</code>, which
         * just discards ill-formed and following portions of the
         * tag).
         *
         * @param languageTag the language tag
         * @return This builder.
         * @throws IllformedLocaleException if <code>languageTag</code> is ill-formed
         * @see UCultureInfo.GetCultureInfoByIetfLanguageTag(string)
         *
         * @stable ICU 4.2
         */
        public UCultureInfoBuilder SetLanguageTag(string languageTag)
        {
            LanguageTag tag = LanguageTag.Parse(languageTag.AsSpan(), out ParseStatus sts);
            if (sts.IsError)
            {
                throw new IllformedLocaleException(sts.ErrorMessage, sts.ErrorIndex);
            }
            _locbld.SetLanguageTag(tag);

            return this;
        }

        /**
         * Sets the language.  If <code>language</code> is the empty string or
         * null, the language in this <code>Builder</code> is removed.  Otherwise,
         * the language must be <a href="./Locale.html#def_language">well-formed</a>
         * or an exception is thrown.
         *
         * <para/>The typical language value is a two or three-letter language
         * code as defined in ISO639.
         *
         * @param language the language
         * @return This builder.
         * @throws IllformedLocaleException if <code>language</code> is ill-formed
         *
         * @stable ICU 4.2
         */
        public UCultureInfoBuilder SetLanguage(string language)
        {
            try
            {
                _locbld.SetLanguage(language);
            }
            catch (FormatException e)
            {
                throw new IllformedLocaleException(e.Message /*, e.getErrorIndex()*/, e);
            }
            return this;
        }

        /**
         * Sets the script. If <code>script</code> is null or the empty string,
         * the script in this <code>Builder</code> is removed.
         * Otherwise, the script must be well-formed or an exception is thrown.
         *
         * <para/>The typical script value is a four-letter script code as defined by ISO 15924.
         *
         * @param script the script
         * @return This builder.
         * @throws IllformedLocaleException if <code>script</code> is ill-formed
         *
         * @stable ICU 4.2
         */
        public UCultureInfoBuilder SetScript(string script)
        {
            try
            {
                _locbld.SetScript(script);
            }
            catch (FormatException e)
            {
                throw new IllformedLocaleException(e.Message /*, e.getErrorIndex()*/, e);
            }
            return this;
        }

        /**
         * Sets the region.  If region is null or the empty string, the region
         * in this <code>Builder</code> is removed.  Otherwise,
         * the region must be well-formed or an exception is thrown.
         *
         * <para/>The typical region value is a two-letter ISO 3166 code or a
         * three-digit UN M.49 area code.
         *
         * <para/>The country value in the <code>Locale</code> created by the
         * <code>Builder</code> is always normalized to upper case.
         *
         * @param region the region
         * @return This builder.
         * @throws IllformedLocaleException if <code>region</code> is ill-formed
         *
         * @stable ICU 4.2
         */
        public UCultureInfoBuilder SetRegion(string region)
        {
            try
            {
                _locbld.SetRegion(region);
            }
            catch (FormatException e)
            {
                throw new IllformedLocaleException(e.Message /*, e.getErrorIndex()*/, e);
            }
            return this;
        }

        /**
         * Sets the variant.  If variant is null or the empty string, the
         * variant in this <code>Builder</code> is removed.  Otherwise, it
         * must consist of one or more well-formed subtags, or an exception is thrown.
         *
         * <para/><b>Note:</b> This method checks if <code>variant</code>
         * satisfies the IETF BCP 47 variant subtag's syntax requirements,
         * and normalizes the value to lowercase letters.  However,
         * the <code>UCultureInfo</code> class does not impose any syntactic
         * restriction on variant.  To set such a variant,
         * use a UCultureInfo constructor.
         *
         * @param variant the variant
         * @return This builder.
         * @throws IllformedLocaleException if <code>variant</code> is ill-formed
         *
         * @stable ICU 4.2
         */
        public UCultureInfoBuilder SetVariant(string variant)
        {
            try
            {
                _locbld.SetVariant(variant);
            }
            catch (FormatException e)
            {
                throw new IllformedLocaleException(e.Message /*, e.getErrorIndex()*/, e);
            }
            return this;
        }

        /**
         * Sets the extension for the given key. If the value is null or the
         * empty string, the extension is removed.  Otherwise, the extension
         * must be well-formed or an exception is thrown.
         *
         * <para/><b>Note:</b> The key {@link UCultureInfo.UnicodeLocaleExtension
         * } ('u') is used for the Unicode locale extension.
         * Setting a value for this key replaces any existing Unicode locale key/type
         * pairs with those defined in the extension.
         *
         * <para/><b>Note:</b> The key {@link UCultureInfo.PrivateUseExtension
         * } ('x') is used for the private use code. To be
         * well-formed, the value for this key needs only to have subtags of one to
         * eight alphanumeric characters, not two to eight as in the general case.
         *
         * @param key the extension key
         * @param value the extension value
         * @return This builder.
         * @throws IllformedLocaleException if <code>key</code> is illegal
         * or <code>value</code> is ill-formed
         * @see #SetUnicodeLocaleKeyword(string, string)
         *
         * @stable ICU 4.2
         */
        public UCultureInfoBuilder SetExtension(char key, string value)
        {
            try
            {
                _locbld.SetExtension(key, value);
            }
            catch (FormatException e)
            {
                throw new IllformedLocaleException(e.Message /*, e.getErrorIndex()*/, e);
            }
            return this;
        }

        /**
         * Sets the Unicode locale keyword type for the given key.  If the type
         * is null, the Unicode keyword is removed.  Otherwise, the key must be
         * non-null and both key and type must be well-formed or an exception
         * is thrown.
         *
         * <para/>Keys and types are converted to lower case.
         *
         * <para/><b>Note</b>:Setting the 'u' extension via {@link #setExtension}
         * replaces all Unicode locale keywords with those defined in the
         * extension.
         *
         * @param key the Unicode locale key
         * @param type the Unicode locale type
         * @return This builder.
         * @throws IllformedLocaleException if <code>key</code> or <code>type</code>
         * is ill-formed
         * @throws NullPointerException if <code>key</code> is null
         * @see #setExtension(char, string)
         *
         * @stable ICU 4.4
         */
        public UCultureInfoBuilder SetUnicodeLocaleKeyword(string key, string type)
        {
            try
            {
                _locbld.SetUnicodeLocaleKeyword(key, type);
            }
            catch (FormatException e)
            {
                throw new IllformedLocaleException(e.Message /*, e.getErrorIndex()*/, e);
            }
            return this;
        }

        /**
         * Adds a unicode locale attribute, if not already present, otherwise
         * has no effect.  The attribute must not be null and must be well-formed
         * or an exception is thrown.
         *
         * @param attribute the attribute
         * @return This builder.
         * @throws NullPointerException if <code>attribute</code> is null
         * @throws IllformedLocaleException if <code>attribute</code> is ill-formed
         * @see #setExtension(char, string)
         *
         * @stable ICU 4.6
         */
        public UCultureInfoBuilder AddUnicodeLocaleAttribute(string attribute)
        {
            try
            {
                _locbld.AddUnicodeLocaleAttribute(attribute);
            }
            catch (FormatException e)
            {
                throw new IllformedLocaleException(e.Message /*, e.getErrorIndex()*/, e);
            }
            return this;
        }

        /**
         * Removes a unicode locale attribute, if present, otherwise has no
         * effect.  The attribute must not be null and must be well-formed
         * or an exception is thrown.
         *
         * <para/>Attribute comparision for removal is case-insensitive.
         *
         * @param attribute the attribute
         * @return This builder.
         * @throws NullPointerException if <code>attribute</code> is null
         * @throws IllformedLocaleException if <code>attribute</code> is ill-formed
         * @see #setExtension(char, string)
         *
         * @stable ICU 4.6
         */
        public UCultureInfoBuilder RemoveUnicodeLocaleAttribute(string attribute)
        {
            try
            {
                _locbld.RemoveUnicodeLocaleAttribute(attribute);
            }
            catch (FormatException e)
            {
                throw new IllformedLocaleException(e.Message /*, e.getErrorIndex()*/, e);
            }
            return this;
        }

        /**
         * Resets the builder to its initial, empty state.
         *
         * @return this builder
         *
         * @stable ICU 4.2
         */
        public UCultureInfoBuilder Clear()
        {
            _locbld.Clear();
            return this;
        }

        /**
         * Resets the extensions to their initial, empty state.
         * Language, script, region and variant are unchanged.
         *
         * @return this builder
         * @see #setExtension(char, string)
         *
         * @stable ICU 4.2
         */
        public UCultureInfoBuilder ClearExtensions()
        {
            _locbld.ClearExtensions();
            return this;
        }

        /**
         * Returns an instance of <code>UCultureInfo</code> created from the fields set
         * on this builder.
         *
         * @return a new CultureInfo
         *
         * @stable ICU 4.4
         */
        public UCultureInfo Build()
        {
            return UCultureInfo.GetInstance(_locbld.GetBaseLocale(), _locbld.GetLocaleExtensions());
        }
    }
}

