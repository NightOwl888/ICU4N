using ICU4N.Globalization;
using ICU4N.Text;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace ICU4N.Impl.Text
{
    /// <summary>
    /// Returns <see cref="IRbnfLenientScanner"/>s that use the old <see cref="RuleBasedNumberFormat"/>
    /// implementation behind LenientParseEnabled, which is based on <see cref="Collator"/>.
    /// </summary>
    /// <internal/>
    [Obsolete("This API is ICU internal only.")]
    internal class RbnfScannerProvider : IRbnfLenientScannerProvider // ICU4N: Made internal instead of public
    {
        private static readonly bool DEBUG = ICUDebug.Enabled("rbnf");
        private readonly ConcurrentDictionary<string, IRbnfLenientScanner> cache;

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public RbnfScannerProvider()
        {
            cache = new ConcurrentDictionary<string, IRbnfLenientScanner>();
        }

        /// <summary>
        /// Returns a collation-based scanner.
        /// <para/>
        /// Only primary differences are treated as significant. This means that case
        /// differences, accent differences, alternate spellings of the same letter
        /// (e.g., ae and a-umlaut in German), ignorable characters, etc. are ignored in
        /// matching the text.  In many cases, numerals will be accepted in place of words
        /// or phrases as well.
        /// <para/>
        /// For example, all of the following will correctly parse as 255 in English in
        /// lenient-parse mode:
        /// <list type="bullet">
        ///     <item><description>"two hundred fifty-five"</description></item>
        ///     <item><description>"two hundred fifty five"</description></item>
        ///     <item><description>"TWO HUNDRED FIFTY-FIVE"</description></item>
        ///     <item><description>"twohundredfiftyfive"</description></item>
        ///     <item><description>"2 hundred fifty-5"</description></item>
        /// </list>
        /// <para/>
        /// The <see cref="Collator"/> used is determined by the locale that was
        /// passed to this object on construction.  The description passed to this object
        /// on construction may supply additional collation rules that are appended to the
        /// end of the default collator for the locale, enabling additional equivalences
        /// (such as adding more ignorable characters or permitting spelled-out version of
        /// symbols; see the demo program for examples).
        /// <para/>
        /// It's important to emphasize that even strict parsing is relatively lenient: it
        /// will accept some text that it won't produce as output.  In English, for example,
        /// it will correctly parse "two hundred zero" and "fifteen hundred".
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public IRbnfLenientScanner Get(UCultureInfo locale, string extras)
        {
            string key = locale.ToString() + "/" + extras;
            return cache.GetOrAdd(key, (key) => CreateScanner(locale, extras));
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        protected IRbnfLenientScanner CreateScanner(UCultureInfo locale, string extras)
        {
            RuleBasedCollator collator = null;
            try
            {
                // create a default collator based on the locale,
                // then pull out that collator's rules, append any additional
                // rules specified in the description, and create a _new_
                // collator based on the combination of those rules
                collator = (RuleBasedCollator)Collator.GetInstance(locale.ToCultureInfo());
                if (extras != null)
                {
                    string rules = collator.GetRules() + extras;
                    collator = new RuleBasedCollator(rules);
                }
                collator.Decomposition = NormalizationMode.CanonicalDecomposition;
            }
            catch (Exception e)
            {
                // If we get here, it means we have a malformed set of
                // collation rules, which hopefully won't happen
                ////CLOVER:OFF
                if (DEBUG)
                { // debug hook
                    Console.Out.WriteLine(e.ToString()); Console.Out.WriteLine("++++");
                }
                collator = null;
                ////CLOVER:ON
            }

            return new RbnfLenientScanner(collator);
        }

        private class RbnfLenientScanner : IRbnfLenientScanner
        {
            private readonly RuleBasedCollator collator;

            internal RbnfLenientScanner(RuleBasedCollator rbc)
            {
                this.collator = rbc;
            }

            public bool AllIgnorable(string s)
            {
                CollationElementIterator iter = collator.GetCollationElementIterator(s);

                int o = iter.Next();
                while (o != CollationElementIterator.NullOrder
                       && CollationElementIterator.PrimaryOrder(o) == 0)
                {
                    o = iter.Next();
                }
                return o == CollationElementIterator.NullOrder;
            }

            public int[] FindText(string str, string key, int startingAt)
            {
                int p = startingAt;
                int keyLen = 0;

                // basically just isolate smaller and smaller substrings of
                // the target string (each running to the end of the string,
                // and with the first one running from startingAt to the end)
                // and then use prefixLength() to see if the search key is at
                // the beginning of each substring.  This is excruciatingly
                // slow, but it will locate the key and tell use how long the
                // matching text was.
                while (p < str.Length && keyLen == 0)
                {
                    keyLen = PrefixLength(str.Substring(p), key);
                    if (keyLen != 0)
                    {
                        return new int[] { p, keyLen };
                    }
                    ++p;
                }
                // if we make it to here, we didn't find it.  Return -1 for the
                // location.  The length should be ignored, but set it to 0,
                // which should be "safe"
                return new int[] { -1, 0 };
            }

            ////CLOVER:OFF
            // The following method contains the same signature as findText
            //  and has never been used by anything once.
            [SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed", Justification = "This is for debugging purposes.")]
            public int[] FindText2(string str, string key, int startingAt)
            {

                CollationElementIterator strIter = collator.GetCollationElementIterator(str);
                CollationElementIterator keyIter = collator.GetCollationElementIterator(key);

                int keyStart = -1;

                strIter.SetOffset(startingAt);

                int oStr = strIter.Next();
                int oKey = keyIter.Next();
                while (oKey != CollationElementIterator.NullOrder)
                {
                    while (oStr != CollationElementIterator.NullOrder &&
                           CollationElementIterator.PrimaryOrder(oStr) == 0)
                    {
                        oStr = strIter.Next();
                    }

                    while (oKey != CollationElementIterator.NullOrder &&
                           CollationElementIterator.PrimaryOrder(oKey) == 0)
                    {
                        oKey = keyIter.Next();
                    }

                    if (oStr == CollationElementIterator.NullOrder)
                    {
                        return new int[] { -1, 0 };
                    }

                    if (oKey == CollationElementIterator.NullOrder)
                    {
                        break;
                    }

                    if (CollationElementIterator.PrimaryOrder(oStr) ==
                        CollationElementIterator.PrimaryOrder(oKey))
                    {
                        keyStart = strIter.GetOffset();
                        oStr = strIter.Next();
                        oKey = keyIter.Next();
                    }
                    else
                    {
                        if (keyStart != -1)
                        {
                            keyStart = -1;
                            keyIter.Reset();
                        }
                        else
                        {
                            oStr = strIter.Next();
                        }
                    }
                }

                return new int[] { keyStart, strIter.GetOffset() - keyStart };
            }
            ////CLOVER:ON

            public int PrefixLength(string str, string prefix)
            {
                // Create two collation element iterators, one over the target string
                // and another over the prefix.
                //
                // Previous code was matching "fifty-" against " fifty" and leaving
                // the number " fifty-7" to parse as 43 (50 - 7).
                // Also it seems that if we consume the entire prefix, that's ok even
                // if we've consumed the entire string, so I switched the logic to
                // reflect this.

                CollationElementIterator strIter = collator.GetCollationElementIterator(str);
                CollationElementIterator prefixIter = collator.GetCollationElementIterator(prefix);

                // match collation elements between the strings
                int oStr = strIter.Next();
                int oPrefix = prefixIter.Next();

                while (oPrefix != CollationElementIterator.NullOrder)
                {
                    // skip over ignorable characters in the target string
                    while (CollationElementIterator.PrimaryOrder(oStr) == 0 && oStr !=
                           CollationElementIterator.NullOrder)
                    {
                        oStr = strIter.Next();
                    }

                    // skip over ignorable characters in the prefix
                    while (CollationElementIterator.PrimaryOrder(oPrefix) == 0 && oPrefix !=
                           CollationElementIterator.NullOrder)
                    {
                        oPrefix = prefixIter.Next();
                    }

                    // if skipping over ignorables brought to the end of
                    // the prefix, we DID match: drop out of the loop
                    if (oPrefix == CollationElementIterator.NullOrder)
                    {
                        break;
                    }

                    // if skipping over ignorables brought us to the end
                    // of the target string, we didn't match and return 0
                    if (oStr == CollationElementIterator.NullOrder)
                    {
                        return 0;
                    }

                    // match collation elements from the two strings
                    // (considering only primary differences).  If we
                    // get a mismatch, dump out and return 0
                    if (CollationElementIterator.PrimaryOrder(oStr) !=
                        CollationElementIterator.PrimaryOrder(oPrefix))
                    {
                        return 0;
                    }

                    // otherwise, advance to the next character in each string
                    // and loop (we drop out of the loop when we exhaust
                    // collation elements in the prefix)

                    oStr = strIter.Next();
                    oPrefix = prefixIter.Next();
                }

                int result = strIter.GetOffset();
                if (oStr != CollationElementIterator.NullOrder)
                {
                    --result;
                }
                return result;
            }
        }
    }
}
