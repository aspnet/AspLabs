// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Controllers
{
    public class WebHookFiltersControllerTests
    {
        private HttpConfiguration _config;
        private WebHookFiltersController _controller;
        private IWebHookFilterManager _filterManager;
        private Mock<IDependencyResolver> _resolverMock;

        public WebHookFiltersControllerTests()
        {
            WildcardWebHookFilterProvider provider = new WildcardWebHookFilterProvider();
            _filterManager = new WebHookFilterManager(new[] { provider });

            _resolverMock = new Mock<IDependencyResolver>();
            _resolverMock.Setup(r => r.GetService(typeof(IWebHookFilterManager)))
                .Returns(_filterManager)
                .Verifiable();

            _config = new HttpConfiguration();
            _config.DependencyResolver = _resolverMock.Object;

            HttpControllerContext controllerContext = new HttpControllerContext()
            {
                Configuration = _config,
                Request = new HttpRequestMessage(),
            };
            _controller = new WebHookFiltersController();
            _controller.ControllerContext = controllerContext;
        }

        [Fact]
        public async Task Get_Returns_ExpectedFilters()
        {
            // Act
            IEnumerable<WebHookFilter> actual = await _controller.Get();

            // Assert
            Assert.Equal(1, actual.Count());
            Assert.Equal(WildcardWebHookFilterProvider.Name, actual.Single().Name);
        }
    }
}
