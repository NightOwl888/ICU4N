using ICU4N.Impl;
using ICU4N.Numerics;
using ICU4N.Text;
using ICU4N.Util;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static ICU4N.Numerics.NumberFormatter;

namespace ICU4N.Numerics
{
    /// <summary>
    /// This class is a <see cref="IModifier"/> that wraps a decimal format pattern. It applies the pattern's affixes in
    /// <see cref="IModifier.Apply(NumberStringBuilder, int, int)"/>.
    /// <para/>
    /// In addition to being a <see cref="IModifier"/>, this class contains the business logic for substituting the correct locale symbols
    /// into the affixes of the decimal format pattern.
    /// <para/>
    /// In order to use this class, create a new instance and call the following four setters: {@link #setPatternInfo},
    /// {@link #setPatternAttributes}, {@link #setSymbols}, and {@link #setNumberProperties}. After calling these four
    /// setters, the instance will be ready for use as a <see cref="IModifier"/>.
    /// <para/>
    /// This is a MUTABLE, NON-THREAD-SAFE class designed for performance. Do NOT save references to this or attempt to use
    /// it from multiple threads! Instead, you can obtain a safe, immutable decimal format pattern modifier by calling
    /// <see cref="CreateImmutable()"/>, in effect treating this instance as a builder for the immutable
    /// variant.
    /// </summary>
    internal class MutablePatternModifier : IModifier, AffixUtils.ISymbolProvider, ICharSequence, IMicroPropsGenerator
    {
        bool ICharSequence.HasValue => true; // ICU4N specific

        // Modifier details
        internal readonly bool isStrong;

        // Pattern details
        internal IAffixPatternProvider patternInfo;
        internal SignDisplay? signDisplay;
        internal bool perMilleReplacesPercent;

        // Symbol details
        internal DecimalFormatSymbols symbols;
        internal UnitWidth? unitWidth;
        internal Currency currency;
        internal PluralRules rules;

        // Number details
        internal bool isNegative;
        internal StandardPlural? plural;

        // QuantityChain details
        internal IMicroPropsGenerator parent;

        // Transient CharSequence fields
        internal bool inCharSequenceMode;
        internal AffixPatternProviderFlags flags;
        internal int length;
        internal bool prependSign;
        internal bool plusReplacesMinusSign;

        /**
         * @param isStrong
         *            Whether the modifier should be considered strong. For more information, see
         *            {@link Modifier#isStrong()}. Most of the time, decimal format pattern modifiers should be considered
         *            as non-strong.
         */
        public MutablePatternModifier(bool isStrong)
        {
            this.isStrong = isStrong;
        }

        /**
         * Sets a reference to the parsed decimal format pattern, usually obtained from
         * {@link PatternStringParser#parseToPatternInfo(String)}, but any implementation of {@link AffixPatternProvider} is
         * accepted.
         */
        public virtual void SetPatternInfo(IAffixPatternProvider patternInfo)
        {
            this.patternInfo = patternInfo;
        }

        /**
         * Sets attributes that imply changes to the literal interpretation of the pattern string affixes.
         *
         * @param signDisplay
         *            Whether to force a plus sign on positive numbers.
         * @param perMille
         *            Whether to substitute the percent sign in the pattern with a permille sign.
         */
        public virtual void SetPatternAttributes(SignDisplay? signDisplay, bool perMille)
        {
            this.signDisplay = signDisplay;
            this.perMilleReplacesPercent = perMille;
        }

        /**
         * Sets locale-specific details that affect the symbols substituted into the pattern string affixes.
         *
         * @param symbols
         *            The desired instance of DecimalFormatSymbols.
         * @param currency
         *            The currency to be used when substituting currency values into the affixes.
         * @param unitWidth
         *            The width used to render currencies.
         * @param rules
         *            Required if the triple currency sign, "¤¤¤", appears in the pattern, which can be determined from the
         *            convenience method {@link #needsPlurals()}.
         */
        public virtual void SetSymbols(DecimalFormatSymbols symbols, Currency currency, UnitWidth? unitWidth, PluralRules rules)
        {
            Debug.Assert((rules != null) == NeedsPlurals);
            this.symbols = symbols;
            this.currency = currency;
            this.unitWidth = unitWidth;
            this.rules = rules;
        }

