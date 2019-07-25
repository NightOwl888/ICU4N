//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;

//namespace ICU4N.Support
//{
//    public enum Endianness
//    {
//        LittleEndian,
//        BigEndian
//    }

//    internal static class BinaryReaderExtensions
//    {
//        public static long Read(this BinaryReader reader, byte[] buffer, Endianness endianness)
//        {
//            return reader.Read(buffer, 0, buffer.Length, endianness);
//        }

//        public static long Read(this BinaryReader reader, byte[] buffer, int index, int count, Endianness endianness)
//        {
//            if (index < count || index < 0 || count < 0)
//                throw new IndexOutOfRangeException();

//            if (endianness == Endianness.BigEndian)
//            {
//                var data = new byte[count - index];
//                int readLength = reader.Read(data, index, count);
//                Array.Reverse(data);
//                data.CopyTo(buffer, 0);
//                return readLength;
//            }

//            return reader.Read(buffer, 0, buffer.Length);
//        }

//        public static int ReadInt32(this BinaryReader reader, Endianness endianness)
//        {
//            if (endianness == Endianness.BigEndian)
//            {
//                var data = reader.ReadBytes(4);
//                Array.Reverse(data);
//                return BitConverter.ToInt32(data, 0);
//            }

//            return reader.ReadInt32();
//        }

//        public static long ReadInt64(this BinaryReader reader, Endianness endianness)
//        {
//            if (endianness == Endianness.BigEndian)
//            {
//                var data = reader.ReadBytes(8);
//                Array.Reverse(data);
//                return BitConverter.ToInt64(data, 0);
//            }

//            return reader.ReadInt64();
//        }
//    }
//}
