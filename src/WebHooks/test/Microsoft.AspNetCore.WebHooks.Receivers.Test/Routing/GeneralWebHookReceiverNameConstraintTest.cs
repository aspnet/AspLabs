// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    public class GeneralWebHookReceiverNameConstraintTest : WebHookConstraintTestBase
    {
        protected override string KeyName => WebHookConstants.ReceiverKeyName;

        [Theory]
        [MemberData(nameof(MatchingDataSet))]
        public void Accept_Fails_OnEmptyMetadata(string requestValue)
        {
            // Accept
            var constraint = new WebHookReceiverNameConstraint(new TestMetadataProvider(null));
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
            var bodyTypeMetadata = new Mock<IWebHookBodyTypeMetadataService>(MockBehavior.Strict);
            bodyTypeMetadata
                .SetupGet(m => m.ReceiverName)
                .Returns("match");
            bodyTypeMetadata
                .Setup(m => m.IsApplicable(It.IsAny<string>()))
                .Returns((string value) => string.Equals("match", value, StringComparison.OrdinalIgnoreCase));

            return new WebHookReceiverNameConstraint(new TestMetadataProvider(bodyTypeMetadata.Object));
        }

        private class TestMetadataProvider : WebHookMetadataProvider
        {
            private readonly IWebHookBodyTypeMetadataService _bodyTypeMetadata;

            public TestMetadataProvider(IWebHookBodyTypeMetadataService bodyTypeMetadata)
            {
                _bodyTypeMetadata = bodyTypeMetadata;
            }

            public override IWebHookBindingMetadata GetBindingMetadata(string receiverName)
            {
                return null;
            }

            public override IWebHookBodyTypeMetadataService GetBodyTypeMetadata(string receiverName)
            {
                if (string.Equals("match", receiverName, StringComparison.OrdinalIgnoreCase))
                {
                    return _bodyTypeMetadata;
                }

                return null;
            }

            public override IWebHookEventFromBodyMetadata GetEventFromBodyMetadata(string receiverName)
            {
                return null;
            }

            public override IWebHookEventMetadata GetEventMetadata(string receiverName)
            {
                return null;
            }

            public override IWebHookFilterMetadata GetFilterMetadata(string receiverName)
            {
                return null;
            }

            public override IWebHookGetHeadRequestMetadata GetGetHeadRequestMetadata(string receiverName)
            {
                return null;
            }

            public override IWebHookPingRequestMetadata GetPingRequestMetadata(string receiverName)
            {
                return null;
            }

            public override IWebHookVerifyCodeMetadata GetVerifyCodeMetadata(string receiverName)
            {
                return null;
            }
        }
    }
}
