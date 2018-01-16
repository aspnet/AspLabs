// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    public class StripeVerifySignatureFilterTests
    {
        private const string ExtraHeaderContent =
            "  v1=5257a869e7ecebeda32affa62cdca3fa51cad7e77a0e56ff536d0ce8e108d8bd, " +
            "v0=6ffbb59b2300aae63f272406069a9788598b792a944a07aba816edb039989a39  ";
        private const string TestContent = "{ \"type\": \"action\", \"id\": \"" + TestStripeId + "\" }";
        private const string TestId = "";
        private const string TestSecret = "12345678901234567890123456789012";
        private const string TestStripeId = "12345";
        private const string TestTimestamp = "1492774577";

        private static readonly string TestSignatureHeader = GetSignatureHeader(TestContent);

        public static TheoryData<string> HeadersWithMissingValues
        {
            get
            {
                return new TheoryData<string>
                {
                    string.Empty,
                    $"{StripeConstants.TimestampKey}={TestTimestamp} ",
                    ExtraHeaderContent,
                    "a=b, b=c, c=d, e=f",
                };
            }
        }

        public static TheoryData<string> InvalidReceiverData
        {
            get
            {
                return new TheoryData<string>
                {
                    { string.Empty },
                    { "你好" },
                    { "1" },
                    { "1234567890" },
                    { "github" },
                };
            }
        }

        public static TheoryData<string> ValidIdData
        {
            get
            {
                return new TheoryData<string>
                {
                    { string.Empty },
                    { "id" },
                    { "你好" },
                    { "1" },
                    { "1234567890" },
                };
            }
        }

        [Fact]
        public void ReceiverName_IsConsistent()
        {
            // Arrange
            var filter = GetFilter();
            var expected = "stripe";

            // Act
            var actual = filter.ReceiverName;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(InvalidReceiverData))]
        public void IsApplicable_ReturnsFalseForNonMatches(string receiverName)
        {
            // Arrange
            var filter = GetFilter();

            // Act
            var actual = filter.IsApplicable(receiverName);

            // Assert
            Assert.False(actual);
        }

        [Theory]
        [InlineData("stripe")]
        [InlineData("Stripe")]
        [InlineData("STRIPE")]
        public void IsApplicable_ReturnsTrueForMatches(string receiverName)
        {
            // Arrange
            var filter = GetFilter();

            // Act
            var actual = filter.IsApplicable(receiverName);

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public async Task OnResourceExecutionAsync_Fails_IfPostHasNoSignatureHeader()
        {
            // Arrange
            var expectedMessage = $"Expecting exactly one '{StripeConstants.SignatureHeaderName}' header field in " +
                "the WebHook request but found 0. Please ensure the request contains exactly one " +
                $"'{StripeConstants.SignatureHeaderName}' header field.";
            var filter = GetFilter(TestSecret);
            var context = GetContext(TestContent);

            // Act
            await filter.OnResourceExecutionAsync(context, () => throw new InvalidOperationException("Unreachable"));

            // Assert
            var result = Assert.IsType<BadRequestObjectResult>(context.Result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(expectedMessage, result.Value);
        }

        [Fact]
        public async Task OnResourceExecutionAsync_Fails_IfPostHasTwoSignatureHeaders()
        {
            // Arrange
            var expectedMessage = $"Expecting exactly one '{StripeConstants.SignatureHeaderName}' header field in " +
                "the WebHook request but found 2. Please ensure the request contains exactly one " +
                $"'{StripeConstants.SignatureHeaderName}' header field.";
            var filter = GetFilter(TestSecret);
            var context = GetContext(TestContent);
            context.HttpContext.Request.Headers.Add(StripeConstants.SignatureHeaderName, "value1");
            context.HttpContext.Request.Headers.Append(StripeConstants.SignatureHeaderName, "value2");

            // Act
            await filter.OnResourceExecutionAsync(context, () => throw new InvalidOperationException("Unreachable"));

            // Assert
            var result = Assert.IsType<BadRequestObjectResult>(context.Result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(expectedMessage, result.Value);
        }

        [Theory]
        [InlineData("你好世界")]
        [InlineData("invalid")]
        [InlineData("==")]
        [InlineData(",=,=,")]
        public async Task OnResourceExecutionAsync_Fails_IfPostHasInvalidSignatureHeader(string header)
        {
            // Arrange
            var expectedMessage = $"The '{StripeConstants.SignatureHeaderName}' header value is invalid. It must be " +
                "formatted as key=value pairs separated by commas.";
            var filter = GetFilter(TestSecret);
            var context = GetContext(TestContent);
            context.HttpContext.Request.Headers.Add(StripeConstants.SignatureHeaderName, header);

            // Act
            await filter.OnResourceExecutionAsync(context, () => throw new InvalidOperationException("Unreachable"));

            // Assert
            var result = Assert.IsType<BadRequestObjectResult>(context.Result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(expectedMessage, result.Value);
        }

        [Theory]
        [MemberData(nameof(HeadersWithMissingValues))]
        public async Task OnResourceExecutionAsync_Fails_IfPostHasSignatureHeaderWithMissingValues(string header)
        {
            // Arrange
            var expectedMessage = $"The '{StripeConstants.SignatureHeaderName}' header value is invalid. It " +
                $"must contain timestamp ('{StripeConstants.TimestampKey}') and signature " +
                $"('{StripeConstants.SignatureKey}') values.";
            var filter = GetFilter(TestSecret);
            var context = GetContext(TestContent);
            context.HttpContext.Request.Headers.Add(StripeConstants.SignatureHeaderName, header);

            // Act
            await filter.OnResourceExecutionAsync(context, () => throw new InvalidOperationException("Unreachable"));

            // Assert
            var result = Assert.IsType<BadRequestObjectResult>(context.Result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(expectedMessage, result.Value);
        }

        [Fact]
        public async Task OnResourceExecutionAsync_Fails_IfPostHasInvalidSignatureEncoding()
        {
            // Arrange
            var expectedMessage = $"The '{StripeConstants.SignatureHeaderName}' header value is invalid. The " +
                "'stripe' receiver requires a valid hex-encoded string.";
            var filter = GetFilter(TestSecret);
            var context = GetContext(TestContent);
            var header = $"{StripeConstants.TimestampKey}={TestTimestamp}, " +
                $"{StripeConstants.SignatureKey}=invalid";
            context.HttpContext.Request.Headers.Add(StripeConstants.SignatureHeaderName, header);

            // Act
            await filter.OnResourceExecutionAsync(context, () => throw new InvalidOperationException("Unreachable"));

            // Assert
            var result = Assert.IsType<BadRequestObjectResult>(context.Result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(expectedMessage, result.Value);
        }

        [Fact]
        public async Task OnResourceExecutionAsync_Fails_IfPostHasIncorrectSignature()
        {
            // Arrange
            var expectedMessage = $"The signature provided by the '{StripeConstants.SignatureHeaderName}' header " +
                $"field does not match the value expected by the '{StripeConstants.ReceiverName}' WebHook receiver. " +
                "WebHook request is invalid.";
            var filter = GetFilter(TestSecret);
            var context = GetContext(TestContent);
            var header = $"{StripeConstants.TimestampKey}={TestTimestamp}, {ExtraHeaderContent}";
            context.HttpContext.Request.Headers.Add(StripeConstants.SignatureHeaderName, header);

            // Act
            await filter.OnResourceExecutionAsync(context, () => throw new InvalidOperationException("Unreachable"));

            // Assert
            var result = Assert.IsType<BadRequestObjectResult>(context.Result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal(expectedMessage, result.Value);
        }

        [Fact]
        public async Task OnResourceExecutionAsync_Succeeds_IfPostIsNotJson()
        {
            // Arrange
            var filter = GetFilter(TestSecret);
            var context = GetContext(TestContent);
            context.HttpContext.Request.ContentType = "application/x-www-form-urlencoded";
            context.HttpContext.Request.Headers.Add(StripeConstants.SignatureHeaderName, TestSignatureHeader);
            var nextCount = 0;

            // Act
            await filter.OnResourceExecutionAsync(context, () =>
            {
                nextCount++;
                var executedContext = new ResourceExecutedContext(context, context.Filters)
                {
                    Result = context.Result,
                };

                return Task.FromResult(executedContext);
            });

            // Assert
            Assert.Null(context.Result);
            Assert.Equal(1, nextCount);
        }

        [Fact]
        public async Task OnResourceExecutionAsync_Succeeds_IfTestId()
        {
            // Arrange
            var filter = GetFilter(TestSecret);
            var content = "{ \"type\": \"action\", \"id\": \"" + StripeConstants.TestNotificationId + "\" }";
            var context = GetContext(content);
            context.HttpContext.Request.Headers.Add(StripeConstants.SignatureHeaderName, GetSignatureHeader(content));
            var nextCount = 0;

            // Act
            await filter.OnResourceExecutionAsync(context, () =>
            {
                nextCount++;
                var executedContext = new ResourceExecutedContext(context, context.Filters)
                {
                    Result = context.Result,
                };

                return Task.FromResult(executedContext);
            });

            // Assert
            Assert.Null(context.Result);
            Assert.Equal(1, nextCount);
        }

        [Fact]
        public async Task OnResourceExecutionAsync_Succeeds_IfTestIdAndTestMode()
        {
            // Arrange
            var settings = new Dictionary<string, string>
            {
                { "WebHooks:Stripe:SecretKey:default", TestSecret },
                { StripeConstants.PassThroughTestEventsConfigurationKey, "true" },
            };

            var filter = GetFilter(settings);
            var content = "{ \"type\": \"action\", \"id\": \"" + StripeConstants.TestNotificationId + "\" }";
            var context = GetContext(content);
            context.HttpContext.Request.Headers.Add(StripeConstants.SignatureHeaderName, GetSignatureHeader(content));
            var nextCount = 0;

            // Act
            await filter.OnResourceExecutionAsync(context, () =>
            {
                nextCount++;
                var executedContext = new ResourceExecutedContext(context, context.Filters)
                {
                    Result = context.Result,
                };

                return Task.FromResult(executedContext);
            });

            // Assert
            Assert.Null(context.Result);
            Assert.Equal(1, nextCount);
        }

        [Theory]
        [MemberData(nameof(ValidIdData))]
        public async Task OnResourceExecutionAsync_Succeeds_IfValidPostRequest(string id)
        {
            // Arrange
            var configurationId = string.IsNullOrEmpty(id) ? "default" : id;
            var settings = new Dictionary<string, string>
            {
                { $"WebHooks:Stripe:SecretKey:{configurationId}", TestSecret },
            };

            var filter = GetFilter(settings);
            var context = GetContext(TestContent);
            context.HttpContext.Request.Headers.Add(StripeConstants.SignatureHeaderName, TestSignatureHeader);
            context.RouteData.Values.Add(WebHookConstants.IdKeyName, id);
            var nextCount = 0;

            // Act
            await filter.OnResourceExecutionAsync(context, () =>
            {
                nextCount++;
                var executedContext = new ResourceExecutedContext(context, context.Filters)
                {
                    Result = context.Result,
                };

                return Task.FromResult(executedContext);
            });

            // Assert
            Assert.Null(context.Result);
            Assert.Equal(1, nextCount);
        }

        [Fact]
        public async Task OnResourceExecutionAsync_Succeeds_IfValidPostRequest_WithExtraSignatureHeaderContent()
        {
            // Arrange
            var filter = GetFilter(TestSecret);
            var context = GetContext(TestContent);
            var header = $" {TestSignatureHeader} , {ExtraHeaderContent} ";
            context.HttpContext.Request.Headers.Add(StripeConstants.SignatureHeaderName, header);
            var nextCount = 0;

            // Act
            await filter.OnResourceExecutionAsync(context, () =>
            {
                nextCount++;
                var executedContext = new ResourceExecutedContext(context, context.Filters)
                {
                    Result = context.Result,
                };

                return Task.FromResult(executedContext);
            });

            // Assert
            Assert.Null(context.Result);
            Assert.Equal(1, nextCount);
        }

        [Fact]
        public async Task OnResourceExecutionAsync_Succeeds_IfNoReceiverName()
        {
            // Arrange
            var filter = GetFilter(TestSecret);
            var context = GetContext(TestContent);
            context.HttpContext.Request.Headers.Add(StripeConstants.SignatureHeaderName, TestSignatureHeader);
            context.RouteData.Values.Remove(WebHookConstants.ReceiverKeyName);
            var nextCount = 0;

            // Act
            await filter.OnResourceExecutionAsync(context, () =>
            {
                nextCount++;
                var executedContext = new ResourceExecutedContext(context, context.Filters)
                {
                    Result = context.Result,
                };

                return Task.FromResult(executedContext);
            });

            // Assert
            Assert.Null(context.Result);
            Assert.Equal(1, nextCount);
        }

        [Theory]
        [MemberData(nameof(InvalidReceiverData))]
        public async Task OnResourceExecutionAsync_Succeeds_IfNotApplicable(string receiverName)
        {
            // Arrange
            var filter = GetFilter(TestSecret);
            var context = GetContext(TestContent);
            context.HttpContext.Request.Headers.Add(StripeConstants.SignatureHeaderName, TestSignatureHeader);
            context.RouteData.Values[WebHookConstants.ReceiverKeyName] = receiverName;
            var nextCount = 0;

            // Act
            await filter.OnResourceExecutionAsync(context, () =>
            {
                nextCount++;
                var executedContext = new ResourceExecutedContext(context, context.Filters)
                {
                    Result = context.Result,
                };

                return Task.FromResult(executedContext);
            });

            // Assert
            Assert.Null(context.Result);
            Assert.Equal(1, nextCount);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("HEAD")]
        [InlineData("PATCH")]
        [InlineData("PUT")]
        [InlineData("OPTIONS")]
        public async Task OnResourceExecutionAsync_Fails_IfInvalidMethod(string method)
        {
            // Arrange
            var filter = GetFilter(TestSecret);
            var context = GetContext(TestContent);
            context.HttpContext.Request.Headers.Add(StripeConstants.SignatureHeaderName, TestSignatureHeader);
            context.HttpContext.Request.Method = method;
            var nextCount = 0;

            // Act
            await filter.OnResourceExecutionAsync(context, () =>
            {
                nextCount++;
                var executedContext = new ResourceExecutedContext(context, context.Filters)
                {
                    Result = context.Result,
                };

                return Task.FromResult(executedContext);
            });

            // Assert
            Assert.Null(context.Result);
            Assert.Equal(1, nextCount);
        }

        private static ResourceExecutingContext GetContext(string content)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(content));
            httpContext.Request.ContentType = "application/json";
            httpContext.Request.IsHttps = true;
            httpContext.Request.Method = HttpMethods.Post;

            var routeData = new RouteData();
            routeData.Values.Add(WebHookConstants.ReceiverKeyName, StripeConstants.ReceiverName);

            return new ResourceExecutingContext(
                new ActionContext(httpContext, routeData, new ActionDescriptor()),
                new List<IFilterMetadata>(),
                new List<IValueProviderFactory>());
        }

        private static StripeVerifySignatureFilter GetFilter(IDictionary<string, string> settings = null)
        {
            var builder = new ConfigurationBuilder();
            if (settings != null)
            {
                builder.AddInMemoryCollection(settings);
            }

            return new StripeVerifySignatureFilter(
                builder.Build(),
                Mock.Of<IHostingEnvironment>(),
                NullLoggerFactory.Instance);
        }

        private static StripeVerifySignatureFilter GetFilter(string secretKey)
        {
            var settings = new Dictionary<string, string>
            {
                { "WebHooks:Stripe:SecretKey:default", secretKey },
            };

            return GetFilter(settings);
        }

        private static string GetSignatureHeader(string content)
        {
            var secret = Encoding.UTF8.GetBytes(TestSecret);
            using (var hasher = new HMACSHA256(secret))
            {
                var fullContent = $"{TestTimestamp}.{content}";
                var data = Encoding.UTF8.GetBytes(fullContent);
                var testHash = hasher.ComputeHash(data);
                var signature = BitConverter.ToString(testHash).Replace("-", string.Empty);

                return $"  {StripeConstants.TimestampKey}={TestTimestamp},  " +
                    $"{StripeConstants.SignatureKey}={signature}  ";
            }
        }
    }
}
