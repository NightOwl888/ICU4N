﻿// ICU4N TODO: These may or may not be useful. Most of this appears to be available in LINQ already.

//using J2N;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using JCG = J2N.Collections.Generic;
//using StringBuffer = System.Text.StringBuilder;

//namespace ICU4N.TestFramework.Dev.Util
//{
//    /// <summary>
//    /// Utilities that ought to be on collections, but aren't
//    /// </summary>
//    /// <internal>CLDR</internal>
//    public sealed class CollectionUtilities
//    {
//        //        /**
//        //     * Join an array of items.
//        //     * @param <T>
//        //     * @param array
//        //     * @param separator
//        //     * @return string
//        //     */
//        //        public static  string Join<T>(T[] array, string separator)
//        //        {
//        //            StringBuffer result = new StringBuffer();
//        //            for (int i = 0; i < array.Length; ++i)
//        //            {
//        //                if (i != 0) result.Append(separator);
//        //                result.Append(array[i]);
//        //            }
//        //            return result.ToString();
//        //        }

//        //        /**
//        //         * Join a collection of items.
//        //         * @param <T>
//        //         * @param collection 
//        //         * @param <U> 
//        //         * @param array
//        //         * @param separator
//        //         * @return string
//        //         */
//        //        public static string Join<T>(IEnumerable<T> collection, string separator) // ICU4N NOTE: Need to ensure T is a value type or string so sb.Append() works right
//        //        {
//        //            StringBuffer result = new StringBuffer();
//        //            bool first = true;
//        //            foreach (var item in collection)
//        //            {
//        //                if (first) first = false;
//        //                else result.Append(separator);
//        //                result.Append(item);
//        //            }
//        //            return result.ToString();
//        //        }

//        //        /**
//        //         * Utility like Arrays.asList()
//        //         * @param source 
//        //         * @param target 
//        //         * @param reverse 
//        //         * @param <T> 
//        //         * @return 
//        //         */
//        //        public static IDictionary<T, T> AsMap<T>(T[][] source, IDictionary<T, T> target, bool reverse)
//        //        {
//        //            int from = 0, to = 1;
//        //            if (reverse)
//        //            {
//        //                from = 1; to = 0;
//        //            }
//        //            for (int i = 0; i < source.Length; ++i)
//        //            {
//        //                target[source[i][from]]= source[i][to];
//        //            }
//        //            return target;
//        //        }

//        //        /**
//        //         * Add all items in iterator to target collection
//        //         * @param <T>
//        //         * @param <U>
//        //         * @param source
//        //         * @param target
//        //         * @return
//        //         */
//        //        public static U AddAll<T, U>(IEnumerator<T> source, U target) where U : ICollection<T>
//        //        {
//        //            while (source.MoveNext())
//        //            {
//        //                target.Add(source.Current);
//        //            }
//        //            return target; // for chaining
//        //        }

//        //        /**
//        //         * Get the size of an iterator (number of items in it).
//        //         * @param source
//        //         * @return
//        //         */
//        //        public static int Size(IEnu source)
//        //        {
//        //            int result = 0;
//        //            while (source.hasNext())
//        //            {
//        //                source.next();
//        //                ++result;
//        //            }
//        //            return result;
//        //        }


//        //        /**
//        //         * @param <T>
//        //         * @param source
//        //         * @return
//        //         */
//        //        public static <T> Map<T, T> asMap(T[][] source)
//        //        {
//        //            return asMap(source, new HashMap<T, T>(), false);
//        //        }

//        //        /**
//        //         * Utility that ought to be on Map
//        //         * @param m 
//        //         * @param itemsToRemove 
//        //         * @param <K> 
//        //         * @param <V> 
//        //         * @return map passed in
//        //         */
//        //        public static <K,V> Map<K, V> removeAll(Map<K, V> m, Collection<K> itemsToRemove)
//        //        {
//        //            for (Iterator it = itemsToRemove.iterator(); it.hasNext();)
//        //            {
//        //                Object item = it.next();
//        //                m.remove(item);
//        //            }
//        //            return m;
//        //        }

