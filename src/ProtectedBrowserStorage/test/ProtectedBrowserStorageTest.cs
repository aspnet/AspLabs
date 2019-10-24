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

        [Theory]
        [InlineData(null)]
        [InlineData("my custom purpose")]
        public void SetAsync_ProtectsAndInvokesJS(string customPurpose)
        {
            // Arrange
            var jsRuntime = new TestJSRuntime();
            var storeName = "test store";
            var dataProtectionProvider = new TestDataProtectionProvider();
            var protectedBrowserStorage = new TestProtectedBrowserStorage(storeName, jsRuntime, dataProtectionProvider);
            var jsResultTask = new ValueTask<object>((object)null);
            var data = new TestModel { StringProperty = "Hello", IntProperty = 123 };
            var keyName = "test key";
            var expectedPurpose = customPurpose == null
                ? $"{typeof(TestProtectedBrowserStorage).FullName}:{storeName}:{keyName}"
                : customPurpose;

            // Act
            jsRuntime.NextInvocationResult = jsResultTask;
            var result = customPurpose == null
                ? protectedBrowserStorage.SetAsync(keyName, data)
                : protectedBrowserStorage.SetAsync(customPurpose, keyName, data);

            // Assert
            var invocation = jsRuntime.Invocations.Single();
            Assert.Equal($"{storeName}.setItem", invocation.Identifier);
            Assert.Collection(invocation.Args,
                arg => Assert.Equal(keyName, arg),
                arg => Assert.Equal(
                    "{\"StringProperty\":\"Hello\",\"IntProperty\":123}",
                    TestDataProtectionProvider.Unprotect(expectedPurpose, (string)arg)));
        }

        [Fact]
        public void SetAsync_ProtectsAndInvokesJS_NullValue()
        {
            // Arrange
            var jsRuntime = new TestJSRuntime();
            var storeName = "test store";
            var dataProtectionProvider = new TestDataProtectionProvider();
            var protectedBrowserStorage = new TestProtectedBrowserStorage(storeName, jsRuntime, dataProtectionProvider);
            var jsResultTask = new ValueTask<object>((object)null);
            var expectedPurpose = $"{typeof(TestProtectedBrowserStorage).FullName}:{storeName}:test key";

            // Act
            jsRuntime.NextInvocationResult = jsResultTask;
            var result = protectedBrowserStorage.SetAsync("test key", null);

            // Assert
            var invocation = jsRuntime.Invocations.Single();
            Assert.Equal($"{storeName}.setItem", invocation.Identifier);
            Assert.Collection(invocation.Args,
                arg => Assert.Equal("test key", arg),
                arg => Assert.Equal(
                    "null",
                    TestDataProtectionProvider.Unprotect(expectedPurpose, (string)arg)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("my custom purpose")]
        public async Task GetAsync_InvokesJSAndUnprotects_ValidData(string customPurpose)
        {
            // Arrange
            var jsRuntime = new TestJSRuntime();
            var dataProtectionProvider = new TestDataProtectionProvider();
            var storeName = "test store";
            var protectedBrowserStorage = new TestProtectedBrowserStorage(storeName, jsRuntime, dataProtectionProvider);
            var data = new TestModel { StringProperty = "Hello", IntProperty = 123 };
            var keyName = "test key";
            var expectedPurpose = customPurpose == null
                ? $"{typeof(TestProtectedBrowserStorage).FullName}:{storeName}:{keyName}"
                : customPurpose;
            var storedJson = "{\"StringProperty\":\"Hello\",\"IntProperty\":123}";
            jsRuntime.NextInvocationResult = new ValueTask<string>(
                TestDataProtectionProvider.Protect(expectedPurpose, storedJson));

            // Act
            var result = customPurpose == null
                ? await protectedBrowserStorage.GetAsync<TestModel>(keyName)
                : await protectedBrowserStorage.GetAsync<TestModel>(customPurpose, keyName);

            // Assert
            Assert.Equal("Hello", result.StringProperty);
            Assert.Equal(123, result.IntProperty);

            var invocation = jsRuntime.Invocations.Single();
            Assert.Equal($"{storeName}.getItem", invocation.Identifier);
            Assert.Collection(invocation.Args,
                arg => Assert.Equal(keyName, arg));
        }

        [Fact]
        public async Task GetAsync_InvokesJSAndUnprotects_NoValue()
        {
            // Arrange
            var jsRuntime = new TestJSRuntime();
            var dataProtectionProvider = new TestDataProtectionProvider();
            var protectedBrowserStorage = new TestProtectedBrowserStorage("test store", jsRuntime, dataProtectionProvider);
            jsRuntime.NextInvocationResult = new ValueTask<string>((string)null);

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
            jsRuntime.NextInvocationResult = new ValueTask<string>(
                TestDataProtectionProvider.Protect(expectedPurpose, storedJson));

            // Act/Assert
            var ex = await Assert.ThrowsAsync<JsonException>(
                async () => await protectedBrowserStorage.GetAsync<TestModel>("test key"));
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

            jsRuntime.NextInvocationResult = new ValueTask<string>(storedString);

            // Act/Assert
            var ex = await Assert.ThrowsAsync<CryptographicException>(
                async () => await protectedBrowserStorage.GetAsync<TestModel>("test key"));
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
            jsRuntime.NextInvocationResult = new ValueTask<string>(
                TestDataProtectionProvider.Protect(expectedPurpose, storedJson));

            // Act/Assert
            var ex = await Assert.ThrowsAsync<CryptographicException>(
                async () => await protectedBrowserStorage.GetAsync<TestModel>("different key"));
            var innerException = ex.InnerException;
            Assert.IsType<ArgumentException>(innerException);
            Assert.Contains("The value is not protected with the expected purpose", innerException.Message);
        }

        [Fact]
        public void DeleteAsync_InvokesJS()
        {
            // Arrange
            var jsRuntime = new TestJSRuntime();
            var storeName = "test store";
            var dataProtectionProvider = new TestDataProtectionProvider();
            var protectedBrowserStorage = new TestProtectedBrowserStorage(storeName, jsRuntime, dataProtectionProvider);
            var nextTask = new ValueTask<object>((object)null);
            jsRuntime.NextInvocationResult = nextTask;

            // Act
            var result = protectedBrowserStorage.DeleteAsync("test key");

            // Assert
            var invocation = jsRuntime.Invocations.Single();
            Assert.Equal($"{storeName}.removeItem", invocation.Identifier);
            Assert.Collection(invocation.Args,
                arg => Assert.Equal("test key", arg));
        }

        [Fact]
        public async Task ReusesCachedProtectorsByPurpose()
        {
            // Arrange
            var jsRuntime = new TestJSRuntime();
            jsRuntime.NextInvocationResult = new ValueTask<object>((object)null);
            var storeName = "test store";
            var dataProtectionProvider = new TestDataProtectionProvider();
            var protectedBrowserStorage = new TestProtectedBrowserStorage(storeName, jsRuntime, dataProtectionProvider);

            // Act
            await protectedBrowserStorage.SetAsync("key 1", null);
            await protectedBrowserStorage.SetAsync("key 2", null);
            await protectedBrowserStorage.SetAsync("key 1", null);
            await protectedBrowserStorage.SetAsync("key 3", null);

            // Assert
            var typeName = typeof(TestProtectedBrowserStorage).FullName;
            var expectedPurposes = new[]
            {
                $"{typeName}:{storeName}:key 1",
                $"{typeName}:{storeName}:key 2",
                $"{typeName}:{storeName}:key 3"
            };
            Assert.Equal(expectedPurposes, dataProtectionProvider.ProtectorsCreated.ToArray());

            Assert.Collection(jsRuntime.Invocations,
                invocation => Assert.Equal(TestDataProtectionProvider.Protect(expectedPurposes[0], "null"), invocation.Args[1]),
                invocation => Assert.Equal(TestDataProtectionProvider.Protect(expectedPurposes[1], "null"), invocation.Args[1]),
                invocation => Assert.Equal(TestDataProtectionProvider.Protect(expectedPurposes[0], "null"), invocation.Args[1]),
                invocation => Assert.Equal(TestDataProtectionProvider.Protect(expectedPurposes[2], "null"), invocation.Args[1]));
        }

        class TestModel
        {
            public string StringProperty { get; set; }

            public int IntProperty { get; set; }
        }
    }
}
