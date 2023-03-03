using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Text;
using ICU4N.Util;
using System.Diagnostics;
using static ICU4N.Numerics.NumberFormatter;
using static ICU4N.Numerics.PatternStringParser;
using static ICU4N.Numerics.Rounder;
using static ICU4N.Text.CompactDecimalFormat;

namespace ICU4N.Numerics
{
    /// <summary>
    /// This class, as well as <see cref="NumberFormatterImpl"/>, could go into the impl package, but they depend on too many
    /// package-private members of the public APIs.
    /// </summary>
    internal sealed class NumberPropertyMapper // ICU4N NOTE: This is supposed to be internal
    {
        /** Convenience method to create a NumberFormatter directly from Properties. */
        public static UnlocalizedNumberFormatter Create(DecimalFormatProperties properties, DecimalFormatSymbols symbols)
        {
            MacroProps macros = OldToNew(properties, symbols, null);
#pragma warning disable CS0618 // Type or member is obsolete
            return NumberFormatter.With().Macros(macros);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /**
         * Convenience method to create a NumberFormatter directly from a pattern string. Something like this could become
         * public API if there is demand.
         */
        public static UnlocalizedNumberFormatter Create(string pattern, DecimalFormatSymbols symbols)
        {
            DecimalFormatProperties properties = PatternStringParser.ParseToProperties(pattern);
            return Create(properties, symbols);
        }

        /**
         * Creates a new {@link MacroProps} object based on the content of a {@link DecimalFormatProperties} object. In
         * other words, maps Properties to MacroProps. This function is used by the JDK-compatibility API to call into the
         * ICU 60 fluent number formatting pipeline.
         *
         * @param properties
         *            The property bag to be mapped.
         * @param symbols
         *            The symbols associated with the property bag.
         * @param exportedProperties
         *            A property bag in which to store validated properties.
         * @return A new MacroProps containing all of the information in the Properties.
         */
        public static MacroProps OldToNew(DecimalFormatProperties properties, DecimalFormatSymbols symbols,
                DecimalFormatProperties exportedProperties)
        {
            MacroProps macros = new MacroProps();
            UCultureInfo locale = symbols.UCulture;

            /////////////
            // SYMBOLS //
            /////////////

            macros.symbols = symbols;

            //////////////////
            // PLURAL RULES //
            //////////////////

            macros.rules = properties.PluralRules;

            /////////////
            // AFFIXES //
            /////////////

            IAffixPatternProvider affixProvider;
            if (properties.CurrencyPluralInfo == null)
            {
                affixProvider = new PropertiesAffixPatternProvider(
                        properties.PositivePrefix != null ? AffixUtils.Escape(properties.PositivePrefix)
                                : properties.PositivePrefixPattern,
                        properties.PositiveSuffix != null ? AffixUtils.Escape(properties.PositiveSuffix)
                                : properties.PositiveSuffixPattern,
                        properties.NegativePrefix != null ? AffixUtils.Escape(properties.NegativePrefix)
                                : properties.NegativePrefixPattern,
                        properties.NegativeSuffix != null ? AffixUtils.Escape(properties.NegativeSuffix)
                                : properties.NegativeSuffixPattern);
            }
            else
            {
                affixProvider = new CurrencyPluralInfoAffixProvider(properties.CurrencyPluralInfo);
            }
            macros.affixProvider = affixProvider;

            ///////////
            // UNITS //
            ///////////

            bool useCurrency = ((properties.Currency != null) || properties.CurrencyPluralInfo != null
                    || properties.CurrencyUsage != null || affixProvider.HasCurrencySign);
            Currency currency = CustomSymbolCurrency.Resolve(properties.Currency, locale, symbols);
            CurrencyUsage? currencyUsage = properties.CurrencyUsage;
            bool explicitCurrencyUsage = currencyUsage != null;
            if (!explicitCurrencyUsage)
            {
                currencyUsage = CurrencyUsage.Standard;
            }
            if (useCurrency)
            {
                macros.unit = currency;
            }

            ///////////////////////
            // ROUNDING STRATEGY //
            ///////////////////////

            int maxInt = properties.MaximumIntegerDigits;
            int minInt = properties.MinimumIntegerDigits;
            int maxFrac = properties.MaximumFractionDigits;
            int minFrac = properties.MinimumFractionDigits;
            int minSig = properties.MinimumSignificantDigits;
            int maxSig = properties.MaximumSignificantDigits;
            BigMath.BigDecimal roundingIncrement = properties.RoundingIncrement;
            BigMath.MathContext mathContext = RoundingUtils.GetMathContextOrUnlimited(properties);
            bool explicitMinMaxFrac = minFrac != -1 || maxFrac != -1;
            bool explicitMinMaxSig = minSig != -1 || maxSig != -1;
            // Validate min/max int/frac.
            // For backwards compatibility, minimum overrides maximum if the two conflict.
            // The following logic ensures that there is always a minimum of at least one digit.
            if (minInt == 0 && maxFrac != 0)
            {
                // Force a digit after the decimal point.
                minFrac = minFrac <= 0 ? 1 : minFrac;
                maxFrac = maxFrac < 0 ? int.MaxValue : maxFrac < minFrac ? minFrac : maxFrac;
                minInt = 0;
                maxInt = maxInt < 0 ? -1 : maxInt > RoundingUtils.MAX_INT_FRAC_SIG ? -1 : maxInt;
            }
            else
            {
                // Force a digit before the decimal point.
                minFrac = minFrac < 0 ? 0 : minFrac;
                maxFrac = maxFrac < 0 ? int.MaxValue : maxFrac < minFrac ? minFrac : maxFrac;
                minInt = minInt <= 0 ? 1 : minInt > RoundingUtils.MAX_INT_FRAC_SIG ? 1 : minInt;
                maxInt = maxInt < 0 ? -1 : maxInt < minInt ? minInt : maxInt > RoundingUtils.MAX_INT_FRAC_SIG ? -1 : maxInt;
            }
            Rounder rounding = null;
            if (explicitCurrencyUsage)
            {
                rounding = Rounder.ConstructCurrency(currencyUsage).WithCurrency(currency);
            }
            else if (roundingIncrement != null)
            {
                rounding = Rounder.ConstructIncrement(roundingIncrement);
            }
            else if (explicitMinMaxSig)
            {
                minSig = minSig < 1 ? 1 : minSig > RoundingUtils.MAX_INT_FRAC_SIG ? RoundingUtils.MAX_INT_FRAC_SIG : minSig;
                maxSig = maxSig < 0 ? RoundingUtils.MAX_INT_FRAC_SIG
                        : maxSig < minSig ? minSig
                                : maxSig > RoundingUtils.MAX_INT_FRAC_SIG ? RoundingUtils.MAX_INT_FRAC_SIG : maxSig;
                rounding = Rounder.ConstructSignificant(minSig, maxSig);
            }
            else if (explicitMinMaxFrac)
            {
                rounding = Rounder.ConstructFraction(minFrac, maxFrac);
            }
            else if (useCurrency)
            {
                rounding = Rounder.ConstructCurrency(currencyUsage);
            }
            if (rounding != null)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                rounding = rounding.WithMode(mathContext);
#pragma warning restore CS0618 // Type or member is obsolete
                macros.rounder = rounding;
            }

            ///////////////////
            // INTEGER WIDTH //
            ///////////////////

            macros.integerWidth = IntegerWidth.ZeroFillTo(minInt).TruncateAt(maxInt);

            ///////////////////////
            // GROUPING STRATEGY //
            ///////////////////////

            int grouping1 = properties.GroupingSize;
            int grouping2 = properties.SecondaryGroupingSize;
            int minGrouping = properties.MinimumGroupingDigits;
            Debug.Assert(grouping1 >= -2); // value of -2 means to forward no grouping information
            grouping1 = grouping1 > 0 ? grouping1 : (grouping2 > 0 ? grouping2 : grouping1);
            grouping2 = grouping2 > 0 ? grouping2 : grouping1;
            // TODO: Is it important to handle minGrouping > 2?
#pragma warning disable CS0618 // Type or member is obsolete
            macros.grouper = Grouper.GetInstance((sbyte)grouping1, (sbyte)grouping2, minGrouping == 2);
#pragma warning restore CS0618 // Type or member is obsolete

            /////////////
            // PADDING //
            /////////////

            if (properties.FormatWidth != -1)
            {
                macros.padder = new Padder(properties.PadString, properties.FormatWidth,
                        properties.PadPosition);
            }

            ///////////////////////////////
            // DECIMAL MARK ALWAYS SHOWN //
            ///////////////////////////////

            macros.@decimal = properties.DecimalSeparatorAlwaysShown ? DecimalSeparatorDisplay.Always
                    : DecimalSeparatorDisplay.Auto;

            ///////////////////////
            // SIGN ALWAYS SHOWN //
            ///////////////////////

            macros.sign = properties.SignAlwaysShown ? SignDisplay.Always : SignDisplay.Auto;

            /////////////////////////
            // SCIENTIFIC NOTATION //
            /////////////////////////

            if (properties.MinimumExponentDigits != -1)
            {
                // Scientific notation is required.
                // This whole section feels like a hack, but it is needed for regression tests.
                // The mapping from property bag to scientific notation is nontrivial due to LDML rules.
                if (maxInt > 8)
                {
                    // But #13110: The maximum of 8 digits has unknown origins and is not in the spec.
                    // If maxInt is greater than 8, it is set to minInt, even if minInt is greater than 8.
                    maxInt = minInt;
                    macros.integerWidth = IntegerWidth.ZeroFillTo(minInt).TruncateAt(maxInt);
                }
                else if (maxInt > minInt && minInt > 1)
                {
                    // Bug #13289: if maxInt > minInt > 1, then minInt should be 1.
                    minInt = 1;
                    macros.integerWidth = IntegerWidth.ZeroFillTo(minInt).TruncateAt(maxInt);
                }
                int engineering = maxInt < 0 ? -1 : maxInt;
                macros.notation = new ScientificNotation(
                        // Engineering interval:
                        engineering,
                        // Enforce minimum integer digits (for patterns like "000.00E0"):
                        (engineering == minInt),
                        // Minimum exponent digits:
                        properties.MinimumExponentDigits,
                        // Exponent sign always shown:
                        properties.ExponentSignAlwaysShown ? SignDisplay.Always : SignDisplay.Auto);
                // Scientific notation also involves overriding the rounding mode.
                // TODO: Overriding here is a bit of a hack. Should this logic go earlier?
                if (macros.rounder is FractionRounder)
                {
                    // For the purposes of rounding, get the original min/max int/frac, since the local variables
                    // have been manipulated for display purposes.
                    int minInt_ = properties.MinimumIntegerDigits;
                    int minFrac_ = properties.MinimumFractionDigits;
                    int maxFrac_ = properties.MaximumFractionDigits;
#pragma warning disable CS0618 // Type or member is obsolete
                    if (minInt_ == 0 && maxFrac_ == 0)
                    {
                        // Patterns like "#E0" and "##E0", which mean no rounding!
                        macros.rounder = Rounder.ConstructInfinite().WithMode(mathContext);
                    }
                    else if (minInt_ == 0 && minFrac_ == 0)
                    {
                        // Patterns like "#.##E0" (no zeros in the mantissa), which mean round to maxFrac+1
                        macros.rounder = Rounder.ConstructSignificant(1, maxFrac_ + 1).WithMode(mathContext);
                    }
                    else
                    {
                        // All other scientific patterns, which mean round to minInt+maxFrac
                        macros.rounder = Rounder.ConstructSignificant(minInt_ + minFrac_, minInt_ + maxFrac_)
                                .WithMode(mathContext);
                    }
#pragma warning restore CS0618 // Type or member is obsolete
                }
            }

            //////////////////////
            // COMPACT NOTATION //
            //////////////////////

            if (properties.CompactStyle != null)
            {
                if (properties.CompactCustomData != null)
                {
                    macros.notation = new CompactNotation(properties.CompactCustomData);
                }
                else if (properties.CompactStyle == CompactStyle.Long)
                {
                    macros.notation = Notation.CompactLong;
                }
                else
                {
                    macros.notation = Notation.CompactShort;
                }
                // Do not forward the affix provider.
                macros.affixProvider = null;
            }

            /////////////////
            // MULTIPLIERS //
            /////////////////

            if (properties.MagnitudeMultiplier != 0)
            {
                macros.multiplier = new Multiplier(properties.MagnitudeMultiplier); // MultiplierImpl
            }
            else if (properties.Multiplier != null)
            {
                macros.multiplier = new Multiplier(properties.Multiplier); // MultiplierImpl
            }

            //////////////////////
            // PROPERTY EXPORTS //
            //////////////////////

            if (exportedProperties != null)
            {

                exportedProperties.MathContext = mathContext;
                exportedProperties.RoundingMode = mathContext.RoundingMode;
                exportedProperties.MinimumIntegerDigits = minInt;
                exportedProperties.MaximumIntegerDigits = (maxInt == -1 ? int.MaxValue : maxInt);

                Rounder rounding_;
                if (rounding is CurrencyRounder)
                {
                    rounding_ = ((CurrencyRounder)rounding).WithCurrency(currency);
                }
                else
                {
                    rounding_ = rounding;
                }
                int minFrac_ = minFrac;
                int maxFrac_ = maxFrac;
                int minSig_ = minSig;
                int maxSig_ = maxSig;
                BigMath.BigDecimal increment_ = null;
                if (rounding_ is FractionRounderImpl)
                {
                    minFrac_ = ((FractionRounderImpl)rounding_).minFrac;
                    maxFrac_ = ((FractionRounderImpl)rounding_).maxFrac;
                }
                else if (rounding_ is IncrementRounderImpl)
                {
                    increment_ = ((IncrementRounderImpl)rounding_).increment;
                    minFrac_ = increment_.Scale;
                    maxFrac_ = increment_.Scale;
                }
                else if (rounding_ is SignificantRounderImpl)
                {
                    minSig_ = ((SignificantRounderImpl)rounding_).minSig;
                    maxSig_ = ((SignificantRounderImpl)rounding_).maxSig;
                }

                exportedProperties.MinimumFractionDigits = minFrac_;
                exportedProperties.MaximumFractionDigits = maxFrac_;
                exportedProperties.MinimumSignificantDigits = minSig_;
                exportedProperties.MaximumSignificantDigits = maxSig_;
                exportedProperties.RoundingIncrement = increment_;
            }

            return macros;
        }

