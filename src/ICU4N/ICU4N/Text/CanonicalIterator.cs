using ICU4N.Impl;
using ICU4N.Lang;
using ICU4N.Support.Text;
using System;
using System.Collections.Generic;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    public sealed class CanonicalIterator
    {
        /**
     * Construct a CanonicalIterator object
     * @param source string to get results for
     * @stable ICU 2.4
     */
        public CanonicalIterator(string source)
        {
            Norm2AllModes allModes = Norm2AllModes.GetNFCInstance();
            nfd = allModes.Decomp;
            nfcImpl = allModes.Impl.EnsureCanonIterData();
            SetSource(source);
        }

        /**
         * Gets the NFD form of the current source we are iterating over.
         * @return gets the source: NOTE: it is the NFD form of the source originally passed in
         * @stable ICU 2.4
         */
        public string Source
        {
            get { return source; }
        }

        /**
         * Resets the iterator so that one can start again from the beginning.
         * @stable ICU 2.4
         */
        public void Reset()
        {
            done = false;
            for (int i = 0; i < current.Length; ++i)
            {
                current[i] = 0;
            }
        }

        /**
         * Get the next canonically equivalent string.
         * <br><b>Warning: The strings are not guaranteed to be in any particular order.</b>
         * @return the next string that is canonically equivalent. The value null is returned when
         * the iteration is done.
         * @stable ICU 2.4
         */
        public string Next()
        {
            if (done) return null;

            // construct return value

            buffer.Length = 0; // delete old contents
            for (int i = 0; i < pieces.Length; ++i)
            {
                buffer.Append(pieces[i][current[i]]);
            }
            string result = buffer.ToString();

            // find next value for next time

            for (int i = current.Length - 1; ; --i)
            {
                if (i < 0)
                {
                    done = true;
                    break;
                }
                current[i]++;
                if (current[i] < pieces[i].Length) break; // got sequence
                current[i] = 0;
            }
            return result;
        }

        /**
         * Set a new source for this iterator. Allows object reuse.
         * @param newSource the source string to iterate against. This allows the same iterator to be used
         * while changing the source string, saving object creation.
         * @stable ICU 2.4
         */
        public void SetSource(String newSource)
        {
            source = nfd.Normalize(newSource);
            done = false;

            // catch degenerate case
            if (newSource.Length == 0)
            {
                pieces = new String[1][];
                current = new int[1];
                pieces[0] = new String[] { "" };
                return;
            }

            // find the segments
            IList<string> segmentList = new List<string>();
            int cp;
            int start = 0;

            // i should be the end of the first code point
            // break up the string into segements

            int i = UTF16.FindOffsetFromCodePoint(source, 1);

            for (; i < source.Length; i += Character.CharCount(cp))
            {
                cp = source.CodePointAt(i);
                if (nfcImpl.IsCanonSegmentStarter(cp))
                {
                    segmentList.Add(source.Substring(start, i - start)); // add up to i
                    start = i;
                }
            }
            segmentList.Add(source.Substring(start, i - start)); // add last one

            // allocate the arrays, and find the strings that are CE to each segment
            pieces = new String[segmentList.Count][];
            current = new int[segmentList.Count];
            for (i = 0; i < pieces.Length; ++i)
            {
                if (PROGRESS) Console.Out.WriteLine("SEGMENT");
                pieces[i] = GetEquivalents(segmentList[i]);
            }
        }

        /**
         * Simple implementation of permutation.
         * <br><b>Warning: The strings are not guaranteed to be in any particular order.</b>
         * @param source the string to find permutations for
         * @param skipZeros set to true to skip characters with canonical combining class zero
         * @param output the set to add the results to
         * @internal
         * @deprecated This API is ICU internal only.
         */
        [Obsolete("This API is ICU internal only.")]
        public static void Permute(string source, bool skipZeros, ISet<string> output)
        {
            // TODO: optimize
            //if (PROGRESS) System.out.println("Permute: " + source);

            // optimization:
            // if zero or one character, just return a set with it
            // we check for length < 2 to keep from counting code points all the time
            if (source.Length <= 2 && UTF16.CountCodePoint(source) <= 1)
            {
                output.Add(source);
                return;
            }

            // otherwise iterate through the string, and recursively permute all the other characters
            ISet<string> subpermute = new HashSet<string>();
            int cp;
            for (int i = 0; i < source.Length; i += UTF16.GetCharCount(cp))
            {
                cp = UTF16.CharAt(source, i);

                // optimization:
                // if the character is canonical combining class zero,
                // don't permute it
                if (skipZeros && i != 0 && UCharacter.GetCombiningClass(cp) == 0)
                {
                    //System.out.println("Skipping " + Utility.hex(UTF16.valueOf(source, i)));
                    continue;
                }

                // see what the permutations of the characters before and after this one are
                subpermute.Clear();
                Permute(source.Substring(0, i - 0)
                    + source.Substring(i + UTF16.GetCharCount(cp)), skipZeros, subpermute);

                // prefix this character to all of them
                string chStr = UTF16.ValueOf(source, i);
                foreach (string s in subpermute)
                {
                    string piece = chStr + s;
                    //if (PROGRESS) System.out.println("  Piece: " + piece);
                    output.Add(piece);
                }
            }
        }

        // FOR TESTING

        /*
         *@return the set of "safe starts", characters that are class zero AND are never non-initial in a decomposition.
         *
        public static UnicodeSet getSafeStart() {
            return (UnicodeSet) SAFE_START.clone();
        }
        */
        /*
         *@return the set of characters whose decompositions start with the given character
         *
        public static UnicodeSet getStarts(int cp) {
            UnicodeSet result = AT_START.get(cp);
            if (result == null) result = EMPTY;
            return (UnicodeSet) result.clone();
        }
        */

        // ===================== PRIVATES ==============================

        // debug
        private static bool PROGRESS = false; // debug progress
                                              //private static Transliterator NAME = PROGRESS ? Transliterator.getInstance("name") : null;
        private static bool SKIP_ZEROS = true;

        // fields
        private readonly Normalizer2 nfd;
        private readonly Normalizer2Impl nfcImpl;
        private string source;
        private bool done;
        private String[][] pieces;
        private int[] current;
        // Note: C will need two more fields, since arrays there don't have lengths
        // int pieces_length;
        // int[] pieces_lengths;

        // transient fields
        private StringBuilder buffer = new StringBuilder();


        // we have a segment, in NFD. Find all the strings that are canonically equivalent to it.
        private string[] GetEquivalents(String segment)
        {
            ISet<string> result = new HashSet<string>();
            ISet<string> basic = GetEquivalents2(segment);
            ISet<string> permutations = new HashSet<string>();

            // now get all the permutations
            // add only the ones that are canonically equivalent
            // TODO: optimize by not permuting any class zero.
            using (IEnumerator<string> it = basic.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    String item = it.Current;
                    permutations.Clear();
                    Permute(item, SKIP_ZEROS, permutations);
                    using (IEnumerator<string> it2 = permutations.GetEnumerator())
                    {
                        while (it2.MoveNext())
                        {
                            string possible = it2.Current;

                            /*
                                            String attempt = Normalizer.normalize(possible, Normalizer.DECOMP, 0);
                                            if (attempt.equals(segment)) {
                            */
                            if (Normalizer.Compare(possible, segment, 0) == 0)
                            {

                                if (PROGRESS) Console.Out.WriteLine("Adding Permutation: " + Utility.Hex(possible));
                                result.Add(possible);

                            }
                            else
                            {
                                if (PROGRESS) Console.Out.WriteLine("-Skipping Permutation: " + Utility.Hex(possible));
                            }
                        }
                    }
                }
            }

            // convert into a String[] to clean up storage
            string[] finalResult = new string[result.Count];
            result.CopyTo(finalResult, 0);
            return finalResult;
        }


        private ISet<string> GetEquivalents2(string segment)
        {

            ISet<string> result = new HashSet<string>();

            if (PROGRESS) Console.Out.WriteLine("Adding: " + Utility.Hex(segment));

            result.Add(segment);
            StringBuffer workingBuffer = new StringBuffer();
            UnicodeSet starts = new UnicodeSet();

            // cycle through all the characters
            int cp;
            for (int i = 0; i < segment.Length; i += Character.CharCount(cp))
            {

                // see if any character is at the start of some decomposition
                cp = segment.CodePointAt(i);
                if (!nfcImpl.GetCanonStartSet(cp, starts))
                {
                    continue;
                }
                // if so, see which decompositions match
                for (UnicodeSetIterator iter = new UnicodeSetIterator(starts); iter.Next();)
                {
                    int cp2 = iter.Codepoint;
                    ISet<string> remainder = Extract(cp2, segment, i, workingBuffer);
                    if (remainder == null)
                    {
                        continue;
                    }

                    // there were some matches, so add all the possibilities to the set.
                    string prefix = segment.Substring(0, i - 0);
                    prefix += UTF16.ValueOf(cp2);
                    foreach (string item in remainder)
                    {
                        result.Add(prefix + item);
                    }
                }
            }
            return result;
            /*
            Set result = new HashSet();
            if (PROGRESS) System.out.println("Adding: " + NAME.transliterate(segment));
            result.add(segment);
            StringBuffer workingBuffer = new StringBuffer();

            // cycle through all the characters
            int cp;

            for (int i = 0; i < segment.length(); i += UTF16.getCharCount(cp)) {
                // see if any character is at the start of some decomposition
                cp = UTF16.charAt(segment, i);
                NormalizerImpl.getCanonStartSet(c,fillSet)
                UnicodeSet starts = AT_START.get(cp);
                if (starts == null) continue;
                UnicodeSetIterator usi = new UnicodeSetIterator(starts);
                // if so, see which decompositions match
                while (usi.next()) {
                    int cp2 = usi.codepoint;
                    // we know that there are no strings in it
                    // so we don't have to check CharacterIterator.IS_STRING
                    Set remainder = extract(cp2, segment, i, workingBuffer);
                    if (remainder == null) continue;

                    // there were some matches, so add all the possibilities to the set.
                    String prefix = segment.substring(0, i) + UTF16.valueOf(cp2);
                    Iterator it = remainder.iterator();
                    while (it.hasNext()) {
                        String item = (String) it.next();
                        if (PROGRESS) System.out.println("Adding: " + NAME.transliterate(prefix + item));
                        result.add(prefix + item);
                    }
                }
            }
            return result;
            */
        }

        /**
         * See if the decomposition of cp2 is at segment starting at segmentPos
         * (with canonical rearrangment!)
         * If so, take the remainder, and return the equivalents
         */
        private ISet<string> Extract(int comp, string segment, int segmentPos, StringBuffer buf)
        {
            if (PROGRESS) Console.Out.WriteLine(" extract: " + Utility.Hex(UTF16.ValueOf(comp))
                + ", " + Utility.Hex(segment.Substring(segmentPos)));

            string decomp = nfcImpl.GetDecomposition(comp);
            if (decomp == null)
            {
                decomp = UTF16.ValueOf(comp);
            }

            // See if it matches the start of segment (at segmentPos)
            bool ok = false;
            int cp;
            int decompPos = 0;
            int decompCp = UTF16.CharAt(decomp, 0);
            decompPos += UTF16.GetCharCount(decompCp); // adjust position to skip first char
                                                       //int decompClass = getClass(decompCp);
            buf.Length = 0; // initialize working buffer, shared among callees

            for (int i = segmentPos; i < segment.Length; i += UTF16.GetCharCount(cp))
            {
                cp = UTF16.CharAt(segment, i);
                if (cp == decompCp)
                { // if equal, eat another cp from decomp
                    if (PROGRESS) Console.Out.WriteLine("  matches: " + Utility.Hex(UTF16.ValueOf(cp)));
                    if (decompPos == decomp.Length)
                    { // done, have all decomp characters!
                        buf.Append(segment.Substring(i + UTF16.GetCharCount(cp))); // add remaining segment chars
                        ok = true;
                        break;
                    }
                    decompCp = UTF16.CharAt(decomp, decompPos);
                    decompPos += UTF16.GetCharCount(decompCp);
                    //decompClass = getClass(decompCp);
                }
                else
                {
                    if (PROGRESS) Console.Out.WriteLine("  buffer: " + Utility.Hex(UTF16.ValueOf(cp)));
                    // brute force approach
                    UTF16.Append(buf, cp);
                    /* TODO: optimize
                    // since we know that the classes are monotonically increasing, after zero
                    // e.g. 0 5 7 9 0 3
                    // we can do an optimization
                    // there are only a few cases that work: zero, less, same, greater
                    // if both classes are the same, we fail
                    // if the decomp class < the segment class, we fail

                    segClass = getClass(cp);
                    if (decompClass <= segClass) return null;
                    */
                }
            }
            if (!ok) return null; // we failed, characters left over
            if (PROGRESS) Console.Out.WriteLine("Matches");
            if (buf.Length == 0) return SET_WITH_NULL_STRING; // succeed, but no remainder
            string remainder = buf.ToString();

            // brute force approach
            // to check to make sure result is canonically equivalent
            /*
            String trial = Normalizer.normalize(UTF16.valueOf(comp) + remainder, Normalizer.DECOMP, 0);
            if (!segment.regionMatches(segmentPos, trial, 0, segment.length() - segmentPos)) return null;
            */

            if (0 != Normalizer.Compare(UTF16.ValueOf(comp) + remainder, segment.Substring(segmentPos), 0)) return null;

            // get the remaining combinations
            return GetEquivalents2(remainder);
        }

        /*
        // TODO: fix once we have a codepoint interface to get the canonical combining class
        // TODO: Need public access to canonical combining class in UCharacter!
        private static int getClass(int cp) {
            return Normalizer.getClass((char)cp);
        }
        */

        // ================= BUILDER =========================
        // TODO: Flatten this data so it doesn't have to be reconstructed each time!

        //private static final UnicodeSet EMPTY = new UnicodeSet(); // constant, don't change
        private static readonly ISet<string> SET_WITH_NULL_STRING = new HashSet<string>(); // constant, don't change
        static CanonicalIterator()
        {
            SET_WITH_NULL_STRING.Add("");
        }

        //  private static UnicodeSet SAFE_START = new UnicodeSet();
        //  private static CharMap AT_START = new CharMap();

        // TODO: WARNING, NORMALIZER doesn't have supplementaries yet !!;
        // Change FFFF to 10FFFF in C, and in Java when normalizer is upgraded.
        //  private static int LAST_UNICODE = 0x10FFFF;
        /*
        static {
            buildData();
        }
        */
        /*
        private static void buildData() {

            if (PROGRESS) System.out.println("Getting Safe Start");
            for (int cp = 0; cp <= LAST_UNICODE; ++cp) {
                if (PROGRESS & (cp & 0x7FF) == 0) System.out.print('.');
                int cc = UCharacter.getCombiningClass(cp);
                if (cc == 0) SAFE_START.add(cp);
                // will fix to be really safe below
            }
            if (PROGRESS) System.out.println();

            if (PROGRESS) System.out.println("Getting Containment");
            for (int cp = 0; cp <= LAST_UNICODE; ++cp) {
                if (PROGRESS & (cp & 0x7FF) == 0) System.out.print('.');

                if (Normalizer.isNormalized(cp, Normalizer.NFD)) continue;

                //String istr = UTF16.valueOf(cp);
                String decomp = Normalizer.normalize(cp, Normalizer.NFD);
                //if (decomp.equals(istr)) continue;

                // add each character in the decomposition to canBeIn

                int component;
                for (int i = 0; i < decomp.length(); i += UTF16.getCharCount(component)) {
                    component = UTF16.charAt(decomp, i);
                    if (i == 0) {
                        AT_START.add(component, cp);
                    } else if (UCharacter.getCombiningClass(component) == 0) {
                        SAFE_START.remove(component);
                    }
                }
            }
            if (PROGRESS) System.out.println();
        }
            // the following is just for a map from characters to a set of characters

        private static class CharMap {
            Map storage = new HashMap();
            MutableInt probe = new MutableInt();
            boolean converted = false;

            public void add(int cp, int whatItIsIn) {
                UnicodeSet result = (UnicodeSet) storage.get(probe.set(cp));
                if (result == null) {
                    result = new UnicodeSet();
                    storage.put(probe, result);
                }
                result.add(whatItIsIn);
            }

            public UnicodeSet get(int cp) {
                return (UnicodeSet) storage.get(probe.set(cp));
            }
        }

        private static class MutableInt {
            public int contents;
            public int hashCode() { return contents; }
            public boolean equals(Object other) {
                return ((MutableInt)other).contents == contents;
            }
            // allows chaining
            public MutableInt set(int contents) {
                this.contents = contents;
                return this;
            }
        }
        */

    }
}
