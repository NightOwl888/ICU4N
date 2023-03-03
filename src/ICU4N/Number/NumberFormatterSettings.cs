using ICU4N.Globalization;
using ICU4N.Text;
using ICU4N.Util;
using System;
using static ICU4N.Numerics.NumberFormatter;
using Long = J2N.Numerics.Int64;

namespace ICU4N.Numerics
{
    // ICU4N specific - added this class to work around lack of wildcard generics in C#
    internal abstract class NumberFormatterSettings
    {
        internal const int KEY_MACROS = 0;
        internal const int KEY_LOCALE = 1;
        internal const int KEY_NOTATION = 2;
        internal const int KEY_UNIT = 3;
        internal const int KEY_ROUNDER = 4;
        internal const int KEY_GROUPER = 5;
        internal const int KEY_PADDER = 6;
        internal const int KEY_INTEGER = 7;
        internal const int KEY_SYMBOLS = 8;
        internal const int KEY_UNIT_WIDTH = 9;
        internal const int KEY_SIGN = 10;
        internal const int KEY_DECIMAL = 11;
        internal const int KEY_THRESHOLD = 12;
        internal const int KEY_MAX = 13;

        internal readonly NumberFormatterSettings/*<?>*/ parent;
        internal readonly int key;
        internal readonly object value;
        internal volatile MacroProps resolvedMacros;

        private protected NumberFormatterSettings(NumberFormatterSettings parent, int key, object value) // ICU4N TODO: parent is nullable
        {
            this.parent = parent;
            this.key = key;
            this.value = value;
        }

        internal MacroProps Resolve()
        {
            if (resolvedMacros != null)
            {
                return resolvedMacros;
            }
            // Although the linked-list fluent storage approach requires this method,
            // my benchmarks show that linked-list is still faster than a full clone
            // of a MacroProps object at each step.
            // TODO: Remove the reference to the parent after the macros are resolved?
            MacroProps macros = new MacroProps();
            NumberFormatterSettings /*<?>*/ current = this;
            while (current != null)
            {
                switch (current.key)
                {
                    case KEY_MACROS:
                        macros.Fallback((MacroProps)current.value);
                        break;
                    case KEY_LOCALE:
                        if (macros.loc == null)
                        {
                            macros.loc = (UCultureInfo)current.value;
                        }
                        break;
                    case KEY_NOTATION:
                        if (macros.notation == null)
                        {
                            macros.notation = (Notation)current.value;
                        }
                        break;
                    case KEY_UNIT:
                        if (macros.unit == null)
                        {
                            macros.unit = (MeasureUnit)current.value;
                        }
                        break;
                    case KEY_ROUNDER:
                        if (macros.rounder == null)
                        {
                            macros.rounder = (Rounder)current.value;
                        }
                        break;
                    case KEY_GROUPER:
                        if (macros.grouper == null)
                        {
#pragma warning disable CS0618 // Type or member is obsolete
                            macros.grouper = (Grouper)current.value;
#pragma warning restore CS0618 // Type or member is obsolete
                        }
                        break;
                    case KEY_PADDER:
                        if (macros.padder == null)
                        {
                            macros.padder = (Padder)current.value;
                        }
                        break;
                    case KEY_INTEGER:
                        if (macros.integerWidth == null)
                        {
                            macros.integerWidth = (IntegerWidth)current.value;
                        }
                        break;
                    case KEY_SYMBOLS:
                        if (macros.symbols == null)
                        {
                            macros.symbols = /* (Object) */ current.value;
                        }
                        break;
                    case KEY_UNIT_WIDTH:
                        if (macros.unitWidth == null)
                        {
                            macros.unitWidth = (UnitWidth)current.value;
                        }
                        break;
                    case KEY_SIGN:
                        if (macros.sign == null)
                        {
                            macros.sign = (SignDisplay)current.value;
                        }
                        break;
                    case KEY_DECIMAL:
                        if (macros.@decimal == null)
                        {
                            macros.@decimal = (DecimalSeparatorDisplay)current.value;
                        }
                        break;
                    case KEY_THRESHOLD:
                        if (macros.threshold == null)
                        {
                            macros.threshold = (Long)current.value;
                        }
                        break;
                    default:
                        //throw new AssertionError("Unknown key: " + current.key);
                        throw new InvalidOperationException("Unknown key: " + current.key);
                }
                current = current.parent;
            }
            resolvedMacros = macros;
            return macros;
        }
    }