        /**
         * Sets attributes of the current number being processed.
         *
         * @param isNegative
         *            Whether the number is negative.
         * @param plural
         *            The plural form of the number, required only if the pattern contains the triple currency sign, "¤¤¤"
         *            (and as indicated by {@link #needsPlurals()}).
         */
        public virtual void SetNumberProperties(bool isNegative, StandardPlural? plural)
        {
            Debug.Assert((plural != null) == NeedsPlurals);
            this.isNegative = isNegative;
            this.plural = plural;
        }

        /**
         * Returns true if the pattern represented by this MurkyModifier requires a plural keyword in order to localize.
         * This is currently true only if there is a currency long name placeholder in the pattern ("¤¤¤").
         */
        public virtual bool NeedsPlurals => patternInfo.ContainsSymbolType(AffixUtils.Type.CurrencyTriple);

        /**
         * Creates a new quantity-dependent Modifier that behaves the same as the current instance, but which is immutable
         * and can be saved for future use. The number properties in the current instance are mutated; all other properties
         * are left untouched.
         *
         * <para/>
         * The resulting modifier cannot be used in a QuantityChain.
         *
         * @return An immutable that supports both positive and negative numbers.
         */
        public virtual ImmutablePatternModifier CreateImmutable()
        {
            return CreateImmutableAndChain(null);
        }

        /**
         * Creates a new quantity-dependent Modifier that behaves the same as the current instance, but which is immutable
         * and can be saved for future use. The number properties in the current instance are mutated; all other properties
         * are left untouched.
         *
         * @param parent
         *            The QuantityChain to which to chain this immutable.
         * @return An immutable that supports both positive and negative numbers.
         */
        public virtual ImmutablePatternModifier CreateImmutableAndChain(IMicroPropsGenerator parent)
        {
            NumberStringBuilder a = new NumberStringBuilder();
            NumberStringBuilder b = new NumberStringBuilder();
            if (NeedsPlurals)
            {
                // Slower path when we require the plural keyword.
                ParameterizedModifier pm = new ParameterizedModifier();
                foreach (StandardPlural plural in StandardPluralUtil.Values)
                {
                    SetNumberProperties(false, plural);
                    pm.SetModifier(false, plural, CreateConstantModifier(a, b));
                    SetNumberProperties(true, plural);
                    pm.SetModifier(true, plural, CreateConstantModifier(a, b));
                }
                pm.Freeze();
                return new ImmutablePatternModifier(pm, rules, parent);
            }
            else
            {
                // Faster path when plural keyword is not needed.
                SetNumberProperties(false, null);
                IModifier positive = CreateConstantModifier(a, b);
                SetNumberProperties(true, null);
                IModifier negative = CreateConstantModifier(a, b);
                ParameterizedModifier pm = new ParameterizedModifier(positive, negative);
                return new ImmutablePatternModifier(pm, null, parent);
            }
        }

        /**
         * Uses the current properties to create a single {@link ConstantMultiFieldModifier} with currency spacing support
         * if required.
         *
         * @param a
         *            A working NumberStringBuilder object; passed from the outside to prevent the need to create many new
         *            instances if this method is called in a loop.
         * @param b
         *            Another working NumberStringBuilder object.
         * @return The constant modifier object.
         */
        private ConstantMultiFieldModifier CreateConstantModifier(NumberStringBuilder a, NumberStringBuilder b)
        {
            InsertPrefix(a.Clear(), 0);
            InsertSuffix(b.Clear(), 0);
            if (patternInfo.HasCurrencySign)
            {
                return new CurrencySpacingEnabledModifier(a, b, isStrong, symbols);
            }
            else
            {
                return new ConstantMultiFieldModifier(a, b, isStrong);
            }
        }

        public class ImmutablePatternModifier : IMicroPropsGenerator
        {
            internal readonly ParameterizedModifier pm;
            internal readonly PluralRules rules;
            internal readonly IMicroPropsGenerator parent;

            internal ImmutablePatternModifier(ParameterizedModifier pm, PluralRules rules, IMicroPropsGenerator parent)
            {
                this.pm = pm;
                this.rules = rules;
                this.parent = parent;
            }

