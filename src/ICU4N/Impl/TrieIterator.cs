using ICU4N.Globalization;
using ICU4N.Support.Text;
using ICU4N.Text;
using ICU4N.Util;
using System;
using System.Collections;
using Element = ICU4N.Util.RangeValueEnumeratorElement;

namespace ICU4N.Impl
{
    /// <summary>
    /// Class enabling iteration of the values in a <see cref="Trie"/>.
    /// </summary>
    /// <remarks>
    /// Result of each iteration contains the interval of codepoints that have
    /// the same value type and the value type itself.
    /// <para/>
    /// The comparison of each codepoint value is done via <see cref="Extract(int)"/>, which the
    /// default implementation is to return the value as it is.
    /// <para/>
    /// Method <see cref="Extract(int)"/> can be overwritten to perform manipulations on
    /// codepoint values in order to perform specialized comparison.
    /// <para/>
    /// TrieEnumerator is designed to be a generic iterator for the <see cref="CharTrie"/>
    /// and the <see cref="Int32Trie"/>, hence to accommodate both types of data, the return
    /// result will be in terms of int (32 bit) values.
    /// <para/>
    /// See <see cref="Text.UCharacterTypeIterator"/> for examples of use.
    /// <para/>
    /// Notes for porting utrie_enum from icu4c to icu4j:
    /// <para/>
    /// Internally, icu4c's utrie_enum performs all iterations in its body. In Java
    /// sense, the caller will have to pass a object with a callback function
    /// UTrieEnumRange(const void *context, UChar32 start, UChar32 limit,
    /// uint32_t value) into utrie_enum. utrie_enum will then find ranges of
    /// codepoints with the same value as determined by
    /// UTrieEnumValue(const void *context, uint32_t value). for each range,
    /// utrie_enum calls the callback function to perform a task. In this way,
    /// icu4c performs the iteration within utrie_enum.
    /// To follow the JDK model, icu4j is slightly different from icu4c.
    /// Instead of requesting the caller to implement an object for a callback.
    /// The caller will have to implement a subclass of TrieEnumerator, fleshing out
    /// the method extract(int) (equivalent to UTrieEnumValue). Independent of icu4j,
    /// the caller will have to code his own iteration and flesh out the task
    /// (equivalent to UTrieEnumRange) to be performed in the iteration loop.
    /// <para/>
    /// There are basically 3 usage scenarios for porting:
    /// <list type="number">
    ///     <item><description>UTrieEnumValue is the only implemented callback then just implement a
    ///     subclass of TrieEnumerator and override the extract(int) method. The
    ///     extract(int) method is analogus to UTrieEnumValue callback.</description></item>
    ///     <item><description>UTrieEnumValue and UTrieEnumRange both are implemented then implement
    ///     a subclass of TrieEnumerator, override the extract method and iterate, e.g
    ///     <code>
    ///         utrie_enum(&normTrie, _enumPropertyStartsValue, _enumPropertyStartsRange, set);
    ///     </code>
    ///     In .NET:
    ///     <code>
    ///         class TrieEnumeratorImpl : TrieEnumerator
    ///         {
    ///             public TrieEnumeratorImpl(Trie data)
    ///                 : base(data)
    ///             {
    ///             }
    ///             
    ///             protected override int Extract(int value)
    ///             {
    ///                 // port the implementation of _enumPropertyStartsValue here
    ///             }
    ///         }
    ///         
    ///         ...
    ///         
    ///         TrieEnumerator fcdIter  = new TrieEnumeratorImpl(fcdTrieImpl.FcdTrie);
    ///         while (fcdIter.MoveNext())
    ///         {
    ///             // port the implementation of _enumPropertyStartsRange
    ///         }
    ///     </code>
    ///     </description></item>
    ///     <item><description>UTrieEnumRange is the only implemented callback then just implement
    ///     the while loop, when utrie_enum is called
    ///     <code>
    ///         // utrie_enum(&fcdTrie, NULL, _enumPropertyStartsRange, set);
    ///         TrieEnumerator fcdIter  = new TrieEnumerator(fcdTrieImpl.FcdTrie);
    ///         while (fcdIter.MoveNext())
    ///         {
    ///             set.Add(fcdIter.Current.Start);
    ///         }
    ///     </code></description></item>
    /// </list>
    /// <para/>
    /// NOTE: This is equivalent to TrieIterator in ICU4J.
    /// </remarks>
    /// <seealso cref="Trie"/>
    /// <author>synwee</author>
    /// <since>release 2.1, Jan 17 2002</since>
    // 2015-sep-03 TODO: Only used in test code, move there.
    public class TrieEnumerator : IRangeValueEnumerator
    {
        // public constructor ---------------------------------------------

