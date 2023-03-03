using ICU4N.Impl;
using ICU4N.Support.Text;
using ICU4N.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Numerics //ICU4N.Impl.Number
{
    /// <summary>
    /// An interface representing a number to be processed by the decimal formatting pipeline. Includes
    /// methods for rounding, plural rules, and decimal digit extraction.
    /// <para/>
    /// By design, this is NOT IMMUTABLE and NOT THREAD SAFE. It is intended to be an intermediate
    /// object holding state during a pass through the decimal formatting pipeline.
    /// <para/>
    /// Implementations of this interface are free to use any internal storage mechanism.
    /// <para/>
    /// TODO: Should I change this to an abstract class so that logic for min/max digits doesn't need
    /// to be copied to every implementation?
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
    internal interface IDecimalQuantity : PluralRules.IFixedDecimal // ICU4N TODO: API - this was public in ICU4J
#pragma warning restore CS0618 // Type or member is obsolete
    {
        /// <summary>
        /// Sets the minimum and maximum integer digits that this <see cref="IDecimalQuantity"/> should generate.
        /// This method does not perform rounding.
        /// </summary>
        /// <param name="minInt">The minimum number of integer digits.</param>
        /// <param name="maxInt">The maximum number of integer digits.</param>
        void SetIntegerLength(int minInt, int maxInt);

        /// <summary>
        /// Sets the minimum and maximum fraction digits that this <see cref="IDecimalQuantity"/> should generate.
        /// </summary>
        /// <param name="minFrac">The minimum number of fraction digits.</param>
        /// <param name="maxFrac">The maximum number of fraction digits.</param>
        void SetFractionLength(int minFrac, int maxFrac);

        /// <summary>
        /// Rounds the number to a specified interval, such as 0.05.
        /// <para/>
        /// If rounding to a power of ten, use the more efficient <see cref="RoundToMagnitude(int, BigMath.MathContext)"/> instead.
        /// </summary>
        /// <param name="roundingInterval">The increment to which to round.</param>
        /// <param name="mathContext">The <see cref="BigMath.MathContext"/> to use if rounding is necessary. Undefined behavior
        /// if <c>null</c>.</param>
        void RoundToIncrement(BigMath.BigDecimal roundingInterval, BigMath.MathContext mathContext);

        /// <summary>
        /// Rounds the number to a specified magnitude (power of ten).
        /// </summary>
        /// <param name="roundingMagnitude">The power of ten to which to round. For example, a value of -2 will
        /// round to 2 decimal places.</param>
        /// <param name="mathContext">The <see cref="BigMath.MathContext"/> to use if rounding is necessary. Undefined behavior
        /// if <c>null</c>.</param>
        void RoundToMagnitude(int roundingMagnitude, BigMath.MathContext mathContext);

        /// <summary>
        /// Rounds the number to an infinite number of decimal points. This has no effect except for
        /// forcing the double in <see cref="DecimalQuantity_AbstractBCD"/> to adopt its exact representation.
        /// </summary>
        void RoundToInfinity();

        /// <summary>
        /// Multiply the internal value.
        /// </summary>
        /// <param name="multiplicand">The value by which to multiply.</param>
        void MultiplyBy(BigMath.BigDecimal multiplicand);

        /// <summary>
        /// Scales the number by a power of ten. For example, if the value is currently "1234.56", calling
        /// this method with delta=-3 will change the value to "1.23456".
        /// </summary>
        /// <param name="delta">The number of magnitudes of ten to change by.</param>
        void AdjustMagnitude(int delta);

        /// <summary>
        /// Returns the power of ten corresponding to the most significant nonzero digit.
        /// </summary>
        /// <returns>The power of ten corresponding to the most significant nonzero digit.</returns>
        /// <exception cref="ArithmeticException">If the value represented is zero.</exception>
        int GetMagnitude();

        /// <summary>
        /// Returns <c>true</c> if the value represented by this <see cref="IDecimalQuantity"/> is zero.
        /// </summary>
        bool IsZero { get; }

        /// <summary>
        /// Returns <c>true</c> if the value represented by this <see cref="IDecimalQuantity"/> is less than zero.
        /// </summary>
        bool IsNegative { get; }

        /// <summary>
        /// Returns <c>true</c> if the value represented by this <see cref="IDecimalQuantity"/> is infinite.
        /// </summary>
        new bool IsInfinity { get; }

        /// <summary>
        /// Returns <c>true</c> if the value represented by this <see cref="IDecimalQuantity"/> is not a number.
        /// </summary>
        new bool IsNaN { get; }

        /// <summary>
        /// Returns the value contained in this <see cref="IDecimalQuantity"/> approximated as a double.
        /// </summary>
        /// <returns>The value contained in this <see cref="IDecimalQuantity"/> approximated as a double.</returns>
        double ToDouble();

        BigMath.BigDecimal ToBigDecimal();

        void SetToBigDecimal(BigMath.BigDecimal input);

        int MaxRepresentableDigits { get; }

        // TODO: Should this method be removed, since DecimalQuantity implements IFixedDecimal now?
        /// <summary>
        /// Computes the plural form for this number based on the specified set of rules.
        /// </summary>
        /// <param name="rules">A <see cref="PluralRules"/> object representing the set of rules.</param>
        /// <returns>The <see cref="StandardPlural"/> according to the <paramref name="rules"/>. If the plural form is not in
        /// the set of standard plurals, <see cref="StandardPlural.Other"/> is returned instead.</returns>
        StandardPlural GetStandardPlural(PluralRules rules);

        /// <summary>
        /// Gets the digit at the specified magnitude. For example, if the represented number is 12.3,
        /// <c>GetDigit(-1)</c> returns 3, since 3 is the digit corresponding to 10^-1.
        /// </summary>
        /// <param name="magnitude">The magnitude of the digit.</param>
        /// <returns>The digit at the specified magnitude.</returns>
        byte GetDigit(int magnitude);

        /// <summary>
        /// Gets the largest power of ten that needs to be displayed. The value returned by this function
        /// will be bounded between minInt and maxInt.
        /// </summary>
        int UpperDisplayMagnitude { get; }

        /// <summary>
        /// Gets the smallest power of ten that needs to be displayed. The value returned by this function
        /// will be bounded between -minFrac and -maxFrac.
        /// </summary>
        int LowerDisplayMagnitude { get; }

        /// <summary>
        /// Returns the string in "plain" format (no exponential notation) using ASCII digits.
        /// </summary>
        /// <returns>The string in "plain" format (no exponential notation) using ASCII digits.</returns>
        string ToPlainString();

        /// <summary>
        /// Like clone, but without the restrictions of the <see cref="ICloneable"/> interface clone.
        /// </summary>
        /// <returns>A copy of this instance which can be mutated without affecting this instance.</returns>
        IDecimalQuantity CreateCopy();

        /// <summary>
        /// Sets this instance to be equal to another instance.
        /// </summary>
        /// <param name="other">The instance to copy from.</param>
        void CopyFrom(IDecimalQuantity other);

        /// <summary>
        /// This property is for internal testing only.
        /// </summary>
        long PositionFingerprint { get; }

        /// <summary>
        /// If the given <see cref="FieldPosition"/> is a <see cref="UFieldPosition"/>, populates it with the fraction
        /// length and fraction long value. If the argument is not a <see cref="UFieldPosition"/>, nothing
        /// happens.
        /// </summary>
        /// <param name="fp">The <see cref="UFieldPosition"/> to populate.</param>
        void PopulateUFieldPosition(FieldPosition fp);
    }
}
