using ICU4N.Util;
using System;
using System.Text;

namespace ICU4N.Impl
{
    /// <summary>
    /// Convenience Methods
    /// </summary>
    public static class Row
    {
        public static Row<TColumn0, TColumn1> Of<TColumn0, TColumn1>(TColumn0 p0, TColumn1 p1)
        {
            return new Row<TColumn0, TColumn1>(p0, p1);
        }
        public static Row<TColumn0, TColumn1, TColumn2> Of<TColumn0, TColumn1, TColumn2>(TColumn0 p0, TColumn1 p1, TColumn2 p2)
        {
            return new Row<TColumn0, TColumn1, TColumn2>(p0, p1, p2);
        }
        public static Row<TColumn0, TColumn1, TColumn2, TColumn3> Of<TColumn0, TColumn1, TColumn2, TColumn3>(TColumn0 p0, TColumn1 p1, TColumn2 p2, TColumn3 p3)
        {
            return new Row<TColumn0, TColumn1, TColumn2, TColumn3>(p0, p1, p2, p3);
        }
        public static Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4> Of<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4>(TColumn0 p0, TColumn1 p1, TColumn2 p2, TColumn3 p3, TColumn4 p4)
        {
            return new Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4>(p0, p1, p2, p3, p4);
        }
    }

    /// <author>Mark Davis</author>
    public class Row<TColumn0, TColumn1> : Row<TColumn0, TColumn1, TColumn1, TColumn1, TColumn1>
    {
        public Row(TColumn0 a, TColumn1 b)
        {
            items = new object[] { a, b };
        }
    }

    /// <author>Mark Davis</author>
    public class Row<TColumn0, TColumn1, TColumn2> : Row<TColumn0, TColumn1, TColumn2, TColumn2, TColumn2>
    {
        public Row(TColumn0 a, TColumn1 b, TColumn2 c)
        {
            items = new object[] { a, b, c };
        }
    }

    /// <author>Mark Davis</author>
    public class Row<TColumn0, TColumn1, TColumn2, TColumn3> : Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn3>
    {
        public Row(TColumn0 a, TColumn1 b, TColumn2 c, TColumn3 d)
        {
            items = new object[] { a, b, c, d };
        }
    }

    /// <author>Mark Davis</author>
    public class Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4> : IComparable, IFreezable<Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4>>
#if FEATURE_CLONEABLE
        , ICloneable
#endif
    {
        protected object[] items;
        protected bool frozen;

        public Row(TColumn0 a, TColumn1 b, TColumn2 c, TColumn3 d, TColumn4 e)
        {
            items = new object[] { a, b, c, d, e };
        }

        protected Row()
        {
        }

        public virtual Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4> Set0(TColumn0 item)
        {
            return Set(0, item);
        }
        public virtual TColumn0 Get0()
        {
            return (TColumn0)items[0];
        }
        public virtual Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4> Set1(TColumn1 item)
        {
            return Set(1, item);
        }
        public virtual TColumn1 Get1()
        {
            return (TColumn1)items[1];
        }
        public virtual Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4> Set2(TColumn2 item)
        {
            return Set(2, item);
        }
        public virtual TColumn2 Get2()
        {
            return (TColumn2)items[2];
        }
        public virtual Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4> Set3(TColumn3 item)
        {
            return Set(3, item);
        }
        public virtual TColumn3 Get3()
        {
            return (TColumn3)items[3];
        }
        public virtual Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4> Set4(TColumn4 item)
        {
            return Set(4, item);
        }
        public virtual TColumn4 Get4()
        {
            return (TColumn4)items[4];
        }

        protected Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4> Set(int i, object item)
        {
            if (frozen)
            {
                throw new NotSupportedException("Attempt to modify frozen object");
            }
            items[i] = item;
            return this;
        }

        public override int GetHashCode()
        {
            int sum = items.Length;
            foreach (object item in items)
            {
                sum = sum * 37 + Utility.CheckHashCode(item);
            }
            return sum;
        }

        public override bool Equals(object other)
        {
            if (other == null)
            {
                return false;
            }
            if (this == other)
            {
                return true;
            }
            try
            {
                Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4> that = (Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4>)other;
                if (items.Length != that.items.Length)
                {
                    return false;
                }
                int i = 0;
                foreach (object item in items)
                {
                    if (!Utility.ObjectEquals(item, that.items[i++]))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public virtual int CompareTo(object other)
        {
            int result;
            Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4> that = (Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4>)other;
            result = items.Length - that.items.Length;
            if (result != 0)
            {
                return result;
            }
            int i = 0;
            foreach (object item in items)
            {
                result = Utility.CheckCompare(((IComparable)item), ((IComparable)that.items[i++]));
                if (result != 0)
                {
                    return result;
                }
            }
            return 0;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder("[");
            bool first = true;
            foreach (object item in items)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    result.Append(", ");
                }
                result.Append(item);
            }
            return result.Append("]").ToString();
        }

        public virtual bool IsFrozen
        {
            get { return frozen; }
        }

        public virtual Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4> Freeze()
        {
            frozen = true;
            return this;
        }

        public virtual object Clone()
        {
            if (frozen) return this;
            Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4> result = (Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4>)base.MemberwiseClone();
            items = (object[])items.Clone();
            return result;
        }

        public virtual Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4> CloneAsThawed()
        {
            Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4> result = (Row<TColumn0, TColumn1, TColumn2, TColumn3, TColumn4>)base.MemberwiseClone();
            items = (object[])items.Clone();
            result.frozen = false;
            return result;
        }
    }
}
