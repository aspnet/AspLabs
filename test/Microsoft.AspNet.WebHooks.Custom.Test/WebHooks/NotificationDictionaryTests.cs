// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.TestUtilities;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class NotificationDictionaryTests
    {
        private const string TestAction = "testAction";

        private readonly NotificationDictionary _notification;

        public NotificationDictionaryTests()
        {
            _notification = new NotificationDictionary(TestAction, data: null);
        }

        public static TheoryData<string, object> CustomProperties
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

        public static TheoryData<string> UnknownKeys
        {
            get
            {
                return new TheoryData<string>
                {
                    "unknown",
                    "你好世界",
                    string.Empty,
                };
            }
        }

        [Fact]
        public void Action_Roundtrips()
        {
            PropertyAssert.Roundtrips(_notification, n => n.Action, PropertySetter.NullThrows, defaultValue: TestAction, roundtripValue: "你好世界");
        }

        [Fact]
        public void Data_Initializes_FromDictionary()
        {
            // Arrange
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "data1", 1234 },
                { "data2", "你好世界" },
            };
            NotificationDictionary not = new NotificationDictionary(TestAction, data);

            // Act
            int actual1 = (int)not["data1"];
            string actual2 = (string)not["data2"];

            // Assert
            Assert.Equal(1234, actual1);
            Assert.Equal("你好世界", actual2);
        }

        [Fact]
        public void Data_Initializes_FromAnonymousType()
        {
            NotificationDictionary not = new NotificationDictionary(TestAction, new { data1 = 1234, data2 = "你好世界" });

            // Act
            int actual1 = (int)not["data1"];
            string actual2 = (string)not["data2"];

            // Assert
            Assert.Equal(1234, actual1);
            Assert.Equal("你好世界", actual2);
        }

        [Fact]
        public void Data_Initializes_FromNull()
        {
            // Assert
            Assert.Equal(TestAction, _notification.Single().Value);
        }

        [Theory]
        [MemberData("CustomProperties")]
        public void Item_Roundtrips(string key, object value)
        {
            // Arrange
            _notification.Add(key, value);

            // Act
            object actual = _notification[key];

            // Assert
            Assert.Equal(value, actual);
        }

        [Theory]
        [MemberData("UnknownKeys")]
        public void Item_Throws_IfKeyNotFound(string key)
        {
            KeyNotFoundException ex = Assert.Throws<KeyNotFoundException>(() => _notification[key]);

            Assert.Equal(string.Format(CultureInfo.CurrentCulture, "No Notification setting was found with key '{0}'.", key), ex.Message);
        }

        [Theory]
        [MemberData("UnknownKeys")]
        public void Item_OnInterface_Throws_IfKeyNotFound(string key)
        {
            IDictionary<string, object> dictionary = (IDictionary<string, object>)_notification;
            KeyNotFoundException ex = Assert.Throws<KeyNotFoundException>(() => dictionary[key]);

            Assert.Contains("The given key was not present in the dictionary.", ex.Message);
        }
    }
}
