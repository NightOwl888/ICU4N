using ICU4N.Impl;
using ICU4N.Impl.Coll;
using ICU4N.Support;
using ICU4N.Support.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ICU4N.Text
{
    /// <summary>
    /// <see cref="CollationElementIterator"/> is an iterator created by
    /// a <see cref="Text.RuleBasedCollator"/> to walk through a string. The return result of
    /// each iteration is a 32-bit collation element (CE) that defines the
    /// ordering priority of the next character or sequence of characters
    /// in the source string.
    /// </summary>
    /// <remarks>
    /// For illustration, consider the following in Slovak and in traditional Spanish collation:
    /// <code>
    /// "ca" -&gt; the first collation element is CE('c') and the second
    ///         collation element is CE('a').
    /// "cha" -&gt; the first collation element is CE('ch') and the second
    ///         collation element is CE('a').
    /// </code>
    /// And in German phonebook collation,
    /// <code>
    /// Since the character '&#230;' is a composed character of 'a' and 'e', the
    /// iterator returns two collation elements for the single character '&#230;'
    /// 
    /// "&#230;b" -&gt; the first collation element is collation_element('a'), the
    ///              second collation element is collation_element('e'), and the
    ///              third collation element is collation_element('b').
    /// </code>
    /// <para/>
    /// For collation ordering comparison, the collation element results
    /// can not be compared simply by using basic arithmetic operators,
    /// e.g. &lt;, == or &gt;, further processing has to be done. Details
    /// can be found in the ICU
    /// <a href="http://userguide.icu-project.org/collation/architecture">
    /// User Guide</a>. An example of using the <see cref="CollationElementIterator"/>
    /// for collation ordering comparison is the class
    /// <see cref="StringSearch"/>.
    /// <para/>
    /// To construct a CollationElementIterator object, users
    /// call the method <see cref="RuleBasedCollator.GetCollationElementIterator(string)"/>
    /// that defines the desired sorting order.
    /// <example>
    /// <code>
    /// string testString = "This is a test";
    /// RuleBasedCollator rbc = new RuleBasedCollator("&amp;a&lt;b");
    /// CollationElementIterator iterator = rbc.GetCollationElementIterator(testString);
    /// int primaryOrder = iterator.Ignorable;
    /// while (primaryOrder != iterator.NullOrder)
    /// {
    ///     int order = iterator.Next();
    ///     if (order != iterator.Ignorable &amp;&amp; order != iterator.NullOrder)
    ///     {
    ///         // order is valid, not ignorable and we have not passed the end
    ///         // of the iteration, we do something
    ///         primaryOrder = CollationElementIterator.PrimaryOrder(order);
    ///         Console.WriteLine($"Next primary order 0x{primaryOrder.ToString("X")}");
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <para/>
    /// The method <see cref="Next()"/> returns the collation order of the next character based on
    /// the comparison level of the collator. The method <see cref="Previous()"/> returns the
    /// collation order of the previous character based on the comparison level of
    /// the collator. The <see cref="CollationElementIterator"/> moves only in one direction
    /// between calls to <see cref="Reset()"/>, <see cref="SetOffset(int)"/> or <see cref="SetText(string)"/>. That is, <see cref="Next()"/> and
    /// <see cref="Previous()"/> can not be inter-used. Whenever <see cref="Previous()"/> is to be called after
    /// <see cref="Next()"/> or vice versa, <see cref="Reset()"/>, <see cref="SetOffset(int)"/> or <see cref="SetText(string)"/> has to be called first
    /// to reset the status, shifting current position to either the end or the start of
    /// the string (<see cref="Reset()"/> or <see cref="SetText(string)"/>), or the specified position (<see cref="SetOffset(int)"/>).
    /// Hence at the next call of <see cref="Next()"/> or <see cref="Previous()"/>, the first or last collation order,
    /// or collation order at the specified position will be returned. If a change of
    /// direction is done without one of these calls, the result is undefined.
    /// <para/>
    /// This class cannot be inherited.
    /// </remarks>
    /// <seealso cref="Collator"/>
    /// <seealso cref="Text.RuleBasedCollator"/>
    /// <seealso cref="StringSearch"/>
    /// <author>Syn Wee Quek</author>
    /// <stable>ICU 2.8</stable>
    public sealed class CollationElementIterator
    {
        private CollationIterator iter_;  // owned
        private RuleBasedCollator rbc_;  // aliased
        private int otherHalf_;

        /// <summary>
        /// &lt;0: backwards; 0: just after <see cref="Reset()"/> (<see cref="Previous()"/> begins from end);
        /// 1: just after <see cref="SetOffset(int)"/>; >1: forward
        /// </summary>
        private sbyte dir_;

        /// <summary>
        /// Stores offsets from expansions and from unsafe-backwards iteration,
        /// so that <see cref="GetOffset()"/> returns intermediate offsets for the CEs
        /// that are consistent with forward iteration.
        /// </summary>
        private List<int> offsets_; // ICU4N specific - nix the UVector32 class - we have real generics in .NET that don't box

        private string string_;  // TODO: needed in Java? if so, then add a UCharacterIterator field too?

        /// <summary>
        /// This constant is returned by the iterator in the methods
        /// <see cref="Next()"/> and <see cref="Previous()"/> when the end or the beginning of the
        /// source string has been reached, and there are no more valid
        /// collation elements to return.
        /// <para/>
        /// See <see cref="CollationElementIterator"/> for an example of use.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        /// <seealso cref="Next()"/>
        /// <seealso cref="Previous()"/>
        public const int NULLORDER = unchecked((int)0xffffffff); // ICU4N TODO: API - Rename NullOrder

        /// <summary>
        /// This constant is returned by the iterator in the methods
        /// <see cref="Next()"/> and <see cref="Previous()"/> when a collation element result is to be
        /// ignored.
        /// <para/>
        /// See <see cref="CollationElementIterator"/> for an example of use.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        /// <seealso cref="Next()"/>
        /// <seealso cref="Previous()"/>
        public const int IGNORABLE = 0; // ICU4N TODO: API - Rename Ingorable

        /// <summary>
        /// Return the primary order of the specified collation element,
        /// i.e. the first 16 bits.  This value is unsigned.
        /// </summary>
        /// <param name="ce">The collation element.</param>
        /// <returns>The element's 16 bits primary order.</returns>
        /// <stable>ICU 2.8</stable>
        public static int PrimaryOrder(int ce)
        {
            return (ce.TripleShift(16)) & 0xffff;
        }

        /// <summary>
        /// Return the secondary order of the specified collation element,
        /// i.e. the 16th to 23th bits, inclusive.  This value is unsigned.
        /// </summary>
        /// <param name="ce">The collation element.</param>
        /// <returns>The element's 8 bits secondary order.</returns>
        /// <stable>ICU 2.8</stable>
        public static int SecondaryOrder(int ce)
        {
            return (ce.TripleShift(8)) & 0xff;
        }

        /// <summary>
        /// Return the tertiary order of the specified collation element, i.e. the last
        /// 8 bits.  This value is unsigned.
        /// </summary>
        /// <param name="ce">The collation element.</param>
        /// <returns>The element's 8 bits tertiary order.</returns>
        /// <stable>ICU 2.8</stable>
        public static int TertiaryOrder(int ce)
        {
            return ce & 0xff;
        }


        private static int GetFirstHalf(long p, int lower32)
        {
            return (int)((int)p & 0xffff0000) | ((lower32 >> 16) & 0xff00) | ((lower32 >> 8) & 0xff);
        }

        private static int GetSecondHalf(long p, int lower32)
        {
            return ((int)p << 16) | ((lower32 >> 8) & 0xff00) | (lower32 & 0x3f);
        }

        private static bool CeNeedsTwoParts(long ce)
        {
            return (ce & 0xffff00ff003fL) != 0;
        }

        private CollationElementIterator(RuleBasedCollator collator)
        {
            iter_ = null;
            rbc_ = collator;
            otherHalf_ = 0;
            dir_ = 0;
            offsets_ = null;
        }

        /// <summary>
        /// <see cref="CollationElementIterator"/> constructor. This takes a source
        /// string and a <see cref="Text.RuleBasedCollator"/>. The iterator will walk through
        /// the source string based on the rules defined by the
        /// collator. If the source string is empty, <see cref="NULLORDER"/> will be
        /// returned on the first call to <see cref="Next()"/>.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="collator">The <see cref="Text.RuleBasedCollator"/>.</param>
        /// <stable>ICU 2.8</stable>
        internal CollationElementIterator(string source, RuleBasedCollator collator)
            : this(collator)
        {
            SetText(source);
        }
        // Note: The constructors should take settings & tailoring, not a collator,
        // to avoid circular dependencies.
        // However, for equals() we would need to be able to compare tailoring data for equality
        // without making CollationData or CollationTailoring depend on TailoredSet.
        // (See the implementation of RuleBasedCollator.equals().)
        // That might require creating an intermediate class that would be used
        // by both CollationElementIterator and RuleBasedCollator
        // but only contain the part of RBC.equals() related to data and rules.

        /// <summary>
        /// <see cref="CollationElementIterator"/> constructor. This takes a source
        /// character iterator and a <see cref="Text.RuleBasedCollator"/>. The iterator will
        /// walk through the source string based on the rules defined by
        /// the collator. If the source string is empty, <see cref="NULLORDER"/> will be
        /// returned on the first call to <see cref="Next()"/>.
        /// </summary>
        /// <param name="source">The source string iterator.</param>
        /// <param name="collator">The <see cref="Text.RuleBasedCollator"/>.</param>
        /// <stable>ICU 2.8</stable>
        internal CollationElementIterator(CharacterIterator source, RuleBasedCollator collator)
            : this(collator)
        {
            SetText(source);
        }

        /// <summary>
        /// <see cref="CollationElementIterator"/> constructor. This takes a source
        /// character iterator and a <see cref="Text.RuleBasedCollator"/>. The iterator will
        /// walk through the source string based on the rules defined by
        /// the collator. If the source string is empty, <see cref="NULLORDER"/> will be
        /// returned on the first call to <see cref="Next()"/>.
        /// </summary>
        /// <param name="source">The source string iterator.</param>
        /// <param name="collator">The <see cref="Text.RuleBasedCollator"/>.</param>
        /// <stable>ICU 2.8</stable>
        internal CollationElementIterator(UCharacterIterator source, RuleBasedCollator collator)
            : this(collator)
        {
            SetText(source);
        }

        /// <summary>
        /// Returns the character offset in the source string
        /// corresponding to the next collation element. I.e., <see cref="GetOffset()"/>
        /// returns the position in the source string corresponding to the
        /// collation element that will be returned by the next call to
        /// <see cref="Next()"/> or <see cref="Previous()"/>. This value could be any of:
        /// <list type="bullet">
        ///     <item>
        ///         <description>
        ///             The index of the <b>first</b> character corresponding to
        ///             the next collation element. (This means that if
        ///             <see cref="SetOffset(int)"/> sets the index in the middle of
        ///             a contraction, <see cref="GetOffset()"/> returns the index of
        ///             the first character in the contraction, which may not be equal
        ///             to the original offset that was set. Hence calling <see cref="GetOffset()"/>
        ///             immediately after <see cref="SetOffset(int)"/> does not guarantee that the
        ///             original offset set will be returned.)
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             If normalization is on, the index of the <b>immediate</b>
        ///             subsequent character, or composite character with the first
        ///             character, having a combining class of 0.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             The length of the source string, if iteration has reached
        ///             the end.
        ///         </description>
        ///     </item>
        /// </list>
        /// </summary>
        /// <returns>
        /// The character offset in the source string corresponding to the
        /// collation element that will be returned by the next call to
        /// <see cref="Next()"/> or <see cref="Previous()"/>.
        /// </returns>
        /// <stable>ICU 2.8</stable>
        public int GetOffset()
        {
            if (dir_ < 0 && offsets_ != null && offsets_.Count > 0)
            {
                // CollationIterator.previousCE() decrements the CEs length
                // while it pops CEs from its internal buffer.
                int i = iter_.CEsLength;
                if (otherHalf_ != 0)
                {
                    // Return the trailing CE offset while we are in the middle of a 64-bit CE.
                    ++i;
                }
                Debug.Assert(i < offsets_.Count);
                return offsets_[i];
            }
            return iter_.Offset;
        }

        /// <summary>
        /// Get the next collation element in the source string.
        /// <para/>
        /// This iterator iterates over a sequence of collation elements
        /// that were built from the string. Because there isn't
        /// necessarily a one-to-one mapping from characters to collation
        /// elements, this doesn't mean the same thing as "return the
        /// collation element [or ordering priority] of the next character
        /// in the string".
        /// <para/>
        /// This function returns the collation element that the
        /// iterator is currently pointing to, and then updates the
        /// internal pointer to point to the next element.
        /// </summary>
        /// <returns>The next collation element or <see cref="NULLORDER"/> if the end of the iteration has been reached.</returns>
        /// <stable>ICU 2.8</stable>
        public int Next()
        {
            if (dir_ > 1)
            {
                // Continue forward iteration. Test this first.
                if (otherHalf_ != 0)
                {
                    int oh = otherHalf_;
                    otherHalf_ = 0;
                    return oh;
                }
            }
            else if (dir_ == 1)
            {
                // next() after setOffset()
                dir_ = 2;
            }
            else if (dir_ == 0)
            {
                // The iter_ is already reset to the start of the text.
                dir_ = 2;
            }
            else /* dir_ < 0 */
            {
                // illegal change of direction
                throw new InvalidOperationException("Illegal change of direction");
                // Java porting note: ICU4C sets U_INVALID_STATE_ERROR to the return status.
            }
            // No need to keep all CEs in the buffer when we iterate.
            iter_.ClearCEsIfNoneRemaining();
            long ce = iter_.NextCE();
            if (ce == Collation.NO_CE)
            {
                return NULLORDER;
            }
            // Turn the 64-bit CE into two old-style 32-bit CEs, without quaternary bits.
            long p = ce.TripleShift(32);
            int lower32 = (int)ce;
            int firstHalf = GetFirstHalf(p, lower32);
            int secondHalf = GetSecondHalf(p, lower32);
            if (secondHalf != 0)
            {
                otherHalf_ = secondHalf | 0xc0; // continuation CE
            }
            return firstHalf;
        }

        /// <summary>
        /// Get the previous collation element in the source string.
        /// <para/>
        /// This iterator iterates over a sequence of collation elements
        /// that were built from the string. Because there isn't
        /// necessarily a one-to-one mapping from characters to collation
        /// elements, this doesn't mean the same thing as "return the
        /// collation element [or ordering priority] of the previous
        /// character in the string".
        /// <para/>
        /// This function updates the iterator's internal pointer to
        /// point to the collation element preceding the one it's currently
        /// pointing to and then returns that element, while <see cref="Next()"/> returns
        /// the current element and then updates the pointer.
        /// </summary>
        /// <returns>The previous collation element, or <see cref="NULLORDER"/> when the start of
        /// the iteration has been reached.</returns>
        /// <stable>ICU 2.8</stable>
        public int Previous()
        {
            if (dir_ < 0)
            {
                // Continue backwards iteration. Test this first.
                if (otherHalf_ != 0)
                {
                    int oh = otherHalf_;
                    otherHalf_ = 0;
                    return oh;
                }
            }
            else if (dir_ == 0)
            {
                iter_.ResetToOffset(string_.Length);
                dir_ = -1;
            }
            else if (dir_ == 1)
            {
                // previous() after setOffset()
                dir_ = -1;
            }
            else /* dir_ > 1 */
            {
                // illegal change of direction
                throw new InvalidOperationException("Illegal change of direction");
                // Java porting note: ICU4C sets U_INVALID_STATE_ERROR to the return status.
            }
            if (offsets_ == null)
            {
                offsets_ = new List<int>();
            }
            // If we already have expansion CEs, then we also have offsets.
            // Otherwise remember the trailing offset in case we need to
            // write offsets for an artificial expansion.
            int limitOffset = iter_.CEsLength == 0 ? iter_.Offset : 0;
            long ce = iter_.PreviousCE(offsets_);
            if (ce == Collation.NO_CE)
            {
                return NULLORDER;
            }
            // Turn the 64-bit CE into two old-style 32-bit CEs, without quaternary bits.
            long p = ce.TripleShift(32);
            int lower32 = (int)ce;
            int firstHalf = GetFirstHalf(p, lower32);
            int secondHalf = GetSecondHalf(p, lower32);
            if (secondHalf != 0)
            {
                if (offsets_.Count == 0)
                {
                    // When we convert a single 64-bit CE into two 32-bit CEs,
                    // we need to make this artificial expansion behave like a normal expansion.
                    // See CollationIterator.previousCE().
                    offsets_.Add(iter_.Offset);
                    offsets_.Add(limitOffset);
                }
                otherHalf_ = firstHalf;
                return secondHalf | 0xc0; // continuation CE
            }
            return firstHalf;
        }

        /// <summary>
        /// Resets the cursor to the beginning of the string. The next
        /// call to <see cref="Next()"/> or <see cref="Previous()"/> will return the first and last
        /// collation element in the string, respectively.
        /// <para/>
        /// If the <see cref="Text.RuleBasedCollator"/> used by this iterator has had its
        /// attributes changed, calling <see cref="Reset()"/> will reinitialize the
        /// iterator to use the new attributes.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        public void Reset()
        {
            iter_.ResetToOffset(0);
            otherHalf_ = 0;
            dir_ = 0;
        }

        /// <summary>
        /// Sets the iterator to point to the collation element
        /// corresponding to the character at the specified offset. The
        /// value returned by the next call to <see cref="Next()"/> will be the collation
        /// element corresponding to the characters at <paramref name="newOffset"/>.
        /// <para/>
        /// If <paramref name="newOffset"/> is in the middle of a contracting character
        /// sequence, the iterator is adjusted to the start of the
        /// contracting sequence. This means that <see cref="GetOffset()"/> is not
        /// guaranteed to return the same value set by this method.
        /// <para/>
        /// If the decomposition mode is on, and offset is in the middle
        /// of a decomposible range of source text, the iterator may not
        /// return a correct result for the next forwards or backwards
        /// iteration.  The user must ensure that the offset is not in the
        /// middle of a decomposible range.
        /// </summary>
        /// <param name="newOffset">The character offset into the original source string to
        /// set. Note that this is not an offset into the corresponding
        /// sequence of collation elements.</param>
        /// <stable>ICU 2.8</stable>
        public void SetOffset(int newOffset)
        {
            if (0 < newOffset && newOffset < string_.Length)
            {
                int offset = newOffset;
                do
                {
                    char c = string_[offset];
                    if (!rbc_.IsUnsafe(c) ||
                            (char.IsHighSurrogate(c) && !rbc_.IsUnsafe(string_.CodePointAt(offset))))
                    {
                        break;
                    }
                    // Back up to before this unsafe character.
                    --offset;
                } while (offset > 0);
                if (offset < newOffset)
                {
                    // We might have backed up more than necessary.
                    // For example, contractions "ch" and "cu" make both 'h' and 'u' unsafe,
                    // but for text "chu" setOffset(2) should remain at 2
                    // although we initially back up to offset 0.
                    // Find the last safe offset no greater than newOffset by iterating forward.
                    int lastSafeOffset = offset;
                    do
                    {
                        iter_.ResetToOffset(lastSafeOffset);
                        do
                        {
                            iter_.NextCE();
                        } while ((offset = iter_.Offset) == lastSafeOffset);
                        if (offset <= newOffset)
                        {
                            lastSafeOffset = offset;
                        }
                    } while (offset < newOffset);
                    newOffset = lastSafeOffset;
                }
            }
            iter_.ResetToOffset(newOffset);
            otherHalf_ = 0;
            dir_ = 1;
        }

        /// <summary>
        /// Set a new source string for iteration, and reset the offset
        /// to the beginning of the text.
        /// </summary>
        /// <param name="source">The new source string for iteration.</param>
        /// <stable>ICU 2.8</stable>
        public void SetText(string source)
        {
            string_ = source; // TODO: do we need to remember the source string in a field?
            CollationIterator newIter;
            bool numeric = rbc_.settings.ReadOnly.IsNumeric;
            if (rbc_.settings.ReadOnly.DontCheckFCD)
            {
                newIter = new UTF16CollationIterator(rbc_.data, numeric, string_.ToCharSequence(), 0);
            }
            else
            {
                newIter = new FCDUTF16CollationIterator(rbc_.data, numeric, string_.ToCharSequence(), 0);
            }
            iter_ = newIter;
            otherHalf_ = 0;
            dir_ = 0;
        }

        /// <summary>
        /// Set a new source string iterator for iteration, and reset the
        /// offset to the beginning of the text.
        /// <para/>
        /// The source iterator's integrity will be preserved since a new copy
        /// will be created for use.
        /// </summary>
        /// <param name="source">The new source string iterator for iteration.</param>
        /// <stable>ICU 2.8</stable>
        public void SetText(UCharacterIterator source)
        {
            string_ = source.GetText(); // TODO: do we need to remember the source string in a field?
                                        // Note: In C++, we just setText(source.getText()).
                                        // In Java, we actually operate on a character iterator.
                                        // (The old code apparently did so only for a CharacterIterator;
                                        // for a UCharacterIterator it also just used source.getText()).
                                        // TODO: do we need to remember the cloned iterator in a field?
            UCharacterIterator src;
            //try
            //{
            src = (UCharacterIterator)source.Clone();
            //}
            //catch (CloneNotSupportedException e)
            //{
            //    // Fall back to ICU 52 behavior of iterating over the text contents
            //    // of the UCharacterIterator.
            //    setText(source.getText());
            //    return;
            //}
            src.SetToStart();
            CollationIterator newIter;
            bool numeric = rbc_.settings.ReadOnly.IsNumeric;
            if (rbc_.settings.ReadOnly.DontCheckFCD)
            {
                newIter = new IterCollationIterator(rbc_.data, numeric, src);
            }
            else
            {
                newIter = new FCDIterCollationIterator(rbc_.data, numeric, src, 0);
            }
            iter_ = newIter;
            otherHalf_ = 0;
            dir_ = 0;
        }

        /// <summary>
        /// Set a new source string iterator for iteration, and reset the
        /// offset to the beginning of the text.
        /// </summary>
        /// <param name="source">The new source string iterator for iteration.</param>
        /// <stable>ICU 2.8</stable>
        public void SetText(CharacterIterator source)
        {
            // Note: In C++, we just setText(source.getText()).
            // In Java, we actually operate on a character iterator.
            // TODO: do we need to remember the iterator in a field?
            // TODO: apparently we don't clone a CharacterIterator in Java,
            // we only clone the text for a UCharacterIterator?? see the old code in the constructors
            UCharacterIterator src = new CharacterIteratorWrapper(source);
            src.SetToStart();
            string_ = src.GetText(); // TODO: do we need to remember the source string in a field?
            CollationIterator newIter;
            bool numeric = rbc_.settings.ReadOnly.IsNumeric;
            if (rbc_.settings.ReadOnly.DontCheckFCD)
            {
                newIter = new IterCollationIterator(rbc_.data, numeric, src);
            }
            else
            {
                newIter = new FCDIterCollationIterator(rbc_.data, numeric, src, 0);
            }
            iter_ = newIter;
            otherHalf_ = 0;
            dir_ = 0;
        }

        private sealed class MaxExpSink : ContractionsAndExpansions.ICESink
        {
            internal MaxExpSink(IDictionary<int, int> h)
            {
                maxExpansions = h;
            }

            public void HandleCE(long ce)
            {
            }

            public void HandleExpansion(IList<long> ces, int start, int length)
            {
                if (length <= 1)
                {
                    // We do not need to add single CEs into the map.
                    return;
                }
                int count = 0; // number of CE "halves"
                for (int i = 0; i < length; ++i)
                {
                    count += CeNeedsTwoParts(ces[start + i]) ? 2 : 1;
                }
                // last "half" of the last CE
                long ce = ces[start + length - 1];
                long p = ce.TripleShift(32);
                int lower32 = (int)ce;
                int lastHalf = GetSecondHalf(p, lower32);
                if (lastHalf == 0)
                {
                    lastHalf = GetFirstHalf(p, lower32);
                    Debug.Assert(lastHalf != 0);
                }
                else
                {
                    lastHalf |= 0xc0; // old-style continuation CE
                }
                int oldCount;
                if (!maxExpansions.TryGetValue(lastHalf, out oldCount) || count > oldCount)
                {
                    maxExpansions[lastHalf] = count;
                }
            }

            private IDictionary<int, int> maxExpansions;
        }

        internal static IDictionary<int, int> ComputeMaxExpansions(CollationData data)
        {
            IDictionary<int, int> maxExpansions = new Dictionary<int, int>();
            MaxExpSink sink = new MaxExpSink(maxExpansions);
            new ContractionsAndExpansions(null, null, sink, true).ForData(data);
            return maxExpansions;
        }

        /// <summary>
        /// Returns the maximum length of any expansion sequence that ends with
        /// the specified collation element. If there is no expansion with this
        /// collation element as the last element, returns 1.
        /// </summary>
        /// <param name="ce">A collation element returned by <see cref="Previous()"/> or <see cref="Next()"/>.</param>
        /// <returns>The maximum length of any expansion sequence ending with the specified collation element.</returns>
        /// <stable>ICU 2.8</stable>
        public int GetMaxExpansion(int ce)
        {
            return GetMaxExpansion(rbc_.tailoring.MaxExpansions, ce);
        }

        internal static int GetMaxExpansion(IDictionary<int, int> maxExpansions, int order)
        {
            if (order == 0)
            {
                return 1;
            }
            if (maxExpansions != null && maxExpansions.TryGetValue(order, out int max))
            {
                return max;
            }
            if ((order & 0xc0) == 0xc0)
            {
                // old-style continuation CE
                return 2;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// Normalizes dir_=1 (just after <see cref="SetOffset(int)"/>) to dir_=0 (just after <see cref="Reset()"/>).
        /// </summary>
        /// <returns></returns>
        private sbyte NormalizeDir()
        {
            return dir_ == 1 ? (sbyte)0 : dir_;
        }

        /// <summary>
        /// Tests that argument object is equals to this <see cref="CollationElementIterator"/>.
        /// Iterators are equal if the objects uses the same <see cref="Text.RuleBasedCollator"/>,
        /// the same source text and have the same current position in iteration.
        /// </summary>
        /// <param name="that">Object to test if it is equals to this <see cref="CollationElementIterator"/>.</param>
        /// <stable>ICU 2.8</stable>
        public override bool Equals(object that)
        {
            if (that == this)
            {
                return true;
            }
            if (that is CollationElementIterator thatceiter)
            {
                return rbc_.Equals(thatceiter.rbc_)
                        && otherHalf_ == thatceiter.otherHalf_
                        && NormalizeDir() == thatceiter.NormalizeDir()
                        && string_.Equals(thatceiter.string_)
                        && iter_.Equals(thatceiter.iter_);
            }
            return false;
        }

        /// <summary>
        /// Mock implementation of <see cref="object.GetHashCode()"/>. This implementation always returns a constant
        /// value. When debugging is enabled, this method triggers an assertion failure.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#pragma warning disable 809
        public override int GetHashCode()
#pragma warning restore 809
        {
            Debug.Assert(false, "GetHashCode not designed");
            return 42;
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public RuleBasedCollator RuleBasedCollator
        {
            get { return rbc_; }
        }
    }
}
