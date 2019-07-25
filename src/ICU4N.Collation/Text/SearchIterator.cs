using ICU4N.Support.Text;
using ICU4N.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    public abstract class SearchIterator
    {
        /**
         * The BreakIterator to define the boundaries of a logical match.
         * This value can be a null.
         * See class documentation for more information.
         * @see #setBreakIterator(BreakIterator)
         * @see #getBreakIterator
         * @see BreakIterator
         * @stable ICU 2.0
         */
        protected BreakIterator m_breakIterator;

        /**
         * Target text for searching.
         * @see #setTarget(CharacterIterator)
         * @see #getTarget
         * @stable ICU 2.0
         */
        protected CharacterIterator m_targetText;
        /**
         * Length of the most current match in target text. 
         * Value 0 is the default value.
         * @see #setMatchLength
         * @see #getMatchLength
         * @stable ICU 2.0
         */
        protected int m_matchLength;

        /**
         * Java port of ICU4C struct USearch (usrchimp.h)
         * 
         * Note:
         * 
         *  ICU4J already exposed some protected members such as
         * targetText, breakIterator and matchedLength as a part of stable
         * APIs. In ICU4C, they are exposed through USearch struct, 
         * although USearch struct itself is internal API.
         * 
         *  This class was created for making ICU4J code parallel to
         * ICU4C implementation. ICU4J implementation access member
         * fields like C struct (e.g. search_.isOverlap_) mostly, except
         * fields already exposed as protected member (e.g. search_.text()).
         * 
         */
        internal sealed class Search
        {
            private readonly SearchIterator outerInstance;
            public Search(SearchIterator outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            internal CharacterIterator Text
            {
                get { return outerInstance.m_targetText; }
            }

            internal void SetTarget(CharacterIterator text)
            {
                outerInstance.m_targetText = text;
            }

            /** Flag to indicate if overlapping search is to be done.
                E.g. looking for "aa" in "aaa" will yield matches at offset 0 and 1. */
            internal bool isOverlap_;

            internal bool isCanonicalMatch_;

            internal SearchIteratorElementComparisonType elementComparisonType_;

            internal BreakIterator internalBreakIter_;

            internal BreakIterator BreakIterator
            {
                get { return outerInstance.m_breakIterator; }
                set { outerInstance.m_breakIterator = value; }
            }

            internal int matchedIndex_;

            internal int MatchedLength
            {
                get { return outerInstance.m_matchLength; }
                set { outerInstance.m_matchLength = value; }
            }

            /** Flag indicates if we are doing a forwards search */
            internal bool isForwardSearching_;

            /** Flag indicates if we are at the start of a string search.
                This indicates that we are in forward search and at the start of m_text. */
            internal bool reset_;

            // Convenient methods for accessing begin/end index of the
            // target text. These are ICU4J only and are not data fields.
            internal int BeginIndex
            {
                get
                {
                    if (outerInstance.m_targetText == null)
                    {
                        return 0;
                    }
                    return outerInstance.m_targetText.BeginIndex;
                }
            }

            internal int EndIndex
        {
            get
            {
                if (outerInstance.m_targetText == null)
                {
                    return 0;
                }
                return outerInstance.m_targetText.EndIndex;
            }
        }
        }

        internal Search search_;

        // public data members -------------------------------------------------

        /**
         * DONE is returned by previous() and next() after all valid matches have 
         * been returned, and by first() and last() if there are no matches at all.
         * @see #previous
         * @see #next
         * @stable ICU 2.0
         */
        public const int Done = -1;

        // public methods -----------------------------------------------------

        // public setters -----------------------------------------------------

        /**
         * <p>
         * Sets the position in the target text at which the next search will start.
         * This method clears any previous match.
         * </p>
         * @param position position from which to start the next search
         * @exception IndexOutOfBoundsException thrown if argument position is out
         *            of the target text range.
         * @see #getIndex
         * @stable ICU 2.8
         */
        public virtual void SetIndex(int position)
        {
            if (position < search_.BeginIndex
                || position > search_.EndIndex)
            {
                throw new IndexOutOfRangeException(
                    "setIndex(int) expected position to be between " +
                    search_.BeginIndex + " and " + search_.EndIndex);
            }
            search_.reset_ = false;
            search_.MatchedLength =0;
            search_.matchedIndex_ = Done;
        }

        /**
         * Determines whether overlapping matches are returned. See the class 
         * documentation for more information about overlapping matches.
         * <p>
         * The default setting of this property is false
         * 
         * @param allowOverlap flag indicator if overlapping matches are allowed
         * @see #isOverlapping
         * @stable ICU 2.8
         */
        //public void setOverlapping(boolean allowOverlap)
        //{
        //    search_.isOverlap_ = allowOverlap;
        //}

        /**
         * Set the BreakIterator that will be used to restrict the points
         * at which matches are detected.
         * 
         * @param breakiter A BreakIterator that will be used to restrict the 
         *                points at which matches are detected. If a match is 
         *                found, but the match's start or end index is not a 
         *                boundary as determined by the {@link BreakIterator}, 
         *                the match will be rejected and another will be searched 
         *                for. If this parameter is <tt>null</tt>, no break
         *                detection is attempted.
         * @see BreakIterator
         * @stable ICU 2.0
         */
        //public void setBreakIterator(BreakIterator breakiter)
        //{
        //    search_.setBreakIter(breakiter);
        //    if (search_.BreakIter() != null)
        //    {
        //        // Create a clone of CharacterItearator, so it won't
        //        // affect the position currently held by search_.text()
        //        if (search_.Text() != null)
        //        {
        //            search_.BreakIter().setText((CharacterIterator)search_.Text().clone());
        //        }
        //    }
        //}

        /**
         * Set the target text to be searched. Text iteration will then begin at 
         * the start of the text string. This method is useful if you want to 
         * reuse an iterator to search within a different body of text.
         * 
         * @param text new text iterator to look for match, 
         * @exception IllegalArgumentException thrown when text is null or has
         *               0 length
         * @see #getTarget
         * @stable ICU 2.4
         */
        //public void setTarget(CharacterIterator text)
        //{
        //    if (text == null || text.getEndIndex() == text.getIndex())
        //    {
        //        throw new IllegalArgumentException("Illegal null or empty text");
        //    }

        //    text.setIndex(text.getBeginIndex());
        //    search_.SetTarget(text);
        //    search_.matchedIndex_ = DONE;
        //    search_.SetMatchedLength(0);
        //    search_.reset_ = true;
        //    search_.isForwardSearching_ = true;
        //    if (search_.BreakIter() != null)
        //    {
        //        // Create a clone of CharacterItearator, so it won't
        //        // affect the position currently held by search_.text()
        //        search_.BreakIter().setText((CharacterIterator)text.clone());
        //    }
        //    if (search_.internalBreakIter_ != null)
        //    {
        //        search_.internalBreakIter_.setText((CharacterIterator)text.clone());
        //    }
        //}

        //TODO: We may add APIs below to match ICU4C APIs
        // setCanonicalMatch

        // public getters ----------------------------------------------------

        /**
        * Returns the index to the match in the text string that was searched.
        * This call returns a valid result only after a successful call to 
        * {@link #first}, {@link #next}, {@link #previous}, or {@link #last}.
        * Just after construction, or after a searching method returns 
        * {@link #DONE}, this method will return {@link #DONE}.
        * <p>
        * Use {@link #getMatchLength} to get the matched string length.
        * 
        * @return index of a substring within the text string that is being 
        *         searched.
        * @see #first
        * @see #next
        * @see #previous
        * @see #last
        * @stable ICU 2.0
        */
        public virtual int MatchStart
        {
            get { return search_.matchedIndex_; }
        }

        /**
         * Return the current index in the text being searched.
         * If the iteration has gone past the end of the text
         * (or past the beginning for a backwards search), {@link #DONE}
         * is returned.
         * 
         * @return current index in the text being searched.
         * @stable ICU 2.8
         */
        public abstract int Index { get; }

        /**
         * Returns the length of text in the string which matches the search 
         * pattern. This call returns a valid result only after a successful call 
         * to {@link #first}, {@link #next}, {@link #previous}, or {@link #last}.
         * Just after construction, or after a searching method returns 
         * {@link #DONE}, this method will return 0.
         * 
         * @return The length of the match in the target text, or 0 if there
         *         is no match currently.
         * @see #first
         * @see #next
         * @see #previous
         * @see #last
         * @stable ICU 2.0
         */
        public virtual int MatchLength
        {
            get { return search_.MatchedLength; }
            set { search_.MatchedLength= value; }
        }

        /**
         * Returns the BreakIterator that is used to restrict the indexes at which 
         * matches are detected. This will be the same object that was passed to 
         * the constructor or to {@link #setBreakIterator}.
         * If the {@link BreakIterator} has not been set, <tt>null</tt> will be returned.
         * See {@link #setBreakIterator} for more information.
         * 
         * @return the BreakIterator set to restrict logic matches
         * @see #setBreakIterator
         * @see BreakIterator
         * @stable ICU 2.0
         */
        public virtual BreakIterator BreakIterator
        {
            get { return search_.BreakIterator; }
            set
            {
                search_.BreakIterator = value;
                if (search_.BreakIterator != null)
                {
                    // Create a clone of CharacterItearator, so it won't
                    // affect the position currently held by search_.text()
                    if (search_.Text != null)
                    {
                        search_.BreakIterator.SetText((CharacterIterator)search_.Text.Clone());
                    }
                }
            }
        }

        /**
         * Return the string text to be searched.
         * @return text string to be searched.
         * @stable ICU 2.0
         */
        public virtual CharacterIterator Target
        {
            get { return search_.Text; }
            set
            {
                if (value == null || value.EndIndex == value.Index)
                {
                    throw new ArgumentException("Illegal null or empty text");
                }

                value.SetIndex(value.BeginIndex);
                search_.SetTarget(value);
                search_.matchedIndex_ = Done;
                search_.MatchedLength = 0;
                search_.reset_ = true;
                search_.isForwardSearching_ = true;
                if (search_.BreakIterator != null)
                {
                    // Create a clone of CharacterItearator, so it won't
                    // affect the position currently held by search_.text()
                    search_.BreakIterator.SetText((CharacterIterator)value.Clone());
                }
                if (search_.internalBreakIter_ != null)
                {
                    search_.internalBreakIter_.SetText((CharacterIterator)value.Clone());
                }
            }
        }

        /**
         * Returns the text that was matched by the most recent call to 
         * {@link #first}, {@link #next}, {@link #previous}, or {@link #last}.
         * If the iterator is not pointing at a valid match (e.g. just after 
         * construction or after {@link #DONE} has been returned, 
         * returns an empty string. 
         * 
         * @return  the substring in the target test of the most recent match,
         *          or null if there is no match currently.
         * @see #first
         * @see #next
         * @see #previous
         * @see #last
         * @stable ICU 2.0
         */
        public string MatchedText
        {
            get
            {
                if (search_.MatchedLength > 0)
                {
                    int limit = search_.matchedIndex_ + search_.MatchedLength;
                    StringBuilder result = new StringBuilder(search_.MatchedLength);
                    CharacterIterator it = search_.Text;
                    it.SetIndex(search_.matchedIndex_);
                    while (it.Index < limit)
                    {
                        result.Append(it.Current);
                        it.Next();
                    }
                    it.SetIndex(search_.matchedIndex_);
                    return result.ToString();
                }
                return null;
            }
        }

        // miscellaneous public methods -----------------------------------------

        /**
         * Returns the index of the next point at which the text matches the
         * search pattern, starting from the current position
         * The iterator is adjusted so that its current index (as returned by 
         * {@link #getIndex}) is the match position if one was found.
         * If a match is not found, {@link #DONE} will be returned and
         * the iterator will be adjusted to a position after the end of the text 
         * string.
         * 
         * @return The index of the next match after the current position,
         *          or {@link #DONE} if there are no more matches.
         * @see #getIndex
         * @stable ICU 2.0
         */
        public virtual int Next()
        {
            int index = Index; // offset = getOffset() in ICU4C
            int matchindex = search_.matchedIndex_;
            int matchlength = search_.MatchedLength;
            search_.reset_ = false;
            if (search_.isForwardSearching_)
            {
                int endIdx = search_.EndIndex;
                if (index == endIdx || matchindex == endIdx ||
                        (matchindex != Done &&
                        matchindex + matchlength >= endIdx))
                {
#pragma warning disable 612, 618
                    SetMatchNotFound();
#pragma warning restore 612, 618
                    return Done;
                }
            }
            else
            {
                // switching direction.
                // if matchedIndex == DONE, it means that either a 
                // setIndex (setOffset in C) has been called or that previous ran off the text
                // string. the iterator would have been set to offset 0 if a 
                // match is not found.
                search_.isForwardSearching_ = true;
                if (search_.matchedIndex_ != Done)
                {
                    // there's no need to set the collation element iterator
                    // the next call to next will set the offset.
                    return matchindex;
                }
            }

            if (matchlength > 0)
            {
                // if matchlength is 0 we are at the start of the iteration
                if (search_.isOverlap_)
                {
                    index++;
                }
                else
                {
                    index += matchlength;
                }
            }

            return HandleNext(index);
        }

        /**
         * Returns the index of the previous point at which the string text 
         * matches the search pattern, starting at the current position.
         * The iterator is adjusted so that its current index (as returned by 
         * {@link #getIndex}) is the match position if one was found.
         * If a match is not found, {@link #DONE} will be returned and
         * the iterator will be adjusted to the index {@link #DONE}.
         * 
         * @return The index of the previous match before the current position,
         *          or {@link #DONE} if there are no more matches.
         * @see #getIndex
         * @stable ICU 2.0
         */
        public virtual int Previous()
        {
            int index;  // offset in ICU4C
            if (search_.reset_)
            {
                index = search_.EndIndex;   // m_search_->textLength in ICU4C
                search_.isForwardSearching_ = false;
                search_.reset_ = false;
                SetIndex(index);
            }
            else
            {
                index = Index;
            }

            int matchindex = search_.matchedIndex_;
            if (search_.isForwardSearching_)
            {
                // switching direction. 
                // if matchedIndex == DONE, it means that either a 
                // setIndex (setOffset in C) has been called or that next ran off the text
                // string. the iterator would have been set to offset textLength if 
                // a match is not found.
                search_.isForwardSearching_ = false;
                if (matchindex != Done)
                {
                    return matchindex;
                }
            }
            else
            {
                int startIdx = search_.BeginIndex;
                if (index == startIdx || matchindex == startIdx)
                {
                    // not enough characters to match
#pragma warning disable 612, 618
                    SetMatchNotFound();
#pragma warning restore 612, 618
                    return Done;
                }
            }

            if (matchindex != Done)
            {
                if (search_.isOverlap_)
                {
                    matchindex += search_.MatchedLength - 2;
                }

                return HandlePrevious(matchindex);
            }

            return HandlePrevious(index);
        }

        /**
         * Return true if the overlapping property has been set.
         * See {@link #setOverlapping(boolean)} for more information.
         * 
         * @see #setOverlapping
         * @return true if the overlapping property has been set, false otherwise
         * @stable ICU 2.8
         */
        public virtual bool IsOverlapping
        {
            get { return search_.isOverlap_; }
            set { search_.isOverlap_ = value; }
        }

        //TODO: We may add APIs below to match ICU4C APIs
        // isCanonicalMatch

        /** 
        * Resets the iteration.
        * Search will begin at the start of the text string if a forward
        * iteration is initiated before a backwards iteration. Otherwise if a
        * backwards iteration is initiated before a forwards iteration, the
        * search will begin at the end of the text string.
        * 
        * @stable ICU 2.0
        */
        public virtual void Reset()
        {
#pragma warning disable 612, 618
            SetMatchNotFound();
#pragma warning restore 612, 618
            SetIndex(search_.BeginIndex);
            search_.isOverlap_ = false;
            search_.isCanonicalMatch_ = false;
            search_.elementComparisonType_ = SearchIteratorElementComparisonType.StandardElementComparison;
            search_.isForwardSearching_ = true;
            search_.reset_ = true;
        }

        /**
         * Returns the first index at which the string text matches the search 
         * pattern. The iterator is adjusted so that its current index (as 
         * returned by {@link #getIndex()}) is the match position if one 
         * 
         * was found.
         * If a match is not found, {@link #DONE} will be returned and
         * the iterator will be adjusted to the index {@link #DONE}.
         * @return The character index of the first match, or 
         *         {@link #DONE} if there are no matches.
         * 
         * @see #getIndex
         * @stable ICU 2.0
         */
        public int First()
        {
            int startIdx = search_.BeginIndex;
            SetIndex(startIdx);
            return HandleNext(startIdx);
        }

        /**
         * Returns the first index equal or greater than <tt>position</tt> at which the 
         * string text matches the search pattern. The iterator is adjusted so 
         * that its current index (as returned by {@link #getIndex()}) is the 
         * match position if one was found.
         * If a match is not found, {@link #DONE} will be returned and the
         * iterator will be adjusted to the index {@link #DONE}.
         * 
         * @param  position where search if to start from.
         * @return The character index of the first match following 
         *         <tt>position</tt>, or {@link #DONE} if there are no matches.
         * @throws IndexOutOfBoundsException    If position is less than or greater
         *      than the text range for searching.
         * @see #getIndex
         * @stable ICU 2.0
         */
        public int Following(int position)
        {
            SetIndex(position);
            return HandleNext(position);
        }

        /**
         * Returns the last index in the target text at which it matches the
         * search pattern. The iterator is adjusted so that its current index
         * (as returned by {@link #getIndex}) is the match position if one was
         * found.
         * If a match is not found, {@link #DONE} will be returned and
         * the iterator will be adjusted to the index {@link #DONE}.
         * 
         * @return The index of the first match, or {@link #DONE} if 
         *         there are no matches.
         * @see #getIndex
         * @stable ICU 2.0
         */
        public int Last()
        {
            int endIdx = search_.EndIndex;
            SetIndex(endIdx);
            return HandlePrevious(endIdx);
        }

        /**
         * Returns the first index less than <tt>position</tt> at which the string 
         * text matches the search pattern. The iterator is adjusted so that its 
         * current index (as returned by {@link #getIndex}) is the match 
         * position if one was found. If a match is not found, 
         * {@link #DONE} will be returned and the iterator will be 
         * adjusted to the index {@link #DONE}
         * <p>
         * When the overlapping option ({@link #isOverlapping}) is off, the last index of the
         * result match is always less than <tt>position</tt>.
         * When the overlapping option is on, the result match may span across
         * <tt>position</tt>.
         *
         * @param  position where search is to start from.
         * @return The character index of the first match preceding 
         *         <tt>position</tt>, or {@link #DONE} if there are 
         *         no matches.
         * @throws IndexOutOfBoundsException If position is less than or greater than
         *                                   the text range for searching
         * @see #getIndex
         * @stable ICU 2.0
         */
        public int Preceding(int position)
        {
            SetIndex(position);
            return HandlePrevious(position);
        }

        // protected constructor ----------------------------------------------

        /**
         * Protected constructor for use by subclasses.
         * Initializes the iterator with the argument target text for searching 
         * and sets the BreakIterator.
         * See class documentation for more details on the use of the target text
         * and {@link BreakIterator}.
         * 
         * @param target The target text to be searched.
         * @param breaker A {@link BreakIterator} that is used to determine the 
         *                boundaries of a logical match. This argument can be null.
         * @exception IllegalArgumentException thrown when argument target is null,
         *            or of length 0
         * @see BreakIterator  
         * @stable ICU 2.0
         */
        protected SearchIterator(CharacterIterator target, BreakIterator breaker)
        {
            this.search_ = new Search(this);

            if (target == null
                || (target.EndIndex - target.BeginIndex) == 0)
            {
                throw new ArgumentException(
                                   "Illegal argument target. " +
                                   " Argument can not be null or of length 0");
            }

            search_.SetTarget(target);
            search_.BreakIterator = breaker;
            if (search_.BreakIterator != null)
            {
                search_.BreakIterator.SetText((CharacterIterator)target.Clone());
            }
            search_.isOverlap_ = false;
            search_.isCanonicalMatch_ = false;
            search_.elementComparisonType_ = SearchIteratorElementComparisonType.StandardElementComparison;
            search_.isForwardSearching_ = true;
            search_.reset_ = true;
            search_.matchedIndex_ = Done;
            search_.MatchedLength = 0;
        }

        // protected methods --------------------------------------------------


        /**
         * Sets the length of the most recent match in the target text. 
         * Subclasses' handleNext() and handlePrevious() methods should call this 
         * after they find a match in the target text.
         * 
         * @param length new length to set
         * @see #handleNext
         * @see #handlePrevious
         * @stable ICU 2.0
         */
        //protected void setMatchLength(int length)
        //{
        //    search_.SetMatchedLength(length);
        //}

        /**
         * Abstract method which subclasses override to provide the mechanism
         * for finding the next match in the target text. This allows different
         * subclasses to provide different search algorithms.
         * <p>
         * If a match is found, the implementation should return the index at
         * which the match starts and should call 
         * {@link #setMatchLength} with the number of characters 
         * in the target text that make up the match. If no match is found, the 
         * method should return {@link #DONE}.
         * 
         * @param start The index in the target text at which the search 
         *              should start.
         * @return index at which the match starts, else if match is not found 
         *         {@link #DONE} is returned
         * @see #setMatchLength
         * @stable ICU 2.0
         */
        protected abstract int HandleNext(int start);

        /**
         * Abstract method which subclasses override to provide the mechanism for
         * finding the previous match in the target text. This allows different
         * subclasses to provide different search algorithms.
         * <p>
         * If a match is found, the implementation should return the index at
         * which the match starts and should call 
         * {@link #setMatchLength} with the number of characters 
         * in the target text that make up the match. If no match is found, the 
         * method should return {@link #DONE}.
         * 
         * @param startAt   The index in the target text at which the search 
         *                  should start.
         * @return index at which the match starts, else if match is not found 
         *         {@link #DONE} is returned
         * @see #setMatchLength
         * @stable ICU 2.0
         */
        protected abstract int HandlePrevious(int startAt);

        /**
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
    //TODO: This protected method is @stable 2.0 in ICU4C
    protected void SetMatchNotFound()
        {
            search_.matchedIndex_ = Done;
            search_.MatchedLength=0;
        }



        ///**
        // * Sets the collation element comparison type.
        // * <p>
        // * The default comparison type is {@link ElementComparisonType#STANDARD_ELEMENT_COMPARISON}.
        // * 
        // * @see ElementComparisonType
        // * @see #getElementComparisonType()
        // * @stable ICU 53
        // */
        //public void setElementComparisonType(ElementComparisonType type)
        //{
        //    search_.elementComparisonType_ = type;
        //}

        ///**
        // * Returns the collation element comparison type.
        // * 
        // * @see ElementComparisonType
        // * @see #setElementComparisonType(ElementComparisonType)
        // * @stable ICU 53
        // */
        //public ElementComparisonType getElementComparisonType()
        //{
        //    return search_.elementComparisonType_;
        //}

        public virtual SearchIteratorElementComparisonType ElementComparisonType
        {
            get { return search_.elementComparisonType_; }
            set { search_.elementComparisonType_ = value; }
        }
    }

    /**
     * Option to control how collation elements are compared.
     * The default value will be {@link #STANDARD_ELEMENT_COMPARISON}.
     * <p>
     * PATTERN_BASE_WEIGHT_IS_WILDCARD supports "asymmetric search" as described in
     * <a href="http://www.unicode.org/reports/tr10/#Asymmetric_Search">
     * UTS #10 Unicode Collation Algorithm</a>, while ANY_BASE_WEIGHT_IS_WILDCARD
     * supports a related option in which "unmarked" characters in either the
     * pattern or the searched text are treated as wildcards that match marked or
     * unmarked versions of the same character.
     * 
     * @see #setElementComparisonType(ElementComparisonType)
     * @see #getElementComparisonType()
     * @stable ICU 53
     */
    public enum SearchIteratorElementComparisonType
    {
        /**
         * Standard collation element comparison at the specified collator strength.
         * 
         * @stable ICU 53
         */
        StandardElementComparison,
        /**
         * Collation element comparison is modified to effectively provide behavior
         * between the specified strength and strength - 1.
         * <p>
         * Collation elements in the pattern that have the base weight for the specified
         * strength are treated as "wildcards" that match an element with any other
         * weight at that collation level in the searched text. For example, with a
         * secondary-strength English collator, a plain 'e' in the pattern will match
         * a plain e or an e with any diacritic in the searched text, but an e with
         * diacritic in the pattern will only match an e with the same diacritic in
         * the searched text.
         * 
         * @stable ICU 53
         */
        PatternBaseWeightIsWildcard,

        /**
         * Collation element comparison is modified to effectively provide behavior
         * between the specified strength and strength - 1.
         * <p>
         * Collation elements in either the pattern or the searched text that have the
         * base weight for the specified strength are treated as "wildcards" that match
         * an element with any other weight at that collation level. For example, with
         * a secondary-strength English collator, a plain 'e' in the pattern will match
         * a plain e or an e with any diacritic in the searched text, but an e with
         * diacritic in the pattern will only match an e with the same diacritic or a
         * plain e in the searched text.
         * 
         * @stable ICU 53
         */
        AnyBaseWeightIsWildcard
    }
}
