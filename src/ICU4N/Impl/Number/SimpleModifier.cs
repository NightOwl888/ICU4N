using ICU4N.Impl;
using J2N;
using System;
using System.Diagnostics;
using Field = ICU4N.Text.NumberFormatField;
using SimpleFormatter = ICU4N.Text.SimpleFormatter;

namespace ICU4N.Numerics
{
    /// <summary>
    /// The second primary implementation of <see cref="IModifier"/>, this one consuming a <see cref="SimpleFormatter"/>
    /// pattern.
    /// </summary>
    internal class SimpleModifier : IModifier
    {
        private readonly string compiledPattern;
        private readonly Field field;
        private readonly bool strong;
        private readonly int prefixLength;
        private readonly int suffixOffset;
        private readonly int suffixLength;

        /** TODO: This is copied from SimpleFormatterImpl. */
        private static readonly int ARG_NUM_LIMIT = 0x100;

        /** Creates a modifier that uses the SimpleFormatter string formats. */
        public SimpleModifier(string compiledPattern, Field field, bool strong)
        {
            //Debug.Assert( compiledPattern != null);
            this.compiledPattern = compiledPattern ?? throw new ArgumentNullException(nameof(compiledPattern));
            this.field = field;
            this.strong = strong;
            Debug.Assert(SimpleFormatterImpl.GetArgumentLimit(compiledPattern.AsSpan()) == 1);
            if (compiledPattern[1] != '\u0000')
            {
                prefixLength = compiledPattern[1] - ARG_NUM_LIMIT;
                suffixOffset = 3 + prefixLength;
            }
            else
            {
                prefixLength = 0;
                suffixOffset = 2;
            }
            if (3 + prefixLength < compiledPattern.Length)
            {
                suffixLength = compiledPattern[suffixOffset] - ARG_NUM_LIMIT;
            }
            else
            {
                suffixLength = 0;
            }
        }

        public virtual int Apply(NumberStringBuilder output, int leftIndex, int rightIndex)
        {
            return FormatAsPrefixSuffix(output, leftIndex, rightIndex, field);
        }

        public virtual int PrefixLength => prefixLength;

        public virtual int CodePointCount
        {
            get
            {
                int count = 0;
                if (prefixLength > 0)
                {
                    count += Character.CodePointCount(compiledPattern, 2, 2 + prefixLength);
                }
                if (suffixLength > 0)
                {
                    count += Character.CodePointCount(compiledPattern, 1 + suffixOffset, 1 + suffixOffset + suffixLength);
                }
                return count;
            }
        }
        public bool IsStrong => strong;

        /**
         * TODO: This belongs in SimpleFormatterImpl. The only reason I haven't moved it there yet is because
         * DoubleSidedStringBuilder is an internal class and SimpleFormatterImpl feels like it should not depend on it.
         *
         * <para/>
         * Formats a value that is already stored inside the StringBuilder <code>result</code> between the indices
         * <code>startIndex</code> and <code>endIndex</code> by inserting characters before the start index and after the
         * end index.
         *
         * <para/>
         * This is well-defined only for patterns with exactly one argument.
         *
         * @param result
         *            The StringBuilder containing the value argument.
         * @param startIndex
         *            The left index of the value within the string builder.
         * @param endIndex
         *            The right index of the value within the string builder.
         * @return The number of characters (UTF-16 code points) that were added to the StringBuilder.
         */
        public int FormatAsPrefixSuffix(NumberStringBuilder result, int startIndex, int endIndex, Field field)
        {
            if (prefixLength > 0)
            {
                result.Insert(startIndex, compiledPattern, 2, 2 + prefixLength, field);
            }
            if (suffixLength > 0)
            {
                result.Insert(endIndex + prefixLength, compiledPattern, 1 + suffixOffset, 1 + suffixOffset + suffixLength,
                        field);
            }
            return prefixLength + suffixLength;
        }
    }
}
