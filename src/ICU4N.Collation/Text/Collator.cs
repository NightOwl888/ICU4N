using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Impl.Coll;
using ICU4N.Support;
using ICU4N.Util;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;

namespace ICU4N.Text
{
    /// <summary>
    /// Use this to set the strength of a <see cref="Collator"/> object.
    /// This is also used to determine the strength of sort keys
    /// generated from <see cref="Collator"/> objects
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
        
        //Default = -1,

        /// <summary>
        /// Strongest collator strength value. Typically used to denote differences
        /// between base characters. See <see cref="Collator"/> documentation for more explanation.
        /// <para/>
        /// Base letter represents a primary difference.  Set comparison
        /// level to Primary to ignore secondary and tertiary differences.
        /// Example of primary difference, "abc" &lt; "abd"
        /// </summary>
        /// <seealso cref="Collator.Strength"/>
        /// <stable>ICU 2.8</stable>
        Primary = 0,

        /// <summary>
        /// Second level collator strength value.
        /// Accents in the characters are considered secondary differences.
        /// Other differences between letters can also be considered secondary
        /// differences, depending on the language.
        /// See <see cref="Collator"/> documentation for more explanation.
        /// <para/>
        /// Diacritical differences on the same base letter represent a secondary
        /// difference.  Set comparison level to Secondary to ignore tertiary
        /// differences.
        /// Example of secondary difference, "a&#x308;" >> "a".
        /// </summary>
        /// <seealso cref="Collator.Strength"/>
        /// <stable>ICU 2.8</stable>
        Secondary = 1,

        /// <summary>
        /// Third level collator strength value.
        /// Upper and lower case differences in characters are distinguished at this
        /// strength level. In addition, a variant of a letter differs from the base
        /// form on the tertiary level.
        /// See <see cref="Collator"/> documentation for more explanation.
        /// <para/>
        /// Set comparison level to Tertiary to include
        /// all comparison differences.
        /// Example of tertiary difference, "abc" &lt;&lt;&lt; "ABC".
        /// </summary>
        /// <seealso cref="Collator.Strength"/>
        /// <stable>ICU 2.8</stable>
        Tertiary = 2,

        /// <summary>
        /// <icu/>Fourth level collator strength value.
        /// When punctuation is ignored
        /// (see <a href="http://userguide.icu-project.org/collation/concepts#TOC-Ignoring-Punctuation">Ignoring Punctuation in the User Guide</a>) 
        /// at <see cref="Primary"/> to <see cref="Tertiary"/>
        /// strength, an additional strength level can
        /// be used to distinguish words with and without punctuation.
        /// See <see cref="Collator"/> documentation for more explanation.
        /// <para/>
        /// Quaternary level is usually only affected by the number of
        /// non-ignorable code points in the string.
        /// </summary>
        /// <seealso cref="Collator.Strength"/>
        /// <stable>ICU 2.8</stable>
        Quaternary = 3,

