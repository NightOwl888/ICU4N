using NUnit.Framework;
using System.Collections.Generic;
using System.Text;

namespace ICU4N.Dev.Test.Translit
{
    public class StringBoilerplateTest : TestBoilerplate<string>
    {
        public void TestStringBoilerplate()
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
        protected override bool HasSameBehavior(string a, string b)
        {
            // we are pretty confident in the equals method, so won't bother with this right now.
            return true;
        }

        protected override bool IsMutable(string a)
        {
            // ICU4N: strings are always immutable in .NET
            return false;
        }

        /* (non-Javadoc)
         * @see com.ibm.icu.dev.test.TestBoilerplate#_addTestObject(java.util.List)
         */
        protected override bool AddTestObject(IList<string> list)
        {
            if (list.Count > 31) return false;
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < 10; ++i)
            {
                result.Append((char)random.Next(0xFF));
            }
            list.Add(result.ToString());
            return true;
        }
    }
}
