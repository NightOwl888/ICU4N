using ICU4N.Impl;
using ICU4N.Impl.Coll;
using ICU4N.Support.Text;
using ICU4N.Util;
using J2N;
using J2N.Collections;
using J2N.Numerics;
using J2N.Text;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ICU4N.Text
{
    /// <summary>
    /// <see cref="RuleBasedCollator"/> is a concrete subclass of <see cref="Collator"/>. It allows customization of the <see cref="Collator"/> via user-specified rule
    /// sets. <see cref="RuleBasedCollator"/> is designed to be fully compliant to the <a
    /// href="http://www.unicode.org/unicode/reports/tr10/">Unicode Collation Algorithm (UCA)</a> and conforms to ISO 14651.
    /// <para/>
    /// A <see cref="Collator"/> is thread-safe only when frozen. See <see cref="IsFrozen"/> and <see cref="IFreezable{T}"/>.
    /// <para/>
    /// Users are strongly encouraged to read the <a href="http://userguide.icu-project.org/collation">User
    /// Guide</a> for more information about the collation service before using this class.
    /// </summary>
    /// <remarks>
    /// Create a <see cref="RuleBasedCollator"/> from a locale by calling the <see cref="Collator.GetInstance(System.Globalization.CultureInfo)"/> factory method in the base class
    /// <see cref="Collator"/>. <see cref="Collator.GetInstance(System.Globalization.CultureInfo)"/> creates a <see cref="RuleBasedCollator"/> object based on the collation rules defined by the
    /// argument locale. If a customized collation ordering or attributes is required, use the <see cref="RuleBasedCollator(string)"/>
    /// constructor with the appropriate rules. The customized <see cref="RuleBasedCollator"/> will base its ordering on the CLDR root collation, while
    /// re-adjusting the attributes and orders of the characters in the specified rule accordingly.
    /// <para/>
    /// RuleBasedCollator provides correct collation orders for most locales supported in ICU. If specific data for a locale
    /// is not available, the orders eventually falls back to the
    /// <a href="http://www.unicode.org/reports/tr35/tr35-collation.html#Root_Collation">CLDR root sort order</a>.
    /// <para/>
    /// For information about the collation rule syntax and details about customization, please refer to the <a
    /// href="http://userguide.icu-project.org/collation/customization">Collation customization</a> section of the
    /// User Guide.
    /// <para/>
    /// Note that
    /// ICU4N's <see cref="RuleBasedCollator"/> does not support turning off the Thai/Lao vowel-consonant swapping using '!', since the UCA clearly
    /// states that it has to be supported to ensure a correct sorting order. If a '!' is encountered, it is ignored.
    /// <para/>
    /// <strong>Examples</strong>
    /// <para/>
    /// Creating Customized <see cref="RuleBasedCollator"/>s: 
    /// <code>
    /// string simple = "&amp; a &lt; b &lt; c &lt; d";
    /// RuleBasedCollator simpleCollator = new RuleBasedCollator(simple);
    /// 
    /// string norwegian = "&amp; a , A &lt; b , B &lt; c , C &lt; d , D &lt; e , E "
    ///                    + "&lt; f , F &lt; g , G &lt; h , H &lt; i , I &lt; j , "
    ///                    + "J &lt; k , K &lt; l , L &lt; m , M &lt; n , N &lt; "
    ///                    + "o , O &lt; p , P &lt; q , Q &lt;r , R &lt;s , S &lt; "
    ///                    + "t , T &lt; u , U &lt; v , V &lt; w , W &lt; x , X "
    ///                    + "&lt; y , Y &lt; z , Z &lt; &#92;u00E5 = a&#92;u030A "
    ///                    + ", &#92;u00C5 = A&#92;u030A ; aa , AA &lt; &#92;u00E6 "
    ///                    + ", &#92;u00C6 &lt; &#92;u00F8 , &#92;u00D8";
    /// RuleBasedCollator norwegianCollator = new RuleBasedCollator(norwegian);
    /// </code>
    /// <para/>
    /// Concatenating rules to combine <c>Collator</c>s:
    /// <code>
    /// // Create an en-US Collator object
    /// RuleBasedCollator en_USCollator = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US"));
    /// // Create a da-DK Collator object
    /// RuleBasedCollator da_DKCollator = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("da-DK"));
    /// 
    /// // Combine the two
    /// 
    /// // First, get the collation rules from en_USCollator
    /// string en_USRules = en_USCollator.GetRules();
    /// // Second, get the collation rules from da_DKCollator
    /// string da_DKRules = da_DKCollator.GetRules();
    /// RuleBasedCollator newCollator = new RuleBasedCollator(en_USRules + da_DKRules);
    /// // newCollator has the combined rules
    /// </code>
    /// <para/>
    /// Making changes to an existing <see cref="RuleBasedCollator"/> to create a new <c>Collator</c> object, by appending changes to
    /// the existing rule: 
    /// <code>
    /// // Create a new Collator object with additional rules
    /// string addRules = "&amp; C &lt; ch, cH, Ch, CH";
    /// RuleBasedCollator myCollator = new RuleBasedCollator(en_USCollator.GetRules() + addRules);
    /// // myCollator contains the new rules
    /// </code>
    /// <para/>
    /// How to change the order of non-spacing accents: 
    /// <code>
    /// // old rule with main accents
    /// string oldRules = "= &#92;u0301 ; &#92;u0300 ; &#92;u0302 ; &#92;u0308 "
    ///                 + "; &#92;u0327 ; &#92;u0303 ; &#92;u0304 ; &#92;u0305 "
    ///                 + "; &#92;u0306 ; &#92;u0307 ; &#92;u0309 ; &#92;u030A "
    ///                 + "; &#92;u030B ; &#92;u030C ; &#92;u030D ; &#92;u030E "
    ///                 + "; &#92;u030F ; &#92;u0310 ; &#92;u0311 ; &#92;u0312 "
    ///                 + "&lt; a , A ; ae, AE ; &#92;u00e6 , &#92;u00c6 "
    ///                 + "&lt; b , B &lt; c, C &lt; e, E &amp; C &lt; d , D";
    /// // change the order of accent characters
    /// string addOn = "&amp; &#92;u0300 ; &#92;u0308 ; &#92;u0302";
    /// RuleBasedCollator myCollator = new RuleBasedCollator(oldRules + addOn);
    /// </code>
    /// <para/>
    /// Putting in a new primary ordering before the default setting, e.g. sort English characters before or after Japanese
    /// characters in the Japanese <c>Collator</c>:
    /// <code>
    /// // get en_US Collator rules
    /// RuleBasedCollator en_USCollator = (RuleBasedCollator)Collator.GetInstance(new CultureInfo("en-US"));
    /// // add a few Japanese characters to sort before English characters
    /// // suppose the last character before the first base letter 'a' in
    /// // the English collation rule is &#92;u2212
    /// string jaString = "&amp; &#92;u2212 &lt;&#92;u3041, &#92;u3042 &lt;&#92;u3043, "
    ///                   + "&#92;u3044";
    /// RuleBasedCollator myJapaneseCollator = new RuleBasedCollator(en_USCollator.GetRules() + jaString);
    /// </code>
    /// <para/>
    /// This class cannot be inherited.
    /// </remarks>
    /// <author>Syn Wee Quek</author>
    /// <stable>ICU 2.8</stable>
    public sealed class RuleBasedCollator : Collator
    {
        // public constructors ---------------------------------------------------

        /// <summary>
        /// Constructor that takes the argument rules for customization.
        /// The collator will be based on the CLDR root collation, with the
        /// attributes and re-ordering of the characters specified in the argument rules.
        /// <para/>
        /// See the User Guide's section on <a href="http://userguide.icu-project.org/collation/customization">
        /// Collation Customization</a> for details on the rule syntax.
        /// </summary>
        /// <param name="rules">The collation rules to build the collation table from.</param>
        /// <exception cref="FormatException">Thrown when argument rules have an invalid syntax.</exception>
        /// <exception cref="System.IO.IOException">Thrown when an error occurred while reading internal data.</exception>
        /// <stable>ICU 2.8</stable>
        public RuleBasedCollator(string rules)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules), "Collation rules can not be null");
            }
            validLocale = ULocale.ROOT;
            InternalBuildTailoring(rules);
        }

        /// <summary>
        /// Implements from-rule constructors.
        /// </summary>
        /// <param name="rules">Rule string.</param>
        /// <exception cref="Exception"/>
        private void InternalBuildTailoring(string rules)
        {
            // ICU4N TODO: Seems like reflection is overkill here.

            CollationTailoring @base = CollationRoot.Root;
            // Most code using Collator does not need to build a Collator from rules.
            // By using reflection, most code will not have a static dependency on the builder code.
            // CollationBuilder builder = new CollationBuilder(base);
#if FEATURE_TYPEEXTENSIONS_GETTYPEINFO
            Assembly classLoader = GetType().GetTypeInfo().Assembly; // ClassLoaderUtil.getClassLoader(GetUnicodeCategory());
#else
            Assembly classLoader = GetType().Assembly; // ClassLoaderUtil.getClassLoader(GetUnicodeCategory());
#endif
            CollationTailoring t;
            try
            {
                Type builderClass = classLoader.GetType("ICU4N.Impl.Coll.CollationBuilder");
                object builder = builderClass.GetConstructor(new Type[] { typeof(CollationTailoring) }).Invoke(new object[] { @base });
                // builder.parseAndBuild(rules);
                //Method parseAndBuild = builderClass.getMethod("parseAndBuild", String.class);
                //            t = (CollationTailoring) parseAndBuild.invoke(builder, rules);

                MethodInfo parseAndBuild = builderClass.GetMethod("ParseAndBuild", new Type[] { typeof(string) });
                t = (CollationTailoring)parseAndBuild.Invoke(builder, new object[] { rules });
            }
            catch (TargetInvocationException e)
            {
                throw e.GetBaseException();
            }


            t.ActualLocale = null;
            AdoptTailoring(t);
        }

        // public methods --------------------------------------------------------

        /// <summary>
        /// Clones the <see cref="RuleBasedCollator"/>.
        /// </summary>
        /// <returns>A new instance of this <see cref="RuleBasedCollator"/> object.</returns>
        /// <stable>ICU 2.8</stable>
        public override object Clone()
        {
            if (IsFrozen)
            {
                return this;
            }
            return CloneAsThawed();
        }

        private void InitMaxExpansions()
        {
            lock (tailoring)
            {
                if (tailoring.MaxExpansions == null)
                {
                    tailoring.MaxExpansions = CollationElementIterator.ComputeMaxExpansions(tailoring.Data);
                }
            }
        }

        /// <summary>
        /// Return a <see cref="CollationElementIterator"/> for the given <see cref="string"/>.
        /// </summary>
        /// <seealso cref="CollationElementIterator"/>
        /// <stable>ICU 2.8</stable>
        public CollationElementIterator GetCollationElementIterator(string source)
        {
            InitMaxExpansions();
            return new CollationElementIterator(source, this);
        }

        /// <summary>
        /// Return a <see cref="CollationElementIterator"/> for the given <see cref="CharacterIterator"/>. The source iterator's integrity will be
        /// preserved since a new copy will be created for use.
        /// </summary>
        /// <seealso cref="CollationElementIterator"/>
        /// <stable>ICU 2.8</stable>
        public CollationElementIterator GetCollationElementIterator(CharacterIterator source)
        {
            InitMaxExpansions();
            CharacterIterator newsource = (CharacterIterator)source.Clone();
            return new CollationElementIterator(newsource, this);
        }

        /// <summary>
        /// Return a <see cref="CollationElementIterator"/> for the given <see cref="UCharacterIterator"/>. The source iterator's integrity will be
        /// preserved since a new copy will be created for use.
        /// </summary>
        /// <seealso cref="CollationElementIterator"/>
        /// <stable>ICU 2.8</stable>
        public CollationElementIterator GetCollationElementIterator(UCharacterIterator source)
        {
            InitMaxExpansions();
            return new CollationElementIterator(source, this);
        }

        // Freezable interface implementation -------------------------------------------------

        /// <summary>
        /// Determines whether the object has been frozen or not.
        /// <para/>
        /// An unfrozen <see cref="Collator"/> is mutable and not thread-safe.
        /// A frozen <see cref="Collator"/> is immutable and thread-safe.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public override bool IsFrozen
        {
            get { return frozenLock != null; }
        }

        /// <summary>
        /// Freezes the collator.
        /// </summary>
        /// <returns>The collator itself.</returns>
        /// <stable>ICU 4.8</stable>
        public override Collator Freeze()
        {
            if (!IsFrozen)
            {
                frozenLock = new object(); // ICU4N: Using object/Monitor to replace ReentrantLock
                if (collationBuffer == null)
                {
                    collationBuffer = new CollationBuffer(data);
                }
            }
            return this;
        }

        /// <summary>
        /// Provides for the clone operation. Any clone is initially unfrozen.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public override Collator CloneAsThawed()
        {
            //try
            //{
            RuleBasedCollator result = (RuleBasedCollator)base.Clone();
            // since all collation data in the RuleBasedCollator do not change
            // we can safely assign the result.fields to this collator 
            // except in cases where we can't
            result.settings = (SharedObject.Reference<CollationSettings>)settings.Clone();
            result.collationBuffer = null;
            result.frozenLock = null;
            return result;
            //}
            //catch (CloneNotSupportedException e)
            //{
            //    // Clone is implemented
            //    return null;
            //}
        }

        // public setters --------------------------------------------------------

        private void CheckNotFrozen()
        {
            if (IsFrozen)
            {
                throw new NotSupportedException("Attempt to modify frozen RuleBasedCollator");
            }
        }

        private CollationSettings GetOwnedSettings()
        {
            return settings.CopyOnWrite();
        }

        private CollationSettings GetDefaultSettings()
        {
            return tailoring.Settings.ReadOnly;
        }

        /// <summary>
        /// Gets or sets the Hiragana Quaternary mode to be on or off. When the Hiragana Quaternary mode is turned on, the collator
        /// positions Hiragana characters before all non-ignorable characters in <see cref="CollationStrength.Quaternary"/> strength. This is to produce a
        /// correct JIS collation order, distinguishing between Katakana and Hiragana characters.
        /// <para/>
        /// This attribute was an implementation detail of the CLDR Japanese tailoring.
        /// Since ICU 50, this attribute is not settable any more via API functions.
        /// Since CLDR 25/ICU 53, explicit quaternary relations are used
        /// to achieve the same Japanese sort order.
        /// </summary>
        /// <seealso cref="SetHiraganaQuaternaryToDefault()"/>
        [Obsolete("ICU 50 Implementation detail, cannot be set via API, was removed from implementation.")]
        public bool IsHiraganaQuaternary
        {
            get { return false; }
            set { CheckNotFrozen(); }
        }

        /// <summary>
        /// Gets or sets the Hiragana Quaternary mode to the initial mode set during construction of the RuleBasedCollator. See
        /// <see cref="IsHiraganaQuaternary"/> for more details.
        /// <para/>
        /// This attribute was an implementation detail of the CLDR Japanese tailoring.
        /// Since ICU 50, this attribute is not settable any more via API functions.
        /// Since CLDR 25/ICU 53, explicit quaternary relations are used
        /// to achieve the same Japanese sort order.
        /// </summary>
        /// <seealso cref="IsHiraganaQuaternary"/>
        [Obsolete("ICU 50 Implementation detail, cannot be set via API, was removed from implementation.")]
        public void SetHiraganaQuaternaryToDefault() // ICU4N specific - Renamed from SetHiraganaQuaternaryDefault()
        {
            CheckNotFrozen();
        }

        /// <summary>
        /// Gets or sets whether uppercase characters sort before lowercase characters or vice versa in strength <see cref="CollationStrength.Tertiary"/>. The
        /// default mode is false, and so lowercase characters sort before uppercase characters. If true, sort uppercase
        /// characters first.
        /// </summary>
        public bool IsUpperCaseFirst
        {
            get
            {
                return (settings.ReadOnly.GetCaseFirst() == CollationSettings.CaseFirstAndUpperMask);
            }
            set
            {
                CheckNotFrozen();
                if (value == IsUpperCaseFirst) { return; }
                CollationSettings ownedSettings = GetOwnedSettings();
                ownedSettings.SetCaseFirst(value ? CollationSettings.CaseFirstAndUpperMask : 0);
                SetFastLatinOptions(ownedSettings);
            }
        }

        /// <summary>
        /// Gets or sets the orders of lower-cased characters to sort before uppercased characters in strength <see cref="CollationStrength.Tertiary"/>. The
        /// default mode is false. If true is set, the <see cref="RuleBasedCollator"/> will sort lowercased characters before the uppercased 
        /// ones. Otherwise, if false is set, the <see cref="RuleBasedCollator"/> will ignore case preferences.
        /// </summary>
        /// <seealso cref="IsUpperCaseFirst"/>
        /// <seealso cref="SetCaseFirstToDefault()"/>
        /// <stable>ICU 2.8</stable>
        public bool IsLowerCaseFirst
        {
            get
            {
                return (settings.ReadOnly.GetCaseFirst() == CollationSettings.CaseFirst);
            }
            set
            {
                CheckNotFrozen();
                if (value == IsLowerCaseFirst) { return; }
                CollationSettings ownedSettings = GetOwnedSettings();
                ownedSettings.SetCaseFirst(value ? CollationSettings.CaseFirst : 0);
                SetFastLatinOptions(ownedSettings);
            }
        }

        /// <summary>
        /// Sets the case first mode to the initial mode set during construction of the <see cref="RuleBasedCollator"/>. See
        /// <see cref="IsUpperCaseFirst"/> and <see cref="IsLowerCaseFirst"/> for more details.
        /// </summary>
        /// <seealso cref="IsUpperCaseFirst"/>
        /// <seealso cref="IsLowerCaseFirst"/>
        /// <stable>ICU 2.8</stable>
        public void SetCaseFirstToDefault() // ICU4N specific - Renamed from SetCaseFirstDefault()
        {
            CheckNotFrozen();
            CollationSettings defaultSettings = GetDefaultSettings();
            if (settings.ReadOnly == defaultSettings) { return; }
            CollationSettings ownedSettings = GetOwnedSettings();
            ownedSettings.SetCaseFirstDefault(defaultSettings.Options);
            SetFastLatinOptions(ownedSettings);
        }

        /// <summary>
        /// Sets the alternate handling mode to the initial mode set during construction of the <see cref="RuleBasedCollator"/>. See
        /// <see cref="IsAlternateHandlingShifted"/> for more details.
        /// </summary>
        /// <seealso cref="IsAlternateHandlingShifted"/>
        /// <stable>ICU 2.8</stable>
        public void SetAlternateHandlingToDefault() // ICU4N specific - Renamed from SetAlternateHandlingDefault()
        {
            CheckNotFrozen();
            CollationSettings defaultSettings = GetDefaultSettings();
            if (settings.ReadOnly == defaultSettings) { return; }
            CollationSettings ownedSettings = GetOwnedSettings();
            ownedSettings.SetAlternateHandlingDefault(defaultSettings.Options);
            SetFastLatinOptions(ownedSettings);
        }

        /// <summary>
        /// Sets the case level mode to the initial mode set during construction of the <see cref="RuleBasedCollator"/>. See
        /// <seealso cref="IsCaseLevel"/> for more details.
        /// </summary>
        /// <seealso cref="IsCaseLevel"/>
        /// <stable>ICU 2.8</stable>
        public void SetCaseLevelToDefault() // ICU4N specific - Renamed from SetCaseLevelDefault()
        {
            CheckNotFrozen();
            CollationSettings defaultSettings = GetDefaultSettings();
            if (settings.ReadOnly == defaultSettings) { return; }
            CollationSettings ownedSettings = GetOwnedSettings();
            ownedSettings.SetFlagDefault(CollationSettings.CaseLevel, defaultSettings.Options);
            SetFastLatinOptions(ownedSettings);
        }

        /// <summary>
        /// Sets the decomposition mode to the initial mode set during construction of the <see cref="RuleBasedCollator"/>. See
        /// <see cref="Decomposition"/> for more details.
        /// </summary>
        /// <seealso cref="Decomposition"/>
        /// <stable>ICU 2.8</stable>
        public void SetDecompositionToDefault() // ICU4N specfic - Renamed from SetDecompositionDefault()
        {
            CheckNotFrozen();
            CollationSettings defaultSettings = GetDefaultSettings();
            if (settings.ReadOnly == defaultSettings) { return; }
            CollationSettings ownedSettings = GetOwnedSettings();
            ownedSettings.SetFlagDefault(CollationSettings.CheckFCD, defaultSettings.Options);
            SetFastLatinOptions(ownedSettings);
        }

        /// <summary>
        /// Sets the French collation mode to the initial mode set during construction of the <see cref="RuleBasedCollator"/>. See
        /// <see cref="IsFrenchCollation"/> for more details.
        /// </summary>
        /// <seealso cref="IsFrenchCollation"/>
        /// <stable>ICU 2.8</stable>
        public void SetFrenchCollationToDefault() // ICU4N specific - Renamed from SetFrenchCollationDefault()
        {
            CheckNotFrozen();
            CollationSettings defaultSettings = GetDefaultSettings();
            if (settings.ReadOnly == defaultSettings) { return; }
            CollationSettings ownedSettings = GetOwnedSettings();
            ownedSettings.SetFlagDefault(CollationSettings.BackwardSecondary, defaultSettings.Options);
            SetFastLatinOptions(ownedSettings);
        }

        /// <summary>
        /// Sets the collation strength to the initial mode set during the construction of the <see cref="RuleBasedCollator"/>. See
        /// <see cref="Strength"/> for more details.
        /// </summary>
        /// <seealso cref="Strength"/>
        /// <stable>ICU 2.8</stable>
        public void SetStrengthToDefault() // ICU4N specific - renamed from SetStrengthDefault()
        {
            CheckNotFrozen();
            CollationSettings defaultSettings = GetDefaultSettings();
            if (settings.ReadOnly == defaultSettings) { return; }
            CollationSettings ownedSettings = GetOwnedSettings();
            ownedSettings.SetStrengthDefault(defaultSettings.Options);
            SetFastLatinOptions(ownedSettings);
        }

        /// <summary>
        /// Method to set numeric collation to its default value.
        /// </summary>
        /// <seealso cref="IsNumericCollation"/>
        /// <stable>ICU 2.8</stable>
        public void SetNumericCollationToDefault() // ICU4N specific - Renamed from SetNumericCollationDefault()
        {
            CheckNotFrozen();
            CollationSettings defaultSettings = GetDefaultSettings();
            if (settings.ReadOnly == defaultSettings) { return; }
            CollationSettings ownedSettings = GetOwnedSettings();
            ownedSettings.SetFlagDefault(CollationSettings.Numeric, defaultSettings.Options);
            SetFastLatinOptions(ownedSettings);
        }

        /// <summary>
        /// Gets or sets whether French Collation is enabled. Sets the mode for the direction of <see cref="CollationStrength.Secondary"/> 
        /// weights to be used in French collation. The default value is false,
        /// which treats <see cref="CollationStrength.Secondary"/> weights in the order they appear. If set to true, the <see cref="CollationStrength.Secondary"/> weights will be sorted
        /// backwards. See the section on <a href="http://userguide.icu-project.org/collation/architecture">
        /// French collation</a> for more information.
        /// </summary>
        /// <seealso cref="SetFrenchCollationToDefault()"/>
        /// <stable>ICU 2.8</stable>
        public bool IsFrenchCollation
        {
            get
            {
                return (settings.ReadOnly.Options & CollationSettings.BackwardSecondary) != 0;
            }
            set
            {
                CheckNotFrozen();
                if (value == IsFrenchCollation) { return; }
                CollationSettings ownedSettings = GetOwnedSettings();
                ownedSettings.SetFlag(CollationSettings.BackwardSecondary, value);
                SetFastLatinOptions(ownedSettings);
            }
        }

        /// <summary>
        /// Gets or sets whether the handling for <see cref="CollationStrength.Quaternary"/> to be either shifted or non-ignorable. See the UCA definition
        /// on <a href="http://www.unicode.org/unicode/reports/tr10/#Variable_Weighting">Variable Weighting</a>. This
        /// property will only be effective when <see cref="Strength"/> is set to <see cref="CollationStrength.Quaternary"/>. The default value for this mode is false,
        /// corresponding to the NON_IGNORABLE mode in UCA. In the NON_IGNORABLE mode, the RuleBasedCollator treats all
        /// the code points with non-ignorable primary weights in the same way. If the mode is set to true, the behavior
        /// corresponds to SHIFTED defined in UCA, this causes code points with <see cref="CollationStrength.Primary"/> orders that are equal or below the
        /// variable top value to be ignored in <see cref="CollationStrength.Primary"/> order and moved to the <see cref="CollationStrength.Quaternary"/> order.
        /// </summary>
        /// <seealso cref="SetAlternateHandlingToDefault()"/>
        /// <stable>ICU 2.8</stable>
        public bool IsAlternateHandlingShifted
        {
            get
            {
                return settings.ReadOnly.AlternateHandling;
            }
            set
            {
                CheckNotFrozen();
                if (value == IsAlternateHandlingShifted) { return; }
                CollationSettings ownedSettings = GetOwnedSettings();
                ownedSettings.SetAlternateHandlingShifted(value);
                SetFastLatinOptions(ownedSettings);
            }
        }

        /// <summary>
        /// Gets or sets case level. When case level is set to true, an additional weight is formed between the 
        /// <see cref="CollationStrength.Secondary"/> and <see cref="CollationStrength.Tertiary"/> weight, known
        /// as the case level. The case level is used to distinguish large and small Japanese Kana characters. Case level
        /// could also be used in other situations. For example to distinguish certain Pinyin characters. The default value
        /// is false, which means the case level is not generated. The contents of the case level are affected by the case
        /// first mode. A simple way to ignore accent differences in a string is to set the strength to <see cref="CollationStrength.Primary"/> and enable
        /// case level.
        /// <para/>
        /// See the section on <a href="http://userguide.icu-project.org/collation/architecture">case
        /// level</a> for more information.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        /// <seealso cref="SetCaseLevelToDefault()"/>
        public bool IsCaseLevel
        {
            get
            {
                return (settings.ReadOnly.Options & CollationSettings.CaseLevel) != 0;
            }
            set
            {
                CheckNotFrozen();
                if (value == IsCaseLevel) { return; }
                CollationSettings ownedSettings = GetOwnedSettings();
                ownedSettings.SetFlag(CollationSettings.CaseLevel, value);
                SetFastLatinOptions(ownedSettings);
            }
        }

        /// <summary>
        /// Gets or sets the decomposition mode of this Collator. The decomposition mode
        /// determines how Unicode composed characters are handled.
        /// <para/>
        /// Setting this property with <see cref="NormalizationMode.CanonicalDecomposition"/> allows the
        /// <see cref="Collator"/> to handle un-normalized text properly, producing the
        /// same results as if the text were normalized. If
        /// <see cref="NormalizationMode.NoDecomposition"/> is set, it is the user's responsibility to
        /// ensure that all text is already in the appropriate form before
        /// a comparison or before getting a <see cref="CollationKey"/>. Adjusting
        /// decomposition mode allows the user to select between faster and
        /// more complete collation behavior.
        /// <para/>
        /// Since a great many of the world's languages do not require
        /// text normalization, most locales set <see cref="NormalizationMode.NoDecomposition"/> as the
        /// default decomposition mode.
        /// <para/>
        /// The default decompositon mode for the <see cref="Collator"/> is <see cref="NormalizationMode.NoDecomposition"/>,
        /// unless specified otherwise by the locale used
        /// to create the <see cref="Collator"/>.
        /// <para/>
        /// See the <see cref="Collator"/> documentation for more details.
        /// </summary>
        /// <seealso cref="NormalizationMode.NoDecomposition"/>
        /// <seealso cref="NormalizationMode.CanonicalDecomposition"/>
        /// <stable>ICU 2.8</stable>
        public override NormalizationMode Decomposition
        {
            get
            {
                return (settings.ReadOnly.Options & CollationSettings.CheckFCD) != 0 ?
                    NormalizationMode.CanonicalDecomposition : NormalizationMode.NoDecomposition;
            }
            set
            {
                CheckNotFrozen();
                bool flag;
                switch (value)
                {
                    case NormalizationMode.NoDecomposition:
                        flag = false;
                        break;
                    case NormalizationMode.CanonicalDecomposition:
                        flag = true;
                        break;
                    default:
                        throw new ArgumentException("Wrong decomposition mode.");
                }
                if (flag == settings.ReadOnly.GetFlag(CollationSettings.CheckFCD)) { return; }
                CollationSettings ownedSettings = GetOwnedSettings();
                ownedSettings.SetFlag(CollationSettings.CheckFCD, flag);
                SetFastLatinOptions(ownedSettings);
            }
        }

        /// <summary>
        /// Gets or sets this Collator's strength attribute. The strength attribute determines the minimum level of difference
        /// considered significant during comparison.
        /// <para/>
        /// See the <see cref="Collator"/> documentation for an example of use.
        /// </summary>
        /// <seealso cref="SetStrengthToDefault()"/>
        /// <seealso cref="CollationStrength.Primary"/>
        /// <seealso cref="CollationStrength.Secondary"/>
        /// <seealso cref="CollationStrength.Tertiary"/>
        /// <seealso cref="CollationStrength.Quaternary"/>
        /// <seealso cref="CollationStrength.Identical"/>
        /// <exception cref="ArgumentException">If the new strength value is not one of <see cref="CollationStrength.Primary"/>, <see cref="CollationStrength.Secondary"/>, 
        /// <see cref="CollationStrength.Tertiary"/>, <see cref="CollationStrength.Quaternary"/> or <see cref="CollationStrength.Identical"/>.</exception>
        /// <stable>ICU 2.8</stable>
        public override CollationStrength Strength
        {
            get
            {
                return settings.ReadOnly.Strength;
            }
            set
            {
                CheckNotFrozen();
                if (value == Strength) { return; }
                CollationSettings ownedSettings = GetOwnedSettings();
                ownedSettings.Strength = value;
                SetFastLatinOptions(ownedSettings);
            }
        }

        /// <summary>
        /// <icu/> Gets or sets the maximum reordering group whose characters are affected by
        /// the alternate handling behavior.
        /// <para/>
        /// Setter will set the variable top to the top of the specified reordering group.
        /// The variable top determines the highest-sorting character
        /// which is affected by the alternate handling behavior.
        /// If that attribute is set to NON_IGNORABLE, then the variable top has no effect.
        /// <para/>
        /// Set to one of <see cref="ReorderCodes.Space"/>, <see cref="ReorderCodes.Punctuation"/>,
        /// <see cref="ReorderCodes.Symbol"/>, <see cref="ReorderCodes.Currency"/>;
        /// or <see cref="ReorderCodes.Default"/> to restore the default max variable group.
        /// </summary>
        /// <stable>ICU 53</stable>
        public override int MaxVariable
        {
            get
            {
                return ReorderCodes.First + settings.ReadOnly.MaxVariable;
            }
            set
            {
                // Convert the reorder code into a MaxVariable number, or UCOL_DEFAULT=-1.
                int val;
                if (value == ReorderCodes.Default)
                {
                    val = -1;  // UCOL_DEFAULT
                }
                else if (ReorderCodes.First <= value && value <= ReorderCodes.Currency)
                {
                    val = value - ReorderCodes.First;
                }
                else
                {
                    throw new ArgumentException("illegal max variable group " + value);
                }
                int oldValue = settings.ReadOnly.MaxVariable;
                if (val == oldValue)
                {
                    return;
                }
                CollationSettings defaultSettings = GetDefaultSettings();
                if (settings.ReadOnly == defaultSettings)
                {
                    if (val < 0)
                    {  // UCOL_DEFAULT
                        return;
                    }
                }
                CollationSettings ownedSettings = GetOwnedSettings();

                if (value == ReorderCodes.Default)
                {
                    value = ReorderCodes.First + defaultSettings.MaxVariable;
                }
                long varTop = data.GetLastPrimaryForGroup((int)value);
                Debug.Assert(varTop != 0);
                ownedSettings.SetMaxVariable(val, defaultSettings.Options);
                ownedSettings.VariableTop = varTop;
                SetFastLatinOptions(ownedSettings);
            }
        }

        /// <summary>
        /// <icu/>Sets the variable top to the primary weight of the specified string.
        /// <para/>
        /// Beginning with ICU 53, the variable top is pinned to
        /// the top of one of the supported reordering groups,
        /// and it must not be beyond the last of those groups.
        /// <see cref="MaxVariable"/>.
        /// </summary>
        /// <param name="varTop">One or more (if contraction) characters to which the variable top should be set.</param>
        /// <returns>Top primary weight.</returns>
        /// <exception cref="ArgumentException">thrown if varTop argument is not a valid variable top element. A variable top element is
        /// invalid when
        /// <list type="bullet">
        ///     <item>
        ///         <description>It is a contraction that does not exist in the <see cref="Collation"/> order</description>
        ///     </item>
        ///     <item>
        ///         <description>The variable top is beyond the last reordering group supported by <see cref="MaxVariable"/>.</description>
        ///     </item>
        ///     <item>
        ///         <description>When the <paramref name="varTop"/> argument is null or zero in length.</description>
        ///     </item>
        /// </list>
        /// </exception>
        /// <seealso cref="VariableTop"/>
        /// <seealso cref="RuleBasedCollator.IsAlternateHandlingShifted"/>
        [Obsolete("ICU 53 Set MaxVariable instead.")]
        public override int SetVariableTop(string varTop)
        {
            CheckNotFrozen();
            if (varTop == null || varTop.Length == 0)
            {
                throw new ArgumentException("Variable top argument string can not be null or zero in length.");
            }
            bool numeric = settings.ReadOnly.IsNumeric;
            long ce1, ce2;
            if (settings.ReadOnly.DontCheckFCD)
            {
                UTF16CollationIterator ci = new UTF16CollationIterator(data, numeric, varTop.AsCharSequence(), 0);
                ce1 = ci.NextCE();
                ce2 = ci.NextCE();
            }
            else
            {
                FCDUTF16CollationIterator ci = new FCDUTF16CollationIterator(data, numeric, varTop.AsCharSequence(), 0);
                ce1 = ci.NextCE();
                ce2 = ci.NextCE();
            }
            if (ce1 == Collation.NoCE || ce2 != Collation.NoCE)
            {
                throw new ArgumentException("Variable top argument string must map to exactly one collation element");
            }
            InternalSetVariableTop(ce1.TripleShift(32));
            return (int)settings.ReadOnly.VariableTop;
        }

        /// <summary>
        /// <icu/>Gets or sets the variable top value of a Collator.
        /// <para/>
        /// Beginning with ICU 53, the variable top is pinned to
        /// the top of one of the supported reordering groups,
        /// and it must not be beyond the last of those groups.
        /// See <see cref="MaxVariable"/>.
        /// </summary>
        /// <seealso cref="SetVariableTop(string)"/>
        /// <seealso cref="MaxVariable"/>
        /// <stable>ICU 2.6</stable>
        [Obsolete("ICU 53 Set MaxVariable instead.")]
        public override int VariableTop
        {
            get
            {
                return (int)settings.ReadOnly.VariableTop;
            }
            set
            {
                CheckNotFrozen();
                InternalSetVariableTop(value & 0xffffffffL);
            }
        }

        private void InternalSetVariableTop(long varTop)
        {
            if (varTop != settings.ReadOnly.VariableTop)
            {
                // Pin the variable top to the end of the reordering group which contains it.
                // Only a few special groups are supported.
                int group = data.GetGroupForPrimary(varTop);
                if (group < ReorderCodes.First || ReorderCodes.Currency < group)
                {
                    throw new ArgumentException("The variable top must be a primary weight in " +
                            "the space/punctuation/symbols/currency symbols range");
                }
                long v = data.GetLastPrimaryForGroup(group);
                Debug.Assert(v != 0 && v >= varTop);
                varTop = v;
                if (varTop != settings.ReadOnly.VariableTop)
                {
                    CollationSettings ownedSettings = GetOwnedSettings();
                    ownedSettings.SetMaxVariable(group - ReorderCodes.First,
                            GetDefaultSettings().Options);
                    ownedSettings.VariableTop = varTop;
                    SetFastLatinOptions(ownedSettings);
                }
            }
        }

        /// <summary>
        /// <icu/>Gets or sets the numeric collation value. When numeric collation is turned on, this Collator generates a
        /// collation key for the numeric value of substrings of digits.
        /// <para/>
        /// This is a way to get '100' to sort AFTER '2'. Note that the longest
        /// digit substring that can be treated as a single unit is
        /// 254 digits (not counting leading zeros). If a digit substring is
        /// longer than that, the digits beyond the limit will be treated as a
        /// separate digit substring.
        /// <para/>
        /// A "digit" in this sense is a code point with General_Category=Nd,
        /// which does not include circled numbers, roman numerals, etc.
        /// Only a contiguous digit substring is considered, that is,
        /// non-negative integers without separators.
        /// There is no support for plus/minus signs, decimals, exponents, etc.
        /// </summary>
        /// <seealso cref="SetNumericCollationToDefault()"/>
        /// <stable>ICU 2.8</stable>
        public bool IsNumericCollation
        {
            get
            {
                return (settings.ReadOnly.Options & CollationSettings.Numeric) != 0;
            }
            set
            {
                CheckNotFrozen();
                // sort substrings of digits as numbers
                if (value == IsNumericCollation) { return; }
                CollationSettings ownedSettings = GetOwnedSettings();
                ownedSettings.SetFlag(CollationSettings.Numeric, value);
                SetFastLatinOptions(ownedSettings);
            }
        }

        /// <summary>
        /// Sets the reordering codes for this collator.
        /// Collation reordering allows scripts and some other groups of characters
        /// to be moved relative to each other. This reordering is done on top of
        /// the DUCET/CLDR standard collation order. Reordering can specify groups to be placed
        /// at the start and/or the end of the collation order. These groups are specified using
        /// <see cref="Globalization.UScript"/> codes and <see cref="ReorderCodes"/> entries.
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
        /// <exception cref="ArgumentException">If the reordering codes are malformed in any way (e.g. duplicates, multiple reset codes, overlapping equivalent scripts).</exception>
        /// <seealso cref="GetReorderCodes"/>
        /// <seealso cref="Collator.GetEquivalentReorderCodes(int)"/>
        /// <seealso cref="ReorderCodes"/>
        /// <seealso cref="Globalization.UScript"/>
        /// <stable>ICU 4.8</stable>
        public override void SetReorderCodes(params int[] order)
        {
            CheckNotFrozen();
            int length = (order != null) ? order.Length : 0;
            if (length == 1 && order[0] == ReorderCodes.None)
            {
                length = 0;
            }
            if (length == 0 ?
                    settings.ReadOnly.ReorderCodes.Length == 0 :
                    ArrayEqualityComparer<int>.OneDimensional.Equals(order, settings.ReadOnly.ReorderCodes))
            {
                return;
            }
            CollationSettings defaultSettings = GetDefaultSettings();
            if (length == 1 && order[0] == ReorderCodes.Default)
            {
                if (settings.ReadOnly != defaultSettings)
                {
                    CollationSettings ownedSettings2 = GetOwnedSettings();
                    ownedSettings2.CopyReorderingFrom(defaultSettings);
                    SetFastLatinOptions(ownedSettings2);
                }
                return;
            }
            CollationSettings ownedSettings = GetOwnedSettings();
            if (length == 0)
            {
                ownedSettings.ResetReordering();
            }
            else
            {
                ownedSettings.SetReordering(data, (int[])order.Clone());
            }
            SetFastLatinOptions(ownedSettings);
        }

        private void SetFastLatinOptions(CollationSettings ownedSettings)
        {
            ownedSettings.FastLatinOptions = CollationFastLatin.GetOptions(
                    data, ownedSettings, ownedSettings.FastLatinPrimaries);
        }

        // public getters --------------------------------------------------------

        /// <summary>
        /// Gets the collation tailoring rules for this <see cref="RuleBasedCollator"/>.
        /// Equivalent to string <c>GetRules(false)</c>.
        /// </summary>
        /// <returns>The collation tailoring rules.</returns>
        /// <stable>ICU 2.8</stable>
        public string GetRules()
        {
            return tailoring.GetRules();
        }

        /// <summary>
        /// Returns current rules.
        /// The <paramref name="fullrules"/> argument defines whether full rules (root collation + tailored) rules are returned
        /// or just the tailoring.
        /// <para/>
        /// The root collation rules are an <i>approximation</i> of the root collator's sort order.
        /// They are almost never used or useful at runtime and can be removed from the data.
        /// See <a href="http://userguide.icu-project.org/collation/customization#TOC-Building-on-Existing-Locales">User Guide:
        /// Collation Customization, Building on Existing Locales</a>
        /// <para/>
        /// <see cref="GetRules()"/> should normally be used instead.
        /// </summary>
        /// <param name="fullrules"><c>true</c> if the rules that defines the full set of collation order is required, otherwise <c>false</c> for
        /// returning only the tailored rules.</param>
        /// <returns>The current rules that define this <see cref="Collator"/>.</returns>
        /// <seealso cref="GetRules()"/>
        /// <seealso cref="GetRules(bool)"/>
        /// <stable>ICU 2.6</stable>
        public string GetRules(bool fullrules)
        {
            if (!fullrules)
            {
                return tailoring.GetRules();
            }
            return CollationLoader.RootRules + tailoring.GetRules();
        }

        /// <summary>
        /// Get a <see cref="UnicodeSet"/> that contains all the characters and sequences tailored in this collator.
        /// </summary>
        /// <returns>A pointer to a <see cref="UnicodeSet"/> object containing all the code points and sequences that may sort differently
        /// than in the root collator.</returns>
        /// <stable>ICU 2.4</stable>
        public override UnicodeSet GetTailoredSet()
        {
            UnicodeSet tailored = new UnicodeSet();
            if (data.Base != null)
            {
                new TailoredSet(tailored).ForData(data);
            }
            return tailored;
        }

        /// <summary>
        /// Gets unicode sets containing contractions and/or expansions of a collator.
        /// </summary>
        /// <param name="contractions">If not null, set to contain contractions.</param>
        /// <param name="expansions">If not null, set to contain expansions.</param>
        /// <param name="addPrefixes">Add the prefix contextual elements to contractions.</param>
        /// <exception cref="Exception">Throws an exception if any errors occurs.</exception>
        /// <stable>ICU 3.4</stable>
        public void GetContractionsAndExpansions(UnicodeSet contractions, UnicodeSet expansions, bool addPrefixes)
        {
            if (contractions != null)
            {
                contractions.Clear();
            }
            if (expansions != null)
            {
                expansions.Clear();
            }
            new ContractionsAndExpansions(contractions, expansions, null, addPrefixes).ForData(data);
        }

        /// <summary>
        /// Adds the contractions that start with character <paramref name="c"/> to the <paramref name="set"/>.
        /// Ignores prefixes. Used by <see cref="AlphabeticIndex{T}"/>.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="set"></param>
        [Obsolete("This API is ICU internal only.")]
        internal void InternalAddContractions(int c, UnicodeSet set) // ICU4N specific - marked internal, since the functionality is obsolete
        {
            new ContractionsAndExpansions(set, null, null, false).ForCodePoint(data, c);
        }

        /// <summary>
        /// Get a <see cref="CollationKey"/> for the argument <see cref="string"/> <paramref name="source"/> from this <see cref="RuleBasedCollator"/>.
        /// <para/>
        /// General recommendation: <br/>
        /// If comparison are to be done to the same <see cref="string"/> multiple times, it would be more efficient to generate
        /// <see cref="CollationKey"/>s for the <see cref="string"/> and use <see cref="CollationKey.CompareTo(CollationKey)"/> for the comparisons. If the each
        /// <see cref="string"/>s are compared to only once, using the method <see cref="RuleBasedCollator.Compare(string, string)"/> will have a better
        /// performance.
        /// <para/>
        /// See <see cref="RuleBasedCollator"/> for an explanation about <see cref="CollationKey"/>s.
        /// </summary>
        /// <param name="source">The text <see cref="string"/> to be transformed into a collation key.</param>
        /// <returns>The <see cref="CollationKey"/> for the given <see cref="string"/> based on this <see cref="RuleBasedCollator"/>'s collation rules. If the source
        /// <see cref="string"/> is null, a null <see cref="CollationKey"/> is returned.</returns>
        /// <seealso cref="CollationKey"/>
        /// <seealso cref="Compare(string, string)"/>
        /// <seealso cref="GetRawCollationKey(string, RawCollationKey)"/>
        /// <stable>ICU 2.8</stable>
        public override CollationKey GetCollationKey(string source)
        {
            if (source == null)
            {
                return null;
            }
            CollationBuffer buffer = null;
            try
            {
                buffer = GetCollationBuffer();
                return GetCollationKey(source, buffer);
            }
            finally
            {
                ReleaseCollationBuffer(buffer);
            }
        }

        private CollationKey GetCollationKey(string source, CollationBuffer buffer)
        {
            // ICU4N Port Note: using the System.Globalization.SortKey was considered as an option, but
            // since its constructor is internal and using Reflection to set the internal
            // properties is an ugly solution (not to mention, there are differnt variable
            // names depending on .NET Framework/Mono/.NET Standard and the class is missing 
            // entirely from .NET Standard 1.x), this was not done.
            // Although it could work, the ICU documentation clearly states that
            // "collation keys cannot be compared with other collator implementations",
            // so reusing SortKey would just be confusing.
            buffer.RawCollationKey = GetRawCollationKey(source.AsCharSequence(), buffer.RawCollationKey, buffer);
            return new CollationKey(source, buffer.RawCollationKey);
        }

        /// <summary>
        /// Gets the simpler form of a <see cref="CollationKey"/> for the <see cref="string"/> <paramref name="source"/> following the rules of this <see cref="Collator"/> and stores the
        /// result into the user provided argument key. If key has a internal byte array of length that's too small for the
        /// result, the internal byte array will be grown to the exact required size.
        /// </summary>
        /// <param name="source">The text <see cref="string"/> to be transformed into a <see cref="RawCollationKey"/>.</param>
        /// <param name="key">Output <see cref="RawCollationKey"/> to store results.</param>
        /// <returns>If key is null, a new instance of <see cref="RawCollationKey"/> will be created and returned, otherwise the user
        /// provided key will be returned.</returns>
        /// <seealso cref="GetCollationKey(string)"/>
        /// <seealso cref="GetCollationKey(string, CollationBuffer)"/>
        /// <seealso cref="Compare(string, string)"/>
        /// <seealso cref="RawCollationKey"/>
        /// <stable>ICU 2.8</stable>
        public override RawCollationKey GetRawCollationKey(string source, RawCollationKey key)
        {
            if (source == null)
            {
                return null;
            }
            CollationBuffer buffer = null;
            try
            {
                buffer = GetCollationBuffer();
                return GetRawCollationKey(source.AsCharSequence(), key, buffer);
            }
            finally
            {
                ReleaseCollationBuffer(buffer);
            }
        }

        private sealed class CollationKeyByteSink : SortKeyByteSink
        {
            internal CollationKeyByteSink(RawCollationKey key)
                : base(key.Bytes)
            {
                key_ = key;
            }

            protected override void AppendBeyondCapacity(byte[] bytes, int start, int n, int length)
            {
                // n > 0 && appended_ > capacity_
                if (Resize(n, length))
                {
                    System.Array.Copy(bytes, start, m_buffer, length, n);
                }
            }

            protected override bool Resize(int appendCapacity, int length)
            {
                int newCapacity = 2 * m_buffer.Length;
                int altCapacity = length + 2 * appendCapacity;
                if (newCapacity < altCapacity)
                {
                    newCapacity = altCapacity;
                }
                if (newCapacity < 200)
                {
                    newCapacity = 200;
                }
                // Do not call key_.ensureCapacity(newCapacity) because we do not
                // keep key_.size in sync with appended_.
                // We only set it when we are done.
                byte[] newBytes = new byte[newCapacity];
                System.Array.Copy(m_buffer, 0, newBytes, 0, length);
                m_buffer = key_.Bytes = newBytes;
                return true;
            }

            private RawCollationKey key_;

            public RawCollationKey Key { get { return key_; } }
        }

        private RawCollationKey GetRawCollationKey(ICharSequence source, RawCollationKey key, CollationBuffer buffer)
        {
            if (key == null)
            {
                key = new RawCollationKey(SimpleKeyLengthEstimate(source));
            }
            else if (key.Bytes == null)
            {
                key.Bytes = new byte[SimpleKeyLengthEstimate(source)];
            }
            CollationKeyByteSink sink = new CollationKeyByteSink(key);
            WriteSortKey(source, sink, buffer);
            key.Length = sink.NumberOfBytesAppended;
            return key;
        }

        private int SimpleKeyLengthEstimate(ICharSequence source)
        {
            return 2 * source.Length + 10;
        }

        private void WriteSortKey(ICharSequence s, CollationKeyByteSink sink, CollationBuffer buffer)
        {
            bool numeric = settings.ReadOnly.IsNumeric;
            if (settings.ReadOnly.DontCheckFCD)
            {
                buffer.LeftUTF16CollIter.SetText(numeric, s, 0);
                CollationKeys.WriteSortKeyUpToQuaternary(
                        buffer.LeftUTF16CollIter, data.CompressibleBytes, settings.ReadOnly,
                        sink, CollationSortKeyLevel.Primary,
                        CollationKeys.SimpleLevelFallback, true);
            }
            else
            {
                buffer.LeftFCDUTF16Iter.SetText(numeric, s, 0);
                CollationKeys.WriteSortKeyUpToQuaternary(
                        buffer.LeftFCDUTF16Iter, data.CompressibleBytes, settings.ReadOnly,
                        sink, CollationSortKeyLevel.Primary,
                        CollationKeys.SimpleLevelFallback, true);
            }
            if (settings.ReadOnly.Strength == CollationStrength.Identical)
            {
                WriteIdenticalLevel(s, sink);
            }
            sink.Append(Collation.TerminatorByte);
        }

        private void WriteIdenticalLevel(ICharSequence s, CollationKeyByteSink sink)
        {
            // NFD quick check
            int nfdQCYesLimit = data.NfcImpl.Decompose(s, 0, s.Length, null);
            sink.Append(Collation.LevelSeparatorByte);
            // Sync the ByteArrayWrapper size with the key length.
            sink.Key.Length = sink.NumberOfBytesAppended;
            int prev = 0;
            if (nfdQCYesLimit != 0)
            {
                prev = BOCSU.WriteIdenticalLevelRun(prev, s, 0, nfdQCYesLimit, sink.Key);
            }
            // Is there non-NFD text?
            if (nfdQCYesLimit < s.Length)
            {
                int destLengthEstimate = s.Length - nfdQCYesLimit;
                StringBuilderCharSequence nfd = new StringBuilderCharSequence(new StringBuilder());
                data.NfcImpl.Decompose(s, nfdQCYesLimit, s.Length, nfd.Value, destLengthEstimate);
                BOCSU.WriteIdenticalLevelRun(prev, nfd, 0, nfd.Length, sink.Key);
            }
            // Sync the key with the buffer again which got bytes appended and may have been reallocated.
            sink.SetBufferAndAppended(sink.Key.Bytes, sink.Key.Length);
        }

        /// <summary>
        /// Returns the CEs for the string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <internal>For tests &amp; tools.</internal>
        [Obsolete("This API is ICU internal only.")]
        internal long[] InternalGetCEs(ICharSequence str) // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            CollationBuffer buffer = null;
            try
            {
                buffer = GetCollationBuffer();
                bool numeric = settings.ReadOnly.IsNumeric;
                CollationIterator iter;
                if (settings.ReadOnly.DontCheckFCD)
                {
                    buffer.LeftUTF16CollIter.SetText(numeric, str, 0);
                    iter = buffer.LeftUTF16CollIter;
                }
                else
                {
                    buffer.LeftFCDUTF16Iter.SetText(numeric, str, 0);
                    iter = buffer.LeftFCDUTF16Iter;
                }
                int length = iter.FetchCEs() - 1;
                Debug.Assert(length >= 0 && iter.GetCE(length) == Collation.NoCE);
                long[] ces = new long[length];
                System.Array.Copy(iter.GetCEs(), 0, ces, 0, length);
                return ces;
            }
            finally
            {
                ReleaseCollationBuffer(buffer);
            }
        }

        // ICU4N specific property getters and setters combined

        /// <summary>
        /// Retrieves the reordering codes for this collator.
        /// These reordering codes are a combination of <see cref="Globalization.UScript"/> codes and <see cref="ReorderCodes"/>.
        /// </summary>
        /// <returns>A copy of the reordering codes for this collator;
        /// if none are set then returns an empty array.</returns>
        /// <seealso cref="SetReorderCodes(int[])"/>
        /// <stable>ICU 4.8</stable>
        public override int[] GetReorderCodes()
        {
            return (int[])settings.ReadOnly.ReorderCodes.Clone();
        }

        // public other methods -------------------------------------------------

        /// <summary>
        /// Compares the equality of two <see cref="RuleBasedCollator"/> objects. <see cref="RuleBasedCollator"/> objects are equal if they have the same
        /// collation (sorting &amp; searching) behavior.
        /// </summary>
        /// <param name="obj">The <see cref="RuleBasedCollator"/> to compare to.</param>
        /// <returns><c>true</c> if this <see cref="RuleBasedCollator"/> has exactly the same collation behavior as <paramref name="obj"/>, <c>false</c> otherwise.</returns>
        /// <stable>ICU 2.8</stable>
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (!base.Equals(obj))
            {
                return false;
            }
            RuleBasedCollator o = (RuleBasedCollator)obj;
            if (!settings.ReadOnly.Equals(o.settings.ReadOnly)) { return false; }
            if (data == o.data) { return true; }
            bool thisIsRoot = data.Base == null;
            bool otherIsRoot = o.data.Base == null;
            Debug.Assert(!thisIsRoot || !otherIsRoot);  // otherwise their data pointers should be ==
            if (thisIsRoot != otherIsRoot) { return false; }
            string theseRules = tailoring.GetRules();
            string otherRules = o.tailoring.GetRules();
            if ((thisIsRoot || theseRules.Length != 0) &&
                    (otherIsRoot || otherRules.Length != 0))
            {
                // Shortcut: If both collators have valid rule strings, then compare those.
                if (theseRules.Equals(otherRules)) { return true; }
            }
            // Different rule strings can result in the same or equivalent tailoring.
            // The rule strings are optional in ICU resource bundles, although included by default.
            // cloneBinary() drops the rule string.
            UnicodeSet thisTailored = GetTailoredSet();
            UnicodeSet otherTailored = o.GetTailoredSet();
            if (!thisTailored.Equals(otherTailored)) { return false; }
            // For completeness, we should compare all of the mappings;
            // or we should create a list of strings, sort it with one collator,
            // and check if both collators compare adjacent strings the same
            // (order & strength, down to quaternary); or similar.
            // Testing equality of collators seems unusual.
            return true;
        }

        /// <summary>
        /// Generates a unique hash code for this <see cref="RuleBasedCollator"/>.
        /// </summary>
        /// <returns>The unique hash code for this <see cref="Collator"/>.</returns>
        /// <stable>ICU 2.8</stable>
        public override int GetHashCode()
        {
            int h = settings.ReadOnly.GetHashCode();
            if (data.Base == null) { return h; }  // root collator
                                                  // Do not rely on the rule string, see comments in operator==().
            UnicodeSet set = GetTailoredSet();
            UnicodeSetIterator iter = new UnicodeSetIterator(set);
            while (iter.Next() && iter.Codepoint != UnicodeSetIterator.IsString)
            {
                h ^= data.GetCE32(iter.Codepoint);
            }
            return h;
        }

        /// <summary>
        /// Compares the source text <see cref="string"/> to the target text <see cref="string"/> according to the collation rules, strength and
        /// decomposition mode for this <see cref="RuleBasedCollator"/>. Returns an <see cref="int"/> less than, equal to or greater than zero
        /// depending on whether the source <see cref="string"/> is less than, equal to or greater than the target <see cref="string"/>. See the <see cref="Collator"/>
        /// documentation for an example of use.
        /// <para/>
        /// General recommendation: <br/>
        /// If comparison are to be done to the same <see cref="string"/> multiple times, it would be more efficient to generate
        /// <see cref="CollationKey"/>s for the <see cref="string"/>s and use <see cref="CollationKey.CompareTo(CollationKey)"/> for the comparisons. If speed
        /// performance is critical and object instantiation is to be reduced, further optimization may be achieved by
        /// generating a simpler key of the form <see cref="RawCollationKey"/> and reusing this <see cref="RawCollationKey"/> object with the method
        /// <see cref="RuleBasedCollator.GetRawCollationKey(string, RawCollationKey)"/>. Internal byte representation can be directly accessed via <see cref="RawCollationKey"/>
        /// and stored for future use. Like <see cref="CollationKey"/>, <see cref="RawCollationKey"/> provides a method <see cref="RawCollationKey.CompareTo(ByteArrayWrapper)"/> for key
        /// comparisons. If the each <see cref="string"/>s are compared to only once, using the method <see cref="RuleBasedCollator.Compare(string, string)"/>
        /// will have a better performance.
        /// </summary>
        /// <param name="source">The source text <see cref="string"/>.</param>
        /// <param name="target">The target text <see cref="string"/>.</param>
        /// <returns>Returns an integer value. Value is less than zero if source is less than target, value is zero if source
        /// and target are equal, value is greater than zero if source is greater than target.</returns>
        /// <seealso cref="CollationKey"/>
        /// <seealso cref="GetCollationKey(string)"/>
        /// <stable>ICU 2.8</stable>
        public override int Compare(string source, string target)
        {
#pragma warning disable 612, 618
            return DoCompare(source.AsCharSequence(), target.AsCharSequence());
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Abstract iterator for identical-level string comparisons.
        /// Returns FCD code points and handles temporary switching to NFD.
        /// <para/>
        /// As with <see cref="CollationIterator"/>,
        /// .NET <see cref="NFDIterator"/> instances are partially constructed and cached,
        /// and completed when reset for use.
        /// C++ NFDIterator instances are stack-allocated.
        /// </summary>
        private abstract class NFDIterator
        {
            /// <summary>
            /// Partial constructor, must call <see cref="Reset()"/>.
            /// </summary>
            internal NFDIterator() { }
            internal void Reset()
            {
                index = -1;
            }

            /// <summary>
            /// Returns the next code point from the internal normalization buffer,
            /// or else the next text code point.
            /// Returns -1 at the end of the text.
            /// </summary>
            internal int NextCodePoint()
            {
                if (index >= 0)
                {
                    if (index == decomp.Length)
                    {
                        index = -1;
                    }
                    else
                    {
                        int c = Character.CodePointAt(decomp, index);
                        index += Character.CharCount(c);
                        return c;
                    }
                }
                return NextRawCodePoint();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="nfcImpl"></param>
            /// <param name="c">The last code point returned by <see cref="NextCodePoint()"/> or <see cref="NextDecomposedCodePoint(Normalizer2Impl, int)"/>.</param>
            /// <returns>The first code point in <paramref name="c"/>'s decomposition, or <paramref name="c"/> itself if it was decomposed already or if it does not decompose.</returns>
            internal int NextDecomposedCodePoint(Normalizer2Impl nfcImpl, int c)
            {
                if (index >= 0) { return c; }
                decomp = nfcImpl.GetDecomposition(c);
                if (decomp == null) { return c; }
                c = Character.CodePointAt(decomp, 0);
                index = Character.CharCount(c);
                return c;
            }

            /// <summary>
            /// Returns the next text code point in FCD order.
            /// Returns -1 at the end of the text.
            /// </summary>
            protected abstract int NextRawCodePoint();

            private string decomp;
            private int index;
        }

        private class UTF16NFDIterator : NFDIterator
        {
            internal UTF16NFDIterator() { }
            internal void SetText(ICharSequence seq, int start)
            {
                Reset();
                s = seq;
                pos = start;
            }

            protected override int NextRawCodePoint()
            {
                if (pos == s.Length) { return Collation.SentinelCodePoint; }
                int c = Character.CodePointAt(s, pos);
                pos += Character.CharCount(c);
                return c;
            }

            protected ICharSequence s;
            protected int pos;
        }

        private sealed class FCDUTF16NFDIterator : UTF16NFDIterator
        {
            internal FCDUTF16NFDIterator() { }
            internal void SetText(Normalizer2Impl nfcImpl, ICharSequence seq, int start)
            {
                Reset();
                int spanLimit = nfcImpl.MakeFCD(seq, start, seq.Length, null);
                if (spanLimit == seq.Length)
                {
                    s = seq;
                    pos = start;
                }
                else
                {
                    if (str is null)
                    {
                        str = new StringBuilderCharSequence(new StringBuilder());
                    }
                    else
                    {
                        str.Value.Length = 0;
                    }
                    str.Value.Append(seq, start, spanLimit - start); // ICU4N: Corrected 3rd parameter
                    ReorderingBuffer buffer = new ReorderingBuffer(nfcImpl, str.Value, seq.Length - start);
                    nfcImpl.MakeFCD(seq, spanLimit, seq.Length, buffer);
                    s = str;
                    pos = 0;
                }
            }

            private StringBuilderCharSequence str;
        }

        private static int CompareNFDIter(Normalizer2Impl nfcImpl, NFDIterator left, NFDIterator right)
        {
            for (; ; )
            {
                // Fetch the next FCD code point from each string.
                int leftCp = left.NextCodePoint();
                int rightCp = right.NextCodePoint();
                if (leftCp == rightCp)
                {
                    if (leftCp < 0) { break; }
                    continue;
                }
                // If they are different, then decompose each and compare again.
                if (leftCp < 0)
                {
                    leftCp = -2;  // end of string
                }
                else if (leftCp == 0xfffe)
                {
                    leftCp = -1;  // U+FFFE: merge separator
                }
                else
                {
                    leftCp = left.NextDecomposedCodePoint(nfcImpl, leftCp);
                }
                if (rightCp < 0)
                {
                    rightCp = -2;  // end of string
                }
                else if (rightCp == 0xfffe)
                {
                    rightCp = -1;  // U+FFFE: merge separator
                }
                else
                {
                    rightCp = right.NextDecomposedCodePoint(nfcImpl, rightCp);
                }
                if (leftCp < rightCp) { return Collation.Less; }
                if (leftCp > rightCp) { return Collation.Greater; }
            }
            return Collation.Equal;
        }

        /// <summary>
        /// Compares two <see cref="ICharSequence"/>s.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal override int DoCompare(ICharSequence left, ICharSequence right) // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            if (left == right)
            {
                return Collation.Equal;
            }

            // Identical-prefix test.
            int equalPrefixLength = 0;
            for (; ; )
            {
                if (equalPrefixLength == left.Length)
                {
                    if (equalPrefixLength == right.Length) { return Collation.Equal; }
                    break;
                }
                else if (equalPrefixLength == right.Length ||
                        left[equalPrefixLength] != right[equalPrefixLength])
                {
                    break;
                }
                ++equalPrefixLength;
            }

            CollationSettings roSettings = settings.ReadOnly;
            bool numeric = roSettings.IsNumeric;
            if (equalPrefixLength > 0)
            {
                if ((equalPrefixLength != left.Length &&
                            data.IsUnsafeBackward(left[equalPrefixLength], numeric)) ||
                        (equalPrefixLength != right.Length &&
                            data.IsUnsafeBackward(right[equalPrefixLength], numeric)))
                {
                    // Identical prefix: Back up to the start of a contraction or reordering sequence.
                    while (--equalPrefixLength > 0 &&
                            data.IsUnsafeBackward(left[equalPrefixLength], numeric)) { }
                }
                // Notes:
                // - A longer string can compare equal to a prefix of it if only ignorables follow.
                // - With a backward level, a longer string can compare less-than a prefix of it.

                // Pass the actual start of each string into the CollationIterators,
                // plus the equalPrefixLength position,
                // so that prefix matches back into the equal prefix work.
            }

            int result;
            int fastLatinOptions = roSettings.FastLatinOptions;
            if (fastLatinOptions >= 0 &&
                    (equalPrefixLength == left.Length ||
                        left[equalPrefixLength] <= CollationFastLatin.LatinMax) &&
                    (equalPrefixLength == right.Length ||
                        right[equalPrefixLength] <= CollationFastLatin.LatinMax))
            {
                result = CollationFastLatin.CompareUTF16(data.FastLatinTable,
                                                          roSettings.FastLatinPrimaries,
                                                          fastLatinOptions,
                                                          left, right, equalPrefixLength);
            }
            else
            {
                result = CollationFastLatin.BailOutResult;
            }

            if (result == CollationFastLatin.BailOutResult)
            {
                CollationBuffer buffer = null;
                try
                {
                    buffer = GetCollationBuffer();
                    if (roSettings.DontCheckFCD)
                    {
                        buffer.LeftUTF16CollIter.SetText(numeric, left, equalPrefixLength);
                        buffer.RightUTF16CollIter.SetText(numeric, right, equalPrefixLength);
                        result = CollationCompare.CompareUpToQuaternary(
                                buffer.LeftUTF16CollIter, buffer.RightUTF16CollIter, roSettings);
                    }
                    else
                    {
                        buffer.LeftFCDUTF16Iter.SetText(numeric, left, equalPrefixLength);
                        buffer.RightFCDUTF16Iter.SetText(numeric, right, equalPrefixLength);
                        result = CollationCompare.CompareUpToQuaternary(
                                buffer.LeftFCDUTF16Iter, buffer.RightFCDUTF16Iter, roSettings);
                    }
                }
                finally
                {
                    ReleaseCollationBuffer(buffer);
                }
            }
            if (result != Collation.Equal || roSettings.Strength < CollationStrength.Identical)
            {
                return result;
            }

            CollationBuffer buffer2 = null;
            try
            {
                buffer2 = GetCollationBuffer();
                // Compare identical level.
                Normalizer2Impl nfcImpl = data.NfcImpl;
                if (roSettings.DontCheckFCD)
                {
                    buffer2.LeftUTF16NFDIter.SetText(left, equalPrefixLength);
                    buffer2.RightUTF16NFDIter.SetText(right, equalPrefixLength);
                    return CompareNFDIter(nfcImpl, buffer2.LeftUTF16NFDIter, buffer2.RightUTF16NFDIter);
                }
                else
                {
                    buffer2.LeftFCDUTF16NFDIter.SetText(nfcImpl, left, equalPrefixLength);
                    buffer2.RightFCDUTF16NFDIter.SetText(nfcImpl, right, equalPrefixLength);
                    return CompareNFDIter(nfcImpl, buffer2.LeftFCDUTF16NFDIter, buffer2.RightFCDUTF16NFDIter);
                }
            }
            finally
            {
                ReleaseCollationBuffer(buffer2);
            }
        }

        // package private constructors ------------------------------------------

        internal RuleBasedCollator(CollationTailoring t, ULocale vl)
        {
            data = t.Data;
            settings = (SharedObject.Reference<CollationSettings>)t.Settings.Clone();
            tailoring = t;
            validLocale = vl;
            actualLocaleIsSameAsValid = false;
        }

        private void AdoptTailoring(CollationTailoring t)
        {
            Debug.Assert(settings == null && data == null && tailoring == null);
            data = t.Data;
            settings = (SharedObject.Reference<CollationSettings>)t.Settings.Clone();
            tailoring = t;
            validLocale = t.ActualLocale;
            actualLocaleIsSameAsValid = false;
        }

        // package private methods -----------------------------------------------

        /// <summary>
        /// Tests whether a character is "unsafe" for use as a collation starting point.
        /// </summary>
        /// <param name="c">Code point or code unit.</param>
        /// <returns><c>true</c> if <paramref name="c"/> is unsafe.</returns>
        /// <seealso cref="CollationElementIterator.SetOffset(int)"/>
        internal bool IsUnsafe(int c)
        {
            return data.IsUnsafeBackward(c, settings.ReadOnly.IsNumeric);
        }

        /// <summary>
        /// Frozen state of the collator.
        /// </summary>
        private object frozenLock; // ICU4N: Using object/Monitor to replace ReentrantLock

        private sealed class CollationBuffer
        {
            internal CollationBuffer(CollationData data)
            {
                LeftUTF16CollIter = new UTF16CollationIterator(data);
                RightUTF16CollIter = new UTF16CollationIterator(data);
                LeftFCDUTF16Iter = new FCDUTF16CollationIterator(data);
                RightFCDUTF16Iter = new FCDUTF16CollationIterator(data);
                LeftUTF16NFDIter = new UTF16NFDIterator();
                RightUTF16NFDIter = new UTF16NFDIterator();
                LeftFCDUTF16NFDIter = new FCDUTF16NFDIterator();
                RightFCDUTF16NFDIter = new FCDUTF16NFDIterator();
            }

            public UTF16CollationIterator LeftUTF16CollIter { get; private set; }
            public UTF16CollationIterator RightUTF16CollIter { get; private set; }
            public FCDUTF16CollationIterator LeftFCDUTF16Iter { get; private set; }
            public FCDUTF16CollationIterator RightFCDUTF16Iter { get; private set; }

            public UTF16NFDIterator LeftUTF16NFDIter { get; private set; }
            public UTF16NFDIterator RightUTF16NFDIter { get; private set; }
            public FCDUTF16NFDIterator LeftFCDUTF16NFDIter { get; private set; }
            public FCDUTF16NFDIterator RightFCDUTF16NFDIter { get; private set; }

            public RawCollationKey RawCollationKey { get; internal set; }


        }

        /// <summary>
        /// Get the version of this collator object.
        /// </summary>
        /// <returns>The version object associated with this collator.</returns>
        /// <stable>ICU 2.8</stable>
        public override VersionInfo GetVersion()
        {
            int version = tailoring.Version;
            int rtVersion = VersionInfo.CollationRuntimeVersion.Major;
            return VersionInfo.GetInstance(
                    (version.TripleShift(24)) + (rtVersion << 4) + (rtVersion >> 4),
                    ((version >> 16) & 0xff), ((version >> 8) & 0xff), (version & 0xff));
        }

        /// <summary>
        /// Get the UCA version of this collator object.
        /// </summary>
        /// <returns>The version object associated with this collator.</returns>
        /// <stable>ICU 2.8</stable>
        public override VersionInfo GetUCAVersion()
        {
            VersionInfo v = GetVersion();
            // Note: This is tied to how the current implementation encodes the UCA version
            // in the overall getVersion().
            // Alternatively, we could load the root collator and get at lower-level data from there.
            // Either way, it will reflect the input collator's UCA version only
            // if it is a known implementation.
            // (C++ comment) It would be cleaner to make this a virtual Collator method.
            // (In Java, it is virtual.)
            return VersionInfo.GetInstance(v.Minor >> 3, v.Minor & 7, v.Milli >> 6, 0);
        }

        private CollationBuffer collationBuffer;

        private CollationBuffer GetCollationBuffer()
        {
            if (IsFrozen)
            {
                Monitor.Enter(frozenLock); // ICU4N: Using object/Monitor to replace ReentrantLock
            }
            else if (collationBuffer == null)
            {
                collationBuffer = new CollationBuffer(data);
            }
            return collationBuffer;
        }

        private void ReleaseCollationBuffer(CollationBuffer buffer)
        {
            if (IsFrozen)
            {
                Monitor.Exit(frozenLock); // ICU4N: Using object/Monitor to replace ReentrantLock
            }
        }

        /// <summary>
        /// <icu/> Returns the locale that was used to create this object, or null.
        /// This may may differ from the locale requested at the time of
        /// this object's creation.  For example, if an object is created
        /// for locale <c>en_US_CALIFORNIA</c>, the actual data may be
        /// drawn from <c>en</c> (the <i>actual</i> locale), and
        /// <c>en_US</c> may be the most specific locale that exists (the
        /// <i>valid</i> locale).
        /// <para/>
        /// Note: This method will be implemented in ICU 3.0; ICU 2.8
        /// contains a partial preview implementation.  The * <i>actual</i>
        /// locale is returned correctly, but the <i>valid</i> locale is
        /// not, in most cases.
        /// </summary>
        /// <param name="type">type of information requested, either 
        /// <see cref="ULocale.VALID_LOCALE"/> or <see cref="ULocale.ACTUAL_LOCALE"/>.</param>
        /// <returns>
        /// the information specified by <i>type</i>, or null if
        /// this object was not constructed from locale data.
        /// </returns>
        /// <seealso cref="ULocale"/>
        /// <seealso cref="ULocale.VALID_LOCALE"/>
        /// <seealso cref="ULocale.ACTUAL_LOCALE"/>
        /// <draft>ICU 53 (retain)</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public override ULocale GetLocale(ULocale.Type type)
        {
            if (type == ULocale.ACTUAL_LOCALE)
            {
                return actualLocaleIsSameAsValid ? validLocale : tailoring.ActualLocale;
            }
            else if (type == ULocale.VALID_LOCALE)
            {
                return validLocale;
            }
            else
            {
                throw new ArgumentException("unknown ULocale.Type " + type);
            }
        }

        /// <summary>
        /// Set information about the locales that were used to create this
        /// object.  If the object was not constructed from locale data,
        /// both arguments should be set to null.  Otherwise, neither
        /// should be null.  The actual locale must be at the same level or
        /// less specific than the valid locale.  This method is intended
        /// for use by factories or other entities that create objects of
        /// this class.
        /// </summary>
        /// <param name="valid">the most specific locale containing any resource data, or null</param>
        /// <param name="actual">the locale containing data used to construct this object, or null</param>
        /// <seealso cref="ULocale"/>
        /// <seealso cref="ULocale.VALID_LOCALE"/>
        /// <seealso cref="ULocale.ACTUAL_LOCALE"/>
        internal override void SetLocale(ULocale valid, ULocale actual)
        {
            // This method is called
            // by other protected functions that checks and makes sure that
            // valid and actual are not null before passing
            Debug.Assert((valid == null) == (actual == null));
            // Another check we could do is that the actual locale is at
            // the same level or less specific than the valid locale.
            // TODO: Starting with Java 7, use Objects.equals(a, b).
            if (Utility.ObjectEquals(actual, tailoring.ActualLocale))
            {
                actualLocaleIsSameAsValid = false;
            }
            else
            {
                Debug.Assert(Utility.ObjectEquals(actual, valid));
                actualLocaleIsSameAsValid = true;
            }
            // Do not modify tailoring.actualLocale:
            // We cannot be sure that that would be thread-safe.
            validLocale = valid;
        }

        internal CollationData data;
        internal SharedObject.Reference<CollationSettings> settings;  // reference-counted
        internal CollationTailoring tailoring;  // C++: reference-counted
        private ULocale validLocale;
        // Note: No need in Java to track which attributes have been set explicitly.
        // int or EnumSet  explicitlySetAttributes;

        private bool actualLocaleIsSameAsValid;
    }
}
