// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.TestUtilities;
using Xunit;

namespace System.Collections.Generic
{
    public class DictionaryExtensionsTests
    {
        private const string TestKey = "newkey";

        private readonly Dictionary<string, object> _strObjDict = new Dictionary<string, object>();

        public static TheoryData<string, object> TestData
        {
            get
            {
                return new TheoryData<string, object>
                {
                    { "key", "你好" },
                    { "key", string.Empty },
                    { "key", "value" },
                    { "你好世界", 1 },
                    { "你好世界", 1.23 },
                    { "你好世界", Guid.NewGuid() },
                    { string.Empty, new Uri("http://localhost") },
                    { string.Empty, DayOfWeek.Friday },
                    { string.Empty, new List<int>() },
                };
            }
        }

        [Theory]
        [MemberData("TestData")]
        public void DictionaryStringObject_TryGetValue_FindsValue<T>(string key, T value)
        {
            // Arrange
            _strObjDict.Add(key, value);

            // Act
            T actual;
            bool result = DictionaryExtensions.TryGetValue(_strObjDict, key, out actual);

            // Assert
            Assert.True(result);
            Assert.Equal(value, actual);
        }

        [Theory]
        [MemberData("TestData")]
        public void DictionaryStringObject_TryGetValue_ReturnsDefaultValue_IfNotFound<T>(string key, T value)
        {
            // Arrange
            T actual;

            // Act
            bool result = _strObjDict.TryGetValue(key, out actual);

            // Assert
            Assert.NotEqual(default(T), value);
            Assert.False(result);
            Assert.Equal(default(T), actual);
        }

        [Theory]
        [MemberData("TestData")]
        public void DictionaryStringObject_TryGetValue_ReturnsDefaultValue_IfWrongType<T>(string key, T value)
        {
            // Arrange
            T actual;
            _strObjDict.Add(key, Tuple.Create(1, 2));

            // Act
            bool result = _strObjDict.TryGetValue("key", out actual);

            // Assert
            Assert.NotEqual(default(T), value);
            Assert.False(result);
            Assert.Equal(default(T), actual);
        }

        [Theory]
        [MemberData("TestData")]
        public void DictionaryStringObject_GetValueOrDefault_FindsValue<T>(string key, T value)
        {
            // Arrange
            _strObjDict.Add(key, value);

            // Act
            T actual = DictionaryExtensions.GetValueOrDefault<T>(_strObjDict, key);

            // Assert
            Assert.Equal(value, actual);
        }

        [Theory]
        [MemberData("TestData")]
        public void DictionaryStringObject_GetValueOrDefault_ReturnsDefaultValue_IfNotFound<T>(string key, T value)
        {
            // Act
            T actual = _strObjDict.GetValueOrDefault<T>(key);

            // Assert
            Assert.NotEqual(default(T), value);
            Assert.Equal(default(T), actual);
        }

        [Theory]
        [MemberData("TestData")]
        public void DictionaryStringObject_GetValueOrDefault_ReturnsDefaultValue_IfWrongType<T>(string key, T value)
        {
            // Arrange
            _strObjDict.Add(key, Tuple.Create(1, 2));

            // Act
            T actual = _strObjDict.GetValueOrDefault<T>(key);

            // Assert
            Assert.NotEqual(default(T), value);
            Assert.Equal(default(T), actual);
        }

        [Theory]
        [MemberData("MixedInstancesDataSet", null, MemberType = typeof(TestDataSets))]
        public void DictionaryStringObject_SetOrClearValue_ClearsEntry_IfDefaultValue<T>(T nonDefaultValue)
        {
            // Arrange
            _strObjDict[TestKey] = nonDefaultValue;

            // Act
            _strObjDict.SetOrClearValue(TestKey, default(T));

            // Assert
            Assert.False(_strObjDict.ContainsKey(TestKey));
        }

        [Theory]
        [MemberData("TestData")]
        public void DictionaryStringObject_SetOrClearValue_SetsEntry<T>(string key, T value)
        {
            // Act
            _strObjDict.SetOrClearValue(key, value);

            // Assert
            Assert.Equal(value, _strObjDict[key]);
        }
    }
}
