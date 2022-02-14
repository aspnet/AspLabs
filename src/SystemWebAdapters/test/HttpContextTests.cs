// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace System.Web.Adapters.Tests
{
    public class HttpContextTests
    {
        [Fact]
        public void ConstructorChecksNull()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpContext(null!));
        }

        [Fact]
        public void RequestIsCached()
        {
            var coreContext = new DefaultHttpContext();
            var context = new HttpContext(coreContext);

            Assert.Same(context.Request, context.Request);
        }

        [Fact]
        public void ResponseIsCached()
        {
            var coreContext = new DefaultHttpContext();
            var context = new HttpContext(coreContext);

            Assert.Same(context.Response, context.Response);
        }

        [Fact]
        public void ServerIsCached()
        {
            var coreContext = new DefaultHttpContext();
            var context = new HttpContext(coreContext);

            Assert.Same(context.Server, context.Server);
        }

        [Fact]
        public void UserIsProxied()
        {
            var coreContext = new DefaultHttpContext();
            var context = new HttpContext(coreContext);

            Assert.Same(coreContext.User, context.User);

            var newUser = new ClaimsPrincipal();
            context.User = newUser;

            Assert.Same(coreContext.User, newUser);
        }

        [Fact]
        public void NonClaimsPrincipalIsCopied()
        {
            var coreContext = new DefaultHttpContext();
            var context = new HttpContext(coreContext);

            var newUser = new Mock<IPrincipal>();
            context.User = newUser.Object;

            Assert.NotSame(coreContext.User, newUser);
        }

        [Fact]
        public void GetServiceReturnsExpected()
        {
            var coreContext = new DefaultHttpContext();
            var context = new HttpContext(coreContext);

            Assert.Same(context.Request, context.GetService(typeof(HttpRequest)));
            Assert.Same(context.Response, context.GetService(typeof(HttpResponse)));
            Assert.Same(context.Server, context.GetService(typeof(HttpServerUtility)));

            Assert.Null(context.GetService(typeof(HttpContext)));
        }

        [Fact]
        public void DefaultItemsContains()
        {
            // Arrange
            var key = new object();
            var value = new object();
            var items = new Mock<IDictionary<object, object?>>();
            items.Setup(i => i[key]).Returns(value);
            items.Setup(i => i.TryGetValue(key, out value)).Returns(true);

            var coreContext = new Mock<HttpContextCore>();
            coreContext.Setup(c => c.Items).Returns(items.Object);

            var context = new HttpContext(coreContext.Object);

            // Act
            var result = context.Items[key];

            // Assert
            Assert.Same(value, result);
        }

        [Fact]
        public void ItemsNotWrappedIfAlreadyImplementsIDictionary()
        {
            // Arrange
            // Use Dictionary<TKey, TValue> since it implements the non-generic IDictionary
            var items = new Dictionary<object, object?>();
            var coreContext = new Mock<HttpContextCore>();
            coreContext.Setup(c => c.Items).Returns(items);

            var context = new HttpContext(coreContext.Object);

            // Act
            var result = context.Items;

            // Assert
            Assert.Same(items, result);
        }
    }
}
