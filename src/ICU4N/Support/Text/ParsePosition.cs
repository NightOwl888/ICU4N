namespace ICU4N.Support.Text
{
    /// <summary>
    /// Tracks the current position in a parsed string. In case of an error the error
    /// index can be set to the position where the error occurred without having to
    /// change the parse position.
    /// </summary>
    public class ParsePosition
    {
        private int currentPosition, errorIndex = -1;

        /// <summary>
        /// Constructs a new <see cref="ParsePosition"/> with the specified index.
        /// </summary>
        /// <param name="index">The index to begin parsing.</param>
        public ParsePosition(int index)
        {
            currentPosition = index;
        }

        /// <summary>
        /// Compares the specified object to this <see cref="ParsePosition"/> and indicates
        /// if they are equal. In order to be equal, <paramref name="obj"/> must be an
        /// instance of <see cref="ParsePosition"/> and it must have the same index and
        /// error index.
        /// </summary>
        /// <param name="obj">The object to compare with this object.</param>
        /// <returns><c>true</c> if the specified object is equal to this
        /// <see cref="ParsePosition"/>; <c>false</c> otherwise.</returns>
        /// <seealso cref="GetHashCode()"/>
        public override bool Equals(object obj)
        {
            if (!(obj is ParsePosition))
            {
                return false;
            }
            ParsePosition pos = (ParsePosition)obj;
            return currentPosition == pos.currentPosition
                    && errorIndex == pos.errorIndex;
        }

        /// <summary>
        /// Gets or sets the index at which the parse could not continue.
        /// <para/>
        /// Returns -1 if there is no error.
        /// </summary>
        public int ErrorIndex
        {
            get { return errorIndex; }
            set { errorIndex = value; }
        }

        /// <summary>
        /// Gets or sets the current parse position.
        /// </summary>
        public int Index
        {
            get { return currentPosition; }
            set { currentPosition = value; }
        }

        public override int GetHashCode()
        {
            return currentPosition + errorIndex;
        }

        /////**
        //// * Sets the index at which the parse could not continue.
        //// * 
        //// * @param index
        //// *            the index of the parse error.
        //// */
        ////public void setErrorIndex(int index)
        ////{
        ////    errorIndex = index;
        ////}

        /////**
        //// * Sets the current parse position.
        //// * 
        //// * @param index
        //// *            the current parse position.
        //// */
        ////public void setIndex(int index)
        ////{
        ////    currentPosition = index;
        ////}

        /// <summary>
        /// Returns the string representation of this parse position.
        /// </summary>
        /// <returns>The string representation of this parse position.</returns>
        public override string ToString()
        {
            return GetType().AssemblyQualifiedName + "[index=" + currentPosition //$NON-NLS-1$
                    + ", errorIndex=" + errorIndex + "]"; //$NON-NLS-1$ //$NON-NLS-2$
        }
    }
}
