using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JCG = J2N.Collections.Generic;

namespace ICU4N.Support.Text
{
    // from Apache Harmony

    /// <summary>
    /// Holds a string with attributes describing the characters of
    /// this string.
    /// </summary>
    internal class AttributedString
#if FEATURE_CLONEABLE
        : ICloneable
#endif
    {
        internal string text;

        internal IDictionary<AttributedCharacterIteratorAttribute, IList<Range>> attributeMap;

        private object syncLock = new object();

        internal class Range
        {
            internal Guid id;

            internal int start;

            internal int end;

            internal object value;

            internal Range(int s, int e, object v)
            {
                id = Guid.NewGuid();
                start = s;
                end = e;
                value = v;
            }

            public override bool Equals(object obj)
            {
                var other = obj as Range;
                if (other == null)
                    return false;
                return this.id == other.id;
            }

            public override int GetHashCode()
            {
                return id.GetHashCode();
            }
        }

        internal class AttributedIterator : AttributedCharacterIterator
        {

            private int begin, end, offset;

            private AttributedString attrString;

            private ISet<AttributedCharacterIteratorAttribute> attributesAllowed;

            internal AttributedIterator(AttributedString attrString)
            {
                this.attrString = attrString;
                begin = 0;
                end = attrString.text.Length;
                offset = 0;
            }

            internal AttributedIterator(AttributedString attrString,
                    AttributedCharacterIteratorAttribute[] attributes, int begin,
                    int end)
            {
                if (begin < 0 || end > attrString.text.Length || begin > end)
                {
                    throw new ArgumentException();
                }
                this.begin = begin;
                this.end = end;
                offset = begin;
                this.attrString = attrString;
                if (attributes != null)
                {
                    var set = new JCG.HashSet<AttributedCharacterIteratorAttribute>(
                            (attributes.Length * 4 / 3) + 1);
                    for (int i = attributes.Length; --i >= 0;)
                    {
                        set.Add(attributes[i]);
                    }
                    attributesAllowed = set;
                }
            }

            /// <summary>
            /// Returns a new <see cref="AttributedIterator"/> with the same source string,
            /// begin, end, and current index as this attributed iterator.
            /// </summary>
            /// <returns>A shallow copy of this attributed iterator.</returns>
            public override object Clone()
            {
                AttributedIterator clone = (AttributedIterator)base.MemberwiseClone();
                if (attributesAllowed != null)
                {
                    clone.attributesAllowed = new JCG.HashSet<AttributedCharacterIteratorAttribute>(attributesAllowed);
                }
                return clone;
            }

            public override char Current
            {
                get
                {
                    if (offset == end)
                    {
                        return Done;
                    }
                    return attrString.text[offset];
                }
            }

            public override char First()
            {
                if (begin == end)
                {
                    return Done;
                }
                offset = begin;
                return attrString.text[offset];
            }

            /// <summary>
            /// Gets the begin index in the source string.
            /// </summary>
            public override int BeginIndex => begin;

            /// <summary>
            /// Gets the end index in the source String. The index is one past the last character to iterate.
            /// </summary>
            public override int EndIndex => end;

            /// <summary>
            /// Gets the current index in the source String.
            /// </summary>
            public override int Index => offset;


            private bool InRange(Range range)
            {
                //if (!(range.value is Annotation)) {
                //    return true;
                //}
                return range.start >= begin && range.start < end
                        && range.end > begin && range.end <= end;
            }

            private bool InRange(IList<Range> ranges)
            {
                //Iterator<Range> it = ranges.iterator();
                //while (it.hasNext())
                //{
                //    Range range = it.next();
                foreach (var range in ranges)
                {
                    if (range.start >= begin && range.start < end)
                    {
                        return /*!(range.value is Annotation) ||*/ (range.end > begin && range.end <= end);
                    }
                    else if (range.end > begin && range.end <= end)
                    {
                        return /*!(range.value is Annotation) ||*/ (range.start >= begin && range.start < end);
                    }
                }
                return false;
            }

            /// <summary>
            /// Returns a set of attributes present in the <see cref="AttributedString"/>.
            /// An empty set returned indicates that no attributes where defined.
            /// </summary>
            /// <returns>A set of attribute keys that may be empty.</returns>
            public override ICollection<AttributedCharacterIteratorAttribute> GetAllAttributeKeys()
            {
                if (begin == 0 && end == attrString.text.Length
                        && attributesAllowed == null)
                {
                    return attrString.attributeMap.Keys;
                }

                ISet<AttributedCharacterIteratorAttribute> result = new JCG.HashSet<AttributedCharacterIteratorAttribute>(
                        (attrString.attributeMap.Count * 4 / 3) + 1);
                //Iterator<Map.Entry<Attribute, List<Range>>> it = attrString.attributeMap
                //        .entrySet().iterator();
                //while (it.hasNext())
                //{
                //    Map.Entry<Attribute, List<Range>> entry = it.next();
                foreach (var entry in attrString.attributeMap)
                {
                    if (attributesAllowed == null
                            || attributesAllowed.Contains(entry.Key))
                    {
                        IList<Range> ranges = entry.Value;
                        if (InRange(ranges))
                        {
                            result.Add(entry.Key);
                        }
                    }
                }
                return result;
            }

            private object CurrentValue(IList<Range> ranges)
            {
                //Iterator<Range> it = ranges.iterator();
                //while (it.hasNext())
                //{
                //    Range range = it.next();
                foreach (var range in ranges)
                {
                    if (offset >= range.start && offset < range.end)
                    {
                        return InRange(range) ? range.value : null;
                    }
                }
                return null;
            }

            public override object GetAttribute(
                    AttributedCharacterIteratorAttribute attribute)
            {
                if (attributesAllowed != null
                        && !attributesAllowed.Contains(attribute))
                {
                    return null;
                }
                IList<Range> ranges; // = (IList<Range>)attrString.attributeMap.get(attribute);
                attrString.attributeMap.TryGetValue(attribute, out ranges);
                if (ranges == null)
                {
                    return null;
                }
                return CurrentValue(ranges);
            }

            public override IDictionary<AttributedCharacterIteratorAttribute, object> GetAttributes()
            {
                IDictionary<AttributedCharacterIteratorAttribute, object> result = new Dictionary<AttributedCharacterIteratorAttribute, object>(
                        (attrString.attributeMap.Count * 4 / 3) + 1);
                //Iterator<Map.Entry<Attribute, List<Range>>> it = attrString.attributeMap
                //        .entrySet().iterator();
                //while (it.hasNext())
                //{
                //    Map.Entry<Attribute, List<Range>> entry = it.next();
                foreach (var entry in attrString.attributeMap)
                {
                    if (attributesAllowed == null
                            || attributesAllowed.Contains(entry.Key))
                    {
                        object value = CurrentValue(entry.Value);
                        if (value != null)
                        {
                            result[entry.Key] = value;
                        }
                    }
                }
                return result;
            }

            public override int GetRunLimit()
            {
                return GetRunLimit(GetAllAttributeKeys());
            }

            private int RunLimit(IList<Range> ranges)
            {
                int result = end;
                //ListIterator<Range> it = ranges.listIterator(ranges.size());
                //while (it.hasPrevious())
                //{
                //    Range range = it.previous();
                foreach (var range in ranges.Reverse())
                {
                    if (range.end <= begin)
                    {
                        break;
                    }
                    if (offset >= range.start && offset < range.end)
                    {
                        return InRange(range) ? range.end : result;
                    }
                    else if (offset >= range.end)
                    {
                        break;
                    }
                    result = range.start;
                }
                return result;
            }

            public override int GetRunLimit(AttributedCharacterIteratorAttribute attribute)
            {
                if (attributesAllowed != null
                        && !attributesAllowed.Contains(attribute))
                {
                    return end;
                }
                IList<Range> ranges;// = (ArrayList<Range>)attrString.attributeMap.get(attribute);
                attrString.attributeMap.TryGetValue(attribute, out ranges);
                if (ranges == null)
                {
                    return end;
                }
                return RunLimit(ranges);
            }

            public override int GetRunLimit<T>(ICollection<T> attributes) 
            {
                int limit = end;
                //Iterator <? extends Attribute > it = attributes.iterator();
                //while (it.hasNext())
                //{
                //    AttributedCharacterIterator.Attribute attribute = it.next();
                foreach (var attribute in attributes)
                {
                    int newLimit = GetRunLimit(attribute);
                    if (newLimit < limit)
                    {
                        limit = newLimit;
                    }
                }
                return limit;
            }

            public override int GetRunStart()
            {
                return GetRunStart(GetAllAttributeKeys());
            }

            private int RunStart(IList<Range> ranges)
            {
                int result = begin;
                //Iterator<Range> it = ranges.iterator();
                //while (it.hasNext())
                //{
                //    Range range = it.next();
                foreach (var range in ranges)
                {
                    if (range.start >= end)
                    {
                        break;
                    }
                    if (offset >= range.start && offset < range.end)
                    {
                        return InRange(range) ? range.start : result;
                    }
                    else if (offset < range.start)
                    {
                        break;
                    }
                    result = range.end;
                }
                return result;
            }

            public override int GetRunStart(AttributedCharacterIteratorAttribute attribute)
            {
                if (attributesAllowed != null
                        && !attributesAllowed.Contains(attribute))
                {
                    return begin;
                }
                IList<Range> ranges; // = (ArrayList<Range>)attrString.attributeMap.get(attribute);
                attrString.attributeMap.TryGetValue(attribute, out ranges);
                if (ranges == null)
                {
                    return begin;
                }
                return RunStart(ranges);
            }

            public override int GetRunStart<T>(ICollection<T> attributes)
            {
                int start = begin;
                //Iterator <? extends Attribute > it = attributes.iterator();
                //while (it.hasNext())
                //{
                //    AttributedCharacterIterator.Attribute attribute = it.next();
                foreach (var attribute in attributes)
                {
                    int newStart = GetRunStart(attribute);
                    if (newStart > start)
                    {
                        start = newStart;
                    }
                }
                return start;
            }

            public override char Last()
            {
                if (begin == end)
                {
                    return Done;
                }
                offset = end - 1;
                return attrString.text[offset];
            }

            public override char Next()
            {
                if (offset >= (end - 1))
                {
                    offset = end;
                    return Done;
                }
                return attrString.text[++offset];
            }

            public override char Previous()
            {
                if (offset == begin)
                {
                    return Done;
                }
                return attrString.text[--offset];
            }

            public override char SetIndex(int location)
            {
                if (location < begin || location > end)
                {
                    throw new ArgumentException();
                }
                offset = location;
                if (offset == end)
                {
                    return Done;
                }
                return attrString.text[offset];
            }
        }

        /// <summary>
        /// Constructs an <see cref="AttributedString"/> from an 
        /// <see cref="AttributedCharacterIterator"/>, which represents attributed text.
        /// </summary>
        /// <param name="iterator">The <see cref="AttributedCharacterIterator"/> that contains the text
        /// for this attributed string.</param>
        public AttributedString(AttributedCharacterIterator iterator)
        {
            if (iterator.BeginIndex > iterator.EndIndex)
            {
                // text.0A=Invalid substring range
                throw new ArgumentException(/*Messages.getString("text.0A")*/); //$NON-NLS-1$
            }
            StringBuilder buffer = new StringBuilder();
            for (int i = iterator.BeginIndex; i < iterator.EndIndex; i++)
            {
                buffer.Append(iterator.Current);
                iterator.Next();
            }
            text = buffer.ToString();
            var attributes = iterator
                    .GetAllAttributeKeys();
            if (attributes == null)
            {
                return;
            }
            attributeMap = new Dictionary<AttributedCharacterIteratorAttribute, IList<Range>>(
                    /*(attributes.size() * 4 / 3) + 1*/);

            //Iterator<Attribute> it = attributes.iterator();
            //while (it.hasNext())
            //{
            //    AttributedCharacterIterator.Attribute attribute = it.next();
            foreach (var attribute in attributes)
            {
                iterator.SetIndex(0);
                while (iterator.Current != CharacterIterator.Done)
                {
                    int start = iterator.GetRunStart(attribute);
                    int limit = iterator.GetRunLimit(attribute);
                    object value = iterator.GetAttribute(attribute);
                    if (value != null)
                    {
                        AddAttribute(attribute, value, start, limit);
                    }
                    iterator.SetIndex(limit);
                }
            }
        }

        private AttributedString(AttributedCharacterIterator iterator, int start,
                int end, ICollection<AttributedCharacterIteratorAttribute> attributes)
        {
            if (start < iterator.BeginIndex || end > iterator.EndIndex
                    || start > end)
            {
                throw new ArgumentException();
            }

            if (attributes == null)
            {
                return;
            }

            StringBuilder buffer = new StringBuilder();
            iterator.SetIndex(start);
            while (iterator.Index < end)
            {
                buffer.Append(iterator.Current);
                iterator.Next();
            }
            text = buffer.ToString();
            attributeMap = new Dictionary<AttributedCharacterIteratorAttribute, IList<Range>>(
                    /*(attributes.size() * 4 / 3) + 1*/);

            //Iterator<Attribute> it = attributes.iterator();
            //while (it.hasNext())
            //{
            //    AttributedCharacterIterator.Attribute attribute = it.next();
            foreach (var attribute in attributes)
            {
                iterator.SetIndex(start);
                while (iterator.Index < end)
                {
                    Object value = iterator.GetAttribute(attribute);
                    int runStart = iterator.GetRunStart(attribute);
                    int limit = iterator.GetRunLimit(attribute);
                    if (/*(value is Annotation && runStart >= start && limit <= end)
                        ||*/ (value != null /*&& !(value instanceof Annotation)*/))
                    {
                        AddAttribute(attribute, value, (runStart < start ? start
                                : runStart)
                                - start, (limit > end ? end : limit) - start);
                    }
                    iterator.SetIndex(limit);
                }
            }
        }

        /// <summary>
        /// Constructs an <see cref="AttributedString"/> from a range of the text contained
        /// in the specified <see cref="AttributedCharacterIterator"/>, starting at 
        /// <paramref name="start"/> and ending at <paramref name="end"/>. All attributes will be copied to this
        /// attributed string.
        /// </summary>
        /// <param name="iterator">The <see cref="AttributedCharacterIterator"/> that contains the text
        /// for this attributed string.</param>
        /// <param name="start">the start index of the range of the copied text.</param>
        /// <param name="end">the end index of the range of the copied text.</param>
        /// <exception cref="ArgumentException">if <paramref name="start"/> is less than first index of
        /// <paramref name="iterator"/>, <paramref name="end"/> is greater than the last index +
        /// 1 in <paramref name="iterator"/> or if <paramref name="start"/> &gt; <paramref name="end"/>.</exception>
        public AttributedString(AttributedCharacterIterator iterator, int start,
                int end)
                : this(iterator, start, end, iterator.GetAllAttributeKeys())
        {
        }

        /// <summary>
        /// Constructs an <see cref="AttributedString"/> from a range of the text contained
        /// in the specified <see cref="AttributedCharacterIterator"/>, starting at 
        /// <paramref name="start"/>, ending at <paramref name="end"/> and it will copy the attributes defined in
        /// the specified set. If the set is <c>null</c> then all attributes are
        /// copied.
        /// </summary>
        /// <param name="iterator">The <see cref="AttributedCharacterIterator"/> that contains the text
        /// for this attributed string.</param>
        /// <param name="start">The start index of the range of the copied text.</param>
        /// <param name="end">The end index of the range of the copied text.</param>
        /// <param name="attributes">The set of attributes that will be copied, or all if it is
        /// <c>null</c>.</param>
        /// <exception cref="ArgumentException">if <paramref name="start"/> is less than first index of
        /// <paramref name="iterator"/>, <paramref name="end"/> is greater than the last index +
        /// 1 in <paramref name="iterator"/> or if <paramref name="start"/> &gt; <paramref name="end"/>.</exception>
        public AttributedString(AttributedCharacterIterator iterator, int start,
                int end, AttributedCharacterIteratorAttribute[] attributes)
                : this(iterator, start, end, new JCG.HashSet<AttributedCharacterIteratorAttribute>(attributes))
        {
        }

        /// <summary>
        /// Creates an <see cref="AttributedString"/> from the given text.
        /// </summary>
        /// <param name="value">the text to take as base for this attributed string.</param>
        public AttributedString(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            text = value;
            attributeMap = new Dictionary<AttributedCharacterIteratorAttribute, IList<Range>>(11);
        }

        /// <summary>
        /// Creates an <see cref="AttributedString"/> from the given text and the
        /// <paramref name="attributes"/>. The whole text has the given attributes applied.
        /// </summary>
        /// <param name="value">the text to take as base for this attributed string.</param>
        /// <param name="attributes">the attributes that the text is associated with.</param>
        /// <exception cref="ArgumentException">if the length of <paramref name="value"/> is 0 but the size of
        /// <paramref name="attributes"/> is greater than 0.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <c>null</c>.</exception>
        public AttributedString(string value,
                IDictionary<AttributedCharacterIteratorAttribute, object> attributes)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (value.Length == 0 && attributes.Any())
            {
                // text.0B=Cannot add attributes to empty string
                throw new ArgumentException(/*Messages.getString("text.0B")*/); //$NON-NLS-1$
            }
            text = value;
            attributeMap = new Dictionary<AttributedCharacterIteratorAttribute, IList<Range>>(
                    (attributes.Count * 4 / 3) + 1);
            //Iterator <?> it = attributes.entrySet().iterator();
            //while (it.hasNext())
            //{
            //    Map.Entry <?, ?> entry = (Map.Entry <?, ?>) it.next();
            foreach (var entry in attributes)
            {
                IList<Range> ranges = new List<Range>(1);
                ranges.Add(new Range(0, text.Length, entry.Value));
                attributeMap[(AttributedCharacterIteratorAttribute)entry.Key] = ranges;
            }
        }

