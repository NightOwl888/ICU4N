using ICU4N.Globalization;
using ICU4N.Support;
using ICU4N.Support.IO;
using ICU4N.Support.Text;
using ICU4N.Text;
using System;
using System.Globalization;
using System.IO;
using System.Resources;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Impl
{
    /// <summary>
    /// Internal class to manage character names.
    /// Since data for names are stored
    /// in an array of <see cref="char"/>, by default indexes used in this class is refering to
    /// a 2 byte count, unless otherwise stated. Cases where the index is refering
    /// to a byte count, the index is halved and depending on whether the index is
    /// even or odd, the MSB or LSB of the result char at the halved index is
    /// returned. For indexes to an array of <see cref="int"/>, the index is multiplied by 2,
    /// result <see cref="char"/> at the multiplied index and its following <see cref="char"/> is returned as an
    /// <see cref="int"/>.
    /// </summary>
    public sealed class UCharacterName
    {
        // public data members ----------------------------------------------

        private static readonly UCharacterName instance;

        /// <summary>
        /// Public singleton instance.
        /// </summary>
        public static UCharacterName Instance
        {
            get { return instance; }
        }

        static UCharacterName()
        {
            try
            {
                instance = new UCharacterName();
            }
            catch (IOException e)
            {
                //CLOVER:OFF
                throw new MissingManifestResourceException("Could not construct UCharacterName. Missing unames.icu", e/*, "", ""*/);
                //CLOVER:ON
            }
        }

        /// <summary>
        /// Number of lines per group
        /// 1 &lt;&lt; GROUP_SHIFT_
        /// </summary>
        public static readonly int LINES_PER_GROUP_ = 1 << 5;
        /// <summary>
        /// Maximum number of groups.
        /// </summary>
        internal int m_groupcount_ = 0;

        public int GroupCount { get { return m_groupcount_; } }

        // public methods ---------------------------------------------------

        /// <summary>
        /// Retrieve the name of a Unicode code point.
        /// Depending on <paramref name="choice"/>, the character name written into the
        /// buffer is the "modern" name or the name that was defined in Unicode
        /// version 1.0.
        /// The name contains only "invariant" characters
        /// like A-Z, 0-9, space, and '-'.
        /// </summary>
        /// <param name="ch">The code point for which to get the name.</param>
        /// <param name="choice">Selector for which name to get.</param>
        /// <returns>If code point is above 0x1fff, null is returned.</returns>
        public string GetName(int ch, UCharacterNameChoice choice)
        {
            if (ch < UChar.MinValue || ch > UChar.MaxValue ||
                choice > UCharacterNameChoice.CharNameChoiceCount)
            {
                return null;
            }

            string result = null;

            result = GetAlgName(ch, choice);

            // getting normal character name
            if (result == null || result.Length == 0)
            {
                if (choice == UCharacterNameChoice.ExtendedCharName)
                {
                    result = GetExtendedName(ch);
                }
                else
                {
                    result = GetGroupName(ch, choice);
                }
            }

            return result;
        }

        /// <summary>
        /// Find a character by its name and return its code point value
        /// 
        /// </summary>
        /// <param name="choice">Selector to indicate if argument name is a Unicode 1.0 or the most current version.</param>
        /// <param name="name">The name to search for.</param>
        /// <returns>Code point.</returns>
        public int GetCharFromName(UCharacterNameChoice choice, string name)
        {
            // checks for illegal arguments
            if ((int)choice >= (int)UCharacterNameChoice.CharNameChoiceCount ||
                name == null || name.Length == 0)
            {
                return -1;
            }

            // try extended names first
            int result = GetExtendedChar(name.ToLowerInvariant(), choice);
            if (result >= -1)
            {
                return result;
            }

            string upperCaseName = name.ToUpperInvariant();
            // try algorithmic names first, if fails then try group names
            // int result = getAlgorithmChar(choice, uppercasename);

            if (choice == UCharacterNameChoice.UnicodeCharName ||
                choice == UCharacterNameChoice.ExtendedCharName
            )
            {
                int count = 0;
                if (m_algorithm_ != null)
                {
                    count = m_algorithm_.Length;
                }
                for (count--; count >= 0; count--)
                {
                    result = m_algorithm_[count].GetChar(upperCaseName);
                    if (result >= 0)
                    {
                        return result;
                    }
                }
            }

            if (choice == UCharacterNameChoice.ExtendedCharName)
            {
                result = GetGroupChar(upperCaseName,
                                      UCharacterNameChoice.UnicodeCharName);
                if (result == -1)
                {
                    result = GetGroupChar(upperCaseName,
                                          UCharacterNameChoice.CharNameAlias);
                }
            }
            else
            {
                result = GetGroupChar(upperCaseName, choice);
            }
            return result;
        }

        // these are all UCharacterNameIterator use methods -------------------

        /// <summary>
        /// Reads a block of compressed lengths of 32 strings and expands them into
        /// offsets and lengths for each string. 
        /// </summary>
        /// <remarks>
        /// Lengths are stored with a
        /// variable-width encoding in consecutive nibbles:
        /// <list type="bullet">
        ///     <item><description>If a nibble&lt;0xc, then it is the length itself (0 = empty string).</description></item>
        ///     <item><description>If a nibble>=0xc, then it forms a length value with the following nibble.</description></item>
        /// </list>
        /// The offsets and lengths arrays must be at least 33 (one more) long
        /// because there is no check here at the end if the last nibble is still
        /// used.
        /// </remarks>
        /// <param name="index">Index of group string object in array.</param>
        /// <param name="offsets">Array to store the value of the string offsets.</param>
        /// <param name="lengths">Array to store the value of the string length.</param>
        /// <returns>Next index of the data string immediately after the lengths
        /// in terms of byte address.</returns>
        public int GetGroupLengths(int index, char[] offsets, char[] lengths)
        {
            char length = (char)0xffff;
            byte b = 0,
                n = 0;
            int shift;
            index = index * m_groupsize_; // byte count offsets of group strings
            int stringoffset = UCharacterUtility.ToInt(
                                     m_groupinfo_[index + OFFSET_HIGH_OFFSET_],
                                     m_groupinfo_[index + OFFSET_LOW_OFFSET_]);

            offsets[0] = (char)0;

            // all 32 lengths must be read to get the offset of the first group
            // string
            for (int i = 0; i < LINES_PER_GROUP_; stringoffset++)
            {
                b = m_groupstring_[stringoffset];
                shift = 4;

                while (shift >= 0)
                {
                    // getting nibble
                    n = (byte)((b >> shift) & 0x0F);
                    if (length == 0xffff && n > SINGLE_NIBBLE_MAX_)
                    {
                        length = (char)((n - 12) << 4);
                    }
                    else
                    {
                        if (length != 0xffff)
                        {
                            lengths[i] = (char)((length | n) + 12);
                        }
                        else
                        {
                            lengths[i] = (char)n;
                        }

                        if (i < LINES_PER_GROUP_)
                        {
                            offsets[i + 1] = (char)(offsets[i] + lengths[i]);
                        }

                        length = (char)0xffff;
                        i++;
                    }

                    shift -= 4;
                }
            }
            return stringoffset;
        }

        /// <summary>
        /// Gets the name of the argument group index.
        /// </summary>
        /// <remarks>
        /// UnicodeData.txt uses ';' as a field separator, so no field can contain
        /// ';' as part of its contents. In unames.icu, it is marked as
        /// token[';'] == -1 only if the semicolon is used in the data file - which
        /// is iff we have Unicode 1.0 names or ISO comments or aliases.
        /// So, it will be token[';'] == -1 if we store U1.0 names/ISO comments/aliases
        /// although we know that it will never be part of a name.
        /// Equivalent to ICU4C's expandName.
        /// </remarks>
        /// <param name="index">Index of the group name string in byte count.</param>
        /// <param name="length">Length of the group name string.</param>
        /// <param name="choice">Choice of Unicode 1.0 name or the most current name.</param>
        /// <returns>Name of the group.</returns>
        public string GetGroupName(int index, int length, UCharacterNameChoice choice)
        {
            if (choice != UCharacterNameChoice.UnicodeCharName &&
                choice != UCharacterNameChoice.ExtendedCharName
            )
            {
                if (';' >= m_tokentable_.Length || m_tokentable_[';'] == 0xFFFF)
                {
                    /*
                     * skip the modern name if it is not requested _and_
                     * if the semicolon byte value is a character, not a token number
                     */
                    int fieldIndex = choice == UCharacterNameChoice.IsoComment ? 2 : (int)choice;
                    do
                    {
                        int oldindex = index;
                        index += UCharacterUtility.SkipByteSubString(m_groupstring_,
                                                           index, length, (byte)';');
                        length -= (index - oldindex);
                    } while (--fieldIndex > 0);
                }
                else
                {
                    // the semicolon byte is a token number, therefore only modern
                    // names are stored in unames.dat and there is no such
                    // requested alternate name here
                    length = 0;
                }
            }

            lock (m_utilStringBuffer_)
            {
                m_utilStringBuffer_.Length = 0;
                byte b;
                char token;
                for (int i = 0; i < length;)
                {
                    b = m_groupstring_[index + i];
                    i++;

                    if (b >= m_tokentable_.Length)
                    {
                        if (b == ';')
                        {
                            break;
                        }
                        m_utilStringBuffer_.Append(b); // implicit letter
                    }
                    else
                    {
                        token = m_tokentable_[b & 0x00ff];
                        if (token == 0xFFFE)
                        {
                            // this is a lead byte for a double-byte token
                            token = m_tokentable_[b << 8 |
                                              (m_groupstring_[index + i] & 0x00ff)];
                            i++;
                        }
                        if (token == 0xFFFF)
                        {
                            if (b == ';')
                            {
                                // skip the semicolon if we are seeking extended
                                // names and there was no 2.0 name but there
                                // is a 1.0 name.
                                if (m_utilStringBuffer_.Length == 0 && choice ==
                                       UCharacterNameChoice.ExtendedCharName)
                                {
                                    continue;
                                }
                                break;
                            }
                            // explicit letter
                            m_utilStringBuffer_.Append((char)(b & 0x00ff));
                        }
                        else
                        { // write token word
                            UCharacterUtility.GetNullTermByteSubString(
                                    m_utilStringBuffer_, m_tokenstring_, token);
                        }
                    }
                }

                if (m_utilStringBuffer_.Length > 0)
                {
                    return m_utilStringBuffer_.ToString();
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieves the extended name.
        /// </summary>
        public string GetExtendedName(int ch)
        {
            string result = GetName(ch, UCharacterNameChoice.UnicodeCharName);
            if (result == null)
            {
                // TODO: Return Name_Alias/control names for control codes 0..1F & 7F..9F.
                result = GetExtendedOr10Name(ch);
            }
            return result;
        }

        /// <summary>
        /// Gets the group index for the codepoint, or the group before it.
        /// </summary>
        /// <param name="codepoint">The codepoint index.</param>
        /// <returns>Group index containing codepoint or the group before it.</returns>
        public int GetGroup(int codepoint)
        {
            int endGroup = m_groupcount_;
            int msb = GetCodepointMSB(codepoint);
            int result = 0;
            // binary search for the group of names that contains the one for
            // code
            // find the group that contains codepoint, or the highest before it
            while (result < endGroup - 1)
            {
                int gindex = (result + endGroup) >> 1;
                if (msb < GetGroupMSB(gindex))
                {
                    endGroup = gindex;
                }
                else
                {
                    result = gindex;
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the extended and 1.0 name when the most current unicode names
        /// fail.
        /// </summary>
        /// <param name="ch">Codepoint.</param>
        /// <returns>Name of codepoint extended or 1.0.</returns>
        public string GetExtendedOr10Name(int ch)
        {
            string result = null;
            // TODO: Return Name_Alias/control names for control codes 0..1F & 7F..9F.
            if (result == null)
            {
                int type = GetType(ch);
                // Return unknown if the table of names above is not up to
                // date.
                if (type >= TYPE_NAMES_.Length)
                {
                    result = UNKNOWN_TYPE_NAME_;
                }
                else
                {
                    result = TYPE_NAMES_[type];
                }
                lock (m_utilStringBuffer_)
                {
                    m_utilStringBuffer_.Length = 0;
                    m_utilStringBuffer_.Append('<');
                    m_utilStringBuffer_.Append(result);
                    m_utilStringBuffer_.Append('-');
                    //string chStr = Integer.toHexString(ch).toUpperCase(Locale.ENGLISH);
                    string chStr = string.Format(CultureInfo.InvariantCulture, "{0:X2}", ch);
                    int zeros = 4 - chStr.Length;
                    while (zeros > 0)
                    {
                        m_utilStringBuffer_.Append('0');
                        zeros--;
                    }
                    m_utilStringBuffer_.Append(chStr);
                    m_utilStringBuffer_.Append('>');
                    result = m_utilStringBuffer_.ToString();
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the MSB from the group index.
        /// </summary>
        /// <param name="gindex">Group index.</param>
        /// <returns>The MSB of the group if gindex is valid, -1 otherwise.</returns>
        public int GetGroupMSB(int gindex)
        {
            if (gindex >= m_groupcount_)
            {
                return -1;
            }
            return m_groupinfo_[gindex * m_groupsize_];
        }

        /// <summary>
        /// Gets the MSB of the codepoint.
        /// </summary>
        /// <param name="codepoint">The codepoint value.</param>
        /// <returns>The MSB of the codepoint.</returns>
        public static int GetCodepointMSB(int codepoint)
        {
            return codepoint >> GROUP_SHIFT_;
        }

        /// <summary>
        /// Gets the maximum codepoint + 1 of the group.
        /// </summary>
        /// <param name="msb">Most significant byte of the group.</param>
        /// <returns>Limit codepoint of the group.</returns>
        public static int GetGroupLimit(int msb)
        {
            return (msb << GROUP_SHIFT_) + LINES_PER_GROUP_;
        }

        /// <summary>
        /// Gets the minimum codepoint of the group.
        /// </summary>
        /// <param name="msb">Most significant byte of the group.</param>
        /// <returns>Minimum codepoint of the group.</returns>
        public static int GetGroupMin(int msb)
        {
            return msb << GROUP_SHIFT_;
        }

        /// <summary>
        /// Gets the offset to a group.
        /// </summary>
        /// <param name="codepoint">The codepoint value.</param>
        /// <returns>Offset to a group.</returns>
        public static int GetGroupOffset(int codepoint)
        {
            return codepoint & GROUP_MASK_;
        }

        /// <summary>
        /// Gets the minimum codepoint of a group.
        /// </summary>
        /// <param name="codepoint">The codepoint value.</param>
        /// <returns>Minimum codepoint in the group which codepoint belongs to.</returns>
        //CLOVER:OFF
        public static int GetGroupMinFromCodepoint(int codepoint)
        {
            return codepoint & ~GROUP_MASK_;
        }
        //CLOVER:ON

        /// <summary>
        /// Gets the Algorithm range length.
        /// </summary>
        public int AlgorithmLength
        {
            get { return m_algorithm_.Length; }
        }

        /// <summary>
        /// Gets the start of the range.
        /// </summary>
        /// <param name="index">Algorithm index.</param>
        /// <returns>Algorithm range start.</returns>
        public int GetAlgorithmStart(int index)
        {
            return m_algorithm_[index].m_rangestart_;
        }

        /// <summary>
        /// Gets the end of the range.
        /// </summary>
        /// <param name="index">Algorithm index.</param>
        /// <returns>Algorithm range end.</returns>
        public int GetAlgorithmEnd(int index)
        {
            return m_algorithm_[index].m_rangeend_;
        }

        /// <summary>
        /// Gets the Algorithmic name of the codepoint.
        /// </summary>
        /// <param name="index">Algorithmic range index.</param>
        /// <param name="codepoint">The codepoint value.</param>
        /// <returns>Algorithmic name of codepoint.</returns>
        public string GetAlgorithmName(int index, int codepoint)
        {
            string result = null;
            lock (m_utilStringBuffer_)
            {
                m_utilStringBuffer_.Length = 0;
                m_algorithm_[index].AppendName(codepoint, m_utilStringBuffer_);
                result = m_utilStringBuffer_.ToString();
            }
            return result;
        }

        /// <summary>
        /// Gets the group name of the character.
        /// </summary>
        /// <param name="ch">Character to get the group name.</param>
        /// <param name="choice"></param>
        /// <returns>Choice name choice selector to choose a unicode 1.0 or newer name.</returns>
        public string GetGroupName(int ch, UCharacterNameChoice choice)
        {
            lock (this)
            {
                // gets the msb
                int msb = GetCodepointMSB(ch);
                int group = GetGroup(ch);

                // return this if it is an exact match
                if (msb == m_groupinfo_[group * m_groupsize_])
                {
                    int index = GetGroupLengths(group, m_groupoffsets_,
                                                m_grouplengths_);
                    int offset = ch & GROUP_MASK_;
                    return GetGroupName(index + m_groupoffsets_[offset],
                                        m_grouplengths_[offset], choice);
                }

                return null;
            }
        }

        // these are transliterator use methods ---------------------------------

        /// <summary>
        /// Gets the maximum length of any codepoint name.
        /// Equivalent to uprv_getMaxCharNameLength.
        /// </summary>
        /// <returns>The maximum length of any codepoint name.</returns>
        public int MaxCharNameLength
        {
            get
            {
                if (InitNameSetsLengths())
                {
                    return m_maxNameLength_;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Gets the maximum length of any iso comments.
        /// Equivalent to uprv_getMaxISOCommentLength.
        /// </summary>
        /// <returns>The maximum length of any codepoint name.</returns>
        //CLOVER:OFF
        public int MaxISOCommentLength
        {
            get
            {
                if (InitNameSetsLengths())
                {
                    return m_maxISOCommentLength_;
                }
                else
                {
                    return 0;
                }
            }
        }
        //CLOVER:ON

        /// <summary>
        /// Fills set with characters that are used in Unicode character names.
        /// Equivalent to uprv_getCharNameCharacters.
        /// </summary>
        /// <param name="set">USet to receive characters. Existing contents are deleted.</param>
        public void GetCharNameCharacters(UnicodeSet set)
        {
            Convert(m_nameSet_, set);
        }

        /// <summary>
        /// Fills set with characters that are used in Unicode character names.
        /// Equivalent to uprv_getISOCommentCharacters.
        /// </summary>
        /// <param name="set">USet to receive characters. Existing contents are deleted.</param>
        //CLOVER:OFF
        public void GetISOCommentCharacters(UnicodeSet set)
        {
            Convert(m_ISOCommentSet_, set);
        }
        //CLOVER:ON

        // package private inner class --------------------------------------

        /// <summary>
        /// Algorithmic name class.
        /// </summary>
        internal sealed class AlgorithmName
        {
            // package private data members ----------------------------------

            /// <summary>
            /// Constant type value of the different AlgorithmName
            /// </summary>
            internal const int TYPE_0_ = 0;
            internal const int TYPE_1_ = 1;

            // package private constructors ----------------------------------

            /// <summary>
            /// Constructor.
            /// </summary>
            internal AlgorithmName()
            {
            }

            // package private methods ---------------------------------------

            /// <summary>
            /// Sets the information for accessing the algorithmic names/
            /// </summary>
            /// <param name="rangestart">Starting code point that lies within this name group.</param>
            /// <param name="rangeend">End code point that lies within this name group.</param>
            /// <param name="type">algorithm type. There's 2 kinds of algorithmic type. First
            /// which uses code point as part of its name and the other uses
            /// variant postfix strings.</param>
            /// <param name="variant">Algorithmic variant.</param>
            /// <returns>true if values are valid.</returns>
            internal bool SetInfo(int rangestart, int rangeend, byte type, byte variant)
            {
                if (rangestart >= UChar.MinValue && rangestart <= rangeend
                    && rangeend <= UChar.MaxValue &&
                    (type == TYPE_0_ || type == TYPE_1_))
                {
                    m_rangestart_ = rangestart;
                    m_rangeend_ = rangeend;
                    m_type_ = type;
                    m_variant_ = variant;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Sets the factor data.
            /// </summary>
            /// <param name="factor">Array of factor.</param>
            /// <returns>true if factors are valid.</returns>
            internal bool SetFactor(char[] factor)
            {
                if (factor.Length == m_variant_)
                {
                    m_factor_ = factor;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Sets the name prefix.
            /// </summary>
            /// <param name="prefix"></param>
            /// <returns>true if prefix is set.</returns>
            internal bool SetPrefix(string prefix)
            {
                if (prefix != null && prefix.Length > 0)
                {
                    m_prefix_ = prefix;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Sets the variant factorized name data.
            /// </summary>
            /// <param name="str">Variant factorized name data.</param>
            /// <returns>true if values are set.</returns>
            internal bool SetFactorString(byte[] str)
            {
                // factor and variant string can be empty for things like
                // hanggul code points
                m_factorstring_ = str;
                return true;
            }

            /// <summary>
            /// Checks if code point lies in Algorithm object at index.
            /// </summary>
            /// <param name="ch">Code point.</param>
            internal bool Contains(int ch)
            {
                return m_rangestart_ <= ch && ch <= m_rangeend_;
            }

            /// <summary>
            /// Appends algorithm name of code point into <see cref="StringBuffer"/>.
            /// Note this method does not check for validity of code point in Algorithm,
            /// result is undefined if code point does not belong in Algorithm.
            /// </summary>
            /// <param name="ch">Code point.</param>
            /// <param name="str"><see cref="StringBuffer"/> to append to.</param>
            internal void AppendName(int ch, StringBuffer str)
            {
                str.Append(m_prefix_);
                switch (m_type_)
                {
                    case TYPE_0_:
                        // prefix followed by hex digits indicating variants
                        str.Append(Utility.Hex(ch, m_variant_));
                        break;
                    case TYPE_1_:
                        // prefix followed by factorized-elements
                        int offset = ch - m_rangestart_;
                        int[] indexes = m_utilIntBuffer_;
                        int factor;

                        // write elements according to the factors
                        // the factorized elements are determined by modulo
                        // arithmetic
                        lock (m_utilIntBuffer_)
                        {
                            for (int i = m_variant_ - 1; i > 0; i--)
                            {
                                factor = m_factor_[i] & 0x00FF;
                                indexes[i] = offset % factor;
                                offset /= factor;
                            }

                            // we don't need to calculate the last modulus because
                            // start <= code <= end guarantees here that
                            // code <= factors[0]
                            indexes[0] = offset;

                            // joining up the factorized strings
                            str.Append(GetFactorString(indexes, m_variant_));
                        }
                        break;
                }
            }

            /// <summary>
            /// Gets the character for the argument algorithmic name.
            /// </summary>
            /// <returns>The algorithmic char or -1 otherwise.</returns>
            internal int GetChar(string name)
            {
                int prefixlen = m_prefix_.Length;
                if (name.Length < prefixlen ||
                    !m_prefix_.Equals(name.Substring(0, prefixlen - 0))) // ICU4N: Checked 2nd parameter
                {
                    return -1;
                }

                switch (m_type_)
                {
                    case (byte)TYPE_0_:
                        try
                        {
                            int result = System.Convert.ToInt32(name.Substring(prefixlen),
                                                          16);
                            // does it fit into the range?
                            if (m_rangestart_ <= result && result <= m_rangeend_)
                            {
                                return result;
                            }
                        }
                        catch (FormatException e)
                        {
                            return -1;
                        }
                        break;
                    case (byte)TYPE_1_:
                        // repetitative suffix name comparison done here
                        // offset is the character code - start
                        for (int ch = m_rangestart_; ch <= m_rangeend_; ch++)
                        {
                            int offset = ch - m_rangestart_;
                            int[] indexes = m_utilIntBuffer_;
                            int factor;

                            // write elements according to the factors
                            // the factorized elements are determined by modulo
                            // arithmetic
                            lock (m_utilIntBuffer_)
                            {
                                for (int i = m_variant_ - 1; i > 0; i--)
                                {
                                    factor = m_factor_[i] & 0x00FF;
                                    indexes[i] = offset % factor;
                                    offset /= factor;
                                }

                                // we don't need to calculate the last modulus
                                // because start <= code <= end guarantees here that
                                // code <= factors[0]
                                indexes[0] = offset;

                                // joining up the factorized strings
                                if (CompareFactorString(indexes, m_variant_, name,
                                                        prefixlen))
                                {
                                    return ch;
                                }
                            }
                        }
                        break;
                }

                return -1;
            }

            /// <summary>
            /// Adds all chars in the set of algorithmic names into the set.
            /// Equivalent to part of calcAlgNameSetsLengths.
            /// </summary>
            /// <param name="set"><see cref="int"/> set to add the chars of the algorithm names into.</param>
            /// <param name="maxlength">Maximum length to compare to.</param>
            /// <returns>The length that is either <paramref name="maxlength"/> of the length of this
            /// algorithm name if it is longer than <paramref name="maxlength"/>.</returns>
            internal int Add(int[] set, int maxlength)
            {
                // prefix length
                int length = UCharacterName.Add(set, m_prefix_);
                switch (m_type_)
                {
                    case TYPE_0_:
                        {
                            // name = prefix + (range->variant times) hex-digits
                            // prefix
                            length += m_variant_;
                            /* synwee to check
                             * addString(set, (const char *)(range + 1))
                                               + range->variant;*/
                            break;
                        }
                    case TYPE_1_:
                        {
                            // name = prefix factorized-elements
                            // get the set and maximum factor suffix length for each
                            // factor
                            for (int i = m_variant_ - 1; i > 0; i--)
                            {
                                int maxfactorlength = 0;
                                int count = 0;
                                for (int factor = m_factor_[i]; factor > 0; --factor)
                                {
                                    lock (m_utilStringBuffer_)
                                    {
                                        m_utilStringBuffer_.Length = 0;
                                        count
                                          = UCharacterUtility.GetNullTermByteSubString(
                                                        m_utilStringBuffer_,
                                                        m_factorstring_, count);
                                        UCharacterName.Add(set, m_utilStringBuffer_);
                                        if (m_utilStringBuffer_.Length
                                                                    > maxfactorlength)
                                        {
                                            maxfactorlength
                                                        = m_utilStringBuffer_.Length;
                                        }
                                    }
                                }
                                length += maxfactorlength;
                            }
                        }
                        break;
                }
                if (length > maxlength)
                {
                    return length;
                }
                return maxlength;
            }

            // private data members ------------------------------------------

            //
            // Algorithmic data information
            //
            internal int m_rangestart_;
            internal int m_rangeend_;
            private byte m_type_;
            private byte m_variant_;
            private char[] m_factor_;
            private string m_prefix_;
            private byte[] m_factorstring_;
            /// <summary>
            /// Utility StringBuffer
            /// </summary>
            private StringBuffer m_utilStringBuffer_ = new StringBuffer();
            /// <summary>
            /// Utility int buffer
            /// </summary>
            private int[] m_utilIntBuffer_ = new int[256];

            // private methods -----------------------------------------------

            /// <summary>
            /// Gets the indexth string in each of the argument factor block.
            /// </summary>
            /// <param name="index">Array with each index corresponding to each factor block.</param>
            /// <param name="length">Length of the array index.</param>
            /// <returns>The combined string of the array of indexth factor string in factor block.</returns>
            private string GetFactorString(int[] index, int length)
            {
                int size = m_factor_.Length;
                if (index == null || length != size)
                {
                    return null;
                }

                lock (m_utilStringBuffer_)
                {
                    m_utilStringBuffer_.Length = 0;
                    int count = 0;
                    int factor;
                    size--;
                    for (int i = 0; i <= size; i++)
                    {
                        factor = m_factor_[i];
                        count = UCharacterUtility.SkipNullTermByteSubString(
                                                 m_factorstring_, count, index[i]);
                        count = UCharacterUtility.GetNullTermByteSubString(
                                              m_utilStringBuffer_, m_factorstring_,
                                              count);
                        if (i != size)
                        {
                            count = UCharacterUtility.SkipNullTermByteSubString(
                                                           m_factorstring_, count,
                                                           factor - index[i] - 1);
                        }
                    }
                    return m_utilStringBuffer_.ToString();
                }
            }

            /// <summary>
            /// Compares the indexth string in each of the argument factor block with
            /// the argument string.
            /// </summary>
            /// <param name="index">Array with each index corresponding to each factor block.</param>
            /// <param name="length">Index array length.</param>
            /// <param name="str">String to compare with.</param>
            /// <param name="offset">Offset of str to start comparison.</param>
            /// <returns>true if string matches.</returns>
            private bool CompareFactorString(int[] index, int length, string str,
                                                int offset)
            {
                int size = m_factor_.Length;
                if (index == null || length != size)
                    return false;

                int count = 0;
                int strcount = offset;
                int factor;
                size--;
                for (int i = 0; i <= size; i++)
                {
                    factor = m_factor_[i];
                    count = UCharacterUtility.SkipNullTermByteSubString(
                                              m_factorstring_, count, index[i]);
                    strcount = UCharacterUtility.CompareNullTermByteSubString(str,
                                              m_factorstring_, strcount, count);
                    if (strcount < 0)
                    {
                        return false;
                    }

                    if (i != size)
                    {
                        count = UCharacterUtility.SkipNullTermByteSubString(
                                      m_factorstring_, count, factor - index[i]);
                    }
                }
                if (strcount != str.Length)
                {
                    return false;
                }
                return true;
            }
        }

        // package private data members --------------------------------------

        /// <summary>
        /// Size of each groups
        /// </summary>
        internal int m_groupsize_ = 0;

        // package private methods --------------------------------------------

        /// <summary>
        /// Sets the token data.
        /// </summary>
        /// <param name="token">Array of tokens.</param>
        /// <param name="tokenstring">Array of string values of the tokens.</param>
        /// <returns>false if there is a data error.</returns>
        internal bool SetToken(char[] token, byte[] tokenstring)
        {
            if (token != null && tokenstring != null && token.Length > 0 &&
                tokenstring.Length > 0)
            {
                m_tokentable_ = token;
                m_tokenstring_ = tokenstring;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Set the algorithm name information array.
        /// </summary>
        /// <param name="alg">Algorithm information array.</param>
        /// <returns>true if the group string offset has been set correctly.</returns>
        internal bool SetAlgorithm(AlgorithmName[] alg)
        {
            if (alg != null && alg.Length != 0)
            {
                m_algorithm_ = alg;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the number of group and size of each group in number of char.
        /// </summary>
        /// <param name="count">Number of groups.</param>
        /// <param name="size">Size of group in char.</param>
        /// <returns>true if group size is set correctly.</returns>
        internal bool SetGroupCountSize(int count, int size)
        {
            if (count <= 0 || size <= 0)
            {
                return false;
            }
            m_groupcount_ = count;
            m_groupsize_ = size;
            return true;
        }

        /// <summary>
        /// Sets the group name data.
        /// </summary>
        /// <param name="group">Index information array.</param>
        /// <param name="groupstring">Name information array.</param>
        /// <returns>false if there is a data error.</returns>
        internal bool SetGroup(char[] group, byte[] groupstring)
        {
            if (group != null && groupstring != null && group.Length > 0 &&
                groupstring.Length > 0)
            {
                m_groupinfo_ = group;
                m_groupstring_ = groupstring;
                return true;
            }
            return false;
        }

        // private data members ----------------------------------------------

        //
        // Data used in unames.icu
        //
        private char[] m_tokentable_;
        private byte[] m_tokenstring_;
        private char[] m_groupinfo_;
        private byte[] m_groupstring_;
        private AlgorithmName[] m_algorithm_;

        //
        // Group use.  Note - access must be synchronized.
        //
        private char[] m_groupoffsets_ = new char[LINES_PER_GROUP_ + 1];
        private char[] m_grouplengths_ = new char[LINES_PER_GROUP_ + 1];

        /// <summary>
        /// Default name of the name datafile
        /// </summary>
        private static readonly string FILE_NAME_ = "unames.icu";
        /// <summary>
        /// Shift count to retrieve group information
        /// </summary>
        private static readonly int GROUP_SHIFT_ = 5;
        /// <summary>
        /// Mask to retrieve the offset for a particular character within a group
        /// </summary>
        private static readonly int GROUP_MASK_ = LINES_PER_GROUP_ - 1;

        /// <summary>
        /// Position of offsethigh in group information array
        /// </summary>
        private static readonly int OFFSET_HIGH_OFFSET_ = 1;

        /// <summary>
        /// Position of offsetlow in group information array
        /// </summary>
        private static readonly int OFFSET_LOW_OFFSET_ = 2;

        /// <summary>
        /// Double nibble indicator, any nibble > this number has to be combined
        /// with its following nibble
        /// </summary>
        private static readonly int SINGLE_NIBBLE_MAX_ = 11;

        /*
         * Maximum length of character names (regular & 1.0).
         */
        //private static int MAX_NAME_LENGTH_ = 0;
        /*
         * Maximum length of ISO comments.
         */
        //private static int MAX_ISO_COMMENT_LENGTH_ = 0;

        /// <summary>
        /// Set of chars used in character names (regular & 1.0).
        /// Chars are platform-dependent (can be EBCDIC).
        /// </summary>
        private int[] m_nameSet_ = new int[8];
        /// <summary>
        /// Set of chars used in ISO comments. (regular & 1.0).
        /// Chars are platform-dependent (can be EBCDIC).
        /// </summary>
        private int[] m_ISOCommentSet_ = new int[8];
        /// <summary>
        /// Utility <see cref="StringBuffer"/>
        /// </summary>
        private StringBuffer m_utilStringBuffer_ = new StringBuffer();
        /// <summary>
        /// Utility <see cref="int"/> buffer
        /// </summary>
        private int[] m_utilIntBuffer_ = new int[2];
        /// <summary>
        /// Maximum ISO comment length
        /// </summary>
        private int m_maxISOCommentLength_;
        /// <summary>
        /// Maximum name length
        /// </summary>
        private int m_maxNameLength_;
        /// <summary>
        /// Type names used for extended names
        /// </summary>
        private static readonly string[] TYPE_NAMES_ = {"unassigned",
                                                 "uppercase letter",
                                                 "lowercase letter",
                                                 "titlecase letter",
                                                 "modifier letter",
                                                 "other letter",
                                                 "non spacing mark",
                                                 "enclosing mark",
                                                 "combining spacing mark",
                                                 "decimal digit number",
                                                 "letter number",
                                                 "other number",
                                                 "space separator",
                                                 "line separator",
                                                 "paragraph separator",
                                                 "control",
                                                 "format",
                                                 "private use area",
                                                 "surrogate",
                                                 "dash punctuation",
                                                 "start punctuation",
                                                 "end punctuation",
                                                 "connector punctuation",
                                                 "other punctuation",
                                                 "math symbol",
                                                 "currency symbol",
                                                 "modifier symbol",
                                                 "other symbol",
                                                 "initial punctuation",
                                                 "final punctuation",
                                                 "noncharacter",
                                                 "lead surrogate",
                                                 "trail surrogate"};
        /// <summary>
        /// Unknown type name
        /// </summary>
        private static readonly string UNKNOWN_TYPE_NAME_ = "unknown";
        /// <summary>
        /// Not a character type
        /// </summary>
        private static readonly int NON_CHARACTER_
                                        = UCharacterCategoryExtensions.CharCategoryCount;
        /// <summary>
        /// Lead surrogate type
        /// </summary>
        private static readonly int LEAD_SURROGATE_
                                      = UCharacterCategoryExtensions.CharCategoryCount + 1;
        /// <summary>
        /// Trail surrogate type
        /// </summary>
        private static readonly int TRAIL_SURROGATE_
                                      = UCharacterCategoryExtensions.CharCategoryCount + 2;
        /// <summary>
        /// Extended category count
        /// </summary>
        static readonly int EXTENDED_CATEGORY_
                                      = UCharacterCategoryExtensions.CharCategoryCount + 3;

        // private constructor ------------------------------------------------

        /// <summary>
        /// Protected constructor for use in <see cref="UChar"/>.
        /// </summary>
        /// <exception cref="IOException">Thrown when data reading fails.</exception>
        private UCharacterName()
        {
            ByteBuffer b = ICUBinary.GetRequiredData(FILE_NAME_);
            UCharacterNameReader reader = new UCharacterNameReader(b);
            reader.Read(this);
        }

        // private methods ---------------------------------------------------

        /// <summary>
        /// Gets the algorithmic name for the argument character.
        /// </summary>
        /// <param name="ch">Character to determine name for.</param>
        /// <param name="choice">Name choice.</param>
        /// <returns>The algorithmic name or null if not found.</returns>
        private string GetAlgName(int ch, UCharacterNameChoice choice)
        {
            /* Only the normative character name can be algorithmic. */
            if (choice == UCharacterNameChoice.UnicodeCharName ||
                choice == UCharacterNameChoice.ExtendedCharName
            )
            {
                // index in terms integer index
                lock (m_utilStringBuffer_)
                {
                    m_utilStringBuffer_.Length = 0;

                    for (int index = m_algorithm_.Length - 1; index >= 0; index--)
                    {
                        if (m_algorithm_[index].Contains(ch))
                        {
                            m_algorithm_[index].AppendName(ch, m_utilStringBuffer_);
                            return m_utilStringBuffer_.ToString();
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Getting the character with the tokenized argument name.
        /// </summary>
        /// <param name="name">Name of the character.</param>
        /// <param name="choice"></param>
        /// <returns>Character with the tokenized argument name or -1 if character is not found.</returns>
        private int GetGroupChar(string name, UCharacterNameChoice choice)
        {
            lock (this)
            {
                for (int i = 0; i < m_groupcount_; i++)
                {
                    // populating the data set of grouptable

                    int startgpstrindex = GetGroupLengths(i, m_groupoffsets_,
                                                          m_grouplengths_);

                    // shift out to function
                    int result = GetGroupChar(startgpstrindex, m_grouplengths_, name,
                                              choice);
                    if (result != -1)
                    {
                        return (m_groupinfo_[i * m_groupsize_] << GROUP_SHIFT_)
                                 | result;
                    }
                }
                return -1;
            }
        }

        /// <summary>
        /// Compares and retrieve character if name is found within the argument
        /// group.
        /// </summary>
        /// <param name="index">Index where the set of names reside in the group block.</param>
        /// <param name="length">List of lengths of the strings.</param>
        /// <param name="name">Character name to search for.</param>
        /// <param name="choice">Choice of either 1.0 or the most current unicode name.</param>
        /// <returns>Relative character in the group which matches name, otherwise if
        /// not found, -1 will be returned.</returns>
        private int GetGroupChar(int index, char[] length, string name,
                                 UCharacterNameChoice choice)
        {
            byte b = 0;
            char token;
            int len;
            int namelen = name.Length;
            int nindex;
            int count;

            for (int result = 0; result <= LINES_PER_GROUP_; result++)
            {
                nindex = 0;
                len = length[result];

                if (choice != UCharacterNameChoice.UnicodeCharName &&
                    choice != UCharacterNameChoice.ExtendedCharName
                )
                {
                    /*
                     * skip the modern name if it is not requested _and_
                     * if the semicolon byte value is a character, not a token number
                     */
                    int fieldIndex = choice == UCharacterNameChoice.IsoComment ? 2 : (int)choice;
                    do
                    {
                        int oldindex = index;
                        index += UCharacterUtility.SkipByteSubString(m_groupstring_,
                                                             index, len, (byte)';');
                        len -= (index - oldindex);
                    } while (--fieldIndex > 0);
                }

                // number of tokens is > the length of the name
                // write each letter directly, and write a token word per token
                for (count = 0; count < len && nindex != -1 && nindex < namelen;
                    )
                {
                    b = m_groupstring_[index + count];
                    count++;

                    if (b >= m_tokentable_.Length)
                    {
                        if (name[nindex++] != (b & 0xFF))
                        {
                            nindex = -1;
                        }
                    }
                    else
                    {
                        token = m_tokentable_[b & 0xFF];
                        if (token == 0xFFFE)
                        {
                            // this is a lead byte for a double-byte token
                            token = m_tokentable_[b << 8 |
                                       (m_groupstring_[index + count] & 0x00ff)];
                            count++;
                        }
                        if (token == 0xFFFF)
                        {
                            if (name[nindex++] != (b & 0xFF))
                            {
                                nindex = -1;
                            }
                        }
                        else
                        {
                            // compare token with name
                            nindex = UCharacterUtility.CompareNullTermByteSubString(
                                            name, m_tokenstring_, nindex, token);
                        }
                    }
                }

                if (namelen == nindex &&
                    (count == len || m_groupstring_[index + count] == ';'))
                {
                    return result;
                }

                index += len;
            }
            return -1;
        }

        /// <summary>
        /// Gets the character extended type.
        /// </summary>
        /// <param name="ch">Character to be tested.</param>
        /// <returns>Extended type it is associated with.</returns>
        private static int GetType(int ch)
        {
            if (UCharacterUtility.IsNonCharacter(ch))
            {
                // not a character we return a invalid category count
                return NON_CHARACTER_;
            }
            int result = UChar.GetType(ch).ToInt32();
            if (result == UCharacterCategory.Surrogate.ToInt32())
            {
                if (ch <= UTF16.LEAD_SURROGATE_MAX_VALUE)
                {
                    result = LEAD_SURROGATE_;
                }
                else
                {
                    result = TRAIL_SURROGATE_;
                }
            }
            return result;
        }

        /// <summary>
        /// Getting the character with extended name of the form &lt;....>.
        /// </summary>
        /// <param name="name">Name of the character to be found.</param>
        /// <param name="choice">Name choice.</param>
        /// <returns>Character associated with the name, -1 if such character is not
        /// found and -2 if we should continue with the search.</returns>
        private static int GetExtendedChar(string name, UCharacterNameChoice choice)
        {
            if (name[0] == '<')
            {
                if (choice == UCharacterNameChoice.ExtendedCharName)
                {
                    int endIndex = name.Length - 1;
                    if (name[endIndex] == '>')
                    {
                        int startIndex = name.LastIndexOf('-');
                        if (startIndex >= 0)
                        { // We've got a category.
                            startIndex++;
                            int result = -1;
                            if (!int.TryParse(name.Substring(startIndex, endIndex - startIndex), // ICU4N: Corrected 2nd parameter
                                NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result))
                            {
                                return -1;
                            }

                            // Now validate the category name. We could use a
                            // binary search, or a trie, if we really wanted to.
                            string type = name.Substring(1, (startIndex - 1) - 1); // ICU4N: Corrected 2nd parameter
                            int length = TYPE_NAMES_.Length;
                            for (int i = 0; i < length; ++i)
                            {
                                if (type.CompareToOrdinal(TYPE_NAMES_[i]) == 0)
                                {
                                    if (GetType(result) == i)
                                    {
                                        return result;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                return -1;
            }
            return -2;
        }

        // sets of name characters, maximum name lengths -----------------------

        /// <summary>
        /// Adds a codepoint into a set of <see cref="int"/>s.
        /// Equivalent to SET_ADD.
        /// </summary>
        /// <param name="set">Set to add to.</param>
        /// <param name="ch">16 bit char to add.</param>
        private static void Add(int[] set, char ch)
        {
            set[ch.TripleShift(5)] |= 1 << (ch & 0x1f);
        }

        /// <summary>
        /// Checks if a codepoint is a part of a set of <see cref="int"/>s.
        /// Equivalent to SET_CONTAINS.
        /// </summary>
        /// <param name="set">Set to check in.</param>
        /// <param name="ch">16 bit char to check.</param>
        /// <returns>true if codepoint is part of the set, false otherwise.</returns>
        private static bool Contains(int[] set, char ch)
        {
            return (set[ch.TripleShift(5)] & (1 << (ch & 0x1f))) != 0;
        }

        /// <summary>
        /// Adds all characters of the argument <paramref name="str"/> and gets the length.
        /// Equivalent to calcStringSetLength.
        /// </summary>
        /// <param name="set">Set to add all chars of <paramref name="str"/> to.</param>
        /// <param name="str">String to add.</param>
        private static int Add(int[] set, string str)
        {
            int result = str.Length;

            for (int i = result - 1; i >= 0; i--)
            {
                Add(set, str[i]);
            }
            return result;
        }

        /// <summary>
        /// Adds all characters of the argument <paramref name="str"/> and gets the length.
        /// Equivalent to calcStringSetLength.
        /// </summary>
        /// <param name="set">Set to add all chars of <paramref name="str"/> to.</param>
        /// <param name="str">String to add.</param>
        /// <returns></returns>
        private static int Add(int[] set, StringBuffer str)
        {
            int result = str.Length;

            for (int i = result - 1; i >= 0; i--)
            {
                Add(set, str[i]);
            }
            return result;
        }

        /// <summary>
        /// Adds all algorithmic names into the name set.
        /// Equivalent to part of calcAlgNameSetsLengths.
        /// </summary>
        /// <param name="maxlength">Length to compare to.</param>
        /// <returns>the maximum length of any possible algorithmic name if it is >
        /// maxlength, otherwise <paramref name="maxlength"/> is returned.</returns>
        private int AddAlgorithmName(int maxlength)
        {
            int result = 0;
            for (int i = m_algorithm_.Length - 1; i >= 0; i--)
            {
                result = m_algorithm_[i].Add(m_nameSet_, maxlength);
                if (result > maxlength)
                {
                    maxlength = result;
                }
            }
            return maxlength;
        }

        /// <summary>
        /// Adds all extended names into the name set.
        /// Equivalent to part of calcExtNameSetsLengths.
        /// </summary>
        /// <param name="maxlength">Length to compare to.</param>
        /// <returns>The <paramref name="maxlength"/> of any possible extended name.</returns>
        private int AddExtendedName(int maxlength)
        {
            for (int i = TYPE_NAMES_.Length - 1; i >= 0; i--)
            {
                // for each category, count the length of the category name
                // plus 9 =
                // 2 for <>
                // 1 for -
                // 6 for most hex digits per code point
                int length = 9 + Add(m_nameSet_, TYPE_NAMES_[i]);
                if (length > maxlength)
                {
                    maxlength = length;
                }
            }
            return maxlength;
        }

        /// <summary>
        /// Adds names of a group to the argument set.
        /// Equivalent to calcNameSetLength.
        /// </summary>
        /// <param name="offset">Offset of the group name string in byte count.</param>
        /// <param name="length">Length of the group name string.</param>
        /// <param name="tokenlength">Array to store the length of each token.</param>
        /// <param name="set">Set to add to.</param>
        /// <returns>The length of the name string and the length of the group
        /// string parsed.</returns>
        private int[] AddGroupName(int offset, int length, byte[] tokenlength,
                                   int[] set)
        {
            int resultnlength = 0;
            int resultplength = 0;
            while (resultplength < length)
            {
                char b = (char)(m_groupstring_[offset + resultplength] & 0xff);
                resultplength++;
                if (b == ';')
                {
                    break;
                }

                if (b >= m_tokentable_.Length)
                {
                    Add(set, b); // implicit letter
                    resultnlength++;
                }
                else
                {
                    char token = m_tokentable_[b & 0x00ff];
                    if (token == 0xFFFE)
                    {
                        // this is a lead byte for a double-byte token
                        b = (char)(b << 8 | (m_groupstring_[offset + resultplength]
                                             & 0x00ff));
                        token = m_tokentable_[b];
                        resultplength++;
                    }
                    if (token == 0xFFFF)
                    {
                        Add(set, b);
                        resultnlength++;
                    }
                    else
                    {
                        // count token word
                        // use cached token length
                        byte tlength = tokenlength[b];
                        if (tlength == 0)
                        {
                            lock (m_utilStringBuffer_)
                            {
                                m_utilStringBuffer_.Length = 0;
                                UCharacterUtility.GetNullTermByteSubString(
                                               m_utilStringBuffer_, m_tokenstring_,
                                               token);
                                tlength = (byte)Add(set, m_utilStringBuffer_);
                            }
                            tokenlength[b] = tlength;
                        }
                        resultnlength += tlength;
                    }
                }
            }
            m_utilIntBuffer_[0] = resultnlength;
            m_utilIntBuffer_[1] = resultplength;
            return m_utilIntBuffer_;
        }

        /// <summary>
        /// Adds names of all group to the argument set.
        /// Sets the data member m_max*Length_.
        /// Method called only once.
        /// Equivalent to calcGroupNameSetsLength.
        /// </summary>
        /// <param name="maxlength">Length to compare to.</param>
        private void AddGroupName(int maxlength)
        {
            int maxisolength = 0;
            char[] offsets = new char[LINES_PER_GROUP_ + 2];
            char[] lengths = new char[LINES_PER_GROUP_ + 2];
            byte[] tokenlengths = new byte[m_tokentable_.Length];

            // enumerate all groups
            // for (int i = m_groupcount_ - 1; i >= 0; i --) {
            for (int i = 0; i < m_groupcount_; i++)
            {
                int offset = GetGroupLengths(i, offsets, lengths);
                // enumerate all lines in each group
                // for (int linenumber = LINES_PER_GROUP_ - 1; linenumber >= 0;
                //    linenumber --) {
                for (int linenumber = 0; linenumber < LINES_PER_GROUP_;
                    linenumber++)
                {
                    int lineoffset = offset + offsets[linenumber];
                    int length = lengths[linenumber];
                    if (length == 0)
                    {
                        continue;
                    }

                    // read regular name
                    int[] parsed = AddGroupName(lineoffset, length, tokenlengths,
                                                m_nameSet_);
                    if (parsed[0] > maxlength)
                    {
                        // 0 for name length
                        maxlength = parsed[0];
                    }
                    lineoffset += parsed[1];
                    if (parsed[1] >= length)
                    {
                        // 1 for parsed group string length
                        continue;
                    }
                    length -= parsed[1];
                    // read Unicode 1.0 name
                    parsed = AddGroupName(lineoffset, length, tokenlengths,
                                          m_nameSet_);
                    if (parsed[0] > maxlength)
                    {
                        // 0 for name length
                        maxlength = parsed[0];
                    }
                    lineoffset += parsed[1];
                    if (parsed[1] >= length)
                    {
                        // 1 for parsed group string length
                        continue;
                    }
                    length -= parsed[1];
                    // read ISO comment
                    parsed = AddGroupName(lineoffset, length, tokenlengths,
                                          m_ISOCommentSet_);
                    if (parsed[1] > maxisolength)
                    {
                        maxisolength = length;
                    }
                }
            }

            // set gMax... - name length last for threading
            m_maxISOCommentLength_ = maxisolength;
            m_maxNameLength_ = maxlength;
        }

        /// <summary>
        /// Sets up the name sets and the calculation of the maximum lengths.
        /// Equivalent to calcNameSetsLengths.
        /// </summary>
        private bool InitNameSetsLengths()
        {
            if (m_maxNameLength_ > 0)
            {
                return true;
            }

            string extra = "0123456789ABCDEF<>-";
            // set hex digits, used in various names, and <>-, used in extended
            // names
            for (int i = extra.Length - 1; i >= 0; i--)
            {
                Add(m_nameSet_, extra[i]);
            }

            // set sets and lengths from algorithmic names
            m_maxNameLength_ = AddAlgorithmName(0);
            // set sets and lengths from extended names
            m_maxNameLength_ = AddExtendedName(m_maxNameLength_);
            // set sets and lengths from group names, set global maximum values
            AddGroupName(m_maxNameLength_);
            return true;
        }

        /// <summary>
        /// Converts the char set cset into a Unicode set uset.
        /// Equivalent to charSetToUSet.
        /// </summary>
        /// <param name="set">Set of 256 bit flags corresponding to a set of chars.</param>
        /// <param name="uset">USet to receive characters. Existing contents are deleted.</param>
        private void Convert(int[] set, UnicodeSet uset)
        {
            uset.Clear();
            if (!InitNameSetsLengths())
            {
                return;
            }

            // build a char string with all chars that are used in character names
            for (char c = (char)255; c > 0; c--)
            {
                if (Contains(set, c))
                {
                    uset.Add(c);
                }
            }
        }
    }
}
