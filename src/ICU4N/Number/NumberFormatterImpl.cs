using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Text;
using static ICU4N.Numerics.CompactData;
using static ICU4N.Numerics.NumberFormatter;
using static ICU4N.Numerics.PatternStringParser;

namespace ICU4N.Numerics
{
    /// <summary>
    /// This is the "brain" of the number formatting pipeline. It ties all the pieces together, taking in a <see cref="MacroProps"/> and a
    /// <see cref="IDecimalQuantity"/> and outputting a properly formatted number string.
    /// <para/>
    /// This class, as well as <see cref="NumberPropertyMapper"/>, could go into the impl package, but they depend on too many
    /// package-private members of the public APIs.
    /// </summary>
    internal class NumberFormatterImpl // ICU4N NOTE: This is supposed to be internal
    {
        /** Builds a "safe" MicroPropsGenerator, which is thread-safe and can be used repeatedly. */
        public static NumberFormatterImpl FromMacros(MacroProps macros)
        {
            IMicroPropsGenerator microPropsGenerator = MacrosToMicroGenerator(macros, true);
            return new NumberFormatterImpl(microPropsGenerator);
        }

        /** Builds and evaluates an "unsafe" MicroPropsGenerator, which is cheaper but can be used only once. */
        public static MicroProps ApplyStatic(MacroProps macros, IDecimalQuantity inValue, NumberStringBuilder outString)
        {
            IMicroPropsGenerator microPropsGenerator = MacrosToMicroGenerator(macros, false);
            MicroProps micros = microPropsGenerator.ProcessQuantity(inValue);
            MicrosToString(micros, inValue, outString);
            return micros;
        }

        private static readonly Currency DEFAULT_CURRENCY = Currency.GetInstance("XXX");

    internal readonly IMicroPropsGenerator microPropsGenerator;

    private NumberFormatterImpl(IMicroPropsGenerator microPropsGenerator)
        {
            this.microPropsGenerator = microPropsGenerator;
        }

        public MicroProps Apply(IDecimalQuantity inValue, NumberStringBuilder outString)
        {
            MicroProps micros = microPropsGenerator.ProcessQuantity(inValue);
            MicrosToString(micros, inValue, outString);
            return micros;
        }

        //////////

        private static bool UnitIsCurrency(MeasureUnit unit)
        {
            // TODO: Check using "instanceof" operator instead?
            return unit != null && "currency".Equals(unit.Type, StringComparison.Ordinal);
        }

        private static bool UnitIsNoUnit(MeasureUnit unit)
        {
            // NOTE: In ICU4C, units cannot be null, and the default unit is a NoUnit.
            // In ICU4J, return TRUE for a null unit from this method.
            return unit == null || "none".Equals(unit.Type, StringComparison.Ordinal);
        }

        private static bool UnitIsPercent(MeasureUnit unit)
        {
            return unit != null && "percent".Equals(unit.Subtype, StringComparison.Ordinal);
        }

        private static bool UnitIsPermille(MeasureUnit unit)
        {
            return unit != null && "permille".Equals(unit.Subtype, StringComparison.Ordinal);
        }

