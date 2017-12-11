using ICU4N.Impl;
using ICU4N.Support.Text;
using ICU4N.Util;
using System;
using System.Globalization;
using System.Text;

namespace ICU4N.Text
{
    public abstract class BreakIterator
#if FEATURE_CLONEABLE
        : ICloneable
#endif
    {
        private static readonly bool DEBUG = ICUDebug.Enabled("breakiterator");

        private static readonly object syncLock = new object();

        /**
         * Default constructor.  There is no state that is carried by this abstract
         * base class.
         * @stable ICU 2.0
         */
        protected BreakIterator()
        {
        }

        /**
         * Clone method.  Creates another BreakIterator with the same behavior and
         * current state as this one.
         * @return The clone.
         * @stable ICU 2.0
         */
        public virtual object Clone()
        {
            return base.MemberwiseClone();
        }

        /**
         * DONE is returned by previous() and next() after all valid
         * boundaries have been returned.
         * @stable ICU 2.0
         */
        public const int DONE = -1;

        /**
         * Set the iterator to the first boundary position.  This is always the beginning
         * index of the text this iterator iterates over.  For example, if
         * the iterator iterates over a whole string, this function will
         * always return 0.
         * @return The character offset of the beginning of the stretch of text
         * being broken.
         * @stable ICU 2.0
         */
        public abstract int First();

        /**
         * Set the iterator to the last boundary position.  This is always the "past-the-end"
         * index of the text this iterator iterates over.  For example, if the
         * iterator iterates over a whole string (call it "text"), this function
         * will always return text.length().
         * @return The character offset of the end of the stretch of text
         * being broken.
         * @stable ICU 2.0
         */
        public abstract int Last();

        /**
         * Move the iterator by the specified number of steps in the text.
         * A positive number moves the iterator forward; a negative number
         * moves the iterator backwards. If this causes the iterator
         * to move off either end of the text, this function returns DONE;
         * otherwise, this function returns the position of the appropriate
         * boundary.  Calling this function is equivalent to calling next() or
         * previous() n times.
         * @param n The number of boundaries to advance over (if positive, moves
         * forward; if negative, moves backwards).
         * @return The position of the boundary n boundaries from the current
         * iteration position, or DONE if moving n boundaries causes the iterator
         * to advance off either end of the text.
         * @stable ICU 2.0
         */
        public abstract int Next(int n);

        /**
         * Advances the iterator forward one boundary.  The current iteration
         * position is updated to point to the next boundary position after the
         * current position, and this is also the value that is returned.  If
         * the current position is equal to the value returned by last(), or to
         * DONE, this function returns DONE and sets the current position to
         * DONE.
         * @return The position of the first boundary position following the
         * iteration position.
         * @stable ICU 2.0
         */
        public abstract int Next();

        /**
         * Move the iterator backward one boundary.  The current iteration
         * position is updated to point to the last boundary position before
         * the current position, and this is also the value that is returned.  If
         * the current position is equal to the value returned by first(), or to
         * DONE, this function returns DONE and sets the current position to
         * DONE.
         * @return The position of the last boundary position preceding the
         * iteration position.
         * @stable ICU 2.0
         */
        public abstract int Previous();

        /**
         * Sets the iterator's current iteration position to be the first
         * boundary position following the specified position.  (Whether the
         * specified position is itself a boundary position or not doesn't
         * matter-- this function always moves the iteration position to the
         * first boundary after the specified position.)  If the specified
         * position is the past-the-end position, returns DONE.
         * @param offset The character position to start searching from.
         * @return The position of the first boundary position following
         * "offset" (whether or not "offset" itself is a boundary position),
         * or DONE if "offset" is the past-the-end offset.
         * @stable ICU 2.0
         */
        public abstract int Following(int offset);

        /**
         * Sets the iterator's current iteration position to be the last
         * boundary position preceding the specified position.  (Whether the
         * specified position is itself a boundary position or not doesn't
         * matter-- this function always moves the iteration position to the
         * last boundary before the specified position.)  If the specified
         * position is the starting position, returns DONE.
         * @param offset The character position to start searching from.
         * @return The position of the last boundary position preceding
         * "offset" (whether of not "offset" itself is a boundary position),
         * or DONE if "offset" is the starting offset of the iterator.
         * @stable ICU 2.0
         */
        public virtual int Preceding(int offset)
        {
            // NOTE:  This implementation is here solely because we can't add new
            // abstract methods to an existing class.  There is almost ALWAYS a
            // better, faster way to do this.
            int pos = Following(offset);
            while (pos >= offset && pos != DONE)
                pos = Previous();
            return pos;
        }

        /**
         * Return true if the specified position is a boundary position.  If the
         * function returns true, the current iteration position is set to the
         * specified position; if the function returns false, the current
         * iteration position is set as though following() had been called.
         * @param offset the offset to check.
         * @return True if "offset" is a boundary position.
         * @stable ICU 2.0
         */
        public virtual bool IsBoundary(int offset)
        {
            // Again, this is the default implementation, which is provided solely because
            // we couldn't add a new abstract method to an existing class.  The real
            // implementations will usually need to do a little more work.
            if (offset == 0)
            {
                return true;
            }
            else
                return Following(offset - 1) == offset;
        }

        /**
         * Return the iterator's current position.
         * @return The iterator's current position.
         * @stable ICU 2.0
         */
        public abstract int Current { get; }


        /**
         * Tag value for "words" that do not fit into any of other categories.
         * Includes spaces and most punctuation.
         * @stable ICU 53
         */
        public const int WORD_NONE = 0;

        /**
         * Upper bound for tags for uncategorized words.
         * @stable ICU 53
         */
        public const int WORD_NONE_LIMIT = 100;

        /**
         * Tag value for words that appear to be numbers, lower limit.
         * @stable ICU 53
         */
        public const int WORD_NUMBER = 100;

        /**
         * Tag value for words that appear to be numbers, upper limit.
         * @stable ICU 53
         */
        public const int WORD_NUMBER_LIMIT = 200;

        /**
         * Tag value for words that contain letters, excluding
         * hiragana, katakana or ideographic characters, lower limit.
         * @stable ICU 53
         */
        public const int WORD_LETTER = 200;

        /**
         * Tag value for words containing letters, upper limit
         * @stable ICU 53
         */
        public const int WORD_LETTER_LIMIT = 300;

        /**
         * Tag value for words containing kana characters, lower limit
         * @stable ICU 53
         */
        public const int WORD_KANA = 300;

        /**
         * Tag value for words containing kana characters, upper limit
         * @stable ICU 53
         */
        public const int WORD_KANA_LIMIT = 400;

        /**
         * Tag value for words containing ideographic characters, lower limit
         * @stable ICU 53
         */
        public const int WORD_IDEO = 400;

        /**
         * Tag value for words containing ideographic characters, upper limit
         * @stable ICU 53
         */
        public const int WORD_IDEO_LIMIT = 500;

        /**
         * For RuleBasedBreakIterators, return the status tag from the
         * break rule that determined the most recently
         * returned break position.
         * <p>
         * For break iterator types that do not support a rule status,
         * a default value of 0 is returned.
         * <p>
         * @return The status from the break rule that determined the most recently
         *         returned break position.
         *
         * @stable ICU 52
         */

        public virtual int RuleStatus
        {
            get { return 0; }
        }

        /**
         * For RuleBasedBreakIterators, get the status (tag) values from the break rule(s)
         * that determined the most recently returned break position.
         * <p>
         * For break iterator types that do not support rule status,
         * no values are returned.
         * <p>
         * If the size  of the output array is insufficient to hold the data,
         *  the output will be truncated to the available length.  No exception
         *  will be thrown.
         *
         * @param fillInArray an array to be filled in with the status values.
         * @return          The number of rule status values from rules that determined
         *                  the most recent boundary returned by the break iterator.
         *                  In the event that the array is too small, the return value
         *                  is the total number of status values that were available,
         *                  not the reduced number that were actually returned.
         * @stable ICU 52
         */
        public virtual int GetRuleStatusVec(int[] fillInArray)
        {
            if (fillInArray != null && fillInArray.Length > 0)
            {
                fillInArray[0] = 0;
            }
            return 1;
        }

        /**
         * Returns a CharacterIterator over the text being analyzed.
         * For at least some subclasses of BreakIterator, this is a reference
         * to the <b>actual iterator being used</b> by the BreakIterator,
         * and therefore, this function's return value should be treated as
         * <tt>const</tt>.  No guarantees are made about the current position
         * of this iterator when it is returned.  If you need to move that
         * position to examine the text, clone this function's return value first.
         * @return A CharacterIterator over the text being analyzed.
         * @stable ICU 2.0
         */
        public abstract CharacterIterator GetText();

        /**
         * Sets the iterator to analyze a new piece of text.  The new
         * piece of text is passed in as a string, and the current
         * iteration position is reset to the beginning of the string.
         * (The old text is dropped.)
         * @param newText A string containing the text to analyze with
         * this BreakIterator.
         * @stable ICU 2.0
         */
        public virtual void SetText(string newText)
        {
            SetText(new StringCharacterIterator(newText));
        }

        // ICU4N specific
        public virtual void SetText(StringBuilder newText)
        {
            SetText(new StringCharacterIterator(newText.ToString())); // ICU4N TODO: make a StringBuilderCharacterIterator
        }

        // ICU4N specific
        public virtual void SetText(char[] newText)
        {
            SetText(new StringCharacterIterator(new string(newText))); // ICU4N TODO: make a CharArrayCharacterIterator
        }

        /**
         * Sets the iterator to analyze a new piece of text.  The new
         * piece of text is passed in as a CharSequence, and the current
         * iteration position is reset to the beginning of the text.
         * (The old text is dropped.)
         * @param newText A CharSequence containing the text to analyze with
         * this BreakIterator.
         * @draft ICU 60
         */
        internal virtual void SetText(ICharSequence newText)
        {
            SetText(new CSCharacterIterator(newText));
        }

        /**
         * Sets the iterator to analyze a new piece of text.  The
         * BreakIterator is passed a CharacterIterator through which
         * it will access the text itself.  The current iteration
         * position is reset to the CharacterIterator's start index.
         * (The old iterator is dropped.)
         * @param newText A CharacterIterator referring to the text
         * to analyze with this BreakIterator (the iterator's current
         * position is ignored, but its other state is significant).
         * @stable ICU 2.0
         */
        public abstract void SetText(CharacterIterator newText);

        /**
         * {@icu}
         * @stable ICU 2.4
         */
        public const int KIND_CHARACTER = 0;
        /**
         * {@icu}
         * @stable ICU 2.4
         */
        public const int KIND_WORD = 1;
        /**
         * {@icu}
         * @stable ICU 2.4
         */
        public const int KIND_LINE = 2;
        /**
         * {@icu}
         * @stable ICU 2.4
         */
        public const int KIND_SENTENCE = 3;
        /**
         * {@icu}
         * @stable ICU 2.4
         */
        public const int KIND_TITLE = 4;

        /**
         * @since ICU 2.8
         */
        private const int KIND_COUNT = 5;

        private static readonly CacheValue<BreakIteratorCache>[] iterCache = new CacheValue<BreakIteratorCache>[5];

        /**
         * Returns a new instance of BreakIterator that locates word boundaries.
         * This function assumes that the text being analyzed is in the default
         * locale's language.
         * @return An instance of BreakIterator that locates word boundaries.
         * @stable ICU 2.0
         */
        public static BreakIterator GetWordInstance()
        {
            return GetWordInstance(ULocale.GetDefault());
        }

        /**
         * Returns a new instance of BreakIterator that locates word boundaries.
         * @param where A locale specifying the language of the text to be
         * analyzed.
         * @return An instance of BreakIterator that locates word boundaries.
         * @throws NullPointerException if <code>where</code> is null.
         * @stable ICU 2.0
         */
        public static BreakIterator GetWordInstance(CultureInfo where)
        {
            return GetBreakInstance(ULocale.ForLocale(where), KIND_WORD);
        }

        /**
         * {@icu} Returns a new instance of BreakIterator that locates word boundaries.
         * @param where A locale specifying the language of the text to be
         * analyzed.
         * @return An instance of BreakIterator that locates word boundaries.
         * @throws NullPointerException if <code>where</code> is null.
         * @stable ICU 3.2
         */
        public static BreakIterator GetWordInstance(ULocale where)
        {
            return GetBreakInstance(where, KIND_WORD);
        }

        /**
         * Returns a new instance of BreakIterator that locates legal line-
         * wrapping positions.  This function assumes the text being broken
         * is in the default locale's language.
         * @return A new instance of BreakIterator that locates legal
         * line-wrapping positions.
         * @stable ICU 2.0
         */
        public static BreakIterator GetLineInstance()
        {
            return GetLineInstance(ULocale.GetDefault());
        }

        /**
         * Returns a new instance of BreakIterator that locates legal line-
         * wrapping positions.
         * @param where A Locale specifying the language of the text being broken.
         * @return A new instance of BreakIterator that locates legal
         * line-wrapping positions.
         * @throws NullPointerException if <code>where</code> is null.
         * @stable ICU 2.0
         */
        public static BreakIterator GetLineInstance(CultureInfo where)
        {
            return GetBreakInstance(ULocale.ForLocale(where), KIND_LINE);
        }

        /**
         * {@icu} Returns a new instance of BreakIterator that locates legal line-
         * wrapping positions.
         * @param where A Locale specifying the language of the text being broken.
         * @return A new instance of BreakIterator that locates legal
         * line-wrapping positions.
         * @throws NullPointerException if <code>where</code> is null.
         * @stable ICU 3.2
         */
        public static BreakIterator GetLineInstance(ULocale where)
        {
            return GetBreakInstance(where, KIND_LINE);
        }

        /**
         * Returns a new instance of BreakIterator that locates logical-character
         * boundaries.  This function assumes that the text being analyzed is
         * in the default locale's language.
         * @return A new instance of BreakIterator that locates logical-character
         * boundaries.
         * @stable ICU 2.0
         */
        public static BreakIterator GetCharacterInstance()
        {
            return GetCharacterInstance(ULocale.GetDefault());
        }

        /**
         * Returns a new instance of BreakIterator that locates logical-character
         * boundaries.
         * @param where A Locale specifying the language of the text being analyzed.
         * @return A new instance of BreakIterator that locates logical-character
         * boundaries.
         * @throws NullPointerException if <code>where</code> is null.
         * @stable ICU 2.0
         */
        public static BreakIterator GetCharacterInstance(CultureInfo where)
        {
            return GetBreakInstance(ULocale.ForLocale(where), KIND_CHARACTER);
        }

        /**
         * {@icu} Returns a new instance of BreakIterator that locates logical-character
         * boundaries.
         * @param where A Locale specifying the language of the text being analyzed.
         * @return A new instance of BreakIterator that locates logical-character
         * boundaries.
         * @throws NullPointerException if <code>where</code> is null.
         * @stable ICU 3.2
         */
        public static BreakIterator GetCharacterInstance(ULocale where)
        {
            return GetBreakInstance(where, KIND_CHARACTER);
        }

        /**
         * Returns a new instance of BreakIterator that locates sentence boundaries.
         * This function assumes the text being analyzed is in the default locale's
         * language.
         * @return A new instance of BreakIterator that locates sentence boundaries.
         * @stable ICU 2.0
         */
        public static BreakIterator GetSentenceInstance()
        {
            return GetSentenceInstance(ULocale.GetDefault());
        }

        /**
         * Returns a new instance of BreakIterator that locates sentence boundaries.
         * @param where A Locale specifying the language of the text being analyzed.
         * @return A new instance of BreakIterator that locates sentence boundaries.
         * @throws NullPointerException if <code>where</code> is null.
         * @stable ICU 2.0
         */
        public static BreakIterator GetSentenceInstance(CultureInfo where)
        {
            return GetBreakInstance(ULocale.ForLocale(where), KIND_SENTENCE);
        }

        /**
         * {@icu} Returns a new instance of BreakIterator that locates sentence boundaries.
         * @param where A Locale specifying the language of the text being analyzed.
         * @return A new instance of BreakIterator that locates sentence boundaries.
         * @throws NullPointerException if <code>where</code> is null.
         * @stable ICU 3.2
         */
        public static BreakIterator GetSentenceInstance(ULocale where)
        {
            return GetBreakInstance(where, KIND_SENTENCE);
        }

        /**
         * {@icu} Returns a new instance of BreakIterator that locates title boundaries.
         * This function assumes the text being analyzed is in the default locale's
         * language. The iterator returned locates title boundaries as described for
         * Unicode 3.2 only. For Unicode 4.0 and above title boundary iteration,
         * please use a word boundary iterator. {@link #getWordInstance}
         * @return A new instance of BreakIterator that locates title boundaries.
         * @stable ICU 2.0
         */
        public static BreakIterator GetTitleInstance()
        {
            return GetTitleInstance(ULocale.GetDefault());
        }

        /**
         * {@icu} Returns a new instance of BreakIterator that locates title boundaries.
         * The iterator returned locates title boundaries as described for
         * Unicode 3.2 only. For Unicode 4.0 and above title boundary iteration,
         * please use Word Boundary iterator.{@link #getWordInstance}
         * @param where A Locale specifying the language of the text being analyzed.
         * @return A new instance of BreakIterator that locates title boundaries.
         * @throws NullPointerException if <code>where</code> is null.
         * @stable ICU 2.0
         */
        public static BreakIterator GetTitleInstance(CultureInfo where)
        {
            return GetBreakInstance(ULocale.ForLocale(where), KIND_TITLE);
        }

        /**
         * {@icu} Returns a new instance of BreakIterator that locates title boundaries.
         * The iterator returned locates title boundaries as described for
         * Unicode 3.2 only. For Unicode 4.0 and above title boundary iteration,
         * please use Word Boundary iterator.{@link #getWordInstance}
         * @param where A Locale specifying the language of the text being analyzed.
         * @return A new instance of BreakIterator that locates title boundaries.
         * @throws NullPointerException if <code>where</code> is null.
         * @stable ICU 3.2
s     */
        public static BreakIterator GetTitleInstance(ULocale where)
        {
            return GetBreakInstance(where, KIND_TITLE);
        }

        /**
         * {@icu} Registers a new break iterator of the indicated kind, to use in the given
         * locale.  Clones of the iterator will be returned if a request for a break iterator
         * of the given kind matches or falls back to this locale.
         *
         * <p>Because ICU may choose to cache BreakIterator objects internally, this must
         * be called at application startup, prior to any calls to
         * BreakIterator.getInstance to avoid undefined behavior.
         *
         * @param iter the BreakIterator instance to adopt.
         * @param locale the Locale for which this instance is to be registered
         * @param kind the type of iterator for which this instance is to be registered
         * @return a registry key that can be used to unregister this instance
         * @stable ICU 2.4
         */
        public static object RegisterInstance(BreakIterator iter, CultureInfo locale, int kind)
        {
            return RegisterInstance(iter, ULocale.ForLocale(locale), kind);
        }

        /**
         * {@icu} Registers a new break iterator of the indicated kind, to use in the given
         * locale.  Clones of the iterator will be returned if a request for a break iterator
         * of the given kind matches or falls back to this locale.
         *
         * <p>Because ICU may choose to cache BreakIterator objects internally, this must
         * be called at application startup, prior to any calls to
         * BreakIterator.getInstance to avoid undefined behavior.
         *
         * @param iter the BreakIterator instance to adopt.
         * @param locale the Locale for which this instance is to be registered
         * @param kind the type of iterator for which this instance is to be registered
         * @return a registry key that can be used to unregister this instance
         * @stable ICU 3.2
         */
        public static object RegisterInstance(BreakIterator iter, ULocale locale, int kind)
        {
            // If the registered object matches the one in the cache, then
            // flush the cached object.
            if (iterCache[kind] != null)
            {
                BreakIteratorCache cache = (BreakIteratorCache)iterCache[kind].Get();
                if (cache != null)
                {
                    if (cache.GetLocale().Equals(locale))
                    {
                        iterCache[kind] = null;
                    }
                }
            }
            return GetShim().RegisterInstance(iter, locale, kind);
        }

        /**
         * {@icu} Unregisters a previously-registered BreakIterator using the key returned
         * from the register call.  Key becomes invalid after this call and should not be used
         * again.
         * @param key the registry key returned by a previous call to registerInstance
         * @return true if the iterator for the key was successfully unregistered
         * @stable ICU 2.4
         */
        public static bool Unregister(object key)
        {
            if (key == null)
            {
                throw new ArgumentException("registry key must not be null");
            }
            // TODO: we don't do code coverage for the following lines
            // because in getBreakInstance we always instantiate the shim,
            // and test execution is such that we always instantiate a
            // breakiterator before we get to the break iterator tests.
            // this is for modularization, and we could remove the
            // dependencies in getBreakInstance by rewriting part of the
            // LocaleData code, or perhaps by accepting it into the
            // module.
            ///CLOVER:OFF
            if (shim != null)
            {
                // Unfortunately, we don't know what is being unregistered
                // -- what `kind' and what locale -- so we flush all
                // caches.  This is safe but inefficient if people are
                // actively registering and unregistering.
                for (int kind = 0; kind < KIND_COUNT; ++kind)
                {
                    iterCache[kind] = null;
                }
                return shim.Unregister(key);
            }
            return false;
            ///CLOVER:ON
        }

        // end of registration

        /**
         * Returns a particular kind of BreakIterator for a locale.
         * Avoids writing a switch statement with getXYZInstance(where) calls.
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public static BreakIterator GetBreakInstance(ULocale where, int kind)
        {
            if (where == null)
            {
                throw new ArgumentNullException(nameof(where), "Specified locale is null");
            }
            if (iterCache[kind] != null)
            {
                BreakIteratorCache cache2 = (BreakIteratorCache)iterCache[kind].Get();
                if (cache2 != null)
                {
                    if (cache2.GetLocale().Equals(where))
                    {
                        return cache2.CreateBreakInstance();
                    }
                }
            }

            // sigh, all to avoid linking in ICULocaleData...
            BreakIterator result = GetShim().CreateBreakIterator(where, kind);

            BreakIteratorCache cache = new BreakIteratorCache(where, result);
            iterCache[kind] = CacheValue<BreakIteratorCache>.GetInstance(cache);
            if (result is RuleBasedBreakIterator)
            {
                RuleBasedBreakIterator rbbi = (RuleBasedBreakIterator)result;
                rbbi.BreakType = kind;
            }

            return result;
        }


        /**
         * Returns a list of locales for which BreakIterators can be used.
         * @return An array of Locales.  All of the locales in the array can
         * be used when creating a BreakIterator.
         * @stable ICU 2.6
         */
        public static CultureInfo[] GetAvailableLocales()
        {
            lock (syncLock)
            {
                // to avoid linking ICULocaleData
                return GetShim().GetAvailableLocales();
            }
        }

        /**
         * {@icu} Returns a list of locales for which BreakIterators can be used.
         * @return An array of Locales.  All of the locales in the array can
         * be used when creating a BreakIterator.
         * @draft ICU 3.2 (retain)
         * @provisional This API might change or be removed in a future release.
         */
        public static ULocale[] GetAvailableULocales()
        {
            lock (syncLock)
            {
                // to avoid linking ICULocaleData
                return GetShim().GetAvailableULocales();
            }
        }

        private sealed class BreakIteratorCache
        {

            private BreakIterator iter;
            private ULocale where;

            internal BreakIteratorCache(ULocale where, BreakIterator iter)
            {
                this.where = where;
                this.iter = (BreakIterator)iter.Clone();
            }

            internal ULocale GetLocale()
            {
                return where;
            }

            internal BreakIterator CreateBreakInstance()
            {
                return (BreakIterator)iter.Clone();
            }
        }

        internal abstract class BreakIteratorServiceShim
        {
            public abstract object RegisterInstance(BreakIterator iter, ULocale l, int k);
            public abstract bool Unregister(Object key);
            public abstract CultureInfo[] GetAvailableLocales();
            public abstract ULocale[] GetAvailableULocales();
            public abstract BreakIterator CreateBreakIterator(ULocale l, int k);
        }

        private static BreakIteratorServiceShim shim;
        private static BreakIteratorServiceShim GetShim()
        {
            // Note: this instantiation is safe on loose-memory-model configurations
            // despite lack of synchronization, since the shim instance has no state--
            // it's all in the class init.  The worst problem is we might instantiate
            // two shim instances, but they'll share the same state so that's ok.
            if (shim == null)
            {
                try
                {
                    //Class <?> cls = Class.forName("com.ibm.icu.text.BreakIteratorFactory");
                    Type cls = Type.GetType("ICU4N.Text.BreakIteratorFactory, ICU4N"); // ICU4N TODO: Set BreakIteratorFactory statically on BreakIterator abstract class.
                    shim = (BreakIteratorServiceShim)Activator.CreateInstance(cls);
                }
                catch (TypeInitializationException e) // ICU4N TODO: Check exception type
                {
                    throw e;
                }
                catch (Exception e)
                {
                    ///CLOVER:OFF
                    //if (DEBUG)
                    //{
                    //    e.printStackTrace();
                    //}
                    throw new Exception(e.ToString(), e);
                    ///CLOVER:ON
                }
            }
            return shim;
        }

        // -------- BEGIN ULocale boilerplate --------

        /**
         * {@icu} Returns the locale that was used to create this object, or null.
         * This may may differ from the locale requested at the time of
         * this object's creation.  For example, if an object is created
         * for locale <tt>en_US_CALIFORNIA</tt>, the actual data may be
         * drawn from <tt>en</tt> (the <i>actual</i> locale), and
         * <tt>en_US</tt> may be the most specific locale that exists (the
         * <i>valid</i> locale).
         *
         * <p>Note: The <i>actual</i> locale is returned correctly, but the <i>valid</i>
         * locale is not, in most cases.
         * @param type type of information requested, either {@link
         * com.ibm.icu.util.ULocale#VALID_LOCALE} or {@link
         * com.ibm.icu.util.ULocale#ACTUAL_LOCALE}.
         * @return the information specified by <i>type</i>, or null if
         * this object was not constructed from locale data.
         * @see com.ibm.icu.util.ULocale
         * @see com.ibm.icu.util.ULocale#VALID_LOCALE
         * @see com.ibm.icu.util.ULocale#ACTUAL_LOCALE
         * @draft ICU 2.8 (retain)
         * @provisional This API might change or be removed in a future release.
         */
        public ULocale GetLocale(ULocale.Type type)
        {
            return type == ULocale.ACTUAL_LOCALE ?
                this.actualLocale : this.validLocale;
        }

        /**
         * Set information about the locales that were used to create this
         * object.  If the object was not constructed from locale data,
         * both arguments should be set to null.  Otherwise, neither
         * should be null.  The actual locale must be at the same level or
         * less specific than the valid locale.  This method is intended
         * for use by factories or other entities that create objects of
         * this class.
         * @param valid the most specific locale containing any resource
         * data, or null
         * @param actual the locale containing data used to construct this
         * object, or null
         * @see com.ibm.icu.util.ULocale
         * @see com.ibm.icu.util.ULocale#VALID_LOCALE
         * @see com.ibm.icu.util.ULocale#ACTUAL_LOCALE
         */
        internal void SetLocale(ULocale valid, ULocale actual)
        {
            // Change the following to an assertion later
            if ((valid == null) != (actual == null))
            {
                ///CLOVER:OFF
                throw new ArgumentException();
                ///CLOVER:ON
            }
            // Another check we could do is that the actual locale is at
            // the same level or less specific than the valid locale.
            this.validLocale = valid;
            this.actualLocale = actual;
        }

        /**
         * The most specific locale containing any resource data, or null.
         * @see com.ibm.icu.util.ULocale
         */
        private ULocale validLocale;

        /**
         * The locale containing data used to construct this object, or
         * null.
         * @see com.ibm.icu.util.ULocale
         */
        private ULocale actualLocale;

        // -------- END ULocale boilerplate --------
    }
}
