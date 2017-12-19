using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ICU4N.Dev.Test.StringPrep
{
    using StringPrep = ICU4N.Text.StringPrep;

    /// <author>ram</author>
    public class TestStringPrep : TestFmwk
    {
        /*
       There are several special identifiers ("who") which need to be
       understood universally, rather than in the context of a particular
       DNS domain.  Some of these identifiers cannot be understood when an
       NFS client accesses the server, but have meaning when a local process
       accesses the file.  The ability to display and modify these
       permissions is permitted over NFS, even if none of the access methods
       on the server understands the identifiers.

        Who                    Description
       _______________________________________________________________

       "OWNER"                The owner of the file.
       "GROUP"                The group associated with the file.
       "EVERYONE"             The world.
       "INTERACTIVE"          Accessed from an interactive terminal.
       "NETWORK"              Accessed via the network.
       "DIALUP"               Accessed as a dialup user to the server.
       "BATCH"                Accessed from a batch job.
       "ANONYMOUS"            Accessed without any authentication.
       "AUTHENTICATED"        Any authenticated user (opposite of
                              ANONYMOUS)
       "SERVICE"              Access from a system service.

       To avoid conflict, these special identifiers are distinguish by an
       appended "@" and should appear in the form "xxxx@" (note: no domain
       name after the "@").  For example: ANONYMOUS@.
    */
        private String[] mixed_prep_data ={
        "OWNER@",
        "GROUP@",
        "EVERYONE@",
        "INTERACTIVE@",
        "NETWORK@",
        "DIALUP@",
        "BATCH@",
        "ANONYMOUS@",
        "AUTHENTICATED@",
        "\u0930\u094D\u092E\u094D\u0915\u094D\u0937\u0947\u0924\u094D@slip129-37-118-146.nc.us.ibm.net",
        "\u0936\u094d\u0930\u0940\u092e\u0926\u094d@saratoga.pe.utexas.edu",
        "\u092d\u0917\u0935\u0926\u094d\u0917\u0940\u0924\u093e@dial-120-45.ots.utexas.edu",
        "\u0905\u0927\u094d\u092f\u093e\u092f@woo-085.dorms.waller.net",
        "\u0905\u0930\u094d\u091c\u0941\u0928@hd30-049.hil.compuserve.com",
        "\u0935\u093f\u0937\u093e\u0926@pem203-31.pe.ttu.edu",
        "\u092f\u094b\u0917@56K-227.MaxTNT3.pdq.net",
        "\u0927\u0943\u0924\u0930\u093e\u0937\u094d\u091f\u094d\u0930@dial-36-2.ots.utexas.edu",
        "\u0909\u0935\u093E\u091A\u0943@slip129-37-23-152.ga.us.ibm.net",
        "\u0927\u0930\u094d\u092e\u0915\u094d\u0937\u0947\u0924\u094d\u0930\u0947@ts45ip119.cadvision.com",
        "\u0915\u0941\u0930\u0941\u0915\u094d\u0937\u0947\u0924\u094d\u0930\u0947@sdn-ts-004txaustP05.dialsprint.net",
        "\u0938\u092e\u0935\u0947\u0924\u093e@bar-tnt1s66.erols.com",
        "\u092f\u0941\u092f\u0941\u0924\u094d\u0938\u0935\u0903@101.st-louis-15.mo.dial-access.att.net",
        "\u092e\u093e\u092e\u0915\u093e\u0903@h92-245.Arco.COM",
        "\u092a\u093e\u0923\u094d\u0921\u0935\u093e\u0936\u094d\u091a\u0948\u0935@dial-13-2.ots.utexas.edu",
        "\u0915\u093f\u092e\u0915\u0941\u0930\u094d\u0935\u0924@net-redynet29.datamarkets.com.ar",
        "\u0938\u0902\u091c\u0935@ccs-shiva28.reacciun.net.ve",
        "\u0c30\u0c18\u0c41\u0c30\u0c3e\u0c2e\u0c4d@7.houston-11.tx.dial-access.att.net",
        "\u0c35\u0c3f\u0c36\u0c4d\u0c35\u0c28\u0c3e\u0c27@ingw129-37-120-26.mo.us.ibm.net",
        "\u0c06\u0c28\u0c02\u0c26\u0c4d@dialup6.austintx.com",
        "\u0C35\u0C26\u0C4D\u0C26\u0C3F\u0C30\u0C3E\u0C1C\u0C41@dns2.tpao.gov.tr",
        "\u0c30\u0c3e\u0c1c\u0c40\u0c35\u0c4d@slip129-37-119-194.nc.us.ibm.net",
        "\u0c15\u0c36\u0c30\u0c2c\u0c3e\u0c26@cs7.dillons.co.uk.203.119.193.in-addr.arpa",
        "\u0c38\u0c02\u0c1c\u0c40\u0c35\u0c4d@swprd1.innovplace.saskatoon.sk.ca",
        "\u0c15\u0c36\u0c30\u0c2c\u0c3e\u0c26@bikini.bologna.maraut.it",
        "\u0c38\u0c02\u0c1c\u0c40\u0c2c\u0c4d@node91.subnet159-198-79.baxter.com",
        "\u0c38\u0c46\u0c28\u0c4d\u0c17\u0c41\u0c2a\u0c4d\u0c24@cust19.max5.new-york.ny.ms.uu.net",
        "\u0c05\u0c2e\u0c30\u0c47\u0c02\u0c26\u0c4d\u0c30@balexander.slip.andrew.cmu.edu",
        "\u0c39\u0c28\u0c41\u0c2e\u0c3e\u0c28\u0c41\u0c32@pool029.max2.denver.co.dynip.alter.net",
        "\u0c30\u0c35\u0c3f@cust49.max9.new-york.ny.ms.uu.net",
        "\u0c15\u0c41\u0c2e\u0c3e\u0c30\u0c4d@s61.abq-dialin2.hollyberry.com",
        "\u0c35\u0c3f\u0c36\u0c4d\u0c35\u0c28\u0c3e\u0c27@\u0917\u0928\u0947\u0936.sanjose.ibm.com",
        "\u0c06\u0c26\u0c3f\u0c24\u0c4d\u0c2f@www.\u00E0\u00B3\u00AF.com",
        "\u0C15\u0C02\u0C26\u0C4D\u0C30\u0C47\u0C17\u0C41\u0c32@www.\u00C2\u00A4.com",
        "\u0c36\u0c4d\u0c30\u0c40\u0C27\u0C30\u0C4D@www.\u00C2\u00A3.com",
        "\u0c15\u0c02\u0c1f\u0c2e\u0c36\u0c46\u0c1f\u0c4d\u0c1f\u0c3f@\u0025",
        "\u0c2e\u0c3e\u0c27\u0c35\u0c4d@\u005C\u005C",
        "\u0c26\u0c46\u0c36\u0c46\u0c1f\u0c4d\u0c1f\u0c3f@www.\u0021.com",
        "test@www.\u0024.com",
        "help@\u00C3\u00BC.com",
    };
        [Test]
        public void TestNFS4MixedPrep()
        {
            for (int i = 0; i < mixed_prep_data.Length; i++)
            {
                try
                {
                    String src = mixed_prep_data[i];
                    byte[] dest = NFS4StringPrep.MixedPrepare(Encoding.UTF8.GetBytes(src));
                    String destString = Encoding.UTF8.GetString(dest);
                    int destIndex = destString.IndexOf('@');
                    if (destIndex < 0)
                    {
                        Errln("Delimiter @ disappeared from the output!");
                    }
                }
                catch (Exception e)
                {
                    Errln("mixed_prepare for string: " + mixed_prep_data[i] + " failed with " + e.ToString());
                }
            }
            /* test the error condition */
            {
                String src = "OWNER@oss.software.ibm.com";
                try
                {
                    byte[] dest = NFS4StringPrep.MixedPrepare(Encoding.UTF8.GetBytes(src));
                    if (dest != null)
                    {
                        Errln("Did not get the expected exception");
                    }
                }
                catch (Exception e)
                {
                    Logln("mixed_prepare for string: " + src + " passed with " + e.ToString());
                }

            }
        }
        [Test]
        public void TestCISPrep()
        {

            for (int i = 0; i < (TestData.conformanceTestCases.Length); i++)
            {
                TestData.ConformanceTestCase testCase = TestData.conformanceTestCases[i];
                String src = testCase.input;
                Exception expected = testCase.expected;
                String expectedDest = testCase.output;
                try
                {
                    byte[] dest = NFS4StringPrep.CISPrepare(Encoding.UTF8.GetBytes(src));
                    String destString = Encoding.UTF8.GetString(dest);
                    if (!expectedDest.Equals(destString, StringComparison.OrdinalIgnoreCase))
                    {
                        Errln("Did not get the expected output for nfs4_cis_prep at index " + i);
                    }
                }
                catch (Exception e)
                {
                    if (expected != null && !expected.Equals(e))
                    {
                        Errln("Did not get the expected exception: " + e.ToString());
                    }
                }

            }
        }

        [Test]
        public void TestCSPrep()
        {

            // Checking for bidi is turned off
            String src = "\uC138\uACC4\uC758\uBAA8\uB4E0\uC0AC\uB78C\uB4E4\uC774\u0644\u064A\u0647\uD55C\uAD6D\uC5B4\uB97C\uC774\uD574\uD55C\uB2E4\uBA74";
            try
            {
                NFS4StringPrep.CSPrepare(Encoding.UTF8.GetBytes(src), false);
            }
            catch (Exception e)
            {
                Errln("Got unexpected exception: " + e.ToString());
            }

            // normalization is turned off
            try
            {
                src = "www.\u00E0\u00B3\u00AF.com";
                byte[] dest = NFS4StringPrep.CSPrepare(Encoding.UTF8.GetBytes(src), false);
                String destStr = Encoding.UTF8.GetString(dest);
                if (!src.Equals(destStr))
                {
                    Errln("Did not get expected output. Expected: " + Prettify(src) +
                          " Got: " + Prettify(destStr));
                }
            }
            catch (Exception e)
            {
                Errln("Got unexpected exception: " + e.ToString());
            }

            // test case insensitive string
            try
            {
                src = "THISISATEST";
                byte[] dest = NFS4StringPrep.CSPrepare(Encoding.UTF8.GetBytes(src), false);
                String destStr = Encoding.UTF8.GetString(dest);
                if (!src.ToLowerInvariant().Equals(destStr))
                {
                    Errln("Did not get expected output. Expected: " + Prettify(src) +
                          " Got: " + Prettify(destStr));
                }
            }
            catch (Exception e)
            {
                Errln("Got unexpected exception: " + e.ToString());
            }
            // test case sensitive string
            try
            {
                src = "THISISATEST";
                byte[] dest = NFS4StringPrep.CSPrepare(Encoding.UTF8.GetBytes(src), true);
                String destStr = Encoding.UTF8.GetString(dest);
                if (!src.Equals(destStr))
                {
                    Errln("Did not get expected output. Expected: " + Prettify(src) +
                          " Got: " + Prettify(destStr));
                }
            }
            catch (Exception e)
            {
                Errln("Got unexpected exception: " + e.ToString());
            }
        }

        [Test]
        public void TestCoverage()
        {
            if (new StringPrepParseException("coverage", 0, "", 0, 0) == null)
            {
                Errln("Construct StringPrepParseException(String, int, String, int, int)");
            }
        }

        /* Tests the method public static StringPrep getInstance(int profile) */
        [Test]
        public void TestGetInstance()
        {
            // Tests when "if (profile < 0 || profile > MAX_PROFILE)" is true
            int[] neg_num_cases = { -100, -50, -10, -5, -2, -1 };
            for (int i = 0; i < neg_num_cases.Length; i++)
            {
                try
                {
                    StringPrep.GetInstance((StringPrepProfile)neg_num_cases[i]);
                    Errln("StringPrep.GetInstance(int) expected an exception for " +
                            "an invalid parameter of " + neg_num_cases[i]);
                }
                catch (Exception e)
                {
                }
            }

            StringPrepProfile[] max_profile_cases = { StringPrepProfile.Rfc4518LdapCaseInsensitive + 1, StringPrepProfile.Rfc4518LdapCaseInsensitive + 2, StringPrepProfile.Rfc4518LdapCaseInsensitive + 5, StringPrepProfile.Rfc4518LdapCaseInsensitive + 10 };
            for (int i = 0; i < max_profile_cases.Length; i++)
            {
                try
                {
                    StringPrep.GetInstance(max_profile_cases[i]);
                    Errln("StringPrep.GetInstance(int) expected an exception for " +
                            "an invalid parameter of " + max_profile_cases[i]);
                }
                catch (Exception e)
                {
                }
            }

            // Tests when "if (instance == null)", "if (stream != null)", "if (instance != null)", and "if (ref != null)" is true
            int[] cases = { 0, 1, (int)StringPrepProfile.Rfc4518LdapCaseInsensitive };
            for (int i = 0; i < cases.Length; i++)
            {
                try
                {
                    StringPrep.GetInstance((StringPrepProfile)cases[i]);
                }
                catch (Exception e)
                {
                    Errln("StringPrep.GetInstance(int) did not expected an exception for " +
                            "an valid parameter of " + cases[i]);
                }
            }
        }

        /* Test the method public String prepare(String src, int options) */
        [Test]
        public void TestPrepare()
        {
            StringPrep sp = StringPrep.GetInstance(0);
            try
            {
                if (!(sp.Prepare("dummy", 0)).Equals("dummy"))
                {
                    Errln("StringPrep.prepare(String,int) was suppose to return " + "'dummy'");
                }
            }
            catch (Exception e)
            {
                Errln("StringPrep.prepare(String,int) was not suppose to return " + "an exception.");
            }
        }

        /*
         * Tests the constructor public StringPrepParseException(String message, int error, String rules, int pos, int
         * lineNumber)
         */
        [Test]
        public void TestStringPrepParseException()
        {
            CultureInfo[] locales = { new CultureInfo("en-US"), new CultureInfo("fr"), new CultureInfo("zh-Hans") };
            String rules = "This is a very odd little set of rules, just for testing, you know...";
            StringPrepParseException[] exceptions = new StringPrepParseException[locales.Length];

            for (int i = 0; i < locales.Length; i += 1)
            {
                exceptions[i] = new StringPrepParseException(locales[i].ToString(), i, rules, i, i);
            }
        }

        /* Tests the method public bool equals(Object other) for StringPrepParseException */
        [Test]
        public void TestStringPrepParseExceptionEquals()
        {
            StringPrepParseException sppe = new StringPrepParseException("dummy", 0, "dummy", 0, 0);
            StringPrepParseException sppe_clone = new StringPrepParseException("dummy", 0, "dummy", 0, 0);
            StringPrepParseException sppe1 = new StringPrepParseException("dummy1", 1, "dummy1", 0, 0);

            // Tests when "if(!(other instanceof StringPrepParseException))" is true
            if (sppe.Equals(0))
            {
                Errln("StringPrepParseException.Equals(Object) is suppose to return false when " +
                        "passing integer '0'");
            }
            if (sppe.Equals(0.0))
            {
                Errln("StringPrepParseException.Equals(Object) is suppose to return false when " +
                        "passing float/double '0.0'");
            }
            if (sppe.Equals("0"))
            {
                Errln("StringPrepParseException.Equals(Object) is suppose to return false when " +
                        "passing string '0'");
            }

            // Tests when "if(!(other instanceof StringPrepParseException))" is true
            if (!sppe.Equals(sppe))
            {
                Errln("StringPrepParseException.Equals(Object) is suppose to return true when " +
                "comparing to the same object");
            }
            if (!sppe.Equals(sppe_clone))
            {
                Errln("StringPrepParseException.Equals(Object) is suppose to return true when " +
                "comparing to the same initiated object");
            }
            if (sppe.Equals(sppe1))
            {
                Errln("StringPrepParseException.Equals(Object) is suppose to return false when " +
                "comparing to another object that isn't the same");
            }
        }

        /* Tests the method public int getError() */
        [Test]
        public void TestGetError()
        {
            for (int i = 0; i < 5; i++)
            {
                StringPrepParseException sppe = new StringPrepParseException("dummy", i, "dummy", 0, 0);
                if (sppe.Error != i)
                {
                    Errln("StringPrepParseExcpetion.getError() was suppose to return " + i + " but got " + sppe.Error);
                }
            }
        }

        /* Tests the private void setPreContext(char[] str, int pos) */
        [Test]
        public void TestSetPreContext()
        {
            String WordAtLeast16Characters = "abcdefghijklmnopqrstuvwxyz";
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    StringPrepParseException sppe = new StringPrepParseException("dummy", i, WordAtLeast16Characters, 0, 0);
                    sppe = new StringPrepParseException(WordAtLeast16Characters, i, "dummy", 0, 0);
                }
                catch (Exception e)
                {
                    Errln("StringPrepParseException.setPreContext was not suppose to return an exception");
                }
            }
        }
    }
}