        /// <summary>
        /// TrieEnumeration constructor.
        /// </summary>
        /// <param name="trie">Trie to be used.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="trie"/> argument is null.</exception>
        public TrieEnumerator(Trie trie)
        {
            if (trie == null)
            {
                throw new ArgumentNullException(nameof(trie), "Argument trie cannot be null");
            }
            m_trie_ = trie;
            // synwee: check that extract belongs to the child class
            m_initialValue_ = Extract(m_trie_.InitialValue);
            Reset();
        }

        // public methods -------------------------------------------------

        /// <summary>
        /// Returns true if we are not at the end of the iteration, false
        /// otherwise.
        /// The next set of codepoints with the same value type will be
        /// calculated during this call and returned in the arguement element.
        /// </summary>
        /// <param name="element">Return result.</param>
        /// <returns>true if we are not at the end of the iteration, false otherwise.</returns>
        /// <seealso cref="Element"/>
        private bool Next(Element element)
        {
            if (m_nextCodepoint_ > UChar.MaxValue)
            {
                return false;
            }
            if (m_nextCodepoint_ < UChar.SupplementaryMinValue &&
                CalculateNextBMPElement(element))
            {
                return true;
            }
            CalculateNextSupplementaryElement(element);
            return true;
        }

        /// <summary>
        /// Gets the current <see cref="Element"/> of the iteration.
        /// </summary>
        public Element Current => current;

        object IEnumerator.Current => current;

        /// <summary>
        /// Returns true if we are not at the end of the iteration, false
        /// otherwise.
        /// The next set of codepoints with the same value type will be
        /// calculated during this call and set to <see cref="Current"/>.
        /// </summary>
        /// <returns>true if we are not at the end of the iteration, false otherwise.</returns>
        /// <seealso cref="Element"/>
        public bool MoveNext()
        {
            var temp = new Element();
            var hasNext = Next(temp);
            if (hasNext)
                current = temp;
            return hasNext;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // nothing to do.
        }

        /// <summary>
        /// Resets the iterator to the beginning of the iteration
        /// </summary>
        public void Reset()
        {
            m_currentCodepoint_ = 0;
            m_nextCodepoint_ = 0;
            m_nextIndex_ = 0;
            m_nextBlock_ = m_trie_.m_index_[0] << Trie.INDEX_STAGE_2_SHIFT_;
            if (m_nextBlock_ == m_trie_.m_dataOffset_)
            {
                m_nextValue_ = m_initialValue_;
            }
            else
            {
                m_nextValue_ = Extract(m_trie_[m_nextBlock_]);
            }
            m_nextBlockIndex_ = 0;
            m_nextTrailIndexOffset_ = TRAIL_SURROGATE_INDEX_BLOCK_LENGTH_;
        }

        // protected methods ----------------------------------------------

        /// <summary>
        /// Called by <see cref="Next(Element)"/> to extract a 32 bit value from a trie value
        /// used for comparison.
        /// This method is to be overwritten if special manipulation is to be done
        /// to retrieve a relevant comparison.
        /// The default function is to return the value as it is.
        /// </summary>
        /// <param name="value">A value from the trie.</param>
        /// <returns>Extracted value.</returns>
        protected virtual int Extract(int value)
        {
            return value;
        }

