using ICU4N.Text;
using J2N.Numerics;
using J2N.Text;
using System;
using System.Diagnostics;

namespace ICU4N.Numerics
{
    /// <summary>
    /// Performs manipulations on affix patterns: the prefix and suffix strings associated with a decimal
    /// format pattern. For example:
    /// 
    /// <list type="table">
    ///     <listheader>
    ///         <term>Affix Pattern</term>
    ///         <term>Example Unescaped (Formatted) String</term>
    ///     </listheader>
    ///     <item>
    ///         <term>abc</term>
    ///         <term>abc</term>
    ///     </item>
    ///     <item>
    ///         <term>ab-</term>
    ///         <term>ab−</term>
    ///     </item>
    ///     <item>
    ///         <term>ab'-'</term>
    ///         <term>ab-</term>
    ///     </item>
    ///     <item>
    ///         <term>ab''</term>
    ///         <term>ab'</term>
    ///     </item>
    /// </list>
    /// 
    /// To manually iterate over tokens in a literal string, use the following pattern, which is designed
    /// to be efficient.
    /// <code>
    /// long tag = 0L;
    /// while (AffixUtils.HasNext(tag, patternString))
    /// {
    ///     tag = AffixUtils.NextToken(tag, patternString);
    ///     int typeOrCp = AffixUtils.GetTypeOrCp(tag);
    ///     switch (typeOrCp)
    ///     {
    ///         case (int)AffixUtils.Type.MinusSign:
    ///             // Current token is a minus sign.
    ///             break;
    ///         case (int)AffixUtils.Type.PlusSign:
    ///             // Current token is a plus sign.
    ///             break;
    ///         case (int)AffixUtils.Type.Percent:
    ///             // Current token is a percent sign.
    ///             break;
    ///         // ... other types ...
    ///         default:
    ///             // Current token is an arbitrary code point.
    ///             // The variable typeOrCp is the code point.
    ///             break;
    ///     }
    /// }
    /// </code>
    /// </summary>
    internal static partial class AffixUtils // ICU4N TODO: API - this was public in ICU4J
    {
        private const int STATE_BASE = 0;
        private const int STATE_FIRST_QUOTE = 1;
        private const int STATE_INSIDE_QUOTE = 2;
        private const int STATE_AFTER_QUOTE = 3;
        private const int STATE_FIRST_CURR = 4;
        private const int STATE_SECOND_CURR = 5;
        private const int STATE_THIRD_CURR = 6;
        private const int STATE_FOURTH_CURR = 7;
        private const int STATE_FIFTH_CURR = 8;
        private const int STATE_OVERFLOW_CURR = 9;

        /// <summary>Represents a literal character; the value is stored in the code point field.</summary>
        private const int TYPE_CODEPOINT = 0;

        /// <summary>
        /// The affix symbol type.
        /// </summary>
        public enum Type
        {
            /// <summary>Represents a minus sign symbol '-'.</summary>
            MinusSign = -1,

            /// <summary>Represents a plus sign symbol '+'.</summary>
            PlusSign = -2,

            /// <summary>Represents a percent sign symbol '%'.</summary>
            Percent = -3,

            /// <summary>Represents a permille sign symbol '‰'.</summary>
            PerMille = -4,

            /// <summary>Represents a single currency symbol '¤'.</summary>
            CurrencySymbol = -5,

            /// <summary>Represents a double currency symbol '¤¤'.</summary>
            CurrencyDouble = -6,

            /// <summary>Represents a triple currency symbol '¤¤¤'.</summary>
            CurrencyTriple = -7,

            /// <summary>Represents a quadruple currency symbol '¤¤¤¤'.</summary>
            CurrencyQuad = -8,

            /// <summary>Represents a quintuple currency symbol '¤¤¤¤¤'.</summary>
            CurrencyQuint = -9,

            /// <summary>Represents a sequence of six or more currency symbols.</summary>
            CurrencyOverflow = -15,
        }

        public interface ISymbolProvider
        {
            // ICU4N TODO: Return ReadOnlySpan<char> where supported? This was originally ICharSequence, but it always is a string.
            public string GetSymbol(Type type);
        }

        // ICU4N specific: Moved EstimateLength(ICharSequence patternString) to AffixUtilsExtension.tt


        // ICU4N TODO: API - Where supported, convert these to use Span<char> and make them TryEscape() so we can use the stack.

        // ICU4N specific: Escape(ICharSequence input, StringBuilder output) to AffixUtilsExtension.tt

