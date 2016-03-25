// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Contains the meta entry for media posted to Instagram
    /// </summary>
    public class InstagramPostMeta
    {
        /// <summary>
        /// Gets or sets the HTTP status code for this Instagram post.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
