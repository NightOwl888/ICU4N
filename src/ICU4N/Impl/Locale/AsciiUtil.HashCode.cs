// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ICU4N.Support.Text;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#nullable enable

namespace ICU4N.Impl.Locale
{
    public static partial class AsciiUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetHashCodeOrdinalIgnoreCase(ReadOnlySpan<char> value)
        {
            ulong seed = Marvin.DefaultSeed;
            return ComputeHash32OrdinalIgnoreCase(ref MemoryMarshal.GetReference(value), value.Length /* in chars, not bytes */, (uint)seed, (uint)(seed >> 32));
        }

        /// <summary>
        /// Compute a Marvin OrdinalIgnoreCase hash and collapse it into a 32-bit hash.
        /// n.b. <paramref name="count"/> is specified as char count, not byte count.
        /// </summary>
        internal static unsafe int ComputeHash32OrdinalIgnoreCase(ref char data, int count, uint p0, uint p1)
        {
            uint ucount = (uint)count; // in chars
            uint byteOffset = 0; // in bytes

            uint tempValue;

            fixed (char* pData = &data)
            {
                byte* pBytes = (byte*)pData;

                // We operate on 32-bit integers (two chars) at a time.

                while (ucount >= 2)
                {
                    tempValue = Unsafe.ReadUnaligned<uint>(pBytes + byteOffset);
                    Debug.Assert(Utf16Utility.AllCharsInUInt32AreAscii(tempValue)); // ASSUMPTION: Caller didn't pass any non-ASCII chars
                    p0 += Utf16Utility.ConvertAllAsciiCharsInUInt32ToLowercase(tempValue); // ICU4N: Note that .NET normally converts to uppercase
                    Marvin.Block(ref p0, ref p1);

                    byteOffset += 4;
                    ucount -= 2;
                }

                // We have either one char (16 bits) or zero chars left over.
                Debug.Assert(ucount < 2);

                if (ucount > 0)
                {
                    tempValue = *(pBytes + byteOffset);

                    // addition is written with -0x80u to allow fall-through to next statement rather than jmp past it
                    p0 += Utf16Utility.ConvertAllAsciiCharsInUInt32ToLowercase(tempValue) + (0x800000u - 0x80u); // ICU4N: Note that .NET normally converts to uppercase
                }
                p0 += 0x80u;

                Marvin.Block(ref p0, ref p1);
                Marvin.Block(ref p0, ref p1);

                return (int)(p1 ^ p0);
            }
        }
    }
}
