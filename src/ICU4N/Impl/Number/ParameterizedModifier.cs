using ICU4N.Impl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ICU4N.Numerics
{
    /// <summary>
    /// A ParameterizedModifier by itself is NOT a Modifier. Rather, it wraps a data structure containing two or more
    /// Modifiers and returns the modifier appropriate for the current situation.
    /// </summary>
    internal class ParameterizedModifier // ICU4N TODO: API - this was public in ICU4J
    {
        private readonly IModifier positive;
        private readonly IModifier negative;
        readonly IModifier[] mods;
        bool frozen;

        /**
         * This constructor populates the ParameterizedModifier with a single positive and negative form.
         *
         * <p>
         * If this constructor is used, a plural form CANNOT be passed to {@link #getModifier}.
         */
        public ParameterizedModifier(IModifier positive, IModifier negative)
        {
            this.positive = positive;
            this.negative = negative;
            this.mods = null;
            this.frozen = true;
        }

        /**
         * This constructor prepares the ParameterizedModifier to be populated with a positive and negative Modifier for
         * multiple plural forms.
         *
         * <p>
         * If this constructor is used, a plural form MUST be passed to {@link #getModifier}.
         */
        public ParameterizedModifier()
        {
            this.positive = null;
            this.negative = null;
            this.mods = new IModifier[2 * StandardPluralUtil.Count];
            this.frozen = false;
        }

        public virtual void SetModifier(bool isNegative, StandardPlural plural, IModifier mod)
        {
            Debug.Assert(!frozen);
            mods[GetModIndex(isNegative, plural)] = mod;
        }

        public virtual void Freeze()
        {
            frozen = true;
        }

        public virtual IModifier GetModifier(bool isNegative)
        {
            Debug.Assert(frozen);
            Debug.Assert(mods is null);
            return isNegative ? negative : positive;
        }

        public virtual IModifier GetModifier(bool isNegative, StandardPlural plural)
        {
            Debug.Assert(frozen);
            Debug.Assert(positive == null);
            return mods[GetModIndex(isNegative, plural)];
        }

        private static int GetModIndex(bool isNegative, StandardPlural plural)
        {
            return (int)plural * 2 + (isNegative ? 1 : 0);
        }
    }
}
