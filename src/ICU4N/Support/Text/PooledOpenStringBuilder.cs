using System;
using System.Buffers;
using System.Text;
#nullable enable

namespace ICU4N.Text
{
    /// <summary>
    /// Extends <see cref="OpenStringBuilder"/> to reuse memory from <see cref="ArrayPool{T}.Shared"/>.
    /// This implementation is disposable and will cause performance issues if <see cref="Dispose()"/>
    /// is not called.
    /// </summary>
    internal sealed class PooledOpenStringBuilder : OpenStringBuilder, IDisposable
    {
        private static ArrayPool<char> arrayPool = ArrayPool<char>.Shared;
        private char[] arrayToReturnToPool;

        public PooledOpenStringBuilder()
            : this(arrayPool.Rent(DefaultCapacity), initialLength: 0)
        { }

        public PooledOpenStringBuilder(int capacity)
            : this(arrayPool.Rent(capacity), initialLength: 0)
        { }

        public PooledOpenStringBuilder(string? value) : this(value.AsSpan()) { }
        public PooledOpenStringBuilder(ReadOnlySpan<char> value)
            : this(arrayPool.Rent(RoundUpToPowerOf2(value.Length)), initialLength: value.Length)
        {
            value.CopyTo(arrayToReturnToPool);
        }

        public PooledOpenStringBuilder(StringBuilder value)
            : this(arrayPool.Rent(RoundUpToPowerOf2(value.Length)), initialLength: value.Length)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            value.CopyTo(0, arrayToReturnToPool, 0, value.Length);
        }

        private PooledOpenStringBuilder(char[] arrayToReturnToPool, int initialLength)
            : base(arrayToReturnToPool, initialLength)
        {
            this.arrayToReturnToPool = arrayToReturnToPool; // Null check in base class
        }

        protected override char[] ReplaceBuffer(ReadOnlySpan<char> value, int newCapacity)
        {
            // Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative.
            // This could also go negative if the actual required length wraps around.
            char[] temp = arrayPool.Rent(newCapacity);
            value.CopyTo(temp);
            arrayPool.Return(arrayToReturnToPool);
            arrayToReturnToPool = temp;
            return temp;
        }

        public void Dispose() => arrayPool.Return(arrayToReturnToPool);
    }
}
