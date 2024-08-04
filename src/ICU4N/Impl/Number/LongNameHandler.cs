using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Numerics;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Text;
using static ICU4N.Numerics.NumberFormatter;

namespace ICU4N.Numerics
{
    internal class LongNameHandler : IMicroPropsGenerator
    {
        internal const int CharStackBufferSize = 32;

        //////////////////////////
        /// BEGIN DATA LOADING ///
        //////////////////////////

        private sealed class PluralTableSink : ResourceSink
        {

            internal readonly IDictionary<StandardPlural, string> output;

            public PluralTableSink(IDictionary<StandardPlural, string> output)
            {
                this.output = output ?? throw new ArgumentNullException(nameof(output)); // ICU4N: Added guard clause
            }

            public override void Put(ResourceKey key, ResourceValue value, bool noFallback)
            {
                IResourceTable pluralsTable = value.GetTable();
                for (int i = 0; pluralsTable.GetKeyAndValue(i, key, value); ++i)
                {
                    if (key.SequenceEqual("dnam") || key.SequenceEqual("per"))
                    {
                        continue;
                    }
                    //StandardPlural plural = StandardPlural.fromString(key);
                    StandardPluralUtil.TryFromString(key, out StandardPlural plural); // ICU4N TODO: Throw here?
                    if (output.ContainsKey(plural))
                    {
                        continue;
                    }
                    string formatString = value.GetString();
                    output[plural] = formatString;
                }
            }
        }

        private static void GetMeasureData(UCultureInfo locale, MeasureUnit unit, UnitWidth? width,
                IDictionary<StandardPlural, string> output)
        {
            PluralTableSink sink = new PluralTableSink(output);
            ICUResourceBundle resource;
            resource = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.IcuUnitBaseName, locale);
            StringBuilder key = new StringBuilder();
            key.Append("units");
            if (width == UnitWidth.Narrow)
            {
                key.Append("Narrow");
            }
            else if (width == UnitWidth.Short)
            {
                key.Append("Short");
            }
            key.Append("/");
            key.Append(unit.Type);
            key.Append("/");
            key.Append(unit.Subtype);
            resource.GetAllItemsWithFallback(key.ToString(), sink);
        }

        private static void GetCurrencyLongNameData(UCultureInfo locale, Currency currency, IDictionary<StandardPlural, string> output)
        {
            // In ICU4J, this method gets a CurrencyData from CurrencyData.provider.
            // TODO(ICU4J): Implement this without going through CurrencyData, like in ICU4C?
            IDictionary<string, string> data = CurrencyData.Provider.GetInstance(locale, true).GetUnitPatterns();
            foreach (var e in data)
            {
                String pluralKeyword = e.Key;
                //StandardPlural plural = StandardPlural.fromString(e.Key);
                StandardPluralUtil.TryFromString(e.Key, out StandardPlural plural);
                String longName = currency.GetName(locale, Currency.PluralLongName, pluralKeyword, out bool _);
                String simpleFormat = e.Value;
                // Example pattern from data: "{0} {1}"
                // Example output after find-and-replace: "{0} US dollars"
                simpleFormat = simpleFormat.Replace("{1}", longName);
                // String compiled = SimpleFormatterImpl.compileToStringMinMaxArguments(simpleFormat, sb, 1, 1);
                // SimpleModifier mod = new SimpleModifier(compiled, Field.CURRENCY, false);
                output[plural] = simpleFormat;
            }
        }

        ////////////////////////
        /// END DATA LOADING ///
        ////////////////////////

        private readonly IDictionary<StandardPlural, SimpleModifier> modifiers;
        private readonly PluralRules rules;
        private readonly IMicroPropsGenerator parent;

        private LongNameHandler(IDictionary<StandardPlural, SimpleModifier> modifiers, PluralRules rules,
                IMicroPropsGenerator parent)
        {
            this.modifiers = modifiers;
            this.rules = rules;
            this.parent = parent;
        }

