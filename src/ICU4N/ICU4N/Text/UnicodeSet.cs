using ICU4N.Impl;
using ICU4N.Lang;
using ICU4N.Support;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using ICU4N.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// A mutable set of Unicode characters and multicharacter strings.
    /// Objects of this class represent <em>character classes</em> used
    /// in regular expressions. A character specifies a subset of Unicode
    /// code points.  Legal code points are U+0000 to U+10FFFF, inclusive.
    /// </summary>
    /// <remarks>
    /// Note: method <see cref="Freeze()"/> will not only make the set immutable, but
    /// also makes important methods much higher performance:
    /// <see cref="Contains(int)"/>, <see cref="ContainsNone(string)"/>, 
    /// <see cref="Span(string, int, SpanCondition)"/>,
    /// <see cref="SpanBack(string, int, SpanCondition)"/>, etc.
    /// After the object is frozen, any subsequent call that wants to change
    /// the object will throw <see cref="NotSupportedException"/>.
    /// <para/>
    /// The <see cref="UnicodeSet"/> class is not designed to be subclassed.
    /// <para/>
    /// <see cref="UnicodeSet"/> supports two APIs. The first is the
    /// <em>operand</em> API that allows the caller to modify the value of
    /// a <code>UnicodeSet</code> object. It conforms to .NET's <see cref="ISet{T}"/>
    /// interface, although <see cref="UnicodeSet"/> does not actually implement that
    /// interface. All methods of <see cref="ISet{T}"/> are supported, with the
    /// modification that they take a character range or single character
    /// instead of a <see cref="string"/>, and they take a
    /// <see cref="UnicodeSet"/> instead of a <see cref="ICollection{T}"/>. The
    /// operand API may be thought of in terms of boolean logic: a boolean
    /// OR is implemented by <see cref="Add(string)"/>, a boolean AND is implemented
    /// by <see cref="Retain(string)"/>, a boolean XOR is implemented by
    /// <see cref="Complement(string)"/> taking an argument, and a boolean NOT is
    /// implemented by <see cref="Complement()"/> with no argument.  In terms
    /// of traditional set theory function names, <see cref="Add(string)"/> is a
    /// union, <see cref="Retain(string)"/> is an intersection, <see cref="Remove(string)"/>
    /// is an asymmetric difference, and <see cref="Complement()"/> with no
    /// argument is a set complement with respect to the superset range
    /// <c><see cref="MinValue"/>-<see cref="MaxValue"/></c>.
    /// <para/>
    /// The second API is the
    /// <see cref="ApplyPattern(string)"/>/<see cref="ToPattern(StringBuilder, bool)"/> API from the
    /// <c>java.text.Format</c>-derived classes.  Unlike the
    /// methods that add characters, add categories, and control the logic
    /// of the set, the method <see cref="ApplyPattern(string)"/> sets all
    /// attributes of a <see cref="UnicodeSet"/> at once, based on a
    /// string pattern.
    /// 
    /// <para/>
    /// <b>Pattern syntax</b>
    /// 
    /// <para/>
    /// Patterns are accepted by the constructors and the
    /// <see cref="ApplyPattern(string)"/> methods and returned by the
    /// <see cref="ToPattern(StringBuilder, bool)"/> method.  These patterns follow a syntax
    /// similar to that employed by .NET regular expression character
    /// classes.  Here are some simple examples:
    /// 
    /// <list type="table">
    ///     <item>
    ///         <term><c>[]</c></term>
    ///         <term>No characters</term>
    ///     </item>
    ///     <item>
    ///         <term><c>[a]</c></term>
    ///         <term>The character 'a'</term>
    ///     </item>
    ///     <item>
    ///         <term><c>[ae]</c></term>
    ///         <term>The characters 'a' and 'e'</term>
    ///     </item>
    ///     <item>
    ///         <term><c>[a-e]</c></term>
    ///         <term>The characters 'a' through 'e' inclusive, in Unicode code
    ///         point order</term>
    ///     </item>
    ///     <item>
    ///         <term><c>[\\u4E01]</c></term>
    ///         <term>The character U+4E01</term>
    ///     </item>
    ///     <item>
    ///         <term><c>[a{ab}{ac}]</c></term>
    ///         <term>The character 'a' and the multicharacter strings &quot;ab&quot; and
    ///         &quot;ac&quot;</term>
    ///     </item>
    ///     <item>
    ///         <term><c>[\p{Lu}]</c></term>
    ///         <term>All characters in the general category Uppercase Letter</term>
    ///     </item>
    /// </list>
    /// 
    /// <para/>
    /// Any character may be preceded by a backslash in order to remove any special
    /// meaning.  White space characters, as defined by the Unicode Pattern_White_Space property, are
    /// ignored, unless they are escaped.
    /// 
    /// <para/>
    /// Property patterns specify a set of characters having a certain
    /// property as defined by the Unicode standard.  Both the POSIX-like
    /// "[:Lu:]" and the Perl-like syntax "\p{Lu}" are recognized.  For a
    /// complete list of supported property patterns, see the User's Guide
    /// for UnicodeSet at
    /// <a href="http://www.icu-project.org/userguide/unicodeSet.html">
    /// http://www.icu-project.org/userguide/unicodeSet.html</a>.
    /// Actual determination of property data is defined by the underlying
    /// Unicode database as implemented by <see cref="UCharacter"/>.
    /// 
    /// <para/>
    /// Patterns specify individual characters, ranges of characters, and
    /// Unicode property sets.  When elements are concatenated, they
    /// specify their union.  To complement a set, place a '^' immediately
    /// after the opening '['.  Property patterns are inverted by modifying
    /// their delimiters; "[:^foo]" and "\P{foo}".  In any other location,
    /// '^' has no special meaning.
    /// 
    /// <para/>
    /// Ranges are indicated by placing two a '-' between two
    /// characters, as in "a-z".  This specifies the range of all
    /// characters from the left to the right, in Unicode order.  If the
    /// left character is greater than or equal to the
    /// right character it is a syntax error.  If a '-' occurs as the first
    /// character after the opening '[' or '[^', or if it occurs as the
    /// last character before the closing ']', then it is taken as a
    /// literal.  Thus "[a\\-b]", "[-ab]", and "[ab-]" all indicate the same
    /// set of three characters, 'a', 'b', and '-'.
    /// 
    /// <para/>
    /// Sets may be intersected using the '&amp;' operator or the asymmetric
    /// set difference may be taken using the '-' operator, for example,
    /// "[[:L:]&amp;[\\u0000-\\u0FFF]]" indicates the set of all Unicode letters
    /// with values less than 4096.  Operators ('&amp;' and '|') have equal
    /// precedence and bind left-to-right.  Thus
    /// "[[:L:]-[a-z]-[\\u0100-\\u01FF]]" is equivalent to
    /// "[[[:L:]-[a-z]]-[\\u0100-\\u01FF]]".  This only really matters for
    /// difference; intersection is commutative.
    /// 
    /// <list type="table">
    ///     <item>
    ///         <term><c>[a]</c></term>
    ///         <term>The set containing 'a'</term>
    ///     </item>
    ///     <item>
    ///         <term><c>[a-z]</c></term>
    ///         <term>The set containing 'a'
    ///         through 'z' and all letters in between, in Unicode order</term>
    ///     </item>
    ///     <item>
    ///         <term><c>[^a-z]</c></term>
    ///         <term>The set containing
    ///         all characters but 'a' through 'z',
    ///         that is, U+0000 through 'a'-1 and 'z'+1 through U+10FFFF</term>
    ///     </item>
    ///     <item>
    ///         <term><c>[[<em>pat1</em>][<em>pat2</em>]]</c></term>
    ///         <term>The union of sets specified by <em>pat1</em> and <em>pat2</em></term>
    ///     </item>
    ///     <item>
    ///         <term><c>[[<em>pat1</em>]&amp;[<em>pat2</em>]]</c></term>
    ///         <term>The intersection of sets specified by <em>pat1</em> and <em>pat2</em></term>
    ///     </item>
    ///     <item>
    ///         <term><c>[[<em>pat1</em>]-[<em>pat2</em>]]</c></term>
    ///         <term>The asymmetric difference of sets specified by <em>pat1</em> and <em>pat2</em></term>
    ///     </item>
    ///     <item>
    ///         <term><c>[:Lu:] or \p{Lu}</c></term>
    ///         <term>The set of characters having the specified
    ///         Unicode property; in
    ///         this case, Unicode uppercase letters</term>
    ///     </item>
    ///     <item>
    ///         <term><c>[:^Lu:] or \P{Lu}</c></term>
    ///         <term>The set of characters <em>not</em> having the given
    ///         Unicode property</term>
    ///     </item>
    /// </list>
    /// 
    /// <para/>
    /// <b>Warning</b>: you cannot add an empty string ("") to a UnicodeSet.
    /// 
    /// <para/>
    /// <b>Formal syntax</b>
    /// 
    /// <list type="table">
    ///     <item>
    ///         <term><c>pattern :=&amp;nbsp; </c></term>
    ///         <term><c>('[' '^'? item* ']') | property</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>item :=&amp;nbsp; </c></term>
    ///         <term><c>char | (char '-' char) | pattern-expr<br/></c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>pattern-expr :=&amp;nbsp; </c></term>
    ///         <term><c>pattern | pattern-expr pattern | pattern-expr op pattern<br/></c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>op :=&amp;nbsp; </c></term>
    ///         <term><c>'&amp;' | '-'<br/></c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>special :=&amp;nbsp; </c></term>
    ///         <term><c>'[' | ']' | '-'<br/></c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>char :=&amp;nbsp; </c></term>
    ///         <term><em>any character that is not</em><c> special<br/>
    ///         | ('\\' </c><em>any character</em><c>)<br/>
    ///         | ('&#92;u' hex hex hex hex)<br/>
    ///         </c></term>
    ///     </item>
    ///     <item>
    ///         <term><c>hex :=&amp;nbsp; </c></term>
    ///         <term><em>any character for which
    ///         </em><c>Character.Digit(c, 16)</c><em>
    ///         returns a non-negative result</em></term>
    ///     </item>
    ///     <item>
    ///         <term><c>property :=&amp;nbsp; </c></term>
    ///         <term><em>a Unicode property set pattern</em></term>
    ///     </item>
    /// </list>
    /// 
    /// <list type="table">
    ///     <item>
    ///         <term>Legend:
    ///             <list type="table">
    ///                 <item>
    ///                     <term><c>a := b</c></term>
    ///                     <term>&#160;</term>
    ///                     <term><c>a</c> may be replaced by <c>b</c></term>
    ///                 </item>
    ///                 <item>
    ///                     <term><c>a?</c></term>
    ///                     <term></term>
    ///                     <term>zero or one instance of <c>a</c></term>
    ///                 </item>
    ///                 <item>
    ///                     <term><c>a*</c></term>
    ///                     <term></term>
    ///                     <term>one or more instances of <c>a</c></term>
    ///                 </item>
    ///                 <item>
    ///                     <term><c>a | b</c></term>
    ///                     <term></term>
    ///                     <term>either <c>a</c> or <c>b</c></term>
    ///                 </item>
    ///                 <item>
    ///                     <term><c>'a'</c></term>
    ///                     <term></term>
    ///                     <term>the literal string between the quotes</term>
    ///                 </item>
    ///             </list>
    ///         </term>
    ///     </item>
    /// </list>
    /// 
    /// <para/>
    /// To iterate over contents of <see cref="UnicodeSet"/>, the following are available:
    /// <list type="bullet">
    ///     <item><description>
    ///         <see cref="Ranges"/> to iterate through the ranges
    ///     </description></item>
    ///     <item><description>
    ///         <see cref="Strings"/> to iterate through the strings
    ///     </description></item>
    ///     <item><description>
    ///         <see cref="GetEnumerator()"/> to iterate through the entire contents in a single loop.
    ///         That method is, however, not particularly efficient, since it "boxes" each code point into a <see cref="string"/>.
    ///     </description></item>
    /// </list>
    /// All of the above can be used in <b>for</b> loops.
    /// The <see cref="UnicodeSetIterator"/> can also be used, but not in <b>for</b> loops.
    /// 
    /// <para/>
    /// To replace, count elements, or delete spans, see <see cref="UnicodeSetSpanner"/>.
    /// </remarks>
    /// <seealso cref="UnicodeSetIterator"/>
    /// <seealso cref="UnicodeSetSpanner"/>
    /// <author>Alan Liu</author>
    /// <stable>ICU 2.0</stable>
    // ICU4N TODO: API - mark sealed (not sure why this wasn't done)
    // ICU4N TODO: API - change ToPattern() to ICustomFormatter.Format(string, object, IFormatProvider) ? Need to find corresponding API in .NET and change accordingly (see the "second API") in documentation above
    public partial class UnicodeSet : UnicodeFilter, IEnumerable<string>, IComparable<UnicodeSet>, IFreezable<UnicodeSet>
    {
        private static readonly object syncLock = new object();

        /// <summary>
        /// Constant for the empty set.
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public static readonly UnicodeSet Empty = new UnicodeSet().Freeze();

        /// <summary>
        /// Constant for the set of all code points. (Since <see cref="UnicodeSet"/>s can include strings, 
        /// does not include everything that a <see cref="UnicodeSet"/> can.)
        /// </summary>
        /// <stable>ICU 4.8</stable>
        public static readonly UnicodeSet AllCodePoints = new UnicodeSet(0, 0x10FFFF).Freeze();

        private static XSymbolTable XSYMBOL_TABLE = null; // for overriding the the function processing

        private const int LOW = 0x000000; // LOW <= all valid values. ZERO for codepoints
        private const int HIGH = 0x110000; // HIGH > all valid values. 10000 for code units.
                                           // 110000 for codepoints

        /// <summary>
        /// Minimum value that can be stored in a <see cref="UnicodeSet"/>.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public const int MinValue = LOW;

        /// <summary>
        /// Maximum value that can be stored in a <see cref="UnicodeSet"/>.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public const int MaxValue = HIGH - 1;

        private int len;      // length used; list may be longer to minimize reallocs
        private int[] list;   // MUST be terminated with HIGH
        private int[] rangeList; // internal buffer
        private int[] buffer; // internal buffer

        // NOTE: normally the field should be of type SortedSet; but that is missing a public clone!!
        // is not private so that UnicodeSetIterator can get access
        private SortedSet<string> strings = new SortedSet<string>(StringComparer.Ordinal);

        /// <summary>
        /// The pattern representation of this set.  This may not be the
        /// most economical pattern.  It is the pattern supplied to
        /// <see cref="ApplyPattern(string)"/>, with variables substituted and whitespace
        /// removed.  For sets constructed without <see cref="ApplyPattern(string)"/>, or
        /// modified using the non-pattern API, this string will be null,
        /// indicating that <see cref="ToPattern(StringBuilder, bool)"/> must generate a pattern
        /// representation from the inversion list.
        /// </summary>
        private string pat = null;

        private const int START_EXTRA = 16;         // initial storage. Must be >= 0
        private const int GROW_EXTRA = START_EXTRA; // extra amount for growth. Must be >= 0

        // Special property set IDs
        private const string ANY_ID = "ANY";   // [\u0000-\U0010FFFF]
        private const string ASCII_ID = "ASCII"; // [\u0000-\u007F]
        private const string ASSIGNED = "Assigned"; // [:^Cn:]

        /// <summary>
        /// A set of all characters _except_ the second through last characters of
        /// certain ranges.  These ranges are ranges of characters whose
        /// properties are all exactly alike, e.g. CJK Ideographs from
        /// U+4E00 to U+9FA5.
        /// </summary>
        private static UnicodeSet[] INCLUSIONS = null;

        private volatile BMPSet bmpSet; // The set is frozen if bmpSet or stringSpan is not null.
        private volatile UnicodeSetStringSpan stringSpan;
        //----------------------------------------------------------------
        // Public API
        //----------------------------------------------------------------
#pragma warning disable 612, 618

        /// <summary>
        /// Constructs an empty set.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet()
        {
            list = new int[1 + START_EXTRA];
            list[len++] = HIGH;
        }

        /// <summary>
        /// Constructs a copy of an existing set.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet(UnicodeSet other)
        {
            Set(other);
        }

        /// <summary>
        /// Constructs a set containing the given range. If <c><paramref name="end"/> &gt;
        /// <paramref name="start"/></c> then an empty set is created.
        /// </summary>
        /// <param name="start">First character, inclusive, of range.</param>
        /// <param name="end">Last character, inclusive, of range.</param>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet(int start, int end)
            : this()
        {
            Complement(start, end);
        }

        /// <summary>
        /// Quickly constructs a set from a set of ranges &lt;s0, e0, s1, e1, s2, e2, ..., sn, en&gt;.
        /// There must be an even number of integers, and they must be all greater than zero,
        /// all less than or equal to <see cref="Character.MAX_CODE_POINT"/>.
        /// In each pair (..., si, ei, ...) it must be true that si &lt;= ei
        /// Between adjacent pairs (...ei, sj...), it must be true that ei+1 &lt; sj.
        /// </summary>
        /// <param name="pairs">Pairs of character representing ranges.</param>
        /// <stable>ICU 4.4</stable>
        public UnicodeSet(params int[] pairs)
        {
            if ((pairs.Length & 1) != 0)
            {
                throw new ArgumentException("Must have even number of integers");
            }
            list = new int[pairs.Length + 1]; // don't allocate extra space, because it is likely that this is a fixed set.
            len = list.Length;
            int last = -1; // used to ensure that the results are monotonically increasing.
            int i = 0;
            while (i < pairs.Length)
            {
                // start of pair
                int start = pairs[i];
                if (last >= start)
                {
                    throw new ArgumentException("Must be monotonically increasing.");
                }
                list[i++] = last = start;
                // end of pair
                int end = pairs[i] + 1;
                if (last >= end)
                {
                    throw new ArgumentException("Must be monotonically increasing.");
                }
                list[i++] = last = end;
            }
            list[i] = HIGH; // terminate
        }

        /// <summary>
        /// Constructs a set from the <paramref name="pattern"/> pattern.  See the 
        /// <see cref="UnicodeSet"/> class description
        /// for the syntax of the <paramref name="pattern"/> language.
        /// Whitespace is ignored.
        /// </summary>
        /// <param name="pattern">A string specifying what characters are in the set.</param>
        /// <exception cref="ArgumentException">If the <paramref name="pattern"/>
        /// contains a syntax error.</exception>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet(string pattern)
            : this()
        {
            ApplyPattern(pattern, null, null, IGNORE_SPACE);
        }

        /// <summary>
        /// Constructs a set from the <paramref name="pattern"/> pattern.  See the 
        /// <see cref="UnicodeSet"/> class description
        /// for the syntax of the <paramref name="pattern"/> language.
        /// </summary>
        /// <param name="pattern">A string specifying what characters are in the set.</param>
        /// <param name="ignoreWhitespace">If true, ignore Unicode Pattern_White_Space characters.</param>
        /// <exception cref="ArgumentException">If the <paramref name="pattern"/>
        /// contains a syntax error.</exception>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet(string pattern, bool ignoreWhitespace)
            : this()
        {
            ApplyPattern(pattern, null, null, ignoreWhitespace ? IGNORE_SPACE : 0);
        }

        /// <summary>
        /// Constructs a set from the <paramref name="pattern"/> pattern.  See the 
        /// <see cref="UnicodeSet"/> class description
        /// for the syntax of the <paramref name="pattern"/> language.
        /// </summary>
        /// <param name="pattern">A string specifying what characters are in the set.</param>
        /// <param name="options">A bitmask indicating which options to apply.
        /// Valid options are <see cref="IGNORE_SPACE"/> and <see cref="CASE"/>.
        /// </param>
        /// <exception cref="ArgumentException">If the <paramref name="pattern"/>
        /// contains a syntax error.</exception>
        /// <stable>ICU 3.8</stable>
        public UnicodeSet(string pattern, int options)
            : this()
        {
            ApplyPattern(pattern, null, null, options);
        }

        /// <summary>
        /// Constructs a set from the given <paramref name="pattern"/>.  See the 
        /// <see cref="UnicodeSet"/> class description
        /// for the syntax of the <paramref name="pattern"/> language.
        /// </summary>
        /// <param name="pattern">A string specifying what characters are in the set.</param>
        /// <param name="pos">On input, the position in pattern at which to start parsing.
        /// On output, the position after the last character parsed.</param>
        /// <param name="symbols">A symbol table mapping variables to char[] arrays
        /// and chars to <see cref="UnicodeSet"/>s.</param>
        /// <exception cref="ArgumentException">If the <paramref name="pattern"/>
        /// contains a syntax error.</exception>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet(string pattern, ParsePosition pos, ISymbolTable symbols)
            : this()
        {
            ApplyPattern(pattern, pos, symbols, IGNORE_SPACE);
        }

        /// <summary>
        /// Constructs a set from the given <paramref name="pattern"/>.  See the
        /// <see cref="UnicodeSet"/> class description
        /// for the syntax of the <paramref name="pattern"/> language.
        /// </summary>
        /// <param name="pattern">A string specifying what characters are in the set.</param>
        /// <param name="pos">On input, the position in pattern at which to start parsing.
        /// On output, the position after the last character parsed.</param>
        /// <param name="symbols">A symbol table mapping variables to char[] arrays
        /// and chars to <see cref="UnicodeSet"/>s.</param>
        /// <param name="options">A bitmask indicating which options to apply.
        /// Valid options are <see cref="IGNORE_SPACE"/> and <see cref="CASE"/>.
        /// </param>
        /// <exception cref="ArgumentException">If the <paramref name="pattern"/>
        /// contains a syntax error.</exception>
        /// <stable>ICU 3.2</stable>
        public UnicodeSet(string pattern, ParsePosition pos, ISymbolTable symbols, int options)
            : this()
        {
            ApplyPattern(pattern, pos, symbols, options);
        }
#pragma warning restore 612, 618

        /// <summary>
        /// Return a new set that is equivalent to this one.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public virtual object Clone()
        {
            if (IsFrozen)
            {
                return this;
            }
            UnicodeSet result = new UnicodeSet(this);
            result.bmpSet = this.bmpSet;
            result.stringSpan = this.stringSpan;
            return result;
        }

        /// <summary>
        /// Make this object represent the range <code>start - end</code>.
        /// If <code>end &gt; start</code> then this object is set to an
        /// an empty range.
        /// </summary>
        /// <param name="start">First character in the set, inclusive.</param>
        /// <param name="end">Last character in the set, inclusive.</param>
        /// <stable>ICU 2.0</stable>
        public virtual UnicodeSet Set(int start, int end)
        {
            CheckFrozen();
            Clear();
            Complement(start, end);
            return this;
        }

        /// <summary>
        /// Make this object represent the same set as <paramref name="other"/>.
        /// </summary>
        /// <param name="other">A <see cref="UnicodeSet"/> whose value will be
        /// copied to this object.</param>
        /// <stable>ICU 2.0</stable>
        public virtual UnicodeSet Set(UnicodeSet other)
        {
            CheckFrozen();
            list = (int[])other.list.Clone();
            len = other.len;
            pat = other.pat;
            strings = new SortedSet<string>(other.strings, StringComparer.Ordinal);
            return this;
        }

        /// <summary>
        /// Modifies this set to represent the set specified by the given <paramref name="pattern"/>.
        /// See the <see cref="UnicodeSet"/> class description for the syntax of the <paramref name="pattern"/> language.
        /// Whitespace is ignored.
        /// </summary>
        /// <param name="pattern">A string specifying what characters are in the set.</param>
        /// <exception cref="ArgumentException">If the <paramref name="pattern"/> contains a syntax error.</exception>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet ApplyPattern(string pattern)
        {
            CheckFrozen();
#pragma warning disable 612, 618
            return ApplyPattern(pattern, null, null, IGNORE_SPACE);
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Modifies this set to represent the set specified by the given <paramref name="pattern"/>,
        /// optionally ignoring whitespace.
        /// See the <see cref="UnicodeSet"/> class description for the syntax of the <paramref name="pattern"/> language.
        /// </summary>
        /// <param name="pattern">A string specifying what characters are in the set.</param>
        /// <param name="ignoreWhitespace">If true then Unicode Pattern_White_Space characters are ignored.</param>
        /// <exception cref="ArgumentException">If the <paramref name="pattern"/> contains a syntax error.</exception>
        /// <stable>ICU 2.0</stable>
        public virtual UnicodeSet ApplyPattern(string pattern, bool ignoreWhitespace)
        {
            CheckFrozen();
#pragma warning disable 612, 618
            return ApplyPattern(pattern, null, null, ignoreWhitespace ? IGNORE_SPACE : 0);
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Modifies this set to represent the set specified by the given <paramref name="pattern"/>,
        /// optionally ignoring whitespace.
        /// See the class description for the syntax of the pattern language.
        /// </summary>
        /// <param name="pattern">A string specifying what characters are in the set.</param>
        /// <param name="options">A bitmask indicating which options to apply.
        /// Valid options are <see cref="IGNORE_SPACE"/> and <see cref="CASE"/>.</param>
        /// <exception cref="ArgumentException">If the <paramref name="pattern"/> contains a syntax error.</exception>
        /// <stable>ICU 3.8</stable>
        public virtual UnicodeSet ApplyPattern(string pattern, int options)
        {
            CheckFrozen();
#pragma warning disable 612, 618
            return ApplyPattern(pattern, null, null, options);
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Return true if the given position, in the given <paramref name="pattern"/>, appears
        /// to be the start of a <see cref="UnicodeSet"/> pattern.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public static bool ResemblesPattern(string pattern, int pos)
        {
            return ((pos + 1) < pattern.Length &&
                    pattern[pos] == '[') ||
                    ResemblesPropertyPattern(pattern, pos);
        }

        // ICU4N specific - AppendCodePoint(IAppendable app, int c) moved to UnicodeSetExtension.tt

        // ICU4N specific - Append(IAppendable app, ICharSequence s) moved to UnicodeSetExtension.tt

        // ICU4N specific - AppendToPat(IAppendable buf, string s, bool escapeUnprintable) moved to UnicodeSetExtension.tt

        // ICU4N specific - AppendToPat(IAppendable buf, int c, bool escapeUnprintable) moved to UnicodeSetExtension.tt

        /// <summary>
        /// Returns a string representation of this set.  If the result of
        /// calling this function is passed to a <see cref="UnicodeSet"/> constructor, it
        /// will produce another set that is equal to this one.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public override string ToPattern(bool escapeUnprintable)
        {
            if (pat != null && !escapeUnprintable)
            {
                return pat;
            }
            StringBuilder result = new StringBuilder();
            return ToPattern(result, escapeUnprintable).ToString();
        }

        // ICU4N specific - ToPattern(IAppendable result, bool escapeUnprintable) moved to UnicodeSetExtension.tt

        /// <summary>
        /// Generate and append a string representation of this set to result.
        /// This does not use <see cref="pat"/>, the cleaned up copy of the string
        /// passed to <see cref="ApplyPattern(string)"/>
        /// </summary>
        /// <param name="result">The buffer into which to generate the pattern.</param>
        /// <param name="escapeUnprintable">Escape unprintable characters if true.</param>
        /// <stable>ICU 3.8</stable>
        public virtual StringBuilder GeneratePattern(StringBuilder result, bool escapeUnprintable)
        {
            return GeneratePattern(result, escapeUnprintable, true);
        }

        /// <summary>
        /// Generate and append a string representation of this set to <paramref name="result"/>.
        /// This does not use <see cref="pat"/>, the cleaned up copy of the string
        /// passed to <see cref="ApplyPattern(string)"/>
        /// </summary>
        /// <param name="result">The buffer into which to generate the pattern.</param>
        /// <param name="escapeUnprintable">Escape unprintable characters if true.</param>
        /// <param name="includeStrings">If false, doesn't include the strings.</param>
        /// <stable>ICU 3.8</stable>
        public virtual StringBuilder GeneratePattern(StringBuilder result,
                bool escapeUnprintable, bool includeStrings)
        {
            return AppendNewPattern(result, escapeUnprintable, includeStrings);
        }

        // ICU4N specific - AppendNewPattern(IAppendable result, bool escapeUnprintable, bool includeStrings) moved to UnicodeSetExtension.tt

        /// <summary>
        /// Returns the number of elements in this set (its cardinality)
        /// Note than the elements of a set may include both individual
        /// codepoints and strings.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public virtual int Count // ICU4N TODO: Not the best candidate for a property...
        {
            get
            {
                int n = 0;
                int count = RangeCount;
                for (int i = 0; i < count; ++i)
                {
                    n += GetRangeEnd(i) - GetRangeStart(i) + 1;
                }
                return n + strings.Count;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if this set contains no elements.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        internal virtual bool IsEmpty // ICU4N specific - changed from public to internal (we are using Any() in .NET)
        {
            get { return len == 1 && strings.Count == 0; }
        }

        /// <summary>
        /// Implementation of UnicodeMatcher API.  Returns <c>true</c> if
        /// this set contains any character whose low byte is the given
        /// value.  This is used by <c>RuleBasedTransliterator</c> for
        /// indexing.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public override bool MatchesIndexValue(int v)
        {
            /* The index value v, in the range [0,255], is contained in this set if
             * it is contained in any pair of this set.  Pairs either have the high
             * bytes equal, or unequal.  If the high bytes are equal, then we have
             * aaxx..aayy, where aa is the high byte.  Then v is contained if xx <=
             * v <= yy.  If the high bytes are unequal we have aaxx..bbyy, bb>aa.
             * Then v is contained if xx <= v || v <= yy.  (This is identical to the
             * time zone month containment logic.)
             */
            for (int i = 0; i < RangeCount; ++i)
            {
                int low = GetRangeStart(i);
                int high = GetRangeEnd(i);
                if ((low & ~0xFF) == (high & ~0xFF))
                {
                    if ((low & 0xFF) <= v && v <= (high & 0xFF))
                    {
                        return true;
                    }
                }
                else if ((low & 0xFF) <= v || v <= (high & 0xFF))
                {
                    return true;
                }
            }
            if (strings.Count != 0)
            {
                foreach (string s in strings)
                {
                    //if (s.Length() == 0) {
                    //    // Empty strings match everything
                    //    return true;
                    //}
                    // assert(s.Length() != 0); // We enforce this elsewhere
                    int c = UTF16.CharAt(s, 0);
                    if ((c & 0xFF) == v)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Implementation of <see cref="IUnicodeMatcher.Matches(IReplaceable, int[], int, bool)"/>.  Always matches the
        /// longest possible multichar string.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public override MatchDegree Matches(IReplaceable text,
                int[] offset,
                int limit,
                bool incremental)
        {

            if (offset[0] == limit)
            {
                // Strings, if any, have length != 0, so we don't worry
                // about them here.  If we ever allow zero-length strings
                // we much check for them here.
                if (Contains(UnicodeMatcher.ETHER))
                {
                    return incremental ? MatchDegree.PartialMatch : MatchDegree.Match;
                }
                else
                {
                    return MatchDegree.Mismatch;
                }
            }
            else
            {
                if (strings.Count != 0)
                { // try strings first

                    // might separate forward and backward loops later
                    // for now they are combined

                    // TODO Improve efficiency of this, at least in the forward
                    // direction, if not in both.  In the forward direction we
                    // can assume the strings are sorted.

                    bool forward = offset[0] < limit;

                    // firstChar is the leftmost char to match in the
                    // forward direction or the rightmost char to match in
                    // the reverse direction.
                    char firstChar = text[offset[0]];

                    // If there are multiple strings that can match we
                    // return the longest match.
                    int highWaterLength = 0;

                    foreach (string trial in strings)
                    {
                        //if (trial.Length() == 0) {
                        //    return U_MATCH; // null-string always matches
                        //}
                        // assert(trial.Length() != 0); // We ensure this elsewhere

                        char c = trial[forward ? 0 : trial.Length - 1];

                        // Strings are sorted, so we can optimize in the
                        // forward direction.
                        if (forward && c > firstChar) break;
                        if (c != firstChar) continue;

                        int length = MatchRest(text, offset[0], limit, trial);

                        if (incremental)
                        {
                            int maxLen = forward ? limit - offset[0] : offset[0] - limit;
                            if (length == maxLen)
                            {
                                // We have successfully matched but only up to limit.
                                return MatchDegree.PartialMatch;
                            }
                        }

                        if (length == trial.Length)
                        {
                            // We have successfully matched the whole string.
                            if (length > highWaterLength)
                            {
                                highWaterLength = length;
                            }
                            // In the forward direction we know strings
                            // are sorted so we can bail early.
                            if (forward && length < highWaterLength)
                            {
                                break;
                            }
                            continue;
                        }
                    }

                    // We've checked all strings without a partial match.
                    // If we have full matches, return the longest one.
                    if (highWaterLength != 0)
                    {
                        offset[0] += forward ? highWaterLength : -highWaterLength;
                        return MatchDegree.Match;
                    }
                }
                return base.Matches(text, offset, limit, incremental);
            }
        }

        /// <summary>
        /// Returns the longest match for <paramref name="s"/> in text at the given position.
        /// If <paramref name="limit"/> > <paramref name="start"/> then match forward from <paramref name="start"/>+1 to <paramref name="limit"/>
        /// matching all characters except s[0].  If <paramref name="limit"/> &lt; start,
        /// go backward starting from <paramref name="start"/>-1 matching all characters
        /// except s[s.Length-1].  This method assumes that the
        /// first character, text[<paramref name="start"/>], matches <paramref name="s"/>, so it does not
        /// check it.
        /// </summary>
        /// <param name="text">The text to match.</param>
        /// <param name="start">The first character to match.  In the forward
        /// direction, <paramref name="text"/>[<paramref name="start"/>] is matched against <paramref name="s"/>[0].
        /// In the reverse direction, it is matched against
        /// <paramref name="s"/>[<paramref name="s"/>.Length-1].
        /// </param>
        /// <param name="limit">The limit offset for matching, either last+1 in
        /// the forward direction, or last-1 in the reverse direction,
        /// where last is the index of the last character to match.
        /// </param>
        /// <param name="s"></param>
        /// <returns>If part of <paramref name="s"/> matches up to the limit, return |limit -
        /// start|.  If all of <paramref name="s"/> matches before reaching the limit, return
        /// <paramref name="s"/>.Length.  If there is a mismatch between <paramref name="s"/> and text, return
        /// 0.
        /// </returns>
        private static int MatchRest(IReplaceable text, int start, int limit, string s)
        {
            int maxLen;
            int slen = s.Length;
            if (start < limit)
            {
                maxLen = limit - start;
                if (maxLen > slen) maxLen = slen;
                for (int i = 1; i < maxLen; ++i)
                {
                    if (text[start + i] != s[i]) return 0;
                }
            }
            else
            {
                maxLen = start - limit;
                if (maxLen > slen) maxLen = slen;
                --slen; // <=> slen = s.Length() - 1;
                for (int i = 1; i < maxLen; ++i)
                {
                    if (text[start - i] != s[slen - i]) return 0;
                }
            }
            return maxLen;
        }

        // ICU4N specific - MatchesAt(ICharSequence text, int offset) moved to UnicodeSetExtension.tt

        // ICU4N specific - MatchesAt(ICharSequence text, int offsetInText, ICharSequence substring) moved to UnicodeSetExtension.tt

        /// <summary>
        /// Implementation of <see cref="IUnicodeMatcher"/> API.  Union the set of all
        /// characters that may be matched by this object into the given
        /// set.
        /// </summary>
        /// <param name="toUnionTo">The set into which to union the source characters.</param>
        /// <stable>ICU 2.2</stable>
        public override void AddMatchSetTo(UnicodeSet toUnionTo)
        {
            toUnionTo.AddAll(this);
        }

        /// <summary>
        /// Returns the index of the given character within this set, where
        /// the set is ordered by ascending code point.  If the character
        /// is not in this set, return -1.  The inverse of this method is
        /// <see cref="this[int]"/>.
        /// </summary>
        /// <returns>An index from 0..Count-1, or -1.</returns>
        /// <stable>ICU 2.0</stable>
        public virtual int IndexOf(int c)
        {
            if (c < MinValue || c > MaxValue)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(c, 6));
            }
            int i = 0;
            int n = 0;
            for (; ; )
            {
                int start = list[i++];
                if (c < start)
                {
                    return -1;
                }
                int limit = list[i++];
                if (c < limit)
                {
                    return n + c - start;
                }
                n += limit - start;
            }
        }

        // ICU4N specific - replaced int CharAt(index) with int this[int index]

        /// <summary>
        /// Returns the character at the given index within this set, where
        /// the set is ordered by ascending code point.  If the index is
        /// out of range, return -1.  The inverse of this method is
        /// <see cref="IndexOf(int)"/>.
        /// </summary>
        /// <remarks>
        /// NOTE: This is equivalent to CharAt(index) in ICU4J.
        /// </remarks>
        /// <param name="index">An index from 0..Count-1.</param>
        /// <returns>The character at the given index, or -1.</returns>
        /// <stable>ICU 2.0</stable>
        public virtual int this[int index]
        {
            get
            {
                if (index >= 0)
                {
                    // len2 is the largest even integer <= len, that is, it is len
                    // for even values and len-1 for odd values.  With odd values
                    // the last entry is UNICODESET_HIGH.
                    int len2 = len & ~1;
                    for (int i = 0; i < len2;)
                    {
                        int start = list[i++];
                        int count = list[i++] - start;
                        if (index < count)
                        {
                            return start + index;
                        }
                        index -= count;
                    }
                }
                return -1;
            }
        }

        /// <summary>
        /// Adds the specified range to this set if it is not already
        /// present.  If this set already contains the specified range,
        /// the call leaves this set unchanged.  If 
        /// <c><paramref name="end"/> &gt; <paramref name="start"/></c>
        /// then an empty range is added, leaving the set unchanged.
        /// </summary>
        /// <param name="start">First character, inclusive, of range to be added
        /// to this set.</param>
        /// <param name="end">Last character, inclusive, of range to be added
        /// to this set.</param>
        /// <stable>ICU 2.0</stable>
        public virtual UnicodeSet Add(int start, int end)
        {
            CheckFrozen();
            return AddUnchecked(start, end);
        }

        /// <summary>
        /// Adds all characters in range (uses preferred naming convention).
        /// </summary>
        /// <param name="start">The index of where to start on adding all characters.</param>
        /// <param name="end">The index of where to end on adding all characters.</param>
        /// <returns>A reference to this object.</returns>
        /// <stable>ICU 4.4</stable>
        internal virtual UnicodeSet AddAll(int start, int end) // ICU4N specific - changed from public to internal (we are using UnionWithChars in .NET)
        {
            CheckFrozen();
            return AddUnchecked(start, end);
        }

        // for internal use, after checkFrozen has been called
        private UnicodeSet AddUnchecked(int start, int end)
        {
            if (start < MinValue || start > MaxValue)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(start, 6));
            }
            if (end < MinValue || end > MaxValue)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(end, 6));
            }
            if (start < end)
            {
                Add(Range(start, end), 2, 0);
            }
            else if (start == end)
            {
                Add(start);
            }
            return this;
        }

        //    /**
        //     * Format out the inversion list as a string, for debugging.  Uncomment when
        //     * needed.
        //     */
        //    public final String dump() {
        //        StringBuffer buf = new StringBuffer("[");
        //        for (int i=0; i<len; ++i) {
        //            if (i != 0) buf.append(", ");
        //            int c = list[i];
        //            //if (c <= 0x7F && c != '\n' && c != '\r' && c != '\t' && c != ' ') {
        //            //    buf.append((char) c);
        //            //} else {
        //                buf.append("U+").append(Utility.Hex(c, (c<0x10000)?4:6));
        //            //}
        //        }
        //        buf.append("]");
        //        return buf.toString();
        //    }

        /// <summary>
        /// Adds the specified character to this set if it is not already
        /// present.  If this set already contains the specified character,
        /// the call leaves this set unchanged.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet Add(int c)
        {
            CheckFrozen();
            return AddUnchecked(c);
        }

        // for internal use only, after checkFrozen has been called
        private UnicodeSet AddUnchecked(int c)
        {
            if (c < MinValue || c > MaxValue)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(c, 6));
            }

            // find smallest i such that c < list[i]
            // if odd, then it is IN the set
            // if even, then it is OUT of the set
            int i = FindCodePoint(c);

            // already in set?
            if ((i & 1) != 0) return this;

            // HIGH is 0x110000
            // assert(list[len-1] == HIGH);

            // empty = [HIGH]
            // [start_0, limit_0, start_1, limit_1, HIGH]

            // [..., start_k-1, limit_k-1, start_k, limit_k, ..., HIGH]
            //                             ^
            //                             list[i]

            // i == 0 means c is before the first range
            // TODO: Is the "list[i]-1" a typo? Even if you pass MAX_VALUE into
            //      add_unchecked, the maximum value that "c" will be compared to
            //      is "MAX_VALUE-1" meaning that "if (c == MAX_VALUE)" will
            //      never be reached according to this logic.
            if (c == list[i] - 1)
            {
                // c is before start of next range
                list[i] = c;
                // if we touched the HIGH mark, then add a new one
                if (c == MaxValue)
                {
                    EnsureCapacity(len + 1);
                    list[len++] = HIGH;
                }
                if (i > 0 && c == list[i - 1])
                {
                    // collapse adjacent ranges

                    // [..., start_k-1, c, c, limit_k, ..., HIGH]
                    //                     ^
                    //                     list[i]
                    System.Array.Copy(list, i + 1, list, i - 1, len - i - 1);
                    len -= 2;
                }
            }

            else if (i > 0 && c == list[i - 1])
            {
                // c is after end of prior range
                list[i - 1]++;
                // no need to chcek for collapse here
            }

            else
            {
                // At this point we know the new char is not adjacent to
                // any existing ranges, and it is not 10FFFF.


                // [..., start_k-1, limit_k-1, start_k, limit_k, ..., HIGH]
                //                             ^
                //                             list[i]

                // [..., start_k-1, limit_k-1, c, c+1, start_k, limit_k, ..., HIGH]
                //                             ^
                //                             list[i]

                // Don't use ensureCapacity() to save on copying.
                // NOTE: This has no measurable impact on performance,
                // but it might help in some usage patterns.
                if (len + 2 > list.Length)
                {
                    int[] temp = new int[len + 2 + GROW_EXTRA];
                    if (i != 0) System.Array.Copy(list, 0, temp, 0, i);
                    System.Array.Copy(list, i, temp, i + 2, len - i);
                    list = temp;
                }
                else
                {
                    System.Array.Copy(list, i, list, i + 2, len - i);
                }

                list[i] = c;
                list[i + 1] = c + 1;
                len += 2;
            }

            pat = null;
            return this;
        }

        // ICU4N specific - Add(ICharSequence s) moved to UnicodeSetExtension.tt

        // ICU4N specific - GetSingleCP(ICharSequence s) moved to UnicodeSetExtension.tt

        // ICU4N specific - AddAll(ICharSequence s) moved to UnicodeSetExtension.tt

        // ICU4N specific - RetainAll(ICharSequence s) moved to UnicodeSetExtension.tt

        // ICU4N specific - ComplementAll(ICharSequence s) moved to UnicodeSetExtension.tt

        // ICU4N specific - RemoveAll(ICharSequence s) moved to UnicodeSetExtension.tt

        /// <summary>
        /// Remove all strings from this <see cref="UnicodeSet"/>
        /// </summary>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 4.2</stable>
        internal UnicodeSet RemoveAllStrings() // ICU4N specific - changed from public to internal (we are using ClearStrings() in .NET)
        {
            CheckFrozen();
            if (strings.Count != 0)
            {
                strings.Clear();
                pat = null;
            }
            return this;
        }

        // ICU4N specific - From(ICharSequence s) moved to UnicodeSetExtension.tt

        // ICU4N specific - FromAll(ICharSequence s) moved to UnicodeSetExtension.tt

        /// <summary>
        /// Retain only the elements in this set that are contained in the
        /// specified range.  If <c><paramref name="end"/> &gt; <paramref name="start"/></c> 
        /// then an empty range is retained, leaving the set empty.
        /// </summary>
        /// <param name="start">First character, inclusive, of range to be retained
        /// to this set.</param>
        /// <param name="end">Last character, inclusive, of range to be retained
        /// to this set.</param>
        /// <stable>ICU 2.0</stable>
        internal virtual UnicodeSet Retain(int start, int end) // ICU4N specific - changed from public to internal (we are using IntersectWith in .NET)
        {
            CheckFrozen();
            if (start < MinValue || start > MaxValue)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(start, 6));
            }
            if (end < MinValue || end > MaxValue)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(end, 6));
            }
            if (start <= end)
            {
                Retain(Range(start, end), 2, 0);
            }
            else
            {
                Clear();
            }
            return this;
        }

        /// <summary>
        /// Retain the specified character from this set if it is present.
        /// Upon return this set will be empty if it did not contain <paramref name="c"/>, or
        /// will only contain c if it did contain <paramref name="c"/>.
        /// </summary>
        /// <param name="c">The character to be retained.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        internal UnicodeSet Retain(int c) // ICU4N specific - changed from public to internal (we are using IntersectWith in .NET)
        {
            return Retain(c, c);
        }

        // ICU4N specific - Retain(ICharSequence s) moved to UnicodeSetExtension.tt

        /// <summary>
        /// Removes the specified range from this set if it is present.
        /// The set will not contain the specified range once the call
        /// returns.  If <c><paramref name="end"/> &gt; <paramref name="start"/></c> 
        /// then an empty range is removed, leaving the set unchanged.
        /// </summary>
        /// <param name="start">First character, inclusive, of range to be removed
        /// from this set.</param>
        /// <param name="end">Last character, inclusive, of range to be removed
        /// from this set.</param>
        /// <stable>ICU 2.0</stable>
        public virtual UnicodeSet Remove(int start, int end)
        {
            CheckFrozen();
            if (start < MinValue || start > MaxValue)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(start, 6));
            }
            if (end < MinValue || end > MaxValue)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(end, 6));
            }
            if (start <= end)
            {
                Retain(Range(start, end), 2, 2);
            }
            return this;
        }

        /// <summary>
        /// Removes the specified character from this set if it is present.
        /// The set will not contain the specified character once the call
        /// returns.
        /// </summary>
        /// <param name="c">The character to be removed.</param>
        /// <returns>This object, for chaining.</returns>
        /// <stable>ICU 2.0</stable>
        public UnicodeSet Remove(int c)
        {
            return Remove(c, c);
        }

        // ICU4N specific - Remove(ICharSequence s) moved to UnicodeSetExtension.tt


        /// <summary>
        /// Complements the specified range in this set.  Any character in
        /// the range will be removed if it is in this set, or will be
        /// added if it is not in this set.  If <c><paramref name="end"/> &gt; <paramref name="start"/></c>
        /// then an empty range is complemented, leaving the set unchanged.
        /// </summary>
        /// <param name="start">First character, inclusive, of range to be removed from this set.</param>
        /// <param name="end">Last character, inclusive, of range to be removed from this set.</param>
        /// <stable>ICU 2.0</stable>
        internal virtual UnicodeSet Complement(int start, int end) // ICU4N specific - changed from public to internal (we are using SymetricExceptWithChars in .NET)
        {
            CheckFrozen();
            if (start < MinValue || start > MaxValue)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(start, 6));
            }
            if (end < MinValue || end > MaxValue)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(end, 6));
            }
            if (start <= end)
            {
                Xor(Range(start, end), 2, 0);
            }
            pat = null;
            return this;
        }

        /// <summary>
        /// Complements the specified character in this set.  The character
        /// will be removed if it is in this set, or will be added if it is
        /// not in this set.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        internal UnicodeSet Complement(int c) // ICU4N specific - changed from public to internal (we are using SymetricExceptWith in .NET)
        {
            return Complement(c, c);
        }

        /// <summary>
        /// This is equivalent to
        /// <c>Complement(<see cref="MinValue"/>, <see cref="MaxValue"/>)</c>
        /// </summary>
        /// <stable>ICU 2.0</stable>
        internal virtual UnicodeSet Complement() // ICU4N specific - changed from public to internal (we are using SymetricExceptWithChars in .NET)
        {
            CheckFrozen();
            if (list[0] == LOW)
            {
                System.Array.Copy(list, 1, list, 0, len - 1);
                --len;
            }
            else
            {
                EnsureCapacity(len + 1);
                System.Array.Copy(list, 0, list, 1, len);
                list[0] = LOW;
                ++len;
            }
            pat = null;
            return this;
        }

        // ICU4N specific - Complement(ICharSequence s) moved to UnicodeSetExtension.tt

        /// <summary>
        /// Returns true if this set contains the given character.
        /// </summary>
        /// <param name="c">Character to be checked for containment.</param>
        /// <returns>true if the test condition is met.</returns>
        /// <stable>ICU 2.0</stable>
        public override bool Contains(int c)
        {
            if (c < MinValue || c > MaxValue)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(c, 6));
            }
            if (bmpSet != null)
            {
                return bmpSet.Contains(c);
            }
            if (stringSpan != null)
            {
                return stringSpan.Contains(c);
            }

            /*
            // Set i to the index of the start item greater than ch
            // We know we will terminate without length test!
            int i = -1;
            while (true) {
                if (c < list[++i]) break;
            }
             */

            int i = FindCodePoint(c);

            return ((i & 1) != 0); // return true if odd
        }

        /// <summary>
        /// Returns the smallest value i such that c &lt; list[i].  Caller
        /// must ensure that <paramref name="c"/> is a legal value or this method will enter
        /// an infinite loop.  This method performs a binary search.
        /// </summary>
        /// <param name="c">A character in the range <see cref="MinValue"/>..<see cref="MaxValue"/>.</param>
        /// <returns>The smallest integer i in the range 0..len-1,
        /// inclusive, such that <paramref name="c"/> &lt; list[i].</returns>
        private int FindCodePoint(int c)
        {
            /* Examples:
                                               findCodePoint(c)
               set              list[]         c=0 1 3 4 7 8
               ===              ==============   ===========
               []               [110000]         0 0 0 0 0 0
               [\u0000-\u0003]  [0, 4, 110000]   1 1 1 2 2 2
               [\u0004-\u0007]  [4, 8, 110000]   0 0 0 1 1 2
               [:all:]          [0, 110000]      1 1 1 1 1 1
             */

            // Return the smallest i such that c < list[i].  Assume
            // list[len - 1] == HIGH and that c is legal (0..HIGH-1).
            if (c < list[0]) return 0;
            // High runner test.  c is often after the last range, so an
            // initial check for this condition pays off.
            if (len >= 2 && c >= list[len - 2]) return len - 1;
            int lo = 0;
            int hi = len - 1;
            // invariant: c >= list[lo]
            // invariant: c < list[hi]
            for (; ; )
            {
                int i = (lo + hi).TripleShift(1);
                if (i == lo) return hi;
                if (c < list[i])
                {
                    hi = i;
                }
                else
                {
                    lo = i;
                }
            }
        }

        //    //----------------------------------------------------------------
        //    // Unrolled binary search
        //    //----------------------------------------------------------------
        //
        //    private int validLen = -1; // validated value of len
        //    private int topOfLow;
        //    private int topOfHigh;
        //    private int power;
        //    private int deltaStart;
        //
        //    private void validate() {
        //        if (len <= 1) {
        //            throw new ArgumentException("list.len==" + len + "; must be >1");
        //        }
        //
        //        // find greatest power of 2 less than or equal to len
        //        for (power = exp2.Length-1; power > 0 && exp2[power] > len; power--) {}
        //
        //        // assert(exp2[power] <= len);
        //
        //        // determine the starting points
        //        topOfLow = exp2[power] - 1;
        //        topOfHigh = len - 1;
        //        deltaStart = exp2[power-1];
        //        validLen = len;
        //    }
        //
        //    private static final int exp2[] = {
        //        0x1, 0x2, 0x4, 0x8,
        //        0x10, 0x20, 0x40, 0x80,
        //        0x100, 0x200, 0x400, 0x800,
        //        0x1000, 0x2000, 0x4000, 0x8000,
        //        0x10000, 0x20000, 0x40000, 0x80000,
        //        0x100000, 0x200000, 0x400000, 0x800000,
        //        0x1000000, 0x2000000, 0x4000000, 0x8000000,
        //        0x10000000, 0x20000000 // , 0x40000000 // no unsigned int in Java
        //    };
        //
        //    /**
        //     * Unrolled lowest index GT.
        //     */
        //    private final int leastIndexGT(int searchValue) {
        //
        //        if (len != validLen) {
        //            if (len == 1) return 0;
        //            validate();
        //        }
        //        int temp;
        //
        //        // set up initial range to search. Each subrange is a power of two in length
        //        int high = searchValue < list[topOfLow] ? topOfLow : topOfHigh;
        //
        //        // Completely unrolled binary search, folhighing "Programming Pearls"
        //        // Each case deliberately falls through to the next
        //        // Logically, list[-1] < all_search_values && list[count] > all_search_values
        //        // although the values -1 and count are never actually touched.
        //
        //        // The bounds at each point are low & high,
        //        // where low == high - delta*2
        //        // so high - delta is the midpoint
        //
        //        // The invariant AFTER each line is that list[low] < searchValue <= list[high]
        //
        //        switch (power) {
        //        //case 31: if (searchValue < list[temp = high-0x40000000]) high = temp; // no unsigned int in Java
        //        case 30: if (searchValue < list[temp = high-0x20000000]) high = temp;
        //        case 29: if (searchValue < list[temp = high-0x10000000]) high = temp;
        //
        //        case 28: if (searchValue < list[temp = high- 0x8000000]) high = temp;
        //        case 27: if (searchValue < list[temp = high- 0x4000000]) high = temp;
        //        case 26: if (searchValue < list[temp = high- 0x2000000]) high = temp;
        //        case 25: if (searchValue < list[temp = high- 0x1000000]) high = temp;
        //
        //        case 24: if (searchValue < list[temp = high-  0x800000]) high = temp;
        //        case 23: if (searchValue < list[temp = high-  0x400000]) high = temp;
        //        case 22: if (searchValue < list[temp = high-  0x200000]) high = temp;
        //        case 21: if (searchValue < list[temp = high-  0x100000]) high = temp;
        //
        //        case 20: if (searchValue < list[temp = high-   0x80000]) high = temp;
        //        case 19: if (searchValue < list[temp = high-   0x40000]) high = temp;
        //        case 18: if (searchValue < list[temp = high-   0x20000]) high = temp;
        //        case 17: if (searchValue < list[temp = high-   0x10000]) high = temp;
        //
        //        case 16: if (searchValue < list[temp = high-    0x8000]) high = temp;
        //        case 15: if (searchValue < list[temp = high-    0x4000]) high = temp;
        //        case 14: if (searchValue < list[temp = high-    0x2000]) high = temp;
        //        case 13: if (searchValue < list[temp = high-    0x1000]) high = temp;
        //
        //        case 12: if (searchValue < list[temp = high-     0x800]) high = temp;
        //        case 11: if (searchValue < list[temp = high-     0x400]) high = temp;
        //        case 10: if (searchValue < list[temp = high-     0x200]) high = temp;
        //        case  9: if (searchValue < list[temp = high-     0x100]) high = temp;
        //
        //        case  8: if (searchValue < list[temp = high-      0x80]) high = temp;
        //        case  7: if (searchValue < list[temp = high-      0x40]) high = temp;
        //        case  6: if (searchValue < list[temp = high-      0x20]) high = temp;
        //        case  5: if (searchValue < list[temp = high-      0x10]) high = temp;
        //
        //        case  4: if (searchValue < list[temp = high-       0x8]) high = temp;
        //        case  3: if (searchValue < list[temp = high-       0x4]) high = temp;
        //        case  2: if (searchValue < list[temp = high-       0x2]) high = temp;
        //        case  1: if (searchValue < list[temp = high-       0x1]) high = temp;
        //        }
        //
        //        return high;
        //    }
        //
        //    // For debugging only
        //    public int len() {
        //        return len;
        //    }
        //
        //    //----------------------------------------------------------------
        //    //----------------------------------------------------------------

        /// <summary>
        /// Returns true if this set contains every character
        /// of the given range.
        /// </summary>
        /// <param name="start">First character, inclusive, of the range.</param>
        /// <param name="end">Last character, inclusive, of the range.</param>
        /// <returns>true if the test condition is met.</returns>
        /// <stable>ICU 2.0</stable>
        public virtual bool Contains(int start, int end)
        {
            if (start < MinValue || start > MaxValue)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(start, 6));
            }
            if (end < MinValue || end > MaxValue)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(end, 6));
            }
            //int i = -1;
            //while (true) {
            //    if (start < list[++i]) break;
            //}
            int i = FindCodePoint(start);
            return ((i & 1) != 0 && end < list[i]);
        }

        // ICU4N specific - Contains(ICharSequence s) moved to UnicodeSetExtension.tt

        /// <summary>
        /// Returns true if this set contains all the characters and strings
        /// of the given set.
        /// </summary>
        /// <param name="b">Set to be checked for containment.</param>
        /// <returns>true if the test condition is met.</returns>
        /// <stable>ICU 2.0</stable>
        internal virtual bool ContainsAll(UnicodeSet b) // ICU4N specific - changed public to internal (we are using IsSupersetOf in .NET)
        {
            // The specified set is a subset if all of its pairs are contained in
            // this set. This implementation accesses the lists directly for speed.
            // TODO: this could be faster if size() were cached. But that would affect building speed
            // so it needs investigation.
            int[] listB = b.list;
            bool needA = true;
            bool needB = true;
            int aPtr = 0;
            int bPtr = 0;
            int aLen = len - 1;
            int bLen = b.len - 1;
            int startA = 0, startB = 0, limitA = 0, limitB = 0;
            while (true)
            {
                // double iterations are such a pain...
                if (needA)
                {
                    if (aPtr >= aLen)
                    {
                        // ran out of A. If B is also exhausted, then break;
                        if (needB && bPtr >= bLen)
                        {
                            break;
                        }
                        return false;
                    }
                    startA = list[aPtr++];
                    limitA = list[aPtr++];
                }
                if (needB)
                {
                    if (bPtr >= bLen)
                    {
                        // ran out of B. Since we got this far, we have an A and we are ok so far
                        break;
                    }
                    startB = listB[bPtr++];
                    limitB = listB[bPtr++];
                }
                // if B doesn't overlap and is greater than A, get new A
                if (startB >= limitA)
                {
                    needA = true;
                    needB = false;
                    continue;
                }
                // if B is wholy contained in A, then get a new B
                if (startB >= startA && limitB <= limitA)
                {
                    needA = false;
                    needB = true;
                    continue;
                }
                // all other combinations mean we fail
                return false;
            }

            if (!strings.IsSupersetOf(b.strings)) return false;
            return true;
        }

        //    /**
        //     * Returns true if this set contains all the characters and strings
        //     * of the given set.
        //     * @param c set to be checked for containment
        //     * @return true if the test condition is met
        //     * @stable ICU 2.0
        //     */
        //    public bool containsAllOld(UnicodeSet c) {
        //        // The specified set is a subset if all of its pairs are contained in
        //        // this set.  It's possible to code this more efficiently in terms of
        //        // direct manipulation of the inversion lists if the need arises.
        //        int n = c.getRangeCount();
        //        for (int i=0; i<n; ++i) {
        //            if (!contains(c.getRangeStart(i), c.getRangeEnd(i))) {
        //                return false;
        //            }
        //        }
        //        if (!strings.containsAll(c.strings)) return false;
        //        return true;
        //    }

        /// <summary>
        /// Returns true if there is a partition of the string such that this set contains each of the partitioned strings.
        /// For example, for the Unicode set [a{bc}{cd}]
        /// <list type="bullet">
        ///     <item><description><see cref="ContainsAll(string)"/> is true for each of: "a", "bc", ""cdbca"</description></item>
        ///     <item><description><see cref="ContainsAll(string)"/> is false for each of: "acb", "bcda", "bcx"</description></item>
        /// </list>
        /// </summary>
        /// <param name="s">String containing characters to be checked for containment.</param>
        /// <returns>true if the test condition is met.</returns>
        /// <stable>ICU 2.0</stable>
        internal virtual bool ContainsAll(string s) // ICU4N specific - changed public to internal (we are using IsSupersetOf in .NET)
        {
            int cp;
            for (int i = 0; i < s.Length; i += UTF16.GetCharCount(cp))
            {
                cp = UTF16.CharAt(s, i);
                if (!Contains(cp))
                {
                    if (strings.Count == 0)
                    {
                        return false;
                    }
                    return ContainsAll(s, 0);
                }
            }
            return true;
        }

        /// <summary>
        /// Recursive routine called if we fail to find a match in <see cref="ContainsAll(string, int)"/>, and there are strings.
        /// </summary>
        /// <param name="s">Source string.</param>
        /// <param name="i">Point to match to the end on.</param>
        /// <returns>true if ok.</returns>
        private bool ContainsAll(string s, int i)
        {
            if (i >= s.Length)
            {
                return true;
            }
            int cp = UTF16.CharAt(s, i);
            if (Contains(cp) && ContainsAll(s, i + UTF16.GetCharCount(cp)))
            {
                return true;
            }
            foreach (string setStr in strings)
            {
                if (s.Substring(i).StartsWith(setStr, StringComparison.Ordinal) && ContainsAll(s, i + setStr.Length))
                {
                    return true;
                }
            }
            return false;

        }

        /// <summary>
        /// Get the Regex equivalent for this <see cref="UnicodeSet"/>.
        /// </summary>
        /// <returns>Regex pattern equivalent to this <see cref="UnicodeSet"/>.</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public string GetRegexEquivalent()
        {
            if (strings.Count == 0)
            {
                return ToString();
            }
            StringBuilder result = new StringBuilder("(?:");
            AppendNewPattern(result, true, false);
            foreach (string s in strings)
            {
                result.Append('|');
                AppendToPat(result, s, true);
            }
            return result.Append(")").ToString();
        }

        /// <summary>
        /// Returns true if this set contains none of the characters
        /// of the given range.
        /// </summary>
        /// <param name="start">First character, inclusive, of the range.</param>
        /// <param name="end">Last character, inclusive, of the range.</param>
        /// <returns>true if the test condition is met.</returns>
        /// <stable>ICU 2.0</stable>
        public virtual bool ContainsNone(int start, int end)
        {
            if (start < MinValue || start > MaxValue)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(start, 6));
            }
            if (end < MinValue || end > MaxValue)
            {
                throw new ArgumentException("Invalid code point U+" + Utility.Hex(end, 6));
            }
            int i = -1;
            while (true)
            {
                if (start < list[++i]) break;
            }
            return ((i & 1) == 0 && end < list[i]);
        }

        /// <summary>
        /// Returns true if none of the characters or strings in this <see cref="UnicodeSet"/> appears in the string.
        /// For example, for the Unicode set [a{bc}{cd}]
        /// <list type="bullet">
        ///     <item><description><see cref="ContainsNone(UnicodeSet)"/> is true for: "xy", "cb"</description></item>
        ///     <item><description><see cref="ContainsNone(UnicodeSet)"/> is false for: "a", "bc", "bcd"</description></item>
        /// </list>
        /// </summary>
        /// <param name="b">Set to be checked for containment.</param>
        /// <returns>true if the test condition is met.</returns>
        /// <stable>2.0</stable>
        public virtual bool ContainsNone(UnicodeSet b)
        {
            // The specified set is a subset if some of its pairs overlap with some of this set's pairs.
            // This implementation accesses the lists directly for speed.
            int[] listB = b.list;
            bool needA = true;
            bool needB = true;
            int aPtr = 0;
            int bPtr = 0;
            int aLen = len - 1;
            int bLen = b.len - 1;
            int startA = 0, startB = 0, limitA = 0, limitB = 0;
            while (true)
            {
                // double iterations are such a pain...
                if (needA)
                {
                    if (aPtr >= aLen)
                    {
                        // ran out of A: break so we test strings
                        break;
                    }
                    startA = list[aPtr++];
                    limitA = list[aPtr++];
                }
                if (needB)
                {
                    if (bPtr >= bLen)
                    {
                        // ran out of B: break so we test strings
                        break;
                    }
                    startB = listB[bPtr++];
                    limitB = listB[bPtr++];
                }
                // if B is higher than any part of A, get new A
                if (startB >= limitA)
                {
                    needA = true;
                    needB = false;
                    continue;
                }
                // if A is higher than any part of B, get new B
                if (startA >= limitB)
                {
                    needA = false;
                    needB = true;
                    continue;
                }
                // all other combinations mean we fail
                return false;
            }

            if (!SortedSetRelation.HasRelation(strings, SortedSetRelation.DISJOINT, b.strings)) return false;
            return true;
        }

        //    /**
        //     * Returns true if none of the characters or strings in this UnicodeSet appears in the string.
        //     * For example, for the Unicode set [a{bc}{cd}]<br>
        //     * containsNone is true for: "xy", "cb"<br>
        //     * containsNone is false for: "a", "bc", "bcd"<br>
        //     * @param c set to be checked for containment
        //     * @return true if the test condition is met
        //     * @stable ICU 2.0
        //     */
        //    public bool containsNoneOld(UnicodeSet c) {
        //        // The specified set is a subset if all of its pairs are contained in
        //        // this set.  It's possible to code this more efficiently in terms of
        //        // direct manipulation of the inversion lists if the need arises.
        //        int n = c.getRangeCount();
        //        for (int i=0; i<n; ++i) {
        //            if (!containsNone(c.getRangeStart(i), c.getRangeEnd(i))) {
        //                return false;
        //            }
        //        }
        //        if (!SortedSetRelation.hasRelation(strings, SortedSetRelation.DISJOINT, c.strings)) return false;
        //        return true;
        //    }

        // ICU4N specific - ContainsNone(ICharSequence s) moved to UnicodeSetExtension.tt

        /// <summary>
        /// Returns true if this set contains one or more of the characters
        /// in the given range.
        /// </summary>
        /// <param name="start">First character, inclusive, of the range.</param>
        /// <param name="end">Last character, inclusive, of the range.</param>
        /// <returns>true if the condition is met.</returns>
        /// <stable>ICU 2.0</stable>
        internal bool ContainsSome(int start, int end) // ICU4N specific - changed from public to internal (we are using Overlaps in .NET)
        {
            return !ContainsNone(start, end);
        }

        /// <summary>
        /// Returns true if this set contains one or more of the characters
        /// and strings of the given set.
        /// </summary>
        /// <param name="s">Set to be checked for containment.</param>
        /// <returns>True if the condition is met.</returns>
        /// <stable>ICU 2.0</stable>
        internal bool ContainsSome(UnicodeSet s) // ICU4N specific - changed from public to internal (we are using Overlaps in .NET)
        {
            return !ContainsNone(s);
        }

        // ICU4N specific - ContainsSome(ICharSequence s) moved to UnicodeSetExtension.tt

        /// <summary>
        /// Adds all of the elements in the specified set to this set if
        /// they're not already present.  This operation effectively
        /// modifies this set so that its value is the <i>union</i> of the two
        /// sets.  The behavior of this operation is unspecified if the specified
        /// collection is modified while the operation is in progress.
        /// </summary>
        /// <param name="c">Set whose elements are to be added to this set.</param>
        /// <stable>ICU 2.0</stable>
        internal virtual UnicodeSet AddAll(UnicodeSet c) // ICU4N specific - changed from public to internal (we are using UnionWith in .NET)
        {
            CheckFrozen();
            Add(c.list, c.len, 0);
            strings.UnionWith(c.strings);
            return this;
        }

        /// <summary>
        /// Retains only the elements in this set that are contained in the
        /// specified set.  In other words, removes from this set all of
        /// its elements that are not contained in the specified set.  This
        /// operation effectively modifies this set so that its value is
        /// the <i>intersection</i> of the two sets.
        /// </summary>
        /// <param name="c">Set that defines which elements this set will retain.</param>
        /// <stable>ICU 2.0</stable>
        internal virtual UnicodeSet RetainAll(UnicodeSet c) // ICU4N specific - changed from public to internal (we are using IntersectWith in .NET)
        {
            CheckFrozen();
            Retain(c.list, c.len, 0);
            strings.IntersectWith(c.strings);
            return this;
        }

        /// <summary>
        /// Removes from this set all of its elements that are contained in the
        /// specified set.  This operation effectively modifies this
        /// set so that its value is the <i>asymmetric set difference</i> of
        /// the two sets.
        /// </summary>
        /// <param name="c">Set that defines which elements will be removed from
        /// this set.</param>
        /// <stable>ICU 2.0</stable>
        internal virtual UnicodeSet RemoveAll(UnicodeSet c) // ICU4N specific - changed from public to internal (we are using ExceptWith in .NET)
        {
            CheckFrozen();
            Retain(c.list, c.len, 2);
            strings.ExceptWith(c.strings);
            return this;
        }

        /// <summary>
        /// Complements in this set all elements contained in the specified
        /// set.  Any character in the other set will be removed if it is
        /// in this set, or will be added if it is not in this set.
        /// </summary>
        /// <param name="c">Set that defines which elements will be complemented from
        /// this set.</param>
        /// <stable>ICU 2.0</stable>
        internal virtual UnicodeSet ComplementAll(UnicodeSet c) // ICU4N specific - changed from public to internal (we are using UnionWith in .NET)
        {
            CheckFrozen();
            Xor(c.list, c.len, 0);
            SortedSetRelation.DoOperation(strings, SortedSetRelation.COMPLEMENTALL, c.strings);
            return this;
        }

        /// <summary>
        /// Removes all of the elements from this set.  This set will be
        /// empty after this call returns.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public virtual UnicodeSet Clear()
        {
            CheckFrozen();
            list[0] = HIGH;
            len = 1;
            pat = null;
            strings.Clear();
            return this;
        }

        /// <summary>
        /// Iteration method that returns the number of ranges contained in
        /// this set.
        /// </summary>
        /// <seealso cref="GetRangeStart(int)"/>
        /// <seealso cref="GetRangeEnd(int)"/>
        /// <stable>ICU 2.0</stable>
        public virtual int RangeCount
        {
            get { return len / 2; }
        }

        /// <summary>
        /// Iteration method that returns the first character in the
        /// specified range of this set.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="index"/> is outside the range <c>0..<see cref="RangeCount"/>-1</c>.</exception>
        /// <seealso cref="RangeCount"/>
        /// <seealso cref="GetRangeEnd(int)"/>
        /// <stable>ICU 2.0</stable>
        public virtual int GetRangeStart(int index)
        {
            return list[index * 2];
        }

        /// <summary>
        /// Iteration method that returns the last character in the
        /// specified range of this set.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">If <paramref name="index"/> is outside the range <c>0..<see cref="RangeCount"/>-1</c>.</exception>
        /// <seealso cref="GetRangeStart(int)"/>
        /// <seealso cref="RangeCount"/>
        /// <stable>ICU 2.0</stable>
        public virtual int GetRangeEnd(int index)
        {
            return (list[index * 2 + 1] - 1);
        }

        /// <summary>
        /// Reallocate this objects internal structures to take up the least
        /// possible space, without changing this object's value.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public virtual UnicodeSet Compact()
        {
            CheckFrozen();
            if (len != list.Length)
            {
                int[] temp = new int[len];
                System.Array.Copy(list, 0, temp, 0, len);
                list = temp;
            }
            rangeList = null;
            buffer = null;
            return this;
        }

        /// <summary>
        /// Compares the specified object with this set for equality.  Returns
        /// <c>true</c> if the specified object is also a set, the two sets
        /// have the same size, and every member of the specified set is
        /// contained in this set (or equivalently, every member of this set is
        /// contained in the specified set).
        /// </summary>
        /// <param name="o">Object to be compared for equality with this set.</param>
        /// <returns><c>true</c> if the specified Object is equal to this set.</returns>
        /// <stable>ICU 2.0</stable>
        public override bool Equals(object o)
        {
            if (o == null)
            {
                return false;
            }
            if (this == o)
            {
                return true;
            }
            // ICU4N specific - removed the try/catch and 
            // added an extra check for null so we don't throw during a cast
            UnicodeSet that = o as UnicodeSet;
            if (that == null)
            {
                return false;
            }
            if (len != that.len) return false;
            for (int i = 0; i < len; ++i)
            {
                if (list[i] != that.list[i]) return false;
            }

            // ICU4N: In .NET, the Equals method of collections do not compare contents,
            // so we must do a manual check here.
            if (!strings.SequenceEqual(that.strings)) return false; // ICU4N TODO: This could be optimized so it doesn't create an enumerator by implementing a custom collection
            return true;
        }

        /// <summary>
        /// Returns the hash code value for this set.
        /// </summary>
        /// <returns>The hash code value for this set.</returns>
        /// <seealso cref="Object.GetHashCode()"/>
        /// <stable>ICU 2.0</stable>
        public override int GetHashCode()
        {
            int result = len;
            for (int i = 0; i < len; ++i)
            {
                result *= 1000003;
                result += list[i];
            }
            return result;
        }

        /// <summary>
        /// Return a programmer-readable string representation of this object.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public override string ToString()
        {
            return ToPattern(true);
        }

        //----------------------------------------------------------------
        // Implementation: Pattern parsing
        //----------------------------------------------------------------

        /// <summary>
        /// Parses the given pattern, starting at the given position.  The character
        /// at pattern[pos.Index] must be '[', or the parse fails.
        /// Parsing continues until the corresponding closing ']'.  If a syntax error
        /// is encountered between the opening and closing brace, the parse fails.
        /// Upon return from a successful parse, the ParsePosition is updated to
        /// point to the character following the closing ']', and an inversion
        /// list for the parsed pattern is returned.  This method
        /// calls itself recursively to parse embedded subpatterns.
        /// </summary>
        /// <param name="pattern">the string containing the pattern to be parsed.  The
        /// portion of the string from pos.Index, which must be a '[', to the
        /// corresponding closing ']', is parsed.
        /// </param>
        /// <param name="pos">upon entry, the position at which to being parsing.  The
        /// character at pattern[pos.Index] must be a '['.  Upon return
        /// from a successful parse, pos.Index is either the character after the
        /// closing ']' of the parsed pattern, or pattern.Length if the closing ']'
        /// is the last character of the pattern string.
        /// </param>
        /// <param name="symbols"></param>
        /// <param name="options"></param>
        /// <returns>An inversion list for the parsed substring of <paramref name="pattern"/>.</returns>
        /// <exception cref="ArgumentException">If the parse fails.</exception>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public virtual UnicodeSet ApplyPattern(string pattern,
                ParsePosition pos,
                ISymbolTable symbols,
                int options)
        {

            // Need to build the pattern in a temporary string because
            // _applyPattern calls add() etc., which set pat to empty.
            bool parsePositionWasNull = pos == null;
            if (parsePositionWasNull)
            {
                pos = new ParsePosition(0);
            }

            StringBuilder rebuiltPat = new StringBuilder();
            RuleCharacterIterator chars =
                    new RuleCharacterIterator(pattern, symbols, pos);
            ApplyPattern(chars, symbols, rebuiltPat.ToAppendable(), options);
            if (chars.InVariable)
            {
                SyntaxError(chars, "Extra chars in variable value");
            }
            pat = rebuiltPat.ToString();
            if (parsePositionWasNull)
            {
                int i = pos.Index;

                // Skip over trailing whitespace
                if ((options & IGNORE_SPACE) != 0)
                {
                    i = PatternProps.SkipWhiteSpace(pattern, i);
                }

                if (i != pattern.Length)
                {
                    throw new ArgumentException("Parse of \"" + pattern +
                            "\" failed at " + i);
                }
            }
            return this;
        }

        // Add constants to make the applyPattern() code easier to follow.

        private const int LAST0_START = 0,
                LAST1_RANGE = 1,
                LAST2_SET = 2;

        private const int MODE0_NONE = 0,
                MODE1_INBRACKET = 1,
                MODE2_OUTBRACKET = 2;

        private const int SETMODE0_NONE = 0,
                SETMODE1_UNICODESET = 1,
                SETMODE2_PROPERTYPAT = 2,
                SETMODE3_PREPARSED = 3;

        /// <summary>
        /// Parse the pattern from the given <see cref="RuleCharacterIterator"/>.  The
        /// iterator is advanced over the parsed pattern.
        /// </summary>
        /// <param name="chars">
        /// Iterator over the pattern characters.  Upon return
        /// it will be advanced to the first character after the parsed
        /// pattern, or the end of the iteration if all characters are
        /// parsed.
        /// </param>
        /// <param name="symbols">
        /// Symbol table to use to parse and dereference
        /// variables, or null if none.
        /// </param>
        /// <param name="rebuiltPat">
        /// The pattern that was parsed, rebuilt or
        /// copied from the input pattern, as appropriate.
        /// </param>
        /// <param name="options">
        /// A bit mask of zero or more of the following:
        /// <see cref="IGNORE_SPACE"/>, <see cref="CASE"/>.
        /// </param>
        private void ApplyPattern(RuleCharacterIterator chars, ISymbolTable symbols,
            IAppendable rebuiltPat, int options) // ICU4N TODO: API - Make [Flags] enum for options
        {

            // Syntax characters: [ ] ^ - & { }

            // Recognized special forms for chars, sets: c-c s-s s&s

            RuleCharacterIteratorOptions opts = RuleCharacterIteratorOptions.ParseVariables |
                    RuleCharacterIteratorOptions.ParseEscapes;
            if ((options & IGNORE_SPACE) != 0)
            {
                opts |= RuleCharacterIteratorOptions.SkipWhitespace;
            }

            StringBuilder patBuf = new StringBuilder(), buf = null;
            bool usePat = false;
            UnicodeSet scratch = null;
            object backup = null;

            // mode: 0=before [, 1=between [...], 2=after ]
            // lastItem: 0=none, 1=char, 2=set
            int lastItem = LAST0_START, lastChar = 0, mode = MODE0_NONE;
            char op = (char)0;

            bool invert = false;

            Clear();
            string lastString = null;

            while (mode != MODE2_OUTBRACKET && !chars.AtEnd)
            {
                //Eclipse stated the following is "dead code"
                /*
                if (false) {
                    // Debugging assertion
                    if (!((lastItem == 0 && op == 0) ||
                            (lastItem == 1 && (op == 0 || op == '-')) ||
                            (lastItem == 2 && (op == 0 || op == '-' || op == '&')))) {
                        throw new ArgumentException();
                    }
                }*/

                int c = 0;
                bool literal = false;
                UnicodeSet nested = null;

                // -------- Check for property pattern

                // setMode: 0=none, 1=unicodeset, 2=propertypat, 3=preparsed
                int setMode = SETMODE0_NONE;
                if (ResemblesPropertyPattern(chars, opts))
                {
                    setMode = SETMODE2_PROPERTYPAT;
                }

                // -------- Parse '[' of opening delimiter OR nested set.
                // If there is a nested set, use `setMode' to define how
                // the set should be parsed.  If the '[' is part of the
                // opening delimiter for this pattern, parse special
                // strings "[", "[^", "[-", and "[^-".  Check for stand-in
                // characters representing a nested set in the symbol
                // table.

                else
                {
                    // Prepare to backup if necessary
                    backup = chars.GetPos(backup);
                    c = chars.Next(opts);
                    literal = chars.IsEscaped;

                    if (c == '[' && !literal)
                    {
                        if (mode == MODE1_INBRACKET)
                        {
                            chars.SetPos(backup); // backup
                            setMode = SETMODE1_UNICODESET;
                        }
                        else
                        {
                            // Handle opening '[' delimiter
                            mode = MODE1_INBRACKET;
                            patBuf.Append('[');
                            backup = chars.GetPos(backup); // prepare to backup
                            c = chars.Next(opts);
                            literal = chars.IsEscaped;
                            if (c == '^' && !literal)
                            {
                                invert = true;
                                patBuf.Append('^');
                                backup = chars.GetPos(backup); // prepare to backup
                                c = chars.Next(opts);
                                literal = chars.IsEscaped;
                            }
                            // Fall through to handle special leading '-';
                            // otherwise restart loop for nested [], \p{}, etc.
                            if (c == '-')
                            {
                                literal = true;
                                // Fall through to handle literal '-' below
                            }
                            else
                            {
                                chars.SetPos(backup); // backup
                                continue;
                            }
                        }
                    }
                    else if (symbols != null)
                    {
                        IUnicodeMatcher m = symbols.LookupMatcher(c); // may be null
                        if (m != null)
                        {
                            try
                            {
                                nested = (UnicodeSet)m;
                                setMode = SETMODE3_PREPARSED;
                            }
                            catch (InvalidCastException e)
                            {
                                SyntaxError(chars, "Syntax error", e);
                            }
                        }
                    }
                }

                // -------- Handle a nested set.  This either is inline in
                // the pattern or represented by a stand-in that has
                // previously been parsed and was looked up in the symbol
                // table.

                if (setMode != SETMODE0_NONE)
                {
                    if (lastItem == LAST1_RANGE)
                    {
                        if (op != 0)
                        {
                            SyntaxError(chars, "Char expected after operator");
                        }
                        AddUnchecked(lastChar, lastChar);
                        AppendToPat(patBuf, lastChar, false);
                        lastItem = LAST0_START;
                        op = (char)0;
                    }

                    if (op == '-' || op == '&')
                    {
                        patBuf.Append(op);
                    }

                    if (nested == null)
                    {
                        if (scratch == null) scratch = new UnicodeSet();
                        nested = scratch;
                    }
                    switch (setMode)
                    {
                        case SETMODE1_UNICODESET:
                            nested.ApplyPattern(chars, symbols, patBuf.ToAppendable(), options);
                            break;
                        case SETMODE2_PROPERTYPAT:
                            chars.SkipIgnored(opts);
                            nested.ApplyPropertyPattern(chars, patBuf.ToAppendable(), symbols);
                            break;
                        case SETMODE3_PREPARSED: // `nested' already parsed
                            nested.ToPattern(patBuf, false);
                            break;
                    }

                    usePat = true;

                    if (mode == MODE0_NONE)
                    {
                        // Entire pattern is a category; leave parse loop
                        Set(nested);
                        mode = MODE2_OUTBRACKET;
                        break;
                    }

                    switch (op)
                    {
                        case '-':
                            RemoveAll(nested);
                            break;
                        case '&':
                            RetainAll(nested);
                            break;
                        case (char)0:
                            AddAll(nested);
                            break;
                    }

                    op = (char)0;
                    lastItem = LAST2_SET;

                    continue;
                }

                if (mode == MODE0_NONE)
                {
                    SyntaxError(chars, "Missing '['");
                }

                // -------- Parse special (syntax) characters.  If the
                // current character is not special, or if it is escaped,
                // then fall through and handle it below.

                if (!literal)
                {
                    switch (c)
                    {
                        case ']':
                            if (lastItem == LAST1_RANGE)
                            {
                                AddUnchecked(lastChar, lastChar);
                                AppendToPat(patBuf, lastChar, false);
                            }
                            // Treat final trailing '-' as a literal
                            if (op == '-')
                            {
                                AddUnchecked(op, op);
                                patBuf.Append(op);
                            }
                            else if (op == '&')
                            {
                                SyntaxError(chars, "Trailing '&'");
                            }
                            patBuf.Append(']');
                            mode = MODE2_OUTBRACKET;
                            continue;
                        case '-':
                            if (op == 0)
                            {
                                if (lastItem != LAST0_START)
                                {
                                    op = (char)c;
                                    continue;
                                }
                                else if (lastString != null)
                                {
                                    op = (char)c;
                                    continue;
                                }
                                else
                                {
                                    // Treat final trailing '-' as a literal
                                    AddUnchecked(c, c);
                                    c = chars.Next(opts);
                                    literal = chars.IsEscaped;
                                    if (c == ']' && !literal)
                                    {
                                        patBuf.Append("-]");
                                        mode = MODE2_OUTBRACKET;
                                        continue;
                                    }
                                }
                            }
                            SyntaxError(chars, "'-' not after char, string, or set");
                            break;
                        case '&':
                            if (lastItem == LAST2_SET && op == 0)
                            {
                                op = (char)c;
                                continue;
                            }
                            SyntaxError(chars, "'&' not after set");
                            break;
                        case '^':
                            SyntaxError(chars, "'^' not after '['");
                            break;
                        case '{':
                            if (op != 0 && op != '-')
                            {
                                SyntaxError(chars, "Missing operand after operator");
                            }
                            if (lastItem == LAST1_RANGE)
                            {
                                AddUnchecked(lastChar, lastChar);
                                AppendToPat(patBuf, lastChar, false);
                            }
                            lastItem = LAST0_START;
                            if (buf == null)
                            {
                                buf = new StringBuilder();
                            }
                            else
                            {
                                buf.Length = 0;
                            }
                            bool ok = false;
                            while (!chars.AtEnd)
                            {
                                c = chars.Next(opts);
                                literal = chars.IsEscaped;
                                if (c == '}' && !literal)
                                {
                                    ok = true;
                                    break;
                                }
                                AppendCodePoint(buf, c);
                            }
                            if (buf.Length < 1 || !ok)
                            {
                                SyntaxError(chars, "Invalid multicharacter string");
                            }
                            // We have new string. Add it to set and continue;
                            // we don't need to drop through to the further
                            // processing
                            string curString = buf.ToString();
                            if (op == '-')
                            {
#pragma warning disable 612, 618
                                int lastSingle = CharSequences.GetSingleCodePoint((lastString == null ? "" : lastString));
                                int curSingle = CharSequences.GetSingleCodePoint(curString);
#pragma warning restore 612, 618
                                if (lastSingle != int.MaxValue && curSingle != int.MaxValue)
                                {
                                    Add(lastSingle, curSingle);
                                }
                                else
                                {
                                    try
                                    {
                                        StringRange.Expand(lastString, curString, true, strings);
                                    }
                                    catch (Exception e)
                                    {
                                        SyntaxError(chars, e.Message);
                                    }
                                }
                                lastString = null;
                                op = (char)0;
                            }
                            else
                            {
                                Add(curString);
                                lastString = curString;
                            }
                            patBuf.Append('{');
                            AppendToPat(patBuf, curString, false);
                            patBuf.Append('}');
                            continue;
                        case SymbolTable.SYMBOL_REF:
                            //         symbols  nosymbols
                            // [a-$]   error    error (ambiguous)
                            // [a$]    anchor   anchor
                            // [a-$x]  var "x"* literal '$'
                            // [a-$.]  error    literal '$'
                            // *We won't get here in the case of var "x"
                            backup = chars.GetPos(backup);
                            c = chars.Next(opts);
                            literal = chars.IsEscaped;
                            bool anchor = (c == ']' && !literal);
                            if (symbols == null && !anchor)
                            {
                                c = SymbolTable.SYMBOL_REF;
                                chars.SetPos(backup);
                                break; // literal '$'
                            }
                            if (anchor && op == 0)
                            {
                                if (lastItem == LAST1_RANGE)
                                {
                                    AddUnchecked(lastChar, lastChar);
                                    AppendToPat(patBuf, lastChar, false);
                                }
                                AddUnchecked(UnicodeMatcher.ETHER);
                                usePat = true;
                                patBuf.Append(SymbolTable.SYMBOL_REF).Append(']');
                                mode = MODE2_OUTBRACKET;
                                continue;
                            }
                            SyntaxError(chars, "Unquoted '$'");
                            break;
                        default:
                            break;
                    }
                }

                // -------- Parse literal characters.  This includes both
                // escaped chars ("\u4E01") and non-syntax characters
                // ("a").

                switch (lastItem)
                {
                    case LAST0_START:
                        if (op == '-' && lastString != null)
                        {
                            SyntaxError(chars, "Invalid range");
                        }
                        lastItem = LAST1_RANGE;
                        lastChar = c;
                        lastString = null;
                        break;
                    case LAST1_RANGE:
                        if (op == '-')
                        {
                            if (lastString != null)
                            {
                                SyntaxError(chars, "Invalid range");
                            }
                            if (lastChar >= c)
                            {
                                // Don't allow redundant (a-a) or empty (b-a) ranges;
                                // these are most likely typos.
                                SyntaxError(chars, "Invalid range");
                            }
                            AddUnchecked(lastChar, c);
                            AppendToPat(patBuf, lastChar, false);
                            patBuf.Append(op);
                            AppendToPat(patBuf, c, false);
                            lastItem = LAST0_START;
                            op = (char)0;
                        }
                        else
                        {
                            AddUnchecked(lastChar, lastChar);
                            AppendToPat(patBuf, lastChar, false);
                            lastChar = c;
                        }
                        break;
                    case LAST2_SET:
                        if (op != 0)
                        {
                            SyntaxError(chars, "Set expected after operator");
                        }
                        lastChar = c;
                        lastItem = LAST1_RANGE;
                        break;
                }
            }

            if (mode != MODE2_OUTBRACKET)
            {
                SyntaxError(chars, "Missing ']'");
            }

            chars.SkipIgnored(opts);

            /*
             * Handle global flags (invert, case insensitivity).  If this
             * pattern should be compiled case-insensitive, then we need
             * to close over case BEFORE COMPLEMENTING.  This makes
             * patterns like /[^abc]/i work.
             */
            if ((options & CASE) != 0)
            {
                CloseOver(CASE);
            }
            if (invert)
            {
                Complement();
            }

            // Use the rebuilt pattern (pat) only if necessary.  Prefer the
            // generated pattern.
            if (usePat)
            {
                Append(rebuiltPat, patBuf);
            }
            else
            {
                AppendNewPattern(rebuiltPat, false, true);
            }
        }

        private static void SyntaxError(RuleCharacterIterator chars, string msg, Exception innerException)
        {
            throw new ArgumentException("Error: " + msg + " at \"" +
                    Utility.Escape(chars.ToString()) +
                    '"', innerException);
        }

        private static void SyntaxError(RuleCharacterIterator chars, string msg)
        {
            throw new ArgumentException("Error: " + msg + " at \"" +
                    Utility.Escape(chars.ToString()) +
                    '"');
        }

        /// <summary>
        /// Add the contents of the UnicodeSet (as strings) into a collection.
        /// </summary>
        /// <typeparam name="T">Collection type.</typeparam>
        /// <param name="target">Collection to add into.</param>
        /// <stable>ICU 4.4</stable>
        internal virtual T AddAllTo<T>(T target) where T : ICollection<string> // ICU4N specific - changed from public to internal (we are using CopyTo in .NET)
        {
            return AddAllTo(this, target);
        }

        // ICU4N specific: Removed AddAllTo(string[] target) overload because it is redundant as in .NET array implements ICollection<T>

        /// <summary>
        /// Add the contents of the <see cref="UnicodeSet"/> (as strings) into an array.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public static string[] ToArray(UnicodeSet set)
        {
            return AddAllTo(set, new string[set.Count]);
        }

        /// <summary>
        /// Add the contents of the collection (as strings) into this <see cref="UnicodeSet"/>.
        /// The collection must not contain null.
        /// </summary>
        /// <typeparam name="T">The type of element to add (this method calls ToString() to convert this type to a string).</typeparam>
        /// <param name="source">The collection to add.</param>
        /// <returns>A reference to this object.</returns>
        /// <stable>ICU 4.4</stable>
        public virtual UnicodeSet Add<T>(IEnumerable<T> source)
        {
            return AddAll(source);
        }

        /// <summary>
        /// Add a collection (as strings) into this <see cref="UnicodeSet"/>.
        /// Uses standard naming convention.
        /// </summary>
        /// <param name="source">Source collection to add into.</param>
        /// <returns>A reference to this object.</returns>
        /// <draft>ICU4N 60</draft>
        // ICU4N specific overload to optimize for string
        internal virtual UnicodeSet AddAll(IEnumerable<string> source) // ICU4N specific - changed from public to internal (we are using UnionWith in .NET)
        {
            CheckFrozen();
            foreach (var o in source)
            {
                Add(o);
            }
            return this;
        }

        // ICU4N NOTE: No point in having a StringBuilder overload because
        // we would need to call ToString() on it anyway, so the generic
        // one will suffice.

        /// <summary>
        /// Add a collection (as strings) into this <see cref="UnicodeSet"/>.
        /// Uses standard naming convention.
        /// </summary>
        /// <param name="source">Source collection to add into.</param>
        /// <returns>A reference to this object.</returns>
        /// <draft>ICU4N 60</draft>
        // ICU4N specific overload to properly convert char array to string
        internal virtual UnicodeSet AddAll(IEnumerable<char[]> source) // ICU4N specific - changed from public to internal (we are using UnionWith in .NET)
        {
            CheckFrozen();
            foreach (var o in source)
            {
                Add(o);
            }
            return this;
        }

        /// <summary>
        /// Add a collection (as strings) into this <see cref="UnicodeSet"/>.
        /// Uses standard naming convention.
        /// </summary>
        /// <typeparam name="T">The type of element to add (this method calls ToString() to convert this type to a string).</typeparam>
        /// <param name="source">Source collection to add into.</param>
        /// <returns>A reference to this object.</returns>
        /// <stable>ICU 4.4</stable>
        internal virtual UnicodeSet AddAll<T>(IEnumerable<T> source) // ICU4N specific - changed from public to internal (we are using UnionWith in .NET)
        {
            CheckFrozen();
            foreach (object o in source)
            {
                Add(o.ToString());
            }
            return this;
        }

        //----------------------------------------------------------------
        // Implementation: Utility methods
        //----------------------------------------------------------------

        private void EnsureCapacity(int newLen)
        {
            if (newLen <= list.Length) return;
            int[] temp = new int[newLen + GROW_EXTRA];
            System.Array.Copy(list, 0, temp, 0, len);
            list = temp;
        }

        private void EnsureBufferCapacity(int newLen)
        {
            if (buffer != null && newLen <= buffer.Length) return;
            buffer = new int[newLen + GROW_EXTRA];
        }

        /// <summary>
        /// Assumes start &lt;= end.
        /// </summary>
        private int[] Range(int start, int end)
        {
            if (rangeList == null)
            {
                rangeList = new int[] { start, end + 1, HIGH };
            }
            else
            {
                rangeList[0] = start;
                rangeList[1] = end + 1;
            }
            return rangeList;
        }

        //----------------------------------------------------------------
        // Implementation: Fundamental operations
        //----------------------------------------------------------------

        // polarity = 0, 3 is normal: x xor y
        // polarity = 1, 2: x xor ~y == x === y

        private UnicodeSet Xor(int[] other, int otherLen, int polarity)
        {
            EnsureBufferCapacity(len + otherLen);
            int i = 0, j = 0, k = 0;
            int a = list[i++];
            int b;
            // TODO: Based on the call hierarchy, polarity of 1 or 2 is never used
            //      so the following if statement will not be called.
            //CLOVER:OFF
            if (polarity == 1 || polarity == 2)
            {
                b = LOW;
                if (other[j] == LOW)
                { // skip base if already LOW
                    ++j;
                    b = other[j];
                }
                //CLOVER:ON
            }
            else
            {
                b = other[j++];
            }
            // simplest of all the routines
            // sort the values, discarding identicals!
            while (true)
            {
                if (a < b)
                {
                    buffer[k++] = a;
                    a = list[i++];
                }
                else if (b < a)
                {
                    buffer[k++] = b;
                    b = other[j++];
                }
                else if (a != HIGH)
                { // at this point, a == b
                  // discard both values!
                    a = list[i++];
                    b = other[j++];
                }
                else
                { // DONE!
                    buffer[k++] = HIGH;
                    len = k;
                    break;
                }
            }
            // swap list and buffer
            int[] temp = list;
            list = buffer;
            buffer = temp;
            pat = null;
            return this;
        }

        // polarity = 0 is normal: x union y
        // polarity = 2: x union ~y
        // polarity = 1: ~x union y
        // polarity = 3: ~x union ~y

        private UnicodeSet Add(int[] other, int otherLen, int polarity)
        {
            EnsureBufferCapacity(len + otherLen);
            int i = 0, j = 0, k = 0;
            int a = list[i++];
            int b = other[j++];
            // change from xor is that we have to check overlapping pairs
            // polarity bit 1 means a is second, bit 2 means b is.
            //main:
            while (true)
            {
                switch (polarity)
                {
                    case 0: // both first; take lower if unequal
                        if (a < b)
                        { // take a
                          // Back up over overlapping ranges in buffer[]
                            if (k > 0 && a <= buffer[k - 1])
                            {
                                // Pick latter end value in buffer[] vs. list[]
                                a = Max(list[i], buffer[--k]);
                            }
                            else
                            {
                                // No overlap
                                buffer[k++] = a;
                                a = list[i];
                            }
                            i++; // Common if/else code factored out
                            polarity ^= 1;
                        }
                        else if (b < a)
                        { // take b
                            if (k > 0 && b <= buffer[k - 1])
                            {
                                b = Max(other[j], buffer[--k]);
                            }
                            else
                            {
                                buffer[k++] = b;
                                b = other[j];
                            }
                            j++;
                            polarity ^= 2;
                        }
                        else
                        { // a == b, take a, drop b
                            if (a == HIGH) goto main_break;
                            // This is symmetrical; it doesn't matter if
                            // we backtrack with a or b. - liu
                            if (k > 0 && a <= buffer[k - 1])
                            {
                                a = Max(list[i], buffer[--k]);
                            }
                            else
                            {
                                // No overlap
                                buffer[k++] = a;
                                a = list[i];
                            }
                            i++;
                            polarity ^= 1;
                            b = other[j++]; polarity ^= 2;
                        }
                        break;
                    case 3: // both second; take higher if unequal, and drop other
                        if (b <= a)
                        { // take a
                            if (a == HIGH) goto main_break;
                            buffer[k++] = a;
                        }
                        else
                        { // take b
                            if (b == HIGH) goto main_break;
                            buffer[k++] = b;
                        }
                        a = list[i++]; polarity ^= 1;   // factored common code
                        b = other[j++]; polarity ^= 2;
                        break;
                    case 1: // a second, b first; if b < a, overlap
                        if (a < b)
                        { // no overlap, take a
                            buffer[k++] = a; a = list[i++]; polarity ^= 1;
                        }
                        else if (b < a)
                        { // OVERLAP, drop b
                            b = other[j++]; polarity ^= 2;
                        }
                        else
                        { // a == b, drop both!
                            if (a == HIGH) goto main_break;
                            a = list[i++]; polarity ^= 1;
                            b = other[j++]; polarity ^= 2;
                        }
                        break;
                    case 2: // a first, b second; if a < b, overlap
                        if (b < a)
                        { // no overlap, take b
                            buffer[k++] = b; b = other[j++]; polarity ^= 2;
                        }
                        else if (a < b)
                        { // OVERLAP, drop a
                            a = list[i++]; polarity ^= 1;
                        }
                        else
                        { // a == b, drop both!
                            if (a == HIGH) goto main_break;
                            a = list[i++]; polarity ^= 1;
                            b = other[j++]; polarity ^= 2;
                        }
                        break;
                }
            }
        main_break: { }
            buffer[k++] = HIGH;    // terminate
            len = k;
            // swap list and buffer
            int[] temp = list;
            list = buffer;
            buffer = temp;
            pat = null;
            return this;
        }

        // polarity = 0 is normal: x intersect y
        // polarity = 2: x intersect ~y == set-minus
        // polarity = 1: ~x intersect y
        // polarity = 3: ~x intersect ~y

        private UnicodeSet Retain(int[] other, int otherLen, int polarity)
        {
            EnsureBufferCapacity(len + otherLen);
            int i = 0, j = 0, k = 0;
            int a = list[i++];
            int b = other[j++];
            // change from xor is that we have to check overlapping pairs
            // polarity bit 1 means a is second, bit 2 means b is.
            //main:
            while (true)
            {
                switch (polarity)
                {
                    case 0: // both first; drop the smaller
                        if (a < b)
                        { // drop a
                            a = list[i++]; polarity ^= 1;
                        }
                        else if (b < a)
                        { // drop b
                            b = other[j++]; polarity ^= 2;
                        }
                        else
                        { // a == b, take one, drop other
                            if (a == HIGH) goto main_break;
                            buffer[k++] = a; a = list[i++]; polarity ^= 1;
                            b = other[j++]; polarity ^= 2;
                        }
                        break;
                    case 3: // both second; take lower if unequal
                        if (a < b)
                        { // take a
                            buffer[k++] = a; a = list[i++]; polarity ^= 1;
                        }
                        else if (b < a)
                        { // take b
                            buffer[k++] = b; b = other[j++]; polarity ^= 2;
                        }
                        else
                        { // a == b, take one, drop other
                            if (a == HIGH) goto main_break;
                            buffer[k++] = a; a = list[i++]; polarity ^= 1;
                            b = other[j++]; polarity ^= 2;
                        }
                        break;
                    case 1: // a second, b first;
                        if (a < b)
                        { // NO OVERLAP, drop a
                            a = list[i++]; polarity ^= 1;
                        }
                        else if (b < a)
                        { // OVERLAP, take b
                            buffer[k++] = b; b = other[j++]; polarity ^= 2;
                        }
                        else
                        { // a == b, drop both!
                            if (a == HIGH) goto main_break;
                            a = list[i++]; polarity ^= 1;
                            b = other[j++]; polarity ^= 2;
                        }
                        break;
                    case 2: // a first, b second; if a < b, overlap
                        if (b < a)
                        { // no overlap, drop b
                            b = other[j++]; polarity ^= 2;
                        }
                        else if (a < b)
                        { // OVERLAP, take a
                            buffer[k++] = a; a = list[i++]; polarity ^= 1;
                        }
                        else
                        { // a == b, drop both!
                            if (a == HIGH) goto main_break;
                            a = list[i++]; polarity ^= 1;
                            b = other[j++]; polarity ^= 2;
                        }
                        break;
                }
            }
        main_break: { }
            buffer[k++] = HIGH;    // terminate
            len = k;
            // swap list and buffer
            int[] temp = list;
            list = buffer;
            buffer = temp;
            pat = null;
            return this;
        }

        private static int Max(int a, int b)
        {
            return (a > b) ? a : b;
        }

        //----------------------------------------------------------------
        // Generic filter-based scanning code
        //----------------------------------------------------------------

        private interface IFilter
        {
            bool Contains(int codePoint);
        }

        private class NumericValueFilter : IFilter
        {
            public double Value { get; set; }
            internal NumericValueFilter(double value)
            {
                this.Value = value;
            }

            public virtual bool Contains(int ch)
            {
                return UCharacter.GetUnicodeNumericValue(ch) == Value;
            }
        }

        private class GeneralCategoryMaskFilter : IFilter
        {
            public int Mask { get; set; }
            internal GeneralCategoryMaskFilter(int mask) { this.Mask = mask; }

            public virtual bool Contains(int ch)
            {
                return ((1 << UCharacter.GetType(ch).ToInt32()) & Mask) != 0;
            }
        }

        private class Int32PropertyFilter : IFilter
        {
            public UProperty Prop { get; set; }
            public int Value { get; set; }
            internal Int32PropertyFilter(UProperty prop, int value)
            {
                this.Prop = prop;
                this.Value = value;
            }
            public virtual bool Contains(int ch)
            {
                return UCharacter.GetInt32PropertyValue(ch, Prop) == Value;
            }
        }

        private class ScriptExtensionsFilter : IFilter
        {
            public int Script { get; set; }
            internal ScriptExtensionsFilter(int script) { this.Script = script; }

            public virtual bool Contains(int c)
            {
                return UScript.HasScript(c, Script);
            }
        }

        // VersionInfo for unassigned characters
        private static readonly VersionInfo NO_VERSION = VersionInfo.GetInstance(0, 0, 0, 0);

        private class VersionFilter : IFilter
        {
            private VersionInfo version;
            internal VersionFilter(VersionInfo version) { this.version = version; }

            public virtual bool Contains(int ch)
            {
                VersionInfo v = UCharacter.GetAge(ch);
                // Reference comparison ok; VersionInfo caches and reuses
                // unique objects.
                return !Utility.SameObjects(v, NO_VERSION) &&
                        v.CompareTo(version) <= 0;
            }
        }

        private static UnicodeSet GetInclusions(int src)
        {
            lock (syncLock)
            {
                if (INCLUSIONS == null)
                {
                    INCLUSIONS = new UnicodeSet[UCharacterProperty.SRC_COUNT];
                }
                if (INCLUSIONS[src] == null)
                {
                    UnicodeSet incl = new UnicodeSet();
                    switch (src)
                    {
                        case UCharacterProperty.SRC_CHAR:
                            UCharacterProperty.Instance.AddPropertyStarts(incl);
                            break;
                        case UCharacterProperty.SRC_PROPSVEC:
                            UCharacterProperty.Instance.upropsvec_addPropertyStarts(incl);
                            break;
                        case UCharacterProperty.SRC_CHAR_AND_PROPSVEC:
                            UCharacterProperty.Instance.AddPropertyStarts(incl);
                            UCharacterProperty.Instance.upropsvec_addPropertyStarts(incl);
                            break;
                        case UCharacterProperty.SRC_CASE_AND_NORM:
                            Norm2AllModes.GetNFCInstance().Impl.AddPropertyStarts(incl);
                            UCaseProps.Instance.AddPropertyStarts(incl);
                            break;
                        case UCharacterProperty.SRC_NFC:
                            Norm2AllModes.GetNFCInstance().Impl.AddPropertyStarts(incl);
                            break;
                        case UCharacterProperty.SRC_NFKC:
                            Norm2AllModes.GetNFKCInstance().Impl.AddPropertyStarts(incl);
                            break;
                        case UCharacterProperty.SRC_NFKC_CF:
                            Norm2AllModes.GetNFKC_CFInstance().Impl.AddPropertyStarts(incl);
                            break;
                        case UCharacterProperty.SRC_NFC_CANON_ITER:
                            Norm2AllModes.GetNFCInstance().Impl.AddCanonIterPropertyStarts(incl);
                            break;
                        case UCharacterProperty.SRC_CASE:
                            UCaseProps.Instance.AddPropertyStarts(incl);
                            break;
                        case UCharacterProperty.SRC_BIDI:
                            UBiDiProps.Instance.AddPropertyStarts(incl);
                            break;
                        default:
                            throw new InvalidOperationException("UnicodeSet.getInclusions(unknown src " + src + ")");
                    }
                    INCLUSIONS[src] = incl;
                }
                return INCLUSIONS[src];
            }
        }

        /// <summary>
        /// Generic filter-based scanning code for UCD property <see cref="UnicodeSet"/>s.
        /// </summary>
        private UnicodeSet ApplyFilter(IFilter filter, int src)
        {
            // Logically, walk through all Unicode characters, noting the start
            // and end of each range for which filter.contain(c) is
            // true.  Add each range to a set.
            //
            // To improve performance, use an inclusions set which
            // encodes information about character ranges that are known
            // to have identical properties.
            // getInclusions(src) contains exactly the first characters of
            // same-value ranges for the given properties "source".

            Clear();

            int startHasProperty = -1;
            UnicodeSet inclusions = GetInclusions(src);
            int limitRange = inclusions.RangeCount;

            for (int j = 0; j < limitRange; ++j)
            {
                // get current range
                int start = inclusions.GetRangeStart(j);
                int end = inclusions.GetRangeEnd(j);

                // for all the code points in the range, process
                for (int ch = start; ch <= end; ++ch)
                {
                    // only add to the unicodeset on inflection points --
                    // where the hasProperty value changes to false
                    if (filter.Contains(ch))
                    {
                        if (startHasProperty < 0)
                        {
                            startHasProperty = ch;
                        }
                    }
                    else if (startHasProperty >= 0)
                    {
                        AddUnchecked(startHasProperty, ch - 1);
                        startHasProperty = -1;
                    }
                }
            }
            if (startHasProperty >= 0)
            {
                AddUnchecked(startHasProperty, 0x10FFFF);
            }

            return this;
        }

        /// <summary>
        /// Remove leading and trailing Pattern_White_Space and compress
        /// internal Pattern_White_Space to a single space character.
        /// </summary>
        private static string MungeCharName(string source)
        {
            source = PatternProps.TrimWhiteSpace(source);
            StringBuilder buf = null;
            for (int i = 0; i < source.Length; ++i)
            {
                char ch = source[i];
                if (PatternProps.IsWhiteSpace(ch))
                {
                    if (buf == null)
                    {
                        buf = new StringBuilder().Append(source, 0, i);
                    }
                    else if (buf[buf.Length - 1] == ' ')
                    {
                        continue;
                    }
                    ch = ' '; // convert to ' '
                }
                if (buf != null)
                {
                    buf.Append(ch);
                }
            }
            return buf == null ? source : buf.ToString();
        }

        //----------------------------------------------------------------
        // Property set API
        //----------------------------------------------------------------

        /// <summary>
        /// Modifies this set to contain those code points which have the
        /// given value for the given binary or enumerated property, as
        /// returned by <see cref="UCharacter.GetInt32PropertyValue(int, UProperty)"/>.  
        /// Prior contents of this set are lost.
        /// </summary>
        /// <param name="prop">
        /// A property in the range
        /// <list type="bullet">
        ///     <item><description><see cref="UProperty.Binary_Start"/>..<see cref="UProperty.Binary_Limit"/>-1 or</description></item>
        ///     <item><description><see cref="UProperty.Int_Start"/>..<see cref="UProperty.Int_Limit"/>-1 or</description></item>
        ///     <item><description><see cref="UProperty.Mask_Start"/>..<see cref="UProperty.Mask_Limit"/>-1</description></item>
        /// </list>
        /// </param>
        /// <param name="value">
        /// A value in the range <see cref="UCharacter.GetIntPropertyMinValue(UProperty)"/>..
        /// <see cref="UCharacter.GetIntPropertyMaxValue(UProperty)"/>, with one exception.
        /// If prop is <see cref="UProperty.General_Category_Mask"/>, then value should not be
        /// a <see cref="UCharacter.GetType(int)"/> result, but rather a mask value produced
        /// by logically ORing (1 &lt;&lt; <see cref="UCharacter.GetType(int)"/>) values together.
        /// <para/>
        /// This allows grouped categories such as [:L:] to be represented.
        /// </param>
        /// <returns>A reference to this set.</returns>
        /// <stable>ICU 2.4</stable>
        public virtual UnicodeSet ApplyInt32PropertyValue(UProperty prop, int value)
        {
            CheckFrozen();
            if (prop == UProperty.General_Category_Mask)
            {
                ApplyFilter(new GeneralCategoryMaskFilter(value), UCharacterProperty.SRC_CHAR);
            }
            else if (prop == UProperty.Script_Extensions)
            {
                ApplyFilter(new ScriptExtensionsFilter(value), UCharacterProperty.SRC_PROPSVEC);
            }
            else
            {
                ApplyFilter(new Int32PropertyFilter(prop, value), UCharacterProperty.Instance.GetSource(prop));
            }
            return this;
        }

        /// <summary>
        /// Modifies this set to contain those code points which have the
        /// given value for the given property.  Prior contents of this
        /// set are lost.
        /// </summary>
        /// <param name="propertyAlias">
        /// A property alias, either short or long.
        /// The name is matched loosely.  See PropertyAliases.txt for names
        /// and a description of loose matching.  If the value string is
        /// empty, then this string is interpreted as either a
        /// General_Category value alias, a Script value alias, a binary
        /// property alias, or a special ID.  Special IDs are matched
        /// loosely and correspond to the following sets:
        /// <list type="bullet">
        ///     <item><description>"ANY" = [\\u0000-\\U0010FFFF]</description></item>
        ///     <item><description>"ASCII" = [\\u0000-\\u007F]</description></item>
        /// </list>
        /// </param>
        /// <param name="valueAlias">
        /// A value alias, either short or long.  The
        /// name is matched loosely.  See PropertyValueAliases.txt for
        /// names and a description of loose matching.  In addition to
        /// aliases listed, numeric values and canonical combining classes
        /// may be expressed numerically, e.g., ("nv", "0.5") or ("ccc",
        /// "220").  The value string may also be empty.
        /// </param>
        /// <returns>A reference to this set.</returns>
        /// <stable>ICU 2.4</stable>
        public virtual UnicodeSet ApplyPropertyAlias(string propertyAlias, string valueAlias)
        {
            return ApplyPropertyAlias(propertyAlias, valueAlias, null);
        }

        /// <summary>
        /// Modifies this set to contain those code points which have the
        /// given value for the given property.  Prior contents of this
        /// set are lost.
        /// </summary>
        /// <param name="propertyAlias">A string of the property alias.</param>
        /// <param name="valueAlias">A string of the value alias.</param>
        /// <param name="symbols">If not null, then symbols are first called to see if a property
        /// is available. If true, then everything else is skipped.</param>
        /// <returns>This set.</returns>
        /// <stable>ICU 3.2</stable>
        public virtual UnicodeSet ApplyPropertyAlias(string propertyAlias,
                string valueAlias, ISymbolTable symbols)
        {
            CheckFrozen();
            UProperty p;
            UProperty v;
            bool invert = false;

            if (symbols != null
                    && (symbols is XSymbolTable)
                        && ((XSymbolTable)symbols).ApplyPropertyAlias(propertyAlias, valueAlias, this))
            {
                return this;
            }

            if (XSYMBOL_TABLE != null)
            {
                if (XSYMBOL_TABLE.ApplyPropertyAlias(propertyAlias, valueAlias, this))
                {
                    return this;
                }
            }

            if (valueAlias.Length > 0)
            {
                p = (UProperty)UCharacter.GetPropertyEnum(propertyAlias);

                // Treat gc as gcm
                if (p == UProperty.General_Category)
                {
                    p = UProperty.General_Category_Mask;
                }

#pragma warning disable 612, 618
                if ((p >= UProperty.Binary_Start && p < UProperty.Binary_Limit) ||
                        (p >= UProperty.Int_Start && p < UProperty.Int_Limit) ||
                        (p >= UProperty.Mask_Start && p < UProperty.Mask_Limit))
#pragma warning restore 612, 618
                {
                    // ICU4N specific - use safe methods that don't throw exceptions -
                    // we only throw if absolutely necessary.
                    int v2;
                    if (UCharacter.TryGetPropertyValueEnum(p, valueAlias, out v2))
                    {
                        v = (UProperty)v2;
                    }
                    else
                    {
                        // Handle numeric CCC
                        if (p == UProperty.Canonical_Combining_Class ||
                                p == UProperty.Lead_Canonical_Combining_Class ||
                                p == UProperty.Trail_Canonical_Combining_Class)
                        {
                            if (int.TryParse(PatternProps.TrimWhiteSpace(valueAlias), NumberStyles.Integer, CultureInfo.InvariantCulture, out v2))
                            {
                                // Anything between 0 and 255 is valid even if unused.
                                if (v2 < 0 || v2 > 255)
                                    throw new ArgumentException(string.Format("{0} is not a valid Property Value Enum.", valueAlias));

                                v = (UProperty)v2;
                            }
                            else
                            {
                                throw new ArgumentException(string.Format("{0} is not a valid Property Value Enum.", valueAlias));
                            }
                        }
                        else
                        {
                            throw new ArgumentException(string.Format("{0} is not a valid Property Value Enum.", valueAlias));
                        }
                    }
                }

                else
                {
                    switch (p)
                    {
                        case UProperty.Numeric_Value:
                            {
                                double value = double.Parse(PatternProps.TrimWhiteSpace(valueAlias), CultureInfo.InvariantCulture);
                                ApplyFilter(new NumericValueFilter(value), UCharacterProperty.SRC_CHAR);
                                return this;
                            }
                        case UProperty.Name:
                            {
                                // Must munge name, since
                                // UCharacter.charFromName() does not do
                                // 'loose' matching.
                                string buf = MungeCharName(valueAlias);
                                int ch = UCharacter.GetCharFromExtendedName(buf);
                                if (ch == -1)
                                {
                                    throw new ArgumentException("Invalid character name");
                                }
                                Clear();
                                AddUnchecked(ch);
                                return this;
                            }
#pragma warning disable 612, 618
                        case UProperty.Unicode_1_Name:
                            // ICU 49 deprecates the Unicode_1_Name property APIs.
                            throw new ArgumentException("Unicode_1_Name (na1) not supported");
#pragma warning restore 612, 618
                        case UProperty.Age:
                            {
                                // Must munge name, since
                                // VersionInfo.getInstance() does not do
                                // 'loose' matching.
                                VersionInfo version = VersionInfo.GetInstance(MungeCharName(valueAlias));
                                ApplyFilter(new VersionFilter(version), UCharacterProperty.SRC_PROPSVEC);
                                return this;
                            }
                        case UProperty.Script_Extensions:
                            v = (UProperty)UCharacter.GetPropertyValueEnum(UProperty.Script, valueAlias);
                            // fall through to calling applyIntPropertyValue()
                            break;
                        default:
                            // p is a non-binary, non-enumerated property that we
                            // don't support (yet).
                            throw new ArgumentException("Unsupported property");
                    }
                }
            }

            else
            {
                // valueAlias is empty.  Interpret as General Category, Script,
                // Binary property, or ANY or ASCII.  Upon success, p and v will
                // be set.
                UPropertyAliases pnames = UPropertyAliases.Instance;
                p = UProperty.General_Category_Mask;
                v = (UProperty)pnames.GetPropertyValueEnum(p, propertyAlias);
#pragma warning disable 612, 618
                if (v == UProperty.Undefined)
                {
                    p = UProperty.Script;
                    v = (UProperty)pnames.GetPropertyValueEnum(p, propertyAlias);
                    if (v == UProperty.Undefined)
                    {
                        p = (UProperty)pnames.GetPropertyEnum(propertyAlias);
                        if (p == UProperty.Undefined)
                        {
                            p = (UProperty)(-1);
                        }
                        if (p >= UProperty.Binary_Start && p < UProperty.Binary_Limit)
#pragma warning restore 612, 618
                        {
                            v = (UProperty)1;
                        }
                        else if ((int)p == -1)
                        {
                            if (0 == UPropertyAliases.Compare(ANY_ID, propertyAlias))
                            {
                                Set(MinValue, MaxValue);
                                return this;
                            }
                            else if (0 == UPropertyAliases.Compare(ASCII_ID, propertyAlias))
                            {
                                Set(0, 0x7F);
                                return this;
                            }
                            else if (0 == UPropertyAliases.Compare(ASSIGNED, propertyAlias))
                            {
                                // [:Assigned:]=[:^Cn:]
                                p = UProperty.General_Category_Mask;
                                v = (UProperty)(1 << UCharacter.UNASSIGNED);
                                invert = true;
                            }
                            else
                            {
                                // Property name was never matched.
                                throw new ArgumentException("Invalid property alias: " + propertyAlias + "=" + valueAlias);
                            }
                        }
                        else
                        {
                            // Valid propery name, but it isn't binary, so the value
                            // must be supplied.
                            throw new ArgumentException("Missing property value");
                        }
                    }
                }
            }

            ApplyInt32PropertyValue(p, (int)v);
            if (invert)
            {
                Complement();
            }

            return this;
        }

        //----------------------------------------------------------------
        // Property set patterns
        //----------------------------------------------------------------

        /// <summary>
        /// Return true if the given position, in the given pattern, appears
        /// to be the start of a property set pattern.
        /// </summary>
        private static bool ResemblesPropertyPattern(string pattern, int pos)
        {
            // Patterns are at least 5 characters long
            if ((pos + 5) > pattern.Length)
            {
                return false;
            }

            // Look for an opening [:, [:^, \p, or \P
            return pattern.RegionMatches(pos, "[:", 0, 2) ||
                    pattern.RegionMatches(true, pos, "\\p", 0, 2) ||
                    pattern.RegionMatches(pos, "\\N", 0, 2);
        }

        /// <summary>
        /// Return true if the given iterator appears to point at a
        /// property pattern.  Regardless of the result, return with the
        /// iterator unchanged.
        /// </summary>
        /// <param name="chars">Iterator over the pattern characters.  Upon return
        /// it will be unchanged.</param>
        /// <param name="iterOpts"><see cref="RuleCharacterIteratorOptions"/> options.</param>
        private static bool ResemblesPropertyPattern(RuleCharacterIterator chars,
                RuleCharacterIteratorOptions iterOpts)
        {
            bool result = false;
            iterOpts &= ~RuleCharacterIteratorOptions.ParseEscapes;
            Object pos = chars.GetPos(null);
            int c = chars.Next(iterOpts);
            if (c == '[' || c == '\\')
            {
                int d = chars.Next(iterOpts & ~RuleCharacterIteratorOptions.SkipWhitespace);
                result = (c == '[') ? (d == ':') :
                    (d == 'N' || d == 'p' || d == 'P');
            }
            chars.SetPos(pos);
            return result;
        }

        /// <summary>
        /// Parse the given property pattern at the given parse position.
        /// </summary>
        private UnicodeSet ApplyPropertyPattern(string pattern, ParsePosition ppos, ISymbolTable symbols)
        {
            int pos = ppos.Index;

            // On entry, ppos should point to one of the following locations:

            // Minimum length is 5 characters, e.g. \p{L}
            if ((pos + 5) > pattern.Length)
            {
                return null;
            }

            bool posix = false; // true for [:pat:], false for \p{pat} \P{pat} \N{pat}
            bool isName = false; // true for \N{pat}, o/w false
            bool invert = false;

            // Look for an opening [:, [:^, \p, or \P
            if (pattern.RegionMatches(pos, "[:", 0, 2))
            {
                posix = true;
                pos = PatternProps.SkipWhiteSpace(pattern, (pos + 2));
                if (pos < pattern.Length && pattern[pos] == '^')
                {
                    ++pos;
                    invert = true;
                }
            }
            else if (pattern.RegionMatches(true, pos, "\\p", 0, 2) ||
                  pattern.RegionMatches(pos, "\\N", 0, 2))
            {
                char c = pattern[pos + 1];
                invert = (c == 'P');
                isName = (c == 'N');
                pos = PatternProps.SkipWhiteSpace(pattern, (pos + 2));
                if (pos == pattern.Length || pattern[pos++] != '{')
                {
                    // Syntax error; "\p" or "\P" not followed by "{"
                    return null;
                }
            }
            else
            {
                // Open delimiter not seen
                return null;
            }

            // Look for the matching close delimiter, either :] or }
            int close = pattern.IndexOf(posix ? ":]" : "}", pos);
            if (close < 0)
            {
                // Syntax error; close delimiter missing
                return null;
            }

            // Look for an '=' sign.  If this is present, we will parse a
            // medium \p{gc=Cf} or long \p{GeneralCategory=Format}
            // pattern.
            int equals = pattern.IndexOf('=', pos);
            string propName, valueName;
            if (equals >= 0 && equals < close && !isName)
            {
                // Equals seen; parse medium/long pattern
                propName = pattern.Substring(pos, equals - pos); // ICU4N: Corrected 2nd parameter
                valueName = pattern.Substring(equals + 1, close - (equals + 1)); // ICU4N: Corrected 2nd parameter
            }

            else
            {
                // Handle case where no '=' is seen, and \N{}
                propName = pattern.Substring(pos, close - pos); // ICU4N: Corrected 2nd parameter
                valueName = "";

                // Handle \N{name}
                if (isName)
                {
                    // This is a little inefficient since it means we have to
                    // parse "na" back to UProperty.NAME even though we already
                    // know it's UProperty.NAME.  If we refactor the API to
                    // support args of (int, String) then we can remove
                    // "na" and make this a little more efficient.
                    valueName = propName;
                    propName = "na";
                }
            }

            ApplyPropertyAlias(propName, valueName, symbols);

            if (invert)
            {
                Complement();
            }

            // Move to the limit position after the close delimiter
            ppos.Index = close + (posix ? 2 : 1);

            return this;
        }

        /// <summary>
        /// Parse a property pattern.
        /// </summary>
        /// <param name="chars">Iterator over the pattern characters.  Upon return
        /// it will be advanced to the first character after the parsed
        /// pattern, or the end of the iteration if all characters are
        /// parsed.
        /// </param>
        /// <param name="rebuiltPat">The pattern that was parsed, rebuilt or
        /// copied from the input pattern, as appropriate.</param>
        /// <param name="symbols">TODO</param>
        private void ApplyPropertyPattern(RuleCharacterIterator chars,
            IAppendable rebuiltPat, ISymbolTable symbols)
        {
            string patStr = chars.Lookahead();
            ParsePosition pos = new ParsePosition(0);
            ApplyPropertyPattern(patStr, pos, symbols);
            if (pos.Index == 0)
            {
                SyntaxError(chars, "Invalid property pattern");
            }
            chars.Jumpahead(pos.Index);
            Append(rebuiltPat, patStr.Substring(0, pos.Index - 0)); // ICU4N: Checked 2nd substring parameter
        }

        //----------------------------------------------------------------
        // Case folding API
        //----------------------------------------------------------------

        /// <summary>
        /// Bitmask for <see cref="UnicodeSet.UnicodeSet(string, int)"/> constructor, <see cref="ApplyPattern(string, int)"/>, and <see cref="CloseOver(int)"/>.
        /// indicating letter case.  This may be ORed together with other
        /// selectors.
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public static readonly int IGNORE_SPACE = 1; // ICU4N TODO: API - make [Flags] enum

        /// <summary>
        /// Bitmask for <see cref="UnicodeSet.UnicodeSet(string, int)"/> constructor, <see cref="ApplyPattern(string, int)"/>, and <see cref="CloseOver(int)"/>.
        /// indicating letter case.  This may be ORed together with other
        /// selectors.
        /// <para/>
        /// Enable case insensitive matching.  E.g., "[ab]" with this flag
        /// will match 'a', 'A', 'b', and 'B'.  "[^ab]" with this flag will
        /// match all except 'a', 'A', 'b', and 'B'. This performs a full
        /// closure over case mappings, e.g. U+017F for s.
        /// <para/>
        /// The resulting set is a superset of the input for the code points but
        /// not for the strings.
        /// It performs a case mapping closure of the code points and adds
        /// full case folding strings for the code points, and reduces strings of
        /// the original set to their full case folding equivalents.
        /// </summary>
        /// <remarks>
        /// This is designed for case-insensitive matches, for example
        /// in regular expressions. The full code point case closure allows checking of
        /// an input character directly against the closure set.
        /// Strings are matched by comparing the case-folded form from the closure
        /// set with an incremental case folding of the string in question.
        /// <para/>
        /// The closure set will also contain single code points if the original
        /// set contained case-equivalent strings (like U+00DF for "ss" or "Ss" etc.).
        /// This is not necessary (that is, redundant) for the above matching method
        /// but results in the same closure sets regardless of whether the original
        /// set contained the code point or a string.
        /// </remarks>
        /// <stable>ICU 3.8</stable>
        public static readonly int CASE = 2; // ICU4N TODO: API - make [Flags] enum

        /// <summary>
        /// Alias for <see cref="UnicodeSet.CASE"/>, for ease of porting from C++ where ICU4C
        /// also has both USET_CASE and USET_CASE_INSENSITIVE (see uset.h).
        /// </summary>
        /// <seealso cref="CASE"/>
        /// <stable>ICU 3.4</stable>
        public static readonly int CASE_INSENSITIVE = 2; // ICU4N TODO: API - make [Flags] enum

        /// <summary>
        /// Bitmask for <see cref="UnicodeSet.UnicodeSet(string, int)"/> constructor, <see cref="ApplyPattern(string, int)"/>, and <see cref="CloseOver(int)"/>.
        /// indicating letter case.  This may be ORed together with other
        /// selectors.
        /// <para/>
        /// Enable case insensitive matching.  E.g., "[ab]" with this flag
        /// will match 'a', 'A', 'b', and 'B'.  "[^ab]" with this flag will
        /// match all except 'a', 'A', 'b', and 'B'. This adds the lower-,
        /// title-, and uppercase mappings as well as the case folding
        /// of each existing element in the set.
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public static readonly int ADD_CASE_MAPPINGS = 4; // ICU4N TODO: API - make [Flags] enum

        //  add the result of a full case mapping to the set
        //  use str as a temporary string to avoid constructing one
        private static void AddCaseMapping(UnicodeSet set, int result, StringBuilder full)
        {
            if (result >= 0)
            {
                if (result > UCaseProps.MAX_STRING_LENGTH)
                {
                    // add a single-code point case mapping
                    set.Add(result);
                }
                else
                {
                    // add a string case mapping from full with length result
                    set.Add(full.ToString());
                    full.Length = 0;
                }
            }
            // result < 0: the code point mapped to itself, no need to add it
            // see UCaseProps
        }

        /// <summary>
        /// Close this set over the given <paramref name="attribute"/>. 
        /// </summary>
        /// <remarks>
        /// For the attribute <see cref="CASE"/>, the result is to modify 
        /// this set so that:
        /// <list type="number">
        ///     <item><description>
        ///         For each character or string 'a' in this set, all strings
        ///         'b' such that FoldCase(a) == FoldCase(b) are added to this set.
        ///         (For most 'a' that are single characters, 'b' will have
        ///         b.Length == 1.)
        ///     </description></item>
        ///     <item><description>
        ///         For each string 'e' in the resulting set, if e !=
        ///         FoldCase(e), 'e' will be removed.
        ///     </description></item>
        /// </list>
        /// <para/>
        /// Example: [aq\u00DF{Bc}{bC}{Fi}] =&gt; [aAqQ\u00DF\uFB01{ss}{bc}{fi}]
        /// <para/>
        /// (Here FoldCase(x) refers to the operation
        /// UCharacter.FoldCase(x, true), and a == b actually denotes
        /// a.Equals(b), not pointer comparison.)
        /// </remarks>
        /// <param name="attribute">Bitmask for attributes to close over.
        /// Currently only the CASE bit is supported.  Any undefined bits
        /// are ignored.
        /// </param>
        /// <returns>A reference to this set.</returns>
        /// <stable>ICU 3.8</stable>
        public virtual UnicodeSet CloseOver(int attribute)
        {
            CheckFrozen();
            if ((attribute & (CASE | ADD_CASE_MAPPINGS)) != 0)
            {
                UCaseProps csp = UCaseProps.Instance;
                UnicodeSet foldSet = new UnicodeSet(this);
                ULocale root = ULocale.ROOT;

                // start with input set to guarantee inclusion
                // CASE: remove strings because the strings will actually be reduced (folded);
                //       therefore, start with no strings and add only those needed
                if ((attribute & CASE) != 0)
                {
                    foldSet.strings.Clear();
                }

                int n = RangeCount;
                int result;
                StringBuilder full = new StringBuilder();

                for (int i = 0; i < n; ++i)
                {
                    int start = GetRangeStart(i);
                    int end = GetRangeEnd(i);

                    if ((attribute & CASE) != 0)
                    {
                        // full case closure
                        for (int cp = start; cp <= end; ++cp)
                        {
                            csp.AddCaseClosure(cp, foldSet);
                        }
                    }
                    else
                    {
                        // add case mappings
                        // (does not add long s for regular s, or Kelvin for k, for example)
                        for (int cp = start; cp <= end; ++cp)
                        {
                            result = csp.ToFullLower(cp, null, full, UCaseProps.LOC_ROOT);
                            AddCaseMapping(foldSet, result, full);

                            result = csp.ToFullTitle(cp, null, full, UCaseProps.LOC_ROOT);
                            AddCaseMapping(foldSet, result, full);

                            result = csp.ToFullUpper(cp, null, full, UCaseProps.LOC_ROOT);
                            AddCaseMapping(foldSet, result, full);

                            result = csp.ToFullFolding(cp, full, 0);
                            AddCaseMapping(foldSet, result, full);
                        }
                    }
                }
                if (strings.Count > 0)
                {
                    if ((attribute & CASE) != 0)
                    {
                        foreach (String s in strings)
                        {
                            string str = UCharacter.FoldCase(s, 0);
                            if (!csp.AddStringCaseClosure(str, foldSet))
                            {
                                foldSet.Add(str); // does not map to code points: add the folded string itself
                            }
                        }
                    }
                    else
                    {
                        BreakIterator bi = BreakIterator.GetWordInstance(root);
                        foreach (string str in strings)
                        {
                            // TODO: call lower-level functions
                            foldSet.Add(UCharacter.ToLower(root, str));
                            foldSet.Add(UCharacter.ToTitleCase(root, str, bi));
                            foldSet.Add(UCharacter.ToUpper(root, str));
                            foldSet.Add(UCharacter.FoldCase(str, 0));
                        }
                    }
                }
                Set(foldSet);
            }
            return this;
        }

        /// <summary>
        /// Internal class for customizing <see cref="UnicodeSet"/> parsing of properties.
        /// </summary>
        /// <author>medavis</author>
        /// <draft>ICU3.8 (retain)</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        // TODO: extend to allow customizing of codepoint ranges
        abstract public class XSymbolTable : ISymbolTable  // ICU4N TODO: API - de-nest ?
        {
            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <draft>ICU3.8 (retain)</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public XSymbolTable() { }

            /// <summary>
            /// Supplies default implementation for <see cref="ISymbolTable"/> (no action).
            /// </summary>
            /// <draft>ICU3.8 (retain)</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public virtual IUnicodeMatcher LookupMatcher(int i)
            {
                return null;
            }

            /// <summary>
            /// Override the interpretation of the sequence [:<paramref name="propertyName"/>=<paramref name="propertyValue"/>:] (and its negated and Perl-style
            /// variant). The <paramref name="propertyName"/> and <paramref name="propertyValue"/> may be existing Unicode aliases, or may not be.
            /// <para/>
            /// This routine will be called whenever the parsing of a UnicodeSet pattern finds such a
            /// <paramref name="propertyName"/>+<paramref name="propertyValue"/> combination.
            /// </summary>
            /// <param name="propertyName">The name of the property.</param>
            /// <param name="propertyValue">The name of the property value.</param>
            /// <param name="result"><see cref="UnicodeSet"/> value to change
            /// a set to which the characters having the <paramref name="propertyName"/>+<paramref name="propertyValue"/> are to be added.
            /// </param>
            /// <returns>true if the <paramref name="propertyName"/>+<paramref name="propertyValue"/> combination is to be overridden, and the characters
            /// with that property have been added to the <see cref="UnicodeSet"/>, and returns false if the
            /// <paramref name="propertyName"/>+<paramref name="propertyValue"/> combination is not recognized (in which case result is unaltered).
            /// </returns>
            /// <draft>ICU3.8 (retain)</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public virtual bool ApplyPropertyAlias(string propertyName, string propertyValue, UnicodeSet result)
            {
                return false;
            }

            /// <summary>
            /// Supplies default implementation for <see cref="ISymbolTable"/> (no action).
            /// </summary>
            /// <draft>ICU3.8 (retain)</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public virtual char[] Lookup(string s)
            {
                return null;
            }

            /// <summary>
            /// Supplies default implementation for <see cref="ISymbolTable"/> (no action).
            /// </summary>
            /// <draft>ICU3.8 (retain)</draft>
            /// <provisional>This API might change or be removed in a future release.</provisional>
            public virtual string ParseReference(string text, ParsePosition pos, int limit)
            {
                return null;
            }
        }

        /// <summary>
        /// Is this frozen, according to the <see cref="IFreezable{T}"/> interface?
        /// </summary>
        /// <stable>ICU 3.8</stable>
        public virtual bool IsFrozen
        {
            get { return (bmpSet != null || stringSpan != null); }
        }

        /// <summary>
        /// Freeze this class, according to the <see cref="IFreezable{T}"/> interface.
        /// </summary>
        /// <returns>This.</returns>
        /// <stable>ICU 4.4</stable>
        public virtual UnicodeSet Freeze()
        {
            if (!IsFrozen)
            {
                // Do most of what compact() does before freezing because
                // compact() will not work when the set is frozen.
                // Small modification: Don't shrink if the savings would be tiny (<=GROW_EXTRA).

                // Delete buffer first to defragment memory less.
                buffer = null;
                if (list.Length > (len + GROW_EXTRA))
                {
                    // Make the capacity equal to len or 1.
                    // We don't want to realloc of 0 size.
                    int capacity = (len == 0) ? 1 : len;
                    int[] oldList = list;
                    list = new int[capacity];
                    for (int i = capacity; i-- > 0;)
                    {
                        list[i] = oldList[i];
                    }
                }

                // Optimize contains() and span() and similar functions.
                if (strings.Count > 0)
                {
                    stringSpan = new UnicodeSetStringSpan(this, new List<string>(strings), UnicodeSetStringSpan.ALL);
                }
                if (stringSpan == null || !stringSpan.NeedsStringSpanUTF16)
                {
                    // Optimize for code point spans.
                    // There are no strings, or
                    // all strings are irrelevant for span() etc. because
                    // all of each string's code points are contained in this set.
                    // However, fully contained strings are relevant for spanAndCount(),
                    // so we create both objects.
                    bmpSet = new BMPSet(list, len);
                }
            }
            return this;
        }

        // ICU4N specific - Span(ICharSequence s, SpanCondition spanCondition) moved to UnicodeSetExtension.tt

        // ICU4N specific - Span(ICharSequence s, int start, SpanCondition spanCondition) moved to UnicodeSetExtension.tt

        // ICU4N specific - SpanAndCount(ICharSequence s, int start, SpanCondition spanCondition, out int outCount) moved to UnicodeSetExtension.tt

        // ICU4N specific - SpanCodePointsAndCount(ICharSequence s, int start,
        //    SpanCondition spanCondition, out int outCount) moved to UnicodeSetExtension.tt

        // ICU4N specific - SpanBack(ICharSequence s, SpanCondition spanCondition) moved to UnicodeSetExtension.tt

        // ICU4N specific - SpanBack(ICharSequence s, int fromIndex, SpanCondition spanCondition) moved to UnicodeSetExtension.tt


        /// <summary>
        /// Clone a thawed version of this class, according to the <see cref="IFreezable{T}"/> interface.
        /// </summary>
        /// <returns>The clone, not frozen.</returns>
        /// <stable>ICU 4.4</stable>
        public virtual UnicodeSet CloneAsThawed()
        {
            UnicodeSet result = new UnicodeSet(this);
            Debug.Assert(!result.IsFrozen);
            return result;
        }

        // internal function
        private void CheckFrozen()
        {
            if (IsFrozen)
            {
                throw new NotSupportedException("Attempt to modify frozen object");
            }
        }

        // ************************
        // Additional methods for integration with Generics and Collections
        // ************************

        /// <summary>
        /// A struct-like class used for iteration through ranges, for faster iteration than by String.
        /// Read about the restrictions on usage in <see cref="UnicodeSet.Ranges"/>.
        /// </summary>
        /// <stable>ICU 54</stable>
        public class EntryRange // ICU4N TODO: API - de-nest ?
        {
            /// <summary>
            /// The starting code point of the range.
            /// </summary>
            /// <stable>ICU 54</stable>
            public int Codepoint { get; set; }

            /// <summary>
            /// The ending code point of the range.
            /// </summary>
            /// <stable>ICU 54</stable>
            public int CodepointEnd { get; set; }


            internal EntryRange()
            {
            }

            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            /// <returns>A string that represents the current object.</returns>
            /// <stable>ICU 54</stable>
            public override string ToString()
            {
                StringBuilder b = new StringBuilder();
                return (
                        Codepoint == CodepointEnd ? AppendToPat(b, Codepoint, false)
                                : AppendToPat(AppendToPat(b, Codepoint, false).Append('-'), CodepointEnd, false))
                                .ToString();
            }
        }

        /// <summary>
        /// Provide for faster enumeration than by <see cref="string"/>. Returns an Enumerable/Enumerator over ranges of code points.
        /// The <see cref="UnicodeSet"/> must not be altered during the iteration.
        /// The <see cref="EntryRange"/> instance is the same each time; the contents are just reset.
        /// </summary>
        /// <remarks>
        /// <b>Warning: </b>To iterate over the full contents, you have to also iterate over the strings.
        /// <para/>
        /// <b>Warning: </b>For speed, <see cref="UnicodeSet"/> iteration does not check for concurrent modification.
        /// Do not alter the <see cref="UnicodeSet"/> while iterating.
        /// <code>
        /// // Sample code
        /// foreach (EntryRange range in us1.Ranges)
        /// {
        ///     // do something with code points between range.Codepoint and range.CodepointEnd;
        /// }
        /// foreach (string s in us1.Strings)
        /// {
        ///     // do something with each string;
        /// }
        /// </code>
        /// </remarks>
        /// <stable>ICU 54</stable>
        public IEnumerable<EntryRange> Ranges
        {
            get { return new EntryRangeEnumerable(this); }
        }

        private class EntryRangeEnumerable : IEnumerable<EntryRange>
        {
            internal readonly UnicodeSet outerInstance;

            internal EntryRangeEnumerable(UnicodeSet outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public virtual IEnumerator<EntryRange> GetEnumerator()
            {
                return new EntryRangeEnumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new EntryRangeEnumerator(this);
            }
        }

        private class EntryRangeEnumerator : IEnumerator<EntryRange>
        {
            private int pos;
            private EntryRange result = new EntryRange();
            private readonly EntryRangeEnumerable outerInstance;

            internal EntryRangeEnumerator(EntryRangeEnumerable outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public virtual EntryRange Current
            {
                get { return result; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public void Dispose()
            {
                // Nothing to do
            }

            public virtual bool MoveNext()
            {
                if (!HasNext)
                    return false;
                return Next() != null;
            }

            public virtual void Reset()
            {
                throw new NotSupportedException();
            }

            private bool HasNext
            {
                get { return pos < outerInstance.outerInstance.len - 1; }
            }

            private EntryRange Next()
            {
                if (pos < outerInstance.outerInstance.len - 1)
                {
                    result.Codepoint = outerInstance.outerInstance.list[pos++];
                    result.CodepointEnd = outerInstance.outerInstance.list[pos++] - 1;
                }
                else
                {
                    return null;
                }
                return result;
            }
            // ICU4N NOTE: Remove() not supported in .NET
        }

        /// <summary>
        /// Returns a string enumerator. Uses the same order of iteration as <see cref="UnicodeSetIterator"/>.
        /// <para/>
        /// <b>Warning: </b>For speed, <see cref="UnicodeSet"/> iteration does not check for concurrent modification.
        /// <para/>
        /// Do not alter the <see cref="UnicodeSet"/> while iterating.
        /// </summary>
        /// <seealso cref="IEnumerable{T}.GetEnumerator()"/>
        /// <stable>ICU 4.4</stable>
        public virtual IEnumerator<string> GetEnumerator()
        {
            return new UnicodeSetEnumerator2(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // Cover for string iteration.
        private class UnicodeSetEnumerator2 : IEnumerator<string>
        {
            // Invariants:
            // sourceList != null then sourceList[item] is a valid character
            // sourceList == null then delegates to stringIterator
            private int[] sourceList;
            private int len;
            private int item;
            private int current;
            private int limit;
            private ISet<string> sourceStrings;
            private IEnumerator<string> stringIterator;
            private char[] buffer;

            private string currentElement = null;

            internal UnicodeSetEnumerator2(UnicodeSet source)
            {
                // set according to invariants
                len = source.len - 1;
                if (len > 0)
                {
                    sourceStrings = source.strings;
                    sourceList = source.list;
                    current = sourceList[item++];
                    limit = sourceList[item++];
                }
                else
                {
                    stringIterator = source.strings.GetEnumerator();
                    sourceList = null;
                }
            }

            public virtual string Current
            {
                get { return currentElement; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public void Dispose()
            {
                // Nothing to do
            }

            public virtual bool MoveNext()
            {
                if (sourceList == null)
                {
                    bool hasNext = stringIterator.MoveNext();
                    if (hasNext)
                    {
                        currentElement = stringIterator.Current;
                        return true;
                    }
                    return false;
                }

                int codepoint = current++;
                // we have the codepoint we need, but we may need to adjust the state
                if (current >= limit)
                {
                    if (item >= len)
                    {
                        stringIterator = sourceStrings.GetEnumerator();
                        sourceList = null;
                    }
                    else
                    {
                        current = sourceList[item++];
                        limit = sourceList[item++];
                    }
                }
                // Now return. Single code point is easy
                if (codepoint <= 0xFFFF)
                {
                    currentElement = new string(new char[] { (char)codepoint });
                    return true;
                }
                // But .NET lacks a valueOfCodePoint, so we handle ourselves for speed
                // allocate a buffer the first time, to make conversion faster.
                if (buffer == null)
                {
                    buffer = new char[2];
                }
                // compute ourselves, to save tests and calls
                int offset = codepoint - Character.MIN_SUPPLEMENTARY_CODE_POINT;
                buffer[0] = (char)(offset.TripleShift(10) + Character.MIN_HIGH_SURROGATE);
                buffer[1] = (char)((offset & 0x3ff) + Character.MIN_LOW_SURROGATE);
                currentElement = new string(buffer);
                return true;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            // ICU4N NOTE: Remove() not supported in .NET
        }

        // ICU4N specific - ContainsAll<T>(IEnumerable<T> collection) where T : ICharSequence moved to UnicodeSetExtension.tt

        // ICU4N specific - ContainsNone<T>(IEnumerable<T> collection) where T : ICharSequence moved to UnicodeSetExtension.tt

        // ICU4N specific - ContainsSome<T>(IEnumerable<T> collection) where T : ICharSequence moved to UnicodeSetExtension.tt

        // ICU4N specific - AddAll(params ICharSequence[] collection) moved to UnicodeSetExtension.tt

        // ICU4N specific - RemoveAll<T>(IEnumerable<T> collection) where T : ICharSequence moved to UnicodeSetExtension.tt

        // ICU4N specific - RetainAll<T>(IEnumerable<T> collection) where T : ICharSequence moved to UnicodeSetExtension.tt


        /// <summary>
        /// Comparison style enums used by <see cref="CompareTo(UnicodeSet, ComparisonStyle)"/>.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public enum ComparisonStyle // ICU4N TODO: API - de-nest and name according to .NET conventions
        {
            /// <stable>ICU 4.4</stable>
            SHORTER_FIRST,
            /// <stable>ICU 4.4</stable>
            LEXICOGRAPHIC,
            /// <stable>ICU 4.4</stable>
            LONGER_FIRST
        }

        /// <summary>
        /// Compares <see cref="UnicodeSet"/>s, where shorter come first, and otherwise lexigraphically
        /// (according to the comparison of the first characters that differ).
        /// </summary>
        /// <seealso cref="IComparable.CompareTo(object)"/>
        /// <stable>ICU 4.4</stable>
        public virtual int CompareTo(UnicodeSet o)
        {
            return CompareTo(o, ComparisonStyle.SHORTER_FIRST);
        }

        /// <summary>
        /// Compares <see cref="UnicodeSet"/>s, in three different ways.
        /// </summary>
        /// <seealso cref="IComparable.CompareTo(object)"/>
        /// <stable>ICU 4.4</stable>
        public virtual int CompareTo(UnicodeSet o, ComparisonStyle style)
        {
            if (style != ComparisonStyle.LEXICOGRAPHIC)
            {
                int diff = Count - o.Count;
                if (diff != 0)
                {
                    return (diff < 0) == (style == ComparisonStyle.SHORTER_FIRST) ? -1 : 1;
                }
            }
            int result;
            for (int i = 0; ; ++i)
            {
                if (0 != (result = list[i] - o.list[i]))
                {
                    // if either list ran out, compare to the last string
                    if (list[i] == HIGH)
                    {
                        if (strings.Count == 0) return 1;
                        string item = strings.First();
                        return Compare(item, o.list[i]);
                    }
                    if (o.list[i] == HIGH)
                    {
                        if (o.strings.Count == 0) return -1;
                        string item = o.strings.First();
                        int compareResult = Compare(item, list[i]);
                        return compareResult > 0 ? -1 : compareResult < 0 ? 1 : 0; // Reverse the order.
                    }
                    // otherwise return the result if even index, or the reversal if not
                    return (i & 1) == 0 ? result : -result;
                }
                if (list[i] == HIGH)
                {
                    break;
                }
            }
            return Compare(strings, o.strings);
        }

        /// <stable>ICU 4.4</stable>
        public virtual int CompareTo(IEnumerable<string> other)
        {
            return Compare(this, other);
        }

        // ICU4N specific - Compare(ICharSequence str, int codePoint) moved to UnicodeSetExtension.tt

        // ICU4N specific - Compare(int codePoint, ICharSequence str) moved to UnicodeSetExtension.tt

        /// <summary>
        /// Utility to compare two enumerators. Warning: the ordering in enumerables is important. For Collections that are ordered,
        /// like Lists, that is expected. However, Sets in .NET violate Leibniz's law when it comes to iteration.
        /// That means that sets can't be compared directly with this method, unless they are <see cref="SortedSet{T}"/>s without
        /// (or with the same) comparer. Unfortunately, it is impossible to reliably detect in .NET whether subclass of
        /// Collection satisfies the right criteria, so it is left to the user to avoid those circumstances.
        /// </summary>
        /// <typeparam name="T">The type of items to compare.</typeparam>
        /// <stable>ICU 4.4</stable>
        public static int Compare<T>(IEnumerable<T> collection1, IEnumerable<T> collection2) where T : IComparable<T>
        {
#pragma warning disable 612, 618
            return Compare(collection1.GetEnumerator(), collection2.GetEnumerator());
#pragma warning restore 612, 618
        }

        /// <summary>
        /// Utility to compare two enumerators. Warning: the ordering in enumerables is important. For Collections that are ordered,
        /// like Lists, that is expected. However, Sets in .NET violate Leibniz's law when it comes to iteration.
        /// That means that sets can't be compared directly with this method, unless they are <see cref="SortedSet{T}"/>s without
        /// (or with the same) comparer. Unfortunately, it is impossible to reliably detect in .NET whether subclass of
        /// Collection satisfies the right criteria, so it is left to the user to avoid those circumstances.
        /// </summary>
        /// <typeparam name="T">The type of items to compare.</typeparam>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public static int Compare<T>(IEnumerator<T> first, IEnumerator<T> other) where T : IComparable<T>
        {
            while (true)
            {
                if (!first.MoveNext())
                {
                    return other.MoveNext() ? -1 : 0;
                }
                else if (!other.MoveNext())
                {
                    return 1;
                }
                T item1 = first.Current;
                T item2 = other.Current;
                int result = item1.CompareTo(item2);
                if (result != 0)
                {
                    return result;
                }
            }
        }

        /// <summary>
        /// Utility to compare two collections, optionally by size, and then lexicographically.
        /// </summary>
        /// <typeparam name="T">The type of items to compare.</typeparam>
        /// <stable>ICU 4.4</stable>
        public static int Compare<T>(ICollection<T> collection1, ICollection<T> collection2, ComparisonStyle style) where T : IComparable<T>
        {
            if (style != ComparisonStyle.LEXICOGRAPHIC)
            {
                int diff = collection1.Count - collection2.Count;
                if (diff != 0)
                {
                    return (diff < 0) == (style == ComparisonStyle.SHORTER_FIRST) ? -1 : 1;
                }
            }
            return Compare(collection1, collection2);
        }

        /// <summary>
        /// Utility for adding the contents of an enumerable to a collection.
        /// </summary>
        /// <typeparam name="T">The source element type.</typeparam>
        /// <typeparam name="U">The target type (must implement <see cref="ICollection{T}"/>).</typeparam>
        /// <stable>ICU 4.4</stable>
        private static U AddAllTo<T, U>(IEnumerable<T> source, U target) where U : ICollection<T> // ICU4N specific - this is a general copy method and has no business being in UnicodeSet. Changed from public to private.
        {
            foreach (T item in source)
            {
                target.Add(item);
            }
            return target;
        }

        /// <summary>
        /// Utility for adding the contents of an enumerable to a collection.
        /// </summary>
        /// <typeparam name="T">The type of items to add.</typeparam>
        /// <stable>ICU 4.4</stable>
        private static T[] AddAllTo<T>(IEnumerable<T> source, T[] target) // ICU4N specific - this is a general copy method and has no business being in UnicodeSet. Changed from public to private.
        {
            int i = 0;
            foreach (T item in source)
            {
                target[i++] = item;
            }
            return target;
        }

        /// <summary>
        /// For iterating through the strings in the set. Example:
        /// <code>
        /// foreach (string key in myUnicodeSet.Strings)
        /// {
        ///     DoSomethingWith(key);
        /// }
        /// </code>
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public virtual ICollection<string> Strings
        {
            get { return strings.ToUnmodifiableSet(); }
        }

        // ICU4N specific - GetSingleCodePoint(ICharSequence s) moved to UnicodeSetExtension.tt

        /// <summary>
        /// Simplify the ranges in a Unicode set by merging any ranges that are only separated by characters in the <paramref name="dontCare"/> set.
        /// For example, the ranges: \\u2E80-\\u2E99\\u2E9B-\\u2EF3\\u2F00-\\u2FD5\\u2FF0-\\u2FFB\\u3000-\\u303E change to \\u2E80-\\u303E
        /// if the <paramref name="dontCare"/> set includes unassigned characters (for a particular version of Unicode).
        /// </summary>
        /// <param name="dontCare">Set with the don't-care characters for spanning.</param>
        /// <returns>The input set, modified.</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public virtual UnicodeSet AddBridges(UnicodeSet dontCare)
        {
            UnicodeSet notInInput = new UnicodeSet(this).Complement();
            for (UnicodeSetIterator it = new UnicodeSetIterator(notInInput); it.NextRange();)
            {
                if (it.Codepoint != 0 && it.Codepoint != UnicodeSetIterator.IS_STRING && it.CodepointEnd != 0x10FFFF && dontCare.Contains(it.Codepoint, it.CodepointEnd))
                {
                    Add(it.Codepoint, it.CodepointEnd);
                }
            }
            return this;
        }

        // ICU4N specific - FindIn(ICharSequence value, int fromIndex, bool findNot) moved to UnicodeSetExtension.tt

        // ICU4N specific - FindLastIn(ICharSequence value, int fromIndex, bool findNot) moved to UnicodeSetExtension.tt

        // ICU4N specific - StripFrom(ICharSequence source, bool matches) moved to UnicodeSetExtension.tt

        // ICU4N specific - De-nested SpanCondition

        /// <summary>
        /// Get the default symbol table. Null means ordinary processing. For internal use only.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public static XSymbolTable DefaultXSymbolTable
        {
            get { return XSYMBOL_TABLE; }
        }

        /// <summary>
        /// Set the default symbol table. Null means ordinary processing. For internal use only. Will affect all subsequent parsing
        /// of <see cref="UnicodeSet"/>s.
        /// <para/>
        /// WARNING: If this function is used with a UnicodeProperty, and the
        /// Unassigned characters (gc=Cn) are different than in ICU other than in ICU, you MUST call
        /// <c>UnicodeProperty.ResetCacheProperties</c> afterwards. If you then set <see cref="UnicodeSet.DefaultXSymbolTable"/>
        /// with null to clear the value, you MUST also call <c>UnicodeProperty.ResetCacheProperties</c>.
        /// </summary>
        /// <param name="xSymbolTable">The new default symbol table.</param>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public static void SetDefaultXSymbolTable(XSymbolTable xSymbolTable) // ICU4N NOTE: Has side-effect, so isn't a good property candidate
        {
            INCLUSIONS = null; // If the properties override inclusions, these have to be regenerated.
            XSYMBOL_TABLE = xSymbolTable;
        }
    }

    /// <summary>
    /// Argument values for whether <see cref="UnicodeSet.Span(string, int, SpanCondition)"/> and similar functions continue while the current character is contained vs.
    /// not contained in the set.
    /// </summary>
    /// <remarks>
    /// The functionality is straightforward for sets with only single code points, without strings (which is the common
    /// case):
    /// <list type="bullet">
    ///     <item><description><see cref="Contained"/> and <see cref="Simple"/> work the same.</description></item>
    ///     <item><description><see cref="Contained"/> and <see cref="Simple"/> are inverses of <see cref="NotContained"/>.</description></item>
    ///     <item><description><see cref="UnicodeSet.Span(string, int, SpanCondition)"/> and <see cref="UnicodeSet.SpanBack(string, int, SpanCondition)"/> partition any string the
    ///         same way when alternating between Span(<see cref="NotContained"/>) and Span(either "contained" condition).</description></item>
    ///     <item><description>Using a complemented (inverted) set and the opposite span conditions yields the same results.</description></item>
    /// </list>
    /// When a set contains multi-code point strings, then these statements may not be true, depending on the strings in
    /// the set (for example, whether they overlap with each other) and the string that is processed. For a set with
    /// strings:
    /// <list type="bullet">
    ///     <item><description>
    ///         The complement of the set contains the opposite set of code points, but the same set of strings.
    ///         Therefore, complementing both the set and the span conditions may yield different results.
    ///     </description></item>
    ///     <item><description>
    ///         When starting spans at different positions in a string (span(s, ...) vs. span(s+1, ...)) the 
    ///         ends of the spans may be different because a set string may start before the later position.
    ///     </description></item>
    ///     <item><description>
    ///         Span(<see cref="Simple"/>) may be shorter than Span(<see cref="Contained"/>) because it will 
    ///         not recursively try all possible paths. For example, with a set which
    ///         contains the three strings "xy", "xya" and "ax", Span("xyax", <see cref="Contained"/>) will return 4 but span("xyax",
    ///         <see cref="Simple"/>) will return 3. Span(<see cref="Simple"/>) will never be longer than Span(<see cref="Contained"/>).
    ///     </description></item>
    ///     <item><description>
    ///         With either "contained" condition, Span() and SpanBack() may partition a string in different ways. For example,
    ///         with a set which contains the two strings "ab" and "ba", and when processing the string "aba", Span() will yield
    ///         contained/not-contained boundaries of { 0, 2, 3 } while SpanBack() will yield boundaries of { 0, 1, 3 }.
    ///     </description></item>
    /// </list>
    /// Note: If it is important to get the same boundaries whether iterating forward or backward through a string, then
    /// either only Span() should be used and the boundaries cached for backward operation, or an ICU <see cref="BreakIterator"/> could
    /// be used.
    /// <para/>
    /// Note: Unpaired surrogates are treated like surrogate code points. Similarly, set strings match only on code point
    /// boundaries, never in the middle of a surrogate pair.
    /// </remarks>
    /// <stable>ICU 4.4</stable>
    public enum SpanCondition
    {
        /// <summary>
        /// Continues a <see cref="UnicodeSet.Span(string, int, SpanCondition)"/> while there is no set element at the current position.
        /// Increments by one code point at a time.
        /// Stops before the first set element (character or string).
        /// (For code points only, this is like while Contains(current)==false).
        /// </summary>
        /// <remarks>
        /// When <see cref="UnicodeSet.Span(string, int, SpanCondition)"/> returns, the substring between where it started and the position it returned consists only of
        /// characters that are not in the set, and none of its strings overlap with the span.
        /// </remarks>
        /// <stable>ICU 4.4</stable>
        NotContained,

        /// <summary>
        /// Spans the longest substring that is a concatenation of set elements (characters or strings).
        /// (For characters only, this is like while Contains(current)==true).
        /// </summary>
        /// <remarks>
        /// When <see cref="UnicodeSet.Span(string, int, SpanCondition)"/> returns, the substring between where it started and the position it returned consists only of set
        /// elements (characters or strings) that are in the set.
        /// <para/>
        /// If a set contains strings, then the span will be the longest substring for which there
        /// exists at least one non-overlapping concatenation of set elements (characters or strings).
        /// This is equivalent to a POSIX regular expression for <c>(OR of each set element)*</c>.
        /// (.NET/ICU/Perl regex stops at the first match of an OR.)
        /// </remarks>
        /// <stable>ICU 4.4</stable>
        Contained,

        /// <summary>
        /// Continues a <see cref="UnicodeSet.Span(string, int, SpanCondition)"/> while there is a set element at the current position.
        /// Increments by the longest matching element at each position.
        /// (For characters only, this is like while Contains(current)==true).
        /// </summary>
        /// <remarks>
        /// When <see cref="UnicodeSet.Span(string, int, SpanCondition)"/> returns, the substring between where it started and the position it returned consists only of set
        /// elements (characters or strings) that are in the set.
        /// <para/>
        /// If a set only contains single characters, then this is the same as <see cref="Contained"/>.
        /// <para/>
        /// If a set contains strings, then the span will be the longest substring with a match at each position with the
        /// longest single set element (character or string).
        /// <para/>
        /// Use this span condition together with other longest-match algorithms, such as ICU converters
        /// (ucnv_getUnicodeSet()).
        /// </remarks>
        /// <stable>ICU 4.4</stable>
        Simple,

        /// <summary>
        /// One more than the last span condition.
        /// </summary>
        /// <stable>ICU 4.4</stable>
        ConditionCount
    }
}
