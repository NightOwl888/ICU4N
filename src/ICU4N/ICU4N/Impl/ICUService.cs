using ICU4N.Support.Collections;
using ICU4N.Support.Globalization;
using ICU4N.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Category = ICU4N.Util.ULocale.Category;

namespace ICU4N.Impl
{
    public class ICUService : ICUNotifier
    {
        /**
         * Name used for debugging.
         */
        protected readonly string name;

        /**
         * Constructor.
         */
        public ICUService()
        {
            name = "";
        }

        private static readonly bool DEBUG = ICUDebug.Enabled("service");
        /**
         * Construct with a name (useful for debugging).
         */
        public ICUService(string name)
        {
            this.name = name;
        }

        /**
         * Access to factories is protected by a read-write lock.  This is
         * to allow multiple threads to read concurrently, but keep
         * changes to the factory list atomic with respect to all readers.
         */
        private readonly ICURWLock factoryLock = new ICURWLock();

        /**
         * All the factories registered with this service.
         */
        private readonly List<IFactory> factories = new List<IFactory>();

        /**
         * Record the default number of factories for this service.
         * Can be set by markDefault.
         */
        private int defaultSize = 0;

        /**
         * Keys are used to communicate with factories to generate an
         * instance of the service.  Keys define how ids are
         * canonicalized, provide both a current id and a current
         * descriptor to use in querying the cache and factories, and
         * determine the fallback strategy.</p>
         *
         * <p>Keys provide both a currentDescriptor and a currentID.
         * The descriptor contains an optional prefix, followed by '/'
         * and the currentID.  Factories that handle complex keys,
         * for example number format factories that generate multiple
         * kinds of formatters for the same locale, use the descriptor
         * to provide a fully unique identifier for the service object,
         * while using the currentID (in this case, the locale string),
         * as the visible IDs that can be localized.
         *
         * <p> The default implementation of Key has no fallbacks and
         * has no custom descriptors.</p>
         */
        public class Key
        {
            private readonly string id;

            /**
             * Construct a key from an id.
             */
            public Key(string id)
            {
                this.id = id;
            }

            /**
             * Return the original ID used to construct this key.
             */
            public string ID
            {
                get { return id; }
            }

            /**
             * Return the canonical version of the original ID.  This implementation
             * returns the original ID unchanged.
             */
            public virtual string CanonicalID
            {
                get { return id; }
            }

            /**
             * Return the (canonical) current ID.  This implementation
             * returns the canonical ID.
             */
            public virtual string CurrentID
            {
                get { return CanonicalID; }
            }

            /**
             * Return the current descriptor.  This implementation returns
             * the current ID.  The current descriptor is used to fully
             * identify an instance of the service in the cache.  A
             * factory may handle all descriptors for an ID, or just a
             * particular descriptor.  The factory can either parse the
             * descriptor or use custom API on the key in order to
             * instantiate the service.
             */
            public virtual string CurrentDescriptor()
            {
                return "/" + CurrentID;
            }

            /**
             * If the key has a fallback, modify the key and return true,
             * otherwise return false.  The current ID will change if there
             * is a fallback.  No currentIDs should be repeated, and fallback
             * must eventually return false.  This implmentation has no fallbacks
             * and always returns false.
             */
            public virtual bool Fallback()
            {
                return false;
            }

            /**
             * If a key created from id would eventually fallback to match the
             * canonical ID of this key, return true.
             */
            public virtual bool IsFallbackOf(string idToCheck)
            {
                return CanonicalID.Equals(idToCheck);
            }
        }

        /**
         * Factories generate the service objects maintained by the
         * service.  A factory generates a service object from a key,
         * updates id->factory mappings, and returns the display name for
         * a supported id.
         */
        public interface IFactory
        {

            /**
             * Create a service object from the key, if this factory
             * supports the key.  Otherwise, return null.
             *
             * <p>If the factory supports the key, then it can call
             * the service's getKey(Key, String[], Factory) method
             * passing itself as the factory to get the object that
             * the service would have created prior to the factory's
             * registration with the service.  This can change the
             * key, so any information required from the key should
             * be extracted before making such a callback.
             */
            object Create(Key key, ICUService service);

