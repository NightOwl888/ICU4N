using ICU4N.Support.Text;
using ICU4N.Util;
using System.Text;

namespace ICU4N.Text
{
    internal class CharsDictionaryMatcher : DictionaryMatcher
    {
        private ICharSequence characters;

        public CharsDictionaryMatcher(string chars)
            : this(chars.ToCharSequence())
        {
        }

        public CharsDictionaryMatcher(StringBuilder chars)
            : this(chars.ToCharSequence())
        {
        }

        public CharsDictionaryMatcher(char[] chars)
            : this(chars.ToCharSequence())
        {
        }

        public CharsDictionaryMatcher(ICharSequence chars)
        {
            characters = chars;
        }

        public override int Matches(CharacterIterator text_, int maxLength, int[] lengths, int[] count_, int limit, int[] values)
        {
            UCharacterIterator text = UCharacterIterator.GetInstance(text_);
            CharsTrie uct = new CharsTrie(characters, 0);
            int c = text.MoveNextCodePoint();
            if (c == UCharacterIterator.Done)
            {
                return 0;
            }
            Result result = uct.FirstForCodePoint(c);
            // TODO: should numChars count Character.charCount?
            int numChars = 1;
            int count = 0;
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
                c = text.MoveNextCodePoint();
                if (c == UCharacterIterator.Done)
                {
                    break;
                }
                ++numChars;
                result = uct.NextForCodePoint(c);
            }
            count_[0] = count;
            return numChars;
        }

        public override int Type
        {
            get { return DictionaryData.TRIE_TYPE_UCHARS; }
        }
    }
}
