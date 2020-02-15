using ICU4N.Support.Text;
using ICU4N.Util;
using J2N;
using J2N.Numerics;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace ICU4N.Text
{
    // C# porting note:
    //
    //        The ICU4C implementation contains dead code in many places.
    //      While porting the ICU4C linear search implementation, this dead code
    //      was not fully ported. The code blocks tagged by "// *** Boyer-Moore ***"
    //      are those dead code blocks, still available in ICU4C.

    //        The ICU4C implementation does not seem to handle UCharacterIterator pointing
    //      to a fragment of text properly. ICU4N uses CharacterIterator to navigate through
    //      the input text. We need to carefully review the code ported from ICU4C
    //      assuming the start index is 0.

    //        ICU4C implementation initializes pattern.CE and pattern.PCE. It looks like
    //      CE is no longer used, except in a few places checking CELength. It looks like this
    //      is a leftover from already-disabled Boyer-Moore search code. This C# implementation
    //      preserves the code, but we should clean this up later.

    /// <summary>
    /// <see cref="StringSearch"/> is a <see cref="SearchIterator"/> that provides
    /// language-sensitive text searching based on the comparison rules defined
    /// in a <see cref="RuleBasedCollator"/> object.
    /// <see cref="StringSearch"/> ensures that language eccentricity can be
    /// handled, e.g. for the German collator, characters &#223; and SS will be matched
    /// if case is chosen to be ignored.
    /// See the <a href="http://source.icu-project.org/repos/icu/icuhtml/trunk/design/collation/ICU_collation_design.htm">
    /// "ICU Collation Design Document"</a> for more information.
    /// <para/>
    /// There are 2 match options for selection:<br/>
    /// Let S' be the sub-string of a text string S between the offsets start and
    /// end [start, end].
    /// <br/>
    /// A pattern string P matches a text string S at the offsets [start, end]
    /// if
    /// <list type="table">
    ///     <item>
    ///         <term>option 1</term>
    ///         <term>Some canonical equivalent of P matches some canonical equivalent
    ///         of S'</term>
    ///     </item>
    ///     <item>
    ///         <term>option 2</term>
    ///         <term>P matches S' and if P starts or ends with a combining mark,
    ///         there exists no non-ignorable combining mark before or after S?
    ///         in S respectively.</term>
    ///     </item>
    /// </list>
    /// Option 2. is the default.
    /// <para/>
    /// This search has APIs similar to that of other text iteration mechanisms
    /// such as the break iterators in <see cref="BreakIterator"/>. Using these
    /// APIs, it is easy to scan through text looking for all occurrences of
    /// a given pattern. This search iterator allows changing of direction by
    /// calling a <see cref="Reset()"/> followed by a <see cref="SearchIterator.Next()"/> or <see cref="SearchIterator.Previous()"/>.
    /// Though a direction change can occur without calling <see cref="Reset()"/> first,
    /// this operation comes with some speed penalty.
    /// Match results in the forward direction will match the result matches in
    /// the backwards direction in the reverse order.
    /// <para/>
    /// <see cref="SearchIterator"/> provides APIs to specify the starting position
    /// within the text string to be searched, e.g. <see cref="SearchIterator.SetIndex(int)"/>,
    /// <see cref="SearchIterator.Preceding(int)"/> and <see cref="SearchIterator.Following(int)"/>.
    /// Since the starting position will be set as it is specified, please take note that
    /// there are some danger points at which the search may render incorrect
    /// results:
    /// <list type="bullet">
    ///     <item>
    ///         <description>
    ///             In the midst of a substring that requires normalization.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             If the following match is to be found, the position should not be the
    ///             second character which requires swapping with the preceding
    ///             character. Vice versa, if the preceding match is to be found, the
    ///             position to search from should not be the first character which
    ///             requires swapping with the next character. E.g certain Thai and
    ///             Lao characters require swapping.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             If a following pattern match is to be found, any position within a
    ///             contracting sequence except the first will fail. Vice versa if a
    ///             preceding pattern match is to be found, an invalid starting point
    ///             would be any character within a contracting sequence except the last.
    ///         </description>
    ///     </item>
    /// </list>
    /// <para/>
    /// A <see cref="BreakIterator"/> can be used if only matches at logical breaks are desired.
    /// Using a <see cref="BreakIterator"/> will only give you results that exactly matches the
    /// boundaries given by the <see cref="BreakIterator"/>. For instance the pattern "e" will
    /// not be found in the string "\u00e9" if a character break iterator is used.
    /// <para/>
    /// Options are provided to handle overlapping matches.
    /// E.g. In English, overlapping matches produces the result 0 and 2
    /// for the pattern "abab" in the text "ababab", where mutually
    /// exclusive matches only produces the result of 0.
    /// <para/>
    /// Options are also provided to implement "asymmetric search" as described in
    /// <a href="http://www.unicode.org/reports/tr10/#Asymmetric_Search">
    /// UTS #10 Unicode Collation Algorithm</a>, specifically the <see cref="ElementComparisonType"/>
    /// values.
    /// <para/>
    /// Though collator attributes will be taken into consideration while
    /// performing matches, there are no APIs here for setting and getting the
    /// attributes. These attributes can be set by getting the collator
    /// from <see cref="Collator"/> and using the APIs in <see cref="RuleBasedCollator"/>.
    /// Lastly to update <see cref="StringSearch"/> to the new collator attributes,
    /// <see cref="Reset()"/> has to be called.
    /// <para/>
    /// Restriction: <br/>
    /// Currently there are no composite characters that consists of a
    /// character with combining class &gt; 0 before a character with combining
    /// class == 0. However, if such a character exists in the future,
    /// <see cref="StringSearch"/> does not guarantee the results for option 1.
    /// <para/>
    /// Consult the <see cref="SearchIterator"/> documentation for information on
    /// and examples of how to use instances of this class to implement text
    /// searching.
    /// <para/>
    /// Note, <see cref="StringSearch"/> is not inheritable.
    /// </summary>
    /// <seealso cref="SearchIterator"/>
    /// <seealso cref="RuleBasedCollator"/>
    /// <author>Laura Werner, synwee</author>
    /// <stable>ICU 2.0</stable>
    // internal notes: all methods do not guarantee the correct status of the
    // characteriterator. the caller has to maintain the original index position
    // if necessary. methods could change the index position as it deems fit
    public sealed class StringSearch : SearchIterator
    {
        private UPattern pattern_;
        private RuleBasedCollator collator_;

        // positions within the collation element iterator is used to determine
        // if we are at the start of the text.
        private CollationElementIterator textIter_;
        private CollationPCE textProcessedIter_;

        // utility collation element, used throughout program for temporary
        // iteration.
        private CollationElementIterator utilIter_;

        private Normalizer2 nfd_;

        private CollationStrength strength_;
        int ceMask_;
        int variableTop_;

        private bool toShift_;

        // *** Boyer-Moore ***
        // private char[] canonicalPrefixAccents_;
        // private char[] canonicalSuffixAccents_;

        /// <summary>
        /// Initializes the iterator to use the language-specific rules defined in
        /// the argument <paramref name="collator"/> to search for argument <paramref name="pattern"/> in the argument
        /// <paramref name="target"/> text. The argument <paramref name="breakiter"/> is used to define logical matches.
        /// <para/>
        /// See super class documentation for more details on the use of the target
        /// text and <see cref="BreakIterator"/>.
        /// </summary>
        /// <param name="pattern">Text to look for.</param>
        /// <param name="target">Target text to search for <paramref name="pattern"/>.</param>
        /// <param name="collator"><see cref="RuleBasedCollator"/> that defines the language rules.</param>
        /// <param name="breakiter">A <see cref="BreakIterator"/> that is used to determine the
        /// boundaries of a logical match. This argument can be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when argument <paramref name="target"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when argument <paramref name="target"/> is length of 0.</exception>
        /// <seealso cref="BreakIterator"/>
        /// <seealso cref="RuleBasedCollator"/>
        /// <stable>ICU 2.0</stable>
        public StringSearch(string pattern, CharacterIterator target, RuleBasedCollator collator,
                BreakIterator breakiter)
            : base(target, breakiter)
        {

            // This implementation is ported from ICU4C usearch_open()

            // string search does not really work when numeric collation is turned on
            if (collator.IsNumericCollation)
            {
                throw new NotSupportedException("Numeric collation is not supported by StringSearch");
            }

            collator_ = collator;
            strength_ = collator.Strength;
            ceMask_ = GetMask(strength_);
            toShift_ = collator.IsAlternateHandlingShifted;
#pragma warning disable 612, 618
            variableTop_ = collator.VariableTop;
#pragma warning restore 612, 618

            nfd_ = Normalizer2.GetNFDInstance();

            pattern_ = new UPattern(pattern);

            search_.MatchedLength = 0;
            search_.matchedIndex_ = Done;

            utilIter_ = null;
            textIter_ = new CollationElementIterator(target, collator);

            textProcessedIter_ = null;

            // This is done by super class constructor
            /*
            search_.isOverlap_ = false;
            search_.isCanonicalMatch_ = false;
            search_.elementComparisonType_ = ElementComparisonType.STANDARD_ELEMENT_COMPARISON;
            search_.isForwardSearching_ = true;
            search_.reset_ = true;
             */
            ULocale collLocale = collator.GetLocale(ULocale.VALID_LOCALE);
            search_.internalBreakIter_ = BreakIterator.GetCharacterInstance(collLocale == null ? ULocale.ROOT : collLocale);
            search_.internalBreakIter_.SetText((CharacterIterator)target.Clone());  // We need to create a clone

            Initialize();
        }

        /// <summary>
        /// Initializes the iterator to use the language-specific rules defined in
        /// the argument <paramref name="collator"/> to search for argument <paramref name="pattern"/> in the argument
        /// <paramref name="target"/> text. No <see cref="BreakIterator"/>s are set to test for logical matches.
        /// </summary>
        /// <param name="pattern">Text to look for.</param>
        /// <param name="target">Target text to search for <paramref name="pattern"/>.</param>
        /// <param name="collator"><see cref="RuleBasedCollator"/> that defines the language rules.</param>
        /// <exception cref="ArgumentNullException">Thrown when argument <paramref name="target"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when argument <paramref name="target"/> is length of 0.</exception>
        /// <seealso cref="RuleBasedCollator"/>
        /// <stable>ICU 2.0</stable>
        public StringSearch(string pattern, CharacterIterator target, RuleBasedCollator collator)
            : this(pattern, target, collator, null)
        {
        }

        /// <summary>
        /// Initializes the iterator to use the language-specific rules and
        /// break iterator rules defined in the argument <paramref name="locale"/> to search for
        /// argument <paramref name="pattern"/> in the argument target text.
        /// </summary>
        /// <param name="pattern">Text to look for.</param>
        /// <param name="target">Target text to search for <paramref name="pattern"/>.</param>
        /// <param name="locale">Locale to use for language and break iterator rules.</param>
        /// <exception cref="ArgumentNullException">Thrown when argument <paramref name="target"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when argument <paramref name="target"/> is length of 0.</exception>
        /// <exception cref="InvalidCastException">Thrown if the collator for the specfied <paramref name="locale"/> is not a <see cref="RuleBasedCollator"/>.</exception>
        /// <stable>ICU 2.0</stable>
        public StringSearch(string pattern, CharacterIterator target, CultureInfo locale)
            : this(pattern, target, ULocale.ForLocale(locale))
        {
        }

        /// <summary>
        /// Initializes the iterator to use the language-specific rules and
        /// break iterator rules defined in the argument <paramref name="locale"/> to search for
        /// argument <paramref name="pattern"/> in the argument <paramref name="target"/> text.
        /// <para/>
        /// See super class documentation for more details on the use of the target
        /// text and <see cref="BreakIterator"/>.
        /// </summary>
        /// <param name="pattern">Text to look for.</param>
        /// <param name="target">Target text to search for <paramref name="pattern"/>.</param>
        /// <param name="locale">Locale to use for language and break iterator rules.</param>
        /// <exception cref="ArgumentNullException">Thrown when argument <paramref name="target"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when argument <paramref name="target"/> is length of 0.</exception>
        /// <exception cref="InvalidCastException">Thrown if the collator for the specfied <paramref name="locale"/> is not a <see cref="RuleBasedCollator"/>.</exception>
        /// <seealso cref="BreakIterator"/>
        /// <seealso cref="RuleBasedCollator"/>
        /// <seealso cref="SearchIterator"/>
        /// <stable>ICU 3.2</stable>
        public StringSearch(string pattern, CharacterIterator target, ULocale locale)
            : this(pattern, target, (RuleBasedCollator)Text.Collator.GetInstance(locale), null)
        {
        }

        /// <summary>
        /// Initializes the iterator to use the language-specific rules and
        /// break iterator rules defined in the default locale to search for
        /// argument pattern in the argument target text.
        /// </summary>
        /// <param name="pattern">Text to look for.</param>
        /// <param name="target">Target text to search for <paramref name="pattern"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when argument <paramref name="target"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when argument <paramref name="target"/> is length of 0.</exception>
        /// <exception cref="InvalidCastException">Thrown if the collator for the default locale is not a <see cref="RuleBasedCollator"/>.</exception>
        /// <stable>ICU 2.0</stable>
        public StringSearch(string pattern, string target)
            : this(pattern, new StringCharacterIterator(target), (RuleBasedCollator)Text.Collator.GetInstance(), null)
        {
        }

        /// <summary>
        /// Gets the <see cref="RuleBasedCollator"/> used for the language rules.
        /// <para/>
        /// Since <see cref="StringSearch"/> depends on the returned <see cref="RuleBasedCollator"/>, any
        /// changes to the <see cref="RuleBasedCollator"/> result should follow with a call to
        /// either <see cref="Reset()"/> or <see cref="SetCollator(RuleBasedCollator)"/> to ensure the correct
        /// search behavior.
        /// </summary>
        /// <seealso cref="RuleBasedCollator"/>
        /// <seealso cref="SetCollator(RuleBasedCollator)"/>
        /// <stable>ICU 2.0</stable>
        public RuleBasedCollator Collator
        {
            get { return collator_; }
        }

        /// <summary>
        /// Sets the <see cref="RuleBasedCollator"/> to be used for language-specific searching.
        /// <para/>
        /// The iterator's position will not be changed by this method.
        /// </summary>
        /// <param name="collator">Collator to use for this <see cref="StringSearch"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="collator"/> is null.</exception>
        /// <seealso cref="Collator"/>
        /// <stable>ICU 2.0</stable>
        public void SetCollator(RuleBasedCollator collator)
        {
            collator_ = collator ?? throw new ArgumentNullException(nameof(collator), "Collator can not be null");
            ceMask_ = GetMask(collator_.Strength);

            ULocale collLocale = collator.GetLocale(ULocale.VALID_LOCALE);
            search_.internalBreakIter_ = BreakIterator.GetCharacterInstance(collLocale ?? ULocale.ROOT);
            search_.internalBreakIter_.SetText((CharacterIterator)search_.Text.Clone());  // We need to create a clone

            toShift_ = collator.IsAlternateHandlingShifted;
#pragma warning disable 612, 618
            variableTop_ = collator.VariableTop;
#pragma warning restore 612, 618
            textIter_ = new CollationElementIterator(pattern_.Text, collator);
            utilIter_ = new CollationElementIterator(pattern_.Text, collator);

            // initialize() _after_ setting the iterators for the new collator.
            Initialize();
        }

        /// <summary>
        /// Gets or sets the pattern for which <see cref="StringSearch"/> is searching for.
        /// The iterator's position will not be changed.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public string Pattern
        {
            get { return pattern_.Text; }
            set
            {
                if (value == null || value.Length <= 0)
                {
                    throw new ArgumentException(
                            "Pattern to search for can not be null or of length 0");
                }
                pattern_.Text = value;
                Initialize();
            }
        }

        /// <summary>
        /// Gets or sets whether canonical matches (option 1, as described in the
        /// <see cref="StringSearch"/> documentation) is set.
        /// <para/>
        /// The default setting for this property is <c>false</c>.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        //TODO: hoist this to SearchIterator
        public bool IsCanonical
        {
            get { return search_.isCanonicalMatch_; }
            set { search_.isCanonicalMatch_ = value; }
        }

        /// <summary>
        /// Set the target text to be searched. Text iteration will then begin at 
        /// the start of the text string. This method is useful if you want to 
        /// reuse an iterator to search within a different body of text.
        /// </summary>
        /// <param name="text">New text iterator to look for match.</param>
        /// <exception cref="ArgumentException">Thrown when text is null or has 0 length.</exception>
        /// <see cref="SearchIterator.Target"/>
        /// <stable>ICU 2.8</stable>
        public override void SetTarget(CharacterIterator text)
        {
            base.SetTarget(text);
            textIter_.SetText(text);
        }

        /// <summary>
        /// Gets the current index in the text being searched.
        /// If the iteration has gone past the end of the text
        /// (or past the beginning for a backwards search), <see cref="SearchIterator.Done"/>
        /// is returned.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        public override int Index
        {
            get
            {
                int result = textIter_.GetOffset();
                if (IsOutOfBounds(search_.BeginIndex, search_.EndIndex, result))
                {
                    return Done;
                }
                return result;
            }
        }

        /// <summary>
        /// Sets the position in the target text at which the next search will start.
        /// This method clears any previous match.
        /// </summary>
        /// <param name="position">Position from which to start the next search.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown if argument position is out of the target text range.</exception>
        /// <see cref="Index"/>
        /// <stable>ICU 2.8</stable>
        public override void SetIndex(int position)
        {
            // Java porting note: This method is equivalent to setOffset() in ICU4C.
            // ICU4C SearchIterator::setOffset() is a pure virtual method, while
            // ICU4J SearchIterator.setIndex() is not abstract method.

            base.SetIndex(position);
            textIter_.SetOffset(position);
        }

        /// <summary>
        /// Resets the iteration.
        /// Search will begin at the start of the text string if a forward
        /// iteration is initiated before a backwards iteration. Otherwise if a
        /// backwards iteration is initiated before a forwards iteration, the
        /// search will begin at the end of the text string.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        public override void Reset()
        {
            // reset is setting the attributes that are already in
            // string search, hence all attributes in the collator should
            // be retrieved without any problems

            bool sameCollAttribute = true;
            int ceMask;
            bool shift;
            int varTop;

            // **** hack to deal w/ how processed CEs encode quaternary ****
            CollationStrength newStrength = collator_.Strength;
            if ((strength_ < CollationStrength.Quaternary && newStrength >= CollationStrength.Quaternary)
                    || (strength_ >= CollationStrength.Quaternary && newStrength < CollationStrength.Quaternary))
            {
                sameCollAttribute = false;
            }

            strength_ = collator_.Strength;
            ceMask = GetMask(strength_);
            if (ceMask_ != ceMask)
            {
                ceMask_ = ceMask;
                sameCollAttribute = false;
            }

            shift = collator_.IsAlternateHandlingShifted;
            if (toShift_ != shift)
            {
                toShift_ = shift;
                sameCollAttribute = false;
            }

#pragma warning disable 612, 618
            varTop = collator_.VariableTop;
#pragma warning restore 612, 618
            if (variableTop_ != varTop)
            {
                variableTop_ = varTop;
                sameCollAttribute = false;
            }

            if (!sameCollAttribute)
            {
                Initialize();
            }

            textIter_.SetText(search_.Text);

            search_.MatchedLength=0;
            search_.matchedIndex_ = Done;
            search_.isOverlap_ = false;
            search_.isCanonicalMatch_ = false;
            search_.elementComparisonType_ = ElementComparisonType.StandardElementComparison;
            search_.isForwardSearching_ = true;
            search_.reset_ = true;
        }

        /// <summary>
        /// Abstract method which subclasses override to provide the mechanism
        /// for finding the next match in the target text. This allows different
        /// subclasses to provide different search algorithms.
        /// <para/>
        /// If a match is found, the implementation should return the index at
        /// which the match starts and should set
        /// <see cref="SearchIterator.MatchLength"/> with the number of characters 
        /// in the target text that make up the match. If no match is found, the 
        /// method should return <see cref="SearchIterator.Done"/>.
        /// </summary>
        /// <param name="position">The index in the target text at which the search 
        /// should start.</param>
        /// <returns>Index at which the match starts, else if match is not found
        /// <see cref="SearchIterator.Done"/> is returned.</returns>
        /// <seealso cref="SearchIterator.MatchLength"/>
        /// <stable>ICU 2.8</stable>
        protected override int HandleNext(int position)
        {
            if (pattern_.CELength == 0)
            {
                search_.matchedIndex_ = search_.matchedIndex_ == Done ?
                                        Index : search_.matchedIndex_ + 1;
                search_.MatchedLength=0;
                textIter_.SetOffset(search_.matchedIndex_);
                if (search_.matchedIndex_ == search_.EndIndex)
                {
                    search_.matchedIndex_ = Done;
                }
            }
            else
            {
                if (search_.MatchedLength <= 0)
                {
                    // the flipping direction issue has already been handled
                    // in next()
                    // for boundary check purposes. this will ensure that the
                    // next match will not preceed the current offset
                    // note search_.matchedIndex_ will always be set to something
                    // in the code
                    search_.matchedIndex_ = position - 1;
                }

                textIter_.SetOffset(position);

                // ICU4C comment:
                // if strsrch_->breakIter is always the same as m_breakiterator_
                // then we don't need to check the match boundaries here because
                // usearch_handleNextXXX will already have done it.
                if (search_.isCanonicalMatch_)
                {
                    // *could* actually use exact here 'cause no extra accents allowed...
                    HandleNextCanonical();
                }
                else
                {
                    HandleNextExact();
                }

                if (search_.matchedIndex_ == Done)
                {
                    textIter_.SetOffset(search_.EndIndex);
                }
                else
                {
                    textIter_.SetOffset(search_.matchedIndex_);
                }

                return search_.matchedIndex_;
            }

            return Done;
        }

        /// <summary>
        /// Abstract method which subclasses override to provide the mechanism for
        /// finding the previous match in the target text. This allows different
        /// subclasses to provide different search algorithms.
        /// <para/>
        /// If a match is found, the implementation should return the index at
        /// which the match starts and should set 
        /// <see cref="SearchIterator.MatchLength"/> with the number of characters 
        /// in the target text that make up the match. If no match is found, the 
        /// method should return <see cref="SearchIterator.Done"/>.
        /// </summary>
        /// <param name="position">The index in the target text at which the search
        /// should start.</param>
        /// <returns>Index at which the match starts, else if match is not found 
        /// <see cref="SearchIterator.Done"/> is returned.</returns>
        /// <seealso cref="SearchIterator.MatchLength"/>
        /// <stable>ICU 2.8</stable>
        protected override int HandlePrevious(int position)
        {
            if (pattern_.CELength == 0)
            {
                search_.matchedIndex_ =
                        search_.matchedIndex_ == Done ? Index : search_.matchedIndex_;
                if (search_.matchedIndex_ == search_.BeginIndex)
                {
#pragma warning disable 612, 618
                    SetMatchNotFound();
#pragma warning restore 612, 618
                }
                else
                {
                    search_.matchedIndex_--;
                    textIter_.SetOffset(search_.matchedIndex_);
                    search_.MatchedLength=0;
                }
            }
            else
            {
                textIter_.SetOffset(position);

                if (search_.isCanonicalMatch_)
                {
                    // *could* use exact match here since extra accents *not* allowed!
                    HandlePreviousCanonical();
                }
                else
                {
                    HandlePreviousExact();
                }
            }

            return search_.matchedIndex_;
        }

        // ------------------ Internal implementation code ---------------------------

        private const int INITIAL_ARRAY_SIZE_ = 256;

        // *** Boyer-Moore ***
        // private static final Normalizer2Impl nfcImpl_ = Norm2AllModes.getNFCInstance().impl;
        // private static final int LAST_BYTE_MASK_ = 0xff;
        // private static final int SECOND_LAST_BYTE_SHIFT_ = 8;

        private const int PRIMARYORDERMASK = unchecked((int)0xffff0000);
        private const int SECONDARYORDERMASK = 0x0000ff00;
        private const int TERTIARYORDERMASK = 0x000000ff;

        /// <summary>
        /// Getting the mask for collation strength
        /// </summary>
        /// <param name="strength">Collation strength.</param>
        /// <returns>Collation element mask.</returns>
        private static int GetMask(CollationStrength strength)
        {
            switch (strength)
            {
                case CollationStrength.Primary:
                    return PRIMARYORDERMASK;
                case CollationStrength.Secondary:
                    return SECONDARYORDERMASK | PRIMARYORDERMASK;
                default:
                    return TERTIARYORDERMASK | SECONDARYORDERMASK | PRIMARYORDERMASK;
            }
        }


        // *** Boyer-Moore ***
        /*
        private final char getFCD(String str, int offset) {
            char ch = str.charAt(offset);
            if (ch < 0x180) {
                return (char) nfcImpl_.getFCD16FromBelow180(ch);
            } else if (nfcImpl_.singleLeadMightHaveNonZeroFCD16(ch)) {
                if (!Character.isHighSurrogate(ch)) {
                    return (char) nfcImpl_.getFCD16FromNormData(ch);
                } else {
                    char c2;
                    if (++offset < str.length() && Character.isLowSurrogate(c2 = str.charAt(offset))) {
                        return (char) nfcImpl_.getFCD16FromNormData(Character.toCodePoint(ch, c2));
                    }
                }
            }
            return 0;
        }

        private final char getFCD(int c) {
            return (char)nfcImpl_.getFCD16(c);
        }
        */

        /// <summary>
        /// Getting the modified collation elements taking into account the collation
        /// attributes.
        /// </summary>
        /// <param name="sourcece"></param>
        /// <returns>The modified collation element.</returns>
        private int GetCE(int sourcece)
        {
            // note for tertiary we can't use the collator->tertiaryMask, that
            // is a preprocessed mask that takes into account case options. since
            // we are only concerned with exact matches, we don't need that.
            sourcece &= ceMask_;

            if (toShift_)
            {
                // alternate handling here, since only the 16 most significant digits
                // is only used, we can safely do a compare without masking
                // if the ce is a variable, we mask and get only the primary values
                // no shifting to quartenary is required since all primary values
                // less than variabletop will need to be masked off anyway.
                if (variableTop_ > sourcece)
                {
                    if (strength_ >= CollationStrength.Quaternary)
                    {
                        sourcece &= PRIMARYORDERMASK;
                    }
                    else
                    {
                        sourcece = CollationElementIterator.Ingorable;
                    }
                }
            }
            else if (strength_ >= CollationStrength.Quaternary && sourcece == CollationElementIterator.Ingorable)
            {
                sourcece = 0xFFFF;
            }

            return sourcece;
        }

        /// <summary>
        /// Direct port of ICU4C static int32_t * addTouint32_tArray(...) in usearch.cpp
        /// (except not taking destination buffer size and status param).
        /// This is used for appending a PCE to Pattern.PCE_ buffer. We probably should
        /// implement this in <see cref="UPattern"/> class.
        /// </summary>
        /// <param name="destination">Target array.</param>
        /// <param name="offset">Destination offset to add <paramref name="value"/>.</param>
        /// <param name="value">Value to be added.</param>
        /// <param name="increments">Incremental size expected.</param>
        /// <returns>New destination array, <paramref name="destination"/> if there was no new allocation.</returns>
        private static int[] AddToInt32Array(int[] destination, int offset, int value, int increments) // ICU4N specific: Renamed from AddToIntArray
        {
            int newlength = destination.Length;
            if (offset + 1 == newlength)
            {
                newlength += increments;
                int[] temp = new int[newlength];
                System.Array.Copy(destination, 0, temp, 0, offset);
                destination = temp;
            }
            destination[offset] = value;
            return destination;
        }

        /// <summary>
        /// Direct port of ICU4C static int64_t * addTouint64_tArray(...) in usearch.cpp.
        /// This is used for appending a PCE to Pattern.PCE_ buffer. We probably should
        /// implement this in <see cref="UPattern"/> class.
        /// </summary>
        /// <param name="destination">Target array.</param>
        /// <param name="offset">Destination offset to add <paramref name="value"/>.</param>
        /// <param name="destinationlength">Target array size.</param>
        /// <param name="value">Value to be added.</param>
        /// <param name="increments">Incremental size expected.</param>
        /// <returns>New destination array, <paramref name="destination"/> if there was no new allocation.</returns>
        private static long[] AddToInt64Array(long[] destination, int offset, int destinationlength,
                long value, int increments) // ICU4N specific - Renamed from AddToLongArray
        {
            int newlength = destinationlength;
            if (offset + 1 == newlength)
            {
                newlength += increments;
                long[] temp = new long[newlength];
                System.Array.Copy(destination, 0, temp, 0, offset);
                destination = temp;
            }
            destination[offset] = value;
            return destination;
        }

        /// <summary>
        /// Initializing the ce table for a pattern.
        /// Stores non-ignorable collation keys.
        /// Table size will be estimated by the size of the pattern text. Table
        /// expansion will be perform as we go along. Adding 1 to ensure that the table
        /// size definitely increases.
        /// </summary>
        /// <returns>Total number of expansions.</returns>
        // TODO: We probably do not need Pattern CE table.
        private int InitializePatternCETable()
        {
            int[] cetable = new int[INITIAL_ARRAY_SIZE_];
            int patternlength = pattern_.Text.Length;
            CollationElementIterator coleiter = utilIter_;

            if (coleiter == null)
            {
                coleiter = new CollationElementIterator(pattern_.Text, collator_);
                utilIter_ = coleiter;
            }
            else
            {
                coleiter.SetText(pattern_.Text);
            }

            int offset = 0;
            int result = 0;
            int ce;

            while ((ce = coleiter.Next()) != CollationElementIterator.NullOrder)
            {
                int newce = GetCE(ce);
                if (newce != CollationElementIterator.Ingorable /* 0 */)
                {
                    int[] temp = AddToInt32Array(cetable, offset, newce,
                            patternlength - coleiter.GetOffset() + 1);
                    offset++;
                    cetable = temp;
                }
                result += (coleiter.GetMaxExpansion(ce) - 1);
            }

            cetable[offset] = 0;
            pattern_.CE_ = cetable;
            pattern_.CELength = offset;

            return result;
        }

        /// <summary>
        /// Initializing the pce table for a pattern.
        /// Stores non-ignorable collation keys.
        /// Table size will be estimated by the size of the pattern text. Table
        /// expansion will be perform as we go along. Adding 1 to ensure that the table
        /// size definitely increases.
        /// </summary>
        /// <returns>Total number of expansions.</returns>
        private int InitializePatternPCETable()
        {
            long[] pcetable = new long[INITIAL_ARRAY_SIZE_];
            int pcetablesize = pcetable.Length;
            int patternlength = pattern_.Text.Length;
            CollationElementIterator coleiter = utilIter_;

            if (coleiter == null)
            {
                coleiter = new CollationElementIterator(pattern_.Text, collator_);
                utilIter_ = coleiter;
            }
            else
            {
                coleiter.SetText(pattern_.Text);
            }

            int offset = 0;
            int result = 0;
            long pce;

            CollationPCE iter = new CollationPCE(coleiter);

            // ** Should processed CEs be signed or unsigned?
            // ** (the rest of the code in this file seems to play fast-and-loose with
            // ** whether a CE is signed or unsigned. For example, look at routine above this one.)
            while ((pce = iter.NextProcessed(null)) != CollationPCE.PROCESSED_NULLORDER)
            {
                long[] temp = AddToInt64Array(pcetable, offset, pcetablesize, pce, patternlength - coleiter.GetOffset() + 1);
                offset++;
                pcetable = temp;
            }

            pcetable[offset] = 0;
            pattern_.PCE_ = pcetable;
            pattern_.PCELength = offset;

            return result;
        }

        // TODO: This method only triggers initializePatternCETable(), which is probably no
        //      longer needed.
        private int InitializePattern()
        {
            // Since the strength is primary, accents are ignored in the pattern.

            // *** Boyer-Moore ***
            /*
            if (strength_ == Collator.PRIMARY) {
                pattern_.hasPrefixAccents_ = false;
                pattern_.hasSuffixAccents_ = false;
            } else {
                pattern_.hasPrefixAccents_ = (getFCD(pattern_.text_, 0) >>> SECOND_LAST_BYTE_SHIFT_) != 0;
                pattern_.hasSuffixAccents_ = (getFCD(pattern_.text_.codePointBefore(pattern_.text_.length())) & LAST_BYTE_MASK_) != 0;
            }
            */

            pattern_.PCE_ = null;

            // since intializePattern is an internal method status is a success.
            return InitializePatternCETable();
        }

        // *** Boyer-Moore ***
        /*
         private final void setShiftTable(char shift[],
                                             char backshift[],
                                             int cetable[], int cesize,
                                             int expansionsize,
                                             int defaultforward,
                                             int defaultbackward) {
             // No implementation
         }
         */

        // TODO: This method only triggers initializePattern(), which is probably no
        //      longer needed.
        private void Initialize()
        {
            /* int expandlength = */
            InitializePattern();

            // *** Boyer-Moore ***
            /*
            if (pattern_.CELength_ > 0) {
                int cesize = pattern_.CELength_;
                int minlength = cesize > expandlength ? cesize - expandlength : 1;
                pattern_.defaultShiftSize_ = minlength;
                setShiftTable(pattern_.shift_, pattern_.backShift_, pattern_.CE_, cesize,
                        expandlength, minlength, minlength);
                return;
            }
            return pattern_.defaultShiftSize_;
            */
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        internal override void SetMatchNotFound() // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
            base.SetMatchNotFound();
            // SearchIterator#setMatchNotFound() does following:
            //      search_.matchedIndex_ = DONE;
            //      search_.setMatchedLength(0);
            if (search_.isForwardSearching_)
            {
                textIter_.SetOffset(search_.Text.EndIndex);
            }
            else
            {
                textIter_.SetOffset(0);
            }
        }

        /// <summary>
        /// Checks if the offset runs out of the text string range.
        /// </summary>
        /// <param name="textstart">Offset of the first character in the range.</param>
        /// <param name="textlimit">Limit offset of the text string range.</param>
        /// <param name="offset">Offset to test.</param>
        /// <returns><c>true</c> if offset is out of bounds, <c>false</c> otherwise.</returns>
        private static bool IsOutOfBounds(int textstart, int textlimit, int offset)
        {
            return offset < textstart || offset > textlimit;
        }

        /// <summary>
        /// Checks for identical match.
        /// </summary>
        /// <param name="start">Start offset of possible match.</param>
        /// <param name="end">End offset of possible match.</param>
        /// <returns><c>true</c> if identical match is found.</returns>
        private bool CheckIdentical(int start, int end)
        {
            if (strength_ != CollationStrength.Identical)
            {
                return true;
            }
            // Note: We could use Normalizer::compare() or similar, but for short strings
            // which may not be in FCD it might be faster to just NFD them.
            string textstr = GetString(m_targetText, start, end - start);
#pragma warning disable 612, 618
            if (Normalizer.QuickCheck(textstr, NormalizerMode.NFD, 0) == QuickCheckResult.No)
            {
                textstr = Normalizer.Decompose(textstr, false);
            }
            string patternstr = pattern_.Text;
            if (Normalizer.QuickCheck(patternstr, NormalizerMode.NFD, 0) == QuickCheckResult.No)
            {
                patternstr = Normalizer.Decompose(patternstr, false);
            }
#pragma warning restore 612, 618
            return textstr.Equals(patternstr);
        }

        private bool InitTextProcessedIter()
        {
            if (textProcessedIter_ == null)
            {
                textProcessedIter_ = new CollationPCE(textIter_);
            }
            else
            {
                textProcessedIter_.Init(textIter_);
            }
            return true;
        }

        /// <summary>
        /// Find the next break boundary after <paramref name="startIndex"/>. If the UStringSearch object
        /// has an external break iterator, use that. Otherwise use the internal character
        /// break iterator.
        /// </summary>
        private int NextBoundaryAfter(int startIndex)
        {
            BreakIterator breakiterator = search_.BreakIterator;

            if (breakiterator == null)
            {
                breakiterator = search_.internalBreakIter_;
            }

            if (breakiterator != null)
            {
                return breakiterator.Following(startIndex);
            }

            return startIndex;
        }

        /// <summary>
        /// Returns <c>true</c> if <paramref name="index"/> is on a break boundary. If the UStringSearch
        /// has an external break iterator, test using that, otherwise test
        /// using the internal character break iterator.
        /// </summary>
        private bool IsBreakBoundary(int index)
        {
            BreakIterator breakiterator = search_.BreakIterator;

            if (breakiterator == null)
            {
                breakiterator = search_.internalBreakIter_;
            }

            return (breakiterator != null && breakiterator.IsBoundary(index));
        }


        // C# porting note: Followings are corresponding to UCompareCEsResult enum
        private const int CE_MATCH = -1;
        private const int CE_NO_MATCH = 0;
        private const int CE_SKIP_TARG = 1;
        private const int CE_SKIP_PATN = 2;

        private static int CE_LEVEL2_BASE = 0x00000005;
        private static int CE_LEVEL3_BASE = 0x00050000;

        private static int CompareCE64s(long targCE, long patCE, ElementComparisonType compareType)
        {
            if (targCE == patCE)
            {
                return CE_MATCH;
            }
            if (compareType == ElementComparisonType.StandardElementComparison)
            {
                return CE_NO_MATCH;
            }

            long targCEshifted = targCE.TripleShift(32);
            long patCEshifted = patCE.TripleShift(32);
            long mask;

            mask = 0xFFFF0000L;
            int targLev1 = (int)(targCEshifted & mask);
            int patLev1 = (int)(patCEshifted & mask);
            if (targLev1 != patLev1)
            {
                if (targLev1 == 0)
                {
                    return CE_SKIP_TARG;
                }
                if (patLev1 == 0
                        && compareType == ElementComparisonType.AnyBaseWeightIsWildcard)
                {
                    return CE_SKIP_PATN;
                }
                return CE_NO_MATCH;
            }

            mask = 0x0000FFFFL;
            int targLev2 = (int)(targCEshifted & mask);
            int patLev2 = (int)(patCEshifted & mask);
            if (targLev2 != patLev2)
            {
                if (targLev2 == 0)
                {
                    return CE_SKIP_TARG;
                }
                if (patLev2 == 0
                        && compareType == ElementComparisonType.AnyBaseWeightIsWildcard)
                {
                    return CE_SKIP_PATN;
                }
                return (patLev2 == CE_LEVEL2_BASE ||
                        (compareType == ElementComparisonType.AnyBaseWeightIsWildcard &&
                            targLev2 == CE_LEVEL2_BASE)) ? CE_MATCH : CE_NO_MATCH;
            }

            mask = 0xFFFF0000L;
            int targLev3 = (int)(targCE & mask);
            int patLev3 = (int)(patCE & mask);
            if (targLev3 != patLev3)
            {
                return (patLev3 == CE_LEVEL3_BASE ||
                        (compareType == ElementComparisonType.AnyBaseWeightIsWildcard &&
                            targLev3 == CE_LEVEL3_BASE)) ? CE_MATCH : CE_NO_MATCH;
            }

            return CE_MATCH;
        }

        /// <summary>
        /// An object used for receiving matched index in <see cref="Search(int, Match)"/> and
        /// <see cref="SearchBackwards(int, Match)"/>.
        /// </summary>
        private class Match
        {
            public int Start { get; set; } = -1;
            public int Limit { get; set; } = -1;
        }

        new private bool Search(int startIdx, Match m)
        {
            // Input parameter sanity check.
            if (pattern_.CELength == 0
                    || startIdx < search_.BeginIndex
                    || startIdx > search_.EndIndex)
            {
                throw new ArgumentException("search(" + startIdx + ", m) - expected position to be between " +
                        search_.BeginIndex + " and " + search_.EndIndex);
            }

            if (pattern_.PCE_ == null)
            {
                InitializePatternPCETable();
            }

            textIter_.SetOffset(startIdx);
            CEBuffer ceb = new CEBuffer(this);

            int targetIx = 0;
            CEI targetCEI = null;
            int patIx;
            bool found;

            int mStart = -1;
            int mLimit = -1;
            int minLimit;
            int maxLimit;

            // Outer loop moves over match starting positions in the
            //      target CE space.
            // Here we see the target as a sequence of collation elements, resulting from the following:
            // 1. Target characters were decomposed, and (if appropriate) other compressions and expansions are applied
            //    (for example, digraphs such as IJ may be broken into two characters).
            // 2. An int64_t CE weight is determined for each resulting unit (high 16 bits are primary strength, next
            //    16 bits are secondary, next 16 (the high 16 bits of the low 32-bit half) are tertiary. Any of these
            //    fields that are for strengths below that of the collator are set to 0. If this makes the int64_t
            //    CE weight 0 (as for a combining diacritic with secondary weight when the collator strentgh is primary),
            //    then the CE is deleted, so the following code sees only CEs that are relevant.
            // For each CE, the lowIndex and highIndex correspond to where this CE begins and ends in the original text.
            // If lowIndex==highIndex, either the CE resulted from an expansion/decomposition of one of the original text
            // characters, or the CE marks the limit of the target text (in which case the CE weight is UCOL_PROCESSED_NULLORDER).
            for (targetIx = 0; ; targetIx++)
            {
                found = true;
                // Inner loop checks for a match beginning at each
                // position from the outer loop.
                int targetIxOffset = 0;
                long patCE = 0;
                // For targetIx > 0, this ceb.get gets a CE that is as far back in the ring buffer
                // (compared to the last CE fetched for the previous targetIx value) as we need to go
                // for this targetIx value, so if it is non-NULL then other ceb.get calls should be OK.
                CEI firstCEI = ceb.Get(targetIx);
                if (firstCEI == null)
                {
                    throw new ICUException("CEBuffer.get(" + targetIx + ") returned null.");
                }

                for (patIx = 0; patIx < pattern_.PCELength; patIx++)
                {
                    patCE = pattern_.PCE_[patIx];
                    targetCEI = ceb.Get(targetIx + patIx + targetIxOffset);
                    // Compare CE from target string with CE from the pattern.
                    // Note that the target CE will be UCOL_PROCESSED_NULLORDER if we reach the end of input,
                    // which will fail the compare, below.
                    int ceMatch = CompareCE64s(targetCEI.CE, patCE, search_.elementComparisonType_);
                    if (ceMatch == CE_NO_MATCH)
                    {
                        found = false;
                        break;
                    }
                    else if (ceMatch > CE_NO_MATCH)
                    {
                        if (ceMatch == CE_SKIP_TARG)
                        {
                            // redo with same patCE, next targCE
                            patIx--;
                            targetIxOffset++;
                        }
                        else
                        { // ceMatch == CE_SKIP_PATN
                          // redo with same targCE, next patCE
                            targetIxOffset--;
                        }
                    }
                }
                targetIxOffset += pattern_.PCELength; // this is now the offset in target CE space to end of the match so far

                if (!found && ((targetCEI == null) || (targetCEI.CE != CollationPCE.PROCESSED_NULLORDER)))
                {
                    // No match at this targetIx.  Try again at the next.
                    continue;
                }

                if (!found)
                {
                    // No match at all, we have run off the end of the target text.
                    break;
                }

                // We have found a match in CE space.
                // Now determine the bounds in string index space.
                // There still is a chance of match failure if the CE range not correspond to
                // an acceptable character range.
                //
                CEI lastCEI = ceb.Get(targetIx + targetIxOffset - 1);

                mStart = firstCEI.LowIndex;
                minLimit = lastCEI.LowIndex;

                // Look at the CE following the match.  If it is UCOL_NULLORDER the match
                // extended to the end of input, and the match is good.

                // Look at the high and low indices of the CE following the match. If
                // they are the same it means one of two things:
                //    1. The match extended to the last CE from the target text, which is OK, or
                //    2. The last CE that was part of the match is in an expansion that extends
                //       to the first CE after the match. In this case, we reject the match.
                CEI nextCEI = null;
                if (search_.elementComparisonType_ == ElementComparisonType.StandardElementComparison)
                {
                    nextCEI = ceb.Get(targetIx + targetIxOffset);
                    maxLimit = nextCEI.LowIndex;
                    if (nextCEI.LowIndex == nextCEI.HighIndex && nextCEI.CE != CollationPCE.PROCESSED_NULLORDER)
                    {
                        found = false;
                    }
                }
                else
                {
                    for (; ; ++targetIxOffset)
                    {
                        nextCEI = ceb.Get(targetIx + targetIxOffset);
                        maxLimit = nextCEI.LowIndex;
                        // If we are at the end of the target too, match succeeds
                        if (nextCEI.CE == CollationPCE.PROCESSED_NULLORDER)
                        {
                            break;
                        }
                        // As long as the next CE has primary weight of 0,
                        // it is part of the last target element matched by the pattern;
                        // make sure it can be part of a match with the last patCE
                        if ((((nextCEI.CE).TripleShift(32)) & 0xFFFF0000L) == 0)
                        {
                            int ceMatch = CompareCE64s(nextCEI.CE, patCE, search_.elementComparisonType_);
                            if (ceMatch == CE_NO_MATCH || ceMatch == CE_SKIP_PATN)
                            {
                                found = false;
                                break;
                            }
                            // If lowIndex == highIndex, this target CE is part of an expansion of the last matched
                            // target element, but it has non-zero primary weight => match fails
                        }
                        else if (nextCEI.LowIndex == nextCEI.HighIndex)
                        {
                            found = false;
                            break;
                            // Else the target CE is not part of an expansion of the last matched element, match succeeds
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                // Check for the start of the match being within a combining sequence.
                // This can happen if the pattern itself begins with a combining char, and
                // the match found combining marks in the target text that were attached
                // to something else.
                // This type of match should be rejected for not completely consuming a
                // combining sequence.
                if (!IsBreakBoundary(mStart))
                {
                    found = false;
                }

                // Check for the start of the match being within an Collation Element Expansion,
                // meaning that the first char of the match is only partially matched.
                // With expansions, the first CE will report the index of the source
                // character, and all subsequent (expansions) CEs will report the source index of the
                // _following_ character.
                int secondIx = firstCEI.HighIndex;
                if (mStart == secondIx)
                {
                    found = false;
                }

                // Allow matches to end in the middle of a grapheme cluster if the following
                // conditions are met; this is needed to make prefix search work properly in
                // Indic, see #11750
                // * the default breakIter is being used
                // * the next collation element after this combining sequence
                //   - has non-zero primary weight
                //   - corresponds to a separate character following the one at end of the current match
                //   (the second of these conditions, and perhaps both, may be redundant given the
                //   subsequent check for normalization boundary; however they are likely much faster
                //   tests in any case)
                // * the match limit is a normalization boundary
                bool allowMidclusterMatch =
                                m_breakIterator == null &&
                                (((nextCEI.CE).TripleShift(32)) & 0xFFFF0000L) != 0 &&
                                maxLimit >= lastCEI.HighIndex && nextCEI.HighIndex > maxLimit &&
                                (nfd_.HasBoundaryBefore(CodePointAt(m_targetText, maxLimit)) ||
                                        nfd_.HasBoundaryAfter(CodePointBefore(m_targetText, maxLimit)));

                // If those conditions are met, then:
                // * do NOT advance the candidate match limit (mLimit) to a break boundary; however
                //   the match limit may be backed off to a previous break boundary. This handles
                //   cases in which mLimit includes target characters that are ignorable with current
                //   settings (such as space) and which extend beyond the pattern match.
                // * do NOT require that end of the combining sequence not extend beyond the match in CE space
                // * do NOT require that match limit be on a breakIter boundary

                // Advance the match end position to the first acceptable match boundary.
                // This advances the index over any combining characters.
                mLimit = maxLimit;
                if (minLimit < maxLimit)
                {
                    // When the last CE's low index is same with its high index, the CE is likely
                    // a part of expansion. In this case, the index is located just after the
                    // character corresponding to the CEs compared above. If the index is right
                    // at the break boundary, move the position to the next boundary will result
                    // incorrect match length when there are ignorable characters exist between
                    // the position and the next character produces CE(s). See ticket#8482.
                    if (minLimit == lastCEI.HighIndex && IsBreakBoundary(minLimit))
                    {
                        mLimit = minLimit;
                    }
                    else
                    {
                        int nba = NextBoundaryAfter(minLimit);
                        // Note that we can have nba < maxLimit && nba >= minLImit, in which
                        // case we want to set mLimit to nba regardless of allowMidclusterMatch
                        // (i.e. we back off mLimit to the previous breakIterator boundary).
                        if (nba >= lastCEI.HighIndex && (!allowMidclusterMatch || nba < maxLimit))
                        {
                            mLimit = nba;
                        }
                    }
                }

                if (!allowMidclusterMatch)
                {
                    // If advancing to the end of a combining sequence in character indexing space
                    // advanced us beyond the end of the match in CE space, reject this match.
                    if (mLimit > maxLimit)
                    {
                        found = false;
                    }

                    if (!IsBreakBoundary(mLimit))
                    {
                        found = false;
                    }
                }

                if (!CheckIdentical(mStart, mLimit))
                {
                    found = false;
                }

                if (found)
                {
                    break;
                }
            }

            // All Done.  Store back the match bounds to the caller.
            //
            if (found == false)
            {
                mLimit = -1;
                mStart = -1;
            }

            if (m != null)
            {
                m.Start = mStart;
                m.Limit = mLimit;
            }

            return found;
        }

        private static int CodePointAt(CharacterIterator iter, int index)
        {
            int currentIterIndex = iter.Index;
            char codeUnit = iter.SetIndex(index);
            int cp = codeUnit;
            if (char.IsHighSurrogate(codeUnit))
            {
                char nextUnit = iter.Next();
                if (char.IsLowSurrogate(nextUnit))
                {
                    cp = Character.ToCodePoint(codeUnit, nextUnit);
                }
            }
            iter.SetIndex(currentIterIndex);  // restore iter position
            return cp;
        }

        private static int CodePointBefore(CharacterIterator iter, int index)
        {
            int currentIterIndex = iter.Index;
            iter.SetIndex(index);
            char codeUnit = iter.Previous();
            int cp = codeUnit;
            if (char.IsLowSurrogate(codeUnit))
            {
                char prevUnit = iter.Previous();
                if (char.IsHighSurrogate(prevUnit))
                {
                    cp = Character.ToCodePoint(prevUnit, codeUnit);
                }
            }
            iter.SetIndex(currentIterIndex);  // restore iter position
            return cp;
        }

        private bool SearchBackwards(int startIdx, Match m)
        {
            //ICU4C_TODO comment:  reject search patterns beginning with a combining char.

            // Input parameter sanity check.
            if (pattern_.CELength == 0
                    || startIdx < search_.BeginIndex
                    || startIdx > search_.EndIndex)
            {
                throw new ArgumentException("searchBackwards(" + startIdx + ", m) - expected position to be between " +
                        search_.BeginIndex + " and " + search_.EndIndex);
            }

            if (pattern_.PCE_ == null)
            {
                InitializePatternPCETable();
            }

            CEBuffer ceb = new CEBuffer(this);
            int targetIx = 0;

            /*
             * Pre-load the buffer with the CE's for the grapheme
             * after our starting position so that we're sure that
             * we can look at the CE following the match when we
             * check the match boundaries.
             *
             * This will also pre-fetch the first CE that we'll
             * consider for the match.
             */
            if (startIdx < search_.EndIndex)
            {
                BreakIterator bi = search_.internalBreakIter_;
                int next = bi.Following(startIdx);

                textIter_.SetOffset(next);

                for (targetIx = 0; ; targetIx++)
                {
                    if (ceb.GetPrevious(targetIx).LowIndex < startIdx)
                    {
                        break;
                    }
                }
            }
            else
            {
                textIter_.SetOffset(startIdx);
            }

            CEI targetCEI = null;
            int patIx;
            bool found;

            int limitIx = targetIx;
            int mStart = -1;
            int mLimit = -1;
            int minLimit;
            int maxLimit;

            // Outer loop moves over match starting positions in the
            //      target CE space.
            // Here, targetIx values increase toward the beginning of the base text (i.e. we get the text CEs in reverse order).
            // But  patIx is 0 at the beginning of the pattern and increases toward the end.
            // So this loop performs a comparison starting with the end of pattern, and prcessd toward the beginning of the pattern
            // and the beginning of the base text.
            for (targetIx = limitIx; ; targetIx++)
            {
                found = true;
                // For targetIx > limitIx, this ceb.getPrevious gets a CE that is as far back in the ring buffer
                // (compared to the last CE fetched for the previous targetIx value) as we need to go
                // for this targetIx value, so if it is non-NULL then other ceb.getPrevious calls should be OK.
                CEI lastCEI = ceb.GetPrevious(targetIx);
                if (lastCEI == null)
                {
                    throw new ICUException("CEBuffer.getPrevious(" + targetIx + ") returned null.");
                }
                // Inner loop checks for a match beginning at each
                // position from the outer loop.
                int targetIxOffset = 0;
                for (patIx = pattern_.PCELength - 1; patIx >= 0; patIx--)
                {
                    long patCE = pattern_.PCE_[patIx];

                    targetCEI = ceb.GetPrevious(targetIx + pattern_.PCELength - 1 - patIx + targetIxOffset);
                    // Compare CE from target string with CE from the pattern.
                    // Note that the target CE will be UCOL_NULLORDER if we reach the end of input,
                    // which will fail the compare, below.
                    int ceMatch = CompareCE64s(targetCEI.CE, patCE, search_.elementComparisonType_);
                    if (ceMatch == CE_NO_MATCH)
                    {
                        found = false;
                        break;
                    }
                    else if (ceMatch > CE_NO_MATCH)
                    {
                        if (ceMatch == CE_SKIP_TARG)
                        {
                            // redo with same patCE, next targCE
                            patIx++;
                            targetIxOffset++;
                        }
                        else
                        { // ceMatch == CE_SKIP_PATN
                          // redo with same targCE, next patCE
                            targetIxOffset--;
                        }
                    }
                }

                if (!found && ((targetCEI == null) || (targetCEI.CE != CollationPCE.PROCESSED_NULLORDER)))
                {
                    // No match at this targetIx.  Try again at the next.
                    continue;
                }

                if (!found)
                {
                    // No match at all, we have run off the end of the target text.
                    break;
                }

                // We have found a match in CE space.
                // Now determine the bounds in string index space.
                // There still is a chance of match failure if the CE range not correspond to
                // an acceptable character range.
                //
                CEI firstCEI = ceb.GetPrevious(targetIx + pattern_.PCELength - 1 + targetIxOffset);
                mStart = firstCEI.LowIndex;

                // Check for the start of the match being within a combining sequence.
                // This can happen if the pattern itself begins with a combining char, and
                // the match found combining marks in the target text that were attached
                // to something else.
                // This type of match should be rejected for not completely consuming a
                // combining sequence.
                if (!IsBreakBoundary(mStart))
                {
                    found = false;
                }

                // Look at the high index of the first CE in the match. If it's the same as the
                // low index, the first CE in the match is in the middle of an expansion.
                if (mStart == firstCEI.HighIndex)
                {
                    found = false;
                }

                minLimit = lastCEI.LowIndex;

                if (targetIx > 0)
                {
                    // Look at the CE following the match.  If it is UCOL_NULLORDER the match
                    // extended to the end of input, and the match is good.

                    // Look at the high and low indices of the CE following the match. If
                    // they are the same it means one of two things:
                    //    1. The match extended to the last CE from the target text, which is OK, or
                    //    2. The last CE that was part of the match is in an expansion that extends
                    //       to the first CE after the match. In this case, we reject the match.
                    CEI nextCEI = ceb.GetPrevious(targetIx - 1);

                    if (nextCEI.LowIndex == nextCEI.HighIndex && nextCEI.CE != CollationPCE.PROCESSED_NULLORDER)
                    {
                        found = false;
                    }

                    mLimit = maxLimit = nextCEI.LowIndex;

                    // Allow matches to end in the middle of a grapheme cluster if the following
                    // conditions are met; this is needed to make prefix search work properly in
                    // Indic, see #11750
                    // * the default breakIter is being used
                    // * the next collation element after this combining sequence
                    //   - has non-zero primary weight
                    //   - corresponds to a separate character following the one at end of the current match
                    //   (the second of these conditions, and perhaps both, may be redundant given the
                    //   subsequent check for normalization boundary; however they are likely much faster
                    //   tests in any case)
                    // * the match limit is a normalization boundary
                    bool allowMidclusterMatch =
                                    m_breakIterator == null &&
                                    (((nextCEI.CE).TripleShift(32)) & 0xFFFF0000L) != 0 &&
                                    maxLimit >= lastCEI.HighIndex && nextCEI.HighIndex > maxLimit &&
                                    (nfd_.HasBoundaryBefore(CodePointAt(m_targetText, maxLimit)) ||
                                            nfd_.HasBoundaryAfter(CodePointBefore(m_targetText, maxLimit)));

                    // If those conditions are met, then:
                    // * do NOT advance the candidate match limit (mLimit) to a break boundary; however
                    //   the match limit may be backed off to a previous break boundary. This handles
                    //   cases in which mLimit includes target characters that are ignorable with current
                    //   settings (such as space) and which extend beyond the pattern match.
                    // * do NOT require that end of the combining sequence not extend beyond the match in CE space
                    // * do NOT require that match limit be on a breakIter boundary

                    // Advance the match end position to the first acceptable match boundary.
                    // This advances the index over any combining charcters.
                    if (minLimit < maxLimit)
                    {
                        int nba = NextBoundaryAfter(minLimit);
                        // Note that we can have nba < maxLimit && nba >= minLImit, in which
                        // case we want to set mLimit to nba regardless of allowMidclusterMatch
                        // (i.e. we back off mLimit to the previous breakIterator boundary).
                        if (nba >= lastCEI.HighIndex && (!allowMidclusterMatch || nba < maxLimit))
                        {
                            mLimit = nba;
                        }
                    }

                    if (!allowMidclusterMatch)
                    {
                        // If advancing to the end of a combining sequence in character indexing space
                        // advanced us beyond the end of the match in CE space, reject this match.
                        if (mLimit > maxLimit)
                        {
                            found = false;
                        }

                        // Make sure the end of the match is on a break boundary
                        if (!IsBreakBoundary(mLimit))
                        {
                            found = false;
                        }
                    }

                }
                else
                {
                    // No non-ignorable CEs after this point.
                    // The maximum position is detected by boundary after
                    // the last non-ignorable CE. Combining sequence
                    // across the start index will be truncated.
                    int nba = NextBoundaryAfter(minLimit);
                    mLimit = maxLimit = (nba > 0) && (startIdx > nba) ? nba : startIdx;
                }

                if (!CheckIdentical(mStart, mLimit))
                {
                    found = false;
                }

                if (found)
                {
                    break;
                }
            }

            // All Done.  Store back the match bounds to the caller.
            //
            if (found == false)
            {
                mLimit = -1;
                mStart = -1;
            }

            if (m != null)
            {
                m.Start = mStart;
                m.Limit = mLimit;
            }

            return found;
        }

        // C# porting note:
        //
        // ICU4C usearch_handleNextExact() is identical to usearch_handleNextCanonical()
        // for the linear search implementation. The differences are addressed in search().
        //
        private bool HandleNextExact()
        {
            return HandleNextCommonImpl();
        }

        private bool HandleNextCanonical()
        {
            return HandleNextCommonImpl();
        }

        private bool HandleNextCommonImpl()
        {
            int textOffset = textIter_.GetOffset();
            Match match = new Match();

            if (Search(textOffset, match))
            {
                search_.matchedIndex_ = match.Start;
                search_.MatchedLength = (match.Limit - match.Start);
                return true;
            }
            else
            {
#pragma warning disable 612, 618
                SetMatchNotFound();
#pragma warning restore 612, 618
                return false;
            }
        }

        // C# porting note:
        //
        // ICU4C usearch_handlePreviousExact() is identical to usearch_handlePreviousCanonical()
        // for the linear search implementation. The differences are addressed in searchBackwards().
        //
        private bool HandlePreviousExact()
        {
            return HandlePreviousCommonImpl();
        }

        private bool HandlePreviousCanonical()
        {
            return HandlePreviousCommonImpl();
        }

        private bool HandlePreviousCommonImpl()
        {
            int textOffset;

            if (search_.isOverlap_)
            {
                if (search_.matchedIndex_ != Done)
                {
                    textOffset = search_.matchedIndex_ + search_.MatchedLength - 1;
                }
                else
                {
                    // move the start position at the end of possible match
                    InitializePatternPCETable();
                    if (!InitTextProcessedIter())
                    {
#pragma warning disable 612, 618
                        SetMatchNotFound();
#pragma warning restore 612, 618
                        return false;
                    }
                    for (int nPCEs = 0; nPCEs < pattern_.PCELength - 1; nPCEs++)
                    {
                        long pce = textProcessedIter_.NextProcessed(null);
                        if (pce == CollationPCE.PROCESSED_NULLORDER)
                        {
                            // at the end of the text
                            break;
                        }
                    }
                    textOffset = textIter_.GetOffset();
                }
            }
            else
            {
                textOffset = textIter_.GetOffset();
            }

            Match match = new Match();
            if (SearchBackwards(textOffset, match))
            {
                search_.matchedIndex_ = match.Start;
                search_.MatchedLength=(match.Limit - match.Start);
                return true;
            }
            else
            {
#pragma warning disable 612, 618
                SetMatchNotFound();
#pragma warning restore 612, 618
                return false;
            }
        }

        /// <summary>
        /// Gets a substring out of a <see cref="CharacterIterator"/>.
        /// <para/>
        /// C# porting note: Not available in ICU4C
        /// </summary>
        /// <param name="text"><see cref="CharacterIterator"/>.</param>
        /// <param name="start">Start offset.</param>
        /// <param name="length">Length of substring.</param>
        /// <returns>Substring from <paramref name="text"/> starting at <paramref name="start"/> and <paramref name="length"/>.</returns>
        private static string GetString(CharacterIterator text, int start, int length)
        {
            StringBuilder result = new StringBuilder(length);
            int offset = text.Index;
            text.SetIndex(start);
            for (int i = 0; i < length; i++)
            {
                result.Append(text.Current);
                text.Next();
            }
            text.SetIndex(offset);
            return result.ToString();
        }

        /// <summary>
        /// C# port of ICU4C struct UPattern (usrchimp.h)
        /// </summary>
        private sealed class UPattern
        {
            /** Pattern string */
            public string Text { get; set; }

            internal long[] PCE_;
            public int PCELength { get; set; } = 0;

            // TODO: We probably do not need CE_ / CELength_
            internal int[] CE_;
            public int CELength { get; set; } = 0;

            // *** Boyer-Moore ***
            // bool hasPrefixAccents_ = false;
            // bool hasSuffixAccents_ = false;
            // int defaultShiftSize_;
            // char[] shift_;
            // char[] backShift_;

            public UPattern(string pattern)
            {
                Text = pattern;
            }
        }

        /// <summary>
        /// C# port of ICU4C UCollationPCE (usrchimp.h)
        /// </summary>
        private class CollationPCE
        {
            public const long PROCESSED_NULLORDER = -1;

            private const int DEFAULT_BUFFER_SIZE = 16;
            private const int BUFFER_GROW = 8;

            // Note: PRIMARYORDERMASK is also duplicated in StringSearch class
            private const uint PRIMARYORDERMASK = 0xffff0000;
            private const int CONTINUATION_MARKER = 0xc0;

            private PCEBuffer pceBuffer_ = new PCEBuffer();
            private CollationElementIterator cei_;
            private CollationStrength strength_;
            private bool toShift_;
            private bool isShifted_;
            private int variableTop_;

            public CollationPCE(CollationElementIterator iter)
            {
                Init(iter);
            }

            public void Init(CollationElementIterator iter)
            {
                cei_ = iter;
#pragma warning disable 612, 618
                Init(iter.RuleBasedCollator);
#pragma warning restore 612, 618
            }

            private void Init(RuleBasedCollator coll)
            {
                strength_ = coll.Strength;
                toShift_ = coll.IsAlternateHandlingShifted;
                isShifted_ = false;
#pragma warning disable 612, 618
                variableTop_ = coll.VariableTop;
#pragma warning restore 612, 618
            }

            private long ProcessCE(int ce)
            {
                long primary = 0, secondary = 0, tertiary = 0, quaternary = 0;

                // This is clean, but somewhat slow...
                // We could apply the mask to ce and then
                // just get all three orders...
                switch (strength_)
                {
                    default:
                        tertiary = CollationElementIterator.TertiaryOrder(ce);
                        // ICU4N: C# doesn't allow fall-through, so faking it by adding the methods
                        secondary = CollationElementIterator.SecondaryOrder(ce);
                        primary = CollationElementIterator.PrimaryOrder(ce);
                        break;
                    ////* note fall-through */

                    case CollationStrength.Secondary:
                        secondary = CollationElementIterator.SecondaryOrder(ce);
                        // ICU4N: C# doesn't allow fall-through, so faking it by adding the methods
                        primary = CollationElementIterator.PrimaryOrder(ce);
                        break;
                    ////* note fall-through */

                    case CollationStrength.Primary:
                        primary = CollationElementIterator.PrimaryOrder(ce);
                        break;
                }

                // **** This should probably handle continuations too. ****
                // **** That means that we need 24 bits for the primary ****
                // **** instead of the 16 that we're currently using. ****
                // **** So we can lay out the 64 bits as: 24.12.12.16. ****
                // **** Another complication with continuations is that ****
                // **** the *second* CE is marked as a continuation, so ****
                // **** we always have to peek ahead to know how long ****
                // **** the primary is... ****
                if ((toShift_ && variableTop_ > ce && primary != 0) || (isShifted_ && primary == 0))
                {

                    if (primary == 0)
                    {
                        return CollationElementIterator.Ingorable;
                    }

                    if (strength_ >= CollationStrength.Quaternary)
                    {
                        quaternary = primary;
                    }

                    primary = secondary = tertiary = 0;
                    isShifted_ = true;
                }
                else
                {
                    if (strength_ >= CollationStrength.Quaternary)
                    {
                        quaternary = 0xFFFF;
                    }

                    isShifted_ = false;
                }

                return primary << 48 | secondary << 32 | tertiary << 16 | quaternary;
            }

            /// <summary>
            /// Get the processed ordering priority of the next collation element in the text.
            /// A single character may contain more than one collation element.
            /// <para/>
            /// Note: This is equivalent to
            /// UCollationPCE::nextProcessed(int32_t *ixLow, int32_t *ixHigh, UErrorCode *status);
            /// </summary>
            /// <param name="range">Range receiving the iterator index before/after fetching the CE.</param>
            /// <returns>The next collation elements ordering, otherwise returns <see cref="PROCESSED_NULLORDER"/>
            /// if an error has occurred or if the end of string has been reached.</returns>
            public long NextProcessed(Range range)
            {
                long result = CollationElementIterator.Ingorable;
                int low = 0, high = 0;

                pceBuffer_.Reset();

                do
                {
                    low = cei_.GetOffset();
                    int ce = cei_.Next();
                    high = cei_.GetOffset();

                    if (ce == CollationElementIterator.NullOrder)
                    {
                        result = PROCESSED_NULLORDER;
                        break;
                    }

                    result = ProcessCE(ce);
                } while (result == CollationElementIterator.Ingorable);

                if (range != null)
                {
                    range.IxLow = low;
                    range.IxHigh = high;
                }

                return result;
            }

            /// <summary>
            /// Get the processed ordering priority of the previous collation element in the text.
            /// A single character may contain more than one collation element.
            /// <para/>
            /// Note: This is equivalent to
            /// UCollationPCE::previousProcessed(int32_t *ixLow, int32_t *ixHigh, UErrorCode *status);
            /// </summary>
            /// <param name="range">Range receiving the iterator index before/after fetching the CE.</param>
            /// <returns>The previous collation elements ordering, otherwise returns
            /// <see cref="PROCESSED_NULLORDER"/> if an error has occurred or if the start of
            /// string has been reached.</returns>
            public long PreviousProcessed(Range range)
            {
                long result = CollationElementIterator.Ingorable;
                int low = 0, high = 0;

                // pceBuffer_.reset();

                while (pceBuffer_.Empty())
                {
                    // buffer raw CEs up to non-ignorable primary
                    RCEBuffer rceb = new RCEBuffer();
                    int ce;

                    bool finish = false;

                    // **** do we need to reset rceb, or will it always be empty at this point ****
                    do
                    {
                        high = cei_.GetOffset();
                        ce = cei_.Previous();
                        low = cei_.GetOffset();

                        if (ce == CollationElementIterator.NullOrder)
                        {
                            if (!rceb.Empty())
                            {
                                break;
                            }

                            finish = true;
                            break;
                        }

                        rceb.Put(ce, low, high);
                    } while ((ce & PRIMARYORDERMASK) == 0 || IsContinuation(ce));

                    if (finish)
                    {
                        break;
                    }

                    // process the raw CEs
                    while (!rceb.Empty())
                    {
                        RCEI rcei = rceb.Get();

                        result = ProcessCE(rcei.CE);

                        if (result != CollationElementIterator.Ingorable)
                        {
                            pceBuffer_.Put(result, rcei.Low, rcei.High);
                        }
                    }
                }

                if (pceBuffer_.Empty())
                {
                    // **** Is -1 the right value for ixLow, ixHigh? ****
                    if (range != null)
                    {
                        range.IxLow = -1;
                        range.IxHigh = -1;
                    }
                    return CollationElementIterator.NullOrder;
                }

                PCEI pcei = pceBuffer_.Get();

                if (range != null)
                {
                    range.IxLow = pcei.Low;
                    range.IxHigh = pcei.High;
                }

                return pcei.CE;
            }

            private static bool IsContinuation(int ce)
            {
                return ((ce & CONTINUATION_MARKER) == CONTINUATION_MARKER);
            }

            public sealed class Range
            {
                public int IxLow { get; set; }
                public int IxHigh { get; set; }
            }

            /// <summary>Processed collation element buffer stuff ported from ICU4C ucoleitr.cpp</summary>
            private sealed class PCEI
            {
                public long CE { get; set; }
                public int Low { get; set; }
                public int High { get; set; }
            }

            private sealed class PCEBuffer
            {
                private PCEI[] buffer_ = new PCEI[DEFAULT_BUFFER_SIZE];
                private int bufferIndex_ = 0;

                public void Reset()
                {
                    bufferIndex_ = 0;
                }

                public bool Empty()
                {
                    return bufferIndex_ <= 0;
                }

                public void Put(long ce, int ixLow, int ixHigh)
                {
                    if (bufferIndex_ >= buffer_.Length)
                    {
                        PCEI[] newBuffer = new PCEI[buffer_.Length + BUFFER_GROW];
                        System.Array.Copy(buffer_, 0, newBuffer, 0, buffer_.Length);
                        buffer_ = newBuffer;
                    }
                    buffer_[bufferIndex_] = new PCEI();
                    buffer_[bufferIndex_].CE = ce;
                    buffer_[bufferIndex_].Low = ixLow;
                    buffer_[bufferIndex_].High = ixHigh;

                    bufferIndex_ += 1;
                }

                public PCEI Get()
                {
                    if (bufferIndex_ > 0)
                    {
                        return buffer_[--bufferIndex_];
                    }
                    return null;
                }
            }

            /// <summary>Raw collation element buffer stuff ported from ICU4C ucoleitr.cpp</summary>
            private sealed class RCEI
            {
                public int CE { get; set; }
                public int Low { get; set; }
                public int High { get; set; }
            }

            private sealed class RCEBuffer
            {
                private RCEI[] buffer_ = new RCEI[DEFAULT_BUFFER_SIZE];
                private int bufferIndex_ = 0;

                public bool Empty()
                {
                    return bufferIndex_ <= 0;
                }

                public void Put(int ce, int ixLow, int ixHigh)
                {
                    if (bufferIndex_ >= buffer_.Length)
                    {
                        RCEI[] newBuffer = new RCEI[buffer_.Length + BUFFER_GROW];
                        System.Array.Copy(buffer_, 0, newBuffer, 0, buffer_.Length);
                        buffer_ = newBuffer;
                    }
                    buffer_[bufferIndex_] = new RCEI();
                    buffer_[bufferIndex_].CE = ce;
                    buffer_[bufferIndex_].Low = ixLow;
                    buffer_[bufferIndex_].High = ixHigh;

                    bufferIndex_ += 1;
                }

                public RCEI Get()
                {
                    if (bufferIndex_ > 0)
                    {
                        return buffer_[--bufferIndex_];
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// C# port of ICU4C CEI (usearch.cpp)
        /// <para/>
        /// CEI  Collation Element + source text index.
        ///      These structs are kept in the circular buffer.
        /// </summary>
        private class CEI
        {
            public long CE { get; set; }
            public int LowIndex { get; set; }
            public int HighIndex { get; set; }
        }

        /// <summary>
        /// CEBuffer A circular buffer of CEs from the text being searched
        /// </summary>
        private class CEBuffer
        {
            // C# porting note: ICU4C uses the size for stack buffer
            // static final int DEFAULT_CEBUFFER_SIZE = 96;

            private const int CEBUFFER_EXTRA = 32;
            private const int MAX_TARGET_IGNORABLES_PER_PAT_JAMO_L = 8;
            private const int MAX_TARGET_IGNORABLES_PER_PAT_OTHER = 3;

            private CEI[] buf_;
            private int bufSize_;
            private int firstIx_;
            private int limitIx_;

            // C# porting note: No references in ICU4C implementation
            // CollationElementIterator ceIter_;

            private StringSearch strSearch_;

            internal CEBuffer(StringSearch ss)
            {
                strSearch_ = ss;
                bufSize_ = ss.pattern_.PCELength + CEBUFFER_EXTRA;
                if (ss.search_.elementComparisonType_ != ElementComparisonType.StandardElementComparison)
                {
                    string patText = ss.pattern_.Text;
                    if (patText != null)
                    {
                        for (int i = 0; i < patText.Length; i++)
                        {
                            char c = patText[i];
                            if (MIGHT_BE_JAMO_L(c))
                            {
                                bufSize_ += MAX_TARGET_IGNORABLES_PER_PAT_JAMO_L;
                            }
                            else
                            {
                                // No check for surrogates, we might allocate slightly more buffer than necessary.
                                bufSize_ += MAX_TARGET_IGNORABLES_PER_PAT_OTHER;
                            }
                        }
                    }
                }

                // Not used - see above
                // ceIter_ = ss.textIter_;

                firstIx_ = 0;
                limitIx_ = 0;

                if (!ss.InitTextProcessedIter())
                {
                    return;
                }

                buf_ = new CEI[bufSize_];
            }

            // Get the CE with the specified index.
            //   Index must be in the range
            //             n-history_size < index < n+1
            //   where n is the largest index to have been fetched by some previous call to this function.
            //   The CE value will be UCOL__PROCESSED_NULLORDER at end of input.
            //
            internal CEI Get(int index)
            {
                int i = index % bufSize_;

                if (index >= firstIx_ && index < limitIx_)
                {
                    // The request was for an entry already in our buffer.
                    // Just return it.
                    return buf_[i];
                }

                // Caller is requesting a new, never accessed before, CE.
                // Verify that it is the next one in sequence, which is all
                // that is allowed.
                if (index != limitIx_)
                {
                    Debug.Assert(false);
                    return null;
                }

                // Manage the circular CE buffer indexing
                limitIx_++;

                if (limitIx_ - firstIx_ >= bufSize_)
                {
                    // The buffer is full, knock out the lowest-indexed entry.
                    firstIx_++;
                }

                CollationPCE.Range range = new CollationPCE.Range();
                if (buf_[i] == null)
                {
                    buf_[i] = new CEI();
                }
                buf_[i].CE = strSearch_.textProcessedIter_.NextProcessed(range);
                buf_[i].LowIndex = range.IxLow;
                buf_[i].HighIndex = range.IxHigh;

                return buf_[i];
            }

            // Get the CE with the specified index.
            //   Index must be in the range
            //             n-history_size < index < n+1
            //   where n is the largest index to have been fetched by some previous call to this function.
            //   The CE value will be UCOL__PROCESSED_NULLORDER at end of input.
            //
            internal CEI GetPrevious(int index)
            {
                int i = index % bufSize_;

                if (index >= firstIx_ && index < limitIx_)
                {
                    // The request was for an entry already in our buffer.
                    // Just return it.
                    return buf_[i];
                }

                // Caller is requesting a new, never accessed before, CE.
                // Verify that it is the next one in sequence, which is all
                // that is allowed.
                if (index != limitIx_)
                {
                    Debug.Assert(false);
                    return null;
                }

                // Manage the circular CE buffer indexing
                limitIx_++;

                if (limitIx_ - firstIx_ >= bufSize_)
                {
                    // The buffer is full, knock out the lowest-indexed entry.
                    firstIx_++;
                }

                CollationPCE.Range range = new CollationPCE.Range();
                if (buf_[i] == null)
                {
                    buf_[i] = new CEI();
                }
                buf_[i].CE = strSearch_.textProcessedIter_.PreviousProcessed(range);
                buf_[i].LowIndex = range.IxLow;
                buf_[i].HighIndex = range.IxHigh;

                return buf_[i];
            }

            private static bool MIGHT_BE_JAMO_L(char c)
            {
                return (c >= 0x1100 && c <= 0x115E)
                        || (c >= 0x3131 && c <= 0x314E)
                        || (c >= 0x3165 && c <= 0x3186);
            }
        }
    }
}
