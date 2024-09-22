using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICU4N.Text
{
    public partial class UnicodeSet
    {
        // ICU4N: These cannot be factored out like char[] because ReadOnlySpan<char> cannot
        // be used in an array or collection.

        /// <seealso cref="UnionWith(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        // See ticket #11395, this is safe.
        [CLSCompliant(false)]
        public virtual UnicodeSet UnionWith(params char[][] collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            return AddAll(collection);
        }

        /// <seealso cref="ExceptWith(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet ExceptWith(IEnumerable<char[]> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            return RemoveAll(collection);
        }

        /// <summary>
        /// Complement the specified <paramref name="collection"/> with this set.
        /// The set will not contain the specified set once the call
        /// returns.
        /// <para/>
        /// <b>Warning: you cannot add an empty string ("") to a UnicodeSet.</b>
        /// </summary>
        /// <param name="collection">The collection to complement.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet SymmetricExceptWith(IEnumerable<char[]> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            // if set is empty, then symmetric difference is other
            if (this.Count == 0)
                return UnionWith(collection);

            // special case this; the symmetric difference of a set with itself is the empty set
            if (collection == this)
            {
                Clear();
                return this;
            }

            var temp = new SortedSet<string>(collection.Select(x => new string(x)), StringComparer.Ordinal);
            temp.ExceptWith(this);
            this.ExceptWith(collection);
            return this.UnionWith(temp);
        }

        /// <seealso cref="IntersectWith(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet IntersectWith(IEnumerable<char[]> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            return RetainAll(collection);
        }

        /// <summary>
        /// Determines whether a <see cref="UnicodeSet"/> object is a proper superset of the specified collection.
        /// </summary>
        /// <param name="collection">The collection to compare to the current <see cref="UnicodeSet"/> object.</param>
        /// <returns>true if the <see cref="UnicodeSet"/> object is a proper superset of <paramref name="collection"/>; otherwise, false.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsProperSupersetOf(IEnumerable<char[]> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            int count = this.Count;
            if (count == 0) // empty set isn't a proper superset of any set.
                return false;

            ICollection<char[]> otherAsCollection = collection as ICollection<char[]>;
            if (otherAsCollection != null && otherAsCollection.Count == 0)
                return true; // note that this has at least one element, based on above check

            int contained = 0;
            foreach (var o in collection)
            {
                if (!Contains(o))
                    return false;
                else
                    contained++;
            }
            return contained < count;
        }

        /// <seealso cref="IsSupersetOf(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsSupersetOf(IEnumerable<char[]> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            return ContainsAll(collection);
        }

        /// <summary>
        /// Determines whether a <see cref="UnicodeSet"/> object is a proper subset of the specified collection.
        /// </summary>
        /// <param name="collection">The collection to compare to the current <see cref="UnicodeSet"/> object.</param>
        /// <returns>true if the <see cref="UnicodeSet"/> object is a proper subset of <paramref name="collection"/>; otherwise, false.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsProperSubsetOf(IEnumerable<char[]> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            int count = this.Count;

            ICollection<char[]> otherAsCollection = collection as ICollection<char[]>;
            if (otherAsCollection != null)
            {
                if (count == 0)
                    return otherAsCollection.Count > 0; // the empty set is a proper subset of anything but the empty set

                var otherAsSet = collection as UnicodeSet;
                if (otherAsSet != null)
                    return IsProperSubsetOf(otherAsSet);
            }

            // ICU4N TODO: This could be optimized better
            // if we had overloads of IndexOf() for each charSequence type.
            // See implementation of System.Collections.Generic.HashSet<T>.

            // count of items in other not found in this
            int unfoundCount = 0;
            // unique items in other found in this
            var found = new HashSet<string>(StringComparer.Ordinal);

            foreach (var o in collection)
            {
                if (Contains(o))
                    found.Add(new string(o));
                else
                    unfoundCount++;
            }
            return unfoundCount > 0 && found.Count == this.Count;
        }

        /// <summary>
        /// Determines whether a <see cref="UnicodeSet"/> object is a subset of the specified collection.
        /// </summary>
        /// <param name="collection">The collection to compare to the current <see cref="UnicodeSet"/> object.</param>
        /// <returns>true if the <see cref="UnicodeSet"/> object is a subset of <paramref name="collection"/>; otherwise, false.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsSubsetOf(IEnumerable<char[]> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            int count = this.Count;
            if (count == 0)
                return true;

            var otherAsSet = collection as UnicodeSet;
            if (otherAsSet != null)
            {
                // if this has more elements then it can't be a subset
                if (count > otherAsSet.Count)
                    return false;

                return IsSubsetOf(otherAsSet);
            }

            // ICU4N TODO: This could be optimized better
            // if we had overloads of IndexOf() for each charSequence type.
            // See implementation of System.Collections.Generic.HashSet<T>.

            // count of items in other not found in this
            int unfoundCount = 0;
            // unique items in other found in this
            var found = new HashSet<string>(StringComparer.Ordinal);

            foreach (var o in collection)
            {
                if (Contains(o))
                    found.Add(new string(o));
                else
                    unfoundCount++;
            }
            return unfoundCount >= 0 && found.Count == this.Count;
        }

        /// <seealso cref="Overlaps(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool Overlaps(IEnumerable<char[]> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            return ContainsSome(collection);
        }

        /// <summary>
        /// Checks if this and other contain the same elements. This is set equality: 
        /// duplicates and order are ignored
        /// </summary>
        /// <param name="collection">The collection to compare to the current <see cref="UnicodeSet"/> object.</param>
        /// <returns><c>true</c> if the sets contain the same elements; otherwise <c>false</c>.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool SetEquals(IEnumerable<char[]> collection)
        {
            if (collection == null)
                return false;

            if (this == collection)
                return true;

            UnicodeSet otherAsSet = collection as UnicodeSet;
            if (otherAsSet != null)
            {
                if (this.Count != otherAsSet.Count)
                    return false;

                return ContainsAll(otherAsSet);
            }
            else
            {
                var otherAsCollection = collection as ICollection<char[]>;
                if (otherAsCollection != null)
                {
                    if (this.Count == 0 && otherAsCollection.Count > 0)
                        return false;
                }
                int countOfOther = 0;
                foreach (var item in collection)
                {
                    if (!this.Contains(item))
                        return false;
                    countOfOther++;
                }
                return countOfOther == this.Count;
            }
        }
    }
}
