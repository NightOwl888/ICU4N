using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.Text
{
    // from Apache Harmony

    /// <summary>
    /// Identifies fields in formatted strings. If a <see cref="FieldPosition"/> is passed
    /// to the format method with such a parameter, then the indices will be set to
    /// the start and end indices of the field in the formatted string.
    /// </summary>
    /// <remarks>
    /// A <see cref="FieldPosition"/> can be created by using the integer constants in the
    /// various format classes (for example <c>NumberFormat.IntegerField</c>) or
    /// one of the fields of type
    /// </remarks>
    internal class FieldPosition
    {
        private int myField, beginIndex, endIndex;

        private FormatField myAttribute;

        /// <summary>
        /// Constructs a new <see cref="FieldPosition"/> for the specified field.
        /// </summary>
        /// <param name="field">The field to identify.</param>
        public FieldPosition(int field)
        {
            myField = field;
        }

        /// <summary>
        /// Constructs a new <see cref="FieldPosition"/> for the specified <see cref="FormatField"/>
        /// <paramref name="attribute"/>.
        /// </summary>
        /// <param name="attribute">the field attribute to identify.</param>
        public FieldPosition(FormatField attribute)
        {
            myAttribute = attribute;
            myField = -1;
        }

        /// <summary>
        /// Constructs a new <see cref="FieldPosition"/> for the specified <see cref="FormatField"/>
        /// <paramref name="attribute"/> and <paramref name="field"/> id.
        /// </summary>
        /// <param name="attribute">the field attribute to identify.</param>
        /// <param name="field">the field to identify.</param>
        public FieldPosition(FormatField attribute, int field)
        {
            myAttribute = attribute;
            myField = field;
        }

        internal void Clear()
        {
            beginIndex = endIndex = 0;
        }

        /// <summary>
        /// Compares the specified object to this field position and indicates if
        /// they are equal. In order to be equal, <paramref name="obj"/> must be an instance
        /// of <see cref="FieldPosition"/> with the same field, begin index and end index.
        /// </summary>
        /// <param name="obj">the object to compare with this object.</param>
        /// <returns><c>true</c> if the specified object is equal to this field
        /// position; <c>false</c> otherwise.</returns>
        /// <seealso cref="GetHashCode()"/>
        public override bool Equals(object obj)
        {
            if (!(obj is FieldPosition))
            {
                return false;
            }
            FieldPosition pos = (FieldPosition)obj;
            return myField == pos.myField //&& myAttribute == pos.myAttribute
                    && beginIndex == pos.beginIndex && endIndex == pos.endIndex;
        }

        /// <summary>
        /// Gets or sets the index of the beginning of the field.
        /// </summary>
        public virtual int BeginIndex
        {
            get { return beginIndex; }
            set { beginIndex = value; }
        }

        /// <summary>
        /// Gets or sets the index one past the end of the field.
        /// </summary>
        public virtual int EndIndex
        {
            get { return endIndex; }
            set { endIndex = value; }
        }

        /// <summary>
        /// Gets the field which is being identified.
        /// </summary>
        public virtual int Field
        {
            get { return myField; }
        }

        /// <summary>
        /// Gets the attribute which is being identified.
        /// </summary>
        public FormatField FieldAttribute
        {
            get => myAttribute;
        }

        public override int GetHashCode()
        {
            int attributeHash = 0;//(myAttribute == null) ? 0 : myAttribute.hashCode();
            return attributeHash + myField * 10 + beginIndex * 100 + endIndex;
        }

        /////**
        //// * Sets the index of the beginning of the field.
        //// * 
        //// * @param index
        //// *            the index of the first character in the field.
        //// */
        ////public void setBeginIndex(int index)
        ////{
        ////    beginIndex = index;
        ////}

        /////**
        //// * Sets the index of the end of the field.
        //// * 
        //// * @param index
        //// *            one past the index of the last character in the field.
        //// */
        ////public void setEndIndex(int index)
        ////{
        ////    endIndex = index;
        ////}

        /// <summary>
        /// Returns the string representation of this field position.
        /// </summary>
        /// <returns>the string representation of this field position.</returns>
        public override string ToString()
        {
            return GetType().Name + /*"[attribute=" + myAttribute + ", */"[field=" //$NON-NLS-1$ //$NON-NLS-2$
                    + myField + ", beginIndex=" + beginIndex + ", endIndex=" //$NON-NLS-1$ //$NON-NLS-2$
                    + endIndex + "]"; //$NON-NLS-1$
        }
    }
}