            public virtual MicroProps ProcessQuantity(IDecimalQuantity quantity)
            {
                MicroProps micros = parent.ProcessQuantity(quantity);
                ApplyToMicros(micros, quantity);
                return micros;
            }

            public virtual void ApplyToMicros(MicroProps micros, IDecimalQuantity quantity)
            {
                if (rules == null)
                {
                    micros.modMiddle = pm.GetModifier(quantity.IsNegative);
                }
                else
                {
                    // TODO: Fix this. Avoid the copy.
                    IDecimalQuantity copy = quantity.CreateCopy();
                    copy.RoundToInfinity();
                    StandardPlural plural = copy.GetStandardPlural(rules);
                    micros.modMiddle = pm.GetModifier(quantity.IsNegative, plural);
                }
            }
        }

        /** Used by the unsafe code path. */
        public virtual IMicroPropsGenerator AddToChain(IMicroPropsGenerator parent)
        {
            this.parent = parent;
            return this;
        }

        public virtual MicroProps ProcessQuantity(IDecimalQuantity fq)
        {
            MicroProps micros = parent.ProcessQuantity(fq);
            if (NeedsPlurals)
            {
                // TODO: Fix this. Avoid the copy.
                IDecimalQuantity copy = fq.CreateCopy();
#pragma warning disable CS0618 // Type or member is obsolete
                micros.rounding.Apply(copy);
#pragma warning restore CS0618 // Type or member is obsolete
                SetNumberProperties(fq.IsNegative, copy.GetStandardPlural(rules));
            }
            else
            {
                SetNumberProperties(fq.IsNegative, null);
            }
            micros.modMiddle = this;
            return micros;
        }

        public virtual int Apply(NumberStringBuilder output, int leftIndex, int rightIndex)
        {
            int prefixLen = InsertPrefix(output, leftIndex);
            int suffixLen = InsertSuffix(output, rightIndex + prefixLen);
            CurrencySpacingEnabledModifier.ApplyCurrencySpacing(output, leftIndex, prefixLen, rightIndex + prefixLen,
                    suffixLen, symbols);
            return prefixLen + suffixLen;
        }

        public virtual int PrefixLength // ICU4N TODO: API perhaps better as a method
        {
            get
            {
                // Enter and exit CharSequence Mode to get the length.
                EnterCharSequenceMode(true);
                int result = AffixUtils.UnescapedCodePointCount(this, this);  // prefix length
                ExitCharSequenceMode();
                return result;
            }
        }

        public virtual int CodePointCount // ICU4N TODO: API perhaps better as a method
        {
            get
            {
                // Enter and exit CharSequence Mode to get the length.
                EnterCharSequenceMode(true);
                int result = AffixUtils.UnescapedCodePointCount(this, this);  // prefix length
                ExitCharSequenceMode();
                EnterCharSequenceMode(false);
                result += AffixUtils.UnescapedCodePointCount(this, this);  // suffix length
                ExitCharSequenceMode();
                return result;
            }
        }

        public virtual bool IsStrong => isStrong;

        private int InsertPrefix(NumberStringBuilder sb, int position)
        {
            EnterCharSequenceMode(true);
            int length = AffixUtils.Unescape(this, sb, position, this);
            ExitCharSequenceMode();
            return length;
        }

        private int InsertSuffix(NumberStringBuilder sb, int position)
        {
            EnterCharSequenceMode(false);
            int length = AffixUtils.Unescape(this, sb, position, this);
            ExitCharSequenceMode();
            return length;
        }

