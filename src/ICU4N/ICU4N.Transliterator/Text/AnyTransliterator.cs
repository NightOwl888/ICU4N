using ICU4N.Lang;
using ICU4N.Support.Collections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Resources;

namespace ICU4N.Text
{
    /// <summary>
    /// A transliterator that translates multiple input scripts to a single
    /// output script.  It is named Any-T or Any-T/V, where T is the target
    /// and V is the optional variant.  The target T is a script.
    /// </summary>
    /// <remarks>
    /// An AnyTransliterator partitions text into runs of the same
    /// script, together with adjacent <see cref="UScript.Common"/> 
    /// or <see cref="UScript.Inherited"/> characters.
    /// After determining the script of each run, it transliterates from
    /// that script to the given target/variant.  It does so by
    /// instantiating a transliterator from the source script to the
    /// target/variant.  If a run consists only of the target script,
    /// <see cref="UScript.Common"/>, or <see cref="UScript.Inherited"/> 
    /// characters, then the run is not changed.
    /// <para/>
    /// At startup, all possible AnyTransliterators are registered with
    /// the system, as determined by examining the registered script
    /// transliterators.
    /// </remarks>
    /// <since>ICU 2.2</since>
    /// <author>Alan Liu</author>
    internal class AnyTransliterator : Transliterator
    {
        //------------------------------------------------------------
        // Constants

        internal static readonly char TARGET_SEP = '-';
        new internal static readonly char VARIANT_SEP = '/';
        internal static readonly string ANY = "Any";
        internal static readonly string NULL_ID = "Null";
        internal static readonly string LATIN_PIVOT = "-Latin;Latin-";

        /// <summary>
        /// Cache mapping Script code values to Transliterator*.
        /// </summary>
        private ConcurrentDictionary<int, Transliterator> cache;

        /// <summary>
        /// The target or target/variant string.
        /// </summary>
        private string target;

        /// <summary>
        /// The target script code.  Never <see cref="UScript.InvalidCode"/>.
        /// </summary>
        private int targetScript;

        /// <summary>
        /// Special code for handling width characters
        /// </summary>
        private Transliterator widthFix = Transliterator.GetInstance("[[:dt=Nar:][:dt=Wide:]] nfkd");