        public static LongNameHandler ForCurrencyLongNames(UCultureInfo locale, Currency currency, PluralRules rules,
                IMicroPropsGenerator parent)
        {
            IDictionary<StandardPlural, string> simpleFormats = new Dictionary<StandardPlural, string>();// null; // new EnumMap<StandardPlural, String>(StandardPlural.class);
            GetCurrencyLongNameData(locale, currency, simpleFormats);
            // TODO(ICU4J): Reduce the number of object creations here?
            IDictionary<StandardPlural, SimpleModifier> modifiers = new Dictionary<StandardPlural, SimpleModifier>(); // null; // new EnumMap<StandardPlural, SimpleModifier>(StandardPlural.class);
            SimpleFormatsToModifiers(simpleFormats, null, modifiers);
            return new LongNameHandler(modifiers, rules, parent);
        }

        public static LongNameHandler ForMeasureUnit(UCultureInfo locale, MeasureUnit unit, UnitWidth? width, PluralRules rules,
                IMicroPropsGenerator parent)
        {
            // ICU4N TODO: EnumMaps
            IDictionary<StandardPlural, string> simpleFormats = new Dictionary<StandardPlural, string>(); //new EnumMap<StandardPlural, string>(StandardPlural.class);
            GetMeasureData(locale, unit, width, simpleFormats);
            // TODO: What field to use for units?
            // TODO(ICU4J): Reduce the number of object creations here?
            IDictionary<StandardPlural, SimpleModifier> modifiers = new Dictionary<StandardPlural, SimpleModifier>(); //new EnumMap<StandardPlural, SimpleModifier>(StandardPlural.class);
            SimpleFormatsToModifiers(simpleFormats, null, modifiers);
            return new LongNameHandler(modifiers, rules, parent);
        }

        private static void SimpleFormatsToModifiers(IDictionary<StandardPlural, string> simpleFormats, NumberFormatField field,
                IDictionary<StandardPlural, SimpleModifier> output)
        {
            ValueStringBuilder sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                foreach (StandardPlural plural in StandardPluralUtil.Values)
                {
                    //string simpleFormat = simpleFormats.get(plural);
                    //if (!simpleFormats.TryGetValue(plural, out string simpleFormat) || simpleFormat == null)
                    //{
                    //    simpleFormat = simpleFormats.get(StandardPlural.OTHER);
                    //}
                    //if (simpleFormat == null)
                    //{
                    //    // There should always be data in the "other" plural variant.
                    //    throw new ICUException("Could not find data in 'other' plural variant with field " + field);
                    //}
                    if ((!simpleFormats.TryGetValue(plural, out string simpleFormat) || simpleFormat == null) && !simpleFormats.TryGetValue(StandardPlural.Other, out simpleFormat))
                    {
                        // There should always be data in the "other" plural variant.
                        throw new ICUException("Could not find data in 'other' plural variant with field " + field);
                    }
                    SimpleFormatterImpl.CompileToStringMinMaxArguments(simpleFormat.AsSpan(), ref sb, 1, 1);
                    string compiled = sb.AsSpan().ToString();
                    output[plural] = new SimpleModifier(compiled, null, false);
                }
            }
            finally
            {
                sb.Dispose();
            }
        }

        public virtual MicroProps ProcessQuantity(IDecimalQuantity quantity)
        {
            MicroProps micros = parent.ProcessQuantity(quantity);
            // TODO: Avoid the copy here?
            IDecimalQuantity copy = quantity.CreateCopy();
#pragma warning disable CS0618 // Type or member is obsolete
            micros.rounding.Apply(copy);
#pragma warning restore CS0618 // Type or member is obsolete
            micros.modOuter = modifiers.TryGetValue(copy.GetStandardPlural(rules), out SimpleModifier value) ? value : null;
            return micros;
        }
    }
}
