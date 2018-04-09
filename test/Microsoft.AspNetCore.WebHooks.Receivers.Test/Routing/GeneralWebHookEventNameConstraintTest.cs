// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Moq;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    public class GeneralWebHookEventNameConstraintTest : WebHookEventNameConstraintTestBase
    {
        protected override ActionConstraintContext GetContext(IActionConstraint constraint)
        {
            var context = base.GetContext(constraint);

            // Constraint throws if receiver name is not present in the request.
            context.RouteContext.RouteData.Values.Add(WebHookConstants.ReceiverKeyName, "ping");

            return context;
        }

        protected override IActionConstraint GetConstraint()
        {
            return new WebHookEventNameConstraint("match", new TestMetadataProvider(null));
        }

        protected override IActionConstraint GetConstraintForPingMatch()
        {
            var pingMetadata = new Mock<IWebHookPingRequestMetadata>(MockBehavior.Strict);
            pingMetadata
                .SetupGet(m => m.PingEventName)
                .Returns("pingMatch");
            pingMetadata
                .SetupGet(m => m.ReceiverName)
                .Returns("ping");
            pingMetadata
                .Setup(m => m.IsApplicable(It.IsAny<string>()))
                .Returns((string value) => string.Equals("ping", value, StringComparison.OrdinalIgnoreCase));

            return new WebHookEventNameConstraint("match", new TestMetadataProvider(pingMetadata.Object));
        }

        private class TestMetadataProvider : WebHookMetadataProvider
        {
            private readonly IWebHookPingRequestMetadata _pingRequestMetadata;

            public TestMetadataProvider(IWebHookPingRequestMetadata pingRequestMetadata)
            {
                _pingRequestMetadata = pingRequestMetadata;
            }

            public override IWebHookBindingMetadata GetBindingMetadata(string receiverName)
            {
                return null;
            }

            public override IWebHookBodyTypeMetadataService GetBodyTypeMetadata(string receiverName)
            {
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
                if (string.Equals("ping", receiverName, StringComparison.OrdinalIgnoreCase))
                {
                    return _pingRequestMetadata;
                }

                return null;
            }

            public override IWebHookVerifyCodeMetadata GetVerifyCodeMetadata(string receiverName)
            {
                return null;
            }
        }
    }
}
