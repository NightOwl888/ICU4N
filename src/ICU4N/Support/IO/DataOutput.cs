using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Support.IO
{
    /// <summary>
    /// Equivalent to Java's DataOutput interface
    /// </summary>
    public interface IDataOutput
    {
        void Write(byte[] buffer);

        void Write(byte[] buffer, int offset, int count);

        void Write(int oneByte);

        void WriteBoolean(bool val);

        void WriteByte(int val);

        void WriteBytes(string str);

        void WriteChar(int val);

        void WriteChars(string str);

        void WriteDouble(double val);

        /// <summary>
        /// NOTE: This was writeFloat() in Java
        /// </summary>
        void WriteSingle(float val);

        /// <summary>
        /// NOTE: This was writeInt() in Java
        /// </summary>
        void WriteInt32(int val);

        /// <summary>
        /// NOTE: This was writeInt64() in Java
        /// </summary>
        void WriteInt64(long val);

        /// <summary>
        /// NOTE: This was writeShort() in Java
        /// </summary>
        void WriteInt16(int val);

        void WriteUTF(string str);
    }
}