        // ICU4N specific: Escape(ICharSequence input) to AffixUtilsExtension.tt



        public static NumberFormatField GetFieldForType(Type type)
        {
            switch (type)
            {
                case Type.MinusSign:
                    return NumberFormatField.Sign;
                case Type.PlusSign:
                    return NumberFormatField.Sign;
                case Type.Percent:
                    return NumberFormatField.Percent;
                case Type.PerMille:
                    return NumberFormatField.PerMille;
                case Type.CurrencySymbol:
                    return NumberFormatField.Currency;
                case Type.CurrencyDouble:
                    return NumberFormatField.Currency;
                case Type.CurrencyTriple:
                    return NumberFormatField.Currency;
                case Type.CurrencyQuad:
                    return NumberFormatField.Currency;
                case Type.CurrencyQuint:
                    return NumberFormatField.Currency;
                case Type.CurrencyOverflow:
                    return NumberFormatField.Currency;
                default:
                    throw new InvalidOperationException(); //throw new AssertionError(); // Should never get here
            }
        }

        // ICU4N TODO: API - Where supported, convert these to use Span<char> and make them TryEscape() so we can use the stack.

        // ICU4N specific: Unescape(
        //      ICharSequence affixPattern,
        //      NumberStringBuilder output,
        //      int position,
        //      ISymbolProvider provider) to AffixUtilsExtension.tt

        // ICU4N specific: UnescapedCodePointCount(ICharSequence affixPattern, ISymbolProvider provider) to AffixUtilsExtension.tt

        // ICU4N specific: ContainsType(ICharSequence affixPattern, Type type) to AffixUtilsExtension.tt

        // ICU4N specific: HasCurrencySymbols(ICharSequence affixPattern) to AffixUtilsExtension.tt


        // ICU4N TODO: Refactor this - We can pass in an allocated Span<char> to make the replacement.

        // ICU4N specific: ReplaceType(ICharSequence affixPattern, Type type, char replacementChar) to AffixUtilsExtension.tt

        // ICU4N specific: NextToken(long tag, ICharSequence patternString) to AffixUtilsExtension.tt

        // ICU4N specific: HasNext(long tag, ICharSequence affixPattern) to AffixUtilsExtension.tt

        /// <summary>
        /// This function helps determine the identity of the token consumed by <see cref="NextToken(long, ICharSequence)"/>.
        /// Converts from a bitmask tag, based on a call to <see cref="NextToken(long, ICharSequence)"/>, to its corresponding symbol
        /// type or code point.
        /// </summary>
        /// <param name="tag">The bitmask tag of the current token, as returned by <see cref="NextToken(long, ICharSequence)"/>.</param>
        /// <returns>If less than zero, a symbol type corresponding to one of the <c>TYPE_</c> constants, such as
        /// <see cref="Type.MinusSign"/>. If greater than or equal to zero, a literal code point.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="tag"/> is less than 0.</exception>
        public static int GetTypeOrCp(long tag)
        {
            if (tag < 0)
                throw new ArgumentOutOfRangeException(nameof(tag)); // ICU4N TODO: Error message
            //assert tag >= 0;
            int type = GetType(tag);
            return (type == TYPE_CODEPOINT) ? GetCodePoint(tag) : -type;
        }

        /// <summary>
        /// Encodes the given values into a 64-bit tag.
        /// 
        /// <list type="bullet">
        ///     <item><description>Bits 0-31 => offset (int32)</description></item>
        ///     <item><description>Bits 32-35 => type (uint4)</description></item>
        ///     <item><description>Bits 36-39 => state (uint4)</description></item>
        ///     <item><description>Bits 40-60 => code point (uint21)</description></item>
        ///     <item><description>Bits 61-63 => unused</description></item>
        /// </list>
        /// </summary>
        private static long MakeTag(int offset, int type, int state, int cp)
        {
            long tag = 0L;
            tag |= (uint)offset;
            tag |= (-(long)type) << 32;
            tag |= ((long)state) << 36;
            tag |= ((long)cp) << 40;
            Debug.Assert(tag >= 0);
            return tag;
        }

        internal static int GetOffset(long tag)
        {
            return (int)(tag & 0xffffffff);
        }

        internal static int GetType(long tag)
        {
            return (int)((tag.TripleShift(32)) & 0xf);
        }

        internal static int GetState(long tag)
        {
            return (int)((tag.TripleShift(36)) & 0xf);
        }

        internal static int GetCodePoint(long tag)
        {
            return (int)(tag.TripleShift(40));
        }
    }
}
