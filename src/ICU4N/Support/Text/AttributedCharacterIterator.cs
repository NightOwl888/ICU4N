using System.Collections.Generic;

namespace ICU4N.Support.Text
{
    // from Apache Harmony

    /// <summary>
    /// Extends the <see cref="CharacterIterator"/> class, adding support for iterating over
    /// attributes and not only characters. An
    /// <see cref="AttributedCharacterIterator"/> also allows the user to find runs and
    /// their limits. Runs are defined as ranges of characters that all have the same
    /// attributes with the same values.
    /// </summary>
    internal abstract class AttributedCharacterIterator : CharacterIterator
    {
        /// <summary>
        /// Returns a set of attributes present in the <see cref="AttributedCharacterIterator"/>.
        /// An empty set is returned if no attributes were defined.
        /// </summary>
        /// <returns>A set of attribute keys; may be empty.</returns>
        public abstract ICollection<AttributedCharacterIteratorAttribute> GetAllAttributeKeys();

        /// <summary>
        /// Returns the value stored in the attribute for the current character. If
        /// the attribute was not defined then <c>null</c> is returned.
        /// </summary>
        /// <param name="attribute">The attribute for which the value should be returned.</param>
        /// <returns>the value of the requested attribute for the current character or <c>null</c>
        /// if it was not defined.</returns>
        public abstract object GetAttribute(AttributedCharacterIteratorAttribute attribute);

        /// <summary>
        /// Returns a map of all attributes of the current character. If no
        /// attributes were defined for the current character then an empty map is
        /// returned.
        /// </summary>
        /// <returns>A dictionary of all attributes for the current character or an empty dictionary.</returns>
        public abstract IDictionary<AttributedCharacterIteratorAttribute, object> GetAttributes();

        /// <summary>
        /// Returns the index of the last character in the run having the same
        /// attributes as the current character.
        /// </summary>
        /// <returns>The index of the last character of the current run.</returns>
        public abstract int GetRunLimit();

        /// <summary>
        /// Returns the index of the last character in the run that has the same
        /// attribute value for the given attribute as the current character.
        /// </summary>
        /// <param name="attribute">The attribute which the run is based on.</param>
        /// <returns>The index of the last character of the current run.</returns>
        public abstract int GetRunLimit(AttributedCharacterIteratorAttribute attribute);

        /// <summary>
        /// Returns the index of the last character in the run that has the same
        /// attribute values for the attributes in the set as the current character.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="attributes">The set of attributes which the run is based on.</param>
        /// <returns>The index of the last character of the current run.</returns>
        public abstract int GetRunLimit<T>(ICollection<T> attributes) where T : AttributedCharacterIteratorAttribute;

        /// <summary>
        /// Returns the index of the first character in the run that has the same
        /// attributes as the current character.
        /// </summary>
        /// <returns>The index of the last character of the current run.</returns>
        public abstract int GetRunStart();

        /// <summary>
        /// Returns the index of the first character in the run that has the same
        /// attribute value for the given attribute as the current character.
        /// </summary>
        /// <param name="attribute">The attribute which the run is based on.</param>
        /// <returns>The index of the last character of the current run.</returns>
        public abstract int GetRunStart(AttributedCharacterIteratorAttribute attribute);

        /// <summary>
        /// Returns the index of the first character in the run that has the same
        /// attribute values for the attributes in the set as the current character.
        /// </summary>
        /// <typeparam name="T">The type of attribute, <see cref="AttributedCharacterIteratorAttribute"/> or a subclass of it.</typeparam>
        /// <param name="attributes">The set of attributes which the run is based on.</param>
        /// <returns>The index of the last character of the current run.</returns>
        public abstract int GetRunStart<T>(ICollection<T> attributes) where T : AttributedCharacterIteratorAttribute;
    }

    /// <summary>
    /// Defines keys for text attributes.
    /// </summary>
    internal class AttributedCharacterIteratorAttribute //implements Serializable
    {

