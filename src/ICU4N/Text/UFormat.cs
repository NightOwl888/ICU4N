using ICU4N.Support.Text;
using ICU4N.Util;
using System;

namespace ICU4N.Text
{
    /// <summary>
    /// An abstract class that extends <see cref="Formatter"/> to provide
    /// additional ICU protocol, specifically, the <see cref="GetLocale(ULocale.Type)"/>
    /// API.  All ICU format classes are subclasses of this class.
    /// </summary>
    /// <seealso cref="ULocale"/>
    /// <author>weiv</author>
    /// <author>Alan Liu</author>
    /// <stable>ICU 2.8</stable>
    internal abstract class UFormat : Formatter // ICU4N: Marked internal until formatters are refactored // ICU4N TODO: API - Add IFormatProvider, ICustomFormatter. See: https://stackoverflow.com/a/35577288
    {
        // jdk1.4.2 serialver
        //private static readonly long serialVersionUID = -4964390515840164416L;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        public UFormat() { }

        // -------- BEGIN ULocale boilerplate --------

        /// <summary>
        /// Return the locale that was used to create this object, or null.
        /// </summary>
        /// <remarks>
        /// This may may differ from the locale requested at the time of
        /// this object's creation.  For example, if an object is created
        /// for locale <tt>en_US_CALIFORNIA</tt>, the actual data may be
        /// drawn from <tt>en</tt> (the <i>actual</i> locale), and
        /// <tt>en_US</tt> may be the most specific locale that exists (the
        /// <i>valid</i> locale).
        /// <para/>
        /// Note: This method will be implemented in ICU 3.0; ICU 2.8
        /// contains a partial preview implementation.  The <i>actual</i>
        /// locale is returned correctly, but the <i>valid</i> locale is
        /// not, in most cases.
        /// </remarks>
        /// <param name="type">
        /// type of information requested, either <see cref="ULocale.VALID_LOCALE"/>
        /// or <see cref="ULocale.ACTUAL_LOCALE"/>.
        /// </param>
        /// <returns>
        /// the information specified by <i>type</i>, or null if
        /// this object was not constructed from locale data.
        /// </returns>
        /// <seealso cref="ULocale"/>
        /// <seealso cref="ULocale.VALID_LOCALE"/>
        /// <seealso cref="ULocale.ACTUAL_LOCALE"/>
        /// <draft>ICU 2.8 (retain)</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public ULocale GetLocale(ULocale.Type type)
        {
            return type == ULocale.ACTUAL_LOCALE ?
                this.actualLocale : this.validLocale;
        }

        /// <summary>
        /// Set information about the locales that were used to create this
        /// object.  If the object was not constructed from locale data,
        /// both arguments should be set to null.  Otherwise, neither
        /// should be null.  The actual locale must be at the same level or
        /// less specific than the valid locale.  This method is intended
        /// for use by factories or other entities that create objects of
        /// this class.
        /// </summary>
        /// <param name="valid">the most specific locale containing any resource
        /// data, or null</param>
        /// <param name="actual">the locale containing data used to construct this
        /// object, or null</param>
        /// <seealso cref="ULocale"/>
        /// <seealso cref="ULocale.VALID_LOCALE"/>
        /// <seealso cref="ULocale.ACTUAL_LOCALE"/>
        internal void SetLocale(ULocale valid, ULocale actual)
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
            this.validLocale = valid;
            this.actualLocale = actual;
        }

        /// <summary>
        /// The most specific locale containing any resource data, or null.
        /// </summary>
        /// <seealso cref="ULocale"/>
        private ULocale validLocale;

        /// <summary>
        /// The locale containing data used to construct this object, or
        /// null.
        /// </summary>
        /// <seealso cref="ULocale"/>
        private ULocale actualLocale;

        // -------- END ULocale boilerplate --------
    }
}
