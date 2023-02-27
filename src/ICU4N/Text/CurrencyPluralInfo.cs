using ICU4N.Globalization;
using ICU4N.Impl;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using JCG = J2N.Collections.Generic;

namespace ICU4N.Text
{
    /// <summary>
    /// This class represents the information needed by
    /// <see cref="DecimalFormat"/> to format currency plural,
    /// such as "3.00 US dollars" or "1.00 US dollar".
    /// <see cref="DecimalFormat"/> creates for itself an instance of
    /// <see cref="CurrencyPluralInfo"/> from its locale data.
    /// If you need to change any of these symbols, you can get the
    /// <see cref="CurrencyPluralInfo"/> object from your
    /// <see cref="DecimalFormat"/> and modify it.
    /// <para/>
    /// Following are the information needed for currency plural format and parse:
    /// <list type="bullet">
    ///     <item><term>locale information</term></item>
    ///     <item><term>plural rule of the locale</term></item>
    ///     <item><term>currency plural pattern of the locale</term></item>
    /// </list>
    /// </summary>
    /// <stable>ICU 4.2</stable>
    [Serializable]
    public class CurrencyPluralInfo
#if FEATURE_CLONEABLE
        , ICloneable
#endif
    {
        //private static readonly long serialVersionUID = 1;

        /// <summary>
        /// Create a <see cref="CurrencyPluralInfo"/> object for <see cref="UCultureInfo.CurrentCulture"/>.
        /// </summary>
        /// <stable>ICU 4.2</stable>
        public CurrencyPluralInfo()
        {
            Initialize(UCultureInfo.CurrentCulture); // ICU4N TODO: .NET defaults to invariant culture
        }

        /// <summary>
        /// Create a <see cref="CurrencyPluralInfo"/> object for the given locale.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <stable>ICU 4.2</stable>
        public CurrencyPluralInfo(CultureInfo locale)
        {
            Initialize(locale.ToUCultureInfo());
        }

        /// <summary>
        /// Create a <see cref="CurrencyPluralInfo"/> object for the given locale.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <stable>ICU 4.2</stable>
        public CurrencyPluralInfo(UCultureInfo locale)
        {
            Initialize(locale);
        }

        /// <summary>
        /// Gets a <see cref="CurrencyPluralInfo"/> instance for the default locale.
        /// </summary>
        /// <returns>A <see cref="CurrencyPluralInfo"/> instance.</returns>
        /// <stable>ICU 4.2</stable>
        public static CurrencyPluralInfo GetInstance()
        {
            return new CurrencyPluralInfo();
        }

        /// <summary>
        /// Gets a <see cref="CurrencyPluralInfo"/> instance for the given locale.
        /// </summary>
        /// <returns>A <see cref="CurrencyPluralInfo"/> instance.</returns>
        /// <stable>ICU 4.2</stable>
        public static CurrencyPluralInfo GetInstance(CultureInfo locale)
        {
            return new CurrencyPluralInfo(locale);
        }

        /// <summary>
        /// Gets a <see cref="CurrencyPluralInfo"/> instance for the given locale.
        /// </summary>
        /// <returns>A <see cref="CurrencyPluralInfo"/> instance.</returns>
        /// <stable>ICU 4.2</stable>
        public static CurrencyPluralInfo GetInstance(UCultureInfo locale)
        {
            return new CurrencyPluralInfo(locale);
        }

        /// <summary>
        /// Gets plural rules of this locale, used for currency plural format.
        /// </summary>
        /// <stable>ICU 4.2</stable>
        public virtual PluralRules PluralRules => pluralRules;

        /// <summary>
        /// Given a plural count, gets currency plural pattern of this locale,
        /// used for currency plural format.
        /// </summary>
        /// <param name="pluralCount">Currency plural count.</param>
        /// <returns>A currency plural pattern based on plural count.</returns>
        /// <stable>ICU 4.2</stable>
        public virtual string GetCurrencyPluralPattern(string pluralCount)
        {
            if (!pluralCountToCurrencyUnitPattern.TryGetValue(pluralCount, out string currencyPluralPattern) || currencyPluralPattern == null)
            {
                // fall back to "other"
                if (!pluralCount.Equals("other", StringComparison.Ordinal))
                {
                    pluralCountToCurrencyUnitPattern.TryGetValue("other", out currencyPluralPattern);
                }
                if (currencyPluralPattern == null)
                {
                    // no currencyUnitPatterns defined,
                    // fallback to predefined default.
                    // This should never happen when ICU resource files are
                    // available, since currencyUnitPattern of "other" is always
                    // defined in root.
                    currencyPluralPattern = defaultCurrencyPluralPattern;
                }
            }
            return currencyPluralPattern;
        }

        /// <summary>
        /// Gets or sets the locale.
        /// </summary>
        /// <stable>ICU 4.2</stable>
        public virtual UCultureInfo Culture
        {
            get => ulocale;
            set => SetCulture(value);
        }

        /// <summary>
        /// Set plural rules.  These are initially set in the constructor based on the locale,
        /// and usually do not need to be changed.
        /// </summary>
        /// <param name="ruleDescription">New plural rule description.</param>
        /// <stable>ICU 4.2</stable>
        public virtual void SetPluralRules(string ruleDescription)
        {
            pluralRules = PluralRules.CreateRules(ruleDescription);
        }

        /// <summary>
        /// Set currency plural patterns.  These are initially set in the constructor based on the
        /// locale, and usually do not need to be changed.
        /// <para/>
        /// The decimal digits part of the pattern cannot be specified via this method.  All plural
        /// forms will use the same decimal pattern as set in the constructor of DecimalFormat.  For
        /// example, you can't set "0.0" for plural "few" but "0.00" for plural "many".
        /// </summary>
        /// <param name="pluralCount">The plural count for which the currency pattern will be overridden.</param>
        /// <param name="pattern">The new currency plural pattern.</param>
        /// <stable>ICU 4.2</stable>
        public void SetCurrencyPluralPattern(string pluralCount, string pattern)
        {
            pluralCountToCurrencyUnitPattern[pluralCount] = pattern;
        }

        /// <summary>
        /// Set locale. This also sets both the plural rules and the currency plural patterns to be
        /// the defaults for the locale.
        /// </summary>
        /// <param name="loc">The new locale to set.</param>
        /// <stable>ICU 4.2</stable>
        private void SetCulture(UCultureInfo loc)
        {
            ulocale = loc;
            Initialize(loc);
        }

        /// <summary>
        /// Standard override
        /// </summary>
        /// <stable>ICU 4.2</stable>
        public virtual object Clone()
        {
            CurrencyPluralInfo other = (CurrencyPluralInfo)base.MemberwiseClone();
            // locale is immutable
            other.ulocale = (UCultureInfo)ulocale.Clone();
            // plural rule is immutable
            //other.pluralRules = pluralRules;
            // clone content
            //other.pluralCountToCurrencyUnitPattern = pluralCountToCurrencyUnitPattern;
            other.pluralCountToCurrencyUnitPattern = new JCG.Dictionary<string, string>(); // ICU4N: This dictionary requires structural equality
            foreach (string pluralCount in pluralCountToCurrencyUnitPattern.Keys)
            {
                if (pluralCountToCurrencyUnitPattern.TryGetValue(pluralCount, out string currencyPattern))
                {
                    other.pluralCountToCurrencyUnitPattern[pluralCount] = currencyPattern;
                }
            }
            return other;
        }

        /// <summary>
        /// Override equals.
        /// </summary>
        /// <stable>ICU 4.2</stable>
        public override bool Equals(object a)
        {
            if (a is CurrencyPluralInfo other)
            {
                return pluralRules.Equals(other.pluralRules) &&
                       pluralCountToCurrencyUnitPattern.Equals(other.pluralCountToCurrencyUnitPattern);
            }
            return false;
        }

        /// <summary>
        /// Override hashCode.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        public override int GetHashCode()
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
        {
            return pluralCountToCurrencyUnitPattern.GetHashCode()
                ^ pluralRules.GetHashCode()
                ^ ulocale.GetHashCode();
        }

        /// <summary>
        /// Given a number, returns the keyword of the first rule that applies
        /// to the number.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal string Select(double number)
        {
            return pluralRules.Select(number);
        }

        /// <summary>
        /// Given a number, returns the keyword of the first rule that applies
        /// to the number.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal string Select(PluralRules.IFixedDecimal numberInfo)
        {
            return pluralRules.Select(numberInfo);
        }

        /// <summary>
        /// Currency plural pattern enumerator.
        /// </summary>
        /// <returns>An enumerator on the currency plural pattern key set.</returns>
        internal IEnumerator<string> GetPluralPatternEnumerator()
        {
            return pluralCountToCurrencyUnitPattern.Keys.GetEnumerator();
        }

        private void Initialize(UCultureInfo uloc)
        {
            ulocale = uloc;
            pluralRules = PluralRules.ForLocale(uloc);
            SetupCurrencyPluralPattern(uloc);
        }

        private void SetupCurrencyPluralPattern(UCultureInfo uloc)
        {
            pluralCountToCurrencyUnitPattern = new JCG.Dictionary<string, string>(); // ICU4N: This dictionary requires structural equality

            string numberStylePattern = NumberFormat.GetPattern(uloc, NumberFormatStyle.NumberStyle);
            // Split the number style pattern into pos and neg if applicable
            int separatorIndex = numberStylePattern.IndexOf(';');
            string negNumberPattern = null;
            if (separatorIndex != -1)
            {
                negNumberPattern = numberStylePattern.Substring(separatorIndex + 1);
                numberStylePattern = numberStylePattern.Substring(0, separatorIndex); // ICU4N: Checked 2nd parameter
            }
            var map = CurrencyData.Provider.GetInstance(uloc, true).GetUnitPatterns();
            foreach (var e in map)
            {
                string pluralCount = e.Key;
                string pattern = e.Value;

                // ICU4N TODO: Reduce replacement allocations

                // replace {0} with numberStylePattern
                // and {1} with triple currency sign
                string patternWithNumber = pattern.Replace("{0}", numberStylePattern);
                string patternWithCurrencySign = patternWithNumber.Replace("{1}", tripleCurrencyStr);
                if (separatorIndex != -1)
                {
                    string negPattern = pattern;
                    string negWithNumber = negPattern.Replace("{0}", negNumberPattern);
                    string negWithCurrSign = negWithNumber.Replace("{1}", tripleCurrencyStr);
                    StringBuilder posNegPatterns = new StringBuilder(patternWithCurrencySign);
                    posNegPatterns.Append(";");
                    posNegPatterns.Append(negWithCurrSign);
                    patternWithCurrencySign = posNegPatterns.ToString();
                }
                pluralCountToCurrencyUnitPattern[pluralCount] = patternWithCurrencySign;
            }
        }


        //-------------------- private data member ---------------------
        //
        // triple currency sign char array
        private static readonly char[] tripleCurrencySign = { (char)0xA4, (char)0xA4, (char)0xA4 };
        // triple currency sign string
        private static readonly string tripleCurrencyStr = new string(tripleCurrencySign);

        // default currency plural pattern char array
        private static readonly char[] defaultCurrencyPluralPatternChar = { (char)0, '.', '#', '#', ' ', (char)0xA4, (char)0xA4, (char)0xA4 };
        // default currency plural pattern string
        private static readonly string defaultCurrencyPluralPattern = new string(defaultCurrencyPluralPatternChar);

        // map from plural count to currency plural pattern, for example
        // one (plural count) --> {0} {1} (currency plural pattern,
        // in which {0} is the amount number, and {1} is the currency plural name).
        private IDictionary<string, string> pluralCountToCurrencyUnitPattern = null; // ICU4N: This dictionary requires structural equality

        /*
         * The plural rule is used to format currency plural name,
         * for example: "3.00 US Dollars".
         * If there are 3 currency signs in the currency pattern,
         * the 3 currency signs will be replaced by the currency plural name.
         */
        private PluralRules pluralRules = null;

        // locale
        private UCultureInfo ulocale = null;
    }
}
