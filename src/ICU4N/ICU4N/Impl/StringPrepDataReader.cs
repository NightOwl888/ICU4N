using ICU4N.Support.IO;
using System;

namespace ICU4N.Impl
{
    /// <author>ram</author>
    public sealed class StringPrepDataReader : IAuthenticate
    {
        private readonly static bool debug = ICUDebug.Enabled("NormalizerDataReader");

        /// <summary>
        /// Private constructor.
        /// </summary>
        /// <param name="bytes">ICU StringPrep data file buffer.</param>
        /// <exception cref="System.IO.IOException">If data file fails authentication.</exception>
        public StringPrepDataReader(ByteBuffer bytes)
        {
            if (debug) Console.Out.WriteLine("Bytes in buffer " + bytes.Remaining);

            byteBuffer = bytes;
            unicodeVersion = ICUBinary.ReadHeader(byteBuffer, DATA_FORMAT_ID, this);

            if (debug) Console.Out.WriteLine("Bytes left in byteBuffer " + byteBuffer.Remaining);
        }

        public char[] Read(int length)
        {
            //Read the extra data
            return ICUBinary.GetChars(byteBuffer, length, 0);
        }

        public bool IsDataVersionAcceptable(byte[] version)
        {
            return version[0] == DATA_FORMAT_VERSION[0]
                   && version[2] == DATA_FORMAT_VERSION[2]
                   && version[3] == DATA_FORMAT_VERSION[3];
        }
        public int[] ReadIndexes(int length)
        {
            int[]
            indexes = new int[length];
            //Read the indexes
            for (int i = 0; i < length; i++)
            {
                indexes[i] = byteBuffer.GetInt32();
            }
            return indexes;
        }

        public byte[] GetUnicodeVersion()
        {
            return ICUBinary.GetVersionByteArrayFromCompactInt(unicodeVersion);
        }
        // private data members -------------------------------------------------


        /// <summary>
        /// ICU data file input stream
        /// </summary>
        private ByteBuffer byteBuffer;
        private int unicodeVersion;
        /**
        * File format version that this class understands.
        * No guarantees are made if a older version is used
        * see store.c of gennorm for more information and values
        */
        //* dataFormat="SPRP" 0x53, 0x50, 0x52, 0x50  */
        private static readonly int DATA_FORMAT_ID = 0x53505250;
        private static readonly byte[] DATA_FORMAT_VERSION = {(byte)0x3, (byte)0x2,
                                                        (byte)0x5, (byte)0x2};
    }
}
