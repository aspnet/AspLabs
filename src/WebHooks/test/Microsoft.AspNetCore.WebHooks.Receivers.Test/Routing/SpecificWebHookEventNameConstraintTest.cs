// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Moq;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    public class SpecificWebHookEventNameConstraintTest : WebHookEventNameConstraintTestBase
    {
        protected override IActionConstraint GetConstraint()
        {
            return new WebHookEventNameConstraint("match");
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

            return new WebHookEventNameConstraint("match", pingMetadata.Object);
        }
    }
}
