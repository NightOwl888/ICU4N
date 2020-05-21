//    /*
//     * Licensed to the Apache Software Foundation (ASF) under one or more
//     * contributor license agreements.  See the NOTICE file distributed with
//     * this work for additional information regarding copyright ownership.
//     * The ASF licenses this file to You under the Apache License, Version 2.0
//     * (the "License"); you may not use this file except in compliance with
//     * the License.  You may obtain a copy of the License at
//     *
//     *     http://www.apache.org/licenses/LICENSE-2.0
//     *
//     * Unless required by applicable law or agreed to in writing, software
//     * distributed under the License is distributed on an "AS IS" BASIS,
//     * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     * See the License for the specific language governing permissions and
//     * limitations under the License.
//     */

#if !NET40 
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ICU4N.Configuration
{
    /*
     * Licensed to the Apache Software Foundation (ASF) under one or more
     * contributor license agreements.  See the NOTICE file distributed with
     * this work for additional information regarding copyright ownership.
     * The ASF licenses this file to You under the Apache License, Version 2.0
     * (the "License"); you may not use this file except in compliance with
     * the License.  You may obtain a copy of the License at
     *
     *     http://www.apache.org/licenses/LICENSE-2.0
     *
     * Unless required by applicable law or agreed to in writing, software
     * distributed under the License is distributed on an "AS IS" BASIS,
     * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     * See the License for the specific language governing permissions and
     * limitations under the License.
     */

    internal class TestConfigurationRootFactory : DefaultConfigurationRootFactory
    {
        private readonly ConcurrentDictionary<string, IConfigurationRoot> configurationCache = new ConcurrentDictionary<string, IConfigurationRoot>();

        public string JsonTestSettingsFileName { get; set; } = "lucene.testsettings.json";

        public IConfigurationBuilder builder { get; }

        public TestConfigurationRootFactory() : base(false)
        {

            //configurationBuilder.AddEnvironmentVariables();
            //configurationBuilder.AddXmlFilesFromRootDirectoryTo(currentPath, defaultXmlConfigurationFilename);
            //configurationBuilder.AddJsonFilesFromRootDirectoryTo(currentPath, defaultJsonConfigurationFilename);
            //configurationBuilder.Add(new TestParameterConfigurationSource(NUnit.Framework.TestContext.Parameters));

            //this.builder = configurationBuilder;
        }

        public override IConfigurationRoot CreateConfiguration()
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();

            string testDirectory =
#if TESTFRAMEWORK_NUNIT
                NUnit.Framework.TestContext.CurrentContext.TestDirectory;
#elif NETSTANDARD
                System.AppContext.BaseDirectory;
#else
                AppDomain.CurrentDomain.BaseDirectory;
#endif

            return configurationCache.GetOrAdd(testDirectory, (key) =>
            {
                return new ConfigurationBuilder()
                    .Add(new ICU4NDefaultConfigurationSource() { Prefix = "lucene:" })
                    //.AddJsonFile(JsonTestSettingsFileName)
#if TESTFRAMEWORK_NUNIT
                    .AddNUnitTestRunSettings()
#endif
                    .Build();
            });
        }
        ///// <summary>
        ///// Initializes the dependencies of this factory.
        ///// </summary>
        //[CLSCompliant(false)]
        //protected override IConfiguration Initialize()
        //{
        //    return builder.Build();
        //}
    }

}
#endif