using ICU4N.Text;
using J2N;
using System;
using System.Diagnostics;

namespace ICU4N.Numerics
{
    internal static class PadPositionExtensions // ICU4N TODO: API - this was public in ICU4J
    {
        public static DecimalFormat.PadPosition ToOld(this Padder.PadPosition padPosition)
        {
            switch (padPosition)
            {
                case Padder.PadPosition.BeforePrefix:
                    return DecimalFormat.PadPosition.BeforePrefix;
                case Padder.PadPosition.AfterPrefix:
                    return DecimalFormat.PadPosition.AfterPrefix;
                case Padder.PadPosition.BeforeSuffix:
                    return DecimalFormat.PadPosition.BeforeSuffix;
                case Padder.PadPosition.AfterSuffix:
                    return DecimalFormat.PadPosition.AfterSuffix;
                default:
                    return (DecimalFormat.PadPosition)(-1); // silence compiler errors
            }
        }

        public static Padder.PadPosition ToNew(this DecimalFormat.PadPosition padPosition)
        {
            switch (padPosition)
            {
                case DecimalFormat.PadPosition.BeforePrefix:
                    return Padder.PadPosition.BeforePrefix;
                case DecimalFormat.PadPosition.AfterPrefix:
                    return Padder.PadPosition.AfterPrefix;
                case DecimalFormat.PadPosition.BeforeSuffix:
                    return Padder.PadPosition.BeforeSuffix;
                case DecimalFormat.PadPosition.AfterSuffix:
                    return Padder.PadPosition.AfterSuffix;
                default:
                    throw new ArgumentException("Don't know how to map " + padPosition);
            }
        }
    }

    internal class Padder // ICU4N TODO: API - this was public in ICU4J
    {
        public static readonly string FallbackPaddingString = "\u0020"; // i.e. a space

        public enum PadPosition
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
            this.position = position; // ICU4N: Defaults to BeforePrefix (0) //(position == null) ? PadPosition.BEFORE_PREFIX : position;
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
