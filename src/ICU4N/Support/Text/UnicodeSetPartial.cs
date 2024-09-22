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

        /// <seealso cref="UnionWith(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        // See ticket #11395, this is safe.
        public virtual UnicodeSet UnionWith(params string[] collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            return AddAll(collection);
        }

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
        /// Adds each of the characters in this string to the set. Thus "ch" =&gt; {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>this object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet UnionWithChars(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            return AddAll(s);
        }

        /// <summary>
        /// Adds each of the characters in this string to the set. Thus "ch" =&gt; {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>this object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet UnionWithChars(ReadOnlySpan<char> s)
        {
            return AddAll(s);
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
                target.Add(item.ToCharArray()); // ICU4N TODO: Eliminate this allocation, if possible
            }
            return target;
        }

        /// <summary>
        /// Complement the specified string in this set.
        /// The set will not contain the specified string once the call
        /// returns.
        /// <para/>
        /// <b>Warning: you cannot add an empty string ("") to a UnicodeSet.</b>
        /// </summary>
        /// <param name="s">The string to complement.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet SymmetricExceptWith(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            return Complement(s);
        }

        /// <summary>
        /// Complement the specified string in this set.
        /// The set will not contain the specified string once the call
        /// returns.
        /// <para/>
        /// <b>Warning: you cannot add an empty string ("") to a UnicodeSet.</b>
        /// </summary>
        /// <param name="s">The string to complement.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet SymmetricExceptWith(ReadOnlySpan<char> s)
        {
            return Complement(s);
        }

        /// <summary>
        /// Complement EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet SymmetricExceptWithChars(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            return ComplementAll(s);
        }

        /// <summary>
        /// Complement EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet SymmetricExceptWithChars(ReadOnlySpan<char> s)
        {
            return ComplementAll(s);
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

        // ICU4N TODO: API - need overload for IsProperSupersetOf(UnicodeSet)

        /// <seealso cref="IsSupersetOf(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsSupersetOf(IEnumerable<string> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            return ContainsAll(collection);
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
        /// of the given string.
        /// </summary>
        /// <param name="s">String containing characters to be checked for containment.</param>
        /// <returns>true if the condition is met.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool Overlaps(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            return ContainsSome(s);
        }

        /// <summary>
        /// Returns true if this set contains one or more of the characters
        /// of the given string.
        /// </summary>
        /// <param name="s">String containing characters to be checked for containment.</param>
        /// <returns>true if the condition is met.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool Overlaps(ReadOnlySpan<char> s)
        {
            return ContainsSome(s);
        }

        /// <seealso cref="Overlaps(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool Overlaps(IEnumerable<string> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            return ContainsSome(collection);
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

        /// <seealso cref="ExceptWith(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet ExceptWith(IEnumerable<string> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            return RemoveAll(collection);
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
        /// Remove EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet ExceptWithChars(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            return RemoveAll(s);
        }

        /// <summary>
        /// Remove EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet ExceptWithChars(ReadOnlySpan<char> s)
        {
            return RemoveAll(s);
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
        /// Retain the specified string in this set if it is present.
        /// Upon return this set will be empty if it did not contain <paramref name="cs"/>, or
        /// will only contain <paramref name="cs"/> if it did contain <paramref name="cs"/>.
        /// </summary>
        /// <param name="cs">The string to be retained.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet IntersectWith(string cs)
        {
            if (cs == null)
                throw new ArgumentNullException(nameof(cs));
            return Retain(cs);
        }

        /// <summary>
        /// Retain the specified string in this set if it is present.
        /// Upon return this set will be empty if it did not contain <paramref name="cs"/>, or
        /// will only contain <paramref name="cs"/> if it did contain <paramref name="cs"/>.
        /// </summary>
        /// <param name="cs">The string to be retained.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet IntersectWith(ReadOnlySpan<char> cs)
        {
            return Retain(cs);
        }

        /// <seealso cref="IntersectWith(UnicodeSet)"/>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet IntersectWith(IEnumerable<string> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            return RetainAll(collection);
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
        /// Retains EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet IntersectWithChars(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            return RetainAll(s);
        }

        /// <summary>
        /// Retains EACH of the characters in this string. Note: "ch" == {"c", "h"}
        /// If this set already any particular character, it has no effect on that character.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual UnicodeSet IntersectWithChars(ReadOnlySpan<char> s)
        {
            return RetainAll(s);
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

        // ***** .NET ISet<T> overloads that are missing from ICU4J *****

        /// <summary>
        /// Returns true if this set contains all of the characters
        /// of the given <see cref="ReadOnlySpan{Char}"/>.
        /// </summary>
        /// <param name="s"><see cref="ReadOnlySpan{Char}"/> containing characters to be checked for a superset.</param>
        /// <returns>true if the <see cref="UnicodeSet"/> object is a superset of <paramref name="s"/>; otherwise, false.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsSupersetOf(ReadOnlySpan<char> s)
        {
            return Span(s, SpanCondition.Contained) == s.Length;
        }

        /// <summary>
        /// Returns true if this set contains all of the characters
        /// of the given <see cref="string"/> plus at least one additional character.
        /// </summary>
        /// <param name="s"><see cref="string"/> containing characters to be checked for a proper superset.</param>
        /// <returns>true if the <see cref="UnicodeSet"/> object is a proper superset of <paramref name="s"/>; otherwise, false.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsProperSupersetOf(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            int contained = Span(s, SpanCondition.Contained);
            return contained == s.Length && contained < this.Count;
        }

        /// <summary>
        /// Returns true if this set contains all of the characters
        /// of the given <see cref="ReadOnlySpan{Char}"/> plus at least one additional character.
        /// </summary>
        /// <param name="s"><see cref="ReadOnlySpan{Char}"/> containing characters to be checked for a proper superset.</param>
        /// <returns>true if the <see cref="UnicodeSet"/> object is a proper superset of <paramref name="s"/>; otherwise, false.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsProperSupersetOf(ReadOnlySpan<char> s)
        {
            int contained = Span(s, SpanCondition.Contained);
            return contained == s.Length && contained < this.Count;
        }

        /// <summary>
        /// Determines whether a <see cref="UnicodeSet"/> object is a proper superset of the specified collection.
        /// </summary>
        /// <param name="collection">The collection to compare to the current <see cref="UnicodeSet"/> object.</param>
        /// <returns>true if the <see cref="UnicodeSet"/> object is a proper superset of <paramref name="collection"/>; otherwise, false.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsProperSupersetOf(IEnumerable<string> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            int count = this.Count;
            if (count == 0) // empty set isn't a proper superset of any set.
                return false;

            ICollection<string> otherAsCollection = collection as ICollection<string>;
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

        /// <summary>
        /// Returns true if this set contains all of the characters
        /// of the given <see cref="string"/> plus at least one additional character.
        /// </summary>
        /// <param name="s"><see cref="string"/> containing characters to be checked for a subset.</param>
        /// <returns>true if the <see cref="UnicodeSet"/> object is a proper subset of <paramref name="s"/>; otherwise, false.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsSubsetOf(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            return Span(s, SpanCondition.Contained) == this.Count;
        }

        /// <summary>
        /// Returns true if this set contains all of the characters
        /// of the given <see cref="ReadOnlySpan{Char}"/> plus at least one additional character.
        /// </summary>
        /// <param name="s"><see cref="ReadOnlySpan{Char}"/> containing characters to be checked for a subset.</param>
        /// <returns>true if the <see cref="UnicodeSet"/> object is a proper subset of <paramref name="s"/>; otherwise, false.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsSubsetOf(ReadOnlySpan<char> s)
        {
            return Span(s, SpanCondition.Contained) == this.Count;
        }

        /// <summary>
        /// Determines whether a <see cref="UnicodeSet"/> object is a subset of the specified collection.
        /// </summary>
        /// <param name="collection">The collection to compare to the current <see cref="UnicodeSet"/> object.</param>
        /// <returns>true if the <see cref="UnicodeSet"/> object is a subset of <paramref name="collection"/>; otherwise, false.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsSubsetOf(IEnumerable<string> collection)
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
                    found.Add(o);
                else
                    unfoundCount++;
            }
            return unfoundCount >= 0 && found.Count == this.Count;
        }

        /// <summary>
        /// Returns true if this set contains all of the characters
        /// of the given <see cref="string"/> plus at least one additional character.
        /// </summary>
        /// <param name="s"><see cref="string"/> containing characters to be checked for a proper subset.</param>
        /// <returns>true if the <see cref="UnicodeSet"/> object is a proper subset of <paramref name="s"/>; otherwise, false.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsProperSubsetOf(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            int count = this.Count;
            return count < s.Length && Span(s, SpanCondition.Contained) == count;
        }

        /// <summary>
        /// Returns true if this set contains all of the characters
        /// of the given <see cref="ReadOnlySpan{Char}"/> plus at least one additional character.
        /// </summary>
        /// <param name="s"><see cref="ReadOnlySpan{Char}"/> containing characters to be checked for a proper subset.</param>
        /// <returns>true if the <see cref="UnicodeSet"/> object is a proper subset of <paramref name="s"/>; otherwise, false.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsProperSubsetOf(ReadOnlySpan<char> s)
        {
            int count = this.Count;
            return count < s.Length && Span(s, SpanCondition.Contained) == count;
        }

        /// <summary>
        /// Determines whether a <see cref="UnicodeSet"/> object is a proper subset of the specified collection.
        /// </summary>
        /// <param name="collection">The collection to compare to the current <see cref="UnicodeSet"/> object.</param>
        /// <returns>true if the <see cref="UnicodeSet"/> object is a proper subset of <paramref name="collection"/>; otherwise, false.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool IsProperSubsetOf(IEnumerable<string> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            int count = this.Count;

            ICollection<string> otherAsCollection = collection as ICollection<string>;
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
                    found.Add(o);
                else
                    unfoundCount++;
            }
            return unfoundCount > 0 && found.Count == this.Count;
        }

        /// <summary>
        /// Returns true if this set contains all of the characters
        /// of the given <see cref="string"/> and contains no aditional characters.
        /// </summary>
        /// <param name="s"><see cref="string"/> containing characters to be checked for set equality.</param>
        /// <returns>true if the <see cref="UnicodeSet"/> object contains the same characters as <paramref name="s"/>; otherwise, false.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool SetEquals(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            return s.Length == this.Count && Span(s, SpanCondition.Contained) == s.Length;
        }

        /// <summary>
        /// Returns true if this set contains all of the characters
        /// of the given <see cref="ReadOnlySpan{Char}"/> and contains no aditional characters.
        /// </summary>
        /// <param name="s"><see cref="ReadOnlySpan{Char}"/> containing characters to be checked for set equality.</param>
        /// <returns>true if the <see cref="UnicodeSet"/> object contains the same characters as <paramref name="s"/>; otherwise, false.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool SetEquals(ReadOnlySpan<char> s)
        {
            return s.Length == this.Count && Span(s, SpanCondition.Contained) == s.Length;
        }

        /// <summary>
        /// Checks if this and other contain the same elements. This is set equality: 
        /// duplicates and order are ignored
        /// </summary>
        /// <param name="collection">The collection to compare to the current <see cref="UnicodeSet"/> object.</param>
        /// <returns><c>true</c> if the sets contain the same elements; otherwise <c>false</c>.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public virtual bool SetEquals(IEnumerable<string> collection)
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
                var otherAsCollection = collection as ICollection<string>;
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
        public virtual UnicodeSet SymmetricExceptWith(IEnumerable<string> collection)
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

            var temp = new SortedSet<string>(collection, StringComparer.Ordinal);
            temp.ExceptWith(this);
            this.ExceptWith(collection);
            return this.UnionWith(temp);
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