        /// <summary>
        /// Applies a given attribute to this string.
        /// </summary>
        /// <param name="attribute">The attribute that will be applied to this string.</param>
        /// <param name="value">The value of the attribute that will be applied to this string.</param>
        /// <exception cref="ArgumentException">If the length of this attributed string is 0.</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="attribute"/> is <c>null</c>.</exception>
        public void AddAttribute(AttributedCharacterIteratorAttribute attribute,
                object value)
        {
            if (null == attribute)
            {
                throw new ArgumentNullException(nameof(attribute));
            }
            if (text.Length == 0)
            {
                throw new ArgumentException();
            }

            IList<Range> ranges; //= attributeMap.get(attribute);
            attributeMap.TryGetValue(attribute, out ranges);
            if (ranges == null)
            {
                ranges = new List<Range>(1);
                attributeMap[attribute] = ranges;
            }
            else
            {
                ranges.Clear();
            }
            ranges.Add(new Range(0, text.Length, value));
        }

        /// <summary>
        /// Applies a given attribute to the given range of this string.
        /// </summary>
        /// <param name="attribute">The attribute that will be applied to this string.</param>
        /// <param name="value">The value of the attribute that will be applied to this string.</param>
        /// <param name="start">The start of the range where the attribute will be applied.</param>
        /// <param name="end">The end of the range where the attribute will be applied.</param>
        /// <exception cref="ArgumentException">If <c>start &lt; 0</c>, <paramref name="end"/> is greater than the length
        /// of this string, or if <c>start &gt;= end</c>.</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="attribute"/> is <c>null</c>.</exception>
        public void AddAttribute(AttributedCharacterIteratorAttribute attribute,
                object value, int start, int end)
        {
            throw new NotImplementedException(); // ICU4N TODO:
            //if (null == attribute)
            //{
            //    throw new ArgumentNullException(nameof(attribute));
            //}
            //if (start < 0 || end > text.Length || start >= end)
            //{
            //    throw new ArgumentException();
            //}

