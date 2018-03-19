// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    public class WebHookReceiverExistsConstraintTests : WebHookConstraintTestBase
    {
        protected override string KeyName => WebHookConstants.ReceiverKeyName;

        [Theory]
        [MemberData(nameof(MatchingDataSet))]
        public void Accept_Fails_OnEmptyMetadata(string requestValue)
        {
            // Accept
            var constraint = new WebHookReceiverExistsConstraint(Array.Empty<IWebHookBodyTypeMetadataService>());
            var context = GetContext(constraint);
            context.CurrentCandidate.Action.RouteValues.Add(KeyName, requestValue);

            // Act
            var result = constraint.Accept(context);

            // Assert
            ConfirmRequestValueNonMatch(context, result);
        }

        protected override void ConfirmRequestValueMatch(ActionConstraintContext context, bool result)
        {
            base.ConfirmRequestValueMatch(context, result);

            var routeValuKvp = Assert.Single(context.RouteContext.RouteData.Values, kvp => string.Equals(
                WebHookConstants.ReceiverExistsKeyName,
                kvp.Key,
                StringComparison.OrdinalIgnoreCase));
            var routeValue = Assert.IsType<bool>(routeValuKvp.Value);
            Assert.True(routeValue);
        }

        protected override void ConfirmRequestValueNonMatch(ActionConstraintContext context, bool result)
        {
            base.ConfirmRequestValueNonMatch(context, result);

            Assert.DoesNotContain(WebHookConstants.ReceiverExistsKeyName, context.RouteContext.RouteData.Values.Keys);
        }

        protected override IActionConstraint GetConstraint()
        {
            var webHookBodyTypeMetadataService = new Mock<IWebHookBodyTypeMetadataService>(MockBehavior.Strict);
            webHookBodyTypeMetadataService
                .SetupGet(m => m.ReceiverName)
                .Returns("match");
            webHookBodyTypeMetadataService
                .Setup(m => m.IsApplicable(It.IsAny<string>()))
                .Returns((string value) => string.Equals("match", value, StringComparison.OrdinalIgnoreCase));

            return new WebHookReceiverExistsConstraint(new[] { webHookBodyTypeMetadataService.Object });
        }
    }
}
