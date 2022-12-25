using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Numerics;
using ICU4N.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static ICU4N.Numerics.CompactData;
using static ICU4N.Numerics.MutablePatternModifier;
using static ICU4N.Numerics.PatternStringParser;
using static ICU4N.Text.CompactDecimalFormat;

namespace ICU4N.Numerics
{
    /// <summary>
    /// A class that defines the scientific notation style to be used when formatting numbers in <see cref="NumberFormatter"/>.
    /// <para/>
    /// This class exposes no public functionality. To create a CompactNotation, use one of the factory methods in
    /// <see cref="Notation"/>.
    /// </summary>
    /// <draft>ICU 60</draft>
    /// <provisional>This API might change or be removed in a future release.</provisional>
    /// <seealso cref="NumberFormatter"/>
    internal class CompactNotation : Notation // ICU4N TODO: API - this was public in ICU4J
    {
        internal readonly CompactStyle? compactStyle;
    internal readonly IDictionary<string, IDictionary<string, string>> compactCustomData;

        /* package-private */
        internal CompactNotation(CompactStyle compactStyle)
        {
            compactCustomData = null;
            this.compactStyle = compactStyle;
        }

        /* package-private */
        internal CompactNotation(IDictionary<string, IDictionary<string, string>> compactCustomData)
        {
            compactStyle = null;
            this.compactCustomData = compactCustomData;
        }

        /* package-private */
        internal IMicroPropsGenerator WithLocaleData(UCultureInfo locale, string nsName, CompactType compactType,
            PluralRules rules, MutablePatternModifier buildReference, IMicroPropsGenerator parent)
        {
            // TODO: Add a data cache? It would be keyed by locale, nsName, compact type, and compact style.
            return new CompactHandler(this, locale, nsName, compactType, rules, buildReference, parent);
        }

        private class CompactHandler : IMicroPropsGenerator
        {

        private class CompactModInfo
        {
            public ImmutablePatternModifier mod;
            public int numDigits;
        }

        internal readonly PluralRules rules;
            internal readonly IMicroPropsGenerator parent;
            private readonly IDictionary<string, CompactModInfo> precomputedMods;
            internal readonly CompactData data;

        internal CompactHandler(CompactNotation notation, UCultureInfo locale, string nsName, CompactType compactType,
                PluralRules rules, MutablePatternModifier buildReference, IMicroPropsGenerator parent)
        {
            this.rules = rules;
            this.parent = parent;
            this.data = new CompactData();
            if (notation.compactStyle != null)
            {
                data.Populate(locale, nsName, notation.compactStyle, compactType);
            }
            else
            {
                data.Populate(notation.compactCustomData);
            }
            if (buildReference != null)
            {
                // Safe code path
                precomputedMods = new Dictionary<string, CompactModInfo>();
                PrecomputeAllModifiers(buildReference);
            }
            else
            {
                // Unsafe code path
                precomputedMods = null;
            }
        }

        /** Used by the safe code path */
        private void PrecomputeAllModifiers(MutablePatternModifier buildReference)
        {
            ISet<string> allPatterns = new HashSet<string>();
            data.GetUniquePatterns(allPatterns);

            foreach (string patternString in allPatterns)
            {
                CompactModInfo info = new CompactModInfo();
                ParsedPatternInfo patternInfo = PatternStringParser.ParseToPatternInfo(patternString);
                buildReference.SetPatternInfo(patternInfo);
                info.mod = buildReference.CreateImmutable();
                info.numDigits = patternInfo.positive.integerTotal;
                precomputedMods[patternString] = info;
            }
        }

        public virtual MicroProps ProcessQuantity(IDecimalQuantity quantity)
        {
            MicroProps micros = parent.ProcessQuantity(quantity);
            Debug.Assert(micros.rounding != null);

            // Treat zero as if it had magnitude 0
            int magnitude;
            if (quantity.IsZero)
            {
                magnitude = 0;
                micros.rounding.Apply(quantity);
            }
            else
            {
                // TODO: Revisit chooseMultiplierAndApply
                int multiplier = micros.rounding.ChooseMultiplierAndApply(quantity, data);
                magnitude = quantity.IsZero ? 0 : quantity.GetMagnitude();
                magnitude -= multiplier;
            }

            StandardPlural plural = quantity.GetStandardPlural(rules);
            string patternString = data.GetPattern(magnitude, plural);
            //@SuppressWarnings("unused") // see #13075
            int numDigits = -1;
            if (patternString == null)
            {
                // Use the default (non-compact) modifier.
                // No need to take any action.
            }
            else if (precomputedMods != null)
            {
                // Safe code path.
                // Java uses a hash set here for O(1) lookup. C++ uses a linear search.
                CompactModInfo info = precomputedMods[patternString]; // ICU4N TODO: Use TryGetValue?
                info.mod.ApplyToMicros(micros, quantity);
                numDigits = info.numDigits;
            }
            else
            {
                // Unsafe code path.
                // Overwrite the PatternInfo in the existing modMiddle.
                Debug.Assert(micros.modMiddle is MutablePatternModifier);
                ParsedPatternInfo patternInfo = PatternStringParser.ParseToPatternInfo(patternString);
                ((MutablePatternModifier)micros.modMiddle).SetPatternInfo(patternInfo);
                numDigits = patternInfo.positive.integerTotal;
            }

            // FIXME: Deal with numDigits == 0 (Awaiting a test case)

            // We already performed rounding. Do not perform it again.
            micros.rounding = Rounder.ConstructPassThrough();

            return micros;
        }
    }
}
}
