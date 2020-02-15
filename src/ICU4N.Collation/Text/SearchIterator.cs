using J2N.Text;
using System;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// <see cref="SearchIterator"/> is an abstract base class that provides 
    /// methods to search for a pattern within a text string. Instances of
    /// <see cref="SearchIterator"/> maintain a current position and scan over the 
    /// target text, returning the indices the pattern is matched and the length 
    /// of each match.
    /// </summary>
    /// <remarks>
    /// <see cref="SearchIterator"/> defines a protocol for text searching. 
    /// Subclasses provide concrete implementations of various search algorithms. 
    /// For example, <see cref="StringSearch"/> implements language-sensitive pattern 
    /// matching based on the comparison rules defined in a 
    /// <see cref="RuleBasedCollator"/> object.
    /// <para/>
    /// Other options for searching include using a <see cref="Text.BreakIterator"/> to restrict
    /// the points at which matches are detected.
    /// <para/>
    /// <see cref="SearchIterator"/> provides an API that is similar to that of
    /// other text iteration classes such as <see cref="Text.BreakIterator"/>. Using 
    /// this class, it is easy to scan through text looking for all occurrences of 
    /// a given pattern. The following example uses a <see cref="StringSearch"/>
    /// object to find all instances of "fox" in the target string. Any other 
    /// subclass of <see cref="SearchIterator"/> can be used in an identical 
    /// manner.
    /// <code>
    /// string target = "The quick brown fox jumped over the lazy fox";
    /// string pattern = "fox";
    /// SearchIterator iter = new StringSearch(pattern, target);
    /// for (int pos = iter.First(); pos != SearchIterator.Done; pos = iter.Next())
    /// {
    ///     Console.WriteLine($"Found match at {pos}, length is {iter.MatchLength}");
    /// }
    /// </code>
    /// </remarks>
    /// <author>Laura Werner, synwee</author>
    /// <stable>ICU 2.0</stable>
    /// <seealso cref="Text.BreakIterator"/>
    /// <seealso cref="RuleBasedCollator"/>
    public abstract class SearchIterator
    {
        /// <summary>
        /// The <see cref="Text.BreakIterator"/> to define the boundaries of a logical match.
        /// This value can be a null.
        /// See <see cref="SearchIterator"/> for more information.
        /// </summary>
        /// <seealso cref="SetBreakIterator(BreakIterator)"/>
        /// <seealso cref="BreakIterator"/>
        /// <stable>ICU 2.0</stable>
        protected BreakIterator m_breakIterator;

        /// <summary>
        /// Target text for searching.
        /// </summary>
        /// <seealso cref="SetTarget(CharacterIterator)"/>
        /// <seealso cref="Target"/>
        /// <stable>ICU 2.0</stable>
        protected CharacterIterator m_targetText;

        /// <summary>
        /// Length of the most current match in target text. 
        /// Value 0 is the default value.
        /// </summary>
        /// <seealso cref="MatchLength"/>
        /// <stable>ICU 2.0</stable>
        protected int m_matchLength;

        /// <summary>
        /// C# port of ICU4C struct USearch (usrchimp.h)
        /// <para/>
        /// Note:
        /// <para/>
        /// ICU4N already exposed some protected members such as
        /// <see cref="m_targetText"/>, <see cref="m_breakIterator"/> and <see cref="m_matchLength"/> as a part of stable
        /// APIs. In ICU4C, they are exposed through USearch struct, 
        /// although USearch struct itself is internal API.
        /// <para/>
        /// This class was created for making ICU4N code parallel to
        /// ICU4C implementation. ICU4N implementation access member
        /// fields like C struct (e.g. search_.isOverlap_) mostly, except
        /// fields already exposed as protected member (e.g. search_.text()).
        /// </summary>
        internal sealed class Search
        {
            private readonly SearchIterator outerInstance;
            public Search(SearchIterator outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            internal CharacterIterator Text => outerInstance.m_targetText;

            internal void SetTarget(CharacterIterator text)
            {
                outerInstance.m_targetText = text;
            }

            /// <summary>
            /// Flag to indicate if overlapping search is to be done.
            /// E.g. looking for "aa" in "aaa" will yield matches at offset 0 and 1.
            /// </summary>
            internal bool isOverlap_;

            internal bool isCanonicalMatch_;

            internal ElementComparisonType elementComparisonType_;

            internal BreakIterator internalBreakIter_;

            internal BreakIterator BreakIterator
            {
                get => outerInstance.m_breakIterator;
                set => outerInstance.m_breakIterator = value;
            }

            internal int matchedIndex_;

            internal int MatchedLength
            {
                get => outerInstance.m_matchLength;
                set => outerInstance.m_matchLength = value;
            }

            /// <summary>
            /// Flag indicates if we are doing a forwards search
            /// </summary>
            internal bool isForwardSearching_;

            /// <summary>
            /// Flag indicates if we are at the start of a string search.
            /// This indicates that we are in forward search and at the start of m_text.
            /// </summary>
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

        /// <summary>
        /// <see cref="Done"/> is returne by <see cref="Previous()"/> and <see cref="Next()"/> after all valid matches have 
        /// been returned, and by <see cref="First()"/> and <see cref="Last()"/> if there are no matches at all.
        /// </summary>
        /// <seealso cref="Previous()"/>
        /// <seealso cref="Next()"/>
        /// <stable>ICU 2.0</stable>
        public const int Done = -1;

        // public methods -----------------------------------------------------

        // public setters -----------------------------------------------------

        /// <summary>
        /// Sets the position in the target text at which the next search will start.
        /// This method clears any previous match.
        /// </summary>
        /// <param name="position">Position from which to start the next search.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown if argument position is out of the target text range.</exception>
        /// <see cref="Index"/>
        /// <stable>ICU 2.8</stable>
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

        /// <summary>
        /// Set the <see cref="Text.BreakIterator"/> that will be used to restrict the points
        /// at which matches are detected.
        /// </summary>
        /// <param name="breakIterator">
        /// A <see cref="Text.BreakIterator"/> that will be used to restrict the 
        /// points at which matches are detected. If a match is 
        /// found, but the match's start or end index is not a 
        /// boundary as determined by the <see cref="Text.BreakIterator"/>,
        /// the match will be rejected and another will be searched 
        /// for. If this parameter is <c>null</c>, no break
        /// detection is attempted.
        /// </param>
        /// <seealso cref="Text.BreakIterator"/>
        /// <stable>ICU 2.0</stable>
        public virtual void SetBreakIterator(BreakIterator breakIterator)
        {
            search_.BreakIterator = breakIterator;
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

        /// <summary>
        /// Set the target text to be searched. Text iteration will then begin at 
        /// the start of the text string. This method is useful if you want to 
        /// reuse an iterator to search within a different body of text.
        /// </summary>
        /// <param name="text">New text iterator to look for match.</param>
        /// <exception cref="ArgumentException">Thrown when text is null or has 0 length.</exception>
        /// <see cref="Target"/>
        /// <stable>ICU 2.4</stable>
        public virtual void SetTarget(CharacterIterator text)
        {
            if (text == null || text.EndIndex == text.Index)
            {
                throw new ArgumentException("Illegal null or empty text");
            }

            text.SetIndex(text.BeginIndex);
            search_.SetTarget(text);
            search_.matchedIndex_ = Done;
            search_.MatchedLength = 0;
            search_.reset_ = true;
            search_.isForwardSearching_ = true;
            if (search_.BreakIterator != null)
            {
                // Create a clone of CharacterItearator, so it won't
                // affect the position currently held by search_.text()
                search_.BreakIterator.SetText((CharacterIterator)text.Clone());
            }
            if (search_.internalBreakIter_ != null)
            {
                search_.internalBreakIter_.SetText((CharacterIterator)text.Clone());
            }
        }

        //TODO: We may add APIs below to match ICU4C APIs
        // setCanonicalMatch

        // public getters ----------------------------------------------------

        /// <summary>
        /// Gets the index to the match in the text string that was searched.
        /// This call returns a valid result only after a successful call to 
        /// <see cref="First()"/>, <see cref="Next()"/>, <see cref="Previous()"/>, or <see cref="Last()"/>.
        /// Just after construction, or after a searching method returns 
        /// <see cref="Done"/>, this property will return <see cref="Done"/>.
        /// <para/>
        /// Use <see cref="MatchLength"/> to get the matched string length.
        /// </summary>
        /// <seealso cref="First()"/>
        /// <seealso cref="Next()"/>
        /// <seealso cref="Previous()"/>
        /// <seealso cref="Last()"/>
        /// <stable>ICU 2.0</stable>
        public virtual int MatchStart => search_.matchedIndex_;

        /// <summary>
        /// Gets the current index in the text being searched.
        /// If the iteration has gone past the end of the text
        /// (or past the beginning for a backwards search), <see cref="Done"/>
        /// is returned.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        public abstract int Index { get; }

        /// <summary>
        /// Gets or sets the length of text in the string which matches the search 
        /// pattern. This call returns a valid result only after a successful call 
        /// to <see cref="First()"/>, <see cref="Next()"/>, <see cref="Previous()"/>, or <see cref="Last()"/>.
        /// Just after construction, or after a searching method returns 
        /// <see cref="Done"/>, this property will return 0.
        /// <para/>
        /// Returns the length of the match in the target text, or 0 if there
        /// is no match currently.
        /// <para/>
        /// Subclass' <see cref="HandleNext(int)"/> and <see cref="HandlePrevious(int)"/> methods should set this
        /// after they find a match in the target text.
        /// </summary>
        /// <seealso cref="First()"/>
        /// <seealso cref="Next()"/>
        /// <seealso cref="Previous()"/>
        /// <seealso cref="Last()"/>
        /// <seealso cref="HandleNext(int)"/>
        /// <seealso cref="HandlePrevious(int)"/>
        /// <stable>ICU 2.0</stable>
        public virtual int MatchLength
        {
            get => search_.MatchedLength;
            protected set => search_.MatchedLength = value;
        }

        /// <summary>
        /// Returns the <see cref="Text.BreakIterator"/> that is used to restrict the indexes at which 
        /// matches are detected. This will be the same object that was passed to 
        /// the constructor or to <see cref="SetBreakIterator(BreakIterator)"/>.
        /// If the <see cref="Text.BreakIterator"/> has not been set, <c>null</c> will be returned.
        /// <see cref="SetBreakIterator(BreakIterator)"/> for more information.
        /// </summary>
        /// <seealso cref="SetBreakIterator(BreakIterator)"/>
        /// <seealso cref="Text.BreakIterator"/>
        /// <stable>ICU 2.0</stable>
        public virtual BreakIterator BreakIterator => search_.BreakIterator;

        /// <summary>
        /// Gets the string text to be searched.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        /// <seealso cref="SetTarget(CharacterIterator)"/>
        public virtual CharacterIterator Target => search_.Text;

        /// <summary>
        /// Returns the text that was matched by the most recent call to 
        /// <see cref="First()"/>, <see cref="Next()"/>, <see cref="Previous()"/>, or <see cref="Last()"/>.
        /// If the iterator is not pointing at a valid match (e.g. just after 
        /// construction or after <see cref="Done"/> has been returned, 
        /// returns an empty string.
        /// </summary>
        /// <returns>The substring in the target test of the most recent match,
        /// or null if there is no match currently.</returns>
        /// <seealso cref="First()"/>
        /// <seealso cref="Next()"/>
        /// <seealso cref="Previous()"/>
        /// <seealso cref="Last()"/>
        /// <stable>ICU 2.0</stable>
        public virtual string GetMatchedText()
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

        // miscellaneous public methods -----------------------------------------

        /// <summary>
        /// Returns the index of the next point at which the text matches the
        /// search pattern, starting from the current position
        /// The iterator is adjusted so that its current index (as returned by 
        /// <see cref="Index"/>) is the match position if one was found.
        /// If a match is not found, <see cref="Done"/> will be returned and
        /// the iterator will be adjusted to a position after the end of the text
        /// string.
        /// </summary>
        /// <returns>The index of the next match after the current position,
        /// or <see cref="Done"/> if there are no more matches.</returns>
        /// <seealso cref="Index"/>
        /// <stable>ICU 2.0</stable>
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

        /// <summary>
        /// Returns the index of the previous point at which the string text 
        /// matches the search pattern, starting at the current position.
        /// The iterator is adjusted so that its current index (as returned by 
        /// <see cref="Index"/>) is the match position if one was found.
        /// If a match is not found, <see cref="Done"/> will be returned and
        /// the iterator will be adjusted to the index <see cref="Done"/>.
        /// </summary>
        /// <returns>The index of the previous match before the current position,
        /// or <see cref="Done"/> if there are no more matches.</returns>
        /// <seealso cref="Index"/>
        /// <stable>ICU 2.0</stable>
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

        /// <summary>
        /// Gets or sets whether overlapping matches are returned. See <see cref="SearchIterator"/>
        /// for more information about overlapping matches.
        /// <para/>
        /// The default setting of this property is false.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        public virtual bool IsOverlapping
        {
            get => search_.isOverlap_;
            set => search_.isOverlap_ = value;
        }

        //TODO: We may add APIs below to match ICU4C APIs
        // isCanonicalMatch

        /// <summary>
        /// Resets the iteration.
        /// Search will begin at the start of the text string if a forward
        /// iteration is initiated before a backwards iteration. Otherwise if a
        /// backwards iteration is initiated before a forwards iteration, the
        /// search will begin at the end of the text string.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public virtual void Reset()
        {
#pragma warning disable 612, 618
            SetMatchNotFound();
#pragma warning restore 612, 618
            SetIndex(search_.BeginIndex);
            search_.isOverlap_ = false;
            search_.isCanonicalMatch_ = false;
            search_.elementComparisonType_ = ElementComparisonType.StandardElementComparison;
            search_.isForwardSearching_ = true;
            search_.reset_ = true;
        }

        /// <summary>
        /// Returns the first index at which the string text matches the search 
        /// pattern. The iterator is adjusted so that its current index (as
        /// returned by <see cref="Index"/>) is the match position if one 
        /// was found.
        /// If a match is not found, <see cref="Done"/> will be returned and
        /// the iterator will be adjusted to the index <see cref="Done"/>.
        /// </summary>
        /// <returns>The character index of the first match, or 
        /// <see cref="Done"/> if there are no matches.</returns>
        /// <seealso cref="Index"/>
        /// <stable>ICU 2.0</stable>
        public int First()
        {
            int startIdx = search_.BeginIndex;
            SetIndex(startIdx);
            return HandleNext(startIdx);
        }

        /// <summary>
        /// Returns the first index equal or greater than <paramref name="position"/> at which the
        /// string text matches the search pattern. The iterator is adjusted so 
        /// that its current index (as returned by <see cref="Index"/>) is the 
        /// match position if one was found.
        /// If a match is not found, <see cref="Done"/> will be returned and the
        /// iterator will be adjusted to the index <see cref="Done"/>.
        /// </summary>
        /// <param name="position">Where search if to start from.</param>
        /// <returns>The character index of the first match following
        /// <paramref name="position"/>, or <see cref="Done"/> if there are no matches.</returns>
        /// <exception cref="IndexOutOfRangeException">If position is less than or greater
        /// than the text range for searching.</exception>
        /// <seealso cref="Index"/>
        /// <stable>ICU 2.0</stable>
        public int Following(int position)
        {
            SetIndex(position);
            return HandleNext(position);
        }

        /// <summary>
        /// Returns the last index in the target text at which it matches the
        /// search pattern. The iterator is adjusted so that its current index
        /// (as returned by <see cref="Index"/>) is the match position if one was
        /// found.
        /// If a match is not found, <see cref="Done"/> will be returned and
        /// the iterator will be adjusted to the index <see cref="Done"/>.
        /// </summary>
        /// <returns>The index of the first match, or <see cref="Done"/> if there are no matches.</returns>
        /// <seealso cref="Index"/>
        /// <stable>ICU 2.0</stable>
        public int Last()
        {
            int endIdx = search_.EndIndex;
            SetIndex(endIdx);
            return HandlePrevious(endIdx);
        }

        /// <summary>
        /// Returns the first index less than <paramref name="position"/> at which the string 
        /// text matches the search pattern. The iterator is adjusted so that its 
        /// current index (as returned by <see cref="Index"/>) is the match 
        /// position if one was found. If a match is not found,
        /// <see cref="Done"/> will be returned and the iterator will be 
        /// adjusted to the index <see cref="Done"/>.
        /// <para/>
        /// When the overlapping option (<see cref="IsOverlapping"/>) is off, the last index of the
        /// result match is always less than <paramref name="position"/>.
        /// When the overlapping option is on, the result match may span across
        /// <paramref name="position"/>.
        /// </summary>
        /// <param name="position">Where search is to start from.</param>
        /// <returns>The character index of the first match preceding 
        /// <paramref name="position"/>, or <see cref="Done"/> if there are no matches.</returns>
        /// <exception cref="IndexOutOfRangeException">If position is less than or greater than the text range for searching.</exception>
        /// <seealso cref="Index"/>
        /// <stable>ICU 2.0</stable>
        public int Preceding(int position)
        {
            SetIndex(position);
            return HandlePrevious(position);
        }

        // protected constructor ----------------------------------------------

        /// <summary>
        /// Protected constructor for use by subclasses.
        /// <para/>
        /// Initializes the iterator with the argument target text for searching 
        /// and sets the <see cref="Text.BreakIterator"/>.
        /// See <see cref="SearchIterator"/> for more details on the use of the target text
        /// and <see cref="Text.BreakIterator"/>.
        /// </summary>
        /// <param name="target">The target text to be searched.</param>
        /// <param name="breaker">A <see cref="Text.BreakIterator"/> that is used to determine the 
        /// boundaries of a logical match. This argument can be null.</param>
        /// <exception cref="ArgumentException">Thrown when argument target is null, or of length 0.</exception>
        /// <seealso cref="Text.BreakIterator"/>
        /// <stable>ICU 2.0</stable>
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
            search_.elementComparisonType_ = ElementComparisonType.StandardElementComparison;
            search_.isForwardSearching_ = true;
            search_.reset_ = true;
            search_.matchedIndex_ = Done;
            search_.MatchedLength = 0;
        }

        // protected methods --------------------------------------------------

        /// <summary>
        /// Abstract method which subclasses override to provide the mechanism
        /// for finding the next match in the target text. This allows different
        /// subclasses to provide different search algorithms.
        /// <para/>
        /// If a match is found, the implementation should return the index at
        /// which the match starts and should set
        /// <see cref="MatchLength"/> with the number of characters 
        /// in the target text that make up the match. If no match is found, the 
        /// method should return <see cref="Done"/>.
        /// </summary>
        /// <param name="start">The index in the target text at which the search 
        /// should start.</param>
        /// <returns>Index at which the match starts, else if match is not found
        /// <see cref="Done"/> is returned.</returns>
        /// <seealso cref="MatchLength"/>
        /// <stable>ICU 2.0</stable>
        protected abstract int HandleNext(int start);

        /// <summary>
        /// Abstract method which subclasses override to provide the mechanism for
        /// finding the previous match in the target text. This allows different
        /// subclasses to provide different search algorithms.
        /// <para/>
        /// If a match is found, the implementation should return the index at
        /// which the match starts and should set 
        /// <see cref="MatchLength"/> with the number of characters 
        /// in the target text that make up the match. If no match is found, the 
        /// method should return <see cref="Done"/>.
        /// </summary>
        /// <param name="startAt">The index in the target text at which the search
        /// should start.</param>
        /// <returns>Index at which the match starts, else if match is not found 
        /// <see cref="Done"/> is returned.</returns>
        /// <seealso cref="MatchLength"/>
        /// <stable>ICU 2.0</stable>
        protected abstract int HandlePrevious(int startAt);

        /// <summary>
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        //TODO: This protected method is @stable 2.0 in ICU4C
        internal virtual void SetMatchNotFound() // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            search_.matchedIndex_ = Done;
            search_.MatchedLength=0;
        }

        /// <summary>
        /// Gets or sets the collation element comparison type.
        /// <para/>
        /// The default comparison type is <see cref="ElementComparisonType.StandardElementComparison"/>.
        /// </summary>
        /// <seealso cref="Text.ElementComparisonType"/>
        /// <stable>ICU 53</stable>
        public virtual ElementComparisonType ElementComparisonType
        {
            get => search_.elementComparisonType_;
            set => search_.elementComparisonType_ = value;
        }
    }

    /// <summary>
    /// Option to control how collation elements are compared.
    /// The default value will be <see cref="StandardElementComparison"/>.
    /// <para/>
    /// <see cref="PatternBaseWeightIsWildcard"/> supports "asymmetric search" as described in
    /// <a href="http://www.unicode.org/reports/tr10/#Asymmetric_Search">
    /// UTS #10 Unicode Collation Algorithm</a>, while <see cref="AnyBaseWeightIsWildcard"/>
    /// supports a related option in which "unmarked" characters in either the
    /// pattern or the searched text are treated as wildcards that match marked or
    /// unmarked versions of the same character.
    /// </summary>
    /// <seealso cref="SearchIterator.ElementComparisonType"/>
    /// <stable>ICU 53</stable>
    public enum ElementComparisonType
    {
        /// <summary>
        /// Standard collation element comparison at the specified collator strength.
        /// </summary>
        /// <stable>ICU 53</stable>
        StandardElementComparison,

        /// <summary>
        /// Collation element comparison is modified to effectively provide behavior
        /// between the specified strength and strength - 1.
        /// <para/>
        /// Collation elements in the pattern that have the base weight for the specified
        /// strength are treated as "wildcards" that match an element with any other
        /// weight at that collation level in the searched text. For example, with a
        /// secondary-strength English collator, a plain 'e' in the pattern will match
        /// a plain e or an e with any diacritic in the searched text, but an e with
        /// diacritic in the pattern will only match an e with the same diacritic in
        /// the searched text.
        /// </summary>
        /// <stable>ICU 53</stable>
        PatternBaseWeightIsWildcard,

        /// <summary>
        /// Collation element comparison is modified to effectively provide behavior
        /// between the specified strength and strength - 1.
        /// <para/>
        /// Collation elements in either the pattern or the searched text that have the
        /// base weight for the specified strength are treated as "wildcards" that match
        /// an element with any other weight at that collation level. For example, with
        /// a secondary-strength English collator, a plain 'e' in the pattern will match
        /// a plain e or an e with any diacritic in the searched text, but an e with
        /// diacritic in the pattern will only match an e with the same diacritic or a
        /// plain e in the searched text.
        /// </summary>
        /// <stable>ICU 53</stable>
        AnyBaseWeightIsWildcard
    }
}
