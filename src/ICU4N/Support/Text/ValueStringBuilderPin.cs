using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#nullable enable

namespace ICU4N.Text
{
    /// <summary>
    /// This is a specialized ref struct used to safely get a pinned pointer to a ValueStringBuilder.
    /// Pinning using this struct is effectively the same operation as:
    /// 
    /// <code>
    /// fixed (char* charPtr = &amp;sb)
    /// {
    ///     // Safe to use charPtr
    /// }
    /// </code>
    /// 
    /// With the exception that the fixed block is not required and the pin must be undone using <see cref="Free()"/>.
    /// <para/>
    /// This allows the use of pinning in the middle of complex business logic without significantly restructuring the
    /// code.
    /// <para/>
    /// Note that a single instance is meant to be used with a single <see cref="ValueStringBuilder"/> instance and
    /// reentry is not supported. Reuse is allowed, but <see cref="Free()"/> must be called first.
    /// </summary>
    internal unsafe ref struct ValueStringBuilderPin
    {
        private GCHandle handle;
        private bool pinned;

        public char* GetSafePointer(ref ValueStringBuilder valueStringBuilder)
        {
            // If already pinned, unpin first.
            if (pinned)
            {
                Debug.Assert(true, "We should not need to unpin this. Only one pinned reference is supported at a time.");
                Free();
            }

            if (!valueStringBuilder.CapacityExceeded)
            {
                // Safe: stack-based
                pinned = false;
                return (char*)Unsafe.AsPointer(ref valueStringBuilder.GetPinnableReference());
            }
            else
            {
                // Unsafe: heap-based, must pin
                handle = GCHandle.Alloc(valueStringBuilder.RawArray!, GCHandleType.Pinned);
                pinned = true;
                return (char*)handle.AddrOfPinnedObject();
            }
        }

        public void Free()
        {
            if (pinned)
            {
                handle.Free();
                pinned = false;
            }
        }

        public void Dispose() => Free();
    }
}
