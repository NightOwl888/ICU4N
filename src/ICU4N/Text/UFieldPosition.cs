using ICU4N.Support.Text;
using System;

namespace ICU4N.Text
{
    /// <summary>
    /// Adds the ability to get the decimal digits.
    /// </summary>
    /// <internal/>
    [Obsolete("This API is ICU internal only.")]
    internal class UFieldPosition : FieldPosition // ICU4N specific - marked internal, since it is obsolete anyway
    {
        private int countVisibleFractionDigits = -1;
        private long fractionDigits = 0;

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public UFieldPosition()
            : base(-1)
        {
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public UFieldPosition(int field)
            : base(field)
        {
        }

        // ICU4N TODO: Missing dependency java.text.Format.Field
        ///**
        // * @internal
        // * @deprecated This API is ICU internal only.
        // */
        //[Obsolete("This API is ICU internal only.")]
        //public UFieldPosition(Field attribute, int fieldID)
        //{
        //    super(attribute, fieldID);
        //}

        ///**
        // * @internal
        // * @deprecated This API is ICU internal only.
        // */
        //[Obsolete("This API is ICU internal only.")]
        //public UFieldPosition(Field attribute)
        //{
        //    super(attribute);
        //}

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public void SetFractionDigits(int countVisibleFractionDigits, long fractionDigits)
        {
            this.countVisibleFractionDigits = countVisibleFractionDigits;
            this.fractionDigits = fractionDigits;
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public int CountVisibleFractionDigits => countVisibleFractionDigits;

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public long FractionDigits => fractionDigits;
    }
}
