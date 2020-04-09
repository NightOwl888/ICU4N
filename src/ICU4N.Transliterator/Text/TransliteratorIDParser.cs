using ICU4N.Impl;
using ICU4N.Support.Collections;
using ICU4N.Util;
using J2N.Text;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using J2N.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    /// <summary>
    /// Parsing component for transliterator IDs.  This class contains only
    /// static members; it cannot be instantiated.  Methods in this class
    /// parse various ID formats, including the following:
    /// <para/>
    /// A basic ID, which contains source, target, and variant, but no
    /// filter and no explicit inverse.  Examples include
    /// "Latin-Greek/UNGEGN" and "Null".
    /// <para/>
    /// A single ID, which is a basic ID plus optional filter and optional
    /// explicit inverse.  Examples include "[a-zA-Z] Latin-Greek" and
    /// "Lower (Upper)".
    /// <para/>
    /// A compound ID, which is a sequence of one or more single IDs,
    /// separated by semicolons, with optional forward and reverse global
    /// filters.  The global filters are <see cref="UnicodeSet"/> patterns prepended or
    /// appended to the IDs, separated by semicolons.  An appended filter
    /// must be enclosed in parentheses and applies in the reverse
    /// direction.
    /// </summary>
    /// <author>Alan Liu</author>
    internal class TransliteratorIDParser
    {
        private const char ID_DELIM = ';';

        private const char TARGET_SEP = '-';

        private const char VARIANT_SEP = '/';

        private const char OPEN_REV = '(';

        private const char CLOSE_REV = ')';

        private const string ANY = "Any";

        private const TransliterationDirection Forward = TransliterationDirection.Forward;

        private const TransliterationDirection Reverse = TransliterationDirection.Reverse;

        private static readonly IDictionary<CaseInsensitiveString, string> SPECIAL_INVERSES =
            new ConcurrentDictionary<CaseInsensitiveString, string>();

        /// <summary>
        /// A structure containing the parsed data of a filtered ID, that
        /// is, a basic ID optionally with a filter.
        /// <para/>
        /// <see cref="Source"/> and <see cref="Target"/> will always be non-null.  The 
        /// <see cref="Variant"/> will be non-null only if a non-empty variant was parsed.
        /// <para/>
        /// <see cref="SawSource"/> is true if there was an explicit source in the
        /// parsed id.  If there was no explicit source, then an implied
        /// source of ANY is returned and <see cref="SawSource"/> is set to false.
        /// <para/>
        /// <see cref="Filter"/> is the parsed filter pattern, or null if there was no
        /// filter.
        /// </summary>
        private class Specs
        {
            public string Source { get; set; } // not null
            public string Target { get; set; } // not null
            public string Variant { get; set; } // may be null
            public string Filter { get; set; } // may be null
            public bool SawSource { get; set; }
            internal Specs(string s, string t, string v, bool sawS, string f)
            {
                Source = s;
                Target = t;
                Variant = v;
                SawSource = sawS;
                Filter = f;
            }
        }

        /// <summary>
        /// A structure containing the canonicalized data of a filtered ID,
        /// that is, a basic ID optionally with a filter.
        /// <para/>
        /// <see cref="CanonID"/> is always non-null.  It may be the empty string "".
        /// It is the id that should be assigned to the created
        /// transliterator.  It _cannot_ be instantiated directly.
        /// <para/>
        /// <see cref="BasicID"/> is always non-null and non-empty.  It is always of
        /// the form S-T or S-T/V.  It is designed to be fed to low-level
        /// instantiation code that only understands these two formats.
        /// <para/>
        /// <see cref="Filter"/> may be null, if there is none, or non-null and
        /// non-empty.
        /// </summary>
        internal class SingleID
        {
            public string CanonID { get; set; }
            public string BasicID { get; set; }
            public string Filter { get; set; }
            internal SingleID(string c, string b, string f)
            {
                CanonID = c;
                BasicID = b;
                Filter = f;
            }
            internal SingleID(string c, string b)
                : this(c, b, null)
            {
            }
            internal Transliterator GetInstance()
            {
                Transliterator t;
                if (BasicID == null || BasicID.Length == 0)
                {
                    t = Transliterator.GetBasicInstance("Any-Null", CanonID);
                }
                else
                {
                    t = Transliterator.GetBasicInstance(BasicID, CanonID);
                }
                if (t != null)
                {
                    if (Filter != null)
                    {
                        t.Filter = new UnicodeSet(Filter);
                    }
                }
                return t;
            }
        }

        /// <summary>
        /// Parse a filter <paramref name="id"/>, that is, an ID of the general form
        /// "[f1] s1-t1/v1", with the filters optional, and the variants optional.
        /// </summary>
        /// <param name="id">The id to be parsed.</param>
        /// <param name="pos">INPUT-OUTPUT parameter.  On input, the position of
        /// the first character to parse.  On output, the position after
        /// the last character parsed.</param>
        /// <returns>A <see cref="SingleID"/> object or null if the parse fails.</returns>
        public static SingleID ParseFilterID(string id, int[] pos)
        {

            int start = pos[0];
            Specs specs = ParseFilterID(id, pos, true);
            if (specs == null)
            {
                pos[0] = start;
                return null;
            }

            // Assemble return results
            SingleID single = SpecsToID(specs, Forward);
            single.Filter = specs.Filter;
            return single;
        }

        /// <summary>
        /// Parse a single <paramref name="id"/>, that is, an ID of the general form
        /// "[f1] s1-t1/v1 ([f2] s2-t3/v2)", with the parenthesized element
        /// optional, the filters optional, and the variants optional.
        /// </summary>
        /// <param name="id">The id to be parsed.</param>
        /// <param name="pos">INPUT-OUTPUT parameter.  On input, the position of
        /// the first character to parse.  On output, the position after
        /// the last character parsed.</param>
        /// <param name="dir">The direction.  If the direction is <see cref="TransliterationDirection.Reverse"/> then the
        /// <see cref="SingleID"/> is constructed for the reverse direction.</param>
        /// <returns>A <see cref="SingleID"/> object or null.</returns>
        public static SingleID ParseSingleID(string id, int[] pos, TransliterationDirection dir)
        {

            int start = pos[0];

            // The ID will be of the form A, A(), A(B), or (B), where
            // A and B are filter IDs.
            Specs specsA = null;
            Specs specsB = null;
            bool sawParen = false;

            // On the first pass, look for (B) or ().  If this fails, then
            // on the second pass, look for A, A(B), or A().
            for (int pass = 1; pass <= 2; ++pass)
            {
                if (pass == 2)
                {
                    specsA = ParseFilterID(id, pos, true);
                    if (specsA == null)
                    {
                        pos[0] = start;
                        return null;
                    }
                }
                if (Utility.ParseChar(id, pos, OPEN_REV))
                {
                    sawParen = true;
                    if (!Utility.ParseChar(id, pos, CLOSE_REV))
                    {
                        specsB = ParseFilterID(id, pos, true);
                        // Must close with a ')'
                        if (specsB == null || !Utility.ParseChar(id, pos, CLOSE_REV))
                        {
                            pos[0] = start;
                            return null;
                        }
                    }
                    break;
                }
            }

            // Assemble return results
            SingleID single;
            if (sawParen)
            {
                if (dir == Forward)
                {
                    single = SpecsToID(specsA, Forward);
                    single.CanonID = single.CanonID +
                        OPEN_REV + SpecsToID(specsB, Forward).CanonID + CLOSE_REV;
                    if (specsA != null)
                    {
                        single.Filter = specsA.Filter;
                    }
                }
                else
                {
                    single = SpecsToID(specsB, Forward);
                    single.CanonID = single.CanonID +
                        OPEN_REV + SpecsToID(specsA, Forward).CanonID + CLOSE_REV;
                    if (specsB != null)
                    {
                        single.Filter = specsB.Filter;
                    }
                }
            }
            else
            {
                // assert(specsA != null);
                if (dir == Forward)
                {
                    single = SpecsToID(specsA, Forward);
                }
                else
                {
                    single = SpecsToSpecialInverse(specsA);
                    if (single == null)
                    {
                        single = SpecsToID(specsA, Reverse);
                    }
                }
                single.Filter = specsA.Filter;
            }

            return single;
        }

        /// <summary>
        /// Parse a global filter of the form "[f]" or "([f])", depending
        /// on <paramref name="withParens"/>.
        /// </summary>
        /// <param name="id">The pattern to parse.</param>
        /// <param name="pos">INPUT-OUTPUT parameter.  On input, the position of
        /// the first character to parse.  On output, the position after
        /// the last character parsed.</param>
        /// <param name="dir">The direction.</param>
        /// <param name="withParens">INPUT-OUTPUT parameter.  On entry, if
        /// withParens[0] is 0, then parens are disallowed.  If it is 1,
        /// then parens are requires.  If it is -1, then parens are
        /// optional, and the return result will be set to 0 or 1.</param>
        /// <param name="canonID">OUTPUT parameter.  The pattern for the filter
        /// added to the canonID, either at the end, if dir is <see cref="TransliterationDirection.Forward"/>, or
        /// at the start, if dir is <see cref="TransliterationDirection.Reverse"/>.  The pattern will be enclosed
        /// in parentheses if appropriate, and will be suffixed with an
        /// <see cref="ID_DELIM"/> character.  May be null.</param>
        /// <returns>A <see cref="UnicodeSet"/> object or null.  A non-null results
        /// indicates a successful parse, regardless of whether the filter
        /// applies to the given direction.  The caller should discard it
        /// if withParens != (dir == <see cref="TransliterationDirection.Reverse"/>).</returns>
        public static UnicodeSet ParseGlobalFilter(string id, int[] pos, TransliterationDirection dir,
                                                   int[] withParens,
                                                   StringBuffer canonID)
        {
            UnicodeSet filter = null;
            int start = pos[0];

            if (withParens[0] == -1)
            {
                withParens[0] = Utility.ParseChar(id, pos, OPEN_REV) ? 1 : 0;
            }
            else if (withParens[0] == 1)
            {
                if (!Utility.ParseChar(id, pos, OPEN_REV))
                {
                    pos[0] = start;
                    return null;
                }
            }

            pos[0] = PatternProps.SkipWhiteSpace(id, pos[0]);

            if (UnicodeSet.ResemblesPattern(id, pos[0]))
            {
                ParsePosition ppos = new ParsePosition(pos[0]);
                try
                {
                    filter = new UnicodeSet(id, ppos, null);
                }
                catch (ArgumentException)
                {
                    pos[0] = start;
                    return null;
                }

                string pattern = id.Substring(pos[0], ppos.Index - pos[0]); // ICU4N: Corrected 2nd parameter
                pos[0] = ppos.Index;

                if (withParens[0] == 1 && !Utility.ParseChar(id, pos, CLOSE_REV))
                {
                    pos[0] = start;
                    return null;
                }

                // In the forward direction, append the pattern to the
                // canonID.  In the reverse, insert it at zero, and invert
                // the presence of parens ("A" <-> "(A)").
                if (canonID != null)
                {
                    if (dir == Forward)
                    {
                        if (withParens[0] == 1)
                        {
                            pattern = OPEN_REV + pattern + CLOSE_REV;
                        }
                        canonID.Append(pattern + ID_DELIM);
                    }
                    else
                    {
                        if (withParens[0] == 0)
                        {
                            pattern = OPEN_REV + pattern + CLOSE_REV;
                        }
                        canonID.Insert(0, pattern + ID_DELIM);
                    }
                }
            }

            return filter;
        }

        /// <summary>
        /// Parse a compound <paramref name="id"/>, consisting of an optional forward global
        /// filter, a separator, one or more single IDs delimited by
        /// separators, an an optional reverse global filter.  The
        /// separator is a semicolon.  The global filters are <see cref="UnicodeSet"/>
        /// patterns.  The reverse global filter must be enclosed in
        /// parentheses.
        /// </summary>
        /// <param name="id">The pattern to parse.</param>
        /// <param name="dir">The direction.</param>
        /// <param name="canonID">OUTPUT parameter that receives the canonical ID,
        /// consisting of canonical IDs for all elements, as returned by
        /// <see cref="ParseSingleID(string, int[], TransliterationDirection)"/></param>, separated by semicolons.  Previous contents
        /// are discarded.
        /// <param name="list">OUTPUT parameter that receives a list of <see cref="SingleID"/>
        /// objects representing the parsed IDs.  Previous contents are
        /// discarded.</param>
        /// <param name="globalFilter">OUTPUT parameter that receives a pointer to
        /// a newly created global filter for this ID in this direction, or
        /// null if there is none.</param>
        /// <returns><c>true</c> if the parse succeeds, that is, if the entire
        /// <paramref name="id"/> is consumed without syntax error.</returns>
        public static bool ParseCompoundID(string id, TransliterationDirection dir,
                                              StringBuffer canonID,
                                              IList<SingleID> list,
                                              UnicodeSet[] globalFilter)
        {
            int[] pos = new int[] { 0 };
            int[] withParens = new int[1];
            list.Clear();
            UnicodeSet filter;
            globalFilter[0] = null;
            canonID.Length = 0;

            // Parse leading global filter, if any
            withParens[0] = 0; // parens disallowed
            filter = ParseGlobalFilter(id, pos, dir, withParens, canonID);
            if (filter != null)
            {
                if (!Utility.ParseChar(id, pos, ID_DELIM))
                {
                    // Not a global filter; backup and resume
                    canonID.Length = 0;
                    pos[0] = 0;
                }
                if (dir == Forward)
                {
                    globalFilter[0] = filter;
                }
            }

            bool sawDelimiter = true;
            for (; ; )
            {
                SingleID single = ParseSingleID(id, pos, dir);
                if (single == null)
                {
                    break;
                }
                if (dir == Forward)
                {
                    list.Add(single);
                }
                else
                {
                    list.Insert(0, single);
                }
                if (!Utility.ParseChar(id, pos, ID_DELIM))
                {
                    sawDelimiter = false;
                    break;
                }
            }

            if (list.Count == 0)
            {
                return false;
            }

            // Construct canonical ID
            for (int i = 0; i < list.Count; ++i)
            {
                SingleID single = list[i];
                canonID.Append(single.CanonID);
                if (i != (list.Count - 1))
                {
                    canonID.Append(ID_DELIM);
                }
            }

            // Parse trailing global filter, if any, and only if we saw
            // a trailing delimiter after the IDs.
            if (sawDelimiter)
            {
                withParens[0] = 1; // parens required
                filter = ParseGlobalFilter(id, pos, dir, withParens, canonID);
                if (filter != null)
                {
                    // Don't require trailing ';', but parse it if present
                    Utility.ParseChar(id, pos, ID_DELIM);

                    if (dir == Reverse)
                    {
                        globalFilter[0] = filter;
                    }
                }
            }

            // Trailing unparsed text is a syntax error
            pos[0] = PatternProps.SkipWhiteSpace(id, pos[0]);
            if (pos[0] != id.Length)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the list of <see cref="Transliterator"/> objects for the
        /// given list of <see cref="SingleID"/> objects.
        /// </summary>
        /// <param name="ids">List vector of <see cref="SingleID"/> objects.</param>
        /// <returns>Actual transliterators for the list of <see cref="SingleID"/>s.</returns>
        internal static IList<Transliterator> InstantiateList(IList<SingleID> ids)
        {
            Transliterator t;
            List<Transliterator> translits = new List<Transliterator>();
            foreach (SingleID single in ids)
            {
                if (single.BasicID.Length == 0)
                {
                    continue;
                }
                t = single.GetInstance();
                if (t == null)
                {
                    throw new ArgumentException("Illegal ID " + single.CanonID);
                }
                translits.Add(t);
            }

            // An empty list is equivalent to a Null transliterator.
            if (translits.Count == 0)
            {
                t = Transliterator.GetBasicInstance("Any-Null", null);
                if (t == null)
                {
                    // Should never happen
                    throw new ArgumentException("Internal error; cannot instantiate Any-Null");
                }
                translits.Add(t);
            }
            return translits;
        }

        /// <summary>
        /// Parse an <paramref name="id"/> into pieces.  Take IDs of the form T, T/V, S-T,
        /// S-T/V, or S/V-T.  If the source is missing, return a source of
        /// ANY.
        /// </summary>
        /// <param name="id">The id string, in any of several forms.</param>
        /// <returns>An array of 4 strings: source, target, variant, and
        /// isSourcePresent.  If the source is not present, ANY will be
        /// given as the source, and isSourcePresent will be null.  Otherwise
        /// isSourcePresent will be non-null.  The target may be empty if the
        /// id is not well-formed.  The variant may be empty.</returns>
        public static string[] IDtoSTV(string id)
        {
            string source = ANY;
            string target = null;
            string variant = "";

            int sep = id.IndexOf(TARGET_SEP);
            int var = id.IndexOf(VARIANT_SEP);
            if (var < 0)
            {
                var = id.Length;
            }
            bool isSourcePresent = false;

            if (sep < 0)
            {
                // Form: T/V or T (or /V)
                target = id.Substring(0, var); // ICU4N: Checked 2nd parameter
                variant = id.Substring(var);
            }
            else if (sep < var)
            {
                // Form: S-T/V or S-T (or -T/V or -T)
                if (sep > 0)
                {
                    source = id.Substring(0, sep); // ICU4N: Checked 2nd parameter
                    isSourcePresent = true;
                }
                target = id.Substring(++sep, var - sep); // ICU4N: Corrected 2nd parameter
                variant = id.Substring(var);
            }
            else
            {
                // Form: (S/V-T or /V-T)
                if (var > 0)
                {
                    source = id.Substring(0, var); // ICU4N: Checked 2nd parameter
                    isSourcePresent = true;
                }
                variant = id.Substring(var, sep++ - var); // ICU4N: Corrected 2nd parameter
                target = id.Substring(sep);
            }

            if (variant.Length > 0)
            {
                variant = variant.Substring(1);
            }

            return new string[] { source, target, variant,
                              isSourcePresent ? "" : null };
        }

        /// <summary>
        /// Given <paramref name="source"/>, <paramref name="target"/>, and <paramref name="variant"/> strings, concatenate them into a
        /// full ID.  If the source is empty, then "Any" will be used for the
        /// source, so the ID will always be of the form s-t/v or s-t.
        /// </summary>
        public static string STVtoID(string source,
                                     string target,
                                     string variant)
        {
            StringBuilder id = new StringBuilder(source);
            if (id.Length == 0)
            {
                id.Append(ANY);
            }
            id.Append(TARGET_SEP).Append(target);
            if (variant != null && variant.Length != 0)
            {
                id.Append(VARIANT_SEP).Append(variant);
            }
            return id.ToString();
        }

        /// <summary>
        /// Register two targets as being inverses of one another.  For
        /// example, calling <c>RegisterSpecialInverse("NFC", "NFD", true)</c> causes
        /// <see cref="Transliterator"/> to form the following inverse relationships:
        /// <code>
        /// NFC => NFD
        /// Any-NFC => Any-NFD
        /// NFD => NFC
        /// Any-NFD => Any-NFC
        /// </code>
        /// 
        /// (Without the special inverse registration, the inverse of NFC
        /// would be NFC-Any.)  Note that NFD is shorthand for Any-NFD, but
        /// that the presence or absence of "Any-" is preserved.
        /// 
        /// <para/>
        /// The relationship is symmetrical; registering (a, b) is
        /// equivalent to registering (b, a).
        /// 
        /// <para/>
        /// The relevant IDs must still be registered separately as
        /// factories or types.
        /// 
        /// <para/>
        /// Only the targets are specified.  Special inverses always
        /// have the form Any-Target1 &lt;=&gt; Any-Target2.  The target should
        /// have canonical casing (the casing desired to be produced when
        /// an inverse is formed) and should contain no whitespace or other
        /// extraneous characters.
        /// </summary>
        /// <param name="target">The target against which to register the inverse.</param>
        /// <param name="inverseTarget">The inverse of target, that is
        /// Any-target.GetInverse() => Any-inverseTarget.</param>
        /// <param name="bidirectional">If true, register the reverse relation
        /// as well, that is, Any-inverseTarget.GetInverse() => Any-target.</param>
        public static void RegisterSpecialInverse(string target,
                                                  string inverseTarget,
                                                  bool bidirectional)
        {
            SPECIAL_INVERSES[new CaseInsensitiveString(target)] = inverseTarget;
            if (bidirectional && !target.Equals(inverseTarget, StringComparison.OrdinalIgnoreCase))
            {
                SPECIAL_INVERSES[new CaseInsensitiveString(inverseTarget)] = target;
            }
        }

        //----------------------------------------------------------------
        // Private implementation
        //----------------------------------------------------------------

        /// <summary>
        /// Parse an ID into component pieces.  Take IDs of the form T,
        /// T/V, S-T, S-T/V, or S/V-T.  If the source is missing, return a
        /// source of ANY.
        /// </summary>
        /// <param name="id">The id string, in any of several forms.</param>
        /// <param name="pos">INPUT-OUTPUT parameter.  On input, pos[0] is the
        /// offset of the first character to parse in id.  On output,
        /// pos[0] is the offset after the last parsed character.  If the
        /// parse failed, pos[0] will be unchanged.</param>
        /// <param name="allowFilter">If true, a <see cref="UnicodeSet"/> pattern is allowed
        /// at any location between specs or delimiters, and is returned
        /// as the fifth string in the array.</param>
        /// <returns>A <see cref="Specs"/> object, or null if the parse failed.  If
        /// neither source nor target was seen in the parsed id, then the
        /// parse fails.  If <paramref name="allowFilter"/> is true, then the parsed filter
        /// pattern is returned in the <see cref="Specs"/> object, otherwise the returned
        /// filter reference is null.  If the parse fails for any reason
        /// null is returned.</returns>
        private static Specs ParseFilterID(string id, int[] pos,
                                           bool allowFilter)
        {
            string first = null;
            string source = null;
            string target = null;
            string variant = null;
            string filter = null;
            char delimiter = (char)0;
            int specCount = 0;
            int start = pos[0];

            // This loop parses one of the following things with each
            // pass: a filter, a delimiter character (either '-' or '/'),
            // or a spec (source, target, or variant).
            for (; ; )
            {
                pos[0] = PatternProps.SkipWhiteSpace(id, pos[0]);
                if (pos[0] == id.Length)
                {
                    break;
                }

                // Parse filters
                if (allowFilter && filter == null &&
                    UnicodeSet.ResemblesPattern(id, pos[0]))
                {

                    ParsePosition ppos = new ParsePosition(pos[0]);
                    // Parse the set to get the position.
                    new UnicodeSet(id, ppos, null);
                    filter = id.Substring(pos[0], ppos.Index - pos[0]); // ICU4N: Corrected 2nd parameter
                    pos[0] = ppos.Index;
                    continue;
                }

                if (delimiter == 0)
                {
                    char c = id[pos[0]];
                    if ((c == TARGET_SEP && target == null) ||
                        (c == VARIANT_SEP && variant == null))
                    {
                        delimiter = c;
                        ++pos[0];
                        continue;
                    }
                }

                // We are about to try to parse a spec with no delimiter
                // when we can no longer do so (we can only do so at the
                // start); break.
                if (delimiter == 0 && specCount > 0)
                {
                    break;
                }

                string spec = Utility.ParseUnicodeIdentifier(id, pos);
                if (spec == null)
                {
                    // Note that if there was a trailing delimiter, we
                    // consume it.  So Foo-, Foo/, Foo-Bar/, and Foo/Bar-
                    // are legal.
                    break;
                }

                switch (delimiter)
                {
                    case (char)0:
                        first = spec;
                        break;
                    case TARGET_SEP:
                        target = spec;
                        break;
                    case VARIANT_SEP:
                        variant = spec;
                        break;
                }
                ++specCount;
                delimiter = (char)0;
            }

            // A spec with no prior character is either source or target,
            // depending on whether an explicit "-target" was seen.
            if (first != null)
            {
                if (target == null)
                {
                    target = first;
                }
                else
                {
                    source = first;
                }
            }

            // Must have either source or target
            if (source == null && target == null)
            {
                pos[0] = start;
                return null;
            }

            // Empty source or target defaults to ANY
            bool sawSource = true;
            if (source == null)
            {
                source = ANY;
                sawSource = false;
            }
            if (target == null)
            {
                target = ANY;
            }

            return new Specs(source, target, variant, sawSource, filter);
        }

        /// <summary>
        /// Given a <see cref="Specs"/> object, convert it to a <see cref="SingleID"/> object.  The
        /// Spec object is a more unprocessed parse result.  The SingleID
        /// object contains information about canonical and basic IDs.
        /// </summary>
        /// <returns>A <see cref="SingleID"/> or null. Returned object always has
        /// <see cref="SingleID.Filter"/> value of null.</returns>
        private static SingleID SpecsToID(Specs specs, TransliterationDirection dir)
        {
            string canonID = "";
            string basicID = "";
            string basicPrefix = "";
            if (specs != null)
            {
                StringBuilder buf = new StringBuilder();
                if (dir == Forward)
                {
                    if (specs.SawSource)
                    {
                        buf.Append(specs.Source).Append(TARGET_SEP);
                    }
                    else
                    {
                        basicPrefix = specs.Source + TARGET_SEP;
                    }
                    buf.Append(specs.Target);
                }
                else
                {
                    buf.Append(specs.Target).Append(TARGET_SEP).Append(specs.Source);
                }
                if (specs.Variant != null)
                {
                    buf.Append(VARIANT_SEP).Append(specs.Variant);
                }
                basicID = basicPrefix + buf.ToString();
                if (specs.Filter != null)
                {
                    buf.Insert(0, specs.Filter);
                }
                canonID = buf.ToString();
            }
            return new SingleID(canonID, basicID);
        }

        /// <summary>
        /// Given a <see cref="Specs"/> object, return a <see cref="SingleID"/> representing the
        /// special inverse of that ID.  If there is no special inverse
        /// then return null.
        /// </summary>
        /// <returns>A <see cref="SingleID"/> or null. Returned object always has
        /// <see cref="SingleID.Filter"/> value of null.</returns>
        private static SingleID SpecsToSpecialInverse(Specs specs)
        {
            if (!specs.Source.Equals(ANY, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            string inverseTarget = SPECIAL_INVERSES.Get(new CaseInsensitiveString(specs.Target));
            if (inverseTarget != null)
            {
                // If the original ID contained "Any-" then make the
                // special inverse "Any-Foo"; otherwise make it "Foo".
                // So "Any-NFC" => "Any-NFD" but "NFC" => "NFD".
                StringBuilder buf = new StringBuilder();
                if (specs.Filter != null)
                {
                    buf.Append(specs.Filter);
                }
                if (specs.SawSource)
                {
                    buf.Append(ANY).Append(TARGET_SEP);
                }
                buf.Append(inverseTarget);

                string basicID = ANY + TARGET_SEP + inverseTarget;

                if (specs.Variant != null)
                {
                    buf.Append(VARIANT_SEP).Append(specs.Variant);
                    basicID = basicID + VARIANT_SEP + specs.Variant;
                }
                return new SingleID(buf.ToString(), basicID);
            }
            return null;
        }
    }
}