        // private methods ------------------------------------------------

        /// <summary>
        /// Set the result values.
        /// </summary>
        /// <param name="element">Return result object.</param>
        /// <param name="start">Start codepoint of range.</param>
        /// <param name="limit">(end + 1) codepoint of range.</param>
        /// <param name="value">Common value of range.</param>
        private void SetResult(Element element, int start, int limit,
                                     int value)
        {
            element.Start = start;
            element.Limit = limit;
            element.Value = value;
        }

        /// <summary>
        /// Finding the next element.
        /// This method is called just before returning the result of
        /// <see cref="Next(Element)"/>.
        /// We always store the next element before it is requested.
        /// In the case that we have to continue calculations into the
        /// supplementary planes, a false will be returned.
        /// </summary>
        /// <param name="element">Return result object.</param>
        /// <returns>true if the next range is found, false if we have to proceed to
        /// the supplementary range.</returns>
        private bool CalculateNextBMPElement(Element element)
        {
            int currentValue = m_nextValue_;
            m_currentCodepoint_ = m_nextCodepoint_;
            m_nextCodepoint_++;
            m_nextBlockIndex_++;
            if (!CheckBlockDetail(currentValue))
            {
                SetResult(element, m_currentCodepoint_, m_nextCodepoint_,
                          currentValue);
                return true;
            }
            // synwee check that next block index == 0 here
            // enumerate BMP - the main loop enumerates data blocks
            while (m_nextCodepoint_ < UChar.SupplementaryMinValue)
            {
                // because of the way the character is split to form the index
                // the lead surrogate and trail surrogate can not be in the
                // mid of a block
                if (m_nextCodepoint_ == LEAD_SURROGATE_MIN_VALUE_)
                {
                    // skip lead surrogate code units,
                    // go to lead surrogate codepoints
                    m_nextIndex_ = BMP_INDEX_LENGTH_;
                }
                else if (m_nextCodepoint_ == TRAIL_SURROGATE_MIN_VALUE_)
                {
                    // go back to regular BMP code points
                    m_nextIndex_ = m_nextCodepoint_ >> Trie.INDEX_STAGE_1_SHIFT_;
                }
                else
                {
                    m_nextIndex_++;
                }

                m_nextBlockIndex_ = 0;
                if (!CheckBlock(currentValue))
                {
                    SetResult(element, m_currentCodepoint_, m_nextCodepoint_,
                              currentValue);
                    return true;
                }
            }
            m_nextCodepoint_--;   // step one back since this value has not been
            m_nextBlockIndex_--;  // retrieved yet.
            return false;
        }

