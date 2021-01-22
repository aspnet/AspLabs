// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Merge Commit Information
    /// </summary>
    public class GitMergeCommit
    {
        /// <summary>
        /// Commit Id
        /// </summary>
        [JsonProperty("commitId")]
        public string CommitId { get; set; }

        /// <summary>
        /// Commit Url
        /// </summary>
        [JsonProperty("url")]
        public Uri Url { get; set; }
    }
}