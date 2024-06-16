using ICU4N.Text;
using J2N.Text;
using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using StringBuffer = System.Text.StringBuilder;

namespace ICU4N.Dev.Test.StringPrep
{
    using StringPrep = ICU4N.Text.StringPrep;

    /// <summary>
    /// This is a dumb implementation of NFS4 profiles. It is a direct port of
    /// C code, does not use Object Oriented principles. Quick and Dirty implementation
    /// for testing.
    /// </summary>
    /// <author>ram</author>
    public sealed class NFS4StringPrep
    {
        private StringPrep nfscss = null;
        private StringPrep nfscsi = null;
        private StringPrep nfscis = null;
        private StringPrep nfsmxp = null;
        private StringPrep nfsmxs = null;
        //singleton instance
        private static readonly NFS4StringPrep prep = new NFS4StringPrep();


        private NFS4StringPrep()
        {
#if FEATURE_TYPEEXTENSIONS_GETTYPEINFO
            Assembly loader = typeof(NFS4StringPrep).GetTypeInfo().Assembly;
#else
            Assembly loader = typeof(NFS4StringPrep).Assembly;
#endif
            try
            {
                string resourcePrefix = "ICU4N.Dev.Data.TestData.";

                using (Stream nfscsiFile = loader.GetManifestResourceStream(resourcePrefix + "nfscsi.spp"))
                    nfscsi = new StringPrep(nfscsiFile);


                using (Stream nfscssFile = loader.GetManifestResourceStream(resourcePrefix + "nfscss.spp"))
                    nfscss = new StringPrep(nfscssFile);


                using (Stream nfscisFile = loader.GetManifestResourceStream(resourcePrefix + "nfscis.spp"))
                    nfscis = new StringPrep(nfscisFile);


                using (Stream nfsmxpFile = loader.GetManifestResourceStream(resourcePrefix + "nfsmxp.spp"))
                    nfsmxp = new StringPrep(nfsmxpFile);


                using (Stream nfsmxsFile = loader.GetManifestResourceStream(resourcePrefix + "nfsmxs.spp"))
                    nfsmxs = new StringPrep(nfsmxsFile);

            }
            catch (IOException e)
            {
                throw new MissingManifestResourceException(e.ToString(), e);
            }
        }

        private static byte[] Prepare(byte[] src, StringPrep strprep)
        {
            // ICU4N: Factored out UCharacterIterator
            String s = Encoding.UTF8.GetString(src);
            string @out = strprep.Prepare(s, StringPrepOptions.Default);
            return Encoding.UTF8.GetBytes(@out);
        }

        public static byte[] CSPrepare(byte[] src, bool isCaseSensitive)
        {
            if (isCaseSensitive == true)
            {
                return Prepare(src, prep.nfscss);
            }
            else
            {
                return Prepare(src, prep.nfscsi);
            }
        }

        public static byte[] CISPrepare(byte[] src)
        {
            return Prepare(src, prep.nfscis);
        }

        /* sorted array for binary search*/
        private static readonly String[] special_prefixes ={
            "ANONYMOUS",
            "AUTHENTICATED",
            "BATCH",
            "DIALUP",
            "EVERYONE",
            "GROUP",
            "INTERACTIVE",
            "NETWORK",
            "OWNER",
        };


        /* binary search the sorted array */
        private static int FindStringIndex(String[] sortedArr, ReadOnlySpan<char> target)
        {

            int left, middle, right, rc;

            left = 0;
            right = sortedArr.Length - 1;

            while (left <= right)
            {
                middle = (left + right) / 2;
                rc = sortedArr[middle].CompareToOrdinal(target);

                if (rc < 0)
                {
                    left = middle + 1;
                }
                else if (rc > 0)
                {
                    right = middle - 1;
                }
                else
                {
                    return middle;
                }
            }
            return -1;
        }

        private const char AT_SIGN = '@';

        public static byte[] MixedPrepare(byte[] src)
        {
            string s = Encoding.UTF8.GetString(src);
            int index = s.IndexOf(AT_SIGN);
            ValueStringBuilder @out = new ValueStringBuilder(stackalloc char[32]);
            // ICU4N: Factored out UCharacterIterator
            if (index > -1)
            {
                /* special prefixes must not be followed by suffixes! */
                ReadOnlySpan<char> prefix = s.AsSpan(0, index); // ICU4N: Checked 2nd parameter
                int i = FindStringIndex(special_prefixes, prefix);
                ReadOnlySpan<char> suffix = s.AsSpan(index + 1, s.Length - (index + 1)); // ICU4N: Corrected 2nd parameter

                if (i > -1 && !suffix.IsEmpty)
                {
                    throw new StringPrepFormatException("Suffix following a special index", StringPrepErrorType.InvalidCharFound);
                }
                @out.Append(prep.nfsmxp.Prepare(prefix, StringPrepOptions.Default));
                @out.Append(AT_SIGN); // add the delimiter
                @out.Append(prep.nfsmxs.Prepare(suffix, StringPrepOptions.Default));
            }
            else
            {
                @out.Append(prep.nfsmxp.Prepare(s, StringPrepOptions.Default));

            }
            return Encoding.UTF8.GetBytes(@out.ToString());
        }
    }
}