        private class PropertiesAffixPatternProvider : IAffixPatternProvider
        {
            private readonly string posPrefixPattern;
            private readonly string posSuffixPattern;
            private readonly string negPrefixPattern;
            private readonly string negSuffixPattern;

            public PropertiesAffixPatternProvider(string ppp, string psp, string npp, string nsp)
            {
                if (ppp == null)
                    ppp = "";
                if (psp == null)
                    psp = "";
                if (npp == null && nsp != null)
                    npp = "-"; // TODO: This is a hack.
                if (nsp == null && npp != null)
                    nsp = "";
                posPrefixPattern = ppp;
                posSuffixPattern = psp;
                negPrefixPattern = npp;
                negSuffixPattern = nsp;
            }

            public virtual char this[AffixPatternProviderFlags flags, int i]
            {
                get
                {
                    bool prefix = (flags & AffixPatternProviderFlags.Prefix) != 0;
                    bool negative = (flags & AffixPatternProviderFlags.NegativeSubpattern) != 0;
                    if (prefix && negative)
                    {
                        return negPrefixPattern[i];
                    }
                    else if (prefix)
                    {
                        return posPrefixPattern[i];
                    }
                    else if (negative)
                    {
                        return negSuffixPattern[i];
                    }
                    else
                    {
                        return posSuffixPattern[i];
                    }
                }
            }

