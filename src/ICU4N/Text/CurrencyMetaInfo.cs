using ICU4N.Impl;
using ICU4N.Support.Collections;
using ICU4N.Util;
using J2N;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// Provides information about currencies that is not specific to a locale.
    /// <para/>
    /// A note about currency dates.  The CLDR data provides data to the day,
    /// inclusive.  The date information used by <see cref="CurrencyInfo"/> and <see cref="CurrencyFilter"/>
    /// is represented by milliseconds, which is overly precise.  These times are
    /// in GMT, so queries involving dates should use UTC times, but more generally
    /// you should avoid relying on time of day in queries.
    /// <para/>
    /// This class is not intended for public subclassing.
    /// </summary>
    /// <stable>ICU 4.4</stable>
    public class CurrencyMetaInfo
    {
        private static readonly CurrencyMetaInfo impl;
        private static readonly bool hasData;

        /// <summary>
        /// Returns the unique instance of the currency meta info.
        /// </summary>
        /// <returns>The meta info.</returns>
        /// <stable>ICU 4.4</stable>
        public static CurrencyMetaInfo GetInstance()
        {
            return impl;
        }

        /// <summary>
        /// Returns the unique instance of the currency meta info, or <c>null</c> if
        /// <paramref name="noSubstitute"/> is <c>true</c> and there is no data to support this API.
        /// </summary>
        /// <param name="noSubstitute"><c>true</c> if no substitute data should be used.</param>
        /// <returns>The meta info, or <c>null</c>.</returns>
        /// <stable>ICU 49</stable>
        public static CurrencyMetaInfo GetInstance(bool noSubstitute)
        {
            return hasData ? impl : null;
        }

        /// <summary>
        /// Returns <c>true</c> if there is data for the currency meta info.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public static bool HasData => hasData;

        /// <summary>
        /// Subclass constructor.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected CurrencyMetaInfo()
        {
        }

        // ICU4N: De-nested CurrencyFilter

        // ICU4N: De-nested CurrencyDigits

        // ICU4N: De-nested CurrencyInfo


        ////CLOVER:OFF

        /// <summary>
        /// Returns the list of <see cref="Text.CurrencyInfo"/>s matching the provided filter.  Results
        /// are ordered by country code, then by highest to lowest priority (0 is highest).
        /// The returned list is unmodifiable.
        /// </summary>
        /// <param name="filter">The filter to control which currency info to return.</param>
        /// <returns>The matching information.</returns>
        /// <stable>ICU 4.4</stable>
        public virtual IList<CurrencyInfo> CurrencyInfo(CurrencyFilter filter)
        {
            return Collection.EmptyList<CurrencyInfo>();
        }

        /// <summary>
        /// Returns the list of currency codes matching the provided filter.
        /// Results are ordered as in <see cref="CurrencyInfo(CurrencyFilter)"/>.
        /// The returned list is unmodifiable.
        /// </summary>
        /// <param name="filter">
        /// The filter to control which currencies to return.  If filter is <c>null</c>,
        /// returns all currencies for which information is available.
        /// </param>
        /// <returns>The matching currency codes.</returns>
        /// <stable>ICU 4.4</stable>
        public virtual IList<string> Currencies(CurrencyFilter filter)
        {
            return Collection.EmptyList<string>();
        }

        /// <summary>
        /// Returns the list of region codes matching the provided filter.
        /// Results are ordered as in <see cref="CurrencyInfo(CurrencyFilter)"/>.
        /// The returned list is unmodifiable.
        /// </summary>
        /// <param name="filter">
        /// the filter to control which regions to return.  If filter is <c>null</c>,
        /// returns all regions for which information is available.
        /// </param>
        /// <returns>The matching region codes.</returns>
        /// <stable>ICU 4.4</stable>
        public virtual IList<string> Regions(CurrencyFilter filter)
        {
            return Collection.EmptyList<string>();
        }
        ////CLOVER:ON

        /// <summary>
        /// Returns the <see cref="Text.CurrencyDigits"/> for the currency code.
        /// This is equivalent to <c>CurrencyDigits(isoCode, CurrencyUsage.Standard);</c>
        /// </summary>
        /// <param name="isoCode">The currency code.</param>
        /// <returns>The <see cref="Text.CurrencyDigits"/>.</returns>
        /// <stable>ICU 4.4</stable>
        public virtual CurrencyDigits CurrencyDigits(string isoCode)
        {
            return CurrencyDigits(isoCode, CurrencyUsage.Standard);
        }

        /// <summary>
        /// Returns the <see cref="Text.CurrencyDigits"/> for the currency code with Context Usage.
        /// </summary>
        /// <param name="isoCode">The currency code.</param>
        /// <param name="currencyUsage">The currency usage.</param>
        /// <returns>The <see cref="Text.CurrencyDigits"/>.</returns>
        /// <stable>ICU 54</stable>
        public virtual CurrencyDigits CurrencyDigits(string isoCode, CurrencyUsage currencyUsage)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return defaultDigits;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected static readonly CurrencyDigits defaultDigits = new CurrencyDigits(2, 0);

        static CurrencyMetaInfo()
        {
            CurrencyMetaInfo temp = null;
            bool tempHasData = false;
            try
            {
                //Class <?> clzz = Class.forName("com.ibm.icu.impl.ICUCurrencyMetaInfo");
                //temp = (CurrencyMetaInfo)clzz.newInstance();
                Type clzz = Type.GetType("ICU4N.Impl.ICUCurrencyMetaInfo, ICU4N");
                temp = (CurrencyMetaInfo)Activator.CreateInstance(clzz);
                tempHasData = temp != null;
            }
            catch (Exception)
            {
                // Intentionally blank
                // ICU4N: .NET won't always throw, so we set the instance below.
            }
#pragma warning disable CS0618 // Type or member is obsolete
            temp ??= new CurrencyMetaInfo();
#pragma warning restore CS0618 // Type or member is obsolete
            impl = temp;
            hasData = tempHasData;
        }

        private static string DateString(long date)
        {
            if (date == long.MaxValue || date == long.MinValue)
            {
                return null;
            }
            return Grego.TimeToString(date);
        }

        internal static string DebugString(object o)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                foreach (FieldInfo f in o.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
                {
                    object v = f.GetValue(o);
                    if (v != null)
                    {
                        string s;
                        if (v is DateTime date)
                        {
                            s = DateString(date.GetMillisecondsSinceUnixEpoch());
                        }
                        else if (v is long lng)
                        {
                            s = DateString(lng);
                        }
                        else
                        {
                            s = v.ToString();
                        }
                        if (s == null)
                        {
                            continue;
                        }
                        if (sb.Length > 0)
                        {
                            sb.Append(",");
                        }
                        sb.Append(f.Name)
                            .Append("='")
                            .Append(s)
                            .Append("'");
                    }
                }
            }
            catch (Exception)
            {
                // Swallow
            }
            sb.Insert(0, o.GetType().Name + "(");
            sb.Append(")");
            return sb.ToString();
        }
    }

    /// <summary>
    /// A filter used to select which currency info is returned.
    /// </summary>
    /// <stable>ICU 4.4</stable>
    public sealed class CurrencyFilter
    {
        private readonly string region;
        private readonly string currency;
        private readonly long from;
        private readonly long to;
        private readonly bool tenderOnly;

        /// <summary>
        /// The region to filter on.  If null, accepts any region.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public string Region => region;

        /// <summary>
        /// The currency to filter on.  If null, accepts any currency.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public string Currency => currency;

        /// <summary>
        /// The from date to filter on (as milliseconds).  Accepts any currency on or after this date.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public long From => from;

        /// <summary>
        /// The to date to filter on (as milliseconds).  Accepts any currency on or before this date.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public long To => to;

        /// <summary>
        /// <c>true</c> if we are filtering only for currencies used as legal tender.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public bool TenderOnly => tenderOnly;

        private CurrencyFilter(string region, string currency, long from, long to, bool tenderOnly)
        {
            this.region = region;
            this.currency = currency;
            this.from = from;
            this.to = to;
            this.tenderOnly = tenderOnly;

        }

        private static readonly CurrencyFilter ALL = new CurrencyFilter(
            null, null, long.MinValue, long.MaxValue, false);

        /// <summary>
        /// Returns a filter that accepts all currency data.
        /// </summary>
        /// <returns>The filter.</returns>
        /// <stable>ICU 4.4</stable>
        public static CurrencyFilter All => ALL;

        /// <summary>
        /// Returns a filter that accepts all currencies in use as of the current date.
        /// </summary>
        /// <returns>The filter.</returns>
        /// <seealso cref="WithDate(DateTime)"/>
        /// <stable>ICU 4.4</stable>
        public static CurrencyFilter Now()
        {
            return ALL.WithDate(DateTime.UtcNow.GetMillisecondsSinceUnixEpoch());
        }

        /// <summary>
        /// Returns a filter that accepts all currencies ever used in the given region.
        /// </summary>
        /// <param name="region">The region code.</param>
        /// <returns>The filter.</returns>
        /// <seealso cref="WithRegion(string)"/>
        /// <stable>ICU 4.4</stable>
        public static CurrencyFilter OnRegion(string region)
        {
            return ALL.WithRegion(region);
        }

        /// <summary>
        /// Returns a filter that accepts the given currency.
        /// </summary>
        /// <param name="currency">The currency code.</param>
        /// <returns>The filter.</returns>
        /// <seealso cref="WithCurrency(string)"/>
        /// <stable>ICU 4.4</stable>
        public static CurrencyFilter OnCurrency(string currency)
        {
            return ALL.WithCurrency(currency);
        }

        /// <summary>
        /// Returns a filter that accepts all currencies in use on the given date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>The filter.</returns>
        /// <seealso cref="WithDate(DateTime)"/>
        /// <stable>ICU 4.4</stable>
        public static CurrencyFilter OnDate(DateTime date)
        {
            return ALL.WithDate(date.GetMillisecondsSinceUnixEpoch());
        }

        /// <summary>
        /// Returns a filter that accepts all currencies that were in use at some point between
        /// the given dates, or if dates are equal, currencies in use on that date.
        /// </summary>
        /// <param name="from">Date on or after a currency must have been in use.</param>
        /// <param name="to">Date on or before which a currency must have been in use,
        /// or if equal to from, the date on which a currency must have been in use.</param>
        /// <returns>A filter.</returns>
        /// <seealso cref="WithDateRange(DateTime?, DateTime?)"/>
        /// <stable>ICU 49</stable>
        public static CurrencyFilter OnDateRange(DateTime? from, DateTime? to)
        {
            return ALL.WithDateRange(from, to);
        }

        /// <summary>
        /// Returns a filter that accepts all currencies in use on the given date.
        /// </summary>
        /// <param name="date">The date as milliseconds after Jan 1, 1970.</param>
        /// <stable>ICU 51</stable>
        public static CurrencyFilter OnDate(long date)
        {
            return ALL.WithDate(date);
        }

        /// <summary>
        /// Returns a filter that accepts all currencies that were in use at some
        /// point between the given dates, or if dates are equal, currencies in
        /// use on that date.
        /// </summary>
        /// <param name="from">The date on or after a currency must have been in use.
        /// Measured in milliseconds since Jan 1, 1970 UTC.</param>
        /// <param name="to">The date on or before which a currency must have been in use.
        /// Measured in milliseconds since Jan 1, 1970 UTC.</param>
        /// <stable>ICU 51</stable>
        public static CurrencyFilter OnDateRange(long from, long to)
        {
            return ALL.WithDateRange(from, to);
        }

        /// <summary>
        /// Returns a <see cref="CurrencyFilter"/> for finding currencies that were either once used,
        /// are used, or will be used as tender.
        /// </summary>
        /// <stable>ICU 51</stable>
        public static CurrencyFilter OnTender()
        {
            return ALL.WithTender();
        }

        /// <summary>
        /// Returns a copy of this filter, with the specified region.  Region can be <c>null</c> to
        /// indicate no filter on region.
        /// </summary>
        /// <param name="region">The region code.</param>
        /// <returns>The filter.</returns>
        /// <seealso cref="OnRegion(string)"/>
        /// <stable>ICU 4.4</stable>
        public CurrencyFilter WithRegion(string region)
        {
            return new CurrencyFilter(region, this.currency, this.from, this.to, this.tenderOnly);
        }

        /// <summary>
        /// Returns a copy of this filter, with the specified currency.  Currency can be <c>null</c> to
        /// indicate no filter on currency.
        /// </summary>
        /// <param name="currency">The currency code.</param>
        /// <returns>The filter.</returns>
        /// <seealso cref="OnCurrency(string)"/>
        /// <stable>ICU 4.4</stable>
        public CurrencyFilter WithCurrency(string currency)
        {
            return new CurrencyFilter(this.region, currency, this.from, this.to, this.tenderOnly);
        }

        /// <summary>
        /// Returns a copy of this filter, with from and to set to the given date.
        /// </summary>
        /// <param name="date">The date on which the currency must have been in use.</param>
        /// <returns>The filter.</returns>
        /// <seealso cref="OnDate(DateTime)"/>
        /// <stable>ICU 4.4</stable>
        public CurrencyFilter WithDate(DateTime date)
        {
            var time = date.GetMillisecondsSinceUnixEpoch();
            return new CurrencyFilter(this.region, this.currency, time, time, this.tenderOnly);
        }

        /// <summary>
        /// Returns a copy of this filter, with from and to set to the given dates.
        /// </summary>
        /// <param name="from">Date on or after which the currency must have been in use. The date is represented as milliseconds since Jan 1, 1970 UTC.</param>
        /// <param name="to">Date on or before which the currency must have been in use. The date is represented as milliseconds since Jan 1, 1970 UTC.</param>
        /// <returns>The filter.</returns>
        /// <seealso cref="OnDateRange(DateTime?, DateTime?)"/>
        /// <stable>ICU 49</stable>
        public CurrencyFilter WithDateRange(DateTime? from, DateTime? to)
        {
            long fromLong = from == null ? long.MinValue : from.Value.GetMillisecondsSinceUnixEpoch();
            long toLong = to == null ? long.MaxValue : to.Value.GetMillisecondsSinceUnixEpoch();
            return new CurrencyFilter(this.region, this.currency, fromLong, toLong, this.tenderOnly);
        }

        /// <summary>
        /// Returns a copy of this filter that accepts all currencies in use on
        /// the given date.
        /// </summary>
        /// <param name="date">The date as milliseconds after Jan 1, 1970 UTC.</param>
        /// <stable>ICU 51</stable>
        public CurrencyFilter WithDate(long date)
        {
            return new CurrencyFilter(this.region, this.currency, date, date, this.tenderOnly);
        }

        /// <summary>
        /// Returns a copy of this filter that accepts all currencies that were
        /// in use at some point between the given dates, or if dates are equal,
        /// currencies in use on that date.
        /// </summary>
        /// <param name="from">The date on or after a currency must have been in use.
        /// Measured in milliseconds since Jan 1, 1970 UTC.</param>
        /// <param name="to">The date on or before which a currency must have been in use.
        /// Measured in milliseconds since Jan 1, 1970 UTC.</param>
        /// <stable>ICU 51</stable>
        public CurrencyFilter WithDateRange(long from, long to)
        {
            return new CurrencyFilter(this.region, this.currency, from, to, this.tenderOnly);
        }

        /// <summary>
        /// Returns a copy of this filter that filters for currencies that were
        /// either once used, are used, or will be used as tender.
        /// </summary>
        /// <stable>ICU 51</stable>
        public CurrencyFilter WithTender()
        {
            return new CurrencyFilter(this.region, this.currency, this.from, this.to, true);
        }

        /// <inheritdoc/>
        /// <stable>ICU 4.4</stable>
        public override bool Equals(object rhs)
        {
            return rhs is CurrencyFilter other &&
                Equals(other);
        }

        /// <summary>
        /// Type-safe override of <see cref="Equals(object)"/>.
        /// </summary>
        /// <param name="rhs">The currency filter to compare to.</param>
        /// <returns><c>true</c> if the filters are equal.</returns>
        /// <stable>ICU 4.4</stable>
        public bool Equals(CurrencyFilter rhs)
        {
            return Utility.SameObjects(this, rhs) || (rhs != null &&
                      Equals(this.region, rhs.region) &&
                      Equals(this.currency, rhs.currency) &&
                      this.from == rhs.from &&
                      this.to == rhs.to &&
                      this.tenderOnly == rhs.tenderOnly);
        }

        /// <inheritdoc/>
        /// <stable>ICU 4.4</stable>
        public override int GetHashCode()
        {
            int hc = 0;
            if (region != null)
            {
                hc = region.GetHashCode();
            }
            if (currency != null)
            {
                hc = hc * 31 + currency.GetHashCode();
            }
            hc = hc * 31 + (int)from;
            hc = hc * 31 + (int)(from >>> 32);
            hc = hc * 31 + (int)to;
            hc = hc * 31 + (int)(to >>> 32);
            hc = hc * 31 + (tenderOnly ? 1 : 0);
            return hc;
        }

        /// <summary>
        /// Returns a string representing the filter, for debugging.
        /// </summary>
        /// <returns>A string representing the filter.</returns>
        /// <stable>ICU 4.4</stable>
        public override string ToString()
        {
            return CurrencyMetaInfo.DebugString(this);
        }

        private static bool Equals(string lhs, string rhs)
        {
            return (Utility.SameObjects(lhs, rhs) ||
                (lhs != null && lhs.Equals(rhs, StringComparison.Ordinal)));
        }
    }

    /// <summary>
    /// Represents the raw information about fraction digits and rounding increment.
    /// </summary>
    /// <stable>ICU 4.4</stable>
    public sealed class CurrencyDigits
    {
        private readonly int fractionDigits;
        private readonly int roundingIncrement;

        /// <summary>
        /// Number of fraction digits used to display this currency.
        /// </summary>
        /// <stable>ICU 49</stable>
        public int FractionDigits => fractionDigits;

        /// <summary>
        /// Rounding increment used when displaying this currency.
        /// </summary>
        /// <stable>ICU 49</stable>
        public int RoundingIncrement => roundingIncrement;

        /// <summary>
        /// Constructor for <see cref="CurrencyDigits"/>.
        /// </summary>
        /// <param name="fractionDigits">The fraction digits.</param>
        /// <param name="roundingIncrement">The rounding increment.</param>
        /// <stable>ICU 4.4</stable>
        public CurrencyDigits(int fractionDigits, int roundingIncrement)
        {
            this.fractionDigits = fractionDigits;
            this.roundingIncrement = roundingIncrement;
        }

        /// <summary>
        /// Returns a string representing the currency digits, for debugging.
        /// </summary>
        /// <returns>A string representing the currency digits.</returns>
        /// <stable>ICU 4.4</stable>
        public override string ToString()
        {
            return CurrencyMetaInfo.DebugString(this);
        }
    }

    /// <summary>
    /// Represents a complete currency info record listing the region, currency, from and to dates,
    /// and priority.
    /// <para/>
    /// Use <see cref="CurrencyMetaInfo.CurrencyInfo(CurrencyFilter)"/>
    /// for a list of info objects matching the filter.
    /// </summary>
    /// <stable>ICU 4.4</stable>
    public sealed class CurrencyInfo
    {
        private readonly string region;
        private readonly string code;
        private readonly long from;
        private readonly long to;
        private readonly int priority;
        private readonly bool tender;

        /// <summary>
        /// Region code where currency is used.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public string Region => region;

        /// <summary>
        /// The three-letter ISO currency code.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public string Code => code;

        /// <summary>
        /// Date on which the currency was first officially used in the region.
        /// This is midnight at the start of the first day on which the currency was used, UTC.
        /// The date is represented as milliseconds since 1970-01-01 at midnight UTC.
        /// If there is no date, this is <see cref="long.MinValue"/>.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public long From => from;

        /// <summary>
        /// Date at which the currency stopped being officially used in the region.
        /// This is one millisecond before midnight at the end of the last day on which the currency was used, UTC.
        /// The date is represented as milliseconds since 1970-01-01 at midnight UTC.
        /// If there is no date, this is <see cref="long.MaxValue"/>.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public long To => to;

        /// <summary>
        /// Preference order of currencies being used at the same time in the region.  Lower
        /// values are preferred (generally, this is a transition from an older to a newer
        /// currency).  Priorities within a single country are unique.
        /// </summary>
        /// <stable>ICU 49</stable>
        public int Priority => priority;

        [Obsolete("ICU 51 Use CurrencyMetaInfo.CurrencyInfo(CurrencyFilter) instead.")]
        public CurrencyInfo(string region, string code, long from, long to, int priority)
                : this(region, code, from, to, priority, true)
        {
        }

        /// <summary>
        /// Constructs a currency info.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public CurrencyInfo(string region, string code, long from, long to, int priority, bool tender)
        {
            this.region = region;
            this.code = code;
            this.from = from;
            this.to = to;
            this.priority = priority;
            this.tender = tender;
        }

        /// <summary>
        /// Returns a string representation of this object, useful for debugging.
        /// </summary>
        /// <returns>A string representation of this object.</returns>
        /// <stable>ICU 4.4</stable>
        public override string ToString()
        {
            return CurrencyMetaInfo.DebugString(this);
        }

        /// <summary>
        /// Determine whether or not this currency was once used, is used,
        /// or will be used as tender in this region.
        /// </summary>
        /// <stable>ICU 51</stable>
        public bool IsTender => tender;
    }
}