        //private static readonly long serialVersionUID = -9142742483513960612L;

        /// <summary>
        /// This attribute marks segments from an input method. Most input
        /// methods create these segments for words.
        /// <para/>
        /// The value objects are of the type <c>Annotation</c> which contain <c>null</c>.
        /// </summary>
        public static readonly AttributedCharacterIteratorAttribute InputMethodSegment = new AttributedCharacterIteratorAttribute(
                    "input_method_segment"); //$NON-NLS-1$

        /// <summary>
        /// The attribute describing the language of a character. The value
        /// objects are of type <see cref="System.Globalization.CultureInfo"/> or a subtype of it.
        /// </summary>
        public static readonly AttributedCharacterIteratorAttribute Language = new AttributedCharacterIteratorAttribute("language"); //$NON-NLS-1$

        /// <summary>
        /// For languages that have different reading directions of text (like
        /// Japanese), this attribute allows to define which reading should be
        /// used. The value objects are of type <c>Annotation</c> which
        /// contain a <see cref="string"/>.
        /// </summary>
        public static readonly AttributedCharacterIteratorAttribute Reading = new AttributedCharacterIteratorAttribute("reading"); //$NON-NLS-1$

        private string name;

        /// <summary>
        /// The constructor for an <see cref="AttributedCharacterIteratorAttribute"/> with the name passed.
        /// </summary>
        /// <param name="name">The name of the new <see cref="AttributedCharacterIteratorAttribute"/>.</param>
        protected AttributedCharacterIteratorAttribute(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Compares this attribute with the specified object. Checks if both
        /// objects are the same instance. It is defined sealed so all subclasses
        /// have the same behavior for this method.
        /// </summary>
        /// <param name="obj">The object to compare against.</param>
        /// <returns><c>true</c> if the object passed is equal to this instance; <c>false</c> otherwise.</returns>
        public override sealed bool Equals(object obj)
        {
            return this == obj;
        }

        /// <summary>
        /// Gets the name of this attribute.
        /// </summary>
        protected virtual string Name => name;

        /// <summary>
        /// Calculates the hash code for objects of type <see cref="AttributedCharacterIteratorAttribute"/>. It
        /// is defined final so all sub types calculate their hash code
        /// identically.
        /// </summary>
        /// <returns>the hash code for this instance of <see cref="AttributedCharacterIteratorAttribute"/>.</returns>
        public override sealed int GetHashCode()
        {
            return base.GetHashCode();
        }

        /**
         * Resolves a deserialized instance to the correct constant attribute.
         *
         * @return the {@code Attribute} this instance represents.
         * @throws InvalidObjectException
         *             if this instance is not of type {@code Attribute.class}
         *             or if it is not a known {@code Attribute}.
         */
        protected virtual object ReadResolve()
        {
            return null;
            // ICU4N TODO: Serialization
            //            if (this.GetUnicodeCategory() != typeof(Attribute) {
            //                // text.0C=cannot resolve subclasses
            //                throw new InvalidCastException(/*Messages.getString("text.0C")*/); //$NON-NLS-1$
            //            }
            //            if (this.getName().equals(INPUT_METHOD_SEGMENT.getName())) {
            //    return INPUT_METHOD_SEGMENT;
            //}
            //            if (this.getName().equals(LANGUAGE.getName())) {
            //    return LANGUAGE;
            //}
            //            if (this.getName().equals(READING.getName())) {
            //    return READING;
            //}
            //            // text.02=Unknown attribute
            //            throw new InvalidObjectException(Messages.getString("text.02")); //$NON-NLS-1$
        }

        /// <summary>
        /// Returns the name of the class followed by a "(", the name of the
        /// attribute, and a ")".
        /// </summary>
        /// <returns>the string representing this instance.</returns>
        public override string ToString()
        {
            return GetType().Name + '(' + Name + ')';
        }
    }
}