        /**
         * Synthesizes the MacroProps into a MicroPropsGenerator. All information, including the locale, is encoded into the
         * MicroPropsGenerator, except for the quantity itself, which is left abstract and must be provided to the returned
         * MicroPropsGenerator instance.
         *
         * @see MicroPropsGenerator
         * @param macros
         *            The {@link MacroProps} to consume. This method does not mutate the MacroProps instance.
         * @param safe
         *            If true, the returned MicroPropsGenerator will be thread-safe. If false, the returned value will
         *            <em>not</em> be thread-safe, intended for a single "one-shot" use only. Building the thread-safe
         *            object is more expensive.
         */
        private static IMicroPropsGenerator MacrosToMicroGenerator(MacroProps macros, bool safe)
        {
            MicroProps micros = new MicroProps(safe);
            IMicroPropsGenerator chain = micros;

            // TODO: Normalize the currency (accept symbols from DecimalFormatSymbols)?
            // currency = CustomSymbolCurrency.resolve(currency, input.loc, micros.symbols);

            // Pre-compute a few values for efficiency.
            bool isCurrency = UnitIsCurrency(macros.unit);
            bool isNoUnit = UnitIsNoUnit(macros.unit);
            bool isPercent = isNoUnit && UnitIsPercent(macros.unit);
            bool isPermille = isNoUnit && UnitIsPermille(macros.unit);
            bool isCldrUnit = !isCurrency && !isNoUnit;
            bool isAccounting = macros.sign == SignDisplay.Accounting || macros.sign == SignDisplay.AccountingAlways;
            Currency currency = isCurrency ? (Currency)macros.unit : DEFAULT_CURRENCY;
            UnitWidth? unitWidth = UnitWidth.Short;
            if (macros.unitWidth != null)
            {
                unitWidth = macros.unitWidth;
            }
            PluralRules rules = macros.rules;

            // Select the numbering system.
            if (!(macros.symbols is NumberingSystem ns)) {
                // TODO: Is there a way to avoid creating the NumberingSystem object?
                ns = NumberingSystem.GetInstance(macros.loc);
            }
            string nsName = ns.Name;

            // Load and parse the pattern string. It is used for grouping sizes and affixes only.
            NumberFormatStyle patternStyle;
            if (isPercent || isPermille)
            {
                patternStyle = NumberFormatStyle.PercentStyle;
            }
            else if (!isCurrency || unitWidth == UnitWidth.FullName)
            {
                patternStyle = NumberFormatStyle.NumberStyle;
            }
            else if (isAccounting)
            {
                // NOTE: Although ACCOUNTING and ACCOUNTING_ALWAYS are only supported in currencies right now,
                // the API contract allows us to add support to other units in the future.
                patternStyle = NumberFormatStyle.AccountingCurrencyStyle;
            }
            else
            {
                patternStyle = NumberFormatStyle.CurrencyStyle;
            }
            string pattern = NumberFormat.GetPatternForStyleAndNumberingSystem(macros.loc, nsName, patternStyle);
            ParsedPatternInfo patternInfo = PatternStringParser.ParseToPatternInfo(pattern);

            /////////////////////////////////////////////////////////////////////////////////////
            /// START POPULATING THE DEFAULT MICROPROPS AND BUILDING THE MICROPROPS GENERATOR ///
            /////////////////////////////////////////////////////////////////////////////////////

            // Symbols
            if (macros.symbols is DecimalFormatSymbols) {
                micros.symbols = (DecimalFormatSymbols)macros.symbols;
            } else
            {
                micros.symbols = DecimalFormatSymbols.ForNumberingSystem(macros.loc, ns);
            }

            // Multiplier (compatibility mode value).
            if (macros.multiplier != null)
            {
                chain = macros.multiplier.CopyAndChain(chain);
            }

            // Rounding strategy
            if (macros.rounder != null)
            {
                micros.rounding = macros.rounder;
            }
            else if (macros.notation is CompactNotation) {
                micros.rounding = Rounder.COMPACT_STRATEGY;
            } else if (isCurrency)
            {
                micros.rounding = Rounder.MONETARY_STANDARD;
            }
            else
            {
                micros.rounding = Rounder.MAX_FRAC_6;
            }
            micros.rounding = micros.rounding.WithLocaleData(currency);

            // Grouping strategy
            if (macros.grouper != null)
            {
                micros.grouping = macros.grouper;
            }
            else if (macros.notation is CompactNotation) {
                // Compact notation uses minGrouping by default since ICU 59
                micros.grouping = Grouper.MinTwoDigits;
            } else
            {
                micros.grouping = Grouper.Defaults; // defaults();
            }
            micros.grouping = micros.grouping.WithLocaleData(patternInfo);

            // Padding strategy
            if (macros.padder != null)
            {
                micros.padding = macros.padder;
            }
            else
            {
                micros.padding = Padder.None;
            }

            // Integer width
            if (macros.integerWidth != null)
            {
                micros.integerWidth = macros.integerWidth;
            }
            else
            {
                micros.integerWidth = IntegerWidth.DEFAULT;
            }

            // Sign display
            if (macros.sign != null)
            {
                micros.sign = macros.sign;
            }
            else
            {
                micros.sign = SignDisplay.Auto;
            }

            // Decimal mark display
            if (macros.@decimal != null) {
                micros.@decimal = macros.@decimal;
            } else
            {
                micros.@decimal = DecimalSeparatorDisplay.Auto;
            }

            // Use monetary separator symbols
            micros.useCurrency = isCurrency;

            // Inner modifier (scientific notation)
            if (macros.notation is ScientificNotation) {
                chain = ((ScientificNotation)macros.notation).WithLocaleData(micros.symbols, safe, chain);
            } else
            {
                // No inner modifier required
                micros.modInner = ConstantAffixModifier.Empty;
            }

            // Middle modifier (patterns, positive/negative, currency symbols, percent)
            // The default middle modifier is weak (thus the false argument).
            MutablePatternModifier patternMod = new MutablePatternModifier(false);
            patternMod.SetPatternInfo((macros.affixProvider != null) ? macros.affixProvider : patternInfo);
            patternMod.SetPatternAttributes(micros.sign, isPermille);
            if (patternMod.NeedsPlurals)
            {
                if (rules == null)
                {
                    // Lazily create PluralRules
                    rules = PluralRules.ForLocale(macros.loc);
                }
                patternMod.SetSymbols(micros.symbols, currency, unitWidth, rules);
            }
            else
            {
                patternMod.SetSymbols(micros.symbols, currency, unitWidth, null);
            }
            if (safe)
            {
                chain = patternMod.CreateImmutableAndChain(chain);
            }
            else
            {
                chain = patternMod.AddToChain(chain);
            }

            // Outer modifier (CLDR units and currency long names)
            if (isCldrUnit)
            {
                if (rules == null)
                {
                    // Lazily create PluralRules
                    rules = PluralRules.ForLocale(macros.loc);
                }
                chain = LongNameHandler.ForMeasureUnit(macros.loc, macros.unit, unitWidth, rules, chain);
            }
            else if (isCurrency && unitWidth == UnitWidth.FullName)
            {
                if (rules == null)
                {
                    // Lazily create PluralRules
                    rules = PluralRules.ForLocale(macros.loc);
                }
                chain = LongNameHandler.ForCurrencyLongNames(macros.loc, currency, rules, chain);
            }
            else
            {
                // No outer modifier required
                micros.modOuter = ConstantAffixModifier.Empty;
            }

            // Compact notation
            // NOTE: Compact notation can (but might not) override the middle modifier and rounding.
            // It therefore needs to go at the end of the chain.
            if (macros.notation is CompactNotation) {
                if (rules == null)
                {
                    // Lazily create PluralRules
                    rules = PluralRules.ForLocale(macros.loc);
                }
                CompactType compactType = (macros.unit is Currency && macros.unitWidth != UnitWidth.FullName)
                    ? CompactType.Currency
                        : CompactType.Decimal;
                chain = ((CompactNotation)macros.notation).WithLocaleData(macros.loc, nsName, compactType, rules,
                        safe ? patternMod : null, chain);
            }

            return chain;
        }

