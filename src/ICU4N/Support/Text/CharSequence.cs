using System;

namespace ICU4N.Text
{
    /// <summary>
    /// A lightweight struct that holds onto the reference of <see cref="string"/>
    /// and <see cref="T:char[]"/> as well as provides a <see cref="ReadOnlyMemory{Span}"/>
    /// for use as a common API.
    /// <para/>
    /// This struct is suitable for adding <see cref="ReadOnlyMemory{T}"/> to hashtables
    /// in mangaged code without allowing the underlying memory to go out of scope. When
    /// using <see cref="CharSequence"/> with unmanaged code, the user is responsible
    /// for ensuring that the underlying memory that is assigned to <see cref="ReadOnlyMemory{T}"/>
    /// doesn't go out of scope.
    /// </summary>
    internal struct CharSequence : IEquatable<CharSequence>
    {
        private ReadOnlyMemory<char> memory;
        private object reference;

        public CharSequence(ReadOnlyMemory<char> sequence)
        {
            this.memory = sequence;
            sequence.TryGetReference(ref reference);
        }

        public CharSequence(string sequence)
        {
            reference = sequence ?? throw new ArgumentNullException(nameof(sequence));
            this.memory = sequence.AsMemory();
        }

        public CharSequence(char[] sequence)
        {
            reference = sequence ?? throw new ArgumentNullException(nameof(sequence));
            this.memory = sequence.AsMemory();
        }

        public ReadOnlySpan<char> AsSpan()
            => memory.Span;

        public ReadOnlySpan<char> AsSpan(int start)
            => memory.Span.Slice(start);

        public ReadOnlySpan<char> AsSpan(int start, int length)
            => memory.Span.Slice(start, length);


        public ReadOnlyMemory<char> AsMemory()
            => memory;

        public ReadOnlyMemory<char> AsMemory(int start)
            => memory.Slice(start);

        public ReadOnlyMemory<char> AsMemory(int start, int length)
            => memory.Slice(start, length);

        public bool Equals(CharSequence other)
        {
            return memory.Span.Equals(other.memory.Span, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (obj is CharSequence sequence) return Equals(sequence);
            return false;
        }

        public override int GetHashCode()
        {
            return StringHelper.GetHashCode(memory.Span);
        }

        public static implicit operator ReadOnlySpan<char>(CharSequence sequence)
        {
            return sequence.memory.Span;
        }

        public static implicit operator ReadOnlyMemory<char>(CharSequence sequence)
        {
            return sequence.memory;
        }

        public static implicit operator CharSequence(ReadOnlyMemory<char> sequence)
        {
            return new CharSequence(sequence);
        }

        public static implicit operator CharSequence(string sequence)
        {
            return new CharSequence(sequence);
        }

        public static implicit operator CharSequence(char[] sequence)
        {
            return new CharSequence(sequence);
        }
    }
}
