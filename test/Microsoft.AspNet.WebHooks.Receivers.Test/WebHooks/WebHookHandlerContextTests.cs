// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.TestUtilities;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookHandlerContextTests
    {
        private List<string> _actions;
        private WebHookHandlerContext _context;

        public WebHookHandlerContextTests()
        {
            _actions = new List<string> { "a1" };
            _context = new WebHookHandlerContext(_actions);
        }

        [Fact]
        public void Data_Roundtrips()
        {
            PropertyAssert.Roundtrips(_context, c => c.Data, PropertySetter.NullRoundtrips, roundtripValue: "你好世界");
        }

        [Fact]
        public void Request_Roundtrips()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();

            // Act/Assert
            PropertyAssert.Roundtrips(_context, c => c.Request, PropertySetter.NullRoundtrips, roundtripValue: request);
        }

        [Fact]
        public void RequestContext_Roundtrips()
        {
            // Arrange
            HttpRequestContext requestContext = new HttpRequestContext();

            // Act/Assert
            PropertyAssert.Roundtrips(_context, c => c.RequestContext, PropertySetter.NullRoundtrips, roundtripValue: requestContext);
        }

        [Fact]
        public void Response_Roundtrips()
        {
            // Arrange
            HttpResponseMessage response = new HttpResponseMessage();

            // Act/Assert
            PropertyAssert.Roundtrips(_context, c => c.Response, PropertySetter.NullRoundtrips, roundtripValue: response);
        }

        [Fact]
        public void Actions_Roundtrip()
        {
            // Act
            string actual = _context.Actions.Single();

            // Assert
            Assert.Equal("a1", actual);
        }
    }
}
