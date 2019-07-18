using System;
using System.Collections.Generic;

namespace ICU4N.Text
{
    public partial class UnicodeSet //: ISet<string>
    {
        /// <summary>
        /// Whether this set is readonly. This is always the same value as <see cref="IsFrozen"/>.
        /// </summary>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsReadOnly => this.IsFrozen;

        /// <summary>
        /// Copies the elements of a <see cref="UnicodeSet"/> collection to an array.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements
        /// copied from the <see cref="UnicodeSet"/> object. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The index of the array to begin copying elements to.</param>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual void CopyTo(string[] array, int arrayIndex)
        {
            using (var iter = this.GetEnumerator())
            {
                for (int i = arrayIndex; iter.MoveNext(); i++)
                    array[i] = iter.Current;
            }
        }

        /// <summary>
        /// Determines whether a <see cref="UnicodeSet"/> object is a proper subset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="UnicodeSet"/> object.</param>
        /// <returns>true if the <see cref="UnicodeSet"/> object is a proper subset of other; otherwise, false.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsProperSubsetOf(IEnumerable<string> other)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether a <see cref="UnicodeSet"/> object is a proper superset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="UnicodeSet"/> object.</param>
        /// <returns>true if the <see cref="UnicodeSet"/> object is a proper superset of other; otherwise, false.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsProperSupersetOf(IEnumerable<string> other)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether a <see cref="UnicodeSet"/> object is a subset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="UnicodeSet"/> object.</param>
        /// <returns>true if the <see cref="UnicodeSet"/> object is a subset of other; otherwise, false.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsSubsetOf(IEnumerable<string> other)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if this and other contain the same elements. This is set equality: 
        /// duplicates and order are ignored
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool SetEquals(IEnumerable<string> other) // TODO: Generate overloads with T4
        {
            if (this == other)
            {
                return true;
            }
            UnicodeSet otherAsSet = other as UnicodeSet;
            if (otherAsSet != null)
            {
                if (this.Count != otherAsSet.Count)
                {
                    return false;
                }
                return ContainsAll(otherAsSet); // ICU4N TODO: API - rename IsSupersetOf
            }
            else
            {
                ICollection<string> otherAsCollection = other as ICollection<string>;
                if (otherAsCollection != null)
                {
                    if (this.Count == 0 && otherAsCollection.Count > 0)
                    {
                        return false;
                    }
                }
                int countOfOther = 0;
                foreach (var item in other)
                {
                    if (!this.Contains(item))
                        return false;
                    countOfOther++;
                }
                return countOfOther == this.Count;
            }
        }

        // ICU4N TODO: API - enable the following overloads after ISet<string> is implemented by this class

        //bool ISet<string>.Add(string item)
        //{
        //    this.Add(item); // standard implementation returns UnicodeSet, so we need an overload
        //    return true;
        //}

        //void ICollection<string>.Add(string item)
        //{
        //    this.Add(item); // standard implementation returns UnicodeSet, so we need an overload
        //}

        //void ICollection<string>.Clear()
        //{
        //    this.Clear(); // standard implementation returns UnicodeSet, so we need an overload
        //}

        //bool ICollection<string>.Remove(string item)
        //{
        //    this.Remove(item); // standard implementation returns UnicodeSet, so we need an overload
        //    return true;
        //}
    }
}
