using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ICU4N
{
    internal static class MemoryHelper
    {
        /// <summary>
        /// Compares two spans to determine if they are pointing to the same underlying memory location.
        /// </summary>
        internal static unsafe bool AreSame<T>(scoped ReadOnlySpan<T> span1, scoped ReadOnlySpan<T> span2)
        {
            // Obtain pointers to the memory addresses of the first elements of the spans
            ref T firstElementSpan1 = ref MemoryMarshal.GetReference(span1);
            ref T firstElementSpan2 = ref MemoryMarshal.GetReference(span2);

            // Cast the char* pointers to byte* to get their memory addresses
            byte* ptr1 = (byte*)Unsafe.AsPointer(ref firstElementSpan1);
            byte* ptr2 = (byte*)Unsafe.AsPointer(ref firstElementSpan2);

            // Compare the memory addresses of the first elements
            return ptr1 == ptr2;
        }
    }
}
