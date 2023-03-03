using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Numerics
{
    internal interface IAffixPatternProvider // ICU4N TODO: API - this was public in ICU4J
    {
        //char CharAt(int flags, int i);
        char this[AffixPatternProviderFlags flags, int index] { get; }

        public int Length(AffixPatternProviderFlags flags);

        public bool HasCurrencySign { get; }

        public bool PositiveHasPlusSign { get; }

        public bool HasNegativeSubpattern { get; }

        public bool NegativeHasMinusSign { get; }

        public bool ContainsSymbolType(AffixUtils.Type type);
    }

    [Flags]
    internal enum AffixPatternProviderFlags
    {
        PluralMask = 0xff,
        Prefix = 0x100,
        NegativeSubpattern = 0x200,
        Padding = 0x400,
    }

    //internal static class AffixPatternProviderFlags
    //{
    //    public const int PLURAL_MASK = 0xff;
    //    public const int PREFIX = 0x100;
    //    public const int NEGATIVE_SUBPATTERN = 0x200;
    //    public const int PADDING = 0x400;
    //}
}
