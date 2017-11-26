using ICU4N.Impl;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Category = ICU4N.Util.ULocale.Category; // ICU4N TODO: De-nest ?
using StringBuffer = System.Text.StringBuilder;
using System.Collections.Concurrent;
using SingleID = ICU4N.Text.TransliteratorIDParser.SingleID;

namespace ICU4N.Text
{
    public abstract class Transliterator : IStringTransform
    {
        // ICU4N specific - need to use the current assembly for resources
        public static readonly Assembly ICU_DATA_CLASS_LOADER = typeof(Transliterator).GetTypeInfo().Assembly;

        /**
         * Direction constant indicating the forward direction in a transliterator,
         * e.g., the forward rules of a RuleBasedTransliterator.  An "A-B"
         * transliterator transliterates A to B when operating in the forward
         * direction, and B to A when operating in the reverse direction.
         * @stable ICU 2.0
         */
        public static readonly int FORWARD = 0;

        /**
         * Direction constant indicating the reverse direction in a transliterator,
         * e.g., the reverse rules of a RuleBasedTransliterator.  An "A-B"
         * transliterator transliterates A to B when operating in the forward
         * direction, and B to A when operating in the reverse direction.
         * @stable ICU 2.0
         */
        public static readonly int REVERSE = 1;

        /**
         * Position structure for incremental transliteration.  This data
         * structure defines two substrings of the text being
         * transliterated.  The first region, [contextStart,
         * contextLimit), defines what characters the transliterator will
         * read as context.  The second region, [start, limit), defines
         * what characters will actually be transliterated.  The second
         * region should be a subset of the first.
         *
         * <p>After a transliteration operation, some of the indices in this
         * structure will be modified.  See the field descriptions for
         * details.
         *
         * <p>contextStart &lt;= start &lt;= limit &lt;= contextLimit
         *
         * <p>Note: All index values in this structure must be at code point
         * boundaries.  That is, none of them may occur between two code units
         * of a surrogate pair.  If any index does split a surrogate pair,
         * results are unspecified.
         * @stable ICU 2.0
         */
        public class Position
        {

            /**
             * Beginning index, inclusive, of the context to be considered for
             * a transliteration operation.  The transliterator will ignore
             * anything before this index.  INPUT/OUTPUT parameter: This parameter
             * is updated by a transliteration operation to reflect the maximum
             * amount of antecontext needed by a transliterator.
             * @stable ICU 2.0
             */
            public int ContextStart { get; set; }

            /**
             * Ending index, exclusive, of the context to be considered for a
             * transliteration operation.  The transliterator will ignore
             * anything at or after this index.  INPUT/OUTPUT parameter: This
             * parameter is updated to reflect changes in the length of the
             * text, but points to the same logical position in the text.
             * @stable ICU 2.0
             */
            public int ContextLimit { get; set; }

            /**
             * Beginning index, inclusive, of the text to be transliteratd.
             * INPUT/OUTPUT parameter: This parameter is advanced past
             * characters that have already been transliterated by a
             * transliteration operation.
             * @stable ICU 2.0
             */
            public int Start { get; set; }

            /**
             * Ending index, exclusive, of the text to be transliteratd.
             * INPUT/OUTPUT parameter: This parameter is updated to reflect
             * changes in the length of the text, but points to the same
             * logical position in the text.
             * @stable ICU 2.0
             */
            public int Limit { get; set; }

            /**
             * Constructs a Position object with start, limit,
             * contextStart, and contextLimit all equal to zero.
             * @stable ICU 2.0
             */
            public Position()
                : this(0, 0, 0, 0)
            {
            }

            /**
             * Constructs a Position object with the given start,
             * contextStart, and contextLimit.  The limit is set to the
             * contextLimit.
             * @stable ICU 2.0
             */
            public Position(int contextStart, int contextLimit, int start)
                : this(contextStart, contextLimit, start, contextLimit)
            {
            }

            /**
             * Constructs a Position object with the given start, limit,
             * contextStart, and contextLimit.
             * @stable ICU 2.0
             */
            public Position(int contextStart, int contextLimit,
                            int start, int limit)
            {
                this.ContextStart = contextStart;
                this.ContextLimit = contextLimit;
                this.Start = start;
                this.Limit = limit;
            }

            /**
             * Constructs a Position object that is a copy of another.
             * @stable ICU 2.6
             */
            public Position(Position pos)
            {
                Set(pos);
            }

            /**
             * Copies the indices of this position from another.
             * @stable ICU 2.6
             */
            public virtual void Set(Position pos)
            {
                ContextStart = pos.ContextStart;
                ContextLimit = pos.ContextLimit;
                Start = pos.Start;
                Limit = pos.Limit;
            }

            /**
             * Returns true if this Position is equal to the given object.
             * @stable ICU 2.6
             */
            public override bool Equals(object obj)
            {
                if (obj is Position)
                {
                    Position pos = (Position)obj;
                    return ContextStart == pos.ContextStart &&
                        ContextLimit == pos.ContextLimit &&
                        Start == pos.Start &&
                        Limit == pos.Limit;
                }
                return false;
            }

            /**
             * Mock implementation of hashCode(). This implementation always returns a constant
             * value. When Java assertion is enabled, this method triggers an assertion failure.
             * @internal
             * @deprecated This API is ICU internal only.
             */
            [Obsolete("This API is ICU internal only.")]
            public override int GetHashCode()
            {
                Debug.Assert(false, "hashCode not designed");
                return 42;
            }

            /**
             * Returns a string representation of this Position.
             * @stable ICU 2.6
             */
            public override string ToString()
            {
                return "[cs=" + ContextStart
                    + ", s=" + Start
                    + ", l=" + Limit
                    + ", cl=" + ContextLimit
                    + "]";
            }

            /**
             * Check all bounds.  If they are invalid, throw an exception.
             * @param length the length of the string this object applies to
             * @exception IllegalArgumentException if any indices are out
             * of bounds
             * @stable ICU 2.0
             */
            public void Validate(int length)
            {
                if (ContextStart < 0 ||
                    Start < ContextStart ||
                    Limit < Start ||
                    ContextLimit < Limit ||
                    length < ContextLimit)
                {
                    throw new ArgumentException("Invalid Position {cs=" +
                                                       ContextStart + ", s=" +
                                                       Start + ", l=" +
                                                       Limit + ", cl=" +
                                                       ContextLimit + "}, len=" +
                                                       length);
                }
            }
        }

        /**
         * Programmatic name, e.g., "Latin-Arabic".
         */
        private string id;

        /**
         * This transliterator's filter.  Any character for which
         * <tt>filter.contains()</tt> returns <tt>false</tt> will not be
         * altered by this transliterator.  If <tt>filter</tt> is
         * <tt>null</tt> then no filtering is applied.
         */
        private UnicodeSet filter;

        private int maximumContextLength = 0;

        /**
         * System transliterator registry.
         */
        private static TransliteratorRegistry registry;

        private static IDictionary<CaseInsensitiveString, string> displayNameCache;

        /**
         * Prefix for resource bundle key for the display name for a
         * transliterator.  The ID is appended to this to form the key.
         * The resource bundle value should be a String.
         */
        private static readonly string RB_DISPLAY_NAME_PREFIX = "%Translit%%";

        /**
         * Prefix for resource bundle key for the display name for a
         * transliterator SCRIPT.  The ID is appended to this to form the key.
         * The resource bundle value should be a String.
         */
        private static readonly string RB_SCRIPT_DISPLAY_NAME_PREFIX = "%Translit%";

        /**
         * Resource bundle key for display name pattern.
         * The resource bundle value should be a String forming a
         * MessageFormat pattern, e.g.:
         * "{0,choice,0#|1#{1} Transliterator|2#{1} to {2} Transliterator}".
         */
        private static readonly string RB_DISPLAY_NAME_PATTERN = "TransliteratorNamePattern";

        /**
         * Delimiter between elements in a compound ID.
         */
        internal static readonly char ID_DELIM = ';';

        /**
         * Delimiter before target in an ID.
         */
        internal static readonly char ID_SEP = '-';

