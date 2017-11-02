using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.IO
{
    /// <summary>
    /// Defines byte order constants.
    /// </summary>
    public sealed class ByteOrder
    {
        /// <summary>
        /// This constant represents big endian.
        /// </summary>
        public static readonly ByteOrder BIG_ENDIAN = new ByteOrder("BIG_ENDIAN"); //$NON-NLS-1$

        /// <summary>
        /// This constant represents little endian.
        /// </summary>
        public static readonly ByteOrder LITTLE_ENDIAN = new ByteOrder("LITTLE_ENDIAN"); //$NON-NLS-1$

        private static readonly ByteOrder NATIVE_ORDER;

        static ByteOrder()
        {
            // Read endianness from the current system.
            if (BitConverter.IsLittleEndian)
            {
                NATIVE_ORDER = LITTLE_ENDIAN;
            }
            else
            {
                NATIVE_ORDER = BIG_ENDIAN;
            }
        }

        /// <summary>
        /// Returns the current platform byte order.
        /// </summary>
        public static ByteOrder NativeOrder
        {
            get { return NATIVE_ORDER; }
        }

        private readonly string name;

        private ByteOrder(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Returns a string that describes this object.
        /// </summary>
        /// <returns>
        /// "BIG_ENDIAN" for <see cref="ByteOrder.BIG_ENDIAN"/> objects,
        /// "LITTLE_ENDIAN" for <see cref="ByteOrder.LITTLE_ENDIAN"/> objects.
        /// </returns>
        public override string ToString()
        {
            return name;
        }
    }
}
