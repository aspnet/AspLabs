// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Contains information about videos contained in an Instagram post.
    /// </summary>
    public class InstagramVideos
    {
        /// <summary>
        /// Gets or sets a low bandwidth version of the media.
        /// </summary>
        [JsonProperty("low_bandwidth")]
        public InstagramMedia LowBandwidth { get; set; }

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
