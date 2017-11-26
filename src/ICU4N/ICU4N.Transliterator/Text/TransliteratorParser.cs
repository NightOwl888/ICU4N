using ICU4N.Impl;
using ICU4N.Lang;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using System;
using System.Collections.Generic;
using System.Text;
using Data = ICU4N.Text.RuleBasedTransliterator.Data;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    internal class TransliteratorParser
    {
        //----------------------------------------------------------------------
        // Data members
        //----------------------------------------------------------------------

        /**
         * PUBLIC data member.
         * A Vector of RuleBasedTransliterator.Data objects, one for each discrete group
         * of rules in the rule set
         */
        public IList<Data> DataVector { get; set; }

        /**
         * PUBLIC data member.
         * A Vector of Strings containing all of the ID blocks in the rule set
         */
        public IList<string> IdBlockVector { get; set; }

        /**
         * The current data object for which we are parsing rules
         */
        private Data curData;

        /**
         * PUBLIC data member containing the parsed compound filter, if any.
         */
        public UnicodeSet CompoundFilter { get; set; }


        private int direction;

        /**
         * Temporary symbol table used during parsing.
         */
        private ParseData parseData;

        /**
         * Temporary vector of set variables.  When parsing is complete, this
         * is copied into the array data.variables.  As with data.variables,
         * element 0 corresponds to character data.variablesBase.
         */
        private List<Object> variablesVector;

        /**
         * Temporary table of variable names.  When parsing is complete, this is
         * copied into data.variableNames.
         */
        private IDictionary<string, char[]> variableNames;

        /**
         * String of standins for segments.  Used during the parsing of a single
         * rule.  segmentStandins.charAt(0) is the standin for "$1" and corresponds
         * to StringMatcher object segmentObjects.elementAt(0), etc.
         */
        private StringBuffer segmentStandins;

        /**
         * Vector of StringMatcher objects for segments.  Used during the
         * parsing of a single rule.
         * segmentStandins.charAt(0) is the standin for "$1" and corresponds
         * to StringMatcher object segmentObjects.elementAt(0), etc.
         */
        private IList<StringMatcher> segmentObjects;

        /**
         * The next available stand-in for variables.  This starts at some point in
         * the private use area (discovered dynamically) and increments up toward
         * <code>variableLimit</code>.  At any point during parsing, available
         * variables are <code>variableNext..variableLimit-1</code>.
         */
        private char variableNext;

        /**
         * The last available stand-in for variables.  This is discovered
         * dynamically.  At any point during parsing, available variables are
         * <code>variableNext..variableLimit-1</code>.  During variable definition
         * we use the special value variableLimit-1 as a placeholder.
         */
        private char variableLimit;

        /**
         * When we encounter an undefined variable, we do not immediately signal
         * an error, in case we are defining this variable, e.g., "$a = [a-z];".
         * Instead, we save the name of the undefined variable, and substitute
         * in the placeholder char variableLimit - 1, and decrement
         * variableLimit.
         */
        private string undefinedVariableName;

        /**
         * The stand-in character for the 'dot' set, represented by '.' in
         * patterns.  This is allocated the first time it is needed, and
         * reused thereafter.
         */
        private int dotStandIn = -1;

        //----------------------------------------------------------------------
        // Constants
        //----------------------------------------------------------------------

        // Indicator for ID blocks
        private const string ID_TOKEN = "::";
        private const int ID_TOKEN_LEN = 2;

        /*
        (reserved for future expansion)
            // markers for beginning and end of rule groups
            private static final String BEGIN_TOKEN = "BEGIN";
            private static final String END_TOKEN = "END";
        */

        // Operators
        private const char VARIABLE_DEF_OP = '=';
        private const char FORWARD_RULE_OP = '>';
        private const char REVERSE_RULE_OP = '<';
        private const char FWDREV_RULE_OP = '~'; // internal rep of <> op

        private const string OPERATORS = "=><\u2190\u2192\u2194";
        private const string HALF_ENDERS = "=><\u2190\u2192\u2194;";

        // Other special characters
        private const char QUOTE = '\'';
        private const char ESCAPE = '\\';
        private const char END_OF_RULE = ';';
        private const char RULE_COMMENT_CHAR = '#';

        private const char CONTEXT_ANTE = '{'; // ante{key
        private const char CONTEXT_POST = '}'; // key}post
        private const char CURSOR_POS = '|';
        private const char CURSOR_OFFSET = '@';
        private const char ANCHOR_START = '^';

        private const char KLEENE_STAR = '*';
        private const char ONE_OR_MORE = '+';
        private const char ZERO_OR_ONE = '?';

        private const char DOT = '.';
        private const String DOT_SET = "[^[:Zp:][:Zl:]\\r\\n$]";

        // By definition, the ANCHOR_END special character is a
        // trailing SymbolTable.SYMBOL_REF character.
        // private const char ANCHOR_END       = '$';

        // Segments of the input string are delimited by "(" and ")".  In the
        // output string these segments are referenced as "$1", "$2", etc.
        private const char SEGMENT_OPEN = '(';
        private const char SEGMENT_CLOSE = ')';

        // A function is denoted &Source-Target/Variant(text)
        private const char FUNCTION = '&';

        // Aliases for some of the syntax characters. These are provided so
        // transliteration rules can be expressed in XML without clashing with
        // XML syntax characters '<', '>', and '&'.
        private const char ALT_REVERSE_RULE_OP = '\u2190'; // Left Arrow
        private const char ALT_FORWARD_RULE_OP = '\u2192'; // Right Arrow
        private const char ALT_FWDREV_RULE_OP = '\u2194'; // Left Right Arrow
        private const char ALT_FUNCTION = '\u2206'; // Increment (~Greek Capital Delta)

        // Special characters disallowed at the top level
        private static UnicodeSet ILLEGAL_TOP = new UnicodeSet("[\\)]");

        // Special characters disallowed within a segment
        private static UnicodeSet ILLEGAL_SEG = new UnicodeSet("[\\{\\}\\|\\@]");

        // Special characters disallowed within a function argument
        private static UnicodeSet ILLEGAL_FUNC = new UnicodeSet("[\\^\\(\\.\\*\\+\\?\\{\\}\\|\\@]");

        //----------------------------------------------------------------------
        // class ParseData
        //----------------------------------------------------------------------

        /**
         * This class implements the SymbolTable interface.  It is used
         * during parsing to give UnicodeSet access to variables that
         * have been defined so far.  Note that it uses variablesVector,
         * _not_ data.variables.
         */
        private class ParseData : ISymbolTable
        {
            private readonly TransliteratorParser outerInstance;

            public ParseData(TransliteratorParser outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            /**
             * Implement SymbolTable API.
             */
            public virtual char[] Lookup(string name)
            {
                return outerInstance.variableNames.Get(name);
            }

            /**
             * Implement SymbolTable API.
             */
            public virtual IUnicodeMatcher LookupMatcher(int ch)
            {
                // Note that we cannot use data.lookup() because the
                // set array has not been constructed yet.
                int i = ch - outerInstance.curData.variablesBase;
                if (i >= 0 && i < outerInstance.variablesVector.Count)
                {
                    return (IUnicodeMatcher)outerInstance.variablesVector[i];
                }
                return null;
            }

            /**
             * Implement SymbolTable API.  Parse out a symbol reference
             * name.
             */
            public virtual string ParseReference(string text, ParsePosition pos, int limit)
            {
                int start = pos.Index;
                int i = start;
                while (i < limit)
                {
                    char c = text[i];
                    if ((i == start && !UCharacter.IsUnicodeIdentifierStart(c)) ||
                        !UCharacter.IsUnicodeIdentifierPart(c))
                    {
                        break;
                    }
                    ++i;
                }
                if (i == start)
                { // No valid name chars
                    return null;
                }
                pos.Index = i;
                return text.Substring(start, i - start); // ICU4N: Corrected 2nd parameter
            }

            /**
             * Return true if the given character is a matcher standin or a plain
             * character (non standin).
             */
            public virtual bool IsMatcher(int ch)
            {
                // Note that we cannot use data.lookup() because the
                // set array has not been constructed yet.
                int i = ch - outerInstance.curData.variablesBase;
                if (i >= 0 && i < outerInstance.variablesVector.Count)
                {
                    return outerInstance.variablesVector[i] is IUnicodeMatcher;
                }
                return true;
            }

            /**
             * Return true if the given character is a replacer standin or a plain
             * character (non standin).
             */
            public virtual bool IsReplacer(int ch)
            {
                // Note that we cannot use data.lookup() because the
                // set array has not been constructed yet.
                int i = ch - outerInstance.curData.variablesBase;
                if (i >= 0 && i < outerInstance.variablesVector.Count)
                {
                    return outerInstance.variablesVector[i] is IUnicodeReplacer;
                }
                return true;
            }
        }

        //----------------------------------------------------------------------
        // classes RuleBody, RuleArray, and RuleReader
        //----------------------------------------------------------------------

        /**
         * A private abstract class representing the interface to rule
         * source code that is broken up into lines.  Handles the
         * folding of lines terminated by a backslash.  This folding
         * is limited; it does not account for comments, quotes, or
         * escapes, so its use to be limited.
         */
        private abstract class RuleBody
        {

            /**
             * Retrieve the next line of the source, or return null if
             * none.  Folds lines terminated by a backslash into the
             * next line, without regard for comments, quotes, or
             * escapes.
             */
            internal virtual string NextLine()
            {
                string s = HandleNextLine();
                if (s != null &&
                    s.Length > 0 &&
                    s[s.Length - 1] == '\\')
                {
                    StringBuilder b = new StringBuilder(s);
                    do
                    {
                        b.Remove(b.Length - 1, 1);
                        s = HandleNextLine();
                        if (s == null)
                        {
                            break;
                        }
                        b.Append(s);
                    } while (s.Length > 0 &&
                             s[s.Length - 1] == '\\');
                    s = b.ToString();
                }
                return s;
            }

            /**
             * Reset to the first line of the source.
             */
            public abstract void Reset();

            /**
             * Subclass method to return the next line of the source.
             */
            public abstract string HandleNextLine();
        }

        /**
         * RuleBody subclass for a String[] array.
         */
        private class RuleArray : RuleBody
        {
            internal string[] array;
            internal int i;
            public RuleArray(string[] array) { this.array = array; i = 0; }

            public override string HandleNextLine()
            {
                return (i < array.Length) ? array[i++] : null;
            }

            public override void Reset()
            {
                i = 0;
            }
        }

        /*
         * RuleBody subclass for a ResourceReader.
         */
        /*    private static class RuleReader extends RuleBody {
                ResourceReader reader;
                public RuleReader(ResourceReader reader) { this.reader = reader; }
                public String handleNextLine() {
                    try {
                        return reader.readLine();
                    } catch (java.io.IOException e) {}
                    return null;
                }
                public void reset() {
                    reader.reset();
                }
            }*/

        //----------------------------------------------------------------------
        // class RuleHalf
        //----------------------------------------------------------------------

        /**
         * A class representing one side of a rule.  This class knows how to
         * parse half of a rule.  It is tightly coupled to the method
         * TransliteratorParser.parseRule().
         */
        private class RuleHalf
        {
            public string Text { get; set; }

            public int Cursor { get; set; } = -1; // position of cursor in text
            public int Ante { get; set; } = -1;   // position of ante context marker '{' in text
            public int Post { get; set; } = -1;   // position of post context marker '}' in text

            // Record the offset to the cursor either to the left or to the
            // right of the key.  This is indicated by characters on the output
            // side that allow the cursor to be positioned arbitrarily within
            // the matching text.  For example, abc{def} > | @@@ xyz; changes
            // def to xyz and moves the cursor to before abc.  Offset characters
            // must be at the start or end, and they cannot move the cursor past
            // the ante- or postcontext text.  Placeholders are only valid in
            // output text.  The length of the ante and post context is
            // determined at runtime, because of supplementals and quantifiers.
            public int CursorOffset { get; set; } = 0; // only nonzero on output side

            // Position of first CURSOR_OFFSET on _right_.  This will be -1
            // for |@, -2 for |@@, etc., and 1 for @|, 2 for @@|, etc.
            private int cursorOffsetPos = 0;

            public bool AnchorStart { get; set; } = false;
            public bool AnchorEnd { get; set; } = false;

            /**
             * The segment number from 1..n of the next '(' we see
             * during parsing; 1-based.
             */
            private int nextSegmentNumber = 1;

            /**
             * Parse one side of a rule, stopping at either the limit,
             * the END_OF_RULE character, or an operator.
             * @return the index after the terminating character, or
             * if limit was reached, limit
             */
            public virtual int Parse(string rule, int pos, int limit,
                             TransliteratorParser parser)
            {
                int start = pos;
                StringBuffer buf = new StringBuffer();
                pos = ParseSection(rule, pos, limit, parser, buf, ILLEGAL_TOP, false);
                Text = buf.ToString();

                if (CursorOffset > 0 && Cursor != cursorOffsetPos)
                {
                    SyntaxError("Misplaced " + CURSOR_POS, rule, start);
                }

                return pos;
            }

            /**
             * Parse a section of one side of a rule, stopping at either
             * the limit, the END_OF_RULE character, an operator, or a
             * segment close character.  This method parses both a
             * top-level rule half and a segment within such a rule half.
             * It calls itself recursively to parse segments and nested
             * segments.
             * @param buf buffer into which to accumulate the rule pattern
             * characters, either literal characters from the rule or
             * standins for UnicodeMatcher objects including segments.
             * @param illegal the set of special characters that is illegal during
             * this parse.
             * @param isSegment if true, then we've already seen a '(' and
             * pos on entry points right after it.  Accumulate everything
             * up to the closing ')', put it in a segment matcher object,
             * generate a standin for it, and add the standin to buf.  As
             * a side effect, update the segments vector with a reference
             * to the segment matcher.  This works recursively for nested
             * segments.  If isSegment is false, just accumulate
             * characters into buf.
             * @return the index after the terminating character, or
             * if limit was reached, limit
             */
            private int ParseSection(string rule, int pos, int limit,
                                     TransliteratorParser parser,
                                     StringBuffer buf,
                                     UnicodeSet illegal,
                                     bool isSegment)
            {
                int start = pos;
                ParsePosition pp = null;
                int quoteStart = -1; // Most recent 'single quoted string'
                int quoteLimit = -1;
                int varStart = -1; // Most recent $variableReference
                int varLimit = -1;
                int[] iref = new int[1];
                int bufStart = buf.Length;

                //main:
                while (pos < limit)
                {
                    // Since all syntax characters are in the BMP, fetching
                    // 16-bit code units suffices here.
                    char c = rule[pos++];
                    if (PatternProps.IsWhiteSpace(c))
                    {
                        continue;
                    }
                    // HALF_ENDERS is all chars that end a rule half: "<>=;"
                    if (HALF_ENDERS.IndexOf(c) >= 0)
                    {
                        ///CLOVER:OFF
                        // isSegment is always false
                        if (isSegment)
                        {
                            SyntaxError("Unclosed segment", rule, start);
                        }
                        ///CLOVER:ON
                        goto main_break;
                    }
                    if (AnchorEnd)
                    {
                        // Text after a presumed end anchor is a syntax err
                        SyntaxError("Malformed variable reference", rule, start);
                    }
                    if (UnicodeSet.ResemblesPattern(rule, pos - 1))
                    {
                        if (pp == null)
                        {
                            pp = new ParsePosition(0);
                        }
                        pp.Index = (pos - 1); // Backup to opening '['
                        buf.Append(parser.ParseSet(rule, pp));
                        pos = pp.Index;
                        continue;
                    }
                    // Handle escapes
                    if (c == ESCAPE)
                    {
                        if (pos == limit)
                        {
                            SyntaxError("Trailing backslash", rule, start);
                        }
                        iref[0] = pos;
                        int escaped = Utility.UnescapeAt(rule, iref);
                        pos = iref[0];
                        if (escaped == -1)
                        {
                            SyntaxError("Malformed escape", rule, start);
                        }
                        parser.CheckVariableRange(escaped, rule, start);
                        UTF16.Append(buf, escaped);
                        continue;
                    }
                    // Handle quoted matter
                    if (c == QUOTE)
                    {
                        int iq = rule.IndexOf(QUOTE, pos);
                        if (iq == pos)
                        {
                            buf.Append(c); // Parse [''] outside quotes as [']
                            ++pos;
                        }
                        else
                        {
                            /* This loop picks up a run of quoted text of the
                             * form 'aaaa' each time through.  If this run
                             * hasn't really ended ('aaaa''bbbb') then it keeps
                             * looping, each time adding on a new run.  When it
                             * reaches the final quote it breaks.
                             */
                            quoteStart = buf.Length;
                            for (; ; )
                            {
                                if (iq < 0)
                                {
                                    SyntaxError("Unterminated quote", rule, start);
                                }
                                buf.Append(rule.Substring(pos, iq - pos)); // ICU4N: Corrected 2nd parameter
                                pos = iq + 1;
                                if (pos < limit && rule[pos] == QUOTE)
                                {
                                    // Parse [''] inside quotes as [']
                                    iq = rule.IndexOf(QUOTE, pos + 1);
                                    // Continue looping
                                }
                                else
                                {
                                    break;
                                }
                            }
                            quoteLimit = buf.Length;

                            for (iq = quoteStart; iq < quoteLimit; ++iq)
                            {
                                parser.CheckVariableRange(buf[iq], rule, start);
                            }
                        }
                        continue;
                    }

                    parser.CheckVariableRange(c, rule, start);

                    if (illegal.Contains(c))
                    {
                        SyntaxError("Illegal character '" + c + '\'', rule, start);
                    }

                    switch (c)
                    {

                        //------------------------------------------------------
                        // Elements allowed within and out of segments
                        //------------------------------------------------------
                        case ANCHOR_START:
                            if (buf.Length == 0 && !AnchorStart)
                            {
                                AnchorStart = true;
                            }
                            else
                            {
                                SyntaxError("Misplaced anchor start",
                                    rule, start);
                            }
                            break;
                        case SEGMENT_OPEN:
                            {
                                // bufSegStart is the offset in buf to the first
                                // character of the segment we are parsing.
                                int bufSegStart = buf.Length;

                                // Record segment number now, since nextSegmentNumber
                                // will be incremented during the call to parseSection
                                // if there are nested segments.
                                int segmentNumber = nextSegmentNumber++; // 1-based

                                // Parse the segment
                                pos = ParseSection(rule, pos, limit, parser, buf, ILLEGAL_SEG, true);

                                // After parsing a segment, the relevant characters are
                                // in buf, starting at offset bufSegStart.  Extract them
                                // into a string matcher, and replace them with a
                                // standin for that matcher.
                                StringMatcher m =
                                    new StringMatcher(buf.ToString(bufSegStart, buf.Length - bufSegStart),
                                                      segmentNumber, parser.curData);

                                // Record and associate object and segment number
                                parser.SetSegmentObject(segmentNumber, m);
                                buf.Length = bufSegStart;
                                buf.Append(parser.GetSegmentStandin(segmentNumber));
                            }
                            break;
                        case FUNCTION:
                        case ALT_FUNCTION:
                            {
                                iref[0] = pos;
                                TransliteratorIDParser.SingleID single = TransliteratorIDParser.ParseFilterID(rule, iref);
                                // The next character MUST be a segment open
                                if (single == null ||
                                    !Utility.ParseChar(rule, iref, SEGMENT_OPEN))
                                {
                                    SyntaxError("Invalid function", rule, start);
                                }

                                Transliterator t = single.GetInstance();
                                if (t == null)
                                {
                                    SyntaxError("Invalid function ID", rule, start);
                                }

                                // bufSegStart is the offset in buf to the first
                                // character of the segment we are parsing.
                                int bufSegStart = buf.Length;

                                // Parse the segment
                                pos = ParseSection(rule, iref[0], limit, parser, buf, ILLEGAL_FUNC, true);

                                // After parsing a segment, the relevant characters are
                                // in buf, starting at offset bufSegStart.
                                FunctionReplacer r =
                                    new FunctionReplacer(t,
                                        new StringReplacer(buf.ToString(bufSegStart, buf.Length - bufSegStart), parser.curData));

                                // Replace the buffer contents with a stand-in
                                buf.Length = bufSegStart;
                                buf.Append(parser.GenerateStandInFor(r));
                            }
                            break;
                        case SymbolTable.SYMBOL_REF:
                            // Handle variable references and segment references "$1" .. "$9"
                            {
                                // A variable reference must be followed immediately
                                // by a Unicode identifier start and zero or more
                                // Unicode identifier part characters, or by a digit
                                // 1..9 if it is a segment reference.
                                if (pos == limit)
                                {
                                    // A variable ref character at the end acts as
                                    // an anchor to the context limit, as in perl.
                                    AnchorEnd = true;
                                    break;
                                }
                                // Parse "$1" "$2" .. "$9" .. (no upper limit)
                                c = rule[pos];
                                int r = UCharacter.Digit(c, 10);
                                if (r >= 1 && r <= 9)
                                {
                                    iref[0] = pos;
                                    r = Utility.ParseNumber(rule, iref, 10);
                                    if (r < 0)
                                    {
                                        SyntaxError("Undefined segment reference",
                                            rule, start);
                                    }
                                    pos = iref[0];
                                    buf.Append(parser.GetSegmentStandin(r));
                                }
                                else
                                {
                                    if (pp == null)
                                    { // Lazy create
                                        pp = new ParsePosition(0);
                                    }
                                    pp.Index = pos;
                                    string name = parser.parseData.
                                        ParseReference(rule, pp, limit);
                                    if (name == null)
                                    {
                                        // This means the '$' was not followed by a
                                        // valid name.  Try to interpret it as an
                                        // end anchor then.  If this also doesn't work
                                        // (if we see a following character) then signal
                                        // an error.
                                        AnchorEnd = true;
                                        break;
                                    }
                                    pos = pp.Index;
                                    // If this is a variable definition statement,
                                    // then the LHS variable will be undefined.  In
                                    // that case appendVariableDef() will append the
                                    // special placeholder char variableLimit-1.
                                    varStart = buf.Length;
                                    parser.AppendVariableDef(name, buf);
                                    varLimit = buf.Length;
                                }
                            }
                            break;
                        case DOT:
                            buf.Append(parser.GetDotStandIn());
                            break;
                        case KLEENE_STAR:
                        case ONE_OR_MORE:
                        case ZERO_OR_ONE:
                            // Quantifiers.  We handle single characters, quoted strings,
                            // variable references, and segments.
                            //  a+      matches  aaa
                            //  'foo'+  matches  foofoofoo
                            //  $v+     matches  xyxyxy if $v == xy
                            //  (seg)+  matches  segsegseg
                            {
                                ///CLOVER:OFF
                                // isSegment is always false
                                if (isSegment && buf.Length == bufStart)
                                {
                                    // The */+ immediately follows '('
                                    SyntaxError("Misplaced quantifier", rule, start);
                                    break;
                                }
                                ///CLOVER:ON

                                int qstart, qlimit;
                                // The */+ follows an isolated character or quote
                                // or variable reference
                                if (buf.Length == quoteLimit)
                                {
                                    // The */+ follows a 'quoted string'
                                    qstart = quoteStart;
                                    qlimit = quoteLimit;
                                }
                                else if (buf.Length == varLimit)
                                {
                                    // The */+ follows a $variableReference
                                    qstart = varStart;
                                    qlimit = varLimit;
                                }
                                else
                                {
                                    // The */+ follows a single character, possibly
                                    // a segment standin
                                    qstart = buf.Length - 1;
                                    qlimit = qstart + 1;
                                }

                                IUnicodeMatcher m;
                                try
                                {
                                    m = new StringMatcher(buf.ToString(), qstart, qlimit,
                                                      0, parser.curData);
                                }
                                catch (Exception e)
                                {
                                    string precontext = pos < 50 ? rule.Substring(0, pos) : "..." + rule.Substring(pos - 50, 50); // ICU4N: pos - (pos - 50) == 50
                                    string postContext = limit - pos <= 50 ? rule.Substring(pos, limit - pos) : rule.Substring(pos, 50) + "..."; // ICU4N: Corrected both 2nd paramemeters of substring
                                    throw new IcuArgumentException("Failure in rule: " + precontext + "$$$"
                                            + postContext, e);
                                }
                                int min = 0;
                                int max = Quantifier.MAX;
                                switch (c)
                                {
                                    case ONE_OR_MORE:
                                        min = 1;
                                        break;
                                    case ZERO_OR_ONE:
                                        min = 0;
                                        max = 1;
                                        break;
                                        // case KLEENE_STAR:
                                        //    do nothing -- min, max already set
                                }
                                m = new Quantifier(m, min, max);
                                buf.Length = qstart;
                                buf.Append(parser.GenerateStandInFor(m));
                            }
                            break;

                        //------------------------------------------------------
                        // Elements allowed ONLY WITHIN segments
                        //------------------------------------------------------
                        case SEGMENT_CLOSE:
                            // assert(isSegment);
                            // We're done parsing a segment.
                            goto main_break;

                        //------------------------------------------------------
                        // Elements allowed ONLY OUTSIDE segments
                        //------------------------------------------------------
                        case CONTEXT_ANTE:
                            if (Ante >= 0)
                            {
                                SyntaxError("Multiple ante contexts", rule, start);
                            }
                            Ante = buf.Length;
                            break;
                        case CONTEXT_POST:
                            if (Post >= 0)
                            {
                                SyntaxError("Multiple post contexts", rule, start);
                            }
                            Post = buf.Length;
                            break;
                        case CURSOR_POS:
                            if (Cursor >= 0)
                            {
                                SyntaxError("Multiple cursors", rule, start);
                            }
                            Cursor = buf.Length;
                            break;
                        case CURSOR_OFFSET:
                            if (CursorOffset < 0)
                            {
                                if (buf.Length > 0)
                                {
                                    SyntaxError("Misplaced " + c, rule, start);
                                }
                                --CursorOffset;
                            }
                            else if (CursorOffset > 0)
                            {
                                if (buf.Length != cursorOffsetPos || Cursor >= 0)
                                {
                                    SyntaxError("Misplaced " + c, rule, start);
                                }
                                ++CursorOffset;
                            }
                            else
                            {
                                if (Cursor == 0 && buf.Length == 0)
                                {
                                    CursorOffset = -1;
                                }
                                else if (Cursor < 0)
                                {
                                    cursorOffsetPos = buf.Length;
                                    CursorOffset = 1;
                                }
                                else
                                {
                                    SyntaxError("Misplaced " + c, rule, start);
                                }
                            }
                            break;

                        //------------------------------------------------------
                        // Non-special characters
                        //------------------------------------------------------
                        default:
                            // Disallow unquoted characters other than [0-9A-Za-z]
                            // in the printable ASCII range.  These characters are
                            // reserved for possible future use.
                            if (c >= 0x0021 && c <= 0x007E &&
                                !((c >= '0' && c <= '9') ||
                                  (c >= 'A' && c <= 'Z') ||
                                  (c >= 'a' && c <= 'z')))
                            {
                                SyntaxError("Unquoted " + c, rule, start);
                            }
                            buf.Append(c);
                            break;
                    }
                }
                main_break: { }
                return pos;
            }

            /**
             * Remove context.
             */
            internal virtual void RemoveContext()
            {
                int start = Ante < 0 ? 0 : Ante;
                Text = Text.Substring(start,
                                      (Post < 0 ? Text.Length : Post) - start); // ICU4N: Corrected 2nd parameter
                Ante = Post = -1;
                AnchorStart = AnchorEnd = false;
            }

            /**
             * Return true if this half looks like valid output, that is, does not
             * contain quantifiers or other special input-only elements.
             */
            public virtual bool IsValidOutput(TransliteratorParser parser)
            {
                for (int i = 0; i < Text.Length;)
                {
                    int c = UTF16.CharAt(Text, i);
                    i += UTF16.GetCharCount(c);
                    if (!parser.parseData.IsReplacer(c))
                    {
                        return false;
                    }
                }
                return true;
            }

            /**
             * Return true if this half looks like valid input, that is, does not
             * contain functions or other special output-only elements.
             */
            public virtual bool IsValidInput(TransliteratorParser parser)
            {
                for (int i = 0; i < Text.Length;)
                {
                    int c = UTF16.CharAt(Text, i);
                    i += UTF16.GetCharCount(c);
                    if (!parser.parseData.IsMatcher(c))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        //----------------------------------------------------------------------
        // PUBLIC methods
        //----------------------------------------------------------------------

        /**
         * Constructor.
         */
        public TransliteratorParser()
        {
        }

        /**
         * Parse a set of rules.  After the parse completes, examine the public
         * data members for results.
         */
        public virtual void Parse(string rules, int dir)
        {
            ParseRules(new RuleArray(new string[] { rules }), dir);
        }

        /*
         * Parse a set of rules.  After the parse completes, examine the public
         * data members for results.
         */
        /*    public void parse(ResourceReader rules, int direction) {
                parseRules(new RuleReader(rules), direction);
            }*/

        //----------------------------------------------------------------------
        // PRIVATE methods
        //----------------------------------------------------------------------

        /**
         * Parse an array of zero or more rules.  The strings in the array are
         * treated as if they were concatenated together, with rule terminators
         * inserted between array elements if not present already.
         *
         * Any previous rules are discarded.  Typically this method is called exactly
         * once, during construction.
         *
         * The member this.data will be set to null if there are no rules.
         *
         * @exception IllegalIcuArgumentException if there is a syntax error in the
         * rules
         */
        private void ParseRules(RuleBody ruleArray, int dir)
        {
            bool parsingIDs = true;
            int ruleCount = 0;

            DataVector = new List<Data>();
            IdBlockVector = new List<string>();
            curData = null;
            direction = dir;
            CompoundFilter = null;
            variablesVector = new List<object>();
            variableNames = new Dictionary<string, char[]>();
            parseData = new ParseData(this);

            List<Exception> errors = new List<Exception>();
            int errorCount = 0;

            ruleArray.Reset();

            StringBuilder idBlockResult = new StringBuilder();

            // The compound filter offset is an index into idBlockResult.
            // If it is 0, then the compound filter occurred at the start,
            // and it is the offset to the _start_ of the compound filter
            // pattern.  Otherwise it is the offset to the _limit_ of the
            // compound filter pattern within idBlockResult.
            this.CompoundFilter = null;
            int compoundFilterOffset = -1;

            //main:
            for (; ; )
            {
                string rule = ruleArray.NextLine();
                if (rule == null)
                {
                    break;
                }
                int pos = 0;
                int limit = rule.Length;
                while (pos < limit)
                {
                    char c = rule[pos++];
                    if (PatternProps.IsWhiteSpace(c))
                    {
                        continue;
                    }
                    // Skip lines starting with the comment character
                    if (c == RULE_COMMENT_CHAR)
                    {
                        pos = rule.IndexOf("\n", pos) + 1;
                        if (pos == 0)
                        {
                            break; // No "\n" found; rest of rule is a commnet
                        }
                        continue; // Either fall out or restart with next line
                    }

                    // skip empty rules
                    if (c == END_OF_RULE)
                        continue;

                    // Often a rule file contains multiple errors.  It's
                    // convenient to the rule author if these are all reported
                    // at once.  We keep parsing rules even after a failure, up
                    // to a specified limit, and report all errors at once.
                    try
                    {
                        ++ruleCount;

                        // We've found the start of a rule or ID.  c is its first
                        // character, and pos points past c.
                        --pos;
                        // Look for an ID token.  Must have at least ID_TOKEN_LEN + 1
                        // chars left.
                        if ((pos + ID_TOKEN_LEN + 1) <= limit &&
                                rule.RegionMatches(pos, ID_TOKEN, 0, ID_TOKEN_LEN))
                        {
                            pos += ID_TOKEN_LEN;
                            c = rule[pos];
                            while (PatternProps.IsWhiteSpace(c) && pos < limit)
                            {
                                ++pos;
                                c = rule[pos];
                            }
                            int[] p = new int[] { pos };

                            if (!parsingIDs)
                            {
                                if (curData != null)
                                {
                                    if (direction == Transliterator.FORWARD)
                                        DataVector.Add(curData);
                                    else
                                        DataVector.Insert(0, curData);
                                    curData = null;
                                }
                                parsingIDs = true;
                            }

                            TransliteratorIDParser.SingleID id =
                                TransliteratorIDParser.ParseSingleID(
                                              rule, p, direction);
                            if (p[0] != pos && Utility.ParseChar(rule, p, END_OF_RULE))
                            {
                                // Successful ::ID parse.

                                if (direction == Transliterator.FORWARD)
                                {
                                    idBlockResult.Append(id.CanonID).Append(END_OF_RULE);
                                }
                                else
                                {
                                    idBlockResult.Insert(0, id.CanonID + END_OF_RULE);
                                }

                            }
                            else
                            {
                                // Couldn't parse an ID.  Try to parse a global filter
                                int[] withParens = new int[] { -1 };
                                UnicodeSet f = TransliteratorIDParser.ParseGlobalFilter(rule, p, direction, withParens, null);
                                if (f != null && Utility.ParseChar(rule, p, END_OF_RULE))
                                {
                                    if ((direction == Transliterator.FORWARD) ==
                                        (withParens[0] == 0))
                                    {
                                        if (CompoundFilter != null)
                                        {
                                            // Multiple compound filters
                                            SyntaxError("Multiple global filters", rule, pos);
                                        }
                                        CompoundFilter = f;
                                        compoundFilterOffset = ruleCount;
                                    }
                                }
                                else
                                {
                                    // Invalid ::id
                                    // Can be parsed as neither an ID nor a global filter
                                    SyntaxError("Invalid ::ID", rule, pos);
                                }
                            }

                            pos = p[0];
                        }
                        else
                        {
                            if (parsingIDs)
                            {
                                if (direction == Transliterator.FORWARD)
                                    IdBlockVector.Add(idBlockResult.ToString());
                                else
                                    IdBlockVector.Insert(0, idBlockResult.ToString());
                                idBlockResult.Delete(0, idBlockResult.Length);
                                parsingIDs = false;
                                curData = new RuleBasedTransliterator.Data();

                                // By default, rules use part of the private use area
                                // E000..F8FF for variables and other stand-ins.  Currently
                                // the range F000..F8FF is typically sufficient.  The 'use
                                // variable range' pragma allows rule sets to modify this.
                                SetVariableRange(0xF000, 0xF8FF);
                            }

                            if (ResemblesPragma(rule, pos, limit))
                            {
                                int ppp = ParsePragma(rule, pos, limit);
                                if (ppp < 0)
                                {
                                    SyntaxError("Unrecognized pragma", rule, pos);
                                }
                                pos = ppp;
                                // Parse a rule
                            }
                            else
                            {
                                pos = ParseRule(rule, pos, limit);
                            }
                        }
                    }
                    catch (ArgumentException e)
                    {
                        // ICU4N TODO: Throw AggregateException ?
                        if (errorCount == 30)
                        {
                            IcuArgumentException icuEx = new IcuArgumentException("\nMore than 30 errors; further messages squelched", e);
                            errors.Add(icuEx);
                            goto main_break;
                        }
                        //e.fillInStackTrace();
                        errors.Add(e);
                        ++errorCount;
                        pos = RuleEnd(rule, pos, limit) + 1; // +1 advances past ';'
                    }
                }
            }
            main_break: { }
            if (parsingIDs && idBlockResult.Length > 0)
            {
                if (direction == Transliterator.FORWARD)
                    IdBlockVector.Add(idBlockResult.ToString());
                else
                    IdBlockVector.Insert(0, idBlockResult.ToString());
            }
            else if (!parsingIDs && curData != null)
            {
                if (direction == Transliterator.FORWARD)
                    DataVector.Add(curData);
                else
                    DataVector.Insert(0, curData);
            }

            // Convert the set vector to an array
            for (int i = 0; i < DataVector.Count; i++)
            {
                Data data = DataVector[i];
                data.variables = new Object[variablesVector.Count];
                variablesVector.CopyTo(data.variables);
                data.variableNames = new Dictionary<string, char[]>();
                data.variableNames.PutAll(variableNames);
            }
            variablesVector = null;

            // Do more syntax checking and index the rules
            try
            {
                if (CompoundFilter != null)
                {
                    if ((direction == Transliterator.FORWARD &&
                         compoundFilterOffset != 1) ||
                        (direction == Transliterator.REVERSE &&
                         compoundFilterOffset != ruleCount))
                    {
                        throw new IcuArgumentException("Compound filters misplaced");
                    }
                }

                for (int i = 0; i < DataVector.Count; i++)
                {
                    Data data = DataVector[i];
                    data.RuleSet.Freeze();
                }

                if (IdBlockVector.Count == 1 && (IdBlockVector[0]).Length == 0)
                    IdBlockVector.RemoveAt(0);

            }
            catch (ArgumentException e)
            {
                //e.fillInStackTrace();
                errors.Add(e);
            }

            if (errors.Count != 0)
            {
                //for (int i = errors.Count - 1; i > 0; --i)
                //{
                //    Exception previous = errors[i - 1];
                //    while (previous.InnerException != null)
                //    {
                //        previous = (Exception)previous.InnerException; // chain specially
                //    }
                //    previous.InitCause(errors[i]);
                //}
                //throw errors[0];
                // if initCause not supported: throw new IllegalArgumentException(errors.toString());

                // In .NET this isn't supported, so we just use AggregateException
                throw new AggregateException(errors);
            }
        }

        /**
         * MAIN PARSER.  Parse the next rule in the given rule string, starting
         * at pos.  Return the index after the last character parsed.  Do not
         * parse characters at or after limit.
         *
         * Important:  The character at pos must be a non-whitespace character
         * that is not the comment character.
         *
         * This method handles quoting, escaping, and whitespace removal.  It
         * parses the end-of-rule character.  It recognizes context and cursor
         * indicators.  Once it does a lexical breakdown of the rule at pos, it
         * creates a rule object and adds it to our rule list.
         *
         * This method is tightly coupled to the inner class RuleHalf.
         */
        private int ParseRule(string rule, int pos, int limit)
        {
            // Locate the left side, operator, and right side
            int start = pos;
            char op = (char)0;

            // Set up segments data
            segmentStandins = new StringBuffer();
            segmentObjects = new List<StringMatcher>();

            RuleHalf left = new RuleHalf();
            RuleHalf right = new RuleHalf();

            undefinedVariableName = null;
            pos = left.Parse(rule, pos, limit, this);

            if (pos == limit ||
                OPERATORS.IndexOf(op = rule[--pos]) < 0)
            {
                SyntaxError("No operator pos=" + pos, rule, start);
            }
            ++pos;

            // Found an operator char.  Check for forward-reverse operator.
            if (op == REVERSE_RULE_OP &&
                (pos < limit && rule[pos] == FORWARD_RULE_OP))
            {
                ++pos;
                op = FWDREV_RULE_OP;
            }

            // Translate alternate op characters.
            switch (op)
            {
                case ALT_FORWARD_RULE_OP:
                    op = FORWARD_RULE_OP;
                    break;
                case ALT_REVERSE_RULE_OP:
                    op = REVERSE_RULE_OP;
                    break;
                case ALT_FWDREV_RULE_OP:
                    op = FWDREV_RULE_OP;
                    break;
            }

            pos = right.Parse(rule, pos, limit, this);

            if (pos < limit)
            {
                if (rule[--pos] == END_OF_RULE)
                {
                    ++pos;
                }
                else
                {
                    // RuleHalf parser must have terminated at an operator
                    SyntaxError("Unquoted operator", rule, start);
                }
            }

            if (op == VARIABLE_DEF_OP)
            {
                // LHS is the name.  RHS is a single character, either a literal
                // or a set (already parsed).  If RHS is longer than one
                // character, it is either a multi-character string, or multiple
                // sets, or a mixture of chars and sets -- syntax error.

                // We expect to see a single undefined variable (the one being
                // defined).
                if (undefinedVariableName == null)
                {
                    SyntaxError("Missing '$' or duplicate definition", rule, start);
                }
                if (left.Text.Length != 1 || left.Text[0] != variableLimit)
                {
                    SyntaxError("Malformed LHS", rule, start);
                }
                if (left.AnchorStart || left.AnchorEnd ||
                    right.AnchorStart || right.AnchorEnd)
                {
                    SyntaxError("Malformed variable def", rule, start);
                }
                // We allow anything on the right, including an empty string.
                int n = right.Text.Length;
                char[] value = new char[n];
                right.Text.CopyTo(0, value, 0, n);
                variableNames[undefinedVariableName]= value;

                ++variableLimit;
                return pos;
            }

            // If this is not a variable definition rule, we shouldn't have
            // any undefined variable names.
            if (undefinedVariableName != null)
            {
                SyntaxError("Undefined variable $" + undefinedVariableName,
                            rule, start);
            }

            // Verify segments
            if (segmentStandins.Length > segmentObjects.Count)
            {
                SyntaxError("Undefined segment reference", rule, start);
            }
            for (int i = 0; i < segmentStandins.Length; ++i)
            {
                if (segmentStandins[i] == 0)
                {
                    SyntaxError("Internal error", rule, start); // will never happen
                }
            }
            for (int i = 0; i < segmentObjects.Count; ++i)
            {
                if (segmentObjects[i] == null)
                {
                    SyntaxError("Internal error", rule, start); // will never happen
                }
            }

            // If the direction we want doesn't match the rule
            // direction, do nothing.
            if (op != FWDREV_RULE_OP &&
                ((direction == Transliterator.FORWARD) != (op == FORWARD_RULE_OP)))
            {
                return pos;
            }

            // Transform the rule into a forward rule by swapping the
            // sides if necessary.
            if (direction == Transliterator.REVERSE)
            {
                RuleHalf temp = left;
                left = right;
                right = temp;
            }

            // Remove non-applicable elements in forward-reverse
            // rules.  Bidirectional rules ignore elements that do not
            // apply.
            if (op == FWDREV_RULE_OP)
            {
                right.RemoveContext();
                left.Cursor = -1;
                left.CursorOffset = 0;
            }

            // Normalize context
            if (left.Ante < 0)
            {
                left.Ante = 0;
            }
            if (left.Post < 0)
            {
                left.Post = left.Text.Length;
            }

            // Context is only allowed on the input side.  Cursors are only
            // allowed on the output side.  Segment delimiters can only appear
            // on the left, and references on the right.  Cursor offset
            // cannot appear without an explicit cursor.  Cursor offset
            // cannot place the cursor outside the limits of the context.
            // Anchors are only allowed on the input side.
            if (right.Ante >= 0 || right.Post >= 0 || left.Cursor >= 0 ||
                (right.CursorOffset != 0 && right.Cursor < 0) ||
                // - The following two checks were used to ensure that the
                // - the cursor offset stayed within the ante- or postcontext.
                // - However, with the addition of quantifiers, we have to
                // - allow arbitrary cursor offsets and do runtime checking.
                //(right.cursorOffset > (left.text.Length - left.post)) ||
                //(-right.cursorOffset > left.ante) ||
                right.AnchorStart || right.AnchorEnd ||
                !left.IsValidInput(this) || !right.IsValidOutput(this) ||
                left.Ante > left.Post)
            {
                SyntaxError("Malformed rule", rule, start);
            }

            // Flatten segment objects vector to an array
            StringMatcher[] segmentsArray = null;
            if (segmentObjects.Count > 0)
            {
                segmentsArray = new StringMatcher[segmentObjects.Count];
                segmentObjects.CopyTo(segmentsArray, 0);
            }

            curData.RuleSet.AddRule(new TransliterationRule(
                                         left.Text, left.Ante, left.Post,
                                         right.Text, right.Cursor, right.CursorOffset,
                                         segmentsArray,
                                         left.AnchorStart, left.AnchorEnd,
                                         curData));

            return pos;
        }

        /**
         * Set the variable range to [start, end] (inclusive).
         */
        private void SetVariableRange(int start, int end)
        {
            if (start > end || start < 0 || end > 0xFFFF)
            {
                throw new IcuArgumentException("Invalid variable range " + start + ", " + end);
            }

            curData.variablesBase = (char)start; // first private use

            if (DataVector.Count == 0)
            {
                variableNext = (char)start;
                variableLimit = (char)(end + 1);
            }
        }

        /**
         * Assert that the given character is NOT within the variable range.
         * If it is, signal an error.  This is neccesary to ensure that the
         * variable range does not overlap characters used in a rule.
         */
        private void CheckVariableRange(int ch, string rule, int start)
        {
            if (ch >= curData.variablesBase && ch < variableLimit)
            {
                SyntaxError("Variable range character in rule", rule, start);
            }
        }

        // (The following method is part of an unimplemented feature.
        // Remove this clover pragma after the feature is implemented.
        // 2003-06-11 ICU 2.6 Alan)
        ///CLOVER:OFF
        /**
         * Set the maximum backup to 'backup', in response to a pragma
         * statement.
         */
        private void PragmaMaximumBackup(int backup)
        {
            //TODO Finish
            throw new IcuArgumentException("use maximum backup pragma not implemented yet");
        }
        ///CLOVER:ON

        // (The following method is part of an unimplemented feature.
        // Remove this clover pragma after the feature is implemented.
        // 2003-06-11 ICU 2.6 Alan)
        ///CLOVER:OFF
        /**
         * Begin normalizing all rules using the given mode, in response
         * to a pragma statement.
         */
        private void PragmaNormalizeRules(Normalizer.Mode mode)
        {
            //TODO Finish
            throw new IcuArgumentException("use normalize rules pragma not implemented yet");
        }
        ///CLOVER:ON

        /**
         * Return true if the given rule looks like a pragma.
         * @param pos offset to the first non-whitespace character
         * of the rule.
         * @param limit pointer past the last character of the rule.
         */
        internal static bool ResemblesPragma(string rule, int pos, int limit)
        {
            // Must start with /use\s/i
            return Utility.ParsePattern(rule, pos, limit, "use ", null) >= 0;
        }

        /**
         * Parse a pragma.  This method assumes resemblesPragma() has
         * already returned true.
         * @param pos offset to the first non-whitespace character
         * of the rule.
         * @param limit pointer past the last character of the rule.
         * @return the position index after the final ';' of the pragma,
         * or -1 on failure.
         */
        private int ParsePragma(string rule, int pos, int limit)
        {
            int[] array = new int[2];

            // resemblesPragma() has already returned true, so we
            // know that pos points to /use\s/i; we can skip 4 characters
            // immediately
            pos += 4;

            // Here are the pragmas we recognize:
            // use variable range 0xE000 0xEFFF;
            // use maximum backup 16;
            // use nfd rules;
            int p = Utility.ParsePattern(rule, pos, limit, "~variable range # #~;", array);
            if (p >= 0)
            {
                SetVariableRange(array[0], array[1]);
                return p;
            }

            p = Utility.ParsePattern(rule, pos, limit, "~maximum backup #~;", array);
            if (p >= 0)
            {
                PragmaMaximumBackup(array[0]);
                return p;
            }

            p = Utility.ParsePattern(rule, pos, limit, "~nfd rules~;", null);
            if (p >= 0)
            {
                PragmaNormalizeRules(Normalizer.NFD);
                return p;
            }

            p = Utility.ParsePattern(rule, pos, limit, "~nfc rules~;", null);
            if (p >= 0)
            {
                PragmaNormalizeRules(Normalizer.NFC);
                return p;
            }

            // Syntax error: unable to parse pragma
            return -1;
        }

        /**
         * Throw an exception indicating a syntax error.  Search the rule string
         * for the probable end of the rule.  Of course, if the error is that
         * the end of rule marker is missing, then the rule end will not be found.
         * In any case the rule start will be correctly reported.
         * @param msg error description
         * @param rule pattern string
         * @param start position of first character of current rule
         */
        internal static void SyntaxError(String msg, String rule, int start)
        {
            int end = RuleEnd(rule, start, rule.Length);
            throw new IcuArgumentException(msg + " in \"" +
                                               Utility.Escape(rule.Substring(start, end - start)) + '"'); // ICU4N: Corrected 2nd substring parameter
        }

        internal static int RuleEnd(string rule, int start, int limit)
        {
            int end = Utility.QuotedIndexOf(rule, start, limit, ";");
            if (end < 0)
            {
                end = limit;
            }
            return end;
        }

        /**
         * Parse a UnicodeSet out, store it, and return the stand-in character
         * used to represent it.
         */
        private char ParseSet(string rule, ParsePosition pos)
        {
            UnicodeSet set = new UnicodeSet(rule, pos, parseData);
            if (variableNext >= variableLimit)
            {
                throw new Exception("Private use variables exhausted");
            }
            set.Compact();
            return GenerateStandInFor(set);
        }

        /**
         * Generate and return a stand-in for a new UnicodeMatcher or UnicodeReplacer.
         * Store the object.
         */
        internal virtual char GenerateStandInFor(object obj)
        {
            // assert(obj != null);

            // Look up previous stand-in, if any.  This is a short list
            // (typical n is 0, 1, or 2); linear search is optimal.
            for (int i = 0; i < variablesVector.Count; ++i)
            {
                if (variablesVector[i] == obj)
                { // [sic] pointer comparison
                    return (char)(curData.variablesBase + i);
                }
            }

            if (variableNext >= variableLimit)
            {
                throw new Exception("Variable range exhausted");
            }
            variablesVector.Add(obj);
            return variableNext++;
        }

        /**
         * Return the standin for segment seg (1-based).
         */
        public virtual char GetSegmentStandin(int seg)
        {
            if (segmentStandins.Length < seg)
            {
                segmentStandins.Length = seg;
            }
            char c = segmentStandins[seg - 1];
            if (c == 0)
            {
                if (variableNext >= variableLimit)
                {
                    throw new Exception("Variable range exhausted");
                }
                c = variableNext++;
                // Set a placeholder in the master variables vector that will be
                // filled in later by setSegmentObject().  We know that we will get
                // called first because setSegmentObject() will call us.
                variablesVector.Add(null);
                segmentStandins[seg - 1] = c;
            }
            return c;
        }

        /**
         * Set the object for segment seg (1-based).
         */
        public virtual void SetSegmentObject(int seg, StringMatcher obj)
        {
            // Since we call parseSection() recursively, nested
            // segments will result in segment i+1 getting parsed
            // and stored before segment i; be careful with the
            // vector handling here.
            while (segmentObjects.Count < seg)
            {
                segmentObjects.Add(null);
            }
            int index = GetSegmentStandin(seg) - curData.variablesBase;
            if (segmentObjects[seg - 1] != null ||
                variablesVector[index] != null)
            {
                throw new Exception(); // should never happen
            }
            segmentObjects[seg - 1] = obj;
            variablesVector[index] = obj;
        }

        /**
         * Return the stand-in for the dot set.  It is allocated the first
         * time and reused thereafter.
         */
        internal virtual char GetDotStandIn()
        {
            if (dotStandIn == -1)
            {
                dotStandIn = GenerateStandInFor(new UnicodeSet(DOT_SET));
            }
            return (char)dotStandIn;
        }

        /**
         * Append the value of the given variable name to the given
         * StringBuffer.
         * @exception IllegalIcuArgumentException if the name is unknown.
         */
        private void AppendVariableDef(string name, StringBuffer buf)
        {
            char[] ch = variableNames.Get(name);
            if (ch == null)
            {
                // We allow one undefined variable so that variable definition
                // statements work.  For the first undefined variable we return
                // the special placeholder variableLimit-1, and save the variable
                // name.
                if (undefinedVariableName == null)
                {
                    undefinedVariableName = name;
                    if (variableNext >= variableLimit)
                    {
                        throw new Exception("Private use variables exhausted");
                    }
                    buf.Append(--variableLimit);
                }
                else
                {
                    throw new IcuArgumentException("Undefined variable $"
                                                       + name);
                }
            }
            else
            {
                buf.Append(ch);
            }
        }
    }
}