            //if (value == null)
            //{
            //    return;
            //}

            //lock (syncLock)
            //{
            //    IList<Range> ranges; // = attributeMap.get(attribute);
            //    attributeMap.TryGetValue(attribute, out ranges);
            //    if (ranges == null)
            //    {
            //        ranges = new List<Range>(1);
            //        ranges.Add(new Range(start, end, value));
            //        attributeMap[attribute] = ranges;
            //        return;
            //    }
            //    //ListIterator<Range> it = ranges.listIterator();
            //    //while (it.hasNext())
            //    //{
            //    //    Range range = it.next();
            //    var toRemove = new List<Range>();
            //    var toAdd = new Dictionary<Range, int>();
            //    int position = 0;
            //    for (int i = 0; i < ranges.Count; i++)
            //    {
            //        position = i;
            //        Range range = ranges[i];
            //        if (end <= range.start)
            //        {
            //            //it.previous();
            //            position--;
            //            break;
            //        }
            //        else if (start < range.end
            //              || (start == range.end && value.Equals(range.value)))
            //        {
            //            Range r1 = null, r3;
            //            //it.remove();
            //            toRemove.Add(range);
            //            r1 = new Range(range.start, start, range.value);
            //            r3 = new Range(end, range.end, range.value);

            //            while (end > range.end && i < ranges.Count /*it.hasNext()*/)
            //            {
            //                //range = it.next();
            //                i++;
            //                position = i;
            //                range = ranges[i];
            //                if (end <= range.end)
            //                {
            //                    if (end > range.start
            //                            || (end == range.start && value.Equals(range.value)))
            //                    {
            //                        //it.remove();
            //                        toRemove.Add(range);
            //                        r3 = new Range(end, range.end, range.value);
            //                        break;
            //                    }
            //                }
            //                else
            //                {
            //                    //it.remove();
            //                    toRemove.Add(range);
            //                }
            //            }

