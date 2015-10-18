// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Contains information about images contained in an Instagram post.
    /// </summary>
    public class InstagramImages
    {
        /// <summary>
        /// Gets or sets a thumbnail of the media.
        /// </summary>
        [JsonProperty("thumbnail")]
        public InstagramMedia Thumbnail { get; set; }

        /// <summary>
        /// Gets or sets a low resolution version of the media.
        /// </summary>
        [JsonProperty("low_resolution")]
        public InstagramMedia LowResolution { get; set; }

        /// <summary>
        /// Gets or sets a standard resolution version of the media.
        /// </summary>
        [JsonProperty("standard_resolution")]
        public InstagramMedia StandardResolution { get; set; }
    }
}
