using ICU4N.Impl;
using System;

namespace ICU4N.Text
{
    /// <summary>
    /// This class has been deprecated since ICU 2.2.
    /// One problem is that this class is not designed to return supplementary characters.
    /// Use the <see cref="Normalizer2"/> and <see cref="UChar"/> classes instead.
    /// </summary>
    /// <remarks>
    /// <see cref="ComposedCharIter"/> is an iterator class that returns all
    /// of the precomposed characters defined in the Unicode standard, along
    /// with their decomposed forms.  This is often useful when building
    /// data tables (<i>e.g.</i> collation tables) which need to treat composed
    /// and decomposed characters equivalently.
    /// <para/>
    /// For example, imagine that you have built a collation table with ordering
    /// rules for the canonically decomposed (<see cref="Normalizer.DECOMP"/>) forms of all
    /// characters used in a particular language.  When you process input text using
    /// this table, the text must first be decomposed so that it matches the form
    /// used in the table.  This can impose a performance penalty that may be
    /// unacceptable in some situations.
    /// <para/>
    /// You can avoid this problem by ensuring that the collation table contains
    /// rules for both the decomposed <i>and</i> composed versions of each character.
    /// To do so, use a <see cref="ComposedCharIter"/> to iterate through all of the
    /// composed characters in Unicode.  If the decomposition for that character
    /// consists solely of characters that are listed in your ruleset, you can
    /// add a new rule for the composed character that makes it equivalent to
    /// its decomposition sequence.
    /// <para/>
    /// Note that <see cref="ComposedCharIter"/> iterates over a <em>static</em> table
    /// of the composed characters in Unicode.  If you want to iterate over the
    /// composed characters in a particular string, use <see cref="Normalizer"/> instead.
    /// <para/>
    /// When constructing a <see cref="ComposedCharIter"/> there is one
    /// optional feature that you can enable or disable:
    /// <list type="bullet">
    ///     <item><description><see cref="Normalizer.IGNORE_HANGUL"/> - Do not iterate over the Hangul
    ///          characters and their corresponding Jamo decompositions.
    ///          This option is off by default (<i>i.e.</i> Hangul processing is enabled)
    ///          since the Unicode standard specifies that Hangul to Jamo 
    ///          is a canonical decomposition.</description></item>
    /// </list>
    /// <para/>
    /// <see cref="ComposedCharIter"/> is currently based on version 2.1.8 of the
    /// <a href="http://www.unicode.org" target="unicode">Unicode Standard</a>.
    /// It will be updated as later versions of Unicode are released.
    /// </remarks>
    [Obsolete("ICU 2.2")]
    public sealed class ComposedCharIter
    {
        /// <summary>
        /// Constant that indicates the iteration has completed.
        /// <see cref="Next()"/> returns this value when there are no more composed characters
        /// over which to iterate.
        /// </summary>
        [Obsolete("ICU 2.2")]
        public const char Done = unchecked((char)Normalizer.Done);

        /// <summary>
        /// Construct a new <see cref="ComposedCharIter"/>.  The iterator will return
        /// all Unicode characters with canonical decompositions, including Korean
        /// Hangul characters.
        /// </summary>
        [Obsolete("ICU 2.2")]
        public ComposedCharIter()
            : this(false, 0)
        {
        }

        /// <summary>
        /// Constructs a non-default <see cref="ComposedCharIter"/> with optional behavior.
        /// </summary>
        /// <param name="compat"><tt>false</tt> for canonical decompositions only;
        /// <tt>true</tt> for both canonical and compatibility decompositions.</param>
        /// <param name="options">Optional decomposition features. None are supported, so this is ignored.</param>
        [Obsolete("ICU 2.2")]
        public ComposedCharIter(bool compat, int options)
        {
            if (compat)
            {
                n2impl = Norm2AllModes.GetNFKCInstance().Impl;
            }
            else
            {
                n2impl = Norm2AllModes.GetNFCInstance().Impl;
            }
        }

        /// <summary>
        /// Determines whether there any precomposed Unicode characters not yet returned
        /// by <see cref="Next()"/>
        /// </summary>
        /// <returns></returns>
        [Obsolete("ICU 2.2")]
        public bool HasNext
        {
            get
            {
                if (nextChar == Normalizer.Done)
                {
                    FindNextChar();
                }
                return nextChar != Normalizer.Done;
            }
        }

        /// <summary>
        /// Returns the next precomposed Unicode character.
        /// Repeated calls to <see cref="Next()"/> return all of the precomposed characters defined
        /// by Unicode, in ascending order.  After all precomposed characters have
        /// been returned, <see cref="HasNext"/> will return <c>false</c> and further calls
        /// to <see cref="Next()"/> will return <see cref="Done"/>.
        /// </summary>
        /// <returns></returns>
        [Obsolete("ICU 2.2")]
        public char Next()
        {
            if (nextChar == Normalizer.Done)
            {
                FindNextChar();
            }
            curChar = nextChar;
            nextChar = Normalizer.Done;
            return (char)curChar;
        }

        /// <summary>
        /// Returns the Unicode decomposition of the current character.
        /// This method returns the decomposition of the precomposed character most
        /// recently returned by <see cref="Next()"/>.  The resulting decomposition is
        /// affected by the settings of the options passed to the constructor.
        /// </summary>
        [Obsolete("ICU 2.2")]
        public string Decomposition()
        {
            // the decomposition buffer contains the decomposition of 
            // current char so just return it
            if (decompBuf != null)
            {
                return decompBuf;
            }
            else
            {
                return "";
            }
        }

        private void FindNextChar()
        {
            int c = curChar + 1;
            decompBuf = null;
            for (; ; )
            {
                if (c < 0xFFFF)
                {
                    decompBuf = n2impl.GetDecomposition(c);
                    if (decompBuf != null)
                    {
                        // the curChar can be decomposed... so it is a composed char
                        // cache the result     
                        break;
                    }
                    c++;
                }
                else
                {
                    c = Normalizer.Done;
                    break;
                }
            }
            nextChar = c;
        }

        private readonly Normalizer2Impl n2impl;
        private string decompBuf;
        private int curChar = 0;
        private int nextChar = Normalizer.Done;
    }
}
