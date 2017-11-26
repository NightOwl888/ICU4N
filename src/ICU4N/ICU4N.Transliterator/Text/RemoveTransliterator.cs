namespace ICU4N.Text
{
    /// <summary>
    /// A transliterator that removes characters.  This is useful in conjunction
    /// with a filter.
    /// </summary>
    internal class RemoveTransliterator : Transliterator
    {
        /// <summary>
        /// ID for this transliterator.
        /// </summary>
        private static readonly string _ID = "Any-Remove";

        /// <summary>
        /// System registration hook.
        /// </summary>
        internal static void Register()
        {
            Transliterator.RegisterFactory(_ID, new Transliterator.Factory(getInstance: (id) =>
            {
                return new RemoveTransliterator();
            }));
            Transliterator.RegisterSpecialInverse("Remove", "Null", false);
        }

        /// <summary>
        /// Constructs a transliterator.
        /// </summary>
        public RemoveTransliterator()
                : base(_ID, null)
        {
        }

        /// <summary>
        /// Implements <see cref="Transliterator.HandleTransliterate(IReplaceable, Position, bool)"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="index"></param>
        /// <param name="incremental"></param>
        protected override void HandleTransliterate(IReplaceable text,
                                           Position index, bool incremental)
        {
            // Our caller (filteredTransliterate) has already narrowed us
            // to an unfiltered run.  Delete it.
            text.Replace(index.Start, index.Limit, "");
            int len = index.Limit - index.Start;
            index.ContextLimit -= len;
            index.Limit -= len;
        }

        /// <seealso cref="Transliterator.AddSourceTargetSet(UnicodeSet, UnicodeSet, UnicodeSet)"/>
        public override void AddSourceTargetSet(UnicodeSet inputFilter, UnicodeSet sourceSet, UnicodeSet targetSet)
        {
            // intersect myFilter with the input filter
            UnicodeSet myFilter = GetFilterAsUnicodeSet(inputFilter);
            sourceSet.AddAll(myFilter);
            // do nothing with the target
        }
    }
}
