// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.TestUtilities;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookHandlerContextExtensionsTests
    {
        private string[] _actions;
        private WebHookHandlerContext _context;

        public WebHookHandlerContextExtensionsTests()
        {
            _actions = new string[] { "a1", "a2" };
            _context = new WebHookHandlerContext(_actions);
        }

        public static TheoryDataCollection<object> Data
        {
            get
            {
                return new TheoryDataCollection<object>
                {
                    new NameValueCollection(),
                    new JObject(),
                    "text",
                    new string[] { "A", "B", "C" },
                    new List<int> { 1, 2, 3 },
                    new Uri("http://localhost")
                };
            }
        }

        [Theory]
        [MemberData("Data")]
        public void GetDataOrDefault_ReturnsExpectedData<T>(T data)
            where T : class
        {
            // Arrange
            _context.Data = data;

            // Act
            T actual = _context.GetDataOrDefault<T>();

            // Assert
            Assert.Same(data, actual);
        }

        [Fact]
        public void GetDataOrDefault_HandlesNullContext()
        {
            // Act
            object actual = WebHookHandlerContextExtensions.GetDataOrDefault<string>(null);

            // Assert
            Assert.Null(actual);
        }

        [Theory]
        [MemberData("Data")]
        public void TryGetData_ReturnsExpectedData<T>(T data)
            where T : class
        {
            // Arrange
            T actual;
            _context.Data = data;

            // Act
            bool result = _context.TryGetData<T>(out actual);

            // Assert
            Assert.Equal(actual != null, result);
        }

        [Fact]
        public void TryGetData_HandlesNullContext()
        {
            // Arrange
            string output;

            // Act
            bool actual = WebHookHandlerContextExtensions.TryGetData<string>(null, out output);

            // Assert
            Assert.False(actual);
        }
    }
}
