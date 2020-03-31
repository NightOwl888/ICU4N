using ICU4N.Text;
using J2N.Text;
using System;
using J2N.Text;

namespace ICU4N.Impl
{
    /// <summary>
    /// Options for the <see cref="RuleCharacterIterator"/>.
    /// </summary>
    [Flags]
    public enum RuleCharacterIteratorOptions
    {
        /// <summary>
        /// Bitmask option to enable parsing of variable names.  If (options &amp;
        /// <see cref="ParseVariables"/> != 0, then an embedded variable will be expanded to
        /// its value.  Variables are parsed using the <see cref="SymbolTable"/> API.
        /// </summary>
        ParseVariables = 1,
        /// <summary>
        /// Bitmask option to enable parsing of escape sequences.  If (options &amp;
        /// <see cref="ParseEscapes"/> != 0, then an embedded escape sequence will be expanded
        /// to its value.  Escapes are parsed using <see cref="Utility.UnescapeAt(string, int[])"/>.
        /// </summary>
        ParseEscapes = 2,
        /// <summary>
        /// Bitmask option to enable skipping of whitespace.  If (options &amp;
        /// <see cref="SkipWhitespace"/> != 0, then Unicode Pattern_White_Space characters will be silently
        /// skipped, as if they were not present in the input.
        /// </summary>
        SkipWhitespace = 4
    }

    /// <summary>
    /// An iterator that returns 32-bit code points.  This class is deliberately
    /// <em>not</em> related to any of the .NET or ICU4N character iterator classes
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

        /// <summary>
        /// Text being iterated.
        /// </summary>
        private string text;

        /// <summary>
        /// Position of iterator.
        /// </summary>
        private ParsePosition pos;

        /// <summary>
        /// Symbol table used to parse and dereference variables.  May be null.
        /// </summary>
        private ISymbolTable sym;

        /// <summary>
        /// Current variable expansion, or null if none.
        /// </summary>
        private char[] buf;

        /// <summary>
        /// Position within buf[].  Meaningless if buf == null.
        /// </summary>
        private int bufPos;

        /// <summary>
        /// Flag indicating whether the last character was parsed from an escape.
        /// </summary>
        private bool isEscaped;

        /// <summary>
        /// Value returned when there are no more characters to iterate.
        /// </summary>
        public const int Done = -1;

        // ICU4N specific - PARSE_VARIABLES, PARSE_ESCAPES, and SKIP_WHITESPACE
        // moved to [Flags] enum RuleCharacterIteratorOptions

        /// <summary>
        /// Constructs an iterator over the given text, starting at the given
        /// position.
        /// </summary>
        /// <param name="text">The text to be iterated.</param>
        /// <param name="sym">The symbol table, or null if there is none.  If sym is null,
        /// then variables will not be deferenced, even if the <see cref="RuleCharacterIteratorOptions.ParseVariables"/>
        /// option is set.</param>
        /// <param name="pos">Upon input, the index of the next character to return.  If a
        /// variable has been dereferenced, then pos will <em>not</em> increment as
        /// characters of the variable value are iterated.</param>
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

        /// <summary>
        /// Returns true if this iterator has no more characters to return.
        /// </summary>
        public virtual bool AtEnd
        {
            get { return buf == null && pos.Index == text.Length; }
        }

