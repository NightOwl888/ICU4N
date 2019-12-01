using ICU4N.Impl;
using ICU4N.Util;
using J2N.Text;
using System;
using System.Globalization;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// The <see cref="FilteredBreakIteratorBuilder"/> is used to modify the behavior of a <see cref="BreakIterator"/>
    /// by constructing a new <see cref="BreakIterator"/> which suppresses certain segment boundaries.
    /// See  http://www.unicode.org/reports/tr35/tr35-general.html#Segmentation_Exceptions .
    /// For example, a typical English Sentence Break Iterator would break on the space
    /// in the string "Mr. Smith" (resulting in two segments),
    /// but with "Mr." as an exception, a filtered break iterator
    /// would consider the string "Mr. Smith" to be a single segment.
    /// <para/>
    /// This class is not intended for public subclassing.
    /// </summary>
    /// <draft>ICU 60</draft>
    /// <provisional>This API might change or be removed in a future release.</provisional>
    public abstract class FilteredBreakIteratorBuilder
    {
        /// <summary>
        /// Construct a <see cref="FilteredBreakIteratorBuilder"/> based on sentence break exception rules in a locale.
        /// The rules are taken from CLDR exception data for the locale,
        /// see http://www.unicode.org/reports/tr35/tr35-general.html#Segmentation_Exceptions
        /// This is the equivalent of calling createInstance(UErrorCode&amp;)
        /// and then repeatedly calling addNoBreakAfter(...) with the contents
        /// of the CLDR exception data.
        /// </summary>
        /// <param name="where">The locale.</param>
        /// <returns>The new builder.</returns>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static FilteredBreakIteratorBuilder GetInstance(CultureInfo where)
        {
            return new SimpleFilteredSentenceBreakIteratorBuilder(where);
        }

        /// <summary>
        /// Construct a <see cref="FilteredBreakIteratorBuilder"/> based on sentence break exception rules in a locale.
        /// The rules are taken from CLDR exception data for the locale,
        /// see http://www.unicode.org/reports/tr35/tr35-general.html#Segmentation_Exceptions
        /// This is the equivalent of calling createInstance(UErrorCode&amp;)
        /// and then repeatedly calling addNoBreakAfter(...) with the contents
        /// of the CLDR exception data.
        /// </summary>
        /// <param name="where">The locale.</param>
        /// <returns>The new builder.</returns>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static FilteredBreakIteratorBuilder GetInstance(ULocale where)
        {
            return new SimpleFilteredSentenceBreakIteratorBuilder(where);
        }

        /// <summary>
        /// Construct an empty <see cref="FilteredBreakIteratorBuilder"/>.
        /// In this state, it will not suppress any segment boundaries.
        /// </summary>
        /// <returns>The new builder.</returns>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static FilteredBreakIteratorBuilder GetEmptyInstance()
        {
            return new SimpleFilteredSentenceBreakIteratorBuilder();
        }

        // ICU4N TODO: API Generate APIs

        public virtual bool SuppressBreakAfter(string str) // ICU4N specific
        {
            return SuppressBreakAfter(str.ToCharSequence());
        }

        public virtual bool SuppressBreakAfter(StringBuilder str) // ICU4N specific
        {
            return SuppressBreakAfter(str.ToCharSequence());
        }

        public virtual bool SuppressBreakAfter(char[] str) // ICU4N specific
        {
            return SuppressBreakAfter(str.ToCharSequence());
        }

        /// <summary>
        /// Suppress a certain string from being the end of a segment.
        /// For example, suppressing "Mr.", then segments ending in "Mr." will not be returned
        /// by the iterator.
        /// </summary>
        /// <param name="str">The string to suppress, such as "Mr."</param>
        /// <returns>true if the string was not present and now added,
        /// false if the call was a no-op because the string was already being suppressed.</returns>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        internal abstract bool SuppressBreakAfter(ICharSequence str);



        public virtual bool UnsuppressBreakAfter(string str) // ICU4N specific
        {
            return UnsuppressBreakAfter(str.ToCharSequence());
        }

        public virtual bool UnsuppressBreakAfter(StringBuilder str) // ICU4N specific
        {
            return UnsuppressBreakAfter(str.ToCharSequence());
        }

        public virtual bool UnsuppressBreakAfter(char[] str) // ICU4N specific
        {
            return UnsuppressBreakAfter(str.ToCharSequence());
        }

        /// <summary>
        /// Stop suppressing a certain string from being the end of the segment.
        /// This function does not create any new segment boundaries, but only serves to un-do
        /// the effect of earlier calls to <see cref="SuppressBreakAfter(ICharSequence)"/>, or to un-do the effect of
        /// locale data which may be suppressing certain strings.
        /// </summary>
        /// <param name="str">The str the string to unsuppress, such as "Mr."</param>
        /// <returns>true if the string was present and now removed,
        /// false if the call was a no-op because the string was not being suppressed.</returns>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        internal abstract bool UnsuppressBreakAfter(ICharSequence str);

        /// <summary>
        /// Wrap (adopt) an existing break iterator in a new filtered instance.
        /// Note that the <paramref name="wrappedBreakIterator"/> is adopted by the new <see cref="BreakIterator"/>
        /// and should no longer be used by the caller.
        /// The <see cref="FilteredBreakIteratorBuilder"/> may be reused.
        /// </summary>
        /// <param name="wrappedBreakIterator">The break iterator to wrap.</param>
        /// <returns>The new <see cref="BreakIterator"/>.</returns>
        /// <draft>ICU 60</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public abstract BreakIterator WrapIteratorWithFilter(BreakIterator wrappedBreakIterator);

        /// <summary>
        /// For subclass use.
        /// </summary>
        /// <internal/>
        [Obsolete("internal to ICU")]
        protected FilteredBreakIteratorBuilder()
        {
        }
    }
}