        /// <summary>
        /// Smallest <see cref="Collator"/> strength value. When all other strengths are equal,
        /// the IDENTICAL strength is used as a tiebreaker. The Unicode code point
        /// values of the NFD form of each string are compared, just in case there
        /// is no difference.
        /// See <see cref="Collator"/> documentation for more explanation.
        /// <para/>
        /// Note this value is different from JDK's
        /// <para/>
        /// Two characters are considered "identical" when they have the same
        /// unicode spellings.
        /// For example, "a&#x308;" == "a&#x308;".
        /// </summary>
        /// <remarks>Identical strength is rarely useful, as it amounts to
        /// codepoints of the NFD form of the string</remarks>
        /// <seealso cref="Collator.Strength"/>
        /// <stable>ICU 2.8</stable>
        Identical = 15
    }

    public enum NormalizationMode
    {
        //FullDecomposition = CollationStrength.Identical, // NOT SUPPORTED by ICU (was only for compatibility with Java APIs)

        /// <summary>
        /// Decomposition mode value. With <see cref="NoDecomposition"/> set, Strings
        /// will not be decomposed for collation. This is the default
        /// decomposition setting unless otherwise specified by the locale
        /// used to create the <see cref="Collator"/>.
        /// <para/>
        /// <strong>Note</strong> this value is different from the JDK's.
        /// </summary>
        /// <seealso cref="CanonicalDecomposition"/>
        /// <seealso cref="Collator.Decomposition"/>
        /// <stable>ICU 2.8</stable>
        NoDecomposition = 16,

        /// <summary>
        /// Decomposition mode value. With <see cref="CanonicalDecomposition"/> set,
        /// characters that are canonical variants according to the Unicode standard
        /// will be decomposed for collation.
        /// <para/>
        /// <see cref="CanonicalDecomposition"/> corresponds to Normalization Form D as
        /// described in <a href="http://www.unicode.org/unicode/reports/tr15/">Unicode Technical Report #15</a>.
        /// </summary>
        /// <seealso cref="NoDecomposition"/>
        /// <seealso cref="Collator.Decomposition"/>
        /// <stable>ICU 2.8</stable>
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

    /// <summary>
    /// A factory used with <see cref="Collator.RegisterFactory(CollatorFactory)"/> to register multiple collators and provide
    /// display names for them.  If standard locale display names are sufficient,
    /// Collator instances may be registered instead.
    /// <para/>
    /// <b>Note:</b> as of ICU4N 3.2, the default API for <see cref="CollatorFactory"/> uses
    /// <see cref="UCultureInfo"/> instead of <see cref="CultureInfo"/>.  Instead of overriding <see cref="CreateCollator(CultureInfo)"/>,
    /// new implementations should override <see cref="CreateCollator(UCultureInfo)"/>. Note that
    /// one of these two methods <b>MUST</b> be overridden or else an infinite
    /// loop will occur.
    /// </summary>
    /// <stable>ICU 2.6</stable>
    public abstract class CollatorFactory
    {
        /// <summary>
        /// Return true if this factory will be visible.  Default is true.
        /// If not visible, the locales supported by this factory will not
        /// be listed by <see cref="Collator.GetAvailableLocales()"/>.
        /// <para/>
        /// true if this factory is visible.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public virtual bool Visible
        {
            get { return true; }
        }

        /// <summary>
        /// Return an instance of the appropriate collator.  If the locale
        /// is not supported, return null.
        /// <para/>
        /// <b>Note:</b> as of ICU4N 3.2, implementations should override
        /// this method instead of <see cref="CreateCollator(CultureInfo)"/>.
        /// </summary>
        /// <param name="loc">the locale for which this collator is to be created.</param>
        /// <returns>the newly created collator.</returns>
        /// <stable>ICU 3.2</stable>
        public virtual Collator CreateCollator(UCultureInfo loc)
        {
            return CreateCollator(loc.ToCultureInfo());
        }

        /// <summary>
        /// Return an instance of the appropriate collator.  If the locale
        /// is not supported, return null.
        /// <para/>
        /// <b>Note:</b> as of ICU4J 3.2, implementations should override
        /// <see cref="CreateCollator(UCultureInfo)"/> instead of this method, and inherit this
        /// method's implementation.  This method is no longer abstract
        /// and instead delegates to <see cref="CreateCollator(UCultureInfo)"/>.
        /// </summary>
        /// <param name="loc">the locale for which this collator is to be created.</param>
        /// <returns>the newly created collator.</returns>
        /// <stable>ICU 2.6</stable>
        public virtual Collator CreateCollator(CultureInfo loc)
        {
            return CreateCollator(loc.ToUCultureInfo());
        }

        /// <summary>
        /// Return the name of the collator for the <paramref name="objectLocale"/>, localized for the <paramref name="displayLocale"/>.
        /// If <paramref name="objectLocale"/> is not visible or not defined by the factory, return null.
        /// </summary>
        /// <param name="objectLocale">the locale identifying the collator</param>
        /// <param name="displayLocale">the locale for which the display name of the collator should be localized</param>
        /// <returns>the display name</returns>
        /// <stable>ICU 2.6</stable>
        public virtual string GetDisplayName(CultureInfo objectLocale, CultureInfo displayLocale)
        {
            return Collator.GetDisplayName(objectLocale.ToUCultureInfo(), displayLocale.ToUCultureInfo());
        }

        /// <summary>
        /// Return the name of the collator for the <paramref name="objectLocale"/>, localized for the <paramref name="displayLocale"/>.
        /// If <paramref name="objectLocale"/> is not visible or not defined by the factory, return null.
        /// </summary>
        /// <param name="objectLocale">the locale identifying the collator</param>
        /// <param name="displayLocale">the locale for which the display name of the collator should be localized</param>
        /// <returns>the display name</returns>
        /// <stable>ICU 3.2</stable>
        public virtual string GetDisplayName(UCultureInfo objectLocale, UCultureInfo displayLocale)
        {
            if (Visible)
            {
                ICollection<string> supported = GetSupportedLocaleIDs();
                string name = objectLocale.Name;
                if (supported.Contains(name))
                {
                    return objectLocale.GetDisplayName(displayLocale);
                }
            }
            return null;
        }

        /// <summary>
        /// Return an unmodifiable collection of the locale names directly
        /// supported by this factory.
        /// </summary>
        /// <returns>the set of supported locale IDs.</returns>
        /// <stable>ICU 2.6</stable>
        public abstract ICollection<string> GetSupportedLocaleIDs(); // ICU4N TODO: API - see if it is possible to return IReadOnlyCollection<T> here

        /// <summary>
        /// Empty default constructor.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        protected CollatorFactory()
        {
        }
    }

    /// <summary>
    /// Collator performs locale-sensitive string comparison. A concrete
    /// subclass, <see cref="RuleBasedCollator"/>, allows customization of the collation
    /// ordering by the use of rule sets.
    /// </summary>
    /// <remarks>
    /// A Collator is thread-safe only when frozen. See <see cref="IsFrozen"/> and <see cref="IFreezable{T}"/>.
    /// <para/>
    /// Following the <a href="http://www.unicode.org">Unicode Consortium</a>'s specifications for the
    /// <a href="http://www.unicode.org/unicode/reports/tr10/">Unicode Collation Algorithm (UCA)</a>, 
    /// there are 5 different levels of strength used in comparisons:
    /// <list type="table">
    ///     <item>
    ///         <term><see cref="CollationStrength.Primary"/> strength:</term>
    ///         <description>
    ///             Typically, this is used to denote differences between
    ///             base characters (for example, "a" &lt; "b").
    ///             It is the strongest difference. For example, dictionaries are divided
    ///             into different sections by base character.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="CollationStrength.Secondary"/> strength:</term>
    ///         <description>
    ///             Accents in the characters are considered secondary
    ///             differences (for example, "as" &lt; "&amp;agrave;s" &lt; "at"). Other
    ///             differences between letters can also be considered secondary differences, depending
    ///             on the language. A secondary difference is ignored when there is a
    ///             primary difference anywhere in the strings.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="CollationStrength.Tertiary"/> strenth:</term>
    ///         <description>
    ///             Upper and lower case differences in characters are
    ///             distinguished at tertiary strength (for example, "ao" &lt; "Ao" &lt;
    ///             "a&amp;ograve;"). In addition, a variant of a letter differs from the base
    ///             form on the tertiary strength (such as "A" and "Ⓐ"). Another
    ///             example is the difference between large and small Kana. A tertiary difference is ignored
    ///             when there is a primary or secondary difference anywhere in the strings.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="CollationStrength.Quaternary"/> strenth:</term>
    ///         <description>
    ///             When punctuation is ignored
    ///             (see <a href="http://userguide.icu-project.org/collation/concepts#TOC-Ignoring-Punctuation">
    ///             Ignoring Punctuations in the User Guide</a>) at <see cref="CollationStrength.Primary"/> to 
    ///             <see cref="CollationStrength.Tertiary"/> strength, an additional strength level can
    ///             be used to distinguish words with and without punctuation (for example,
    ///             "ab" &lt; "a-b" &lt; "aB").
    ///             This difference is ignored when there is a <see cref="CollationStrength.Primary"/>, 
    ///             <see cref="CollationStrength.Secondary"/> or <see cref="CollationStrength.Tertiary"/>
    ///             difference. The <see cref="CollationStrength.Quaternary"/> strength should only be used if ignoring
    ///             punctuation is required.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="CollationStrength.Identical"/> strength:</term>
    ///         <description>
    ///             When all other strengths are equal, the <see cref="CollationStrength.Identical"/> strength is used as a
    ///             tiebreaker. The Unicode code point values of the NFD form of each string
    ///             are compared, just in case there is no difference.
    ///             For example, Hebrew cantellation marks are only distinguished at this
    ///             strength. This strength should be used sparingly, as only code point
    ///             value differences between two strings is an extremely rare occurrence.
    ///             Using this strength substantially decreases the performance for both
    ///             comparison and collation key generation APIs. This strength also
    ///             increases the size of the collation key.
    ///         </description>
    ///     </item>
    /// </list>
    /// <para/>
    /// Unlike the JDK, ICU4N's Collator deals only with 2 decomposition modes,
    /// the canonical decomposition mode and one that does not use any decomposition.
    /// The compatibility decomposition mode, java.text.Collator.FULL_DECOMPOSITION
    /// is not supported here. If the canonical
    /// decomposition mode is set, the Collator handles un-normalized text properly,
    /// producing the same results as if the text were normalized in NFD. If
    /// canonical decomposition is turned off, it is the user's responsibility to
    /// ensure that all text is already in the appropriate form before performing
    /// a comparison or before getting a <see cref="CollationKey"/>.
    /// <para/>
    /// For more information about the collation service see the
    /// <a href="http://userguide.icu-project.org/collation">User Guide</a>.
    /// <para/>
    /// Examples of use
    /// <code>
    ///     // Get the Collator for US English and set its strength to CollationStrength.Primary
    ///     Collator usCollator = Collator.GetInstance(new CultureInfo("en-us"));
    ///     usCollator.Strength = CollationStrength.Primary;
    ///     if (usCollator.Compare("abc", "ABC") == 0)
    ///     {
    ///         Console.WriteLine("Strings are equivalent");
    ///     }
    /// </code>
    /// The following example shows how to compare two strings using the
    /// <see cref="Collator"/> for the default locale.
    /// <code>
    ///     // Compare two strings in the default locale
    ///     Collator myCollator = Collator.GetInstance();
    ///     myCollator.Decomposition = NormalizationMode.NoDecomposition;
    ///     if (myCollator.Compare("&amp;agrave;&#92;u0325", "a&#92;u0325&#768;") != 0)
    ///     {
    ///         Console.WriteLine("&amp;agrave;&#92;u0325 is not equals to a&#92;u0325&#768; without decomposition");
    ///         myCollator.Decomposition = NormalizationMode.CanonicalDecomposition;
    ///         if (myCollator.Compare("&amp;agrave;&#92;u0325", "a&#92;u0325&#768;") != 0)
    ///         {
    ///             Console.WriteLine("Error: &amp;agrave;&#92;u0325 should be equals to a&#92;u0325&#768; with decomposition");
    ///         }
    ///         else
    ///         {
    ///             Console.WriteLine("&amp;agrave;&#92;u0325 is equals to a&#92;u0325&#768; with decomposition");
    ///         }
    ///     }
    ///     else
    ///     {
    ///         Console.WriteLine("Error: &amp;agrave;&#92;u0325 should be not equals to a&#92;u0325&#768; without decomposition");
    ///     }
    /// </code>
    /// </remarks>
    /// <seealso cref="RuleBasedCollator"/>
    /// <seealso cref="CollationKey"/>
    /// <author>Syn Wee Quek</author>
    /// <stable>ICU 2.8</stable>
    public abstract class Collator : IComparer<object>, IFreezable<Collator>
#if FEATURE_CLONEABLE
        , ICloneable
#endif
    {
        //// public data members ---------------------------------------------------

        /// <summary>
        /// Strongest collator strength value. Typically used to denote differences
        /// between base characters. See class documentation for more explanation.
        /// </summary>
        /// <seealso cref="Collator.Strength"/>
        /// <stable>ICU 2.8</stable>
        public const CollationStrength Primary = CollationStrength.Primary; // ICU4N: changed API from public to internal, since we have CollationStrength enum

        /// <summary>
        /// Second level collator strength value.
        /// Accents in the characters are considered secondary differences.
        /// Other differences between letters can also be considered secondary
        /// differences, depending on the language.
        /// See <see cref="Collator"/> documentation for more explanation.
        /// <para/>
        /// Diacritical differences on the same base letter represent a secondary
        /// difference.  Set comparison level to Secondary to ignore tertiary
        /// differences.
        /// Example of secondary difference, "a&#x308;" >> "a".
        /// </summary>
        /// <seealso cref="Collator.Strength"/>
        /// <stable>ICU 2.8</stable>
        public const CollationStrength Secondary = CollationStrength.Secondary; // ICU4N: changed API from public to internal, since we have CollationStrength enum

        /// <summary>
        /// Third level collator strength value.
        /// Upper and lower case differences in characters are distinguished at this
        /// strength level. In addition, a variant of a letter differs from the base
        /// form on the tertiary level.
        /// See <see cref="Collator"/> documentation for more explanation.
        /// <para/>
        /// Set comparison level to Tertiary to include
        /// all comparison differences.
        /// Example of tertiary difference, "abc" &lt;&lt;&lt; "ABC".
        /// </summary>
        /// <seealso cref="Collator.Strength"/>
        /// <stable>ICU 2.8</stable>
        public const CollationStrength Tertiary = CollationStrength.Tertiary; // ICU4N: changed API from public to internal, since we have CollationStrength enum

        /// <summary>
        /// <icu/>Fourth level collator strength value.
        /// When punctuation is ignored
        /// (see <a href="http://userguide.icu-project.org/collation/concepts#TOC-Ignoring-Punctuation">Ignoring Punctuation in the User Guide</a>) 
        /// at <see cref="Primary"/> to <see cref="Tertiary"/>
        /// strength, an additional strength level can
        /// be used to distinguish words with and without punctuation.
        /// See <see cref="Collator"/> documentation for more explanation.
        /// <para/>
        /// Quaternary level is usually only affected by the number of
        /// non-ignorable code points in the string.
        /// </summary>
        /// <seealso cref="Collator.Strength"/>
        /// <stable>ICU 2.8</stable>
        public const CollationStrength Quaternary = CollationStrength.Quaternary; // ICU4N: changed API from public to internal, since we have CollationStrength enum

        /// <summary>
        /// Smallest <see cref="Collator"/> strength value. When all other strengths are equal,
        /// the IDENTICAL strength is used as a tiebreaker. The Unicode code point
        /// values of the NFD form of each string are compared, just in case there
        /// is no difference.
        /// See <see cref="Collator"/> documentation for more explanation.
        /// <para/>
        /// Note this value is different from JDK's
        /// <para/>
        /// Two characters are considered "identical" when they have the same
        /// unicode spellings.
        /// For example, "a&#x308;" == "a&#x308;".
        /// </summary>
        /// <remarks>Identical strength is rarely useful, as it amounts to
        /// codepoints of the NFD form of the string</remarks>
        /// <seealso cref="Collator.Strength"/>
        /// <stable>ICU 2.8</stable>
        public const CollationStrength Identical = CollationStrength.Identical; // ICU4N: changed API from public to internal, since we have CollationStrength enum

        /////**
        //// * {@icunote} This is for backwards compatibility with Java APIs only.  It
        //// * should not be used, IDENTICAL should be used instead.  ICU's
        //// * collation does not support Java's FULL_DECOMPOSITION mode.
        //// * @stable ICU 3.4
        //// */
        ////public const NormalizationMode FULL_DECOMPOSITION = (NormalizationMode)IDENTICAL;

        /// <summary>
        /// Decomposition mode value. With <see cref="NoDecomposition"/> set, Strings
        /// will not be decomposed for collation. This is the default
        /// decomposition setting unless otherwise specified by the locale
        /// used to create the <see cref="Collator"/>.
        /// <para/>
        /// <strong>Note</strong> this value is different from the JDK's.
        /// </summary>
        /// <seealso cref="CanonicalDecomposition"/>
        /// <see cref="Decomposition"/>
        /// <stable>ICU 2.8</stable>
        public const NormalizationMode NoDecomposition = NormalizationMode.NoDecomposition;

        /// <summary>
        /// Decomposition mode value. With CANONICAL_DECOMPOSITION set,
        /// characters that are canonical variants according to the Unicode standard
        /// will be decomposed for collation.
        /// <para/>
        /// CANONICAL_DECOMPOSITION corresponds to Normalization Form D as
        /// described in <a href="http://www.unicode.org/unicode/reports/tr15/">Unicode Technical Report #15</a>.
        /// </summary>
        /// <see cref="NoDecomposition"/>
        /// <see cref="Decomposition"/>
        /// <stable>ICU 2.8</stable>
        public const NormalizationMode CanonicalDecomposition = NormalizationMode.CanonicalDecomposition;

        // ICU4N specific - De-nested ReorderCodes class

        // public methods --------------------------------------------------------

        /// <summary>
        /// Compares the equality of two <see cref="Collator"/> objects. <see cref="Collator"/> objects are equal if they have the same
        /// collation (sorting &amp; searching) behavior.
        /// <para/>
        /// The base class checks for null and for equal types.
        /// Subclasses should override.
        /// </summary>
        /// <param name="obj">The <see cref="Collator"/> to compare to.</param>
        /// <returns><c>true</c> if this <see cref="Collator"/> has exactly the same collation behavior as <paramref name="obj"/>, <c>false</c> otherwise.</returns>
        /// <stable>ICU 2.8</stable>
        public override bool Equals(object obj)
        {
            // Subclasses: Call this method and then add more specific checks.
            return this == obj || (obj != null && GetType() == obj.GetType());
        }

        /// <summary>
        /// Generates a hash code for this <see cref="Collator"/> object.
        /// <para/>
        /// The implementation exists just for consistency with <see cref="Equals(object)"/>
        /// implementation in this class and does not generate a useful hash code.
        /// Subclasses should override this implementation.
        /// </summary>
        /// <returns>a hash code value.</returns>
        /// <stable>ICU 58</stable>
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

        /// <summary>
        /// Gets or sets this <see cref="Collator"/>'s strength attribute. The strength attribute
        /// determines the minimum level of difference considered significant
        /// during comparison.
        /// <para/>
        /// The base class setter does nothing. The base class getter always 
        /// returns <see cref="CollationStrength.Tertiary"/>. Subclasses should override it if appropriate.
        /// <para/>
        /// See the <see cref="Collator"/> class description for an example of use.
        /// </summary>
        /// <seealso cref="CollationStrength.Primary"/>
        /// <seealso cref="CollationStrength.Secondary"/>
        /// <seealso cref="CollationStrength.Tertiary"/>
        /// <seealso cref="CollationStrength.Quaternary"/>
        /// <seealso cref="CollationStrength.Identical"/>
        /// <exception cref="ArgumentException">if the new strength value is not valid.</exception>
        /// <stable>ICU 2.8</stable>
        public virtual CollationStrength Strength
        {
            get { return CollationStrength.Tertiary; }
            set { CheckNotFrozen(); }
        }

        /// <summary>
        /// Internal, used in UnicodeTools.
        /// </summary>
        /// <param name="newStrength"></param>
        /// <returns>this, for chaining</returns>
        [Obsolete("This API is ICU internal only.")]
        internal virtual Collator SetStrength2(CollationStrength newStrength) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            Strength = newStrength;
            return this;
        }

        /// <summary>
        /// Gets or sets the decomposition mode of this <see cref="Collator"/>.  Setting this
        /// decomposition attribute with <see cref="NormalizationMode.CanonicalDecomposition"/> allows the
        /// <see cref="Collator"/> to handle un-normalized text properly, producing the
        /// same results as if the text were normalized. If
        /// <see cref="NormalizationMode.NoDecomposition"/> is set, it is the user's responsibility to
        /// insure that all text is already in the appropriate form before
        /// a comparison or before getting a <see cref="CollationKey"/>. Adjusting
        /// decomposition mode allows the user to select between faster and
        /// more complete collation behavior. The decomposition mode
        /// determines how Unicode composed characters are handled.
        /// See the <see cref="Collator"/> description for more details.
        /// <para/>
        /// Since a great many of the world's languages do not require
        /// text normalization, most locales set <see cref="NormalizationMode.NoDecomposition"/> as the
        /// default decomposition mode.
        /// <para/>
        /// The base class setter does nothing. The base class method always returns <see cref="NormalizationMode.NoDecomposition"/>
        /// Subclasses should override it if appropriate.
        /// </summary>
        /// <seealso cref="NormalizationMode.NoDecomposition"/>
        /// <seealso cref="NormalizationMode.CanonicalDecomposition"/>
        /// <exception cref="ArgumentException">If the given value is not a valid decomposition mode.</exception>
        /// <stable>ICU 2.8</stable>
        public virtual NormalizationMode Decomposition
        {
            get { return NormalizationMode.NoDecomposition; }
            set { CheckNotFrozen(); }
        }

        /// <summary>
        /// Sets the reordering codes for this collator.
        /// Collation reordering allows scripts and some other groups of characters
        /// to be moved relative to each other. This reordering is done on top of
        /// the DUCET/CLDR standard collation order. Reordering can specify groups to be placed
        /// at the start and/or the end of the collation order. These groups are specified using
        /// <see cref="UScript"/> codes and <see cref="ReorderCodes"/> entries.
        /// <para/>
        /// By default, reordering codes specified for the start of the order are placed in the
        /// order given after several special non-script blocks. These special groups of characters
        /// are space, punctuation, symbol, currency, and digit. These special groups are represented with
        /// <see cref="ReorderCodes"/> entries. Script groups can be intermingled with
        /// these special non-script groups if those special groups are explicitly specified in the reordering.
        /// <para/>
        /// The special code <see cref="ReorderCodes.Others"/>
        /// stands for any script that is not explicitly
        /// mentioned in the list of reordering codes given. Anything that is after <see cref="ReorderCodes.Others"/>
        /// will go at the very end of the reordering in the order given.
        /// <para/>
        /// The special reorder code <see cref="ReorderCodes.Default"/>
        /// will reset the reordering for this collator
        /// to the default for this collator. The default reordering may be the DUCET/CLDR order or may be a reordering that
        /// was specified when this collator was created from resource data or from rules. The
        /// DEFAULT code <b>must</b> be the sole code supplied when it is used.
        /// If not, then an <see cref="ArgumentException"/> will be thrown.
        /// <para/>
        /// The special reorder code <see cref="ReorderCodes.None"/>
        /// will remove any reordering for this collator.
        /// The result of setting no reordering will be to have the DUCET/CLDR ordering used. The
        /// <see cref="ReorderCodes.None"/> code <b>must</b> be the sole code supplied when it is used.
        /// </summary>
        /// <param name="order">
        /// The reordering codes to apply to this collator; if this is null or an empty array
        /// then this clears any existing reordering.
        /// </param>
        /// <seealso cref="GetReorderCodes"/>
        /// <seealso cref="GetEquivalentReorderCodes(int)"/>
        /// <seealso cref="ReorderCodes"/>
        /// <seealso cref="UScript"/>
        /// <stable>ICU 4.8</stable>
        public virtual void SetReorderCodes(params int[] order)
        {
            throw new NotSupportedException("Needs to be implemented by the subclass.");
        }

        // public getters --------------------------------------------------------

        /// <summary>
        /// Returns the <see cref="Collator"/> for the current default locale.
        /// The default locale is determined by <see cref="CultureInfo.CurrentCulture"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="Collator"/> for the default locale (for example, en-US) if it
        /// is created successfully. Otherwise if there is no <see cref="Collator"/>
        /// associated with the current locale, the root collator
        /// will be returned.
        /// </returns>
        /// <seealso cref="CultureInfo.CurrentCulture"/>
        /// <seealso cref="GetInstance(CultureInfo)"/>
        /// <stable>ICU 2.8</stable>
        public static Collator GetInstance() // ICU4N TODO: API - make this method's return type generic to eliminate casting with RuleBasedCollator
        {
            return GetInstance(UCultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Clones the collator.
        /// </summary>
        /// <returns>A clone of this collator.</returns>
        /// <stable>ICU 2.6</stable>
        public virtual object Clone()
        {
            return base.MemberwiseClone();
        }

        // begin registry stuff

        // ICU4N specific - de-nested CollatorFactory

        internal abstract class ServiceShim
        {
            internal abstract Collator GetInstance(UCultureInfo l);
            internal abstract object RegisterInstance(Collator c, UCultureInfo l);
            internal abstract object RegisterFactory(CollatorFactory f);
            internal abstract bool Unregister(object k);
            internal abstract CultureInfo[] GetAvailableLocales(); // TODO remove // ICU4N TODO: API - Rename GetCultures(), add CultureTypes enum
            internal abstract UCultureInfo[] GetAvailableULocales(); // ICU4N TODO: API - Rename GetUCultures(), add CultureTypes enum
            internal abstract string GetDisplayName(UCultureInfo ol, UCultureInfo dl);
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
                    ////CLOVER:OFF
                    throw e;
                    ////CLOVER:ON
                }
                catch (Exception e)
                {
                    ////CLOVER:OFF
                    if (DEBUG)
                    {
                        e.PrintStackTrace();
                    }
                    throw new ICUException(e);
                    ////CLOVER:ON
                }
            }
            return shim;
        }

        /// <summary>
        /// Simpler/faster methods for ASCII than ones based on Unicode data.
        /// TODO: There should be code like this somewhere already??
        /// </summary>
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

        /// <summary>
        /// Sets collation attributes according to locale keywords. See
        /// http://www.unicode.org/reports/tr35/tr35-collation.html#Collation_Settings
        /// <para/>
        /// Using "alias" keywords and values where defined:
        /// http://www.unicode.org/reports/tr35/tr35.html#Old_Locale_Extension_Syntax
        /// http://unicode.org/repos/cldr/trunk/common/bcp47/collation.xml
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="coll"></param>
        /// <param name="rbc"></param>
        private static void SetAttributesFromKeywords(UCultureInfo loc, Collator coll, RuleBasedCollator rbc)
        {
            // Check for collation keywords that were already deprecated
            // before any were supported in createInstance() (except for "collation").
            if (loc.Keywords.TryGetValue("colHiraganaQuaternary", out string value) && value != null)
            {
                throw new NotSupportedException("locale keyword kh/colHiraganaQuaternary");
            }
            if (loc.Keywords.TryGetValue("variableTop", out value) && value != null)
            {
                throw new NotSupportedException("locale keyword vt/variableTop");
            }
            // Parse known collation keywords, ignore others.
            if (loc.Keywords.TryGetValue("colStrength", out value) && value != null)
            {
                // Note: Not supporting typo "quarternary" because it was never supported in locale IDs.
                int strength = GetInt32Value("colStrength", value,
                        "primary", "secondary", "tertiary", "quaternary", "identical");
                coll.Strength = strength <= (int)CollationStrength.Quaternary ? (CollationStrength)strength : CollationStrength.Identical;
            }
            if (loc.Keywords.TryGetValue("colBackwards", out value) && value != null)
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
            if (loc.Keywords.TryGetValue("colCaseLevel", out value) && value != null)
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
            if (loc.Keywords.TryGetValue("colCaseFirst", out value) && value != null)
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
            if (loc.Keywords.TryGetValue("colAlternate", out value) && value != null)
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
            if (loc.Keywords.TryGetValue("colNormalization", out value) && value != null)
            {
                coll.Decomposition = GetYesOrNo("colNormalization", value) ?
                        NormalizationMode.CanonicalDecomposition : NormalizationMode.NoDecomposition;
            }
            if (loc.Keywords.TryGetValue("colNumeric", out value) && value != null)
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
            if (loc.Keywords.TryGetValue("colReorder", out value) && value != null)
            {
#pragma warning disable 612, 618
                int[] codes = new int[UScript.CodeLimit + ReorderCodes.Limit - ReorderCodes.First];
#pragma warning restore 612, 618
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
                        code = UChar.GetPropertyValueEnum(UProperty.Script, scriptName);
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
            if (loc.Keywords.TryGetValue("kv", out value) && value != null)
            {
                coll.MaxVariable = GetReorderCode("kv", value);
            }
        }

        /// <summary>
        /// <icu/> Returns the <see cref="Collator"/> for the desired locale.
        /// <para/>
        /// For some languages, multiple collation types are available;
        /// for example, "de@collation=phonebook".
        /// Starting with ICU 54, collation attributes can be specified via locale keywords as well,
        /// in the old locale extension syntax ("el@colCaseFirst=upper")
        /// or in language tag syntax ("el-u-kf-upper").
        /// See <a href="http://userguide.icu-project.org/collation/api">User Guide: Collation API</a>.
        /// </summary>
        /// <param name="locale">the desired locale.</param>
        /// <returns>
        /// <see cref="Collator"/> for the desired locale if it is created successfully.
        /// Otherwise if there is no Collator
        /// associated with the current locale, the root collator will
        /// be returned.
        /// </returns>
        /// <seealso cref="CultureInfo"/>
        /// <seealso cref="ResourceBundle"/>
        /// <seealso cref="GetInstance(CultureInfo)"/>
        /// <seealso cref="GetInstance()"/>
        /// <stable>ICU 3.0</stable>
        public static Collator GetInstance(UCultureInfo locale) // ICU4N TODO: API - make this method's return type generic to eliminate casting with RuleBasedCollator
        {
            // fetching from service cache is faster than instantiation
            if (locale == null)
            {
                locale = UCultureInfo.CurrentCulture;
            }
            Collator coll = GetShim().GetInstance(locale);
            if (!locale.FullName.Equals(locale.Name))
            {  // any keywords?
                SetAttributesFromKeywords(locale, coll,
                        (coll is RuleBasedCollator) ? (RuleBasedCollator)coll : null);
            }
            return coll;
        }

        /// <summary>
        /// Returns the <see cref="Collator"/> for the desired locale.
        /// <para/>
        /// For some languages, multiple collation types are available;
        /// for example, "de-u-co-phonebk".
        /// Starting with ICU 54, collation attributes can be specified via locale keywords as well,
        /// in the old locale extension syntax ("el@colCaseFirst=upper", only with <see cref="UCultureInfo"/>)
        /// or in language tag syntax ("el-u-kf-upper").
        /// See <a href="http://userguide.icu-project.org/collation/api">User Guide: Collation API</a>.
        /// </summary>
        /// <param name="locale">the desired locale.</param>
        /// <returns>
        /// <see cref="Collator"/> for the desired locale if it is created successfully.
        /// Otherwise if there is no <see cref="Collator"/>
        /// associated with the current locale, the root collator will
        /// be returned.
        /// </returns>
        /// <seealso cref="CultureInfo"/>
        /// <seealso cref="ResourceBundle"/>
        /// <seealso cref="GetInstance(CultureInfo)"/>
        /// <seealso cref="GetInstance()"/>
        /// <stable>ICU 2.8</stable>
        public static Collator GetInstance(CultureInfo locale) // ICU4N TODO: API - make this method's return type generic to eliminate casting with RuleBasedCollator
        {
            return GetInstance(locale.ToUCultureInfo());
        }

        /// <summary>
        /// <icu/> Registers a collator as the default collator for the provided locale.  The
        /// collator should not be modified after it is registered.
        /// <para/>
        /// Because ICU may choose to cache Collator objects internally, this must
        /// be called at application startup, prior to any calls to
        /// <see cref="Collator.GetInstance()"/> to avoid undefined behavior.
        /// </summary>
        /// <param name="collator">the collator to register</param>
        /// <param name="locale">the locale for which this is the default collator</param>
        /// <returns>an object that can be used to unregister the registered collator.</returns>
        /// <stable>ICU 3.2</stable>
        public static object RegisterInstance(Collator collator, UCultureInfo locale)
        {
            return GetShim().RegisterInstance(collator, locale);
        }

        /// <summary>
        /// <icu/> Registers a collator factory.
        /// <para/>
        /// Because ICU may choose to cache Collator objects internally, this must
        /// be called at application startup, prior to any calls to
        /// <see cref="Collator.GetInstance()"/> to avoid undefined behavior.
        /// </summary>
        /// <param name="factory">the factory to register</param>
        /// <returns>an object that can be used to unregister the registered factory.</returns>
        /// <stable>ICU 2.6</stable>
        public static object RegisterFactory(CollatorFactory factory)
        {
            return GetShim().RegisterFactory(factory);
        }

        /// <summary>
        /// <icu/> Unregisters a collator previously registered using <see cref="RegisterInstance(Collator, UCultureInfo)"/>.
        /// </summary>
        /// <param name="registryKey">the object previously returned by <see cref="RegisterInstance(Collator, UCultureInfo)"/>.</param>
        /// <returns>true if the collator was successfully unregistered.</returns>
        /// <stable>ICU 2.6</stable>
        public static bool Unregister(object registryKey)
        {
            if (shim == null)
            {
                return false;
            }
            return shim.Unregister(registryKey);
        }

        /// <summary>
        /// Returns the set of locales, as <see cref="CultureInfo"/> objects, for which collators
        /// are installed.  Note that <see cref="CultureInfo"/> objects do not support RFC 3066.
        /// </summary>
        /// <returns>
        /// the list of locales in which collators are installed.
        /// This list includes any that have been registered, in addition to
        /// those that are installed with ICU4N.
        /// </returns>
        /// <stable>ICU 2.4</stable>
        public static CultureInfo[] GetAvailableLocales() // ICU4N TODO: API - rename GetCultures() and add CultureTypes enum
        {
            // TODO make this wrap getAvailableULocales later
            if (shim == null)
            {
                return ICUResourceBundle.GetAvailableLocales(
                    ICUData.IcuCollationBaseName, CollationData.IcuDataAssembly /* ICUResourceBundle.ICU_DATA_CLASS_LOADER */);
            }
            return shim.GetAvailableLocales();
        }

        /// <summary>
        /// <icu/> Returns the set of locales, as <see cref="UCultureInfo"/> objects, for which collators
        /// are installed.  <see cref="UCultureInfo"/> objects support RFC 3066.
        /// </summary>
        /// <returns>
        /// the list of locales in which collators are installed.
        /// This list includes any that have been registered, in addition to
        /// those that are installed with ICU4N.
        /// </returns>
        /// <stable>ICU 3.0</stable>
        public static UCultureInfo[] GetAvailableULocales() // ICU4N TODO: API - Rename GetUCultures() and add CultureTypes enum
        {
            if (shim == null)
            {
                return ICUResourceBundle.GetAvailableUCultures(
                    ICUData.IcuCollationBaseName, CollationData.IcuDataAssembly /* ICUResourceBundle.ICU_DATA_CLASS_LOADER */);
            }
            return shim.GetAvailableULocales();
        }

        /// <summary>
        /// The list of keywords for this service.  This must be kept in sync with
        /// the resource data.
        /// </summary>
        /// <since>ICU 3.0</since>
        private static readonly string[] KEYWORDS = { "collation" }; // ICU4N TODO: API - Rename Keywords ? Possible collision

        /// <summary>
        /// The resource name for this service.  Note that this is not the same as
        /// the keyword for this service.
        /// </summary>
        /// <since>ICU 3.0</since>
        private const string RESOURCE = "collations"; // ICU4N TODO: API - Rename Resource

        /// <summary>
        /// The resource bundle base name for this service.
        /// </summary>
        /// <since>ICU 3.0</since>
        private static readonly string BASE = ICUData.IcuCollationBaseName; // ICU4N TODO: API - Rename Base

        /// <summary>
        /// <icu/> Returns a list of all possible keywords that are relevant to
        /// collation. At this point, the only recognized keyword for this
        /// service is "collation".
        /// </summary>
        /// <seealso cref="GetKeywordValues(string)"/>
        /// <stable>ICU 3.0</stable>
        public static IList<string> Keywords // ICU4N TODO: Change this back to a method GetKeywords() that returns array - it enumerates the value before returning. // ICU4N specific - changed to IList, since returning Arrays from properties is not recommended by MSDN
        {
            get { return KEYWORDS; }
        }

        /// <summary>
        /// <icu/> Given a keyword, returns an array of all values for
        /// that keyword that are currently in use.
        /// </summary>
        /// <param name="keyword">one of the keywords returned by <see cref="Keywords"/>.</param>
        /// <returns></returns>
        /// <seealso cref="Keywords"/>
        /// <stable>ICU 3.0</stable>
        public static string[] GetKeywordValues(string keyword)
        {
            if (!keyword.Equals(KEYWORDS[0]))
            {
                throw new ArgumentException("Invalid keyword: " + keyword);
            }
            return ICUResourceBundle.GetKeywordValues(BASE, RESOURCE, CollationData.IcuDataAssembly);
        }

        /// <summary>
        /// <icu/> Given a key and a locale, returns an array of string values in a preferred
        /// order that would make a difference. These are all and only those values where
        /// the open (creation) of the service with the locale formed from the input locale
        /// plus input keyword and that value has different behavior than creation with the
        /// input locale alone.
        /// </summary>
        /// <param name="key">
        /// one of the keys supported by this service. For now, only
        /// "collation" is supported.
        /// </param>
        /// <param name="locale">the locale</param>
        /// <param name="commonlyUsed">if set to true it will return only commonly used values
        /// with the given locale in preferred order.  Otherwise,
        /// it will return all the available values for the locale.
        /// </param>
        /// <returns>An array of string values for the given key and the locale.</returns>
        /// <stable>ICU 4.2</stable>
        public static string[] GetKeywordValuesForLocale(string key, UCultureInfo locale,
                                                               bool commonlyUsed)
        {
            // Note: The parameter commonlyUsed is not used.
            // The switch is in the method signature for consistency
            // with other locale services.

            // Read available collation values from collation bundles.
            ICUResourceBundle bundle = (ICUResourceBundle)
                    UResourceBundle.GetBundleInstance(
                            ICUData.IcuCollationBaseName, locale, CollationData.IcuDataAssembly);
            KeywordsSink sink = new KeywordsSink();
            bundle.GetAllItemsWithFallback("collations", sink);
            return sink.values.ToArray();
        }

        private sealed class KeywordsSink : ResourceSink
        {
            internal LinkedList<string> values = new LinkedList<string>();
            internal bool hasDefault = false;

            public override void Put(ResourceKey key, ResourceValue value, bool noFallback)
            {
                IResourceTable collations = value.GetTable();
                for (int i = 0; collations.GetKeyAndValue(i, key, value); ++i)
                {
                    UResourceType type = value.Type;
                    if (type == UResourceType.String)
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
                    else if (type == UResourceType.Table && !key.StartsWith("private-"))
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

        /// <summary>
        /// <icu/> Returns the functionally equivalent locale for the given
        /// requested locale, with respect to given keyword, for the
        /// collation service.  If two locales return the same result, then
        /// collators instantiated for these locales will behave
        /// equivalently.  The converse is not always true; two collators
        /// may in fact be equivalent, but return different results, due to
        /// internal details.  The return result has no other meaning than
        /// that stated above, and implies nothing as to the relationship
        /// between the two locales.  This is intended for use by
        /// applications who wish to cache collators, or otherwise reuse
        /// collators when possible.  The functional equivalent may change
        /// over time.  For more information, please see the <a
        /// href="http://userguide.icu-project.org/locale#TOC-Locales-and-Services">
        /// Locales and Services</a> section of the ICU User Guide.
        /// </summary>
        /// <param name="keyword">A particular keyword as enumerated by <see cref="Keywords"/>.</param>
        /// <param name="locID">The requested locale</param>
        /// <param name="isAvailable">If non-null, <paramref name="isAvailable"/>[0] will receive and
        /// output boolean that indicates whether the requested locale was
        /// 'available' to the collation service. If non-null, <paramref name="isAvailable"/>
        /// must have length &gt;= 1.</param>
        /// <returns>the locale</returns>
        /// <stable>ICU 3.0</stable>
        public static UCultureInfo GetFunctionalEquivalent(string keyword,
                                                            UCultureInfo locID,
                                                            bool[] isAvailable)
        {
            return ICUResourceBundle.GetFunctionalEquivalent(BASE, CollationData.IcuDataAssembly /* ICUResourceBundle.ICU_DATA_CLASS_LOADER */, RESOURCE,
                                                             keyword, locID, isAvailable, true);
        }

        /// <summary>
        /// <icu/> Returns the functionally equivalent locale for the given
        /// requested locale, with respect to given keyword, for the
        /// collation service.
        /// </summary>
        /// <param name="keyword">a particular keyword as enumerated by <see cref="Keywords"/>.</param>
        /// <param name="locID">The requested locale</param>
        /// <returns>the locale</returns>
        /// <seealso cref="GetFunctionalEquivalent(string, UCultureInfo, bool[])"/>
        /// <stable>ICU 3.0</stable>
        public static UCultureInfo GetFunctionalEquivalent(string keyword,
                                                            UCultureInfo locID)
        {
            return GetFunctionalEquivalent(keyword, locID, null);
        }

        /// <summary>
        /// <icu/> Returns the name of the collator for the <paramref name="objectLocale"/>, localized for the
        /// <paramref name="displayLocale"/>.
        /// </summary>
        /// <param name="objectLocale">the locale of the collator</param>
        /// <param name="displayLocale">the locale for the collator's display name</param>
        /// <returns>the display name</returns>
        /// <stable>ICU 2.6</stable>
        public static string GetDisplayName(CultureInfo objectLocale, CultureInfo displayLocale)
        {
            return GetShim().GetDisplayName(objectLocale.ToUCultureInfo(),
                                            displayLocale.ToUCultureInfo());
        }

        /// <summary>
        /// <icu/> Returns the name of the collator for the <paramref name="objectLocale"/>, localized for the
        /// <paramref name="displayLocale"/>.
        /// </summary>
        /// <param name="objectLocale">the locale of the collator</param>
        /// <param name="displayLocale">the locale for the collator's display name</param>
        /// <returns>the display name</returns>
        /// <stable>ICU 3.2</stable>
        public static string GetDisplayName(UCultureInfo objectLocale, UCultureInfo displayLocale)
        {
            return GetShim().GetDisplayName(objectLocale, displayLocale);
        }

        /// <summary>
        /// <icu/> Returns the name of the collator for the objectLocale, localized for the
        /// <see cref="UCultureInfo.CurrentUICulture"/> locale.
        /// </summary>
        /// <param name="objectLocale">the locale of the collator</param>
        /// <returns>the display name</returns>
        /// <seealso cref="UCultureInfo.CurrentUICulture"/>
        /// <stable>ICU 2.6</stable>
        public static string GetDisplayName(CultureInfo objectLocale)
        {
            return GetShim().GetDisplayName(objectLocale.ToUCultureInfo(), UCultureInfo.CurrentUICulture);
        }

        /// <summary>
        /// <icu/> Returns the name of the collator for the <paramref name="objectLocale"/>, localized for the
        /// <see cref="UCultureInfo.CurrentUICulture"/> locale.
        /// </summary>
        /// <param name="objectLocale">the locale of the collator</param>
        /// <returns>the display name</returns>
        /// <stable>ICU 3.2</stable>
        public static string GetDisplayName(UCultureInfo objectLocale)
        {
            return GetShim().GetDisplayName(objectLocale, UCultureInfo.CurrentUICulture);
        }

        // ICU4N specific - combined getStrength() and setStrength() into a property named Strength

        // ICU4N specific - combined getDecomposition() and setDecomposition() into a property named Decomposition

        // public other methods -------------------------------------------------

        /// <summary>
        /// Compares the equality of two text Strings using
        /// this <see cref="Collator"/>'s rules, strength and decomposition mode.  Convenience method.
        /// </summary>
        /// <param name="source">the source string to be compared.</param>
        /// <param name="target">the target string to be compared.</param>
        /// <returns>true if the strings are equal according to the collation rules, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">thrown if either arguments is null.</exception>
        /// <stable>ICU 2.8</stable>
        public virtual bool Equals(string source, string target) // ICU4N TODO: Throw ArgumentNullException ?
        {
            return (Compare(source, target) == 0);
        }

        /// <summary>
        /// <icu/> Returns a <see cref="UnicodeSet"/> that contains all the characters and sequences tailored
        /// in this collator.
        /// </summary>
        /// <returns>
        /// A pointer to a UnicodeSet object containing all the
        /// code points and sequences that may sort differently than
        /// in the root collator.
        /// </returns>
        /// <stable>ICU 2.4</stable>
        public virtual UnicodeSet GetTailoredSet()
        {
            return new UnicodeSet(0, 0x10FFFF);
        }

        /// <summary>
        /// Compares the source text <see cref="string"/> to the target text <see cref="string"/> according to
        /// this <see cref="Collator"/>'s rules, strength and decomposition mode.
        /// Returns an integer less than,
        /// equal to or greater than zero depending on whether the source String is
        /// less than, equal to or greater than the target <see cref="string"/>. See the <see cref="Collator"/>
        /// class description for an example of use.
        /// </summary>
        /// <param name="source">the source string.</param>
        /// <param name="target">the target string.</param>
        /// <returns>
        /// Returns an integer value. Value is less than zero if source is
        /// less than target, value is zero if source and target are equal,
        /// value is greater than zero if source is greater than target.
        /// </returns>
        /// <exception cref="ArgumentNullException">thrown if either argument is null.</exception>
        /// <stable>ICU 2.8</stable>
        public abstract int Compare(string source, string target); // ICU4N TODO: Throw ArgumentNullException ?

        /// <summary>
        /// Compares the source <see cref="object"/> to the target <see cref="object"/>.
        /// </summary>
        /// <param name="source">the source <see cref="object"/>.</param>
        /// <param name="target">the target <see cref="object"/>.</param>
        /// <returns>
        /// Returns an integer value. Value is less than zero if source is
        /// less than target, value is zero if source and target are equal,
        /// value is greater than zero if source is greater than target.
        /// </returns>
        /// <exception cref="InvalidCastException">thrown if either arguments cannot be cast to <see cref="string"/>.</exception>
        /// <stable>ICU 4.2</stable>
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

        /// <summary>
        /// Compares two <see cref="ICharSequence"/>s.
        /// The base class just calls <c>Compare(left.ToString(), right.ToString())</c>.
        /// Subclasses should instead implement this method and have the String API call this method.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        [Obsolete("This API is ICU internal only.")]
        internal virtual int DoCompare(ICharSequence left, ICharSequence right) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            return Compare(left.ToString(), right.ToString());
        }

        /// <summary>
        /// Transforms the <see cref="string"/> into a <see cref="CollationKey"/> suitable for efficient
        /// repeated comparison.  The resulting key depends on the collator's
        /// rules, strength and decomposition mode.
        /// <para/>
        /// Note that collation keys are often less efficient than simply doing comparison.
        /// For more details, see the ICU User Guide.
        /// <para/>
        /// See the CollationKey class documentation for more information.
        /// </summary>
        /// <param name="source">the <see cref="string"/> to be transformed into a <see cref="CollationKey"/>.</param>
        /// <returns>
        /// The <see cref="CollationKey"/> for the given <see cref="string"/> based on this <see cref="Collator"/>'s
        /// collation rules. If the source <see cref="string"/> is null, a null
        /// <see cref="CollationKey"/> is returned.
        /// </returns>
        /// <seealso cref="CollationKey"/>
        /// <seealso cref="Compare(string, string)"/>
        /// <seealso cref="GetRawCollationKey(string, RawCollationKey)"/>
        /// <stable>ICU 2.8</stable>
        public abstract CollationKey GetCollationKey(string source);

        /// <summary>
        /// <icu/> Returns the simpler form of a <see cref="CollationKey"/> for the String source following
        /// the rules of this Collator and stores the result into the user provided argument
        /// key.  If key has a internal byte array of length that's too small for the result,
        /// the internal byte array will be grown to the exact required size.
        /// <para/>
        /// Note that collation keys are often less efficient than simply doing comparison.
        /// For more details, see the ICU User Guide.
        /// </summary>
        /// <param name="source">the text <see cref="string"/> to be transformed into a <see cref="RawCollationKey"/></param>
        /// <param name="key"></param>
        /// <returns>
        /// If key is null, a new instance of <see cref="RawCollationKey"/> will be
        /// created and returned, otherwise the user provided key will be
        /// returned.
        /// </returns>
        /// <seealso cref="Compare(string, string)"/>
        /// <seealso cref="GetCollationKey(string)"/>
        /// <seealso cref="RawCollationKey"/>
        /// <stable>ICU 2.8</stable>
        public abstract RawCollationKey GetRawCollationKey(string source,
                                                           RawCollationKey key);

        /// <summary>
        /// <icu/> Gets or sets the variable top to the top of the specified reordering group.
        /// The variable top determines the highest-sorting character
        /// which is affected by the alternate handling behavior.
        /// If that attribute is set to UCOL_NON_IGNORABLE, then the variable top has no effect.
        /// <para/>
        /// The base class setter throws a <see cref="NotSupportedException"/> and the getter
        /// returns <see cref="ReorderCodes.Punctuation"/>.
        /// <para/>
        /// Valid values are one of <see cref="ReorderCodes.Space"/>, <see cref="ReorderCodes.Punctuation"/>,
        /// <see cref="ReorderCodes.Symbol"/>, <see cref="ReorderCodes.Currency"/>; 
        /// or <see cref="ReorderCodes.Default"/> to restore the default max variable group.
        /// </summary>
        /// <stable>ICU 53</stable>
        public virtual int MaxVariable
        {
            get { return ReorderCodes.Punctuation; }
            set { throw new NotSupportedException("Needs to be implemented by the subclass."); }
        }

        /// <summary>
        /// <icu/> Sets the variable top to the primary weight of the specified string.
        /// <para/>
        /// Beginning with ICU 53, the variable top is pinned to
        /// the top of one of the supported reordering groups,
        /// and it must not be beyond the last of those groups.
        /// See the setter of <see cref="MaxVariable"/>.
        /// </summary>
        /// <param name="varTop">
        /// one or more (if contraction) characters to which the
        /// variable top should be set
        /// </param>
        /// <returns>variable top primary weight</returns>
        /// <exception cref="ArgumentException">
        /// is thrown if varTop argument is not a valid variable top element. A variable top element is
        /// invalid when
        /// <list type="bullet">
        ///     <item><description>it is a contraction that does not exist in the Collation order</description></item>
        ///     <item><description>the variable top is beyond the last reordering group supported by <see cref="MaxVariable"/></description></item>
        ///     <item><description>when the <paramref name="varTop"/> argument is null or zero in length.</description></item>
        /// </list>
        /// </exception>
        /// <seealso cref="RuleBasedCollator.IsAlternateHandlingShifted"/>
        [Obsolete("ICU 53 Set MaxVariable instead.")]
        public abstract int SetVariableTop(string varTop);

        // ICU4N specific - combined SetVariableTop(int) and GetVariableTop() into property VariableTop

        /// <summary>
        /// Gets or sets the variable top to the specified primary weight.
        /// <para/>
        /// Beginning with ICU 53, the variable top is pinned to
        /// the top of one of the supported reordering groups,
        /// and it must not be beyond the last of those groups.
        /// See the setter for <see cref="MaxVariable"/>.
        /// </summary>
        /// <seealso cref="SetVariableTop(string)"/>
        /// <stable>ICU 2.6</stable>
        [Obsolete("ICU 53 Set MaxVariable instead.")]
        public abstract int VariableTop { get; set; }

        /// <summary>
        /// <icu/> Returns the version of this collator object.
        /// </summary>
        /// <returns>the version object associated with this collator</returns>
        /// <stable>ICU 2.8</stable>
        public abstract VersionInfo GetVersion();

        /// <summary>
        /// <icu/> Returns the UCA version of this collator object.
        /// </summary>
        /// <returns>the version object associated with this collator</returns>
        /// <stable>ICU 2.8</stable>
        public abstract VersionInfo GetUCAVersion();

        /// <summary>
        /// Retrieves the reordering codes for this collator.
        /// These reordering codes are a combination of <see cref="UScript"/> codes and <see cref="ReorderCodes"/>.
        /// </summary>
        /// <returns>
        /// a copy of the reordering codes for this collator;
        /// if none are set then returns an empty array
        /// </returns>
        /// <seealso cref="SetReorderCodes(int[])"/>
        /// <seealso cref="GetEquivalentReorderCodes(int)"/>
        /// <seealso cref="ReorderCodes"/>
        /// <seealso cref="UScript"/>
        /// <stable>ICU 4.8</stable>
        public virtual int[] GetReorderCodes()
        {
            throw new NotSupportedException("Needs to be implemented by the subclass.");
        }

        /// <summary>
        /// Retrieves all the reorder codes that are grouped with the given reorder code. Some reorder
        /// codes are grouped and must reorder together.
        /// Beginning with ICU 55, scripts only reorder together if they are primary-equal,
        /// for example Hiragana and Katakana.
        /// </summary>
        /// <param name="reorderCode">The reorder code to determine equivalence for.</param>
        /// <returns>The set of all reorder codes in the same group as the given reorder code.</returns>
        /// <seealso cref="SetReorderCodes(int[])"/>
        /// <seealso cref="GetReorderCodes()"/>
        /// <seealso cref="ReorderCodes"/>
        /// <seealso cref="UScript"/>
        /// <stable>ICU 4.8</stable>
        public static int[] GetEquivalentReorderCodes(int reorderCode)
        {
            CollationData baseData = CollationRoot.Data;
            return baseData.GetEquivalentScripts(reorderCode);
        }


        // Freezable interface implementation -------------------------------------------------

        /// <summary>
        /// Determines whether the object has been frozen or not.
        /// <para/>
        /// An unfrozen <see cref="Collator"/> is mutable and not thread-safe.
        /// A frozen <see cref="Collator"/> is immutable and thread-safe.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public virtual bool IsFrozen
        {
            get { return false; }
        }

        /// <summary>
        /// Freezes the collator.
        /// </summary>
        /// <returns>The collator itself.</returns>
        /// <stable>ICU 4.8</stable>
        public virtual Collator Freeze()
        {
            throw new NotSupportedException("Needs to be implemented by the subclass.");
        }

        /// <summary>
        /// Provides for the clone operation. Any clone is initially unfrozen.
        /// </summary>
        /// <returns></returns>
        /// <stable>ICU 4.8</stable>
        public virtual Collator CloneAsThawed()
        {
            throw new NotSupportedException("Needs to be implemented by the subclass.");
        }

        /// <summary>
        /// Empty default constructor
        /// </summary>
        /// <stable>ICU 2.4</stable>
        protected Collator()
        {
        }

        private static readonly bool DEBUG = ICUDebug.Enabled("collator");

        // -------- BEGIN UCultureInfo boilerplate --------

        /// <summary>
        /// <icu/> Gets the locale that was used to create this object, or <c>null</c>.
        /// <para/>
        /// Indicates the locale of the resource containing the data. This is always
        /// at or above the valid locale. If the valid locale does not contain the
        /// specific data being requested, then the actual locale will be
        /// above the valid locale. If the object was not constructed from
        /// locale data, then the valid locale is <c>null</c>.
        /// <para/>
        /// This may may differ from the locale requested at the time of
        /// this object's creation. For example, if an object is created
        /// for locale <c>en_US_CALIFORNIA</c>, the actual data may be
        /// drawn from <c>en</c> (the <i>actual</i> locale), and
        /// <c>en_US</c> may be the most specific locale that exists (the
        /// <i>valid</i> locale).
        /// <para/>
        /// Note: This property will be implemented in ICU 3.0; ICU 2.8
        /// contains a partial preview implementation. The * <i>actual</i>
        /// locale is returned correctly, but the <i>valid</i> locale is
        /// not, in most cases.
        /// <para/>
        /// The base class method always returns <see cref="UCultureInfo.InvariantCulture"/>
        /// Subclasses should override it if appropriate.
        /// </summary>
        /// <seealso cref="UCultureInfo"/>
        /// <draft>ICU 2.8 (retain)</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UCultureInfo ActualCulture
            => UCultureInfo.InvariantCulture;

        /// <summary>
        /// <icu/> Gets the locale that was used to create this object, or <c>null</c>.
        /// <para/>
        /// Indicates the most specific locale for which any data exists.
        /// This is always at or above the requested locale, and at or below
        /// the actual locale. If the requested locale does not correspond
        /// to any resource data, then the valid locale will be above the
        /// requested locale. If the object was not constructed from locale
        /// data, then the actual locale is <c>null</c>.
        /// <para/>
        /// This may may differ from the locale requested at the time of
        /// this object's creation. For example, if an object is created
        /// for locale <c>en_US_CALIFORNIA</c>, the actual data may be
        /// drawn from <c>en</c> (the <i>actual</i> locale), and
        /// <c>en_US</c> may be the most specific locale that exists (the
        /// <i>valid</i> locale).
        /// <para/>
        /// Note: This property will be implemented in ICU 3.0; ICU 2.8
        /// contains a partial preview implementation. The * <i>actual</i>
        /// locale is returned correctly, but the <i>valid</i> locale is
        /// not, in most cases.
        /// <para/>
        /// The base class method always returns <see cref="UCultureInfo.InvariantCulture"/>
        /// Subclasses should override it if appropriate.
        /// </summary>
        /// <seealso cref="UCultureInfo"/>
        /// <draft>ICU 2.8 (retain)</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UCultureInfo ValidCulture
            => UCultureInfo.InvariantCulture;

        /// <summary>
        /// Set information about the locales that were used to create this
        /// object.  If the object was not constructed from locale data,
        /// both arguments should be set to null.  Otherwise, neither
        /// should be null.  The actual locale must be at the same level or
        /// less specific than the valid locale. This method is intended
        /// for use by factories or other entities that create objects of
        /// this class.
        /// <para/>
        /// The base class method does nothing. Subclasses should override it if appropriate.
        /// </summary>
        /// <param name="valid">The most specific locale containing any resource data, or <c>null</c>.</param>
        /// <param name="actual">The locale containing data used to construct this object, or <c>null</c>.</param>
        /// <seealso cref="UCultureInfo"/>
        internal virtual void SetCulture(UCultureInfo valid, UCultureInfo actual) { }

        // -------- END UCultureInfo boilerplate --------
    }
}
