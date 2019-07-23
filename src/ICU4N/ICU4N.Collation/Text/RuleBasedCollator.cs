using ICU4N.Impl.Coll;
using ICU4N.Impl;
using ICU4N.Support;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using SortKeyByteSink = ICU4N.Impl.Coll.CollationKeys.SortKeyByteSink;
using ReorderingBuffer = ICU4N.Impl.Normalizer2Impl.ReorderingBuffer;
using ICU4N.Support.Threading;

namespace ICU4N.Text
{
    public sealed class RuleBasedCollator : Collator
    {
        // public constructors ---------------------------------------------------

        /**
         * <p>
         * Constructor that takes the argument rules for customization.
         * The collator will be based on the CLDR root collation, with the
         * attributes and re-ordering of the characters specified in the argument rules.
         * <p>
         * See the User Guide's section on <a href="http://userguide.icu-project.org/collation/customization">
         * Collation Customization</a> for details on the rule syntax.
         *
         * @param rules
         *            the collation rules to build the collation table from.
         * @exception ParseException
         *                and IOException thrown. ParseException thrown when argument rules have an invalid syntax.
         *                IOException thrown when an error occurred while reading internal data.
         * @stable ICU 2.8
         */
        public RuleBasedCollator(string rules)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules), "Collation rules can not be null");
            }
            validLocale = ULocale.ROOT;
            InternalBuildTailoring(rules);
        }

        /**
         * Implements from-rule constructors.
         * @param rules rule string
         * @throws Exception
         */
        private void InternalBuildTailoring(string rules)
        {
            // ICU4N TODO: Seems like reflection is overkill here.

            CollationTailoring @base = CollationRoot.Root;
            // Most code using Collator does not need to build a Collator from rules.
            // By using reflection, most code will not have a static dependency on the builder code.
            // CollationBuilder builder = new CollationBuilder(base);
            Assembly classLoader = GetType().GetTypeInfo().Assembly; // ClassLoaderUtil.getClassLoader(GetType());
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

        /**
         * Clones the RuleBasedCollator
         * 
         * @return a new instance of this RuleBasedCollator object
         * @stable ICU 2.8
         */
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

        /**
         * Return a CollationElementIterator for the given String.
         * 
         * @see CollationElementIterator
         * @stable ICU 2.8
         */
        public CollationElementIterator GetCollationElementIterator(string source)
        {
            InitMaxExpansions();
            return new CollationElementIterator(source, this);
        }

        /**
         * Return a CollationElementIterator for the given CharacterIterator. The source iterator's integrity will be
         * preserved since a new copy will be created for use.
         * 
         * @see CollationElementIterator
         * @stable ICU 2.8
         */
        public CollationElementIterator GetCollationElementIterator(CharacterIterator source)
        {
            InitMaxExpansions();
            CharacterIterator newsource = (CharacterIterator)source.Clone();
            return new CollationElementIterator(newsource, this);
        }

        /**
         * Return a CollationElementIterator for the given UCharacterIterator. The source iterator's integrity will be
         * preserved since a new copy will be created for use.
         * 
         * @see CollationElementIterator
         * @stable ICU 2.8
         */
        public CollationElementIterator GetCollationElementIterator(UCharacterIterator source)
        {
            InitMaxExpansions();
            return new CollationElementIterator(source, this);
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
        public override bool IsFrozen
        {
            get { return frozenLock != null; }
        }

        /**
         * Freezes the collator.
         * @return the collator itself.
         * @stable ICU 4.8
         */
        public override Collator Freeze()
        {
            if (!IsFrozen)
            {
                frozenLock = new ReentrantLock();
                if (collationBuffer == null)
                {
                    collationBuffer = new CollationBuffer(data);
                }
            }
            return this;
        }

        /**
         * Provides for the clone operation. Any clone is initially unfrozen.
         * @stable ICU 4.8
         */
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

        /**
         * Sets the Hiragana Quaternary mode to be on or off. When the Hiragana Quaternary mode is turned on, the collator
         * positions Hiragana characters before all non-ignorable characters in QUATERNARY strength. This is to produce a
         * correct JIS collation order, distinguishing between Katakana and Hiragana characters.
         *
         * <p>This attribute was an implementation detail of the CLDR Japanese tailoring.
         * Since ICU 50, this attribute is not settable any more via API functions.
         * Since CLDR 25/ICU 53, explicit quaternary relations are used
         * to achieve the same Japanese sort order.
         *
         * @param flag
         *            true if Hiragana Quaternary mode is to be on, false otherwise
         * @see #setHiraganaQuaternaryDefault
         * @see #isHiraganaQuaternary
         * @deprecated ICU 50 Implementation detail, cannot be set via API, was removed from implementation.
         */
        //[Obsolete("ICU 50 Implementation detail, cannot be set via API, was removed from implementation.")]
        //    public void SetHiraganaQuaternary(bool flag)
        //{
        //    CheckNotFrozen();
        //}

        [Obsolete("ICU 50 Implementation detail, cannot be set via API, was removed from implementation.")]
        public bool IsHiraganaQuaternary
        {
            get { return false; }
            set { CheckNotFrozen(); }
        }

        /**
         * Sets the Hiragana Quaternary mode to the initial mode set during construction of the RuleBasedCollator. See
         * setHiraganaQuaternary(boolean) for more details.
         *
         * <p>This attribute was an implementation detail of the CLDR Japanese tailoring.
         * Since ICU 50, this attribute is not settable any more via API functions.
         * Since CLDR 25/ICU 53, explicit quaternary relations are used
         * to achieve the same Japanese sort order.
         *
         * @see #setHiraganaQuaternary(boolean)
         * @see #isHiraganaQuaternary
         * @deprecated ICU 50 Implementation detail, cannot be set via API, was removed from implementation.
         */
        [Obsolete("ICU 50 Implementation detail, cannot be set via API, was removed from implementation.")]
        public void SetHiraganaQuaternaryDefault()
        {
            CheckNotFrozen();
        }

        /**
         * Sets whether uppercase characters sort before lowercase characters or vice versa, in strength TERTIARY. The
         * default mode is false, and so lowercase characters sort before uppercase characters. If true, sort upper case
         * characters first.
         * 
         * @param upperfirst
         *            true to sort uppercase characters before lowercase characters, false to sort lowercase characters
         *            before uppercase characters
         * @see #isLowerCaseFirst
         * @see #isUpperCaseFirst
         * @see #setLowerCaseFirst
         * @see #setCaseFirstDefault
         * @stable ICU 2.8
         */
        //public void setUpperCaseFirst(bool upperfirst)
        //{
        //    CheckNotFrozen();
        //    if (upperfirst == isUpperCaseFirst()) { return; }
        //    CollationSettings ownedSettings = GetOwnedSettings();
        //    ownedSettings.setCaseFirst(upperfirst ? CollationSettings.CASE_FIRST_AND_UPPER_MASK : 0);
        //    setFastLatinOptions(ownedSettings);
        //}

        public bool IsUpperCaseFirst
        {
            get
            {
                return (settings.ReadOnly.CaseFirst == CollationSettings.CASE_FIRST_AND_UPPER_MASK);
            }
            set
            {
                CheckNotFrozen();
                if (value == IsUpperCaseFirst) { return; }
                CollationSettings ownedSettings = GetOwnedSettings();
                ownedSettings.CaseFirst = value ? CollationSettings.CASE_FIRST_AND_UPPER_MASK : 0;
                SetFastLatinOptions(ownedSettings);
            }
        }

        /**
         * Sets the orders of lower cased characters to sort before upper cased characters, in strength TERTIARY. The
         * default mode is false. If true is set, the RuleBasedCollator will sort lower cased characters before the upper
         * cased ones. Otherwise, if false is set, the RuleBasedCollator will ignore case preferences.
         * 
         * @param lowerfirst
         *            true for sorting lower cased characters before upper cased characters, false to ignore case
         *            preferences.
         * @see #isLowerCaseFirst
         * @see #isUpperCaseFirst
         * @see #setUpperCaseFirst
         * @see #setCaseFirstDefault
         * @stable ICU 2.8
         */
        //        public void setLowerCaseFirst(boolean lowerfirst)
        //{
        //    CheckNotFrozen();
        //    if (lowerfirst == isLowerCaseFirst()) { return; }
        //    CollationSettings ownedSettings = GetOwnedSettings();
        //    ownedSettings.setCaseFirst(lowerfirst ? CollationSettings.CASE_FIRST : 0);
        //    setFastLatinOptions(ownedSettings);
        //}

        public bool IsLowerCaseFirst
        {
            get
            {
                return (settings.ReadOnly.CaseFirst == CollationSettings.CASE_FIRST);
            }
            set
            {
                CheckNotFrozen();
                if (value == IsLowerCaseFirst) { return; }
                CollationSettings ownedSettings = GetOwnedSettings();
                ownedSettings.CaseFirst = value ? CollationSettings.CASE_FIRST : 0;
                SetFastLatinOptions(ownedSettings);
            }
        }

        /**
         * Sets the case first mode to the initial mode set during construction of the RuleBasedCollator. See
         * setUpperCaseFirst(boolean) and setLowerCaseFirst(boolean) for more details.
         * 
         * @see #isLowerCaseFirst
         * @see #isUpperCaseFirst
         * @see #setLowerCaseFirst(boolean)
         * @see #setUpperCaseFirst(boolean)
         * @stable ICU 2.8
         */
        public void SetCaseFirstDefault()
        {
            CheckNotFrozen();
            CollationSettings defaultSettings = GetDefaultSettings();
            if (settings.ReadOnly == defaultSettings) { return; }
            CollationSettings ownedSettings = GetOwnedSettings();
            ownedSettings.SetCaseFirstDefault(defaultSettings.Options);
            SetFastLatinOptions(ownedSettings);
        }

        /**
         * Sets the alternate handling mode to the initial mode set during construction of the RuleBasedCollator. See
         * setAlternateHandling(boolean) for more details.
         * 
         * @see #setAlternateHandlingShifted(boolean)
         * @see #isAlternateHandlingShifted()
         * @stable ICU 2.8
         */
        public void SetAlternateHandlingDefault()
        {
            CheckNotFrozen();
            CollationSettings defaultSettings = GetDefaultSettings();
            if (settings.ReadOnly == defaultSettings) { return; }
            CollationSettings ownedSettings = GetOwnedSettings();
            ownedSettings.SetAlternateHandlingDefault(defaultSettings.Options);
            SetFastLatinOptions(ownedSettings);
        }

        /**
         * Sets the case level mode to the initial mode set during construction of the RuleBasedCollator. See
         * setCaseLevel(boolean) for more details.
         * 
         * @see #setCaseLevel(boolean)
         * @see #isCaseLevel
         * @stable ICU 2.8
         */
        public void SetCaseLevelDefault()
        {
            CheckNotFrozen();
            CollationSettings defaultSettings = GetDefaultSettings();
            if (settings.ReadOnly == defaultSettings) { return; }
            CollationSettings ownedSettings = GetOwnedSettings();
            ownedSettings.SetFlagDefault(CollationSettings.CASE_LEVEL, defaultSettings.Options);
            SetFastLatinOptions(ownedSettings);
        }

        /**
         * Sets the decomposition mode to the initial mode set during construction of the RuleBasedCollator. See
         * setDecomposition(int) for more details.
         * 
         * @see #getDecomposition
         * @see #setDecomposition(int)
         * @stable ICU 2.8
         */
        public void SetDecompositionDefault()
        {
            CheckNotFrozen();
            CollationSettings defaultSettings = GetDefaultSettings();
            if (settings.ReadOnly == defaultSettings) { return; }
            CollationSettings ownedSettings = GetOwnedSettings();
            ownedSettings.SetFlagDefault(CollationSettings.CHECK_FCD, defaultSettings.Options);
            SetFastLatinOptions(ownedSettings);
        }

        /**
         * Sets the French collation mode to the initial mode set during construction of the RuleBasedCollator. See
         * setFrenchCollation(boolean) for more details.
         * 
         * @see #isFrenchCollation
         * @see #setFrenchCollation(boolean)
         * @stable ICU 2.8
         */
        public void SetFrenchCollationDefault()
        {
            CheckNotFrozen();
            CollationSettings defaultSettings = GetDefaultSettings();
            if (settings.ReadOnly == defaultSettings) { return; }
            CollationSettings ownedSettings = GetOwnedSettings();
            ownedSettings.SetFlagDefault(CollationSettings.BACKWARD_SECONDARY, defaultSettings.Options);
            SetFastLatinOptions(ownedSettings);
        }

        /**
         * Sets the collation strength to the initial mode set during the construction of the RuleBasedCollator. See
         * setStrength(int) for more details.
         * 
         * @see #setStrength(int)
         * @see #getStrength
         * @stable ICU 2.8
         */
        public void SetStrengthDefault()
        {
            CheckNotFrozen();
            CollationSettings defaultSettings = GetDefaultSettings();
            if (settings.ReadOnly == defaultSettings) { return; }
            CollationSettings ownedSettings = GetOwnedSettings();
            ownedSettings.SetStrengthDefault(defaultSettings.Options);
            SetFastLatinOptions(ownedSettings);
        }

        /**
         * Method to set numeric collation to its default value.
         *
         * @see #getNumericCollation
         * @see #setNumericCollation
         * @stable ICU 2.8
         */
        public void SetNumericCollationDefault()
        {
            CheckNotFrozen();
            CollationSettings defaultSettings = GetDefaultSettings();
            if (settings.ReadOnly == defaultSettings) { return; }
            CollationSettings ownedSettings = GetOwnedSettings();
            ownedSettings.SetFlagDefault(CollationSettings.NUMERIC, defaultSettings.Options);
            SetFastLatinOptions(ownedSettings);
        }

        /**
         * Sets the mode for the direction of SECONDARY weights to be used in French collation. The default value is false,
         * which treats SECONDARY weights in the order they appear. If set to true, the SECONDARY weights will be sorted
         * backwards. See the section on <a href="http://userguide.icu-project.org/collation/architecture">
         * French collation</a> for more information.
         * 
         * @param flag
         *            true to set the French collation on, false to set it off
         * @stable ICU 2.8
         * @see #isFrenchCollation
         * @see #setFrenchCollationDefault
         */
        //public void setFrenchCollation(bool flag)
        //{
        //    CheckNotFrozen();
        //    if (flag == isFrenchCollation()) { return; }
        //    CollationSettings ownedSettings = GetOwnedSettings();
        //    ownedSettings.setFlag(CollationSettings.BACKWARD_SECONDARY, flag);
        //    SetFastLatinOptions(ownedSettings);
        //}

        public bool IsFrenchCollation
        {
            get
            {
                return (settings.ReadOnly.Options & CollationSettings.BACKWARD_SECONDARY) != 0;
            }
            set
            {
                CheckNotFrozen();
                if (value == IsFrenchCollation) { return; }
                CollationSettings ownedSettings = GetOwnedSettings();
                ownedSettings.SetFlag(CollationSettings.BACKWARD_SECONDARY, value);
                SetFastLatinOptions(ownedSettings);
            }
        }

        /**
         * Sets the alternate handling for QUATERNARY strength to be either shifted or non-ignorable. See the UCA definition
         * on <a href="http://www.unicode.org/unicode/reports/tr10/#Variable_Weighting">Variable Weighting</a>. This
         * attribute will only be effective when QUATERNARY strength is set. The default value for this mode is false,
         * corresponding to the NON_IGNORABLE mode in UCA. In the NON_IGNORABLE mode, the RuleBasedCollator treats all
         * the code points with non-ignorable primary weights in the same way. If the mode is set to true, the behavior
         * corresponds to SHIFTED defined in UCA, this causes code points with PRIMARY orders that are equal or below the
         * variable top value to be ignored in PRIMARY order and moved to the QUATERNARY order.
         * 
         * @param shifted
         *            true if SHIFTED behavior for alternate handling is desired, false for the NON_IGNORABLE behavior.
         * @see #isAlternateHandlingShifted
         * @see #setAlternateHandlingDefault
         * @stable ICU 2.8
         */
        //public void setAlternateHandlingShifted(boolean shifted)
        //{
        //    CheckNotFrozen();
        //    if (shifted == isAlternateHandlingShifted()) { return; }
        //    CollationSettings ownedSettings = GetOwnedSettings();
        //    ownedSettings.setAlternateHandlingShifted(shifted);
        //    SetFastLatinOptions(ownedSettings);
        //}

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

        /**
         * <p>
         * When case level is set to true, an additional weight is formed between the SECONDARY and TERTIARY weight, known
         * as the case level. The case level is used to distinguish large and small Japanese Kana characters. Case level
         * could also be used in other situations. For example to distinguish certain Pinyin characters. The default value
         * is false, which means the case level is not generated. The contents of the case level are affected by the case
         * first mode. A simple way to ignore accent differences in a string is to set the strength to PRIMARY and enable
         * case level.
         * <p>
         * See the section on <a href="http://userguide.icu-project.org/collation/architecture">case
         * level</a> for more information.
         *
         * @param flag
         *            true if case level sorting is required, false otherwise
         * @stable ICU 2.8
         * @see #setCaseLevelDefault
         * @see #isCaseLevel
         */
        //public void setCaseLevel(boolean flag)
        //{
        //    CheckNotFrozen();
        //    if (flag == isCaseLevel()) { return; }
        //    CollationSettings ownedSettings = GetOwnedSettings();
        //    ownedSettings.setFlag(CollationSettings.CASE_LEVEL, flag);
        //    SetFastLatinOptions(ownedSettings);
        //}

        public bool IsCaseLevel
        {
            get
            {
                return (settings.ReadOnly.Options & CollationSettings.CASE_LEVEL) != 0;
            }
            set
            {
                CheckNotFrozen();
                if (value == IsCaseLevel) { return; }
                CollationSettings ownedSettings = GetOwnedSettings();
                ownedSettings.SetFlag(CollationSettings.CASE_LEVEL, value);
                SetFastLatinOptions(ownedSettings);
            }
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
         * The default decompositon mode for the Collator is
         * NO_DECOMPOSITON, unless specified otherwise by the locale used
         * to create the Collator.
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
        //@Override
        //    public void setDecomposition(int decomposition)
        //{
        //    CheckNotFrozen();
        //    boolean flag;
        //    switch (decomposition)
        //    {
        //        case NO_DECOMPOSITION:
        //            flag = false;
        //            break;
        //        case CANONICAL_DECOMPOSITION:
        //            flag = true;
        //            break;
        //        default:
        //            throw new IllegalArgumentException("Wrong decomposition mode.");
        //    }
        //    if (flag == settings.ReadOnly.getFlag(CollationSettings.CHECK_FCD)) { return; }
        //    CollationSettings ownedSettings = GetOwnedSettings();
        //    ownedSettings.setFlag(CollationSettings.CHECK_FCD, flag);
        //    SetFastLatinOptions(ownedSettings);
        //}

        public override NormalizationMode Decomposition
        {
            get
            {
                return (settings.ReadOnly.Options & CollationSettings.CHECK_FCD) != 0 ?
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
                if (flag == settings.ReadOnly.GetFlag(CollationSettings.CHECK_FCD)) { return; }
                CollationSettings ownedSettings = GetOwnedSettings();
                ownedSettings.SetFlag(CollationSettings.CHECK_FCD, flag);
                SetFastLatinOptions(ownedSettings);
            }
        }

        /**
         * Sets this Collator's strength attribute. The strength attribute determines the minimum level of difference
         * considered significant during comparison.
         *
         * <p>See the Collator class description for an example of use.
         * 
         * @param newStrength
         *            the new strength value.
         * @see #getStrength
         * @see #setStrengthDefault
         * @see #PRIMARY
         * @see #SECONDARY
         * @see #TERTIARY
         * @see #QUATERNARY
         * @see #IDENTICAL
         * @exception IllegalArgumentException
         *                If the new strength value is not one of PRIMARY, SECONDARY, TERTIARY, QUATERNARY or IDENTICAL.
         * @stable ICU 2.8
         */
        //        @Override
        //    public void setStrength(int newStrength)
        //{
        //    CheckNotFrozen();
        //    if (newStrength == getStrength()) { return; }
        //    CollationSettings ownedSettings = GetOwnedSettings();
        //    ownedSettings.setStrength(newStrength);
        //    SetFastLatinOptions(ownedSettings);
        //}

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

        /**
         * {@icu} Sets the variable top to the top of the specified reordering group.
         * The variable top determines the highest-sorting character
         * which is affected by the alternate handling behavior.
         * If that attribute is set to NON_IGNORABLE, then the variable top has no effect.
         * @param group one of Collator.ReorderCodes.SPACE, Collator.ReorderCodes.PUNCTUATION,
         *              Collator.ReorderCodes.SYMBOL, Collator.ReorderCodes.CURRENCY;
         *              or Collator.ReorderCodes.DEFAULT to restore the default max variable group
         * @return this
         * @see #getMaxVariable
         * @stable ICU 53
         */
        //        @Override
        //    public RuleBasedCollator setMaxVariable(int group)
        //{
        //    // Convert the reorder code into a MaxVariable number, or UCOL_DEFAULT=-1.
        //    int value;
        //    if (group == Collator.ReorderCodes.DEFAULT)
        //    {
        //        value = -1;  // UCOL_DEFAULT
        //    }
        //    else if (Collator.ReorderCodes.FIRST <= group && group <= Collator.ReorderCodes.CURRENCY)
        //    {
        //        value = group - Collator.ReorderCodes.FIRST;
        //    }
        //    else
        //    {
        //        throw new IllegalArgumentException("illegal max variable group " + group);
        //    }
        //    int oldValue = settings.ReadOnly.getMaxVariable();
        //    if (value == oldValue)
        //    {
        //        return this;
        //    }
        //    CollationSettings defaultSettings = GetDefaultSettings();
        //    if (settings.ReadOnly == defaultSettings)
        //    {
        //        if (value < 0)
        //        {  // UCOL_DEFAULT
        //            return this;
        //        }
        //    }
        //    CollationSettings ownedSettings = GetOwnedSettings();

        //    if (group == Collator.ReorderCodes.DEFAULT)
        //    {
        //        group = Collator.ReorderCodes.FIRST + defaultSettings.getMaxVariable();
        //    }
        //    long varTop = data.getLastPrimaryForGroup(group);
        //    assert(varTop != 0);
        //    ownedSettings.setMaxVariable(value, defaultSettings.options);
        //    ownedSettings.variableTop = varTop;
        //    SetFastLatinOptions(ownedSettings);
        //    return this;
        //}

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

        /**
         * {@icu} Returns the maximum reordering group whose characters are affected by
         * the alternate handling behavior.
         * @return the maximum variable reordering group.
         * @see #setMaxVariable
         * @stable ICU 53
         */
        //        @Override
        //    public int getMaxVariable()
        //{
        //    return Collator.ReorderCodes.FIRST + settings.ReadOnly.getMaxVariable();
        //}

        /**
         * {@icu} Sets the variable top to the primary weight of the specified string.
         *
         * <p>Beginning with ICU 53, the variable top is pinned to
         * the top of one of the supported reordering groups,
         * and it must not be beyond the last of those groups.
         * See {@link #setMaxVariable(int)}.
         * 
         * @param varTop
         *            one or more (if contraction) characters to which the variable top should be set
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
                UTF16CollationIterator ci = new UTF16CollationIterator(data, numeric, varTop.ToCharSequence(), 0);
                ce1 = ci.NextCE();
                ce2 = ci.NextCE();
            }
            else
            {
                FCDUTF16CollationIterator ci = new FCDUTF16CollationIterator(data, numeric, varTop.ToCharSequence(), 0);
                ce1 = ci.NextCE();
                ce2 = ci.NextCE();
            }
            if (ce1 == Collation.NO_CE || ce2 != Collation.NO_CE)
            {
                throw new ArgumentException("Variable top argument string must map to exactly one collation element");
            }
            InternalSetVariableTop(ce1.TripleShift(32));
            return (int)settings.ReadOnly.VariableTop;
        }

        /**
         * {@icu} Sets the variable top to the specified primary weight.
         *
         * <p>Beginning with ICU 53, the variable top is pinned to
         * the top of one of the supported reordering groups,
         * and it must not be beyond the last of those groups.
         * See {@link #setMaxVariable(int)}.
         * 
         * @param varTop primary weight, as returned by setVariableTop or getVariableTop
         * @see #getVariableTop
         * @see #setVariableTop(String)
         * @deprecated ICU 53 Call setMaxVariable() instead.
         */
        //@Override
        //@Deprecated
        //    public void setVariableTop(int varTop)
        //{
        //    CheckNotFrozen();
        //    internalSetVariableTop(varTop & 0xffffffffL);
        //}

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

        /**
         * {@icu} When numeric collation is turned on, this Collator makes
         * substrings of digits sort according to their numeric values.
         *
         * <p>This is a way to get '100' to sort AFTER '2'. Note that the longest
         * digit substring that can be treated as a single unit is
         * 254 digits (not counting leading zeros). If a digit substring is
         * longer than that, the digits beyond the limit will be treated as a
         * separate digit substring.
         *
         * <p>A "digit" in this sense is a code point with General_Category=Nd,
         * which does not include circled numbers, roman numerals, etc.
         * Only a contiguous digit substring is considered, that is,
         * non-negative integers without separators.
         * There is no support for plus/minus signs, decimals, exponents, etc.
         *
         * @param flag
         *            true to turn numeric collation on and false to turn it off
         * @see #getNumericCollation
         * @see #setNumericCollationDefault
         * @stable ICU 2.8
         */
        //public void setNumericCollation(boolean flag)
        //{
        //    CheckNotFrozen();
        //    // sort substrings of digits as numbers
        //    if (flag == getNumericCollation()) { return; }
        //    CollationSettings ownedSettings = GetOwnedSettings();
        //    ownedSettings.setFlag(CollationSettings.NUMERIC, flag);
        //    SetFastLatinOptions(ownedSettings);
        //}

        public bool IsNumericCollation
        {
            get
            {
                return (settings.ReadOnly.Options & CollationSettings.NUMERIC) != 0;
            }
            set
            {
                CheckNotFrozen();
                // sort substrings of digits as numbers
                if (value == IsNumericCollation) { return; }
                CollationSettings ownedSettings = GetOwnedSettings();
                ownedSettings.SetFlag(CollationSettings.NUMERIC, value);
                SetFastLatinOptions(ownedSettings);
            }
        }

        /**
         * {@inheritDoc}
         *
         * @param order the reordering codes to apply to this collator; if this is null or an empty array
         * then this clears any existing reordering
         * @throws IllegalArgumentException if the reordering codes are malformed in any way (e.g. duplicates, multiple reset codes, overlapping equivalent scripts)
         * @see #getReorderCodes
         * @see Collator#getEquivalentReorderCodes
         * @see Collator.ReorderCodes
         * @see UScript
         * @stable ICU 4.8
         */
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
                    Arrays.Equals(order, settings.ReadOnly.ReorderCodes))
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

        /**
         * Gets the collation tailoring rules for this RuleBasedCollator.
         * Equivalent to String getRules(false).
         * 
         * @return the collation tailoring rules
         * @see #getRules(boolean)
         * @stable ICU 2.8
         */
        public string GetRules()
        {
            return tailoring.GetRules();
        }

        /**
         * Returns current rules.
         * The argument defines whether full rules (root collation + tailored) rules are returned
         * or just the tailoring.
         * 
         * <p>The root collation rules are an <i>approximation</i> of the root collator's sort order.
         * They are almost never used or useful at runtime and can be removed from the data.
         * See <a href="http://userguide.icu-project.org/collation/customization#TOC-Building-on-Existing-Locales">User Guide:
         * Collation Customization, Building on Existing Locales</a>
         *
         * <p>{@link #getRules()} should normally be used instead.
         * @param fullrules
         *            true if the rules that defines the full set of collation order is required, otherwise false for
         *            returning only the tailored rules
         * @return the current rules that defines this Collator.
         * @see #getRules()
         * @stable ICU 2.6
         */
        public string GetRules(bool fullrules)
        {
            if (!fullrules)
            {
                return tailoring.GetRules();
            }
            return CollationLoader.RootRules + tailoring.GetRules();
        }

        /**
         * Get a UnicodeSet that contains all the characters and sequences tailored in this collator.
         * 
         * @return a pointer to a UnicodeSet object containing all the code points and sequences that may sort differently
         *         than in the root collator.
         * @stable ICU 2.4
         */
        public override UnicodeSet GetTailoredSet()
        {
            UnicodeSet tailored = new UnicodeSet();
            if (data.Base != null)
            {
                new TailoredSet(tailored).ForData(data);
            }
            return tailored;
        }

        /**
         * Gets unicode sets containing contractions and/or expansions of a collator
         * 
         * @param contractions
         *            if not null, set to contain contractions
         * @param expansions
         *            if not null, set to contain expansions
         * @param addPrefixes
         *            add the prefix contextual elements to contractions
         * @throws Exception
         *             Throws an exception if any errors occurs.
         * @stable ICU 3.4
         */
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

        /**
         * Adds the contractions that start with character c to the set.
         * Ignores prefixes. Used by AlphabeticIndex.
         * @internal
         * @deprecated This API is ICU internal only.
         */
        internal void InternalAddContractions(int c, UnicodeSet set)
        {
            new ContractionsAndExpansions(set, null, null, false).ForCodePoint(data, c);
        }

        /**
         * <p>
         * Get a Collation key for the argument String source from this RuleBasedCollator.
         * <p>
         * General recommendation: <br>
         * If comparison are to be done to the same String multiple times, it would be more efficient to generate
         * CollationKeys for the Strings and use CollationKey.compareTo(CollationKey) for the comparisons. If the each
         * Strings are compared to only once, using the method RuleBasedCollator.compare(String, String) will have a better
         * performance.
         * <p>
         * See the class documentation for an explanation about CollationKeys.
         *
         * @param source
         *            the text String to be transformed into a collation key.
         * @return the CollationKey for the given String based on this RuleBasedCollator's collation rules. If the source
         *         String is null, a null CollationKey is returned.
         * @see CollationKey
         * @see #compare(String, String)
         * @see #getRawCollationKey
         * @stable ICU 2.8
         */
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
            buffer.RawCollationKey = GetRawCollationKey(source.ToCharSequence(), buffer.RawCollationKey, buffer);
            return new CollationKey(source, buffer.RawCollationKey);
        }

        /**
         * Gets the simpler form of a CollationKey for the String source following the rules of this Collator and stores the
         * result into the user provided argument key. If key has a internal byte array of length that's too small for the
         * result, the internal byte array will be grown to the exact required size.
         * 
         * @param source the text String to be transformed into a RawCollationKey
         * @param key output RawCollationKey to store results
         * @return If key is null, a new instance of RawCollationKey will be created and returned, otherwise the user
         *         provided key will be returned.
         * @see #getCollationKey
         * @see #compare(String, String)
         * @see RawCollationKey
         * @stable ICU 2.8
         */
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
                return GetRawCollationKey(source.ToCharSequence(), key, buffer);
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
                    System.Array.Copy(bytes, start, buffer_, length, n);
                }
            }

            protected override bool Resize(int appendCapacity, int length)
            {
                int newCapacity = 2 * buffer_.Length;
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
                System.Array.Copy(buffer_, 0, newBytes, 0, length);
                buffer_ = key_.Bytes = newBytes;
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
                        buffer.LeftUTF16CollIter, data.compressibleBytes, settings.ReadOnly,
                        sink, Collation.PRIMARY_LEVEL,
                        CollationKeys.SIMPLE_LEVEL_FALLBACK, true);
            }
            else
            {
                buffer.LeftFCDUTF16Iter.SetText(numeric, s, 0);
                CollationKeys.WriteSortKeyUpToQuaternary(
                        buffer.LeftFCDUTF16Iter, data.compressibleBytes, settings.ReadOnly,
                        sink, Collation.PRIMARY_LEVEL,
                        CollationKeys.SIMPLE_LEVEL_FALLBACK, true);
            }
            if (settings.ReadOnly.Strength == CollationStrength.Identical)
            {
                WriteIdenticalLevel(s, sink);
            }
            sink.Append(Collation.TERMINATOR_BYTE);
        }

        private void WriteIdenticalLevel(ICharSequence s, CollationKeyByteSink sink)
        {
            // NFD quick check
            int nfdQCYesLimit = data.nfcImpl.Decompose(s, 0, s.Length, null);
            sink.Append(Collation.LEVEL_SEPARATOR_BYTE);
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
                StringBuilderCharSequence nfd = new StringBuilderCharSequence();
                data.nfcImpl.Decompose(s, nfdQCYesLimit, s.Length, nfd.StringBuilder, destLengthEstimate);
                BOCSU.WriteIdenticalLevelRun(prev, nfd, 0, nfd.Length, sink.Key);
            }
            // Sync the key with the buffer again which got bytes appended and may have been reallocated.
            sink.SetBufferAndAppended(sink.Key.Bytes, sink.Key.Length);
        }

        /**
         * Returns the CEs for the string.
         * @param str the string
         * @internal for tests &amp; tools
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public long[] InternalGetCEs(ICharSequence str)
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
                Debug.Assert(length >= 0 && iter.GetCE(length) == Collation.NO_CE);
                long[] ces = new long[length];
                System.Array.Copy(iter.GetCEs(), 0, ces, 0, length);
                return ces;
            }
            finally
            {
                ReleaseCollationBuffer(buffer);
            }
        }

        /**
         * Returns this Collator's strength attribute. The strength attribute
         * determines the minimum level of difference considered significant.
         *
         * <p>{@icunote} This can return QUATERNARY strength, which is not supported by the
         * JDK version.
         *
         * <p>See the Collator class description for more details.
         *
         * @return this Collator's current strength attribute.
         * @see #setStrength
         * @see #PRIMARY
         * @see #SECONDARY
         * @see #TERTIARY
         * @see #QUATERNARY
         * @see #IDENTICAL
         * @stable ICU 2.8
         */
        //@Override
        //    public int getStrength()
        //{
        //    return settings.ReadOnly.getStrength();
        //}

        /**
         * Returns the decomposition mode of this Collator. The decomposition mode
         * determines how Unicode composed characters are handled.
         *
         * <p>See the Collator class description for more details.
         *
         * @return the decomposition mode
         * @see #setDecomposition
         * @see #NO_DECOMPOSITION
         * @see #CANONICAL_DECOMPOSITION
         * @stable ICU 2.8
         */
        //@Override
        //    public int getDecomposition()
        //{
        //    return (settings.ReadOnly.options & CollationSettings.CHECK_FCD) != 0 ?
        //            CANONICAL_DECOMPOSITION : NO_DECOMPOSITION;
        //}

        ///**
        // * Return true if an uppercase character is sorted before the corresponding lowercase character. See
        // * setCaseFirst(boolean) for details.
        // * 
        // * @see #setUpperCaseFirst
        // * @see #setLowerCaseFirst
        // * @see #isLowerCaseFirst
        // * @see #setCaseFirstDefault
        // * @return true if upper cased characters are sorted before lower cased characters, false otherwise
        // * @stable ICU 2.8
        // */
        //public boolean isUpperCaseFirst()
        //{
        //    return (settings.ReadOnly.getCaseFirst() == CollationSettings.CASE_FIRST_AND_UPPER_MASK);
        //}

        ///**
        // * Return true if a lowercase character is sorted before the corresponding uppercase character. See
        // * setCaseFirst(boolean) for details.
        // * 
        // * @see #setUpperCaseFirst
        // * @see #setLowerCaseFirst
        // * @see #isUpperCaseFirst
        // * @see #setCaseFirstDefault
        // * @return true lower cased characters are sorted before upper cased characters, false otherwise
        // * @stable ICU 2.8
        // */
        //public boolean isLowerCaseFirst()
        //{
        //    return (settings.ReadOnly.getCaseFirst() == CollationSettings.CASE_FIRST);
        //}

        /**
         * Checks if the alternate handling behavior is the UCA defined SHIFTED or NON_IGNORABLE. If return value is true,
         * then the alternate handling attribute for the Collator is SHIFTED. Otherwise if return value is false, then the
         * alternate handling attribute for the Collator is NON_IGNORABLE See setAlternateHandlingShifted(boolean) for more
         * details.
         * 
         * @return true or false
         * @see #setAlternateHandlingShifted(boolean)
         * @see #setAlternateHandlingDefault
         * @stable ICU 2.8
         */
        //public boolean isAlternateHandlingShifted()
        //{
        //    return settings.ReadOnly.getAlternateHandling();
        //}

        /**
         * Checks if case level is set to true. See setCaseLevel(boolean) for details.
         * 
         * @return the case level mode
         * @see #setCaseLevelDefault
         * @see #isCaseLevel
         * @see #setCaseLevel(boolean)
         * @stable ICU 2.8
         */
        //public boolean isCaseLevel()
        //{
        //    return (settings.ReadOnly.options & CollationSettings.CASE_LEVEL) != 0;
        //}

        ///**
        // * Checks if French Collation is set to true. See setFrenchCollation(boolean) for details.
        // * 
        // * @return true if French Collation is set to true, false otherwise
        // * @see #setFrenchCollation(boolean)
        // * @see #setFrenchCollationDefault
        // * @stable ICU 2.8
        // */
        //public boolean isFrenchCollation()
        //{
        //    return (settings.ReadOnly.options & CollationSettings.BACKWARD_SECONDARY) != 0;
        //}

        ///**
        // * Checks if the Hiragana Quaternary mode is set on. See setHiraganaQuaternary(boolean) for more details.
        // *
        // * <p>This attribute was an implementation detail of the CLDR Japanese tailoring.
        // * Since ICU 50, this attribute is not settable any more via API functions.
        // * Since CLDR 25/ICU 53, explicit quaternary relations are used
        // * to achieve the same Japanese sort order.
        // *
        // * @return false
        // * @see #setHiraganaQuaternaryDefault
        // * @see #setHiraganaQuaternary(boolean)
        // * @deprecated ICU 50 Implementation detail, cannot be set via API, was removed from implementation.
        // */
        //@Deprecated
        //    public boolean isHiraganaQuaternary()
        //{
        //    return false;
        //}

        /**
         * {@icu} Gets the variable top value of a Collator.
         * 
         * @return the variable top primary weight
         * @see #getMaxVariable
         * @stable ICU 2.6
         */
        //@Override
        //    public int getVariableTop()
        //{
        //    return (int)settings.ReadOnly.variableTop;
        //}

        /**
         * Method to retrieve the numeric collation value. When numeric collation is turned on, this Collator generates a
         * collation key for the numeric value of substrings of digits. This is a way to get '100' to sort AFTER '2'
         * 
         * @see #setNumericCollation
         * @see #setNumericCollationDefault
         * @return true if numeric collation is turned on, false otherwise
         * @stable ICU 2.8
         */
        //public boolean getNumericCollation()
        //{
        //    return (settings.ReadOnly.options & CollationSettings.NUMERIC) != 0;
        //}

        /**  
         * Retrieves the reordering codes for this collator.
         * These reordering codes are a combination of UScript codes and ReorderCodes.
         * @return a copy of the reordering codes for this collator; 
         * if none are set then returns an empty array
         * @see #setReorderCodes
         * @see Collator#getEquivalentReorderCodes
         * @stable ICU 4.8
         */
        public override int[] GetReorderCodes()
        {
            return (int[])settings.ReadOnly.ReorderCodes.Clone();
        }

        // public other methods -------------------------------------------------

        /**
         * {@inheritDoc}
         * @stable ICU 2.8
         */
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

        /**
         * Generates a unique hash code for this RuleBasedCollator.
         * 
         * @return the unique hash code for this Collator
         * @stable ICU 2.8
         */
        public override int GetHashCode()
        {
            int h = settings.ReadOnly.GetHashCode();
            if (data.Base == null) { return h; }  // root collator
                                                  // Do not rely on the rule string, see comments in operator==().
            UnicodeSet set = GetTailoredSet();
            UnicodeSetIterator iter = new UnicodeSetIterator(set);
            while (iter.Next() && iter.Codepoint != UnicodeSetIterator.IS_STRING)
            {
                h ^= data.GetCE32(iter.Codepoint);
            }
            return h;
        }

        /**
         * Compares the source text String to the target text String according to the collation rules, strength and
         * decomposition mode for this RuleBasedCollator. Returns an integer less than, equal to or greater than zero
         * depending on whether the source String is less than, equal to or greater than the target String. See the Collator
         * class description for an example of use.
         * <p>
         * General recommendation: <br>
         * If comparison are to be done to the same String multiple times, it would be more efficient to generate
         * CollationKeys for the Strings and use CollationKey.compareTo(CollationKey) for the comparisons. If speed
         * performance is critical and object instantiation is to be reduced, further optimization may be achieved by
         * generating a simpler key of the form RawCollationKey and reusing this RawCollationKey object with the method
         * RuleBasedCollator.getRawCollationKey. Internal byte representation can be directly accessed via RawCollationKey
         * and stored for future use. Like CollationKey, RawCollationKey provides a method RawCollationKey.compareTo for key
         * comparisons. If the each Strings are compared to only once, using the method RuleBasedCollator.compare(String,
         * String) will have a better performance.
         *
         * @param source
         *            the source text String.
         * @param target
         *            the target text String.
         * @return Returns an integer value. Value is less than zero if source is less than target, value is zero if source
         *         and target are equal, value is greater than zero if source is greater than target.
         * @see CollationKey
         * @see #getCollationKey
         * @stable ICU 2.8
         */
        public override int Compare(string source, string target)
        {
#pragma warning disable 612, 618
            return DoCompare(source.ToCharSequence(), target.ToCharSequence());
#pragma warning restore 612, 618
        }

        /**
        * Abstract iterator for identical-level string comparisons.
        * Returns FCD code points and handles temporary switching to NFD.
        *
        * <p>As with CollationIterator,
        * Java NFDIterator instances are partially constructed and cached,
        * and completed when reset for use.
        * C++ NFDIterator instances are stack-allocated.
*/
        private abstract class NFDIterator
        {
            /**
             * Partial constructor, must call reset().
             */
            internal NFDIterator() { }
            internal void Reset()
            {
                index = -1;
            }

            /**
             * Returns the next code point from the internal normalization buffer,
             * or else the next text code point.
             * Returns -1 at the end of the text.
             */
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
            /**
             * @param nfcImpl
             * @param c the last code point returned by nextCodePoint() or nextDecomposedCodePoint()
             * @return the first code point in c's decomposition,
             *         or c itself if it was decomposed already or if it does not decompose
             */
            internal int NextDecomposedCodePoint(Normalizer2Impl nfcImpl, int c)
            {
                if (index >= 0) { return c; }
                decomp = nfcImpl.GetDecomposition(c);
                if (decomp == null) { return c; }
                c = Character.CodePointAt(decomp, 0);
                index = Character.CharCount(c);
                return c;
            }

            /**
             * Returns the next text code point in FCD order.
             * Returns -1 at the end of the text.
             */
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
                if (pos == s.Length) { return Collation.SENTINEL_CP; }
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
                    if (str == null)
                    {
                        str = new StringBuilderCharSequence();
                    }
                    else
                    {
                        str.StringBuilder.Length = 0;
                    }
                    str.StringBuilder.Append(seq, start, spanLimit);
                    ReorderingBuffer buffer = new ReorderingBuffer(nfcImpl, str.StringBuilder, seq.Length - start);
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
                if (leftCp < rightCp) { return Collation.LESS; }
                if (leftCp > rightCp) { return Collation.GREATER; }
            }
            return Collation.EQUAL;
        }

        /**
         * Compares two CharSequences.
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        protected override int DoCompare(ICharSequence left, ICharSequence right)
        {
            if (left == right)
            {
                return Collation.EQUAL;
            }

            // Identical-prefix test.
            int equalPrefixLength = 0;
            for (; ; )
            {
                if (equalPrefixLength == left.Length)
                {
                    if (equalPrefixLength == right.Length) { return Collation.EQUAL; }
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
                        left[equalPrefixLength] <= CollationFastLatin.LATIN_MAX) &&
                    (equalPrefixLength == right.Length ||
                        right[equalPrefixLength] <= CollationFastLatin.LATIN_MAX))
            {
                result = CollationFastLatin.CompareUTF16(data.fastLatinTable,
                                                          roSettings.FastLatinPrimaries,
                                                          fastLatinOptions,
                                                          left, right, equalPrefixLength);
            }
            else
            {
                result = CollationFastLatin.BAIL_OUT_RESULT;
            }

            if (result == CollationFastLatin.BAIL_OUT_RESULT)
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
            if (result != Collation.EQUAL || roSettings.Strength < CollationStrength.Identical)
            {
                return result;
            }

            CollationBuffer buffer2 = null;
            try
            {
                buffer2 = GetCollationBuffer();
                // Compare identical level.
                Normalizer2Impl nfcImpl = data.nfcImpl;
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

        /**
         * Tests whether a character is "unsafe" for use as a collation starting point.
         *
         * @param c code point or code unit
         * @return true if c is unsafe
         * @see CollationElementIterator#setOffset(int)
         */
        internal bool IsUnsafe(int c)
        {
            return data.IsUnsafeBackward(c, settings.ReadOnly.IsNumeric);
        }

        /**
         * Frozen state of the collator.
         */
        private ReentrantLock frozenLock;

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

        /**
         * Get the version of this collator object.
         * 
         * @return the version object associated with this collator
         * @stable ICU 2.8
         */
        public override VersionInfo GetVersion()
        {
            int version = tailoring.Version;
            int rtVersion = VersionInfo.UCOL_RUNTIME_VERSION.Major;
            return VersionInfo.GetInstance(
                    (version.TripleShift(24)) + (rtVersion << 4) + (rtVersion >> 4),
                    ((version >> 16) & 0xff), ((version >> 8) & 0xff), (version & 0xff));
        }

        /**
         * Get the UCA version of this collator object.
         * 
         * @return the version object associated with this collator
         * @stable ICU 2.8
         */
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
                frozenLock.Lock();
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
                frozenLock.Unlock();
            }
        }

        /**
         * {@inheritDoc}
         * @draft ICU 53 (retain)
         * @provisional This API might change or be removed in a future release.
         */
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

        /**
         * {@inheritDoc}
         */
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
