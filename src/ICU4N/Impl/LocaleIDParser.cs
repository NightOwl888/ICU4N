using ICU4N.Impl.Locale;
using ICU4N.Support.Collections;
using ICU4N.Support.Text;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Globalization // ICU4N: Moved from ICU4N.Impl namespace
{
    /// <summary>
    /// Utility class to parse and normalize locale ids (including POSIX style)
    /// </summary>
#if FEATURE_SPAN
    public ref struct LocaleIDParser
#else
    public sealed class LocaleIDParser : IDisposable
#endif
    {
        /// <summary>
        /// Char array representing the locale ID.
        /// </summary>
#if FEATURE_SPAN
        private ReadOnlySpan<char> id;
#else
        private char[] id;
#endif

        /// <summary>
        /// Current position in <see cref="id"/> (while parsing).
        /// </summary>
        private int index;

        /// <summary>
        /// Temporary buffer for parsed sections of data.
        /// </summary>
#if FEATURE_SPAN
        private ValueStringBuilder buffer;
#else
        private StringBuilder buffer;
#endif

        // um, don't handle POSIX ids unless we request it.  why not?  well... because.
        private bool canonicalize;
        private bool hadCountry;

        // used when canonicalizing
        private IDictionary<string, string> keywords;
#if FEATURE_SPAN
        private ReadOnlySpan<char> baseName;
#else
        private string baseName;
#endif

        /// <summary>
        /// Parsing constants.
        /// </summary>
        private const char KEYWORD_SEPARATOR = '@';
        private const char HYPHEN = '-';
        private const char KEYWORD_ASSIGN = '=';
        private const char COMMA = ',';
        private const char ITEM_SEPARATOR = ';';
        private const char DOT = '.';
        private const char UNDERSCORE = '_';

#if FEATURE_SPAN

        public LocaleIDParser(Span<char> initialBuffer, ReadOnlySpan<char> localeID)
            : this(initialBuffer, localeID, false)
        {
        }

        public LocaleIDParser(Span<char> initialBuffer, ReadOnlySpan<char> localeID, bool canonicalize)
        {
            id = localeID;
            index = 0;
            buffer = new ValueStringBuilder(initialBuffer);
            this.canonicalize = canonicalize;
            this.hadCountry = false;
            this.keywords = null;
            this.baseName = ReadOnlySpan<char>.Empty;
        }
#else

        public LocaleIDParser(string localeID)
            : this(localeID, false)
        {
        }

        public LocaleIDParser(string localeID, bool canonicalize)
        {
            Reset(localeID, canonicalize);
        }
#endif

#if FEATURE_SPAN
        public void Reset(ReadOnlySpan<char> localeID)
        {
            Reset(localeID, false);
        }

        public void Reset(ReadOnlySpan<char> localeID, bool canonicalize)
        {
            id = localeID;
            index = 0;
            buffer.Length = 0;
            this.canonicalize = canonicalize;
            this.hadCountry = false;
        }
#else

        public void Reset(string localeID)
        {
            Reset(localeID, false);
        }

        public void Reset(string localeID, bool canonicalize)
        {
            id = localeID.ToCharArray();
            index = 0;
            buffer = new StringBuilder(id.Length + 5);
            this.canonicalize = canonicalize;
            this.hadCountry = false;
        }
#endif

        private void Reset()
        {
            index = 0;
#if FEATURE_SPAN
            buffer.Length = 0;
#else
            buffer = new StringBuilder(id.Length + 5);
#endif
        }

        // utilities for working on text in the buffer

        /// <summary>
        /// Append <paramref name="c"/> to the buffer.
        /// </summary>
        private void Append(char c)
        {
            buffer.Append(c);
        }

        private void AddSeparator(char separator = UNDERSCORE)
        {
            Append(separator);
        }

        /// <summary>
        /// Returns the text in the buffer from start to blen as a string.
        /// </summary>
        private string GetString(int start)
        {
#if FEATURE_SPAN
            return buffer.AsSpan(start).ToString();
#else
            return buffer.ToString(start, buffer.Length - start);
#endif
        }

#if FEATURE_SPAN
        private ReadOnlySpan<char> AsSpan(int start)
        {
            return buffer.AsSpan(start);
        }
#endif

        /// <summary>
        /// Set the length of the buffer to pos, then append the string.
        /// </summary>
#if FEATURE_SPAN
        private void Set(int pos, ReadOnlySpan<char> s)
        {
            buffer.Length = pos;
            buffer.Insert(pos, s);
        }
#else
        private void Set(int pos, string s)
        {
            buffer.Delete(pos, buffer.Length - pos); // ICU4N: Corrected 2nd parameter
            buffer.Insert(pos, s);
        }
#endif

        /// <summary>
        /// Append the string to the buffer.
        /// </summary>
        private void Append(string s)
        {
            buffer.Append(s);
        }

#if FEATURE_SPAN

        /// <summary>
        /// Append the span to the buffer.
        /// </summary>
        private void Append(ReadOnlySpan<char> s)
        {
            buffer.Append(s);
        }

#endif

        // utilities for parsing text out of the id

        /// <summary>
        /// Character to indicate no more text is available in the id.
        /// </summary>
        private const char DONE = '\uffff';

        /// <summary>
        /// Returns the character at index in the id, and advance index.  The returned character
        /// is <see cref="DONE"/> if index was at the limit of the buffer.  The index is advanced regardless
        /// so that decrementing the index will always 'unget' the last character returned.
        /// </summary>
        /// <returns></returns>
        private char Next()
        {
            if (index == id.Length)
            {
                index++;
                return DONE;
            }

            return id[index++];
        }

        /// <summary>
        /// Advance index until the next terminator or id separator, and leave it there.
        /// </summary>
        private void SkipUntilTerminatorOrIDSeparator()
        {
            while (!IsTerminatorOrIDSeparator(Next())) ;
            --index;
        }

        /// <summary>
        /// Returns true if the character at index in the id is a terminator.
        /// </summary>
        private bool AtTerminator()
        {
            return index >= id.Length || IsTerminator(id[index]);
        }

        /// <summary>
        /// Returns true if the character is a terminator (keyword separator, dot, or DONE).
        /// Dot is a terminator because of the POSIX form, where dot precedes the codepage.
        /// </summary>
        private bool IsTerminator(char c)
        {
            // always terminate at DOT, even if not handling POSIX.  It's an error...
            return c == KEYWORD_SEPARATOR || c == DONE || c == DOT;
        }

        /// <summary>
        /// Returns true if the character is a terminator or id separator.
        /// </summary>
        private bool IsTerminatorOrIDSeparator(char c)
        {
            return c == UNDERSCORE || c == HYPHEN || IsTerminator(c);
        }

        /// <summary>
        /// Returns true if the start of the buffer has an experimental or private language
        /// prefix, the pattern '[ixIX][-_].' shows the syntax checked.
        /// </summary>
        private bool HaveExperimentalLanguagePrefix()
        {
            if (id.Length > 2)
            {
                char c = id[1];
                if (c == HYPHEN || c == UNDERSCORE)
                {
                    c = id[0];
                    return c == 'x' || c == 'X' || c == 'i' || c == 'I';
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if a value separator occurs at or after index.
        /// </summary>
        private bool HaveKeywordAssign()
        {
            // assume it is safe to start from index
            for (int i = index; i < id.Length; ++i)
            {
                if (id[i] == KEYWORD_ASSIGN)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Advance index past language, and accumulate normalized language code in buffer.
        /// Index must be at 0 when this is called.  Index is left at a terminator or id
        /// separator.  Returns the start of the language code in the buffer.
        /// </summary>
        private int ParseLanguage()
        {
            int startLength = buffer.Length;

            if (HaveExperimentalLanguagePrefix())
            {
                Append(AsciiUtil.ToLower(id[0]));
                Append(HYPHEN);
                index = 2;
            }

            char c;
            while (!IsTerminatorOrIDSeparator(c = Next()))
            {
                Append(AsciiUtil.ToLower(c));
            }
            --index; // unget

            if (buffer.Length - startLength == 3)
            {
#if FEATURE_SPAN
                string lang = LocaleIDs.ThreeToTwoLetterLanguage(AsSpan(0));
                if (lang != null)
                {
                    Set(0, lang.AsSpan());
                }
#else
                string lang = LocaleIDs.ThreeToTwoLetterLanguage(GetString(0));
                if (lang != null)
                {
                    Set(0, lang);
                }
#endif
            }

            return 0;
        }

        /// <summary>
        /// Advance index past language.  Index must be at 0 when this is called.  Index
        /// is left at a terminator or id separator.
        /// </summary>
        private void SkipLanguage()
        {
            if (HaveExperimentalLanguagePrefix())
            {
                index = 2;
            }
            SkipUntilTerminatorOrIDSeparator();
        }

        /// <summary>
        /// Advance index past script, and accumulate normalized script in buffer.
        /// Index must be immediately after the language.
        /// If the item at this position is not a script (is not four characters
        /// long) leave index and buffer unchanged.  Otherwise index is left at
        /// a terminator or id separator.  Returns the start of the script code
        /// in the buffer (this may be equal to the buffer length, if there is no
        /// script).
        /// </summary>
        private int ParseScript(char separator = UNDERSCORE)
        {
            if (!AtTerminator())
            {
                int oldIndex = index; // save original index
                ++index;

                int oldBlen = buffer.Length; // get before append hyphen, if we truncate everything is undone
                char c;
                bool firstPass = true;
                while (!IsTerminatorOrIDSeparator(c = Next()) && AsciiUtil.IsAlpha(c))
                {
                    if (firstPass)
                    {
                        AddSeparator(separator);
                        Append(AsciiUtil.ToUpper(c));
                        firstPass = false;
                    }
                    else
                    {
                        Append(AsciiUtil.ToLower(c));
                    }
                }
                --index; // unget

                /* If it's not exactly 4 characters long, then it's not a script. */
                if (index - oldIndex != 5)
                { // +1 to account for separator
                    index = oldIndex;
                    //buffer.Delete(oldBlen, buffer.Length - oldBlen); // ICU4N: Corrected 2nd parameter
                    buffer.Length = oldBlen;
                }
                else
                {
                    oldBlen++; // index past hyphen, for clients who want to extract just the script
                }

                return oldBlen;
            }
            return buffer.Length;
        }

        /// <summary>
        /// Advance index past script.
        /// Index must be immediately after the language and IDSeparator.
        /// If the item at this position is not a script (is not four characters
        /// long) leave index.  Otherwise index is left at a terminator or
        /// id separator.
        /// </summary>
        private void SkipScript()
        {
            if (!AtTerminator())
            {
                int oldIndex = index;
                ++index;

                char c;
                while (!IsTerminatorOrIDSeparator(c = Next()) && AsciiUtil.IsAlpha(c)) ;
                --index;

                if (index - oldIndex != 5)
                { // +1 to account for separator
                    index = oldIndex;
                }
            }
        }

        /// <summary>
        /// Advance index past country, and accumulate normalized country in buffer.
        /// Index must be immediately after the script (if there is one, else language)
        /// and IDSeparator.  Return the start of the country code in the buffer.
        /// </summary>
        /// <returns></returns>
        private int ParseCountry(char separator = UNDERSCORE)
        {
            if (!AtTerminator())
            {
                int oldIndex = index;
                ++index;

                int oldBlen = buffer.Length;
                char c;
                bool firstPass = true;
                while (!IsTerminatorOrIDSeparator(c = Next()))
                {
                    if (firstPass)
                    { // first, add hyphen
                        hadCountry = true; // we have a country, let variant parsing know
                        AddSeparator(separator);
                        ++oldBlen; // increment past hyphen
                        firstPass = false;
                    }
                    Append(AsciiUtil.ToUpper(c));
                }
                --index; // unget

                int charsAppended = buffer.Length - oldBlen;

                if (charsAppended == 0)
                {
                    // Do nothing.
                }
                else if (charsAppended < 2 || charsAppended > 3)
                {
                    // It's not a country, so return index and blen to
                    // their previous values.
                    index = oldIndex;
                    --oldBlen;
                    //buffer.Delete(oldBlen, buffer.Length - oldBlen); // ICU4N: Corrected 2nd parameter
                    buffer.Length = oldBlen;
                    hadCountry = false;
                }
                else if (charsAppended == 3)
                {
#if FEATURE_SPAN
                    string region = LocaleIDs.ThreeToTwoLetterRegion(AsSpan(oldBlen));
                    if (region != null)
                    {
                        Set(oldBlen, region.AsSpan());
                    }
#else
                    string region = LocaleIDs.ThreeToTwoLetterRegion(GetString(oldBlen));
                    if (region != null)
                    {
                        Set(oldBlen, region);
                    }
#endif

                }

                return oldBlen;
            }

            return buffer.Length;
        }

        /// <summary>
        /// Advance index past country.
        /// Index must be immediately after the script (if there is one, else language)
        /// and IDSeparator.
        /// </summary>
        private void SkipCountry()
        {
            if (!AtTerminator())
            {
                if (id[index] == UNDERSCORE || id[index] == HYPHEN)
                {
                    ++index;
                }
                /*
                 * Save the index point after the separator, since the format
                 * requires two separators if the country is not present.
                 */
                int oldIndex = index;

                SkipUntilTerminatorOrIDSeparator();
                int charsSkipped = index - oldIndex;
                if (charsSkipped < 2 || charsSkipped > 3)
                {
                    index = oldIndex;
                }
            }
        }

        /// <summary>
        /// Advance index past variant, and accumulate normalized variant in buffer.  This ignores
        /// the codepage information from POSIX ids.  Index must be immediately after the country
        /// or script.  Index is left at the keyword separator or at the end of the text.  Return
        /// the start of the variant code in the buffer.
        /// </summary>
        /// <remarks>
        /// In standard form, we can have the following forms:
        /// <list type="bullet">
        ///     <item><description>ll__VVVV</description></item>
        ///     <item><description>ll_CC_VVVV</description></item>
        ///     <item><description>ll_Ssss_VVVV</description></item>
        ///     <item><description>ll_Ssss_CC_VVVV</description></item>
        /// </list>
        /// <para/>
        /// This also handles POSIX ids, which can have the following forms (pppp is code page id):
        /// <list type="bullet">
        ///     <item><description>ll_CC.pppp          --> ll_CC</description></item>
        ///     <item><description>ll_CC.pppp@VVVV     --> ll_CC_VVVV</description></item>
        ///     <item><description>ll_CC@VVVV          --> ll_CC_VVVV</description></item>
        /// </list>
        /// <para/>
        /// We identify this use of '@' in POSIX ids by looking for an '=' following
        /// the '@'.  If there is one, we consider '@' to start a keyword list, instead of
        /// being part of a POSIX id.
        /// <para/>
        /// Note:  since it was decided that we want an option to not handle POSIX ids, this
        /// becomes a bit more complex.
        /// </remarks>
        private int ParseVariant(char separator = UNDERSCORE)
        {
            int oldBlen = buffer.Length;

            bool start = true;
            bool needSeparator = true;
            bool skipping = false;
            char c;
            bool firstPass = true;

            while ((c = Next()) != DONE)
            {
                if (c == DOT)
                {
                    start = false;
                    skipping = true;
                }
                else if (c == KEYWORD_SEPARATOR)
                {
                    if (HaveKeywordAssign())
                    {
                        break;
                    }
                    skipping = false;
                    start = false;
                    needSeparator = true; // add another underscore if we have more text
                }
                else if (start)
                {
                    start = false;
                    if (c != UNDERSCORE && c != HYPHEN)
                    {
                        index--;
                    }
                }
                else if (!skipping)
                {
                    if (needSeparator)
                    {
                        needSeparator = false;
                        if (firstPass && !hadCountry)
                        { // no country, we'll need two
                            AddSeparator(separator);
                            ++oldBlen; // for sure
                        }
                        AddSeparator(separator);
                        if (firstPass)
                        { // only for the first separator
                            ++oldBlen;
                            firstPass = false;
                        }
                    }
                    c = AsciiUtil.ToUpper(c);
                    if (c == HYPHEN || c == COMMA)
                    {
                        c = UNDERSCORE;
                    }
                    Append(c);
                }
            }
            --index; // unget

            return oldBlen;
        }

        // no need for skipvariant, to get the keywords we'll just scan directly for
        // the keyword separator


#if FEATURE_SPAN
        /// <summary>
        /// Copies the normalized language id into <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">The span in which to write this instance's language id as a span of characters.</param>
        /// <param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination"/>.</param>
        /// <returns><c>false</c> if <paramref name="destination"/> is not long enough; otherwise, <c>true</c>.</returns>
        public bool TryGetLanguage(Span<char> destination, out int charsWritten)
        {
            Reset();
            ReadOnlySpan<char> result = AsSpan(ParseLanguage());
            bool success = result.TryCopyTo(destination);
            charsWritten = success ? result.Length : 0;
            return success;
        }

        /// <summary>
        /// Copies the normalized language id into <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">The span in which to write this instance's language id as a span of characters.</param>
        /// <param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination"/>.</param>
        /// <exception cref="ArgumentException"><paramref name="destination"/> is too short.</exception>
        public void GetLanguage(Span<char> destination, out int charsWritten)
        {
            Reset();
            ReadOnlySpan<char> result = AsSpan(ParseLanguage());
            result.CopyTo(destination);
            charsWritten = result.Length;
        }
#endif
        /// <summary>
        /// Returns the normalized language id, or the empty string.
        /// </summary>
        public string GetLanguage()
        {
            Reset();
            return GetString(ParseLanguage());
        }


#if FEATURE_SPAN
        /// <summary>
        /// Copies the normalized script id into <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">The span in which to write this instance's script id as a span of characters.</param>
        /// <param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination"/>.</param>
        /// <returns><c>false</c> if <paramref name="destination"/> is not long enough; otherwise, <c>true</c>.</returns>
        public bool TryGetScript(Span<char> destination, out int charsWritten)
        {
            Reset();
            SkipLanguage();
            ReadOnlySpan<char> result = AsSpan(ParseScript());
            bool success = result.TryCopyTo(destination);
            charsWritten = success ? result.Length : 0;
            return success;
        }

        /// <summary>
        /// Copies the normalized script id into <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">The span in which to write this instance's script id as a span of characters.</param>
        /// <param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination"/>.</param>
        /// <exception cref="ArgumentException"><paramref name="destination"/> is too short.</exception>
        public void GetScript(Span<char> destination, out int charsWritten)
        {
            Reset();
            SkipLanguage();
            ReadOnlySpan<char> result = AsSpan(ParseScript());
            result.CopyTo(destination);
            charsWritten = result.Length;
        }
#endif

        /// <summary>
        /// Returns the normalized script id, or the empty string.
        /// </summary>
        public string GetScript()
        {
            Reset();
            SkipLanguage();
            return GetString(ParseScript());
        }



#if FEATURE_SPAN
        /// <summary>
        /// Copies the normalized country id into <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">The span in which to write this instance's country id as a span of characters.</param>
        /// <param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination"/>.</param>
        /// <returns><c>false</c> if <paramref name="destination"/> is not long enough; otherwise, <c>true</c>.</returns>
        public bool TryGetCountry(Span<char> destination, out int charsWritten)
        {
            Reset();
            SkipLanguage();
            SkipScript();
            ReadOnlySpan<char> result = AsSpan(ParseCountry());
            bool success = result.TryCopyTo(destination);
            charsWritten = success ? result.Length : 0;
            return success;
        }

        /// <summary>
        /// Copies the normalized country id into <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">The span in which to write this instance's country id as a span of characters.</param>
        /// <param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination"/>.</param>
        /// <exception cref="ArgumentException"><paramref name="destination"/> is too short.</exception>
        public void GetCountry(Span<char> destination, out int charsWritten)
        {
            Reset();
            SkipLanguage();
            SkipScript();
            ReadOnlySpan<char> result = AsSpan(ParseCountry());
            result.CopyTo(destination);
            charsWritten = result.Length;
        }
#endif

        /// <summary>
        /// Returns the normalized country id, or the empty string.
        /// </summary>
        public string GetCountry()
        {
            Reset();
            SkipLanguage();
            SkipScript();
            return GetString(ParseCountry());
        }



#if FEATURE_SPAN
        /// <summary>
        /// Copies the normalized variant id into <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">The span in which to write this instance's variant id as a span of characters.</param>
        /// <param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination"/>.</param>
        /// <returns><c>false</c> if <paramref name="destination"/> is not long enough; otherwise, <c>true</c>.</returns>
        public bool TryGetVariant(Span<char> destination, out int charsWritten)
        {
            Reset();
            SkipLanguage();
            SkipScript();
            SkipCountry();
            ReadOnlySpan<char> result = AsSpan(ParseVariant());
            bool success = result.TryCopyTo(destination);
            charsWritten = success ? result.Length : 0;
            return success;
        }

        /// <summary>
        /// Copies the normalized variant id into <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">The span in which to write this instance's variant id as a span of characters.</param>
        /// <param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination"/>.</param>
        /// <exception cref="ArgumentException"><paramref name="destination"/> is too short.</exception>
        public void GetVariant(Span<char> destination, out int charsWritten)
        {
            Reset();
            SkipLanguage();
            SkipScript();
            SkipCountry();
            ReadOnlySpan<char> result = AsSpan(ParseVariant());
            result.CopyTo(destination);
            charsWritten = result.Length;
        }
#endif

        /// <summary>
        /// Returns the normalized variant id, or the empty string.
        /// </summary>
        public string GetVariant()
        {
            Reset();
            SkipLanguage();
            SkipScript();
            SkipCountry();
            return GetString(ParseVariant());
        }

        /// <summary>
        /// Returns the language id, script id, country id, and variant id as separate strings
        /// in a <see cref="LocaleID"/> structure.
        /// </summary>
        public LocaleID GetLocaleID()
        {
            Reset();
            return new LocaleID(
                language: GetString(ParseLanguage()),
                script: GetString(ParseScript()),
                country: GetString(ParseCountry()),
                variant: GetString(ParseVariant())
            );
        }

#if FEATURE_SPAN
        public void SetBaseName(ReadOnlySpan<char> baseName)
#else
        public void SetBaseName(string baseName)
#endif
        {
            this.baseName = baseName;
        }

        public void ParseBaseName(char separator = UNDERSCORE)
        {
#if FEATURE_SPAN
            if (!baseName.IsEmpty)
#else
            if (baseName != null)
#endif
            {
                Set(0, baseName);
            }
            else
            {
                Reset();
                ParseLanguage();
                ParseScript(separator);
                ParseCountry(separator);
                ParseVariant(separator);

                // catch unwanted trailing underscore or hyphen after country if there was no variant
                int len = buffer.Length;
                if (len > 0 && (buffer[len - 1] == UNDERSCORE || buffer[len - 1] == HYPHEN))
                {
                    //buffer.DeleteCharAt(len - 1);
                    buffer.Remove(len - 1, 1);
                }
            }
        }

        private void RemoveLeadingSeparator(char separator = UNDERSCORE)
        {
            int len = buffer.Length;
            if (len > 0 && (buffer[0] == separator))
            {
                buffer.Remove(0, 1);
            }
        }

#if FEATURE_SPAN
        /// <summary>
        /// Copies the normalized base form of the locale id to <paramref name="destination"/>.
        /// The base form does not include keywords.
        /// <para/>
        /// Usage Note: The destination may be longer than the locale id. It is recommended to use a buffer
        /// at least <c>localeID.Length + 10</c>.
        /// </summary>
        /// <param name="destination">The span in which to write this instance's locale id as a span of characters.</param>
        /// <param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination"/>.</param>
        /// <returns><c>false</c> if <paramref name="destination"/> is not long enough; otherwise, <c>true</c>.</returns>
        public bool TryGetBaseName(Span<char> destination, out int charsWritten)
        {
            ReadOnlySpan<char> result;
            if (!baseName.IsEmpty)
            {
                result = baseName;
            }
            else
            {
                ParseBaseName();
                result = AsSpan(0);
            }
            bool success = result.TryCopyTo(destination);
            charsWritten = success ? result.Length : 0;
            return success;
        }

        /// <summary>
        /// Copies the normalized base form of the locale id to <paramref name="destination"/>.
        /// The base form does not include keywords.
        /// <para/>
        /// Usage Note: The destination may be longer than the locale id. It is recommended to use a buffer
        /// at least <c>localeID.Length + 10</c>.
        /// </summary>
        /// <param name="destination">The span in which to write this instance's locale id as a span of characters.</param>
        /// <param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination"/>.</param>
        /// <exception cref="ArgumentException"><paramref name="destination"/> is too short.</exception>
        public void GetBaseName(Span<char> destination, out int charsWritten)
        {
            ReadOnlySpan<char> result;
            if (!baseName.IsEmpty)
            {
                result = baseName;
            }
            else
            {
                ParseBaseName();
                result = AsSpan(0);
            }
            result.CopyTo(destination);
            charsWritten = result.Length;
        }

        /// <summary>
        /// Returns the base name as a <see cref="ReadOnlySpan{T}"/>.
        /// Note this value may be overwritten if another Get or TryGet
        /// method is called, so it may only be used prior to those operations.
        /// <para/>
        /// Despite this limitation, this method allows the use of the base name
        /// without having to allocate memory first.
        /// </summary>
        /// <returns>The base name as a <see cref="ReadOnlySpan{T}"/>.</returns>
        internal ReadOnlySpan<char> GetBaseNameAsSpan()
        {
            if (!baseName.IsEmpty)
            {
                return baseName;
            }
            ParseBaseName();
            return AsSpan(0);
        }
#endif

        /// <summary>
        /// Returns the normalized base form of the locale id.  The base
        /// form does not include keywords.
        /// </summary>
        public string GetBaseName()
        {
#if FEATURE_SPAN
            if (!baseName.IsEmpty)
            {
                return baseName.ToString();
            }
#else
            if (baseName != null)
            {
                return baseName;
            }
#endif
            ParseBaseName();
            return GetString(0);
        }


#if FEATURE_SPAN
        /// <summary>
        /// Copies the normalized base form of the locale id using hyphen as the
        /// separator to <paramref name="destination"/>. This is for compatibility with <see cref="System.Globalization.CultureInfo.Name"/>.
        /// Does not include keywords.
        /// </summary>
        /// <param name="destination">The span in which to write this instance's locale id as a span of characters.</param>
        /// <param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination"/>.</param>
        /// <returns><c>false</c> if <paramref name="destination"/> is not long enough; otherwise, <c>true</c>.</returns>
        public bool TryGetName(Span<char> destination, out int charsWritten)
        {
            ParseBaseName(HYPHEN);
            RemoveLeadingSeparator(HYPHEN);
            ReadOnlySpan<char> result = AsSpan(0);
            bool success = result.TryCopyTo(destination);
            charsWritten = success ? result.Length : 0;
            return success;
        }

        /// <summary>
        /// Copies the normalized base form of the locale id using hyphen as the
        /// separator to <paramref name="destination"/>. This is for compatibility with <see cref="System.Globalization.CultureInfo.Name"/>.
        /// Does not include keywords.
        /// </summary>
        /// <param name="destination">The span in which to write this instance's locale id as a span of characters.</param>
        /// <param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination"/>.</param>
        /// <exception cref="ArgumentException"><paramref name="destination"/> is too short.</exception>
        public void GetName(Span<char> destination, out int charsWritten)
        {
            ParseBaseName(HYPHEN);
            RemoveLeadingSeparator(HYPHEN);
            ReadOnlySpan<char> result = AsSpan(0);
            result.CopyTo(destination);
            charsWritten = result.Length;
        }
#endif
        /// <summary>
        /// Returns the normalized base form of the locale id using hyphen as the
        /// separator, for compatibility with <see cref="System.Globalization.CultureInfo.Name"/>.
        /// Does not include keywords.
        /// </summary>
        public string GetName()
        {
            ParseBaseName(HYPHEN);
            RemoveLeadingSeparator(HYPHEN);
            return GetString(0);
        }

#if FEATURE_SPAN
        /// <summary>
        /// Copies the normalized full form of the locale id to <paramref name="destination"/>.
        /// The full form includes keywords if they are present.
        /// </summary>
        /// <param name="destination">The span in which to write this instance's normalized full form of the locale id as a span of characters.</param>
        /// <param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination"/>.</param>
        /// <returns><c>false</c> if <paramref name="destination"/> is not long enough; otherwise, <c>true</c>.</returns>
        public bool TryGetFullName(Span<char> destination, out int charsWritten)
        {
            ParseBaseName();
            ParseKeywords();
            ReadOnlySpan<char> result = AsSpan(0);
            bool success = result.TryCopyTo(destination);
            charsWritten = success ? result.Length : 0;
            return success;
        }

        /// <summary>
        /// Copies the normalized full form of the locale id to <paramref name="destination"/>.
        /// The full form includes keywords if they are present.
        /// </summary>
        /// <param name="destination">The span in which to write this instance's normalized full form of the locale id as a span of characters.</param>
        /// <param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination"/>.</param>
        /// <exception cref="ArgumentException"><paramref name="destination"/> is too short.</exception>
        public void GetFullName(Span<char> destination, out int charsWritten)
        {
            ParseBaseName();
            ParseKeywords();
            ReadOnlySpan<char> result = AsSpan(0);
            result.CopyTo(destination);
            charsWritten = result.Length;
        }
#endif

        /// <summary>
        /// Returns the normalized full form of the locale id.  The full
        /// form includes keywords if they are present.
        /// </summary>
        public string GetFullName()
        {
            ParseBaseName();
            ParseKeywords();
            return GetString(0);
        }

        // keyword utilities

        /// <summary>
        /// If we have keywords, advance index to the start of the keywords and return true,
        /// otherwise return false.
        /// </summary>
        private bool SetToKeywordStart()
        {
            for (int i = index; i < id.Length; ++i)
            {
                if (id[i] == KEYWORD_SEPARATOR)
                {
                    if (canonicalize)
                    {
                        for (int j = ++i; j < id.Length; ++j)
                        { // increment i past separator for return
                            if (id[j] == KEYWORD_ASSIGN)
                            {
                                index = i;
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (++i < id.Length)
                        {
                            index = i;
                            return true;
                        }
                    }
                    break;
                }
            }
            return false;
        }

        private static bool IsDoneOrKeywordAssign(char c)
        {
            return c == DONE || c == KEYWORD_ASSIGN;
        }

        private static bool IsDoneOrItemSeparator(char c)
        {
            return c == DONE || c == ITEM_SEPARATOR;
        }

        private string GetKeyword()
        {
            int start = index;
            while (!IsDoneOrKeywordAssign(Next()))
            {
            }
            --index;
#if FEATURE_SPAN
            int bufferLength = index - start;
            Span<char> buffer = bufferLength <= 64 ? stackalloc char[bufferLength] : new char[bufferLength];
            int length = id.Slice(start, index - start).Trim().ToLowerInvariant(buffer);
            return buffer.Slice(0, length).ToString();
#else
            return AsciiUtil.ToLower(new string(id, start, index - start).Trim());
#endif
        }

        private string GetValue()
        {
            int start = index;
            while (!IsDoneOrItemSeparator(Next()))
            {
            }
            --index;
#if FEATURE_SPAN
            return id.Slice(start, index - start).Trim().ToString();
#else
            return new string(id, start, index - start).Trim(); // leave case alone
#endif
        }

        private static IComparer<string> KeyComparer { get; } = StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// Returns a map of the keywords and values, or null if there are none.
        /// </summary>
#if FEATURE_IREADONLYCOLLECTIONS
        public IReadOnlyDictionary<string, string> Keywords
#else
        public IDictionary<string, string> Keywords
#endif
            => J2N.Collections.Generic.Extensions.DictionaryExtensions.AsReadOnly(GetKeywords());

        private IDictionary<string, string> GetKeywords()
        { 
            if (keywords == null)
            {
                IDictionary<string, string> m = null;
                if (SetToKeywordStart())
                {
                    // trim spaces and convert to lower case, both keywords and values.
                    do
                    {
                        string key = GetKeyword();
                        if (key.Length == 0)
                        {
                            break;
                        }
                        char c = Next();
                        if (c != KEYWORD_ASSIGN)
                        {
                            // throw new IllegalArgumentException("key '" + key + "' missing a value.");
                            if (c == DONE)
                            {
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        string value = GetValue();
                        if (value.Length == 0)
                        {
                            // throw new IllegalArgumentException("key '" + key + "' missing a value.");
                            continue;
                        }
                        if (m == null)
                        {
                            m = new SortedDictionary<string, string>(KeyComparer);
                        }
                        else if (m.ContainsKey(key))
                        {
                            // throw new IllegalArgumentException("key '" + key + "' already has a value.");
                            continue;
                        }
                        m[key] = value;
                    } while (Next() == ITEM_SEPARATOR);
                }
                keywords = m ?? J2N.Collections.Generic.Extensions.DictionaryExtensions.AsReadOnly(new Dictionary<string, string>());
            }

            return keywords;
        }

        /// <summary>
        /// Parse the keywords and return start of the string in the buffer.
        /// </summary>
        private int ParseKeywords()
        {
            int oldBlen = buffer.Length;
            var m = GetKeywords();
            if (m.Count > 0)
            {
                bool first = true;
                foreach (var e in m)
                {
                    Append(first ? KEYWORD_SEPARATOR : ITEM_SEPARATOR);
                    first = false;
                    Append(e.Key);
                    Append(KEYWORD_ASSIGN);
                    Append(e.Value);
                }
                if (first == false)
                {
                    ++oldBlen;
                }
            }
            return oldBlen;
        }

        /// <summary>
        /// Returns the value for the named keyword, or null if the keyword is not
        /// present.
        /// </summary>
        public string GetKeywordValue(string keywordName)
        {
            var m = GetKeywords();
            return m.Count == 0 ? null : m.Get(AsciiUtil.ToLower(keywordName.Trim()));
        }

        /// <summary>
        /// Set the keyword value only if it is not already set to something else.
        /// </summary>
        public void DefaultKeywordValue(string keywordName, string value)
        {
            SetKeywordValue(keywordName, value, false);
        }

        /// <summary>
        /// Set the value for the named keyword, or unset it if <paramref name="value"/> is null.  If
        /// <paramref name="keywordName"/> itself is null, unset all keywords.  If <paramref name="keywordName"/> is not null,
        /// <paramref name="value"/> must not be null.
        /// </summary>
        public void SetKeywordValue(string keywordName, string value)
        {
            SetKeywordValue(keywordName, value, true);
        }

        /// <summary>
        /// Set the value for the named keyword, or unset it if <paramref name="value"/> is null.  If
        /// <paramref name="keywordName"/> itself is null, unset all keywords.  If <paramref name="keywordName"/> is not null,
        /// <paramref name="value"/> must not be null.  If reset is true, ignore any previous value for
        /// the keyword, otherwise do not change the keyword (including removal of
        /// one or all keywords).
        /// </summary>
        private void SetKeywordValue(string keywordName, string value, bool reset)
        {
            if (keywordName == null)
            {
                if (reset)
                {
                    // force new map, ignore value
                    keywords = new Dictionary<string, string>();
                }
            }
            else
            {
                keywordName = AsciiUtil.ToLower(keywordName.Trim());
                if (keywordName.Length == 0)
                {
                    throw new ArgumentException("keyword must not be empty");
                }
                if (value != null)
                {
                    value = value.Trim();
                    if (value.Length == 0)
                    {
                        throw new ArgumentException("value must not be empty");
                    }
                }
                var m = GetKeywords();
                if (m.Count == 0)
                { // it is EMPTY_MAP
                    if (value != null)
                    {
                        // force new map
                        keywords = new SortedDictionary<string, string>(KeyComparer)
                        {
                            [keywordName] = value.Trim()
                        };
                    }
                }
                else
                {
                    if (reset || !m.ContainsKey(keywordName))
                    {
                        if (value != null)
                        {
                            m[keywordName] = value;
                        }
                        else
                        {
                            m.Remove(keywordName);
                            if (m.Count == 0)
                            {
                                // force new map
                                keywords = J2N.Collections.Generic.Extensions.DictionaryExtensions.AsReadOnly(new Dictionary<string, string>());
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            // Just for compatibility with ValueStringBuilder (so it can return memory to the array pool)
#if FEATURE_SPAN
            buffer.Dispose();
#endif
        }
    }
}
