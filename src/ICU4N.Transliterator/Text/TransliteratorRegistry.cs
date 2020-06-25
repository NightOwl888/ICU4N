using ICU4N.Impl;
using ICU4N.Globalization;
using ICU4N.Support.Collections;
using ICU4N.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;
using Data = ICU4N.Text.RuleBasedTransliterator.Data;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Text
{
    internal class TransliteratorRegistry
    {
        // char constants
        private const char LOCALE_SEP = '_';

        // String constants
        private const string NO_VARIANT = ""; // empty string
        private const string ANY = "Any";

        /// <summary>
        /// Dynamic registry mapping full IDs to Entry objects.  This
        /// contains both public and internal entities.  The visibility is
        /// controlled by whether an entry is listed in <see cref="availableIDs"/> and
        /// specDAG or not.
        /// <para/>
        /// Keys are <see cref="CaseInsensitiveString"/> objects.
        /// Values are objects of class Class (subclass of <see cref="Transliterator"/>),
        /// <see cref="RuleBasedTransliterator.Data"/>, <see cref="Transliterator.Factory"/>, or one
        /// of the entry classes defined here (<see cref="AliasEntry"/> or <see cref="ResourceEntry"/>).
        /// </summary>
        private IDictionary<CaseInsensitiveString, object[]> registry;

        /// <summary>
        /// DAG of visible IDs by spec.  IDictionary: source => (IDictionary:
        /// target => (Vector: variant)) The Vector of variants is never
        /// empty.  For a source-target with no variant, the special
        /// variant <see cref="NO_VARIANT"/> (the empty string) is stored in slot zero of
        /// the UVector.
        /// <para/>
        /// Keys are <see cref="CaseInsensitiveString"/> objects.
        /// Values are IDictionary of (<see cref="CaseInsensitiveString"/> -> Vector of
        /// <see cref="CaseInsensitiveString"/>)
        /// </summary>
        private IDictionary<CaseInsensitiveString, IDictionary<CaseInsensitiveString, IList<CaseInsensitiveString>>> specDAG;

        /// <summary>
        /// Vector of public full IDs (<see cref="CaseInsensitiveString"/> objects).
        /// </summary>
        private IList<CaseInsensitiveString> availableIDs;

        //----------------------------------------------------------------------
        // class Spec
        //----------------------------------------------------------------------

        /// <summary>
        /// A <see cref="Spec"/> is a string specifying either a source or a target.  In more
        /// general terms, it may also specify a variant, but we only use the
        /// <see cref="Spec"/> class for sources and targets.
        /// <para/>
        /// A <see cref="Spec"/> may be a locale or a script.  If it is a locale, it has a
        /// fallback chain that goes xx_YY_ZZZ -> xx_YY -> xx -> ssss, where
        /// ssss is the script mapping of xx_YY_ZZZ.  The Spec API methods
        /// <see cref="HasFallback"/>, <see cref="Next()"/>, <see cref="Reset()"/>
        /// iterate over this fallback sequence.
        /// <para/>
        /// The <see cref="Spec"/> class canonicalizes itself, so the locale is put into
        /// canonical form, or the script is transformed from an abbreviation
        /// to a full name.
        /// </summary>
        internal class Spec
        {
            private string top;        // top spec
            private string spec;       // current spec
            private string nextSpec;   // next spec
            private string scriptName; // script name equivalent of top, if != top
            private bool isSpecLocale; // TRUE if spec is a locale
            private bool isNextLocale; // TRUE if nextSpec is a locale
            private ICUResourceBundle res;

            public Spec(string theSpec)
            {
                top = theSpec;
                spec = null;
                scriptName = null;
                try
                {
                    // Canonicalize script name.  If top is a script name then
                    // script != UScript.INVALID_CODE.
                    int script = UScript.GetCodeFromName(top);

                    // Canonicalize script name -or- do locale->script mapping
                    int[] s = UScript.GetCode(top);
                    if (s != null)
                    {
                        scriptName = UScript.GetName(s[0]);
                        // If the script name is the same as top then it's redundant
                        if (scriptName.Equals(top, StringComparison.OrdinalIgnoreCase))
                        {
                            scriptName = null;
                        }
                    }

                    isSpecLocale = false;
                    res = null;
                    // If 'top' is not a script name, try a locale lookup
                    if (script == UScript.InvalidCode)
                    {
                        // ICU4N specific - CultureInfo doesn't support IANA culture names, so we use ULocale instead.
                        UCultureInfo toploc = new UCultureInfo(top);

                        //CultureInfo toploc = LocaleUtility.GetLocaleFromName(top);
                        res = (ICUResourceBundle)UResourceBundle.GetBundleInstance(ICUData.IcuTransliteratorBaseName, toploc, Transliterator.IcuDataAssembly);
                        // Make sure we got the bundle we wanted; otherwise, don't use it
                        if (res != null && LocaleUtility.IsFallbackOf(res.UCulture.ToString(), top))
                        {
                            isSpecLocale = true;
                        }
                    }
                }
                catch (MissingManifestResourceException)
                {
                    ////CLOVER:OFF
                    // The constructor is called from multiple private methods
                    //  that protects an invalid scriptName
                    scriptName = null;
                    ////CLOVER:ON
                }
                // assert(spec != top);
                Reset();
            }

            public virtual bool HasFallback
            {
                get { return nextSpec != null; }
            }

            public virtual void Reset()
            {
                if (!Utility.SameObjects(spec, top))
                {
                    spec = top;
                    isSpecLocale = (res != null);
                    SetupNext();
                }
            }

            private void SetupNext()
            {
                isNextLocale = false;
                if (isSpecLocale)
                {
                    nextSpec = spec;
                    int i = nextSpec.LastIndexOf(LOCALE_SEP);
                    // If i == 0 then we have _FOO, so we fall through
                    // to the scriptName.
                    if (i > 0)
                    {
                        nextSpec = spec.Substring(0, i); // ICU4N: Checked 2nd parameter
                        isNextLocale = true;
                    }
                    else
                    {
                        nextSpec = scriptName; // scriptName may be null
                    }
                }
                else
                {
                    // Fallback to the script, which may be null
                    if (!Utility.SameObjects(nextSpec, scriptName))
                    {
                        nextSpec = scriptName;
                    }
                    else
                    {
                        nextSpec = null;
                    }
                }
            }

            // Protocol:
            // for(String& s(spec.get());
            //     spec.hasFallback(); s(spec.next())) { ...

            public virtual string Next()
            {
                spec = nextSpec;
                isSpecLocale = isNextLocale;
                SetupNext();
                return spec;
            }

            public virtual string Get()
            {
                return spec;
            }

            public virtual bool IsLocale
            {
                get { return isSpecLocale; }
            }

            /// <summary>
            /// Return the ResourceBundle for this spec, at the current
            /// level of iteration.  The level of iteration goes from
            /// aa_BB_CCC to aa_BB to aa.  If the bundle does not
            /// correspond to the current level of iteration, return null.
            /// If <see cref="IsLocale"/> is false, always return null.
            /// </summary>
            public virtual ResourceBundle GetBundle()
            {
                if (res != null &&
                    res.UCulture.ToString().Equals(spec))
                {
                    return res;
                }
                return null;
            }

            public virtual string Top
            {
                get { return top; }
            }
        }

        //----------------------------------------------------------------------
        // Entry classes
        //----------------------------------------------------------------------

        internal class ResourceEntry
        {
            public string Resource { get; set; }
            public TransliterationDirection Direction { get; set; }
            public ResourceEntry(string n, TransliterationDirection d)
            {
                Resource = n;
                Direction = d;
            }
        }

        // An entry representing a rule in a locale resource bundle
        internal class LocaleEntry
        {
            public string Rule { get; set; }
            public TransliterationDirection Direction { get; set; }
            public LocaleEntry(string r, TransliterationDirection d)
            {
                Rule = r;
                Direction = d;
            }
        }

        internal class AliasEntry
        {
            public string Alias { get; set; }
            public AliasEntry(string a)
            {
                Alias = a;
            }
        }

        internal class CompoundRBTEntry
        {
            private string id;
            private IList<string> idBlockVector;
#pragma warning disable 612, 618
            private IList<Data> dataVector;
#pragma warning restore 612, 618
            private UnicodeSet compoundFilter;

            public CompoundRBTEntry(string theID, IList<string> theIDBlockVector,
#pragma warning disable 612, 618
                                    IList<Data> theDataVector,
#pragma warning restore 612, 618
                                    UnicodeSet theCompoundFilter)
            {
                id = theID;
                idBlockVector = theIDBlockVector;
                dataVector = theDataVector;
                compoundFilter = theCompoundFilter;
            }

            public virtual Transliterator GetInstance()
            {
                List<Transliterator> transliterators = new List<Transliterator>();
                int passNumber = 1;

                int limit = Math.Max(idBlockVector.Count, dataVector.Count);
                for (int i = 0; i < limit; i++)
                {
                    if (i < idBlockVector.Count)
                    {
                        string idBlock = idBlockVector[i];
                        if (idBlock.Length > 0)
                            transliterators.Add(Transliterator.GetInstance(idBlock));
                    }
                    if (i < dataVector.Count)
                    {
#pragma warning disable 612, 618
                        Data data = dataVector[i];
                        transliterators.Add(new RuleBasedTransliterator("%Pass" + passNumber++, data, null));
#pragma warning restore 612, 618
                    }
                }

                Transliterator t = new CompoundTransliterator(transliterators, passNumber - 1);
                t.ID = id;
                if (compoundFilter != null)
                {
                    t.Filter = compoundFilter;
                }
                return t;
            }
        }

        //----------------------------------------------------------------------
        // class TransliteratorRegistry: Basic public API
        //----------------------------------------------------------------------

        public TransliteratorRegistry()
        {
            registry = new ConcurrentDictionary<CaseInsensitiveString, object[]>();
            specDAG = new ConcurrentDictionary<CaseInsensitiveString, IDictionary<CaseInsensitiveString, IList<CaseInsensitiveString>>>();
            availableIDs = new List<CaseInsensitiveString>();
        }

        /// <summary>
        /// Given a simple <paramref name="id"/> (forward direction, no inline filter, not
        /// compound) attempt to instantiate it from the registry.  Return
        /// 0 on failure.
        /// <para/>
        /// Return a non-empty <paramref name="aliasReturn"/> value if the <paramref name="id"/> points to an alias.
        /// We cannot instantiate it ourselves because the alias may contain
        /// filters or compounds, which we do not understand.  Caller should
        /// make <paramref name="aliasReturn"/> empty before calling.
        /// </summary>
        public virtual Transliterator Get(string id,
                                  StringBuffer aliasReturn)
        {
            object[] entry = Find(id);
            return (entry == null) ? null
                : InstantiateEntry(id, entry, aliasReturn);
        }

        /// <summary>
        /// Register a <see cref="Type"/>.  This adds an entry to the
        /// dynamic store, or replaces an existing entry.  Any entry in the
        /// underlying static locale resource store is masked.
        /// </summary>
        public virtual void Put(string id,
                        Type transliteratorSubclass,
                        bool visible)
        {
            RegisterEntry(id, transliteratorSubclass, visible);
        }

        /// <summary>
        /// Register an <paramref name="id"/> and a <paramref name="factory"/> function pointer.  This adds an
        /// entry to the dynamic store, or replaces an existing entry.  Any
        /// entry in the underlying static locale resource store is masked.
        /// </summary>
        public virtual void Put(string id,
                        ITransliteratorFactory factory,
                        bool visible)
        {
            RegisterEntry(id, factory, visible);
        }

        /// <summary>
        /// Register an <paramref name="id"/> and a <paramref name="resourceName"/>. This adds an entry to the
        /// dynamic store, or replaces an existing entry.  Any entry in the
        /// underlying static locale resource store is masked.
        /// </summary>
        public virtual void Put(string id,
                        string resourceName,
                        TransliterationDirection dir,
                        bool visible)
        {
            RegisterEntry(id, new ResourceEntry(resourceName, dir), visible);
        }

        /// <summary>
        /// Register an <paramref name="id"/> and an <paramref name="alias"/> ID.  This adds an entry to the
        /// dynamic store, or replaces an existing entry.  Any entry in the
        /// underlying static locale resource store is masked.
        /// </summary>
        public virtual void Put(string id,
                        string alias,
                        bool visible)
        {
            RegisterEntry(id, new AliasEntry(alias), visible);
        }

        /// <summary>
        /// Register an <paramref name="id"/> and a <see cref="Transliterator"/> object.  This adds an entry
        /// to the dynamic store, or replaces an existing entry.  Any entry
        /// in the underlying static locale resource store is masked.
        /// </summary>
        public virtual void Put(string id,
                        Transliterator trans,
                        bool visible)
        {
            RegisterEntry(id, trans, visible);
        }

        /// <summary>
        /// Unregister an <paramref name="id"/>.  This removes an entry from the dynamic store
        /// if there is one.  The static locale resource store is
        /// unaffected.
        /// </summary>
        public virtual void Remove(string id)
        {
            string[] stv = TransliteratorIDParser.IDtoSTV(id);
            // Only need to do this if ID.indexOf('-') < 0
            string id2 = TransliteratorIDParser.STVtoID(stv[0], stv[1], stv[2]);
            registry.Remove(new CaseInsensitiveString(id2));
            RemoveSTV(stv[0], stv[1], stv[2]);
            availableIDs.Remove(new CaseInsensitiveString(id2));
        }

        //----------------------------------------------------------------------
        // class TransliteratorRegistry: Public ID and spec management
        //----------------------------------------------------------------------

        /////**
        //// * An internal class that adapts an enumeration over
        //// * CaseInsensitiveStrings to an enumeration over Strings.
        //// */
        ////private class IDEnumerable : IEnumerable<string> {
        ////    IEnumerable<CaseInsensitiveString> en;

        ////public IDEnumerable(IEnumerable<CaseInsensitiveString> e)
        ////{
        ////    en = e;
        ////}

        ////    public IEnumerator<string> GetEnumerator()
        ////    {
        ////        throw new NotImplementedException();
        ////    }

        ////    IEnumerator IEnumerable.GetEnumerator()
        ////    {
        ////        throw new NotImplementedException();
        ////    }

        ////    //@Override
        ////    //public boolean hasMoreElements()
        ////    //{
        ////    //    return en != null && en.hasMoreElements();
        ////    //}

        ////    //@Override
        ////    //public String nextElement()
        ////    //{
        ////    //    return (en.nextElement()).getString();
        ////    //}
        ////}

        /// <summary>
        /// Returns an enumerable over the programmatic names of visible
        /// registered transliterators.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{String}"/>.</returns>
        public virtual IEnumerable<string> GetAvailableIDs()
        {
            // Since the cache contains CaseInsensitiveString objects, but
            // the caller expects Strings, we have to use an intermediary.
            foreach (var id in availableIDs)
                yield return id.String;
        }

        /// <summary>
        /// Returns an enumerable over all visible source names.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{String}"/>.</returns>
        public virtual IEnumerable<string> GetAvailableSources()
        {
            //return new IDEnumeration(Collections.enumeration(specDAG.keySet()));
            foreach (var id in specDAG.Keys)
                yield return id.String;
        }

        /// <summary>
        /// Returns an enumerable over visible target names for the given
        /// <paramref name="source"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{String}"/>.</returns>
        public virtual IEnumerable<string> GetAvailableTargets(string source)
        {
            CaseInsensitiveString cisrc = new CaseInsensitiveString(source);
            var targets = specDAG.Get(cisrc);
            if (targets != null)
            {
                foreach (var id in targets.Keys)
                    yield return id.String;
            }
        }

        /// <summary>
        /// Returns an enumerable over visible variant names for the given
        /// <paramref name="source"/> and <paramref name="target"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{String}"/>.</returns>
        public virtual IEnumerable<string> GetAvailableVariants(string source, string target)
        {
            CaseInsensitiveString cisrc = new CaseInsensitiveString(source);
            CaseInsensitiveString citrg = new CaseInsensitiveString(target);
            var targets = specDAG.Get(cisrc);
            if (targets != null)
            {
                var variants = targets.Get(citrg);
                if (variants != null)
                {
                    foreach (var id in variants)
                        yield return id.String;
                }
            }
        }

        //----------------------------------------------------------------------
        // class TransliteratorRegistry: internal
        //----------------------------------------------------------------------

        /// <summary>
        /// Convenience method. Calls <see cref="RegisterEntry(string, string, string, string, object, bool)"/>.
        /// </summary>
        private void RegisterEntry(string source,
                                   string target,
                                   string variant,
                                   object entry,
                                   bool visible)
        {
            string s = source;
            if (s.Length == 0)
            {
                s = ANY;
            }
            string ID = TransliteratorIDParser.STVtoID(source, target, variant);
            RegisterEntry(ID, s, target, variant, entry, visible);
        }

        /// <summary>
        /// Convenience method. Calls <see cref="RegisterEntry(string, string, string, string, object, bool)"/>.
        /// </summary>
        private void RegisterEntry(string ID,
                                   object entry,
                                   bool visible)
        {
            string[] stv = TransliteratorIDParser.IDtoSTV(ID);
            // Only need to do this if ID.indexOf('-') < 0
            string id = TransliteratorIDParser.STVtoID(stv[0], stv[1], stv[2]);
            RegisterEntry(id, stv[0], stv[1], stv[2], entry, visible);
        }

        /// <summary>
        /// Register an <paramref name="entry"/> object (adopted) with the given <paramref name="ID"/>, <paramref name="source"/>,
        /// <paramref name="target"/>, and <paramref name="variant"/> strings.
        /// </summary>
        private void RegisterEntry(string ID,
                                   string source,
                                   string target,
                                   string variant,
                                   object entry,
                                   bool visible)
        {
            CaseInsensitiveString ciID = new CaseInsensitiveString(ID);
            object[] arrayOfObj;

            // Store the entry within an array so it can be modified later
            if (entry is object[])
            {
                arrayOfObj = (object[])entry;
            }
            else
            {
                arrayOfObj = new object[] { entry };
            }

            registry[ciID] = arrayOfObj;
            if (visible)
            {
                RegisterSTV(source, target, variant);
                if (!availableIDs.Contains(ciID))
                {
                    availableIDs.Add(ciID);
                }
            }
            else
            {
                RemoveSTV(source, target, variant);
                availableIDs.Remove(ciID);
            }
        }

        /// <summary>
        /// Register a source-target/variant in the specDAG.  Variant may be
        /// empty, but <paramref name="source"/> and <paramref name="target"/> must not be.  If variant is empty then
        /// the special variant <see cref="NO_VARIANT"/> is stored in slot zero of the
        /// UVector of variants.
        /// </summary>
        private void RegisterSTV(string source,
                                 string target,
                                 string variant)
        {
            // assert(source.length() > 0);
            // assert(target.length() > 0);
            CaseInsensitiveString cisrc = new CaseInsensitiveString(source);
            CaseInsensitiveString citrg = new CaseInsensitiveString(target);
            CaseInsensitiveString civar = new CaseInsensitiveString(variant);
            var targets = specDAG.Get(cisrc);
            if (targets == null)
            {
                targets = new ConcurrentDictionary<CaseInsensitiveString, IList<CaseInsensitiveString>>();
                specDAG[cisrc] = targets;
            }
            var variants = targets.Get(citrg);
            if (variants == null)
            {
                variants = new List<CaseInsensitiveString>();
                targets[citrg] = variants;
            }
            // assert(NO_VARIANT == "");
            // We add the variant string.  If it is the special "no variant"
            // string, that is, the empty string, we add it at position zero.
            if (!variants.Contains(civar))
            {
                if (variant.Length > 0)
                {
                    variants.Add(civar);
                }
                else
                {
                    variants.Insert(0, civar);
                }
            }
        }

        /// <summary>
        /// Remove a source-target/variant from the specDAG.
        /// </summary>
        private void RemoveSTV(string source,
                               string target,
                               string variant)
        {
            // assert(source.length() > 0);
            // assert(target.length() > 0);
            CaseInsensitiveString cisrc = new CaseInsensitiveString(source);
            CaseInsensitiveString citrg = new CaseInsensitiveString(target);
            CaseInsensitiveString civar = new CaseInsensitiveString(variant);
            var targets = specDAG.Get(cisrc);
            if (targets == null)
            {
                return; // should never happen for valid s-t/v
            }
            var variants = targets.Get(citrg);
            if (variants == null)
            {
                return; // should never happen for valid s-t/v
            }
            variants.Remove(civar);
            if (variants.Count == 0)
            {
                targets.Remove(citrg); // should delete variants
                if (targets.Count == 0)
                {
                    specDAG.Remove(cisrc); // should delete targets
                }
            }
        }

        private static readonly bool DEBUG = false;

        /// <summary>
        /// Attempt to find a source-target/variant in the dynamic registry
        /// store.  Return 0 on failure.
        /// </summary>
        private object[] FindInDynamicStore(Spec src,
                                          Spec trg,
                                          string variant)
        {
            string ID = TransliteratorIDParser.STVtoID(src.Get(), trg.Get(), variant);
            ////CLOVER:OFF
            if (DEBUG)
            {
                Console.Out.WriteLine("TransliteratorRegistry.findInDynamicStore:" +
                                   ID);
            }
            ////CLOVER:ON
            return registry.Get(new CaseInsensitiveString(ID));
        }

        /// <summary>
        /// Attempt to find a source-target/variant in the static locale
        /// resource store.  Do not perform fallback.  Return 0 on failure.
        /// <para/>
        /// On success, create a new entry object, register it in the dynamic
        /// store, and return a pointer to it, but do not make it public --
        /// just because someone requested something, we do not expand the
        /// available ID list (or spec DAG).
        /// </summary>
        private object[] FindInStaticStore(Spec src,
                                         Spec trg,
                                         string variant)
        {
            ////CLOVER:OFF
            if (DEBUG)
            {
                string ID = TransliteratorIDParser.STVtoID(src.Get(), trg.Get(), variant);
                Console.Out.WriteLine("TransliteratorRegistry.findInStaticStore:" +
                                   ID);
            }
            ////CLOVER:ON
            object[] entry = null;
            if (src.IsLocale)
            {
                entry = FindInBundle(src, trg, variant, Transliterator.Forward);
            }
            else if (trg.IsLocale)
            {
                entry = FindInBundle(trg, src, variant, Transliterator.Reverse);
            }

            // If we found an entry, store it in the Hashtable for next
            // time.
            if (entry != null)
            {
                RegisterEntry(src.Top, trg.Top, variant, entry, false);
            }

            return entry;
        }

        /// <summary>
        /// Attempt to find an entry in a single resource bundle.  This is
        /// a one-sided lookup. <see cref="FindInStaticStore(Spec, Spec, string)"/> performs up to two such
        /// lookups, one for the source, and one for the target.
        /// <para/>
        /// Do not perform fallback.  Return 0 on failure.
        /// <para/>
        /// On success, create a new Entry object, populate it, and return it.
        /// The caller owns the returned object.
        /// </summary>
        private object[] FindInBundle(Spec specToOpen,
                                      Spec specToFind,
                                      string variant,
                                      TransliterationDirection direction)
        {
            // assert(specToOpen.isLocale());
            ResourceBundle res = specToOpen.GetBundle();

            if (res == null)
            {
                // This means that the bundle's locale does not match
                // the current level of iteration for the spec.
                return null;
            }

            for (int pass = 0; pass < 2; ++pass)
            {
                StringBuilder tag = new StringBuilder();
                // First try either TransliteratorTo_xxx or
                // TransliterateFrom_xxx, then try the bidirectional
                // Transliterate_xxx.  This precedence order is arbitrary
                // but must be consistent and documented.
                if (pass == 0)
                {
                    tag.Append(direction == Transliterator.Forward ?
                               "TransliterateTo" : "TransliterateFrom");
                }
                else
                {
                    tag.Append("Transliterate");
                }
                tag.Append(specToFind.Get().ToUpperInvariant());

                try
                {
                    // The Transliterate*_xxx resource is an array of
                    // strings of the format { <v0>, <r0>, ... }.  Each
                    // <vi> is a variant name, and each <ri> is a rule.
                    string[] subres = res.GetStringArray(tag.ToString());

                    // assert(subres != null);
                    // assert(subres.length % 2 == 0);
                    int i = 0;
                    if (variant.Length != 0)
                    {
                        for (i = 0; i < subres.Length; i += 2)
                        {
                            if (subres[i].Equals(variant, StringComparison.OrdinalIgnoreCase))
                            {
                                break;
                            }
                        }
                    }

                    if (i < subres.Length)
                    {
                        // We have a match, or there is no variant and i == 0.
                        // We have succeeded in loading a string from the
                        // locale resources.  Return the rule string which
                        // will itself become the registry entry.

                        // The direction is always forward for the
                        // TransliterateTo_xxx and TransliterateFrom_xxx
                        // items; those are unidirectional forward rules.
                        // For the bidirectional Transliterate_xxx items,
                        // the direction is the value passed in to this
                        // function.
                        TransliterationDirection dir = (pass == 0) ? Transliterator.Forward : direction;
                        return new Object[] { new LocaleEntry(subres[i + 1], dir) };
                    }

                }
                catch (MissingManifestResourceException e)
                {
                    ////CLOVER:OFF
                    if (DEBUG) Console.Out.WriteLine("missing resource: " + e);
                    ////CLOVER:ON
                }
            }

            // If we get here we had a missing resource exception or we
            // failed to find a desired variant.
            return null;
        }

        /// <summary>
        /// Convenience method. Calls <see cref="Find(string, string, string)"/>.
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        private object[] Find(string ID)
        {
            string[] stv = TransliteratorIDParser.IDtoSTV(ID);
            return Find(stv[0], stv[1], stv[2]);
        }

        /// <summary>
        /// Top-level find method.  Attempt to find a source-target/variant in
        /// either the dynamic or the static (locale resource) store.  Perform
        /// fallback.
        /// <para/>
        /// Lookup sequence for ss_SS_SSS-tt_TT_TTT/v:
        /// <code>
        ///   ss_SS_SSS-tt_TT_TTT/v -- in hashtable
        ///   ss_SS_SSS-tt_TT_TTT/v -- in ss_SS_SSS (no fallback)
        ///   
        ///     repeat with t = tt_TT_TTT, tt_TT, tt, and tscript
        /// 
        ///     ss_SS_SSS-t/*
        ///     ss_SS-t/*
        ///     ss-t/*
        ///     sscript-t/*
        /// </code>
        /// Here * matches the first variant listed.
        /// <para/>
        /// Caller does NOT own returned object.  Return 0 on failure.
        /// </summary>
        private object[] Find(string source,
                              string target,
                              string variant)
        {

            Spec src = new Spec(source);
            Spec trg = new Spec(target);
            object[] entry = null;

            if (variant.Length != 0)
            {

                // Seek exact match in hashtable
                entry = FindInDynamicStore(src, trg, variant);
                if (entry != null)
                {
                    return entry;
                }

                // Seek exact match in locale resources
                entry = FindInStaticStore(src, trg, variant);
                if (entry != null)
                {
                    return entry;
                }
            }

            for (; ; )
            {
                src.Reset();
                for (; ; )
                {
                    // Seek match in hashtable
                    entry = FindInDynamicStore(src, trg, NO_VARIANT);
                    if (entry != null)
                    {
                        return entry;
                    }

                    // Seek match in locale resources
                    entry = FindInStaticStore(src, trg, NO_VARIANT);
                    if (entry != null)
                    {
                        return entry;
                    }
                    if (!src.HasFallback)
                    {
                        break;
                    }
                    src.Next();
                }
                if (!trg.HasFallback)
                {
                    break;
                }
                trg.Next();
            }

            return null;
        }

        /// <summary>
        /// Given an Entry object, instantiate it.  Caller owns result.  Return
        /// 0 on failure.
        /// <para/>
        /// Return a non-empty <paramref name="aliasReturn"/> value if the <paramref name="ID"/> points to an alias.
        /// We cannot instantiate it ourselves because the alias may contain
        /// filters or compounds, which we do not understand.  Caller should
        /// make <paramref name="aliasReturn"/> empty before calling.
        /// <para/>
        /// The entry object is assumed to reside in the dynamic store.  It may be
        /// modified.
        /// </summary>
        private Transliterator InstantiateEntry(string ID,
                                                object[] entryWrapper,
                                                StringBuffer aliasReturn)
        {
            // We actually modify the entry object in some cases.  If it
            // is a string, we may partially parse it and turn it into a
            // more processed precursor.  This makes the next
            // instantiation faster and allows sharing of immutable
            // components like the RuleBasedTransliterator.Data objects.
            // For this reason, the entry object is an Object[] of length
            // 1.

            for (; ; )
            {
                object entry = entryWrapper[0];
#pragma warning disable 612, 618
                if (entry is RuleBasedTransliterator.Data)
                {
                    RuleBasedTransliterator.Data data = (RuleBasedTransliterator.Data)entry;
                    return new RuleBasedTransliterator(ID, data, null);
#pragma warning restore 612, 618
                }
                else if (entry is Type)
                {
                    try
                    {
                        //return (Transliterator)((Type)entry).newInstance();
                        return (Transliterator)Activator.CreateInstance((Type)entry);
                    }
                    catch (TargetInvocationException)
                    {
                    }
                    catch (MethodAccessException) { }
                    return null;
                }
                else if (entry is AliasEntry)
                {
                    aliasReturn.Append(((AliasEntry)entry).Alias);
                    return null;
                }
                else if (entry is ITransliteratorFactory)
                {
                    return ((ITransliteratorFactory)entry).GetInstance(ID);
                }
                else if (entry is CompoundRBTEntry)
                {
                    return ((CompoundRBTEntry)entry).GetInstance();
                }
                else if (entry is AnyTransliterator)
                {
                    AnyTransliterator temp = (AnyTransliterator)entry;
                    return temp.SafeClone();
                }
#pragma warning disable 612, 618
                else if (entry is RuleBasedTransliterator)
                {
                    RuleBasedTransliterator temp = (RuleBasedTransliterator)entry;
                    return temp.SafeClone();
                }
#pragma warning restore 612, 618
                else if (entry is CompoundTransliterator)
                {
                    CompoundTransliterator temp = (CompoundTransliterator)entry;
                    return temp.SafeClone();
                }
                else if (entry is Transliterator)
                {
                    return (Transliterator)entry;
                }

                // At this point entry type must be either RULES_FORWARD or
                // RULES_REVERSE.  We process the rule data into a
                // TransliteratorRuleData object, and possibly also into an
                // .id header and/or footer.  Then we modify the registry with
                // the parsed data and retry.

                TransliteratorParser parser = new TransliteratorParser();

                try
                {

                    ResourceEntry re = (ResourceEntry)entry;
                    parser.Parse(re.Resource, re.Direction);

                }
                catch (InvalidCastException)
                {
                    // If we pull a rule from a locale resource bundle it will
                    // be a LocaleEntry.
                    LocaleEntry le = (LocaleEntry)entry;
                    parser.Parse(le.Rule, le.Direction);
                }

                // Reset entry to something that we process at the
                // top of the loop, then loop back to the top.  As long as we
                // do this, we only loop through twice at most.
                // NOTE: The logic here matches that in
                // Transliterator.createFromRules().
                if (parser.IdBlockVector.Count == 0 && parser.DataVector.Count == 0)
                {
                    // No idBlock, no data -- this is just an
                    // alias for Null
                    entryWrapper[0] = new AliasEntry(NullTransliterator._ID);
                }
                else if (parser.IdBlockVector.Count == 0 && parser.DataVector.Count == 1)
                {
                    // No idBlock, data != 0 -- this is an
                    // ordinary RBT_DATA
                    entryWrapper[0] = parser.DataVector[0];
                }
                else if (parser.IdBlockVector.Count == 1 && parser.DataVector.Count == 0)
                {
                    // idBlock, no data -- this is an alias.  The ID has
                    // been munged from reverse into forward mode, if
                    // necessary, so instantiate the ID in the forward
                    // direction.
                    if (parser.CompoundFilter != null)
                    {
                        entryWrapper[0] = new AliasEntry(parser.CompoundFilter.ToPattern(false) + ";"
                                + parser.IdBlockVector[0]);
                    }
                    else
                    {
                        entryWrapper[0] = new AliasEntry(parser.IdBlockVector[0]);
                    }
                }
                else
                {
                    entryWrapper[0] = new CompoundRBTEntry(ID, parser.IdBlockVector, parser.DataVector,
                            parser.CompoundFilter);
                }
            }
        }
    }
}
