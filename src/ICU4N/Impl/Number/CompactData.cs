using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static ICU4N.Text.CompactDecimalFormat;

namespace ICU4N.Numerics
{
    internal class CompactData : IMultiplierProducer // ICU4N TODO: API - this was public in ICU4J
    {
        public enum CompactType
        {
            Decimal, Currency
        }

        // A dummy object used when a "0" compact decimal entry is encountered. This is necessary
        // in order to prevent falling back to root. Object equality ("==") is intended.
        private static readonly string USE_FALLBACK = "<USE FALLBACK>";

        private readonly string[] patterns;
        private readonly sbyte[] multipliers;
        private byte largestMagnitude;
        private bool isEmpty;

        private const int COMPACT_MAX_DIGITS = 15;

        public CompactData()
        {
            patterns = new string[(CompactData.COMPACT_MAX_DIGITS + 1) * StandardPluralUtil.Count];
            multipliers = new sbyte[CompactData.COMPACT_MAX_DIGITS + 1];
            largestMagnitude = 0;
            isEmpty = true;
        }

        public virtual void Populate(UCultureInfo locale, string nsName, CompactStyle? compactStyle, CompactType compactType)
        {
            Debug.Assert(isEmpty);
            CompactDataSink sink = new CompactDataSink(this);
            ICUResourceBundle rb = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.IcuBaseName, locale);

            bool nsIsLatn = nsName.Equals("latn", StringComparison.Ordinal);
            bool compactIsShort = compactStyle == CompactStyle.Short;

            // Fall back to latn numbering system and/or short compact style.
            StringBuilder resourceKey = new StringBuilder();
            GetResourceBundleKey(nsName, compactStyle, compactType, resourceKey);
            rb.GetAllItemsWithFallbackNoFail(resourceKey.ToString(), sink);
            if (isEmpty && !nsIsLatn)
            {
                GetResourceBundleKey("latn", compactStyle, compactType, resourceKey);
                rb.GetAllItemsWithFallbackNoFail(resourceKey.ToString(), sink);
            }
            if (isEmpty && !compactIsShort)
            {
                GetResourceBundleKey(nsName, CompactStyle.Short, compactType, resourceKey);
                rb.GetAllItemsWithFallbackNoFail(resourceKey.ToString(), sink);
            }
            if (isEmpty && !nsIsLatn && !compactIsShort)
            {
                GetResourceBundleKey("latn", CompactStyle.Short, compactType, resourceKey);
                rb.GetAllItemsWithFallbackNoFail(resourceKey.ToString(), sink);
            }

            // The last fallback should be guaranteed to return data.
            if (isEmpty)
            {
                throw new ICUException("Could not load compact decimal data for locale " + locale);
            }
        }

        /** Produces a string like "NumberElements/latn/patternsShort/decimalFormat". */
        private static void GetResourceBundleKey(string nsName, CompactStyle? compactStyle, CompactType? compactType, StringBuilder sb)
        {
            sb.Length = 0;
            sb.Append("NumberElements/");
            sb.Append(nsName);
            sb.Append(compactStyle == CompactStyle.Short ? "/patternsShort" : "/patternsLong");
            sb.Append(compactType == CompactType.Decimal ? "/decimalFormat" : "/currencyFormat");
        }

        /** Java-only method used by CLDR tooling. */
        public virtual void Populate(IDictionary<string, IDictionary<string, string>> powersToPluralsToPatterns)
        {
            Debug.Assert(isEmpty);
            foreach (var magnitudeEntry in powersToPluralsToPatterns)
            {
                byte magnitude = (byte)(magnitudeEntry.Key.Length - 1);
                foreach (var pluralEntry in magnitudeEntry.Value)
                {
                    //StandardPlural plural = StandardPlural.FromString(pluralEntry.Key.ToString());
                    StandardPluralUtil.TryFromString(pluralEntry.Key, out StandardPlural plural); // ICU4N TODO: Throw here?
                    string patternString = pluralEntry.Value;
                    patterns[GetIndex(magnitude, plural)] = patternString;
                    int numZeros = CountZeros(patternString);
                    if (numZeros > 0)
                    { // numZeros==0 in certain cases, like Somali "Kun"
                      // Save the multiplier.
                        multipliers[magnitude] = (sbyte)(numZeros - magnitude - 1);
                        if (magnitude > largestMagnitude)
                        {
                            largestMagnitude = magnitude;
                        }
                        isEmpty = false;
                    }
                }
            }
        }

