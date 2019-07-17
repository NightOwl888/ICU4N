namespace ICU4N.Text
{
    /// <summary>
    /// A transliterator that leaves text unchanged.
    /// </summary>
    internal class NullTransliterator : Transliterator
    {
        /// <summary>
        /// Package accessible IDs for this transliterator.
        /// </summary>
        internal static readonly string SHORT_ID = "Null";
        internal static readonly string _ID = "Any-Null";

        /// <summary>
        /// Constructs a transliterator.
        /// </summary>
        public NullTransliterator()
                : base(_ID, null)
        {
        }

        /// <summary>
        /// Implements <see cref="Transliterator.HandleTransliterate(IReplaceable, TransliterationPosition, bool)"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="offsets"></param>
        /// <param name="incremental"></param>
        protected override void HandleTransliterate(IReplaceable text,
                                           TransliterationPosition offsets, bool incremental)
        {
            offsets.Start = offsets.Limit;
        }

        /// <seealso cref="Transliterator.AddSourceTargetSet(UnicodeSet, UnicodeSet, UnicodeSet)"/>
        public override void AddSourceTargetSet(UnicodeSet inputFilter, UnicodeSet sourceSet, UnicodeSet targetSet)
        {
            // do nothing
        }
    }
}
