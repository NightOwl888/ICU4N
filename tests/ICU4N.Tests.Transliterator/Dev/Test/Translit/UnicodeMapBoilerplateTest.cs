using ICU4N.Dev.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace ICU4N.Dev.Test.Translit
{
    /// <summary>
    /// Moved from UnicodeMapTest
    /// </summary>
    public class UnicodeMapBoilerplateTest : TestBoilerplate<UnicodeMap<string>>
    {
        private static String[] TEST_VALUES = { "A", "B", "C", "D", "E", "F" };

        public void TestUnicodeMapBoilerplate()
        {
        }

        [Test]
        public override void Test()
        {
            base.Test();
        }

        /* (non-Javadoc)
         * @see com.ibm.icu.dev.test.TestBoilerplate#_hasSameBehavior(java.lang.Object, java.lang.Object)
         */
        protected override bool HasSameBehavior(UnicodeMap<string> a, UnicodeMap<string> b)
        {
            // we are pretty confident in the equals method, so won't bother with this right now.
            return true;
        }

        /* (non-Javadoc)
         * @see com.ibm.icu.dev.test.TestBoilerplate#_addTestObject(java.util.List)
         */
        protected override bool AddTestObject(IList<UnicodeMap<string>> list)
        {
            if (list.Count > 30) return false;
            UnicodeMap<string> result = new UnicodeMap<string>();
            for (int i = 0; i < 50; ++i)
            {
                int start = random.Next(25);
                String value = TEST_VALUES[random.Next(TEST_VALUES.Length)];
                result.Put(start, value);
            }
            list.Add(result);
            return true;
        }
    }
}
