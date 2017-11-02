using ICU4N.Support.Text;
using ICU4N.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Impl
{
    /// <summary>
    /// An iterator that returns 32-bit code points.  This class is deliberately
    /// <em>not</em> related to any of the JDK or ICU4J character iterator classes
    /// in order to minimize complexity.
    /// </summary>
    /// <author>Alan Liu</author>
    /// <since>ICU 2.8</since>
    public class RuleCharacterIterator
    {
        // TODO: Ideas for later.  (Do not implement if not needed, lest the
        // code coverage numbers go down due to unused methods.)
        // 1. Add a copy constructor, equals() method, clone() method.
        // 2. Rather than return DONE, throw an exception if the end
        // is reached -- this is an alternate usage model, probably not useful.
        // 3. Return isEscaped from next().  If this happens,
        // don't keep an isEscaped member variable.

        /**
         * Text being iterated.
         */
        private string text;

        /**
         * Position of iterator.
         */
        private ParsePosition pos;

        /**
         * Symbol table used to parse and dereference variables.  May be null.
         */
        private ISymbolTable sym;

        /**
         * Current variable expansion, or null if none.
         */
        private char[] buf;

        /**
         * Position within buf[].  Meaningless if buf == null.
         */
        private int bufPos;

        /**
         * Flag indicating whether the last character was parsed from an escape.
         */
        private bool isEscaped;

        /**
         * Value returned when there are no more characters to iterate.
         */
        public static readonly int DONE = -1;

        /**
         * Bitmask option to enable parsing of variable names.  If (options &
         * PARSE_VARIABLES) != 0, then an embedded variable will be expanded to
         * its value.  Variables are parsed using the SymbolTable API.
         */
        public static readonly int PARSE_VARIABLES = 1;

        /**
         * Bitmask option to enable parsing of escape sequences.  If (options &
         * PARSE_ESCAPES) != 0, then an embedded escape sequence will be expanded
         * to its value.  Escapes are parsed using Utility.unescapeAt().
         */
        public static readonly int PARSE_ESCAPES = 2;

        /**
         * Bitmask option to enable skipping of whitespace.  If (options &
         * SKIP_WHITESPACE) != 0, then Unicode Pattern_White_Space characters will be silently
         * skipped, as if they were not present in the input.
         */
        public static readonly int SKIP_WHITESPACE = 4;

        /**
         * Constructs an iterator over the given text, starting at the given
         * position.
         * @param text the text to be iterated
         * @param sym the symbol table, or null if there is none.  If sym is null,
         * then variables will not be deferenced, even if the PARSE_VARIABLES
         * option is set.
         * @param pos upon input, the index of the next character to return.  If a
         * variable has been dereferenced, then pos will <em>not</em> increment as
         * characters of the variable value are iterated.
         */
        public RuleCharacterIterator(string text, ISymbolTable sym,
                                     ParsePosition pos)
        {
            if (text == null || pos.Index > text.Length)
            {
                throw new ArgumentException();
            }
            this.text = text;
            this.sym = sym;
            this.pos = pos;
            buf = null;
        }

        /**
         * Returns true if this iterator has no more characters to return.
         */
        public bool AtEnd
        {
            get { return buf == null && pos.Index == text.Length; }
        }

        /**
         * Returns the next character using the given options, or DONE if there
         * are no more characters, and advance the position to the next
         * character.
         * @param options one or more of the following options, bitwise-OR-ed
         * together: PARSE_VARIABLES, PARSE_ESCAPES, SKIP_WHITESPACE.
         * @return the current 32-bit code point, or DONE
         */
        public int Next(int options)
        {
            int c = DONE;
            isEscaped = false;

            for (; ; )
            {
                c = _current();
                _advance(UTF16.GetCharCount(c));

                if (c == SymbolTable.SYMBOL_REF && buf == null &&
                    (options & PARSE_VARIABLES) != 0 && sym != null)
                {
                    String name = sym.ParseReference(text, pos, text.Length);
                    // If name == null there was an isolated SYMBOL_REF;
                    // return it.  Caller must be prepared for this.
                    if (name == null)
                    {
                        break;
                    }
                    bufPos = 0;
                    buf = sym.Lookup(name);
                    if (buf == null)
                    {
                        throw new ArgumentException(
                                    "Undefined variable: " + name);
                    }
                    // Handle empty variable value
                    if (buf.Length == 0)
                    {
                        buf = null;
                    }
                    continue;
                }

                if ((options & SKIP_WHITESPACE) != 0 &&
                    PatternProps.IsWhiteSpace(c))
                {
                    continue;
                }

                if (c == '\\' && (options & PARSE_ESCAPES) != 0)
                {
                    int[] offset = new int[] { 0 };
                    c = Utility.UnescapeAt(Lookahead(), offset);
                    Jumpahead(offset[0]);
                    isEscaped = true;
                    if (c < 0)
                    {
                        throw new ArgumentException("Invalid escape");
                    }
                }

                break;
            }

            return c;
        }

        /**
         * Returns true if the last character returned by next() was
         * escaped.  This will only be the case if the option passed in to
         * next() included PARSE_ESCAPED and the next character was an
         * escape sequence.
         */
        public bool IsEscaped
        {
            get { return isEscaped; }
        }

        /**
         * Returns true if this iterator is currently within a variable expansion.
         */
        public bool InVariable
        {
            get { return buf != null; }
        }

        /**
         * Returns an object which, when later passed to setPos(), will
         * restore this iterator's position.  Usage idiom:
         *
         * RuleCharacterIterator iterator = ...;
         * Object pos = iterator.getPos(null); // allocate position object
         * for (;;) {
         *   pos = iterator.getPos(pos); // reuse position object
         *   int c = iterator.next(...);
         *   ...
         * }
         * iterator.setPos(pos);
         *
         * @param p a position object previously returned by getPos(),
         * or null.  If not null, it will be updated and returned.  If
         * null, a new position object will be allocated and returned.
         * @return a position object which may be passed to setPos(),
         * either `p,' or if `p' == null, a newly-allocated object
         */
        public object GetPos(object p)
        {
            if (p == null)
            {
                return new object[] { buf, new int[] { pos.Index, bufPos } };
            }
            Object[] a = (Object[])p;
            a[0] = buf;
            int[] v = (int[])a[1];
            v[0] = pos.Index;
            v[1] = bufPos;
            return p;
        }

        /**
         * Restores this iterator to the position it had when getPos()
         * returned the given object.
         * @param p a position object previously returned by getPos()
         */
        public void SetPos(object p)
        {
            object[] a = (object[])p;
            buf = (char[])a[0];
            int[] v = (int[])a[1];
            pos.Index = v[0];
            bufPos = v[1];
        }

        /**
         * Skips ahead past any ignored characters, as indicated by the given
         * options.  This is useful in conjunction with the lookahead() method.
         *
         * Currently, this only has an effect for SKIP_WHITESPACE.
         * @param options one or more of the following options, bitwise-OR-ed
         * together: PARSE_VARIABLES, PARSE_ESCAPES, SKIP_WHITESPACE.
         */
        public void SkipIgnored(int options)
        {
            if ((options & SKIP_WHITESPACE) != 0)
            {
                for (; ; )
                {
                    int a = _current();
                    if (!PatternProps.IsWhiteSpace(a)) break;
                    _advance(UTF16.GetCharCount(a));
                }
            }
        }

        /**
         * Returns a string containing the remainder of the characters to be
         * returned by this iterator, without any option processing.  If the
         * iterator is currently within a variable expansion, this will only
         * extend to the end of the variable expansion.  This method is provided
         * so that iterators may interoperate with string-based APIs.  The typical
         * sequence of calls is to call skipIgnored(), then call lookahead(), then
         * parse the string returned by lookahead(), then call jumpahead() to
         * resynchronize the iterator.
         * @return a string containing the characters to be returned by future
         * calls to next()
         */
        public string Lookahead()
        {
            if (buf != null)
            {
                return new string(buf, bufPos, buf.Length - bufPos);
            }
            else
            {
                return text.Substring(pos.Index);
            }
        }

        /**
         * Advances the position by the given number of 16-bit code units.
         * This is useful in conjunction with the lookahead() method.
         * @param count the number of 16-bit code units to jump over
         */
        public void Jumpahead(int count)
        {
            if (count < 0)
            {
                throw new ArgumentException();
            }
            if (buf != null)
            {
                bufPos += count;
                if (bufPos > buf.Length)
                {
                    throw new ArgumentException();
                }
                if (bufPos == buf.Length)
                {
                    buf = null;
                }
            }
            else
            {
                int i = pos.Index + count;
                pos.Index = i;
                if (i > text.Length)
                {
                    throw new ArgumentException();
                }
            }
        }

        /**
         * Returns a string representation of this object, consisting of the
         * characters being iterated, with a '|' marking the current position.
         * Position within an expanded variable is <em>not</em> indicated.
         * @return a string representation of this object
         */
        public override string ToString()
        {
            int b = pos.Index;
            return text.Substring(0, b - 0) + '|' + text.Substring(b);
        }

        /**
         * Returns the current 32-bit code point without parsing escapes, parsing
         * variables, or skipping whitespace.
         * @return the current 32-bit code point
         */
        private int _current()
        {
            if (buf != null)
            {
                return UTF16.CharAt(buf, 0, buf.Length, bufPos);
            }
            else
            {
                int i = pos.Index;
                return (i < text.Length) ? UTF16.CharAt(text, i) : DONE;
            }
        }

        /**
         * Advances the position by the given amount.
         * @param count the number of 16-bit code units to advance past
         */
        private void _advance(int count)
        {
            if (buf != null)
            {
                bufPos += count;
                if (bufPos == buf.Length)
                {
                    buf = null;
                }
            }
            else
            {
                pos.Index += count;
                if (pos.Index > text.Length)
                {
                    pos.Index = text.Length;
                }
            }
        }
    }
}
