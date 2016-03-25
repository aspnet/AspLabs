// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Contains the top-level entry for media posted to Instagram
    /// </summary>
    public class InstagramPost
    {
        /// <summary>
        /// Gets or sets the meta data portion of an Instagram post
        /// </summary>
        [JsonProperty("meta")]
        public InstagramPostMeta Meta { get; set; }

        /// <summary>
        /// Gets or sets the data portion of an Instagram post.
        /// </summary>
        [JsonProperty("data")]
        public InstagramPostData Data { get; set; }
    }
}
