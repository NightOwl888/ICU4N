using ICU4N.Dev.Test;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ICU4N.Configuration
{
    public class TestEnvironmentVariables : TestFmwk
    {
        private const string ConfigurationPrefix = "myPrefix:";
        private const string EnvironmentVariable = "someSetting";

        [Test]
        public void AddEnvironmentVariablesUsingNormalizedPrefix_Get_PrefixMatches()
        {
            try
            {
                Environment.SetEnvironmentVariable(ConfigurationPrefix + EnvironmentVariable, "myFooValue");

                var configuration = new ConfigurationRoot(new IConfigurationProvider[] {
                    new EnvironmentVariablesConfigurationProvider(prefix: ConfigurationPrefix, ignoreSecurityExceptionsOnRead: true),
                });

                Assert.AreEqual("myFooValue", configuration[EnvironmentVariable]);
            }
            finally
            {
                Environment.SetEnvironmentVariable(ConfigurationPrefix + EnvironmentVariable, null);
            }
        }
    }
}