    internal abstract class NumberFormatterSettings<T> : NumberFormatterSettings
        where T : NumberFormatterSettings
    {
        // ICU4N: Moved parent/key/value/resolvedMacros to NumberFormatSettings

        internal NumberFormatterSettings(NumberFormatterSettings/*<?>*/ parent, int key, object value)
            : base(parent, key, value)
        {
        }

        /// <summary>
        /// Specifies the notation style (simple, scientific, or compact) for rendering numbers.
        /// 
        /// <list type="bullet">
        ///     <item><term>Simple notation</term><description>"12,300"</description></item>
        ///     <item><term>Scientific notation</term><description>"1.23E4"</description></item>
        ///     <item><term>Compact notation</term><description>"12K"</description></item>
        /// </list>
        /// 
        /// <para/>
        /// All notation styles will be properly localized with locale data, and all notation styles are compatible with
        /// units, rounding strategies, and other number formatter settings.
        /// 
        /// <para/>
        /// Pass this method the return value of a <see cref="Notation(Numerics.Notation)"/> factory method. For example:
        /// 
        /// <code>
        /// NumberFormatter.With().Notation(Notation.CompactShort)
        /// </code>
        /// 
        /// The default is to use simple notation.
        /// </summary>
        /// <param name="notation">The notation strategy to use.</param>
        /// <returns>The fluent chain.</returns>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual T Notation(Notation notation)
        {
            return Create(KEY_NOTATION, notation);
        }

        /// <summary>
        /// Specifies the unit (unit of measure, currency, or percent) to associate with rendered numbers.
        /// 
        /// <list type="bullet">
        ///     <item><term>Unit of measure</term><description>"12.3 meters"</description></item>
        ///     <item><term>Currency</term><description>"$12.30"</description></item>
        ///     <item><term>Percent</term><description>"12.3%"</description></item>
        /// </list>
        /// 
        /// <para/>
        /// <strong>Note:</strong> The unit can also be specified by passing a <see cref="Measure"/> to
        /// <see cref="LocalizedNumberFormatter.Format(Measure)"/>. Units specified via the
        /// <see cref="LocalizedNumberFormatter.Format(Measure)"/> method take precedence over
        /// units specified here. This setter is designed for situations when the unit is constant for the duration of the
        /// number formatting process.
        /// 
        /// <para/>
        /// All units will be properly localized with locale data, and all units are compatible with notation styles,
        /// rounding strategies, and other number formatter settings.
        /// 
        /// <para/>
        /// Pass this method any instance of <see cref="MeasureUnit"/>. For units of measure:
        /// 
        /// <code>
        /// NumberFormatter.With().Unit(MeasureUnit.Meter)
        /// </code>
        /// 
        /// Currency:
        /// 
        /// <code>
        /// NumberFormatter.With().Unit(Currency.GetInstance("USD"))
        /// </code>
        /// 
        /// Percent:
        /// 
        /// <code>
        /// NumberFormatter.With().Unit(NoUnit.Percent)
        /// </code>
        /// 
        /// The default is to render without units (equivalent to <see cref="NoUnit.Base"/>).
        /// </summary>
        /// <param name="unit">The unit to render.</param>
        /// <returns>The fluent chain.</returns>
        /// <seealso cref="MeasureUnit"/>
        /// <seealso cref="Currency"/>
        /// <seealso cref="NoUnit"/>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual T Unit(MeasureUnit unit)
        {
            return Create(KEY_UNIT, unit);
        }

        /// <summary>
        /// Specifies the rounding strategy to use when formatting numbers.
        /// 
        /// <list type="bullet">
        ///     <item><term>Round to 3 decimal places</term><description>"3.142"</description></item>
        ///     <item><term>Round to 3 significant figures</term><description>"3.14"</description></item>
        ///     <item><term>Round to the closest nickel</term><description>"3.15"</description></item>
        ///     <item><term>Do not perform rounding</term><description>"3.1415926..."</description></item>
        /// </list>
        /// 
        /// <para/>
        /// Pass this method the return value of one of the factory methods on <see cref="Rounder"/>. For example:
        /// 
        /// <code>
        /// NumberFormatter.With().Rounding(Rounder.FixedFraction(2))
        /// </code>
        /// 
        /// <para/>
        /// In most cases, the default rounding strategy is to round to 6 fraction places; i.e.,
        /// <c>Rounder.MaxFraction(6)</c>. The exceptions are if compact notation is being used, then the compact
        /// notation rounding strategy is used (see <see cref="Numerics.Notation.CompactShort"/> for details),
        /// or if the unit is a currency, then standard currency rounding is used, which varies from currency
        /// to currency (see <see cref="Rounder.Currency(CurrencyUsage)"/> for details).
        /// </summary>
        /// <param name="rounder">The rounding strategy to use.</param>
        /// <returns>The fluent chain.</returns>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual T Rounding(Rounder rounder)
        {
            return Create(KEY_ROUNDER, rounder);
        }

        /// <summary>
        /// Specifies the grouping strategy to use when formatting numbers.
        /// 
        /// <list type="bullet">
        ///     <item><term>Default grouping</term><description>"12,300" and "1,230"</description></item>
        ///     <item><term>Grouping with at least 2 digits</term><description>"12,300" and "1230"</description></item>
        ///     <item><term></term><description>"12300" and "1230"</description></item>
        /// </list>
        /// 
        /// <para/>
        /// The exact grouping widths will be chosen based on the locale.
        /// 
        /// <para/>
        /// Pass this method the return value of one of the factory methods on <see cref="Grouper"/>. For example:
        /// 
        /// <code>
        /// NumberFormatter.With().Grouping(Grouper.MinTwoDigits)
        /// </code>
        /// 
        /// The default is to perform grouping without concern for the minimum grouping digits.
        /// </summary>
        /// <param name="grouper">The grouping strategy to use.</param>
        /// <returns>The fluent chain.</returns>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        [Obsolete("ICU 60 This API is technical preview; see #7861.")]
        public virtual T Grouping(Grouper grouper)
        {
            return Create(KEY_GROUPER, grouper);
        }

        /// <summary>
        /// Specifies the minimum and maximum number of digits to render before the decimal mark.
        /// 
        /// <list type="bullet">
        ///     <item><term>Zero minimum integer digits</term><description>".08"</description></item>
        ///     <item><term>One minimum integer digit</term><description>"0.08"</description></item>
        ///     <item><term>Two minimum integer digits</term><description>"00.08"</description></item>
        /// </list>
        /// 
        /// <para/>
        /// Pass this method the return value of <see cref="Numerics.IntegerWidth.ZeroFillTo(int)"/>. For example:
        /// 
        /// <code>
        /// NumberFormatter.With().IntegerWidth(IntegerWidth.ZeroFillTo(2))
        /// </code>
        /// 
        /// The default is to have one minimum integer digit
        /// </summary>
        /// <param name="style">The integer width to use.</param>
        /// <returns>The fluent chain.</returns>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual T IntegerWidth(IntegerWidth style)
        {
            return Create(KEY_INTEGER, style);
        }

        /// <summary>
        /// Specifies the symbols (decimal separator, grouping separator, percent sign, numerals, etc.) to use when rendering
        /// numbers.
        /// 
        /// <list type="bullet">
        ///     <item><term><em>en_US</em> symbols</term><description>"12,345.67"</description></item>
        ///     <item><term><em>fr_FR</em> symbols</term><description>"12 345,67"</description></item>
        ///     <item><term><em>de_CH</em> symbols</term><description>"12’345.67"</description></item>
        ///     <item><term><em>my_MY</em> symbols</term><description>"၁၂,၃၄၅.၆၇"</description></item>
        /// </list>
        /// 
        /// <para/>
        /// Pass this method an instance of <see cref="DecimalFormatSymbols"/>. For example:
        /// 
        /// <code>
        /// NumberFormatter.With().Symbols(DecimalFormatSymbols.GetInstance(new UCultureInfo("de_CH")))
        /// </code>
        /// 
        /// <para/>
        /// <strong>Note:</strong> The instance of <see cref="DecimalFormatSymbols"/> will be copied: changes made to the symbols object
        /// In the examples above, the first three are using the Latin numbering system, and the fourth is using the Myanmar
        /// numbering system.
        /// 
        /// <para/>
        /// <strong>Note:</strong> Calling this method will override the <see cref="NumberingSystem"/> previously specified in
        /// <see cref="Symbols(NumberingSystem)"/>.
        /// 
        /// <para/>
        /// The default is to choose the symbols based on the locale specified in the fluent chain.
        /// </summary>
        /// <param name="symbols">The <see cref="DecimalFormatSymbols"/> to use.</param>
        /// <returns>The fluent chain.</returns>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual T Symbols(DecimalFormatSymbols symbols)
        {
            symbols = (DecimalFormatSymbols)symbols.Clone();
            return Create(KEY_SYMBOLS, symbols);
        }

        /// <summary>
        /// Specifies that the given numbering system should be used when fetching symbols.
        /// 
        /// <list type="bullet">
        ///     <item><term>Latin numbering system</term><description>"12,345"</description></item>
        ///     <item><term>Myanmar numbering system</term><description>"၁၂,၃၄၅"</description></item>
        ///     <item><term>Math Sans Bold numbering system</term><description>"𝟭𝟮,𝟯𝟰𝟱"</description></item>
        /// </list>
        /// 
        /// <para/>
        /// Pass this method an instance of <see cref="NumberingSystem"/>. For example, to force the locale to always use the Latin
        /// alphabet numbering system (ASCII digits):
        /// 
        /// <code>
        /// NumberFormatter.With().Symbols(NumberingSystem.Latin)
        /// </code>
        /// 
        /// <para/>
        /// <strong>Note:</strong> Calling this method will override the <see cref="DecimalFormatSymbols"/> previously specified in
        /// <see cref="Symbols(DecimalFormatSymbols)"/>
        /// 
        /// <para/>
        /// The default is to choose the best numbering system for the locale.
        /// </summary>
        /// <param name="ns">The <see cref="NumberingSystem"/> to use.</param>
        /// <returns>The fluent chain.</returns>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual T Symbols(NumberingSystem ns)
        {
            return Create(KEY_SYMBOLS, ns);
        }

        /// <summary>
        /// Sets the width of the unit (measure unit or currency). Most common values:
        /// 
        /// <list type="bullet">
        ///     <item><term>Short</term><description>"$12.00", "12 m"</description></item>
        ///     <item><term>ISO Code</term><description>"USD 12.00"</description></item>
        ///     <item><term>Full name</term><description>"12.00 US dollars", "12 meters"</description></item>
        /// </list>
        /// 
        /// <para/>
        /// Pass an element from the <see cref="NumberFormatter.UnitWidth"/> enum to this setter. For example:
        /// 
        /// <code>
        /// NumberFormatter.With().UnitWidth(UnitWidth.FullName)
        /// </code>
        /// 
        /// <para/>
        /// The default is the <see cref="NumberFormatter.UnitWidth.Short"/> width.
        /// </summary>
        /// <param name="style">The width to use when rendering numbers.</param>
        /// <returns>The fluent chain.</returns>
        /// <seealso cref="NumberFormatter.UnitWidth"/>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual T UnitWidth(UnitWidth style)
        {
            return Create(KEY_UNIT_WIDTH, style);
        }

        /// <summary>
        /// Sets the plus/minus sign display strategy. Most common values:
        /// 
        /// <list type="bullet">
        ///     <item><term>Auto</term><description>"123", "-123"</description></item>
        ///     <item><term>Always</term><description>"+123", "-123"</description></item>
        ///     <item><term>Accounting</term><description>"$123", "($123)"</description></item>
        /// </list>
        /// 
        /// <para/>
        /// Pass an element from the <see cref="SignDisplay"/> enum to this setter. For example:
        /// 
        /// <code>
        /// NumberFormatter.With().Sign(SignDisplay.Always)
        /// </code>
        /// 
        /// <para/>
        /// The default is <see cref="SignDisplay.Auto"/> sign display.
        /// </summary>
        /// <param name="style">The sign display strategy to use when rendering numbers.</param>
        /// <returns>The fluent chain.</returns>
        /// <seealso cref="Sign(SignDisplay)"/>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual T Sign(SignDisplay style)
        {
            return Create(KEY_SIGN, style);
        }

        /// <summary>
        /// Sets the decimal separator display strategy. This affects integer numbers with no fraction part. Most common
        /// values:
        /// 
        /// <list type="bullet">
        ///     <item><term>Auto</term><description>"1"</description></item>
        ///     <item><term>Always</term><description>"1."</description></item>
        /// </list>
        /// 
        /// <para/>
        /// Pass an element from the <see cref="DecimalSeparatorDisplay"/> enum to this setter. For example:
        /// 
        /// <code>
        /// NumberFormatter.With().Decimal(DecimalSeparatorDisplay.Always)
        /// </code>
        /// 
        /// <para/>
        /// The default is <see cref="DecimalSeparatorDisplay.Auto"/> decimal separator display.
        /// </summary>
        /// <param name="style"></param>
        /// <returns>The fluent chain.</returns>
        /// <seealso cref="DecimalSeparatorDisplay"/>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual T Decimal(DecimalSeparatorDisplay style)
        {
            return Create(KEY_DECIMAL, style);
        }

        /// <summary>
        /// Internal method to set a starting macros.
        /// </summary>
        /// <internal/>
        [Obsolete("ICU 60 This API is ICU internal only.")]
        public virtual T Macros(MacroProps macros)
        {
            return Create(KEY_MACROS, macros);
        }

        /// <summary>
        /// Set the padding strategy. May be added to ICU 61; see #13338.
        /// </summary>
        /// <internal/>
        [Obsolete("ICU 60 This API is ICU internal only.")]
        public virtual T Padding(Padder padder)
        {
            return Create(KEY_PADDER, padder);
        }

        /// <summary>
        /// Internal fluent setter to support a custom regulation threshold. A threshold of 1 causes the data structures to
        /// be built right away. A threshold of 0 prevents the data structures from being built.
        /// </summary>
        /// <internal/>
        [Obsolete("ICU 60 This API is ICU internal only.")]
        public virtual T Threshold(Long threshold)
        {
            return Create(KEY_THRESHOLD, threshold);
        }

        /* package-protected */
        internal abstract T Create(int key, object value);

        // ICU4N: Moved Resolve() to NumberFormatterSettings class

        /// <inheritdoc/>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public override int GetHashCode()
        {
            return Resolve().GetHashCode();
        }

        /// <inheritdoc/>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public override bool Equals(object other)
        {
            if (this == other)
            {
                return true;
            }
            if (other is null)
            {
                return false;
            }
            if (other is NumberFormatterSettings o)
            {
                return Resolve().Equals(o.Resolve());
            }
            return false;
        }
    }
}
