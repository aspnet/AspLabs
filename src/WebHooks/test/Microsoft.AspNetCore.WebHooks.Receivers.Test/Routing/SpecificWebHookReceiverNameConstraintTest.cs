// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Moq;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    public class SpecificWebHookReceiverNameConstraintTest : WebHookConstraintTestBase
    {
        protected override string KeyName => WebHookConstants.ReceiverKeyName;

        protected override IActionConstraint GetConstraint()
        {
            var bodyTypeMetadata = new Mock<IWebHookBodyTypeMetadataService>(MockBehavior.Strict);
            bodyTypeMetadata
                .SetupGet(m => m.ReceiverName)
                .Returns("match");
            bodyTypeMetadata
                .Setup(m => m.IsApplicable(It.IsAny<string>()))
                .Returns((string value) => string.Equals("match", value, StringComparison.OrdinalIgnoreCase));

            return new WebHookReceiverNameConstraint(bodyTypeMetadata.Object);
        }
    }
}