        /// <summary>
        /// Implements <see cref="Transliterator.HandleTransliterate(IReplaceable, Position, bool)"/>.
        /// </summary>
        protected override void HandleTransliterate(IReplaceable text,
                                           Position pos, bool isIncremental)
        {
            int allStart = pos.Start;
            int allLimit = pos.Limit;

            ScriptRunIterator it =
                new ScriptRunIterator(text, pos.ContextStart, pos.ContextLimit);

            while (it.Next())
            {
                // Ignore runs in the ante context
                if (it.Limit <= allStart) continue;

                // Try to instantiate transliterator from it.scriptCode to
                // our target or target/variant
                Transliterator t = GetTransliterator(it.ScriptCode);

                if (t == null)
                {
                    // We have no transliterator.  Do nothing, but keep
                    // pos.start up to date.
                    pos.Start = it.Limit;
                    continue;
                }

                // If the run end is before the transliteration limit, do
                // a non-incremental transliteration.  Otherwise do an
                // incremental one.
                bool incremental = isIncremental && (it.Limit >= allLimit);

                pos.Start = Math.Max(allStart, it.Start);
                pos.Limit = Math.Min(allLimit, it.Limit);
                int limit = pos.Limit;
                t.FilteredTransliterate(text, pos, incremental);
                int delta = pos.Limit - limit;
                allLimit += delta;
                it.AdjustLimit(delta);

                // We're done if we enter the post context
                if (it.Limit >= allLimit) break;
            }

            // Restore limit.  pos.start is fine where the last transliterator
            // left it, or at the end of the last run.
            pos.Limit = allLimit;
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="id">The ID of the form S-T or S-T/V, where T is theTarget
        /// and V is theVariant.  Must not be empty.</param>
        /// <param name="theTarget">The target name.  Must not be empty, and must
        ///  name a script corresponding to theTargetScript.</param>
        /// <param name="theVariant">The variant name, or the empty string if
        /// there is no variant.</param>
        /// <param name="theTargetScript">The script code corresponding to
        /// theTarget.</param>
        private AnyTransliterator(string id,
                                  string theTarget,
                                  string theVariant,
                                  int theTargetScript)
            : base(id, null)
        {
            targetScript = theTargetScript;
            cache = new ConcurrentDictionary<int, Transliterator>();

            target = theTarget;
            if (theVariant.Length > 0)
            {
                target = theTarget + VARIANT_SEP + theVariant;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">The ID of the form S-T or S-T/V, where T is theTarget
        /// and V is theVariant.  Must not be empty.</param>
        /// <param name="filter">The Unicode filter.</param>
        /// <param name="target2">The target name.</param>
        /// <param name="targetScript2">The script code corresponding to theTarget.</param>
        /// <param name="widthFix2">The <see cref="Transliterator"/> width fix.</param>
        /// <param name="cache2">The <see cref="ConcurrentDictionary{TKey, TValue}"/> object for cache.</param>
        public AnyTransliterator(string id, UnicodeFilter filter, string target2,
                int targetScript2, Transliterator widthFix2, ConcurrentDictionary<int, Transliterator> cache2)
            : base(id, filter)
        {
            targetScript = targetScript2;
            cache = cache2;
            target = target2;
        }

        /// <summary>
        /// Returns a transliterator from the given source to our target or
        /// target/variant.  Returns NULL if the source is the same as our
        /// target script, or if the source is <see cref="UScript.InvalidCode"/>.
        /// Caches the result and returns the same transliterator the next
        /// time.  The caller does NOT own the result and must not delete
        /// it.
        /// </summary>
        private Transliterator GetTransliterator(int source)
        {
            if (source == targetScript || source == UScript.InvalidCode)
            {
                if (IsWide(targetScript))
                {
                    return null;
                }
                else
                {
                    return widthFix;
                }
            }

            int key = (int)source;
            Transliterator t = cache.Get(key);
            if (!cache.TryGetValue(key, out t) || t == null)
            {
                string sourceName = UScript.GetName(source);
                string id = sourceName + TARGET_SEP + target;

                try
                {
                    t = Transliterator.GetInstance(id, Forward);
                }
                catch (Exception e) { }
                if (t == null)
                {

                    // Try to pivot around Latin, our most common script
                    id = sourceName + LATIN_PIVOT + target;
                    try
                    {
                        t = Transliterator.GetInstance(id, Forward);
                    }
                    catch (Exception e) { }
                }

                if (t != null)
                {
                    if (!IsWide(targetScript))
                    {
                        IList<Transliterator> v = new List<Transliterator>();
                        v.Add(widthFix);
                        v.Add(t);
                        t = new CompoundTransliterator(v);
                    }
                    //Transliterator prevCachedT = cache.putIfAbsent(key, t);
                    Transliterator prevCachedT;
                    // ICU4N: This is to simulate putIfAbsent
                    // ICU4N TODO: If this works, make it into a PutIfAbsent extension method so we can go back to using ConcurrentDictionary elsewhere
                    if (!cache.TryGetValue(key, out prevCachedT))
                    {
                        // If another thread beat us here, set the prevCachedT
                        // value to NullTransliterator to indicate it already exists
                        if (!cache.TryAdd(key, t))
                            prevCachedT = new NullTransliterator();
                    }
                    if (prevCachedT != null)
                    {
                        t = prevCachedT;
                    }
                }
                else if (!IsWide(targetScript))
                {
                    return widthFix;
                }
            }

            return t;
        }

        private bool IsWide(int script)
        {
            return script == UScript.Bopomofo || script == UScript.Han || script == UScript.Hangul || script == UScript.Hiragana || script == UScript.Katakana;
        }

        /// <summary>
        /// Registers standard transliterators with the system.  Called by
        /// <see cref="Transliterator"/> during initialization.  Scan all current targets
        /// and register those that are scripts T as Any-T/V.
        /// </summary>
        internal static void Register()
        {

            IDictionary<string, ISet<string>> seen = new Dictionary<string, ISet<string>>(); // old code used set, but was dependent on order

            foreach (string source in Transliterator.GetAvailableSources())
            {
                // Ignore the "Any" source
                if (source.Equals(ANY, StringComparison.OrdinalIgnoreCase)) continue;

                foreach (string target in Transliterator.GetAvailableTargets(source))
                {
                    // Get the script code for the target.  If not a script, ignore.
                    int targetScript = ScriptNameToCode(target);
                    if (targetScript == UScript.InvalidCode)
                    {
                        continue;
                    }

                    ISet<string> seenVariants = seen.Get(target);
                    if (seenVariants == null)
                    {
                        seen[target] = seenVariants = new HashSet<string>();
                    }

                    foreach (string variant in Transliterator.GetAvailableVariants(source, target))
                    {
                        // Only process each target/variant pair once
                        if (seenVariants.Contains(variant))
                        {
                            continue;
                        }
                        seenVariants.Add(variant);

                        string id;
                        id = TransliteratorIDParser.STVtoID(ANY, target, variant);
                        AnyTransliterator trans = new AnyTransliterator(id, target, variant,
                                                                        targetScript);
                        Transliterator.RegisterInstance(trans);
                        Transliterator.RegisterSpecialInverse(target, NULL_ID, false);
                    }
                }
            }
        }

        /// <summary>
        /// Return the script code for a given name, or
        /// <see cref="UScript.InvalidCode"/> if not found.
        /// </summary>
        private static int ScriptNameToCode(string name)
        {
            try
            {
                int[] codes = UScript.GetCode(name);
                return codes != null ? codes[0] : UScript.InvalidCode;
            }
            catch (MissingManifestResourceException)
            {
                ///CLOVER:OFF
                return UScript.InvalidCode;
                ///CLOVER:ON
            }
        }

        //------------------------------------------------------------
        // ScriptRunIterator

        /// <summary>
        /// Returns a series of ranges corresponding to scripts. They will be
        /// of the form:
        /// <code>
        /// ccccSScSSccccTTcTcccc   - c = common, S = first script, T = second
        /// |            |          - first run (start, limit)
        ///          |           |  - second run (start, limit)
        /// </code>
        /// That is, the runs will overlap. The reason for this is so that a
        /// transliterator can consider common characters both before and after
        /// the scripts.
        /// </summary>
        private class ScriptRunIterator
        {
            private IReplaceable text;
            private int textStart;
            private int textLimit;

            /// <summary>
            /// The code of the current run, valid after <see cref="Next()"/> returns.  May
            /// be <see cref="UScript.InvalidCode"/> if and only if the entire text is
            /// <see cref="UScript.Common"/>/<see cref="UScript.Inherited"/>.
            /// </summary>
            public int ScriptCode { get; set; }

            /// <summary>
            /// The start of the run, inclusive, valid after <see cref="Next()"/> returns.
            /// </summary>
            public int Start { get; set; }

            /// <summary>
            /// The end of the run, exclusive, valid after <see cref="Next()"/> returns.
            /// </summary>
            public int Limit { get; set; }

            /// <summary>
            /// Constructs a run iterator over the given text from start
            /// (inclusive) to limit (exclusive).
            /// </summary>
            /// <param name="text"></param>
            /// <param name="start"></param>
            /// <param name="limit"></param>
            public ScriptRunIterator(IReplaceable text, int start, int limit)
            {
                this.text = text;
                this.textStart = start;
                this.textLimit = limit;
                this.Limit = start;
            }

            /// <summary>
            /// Returns TRUE if there are any more runs.  TRUE is always
            /// returned at least once.  Upon return, the caller should
            /// examine scriptCode, start, and limit.
            /// </summary>
            public virtual bool Next()
            {
                int ch;
                int s;

                ScriptCode = UScript.InvalidCode; // don't know script yet
                Start = Limit;

                // Are we done?
                if (Start == textLimit)
                {
                    return false;
                }

                // Move start back to include adjacent <see cref="UScript.Common"/> / <see cref="UScript.Inherited"/>
                // characters
                while (Start > textStart)
                {
                    ch = text.Char32At(Start - 1); // look back
                    s = UScript.GetScript(ch);
                    if (s == UScript.Common || s == UScript.Inherited)
                    {
                        --Start;
                    }
                    else
                    {
                        break;
                    }
                }

                // Move limit ahead to include COMMON, INHERITED, and characters
                // of the current script.
                while (Limit < textLimit)
                {
                    ch = text.Char32At(Limit); // look ahead
                    s = UScript.GetScript(ch);
                    if (s != UScript.Common && s != UScript.Inherited)
                    {
                        if (ScriptCode == UScript.InvalidCode)
                        {
                            ScriptCode = s;
                        }
                        else if (s != ScriptCode)
                        {
                            break;
                        }
                    }
                    ++Limit;
                }

                // Return TRUE even if the entire text is COMMON / INHERITED, in
                // which case scriptCode will be UScript.InvalidCode.
                return true;
            }

            /// <summary>
            /// Adjusts internal indices for a change in the limit index of the
            /// given delta.  A positive delta means the limit has increased.
            /// </summary>
            /// <param name="delta"></param>
            public virtual void AdjustLimit(int delta)
            {
                Limit += delta;
                textLimit += delta;
            }
        }

        /// <summary>
        /// Temporary hack for registry problem. Needs to be replaced by better architecture.
        /// </summary>
        public virtual Transliterator SafeClone()
        {
            UnicodeFilter filter = Filter;
            if (filter != null && filter is UnicodeSet)
            {
                filter = new UnicodeSet((UnicodeSet)filter);
            }
            return new AnyTransliterator(ID, filter, target, targetScript, widthFix, cache);
        }

        /// <seealso cref="Transliterator.AddSourceTargetSet(UnicodeSet, UnicodeSet, UnicodeSet)"/>
        public override void AddSourceTargetSet(UnicodeSet inputFilter, UnicodeSet sourceSet, UnicodeSet targetSet)
        {
            UnicodeSet myFilter = GetFilterAsUnicodeSet(inputFilter);
            // Assume that it can modify any character to any other character
            sourceSet.AddAll(myFilter);
            if (myFilter.Count != 0)
            {
                targetSet.AddAll(0, 0x10FFFF);
            }
        }
    }
}