            //@Override
            //public char charAt(int flags, int i)
            //{
            //    boolean prefix = (flags & Flags.PREFIX) != 0;
            //    boolean negative = (flags & Flags.NEGATIVE_SUBPATTERN) != 0;
            //    if (prefix && negative)
            //    {
            //        return negPrefixPattern.charAt(i);
            //    }
            //    else if (prefix)
            //    {
            //        return posPrefixPattern.charAt(i);
            //    }
            //    else if (negative)
            //    {
            //        return negSuffixPattern.charAt(i);
            //    }
            //    else
            //    {
            //        return posSuffixPattern.charAt(i);
            //    }
            //}

            public virtual int Length(AffixPatternProviderFlags flags)
            {
                bool prefix = (flags & AffixPatternProviderFlags.Prefix) != 0;
                bool negative = (flags & AffixPatternProviderFlags.NegativeSubpattern) != 0;
                if (prefix && negative)
                {
                    return negPrefixPattern.Length;
                }
                else if (prefix)
                {
                    return posPrefixPattern.Length;
                }
                else if (negative)
                {
                    return negSuffixPattern.Length;
                }
                else
                {
                    return posSuffixPattern.Length;
                }
            }

            public virtual bool PositiveHasPlusSign
            {
                get => AffixUtils.ContainsType(posPrefixPattern, AffixUtils.Type.PlusSign)
                        || AffixUtils.ContainsType(posSuffixPattern, AffixUtils.Type.PlusSign);
            }

