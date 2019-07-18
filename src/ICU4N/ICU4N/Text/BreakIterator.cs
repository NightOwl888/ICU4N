using ICU4N.Impl;
using ICU4N.Support.Text;
using ICU4N.Util;
using System;
using System.Globalization;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// A class that locates boundaries in text.  This class defines a protocol for
    /// objects that break up a piece of natural-language text according to a set
    /// of criteria.  Instances or subclasses of <see cref="BreakIterator"/> can be provided, for
    /// example, to break a piece of text into words, sentences, or logical characters
    /// according to the conventions of some language or group of languages.
    /// </summary>
    /// <remarks>
    /// We provide five built-in types of <see cref="BreakIterator"/>:
    /// <list type="table">
    ///     <item>
    ///         <term><see cref="GetTitleInstance()"/></term>
    ///         <description>Returns a <see cref="BreakIterator"/> that locates boundaries between title breaks.</description></item>
    ///     <item>
    ///         <term><see cref="GetSentenceInstance()"/></term>
    ///         <description>
    ///             Returns a <see cref="BreakIterator"/> that locates boundaries
    ///             between sentences.  This is useful for triple-click selection, for example.
    ///         </description></item>
    ///     <item>
    ///         <term><see cref="GetWordInstance()"/></term>
    ///         <description>
    ///             Returns a <see cref="BreakIterator"/> that locates boundaries between
    ///             words.  This is useful for double-click selection or "find whole words" searches.
    ///             This type of <see cref="BreakIterator"/> makes sure there is a boundary position at the
    ///             beginning and end of each legal word.  (Numbers count as words, too.)  Whitespace
    ///             and punctuation are kept separate from real words.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="GetLineInstance()"/></term>
    ///         <description>
    ///             Returns a <see cref="BreakIterator"/> that locates positions where it is
    ///             legal for a text editor to wrap lines.  This is similar to word breaking, but
    ///             not the same: punctuation and whitespace are generally kept with words (you don't
    ///             want a line to start with whitespace, for example), and some special characters
    ///             can force a position to be considered a line-break position or prevent a position
    ///             from being a line-break position.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="GetCharacterInstance()"/></term>
    ///         <description>
    ///             Returns a <see cref="BreakIterator"/> that locates boundaries between
    ///             logical characters.  Because of the structure of the Unicode encoding, a logical
    ///             character may be stored internally as more than one Unicode code point.  (A with an
    ///             umlaut may be stored as an a followed by a separate combining umlaut character,
    ///             for example, but the user still thinks of it as one character.)  This iterator allows
    ///             various processes (especially text editors) to treat as characters the units of text
    ///             that a user would think of as characters, rather than the units of text that the
    ///             computer sees as "characters".
    ///         </description>
    ///     </item>
    /// </list>
    /// The text boundary positions are found according to the rules
    /// described in Unicode Standard Annex #29, Text Boundaries, and
    /// Unicode Standard Annex #14, Line Breaking Properties.  These
    /// are available at <a href="http://www.unicode.org/reports/tr14/">http://www.unicode.org/reports/tr14/</a> and
    /// <a href="http://www.unicode.org/reports/tr29/">http://www.unicode.org/reports/tr29/</a>.
    /// <para/>
    /// BreakIterator's interface follows an "iterator" model (hence the name), meaning it
    /// has a concept of a "current position" and methods like <see cref="MoveFirst()"/>, <see cref="MoveLast()"/>, <see cref="MoveNext()"/>,
    /// and <see cref="MovePrevious()"/> that update the current position.  All <see cref="BreakIterator"/>s uphold the
    /// following invariants:
    /// <list type="bullet">
    ///     <item><description>
    ///         
    ///     </description></item>
    ///     <item><description>
    ///         The beginning and end of the text are always treated as boundary positions.
    ///     </description></item>
    ///     <item><description>
    ///         The current position of the iterator is always a boundary position (random-
    ///         access methods move the iterator to the nearest boundary position before or
    ///         after the specified position, not _to_ the specified position).
    ///     </description></item>
    ///     <item><description>
    ///         <see cref="DONE"/> is used as a flag to indicate when iteration has stopped.  <see cref="DONE"/> is only
    ///         returned when the current position is the end of the text and the user calls <see cref="MoveNext()"/>,
    ///         or when the current position is the beginning of the text and the user calls
    ///         <see cref="MovePrevious()"/>.
    ///     </description></item>
    ///     <item><description>
    ///         Break positions are numbered by the positions of the characters that follow
    ///         them.  Thus, under normal circumstances, the position before the first character
    ///         is 0, the position after the first character is 1, and the position after the
    ///         last character is 1 plus the length of the string.
    ///     </description></item>
    ///     <item><description>
    ///         The client can change the position of an iterator, or the text it analyzes,
    ///         at will, but cannot change the behavior.  If the user wants different behavior, he
    ///         must instantiate a new iterator.
    ///     </description></item>
    /// </list>
    /// <para/>
    /// <see cref="BreakIterator"/> accesses the text it analyzes through a <see cref="CharacterIterator"/>, which makes
    /// it possible to use <see cref="BreakIterator"/> to analyze text in any text-storage vehicle that
    /// provides a <see cref="CharacterIterator"/> interface.
    /// <para/>
    /// <b>Note:</b>  Some types of BreakIterator can take a long time to create, and
    /// instances of BreakIterator are not currently cached by the system.  For
    /// optimal performance, keep instances of BreakIterator around as long as makes
    /// sense.  For example, when word-wrapping a document, don't create and destroy a
    /// new <see cref="BreakIterator"/> for each line.  Create one break iterator for the whole document
    /// (or whatever stretch of text you're wrapping) and use it to do the whole job of
    /// wrapping the text.
    /// 
    /// <para/>
    /// Creating and using text boundaries
    /// <code>
    /// public static void Main(string args[])
    /// {
    ///     if (args.Length == 1)
    ///     {
    ///         string stringToExamine = args[0];
    ///         //print each word in order
    ///         BreakIterator boundary = BreakIterator.GetWordInstance();
    ///         boundary.SetText(stringToExamine);
    ///         PrintEachForward(boundary, stringToExamine);
    ///         //print each sentence in reverse order
    ///         boundary = BreakIterator.GetSentenceInstance(new CultureInfo("en-US"));
    ///         boundary.SetText(stringToExamine);
    ///         PrintEachBackward(boundary, stringToExamine);
    ///         PrintFirst(boundary, stringToExamine);
    ///         PrintLast(boundary, stringToExamine);
    ///     }
    /// }
    /// </code>
    /// Print each element in order
    /// <code>
    /// public static void PrintEachForward(BreakIterator boundary, string source)
    /// {
    ///     int start = boundary.MoveFirst();
    ///     for (int end = boundary.MoveNext();
    ///         end != BreakIterator.DONE;
    ///         start = end, end = boundary.MoveNext())
    ///     {
    ///         Console.WriteLine(source.Substring(start, end - start));
    ///     }
    /// }
    /// </code>
    /// Print each element in reverse order
    /// <code>
    /// public static void PrintEachBackward(BreakIterator boundary, string source)
    /// {
    ///     int end = boundary.MoveLast();
    ///     for (int start = boundary.MovePrevious();
    ///         start != BreakIterator.DONE;
    ///         end = start, start = boundary.MovePrevious())
    ///     {
    ///         Console.WriteLine(source.Substring(start, end - start));
    ///     }
    /// }
    /// </code>
    /// Print first element
    /// <code>
    /// public static void PrintFirst(BreakIterator boundary, string source)
    /// {
    ///     int start = boundary.MoveFirst();
    ///     int end = boundary.MoveNext();
    ///     Console.WriteLine(source.Substring(start, end - start));
    /// }
    /// </code>
    /// Print last element
    /// <code>
    /// public static void PrintLast(BreakIterator boundary, string source)
    /// {
    ///     int end = boundary.MoveLast();
    ///     int start = boundary.MovePrevious();
    ///     Console.WriteLine(source.Substring(start, end - start));
    /// }
    /// </code>
    /// Print the element at a specified position
    /// <code>
    /// public static void PrintAt(BreakIterator boundary, int pos, string source)
    /// {
    ///     int end = boundary.MoveFollowing(pos);
    ///     int start = boundary.MovePrevious();
    ///     Console.WriteLine(source.Substring(start, end - start));
    /// }
    /// </code>
    /// Find the next word
    /// <code>
    /// public static int NextWordStartAfter(int pos, string text)
    /// {
    ///     BreakIterator wb = BreakIterator.GetWordInstance();
    ///     wb.SetText(text);
    ///     int wordStart = wb.MoveFollowing(pos);
    ///     while (true)
    ///     {
    ///         int wordLimit = wb.MoveNext();
    ///         if (wordLimit == BreakIterator.DONE)
    ///         {
    ///             return BreakIterator.DONE;
    ///         }
    ///         int wordStatus = wb.RuleStatus;
    ///         if (wordStatus != BreakIterator.WORD_NONE)
    ///         {
    ///             return wordStart;
    ///         }
    ///         wordStart = wordLimit;
    ///     }
    /// }
    /// </code>
    /// <para/>
    /// The iterator returned by <see cref="GetWordInstance()"/> is unique in that
    /// the break positions it returns don't represent both the start and end of the
    /// thing being iterated over.  That is, a sentence-break iterator returns breaks
    /// that each represent the end of one sentence and the beginning of the next.
    /// With the word-break iterator, the characters between two boundaries might be a
    /// word, or they might be the punctuation or whitespace between two words.  The
    /// above code uses <see cref="RuleStatus"/> to identify and ignore boundaries associated
    /// with punctuation or other non-word characters.
    /// </remarks>
    /// <seealso cref="CharacterIterator"/>
    /// <stable>ICU 2.0</stable>
    public abstract class BreakIterator