        /**
         * Returns the string that substitutes a given symbol type in a pattern.
         */
        public string GetSymbol(AffixUtils.Type type)
        {
            switch (type)
            {
                case AffixUtils.Type.MinusSign:
                    return symbols.MinusSignString;
                case AffixUtils.Type.PlusSign:
                    return symbols.PlusSignString;
                case AffixUtils.Type.Percent:
                    return symbols.PercentString;
                case AffixUtils.Type.PerMille:
                    return symbols.PerMillString;
                case AffixUtils.Type.CurrencySymbol:
                    // UnitWidth ISO, HIDDEN, or NARROW overrides the singular currency symbol.
                    if (unitWidth == UnitWidth.ISOCode)
                    {
                        return currency.CurrencyCode;
                    }
                    else if (unitWidth == UnitWidth.Hidden)
                    {
                        return string.Empty;
                    }
                    else if (unitWidth == UnitWidth.Narrow)
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        return currency.GetName(symbols.UCulture, Currency.NarrowSymbolName, out bool _);
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    else
                    {
                        return currency.GetName(symbols.UCulture, Currency.SymbolName, out bool _);
                    }
                case AffixUtils.Type.CurrencyDouble:
                    return currency.CurrencyCode;
                case AffixUtils.Type.CurrencyTriple:
                    // NOTE: This is the code path only for patterns containing "¤¤¤".
                    // Plural currencies set via the API are formatted in LongNameHandler.
                    // This code path is used by DecimalFormat via CurrencyPluralInfo.
                    Debug.Assert(plural != null);
                    return currency.GetName(symbols.UCulture, Currency.PluralLongName, plural.Value.GetKeyword(), out bool _);
                case AffixUtils.Type.CurrencyQuad:
                    return "\uFFFD";
                case AffixUtils.Type.CurrencyQuint:
#pragma warning disable CS0618 // Type or member is obsolete
                    return currency.GetName(symbols.UCulture, Currency.NarrowSymbolName, out bool _);
#pragma warning restore CS0618 // Type or member is obsolete
                default:
                    throw new InvalidOperationException(); //throw new AssertionError();
            }
        }

        /** This method contains the heart of the logic for rendering LDML affix strings. */
        private void EnterCharSequenceMode(bool isPrefix)
        {
            Debug.Assert(!inCharSequenceMode);
            inCharSequenceMode = true;

            // Should the output render '+' where '-' would normally appear in the pattern?
            plusReplacesMinusSign = !isNegative
                    && (signDisplay == SignDisplay.Always || signDisplay == SignDisplay.AccountingAlways)
                    && patternInfo.PositiveHasPlusSign == false;

            // Should we use the affix from the negative subpattern? (If not, we will use the positive subpattern.)
            bool useNegativeAffixPattern = patternInfo.HasNegativeSubpattern
                    && (isNegative || (patternInfo.NegativeHasMinusSign && plusReplacesMinusSign));

            // Resolve the flags for the affix pattern.
            flags = 0;
            if (useNegativeAffixPattern)
            {
                flags |= AffixPatternProviderFlags.NegativeSubpattern;
            }
            if (isPrefix)
            {
                flags |= AffixPatternProviderFlags.Prefix;
            }
            if (plural != null)
            {
                Debug.Assert((int)plural == ((int)AffixPatternProviderFlags.PluralMask & (int)plural));
                flags |= (AffixPatternProviderFlags)plural;
            }

            // Should we prepend a sign to the pattern?
            if (!isPrefix || useNegativeAffixPattern)
            {
                prependSign = false;
            }
            else if (isNegative)
            {
                prependSign = signDisplay != SignDisplay.Never;
            }
            else
            {
                prependSign = plusReplacesMinusSign;
            }

            // Finally, compute the length of the affix pattern.
            length = patternInfo.Length(flags) + (prependSign ? 1 : 0);
        }

        private void ExitCharSequenceMode()
        {
            Debug.Assert(inCharSequenceMode);
            inCharSequenceMode = false;
        }

        public virtual int Length
        {
            get
            {
                Debug.Assert(inCharSequenceMode);
                return length;
            }
        }

        public virtual char this[int index] // ICU4N TODO: Need to figure out how to make it possible to use ReadOnlySpan<char> with this
        {
            get
            {
                Debug.Assert(inCharSequenceMode);
                char candidate;
                if (prependSign && index == 0)
                {
                    candidate = '-';
                }
                else if (prependSign)
                {
                    candidate = patternInfo[flags, index - 1];
                }
                else
                {
                    candidate = patternInfo[flags, index];
                }
                if (plusReplacesMinusSign && candidate == '-')
                {
                    return '+';
                }
                if (perMilleReplacesPercent && candidate == '%')
                {
                    return '‰';
                }
                return candidate;
            }
        }


        public virtual ICharSequence SubSequence(int startIndex, int length)
        {
            // Never called by AffixUtils
            //throw new AssertionError();
            throw new NotSupportedException();
        }

        ICharSequence ICharSequence.Subsequence(int startIndex, int length)
        {
            throw new NotImplementedException();
        }
    }
}
