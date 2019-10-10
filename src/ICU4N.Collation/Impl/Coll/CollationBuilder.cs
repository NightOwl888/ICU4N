using ICU4N.Globalization;
using ICU4N.Support;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ICU4N.Impl.Coll
{
    public sealed class CollationBuilder : CollationRuleParser.ISink
    {
        private static readonly bool DEBUG = false;
        private sealed class BundleImporter : CollationRuleParser.IImporter
        {
            internal BundleImporter() { }

            public string GetRules(string localeID, string collationType)
            {
                return CollationLoader.LoadRules(new ULocale(localeID), collationType);
            }
        }

        public CollationBuilder(CollationTailoring b)
        {
            nfd = Normalizer2.GetNFDInstance();
            fcd = Norm2AllModes.GetFCDNormalizer2();
            nfcImpl = Norm2AllModes.GetNFCInstance().Impl;
            @base = b;
            baseData = b.Data;
            rootElements = new CollationRootElements(b.Data.rootElements);
            variableTop = 0;
            dataBuilder = new CollationDataBuilder();
            fastLatinEnabled = true;
            cesLength = 0;
            rootPrimaryIndexes = new List<int>(32);
            nodes = new List<long>(32);
            nfcImpl.EnsureCanonIterData();
            dataBuilder.InitForTailoring(baseData);
        }

        public CollationTailoring ParseAndBuild(string ruleString)
        {
            if (baseData.rootElements == null)
            {
                // C++ U_MISSING_RESOURCE_ERROR
                throw new NotSupportedException(
                        "missing root elements data, tailoring not supported");
            }
            CollationTailoring tailoring = new CollationTailoring(@base.Settings);
            CollationRuleParser parser = new CollationRuleParser(baseData);
            // Note: This always bases &[last variable] and &[first regular]
            // on the root collator's maxVariable/variableTop.
            // If we wanted this to change after [maxVariable x], then we would keep
            // the tailoring.settings pointer here and read its variableTop when we need it.
            // See http://unicode.org/cldr/trac/ticket/6070
            variableTop = @base.Settings.ReadOnly.VariableTop;
            parser.SetSink(this);
            // In Java, there is only one Importer implementation.
            // In C++, the importer is a parameter for this method.
            parser.SetImporter(new BundleImporter());
            CollationSettings ownedSettings = tailoring.Settings.CopyOnWrite();
            parser.Parse(ruleString, ownedSettings);
            if (dataBuilder.HasMappings)
            {
                MakeTailoredCEs();
                CloseOverComposites();
                FinalizeCEs();
                // Copy all of ASCII, and Latin-1 letters, into each tailoring.
                optimizeSet.Add(0, 0x7f);
                optimizeSet.Add(0xc0, 0xff);
                // Hangul is decomposed on the fly during collation,
                // and the tailoring data is always built with HANGUL_TAG specials.
                optimizeSet.Remove(Hangul.HangulBase, Hangul.HangulEnd);
                dataBuilder.Optimize(optimizeSet);
                tailoring.EnsureOwnedData();
                if (fastLatinEnabled) { dataBuilder.EnableFastLatin(); }
                dataBuilder.Build(tailoring.OwnedData);
                // C++ tailoring.builder = dataBuilder;
                dataBuilder = null;
            }
            else
            {
                tailoring.Data = baseData;
            }
            ownedSettings.FastLatinOptions = CollationFastLatin.GetOptions(
                    tailoring.Data, ownedSettings,
                    ownedSettings.FastLatinPrimaries);
            tailoring.SetRules(ruleString);
            // In Java, we do not have a rules version.
            // In C++, the genrb build tool reads and supplies one,
            // and the rulesVersion is a parameter for this method.
            tailoring.SetVersion(@base.Version, 0 /* rulesVersion */);
            return tailoring;
        }

        /// <summary>Implements <see cref="CollationRuleParser.ISink"/>.</summary>
        void CollationRuleParser.ISink.AddReset(CollationStrength strength, ICharSequence str)
        {
            Debug.Assert(str.Length != 0);
            if (str[0] == CollationRuleParser.POS_LEAD)
            {
                ces[0] = GetSpecialResetPosition(str);
                cesLength = 1;
                Debug.Assert((ces[0] & Collation.CaseAndQuaternaryMask) == 0);
            }
            else
            {
                // normal reset to a character or string
                string nfdString = nfd.Normalize(str);
                cesLength = dataBuilder.GetCEs(nfdString.ToCharSequence(), ces, 0);
                if (cesLength > Collation.MAX_EXPANSION_LENGTH)
                {
                    throw new ArgumentException(
                            "reset position maps to too many collation elements (more than 31)");
                }
            }
            if (strength == CollationStrength.Identical) { return; }  // simple reset-at-position

            // &[before strength]position
            Debug.Assert(CollationStrength.Primary <= strength && strength <= CollationStrength.Tertiary);
            int index = FindOrInsertNodeForCEs(strength);

            long node = nodes[index];
            // If the index is for a "weaker" node,
            // then skip backwards over this and further "weaker" nodes.
            while (StrengthFromNode(node) > strength)
            {
                index = PreviousIndexFromNode(node);
                node = nodes[index];
            }

            // Find or insert a node whose index we will put into a temporary CE.
            if (StrengthFromNode(node) == strength && IsTailoredNode(node))
            {
                // Reset to just before this same-strength tailored node.
                index = PreviousIndexFromNode(node);
            }
            else if (strength == CollationStrength.Primary)
            {
                // root primary node (has no previous index)
                long p = Weight32FromNode(node);
                if (p == 0)
                {
                    throw new NotSupportedException(
                            "reset primary-before ignorable not possible");
                }
                if (p <= rootElements.FirstPrimary)
                {
                    // There is no primary gap between ignorables and the space-first-primary.
                    throw new NotSupportedException(
                            "reset primary-before first non-ignorable not supported");
                }
                if (p == Collation.FIRST_TRAILING_PRIMARY)
                {
                    // We do not support tailoring to an unassigned-implicit CE.
                    throw new NotSupportedException(
                            "reset primary-before [first trailing] not supported");
                }
                p = rootElements.GetPrimaryBefore(p, baseData.IsCompressiblePrimary(p));
                index = FindOrInsertNodeForPrimary(p);
                // Go to the last node in this list:
                // Tailor after the last node between adjacent root nodes.
                for (; ; )
                {
                    node = nodes[index];
                    int nextIndex = NextIndexFromNode(node);
                    if (nextIndex == 0) { break; }
                    index = nextIndex;
                }
            }
            else
            {
                // &[before 2] or &[before 3]
                index = FindCommonNode(index, CollationStrength.Secondary);
                if (strength >= CollationStrength.Tertiary)
                {
                    index = FindCommonNode(index, CollationStrength.Tertiary);
                }
                // findCommonNode() stayed on the stronger node or moved to
                // an explicit common-weight node of the reset-before strength.
                node = nodes[index];
                if (StrengthFromNode(node) == strength)
                {
                    // Found a same-strength node with an explicit weight.
                    int weight16 = Weight16FromNode(node);
                    if (weight16 == 0)
                    {
                        throw new NotSupportedException(
                                (strength == CollationStrength.Secondary) ?
                                        "reset secondary-before secondary ignorable not possible" :
                                        "reset tertiary-before completely ignorable not possible");
                    }
                    Debug.Assert(weight16 > Collation.BEFORE_WEIGHT16);
                    // Reset to just before this node.
                    // Insert the preceding same-level explicit weight if it is not there already.
                    // Which explicit weight immediately precedes this one?
                    weight16 = GetWeight16Before(index, node, strength);
                    // Does this preceding weight have a node?
                    int previousWeight16;
                    int previousIndex = PreviousIndexFromNode(node);
                    for (int i = previousIndex; ; i = PreviousIndexFromNode(node))
                    {
                        node = nodes[i];
                        CollationStrength previousStrength = StrengthFromNode(node);
                        if (previousStrength < strength)
                        {
                            Debug.Assert(weight16 >= Collation.CommonWeight16 || i == previousIndex);
                            // Either the reset element has an above-common weight and
                            // the parent node provides the implied common weight,
                            // or the reset element has a weight<=common in the node
                            // right after the parent, and we need to insert the preceding weight.
                            previousWeight16 = Collation.CommonWeight16;
                            break;
                        }
                        else if (previousStrength == strength && !IsTailoredNode(node))
                        {
                            previousWeight16 = Weight16FromNode(node);
                            break;
                        }
                        // Skip weaker nodes and same-level tailored nodes.
                    }
                    if (previousWeight16 == weight16)
                    {
                        // The preceding weight has a node,
                        // maybe with following weaker or tailored nodes.
                        // Reset to the last of them.
                        index = previousIndex;
                    }
                    else
                    {
                        // Insert a node with the preceding weight, reset to that.
                        node = NodeFromWeight16(weight16) | NodeFromStrength(strength);
                        index = InsertNodeBetween(previousIndex, index, node);
                    }
                }
                else
                {
                    // Found a stronger node with implied strength-common weight.
                    int weight16 = GetWeight16Before(index, node, strength);
                    index = FindOrInsertWeakNode(index, weight16, strength);
                }
                // Strength of the temporary CE = strength of its reset position.
                // Code above raises an error if the before-strength is stronger.
                strength = CeStrength(ces[cesLength - 1]);
            }
            ces[cesLength - 1] = TempCEFromIndexAndStrength(index, strength);
        }

        /// <summary>
        /// Returns the secondary or tertiary weight preceding the current node's weight.
        /// node=nodes[index].
        /// </summary>
        private int GetWeight16Before(int index, long node, CollationStrength level)
        {
            Debug.Assert(StrengthFromNode(node) < level || !IsTailoredNode(node));
            // Collect the root CE weights if this node is for a root CE.
            // If it is not, then return the low non-primary boundary for a tailored CE.
            int t;
            if (StrengthFromNode(node) == CollationStrength.Tertiary)
            {
                t = Weight16FromNode(node);
            }
            else
            {
                t = Collation.CommonWeight16;  // Stronger node with implied common weight.
            }
            while (StrengthFromNode(node) > CollationStrength.Secondary)
            {
                index = PreviousIndexFromNode(node);
                node = nodes[index];
            }
            if (IsTailoredNode(node))
            {
                return Collation.BEFORE_WEIGHT16;
            }
            int s;
            if (StrengthFromNode(node) == CollationStrength.Secondary)
            {
                s = Weight16FromNode(node);
            }
            else
            {
                s = Collation.CommonWeight16;  // Stronger node with implied common weight.
            }
            while (StrengthFromNode(node) > CollationStrength.Primary)
            {
                index = PreviousIndexFromNode(node);
                node = nodes[index];
            }
            if (IsTailoredNode(node))
            {
                return Collation.BEFORE_WEIGHT16;
            }
            // [p, s, t] is a root CE. Return the preceding weight for the requested level.
            long p = Weight32FromNode(node);
            int weight16;
            if (level == CollationStrength.Secondary)
            {
                weight16 = rootElements.GetSecondaryBefore(p, s);
            }
            else
            {
                weight16 = rootElements.GetTertiaryBefore(p, s, t);
                Debug.Assert((weight16 & ~Collation.OnlyTertiaryMask) == 0);
            }
            return weight16;
        }

        private long GetSpecialResetPosition(ICharSequence str)
        {
            Debug.Assert(str.Length == 2);
            long ce;
            CollationStrength strength = CollationStrength.Primary;
            bool isBoundary = false;
            CollationRuleParser.Position pos =
                    CollationRuleParser.POSITION_VALUES[str[1] - CollationRuleParser.POS_BASE];
            switch (pos)
            {
                case CollationRuleParser.Position.FIRST_TERTIARY_IGNORABLE:
                    // Quaternary CEs are not supported.
                    // Non-zero quaternary weights are possible only on tertiary or stronger CEs.
                    return 0;
                case CollationRuleParser.Position.LAST_TERTIARY_IGNORABLE:
                    return 0;
                case CollationRuleParser.Position.FIRST_SECONDARY_IGNORABLE:
                    {
                        // Look for a tailored tertiary node after [0, 0, 0].
                        int index2 = FindOrInsertNodeForRootCE(0, CollationStrength.Tertiary);
                        long node2 = nodes[index2];
                        if ((index2 = NextIndexFromNode(node2)) != 0)
                        {
                            node2 = nodes[index2];
                            Debug.Assert(StrengthFromNode(node2) <= CollationStrength.Tertiary);
                            if (IsTailoredNode(node2) && StrengthFromNode(node2) == CollationStrength.Tertiary)
                            {
                                return TempCEFromIndexAndStrength(index2, CollationStrength.Tertiary);
                            }
                        }
                        return rootElements.FirstTertiaryCE;
                        // No need to look for nodeHasAnyBefore() on a tertiary node.
                    }
                case CollationRuleParser.Position.LAST_SECONDARY_IGNORABLE:
                    ce = rootElements.LastTertiaryCE;
                    strength = CollationStrength.Tertiary;
                    break;
                case CollationRuleParser.Position.FIRST_PRIMARY_IGNORABLE:
                    {
                        // Look for a tailored secondary node after [0, 0, *].
                        int index2 = FindOrInsertNodeForRootCE(0, CollationStrength.Secondary);
                        long node2 = nodes[index2];
                        while ((index2 = NextIndexFromNode(node2)) != 0)
                        {
                            node2 = nodes[index2];
                            strength = StrengthFromNode(node2);
                            if (strength < CollationStrength.Secondary) { break; }
                            if (strength == CollationStrength.Secondary)
                            {
                                if (IsTailoredNode(node2))
                                {
                                    if (NodeHasBefore3(node2))
                                    {
                                        index2 = NextIndexFromNode(nodes[NextIndexFromNode(node2)]);
                                        Debug.Assert(IsTailoredNode(nodes[index2]));
                                    }
                                    return TempCEFromIndexAndStrength(index2, CollationStrength.Secondary);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        ce = rootElements.FirstSecondaryCE;
                        strength = CollationStrength.Secondary;
                        break;
                    }
                case CollationRuleParser.Position.LAST_PRIMARY_IGNORABLE:
                    ce = rootElements.LastSecondaryCE;
                    strength = CollationStrength.Secondary;
                    break;
                case CollationRuleParser.Position.FIRST_VARIABLE:
                    ce = rootElements.FirstPrimaryCE;
                    isBoundary = true;  // FractionalUCA.txt: FDD1 00A0, SPACE first primary
                    break;
                case CollationRuleParser.Position.LAST_VARIABLE:
                    ce = rootElements.LastCEWithPrimaryBefore(variableTop + 1);
                    break;
                case CollationRuleParser.Position.FIRST_REGULAR:
                    ce = rootElements.FirstCEWithPrimaryAtLeast(variableTop + 1);
                    isBoundary = true;  // FractionalUCA.txt: FDD1 263A, SYMBOL first primary
                    break;
                case CollationRuleParser.Position.LAST_REGULAR:
                    // Use the Hani-first-primary rather than the actual last "regular" CE before it,
                    // for backward compatibility with behavior before the introduction of
                    // script-first-primary CEs in the root collator.
                    ce = rootElements.FirstCEWithPrimaryAtLeast(
                        baseData.GetFirstPrimaryForGroup(UScript.Han));
                    break;
                case CollationRuleParser.Position.FIRST_IMPLICIT:
                    ce = baseData.GetSingleCE(0x4e00);
                    break;
                case CollationRuleParser.Position.LAST_IMPLICIT:
                    // We do not support tailoring to an unassigned-implicit CE.
                    throw new NotSupportedException(
                            "reset to [last implicit] not supported");
                case CollationRuleParser.Position.FIRST_TRAILING:
                    ce = Collation.MakeCE(Collation.FIRST_TRAILING_PRIMARY);
                    isBoundary = true;  // trailing first primary (there is no mapping for it)
                    break;
                case CollationRuleParser.Position.LAST_TRAILING:
                    throw new ArgumentException("LDML forbids tailoring to U+FFFF");
                default:
                    Debug.Assert(false);
                    return 0;
            }

            int index = FindOrInsertNodeForRootCE(ce, strength);
            long node = nodes[index];
            if (((int)pos & 1) == 0)
            {
                // even pos = [first xyz]
                if (!NodeHasAnyBefore(node) && isBoundary)
                {
                    // A <group> first primary boundary is artificially added to FractionalUCA.txt.
                    // It is reachable via its special contraction, but is not normally used.
                    // Find the first character tailored after the boundary CE,
                    // or the first real root CE after it.
                    if ((index = NextIndexFromNode(node)) != 0)
                    {
                        // If there is a following node, then it must be tailored
                        // because there are no root CEs with a boundary primary
                        // and non-common secondary/tertiary weights.
                        node = nodes[index];
                        Debug.Assert(IsTailoredNode(node));
                        ce = TempCEFromIndexAndStrength(index, strength);
                    }
                    else
                    {
                        Debug.Assert(strength == CollationStrength.Primary);
                        long p = ce.TripleShift( 32);
                        int pIndex = rootElements.FindPrimary(p);
                        bool isCompressible = baseData.IsCompressiblePrimary(p);
                        p = rootElements.GetPrimaryAfter(p, pIndex, isCompressible);
                        ce = Collation.MakeCE(p);
                        index = FindOrInsertNodeForRootCE(ce, CollationStrength.Primary);
                        node = nodes[index];
                    }
                }
                if (NodeHasAnyBefore(node))
                {
                    // Get the first node that was tailored before this one at a weaker strength.
                    if (NodeHasBefore2(node))
                    {
                        index = NextIndexFromNode(nodes[NextIndexFromNode(node)]);
                        node = nodes[index];
                    }
                    if (NodeHasBefore3(node))
                    {
                        index = NextIndexFromNode(nodes[NextIndexFromNode(node)]);
                    }
                    Debug.Assert(IsTailoredNode(nodes[index]));
                    ce = TempCEFromIndexAndStrength(index, strength);
                }
            }
            else
            {
                // odd pos = [last xyz]
                // Find the last node that was tailored after the [last xyz]
                // at a strength no greater than the position's strength.
                for (; ; )
                {
                    int nextIndex = NextIndexFromNode(node);
                    if (nextIndex == 0) { break; }
                    long nextNode = nodes[nextIndex];
                    if (StrengthFromNode(nextNode) < strength) { break; }
                    index = nextIndex;
                    node = nextNode;
                }
                // Do not make a temporary CE for a root node.
                // This last node might be the node for the root CE itself,
                // or a node with a common secondary or tertiary weight.
                if (IsTailoredNode(node))
                {
                    ce = TempCEFromIndexAndStrength(index, strength);
                }
            }
            return ce;
        }

        /// <summary>Implements <see cref="CollationRuleParser.ISink"/>.</summary>
        void CollationRuleParser.ISink.AddRelation(CollationStrength strength, ICharSequence prefix, ICharSequence str, string extension) // ICU4N specific - changed extension from ICharSequence to string
        {
            StringCharSequence nfdPrefix;
            if (prefix.Length == 0)
            {
                nfdPrefix = new StringCharSequence("");
            }
            else
            {
                nfdPrefix = new StringCharSequence(nfd.Normalize(prefix));
            }
            StringCharSequence nfdString = new StringCharSequence(nfd.Normalize(str));

            // The runtime code decomposes Hangul syllables on the fly,
            // with recursive processing but without making the Jamo pieces visible for matching.
            // It does not work with certain types of contextual mappings.
            int nfdLength = nfdString.Length;
            if (nfdLength >= 2)
            {
                char c = nfdString[0];
                if (Hangul.IsJamoL(c) || Hangul.IsJamoV(c))
                {
                    // While handling a Hangul syllable, contractions starting with Jamo L or V
                    // would not see the following Jamo of that syllable.
                    throw new NotSupportedException(
                            "contractions starting with conjoining Jamo L or V not supported");
                }
                c = nfdString[nfdLength - 1];
                if (Hangul.IsJamoL(c) ||
                        (Hangul.IsJamoV(c) && Hangul.IsJamoL(nfdString[nfdLength - 2])))
                {
                    // A contraction ending with Jamo L or L+V would require
                    // generating Hangul syllables in addTailComposites() (588 for a Jamo L),
                    // or decomposing a following Hangul syllable on the fly, during contraction matching.
                    throw new NotSupportedException(
                            "contractions ending with conjoining Jamo L or L+V not supported");
                }
                // A Hangul syllable completely inside a contraction is ok.
            }
            // Note: If there is a prefix, then the parser checked that
            // both the prefix and the string beging with NFC boundaries (not Jamo V or T).
            // Therefore: prefix.isEmpty() || !isJamoVOrT(nfdString.charAt(0))
            // (While handling a Hangul syllable, prefixes on Jamo V or T
            // would not see the previous Jamo of that syllable.)

            if (strength != CollationStrength.Identical)
            {
                // Find the node index after which we insert the new tailored node.
                int index = FindOrInsertNodeForCEs(strength);
                Debug.Assert(cesLength > 0);
                long ce = ces[cesLength - 1];
                if (strength == CollationStrength.Primary && !IsTempCE(ce) && (ce.TripleShift(32)) == 0)
                {
                    // There is no primary gap between ignorables and the space-first-primary.
                    throw new NotSupportedException(
                            "tailoring primary after ignorables not supported");
                }
                if (strength == CollationStrength.Quaternary && ce == 0)
                {
                    // The CE data structure does not support non-zero quaternary weights
                    // on tertiary ignorables.
                    throw new NotSupportedException(
                            "tailoring quaternary after tertiary ignorables not supported");
                }
                // Insert the new tailored node.
                index = InsertTailoredNodeAfter(index, strength);
                // Strength of the temporary CE:
                // The new relation may yield a stronger CE but not a weaker one.
                CollationStrength tempStrength = CeStrength(ce);
                if (strength < tempStrength) { tempStrength = strength; }
                ces[cesLength - 1] = TempCEFromIndexAndStrength(index, tempStrength);
            }

            SetCaseBits(nfdString);

            int cesLengthBeforeExtension = cesLength;
            if (extension.Length != 0)
            {
                string nfdExtension = nfd.Normalize(extension);
                cesLength = dataBuilder.GetCEs(nfdExtension.ToCharSequence(), ces, cesLength);
                if (cesLength > Collation.MAX_EXPANSION_LENGTH)
                {
                    throw new ArgumentException(
                            "extension string adds too many collation elements (more than 31 total)");
                }
            }
            int ce32 = Collation.UNASSIGNED_CE32;
            if ((!nfdPrefix.String.ContentEquals(prefix) || !nfdString.String.ContentEquals(str)) &&
                    !IgnorePrefix(prefix) && !IgnoreString(str))
            {
                // Map from the original input to the CEs.
                // We do this in case the canonical closure is incomplete,
                // so that it is possible to explicitly provide the missing mappings.
                ce32 = AddIfDifferent(prefix, str, ces, cesLength, ce32);
            }
            AddWithClosure(nfdPrefix, nfdString, ces, cesLength, ce32);
            cesLength = cesLengthBeforeExtension;
        }

        /// <summary>
        /// Picks one of the current CEs and finds or inserts a node in the graph
        /// for the CE + strength.
        /// </summary>
        private int FindOrInsertNodeForCEs(CollationStrength strength)
        {
            Debug.Assert(CollationStrength.Primary <= strength && strength <= CollationStrength.Quaternary);

            // Find the last CE that is at least as "strong" as the requested difference.
            // Note: Stronger is smaller (Collator.PRIMARY=0).
            long ce;
            for (; ; --cesLength)
            {
                if (cesLength == 0)
                {
                    ce = ces[0] = 0;
                    cesLength = 1;
                    break;
                }
                else
                {
                    ce = ces[cesLength - 1];
                }
                if (CeStrength(ce) <= strength) { break; }
            }

            if (IsTempCE(ce))
            {
                // No need to findCommonNode() here for lower levels
                // because insertTailoredNodeAfter() will do that anyway.
                return IndexFromTempCE(ce);
            }

            // root CE
            if ((int)(ce.TripleShift(56)) == Collation.UNASSIGNED_IMPLICIT_BYTE)
            {
                throw new NotSupportedException(
                        "tailoring relative to an unassigned code point not supported");
            }
            return FindOrInsertNodeForRootCE(ce, strength);
        }

        private int FindOrInsertNodeForRootCE(long ce, CollationStrength strength)
        {
            Debug.Assert((int)(ce.TripleShift(56)) != Collation.UNASSIGNED_IMPLICIT_BYTE);

            // Find or insert the node for each of the root CE's weights,
            // down to the requested level/strength.
            // Root CEs must have common=zero quaternary weights (for which we never insert any nodes).
            Debug.Assert((ce & 0xc0) == 0);
            int index = FindOrInsertNodeForPrimary(ce.TripleShift(32));
            if (strength >= CollationStrength.Secondary)
            {
                int lower32 = (int)ce;
                index = FindOrInsertWeakNode(index, lower32.TripleShift(16), CollationStrength.Secondary);
                if (strength >= CollationStrength.Tertiary)
                {
                    index = FindOrInsertWeakNode(index, lower32 & Collation.OnlyTertiaryMask,
                                                CollationStrength.Tertiary);
                }
            }
            return index;
        }

        /// <summary>
        /// Like Java Collections.binarySearch(List, key, Comparator).
        /// </summary>
        /// <returns>The index>=0 where the item was found,
        /// or the index&lt;0 for inserting the string at ~index in sorted order
        /// (index into rootPrimaryIndexes)</returns>
        private static int BinarySearchForRootPrimaryNode(
                IList<int> rootPrimaryIndexes, int length, IList<long> nodes, long p)
        {
            if (length == 0) { return ~0; }
            int start = 0;
            int limit = length;
            for (; ; )
            {
                int i = (int)(((long)start + (long)limit) / 2);
                long node = nodes[rootPrimaryIndexes[i]];
                long nodePrimary = node.TripleShift( 32);  // weight32FromNode(node)
                if (p == nodePrimary)
                {
                    return i;
                }
                else if (p < nodePrimary)
                {
                    if (i == start)
                    {
                        return ~start;  // insert s before i
                    }
                    limit = i;
                }
                else
                {
                    if (i == start)
                    {
                        return ~(start + 1);  // insert s after i
                    }
                    start = i;
                }
            }
        }

        /// <summary>Finds or inserts the node for a root CE's primary weight.</summary>
        private int FindOrInsertNodeForPrimary(long p)
        {
            int rootIndex = BinarySearchForRootPrimaryNode(
                rootPrimaryIndexes, rootPrimaryIndexes.Count, nodes, p);
            if (rootIndex >= 0)
            {
                return rootPrimaryIndexes[rootIndex];
            }
            else
            {
                // Start a new list of nodes with this primary.
                int index = nodes.Count;
                nodes.Add(NodeFromWeight32(p));
                rootPrimaryIndexes.Insert(~rootIndex, index);
                return index;
            }
        }

        /// <summary>Finds or inserts the node for a secondary or tertiary weight.</summary>
        private int FindOrInsertWeakNode(int index, int weight16, CollationStrength level)
        {
            Debug.Assert(0 <= index && index < nodes.Count);
            Debug.Assert(CollationStrength.Secondary <= level && level <= CollationStrength.Tertiary);

            if (weight16 == Collation.CommonWeight16)
            {
                return FindCommonNode(index, level);
            }

            // If this will be the first below-common weight for the parent node,
            // then we will also need to insert a common weight after it.
            long node = nodes[index];
            Debug.Assert(StrengthFromNode(node) < level);  // parent node is stronger
            if (weight16 != 0 && weight16 < Collation.CommonWeight16)
            {
                int hasThisLevelBefore = level == CollationStrength.Secondary ? HAS_BEFORE2 : HAS_BEFORE3;
                if ((node & hasThisLevelBefore) == 0)
                {
                    // The parent node has an implied level-common weight.
                    long commonNode =
                        NodeFromWeight16(Collation.CommonWeight16) | NodeFromStrength(level);
                    if (level == CollationStrength.Secondary)
                    {
                        // Move the HAS_BEFORE3 flag from the parent node
                        // to the new secondary common node.
                        commonNode |= node & HAS_BEFORE3;
                        node &= ~(long)HAS_BEFORE3;
                    }
                    nodes[index] = (node | (uint)hasThisLevelBefore);
                    // Insert below-common-weight node.
                    int nextIndex2 = NextIndexFromNode(node);
                    node = NodeFromWeight16(weight16) | NodeFromStrength(level);
                    index = InsertNodeBetween(index, nextIndex2, node);
                    // Insert common-weight node.
                    InsertNodeBetween(index, nextIndex2, commonNode);
                    // Return index of below-common-weight node.
                    return index;
                }
            }

            // Find the root CE's weight for this level.
            // Postpone insertion if not found:
            // Insert the new root node before the next stronger node,
            // or before the next root node with the same strength and a larger weight.
            int nextIndex;
            while ((nextIndex = NextIndexFromNode(node)) != 0)
            {
                node = nodes[nextIndex];
                CollationStrength nextStrength = StrengthFromNode(node);
                if (nextStrength <= level)
                {
                    // Insert before a stronger node.
                    if (nextStrength < level) { break; }
                    // nextStrength == level
                    if (!IsTailoredNode(node))
                    {
                        int nextWeight16 = Weight16FromNode(node);
                        if (nextWeight16 == weight16)
                        {
                            // Found the node for the root CE up to this level.
                            return nextIndex;
                        }
                        // Insert before a node with a larger same-strength weight.
                        if (nextWeight16 > weight16) { break; }
                    }
                }
                // Skip the next node.
                index = nextIndex;
            }
            node = NodeFromWeight16(weight16) | NodeFromStrength(level);
            return InsertNodeBetween(index, nextIndex, node);
        }

        /// <summary>
        /// Makes and inserts a new tailored node into the list, after the one at <paramref name="index"/>.
        /// Skips over nodes of weaker strength to maintain collation order
        /// ("postpone insertion").
        /// </summary>
        /// <returns>The new node's index.</returns>
        private int InsertTailoredNodeAfter(int index, CollationStrength strength)
        {
            Debug.Assert(0 <= index && index < nodes.Count);
            if (strength >= CollationStrength.Secondary)
            {
                index = FindCommonNode(index, CollationStrength.Secondary);
                if (strength >= CollationStrength.Tertiary)
                {
                    index = FindCommonNode(index, CollationStrength.Tertiary);
                }
            }
            // Postpone insertion:
            // Insert the new node before the next one with a strength at least as strong.
            long node = nodes[index];
            int nextIndex;
            while ((nextIndex = NextIndexFromNode(node)) != 0)
            {
                node = nodes[nextIndex];
                if (StrengthFromNode(node) <= strength) { break; }
                // Skip the next node which has a weaker (larger) strength than the new one.
                index = nextIndex;
            }
            node = IS_TAILORED | NodeFromStrength(strength);
            return InsertNodeBetween(index, nextIndex, node);
        }

        /// <summary>
        /// Inserts a new node into the list, between list-adjacent items.
        /// The node's previous and next indexes must not be set yet.
        /// </summary>
        /// <returns>The new node's index.</returns>
        private int InsertNodeBetween(int index, int nextIndex, long node)
        {
            Debug.Assert(PreviousIndexFromNode(node) == 0);
            Debug.Assert(NextIndexFromNode(node) == 0);
            Debug.Assert(NextIndexFromNode(nodes[index]) == nextIndex);
            // Append the new node and link it to the existing nodes.
            int newIndex = nodes.Count;
            node |= NodeFromPreviousIndex(index) | NodeFromNextIndex(nextIndex);
            nodes.Add(node);
            // nodes[index].nextIndex = newIndex
            node = nodes[index];
            nodes[index] = ChangeNodeNextIndex(node, newIndex);
            // nodes[nextIndex].previousIndex = newIndex
            if (nextIndex != 0)
            {
                node = nodes[nextIndex];
                nodes[nextIndex] = ChangeNodePreviousIndex(node, newIndex);
            }
            return newIndex;
        }

        /// <summary>
        /// Finds the node which implies or contains a common=05 weight of the given <paramref name="strength"/>
        /// (secondary or tertiary), if the current node is stronger.
        /// Skips weaker nodes and tailored nodes if the current node is stronger
        /// and is followed by an explicit-common-weight node.
        /// Always returns the input <paramref name="index"/> if that node is no stronger than the given <paramref name="strength"/>.
        /// </summary>
        private int FindCommonNode(int index, CollationStrength strength)
        {
            Debug.Assert(CollationStrength.Secondary <= strength && strength <= CollationStrength.Tertiary);
            long node = nodes[index];
            if (StrengthFromNode(node) >= strength)
            {
                // The current node is no stronger.
                return index;
            }
            if (strength == CollationStrength.Secondary ? !NodeHasBefore2(node) : !NodeHasBefore3(node))
            {
                // The current node implies the strength-common weight.
                return index;
            }
            index = NextIndexFromNode(node);
            node = nodes[index];
            Debug.Assert(!IsTailoredNode(node) && StrengthFromNode(node) == strength &&
            Weight16FromNode(node) < Collation.CommonWeight16);
            // Skip to the explicit common node.
            do
            {
                index = NextIndexFromNode(node);
                node = nodes[index];
                Debug.Assert(StrengthFromNode(node) >= strength);
            } while (IsTailoredNode(node) || StrengthFromNode(node) > strength ||
                    Weight16FromNode(node) < Collation.CommonWeight16);
            Debug.Assert(Weight16FromNode(node) == Collation.CommonWeight16);
            return index;
        }

        private void SetCaseBits(ICharSequence nfdString)
        {
            int numTailoredPrimaries = 0;
            for (int i = 0; i < cesLength; ++i)
            {
                if (CeStrength(ces[i]) == CollationStrength.Primary) { ++numTailoredPrimaries; }
            }
            // We should not be able to get too many case bits because
            // cesLength<=31==MAX_EXPANSION_LENGTH.
            // 31 pairs of case bits fit into an long without setting its sign bit.
            Debug.Assert(numTailoredPrimaries <= 31);

            long cases = 0;
            if (numTailoredPrimaries > 0)
            {
                ICharSequence s = nfdString;
                UTF16CollationIterator baseCEs = new UTF16CollationIterator(baseData, false, s, 0);
                int baseCEsLength = baseCEs.FetchCEs() - 1;
                Debug.Assert(baseCEsLength >= 0 && baseCEs.GetCE(baseCEsLength) == Collation.NoCE);

                int lastCase = 0;
                int numBasePrimaries = 0;
                for (int i = 0; i < baseCEsLength; ++i)
                {
                    long ce = baseCEs.GetCE(i);
                    if ((ce.TripleShift(32)) != 0)
                    {
                        ++numBasePrimaries;
                        int c = ((int)ce >> 14) & 3;
                        Debug.Assert(c == 0 || c == 2);  // lowercase or uppercase, no mixed case in any base CE
                        if (numBasePrimaries < numTailoredPrimaries)
                        {
                            cases |= (long)c << ((numBasePrimaries - 1) * 2);
                        }
                        else if (numBasePrimaries == numTailoredPrimaries)
                        {
                            lastCase = c;
                        }
                        else if (c != lastCase)
                        {
                            // There are more base primary CEs than tailored primaries.
                            // Set mixed case if the case bits of the remainder differ.
                            lastCase = 1;
                            // Nothing more can change.
                            break;
                        }
                    }
                }
                if (numBasePrimaries >= numTailoredPrimaries)
                {
                    cases |= (long)lastCase << ((numTailoredPrimaries - 1) * 2);
                }
            }

            for (int i = 0; i < cesLength; ++i)
            {
                long ce = (long)((ulong)ces[i] & 0xffffffffffff3fffL);  // clear old case bits
                CollationStrength strength = CeStrength(ce);
                if (strength == CollationStrength.Primary)
                {
                    ce |= (cases & 3) << 14;
                    //cases >>>= 2;
                    cases = cases.TripleShift(2);
                }
                else if (strength == CollationStrength.Tertiary)
                {
                    // Tertiary CEs must have uppercase bits.
                    // See the LDML spec, and comments in class CollationCompare.
                    ce |= 0x8000;
                }
                // Tertiary ignorable CEs must have 0 case bits.
                // We set 0 case bits for secondary CEs too
                // since currently only U+0345 is cased and maps to a secondary CE,
                // and it is lowercase. Other secondaries are uncased.
                // See [[:Cased:]&[:uca1=:]] where uca1 queries the root primary weight.
                ces[i] = ce;
            }
        }

        /// <summary>Implements <see cref="CollationRuleParser.ISink"/>.</summary>
        void CollationRuleParser.ISink.SuppressContractions(UnicodeSet set)
        {
            dataBuilder.SuppressContractions(set);
        }

        /// <summary>Implements <see cref="CollationRuleParser.ISink"/>.</summary>
        void CollationRuleParser.ISink.Optimize(UnicodeSet set)
        {
            optimizeSet.AddAll(set);
        }

        /// <summary>
        /// Adds the mapping and its canonical closure.
        /// Takes ce32=dataBuilder.EncodeCEs(...) so that the data builder
        /// need not re-encode the CEs multiple times.
        /// </summary>
        private int AddWithClosure(ICharSequence nfdPrefix, ICharSequence nfdString,
                    long[] newCEs, int newCEsLength, int ce32)
        {
            // Map from the NFD input to the CEs.
            ce32 = AddIfDifferent(nfdPrefix, nfdString, newCEs, newCEsLength, ce32);
            ce32 = AddOnlyClosure(nfdPrefix, nfdString, newCEs, newCEsLength, ce32);
            AddTailComposites(nfdPrefix, nfdString);
            return ce32;
        }

        private int AddOnlyClosure(ICharSequence nfdPrefix, ICharSequence nfdString,
            long[] newCEs, int newCEsLength, int ce32)
        {
            // Map from canonically equivalent input to the CEs. (But not from the all-NFD input.)
            // TODO: make CanonicalIterator work with CharSequence, or maybe change arguments here to String
            if (nfdPrefix.Length == 0)
            {
                CanonicalIterator stringIter = new CanonicalIterator(nfdString.ToString());
                ICharSequence prefix = new StringCharSequence("");
                for (; ; )
                {
                    string str = stringIter.Next();
                    if (str == null) { break; }
                    if (IgnoreString(str) || str.ContentEquals(nfdString)) { continue; }
                    ce32 = AddIfDifferent(prefix, str.ToCharSequence(), newCEs, newCEsLength, ce32);
                }
            }
            else
            {
                CanonicalIterator prefixIter = new CanonicalIterator(nfdPrefix.ToString());
                CanonicalIterator stringIter = new CanonicalIterator(nfdString.ToString());
                for (; ; )
                {
                    string prefix = prefixIter.Next();
                    if (prefix == null) { break; }
                    if (IgnorePrefix(prefix)) { continue; }
                    bool samePrefix = prefix.ContentEquals(nfdPrefix);
                    ICharSequence prefixCharSequence = prefix.ToCharSequence();
                    for (; ; )
                    {
                        string str = stringIter.Next();
                        if (str == null) { break; }
                        if (IgnoreString(str) || (samePrefix && str.ContentEquals(nfdString))) { continue; }
                        ce32 = AddIfDifferent(prefixCharSequence, str.ToCharSequence(), newCEs, newCEsLength, ce32);
                    }
                    stringIter.Reset();
                }
            }
            return ce32;
        }

        private void AddTailComposites(ICharSequence nfdPrefix, ICharSequence nfdString)
        {
            // Look for the last starter in the NFD string.
            int lastStarter;
            int indexAfterLastStarter = nfdString.Length;
            for (; ; )
            {
                if (indexAfterLastStarter == 0) { return; }  // no starter at all
                lastStarter = Character.CodePointBefore(nfdString, indexAfterLastStarter);
                if (nfd.GetCombiningClass(lastStarter) == 0) { break; }
                indexAfterLastStarter -= Character.CharCount(lastStarter);
            }
            // No closure to Hangul syllables since we decompose them on the fly.
            if (Hangul.IsJamoL(lastStarter)) { return; }

            // Are there any composites whose decomposition starts with the lastStarter?
            // Note: Normalizer2Impl does not currently return start sets for NFC_QC=Maybe characters.
            // We might find some more equivalent mappings here if it did.
            UnicodeSet composites = new UnicodeSet();
            if (!nfcImpl.GetCanonStartSet(lastStarter, composites)) { return; }

            StringBuilderCharSequence newNFDString = new StringBuilderCharSequence(), newString = new StringBuilderCharSequence();
            long[] newCEs = new long[Collation.MAX_EXPANSION_LENGTH];
            UnicodeSetIterator iter = new UnicodeSetIterator(composites);
            while (iter.Next())
            {
                Debug.Assert(iter.Codepoint != UnicodeSetIterator.IsString);
                int composite = iter.Codepoint;
                string decomp = nfd.GetDecomposition(composite);
                if (!MergeCompositeIntoString(nfdString, indexAfterLastStarter, composite, decomp,
                        newNFDString.StringBuilder, newString.StringBuilder))
                {
                    continue;
                }
                int newCEsLength = dataBuilder.GetCEs(nfdPrefix, newNFDString, newCEs, 0);
                if (newCEsLength > Collation.MAX_EXPANSION_LENGTH)
                {
                    // Ignore mappings that we cannot store.
                    continue;
                }
                // Note: It is possible that the newCEs do not make use of the mapping
                // for which we are adding the tail composites, in which case we might be adding
                // unnecessary mappings.
                // For example, when we add tail composites for ae^ (^=combining circumflex),
                // UCA discontiguous-contraction matching does not find any matches
                // for ae_^ (_=any combining diacritic below) *unless* there is also
                // a contraction mapping for ae.
                // Thus, if there is no ae contraction, then the ae^ mapping is ignored
                // while fetching the newCEs for ae_^.
                // TODO: Try to detect this effectively.
                // (Alternatively, print a warning when prefix contractions are missing.)

                // We do not need an explicit mapping for the NFD strings.
                // It is fine if the NFD input collates like this via a sequence of mappings.
                // It also saves a little bit of space, and may reduce the set of characters with contractions.
                int ce32 = AddIfDifferent(nfdPrefix, newString,
                                              newCEs, newCEsLength, Collation.UNASSIGNED_CE32);
                if (ce32 != Collation.UNASSIGNED_CE32)
                {
                    // was different, was added
                    AddOnlyClosure(nfdPrefix, newNFDString, newCEs, newCEsLength, ce32);
                }
            }
        }

        private bool MergeCompositeIntoString(ICharSequence nfdString, int indexAfterLastStarter,
                    int composite, string decomp,
                    StringBuilder newNFDString, StringBuilder newString) // ICU4N specific - changed decomp from ICharSequence to string
        {
            Debug.Assert(Character.CodePointBefore(nfdString, indexAfterLastStarter) ==
            Character.CodePointAt(decomp, 0));
            int lastStarterLength = Character.OffsetByCodePoints(decomp, 0, 1);
            if (lastStarterLength == decomp.Length)
            {
                // Singleton decompositions should be found by addWithClosure()
                // and the CanonicalIterator, so we can ignore them here.
                return false;
            }
            if (EqualSubSequences(nfdString, indexAfterLastStarter, decomp, lastStarterLength))
            {
                // same strings, nothing new to be found here
                return false;
            }

            // Make new FCD strings that combine a composite, or its decomposition,
            // into the nfdString's last starter and the combining marks following it.
            // Make an NFD version, and a version with the composite.
            newNFDString.Length = 0;
            newNFDString.Append(nfdString, 0, indexAfterLastStarter - 0); // ICU4N: Checked 3rd parameter
            newString.Length = 0;
            newString.Append(nfdString, 0, (indexAfterLastStarter - lastStarterLength) - 0) // ICU4N: Checked 3rd parameter
                .AppendCodePoint(composite);

            // The following is related to discontiguous contraction matching,
            // but builds only FCD strings (or else returns false).
            int sourceIndex = indexAfterLastStarter;
            int decompIndex = lastStarterLength;
            // Small optimization: We keep the source character across loop iterations
            // because we do not always consume it,
            // and then need not fetch it again nor look up its combining class again.
            int sourceChar = Collation.SentinelCodePoint;
            // The cc variables need to be declared before the loop so that at the end
            // they are set to the last combining classes seen.
            int sourceCC = 0;
            int decompCC = 0;
            for (; ; )
            {
                if (sourceChar < 0)
                {
                    if (sourceIndex >= nfdString.Length) { break; }
                    sourceChar = Character.CodePointAt(nfdString, sourceIndex);
                    sourceCC = nfd.GetCombiningClass(sourceChar);
                    Debug.Assert(sourceCC != 0);
                }
                // We consume a decomposition character in each iteration.
                if (decompIndex >= decomp.Length) { break; }
                int decompChar = Character.CodePointAt(decomp, decompIndex);
                decompCC = nfd.GetCombiningClass(decompChar);
                // Compare the two characters and their combining classes.
                if (decompCC == 0)
                {
                    // Unable to merge because the source contains a non-zero combining mark
                    // but the composite's decomposition contains another starter.
                    // The strings would not be equivalent.
                    return false;
                }
                else if (sourceCC < decompCC)
                {
                    // Composite + sourceChar would not be FCD.
                    return false;
                }
                else if (decompCC < sourceCC)
                {
                    newNFDString.AppendCodePoint(decompChar);
                    decompIndex += Character.CharCount(decompChar);
                }
                else if (decompChar != sourceChar)
                {
                    // Blocked because same combining class.
                    return false;
                }
                else
                {  // match: decompChar == sourceChar
                    newNFDString.AppendCodePoint(decompChar);
                    decompIndex += Character.CharCount(decompChar);
                    sourceIndex += Character.CharCount(decompChar);
                    sourceChar = Collation.SentinelCodePoint;
                }
            }
            // We are at the end of at least one of the two inputs.
            if (sourceChar >= 0)
            {  // more characters from nfdString but not from decomp
                if (sourceCC < decompCC)
                {
                    // Appending the next source character to the composite would not be FCD.
                    return false;
                }
                newNFDString.Append(nfdString, sourceIndex, nfdString.Length - sourceIndex); // ICU4N: Corrected 3rd parameter
                newString.Append(nfdString, sourceIndex, nfdString.Length - sourceIndex); // ICU4N: Corrected 3rd parameter
            }
            else if (decompIndex < decomp.Length)
            {  // more characters from decomp, not from nfdString
                newNFDString.Append(decomp, decompIndex, decomp.Length - decompIndex); // ICU4N: Corrected 3rd parameter
            }
            Debug.Assert(nfd.IsNormalized(newNFDString));
            Debug.Assert(fcd.IsNormalized(newString));
            Debug.Assert(nfd.Normalize(newString).Equals(newNFDString.ToString()));  // canonically equivalent
            return true;
        }

        private bool EqualSubSequences(ICharSequence left, int leftStart, string right, int rightStart) // ICU4N specific - changed right from ICharSequence to string
        {
            // C++ UnicodeString::compare(leftStart, 0x7fffffff, right, rightStart, 0x7fffffff) == 0
            int leftLength = left.Length;
            if ((leftLength - leftStart) != (right.Length - rightStart)) { return false; }
            while (leftStart < leftLength)
            {
                if (left[leftStart++] != right[rightStart++])
                {
                    return false;
                }
            }
            return true;
        }
        // ICU4N specific overload
        private bool IgnorePrefix(string s)
        {
            // Do not map non-FCD prefixes.
            return !IsFCD(s);
        }
        private bool IgnorePrefix(ICharSequence s) 
        {
            // Do not map non-FCD prefixes.
            return !IsFCD(s);
        }
        // ICU4N specific overload
        private bool IgnoreString(string s) 
        {
            // Do not map non-FCD strings.
            // Do not map strings that start with Hangul syllables: We decompose those on the fly.
            return !IsFCD(s) || Hangul.IsHangul(s[0]);
        }
        private bool IgnoreString(ICharSequence s)
        {
            // Do not map non-FCD strings.
            // Do not map strings that start with Hangul syllables: We decompose those on the fly.
            return !IsFCD(s) || Hangul.IsHangul(s[0]);
        }
        // ICU4N specific overload
        private bool IsFCD(string s)
        {
            return fcd.IsNormalized(s);
        }
        private bool IsFCD(ICharSequence s)
        {
            return fcd.IsNormalized(s);
        }

        private static readonly UnicodeSet COMPOSITES = new UnicodeSet("[:NFD_QC=N:]");
        static CollationBuilder()
        {
            // Hangul is decomposed on the fly during collation.
            COMPOSITES.Remove(Hangul.HangulBase, Hangul.HangulEnd);
        }

        private void CloseOverComposites()
        {
            ICharSequence prefix = new StringCharSequence("");  // empty
            UnicodeSetIterator iter = new UnicodeSetIterator(COMPOSITES);
            // ICU4N: reusable ICharSequence wrapper to pass strings as ICharSequence 
            StringCharSequence wrapper = new StringCharSequence("");
            while (iter.Next())
            {
                Debug.Assert(iter.Codepoint != UnicodeSetIterator.IsString);
                string nfdString = nfd.GetDecomposition(iter.Codepoint);
                wrapper.String = nfdString;
                cesLength = dataBuilder.GetCEs(wrapper, ces, 0);
                if (cesLength > Collation.MAX_EXPANSION_LENGTH)
                {
                    // Too many CEs from the decomposition (unusual), ignore this composite.
                    // We could add a capacity parameter to getCEs() and reallocate if necessary.
                    // However, this can only really happen in contrived cases.
                    continue;
                }
                string composite = iter.GetString();
                wrapper.String = composite;
                AddIfDifferent(prefix, wrapper, ces, cesLength, Collation.UNASSIGNED_CE32);
            }
        }

        private int AddIfDifferent(ICharSequence prefix, ICharSequence str,
                    long[] newCEs, int newCEsLength, int ce32)
        {
            long[] oldCEs = new long[Collation.MAX_EXPANSION_LENGTH];
            int oldCEsLength = dataBuilder.GetCEs(prefix, str, oldCEs, 0);
            if (!SameCEs(newCEs, newCEsLength, oldCEs, oldCEsLength))
            {
                if (ce32 == Collation.UNASSIGNED_CE32)
                {
                    ce32 = dataBuilder.EncodeCEs(newCEs, newCEsLength);
                }
                dataBuilder.AddCE32(prefix, str, ce32);
            }
            return ce32;
        }

        private static bool SameCEs(long[] ces1, int ces1Length,
                    long[] ces2, int ces2Length)
        {
            if (ces1Length != ces2Length)
            {
                return false;
            }
            Debug.Assert(ces1Length <= Collation.MAX_EXPANSION_LENGTH);
            for (int i = 0; i < ces1Length; ++i)
            {
                if (ces1[i] != ces2[i]) { return false; }
            }
            return true;
        }

        private static int AlignWeightRight(int w)
        {
            if (w != 0)
            {
                while ((w & 0xff) == 0) { w = w.TripleShift(8); /*w >>>= 8;*/ }
            }
            return w;
        }

        /// <summary>
        /// Walks the tailoring graph and overwrites tailored nodes with new CEs.
        /// After this, the graph is destroyed.
        /// The nodes array can then be used only as a source of tailored CEs.
        /// </summary>
        private void MakeTailoredCEs()
        {
            CollationWeights primaries = new CollationWeights();
            CollationWeights secondaries = new CollationWeights();
            CollationWeights tertiaries = new CollationWeights();
            //long[] nodesArray = nodes.getBuffer();
            if (DEBUG)
            {
                Console.Out.WriteLine("\nCollationBuilder.makeTailoredCEs()");
            }

            for (int rpi = 0; rpi < rootPrimaryIndexes.Count; ++rpi)
            {
                int i = rootPrimaryIndexes[rpi];
                long node = nodes[i];
                long p = Weight32FromNode(node);
                int s = p == 0 ? 0 : Collation.CommonWeight16;
                int t = s;
                int q = 0;
                bool pIsTailored = false;
                bool sIsTailored = false;
                bool tIsTailored = false;
                if (DEBUG)
                {
                    Console.Out.Write("\nprimary     {0:x}\n", AlignWeightRight((int)p));
                }
                int pIndex = p == 0 ? 0 : rootElements.FindPrimary(p);
                int nextIndex = NextIndexFromNode(node);
                while (nextIndex != 0)
                {
                    i = nextIndex;
                    node = nodes[i];
                    nextIndex = NextIndexFromNode(node);
                    CollationStrength strength = StrengthFromNode(node);
                    if (strength == CollationStrength.Quaternary)
                    {
                        Debug.Assert(IsTailoredNode(node));
                        if (DEBUG)
                        {
                            Console.Out.Write("      quat+     ");
                        }
                        if (q == 3)
                        {
                            // C++ U_BUFFER_OVERFLOW_ERROR
                            throw new NotSupportedException("quaternary tailoring gap too small");
                        }
                        ++q;
                    }
                    else
                    {
                        if (strength == CollationStrength.Tertiary)
                        {
                            if (IsTailoredNode(node))
                            {
                                if (DEBUG)
                                {
                                    Console.Out.Write("    ter+        ");
                                }
                                if (!tIsTailored)
                                {
                                    // First tailored tertiary node for [p, s].
                                    int tCount = CountTailoredNodes(nodes, nextIndex,
                                                                        CollationStrength.Tertiary) + 1;
                                    int tLimit;
                                    if (t == 0)
                                    {
                                        // Gap at the beginning of the tertiary CE range.
                                        t = rootElements.TertiaryBoundary - 0x100;
                                        tLimit = (int)rootElements.FirstTertiaryCE & Collation.OnlyTertiaryMask;
                                    }
                                    else if (!pIsTailored && !sIsTailored)
                                    {
                                        // p and s are root weights.
                                        tLimit = rootElements.GetTertiaryAfter(pIndex, s, t);
                                    }
                                    else if (t == Collation.BEFORE_WEIGHT16)
                                    {
                                        tLimit = Collation.CommonWeight16;
                                    }
                                    else
                                    {
                                        // [p, s] is tailored.
                                        Debug.Assert(t == Collation.CommonWeight16);
                                        tLimit = rootElements.TertiaryBoundary;
                                    }
                                    Debug.Assert(tLimit == 0x4000 || (tLimit & ~Collation.OnlyTertiaryMask) == 0);
                                    tertiaries.InitForTertiary();
                                    if (!tertiaries.AllocWeights(t, tLimit, tCount))
                                    {
                                        // C++ U_BUFFER_OVERFLOW_ERROR
                                        throw new NotSupportedException("tertiary tailoring gap too small");
                                    }
                                    tIsTailored = true;
                                }
                                // ICU4N: Need to capture the value as long so we can use it in the assert
                                long temp = tertiaries.NextWeight();
                                t = (int)temp;
                                Debug.Assert(temp != 0xffffffff);
                            }
                            else
                            {
                                t = Weight16FromNode(node);
                                tIsTailored = false;
                                if (DEBUG)
                                {
                                    Console.Out.Write("    ter     {0:x}\n", AlignWeightRight(t));
                                }
                            }
                        }
                        else
                        {
                            if (strength == CollationStrength.Secondary)
                            {
                                if (IsTailoredNode(node))
                                {
                                    if (DEBUG)
                                    {
                                        Console.Out.Write("  sec+          ");
                                    }
                                    if (!sIsTailored)
                                    {
                                        // First tailored secondary node for p.
                                        int sCount = CountTailoredNodes(nodes, nextIndex,
                                                                            CollationStrength.Secondary) + 1;
                                        int sLimit;
                                        if (s == 0)
                                        {
                                            // Gap at the beginning of the secondary CE range.
                                            s = rootElements.SecondaryBoundary - 0x100;
                                            sLimit = (int)(rootElements.FirstSecondaryCE >> 16);
                                        }
                                        else if (!pIsTailored)
                                        {
                                            // p is a root primary.
                                            sLimit = rootElements.GetSecondaryAfter(pIndex, s);
                                        }
                                        else if (s == Collation.BEFORE_WEIGHT16)
                                        {
                                            sLimit = Collation.CommonWeight16;
                                        }
                                        else
                                        {
                                            // p is a tailored primary.
                                            Debug.Assert(s == Collation.CommonWeight16);
                                            sLimit = rootElements.SecondaryBoundary;
                                        }
                                        if (s == Collation.CommonWeight16)
                                        {
                                            // Do not tailor into the getSortKey() range of
                                            // compressed common secondaries.
                                            s = rootElements.LastCommonSecondary;
                                        }
                                        secondaries.InitForSecondary();
                                        if (!secondaries.AllocWeights(s, sLimit, sCount))
                                        {
                                            // C++ U_BUFFER_OVERFLOW_ERROR
                                            if (DEBUG)
                                            {
                                                Console.Out.Write("!secondaries.allocWeights({0:x}, {1:x}, sCount={2})\n",
                                                        AlignWeightRight(s), AlignWeightRight(sLimit),
                                                        AlignWeightRight(sCount));
                                            }
                                            throw new NotSupportedException("secondary tailoring gap too small");
                                        }
                                        sIsTailored = true;
                                    }
                                    long temp = secondaries.NextWeight();
                                    s = (int)temp;
                                    Debug.Assert(temp != 0xffffffff);
                                }
                                else
                                {
                                    s = Weight16FromNode(node);
                                    sIsTailored = false;
                                    if (DEBUG)
                                    {
                                        Console.Out.Write("  sec       {0:x}\n", AlignWeightRight(s));
                                    }
                                }
                            }
                            else /* Collator.PRIMARY */
                            {
                                Debug.Assert(IsTailoredNode(node));
                                if (DEBUG)
                                {
                                    Console.Out.Write("pri+            ");
                                }
                                if (!pIsTailored)
                                {
                                    // First tailored primary node in this list.
                                    int pCount = CountTailoredNodes(nodes, nextIndex,
                                                                        CollationStrength.Primary) + 1;
                                    bool isCompressible = baseData.IsCompressiblePrimary(p);
                                    long pLimit =
                                        rootElements.GetPrimaryAfter(p, pIndex, isCompressible);
                                    primaries.InitForPrimary(isCompressible);
                                    if (!primaries.AllocWeights(p, pLimit, pCount))
                                    {
                                        // C++ U_BUFFER_OVERFLOW_ERROR  // TODO: introduce a more specific UErrorCode?
                                        throw new NotSupportedException("primary tailoring gap too small");
                                    }
                                    pIsTailored = true;
                                }
                                p = primaries.NextWeight();
                                Debug.Assert(p != 0xffffffffL);
                                s = Collation.CommonWeight16;
                                sIsTailored = false;
                            }
                            t = s == 0 ? 0 : Collation.CommonWeight16;
                            tIsTailored = false;
                        }
                        q = 0;
                    }
                    if (IsTailoredNode(node))
                    {
                        nodes[i] = Collation.MakeCE(p, s, t, q);
                        if (DEBUG)
                        {
                            Console.Out.Write("{0:x16}\n", nodes[i]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Counts the tailored nodes of the given strength up to the next node
        /// which is either stronger or has an explicit weight of this <paramref name="strength"/>.
        /// </summary>
        private static int CountTailoredNodes(IList<long> nodesArray, int i, CollationStrength strength)
        {
            int count = 0;
            for (; ; )
            {
                if (i == 0) { break; }
                long node = nodesArray[i];
                if (StrengthFromNode(node) < strength) { break; }
                if (StrengthFromNode(node) == strength)
                {
                    if (IsTailoredNode(node))
                    {
                        ++count;
                    }
                    else
                    {
                        break;
                    }
                }
                i = NextIndexFromNode(node);
            }
            return count;
        }

        private sealed class CEFinalizer : CollationDataBuilder.ICEModifier
        {
            internal CEFinalizer(IList<long> ces)
            {
                finalCEs = ces;
            }
                public long ModifyCE32(int ce32)
            {
                Debug.Assert(!Collation.IsSpecialCE32(ce32));
                if (CollationBuilder.IsTempCE32(ce32))
                {
                    // retain case bits
                    return finalCEs[CollationBuilder.IndexFromTempCE32(ce32)] | (uint)((ce32 & 0xc0) << 8);
                }
                else
                {
                    return Collation.NoCE;
                }
            }
                public long ModifyCE(long ce)
            {
                if (CollationBuilder.IsTempCE(ce))
                {
                    // retain case bits
                    return finalCEs[CollationBuilder.IndexFromTempCE(ce)] | (ce & 0xc000);
                }
                else
                {
                    return Collation.NoCE;
                }
            }

            private IList<long> finalCEs;
        };

        /// <summary>Replaces temporary CEs with the final CEs they point to.</summary>
        private void FinalizeCEs()
        {
            CollationDataBuilder newBuilder = new CollationDataBuilder();
            newBuilder.InitForTailoring(baseData);
            CEFinalizer finalizer = new CEFinalizer(nodes);
            newBuilder.CopyFrom(dataBuilder, finalizer);
            dataBuilder = newBuilder;
        }

        /// <summary>
        /// Encodes "temporary CE" data into a CE that fits into the CE32 data structure,
        /// with 2-byte primary, 1-byte secondary and 6-bit tertiary,
        /// with valid CE byte values.
        /// <para/>
        /// The index must not exceed 20 bits (0xfffff).
        /// The strength must fit into 2 bits (<see cref="CollationStrength.Primary"/>..<see cref="CollationStrength.Quaternary"/>).
        /// <para/>
        /// Temporary CEs are distinguished from real CEs by their use of
        /// secondary weights 06..45 which are otherwise reserved for compressed sort keys.
        /// <para/>
        /// The case bits are unused and available.
        /// </summary>
        private static long TempCEFromIndexAndStrength(int index, CollationStrength strength)
        {
            return
                // CE byte offsets, to ensure valid CE bytes, and case bits 11
                0x4040000006002000L +
                // index bits 19..13 -> primary byte 1 = CE bits 63..56 (byte values 40..BF)
                ((long)(index & 0xfe000) << 43) +
                // index bits 12..6 -> primary byte 2 = CE bits 55..48 (byte values 40..BF)
                ((long)(index & 0x1fc0) << 42) +
                // index bits 5..0 -> secondary byte 1 = CE bits 31..24 (byte values 06..45)
                ((index & 0x3f) << 24) +
                // strength bits 1..0 -> tertiary byte 1 = CE bits 13..8 (byte values 20..23)
                ((int)strength << 8);
        }
        private static int IndexFromTempCE(long tempCE)
        {
            tempCE -= 0x4040000006002000L;
            return
                ((int)(tempCE >> 43) & 0xfe000) |
                ((int)(tempCE >> 42) & 0x1fc0) |
                ((int)(tempCE >> 24) & 0x3f);
        }
        private static CollationStrength StrengthFromTempCE(long tempCE)
        {
            return (CollationStrength)(((int)tempCE >> 8) & 3);
        }
        private static bool IsTempCE(long ce)
        {
            int sec = ((int)ce).TripleShift(24);
            return 6 <= sec && sec <= 0x45;
        }

        private static int IndexFromTempCE32(int tempCE32)
        {
            tempCE32 -= 0x40400620;
            return
                ((tempCE32 >> 11) & 0xfe000) |
                ((tempCE32 >> 10) & 0x1fc0) |
                ((tempCE32 >> 8) & 0x3f);
        }
        private static bool IsTempCE32(int ce32)
        {
            return
                (ce32 & 0xff) >= 2 &&  // not a long-primary/long-secondary CE32
                6 <= ((ce32 >> 8) & 0xff) && ((ce32 >> 8) & 0xff) <= 0x45;
        }

        private static CollationStrength CeStrength(long ce)
        {
            return
                IsTempCE(ce) ? StrengthFromTempCE(ce) :
                (ce & unchecked((long)0xff00000000000000L)) != 0 ? CollationStrength.Primary :
                ((int)ce & 0xff000000) != 0 ? CollationStrength.Secondary :
                ce != 0 ? CollationStrength.Tertiary :
                CollationStrength.Identical;
        }

        /// <summary>At most 1M nodes, limited by the 20 bits in node bit fields.</summary>
        private const int MAX_INDEX = 0xfffff;
        /// <summary>
        /// Node bit 6 is set on a primary node if there are nodes
        /// with secondary values below the common secondary weight (05).
        /// </summary>
        private const int HAS_BEFORE2 = 0x40;
        /// <summary>
        /// Node bit 5 is set on a primary or secondary node if there are nodes
        /// with tertiary values below the common tertiary weight (05).
        /// </summary>
        private const int HAS_BEFORE3 = 0x20;
        /// <summary>
        /// Node bit 3 distinguishes a tailored node, which has no weight value,
        /// from a node with an explicit (root or default) weight.
        /// </summary>
        private const int IS_TAILORED = 8;

        private static long NodeFromWeight32(long weight32)
        {
            return weight32 << 32;
        }
        private static long NodeFromWeight16(int weight16)
        {
            return (long)weight16 << 48;
        }
        private static long NodeFromPreviousIndex(int previous)
        {
            return (long)previous << 28;
        }
        private static long NodeFromNextIndex(int next)
        {
            return next << 8;
        }
        private static long NodeFromStrength(CollationStrength strength)
        {
            return (long)strength;
        }

        private static long Weight32FromNode(long node)
        {
            return node.TripleShift(32);
        }
        private static int Weight16FromNode(long node)
        {
            return (int)(node >> 48) & 0xffff;
        }
        private static int PreviousIndexFromNode(long node)
        {
            return (int)(node >> 28) & MAX_INDEX;
        }
        private static int NextIndexFromNode(long node)
        {
            return ((int)node >> 8) & MAX_INDEX;
        }
        private static CollationStrength StrengthFromNode(long node)
        {
            return (CollationStrength)((int)node & 3);
        }

        private static bool NodeHasBefore2(long node)
        {
            return (node & HAS_BEFORE2) != 0;
        }
        private static bool NodeHasBefore3(long node)
        {
            return (node & HAS_BEFORE3) != 0;
        }
        private static bool NodeHasAnyBefore(long node)
        {
            return (node & (HAS_BEFORE2 | HAS_BEFORE3)) != 0;
        }
        private static bool IsTailoredNode(long node)
        {
            return (node & IS_TAILORED) != 0;
        }

        private static long ChangeNodePreviousIndex(long node, int previous)
        {
            return (long)((ulong)node & 0xffff00000fffffffL) | NodeFromPreviousIndex(previous);
        }
        private static long ChangeNodeNextIndex(long node, int next)
        {
            return (long)((ulong)node & 0xfffffffff00000ffL) | NodeFromNextIndex(next);
        }

        private Normalizer2 nfd, fcd;
        private Normalizer2Impl nfcImpl;

        private CollationTailoring @base;
        private CollationData baseData;
        private CollationRootElements rootElements;
        private long variableTop;

        private CollationDataBuilder dataBuilder;
        private bool fastLatinEnabled;
        private UnicodeSet optimizeSet = new UnicodeSet();

        private long[] ces = new long[Collation.MAX_EXPANSION_LENGTH];
        private int cesLength;

        /// <summary>
        /// Indexes of nodes with root primary weights, sorted by primary.
        /// Compact form of a <see cref="SortedDictionary{TKey, TValue}"/> from root primary to node index.
        /// <para/>
        /// This is a performance optimization for finding reset positions.
        /// Without this, we would have to search through the entire nodes list.
        /// It also allows storing root primary weights in list head nodes,
        /// without previous index, leaving room in root primary nodes for 32-bit primary weights.
        /// </summary>
        private IList<int> rootPrimaryIndexes;
        /// <summary>
        /// Data structure for assigning tailored weights and CEs.
        /// Doubly-linked lists of nodes in mostly collation order.
        /// Each list starts with a root primary node and ends with a nextIndex of 0.
        /// </summary>
        /// <remarks>
        /// When there are any nodes in the list, then there is always a root primary node at index 0.
        /// This allows some code not to have to check explicitly for nextIndex==0.
        /// <para/>
        /// Root primary nodes have 32-bit weights but do not have previous indexes.
        /// All other nodes have at most 16-bit weights and do have previous indexes.
        /// <para/>
        /// Nodes with explicit weights store root collator weights,
        /// or default weak weights (e.g., secondary 05) for stronger nodes.
        /// "Tailored" nodes, with the IS_TAILORED bit set,
        /// do not store explicit weights but rather
        /// create a difference of a certain strength from the preceding node.
        /// <para/>
        /// A root node is followed by either
        /// - a root/default node of the same strength, or
        /// - a root/default node of the next-weaker strength, or
        /// - a tailored node of the same strength.
        /// <para/>
        /// A node of a given strength normally implies "common" weights on weaker levels.
        /// <para/>
        /// A node with HAS_BEFORE2 must be immediately followed by
        /// a secondary node with an explicit below-common weight, then a secondary tailored node,
        /// and later an explicit common-secondary node.
        /// The below-common weight can be a root weight,
        /// or it can be BEFORE_WEIGHT16 for tailoring before an implied common weight
        /// or before the lowest root weight.
        /// (&amp;[before 2] resets to an explicit secondary node so that
        /// the following addRelation(secondary) tailors right after that.
        /// If we did not have this node and instead were to reset on the primary node,
        /// then addRelation(secondary) would skip forward to the the COMMON_WEIGHT16 node.)
        /// <para/>
        /// If the flag is not set, then there are no explicit secondary nodes
        /// with the common or lower weights.
        /// <para/>
        /// Same for HAS_BEFORE3 for tertiary nodes and weights.
        /// A node must not have both flags set.
        /// <para/>
        /// Tailored CEs are initially represented in a CollationDataBuilder as temporary CEs
        /// which point to stable indexes in this list,
        /// and temporary CEs stored in a CollationDataBuilder only point to tailored nodes.
        /// <para/>
        /// A temporary CE in the ces[] array may point to a non-tailored reset-before-position node,
        /// until the next relation is added.
        /// <para/>
        /// At the end, the tailored weights are allocated as necessary,
        /// then the tailored nodes are replaced with final CEs,
        /// and the CollationData is rewritten by replacing temporary CEs with final ones.
        /// <para/>
        /// We cannot simply insert new nodes in the middle of the array
        /// because that would invalidate the indexes stored in existing temporary CEs.
        /// We need to use a linked graph with stable indexes to existing nodes.
        /// A doubly-linked list seems easiest to maintain.
        /// <para/>
        /// Each node is stored as an long, with its fields stored as bit fields.
        /// <para/>
        /// Root primary node:
        /// - primary weight: 32 bits 63..32
        /// - reserved/unused/zero: 4 bits 31..28
        /// <para/>
        /// Weaker root nodes &amp; tailored nodes:
        /// - a weight: 16 bits 63..48
        ///   + a root or default weight for a non-tailored node
        ///   + unused/zero for a tailored node
        /// - index to the previous node: 20 bits 47..28
        /// <para/>
        /// All types of nodes:
        /// - index to the next node: 20 bits 27..8
        ///   + nextIndex=0 in last node per root-primary list
        /// - reserved/unused/zero bits: bits 7, 4, 2
        /// - HAS_BEFORE2: bit 6
        /// - HAS_BEFORE3: bit 5
        /// - IS_TAILORED: bit 3
        /// - the difference strength (primary/secondary/tertiary/quaternary): 2 bits 1..0
        /// <para/>
        /// We could allocate structs with pointers, but we would have to store them
        /// in a pointer list so that they can be indexed from temporary CEs,
        /// and they would require more memory allocations.
        /// </remarks>
        private IList<long> nodes;
    }
}
