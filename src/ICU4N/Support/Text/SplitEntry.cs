using System;

#if FEATURE_SPAN
namespace ICU4N.Text
{
    /// <summary>
    /// A struct that represents the text between a split character and the text of the matched split character.
    /// <para/>
    /// This is used for an implementation of a allocationless split functionality. See: https://www.meziantou.net/split-a-string-into-lines-without-allocation.htm.
    /// </summary>
    internal readonly ref struct SplitEntry
    {
        public SplitEntry(ReadOnlySpan<char> text, ReadOnlySpan<char> delimiter)
        {
            Text = text;
            Delimiter = delimiter;
        }

        public ReadOnlySpan<char> Text { get; }
        public ReadOnlySpan<char> Delimiter { get; }

        // This method allow to deconstruct the type, so you can write any of the following code
        // foreach (var entry in str.SplitLines()) { _ = entry.Line; }
        // foreach (var (line, endOfLine) in str.SplitLines()) { _ = line; }
        // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct?WT.mc_id=DT-MVP-5003978#deconstructing-user-defined-types
        public void Deconstruct(out ReadOnlySpan<char> text, out ReadOnlySpan<char> delimiter)
        {
            text = Text;
            delimiter = Delimiter;
        }

        // This method allow to implicitly cast the type into a ReadOnlySpan<char>, so you can write the following code
        // foreach (ReadOnlySpan<char> entry in str.SplitLines())
        public static implicit operator ReadOnlySpan<char>(SplitEntry entry) => entry.Text;
    }
}
#endif