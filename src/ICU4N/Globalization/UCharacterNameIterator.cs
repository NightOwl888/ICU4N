using ICU4N.Impl;
using ICU4N.Util;
using System;
using System.Collections;

namespace ICU4N.Globalization
{
    /// <summary>
    /// Class enabling iteration of the codepoints and their names.
    /// <para/>
    /// Result of each iteration contains a valid codepoint that has valid
    /// name.
    /// <para/>
    /// See <see cref="UCharacter.GetNameEnumerator()"/> for an example of use.
    /// </summary>
    /// <remarks>
    /// NOTE: This is equivalent to UCharacterNameIterator in ICU4J.
    /// </remarks>
    /// <author>synwee</author>
    /// <since>release 2.1, March 5 2002</since>
    internal class UCharacterNameEnumerator : IValueEnumerator
    {
        // public methods ----------------------------------------------------

        /// <summary>
        /// Gets the next result for this iteration and returns
        /// true if we are not at the end of the iteration, false otherwise.
        /// <para/>
        /// If the return boolean is a false, the contents of elements will not
        /// be updated.
        /// </summary>
        /// <param name="element">Element for storing the result codepoint and name.</param>
        /// <returns>true if we are not at the end of the iteration, false otherwise.</returns>
        /// <seealso cref="ValueEnumeratorElement"/>
        private bool Next(ValueEnumeratorElement element)
        {
            if (m_current_ >= m_limit_)
            {
                return false;
            }

            if (m_choice_ == (int)UCharacterNameChoice.UnicodeCharName ||
                m_choice_ == (int)UCharacterNameChoice.ExtendedCharName
            )
            {
                int length = m_name_.AlgorithmLength;
                if (m_algorithmIndex_ < length)
                {
                    while (m_algorithmIndex_ < length)
                    {
                        // find the algorithm range that could contain m_current_
                        if (m_algorithmIndex_ < 0 ||
                            m_name_.GetAlgorithmEnd(m_algorithmIndex_) <
                            m_current_)
                        {
                            m_algorithmIndex_++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (m_algorithmIndex_ < length)
                    {
                        // interleave the data-driven ones with the algorithmic ones
                        // iterate over all algorithmic ranges; assume that they are
                        // in ascending order
                        int start = m_name_.GetAlgorithmStart(m_algorithmIndex_);
                        if (m_current_ < start)
                        {
                            // this should get rid of those codepoints that are not
                            // in the algorithmic range
                            int end = start;
                            if (m_limit_ <= start)
                            {
                                end = m_limit_;
                            }
                            if (!IterateGroup(element, end))
                            {
                                m_current_++;
                                return true;
                            }
                        }
                        /*
                        // "if (m_current_ >= m_limit_)" would not return true
                        // because it can never be reached due to:
                        // 1) It has already been checked earlier
                        // 2) When m_current_ is updated earlier, it returns true
                        // 3) No updates on m_limit_*/
                        if (m_current_ >= m_limit_)
                        {
                            // after iterateGroup fails, current codepoint may be
                            // greater than limit
                            return false;
                        }

                        element.Integer = m_current_;
                        element.Value = m_name_.GetAlgorithmName(m_algorithmIndex_,
                                                                       m_current_);
                        // reset the group index if we are in the algorithmic names
                        m_groupIndex_ = -1;
                        m_current_++;
                        return true;
                    }
                }
            }
            // enumerate the character names after the last algorithmic range
            if (!IterateGroup(element, m_limit_))
            {
                m_current_++;
                return true;
            }
            else if (m_choice_ == (int)UCharacterNameChoice.ExtendedCharName)
            {
                if (!IterateExtended(element, m_limit_))
                {
                    m_current_++;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the current <see cref="RangeValueEnumeratorElement"/> in the iteration.
        /// </summary>
        public ValueEnumeratorElement Current => current;

        object IEnumerator.Current => current;

        /// <summary>
        /// Gets the next result for this iteration and returns
        /// true if we are not at the end of the iteration, false otherwise.
        /// </summary>
        /// <returns>true if we are not at the end of the iteration, false otherwise.</returns>
        /// <seealso cref="ValueEnumeratorElement"/>
        public bool MoveNext()
        {
            return Next(current);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            // Nothing to do
        }

        /// <summary>
        /// Resets the iterator to start iterating from the integer index
        /// <see cref="UCharacter.MinValue"/> or X if a <c>SetRange(X, Y)</c> has been called previously.
        /// </summary>
        public virtual void Reset()
        {
            m_current_ = m_start_;
            m_groupIndex_ = -1;
            m_algorithmIndex_ = -1;
        }

        /// <summary>
        /// Restricts the range of integers to iterate and resets the iteration
        /// to begin at the index argument start.
        /// </summary>
        /// <remarks>
        /// If <see cref="SetRange(int, int)"/> is not performed before <see cref="MoveNext()"/> is
        /// called, the iteration will start from the integer index
        /// <see cref="UCharacter.MinValue"/> and end at <see cref="UCharacter.MaxValue"/>.
        /// <para/>
        /// If this range is set outside the range of <see cref="UCharacter.MinValue"/> and 
        /// <see cref="UCharacter.MaxValue"/>, <see cref="MoveNext()"/> will always return false.
        /// </remarks>
        /// <param name="start">First integer in range to iterate.</param>
        /// <param name="limit">1 integer after the last integer in range.</param>
        /// <exception cref="ArgumentException">Thrown when attempting to set an
        /// illegal range. E.g limit &lt;= start.</exception>
        public virtual void SetRange(int start, int limit)
        {
            if (start >= limit)
            {
                throw new ArgumentException(
                    "start or limit has to be valid Unicode codepoints and start < limit");
            }
            if (start < UCharacter.MinValue)
            {
                m_start_ = UCharacter.MinValue;
            }
            else
            {
                m_start_ = start;
            }

            if (limit > UCharacter.MaxValue + 1)
            {
                m_limit_ = UCharacter.MaxValue + 1;
            }
            else
            {
                m_limit_ = limit;
            }
            m_current_ = m_start_;
        }

        // protected constructor ---------------------------------------------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name data.</param>
        /// <param name="choice">Name choice from the class <see cref="UCharacterNameChoice"/>.</param>
        public UCharacterNameEnumerator(UCharacterName name, UCharacterNameChoice choice)
        {
            if (name == null)
            {
                throw new ArgumentException("UCharacterName name argument cannot be null. Missing unames.icu?");
            }
            m_name_ = name;
            // no explicit choice in UCharacter so no checks on choice
            m_choice_ = (int)choice;
            m_start_ = UCharacter.MinValue;
            m_limit_ = UCharacter.MaxValue + 1;
            m_current_ = m_start_;
        }

        // private data members ---------------------------------------------

        /// <summary>
        /// Name data
        /// </summary>
        private UCharacterName m_name_;
        /// <summary>
        /// Name choice
        /// </summary>
        private int m_choice_;
        /// <summary>
        /// Start iteration range
        /// </summary>
        private int m_start_;
        /// <summary>
        /// End + 1 iteration range
        /// </summary>
        private int m_limit_;
        /// <summary>
        /// Current codepoint
        /// </summary>
        private int m_current_;
        /// <summary>
        /// Group index
        /// </summary>
        private int m_groupIndex_ = -1;
        /// <summary>
        /// Algorithm index
        /// </summary>
        private int m_algorithmIndex_ = -1;
        /// <summary>
        /// Group use
        /// </summary>
        private static char[] GROUP_OFFSETS_ =
                                    new char[UCharacterName.LINES_PER_GROUP_ + 1];
        private static char[] GROUP_LENGTHS_ =
                                    new char[UCharacterName.LINES_PER_GROUP_ + 1];

        /// <summary>
        /// Current enumerator element
        /// </summary>
        private ValueEnumeratorElement current = new ValueEnumeratorElement();

        // private methods --------------------------------------------------

        /// <summary>
        /// Group name iteration, iterate all the names in the current 32-group and
        /// returns the first codepoint that has a valid name.
        /// </summary>
        /// <param name="result">Stores the result codepoint and name.</param>
        /// <param name="limit">Last codepoint + 1 in range to search.</param>
        /// <returns>false if a codepoint with a name is found in group and we can
        /// bail from further iteration, true to continue on with the
        /// iteration.</returns>
        private bool IterateSingleGroup(ValueEnumeratorElement result, int limit)
        {
            lock (GROUP_OFFSETS_)
            {
                lock (GROUP_LENGTHS_)
                {
                    int index = m_name_.GetGroupLengths(m_groupIndex_, GROUP_OFFSETS_,
                                                        GROUP_LENGTHS_);
                    while (m_current_ < limit)
                    {
                        int offset = UCharacterName.GetGroupOffset(m_current_);
                        string name = m_name_.GetGroupName(
                                                  index + GROUP_OFFSETS_[offset],
                                                  GROUP_LENGTHS_[offset], (UCharacterNameChoice)m_choice_);
                        if ((name == null || name.Length == 0) &&
                            m_choice_ == (int)UCharacterNameChoice.ExtendedCharName)
                        {
                            name = m_name_.GetExtendedName(m_current_);
                        }
                        if (name != null && name.Length > 0)
                        {
                            result.Integer = m_current_;
                            result.Value = name;
                            return false;
                        }
                        ++m_current_;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Group name iteration, iterate all the names in the current 32-group and
        /// returns the first codepoint that has a valid name.
        /// </summary>
        /// <param name="result">Stores the result codepoint and name.</param>
        /// <param name="limit">Last codepoint + 1 in range to search.</param>
        /// <returns>false if a codepoint with a name is found in group and we can
        /// bail from further iteration, true to continue on with the
        /// iteration.</returns>
        private bool IterateGroup(ValueEnumeratorElement result, int limit)
        {
            if (m_groupIndex_ < 0)
            {
                m_groupIndex_ = m_name_.GetGroup(m_current_);
            }

            while (m_groupIndex_ < m_name_.m_groupcount_ &&
                   m_current_ < limit)
            {
                // iterate till the last group or the last codepoint
                int startMSB = UCharacterName.GetCodepointMSB(m_current_);
                int gMSB = m_name_.GetGroupMSB(m_groupIndex_); // can be -1
                if (startMSB == gMSB)
                {
                    if (startMSB == UCharacterName.GetCodepointMSB(limit - 1))
                    {
                        // if start and limit - 1 are in the same group, then enumerate
                        // only in that one
                        return IterateSingleGroup(result, limit);
                    }
                    // enumerate characters in the partial start group
                    // if (m_name_.getGroupOffset(m_current_) != 0) {
                    if (!IterateSingleGroup(result,
                                            UCharacterName.GetGroupLimit(gMSB)))
                    {
                        return false;
                    }
                    ++m_groupIndex_; // continue with the next group
                }
                else if (startMSB > gMSB)
                {
                    // make sure that we start enumerating with the first group
                    // after start
                    m_groupIndex_++;
                }
                else
                {
                    int gMIN = UCharacterName.GetGroupMin(gMSB);
                    if (gMIN > limit)
                    {
                        gMIN = limit;
                    }
                    if (m_choice_ == (int)UCharacterNameChoice.ExtendedCharName)
                    {
                        if (!IterateExtended(result, gMIN))
                        {
                            return false;
                        }
                    }
                    m_current_ = gMIN;
                }
            }

            return true;
        }

        /// <summary>
        /// Iterate extended names.
        /// </summary>
        /// <param name="result">Stores the result codepoint and name.</param>
        /// <param name="limit">Last codepoint + 1 in range to search.</param>
        /// <returns>false if a codepoint with a name is found and we can
        /// bail from further iteration, true to continue on with the
        /// iteration (this will always be false for valid codepoints).</returns>
        private bool IterateExtended(ValueEnumeratorElement result,
                                        int limit)
        {
            while (m_current_ < limit)
            {
                string name = m_name_.GetExtendedOr10Name(m_current_);
                if (name != null && name.Length > 0)
                {
                    result.Integer = m_current_;
                    result.Value = name;
                    return false;
                }
                ++m_current_;
            }
            return true;
        }
    }
}
