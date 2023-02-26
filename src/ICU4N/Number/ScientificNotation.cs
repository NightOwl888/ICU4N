using ICU4N.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static ICU4N.Numerics.NumberFormatter;
using static ICU4N.Numerics.Rounder;

namespace ICU4N.Numerics
{
    /// <summary>
    /// A class that defines the scientific notation style to be used when formatting numbers in <see cref="NumberFormatter"/>.
    /// </summary>
    /// <draft>ICU 60</draft>
    /// <provisional>This API might change or be removed in a future release.</provisional>
    /// <seealso cref="NumberFormatter"/>
    internal class ScientificNotation : Notation // ICU4N TODO: API - this was public in ICU4J
    {
        internal int engineeringInterval;
        internal bool requireMinInt;
        internal int minExponentDigits;
        internal SignDisplay exponentSignDisplay;

        /* package-private */
        internal ScientificNotation(int engineeringInterval, bool requireMinInt, int minExponentDigits,
            SignDisplay exponentSignDisplay)
        {
            this.engineeringInterval = engineeringInterval;
            this.requireMinInt = requireMinInt;
            this.minExponentDigits = minExponentDigits;
            this.exponentSignDisplay = exponentSignDisplay;
        }

        /**
         * Sets the minimum number of digits to show in the exponent of scientific notation, padding with zeros if
         * necessary. Useful for fixed-width display.
         *
         * <para/>
         * For example, with minExponentDigits=2, the number 123 will be printed as "1.23E02" in <em>en-US</em> instead of
         * the default "1.23E2".
         *
         * @param minExponentDigits
         *            The minimum number of digits to show in the exponent.
         * @return A ScientificNotation, for chaining.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public virtual ScientificNotation WithMinExponentDigits(int minExponentDigits)
        {
            if (minExponentDigits >= 0 && minExponentDigits < RoundingUtils.MAX_INT_FRAC_SIG)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                ScientificNotation other = (ScientificNotation)this.Clone();
#pragma warning restore CS0618 // Type or member is obsolete
                other.minExponentDigits = minExponentDigits;
                return other;
            }
            else
            {
                throw new ArgumentException(
                        "Integer digits must be between 0 and " + RoundingUtils.MAX_INT_FRAC_SIG);
            }
        }

        /**
         * Sets whether to show the sign on positive and negative exponents in scientific notation. The default is AUTO,
         * showing the minus sign but not the plus sign.
         *
         * <para/>
         * For example, with exponentSignDisplay=ALWAYS, the number 123 will be printed as "1.23E+2" in <em>en-US</em>
         * instead of the default "1.23E2".
         *
         * @param exponentSignDisplay
         *            The strategy for displaying the sign in the exponent.
         * @return A ScientificNotation, for chaining.
         * @draft ICU 60
         * @provisional This API might change or be removed in a future release.
         * @see NumberFormatter
         */
        public virtual ScientificNotation WithExponentSignDisplay(SignDisplay exponentSignDisplay)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            ScientificNotation other = (ScientificNotation)this.Clone();
#pragma warning restore CS0618 // Type or member is obsolete
            other.exponentSignDisplay = exponentSignDisplay;
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

        /* package-private */
        internal IMicroPropsGenerator WithLocaleData(DecimalFormatSymbols symbols, bool build,
            IMicroPropsGenerator parent)
        {
            return new ScientificHandler(this, symbols, build, parent);
        }

        // NOTE: The object lifecycle of ScientificModifier and ScientificHandler differ greatly in Java and C++.
        //
        // During formatting, we need to provide an object with state (the exponent) as the inner modifier.
        //
        // In Java, where the priority is put on reducing object creations, the unsafe code path re-uses the
        // ScientificHandler as a ScientificModifier, and the safe code path pre-computes 25 ScientificModifier
        // instances. This scheme reduces the number of object creations by 1 in both safe and unsafe.
        //
        // In C++, MicroProps provides a pre-allocated ScientificModifier, and ScientificHandler simply populates
        // the state (the exponent) into that ScientificModifier. There is no difference between safe and unsafe.

        private class ScientificHandler : IMicroPropsGenerator, IMultiplierProducer, IModifier
        {

            internal readonly ScientificNotation notation;
            internal readonly DecimalFormatSymbols symbols;
            internal readonly ScientificModifier[] precomputedMods;
            internal readonly IMicroPropsGenerator parent;
            /* unsafe */
            int exponent;

            internal ScientificHandler(ScientificNotation notation, DecimalFormatSymbols symbols, bool safe,
                    IMicroPropsGenerator parent)
            {
                this.notation = notation;
                this.symbols = symbols;
                this.parent = parent;

                if (safe)
                {
                    // Pre-build the modifiers for exponents -12 through 12
                    precomputedMods = new ScientificModifier[25];
                    for (int i = -12; i <= 12; i++)
                    {
                        precomputedMods[i + 12] = new ScientificModifier(i, this);
                    }
                }
                else
                {
                    precomputedMods = null;
                }
            }

