// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json;

#if NETSTANDARD2_0
namespace Microsoft.AspNetCore.WebHooks
#else
namespace Microsoft.AspNet.WebHooks
#endif
{
    /// <summary>
    /// Contains information about a Bitbucket repository.
    /// </summary>
    public class BitbucketRepository
    {
        private readonly IDictionary<string, BitbucketLink> _links = new Dictionary<string, BitbucketLink>();

        /// <summary>
        /// Gets or sets a unique ID for this repository.
        /// </summary>
        [JsonProperty("uuid")]
        public string RepositoryId { get; set; }

        /// <summary>
        /// Gets or sets the full name of the repository, e.g. '<c>someuser/myrepo</c>'.
        /// </summary>
        [JsonProperty("full_name")]
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the name of the repository, e.g. '<c>myrepo</c>'.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating with the repository is private or not.
        /// </summary>
        [JsonProperty("is_private")]
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Gets the collection of <see cref="BitbucketLink"/> instances and their link relationships. The
        /// key is the link relationship and the value is the actual link.
        /// </summary>
        [JsonProperty("links")]
        public IDictionary<string, BitbucketLink> Links
        {
            get { return _links; }
        }

        /// <summary>
        /// Gets or sets the kind of this element, e.g. '<c>repository</c>'.
        /// </summary>
        [JsonProperty("type")]
        public string ItemType { get; set; }

        /// <summary>
        /// Gets or sets the type of repository, e.g. '<c>hg</c>'.
        /// </summary>
        [JsonProperty("scm")]
        public string RepositoryType { get; set; }

        /// <summary>
        /// Gets or sets the Bitbucket user information for owner of the repository.
        /// </summary>
        public BitbucketUser Owner { get; set; }
    }
}