//        //        /**
//        //         * Get first item in collection, or null if there is none.
//        //         * @param <T>
//        //         * @param <U>
//        //         * @param c
//        //         * @return first item
//        //         */
//        //        public <T, U extends Collection<T>> T getFirst(U c)
//        //        {
//        //            Iterator<T> it = c.iterator();
//        //            if (!it.hasNext()) return null;
//        //            return it.next();
//        //        }

//        //        /**
//        //         * Get the "best" in collection. That is the least if direction is < 0, otherwise the greatest. The first is chosen if there are multiples.
//        //         * @param <T>
//        //         * @param <U>
//        //         * @param c
//        //         * @param comp
//        //         * @param direction
//        //         * @return
//        //         */
//        //        public static <T, U extends Collection<T>> T getBest(U c, Comparator<T> comp, int direction)
//        //        {
//        //            Iterator<T> it = c.iterator();
//        //            if (!it.hasNext()) return null;
//        //            T bestSoFar = it.next();
//        //            if (direction < 0)
//        //            {
//        //                while (it.hasNext())
//        //                {
//        //                    T item = it.next();
//        //                    int compValue = comp.compare(item, bestSoFar);
//        //                    if (compValue < 0)
//        //                    {
//        //                        bestSoFar = item;
//        //                    }
//        //                }
//        //            }
//        //            else
//        //            {
//        //                while (it.hasNext())
//        //                {
//        //                    T item = it.next();
//        //                    int compValue = comp.compare(item, bestSoFar);
//        //                    if (compValue > 0)
//        //                    {
//        //                        bestSoFar = item;
//        //                    }
//        //                }
//        //            }
//        //            return bestSoFar;
//        //        }

//        //        /**
//        //         * Matches item.
//        //         * @param <T>
//        //         */
//        //        public interface ObjectMatcher<T>
//        //        {
//        //            /**
//        //             * Must handle null, never throw exception
//        //             * @param o 
//        //             * @return 
//        //             */
//        //            boolean matches(T o);
//        //        }

//        //        /**
//        //         * Reverse a match
//        //         * @param <T>
//        //         */
//        //        public static class InverseMatcher<T> implements ObjectMatcher<T> {
//        //            ObjectMatcher<T> other;
//        //        /**
//        //         * @param toInverse
//        //         * @return
//        //         */
//        //        public ObjectMatcher set(ObjectMatcher toInverse)
//        //        {
//        //            other = toInverse;
//        //            return this;
//        //        }
//        //        public boolean matches(T value)
//        //        {
//        //            return !other.matches(value);
//        //        }
//        //    }

//        //    /**
//        //     * Remove matching items
//        //     * @param <T>
//        //     * @param <U>
//        //     * @param c
//        //     * @param f
//        //     * @return
//        //     */
//        //    public static <T, U extends Collection<T>> U removeAll(U c, ObjectMatcher<T> f)
//        //    {
//        //        for (Iterator<T> it = c.iterator(); it.hasNext();)
//        //        {
//        //            T item = it.next();
//        //            if (f.matches(item)) it.remove();
//        //        }
//        //        return c;
//        //    }

//        //    /**
//        //     * Retain matching items
//        //     * @param <T>
//        //     * @param <U>
//        //     * @param c
//        //     * @param f
//        //     * @return
//        //     */
//        //    public static <T, U extends Collection<T>> U retainAll(U c, ObjectMatcher<T> f)
//        //    {
//        //        for (Iterator<T> it = c.iterator(); it.hasNext();)
//        //        {
//        //            T item = it.next();
//        //            if (!f.matches(item)) it.remove();
//        //        }
//        //        return c;
//        //    }

//        //    /**
//        //     * @param a
//        //     * @param b
//        //     * @return
//        //     */
//        //    public static boolean containsSome(Collection a, Collection b)
//        //    {
//        //        // fast paths
//        //        if (a.size() == 0 || b.size() == 0) return false;
//        //        if (a == b) return true; // must test after size test.

