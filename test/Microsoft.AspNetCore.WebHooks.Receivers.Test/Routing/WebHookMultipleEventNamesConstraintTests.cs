// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Moq;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    public class WebHookMultipleEventNamesConstraintTests : WebHookEventNamesConstraintTestBase
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
            return new WebHookMultipleEventNamesConstraint("match", Array.Empty<IWebHookPingRequestMetadata>());
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

            return new WebHookMultipleEventNamesConstraint("match", new[] { pingMetadata.Object });
        }
    }
}
