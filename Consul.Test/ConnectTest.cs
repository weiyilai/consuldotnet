// -----------------------------------------------------------------------
//  <copyright file="ConnectTest.cs" company="G-Research Limited">
//    Copyright 2020 G-Research Limited
//
//    Licensed under the Apache License, Version 2.0 (the "License"),
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Versioning;
using Xunit;

namespace Consul.Test
{
    public class ConnectTest : BaseFixture
    {
        [Fact]
        public async Task Connect_CARoots()
        {
            var req = await _client.Connect.CARoots();
            var result = req.Response;
            var root = result.Roots.First();
            Assert.Equal("11111111-2222-3333-4444-555555555555.consul", result.TrustDomain);
            Assert.NotEmpty(result.Roots);
            Assert.NotEmpty(result.ActiveRootID);
            Assert.NotEmpty(root.RootCert);
            Assert.NotEmpty(root.Name);
            Assert.NotNull(root.RootCert);
            Assert.NotNull(root.SigningKeyID);
        }

        [Fact]
        public async Task Connect_GetCAConfigurationTest()
        {
            var req = await _client.Connect.CAGetConfig();
            var result = req.Response;

            Assert.Equal("consul", result.Provider);
            Assert.NotEmpty(result.Config);
            Assert.False(result.ForceWithoutCrossSigning);
            Assert.NotEqual((ulong)0, result.CreateIndex);
            Assert.NotEqual((ulong)0, result.ModifyIndex);
        }

        [SkippableFact]
        public async Task Connect_CASetConfig()
        {
            var cutOffVersion = SemanticVersion.Parse("1.7.0");
            Skip.If(AgentVersion < cutOffVersion, $"Current version is {AgentVersion}, but setting CA config is only supported from Consul {cutOffVersion}");

            var req = await _client.Connect.CAGetConfig();
            var config = req.Response;

            config.Config["test_state"] = new Dictionary<string, string> { { "foo", "bar" } };
            config.Config["PrivateKey"] = "";

            await _client.Connect.CASetConfig(config);
            req = await _client.Connect.CAGetConfig();
            var updatedConfig = req.Response;

            Assert.Equal("consul", updatedConfig.Provider);
            Assert.Equal("bar", updatedConfig.State["foo"]);
            Assert.Equal("", updatedConfig.Config["PrivateKey"]);
        }
    }
}
