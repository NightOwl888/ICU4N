using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICU4N
{
    /// <summary>
    /// A ref struct that can be used to implement a structure similar to a params array
    /// with up to 16 arguments using <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of array.</typeparam>
    internal ref struct ReadOnlySpanArray<T>
    {
        private int length;
        private ReadOnlySpan<T> value0;
        private ReadOnlySpan<T> value1;
        private ReadOnlySpan<T> value2;
        private ReadOnlySpan<T> value3;
        private ReadOnlySpan<T> value4;
        private ReadOnlySpan<T> value5;
        private ReadOnlySpan<T> value6;
        private ReadOnlySpan<T> value7;
        private ReadOnlySpan<T> value8;
        private ReadOnlySpan<T> value9;
        private ReadOnlySpan<T> value10;
        private ReadOnlySpan<T> value11;
        private ReadOnlySpan<T> value12;
        private ReadOnlySpan<T> value13;
        private ReadOnlySpan<T> value14;
        private ReadOnlySpan<T> value15;

        public ReadOnlySpanArray(ReadOnlySpan<T> value0)
        {
            this.length = 1;
            this.value0 = value0;
            this.value1 = default;
            this.value2 = default;
            this.value3 = default;
            this.value4 = default;
            this.value5 = default;
            this.value6 = default;
            this.value7 = default;
            this.value8 = default;
            this.value9 = default;
            this.value10 = default;
            this.value11 = default;
            this.value12 = default;
            this.value13 = default;
            this.value14 = default;
            this.value15 = default;
        }

        public ReadOnlySpanArray(ReadOnlySpan<T> value0, ReadOnlySpan<T> value1)
        {
            this.length = 2;
            this.value0 = value0;
            this.value1 = value1;
            this.value2 = default;
            this.value3 = default;
            this.value4 = default;
            this.value5 = default;
            this.value6 = default;
            this.value7 = default;
            this.value8 = default;
            this.value9 = default;
            this.value10 = default;
            this.value11 = default;
            this.value12 = default;
            this.value13 = default;
            this.value14 = default;
            this.value15 = default;
        }

        public ReadOnlySpanArray(ReadOnlySpan<T> value0, ReadOnlySpan<T> value1, ReadOnlySpan<T> value2)
        {
            this.length = 3;
            this.value0 = value0;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = default;
            this.value4 = default;
            this.value5 = default;
            this.value6 = default;
            this.value7 = default;
            this.value8 = default;
            this.value9 = default;
            this.value10 = default;
            this.value11 = default;
            this.value12 = default;
            this.value13 = default;
            this.value14 = default;
            this.value15 = default;
        }

        public ReadOnlySpanArray(ReadOnlySpan<T> value0, ReadOnlySpan<T> value1, ReadOnlySpan<T> value2, ReadOnlySpan<T> value3)
        {
            this.length = 4;
            this.value0 = value0;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
            this.value4 = default;
            this.value5 = default;
            this.value6 = default;
            this.value7 = default;
            this.value8 = default;
            this.value9 = default;
            this.value10 = default;
            this.value11 = default;
            this.value12 = default;
            this.value13 = default;
            this.value14 = default;
            this.value15 = default;
        }

        public ReadOnlySpanArray(ReadOnlySpan<T> value0, ReadOnlySpan<T> value1, ReadOnlySpan<T> value2, ReadOnlySpan<T> value3, ReadOnlySpan<T> value4)
        {
            this.length = 5;
            this.value0 = value0;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
            this.value4 = value4;
            this.value5 = default;
            this.value6 = default;
            this.value7 = default;
            this.value8 = default;
            this.value9 = default;
            this.value10 = default;
            this.value11 = default;
            this.value12 = default;
            this.value13 = default;
            this.value14 = default;
            this.value15 = default;
        }

        public ReadOnlySpanArray(ReadOnlySpan<T> value0, ReadOnlySpan<T> value1, ReadOnlySpan<T> value2, ReadOnlySpan<T> value3, ReadOnlySpan<T> value4, ReadOnlySpan<T> value5)
        {
            this.length = 6;
            this.value0 = value0;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
            this.value4 = value4;
            this.value5 = value5;
            this.value6 = default;
            this.value7 = default;
            this.value8 = default;
            this.value9 = default;
            this.value10 = default;
            this.value11 = default;
            this.value12 = default;
            this.value13 = default;
            this.value14 = default;
            this.value15 = default;
        }

        public ReadOnlySpanArray(ReadOnlySpan<T> value0, ReadOnlySpan<T> value1, ReadOnlySpan<T> value2, ReadOnlySpan<T> value3, ReadOnlySpan<T> value4, ReadOnlySpan<T> value5, ReadOnlySpan<T> value6)
        {
            this.length = 7;
            this.value0 = value0;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
            this.value4 = value4;
            this.value5 = value5;
            this.value6 = value6;
            this.value7 = default;
            this.value8 = default;
            this.value9 = default;
            this.value10 = default;
            this.value11 = default;
            this.value12 = default;
            this.value13 = default;
            this.value14 = default;
            this.value15 = default;
        }

        public ReadOnlySpanArray(ReadOnlySpan<T> value0, ReadOnlySpan<T> value1, ReadOnlySpan<T> value2, ReadOnlySpan<T> value3, ReadOnlySpan<T> value4, ReadOnlySpan<T> value5, ReadOnlySpan<T> value6, ReadOnlySpan<T> value7)
        {
            this.length = 8;
            this.value0 = value0;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
            this.value4 = value4;
            this.value5 = value5;
            this.value6 = value6;
            this.value7 = value7;
            this.value8 = default;
            this.value9 = default;
            this.value10 = default;
            this.value11 = default;
            this.value12 = default;
            this.value13 = default;
            this.value14 = default;
            this.value15 = default;
        }

        public ReadOnlySpanArray(ReadOnlySpan<T> value0, ReadOnlySpan<T> value1, ReadOnlySpan<T> value2, ReadOnlySpan<T> value3, ReadOnlySpan<T> value4, ReadOnlySpan<T> value5, ReadOnlySpan<T> value6, ReadOnlySpan<T> value7, ReadOnlySpan<T> value8)
        {
            this.length = 9;
            this.value0 = value0;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
            this.value4 = value4;
            this.value5 = value5;
            this.value6 = value6;
            this.value7 = value7;
            this.value8 = value8;
            this.value9 = default;
            this.value10 = default;
            this.value11 = default;
            this.value12 = default;
            this.value13 = default;
            this.value14 = default;
            this.value15 = default;
        }

        public ReadOnlySpanArray(ReadOnlySpan<T> value0, ReadOnlySpan<T> value1, ReadOnlySpan<T> value2, ReadOnlySpan<T> value3, ReadOnlySpan<T> value4, ReadOnlySpan<T> value5, ReadOnlySpan<T> value6, ReadOnlySpan<T> value7, ReadOnlySpan<T> value8, ReadOnlySpan<T> value9)
        {
            this.length = 10;
            this.value0 = value0;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
            this.value4 = value4;
            this.value5 = value5;
            this.value6 = value6;
            this.value7 = value7;
            this.value8 = value8;
            this.value9 = value9;
            this.value10 = default;
            this.value11 = default;
            this.value12 = default;
            this.value13 = default;
            this.value14 = default;
            this.value15 = default;
        }

        public ReadOnlySpanArray(ReadOnlySpan<T> value0, ReadOnlySpan<T> value1, ReadOnlySpan<T> value2, ReadOnlySpan<T> value3, ReadOnlySpan<T> value4, ReadOnlySpan<T> value5, ReadOnlySpan<T> value6, ReadOnlySpan<T> value7, ReadOnlySpan<T> value8, ReadOnlySpan<T> value9, ReadOnlySpan<T> value10)
        {
            this.length = 11;
            this.value0 = value0;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
            this.value4 = value4;
            this.value5 = value5;
            this.value6 = value6;
            this.value7 = value7;
            this.value8 = value8;
            this.value9 = value9;
            this.value10 = value10;
            this.value11 = default;
            this.value12 = default;
            this.value13 = default;
            this.value14 = default;
            this.value15 = default;
        }

        public ReadOnlySpanArray(ReadOnlySpan<T> value0, ReadOnlySpan<T> value1, ReadOnlySpan<T> value2, ReadOnlySpan<T> value3, ReadOnlySpan<T> value4, ReadOnlySpan<T> value5, ReadOnlySpan<T> value6, ReadOnlySpan<T> value7, ReadOnlySpan<T> value8, ReadOnlySpan<T> value9, ReadOnlySpan<T> value10, ReadOnlySpan<T> value11)
        {
            this.length = 12;
            this.value0 = value0;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
            this.value4 = value4;
            this.value5 = value5;
            this.value6 = value6;
            this.value7 = value7;
            this.value8 = value8;
            this.value9 = value9;
            this.value10 = value10;
            this.value11 = value11;
            this.value12 = default;
            this.value13 = default;
            this.value14 = default;
            this.value15 = default;
        }

        public ReadOnlySpanArray(ReadOnlySpan<T> value0, ReadOnlySpan<T> value1, ReadOnlySpan<T> value2, ReadOnlySpan<T> value3, ReadOnlySpan<T> value4, ReadOnlySpan<T> value5, ReadOnlySpan<T> value6, ReadOnlySpan<T> value7, ReadOnlySpan<T> value8, ReadOnlySpan<T> value9, ReadOnlySpan<T> value10, ReadOnlySpan<T> value11, ReadOnlySpan<T> value12)
        {
            this.length = 13;
            this.value0 = value0;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
            this.value4 = value4;
            this.value5 = value5;
            this.value6 = value6;
            this.value7 = value7;
            this.value8 = value8;
            this.value9 = value9;
            this.value10 = value10;
            this.value11 = value11;
            this.value12 = value12;
            this.value13 = default;
            this.value14 = default;
            this.value15 = default;
        }

        public ReadOnlySpanArray(ReadOnlySpan<T> value0, ReadOnlySpan<T> value1, ReadOnlySpan<T> value2, ReadOnlySpan<T> value3, ReadOnlySpan<T> value4, ReadOnlySpan<T> value5, ReadOnlySpan<T> value6, ReadOnlySpan<T> value7, ReadOnlySpan<T> value8, ReadOnlySpan<T> value9, ReadOnlySpan<T> value10, ReadOnlySpan<T> value11, ReadOnlySpan<T> value12, ReadOnlySpan<T> value13)
        {
            this.length = 14;
            this.value0 = value0;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
            this.value4 = value4;
            this.value5 = value5;
            this.value6 = value6;
            this.value7 = value7;
            this.value8 = value8;
            this.value9 = value9;
            this.value10 = value10;
            this.value11 = value11;
            this.value12 = value12;
            this.value13 = value13;
            this.value14 = default;
            this.value15 = default;
        }

        public ReadOnlySpanArray(ReadOnlySpan<T> value0, ReadOnlySpan<T> value1, ReadOnlySpan<T> value2, ReadOnlySpan<T> value3, ReadOnlySpan<T> value4, ReadOnlySpan<T> value5, ReadOnlySpan<T> value6, ReadOnlySpan<T> value7, ReadOnlySpan<T> value8, ReadOnlySpan<T> value9, ReadOnlySpan<T> value10, ReadOnlySpan<T> value11, ReadOnlySpan<T> value12, ReadOnlySpan<T> value13, ReadOnlySpan<T> value14)
        {
            this.length = 15;
            this.value0 = value0;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
            this.value4 = value4;
            this.value5 = value5;
            this.value6 = value6;
            this.value7 = value7;
            this.value8 = value8;
            this.value9 = value9;
            this.value10 = value10;
            this.value11 = value11;
            this.value12 = value12;
            this.value13 = value13;
            this.value14 = value14;
            this.value15 = default;
        }

        public ReadOnlySpanArray(ReadOnlySpan<T> value0, ReadOnlySpan<T> value1, ReadOnlySpan<T> value2, ReadOnlySpan<T> value3, ReadOnlySpan<T> value4, ReadOnlySpan<T> value5, ReadOnlySpan<T> value6, ReadOnlySpan<T> value7, ReadOnlySpan<T> value8, ReadOnlySpan<T> value9, ReadOnlySpan<T> value10, ReadOnlySpan<T> value11, ReadOnlySpan<T> value12, ReadOnlySpan<T> value13, ReadOnlySpan<T> value14, ReadOnlySpan<T> value15)
        {
            this.length = 16;
            this.value0 = value0;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
            this.value4 = value4;
            this.value5 = value5;
            this.value6 = value6;
            this.value7 = value7;
            this.value8 = value8;
            this.value9 = value9;
            this.value10 = value10;
            this.value11 = value11;
            this.value12 = value12;
            this.value13 = value13;
            this.value14 = value14;
            this.value15 = value15;
        }

        public ReadOnlySpan<T> this[int index] => index switch
        {
            0 => value0,
            1 => value1,
            2 => value2,
            3 => value3,
            4 => value4,
            5 => value5,
            6 => value6,
            7 => value7,
            8 => value8,
            9 => value9,
            10 => value10,
            11 => value11,
            12 => value12,
            13 => value13,
            14 => value14,
            15 => value15,
            _ => throw new IndexOutOfRangeException()
        };

        public int Length => length;
    }
}
