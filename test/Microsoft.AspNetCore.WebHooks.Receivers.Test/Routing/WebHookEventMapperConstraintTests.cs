// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    public class WebHookEventMapperConstraintTests : WebHookConstraintTestBase
    {
        protected override string KeyName => WebHookConstants.ReceiverKeyName;

        [Fact]
        public void Accept_Succeeds_WithConstantValue()
        {
            // Arrange
            var webHookEventMetadata = GetEventMetadata();
            webHookEventMetadata
                .SetupGet(m => m.ConstantValue)
                .Returns("constant value");

            var constraint = new WebHookEventMapperConstraint(
                new[] { webHookEventMetadata.Object },
                NullLoggerFactory.Instance);
            var context = GetContext(constraint);
            context.RouteContext.RouteData.Values.Add(KeyName, "match");

            // Act
            var result = constraint.Accept(context);

            // Assert
            ConfirmRequestValueMatch(context, result, expectedValue: "constant value");
        }

        [Fact]
        public void Accept_Fails_OnHeaderNonMatch()
        {
            // Arrange
            var webHookEventMetadata = GetEventMetadata();
            webHookEventMetadata
                .SetupGet(m => m.HeaderName)
                .Returns("another");

            var constraint = new WebHookEventMapperConstraint(
                new[] { webHookEventMetadata.Object },
                NullLoggerFactory.Instance);
            var context = GetContext(constraint);
            context.RouteContext.RouteData.Values.Add(KeyName, "match");

            // Act
            var result = constraint.Accept(context);

            // Assert (one of the few cases where constraint returns false)
            Assert.False(result);
            Assert.DoesNotContain(WebHookConstants.EventKeyName, context.RouteContext.RouteData.Values.Keys);
        }

        [Fact]
        public void Accept_Succeeds_OnQueryStringMatch()
        {
            // Arrange
            var webHookEventMetadata = GetEventMetadata();
            webHookEventMetadata
                .SetupGet(m => m.ConstantValue)
                .Returns("constant value");
            webHookEventMetadata
                .SetupGet(m => m.QueryParameterName)
                .Returns("query");

            var constraint = new WebHookEventMapperConstraint(
                new[] { webHookEventMetadata.Object },
                NullLoggerFactory.Instance);
            var context = GetContext(constraint);
            context.RouteContext.RouteData.Values.Add(KeyName, "match");

            // Act
            var result = constraint.Accept(context);

            // Assert
            // Query string match has precedence over constant value.
            ConfirmRequestValueMatch(context, result, expectedValue: "query string value");
        }

        [Fact]
        public void Accept_Fails_OnQueryStringNonMatch()
        {
            // Arrange
            var webHookEventMetadata = GetEventMetadata();
            webHookEventMetadata
                .SetupGet(m => m.QueryParameterName)
                .Returns("another");

            var constraint = new WebHookEventMapperConstraint(
                new[] { webHookEventMetadata.Object },
                NullLoggerFactory.Instance);
            var context = GetContext(constraint);
            context.RouteContext.RouteData.Values.Add(KeyName, "match");

            // Act
            var result = constraint.Accept(context);

            // Assert (one of the few cases where constraint returns false)
            Assert.False(result);
            Assert.DoesNotContain(WebHookConstants.EventKeyName, context.RouteContext.RouteData.Values.Keys);
        }

        protected override void ConfirmRequestValueMatch(ActionConstraintContext context, bool result)
        {
            // Header match has precedence over query string and constant value.
            ConfirmRequestValueMatch(context, result, expectedValue: "header value");
        }

        private void ConfirmRequestValueMatch(ActionConstraintContext context, bool result, string expectedValue)
        {
            base.ConfirmRequestValueMatch(context, result);

            var routeValuKvp = Assert.Single(context.RouteContext.RouteData.Values, kvp => string.Equals(
                WebHookConstants.EventKeyName,
                kvp.Key,
                StringComparison.OrdinalIgnoreCase));
            var routeValue = Assert.IsType<string>(routeValuKvp.Value);

            Assert.Equal(expectedValue, routeValue);
        }

        protected override void ConfirmRequestValueNonMatch(ActionConstraintContext context, bool result)
        {
            // This constraint returns true when no IWebHookEventMetadata exists for the receiver.
            Assert.True(result);

            Assert.DoesNotContain(WebHookConstants.EventKeyName, context.RouteContext.RouteData.Values.Keys);
        }

        protected override void ConfirmRequestValueNonMatch(
            ActionConstraintContext context,
            Func<ActionConstraintContext, bool> process)
        {
            // This constraint throws if request does not contain receiver name route value.
            // Arrange 2
            var expectedMessage = "Invalid WebHook constraint configuration encountered. Request contained no " +
                $"receiver name and '{typeof(WebHookReceiverExistsConstraint)}' should have disallowed the request.";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => process(context));
            Assert.Equal(expectedMessage, exception.Message);
        }

        protected override ActionConstraintContext GetContext(IActionConstraint constraint)
        {
            var context = base.GetContext(constraint);
            var request = context.RouteContext.HttpContext.Request;
            request.Headers.Add("header", "header value");
            request.QueryString = request.QueryString.Add("query", "query string value");

            return context;
        }

        protected override IActionConstraint GetConstraint()
        {
            var webHookEventMetadata = GetEventMetadata();
            webHookEventMetadata
                .SetupGet(m => m.ConstantValue)
                .Returns("constant value");
            webHookEventMetadata
                .SetupGet(m => m.HeaderName)
                .Returns("header");
            webHookEventMetadata
                .SetupGet(m => m.QueryParameterName)
                .Returns("query");

            return new WebHookEventMapperConstraint(new[] { webHookEventMetadata.Object }, NullLoggerFactory.Instance);
        }

        private Mock<IWebHookEventMetadata> GetEventMetadata()
        {
            var webHookEventMetadata = new Mock<IWebHookEventMetadata>(MockBehavior.Strict);
            webHookEventMetadata
                .SetupGet(m => m.ReceiverName)
                .Returns("match");
            webHookEventMetadata
                .Setup(m => m.IsApplicable(It.IsAny<string>()))
                .Returns((string value) => string.Equals("match", value, StringComparison.OrdinalIgnoreCase));

            // Callers all override at least one property setup.
            webHookEventMetadata
                .SetupGet(m => m.ConstantValue)
                .Returns((string)null);
            webHookEventMetadata
                .SetupGet(m => m.HeaderName)
                .Returns((string)null);
            webHookEventMetadata
                .SetupGet(m => m.QueryParameterName)
                .Returns((string)null);

            return webHookEventMetadata;
        }
    }
}