            public virtual bool HasNegativeSubpattern => negPrefixPattern != null;

            public virtual bool NegativeHasMinusSign
            {
                get => AffixUtils.ContainsType(negPrefixPattern, AffixUtils.Type.MinusSign)
                        || AffixUtils.ContainsType(negSuffixPattern, AffixUtils.Type.MinusSign);
            }

            public virtual bool HasCurrencySign
            {
                get => AffixUtils.HasCurrencySymbols(posPrefixPattern) || AffixUtils.HasCurrencySymbols(posSuffixPattern)
                        || AffixUtils.HasCurrencySymbols(negPrefixPattern)
                        || AffixUtils.HasCurrencySymbols(negSuffixPattern);
            }

            public virtual bool ContainsSymbolType(AffixUtils.Type type)
            {
                return AffixUtils.ContainsType(posPrefixPattern, type) || AffixUtils.ContainsType(posSuffixPattern, type)
                        || AffixUtils.ContainsType(negPrefixPattern, type)
                        || AffixUtils.ContainsType(negSuffixPattern, type);
            }
        }

        private class CurrencyPluralInfoAffixProvider : IAffixPatternProvider
        {
            private readonly IAffixPatternProvider[] affixesByPlural;

            public CurrencyPluralInfoAffixProvider(CurrencyPluralInfo cpi)
            {
                affixesByPlural = new ParsedPatternInfo[StandardPluralUtil.Count];
                foreach (StandardPlural plural in StandardPluralUtil.Values)
                {
                    affixesByPlural[(int)plural] = PatternStringParser
                            .ParseToPatternInfo(cpi.GetCurrencyPluralPattern(plural.GetKeyword()));
                }
            }

