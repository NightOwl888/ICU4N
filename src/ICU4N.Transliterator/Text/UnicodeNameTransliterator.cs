using ICU4N.Globalization;
using System.Text;

namespace ICU4N.Text
{
    /// <summary>
    /// A transliterator that performs character to name mapping.
    /// It generates the Perl syntax \N{name}.
    /// </summary>
    /// <author>Alan Liu</author>
    internal class UnicodeNameTransliterator : Transliterator
    {
        internal const string _ID = "Any-Name";

        internal const string OPEN_DELIM = "\\N{";
        internal const char CLOSE_DELIM = '}';
        internal const int OPEN_DELIM_LEN = 3;

        /// <summary>
        /// System registration hook.
        /// </summary>
        internal static void Register()
        {
            Transliterator.RegisterFactory(_ID, new Transliterator.Factory(getInstance: (id) =>
            {
                return new UnicodeNameTransliterator(null);
            }));
        }

        /// <summary>
        /// Constructs a transliterator.
        /// </summary>
        /// <param name="filter"></param>
        public UnicodeNameTransliterator(UnicodeFilter filter)
                : base(_ID, filter)
        {
        }

        /// <summary>
        /// Implements <see cref="Transliterator.HandleTransliterate(IReplaceable, TransliterationPosition, bool)"/>.
        /// </summary>
        protected override void HandleTransliterate(IReplaceable text,
                                           TransliterationPosition offsets, bool isIncremental)
        {
            int cursor = offsets.Start;
            int limit = offsets.Limit;

            StringBuilder str = new StringBuilder();
            str.Append(OPEN_DELIM);
            int len;
            string name;

            while (cursor < limit)
            {
                int c = text.Char32At(cursor);
                if ((name = UChar.GetExtendedName(c)) != null)
                {

                    str.Length = OPEN_DELIM_LEN;
                    str.Append(name).Append(CLOSE_DELIM);

                    int clen = UTF16.GetCharCount(c);
                    text.Replace(cursor, cursor + clen, str.ToString());
                    len = str.Length;
                    cursor += len; // advance cursor by 1 and adjust for new text
                    limit += len - clen; // change in length
                }
                else
                {
                    ++cursor;
                }
            }

            offsets.ContextLimit += limit - offsets.Limit;
            offsets.Limit = limit;
            offsets.Start = cursor;
        }

        /// <seealso cref="Transliterator.AddSourceTargetSet(UnicodeSet, UnicodeSet, UnicodeSet)"/>
#pragma warning disable 672
        public override void AddSourceTargetSet(UnicodeSet inputFilter, UnicodeSet sourceSet, UnicodeSet targetSet)
#pragma warning restore 672
        {
#pragma warning disable 612, 618
            UnicodeSet myFilter = GetFilterAsUnicodeSet(inputFilter);
#pragma warning restore 612, 618
            if (myFilter.Count > 0)
            {
                sourceSet.AddAll(myFilter);
                targetSet.AddAll('0', '9')
                .AddAll('A', 'Z')
                .Add('-')
                .Add(' ')
                .AddAll(OPEN_DELIM)
                .Add(CLOSE_DELIM)
                .AddAll('a', 'z') // for controls
                .Add('<').Add('>') // for controls
                .Add('(').Add(')') // for controls
                ;
            }
        }
    }
}
