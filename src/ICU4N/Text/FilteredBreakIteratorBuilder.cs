using ICU4N.Impl;
using ICU4N.Util;
using System;
using System.Globalization;

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
    public abstract partial class FilteredBreakIteratorBuilder
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

        // ICU4N specific - moved SuppressBreakAfter and UnsuppressBreakAfter methods to FilteredBreakIteratorBuilderExtension.tt

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
        internal FilteredBreakIteratorBuilder() // ICU4N specific - marked internal instead of protected, since the functionality is obsolete
        {
        }
    }
}
