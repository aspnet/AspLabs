// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookReceiverConfigTests
    {
        private ILogger _logger;

        public WebHookReceiverConfigTests()
        {
            _logger = new Mock<ILogger>().Object;
        }

        public static TheoryData<IDictionary<string, string>, string, string, string> ConfigData
        {
            get
            {
                return new TheoryData<IDictionary<string, string>, string, string, string>
                {
                    { new Dictionary<string, string> { { "ms_webhookreceiversecret_a", "b" } }, "a", null, "b" },
                    { new Dictionary<string, string> { { "ms_webhookreceiversecret_a", "b" } }, "a", "b", null },
                    { new Dictionary<string, string> { { "ms_webhookreceiversecret_a", "你好" } }, "a", null, "你好" },
                    { new Dictionary<string, string> { { "ms_webhookreceiversecret_a", "你好" } }, "b", null, null },
                    { new Dictionary<string, string> { { "ms_webhookreceiversecret_a", "b=c" } }, "a", "b", "c" },
                    { new Dictionary<string, string> { { "ms_webhookreceiversecret_a", "b=c" } }, "a", "c", null },
                    { new Dictionary<string, string> { { "ms_webhookreceiversecret_你好", "b" } }, "你好", null, "b" },
                    { new Dictionary<string, string> { { "ms_webhookreceiversecret_你好", "b" } }, "你好", string.Empty, "b" },
                    { new Dictionary<string, string> { { "ms_webhookreceiversecret_你好", "b=c" } }, "你好", "b", "c" },
                    { new Dictionary<string, string> { { "ms_webhookreceiversecret_你好", "b=c" } }, "你好", "c", null },
                };
            }
        }

        public static TheoryData<IDictionary<string, string>, IDictionary<string, string>> SettingsData
        {
            get
            {
                return new TheoryData<IDictionary<string, string>, IDictionary<string, string>>
                {
                    { new Dictionary<string, string>(), new Dictionary<string, string>() },
                    { new Dictionary<string, string> { { "k1", "v1" } }, new Dictionary<string, string>() },
                    { new Dictionary<string, string> { { "MS_WebHookReceiverSecret_", "secret1" } }, new Dictionary<string, string>() },
                    { new Dictionary<string, string> { { "ms_webhookreceiversecret_a", "secret1" } }, new Dictionary<string, string> { { "a/", "secret1" } } },
                    { new Dictionary<string, string> { { "MS_WEBHOOKRECEIVERSECRET_A", "secret1" } }, new Dictionary<string, string> { { "a/", "secret1" } } },
                    { new Dictionary<string, string> { { "MS_WebHookReceiverSecret_A", "secret1" } }, new Dictionary<string, string> { { "a/", "secret1" } } },
                    { new Dictionary<string, string> { { "MS_WebHookReceiverSecret_你好", "世界" } }, new Dictionary<string, string> { { "你好/", "世界" } } },
                    { new Dictionary<string, string> { { "k1", "v1" }, { "MS_WebHookReceiverSecret_你好", "世界" }, { "k2", "v2" } }, new Dictionary<string, string> { { "你好/", "世界" } } },
                    { new Dictionary<string, string> { { "k1", "v1" }, { "MS_WebHookReceiverSecret_你好", "a=1,世界=2,c=世界" }, { "k2", "v2" } }, new Dictionary<string, string> { { "你好/a", "1" }, { "你好/世界", "2" }, { "你好/c", "世界" } } },
                    { new Dictionary<string, string> { { "k1", "v1" }, { "MS_WebHookReceiverSecret_你好", "世界, , , , ,,,a=1  ,  世界=2 , c=世界" }, { "MS_WebHookReceiverSecret_A", "B=2" } }, new Dictionary<string, string> { { "你好/", "世界" }, { "你好/a", "1" }, { "你好/世界", "2" }, { "你好/c", "世界" }, { "a/b", "2" } } },
                };
            }
        }

        public static TheoryData<string, string, string> KeyData
        {
            get
            {
                return new TheoryData<string, string, string>
                {
                    { string.Empty, string.Empty, "/" },
                    { string.Empty, "A", "/a" },
                    { "A", string.Empty, "a/" },
                    { string.Empty, "世界", "/世界" },
                    { "世界", string.Empty, "世界/" },
                };
            }
        }

        [Theory]
        [MemberData("ConfigData")]
        public async Task GetReceiverConfigAsync_Returns_ExpectedValue(IDictionary<string, string> input, string receiver, string id, string expected)
        {
            // Arrange
            SettingsDictionary settings = GetSettings(input);
            WebHookReceiverConfig config = new WebHookReceiverConfig(settings, _logger);

            // Act
            string actual = await config.GetReceiverConfigAsync(receiver, id);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SettingsData")]
        public void ReadSettings_Parses_ValidValues(IDictionary<string, string> input, IDictionary<string, string> expected)
        {
            // Arrange
            SettingsDictionary settings = GetSettings(input);

            // Act
            IDictionary<string, string> actual = WebHookReceiverConfig.ReadSettings(settings, logger: null);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReadSettings_Throws_OnInvalidValues()
        {
            // Arrange
            SettingsDictionary settings = GetSettings(new Dictionary<string, string> { { "MS_WebHookReceiverSecret_A", "a=b=c" } });

            // Act
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => WebHookReceiverConfig.ReadSettings(settings, logger: null));

            // Assert
            Assert.Equal("The 'MS_WebHookReceiverSecret_A' application setting must have a comma-separated value of one or more secrets of the form <secret> or <id>=<secret>.", ex.Message);
        }

        [Theory]
        [MemberData("KeyData")]
        public void AddKey_AddsItem(string receiver, string id, string key)
        {
            // Arrange
            IDictionary<string, string> config = new Dictionary<string, string>();

            // Act
            WebHookReceiverConfig.AddKey(config, _logger, receiver, id, "Value");

            // Assert
            Assert.Equal("Value", config[key]);
        }

        [Fact]
        public void AddKey_ThrowsOnDuplicateKey()
        {
            // Arrange
            IDictionary<string, string> config = new Dictionary<string, string>();
            WebHookReceiverConfig.AddKey(config, _logger, "Receiver", "Id", "Value");

            // Act
            InvalidOperationException ioex = Assert.Throws<InvalidOperationException>(() => WebHookReceiverConfig.AddKey(config, _logger, "Receiver", "Id", "Value"));

            // Assert
            Assert.Equal("Could not add configuration for receiver 'Receiver' and id 'Id': An item with the same key has already been added.", ioex.Message);
        }

        [Theory]
        [MemberData("KeyData")]
        public void GetConfigKey(string receiver, string id, string expected)
        {
            // Act
            string actual = WebHookReceiverConfig.GetConfigKey(receiver, id);

            // Assert
            Assert.Equal(expected, actual);
        }

        private static SettingsDictionary GetSettings(IDictionary<string, string> values)
        {
            SettingsDictionary settings = new SettingsDictionary();
            settings.Connections.Add("name", new ConnectionSettings("name", "connectionstring"));
            foreach (KeyValuePair<string, string> value in values)
            {
                settings.Add(value.Key, value.Value);
            }
            return settings;
        }
    }
}
