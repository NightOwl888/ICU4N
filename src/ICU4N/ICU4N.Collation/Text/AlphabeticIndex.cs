using ICU4N.Lang;
using ICU4N.Support;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Collections;

namespace ICU4N.Text
{
    public sealed class AlphabeticIndex<T> : IEnumerable<AlphabeticIndex<T>.Bucket>
    {
        /**
     * Prefix string for Chinese index buckets.
     * See http://unicode.org/repos/cldr/trunk/specs/ldml/tr35-collation.html#Collation_Indexes
     */
        private static readonly string BASE = "\uFDD0";

        private static readonly char CGJ = '\u034F';

        private static readonly IComparer<string> binaryCmp = new UTF16.StringComparer(true, false, 0);

        private readonly RuleBasedCollator collatorOriginal;
        private readonly RuleBasedCollator collatorPrimaryOnly;
        private RuleBasedCollator collatorExternal;

        private class RecordComparer : IComparer<Record>
        {
            private readonly RuleBasedCollator collatorOriginal;

            public RecordComparer(RuleBasedCollator collatorOriginal)
            {
                this.collatorOriginal = collatorOriginal;
            }

            public int Compare(Record o1, Record o2)
            {
                return collatorOriginal.Compare(o1.Name, o2.Name);
            }
        }

        // Comparator for records, so that the Record class can be static.
        private readonly IComparer<Record> recordComparator;


        private readonly List<string> firstCharsInScripts;

        // We accumulate these as we build up the input parameters
        private readonly UnicodeSet initialLabels = new UnicodeSet();
        private List<Record> inputList;

        // Lazy evaluated: null means that we have not built yet.
        private BucketList buckets;

        private string overflowLabel = "\u2026";
        private string underflowLabel = "\u2026";
        private string inflowLabel = "\u2026";

        /**
         * Immutable, thread-safe version of {@link AlphabeticIndex}.
         * This class provides thread-safe methods for bucketing,
         * and random access to buckets and their properties,
         * but does not offer adding records to the index.
         *
         * @param <V> The Record value type is unused. It can be omitted for this class
         * if it was omitted for the AlphabeticIndex that built it.
         * @stable ICU 51
         */
        public sealed class ImmutableIndex : IEnumerable<Bucket>
        {
            internal readonly BucketList buckets;
            internal readonly Collator collatorPrimaryOnly;

            internal ImmutableIndex(BucketList bucketList, Collator collatorPrimaryOnly)
            {
                this.buckets = bucketList;
                this.collatorPrimaryOnly = collatorPrimaryOnly;
            }

            /**
             * Returns the number of index buckets and labels, including underflow/inflow/overflow.
             *
             * @return the number of index buckets
             * @stable ICU 51
             */
            public int BucketCount
            {
                get { return buckets.BucketCount; }
            }

            /**
             * Finds the index bucket for the given name and returns the number of that bucket.
             * Use {@link #getBucket(int)} to get the bucket's properties.
             *
             * @param name the string to be sorted into an index bucket
             * @return the bucket number for the name
             * @stable ICU 51
             */
            public int GetBucketIndex(ICharSequence name)
            {
                return buckets.GetBucketIndex(name, collatorPrimaryOnly);
            }

            /**
             * Returns the index-th bucket. Returns null if the index is out of range.
             *
             * @param index bucket number
             * @return the index-th bucket
             * @stable ICU 51
             */
            public Bucket GetBucket(int index)
            {
                if (0 <= index && index < buckets.BucketCount)
                {
                    return buckets.ImmutableVisibleList[index];
                }
                else
                {
                    return null;
                }
            }

            /**
             * {@inheritDoc}
             * @stable ICU 51
             */
            public IEnumerator<Bucket> GetEnumerator()
            {
                return buckets.GetEnumerator();
            }

            #region .NET Compatibility
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            #endregion
        }

        /**
         * Create the index object.
         *
         * @param locale
         *            The locale for the index.
         * @stable ICU 4.8
         */
        public AlphabeticIndex(ULocale locale)
                    : this(locale, null)
        {
        }

        /**
         * Create the index object.
         *
         * @param locale
         *            The locale for the index.
         * @stable ICU 4.8
         */
        public AlphabeticIndex(CultureInfo locale)
                    : this(ULocale.ForLocale(locale), null)
        {
        }

        /**
         * Create an AlphabeticIndex that uses a specific collator.
         *
         * <p>The index will be created with no labels; the addLabels() function must be called
         * after creation to add the desired labels to the index.
         *
         * <p>The index will work directly with the supplied collator. If the caller will need to
         * continue working with the collator it should be cloned first, so that the
         * collator provided to the AlphabeticIndex remains unchanged after creation of the index.
         *
         * @param collator The collator to use to order the contents of this index.
         * @stable ICU 51
         */
        public AlphabeticIndex(RuleBasedCollator collator)
                    : this(null, collator)
        {
        }

