﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Describes the entire payload of event '<c>git.push</c>'.
    /// </summary>
    public class GitPushPayload : BasePayload<GitPushResource>
    {
    }
}
