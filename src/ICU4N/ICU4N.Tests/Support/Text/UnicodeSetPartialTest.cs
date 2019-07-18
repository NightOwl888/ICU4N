using ICU4N.Dev.Test;
using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICU4N.Support.Text
{
    public class UnicodeSetPartialTest : TestFmwk
    {
        [Test]
        public void TestSetEquals()
        {
            // Initialize UnicodeSets
            var thaiWordSet = new UnicodeSet();
            var thaiWordSet2 = new UnicodeSet();
            var burmeseWordSet = new UnicodeSet();


            burmeseWordSet.ApplyPattern("[[:Mymr:]&[:LineBreak=SA:]]");
            burmeseWordSet.Compact();

            thaiWordSet.ApplyPattern("[[:Thai:]&[:LineBreak=SA:]]");
            thaiWordSet.Compact();
            thaiWordSet2.ApplyPattern("[[:Thai:]&[:LineBreak=SA:]]");
            thaiWordSet2.Compact();

            assertTrue("UnicodeSet.SetEquals: The word sets are not equal", thaiWordSet.SetEquals(thaiWordSet2));
            assertTrue("UnicodeSet.SetEquals: The word sets are not equal", thaiWordSet2.SetEquals(thaiWordSet));
            assertFalse("UnicodeSet.SetEquals: The word sets are equal", thaiWordSet.SetEquals(burmeseWordSet));
            var equivSet = new List<string>();
            thaiWordSet.AddAllTo(equivSet);
            assertTrue("UnicodeSet.SetEquals: The word sets are not equal", thaiWordSet.SetEquals(equivSet));
        }
    }
}