        /**
         * Internal constructor containing implementation used by public constructors.
         */
        private AlphabeticIndex(ULocale locale, RuleBasedCollator collator)
        {
            recordComparator = new RecordComparer(collatorOriginal);

            collatorOriginal = collator != null ? collator : (RuleBasedCollator)Collator.GetInstance(locale);
            try
            {
                collatorPrimaryOnly = (RuleBasedCollator)collatorOriginal.CloneAsThawed();
            }
            catch (Exception e)
            {
                // should never happen
                throw new InvalidOperationException("Collator cannot be cloned", e);
            }
            collatorPrimaryOnly.Strength = CollationStrength.Primary;
            collatorPrimaryOnly.Freeze();

            firstCharsInScripts = GetFirstCharactersInScripts();
            firstCharsInScripts.Sort(collatorPrimaryOnly);
            // Guard against a degenerate collator where
            // some script boundary strings are primary ignorable.
            for (; ; )
            {
                if (firstCharsInScripts.Count == 0)
                {
                    throw new ArgumentException(
                            "AlphabeticIndex requires some non-ignorable script boundary strings");
                }
                if (collatorPrimaryOnly.Compare(firstCharsInScripts.FirstOrDefault(), "") == 0)
                {
                    firstCharsInScripts.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }

            // Chinese index characters, which are specific to each of the several Chinese tailorings,
            // take precedence over the single locale data exemplar set per language.
            if (!AddChineseIndexCharacters() && locale != null)
            {
                AddIndexExemplars(locale);
            }
        }

        /**
         * Add more index characters (aside from what are in the locale)
         * @param additions additional characters to add to the index, such as A-Z.
         * @return this, for chaining
         * @stable ICU 4.8
         */
        public AlphabeticIndex<T> AddLabels(UnicodeSet additions)
        {
            initialLabels.AddAll(additions);
            buckets = null;
            return this;
        }

        /**
         * Add more index characters (aside from what are in the locale)
         * @param additions additional characters to add to the index, such as those in Swedish.
         * @return this, for chaining
         * @stable ICU 4.8
         */
        public AlphabeticIndex<T> AddLabels(params ULocale[] additions)
        {
            foreach (ULocale addition in additions)
            {
                AddIndexExemplars(addition);
            }
            buckets = null;
            return this;
        }

        /**
         * Add more index characters (aside from what are in the locale)
         * @param additions additional characters to add to the index, such as those in Swedish.
         * @return this, for chaining
         * @stable ICU 4.8
         */
        public AlphabeticIndex<T> AddLabels(params CultureInfo[] additions)
        {
            foreach (var addition in additions)
            {
                AddIndexExemplars(ULocale.ForLocale(addition));
            }
            buckets = null;
            return this;
        }

        /**
         * Set the overflow label
         * @param overflowLabel see class description
         * @return this, for chaining
         * @stable ICU 4.8
         */
        public AlphabeticIndex<T> SetOverflowLabel(string overflowLabel)
        {
            this.overflowLabel = overflowLabel;
            buckets = null;
            return this;
        }

        /**
         * Get the default label used in the IndexCharacters' locale for underflow, eg the last item in: X Y Z ...
         *
         * @return underflow label
         * @stable ICU 4.8
         */
        public string UnderflowLabel
        {
            get { return underflowLabel; } // TODO get localized version
        }


        /**
         * Set the underflowLabel label
         * @param underflowLabel see class description
         * @return this, for chaining
         * @stable ICU 4.8
         */
        public AlphabeticIndex<T> SetUnderflowLabel(string underflowLabel) // ICU4N TODO: API - make extension method ?
        {
            this.underflowLabel = underflowLabel;
            buckets = null;
            return this;
        }

        /**
         * Get the default label used in the IndexCharacters' locale for overflow, eg the first item in: ... A B C
         *
         * @return overflow label
         * @stable ICU 4.8
         */
        public string OverflowLabel
        {
            get { return overflowLabel; } // TODO get localized version
        }


        /**
         * Set the inflowLabel label
         * @param inflowLabel see class description
         * @return this, for chaining
         * @stable ICU 4.8
         */
        public AlphabeticIndex<T> SetInflowLabel(string inflowLabel)
        {
            this.inflowLabel = inflowLabel;
            buckets = null;
            return this;
        }

        /**
         * Get the default label used for abbreviated buckets <i>between</i> other labels. For example, consider the labels
         * for Latin and Greek are used: X Y Z ... &#x0391; &#x0392; &#x0393;.
         *
         * @return inflow label
         * @stable ICU 4.8
         */
        public string InflowLabel
        {
            get { return inflowLabel; } // TODO get localized version
        }


        /**
         * Get the limit on the number of labels in the index. The number of buckets can be slightly larger: see getBucketCount().
         *
         * @return maxLabelCount maximum number of labels.
         * @stable ICU 4.8
         */
        public int MaxLabelCount
        {
            get { return maxLabelCount; }
        }

        /**
         * Set a limit on the number of labels in the index. The number of buckets can be slightly larger: see
         * getBucketCount().
         *
         * @param maxLabelCount Set the maximum number of labels. Currently, if the number is exceeded, then every
         *         nth item is removed to bring the count down. A more sophisticated mechanism may be available in the
         *         future.
         * @return this, for chaining
         * @stable ICU 4.8
         */
        public AlphabeticIndex<T> SetMaxLabelCount(int maxLabelCount)
        {
            this.maxLabelCount = maxLabelCount;
            buckets = null;
            return this;
        }

        /**
         * Determine the best labels to use. This is based on the exemplars, but we also process to make sure that they are unique,
         * and sort differently, and that the overall list is small enough.
         */
        private IList<string> InitLabels()
        {
            Normalizer2 nfkdNormalizer = Normalizer2.GetNFKDInstance();
            List<String> indexCharacters = new List<string>();

            string firstScriptBoundary = firstCharsInScripts.FirstOrDefault();
            string overflowBoundary = firstCharsInScripts[firstCharsInScripts.Count - 1];

            // We make a sorted array of elements.
            // Some of the input may be redundant.
            // That is, we might have c, ch, d, where "ch" sorts just like "c", "h".
            // We filter out those cases.
            foreach (string item2 in initialLabels)
            {
                string item = item2;
                bool checkDistinct;
                if (!UTF16.HasMoreCodePointsThan(item, 1))
                {
                    checkDistinct = false;
                }
                else if (item[item.Length - 1] == '*' &&
                      item[item.Length - 2] != '*')
                {
                    // Use a label if it is marked with one trailing star,
                    // even if the label string sorts the same when all contractions are suppressed.
                    item = item.Substring(0, item.Length - 1); // ICU4N: Checked 2nd parameter
                    checkDistinct = false;
                }
                else
                {
                    checkDistinct = true;
                }
                if (collatorPrimaryOnly.Compare(item, firstScriptBoundary) < 0)
                {
                    // Ignore a primary-ignorable or non-alphabetic index character.
                }
                else if (collatorPrimaryOnly.Compare(item, overflowBoundary) >= 0)
                {
                    // Ignore an index character that will land in the overflow bucket.
                }
                else if (checkDistinct && collatorPrimaryOnly.Compare(item, Separated(item)) == 0)
                {
                    // Ignore a multi-code point index character that does not sort distinctly
                    // from the sequence of its separate characters.
                }
                else
                {
                    int insertionPoint = indexCharacters.BinarySearch(item, collatorPrimaryOnly);
                    if (insertionPoint < 0)
                    {
                        indexCharacters.Insert(~insertionPoint, item);
                    }
                    else
                    {
                        string itemAlreadyIn = indexCharacters[insertionPoint];
                        if (IsOneLabelBetterThanOther(nfkdNormalizer, item, itemAlreadyIn))
                        {
                            indexCharacters[insertionPoint] = item;
                        }
                    }
                }
            }

            // if the result is still too large, cut down to maxLabelCount elements, by removing every nth element

            int size = indexCharacters.Count - 1;
            if (size > maxLabelCount)
            {
                int count = 0;
                int old = -1;
                for (int i = 0; i < indexCharacters.Count; i++)
                {
                    ++count;
                    int bump = count * maxLabelCount / size;
                    if (bump != old)
                    {
                        // If this is not an element we want to toss,
                        // move it to the next position in the list.
                        if (count - 1 != i)
                        {
                            indexCharacters[count - 1] = indexCharacters[i];
                        }
                        old = bump;
                    }
                }

                // Remove everything else at once. This is several orders of magnitude
                // faster than removing elements one at a time.
                // Reference: https://stackoverflow.com/a/621836
                indexCharacters.RemoveRange(count - 1, size - (count - 1));

                //for (Iterator<String> it = indexCharacters.iterator(); it.hasNext();)
                //{
                //    ++count;
                //    it.next();
                //    int bump = count * maxLabelCount / size;
                //    if (bump == old)
                //    {
                //        it.remove();
                //    }
                //    else
                //    {
                //        old = bump;
                //    }
                //}
            }

            return indexCharacters;
        }

        private static string FixLabel(string current)
        {
            if (!current.StartsWith(BASE, StringComparison.Ordinal))
            {
                return current;
            }
            int rest = current[BASE.Length];
            if (0x2800 < rest && rest <= 0x28FF)
            { // stroke count
                return (rest - 0x2800) + "\u5283";
            }
            return current.Substring(BASE.Length);
        }

        /**
         * This method is called to get the index exemplars. Normally these come from the locale directly,
         * but if they aren't available, we have to synthesize them.
         */
        private void AddIndexExemplars(ULocale locale)
        {
            UnicodeSet exemplars = LocaleData.GetExemplarSet(locale, 0, LocaleData.ES_INDEX);
            if (exemplars != null)
            {
                initialLabels.AddAll(exemplars);
                return;
            }

            // The locale data did not include explicit Index characters.
            // Synthesize a set of them from the locale's standard exemplar characters.
            exemplars = LocaleData.GetExemplarSet(locale, 0, LocaleData.ES_STANDARD);

            exemplars = exemplars.CloneAsThawed();
            // question: should we add auxiliary exemplars?
            if (exemplars.ContainsSome('a', 'z') || exemplars.Count == 0)
            {
                exemplars.AddAll('a', 'z');
            }
            if (exemplars.ContainsSome(0xAC00, 0xD7A3))
            {  // Hangul syllables
               // cut down to small list
                exemplars.Remove(0xAC00, 0xD7A3).
                    Add(0xAC00).Add(0xB098).Add(0xB2E4).Add(0xB77C).
                    Add(0xB9C8).Add(0xBC14).Add(0xC0AC).Add(0xC544).
                    Add(0xC790).Add(0xCC28).Add(0xCE74).Add(0xD0C0).
                    Add(0xD30C).Add(0xD558);
            }
            if (exemplars.ContainsSome(0x1200, 0x137F))
            {  // Ethiopic block
               // cut down to small list
               // make use of the fact that Ethiopic is allocated in 8's, where
               // the base is 0 mod 8.
                UnicodeSet ethiopic = new UnicodeSet("[[:Block=Ethiopic:]&[:Script=Ethiopic:]]");
                UnicodeSetIterator it = new UnicodeSetIterator(ethiopic);
                while (it.Next() && it.Codepoint != UnicodeSetIterator.IS_STRING)
                {
                    if ((it.Codepoint & 0x7) != 0)
                    {
                        exemplars.Remove(it.Codepoint);
                    }
                }
            }

            // Upper-case any that aren't already so.
            //   (We only do this for synthesized index characters.)
            foreach (string item in exemplars)
            {
                initialLabels.Add(UCharacter.ToUpper(locale, item));
            }
        }

        /**
         * Add Chinese index characters from the tailoring.
         */
        private bool AddChineseIndexCharacters()
        {
            UnicodeSet contractions = new UnicodeSet();
            try
            {
                collatorPrimaryOnly.InternalAddContractions(BASE[0], contractions);
            }
            catch (Exception e)
            {
                return false;
            }
            if (contractions.IsEmpty) { return false; }
            initialLabels.AddAll(contractions);
            foreach (string s in contractions)
            {
                Debug.Assert(s.StartsWith(BASE, StringComparison.Ordinal));
                char c = s[s.Length - 1];
                if (0x41 <= c && c <= 0x5A)
                {  // A-Z
                   // There are Pinyin labels, add ASCII A-Z labels as well.
                    initialLabels.Add(0x41, 0x5A);  // A-Z
                    break;
                }
            }
            return true;
        }

        /**
         * Return the string with interspersed CGJs. Input must have more than 2 codepoints.
         * <p>This is used to test whether contractions sort differently from their components.
         */
        private string Separated(string item)
        {
            StringBuilder result = new StringBuilder();
            // add a CGJ except within surrogates
            char last = item[0];
            result.Append(last);
            for (int i = 1; i < item.Length; ++i)
            {
                char ch = item[i];
                if (!UCharacter.IsHighSurrogate(last) || !UCharacter.IsLowSurrogate(ch))
                {
                    result.Append(CGJ);
                }
                result.Append(ch);
                last = ch;
            }
            return result.ToString();
        }

        /**
         * Builds an immutable, thread-safe version of this instance, without data records.
         *
         * @return an immutable index instance
         * @stable ICU 51
         */
        public ImmutableIndex BuildImmutableIndex()
        {
            // The current AlphabeticIndex Java code never modifies the bucket list once built.
            // If it contains no records, we can use it.
            // addRecord() sets buckets=null rather than inserting the new record into it.
            BucketList immutableBucketList;
            if (inputList != null && inputList.Count > 0)
            {
                // We need a bucket list with no records.
                immutableBucketList = CreateBucketList();
            }
            else
            {
                if (buckets == null)
                {
                    buckets = CreateBucketList();
                }
                immutableBucketList = buckets;
            }
            return new ImmutableIndex(immutableBucketList, collatorPrimaryOnly);
        }

        /**
         * Get the labels.
         *
         * @return The list of bucket labels, after processing.
         * @stable ICU 4.8
         */
        public IList<string> GetBucketLabels()
        {
            InitBuckets();
            List<string> result = new List<string>();
            foreach (var bucket in buckets)
            {
                result.Add(bucket.Label);
            }
            return result;
        }

        /**
         * Get a clone of the collator used internally. Note that for performance reasons, the clone is only done once, and
         * then stored. The next time it is accessed, the same instance is returned.
         * <p>
         * <b><i>Don't use this method across threads if you are changing the settings on the collator, at least not without
         * synchronizing.</i></b>
         *
         * @return a clone of the collator used internally
         * @stable ICU 4.8
         */
        public RuleBasedCollator GetCollator()
        {
            if (collatorExternal == null)
            {
                try
                {
                    collatorExternal = (RuleBasedCollator)(collatorOriginal.Clone());
                }
                catch (Exception e)
                {
                    // should never happen
                    throw new InvalidOperationException("Collator cannot be cloned", e);
                }
            }
            return collatorExternal;
        }

        /**
         * Add a record (name and data) to the index. The name will be used to sort the items into buckets, and to sort
         * within the bucket. Two records may have the same name. When they do, the sort order is according to the order added:
         * the first added comes first.
         *
         * @param name
         *            Name, such as a name
         * @param data
         *            Data, such as an address or link
         * @return this, for chaining
         * @stable ICU 4.8
         */
        public AlphabeticIndex<T> AddRecord(ICharSequence name, T data)
        {
            // TODO instead of invalidating, just add to unprocessed list.
            buckets = null; // invalidate old bucketlist
            if (inputList == null)
            {
                inputList = new List<Record>();
            }
            inputList.Add(new Record(name, data));
            return this;
        }

        /**
         * Get the bucket number for the given name. This routine permits callers to implement their own bucket handling
         * mechanisms, including client-server handling. For example, when a new name is created on the client, it can ask
         * the server for the bucket for that name, and the sortkey (using getCollator). Once the client has that
         * information, it can put the name into the right bucket, and sort it within that bucket, without having access to
         * the index or collator.
         * <p>
         * Note that the bucket number (and sort key) are only valid for the settings of the current AlphabeticIndex; if
         * those are changed, then the bucket number and sort key must be regenerated.
         *
         * @param name
         *            Name, such as a name
         * @return the bucket index for the name
         * @stable ICU 4.8
         */
        public int GetBucketIndex(ICharSequence name)
        {
            InitBuckets();
            return buckets.GetBucketIndex(name, collatorPrimaryOnly);
        }

        /**
         * Clear the index.
         *
         * @return this, for chaining
         * @stable ICU 4.8
         */
        public AlphabeticIndex<T> ClearRecords()
        {
            if (inputList != null && inputList.Count > 0)
            {
                inputList.Clear();
                buckets = null;
            }
            return this;
        }

        /**
         * Return the number of buckets in the index. This will be the same as the number of labels, plus buckets for the underflow, overflow, and inflow(s).
         *
         * @return number of buckets
         * @stable ICU 4.8
         */
        public int BucketCount
        {
            get
            {
                InitBuckets();
                return buckets.BucketCount;
            }
        }

        /**
         * Return the number of records in the index: that is, the total number of distinct &lt;name,data&gt; pairs added with addRecord(...), over all the buckets.
         *
         * @return total number of records in buckets
         * @stable ICU 4.8
         */
        public int RecordCount
        {
            get { return inputList != null ? inputList.Count : 0; }
        }

        /**
         * Return an iterator over the buckets.
         *
         * @return iterator over buckets.
         * @stable ICU 4.8
         */
        public IEnumerator<Bucket> GetEnumerator()
        {
            InitBuckets();
            return buckets.GetEnumerator();
        }

        #region .NET Compatibility
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        /**
         * Creates an index, and buckets and sorts the list of records into the index.
         */
        private void InitBuckets()
        {
            if (buckets != null)
            {
                return;
            }
            buckets = CreateBucketList();
            if (inputList == null || inputList.Count == 0)
            {
                return;
            }

            // Sort the records by name.
            // Stable sort preserves input order of collation duplicates.
            inputList.Sort(recordComparator);

            // Now, we traverse all of the input, which is now sorted.
            // If the item doesn't go in the current bucket, we find the next bucket that contains it.
            // This makes the process order n*log(n), since we just sort the list and then do a linear process.
            // However, if the user adds an item at a time and then gets the buckets, this isn't efficient, so
            // we need to improve it for that case.

            IEnumerator<Bucket> bucketIterator = buckets.GetFullEnumerator();
            bucketIterator.MoveNext();
            Bucket currentBucket = bucketIterator.Current;
            Bucket nextBucket;
            string upperBoundary;
            if (bucketIterator.MoveNext())
            {
                nextBucket = bucketIterator.Current;
                upperBoundary = nextBucket.LowerBoundary;
            }
            else
            {
                nextBucket = null;
                upperBoundary = null;
            }
            foreach (Record r in inputList)
            {
                // if the current bucket isn't the right one, find the one that is
                // We have a special flag for the last bucket so that we don't look any further
                while (upperBoundary != null &&
                        collatorPrimaryOnly.Compare(r.Name, upperBoundary) >= 0)
                {
                    currentBucket = nextBucket;
                    // now reset the boundary that we compare against
                    if (bucketIterator.MoveNext())
                    {
                        nextBucket = bucketIterator.Current;
                        upperBoundary = nextBucket.LowerBoundary;
                    }
                    else
                    {
                        upperBoundary = null;
                    }
                }
                // now put the record into the bucket.
                Bucket bucket = currentBucket;
                if (bucket.DisplayBucket != null)
                {
                    bucket = bucket.DisplayBucket;
                }
                if (bucket.Records == null)
                {
                    bucket.Records = new List<Record>();
                }
                bucket.Records.Add(r);
            }
        }

        private int maxLabelCount = 99;

        /**
         * Returns true if one index character string is "better" than the other.
         * Shorter NFKD is better, and otherwise NFKD-binary-less-than is
         * better, and otherwise binary-less-than is better.
         */
        private static bool IsOneLabelBetterThanOther(Normalizer2 nfkdNormalizer, string one, string other)
        {
            // This is called with primary-equal strings, but never with one.equals(other).
            string n1 = nfkdNormalizer.Normalize(one);
            string n2 = nfkdNormalizer.Normalize(other);
            int result = n1.CodePointCount(0, n1.Length) - n2.CodePointCount(0, n2.Length);
            if (result != 0)
            {
                return result < 0;
            }
            result = binaryCmp.Compare(n1, n2);
            if (result != 0)
            {
                return result < 0;
            }
            return binaryCmp.Compare(one, other) < 0;
        }

        /**
         * A (name, data) pair, to be sorted by name into one of the index buckets.
         * The user data is not used by the index implementation.
         *
         * @stable ICU 4.8
         */
        public class Record
        {
            private readonly ICharSequence name;
            private readonly T data;




            internal Record(ICharSequence name, T data)
            {
                this.name = name;
                this.data = data;
            }

            /**
             * Get the name
             *
             * @return the name
             * @stable ICU 4.8
             */
            public virtual ICharSequence Name
            {
                get { return name; }
            }

            /**
             * Get the data
             *
             * @return the data
             * @stable ICU 4.8
             */
            public virtual T Data
            {
                get { return data; }
            }

            /**
             * Standard toString()
             * @stable ICU 4.8
             */
            public override string ToString()
            {
                return name + "=" + data;
            }
        }

        /**
 * Type of the label
 *
 * @stable ICU 4.8
 */
        public enum BucketLabelType
        {
            /**
             * Normal
             * @stable ICU 4.8
             */
            NORMAL,
            /**
             * Underflow (before the first)
             * @stable ICU 4.8
             */
            UNDERFLOW,
            /**
             * Inflow (between scripts)
             * @stable ICU 4.8
             */
            INFLOW,
            /**
             * Overflow (after the last)
             * @stable ICU 4.8
             */
            OVERFLOW
        }

        /**
         * An index "bucket" with a label string and type.
         * It is referenced by {@link AlphabeticIndex#getBucketIndex(CharSequence)}
         * and {@link AlphabeticIndex.ImmutableIndex#getBucketIndex(CharSequence)},
         * returned by {@link AlphabeticIndex.ImmutableIndex#getBucket(int)},
         * and {@link AlphabeticIndex#addRecord(CharSequence, Object)} adds a record
         * into a bucket according to the record's name.
         *
         * @param <V>
         *            Data type
         * @stable ICU 4.8
         */
        public class Bucket : IEnumerable<Record>
        {
            private readonly string label;
            private readonly string lowerBoundary;
            private readonly BucketLabelType labelType;
            private Bucket displayBucket;
            private int displayIndex;
            private IList<Record> records;

            internal string LowerBoundary
            {
                get { return lowerBoundary; }
            }

            internal Bucket DisplayBucket
            {
                get { return displayBucket; }
                set { displayBucket = value; }
            }

            internal int DisplayIndex
            {
                get { return displayIndex; }
                set { displayIndex = value; }
            }

            internal IList<Record> Records
            {
                get { return records; }
                set { records = value; }
            }

            /**
             * Set up the bucket.
             *
             * @param label
             *            label for the bucket
             * @param labelType
             *            is an underflow, overflow, or inflow bucket
             * @stable ICU 4.8
             */
            internal Bucket(string label, string lowerBoundary, BucketLabelType labelType)
            {
                this.label = label;
                this.lowerBoundary = lowerBoundary;
                this.labelType = labelType;
            }

            /**
             * Get the label
             *
             * @return label for the bucket
             * @stable ICU 4.8
             */
            public virtual string Label
            {
                get { return label; }
            }

            /**
             * Is a normal, underflow, overflow, or inflow bucket
             *
             * @return is an underflow, overflow, or inflow bucket
             * @stable ICU 4.8
             */
            public BucketLabelType LabelType
            {
                get { return labelType; }
            }

            /**
             * Get the number of records in the bucket.
             *
             * @return number of records in bucket
             * @stable ICU 4.8
             */
            public virtual int Count
            {
                get { return records == null ? 0 : records.Count; }
            }

            /**
             * Iterator over the records in the bucket
             * @stable ICU 4.8
             */
            public virtual IEnumerator<Record> GetEnumerator()
            {
                if (records == null)
                {
                    return new List<Record>.Enumerator();
                }
                return records.GetEnumerator();
            }

            #region .NET Compatibility
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            #endregion

            /**
             * Standard toString()
             * @stable ICU 4.8
             */
            public override string ToString()
            {
                return "{" +
                "labelType=" + labelType
                + ", " +
                "lowerBoundary=" + lowerBoundary
                + ", " +
                "label=" + label
                + "}"
                ;
            }


        }

        private BucketList CreateBucketList()
        {
            // Initialize indexCharacters.
            IList<string> indexCharacters = InitLabels();

            // Variables for hasMultiplePrimaryWeights().
            long variableTop;
            if (collatorPrimaryOnly.IsAlternateHandlingShifted)
            {
                variableTop = collatorPrimaryOnly.VariableTop & 0xffffffffL;
            }
            else
            {
                variableTop = 0;
            }
            bool hasInvisibleBuckets = false;

            // Helper arrays for Chinese Pinyin collation.
            Bucket[] asciiBuckets = new Bucket[26];
            Bucket[] pinyinBuckets = new Bucket[26];
            bool hasPinyin = false;

            List<Bucket> bucketList = new List<Bucket>
            {

                // underflow bucket
                new Bucket(UnderflowLabel, "", BucketLabelType.UNDERFLOW)
            };

            // fix up the list, adding underflow, additions, overflow
            // Insert inflow labels as needed.
            int scriptIndex = -1;
            string scriptUpperBoundary = "";
            foreach (string current in indexCharacters)
            {
                if (collatorPrimaryOnly.Compare(current, scriptUpperBoundary) >= 0)
                {
                    // We crossed the script boundary into a new script.
                    string inflowBoundary = scriptUpperBoundary;
                    bool skippedScript = false;
                    for (; ; )
                    {
                        scriptUpperBoundary = firstCharsInScripts[++scriptIndex];
                        if (collatorPrimaryOnly.Compare(current, scriptUpperBoundary) < 0)
                        {
                            break;
                        }
                        skippedScript = true;
                    }
                    if (skippedScript && bucketList.Count > 1)
                    {
                        // We are skipping one or more scripts,
                        // and we are not just getting out of the underflow label.
                        bucketList.Add(new Bucket(InflowLabel, inflowBoundary,
                                BucketLabelType.INFLOW));
                    }
                }
                // Add a bucket with the current label.
                Bucket bucket = new Bucket(FixLabel(current), current, BucketLabelType.NORMAL);
                bucketList.Add(bucket);
                // Remember ASCII and Pinyin buckets for Pinyin redirects.
                char c;
                if (current.Length == 1 && 'A' <= (c = current[0]) && c <= 'Z')
                {
                    asciiBuckets[c - 'A'] = bucket;
                }
                else if (current.Length == BASE.Length + 1 && current.StartsWith(BASE, StringComparison.Ordinal) &&
                      'A' <= (c = current[BASE.Length]) && c <= 'Z')
                {
                    pinyinBuckets[c - 'A'] = bucket;
                    hasPinyin = true;
                }
                // Check for multiple primary weights.
                if (!current.StartsWith(BASE, StringComparison.Ordinal) &&
                        HasMultiplePrimaryWeights(collatorPrimaryOnly, variableTop, current) &&
                        !current.EndsWith("\uffff", StringComparison.Ordinal))
                {
                    // "Æ" or "Sch" etc.
                    for (int i2 = bucketList.Count - 2; ; --i2)
                    {
                        Bucket singleBucket = bucketList[i2];
                        if (singleBucket.LabelType != BucketLabelType.NORMAL)
                        {
                            // There is no single-character bucket since the last
                            // underflow or inflow label.
                            break;
                        }
                        if (singleBucket.DisplayBucket == null &&
                                !HasMultiplePrimaryWeights(collatorPrimaryOnly, variableTop, singleBucket.LowerBoundary))
                        {
                            // Add an invisible bucket that redirects strings greater than the expansion
                            // to the previous single-character bucket.
                            // For example, after ... Q R S Sch we add Sch\uFFFF->S
                            // and after ... Q R S Sch Sch\uFFFF St we add St\uFFFF->S.
                            bucket = new Bucket("", current + "\uFFFF", BucketLabelType.NORMAL);
                            bucket.DisplayBucket = singleBucket;
                            bucketList.Add(bucket);
                            hasInvisibleBuckets = true;
                            break;
                        }
                    }
                }
            }
            if (bucketList.Count == 1)
            {
                // No real labels, show only the underflow label.
                return new BucketList(bucketList, bucketList);
            }
            // overflow bucket
            bucketList.Add(new Bucket(OverflowLabel, scriptUpperBoundary, BucketLabelType.OVERFLOW)); // final

            if (hasPinyin)
            {
                // Redirect Pinyin buckets.
                Bucket asciiBucket = null;
                for (int i3 = 0; i3 < 26; ++i3)
                {
                    if (asciiBuckets[i3] != null)
                    {
                        asciiBucket = asciiBuckets[i3];
                    }
                    if (pinyinBuckets[i3] != null && asciiBucket != null)
                    {
                        pinyinBuckets[i3].DisplayBucket = asciiBucket;
                        hasInvisibleBuckets = true;
                    }
                }
            }

            if (!hasInvisibleBuckets)
            {
                return new BucketList(bucketList, bucketList);
            }
            // Merge inflow buckets that are visually adjacent.
            // Iterate backwards: Merge inflow into overflow rather than the other way around.
            int i = bucketList.Count - 1;
            Bucket nextBucket = bucketList[i];
            while (--i > 0)
            {
                Bucket bucket = bucketList[i];
                if (bucket.DisplayBucket != null)
                {
                    continue;  // skip invisible buckets
                }
                if (bucket.LabelType == BucketLabelType.INFLOW)
                {
                    if (nextBucket.LabelType != BucketLabelType.NORMAL)
                    {
                        bucket.DisplayBucket = nextBucket;
                        continue;
                    }
                }
                nextBucket = bucket;
            }

            List<Bucket> publicBucketList = new List<Bucket>();
            foreach (Bucket bucket in bucketList)
            {
                if (bucket.DisplayBucket == null)
                {
                    publicBucketList.Add(bucket);
                }
            }
            return new BucketList(bucketList, publicBucketList);
        }

        internal class BucketList : IEnumerable<Bucket>
        {
            private readonly List<Bucket> bucketList;
            private readonly IList<Bucket> immutableVisibleList;

            public IList<Bucket> ImmutableVisibleList
            {
                get { return immutableVisibleList; }
            }

            internal BucketList(List<Bucket> bucketList, List<Bucket> publicBucketList)
            {
                this.bucketList = bucketList;

                int displayIndex = 0;
                foreach (Bucket bucket in publicBucketList)
                {
                    bucket.DisplayIndex = displayIndex++;
                }
                immutableVisibleList = publicBucketList.ToUnmodifiableList();
            }

            internal int BucketCount
            {
                get { return immutableVisibleList.Count; }
            }

            internal int GetBucketIndex(ICharSequence name, Collator collatorPrimaryOnly)
            {
                // binary search
                int start = 0;
                int limit = bucketList.Count;
                while ((start + 1) < limit)
                {
                    int i = (start + limit) / 2;
                    Bucket bucket = bucketList[i];
                    int nameVsBucket = collatorPrimaryOnly.Compare(name, bucket.LowerBoundary);
                    if (nameVsBucket < 0)
                    {
                        limit = i;
                    }
                    else
                    {
                        start = i;
                    }
                }
                Bucket bucket2 = bucketList[start];
                if (bucket2.DisplayBucket != null)
                {
                    bucket2 = bucket2.DisplayBucket;
                }
                return bucket2.DisplayIndex;
            }

            /**
             * Private iterator over all the buckets, visible and invisible
             */
            internal IEnumerator<Bucket> GetFullEnumerator()
            {
                return bucketList.GetEnumerator();
            }

            /**
             * Iterator over just the visible buckets.
             */
            public virtual IEnumerator<Bucket> GetEnumerator()
            {
                return immutableVisibleList.GetEnumerator(); // use immutable list to prevent remove().
            }

            #region .NET Compatibility
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            #endregion
        }

        private static bool HasMultiplePrimaryWeights(
                RuleBasedCollator coll, long variableTop, string s)
        {
            long[] ces = coll.InternalGetCEs(s.ToCharSequence());
            bool seenPrimary = false;
            for (int i = 0; i < ces.Length; ++i)
            {
                long ce = ces[i];
                long p = ce.TripleShift(32);
                if (p > variableTop)
                {
                    // not primary ignorable
                    if (seenPrimary)
                    {
                        return true;
                    }
                    seenPrimary = true;
                }
            }
            return false;
        }

        // TODO: Surely we have at least a ticket for porting these mask values to UCharacter.java?!
        private static readonly int GC_LU_MASK = 1 << UnicodeCategory.UppercaseLetter.ToIcuValue();
        private static readonly int GC_LL_MASK = 1 << UnicodeCategory.LowercaseLetter.ToIcuValue();
        private static readonly int GC_LT_MASK = 1 << UnicodeCategory.TitlecaseLetter.ToIcuValue();
        private static readonly int GC_LM_MASK = 1 << UnicodeCategory.ModifierLetter.ToIcuValue();
        private static readonly int GC_LO_MASK = 1 << UnicodeCategory.OtherLetter.ToIcuValue();
        private static readonly int GC_L_MASK =
                GC_LU_MASK | GC_LL_MASK | GC_LT_MASK | GC_LM_MASK | GC_LO_MASK;
        private static readonly int GC_CN_MASK = 1 << UnicodeCategory.OtherNotAssigned.ToIcuValue();

        /**
         * Return a list of the first character in each script. Only exposed for testing.
         *
         * @return list of first characters in each script
         * @internal
         * @deprecated This API is ICU internal, only for testing.
         */
        [Obsolete("This API is ICU internal, only for testing.")]
        public List<string> GetFirstCharactersInScripts()
        {
            List<string> dest = new List<string>(200);
            // Fetch the script-first-primary contractions which are defined in the root collator.
            // They all start with U+FDD1.
            UnicodeSet set = new UnicodeSet();
            collatorPrimaryOnly.InternalAddContractions(0xFDD1, set);
            if (set.IsEmpty)
            {
                throw new NotSupportedException(
                        "AlphabeticIndex requires script-first-primary contractions");
            }
            foreach (string boundary in set)
            {
                int gcMask = 1 << UCharacter.GetType(boundary.CodePointAt(1)).ToIcuValue();
                if ((gcMask & (GC_L_MASK | GC_CN_MASK)) == 0)
                {
                    // Ignore boundaries for the special reordering groups.
                    // Take only those for "real scripts" (where the sample character is a Letter,
                    // and the one for unassigned implicit weights (Cn).
                    continue;
                }
                dest.Add(boundary);
            }
            return dest;
        }
    }
}