//        //        if (a instanceof SortedSet && b instanceof SortedSet) {
//        //            SortedSet aa = (SortedSet)a;
//        //            SortedSet bb = (SortedSet)b;
//        //            Comparator bbc = bb.comparator();
//        //            Comparator aac = aa.comparator();
//        //            if (bbc == null && aac == null)
//        //            {
//        //                Iterator ai = aa.iterator();
//        //                Iterator bi = bb.iterator();
//        //                Comparable ao = (Comparable)ai.next(); // these are ok, since the sizes are != 0
//        //                Comparable bo = (Comparable)bi.next();
//        //                while (true)
//        //                {
//        //                    int rel = ao.compareTo(bo);
//        //                    if (rel < 0)
//        //                    {
//        //                        if (!ai.hasNext()) return false;
//        //                        ao = (Comparable)ai.next();
//        //                    }
//        //                    else if (rel > 0)
//        //                    {
//        //                        if (!bi.hasNext()) return false;
//        //                        bo = (Comparable)bi.next();
//        //                    }
//        //                    else
//        //                    {
//        //                        return true;
//        //                    }
//        //                }
//        //            }
//        //            else if (bbc.equals(a))
//        //            {
//        //                Iterator ai = aa.iterator();
//        //                Iterator bi = bb.iterator();
//        //                Object ao = ai.next(); // these are ok, since the sizes are != 0
//        //                Object bo = bi.next();
//        //                while (true)
//        //                {
//        //                    int rel = aac.compare(ao, bo);
//        //                    if (rel < 0)
//        //                    {
//        //                        if (!ai.hasNext()) return false;
//        //                        ao = ai.next();
//        //                    }
//        //                    else if (rel > 0)
//        //                    {
//        //                        if (!bi.hasNext()) return false;
//        //                        bo = bi.next();
//        //                    }
//        //                    else
//        //                    {
//        //                        return true;
//        //                    }
//        //                }
//        //            }
//        //        }
//        //        for (Iterator it = a.iterator(); it.hasNext();)
//        //        {
//        //            if (b.contains(it.next())) return true;
//        //        }
//        //        return false;
//        //    }

//        //    public static boolean containsAll(Collection a, Collection b)
//        //    {
//        //        // fast paths
//        //        if (a == b) return true;
//        //        if (b.size() == 0) return true;
//        //        if (a.size() < b.size()) return false;

//        //        if (a instanceof SortedSet && b instanceof SortedSet) {
//        //            SortedSet aa = (SortedSet)a;
//        //            SortedSet bb = (SortedSet)b;
//        //            Comparator bbc = bb.comparator();
//        //            Comparator aac = aa.comparator();
//        //            if (bbc == null && aac == null)
//        //            {
//        //                Iterator ai = aa.iterator();
//        //                Iterator bi = bb.iterator();
//        //                Comparable ao = (Comparable)ai.next(); // these are ok, since the sizes are != 0
//        //                Comparable bo = (Comparable)bi.next();
//        //                while (true)
//        //                {
//        //                    int rel = ao.compareTo(bo);
//        //                    if (rel == 0)
//        //                    {
//        //                        if (!bi.hasNext()) return true;
//        //                        if (!ai.hasNext()) return false;
//        //                        bo = (Comparable)bi.next();
//        //                        ao = (Comparable)ai.next();
//        //                    }
//        //                    else if (rel < 0)
//        //                    {
//        //                        if (!ai.hasNext()) return false;
//        //                        ao = (Comparable)ai.next();
//        //                    }
//        //                    else
//        //                    {
//        //                        return false;
//        //                    }
//        //                }
//        //            }
//        //            else if (bbc.equals(aac))
//        //            {
//        //                Iterator ai = aa.iterator();
//        //                Iterator bi = bb.iterator();
//        //                Object ao = ai.next(); // these are ok, since the sizes are != 0
//        //                Object bo = bi.next();
//        //                while (true)
//        //                {
//        //                    int rel = aac.compare(ao, bo);
//        //                    if (rel == 0)
//        //                    {
//        //                        if (!bi.hasNext()) return true;
//        //                        if (!ai.hasNext()) return false;
//        //                        bo = bi.next();
//        //                        ao = ai.next();
//        //                    }
//        //                    else if (rel < 0)
//        //                    {
//        //                        if (!ai.hasNext()) return false;
//        //                        ao = ai.next();
//        //                    }
//        //                    else
//        //                    {
//        //                        return false;
//        //                    }
//        //                }
//        //            }
//        //        }
//        //        return a.containsAll(b);
//        //    }