            /**
             * Update the result IDs (not descriptors) to reflect the IDs
             * this factory handles.  This function and getDisplayName are
             * used to support ICUService.getDisplayNames.  Basically, the
             * factory has to determine which IDs it will permit to be
             * available, and of those, which it will provide localized
             * display names for.  In most cases this reflects the IDs that
             * the factory directly supports.
             */
            void UpdateVisibleIDs(IDictionary<string, IFactory> result);

            /**
             * Return the display name for this id in the provided locale.
             * This is an localized id, not a descriptor.  If the id is
             * not visible or not defined by the factory, return null.
             * If locale is null, return id unchanged.
             */
            string GetDisplayName(string id, ULocale locale);
        }

        /**
         * A default implementation of factory.  This provides default
         * implementations for subclasses, and implements a singleton
         * factory that matches a single id  and returns a single
         * (possibly deferred-initialized) instance.  This implements
         * updateVisibleIDs to add a mapping from its ID to itself
         * if visible is true, or to remove any existing mapping
         * for its ID if visible is false.
         */
        public class SimpleFactory : IFactory
        {
            protected object instance;
            protected string id;
            protected bool visible;

            /**
             * Convenience constructor that calls SimpleFactory(Object, String, boolean)
             * with visible true.
             */
            public SimpleFactory(object instance, string id)
                : this(instance, id, true)
            {
            }

            /**
             * Construct a simple factory that maps a single id to a single
             * service instance.  If visible is true, the id will be visible.
             * Neither the instance nor the id can be null.
             */
            public SimpleFactory(object instance, string id, bool visible)
            {
                if (instance == null || id == null)
                {
                    throw new ArgumentException("Instance or id is null");
                }
                this.instance = instance;
                this.id = id;
                this.visible = visible;
            }

            /**
             * Return the service instance if the factory's id is equal to
             * the key's currentID.  Service is ignored.
             */
            public virtual object Create(Key key, ICUService service)
            {
                if (id.Equals(key.CurrentID))
                {
                    return instance;
                }
                return null;
            }

            /**
             * If visible, adds a mapping from id -> this to the result,
             * otherwise removes id from result.
             */
            public virtual void UpdateVisibleIDs(IDictionary<string, IFactory> result)
            {
                if (visible)
                {
                    result[id] = this;
                }
                else
                {
                    result.Remove(id);
                }
            }

            /**
             * If this.id equals id, returns id regardless of locale,
             * otherwise returns null.  (This default implementation has
             * no localized id information.)
             */
            public virtual string GetDisplayName(string identifier, ULocale locale)
            {
                return (visible && id.Equals(identifier)) ? identifier : null;
            }

            /**
             * For debugging.
             */
            public override string ToString()
            {
                StringBuilder buf = new StringBuilder(base.ToString());
                buf.Append(", id: ");
                buf.Append(id);
                buf.Append(", visible: ");
                buf.Append(visible);
                return buf.ToString();
            }
        }

        /**
         * Convenience override for get(String, String[]). This uses
         * createKey to create a key for the provided descriptor.
         */
        public virtual object Get(string descriptor) // ICU4N TODO: API Use indexer?
        {
            return GetKey(CreateKey(descriptor), null);
        }

