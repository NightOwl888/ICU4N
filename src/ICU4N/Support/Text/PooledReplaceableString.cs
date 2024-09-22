using System;
using System.Buffers;
using System.Text;
#nullable enable

namespace ICU4N.Text
{
    /// <summary>
    /// Extends <see cref="ReplaceableString"/> to reuse memory from <see cref="ArrayPool{T}.Shared"/>.
    /// This implementation is disposable and will cause performance issues if <see cref="Dispose()"/>
    /// is not called.
    /// </summary>
    internal sealed class PooledReplaceableString : ReplaceableString, IDisposable
    {
        private readonly PooledOpenStringBuilder buffer;

        /// <summary>
        /// Construct a new object with the given initial contents.
        /// </summary>
        /// <param name="str">Initial contents.</param>
        /// <stable>ICU 60.1</stable>
        public PooledReplaceableString(string? str)
            : this(new PooledOpenStringBuilder(str))
        {
        }

        /// <summary>
        /// Construct a new object with the given initial contents.
        /// </summary>
        /// <param name="str">Initial contents.</param>
        /// <stable>ICU 60.1</stable>
        public PooledReplaceableString(ReadOnlySpan<char> str)
            : this(new PooledOpenStringBuilder(str))
        {
        }

        /// <summary>
        /// Construct a new object using <paramref name="buf"/> for internal
        /// storage.  The contents of <paramref name="buf"/> at the time of
        /// construction are used as the initial contents.
        /// </summary>
        /// <param name="buf">Object to be used as internal storage.</param>
        /// <stable>ICU 60.1</stable>
        public PooledReplaceableString(StringBuilder buf)
            : this(new PooledOpenStringBuilder(buf))
        {
        }

        /// <summary>
        /// Construct a new empty object.
        /// </summary>
        /// <stable>ICU 60.1</stable>
        public PooledReplaceableString()
            : this(new PooledOpenStringBuilder())
        {
        }

        private PooledReplaceableString(PooledOpenStringBuilder buffer)
            : base(buffer)
        {
            this.buffer = buffer;
        }

        public void Dispose()
        {
            buffer.Dispose();
        }
    }
}
