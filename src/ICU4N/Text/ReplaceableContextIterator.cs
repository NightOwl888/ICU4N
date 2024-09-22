using ICU4N.Impl;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ICU4N.Text
{
    internal static class Replaceable
    {
        /// <summary>
        /// Case context iterator using a <see cref="IReplaceable"/>.
        /// This is an implementation that matches the delegate <see cref="UCaseContextIterator"/>.
        /// Direct port of casetrn.cpp/utrans_rep_caseContextIterator()
        /// </summary>
        // ICU4N: This was declared in the namespace in C but we cannot do that in C#. So, added a Replaceable type.
        // [CLSCompliant(false)]
        public static unsafe int CaseContextIterator(IntPtr context, sbyte dir)
        {
            UCaseContext* csc = (UCaseContext*)context;
            GCHandle handle = GCHandle.FromIntPtr(csc->p);
            IReplaceable rep = (IReplaceable)handle.Target;

            int c;

            if (dir < 0)
            {
                /* reset for backward iteration */
                csc->index = csc->cpStart;
                csc->dir = dir;
            }
            else if (dir > 0)
            {
                /* reset for forward iteration */
                csc->index = csc->cpLimit;
                csc->dir = dir;
            }
            else
            {
                /* continue current iteration direction */
                dir = csc->dir;
            }

            // automatically adjust start and limit if the Replaceable disagrees
            // with the original values
            if (dir < 0)
            {
                if (csc->start < csc->index)
                {
                    c = rep.Char32At(csc->index - 1);
                    if (c < 0)
                    {
                        csc->start = csc->index;
                    }
                    else
                    {
                        csc->index -= UTF16.GetCharCount(c);
                        return c;
                    }
                }
            }
            else
            {
                // detect, and store in csc->b1, if we hit the limit
                if (csc->index < csc->limit)
                {
                    c = rep.Char32At(csc->index);
                    if (c < 0)
                    {
                        csc->limit = csc->index;
                        csc->b1 = true;
                    }
                    else
                    {
                        csc->index += UTF16.GetCharCount(c);
                        return c;
                    }
                }
                else
                {
                    csc->b1 = true;
                }
            }
            return CaseMapImpl.U_SENTINEL;
        }
    }
}
