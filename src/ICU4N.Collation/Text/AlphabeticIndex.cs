using ICU4N.Globalization;
using ICU4N.Support;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using ICU4N.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// <see cref="AlphabeticIndex{T}"/> supports the creation of a UI index appropriate for a given language.
    /// It can support either direct use, or use with a client that doesn't support localized collation.
    /// </summary>
    /// <remarks>
    /// The following is an example of what an index might look like in a UI:
    /// <code>
    ///  <b>... A B C D E F G H I J K L M N O P Q R S T U V W X Y Z  ...</b>
    ///  
    ///  <b>A</b>
    ///     Addison
    ///     Albertson
    ///     Azensky
    ///  <b>B</b>
    ///     Baecker
    ///  ...
    /// </code>
    /// <para/>
    /// The class can generate a list of labels for use as a UI "index", that is, a list of
    /// clickable characters (or character sequences) that allow the user to see a segment
    /// (bucket) of a larger "target" list. That is, each label corresponds to a bucket in
    /// the target list, where everything in the bucket is greater than or equal to the character
    /// (according to the locale's collation). Strings can be added to the index;
    /// they will be in sorted order in the right bucket.
    /// <para/>
    /// The class also supports having buckets for strings before the first (underflow),
    /// after the last (overflow), and between scripts (inflow). For example, if the index
    /// is constructed with labels for Russian and English, Greek characters would fall
    /// into an inflow bucket between the other two scripts.
    /// <para/>
    /// <em>Note:</em> If you expect to have a lot of ASCII or Latin characters
    /// as well as characters from the user's language,
    /// then it is a good idea to call <see cref="AddLabels(ULocale[])"/> with <see cref="ULocale.ENGLISH"/>.
    /// <h2>Direct Use</h2>
    /// The following shows an example of building an index directly.
    /// The "show..." methods below are just to illustrate usage.
    /// <code>
    /// // Create a simple index where the values for the strings are Integers, and add the strings
    /// 
    /// AlphabeticIndex&lt;int&gt; index = new AlphabeticIndex&lt;int&gt;(desiredLocale).AddLabels(additionalLocale);
    /// int counter = 0;
    /// foreach (string item in test)
    /// {
    ///     index.AddRecord(item, counter++);
    /// }
    /// ...
    /// // Show index at top. We could skip or gray out empty buckets
    /// 
    /// foreach (AlphabeticIndex&lt;int&gt;.Bucket bucket in index)
    /// {
    ///     if (showAll || bucket.Count != 0)
    ///     {
    ///         ShowLabelAtTop(UI, bucket.Label);
    ///     }
    /// }
    /// ...
    /// // Show the buckets with their contents, skipping empty buckets
    /// 
    /// foreach (AlphabeticIndex&lt;int&gt;.Bucket bucket in index)
    /// {
    ///     if (bucket.Count != 0)
    ///     {
    ///         ShowLabelInList(UI, bucket.Label);
    ///         foreach (AlphabeticIndex&lt;int&gt;.Record item in bucket)
    ///         {
    ///             ShowIndexedItem(UI, item.Name, item.Data);
    ///         }
    ///     }
    /// }
    /// </code>
    /// The caller can build different UIs using this class.
    /// For example, an index character could be omitted or grayed-out
    /// if its bucket is empty. Small buckets could also be combined based on size, such as:
    /// <code>
    /// <b>... A-F G-N O-Z ...</b>
    /// </code>
    /// <h2>Client Support</h2>
    /// Callers can also use the <see cref="ImmutableIndex{T}"/>, or the <see cref="AlphabeticIndex{T}"/> itself,
    /// to support sorting on a client that doesn't support <see cref="AlphabeticIndex{T}"/> functionality.
    /// <para/>
    /// The <see cref="ImmutableIndex{T}"/> is both immutable and thread-safe.
    /// The corresponding <see cref="AlphabeticIndex{T}"/> methods are not thread-safe because
    /// they "lazily" build the index buckets.
    /// <list type="bullet">
    ///     <item><description>
    ///         <see cref="ImmutableIndex{T}.GetBucket(int)"/> provides random access to all
    ///         buckets and their labels and label types.
    ///     </description></item>
    ///     <item><description>
    ///         <see cref="AlphabeticIndex{T}.GetBucketLabels()"/> or the bucket iterator on either class
    ///         can be used to get a list of the labels,
    ///         such as "...", "A", "B",..., and send that list to the client.
    ///     </description></item>
    ///     <item><description>
    ///         When the client has a new name, it sends that name to the server.
    ///         The server needs to call the following methods,
    ///         and communicate the bucketIndex and collationKey back to the client.
    ///         
    ///         <code>
    ///         int bucketIndex = index.GetBucketIndex(name);
    ///         string label = immutableIndex.GetBucket(bucketIndex).Label;  // optional
    ///         RawCollationKey collationKey = collator.GetRawCollationKey(name, null);
    ///         </code>
    ///         
    ///     </description></item>
    ///     <item><description>
    ///         The client would put the name (and associated information) into its bucket for bucketIndex. The collationKey is a
    ///         sequence of bytes that can be compared with a binary compare, and produce the right localized result.
    ///     </description></item>
    /// </list>
    /// </remarks>
    /// <typeparam name="T">Data type of bucket data.</typeparam>
    /// <author>Mark Davis</author>
    /// <stable>ICU 4.8</stable>
    public sealed class AlphabeticIndex<T> : IEnumerable<Bucket<T>>
    {
        /// <summary>
        /// Prefix string for Chinese index buckets.
        /// See http://unicode.org/repos/cldr/trunk/specs/ldml/tr35-collation.html#Collation_Indexes
        /// </summary>
        private static readonly string BASE = "\uFDD0";

        private static readonly char CGJ = '\u034F';

        private static readonly IComparer<string> binaryCmp = new UTF16.StringComparer(true, false, 0);

        private readonly RuleBasedCollator collatorOriginal;
        private readonly RuleBasedCollator collatorPrimaryOnly;
        private RuleBasedCollator collatorExternal;

        private class RecordComparer : IComparer<Record<T>>
        {
            private readonly RuleBasedCollator collatorOriginal;

            public RecordComparer(RuleBasedCollator collatorOriginal)
            {
                this.collatorOriginal = collatorOriginal;
            }

            public int Compare(Record<T> o1, Record<T> o2)
            {
                return collatorOriginal.Compare(o1.Name, o2.Name);
            }
        }

        // Comparer for records, so that the Record class can be static.
        private readonly IComparer<Record<T>> recordComparer;


        private readonly List<string> firstCharsInScripts;

        // We accumulate these as we build up the input parameters
        private readonly UnicodeSet initialLabels = new UnicodeSet();
        private List<Record<T>> inputList;

        // Lazy evaluated: null means that we have not built yet.
        private BucketList buckets;

        private string overflowLabel = "\u2026";
        private string underflowLabel = "\u2026";
        private string inflowLabel = "\u2026";

        // ICU4N specific - de-nested ImmutableIndex

        /// <summary>
        /// Create the index object.
        /// </summary>
        /// <param name="locale">The locale for the index.</param>
        /// <stable>ICU 4.8</stable>
        public AlphabeticIndex(ULocale locale)
            : this(locale, null)
        {
        }

        /// <summary>
        /// Create the index object.
        /// </summary>
        /// <param name="locale">The locale for the index.</param>
        /// <stable>ICU 4.8</stable>
        public AlphabeticIndex(CultureInfo locale)
            : this(ULocale.ForLocale(locale), null)
        {
        }

        /// <summary>
        /// Create an <see cref="AlphabeticIndex{T}"/> that uses a specific collator.
        /// <para/>
        /// The index will be created with no labels; the <see cref="AddLabels(CultureInfo[])"/> function (or overload) must be called
        /// after creation to add the desired labels to the index.
        /// <para/>
        /// The index will work directly with the supplied collator. If the caller will need to
        /// continue working with the collator it should be cloned first, so that the
        /// collator provided to the <see cref="AlphabeticIndex{T}"/> remains unchanged after creation of the index.
        /// </summary>
        /// <param name="collator">The collator to use to order the contents of this index.</param>
        /// <stable>ICU 51</stable>
        public AlphabeticIndex(RuleBasedCollator collator)
            : this(null, collator)
        {
        }

        /// <summary>
        /// Internal constructor containing implementation used by public constructors.
        /// </summary>
        private AlphabeticIndex(ULocale locale, RuleBasedCollator collator)
        {
            collatorOriginal = collator != null ? collator : (RuleBasedCollator)Text.Collator.GetInstance(locale);
            // ICU4N specific - we neeed to initialize RecordComparer in the constructor, since it 
            // references a local variable that is initialized above.
            recordComparer = new RecordComparer(collatorOriginal);
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

#pragma warning disable 612, 618
            firstCharsInScripts = GetFirstCharactersInScripts();
#pragma warning restore 612, 618
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

        /// <summary>
        /// Add more index characters (aside from what are in the locale)
        /// </summary>
        /// <param name="additions">Additional characters to add to the index, such as A-Z.</param>
        /// <returns>This, for chaining.</returns>
        /// <stable>ICU 4.8</stable>
        public AlphabeticIndex<T> AddLabels(UnicodeSet additions)
        {
            initialLabels.AddAll(additions);
            buckets = null;
            return this;
        }

        /// <summary>
        /// Add more index characters (aside from what are in the locale)
        /// </summary>
        /// <param name="additions">Additional characters to add to the index, such as those in Swedish.</param>
        /// <returns>This, for chaining.</returns>
        /// <stable>ICU 4.8</stable>
        public AlphabeticIndex<T> AddLabels(params ULocale[] additions)
        {
            foreach (ULocale addition in additions)
            {
                AddIndexExemplars(addition);
            }
            buckets = null;
            return this;
        }

        /// <summary>
        /// Add more index characters (aside from what are in the locale)
        /// </summary>
        /// <param name="additions">Additional characters to add to the index, such as those in Swedish.</param>
        /// <returns>This, for chaining.</returns>
        /// <stable>ICU 4.8</stable>
        public AlphabeticIndex<T> AddLabels(params CultureInfo[] additions)
        {
            foreach (var addition in additions)
            {
                AddIndexExemplars(ULocale.ForLocale(addition));
            }
            buckets = null;
            return this;
        }

        /// <summary>
        /// Set the overflow label.
        /// </summary>
        /// <param name="overflowLabel">See <see cref="AlphabeticIndex{T}"/> class description.</param>
        /// <returns>This, for chaining.</returns>
        /// <stable>ICU 4.8</stable>
        public AlphabeticIndex<T> SetOverflowLabel(string overflowLabel)
        {
            this.overflowLabel = overflowLabel;
            buckets = null;
            return this;
        }

        /// <summary>
        /// Get the default label used in the IndexCharacters' locale for underflow, eg the last item in: X Y Z ...
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public string UnderflowLabel
        {
            get { return underflowLabel; } // TODO get localized version
        }

        /// <summary>
        /// Set the underflowLabel label.
        /// </summary>
        /// <param name="underflowLabel">See <see cref="AlphabeticIndex{T}"/> class description.</param>
        /// <returns>This, for chaining.</returns>
        /// <stable>ICU 4.8</stable>
        public AlphabeticIndex<T> SetUnderflowLabel(string underflowLabel) // ICU4N TODO: API - make extension method ?
        {
            this.underflowLabel = underflowLabel;
            buckets = null;
            return this;
        }

        /// <summary>
        /// Get the default label used in the IndexCharacters' locale for overflow, eg the first item in: ... A B C
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public string OverflowLabel
        {
            get { return overflowLabel; } // TODO get localized version
        }

        /// <summary>
        /// Set the inflowLabel label.
        /// </summary>
        /// <param name="inflowLabel">See <see cref="AlphabeticIndex{T}"/> class description.</param>
        /// <returns>This, for chaining.</returns>
        /// <stable>ICU 4.8</stable>
        public AlphabeticIndex<T> SetInflowLabel(string inflowLabel)
        {
            this.inflowLabel = inflowLabel;
            buckets = null;
            return this;
        }

        /// <summary>
        /// Get the default label used for abbreviated buckets <i>between</i> other labels. For example, consider the labels
        /// for Latin and Greek are used: X Y Z ... &#x0391; &#x0392; &#x0393;.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public string InflowLabel
        {
            get { return inflowLabel; } // TODO get localized version
        }

        /// <summary>
        /// Get the limit on the number of labels in the index. The number of buckets can be slightly larger: see <see cref="BucketCount"/>.
        /// Returns maximum number of labels.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public int MaxLabelCount
        {
            get { return maxLabelCount; }
        }

        /// <summary>
        /// Set a limit on the number of labels in the index. The number of buckets can be slightly larger: see
        /// <see cref="BucketCount"/>.
        /// </summary>
        /// <param name="maxLabelCount">
        /// Set the maximum number of labels. Currently, if the number is exceeded, then every
        /// nth item is removed to bring the count down. A more sophisticated mechanism may be available in the
        /// future.
        /// </param>
        /// <returns>This, for chaining.</returns>
        /// <stable>ICU 4.8</stable>
        public AlphabeticIndex<T> SetMaxLabelCount(int maxLabelCount)
        {
            this.maxLabelCount = maxLabelCount;
            buckets = null;
            return this;
        }

        /// <summary>
        /// Determine the best labels to use. This is based on the exemplars, but we also process to make sure that they are unique,
        /// and sort differently, and that the overall list is small enough.
        /// </summary>
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

        /// <summary>
        /// This method is called to get the index exemplars. Normally these come from the <paramref name="locale"/> directly,
        /// but if they aren't available, we have to synthesize them.
        /// </summary>
        private void AddIndexExemplars(ULocale locale)
        {
            UnicodeSet exemplars = LocaleData.GetExemplarSet(locale, 0, ExemplarSetType.Index);
            if (exemplars != null)
            {
                initialLabels.AddAll(exemplars);
                return;
            }

            // The locale data did not include explicit Index characters.
            // Synthesize a set of them from the locale's standard exemplar characters.
            exemplars = LocaleData.GetExemplarSet(locale, 0, ExemplarSetType.Standard);

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
                while (it.Next() && it.Codepoint != UnicodeSetIterator.IsString)
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
                initialLabels.Add(UChar.ToUpper(locale, item));
            }
        }

        /// <summary>
        /// Add Chinese index characters from the tailoring.
        /// </summary>
        private bool AddChineseIndexCharacters()
        {
            UnicodeSet contractions = new UnicodeSet();
            try // ICU4N TODO: Try to remove this catch block and use Try.. version of method
            {
                collatorPrimaryOnly.InternalAddContractions(BASE[0], contractions);
            }
            catch (Exception)
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

        /// <summary>
        /// Return the string with interspersed CGJs. Input must have more than 2 codepoints.
        /// <para/>
        /// This is used to test whether contractions sort differently from their components.
        /// </summary>
        private string Separated(string item)
        {
            StringBuilder result = new StringBuilder();
            // add a CGJ except within surrogates
            char last = item[0];
            result.Append(last);
            for (int i = 1; i < item.Length; ++i)
            {
                char ch = item[i];
                if (!UChar.IsHighSurrogate(last) || !UChar.IsLowSurrogate(ch))
                {
                    result.Append(CGJ);
                }
                result.Append(ch);
                last = ch;
            }
            return result.ToString();
        }

        /// <summary>
        /// Builds an immutable, thread-safe version of this instance, without data records.
        /// </summary>
        /// <returns>An immutable index instance.</returns>
        /// <stable>ICU 51</stable>
        public ImmutableIndex<T> BuildImmutableIndex()
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
            return new ImmutableIndex<T>(immutableBucketList, collatorPrimaryOnly);
        }

        /// <summary>
        /// Get the labels.
        /// </summary>
        /// <returns>The list of bucket labels, after processing.</returns>
        /// <stable>ICU 4.8</stable>
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

        /// <summary>
        /// Get a clone of the collator used internally. Note that for performance reasons, the clone is only done once, and
        /// then stored. The next time it is accessed, the same instance is returned.
        /// <para/>
        /// <b><i>Don't use this property across threads if you are changing the settings on the collator, at least not without
        /// synchronizing.</i></b>
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public RuleBasedCollator Collator
        {
            get
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
        }

        /// <summary>
        /// Add a record (name and data) to the index. The name will be used to sort the items into buckets, and to sort
        /// within the bucket. Two records may have the same name. When they do, the sort order is according to the order added:
        /// the first added comes first.
        /// </summary>
        /// <param name="name">Name, such as a name.</param>
        /// <param name="data">Data, such as an address or link.</param>
        /// <returns>this, for chaining.</returns>
        /// <stable>ICU 4.8</stable>
        public AlphabeticIndex<T> AddRecord(string name, T data) // ICU4N specific - changed name from ICharSequence to string
        {
            // TODO instead of invalidating, just add to unprocessed list.
            buckets = null; // invalidate old bucketlist
            if (inputList == null)
            {
                inputList = new List<Record<T>>();
            }
            inputList.Add(new Record<T>(name, data));
            return this;
        }

        /// <summary>
        /// Get the bucket number for the given name. This routine permits callers to implement their own bucket handling
        /// mechanisms, including client-server handling. For example, when a new name is created on the client, it can ask
        /// the server for the bucket for that name, and the sortkey (using <see cref="Collator"/>). Once the client has that
        /// information, it can put the name into the right bucket, and sort it within that bucket, without having access to
        /// the index or collator.
        /// <para/>
        /// Note that the bucket number (and sort key) are only valid for the settings of the current <see cref="AlphabeticIndex{T}"/>; if
        /// those are changed, then the bucket number and sort key must be regenerated.
        /// </summary>
        /// <param name="name">Name, such as a name.</param>
        /// <returns>The bucket index for the name.</returns>
        /// <stable>ICU 4.8</stable>
        public int GetBucketIndex(string name) // ICU4N specific - changed name from ICharSequence to string
        {
            InitBuckets();
            return buckets.GetBucketIndex(name, collatorPrimaryOnly);
        }

        /// <summary>
        /// Clear the index.
        /// </summary>
        /// <returns>This, for chaining.</returns>
        /// <stable>ICU 4.8</stable>
        public AlphabeticIndex<T> ClearRecords()
        {
            if (inputList != null && inputList.Count > 0)
            {
                inputList.Clear();
                buckets = null;
            }
            return this;
        }

        /// <summary>
        /// Gets the number of buckets in the index. This will be the same as the number of labels, plus buckets for the underflow, overflow, and inflow(s).
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public int BucketCount
        {
            get
            {
                InitBuckets();
                return buckets.BucketCount;
            }
        }

        /// <summary>
        /// Gets the number of records in the index: that is, the total number of distinct &lt;name,data&gt; pairs added with AddRecord(...), over all the buckets.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public int RecordCount
        {
            get { return inputList != null ? inputList.Count : 0; }
        }

        /// <summary>
        /// Return an enumerator over the buckets.
        /// </summary>
        /// <returns>Enumerator over buckets.</returns>
        /// <stable>ICU 4.8</stable>
        public IEnumerator<Bucket<T>> GetEnumerator()
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

        /// <summary>
        /// Creates an index, and buckets and sorts the list of records into the index.
        /// </summary>
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
            inputList.Sort(recordComparer);

            // Now, we traverse all of the input, which is now sorted.
            // If the item doesn't go in the current bucket, we find the next bucket that contains it.
            // This makes the process order n*log(n), since we just sort the list and then do a linear process.
            // However, if the user adds an item at a time and then gets the buckets, this isn't efficient, so
            // we need to improve it for that case.

            IEnumerator<Bucket<T>> bucketIterator = buckets.GetFullEnumerator();
            bucketIterator.MoveNext();
            Bucket<T> currentBucket = bucketIterator.Current;
            Bucket<T> nextBucket;
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
            foreach (Record<T> r in inputList)
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
                Bucket<T> bucket = currentBucket;
                if (bucket.DisplayBucket != null)
                {
                    bucket = bucket.DisplayBucket;
                }
                if (bucket.Records == null)
                {
                    bucket.Records = new List<Record<T>>();
                }
                bucket.Records.Add(r);
            }
        }

        private int maxLabelCount = 99;

        /// <summary>
        /// Returns true if one index character string is "better" than the other.
        /// Shorter NFKD is better, and otherwise NFKD-binary-less-than is
        /// better, and otherwise binary-less-than is better.
        /// </summary>
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

        // ICU4N specific - de-nested Record<T>

        // ICU4N specific - de-nested Bucket<T>

        private BucketList CreateBucketList()
        {
            // Initialize indexCharacters.
            IList<string> indexCharacters = InitLabels();

            // Variables for hasMultiplePrimaryWeights().
            long variableTop;
            if (collatorPrimaryOnly.IsAlternateHandlingShifted)
            {
#pragma warning disable 612, 618
                variableTop = collatorPrimaryOnly.VariableTop & 0xffffffffL;
#pragma warning restore 612, 618
            }
            else
            {
                variableTop = 0;
            }
            bool hasInvisibleBuckets = false;

            // Helper arrays for Chinese Pinyin collation.
            Bucket<T>[] asciiBuckets = new Bucket<T>[26];
            Bucket<T>[] pinyinBuckets = new Bucket<T>[26];
            bool hasPinyin = false;

            List<Bucket<T>> bucketList = new List<Bucket<T>>
            {

                // underflow bucket
                new Bucket<T>(UnderflowLabel, "", BucketLabelType.Underflow)
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
                        bucketList.Add(new Bucket<T>(InflowLabel, inflowBoundary,
                                BucketLabelType.Inflow));
                    }
                }
                // Add a bucket with the current label.
                Bucket<T> bucket = new Bucket<T>(FixLabel(current), current, BucketLabelType.Normal);
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
                        Bucket<T> singleBucket = bucketList[i2];
                        if (singleBucket.LabelType != BucketLabelType.Normal)
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
                            bucket = new Bucket<T>("", current + "\uFFFF", BucketLabelType.Normal);
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
            bucketList.Add(new Bucket<T>(OverflowLabel, scriptUpperBoundary, BucketLabelType.Overflow)); // final

            if (hasPinyin)
            {
                // Redirect Pinyin buckets.
                Bucket<T> asciiBucket = null;
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
            Bucket<T> nextBucket = bucketList[i];
            while (--i > 0)
            {
                Bucket<T> bucket = bucketList[i];
                if (bucket.DisplayBucket != null)
                {
                    continue;  // skip invisible buckets
                }
                if (bucket.LabelType == BucketLabelType.Inflow)
                {
                    if (nextBucket.LabelType != BucketLabelType.Normal)
                    {
                        bucket.DisplayBucket = nextBucket;
                        continue;
                    }
                }
                nextBucket = bucket;
            }

            List<Bucket<T>> publicBucketList = new List<Bucket<T>>();
            foreach (Bucket<T> bucket in bucketList)
            {
                if (bucket.DisplayBucket == null)
                {
                    publicBucketList.Add(bucket);
                }
            }
            return new BucketList(bucketList, publicBucketList);
        }

        internal class BucketList : IEnumerable<Bucket<T>>
        {
            private readonly List<Bucket<T>> bucketList;
            private readonly IList<Bucket<T>> immutableVisibleList;

            public IList<Bucket<T>> ImmutableVisibleList
            {
                get { return immutableVisibleList; }
            }

            internal BucketList(List<Bucket<T>> bucketList, List<Bucket<T>> publicBucketList)
            {
                this.bucketList = bucketList;

                int displayIndex = 0;
                foreach (Bucket<T> bucket in publicBucketList)
                {
                    bucket.DisplayIndex = displayIndex++;
                }
                immutableVisibleList = publicBucketList.ToUnmodifiableList();
            }

            internal int BucketCount
            {
                get { return immutableVisibleList.Count; }
            }

            internal int GetBucketIndex(string name, Collator collatorPrimaryOnly) // ICU4N specific - changed name from ICharSequence to string
            {
                // binary search
                int start = 0;
                int limit = bucketList.Count;
                while ((start + 1) < limit)
                {
                    int i = (start + limit) / 2;
                    Bucket<T> bucket = bucketList[i];
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
                Bucket<T> bucket2 = bucketList[start];
                if (bucket2.DisplayBucket != null)
                {
                    bucket2 = bucket2.DisplayBucket;
                }
                return bucket2.DisplayIndex;
            }

            /// <summary>
            /// Private enumerator over all the buckets, visible and invisible
            /// </summary>
            internal IEnumerator<Bucket<T>> GetFullEnumerator()
            {
                return bucketList.GetEnumerator();
            }

            /// <summary>
            /// Enumerator over just the visible buckets.
            /// </summary>
            public virtual IEnumerator<Bucket<T>> GetEnumerator()
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
#pragma warning disable 612, 618
            long[] ces = coll.InternalGetCEs(s.ToCharSequence());
#pragma warning restore 612, 618
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
        private static readonly int GC_LU_MASK = 1 << UUnicodeCategory.UppercaseLetter.ToInt32();
        private static readonly int GC_LL_MASK = 1 << UUnicodeCategory.LowercaseLetter.ToInt32();
        private static readonly int GC_LT_MASK = 1 << UUnicodeCategory.TitlecaseLetter.ToInt32();
        private static readonly int GC_LM_MASK = 1 << UUnicodeCategory.ModifierLetter.ToInt32();
        private static readonly int GC_LO_MASK = 1 << UUnicodeCategory.OtherLetter.ToInt32();
        private static readonly int GC_L_MASK =
                GC_LU_MASK | GC_LL_MASK | GC_LT_MASK | GC_LM_MASK | GC_LO_MASK;
        private static readonly int GC_CN_MASK = 1 << UUnicodeCategory.OtherNotAssigned.ToInt32();

        /// <summary>
        /// Return a list of the first character in each script. Only exposed for testing.
        /// </summary>
        /// <returns>List of first characters in each script.</returns>
        /// <internal/>
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
                int gcMask = 1 << UChar.GetUnicodeCategory(boundary.CodePointAt(1)).ToInt32();
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

    /// <summary>
    /// Immutable, thread-safe version of <see cref="AlphabeticIndex{T}"/>.
    /// This class provides thread-safe methods for bucketing,
    /// and random access to buckets and their properties,
    /// but does not offer adding records to the index.
    /// </summary>
    /// <stable>ICU 51</stable>
    public sealed class ImmutableIndex<T> : IEnumerable<Bucket<T>>
    {
        internal readonly AlphabeticIndex<T>.BucketList buckets;
        internal readonly Collator collatorPrimaryOnly;

        internal ImmutableIndex(AlphabeticIndex<T>.BucketList bucketList, Collator collatorPrimaryOnly)
        {
            this.buckets = bucketList;
            this.collatorPrimaryOnly = collatorPrimaryOnly;
        }

        /// <summary>
        /// Gets the number of index buckets and labels, including underflow/inflow/overflow.
        /// </summary>
        /// <stable>ICU 51</stable>
        public int BucketCount
        {
            get { return buckets.BucketCount; }
        }

        /// <summary>
        /// Finds the index bucket for the given name and returns the number of that bucket.
        /// Use <see cref="GetBucket(int)"/> to get the bucket's properties.
        /// </summary>
        /// <param name="name">The string to be sorted into an index bucket.</param>
        /// <returns>The bucket number for the name.</returns>
        /// <stable>ICU 51</stable>
        public int GetBucketIndex(string name) // ICU4N specific - changed name from ICharSequence to string
        {
            return buckets.GetBucketIndex(name, collatorPrimaryOnly);
        }

        /// <summary>
        /// Returns the <paramref name="index"/>-th bucket. Returns null if the index is out of range.
        /// </summary>
        /// <param name="index">Bucket number.</param>
        /// <returns>The <paramref name="index"/>-th bucket.</returns>
        /// <stable>ICU 51</stable>
        public Bucket<T> GetBucket(int index)
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

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        /// <stable>ICU 51</stable>
        public IEnumerator<Bucket<T>> GetEnumerator()
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

    /// <summary>
    /// A (name, data) pair, to be sorted by name into one of the index buckets.
    /// The user data is not used by the index implementation.
    /// </summary>
    /// <stable>ICU 4.8</stable>
    public class Record<T>
    {
        private readonly string name;
        private readonly T data;

        internal Record(string name, T data) // ICU4N specific - changed name from ICharsequence to string
        {
            this.name = name;
            this.data = data;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public virtual string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public virtual T Data
        {
            get { return data; }
        }

        /// <summary>
        /// Returns <c>name + "=" + data</c>.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public override string ToString()
        {
            return name + "=" + data;
        }
    }

    /// <summary>
    /// An index "bucket" with a label string and type.
    /// It is referenced by <see cref="AlphabeticIndex{T}.GetBucketIndex(string)"/>
    /// and <see cref="ImmutableIndex{T}.GetBucketIndex(string)"/>,
    /// returned by <see cref="ImmutableIndex{T}.GetBucket(int)"/>,
    /// and <see cref="AlphabeticIndex{T}.AddRecord(string, T)"/> adds a record
    /// into a bucket according to the record's name.
    /// </summary>
    /// <stable>ICU 4.8</stable>
    public class Bucket<T> : IEnumerable<Record<T>>
    {
        private readonly string label;
        private readonly string lowerBoundary;
        private readonly BucketLabelType labelType;
        private Bucket<T> displayBucket;
        private int displayIndex;
        private IList<Record<T>> records;

        // ICU4N specific - de-nested LabelType enum and renamed BucketLabelType

        internal string LowerBoundary
        {
            get { return lowerBoundary; }
        }

        internal Bucket<T> DisplayBucket
        {
            get { return displayBucket; }
            set { displayBucket = value; }
        }

        internal int DisplayIndex
        {
            get { return displayIndex; }
            set { displayIndex = value; }
        }

        internal IList<Record<T>> Records
        {
            get { return records; }
            set { records = value; }
        }

        /// <summary>
        /// Set up the bucket.
        /// </summary>
        /// <param name="label">Label for the bucket.</param>
        /// <param name="lowerBoundary"></param>
        /// <param name="labelType">Is an underflow, overflow, or inflow bucket.</param>
        /// <stable>ICU 4.8</stable>
        internal Bucket(string label, string lowerBoundary, BucketLabelType labelType)
        {
            this.label = label;
            this.lowerBoundary = lowerBoundary;
            this.labelType = labelType;
        }

        /// <summary>
        /// Gets the label for the bucket.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public virtual string Label
        {
            get { return label; }
        }

        /// <summary>
        /// Is a normal, underflow, overflow, or inflow bucket?
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public BucketLabelType LabelType
        {
            get { return labelType; }
        }

        /// <summary>
        /// Gets the number of records in the bucket.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public virtual int Count
        {
            get { return records == null ? 0 : records.Count; }
        }

        /// <summary>
        /// Enumerator over the records in the bucket.
        /// </summary>
        /// <returns>An enumerator over the records in the bucket.</returns>
        /// <stable>ICU 4.8</stable>
        public virtual IEnumerator<Record<T>> GetEnumerator()
        {
            if (records == null)
            {
                return new List<Record<T>>().GetEnumerator();
            }
            return records.GetEnumerator();
        }

        #region .NET Compatibility
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        /// <summary>
        /// Returns a name with the <see cref="labelType"/>, <see cref="lowerBoundary"/>, and <see cref="label"/>.
        /// </summary>
        /// <stable>ICU 4.8</stable>
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

    /// <summary>
    /// Type of the label
    /// </summary>
    /// <stable>ICU 4.8</stable>
    public enum BucketLabelType
    {
        /// <summary>
        /// Normal
        /// </summary>
        /// <stable>ICU 4.8</stable>
        Normal,
        /// <summary>
        /// Underflow (before the first)
        /// </summary>
        /// <stable>ICU 4.8</stable>
        Underflow,
        /// <summary>
        /// Inflow (between scripts)
        /// </summary>
        /// <stable>ICU 4.8</stable>
        Inflow,
        /// <summary>
        /// Overflow (after the last)
        /// </summary>
        /// <stable>ICU 4.8</stable>
        Overflow
    }
}
