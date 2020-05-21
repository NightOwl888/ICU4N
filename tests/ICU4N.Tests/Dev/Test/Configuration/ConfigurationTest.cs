
#if !NET40
using ICU4N.Configuration;
using ICU4N.Support;
using ICU4N.Util;
using NUnit.Framework;
using After = NUnit.Framework.TearDownAttribute;

namespace ICU4N.Dev.Test.Configuration
{
    /// <summary>
    /// Test MessagePatternUtil (MessagePattern-as-tree-of-nodes API)
    /// by building parallel trees of nodes and verifying that they match.
    /// </summary>
    public class ConfigurationTest : TestFmwk
    {
        /// <summary>
        /// For subclasses to override. Overrides must call <c>base.TearDown()</c>.
        /// </summary>
        [After]
#pragma warning disable xUnit1013
        public virtual void TearDown()
#pragma warning restore xUnit1013
        {
            ConfigurationSettings.CurrentConfiguration.Reload();
        }

        [Test]
        public virtual void EnvironmentTest2()
        {
            string testKey = "lucene:tests:setting";
            string testValue = "test.success";
            ICU4N.Configuration.ConfigurationSettings.CurrentConfiguration[testKey] = testValue;
            Assert.AreEqual(ICU4N.Configuration.ConfigurationSettings.CurrentConfiguration[testKey], testValue);
            Assert.AreEqual(testValue, SystemProperties.GetProperty(testKey));
        }
        [Test]
        public virtual void SetTest()
        {
            Assert.AreEqual("fr", ICU4N.Configuration.ConfigurationSettings.CurrentConfiguration["tests:locale"]);
            Assert.AreEqual("fr", SystemProperties.GetProperty("tests:locale"));
            ICU4N.Configuration.ConfigurationSettings.CurrentConfiguration["tests:locale"] = "en";
            Assert.AreEqual("en", ICU4N.Configuration.ConfigurationSettings.CurrentConfiguration["tests:locale"]);
            Assert.AreEqual("en", SystemProperties.GetProperty("tests:locale"));
        }


        [Test]
        public virtual void TestCommandLineProperty()
        {
            TestContext.Progress.WriteLine("TestContext.Parameters ({0})", TestContext.Parameters.Count);
            foreach (var x in TestContext.Parameters.Names)
                TestContext.Progress.WriteLine(string.Format("{0}={1}", x, TestContext.Parameters[x]));
        }

        [Test]
        public virtual void TestCachedConfigProperty()
        {
            Assert.AreEqual("0x00000010", ICU4N.Configuration.ConfigurationSettings.CurrentConfiguration["tests:seed"]);
            //Assert.AreEqual(0xf6a5c420, (uint)StringHelper.Murmurhash3_x86_32(new BytesRef("foo"), 0));
            //Assert.AreEqual(16, ICU4N.Configuration.ConfigurationSettings.CurrentConfiguration["test.seed"));
            //// Hashes computed using murmurTR3_32 from https://code.google.com/p/pyfasthash
            //Assert.AreEqual(0xcd018ef6, (uint)StringHelper.Murmurhash3_x86_32(new BytesRef("foo"), StringHelper.GOOD_FAST_HASH_SEED));
        }

        [Test]
        public void TestCreateCollator()
        {
            //ICU4N.Text.LocaleDisplayNames
            //string testKey = "ICU4N.Text.LocaleDisplayNames.impl";
            //string testValue = "ICU4N.Text.LocaleDisplayNamesImpl";
            //ConfigurationSettings.GetConfigurationFactory().CreateConfiguration()[testKey] = testValue;
            //Assert.AreEqual(testValue, ConfigurationSettings.GetConfigurationFactory().CreateConfiguration()[testKey]);
            //Assert.AreEqual(testValue, SystemProperties.GetProperty(testKey));
            var us = new ULocale("en_US").GetDisplayName(new ULocale("jp_JP"));



        }
    }
}
#endif