            //            if (value.Equals(r1.value))
            //            {
            //                if (value.Equals(r3.value))
            //                {
            //                    it.add(new Range(r1.start < start ? r1.start : start,
            //                            r3.end > end ? r3.end : end, r1.value));
            //                }
            //                else
            //                {
            //                    it.add(new Range(r1.start < start ? r1.start : start,
            //                            end, r1.value));
            //                    if (r3.start < r3.end)
            //                    {
            //                        it.add(r3);
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                if (value.Equals(r3.value))
            //                {
            //                    if (r1.start < r1.end)
            //                    {
            //                        it.add(r1);
            //                    }
            //                    it.add(new Range(start, r3.end > end ? r3.end : end,
            //                            r3.value));
            //                }
            //                else
            //                {
            //                    if (r1.start < r1.end)
            //                    {
            //                        it.add(r1);
            //                    }
            //                    it.add(new Range(start, end, value));
            //                    if (r3.start < r3.end)
            //                    {
            //                        it.add(r3);
            //                    }
            //                }
            //            }
            //            return;
            //        }
            //    }
            //    it.add(new Range(start, end, value));
            //}

            //ListIterator<Range> it = ranges.listIterator();
            //while (it.hasNext())
            //{
            //    Range range = it.next();
            //    if (end <= range.start)
            //    {
            //        it.previous();
            //        break;
            //    }
            //    else if (start < range.end
            //          || (start == range.end && value.Equals(range.value)))
            //    {
            //        Range r1 = null, r3;
            //        it.remove();
            //        r1 = new Range(range.start, start, range.value);
            //        r3 = new Range(end, range.end, range.value);

