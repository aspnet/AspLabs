// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public abstract class WebHookStoreTest
    {
        private const string OtherUser = "OtherUser";
        private const int WebHookCount = 8;

        private readonly IWebHookStore _store;

        protected WebHookStoreTest(IWebHookStore store)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }
            _store = store;
        }

        protected string TestUser
        {
            get
            {
                return "TestUser";
            }
        }

        protected IWebHookStore Store
        {
            get
            {
                return _store;
            }
        }

        [Fact]
        public async Task GetAllWebHooksAsync_ReturnsExpectedItems()
        {
            // Arrange
            await Initialize();

            // Act
            ICollection<WebHook> actual = await _store.GetAllWebHooksAsync(TestUser);

            // Assert
            Assert.Equal(WebHookCount, actual.Count);
            Assert.Equal(WebHookCount, actual.Where(h => h.Description == TestUser).Count());
        }

        [Theory]
        [InlineData("a1", true)]
        [InlineData("A1", true)]
        [InlineData("b1", false)]
        [InlineData("", false)]
        public async Task QueryWebHooksAsync_ReturnsExpectedItems(string action, bool present)
        {
            // Arrange
            await Initialize();
            WebHook w1 = CreateWebHook(TestUser, 32, filter: "h1");
            await _store.InsertWebHookAsync(TestUser, w1);

            // Act
            ICollection<WebHook> actual = await _store.QueryWebHooksAsync(TestUser, new[] { action });

            // Assert
            int expectedCount = present ? WebHookCount : 0;
            Assert.Equal(expectedCount, actual.Count);
            Assert.Equal(expectedCount, actual.Where(h => h.Description == TestUser).Count());
        }

        [Fact]
        public async Task QueryWebHooksAsync_SkipsPausedWebHooks()
        {
            // Arrange
            await Initialize();
            WebHook w1 = CreateWebHook(TestUser, 32);
            w1.IsPaused = true;
            await _store.InsertWebHookAsync(TestUser, w1);

            // Act
            ICollection<WebHook> actual = await _store.QueryWebHooksAsync(TestUser, new[] { "a1" });

            // Assert
            Assert.Equal(WebHookCount, actual.Count);
            Assert.Equal(WebHookCount, actual.Where(h => h.Description == TestUser).Count());
        }

        [Fact]
        public async Task QueryWebHooksAsync_FindsWildcards()
        {
            // Arrange
            await Initialize();
            WebHook w1 = CreateWebHook(TestUser, 32, filter: WildcardWebHookFilterProvider.Name);
            await _store.InsertWebHookAsync(TestUser, w1);

            // Act
            ICollection<WebHook> actual = await _store.QueryWebHooksAsync(TestUser, new[] { "a1" });

            // Assert
            Assert.Equal(WebHookCount + 1, actual.Count);
            Assert.Equal(WebHookCount, actual.Where(h => h.Filters.Contains("a1")).Count());
            Assert.Equal(1, actual.Where(h => h.Filters.Contains("*")).Count());
        }

        [Theory]
        [InlineData("1", true)]
        [InlineData("7", true)]
        [InlineData("unknown", false)]
        public async Task LookupWebHooksAsync_ReturnsExpectedItem(string id, bool present)
        {
            // Arrange
            await Initialize();

            // Act
            WebHook actual = await _store.LookupWebHookAsync(TestUser, id);

            // Assert
            if (present)
            {
                Assert.NotNull(actual);
                Assert.Equal(id, actual.Id);
            }
            else
            {
                Assert.Null(actual);
            }
        }

        [Fact]
        public async Task InsertWebHookAsync_InsertsItem()
        {
            // Arrange
            await Initialize();
            WebHook webHook = CreateWebHook(TestUser, 32);

            // Act
            StoreResult actual = await _store.InsertWebHookAsync(TestUser, webHook);

            // Assert
            Assert.Equal(StoreResult.Success, actual);
        }

        [Fact]
        public async Task InsertWebHookAsync_DetectsConflictOnInsertingAlreadyExistingItem()
        {
            // Arrange
            await Initialize();
            WebHook webHook = CreateWebHook(TestUser, 32);

            // Act
            StoreResult actual1 = await _store.InsertWebHookAsync(TestUser, webHook);
            StoreResult actual2 = await _store.InsertWebHookAsync(TestUser, webHook);

            // Assert
            Assert.Equal(StoreResult.Success, actual1);
            Assert.Equal(StoreResult.Conflict, actual2);
        }

        [Fact]
        public async Task UpdateWebHookAsync_UpdatesItem()
        {
            // Arrange
            await Initialize();
            WebHook webHook = CreateWebHook(TestUser, 0);
            webHook.Description = "Updated!";

            // Act
            StoreResult actual1 = await _store.UpdateWebHookAsync(TestUser, webHook);
            WebHook actual2 = await _store.LookupWebHookAsync(TestUser, "0");

            // Assert
            Assert.Equal(StoreResult.Success, actual1);
            Assert.Equal("Updated!", actual2.Description);
        }

        [Fact]
        public async Task UpdateWebHookAsync_DetectsWhenUpdatedItemDoesNotExist()
        {
            // Arrange
            await Initialize();
            WebHook webHook = CreateWebHook(TestUser, 32);

            // Act
            StoreResult actual = await _store.UpdateWebHookAsync(TestUser, webHook);

            // Assert
            Assert.Equal(StoreResult.NotFound, actual);
        }

        [Fact]
        public async Task DeleteWebHookAsync_DeletesItem()
        {
            // Arrange
            await Initialize();

            // Act
            StoreResult actual = await _store.DeleteWebHookAsync(TestUser, "0");

            // Assert
            Assert.Equal(StoreResult.Success, actual);
        }

        [Fact]
        public async Task DeleteWebHookAsync_DetectsWhenDeletedItemDoesNotExist()
        {
            // Arrange
            await Initialize();

            // Act
            StoreResult actual1 = await _store.DeleteWebHookAsync(TestUser, "0");
            StoreResult actual2 = await _store.DeleteWebHookAsync(TestUser, "0");

            // Assert
            Assert.Equal(StoreResult.Success, actual1);
            Assert.Equal(StoreResult.NotFound, actual2);
        }

        [Fact]
        public async Task DeleteAllWebHooksAsync_DeletesAllItems()
        {
            // Arrange
            await Initialize();

            // Act
            await _store.DeleteAllWebHooksAsync(TestUser);
            ICollection<WebHook> actual1 = await _store.GetAllWebHooksAsync(TestUser);
            ICollection<WebHook> actual2 = await _store.GetAllWebHooksAsync(OtherUser);

            // Assert
            Assert.Empty(actual1);
            Assert.Equal(WebHookCount, actual2.Count);
        }

        [Fact]
        public async Task DeleteAllWebHooksAsync_WorksIfAlreadyEmpty()
        {
            // Arrange
            await Initialize();

            // Act
            await _store.DeleteAllWebHooksAsync(TestUser);
            ICollection<WebHook> actual1 = await _store.GetAllWebHooksAsync(TestUser);
            ICollection<WebHook> actual2 = await _store.GetAllWebHooksAsync(TestUser);

            // Assert
            Assert.Empty(actual1);
            Assert.Empty(actual2);
        }

        protected static WebHook CreateWebHook(string user, int offset, string filter = "a1")
        {
            WebHook hook = new WebHook
            {
                Id = offset.ToString(),
                Description = user,
                Secret = "123456789012345678901234567890123456789012345678",
                WebHookUri = "http://localhost/hook/" + offset
            };
            hook.Headers.Add("h1", "hv1");
            hook.Properties.Add("p1", "pv1");
            hook.Filters.Add(filter);
            return hook;
        }

        protected virtual async Task Initialize()
        {
            // Reset items for test user
            await _store.DeleteAllWebHooksAsync(TestUser);
            for (int cnt = 0; cnt < WebHookCount; cnt++)
            {
                WebHook webHook = CreateWebHook(TestUser, cnt);
                await _store.InsertWebHookAsync(TestUser, webHook);
            }

            // Insert items for other user which should not show up
            await _store.DeleteAllWebHooksAsync(OtherUser);
            for (int cnt = 0; cnt < WebHookCount; cnt++)
            {
                WebHook webHook = CreateWebHook(OtherUser, cnt);
                await _store.InsertWebHookAsync(OtherUser, webHook);
            }
        }
    }
}
