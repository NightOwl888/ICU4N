using ICU4N.Text;
using J2N.Collections;
using J2N.IO;
using System.IO;
using System.Text;
#nullable enable

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

        /// <summary>
        /// Protected constructor.
        /// </summary>
        /// <param name="bytes">ICU uprop.dat file buffer.</param>
        /// <exception cref="IOException">If data file fails authentication.</exception>
        internal UCharacterNameReader(ByteBuffer bytes)
        {
            ICUBinary.ReadHeader(bytes, DATA_FORMAT_ID_, this);
            m_byteBuffer_ = bytes;
        }

        // protected methods -------------------------------------------------

        /// <summary>
        /// Read and break up the stream of data passed in as arguments
        /// and fills up <see cref="UCharacterName"/>.
        /// If unsuccessful false will be returned.
        /// </summary>
        /// <param name="data">Instance of datablock.</param>
        /// <exception cref="IOException">Thrown when there's a data error.</exception>
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
                UCharacterName.AlgorithmName? an = ReadAlg();
                if (an == null)
                {
                    throw new IOException("unames.icu read error: Algorithmic names creation error");
                }
                alg[i] = an;
            }
            data.SetAlgorithm(alg);
        }

        /// <summary>
        /// Checking the file for the correct format.
        /// </summary>
        /// <param name="dataformatid"></param>
        /// <param name="dataformatversion"></param>
        /// <returns>true if the file format version is correct.</returns>
        //CLOVER:OFF
        private bool Authenticate(byte[] dataformatid,
                                       byte[] dataformatversion)
        {
            return ArrayEqualityComparer<byte>.OneDimensional.Equals(
                    ICUBinary.GetVersionByteArrayFromCompactInt32(DATA_FORMAT_ID_),
                    dataformatid) &&
                   IsDataVersionAcceptable(dataformatversion);
        }
        //CLOVER:ON

        // private variables -------------------------------------------------

        /// <summary>
        /// Byte buffer for names
        /// </summary>
        private ByteBuffer m_byteBuffer_;
        /// <summary>
        /// Size of the group information block in number of char
        /// </summary>
        private const int GROUP_INFO_SIZE_ = 3;

        /// <summary>
        /// Index of the offset information
        /// </summary>
        private int m_tokenstringindex_;
        private int m_groupindex_;
        private int m_groupstringindex_;
        private int m_algnamesindex_;

        /// <summary>
        /// Size of an algorithmic name information group
        /// start code point size + end code point size + type size + variant size +
        /// size of data size
        /// </summary>
        private const int ALG_INFO_SIZE_ = 12;

        /// <summary>
        /// File format id that this class understands.
        /// </summary>
        private const int DATA_FORMAT_ID_ = 0x756E616D;

        private const int CharStackBufferSize = 32;

        // private methods ---------------------------------------------------

        /// <summary>
        /// Reads an individual record of <see cref="UCharacterName.AlgorithmName"/>s
        /// </summary>
        /// <returns>An instance of <see cref="UCharacterName.AlgorithmName"/>s if read is successful otherwise null.</returns>
        /// <exception cref="IOException">Thrown when file read error occurs or data is corrupted.</exception>
        private UCharacterName.AlgorithmName? ReadAlg()
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

            ValueStringBuilder prefix = new ValueStringBuilder(stackalloc char[CharStackBufferSize]);
            try
            {
                char c = (char)(m_byteBuffer_.Get() & 0x00FF);
                while (c != 0)
                {
                    prefix.Append(c);
                    c = (char)(m_byteBuffer_.Get() & 0x00FF);
                }

                size -= (ALG_INFO_SIZE_ + prefix.Length + 1);

                result.SetPrefix(prefix.ToString());
            }
            finally
            {
                prefix.Dispose();
            }

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
