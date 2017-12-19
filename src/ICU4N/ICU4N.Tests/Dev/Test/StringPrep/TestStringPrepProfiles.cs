using ICU4N.Text;
using ICU4N.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Dev.Test.StringPrep
{
    using StringPrep = ICU4N.Text.StringPrep;

    /// <author>Michael Ow</author>
    public class TestStringPrepProfiles : TestFmwk
    {
        /*
     * The format of the test cases should be the following:
     * {
     *     Profile name
     *     src string1
     *     expected result1
     *     src string2
     *     expected result2
     *     ...
     * }
     *
     * *Note: For expected failures add FAIL to beginning of the source string and for expected result use "FAIL".
     */
        private static string[][] testCases = {
                new string[] {
                    "RFC4013_SASLPREP",
                    "user:\u00A0\u0AC6\u1680\u00ADpassword1",
                    "user: \u0AC6 password1"
                },
                new string[] {
                    "RFC4011_MIB",
                    "Policy\u034F\u200DBase\u0020d\u1806\u200C",
                    "PolicyBase d"
                },
                new string[] {
                    "RFC4505_TRACE",
                    "Anony\u0020\u00A0mous\u3000\u0B9D\u034F\u00AD",
                    "Anony\u0020\u00A0mous\u3000\u0B9D\u034F\u00AD"
                },
                new string[] {
                    "RFC4518_LDAP",
                    "Ldap\uFB01\u00ADTest\u0020\u00A0\u2062ing",
                    "LdapfiTest  ing"
                },
                new string[] {
                    "RFC4518_LDAP_CI",
                    "Ldap\uFB01\u00ADTest\u0020\u00A0\u2062ing12345",
                    "ldapfitest  ing12345"
                },
                new string[] {
                    "RFC3920_RESOURCEPREP",
                    "ServerXM\u2060\uFE00\uFE09PP s p ",
                    "ServerXMPP s p "
                },
                new string[] {
                    "RFC3920_NODEPREP",
                    "Server\u200DXMPPGreEK\u03D0",
                    "serverxmppgreek\u03B2"
                },
                new string[] {
                    "RFC3722_ISCSI",
                    "InternetSmallComputer\uFB01\u0032\u2075Interface",
                    "internetsmallcomputerfi25interface",
                    "FAILThisShouldFailBecauseOfThis\u002F",
                    "FAIL"
                },
                new string[] {
                    "RFC3530_NFS4_CS_PREP",
                    "\u00ADUser\u2060Name@ \u06DDDOMAIN.com",
                    "UserName@ \u06DDDOMAIN.com"
                },
                new string[] {
                    "RFC3530_NFS4_CS_PREP_CI",
                    "\u00ADUser\u2060Name@ \u06DDDOMAIN.com",
                    "username@ \u06DDdomain.com"
                },
                new string[] {
                    "RFC3530_NFS4_CIS_PREP",
                    "AA\u200C\u200D @@DomAin.org",
                    "aa @@domain.org"
                },
                new string[] {
                    "RFC3530_NFS4_MIXED_PREP_PREFIX",
                    "PrefixUser \u007F\uFB01End",
                    "PrefixUser \u007FfiEnd"
                },
                new string[] {
                    "RFC3530_NFS4_MIXED_PREP_SUFFIX",
                    "SuffixDomain \u007F\uFB01EnD",
                    "suffixdomain \u007Ffiend"
                }
            };

        private StringPrepProfile GetOptionFromProfileName(String profileName)
        {
            if (profileName.Equals("RFC4013_SASLPREP"))
            {
                return StringPrepProfile.Rfc4013SaslPrep;
            }
            else if (profileName.Equals("RFC4011_MIB"))
            {
                return StringPrepProfile.Rfc4011MIB;
            }
            else if (profileName.Equals("RFC4505_TRACE"))
            {
                return StringPrepProfile.Rfc4505Trace;
            }
            else if (profileName.Equals("RFC4518_LDAP"))
            {
                return StringPrepProfile.Rfc4518Ldap;
            }
            else if (profileName.Equals("RFC4518_LDAP_CI"))
            {
                return StringPrepProfile.Rfc4518LdapCaseInsensitive;
            }
            else if (profileName.Equals("RFC3920_RESOURCEPREP"))
            {
                return StringPrepProfile.Rfc3920ResourcePrep;
            }
            else if (profileName.Equals("RFC3920_NODEPREP"))
            {
                return StringPrepProfile.Rfc3920NodePrep;
            }
            else if (profileName.Equals("RFC3722_ISCSI"))
            {
                return StringPrepProfile.Rfc3722iSCSI;
            }
            else if (profileName.Equals("RFC3530_NFS4_CS_PREP"))
            {
                return StringPrepProfile.Rfc3530Nfs4CsPrep;
            }
            else if (profileName.Equals("RFC3530_NFS4_CS_PREP_CI"))
            {
                return StringPrepProfile.Rfc3530Nfs4CsPrepCaseInsensitive;
            }
            else if (profileName.Equals("RFC3530_NFS4_CIS_PREP"))
            {
                return StringPrepProfile.Rfc3530Nfs4CisPrep;
            }
            else if (profileName.Equals("RFC3530_NFS4_MIXED_PREP_PREFIX"))
            {
                return StringPrepProfile.Rfc3530Nfs4MixedPrepPrefix;
            }
            else if (profileName.Equals("RFC3530_NFS4_MIXED_PREP_SUFFIX"))
            {
                return StringPrepProfile.Rfc3530Nfs4MixedPrepSuffix;
            }

            // Should not happen.
            return (StringPrepProfile)(-1);
        }

        [Test]
        public void TestProfiles()
        {
            String profileName = null;
            StringPrep sprep = null;
            String result = null;
            String src = null;
            String expected = null;

            for (int i = 0; i < testCases.Length; i++)
            {
                for (int j = 0; j < testCases[i].Length; j++)
                {
                    if (j == 0)
                    {
                        profileName = testCases[i][j];

                        sprep = StringPrep.GetInstance(GetOptionFromProfileName(profileName));
                    }
                    else
                    {
                        src = testCases[i][j];
                        expected = testCases[i][++j];
                        try
                        {
                            result = sprep.Prepare(src, StringPrepOptions.AllowUnassigned);
                            if (src.StartsWith("FAIL", StringComparison.Ordinal))
                            {
                                Errln("Failed: Expected error for Test[" + i + "] Profile: " + profileName);
                            }
                            else if (!result.Equals(expected))
                            {
                                Errln("Failed: Test[" + i + "] Result string does not match expected string for StringPrep test for profile: " + profileName);
                            }
                        }
                        catch (StringPrepParseException ex)
                        {
                            if (!src.StartsWith("FAIL", StringComparison.Ordinal))
                            {
                                Errln("Failed: Test[" + i + "] StringPrep profile " + profileName + " got error: " + ex);
                            }
                        }
                    }
                }
            }
        }
    }
}
