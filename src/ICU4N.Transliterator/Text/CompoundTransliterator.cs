using ICU4N.Impl;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    /// <summary>
    /// A transliterator that is composed of two or more other
    /// transliterator objects linked together.  For example, if one
    /// transliterator transliterates from script A to script B, and
    /// another transliterates from script B to script C, the two may be
    /// combined to form a new transliterator from A to C.
    /// </summary>
    /// <remarks>
    /// Composed transliterators may not behave as expected.  For
    /// example, inverses may not combine to form the identity
    /// transliterator.  See the class documentation for 
    /// <see cref="Transliterator"/> for details.
    /// <para/>
    /// Copyright &#169; IBM Corporation 1999.  All rights reserved.
    /// </remarks>
    /// <author>Alan Liu</author>
    internal class CompoundTransliterator : Transliterator
    {
        private Transliterator[] trans;

        private int numAnonymousRBTs = 0;

        ///// <summary>
        ///// Constructs a new compound transliterator given an array of
        ///// transliterators.  The array of transliterators may be of any
        ///// length, including zero or one, however, useful compound
        ///// transliterators have at least two components.
        ///// </summary>
        ///// <param name="transliterators">Array of <see cref="Transliterator"/> objects.</param>
        ///// <param name="filter">the filter.  Any character for which
        ///// <c>filter.Contains()</c> returns <c>false</c> will not be
        ///// altered by this transliterator.  If <c>filter</c> is
        ///// <c>null</c> then no filtering is applied.
        ///// </param>
        //public CompoundTransliterator(Transliterator[] transliterators,
        //                              UnicodeFilter filter)
        //    : base(JoinIDs(transliterators), filter)
        //{
        //    trans = new Transliterator[transliterators.Length];
        //    System.Array.Copy(transliterators, 0, trans, 0, trans.Length);
        //    ComputeMaximumContextLength();
        //}

        ///// <summary>
        ///// Constructs a new compound transliterator given an array of
        ///// transliterators.  The array of transliterators may be of any
        ///// length, including zero or one, however, useful compound
        ///// transliterators have at least two components.
        ///// </summary>
        ///// <param name="transliterators">Array of <see cref="Transliterator"/> objects.</param>
        //public CompoundTransliterator(Transliterator[] transliterators)
        //    : this(transliterators, null)
        //{
        //}

        ///// <summary>
        ///// Constructs a new compound transliterator.
        ///// </summary>
        ///// <param name="ID">compound ID</param>
        ///// <param name="direction">Either <see cref="TransliterationDirection.Forward"/> or <see cref="TransliterationDirection.Reverse"/>.</param>
        ///// <param name="filter">A global filter for this compound transliterator.</param>
        //public CompoundTransliterator(string ID, TransliterationDirection direction,
        //                              UnicodeFilter filter)
        //    : base(ID, filter)
        //{
        //    Init(ID, direction, true);
        //}

        ///// <summary>
        ///// Constructs a new compound transliterator with no filter.
        ///// </summary>
        ///// <param name="ID">compound ID</param>
        ///// <param name="direction">Either <see cref="TransliterationDirection.Forward"/> or <see cref="TransliterationDirection.Reverse"/>.</param>
        //public CompoundTransliterator(string ID, TransliterationDirection direction)
        //    : this(ID, direction, null)
        //{
        //}

        ///// <summary>
        ///// Constructs a new forward compound transliterator with no filter.
        ///// </summary>
        ///// <param name="ID">compound ID</param>
        //public CompoundTransliterator(string ID)
        //    : this(ID, TransliterationDirection.Forward, null)
        //{
        //}

        /// <summary>
        /// Internal constructor for Transliterator from a vector of
        /// transliterators.  The caller is responsible for fixing up the
        /// ID.
        /// </summary>
        /// <param name="list"></param>
        internal CompoundTransliterator(IList<Transliterator> list)
            : this(list, 0)
        {
        }

        internal CompoundTransliterator(IList<Transliterator> list, int numAnonymousRBTs)
            : base("", null)
        {
            trans = null;
            Init(list, Forward, false);
            this.numAnonymousRBTs = numAnonymousRBTs;
            // assume caller will fixup ID
        }

        /// <summary>
        /// Internal method for SafeClone...
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filter2"></param>
        /// <param name="trans2"></param>
        /// <param name="numAnonymousRBTs2"></param>
        internal CompoundTransliterator(string id, UnicodeFilter filter2, Transliterator[] trans2, int numAnonymousRBTs2)
            : base(id, filter2)
        {
            trans = trans2;
            numAnonymousRBTs = numAnonymousRBTs2;
        }

        ///// <summary>
        ///// Finish constructing a transliterator: only to be called by
        ///// constructors.  Before calling Init(), set trans and filter to NULL.
        ///// </summary>
        ///// <param name="id">The id containing ';'-separated entries.</param>
        ///// <param name="direction">Either <see cref="TransliterationDirection.Forward"/> or <see cref="TransliterationDirection.Reverse"/>.</param>
        ///// <param name="fixReverseID">if TRUE, then reconstruct the ID of reverse
        ///// entries by calling ID of component entries.  Some constructors
        ///// do not require this because they apply a facade ID anyway.</param>
        //private void Init(string id,
        //                  TransliterationDirection direction,
        //                  bool fixReverseID) {
        //    // Debug.Assert(trans == 0);

        //    var list = new List<TransliteratorIDParser.SingleID>();
        //    UnicodeSet[] compoundFilter = new UnicodeSet[1];
        //    StringBuffer regenID = new StringBuffer();
        //    if (!TransliteratorIDParser.ParseCompoundID(id, direction,
        //             regenID, list, compoundFilter)) {
        //        throw new ArgumentException("Invalid ID " + id);
        //    }

        //    TransliteratorIDParser.InstantiateList(list);

        //    Init(list, direction, fixReverseID);

        //    if (compoundFilter[0] != null) {
        //        Filter = compoundFilter[0];
        //    }
        //}

        /// <summary>
        /// Finish constructing a transliterator: only to be called by
        /// constructors.  Before calling Init(), set trans and filter to NULL.
        /// </summary>
        /// <param name="list">A vector of transliterator objects to be adopted.  It
        /// should NOT be empty.  The list should be in declared order.  That
        /// is, it should be in the FORWARD order; if direction is REVERSE then
        /// the list order will be reversed.</param>
        /// <param name="direction">Either FORWARD or REVERSE.</param>
        /// <param name="fixReverseID">If TRUE, then reconstruct the ID of reverse
        /// entries by calling <see cref="Transliterator.ID"/> of component entries.  Some constructors
        /// do not require this because they apply a facade ID anyway.
        /// </param>
        private void Init(IList<Transliterator> list,
                          TransliterationDirection direction,
                          bool fixReverseID)
        {
            // assert(trans == 0);

            // Allocate array
            int count = list.Count;
            trans = new Transliterator[count];

            // Move the transliterators from the vector into an array.
            // Reverse the order if necessary.
            int i;
            for (i = 0; i < count; ++i)
            {
                int j = (direction == Forward) ? i : count - 1 - i;
                trans[i] = list[j];
            }

            // If the direction is UTRANS_REVERSE then we may need to fix the
            // ID.
            if (direction == Reverse && fixReverseID)
            {
                ValueStringBuilder newID = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
                try
                {
                    for (i = 0; i < count; ++i)
                    {
                        if (i > 0)
                        {
                            newID.Append(ID_DELIM);
                        }
                        newID.Append(trans[i].ID);
                    }
                    ID = newID.ToString();
                }
                finally
                {
                    newID.Dispose();
                }
            }

            ComputeMaximumContextLength();
        }

        ///// <summary>
        ///// Return the IDs of the given list of transliterators, concatenated
        ///// with ';' delimiting them.  Equivalent to the perlish expression
        ///// join(';', map($_.getID(), transliterators).
        ///// </summary>
        //private static string JoinIDs(Transliterator[] transliterators)
        //{
        //    StringBuffer id = new StringBuffer();
        //    for (int i = 0; i < transliterators.Length; ++i)
        //    {
        //        if (i > 0)
        //        {
        //            id.Append(';');
        //        }
        //        id.Append(transliterators[i].ID);
        //    }
        //    return id.ToString();
        //}

        /// <summary>
        /// Gets the number of transliterators in this chain.
        /// </summary>
        public virtual int Count => trans.Length;


        /// <summary>
        /// Returns the transliterator at the given index in this chain.
        /// </summary>
        /// <param name="index">Index into chain, from 0 to <c>Count - 1</c>.</param>
        /// <returns>Transliterator at the given index.</returns>
        public virtual Transliterator GetTransliterator(int index)
        {
            return trans[index];
        }

        /// <summary>
        /// Append <paramref name="c"/> to <paramref name="buf"/>, unless <paramref name="buf"/> 
        /// is empty or buf already ends in <paramref name="c"/>.
        /// </summary>
        private static void SmartAppend(ref ValueStringBuilder buf, char c)
        {
            if (buf.Length != 0 &&
                buf[buf.Length - 1] != c)
            {
                buf.Append(c);
            }
        }

#nullable enable

        /// <summary>
        /// Override Transliterator:
        /// Create a rule string that can be passed to <see cref="Transliterator.CreateFromRules(string, string, TransliterationDirection)"/>
        /// to recreate this transliterator.
        /// </summary>
        /// <param name="escapeUnprintable">If TRUE then convert unprintable
        /// character to their hex escape representations, \\uxxxx or
        /// \\Uxxxxxxxx.  Unprintable characters are those other than
        /// U+000A, U+0020..U+007E.</param>
        /// <returns>The rule string.</returns>
        public override string ToRules(bool escapeUnprintable)
        {
            var sb = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                ToRules(escapeUnprintable, ref sb);
                return sb.ToString();
            }
            finally
            {
                sb.Dispose();
            }
        }

        /// <summary>
        /// Override Transliterator:
        /// Create a rule string that can be passed to <see cref="Transliterator.CreateFromRules(string, string, TransliterationDirection)"/>
        /// to recreate this transliterator.
        /// </summary>
        /// <param name="escapeUnprintable">If TRUE then convert unprintable
        /// character to their hex escape representations, \\uxxxx or
        /// \\Uxxxxxxxx.  Unprintable characters are those other than
        /// U+000A, U+0020..U+007E.</param>
        /// <param name="destination">Upon return from this method, will contain the rule string.</param>
        internal override void ToRules(bool escapeUnprintable, ref ValueStringBuilder destination)
        {
            // We do NOT call toRules() on our component transliterators, in
            // general.  If we have several rule-based transliterators, this
            // yields a concatenation of the rules -- not what we want.  We do
            // handle compound RBT transliterators specially -- those for which
            // compoundRBTIndex >= 0.  For the transliterator at compoundRBTIndex,
            // we do call toRules() recursively.
            if (numAnonymousRBTs >= 1 && Filter != null)
            {
                // If we are a compound RBT and if we have a global
                // filter, then emit it at the top.
                destination.Append("::");
                char[]? matcherPatternArray = null;
                try
                {
                    Span<char> matcherPattern = stackalloc char[CharStackBufferSize];
                    if (!Filter.TryToPattern(escapeUnprintable, matcherPattern, out int matcherPatternLength))
                    {
                        // Not enough buffer, use the array pool
                        matcherPattern = matcherPatternArray = ArrayPool<char>.Shared.Rent(matcherPatternLength);
                        bool success = Filter.TryToPattern(escapeUnprintable, matcherPattern, out matcherPatternLength);
                        Debug.Assert(success); // Unexpected
                    }
                    destination.Append(matcherPattern.Slice(0, matcherPatternLength));
                }
                finally
                {
                    ArrayPool<char>.Shared.ReturnIfNotNull(matcherPatternArray);
                }
                destination.Append(ID_DELIM);
            }
            for (int i = 0; i < trans.Length; ++i)
            {
                SmartAppend(ref destination, '\n');

                // Anonymous RuleBasedTransliterators (inline rules and
                // ::BEGIN/::END blocks) are given IDs that begin with
                // "%Pass": use toRules() to write all the rules to the output
                // (and insert "::Null;" if we have two in a row)
                if (trans[i].ID.StartsWith("%Pass", StringComparison.Ordinal))
                {
                    if (numAnonymousRBTs > 1 && i > 0 && trans[i - 1].ID.StartsWith("%Pass", StringComparison.Ordinal))
                        destination.Append("::Null;");
                    trans[i].ToRules(escapeUnprintable, ref destination);

                    // we also use toRules() on CompoundTransliterators (which we
                    // check for by looking for a semicolon in the ID)-- this gets
                    // the list of their child transliterators output in the right
                    // format
                }
                else if (trans[i].ID.IndexOf(';') >= 0)
                {
                    trans[i].ToRules(escapeUnprintable, ref destination);
                }
                else // for everything else, use baseToRules()
                {
                    trans[i].BaseToRules(escapeUnprintable, ref destination);
                }
                
                SmartAppend(ref destination, ID_DELIM);
            }
        }

