using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Util
{
    /// <summary>
    /// Class to store version numbers of the form major.minor.milli.micro.
    /// </summary>
    /// <author>synwee</author>
    /// <stable>ICU 2.6</stable>
    public sealed class VersionInfo : IComparable<VersionInfo>
    {
        internal static object syncLock = new object();

        // public data members -------------------------------------------------

        /// <summary>
        /// Unicode 1.0 version
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public static readonly VersionInfo UNICODE_1_0;

        /// <summary>
        /// Unicode 1.0.1 version
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public static readonly VersionInfo UNICODE_1_0_1;

        /// <summary>
        /// Unicode 1.1.0 version
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public static readonly VersionInfo UNICODE_1_1_0;

        /// <summary>
        /// Unicode 1.1.5 version
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public static readonly VersionInfo UNICODE_1_1_5;

        /// <summary>
        /// Unicode 2.0 version
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public static readonly VersionInfo UNICODE_2_0;

        /// <summary>
        /// Unicode 2.1.2 version
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public static readonly VersionInfo UNICODE_2_1_2;

        /// <summary>
        /// Unicode 2.1.5 version
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public static readonly VersionInfo UNICODE_2_1_5;

        /// <summary>
        /// Unicode 2.1.8 version
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public static readonly VersionInfo UNICODE_2_1_8;

        /// <summary>
        /// Unicode 2.1.9 version
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public static readonly VersionInfo UNICODE_2_1_9;

        /// <summary>
        /// Unicode 3.0 version
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public static readonly VersionInfo UNICODE_3_0;

        /// <summary>
        /// Unicode 3.0.1 version
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public static readonly VersionInfo UNICODE_3_0_1;

        /// <summary>
        /// Unicode 3.1.0 version
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public static readonly VersionInfo UNICODE_3_1_0;

        /// <summary>
        /// Unicode 3.1.1 version
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public static readonly VersionInfo UNICODE_3_1_1;

        /// <summary>
        /// Unicode 3.2 version
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public static readonly VersionInfo UNICODE_3_2;

        /// <summary>
        /// Unicode 4.0 version
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public static readonly VersionInfo UNICODE_4_0;

        /// <summary>
        /// Unicode 4.0.1 version
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public static readonly VersionInfo UNICODE_4_0_1;

        /// <summary>
        /// Unicode 4.1 version
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public static readonly VersionInfo UNICODE_4_1;

        /// <summary>
        /// Unicode 5.0 version
        /// </summary>
        /// <stable>ICU 3.4</stable>
        public static readonly VersionInfo UNICODE_5_0;

        /// <summary>
        /// Unicode 5.1 version
        /// </summary>
        /// <stable>ICU 4.2</stable>
        public static readonly VersionInfo UNICODE_5_1;

        /// <summary>
        /// Unicode 5.2 version
        /// </summary>
        /// <stable>ICU 4.4</stable>
        public static readonly VersionInfo UNICODE_5_2;

        /// <summary>
        /// Unicode 6.0 version
        /// </summary>
        /// <stable>ICU 4.6</stable>
        public static readonly VersionInfo UNICODE_6_0;

        /// <summary>
        /// Unicode 6.1 version
        /// </summary>
        /// <stable>ICU 49</stable>
        public static readonly VersionInfo UNICODE_6_1;

        /// <summary>
        /// Unicode 6.2 version
        /// </summary>
        /// <stable>ICU 50</stable>
        public static readonly VersionInfo UNICODE_6_2;

        /// <summary>
        /// Unicode 6.3 version
        /// </summary>
        /// <stable>ICU 52</stable>
        public static readonly VersionInfo UNICODE_6_3;

        /// <summary>
        /// Unicode 7.0 version
        /// </summary>
        /// <stable>ICU 54</stable>
        public static readonly VersionInfo UNICODE_7_0;

        /// <summary>
        /// Unicode 8.0 version
        /// </summary>
        /// <stable>ICU 56</stable>
        public static readonly VersionInfo UNICODE_8_0;

        /// <summary>
        /// Unicode 9.0 version
        /// </summary>
        /// <stable>ICU 58</stable>
        public static readonly VersionInfo UNICODE_9_0;

        /// <summary>
        /// Unicode 10.0 version
        /// </summary>
        /// <stable>ICU 60</stable>
        public static readonly VersionInfo UNICODE_10_0;

        /// <summary>
        /// ICU4N current release version
        /// </summary>
        /// <stable>ICU 2.8</stable>
        public static readonly VersionInfo ICU_VERSION;

        /// <summary>
        /// Data version string for ICU's internal data.
        /// Used for appending to data path (e.g. icudt43b)
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public static readonly String ICU_DATA_VERSION_PATH = "60b";

        /// <summary>
        /// Data version in ICU4N.
        /// </summary>
        /// <internal/>
        [Obsolete("This API is ICU internal only.")]
        public static readonly VersionInfo ICU_DATA_VERSION;

        /// <summary>
        /// Collation runtime version (sort key generator, string comparisons).
        /// If the version is different, sort keys for the same string could be different.
        /// This value may change in subsequent releases of ICU.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        public static readonly VersionInfo UCOL_RUNTIME_VERSION;

        /// <summary>
        ///  Collation builder code version.
        ///  When this is different, the same tailoring might result
        ///  in assigning different collation elements to code points.
        ///  This value may change in subsequent releases of ICU.
        /// </summary>
        /// <stable>ICU 2.8</stable>
        public static readonly VersionInfo UCOL_BUILDER_VERSION;

        /// <summary>
        /// Constant version 1.
        /// This was intended to be the version of collation tailorings,
        /// but instead the tailoring data carries a version number.
        /// </summary>
        [Obsolete("ICU 54")]
        public static readonly VersionInfo UCOL_TAILORINGS_VERSION;


        // public methods ------------------------------------------------------

        /// <summary>
        /// Returns an instance of <see cref="VersionInfo"/> with the argument version.
        /// or "major.minor.milli" or "major.minor" or "major",
        /// where major, minor, milli, micro are non-negative numbers
        /// &lt;= 255. If the trailing version numbers are
        /// not specified they are taken as 0s. E.g. Version "3.1" is
        /// equivalent to "3.1.0.0".</summary>
        /// <param name="version">Version string in the format of "major.minor.milli.micro"
        /// </param>
        /// <returns>An instance of <see cref="VersionInfo"/> with the argument version.</returns>
        /// <exception cref="ArgumentException">When the argument version is not in the right format.</exception>
        /// <stable>ICU 2.6</stable>
        public static VersionInfo GetInstance(string version)
        {
            int length = version.Length;
            int[] array = { 0, 0, 0, 0 };
            int count = 0;
            int index = 0;

            while (count < 4 && index < length)
            {
                char c = version[index];
                if (c == '.')
                {
                    count++;
                }
                else
                {
                    c -= '0';
                    if (c < 0 || c > 9)
                    {
                        throw new ArgumentException(INVALID_VERSION_NUMBER_);
                    }
                    array[count] *= 10;
                    array[count] += c;
                }
                index++;
            }
            if (index != length)
            {
                throw new ArgumentException(
                                                   "Invalid version number: String '" + version + "' exceeds version format");
            }
            for (int i = 0; i < 4; i++)
            {
                if (array[i] < 0 || array[i] > 255)
                {
                    throw new ArgumentException(INVALID_VERSION_NUMBER_);
                }
            }

            return GetInstance(array[0], array[1], array[2], array[3]);
        }

        /// <summary>
        /// Returns an instance of <see cref="VersionInfo"/> with the argument version.
        /// </summary>
        /// <param name="major">Major version, non-negative number &lt;= 255.</param>
        /// <param name="minor">Minor version, non-negative number &lt;= 255.</param>
        /// <param name="milli">Milli version, non-negative number &lt;= 255.</param>
        /// <param name="micro">Micro version, non-negative number &lt;= 255.</param>
        /// <returns>An instance of <see cref="VersionInfo"/> with the arguments' version.</returns>
        /// <exception cref="ArgumentException">When any argument is negative or &gt; 255.</exception>
        /// <stable>ICU 2.6</stable>
        public static VersionInfo GetInstance(int major, int minor, int milli,
                                              int micro)
        {
            // checks if it is in the hashmap
            // else
            if (major < 0 || major > 255 || minor < 0 || minor > 255 ||
                milli < 0 || milli > 255 || micro < 0 || micro > 255)
            {
                throw new ArgumentException(INVALID_VERSION_NUMBER_);
            }
            int key = GetInt32(major, minor, milli, micro);
            int version = key;
            VersionInfo result;
            lock (MAP_)
                MAP_.TryGetValue(key, out result);
            if (result == null)
            {
                result = new VersionInfo(version);
                VersionInfo tmpvi;
                lock (MAP_)
                {
                    if (!MAP_.TryGetValue(key, out tmpvi) || tmpvi == null)
                    {
                        // Put if absent
                        MAP_[key] = result;
                    }   
                }
                if (tmpvi != null)
                {
                    result = tmpvi;
                }

                //VersionInfo tmpvi = MAP_.putIfAbsent(key, result);
                //if (tmpvi != null)
                //{
                //    result = tmpvi;
                //}
            }
            return result;
        }

        /// <summary>
        /// Returns an instance of <see cref="VersionInfo"/> with the argument version.
        /// Equivalent to <c>GetInstance(major, minor, milli, 0)</c>.
        /// </summary>
        /// <param name="major">Major version, non-negative number &lt;= 255.</param>
        /// <param name="minor">Minor version, non-negative number &lt;= 255.</param>
        /// <param name="milli">Milli version, non-negative number &lt;= 255.</param>
        /// <returns>An instance of <see cref="VersionInfo"/> with the arguments' version.</returns>
        /// <exception cref="ArgumentException">When any argument is negative or &gt; 255.</exception>
        /// <stable>ICU 2.6</stable>
        public static VersionInfo GetInstance(int major, int minor, int milli)
        {
            return GetInstance(major, minor, milli, 0);
        }

        /// <summary>
        /// Returns an instance of <see cref="VersionInfo"/> with the argument version.
        /// Equivalent to <c>GetInstance(major, minor, 0, 0)</c>.
        /// </summary>
        /// <param name="major">Major version, non-negative number &lt;= 255.</param>
        /// <param name="minor">Minor version, non-negative number &lt;= 255.</param>
        /// <returns>An instance of <see cref="VersionInfo"/> with the arguments' version.</returns>
        /// <exception cref="ArgumentException">When any argument is negative or &gt; 255.</exception>
        /// <stable>ICU 2.6</stable>
        public static VersionInfo GetInstance(int major, int minor)
        {
            return GetInstance(major, minor, 0, 0);
        }

        /// <summary>
        /// Returns an instance of <see cref="VersionInfo"/> with the argument version.
        /// Equivalent to <c>GetInstance(major, 0, 0, 0)</c>.
        /// </summary>
        /// <param name="major">Major version, non-negative number &lt;= 255.</param>
        /// <returns>An instance of <see cref="VersionInfo"/> with the argument's version.</returns>
        /// <exception cref="ArgumentException">When any argument is negative or &gt; 255.</exception>
        /// <stable>ICU 2.6</stable>
        public static VersionInfo GetInstance(int major)
        {
            return GetInstance(major, 0, 0, 0);
        }

        // ICU4N: JavaVersion not applicable in .NET

        /// <summary>
        /// Returns the <see cref="string"/> representative of <see cref="VersionInfo"/> in the format of
        /// "major.minor.milli.micro"
        /// </summary>
        /// <returns><see cref="string"/> representative of <see cref="VersionInfo"/>.</returns>
        /// <stable>ICU 2.6</stable>
        public override string ToString()
        {
            StringBuilder result = new StringBuilder(7);
            result.Append(Major);
            result.Append('.');
            result.Append(Minor);
            result.Append('.');
            result.Append(Milli);
            result.Append('.');
            result.Append(Micro);
            return result.ToString();
        }

        /// <summary>
        /// Gets the major version number.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public int Major
        {
            get { return (m_version_ >> 24) & LAST_BYTE_MASK_; }
        }

        /// <summary>
        /// Gets the minor version number.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public int Minor
        {
            get { return (m_version_ >> 16) & LAST_BYTE_MASK_; }
        }

        /// <summary>
        /// Gets the milli version number.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public int Milli
        {
            get { return (m_version_ >> 8) & LAST_BYTE_MASK_; }
        }

        /// <summary>
        /// Gets the micro version number.
        /// </summary>
        /// <stable>ICU 2.6</stable>
        public int Micro
        {
            get { return m_version_ & LAST_BYTE_MASK_; }
        }

        /// <summary>
        /// Checks if this version information is equals to the argument version
        /// </summary>
        /// <param name="other">Object to be compared.</param>
        /// <returns>true if other is equals to this object's version information,
        /// false otherwise.</returns>
        /// <stable>ICU 2.6</stable>
        public override bool Equals(object other)
        {
            return other == this;
        }

        /// <summary>
        /// Returns the hash code value for this set.
        /// </summary>
        /// <returns>The hash code value for this set.</returns>
        /// <seealso cref="object.GetHashCode()"/>
        /// <stable>ICU 58</stable>
        public override int GetHashCode()
        {
            return m_version_;
        }

        /// <summary>
        /// Compares other with this <see cref="VersionInfo"/>.
        /// </summary>
        /// <param name="other"><see cref="VersionInfo"/> to be compared.</param>
        /// <returns>
        /// 0 if the argument is a <see cref="VersionInfo"/> object that has version
        /// information equal to this object.
        /// Less than 0 if the argument is a <see cref="VersionInfo"/> object that has
        /// version information greater than this object.
        /// Greater than 0 if the argument is a <see cref="VersionInfo"/> object that
        /// has version information less than this object.
        /// </returns>
        /// <stable>ICU 2.6</stable>
        public int CompareTo(VersionInfo other)
        {
            return m_version_ - other.m_version_;
        }

        // private data members ----------------------------------------------

        /// <summary>
        /// Unicode data version used by the current release.
        /// Defined here privately for printing by the <see cref="PrintVersionInfo(string[])"/> method in this class.
        /// Should be the same as <see cref="UChar.UnicodeVersion"/>
        /// which gets the version number from a data file.
        /// We do not want <see cref="VersionInfo"/> to have an import dependency on <see cref="UChar"/>.
        /// </summary>
        private static readonly VersionInfo UNICODE_VERSION;
 
        /// <summary>
        /// Version number stored as a byte for each of the major, minor, milli and
        /// micro numbers in the 32 bit int.
        /// Most significant for the major and the least significant contains the
        /// micro numbers.
        /// </summary>
        private int m_version_;
        /// <summary>
        /// Map of singletons
        /// </summary>
        // ICU4N specific - we need to lock the dictionary externally anyway, so there is no need for ConcurrentDictionary
        private static readonly IDictionary<int, VersionInfo> MAP_ = new Dictionary<int, VersionInfo>();
        /// <summary>
        /// Last byte mask
        /// </summary>
        private static readonly int LAST_BYTE_MASK_ = 0xFF;
        /// <summary>
        /// Error statement string
        /// </summary>
        private static readonly string INVALID_VERSION_NUMBER_ =
                "Invalid version number: Version number may be negative or greater than 255";

        // static declaration ------------------------------------------------

        /// <summary>
        /// Initialize versions only after <see cref="MAP_"/> has been created
        /// </summary>
        static VersionInfo()
        {
            UNICODE_1_0 = GetInstance(1, 0, 0, 0);
            UNICODE_1_0_1 = GetInstance(1, 0, 1, 0);
            UNICODE_1_1_0 = GetInstance(1, 1, 0, 0);
            UNICODE_1_1_5 = GetInstance(1, 1, 5, 0);
            UNICODE_2_0 = GetInstance(2, 0, 0, 0);
            UNICODE_2_1_2 = GetInstance(2, 1, 2, 0);
            UNICODE_2_1_5 = GetInstance(2, 1, 5, 0);
            UNICODE_2_1_8 = GetInstance(2, 1, 8, 0);
            UNICODE_2_1_9 = GetInstance(2, 1, 9, 0);
            UNICODE_3_0 = GetInstance(3, 0, 0, 0);
            UNICODE_3_0_1 = GetInstance(3, 0, 1, 0);
            UNICODE_3_1_0 = GetInstance(3, 1, 0, 0);
            UNICODE_3_1_1 = GetInstance(3, 1, 1, 0);
            UNICODE_3_2 = GetInstance(3, 2, 0, 0);
            UNICODE_4_0 = GetInstance(4, 0, 0, 0);
            UNICODE_4_0_1 = GetInstance(4, 0, 1, 0);
            UNICODE_4_1 = GetInstance(4, 1, 0, 0);
            UNICODE_5_0 = GetInstance(5, 0, 0, 0);
            UNICODE_5_1 = GetInstance(5, 1, 0, 0);
            UNICODE_5_2 = GetInstance(5, 2, 0, 0);
            UNICODE_6_0 = GetInstance(6, 0, 0, 0);
            UNICODE_6_1 = GetInstance(6, 1, 0, 0);
            UNICODE_6_2 = GetInstance(6, 2, 0, 0);
            UNICODE_6_3 = GetInstance(6, 3, 0, 0);
            UNICODE_7_0 = GetInstance(7, 0, 0, 0);
            UNICODE_8_0 = GetInstance(8, 0, 0, 0);
            UNICODE_9_0 = GetInstance(9, 0, 0, 0);
            UNICODE_10_0 = GetInstance(10, 0, 0, 0);

            ICU_VERSION = GetInstance(60, 1, 0, 0);
#pragma warning disable 612, 618
            ICU_DATA_VERSION = GetInstance(60, 0, 1, 0);
#pragma warning restore 612, 618
            UNICODE_VERSION = UNICODE_10_0;

            UCOL_RUNTIME_VERSION = GetInstance(9);
            UCOL_BUILDER_VERSION = GetInstance(9);
#pragma warning disable 612, 618
            UCOL_TAILORINGS_VERSION = GetInstance(1);
#pragma warning restore 612, 618
        }

        // private constructor -----------------------------------------------

        /// <summary>
        /// Constructor with <see cref="int"/>.
        /// </summary>
        /// <param name="compactversion">A 32 bit int with each byte representing a number.</param>
        private VersionInfo(int compactversion)
        {
            m_version_ = compactversion;
        }

        /// <summary>
        /// Gets the <see cref="int"/> from the version numbers.
        /// </summary>
        /// <param name="major">Non-negative version number.</param>
        /// <param name="minor">Non-negative version number.</param>
        /// <param name="milli">Non-negative version number.</param>
        /// <param name="micro">Non-negative version number.</param>
        private static int GetInt32(int major, int minor, int milli, int micro)
        {
            return (major << 24) | (minor << 16) | (milli << 8) | micro;
        }
        ///CLOVER:OFF
        /// <summary>
        /// Main method prints out ICU version information
        /// </summary>
        /// <param name="args">Arguments (currently not used).</param>
        /// <stable>ICU 4.6</stable>
        public static void PrintVersionInfo(string[] args) // ICU4N specific - was Main() in icu4j
        {
            string icuApiVer;

            if (ICU_VERSION.Major <= 4)
            {
                if (ICU_VERSION.Minor % 2 != 0)
                {
                    // Development mile stone
                    int major = ICU_VERSION.Major;
                    int minor = ICU_VERSION.Minor + 1;
                    if (minor >= 10)
                    {
                        minor -= 10;
                        major++;
                    }
                    icuApiVer = "" + major + "." + minor + "M" + ICU_VERSION.Milli;
                }
                else
                {
#pragma warning disable 612, 618
                    icuApiVer = ICU_VERSION.GetVersionString(2, 2);
#pragma warning restore 612, 618
                }
            }
            else
            {
                if (ICU_VERSION.Minor == 0)
                {
                    // Development mile stone
                    icuApiVer = "" + ICU_VERSION.Major + "M" + ICU_VERSION.Milli;
                }
                else
                {
#pragma warning disable 612, 618
                    icuApiVer = ICU_VERSION.GetVersionString(2, 2);
#pragma warning restore 612, 618
                }
            }


            Console.Out.WriteLine("International Components for Unicode for Java " + icuApiVer);

            Console.Out.WriteLine("");
#pragma warning disable 612, 618
            Console.Out.WriteLine("Implementation Version: " + ICU_VERSION.GetVersionString(2, 4));
            Console.Out.WriteLine("Unicode Data Version:   " + UNICODE_VERSION.GetVersionString(2, 4));
            Console.Out.WriteLine("CLDR Data Version:      " + LocaleData.GetCLDRVersion().GetVersionString(2, 4));
#pragma warning restore 612, 618
            Console.Out.WriteLine("Time Zone Data Version: " + GetTZDataVersion());
        }

        /// <summary>
        /// Generate version string separated by dots with
        /// the specified digit width.  Version digit 0
        /// after <paramref name="minDigits"/> will be trimmed off.
        /// </summary>
        /// <param name="minDigits">Minimum number of version digits.</param>
        /// <param name="maxDigits">Maximum number of version digits.</param>
        /// <returns>A tailored version string.</returns>
        /// <internal/>
        [Obsolete("This API is ICU internal only. (For use in CLDR, etc.)")]
        public string GetVersionString(int minDigits, int maxDigits)
        {
            if (minDigits < 1 || maxDigits < 1
                    || minDigits > 4 || maxDigits > 4 || minDigits > maxDigits)
            {
                throw new ArgumentException("Invalid min/maxDigits range");
            }

            int[] digits = new int[4];
            digits[0] = Major;
            digits[1] = Minor;
            digits[2] = Milli;
            digits[3] = Micro;

            int numDigits = maxDigits;
            while (numDigits > minDigits)
            {
                if (digits[numDigits - 1] != 0)
                {
                    break;
                }
                numDigits--;
            }

            StringBuilder verStr = new StringBuilder(7);
            verStr.Append(digits[0]);
            for (int i = 1; i < numDigits; i++)
            {
                verStr.Append(".");
                verStr.Append(digits[i]);
            }

            return verStr.ToString();
        }
        ///CLOVER:ON


        // Moved from TimeZone class
        private static volatile String TZDATA_VERSION = null;

        internal static string GetTZDataVersion()
        {
            if (TZDATA_VERSION == null)
            {
                lock (syncLock)
                {
                    if (TZDATA_VERSION == null)
                    {
                        UResourceBundle tzbundle = UResourceBundle.GetBundleInstance("Impl/Data/icudt"
#pragma warning disable 612, 618
                                + VersionInfo.ICU_DATA_VERSION_PATH, "zoneinfo64");
#pragma warning restore 612, 618
                        TZDATA_VERSION = tzbundle.GetString("TZVersion");
                    }
                }
            }
            return TZDATA_VERSION;
        }
    }
}
