// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.WindowsAzure.Storage.Table;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Extensions
{
    public class TableResultExtensionsTests
    {
        private readonly TableResult _result;

        public TableResultExtensionsTests()
        {
            _result = new TableResult();
        }

        [Theory]
        [InlineData(199, false)]
        [InlineData(200, true)]
        [InlineData(299, true)]
        [InlineData(300, false)]
        public void IsSuccess_DetectsCorrectly(int status, bool expected)
        {
            // Arrange
            _result.HttpStatusCode = status;

            // Act
            bool actual = _result.IsSuccess();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(403, false)]
        [InlineData(404, true)]
        [InlineData(405, false)]
        public void IsNotFound_DetectsCorrectly(int status, bool expected)
        {
            // Arrange
            _result.HttpStatusCode = status;

            // Act
            bool actual = _result.IsNotFound();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(408, false)]
        [InlineData(409, true)]
        [InlineData(410, false)]
        [InlineData(411, false)]
        [InlineData(412, true)]
        [InlineData(413, false)]
        public void IsConflict_DetectsCorrectly(int status, bool expected)
        {
            // Arrange
            _result.HttpStatusCode = status;

            // Act
            bool actual = _result.IsConflict();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(499, false)]
        [InlineData(500, true)]
        [InlineData(599, true)]
        [InlineData(600, false)]
        public void IsServerError_DetectsCorrectly(int status, bool expected)
        {
            // Arrange
            _result.HttpStatusCode = status;

            // Act
            bool actual = _result.IsServerError();

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
