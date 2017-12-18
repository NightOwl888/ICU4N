using ICU4N.Support.Text;
using ICU4N.Util;

namespace ICU4N.Impl.Coll
{
    public class BOCSU
    {
        // public methods -------------------------------------------------------

        /**
         * Encode the code points of a string as
         * a sequence of byte-encoded differences (slope detection),
         * preserving lexical order.
         *
         * <p>Optimize the difference-taking for runs of Unicode text within
         * small scripts:
         *
         * <p>Most small scripts are allocated within aligned 128-blocks of Unicode
         * code points. Lexical order is preserved if "prev" is always moved
         * into the middle of such a block.
         *
         * <p>Additionally, "prev" is moved from anywhere in the Unihan
         * area into the middle of that area.
         * Note that the identical-level run in a sort key is generated from
         * NFD text - there are never Hangul characters included.
         */
        public static int WriteIdenticalLevelRun(int prev, ICharSequence s, int i, int length, ByteArrayWrapper sink)
        {
            while (i < length)
            {
                // We must have capacity>=SLOPE_MAX_BYTES in case writeDiff() writes that much,
                // but we do not want to force the sink to allocate
                // for a large min_capacity because we might actually only write one byte.
                EnsureAppendCapacity(sink, 16, s.Length * 2);
                byte[] buffer = sink.Bytes;
                int capacity = buffer.Length;
                int p = sink.Length;
                int lastSafe = capacity - SLOPE_MAX_BYTES_;
                while (i < length && p <= lastSafe)
                {
                    if (prev < 0x4e00 || prev >= 0xa000)
                    {
                        prev = (prev & ~0x7f) - SLOPE_REACH_NEG_1_;
                    }
                    else
                    {
                        // Unihan U+4e00..U+9fa5:
                        // double-bytes down from the upper end
                        prev = 0x9fff - SLOPE_REACH_POS_2_;
                    }

                    int c = Character.CodePointAt(s, i);
                    i += Character.CharCount(c);
                    if (c == 0xfffe)
                    {
                        buffer[p++] = 2;  // merge separator
                        prev = 0;
                    }
                    else
                    {
                        p = WriteDiff(c - prev, buffer, p);
                        prev = c;
                    }
                }
                sink.Length = p;
            }
            return prev;
        }

        private static void EnsureAppendCapacity(ByteArrayWrapper sink, int minCapacity, int desiredCapacity)
        {
            int remainingCapacity = sink.Bytes.Length - sink.Length;
            if (remainingCapacity >= minCapacity) { return; }
            if (desiredCapacity < minCapacity) { desiredCapacity = minCapacity; }
            sink.EnsureCapacity(sink.Length + desiredCapacity);
        }

        // private data members --------------------------------------------------

        /** 
         * Do not use byte values 0, 1, 2 because they are separators in sort keys.
         */
        private static readonly int SLOPE_MIN_ = 3;
        private static readonly int SLOPE_MAX_ = 0xff;
        private static readonly int SLOPE_MIDDLE_ = 0x81;
        private static readonly int SLOPE_TAIL_COUNT_ = SLOPE_MAX_ - SLOPE_MIN_ + 1;
        private static readonly int SLOPE_MAX_BYTES_ = 4;

        /**
         * Number of lead bytes:
         * 1        middle byte for 0
         * 2*80=160 single bytes for !=0
         * 2*42=84  for double-byte values
         * 2*3=6    for 3-byte values
         * 2*1=2    for 4-byte values
         *
         * The sum must be &lt;=SLOPE_TAIL_COUNT.
         *
         * Why these numbers?
         * - There should be >=128 single-byte values to cover 128-blocks
         *   with small scripts.
         * - There should be >=20902 single/double-byte values to cover Unihan.
         * - It helps CJK Extension B some if there are 3-byte values that cover
         *   the distance between them and Unihan.
         *   This also helps to jump among distant places in the BMP.
         * - Four-byte values are necessary to cover the rest of Unicode.
         *
         * Symmetrical lead byte counts are for convenience.
         * With an equal distribution of even and odd differences there is also
         * no advantage to asymmetrical lead byte counts.
         */
        private static readonly int SLOPE_SINGLE_ = 80;
        private static readonly int SLOPE_LEAD_2_ = 42;
        private static readonly int SLOPE_LEAD_3_ = 3;
        //private static readonly int SLOPE_LEAD_4_ = 1;

        /** 
         * The difference value range for single-byters.
         */
        private static readonly int SLOPE_REACH_POS_1_ = SLOPE_SINGLE_;
        private static readonly int SLOPE_REACH_NEG_1_ = (-SLOPE_SINGLE_);

        /** 
         * The difference value range for double-byters.
         */
        private static readonly int SLOPE_REACH_POS_2_ =
            SLOPE_LEAD_2_ * SLOPE_TAIL_COUNT_ + SLOPE_LEAD_2_ - 1;
        private static readonly int SLOPE_REACH_NEG_2_ = (-SLOPE_REACH_POS_2_ - 1);

        /** 
         * The difference value range for 3-byters.
         */
        private static readonly int SLOPE_REACH_POS_3_ = SLOPE_LEAD_3_
            * SLOPE_TAIL_COUNT_
            * SLOPE_TAIL_COUNT_
            + (SLOPE_LEAD_3_ - 1)
            * SLOPE_TAIL_COUNT_ +
            (SLOPE_TAIL_COUNT_ - 1);
        private static readonly int SLOPE_REACH_NEG_3_ = (-SLOPE_REACH_POS_3_ - 1);

