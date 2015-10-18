// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Serialization
{
    public class InstagramUnixTimeConverterTests
    {
        private static readonly DateTime _Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static TheoryData<string> NullDateTimeValues
        {
            get
            {
                return new TheoryData<string>
                {
                    "{ \"Value\": null }",
                    "{ \"Value\": { } }",
                    "{ \"Value\": [ ] }",
                };
            }
        }

        public static TheoryData<string> InvalidDateTimeValues
        {
            get
            {
                return new TheoryData<string>
                {
                    "{ \"Value\": \"invalid\" }",
                    "{ \"Value\": 123456 }",
                    "{ \"Value\": 1.23456 }",
                    "{ \"Value\": true }",
                };
            }
        }

        public static TheoryData<long> ValidReadDateTimeValues
        {
            get
            {
                return new TheoryData<long>
                {
                   0,
                   1442710070,
                   -144271007,
                   int.MinValue,
                   int.MaxValue,
                };
            }
        }

        public static TheoryData<DateTime, string> ValidWriteDateTimeValues
        {
            get
            {
                return new TheoryData<DateTime, string>
                {
                    { new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), "{\"Value\":\"0\"}" },
                    { new DateTime(1960, 1, 1, 0, 0, 0, DateTimeKind.Utc), "{\"Value\":\"-315619200\"}" },
                    { new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc), "{\"Value\":\"1420070400\"}" },
                };
            }
        }

        public static TheoryData<DateTime> RoundtripValues
        {
            get
            {
                return new TheoryData<DateTime>
                {
                    new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(1960, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                };
            }
        }

        [Theory]
        [MemberData("NullDateTimeValues")]
        public void ReadJson_ThrowsOnNull(string input)
        {
            // Act
            InvalidOperationException iex = Assert.Throws<InvalidOperationException>(() => JsonConvert.DeserializeObject<TestClass>(input));

            // Assert
            Assert.Equal("Cannot convert null value to type 'DateTime'.", iex.Message);
        }

        [Theory]
        [MemberData("InvalidDateTimeValues")]
        public void ReadJson_ThrowsOnInvalidValue(string input)
        {
            // Act
            InvalidOperationException iex = Assert.Throws<InvalidOperationException>(() => JsonConvert.DeserializeObject<TestClass>(input));

            // Assert
            Assert.StartsWith("Cannot read value '", iex.Message);
        }

        [Theory]
        [MemberData("ValidReadDateTimeValues")]
        public void ReadJson_ReadsValue_AsUtc(long delta)
        {
            // Arrange
            DateTime expected = _Epoch.AddSeconds(delta);
            string input = string.Format("{{ \"Value\": \"{0}\" }}", delta);

            // Act
            TestClass actual = JsonConvert.DeserializeObject<TestClass>(input);
            DateTime utcActual = actual.Value.ToUniversalTime();

            // Assert            
            Assert.Equal(expected, actual.Value);
            Assert.Equal(expected, utcActual);
        }

        [Theory]
        [MemberData("ValidWriteDateTimeValues")]
        public void WriteJson_WritesValue_AsUtc(DateTime input, string expected)
        {
            // Arrange
            TestClass data = new TestClass() { Value = input };

            // Act
            string actual = JsonConvert.SerializeObject(data);

            // Assert            
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("RoundtripValues")]
        public void WriteJson_ReadJson_Roundtrips(DateTime expected)
        {
            // Arrange
            TestClass data = new TestClass() { Value = expected };

            // Act
            string serialized = JsonConvert.SerializeObject(data);
            TestClass actualData = JsonConvert.DeserializeObject<TestClass>(serialized);
            DateTime actual = actualData.Value;

            // Assert            
            Assert.Equal(expected, actual);
        }

        private class TestClass
        {
            [JsonConverter(typeof(InstagramUnixTimeConverter))]
            public DateTime Value { get; set; }
        }
    }
}
