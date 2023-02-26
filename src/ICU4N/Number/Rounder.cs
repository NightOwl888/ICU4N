using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ICU4N.Numerics
{
    /// <summary>
    /// A class that defines the rounding strategy to be used when formatting numbers in <see cref="NumberFormatter"/>.
    /// <para/>
    /// To create a <see cref="Rounder"/>, use one of the factory methods.
    /// </summary>
    /// <draft>ICU 60</draft>
    /// <provisional>This API might change or be removed in a future release.</provisional>
    /// <seealso cref="NumberFormatter"/>
    internal abstract class Rounder // ICU4N TODO: API - this was public in ICU4J
#if FEATURE_CLONEABLE
        , ICloneable
#endif
    {
        /* package-private final */
        internal BigMath.MathContext mathContext;

        /* package-private */
        internal Rounder()
        {
            mathContext = RoundingUtils.MathContextUnlimited(RoundingUtils.DefaultRoundingMode);
        }

        /**
         * Show all available digits to full precision.
         *
         * <para/>
         * <strong>NOTE:</strong> When formatting a <em>double</em>, this method, along with {@link #minFraction} and
         * {@link #minDigits}, will trigger complex algorithm similar to <em>Dragon4</em> to determine the low-order digits
         * and the number of digits to display based on the value of the double. If the number of fraction places or
         * significant digits can be bounded, consider using {@link #maxFraction} or {@link #maxDigits} instead to maximize
         * performance. For more information, read the following blog post.
         *
         * <para/>
         * http://www.serpentine.com/blog/2011/06/29/here-be-dragons-advances-in-problems-you-didnt-even-know-you-had/
         *
         * @return A Rounder for chaining or passing to the NumberFormatter rounding() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public static Rounder Unlimited => ConstructInfinite();

        /**
         * Show numbers rounded if necessary to the nearest integer.
         *
         * @return A FractionRounder for chaining or passing to the NumberFormatter rounding() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public static FractionRounder Integer => ConstructFraction(0, 0);

        /**
         * Show numbers rounded if necessary to a certain number of fraction places (numerals after the decimal separator).
         * Additionally, pad with zeros to ensure that this number of places are always shown.
         *
         * <para/>
         * Example output with minMaxFractionPlaces = 3:
         *
         * <para/>
         * 87,650.000<br>
         * 8,765.000<br>
         * 876.500<br>
         * 87.650<br>
         * 8.765<br>
         * 0.876<br>
         * 0.088<br>
         * 0.009<br>
         * 0.000 (zero)
         *
         * <para/>
         * This method is equivalent to {@link #minMaxFraction} with both arguments equal.
         *
         * @param minMaxFractionPlaces
         *            The minimum and maximum number of numerals to display after the decimal separator (rounding if too
         *            long or padding with zeros if too short).
         * @return A FractionRounder for chaining or passing to the NumberFormatter rounding() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public static FractionRounder FixedFraction(int minMaxFractionPlaces)
        {
            if (minMaxFractionPlaces >= 0 && minMaxFractionPlaces <= RoundingUtils.MAX_INT_FRAC_SIG)
            {
                return ConstructFraction(minMaxFractionPlaces, minMaxFractionPlaces);
            }
            else
            {
                throw new ArgumentException(
                        "Fraction length must be between 0 and " + RoundingUtils.MAX_INT_FRAC_SIG);
            }
        }

        /**
         * Always show at least a certain number of fraction places after the decimal separator, padding with zeros if
         * necessary. Do not perform rounding (display numbers to their full precision).
         *
         * <para/>
         * <strong>NOTE:</strong> If you are formatting <em>doubles</em>, see the performance note in {@link #unlimited}.
         *
         * @param minFractionPlaces
         *            The minimum number of numerals to display after the decimal separator (padding with zeros if
         *            necessary).
         * @return A FractionRounder for chaining or passing to the NumberFormatter rounding() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public static FractionRounder MinFraction(int minFractionPlaces)
        {
            if (minFractionPlaces >= 0 && minFractionPlaces < RoundingUtils.MAX_INT_FRAC_SIG)
            {
                return ConstructFraction(minFractionPlaces, -1);
            }
            else
            {
                throw new ArgumentException(
                        "Fraction length must be between 0 and " + RoundingUtils.MAX_INT_FRAC_SIG);
            }
        }

        /**
         * Show numbers rounded if necessary to a certain number of fraction places (numerals after the decimal separator).
         * Unlike the other fraction rounding strategies, this strategy does <em>not</em> pad zeros to the end of the
         * number.
         *
         * @param maxFractionPlaces
         *            The maximum number of numerals to display after the decimal mark (rounding if necessary).
         * @return A FractionRounder for chaining or passing to the NumberFormatter rounding() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public static FractionRounder MaxFraction(int maxFractionPlaces)
        {
            if (maxFractionPlaces >= 0 && maxFractionPlaces < RoundingUtils.MAX_INT_FRAC_SIG)
            {
                return ConstructFraction(0, maxFractionPlaces);
            }
            else
            {
                throw new ArgumentException(
                        "Fraction length must be between 0 and " + RoundingUtils.MAX_INT_FRAC_SIG); // ICU4N TODO: Guard clause exception types
            }
        }

        /**
         * Show numbers rounded if necessary to a certain number of fraction places (numerals after the decimal separator);
         * in addition, always show at least a certain number of places after the decimal separator, padding with zeros if
         * necessary.
         *
         * @param minFractionPlaces
         *            The minimum number of numerals to display after the decimal separator (padding with zeros if
         *            necessary).
         * @param maxFractionPlaces
         *            The maximum number of numerals to display after the decimal separator (rounding if necessary).
         * @return A FractionRounder for chaining or passing to the NumberFormatter rounding() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public static FractionRounder MinMaxFraction(int minFractionPlaces, int maxFractionPlaces)
        {
            if (minFractionPlaces >= 0 && maxFractionPlaces <= RoundingUtils.MAX_INT_FRAC_SIG
                    && minFractionPlaces <= maxFractionPlaces)
            {
                return ConstructFraction(minFractionPlaces, maxFractionPlaces);
            }
            else
            {
                throw new ArgumentException(
                        "Fraction length must be between 0 and " + RoundingUtils.MAX_INT_FRAC_SIG); // ICU4N TODO: Guard clause exception types
            }
        }

        /**
         * Show numbers rounded if necessary to a certain number of significant digits or significant figures. Additionally,
         * pad with zeros to ensure that this number of significant digits/figures are always shown.
         *
         * <para/>
         * This method is equivalent to {@link #minMaxDigits} with both arguments equal.
         *
         * @param minMaxSignificantDigits
         *            The minimum and maximum number of significant digits to display (rounding if too long or padding with
         *            zeros if too short).
         * @return A Rounder for chaining or passing to the NumberFormatter rounding() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public static Rounder FixedDigits(int minMaxSignificantDigits)
        {
            if (minMaxSignificantDigits > 0 && minMaxSignificantDigits <= RoundingUtils.MAX_INT_FRAC_SIG)
            {
                return ConstructSignificant(minMaxSignificantDigits, minMaxSignificantDigits);
            }
            else
            {
                throw new ArgumentException(
                        "Significant digits must be between 0 and " + RoundingUtils.MAX_INT_FRAC_SIG); // ICU4N TODO: Guard clause exception types
            }
        }

        /**
         * Always show at least a certain number of significant digits/figures, padding with zeros if necessary. Do not
         * perform rounding (display numbers to their full precision).
         *
         * <para/>
         * <strong>NOTE:</strong> If you are formatting <em>doubles</em>, see the performance note in {@link #unlimited}.
         *
         * @param minSignificantDigits
         *            The minimum number of significant digits to display (padding with zeros if too short).
         * @return A Rounder for chaining or passing to the NumberFormatter rounding() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public static Rounder MinDigits(int minSignificantDigits)
        {
            if (minSignificantDigits > 0 && minSignificantDigits <= RoundingUtils.MAX_INT_FRAC_SIG)
            {
                return ConstructSignificant(minSignificantDigits, -1);
            }
            else
            {
                throw new ArgumentException(
                        "Significant digits must be between 0 and " + RoundingUtils.MAX_INT_FRAC_SIG); // ICU4N TODO: Guard clause exception types
            }
        }

        /**
         * Show numbers rounded if necessary to a certain number of significant digits/figures.
         *
         * @param maxSignificantDigits
         *            The maximum number of significant digits to display (rounding if too long).
         * @return A Rounder for chaining or passing to the NumberFormatter rounding() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public static Rounder MaxDigits(int maxSignificantDigits)
        {
            if (maxSignificantDigits > 0 && maxSignificantDigits <= RoundingUtils.MAX_INT_FRAC_SIG)
            {
                return ConstructSignificant(0, maxSignificantDigits);
            }
            else
            {
                throw new ArgumentException(
                        "Significant digits must be between 0 and " + RoundingUtils.MAX_INT_FRAC_SIG); // ICU4N TODO: Guard clause exception types
            }
        }

        /**
         * Show numbers rounded if necessary to a certain number of significant digits/figures; in addition, always show at
         * least a certain number of significant digits, padding with zeros if necessary.
         *
         * @param minSignificantDigits
         *            The minimum number of significant digits to display (padding with zeros if necessary).
         * @param maxSignificantDigits
         *            The maximum number of significant digits to display (rounding if necessary).
         * @return A Rounder for chaining or passing to the NumberFormatter rounding() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public static Rounder MinMaxDigits(int minSignificantDigits, int maxSignificantDigits)
        {
            if (minSignificantDigits > 0 && maxSignificantDigits <= RoundingUtils.MAX_INT_FRAC_SIG
                    && minSignificantDigits <= maxSignificantDigits)
            {
                return ConstructSignificant(minSignificantDigits, maxSignificantDigits);
            }
            else
            {
                throw new ArgumentException(
                        "Significant digits must be between 0 and " + RoundingUtils.MAX_INT_FRAC_SIG); // ICU4N TODO: Guard clause exception types
            }
        }

        /**
         * Show numbers rounded if necessary to the closest multiple of a certain rounding increment. For example, if the
         * rounding increment is 0.5, then round 1.2 to 1 and round 1.3 to 1.5.
         *
         * <para/>
         * In order to ensure that numbers are padded to the appropriate number of fraction places, set the scale on the
         * rounding increment BigDecimal. For example, to round to the nearest 0.5 and always display 2 numerals after the
         * decimal separator (to display 1.2 as "1.00" and 1.3 as "1.50"), you can run:
         *
         * <code>
         * Rounder.increment(new BigDecimal("0.50"))
         * </code>
         *
         * <para/>
         * For more information on the scale of Java BigDecimal, see {@link java.math.BigDecimal#scale()}.
         *
         * @param roundingIncrement
         *            The increment to which to round numbers.
         * @return A Rounder for chaining or passing to the NumberFormatter rounding() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public static Rounder Increment(BigMath.BigDecimal roundingIncrement)
        {
            if (roundingIncrement != null && roundingIncrement.CompareTo(BigMath.BigDecimal.Zero) > 0)
            {
                return ConstructIncrement(roundingIncrement);
            }
            else
            {
                throw new ArgumentException("Rounding increment must be positive and non-null"); // ICU4N TODO: Guard clause exception types
            }
        }

        /**
         * Show numbers rounded and padded according to the rules for the currency unit. The most common rounding settings
         * for currencies include <c>Rounder.fixedFraction(2)</c>, <c>Rounder.integer()</c>, and
         * <c>Rounder.increment(0.05)</c> for cash transactions ("nickel rounding").
         *
         * <para/>
         * The exact rounding details will be resolved at runtime based on the currency unit specified in the
         * NumberFormatter chain. To round according to the rules for one currency while displaying the symbol for another
         * currency, the withCurrency() method can be called on the return value of this method.
         *
         * @param currencyUsage
         *            Either STANDARD (for digital transactions) or CASH (for transactions where the rounding increment may
         *            be limited by the available denominations of cash or coins).
         * @return A CurrencyRounder for chaining or passing to the NumberFormatter rounding() setter.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public static CurrencyRounder Currency(CurrencyUsage currencyUsage)
        {
            if (currencyUsage != null)
            {
                return ConstructCurrency(currencyUsage);
            }
            else
            {
                throw new ArgumentNullException(nameof(currencyUsage), "CurrencyUsage must be non-null"); // ICU4N TODO: Guard clause exception types
            }
        }

        /**
         * Sets the {@link java.math.RoundingMode} to use when picking the direction to round (up or down). Common values
         * include HALF_EVEN, HALF_UP, and FLOOR. The default is HALF_EVEN.
         *
         * @param roundingMode
         *            The RoundingMode to use.
         * @return A Rounder for chaining.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public Rounder WithMode(BigMath.RoundingMode roundingMode)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return WithMode(RoundingUtils.MathContextUnlimited(roundingMode));
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /**
         * Sets a MathContext directly instead of RoundingMode.
         *
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public Rounder WithMode(BigMath.MathContext mathContext)
        {
            if (this.mathContext.Equals(mathContext))
            {
                return this;
            }
            Rounder other = (Rounder)this.Clone();
            other.mathContext = mathContext;
            return other;
        }

        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        /**
         * @internal
         * @deprecated ICU 60 This API is ICU internal only.
         */
        [Obsolete("ICU 60 This API is ICU internal only.")]
        public abstract void Apply(IDecimalQuantity value);

        //////////////////////////
        // PACKAGE-PRIVATE APIS //
        //////////////////////////

        internal static readonly InfiniteRounderImpl NONE = new InfiniteRounderImpl();

        internal static readonly FractionRounderImpl FIXED_FRAC_0 = new FractionRounderImpl(0, 0);
        internal static readonly FractionRounderImpl FIXED_FRAC_2 = new FractionRounderImpl(2, 2);
        internal static readonly FractionRounderImpl MAX_FRAC_6 = new FractionRounderImpl(0, 6);

        internal static readonly SignificantRounderImpl FIXED_SIG_2 = new SignificantRounderImpl(2, 2);
        internal static readonly SignificantRounderImpl FIXED_SIG_3 = new SignificantRounderImpl(3, 3);
        internal static readonly SignificantRounderImpl RANGE_SIG_2_3 = new SignificantRounderImpl(2, 3);

        internal static readonly FracSigRounderImpl COMPACT_STRATEGY = new FracSigRounderImpl(0, 0, 2, -1);

        // ICU4N: Using GetInstance() is intentional here because we want the value to be truncated
        // to match the input double value.
        internal static readonly IncrementRounderImpl NICKEL = new IncrementRounderImpl(BigMath.BigDecimal.GetInstance(0.05));

        internal static readonly CurrencyRounderImpl MONETARY_STANDARD = new CurrencyRounderImpl(CurrencyUsage.Standard);
        internal static readonly CurrencyRounderImpl MONETARY_CASH = new CurrencyRounderImpl(CurrencyUsage.Cash);

        internal static readonly PassThroughRounderImpl PASS_THROUGH = new PassThroughRounderImpl();

        internal static Rounder ConstructInfinite()
        {
            return NONE;
        }

        internal static FractionRounder ConstructFraction(int minFrac, int maxFrac)
        {
            if (minFrac == 0 && maxFrac == 0)
            {
                return FIXED_FRAC_0;
            }
            else if (minFrac == 2 && maxFrac == 2)
            {
                return FIXED_FRAC_2;
            }
            else if (minFrac == 0 && maxFrac == 6)
            {
                return MAX_FRAC_6;
            }
            else
            {
                return new FractionRounderImpl(minFrac, maxFrac);
            }
        }

        /** Assumes that minSig <= maxSig. */
        internal static Rounder ConstructSignificant(int minSig, int maxSig)
        {
            if (minSig == 2 && maxSig == 2)
            {
                return FIXED_SIG_2;
            }
            else if (minSig == 3 && maxSig == 3)
            {
                return FIXED_SIG_3;
            }
            else if (minSig == 2 && maxSig == 3)
            {
                return RANGE_SIG_2_3;
            }
            else
            {
                return new SignificantRounderImpl(minSig, maxSig);
            }
        }

        internal static Rounder ConstructFractionSignificant(FractionRounder base_, int minSig, int maxSig)
        {

            Debug.Assert(base_ is FractionRounderImpl);
            FractionRounderImpl @base = (FractionRounderImpl)base_;
            if (@base.minFrac == 0 && @base.maxFrac == 0 && minSig == 2 /* && maxSig == -1 */)
            {
                return COMPACT_STRATEGY;
            }
            else
            {
                return new FracSigRounderImpl(@base.minFrac, @base.maxFrac, minSig, maxSig);
            }
        }

        internal static Rounder ConstructIncrement(BigMath.BigDecimal increment)
        {
            // NOTE: .equals() is what we want, not .compareTo()
            if (increment.Equals(NICKEL.increment))
            {
                return NICKEL;
            }
            else
            {
                return new IncrementRounderImpl(increment);
            }
        }

        internal static CurrencyRounder ConstructCurrency(CurrencyUsage? usage)
        {
            if (usage == CurrencyUsage.Standard)
            {
                return MONETARY_STANDARD;
            }
            else if (usage == CurrencyUsage.Cash)
            {
                return MONETARY_CASH;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(usage)); //throw new AssertionError();
            }
        }

        internal static Rounder ConstructFromCurrency(CurrencyRounder base_, Currency currency)
        {
            Debug.Assert(base_ is CurrencyRounderImpl);
            CurrencyRounderImpl @base = (CurrencyRounderImpl)base_;
            double incrementDouble = currency.GetRoundingIncrement(@base.usage);
            if (incrementDouble != 0.0)
            {
                // ICU4N: Using GetInstance() is intentional here because we want the value to be truncated
                // to match the input double value.
                BigMath.BigDecimal increment = BigMath.BigDecimal.GetInstance(incrementDouble);
                return ConstructIncrement(increment);
            }
            else
            {
                int minMaxFrac = currency.GetDefaultFractionDigits(@base.usage);
                return ConstructFraction(minMaxFrac, minMaxFrac);
            }
        }

        internal static Rounder ConstructPassThrough()
        {
            return PASS_THROUGH;
        }

        /**
         * Returns a valid working Rounder. If the Rounder is a CurrencyRounder, applies the given currency. Otherwise,
         * simply passes through the argument.
         *
         * @param currency
         *            A currency object to use in case the input object needs it.
         * @return A Rounder object ready for use.
         */
        internal Rounder WithLocaleData(Currency currency)
        {
            if (this is CurrencyRounder currencyRounder)
            {
                return currencyRounder.WithCurrency(currency);
            }
            else
            {
                return this;
            }
        }

        internal int ChooseMultiplierAndApply(IDecimalQuantity input, IMultiplierProducer producer)
        {
            // TODO: Make a better and more efficient implementation.
            // TODO: Avoid the object creation here.
            IDecimalQuantity copy = input.CreateCopy();

            Debug.Assert(!input.IsZero);
            int magnitude = input.GetMagnitude();
            int multiplier = producer.GetMultiplier(magnitude);
            input.AdjustMagnitude(multiplier);
#pragma warning disable CS0618 // Type or member is obsolete
            Apply(input);
#pragma warning restore CS0618 // Type or member is obsolete

            // If the number turned to zero when rounding, do not re-attempt the rounding.
            if (!input.IsZero && input.GetMagnitude() == magnitude + multiplier + 1)
            {
                magnitude += 1;
                input.CopyFrom(copy);
                multiplier = producer.GetMultiplier(magnitude);
                input.AdjustMagnitude(multiplier);
                Debug.Assert(input.GetMagnitude() == magnitude + multiplier - 1);
#pragma warning disable CS0618 // Type or member is obsolete
                Apply(input);
#pragma warning restore CS0618 // Type or member is obsolete
                Debug.Assert(input.GetMagnitude() == magnitude + multiplier);
            }

            return multiplier;
        }

        ///////////////
        // INTERNALS //
        ///////////////

        internal class InfiniteRounderImpl : Rounder
        {

            public InfiniteRounderImpl()
            {
            }

            public override void Apply(IDecimalQuantity value)
            {
                value.RoundToInfinity();
                value.SetFractionLength(0, int.MaxValue);
            }
        }

        internal class FractionRounderImpl : FractionRounder
        {
            internal readonly int minFrac;
            internal readonly int maxFrac;

            public FractionRounderImpl(int minFrac, int maxFrac)
            {
                this.minFrac = minFrac;
                this.maxFrac = maxFrac;
            }

            public override void Apply(IDecimalQuantity value)
            {
                value.RoundToMagnitude(GetRoundingMagnitudeFraction(maxFrac), mathContext);
                value.SetFractionLength(Math.Max(0, -GetDisplayMagnitudeFraction(minFrac)), int.MaxValue);
            }
        }

        internal class SignificantRounderImpl : Rounder
        {
            internal readonly int minSig;
            internal readonly int maxSig;

            public SignificantRounderImpl(int minSig, int maxSig)
            {
                this.minSig = minSig;
                this.maxSig = maxSig;
            }

#pragma warning disable CS0672 // Member overrides obsolete member
            public override void Apply(IDecimalQuantity value)
#pragma warning restore CS0672 // Member overrides obsolete member
            {
                value.RoundToMagnitude(GetRoundingMagnitudeSignificant(value, maxSig), mathContext);
                value.SetFractionLength(Math.Max(0, -GetDisplayMagnitudeSignificant(value, minSig)), int.MaxValue);
            }

            /** Version of {@link #apply} that obeys minInt constraints. Used for scientific notation compatibility mode. */
            public void Apply(IDecimalQuantity quantity, int minInt)
            {
                Debug.Assert(quantity.IsZero);
                quantity.SetFractionLength(minSig - minInt, int.MaxValue);
            }
        }

        internal class FracSigRounderImpl : Rounder
        {
            internal readonly int minFrac;
            internal readonly int maxFrac;
            internal readonly int minSig;
            internal readonly int maxSig;

            public FracSigRounderImpl(int minFrac, int maxFrac, int minSig, int maxSig)
            {
                this.minFrac = minFrac;
                this.maxFrac = maxFrac;
                this.minSig = minSig;
                this.maxSig = maxSig;
            }

            public override void Apply(IDecimalQuantity value)
            {
                int displayMag = GetDisplayMagnitudeFraction(minFrac);
                int roundingMag = GetRoundingMagnitudeFraction(maxFrac);
                if (minSig == -1)
                {
                    // Max Sig override
                    int candidate = GetRoundingMagnitudeSignificant(value, maxSig);
                    roundingMag = Math.Max(roundingMag, candidate);
                }
                else
                {
                    // Min Sig override
                    int candidate = GetDisplayMagnitudeSignificant(value, minSig);
                    roundingMag = Math.Min(roundingMag, candidate);
                }
                value.RoundToMagnitude(roundingMag, mathContext);
                value.SetFractionLength(Math.Max(0, -displayMag), int.MaxValue);
            }
        }

        internal class IncrementRounderImpl : Rounder
        {
            internal readonly BigMath.BigDecimal increment;

            public IncrementRounderImpl(BigMath.BigDecimal increment)
            {
                this.increment = increment;
            }

            public override void Apply(IDecimalQuantity value)
            {
                value.RoundToIncrement(increment, mathContext);
                value.SetFractionLength(increment.Scale, increment.Scale);
            }
        }

        internal class CurrencyRounderImpl : CurrencyRounder
        {
            internal readonly CurrencyUsage usage;

            public CurrencyRounderImpl(CurrencyUsage usage)
            {
                this.usage = usage;
            }

            public override void Apply(IDecimalQuantity value)
            {
                // Call .withCurrency() before .apply()!
                throw new InvalidOperationException(); //throw new AssertionError();
            }
        }

        internal class PassThroughRounderImpl : Rounder
        {

            public PassThroughRounderImpl()
            {
            }

            public override void Apply(IDecimalQuantity value)
            {
                // TODO: Assert that value has already been rounded
            }
        }

        private static int GetRoundingMagnitudeFraction(int maxFrac)
        {
            if (maxFrac == -1)
            {
                return int.MinValue;
            }
            return -maxFrac;
        }

        private static int GetRoundingMagnitudeSignificant(IDecimalQuantity value, int maxSig)
        {
            if (maxSig == -1)
            {
                return int.MinValue;
            }
            int magnitude = value.IsZero ? 0 : value.GetMagnitude();
            return magnitude - maxSig + 1;
        }

        private static int GetDisplayMagnitudeFraction(int minFrac)
        {
            if (minFrac == 0)
            {
                return int.MaxValue;
            }
            return -minFrac;
        }

        private static int GetDisplayMagnitudeSignificant(IDecimalQuantity value, int minSig)
        {
            int magnitude = value.IsZero ? 0 : value.GetMagnitude();
            return magnitude - minSig + 1;
        }
    }
}
