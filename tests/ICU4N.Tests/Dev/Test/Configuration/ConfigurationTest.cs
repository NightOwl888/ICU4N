using ICU4N.Text;
using ICU4N.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ICU4N.Text.MessagePatternUtil;

namespace ICU4N.Dev.Test.Format
{
    /// <summary>
    /// Test MessagePatternUtil (MessagePattern-as-tree-of-nodes API)
    /// by building parallel trees of nodes and verifying that they match.
    /// </summary>
    public class ConfigurationTest : TestFmwk
    {

        [Test]
        public void TestConfigurationJSONSettings()
        {
        }

        [Test]
        public virtual void EnvironmentTest2()
        {
            string testKey = "lucene:tests:setting";
            string testValue = "test.success";
            ConfigurationSettings.GetConfigurationFactory().CreateConfiguration()[testKey] = testValue;
            Assert.AreEqual(ConfigurationFactory.CreateConfiguration()[testKey], testValue);
        }
        [Test]
        public virtual void SetTest()
        {
            var test = ConfigurationFactory.CreateConfiguration();
            Assert.AreEqual("fr", ConfigurationFactory.CreateConfiguration()["tests:locale"]);
            //ConfigurationSettings.GetConfigurationFactory().CreateConfiguration()["tests:locale"] = "en";
            //Assert.AreEqual("en", ConfigurationFactory.CreateConfiguration()["tests:locale"]);
        }
    }
}
