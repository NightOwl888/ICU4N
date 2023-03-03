using ICU4N.Dev.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * @author Elena Semukhina
 */

namespace ICU4N.Numerics.BigMath
{
    /// <summary>
    /// Class:   java.math.BigInteger
    /// Constructors: BigInteger(byte[] a), BigInteger(int sign, byte[] a), 
    ///               BigInteger(String val, int radix)
    /// </summary>
    public class BigIntegerConstructorsTest : TestFmwk
    {
        /**
         * Create a number from an array of bytes.
         * Verify an exception thrown if an array is zero bytes long
         */
        [Test]
        public void testConstructorBytesException()
        {
            byte[] aBytes = { };
            try
            {
                new BigInteger(aBytes);
                fail("NumberFormatException has not been caught");
            }
            catch (FormatException e) // ICU4N TODO: .NET BigInteger doesn't throw in this case
            {
                assertEquals("Improper exception message", "Zero length BigInteger", e.Message);
            }
        }

        /**
         * Create a positive number from an array of bytes.
         * The number fits in an array of integers.
         */
        [Test]
        public void testConstructorBytesPositive1()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            byte[] rBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            BigInteger aNumber = new BigInteger(aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * Create a positive number from an array of bytes.
         * The number fits in an integer.
         */
        [Test]
        public void testConstructorBytesPositive2()
        {
            byte[] aBytes = { 12, 56, 100 };
            byte[] rBytes = { 12, 56, 100 };
            BigInteger aNumber = new BigInteger(aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * Create a positive number from an array of bytes.
         * The number of bytes is 4.
         */
        [Test]
        public void testConstructorBytesPositive3()
        {
            byte[] aBytes = { 127, 56, 100, unchecked((byte)-1) };
            byte[] rBytes = { 127, 56, 100, unchecked((byte)-1) };
            BigInteger aNumber = new BigInteger(aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * Create a positive number from an array of bytes.
         * The number of bytes is multiple of 4.
         */
        [Test]
        public void testConstructorBytesPositive()
        {
            byte[] aBytes = { 127, 56, 100, unchecked((byte)-1), 14, 75, unchecked((byte)-24), unchecked((byte)-100) };
            byte[] rBytes = { 127, 56, 100, unchecked((byte)-1), 14, 75, unchecked((byte)-24), unchecked((byte)-100) };
            BigInteger aNumber = new BigInteger(aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * Create a negative number from an array of bytes.
         * The number fits in an array of integers.
         */
        [Test]
        public void testConstructorBytesNegative1()
        {
            byte[] aBytes = { unchecked((byte)-12), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            byte[] rBytes = { unchecked((byte)-12), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 35, 26, 3, 91 };
            BigInteger aNumber = new BigInteger(aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, aNumber.Sign);
        }

        /**
         * Create a negative number from an array of bytes.
         * The number fits in an integer.
         */
        [Test]
        public void testConstructorBytesNegative2()
        {
            byte[] aBytes = { unchecked((byte)-12), 56, 100 };
            byte[] rBytes = { unchecked((byte)-12), 56, 100 };
            BigInteger aNumber = new BigInteger(aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, aNumber.Sign);
        }

        /**
         * Create a negative number from an array of bytes.
         * The number of bytes is 4.
         */
        [Test]
        public void testConstructorBytesNegative3()
        {
            byte[] aBytes = { unchecked((byte)-128), unchecked((byte)-12), 56, 100 };
            byte[] rBytes = { unchecked((byte)-128), unchecked((byte)-12), 56, 100 };
            BigInteger aNumber = new BigInteger(aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, aNumber.Sign);
        }

        /**
         * Create a negative number from an array of bytes.
         * The number of bytes is multiple of 4.
         */
        [Test]
        public void testConstructorBytesNegative4()
        {
            byte[] aBytes = { unchecked((byte)-128), unchecked((byte)-12), 56, 100, unchecked((byte)-13), 56, 93, unchecked((byte)-78) };
            byte[] rBytes = { unchecked((byte)-128), unchecked((byte)-12), 56, 100, unchecked((byte)-13), 56, 93, unchecked((byte)-78) };
            BigInteger aNumber = new BigInteger(aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", -1, aNumber.Sign);
        }

        /**
         * Create a zero number from an array of zero bytes.
         */
        [Test]
        public void testConstructorBytesZero()
        {
            byte[] aBytes = { 0, 0, 0, unchecked((byte)-0), +0, 0, unchecked((byte)-0) };
            byte[] rBytes = { 0 };
            BigInteger aNumber = new BigInteger(aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, aNumber.Sign);
        }

        /**
         * Create a number from a sign and an array of bytes.
         * Verify an exception thrown if a sign has improper value.
         */
        [Test]
        public void testConstructorSignBytesException1()
        {
            byte[] aBytes = { 123, 45, unchecked((byte)-3), unchecked((byte)-76) };
            int aSign = 3;
            try
            {
                new BigInteger(aSign, aBytes);
                fail("NumberFormatException has not been caught");
            }
            catch (FormatException e) // ICU4N TODO: FormatExcpetion seems like the wrong type for this overload
            {
                assertEquals("Improper exception message", "Invalid signum value", e.Message);
            }
        }

        /**
         * Create a number from a sign and an array of bytes.
         * Verify an exception thrown if the array contains non-zero bytes while the sign is 0. 
         */
        [Test]
        public void testConstructorSignBytesException2()
        {
            byte[] aBytes = { 123, 45, unchecked((byte)-3), unchecked((byte)-76) };
            int aSign = 0;
            try
            {
                new BigInteger(aSign, aBytes);
                fail("NumberFormatException has not been caught");
            }
            catch (FormatException e) // ICU4N TODO: FormatExcpetion seems like the wrong type for this overload
            {
                assertEquals("Improper exception message", "signum-magnitude mismatch", e.Message);
            }
        }

        /**
         * Create a positive number from a sign and an array of bytes.
         * The number fits in an array of integers.
         * The most significant byte is positive.
         */
        [Test]
        public void testConstructorSignBytesPositive1()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15) };
            int aSign = 1;
            sbyte[] rBytes = { 12, 56, 100, -2, -76, 89, 45, 91, 3, -15 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * Create a positive number from a sign and an array of bytes.
         * The number fits in an array of integers.
         * The most significant byte is negative.
         */
        [Test]
        public void testConstructorSignBytesPositive2()
        {
            byte[] aBytes = { unchecked((byte)-12), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15) };
            int aSign = 1;
            sbyte[] rBytes = { 0, -12, 56, 100, -2, -76, 89, 45, 91, 3, -15 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * Create a positive number from a sign and an array of bytes.
         * The number fits in an integer.
         */
        [Test]
        public void testConstructorSignBytesPositive3()
        {
            byte[] aBytes = { unchecked((byte)-12), 56, 100 };
            int aSign = 1;
            sbyte[] rBytes = { 0, -12, 56, 100 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * Create a positive number from a sign and an array of bytes.
         * The number of bytes is 4.
         * The most significant byte is positive.
         */
        [Test]
        public void testConstructorSignBytesPositive4()
        {
            byte[] aBytes = { 127, 56, 100, unchecked((byte)-2) };
            int aSign = 1;
            sbyte[] rBytes = { 127, 56, 100, -2 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * Create a positive number from a sign and an array of bytes.
         * The number of bytes is 4.
         * The most significant byte is negative.
         */
        [Test]
        public void testConstructorSignBytesPositive5()
        {
            byte[] aBytes = { unchecked((byte)-127), 56, 100, unchecked((byte)-2) };
            int aSign = 1;
            sbyte[] rBytes = { 0, -127, 56, 100, -2 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * Create a positive number from a sign and an array of bytes.
         * The number of bytes is multiple of 4.
         * The most significant byte is positive.
         */
        [Test]
        public void testConstructorSignBytesPositive6()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 23, unchecked((byte)-101) };
            int aSign = 1;
            sbyte[] rBytes = { 12, 56, 100, -2, -76, 89, 45, 91, 3, -15, 23, -101 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * Create a positive number from a sign and an array of bytes.
         * The number of bytes is multiple of 4.
         * The most significant byte is negative.
         */
        [Test]
        public void testConstructorSignBytesPositive7()
        {
            byte[] aBytes = { unchecked((byte)-12), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 23, unchecked((byte)-101) };
            int aSign = 1;
            sbyte[] rBytes = { 0, -12, 56, 100, -2, -76, 89, 45, 91, 3, -15, 23, -101 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * Create a negative number from a sign and an array of bytes.
         * The number fits in an array of integers.
         * The most significant byte is positive.
         */
        [Test]
        public void testConstructorSignBytesNegative1()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15) };
            int aSign = -1;
            sbyte[] rBytes = { -13, -57, -101, 1, 75, -90, -46, -92, -4, 15 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", -1, aNumber.Sign);
        }

        /**
         * Create a negative number from a sign and an array of bytes.
         * The number fits in an array of integers.
         * The most significant byte is negative.
         */
        [Test]
        public void testConstructorSignBytesNegative2()
        {
            byte[] aBytes = { unchecked((byte)-12), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15) };
            int aSign = -1;
            sbyte[] rBytes = { -1, 11, -57, -101, 1, 75, -90, -46, -92, -4, 15 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", -1, aNumber.Sign);
        }

        /**
         * Create a negative number from a sign and an array of bytes.
         * The number fits in an integer.
         */
        [Test]
        public void testConstructorSignBytesNegative3()
        {
            byte[] aBytes = { unchecked((byte)-12), 56, 100 };
            int aSign = -1;
            sbyte[] rBytes = { -1, 11, -57, -100 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", -1, aNumber.Sign);
        }

        /**
         * Create a negative number from a sign and an array of bytes.
         * The number of bytes is 4.
         * The most significant byte is positive.
         */
        [Test]
        public void testConstructorSignBytesNegative4()
        {
            byte[] aBytes = { 127, 56, 100, unchecked((byte)-2) };
            int aSign = -1;
            sbyte[] rBytes = { -128, -57, -101, 2 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", -1, aNumber.Sign);
        }

        /**
         * Create a negative number from a sign and an array of bytes.
         * The number of bytes is 4.
         * The most significant byte is negative.
         */
        [Test]
        public void testConstructorSignBytesNegative5()
        {
            byte[] aBytes = { unchecked((byte)-127), 56, 100, unchecked((byte)-2) };
            int aSign = -1;
            sbyte[] rBytes = { -1, 126, -57, -101, 2 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", -1, aNumber.Sign);
        }

        /**
         * Create a negative number from a sign and an array of bytes.
         * The number of bytes is multiple of 4.
         * The most significant byte is positive.
         */
        [Test]
        public void testConstructorSignBytesNegative6()
        {
            byte[] aBytes = { 12, 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 23, unchecked((byte)-101) };
            int aSign = -1;
            sbyte[] rBytes = { -13, -57, -101, 1, 75, -90, -46, -92, -4, 14, -24, 101 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", -1, aNumber.Sign);
        }

        /**
         * Create a negative number from a sign and an array of bytes.
         * The number of bytes is multiple of 4.
         * The most significant byte is negative.
         */
        [Test]
        public void testConstructorSignBytesNegative7()
        {
            byte[] aBytes = { unchecked((byte)-12), 56, 100, unchecked((byte)-2), unchecked((byte)-76), 89, 45, 91, 3, unchecked((byte)-15), 23, unchecked((byte)-101) };
            int aSign = -1;
            sbyte[] rBytes = { -1, 11, -57, -101, 1, 75, -90, -46, -92, -4, 14, -24, 101 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", -1, aNumber.Sign);
        }

        /**
         * Create a zero number from a sign and an array of zero bytes.
         * The sign is -1.
         */
        [Test]
        public void testConstructorSignBytesZero1()
        {
            byte[] aBytes = { unchecked((byte)-0), 0, +0, 0, 0, 00, 000 };
            int aSign = -1;
            byte[] rBytes = { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, aNumber.Sign);
        }

        /**
         * Create a zero number from a sign and an array of zero bytes.
         * The sign is 0.
         */
        [Test]
        public void testConstructorSignBytesZero2()
        {
            byte[] aBytes = { unchecked((byte)-0), 0, +0, 0, 0, 00, 000 };
            int aSign = 0;
            byte[] rBytes = { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, aNumber.Sign);
        }

        /**
         * Create a zero number from a sign and an array of zero bytes.
         * The sign is 1.
         */
        [Test]
        public void testConstructorSignBytesZero3()
        {
            byte[] aBytes = { unchecked((byte)-0), 0, +0, 0, 0, 00, 000 };
            int aSign = 1;
            byte[] rBytes = { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, aNumber.Sign);
        }

        /**
         * Create a zero number from a sign and an array of zero length.
         * The sign is -1.
         */
        [Test]
        public void testConstructorSignBytesZeroNull1()
        {
            byte[] aBytes = { };
            int aSign = -1;
            byte[] rBytes = { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, aNumber.Sign);
        }

        /**
         * Create a zero number from a sign and an array of zero length.
         * The sign is 0.
         */
        [Test]
        public void testConstructorSignBytesZeroNull2()
        {
            byte[] aBytes = { };
            int aSign = 0;
            byte[] rBytes = { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, aNumber.Sign);
        }

        /**
         * Create a zero number from a sign and an array of zero length.
         * The sign is 1.
         */
        [Test]
        public void testConstructorSignBytesZeroNull3()
        {
            byte[] aBytes = { };
            int aSign = 1;
            byte[] rBytes = { 0 };
            BigInteger aNumber = new BigInteger(aSign, aBytes);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, aNumber.Sign);
        }

        /**
         * Create a number from a string value and radix.
         * Verify an exception thrown if a radix is out of range
         */
        [Test]
        public void testConstructorStringException1()
        {
            String value = "9234853876401";
            int radix = 45;
            try
            {
                BigInteger.Parse(value, radix);
                fail("NumberFormatException has not been caught");
            }
            catch (ArgumentOutOfRangeException e) // ICU4N: Using ArgumentOutOfRangeException to match .NET
            {
                assertTrue("Improper exception message", e.Message.Contains("Radix must be greater than or equal to Character.MinRadix and less than or equal to Character.MaxRadix."));
            }
        }

        /**
         * Create a number from a string value and radix.
         * Verify an exception thrown if the string starts with a space.
         */
        [Test]
        public void testConstructorStringException2()
        {
            String value = "   9234853876401";
            int radix = 10;
            try
            {
                BigInteger.Parse(value, radix);
                fail("NumberFormatException has not been caught");
            }
            catch (FormatException e)
            {
            }
        }

        /**
         * Create a number from a string value and radix.
         * Verify an exception thrown if the string contains improper characters.
         */
        [Test]
        public void testConstructorStringException3()
        {
            String value = "92348$*#78987";
            int radix = 34;
            try
            {
                BigInteger.Parse(value, radix);
                fail("NumberFormatException has not been caught");
            }
            catch (FormatException e)
            {
            }
        }

        /**
         * Create a number from a string value and radix.
         * Verify an exception thrown if some digits are greater than radix.
         */
        [Test]
        public void testConstructorStringException4()
        {
            String value = "98zv765hdsaiy";
            int radix = 20;
            try
            {
                BigInteger.Parse(value, radix);
                fail("NumberFormatException has not been caught");
            }
            catch (FormatException e)
            {
            }
        }

        /**
         * Create a positive number from a string value and radix 2.
         */
        [Test]
        public void testConstructorStringRadix2()
        {
            String value = "10101010101010101";
            int radix = 2;
            byte[] rBytes = { 1, 85, 85 };
            BigInteger aNumber = BigInteger.Parse(value, radix);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * Create a positive number from a string value and radix 8.
         */
        [Test]
        public void testConstructorStringRadix8()
        {
            String value = "76356237071623450";
            int radix = 8;
            sbyte[] rBytes = { 7, -50, -28, -8, -25, 39, 40 };
            BigInteger aNumber = BigInteger.Parse(value, radix);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * Create a positive number from a string value and radix 10.
         */
        [Test]
        public void testConstructorStringRadix10()
        {
            String value = "987328901348934898";
            int radix = 10;
            sbyte[] rBytes = { 13, -77, -78, 103, -103, 97, 68, -14 };
            BigInteger aNumber = BigInteger.Parse(value, radix);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * Create a positive number from a string value and radix 16.
         */
        [Test]
        public void testConstructorStringRadix16()
        {
            String value = "fe2340a8b5ce790";
            int radix = 16;
            sbyte[] rBytes = { 15, -30, 52, 10, -117, 92, -25, -112 };
            BigInteger aNumber = BigInteger.Parse(value, radix);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * Create a positive number from a string value and radix 36.
         */
        [Test]
        public void testConstructorStringRadix36()
        {
            String value = "skdjgocvhdjfkl20jndjkf347ejg457";
            int radix = 36;
            sbyte[] rBytes = { 0, -12, -116, 112, -105, 12, -36, 66, 108, 66, -20, -37, -15, 108, -7, 52, -99, -109, -8, -45, -5 };
            BigInteger aNumber = BigInteger.Parse(value, radix);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", 1, aNumber.Sign);
        }

        /**
         * Create a negative number from a string value and radix 10.
         */
        [Test]
        public void testConstructorStringRadix10Negative()
        {
            String value = "-234871376037";
            int radix = 36;
            sbyte[] rBytes = { -4, 48, 71, 62, -76, 93, -105, 13 };
            BigInteger aNumber = BigInteger.Parse(value, radix);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == (byte)rBytes[i]);
            }
            assertEquals("incorrect sign", -1, aNumber.Sign);
        }

        /**
         * Create a zero number from a string value and radix 36.
         */
        [Test]
        public void testConstructorStringRadix10Zero()
        {
            String value = "-00000000000000";
            int radix = 10;
            byte[] rBytes = { 0 };
            BigInteger aNumber = BigInteger.Parse(value, radix);
            byte[] resBytes;
            resBytes = aNumber.ToByteArray();
            for (int i = 0; i < resBytes.Length; i++)
            {
                assertTrue("incorrect value", resBytes[i] == rBytes[i]);
            }
            assertEquals("incorrect sign", 0, aNumber.Sign);
        }

        /**
         * Create a random number of 75 bits length.
         */
        [Test]
        public void testConstructorRandom()
        {
            int bitLen = 75;
            Random rnd = new Random();
            BigInteger aNumber = new BigInteger(bitLen, rnd);
            assertTrue("incorrect bitLength", aNumber.BitLength <= bitLen);
        }

        /**
         * Create a prime number of 25 bits length.
         */
        [Test]
        public void testConstructorPrime()
        {
            int bitLen = 25;
            Random rnd = new Random();
            BigInteger aNumber = new BigInteger(bitLen, 80, rnd);
            assertTrue("incorrect bitLength", aNumber.BitLength == bitLen);
        }

        /**
         * Create a prime number of 2 bits length.
         */
        [Test]
        public void testConstructorPrime2()
        {
            int bitLen = 2;
            Random rnd = new Random();
            BigInteger aNumber = new BigInteger(bitLen, 80, rnd);
            assertTrue("incorrect bitLength", aNumber.BitLength == bitLen);
            int num = aNumber.ToInt32();
            assertTrue("incorrect value", num == 2 || num == 3);
        }
    }
}
