using ICU4N.Support;
using ICU4N.Text;
using System.Diagnostics;

namespace ICU4N.Impl.Coll
{
    /// <since>2012feb14</since>
    /// <author>Markus W. Scherer</author>
    public static class CollationCompare /* all static */
    {
        public static int CompareUpToQuaternary(CollationIterator left, CollationIterator right,
            CollationSettings settings)
        {
            int options = settings.Options;
            long variableTop;
            if ((options & CollationSettings.AlternateMask) == 0)
            {
                variableTop = 0;
            }
            else
            {
                // +1 so that we can use "<" and primary ignorables test out early.
                variableTop = settings.VariableTop + 1;
            }
            bool anyVariable = false;

            // Fetch CEs, compare primaries, store secondary & tertiary weights.
            for (; ; )
            {
                // We fetch CEs until we get a non-ignorable primary or reach the end.
                long leftPrimary;
                do
                {
                    long ce = left.NextCE();
                    leftPrimary = ce.TripleShift(32);
                    if (leftPrimary < variableTop && leftPrimary > Collation.MergeSeparatorPrimary)
                    {
                        // Variable CE, shift it to quaternary level.
                        // Ignore all following primary ignorables, and shift further variable CEs.
                        anyVariable = true;
                        do
                        {
                            // Store only the primary of the variable CE.
                            left.SetCurrentCE(ce & unchecked((long)0xffffffff00000000L));
                            for (; ; )
                            {
                                ce = left.NextCE();
                                leftPrimary = ce.TripleShift(32);
                                if (leftPrimary == 0)
                                {
                                    left.SetCurrentCE(0);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        } while (leftPrimary < variableTop && leftPrimary > Collation.MergeSeparatorPrimary);
                    }
                } while (leftPrimary == 0);

                long rightPrimary;
                do
                {
                    long ce = right.NextCE();
                    rightPrimary = ce.TripleShift(32);
                    if (rightPrimary < variableTop && rightPrimary > Collation.MergeSeparatorPrimary)
                    {
                        // Variable CE, shift it to quaternary level.
                        // Ignore all following primary ignorables, and shift further variable CEs.
                        anyVariable = true;
                        do
                        {
                            // Store only the primary of the variable CE.
                            right.SetCurrentCE(ce & unchecked((long)0xffffffff00000000L));
                            for (; ; )
                            {
                                ce = right.NextCE();
                                rightPrimary = ce.TripleShift(32);
                                if (rightPrimary == 0)
                                {
                                    right.SetCurrentCE(0);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        } while (rightPrimary < variableTop && rightPrimary > Collation.MergeSeparatorPrimary);
                    }
                } while (rightPrimary == 0);

                if (leftPrimary != rightPrimary)
                {
                    // Return the primary difference, with script reordering.
                    if (settings.HasReordering)
                    {
                        leftPrimary = settings.Reorder(leftPrimary);
                        rightPrimary = settings.Reorder(rightPrimary);
                    }
                    return (leftPrimary < rightPrimary) ? Collation.Less : Collation.Greater;
                }
                if (leftPrimary == Collation.NO_CE_PRIMARY)
                {
                    break;
                }
            }

            // Compare the buffered secondary & tertiary weights.
            // We might skip the secondary level but continue with the case level
            // which is turned on separately.
            if (CollationSettings.GetStrength(options) >= CollationStrength.Secondary)
            {
                if ((options & CollationSettings.BackwardSecondary) == 0)
                {
                    int leftIndex2 = 0;
                    int rightIndex2 = 0;
                    for (; ; )
                    {
                        int leftSecondary;
                        do
                        {
                            leftSecondary = ((int)left.GetCE(leftIndex2++)).TripleShift(16);
                        } while (leftSecondary == 0);

                        int rightSecondary;
                        do
                        {
                            rightSecondary = ((int)right.GetCE(rightIndex2++)).TripleShift(16);
                        } while (rightSecondary == 0);

                        if (leftSecondary != rightSecondary)
                        {
                            return (leftSecondary < rightSecondary) ? Collation.Less : Collation.Greater;
                        }
                        if (leftSecondary == Collation.NO_CE_WEIGHT16)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    // The backwards secondary level compares secondary weights backwards
                    // within segments separated by the merge separator (U+FFFE, weight 02).
                    int leftStart = 0;
                    int rightStart = 0;
                    for (; ; )
                    {
                        // Find the merge separator or the NO_CE terminator.
                        long p;
                        int leftLimit = leftStart;
                        while ((p = left.GetCE(leftLimit).TripleShift(32)) > Collation.MergeSeparatorPrimary
                                || p == 0)
                        {
                            ++leftLimit;
                        }
                        int rightLimit = rightStart;
                        while ((p = right.GetCE(rightLimit).TripleShift(32)) > Collation.MergeSeparatorPrimary
                                || p == 0)
                        {
                            ++rightLimit;
                        }

                        // Compare the segments.
                        int leftIndex3 = leftLimit;
                        int rightIndex3 = rightLimit;
                        for (; ; )
                        {
                            int leftSecondary = 0;
                            while (leftSecondary == 0 && leftIndex3 > leftStart)
                            {
                                leftSecondary = ((int)left.GetCE(--leftIndex3)).TripleShift(16);
                            }

                            int rightSecondary = 0;
                            while (rightSecondary == 0 && rightIndex3 > rightStart)
                            {
                                rightSecondary = ((int)right.GetCE(--rightIndex3)).TripleShift(16);
                            }

                            if (leftSecondary != rightSecondary)
                            {
                                return (leftSecondary < rightSecondary) ? Collation.Less : Collation.Greater;
                            }
                            if (leftSecondary == 0)
                            {
                                break;
                            }
                        }

                        // Did we reach the end of either string?
                        // Both strings have the same number of merge separators,
                        // or else there would have been a primary-level difference.
                        Debug.Assert(left.GetCE(leftLimit) == right.GetCE(rightLimit));
                        if (p == Collation.NO_CE_PRIMARY)
                        {
                            break;
                        }
                        // Skip both merge separators and continue.
                        leftStart = leftLimit + 1;
                        rightStart = rightLimit + 1;
                    }
                }
            }

            if ((options & CollationSettings.CaseLevel) != 0)
            {
                CollationStrength strength = CollationSettings.GetStrength(options);
                int leftIndex4 = 0;
                int rightIndex4 = 0;
                for (; ; )
                {
                    int leftCase, leftLower32, rightCase;
                    if (strength == CollationStrength.Primary)
                    {
                        // Primary+caseLevel: Ignore case level weights of primary ignorables.
                        // Otherwise we would get a-umlaut > a
                        // which is not desirable for accent-insensitive sorting.
                        // Check for (lower 32 bits) == 0 as well because variable CEs are stored
                        // with only primary weights.
                        long ce;
                        do
                        {
                            ce = left.GetCE(leftIndex4++);
                            leftCase = (int)ce;
                        } while ((ce.TripleShift(32)) == 0 || leftCase == 0);
                        leftLower32 = leftCase;
                        leftCase &= 0xc000;

                        do
                        {
                            ce = right.GetCE(rightIndex4++);
                            rightCase = (int)ce;
                        } while ((ce.TripleShift(32)) == 0 || rightCase == 0);
                        rightCase &= 0xc000;
                    }
                    else
                    {
                        // Secondary+caseLevel: By analogy with the above,
                        // ignore case level weights of secondary ignorables.
                        //
                        // Note: A tertiary CE has uppercase case bits (0.0.ut)
                        // to keep tertiary+caseFirst well-formed.
                        //
                        // Tertiary+caseLevel: Also ignore case level weights of secondary ignorables.
                        // Otherwise a tertiary CE's uppercase would be no greater than
                        // a primary/secondary CE's uppercase.
                        // (See UCA well-formedness condition 2.)
                        // We could construct a special case weight higher than uppercase,
                        // but it's simpler to always ignore case weights of secondary ignorables,
                        // turning 0.0.ut into 0.0.0.t.
                        // (See LDML Collation, Case Parameters.)
                        do
                        {
                            leftCase = (int)left.GetCE(leftIndex4++);
                        } while ((leftCase & 0xffff0000) == 0);
                        leftLower32 = leftCase;
                        leftCase &= 0xc000;

                        do
                        {
                            rightCase = (int)right.GetCE(rightIndex4++);
                        } while ((rightCase & 0xffff0000) == 0);
                        rightCase &= 0xc000;
                    }

                    // No need to handle NO_CE and MERGE_SEPARATOR specially:
                    // There is one case weight for each previous-level weight,
                    // so level length differences were handled there.
                    if (leftCase != rightCase)
                    {
                        if ((options & CollationSettings.UpperFirst) == 0)
                        {
                            return (leftCase < rightCase) ? Collation.Less : Collation.Greater;
                        }
                        else
                        {
                            return (leftCase < rightCase) ? Collation.Greater : Collation.Less;
                        }
                    }
                    if ((leftLower32.TripleShift(16)) == Collation.NO_CE_WEIGHT16)
                    {
                        break;
                    }
                }
            }
            if (CollationSettings.GetStrength(options) <= CollationStrength.Secondary)
            {
                return Collation.Equal;
            }

            int tertiaryMask = CollationSettings.GetTertiaryMask(options);

            int leftIndex = 0;
            int rightIndex = 0;
            int anyQuaternaries = 0;
            for (; ; )
            {
                int leftLower32, leftTertiary;
                do
                {
                    leftLower32 = (int)left.GetCE(leftIndex++);
                    anyQuaternaries |= leftLower32;
                    Debug.Assert((leftLower32 & Collation.OnlyTertiaryMask) != 0 || (leftLower32 & 0xc0c0) == 0);
                    leftTertiary = leftLower32 & tertiaryMask;
                } while (leftTertiary == 0);

                int rightLower32, rightTertiary;
                do
                {
                    rightLower32 = (int)right.GetCE(rightIndex++);
                    anyQuaternaries |= rightLower32;
                    Debug.Assert((rightLower32 & Collation.OnlyTertiaryMask) != 0 || (rightLower32 & 0xc0c0) == 0);
                    rightTertiary = rightLower32 & tertiaryMask;
                } while (rightTertiary == 0);

                if (leftTertiary != rightTertiary)
                {
                    if (CollationSettings.SortsTertiaryUpperCaseFirst(options))
                    {
                        // Pass through NO_CE and keep real tertiary weights larger than that.
                        // Do not change the artificial uppercase weight of a tertiary CE (0.0.ut),
                        // to keep tertiary CEs well-formed.
                        // Their case+tertiary weights must be greater than those of
                        // primary and secondary CEs.
                        if (leftTertiary > Collation.NO_CE_WEIGHT16)
                        {
                            if ((leftLower32 & 0xffff0000) != 0)
                            {
                                leftTertiary ^= 0xc000;
                            }
                            else
                            {
                                leftTertiary += 0x4000;
                            }
                        }
                        if (rightTertiary > Collation.NO_CE_WEIGHT16)
                        {
                            if ((rightLower32 & 0xffff0000) != 0)
                            {
                                rightTertiary ^= 0xc000;
                            }
                            else
                            {
                                rightTertiary += 0x4000;
                            }
                        }
                    }
                    return (leftTertiary < rightTertiary) ? Collation.Less : Collation.Greater;
                }
                if (leftTertiary == Collation.NO_CE_WEIGHT16)
                {
                    break;
                }
            }
            if (CollationSettings.GetStrength(options) <= CollationStrength.Tertiary)
            {
                return Collation.Equal;
            }

            if (!anyVariable && (anyQuaternaries & 0xc0) == 0)
            {
                // If there are no "variable" CEs and no non-zero quaternary weights,
                // then there are no quaternary differences.
                return Collation.Equal;
            }

            leftIndex = 0;
            rightIndex = 0;
            for (; ; )
            {
                long leftQuaternary;
                do
                {
                    long ce = left.GetCE(leftIndex++);
                    leftQuaternary = ce & 0xffff;
                    if (leftQuaternary <= Collation.NO_CE_WEIGHT16)
                    {
                        // Variable primary or completely ignorable or NO_CE.
                        leftQuaternary = ce.TripleShift(32);
                    }
                    else
                    {
                        // Regular CE, not tertiary ignorable.
                        // Preserve the quaternary weight in bits 7..6.
                        leftQuaternary |= 0xffffff3fL;
                    }
                } while (leftQuaternary == 0);

                long rightQuaternary;
                do
                {
                    long ce = right.GetCE(rightIndex++);
                    rightQuaternary = ce & 0xffff;
                    if (rightQuaternary <= Collation.NO_CE_WEIGHT16)
                    {
                        // Variable primary or completely ignorable or NO_CE.
                        rightQuaternary = ce.TripleShift(32);
                    }
                    else
                    {
                        // Regular CE, not tertiary ignorable.
                        // Preserve the quaternary weight in bits 7..6.
                        rightQuaternary |= 0xffffff3fL;
                    }
                } while (rightQuaternary == 0);

                if (leftQuaternary != rightQuaternary)
                {
                    // Return the difference, with script reordering.
                    if (settings.HasReordering)
                    {
                        leftQuaternary = settings.Reorder(leftQuaternary);
                        rightQuaternary = settings.Reorder(rightQuaternary);
                    }
                    return (leftQuaternary < rightQuaternary) ? Collation.Less : Collation.Greater;
                }
                if (leftQuaternary == Collation.NO_CE_PRIMARY)
                {
                    break;
                }
            }
            return Collation.Equal;
        }
    }
}
