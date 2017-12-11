using ICU4N.Support.Collections;
using ICU4N.Support.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ICU4N.Impl
{
    /// <summary>
    /// Internal reader class for ICU data file uname.dat containing
    /// Unicode codepoint name data.
    /// </summary>
    /// <remarks>
    /// This class simply reads unames.icu, authenticates that it is a valid
    /// ICU data file and split its contents up into blocks of data for use in
    /// <see cref="UCharacterName"/>.
    /// <para/>
    /// unames.icu which is in big-endian format is jared together with this
    /// package.
    /// </remarks>
    /// <author>Syn Wee Quek</author>
    /// <since>release 2.1, February 1st 2002</since>
    internal sealed class UCharacterNameReader : IAuthenticate
    {
        // public methods ----------------------------------------------------

        public bool IsDataVersionAcceptable(byte[] version)
        {
            return version[0] == 1;
        }

        // protected constructor ---------------------------------------------

        /**
        * <p>Protected constructor.</p>
        * @param bytes ICU uprop.dat file buffer
        * @exception IOException throw if data file fails authentication
        */
        internal UCharacterNameReader(ByteBuffer bytes)
        {
            ICUBinary.ReadHeader(bytes, DATA_FORMAT_ID_, this);
            m_byteBuffer_ = bytes;
        }

        // protected methods -------------------------------------------------

        /**
        * Read and break up the stream of data passed in as arguments
        * and fills up UCharacterName.
        * If unsuccessful false will be returned.
        * @param data instance of datablock
        * @exception IOException thrown when there's a data error.
        */
        internal void Read(UCharacterName data)
        {
            // reading index
            m_tokenstringindex_ = m_byteBuffer_.GetInt32();
            m_groupindex_ = m_byteBuffer_.GetInt32();
            m_groupstringindex_ = m_byteBuffer_.GetInt32();
            m_algnamesindex_ = m_byteBuffer_.GetInt32();

            // reading tokens
            int count = m_byteBuffer_.GetChar();
            char[] token = ICUBinary.GetChars(m_byteBuffer_, count, 0);
            int size = m_groupindex_ - m_tokenstringindex_;
            byte[] tokenstr = new byte[size];
            m_byteBuffer_.Get(tokenstr);
            data.SetToken(token, tokenstr);

            // reading the group information records
            count = m_byteBuffer_.GetChar();
            data.SetGroupCountSize(count, GROUP_INFO_SIZE_);
            count *= GROUP_INFO_SIZE_;
            char[] group = ICUBinary.GetChars(m_byteBuffer_, count, 0);

            size = m_algnamesindex_ - m_groupstringindex_;
            byte[] groupstring = new byte[size];
            m_byteBuffer_.Get(groupstring);

            data.SetGroup(group, groupstring);

            count = m_byteBuffer_.GetInt32();
            UCharacterName.AlgorithmName[] alg =
                                     new UCharacterName.AlgorithmName[count];

            for (int i = 0; i < count; i++)
            {
                UCharacterName.AlgorithmName an = ReadAlg();
                if (an == null)
                {
                    throw new IOException("unames.icu read error: Algorithmic names creation error");
                }
                alg[i] = an;
            }
            data.SetAlgorithm(alg);
        }

        /**
        * <p>Checking the file for the correct format.</p>
        * @param dataformatid
        * @param dataformatversion
        * @return true if the file format version is correct
        */
        ///CLOVER:OFF
        protected bool Authenticate(byte[] dataformatid,
                                       byte[] dataformatversion)
        {
            return Arrays.Equals(
                    ICUBinary.GetVersionByteArrayFromCompactInt(DATA_FORMAT_ID_),
                    dataformatid) &&
                   IsDataVersionAcceptable(dataformatversion);
        }
        ///CLOVER:ON

        // private variables -------------------------------------------------

        /**
        * Byte buffer for names
*/
        private ByteBuffer m_byteBuffer_;
        /**
        * Size of the group information block in number of char
*/
        private static readonly int GROUP_INFO_SIZE_ = 3;

        /**
        * Index of the offset information
*/
        private int m_tokenstringindex_;
        private int m_groupindex_;
        private int m_groupstringindex_;
        private int m_algnamesindex_;

        /**
        * Size of an algorithmic name information group
        * start code point size + end code point size + type size + variant size +
        * size of data size
*/
        private static readonly int ALG_INFO_SIZE_ = 12;

        /**
        * File format id that this class understands.
*/
        private static readonly int DATA_FORMAT_ID_ = 0x756E616D;

        // private methods ---------------------------------------------------

        /**
        * Reads an individual record of AlgorithmNames
        * @return an instance of AlgorithNames if read is successful otherwise null
        * @exception IOException thrown when file read error occurs or data is corrupted
*/
        private UCharacterName.AlgorithmName ReadAlg()
        {
            UCharacterName.AlgorithmName result =
                                               new UCharacterName.AlgorithmName();
            int rangestart = m_byteBuffer_.GetInt32();
            int rangeend = m_byteBuffer_.GetInt32();
            byte type = m_byteBuffer_.Get();
            byte variant = m_byteBuffer_.Get();
            if (!result.SetInfo(rangestart, rangeend, type, variant))
            {
                return null;
            }

            int size = m_byteBuffer_.GetChar();
            if (type == UCharacterName.AlgorithmName.TYPE_1_)
            {
                char[] factor = ICUBinary.GetChars(m_byteBuffer_, variant, 0);

                result.SetFactor(factor);
                size -= (variant << 1);
            }

            StringBuilder prefix = new StringBuilder();
            char c = (char)(m_byteBuffer_.Get() & 0x00FF);
            while (c != 0)
            {
                prefix.Append(c);
                c = (char)(m_byteBuffer_.Get() & 0x00FF);
            }

            result.SetPrefix(prefix.ToString());

            size -= (ALG_INFO_SIZE_ + prefix.Length + 1);

            if (size > 0)
            {
                byte[] str = new byte[size];
                m_byteBuffer_.Get(str);
                result.SetFactorString(str);
            }
            return result;
        }
    }
}
