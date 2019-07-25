using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Dev.Test.Translit
{
    /// <summary>
    /// Moved from UnicodeMapTest
    /// </summary>
    public class UnicodeSetBoilerplateTest : TestBoilerplate<UnicodeSet>
    {
        public void TestUnicodeSetBoilerplate()
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
        protected override bool HasSameBehavior(UnicodeSet a, UnicodeSet b)
        {
            // we are pretty confident in the equals method, so won't bother with this right now.
            return true;
        }

        /* (non-Javadoc)
         * @see com.ibm.icu.dev.test.TestBoilerplate#_addTestObject(java.util.List)
         */
        protected override bool AddTestObject(IList<UnicodeSet> list)
        {
            if (list.Count > 32) return false;
            UnicodeSet result = new UnicodeSet();
            for (int i = 0; i < 50; ++i)
            {
                result.Add(random.Next(100));
            }
            list.Add(result);
            return true;
        }
    }
}
