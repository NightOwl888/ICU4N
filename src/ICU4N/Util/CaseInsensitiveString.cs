using ICU4N.Lang;

namespace ICU4N.Util
{
    /// <summary>
    /// A string used as a key in <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> and other
    /// collections.  It retains case information, but its <see cref="Equals(object)"/> and
    /// <see cref="GetHashCode()"/> methods ignore case.
    /// </summary>
    /// <stable>ICU 2.0</stable>
    public class CaseInsensitiveString
    {
        private string str;
        private int hash = 0;
        private string folded = null;

        private static string FoldCase(string foldee)
        {
            return UCharacter.FoldCase(foldee, true);
        }

        private void GetFolded()
        {
            if (folded == null)
            {
                folded = FoldCase(str);
            }
        }

        /// <summary>
        /// Constructs an <see cref="CaseInsensitiveString"/> object from the given string
        /// </summary>
        /// <param name="s">The string to construct this object from.</param>
        /// <stable>ICU 2.0</stable>
        public CaseInsensitiveString(string s)
        {
            str = s;
        }

        /// <summary>
        /// Gets the underlying string.
        /// </summary>
        /// <stable>ICU 2.0</stable>
        public virtual string String
        {
            get { return str; }
        }

        /// <summary>
        /// Compare the object with this.
        /// </summary>
        /// <param name="o">Object to compare this object with.</param>
        /// <stable>ICU 2.0</stable>
        public override bool Equals(object o)
        {
            if (o == null)
            {
                return false;
            }
            if (this == o)
            {
                return true;
            }
            if (o is CaseInsensitiveString)
            {
                GetFolded();
                CaseInsensitiveString cis = (CaseInsensitiveString)o;
                cis.GetFolded();
                return folded.Equals(cis.folded);
            }
            return false;
        }

        /// <summary>
        /// Returns the hashCode of this object.
        /// </summary>
        /// <returns>int hashcode.</returns>
        /// <stable>ICU 2.0</stable>
        public override int GetHashCode()
        {
            GetFolded();

            if (hash == 0)
            {
                hash = folded.GetHashCode();
            }

            return hash;
        }

        /// <summary>
        /// Overrides superclass method.
        /// </summary>
        /// <stable>ICU 3.6</stable>
        public override string ToString()
        {
            return str;
        }
    }
}
