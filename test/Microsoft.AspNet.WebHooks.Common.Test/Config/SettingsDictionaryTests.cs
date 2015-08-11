// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Config
{
    public class SettingsDictionaryTests
    {
        private readonly SettingsDictionary _settings = new SettingsDictionary();

        public static TheoryData<string, string> CustomSettings
        {
            get
            {
                return new TheoryData<string, string>
                {
                    { "key", null },
                    { "key", string.Empty },
                    { "key", "value" },
                    { "你好世界", null },
                    { "你好世界", string.Empty },
                    { "你好世界", "你好" },
                    { string.Empty, null },
                    { string.Empty, string.Empty },
                    { string.Empty, "你好" },
                };
            }
        }

        public static TheoryData<string> UnknownKeys
        {
            get
            {
                return new TheoryData<string>
                {
                    "key",
                    "你好世界",
                    string.Empty,
                    "unknown"
                };
            }
        }

        [Theory]
        [MemberData("CustomSettings")]
        public void CustomSetting_Roundtrips(string key, string value)
        {
            // Arrange
            _settings.Add(key, value);

            // Act
            string actual = _settings[key];

            // Assert
            Assert.Equal(value, actual);
        }

        [Theory]
        [MemberData("CustomSettings")]
        public void Item_Roundtrips(string key, string value)
        {
            // Arrange
            _settings[key] = value;

            // Act
            string actual = _settings.GetValueOrDefault(key);

            // Assert
            Assert.Equal(value, actual);
        }

        [Theory]
        [MemberData("UnknownKeys")]
        public void Item_Throws_KeyNotFoundException(string key)
        {
            KeyNotFoundException ex = Assert.Throws<KeyNotFoundException>(() => _settings[key]);

            Assert.Contains(string.Format(CultureInfo.CurrentCulture, "No WebHook setting was found with key '{0}'. Please ensure that the WebHooks module is initialized with the correct application settings.", key), ex.Message);
        }

        [Theory]
        [MemberData("UnknownKeys")]
        public void Item_OnInterface_Throws_KeyNotFoundException(string key)
        {
            IDictionary<string, string> dictionary = (IDictionary<string, string>)_settings;
            KeyNotFoundException ex = Assert.Throws<KeyNotFoundException>(() => dictionary[key]);

            Assert.Contains("The given key was not present in the dictionary.", ex.Message);
        }

        [Theory]
        [MemberData("CustomSettings")]
        public void SetOrClearValue_SetsNonNullValue(string key, string value)
        {
            // Act
            _settings.SetOrClearValue(key, value);

            // Assert
            if (value == null)
            {
                Assert.False(_settings.ContainsKey(key));
            }
            else
            {
                Assert.Equal(value, _settings[key]);
            }
        }
    }
}
