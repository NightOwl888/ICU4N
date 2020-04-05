using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Globalization;

//
// Port From:   ICU4C v2.1 : Collate/CollationTurkishTest
// Source File: $ICU4CRoot/source/test/intltest/trcoll.cpp
//
namespace ICU4N.Dev.Test.Collate
{
    public class CollationChineseTest : TestFmwk
    {
        public CollationChineseTest()
        {
        }

        [Test]
        public void TestPinYin()
        {
            String[] seq
                = {"\u963f", "\u554a", "\u54ce", "\u6371", "\u7231", "\u9f98",
               "\u4e5c", "\u8baa", "\u4e42", "\u53c8"};
            RuleBasedCollator collator = null;
            try
            {
                collator = (RuleBasedCollator)Collator.GetInstance(
                                                // ICU4N: See: https://stackoverflow.com/questions/9416435/what-culture-code-should-i-use-for-pinyin#comment11937203_9421566
                                                new CultureInfo("zh-Hans")); //("zh", "", "PINYIN")); // ICU4N TODO: Can we replicate the 3rd parameter somehow?
            }
            catch (Exception e)
            {
                Warnln("ERROR: in creation of collator of zh__PINYIN locale");
                return;
            }
            for (int i = 0; i < seq.Length - 1; i++)
            {
                CollationTest.DoTest(this, collator, seq[i], seq[i + 1], -1);
            }
        }
    }
}
