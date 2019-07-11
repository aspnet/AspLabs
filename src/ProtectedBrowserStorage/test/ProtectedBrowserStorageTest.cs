// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.ProtectedBrowserStorage.Tests.TestServices;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;

namespace Microsoft.AspNetCore.ProtectedBrowserStorage.Tests
{
    public class ProtectedBrowserStorageTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void RequiresStoreName(string storeName)
        {
            var ex = Assert.Throws<ArgumentException>(
                () => new TestProtectedBrowserStorage(storeName, new TestJSRuntime(), new TestDataProtectionProvider()));
            Assert.Equal("storeName", ex.ParamName);
        }

        [Fact]
        public void RequiresJSRuntime()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => new TestProtectedBrowserStorage("someStoreName", null, new TestDataProtectionProvider()));
            Assert.Equal("jsRuntime", ex.ParamName);
        }

        [Fact]
        public void RequiresDataProtectionProvider()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => new TestProtectedBrowserStorage("someStoreName", new TestJSRuntime(), null));
            Assert.Equal("dataProtectionProvider", ex.ParamName);
        }

        [Fact]
        public void SetAsync_ProtectsAndInvokesJS()
        {
            // Arrange
            var jsRuntime = new TestJSRuntime();
            var dataProtectionProvider = new TestDataProtectionProvider();
            var protectedBrowserStorage = new TestProtectedBrowserStorage("test store", jsRuntime, dataProtectionProvider);
            var jsResultTask = Task.FromResult((object)null);
            var data = new TestModel { StringProperty = "Hello", IntProperty = 123 };
            var expectedPurpose = $"{typeof(TestProtectedBrowserStorage).FullName}:test store:test key";

            // Act
            jsRuntime.NextInvocationResult = jsResultTask;
            var result = protectedBrowserStorage.SetAsync("test key", data);

            // Assert
            Assert.Same(jsResultTask, result);
            var invocation = jsRuntime.Invocations.Single();
            Assert.Equal("blazorBrowserStorage.set", invocation.Identifier);
            Assert.Collection(invocation.Args,
                arg => Assert.Equal("test store", arg),
                arg => Assert.Equal("test key", arg),
                arg => Assert.Equal(
                    "{\"StringProperty\":\"Hello\",\"IntProperty\":123}",
                    TestDataProtectionProvider.Unprotect(expectedPurpose, (string)arg)));
        }

        [Fact]
        public void SetAsync_ProtectsAndInvokesJS_NullValue()
        {
            // Arrange
            var jsRuntime = new TestJSRuntime();
            var dataProtectionProvider = new TestDataProtectionProvider();
            var protectedBrowserStorage = new TestProtectedBrowserStorage("test store", jsRuntime, dataProtectionProvider);
            var jsResultTask = Task.FromResult((object)null);
            var expectedPurpose = $"{typeof(TestProtectedBrowserStorage).FullName}:test store:test key";

            // Act
            jsRuntime.NextInvocationResult = jsResultTask;
            var result = protectedBrowserStorage.SetAsync("test key", null);

            // Assert
            Assert.Same(jsResultTask, result);
            var invocation = jsRuntime.Invocations.Single();
            Assert.Equal("blazorBrowserStorage.set", invocation.Identifier);
            Assert.Collection(invocation.Args,
                arg => Assert.Equal("test store", arg),
                arg => Assert.Equal("test key", arg),
                arg => Assert.Equal(
                    "null",
                    TestDataProtectionProvider.Unprotect(expectedPurpose, (string)arg)));
        }

        [Fact]
        public async Task GetAsync_InvokesJSAndUnprotects_ValidData()
        {
            // Arrange
            var jsRuntime = new TestJSRuntime();
            var dataProtectionProvider = new TestDataProtectionProvider();
            var protectedBrowserStorage = new TestProtectedBrowserStorage("test store", jsRuntime, dataProtectionProvider);
            var data = new TestModel { StringProperty = "Hello", IntProperty = 123 };
            var expectedPurpose = $"{typeof(TestProtectedBrowserStorage).FullName}:test store:test key";
            var storedJson = "{\"StringProperty\":\"Hello\",\"IntProperty\":123}";
            jsRuntime.NextInvocationResult = Task.FromResult(
                TestDataProtectionProvider.Protect(expectedPurpose, storedJson));

            // Act
            var result = await protectedBrowserStorage.GetAsync<TestModel>("test key");

            // Assert
            Assert.Equal("Hello", result.StringProperty);
            Assert.Equal(123, result.IntProperty);

            var invocation = jsRuntime.Invocations.Single();
            Assert.Equal("blazorBrowserStorage.get", invocation.Identifier);
            Assert.Collection(invocation.Args,
                arg => Assert.Equal("test store", arg),
                arg => Assert.Equal("test key", arg));
        }

        [Fact]
        public async Task GetAsync_InvokesJSAndUnprotects_NoValue()
        {
            // Arrange
            var jsRuntime = new TestJSRuntime();
            var dataProtectionProvider = new TestDataProtectionProvider();
            var protectedBrowserStorage = new TestProtectedBrowserStorage("test store", jsRuntime, dataProtectionProvider);
            jsRuntime.NextInvocationResult = Task.FromResult((string)null);

            // Act
            var result = await protectedBrowserStorage.GetAsync<TestModel>("test key");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_InvokesJSAndUnprotects_InvalidJson()
        {
            // Arrange
            var jsRuntime = new TestJSRuntime();
            var dataProtectionProvider = new TestDataProtectionProvider();
            var protectedBrowserStorage = new TestProtectedBrowserStorage("test store", jsRuntime, dataProtectionProvider);
            var expectedPurpose = $"{typeof(TestProtectedBrowserStorage).FullName}:test store:test key";
            var storedJson = "you can't parse this";
            jsRuntime.NextInvocationResult = Task.FromResult(
                TestDataProtectionProvider.Protect(expectedPurpose, storedJson));

            // Act/Assert
            var ex = await Assert.ThrowsAsync<JsonException>(
                () => protectedBrowserStorage.GetAsync<TestModel>("test key"));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetAsync_InvokesJSAndUnprotects_InvalidProtection(bool base64Encode)
        {
            // Arrange
            var jsRuntime = new TestJSRuntime();
            var dataProtectionProvider = new TestDataProtectionProvider();
            var protectedBrowserStorage = new TestProtectedBrowserStorage("test store", jsRuntime, dataProtectionProvider);
            var storedString = "This string is not even protected";

            if (base64Encode)
            {
                // DataProtection deals with strings by base64-encoding the results.
                // Depending on whether the stored data is base64-encoded or not,
                // it will trigger a different failure point in data protection.
                storedString = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(storedString));
            }

            jsRuntime.NextInvocationResult = Task.FromResult(storedString);

            // Act/Assert
            var ex = await Assert.ThrowsAsync<CryptographicException>(
                () => protectedBrowserStorage.GetAsync<TestModel>("test key"));
        }

        [Fact]
        public async Task GetAsync_InvokesJSAndUnprotects_WrongPurpose()
        {
            // Arrange
            var jsRuntime = new TestJSRuntime();
            var dataProtectionProvider = new TestDataProtectionProvider();
            var protectedBrowserStorage = new TestProtectedBrowserStorage("test store", jsRuntime, dataProtectionProvider);
            var expectedPurpose = $"{typeof(TestProtectedBrowserStorage).FullName}:test store:test key";
            var storedJson = "we won't even try to parse this";
            jsRuntime.NextInvocationResult = Task.FromResult(
                TestDataProtectionProvider.Protect(expectedPurpose, storedJson));

            // Act/Assert
            var ex = await Assert.ThrowsAsync<CryptographicException>(
                () => protectedBrowserStorage.GetAsync<TestModel>("different key"));
            var innerException = ex.InnerException;
            Assert.IsType<ArgumentException>(innerException);
            Assert.Contains("The value is not protected with the expected purpose", innerException.Message);
        }

        [Fact]
        public async Task DeleteAsync_InvokesJS()
        {
            // Arrange
            var jsRuntime = new TestJSRuntime();
            var dataProtectionProvider = new TestDataProtectionProvider();
            var protectedBrowserStorage = new TestProtectedBrowserStorage("test store", jsRuntime, dataProtectionProvider);
            var nextTask = Task.FromResult((object)null);
            jsRuntime.NextInvocationResult = nextTask;

            // Act
            var result = protectedBrowserStorage.DeleteAsync("test key");

            // Assert
            Assert.Same(nextTask, result);
            var invocation = jsRuntime.Invocations.Single();
            Assert.Equal("blazorBrowserStorage.delete", invocation.Identifier);
            Assert.Collection(invocation.Args,
                arg => Assert.Equal("test store", arg),
                arg => Assert.Equal("test key", arg));
        }

        [Fact]
        public async Task ReusesCachedProtectorsByPurpose()
        {
            // Arrange
            var jsRuntime = new TestJSRuntime();
            jsRuntime.NextInvocationResult = Task.FromResult((object)null);
            var dataProtectionProvider = new TestDataProtectionProvider();
            var protectedBrowserStorage = new TestProtectedBrowserStorage("test store", jsRuntime, dataProtectionProvider);

            // Act
            await protectedBrowserStorage.SetAsync("key 1", null);
            await protectedBrowserStorage.SetAsync("key 2", null);
            await protectedBrowserStorage.SetAsync("key 1", null);
            await protectedBrowserStorage.SetAsync("key 3", null);

            // Assert
            var typeName = typeof(TestProtectedBrowserStorage).FullName;
            var expectedPurposes = new[]
            {
                $"{typeName}:test store:key 1",
                $"{typeName}:test store:key 2",
                $"{typeName}:test store:key 3"
            };
            Assert.Equal(expectedPurposes, dataProtectionProvider.ProtectorsCreated.ToArray());

            Assert.Collection(jsRuntime.Invocations,
                invocation => Assert.Equal(TestDataProtectionProvider.Protect(expectedPurposes[0], "null"), invocation.Args[2]),
                invocation => Assert.Equal(TestDataProtectionProvider.Protect(expectedPurposes[1], "null"), invocation.Args[2]),
                invocation => Assert.Equal(TestDataProtectionProvider.Protect(expectedPurposes[0], "null"), invocation.Args[2]),
                invocation => Assert.Equal(TestDataProtectionProvider.Protect(expectedPurposes[2], "null"), invocation.Args[2]));
        }

        class TestModel
        {
            public string StringProperty { get; set; }

            public int IntProperty { get; set; }
        }
    }
}