        /// <summary>
        /// Returns the next character using the given options, or <see cref="Done"/> if there
        /// are no more characters, and advance the position to the next
        /// character.
        /// </summary>
        /// <param name="options">One or more of the following options, bitwise-OR-ed
        /// together: <see cref="RuleCharacterIteratorOptions.ParseVariables"/>, 
        /// <see cref="RuleCharacterIteratorOptions.ParseEscapes"/>, 
        /// <see cref="RuleCharacterIteratorOptions.SkipWhitespace"/>.</param>
        /// <returns>The current 32-bit code point, or <see cref="Done"/>.</returns>
        public virtual int Next(RuleCharacterIteratorOptions options)
        {
            int c = Done;
            isEscaped = false;

            for (; ; )
            {
                c = Current();
                Advance(UTF16.GetCharCount(c));

                if (c == SymbolTable.SymbolReference && buf == null &&
                    (options & RuleCharacterIteratorOptions.ParseVariables) != 0 && sym != null)
                {
                    string name = sym.ParseReference(text, pos, text.Length);
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

                if ((options & RuleCharacterIteratorOptions.SkipWhitespace) != 0 &&
                    PatternProps.IsWhiteSpace(c))
                {
                    continue;
                }

                if (c == '\\' && (options & RuleCharacterIteratorOptions.ParseEscapes) != 0)
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

        /// <summary>
        /// Returns true if the last character returned by <see cref="Next(RuleCharacterIteratorOptions)"/> was
        /// escaped.  This will only be the case if the option passed in to
        /// <see cref="Next(RuleCharacterIteratorOptions)"/> included 
        /// <see cref="RuleCharacterIteratorOptions.ParseEscapes"/> and the next character was an
        /// escape sequence.
        /// </summary>
        public virtual bool IsEscaped
        {
            get { return isEscaped; }
        }

        /// <summary>
        /// Returns true if this iterator is currently within a variable expansion.
        /// </summary>
        public virtual bool InVariable
        {
            get { return buf != null; }
        }

        /// <summary>
        /// Returns an object which, when later passed to <see cref="SetPos(object)"/>, will
        /// restore this iterator's position.
        /// </summary>
        /// <remarks>
        /// Usage idiom:
        /// <code>
        /// RuleCharacterIterator iterator = ...;
        /// object pos = iterator.GetPos(null); // allocate position object
        /// for (;;) 
        /// {
        ///     pos = iterator.GetPos(pos); // reuse position object
        ///     int c = iterator.Next(...);
        /// }
        /// </code>
        /// </remarks>
        /// <param name="p">A position object previously returned by <see cref="GetPos(object)"/>,
        /// or null.  If not null, it will be updated and returned.  If
        /// null, a new position object will be allocated and returned.</param>
        /// <returns>A position object which may be passed to <see cref="SetPos(object)"/>,
        /// either `p,' or if `p' == null, a newly-allocated object.</returns>
        public virtual object GetPos(object p)
        {
            if (p == null)
            {
                return new object[] { buf, new int[] { pos.Index, bufPos } };
            }
            object[] a = (object[])p;
            a[0] = buf;
            int[] v = (int[])a[1];
            v[0] = pos.Index;
            v[1] = bufPos;
            return p;
        }

        /// <summary>
        /// Restores this iterator to the position it had when <see cref="GetPos(object)"/>
        /// returned the given object.
        /// </summary>
        /// <param name="p">A position object previously returned by <see cref="GetPos(object)"/>.</param>
        public virtual void SetPos(object p)
        {
            object[] a = (object[])p;
            buf = (char[])a[0];
            int[] v = (int[])a[1];
            pos.Index = v[0];
            bufPos = v[1];
        }

        /// <summary>
        /// Skips ahead past any ignored characters, as indicated by the given
        /// options.  This is useful in conjunction with the <see cref="Lookahead()"/> method.
        /// <para/>
        /// Currently, this only has an effect for <see cref="RuleCharacterIteratorOptions.SkipWhitespace"/>.
        /// </summary>
        /// <param name="options">One or more of the following options, bitwise-OR-ed
        /// together: <see cref="RuleCharacterIteratorOptions.ParseVariables"/>, 
        /// <see cref="RuleCharacterIteratorOptions.ParseEscapes"/>, 
        /// <see cref="RuleCharacterIteratorOptions.SkipWhitespace"/>.</param>
        public virtual void SkipIgnored(RuleCharacterIteratorOptions options)
        {
            if ((options & RuleCharacterIteratorOptions.SkipWhitespace) != 0)
            {
                for (; ; )
                {
                    int a = Current();
                    if (!PatternProps.IsWhiteSpace(a)) break;
                    Advance(UTF16.GetCharCount(a));
                }
            }
        }

        /// <summary>
        /// Returns a string containing the remainder of the characters to be
        /// returned by this iterator, without any option processing.  
        /// </summary>
        /// <remarks>
        /// If the iterator is currently within a variable expansion, this will only
        /// extend to the end of the variable expansion.  This method is provided
        /// so that iterators may interoperate with string-based APIs.  The typical
        /// sequence of calls is to call <see cref="SkipIgnored(RuleCharacterIteratorOptions)"/>, then call <see cref="Lookahead()"/>, then
        /// parse the string returned by <see cref="Lookahead()"/>, then call <see cref="Jumpahead(int)"/> to
        /// resynchronize the iterator.
        /// </remarks>
        /// <returns>A string containing the characters to be returned by future
        /// calls to <see cref="Next(RuleCharacterIteratorOptions)"/>.</returns>
        public virtual string Lookahead()
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

        /// <summary>
        /// Advances the position by the given number of 16-bit code units.
        /// This is useful in conjunction with the <see cref="Lookahead()"/> method.
        /// </summary>
        /// <param name="count">The number of 16-bit code units to jump over.</param>
        public virtual void Jumpahead(int count)
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

        /// <summary>
        /// Returns a string representation of this object, consisting of the
        /// characters being iterated, with a '|' marking the current position.
        /// Position within an expanded variable is <em>not</em> indicated.
        /// </summary>
        /// <returns>A string representation of this object.</returns>
        public override string ToString()
        {
            int b = pos.Index;
            return text.Substring(0, b - 0) + '|' + text.Substring(b);
        }

        /// <summary>
        /// Returns the current 32-bit code point without parsing escapes, parsing
        /// variables, or skipping whitespace.
        /// </summary>
        /// <returns>The current 32-bit code point.</returns>
        private int Current()
        {
            if (buf != null)
            {
                return UTF16.CharAt(buf, 0, buf.Length, bufPos);
            }
            else
            {
                int i = pos.Index;
                return (i < text.Length) ? UTF16.CharAt(text, i) : Done;
            }
        }

        /// <summary>
        /// Advances the position by the given amount.
        /// </summary>
        /// <param name="count">The number of 16-bit code units to advance past.</param>
        private void Advance(int count)
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