#nullable restore

        /// <seealso cref="Transliterator.AddSourceTargetSet(UnicodeSet, UnicodeSet, UnicodeSet)"/>
#pragma warning disable 672
        public override void AddSourceTargetSet(UnicodeSet filter, UnicodeSet sourceSet, UnicodeSet targetSet)
#pragma warning restore 672
        {
#pragma warning disable 612, 618
            UnicodeSet myFilter = new UnicodeSet(GetFilterAsUnicodeSet(filter));
#pragma warning restore 612, 618
            UnicodeSet tempTargetSet = new UnicodeSet();
            for (int i = 0; i < trans.Length; ++i)
            {
                // each time we produce targets, those can be used by subsequent items, despite the filter.
                // so we get just those items, and add them to the filter each time.
                tempTargetSet.Clear();
#pragma warning disable 612, 618
                trans[i].AddSourceTargetSet(myFilter, sourceSet, tempTargetSet);
#pragma warning restore 612, 618
                targetSet.AddAll(tempTargetSet);
                myFilter.AddAll(tempTargetSet);
            }
        }

        //    /**
        //     * Returns the set of all characters that may be generated as
        //     * replacement text by this transliterator.
        //     */
        //    public UnicodeSet getTargetSet() {
        //        UnicodeSet set = new UnicodeSet();
        //        for (int i=0; i<trans.length; ++i) {
        //            // This is a heuristic, and not 100% reliable.
        //            set.addAll(trans[i].getTargetSet());
        //        }
        //        return set;
        //    }

        /// <summary>
        /// Implements <see cref="Transliterator.HandleTransliterate(IReplaceable, TransliterationPosition, bool)"/>.
        /// </summary>
        protected override void HandleTransliterate(IReplaceable text,
                                           TransliterationPosition index, bool incremental)
        {
            /* Call each transliterator with the same start value and
             * initial cursor index, but with the limit index as modified
             * by preceding transliterators.  The cursor index must be
             * reset for each transliterator to give each a chance to
             * transliterate the text.  The initial cursor index is known
             * to still point to the same place after each transliterator
             * is called because each transliterator will not change the
             * text between start and the initial value of cursor.
             *
             * IMPORTANT: After the first transliterator, each subsequent
             * transliterator only gets to transliterate text committed by
             * preceding transliterators; that is, the cursor (output
             * value) of transliterator i becomes the limit (input value)
             * of transliterator i+1.  Finally, the overall limit is fixed
             * up before we return.
             *
             * Assumptions we make here:
             * (1) contextStart <= start <= limit <= contextLimit <= text.length()
             * (2) start <= start' <= limit'  ;cursor doesn't move back
             * (3) start <= limit'            ;text before cursor unchanged
             * - start' is the value of start after calling handleKT
             * - limit' is the value of limit after calling handleKT
             */

            /*
             * Example: 3 transliterators.  This example illustrates the
             * mechanics we need to implement.  C, S, and L are the contextStart,
             * start, and limit.  gl is the globalLimit.  contextLimit is
             * equal to limit throughout.
             *
             * 1. h-u, changes hex to Unicode
             *
             *    4  7  a  d  0      4  7  a
             *    abc/u0061/u    =>  abca/u
             *    C  S       L       C   S L   gl=f->a
             *
             * 2. upup, changes "x" to "XX"
             *
             *    4  7  a       4  7  a
             *    abca/u    =>  abcAA/u
             *    C  SL         C    S
             *                       L    gl=a->b
             * 3. u-h, changes Unicode to hex
             *
             *    4  7  a        4  7  a  d  0  3
             *    abcAA/u    =>  abc/u0041/u0041/u
             *    C  S L         C              S
             *                                  L   gl=b->15
             * 4. return
             *
             *    4  7  a  d  0  3
             *    abc/u0041/u0041/u
             *    C S L
             */

            if (trans.Length < 1)
            {
                index.Start = index.Limit;
                return; // Short circuit for empty compound transliterators
            }

            // compoundLimit is the limit value for the entire compound
            // operation.  We overwrite index.limit with the previous
            // index.start.  After each transliteration, we update
            // compoundLimit for insertions or deletions that have happened.
            int compoundLimit = index.Limit;

            // compoundStart is the start for the entire compound
            // operation.
            int compoundStart = index.Start;

            int delta = 0; // delta in length

            StringBuffer log = null;
            ////CLOVER:OFF
            if (DEBUG)
            {
                log = new StringBuffer("CompoundTransliterator{" + ID +
                                       (incremental ? "}i: IN=" : "}: IN="));
                UtilityExtensions.FormatInput(log, text, index);
                Console.Out.WriteLine(Utility.Escape(log.ToString()));
            }
            ////CLOVER:ON

            // Give each transliterator a crack at the run of characters.
            // See comments at the top of the method for more detail.
            for (int i = 0; i < trans.Length; ++i)
            {
                index.Start = compoundStart; // Reset start
                int limit = index.Limit;

                if (index.Start == index.Limit)
                {
                    // Short circuit for empty range
                    ////CLOVER:OFF
                    if (DEBUG)
                    {
                        Console.Out.WriteLine("CompoundTransliterator[" + i +
                                           ".." + (trans.Length - 1) +
                                           (incremental ? "]i: " : "]: ") +
                                           UtilityExtensions.FormatInput(text, index) +
                                           " (NOTHING TO DO)");
                    }
                    ////CLOVER:ON
                    break;
                }

                ////CLOVER:OFF
                if (DEBUG)
                {
                    log.Length = 0;
                    log.Append("CompoundTransliterator[" + i + "=" +
                               trans[i].ID +
                               (incremental ? "]i: " : "]: "));
                    UtilityExtensions.FormatInput(log, text, index);
                }
                ////CLOVER:ON

                trans[i].FilteredTransliterate(text, index, incremental);

                // In a properly written transliterator, start == limit after
                // handleTransliterate() returns when incremental is false.
                // Catch cases where the subclass doesn't do this, and throw
                // an exception.  (Just pinning start to limit is a bad idea,
                // because what's probably happening is that the subclass
                // isn't transliterating all the way to the end, and it should
                // in non-incremental mode.)
                if (!incremental && index.Start != index.Limit)
                {
                    throw new Exception("ERROR: Incomplete non-incremental transliteration by " + trans[i].ID);
                }

                ////CLOVER:OFF
                if (DEBUG)
                {
                    log.Append(" => ");
                    UtilityExtensions.FormatInput(log, text, index);
                    Console.Out.WriteLine(Utility.Escape(log.ToString()));
                }
                ////CLOVER:ON

                // Cumulative delta for insertions/deletions
                delta += index.Limit - limit;

                if (incremental)
                {
                    // In the incremental case, only allow subsequent
                    // transliterators to modify what has already been
                    // completely processed by prior transliterators.  In the
                    // non-incrmental case, allow each transliterator to
                    // process the entire text.
                    index.Limit = index.Start;
                }
            }

            compoundLimit += delta;

            // Start is good where it is -- where the last transliterator left
            // it.  Limit needs to be put back where it was, modulo
            // adjustments for deletions/insertions.
            index.Limit = compoundLimit;

            ////CLOVER:OFF
            if (DEBUG)
            {
                log.Length = 0;
                log.Append("CompoundTransliterator{" + ID +
                           (incremental ? "}i: OUT=" : "}: OUT="));
                UtilityExtensions.FormatInput(log, text, index);
                Console.Out.WriteLine(Utility.Escape(log.ToString()));
            }
            ////CLOVER:ON
        }

        /// <summary>
        /// Compute and set the length of the longest context required by this transliterator.
        /// This is <em>preceding</em> context.
        /// </summary>
        private void ComputeMaximumContextLength()
        {
            int max = 0;
            for (int i = 0; i < trans.Length; ++i)
            {
                int len = trans[i].MaximumContextLength;
                if (len > max)
                {
                    max = len;
                }
            }
            MaximumContextLength = max;
        }

        /// <summary>
        /// Temporary hack for registry problem. Needs to be replaced by better architecture.
        /// </summary>
        public virtual Transliterator SafeClone()
        {
            UnicodeFilter filter = Filter;
            if (filter != null && filter is UnicodeSet)
            {
                filter = new UnicodeSet((UnicodeSet)filter);
            }
            return new CompoundTransliterator(ID, filter, trans, numAnonymousRBTs);
        }
    }
}
