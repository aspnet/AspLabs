// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    public class WebHookSingleEventNamesConstraintTests : WebHookEventNamesConstraintTestBase
    {
        protected override IActionConstraint GetConstraint()
        {
            return new WebHookSingleEventNamesConstraint("match", pingEventName: null);
        }

        protected override IActionConstraint GetConstraintForPingMatch()
        {
            return new WebHookSingleEventNamesConstraint("match", "pingMatch");
        }
    }
}
