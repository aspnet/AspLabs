// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookHandlerSorterTests
    {
        private IWebHookHandlerSorter _sorter;

        public WebHookHandlerSorterTests()
        {
            _sorter = new WebHookHandlerSorter();
        }

        [Fact]
        public void SortHandlers_SortsByOrderAscending()
        {
            // Arrange
            List<IWebHookHandler> handlers = new List<IWebHookHandler>
            {
                new TestHandler(100),
                new TestHandler(20),
                new TestHandler(50),
                new TestHandler(int.MinValue),
                new TestHandler(int.MaxValue),
            };

            List<IWebHookHandler> expected = new List<IWebHookHandler>
            {
                new TestHandler(int.MinValue),
                new TestHandler(20),
                new TestHandler(50),
                new TestHandler(100),
                new TestHandler(int.MaxValue),
            };

            // Act
            IEnumerable<IWebHookHandler> actual = _sorter.SortHandlers(handlers);

            // Assert
            Assert.Equal(expected, actual);
        }

        private class TestHandler : WebHookHandler
        {
            public TestHandler(int order)
            {
                Order = order;
            }

            public override Task ExecuteAsync(string receiver, WebHookHandlerContext context)
            {
                throw new NotImplementedException();
            }

            public override bool Equals(object obj)
            {
                IWebHookHandler handler = obj as IWebHookHandler;
                return handler != null && Order == handler.Order;
            }

            public override int GetHashCode()
            {
                return Order;
            }
        }
    }
}