//        //    public static boolean containsNone(Collection a, Collection b)
//        //    {
//        //        return !containsSome(a, b);
//        //    }

//        //    /**
//        //     * Used for results of getContainmentRelation
//        //     */
//        //    public static final int
//        //    ALL_EMPTY = 0,
//        //    NOT_A_SUPERSET_B = 1,
//        //    NOT_A_DISJOINT_B = 2,
//        //    NOT_A_SUBSET_B = 4,
//        //    NOT_A_EQUALS_B = NOT_A_SUBSET_B | NOT_A_SUPERSET_B,
//        //    A_PROPER_SUBSET_OF_B = NOT_A_DISJOINT_B | NOT_A_SUPERSET_B,
//        //    A_PROPER_SUPERSET_B = NOT_A_SUBSET_B | NOT_A_DISJOINT_B,
//        //    A_PROPER_OVERLAPS_B = NOT_A_SUBSET_B | NOT_A_DISJOINT_B | NOT_A_SUPERSET_B;

//        //    /**
//        //     * Assesses all the possible containment relations between collections A and B with one call.<br>
//        //     * Returns an int with bits set, according to a "Venn Diagram" view of A vs B.<br>
//        //     * NOT_A_SUPERSET_B: a - b != {}<br>
//        //     * NOT_A_DISJOINT_B: a * b != {}  // * is intersects<br>
//        //     * NOT_A_SUBSET_B: b - a != {}<br>
//        //     * Thus the bits can be used to get the following relations:<br>
//        //     * for A_SUPERSET_B, use (x & CollectionUtilities.NOT_A_SUPERSET_B) == 0<br>
//        //     * for A_SUBSET_B, use (x & CollectionUtilities.NOT_A_SUBSET_B) == 0<br>
//        //     * for A_EQUALS_B, use (x & CollectionUtilities.NOT_A_EQUALS_B) == 0<br>
//        //     * for A_DISJOINT_B, use (x & CollectionUtilities.NOT_A_DISJOINT_B) == 0<br>
//        //     * for A_OVERLAPS_B, use (x & CollectionUtilities.NOT_A_DISJOINT_B) != 0<br>
//        //     */
//        //    public static int getContainmentRelation(Collection a, Collection b)
//        //    {
//        //        if (a.size() == 0)
//        //        {
//        //            return (b.size() == 0) ? ALL_EMPTY : NOT_A_SUPERSET_B;
//        //        }
//        //        else if (b.size() == 0)
//        //        {
//        //            return NOT_A_SUBSET_B;
//        //        }
//        //        int result = 0;
//        //        // WARNING: one might think that the following can be short-circuited, by looking at
//        //        // the sizes of a and b. However, this would fail in general, where a different comparator is being
//        //        // used in the two collections. Unfortunately, there is no failsafe way to test for that.
//        //        for (Iterator it = a.iterator(); result != 6 && it.hasNext();)
//        //        {
//        //            result |= (b.contains(it.next())) ? NOT_A_DISJOINT_B : NOT_A_SUBSET_B;
//        //        }
//        //        for (Iterator it = b.iterator(); (result & 3) != 3 && it.hasNext();)
//        //        {
//        //            result |= (a.contains(it.next())) ? NOT_A_DISJOINT_B : NOT_A_SUPERSET_B;
//        //        }
//        //        return result;
//        //    }

//        //    public static String remove(String source, UnicodeSet removals)
//        //    {
//        //        StringBuffer result = new StringBuffer();
//        //        int cp;
//        //        for (int i = 0; i < source.length(); i += UTF16.getCharCount(cp))
//        //        {
//        //            cp = UTF16.charAt(source, i);
//        //            if (!removals.contains(cp)) UTF16.append(result, cp);
//        //        }
//        //        return result.toString();
//        //    }

