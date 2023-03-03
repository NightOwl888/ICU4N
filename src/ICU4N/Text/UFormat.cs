using ICU4N.Globalization;
using ICU4N.Support.Text;
using System;

namespace ICU4N.Text
{
    /// <summary>
    /// An abstract class that extends <see cref="Formatter"/> to provide
    /// additional ICU protocol, specifically, the <see cref="ActualCulture"/>
    /// and <see cref="ValidCulture"/> API. All ICU format classes are
    /// subclasses of this class.
    /// </summary>
    /// <seealso cref="UCultureInfo"/>
    /// <author>weiv</author>
    /// <author>Alan Liu</author>
    /// <stable>ICU 2.8</stable>
#if FEATURE_LEGACY_NUMBER_FORMAT
    public
#else
    internal
#endif
    abstract class UFormat : Formatter // ICU4N: Marked internal until formatters are refactored // ICU4N TODO: API - Add IFormatProvider, ICustomFormatter. See: https://stackoverflow.com/a/35577288
    {
        // jdk1.4.2 serialver
        //private static readonly long serialVersionUID = -4964390515840164416L;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        public UFormat() { }

        // -------- BEGIN UCultureInfo boilerplate --------

        /// <summary>
        /// <icu/> Gets the locale that was used to create this object, or <c>null</c>.
        /// <para/>
        /// Indicates the locale of the resource containing the data. This is always
        /// at or above the valid locale. If the valid locale does not contain the
        /// specific data being requested, then the actual locale will be
        /// above the valid locale. If the object was not constructed from
        /// locale data, then the valid locale is <c>null</c>.
        /// <para/>
        /// This may may differ from the locale requested at the time of
        /// this object's creation. For example, if an object is created
        /// for locale <c>en_US_CALIFORNIA</c>, the actual data may be
        /// drawn from <c>en</c> (the <i>actual</i> locale), and
        /// <c>en_US</c> may be the most specific locale that exists (the
        /// <i>valid</i> locale).
        /// <para/>
        /// Note: This property will be implemented in ICU 3.0; ICU 2.8
        /// contains a partial preview implementation. The * <i>actual</i>
        /// locale is returned correctly, but the <i>valid</i> locale is
        /// not, in most cases.
        /// <para/>
        /// The base class method always returns <see cref="UCultureInfo.InvariantCulture"/>
        /// Subclasses should override it if appropriate.
        /// </summary>
        /// <seealso cref="UCultureInfo"/>
        /// <draft>ICU 2.8 (retain)</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public UCultureInfo ActualCulture
            => actualCulture;

        /// <summary>
        /// <icu/> Gets the locale that was used to create this object, or <c>null</c>.
        /// <para/>
        /// Indicates the most specific locale for which any data exists.
        /// This is always at or above the requested locale, and at or below
        /// the actual locale. If the requested locale does not correspond
        /// to any resource data, then the valid locale will be above the
        /// requested locale. If the object was not constructed from locale
        /// data, then the actual locale is <c>null</c>.
        /// <para/>
        /// This may may differ from the locale requested at the time of
        /// this object's creation. For example, if an object is created
        /// for locale <c>en_US_CALIFORNIA</c>, the actual data may be
        /// drawn from <c>en</c> (the <i>actual</i> locale), and
        /// <c>en_US</c> may be the most specific locale that exists (the
        /// <i>valid</i> locale).
        /// <para/>
        /// Note: This property will be implemented in ICU 3.0; ICU 2.8
        /// contains a partial preview implementation. The * <i>actual</i>
        /// locale is returned correctly, but the <i>valid</i> locale is
        /// not, in most cases.
        /// <para/>
        /// The base class method always returns <see cref="UCultureInfo.InvariantCulture"/>
        /// Subclasses should override it if appropriate.
        /// </summary>
        /// <seealso cref="UCultureInfo"/>
        /// <draft>ICU 2.8 (retain)</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public UCultureInfo ValidCulture
            => validCulture;

        /// <summary>
        /// Set information about the locales that were used to create this
        /// object.  If the object was not constructed from locale data,
        /// both arguments should be set to null.  Otherwise, neither
        /// should be null.  The actual locale must be at the same level or
        /// less specific than the valid locale. This method is intended
        /// for use by factories or other entities that create objects of
        /// this class.
        /// </summary>
        /// <param name="valid">The most specific locale containing any resource data, or <c>null</c>.</param>
        /// <param name="actual">The locale containing data used to construct this object, or <c>null</c>.</param>
        /// <seealso cref="UCultureInfo"/>
        internal void SetCulture(UCultureInfo valid, UCultureInfo actual) // ICU4N TODO: API - In general, formatters in .NET should be unaware of the culture unless it is explictly passed to the Format() method. Need to rework this.
        {
            // Change the following to an assertion later
            if ((valid == null) != (actual == null))
            {
                ////CLOVER:OFF
                throw new ArgumentException();
                ////CLOVER:ON
            }
            // Another check we could do is that the actual locale is at
            // the same level or less specific than the valid locale.
            this.validCulture = valid;
            this.actualCulture = actual;
        }

        /// <summary>
        /// The most specific locale containing any resource data, or null.
        /// </summary>
        /// <seealso cref="UCultureInfo"/>
        private UCultureInfo validCulture;

        /// <summary>
        /// The locale containing data used to construct this object, or
        /// null.
        /// </summary>
        /// <seealso cref="UCultureInfo"/>
        private UCultureInfo actualCulture;

        // -------- END UCultureInfo boilerplate --------
    }
}