        /// <summary>
        /// Finds the next supplementary element.
        /// For each entry in the trie, the value to be delivered is passed through
        /// <see cref="Extract(int)"/>.
        /// We always store the next element before it is requested.
        /// Called after <see cref="CalculateNextBMPElement(Element)"/> completes its round of BMP characters.
        /// There is a slight difference in the usage of <see cref="m_currentCodepoint_"/>.
        /// here as compared to <see cref="CalculateNextBMPElement(Element)"/>. Though both represents the
        /// lower bound of the next element, in <see cref="CalculateNextBMPElement(Element)"/> it gets set
        /// at the start of any loop, where-else, in <see cref="CalculateNextSupplementaryElement(Element)"/>
        /// since <see cref="m_currentCodepoint_"/> already contains the lower bound of the
        /// next element (passed down from <see cref="CalculateNextBMPElement(Element)"/>), we keep it till
        /// the end before resetting it to the new value.
        /// Note, if there are no more iterations, it will never get to here.
        /// Blocked out by <see cref="Next(Element)"/>.
        /// </summary>
        /// <param name="element">Return result object.</param>
        private void CalculateNextSupplementaryElement(Element element)
        {
            int currentValue = m_nextValue_;
            m_nextCodepoint_++;
            m_nextBlockIndex_++;

            if (UTF16.GetTrailSurrogate(m_nextCodepoint_)
                                            != UTF16.TrailSurrogateMinValue)
            {
                // this piece is only called when we are in the middle of a lead
                // surrogate block
                if (!CheckNullNextTrailIndex() && !CheckBlockDetail(currentValue))
                {
                    SetResult(element, m_currentCodepoint_, m_nextCodepoint_,
                              currentValue);
                    m_currentCodepoint_ = m_nextCodepoint_;
                    return;
                }
                // we have cleared one block
                m_nextIndex_++;
                m_nextTrailIndexOffset_++;
                if (!CheckTrailBlock(currentValue))
                {
                    SetResult(element, m_currentCodepoint_, m_nextCodepoint_,
                              currentValue);
                    m_currentCodepoint_ = m_nextCodepoint_;
                    return;
                }
            }
            int nextLead = UTF16.GetLeadSurrogate(m_nextCodepoint_);
            // enumerate supplementary code points
            while (nextLead < TRAIL_SURROGATE_MIN_VALUE_)
            {
                // lead surrogate access
                int leadBlock =
                       m_trie_.m_index_[nextLead >> Trie.INDEX_STAGE_1_SHIFT_] <<
                                                       Trie.INDEX_STAGE_2_SHIFT_;
                if (leadBlock == m_trie_.m_dataOffset_)
                {
                    // no entries for a whole block of lead surrogates
                    if (currentValue != m_initialValue_)
                    {
                        m_nextValue_ = m_initialValue_;
                        m_nextBlock_ = leadBlock;  // == m_trie_.m_dataOffset_
                        m_nextBlockIndex_ = 0;
                        SetResult(element, m_currentCodepoint_, m_nextCodepoint_,
                                  currentValue);
                        m_currentCodepoint_ = m_nextCodepoint_;
                        return;
                    }

                    nextLead += DATA_BLOCK_LENGTH_;
                    // number of total affected supplementary codepoints in one
                    // block
                    // this is not a simple addition of
                    // DATA_BLOCK_SUPPLEMENTARY_LENGTH since we need to consider
                    // that we might have moved some of the codepoints
                    m_nextCodepoint_ = Character.ToCodePoint((char)nextLead, (char)UTF16.TrailSurrogateMinValue);
                    continue;
                }
                if (m_trie_.m_dataManipulate_ == null)
                {
                    throw new InvalidOperationException(
                                "The field DataManipulate in this Trie is null"); // ICU4N: This was originally NullPointerException
                }
                // enumerate trail surrogates for this lead surrogate
                m_nextIndex_ = m_trie_.m_dataManipulate_.GetFoldingOffset(
                                   m_trie_[leadBlock +
                                       (nextLead & Trie.INDEX_STAGE_3_MASK_)]);
                if (m_nextIndex_ <= 0)
                {
                    // no data for this lead surrogate
                    if (currentValue != m_initialValue_)
                    {
                        m_nextValue_ = m_initialValue_;
                        m_nextBlock_ = m_trie_.m_dataOffset_;
                        m_nextBlockIndex_ = 0;
                        SetResult(element, m_currentCodepoint_, m_nextCodepoint_,
                                  currentValue);
                        m_currentCodepoint_ = m_nextCodepoint_;
                        return;
                    }
                    m_nextCodepoint_ += TRAIL_SURROGATE_COUNT_;
                }
                else
                {
                    m_nextTrailIndexOffset_ = 0;
                    if (!CheckTrailBlock(currentValue))
                    {
                        SetResult(element, m_currentCodepoint_, m_nextCodepoint_,
                                  currentValue);
                        m_currentCodepoint_ = m_nextCodepoint_;
                        return;
                    }
                }
                nextLead++;
            }

            // deliver last range
            SetResult(element, m_currentCodepoint_, UChar.MaxValue + 1,
                      currentValue);
        }

