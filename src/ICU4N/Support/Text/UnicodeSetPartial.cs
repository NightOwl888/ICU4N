using J2N.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    public partial class UnicodeSet : ISet<string>
    {
        // These are the .NET set facade methods that both mimic and satisfy
        // ISet<string> in .NET. 

        /// <summary>
        /// Add a collection (as strings) into this <see cref="UnicodeSet"/>.
        /// Uses standard naming convention.
        /// </summary>
        /// <param name="source">Source collection to add into.</param>
        /// <returns>A reference to this object.</returns>
        /// <draft>ICU4N 60</draft>
        // ICU4N specific overload to optimize for string
        public virtual UnicodeSet UnionWith(IEnumerable<string> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return AddAll(source);
        }

        // ICU4N NOTE: No point in having a StringBuilder overload because
        // we would need to call ToString() on it anyway, so the generic
        // one will suffice.

        /// <summary>
        /// Add a collection (as strings) into this <see cref="UnicodeSet"/>.
        /// Uses standard naming convention.
        /// </summary>
        /// <param name="source">Source collection to add into.</param>
        /// <returns>A reference to this object.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        // ICU4N specific overload to properly convert char array to string
        public virtual UnicodeSet UnionWith(IEnumerable<char[]> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return AddAll(source);
        }

        /// <summary>
        /// Add a collection (as strings) into this <see cref="UnicodeSet"/>.
        /// Uses standard naming convention.
        /// </summary>
        /// <typeparam name="T">The type of element to add (this method calls ToString() to convert this type to a string).</typeparam>
        /// <param name="source">Source collection to add into.</param>
        /// <returns>A reference to this object.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet UnionWith<T>(IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return AddAll(source);
        }

        /// <summary>
        /// Adds all characters in range (uses preferred naming convention).
        /// </summary>
        /// <param name="start">The index of where to start on adding all characters.</param>
        /// <param name="end">The index of where to end on adding all characters.</param>
        /// <returns>A reference to this object.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet UnionWithChars(int start, int end)
        {
            return AddAll(start, end);
        }

        /// <summary>
        /// Adds all of the elements in the specified set to this set if
        /// they're not already present.  This operation effectively
        /// modifies this set so that its value is the <i>union</i> of the two
        /// sets.  The behavior of this operation is unspecified if the specified
        /// collection is modified while the operation is in progress.
        /// </summary>
        /// <param name="c">Set whose elements are to be added to this set.</param>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet UnionWith(UnicodeSet c)
        {
            if (c == null)
                throw new ArgumentNullException(nameof(c));
            return AddAll(c);
        }

        /// <summary>
        /// Add the contents of the UnicodeSet (as strings) into a collection.
        /// </summary>
        /// <param name="target">Collection to add into.</param>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual ICollection<string> CopyTo(ICollection<string> target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            return AddAllTo(this, target);
        }

        /// <summary>
        /// Add the contents of the UnicodeSet (as <see cref="T:char[]"/>s) into a collection.
        /// </summary>
        /// <param name="target">Collection to add into.</param>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual ICollection<char[]> CopyTo(ICollection<char[]> target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            foreach (var item in this)
            {
                target.Add(item.ToCharArray());
            }
            return target;
        }

        /// <summary>
        /// Add the contents of the UnicodeSet (as <see cref="StringBuilder"/>s) into a collection.
        /// </summary>
        /// <param name="target">Collection to add into.</param>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual ICollection<StringBuilder> CopyTo(ICollection<StringBuilder> target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            foreach (var item in this)
            {
                target.Add(new StringBuilder(item));
            }
            return target;
        }

        /// <summary>
        /// Add the contents of the UnicodeSet (as <see cref="StringCharSequence"/>s) into a collection.
        /// </summary>
        /// <param name="target">Collection to add into.</param>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        internal virtual ICollection<ICharSequence> CopyTo(ICollection<ICharSequence> target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            foreach (var item in this)
            {
                target.Add(item.AsCharSequence());
            }
            return target;
        }

        /// <summary>
        /// Add the contents of the UnicodeSet (using a factory delegate to instantiate the <see cref="ICharSequence"/> types) into a collection.
        /// </summary>
        /// <typeparam name="T">Collection type.</typeparam>
        /// <param name="target">Collection to add into.</param>
        /// <param name="charSequencFactory">A factory delegate to  instantiate the <see cref="ICharSequence"/> types.</param>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        internal virtual T CopyTo<T>(T target, Func<string, ICharSequence> charSequencFactory) where T : ICollection<ICharSequence>
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            foreach (var item in this)
            {
                target.Add(charSequencFactory(item));
            }
            return target;
        }

        /// <summary>
        /// Complements the specified range in this set.  Any character in
        /// the range will be removed if it is in this set, or will be
        /// added if it is not in this set.  If <c><paramref name="end"/> &gt; <paramref name="start"/></c>
        /// then an empty range is complemented, leaving the set unchanged.
        /// </summary>
        /// <param name="start">First character, inclusive, of range to be removed from this set.</param>
        /// <param name="end">Last character, inclusive, of range to be removed from this set.</param>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet SymmetricExceptWithChars(int start, int end)
        {
            return Complement(start, end);
        }

        /// <summary>
        /// Complements the specified character in this set.  The character
        /// will be removed if it is in this set, or will be added if it is
        /// not in this set.
        /// </summary>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public UnicodeSet SymmetricExceptWith(int c)
        {
            return Complement(c);
        }

        /// <summary>
        /// This is equivalent to
        /// <c>Complement(<see cref="MinValue"/>, <see cref="MaxValue"/>)</c>
        /// </summary>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet SymmetricExceptWithChars()
        {
            return Complement();
        }

        /// <summary>
        /// Complements in this set all elements contained in the specified
        /// set.  Any character in the other set will be removed if it is
        /// in this set, or will be added if it is not in this set.
        /// </summary>
        /// <param name="c">Set that defines which elements will be complemented from
        /// this set.</param>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet SymmetricExceptWith(UnicodeSet c)
        {
            if (c == null)
                throw new ArgumentNullException(nameof(c));
            return ComplementAll(c);
        }

        /// <summary>
        /// Returns true if there is a partition of the string such that this set contains each of the partitioned strings.
        /// For example, for the Unicode set [a{bc}{cd}]
        /// <list type="bullet">
        ///     <item><description><see cref="IsSupersetOf(string)"/> is true for each of: "a", "bc", ""cdbca"</description></item>
        ///     <item><description><see cref="IsSupersetOf(string)"/> is false for each of: "acb", "bcda", "bcx"</description></item>
        /// </list>
        /// </summary>
        /// <param name="s">String containing characters to be checked for containment.</param>
        /// <returns>true if the test condition is met.</returns>
        /// <stable>ICU 2.0</stable>
        public virtual bool IsSupersetOf(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            return ContainsAll(s);
        }

        /// <summary>
        /// Returns true if this set contains all the characters and strings
        /// of the given set.
        /// </summary>
        /// <param name="b">Set to be checked for containment.</param>
        /// <returns>true if the test condition is met.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsSupersetOf(UnicodeSet b)
        {
            if (b == null)
                throw new ArgumentNullException(nameof(b));
            return ContainsAll(b);
        }

        /// <summary>
        /// Determines whether a <see cref="UnicodeSet"/> object is a proper subset of the specified <see cref="UnicodeSet"/>.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="UnicodeSet"/> object.</param>
        /// <returns>true if the <see cref="UnicodeSet"/> object is a proper subset of other; otherwise, false.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsProperSubsetOf(UnicodeSet other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (this.Count >= other.Count)
                return false;

            return IsSubsetOfInternal(other);
        }

        /// <summary>
        /// Determines whether a <see cref="UnicodeSet"/> object is a subset of the specified <see cref="UnicodeSet"/>.
        /// </summary>
        /// <param name="other">The collection to compare to the current <see cref="UnicodeSet"/> object.</param>
        /// <returns>true if the <see cref="UnicodeSet"/> object is a subset of other; otherwise, false.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsSubsetOf(UnicodeSet other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return IsSubsetOfInternal(other);
        }

        private bool IsSubsetOfInternal(UnicodeSet other)
        {
            foreach (var item in this)
            {
                if (!other.Contains(item))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if this set contains one or more of the characters
        /// in the given range.
        /// </summary>
        /// <param name="start">First character, inclusive, of the range.</param>
        /// <param name="end">Last character, inclusive, of the range.</param>
        /// <returns>true if the condition is met.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public bool Overlaps(int start, int end)
        {
            return ContainsSome(start, end);
        }

        /// <summary>
        /// Returns true if this set contains one or more of the characters
        /// and strings of the given set.
        /// </summary>
        /// <param name="s">Set to be checked for containment.</param>
        /// <returns>True if the condition is met.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public bool Overlaps(UnicodeSet s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            return ContainsSome(s);
        }

        /// <summary>
        /// Returns <c>true</c> if this set contains elements.
        /// </summary>
        /// <remarks>
        /// This method will override the default behavior of the LINQ Any() extension method.
        /// </remarks>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool Any()
        {
            return !IsEmpty;
        }

        /// <summary>
        /// Removes from this set all of its elements that are contained in the
        /// specified set.  This operation effectively modifies this
        /// set so that its value is the <i>asymmetric set difference</i> of
        /// the two sets.
        /// </summary>
        /// <param name="c">Set that defines which elements will be removed from
        /// this set.</param>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet ExceptWith(UnicodeSet c)
        {
            if (c == null)
                throw new ArgumentNullException(nameof(c));
            return RemoveAll(c);
        }

        /// <summary>
        /// Remove all strings from this <see cref="UnicodeSet"/>
        /// </summary>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public UnicodeSet ClearStrings()
        {
            return RemoveAllStrings();
        }

        /// <summary>
        /// Retains only the elements in this set that are contained in the
        /// specified set.  In other words, removes from this set all of
        /// its elements that are not contained in the specified set.  This
        /// operation effectively modifies this set so that its value is
        /// the <i>intersection</i> of the two sets.
        /// </summary>
        /// <param name="c">Set that defines which elements this set will retain.</param>
        /// <stable>ICU 2.0</stable>
        public virtual UnicodeSet IntersectWith(UnicodeSet c)
        {
            if (c == null)
                throw new ArgumentNullException(nameof(c));
            return RetainAll(c);
        }

        /// <summary>
        /// Retain the specified character from this set if it is present.
        /// Upon return this set will be empty if it did not contain <paramref name="c"/>, or
        /// will only contain c if it did contain <paramref name="c"/>.
        /// </summary>
        /// <param name="c">The character to be retained.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public UnicodeSet IntersectWith(int c)
        {
            return Retain(c);
        }

        /// <summary>
        /// Retain only the elements in this set that are contained in the
        /// specified range.  If <c><paramref name="end"/> &gt; <paramref name="start"/></c> 
        /// then an empty range is retained, leaving the set empty.
        /// </summary>
        /// <param name="start">First character, inclusive, of range to be retained
        /// to this set.</param>
        /// <param name="end">Last character, inclusive, of range to be retained
        /// to this set.</param>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet IntersectWithChars(int start, int end)
        {
            return Retain(start, end);
        }

        #region ISet<string> Members

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
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            using (var iter = this.GetEnumerator())
            {
                for (int i = arrayIndex; iter.MoveNext(); i++)
                    array[i] = iter.Current;
            }
        }

        void ISet<string>.ExceptWith(IEnumerable<string> other)
        {
            this.ExceptWith(other); // standard implementation returns UnicodeSet, so we need an overload
        }

        void ISet<string>.IntersectWith(IEnumerable<string> other)
        {
            this.IntersectWith(other); // standard implementation returns UnicodeSet, so we need an overload
        }

        void ISet<string>.SymmetricExceptWith(IEnumerable<string> other)
        {
            this.SymmetricExceptWith(other); // standard implementation returns UnicodeSet, so we need an overload
        }

        void ISet<string>.UnionWith(IEnumerable<string> other)
        {
            this.UnionWith(other); // standard implementation returns UnicodeSet, so we need an overload
        }

        bool ISet<string>.Add(string item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            this.Add(item); // standard implementation returns UnicodeSet, so we need an overload
            return true;
        }

        void ICollection<string>.Add(string item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            this.Add(item); // standard implementation returns UnicodeSet, so we need an overload
        }

        void ICollection<string>.Clear()
        {
            this.Clear(); // standard implementation returns UnicodeSet, so we need an overload
        }

        bool ICollection<string>.Remove(string item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            this.Remove(item); // standard implementation returns UnicodeSet, so we need an overload
            return true;
        }
        #endregion
    }
}
