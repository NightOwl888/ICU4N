using ICU4N.Globalization;
using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Linq;

namespace ICU4N.Dev.Test.Format
{
    public class NumberFormatRegistrationTest : TestFmwk
    {

        internal static readonly UCultureInfo SRC_LOC = new UCultureInfo("fr-FR");// ULocale.FRANCE;
        internal static readonly UCultureInfo SWAP_LOC = new UCultureInfo("en-US");// ULocale.US;

        class TestFactory : SimpleNumberFormatFactory
        {
            NumberFormat currencyStyle;

            public TestFactory()
                : this(SRC_LOC, SWAP_LOC)
            {
            }

            public TestFactory(UCultureInfo srcLoc, UCultureInfo swapLoc)
                        : base(srcLoc)
            {
                currencyStyle = NumberFormat.GetIntegerInstance(swapLoc);
            }

            public override NumberFormat CreateFormat(UCultureInfo loc, int formatType)
            {
                if (formatType == FormatCurrency /*FORMAT_CURRENCY*/)
                {
                    return currencyStyle;
                }
                return null;
            }
        }

        [Test]
        public void TestRegistration()
        {
            {
                // coverage before registration

                try
                {
                    NumberFormat.Unregister(null);
                    Errln("did not throw exception on null unregister");
                }
                catch (Exception e)
                {
                    Logln("PASS: null unregister failed as expected");
                }

                try
                {
                    NumberFormat.RegisterFactory(null);
                    Errln("did not throw exception on null register");
                }
                catch (Exception e)
                {
                    Logln("PASS: null register failed as expected");
                }

                try
                {
                    // if no NF has been registered yet, shim is null, so this silently
                    // returns false.  if, on the other hand, a NF has been registered,
                    // this will try to cast the argument to a Factory, and throw
                    // an exception.
                    if (NumberFormat.Unregister(""))
                    {
                        Errln("unregister of empty string key succeeded");
                    }
                }
                catch (Exception e)
                {
                }
            }

            UCultureInfo fu_FU = new UCultureInfo("fu_FU");
            NumberFormat f0 = NumberFormat.GetIntegerInstance(SWAP_LOC);
            NumberFormat f1 = NumberFormat.GetIntegerInstance(SRC_LOC);
            NumberFormat f2 = NumberFormat.GetCurrencyInstance(SRC_LOC);
            Object key = NumberFormat.RegisterFactory(new TestFactory());
            Object key2 = NumberFormat.RegisterFactory(new TestFactory(fu_FU, new UCultureInfo("de-DE")));
            if (!NumberFormat.GetUCultures(UCultureTypes.AllCultures).Contains(fu_FU))
            {
                Errln("did not list fu_FU");
            }
            NumberFormat f3 = NumberFormat.GetCurrencyInstance(SRC_LOC);
            NumberFormat f4 = NumberFormat.GetIntegerInstance(SRC_LOC);
            NumberFormat.Unregister(key); // restore for other tests
            NumberFormat f5 = NumberFormat.GetCurrencyInstance(SRC_LOC);

            NumberFormat.Unregister(key2);

            float n = 1234.567f;
            Logln("f0 swap int: " + f0.Format(n));
            Logln("f1 src int: " + f1.Format(n));
            Logln("f2 src cur: " + f2.Format(n));
            Logln("f3 reg cur: " + f3.Format(n));
            Logln("f4 reg int: " + f4.Format(n));
            Logln("f5 unreg cur: " + f5.Format(n));

            if (!f3.Format(n).Equals(f0.Format(n), StringComparison.Ordinal))
            {
                Errln("registered service did not match");
            }
            if (!f4.Format(n).Equals(f1.Format(n), StringComparison.Ordinal))
            {
                Errln("registered service did not inherit");
            }
            if (!f5.Format(n).Equals(f2.Format(n), StringComparison.Ordinal))
            {
                Errln("unregistered service did not match original");
            }

            // coverage
            NumberFormat f6 = NumberFormat.GetNumberInstance(fu_FU);
            if (f6 == null)
            {
                Errln("getNumberInstance(fu_FU) returned null");
            }
        }
    }
}
