using ICU4N.Support.Collections;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ICU4N.Impl
{
    public class CurrencyData
    {
        private static readonly ICurrencyDisplayInfoProvider provider;

        public static ICurrencyDisplayInfoProvider Provider { get { return provider; } }

        private CurrencyData() { }

        public interface ICurrencyDisplayInfoProvider // ICU4N TODO: API de-nest
        {
            CurrencyDisplayInfo GetInstance(ULocale locale, bool withFallback);
            bool HasData { get; }
        }

        public abstract class CurrencyDisplayInfo : CurrencyDisplayNames // ICU4N TODO: API de-nest
        {
            public CurrencyDisplayInfo()
#pragma warning disable 612, 618
                : base()
#pragma warning restore 612, 618
            {
            }

            public abstract IDictionary<string, string> GetUnitPatterns();
            public abstract CurrencyFormatInfo GetFormatInfo(string isoCode);
            public abstract CurrencySpacingInfo GetSpacingInfo();
            public abstract string GetNarrowSymbol(string isoCode);
        }

        public sealed class CurrencyFormatInfo // ICU4N TODO: API de-nest
        {
            public string IsoCode { get; private set; }
            public string CurrencyPattern { get; private set; }
            public string MonetaryDecimalSeparator { get; private set; }
            public string MonetaryGroupingSeparator { get; private set; }

            public CurrencyFormatInfo(string isoCode, string currencyPattern, string monetarySeparator,
                    string monetaryGroupingSeparator)
            {
                this.IsoCode = isoCode;
                this.CurrencyPattern = currencyPattern;
                this.MonetaryDecimalSeparator = monetarySeparator;
                this.MonetaryGroupingSeparator = monetaryGroupingSeparator;
            }
        }

        public sealed class CurrencySpacingInfo // ICU4N TODO: API de-nest
        {
            private static readonly int SpacingTypeCount = Enum.GetNames(typeof(SpacingType)).Length;
            private static readonly int SpacingPatternCount = Enum.GetNames(typeof(SpacingPattern)).Length;
            private readonly string[][] symbols = Arrays.NewRectangularArray<string>(SpacingTypeCount, SpacingTypeCount); ///new String[SpacingType.COUNT.ordinal()][SpacingPattern.COUNT.ordinal()];

            public bool hasBeforeCurrency = false;
            public bool hasAfterCurrency = false;

            public enum SpacingType { Before, After }; // ICU4N TODO: API de-nest

            public enum SpacingPattern // ICU4N TODO: API de-nest
            {
                CurrencyMatch,
                SurroundingMatch,
                InsertBetween
            };

            public CurrencySpacingInfo() { }

            public CurrencySpacingInfo(params string[] strings)
            {
                Debug.Assert(strings.Length == 6);

                int k = 0;
                for (int i = 0; i < (int)SpacingTypeCount; i++)
                {
                    for (int j = 0; j < (int)SpacingPatternCount; j++)
                    {
                        symbols[i][j] = strings[k];
                        k++;
                    }
                }
            }

            public void SetSymbolIfNull(SpacingType type, SpacingPattern pattern, string value)
            {
                int i = (int)type;
                int j = (int)pattern;
                if (symbols[i][j] == null)
                {
                    symbols[i][j] = value;
                }
            }

            public string[] GetBeforeSymbols()
            {
                return symbols[(int)SpacingType.Before];
            }

            public string[] GetAfterSymbols()
            {
                return symbols[(int)SpacingType.After];
            }

            private static readonly string DEFAULT_CUR_MATCH = "[:letter:]";
            private static readonly string DEFAULT_CTX_MATCH = "[:digit:]";
            private static readonly string DEFAULT_INSERT = " ";

            public static readonly CurrencySpacingInfo DEFAULT = new CurrencySpacingInfo(
                    DEFAULT_CUR_MATCH, DEFAULT_CTX_MATCH, DEFAULT_INSERT,
                    DEFAULT_CUR_MATCH, DEFAULT_CTX_MATCH, DEFAULT_INSERT); // ICU4N TODO: API - rename to follow .NET Conventions
        }

        private class FallbackCurrencyDisplayInfoProvider : ICurrencyDisplayInfoProvider
        {
            public bool HasData => false;

            public CurrencyDisplayInfo GetInstance(ULocale locale, bool withFallback)
            {
                return DefaultInfo.GetWithFallback(withFallback);
            }
        }

        static CurrencyData()
        {
            ICurrencyDisplayInfoProvider temp = null;
            try
            {
                Type clzz = Type.GetType("ICU4N.Impl.ICUCurrencyDisplayInfoProvider, ICU4N.CurrencyData");
                temp = (ICurrencyDisplayInfoProvider)Activator.CreateInstance(clzz); //clzz.newInstance();
            }
            catch (Exception t)
            {
                temp = new FallbackCurrencyDisplayInfoProvider();
            }
            provider = temp;
        }

        public class DefaultInfo : CurrencyDisplayInfo // ICU4N TODO: API de-nest
        {
            private readonly bool fallback;

            private DefaultInfo(bool fallback)
            {
                this.fallback = fallback;
            }

            public static CurrencyDisplayInfo GetWithFallback(bool fallback)
            {
                return fallback ? FALLBACK_INSTANCE : NO_FALLBACK_INSTANCE;
            }

            public override string GetName(string isoCode)
            {
                return fallback ? isoCode : null;
            }

            public override string GetPluralName(string isoCode, string pluralType)
            {
                return fallback ? isoCode : null;
            }

            public override string GetSymbol(string isoCode)
            {
                return fallback ? isoCode : null;
            }

            public override string GetNarrowSymbol(string isoCode)
            {
                return fallback ? isoCode : null;
            }

            public override IDictionary<string, string> SymbolMap
            {
                get { return new Dictionary<string, string>(); }
            }

            public override IDictionary<string, string> NameMap
            {
                get { return new Dictionary<string, string>(); }
            }

            public override ULocale ULocale // ICU4N TODO: API - rename UCultureInfo
            {
                get { return ULocale.ROOT; }
            }

            public override IDictionary<string, string> GetUnitPatterns()
            {
                if (fallback)
                {
                    return new Dictionary<string, string>();
                }
                return null;
            }

            public override CurrencyFormatInfo GetFormatInfo(string isoCode)
            {
                return null;
            }

            public override CurrencySpacingInfo GetSpacingInfo()
            {
                return fallback ? CurrencySpacingInfo.DEFAULT : null;
            }

            private static readonly CurrencyDisplayInfo FALLBACK_INSTANCE = new DefaultInfo(true);
            private static readonly CurrencyDisplayInfo NO_FALLBACK_INSTANCE = new DefaultInfo(false);
        }
    }
}
