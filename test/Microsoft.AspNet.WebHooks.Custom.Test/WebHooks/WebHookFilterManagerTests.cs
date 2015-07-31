// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookFilterManagerTests
    {
        [Theory]
        [InlineData(0, 0)]
        [InlineData(16, 15)]
        public async Task GetAllWebHookFilters_GetsAllFilters(int providerCount, int expectedFilterCount)
        {
            // Arrange
            IWebHookFilterManager manager = CreateWebHookFilterManager(providerCount);

            // Act
            IDictionary<string, WebHookFilter> actual = await manager.GetAllWebHookFiltersAsync();

            // Assert
            Assert.Equal(expectedFilterCount, actual.Count);
        }

        internal static IWebHookFilterManager CreateWebHookFilterManager(int providerCount)
        {
            Mock<IWebHookFilterProvider>[] providerMocks = new Mock<IWebHookFilterProvider>[providerCount];
            for (int cnt = 0; cnt < providerMocks.Length; cnt++)
            {
                providerMocks[cnt] = CreateFilterProvider(cnt);
            }
            WebHookFilterManager manager = new WebHookFilterManager(providerMocks.Select(p => p.Object));
            return manager;
        }

        private static Mock<IWebHookFilterProvider> CreateFilterProvider(int filterCount)
        {
            Collection<WebHookFilter> filters = new Collection<WebHookFilter>();
            for (int cnt = 0; cnt < filterCount; cnt++)
            {
                filters.Add(new WebHookFilter { Name = "filter" + cnt, Description = "Description" + cnt });
                filters.Add(new WebHookFilter { Name = "FILTER" + cnt, Description = "DESCRIPTION" + cnt });
            }

            Mock<IWebHookFilterProvider> providerMock = new Mock<IWebHookFilterProvider>();
            providerMock.Setup<Task<Collection<WebHookFilter>>>(p => p.GetFiltersAsync())
                .ReturnsAsync(filters)
                .Verifiable();

            return providerMock;
        }
    }
}
