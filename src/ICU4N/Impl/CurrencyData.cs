using ICU4N.Globalization;
using ICU4N.Support.Collections;
using ICU4N.Text;
using J2N.Collections.Generic.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ICU4N.Impl
{
    public interface ICurrencyDisplayInfoProvider
    {
        CurrencyDisplayInfo GetInstance(UCultureInfo locale, bool withFallback);
        bool HasData { get; }
    }

    public abstract class CurrencyDisplayInfo : CurrencyDisplayNames
    {
        public CurrencyDisplayInfo()
#pragma warning disable 612, 618
                : base()
#pragma warning restore 612, 618
        {
        }

        public abstract IDictionary<string, string> GetUnitPatterns();
        public abstract CurrencyFormatInfo GetFormatInfo(string isoCode);
        public abstract CurrencySpacingInfo GetSpacingInfo(); // ICU4N TODO: API - make into property ?
        public abstract string GetNarrowSymbol(string isoCode);
    }

    public sealed class CurrencyFormatInfo
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

    public sealed class CurrencySpacingInfo
    {
        private static readonly int SpacingTypeCount = Enum.GetNames(typeof(SpacingType)).Length;
        private static readonly int SpacingPatternCount = Enum.GetNames(typeof(SpacingPattern)).Length;
        private readonly string[][] symbols = Arrays.NewRectangularArray<string>(SpacingTypeCount, SpacingTypeCount);

        public bool HasBeforeCurrency { get; set; } = false;
        public bool HasAfterCurrency { get; set; } = false;

        public enum SpacingType { Before, After };

        public enum SpacingPattern
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

        private const string DEFAULT_CUR_MATCH = "[:letter:]";
        private const string DEFAULT_CTX_MATCH = "[:digit:]";
        private const string DEFAULT_INSERT = " ";

        public static readonly CurrencySpacingInfo Default = new CurrencySpacingInfo(
                DEFAULT_CUR_MATCH, DEFAULT_CTX_MATCH, DEFAULT_INSERT,
                DEFAULT_CUR_MATCH, DEFAULT_CTX_MATCH, DEFAULT_INSERT);
    }

    public class DefaultCurrencyDisplayInfo : CurrencyDisplayInfo // ICU4N: Renamed from DefaultInfo
    {
        private readonly bool fallback;

        private DefaultCurrencyDisplayInfo(bool fallback)
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

        public override IDictionary<string, string> SymbolMap => new Dictionary<string, string>().AsReadOnly();

        public override IDictionary<string, string> NameMap => new Dictionary<string, string>().AsReadOnly();

        public override UCultureInfo UCulture
            => UCultureInfo.InvariantCulture;

        public override IDictionary<string, string> GetUnitPatterns()
        {
            if (fallback)
            {
                return new Dictionary<string, string>().AsReadOnly();
            }
            return null;
        }

        public override CurrencyFormatInfo GetFormatInfo(string isoCode)
        {
            return null;
        }

        public override CurrencySpacingInfo GetSpacingInfo()
        {
            return fallback ? CurrencySpacingInfo.Default : null;
        }

        private static readonly CurrencyDisplayInfo FALLBACK_INSTANCE = new DefaultCurrencyDisplayInfo(true);
        private static readonly CurrencyDisplayInfo NO_FALLBACK_INSTANCE = new DefaultCurrencyDisplayInfo(false);
    }

    public class CurrencyData
    {
        public static ICurrencyDisplayInfoProvider Provider { get; private set; } = LoadCurrencyDisplayInfoProvider();

        private static ICurrencyDisplayInfoProvider LoadCurrencyDisplayInfoProvider()
        {
            // ICU4N specific - since we have merged this into the ICU4N assembly, we need to fallback if
            // there is no data. We cannot determine that by whether the assembly is installed.
            ICurrencyDisplayInfoProvider temp = new ICUCurrencyDisplayInfoProvider();
            if (temp.HasData)
                return temp;

            return new FallbackCurrencyDisplayInfoProvider();

            //try
            //{
            //    Type clzz = Type.GetType("ICU4N.Impl.ICUCurrencyDisplayInfoProvider, ICU4N.CurrencyData");
            //    temp = (ICurrencyDisplayInfoProvider)Activator.CreateInstance(clzz); //clzz.newInstance();
            //}
            //catch (Exception)
            //{
            //    temp = new FallbackCurrencyDisplayInfoProvider();
            //}
            //return temp;
        }

        private CurrencyData() { }

        // ICU4N specific - de-nested ICurrencyDisplayInfoProvider

        // ICU4N specific - de-nested CurrencyDisplayInfo

        // ICU4N specific - de-nested CurrencyFormatInfo

        // ICU4N specific - de-nested CurrencySpacingInfo

        private class FallbackCurrencyDisplayInfoProvider : ICurrencyDisplayInfoProvider
        {
            public bool HasData => false;

            public CurrencyDisplayInfo GetInstance(UCultureInfo locale, bool withFallback)
            {
                return DefaultCurrencyDisplayInfo.GetWithFallback(withFallback);
            }
        }

        // ICU4N specific - de-nested DefaultInfo
    }
}
