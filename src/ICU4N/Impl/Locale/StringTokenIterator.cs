using ICU4N.Text;
using System;

namespace ICU4N.Impl.Locale
{
    /// <summary>
    /// NOTE: This is similar to StringTokenIterator in ICU4J, but provides more functionality including
    /// the ability to trim specific characters from the start or end of each token during enumeration.
    /// </summary>
    public ref struct StringTokenEnumerator
    {
        private ReadOnlySpan<char> text;
        private int start; // The index into text for the next call to MoveNext(). Note that the original text is not preserved.
        private int length; // The length of the original text that was passed into the constructor.
        private bool hasNext; // Whether or not there are additional tokens (including when the last one is empty).
        private bool isDone; // Whether or not the last call to MoveNext() returned false.
#pragma warning disable IDE0044, S2933 // Add readonly modifier
        private ReadOnlySpan<char> delimiters;
        private char delimiter;
        private bool isSingleCharDelimiter;
        private TrimBehavior trimBehavior;
        private ReadOnlySpan<char> trimChars;
#pragma warning restore IDE0044, S2933 // Add readonly modifier

        /// <summary>
        /// Creates an enumerator that tokenizes <paramref name="text"/>, splitting on the <paramref name="delimiter"/> and
        /// then trimming each segment using <paramref name="trimChars"/>. This implements the Enumerator contract in C# so it can
        /// be used inside of a foreach statement.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <param name="delimiter">The character to split on.</param>
        /// <param name="trimChars">The characters to trim from the beginning and end of each token.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        public StringTokenEnumerator(ReadOnlySpan<char> text, char delimiter, ReadOnlySpan<char> trimChars = default, TrimBehavior trimBehavior = TrimBehavior.StartAndEnd)
        {
            this.text = text;
            this.start = 0;
            this.length = text.Length;
            this.hasNext = !text.IsEmpty;
            this.isDone = false;
            this.delimiters = default;
            this.delimiter = delimiter;
            this.isSingleCharDelimiter = true;
            this.trimBehavior = trimBehavior;
            this.trimChars = trimChars;
            Current = default;
        }

        /// <summary>
        /// Creates an enumerator that tokenizes <paramref name="text"/>, splitting on any of the <paramref name="delimiters"/> and
        /// then trimming each segment using <paramref name="trimChars"/>. This implements the Enumerator contract in C# so it can
        /// be used inside of a foreach statement.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <param name="delimiters">The character(s) to split on.</param>
        /// <param name="trimChars">The characters to trim from the beginning and end of each token.</param>
        /// <param name="trimBehavior">Bitwise flags to determine whether to trim the beginning of the string, the end of the string, or both.</param>
        public StringTokenEnumerator(ReadOnlySpan<char> text, ReadOnlySpan<char> delimiters, ReadOnlySpan<char> trimChars = default, TrimBehavior trimBehavior = TrimBehavior.StartAndEnd)
        {
            this.text = text;
            this.start = 0;
            this.length = text.Length;
            this.hasNext = !text.IsEmpty;
            this.isDone = false;
            this.delimiters = delimiters;
            this.delimiter = default;
            this.isSingleCharDelimiter = false;
            this.trimBehavior = trimBehavior;
            this.trimChars = trimChars;
            Current = default;
        }

        // Needed to be compatible with the foreach operator
        public StringTokenEnumerator GetEnumerator() => this;

        /// <summary>
        /// Gets a value of the return value of the next call to <see cref="MoveNext()"/>
        /// without actually moving the cursor.
        /// </summary>
        public bool HasNext => hasNext;

        /// <summary>
        /// Gets a value indicating whether the last call to <see cref="MoveNext()"/>
        /// occurred when there were no more tokens. This can be used to determine
        /// whether the entire string was consumed by a parser versus there being
        /// an empty token at the end of the string that was not consumed.
        /// </summary>
        public bool IsDone => isDone;

        public bool MoveNext()
        {
            var span = text;
            if (!hasNext) // Reached the end of the string
            {
                Current = new TokenEntry(default, default, length); // Clear current
                isDone = true;
                return false;
            }

            var index = isSingleCharDelimiter ? span.IndexOf(delimiter) : span.IndexOfAny(delimiters);
            if (index == -1) // The string is composed of only 1 segment
            {
                text = ReadOnlySpan<char>.Empty; // The remaining string is an empty string
                Current = new TokenEntry(Trim(span), default, start);
                hasNext = false;
                return true;
            }

            // We do the split and then trim off the unwanted characters before returning the entry.
            Current = new TokenEntry(Trim(span.Slice(0, index)), span.Slice(index, 1), start);
            text = span.Slice(index + 1); // Skip the matched char.
            start += index + 1;
            return true;
        }

        /// <summary>
        /// Trims the string as specified in the constructor.
        /// </summary>
        private ReadOnlySpan<char> Trim(ReadOnlySpan<char> text)
        {
            if (trimChars.Length == 0)
                return text;

            switch (trimBehavior)
            {
                case TrimBehavior.StartAndEnd:
                    return text.Trim(trimChars);
                case TrimBehavior.Start:
                    return text.TrimStart(trimChars);
                case TrimBehavior.End:
                    return text.TrimEnd(trimChars);
                default:
                    return text.Trim(trimChars);
            }
        }

        public TokenEntry Current { get; private set; }
    }

    /// <summary>
    /// A struct that represents the text between a split character and the text of the matched split character.
    /// <para/>
    /// This is used for an implementation of an allocationless split functionality. See: https://www.meziantou.net/split-a-string-into-lines-without-allocation.htm.
    /// </summary>
    public readonly ref struct TokenEntry
    {
        public TokenEntry(ReadOnlySpan<char> text, ReadOnlySpan<char> delimiter, int startIndex)
        {
            Text = text;
            Delimiter = delimiter;
            StartIndex = startIndex;
        }

        /// <summary>
        /// Gets the index into the original text where the slice <see cref="Text"/> starts.
        /// </summary>
        public int StartIndex { get; }
        public ReadOnlySpan<char> Text { get; }
        public ReadOnlySpan<char> Delimiter { get; }

        // This method allow to deconstruct the type, so you can write any of the following code
        // foreach (var entry in str.AsTokens(",|-")) { _ = entry.Text; }
        // foreach (var (text, delimiter, startIndex) in str.AsTokens(",|-")) { _ = text; }
        // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct?WT.mc_id=DT-MVP-5003978#deconstructing-user-defined-types
        public void Deconstruct(out ReadOnlySpan<char> text, out ReadOnlySpan<char> delimiter, out int startIndex)
        {
            text = Text;
            delimiter = Delimiter;
            startIndex = StartIndex;
        }

        // This method allow to implicitly cast the type into a ReadOnlySpan<char>, so you can write the following code
        // foreach (ReadOnlySpan<char> entry in str.SplitLines())
        public static implicit operator ReadOnlySpan<char>(TokenEntry entry) => entry.Text;
    }
}
