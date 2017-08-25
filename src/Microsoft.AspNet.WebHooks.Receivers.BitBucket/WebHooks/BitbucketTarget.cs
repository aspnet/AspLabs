// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

#if NETSTANDARD2_0
namespace Microsoft.AspNetCore.WebHooks
#else
namespace Microsoft.AspNet.WebHooks
#endif
{
    /// <summary>
    /// Contains details about the most recent operation to a Bitbucket repository after a push.
    /// </summary>
    public class BitbucketTarget
    {
        private readonly Collection<BitbucketParent> _parents = new Collection<BitbucketParent>();
        private readonly IDictionary<string, BitbucketLink> _links = new Dictionary<string, BitbucketLink>();

        /// <summary>
        /// Gets or sets the email alias for this author.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

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

        /// <summary>
        /// Gets or sets the hash of the commit
        /// </summary>
        [JsonProperty("hash")]
        public string Hash { get; set; }

        /// <summary>
        /// Gets the collection of <see cref="BitbucketParent"/> instances for this target.
        /// </summary>
        [JsonProperty("parents")]
        public Collection<BitbucketParent> Parents
        {
            get { return _parents; }
        }

        /// <summary>
        /// Gets or sets the <see cref="BitbucketAuthor"/> for this target.
        /// </summary>
        [JsonProperty("author")]
        public BitbucketAuthor Author { get; set; }

        /// <summary>
        /// Gets or set the UTC time of this target.
        /// </summary>
        [JsonProperty("date")]
        public DateTime Date { get; set; }
    }
}
