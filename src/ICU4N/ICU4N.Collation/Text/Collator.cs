using ICU4N.Impl;
using ICU4N.Impl.Coll;
using ICU4N.Lang;
using ICU4N.Support;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using Category = ICU4N.Util.ULocale.Category; // ICU4N TODO: De-nest?

namespace ICU4N.Text
{
    /// <summary>Use this to set the strength of a Collator object.
	///  This is also used to determine the strength of sort keys
	///  generated from Collator objects
	/// The usual strength for most locales (except Japanese) is tertiary.
	/// Quaternary strength is useful when combined with shifted setting
	/// for alternate handling attribute and for JIS x 4061 collation,
	/// when it is used to distinguish between Katakana and Hiragana
	/// (this is achieved by setting the <see cref="RuleBasedCollator.IsHiraganaQuaternary"/> to true.
	/// Otherwise, quaternary level is affected only by the number of
	/// non ignorable code points in the string.
	/// </summary>
    public enum CollationStrength
    {
        /// <summary>
		/// Use the strength set in the locale or rules
		/// </summary>
		Default = -1,
        /// <summary>
        /// Base letter represents a primary difference.  Set comparison
        /// level to Primary to ignore secondary and tertiary differences.
        /// Example of primary difference, "abc" &lt; "abd"
        /// </summary>
        Primary = 0,
        /// <summary>
        /// Diacritical differences on the same base letter represent a secondary
        /// difference.  Set comparison level to Secondary to ignore tertiary
        /// differences.
        /// Example of secondary difference, "a&#x308;" >> "a".
        /// </summary>
        Secondary = 1,
        /// <summary>
        ///  Uppercase and lowercase versions of the same character represents a
        /// tertiary difference.  Set comparison level to Tertiary to include
        ///  all comparison differences.
        ///  Example of tertiary difference, "abc" &lt;&lt;&lt; "ABC".
        /// </summary>
        Tertiary = 2,
        /// <summary>
        /// Quaternary level is usually only affected by the number of
        /// non-ignorable code points in the string.
        /// </summary>
        Quaternary = 3,
        /// <summary>
        ///  Two characters are considered "identical" when they have the same
        /// unicode spellings.
        /// For example, "a&#x308;" == "a&#x308;".
        /// </summary>
        /// <remarks>Identical strength is rarely useful, as it amounts to
        /// codepoints of the NFD form of the string</remarks>
        Identical = 15
    }

    public enum NormalizationMode
    {
        //FullDecomposition = CollationStrength.Identical, // NOT SUPPORTED by ICU (was only for compatibility with Java APIs)
        NoDecomposition = 16,
        CanonicalDecomposition = 17
    }

    /// <summary>
    ///  Reordering codes for non-script groups that can be reordered under collation.
    /// </summary>
    /// <see cref="Collator.GetReorderCodes()"/>
    /// <see cref="Collator.SetReorderCodes(int[])"/>
    /// <see cref="Collator.GetEquivalentReorderCodes(int)"/>
    /// <stable>ICU 4.8</stable>
    public static class ReorderCodes
    {
        /// <summary>
        /// A special reordering code that is used to specify the default reordering codes for a locale.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public const int Default = -1;
        /// <summary>
        /// A special reordering code that is used to specify no reordering codes.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public const int None = UScript.Unknown;
        /// <summary>
        /// A special reordering code that is used to specify all other codes used for reordering except
        /// for the codes listed as ReorderingCodes and those listed explicitly in a reordering.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public const int Others = UScript.Unknown;
        /// <summary>
        /// Characters with the space property.
        /// This is equivalent to the rule value "space".
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public const int Space = 0x1000;
        /// <summary>
        /// The first entry in the enumeration of reordering groups. This is intended for use in
        /// range checking and enumeration of the reorder codes.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public const int First = Space;
        /// <summary>
        /// Characters with the punctuation property.
        /// This is equivalent to the rule value "punct".
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public const int Punctuation = 0x1001;
        /// <summary>
        /// Characters with the symbol property.
        /// This is equivalent to the rule value "symbol".
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public const int Symbol = 0x1002;
        /// <summary>
        /// Characters with the currency property.
        /// This is equivalent to the rule value "currency".
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public const int Currency = 0x1003;
        /// <summary>
        /// Characters with the digit property.
        /// This is equivalent to the rule value "digit".
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public const int Digit = 0x1004;
        /// <summary>
        /// One more than the highest normal <see cref="ReorderCodes"/> value.
        /// </summary>
        [Obsolete("ICU 58 The numeric value may change over time; see ICU ticket #12420.")]
        public const int Limit = 0x1005;
    }