        /// <summary>
        /// Internal block value calculations
        /// Performs calculations on a data block to find codepoints in <see cref="m_nextBlock_"/>
        /// after the index m_nextBlockIndex_ that has the same value.
        /// Note m_*_ variables at this point is the next codepoint whose value
        /// has not been calculated.
        /// But when returned with false, it will be the last codepoint whose
        /// value has been calculated.
        /// </summary>
        /// <param name="currentValue">The value which other codepoints are tested against.</param>
        /// <returns>true if the whole block has the same value as currentValue or if
        /// the whole block has been calculated, false otherwise.</returns>
        private bool CheckBlockDetail(int currentValue)
        {
            while (m_nextBlockIndex_ < DATA_BLOCK_LENGTH_)
            {
                m_nextValue_ = Extract(m_trie_[m_nextBlock_ +
                                                        m_nextBlockIndex_]);
                if (m_nextValue_ != currentValue)
                {
                    return false;
                }
                ++m_nextBlockIndex_;
                ++m_nextCodepoint_;
            }
            return true;
        }

        /// <summary>
        /// Internal block value calculations
        /// Performs calculations on a data block to find codepoints in <see cref="m_nextBlock_"/>
        /// that has the same value.
        /// Will call <see cref="CheckBlockDetail(int)"/> if highlevel check fails.
        /// Note m_*_ variables at this point is the next codepoint whose value
        /// has not been calculated.
        /// </summary>
        /// <param name="currentValue">The value which other codepoints are tested against.</param>
        /// <returns>true if the whole block has the same value as currentValue or if
        /// the whole block has been calculated, false otherwise.</returns>
        private bool CheckBlock(int currentValue)
        {
            int currentBlock = m_nextBlock_;
            m_nextBlock_ = m_trie_.m_index_[m_nextIndex_] <<
                                                      Trie.INDEX_STAGE_2_SHIFT_;
            if (m_nextBlock_ == currentBlock &&
                (m_nextCodepoint_ - m_currentCodepoint_) >= DATA_BLOCK_LENGTH_)
            {
                // the block is the same as the previous one, filled with
                // currentValue
                m_nextCodepoint_ += DATA_BLOCK_LENGTH_;
            }
            else if (m_nextBlock_ == m_trie_.m_dataOffset_)
            {
                // this is the all-initial-value block
                if (currentValue != m_initialValue_)
                {
                    m_nextValue_ = m_initialValue_;
                    m_nextBlockIndex_ = 0;
                    return false;
                }
                m_nextCodepoint_ += DATA_BLOCK_LENGTH_;
            }
            else
            {
                if (!CheckBlockDetail(currentValue))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Internal block value calculations
        /// Performs calculations on multiple data blocks for a set of trail
        /// surrogates to find codepoints in m_nextBlock_ that has the same value.
        /// Will call <see cref="CheckBlock(int)"/> for internal block checks.
        /// Note m_*_ variables at this point is the next codepoint whose value
        /// Note m_*_ variables at this point is the next codepoint whose value
        /// has not been calculated.
        /// </summary>
        /// <param name="currentValue">The value which other codepoints are tested against.</param>
        /// <returns>true if the whole block has the same value as currentValue or if
        /// the whole block has been calculated, false otherwise.</returns>
        private bool CheckTrailBlock(int currentValue)
        {
            // enumerate code points for this lead surrogate
            while (m_nextTrailIndexOffset_ < TRAIL_SURROGATE_INDEX_BLOCK_LENGTH_)
            {
                // if we ever reach here, we are at the start of a new block
                m_nextBlockIndex_ = 0;
                // copy of most of the body of the BMP loop
                if (!CheckBlock(currentValue))
                {
                    return false;
                }
                m_nextTrailIndexOffset_++;
                m_nextIndex_++;
            }
            return true;
        }

        /// <summary>
        /// Checks if we are beginning at the start of a initial block.
        /// If we are then the rest of the codepoints in this initial block
        /// has the same values.
        /// We increment <see cref="m_nextCodepoint_"/> and relevant data members if so.
        /// This is used only in for the supplementary codepoints because
        /// the offset to the trail indexes could be 0.
        /// </summary>
        /// <returns>true if we are at the start of a initial block.</returns>
        private bool CheckNullNextTrailIndex()
        {
            if (m_nextIndex_ <= 0)
            {
                m_nextCodepoint_ += TRAIL_SURROGATE_COUNT_ - 1;
                int nextLead = UTF16.GetLeadSurrogate(m_nextCodepoint_);
                int leadBlock =
                       m_trie_.m_index_[nextLead >> Trie.INDEX_STAGE_1_SHIFT_] <<
                                                       Trie.INDEX_STAGE_2_SHIFT_;
                if (m_trie_.m_dataManipulate_ == null)
                {
                    throw new InvalidOperationException(
                                "The field DataManipulate in this Trie is null"); // ICU4N: This was originally NullPointerException
                }
                m_nextIndex_ = m_trie_.m_dataManipulate_.GetFoldingOffset(
                                   m_trie_[leadBlock +
                                       (nextLead & Trie.INDEX_STAGE_3_MASK_)]);
                m_nextIndex_--;
                m_nextBlockIndex_ = DATA_BLOCK_LENGTH_;
                return true;
            }
            return false;
        }

        // private data members --------------------------------------------

        /// <summary>
        /// Size of the stage 1 BMP indexes
        /// </summary>
        private static readonly int BMP_INDEX_LENGTH_ =
                                            0x10000 >> Trie.INDEX_STAGE_1_SHIFT_;
        /// <summary>
        /// Lead surrogate minimum value
        /// </summary>
        private static readonly int LEAD_SURROGATE_MIN_VALUE_ = 0xD800;
        /// <summary>
        /// Trail surrogate minimum value
        /// </summary>
        private static readonly int TRAIL_SURROGATE_MIN_VALUE_ = 0xDC00;
        ///// <summary>
        ///// Trail surrogate maximum value
        ///// </summary>
        //private static final int TRAIL_SURROGATE_MAX_VALUE_ = 0xDFFF;
        /// <summary>
        /// Number of trail surrogate
        /// </summary>
        private static readonly int TRAIL_SURROGATE_COUNT_ = 0x400;

        /// <summary>
        /// Number of stage 1 indexes for supplementary calculations that maps to
        /// each lead surrogate character.
        /// See second pass into GetRawOffset for the trail surrogate character.
        /// 10 for significant number of bits for trail surrogates, 5 for what we
        /// discard during shifting.
        /// </summary>
        private static readonly int TRAIL_SURROGATE_INDEX_BLOCK_LENGTH_ =
                                        1 << (10 - Trie.INDEX_STAGE_1_SHIFT_);
        /// <summary>
        /// Number of data values in a stage 2 (data array) block.
        /// </summary>
        private static readonly int DATA_BLOCK_LENGTH_ =
                                                  1 << Trie.INDEX_STAGE_1_SHIFT_;
        //    /**
        //    * Number of codepoints in a stage 2 block
        //    */
        //    private static final int DATA_BLOCK_SUPPLEMENTARY_LENGTH_ =
        //                                                     DATA_BLOCK_LENGTH_ << 10;
        /// <summary>
        /// Trie instance
        /// </summary>
        private Trie m_trie_;
        /// <summary>
        /// Initial value for trie values
        /// </summary>
        private int m_initialValue_;
        // Next element results and data.
        private int m_currentCodepoint_;
        private int m_nextCodepoint_;
        private int m_nextValue_;
        private int m_nextIndex_;
        private int m_nextBlock_;
        private int m_nextBlockIndex_;
        private int m_nextTrailIndexOffset_;

        // Holds current element
        private Element current;
    }
}
