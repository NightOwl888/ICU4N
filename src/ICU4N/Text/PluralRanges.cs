using ICU4N.Impl;
using ICU4N.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// Utility class for returning the plural category for a range of numbers, such as 1–5, so that appropriate messages can
    /// be chosen. The rules for determining this value vary widely across locales.
    /// </summary>
    /// <author>markdavis</author>
    /// <internal/>
    [Obsolete("This API is ICU internal only.")]
    internal sealed class PluralRanges : IFreezable<PluralRanges>, IComparable<PluralRanges> // ICU4N: Marked internal since it is obsolete anyway
    {
        private volatile bool isFrozen;
        private Matrix matrix = new Matrix();
        private bool[] @explicit = new bool[StandardPluralUtil.Count];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public PluralRanges()
        {
        }

        /// <summary>
        /// Internal class for mapping from two StandardPluralCategories values to another.
        /// </summary>
        private sealed class Matrix : IComparable<Matrix>
#if FEATURE_CLONEABLE
            , ICloneable
#endif
        {
            private sbyte[] data = new sbyte[StandardPluralUtil.Count * StandardPluralUtil.Count];


            internal Matrix()
            {
                for (int i = 0; i < data.Length; ++i)
                {
                    data[i] = (sbyte)-1;
                }
            }

            /// <summary>
            /// Internal method for setting.
            /// </summary>
            internal void Set(StandardPlural start, StandardPlural end, StandardPlural result)
            {
                data[(int)start * StandardPluralUtil.Count + (int)end] = /*result == null ? (sbyte)-1 :*/ (sbyte)result;
            }

            /// <summary>
            /// Internal method for setting; throws exception if already set.
            /// </summary>
            internal void SetIfNew(StandardPlural start, StandardPlural end,
                    StandardPlural result)
            {
                sbyte old = data[(int)start * StandardPluralUtil.Count + (int)end];
                if (old >= 0)
                {
                    throw new ArgumentException("Previously set value for <" + start + ", " + end + ", "
                            + StandardPluralUtil.Values[old] + ">");
                }
                data[(int)start * StandardPluralUtil.Count + (int)end] = /*result == null ? (sbyte)-1 :*/ (sbyte)result;
            }

            /// <summary>
            /// Internal method for getting.
            /// </summary>
            internal StandardPlural? Get(StandardPlural start, StandardPlural end)
            {
                sbyte result = data[(int)start * StandardPluralUtil.Count + (int)end];
                return result < 0 ? (StandardPlural?)null : StandardPluralUtil.Values[result];
            }

            /// <summary>
            /// Internal method to see if &lt;*,end&gt; values are all the same.
            /// </summary>
            internal StandardPlural? EndSame(StandardPlural end)
            {
                StandardPlural? first = null;
                foreach (StandardPlural start in StandardPluralUtil.Values)
                {
                    StandardPlural? item = Get(start, end);
                    if (item == null)
                    {
                        continue;
                    }
                    if (first == null)
                    {
                        first = item;
                        continue;
                    }
                    if (first != item)
                    {
                        return null;
                    }
                }
                return first;
            }

            /// <summary>
            /// Internal method to see if &lt;start,*&gt; values are all the same.
            /// </summary>
            internal StandardPlural? StartSame(StandardPlural start,
                    IList<StandardPlural> endDone, out bool emit)
            {
                emit = false;
                StandardPlural? first = null;
                foreach (StandardPlural end in StandardPluralUtil.Values)
                {
                    StandardPlural? item = Get(start, end);
                    if (item == null)
                    {
                        continue;
                    }
                    if (first == null)
                    {
                        first = item;
                        continue;
                    }
                    if (first != item)
                    {
                        return null;
                    }
                    // only emit if we didn't cover with the 'end' values
                    if (!endDone.Contains(end))
                    {
                        emit = true;
                    }
                }
                return first;
            }


            public override int GetHashCode()
            {
                int result = 0;
                for (int i = 0; i < data.Length; ++i)
                {
                    result = result * 37 + data[i];
                }
                return result;
            }

            public override bool Equals(Object other)
            {
                if (!(other is Matrix))
                {
                    return false;
                }
                return 0 == CompareTo((Matrix)other);
            }


            public int CompareTo(Matrix o)
            {
                for (int i = 0; i < data.Length; ++i)
                {
                    int diff = data[i] - o.data[i];
                    if (diff != 0)
                    {
                        return diff;
                    }
                }
                return 0;
            }


            public Matrix Clone()
            {
                Matrix result = new Matrix();
                result.data = (sbyte[])data.Clone();
                return result;
            }

            public override String ToString()
            {
                StringBuilder result = new StringBuilder();
                foreach (StandardPlural i in Enum.GetValues(typeof(StandardPlural)))
                {
                    foreach (StandardPlural j in Enum.GetValues(typeof(StandardPlural)))
                    {
                        StandardPlural? x = Get(i, j);
                        if (x != null)
                        {
                            result.Append(i + " & " + j + " → " + x + ";\n");
                        }
                    }
                }
                return result.ToString();
            }
        }

        /// <summary>
        /// Internal method for building. If the start or end are null, it means everything of that type.
        /// </summary>
        /// <param name="rangeStart">plural category for the start of the range</param>
        /// <param name="rangeEnd">plural category for the end of the range</param>
        /// <param name="result">the resulting plural category</param>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public void Add(StandardPlural rangeStart, StandardPlural rangeEnd,
                    StandardPlural result)
        {
            if (isFrozen)
            {
                throw new InvalidOperationException();
            }
            @explicit[(int)result] = true;
            // ICU4N TODO: API Need to work out whether we should nullify or add another element to StandardPlural enum
            if (rangeStart == null)
            {
                foreach (StandardPlural rs in Enum.GetValues(typeof(StandardPlural)))
                {
                    if (rangeEnd == null)
                    {
                        foreach (StandardPlural re in Enum.GetValues(typeof(StandardPlural)))
                        {
                            matrix.SetIfNew(rs, re, result);
                        }
                    }
                    else
                    {
                        @explicit[(int)rangeEnd] = true;
                        matrix.SetIfNew(rs, rangeEnd, result);
                    }
                }
            }
            else if (rangeEnd == null)
            {
                @explicit[(int)rangeStart] = true;
                foreach (StandardPlural re in Enum.GetValues(typeof(StandardPlural)))
                {
                    matrix.SetIfNew(rangeStart, re, result);
                }
            }
            else
            {
                @explicit[(int)rangeStart] = true;
                @explicit[(int)rangeEnd] = true;
                matrix.SetIfNew(rangeStart, rangeEnd, result);
            }
        }

        /// <summary>
        /// Returns the appropriate plural category for a range from <paramref name="start"/> to <paramref name="end"/>. If there is no available data, then
        /// 'end' is returned as an implicit value. (Such an implicit value can be tested for with <see cref="IsExplicit(StandardPlural, StandardPlural)"/>.)
        /// </summary>
        /// <param name="start">plural category for the start of the range</param>
        /// <param name="end">plural category for the end of the range</param>
        /// <returns>the resulting plural category, or 'end' if there is no data.</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public StandardPlural Get(StandardPlural start, StandardPlural end)
        {
            StandardPlural? result = matrix.Get(start, end);
            return result == null ? end : result.Value;
        }

        /// <summary>
        /// Returns whether the appropriate plural category for a range from <paramref name="start"/> to <paramref name="end"/>
        /// is explicitly in the data (vs given an implicit value). See also <see cref="Get(StandardPlural, StandardPlural)"/>.
        /// </summary>
        /// <param name="start">plural category for the start of the range</param>
        /// <param name="end">plural category for the end of the range</param>
        /// <returns>Whether the value for (start,end) is explicit or not.</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public bool IsExplicit(StandardPlural start, StandardPlural end)
        {
            return matrix.Get(start, end) != null;
        }

        /// <summary>
        /// Internal method to determines whether the StandardPluralCategories was explicitly used in any add statement.
        /// </summary>
        /// <param name="count">plural category to test</param>
        /// <returns>true if set</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public bool IsExplicitlySet(StandardPlural count)
        {
            return @explicit[(int)count];
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#pragma warning disable 809
        public override bool Equals(object other)
#pragma warning restore 809
        {
            if (this == other)
            {
                return true;
            }
            if (!(other is PluralRanges))
            {
                return false;
            }
            PluralRanges otherPR = (PluralRanges)other;
            return matrix.Equals(otherPR.matrix) && Array.Equals(@explicit, otherPR.@explicit);
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#pragma warning disable 809
        public override int GetHashCode()
#pragma warning restore 809
        {
            return matrix.GetHashCode();
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public int CompareTo(PluralRanges that)
        {
            return matrix.CompareTo(that.matrix);
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public bool IsFrozen
        {
            get { return isFrozen; }
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public PluralRanges Freeze()
        {
            isFrozen = true;
            return this;
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public PluralRanges CloneAsThawed()
        {
            PluralRanges result = new PluralRanges();
            result.@explicit = (bool[])@explicit.Clone();
            result.matrix = matrix.Clone();
            return result;
        }

        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
#pragma warning disable 809
        public override string ToString()
#pragma warning restore 809
        {
            return matrix.ToString();
        }
    }
}
