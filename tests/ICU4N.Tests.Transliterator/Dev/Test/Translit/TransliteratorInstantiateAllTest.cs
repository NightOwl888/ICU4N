using ICU4N.Text;
using NUnit.Framework;
using System;
using System.Collections;

namespace ICU4N.Dev.Test.Translit
{
    /// <summary>
    /// This test class uses NUnit parametrization to iterate over all
    /// transliterators and to execute a sample operation.
    /// </summary>
    public class TransliteratorInstantiateAllTest : TestFmwk
    {
        public static IEnumerable TestData
        {
            get
            {
                foreach (var e in Transliterator.GetAvailableIDs())
                {
                    yield return e;
                }
            }
        }

        [Test, TestCaseSource(typeof(TransliteratorInstantiateAllTest), "TestData")]
        public void TestInstantiation(string testTransliteratorID)
        {
            Transliterator t = null;

            try
            {
                t = Transliterator.GetInstance(testTransliteratorID);
            }
            catch (ArgumentException ex)
            {
                Errln("FAIL: " + testTransliteratorID);
                throw ex;
            }

            //if (t != null)
            //{
            //    // Test toRules
            //    String rules = null;
            //    try
            //    {
            //        rules = t.ToRules(true);
            //        Transliterator.CreateFromRules("x", rules, Transliterator.Forward);
            //    }
            //    catch (ArgumentException ex2)
            //    {
            //        Errln("FAIL: " + "ID" + ".toRules() => bad rules: " +
            //              rules);
            //        throw ex2;
            //    }
            //}
        }
    }
}
