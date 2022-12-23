using ICU4N.Text;
using ICU4N.Util;
using J2N.Collections.Generic.Extensions;
using System.Collections.Generic;

namespace ICU4N.Impl
{
    /// <summary>
    /// ICU's currency meta info data.
    /// </summary>
    public class ICUCurrencyMetaInfo : CurrencyMetaInfo
    {
        private ICUResourceBundle regionInfo;
        private ICUResourceBundle digitInfo;

#pragma warning disable CS0618 // Type or member is obsolete
        public ICUCurrencyMetaInfo()
        {
            ICUResourceBundle bundle = (ICUResourceBundle)ICUResourceBundle.GetBundleInstance(
                ICUData.IcuCurrencyBaseName, "supplementalData",
                ICUResourceBundle.IcuDataAssembly);
            regionInfo = bundle.FindTopLevel("CurrencyMap");
            digitInfo = bundle.FindTopLevel("CurrencyMeta");
        }
#pragma warning restore CS0618 // Type or member is obsolete

        public override IList<CurrencyInfo> CurrencyInfo(CurrencyFilter filter)
        {
            return Collect(new InfoCollector(), filter);
        }

        public override IList<string> Currencies(CurrencyFilter filter)
        {
            return Collect(new CurrencyCollector(), filter);
        }

        public override IList<string> Regions(CurrencyFilter filter)
        {
            return Collect(new RegionCollector(), filter);
        }

        public override CurrencyDigits CurrencyDigits(string isoCode)
        {
            return CurrencyDigits(isoCode, CurrencyUsage.Standard);
        }

        public override CurrencyDigits CurrencyDigits(string isoCode, CurrencyUsage currencyPurpose)
        {
            ICUResourceBundle b = digitInfo.FindWithFallback(isoCode);
            if (b is null)
            {
                b = digitInfo.FindWithFallback("DEFAULT");
            }
            int[] data = b.GetInt32Vector();
            if (currencyPurpose == CurrencyUsage.Cash)
            {
                return new CurrencyDigits(data[2], data[3]);
            }
            else if (currencyPurpose == CurrencyUsage.Standard)
            {
                return new CurrencyDigits(data[0], data[1]);
            }
            else
            {
                return new CurrencyDigits(data[0], data[1]);
            }
        }

        private IList<T> Collect<T>(ICollector<T> collector, CurrencyFilter filter)
        {
            // We rely on the fact that the data lists the regions in order, and the
            // priorities in order within region.  This means we don't need
            // to sort the results to ensure the ordering matches the spec.

            if (filter == null)
            {
                filter = CurrencyFilter.All;
            }
            int needed = collector.Collects;
            if (filter.Region != null)
            {
                needed |= Region;
            }
            if (filter.Currency != null)
            {
                needed |= Currency;
            }
            if (filter.From != long.MinValue || filter.To != long.MaxValue)
            {
                needed |= Date;
            }
            if (filter.TenderOnly)
            {
                needed |= Tender;
            }

            if (needed != 0)
            {
                if (filter.Region != null)
                {
                    ICUResourceBundle b = regionInfo.FindWithFallback(filter.Region);
                    if (b != null)
                    {
                        CollectRegion(collector, filter, needed, b);
                    }
                }
                else
                {
                    for (int i = 0; i < regionInfo.Length; i++)
                    {
                        CollectRegion(collector, filter, needed, regionInfo.At(i));
                    }
                }
            }

            return collector.ToList();
        }

