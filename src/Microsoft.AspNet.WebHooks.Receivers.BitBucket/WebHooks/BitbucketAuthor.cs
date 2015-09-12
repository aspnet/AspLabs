// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Contains information about a Bitbucket author.
    /// </summary>
    public class BitbucketAuthor
    {
        /// <summary>
        /// Gets or sets the Bitbucket user information for this author.
        /// </summary>
        [JsonProperty("user")]
        public BitbucketUser User { get; set; }

        /// <summary>
        /// Gets or sets the raw author in the form of a name and email alias.
        /// </summary>
        [JsonProperty("raw")]
        public string Raw { get; set; }
    }
}
