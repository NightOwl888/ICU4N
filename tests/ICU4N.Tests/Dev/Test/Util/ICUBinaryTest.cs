using ICU4N.Impl;
using J2N.IO;
using NUnit.Framework;
using System.IO;

namespace ICU4N.Dev.Test.Util
{
    /// <summary>
    /// Testing class for Trie. Tests here will be simple, since both CharTrie and
    /// IntTrie are very similar and are heavily used in other parts of ICU4J.
    /// Codes using Tries are expected to have detailed tests.
    /// </summary>
    /// <author>Syn Wee Quek</author>
    /// <since>release 2.1 Jan 01 2002</since>
    public sealed class ICUBinaryTest : TestFmwk
    {
        // constructor ---------------------------------------------------

        /**
        * Constructor
        */
        public ICUBinaryTest()
        {
        }

        private class Authenticate : IAuthenticate
        {
            public bool IsDataVersionAcceptable(byte[] version)
            {
                return version[0] == 1;
            }
        }

        // public methods -----------------------------------------------

        /**
         * Testing the constructors of the Tries
         */
        [Test]
        public void TestReadHeader()
        {
            int formatid = 0x01020304;
            byte[] array = {
            // header size
            0, 0x18,
            // magic numbers
            (byte)0xda, 0x27,
            // size
            0, 0x14,
            // reserved word
            0, 0,
            // bigendian
            1,
            // charset
            0,
            // charsize
            2,
            // reserved byte
            0,
            // data format id
            1, 2, 3, 4,
            // dataVersion
            1, 2, 3, 4,
            // unicodeVersion
            3, 2, 0, 0
        };
            ByteBuffer bytes = ByteBuffer.Wrap(array);
            IAuthenticate authenticate
                    = new Authenticate();
            // check full data version
            try
            {
                ICUBinary.ReadHeader(bytes, formatid, authenticate);
            }
            catch (IOException e)
            {
                Errln("Failed: Lenient authenticate object should pass ICUBinary.readHeader");
            }
            // no restriction to the data version
            try
            {
                bytes.Rewind();
                ICUBinary.ReadHeader(bytes, formatid, null);
            }
            catch (IOException e)
            {
                Errln("Failed: Null authenticate object should pass ICUBinary.readHeader");
            }
            // lenient data version
            array[17] = 9;
            try
            {
                bytes.Rewind();
                ICUBinary.ReadHeader(bytes, formatid, authenticate);
            }
            catch (IOException e)
            {
                Errln("Failed: Lenient authenticate object should pass ICUBinary.readHeader");
            }
            // changing the version to an incorrect one, expecting failure
            array[16] = 2;
            try
            {
                bytes.Rewind();
                ICUBinary.ReadHeader(bytes, formatid, authenticate);
                Errln("Failed: Invalid version number should not pass authenticate object");
            }
            catch (IOException e)
            {
                Logln("PASS: ICUBinary.readHeader with invalid version number failed as expected");
            }
        }
    }
}
