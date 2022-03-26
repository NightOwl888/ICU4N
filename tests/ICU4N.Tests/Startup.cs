using ICU4N.Configuration;
using ICU4N.Configuration.Custom;
using J2N.Threading.Atomic;
using NUnit.Framework;

namespace ICU4N
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

    [SetUpFixture]
    public class Startup
    {
        internal static AtomicInt32 initializationCount = new AtomicInt32(0);
        internal static bool initilizationReset = false;

        public Startup()
        {
            initilizationReset = initializationCount.GetAndSet(0) != 0;
        }

        [OneTimeSetUp]
        public void OneTimeSetUpBeforeTests()
        {
            // Decorate the existing configuration factory with mock settings
            // so we don't interfere with the operation of the test framework.
            ConfigurationSettings.SetConfigurationFactory(new MockConfigurationFactory(ConfigurationSettings.GetConfigurationFactory()));
            initializationCount.IncrementAndGet();
        }

        [OneTimeTearDown]
        public void OneTimeTearDownAfterTests()
        {

        }
    }
}
