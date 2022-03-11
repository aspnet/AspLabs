// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.Internal;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace System.Web
{
    public class HttpResponseTests
    {
        [Fact]
        public void Headers()
        {
            // Arrange
            var headersCore = new HeaderDictionary();

            var responseCore = new Mock<HttpResponseCore>();
            responseCore.Setup(r => r.Headers).Returns(headersCore);

            var response = new HttpResponse(responseCore.Object);

            // Act
            var headers1 = response.Headers;
            var headers2 = response.Headers;

            // Assert
            Assert.Same(headers1, headers2);
            Assert.IsType<StringValuesDictionaryNameValueCollection>(headers1);
        }
    }
}
