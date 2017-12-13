using ICU4N.Support.IO;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ICU4N.Impl
{
    internal class ICUResourceBundleImpl : ICUResourceBundle
    {
        protected int resource;

        protected ICUResourceBundleImpl(ICUResourceBundleImpl container, string key, int resource)
            : base(container, key)
        {
            this.resource = resource;
        }
        internal ICUResourceBundleImpl(WholeBundle wholeBundle)
            : base(wholeBundle)
        {
            resource = wholeBundle.reader.RootResource;
        }
        public int GetResource()
        {
            return resource;
        }
        protected ICUResourceBundle CreateBundleObject(string _key,
                                                             int _resource,
                                                             IDictionary<string, string> aliasesVisited,
                                                             UResourceBundle requested)
        {
            switch (ICUResourceBundleReader.RES_GET_TYPE(_resource))
            {
                case STRING:
                case STRING_V2:
                    return new ICUResourceBundleImpl.ResourceString(this, _key, _resource);
                case BINARY:
                    return new ICUResourceBundleImpl.ResourceBinary(this, _key, _resource);
                case ALIAS:
                    return GetAliasedResource(this, null, 0, _key, _resource, aliasesVisited, requested);
                case INT32:
                    return new ICUResourceBundleImpl.ResourceInt(this, _key, _resource);
                case INT32_VECTOR:
                    return new ICUResourceBundleImpl.ResourceIntVector(this, _key, _resource);
                case ARRAY:
                case ARRAY16:
                    return new ICUResourceBundleImpl.ResourceArray(this, _key, _resource);
                case TABLE:
                case TABLE16:
                case TABLE32:
                    return new ICUResourceBundleImpl.ResourceTable(this, _key, _resource);
                default:
                    throw new InvalidOperationException("The resource type is unknown");
            }
        }

        // Scalar values ------------------------------------------------------- ***

        private sealed class ResourceBinary : ICUResourceBundleImpl
        {
            public override int Type
            {
                get { return BINARY; }
            }

            public override ByteBuffer GetBinary()
            {
                return wholeBundle.reader.GetBinary(resource);
            }

            public override byte[] GetBinary(byte[] ba)
            {
                return wholeBundle.reader.GetBinary(resource, ba);
            }
            internal ResourceBinary(ICUResourceBundleImpl container, string key, int resource)
                : base(container, key, resource)
            {
            }
        }
        private sealed class ResourceInt : ICUResourceBundleImpl // ICU4N TODO: API - rename ResourceInt32
        {
            public override int Type
            {
                get { return INT32; }
            }

            public override int GetInt32()
            {
                return ICUResourceBundleReader.RES_GET_INT(resource);
            }

            public override int GetUInt32() // ICU4N TODO: API - should this actually be uint rather than int?
            {
                return ICUResourceBundleReader.RES_GET_UINT(resource);
            }
            internal ResourceInt(ICUResourceBundleImpl container, string key, int resource)
                    : base(container, key, resource)
            {
            }
        }
        private sealed class ResourceString : ICUResourceBundleImpl
        {
            private string value;

            public override int Type
            {
                get { return STRING; }
            }

            public override string GetString()
            {
                if (value != null)
                {
                    return value;
                }
                return wholeBundle.reader.GetString(resource);
            }
            internal ResourceString(ICUResourceBundleImpl container, string key, int resource)
                : base(container, key, resource)
            {
                string s = wholeBundle.reader.GetString(resource);
                // Allow the reader cache's SoftReference to do its job.
                if (s.Length < ICUResourceBundleReader.LARGE_SIZE / 2 ||
                        CacheValue<object>.FutureInstancesWillBeStrong)
                {
                    value = s;
                }
            }
        }
        private sealed class ResourceIntVector : ICUResourceBundleImpl // ICU4N TODO: API Rename ResourceInt32Vector
        {
            public override int Type
            {
                get { return INT32_VECTOR; }
            }

            public override int[] GetInt32Vector()
            {
                return wholeBundle.reader.GetInt32Vector(resource);
            }
            internal ResourceIntVector(ICUResourceBundleImpl container, string key, int resource)
                : base(container, key, resource)
            {
            }
        }

        // Container values ---------------------------------------------------- ***

        internal abstract class ResourceContainer : ICUResourceBundleImpl
        {
            protected internal ICUResourceBundleReader.Container value;

            public override int Length
            {
                get { return value.Length; }
            }

            public override string GetString(int index)
            {
                int res = value.GetContainerResource(wholeBundle.reader, index);
                if (res == RES_BOGUS)
                {
                    throw new IndexOutOfRangeException();
                }
                string s = wholeBundle.reader.GetString(res);
                if (s != null)
                {
                    return s;
                }
                return base.GetString(index);
            }
            protected virtual int GetContainerResource(int index)
            {
                return value.GetContainerResource(wholeBundle.reader, index);
            }
            protected virtual UResourceBundle CreateBundleObject(int index, string resKey, IDictionary<string, string> aliasesVisited,
                                                         UResourceBundle requested)
            {
                int item = GetContainerResource(index);
                if (item == RES_BOGUS)
                {
                    throw new IndexOutOfRangeException();
                }
                return CreateBundleObject(resKey, item, aliasesVisited, requested);
            }

            internal ResourceContainer(ICUResourceBundleImpl container, string key, int resource)
                : base(container, key, resource)
            {
            }
            internal ResourceContainer(WholeBundle wholeBundle)
                : base(wholeBundle)
            {
            }
        }
        internal class ResourceArray : ResourceContainer
        {
            public override int Type
            {
                get { return ARRAY; }
            }

            protected override string[] HandleGetStringArray()
            {
                ICUResourceBundleReader reader = wholeBundle.reader;
                int length = value.Length;
                string[] strings = new string[length];
                for (int i = 0; i < length; ++i)
                {
                    string s = reader.GetString(value.GetContainerResource(reader, i));
                    if (s == null)
                    {
                        throw new UResourceTypeMismatchException("");
                    }
                    strings[i] = s;
                }
                return strings;
            }

            public override string[] GetStringArray()
            {
                return HandleGetStringArray();
            }

            protected override UResourceBundle HandleGet(string indexStr, IDictionary<string, string> aliasesVisited,
                                                UResourceBundle requested)
            {
                int i = int.Parse(indexStr, CultureInfo.InvariantCulture);
                return CreateBundleObject(i, indexStr, aliasesVisited, requested);
            }

            protected override UResourceBundle HandleGet(int index, IDictionary<string, string> aliasesVisited,
                                                UResourceBundle requested)
            {
                return CreateBundleObject(index, index.ToString(CultureInfo.InvariantCulture), aliasesVisited, requested);
            }
            internal ResourceArray(ICUResourceBundleImpl container, string key, int resource)
                : base(container, key, resource)
            {
                value = wholeBundle.reader.GetArray(resource);
            }
        }
        internal class ResourceTable : ResourceContainer
        {
            public override int Type
            {
                get { return TABLE; }
            }
            protected virtual string GetKey(int index)
            {
                return ((ICUResourceBundleReader.Table)value).GetKey(wholeBundle.reader, index);
            }
            protected override ISet<string> HandleKeySet()
            {
                ICUResourceBundleReader reader = wholeBundle.reader;
                SortedSet<string> keySet = new SortedSet<string>(StringComparer.Ordinal);
                ICUResourceBundleReader.Table table = (ICUResourceBundleReader.Table)value;
                for (int i = 0; i < table.Length; ++i)
                {
                    keySet.Add(table.GetKey(reader, i));
                }
                return keySet;
            }

            protected override UResourceBundle HandleGet(string resKey, IDictionary<string, string> aliasesVisited,
                                                UResourceBundle requested)
            {
                int i = ((ICUResourceBundleReader.Table)value).FindTableItem(wholeBundle.reader, resKey);
                if (i < 0)
                {
                    return null;
                }
                return CreateBundleObject(resKey, GetContainerResource(i), aliasesVisited, requested);
            }

            protected override UResourceBundle HandleGet(int index, IDictionary<string, string> aliasesVisited,
                                                UResourceBundle requested)
            {
                string itemKey = ((ICUResourceBundleReader.Table)value).GetKey(wholeBundle.reader, index);
                if (itemKey == null)
                {
                    throw new IndexOutOfRangeException();
                }
                return CreateBundleObject(itemKey, GetContainerResource(index), aliasesVisited, requested);
            }

            protected override object HandleGetObject(string key)
            {
                // Fast path for common cases: Avoid creating UResourceBundles if possible.
                // It would be even better if we could override getString(key)/getStringArray(key),
                // so that we know the expected object type,
                // but those are final in java.util.ResourceBundle.
                ICUResourceBundleReader reader = wholeBundle.reader;
                int index = ((ICUResourceBundleReader.Table)value).FindTableItem(reader, key);
                if (index >= 0)
                {
                    int res = value.GetContainerResource(reader, index);
                    // getString(key)
                    string s = reader.GetString(res);
                    if (s != null)
                    {
                        return s;
                    }
                    // getStringArray(key)
                    ICUResourceBundleReader.Container array = reader.GetArray(res);
                    if (array != null)
                    {
                        int length = array.Length;
                        string[] strings = new string[length];
                        for (int j = 0; ; ++j)
                        {
                            if (j == length)
                            {
                                return strings;
                            }
                            s = reader.GetString(array.GetContainerResource(reader, j));
                            if (s == null)
                            {
                                // Equivalent to resolveObject(key, requested):
                                // If this is not a string array,
                                // then build and return a UResourceBundle.
                                break;
                            }
                            strings[j] = s;
                        }
                    }
                }
                return base.HandleGetObject(key);
            }
            /// <summary>
            /// Returns a String if found, or null if not found or if the key item is not a string.
            /// </summary>
            internal virtual string FindString(string key)
            {
                ICUResourceBundleReader reader = wholeBundle.reader;
                int index = ((ICUResourceBundleReader.Table)value).FindTableItem(reader, key);
                if (index < 0)
                {
                    return null;
                }
                return reader.GetString(value.GetContainerResource(reader, index));
            }
            internal ResourceTable(ICUResourceBundleImpl container, string key, int resource)
                : base(container, key, resource)
            {
                value = wholeBundle.reader.GetTable(resource);
            }
            /// <summary>
            /// Constructor for the root table of a bundle.
            /// </summary>
            internal ResourceTable(WholeBundle wholeBundle, int rootRes)
                : base(wholeBundle)
            {
                value = wholeBundle.reader.GetTable(rootRes);
            }
        }
    }
}
