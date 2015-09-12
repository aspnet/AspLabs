// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Contains information about a parent commit in Bitbucket.
    /// </summary>
    public class BitbucketParent
    {
        private readonly IDictionary<string, BitbucketLink> _links = new Dictionary<string, BitbucketLink>();

        /// <summary>
        /// Gets or sets the hash of the parent commit.
        /// </summary>
        [JsonProperty("hash")]
        public string Hash { get; set; }

        /// <summary>
        /// The type of operation on the repository, e.g. 'commit'.
        /// </summary>
        [JsonProperty("type")]
        public string Operation { get; set; }

        /// <summary>
        /// Gets the collection of <see cref="BitbucketLink"/> instances and their link relationships. The
        /// key is the link relationship and the value is the actual link.
        /// </summary>
        [JsonProperty("links")]
        public IDictionary<string, BitbucketLink> Links
        {
            get { return _links; }
        }
    }
}
