﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Payload for the creation of a pull request
    /// </summary>
    public class GitPullRequestCreatedPayload : BasePayload<GitPullRequestResource>
    {
    }
}