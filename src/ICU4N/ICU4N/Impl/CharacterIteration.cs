using ICU4N.Support.Text;
using ICU4N.Text;

namespace ICU4N.Impl
{
    public static class CharacterIteration // ICU4N specific - made into static class rather than using a private constructor
    {
        /// <summary>
        /// 32 bit Char value returned from when an iterator has run out of range.
        ///     Positive value so fast case (not end, not surrogate) can be checked
        ///     with a single test.
        /// </summary>
        public static readonly int DONE32 = 0x7fffffff; // ICU4N TODO: API - rename to follow .NET Conventions

        /// <summary>
        /// Move the iterator forward to the next code point, and return that code point,
        /// leaving the iterator positioned at char returned.
        /// For Supplementary chars, the iterator is left positioned at the lead surrogate.
        /// </summary>
        /// <param name="ci">The character iterator.</param>
        /// <returns>The next code point.</returns>
        public static int Next32(CharacterIterator ci)
        {
            // If the current position is at a surrogate pair, move to the trail surrogate
            //   which leaves it in position for underlying iterator's next() to work.
            int c = ci.Current;
            if (c >= UTF16.LEAD_SURROGATE_MIN_VALUE && c <= UTF16.LEAD_SURROGATE_MAX_VALUE)
            {
                c = ci.MoveNext();
                if (c < UTF16.TRAIL_SURROGATE_MIN_VALUE || c > UTF16.TRAIL_SURROGATE_MAX_VALUE)
                {
                    ci.MovePrevious();
                }
            }

            // For BMP chars, this next() is the real deal.
            c = ci.MoveNext();

            // If we might have a lead surrogate, we need to peak ahead to get the trail 
            //  even though we don't want to really be positioned there.
            if (c >= UTF16.LEAD_SURROGATE_MIN_VALUE)
            {
                c = NextTrail32(ci, c);
            }

            if (c >= UTF16.SUPPLEMENTARY_MIN_VALUE && c != DONE32)
            {
                // We got a supplementary char.  Back the iterator up to the postion
                // of the lead surrogate.
                ci.MovePrevious();
            }
            return c;
        }
   
        /// <summary>
        /// Out-of-line portion of the in-line <see cref="Next32(CharacterIterator)"/> code.
        /// The call site does an initial ci.Next() and calls this function
        /// if the 16 bit value it gets is >= <see cref="UTF16.LEAD_SURROGATE_MIN_VALUE"/>.
        /// </summary>
        // NOTE:  we leave the underlying char iterator positioned in the
        //        middle of a surrogate pair.  ci.next() will work correctly
        //        from there, but the ci.getIndex() will be wrong, and needs
        //        adjustment.
        public static int NextTrail32(CharacterIterator ci, int lead)
        {
            if (lead == CharacterIterator.Done && ci.Index >= ci.EndIndex)
            {
                return DONE32;
            }
            int retVal = lead;
            if (lead <= UTF16.LEAD_SURROGATE_MAX_VALUE)
            {
                char cTrail = ci.MoveNext();
                if (UTF16.IsTrailSurrogate(cTrail))
                {
                    retVal = ((lead - UTF16.LEAD_SURROGATE_MIN_VALUE) << 10) +
                                (cTrail - UTF16.TRAIL_SURROGATE_MIN_VALUE) +
                                UTF16.SUPPLEMENTARY_MIN_VALUE;
                }
                else
                {
                    ci.MovePrevious();
                }
            }
            return retVal;
        }

        public static int Previous32(CharacterIterator ci)
        {
            if (ci.Index <= ci.BeginIndex)
            {
                return DONE32;
            }
            char trail = ci.MovePrevious();
            int retVal = trail;
            if (UTF16.IsTrailSurrogate(trail) && ci.Index > ci.BeginIndex)
            {
                char lead = ci.MovePrevious();
                if (UTF16.IsLeadSurrogate(lead))
                {
                    retVal = (((int)lead - UTF16.LEAD_SURROGATE_MIN_VALUE) << 10) +
                              ((int)trail - UTF16.TRAIL_SURROGATE_MIN_VALUE) +
                              UTF16.SUPPLEMENTARY_MIN_VALUE;
                }
                else
                {
                    ci.MoveNext();
                }
            }
            return retVal;
        }

        public static int Current32(CharacterIterator ci)
        {
            char lead = ci.Current;
            int retVal = lead;
            if (retVal < UTF16.LEAD_SURROGATE_MIN_VALUE)
            {
                return retVal;
            }
            if (UTF16.IsLeadSurrogate(lead))
            {
                int trail = (int)ci.MoveNext();
                ci.MovePrevious();
                if (UTF16.IsTrailSurrogate((char)trail))
                {
                    retVal = ((lead - UTF16.LEAD_SURROGATE_MIN_VALUE) << 10) +
                             (trail - UTF16.TRAIL_SURROGATE_MIN_VALUE) +
                             UTF16.SUPPLEMENTARY_MIN_VALUE;
                }
            }
            else
            {
                if (lead == CharacterIterator.Done)
                {
                    if (ci.Index >= ci.EndIndex)
                    {
                        retVal = DONE32;
                    }
                }
            }
            return retVal;
        }
    }
}