        private void CollectRegion<T>(ICollector<T> collector, CurrencyFilter filter,
                int needed, ICUResourceBundle b)
        {

            string region = b.Key;
            if (needed == Region)
            {
                collector.Collect(b.Key, null, 0, 0, -1, false);
                return;
            }

            for (int i = 0; i < b.Length; i++)
            {
                ICUResourceBundle r = b.At(i);
                if (r.Length == 0)
                {
                    // AQ[0] is an empty array instead of a table, so the bundle is null.
                    // There's no data here, so we skip this entirely.
                    // We'd do a type test, but the ResourceArray type is private.
                    continue;
                }
                string currency = null;
                long from = long.MinValue;
                long to = long.MaxValue;
                bool tender = true;

                if ((needed & Currency) != 0)
                {
                    ICUResourceBundle currBundle = r.At("id");
                    currency = currBundle.GetString();
                    if (filter.Currency != null && !filter.Currency.Equals(currency))
                    {
                        continue;
                    }
                }

                if ((needed & Date) != 0)
                {
                    from = GetDate(r.At("from"), long.MinValue, false);
                    to = GetDate(r.At("to"), long.MaxValue, true);
                    // In the data, to is always > from.  This means that when we have a range
                    // from == to, the comparisons below will always do the right thing, despite
                    // the range being technically empty.  It really should be [from, from+1) but
                    // this way we don't need to fiddle with it.
                    if (filter.From > to)
                    {
                        continue;
                    }
                    if (filter.To < from)
                    {
                        continue;
                    }
                }
                if ((needed & Tender) != 0)
                {
                    ICUResourceBundle tenderBundle = r.At("tender");
                    tender = tenderBundle == null || "true".Equals(tenderBundle.GetString());
                    if (filter.TenderOnly && !tender)
                    {
                        continue;
                    }
                }

                // data lists elements in priority order, so 'i' suffices
                collector.Collect(region, currency, from, to, i, tender);
            }
        }

        private const long MASK = 4294967295L;
        private long GetDate(ICUResourceBundle b, long defaultValue, bool endOfDay)
        {
            if (b == null)
            {
                return defaultValue;
            }
            int[] values = b.GetInt32Vector();
            return ((long)values[0] << 32) | ((values[1]) & MASK);
        }

        // Utility, just because I don't like the n^2 behavior of using list.contains to build a
        // list of unique items.  If we used java 6 we could use their class for this.
        private class UniqueList<T>
        {
            private ISet<T> seen = new HashSet<T>();
            private IList<T> list = new List<T>();

            internal static UniqueList<T> Create()
            {
                return new UniqueList<T>();
            }

            internal void Add(T value)
            {
                if (!seen.Contains(value))
                {
                    list.Add(value);
                    seen.Add(value);
                }
            }

            internal IList<T> ToList()
            {
                return list.AsReadOnly();
            }
        }

        private sealed class InfoCollector : ICollector<CurrencyInfo>
        {
            // Data is already unique by region/priority, so we don't need to be concerned
            // about duplicates.
            private IList<CurrencyInfo> result = new List<CurrencyInfo>();

            public void Collect(string region, string currency, long from, long to, int priority, bool tender)
            {
                result.Add(new CurrencyInfo(region, currency, from, to, priority, tender));
            }

            public IList<CurrencyInfo> ToList()
            {
                return result.AsReadOnly();
            }

            public int Collects => Everything;
        }

        private sealed class RegionCollector : ICollector<string>
        {
            private readonly UniqueList<string> result = UniqueList<string>.Create();

            public void Collect(
                    string region, string currency, long from, long to, int priority, bool tender)
            {
                result.Add(region);
            }

            public int Collects => Region;

            public IList<string> ToList()
            {
                return result.ToList();
            }
        }

        private sealed class CurrencyCollector : ICollector<string>
        {
            private readonly UniqueList<string> result = UniqueList<string>.Create();

            public void Collect(
                    string region, string currency, long from, long to, int priority, bool tender)
            {
                result.Add(currency);
            }

            public int Collects => Currency;

            public IList<string> ToList()
            {
                return result.ToList();
            }
        }

        private const int Region = 1;
        private const int Currency = 2;
        private const int Date = 4;
        private const int Tender = 8;
        private const int Everything = int.MaxValue;

        private interface ICollector<T>
        {
            /// <summary>
            /// Gets a bitmask of Region/Currency/Date indicating which features we collect.
            /// </summary>
            int Collects { get; }

            /// <summary>
            /// Called with data passed by filter. Values not collected by filter should be ignored.
            /// </summary>
            /// <param name="region">The region code (null if ignored).</param>
            /// <param name="currency">The currency code (null if ignored).</param>
            /// <param name="from">Start time (0 if ignored).</param>
            /// <param name="to">End time (0 if ignored).</param>
            /// <param name="priority">Priority (-1 if ignored).</param>
            /// <param name="tender"><c>true</c> if currency is legal tender.</param>
            void Collect(string region, string currency, long from, long to, int priority, bool tender);

            /// <summary>
            /// Return the list of unique items in the order in which we encountered them for the
            /// first time. The returned list is unmodifiable.
            /// </summary>
            /// <returns>The list.</returns>
            IList<T> ToList();
        }
    }
}