//        //    /**
//        //     * Does one string contain another, starting at a specific offset?
//        //     * @param text
//        //     * @param offset
//        //     * @param other
//        //     * @return
//        //     */
//        //    public static int matchesAt(CharSequence text, int offset, CharSequence other)
//        //    {
//        //        int len = other.length();
//        //        int i = 0;
//        //        int j = offset;
//        //        for (; i < len; ++i, ++j)
//        //        {
//        //            char pc = other.charAt(i);
//        //            char tc = text.charAt(j);
//        //            if (pc != tc) return -1;
//        //        }
//        //        return i;
//        //    }

//        //    /**
//        //     * Returns the ending offset found by matching characters with testSet, until a position is found that doen't match
//        //     * @param string
//        //     * @param offset
//        //     * @param testSet
//        //     * @return
//        //     */
//        //    public int span(CharSequence string, int offset, UnicodeSet testSet)
//        //    {
//        //        while (true)
//        //        {
//        //            int newOffset = testSet.matchesAt(string, offset);
//        //            if (newOffset < 0) return offset;
//        //        }
//        //    }

//        //    /**
//        //     * Returns the ending offset found by matching characters with testSet, until a position is found that does match
//        //     * @param string
//        //     * @param offset
//        //     * @param testSet
//        //     * @return
//        //     */
//        //    public int spanNot(CharSequence string, int offset, UnicodeSet testSet)
//        //    {
//        //        while (true)
//        //        {
//        //            int newOffset = testSet.matchesAt(string, offset);
//        //            if (newOffset >= 0) return offset;
//        //            ++offset; // try next character position
//        //            // we don't have to worry about surrogates for this.
//        //        }
//        //    }

//        //    /**
//        //     * Modifies Unicode set to flatten the strings. Eg [abc{da}] => [abcd]
//        //     * Returns the set for chaining.
//        //     * @param exemplar1
//        //     * @return
//        //     */
//        //    public static UnicodeSet flatten(UnicodeSet exemplar1)
//        //    {
//        //        UnicodeSet result = new UnicodeSet();
//        //        boolean gotString = false;
//        //        for (UnicodeSetIterator it = new UnicodeSetIterator(exemplar1); it.nextRange();)
//        //        {
//        //            if (it.codepoint == UnicodeSetIterator.IS_STRING)
//        //            {
//        //                result.addAll(it.string);
//        //                gotString = true;
//        //            }
//        //            else
//        //            {
//        //                result.add(it.codepoint, it.codepointEnd);
//        //            }
//        //        }
//        //        if (gotString) exemplar1.set(result);
//        //        return exemplar1;
//        //    }

//        //    /**
//        //     * For producing filtered iterators
//        //     */
//        //    public static abstract class FilteredIterator implements Iterator
//        //    {
//        //        private Iterator baseIterator;
//        //    private static final Object EMPTY = new Object();
//        //    private static final Object DONE = new Object();
//        //    private Object nextObject = EMPTY;
//        //    public FilteredIterator set(Iterator baseIterator)
//        //    {
//        //        this.baseIterator = baseIterator;
//        //        return this;
//        //    }
//        //    public void remove()
//        //    {
//        //        throw new UnsupportedOperationException("Doesn't support removal");
//        //    }
//        //    public Object next()
//        //    {
//        //        Object result = nextObject;
//        //        nextObject = EMPTY;
//        //        return result;
//        //    }
//        //    public boolean hasNext()
//        //    {
//        //        if (nextObject == DONE) return false;
//        //        if (nextObject != EMPTY) return true;
//        //        while (baseIterator.hasNext())
//        //        {
//        //            nextObject = baseIterator.next();
//        //            if (isIncluded(nextObject))
//        //            {
//        //                return true;
//        //            }
//        //        }
//        //        nextObject = DONE;
//        //        return false;
//        //    }
//        //    abstract public boolean isIncluded(Object item);
//        //}

//        //public static class PrefixIterator extends FilteredIterator
//        //{
//        //        private String prefix;
//        //public PrefixIterator set(Iterator baseIterator, String prefix)
//        //{
//        //    super.set(baseIterator);
//        //    this.prefix = prefix;
//        //    return this;
//        //}
//        //public boolean isIncluded(Object item)
//        //{
//        //    return ((String)item).StartsWith(prefix, StringComparison.Ordinal);
//        //}
//        //    }