            public virtual char this[AffixPatternProviderFlags flags, int i]
            {
                get
                {
                    int pluralOrdinal = (int)(flags & AffixPatternProviderFlags.PluralMask);
                    return affixesByPlural[pluralOrdinal][flags, i];
                }
            }

            //        @Override
            //    public char charAt(int flags, int i)
            //{
            //    int pluralOrdinal = (flags & Flags.PLURAL_MASK);
            //    return affixesByPlural[pluralOrdinal].charAt(flags, i);
            //}

            public virtual int Length(AffixPatternProviderFlags flags)
            {
                int pluralOrdinal = (int)(flags & AffixPatternProviderFlags.PluralMask);
                return affixesByPlural[pluralOrdinal].Length(flags);
            }

            public virtual bool PositiveHasPlusSign
            {
                get => affixesByPlural[(int)StandardPlural.Other].PositiveHasPlusSign;
            }

            public virtual bool HasNegativeSubpattern
            {
                get => affixesByPlural[(int)StandardPlural.Other].HasNegativeSubpattern;
            }

            public virtual bool NegativeHasMinusSign
            {
                get => affixesByPlural[(int)StandardPlural.Other].NegativeHasMinusSign;
            }

            public virtual bool HasCurrencySign
            {
                get => affixesByPlural[(int)StandardPlural.Other].HasCurrencySign;
            }

            public virtual bool ContainsSymbolType(AffixUtils.Type type)
            {
                return affixesByPlural[(int)StandardPlural.Other].ContainsSymbolType(type);
            }
        }
    }
}
