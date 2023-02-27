using ICU4N.Util;
#nullable enable

namespace ICU4N.Text
{
    internal interface IDecimalFormatSymbols
    {
        //UCultureInfo ActualCulture { get; }
        int CodePointZero { get; } // Required
        //CultureInfo Culture { get; }
        //Currency Currency { get; set; } // ICU4N TODO: Make this part of UCultureInfo? To lookup the currencySymbol, we need the current culture. Maybe this can be decoupled using the name, but we need the referece every time UCultureInfo.Currency is set.
        string CurrencySymbol { get; set; } // Required
        char DecimalSeparator { get; set; }
        string DecimalSeparatorString { get; set; } // Required
        char Digit { get; set; } // Required
        char[] Digits { get; }
        string[] DigitStrings { get; set; } // Required
        string[] DigitStringsLocal { get; }
        string ExponentMultiplicationSign { get; set; } // Required
        string ExponentSeparator { get; set; } // Required
        char GroupingSeparator { get; set; }
        string GroupingSeparatorString { get; set; }
        string Infinity { get; set; }
        string InternationalCurrencySymbol { get; set; } // For backward compat only - Currency isn't handled here. Return null is okay.
        char MinusSign { get; set; }
        string MinusSignString { get; set; }
        char MonetaryDecimalSeparator { get; set; }
        string MonetaryDecimalSeparatorString { get; set; }
        char MonetaryGroupingSeparator { get; set; }
        string MonetaryGroupingSeparatorString { get; set; }
        string NaN { get; set; }
        char PadEscape { get; set; }
        char PatternSeparator { get; set; }
        char Percent { get; set; }
        string PercentString { get; set; }
        char PerMill { get; set; }
        string PerMillString { get; set; }
        char PlusSign { get; set; }
        string PlusSignString { get; set; }
        char SignificantDigit { get; set; }
        //UCultureInfo UCulture { get; }
        //UCultureInfo ValidCulture { get; }
        char ZeroDigit { get; set; } // ICU4N: discouraged for use ICU58, use DigitStrings instead.

        string? GetPatternForCurrencySpacing(CurrencySpacingPattern itemType, bool beforeCurrency);
        //void SetPatternForCurrencySpacing(CurrencySpacingPattern itemType, bool beforeCurrency, string pattern); // ICU4N: Remove

        // NON-State
        object Clone();
        //bool Equals(object obj);
        //int GetHashCode();

    }
}
