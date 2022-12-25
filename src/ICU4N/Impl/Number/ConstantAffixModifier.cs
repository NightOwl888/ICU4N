using J2N;
using J2N.Text;
using Field = ICU4N.Text.NumberFormatField;

namespace ICU4N.Numerics
{
    /// <summary>
    /// The canonical implementation of <see cref="IModifier"/>, containing a prefix and suffix string.
    /// </summary>
    internal class ConstantAffixModifier : IModifier
    {
        // TODO: Avoid making a new instance by default if prefix and suffix are empty
        public static readonly ConstantAffixModifier Empty = new ConstantAffixModifier();

        private readonly string prefix;
        private readonly string suffix;
        private readonly Field field;
        private readonly bool strong;

        /// <summary>
        /// Constructs an instance with the given strings.
        /// <para/>
        /// The arguments need to be <see cref="string"/>s, not <see cref="ICharSequence"/>s, because
        /// strings are immutable but <see cref="ICharSequence"/>s are not.
        /// </summary>
        /// <param name="prefix">The prefix string.</param>
        /// <param name="suffix">The suffix string.</param>
        /// <param name="field">The field type to be associated with this modifier. Can be <c>null</c>.</param>
        /// <param name="strong">Whether this modifier should be strongly applied.</param>
        /// <seealso cref="Field"/>
        public ConstantAffixModifier(string prefix, string suffix, Field field, bool strong)
        {
            // Use an empty string instead of null if we are given null
            // TODO: Consider returning a null modifier if both prefix and suffix are empty.
            this.prefix = (prefix == null ? string.Empty : prefix);
            this.suffix = (suffix == null ? string.Empty : suffix);
            this.field = field;
            this.strong = strong;
        }

        /// <summary>
        /// Constructs a new instance with an empty prefix, suffix, and field.
        /// </summary>
        public ConstantAffixModifier()
        {
            prefix = string.Empty;
            suffix = string.Empty;
            field = null;
            strong = false;
        }

        public virtual int Apply(NumberStringBuilder output, int leftIndex, int rightIndex)
        {
            // Insert the suffix first since inserting the prefix will change the rightIndex
            int length = output.Insert(rightIndex, suffix, field);
            length += output.Insert(leftIndex, prefix, field);
            return length;
        }

        public virtual int PrefixLength => prefix.Length;

        public virtual int CodePointCount => prefix.CodePointCount(0, prefix.Length) + suffix.CodePointCount(0, suffix.Length);

        public virtual bool IsStrong => strong;

        public override string ToString()
        {
            return $"<ConstantAffixModifier prefix:'{prefix}' suffix:'{suffix}'>";
        }
    }
}
