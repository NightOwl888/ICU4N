using ICU4N.Globalization;
using ICU4N.Impl;
using ICU4N.Util;
using J2N.Text;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using SingleID = ICU4N.Text.TransliteratorIDParser.SingleID;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    /// <summary>
    /// Direction options for <see cref="Transliterator"/>.
    /// </summary>
    /// <draft>ICU4N 60.1</draft>
    public enum TransliterationDirection
    {
        /// <summary>
        /// Direction constant indicating the forward direction in a transliterator,
        /// e.g., the forward rules of a RuleBasedTransliterator.  An "A-B"
        /// transliterator transliterates A to B when operating in the forward
        /// direction, and B to A when operating in the reverse direction.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        Forward = 0,

        /// <summary>
        /// Direction constant indicating the reverse direction in a transliterator,
        /// e.g., the reverse rules of a RuleBasedTransliterator.  An "A-B"
        /// transliterator transliterates A to B when operating in the forward
        /// direction, and B to A when operating in the reverse direction.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        Reverse = 1
    }

    /// <summary>
    /// The factory interface for transliterators.  Transliterator
    /// subclasses can register factory objects for IDs using the
    /// <see cref="Transliterator.RegisterFactory(string, ITransliteratorFactory)"/>
    /// method of Transliterator.  When invoked, the
    /// factory object will be passed the ID being instantiated.  This
    /// makes it possible to register one factory method to more than
    /// one ID, or for a factory method to parameterize its result
    /// based on the variant.
    /// </summary>
    /// <stable>ICU 2.0</stable>
    public interface ITransliteratorFactory
    {
        /// <summary>
        /// Return a transliterator for the given id.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        Transliterator GetInstance(string id);
    }

    /// <summary>
    /// <see cref="TransliterationPosition"/> structure for incremental transliteration.  This data
    /// structure defines two substrings of the text being
    /// transliterated.  The first region, [<see cref="ContextStart"/>,
    /// <see cref="ContextLimit"/>), defines what characters the transliterator will
    /// read as context.  The second region, [<see cref="Start"/>, <see cref="Limit"/>), defines
    /// what characters will actually be transliterated.  The second
    /// region should be a subset of the first.
    /// <para/>
    /// After a transliteration operation, some of the indices in this
    /// structure will be modified.  See the field descriptions for
    /// details.
    /// <para/>
    /// <see cref="ContextStart"/> &lt;= <see cref="Start"/> &lt;= <see cref="Limit"/> &lt;= <see cref="ContextLimit"/>
    /// <para/>
    /// Note: All index values in this structure must be at code point
    /// boundaries.  That is, none of them may occur between two code units
    /// of a surrogate pair.  If any index does split a surrogate pair,
    /// results are unspecified.
    /// </summary>
    /// <stable>ICU 2.0</stable>
    public class TransliterationPosition
    {
        /// <summary>
        /// Beginning index, inclusive, of the context to be considered for
        /// a transliteration operation.  The transliterator will ignore
        /// anything before this index.  INPUT/OUTPUT parameter: This parameter
        /// is updated by a transliteration operation to reflect the maximum
        /// amount of antecontext needed by a transliterator.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public int ContextStart { get; set; }

        /// <summary>
        /// Ending index, exclusive, of the context to be considered for a
        /// transliteration operation.  The transliterator will ignore
        /// anything at or after this index.  INPUT/OUTPUT parameter: This
        /// parameter is updated to reflect changes in the length of the
        /// text, but points to the same logical position in the text.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public int ContextLimit { get; set; } // ICU4N TODO: API - Change to ContextLength

        /// <summary>
        /// Beginning index, inclusive, of the text to be transliteratd.
        /// INPUT/OUTPUT parameter: This parameter is advanced past
        /// characters that have already been transliterated by a
        /// transliteration operation.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public int Start { get; set; }

        /// <summary>
        /// Ending index, exclusive, of the text to be transliteratd.
        /// INPUT/OUTPUT parameter: This parameter is updated to reflect
        /// changes in the length of the text, but points to the same
        /// logical position in the text.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public int Limit { get; set; } // ICU4N TODO: API - Change to ContextLength

        /// <summary>
        /// Constructs a <see cref="TransliterationPosition"/> object with <see cref="Start"/>, <see cref="Limit"/>,
        /// <see cref="ContextStart"/>, and <see cref="ContextLimit"/> all equal to zero.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public TransliterationPosition()
            : this(0, 0, 0, 0)
        {
        }

        /// <summary>
        /// Constructs a <see cref="TransliterationPosition"/> object with the given <paramref name="start"/>,
        /// <paramref name="contextStart"/>, and <paramref name="contextLimit"/>. The limit is set to the
        /// <paramref name="contextLimit"/>.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public TransliterationPosition(int contextStart, int contextLimit, int start)
            : this(contextStart, contextLimit, start, contextLimit)
        {
        }

        /// <summary>
        /// Constructs a <see cref="TransliterationPosition"/> object with the given <paramref name="start"/>, <paramref name="limit"/>,
        /// <paramref name="contextStart"/>, and <paramref name="contextLimit"/>.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public TransliterationPosition(int contextStart, int contextLimit,
                        int start, int limit)
        {
            this.ContextStart = contextStart;
            this.ContextLimit = contextLimit;
            this.Start = start;
            this.Limit = limit;
        }

        /// <summary>
        /// Constructs a <see cref="TransliterationPosition"/> object that is a copy of another.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public TransliterationPosition(TransliterationPosition pos)
        {
            Set(pos);
        }

        /// <summary>
        /// Copies the indices of this position from another.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public virtual void Set(TransliterationPosition pos)
        {
            ContextStart = pos.ContextStart;
            ContextLimit = pos.ContextLimit;
            Start = pos.Start;
            Limit = pos.Limit;
        }

        /// <summary>
        /// Returns true if this <see cref="TransliterationPosition"/> is equal to the given object.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public override bool Equals(object obj)
        {
            if (obj is TransliterationPosition pos)
            {
                return ContextStart == pos.ContextStart &&
                    ContextLimit == pos.ContextLimit &&
                    Start == pos.Start &&
                    Limit == pos.Limit;
            }
            return false;
        }

        /// <summary>
        /// Gets a hash code for the current <see cref="TransliterationPosition"/>.
        /// </summary>
        /// <internal/>
        //[Obsolete("This API is ICU internal only.")] // ICU4N: Not possible for GetHashCode() to be obsolete, since it is required by the framework
        public override int GetHashCode()
        {
            // ICU4N specific - implemented hash code
            return ContextStart ^ ContextLimit ^ Start ^ Limit;
        }

        /// <summary>
        /// Returns a string representation of this <see cref="TransliterationPosition"/>.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public override string ToString()
        {
            return "[cs=" + ContextStart
                + ", s=" + Start
                + ", l=" + Limit
                + ", cl=" + ContextLimit
                + "]";
        }

        /// <summary>
        /// Check all bounds.  If they are invalid, throw an exception.
        /// </summary>
        /// <param name="length">The length of the string this object applies to.</param>
        /// <exception cref="ArgumentException">If any indices are out of bounds.</exception>
        /// <stable>ICU 2.0</stable>
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


    /// <summary>
    /// <see cref="Transliterator"/> is an abstract class that transliterates text from one format to another. The most common
    /// kind of transliterator is a script, or alphabet, transliterator. For example, a Russian to Latin transliterator
    /// changes Russian text written in Cyrillic characters to phonetically equivalent Latin characters. It does not
    /// <em>translate</em> Russian to English! Transliteration, unlike translation, operates on characters, without reference
    /// to the meanings of words and sentences.
    /// <para/>
    /// Although script conversion is its most common use, a transliterator can actually perform a more general class of
    /// tasks. In fact, <see cref="Transliterator"/> defines a very general API which specifies only that a segment of the
    /// input text is replaced by new text. The particulars of this conversion are determined entirely by subclasses of
    /// <see cref="Transliterator"/>.
    /// </summary>
    /// <remarks>
    /// <b>Transliterators are stateless</b>
    /// 
    /// <para/>
    /// <see cref="Transliterator"/> objects are <em>stateless</em>; they retain no information between calls to
    /// <see cref="Transliterate(IReplaceable)"/>. As a result, threads may share transliterators without synchronizing them. This might
    /// seem to limit the complexity of the transliteration operation. In practice, subclasses perform complex
    /// transliterations by delaying the replacement of text until it is known that no other replacements are possible. In
    /// other words, although the <see cref="Transliterator"/> objects are stateless, the source text itself embodies all the
    /// needed information, and delayed operation allows arbitrary complexity.
    /// <para/>
    /// <b>Batch transliteration</b>
    /// 
    /// <para/>
    /// The simplest way to perform transliteration is all at once, on a string of existing text. This is referred to as
    /// <em>batch</em> transliteration. For example, given a string <c>input</c> and a transliterator <c>t</c>,
    /// the call
    /// <code>string result = t.Transliterate(input);</code>
    /// will transliterate it and return the result. Other methods allow the client to specify a substring to be
    /// transliterated and to use <see cref="IReplaceable"/> objects instead of strings, in order to preserve out-of-band
    /// information (such as text styles).
    /// <para/>
    /// 
    /// <b>Keyboard transliteration</b>
    /// 
    /// <para/>
    /// Somewhat more involved is <em>keyboard</em>, or incremental transliteration. This is the transliteration of text that
    /// is arriving from some source (typically the user's keyboard) one character at a time, or in some other piecemeal
    /// fashion.
    /// <para/>
    /// In keyboard transliteration, a <see cref="IReplaceable"/> buffer stores the text. As text is inserted, as much as
    /// possible is transliterated on the fly. This means a GUI that displays the contents of the buffer may show text being
    /// modified as each new character arrives.
    /// <para/>
    /// Consider the simple <see cref="RuleBasedTransliterator"/>:
    /// <code>
    /// th&gt;{theta}<br/>
    /// t&gt;{tau}
    /// </code>
    /// When the user types 't', nothing will happen, since the transliterator is waiting to see if the next character is
    /// 'h'. To remedy this, we introduce the notion of a cursor, marked by a '|' in the output string:
    /// <code>
    /// t&gt;|{tau}<br/>
    /// {tau}h&gt;{theta}
    /// </code>
    /// Now when the user types 't', tau appears, and if the next character is 'h', the tau changes to a theta. This is
    /// accomplished by maintaining a cursor position (independent of the insertion point, and invisible in the GUI) across
    /// calls to <see cref="Transliterate(IReplaceable)"/>. Typically, the cursor will be coincident with the insertion point, but in a
    /// case like the one above, it will precede the insertion point.
    /// <para/>
    /// Keyboard transliteration methods maintain a set of three indices that are updated with each call to
    /// <see cref="Transliterate(IReplaceable, TransliterationPosition)"/>, including the cursor, start, and limit. These indices are changed by the method, and
    /// they are passed in and out via a <see cref="TransliterationPosition"/> object. The <c>start</c> index marks the beginning of the substring
    /// that the transliterator will look at. It is advanced as text becomes committed (but it is not the committed index;
    /// that's the <c>cursor</c>). The <c>cursor</c> index, described above, marks the point at which the
    /// transliterator last stopped, either because it reached the end, or because it required more characters to
    /// disambiguate between possible inputs. The <c>cursor</c> can also be explicitly set by rules in a
    /// <see cref="RuleBasedTransliterator"/>. Any characters before the <c>cursor</c> index are frozen; future keyboard
    /// transliteration calls within this input sequence will not change them. New text is inserted at the <c>limit</c>
    /// index, which marks the end of the substring that the transliterator looks at.
    /// <para/>
    /// Because keyboard transliteration assumes that more characters are to arrive, it is conservative in its operation. It
    /// only transliterates when it can do so unambiguously. Otherwise it waits for more characters to arrive. When the
    /// client code knows that no more characters are forthcoming, perhaps because the user has performed some input
    /// termination operation, then it should call <see cref="FinishTransliteration(IReplaceable, TransliterationPosition)"/> to complete any pending
    /// transliterations.
    /// <para/>
    /// 
    /// <b>Inverses</b>
    /// 
    /// <para/>
    /// Pairs of transliterators may be inverses of one another. For example, if transliterator <b>A</b> transliterates
    /// characters by incrementing their Unicode value (so "abc" -&gt; "def"), and transliterator <b>B</b> decrements character
    /// values, then <b>A</b> is an inverse of <b>B</b> and vice versa. If we compose <b>A</b> with <b>B</b> in a compound
    /// transliterator, the result is the indentity transliterator, that is, a transliterator that does not change its input
    /// text.
    /// <para/>
    /// The <see cref="Transliterator.GetInverse()"/> method returns a transliterator's inverse, if one exists,
    /// or <c>null</c> otherwise. However, the result of <see cref="Transliterator.GetInverse()"/> usually will <em>not</em> be a true
    /// mathematical inverse. This is because true inverse transliterators are difficult to formulate. For example, consider
    /// two transliterators: <b>AB</b>, which transliterates the character 'A' to 'B', and <b>BA</b>, which transliterates
    /// 'B' to 'A'. It might seem that these are exact inverses, since
    /// <code>
    /// "A" x <b>AB</b> -&gt; "B"<br/>
    /// "B" x <b>BA</b> -&gt; "A"
    /// </code>
    /// where 'x' represents transliteration. However,
    /// <code>
    /// "ABCD" x <b>AB</b> -&gt; "BBCD"<br/>
    /// "BBCD" x <b>BA</b> -&gt; "AACD"
    /// </code>
    /// so <b>AB</b> composed with <b>BA</b> is not the identity. Nonetheless, <b>BA</b> may be usefully considered to be
    /// <b>AB</b>'s inverse, and it is on this basis that <b>AB</b><c>.GetInverse()</c> could legitimately return
    /// <b>BA</b>.
    /// <para/>
    /// 
    /// <b>Filtering</b>
    /// 
    /// <para/>
    /// Each transliterator has a filter, which restricts changes to those characters selected by the filter. The
    /// filter affects just the characters that are changed -- the characters outside of the filter are still part of the
    /// context for the filter. For example, in the following even though 'x' is filtered out, and doesn't convert to y, it does affect the conversion of 'a'.
    /// <code>
    /// string rules = &quot;x &gt; y; x{a} &gt; b; &quot;
    /// Transliterator tempTrans = Transliterator.CreateFromRules(&quot;temp&quot;, rules, TransliterationDirection.Forward)
    /// {
    ///     Filter = new UnicodeSet(&quot;[a]&quot;);
    /// };
    /// string tempResult = tempTrans.Transform(&quot;xa&quot;);
    /// // results in &quot;xb&quot;
    /// </code>
    /// <para/>
    /// 
    /// <b>IDs and display names</b>
    /// 
    /// <para/>
    /// A transliterator is designated by a short identifier string or <em>ID</em>. IDs follow the format
    /// <em>source-destination</em>, where <em>source</em> describes the entity being replaced, and <em>destination</em>
    /// describes the entity replacing <em>source</em>. The entities may be the names of scripts, particular sequences of
    /// characters, or whatever else it is that the transliterator converts to or from. For example, a transliterator from
    /// Russian to Latin might be named "Russian-Latin". A transliterator from keyboard escape sequences to Latin-1
    /// characters might be named "KeyboardEscape-Latin1". By convention, system entity names are in English, with the
    /// initial letters of words capitalized; user entity names may follow any format so long as they do not contain dashes.
    /// <para/>
    /// In addition to programmatic IDs, transliterator objects have display names for presentation in user interfaces,
    /// returned by <see cref="GetDisplayName(string)"/>.
    /// <para/>
    /// 
    /// <b>Factory methods and registration</b>
    /// 
    /// <para/>
    /// In general, client code should use the factory method <see cref="GetInstance(string)"/> to obtain an instance of a
    /// transliterator given its ID. Valid IDs may be enumerated using <see cref="GetAvailableIDs()"/>. Since transliterators
    /// are stateless, multiple calls to <see cref="GetInstance(string)"/> with the same ID will return the same object.
    /// <para/>
    /// In addition to the system transliterators registered at startup, user transliterators may be registered by calling
    /// <see cref="RegisterInstance(Transliterator)"/> at run time. To register a transliterator subclass without instantiating it (until it
    /// is needed), users may call <see cref="RegisterType(string, Type, string)"/>.
    /// <para/>
    /// 
    /// <b>Composed transliterators</b>
    /// 
    /// <para/>
    /// In addition to built-in system transliterators like "Latin-Greek", there are also built-in <em>composed</em>
    /// transliterators. These are implemented by composing two or more component transliterators. For example, if we have
    /// scripts "A", "B", "C", and "D", and we want to transliterate between all pairs of them, then we need to write 12
    /// transliterators: "A-B", "A-C", "A-D", "B-A",..., "D-A", "D-B", "D-C". If it is possible to convert all scripts to an
    /// intermediate script "M", then instead of writing 12 rule sets, we only need to write 8: "A~M", "B~M", "C~M", "D~M",
    /// "M~A", "M~B", "M~C", "M~D". (This might not seem like a big win, but it's really 2<em>n</em> vs. <em>n</em>
    /// <sup>2</sup> - <em>n</em>, so as <em>n</em> gets larger the gain becomes significant. With 9 scripts, it's 18 vs. 72
    /// rule sets, a big difference.) Note the use of "~" rather than "-" for the script separator here; this indicates that
    /// the given transliterator is intended to be composed with others, rather than be used as is.
    /// <para/>
    /// Composed transliterators can be instantiated as usual. For example, the system transliterator "Devanagari-Gujarati"
    /// is a composed transliterator built internally as "Devanagari~InterIndic;InterIndic~Gujarati". When this
    /// transliterator is instantiated, it appears externally to be a standard transliterator (e.g., <see cref="ID"/> returns
    /// "Devanagari-Gujarati").
    /// <para/>
    /// 
    /// <b>Subclassing</b>
    /// 
    /// <para/>
    /// Subclasses must implement the abstract method <see cref="HandleTransliterate(IReplaceable, TransliterationPosition, bool)"/>.
    /// </remarks>
    /// <author>Alan Liu</author>
    /// <stable>ICU 2.0</stable>
    public abstract class Transliterator : IStringTransform
    {
        internal const int CharStackBufferSize = 64;

        /// <summary>
        /// Direction constant indicating the forward direction in a transliterator,
        /// e.g., the forward rules of a RuleBasedTransliterator.  An "A-B"
        /// transliterator transliterates A to B when operating in the forward
        /// direction, and B to A when operating in the reverse direction.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public static readonly TransliterationDirection Forward = TransliterationDirection.Forward; 

        /// <summary>
        /// Direction constant indicating the forward direction in a transliterator,
        /// e.g., the forward rules of a RuleBasedTransliterator.  An "A-B"
        /// transliterator transliterates A to B when operating in the forward
        /// direction, and B to A when operating in the reverse direction.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public static readonly TransliterationDirection Reverse = TransliterationDirection.Reverse;

        // ICU4N specific - de-nested the Postion class and renamed it TransliteratorPosition

        /// <summary>
        /// Programmatic name, e.g., "Latin-Arabic".
        /// </summary>
        private string id;

        /// <summary>
        /// This transliterator's filter.  Any character for which
        /// <c>filter.Contains()</c> returns <c>false</c> will not be
        /// altered by this transliterator.  If <see cref="filter"/> is
        /// <c>null</c> then no filtering is applied.
        /// </summary>
        private UnicodeSet filter;

        private int maximumContextLength = 0;

        /// <summary>
        /// System transliterator registry.
        /// </summary>
        private static TransliteratorRegistry registry;

        private static IDictionary<CaseInsensitiveString, string> displayNameCache;

        /// <summary>
        /// Prefix for resource bundle key for the display name for a
        /// transliterator.  The ID is appended to this to form the key.
        /// The resource bundle value should be a <see cref="string"/>.
        /// </summary>
        private const string RB_DISPLAY_NAME_PREFIX = "%Translit%%";

        /// <summary>
        /// Prefix for resource bundle key for the display name for a
        /// transliterator SCRIPT.  The ID is appended to this to form the key.
        /// The resource bundle value should be a <see cref="string"/>.
        /// </summary>
        private const string RB_SCRIPT_DISPLAY_NAME_PREFIX = "%Translit%";

        /// <summary>
        /// Resource bundle key for display name pattern.
        /// The resource bundle value should be a <see cref="string"/> forming a
        /// <see cref="MessageFormat"/>, e.g.:
        /// "{0,choice,0#|1#{1} Transliterator|2#{1} to {2} Transliterator}".
        /// </summary>
        private const string RB_DISPLAY_NAME_PATTERN = "TransliteratorNamePattern";

        /// <summary>
        /// Delimiter between elements in a compound ID.
        /// </summary>
        internal const char ID_DELIM = ';';

        /// <summary>
        /// Delimiter before target in an ID.
        /// </summary>
        internal const char ID_SEP = '-';

        /// <summary>
        /// Delimiter before variant in an ID.
        /// </summary>
        internal const char VARIANT_SEP = '/';

        /// <summary>
        /// To enable debugging output in the Transliterator component, set
        /// <see cref="DEBUG"/> to true.
        /// <para/>
        /// N.B. Make sure to recompile all of the ICU4N assembly
        /// after changing this.
        /// <para/>
        /// <strong>This generates a lot of output.</strong>
        /// </summary>
        internal static readonly bool DEBUG = false;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="id">The string identifier for this transliterator.</param>
        /// <param name="filter">The filter.  Any character for which
        /// <c>filter.Contains()</c> returns <c>false</c> will not be
        /// altered by this transliterator.  If <paramref name="filter"/> is
        /// <c>null</c> then no filtering is applied.</param>
        /// <stable>ICU 2.0</stable>
        protected Transliterator(string id, UnicodeFilter filter)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(Text.Transliterator.id));
            }
            this.id = id;
            Filter = filter;
        }

        /// <summary>
        /// Transliterates a segment of a string, with optional filtering.
        /// </summary>
        /// <param name="text">The string to be transliterated.</param>
        /// <param name="start">the beginning index, inclusive; <c>0 &lt;= start
        /// &lt;= limit</c>.</param>
        /// <param name="limit">The ending index, exclusive; <c>start &lt;= limit
        /// &lt;= text.length()</c>.</param>
        /// <returns>The new limit index.  The text previously occupying <c>[start,
        /// limit)</c> has been transliterated, possibly to a string of a different
        /// length, at <c>[start, </c><em>new-limit</em><c>)</c>, where
        /// <em>new-limit</em> is the return value. If the input offsets are out of bounds,
        /// the returned value is -1 and the input string remains unchanged.</returns>
        /// <stable>ICU 2.0</stable>
        public int Transliterate(IReplaceable text, int start, int limit)
        {
            if (start < 0 ||
                limit < start ||
                text.Length < limit)
            {
                return -1;
            }

            TransliterationPosition pos = new TransliterationPosition(start, limit, start);
            FilteredTransliterate(text, pos, false, true);
            return pos.Limit;
        }

        /// <summary>
        /// Transliterates an entire string in place. Convenience method.
        /// </summary>
        /// <param name="text">The string to be transliterated.</param>
        /// <stable>ICU 2.0</stable>
        public void Transliterate(IReplaceable text)
        {
            Transliterate(text, 0, text.Length);
        }

        /// <summary>
        /// Transliterate an entire string and returns the result. Convenience method.
        /// </summary>
        /// <param name="text">The string to be transliterated.</param>
        /// <returns>The transliterated text.</returns>
        /// <stable>ICU 2.0</stable>
        public string Transliterate(string text)
        {
            ReplaceableString result = new ReplaceableString(text);
            Transliterate(result);
            return result.ToString();
        }

        /// <summary>
        /// Transliterates the portion of the text buffer that can be
        /// transliterated unambiguosly after new text has been inserted,
        /// typically as a result of a keyboard event.  The new text in
        /// <paramref name="insertion"/> will be inserted into <paramref name="text"/>
        /// at <c>text.ContextLimit</c>, advancing
        /// <c>index.ContextLimit</c> by <c>insertion.Length</c>.
        /// Then the transliterator will try to transliterate characters of
        /// <paramref name="text"/> between <c>index.Start</c> and
        /// <c>index.ContextLimit</c>.  Characters before
        /// <c>index.Start</c> will not be changed.
        /// <para/>
        /// Upon return, values in <paramref name="index"/> will be updated.
        /// <c>index.ContextStart</c> will be advanced to the first
        /// character that future calls to this method will read.
        /// <c>index.Start</c> and <c>index.ContextLimit</c> will
        /// be adjusted to delimit the range of text that future calls to
        /// this method may change.
        /// <para/>
        /// Typical usage of this method begins with an initial call
        /// with <c>index.ContextStart</c> and <c>index.ContextLimit</c>
        /// set to indicate the portion of <paramref name="text"/> to be
        /// transliterated, and <c>index.Start == index.ContextStart</c>.
        /// Thereafter, <paramref name="index"/> can be used without
        /// modification in future calls, provided that all changes to
        /// <paramref name="text"/> are made via this method.
        /// <para/>
        /// This method assumes that future calls may be made that will
        /// insert new text into the buffer.  As a result, it only performs
        /// unambiguous transliterations.  After the last call to this
        /// method, there may be untransliterated text that is waiting for
        /// more input to resolve an ambiguity.  In order to perform these
        /// pending transliterations, clients should call <see cref="FinishTransliteration(IReplaceable, TransliterationPosition)"/>
        /// after the last call to this
        /// method has been made.
        /// </summary>
        /// <param name="text">The buffer holding transliterated and untransliterated text.</param>
        /// <param name="index">The start and limit of the text, the position
        /// of the cursor, and the start and limit of transliteration.</param>
        /// <param name="insertion">Text to be inserted and possibly
        /// transliterated into the translation buffer at
        /// <c>index.ContextLimit</c>.  If <c>null</c> then no text
        /// is inserted.</param>
        /// <seealso cref="HandleTransliterate(IReplaceable, TransliterationPosition, bool)"/>
        /// <exception cref="ArgumentException">If <paramref name="index"/>
        /// is invalid.</exception>
        /// <stable>ICU 2.0</stable>
        public void Transliterate(IReplaceable text, TransliterationPosition index,
                                        string insertion)
        {
            index.Validate(text.Length);

            //        int originalStart = index.contextStart;
            if (insertion != null)
            {
                text.Replace(index.Limit, index.Limit - index.Limit, insertion); // ICU4N: Corrected 2nd parameter
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

        /// <summary>
        /// Transliterates the portion of the text buffer that can be
        /// transliterated unambiguosly after a new character has been
        /// inserted, typically as a result of a keyboard event.  This is a
        /// convenience method; see <see cref="Transliterate(IReplaceable, TransliterationPosition, string)"/> for details.
        /// </summary>
        /// <param name="text">The buffer holding transliterated and
        /// untransliterated text.</param>
        /// <param name="index">The start and limit of the text, the position
        /// of the cursor, and the start and limit of transliteration.</param>
        /// <param name="insertion">Text to be inserted and possibly
        /// transliterated into the translation buffer at
        /// <c>index.ContextLimit</c>.</param>
        /// <seealso cref="Transliterate(IReplaceable, TransliterationPosition, string)"/>
        /// <stable>ICU 2.0</stable>
        public void Transliterate(IReplaceable text, TransliterationPosition index,
                                        int insertion)
        {
            Transliterate(text, index, UTF16.ValueOf(insertion));
        }

        /// <summary>
        /// Transliterates the portion of the text buffer that can be
        /// transliterated unambiguosly.  This is a convenience method; see
        /// <see cref="Transliterate(IReplaceable, TransliterationPosition, string)"/>
        /// for details.
        /// </summary>
        /// <param name="text">The buffer holding transliterated and
        /// untransliterated text.</param>
        /// <param name="index">The start and limit of the text, the position
        /// of the cursor, and the start and limit of transliteration.</param>
        /// <seealso cref="Transliterate(IReplaceable, TransliterationPosition, string)"/>
        /// <stable>ICU 2.0</stable>
        public void Transliterate(IReplaceable text, TransliterationPosition index)
        {
            Transliterate(text, index, null);
        }

        /// <summary>
        /// Finishes any pending transliterations that were waiting for
        /// more characters.  Clients should call this method as the last
        /// call after a sequence of one or more calls to
        /// <see cref="Transliterate(IReplaceable, TransliterationPosition)"/>.
        /// </summary>
        /// <param name="text">The buffer holding transliterated and
        /// untransliterated text.</param>
        /// <param name="index">The array of indices previously passed to 
        /// <see cref="Transliterate(IReplaceable, TransliterationPosition)"/>.</param>
        /// <stable>ICU 2.0</stable>
        public void FinishTransliteration(IReplaceable text,
                                                TransliterationPosition index)
        {
            index.Validate(text.Length);
            FilteredTransliterate(text, index, false, true);
        }

        /// <summary>
        /// Abstract method that concrete subclasses define to implement
        /// their transliteration algorithm.  This method handles both
        /// incremental and non-incremental transliteration.  Let
        /// <c>originalStart</c> refer to the value of
        /// <c>pos.Start</c> upon entry.
        /// <para/>
        /// <list type="bullet">
        ///     <item>
        ///         <description>
        ///             If <paramref name="incremental"/> is false, then this method
        ///             should transliterate all characters between
        ///             <c>pos.Start</c> and <code>pos.Limit</code>. Upon return
        ///             <code>pos.Start</code> must == <c>pos.Limit</c>.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             If <paramref name="incremental"/> is true, then this method
        ///             should transliterate all characters between
        ///             <c>pos.Start</c> and <c>pos.Limit</c> that can be
        ///             unambiguously transliterated, regardless of future insertions
        ///             of text at <c>pos.Limit</c>.  Upon return,
        ///             <c>pos.Start</c> should be in the range
        ///             [<c>originalStart</c>, <c>pos.Limit</c>).
        ///             <c>pos.Start</c> should be positioned such that
        ///             characters [<c>originalStart</c>, <c>
        ///             pos.Start</c>) will not be changed in the future by this
        ///             transliterator and characters [<c>pos.start</c>,
        ///             <c>pos.limit</c>) are unchanged.
        ///         </description>
        ///     </item>
        /// </list>
        /// <para/>
        /// Implementations of this method should also obey the
        /// following invariants:
        /// <list type="bullet">
        ///     <item>
        ///         <description>
        ///             <c>pos.Limit</c> and <c>pos.ContextLimit</c>
        ///             should be updated to reflect changes in length of the text
        ///             between <c>pos.Start</c> and <c>pos.Limit</c>. The
        ///             difference <c>pos.ContextLimit - pos.Limit</c> should
        ///             not change.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             <c>pos.ContextStart</c> should not change.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             Upon return, neither <c>pos.Start</c> nor
        ///             <c>pos.Limit</c> should be less than
        ///             <c>originalStart</c>.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             Text before <c>originalStart</c> and text after
        ///             <c>pos.Limit</c> should not change.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <description>
        ///             Text before <code>pos.contextStart</code> and text after
        ///             <c>pos.contextLimit</c> should be ignored.
        ///         </description>
        ///     </item>
        /// </list>
        /// <para/>
        /// Subclasses may safely assume that all characters in
        /// [<c>pos.Start</c>, <c>pos.Limit</c>) are filtered.
        /// In other words, the filter has already been applied by the time
        /// this method is called.  See <see cref="FilteredTransliterate(IReplaceable, TransliterationPosition, bool, bool)"/>.
        /// <para/>
        /// This method is <b>not</b> for public consumption.  Calling
        /// this method directly will transliterate
        /// [<c>pos.Start</c>, <c>pos.Limit</c>) without
        /// applying the filter. End user code should call 
        /// <see cref="Transliterate(IReplaceable, TransliterationPosition)"/>
        /// instead of this method. Subclass code
        /// should call <see cref="FilteredTransliterate(IReplaceable, TransliterationPosition, bool, bool)"/> instead of
        /// this method.
        /// </summary>
        /// <param name="text">The buffer holding transliterated and
        /// untransliterated text.</param>
        /// <param name="pos">The indices indicating the start, limit, context
        /// start, and context limit of the text.</param>
        /// <param name="incremental">if true, assume more text may be inserted at
        /// <c>pos.Limit</c> and act accordingly.  Otherwise,
        /// transliterate all text between <c>pos.Start</c> and
        /// <c>pos.Limit</c> and move <c>pos.Start</c> up to
        /// <c>pos.Limit</c>.</param>
        /// <seealso cref="Transliterate(IReplaceable)"/>
        /// <seealso cref="Transliterate(IReplaceable, int, int)"/>
        /// <seealso cref="Transliterate(IReplaceable, TransliterationPosition)"/>
        /// <seealso cref="Transliterate(IReplaceable, TransliterationPosition, int)"/>
        /// <seealso cref="Transliterate(IReplaceable, TransliterationPosition, string)"/>
        /// <stable>ICU 2.0</stable>
        protected abstract void HandleTransliterate(IReplaceable text,
                                                    TransliterationPosition pos, bool incremental);

        /// <summary>
        /// Top-level transliteration method, handling filtering, incremental and
        /// non-incremental transliteration, and rollback.  All transliteration
        /// public API methods eventually call this method with a rollback argument
        /// of <c>true</c>.  Other entities may call this method but rollback should be
        /// <c>false</c>.
        /// </summary>
        /// <remarks>
        /// If this transliterator has a filter, break up the input text into runs
        /// of unfiltered characters.  Pass each run to
        /// <c>[subclass].HandleTransliterate().</c>.
        /// <para/>
        /// In <paramref name="incremental"/> mode, if rollback is <c>true</c>, perform a special
        /// incremental procedure in which several passes are made over the input
        /// text, adding one character at a time, and committing successful
        /// transliterations as they occur.  Unsuccessful transliterations are rolled
        /// back and retried with additional characters to give correct results.
        /// </remarks>
        /// <param name="text">The text to be transliterated.</param>
        /// <param name="index">The position indices.</param>
        /// <param name="incremental">If <c>true</c>, then assume more characters may be inserted
        /// at <c>index.Limit</c>, and postpone processing to accomodate future incoming
        /// characters.</param>
        /// <param name="rollback">If <c>true</c> and if <paramref name="incremental"/> is <c>true</c>, then perform special
        /// incremental processing, as described above, and undo partial
        /// transliterations where necessary.  If <paramref name="incremental"/> is <c>false</c> then this
        /// parameter is ignored.</param>
        private void FilteredTransliterate(IReplaceable text,
                                           TransliterationPosition index,
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
                    text.Copy(runStart, runLength, rollbackOrigin); // ICU4N: Corrected 2nd parameter

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
                            text.Replace(passStart, index.Limit - passStart, ""); // ICU4N: Corrected 2nd parameter

                            // Copy the rollback text back
                            text.Copy(rs, uncommittedLength, passStart); // ICU4N: Corrected 2nd parameter

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
                    text.Replace(rollbackOrigin, runLength, ""); // ICU4N: Corrected 2nd parameter

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

        /// <summary>
        /// Transliterate a substring of text, as specified by index, taking filters
        /// into account.  This method is for subclasses that need to delegate to
        /// another transliterator, such as <see cref="CompoundTransliterator"/>.
        /// </summary>
        /// <param name="text">The text to be transliterated.</param>
        /// <param name="index">The position indices.</param>
        /// <param name="incremental">If <c>true</c>, then assume more characters may be inserted
        /// at index.Limit, and postpone processing to accomodate future incoming
        /// characters.</param>
        /// <stable>ICU 2.0</stable>
        public virtual void FilteredTransliterate(IReplaceable text,
                                             TransliterationPosition index,
                                             bool incremental)
        {
            FilteredTransliterate(text, index, incremental, false);
        }

        /// <summary>
        /// Gets or sets the length of the longest context required by this transliterator.
        /// This is <em>preceding</em> context.  The default value is zero, but
        /// subclasses can set this property.
        /// For example, if a transliterator translates "ddd" (where
        /// d is any digit) to "555" when preceded by "(ddd)", then the preceding
        /// context length is 5, the length of "(ddd)".
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public int MaximumContextLength
        {
            get => maximumContextLength;
            protected set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Invalid context length " + value);
                }
                maximumContextLength = value;
            }
        }

        /// <summary>
        /// Gets or sets a programmatic identifier for this transliterator.
        /// If this identifier is passed to <see cref="GetInstance(string)"/>, it
        /// will return this object, if it has been registered.
        /// The setter is only for use by subclasses.
        /// </summary>
        /// <see cref="RegisterType(string, Type, string)"/>
        /// <see cref="GetAvailableIDs()"/>
        /// <stable>ICU 2.0</stable>
        public string ID
        {
            get => id;
            protected internal set => this.id = value;
        }

        /// <summary>
        /// Returns a name for this transliterator that is appropriate for
        /// display to the user in the default <see cref="UCultureInfo.CurrentUICulture"/>.
        /// See <see cref="GetDisplayName(string, CultureInfo)"/> for details.
        /// </summary>
        /// <seealso cref="UCultureInfo.CurrentUICulture"/>
        /// <stable>ICU 2.0</stable>
        public static string GetDisplayName(string ID)
        {
            return GetDisplayName(ID, UCultureInfo.CurrentUICulture);
        }

        /// <summary>
        /// Returns a name for this transliterator that is appropriate for
        /// display to the user in the given locale.  This name is taken
        /// from the locale resource data in the standard manner of the
        /// <c>java.text</c> package.
        /// <para/>
        /// If no localized names exist in the system resource bundles,
        /// a name is synthesized using a localized
        /// <see cref="MessageFormat"/> pattern from the resource data.  The
        /// arguments to this pattern are an integer followed by one or two
        /// strings.  The integer is the number of strings, either 1 or 2.
        /// The strings are formed by splitting the ID for this
        /// transliterator at the first '-'.  If there is no '-', then the
        /// entire ID forms the only string.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="inLocale">The <see cref="CultureInfo"/> in which the display name should be
        /// localized.</param>
        /// <seealso cref="MessageFormat"/>
        /// <stable>ICU 2.0</stable>
        public static string GetDisplayName(string id, CultureInfo inLocale)
        {
            return GetDisplayName(id, inLocale.ToUCultureInfo());
        }

        /// <summary>
        /// Returns a name for this transliterator that is appropriate for
        /// display to the user in the given locale.  This name is taken
        /// from the locale resource data in the standard manner of the
        /// <c>java.text</c> package.
        /// <para/>
        /// If no localized names exist in the system resource bundles,
        /// a name is synthesized using a localized
        /// <see cref="MessageFormat"/> pattern from the resource data.  The
        /// arguments to this pattern are an integer followed by one or two
        /// strings.  The integer is the number of strings, either 1 or 2.
        /// The strings are formed by splitting the ID for this
        /// transliterator at the first '-'.  If there is no '-', then the
        /// entire ID forms the only string.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="inLocale">The <see cref="UCultureInfo"/> in which the display name should be
        /// localized.</param>
        /// <seealso cref="MessageFormat"/>
        /// <stable>ICU 3.2</stable>
        public static string GetDisplayName(string id, UCultureInfo inLocale)
        {

            // Resource bundle containing display name keys and the
            // RB_RULE_BASED_IDS array.
            //
            //If we ever integrate this with the Sun JDK, the resource bundle
            // root will change to sun.text.resources.LocaleElements

            ICUResourceBundle bundle = (ICUResourceBundle)UResourceBundle.
                // ICU4N specific - we need to pass the current assembly to load the resource data
                GetBundleInstance(ICUData.IcuTransliteratorBaseName, inLocale, ICUResourceBundle.IcuDataAssembly);

            // Normalize the ID
            TransliteratorIDParser.IDtoSTV(id, out string source, out string target, out string variant, out bool _);
            // ICU4N specific - since we are not returning an array, it cannot be null (and it was never null anyway)

            // ICU4N specific - use Concat
            string ID;
            if (!string.IsNullOrEmpty(variant))
            {
                ID = string.Concat(source, "-", target, "/", variant);
            }
            else
            {
                ID = string.Concat(source, "-", target);
            }

            // Use the registered display name, if any
            if (displayNameCache.TryGetValue(new CaseInsensitiveString(ID), out string n) && n != null)
            {
                return n;
            }

            // Use display name for the entire transliterator, if it
            // exists.
            try
            {
                return bundle.GetString(RB_DISPLAY_NAME_PREFIX + ID);
            }
            catch (MissingManifestResourceException) { }

            try
            {
                // Construct the formatter first; if getString() fails
                // we'll exit the try block
                MessageFormat format = new ICU4N.Text.MessageFormat( // ICU4N specific - we are using ICU's MessageFormat, original code was using JDK
                        bundle.GetString(RB_DISPLAY_NAME_PATTERN));

                // Construct the argument array
                object[] args = new object[] { 2, source, target };

                // Use display names for the scripts, if they exist
                for (int j = 1; j <= 2; ++j)
                {
                    try
                    {
                        args[j] = bundle.GetString(RB_SCRIPT_DISPLAY_NAME_PREFIX +
                                                   (string)args[j]);
                    }
                    catch (MissingManifestResourceException) { }
                }

                // Format it using the pattern in the resource
                return (variant.Length > 0) ?
                    (format.Format(args) + '/' + variant) :
                    format.Format(args);
            }
            catch (MissingManifestResourceException) { }

            // We should not reach this point unless there is something
            // wrong with the build or the RB_DISPLAY_NAME_PATTERN has
            // been deleted from the root RB_LOCALE_ELEMENTS resource.
            throw new Exception();
        }

        /// <summary>
        /// Gets or sets the filter used by this transliterator.  If the filter
        /// is set to <c>null</c> then no filtering will occur.
        /// <para/>
        /// Callers must take care if a transliterator is in use by
        /// multiple threads.  The filter should not be changed by one
        /// thread while another thread may be transliterating.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public UnicodeFilter Filter
        {
            get => filter;
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

        /// <summary>
        /// Returns a <see cref="Transliterator"/> object given its <paramref name="id"/>.
        /// The <paramref name="id"/> must be either a system transliterator ID or a ID registered
        /// using <see cref="RegisterType(string, Type, string)"/>.
        /// </summary>
        /// <param name="id">A valid <paramref name="id"/>, as enumerated by <see cref="GetAvailableIDs()"/>.</param>
        /// <returns>A <see cref="Transliterator"/> object with the given <paramref name="id"/>.</returns>
        /// <exception cref="ArgumentException">If the given <paramref name="id"/> is invalid.</exception>
        /// <stable>ICU 2.0</stable>
        public static Transliterator GetInstance(string id)
        {
            return GetInstance(id, Forward);
        }

        /// <summary>
        /// Returns a <see cref="Transliterator"/> object given its <paramref name="id"/>.
        /// The <paramref name="id"/> must be either a system transliterator ID or a ID registered
        /// using <see cref="RegisterType(string, Type, string)"/>.
        /// </summary>
        /// <param name="id">A valid ID, as enumerated by <see cref="GetAvailableIDs()"/>.</param>
        /// <param name="dir">Either <see cref="TransliterationDirection.Forward"/> or 
        /// <see cref="TransliterationDirection.Reverse"/>. If <see cref="TransliterationDirection.Reverse"/> then the
        /// inverse of the given <paramref name="id"/> is instantiated.</param>
        /// <returns>A <see cref="Transliterator"/> object with the given <paramref name="id"/>.</returns>
        /// <seealso cref="RegisterType(string, Type, string)"/>
        /// <seealso cref="GetAvailableIDs()"/>
        /// <seealso cref="ID"/>
        /// <stable>ICU 2.0</stable>
        public static Transliterator GetInstance(string id,
                                                 TransliterationDirection dir)
        {
            StringBuffer canonID = new StringBuffer();
            IList<SingleID> list = new List<SingleID>();
            if (!TransliteratorIDParser.ParseCompoundID(id, dir, canonID, list, out UnicodeSet globalFilter)) // ICU4N: Changed globalFilter from UnicodeSet[] to out UnicodeSet
            {
                throw new ArgumentException("Invalid ID " + id);
            }

            IList<Transliterator> translits = TransliteratorIDParser.InstantiateList(list);

            // assert(list.size() > 0);
            Transliterator t = null;
            if (list.Count > 1 || canonID.IndexOf(";", StringComparison.Ordinal) >= 0)
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
            if (globalFilter != null)
            {
                t.Filter = globalFilter;
            }
            return t;
        }

        /// <summary>
        /// Create a transliterator from a basic ID.  This is an ID
        /// containing only the forward direction source, target, and
        /// variant.
        /// </summary>
        /// <param name="id">A basic ID of the form S-T or S-T/V.</param>
        /// <param name="canonID">Canonical ID to apply to the result, or
        /// null to leave the ID unchanged.</param>
        /// <returns>A newly created Transliterator or null if the ID is
        /// invalid.</returns>
        internal static Transliterator GetBasicInstance(string id, string canonID)
        {
            Transliterator t = registry.Get(id, out string s);
            if (s.Length != 0)
            {
                // assert(t==0);
                // Instantiate an alias
                t = GetInstance(s, Forward);
            }
            if (t != null && canonID != null)
            {
                t.ID = canonID;
            }
            return t;
        }

        /// <summary>
        /// Returns a <see cref="Transliterator"/> object constructed from
        /// the given rule string.  This will be a <see cref="RuleBasedTransliterator"/>,
        /// if the rule string contains only rules, or a
        /// <see cref="CompoundTransliterator"/>, if it contains ID blocks, or a
        /// <see cref="NullTransliterator"/>, if it contains ID blocks which parse as
        /// empty for the given direction.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public static Transliterator CreateFromRules(string id, string rules, TransliterationDirection dir)
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
#pragma warning disable 612, 618
                t = new RuleBasedTransliterator(id, parser.DataVector[0], parser.CompoundFilter);
#pragma warning restore 612, 618
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
#pragma warning disable 612, 618
                        transliterators.Add(new RuleBasedTransliterator("%Pass" + passNumber++, data, null));
#pragma warning restore 612, 618
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

        /// <summary>
        /// Returns a rule string for this transliterator.
        /// </summary>
        /// <param name="escapeUnprintable">If true, then unprintable characters
        /// will be converted to escape form backslash-'u' or
        /// backslash-'U'.</param>
        /// <stable>ICU 2.0</stable>
        public virtual string ToRules(bool escapeUnprintable)
        {
            return BaseToRules(escapeUnprintable);
        }

        /// <summary>
        /// Returns a rule string for this transliterator.  This is
        /// a non-overrideable base class implementation that subclasses
        /// may call.  It simply munges the ID into the correct format,
        /// that is, "foo" =&gt; "::foo".
        /// </summary>
        /// <param name="escapeUnprintable">If true, then unprintable characters
        /// will be converted to escape form backslash-'u' or
        /// backslash-'U'.</param>
        /// <stable>ICU 2.0</stable>
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

        /// <summary>
        /// Return the elements that make up this transliterator.  For
        /// example, if the transliterator "NFD;Jamo-Latin;Latin-Greek"
        /// were created, the return value of this method would be an array
        /// of the three transliterator objects that make up that
        /// transliterator: [NFD, Jamo-Latin, Latin-Greek].
        /// <para/>
        /// If this transliterator is not composed of other
        /// transliterators, then this method will return an array of
        /// length one containing a reference to this transliterator.
        /// </summary>
        /// <returns>An array of one or more transliterators that make up
        /// this transliterator.</returns>
        /// <stable>ICU 3.0</stable>
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

        /// <summary>
        /// Returns the set of all characters that may be modified in the
        /// input text by this <see cref="Transliterator"/>.  This incorporates this
        /// object's current filter; if the filter is changed, the return
        /// value of this function will change.  The default implementation
        /// returns an empty set.  Some subclasses may override <see cref="HandleGetSourceSet()"/>
        /// to return a more precise result.  The
        /// return result is approximate in any case and is intended for
        /// use by tests, tools, or utilities.
        /// </summary>
        /// <seealso cref="GetTargetSet()"/>
        /// <seealso cref="HandleGetSourceSet()"/>
        /// <stable>ICU 2.2</stable>
        public UnicodeSet GetSourceSet()
        {
            UnicodeSet result = new UnicodeSet();
#pragma warning disable 612, 618
            AddSourceTargetSet(GetFilterAsUnicodeSet(UnicodeSet.AllCodePoints), result, new UnicodeSet());
#pragma warning restore 612, 618
            return result;
        }

        /// <summary>
        /// Framework method that returns the set of all characters that
        /// may be modified in the input text by this <see cref="Transliterator"/>,
        /// ignoring the effect of this object's filter.  The base class
        /// implementation returns the empty set.  Subclasses that wish to
        /// implement this should override this method.
        /// </summary>
        /// <returns>The set of characters that this transliterator may
        /// modify.  The set may be modified, so subclasses should return a
        /// newly-created object.</returns>
        /// <see cref="GetSourceSet()"/>
        /// <see cref="GetTargetSet()"/>
        /// <stable>ICU 2.2</stable>
        protected virtual UnicodeSet HandleGetSourceSet()
        {
            return new UnicodeSet();
        }

        /// <summary>
        /// Returns the set of all characters that may be generated as
        /// replacement text by this transliterator.  The default
        /// implementation returns the empty set.  Some subclasses may
        /// override this method to return a more precise result.  The
        /// return result is approximate in any case and is intended for
        /// use by tests, tools, or utilities requiring such
        /// meta-information.
        /// </summary>
        /// <remarks>
        /// Warning. You might expect an empty filter to always produce an empty target.
        /// However, consider the following:
        /// <code>
        /// [Pp]{}[\u03A3\u03C2\u03C3\u03F7\u03F8\u03FA\u03FB] &gt; \';
        /// </code>
        /// With a filter of [], you still get some elements in the target set, because this rule will still match. It could
        /// be recast to the following if it were important.
        /// <code>
        /// [Pp]{([\u03A3\u03C2\u03C3\u03F7\u03F8\u03FA\u03FB])} &gt; \' | $1;
        /// </code>
        /// </remarks>
        /// <see cref="GetSourceSet()"/>
        /// <stable>ICU 2.2</stable>
        public virtual UnicodeSet GetTargetSet()
        {
            UnicodeSet result = new UnicodeSet();
#pragma warning disable 612, 618
            AddSourceTargetSet(GetFilterAsUnicodeSet(UnicodeSet.AllCodePoints), new UnicodeSet(), result);
#pragma warning restore 612, 618
            return result;
        }

        /// <summary>
        /// Returns the set of all characters that may be generated as
        /// replacement text by this transliterator, filtered by BOTH the input filter, and the current <see cref="Filter"/>.
        /// <para/>
        /// SHOULD BE OVERRIDEN BY SUBCLASSES.
        /// </summary>
        /// <remarks>
        /// It is probably an error for any transliterator to NOT override this, but we can't force them to
        /// for backwards compatibility.
        /// <para/>
        /// Other methods vector through this.
        /// <para/>
        /// When gathering the information on source and target, the compound transliterator makes things complicated.
        /// For example, suppose we have:
        /// <code>
        /// Global FILTER = [ax]
        /// a &gt; b;
        /// :: NULL;
        /// b &gt; c;
        /// x &gt; d;
        /// </code>
        /// While the filter just allows a and x, b is an intermediate result, which could produce c. So the source and target sets
        /// cannot be gathered independently. What we have to do is filter the sources for the first transliterator according to
        /// the global filter, intersect that transliterator's filter. Based on that we get the target.
        /// The next transliterator gets as a global filter (global + last target). And so on.
        /// <para/>
        /// There is another complication:
        /// <code>
        /// Global FILTER = [ax]
        /// a &gt;|b;
        /// b &gt;c;
        /// </code>
        /// Even though b would be filtered from the input, whenever we have a backup, it could be part of the input. So ideally we will
        /// change the global filter as we go.
        /// </remarks>
        /// <param name="inputFilter"></param>
        /// <param name="sourceSet"></param>
        /// <param name="targetSet">TODO</param>
        /// <seealso cref="GetTargetSet()"/>
        /// <internal/>
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

        /// <summary>
        /// Returns the intersection of this instance's filter intersected with an external filter.
        /// The <paramref name="externalFilter"/> must be frozen (it is frozen if not).
        /// The result may be frozen, so don't attempt to modify.
        /// </summary>
        /// <internal/>
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

        /// <summary>
        /// Returns this transliterator's inverse.  See the <see cref="Transliterator"/>
        /// documentation for details.  This implementation simply inverts
        /// the two entities in the <see cref="ID"/> and attempts to retrieve the
        /// resulting transliterator.  That is, if <see cref="ID"/>
        /// returns "A-B", then this method will return the result of
        /// <see cref="GetInstance(string)"/> with <c>"B-A"</c> as the parameter,
        /// or <c>null</c> if that call fails.
        /// <para/>
        /// Subclasses with knowledge of their inverse may wish to
        /// override this method.
        /// </summary>
        /// <returns>A transliterator that is an inverse, not necessarily
        /// exact, of this transliterator, or <c>null</c> if no such
        /// transliterator is registered.</returns>
        /// <seealso cref="RegisterType(string, Type, string)"/>
        /// <stable>ICU 2.0</stable>
        public Transliterator GetInverse()
        {
            return GetInstance(id, Reverse);
        }

        /// <summary>
        /// Registers a subclass of <code>Transliterator</code> with the
        /// system.  This subclass must have a public constructor taking no
        /// arguments.  When that constructor is called, the resulting
        /// object must return the <paramref name="id"/> passed to this method if
        /// its <see cref="ID"/> property is called.
        /// </summary>
        /// <param name="id">The result of <see cref="ID"/> for this transliterator.</param>
        /// <param name="transClass">A subclass of <see cref="Transliterator"/>.</param>
        /// <param name="displayName"></param>
        /// <seealso cref="Unregister(string)"/>
        /// <stable>ICU 2.0</stable>
        public static void RegisterType(string id, Type transClass, string displayName) // ICU4N specific - Renamed from RegisterClass to RegisterType
        {
            registry.Put(id, transClass, true); // ICU4N TODO: Indexer ?
            if (displayName != null)
            {
                displayNameCache[new CaseInsensitiveString(id)] = displayName;
            }
        }

        /// <summary>
        /// Register a factory object with the given ID.  The factory
        /// method should return a new instance of the given transliterator.
        /// <para/>
        /// Because ICU may choose to cache <see cref="Transliterator"/> objects internally, this must
        /// be called at application startup, prior to any calls to
        /// <see cref="GetInstance(string)"/> to avoid undefined behavior.
        /// </summary>
        /// <param name="ID">The ID of this transliterator.</param>
        /// <param name="factory">The factory object.</param>
        /// <stable>ICU 2.0</stable>
        public static void RegisterFactory(string ID, ITransliteratorFactory factory)
        {
            registry.Put(ID, factory, true);
        }

        /// <summary>
        /// Register a <see cref="Transliterator"/> object with the given ID.
        /// <para/>
        /// Because ICU may choose to cache Transliterator objects internally, this must
        /// be called at application startup, prior to any calls to
        /// <see cref="GetInstance(string)"/> to avoid undefined behavior.
        /// </summary>
        /// <param name="trans">Trans the <see cref="Transliterator"/> object.</param>
        /// <stable>ICU 2.2</stable>
        public static void RegisterInstance(Transliterator trans)
        {
            registry.Put(trans.ID, trans, true);
        }

        /// <summary>
        /// Register a <see cref="Transliterator"/> object.
        /// <para/>
        /// Because ICU may choose to cache <see cref="Transliterator"/> objects internally, this must
        /// be called at application startup, prior to any calls to
        /// <see cref="GetInstance(string)"/> to avoid undefined behavior.
        /// </summary>
        /// <param name="trans">Trans the <see cref="Transliterator"/> object.</param>
        /// <param name="visible"></param>
        internal static void RegisterInstance(Transliterator trans, bool visible)
        {
            registry.Put(trans.ID, trans, visible);
        }

        /// <summary>
        /// Register an ID as an alias of another ID.  Instantiating
        /// alias ID produces the same result as instantiating the original ID.
        /// This is generally used to create short aliases of compound IDs.
        /// <para/>
        /// Because ICU may choose to cache <see cref="Transliterator"/> objects internally, this must
        /// be called at application startup, prior to any calls to
        /// <see cref="GetInstance(string)"/> to avoid undefined behavior.
        /// </summary>
        /// <param name="aliasID">The new ID being registered.</param>
        /// <param name="realID">The existing ID that the new ID should be an alias of.</param>
        /// <stable>ICU 3.6</stable>
        public static void RegisterAlias(string aliasID, string realID)
        {
            registry.Put(aliasID, realID, true);
        }

        /// <summary>
        /// Register two targets as being inverses of one another.  For
        /// example, calling registerSpecialInverse("NFC", "NFD", true) causes
        /// <see cref="Transliterator"/> to form the following inverse relationships:
        /// <code>
        /// NFC =&gt; NFD
        /// Any-NFC =&gt; Any-NFD
        /// NFD =&gt; NFC
        /// Any-NFD =&gt; Any-NFC
        /// </code>
        /// <para/>
        /// (Without the special inverse registration, the inverse of NFC
        /// would be NFC-Any.)  Note that NFD is shorthand for Any-NFD, but
        /// that the presence or absence of "Any-" is preserved.
        /// <para/>
        /// The relationship is symmetrical; registering (a, b) is
        /// equivalent to registering (b, a).
        /// <para/>
        /// The relevant IDs must still be registered separately as
        /// factories or classes.
        /// <para/>
        /// Only the targets are specified.  Special inverses always
        /// have the form Any-Target1 &lt;=&gt; Any-Target2.  The target should
        /// have canonical casing (the casing desired to be produced when
        /// an inverse is formed) and should contain no whitespace or other
        /// extraneous characters.
        /// </summary>
        /// <param name="target">The target against which to register the inverse.</param>
        /// <param name="inverseTarget">The inverse of target, that is
        /// Any-target.GetInverse() =&gt; Any-inverseTarget</param>
        /// <param name="bidirectional">If true, register the reverse relation
        /// as well, that is, Any-inverseTarget.GetInverse() =&gt; Any-target</param>
        internal static void RegisterSpecialInverse(string target,
                                           string inverseTarget,
                                           bool bidirectional)
        {
            TransliteratorIDParser.RegisterSpecialInverse(target, inverseTarget, bidirectional);
        }

        /// <summary>
        /// Unregisters a transliterator or class.  This may be either
        /// a system transliterator or a user transliterator or class.
        /// </summary>
        /// <param name="ID">the ID of the transliterator or class</param>
        /// <seealso cref="RegisterType(string, Type, string)"/>
        /// <stable>ICU 2.0</stable>
        public static void Unregister(string ID)
        {
            displayNameCache.Remove(new CaseInsensitiveString(ID));
            registry.Remove(ID);
        }

        /// <summary>
        /// Returns an enumeration over the programmatic names of registered
        /// <see cref="Transliterator"/> objects.  This includes both system
        /// transliterators and user transliterators registered using
        /// <see cref="RegisterType(string, Type, string)"/>. The enumerated names may be
        /// passed to <see cref="GetInstance(string)"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{String}"/>.</returns>
        /// <seealso cref="GetInstance(string)"/>
        /// <seealso cref="GetInstance(string, TransliterationDirection)"/>
        /// <stable>ICU 2.0</stable>
        public static IEnumerable<string> GetAvailableIDs()
        {
            return registry.GetAvailableIDs();
        }

        /// <summary>
        /// Returns an enumeration over the source names of registered
        /// transliterators.  Source names may be passed to
        /// <see cref="GetAvailableTargets(string)"/> to obtain available targets for each
        /// source.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public static IEnumerable<string> GetAvailableSources()
        {
            return registry.GetAvailableSources();
        }

        /// <summary>
        /// Returns an enumeration over the target names of registered
        /// transliterators having a given source name.  Target names may
        /// be passed to <see cref="GetAvailableTargets(string)"/> to obtain available
        /// variants for each source and target pair.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public static IEnumerable<string> GetAvailableTargets(string source)
        {
            return registry.GetAvailableTargets(source);
        }

        /// <summary>
        /// Returns an enumeration over the variant names of registered
        /// transliterators having a given source name and target name.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public static IEnumerable<string> GetAvailableVariants(string source,
                                                             string target)
        {
            return registry.GetAvailableVariants(source, target);
        }
        private const string ROOT = "root",
                                        RB_RULE_BASED_IDS = "RuleBasedTransliteratorIDs";
        static Transliterator() // ICU4N TODO: Avoid static constructor
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
            // ICU4N specific - we must pass this assembly so the resources are loaded from here
            bundle = UResourceBundle.GetBundleInstance(ICUData.IcuTransliteratorBaseName, ROOT, ICUResourceBundle.IcuDataAssembly);
            transIDs = bundle.Get(RB_RULE_BASED_IDS);

            int row, maxRows;
            maxRows = transIDs.Length;
            for (row = 0; row < maxRows; row++)
            {
                colBund = transIDs.Get(row);
                string ID = colBund.Key;
                if (ID.IndexOf("-t-", StringComparison.Ordinal) >= 0)
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
                    TransliterationDirection dir;
                    string direction = res.GetString("direction");
                    switch (direction[0])
                    {
                        case 'F':
                            dir = Forward;
                            break;
                        case 'R':
                            dir = Reverse;
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
            RegisterType(NullTransliterator._ID,
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

        /// <summary>
        /// Register the script-based "Any" transliterators: Any-Latin, Any-Greek
        /// </summary>
        [Obsolete("This API is ICU internal only.")]
        internal static void RegisterAny() // ICU4N specific - marked internal instead of public, since the functionality is obsolete
        {
            AnyTransliterator.Register();
        }

        /// <summary>
        /// Implements StringTransform via this method.
        /// </summary>
        /// <param name="source">text to be transformed (eg lowercased)</param>
        /// <stable>ICU 3.8</stable>
        public virtual string Transform(string source)
        {
            return Transliterate(source);
        }
    }
}
