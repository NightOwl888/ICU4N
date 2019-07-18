using ICU4N.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.Text
{
    /// <summary>
    /// ICU4J compatible method names, in case you don't like the rewiring of ICU4N.
    /// </summary>
    public static partial class UnicodeSetExtensions
    {
        /// <summary>
        /// Add a collection (as strings) into this <see cref="UnicodeSet"/>.
        /// Uses standard naming convention.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="source">Source collection to add into.</param>
        /// <returns>A reference to this object.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        // ICU4N specific overload to properly convert char array to string
        public static UnicodeSet AddAll(this UnicodeSet set, IEnumerable<char[]> source)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.AddAll(source);
        }

        /// <summary>
        /// Add a collection (as strings) into this <see cref="UnicodeSet"/>.
        /// Uses standard naming convention.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="source">Source collection to add into.</param>
        /// <returns>A reference to this object.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        // ICU4N specific overload to optimize for string
        public static UnicodeSet AddAll(this UnicodeSet set, IEnumerable<string> source)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.AddAll(source);
        }

        /// <summary>
        /// Adds all characters in range (uses preferred naming convention).
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="start">The index of where to start on adding all characters.</param>
        /// <param name="end">The index of where to end on adding all characters.</param>
        /// <returns>A reference to this object.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet AddAll(this UnicodeSet set, int start, int end)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.AddAll(start, end);
        }

        /// <summary>
        /// Add the contents of the UnicodeSet (as strings) into a collection.
        /// </summary>
        /// <typeparam name="T">Collection type.</typeparam>
        /// <param name="set">This set.</param>
        /// <param name="target">Collection to add into.</param>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static T AddAllTo<T>(this UnicodeSet set, T target) where T : ICollection<string>
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.AddAllTo(target);
        }

        ///// <summary>
        ///// Utility for adding the contents of an enumerable to a collection.
        ///// </summary>
        ///// <typeparam name="T">The source element type.</typeparam>
        ///// <typeparam name="U">The target type (must implement <see cref="ICollection{T}"/>).</typeparam>
        ///// <draft>ICU4N 60.1</draft>
        ///// <provisional>This API might change or be removed in a future release.</provisional>
        //public static U AddAllTo<T, U>(this UnicodeSet set, IEnumerable<T> source, U target) where U : ICollection<T>
        //{
        //    if (set == null)
        //        throw new ArgumentNullException(nameof(set));
        //    return set.AddAllTo<T, U>(source, target);
        //}


        ///// <summary>
        ///// Utility for adding the contents of an enumerable to a collection.
        ///// </summary>
        ///// <typeparam name="T">The type of items to add.</typeparam>
        ///// <draft>ICU4N 60.1</draft>
        ///// <provisional>This API might change or be removed in a future release.</provisional>
        //public static T[] AddAllTo<T>(this UnicodeSet set, IEnumerable<T> source, T[] target)
        //{
        //    if (set == null)
        //        throw new ArgumentNullException(nameof(set));
        //    return set.AddAllTo<T>(source, target);
        //}

        /// <summary>
        /// Complements the specified range in this set.  Any character in
        /// the range will be removed if it is in this set, or will be
        /// added if it is not in this set.  If <c><paramref name="end"/> &gt; <paramref name="start"/></c>
        /// then an empty range is complemented, leaving the set unchanged.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="start">First character, inclusive, of range to be removed from this set.</param>
        /// <param name="end">Last character, inclusive, of range to be removed from this set.</param>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet Complement(this UnicodeSet set,  int start, int end)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.Complement(start, end);
        }

        /// <summary>
        /// Complements the specified character in this set.  The character
        /// will be removed if it is in this set, or will be added if it is
        /// not in this set.
        /// </summary>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet Complement(this UnicodeSet set, int c)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.Complement(c);
        }

        /// <summary>
        /// Complements in this set all elements contained in the specified
        /// set.  Any character in the other set will be removed if it is
        /// in this set, or will be added if it is not in this set.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="c">Set that defines which elements will be complemented from
        /// this set.</param>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet ComplementAll(this UnicodeSet set, UnicodeSet c)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ComplementAll(c);
        }

        /// <summary>
        /// Returns true if this set contains all the characters and strings
        /// of the given set.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="b">Set to be checked for containment.</param>
        /// <returns>true if the test condition is met.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static bool ContainsAll(this UnicodeSet set, UnicodeSet b) 
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ContainsAll(b);
        }

        /// <summary>
        /// Returns true if there is a partition of the string such that this set contains each of the partitioned strings.
        /// For example, for the Unicode set [a{bc}{cd}]
        /// <list type="bullet">
        ///     <item><description><see cref="ContainsAll(UnicodeSet, string)"/> is true for each of: "a", "bc", ""cdbca"</description></item>
        ///     <item><description><see cref="ContainsAll(UnicodeSet, string)"/> is false for each of: "acb", "bcda", "bcx"</description></item>
        /// </list>
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">String containing characters to be checked for containment.</param>
        /// <returns>true if the test condition is met.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static bool ContainsAll(this UnicodeSet set, string s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ContainsAll(s);
        }

        /// <summary>
        /// Returns true if this set contains one or more of the characters
        /// in the given range.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="start">First character, inclusive, of the range.</param>
        /// <param name="end">Last character, inclusive, of the range.</param>
        /// <returns>true if the condition is met.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static bool ContainsSome(this UnicodeSet set, int start, int end)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ContainsSome(start, end);
        }

        /// <summary>
        /// Returns true if this set contains one or more of the characters
        /// and strings of the given set.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="s">Set to be checked for containment.</param>
        /// <returns>True if the condition is met.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static bool ContainsSome(this UnicodeSet set, UnicodeSet s)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.ContainsSome(s);
        }

        /// <summary>
        /// Removes from this set all of its elements that are contained in the
        /// specified set.  This operation effectively modifies this
        /// set so that its value is the <i>asymmetric set difference</i> of
        /// the two sets.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="c">Set that defines which elements will be removed from
        /// this set.</param>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet RemoveAll(this UnicodeSet set, UnicodeSet c)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.RemoveAll(c);
        }

        /// <summary>
        /// Remove all strings from this <see cref="UnicodeSet"/>
        /// </summary>
        /// <param name="set">This set.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet RemoveAllStrings(this UnicodeSet set)
        {
            return set.RemoveAllStrings();
        }

        /// <summary>
        /// Retain only the elements in this set that are contained in the
        /// specified range.  If <c><paramref name="end"/> &gt; <paramref name="start"/></c> 
        /// then an empty range is retained, leaving the set empty.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="start">First character, inclusive, of range to be retained
        /// to this set.</param>
        /// <param name="end">Last character, inclusive, of range to be retained
        /// to this set.</param>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet Retain(this UnicodeSet set, int start, int end)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.Retain(start, end);
        }

        /// <summary>
        /// Retain the specified character from this set if it is present.
        /// Upon return this set will be empty if it did not contain <paramref name="c"/>, or
        /// will only contain c if it did contain <paramref name="c"/>.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="c">The character to be retained.</param>
        /// <returns>This object, for chaining.</returns>
        /// <draft>ICU4N 60.1</draft>
        /// <provisional>This API might change or be removed in a future release.</provisional>
        public static UnicodeSet Retain(this UnicodeSet set, int c)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.Retain(c);
        }

        /// <summary>
        /// Retains only the elements in this set that are contained in the
        /// specified set.  In other words, removes from this set all of
        /// its elements that are not contained in the specified set.  This
        /// operation effectively modifies this set so that its value is
        /// the <i>intersection</i> of the two sets.
        /// </summary>
        /// <param name="set">This set.</param>
        /// <param name="c">Set that defines which elements this set will retain.</param>
        /// <stable>ICU 2.0</stable>
        public static UnicodeSet RetainAll(this UnicodeSet set, UnicodeSet c)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));
            return set.RetainAll(c);
        }
    }
}