        public virtual int GetMultiplier(int magnitude)
        {
            if (magnitude < 0)
            {
                return 0;
            }
            if (magnitude > largestMagnitude)
            {
                magnitude = largestMagnitude;
            }
            return multipliers[magnitude];
        }

        public virtual string GetPattern(int magnitude, StandardPlural plural)
        {
            if (magnitude < 0)
            {
                return null;
            }
            if (magnitude > largestMagnitude)
            {
                magnitude = largestMagnitude;
            }
            string patternString = patterns[GetIndex(magnitude, plural)];
            if (patternString == null && plural != StandardPlural.Other)
            {
                // Fall back to "other" plural variant
                patternString = patterns[GetIndex(magnitude, StandardPlural.Other)];
            }
            if (patternString == USE_FALLBACK)
            { // == is intended
              // Return null if USE_FALLBACK is present
                patternString = null;
            }
            return patternString;
        }

        public virtual void GetUniquePatterns(ISet<string> output)
        {
            Debug.Assert(output.Count == 0);
            // NOTE: In C++, this is done more manually with a UVector.
            // In Java, we can take advantage of JDK HashSet.
            output.UnionWith(patterns);
            output.Remove(USE_FALLBACK);
            output.Remove(null);
        }

        private sealed class CompactDataSink : ResourceSink
        {
            private const int CharStackBufferSize = 32;

            internal CompactData data;

            public CompactDataSink(CompactData data)
            {
                this.data = data;
            }

            public override void Put(ResourceKey key, ResourceValue value, bool isRoot)
            {
                // traverse into the table of powers of ten
                IResourceTable powersOfTenTable = value.GetTable();
                for (int i3 = 0; powersOfTenTable.GetKeyAndValue(i3, key, value); ++i3)
                {

                    // Assumes that the keys are always of the form "10000" where the magnitude is the
                    // length of the key minus one.  We expect magnitudes to be less than MAX_DIGITS.
                    byte magnitude = (byte)(key.Length - 1);
                    sbyte multiplier = data.multipliers[magnitude];
                    Debug.Assert(magnitude < COMPACT_MAX_DIGITS);

                    // Iterate over the plural variants ("one", "other", etc)
                    IResourceTable pluralVariantsTable = value.GetTable();
                    for (int i4 = 0; pluralVariantsTable.GetKeyAndValue(i4, key, value); ++i4)
                    {

                        // Skip this magnitude/plural if we already have it from a child locale.
                        // Note: This also skips USE_FALLBACK entries.
                        //StandardPlural plural = StandardPluralUtil.FromString(key.ToString());
                        StandardPluralUtil.TryFromString(key, out StandardPlural plural); // ICU4N TODO: Throw here?
                        if (data.patterns[GetIndex(magnitude, plural)] != null)
                        {
                            continue;
                        }

                        // The value "0" means that we need to use the default pattern and not fall back
                        // to parent locales. Example locale where this is relevant: 'it'.
                        string patternString = value.ToString();
                        if (patternString.Equals("0", StringComparison.Ordinal))
                        {
                            patternString = USE_FALLBACK;
                        }

                        // Save the pattern string. We will parse it lazily.
                        data.patterns[GetIndex(magnitude, plural)] = patternString;

                        // If necessary, compute the multiplier: the difference between the magnitude
                        // and the number of zeros in the pattern.
                        if (multiplier == 0)
                        {
                            int numZeros = CountZeros(patternString);
                            if (numZeros > 0)
                            { // numZeros==0 in certain cases, like Somali "Kun"
                                multiplier = (sbyte)(numZeros - magnitude - 1);
                            }
                        }
                    }

                    // Save the multiplier.
                    if (data.multipliers[magnitude] == 0)
                    {
                        data.multipliers[magnitude] = multiplier;
                        if (magnitude > data.largestMagnitude)
                        {
                            data.largestMagnitude = magnitude;
                        }
                        data.isEmpty = false;
                    }
                    else
                    {
                        Debug.Assert(data.multipliers[magnitude] == multiplier);
                    }
                }
            }
        }

        private static int GetIndex(int magnitude, StandardPlural plural)
        {
            return magnitude * StandardPluralUtil.Count + (int)plural;
        }

        private static int CountZeros(string patternString)
        {
            // NOTE: This strategy for computing the number of zeros is a hack for efficiency.
            // It could break if there are any 0s that aren't part of the main pattern.
            int numZeros = 0;
            for (int i = 0; i < patternString.Length; i++)
            {
                if (patternString[i] == '0')
                {
                    numZeros++;
                }
                else if (numZeros > 0)
                {
                    break; // zeros should always be contiguous
                }
            }
            return numZeros;
        }
    }
}
