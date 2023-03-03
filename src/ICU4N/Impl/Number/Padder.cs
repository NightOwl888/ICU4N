using ICU4N.Text;
using J2N;
using System;
using System.Diagnostics;

namespace ICU4N.Numerics
{
    internal static class PadPositionExtensions // ICU4N TODO: API - this was public in ICU4J
    {
        public static PadPosition ToOld(this Padder.PadPosition padPosition)
        {
            return padPosition switch
            {
                Padder.PadPosition.BeforePrefix => PadPosition.BeforePrefix,
                Padder.PadPosition.AfterPrefix => PadPosition.AfterPrefix,
                Padder.PadPosition.BeforeSuffix => PadPosition.BeforeSuffix,
                Padder.PadPosition.AfterSuffix => PadPosition.AfterSuffix,
                _ => (PadPosition)(-1),// silence compiler errors
            };
        }

        public static Padder.PadPosition ToNew(this PadPosition padPosition)
        {
            return padPosition switch
            {
                PadPosition.BeforePrefix => Padder.PadPosition.BeforePrefix,
                PadPosition.AfterPrefix => Padder.PadPosition.AfterPrefix,
                PadPosition.BeforeSuffix => Padder.PadPosition.BeforeSuffix,
                PadPosition.AfterSuffix => Padder.PadPosition.AfterSuffix,
                _ => throw new ArgumentException("Don't know how to map " + padPosition),
            };
        }
    }

    internal class Padder // ICU4N TODO: API - this was public in ICU4J
    {
        public static readonly string FallbackPaddingString = "\u0020"; // i.e. a space

        public enum PadPosition // ICU4N TODO: API - merge with ICU4N.Text.PadPosition?
        {
            BeforePrefix,
            AfterPrefix,
            BeforeSuffix,
            AfterSuffix
        }

        /* like package-private */
        private static readonly Padder NONE = new Padder(null, -1, default);

        internal string paddingString;
        internal int targetWidth;
        internal PadPosition? position;

        public Padder(string paddingString, int targetWidth, PadPosition? position)
        {
            // TODO: Add a few default instances
            this.paddingString = (paddingString is null) ? FallbackPaddingString : paddingString;
            this.targetWidth = targetWidth;
            this.position = position ?? PadPosition.BeforePrefix; // ICU4N: Defaults to BeforePrefix (0) //(position == null) ? PadPosition.BEFORE_PREFIX : position;
        }

        public static Padder None => NONE;

        public static Padder CodePoints(int cp, int targetWidth, PadPosition position)
        {
            // TODO: Validate the code point
            if (targetWidth >= 0)
            {
                string paddingString = new string(Character.ToChars(cp));
                return new Padder(paddingString, targetWidth, position);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(targetWidth), "Padding width must not be negative");
            }
        }

        public bool IsValid => targetWidth > 0;

        public int PadAndApply(IModifier mod1, IModifier mod2, NumberStringBuilder buffer, int leftIndex, int rightIndex)
        {
            int modLength = mod1.CodePointCount + mod2.CodePointCount;
            int requiredPadding = targetWidth - modLength - buffer.CodePointCount;
            Debug.Assert(leftIndex == 0 && rightIndex == buffer.Length); // fix the previous line to remove this assertion

            int length = 0;
            if (requiredPadding <= 0)
            {
                // Padding is not required.
                length += mod1.Apply(buffer, leftIndex, rightIndex);
                length += mod2.Apply(buffer, leftIndex, rightIndex + length);
                return length;
            }

            if (position == PadPosition.AfterPrefix)
            {
                length += AddPaddingHelper(paddingString, requiredPadding, buffer, leftIndex);
            }
            else if (position == PadPosition.BeforeSuffix)
            {
                length += AddPaddingHelper(paddingString, requiredPadding, buffer, rightIndex + length);
            }
            length += mod1.Apply(buffer, leftIndex, rightIndex + length);
            length += mod2.Apply(buffer, leftIndex, rightIndex + length);
            if (position == PadPosition.BeforePrefix)
            {
                length += AddPaddingHelper(paddingString, requiredPadding, buffer, leftIndex);
            }
            else if (position == PadPosition.AfterSuffix)
            {
                length += AddPaddingHelper(paddingString, requiredPadding, buffer, rightIndex + length);
            }

            return length;
        }

        private static int AddPaddingHelper(string paddingString, int requiredPadding, NumberStringBuilder buffer,
                int index)
        {
            for (int i = 0; i < requiredPadding; i++)
            {
                // TODO: If appending to the end, this will cause actual insertion operations. Improve.
                buffer.Insert(index, paddingString, null);
            }
            return paddingString.Length * requiredPadding;
        }
    }
}
