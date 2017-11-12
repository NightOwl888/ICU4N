using ICU4N.Impl;
using ICU4N.Lang;
using ICU4N.Support;
using ICU4N.Support.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Text
{
    internal sealed class UnhandledBreakEngine : ILanguageBreakEngine
    {
        // TODO: Use two arrays of UnicodeSet, one with all frozen sets, one with unfrozen.
        // in handleChar(), update the unfrozen version, clone, freeze, replace the frozen one.

        // Note on concurrency: A single instance of UnhandledBreakEngine is shared across all
        // RuleBasedBreakIterators in a process. They may make arbitrary concurrent calls.
        // If handleChar() is updating the set of unhandled characters at the same time
        // findBreaks() or handles() is referencing it, the referencing functions must see
        // a consistent set. It doesn't matter whether they see it before or after the update,
        // but they should not see an inconsistent, changing set.
        //
        // To do this, an update is made by cloning the old set, updating the clone, then
        // replacing the old with the new. Once made visible, each set remains constant.

        // TODO: it's odd that findBreaks() can produce different results, depending
        // on which scripts have been previously seen by handleChar(). (This is not a
        // threading specific issue). Possibly stop on script boundaries?

        internal readonly AtomicReferenceArray<UnicodeSet> fHandled = new AtomicReferenceArray<UnicodeSet>(BreakIterator.KIND_TITLE + 1);
        public UnhandledBreakEngine()
        {
            for (int i = 0; i < fHandled.Length; i++)
            {
                fHandled[i]= new UnicodeSet();
            }
        }

        public bool Handles(int c, int breakType)
        {
            return (breakType >= 0 && breakType < fHandled.Length) &&
                    (fHandled[breakType].Contains(c));
        }

        public int FindBreaks(CharacterIterator text, int startPos, int endPos,
                int breakType, DictionaryBreakEngine.DequeI foundBreaks)
        {
            if (breakType >= 0 && breakType < fHandled.Length)
            {
                UnicodeSet uniset = fHandled[breakType];
                int c = CharacterIteration.Current32(text);
                while (text.Index < endPos && uniset.Contains(c))
                {
                    CharacterIteration.Next32(text);
                    c = CharacterIteration.Current32(text);
                }
            }
            return 0;
        }

        /**
         * Update the set of unhandled characters for the specified breakType to include
         * all that have the same script as c.
         * May be called concurrently with handles() or findBreaks().
         * Must not be called concurrently with itself.
         */
        public void HandleChar(int c, int breakType)
        {
            if (breakType >= 0 && breakType < fHandled.Length && c != CharacterIteration.DONE32)
            {
                UnicodeSet originalSet = fHandled[breakType];
                if (!originalSet.Contains(c))
                {
                    int script = UCharacter.GetInt32PropertyValue(c, UProperty.SCRIPT);
                    UnicodeSet newSet = new UnicodeSet();
                    newSet.ApplyIntPropertyValue((int)UProperty.SCRIPT, script);
                    newSet.AddAll(originalSet);
                    fHandled[breakType]= newSet;
                }
            }
        }
    }
}