        /**
         * Delimiter before variant in an ID.
         */
        internal static readonly char VARIANT_SEP = '/';

        /**
         * To enable debugging output in the Transliterator component, set
         * DEBUG to true.
         *
         * N.B. Make sure to recompile all of the com.ibm.icu.text package
         * after changing this.  Easiest way to do this is 'ant clean
         * core' ('ant' will NOT pick up the dependency automatically).
         *
         * <<This generates a lot of output.>>
         */
        internal static readonly bool DEBUG = false;

        /**
         * Default constructor.
         * @param ID the string identifier for this transliterator
         * @param filter the filter.  Any character for which
         * <tt>filter.contains()</tt> returns <tt>false</tt> will not be
         * altered by this transliterator.  If <tt>filter</tt> is
         * <tt>null</tt> then no filtering is applied.
         * @stable ICU 2.0
         */
        protected Transliterator(string id, UnicodeFilter filter)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(Text.Transliterator.id));
            }
            this.id = id;
            Filter = filter;
        }

        /**
         * Transliterates a segment of a string, with optional filtering.
         *
         * @param text the string to be transliterated
         * @param start the beginning index, inclusive; <code>0 &lt;= start
         * &lt;= limit</code>.
         * @param limit the ending index, exclusive; <code>start &lt;= limit
         * &lt;= text.length()</code>.
         * @return The new limit index.  The text previously occupying <code>[start,
         * limit)</code> has been transliterated, possibly to a string of a different
         * length, at <code>[start, </code><em>new-limit</em><code>)</code>, where
         * <em>new-limit</em> is the return value. If the input offsets are out of bounds,
         * the returned value is -1 and the input string remains unchanged.
         * @stable ICU 2.0
         */
        public int Transliterate(IReplaceable text, int start, int limit)
        {
            if (start < 0 ||
                limit < start ||
                text.Length < limit)
            {
                return -1;
            }

            Position pos = new Position(start, limit, start);
            FilteredTransliterate(text, pos, false, true);
            return pos.Limit;
        }

        /**
         * Transliterates an entire string in place. Convenience method.
         * @param text the string to be transliterated
         * @stable ICU 2.0
         */
        public void Transliterate(IReplaceable text)
        {
            Transliterate(text, 0, text.Length);
        }

        /**
         * Transliterate an entire string and returns the result. Convenience method.
         *
         * @param text the string to be transliterated
         * @return The transliterated text
         * @stable ICU 2.0
         */
        public string Transliterate(string text)
        {
            ReplaceableString result = new ReplaceableString(text);
            Transliterate(result);
            return result.ToString();
        }

        /**
         * Transliterates the portion of the text buffer that can be
         * transliterated unambiguosly after new text has been inserted,
         * typically as a result of a keyboard event.  The new text in
         * <code>insertion</code> will be inserted into <code>text</code>
         * at <code>index.contextLimit</code>, advancing
         * <code>index.contextLimit</code> by <code>insertion.length()</code>.
         * Then the transliterator will try to transliterate characters of
         * <code>text</code> between <code>index.start</code> and
         * <code>index.contextLimit</code>.  Characters before
         * <code>index.start</code> will not be changed.
         *
         * <p>Upon return, values in <code>index</code> will be updated.
         * <code>index.contextStart</code> will be advanced to the first
         * character that future calls to this method will read.
         * <code>index.start</code> and <code>index.contextLimit</code> will
         * be adjusted to delimit the range of text that future calls to
         * this method may change.
         *
         * <p>Typical usage of this method begins with an initial call
         * with <code>index.contextStart</code> and <code>index.contextLimit</code>
         * set to indicate the portion of <code>text</code> to be
         * transliterated, and <code>index.start == index.contextStart</code>.
         * Thereafter, <code>index</code> can be used without
         * modification in future calls, provided that all changes to
         * <code>text</code> are made via this method.
         *
         * <p>This method assumes that future calls may be made that will
         * insert new text into the buffer.  As a result, it only performs
         * unambiguous transliterations.  After the last call to this
         * method, there may be untransliterated text that is waiting for
         * more input to resolve an ambiguity.  In order to perform these
         * pending transliterations, clients should call {@link
         * #finishTransliteration} after the last call to this
         * method has been made.
         *
         * @param text the buffer holding transliterated and untransliterated text
         * @param index the start and limit of the text, the position
         * of the cursor, and the start and limit of transliteration.
         * @param insertion text to be inserted and possibly
         * transliterated into the translation buffer at
         * <code>index.contextLimit</code>.  If <code>null</code> then no text
         * is inserted.
         * @see #handleTransliterate
         * @exception IllegalArgumentException if <code>index</code>
         * is invalid
         * @stable ICU 2.0
         */
        public void Transliterate(IReplaceable text, Position index,
                                        string insertion)
        {
            index.Validate(text.Length);

            //        int originalStart = index.contextStart;
            if (insertion != null)
            {
                text.Replace(index.Limit, index.Limit, insertion);
                index.Limit += insertion.Length;
                index.ContextLimit += insertion.Length;
            }

            if (index.Limit > 0 &&
                UTF16.IsLeadSurrogate(text[index.Limit - 1]))
            {
                // Oops, there is a dangling lead surrogate in the buffer.
                // This will break most transliterators, since they will
                // assume it is part of a pair.  Don't transliterate until
                // more text comes in.
                return;
            }

            FilteredTransliterate(text, index, true, true);

            // TODO
            // This doesn't work once we add quantifier support.  Need to rewrite
            // this code to support quantifiers and 'use maximum backup <n>;'.
            //
            //        index.contextStart = Math.max(index.start - getMaximumContextLength(),
            //                                      originalStart);
        }

        /**
         * Transliterates the portion of the text buffer that can be
         * transliterated unambiguosly after a new character has been
         * inserted, typically as a result of a keyboard event.  This is a
         * convenience method; see {@link #transliterate(Replaceable,
         * Transliterator.Position, String)} for details.
         * @param text the buffer holding transliterated and
         * untransliterated text
         * @param index the start and limit of the text, the position
         * of the cursor, and the start and limit of transliteration.
         * @param insertion text to be inserted and possibly
         * transliterated into the translation buffer at
         * <code>index.contextLimit</code>.
         * @see #transliterate(Replaceable, Transliterator.Position, String)
         * @stable ICU 2.0
         */
        public void Transliterate(IReplaceable text, Position index,
                                        int insertion)
        {
            Transliterate(text, index, UTF16.ValueOf(insertion));
        }

        /**
         * Transliterates the portion of the text buffer that can be
         * transliterated unambiguosly.  This is a convenience method; see
         * {@link #transliterate(Replaceable, Transliterator.Position,
         * String)} for details.
         * @param text the buffer holding transliterated and
         * untransliterated text
         * @param index the start and limit of the text, the position
         * of the cursor, and the start and limit of transliteration.
         * @see #transliterate(Replaceable, Transliterator.Position, String)
         * @stable ICU 2.0
         */
        public void Transliterate(IReplaceable text, Position index)
        {
            Transliterate(text, index, null);
        }

        /**
         * Finishes any pending transliterations that were waiting for
         * more characters.  Clients should call this method as the last
         * call after a sequence of one or more calls to
         * <code>transliterate()</code>.
         * @param text the buffer holding transliterated and
         * untransliterated text.
         * @param index the array of indices previously passed to {@link
         * #transliterate}
         * @stable ICU 2.0
         */
        public void FinishTransliteration(IReplaceable text,
                                                Position index)
        {
            index.Validate(text.Length);
            FilteredTransliterate(text, index, false, true);
        }

        /**
         * Abstract method that concrete subclasses define to implement
         * their transliteration algorithm.  This method handles both
         * incremental and non-incremental transliteration.  Let
         * <code>originalStart</code> refer to the value of
         * <code>pos.start</code> upon entry.
         *
         * <ul>
         *  <li>If <code>incremental</code> is false, then this method
         *  should transliterate all characters between
         *  <code>pos.start</code> and <code>pos.limit</code>. Upon return
         *  <code>pos.start</code> must == <code> pos.limit</code>.</li>
         *
         *  <li>If <code>incremental</code> is true, then this method
         *  should transliterate all characters between
         *  <code>pos.start</code> and <code>pos.limit</code> that can be
         *  unambiguously transliterated, regardless of future insertions
         *  of text at <code>pos.limit</code>.  Upon return,
         *  <code>pos.start</code> should be in the range
         *  [<code>originalStart</code>, <code>pos.limit</code>).
         *  <code>pos.start</code> should be positioned such that
         *  characters [<code>originalStart</code>, <code>
         *  pos.start</code>) will not be changed in the future by this
         *  transliterator and characters [<code>pos.start</code>,
         *  <code>pos.limit</code>) are unchanged.</li>
         * </ul>
         *
         * <p>Implementations of this method should also obey the
         * following invariants:</p>
         *
         * <ul>
         *  <li> <code>pos.limit</code> and <code>pos.contextLimit</code>
         *  should be updated to reflect changes in length of the text
         *  between <code>pos.start</code> and <code>pos.limit</code>. The
         *  difference <code> pos.contextLimit - pos.limit</code> should
         *  not change.</li>
         *
         *  <li><code>pos.contextStart</code> should not change.</li>
         *
         *  <li>Upon return, neither <code>pos.start</code> nor
         *  <code>pos.limit</code> should be less than
         *  <code>originalStart</code>.</li>
         *
         *  <li>Text before <code>originalStart</code> and text after
         *  <code>pos.limit</code> should not change.</li>
         *
         *  <li>Text before <code>pos.contextStart</code> and text after
         *  <code> pos.contextLimit</code> should be ignored.</li>
         * </ul>
         *
         * <p>Subclasses may safely assume that all characters in
         * [<code>pos.start</code>, <code>pos.limit</code>) are filtered.
         * In other words, the filter has already been applied by the time
         * this method is called.  See
         * <code>filteredTransliterate()</code>.
         *
         * <p>This method is <b>not</b> for public consumption.  Calling
         * this method directly will transliterate
         * [<code>pos.start</code>, <code>pos.limit</code>) without
         * applying the filter. End user code should call <code>
         * transliterate()</code> instead of this method. Subclass code
         * should call <code>filteredTransliterate()</code> instead of
         * this method.<p>
         *
         * @param text the buffer holding transliterated and
         * untransliterated text
         *
         * @param pos the indices indicating the start, limit, context
         * start, and context limit of the text.
         *
         * @param incremental if true, assume more text may be inserted at
         * <code>pos.limit</code> and act accordingly.  Otherwise,
         * transliterate all text between <code>pos.start</code> and
         * <code>pos.limit</code> and move <code>pos.start</code> up to
         * <code>pos.limit</code>.
         *
         * @see #transliterate
         * @stable ICU 2.0
         */
        protected abstract void HandleTransliterate(IReplaceable text,
                                                    Position pos, bool incremental);

        /**
         * Top-level transliteration method, handling filtering, incremental and
         * non-incremental transliteration, and rollback.  All transliteration
         * public API methods eventually call this method with a rollback argument
         * of TRUE.  Other entities may call this method but rollback should be
         * FALSE.
         *
         * <p>If this transliterator has a filter, break up the input text into runs
         * of unfiltered characters.  Pass each run to
         * <subclass>.handleTransliterate().
         *
         * <p>In incremental mode, if rollback is TRUE, perform a special
         * incremental procedure in which several passes are made over the input
         * text, adding one character at a time, and committing successful
         * transliterations as they occur.  Unsuccessful transliterations are rolled
         * back and retried with additional characters to give correct results.
         *
         * @param text the text to be transliterated
         * @param index the position indices
         * @param incremental if TRUE, then assume more characters may be inserted
         * at index.limit, and postpone processing to accomodate future incoming
         * characters
         * @param rollback if TRUE and if incremental is TRUE, then perform special
         * incremental processing, as described above, and undo partial
         * transliterations where necessary.  If incremental is FALSE then this
         * parameter is ignored.
         */
        private void FilteredTransliterate(IReplaceable text,
                                           Position index,
                                           bool incremental,
                                           bool rollback)
        {
            // Short circuit path for transliterators with no filter in
            // non-incremental mode.
            if (filter == null && !rollback)
            {
                HandleTransliterate(text, index, incremental);
                return;
            }

            //----------------------------------------------------------------------
            // This method processes text in two groupings:
            //
            // RUNS -- A run is a contiguous group of characters which are contained
            // in the filter for this transliterator (filter.contains(ch) == true).
            // Text outside of runs may appear as context but it is not modified.
            // The start and limit Position values are narrowed to each run.
            //
            // PASSES (incremental only) -- To make incremental mode work correctly,
            // each run is broken up into n passes, where n is the length (in code
            // points) of the run.  Each pass contains the first n characters.  If a
            // pass is completely transliterated, it is committed, and further passes
            // include characters after the committed text.  If a pass is blocked,
            // and does not transliterate completely, then this method rolls back
            // the changes made during the pass, extends the pass by one code point,
            // and tries again.
            //----------------------------------------------------------------------

            // globalLimit is the limit value for the entire operation.  We
            // set index.limit to the end of each unfiltered run before
            // calling handleTransliterate(), so we need to maintain the real
            // value of index.limit here.  After each transliteration, we
            // update globalLimit for insertions or deletions that have
            // happened.
            int globalLimit = index.Limit;

            // If there is a non-null filter, then break the input text up.  Say the
            // input text has the form:
            //   xxxabcxxdefxx
            // where 'x' represents a filtered character (filter.contains('x') ==
            // false).  Then we break this up into:
            //   xxxabc xxdef xx
            // Each pass through the loop consumes a run of filtered
            // characters (which are ignored) and a subsequent run of
            // unfiltered characters (which are transliterated).

            StringBuffer log = null;
            if (DEBUG)
            {
                log = new StringBuffer();
            }

            for (; ; )
            {

                if (filter != null)
                {
                    // Narrow the range to be transliterated to the first run
                    // of unfiltered characters at or after index.start.

                    // Advance past filtered chars
                    int c;
                    while (index.Start < globalLimit &&
                           !filter.Contains(c = text.Char32At(index.Start)))
                    {
                        index.Start += UTF16.GetCharCount(c);
                    }

                    // Find the end of this run of unfiltered chars
                    index.Limit = index.Start;
                    while (index.Limit < globalLimit &&
                           filter.Contains(c = text.Char32At(index.Limit)))
                    {
                        index.Limit += UTF16.GetCharCount(c);
                    }
                }

                // Check to see if the unfiltered run is empty.  This only
                // happens at the end of the string when all the remaining
                // characters are filtered.
                if (index.Start == index.Limit)
                {
                    break;
                }

                // Is this run incremental?  If there is additional
                // filtered text (if limit < globalLimit) then we pass in
                // an incremental value of FALSE to force the subclass to
                // complete the transliteration for this run.
                bool isIncrementalRun =
                    (index.Limit < globalLimit ? false : incremental);

                int delta;

                // Implement rollback.  To understand the need for rollback,
                // consider the following transliterator:
                //
                //  "t" is "a > A;"
                //  "u" is "A > b;"
                //  "v" is a compound of "t; NFD; u" with a filter [:Ll:]
                //
                // Now apply "v" to the input text "a".  The result is "b".  But if
                // the transliteration is done incrementally, then the NFD holds
                // things up after "t" has already transformed "a" to "A".  When
                // finishTransliterate() is called, "A" is _not_ processed because
                // it gets excluded by the [:Ll:] filter, and the end result is "A"
                // -- incorrect.  The problem is that the filter is applied to a
                // partially-transliterated result, when we only want it to apply to
                // input text.  Although this example describes a compound
                // transliterator containing NFD and a specific filter, it can
                // happen with any transliterator which does a partial
                // transformation in incremental mode into characters outside its
                // filter.
                //
                // To handle this, when in incremental mode we supply characters to
                // handleTransliterate() in several passes.  Each pass adds one more
                // input character to the input text.  That is, for input "ABCD", we
                // first try "A", then "AB", then "ABC", and finally "ABCD".  If at
                // any point we block (upon return, start < limit) then we roll
                // back.  If at any point we complete the run (upon return start ==
                // limit) then we commit that run.

                if (rollback && isIncrementalRun)
                {

                    if (DEBUG)
                    {
                        log.Length = 0;
                        Console.Out.WriteLine("filteredTransliterate{" + ID + "}i: IN=" +
                                           UtilityExtensions.FormatInput(text, index));
                    }

                    int runStart = index.Start;
                    int runLimit = index.Limit;
                    int runLength = runLimit - runStart;

                    // Make a rollback copy at the end of the string
                    int rollbackOrigin = text.Length;
                    text.Copy(runStart, runLimit, rollbackOrigin);

                    // Variables reflecting the commitment of completely
                    // transliterated text.  passStart is the runStart, advanced
                    // past committed text.  rollbackStart is the rollbackOrigin,
                    // advanced past rollback text that corresponds to committed
                    // text.
                    int passStart = runStart;
                    int rollbackStart = rollbackOrigin;

                    // The limit for each pass; we advance by one code point with
                    // each iteration.
                    int passLimit = index.Start;

                    // Total length, in 16-bit code units, of uncommitted text.
                    // This is the length to be rolled back.
                    int uncommittedLength = 0;

                    // Total delta (change in length) for all passes
                    int totalDelta = 0;

                    // PASS MAIN LOOP -- Start with a single character, and extend
                    // the text by one character at a time.  Roll back partial
                    // transliterations and commit complete transliterations.
                    for (; ; )
                    {
                        // Length of additional code point, either one or two
                        int charLength =
                            UTF16.GetCharCount(text.Char32At(passLimit));
                        passLimit += charLength;
                        if (passLimit > runLimit)
                        {
                            break;
                        }
                        uncommittedLength += charLength;

                        index.Limit = passLimit;

                        if (DEBUG)
                        {
                            log.Length = 0;
                            log.Append("filteredTransliterate{" + ID + "}i: ");
                            UtilityExtensions.FormatInput(log, text, index);
                        }

                        // Delegate to subclass for actual transliteration.  Upon
                        // return, start will be updated to point after the
                        // transliterated text, and limit and contextLimit will be
                        // adjusted for length changes.
                        HandleTransliterate(text, index, true);

                        if (DEBUG)
                        {
                            log.Append(" => ");
                            UtilityExtensions.FormatInput(log, text, index);
                        }

                        delta = index.Limit - passLimit; // change in length

                        // We failed to completely transliterate this pass.
                        // Roll back the text.  Indices remain unchanged; reset
                        // them where necessary.
                        if (index.Start != index.Limit)
                        {
                            // Find the rollbackStart, adjusted for length changes
                            // and the deletion of partially transliterated text.
                            int rs = rollbackStart + delta - (index.Limit - passStart);

                            // Delete the partially transliterated text
                            text.Replace(passStart, index.Limit, "");

                            // Copy the rollback text back
                            text.Copy(rs, rs + uncommittedLength, passStart);

                            // Restore indices to their original values
                            index.Start = passStart;
                            index.Limit = passLimit;
                            index.ContextLimit -= delta;

                            if (DEBUG)
                            {
                                log.Append(" (ROLLBACK)");
                            }
                        }

                        // We did completely transliterate this pass.  Update the
                        // commit indices to record how far we got.  Adjust indices
                        // for length change.
                        else
                        {
                            // Move the pass indices past the committed text.
                            passStart = passLimit = index.Start;

                            // Adjust the rollbackStart for length changes and move
                            // it past the committed text.  All characters we've
                            // processed to this point are committed now, so zero
                            // out the uncommittedLength.
                            rollbackStart += delta + uncommittedLength;
                            uncommittedLength = 0;

                            // Adjust indices for length changes.
                            runLimit += delta;
                            totalDelta += delta;
                        }

                        if (DEBUG)
                        {
                            Console.Out.WriteLine(Utility.Escape(log.ToString()));
                        }
                    }

                    // Adjust overall limit and rollbackOrigin for insertions and
                    // deletions.  Don't need to worry about contextLimit because
                    // handleTransliterate() maintains that.
                    rollbackOrigin += totalDelta;
                    globalLimit += totalDelta;

                    // Delete the rollback copy
                    text.Replace(rollbackOrigin, rollbackOrigin + runLength, "");

                    // Move start past committed text
                    index.Start = passStart;
                }

                else
                {
                    // Delegate to subclass for actual transliteration.
                    if (DEBUG)
                    {
                        log.Length = 0;
                        log.Append("filteredTransliterate{" + ID + "}: ");
                        UtilityExtensions.FormatInput(log, text, index);
                    }

                    int limit = index.Limit;
                    HandleTransliterate(text, index, isIncrementalRun);
                    delta = index.Limit - limit; // change in length

                    if (DEBUG)
                    {
                        log.Append(" => ");
                        UtilityExtensions.FormatInput(log, text, index);
                    }

                    // In a properly written transliterator, start == limit after
                    // handleTransliterate() returns when incremental is false.
                    // Catch cases where the subclass doesn't do this, and throw
                    // an exception.  (Just pinning start to limit is a bad idea,
                    // because what's probably happening is that the subclass
                    // isn't transliterating all the way to the end, and it should
                    // in non-incremental mode.)
                    if (!isIncrementalRun && index.Start != index.Limit)
                    {
                        throw new Exception("ERROR: Incomplete non-incremental transliteration by " + ID);
                    }

                    // Adjust overall limit for insertions/deletions.  Don't need
                    // to worry about contextLimit because handleTransliterate()
                    // maintains that.
                    globalLimit += delta;

                    if (DEBUG)
                    {
                        Console.Out.WriteLine(Utility.Escape(log.ToString()));
                    }
                }

                if (filter == null || isIncrementalRun)
                {
                    break;
                }

                // If we did completely transliterate this
                // run, then repeat with the next unfiltered run.
            }

            // Start is valid where it is.  Limit needs to be put back where
            // it was, modulo adjustments for deletions/insertions.
            index.Limit = globalLimit;

            if (DEBUG)
            {
                Console.Out.WriteLine("filteredTransliterate{" + ID + "}: OUT=" +
                                   UtilityExtensions.FormatInput(text, index));
            }
        }

        /**
         * Transliterate a substring of text, as specified by index, taking filters
         * into account.  This method is for subclasses that need to delegate to
         * another transliterator, such as CompoundTransliterator.
         * @param text the text to be transliterated
         * @param index the position indices
         * @param incremental if TRUE, then assume more characters may be inserted
         * at index.limit, and postpone processing to accomodate future incoming
         * characters
         * @stable ICU 2.0
         */
        public virtual void FilteredTransliterate(IReplaceable text,
                                             Position index,
                                             bool incremental)
        {
            FilteredTransliterate(text, index, incremental, false);
        }

        /**
         * Returns the length of the longest context required by this transliterator.
         * This is <em>preceding</em> context.  The default value is zero, but
         * subclasses can change this by calling <code>setMaximumContextLength()</code>.
         * For example, if a transliterator translates "ddd" (where
         * d is any digit) to "555" when preceded by "(ddd)", then the preceding
         * context length is 5, the length of "(ddd)".
         *
         * @return The maximum number of preceding context characters this
         * transliterator needs to examine
         * @stable ICU 2.0
         */
        public int MaximumContextLength
        {
            get { return maximumContextLength; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Invalid context length " + value);
                }
                maximumContextLength = value;
            }
        }

        ///**
        // * Method for subclasses to use to set the maximum context length.
        // * @see #getMaximumContextLength
        // * @stable ICU 2.0
        // */
        //protected void setMaximumContextLength(int a)
        //{
        //    if (a < 0)
        //    {
        //        throw new ArgumentException("Invalid context length " + a);
        //    }
        //    maximumContextLength = a;
        //}

        /**
         * Returns a programmatic identifier for this transliterator.
         * If this identifier is passed to <code>getInstance()</code>, it
         * will return this object, if it has been registered.
         * @see #registerClass
         * @see #getAvailableIDs
         * @stable ICU 2.0
         */
        public string ID
        {
            get { return id; }
            set { this.id = value; }
        }

        ///**
        // * Set the programmatic identifier for this transliterator.  Only
        // * for use by subclasses.
        // * @stable ICU 2.0
        // */
        //protected final void setID(String id)
        //{
        //    this.id = id;
        //}

        /**
         * Returns a name for this transliterator that is appropriate for
         * display to the user in the default <code>DISPLAY</code> locale.  See {@link
         * #getDisplayName(String,Locale)} for details.
         * @see com.ibm.icu.util.ULocale.Category#DISPLAY
         * @stable ICU 2.0
         */
        public static string GetDisplayName(string ID)
        {
            return GetDisplayName(ID, ULocale.GetDefault(Category.DISPLAY));
        }

        /**
         * Returns a name for this transliterator that is appropriate for
         * display to the user in the given locale.  This name is taken
         * from the locale resource data in the standard manner of the
         * <code>java.text</code> package.
         *
         * <p>If no localized names exist in the system resource bundles,
         * a name is synthesized using a localized
         * <code>MessageFormat</code> pattern from the resource data.  The
         * arguments to this pattern are an integer followed by one or two
         * strings.  The integer is the number of strings, either 1 or 2.
         * The strings are formed by splitting the ID for this
         * transliterator at the first '-'.  If there is no '-', then the
         * entire ID forms the only string.
         * @param inLocale the Locale in which the display name should be
         * localized.
         * @see java.text.MessageFormat
         * @stable ICU 2.0
         */
        public static string GetDisplayName(string id, CultureInfo inLocale)
        {
            return GetDisplayName(id, ULocale.ForLocale(inLocale));
        }

        /**
         * Returns a name for this transliterator that is appropriate for
         * display to the user in the given locale.  This name is taken
         * from the locale resource data in the standard manner of the
         * <code>java.text</code> package.
         *
         * <p>If no localized names exist in the system resource bundles,
         * a name is synthesized using a localized
         * <code>MessageFormat</code> pattern from the resource data.  The
         * arguments to this pattern are an integer followed by one or two
         * strings.  The integer is the number of strings, either 1 or 2.
         * The strings are formed by splitting the ID for this
         * transliterator at the first '-'.  If there is no '-', then the
         * entire ID forms the only string.
         * @param inLocale the ULocale in which the display name should be
         * localized.
         * @see java.text.MessageFormat
         * @stable ICU 3.2
         */
        public static string GetDisplayName(string id, ULocale inLocale)
        {

            // Resource bundle containing display name keys and the
            // RB_RULE_BASED_IDS array.
            //
            //If we ever integrate this with the Sun JDK, the resource bundle
            // root will change to sun.text.resources.LocaleElements

            ICUResourceBundle bundle = (ICUResourceBundle)UResourceBundle.
                // ICU4N specific - we need to pass the current assembly to load the resource data
                GetBundleInstance(ICUData.ICU_TRANSLIT_BASE_NAME, inLocale, ICU_DATA_CLASS_LOADER);

            // Normalize the ID
            string[] stv = TransliteratorIDParser.IDtoSTV(id);
            if (stv == null)
            {
                // No target; malformed id
                return "";
            }
            string ID = stv[0] + '-' + stv[1];
            if (stv[2] != null && stv[2].Length > 0)
            {
                ID = ID + '/' + stv[2];
            }

            // Use the registered display name, if any
            string n = displayNameCache.Get(new CaseInsensitiveString(ID));
            if (n != null)
            {
                return n;
            }

            // Use display name for the entire transliterator, if it
            // exists.
            try
            {
                return bundle.GetString(RB_DISPLAY_NAME_PREFIX + ID);
            }
            catch (MissingManifestResourceException e) { }

            try
            {
                //// Construct the formatter first; if getString() fails
                //// we'll exit the try block
                //MessageFormat format = new MessageFormat(
                //        bundle.GetString(RB_DISPLAY_NAME_PATTERN));
                // Construct the argument array
                object[] args = new object[] { 2, stv[0], stv[1] };

                // Use display names for the scripts, if they exist
                for (int j = 1; j <= 2; ++j)
                {
                    try
                    {
                        args[j] = bundle.GetString(RB_SCRIPT_DISPLAY_NAME_PREFIX +
                                                   (string)args[j]);
                    }
                    catch (MissingManifestResourceException e) { }
                }

                // Format it using the pattern in the resource
                //return (stv[2].length() > 0) ?
                //    (format.format(args) + '/' + stv[2]) :
                //    format.format(args);

                string pattern = bundle.GetString(RB_DISPLAY_NAME_PATTERN);

                return (stv[2].Length > 0) ? string.Format(pattern, args) + '/' + stv[2] : string.Format(pattern, args);
            }
            catch (MissingManifestResourceException e2) { }

            // We should not reach this point unless there is something
            // wrong with the build or the RB_DISPLAY_NAME_PATTERN has
            // been deleted from the root RB_LOCALE_ELEMENTS resource.
            throw new Exception();
        }

        /**
         * Returns the filter used by this transliterator, or <tt>null</tt>
         * if this transliterator uses no filter.
         * @stable ICU 2.0
         */
        public UnicodeFilter Filter
        {
            get { return filter; }
            set
            {
                if (value == null)
                {
                    this.filter = null;
                }
                else
                {
                    try
                    {
                        // fast high-runner case
                        this.filter = new UnicodeSet((UnicodeSet)value).Freeze();
                    }
                    catch (Exception)
                    {
                        this.filter = new UnicodeSet();
                        value.AddMatchSetTo(this.filter);
                        this.filter.Freeze();
                    }
                }
            }
        }

        ///**
        // * Changes the filter used by this transliterator.  If the filter
        // * is set to <tt>null</tt> then no filtering will occur.
        // *
        // * <p>Callers must take care if a transliterator is in use by
        // * multiple threads.  The filter should not be changed by one
        // * thread while another thread may be transliterating.
        // * @stable ICU 2.0
        // */
        //public void setFilter(UnicodeFilter filter)
        //{
        //    if (filter == null)
        //    {
        //        this.filter = null;
        //    }
        //    else
        //    {
        //        try
        //        {
        //            // fast high-runner case
        //            this.filter = new UnicodeSet((UnicodeSet)filter).freeze();
        //        }
        //        catch (Exception e)
        //        {
        //            this.filter = new UnicodeSet();
        //            filter.addMatchSetTo(this.filter);
        //            this.filter.freeze();
        //        }
        //    }
        //}

        /**
         * Returns a <code>Transliterator</code> object given its ID.
         * The ID must be either a system transliterator ID or a ID registered
         * using <code>registerClass()</code>.
         *
         * @param ID a valid ID, as enumerated by <code>getAvailableIDs()</code>
         * @return A <code>Transliterator</code> object with the given ID
         * @exception IllegalArgumentException if the given ID is invalid.
         * @stable ICU 2.0
         */
        public static Transliterator GetInstance(string id)
        {
            return GetInstance(id, FORWARD);
        }

        /**
         * Returns a <code>Transliterator</code> object given its ID.
         * The ID must be either a system transliterator ID or a ID registered
         * using <code>registerClass()</code>.
         *
         * @param ID a valid ID, as enumerated by <code>getAvailableIDs()</code>
         * @param dir either FORWARD or REVERSE.  If REVERSE then the
         * inverse of the given ID is instantiated.
         * @return A <code>Transliterator</code> object with the given ID
         * @exception IllegalArgumentException if the given ID is invalid.
         * @see #registerClass
         * @see #getAvailableIDs
         * @see #getID
         * @stable ICU 2.0
         */
        public static Transliterator GetInstance(string id,
                                                 int dir)
        {
            StringBuffer canonID = new StringBuffer();
            IList<SingleID> list = new List<SingleID>();
            UnicodeSet[] globalFilter = new UnicodeSet[1];
            if (!TransliteratorIDParser.ParseCompoundID(id, dir, canonID, list, globalFilter))
            {
                throw new ArgumentException("Invalid ID " + id);
            }

            IList<Transliterator> translits = TransliteratorIDParser.InstantiateList(list);

            // assert(list.size() > 0);
            Transliterator t = null;
            if (list.Count > 1 || canonID.IndexOf(";") >= 0)
            {
                // [NOTE: If it's a compoundID, we instantiate a CompoundTransliterator even if it only
                // has one child transliterator.  This is so that toRules() will return the right thing
                // (without any inactive ID), but our main ID still comes out correct.  That is, if we
                // instantiate "(Lower);Latin-Greek;", we want the rules to come out as "::Latin-Greek;"
                // even though the ID is "(Lower);Latin-Greek;".
                t = new CompoundTransliterator(translits);
            }
            else
            {
                t = translits[0];
            }

            t.ID = canonID.ToString();
            if (globalFilter[0] != null)
            {
                t.Filter = globalFilter[0];
            }
            return t;
        }

        /**
         * Create a transliterator from a basic ID.  This is an ID
         * containing only the forward direction source, target, and
         * variant.
         * @param id a basic ID of the form S-T or S-T/V.
         * @param canonID canonical ID to apply to the result, or
         * null to leave the ID unchanged
         * @return a newly created Transliterator or null if the ID is
         * invalid.
         */
        internal static Transliterator GetBasicInstance(string id, string canonID)
        {
            StringBuffer s = new StringBuffer();
            Transliterator t = registry.Get(id, s);
            if (s.Length != 0)
            {
                // assert(t==0);
                // Instantiate an alias
                t = GetInstance(s.ToString(), FORWARD);
            }
            if (t != null && canonID != null)
            {
                t.ID = canonID;
            }
            return t;
        }

        /**
         * Returns a <code>Transliterator</code> object constructed from
         * the given rule string.  This will be a RuleBasedTransliterator,
         * if the rule string contains only rules, or a
         * CompoundTransliterator, if it contains ID blocks, or a
         * NullTransliterator, if it contains ID blocks which parse as
         * empty for the given direction.
         * @stable ICU 2.0
         */
        public static Transliterator CreateFromRules(string id, string rules, int dir)
        {
            Transliterator t = null;

            TransliteratorParser parser = new TransliteratorParser();
            parser.Parse(rules, dir);

            // NOTE: The logic here matches that in TransliteratorRegistry.
            if (parser.IdBlockVector.Count == 0 && parser.DataVector.Count == 0)
            {
                t = new NullTransliterator();
            }
            else if (parser.IdBlockVector.Count == 0 && parser.DataVector.Count == 1)
            {
                t = new RuleBasedTransliterator(id, parser.DataVector[0], parser.CompoundFilter);
            }
            else if (parser.IdBlockVector.Count == 1 && parser.DataVector.Count == 0)
            {
                // idBlock, no data -- this is an alias.  The ID has
                // been munged from reverse into forward mode, if
                // necessary, so instantiate the ID in the forward
                // direction.
                if (parser.CompoundFilter != null)
                {
                    t = GetInstance(parser.CompoundFilter.ToPattern(false) + ";"
                            + parser.IdBlockVector[0]);
                }
                else
                {
                    t = GetInstance(parser.IdBlockVector[0]);
                }

                if (t != null)
                {
                    t.ID = id;
                }
            }
            else
            {
                IList<Transliterator> transliterators = new List<Transliterator>();
                int passNumber = 1;

                int limit = Math.Max(parser.IdBlockVector.Count, parser.DataVector.Count);
                for (int i = 0; i < limit; i++)
                {
                    if (i < parser.IdBlockVector.Count)
                    {
                        string idBlock = parser.IdBlockVector[i];
                        if (idBlock.Length > 0)
                        {
                            Transliterator temp = GetInstance(idBlock);
                            if (!(temp is NullTransliterator))
                                transliterators.Add(GetInstance(idBlock));
                        }
                    }
                    if (i < parser.DataVector.Count)
                    {
                        var data = parser.DataVector[i];
                        transliterators.Add(new RuleBasedTransliterator("%Pass" + passNumber++, data, null));
                    }
                }

                t = new CompoundTransliterator(transliterators, passNumber - 1);
                t.ID = id;
                if (parser.CompoundFilter != null)
                {
                    t.Filter = parser.CompoundFilter;
                }
            }

            return t;
        }

        /**
         * Returns a rule string for this transliterator.
         * @param escapeUnprintable if true, then unprintable characters
         * will be converted to escape form backslash-'u' or
         * backslash-'U'.
         * @stable ICU 2.0
         */
        public virtual string ToRules(bool escapeUnprintable)
        {
            return BaseToRules(escapeUnprintable);
        }

        /**
         * Returns a rule string for this transliterator.  This is
         * a non-overrideable base class implementation that subclasses
         * may call.  It simply munges the ID into the correct format,
         * that is, "foo" =&gt; "::foo".
         * @param escapeUnprintable if true, then unprintable characters
         * will be converted to escape form backslash-'u' or
         * backslash-'U'.
         * @stable ICU 2.0
         */
        protected internal string BaseToRules(bool escapeUnprintable)
        {
            // The base class implementation of toRules munges the ID into
            // the correct format.  That is: foo => ::foo
            // KEEP in sync with rbt_pars
            if (escapeUnprintable)
            {
                StringBuffer rulesSource = new StringBuffer();
                string id = ID;
                for (int i = 0; i < id.Length;)
                {
                    int c = UTF16.CharAt(id, i);
                    if (!Utility.EscapeUnprintable(rulesSource, c))
                    {
                        UTF16.Append(rulesSource, c);
                    }
                    i += UTF16.GetCharCount(c);
                }
                rulesSource.Insert(0, "::");
                rulesSource.Append(ID_DELIM);
                return rulesSource.ToString();
            }
            return "::" + ID + ID_DELIM;
        }

        /**
         * Return the elements that make up this transliterator.  For
         * example, if the transliterator "NFD;Jamo-Latin;Latin-Greek"
         * were created, the return value of this method would be an array
         * of the three transliterator objects that make up that
         * transliterator: [NFD, Jamo-Latin, Latin-Greek].
         *
         * <p>If this transliterator is not composed of other
         * transliterators, then this method will return an array of
         * length one containing a reference to this transliterator.
         * @return an array of one or more transliterators that make up
         * this transliterator
         * @stable ICU 3.0
         */
        public virtual Transliterator[] GetElements()
        {
            Transliterator[] result;
            if (this is CompoundTransliterator)
            {
                CompoundTransliterator cpd = (CompoundTransliterator)this;
                result = new Transliterator[cpd.Count];
                for (int i = 0; i < result.Length; ++i)
                {
                    result[i] = cpd.GetTransliterator(i); // ICU4N TODO: Indexer ?
                }
            }
            else
            {
                result = new Transliterator[] { this };
            }
            return result;
        }

        /**
         * Returns the set of all characters that may be modified in the
         * input text by this Transliterator.  This incorporates this
         * object's current filter; if the filter is changed, the return
         * value of this function will change.  The default implementation
         * returns an empty set.  Some subclasses may override {@link
         * #handleGetSourceSet} to return a more precise result.  The
         * return result is approximate in any case and is intended for
         * use by tests, tools, or utilities.
         * @see #getTargetSet
         * @see #handleGetSourceSet
         * @stable ICU 2.2
         */
        public UnicodeSet GetSourceSet()
        {
            UnicodeSet result = new UnicodeSet();
            AddSourceTargetSet(GetFilterAsUnicodeSet(UnicodeSet.ALL_CODE_POINTS), result, new UnicodeSet());
            return result;
        }

        /**
         * Framework method that returns the set of all characters that
         * may be modified in the input text by this Transliterator,
         * ignoring the effect of this object's filter.  The base class
         * implementation returns the empty set.  Subclasses that wish to
         * implement this should override this method.
         * @return the set of characters that this transliterator may
         * modify.  The set may be modified, so subclasses should return a
         * newly-created object.
         * @see #getSourceSet
         * @see #getTargetSet
         * @stable ICU 2.2
         */
        protected virtual UnicodeSet HandleGetSourceSet()
        {
            return new UnicodeSet();
        }

        /**
         * Returns the set of all characters that may be generated as
         * replacement text by this transliterator.  The default
         * implementation returns the empty set.  Some subclasses may
         * override this method to return a more precise result.  The
         * return result is approximate in any case and is intended for
         * use by tests, tools, or utilities requiring such
         * meta-information.
         * <p>Warning. You might expect an empty filter to always produce an empty target.
         * However, consider the following:
         * <pre>
         * [Pp]{}[\u03A3\u03C2\u03C3\u03F7\u03F8\u03FA\u03FB] &gt; \';
         * </pre>
         * With a filter of [], you still get some elements in the target set, because this rule will still match. It could
         * be recast to the following if it were important.
         * <pre>
         * [Pp]{([\u03A3\u03C2\u03C3\u03F7\u03F8\u03FA\u03FB])} &gt; \' | $1;
         * </pre>
         * @see #getTargetSet
         * @stable ICU 2.2
         */
        public virtual UnicodeSet GetTargetSet()
        {
            UnicodeSet result = new UnicodeSet();
            AddSourceTargetSet(GetFilterAsUnicodeSet(UnicodeSet.ALL_CODE_POINTS), new UnicodeSet(), result);
            return result;
        }

        /**
         * Returns the set of all characters that may be generated as
         * replacement text by this transliterator, filtered by BOTH the input filter, and the current getFilter().
         * <p>SHOULD BE OVERRIDEN BY SUBCLASSES.
         * It is probably an error for any transliterator to NOT override this, but we can't force them to
         * for backwards compatibility.
         * <p>Other methods vector through this.
         * <p>When gathering the information on source and target, the compound transliterator makes things complicated.
         * For example, suppose we have:
         * <pre>
         * Global FILTER = [ax]
         * a &gt; b;
         * :: NULL;
         * b &gt; c;
         * x &gt; d;
         * </pre>
         * While the filter just allows a and x, b is an intermediate result, which could produce c. So the source and target sets
         * cannot be gathered independently. What we have to do is filter the sources for the first transliterator according to
         * the global filter, intersect that transliterator's filter. Based on that we get the target.
         * The next transliterator gets as a global filter (global + last target). And so on.
         * <p>There is another complication:
         * <pre>
         * Global FILTER = [ax]
         * a &gt;|b;
         * b &gt;c;
         * </pre>
         * Even though b would be filtered from the input, whenever we have a backup, it could be part of the input. So ideally we will
         * change the global filter as we go.
         * @param targetSet TODO
         * @see #getTargetSet
         * @internal
         * @deprecated  This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public virtual void AddSourceTargetSet(UnicodeSet inputFilter, UnicodeSet sourceSet, UnicodeSet targetSet)
        {
            UnicodeSet myFilter = GetFilterAsUnicodeSet(inputFilter);
            UnicodeSet temp = new UnicodeSet(HandleGetSourceSet()).RetainAll(myFilter);
            // use old method, if we don't have anything better
            sourceSet.AddAll(temp);
            // clumsy guess with target
            foreach (string s in temp)
            {
                string t = Transliterate(s);
                if (!s.Equals(t))
                {
                    targetSet.AddAll(t);
                }
            }
        }

        /**
         * Returns the intersectionof this instance's filter intersected with an external filter.
         * The externalFilter must be frozen (it is frozen if not).
         * The result may be frozen, so don't attempt to modify.
         * @internal
         * @deprecated  This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        // TODO change to getMergedFilter
        public virtual UnicodeSet GetFilterAsUnicodeSet(UnicodeSet externalFilter)
        {
            if (filter == null)
            {
                return externalFilter;
            }
            UnicodeSet filterSet = new UnicodeSet(externalFilter);
            // Most, but not all filters will be UnicodeSets.  Optimize for
            // the high-runner case.
            UnicodeSet temp;
            try
            {
                temp = filter;
            }
            catch (InvalidCastException)
            {
                filter.AddMatchSetTo(temp = new UnicodeSet());
            }
            return filterSet.RetainAll(temp).Freeze();
        }

        /**
         * Returns this transliterator's inverse.  See the class
         * documentation for details.  This implementation simply inverts
         * the two entities in the ID and attempts to retrieve the
         * resulting transliterator.  That is, if <code>getID()</code>
         * returns "A-B", then this method will return the result of
         * <code>getInstance("B-A")</code>, or <code>null</code> if that
         * call fails.
         *
         * <p>Subclasses with knowledge of their inverse may wish to
         * override this method.
         *
         * @return a transliterator that is an inverse, not necessarily
         * exact, of this transliterator, or <code>null</code> if no such
         * transliterator is registered.
         * @see #registerClass
         * @stable ICU 2.0
         */
        public Transliterator GetInverse()
        {
            return GetInstance(id, REVERSE);
        }

        /**
         * Registers a subclass of <code>Transliterator</code> with the
         * system.  This subclass must have a public constructor taking no
         * arguments.  When that constructor is called, the resulting
         * object must return the <code>ID</code> passed to this method if
         * its <code>getID()</code> method is called.
         *
         * @param ID the result of <code>getID()</code> for this
         * transliterator
         * @param transClass a subclass of <code>Transliterator</code>
         * @see #unregister
         * @stable ICU 2.0
         */
        public static void RegisterClass(string id, Type transClass, string displayName) // ICU4N TODO: API - rename RegisterType
        {
            registry.Put(id, transClass, true); // ICU4N TODO: Indexer ?
            if (displayName != null)
            {
                displayNameCache[new CaseInsensitiveString(id)] = displayName;
            }
        }

        /**
         * Register a factory object with the given ID.  The factory
         * method should return a new instance of the given transliterator.
         *
         * <p>Because ICU may choose to cache Transliterator objects internally, this must
         * be called at application startup, prior to any calls to
         * Transliterator.getInstance to avoid undefined behavior.
         *
         * @param ID the ID of this transliterator
         * @param factory the factory object
         * @stable ICU 2.0
         */
        public static void RegisterFactory(string ID, IFactory factory)
        {
            registry.Put(ID, factory, true);
        }

        /**
         * Register a Transliterator object with the given ID.
         *
         * <p>Because ICU may choose to cache Transliterator objects internally, this must
         * be called at application startup, prior to any calls to
         * Transliterator.getInstance to avoid undefined behavior.
         *
         * @param trans the Transliterator object
         * @stable ICU 2.2
         */
        public static void RegisterInstance(Transliterator trans)
        {
            registry.Put(trans.ID, trans, true);
        }

        /**
         * Register a Transliterator object.
         *
         * <p>Because ICU may choose to cache Transliterator objects internally, this must
         * be called at application startup, prior to any calls to
         * Transliterator.getInstance to avoid undefined behavior.
         *
         * @param trans the Transliterator object
         */
        internal static void RegisterInstance(Transliterator trans, bool visible)
        {
            registry.Put(trans.ID, trans, visible);
        }

        /**
         * Register an ID as an alias of another ID.  Instantiating
         * alias ID produces the same result as instantiating the original ID.
         * This is generally used to create short aliases of compound IDs.
         *
         * <p>Because ICU may choose to cache Transliterator objects internally, this must
         * be called at application startup, prior to any calls to
         * Transliterator.getInstance to avoid undefined behavior.
         *
         * @param aliasID The new ID being registered.
         * @param realID The existing ID that the new ID should be an alias of.
         * @stable ICU 3.6
         */
        public static void RegisterAlias(string aliasID, string realID)
        {
            registry.Put(aliasID, realID, true);
        }

        /**
         * Register two targets as being inverses of one another.  For
         * example, calling registerSpecialInverse("NFC", "NFD", true) causes
         * Transliterator to form the following inverse relationships:
         *
         * <pre>NFC =&gt; NFD
         * Any-NFC =&gt; Any-NFD
         * NFD =&gt; NFC
         * Any-NFD =&gt; Any-NFC</pre>
         *
         * (Without the special inverse registration, the inverse of NFC
         * would be NFC-Any.)  Note that NFD is shorthand for Any-NFD, but
         * that the presence or absence of "Any-" is preserved.
         *
         * <p>The relationship is symmetrical; registering (a, b) is
         * equivalent to registering (b, a).
         *
         * <p>The relevant IDs must still be registered separately as
         * factories or classes.
         *
         * <p>Only the targets are specified.  Special inverses always
         * have the form Any-Target1 &lt;=&gt; Any-Target2.  The target should
         * have canonical casing (the casing desired to be produced when
         * an inverse is formed) and should contain no whitespace or other
         * extraneous characters.
         *
         * @param target the target against which to register the inverse
         * @param inverseTarget the inverse of target, that is
         * Any-target.getInverse() =&gt; Any-inverseTarget
         * @param bidirectional if true, register the reverse relation
         * as well, that is, Any-inverseTarget.getInverse() =&gt; Any-target
         */
        internal static void RegisterSpecialInverse(string target,
                                           string inverseTarget,
                                           bool bidirectional)
        {
            TransliteratorIDParser.RegisterSpecialInverse(target, inverseTarget, bidirectional);
        }

        /**
         * Unregisters a transliterator or class.  This may be either
         * a system transliterator or a user transliterator or class.
         *
         * @param ID the ID of the transliterator or class
         * @see #registerClass
         * @stable ICU 2.0
         */
        public static void Unregister(String ID)
        {
            displayNameCache.Remove(new CaseInsensitiveString(ID));
            registry.Remove(ID);
        }

        /**
         * Returns an enumeration over the programmatic names of registered
         * <code>Transliterator</code> objects.  This includes both system
         * transliterators and user transliterators registered using
         * <code>registerClass()</code>.  The enumerated names may be
         * passed to <code>getInstance()</code>.
         *
         * @return An <code>Enumeration</code> over <code>String</code> objects
         * @see #getInstance
         * @see #registerClass
         * @stable ICU 2.0
         */
        public static IEnumerable<string> GetAvailableIDs() // ICU4N TODO: API - property ?
        {
            return registry.GetAvailableIDs();
        }

        /**
         * Returns an enumeration over the source names of registered
         * transliterators.  Source names may be passed to
         * getAvailableTargets() to obtain available targets for each
         * source.
         * @stable ICU 2.0
         */
        public static IEnumerable<string> GetAvailableSources() // ICU4N TODO: API - property ?
        {
            return registry.GetAvailableSources();
        }

        /**
         * Returns an enumeration over the target names of registered
         * transliterators having a given source name.  Target names may
         * be passed to getAvailableVariants() to obtain available
         * variants for each source and target pair.
         * @stable ICU 2.0
         */
        public static IEnumerable<string> GetAvailableTargets(string source)
        {
            return registry.GetAvailableTargets(source);
        }

        /**
         * Returns an enumeration over the variant names of registered
         * transliterators having a given source name and target name.
         * @stable ICU 2.0
         */
        public static IEnumerable<string> GetAvailableVariants(string source,
                                                             string target)
        {
            return registry.GetAvailableVariants(source, target);
        }
        private static readonly string ROOT = "root",
                                        RB_RULE_BASED_IDS = "RuleBasedTransliteratorIDs";
        static Transliterator()
        {
            registry = new TransliteratorRegistry();

            // The display name cache starts out empty
            displayNameCache = new ConcurrentDictionary<CaseInsensitiveString, string>();
            /* The following code parses the index table located in
             * icu/data/translit/root.txt.  The index is an n x 4 table
             * that follows this format:
             *  <id>{
             *      file{
             *          resource{"<resource>"}
             *          direction{"<direction>"}
             *      }
             *  }
             *  <id>{
             *      internal{
             *          resource{"<resource>"}
             *          direction{"<direction"}
             *       }
             *  }
             *  <id>{
             *      alias{"<getInstanceArg"}
             *  }
             * <id> is the ID of the system transliterator being defined.  These
             * are public IDs enumerated by Transliterator.getAvailableIDs(),
             * unless the second field is "internal".
             *
             * <resource> is a ResourceReader resource name.  Currently these refer
             * to file names under com/ibm/text/resources.  This string is passed
             * directly to ResourceReader, together with <encoding>.
             *
             * <direction> is either "FORWARD" or "REVERSE".
             *
             * <getInstanceArg> is a string to be passed directly to
             * Transliterator.getInstance().  The returned Transliterator object
             * then has its ID changed to <id> and is returned.
             *
             * The extra blank field on "alias" lines is to make the array square.
             */
            UResourceBundle bundle, transIDs, colBund;
            bundle = UResourceBundle.GetBundleInstance(ICUData.ICU_TRANSLIT_BASE_NAME, ROOT);
            transIDs = bundle.Get(RB_RULE_BASED_IDS);

            int row, maxRows;
            maxRows = transIDs.Length;
            for (row = 0; row < maxRows; row++)
            {
                colBund = transIDs.Get(row);
                string ID = colBund.Key;
                if (ID.IndexOf("-t-") >= 0)
                {
                    continue;
                }
                UResourceBundle res = colBund.Get(0);
                string type = res.Key;
                if (type.Equals("file") || type.Equals("internal"))
                {
                    // Rest of line is <resource>:<encoding>:<direction>
                    //                pos       colon      c2
                    string resString = res.GetString("resource");
                    int dir;
                    string direction = res.GetString("direction");
                    switch (direction[0])
                    {
                        case 'F':
                            dir = FORWARD;
                            break;
                        case 'R':
                            dir = REVERSE;
                            break;
                        default:
                            throw new Exception("Can't parse direction: " + direction);
                    }
                    registry.Put(ID,
                                 resString, // resource
                                 dir,
                                 !type.Equals("internal"));
                }
                else if (type.Equals("alias"))
                {
                    //'alias'; row[2]=createInstance argument
                    string resString = res.GetString();
                    registry.Put(ID, resString, true);
                }
                else
                {
                    // Unknown type
                    throw new Exception("Unknow type: " + type);
                }
            }

            RegisterSpecialInverse(NullTransliterator.SHORT_ID, NullTransliterator.SHORT_ID, false);

            // Register non-rule-based transliterators
            RegisterClass(NullTransliterator._ID,
                          typeof(NullTransliterator), null);
            RemoveTransliterator.Register();
            EscapeTransliterator.Register();
            UnescapeTransliterator.Register();
            LowercaseTransliterator.Register();
            UppercaseTransliterator.Register();
            TitlecaseTransliterator.Register();
            CaseFoldTransliterator.Register();
            UnicodeNameTransliterator.Register();
            NameUnicodeTransliterator.Register();
            NormalizationTransliterator.Register();
            BreakTransliterator.Register();
            AnyTransliterator.Register(); // do this last!
        }

        /**
         * Register the script-based "Any" transliterators: Any-Latin, Any-Greek
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public static void RegisterAny()
        {
            AnyTransliterator.Register();
        }

        // ICU4N specific class to make anonymous transforms
        internal class StringTransform : IStringTransform
        {
            private readonly Func<string, string> transform;

            public StringTransform(Func<string, string> transform)
            {
                if (transform == null)
                    throw new ArgumentNullException(nameof(transform));
                this.transform = transform;
            }

            public string Transform(string source)
            {
                return transform(source);
            }
        }

        // ICU4N specific class to make anonymous factories 
        internal class Factory : IFactory
        {
            private readonly Func<string, Transliterator> getInstance;

            public Factory(Func<string, Transliterator> getInstance)
            {
                if (getInstance == null)
                    throw new ArgumentNullException(nameof(getInstance));
                this.getInstance = getInstance;
            }

            public Transliterator GetInstance(string id)
            {
                return this.getInstance(id);
            }
        }

        /**
         * The factory interface for transliterators.  Transliterator
         * subclasses can register factory objects for IDs using the
         * registerFactory() method of Transliterator.  When invoked, the
         * factory object will be passed the ID being instantiated.  This
         * makes it possible to register one factory method to more than
         * one ID, or for a factory method to parameterize its result
         * based on the variant.
         * @stable ICU 2.0
         */
        public interface IFactory
        {
            /**
             * Return a transliterator for the given id.
             * @stable ICU 2.0
             */
            Transliterator GetInstance(string id);
        }

        /**
         * Implements StringTransform via this method.
         * @param source text to be transformed (eg lowercased)
         * @return result
         * @stable ICU 3.8
         */
        public virtual string Transform(string source)
        {
            return Transliterate(source);
        }
    }
}