#if FEATURE_CLONEABLE
        : ICloneable
#endif
    {
        private static readonly bool DEBUG = ICUDebug.Enabled("breakiterator");

        private static readonly object syncLock = new object();

        /// <summary>
        /// Default constructor.  There is no state that is carried by this abstract
        /// base class.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        protected BreakIterator()
        {
        }

        /// <summary>
        /// Clone method.  Creates another <see cref="BreakIterator"/> with the same behavior and
        /// current state as this one.
        /// </summary>
        /// <returns>The clone.</returns>
        /// <stable>ICU 2.0</stable>
        public virtual object Clone()
        {
            return base.MemberwiseClone();
        }

        /// <summary>
        /// DONE is returned by <see cref="MovePrevious()"/> and <see cref="MoveNext()"/> after all valid
        /// boundaries have been returned.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public const int DONE = -1; // ICU4N TODO: API - name according to .NET Conventions

        /// <summary>
        /// Set the iterator to the first boundary position.  This is always the beginning
        /// index of the text this iterator iterates over.  For example, if
        /// the iterator iterates over a whole string, this function will
        /// always return 0.
        /// </summary>
        /// <returns>The character offset of the beginning of the stretch of text
        /// being broken.</returns>
        /// <stable>ICU 2.0</stable>
        public abstract int MoveFirst(); // ICU4N specific - renamed from First()

        /// <summary>
        /// Set the iterator to the last boundary position.  This is always the "past-the-end"
        /// index of the text this iterator iterates over.  For example, if the
        /// iterator iterates over a whole string (call it "text"), this function
        /// will always return text.Length.
        /// </summary>
        /// <returns>The character offset of the end of the stretch of text
        /// being broken.</returns>
        /// <stable>ICU 2.0</stable>
        public abstract int MoveLast(); // ICU4N specific - renamed from Last()

        /// <summary>
        /// Move the iterator by the specified number of steps in the text.
        /// A positive number moves the iterator forward; a negative number
        /// moves the iterator backwards. If this causes the iterator
        /// to move off either end of the text, this function returns <see cref="DONE"/>;
        /// otherwise, this function returns the position of the appropriate
        /// boundary.  Calling this function is equivalent to calling <see cref="MoveNext()"/> or
        /// <see cref="MovePrevious()"/> <paramref name="n"/> times.
        /// </summary>
        /// <param name="n">The number of boundaries to advance over (if positive, moves
        /// forward; if negative, moves backwards).</param>
        /// <returns>The position of the boundary <paramref name="n"/> boundaries from the current
        /// iteration position, or <see cref="DONE"/> if moving <paramref name="n"/> boundaries causes the iterator
        /// to advance off either end of the text.</returns>
        /// <stable>ICU 2.0</stable>
        public abstract int Move(int n); // ICU4N specific - renamed from Next(int)

        /// <summary>
        /// Advances the iterator forward one boundary.  The current iteration
        /// position is updated to point to the next boundary position after the
        /// current position, and this is also the value that is returned.  If
        /// the current position is equal to the value returned by <see cref="MoveLast()"/>, or to
        /// <see cref="DONE"/>, this function returns <see cref="DONE"/> and sets the current position to
        /// <see cref="DONE"/>.
        /// </summary>
        /// <returns>The position of the first boundary position following the
        /// iteration position.</returns>
        /// <stable>ICU 2.0</stable>
        public abstract int MoveNext(); // ICU4N specific - renamed from Next()

        /// <summary>
        /// Move the iterator backward one boundary.  The current iteration
        /// position is updated to point to the last boundary position before
        /// the current position, and this is also the value that is returned.  If
        /// the current position is equal to the value returned by <see cref="MoveFirst()"/>, or to
        /// <see cref="DONE"/>, this function returns <see cref="DONE"/> and sets the current position to
        /// <see cref="DONE"/>.
        /// </summary>
        /// <returns>position of the last boundary position preceding the
        /// iteration position.</returns>
        /// <stable>ICU 2.0</stable>
        public abstract int MovePrevious(); // ICU4N specific - renamed from Previous()

        /// <summary>
        /// Sets the iterator's current iteration position to be the first
        /// boundary position following the specified position.  (Whether the
        /// specified position is itself a boundary position or not doesn't
        /// matter-- this function always moves the iteration position to the
        /// first boundary after the specified position.)  If the specified
        /// position is the past-the-end position, returns <see cref="DONE"/>.
        /// </summary>
        /// <param name="offset">The character position to start searching from.</param>
        /// <returns>The position of the first boundary position following
        /// "<paramref name="offset"/>" (whether or not "offset" itself is a boundary position),
        /// or <see cref="DONE"/> if "<paramref name="offset"/>" is the past-the-end offset.</returns>
        /// <stable>ICU 2.0</stable>
        public abstract int MoveFollowing(int offset); // ICU4N specific - renamed from Following(int)

        /// <summary>
        /// Sets the iterator's current iteration position to be the last
        /// boundary position preceding the specified position.  (Whether the
        /// specified position is itself a boundary position or not doesn't
        /// matter-- this function always moves the iteration position to the
        /// last boundary before the specified position.)  If the specified
        /// position is the starting position, returns <see cref="DONE"/>.
        /// </summary>
        /// <param name="offset">The character position to start searching from.</param>
        /// <returns>The position of the last boundary position preceding
        /// "<paramref name="offset"/>" (whether of not "offset" itself is a boundary position),
        /// or <see cref="DONE"/> if "<paramref name="offset"/>" is the starting offset of the iterator.</returns>
        /// <stable>ICU 2.0</stable>
        public virtual int MovePreceding(int offset) // ICU4N specific - renamed from Preceding(int)
        {
            // NOTE:  This implementation is here solely because we can't add new
            // abstract methods to an existing class.  There is almost ALWAYS a
            // better, faster way to do this.
            int pos = MoveFollowing(offset);
            while (pos >= offset && pos != DONE)
                pos = MovePrevious();
            return pos;
        }

        /// <summary>
        /// Return true if the specified position is a boundary position.  If the
        /// function returns true, the current iteration position is set to the
        /// specified position; if the function returns false, the current
        /// iteration position is set as though <see cref="MoveFollowing(int)"/> had been called.
        /// </summary>
        /// <param name="offset">The offset to check.</param>
        /// <returns>True if "<paramref name="offset"/>" is a boundary position.</returns>
        /// <stable>ICU 2.0</stable>
        public virtual bool IsBoundary(int offset)
        {
            // Again, this is the default implementation, which is provided solely because
            // we couldn't add a new abstract method to an existing class.  The real
            // implementations will usually need to do a little more work.
            if (offset == 0)
            {
                return true;
            }
            else
                return MoveFollowing(offset - 1) == offset;
        }

        /// <summary>
        /// Gets the iterator's current position.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public abstract int Current { get; }

        /// <summary>
        /// Tag value for "words" that do not fit into any of other categories.
        /// Includes spaces and most punctuation.
        /// </summary>
        /// <stable>ICU 53</stable>
        public const int WORD_NONE = 0;

        /// <summary>
        /// Upper bound for tags for uncategorized words.
        /// </summary>
        /// <stable>ICU 53</stable>
        public const int WORD_NONE_LIMIT = 100;

        /// <summary>
        /// Tag value for words that appear to be numbers, lower limit.
        /// </summary>
        /// <stable>ICU 53</stable>
        public const int WORD_NUMBER = 100;

        /// <summary>
        /// Tag value for words that appear to be numbers, upper limit.
        /// </summary>
        /// <stable>ICU 53</stable>
        public const int WORD_NUMBER_LIMIT = 200;

        /// <summary>
        /// Tag value for words that contain letters, excluding
        /// hiragana, katakana or ideographic characters, lower limit.
        /// </summary>
        /// <stable>ICU 53</stable>
        public const int WORD_LETTER = 200;

        /// <summary>
        /// Tag value for words containing letters, upper limit
        /// </summary>
        /// <stable>ICU 53</stable>
        public const int WORD_LETTER_LIMIT = 300;

        /// <summary>
        /// Tag value for words containing kana characters, lower limit
        /// </summary>
        /// <stable>ICU 53</stable>
        public const int WORD_KANA = 300;

        /// <summary>
        /// Tag value for words containing kana characters, upper limit
        /// </summary>
        /// <stable>ICU 53</stable>
        public const int WORD_KANA_LIMIT = 400;

        /// <summary>
        /// Tag value for words containing ideographic characters, lower limit
        /// </summary>
        /// <stable>ICU 53</stable>
        public const int WORD_IDEO = 400;

        /// <summary>
        /// Tag value for words containing ideographic characters, upper limit
        /// </summary>
        /// <stable>ICU 53</stable>
        public const int WORD_IDEO_LIMIT = 500;

        /// <summary>
        /// For <see cref="RuleBasedBreakIterator"/>s, return the status tag from the
        /// break rule that determined the most recently
        /// returned break position.
        /// <para/>
        /// For break iterator types that do not support a rule status,
        /// a default value of 0 is returned.
        /// </summary>
        /// <stable>ICU 52</stable>
        public virtual int RuleStatus
        {
            get { return 0; }
        }

        /// <summary>
        /// For <see cref="RuleBasedBreakIterator"/>s, get the status (tag) values from the break rule(s)
        /// that determined the most recently returned break position.
        /// <para/>
        /// For break iterator types that do not support rule status,
        /// no values are returned.
        /// <para/>
        /// If the size of the output array is insufficient to hold the data,
        /// the output will be truncated to the available length.  No exception
        /// will be thrown.
        /// </summary>
        /// <param name="fillInArray">An array to be filled in with the status values.</param>
        /// <returns>
        /// The number of rule status values from rules that determined
        /// the most recent boundary returned by the break iterator.
        /// In the event that the array is too small, the return value
        /// is the total number of status values that were available,
        /// not the reduced number that were actually returned.
        /// </returns>
        /// <stable>ICU 52</stable>
        public virtual int GetRuleStatusVec(int[] fillInArray)
        {
            if (fillInArray != null && fillInArray.Length > 0)
            {
                fillInArray[0] = 0;
            }
            return 1;
        }

        /// <summary>
        /// Gets a <see cref="CharacterIterator"/> over the text being analyzed.
        /// For at least some subclasses of <see cref="BreakIterator"/>, this is a reference
        /// to the <b>actual iterator being used</b> by the <see cref="BreakIterator"/>,
        /// and therefore, this function's return value should be treated as
        /// <c>const</c>.  No guarantees are made about the current position
        /// of this iterator when it is returned.  If you need to move that
        /// position to examine the text, clone this function's return value first.
        /// </summary>
        /// <returns>A <see cref="CharacterIterator"/> over the text being analyzed.</returns>
        /// <stable>ICU 2.0</stable>
        public abstract CharacterIterator Text { get; }

        /// <summary>
        /// Sets the iterator to analyze a new piece of text.  The new
        /// piece of text is passed in as a <see cref="string"/>, and the current
        /// iteration position is reset to the beginning of the string.
        /// (The old text is dropped.)
        /// </summary>
        /// <param name="newText">A <see cref="string"/> containing the text to analyze with
        /// this <see cref="BreakIterator"/>.</param>
        /// <stable>ICU 2.0</stable>
        public virtual void SetText(string newText)
        {
            SetText(new StringCharacterIterator(newText));
        }

        /// <summary>
        /// Sets the iterator to analyze a new piece of text.  The new
        /// piece of text is passed in as a <see cref="StringBuilder"/>, and the current
        /// iteration position is reset to the beginning of the string.
        /// (The old text is dropped.)
        /// </summary>
        /// <param name="newText">A <see cref="StringBuilder"/> containing the text to analyze with
        /// this <see cref="BreakIterator"/>.</param>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N specific
        public virtual void SetText(StringBuilder newText)
        {
            SetText(new StringCharacterIterator(newText.ToString())); // ICU4N TODO: make a StringBuilderCharacterIterator
        }

        /// <summary>
        /// Sets the iterator to analyze a new piece of text.  The new
        /// piece of text is passed in as a <see cref="T:char[]"/>, and the current
        /// iteration position is reset to the beginning of the string.
        /// (The old text is dropped.)
        /// </summary>
        /// <param name="newText">A <see cref="T:char[]"/> containing the text to analyze with
        /// this <see cref="BreakIterator"/>.</param>
        /// <draft>ICU4N 60.1</draft>
        // ICU4N specific
        public virtual void SetText(char[] newText)
        {
            SetText(new StringCharacterIterator(new string(newText))); // ICU4N TODO: make a CharArrayCharacterIterator
        }

        /// <summary>
        /// Sets the iterator to analyze a new piece of text.  The new
        /// piece of text is passed in as a <see cref="ICharSequence"/>, and the current
        /// iteration position is reset to the beginning of the text.
        /// (The old text is dropped.)
        /// </summary>
        /// <param name="newText">A <see cref="ICharSequence"/> containing the text to analyze with
        /// this <see cref="BreakIterator"/>.</param>
        /// <draft>ICU 60</draft>
        internal virtual void SetText(ICharSequence newText)
        {
            SetText(new CSCharacterIterator(newText));
        }

        /// <summary>
        /// Sets the iterator to analyze a new piece of text.  The
        /// <see cref="BreakIterator"/> is passed a <see cref="CharacterIterator"/> through which
        /// it will access the text itself.  The current iteration
        /// position is reset to the <see cref="CharacterIterator"/>'s start index.
        /// (The old iterator is dropped.)
        /// </summary>
        /// <param name="newText">A <see cref="CharacterIterator"/> referring to the text
        /// to analyze with this BreakIterator (the iterator's current
        /// position is ignored, but its other state is significant).</param>
        /// <stable>ICU 2.0</stable>
        public abstract void SetText(CharacterIterator newText);

        /// <icu/>
        /// <stable>ICU 2.4</stable>
        public const int KIND_CHARACTER = 0; // ICU4N TODO: API - change to enum ? rename to follow .NET Conventions...?

        /// <icu/>
        /// <stable>ICU 2.4</stable>
        public const int KIND_WORD = 1;

        /// <icu/>
        /// <stable>ICU 2.4</stable>
        public const int KIND_LINE = 2;

        /// <icu/>
        /// <stable>ICU 2.4</stable>
        public const int KIND_SENTENCE = 3;

        /// <icu/>
        /// <stable>ICU 2.4</stable>
        public const int KIND_TITLE = 4;

        /// <since>ICU 2.8</since>
        private const int KIND_COUNT = 5;

        private static readonly CacheValue<BreakIteratorCache>[] iterCache = new CacheValue<BreakIteratorCache>[5];

        /// <summary>
        /// Returns a new instance of <see cref="BreakIterator"/> that locates word boundaries.
        /// This function assumes that the text being analyzed is in the default
        /// locale's language.
        /// </summary>
        /// <returns>An instance of <see cref="BreakIterator"/> that locates word boundaries.</returns>
        /// <stable>ICU 2.0</stable>
        public static BreakIterator GetWordInstance()
        {
            return GetWordInstance(ULocale.GetDefault());
        }

        /// <summary>
        /// Returns a new instance of <see cref="BreakIterator"/> that locates word boundaries.
        /// </summary>
        /// <param name="where">A <see cref="CultureInfo"/> specifying the language of the text to be
        /// analyzed.</param>
        /// <returns>An instance of <see cref="BreakIterator"/> that locates word boundaries.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="where"/> is null.</exception>
        /// <stable>ICU 2.0</stable>
        public static BreakIterator GetWordInstance(CultureInfo where)
        {
            return GetBreakInstance(ULocale.ForLocale(where), KIND_WORD);
        }

        /// <icu/>
        /// <summary>
        /// Returns a new instance of <see cref="BreakIterator"/> that locates word boundaries.
        /// </summary>
        /// <param name="where">A <see cref="ULocale"/> specifying the language of the text to be
        /// analyzed.</param>
        /// <returns>An instance of <see cref="BreakIterator"/> that locates word boundaries.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="where"/> is null.</exception>
        /// <stable>ICU 3.2</stable>
        public static BreakIterator GetWordInstance(ULocale where)
        {
            return GetBreakInstance(where, KIND_WORD);
        }

        /// <summary>
        /// Returns a new instance of <see cref="BreakIterator"/> that locates legal line-
        /// wrapping positions.  This function assumes the text being broken
        /// is in the default locale's language.
        /// </summary>
        /// <returns>A new instance of <see cref="BreakIterator"/> that locates legal
        /// line-wrapping positions.</returns>
        /// <stable>ICU 2.0</stable>
        public static BreakIterator GetLineInstance()
        {
            return GetLineInstance(ULocale.GetDefault());
        }

        /// <summary>
        /// Returns a new instance of <see cref="BreakIterator"/> that locates legal line-
        /// wrapping positions.
        /// </summary>
        /// <param name="where">A <see cref="CultureInfo"/> specifying the language of the text being broken.</param>
        /// <returns>A new instance of <see cref="BreakIterator"/> that locates legal
        /// line-wrapping positions.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="where"/> is null.</exception>
        /// <stable>ICU 2.0</stable>
        public static BreakIterator GetLineInstance(CultureInfo where)
        {
            return GetBreakInstance(ULocale.ForLocale(where), KIND_LINE);
        }

        /// <icu/>
        /// <summary>
        /// Returns a new instance of <see cref="BreakIterator"/> that locates legal line-
        /// wrapping positions.
        /// </summary>
        /// <param name="where">A <see cref="ULocale"/> specifying the language of the text being broken.</param>
        /// <returns>A new instance of <see cref="BreakIterator"/> that locates legal
        /// line-wrapping positions.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="where"/> is null.</exception>
        /// <stable>ICU 3.2</stable>
        public static BreakIterator GetLineInstance(ULocale where)
        {
            return GetBreakInstance(where, KIND_LINE);
        }

        /// <summary>
        /// Returns a new instance of <see cref="BreakIterator"/> that locates logical-character
        /// boundaries.  This function assumes that the text being analyzed is
        /// in the default locale's language.
        /// </summary>
        /// <returns>A new instance of <see cref="BreakIterator"/> that locates logical-character
        /// boundaries.</returns>
        /// <stable>ICU 2.0</stable>
        public static BreakIterator GetCharacterInstance()
        {
            return GetCharacterInstance(ULocale.GetDefault());
        }

        /// <summary>
        /// Returns a new instance of <see cref="BreakIterator"/> that locates logical-character
        /// boundaries.
        /// </summary>
        /// <param name="where">A <see cref="CultureInfo"/> specifying the language of the text being analyzed.</param>
        /// <returns>A new instance of <see cref="BreakIterator"/> that locates logical-character
        /// boundaries.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="where"/> is null.</exception>
        /// <stable>ICU 2.0</stable>
        public static BreakIterator GetCharacterInstance(CultureInfo where)
        {
            return GetBreakInstance(ULocale.ForLocale(where), KIND_CHARACTER);
        }

        /// <icu/>
        /// <summary>
        /// Returns a new instance of <see cref="BreakIterator"/> that locates logical-character
        /// boundaries.
        /// </summary>
        /// <param name="where">A <see cref="ULocale"/> specifying the language of the text being analyzed.</param>
        /// <returns>A new instance of <see cref="BreakIterator"/> that locates logical-character
        /// boundaries.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="where"/> is null.</exception>
        /// <stable>ICU 3.2</stable>
        public static BreakIterator GetCharacterInstance(ULocale where)
        {
            return GetBreakInstance(where, KIND_CHARACTER);
        }

        /// <summary>
        /// Returns a new instance of BreakIterator that locates sentence boundaries.
        /// This function assumes the text being analyzed is in the default locale's
        /// language.
        /// </summary>
        /// <returns>A new instance of BreakIterator that locates sentence boundaries.</returns>
        /// <stable>ICU 2.0</stable>
        public static BreakIterator GetSentenceInstance()
        {
            return GetSentenceInstance(ULocale.GetDefault());
        }

        /// <summary>
        /// Returns a new instance of <see cref="BreakIterator"/> that locates sentence boundaries.
        /// </summary>
        /// <param name="where">A <see cref="CultureInfo"/> specifying the language of the text being analyzed.</param>
        /// <returns>A new instance of <see cref="BreakIterator"/> that locates sentence boundaries.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="where"/> is null.</exception>
        /// <stable>ICU 2.0</stable>
        public static BreakIterator GetSentenceInstance(CultureInfo where)
        {
            return GetBreakInstance(ULocale.ForLocale(where), KIND_SENTENCE);
        }

        /// <icu/>
        /// <summary>
        /// Returns a new instance of <see cref="BreakIterator"/> that locates sentence boundaries.
        /// </summary>
        /// <param name="where">A <see cref="ULocale"/> specifying the language of the text being analyzed.</param>
        /// <returns>A new instance of <see cref="BreakIterator"/> that locates sentence boundaries.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="where"/> is null.</exception>
        /// <stable>ICU 3.2</stable>
        public static BreakIterator GetSentenceInstance(ULocale where)
        {
            return GetBreakInstance(where, KIND_SENTENCE);
        }

        /// <icu/>
        /// <summary>
        /// Returns a new instance of <see cref="BreakIterator"/> that locates title boundaries.
        /// This function assumes the text being analyzed is in the default locale's
        /// language. The iterator returned locates title boundaries as described for
        /// Unicode 3.2 only. For Unicode 4.0 and above title boundary iteration,
        /// please use a word boundary iterator. <see cref="GetWordInstance()"/>
        /// </summary>
        /// <returns>A new instance of <see cref="BreakIterator"/> that locates title boundaries.</returns>
        /// <stable>ICU 2.0</stable>
        public static BreakIterator GetTitleInstance()
        {
            return GetTitleInstance(ULocale.GetDefault());
        }

        /// <icu/>
        /// <summary>
        /// Returns a new instance of <see cref="BreakIterator"/> that locates title boundaries.
        /// The iterator returned locates title boundaries as described for
        /// Unicode 3.2 only. For Unicode 4.0 and above title boundary iteration,
        /// please use Word Boundary iterator. <see cref="GetWordInstance(CultureInfo)"/>
        /// </summary>
        /// <param name="where">A <see cref="CultureInfo"/> specifying the language of the text being analyzed.</param>
        /// <returns>A new instance of <see cref="BreakIterator"/> that locates title boundaries.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="where"/> is null.</exception>
        /// <stable>ICU 2.0</stable>
        public static BreakIterator GetTitleInstance(CultureInfo where)
        {
            return GetBreakInstance(ULocale.ForLocale(where), KIND_TITLE);
        }

        /// <icu/>
        /// <summary>
        /// Returns a new instance of <see cref="BreakIterator"/> that locates title boundaries.
        /// The iterator returned locates title boundaries as described for
        /// Unicode 3.2 only. For Unicode 4.0 and above title boundary iteration,
        /// please use Word Boundary iterator. <see cref="GetWordInstance(ULocale)"/>
        /// </summary>
        /// <param name="where">A <see cref="ULocale"/> specifying the language of the text being analyzed.</param>
        /// <returns>A new instance of <see cref="BreakIterator"/> that locates title boundaries.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="where"/> is null.</exception>
        /// <stable>ICU 3.2</stable>
        public static BreakIterator GetTitleInstance(ULocale where)
        {
            return GetBreakInstance(where, KIND_TITLE);
        }

        /// <icu/>
        /// <summary>
        /// Registers a new break iterator of the indicated kind, to use in the given
        /// locale.  Clones of the iterator will be returned if a request for a break iterator
        /// of the given kind matches or falls back to this locale.
        /// <para/>
        /// Because ICU may choose to cache <see cref="BreakIterator"/> objects internally, this must
        /// be called at application startup, prior to any calls to
        /// BreakIterator.GetXYZInstance(where) to avoid undefined behavior.
        /// </summary>
        /// <param name="iter">The <see cref="BreakIterator"/> instance to adopt.</param>
        /// <param name="locale">The <see cref="CultureInfo"/> for which this instance is to be registered.</param>
        /// <param name="kind">The type of iterator for which this instance is to be registered.</param>
        /// <returns>A registry key that can be used to unregister this instance.</returns>
        /// <stable>ICU 2.4</stable>
        public static object RegisterInstance(BreakIterator iter, CultureInfo locale, int kind)
        {
            return RegisterInstance(iter, ULocale.ForLocale(locale), kind);
        }

        /// <icu/>
        /// <summary>
        /// Registers a new break iterator of the indicated kind, to use in the given
        /// locale.  Clones of the iterator will be returned if a request for a break iterator
        /// of the given kind matches or falls back to this locale.
        /// <para/>
        /// Because ICU may choose to cache <see cref="BreakIterator"/> objects internally, this must
        /// be called at application startup, prior to any calls to
        /// BreakIterator.GetXYZInstance(where) to avoid undefined behavior.
        /// </summary>
        /// <param name="iter">The <see cref="BreakIterator"/> instance to adopt.</param>
        /// <param name="locale">The <see cref="ULocale"/> for which this instance is to be registered.</param>
        /// <param name="kind">The type of iterator for which this instance is to be registered.</param>
        /// <returns>A registry key that can be used to unregister this instance.</returns>
        /// <stable>ICU 3.2</stable>
        public static object RegisterInstance(BreakIterator iter, ULocale locale, int kind)
        {
            // If the registered object matches the one in the cache, then
            // flush the cached object.
            if (iterCache[kind] != null)
            {
                BreakIteratorCache cache = (BreakIteratorCache)iterCache[kind].Get();
                if (cache != null)
                {
                    if (cache.Locale.Equals(locale))
                    {
                        iterCache[kind] = null;
                    }
                }
            }
            return GetShim().RegisterInstance(iter, locale, kind);
        }

        /// <icu/>
        /// <summary>
        /// Unregisters a previously-registered <see cref="BreakIterator"/> using the key returned
        /// from the register call.  Key becomes invalid after this call and should not be used
        /// again.
        /// </summary>
        /// <param name="key">The registry key returned by a previous call to 
        /// <see cref="RegisterInstance(BreakIterator, CultureInfo, int)"/>
        /// or <see cref="RegisterInstance(BreakIterator, ULocale, int)"/>.</param>
        /// <returns>true if the iterator for the key was successfully unregistered.</returns>
        /// <stable>ICU 2.4</stable>
        public static bool Unregister(object key)
        {
            if (key == null)
            {
                throw new ArgumentException("registry key must not be null");
            }
            // TODO: we don't do code coverage for the following lines
            // because in getBreakInstance we always instantiate the shim,
            // and test execution is such that we always instantiate a
            // breakiterator before we get to the break iterator tests.
            // this is for modularization, and we could remove the
            // dependencies in getBreakInstance by rewriting part of the
            // LocaleData code, or perhaps by accepting it into the
            // module.
            ///CLOVER:OFF
            if (shim != null)
            {
                // Unfortunately, we don't know what is being unregistered
                // -- what `kind' and what locale -- so we flush all
                // caches.  This is safe but inefficient if people are
                // actively registering and unregistering.
                for (int kind = 0; kind < KIND_COUNT; ++kind)
                {
                    iterCache[kind] = null;
                }
                return shim.Unregister(key);
            }
            return false;
            ///CLOVER:ON
        }

        // end of registration

        /// <summary>
        /// Returns a particular kind of <see cref="BreakIterator"/> for a locale.
        /// Avoids writing a switch statement with GetXYZInstance(where) calls.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public static BreakIterator GetBreakInstance(ULocale where, int kind)
        {
            if (where == null)
            {
                throw new ArgumentNullException(nameof(where), "Specified locale is null");
            }
            if (iterCache[kind] != null)
            {
                BreakIteratorCache cache2 = (BreakIteratorCache)iterCache[kind].Get();
                if (cache2 != null)
                {
                    if (cache2.Locale.Equals(where))
                    {
                        return cache2.CreateBreakInstance();
                    }
                }
            }

            // sigh, all to avoid linking in ICULocaleData...
            BreakIterator result = GetShim().CreateBreakIterator(where, kind);

            BreakIteratorCache cache = new BreakIteratorCache(where, result);
            iterCache[kind] = CacheValue<BreakIteratorCache>.GetInstance(cache);
            if (result is RuleBasedBreakIterator)
            {
                RuleBasedBreakIterator rbbi = (RuleBasedBreakIterator)result;
                rbbi.BreakType = kind;
            }

            return result;
        }

        /// <summary>
        /// Returns a list of locales for which <see cref="BreakIterator"/>s can be used.
        /// </summary>
        /// <returns>An array of <see cref="CultureInfo"/>s.  All of the locales in the array can
        /// be used when creating a <see cref="BreakIterator"/>.</returns>
        /// <stable>ICU 2.6</stable>
        public static CultureInfo[] GetAvailableLocales()
        {
            lock (syncLock)
            {
                // to avoid linking ICULocaleData
                return GetShim().GetAvailableLocales();
            }
        }

        /// <summary>
        /// Returns a list of locales for which <see cref="BreakIterator"/>s can be used.
        /// </summary>
        /// <returns>An array of <see cref="ULocale"/>s.  All of the locales in the array can
        /// be used when creating a <see cref="BreakIterator"/>.</returns>
        /// <draft>ICU 3.2 (retain)</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static ULocale[] GetAvailableULocales()
        {
            lock (syncLock)
            {
                // to avoid linking ICULocaleData
                return GetShim().GetAvailableULocales();
            }
        }

        private sealed class BreakIteratorCache
        {
            private BreakIterator iter;
            private ULocale where;

            internal BreakIteratorCache(ULocale where, BreakIterator iter)
            {
                this.where = where;
                this.iter = (BreakIterator)iter.Clone();
            }

            internal ULocale Locale
            {
                get { return where; }
            }

            internal BreakIterator CreateBreakInstance()
            {
                return (BreakIterator)iter.Clone();
            }
        }

        internal abstract class BreakIteratorServiceShim
        {
            public abstract object RegisterInstance(BreakIterator iter, ULocale l, int k);
            public abstract bool Unregister(Object key);
            public abstract CultureInfo[] GetAvailableLocales();
            public abstract ULocale[] GetAvailableULocales();
            public abstract BreakIterator CreateBreakIterator(ULocale l, int k);
        }

        private static BreakIteratorServiceShim shim;
        private static BreakIteratorServiceShim GetShim()
        {
            // Note: this instantiation is safe on loose-memory-model configurations
            // despite lack of synchronization, since the shim instance has no state--
            // it's all in the class init.  The worst problem is we might instantiate
            // two shim instances, but they'll share the same state so that's ok.
            if (shim == null)
            {
                try
                {
                    //Class <?> cls = Class.forName("com.ibm.icu.text.BreakIteratorFactory");
                    Type cls = Type.GetType("ICU4N.Text.BreakIteratorFactory, ICU4N"); // ICU4N TODO: API Set BreakIteratorFactory statically on BreakIterator abstract class so it can be injected (this won't allow external injection)
                    shim = (BreakIteratorServiceShim)Activator.CreateInstance(cls);
                }
                catch (TypeInitializationException) // ICU4N TODO: Check exception type
                {
                    throw;
                }
                catch (Exception e)
                {
                    ///CLOVER:OFF
                    //if (DEBUG)
                    //{
                    //    e.printStackTrace();
                    //}
                    throw new Exception(e.ToString(), e);
                    ///CLOVER:ON
                }
            }
            return shim;
        }

        // -------- BEGIN ULocale boilerplate --------

        /// <icu/>
        /// <summary>
        /// Returns the locale that was used to create this object, or null.
        /// This may may differ from the locale requested at the time of
        /// this object's creation.  For example, if an object is created
        /// for locale <c>en_US_CALIFORNIA</c>, the actual data may be
        /// drawn from <c>en</c> (the <i>actual</i> locale), and
        /// <tt>en_US</tt> may be the most specific locale that exists (the
        /// <i>valid</i> locale).
        /// <para/>
        /// Note: The <i>actual</i> locale is returned correctly, but the <i>valid</i>
        /// locale is not, in most cases.
        /// </summary>
        /// <param name="type">Type of information requested, either 
        /// <see cref="ULocale.VALID_LOCALE"/>
        /// or <see cref="ULocale.ACTUAL_LOCALE"/>.</param>
        /// <returns>The information specified by <i>type</i>, or null if
        /// this object was not constructed from locale data.</returns>
        /// <seealso cref="ULocale"/>
        /// <seealso cref="ULocale.VALID_LOCALE"/>
        /// <seealso cref="ULocale.ACTUAL_LOCALE"/>
        /// <draft>ICU 2.8 (retain)</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public ULocale GetLocale(ULocale.Type type)
        {
            return type == ULocale.ACTUAL_LOCALE ?
                this.actualLocale : this.validLocale;
        }

        /// <summary>
        /// Set information about the locales that were used to create this
        /// object.  If the object was not constructed from locale data,
        /// both arguments should be set to null.  Otherwise, neither
        /// should be null.  The actual locale must be at the same level or
        /// less specific than the valid locale.  This method is intended
        /// for use by factories or other entities that create objects of
        /// this class.
        /// </summary>
        /// <param name="valid">The most specific locale containing any resource
        /// data, or null.</param>
        /// <param name="actual">The locale containing data used to construct this
        /// object, or null.</param>
        /// <seealso cref="ULocale"/>
        /// <seealso cref="ULocale.VALID_LOCALE"/>
        /// <seealso cref="ULocale.ACTUAL_LOCALE"/>
        internal void SetLocale(ULocale valid, ULocale actual)
        {
            // Change the following to an assertion later
            if ((valid == null) != (actual == null))
            {
                ///CLOVER:OFF
                throw new ArgumentException();
                ///CLOVER:ON
            }
            // Another check we could do is that the actual locale is at
            // the same level or less specific than the valid locale.
            this.validLocale = valid;
            this.actualLocale = actual;
        }

        /// <summary>
        /// The most specific locale containing any resource data, or null.
        /// </summary>
        /// <seealso cref="ULocale"/>
        private ULocale validLocale;

        /// <summary>
        /// The locale containing data used to construct this object, or
        /// null.
        /// </summary>
        /// <seealso cref="ULocale"/>
        private ULocale actualLocale;

        // -------- END ULocale boilerplate --------
    }
}
