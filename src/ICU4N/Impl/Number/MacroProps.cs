using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using static ICU4N.Numerics.NumberFormatter;
using Long = J2N.Numerics.Int64;

namespace ICU4N.Numerics
{
    internal class MacroProps // ICU4N TODO: API - this was public in ICU4J
#if FEATURE_CLONEABLE
        , ICloneable
#endif
    {
        public Notation notation;
        public MeasureUnit unit;
        public Rounder rounder;
        public Grouper grouper;
        public Padder padder;
        public IntegerWidth integerWidth;
        public object symbols;
        public UnitWidth? unitWidth;
        public SignDisplay? sign;
        public DecimalSeparatorDisplay? @decimal;
        public IAffixPatternProvider affixProvider; // not in API; for JDK compatibility mode only
        public Multiplier multiplier; // not in API; for JDK compatibility mode only
        public PluralRules rules; // not in API; could be made public in the future
        public Long threshold; // not in API; controls internal self-regulation threshold
        public UCultureInfo loc;

        /**
         * Copies values from fallback into this instance if they are null in this instance.
         *
         * @param fallback The instance to copy from; not modified by this operation.
         */
        public virtual void Fallback(MacroProps fallback)
        {
            notation ??= fallback.notation;
            unit ??= fallback.unit;
            rounder ??= fallback.rounder;
            grouper ??= fallback.grouper;
            padder ??= fallback.padder;
            integerWidth ??= fallback.integerWidth;
            symbols ??= fallback.symbols;
            unitWidth ??= fallback.unitWidth;
            sign ??= fallback.sign;
            @decimal ??= fallback.@decimal;
            affixProvider ??= fallback.affixProvider;
            multiplier ??= fallback.multiplier;
            rules ??= fallback.rules;
            loc ??= fallback.loc;
        }

        public override int GetHashCode()
        {
            int hashCode = 1;
            hashCode = 31 * hashCode + notation?.GetHashCode() ?? 0;
            hashCode = 31 * hashCode + unit?.GetHashCode() ?? 0;
            hashCode = 31 * hashCode + rounder?.GetHashCode() ?? 0;
            hashCode = 31 * hashCode + grouper?.GetHashCode() ?? 0;
            hashCode = 31 * hashCode + padder?.GetHashCode() ?? 0;
            hashCode = 31 * hashCode + integerWidth?.GetHashCode() ?? 0;
            hashCode = 31 * hashCode + symbols?.GetHashCode() ?? 0;
            hashCode = 31 * hashCode + unitWidth?.GetHashCode() ?? 0;
            hashCode = 31 * hashCode + sign?.GetHashCode() ?? 0;
            hashCode = 31 * hashCode + @decimal?.GetHashCode() ?? 0;
            hashCode = 31 * hashCode + affixProvider?.GetHashCode() ?? 0;
            hashCode = 31 * hashCode + multiplier?.GetHashCode() ?? 0;
            hashCode = 31 * hashCode + rules?.GetHashCode() ?? 0;
            hashCode = 31 * hashCode + loc?.GetHashCode() ?? 0;
            return hashCode;

            //return Utility.Hash(
            //    notation,
            //    unit,
            //    rounder,
            //    grouper,
            //    padder,
            //    integerWidth,
            //    symbols,
            //    unitWidth,
            //    sign,
            //    @decimal,
            //    affixProvider,
            //    multiplier,
            //    rules,
            //    loc);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (this == obj) return true;
            if (!(obj is MacroProps other)) return false;

            return EqualityComparer<Notation>.Default.Equals(notation, other.notation)
               && EqualityComparer<MeasureUnit>.Default.Equals(unit, other.unit)
               && EqualityComparer<Rounder>.Default.Equals(rounder, other.rounder)
               && EqualityComparer<Grouper>.Default.Equals(grouper, other.grouper)
               && EqualityComparer<Padder>.Default.Equals(padder, other.padder)
               && EqualityComparer<IntegerWidth>.Default.Equals(integerWidth, other.integerWidth)
               && EqualityComparer<object>.Default.Equals(symbols, other.symbols)
               && EqualityComparer<UnitWidth?>.Default.Equals(unitWidth, other.unitWidth)
               && EqualityComparer<SignDisplay?>.Default.Equals(sign, other.sign)
               && EqualityComparer<DecimalSeparatorDisplay?>.Default.Equals(@decimal, other.@decimal)
               && EqualityComparer<IAffixPatternProvider>.Default.Equals(affixProvider, other.affixProvider)
               && EqualityComparer<Multiplier>.Default.Equals(multiplier, other.multiplier)
               && EqualityComparer<PluralRules>.Default.Equals(rules, other.rules)
               && EqualityComparer<UCultureInfo>.Default.Equals(loc, other.loc);
        }

        public virtual object Clone()
        {
            // TODO: Remove this method?
            return MemberwiseClone();
        }
    }
}
