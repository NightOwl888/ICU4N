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
        public static readonly int Done32 = 0x7fffffff;

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
            if (c >= UTF16.LeadSurrogateMinValue && c <= UTF16.LeadSurrogateMaxValue)
            {
                c = ci.Next();
                if (c < UTF16.TrailSurrogateMinValue || c > UTF16.TrailSurrogateMaxValue)
                {
                    ci.Previous();
                }
            }

            // For BMP chars, this next() is the real deal.
            c = ci.Next();

            // If we might have a lead surrogate, we need to peak ahead to get the trail 
            //  even though we don't want to really be positioned there.
            if (c >= UTF16.LeadSurrogateMinValue)
            {
                c = NextTrail32(ci, c);
            }

            if (c >= UTF16.SupplementaryMinValue && c != Done32)
            {
                // We got a supplementary char.  Back the iterator up to the postion
                // of the lead surrogate.
                ci.Previous();
            }
            return c;
        }
   
        /// <summary>
        /// Out-of-line portion of the in-line <see cref="Next32(CharacterIterator)"/> code.
        /// The call site does an initial ci.Next() and calls this function
        /// if the 16 bit value it gets is >= <see cref="UTF16.LeadSurrogateMinValue"/>.
        /// </summary>
        // NOTE:  we leave the underlying char iterator positioned in the
        //        middle of a surrogate pair.  ci.next() will work correctly
        //        from there, but the ci.getIndex() will be wrong, and needs
        //        adjustment.
        public static int NextTrail32(CharacterIterator ci, int lead)
        {
            if (lead == CharacterIterator.Done && ci.Index >= ci.EndIndex)
            {
                return Done32;
            }
            int retVal = lead;
            if (lead <= UTF16.LeadSurrogateMaxValue)
            {
                char cTrail = ci.Next();
                if (UTF16.IsTrailSurrogate(cTrail))
                {
                    retVal = ((lead - UTF16.LeadSurrogateMinValue) << 10) +
                                (cTrail - UTF16.TrailSurrogateMinValue) +
                                UTF16.SupplementaryMinValue;
                }
                else
                {
                    ci.Previous();
                }
            }
            return retVal;
        }

        public static int Previous32(CharacterIterator ci)
        {
            if (ci.Index <= ci.BeginIndex)
            {
                return Done32;
            }
            char trail = ci.Previous();
            int retVal = trail;
            if (UTF16.IsTrailSurrogate(trail) && ci.Index > ci.BeginIndex)
            {
                char lead = ci.Previous();
                if (UTF16.IsLeadSurrogate(lead))
                {
                    retVal = (((int)lead - UTF16.LeadSurrogateMinValue) << 10) +
                              ((int)trail - UTF16.TrailSurrogateMinValue) +
                              UTF16.SupplementaryMinValue;
                }
                else
                {
                    ci.Next();
                }
            }
            return retVal;
        }

        public static int Current32(CharacterIterator ci)
        {
            char lead = ci.Current;
            int retVal = lead;
            if (retVal < UTF16.LeadSurrogateMinValue)
            {
                return retVal;
            }
            if (UTF16.IsLeadSurrogate(lead))
            {
                int trail = (int)ci.Next();
                ci.Previous();
                if (UTF16.IsTrailSurrogate((char)trail))
                {
                    retVal = ((lead - UTF16.LeadSurrogateMinValue) << 10) +
                             (trail - UTF16.TrailSurrogateMinValue) +
                             UTF16.SupplementaryMinValue;
                }
            }
            else
            {
                if (lead == CharacterIterator.Done)
                {
                    if (ci.Index >= ci.EndIndex)
                    {
                        retVal = Done32;
                    }
                }
            }
            return retVal;
        }
    }
}