    public abstract class Collator : IComparer<object>, IFreezable<Collator>
#if FEATURE_CLONEABLE
        , ICloneable
#endif
    {
        //// public data members ---------------------------------------------------

        /**
         * Strongest collator strength value. Typically used to denote differences
         * between base characters. See class documentation for more explanation.
         * @see #setStrength
         * @see #getStrength
         * @stable ICU 2.8
         */
        public const CollationStrength PRIMARY = CollationStrength.Primary;

        /**
         * Second level collator strength value.
         * Accents in the characters are considered secondary differences.
         * Other differences between letters can also be considered secondary
         * differences, depending on the language.
         * See class documentation for more explanation.
         * @see #setStrength
         * @see #getStrength
         * @stable ICU 2.8
         */
        public const CollationStrength SECONDARY = CollationStrength.Secondary;

        /**
         * Third level collator strength value.
         * Upper and lower case differences in characters are distinguished at this
         * strength level. In addition, a variant of a letter differs from the base
         * form on the tertiary level.
         * See class documentation for more explanation.
         * @see #setStrength
         * @see #getStrength
         * @stable ICU 2.8
         */
        public const CollationStrength TERTIARY = CollationStrength.Tertiary;

        /**
         * {@icu} Fourth level collator strength value.
         * When punctuation is ignored
         * (see <a href="http://userguide.icu-project.org/collation/concepts#TOC-Ignoring-Punctuation">
         * Ignoring Punctuation in the User Guide</a>) at PRIMARY to TERTIARY
         * strength, an additional strength level can
         * be used to distinguish words with and without punctuation.
         * See class documentation for more explanation.
         * @see #setStrength
         * @see #getStrength
         * @stable ICU 2.8
         */
        public const CollationStrength QUATERNARY = CollationStrength.Quaternary;

        /**
         * Smallest Collator strength value. When all other strengths are equal,
         * the IDENTICAL strength is used as a tiebreaker. The Unicode code point
         * values of the NFD form of each string are compared, just in case there
         * is no difference.
         * See class documentation for more explanation.
         * <p>
         * Note this value is different from JDK's
         * @stable ICU 2.8
         */
        public const CollationStrength IDENTICAL = CollationStrength.Identical;

        ///**
        // * {@icunote} This is for backwards compatibility with Java APIs only.  It
        // * should not be used, IDENTICAL should be used instead.  ICU's
        // * collation does not support Java's FULL_DECOMPOSITION mode.
        // * @stable ICU 3.4
        // */
        //public const NormalizationMode FULL_DECOMPOSITION = (NormalizationMode)IDENTICAL;

        /**
         * Decomposition mode value. With NO_DECOMPOSITION set, Strings
         * will not be decomposed for collation. This is the default
         * decomposition setting unless otherwise specified by the locale
         * used to create the Collator.
         *
         * <p><strong>Note</strong> this value is different from the JDK's.
         * @see #CANONICAL_DECOMPOSITION
         * @see #getDecomposition
         * @see #setDecomposition
         * @stable ICU 2.8
         */
        public const NormalizationMode NO_DECOMPOSITION = NormalizationMode.NoDecomposition;

        /**
         * Decomposition mode value. With CANONICAL_DECOMPOSITION set,
         * characters that are canonical variants according to the Unicode standard
         * will be decomposed for collation.
         *
         * <p>CANONICAL_DECOMPOSITION corresponds to Normalization Form D as
         * described in <a href="http://www.unicode.org/unicode/reports/tr15/">
         * Unicode Technical Report #15</a>.
         *
         * @see #NO_DECOMPOSITION
         * @see #getDecomposition
         * @see #setDecomposition
         * @stable ICU 2.8
         */
        public const NormalizationMode CANONICAL_DECOMPOSITION = NormalizationMode.CanonicalDecomposition;

        // ICU4N specific - De-nested ReorderCodes class

        // public methods --------------------------------------------------------

        /**
         * Compares the equality of two Collator objects. Collator objects are equal if they have the same
         * collation (sorting &amp; searching) behavior.
         *
         * <p>The base class checks for null and for equal types.
         * Subclasses should override.
         *
         * @param obj the Collator to compare to.
         * @return true if this Collator has exactly the same collation behavior as obj, false otherwise.
         * @stable ICU 2.8
         */
        public override bool Equals(object obj)
        {
            // Subclasses: Call this method and then add more specific checks.
            return this == obj || (obj != null && GetType() == obj.GetType());
        }

        /**
         * Generates a hash code for this Collator object.
         *
         * <p>The implementation exists just for consistency with {@link #equals(Object)}
         * implementation in this class and does not generate a useful hash code.
         * Subclasses should override this implementation.
         *
         * @return a hash code value.
         * @stable ICU 58
         */
        public override int GetHashCode()
        {
            // Dummy return to prevent compile warnings.
            return 0;
        }

        // public setters --------------------------------------------------------

        private void CheckNotFrozen()
        {
            if (IsFrozen)
            {
                throw new NotSupportedException("Attempt to modify frozen Collator");
            }
        }

        /**
         * Sets this Collator's strength attribute. The strength attribute
         * determines the minimum level of difference considered significant
         * during comparison.
         *
         * <p>The base class method does nothing. Subclasses should override it if appropriate.
         *
         * <p>See the Collator class description for an example of use.
         * @param newStrength the new strength value.
         * @see #getStrength
         * @see #PRIMARY
         * @see #SECONDARY
         * @see #TERTIARY
         * @see #QUATERNARY
         * @see #IDENTICAL
         * @throws IllegalArgumentException if the new strength value is not valid.
         * @stable ICU 2.8
         */
        //public void setStrength(int newStrength)
        //{
        //    CheckNotFrozen();
        //}

        public virtual CollationStrength Strength
        {
            get { return CollationStrength.Tertiary; }
            set { CheckNotFrozen(); }
        }

        /**
         * @return this, for chaining
         * @internal Used in UnicodeTools
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public virtual Collator SetStrength2(CollationStrength newStrength)
        {
            Strength = newStrength;
            return this;
        }

        /**
         * Sets the decomposition mode of this Collator.  Setting this
         * decomposition attribute with CANONICAL_DECOMPOSITION allows the
         * Collator to handle un-normalized text properly, producing the
         * same results as if the text were normalized. If
         * NO_DECOMPOSITION is set, it is the user's responsibility to
         * insure that all text is already in the appropriate form before
         * a comparison or before getting a CollationKey. Adjusting
         * decomposition mode allows the user to select between faster and
         * more complete collation behavior.
         *
         * <p>Since a great many of the world's languages do not require
         * text normalization, most locales set NO_DECOMPOSITION as the
         * default decomposition mode.
         *
         * <p>The base class method does nothing. Subclasses should override it if appropriate.
         *
         * <p>See getDecomposition for a description of decomposition
         * mode.
         *
         * @param decomposition the new decomposition mode
         * @see #getDecomposition
         * @see #NO_DECOMPOSITION
         * @see #CANONICAL_DECOMPOSITION
         * @throws IllegalArgumentException If the given value is not a valid
         *            decomposition mode.
         * @stable ICU 2.8
         */
        //public void setDecomposition(int decomposition)
        //{
        //    CheckNotFrozen();
        //}

        public virtual NormalizationMode Decomposition
        {
            get { return NormalizationMode.NoDecomposition; }
            set { CheckNotFrozen(); }
        }

        /**
         * Sets the reordering codes for this collator.
         * Collation reordering allows scripts and some other groups of characters
         * to be moved relative to each other. This reordering is done on top of
         * the DUCET/CLDR standard collation order. Reordering can specify groups to be placed
         * at the start and/or the end of the collation order. These groups are specified using
         * UScript codes and {@link Collator.ReorderCodes} entries.
         *
         * <p>By default, reordering codes specified for the start of the order are placed in the
         * order given after several special non-script blocks. These special groups of characters
         * are space, punctuation, symbol, currency, and digit. These special groups are represented with
         * {@link Collator.ReorderCodes} entries. Script groups can be intermingled with
         * these special non-script groups if those special groups are explicitly specified in the reordering.
         *
         * <p>The special code {@link Collator.ReorderCodes#OTHERS OTHERS}
         * stands for any script that is not explicitly
         * mentioned in the list of reordering codes given. Anything that is after OTHERS
         * will go at the very end of the reordering in the order given.
         *
         * <p>The special reorder code {@link Collator.ReorderCodes#DEFAULT DEFAULT}
         * will reset the reordering for this collator
         * to the default for this collator. The default reordering may be the DUCET/CLDR order or may be a reordering that
         * was specified when this collator was created from resource data or from rules. The
         * DEFAULT code <b>must</b> be the sole code supplied when it is used.
         * If not, then an {@link IllegalArgumentException} will be thrown.
         *
         * <p>The special reorder code {@link Collator.ReorderCodes#NONE NONE}
         * will remove any reordering for this collator.
         * The result of setting no reordering will be to have the DUCET/CLDR ordering used. The
         * NONE code <b>must</b> be the sole code supplied when it is used.
         *
         * @param order the reordering codes to apply to this collator; if this is null or an empty array
         * then this clears any existing reordering
         * @see #getReorderCodes
         * @see #getEquivalentReorderCodes
         * @see Collator.ReorderCodes
         * @see UScript
         * @stable ICU 4.8
         */
        public virtual void SetReorderCodes(params int[] order)
        {
            throw new NotSupportedException("Needs to be implemented by the subclass.");
        }

        //public virtual ReorderCodes ReorderCodes
        //{
        //    get { return default(ReorderCodes); }
        //    set { throw new NotSupportedException("Needs to be implemented by the subclass."); }
        //}

        // public getters --------------------------------------------------------

        /**
         * Returns the Collator for the current default locale.
         * The default locale is determined by java.util.Locale.getDefault().
         * @return the Collator for the default locale (for example, en_US) if it
         *         is created successfully. Otherwise if there is no Collator
         *         associated with the current locale, the root collator
         *         will be returned.
         * @see java.util.Locale#getDefault()
         * @see #getInstance(Locale)
         * @stable ICU 2.8
         */
        public static Collator GetInstance()
        {
            return GetInstance(ULocale.GetDefault());
        }

        /**
         * Clones the collator.
         * @stable ICU 2.6
         * @return a clone of this collator.
         */
        public virtual object Clone()
        {
            return base.MemberwiseClone();
        }

        // begin registry stuff

        /**
         * A factory used with registerFactory to register multiple collators and provide
         * display names for them.  If standard locale display names are sufficient,
         * Collator instances may be registered instead.
         * <p><b>Note:</b> as of ICU4J 3.2, the default API for CollatorFactory uses
         * ULocale instead of Locale.  Instead of overriding createCollator(Locale),
         * new implementations should override createCollator(ULocale).  Note that
         * one of these two methods <b>MUST</b> be overridden or else an infinite
         * loop will occur.
         * @stable ICU 2.6
         */
        public abstract class CollatorFactory
        {
            /**
             * Return true if this factory will be visible.  Default is true.
             * If not visible, the locales supported by this factory will not
             * be listed by getAvailableLocales.
             *
             * @return true if this factory is visible
             * @stable ICU 2.6
             */
            public virtual bool Visible
            {
                get { return true; }
            }

            /**
             * Return an instance of the appropriate collator.  If the locale
             * is not supported, return null.
             * <b>Note:</b> as of ICU4J 3.2, implementations should override
             * this method instead of createCollator(Locale).
             * @param loc the locale for which this collator is to be created.
             * @return the newly created collator.
             * @stable ICU 3.2
             */
            public virtual Collator CreateCollator(ULocale loc)
            {
                return CreateCollator(loc.ToLocale());
            }

            /**
             * Return an instance of the appropriate collator.  If the locale
             * is not supported, return null.
             * <p><b>Note:</b> as of ICU4J 3.2, implementations should override
             * createCollator(ULocale) instead of this method, and inherit this
             * method's implementation.  This method is no longer abstract
             * and instead delegates to createCollator(ULocale).
             * @param loc the locale for which this collator is to be created.
             * @return the newly created collator.
             * @stable ICU 2.6
             */
            public virtual Collator CreateCollator(CultureInfo loc)
            {
                return CreateCollator(ULocale.ForLocale(loc));
            }

            /**
             * Return the name of the collator for the objectLocale, localized for the displayLocale.
             * If objectLocale is not visible or not defined by the factory, return null.
             * @param objectLocale the locale identifying the collator
             * @param displayLocale the locale for which the display name of the collator should be localized
             * @return the display name
             * @stable ICU 2.6
             */
            public virtual string GetDisplayName(CultureInfo objectLocale, CultureInfo displayLocale)
            {
                return Collator.GetDisplayName(ULocale.ForLocale(objectLocale), ULocale.ForLocale(displayLocale));
            }

            /**
             * Return the name of the collator for the objectLocale, localized for the displayLocale.
             * If objectLocale is not visible or not defined by the factory, return null.
             * @param objectLocale the locale identifying the collator
             * @param displayLocale the locale for which the display name of the collator should be localized
             * @return the display name
             * @stable ICU 3.2
             */
            public virtual string GetDisplayName(ULocale objectLocale, ULocale displayLocale)
            {
                if (Visible)
                {
                    ICollection<string> supported = GetSupportedLocaleIDs();
                    string name = objectLocale.GetBaseName();
                    if (supported.Contains(name))
                    {
                        return objectLocale.GetDisplayName(displayLocale);
                    }
                }
                return null;
            }

            /**
             * Return an unmodifiable collection of the locale names directly
             * supported by this factory.
             *
             * @return the set of supported locale IDs.
             * @stable ICU 2.6
             */
            public abstract ICollection<string> GetSupportedLocaleIDs(); // ICU4N TODO: API - property ?

            /**
             * Empty default constructor.
             * @stable ICU 2.6
             */
            protected CollatorFactory()
            {
            }
        }

        internal abstract class ServiceShim
        {
            internal abstract Collator GetInstance(ULocale l);
            internal abstract object RegisterInstance(Collator c, ULocale l);
            internal abstract object RegisterFactory(CollatorFactory f);
            internal abstract bool Unregister(Object k);
            internal abstract CultureInfo[] GetAvailableLocales(); // TODO remove
            internal abstract ULocale[] GetAvailableULocales();
            internal abstract string GetDisplayName(ULocale ol, ULocale dl);
        }

        private static ServiceShim shim;
        private static ServiceShim GetShim()
        {
            // Note: this instantiation is safe on loose-memory-model configurations
            // despite lack of synchronization, since the shim instance has no state--
            // it's all in the class init.  The worst problem is we might instantiate
            // two shim instances, but they'll share the same state so that's ok.
            if (shim == null)
            {
                try
                {
                    Type cls = Type.GetType("ICU4N.Text.CollatorServiceShim, ICU4N.Collation");
                    shim = (ServiceShim)Activator.CreateInstance(cls);
                }
                catch (MissingManifestResourceException e)
                {
                    ///CLOVER:OFF
                    throw e;
                    ///CLOVER:ON
                }
                catch (Exception e)
                {
                    ///CLOVER:OFF
                    if (DEBUG)
                    {
                        e.PrintStackTrace();
                    }
                    throw new ICUException(e);
                    ///CLOVER:ON
                }
            }
            return shim;
        }

        /**
         * Simpler/faster methods for ASCII than ones based on Unicode data.
         * TODO: There should be code like this somewhere already??
         */
        private sealed class ASCII
        {
            internal static bool EqualIgnoreCase(string left, string right) // ICU4N specific - changed params from ICharSequence to string
            {
                int length = left.Length;
                if (length != right.Length) { return false; }
                for (int i = 0; i < length; ++i)
                {
                    char lc = left[i];
                    char rc = right[i];
                    if (lc == rc) { continue; }
                    if ('A' <= lc && lc <= 'Z')
                    {
                        if ((lc + 0x20) == rc) { continue; }
                    }
                    else if ('A' <= rc && rc <= 'Z')
                    {
                        if ((rc + 0x20) == lc) { continue; }
                    }
                    return false;
                }
                return true;
            }
        }

        private static bool GetYesOrNo(string keyword, string s)
        {
            if (ASCII.EqualIgnoreCase(s, "yes"))
            {
                return true;
            }
            if (ASCII.EqualIgnoreCase(s, "no"))
            {
                return false;
            }
            throw new ArgumentException("illegal locale keyword=value: " + keyword + "=" + s);
        }

        private static int GetInt32Value(string keyword, string s, params string[] values)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                if (ASCII.EqualIgnoreCase(s, values[i]))
                {
                    return i;
                }
            }
            throw new ArgumentException("illegal locale keyword=value: " + keyword + "=" + s);
        }

        private static int GetReorderCode(string keyword, string s)
        {
            return ReorderCodes.First +
                    GetInt32Value(keyword, s, "space", "punct", "symbol", "currency", "digit");
            // Not supporting "others" = UCOL_REORDER_CODE_OTHERS
            // as a synonym for Zzzz = USCRIPT_UNKNOWN for now:
            // Avoid introducing synonyms/aliases.
        }

        /**
         * Sets collation attributes according to locale keywords. See
         * http://www.unicode.org/reports/tr35/tr35-collation.html#Collation_Settings
         *
         * Using "alias" keywords and values where defined:
         * http://www.unicode.org/reports/tr35/tr35.html#Old_Locale_Extension_Syntax
         * http://unicode.org/repos/cldr/trunk/common/bcp47/collation.xml
         */
        private static void SetAttributesFromKeywords(ULocale loc, Collator coll, RuleBasedCollator rbc)
        {
            // Check for collation keywords that were already deprecated
            // before any were supported in createInstance() (except for "collation").
            string value = loc.GetKeywordValue("colHiraganaQuaternary");
            if (value != null)
            {
                throw new NotSupportedException("locale keyword kh/colHiraganaQuaternary");
            }
            value = loc.GetKeywordValue("variableTop");
            if (value != null)
            {
                throw new NotSupportedException("locale keyword vt/variableTop");
            }
            // Parse known collation keywords, ignore others.
            value = loc.GetKeywordValue("colStrength");
            if (value != null)
            {
                // Note: Not supporting typo "quarternary" because it was never supported in locale IDs.
                int strength = GetInt32Value("colStrength", value,
                        "primary", "secondary", "tertiary", "quaternary", "identical");
                coll.Strength = strength <= (int)CollationStrength.Quaternary ? (CollationStrength)strength : CollationStrength.Identical;
            }
            value = loc.GetKeywordValue("colBackwards");
            if (value != null)
            {
                if (rbc != null)
                {
                    rbc.IsFrenchCollation=GetYesOrNo("colBackwards", value);
                }
                else
                {
                    throw new NotSupportedException(
                            "locale keyword kb/colBackwards only settable for RuleBasedCollator");
                }
            }
            value = loc.GetKeywordValue("colCaseLevel");
            if (value != null)
            {
                if (rbc != null)
                {
                    rbc.IsCaseLevel = GetYesOrNo("colCaseLevel", value);
                }
                else
                {
                    throw new NotSupportedException(
                            "locale keyword kb/colBackwards only settable for RuleBasedCollator");
                }
            }
            value = loc.GetKeywordValue("colCaseFirst");
            if (value != null)
            {
                if (rbc != null)
                {
                    int cf = GetInt32Value("colCaseFirst", value, "no", "lower", "upper");
                    if (cf == 0)
                    {
                        rbc.IsLowerCaseFirst=false;
                        rbc.IsUpperCaseFirst=false;
                    }
                    else if (cf == 1)
                    {
                        rbc.IsLowerCaseFirst=true;
                    }
                    else /* cf == 2 */
                    {
                        rbc.IsUpperCaseFirst=true;
                    }
                }
                else
                {
                    throw new NotSupportedException(
                            "locale keyword kf/colCaseFirst only settable for RuleBasedCollator");
                }
            }
            value = loc.GetKeywordValue("colAlternate");
            if (value != null)
            {
                if (rbc != null)
                {
                    rbc.IsAlternateHandlingShifted=
                            GetInt32Value("colAlternate", value, "non-ignorable", "shifted") != 0;
                }
                else
                {
                    throw new NotSupportedException(
                            "locale keyword ka/colAlternate only settable for RuleBasedCollator");
                }
            }
            value = loc.GetKeywordValue("colNormalization");
            if (value != null)
            {
                coll.Decomposition = GetYesOrNo("colNormalization", value) ?
                        NormalizationMode.CanonicalDecomposition : NormalizationMode.NoDecomposition;
            }
            value = loc.GetKeywordValue("colNumeric");
            if (value != null)
            {
                if (rbc != null)
                {
                    rbc.IsNumericCollation=GetYesOrNo("colNumeric", value);
                }
                else
                {
                    throw new NotSupportedException(
                            "locale keyword kn/colNumeric only settable for RuleBasedCollator");
                }
            }
            value = loc.GetKeywordValue("colReorder");
            if (value != null)
            {
                int[] codes = new int[UScript.CodeLimit + ReorderCodes.Limit - ReorderCodes.First];
                int codesLength = 0;
                int scriptNameStart = 0;
                for (; ; )
                {
                    if (codesLength == codes.Length)
                    {
                        throw new ArgumentException(
                                "too many script codes for colReorder locale keyword: " + value);
                    }
                    int limit = scriptNameStart;
                    while (limit < value.Length && value[limit] != '-') { ++limit; }
                    string scriptName = value.Substring(scriptNameStart, limit - scriptNameStart); // ICU4N: Corrected 2nd parameter
                    int code;
                    if (scriptName.Length == 4)
                    {
                        // Strict parsing, accept only 4-letter script codes, not long names.
                        code = UCharacter.GetPropertyValueEnum(UProperty.SCRIPT, scriptName);
                    }
                    else
                    {
                        code = GetReorderCode("colReorder", scriptName);
                    }
                    codes[codesLength++] = code;
                    if (limit == value.Length) { break; }
                    scriptNameStart = limit + 1;
                }
                if (codesLength == 0)
                {
                    throw new ArgumentException("no script codes for colReorder locale keyword");
                }
                int[] args = new int[codesLength];
                System.Array.Copy(codes, 0, args, 0, codesLength);
                coll.SetReorderCodes(args);
            }
            value = loc.GetKeywordValue("kv");
            if (value != null)
            {
                coll.MaxVariable = GetReorderCode("kv", value);
            }
        }

        /**
         * {@icu} Returns the Collator for the desired locale.
         *
         * <p>For some languages, multiple collation types are available;
         * for example, "de@collation=phonebook".
         * Starting with ICU 54, collation attributes can be specified via locale keywords as well,
         * in the old locale extension syntax ("el@colCaseFirst=upper")
         * or in language tag syntax ("el-u-kf-upper").
         * See <a href="http://userguide.icu-project.org/collation/api">User Guide: Collation API</a>.
         *
         * @param locale the desired locale.
         * @return Collator for the desired locale if it is created successfully.
         *         Otherwise if there is no Collator
         *         associated with the current locale, the root collator will
         *         be returned.
         * @see java.util.Locale
         * @see java.util.ResourceBundle
         * @see #getInstance(Locale)
         * @see #getInstance()
         * @stable ICU 3.0
         */
        public static Collator GetInstance(ULocale locale)
        {
            // fetching from service cache is faster than instantiation
            if (locale == null)
            {
                locale = ULocale.GetDefault();
            }
            Collator coll = GetShim().GetInstance(locale);
            if (!locale.GetName().Equals(locale.GetBaseName()))
            {  // any keywords?
                SetAttributesFromKeywords(locale, coll,
                        (coll is RuleBasedCollator) ? (RuleBasedCollator)coll : null);
            }
            return coll;
        }

        /**
         * Returns the Collator for the desired locale.
         *
         * <p>For some languages, multiple collation types are available;
         * for example, "de-u-co-phonebk".
         * Starting with ICU 54, collation attributes can be specified via locale keywords as well,
         * in the old locale extension syntax ("el@colCaseFirst=upper", only with {@link ULocale})
         * or in language tag syntax ("el-u-kf-upper").
         * See <a href="http://userguide.icu-project.org/collation/api">User Guide: Collation API</a>.
         *
         * @param locale the desired locale.
         * @return Collator for the desired locale if it is created successfully.
         *         Otherwise if there is no Collator
         *         associated with the current locale, the root collator will
         *         be returned.
         * @see java.util.Locale
         * @see java.util.ResourceBundle
         * @see #getInstance(ULocale)
         * @see #getInstance()
         * @stable ICU 2.8
         */
        public static Collator GetInstance(CultureInfo locale)
        {
            return GetInstance(ULocale.ForLocale(locale));
        }

        /**
         * {@icu} Registers a collator as the default collator for the provided locale.  The
         * collator should not be modified after it is registered.
         *
         * <p>Because ICU may choose to cache Collator objects internally, this must
         * be called at application startup, prior to any calls to
         * Collator.getInstance to avoid undefined behavior.
         *
         * @param collator the collator to register
         * @param locale the locale for which this is the default collator
         * @return an object that can be used to unregister the registered collator.
         *
         * @stable ICU 3.2
         */
        public static object RegisterInstance(Collator collator, ULocale locale)
        {
            return GetShim().RegisterInstance(collator, locale);
        }

        /**
         * {@icu} Registers a collator factory.
         *
         * <p>Because ICU may choose to cache Collator objects internally, this must
         * be called at application startup, prior to any calls to
         * Collator.getInstance to avoid undefined behavior.
         *
         * @param factory the factory to register
         * @return an object that can be used to unregister the registered factory.
         *
         * @stable ICU 2.6
         */
        public static object RegisterFactory(CollatorFactory factory)
        {
            return GetShim().RegisterFactory(factory);
        }

        /**
         * {@icu} Unregisters a collator previously registered using registerInstance.
         * @param registryKey the object previously returned by registerInstance.
         * @return true if the collator was successfully unregistered.
         * @stable ICU 2.6
         */
        public static bool Unregister(object registryKey)
        {
            if (shim == null)
            {
                return false;
            }
            return shim.Unregister(registryKey);
        }

        /**
         * Returns the set of locales, as Locale objects, for which collators
         * are installed.  Note that Locale objects do not support RFC 3066.
         * @return the list of locales in which collators are installed.
         * This list includes any that have been registered, in addition to
         * those that are installed with ICU4J.
         * @stable ICU 2.4
         */
        public static CultureInfo[] GetAvailableLocales()
        {
            // TODO make this wrap getAvailableULocales later
            if (shim == null)
            {
                return ICUResourceBundle.GetAvailableLocales(
                    ICUData.ICU_COLLATION_BASE_NAME, CollationData.ICU_DATA_CLASS_LOADER /* ICUResourceBundle.ICU_DATA_CLASS_LOADER */);
            }
            return shim.GetAvailableLocales();
        }

        /**
         * {@icu} Returns the set of locales, as ULocale objects, for which collators
         * are installed.  ULocale objects support RFC 3066.
         * @return the list of locales in which collators are installed.
         * This list includes any that have been registered, in addition to
         * those that are installed with ICU4J.
         * @stable ICU 3.0
         */
        public static ULocale[] GetAvailableULocales()
        {
            if (shim == null)
            {
                return ICUResourceBundle.GetAvailableULocales(
                    ICUData.ICU_COLLATION_BASE_NAME, CollationData.ICU_DATA_CLASS_LOADER /* ICUResourceBundle.ICU_DATA_CLASS_LOADER */);
            }
            return shim.GetAvailableULocales();
        }

        /**
         * The list of keywords for this service.  This must be kept in sync with
         * the resource data.
         * @since ICU 3.0
         */
        private static readonly string[] KEYWORDS = { "collation" };

        /**
         * The resource name for this service.  Note that this is not the same as
         * the keyword for this service.
         * @since ICU 3.0
         */
        private static readonly string RESOURCE = "collations";

        /**
         * The resource bundle base name for this service.
         * *since ICU 3.0
         */

        private static readonly string BASE = ICUData.ICU_COLLATION_BASE_NAME;

        /**
         * {@icu} Returns an array of all possible keywords that are relevant to
         * collation. At this point, the only recognized keyword for this
         * service is "collation".
         * @return an array of valid collation keywords.
         * @see #getKeywordValues
         * @stable ICU 3.0
         */
        public static IList<string> Keywords // ICU4N specific - changed to IList, since returning Arrays from properties is not recommended by MSDN
        {
            get { return KEYWORDS; }
        }

        /**
         * {@icu} Given a keyword, returns an array of all values for
         * that keyword that are currently in use.
         * @param keyword one of the keywords returned by getKeywords.
         * @see #getKeywords
         * @stable ICU 3.0
         */
        public static string[] GetKeywordValues(string keyword)
        {
            if (!keyword.Equals(KEYWORDS[0]))
            {
                throw new ArgumentException("Invalid keyword: " + keyword);
            }
            return ICUResourceBundle.GetKeywordValues(BASE, RESOURCE, CollationData.ICU_DATA_CLASS_LOADER);
        }

        /**
         * {@icu} Given a key and a locale, returns an array of string values in a preferred
         * order that would make a difference. These are all and only those values where
         * the open (creation) of the service with the locale formed from the input locale
         * plus input keyword and that value has different behavior than creation with the
         * input locale alone.
         * @param key           one of the keys supported by this service.  For now, only
         *                      "collation" is supported.
         * @param locale        the locale
         * @param commonlyUsed  if set to true it will return only commonly used values
         *                      with the given locale in preferred order.  Otherwise,
         *                      it will return all the available values for the locale.
         * @return an array of string values for the given key and the locale.
         * @stable ICU 4.2
         */
        public static string[] GetKeywordValuesForLocale(string key, ULocale locale,
                                                               bool commonlyUsed)
        {
            // Note: The parameter commonlyUsed is not used.
            // The switch is in the method signature for consistency
            // with other locale services.

            // Read available collation values from collation bundles.
            ICUResourceBundle bundle = (ICUResourceBundle)
                    UResourceBundle.GetBundleInstance(
                            ICUData.ICU_COLLATION_BASE_NAME, locale, CollationData.ICU_DATA_CLASS_LOADER);
            KeywordsSink sink = new KeywordsSink();
            bundle.GetAllItemsWithFallback("collations", sink);
            return sink.values.ToArray();
        }

        private sealed class KeywordsSink : UResource.Sink
        {
            internal LinkedList<string> values = new LinkedList<string>();
            internal bool hasDefault = false;

            public override void Put(UResource.Key key, UResource.Value value, bool noFallback)
            {
                UResource.ITable collations = value.GetTable();
                for (int i = 0; collations.GetKeyAndValue(i, key, value); ++i)
                {
                    int type = value.Type;
                    if (type == UResourceBundle.STRING)
                    {
                        if (!hasDefault && key.ContentEquals("default"))
                        {
                            string defcoll = value.GetString();
                            if (!string.IsNullOrEmpty(defcoll))
                            {
                                values.Remove(defcoll);
                                values.AddFirst(defcoll);
                                hasDefault = true;
                            }
                        }
                    }
                    else if (type == UResourceBundle.TABLE && !key.StartsWith("private-"))
                    {
                        string collkey = key.ToString();
                        if (!values.Contains(collkey))
                        {
                            values.AddLast(collkey);
                        }
                    }
                }
            }
        }

        /**
         * {@icu} Returns the functionally equivalent locale for the given
         * requested locale, with respect to given keyword, for the
         * collation service.  If two locales return the same result, then
         * collators instantiated for these locales will behave
         * equivalently.  The converse is not always true; two collators
         * may in fact be equivalent, but return different results, due to
         * internal details.  The return result has no other meaning than
         * that stated above, and implies nothing as to the relationship
         * between the two locales.  This is intended for use by
         * applications who wish to cache collators, or otherwise reuse
         * collators when possible.  The functional equivalent may change
         * over time.  For more information, please see the <a
         * href="http://userguide.icu-project.org/locale#TOC-Locales-and-Services">
         * Locales and Services</a> section of the ICU User Guide.
         * @param keyword a particular keyword as enumerated by
         * getKeywords.
         * @param locID The requested locale
         * @param isAvailable If non-null, isAvailable[0] will receive and
         * output boolean that indicates whether the requested locale was
         * 'available' to the collation service. If non-null, isAvailable
         * must have length &gt;= 1.
         * @return the locale
         * @stable ICU 3.0
         */
        public static ULocale GetFunctionalEquivalent(string keyword,
                                                            ULocale locID,
                                                            bool[] isAvailable)
        {
            return ICUResourceBundle.GetFunctionalEquivalent(BASE, CollationData.ICU_DATA_CLASS_LOADER /* ICUResourceBundle.ICU_DATA_CLASS_LOADER */, RESOURCE,
                                                             keyword, locID, isAvailable, true);
        }

        /**
         * {@icu} Returns the functionally equivalent locale for the given
         * requested locale, with respect to given keyword, for the
         * collation service.
         * @param keyword a particular keyword as enumerated by
         * getKeywords.
         * @param locID The requested locale
         * @return the locale
         * @see #getFunctionalEquivalent(String,ULocale,boolean[])
         * @stable ICU 3.0
         */
        public static ULocale GetFunctionalEquivalent(string keyword,
                                                            ULocale locID)
        {
            return GetFunctionalEquivalent(keyword, locID, null);
        }

        /**
         * {@icu} Returns the name of the collator for the objectLocale, localized for the
         * displayLocale.
         * @param objectLocale the locale of the collator
         * @param displayLocale the locale for the collator's display name
         * @return the display name
         * @stable ICU 2.6
         */
        public static string GetDisplayName(CultureInfo objectLocale, CultureInfo displayLocale)
        {
            return GetShim().GetDisplayName(ULocale.ForLocale(objectLocale),
                                            ULocale.ForLocale(displayLocale));
        }

        /**
         * {@icu} Returns the name of the collator for the objectLocale, localized for the
         * displayLocale.
         * @param objectLocale the locale of the collator
         * @param displayLocale the locale for the collator's display name
         * @return the display name
         * @stable ICU 3.2
         */
        public static string GetDisplayName(ULocale objectLocale, ULocale displayLocale)
        {
            return GetShim().GetDisplayName(objectLocale, displayLocale);
        }

        /**
         * {@icu} Returns the name of the collator for the objectLocale, localized for the
         * default <code>DISPLAY</code> locale.
         * @param objectLocale the locale of the collator
         * @return the display name
         * @see com.ibm.icu.util.ULocale.Category#DISPLAY
         * @stable ICU 2.6
         */
        public static string GetDisplayName(CultureInfo objectLocale)
        {
            return GetShim().GetDisplayName(ULocale.ForLocale(objectLocale), ULocale.GetDefault(Category.DISPLAY));
        }

        /**
         * {@icu} Returns the name of the collator for the objectLocale, localized for the
         * default <code>DISPLAY</code> locale.
         * @param objectLocale the locale of the collator
         * @return the display name
         * @see com.ibm.icu.util.ULocale.Category#DISPLAY
         * @stable ICU 3.2
         */
        public static string GetDisplayName(ULocale objectLocale)
        {
            return GetShim().GetDisplayName(objectLocale, ULocale.GetDefault(Category.DISPLAY));
        }

        ///**
        // * Returns this Collator's strength attribute. The strength attribute
        // * determines the minimum level of difference considered significant.
        // * {@icunote} This can return QUATERNARY strength, which is not supported by the
        // * JDK version.
        // * <p>
        // * See the Collator class description for more details.
        // * <p>The base class method always returns {@link #TERTIARY}.
        // * Subclasses should override it if appropriate.
        // *
        // * @return this Collator's current strength attribute.
        // * @see #setStrength
        // * @see #PRIMARY
        // * @see #SECONDARY
        // * @see #TERTIARY
        // * @see #QUATERNARY
        // * @see #IDENTICAL
        // * @stable ICU 2.8
        // */
        //public int getStrength()
        //{
        //    return TERTIARY;
        //}

        ///**
        // * Returns the decomposition mode of this Collator. The decomposition mode
        // * determines how Unicode composed characters are handled.
        // * <p>
        // * See the Collator class description for more details.
        // * <p>The base class method always returns {@link #NO_DECOMPOSITION}.
        // * Subclasses should override it if appropriate.
        // *
        // * @return the decomposition mode
        // * @see #setDecomposition
        // * @see #NO_DECOMPOSITION
        // * @see #CANONICAL_DECOMPOSITION
        // * @stable ICU 2.8
        // */
        //public int getDecomposition()
        //{
        //    return NO_DECOMPOSITION;
        //}

        // public other methods -------------------------------------------------

        /**
         * Compares the equality of two text Strings using
         * this Collator's rules, strength and decomposition mode.  Convenience method.
         * @param source the source string to be compared.
         * @param target the target string to be compared.
         * @return true if the strings are equal according to the collation
         *         rules, otherwise false.
         * @see #compare
         * @throws NullPointerException thrown if either arguments is null.
         * @stable ICU 2.8
         */
        public virtual bool Equals(string source, string target)
        {
            return (Compare(source, target) == 0);
        }

        /**
         * {@icu} Returns a UnicodeSet that contains all the characters and sequences tailored
         * in this collator.
         * @return a pointer to a UnicodeSet object containing all the
         *         code points and sequences that may sort differently than
         *         in the root collator.
         * @stable ICU 2.4
         */
        public virtual UnicodeSet GetTailoredSet()
        {
            return new UnicodeSet(0, 0x10FFFF);
        }

        /**
         * Compares the source text String to the target text String according to
         * this Collator's rules, strength and decomposition mode.
         * Returns an integer less than,
         * equal to or greater than zero depending on whether the source String is
         * less than, equal to or greater than the target String. See the Collator
         * class description for an example of use.
         *
         * @param source the source String.
         * @param target the target String.
         * @return Returns an integer value. Value is less than zero if source is
         *         less than target, value is zero if source and target are equal,
         *         value is greater than zero if source is greater than target.
         * @see CollationKey
         * @see #getCollationKey
         * @throws NullPointerException thrown if either argument is null.
         * @stable ICU 2.8
         */
        public abstract int Compare(string source, string target);

        /**
         * Compares the source Object to the target Object.
         *
         * @param source the source Object.
         * @param target the target Object.
         * @return Returns an integer value. Value is less than zero if source is
         *         less than target, value is zero if source and target are equal,
         *         value is greater than zero if source is greater than target.
         * @throws ClassCastException thrown if either arguments cannot be cast to CharSequence.
         * @stable ICU 4.2
         */
        public virtual int Compare(object source, object target)
        {
            return Compare(ObjectToString(source), ObjectToString(target));

            //return DoCompare((ICharSequence)source, (ICharSequence)target); 
        }

        private string ObjectToString(object obj)
        {
            if (obj is string)
            {
                return (string)obj;
            }
            else if (obj is char[])
            {
                return new string((char[])obj);
            }
            else
            {
                return obj.ToString();
            }
        }

        /**
         * Compares two CharSequences.
         * The base class just calls compare(left.toString(), right.toString()).
         * Subclasses should instead implement this method and have the String API call this method.
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        protected virtual int DoCompare(ICharSequence left, ICharSequence right)
        {
            return Compare(left.ToString(), right.ToString());
        }

        /**
         * <p>
         * Transforms the String into a CollationKey suitable for efficient
         * repeated comparison.  The resulting key depends on the collator's
         * rules, strength and decomposition mode.
         *
         * <p>Note that collation keys are often less efficient than simply doing comparison.
         * For more details, see the ICU User Guide.
         *
         * <p>See the CollationKey class documentation for more information.
         * @param source the string to be transformed into a CollationKey.
         * @return the CollationKey for the given String based on this Collator's
         *         collation rules. If the source String is null, a null
         *         CollationKey is returned.
         * @see CollationKey
         * @see #compare(String, String)
         * @see #getRawCollationKey
         * @stable ICU 2.8
         */
        public abstract CollationKey GetCollationKey(string source);

        /**
         * {@icu} Returns the simpler form of a CollationKey for the String source following
         * the rules of this Collator and stores the result into the user provided argument
         * key.  If key has a internal byte array of length that's too small for the result,
         * the internal byte array will be grown to the exact required size.
         *
         * <p>Note that collation keys are often less efficient than simply doing comparison.
         * For more details, see the ICU User Guide.
         *
         * @param source the text String to be transformed into a RawCollationKey
         * @return If key is null, a new instance of RawCollationKey will be
         *         created and returned, otherwise the user provided key will be
         *         returned.
         * @see #compare(String, String)
         * @see #getCollationKey
         * @see RawCollationKey
         * @stable ICU 2.8
         */
        public abstract RawCollationKey GetRawCollationKey(string source,
                                                           RawCollationKey key);

        ///**
        // * {@icu} Sets the variable top to the top of the specified reordering group.
        // * The variable top determines the highest-sorting character
        // * which is affected by the alternate handling behavior.
        // * If that attribute is set to UCOL_NON_IGNORABLE, then the variable top has no effect.
        // *
        // * <p>The base class implementation throws an UnsupportedOperationException.
        // * @param group one of Collator.ReorderCodes.SPACE, Collator.ReorderCodes.PUNCTUATION,
        // *              Collator.ReorderCodes.SYMBOL, Collator.ReorderCodes.CURRENCY;
        // *              or Collator.ReorderCodes.DEFAULT to restore the default max variable group
        // * @return this
        // * @see #getMaxVariable
        // * @stable ICU 53
        // */
        //public Collator setMaxVariable(int group)
        //{
        //    throw new UnsupportedOperationException("Needs to be implemented by the subclass.");
        //}

        ///**
        // * {@icu} Returns the maximum reordering group whose characters are affected by
        // * the alternate handling behavior.
        // *
        // * <p>The base class implementation returns Collator.ReorderCodes.PUNCTUATION.
        // * @return the maximum variable reordering group.
        // * @see #setMaxVariable
        // * @stable ICU 53
        // */
        //public int getMaxVariable()
        //{
        //    return Collator.ReorderCodes.PUNCTUATION;
        //}

        public virtual int MaxVariable
        {
            get { return ReorderCodes.Punctuation; }
            set { throw new NotSupportedException("Needs to be implemented by the subclass."); }
        }

        /**
         * {@icu} Sets the variable top to the primary weight of the specified string.
         *
         * <p>Beginning with ICU 53, the variable top is pinned to
         * the top of one of the supported reordering groups,
         * and it must not be beyond the last of those groups.
         * See {@link #setMaxVariable(int)}.
         *
         * @param varTop one or more (if contraction) characters to which the
         *               variable top should be set
         * @return variable top primary weight
         * @exception IllegalArgumentException
         *                is thrown if varTop argument is not a valid variable top element. A variable top element is
         *                invalid when
         *                <ul>
         *                <li>it is a contraction that does not exist in the Collation order
         *                <li>the variable top is beyond
         *                    the last reordering group supported by setMaxVariable()
         *                <li>when the varTop argument is null or zero in length.
         *                </ul>
         * @see #getVariableTop
         * @see RuleBasedCollator#setAlternateHandlingShifted
         * @deprecated ICU 53 Call {@link #setMaxVariable(int)} instead.
         */
        [Obsolete("ICU 53 Set MaxVariable instead.")]
        public abstract int SetVariableTop(string varTop);

        ///**
        // * {@icu} Gets the variable top value of a Collator.
        // *
        // * @return the variable top primary weight
        // * @see #getMaxVariable
        // * @stable ICU 2.6
        // */
        //public abstract int getVariableTop();



        //    /**
        //     * {@icu} Sets the variable top to the specified primary weight.
        //     *
        //     * <p>Beginning with ICU 53, the variable top is pinned to
        //     * the top of one of the supported reordering groups,
        //     * and it must not be beyond the last of those groups.
        //     * See {@link #setMaxVariable(int)}.
        //     *
        //     * @param varTop primary weight, as returned by setVariableTop or getVariableTop
        //     * @see #getVariableTop
        //     * @see #setVariableTop(String)
        //     * @deprecated ICU 53 Call setMaxVariable() instead.
        //     */
        //    @Deprecated
        //public abstract void setVariableTop(int varTop);

        [Obsolete("ICU 53 Set MaxVariable instead.")]
        public abstract int VariableTop { get; set; }



        /**
         * {@icu} Returns the version of this collator object.
         * @return the version object associated with this collator
         * @stable ICU 2.8
         */
        public abstract VersionInfo GetVersion();

        /**
         * {@icu} Returns the UCA version of this collator object.
         * @return the version object associated with this collator
         * @stable ICU 2.8
         */
        public abstract VersionInfo GetUCAVersion();

        /**
         * Retrieves the reordering codes for this collator.
         * These reordering codes are a combination of UScript codes and ReorderCodes.
         * @return a copy of the reordering codes for this collator;
         * if none are set then returns an empty array
         * @see #setReorderCodes
         * @see #getEquivalentReorderCodes
         * @see Collator.ReorderCodes
         * @see UScript
         * @stable ICU 4.8
         */
        public virtual int[] GetReorderCodes()
        {
            throw new NotSupportedException("Needs to be implemented by the subclass.");
        }

        /**
         * Retrieves all the reorder codes that are grouped with the given reorder code. Some reorder
         * codes are grouped and must reorder together.
         * Beginning with ICU 55, scripts only reorder together if they are primary-equal,
         * for example Hiragana and Katakana.
         *
         * @param reorderCode The reorder code to determine equivalence for.
         * @return the set of all reorder codes in the same group as the given reorder code.
         * @see #setReorderCodes
         * @see #getReorderCodes
         * @see Collator.ReorderCodes
         * @see UScript
         * @stable ICU 4.8
         */
        public static int[] GetEquivalentReorderCodes(int reorderCode)
        {
            CollationData baseData = CollationRoot.Data;
            return baseData.GetEquivalentScripts(reorderCode);
        }


        // Freezable interface implementation -------------------------------------------------

        /**
         * Determines whether the object has been frozen or not.
         *
         * <p>An unfrozen Collator is mutable and not thread-safe.
         * A frozen Collator is immutable and thread-safe.
         *
         * @stable ICU 4.8
         */
        public virtual bool IsFrozen
        {
            get { return false; }
        }

        /**
         * Freezes the collator.
         * @return the collator itself.
         * @stable ICU 4.8
         */
        public virtual Collator Freeze()
        {
            throw new NotSupportedException("Needs to be implemented by the subclass.");
        }

        /**
         * Provides for the clone operation. Any clone is initially unfrozen.
         * @stable ICU 4.8
         */
        public virtual Collator CloneAsThawed()
        {
            throw new NotSupportedException("Needs to be implemented by the subclass.");
        }

        /**
         * Empty default constructor to make javadocs happy
         * @stable ICU 2.4
         */
        protected Collator()
        {
        }

        private static readonly bool DEBUG = ICUDebug.Enabled("collator");

        // -------- BEGIN ULocale boilerplate --------

        /**
         * {@icu} Returns the locale that was used to create this object, or null.
         * This may may differ from the locale requested at the time of
         * this object's creation.  For example, if an object is created
         * for locale <tt>en_US_CALIFORNIA</tt>, the actual data may be
         * drawn from <tt>en</tt> (the <i>actual</i> locale), and
         * <tt>en_US</tt> may be the most specific locale that exists (the
         * <i>valid</i> locale).
         *
         * <p>Note: This method will be implemented in ICU 3.0; ICU 2.8
         * contains a partial preview implementation.  The * <i>actual</i>
         * locale is returned correctly, but the <i>valid</i> locale is
         * not, in most cases.
         *
         * <p>The base class method always returns {@link ULocale#ROOT}.
         * Subclasses should override it if appropriate.
         *
         * @param type type of information requested, either {@link
         * com.ibm.icu.util.ULocale#VALID_LOCALE} or {@link
         * com.ibm.icu.util.ULocale#ACTUAL_LOCALE}.
         * @return the information specified by <i>type</i>, or null if
         * this object was not constructed from locale data.
         * @see com.ibm.icu.util.ULocale
         * @see com.ibm.icu.util.ULocale#VALID_LOCALE
         * @see com.ibm.icu.util.ULocale#ACTUAL_LOCALE
         * @draft ICU 2.8 (retain)
         * @provisional This API might change or be removed in a future release.
         */
        public virtual ULocale GetLocale(ULocale.Type type)
        {
            return ULocale.ROOT;
        }

        /**
         * Set information about the locales that were used to create this
         * object.  If the object was not constructed from locale data,
         * both arguments should be set to null.  Otherwise, neither
         * should be null.  The actual locale must be at the same level or
         * less specific than the valid locale.  This method is intended
         * for use by factories or other entities that create objects of
         * this class.
         *
         * <p>The base class method does nothing. Subclasses should override it if appropriate.
         *
         * @param valid the most specific locale containing any resource
         * data, or null
         * @param actual the locale containing data used to construct this
         * object, or null
         * @see com.ibm.icu.util.ULocale
         * @see com.ibm.icu.util.ULocale#VALID_LOCALE
         * @see com.ibm.icu.util.ULocale#ACTUAL_LOCALE
         */
        internal virtual void SetLocale(ULocale valid, ULocale actual) { }

        // -------- END ULocale boilerplate --------
    }
}
