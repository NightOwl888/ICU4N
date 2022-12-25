using J2N;
using Field = ICU4N.Text.NumberFormatField;

namespace ICU4N.Numerics
{
    /// <summary>
    /// An implementation of <see cref="IModifier"/> that allows for multiple types of fields in the same modifier. Constructed
    /// based on the contents of two <see cref="NumberStringBuilder"/> instances (one for the prefix, one for the suffix).
    /// </summary>
    internal class ConstantMultiFieldModifier : IModifier
    {
        // NOTE: In Java, these are stored as array pointers. In C++, the NumberStringBuilder is stored by
        // value and is treated internally as immutable.
        protected readonly char[] prefixChars;
        protected readonly char[] suffixChars;
        protected readonly Field[] prefixFields;
        protected readonly Field[] suffixFields;
        private readonly bool strong;

        public ConstantMultiFieldModifier(NumberStringBuilder prefix, NumberStringBuilder suffix, bool strong)
        {
            prefixChars = prefix.ToCharArray();
            suffixChars = suffix.ToCharArray();
            prefixFields = prefix.ToFieldArray();
            suffixFields = suffix.ToFieldArray();
            this.strong = strong;
        }

        public virtual int Apply(NumberStringBuilder output, int leftIndex, int rightIndex)
        {
            // Insert the suffix first since inserting the prefix will change the rightIndex
            int length = output.Insert(rightIndex, suffixChars, suffixFields);
            length += output.Insert(leftIndex, prefixChars, prefixFields);
            return length;
        }

        public virtual int PrefixLength => prefixChars.Length;

        public virtual int CodePointCount
        {
            get => Character.CodePointCount(prefixChars, 0, prefixChars.Length)
                    + Character.CodePointCount(suffixChars, 0, suffixChars.Length);
        }
        public virtual bool IsStrong => strong;

        public override string ToString()
        {
            NumberStringBuilder temp = new NumberStringBuilder();
            Apply(temp, 0, 0);
            int prefixLength = PrefixLength;
            return $"<ConstantMultiFieldModifier prefix:'{temp.ToString(0, prefixLength)}' suffix:'{temp.ToString(prefixLength, temp.Length - prefixLength)}'>";
        }
    }
}