//        //    public static class RegexIterator extends FilteredIterator
//        //{
//        //        private Matcher matcher;
//        //public RegexIterator set(Iterator baseIterator, Matcher matcher)
//        //{
//        //    super.set(baseIterator);
//        //    this.matcher = matcher;
//        //    return this;
//        //}
//        //public boolean isIncluded(Object item)
//        //{
//        //    return matcher.reset((String)item).matches();
//        //}
//        //    }

//        //    /**
//        //     * Compare, allowing nulls
//        //     * @param a
//        //     * @param b
//        //     * @return
//        //     */
//        //    public static <T> boolean equals(T a, T b)
//        //{
//        //    return a == null
//        //            ? b == null
//        //            : b == null ? false : a.equals(b);
//        //}

//        ////**
//        // * Compare, allowing nulls and putting them first
//        // * @param a
//        // * @param b
//        // * @return
//        // */
//        //public static <T extends Comparable> int compare(T a, T b)
//        //{
//        //    return a == null
//        //            ? b == null ? 0 : -1
//        //                    : b == null ? 1 : a.compareTo(b);
//        //}

//        public static int Compare<T>(IEnumerator<T> iterator1, IEnumerator<T> iterator2) where T: IComparable<T>
//        {
//            int diff;
//            while (iterator1.MoveNext())
//            {
//                if (!iterator2.MoveNext())
//                    return -1;

//                diff = J2N.Collections.Generic.Comparer<T>.Default.Compare(iterator1.Current, iterator2.Current);
//                if (diff != 0) return diff;
//            }
//            return iterator2.MoveNext() ? 1 : 0;
//        }

//        ////**
//        // * Compare iterators
//        // * @param iterator1
//        // * @param iterator2
//        // * @return
//        // */
//        //public static <T extends Comparable> int compare(Iterator<T> iterator1, Iterator<T> iterator2)
//        //{
//        //    int diff;
//        //    while (true)
//        //    {
//        //        if (!iterator1.hasNext())
//        //        {
//        //            return iterator2.hasNext() ? -1 : 0;
//        //        }
//        //        else if (!iterator2.hasNext())
//        //        {
//        //            return 1;
//        //        }
//        //        diff = CollectionUtilities.compare(iterator1.next(), iterator2.next());
//        //        if (diff != 0)
//        //        {
//        //            return diff;
//        //        }
//        //    }
//        //}

//        public static int Compare<T, U>(U o1, U o2)
//            where T : IComparable<T>
//            where U : ICollection<T>
//        {
//            int diff = o1.Count - o2.Count;
//            if (diff != 0)
//                return diff;

//            var iterator1 = o1.GetEnumerator();
//            var iterator2 = o2.GetEnumerator();
//            return Compare(iterator1, iterator2);
//        }

//        ////**
//        // * Compare, with shortest first, and otherwise lexicographically
//        // * @param a
//        // * @param b
//        // * @return
//        // */
//        //public static <T extends Comparable, U extends Collection<T>> int compare(U o1, U o2)
//        //{
//        //    int diff = o1.size() - o2.size();
//        //    if (diff != 0)
//        //    {
//        //        return diff;
//        //    }
//        //    Iterator<T> iterator1 = o1.iterator();
//        //    Iterator<T> iterator2 = o2.iterator();
//        //    return compare(iterator1, iterator2);
//        //}

//        public static int Compare<T, U>(U o1, U o2)
//            where T : IComparable<T>
//            where U : ISet<T>
//        {
//            int diff = o1.Count - o2.Count;
//            if (diff != 0) return diff;

//            var t1 = o1.GetType();
//            var t2 = o2.GetType();
//            ICollection<T> x1 = t1.ImplementsGenericInterface(typeof(ISet<>)) && t1.Name.StartsWith("SortedSet", StringComparison.Ordinal) ? (ICollection<T>)o1 : new JCG.SortedSet<T>(o1);
//            ICollection<T> x2 = t1.ImplementsGenericInterface(typeof(ISet<>)) && t2.Name.StartsWith("SortedSet", StringComparison.Ordinal) ? (ICollection<T>)o2 : new JCG.SortedSet<T>(o2);
//            return Compare(x1, x2);
//        }

