// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Contains information about a user in Bitbucket.
    /// </summary>
    public class BitbucketUser
    {
        private readonly IDictionary<string, BitbucketLink> _links = new Dictionary<string, BitbucketLink>();

        /// <summary>
        /// Gets or sets a unique ID for this user.
        /// </summary>
        [JsonProperty("uuid")]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the type of the user.
        /// </summary>
        [JsonProperty("type")]
        public string UserType { get; set; }

        /// <summary>
        /// Gets or sets the the first and last name of the user.
        /// </summary>
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the user name for this user.
        /// </summary>
        [JsonProperty("username")]
        public string UserName { get; set; }

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
