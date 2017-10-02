// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookHandlerContextExtensionsTests
    {
        private WebHookHandlerContext _context;

        public WebHookHandlerContextExtensionsTests()
        {
            var actions = new string[] { "a1", "a2" };
            _context = new WebHookHandlerContext(actions);
        }

        public static TheoryData<object> Data
        {
            get
            {
                return new TheoryData<object>
                {
                    new NameValueCollection(),
                    new JArray(),
                    new JObject(),
                    (JValue)42,
                    "text",
                    new string[] { "A", "B", "C" },
                    new List<int> { 1, 2, 3 },
                    new Uri("http://localhost")
                };
            }
        }

        public static TheoryData<JToken> InvalidJArrayData
        {
            get
            {
                var test = new TestClass { Age = 1024, Name = "Henrik" };

                return new TheoryData<JToken>
                {
                    JObject.FromObject(test),
                    "42",
                    42,
                };
            }
        }

        public static TheoryData<JToken> InvalidJObjectData
        {
            get
            {
                var tests = new List<TestClass>
                {
                    new TestClass { Age = 1, Name = "Henrik1" },
                    new TestClass { Age = 2, Name = "Henrik2" },
                };

                return new TheoryData<JToken>
                {
                    JArray.FromObject(tests),
                    "42",
                    42
                };
            }
        }

        public static TheoryData<JToken> InvalidJValueData
        {
            get
            {
                var test = new TestClass { Age = 1024, Name = "Henrik" };
                var tests = new List<TestClass>
                {
                    new TestClass { Age = 1, Name = "Henrik1" },
                    new TestClass { Age = 2, Name = "Henrik2" },
                };

                return new TheoryData<JToken>
                {
                    JArray.FromObject(tests),
                    JObject.FromObject(test),
                };
            }
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void GetDataOrDefault_ReturnsExpectedData<T>(T data)
            where T : class
        {
            // Arrange
            _context.Data = data;

            // Act
            var actual = _context.GetDataOrDefault<T>();

            // Assert
            Assert.Same(data, actual);
        }

        [Fact]
        public void GetDataOrDefault_ReturnsTypeFromJObject()
        {
            // Arrange
            var test = new TestClass { Age = 1024, Name = "Henrik" };
            _context.Data = JObject.FromObject(test);

            // Act
            var actual = _context.GetDataOrDefault<TestClass>();

            // Assert
            Assert.Equal(1024, actual.Age);
            Assert.Equal("Henrik", actual.Name);
        }

        [Fact]
        public void GetDataOrDefault_ReturnsDictionaryFromJObject()
        {
            // Arrange
            var test = new TestClass { Age = 1024, Name = "Henrik" };
            var data = JObject.FromObject(test);
            _context.Data = data;
            var expected = ((IDictionary<string, JToken>)data).ToArray();

            // Act
            var actual = _context.GetDataOrDefault<IDictionary<string, JToken>>();

            // Assert (JObject implements IDictionary<string, JToken> but GetDataOrDefault returns a new object.)
            Assert.NotSame(data, actual);

            // Do not compare JObject and dictionary directly. JObject's non-generic IEnumerable implementation matches
            // its IList<JToken> implementation, not a collection of KeyValuePair<string, JToken> instances.
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetDataOrDefault_ReturnsTypeFromJArray()
        {
            // Arrange
            var test = new List<TestClass>
            {
                new TestClass { Age = 1, Name = "Henrik1" },
                new TestClass { Age = 2, Name = "Henrik2" },
            };
            _context.Data = JArray.FromObject(test);

            // Act
            var actual = _context.GetDataOrDefault<IEnumerable<TestClass>>();

            // Assert
            Assert.Equal(test, actual);
        }

        [Fact]
        public void GetDataOrDefault_ReturnsTypeFromJValue()
        {
            // Arrange
            var test = "this is the test";
            _context.Data = new JValue(test);

            // Act
            var actual = _context.GetDataOrDefault<string>();

            // Assert
            Assert.Same(test, actual);
        }

        [Theory]
        [MemberData(nameof(InvalidJArrayData))]
        public void GetDataOrDefault_ReturnsNullFromInvalidConversionToJArray(JToken data)
        {
            // Arrange
            _context.Data = data;

            // Act (Does not throw despite _context.RequestContext==null. Does not attempt conversion.)
            var actual = _context.GetDataOrDefault<JArray>();

            // Assert
            Assert.Null(actual);
        }

        [Theory]
        [MemberData(nameof(InvalidJObjectData))]
        public void GetDataOrDefault_ReturnsNullFromInvalidConversionToJObject(JToken data)
        {
            // Arrange
            _context.Data = data;

            // Act (Does not throw despite _context.RequestContext==null. Does not attempt conversion.)
            var actual = _context.GetDataOrDefault<JObject>();

            // Assert
            Assert.Null(actual);
        }

        [Theory]
        [MemberData(nameof(InvalidJValueData))]
        public void GetDataOrDefault_ReturnsNullFromInvalidConversionToJValue(JToken data)
        {
            // Arrange
            _context.Data = data;

            // Act (Does not throw despite _context.RequestContext==null. Does not attempt conversion.)
            var actual = _context.GetDataOrDefault<JValue>();

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetDataOrDefault_HandlesNullContext()
        {
            // Arrange & Act
            object actual = WebHookHandlerContextExtensions.GetDataOrDefault<string>(null);

            // Assert
            Assert.Null(actual);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void TryGetData_ReturnsExpectedData<T>(T data)
            where T : class
        {
            // Arrange
            _context.Data = data;

            // Act
            var result = _context.TryGetData<T>(out var actual);

            // Assert
            Assert.True(result);
            Assert.Same(data, actual);
        }

        [Fact]
        public void TryGetData_ReturnsTypeFromJObject()
        {
            // Arrange
            var test = new TestClass { Age = 1024, Name = "Henrik" };
            _context.Data = JObject.FromObject(test);

            // Act
            var result = _context.TryGetData<TestClass>(out var actual);

            // Assert
            Assert.True(result);
            Assert.Equal(1024, actual.Age);
            Assert.Equal("Henrik", actual.Name);
        }

        [Fact]
        public void TryGetData_ReturnsTypeFromJArray()
        {
            // Arrange
            var test = new List<TestClass>
            {
                new TestClass { Age = 1, Name = "Henrik1" },
                new TestClass { Age = 2, Name = "Henrik2" },
            };
            _context.Data = JArray.FromObject(test);

            // Act
            var result = _context.TryGetData<IEnumerable<TestClass>>(out var actual);

            // Assert
            Assert.True(result);
            Assert.Equal(test, actual);
        }

        [Fact]
        public void TryGetData_HandlesNullContext()
        {
            // Arrange & Act
            var actual = WebHookHandlerContextExtensions.TryGetData<string>(null, out var output);

            // Assert
            Assert.False(actual);
        }

        public class TestClass
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public override bool Equals(object obj)
            {
                var x = obj as TestClass;
                if (x == null)
                {
                    return false;
                }

                return Name == x.Name && Age == x.Age;
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode() ^ Age;
            }
        }
    }
}