//        ////**
//        // * Compare, with shortest first, and otherwise lexicographically
//        // * @param a
//        // * @param b
//        // * @return
//        // */
//        //public static <T extends Comparable, U extends Set<T>> int compare(U o1, U o2)
//        //{
//        //    int diff = o1.size() - o2.size();
//        //    if (diff != 0)
//        //    {
//        //        return diff;
//        //    }
//        //    Collection<T> x1 = SortedSet.class.isInstance(o1) ? o1 : new TreeSet<T>(o1);
//        //        Collection<T> x2 = SortedSet.class.isInstance(o2) ? o2 : new TreeSet<T>(o2);
//        //        return compare(x1, x2);
//        //    }

//        //    public static class SetComparator<T extends Comparable>
//        //    implements Comparator<Set<T>> {
//        //    public int compare(Set<T> o1, Set<T> o2)
//        //    {
//        //        return CollectionUtilities.compare(o1, o2);
//        //    }
//        //};

//        public class CollectionComparer<T> : IComparer<ICollection<T>> where T : IComparable<T>
//        {
//            public int Compare(ICollection<T> o1, ICollection<T> o2)
//            {
//                return Compare(o1, o2);
//            }
//        }

//        public class SetComparer<T> : IComparer<ISet<T>> where T : IComparable<T>
//        {
//            public int Compare(ISet<T> x, ISet<T> y)
//            {
//                throw new NotImplementedException();
//            }
//        }

//        //public static class CollectionComparator<T extends Comparable>
//        //    implements Comparator<Collection<T>> {
//        //    public int compare(Collection<T> o1, Collection<T> o2)
//        //    {
//        //        return CollectionUtilities.compare(o1, o2);
//        //    }
//        //};

//        ////**
//        // * Compare, allowing nulls and putting them first
//        // * @param a
//        // * @param b
//        // * @return
//        // */
//        //public static <K extends Comparable, V extends Comparable, T extends Entry<K, V>> int compare(T a, T b)
//        //{
//        //    if (a == null)
//        //    {
//        //        return b == null ? 0 : -1;
//        //    }
//        //    else if (b == null)
//        //    {
//        //        return 1;
//        //    }
//        //    int diff = compare(a.getKey(), b.getKey());
//        //    if (diff != 0)
//        //    {
//        //        return diff;
//        //    }
//        //    return compare(a.getValue(), b.getValue());
//        //}

//        //public static <K extends Comparable, V extends Comparable, T extends Entry<K, V>> int compareEntrySets(Collection<T> o1, Collection<T> o2)
//        //{
//        //    int diff = o1.size() - o2.size();
//        //    if (diff != 0)
//        //    {
//        //        return diff;
//        //    }
//        //    Iterator<T> iterator1 = o1.iterator();
//        //    Iterator<T> iterator2 = o2.iterator();
//        //    while (true)
//        //    {
//        //        if (!iterator1.hasNext())
//        //        {
//        //            return iterator2.hasNext() ? -1 : 0;
//        //        }
//        //        else if (!iterator2.hasNext())
//        //        {
//        //            return 1;
//        //        }
//        //        T item1 = iterator1.next();
//        //        T item2 = iterator2.next();
//        //        diff = CollectionUtilities.compare(item1, item2);
//        //        if (diff != 0)
//        //        {
//        //            return diff;
//        //        }
//        //    }
//        //}

//        //public static class MapComparator<K extends Comparable, V extends Comparable> implements Comparator<Map<K, V>> {
//        //    public int compare(Map<K, V> o1, Map<K, V> o2)
//        //    {
//        //        return CollectionUtilities.compareEntrySets(o1.entrySet(), o2.entrySet());
//        //    }
//        //};

//        //public static class ComparableComparator<T extends Comparable> implements Comparator<T> {
//        //        public int compare(T arg0, T arg1)
//        //{
//        //    return CollectionUtilities.compare(arg0, arg1);
//        //}
//        //    }
//    }
//}