            public virtual MicroProps ProcessQuantity(IDecimalQuantity quantity)
            {
                MicroProps micros = parent.ProcessQuantity(quantity);
                Debug.Assert(micros.rounding != null);

                // Treat zero as if it had magnitude 0
                int exponent;
                if (quantity.IsZero)
                {
                    if (notation.requireMinInt && micros.rounding is SignificantRounderImpl significantRounder)
                    {
                        // Show "00.000E0" on pattern "00.000E0"
                        significantRounder.Apply(quantity, notation.engineeringInterval);
                        exponent = 0;
                    }
                    else
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        micros.rounding.Apply(quantity);
#pragma warning restore CS0618 // Type or member is obsolete
                        exponent = 0;
                    }
                }
                else
                {
                    exponent = -micros.rounding.ChooseMultiplierAndApply(quantity, this);
                }

                // Add the Modifier for the scientific format.
                if (precomputedMods != null && exponent >= -12 && exponent <= 12)
                {
                    // Safe code path A
                    micros.modInner = precomputedMods[exponent + 12];
                }
                else if (precomputedMods != null)
                {
                    // Safe code path B
                    micros.modInner = new ScientificModifier(exponent, this);
                }
                else
                {
                    // Unsafe code path: mutates the object and re-uses it as a Modifier!
                    this.exponent = exponent;
                    micros.modInner = this;
                }

                // We already performed rounding. Do not perform it again.
                micros.rounding = Rounder.ConstructPassThrough();

                return micros;
            }

            public virtual int GetMultiplier(int magnitude)
            {
                int interval = notation.engineeringInterval;
                int digitsShown;
                if (notation.requireMinInt)
                {
                    // For patterns like "000.00E0" and ".00E0"
                    digitsShown = interval;
                }
                else if (interval <= 1)
                {
                    // For patterns like "0.00E0" and "@@@E0"
                    digitsShown = 1;
                }
                else
                {
                    // For patterns like "##0.00"
                    digitsShown = ((magnitude % interval + interval) % interval) + 1;
                }
                return digitsShown - magnitude - 1;
            }


            public virtual int PrefixLength
                // TODO: Localized exponent separator location.
                => 0;

            public virtual int CodePointCount
                // This method is not used for strong modifiers.
                => throw new NotSupportedException(); // throw new AssertionError();

            public virtual bool IsStrong
                    // Scientific is always strong
                    => true;

            public virtual int Apply(NumberStringBuilder output, int leftIndex, int rightIndex)
            {
                return DoApply(exponent, output, rightIndex);
            }

            internal int DoApply(int exponent, NumberStringBuilder output, int rightIndex)
            {
                // FIXME: Localized exponent separator location.
                int i = rightIndex;
                // Append the exponent separator and sign
                i += output.Insert(i, symbols.ExponentSeparator, NumberFormatField.ExponentSymbol);
                if (exponent < 0 && notation.exponentSignDisplay != SignDisplay.Never)
                {
                    i += output.Insert(i, symbols.MinusSignString, NumberFormatField.ExponentSign);
                }
                else if (exponent >= 0 && notation.exponentSignDisplay == SignDisplay.Always)
                {
                    i += output.Insert(i, symbols.PlusSignString, NumberFormatField.ExponentSign);
                }
                // Append the exponent digits (using a simple inline algorithm)
                int disp = Math.Abs(exponent);
                for (int j = 0; j < notation.minExponentDigits || disp > 0; j++, disp /= 10)
                {
                    int d = disp % 10;
#pragma warning disable CS0618 // Type or member is obsolete
                    string digitString = symbols.DigitStringsLocal[d];
#pragma warning restore CS0618 // Type or member is obsolete
                    i += output.Insert(i - j, digitString, NumberFormatField.Exponent);
                }
                return i - rightIndex;
            }
        }

        private class ScientificModifier : IModifier
        {
            internal readonly int exponent;
            internal readonly ScientificHandler handler;

            internal ScientificModifier(int exponent, ScientificHandler handler)
            {
                this.exponent = exponent;
                this.handler = handler;
            }

            public virtual int Apply(NumberStringBuilder output, int leftIndex, int rightIndex)
            {
                return handler.DoApply(exponent, output, rightIndex);
            }

            public virtual int PrefixLength
                // TODO: Localized exponent separator location.
                => 0;


            public virtual int CodePointCount
                // This method is not used for strong modifiers.
                => throw new NotSupportedException(); //throw new AssertionError();

            public bool IsStrong
                // Scientific is always strong
                => true;
        }
    }
}