        /**
         * Convenience override for get(Key, String[]).  This uses
         * createKey to create a key from the provided descriptor.
         */
        public virtual object Get(string descriptor, string[] actualReturn) // ICU4N TODO: API Use indexer?
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor must not be null");
            }
            return GetKey(CreateKey(descriptor), actualReturn);
        }

        /**
         * Convenience override for get(Key, String[]).
         */
        public virtual object GetKey(Key key)
        {
            return GetKey(key, null);
        }

        /**
         * <p>Given a key, return a service object, and, if actualReturn
         * is not null, the descriptor with which it was found in the
         * first element of actualReturn.  If no service object matches
         * this key, return null, and leave actualReturn unchanged.</p>
         *
         * <p>This queries the cache using the key's descriptor, and if no
         * object in the cache matches it, tries the key on each
         * registered factory, in order.  If none generates a service
         * object for the key, repeats the process with each fallback of
         * the key, until either one returns a service object, or the key
         * has no fallback.</p>
         *
         * <p>If key is null, just returns null.</p>
         */
        public virtual object GetKey(Key key, string[] actualReturn)
        {
            return GetKey(key, actualReturn, null);
        }

        // debugging
        // Map hardRef;

        public virtual object GetKey(Key key, string[] actualReturn, IFactory factory)
        {
            if (factories.Count == 0)
            {
                return HandleDefault(key, actualReturn);
            }

            if (DEBUG) Console.Out.WriteLine("Service: " + name + " key: " + key.CanonicalID);

            CacheEntry result = null;
            if (key != null)
            {
                try
                {
                    // The factory list can't be modified until we're done,
                    // otherwise we might update the cache with an invalid result.
                    // The cache has to stay in synch with the factory list.
                    factoryLock.AcquireRead();

                    IDictionary<string, CacheEntry> cache = this.cache; // copy so we don't need to sync on this
                    if (cache == null)
                    {
                        if (DEBUG) Console.Out.WriteLine("Service " + name + " cache was empty");
                        // synchronized since additions and queries on the cache must be atomic
                        // they can be interleaved, though
                        cache = new ConcurrentDictionary<string, CacheEntry>();
                    }

                    string currentDescriptor = null;
                    List<string> cacheDescriptorList = null;
                    bool putInCache = false;

                    int NDebug = 0;

                    int startIndex = 0;
                    int limit = factories.Count;
                    bool cacheResult = true;
                    if (factory != null)
                    {
                        for (int i = 0; i < limit; ++i)
                        {
                            if (factory == factories[i])
                            {
                                startIndex = i + 1;
                                break;
                            }
                        }
                        if (startIndex == 0)
                        {
                            throw new InvalidOperationException("Factory " + factory + "not registered with service: " + this);
                        }
                        cacheResult = false;
                    }

                    //outer:
                    do
                    {
                        currentDescriptor = key.CurrentDescriptor();
                        if (DEBUG) Console.Out.WriteLine(name + "[" + NDebug++ + "] looking for: " + currentDescriptor);
                        result = cache.Get(currentDescriptor);
                        if (result != null)
                        {
                            if (DEBUG) Console.Out.WriteLine(name + " found with descriptor: " + currentDescriptor);
                            goto outer_break;
                        }
                        else
                        {
                            if (DEBUG) Console.Out.WriteLine("did not find: " + currentDescriptor + " in cache");
                        }

                        // first test of cache failed, so we'll have to update
                        // the cache if we eventually succeed-- that is, if we're
                        // going to update the cache at all.
                        putInCache = cacheResult;

                        //  int n = 0;
                        int index = startIndex;
                        while (index < limit)
                        {
                            IFactory f = factories[index++];
                            if (DEBUG) Console.Out.WriteLine("trying factory[" + (index - 1) + "] " + f.ToString());
                            object service = f.Create(key, this);
                            if (service != null)
                            {
                                result = new CacheEntry(currentDescriptor, service);
                                if (DEBUG) Console.Out.WriteLine(name + " factory supported: " + currentDescriptor + ", caching");
                                goto outer_break;
                            }
                            else
                            {
                                if (DEBUG) Console.Out.WriteLine("factory did not support: " + currentDescriptor);
                            }
                        }

                        // prepare to load the cache with all additional ids that
                        // will resolve to result, assuming we'll succeed.  We
                        // don't want to keep querying on an id that's going to
                        // fallback to the one that succeeded, we want to hit the
                        // cache the first time next goaround.
                        if (cacheDescriptorList == null)
                        {
                            cacheDescriptorList = new List<string>(5);
                        }
                        cacheDescriptorList.Add(currentDescriptor);

                    } while (key.Fallback());
                    outer_break: { }

                    if (result != null)
                    {
                        if (putInCache)
                        {
                            if (DEBUG) Console.Out.WriteLine("caching '" + result.actualDescriptor + "'");
                            cache[result.actualDescriptor] = result;
                            if (cacheDescriptorList != null)
                            {
                                foreach (string desc in cacheDescriptorList)
                                {
                                    if (DEBUG) Console.Out.WriteLine(name + " adding descriptor: '" + desc + "' for actual: '" + result.actualDescriptor + "'");

                                    cache[desc] = result;
                                }
                            }
                            // Atomic update.  We held the read lock all this time
                            // so we know our cache is consistent with the factory list.
                            // We might stomp over a cache that some other thread
                            // rebuilt, but that's the breaks.  They're both good.
                            this.cache = cache;
                        }

                        if (actualReturn != null)
                        {
                            // strip null prefix
                            if (result.actualDescriptor.IndexOf("/") == 0)
                            {
                                actualReturn[0] = result.actualDescriptor.Substring(1);
                            }
                            else
                            {
                                actualReturn[0] = result.actualDescriptor;
                            }
                        }

                        if (DEBUG) Console.Out.WriteLine("found in service: " + name);

                        return result.service;
                    }
                }
                finally
                {
                    factoryLock.ReleaseRead();
                }
            }

            if (DEBUG) Console.Out.WriteLine("not found in service: " + name);

            return HandleDefault(key, actualReturn);
        }
        private IDictionary<string, CacheEntry> cache;

        // Record the actual id for this service in the cache, so we can return it
        // even if we succeed later with a different id.
        private sealed class CacheEntry
        {
            internal readonly string actualDescriptor;
            internal readonly object service;
            internal CacheEntry(string actualDescriptor, object service)
            {
                this.actualDescriptor = actualDescriptor;
                this.service = service;
            }
        }


        /**
         * Default handler for this service if no factory in the list
         * handled the key.
         */
        protected virtual object HandleDefault(Key key, string[] actualIDReturn)
        {
            return null;
        }

        /**
         * Convenience override for getVisibleIDs(String) that passes null
         * as the fallback, thus returning all visible IDs.
         */
        public virtual ICollection<string> GetVisibleIDs() // ICU4N specific - changed return type from ISet to ICollection to avoid O(n)
        {
            return GetVisibleIDs(null);
        }

        /**
         * <p>Return a snapshot of the visible IDs for this service.  This
         * set will not change as Factories are added or removed, but the
         * supported ids will, so there is no guarantee that all and only
         * the ids in the returned set are visible and supported by the
         * service in subsequent calls.</p>
         *
         * <p>matchID is passed to createKey to create a key.  If the
         * key is not null, it is used to filter out ids that don't have
         * the key as a fallback.
         */
        public virtual ICollection<string> GetVisibleIDs(string matchID) // ICU4N specific - changed return type from ISet to ICollection to avoid O(n)
        {
            ICollection<string> result = GetVisibleIDMap().Keys;

            Key fallbackKey = CreateKey(matchID);

            if (fallbackKey != null)
            {
                ISet<string> temp = new HashSet<string>(/*result.Count*/);
                foreach (string id in result)
                {
                    if (fallbackKey.IsFallbackOf(id))
                    {
                        temp.Add(id);
                    }
                }
                result = temp;
            }
            return result;
        }

        /**
         * Return a map from visible ids to factories.
         */
        private IDictionary<string, IFactory> GetVisibleIDMap()
        {
            lock (this)
            { // or idcache-only lock?
                if (idcache == null)
                {
                    try
                    {
                        factoryLock.AcquireRead();
                        IDictionary<string, IFactory> mutableMap = new Dictionary<string, IFactory>();
                        //ListIterator<Factory> lIter = factories.listIterator(factories.Count);
                        //while (lIter.hasPrevious())
                        //{
                        //    IFactory f = lIter.previous();
                        //    f.UpdateVisibleIDs(mutableMap);
                        //}
                        for (int i = factories.Count - 1; i >= 0; i--)
                        {
                            IFactory f = factories[i];
                            f.UpdateVisibleIDs(mutableMap);
                        }

                        this.idcache = mutableMap.ToUnmodifiableDictionary();
                    }
                    finally
                    {
                        factoryLock.ReleaseRead();
                    }
                }
            }
            return idcache;
        }
        private IDictionary<string, IFactory> idcache;

        /**
         * Convenience override for getDisplayName(String, ULocale) that
         * uses the current default locale.
         */
        public virtual string GetDisplayName(string id)
        {
            return GetDisplayName(id, ULocale.GetDefault(Category.DISPLAY));
        }

        /**
         * Given a visible id, return the display name in the requested locale.
         * If there is no directly supported id corresponding to this id, return
         * null.
         */
        public virtual string GetDisplayName(string id, ULocale locale)
        {
            IDictionary<string, IFactory> m = GetVisibleIDMap();
            IFactory f = m.Get(id);
            if (f != null)
            {
                return f.GetDisplayName(id, locale);
            }

            Key key = CreateKey(id);
            while (key.Fallback())
            {
                f = m.Get(key.CurrentID);
                if (f != null)
                {
                    return f.GetDisplayName(id, locale);
                }
            }

            return null;
        }

        /// <summary>
        /// Convenience override of <see cref="GetDisplayNames(ULocale, IComparer{string}, string)"/> that
        /// uses the current default Locale as the locale, null as
        /// the comparer, and null for the matchID.
        /// </summary>
        public virtual SortedDictionary<string, string> GetDisplayNames()
        {
            ULocale locale = ULocale.GetDefault(Category.DISPLAY);
            return GetDisplayNames(locale, (IComparer<string>)null, null);
        }

        /// <summary>
        /// Convenience override of <see cref="GetDisplayNames(ULocale, IComparer{string}, string)"/> that
        /// uses null for the comparer, and null for the matchID.
        /// </summary>
        public virtual SortedDictionary<string, string> GetDisplayNames(ULocale locale)
        {
            return GetDisplayNames(locale, (IComparer<string>)null, null);
        }

        /// <summary>
        /// Convenience override of <see cref="GetDisplayNames(ULocale, IComparer{string}, string)"/> that
        /// uses null for the matchID, thus returning all display names.
        /// </summary>
        public virtual SortedDictionary<string, string> GetDisplayNames(ULocale locale, CompareInfo com)
        {
            return GetDisplayNames(locale, com.ToComparer(), null);
        }

        /// <summary>
        /// Convenience override of <see cref="GetDisplayNames(ULocale, IComparer{string}, string)"/> that
        /// uses null for the matchID, thus returning all display names.
        /// </summary>
        public virtual SortedDictionary<string, string> GetDisplayNames(ULocale locale, IComparer<string> com) // ICU4N TODO: API Add overloads for IComparer<StringBuilder> and IComparer<char[]> ?
        {
            return GetDisplayNames(locale, com, null);
        }

        /// <summary>
        /// Convenience override of <see cref="GetDisplayNames(ULocale, IComparer{string}, string)"/> that
        /// uses null for the comparator.
        /// </summary>
        public virtual SortedDictionary<string, string> GetDisplayNames(ULocale locale, string matchID)
        {
            return GetDisplayNames(locale, (IComparer<string>)null, matchID);
        }

        /// <summary>
        /// Return a snapshot of the mapping from display names to visible
        /// IDs for this service.  This set will not change as factories
        /// are added or removed, but the supported ids will, so there is
        /// no guarantee that all and only the ids in the returned map will
        /// be visible and supported by the service in subsequent calls,
        /// nor is there any guarantee that the current display names match
        /// those in the set.  The display names are sorted based on the
        /// comparer provided.
        /// </summary>
        public virtual SortedDictionary<string, string> GetDisplayNames(ULocale locale, CompareInfo com, string matchID)
        {
            return GetDisplayNames(locale, com.ToComparer(), matchID);
        }

        /// <summary>
        /// Return a snapshot of the mapping from display names to visible
        /// IDs for this service.  This set will not change as factories
        /// are added or removed, but the supported ids will, so there is
        /// no guarantee that all and only the ids in the returned map will
        /// be visible and supported by the service in subsequent calls,
        /// nor is there any guarantee that the current display names match
        /// those in the set.  The display names are sorted based on the
        /// comparer provided.
        /// </summary>
        public virtual SortedDictionary<string, string> GetDisplayNames(ULocale locale, IComparer<string> com, string matchID) // ICU4N TODO: API Add overloads for IComparer<StringBuilder> and IComparer<char[]> ?
        {
            SortedDictionary<string, string> dncache = null;
            LocaleRef reference = dnref;

            if (reference != null)
            {
                dncache = reference.Get(locale, com);
            }

            while (dncache == null)
            {
                lock (this)
                {
                    if (reference == dnref || dnref == null)
                    {
                        dncache = new SortedDictionary<string, string>(com); // sorted

                        IDictionary<string, IFactory> m = GetVisibleIDMap();
                        using (var ei = m.GetEnumerator())
                        {
                            while (ei.MoveNext())
                            {
                                var e = ei.Current;
                                string id = e.Key;
                                IFactory f = e.Value;
                                dncache[f.GetDisplayName(id, locale)] = id;
                            }
                        }

                        // ICU4N TODO: Need to make the cache unmodifiable, but stil keep the type a SortedDictionary
                        //dncache = dncache.ToUnmodifiableDictionary();
                        dnref = new LocaleRef(dncache, locale, com);
                    }
                    else
                    {
                        reference = dnref;
                        dncache = reference.Get(locale, com);
                    }
                }
            }

            Key matchKey = CreateKey(matchID);
            if (matchKey == null)
            {
                return dncache;
            }

            // ICU4N: Rather than copying and then removing the items (which isn't allowed with
            // .NET iterators), we reverse the logic and add the items only if they are fallback.
            SortedDictionary<string, string> result = new SortedDictionary<string, string>(((SortedDictionary<string, string>)dncache).Comparer);
            using (var iter = dncache.GetEnumerator())
            {
                while (iter.MoveNext())
                {
                    var e = iter.Current;
                    if (matchKey.IsFallbackOf(e.Value))
                    {
                        result.Add(e.Key, e.Value);
                    }
                }
            }
            return result;
        }

        // we define a class so we get atomic simultaneous access to the
        // locale, comparator, and corresponding map.
        private class LocaleRef
        {
            private readonly ULocale locale;
            private SortedDictionary<string, string> dnCache;
            private IComparer<string> com;

            internal LocaleRef(SortedDictionary<string, string> dnCache, ULocale locale, IComparer<string> com)
            {
                this.locale = locale;
                this.com = com;
                this.dnCache = dnCache;
            }


            internal virtual SortedDictionary<string, string> Get(ULocale loc, IComparer<string> comp)
            {
                SortedDictionary<string, string> m = dnCache;
                if (m != null &&
                    this.locale.Equals(loc) &&
                    (this.com == comp || (this.com != null && this.com.Equals(comp))))
                {

                    return m;
                }
                return null;
            }
        }
        private LocaleRef dnref;

        /**
         * Return a snapshot of the currently registered factories.  There
         * is no guarantee that the list will still match the current
         * factory list of the service subsequent to this call.
         */
        public IList<IFactory> Factories()
        {
            try
            {
                factoryLock.AcquireRead();
                return new List<IFactory>(factories);
            }
            finally
            {
                factoryLock.ReleaseRead();
            }
        }

        /**
         * A convenience override of registerObject(Object, String, boolean)
         * that defaults visible to true.
         */
        public virtual IFactory RegisterObject(object obj, string id)
        {
            return RegisterObject(obj, id, true);
        }

        /**
         * Register an object with the provided id.  The id will be
         * canonicalized.  The canonicalized ID will be returned by
         * getVisibleIDs if visible is true.
         */
        public virtual IFactory RegisterObject(object obj, string id, bool visible)
        {
            string canonicalID = CreateKey(id).CanonicalID;
            return RegisterFactory(new SimpleFactory(obj, canonicalID, visible));
        }

        /**
         * Register a Factory.  Returns the factory if the service accepts
         * the factory, otherwise returns null.  The default implementation
         * accepts all factories.
         */
        public IFactory RegisterFactory(IFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }
            try
            {
                factoryLock.AcquireWrite();
                factories.Insert(0, factory);
                ClearCaches();
            }
            finally
            {
                factoryLock.ReleaseWrite();
            }
            NotifyChanged();
            return factory;
        }

        /**
         * Unregister a factory.  The first matching registered factory will
         * be removed from the list.  Returns true if a matching factory was
         * removed.
         */
        public bool UnregisterFactory(IFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            bool result = false;
            try
            {
                factoryLock.AcquireWrite();
                if (factories.Remove(factory))
                {
                    result = true;
                    ClearCaches();
                }
            }
            finally
            {
                factoryLock.ReleaseWrite();
            }

            if (result)
            {
                NotifyChanged();
            }
            return result;
        }

        /**
         * Reset the service to the default factories.  The factory
         * lock is acquired and then reInitializeFactories is called.
         */
        public void Reset()
        {
            try
            {
                factoryLock.AcquireWrite();
                ReInitializeFactories();
                ClearCaches();
            }
            finally
            {
                factoryLock.ReleaseWrite();
            }
            NotifyChanged();
        }

        /**
         * Reinitialize the factory list to its default state.  By default
         * this clears the list.  Subclasses can override to provide other
         * default initialization of the factory list.  Subclasses must
         * not call this method directly, as it must only be called while
         * holding write access to the factory list.
         */
        protected virtual void ReInitializeFactories()
        {
            factories.Clear();
        }

        /**
         * Return true if the service is in its default state.  The default
         * implementation returns true if there are no factories registered.
         */
        public virtual bool IsDefault
        {
            get { return factories.Count == defaultSize; }
        }

        /**
         * Set the default size to the current number of registered factories.
         * Used by subclasses to customize the behavior of isDefault.
         */
        protected virtual void MarkDefault()
        {
            defaultSize = factories.Count;
        }

        /**
         * Create a key from an id.  This creates a Key instance.
         * Subclasses can override to define more useful keys appropriate
         * to the factories they accept.  If id is null, returns null.
         */
        public virtual Key CreateKey(string id)
        {
            return id == null ? null : new Key(id);
        }

        /**
         * Clear caches maintained by this service.  Subclasses can
         * override if they implement additional that need to be cleared
         * when the service changes. Subclasses should generally not call
         * this method directly, as it must only be called while
         * synchronized on this.
         */
        protected virtual void ClearCaches()
        {
            // we don't synchronize on these because methods that use them
            // copy before use, and check for changes if they modify the
            // caches.
            cache = null;
            idcache = null;
            dnref = null;
        }

        /**
         * Clears only the service cache.
         * This can be called by subclasses when a change affects the service
         * cache but not the id caches, e.g., when the default locale changes
         * the resolution of ids changes, but not the visible ids themselves.
         */
        protected virtual void ClearServiceCache()
        {
            cache = null;
        }


        /**
         * ServiceListener is the listener that ICUService provides by default.
         * ICUService will notifiy this listener when factories are added to
         * or removed from the service.  Subclasses can provide
         * different listener interfaces that extend EventListener, and modify
         * acceptsListener and notifyListener as appropriate.
         */
        public abstract class ServiceListener : EventListener
        {
            //public override void OnNext(object value)
            //{
            //    ServiceChanged((ICUService)value);
            //}

            public abstract void ServiceChanged(ICUService service);
        }

        /**
         * Return true if the listener is accepted; by default this
         * requires a ServiceListener.  Subclasses can override to accept
         * different listeners.
         */
        protected override bool AcceptsListener(EventListener l)
        {
            return l is ServiceListener;
        }

        /**
         * Notify the listener, which by default is a ServiceListener.
         * Subclasses can override to use a different listener.
         */
        protected override void NotifyListener(EventListener l)
        {
            ((ServiceListener)l).ServiceChanged(this);
        }

        /**
         * When the statistics for this service is already enabled,
         * return the log and resets he statistics.
         * When the statistics is not enabled, this method enable
         * the statistics. Used for debugging purposes.
         */
        public virtual string Stats()
        {
            ICURWLock.Stats stats = factoryLock.ResetStats();
            if (stats != null)
            {
                return stats.ToString();
            }
            return "no stats";
        }

        /**
         * Return the name of this service. This will be the empty string if none was assigned.
         */
        public virtual string Name
        {
            get { return name; }
        }

        /**
         * Returns the result of super.toString, appending the name in curly braces.
         */
        public override string ToString()
        {
            // ICU4N TODO: Fix "base" implementation so it returns
            // the same string as ICUNotifier in icu4j
            return base.ToString() + "{" + name + "}";
        }
    }
}
