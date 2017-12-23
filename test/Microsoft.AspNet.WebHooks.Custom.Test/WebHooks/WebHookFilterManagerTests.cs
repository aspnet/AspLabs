// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
            var manager = CreateWebHookFilterManager(providerCount);

            // Act
            var actual = await manager.GetAllWebHookFiltersAsync();

            // Assert
            Assert.Equal(expectedFilterCount, actual.Count);
        }

        internal static IWebHookFilterManager CreateWebHookFilterManager(int providerCount)
        {
            var providerMocks = new Mock<IWebHookFilterProvider>[providerCount];
            for (var i = 0; i < providerMocks.Length; i++)
            {
                providerMocks[i] = CreateFilterProvider(i);
            }
            var manager = new WebHookFilterManager(providerMocks.Select(p => p.Object));
            return manager;
        }

        private static Mock<IWebHookFilterProvider> CreateFilterProvider(int filterCount)
        {
            var filters = new Collection<WebHookFilter>();
            for (var i = 0; i < filterCount; i++)
            {
                filters.Add(new WebHookFilter { Name = "filter" + i, Description = "Description" + i });
                filters.Add(new WebHookFilter { Name = "FILTER" + i, Description = "DESCRIPTION" + i });
            }

            var providerMock = new Mock<IWebHookFilterProvider>();
            providerMock.Setup<Task<Collection<WebHookFilter>>>(p => p.GetFiltersAsync())
                .ReturnsAsync(filters)
                .Verifiable();

            return providerMock;
        }
    }
}