        /** 
         * The lead byte start values.
         */
        private static readonly int SLOPE_START_POS_2_ = SLOPE_MIDDLE_
            + SLOPE_SINGLE_ + 1;
        private static readonly int SLOPE_START_POS_3_ = SLOPE_START_POS_2_
            + SLOPE_LEAD_2_;
        private static readonly int SLOPE_START_NEG_2_ = SLOPE_MIDDLE_ +
            SLOPE_REACH_NEG_1_;
        private static readonly int SLOPE_START_NEG_3_ = SLOPE_START_NEG_2_
            - SLOPE_LEAD_2_;

        // private constructor ---------------------------------------------------

        /**
         * Constructor private to prevent initialization
         */
        ///CLOVER:OFF
        private BOCSU()
        {
        }
        ///CLOVER:ON                                                                                       

        // private methods -------------------------------------------------------

        /**
         * Integer division and modulo with negative numerators
         * yields negative modulo results and quotients that are one more than
         * what we need here.
         * @param number which operations are to be performed on
         * @param factor the factor to use for division
         * @return (result of division) << 32 | modulo 
         */
        private static long GetNegDivMod(int number, int factor)
        {
            int modulo = number % factor;
            long result = number / factor;
            if (modulo < 0)
            {
                --result;
                modulo += factor;
            }
            return (result << 32) | modulo;
        }

        /**
         * Encode one difference value -0x10ffff..+0x10ffff in 1..4 bytes,
         * preserving lexical order
         * @param diff
         * @param buffer byte buffer to append to
         * @param offset to the byte buffer to start appending
         * @return end offset where the appending stops
         */
        private static int WriteDiff(int diff, byte[] buffer, int offset)
        {
            if (diff >= SLOPE_REACH_NEG_1_)
            {
                if (diff <= SLOPE_REACH_POS_1_)
                {
                    buffer[offset++] = (byte)(SLOPE_MIDDLE_ + diff);
                }
                else if (diff <= SLOPE_REACH_POS_2_)
                {
                    buffer[offset++] = (byte)(SLOPE_START_POS_2_
                                               + (diff / SLOPE_TAIL_COUNT_));
                    buffer[offset++] = (byte)(SLOPE_MIN_ +
                                               (diff % SLOPE_TAIL_COUNT_));
                }
                else if (diff <= SLOPE_REACH_POS_3_)
                {
                    buffer[offset + 2] = (byte)(SLOPE_MIN_
                                                + (diff % SLOPE_TAIL_COUNT_));
                    diff /= SLOPE_TAIL_COUNT_;
                    buffer[offset + 1] = (byte)(SLOPE_MIN_
                                                + (diff % SLOPE_TAIL_COUNT_));
                    buffer[offset] = (byte)(SLOPE_START_POS_3_
                                            + (diff / SLOPE_TAIL_COUNT_));
                    offset += 3;
                }
                else
                {
                    buffer[offset + 3] = (byte)(SLOPE_MIN_
                                                + diff % SLOPE_TAIL_COUNT_);
                    diff /= SLOPE_TAIL_COUNT_;
                    buffer[offset + 2] = (byte)(SLOPE_MIN_
                                            + diff % SLOPE_TAIL_COUNT_);
                    diff /= SLOPE_TAIL_COUNT_;
                    buffer[offset + 1] = (byte)(SLOPE_MIN_
                                                + diff % SLOPE_TAIL_COUNT_);
                    buffer[offset] = (byte)SLOPE_MAX_;
                    offset += 4;
                }
            }
            else
            {
                long division = GetNegDivMod(diff, SLOPE_TAIL_COUNT_);
                int modulo = (int)division;
                if (diff >= SLOPE_REACH_NEG_2_)
                {
                    diff = (int)(division >> 32);
                    buffer[offset++] = (byte)(SLOPE_START_NEG_2_ + diff);
                    buffer[offset++] = (byte)(SLOPE_MIN_ + modulo);
                }
                else if (diff >= SLOPE_REACH_NEG_3_)
                {
                    buffer[offset + 2] = (byte)(SLOPE_MIN_ + modulo);
                    diff = (int)(division >> 32);
                    division = GetNegDivMod(diff, SLOPE_TAIL_COUNT_);
                    modulo = (int)division;
                    diff = (int)(division >> 32);
                    buffer[offset + 1] = (byte)(SLOPE_MIN_ + modulo);
                    buffer[offset] = (byte)(SLOPE_START_NEG_3_ + diff);
                    offset += 3;
                }
                else
                {
                    buffer[offset + 3] = (byte)(SLOPE_MIN_ + modulo);
                    diff = (int)(division >> 32);
                    division = GetNegDivMod(diff, SLOPE_TAIL_COUNT_);
                    modulo = (int)division;
                    diff = (int)(division >> 32);
                    buffer[offset + 2] = (byte)(SLOPE_MIN_ + modulo);
                    division = GetNegDivMod(diff, SLOPE_TAIL_COUNT_);
                    modulo = (int)division;
                    buffer[offset + 1] = (byte)(SLOPE_MIN_ + modulo);
                    buffer[offset] = (byte)SLOPE_MIN_;
                    offset += 4;
                }
            }
            return offset;
        }
    }
}
