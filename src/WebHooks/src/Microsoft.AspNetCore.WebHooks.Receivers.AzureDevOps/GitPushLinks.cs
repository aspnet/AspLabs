// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// A collection of links about this push.
    /// </summary>
    public class GitPushLinks
    {
        /// <summary>
        /// The link to the push.
        /// </summary>
        [JsonProperty("self")]
        public GitLink Self { get; set; }

        /// <summary>
        /// The link to the repository.
        /// </summary>
        [JsonProperty("repository")]
        public GitLink Repository { get; set; }

        /// <summary>
        /// The link to the commits.
        /// </summary>
        [JsonProperty("commits")]
        public GitLink Commits { get; set; }

        /// <summary>
        /// The link to the user pushing the code.
        /// </summary>
        [JsonProperty("pusher")]
        public GitLink Pusher { get; set; }

        /// <summary>
        /// The link to any references.
        /// </summary>
        [JsonProperty("refs")]
        public GitLink Refs { get; set; }
    }
}