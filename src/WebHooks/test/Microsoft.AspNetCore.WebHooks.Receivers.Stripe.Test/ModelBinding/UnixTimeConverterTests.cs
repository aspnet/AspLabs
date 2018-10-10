// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNetCore.WebHooks.ModelBinding
{
    public class UnixTimeConverterTests
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
                    { new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), "{\"Value\":0}" },
                    { new DateTime(1960, 1, 1, 0, 0, 0, DateTimeKind.Utc), "{\"Value\":-315619200}" },
                    { new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc), "{\"Value\":1420070400}" }
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
        [MemberData(nameof(NullDateTimeValues))]
        public void ReadJson_ThrowsOnNull(string input)
        {
            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => JsonConvert.DeserializeObject<TestClass>(input));

            // Assert
            Assert.Equal("Cannot convert null value to type 'System.DateTime'.", ex.Message);
        }

        [Theory]
        [MemberData(nameof(InvalidDateTimeValues))]
        public void ReadJson_ThrowsOnInvalidValue(string input)
        {
            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => JsonConvert.DeserializeObject<TestClass>(input));

            // Assert
            Assert.StartsWith("Cannot read value '", ex.Message);
        }

        [Theory]
        [MemberData(nameof(ValidReadDateTimeValues))]
        public void ReadJson_ReadsValue_AsUtc(long delta)
        {
            // Arrange
            var expected = _Epoch.AddSeconds(delta);
            var input = string.Format("{{ \"Value\": \"{0}\" }}", delta);

            // Act
            var actual = JsonConvert.DeserializeObject<TestClass>(input);
            var utcActual = actual.Value.ToUniversalTime();

            // Assert
            Assert.Equal(expected, actual.Value);
            Assert.Equal(expected, utcActual);
        }

        [Theory]
        [MemberData(nameof(ValidWriteDateTimeValues))]
        public void WriteJson_WritesValue_AsUtc(DateTime input, string expected)
        {
            // Arrange
            var data = new TestClass() { Value = input };

            // Act
            var actual = JsonConvert.SerializeObject(data);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(RoundtripValues))]
        public void WriteJson_ReadJson_Roundtrips(DateTime expected)
        {
            // Arrange
            var data = new TestClass() { Value = expected };

            // Act
            var serialized = JsonConvert.SerializeObject(data);
            var actualData = JsonConvert.DeserializeObject<TestClass>(serialized);
            var actual = actualData.Value;

            // Assert
            Assert.Equal(expected, actual);
        }

        private class TestClass
        {
            [JsonConverter(typeof(UnixTimeConverter))]
            public DateTime Value { get; set; }
        }
    }
}
