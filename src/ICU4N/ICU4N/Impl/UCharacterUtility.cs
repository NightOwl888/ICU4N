using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Impl
{
    /// <summary>
    /// Internal character utility class for simple data type conversion and string 
    /// parsing functions. Does not have an analog in .NET.
    /// </summary>
    /// <author>Syn Wee Quek</author>
    /// <since>sep2900</since>
    public static class UCharacterUtility
    {
        // public methods -----------------------------------------------------

        /// <summary>
        /// Determines if codepoint is a non character.
        /// </summary>
        /// <param name="ch">Codepoint.</param>
        /// <returns>true if codepoint is a non character false otherwise.</returns>
        public static bool IsNonCharacter(int ch)
        {
            if ((ch & NON_CHARACTER_SUFFIX_MIN_3_0_) ==
                                                NON_CHARACTER_SUFFIX_MIN_3_0_)
            {
                return true;
            }

            return ch >= NON_CHARACTER_MIN_3_1_ && ch <= NON_CHARACTER_MAX_3_1_;
        }

        // package private methods ---------------------------------------------

        /// <summary>
        /// Joining 2 chars to form an int.
        /// </summary>
        /// <param name="msc">Most significant char.</param>
        /// <param name="lsc">Least significant char.</param>
        /// <returns><see cref="int"/> form.</returns>
        internal static int ToInt(char msc, char lsc)
        {
            return ((msc << 16) | lsc);
        }

        /// <summary>
        /// Retrieves a null terminated substring from an array of bytes.
        /// Substring is a set of non-zero bytes starting from argument start to the 
        /// next zero byte. If the first byte is a zero, the next byte will be taken as
        /// the first byte.
        /// </summary>
        /// <param name="str"><see cref="StringBuffer"/> to store data in, data will be store with each byte as a char.</param>
        /// <param name="array">Byte array.</param>
        /// <param name="index">Index to start substring in byte count.</param>
        /// <returns>The end position of the substring within the character array.</returns>
        internal static int GetNullTermByteSubString(StringBuffer str, byte[] array,
                                                      int index)
        {
            byte b = 1;

            while (b != 0)
            {
                b = array[index];
                if (b != 0)
                {
                    str.Append((char)(b & 0x00FF));
                }
                index++;
            }
            return index;
        }

        /// <summary>
        /// Compares a null terminated substring from an array of bytes.
        /// Substring is a set of non-zero bytes starting from argument start to the 
        /// next zero byte. if the first byte is a zero, the next byte will be taken as
        /// the first byte.
        /// </summary>
        /// <param name="str">String to compare.</param>
        /// <param name="array">Byte array.</param>
        /// <param name="strindex">Index within str to start comparing.</param>
        /// <param name="aindex">Array index to start in byte count.</param>
        /// <returns>The end position of the substring within str if matches otherwise a -1.</returns>
        internal static int CompareNullTermByteSubString(string str, byte[] array,
                                                          int strindex, int aindex)
        {
            byte b = 1;
            int length = str.Length;

            while (b != 0)
            {
                b = array[aindex];
                aindex++;
                if (b == 0)
                {
                    break;
                }
                // if we have reached the end of the string and yet the array has not 
                // reached the end of their substring yet, abort
                if (strindex == length
                    || (str[strindex] != (char)(b & 0xFF)))
                {
                    return -1;
                }
                strindex++;
            }
            return strindex;
        }

        /// <summary>
        /// Skip null terminated substrings from an array of bytes.
        /// Substring is a set of non-zero bytes starting from argument start to the
        /// next zero byte. If the first byte is a zero, the next byte will be taken as
        /// the first byte.
        /// </summary>
        /// <param name="array">Byte array.</param>
        /// <param name="index">Index to start substrings in byte count.</param>
        /// <param name="skipcount">Number of null terminated substrings to skip.</param>
        /// <returns>The end position of the substrings within the character array.</returns>
        internal static int SkipNullTermByteSubString(byte[] array, int index,
                                                       int skipcount)
        {
            byte b;
            for (int i = 0; i < skipcount; i++)
            {
                b = 1;
                while (b != 0)
                {
                    b = array[index];
                    index++;
                }
            }
            return index;
        }

        /// <summary>
        /// Skip substrings from an array of characters, where each character is a set 
        /// of 2 bytes. substring is a set of non-zero bytes starting from argument 
        /// start to the byte of the argument value. skips up to a max number of 
        /// characters.
        /// </summary>
        /// <param name="array">Byte array to parse.</param>
        /// <param name="index">Index to start substrings in byte count.</param>
        /// <param name="length">The max number of bytes to skip.</param>
        /// <param name="skipend">Value of byte to skip to.</param>
        /// <returns>The number of bytes skipped.</returns>
        internal static int SkipByteSubString(byte[] array, int index, int length,
                                               byte skipend)
        {
            int result;
            byte b;

            for (result = 0; result < length; result++)
            {
                b = array[index + result];
                if (b == skipend)
                {
                    result++;
                    break;
                }
            }

            return result;
        }

        // private data member --------------------------------------------------

        /// <summary>
        /// Minimum suffix value that indicates if a character is non character.
        /// Unicode 3.0 non characters.
        /// </summary>
        private const int NON_CHARACTER_SUFFIX_MIN_3_0_ = 0xFFFE;
        /// <summary>
        /// New minimum non character in Unicode 3.1
        /// </summary>
        private const int NON_CHARACTER_MIN_3_1_ = 0xFDD0;
        /// <summary>
        /// New non character range in Unicode 3.1
        /// </summary>
        private const int NON_CHARACTER_MAX_3_1_ = 0xFDEF;
    }
}
