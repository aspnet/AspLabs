// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Contains information about a link in Bitbucket.
    /// </summary>
    public class BitbucketLink
    {
        /// <summary>
        /// Gets or sets the URI of the link.
        /// </summary>
        [JsonProperty("href")]
        public string Reference { get; set; }
    }
}
