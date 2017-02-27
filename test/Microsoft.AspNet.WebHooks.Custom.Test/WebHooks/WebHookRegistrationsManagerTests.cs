// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookRegistrationsManagerTests
    {
        private const string MockUser = "mockUser";

        private readonly Mock<IWebHookManager> _managerMock;
        private readonly Mock<IWebHookStore> _storeMock;
        private readonly Mock<IWebHookFilterManager> _filtersMock;
        private readonly Mock<IPrincipal> _principalMock;
        private readonly Mock<IWebHookUser> _userMock;
        private readonly Mock<WebHookRegistrationsManager> _regsMock;

        private readonly WebHook _webHook;

        public WebHookRegistrationsManagerTests()
        {
            _managerMock = new Mock<IWebHookManager>();
            _storeMock = new Mock<IWebHookStore>();
            _filtersMock = new Mock<IWebHookFilterManager>();
            _principalMock = new Mock<IPrincipal>();
            _userMock = new Mock<IWebHookUser>();
            _userMock.Setup(u => u.GetUserIdAsync(_principalMock.Object))
                .ReturnsAsync(MockUser);

            _regsMock = new Mock<WebHookRegistrationsManager>(_managerMock.Object, _storeMock.Object, _filtersMock.Object, _userMock.Object)
            {
                CallBase = true
            };

            _webHook = new WebHook();
        }

        public static TheoryData<string, string> NormalizedFilterData
        {
            get
            {
                return new TheoryData<string, string>
                {
                    { string.Empty, string.Empty },
                    { "FILTER", "filter" },
                    { "FiLTeR", "filter" },
                    { "Filter", "filter" },
                    { "filter", "Filter" },
                    { "你好世界", "你好世界" },
                };
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task GetWebHooks_Returns_WebHooksAndCallsFilterPredicate(bool nonempty, bool applyPredicate)
        {
            // Arrange
            var expected = nonempty ? new WebHook[] { _webHook } : new WebHook[0];
            _storeMock.Setup(s => s.GetAllWebHooksAsync(MockUser))
                .ReturnsAsync(expected)
                .Verifiable();

            string actualUser = null;
            WebHook actualWebHook = null;
            Func<string, WebHook, Task> predicate = (s, w) =>
            {
                actualUser = s;
                actualWebHook = w;
                return Task.FromResult(true);
            };

            // Act
            var actual = await _regsMock.Object.GetWebHooksAsync(_principalMock.Object, applyPredicate ? predicate : null);

            // Assert
            Assert.Equal(applyPredicate && nonempty ? MockUser : null, actualUser);
            Assert.Equal(applyPredicate && nonempty ? _webHook : null, actualWebHook);
            Assert.Equal(expected, actual);
            _storeMock.Verify();
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task LookupWebHooks_Returns_WebHookAndCallsFilterPredicate(bool nonempty, bool applyPredicate)
        {
            // Arrange
            var expected = nonempty ? _webHook : null;
            _storeMock.Setup(s => s.LookupWebHookAsync(MockUser, "12345"))
                .ReturnsAsync(expected)
                .Verifiable();

            string actualUser = null;
            WebHook actualWebHook = null;
            Func<string, WebHook, Task> predicate = (s, w) =>
            {
                actualUser = s;
                actualWebHook = w;
                return Task.FromResult(true);
            };

            // Act
            var actual = await _regsMock.Object.LookupWebHookAsync(_principalMock.Object, "12345", applyPredicate ? predicate : null);

            // Assert
            Assert.Equal(applyPredicate && nonempty ? MockUser : null, actualUser);
            Assert.Equal(applyPredicate && nonempty ? _webHook : null, actualWebHook);
            Assert.Equal(expected, actual);
            _storeMock.Verify();
        }

        [Fact]
        public async Task AddWebHook_Calls_FilterPredicate()
        {
            // Arrange
            string actualUser = null;
            WebHook actualWebHook = null;
            Func<string, WebHook, Task> predicate = (s, w) =>
            {
                actualUser = s;
                actualWebHook = w;
                return Task.FromResult(true);
            };

            // Act
            await _regsMock.Object.AddWebHookAsync(_principalMock.Object, _webHook, predicate);

            // Assert
            Assert.Equal(MockUser, actualUser);
            Assert.Equal(_webHook, actualWebHook);
        }

        [Fact]
        public async Task AddWebHook_Returns_StoreResult()
        {
            // Arrange
            StoreResult expected = new StoreResult();
            _storeMock.Setup(s => s.InsertWebHookAsync(MockUser, _webHook))
                .ReturnsAsync(expected)
                .Verifiable();

            // Act
            StoreResult actual = await _regsMock.Object.AddWebHookAsync(_principalMock.Object, _webHook, null);

            // Assert
            _storeMock.Verify();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task UpdateWebHook_Calls_FilterPredicate()
        {
            // Arrange
            string actualUser = null;
            WebHook actualWebHook = null;
            Func<string, WebHook, Task> predicate = (s, w) =>
            {
                actualUser = s;
                actualWebHook = w;
                return Task.FromResult(true);
            };

            // Act
            await _regsMock.Object.UpdateWebHookAsync(_principalMock.Object, _webHook, predicate);

            // Assert
            Assert.Equal(MockUser, actualUser);
            Assert.Equal(_webHook, actualWebHook);
        }

        [Fact]
        public async Task UpdateWebHook_Returns_StoreResult()
        {
            // Arrange
            StoreResult expected = new StoreResult();
            _storeMock.Setup(s => s.UpdateWebHookAsync(MockUser, _webHook))
                .ReturnsAsync(expected)
                .Verifiable();

            // Act
            StoreResult actual = await _regsMock.Object.UpdateWebHookAsync(_principalMock.Object, _webHook, null);

            // Assert
            _storeMock.Verify();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task DeleteWebHook_Returns_StoreResult()
        {
            // Arrange
            StoreResult expected = new StoreResult();
            _storeMock.Setup(s => s.DeleteWebHookAsync(MockUser, "12345"))
                .ReturnsAsync(expected)
                .Verifiable();

            // Act
            StoreResult actual = await _regsMock.Object.DeleteWebHookAsync(_principalMock.Object, "12345");

            // Assert
            _storeMock.Verify();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task DeleteAllWebHooks_Calls_Store()
        {
            // Arrange
            _storeMock.Setup(s => s.DeleteAllWebHooksAsync(MockUser))
                .Returns(Task.FromResult(true))
                .Verifiable();

            // Act
            await _regsMock.Object.DeleteAllWebHooksAsync(_principalMock.Object);

            // Assert
            _storeMock.Verify();
        }

        [Fact]
        public async Task VerifySecret_SetsSecret_IfNoneProvided()
        {
            // Arrange
            string current = _webHook.Secret;

            // Act
            await _regsMock.Object.VerifySecretAsync(_webHook);
            Guid actual = Guid.ParseExact(_webHook.Secret, "N");

            // Assert
            Assert.NotEqual(current, _webHook.Secret);
        }

        [Fact]
        public async Task VerifyFilter_SetsWildcard_IfNoFiltersProvided()
        {
            // Act
            await _regsMock.Object.VerifyFiltersAsync(_webHook);

            // Assert
            Assert.Equal("*", _webHook.Filters.Single());
        }

        [Theory]
        [MemberData(nameof(NormalizedFilterData))]
        public async Task VerifyFilter_Adds_NormalizedFilters(string input, string expected)
        {
            // Arrange
            IDictionary<string, WebHookFilter> filters = new Dictionary<string, WebHookFilter>(StringComparer.OrdinalIgnoreCase)
            {
                { expected, new WebHookFilter { Name = expected } }
            };
            _filtersMock.Setup(f => f.GetAllWebHookFiltersAsync())
                .ReturnsAsync(filters)
                .Verifiable();
            _webHook.Filters.Add(input);

            // Act
            await _regsMock.Object.VerifyFiltersAsync(_webHook);

            // Assert
            _filtersMock.Verify();
            Assert.Equal(expected, _webHook.Filters.Single());
        }

        [Fact]
        public async Task VerifyFilter_Throws_IfInvalidFilters()
        {
            // Arrange
            IDictionary<string, WebHookFilter> filters = new Dictionary<string, WebHookFilter>();
            _filtersMock.Setup(f => f.GetAllWebHookFiltersAsync())
                .ReturnsAsync(filters)
                .Verifiable();
            _webHook.Filters.Add("Unknown");

            // Act
            InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _regsMock.Object.VerifyFiltersAsync(_webHook));

            // Assert
            _filtersMock.Verify();
            Assert.Equal("The following filters are not valid: 'Unknown'.", ex.Message);
        }
    }
}
