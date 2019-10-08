using ICU4N.Globalization;
using System.Collections.Generic;

namespace ICU4N.Text
{
    /// <summary>
    /// Simple internal utility class for helping with getSource/TargetSet
    /// </summary>
    internal class SourceTargetUtility
    {
        internal readonly ITransform<string, string> transform;
        internal readonly UnicodeSet sourceCache;
        internal readonly ISet<string> sourceStrings;
        internal static readonly UnicodeSet NON_STARTERS = new UnicodeSet("[:^ccc=0:]").Freeze();
        internal static Normalizer2 NFC = Normalizer2.GetNFCInstance();
        //internal static readonly UnicodeSet TRAILING_COMBINING = new UnicodeSet();

        public SourceTargetUtility(ITransform<string, string> transform)
            : this(transform, null)
        {
        }

        public SourceTargetUtility(ITransform<string, string> transform, Normalizer2 normalizer)
        {
            this.transform = transform;
            if (normalizer != null)
            {
                //            synchronized (SourceTargetUtility.class) {
                //                if (NFC == null) {
                //                    NFC = Normalizer2.getInstance(null, "nfc", Mode.COMPOSE);
                //                    for (int i = 0; i <= 0x10FFFF; ++i) {
                //                        String d = NFC.getDecomposition(i);
                //                        if (d == null) {
                //                            continue;
                //                        }
                //                        String s = NFC.normalize(d);
                //                        if (!CharSequences.equals(i, s)) {
                //                            continue;
                //                        }
                //                        // composes
                //                        boolean first = false;
                //                        for (int trailing : CharSequences.codePoints(d)) {
                //                            if (first) {
                //                                first = false;
                //                            } else {
                //                                TRAILING_COMBINING.add(trailing);
                //                            }
                //                        }
                //                    }
                //                }
                //            }
                sourceCache = new UnicodeSet("[:^ccc=0:]");
            }
            else
            {
                sourceCache = new UnicodeSet();
            }
            sourceStrings = new HashSet<string>();
            for (int i = 0; i <= 0x10FFFF; ++i)
            {
                string s = transform.Transform(UTF16.ValueOf(i));
                bool added = false;
#pragma warning disable 612, 618
                if (!CharSequences.Equals(i, s))
#pragma warning restore 612, 618
                {
                    sourceCache.Add(i);
                    added = true;
                }
                if (normalizer == null)
                {
                    continue;
                }
                string d = NFC.GetDecomposition(i);
                if (d == null)
                {
                    continue;
                }
                s = transform.Transform(d);
                if (!d.Equals(s))
                {
                    sourceStrings.Add(d);
                }
                if (added)
                {
                    continue;
                }
                if (!normalizer.IsInert(i))
                {
                    sourceCache.Add(i);
                    continue;
                }
                // see if any of the non-starters change s; if so, add i
                //            for (String ns : TRAILING_COMBINING) {
                //                String s2 = transform.transform(s + ns);
                //                if (!s2.StartsWith(s, StringComparison.Ordinal)) {
                //                    sourceCache.add(i);
                //                    break;
                //                }
                //            }

                // int endOfFirst = CharSequences.onCharacterBoundary(d, 1) ? 1 : 2;
                // if (endOfFirst >= d.length()) {
                // continue;
                // }
                // // now add all initial substrings
                // for (int j = 1; j < d.length(); ++j) {
                // if (!CharSequences.onCharacterBoundary(d, j)) {
                // continue;
                // }
                // String dd = d.substring(0,j);
                // s = transform.transform(dd);
                // if (!dd.equals(s)) {
                // sourceStrings.add(dd);
                // }
                // }
            }
            sourceCache.Freeze();
        }

        public virtual void AddSourceTargetSet(Transliterator transliterator, UnicodeSet inputFilter, UnicodeSet sourceSet,
                UnicodeSet targetSet)
        {
#pragma warning disable 612, 618
            UnicodeSet myFilter = transliterator.GetFilterAsUnicodeSet(inputFilter);
#pragma warning restore 612, 618
            UnicodeSet affectedCharacters = new UnicodeSet(sourceCache).RetainAll(myFilter);
            sourceSet.AddAll(affectedCharacters);
            foreach (string s in affectedCharacters)
            {
                targetSet.AddAll(transform.Transform(s));
            }
            foreach (string s in sourceStrings)
            {
                if (myFilter.ContainsAll(s))
                {
                    string t = transform.Transform(s);
                    if (!s.Equals(t))
                    {
                        targetSet.AddAll(t);
                        sourceSet.AddAll(s);
                    }
                }
            }
        }
    }
}
