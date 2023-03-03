using ICU4N.Numerics;
using ICU4N.Text;
using System;
using System.Collections.Generic;
using System.Text;
using static ICU4N.Numerics.NumberFormatter;

namespace ICU4N.Numerics
{
    internal class MicroProps : IMicroPropsGenerator // ICU4N TODO: API - this was public in ICU4J
#if FEATURE_CLONEABLE
        , ICloneable
#endif
    {
        // Populated globally:
        public SignDisplay? sign; // ICU4N TODO: API - fix public fields (convert to properties?)
        public DecimalFormatSymbols symbols;
        public Padder padding;
        public DecimalSeparatorDisplay? @decimal;
        public IntegerWidth integerWidth;

        // Populated by notation/unit:
        public IModifier modOuter;
        public IModifier modMiddle;
        public IModifier modInner;
        public Rounder rounding;
#pragma warning disable CS0618 // Type or member is obsolete
        public Grouper grouping;
#pragma warning restore CS0618 // Type or member is obsolete
        public bool useCurrency;

        // Internal fields:
        private readonly bool immutable;
        private volatile bool exhausted;

        /**
         * @param immutable
         *            Whether this MicroProps should behave as an immutable after construction with respect to the quantity
         *            chain.
         */
        public MicroProps(bool immutable)
        {
            this.immutable = immutable;
        }

        public virtual MicroProps ProcessQuantity(IDecimalQuantity quantity)
        {
            if (immutable)
            {
                return (MicroProps)this.Clone();
            }
            else if (exhausted)
            {
                // Safety check
                //throw new AssertionError("Cannot re-use a mutable MicroProps in the quantity chain");
                throw new InvalidOperationException("Cannot re-use a mutable MicroProps in the quantity chain");
            }
            else
            {
                exhausted = true;
                return this;
            }
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}