        //////////

        /**
         * Synthesizes the output string from a MicroProps and DecimalQuantity.
         *
         * @param micros
         *            The MicroProps after the quantity has been consumed. Will not be mutated.
         * @param quantity
         *            The DecimalQuantity to be rendered. May be mutated.
         * @param string
         *            The output string. Will be mutated.
         */
        private static void MicrosToString(MicroProps micros, IDecimalQuantity quantity, NumberStringBuilder str)
        {
            micros.rounding.Apply(quantity);
            if (micros.integerWidth.maxInt == -1)
            {
                quantity.SetIntegerLength(micros.integerWidth.minInt, int.MaxValue);
            }
            else
            {
                quantity.SetIntegerLength(micros.integerWidth.minInt, micros.integerWidth.maxInt);
            }
            int length = WriteNumber(micros, quantity, str);
            // NOTE: When range formatting is added, these modifiers can bubble up.
            // For now, apply them all here at once.
            // Always apply the inner modifier (which is "strong").
            length += micros.modInner.Apply(str, 0, length);
            if (micros.padding.IsValid)
            {
                micros.padding.PadAndApply(micros.modMiddle, micros.modOuter, str, 0, length);
            }
            else
            {
                length += micros.modMiddle.Apply(str, 0, length);
                length += micros.modOuter.Apply(str, 0, length);
            }
        }