            //        while (end > range.end && it.hasNext())
            //        {
            //            range = it.next();
            //            if (end <= range.end)
            //            {
            //                if (end > range.start
            //                        || (end == range.start && value.Equals(range.value)))
            //                {
            //                    it.remove();
            //                    r3 = new Range(end, range.end, range.value);
            //                    break;
            //                }
            //            }
            //            else
            //            {
            //                it.remove();
            //            }
            //        }

            //        if (value.Equals(r1.value))
            //        {
            //            if (value.Equals(r3.value))
            //            {
            //                it.add(new Range(r1.start < start ? r1.start : start,
            //                        r3.end > end ? r3.end : end, r1.value));
            //            }
            //            else
            //            {
            //                it.add(new Range(r1.start < start ? r1.start : start,
            //                        end, r1.value));
            //                if (r3.start < r3.end)
            //                {
            //                    it.add(r3);
            //                }
            //            }
            //        }
            //        else
            //        {
            //            if (value.Equals(r3.value))
            //            {
            //                if (r1.start < r1.end)
            //                {
            //                    it.add(r1);
            //                }
            //                it.add(new Range(start, r3.end > end ? r3.end : end,
            //                        r3.value));
            //            }
            //            else
            //            {
            //                if (r1.start < r1.end)
            //                {
            //                    it.add(r1);
            //                }
            //                it.add(new Range(start, end, value));
            //                if (r3.start < r3.end)
            //                {
            //                    it.add(r3);
            //                }
            //            }
            //        }
            //        return;
            //    }
            //}
            //it.add(new Range(start, end, value));
        }

