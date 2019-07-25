//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ICU4N.Impl.Coll
//{
//    // TODO: There must be a Java class for a growable array of ints without auto-boxing to Integer?!
//    // Keep the API parallel to the C++ version for ease of porting. Port methods only as needed.
//    // If & when we start using something else, we might keep this as a thin wrapper for porting.

//    /// <created>2014feb10</created>
//    /// <author>Markus W. Scherer</author>
//    public sealed class UVector32
//    {
//        public UVector32() { }
//        public bool IsEmpty { get { return length == 0; } }
//        public int Length { get { return length; } }
//        public int ElementAt(int i) { return buffer[i]; }
//        public int[] GetBuffer() { return buffer; }
//        public void AddElement(int e)
//        {
//            EnsureAppendCapacity();
//            buffer[length++] = e;
//        }
//        public void SetElementAt(int elem, int index) { buffer[index] = elem; }
//        public void InsertElementAt(int elem, int index)
//        {
//            EnsureAppendCapacity();
//            System.Array.Copy(buffer, index, buffer, index + 1, length - index);
//            buffer[index] = elem;
//            ++length;
//        }
//        public void RemoveAllElements()
//        {
//            length = 0;
//        }

//        private void EnsureAppendCapacity()
//        {
//            if (length >= buffer.Length)
//            {
//                int newCapacity = buffer.Length <= 0xffff ? 4 * buffer.Length : 2 * buffer.Length;
//                int[] newBuffer = new int[newCapacity];
//                System.Array.Copy(buffer, 0, newBuffer, 0, length);
//                buffer = newBuffer;
//            }
//        }
//        private int[] buffer = new int[32];
//        private int length = 0;
//    }
//}
