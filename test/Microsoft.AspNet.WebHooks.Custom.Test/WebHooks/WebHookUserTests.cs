// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class WebHookUserTests : IDisposable
    {
        [Fact]
        public async Task GetUserIdAsync_Throws_IfInvalidUser()
        {
            WebHookUser user = new WebHookUser();
            Mock<IPrincipal> principalMock = new Mock<IPrincipal>();

            // Act
            InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => user.GetUserIdAsync(principalMock.Object));

            // Assert
            Assert.Equal("Could not determine the user ID from the given principal.", ex.Message);
        }

        [Fact]
        public async Task GetUserIdAsync_Succeeds_IfValidNameIdentifierClaim()
        {
            // Arrange
            WebHookUser user = new WebHookUser();
            ClaimsIdentity identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "TestUser"));
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            // Act
            string actual = await user.GetUserIdAsync(principal);

            // Assert
            Assert.Equal("TestUser", actual);
        }

        [Fact]
        public async Task GetUserIdAsync_Succeeds_IfValidNameClaim()
        {
            // Arrange
            WebHookUser user = new WebHookUser();
            ClaimsIdentity identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "NameIdentifier"));
            identity.AddClaim(new Claim(ClaimTypes.Name, "TestUser"));
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            // Act
            string actual = await user.GetUserIdAsync(principal);

            // Assert
            Assert.Equal("TestUser", actual);
        }

        [Fact]
        public async Task GetUserIdAsync_Succeeds_IfValidCustomClaim()
        {
            // Arrange
            WebHookUser.IdClaimsType = ClaimTypes.PrimarySid;
            WebHookUser user = new WebHookUser();
            ClaimsIdentity identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Name, "Name"));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "NameIdentifier"));
            identity.AddClaim(new Claim(ClaimTypes.PrimarySid, "TestUser"));
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            // Act
            string actual = await user.GetUserIdAsync(principal);

            // Assert
            Assert.Equal("TestUser", actual);
        }

        [Fact]
        public async Task GetUserIdAsync_Succeeds_IfValidNameProperty()
        {
            // Arrange
            WebHookUser user = new WebHookUser();
            Mock<IPrincipal> principalMock = new Mock<IPrincipal>();
            principalMock.Setup(p => p.Identity.Name)
                .Returns("TestUser")
                .Verifiable();

            // Act
            string actual = await user.GetUserIdAsync(principalMock.Object);

            // Assert
            Assert.Equal("TestUser", actual);
        }

        public void Dispose()
        {
            WebHookUser.Reset();
        }
    }
}