        /// <summary>
        /// Applies a given set of attributes to the given range of the string.
        /// </summary>
        /// <param name="attributes">the set of attributes that will be applied to this string.</param>
        /// <param name="start">the start of the range where the attribute will be applied.</param>
        /// <param name="end">the end of the range where the attribute will be applied.</param>
        /// <exception cref="ArgumentException">If <c>start &lt; 0</c>, <paramref name="end"/> is greater than the length
        /// of this string, or if <c>start &gt;= end</c>.</exception>
        public void AddAttributes(
                IDictionary<AttributedCharacterIteratorAttribute, object> attributes,
                int start, int end)
        {
            //Iterator <?> it = attributes.entrySet().iterator();
            //while (it.hasNext())
            //{
            //    Map.Entry <?, ?> entry = (Map.Entry <?, ?>) it.next();
            foreach (var entry in attributes)
            {
                AddAttribute(
                        (AttributedCharacterIteratorAttribute)entry.Key,
                        entry.Value, start, end);
            }
        }

        /// <summary>
        /// Returns an <see cref="AttributedCharacterIterator"/> that gives access to the
        /// complete content of this attributed string.
        /// </summary>
        /// <returns>The newly created <see cref="AttributedCharacterIterator"/>.</returns>
        public AttributedCharacterIterator GetIterator()
        {
            return new AttributedIterator(this);
        }

