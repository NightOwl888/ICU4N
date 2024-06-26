using ICU4N.Support.Text;
using ICU4N.Util;
using J2N.Text;
using System;
using System.Text;
#nullable enable

namespace ICU4N.Text
{
    internal class CharsDictionaryMatcher : DictionaryMatcher
    {
        private readonly ReadOnlyMemory<char> characters;
        private readonly object? reference; // ICU4N: Keeps the string or char[] behind characters alive for the lifetime of this class

        public CharsDictionaryMatcher(string chars)
            : this(chars.AsMemory())
        {
        }

        public CharsDictionaryMatcher(ReadOnlyMemory<char> chars)
        {
            characters = chars;
            chars.TryGetReference(ref reference);
        }

        // ICU4N: Changed count parameter from int[] to out int
        public override int Matches(CharacterIterator text_, int maxLength, int[] lengths, out int count, int limit, int[] values)
        {
            count = 0;
            UCharacterIterator text = UCharacterIterator.GetInstance(text_);
            CharsTrie uct = new CharsTrie(characters, 0);
            int c = text.NextCodePoint();
            if (c == UCharacterIterator.Done)
            {
                return 0;
            }
            Result result = uct.FirstForCodePoint(c);
            // TODO: should numChars count Character.charCount?
            int numChars = 1;
            for (; ; )
            {
                if (result.HasValue())
                {
                    if (count < limit)
                    {
                        if (values != null)
                        {
                            values[count] = uct.GetValue();
                        }
                        lengths[count] = numChars;
                        count++;
                    }

                    if (result == Result.FinalValue)
                    {
                        break;
                    }
                }
                else if (result == Result.NoMatch)
                {
                    break;
                }

                if (numChars >= maxLength)
                {
                    break;
                }
                c = text.NextCodePoint();
                if (c == UCharacterIterator.Done)
                {
                    break;
                }
                ++numChars;
                result = uct.NextForCodePoint(c);
            }
            return numChars;
        }

        public override int Type => DictionaryData.TRIE_TYPE_UCHARS;
    }
}
