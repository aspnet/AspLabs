// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    public class WebHookIdConstraintTests : WebHookConstraintTestBase
    {
        protected override string KeyName => WebHookConstants.IdKeyName;

        protected override IActionConstraint GetConstraint()
        {
            return new WebHookIdConstraint("match");
        }
    }
}