        /// <summary>
        /// Returns an <see cref="AttributedCharacterIterator"/> that gives access to the
        /// complete content of this attributed string. Only attributes contained in
        /// <paramref name="attributes"/> are available from this iterator if they are defined
        /// for this text.
        /// </summary>
        /// <param name="attributes">the array containing attributes that will be in the new
        /// iterator if they are defined for this text.</param>
        /// <returns>The newly created <see cref="AttributedCharacterIterator"/>.</returns>
        public AttributedCharacterIterator GetIterator(
                AttributedCharacterIteratorAttribute[] attributes)
        {
            return new AttributedIterator(this, attributes, 0, text.Length);
        }

        /// <summary>
        /// Returns an <see cref="AttributedCharacterIterator"/> that gives access to the
        /// contents of this attributed string starting at index <paramref name="start"/> up to
        /// index <paramref name="end"/>. Only attributes contained in <paramref name="attributes"/> are
        /// available from this iterator if they are defined for this text.
        /// </summary>
        /// <param name="attributes">the array containing attributes that will be in the new
        /// iterator if they are defined for this text.</param>
        /// <param name="start">the start index of the iterator on the underlying text.</param>
        /// <param name="end">the end index of the iterator on the underlying text.</param>
        /// <returns>The newly created <see cref="AttributedCharacterIterator"/>.</returns>
        public AttributedCharacterIterator GetIterator(
                AttributedCharacterIteratorAttribute[] attributes, int start,
                int end)
        {
            return new AttributedIterator(this, attributes, start, end);
        }
    }
}