        private static int WriteNumber(MicroProps micros, IDecimalQuantity quantity, NumberStringBuilder str)
        {
            int length = 0;
            if (quantity.IsInfinity)
            {
                length += str.Insert(length, micros.symbols.Infinity, NumberFormatField.Integer);

            }
            else if (quantity.IsNaN)
            {
                length += str.Insert(length, micros.symbols.NaN, NumberFormatField.Integer);

            }
            else
            {
                // Add the integer digits
                length += WriteIntegerDigits(micros, quantity, str);

                // Add the decimal point
                if (quantity.LowerDisplayMagnitude < 0 || micros.@decimal == DecimalSeparatorDisplay.Always) {
                    length += str.Insert(length, micros.useCurrency ? micros.symbols.MonetaryDecimalSeparatorString
                            : micros.symbols.DecimalSeparatorString, NumberFormatField.DecimalSeparator);
                }

                // Add the fraction digits
                length += WriteFractionDigits(micros, quantity, str);
            }

            return length;
        }

        private static int WriteIntegerDigits(MicroProps micros, IDecimalQuantity quantity, NumberStringBuilder str)
        {
            int length = 0;
            int integerCount = quantity.UpperDisplayMagnitude + 1;
            for (int i = 0; i < integerCount; i++)
            {
                // Add grouping separator
                if (micros.grouping.GroupAtPosition(i, quantity))
                {
                    length += str.Insert(0, micros.useCurrency ? micros.symbols.MonetaryGroupingSeparatorString
                            : micros.symbols.GroupingSeparatorString, NumberFormatField.GroupingSeparator);
                }

                // Get and append the next digit value
                byte nextDigit = quantity.GetDigit(i);
                if (micros.symbols.CodePointZero != -1)
                {
                    length += str.InsertCodePoint(0, micros.symbols.CodePointZero + nextDigit,
                            NumberFormatField.Integer);
                }
                else
                {
                    length += str.Insert(0, micros.symbols.DigitStringsLocal[nextDigit],
                            NumberFormatField.Integer);
                }
            }
            return length;
        }

        private static int WriteFractionDigits(MicroProps micros, IDecimalQuantity quantity, NumberStringBuilder str)
        {
            int length = 0;
            int fractionCount = -quantity.LowerDisplayMagnitude;
            for (int i = 0; i < fractionCount; i++)
            {
                // Get and append the next digit value
                byte nextDigit = quantity.GetDigit(-i - 1);
                if (micros.symbols.CodePointZero != -1)
                {
                    length += str.AppendCodePoint(micros.symbols.CodePointZero + nextDigit,
                            NumberFormatField.Fraction);
                }
                else
                {
                    length += str.Append(micros.symbols.DigitStringsLocal[nextDigit], NumberFormatField.Fraction);
                }
            }
            return length;
        }
    }
}
